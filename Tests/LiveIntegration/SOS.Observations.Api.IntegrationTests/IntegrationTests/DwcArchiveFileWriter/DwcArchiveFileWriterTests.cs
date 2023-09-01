using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using SOS.Lib.Enums;
using SOS.Lib.IO.DwcArchive;
using SOS.Lib.Models.Shared;
using SOS.Observations.Api.IntegrationTests.Fixtures;
using Xunit;

namespace SOS.Observations.Api.IntegrationTests.IntegrationTests.DwcArchiveFileWriter
{
    [Collection(Collections.ApiIntegrationTestsCollection)]
    public class DwcArchiveFileWriterTests
    {
        private readonly ApiIntegrationTestFixture _fixture;

        public DwcArchiveFileWriterTests(ApiIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task TestDwcArchiveFileWriter()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            const string tempFolder = @"C:\temp";
            const string occurrenceCsvFolder = @"C:\temp\2021-10-29"; // folder containing headerless occurrence.csv
            List<DwcaFilePartsInfo> fileParts = new List<DwcaFilePartsInfo>();
            string tempFilePath = Path.Combine(tempFolder, $"Temp_{Path.GetRandomFileName()}.dwca.zip");
            DwcaFilePartsInfo filePart = new DwcaFilePartsInfo();
            filePart.FilePathByBatchIdAndFilePart =
                new Dictionary<string, Dictionary<DwcaFilePart, string>> { { "1", new Dictionary<DwcaFilePart, string>() } };
            filePart.FilePathByBatchIdAndFilePart["1"].Add(DwcaFilePart.Occurrence, Path.Join(occurrenceCsvFolder, "occurrence.csv"));
            filePart.ExportFolder = occurrenceCsvFolder; 
            fileParts.Add(filePart);
            DataProvider dataProvider = new DataProvider();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            await _fixture.DwcArchiveFileWriter.CreateDwcArchiveFileAsync(dataProvider, fileParts, tempFilePath);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            File.Exists(tempFilePath).Should().BeTrue();
        }
    }
}