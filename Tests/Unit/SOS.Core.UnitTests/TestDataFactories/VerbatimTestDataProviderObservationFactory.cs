using System;
using System.Collections.Generic;
using System.Text;
using SOS.Core.Models.Observations;

namespace SOS.Core.Tests.TestDataFactories
{
    public class VerbatimTestDataProviderObservationFactory
    {
        private static readonly Random Random = new Random();
        private static int _counter = 1;

        public static List<VerbatimTestDataProviderObservation> CreateRandomObservations(int numberOfObservations)
        {
            List<VerbatimTestDataProviderObservation> verbatimObservations = new List<VerbatimTestDataProviderObservation>(numberOfObservations);
            for (int i = 0; i < numberOfObservations; i++)
            {
                verbatimObservations.Add(CreateRandomObservation());
            }

            return verbatimObservations;
        }


        public static VerbatimTestDataProviderObservation CreateRandomObservation()
        {
            VerbatimTestDataProviderObservation observation = new VerbatimTestDataProviderObservation();
            observation.Id = _counter++;
            observation.ScientificName = GetRandomScientificName();
            observation.ObservedDate = new DateTime(Random.Next(1970, 2019), Random.Next(1, 13), Random.Next(1, 29)).ToUniversalTime();
            observation.ObserverName = PersonTestDataFactory.GetRandomPerson().FullName;
            observation.XCoord = Random.NextDouble() * 100;
            observation.YCoord = Random.NextDouble() * 100;

            return observation;
        }

        private static string GetRandomScientificName()
        {
            string[] values =
            {
                "Cyanistes caeruleus", // Blåmes [103025]
                "Psophus stridulus", // Trumgräshoppa [101656]
                "Haliaeetus albicilla", // Havsörn [100067]
                "Canis lupus", // Varg [100024]
                "Tussilago farfara" // Tussilago (hästhov) [220396]
            };

            return values[Random.Next(0, values.Length)];
        }
    }
}
