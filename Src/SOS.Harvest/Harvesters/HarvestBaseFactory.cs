namespace SOS.Harvest.Harvesters
{
    public class HarvestBaseFactory
    {
        private int _idCounter;
        protected int NextId => Interlocked.Increment(ref _idCounter);

        protected HarvestBaseFactory()
        {
            _idCounter = 0;
        }
    }
}
