namespace SOS.Harvest.Entities.Artportalen
{
    /// <summary>
    ///     Represents a person
    /// </summary>
    public class PersonEntity
    {
        /// <summary>
        ///     Id of person
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///     The user id.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Id in User Service
        /// </summary>
        public int? UserServiceUserId { get; set; }

        /// <summary>
        ///     First name of person
        /// </summary>
        public string? FirstName { get; set; }

        /// <summary>
        ///     Last name of person
        /// </summary>
        public string? LastName { get; set; }

        /// <summary>
        ///     The users alias
        /// </summary>
        public string? Alias { get; set; }

        public override string ToString()
        {
            return
                $"{nameof(Id)}: {Id}, {nameof(UserId)}: {UserId}, {nameof(FirstName)}: {FirstName}, {nameof(LastName)}: {LastName}, {nameof(Alias)}: {Alias}";
        }
    }
}