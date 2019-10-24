﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SOS.Import.Entities
{
    public class SightingRelationEntity
    {
        public int Id { get; set; }
        public int SightingId { get; set; }
        public int UserId { get; set; }
        public int SightingRelationTypeId { get; set; }
        public int Sort { get; set; }
        public bool IsPublic { get; set; }
        public DateTime RegisterDate { get; set; }
        public DateTime EditDate { get; set; }
        public int? DeterminationYear { get; set; }
    }
}
