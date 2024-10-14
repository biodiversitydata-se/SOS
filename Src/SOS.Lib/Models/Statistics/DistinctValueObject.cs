namespace SOS.Lib.Models.Statistics
{
    public class DistinctValueObject<T>
    {
        public DistinctValueObject(T value)
        {
            Value = value;
        }

        public T Value { get; set; }
    }
}