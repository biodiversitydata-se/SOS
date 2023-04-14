using FluentAssertions;
using Xunit;
using SOS.Lib.IO.DwcArchive;

namespace SOS.Process.IntegrationTests.Dwca
{
    public class CreatePartialDwcaFileTests
    {      
        [Fact]
        public void Create_partial_dwca_file()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            string sourceFilePath = @"C:\DwC-A\_PartialRead\Artportalen.dwca.zip";
            string outputFolder = @"C:\Temp\";
            string outputFileName = "Artportalen";
            int nrRowsLimit = 200000;
            int startRow = 15000000;

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------            
            string filePath = PartialDwcaFileCreator.CreateDwcaFile(
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


        [Fact]
        public void Create_partial_event_dwca_file()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            string sourceFilePath = @"C:\DwC-A\_PartialRead\Riksskogstaxeringen.dwca.zip";
            string outputFolder = @"C:\Temp\";
            string outputFileName = "Riksskogstaxeringen";
            int nrRowsLimit = 100;
            int startRow = 0;

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------            
            string filePath = PartialEventDwcaFileCreator.CreateDwcaFile(
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

        [Fact]
        public void Get_distinct_values_from_dwca_file()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            string sourceFilePath = @"C:\DwC-A\Artportalen.dwca.zip";            
            int nrRowsLimit = 100;
            int startRow = 0;

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------            
            System.Collections.Generic.HashSet<string> distinctValues = PartialDwcaFileCreator.GetDistinctValuesFromDwcaFile(
                sourceFilePath,
                nrRowsLimit,
                startRow,
                "institutionCode");

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------            
            distinctValues.Should().NotBeEmpty();
        }

    }
}