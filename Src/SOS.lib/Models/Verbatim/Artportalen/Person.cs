namespace SOS.Lib.Models.Verbatim.Artportalen
{
    /// <summary>
    /// Represents a person
    /// </summary>
    public class Person
    {
        /// <summary>
        /// Id of person
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The user Id.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// First name of person
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Last name of person
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// User alias
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// The full name of the person.
        /// </summary>
        public string FullName => $"{FirstName} {LastName}";
    }
}
