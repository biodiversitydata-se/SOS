using System.Collections.Generic;

namespace SOS.Lib.Models.UserService
{
    /// <summary>
    /// User role.
    /// </summary>
    public class UserRole
    {
        /// <summary>
        /// Role id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Role name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Role short name.
        /// </summary>
        public string ShortName { get; set; }

        /// <summary>
        /// Role description.
        /// </summary>
        public string Description { get; set; }
        //public ICollection<int> AuthorityIds { get; set; }

        public List<UserAuthority> Authorities { get; set; }
        public List<UserArea> Areas { get; set; }

        /// <summary>
        /// Indicates whether this role has sensitive species observation authority.
        /// </summary>
        public bool HasSensitiveSpeciesAuthority { get; set; }

        /// <summary>
        /// Indicates whether this role has sighting indication authority.
        /// </summary>
        public bool HasSightingIndicationAuthority { get; set; }
    }
}