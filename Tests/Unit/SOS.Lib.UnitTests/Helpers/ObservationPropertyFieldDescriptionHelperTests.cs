using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using SOS.Lib.Helpers;
using Xunit;

namespace SOS.Lib.UnitTests.Helpers
{
    public class ObservationPropertyFieldDescriptionHelperTests
    {
        [Fact]
        [Trait("Category", "Unit")]
        public void ValidateUniquePropertyNamesAndTitles()
        {
            // Act
            bool unique = ObservationPropertyFieldDescriptionHelper.ValidateUniquePropertyNames();

            // Assert
            unique.Should().BeTrue();
        }
    }
}
