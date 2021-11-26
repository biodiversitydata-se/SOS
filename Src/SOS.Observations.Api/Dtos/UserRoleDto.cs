namespace SOS.Observations.Api.Dtos
{
    /// <summary>
    /// User role.
    /// </summary>
    public class UserRoleDto
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
    }
}
