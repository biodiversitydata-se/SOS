using SOS.Process.UnitTests.TestHelpers;
using Xunit;

namespace SOS.Process.UnitTests.Processors.DarwinCoreArchive.DwcaObservationFactory
{
    [CollectionDefinition("DwcaObservationFactory collection")]
    public class EventTests : IClassFixture<DwcaObservationFactoryFixture>
    {
        private readonly DwcaObservationFactoryFixture _fixture;

        public EventTests(DwcaObservationFactoryFixture fixture)
        {
            _fixture = fixture;
        }
    }
}