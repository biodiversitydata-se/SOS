using System;

namespace SOS.Lib.Models.UserService
{
    public class PersonModel
    {
        public int Id { get; set; }
        public string GUID { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public int GenderId { get; set; }
        public string LocaleString { get; set; }
        public string EmailAddress { get; set; }
        public string PostalAddress1 { get; set; }
        public string PostalAddress2 { get; set; }
        public string ZipCode { get; set; }
        public string City { get; set; }
        public string CountryA2 { get; set; }        
        public string PhoneNumber { get; set; }
        public bool? ShowEmail { get; set; }
        public bool ShowAddresses { get; set; }
        public bool ShowPhoneNumbers { get; set; }
        public DateTime? BirthYear { get; set; }
        public DateTime? DeathYear { get; set; }
        public int? AdministrationRoleId { get; set; }
        public bool HasCollection { get; set; }
        public string URL { get; set; }
        public string Presentation { get; set; }
        public int TaxonNameTypeId { get; set; }
        public bool ShowPresentation { get; set; }
        public bool ShowPersonalInformation { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int ModifiedBy { get; set; }
    }
}