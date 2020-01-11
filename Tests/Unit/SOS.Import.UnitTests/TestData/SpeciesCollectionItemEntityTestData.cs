using System.Collections.Generic;
using SOS.Import.Entities;

namespace SOS.Import.UnitTests.TestData
{
    public static class SpeciesCollectionItemEntityTestData
    {
        public static List<SpeciesCollectionItemEntity> CreateItems()
        {
            List<SpeciesCollectionItemEntity> entities = new List<SpeciesCollectionItemEntity>
            {
                new SpeciesCollectionItemEntity
                {
                    SightingId = 65324846,
                    Description = "Foto",
                    DeterminerText = "Håkan Ljungberg",
                    ConfirmatorText = "Stig Lundberg, Luleå",
                    DeterminerYear = 1997,
                    ConfirmatorYear = 1998
                },
                new SpeciesCollectionItemEntity
                {
                    SightingId = 65575174,
                    Description = "",
                    DeterminerText = "Paul Westrich, Kusterdingen (DE)",
                    ConfirmatorText = "L. Anders Nilsson, Uppsala",
                    DeterminerYear = 1997,
                    ConfirmatorYear = 2004
                },
                new SpeciesCollectionItemEntity
                {
                    SightingId = 66255582,
                    Description = "Foto",
                    DeterminerText = "Gunnar Isacsson",
                    ConfirmatorText = "Bengt-Åke Bengtsson",
                    DeterminerYear = 2017,
                    ConfirmatorYear = 2017
                }
            };

            return entities;
        }
    }
}
