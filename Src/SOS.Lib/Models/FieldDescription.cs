﻿using Newtonsoft.Json;
using SOS.Lib.Enums;

namespace SOS.Export.Models
{
    public class FieldDescription
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string DwcIdentifier { get; set; }
        public string Class { get; set; }
        public int Importance { get; set; }
        public bool IncludedByDefaultInDwcExport { get; set; }
        public bool IsDwC { get; set; }
        [JsonIgnore]
        public FieldDescriptionId FieldDescriptionId => (FieldDescriptionId)Id;

        protected bool Equals(FieldDescription other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((FieldDescription)obj);
        }

        public override int GetHashCode()
        {
            return Id;
        }

        public override string ToString()
        {
            return $"{Name} [Id={Id}]";
        }
    }
}