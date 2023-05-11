namespace SOS.Observations.Api.Dtos
{
    public class IdNameDto<T>
    {
        /// <summary>
        ///     Id
        /// </summary>
        public T Id { get; set; }

        /// <summary>
        ///     Name of area
        /// </summary>
        public string Name { get; set; }
    }
}