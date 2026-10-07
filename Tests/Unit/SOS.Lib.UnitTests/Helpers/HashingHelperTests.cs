using FluentAssertions;
using SOS.Lib.Helpers;
using Xunit;

namespace SOS.Lib.UnitTests.Helpers;

public class HashingHelperTests
{
    [Theory]
    [InlineData(HashingClassAlgorithms.MD5, "900150983cd24fb0d6963f7d28e17f72")]
    [InlineData(HashingClassAlgorithms.SHA512, "ddaf35a193617abacc417349ae20413112e6fa4e89a97ea20a9eeee64b55d39a2192992a274fc1a836ba3c23a3feebbd454d4423643ce80e2a9ac94fa54ca49f")]
    public void ComputeHash_ReturnsKnownDigestInRequestedCase(HashingClassAlgorithms algorithm, string expected)
    {
        HashingHelper.ComputeHashInLowerCase("abc", algorithm).Should().Be(expected);
        HashingHelper.ComputeHashInUpperCase("abc", algorithm).Should().Be(expected.ToUpperInvariant());
    }
}
