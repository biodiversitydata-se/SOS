using FluentAssertions;
using SOS.Lib.Enums;
using SOS.Process.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;
using SharpCompress.Common;
using static SOS.Process.IntegrationTests.Dwca.CreatePartialDwcaFileTests;
using SOS.Lib.Helpers;
using Nest;
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
    }
}