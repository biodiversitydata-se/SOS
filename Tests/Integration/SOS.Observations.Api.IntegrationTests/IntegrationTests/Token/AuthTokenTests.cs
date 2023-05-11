using FluentAssertions;
using System.IdentityModel.Tokens.Jwt;
using Xunit;

namespace SOS.Observations.Api.IntegrationTests.IntegrationTests.Token
{
    public class AuthTokenTests
    {
        [Fact]        
        public void Inspect_auth_token()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var accessToken = "eyJhbGciOiJSUzI1NiIsImtpZCI6IkI5QTMxQ0UxMTQzMTcwNENDQjJGOEYzODBGQURDQjhCNzZCMjU3MjEiLCJ0eXAiOiJKV1QiLCJ4NXQiOiJ1YU1jNFJReGNFekxMNDg0RDYzTGkzYXlWeUUifQ.eyJuYmYiOjE2NDI2NjU1MTksImV4cCI6MTY0Mjc1MTkxOSwiaXNzIjoiaHR0cHM6Ly9pZHMuYXJ0ZGF0YWJhbmtlbi5zZSIsImF1ZCI6Imh0dHBzOi8vaWRzLmFydGRhdGFiYW5rZW4uc2UvcmVzb3VyY2VzIiwiY2xpZW50X2lkIjoiU3ZlbnNrQm90YW5payIsInN1YiI6IjUyOSIsImF1dGhfdGltZSI6MTY0MjY2NTUxOCwiaWRwIjoibG9jYWwiLCJlbWFpbCI6Implb25hc0BnbWFpbC5jb20iLCJuYW1lIjoiSm9uYXMgUm90aCIsInJpZCI6IjIiLCJybmFtZSI6IlByaXZhdCIsInNjb3BlIjpbIm9wZW5pZCIsIm5hbWUiLCJlbWFpbCJdLCJhbXIiOlsicHdkIl19.uMz6JM6QcqliTtvuIHj68OChq5YZ-uZNVDvEForKRX3uIV8dYouLlXu8kRBuQJR1NAPLaCkjUpruwsGmqCoFCVXE7zTZoBKIbCFsrLSVxwrAEFc6fZj-jjVbiL2bLL_0pthWbWZ_Xdy74ZBxJIlNQWYCHahsuDFV6Cp8wNjj_rSfFK0YedpEYVRnWR4uEvdsIdLylhdpJmLqnTym5O7BixayPwAfdWoDt9m-Msvk44vtBvpQju9M2aO7p5I7W2E-LTac_EHImm6hnApGK12Dw4B2eONGIh8YSCNdl5oLCdGfds4qBVyzvJBTPNYViRzRn44XAfeO1fn6n2AGjS-8vA";

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var jwtToken = new JwtSecurityToken(jwtEncodedString: accessToken);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            jwtToken.Should().NotBeNull();
        }
    }
}