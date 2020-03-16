using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DwC_A;
using Xunit;

namespace SOS.Import.IntegrationTests.DwcImport
{
    public class ImportDwcFileIntegrationTests
    {
        private const string archiveFileName = "./resources/psophus-stridulus-lifewatch-occurrences-dwca.zip";

        [Fact]
        public void ShouldOpenCoreFile()
        {
            using (var archive = new ArchiveReader(archiveFileName))
            {
                foreach (var row in archive.CoreFile.Rows)
                {
                    Assert.NotNull(row[0]);
                }
            }
        }

        [Fact]
        public async Task ShouldOpenCoreFileAsync()
        {
            using (var archive = new ArchiveReader(archiveFileName))
            {
                await foreach (var row in archive.GetAsyncCoreFile().GetDataRowsAsync())
                {
                    Assert.NotNull(row[0]);
                }
            }
        }
    }
}
