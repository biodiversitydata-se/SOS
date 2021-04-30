using System.Threading;

namespace SOS.Import.Factories.Harvest
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
