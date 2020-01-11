using System;
using System.Collections.Generic;
using System.Text;

namespace SOS.Import.Entities
{
    public class OrganizationEntity
    {
        public int Id { get; set; }
        public int OrganizationId { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}, {nameof(OrganizationId)}: {OrganizationId}, {nameof(Name)}: {Name}";
        }
    }
}