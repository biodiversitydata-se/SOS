using System;
using System.Collections.Generic;

namespace SOS.Lib.Models.UserService
{
   
    public class AuthorityModel
    {
        public int Id { get; set; }
        public string GUID { get; set; }
        public string Name { get; set; }
        public string Obligation { get; set; }
        public string AuthorityIdentity { get; set; }
        public bool ShowNonPublicData { get; set; }
        public int MaxProtectionLevel { get; set; }
        public bool ReadPermission { get; set; }
        public bool CreatePermission { get; set; }
        public bool UpdatePermission { get; set; }
        public bool DeletePermission { get; set; }
        public int? AdministrationRoleId { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime ValidFromDate { get; set; }
        public DateTime ValidToDate { get; set; }
        public ICollection<int> ActionIds { get; set; }
        public ICollection<AreaModel> Areas { get; set; }
        public ICollection<int> FactorIds { get; set; }
        public ICollection<int> LocalityIds { get; set; }
        public ICollection<int> ProjectIds { get; set; }
        public ICollection<int> TaxonIds { get; set; }
    }
}
