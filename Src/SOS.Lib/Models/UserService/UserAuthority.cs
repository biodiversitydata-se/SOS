using System.Collections.Generic;

namespace SOS.Lib.Models.UserService
{
    public class UserAuthority
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<UserArea> Areas { get; set; }
        public string AuthorityIdentity { get; set; }
        public int MaxProtectionLevel { get; set; }
    }
}