using System;

namespace SOS.Lib.Models.UserService
{
    public class UserModel
    {
        public string UserName { get; set; }
        public int Id { get; set; }
        public int? PersonId { get; set; }
        public string UserType { get; set; }
        public bool ShowEmail { get; set; }
        public string EmailAddress { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ValidFromDate { get; set; }
        public DateTime ValidToDate { get; set; }
        public int? LocaleId { get; set; }
        public bool AccountActivated { get; set; }
    }
}
