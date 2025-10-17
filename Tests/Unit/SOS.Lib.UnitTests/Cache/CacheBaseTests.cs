using Microsoft.Extensions.Logging;
using NSubstitute;
using SOS.Lib.Cache;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SOS.Lib.UnitTests.Cache;

public class CacheBaseTests
{    
    public class TestEntity : IEntity<string>
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
    
    public class TestCacheWithShortCacheDuration : CacheBase<string, TestEntity>
    {
        public override TimeSpan CacheDuration { get; set; } = TimeSpan.FromMilliseconds(200); // kort cache-tid

        public TestCacheWithShortCacheDuration(IRepositoryBase<TestEntity, string> repo, ILogger<CacheBase<string, TestEntity>> logger)
            : base(repo, logger)
        {
        }

        // Gör OnCacheEviction publik för att kunna trigga i test
        public void ForceEviction() => typeof(CacheBase<string, TestEntity>)
            .GetMethod("OnCacheEviction", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(this, null);
    }

    public class TestCacheWithLongCacheDuration : CacheBase<string, TestEntity>
    {
        public override TimeSpan CacheDuration { get; set; } = TimeSpan.FromMinutes(10);

        public TestCacheWithLongCacheDuration(IRepositoryBase<TestEntity, string> repo, ILogger<CacheBase<string, TestEntity>> logger)
            : base(repo, logger)
        {
        }

        // Gör OnCacheEviction publik för att kunna trigga i test
        public void ForceEviction() => typeof(CacheBase<string, TestEntity>)
            .GetMethod("OnCacheEviction", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(this, null);
    }

    private readonly ILogger<CacheBase<string, TestEntity>> _logger =
        Substitute.For<ILogger<CacheBase<string, TestEntity>>>();

    [Fact]
    public async Task GetAsync_Should_Return_Entity_And_Cache_It()
    {
        // Arrange
        var repo = Substitute.For<IRepositoryBase<TestEntity, string>>();
        repo.GetAsync("1").Returns(new TestEntity { Id = "1", Name = "Alice" });
        var cache = new TestCacheWithLongCacheDuration(repo, _logger);

        // Act
        var entity1 = await cache.GetAsync("1");
        var entity2 = await cache.GetAsync("1");

        // Assert
        Assert.Equal("Alice", entity1.Name);
        await repo.Received(1).GetAsync("1"); // GetAsync should not call the repository again
        Assert.Equal(entity1, entity2);
    }

    [Fact]
    public async Task AddOrUpdateAsync_Should_Update_Cache_And_Repository()
    {
        // Arrange
        var repo = Substitute.For<IRepositoryBase<TestEntity, string>>();
        repo.AddOrUpdateAsync(Arg.Any<TestEntity>()).Returns(true);
        var cache = new TestCacheWithLongCacheDuration(repo, _logger);
        var entity = new TestEntity { Id = "2", Name = "Bob" };

        // Act
        var result = await cache.AddOrUpdateAsync(entity);

        // Assert
        var cached = await cache.GetAsync("2");
        Assert.Equal("Bob", cached.Name);
        await repo.Received(1).AddOrUpdateAsync(entity);
    }

    [Fact]
    public async Task GetAllAsync_Should_Load_All_Entities_From_Repository()
    {
        // Arrange
        var repo = Substitute.For<IRepositoryBase<TestEntity, string>>();
        repo.GetAllAsync().Returns(new List<TestEntity>
            {
                new() { Id = "A", Name = "Anna" },
                new() { Id = "B", Name = "Bertil" }
            });
        var cache = new TestCacheWithLongCacheDuration(repo, _logger);
        
        // Act
        var all = (await cache.GetAllAsync()).ToList();

        // Assert
        Assert.Equal(2, all.Count);
        Assert.Contains(all, e => e.Name == "Anna");
        await repo.Received(1).GetAllAsync();

        // Act - the second request should use the same cache
        var all2 = (await cache.GetAllAsync()).ToList();

        // Assert
        await repo.Received(1).GetAllAsync(); // No additional calls to repository
    }

    [Fact]
    public async Task Clear_Should_Remove_All_Cache_Entries()
    {
        // Arrange
        var repo = Substitute.For<IRepositoryBase<TestEntity, string>>();
        repo.GetAsync("X").Returns(new TestEntity { Id = "X", Name = "Xena" });
        var cache = new TestCacheWithLongCacheDuration(repo, _logger);
        await cache.GetAsync("X");

        // Act
        cache.Clear();

        // Assert
        await cache.GetAsync("X");
        await repo.Received(2).GetAsync("X"); // After Clear, repo should be called again
    }

    [Fact]
    public async Task Cache_Should_Reset_After_Timer_Expiration()
    {
        // Arrange
        var repo = Substitute.For<IRepositoryBase<TestEntity, string>>();
        repo.GetAllAsync().Returns(new List<TestEntity> { new() { Id = "Z", Name = "Zelda" } });
        repo.GetAsync("Z").Returns(new TestEntity { Id = "Z", Name = "Zelda" });
        var cache = new TestCacheWithShortCacheDuration(repo, _logger);
        await cache.GetAllAsync();

        // Act & Assert
        await cache.GetAllAsync(); 
        await repo.Received(1).GetAllAsync(); // Should use cache in first request

        // Act
        await Task.Delay(400); // Wait longer than cache duration
        await cache.GetAllAsync();

        // Assert
        await repo.Received(2).GetAllAsync(); // The first request after expiration should call repository again
    }

    [Fact]
    public async Task Cache_Should_Reset_After_Timer_Expiration_And_New_Value_Received_If_Repository_Value_Changed()
    {
        // Arrange
        var repo = Substitute.For<IRepositoryBase<TestEntity, string>>();       
        repo.GetAsync("ProcessedConfiguration").Returns(new TestEntity { Id = "ProcessedConfiguration", Name = "0" });
        var cache = new TestCacheWithShortCacheDuration(repo, _logger);

        // Act & Assert
        var processedConfig = await cache.GetAsync("ProcessedConfiguration");
        Assert.Equal("0", processedConfig.Name);
        processedConfig = await cache.GetAsync("ProcessedConfiguration");
        Assert.Equal("0", processedConfig.Name);

        // Act & Assert
        repo.GetAsync("ProcessedConfiguration").Returns(new TestEntity { Id = "ProcessedConfiguration", Name = "1" });
        await Task.Delay(400); // Wait longer than cache duration
        processedConfig = await cache.GetAsync("ProcessedConfiguration");
        Assert.Equal("1", processedConfig.Name);
    }

    [Fact]
    public async Task GetAsync_Should_Be_ThreadSafe()
    {
        // Arrange
        var repo = Substitute.For<IRepositoryBase<TestEntity, string>>();
        repo.GetAsync("multi").Returns(new TestEntity { Id = "multi", Name = "Concurrent" });
        var cache = new TestCacheWithLongCacheDuration(repo, _logger);
        await cache.GetAsync("multi"); // First request in order to initialize cache

        // Act
        var tasks = Enumerable.Range(0, 20)
            .Select(_ => Task.Run(() => cache.GetAsync("multi")));
        var results = await Task.WhenAll(tasks);

        // Assert
        Assert.All(results, r => Assert.Equal("Concurrent", r.Name));        
        await repo.Received(1).GetAsync("multi"); // Repository should only have been called once
    }
}