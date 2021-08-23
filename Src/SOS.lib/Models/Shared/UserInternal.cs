namespace SOS.Lib.Models.Shared
{
    public class UserInternal
    {
        /// <summary>
        ///     User Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///     User alias
        /// </summary>
        public string UserAlias { get; set; }

        /// <summary>
        /// User with sort > 0 is authorized to view the observation
        /// </summary>
        public bool? ViewAccess { get; set; }
    }
}