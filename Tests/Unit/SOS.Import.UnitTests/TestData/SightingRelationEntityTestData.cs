﻿using SOS.Harvest.Entities.Artportalen;
using SOS.Harvest.Repositories.Source.Artportalen.Enums;
using System.Collections.Generic;

namespace SOS.Import.UnitTests.TestData
{
    public static class SightingRelationEntityTestData
    {
        public static List<SightingRelationEntity> CreateItems()
        {
            var sightingRelationEntities = new List<SightingRelationEntity>
            {
                new SightingRelationEntity
                {
                    SightingId = 1,
                    UserId = 25,
                    SightingRelationTypeId = (int) SightingRelationTypeId.Determiner,
                    DeterminationYear = 2013,
                    Sort = 0
                },
                new SightingRelationEntity
                {
                    SightingId = 1,
                    UserId = 12,
                    SightingRelationTypeId = (int) SightingRelationTypeId.Confirmator,
                    DeterminationYear = null,
                    Sort = 0
                },
                new SightingRelationEntity
                {
                    SightingId = 1,
                    UserId = 23,
                    SightingRelationTypeId = (int) SightingRelationTypeId.Reporter,
                    DeterminationYear = null,
                    Sort = 0
                },
                new SightingRelationEntity
                {
                    SightingId = 2,
                    UserId = 54,
                    SightingRelationTypeId = (int) SightingRelationTypeId.Determiner,
                    DeterminationYear = null,
                    Sort = 0
                },
                new SightingRelationEntity
                {
                    SightingId = 3,
                    UserId = 12,
                    SightingRelationTypeId = (int) SightingRelationTypeId.Confirmator,
                    DeterminationYear = 2014,
                    Sort = 0
                },
                new SightingRelationEntity
                {
                    SightingId = 65324846,
                    UserId = 54,
                    SightingRelationTypeId = (int) SightingRelationTypeId.Observer,
                    DeterminationYear = 2012,
                    Sort = 0
                },
                new SightingRelationEntity
                {
                    SightingId = 65575174,
                    UserId = 54,
                    SightingRelationTypeId = (int) SightingRelationTypeId.Observer,
                    DeterminationYear = null,
                    Sort = 0
                },
                new SightingRelationEntity
                {
                    SightingId = 66255582,
                    UserId = 54,
                    SightingRelationTypeId = (int) SightingRelationTypeId.Observer,
                    DeterminationYear = null,
                    Sort = 0
                }
            };

            return sightingRelationEntities;
        }
    }
}