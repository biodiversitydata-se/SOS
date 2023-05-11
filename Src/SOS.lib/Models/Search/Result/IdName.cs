namespace SOS.Lib.Models.Search.Result
{
    /// <summary>
    ///     Generic id name object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class IdName<T>
    {
        /// <summary>
        ///     Id
        /// </summary>
        public T Id { get; set; }

        /// <summary>
        ///     Name
        /// </summary>
        public string Name { get; set; }
    }
}