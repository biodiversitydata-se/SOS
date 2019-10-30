using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using KulService;
using SOS.Import.Repositories.Source.Kul;
using Xunit;

namespace SOS.Import.Test.Repositories
{
    public class KulRepositoryTests
    {
        [Fact]
        public async Task TestGetObservationsUsingRepository()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var kulRepository = new KulSightingRepository();
            var changedFrom = new DateTime(2015,1,1);
            var changedTo = changedFrom.AddYears(1);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await kulRepository.GetAsync(changedFrom, changedTo);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().NotBeNull();
        }
    }
}