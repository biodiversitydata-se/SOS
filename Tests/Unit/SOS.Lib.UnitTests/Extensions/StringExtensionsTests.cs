using FluentAssertions;
using SOS.Lib.Extensions;
using Xunit;

namespace SOS.Lib.UnitTests.Extensions
{
    public class StringExtensionsTests
    {
        [Fact]
        public void HasValue_indicates_whether_a_string_is_null_or_empty()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Act, Assert
            //-----------------------------------------------------------------------------------------------------------
            "".HasValue().Should().BeFalse();
            "   ".HasValue().Should().BeFalse();
            "a".HasValue().Should().BeTrue();
            ((string) null).HasValue().Should().BeFalse();
        }

        [Fact]
        public void ParseDouble_handles_both_comma_and_point_as_delimiter()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Act, Assert
            //-----------------------------------------------------------------------------------------------------------
            "3,141592".ParseDouble().Should().BeApproximately(3.141592, 0.001);
            "3.141592".ParseDouble().Should().BeApproximately(3.141592, 0.001);
            "  3,141592  ".ParseDouble(true).Should().BeApproximately(3.141592, 0.001);
            "3".ParseDouble().Should().BeApproximately(3, 0.001);
            "  3  ".ParseDouble(true).Should().BeApproximately(3, 0.001);
            "coffee".ParseDouble().HasValue.Should().BeFalse();
        }

        [Fact]
        public void TryParseDouble_handles_both_comma_and_point_as_delimiter()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Act, Assert
            //-----------------------------------------------------------------------------------------------------------
            "3,141592".TryParseDouble(out var result1).Should().BeTrue();
            "3.141592".TryParseDouble(out var result2).Should().BeTrue();
            "  3,141592  ".TryParseDouble(out var result3).Should().BeTrue();
            "coffee".TryParseDouble(out var result4).Should().BeFalse();
            "".TryParseDouble(out var result5).Should().BeFalse();
            ((string) null).TryParseDouble(out var result6).Should().BeFalse();
        }
    }
}