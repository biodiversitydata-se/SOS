using Elastic.Clients.Elasticsearch;
using FizzWare.NBuilder;
using FizzWare.NBuilder.Implementation;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Models.Processed.Observation;
using SOS.Observations.Api.IntegrationTests.Helpers;

namespace SOS.Observations.Api.IntegrationTests.TestData.Factories;
internal static class ObservationsBuilderFactory
{
    private static Bogus.Faker _faker = new Bogus.Faker("sv");

    public static IOperable<Observation> Create(int size)
    {
        var observationsBuilder = Builder<Observation>.CreateListOfSize(size)
            .All()
                .With(m => m.Event = new Event
                {
                    EventId = DataHelper.RandomString(8),
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now,
                })
                .With(m => m.Occurrence = new Occurrence
                {
                    OccurrenceId = DataHelper.RandomString(8)
                })
                .With(m => m.DataStewardship = new Lib.Models.Processed.DataStewardship.Common.DataStewardshipInfo
                {
                    DatasetIdentifier = DataHelper.RandomString(8)
                })
                .With(m => m.DataProviderId = 1)
                .With(m => m.ArtportalenInternal = null)
                .With(m => m.Sensitive = false)
                .With(m => m.Taxon = new Taxon
                {
                    Id = _faker.Random.Int(0, 10000)
                })
                .With(m => m.Location = new Location
                {

                });

        return observationsBuilder;
    }

    extension(IOperable<Observation> operable)
    {
        public IOperable<Observation> HaveDatasetIds(IEnumerable<string> datasetIds)
        {
            var builder = ((IDeclaration<Observation>)operable).ObjectBuilder;
            var datasetsIdsList = datasetIds.ToList();
            builder.With((obs, index) =>
            {
                var datasetIdsIndex = index % datasetsIdsList.Count;
                var datasetId = datasetsIdsList[datasetIdsIndex];
                obs.DataStewardship = new Lib.Models.Processed.DataStewardship.Common.DataStewardshipInfo
                {
                    DatasetIdentifier = datasetId
                };
            });

            return operable;
        }

        public IOperable<Observation> HaveEventIds(IEnumerable<string> eventIds)
        {
            var builder = ((IDeclaration<Observation>)operable).ObjectBuilder;
            var eventIdsList = eventIds.ToList();
            builder.With((obs, index) =>
            {
                int eventIdsIndex = index % eventIdsList.Count;
                string eventId = eventIdsList[eventIdsIndex];
                obs.Event.EventId = eventId;
            });

            return operable;
        }

        public IOperable<Observation> WithDate(DateTime date)
        {
            var builder = ((IDeclaration<Observation>)operable).ObjectBuilder;
            builder.With((obs, index) =>
            {
                obs.Event.StartDate = date;
                obs.Event.EndDate = date;
            });

            return operable;
        }

        public IOperable<Observation> WithDates(DateTime startDate, DateTime endDate)
        {
            var builder = ((IDeclaration<Observation>)operable).ObjectBuilder;
            builder.With((obs, index) =>
            {
                obs.Event.StartDate = startDate;
                obs.Event.EndDate = endDate;
            });

            return operable;
        }

        public IOperable<Observation> WithPosition(double latitude, double longitude)
        {
            var builder = ((IDeclaration<Observation>)operable).ObjectBuilder;
            builder.With((obs, index) =>
            {
                obs.Location.DecimalLatitude = latitude;
                obs.Location.DecimalLongitude = longitude;
                obs.Location.PointLocation = new LatLonGeoLocation { Lat = latitude, Lon = longitude };
                obs.Location.Point = new NetTopologySuite.Geometries.Point(longitude, latitude);
            });

            return operable;
        }

        public IOperable<Observation> WithMunicipality(MunicipalityId municipalityId)
        {
            var builder = ((IDeclaration<Observation>)operable).ObjectBuilder;
            builder.With((obs, index) =>
            {
                obs.Location.Municipality = new Area { FeatureId = ((int)municipalityId).ToString() };
            });

            return operable;
        }

        public IOperable<Observation> WithCounty(string countyId)
        {
            var builder = ((IDeclaration<Observation>)operable).ObjectBuilder;
            builder.With((obs, index) =>
            {
                obs.Location.County = new Area { FeatureId = countyId };
            });

            return operable;
        }

        public IOperable<Observation> WithProvince(string provinceId)
        {
            var builder = ((IDeclaration<Observation>)operable).ObjectBuilder;
            builder.With((obs, index) =>
            {
                obs.Location.Province = new Area { FeatureId = provinceId };
            });

            return operable;
        }

        public IOperable<Observation> WithParish(string parishId)
        {
            var builder = ((IDeclaration<Observation>)operable).ObjectBuilder;
            builder.With((obs, index) =>
            {
                obs.Location.Parish = new Area { FeatureId = parishId };
            });

            return operable;
        }

        public IOperable<Observation> WithEventId(string eventId)
        {
            var builder = ((IDeclaration<Observation>)operable).ObjectBuilder;
            builder.With((obs, index) =>
            {
                obs.Event.EventId = eventId;
            });

            return operable;
        }
    }
}

