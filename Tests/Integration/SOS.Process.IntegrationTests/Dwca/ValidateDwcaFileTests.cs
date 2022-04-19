using FluentAssertions;
using Xunit;
using SOS.Lib.IO.DwcArchive;

namespace SOS.Process.IntegrationTests.Dwca
{
    public class ValidateDwcaFileTests
    {      
        [Fact]
        public void ValidateDwcaFile()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            string sourceFilePath = @"C:\DwC-A\_PartialRead\Artportalen-st.dwca.zip";
            string outputFolder = @"C:\Temp\";
            string outputFileName = "Artportalen";
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