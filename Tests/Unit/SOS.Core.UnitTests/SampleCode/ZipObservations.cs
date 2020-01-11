//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.IO.Compression;
//using System.Text;
//using System.Threading.Tasks;
//using SOS.Core.Services;

//namespace SOS.Core.Tests.SampleCode
//{
//    public class ZipObservations
//    {
//        private async Task<byte[]> CreateMessagePackFileWithAllObservationIds(DoiService doiService)
//        {
//            using (MemoryStream memoryStream = new MemoryStream())
//            {
//                await doiService.CreateDoiCsvFileWithAllObservations(memoryStream);
//                var fileArray = memoryStream.ToArray();

//                using (var compressedFileStream = new MemoryStream())
//                {
//                    doiService.CreateZipFile(compressedFileStream, fileArray, "doifile.csv");
//                    var zipFileArray = compressedFileStream.ToArray();
//                    return zipFileArray;
//                }
//            }
//        }


//        private async Task<byte[]> CreateZipFileWithAllObservationIds(DoiService doiService)
//        {
//            using (MemoryStream memoryStream = new MemoryStream())
//            {
//                await doiService.CreateDoiCsvFileWithAllObservations(memoryStream);
//                var fileArray = memoryStream.ToArray();

//                using (var compressedFileStream = new MemoryStream())
//                {
//                    doiService.CreateZipFile(compressedFileStream, fileArray, "doifile.csv");
//                    var zipFileArray = compressedFileStream.ToArray();
//                    return zipFileArray;
//                }
//            }
//        }

//        public void CreateZipFile(Stream stream, byte[] fileToZip, string fileName)
//        {
//            using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Create))
//            {
//                var entry = archive.CreateEntry(fileName);
//                using (var zipEntryStream = entry.Open())
//                {
//                    zipEntryStream.Write(fileToZip, 0, fileToZip.Length);
//                }
//            }
//        }

//        public void CreateZipFile(Stream stream, Stream fileToZip, string fileName)
//        {
//            using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Create, false))
//            {
//                var entry = archive.CreateEntry(fileName);
//                using (var zipEntryStream = entry.Open())
//                {
//                    fileToZip.CopyTo(zipEntryStream);
//                }
//            }
//        }


//        public byte[] ReadZipFile(Stream stream, string filename)
//        {
//            using (var zip = new ZipArchive(stream, ZipArchiveMode.Read))
//            {
//                var entry = zip.GetEntry(filename);
//                using (var unzippedEntryStream = entry.Open())
//                {
//                    using (var ms = new MemoryStream())
//                    {
//                        unzippedEntryStream.CopyTo(ms);
//                        var unzippedArray = ms.ToArray();
//                        return unzippedArray;
//                    }
//                }
//            }

//            //using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
//            //{
//            //    decompressionStream.CopyTo(decompressedFileStream);
//            //    Console.WriteLine($"Decompressed: {fileToDecompress.Name}");
//            //}

//            //using (ZipArchive archive = ZipFile.OpenRead(zipPath))
//            //{
//            //}
//        }

//        //[Fact]
//        //public async Task TestGetAllVersionIds()
//        //{
//        //    //-----------------------------------------------------------------------------------------------------------
//        //    // Arrange
//        //    //-----------------------------------------------------------------------------------------------------------
//        //    bool writeFileToDisk = true;
//        //    string filePath = FilenameGenerator.CreateFilename(_baseFilePath + "SosDoi", "txt");
//        //    MongoDbContext dbContext = new MongoDbContext(MongoUrl, DatabaseName, ObservationsCollectionName);
//        //    MongoDbContext<DoiInfo> doiDbContext = new MongoDbContext<DoiInfo>(MongoUrl, DatabaseName, DoiCollectionName);
//        //    //await dbContext.Mongodb.DropCollectionAsync(ObservationsCollectionName);
//        //    var observationRepository = new VersionedObservationRepository<ProcessedDwcObservation>(dbContext);
//        //    var doiRepository = new DoiRepository(doiDbContext);
//        //    byte[] fileArray;
//        //    DoiService doiService = new DoiService(observationRepository, doiRepository);

//        //    //-----------------------------------------------------------------------------------------------------------
//        //    // Act
//        //    //-----------------------------------------------------------------------------------------------------------
//        //    using (MemoryStream memoryStream = new MemoryStream())
//        //    {
//        //        await doiService.CreateDoiCsvFileWithAllObservations(memoryStream);
//        //        fileArray = memoryStream.ToArray();
//        //        if (writeFileToDisk) File.WriteAllBytes(filePath, fileArray);

//        //        // Zip the file
//        //        byte[] zipFileArray;
//        //        using (var compressedFileStream = new MemoryStream())
//        //        {
//        //            doiService.CreateZipFile(compressedFileStream, fileArray, "doifile.csv");
//        //            zipFileArray = compressedFileStream.ToArray();
//        //        }
//        //        File.WriteAllBytes(
//        //            FilenameGenerator.CreateFilename(_baseFilePath + "SosDoi", "zip"),
//        //            zipFileArray);

//        //        byte[] decompressed = doiService.ReadZipFile(new MemoryStream(zipFileArray), "doifile.csv");
//        //        File.WriteAllBytes(@"c:\temp\testssfdfasfd.txt", decompressed);
//        //        DoiInfo doiInfo = DoiInfo.Create(null);

//        //        await doiService.InsertDoiFileAsync(zipFileArray, doiInfo.DoiId, doiInfo.FileName);
//        //        //await doiService.InsertDoiFileAsync(fileArray);
//        //    }



//        //    //-----------------------------------------------------------------------------------------------------------
//        //    // Assert
//        //    //-----------------------------------------------------------------------------------------------------------
//        //    fileArray.Length.Should().BeGreaterThan(0, "a file should have been generated");

//        //}

//    }
//}
