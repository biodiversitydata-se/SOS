namespace SOS.Harvest.Entities.Artportalen
{
    public class IdValueEntity<T>
    {
        public IdValueEntity(T id)
        {
            Id = id;
        }
        /// <summary>
        ///     Id.
        /// </summary>
        public T Id { get; set; }

        /// <summary>
        ///     Value 
        /// </summary>
        public string? Value { get; set; }
    }
}