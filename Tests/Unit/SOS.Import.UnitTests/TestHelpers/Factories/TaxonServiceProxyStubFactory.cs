using System.IO;
using Moq;
using SOS.Import.Services.Interfaces;

namespace SOS.Import.UnitTests.TestHelpers.Factories
{
    public static class TaxonServiceProxyStubFactory
    {
        public static Mock<ITaxonServiceProxy> Create(string filePath)
        {
            Stream zipStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var stub = new Mock<ITaxonServiceProxy>();
            stub.Setup(m => m.GetDwcaFileAsync(It.IsAny<string>()))
                .ReturnsAsync(zipStream);

            return stub;
        }
    }
}
