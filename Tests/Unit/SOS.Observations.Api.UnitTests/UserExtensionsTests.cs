using FluentAssertions;
using SOS.Shared.Api.Extensions.Controller;
using System.Security.Claims;
using Xunit;

namespace SOS.Observations.Api.UnitTests;

public class UserExtensionsTests
{
    [Fact]
    public void TestGetUserId()
    {
        // Arrange
        var identity = new ClaimsIdentity();            
        identity.AddClaims(new[]
        {                
            new Claim(ClaimTypes.NameIdentifier, "25")
        });
        var claimsPrincipal = new ClaimsPrincipal(identity);
        
        // Act
        int userId = UserExtensions.GetUserId(claimsPrincipal);
        
        // Assert
        userId.Should().Be(25);
    }

    [Fact]
    public void TestGetApplicationUserId()
    {
        // Arrange
        var identity = new ClaimsIdentity();
        identity.AddClaims(new[]
        {
            new Claim("client_uaid", "50")
        });

        var claimsPrincipal = new ClaimsPrincipal(identity);
        
        // Act
        int userId = UserExtensions.GetUserId(claimsPrincipal);

        // Assert
        userId.Should().Be(50);
    }
}