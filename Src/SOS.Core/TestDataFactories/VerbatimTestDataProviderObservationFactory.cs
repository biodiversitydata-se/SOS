using System;
using System.Collections.Generic;
using SOS.Core.Models.Observations;

namespace SOS.Core.TestDataFactories
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
            observation.ObserverName = GetRandomPersonName();
            observation.XCoord = GetRandomNumber(11, 20);
            observation.YCoord = GetRandomNumber(55, 65);

            return observation;
        }

        private static double GetRandomNumber(double minimum, double maximum)
        {
            return Random.NextDouble() * (maximum - minimum) + minimum;
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

        private static string GetRandomPersonName()
        {
            string[] firstNames =
            {
                "Jerry",
                "George",
                "Elaine",
                "Cosmo",
                "Jackie",
                "Susan",
                "Jacopo",
                "Mickey",
                "Kenny",
                "Matt",
                "David"
            };

            string[] lastNames =
            {
                "Seinfeld",
                "Costanza",
                "Benes",
                "Kramer",
                "Chiles",
                "Ross",
                "Peterman",
                "Steinbrenner",
                "Lippman",
                "Abbott",
                "Bania",
                "Puddy",
                "Wilhelm"
            };

            string name = string.Format("{0} {1}",
                firstNames[Random.Next(0, firstNames.Length)],
                lastNames[Random.Next(0, lastNames.Length)]);
            
            return name;
        }

    }
}
