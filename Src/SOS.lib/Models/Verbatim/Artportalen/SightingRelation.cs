using System;

namespace SOS.Lib.Models.Verbatim.Artportalen
{
    public class SightingRelation
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

        public override string ToString()
        {
            return $"{nameof(SightingId)}: {SightingId}, {nameof(UserId)}: {UserId}, {nameof(SightingRelationTypeId)}: {SightingRelationTypeId}, {nameof(Sort)}: {Sort}, {nameof(IsPublic)}: {IsPublic}, {nameof(DeterminationYear)}: {DeterminationYear}";
        }
    }
}
