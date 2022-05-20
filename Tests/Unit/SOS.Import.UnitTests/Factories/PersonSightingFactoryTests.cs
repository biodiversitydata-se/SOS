using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Harvest.Entities.Artportalen;
using SOS.Harvest.Factories;
using SOS.Harvest.Repositories.Source.Artportalen.Enums;
using SOS.Lib.Managers;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Lib.Repositories.Resource.Interfaces;
using Xunit;

namespace SOS.Import.UnitTests.Managers
{
    public class PersonSightingFactoryTests
    {                
        [Fact]
        public async Task observation_wihout_observers_returns_personSighting_with_via_prefix()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var sightingIds = new HashSet<int> { 1 };
            var organizations = new Dictionary<int, Metadata>();
            var speciesCollectionItemsBySightingId = new Dictionary<int, ICollection<SpeciesCollectionItemEntity>>();
            var sightingRelations = new List<SightingRelation>()
            {
                new SightingRelation { SightingId=1, UserId=1, SightingRelationTypeId = (int)SightingRelationTypeId.Reporter, IsPublic = true }
            };
            var personByUserId = new Dictionary<int, Person>()
            {
                { 1, new Person() { Id = 1, UserId = 1, UserServiceUserId = 1, FirstName = "Tom", LastName = "Volgers" } }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var personSightingBySightingId = PersonSightingFactory.CreatePersonSightingDictionary(sightingIds, 
                personByUserId, 
                organizations, 
                speciesCollectionItemsBySightingId, 
                sightingRelations);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            var personSighting = personSightingBySightingId[1];
            personSighting.ReportedBy.Should().Be("Tom Volgers");
            personSighting.Observers.Should().Be("Via Tom Volgers", "because there should be an observer, and in this case set it to the reporter");
            personSighting.ObserversInternal.Should().BeNull("because the RecordedByMe filter should not return any result");
            personSighting.ReportedByUserServiceUserId.Should().Be(1);
        }
    }
}