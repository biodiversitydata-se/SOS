//using System;
//using System.Collections.Generic;
//using System.Text;
//using CsvHelper.Configuration;
//using SOS.Core.Models.Observations;

//namespace SOS.Core.Tests.SampleCode
//{
//    public class CsvWriteRead
//    {
//        //private async Task CreateObservationIdCsvFileAsync(Stream stream, DoiSettings doiSettings)
//        //{
//        //    CsvHelper.Configuration.Configuration csvConfiguration = new CsvHelper.Configuration.Configuration
//        //    {
//        //        //QuoteAllFields = false,
//        //        Delimiter = "\t", // tab
//        //        Encoding = Encoding.UTF8
//        //    };

//        //    using (var streamWriter = new StreamWriter(stream, Encoding.UTF8))
//        //    {
//        //        using (CsvWriter writer = new CsvWriter(streamWriter, csvConfiguration))
//        //        {
//        //            await AddHeadersAsync(writer);
//        //            await AddObservationIdRowsAsync(writer, doiSettings);
//        //        }
//        //    }
//        //}

//        //private async Task AddHeadersAsync(CsvWriter writer)
//        //{
//        //    writer.WriteField("ObservationId");
//        //    writer.WriteField("DataProviderId");
//        //    writer.WriteField("CatalogNumber");
//        //    writer.WriteField("ObservationVersion");
//        //    await writer.NextRecordAsync();
//        //}


//        //private async Task AddObservationIdRowsAsync(CsvWriter writer, DoiSettings doiSettings)
//        //{
//        //    foreach (var observationVersionIdentifier in _observationRepository.GetAllObservationVersionIdentifiersEnumerable())
//        //    {
//        //        writer.WriteField(observationVersionIdentifier.Id);
//        //        writer.WriteField(observationVersionIdentifier.DataProviderId);
//        //        writer.WriteField(observationVersionIdentifier.CatalogNumber);
//        //        writer.WriteField(observationVersionIdentifier.Version);
//        //        await writer.NextRecordAsync();
//        //    }

//        //    //foreach (ITaxonRelationsTreeNode treeNode in _taxonRelationsTree.TreeNodes.Where(x => x.IncludeInDwC))
//        //    //{
//        //    //    var parentTaxon = treeNode.GetClosestParentNodeIncludedInDwC();
//        //    //    await WriteRecommendedScientificName(writer, treeNode, parentTaxon, fileSettings);
//        //    //    foreach (var name in Enumerable.Union(treeNode.Taxon.ScientificSynonyms, treeNode.Taxon.MisappliedScientificNames))
//        //    //    {
//        //    //        await WriteSynonym(writer, treeNode, parentTaxon, name, fileSettings);
//        //    //    }
//        //    //}

//        //}


//        ///// <summary>
//        ///// Creates a DOI file with all observations.
//        ///// </summary>
//        //public async Task CreateDoiCsvFileWithAllObservations(Stream stream)
//        //{
//        //DoiSettings doiSettings = new DoiSettings();
//        //await CreateObservationIdCsvFileAsync(stream, doiSettings);

//        //    return;

//        //string temporaryFileStoragePath = Path.GetTempPath();
//        //string coreCsvFilePath = Path.Combine(
//        //        temporaryFileStoragePath,
//        //        string.Format("Temp_{0}.csv", Path.GetRandomFileName()));
//        //    using (FileStream fileStream = File.Create(coreCsvFilePath))
//        //{
//        //    await CreateObservationIdCsvFileAsync(fileStream, doiSettings)
//        //        .ConfigureAwait(false);
//        //}
//        //}

//        //public async Task<IEnumerable<ObservationVersionIdentifier>> GetDoiObservationsAsync(string filename)
//        //{
//        //var messagePackFile = await GetDoiFileAsync(filename);
//        //var res = MessagePack.MessagePackSerializer.Deserialize<IEnumerable<ObservationVersionIdentifier>>(messagePackFile);
//        //    return res;

//        ////byte[] decompressed = ReadZipFile(new MemoryStream(zipFile), "doifile.csv");
//        ////CsvHelper.Configuration.Configuration csvConfiguration = new CsvHelper.Configuration.Configuration
//        ////{
//        ////    //QuoteAllFields = false,
//        ////    Delimiter = "\t", // tab
//        ////    Encoding = Encoding.UTF8,
//        ////    HeaderValidated = null,
//        ////    MissingFieldFound = null
//        ////};

//        ////using (var reader = new StreamReader(new MemoryStream(decompressed)))
//        ////{
//        ////    using (var csv = new CsvReader(reader, csvConfiguration))
//        ////    {
//        ////        csv.Configuration.RegisterClassMap<ObservationVersionIdentifierMap>();
//        ////        var records = csv.GetRecords<ObservationVersionIdentifier>().ToList();
//        ////        return records;
//        ////    }
//        ////}
//        //}

//    }

//    public sealed class ObservationVersionIdentifierMap : ClassMap<ObservationVersionIdentifier>
//    {
//        public ObservationVersionIdentifierMap()
//        {
//            Map(m => m.Id).Name("ObservationId");
//            //Map(m => m.Id).ConvertUsing(row =>
//            //{
//            //    var val = row.GetField<string>("ObservationId");
//            //    return new ObjectId(val);
//            //});
//            Map(m => m.DataProviderId).Name("DataProviderId");
//            Map(m => m.CatalogNumber).Name("CatalogNumber");
//            Map(m => m.Version).Name("ObservationVersion");
//        }
//    }

//}
