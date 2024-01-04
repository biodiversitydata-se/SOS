using FluentAssertions;
using SOS.Lib.IO.DwcArchive;
using Xunit;

namespace SOS.Process.LiveIntegrationTests.Dwca
{
    public class ValidateDwcaFileTests
    {
        [Fact]
        public void ValidateDwcaFile()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            string sourceFilePath = @"C:\DwC-A\_PartialRead\ST\sos.dwca.zip";
            string outputFolder = @"C:\Temp\";
            string outputFileName = "SOS";
            int nrRowsLimit = int.MaxValue;
            int startRow = 0;

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------            
            string filePath = DwcaFileValidator.ValidateFile(
                sourceFilePath,
                outputFolder,
                outputFileName,
                nrRowsLimit,
                startRow);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------            
            filePath.Should().NotBeEmpty();
        }
    }
}