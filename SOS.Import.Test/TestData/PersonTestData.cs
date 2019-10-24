using System;
using System.Collections.Generic;
using System.Text;
using SOS.Import.Models.Aggregates;
using SOS.Lib.Models.Verbatim.SpeciesPortal;

namespace SOS.Import.Test.TestData
{
    public static class PersonTestData
    {
        public static Dictionary<int, Person> CreatePersonDictionary()
        {
            var personById = new Dictionary<int, Person>
            {
                {12, new Person { UserId = 12, FirstName = "Romeo", LastName = "Olsson"} },
                {23, new Person { UserId = 23, FirstName = "Tord", LastName = "Yvel"} },
                {25, new Person { UserId = 25, FirstName = "Art", LastName = "Vandelay"} },
                {54, new Person { UserId = 54, FirstName = "Kel", LastName = "Varnsen"} },
            };

            return personById;
        }
    }
}
