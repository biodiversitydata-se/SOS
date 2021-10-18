using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using FluentAssertions;
using SOS.Lib.Cache;
using SOS.Lib.Models.Cache;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.IntegrationTests.Extensions;
using SOS.Observations.Api.IntegrationTests.Fixtures;
using Xunit;

namespace SOS.Observations.Api.IntegrationTests.IntegrationTests.ObservationsController.BatchBasicCountEndpoint
{
    [Collection(Collections.ApiIntegrationTestsCollection)]
    public class CachedCountIntegrationTests
    {
        private readonly ApiIntegrationTestFixture _fixture;

        public CachedCountIntegrationTests(ApiIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }
        
        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Cached_count_for_mammalia_taxa()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            List<int> taxonIds = new List<int>()
            {
                4000107, // Mammalia (däggdjur)
                3000303, // Carnivora (rovdjur)
                100053 // igelkott
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            
            // Get count first time. The result is not yet cached.
            var notYetCachedStopwatch = Stopwatch.StartNew();
            var notYetCachedResponse = await _fixture.ObservationsController.MultipleCachedCount(
                taxonIds,
                true);
            var notYetCachedResult = notYetCachedResponse.GetResult<IEnumerable<TaxonObservationCountDto>>();
            notYetCachedStopwatch.Stop();

            // Get count second time. The result is cached
            var cachedStopwatch = Stopwatch.StartNew();
            var cachedResponse = await _fixture.ObservationsController.MultipleCachedCount(
                taxonIds,
                true);
            var cachedResult = cachedResponse.GetResult<IEnumerable<TaxonObservationCountDto>>();
            cachedStopwatch.Stop();

            // Get count first time. The result is not yet cached.
            var notYetCachedStopwatch2 = Stopwatch.StartNew();
            var notYetCachedResponse2 = await _fixture.ObservationsController.MultipleCachedCount(
                taxonIds,
                true,
                null,
                null,
                null,
                null,
                new List<int> {1});
            var notYetCachedResult2 = notYetCachedResponse2.GetResult<IEnumerable<TaxonObservationCountDto>>();
            notYetCachedStopwatch2.Stop();

            var cachedStopwatch2 = Stopwatch.StartNew();
            var cachedResponse2 = await _fixture.ObservationsController.MultipleCachedCount(
                taxonIds,
                true,
                null,
                null,
                null,
                null,
                new List<int> { 1 });
            var cachedResult2 = cachedResponse2.GetResult<IEnumerable<TaxonObservationCountDto>>();
            cachedStopwatch2.Stop();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            notYetCachedResult.Should().NotBeNull();
            cachedResult.Should().BeEquivalentTo(notYetCachedResult);
            cachedStopwatch.ElapsedMilliseconds.Should().BeLessThan(notYetCachedStopwatch.ElapsedMilliseconds / 1000, "the cached result should be at least 1000 times faster than the calculated result.");
            cachedResult2.Should().BeEquivalentTo(notYetCachedResult2);
            cachedStopwatch2.ElapsedMilliseconds.Should().BeLessThan(notYetCachedStopwatch2.ElapsedMilliseconds / 10, "there is already a cache in Elasticsearch, but our own cache is at least 10 times faster.");
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public void Cached_count_cache_memory_test()
        {
            const int nrRequests = 100000;
            var cache = new TaxonObservationCountCache();
            double memBeforeCache = GC.GetTotalMemory(true);
            for (int i = 0; i < nrRequests; i++)
            {
                var key = new TaxonObservationCountCacheKey
                {
                    TaxonId = i
                };

                cache.Add(key, i);
                bool success = cache.TryGetCount(key, out var count);
                success.Should().BeTrue();
            }

            double memAfterCache = GC.GetTotalMemory(true);
            double bytesPerItem = (memAfterCache - memBeforeCache) / nrRequests;
            bytesPerItem.Should().BeLessThan(150, "less than 150 bytes should be used to store cache item");
        }
    }
}