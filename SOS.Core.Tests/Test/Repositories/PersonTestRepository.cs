using System;
using System.Collections.Generic;
using System.Text;
using SOS.Core.Tests.Test.Models;

namespace SOS.Core.Tests.TestRepositories
{
    public static class PersonTestRepository
    {
        private static readonly Random Random = new Random();

        private static readonly string[] FirstNames =
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

        private static readonly string[] LastNames =
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

        
        public static List<Person> GetRandomPersons(int numberOfPersons)
        {
            List<Person> persons = new List<Person>(numberOfPersons);
            for (int i = 0; i < numberOfPersons; i++)
            {
                persons.Add(GetRandomPerson());
            }

            return persons;
        }

        public static Person GetRandomPerson()
        {
            string firstName = FirstNames[Random.Next(0, FirstNames.Length)];
            string lastName = LastNames[Random.Next(0, LastNames.Length)];
            var person = new Person(firstName, lastName);
            return person;
        }

        public static List<Person> UpdatePersons(
            List<Person> persons,
            int percentToUpdate)
        {
            List<Person> updatedPersons = new List<Person>(persons.Count);
            foreach (var person in persons)
            {
                if (Random.Next(1, 101) <= percentToUpdate)
                {
                    updatedPersons.Add(GetRandomPerson());
                }
                else
                {
                    updatedPersons.Add(person);
                }
            }

            return updatedPersons;
        }

        public static void UpdatePersonsInList(
            List<Person> persons,
            int percentToUpdate)
        {
            for (var i = 0; i < persons.Count; i++)
            {
                if (Random.Next(1, 101) <= percentToUpdate)
                {
                    persons[i] = GetRandomPerson();
                }
            }
        }

        private static string[] _names =
        {
            "H.E. Pennypacker",
            "Kel Varnsen",
            "Art Vandelay",
            "Martin Van Nostrand",
            "Peter Van Nostrand",
            "Wanda Pepper",
            "Dylan Murphy",
            "Eduardo Corrochio",
            "Harry Fong",
            "Steven Snell"
        };

    }
}
