using FizzWare.NBuilder.Implementation;
using FizzWare.NBuilder;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Models.Processed.Observation;
using SOS.ContainerIntegrationTests.Helpers;
using Nest;

namespace SOS.ContainerIntegrationTests.TestData.Factories;
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

    public static IOperable<Observation> HaveDatasetIds(this IOperable<Observation> operable, IEnumerable<string> datasetIds)
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

    public static IOperable<Observation> HaveEventIds(this IOperable<Observation> operable, IEnumerable<string> eventIds)
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

    public static IOperable<Observation> WithDate(this IOperable<Observation> operable, DateTime date)
    {
        var builder = ((IDeclaration<Observation>)operable).ObjectBuilder;
        builder.With((obs, index) =>
        {
            obs.Event.StartDate = date;
            obs.Event.EndDate = date;
        });

        return operable;
    }

    public static IOperable<Observation> WithDates(this IOperable<Observation> operable, DateTime startDate, DateTime endDate)
    {
        var builder = ((IDeclaration<Observation>)operable).ObjectBuilder;
        builder.With((obs, index) =>
        {
            obs.Event.StartDate = startDate;
            obs.Event.EndDate = endDate;
        });

        return operable;
    }

    public static IOperable<Observation> WithPosition(this IOperable<Observation> operable, double latitude, double longitude)
    {
        var builder = ((IDeclaration<Observation>)operable).ObjectBuilder;
        builder.With((obs, index) =>
        {
            obs.Location.DecimalLatitude = latitude;
            obs.Location.DecimalLongitude = longitude;
            obs.Location.PointLocation = new GeoLocation(latitude, longitude);
            obs.Location.Point = new PointGeoShape(new GeoCoordinate(latitude, longitude));
        });

        return operable;
    }

    public static IOperable<Observation> WithMunicipality(this IOperable<Observation> operable, MunicipalityId municipalityId)
    {
        var builder = ((IDeclaration<Observation>)operable).ObjectBuilder;
        builder.With((obs, index) =>
        {
            obs.Location.Municipality = new Area { FeatureId = ((int)municipalityId).ToString() };
        });

        return operable;
    }

    public static IOperable<Observation> WithCounty(this IOperable<Observation> operable, string countyId)
    {
        var builder = ((IDeclaration<Observation>)operable).ObjectBuilder;
        builder.With((obs, index) =>
        {
            obs.Location.County = new Area { FeatureId = countyId };
        });

        return operable;
    }

    public static IOperable<Observation> WithProvince(this IOperable<Observation> operable, string provinceId)
    {
        var builder = ((IDeclaration<Observation>)operable).ObjectBuilder;
        builder.With((obs, index) =>
        {
            obs.Location.Province = new Area { FeatureId = provinceId };
        });

        return operable;
    }

    public static IOperable<Observation> WithParish(this IOperable<Observation> operable, string parishId)
    {
        var builder = ((IDeclaration<Observation>)operable).ObjectBuilder;
        builder.With((obs, index) =>
        {
            obs.Location.Parish = new Area { FeatureId = parishId };
        });

        return operable;
    }

    public static IOperable<Observation> WithEventId(this IOperable<Observation> operable, string eventId)
    {
        var builder = ((IDeclaration<Observation>)operable).ObjectBuilder;
        builder.With((obs, index) =>
        {
            obs.Event.EventId = eventId;
        });

        return operable;
    }
}

