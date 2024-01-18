using System.Collections.Generic;

namespace SOS.Shared.Api.Dtos
{
    /// <summary>
    /// User role authority.
    /// </summary>
    public class UserAuthorityDto
    {
        /// <summary>
        /// Authority id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Authority name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Authority areas.
        /// </summary>
        public List<UserAreaDto> Areas { get; set; }
    }
}
