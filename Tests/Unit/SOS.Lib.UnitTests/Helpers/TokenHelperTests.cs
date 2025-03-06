using FluentAssertions;
using SOS.Lib.Helpers;
using Xunit;

namespace SOS.Lib.UnitTests.Helpers
{
    public class TokenHelperTests
    {        
        [Fact]
        public void TestToken()
        {
            // Arrange
            string token = "Bearer \"eyJhbGciOiJSUzI1NiIsImtpZCI6IkI5QTMxQ0UxMTQzMTcwNENDQjJGOEYzODBGQURDQjhCNzZCMjU3MjEiLCJ0eXAiOiJKV1QiLCJ4NXQiOiJ1YU1jNFJReGNFekxMNDg0RDYzTGkzYXlWeUUifQ.eyJuYmYiOjE3NDEwOTc2MTAsImV4cCI6MTc0MTE4MjIxMCwiaXNzIjoiaHR0cHM6Ly9pZHMuYXJ0ZGF0YWJhbmtlbi5zZSIsImF1ZCI6WyJodHRwczovL2lkcy5hcnRkYXRhYmFua2VuLnNlL3Jlc291cmNlcyIsIlNPUy5PYnNlcnZhdGlvbnMiXSwiY2xpZW50X2lkIjoiU2tzQXV0aEFydHBvcnRhbFNlcnZpY2VBcHAiLCJjbGllbnRfdWFpZCI6IjgyODIiLCJzY29wZSI6WyJTT1MuT2JzZXJ2YXRpb25zLlByb3RlY3RlZCJdfQ.UdzVmzVGnhHFIISmsyyv3azzl-ctmwLcfxD2VP3PaYWPBJnR6RKO7gzkJrHxF_O3SGQcIjmdJgvvMOAMskWzCzDLlq9Jx5YsVbZ678c5nCsd18x5QwkAI8lZ8u8qVVECaLjI4r69dNXy_m6VUurBqbUD2bEUJsvV_Y4HN1ZJ0o_HarSyUVFNvC9COy9MxNe_MeIkcsId1BOCEZ-1AGmFUEI6n-DSYuQeWhuWgweREvFvSNTTjAv1yfeT0yevSRZI_PcYNARuf7Ye9anxGglgZGLhA5ngtmzbOy_rZWnTd3-6IoCARTT-83cXxAXzYjcErpIi7okYMvOwODT0I__ZRw\"";

            // Act
            bool isUserAdmin2Token = TokenHelper.IsUserAdmin2Token(token, "");

            // Assert
            isUserAdmin2Token.Should().BeFalse(because: "The token contains invalid \"");
        }
    }
}
