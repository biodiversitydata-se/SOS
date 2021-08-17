namespace SOS.Observations.Api.Dtos
{
    /// <summary>
    /// Id value dto
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class IdValueDto<T>
    {
        /// <summary>
        /// Id of item
        /// </summary>
        public T Id { get; set; }

        /// <summary>
        /// The value.
        /// </summary>
        public string Value { get; set; }
    }
}
