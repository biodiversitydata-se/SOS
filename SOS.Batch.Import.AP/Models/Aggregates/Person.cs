namespace SOS.Batch.Import.AP.Models.Aggregates
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
        /// First name of person
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Last name of person
        /// </summary>
        public string LastName { get; set; }
    }
}
