using System.Collections.Generic;

namespace SOS.Lib.Models.UserService
{
    /// <summary>
    /// User information.
    /// </summary>
    public class UserInformation
    {
        /// <summary>
        /// User id.
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Username.
        /// </summary>
        public string UserName { get; set; }
        
        /// <summary>
        /// First name.
        /// </summary>
        public string FirstName { get; set; }
        
        /// <summary>
        /// Last name.
        /// </summary>
        public string LastName { get; set; }
        
        /// <summary>
        /// Email.
        /// </summary>
        public string Email { get; set; }
        
        /// <summary>
        /// Indicates whether the user has any role with sensitive species observation authority.
        /// </summary>
        public bool HasSensitiveSpeciesAuthority { get; set; }
        
        /// <summary>
        /// Indicates whether the use user has any role with sighting indication authority.
        /// </summary>
        public bool HasSightingIndicationAuthority { get; set; }
        
        /// <summary>
        /// User roles.
        /// </summary>
        public ICollection<UserRole> Roles { get; set; }
        //public ICollection<UserAuthority> Authorities { get; set; }        

        //public class UserAuthority
        //{
        //    public int Id { get; set; }
        //    public string Name { get; set; }
        //    public string AuthorityIdentity { get; set; }
        //}            
    }
}