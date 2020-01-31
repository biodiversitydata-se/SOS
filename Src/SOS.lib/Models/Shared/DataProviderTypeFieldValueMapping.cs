namespace SOS.Lib.Models.Shared
{
    public class DataProviderTypeFieldValueMapping
    {
        /// <summary>
        /// Value in data provider.
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Id in SOS (Species Observation System).
        /// </summary>
        public int SosId { get; set; }

        public override string ToString()
        {
            return $"{nameof(Value)}: {Value}, {nameof(SosId)}: {SosId}";
        }
    }
}