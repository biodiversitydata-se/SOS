using SOS.Lib.Models.Processed.Observation;

namespace SOS.Lib.UnitTests.Gis;

internal static class LocationClustererTestData
{
    public static List<Observation> Make361ObservationsFarApart()
    {
        var observations = new List<Observation>();
        for (var i = 1; i <= 19; i++)
        {
            for (var j = 1; j <= 19; j++)
            {
                observations.Add(CreateObservation(new Location
                {
                    DecimalLongitude = 10.0 + i * 0.5,
                    DecimalLatitude = 55.0 + j * 0.5
                }));
            }
        }

        return observations;
    }

    public static List<Observation> Make16ObservationsCloseToEachOtherAndOneRemote()
    {
        var observations = new List<Observation>();
        var locationIndex = 0;
        for (var i = 1; i <= 4; i++)
        {
            for (var j = 1; j <= 4; j++)
            {
                var location = new Location
                {
                    LocationId = $"close-location-{locationIndex++}",
                    DecimalLongitude = 18.0 + i * 0.0001,
                    DecimalLatitude = 59.0 + j * 0.0001
                };

                var closeLocationIndex = locationIndex - 1;
                observations.Add(CreateObservation(
                    location,
                    $"close-observation-{closeLocationIndex}",
                    closeLocationIndex % 3 + 1,
                    closeLocationIndex % 2 == 0,
                    closeLocationIndex % 3 == 0));
            }
        }

        observations.Add(CreateObservation(
            new Location
            {
                LocationId = "remote-location-0",
                DecimalLongitude = 30.0,
                DecimalLatitude = 70.0
            },
            "remote-observation-0",
            99,
            true,
            false));

        return observations;
    }

    public static List<Observation> MakeRandomObservations(int count)
    {
        var random = new Random(12345);
        var observations = new List<Observation>(count);
        var locationIndex = 0;

        while (observations.Count < count)
        {
            var location = new Location
            {
                DecimalLongitude = 10.0 + random.NextDouble() * 20.0,
                DecimalLatitude = 55.0 + random.NextDouble() * 10.0,
                LocationId = $"location-{locationIndex++}"
            };

            var observationsToCreate = Math.Min(2, count - observations.Count);
            observations.AddRange(CreateObservations(location, observationsToCreate));
        }

        return observations;
    }

    public static Observation CreateObservation(
        Location location,
        string? observationId = null,
        int taxonId = 1,
        bool isPositiveObservation = true,
        bool isGeneralized = false)
    {
        return new Observation
        {
            Id = observationId ?? Guid.NewGuid().ToString(),
            IsGeneralized = isGeneralized,
            Location = location,
            Occurrence = new Occurrence
            {
                OccurrenceId = observationId ?? Guid.NewGuid().ToString(),
                IsPositiveObservation = isPositiveObservation
            },
            Taxon = new Taxon { Id = taxonId }
        };
    }

    public static IEnumerable<Observation> CreateObservations(Location location, int count)
    {
        return Enumerable.Range(0, count)
            .Select(_ => CreateObservation(location));
    }
}
