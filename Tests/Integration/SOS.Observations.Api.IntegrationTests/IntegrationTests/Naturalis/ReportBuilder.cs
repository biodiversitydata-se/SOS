using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search.Filters;
using SOS.Observations.Api.IntegrationTests.Fixtures;
using Xunit;

namespace SOS.Observations.Api.IntegrationTests.IntegrationTests.Naturalis
{
    [Collection(Collections.ApiIntegrationTestsCollection)]
    public class ReportBuilder
    {
        private readonly ApiIntegrationTestFixture _fixture;

        private async Task WriteTaxaFileAsync(IEnumerable<int> taxonIds)
        {
            if (!taxonIds?.Any() ?? true)
            {
                return;
            }

            using var taxaStream = new FileStream(@"c:\\temp\taxa.csv", FileMode.CreateNew);
            using var csvFileHelperTaxa = new CsvFileHelper();
            csvFileHelperTaxa.InitializeWrite(taxaStream, ",");
            csvFileHelperTaxa.WriteRow(new[] {
                "taxon_id_at_source",
                "taxon_full_name",
                "status_at_source",
                "accepted_taxon_id_at_source",
                "kingdom",
                "division",
                "class",
                "order",
                "family",
                "genus",
                "specific_epithet",
                "infraspecific_epithet"
            });

            var taxa = (await _fixture.TaxonRepository.GetAllAsync()).ToDictionary(t => t.Id, t => t);
            var writtenTaxonIds = new HashSet<int>();
            foreach(var taxonId in taxonIds)
            {
                WriteTaxon(csvFileHelperTaxa, taxa, taxonId, writtenTaxonIds);
            }

            await csvFileHelperTaxa.FlushAsync();
            await taxaStream.FlushAsync();
            csvFileHelperTaxa.FinishWrite();
        }

        private void WriteTaxon(CsvFileHelper csvFileHelperTaxa, IDictionary<int, Taxon> taxa, int taxonId, HashSet<int> writtenTaxonIds)
        {
            if (writtenTaxonIds.TryGetValue(taxonId, out var _))
            {
                Console.WriteLine($"Taxon already written {taxonId}");
                return;
            }

            if (!taxa.TryGetValue(taxonId, out var taxon))
            {
                Console.WriteLine($"Taxon not found {taxonId}");
                return;
            }

            var family = taxon.Family;

            // Quick fix to prevent error in file validation when family and sub family have same name
            if (new[] { 261548, 1003756, 261522, 6009465, 261509 }.Contains(taxon.Id))
            {
                family += " (sub family)";
            }

            csvFileHelperTaxa.WriteRow(new[] {
                taxon.TaxonId,
                taxon.ScientificName.Contains(',') ? $"\"{taxon.ScientificName}\"" : taxon.ScientificName,
                taxon.TaxonomicStatus,
                taxon.AcceptedNameUsageId == taxon.TaxonId ? "" : taxon.AcceptedNameUsageId,
                string.IsNullOrEmpty(taxon.Kingdom) ? "unknown" : taxon.Kingdom,
                string.IsNullOrEmpty(taxon.Phylum) ? "unknown" : taxon.Phylum,
                string.IsNullOrEmpty(taxon.Class) ? "unknown" : taxon.Class,
                string.IsNullOrEmpty(taxon.Order) ? "unknown" : taxon.Order,
                family,
                taxon.Genus,
                taxon.SpecificEpithet,
                taxon.InfraspecificEpithet
            });
            writtenTaxonIds.Add(taxonId);

            if (taxon.AcceptedNameUsageId != taxon.TaxonId)
            {
                var regex = new Regex(@"\d+$");
                var match = regex.Match(taxon.AcceptedNameUsageId);
                if (int.TryParse(match.Value, out var relatedTaxonId))
                {
                    WriteTaxon(csvFileHelperTaxa, taxa, relatedTaxonId, writtenTaxonIds);
                }
            }  
        } 

        public ReportBuilder(ApiIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task CreateReport()
        {
            var filter = new SearchFilterInternal(0);
            filter.DataProviderIds = new List<int>(){ 1 };
            filter.OnlyWithMedia = true;
            filter.Output.Fields = new[] {
                "event.plainStartDate",
                "event.plainStartTime",
                "location.decimalLongitude",
                "location.decimalLatitude",
                "occurrence.occurrenceId",
                "occurrence.lifeStage",
                "occurrence.media",
                "occurrence.sex",
                "taxon.id",
                "taxon.scientificName",
                "taxon.taxonId",
            };
            await _fixture.FilterManager.PrepareFilterAsync(0, "", filter);
            
            var taxonIds = new HashSet<int>();

            using var imagesStream = new FileStream(@"c:\\temp\images.csv", FileMode.CreateNew);
            using var csvFileHelperImages = new CsvFileHelper();
            csvFileHelperImages.InitializeWrite(imagesStream, ",");
            csvFileHelperImages.WriteRow(new[] {
                "image_url",
                "observation_id",
                "image_id",
                "taxon_id_at_source",
                "taxon_full_name",
                "sex",
                "morph",
                "morph_id",
                "location_latitude",
                "location_longitude",
                "date",
                "time"
            });

            var data = await _fixture.ProcessedObservationRepositoryTest.GetNaturalisChunkAsync(filter);

            while (data?.Records?.Any() ?? false)
            {
                _fixture.VocabularyValueResolver.ResolveVocabularyMappedValues(data.Records, "en-GB", true);
                foreach (var observation in data.Records)
                {
                    if (observation.Occurrence?.Media?.Any() ?? false)
                    {
                        taxonIds.Add(observation.Taxon.Id);

                        foreach (var media in observation.Occurrence.Media)
                        {
                            if (
                                (!media.Type?.Equals("image", StringComparison.CurrentCultureIgnoreCase) ?? true)
                                || media.Identifier.IndexOf("AwaitingImage.png", StringComparison.CurrentCultureIgnoreCase) != -1
                            ) 
                            {
                                continue;
                            }

                            csvFileHelperImages.WriteRow(new[] {
                                media.Identifier,
                                observation.Occurrence.OccurrenceId,
                                media.References,
                                observation.Taxon.TaxonId,
                                observation.Taxon.ScientificName.Contains(',') ? $"\"{observation.Taxon.ScientificName}\"" : observation.Taxon.ScientificName,
                                observation.Occurrence.Sex?.Value,
                                observation.Occurrence.LifeStage?.Value,
                                (observation.Occurrence.LifeStage?.Id ?? 0) > 0 ? $"urn:lsid:artportalen.se:lifestage:{observation.Occurrence.LifeStage?.Id}" : "",
                                observation.Location.DecimalLatitude.ToString().Replace(',', '.'),
                                observation.Location.DecimalLongitude.ToString().Replace(',', '.'),
                                observation.Event.PlainStartDate,
                                observation.Event.PlainStartTime == "00:00" ? "" : observation.Event.PlainStartTime
                            });
                        }
                    }
                }
               
                data = await _fixture.ProcessedObservationRepositoryTest.GetNaturalisChunkAsync(filter, data.PointInTimeId, data.SearchAfter);
            }

            await csvFileHelperImages.FlushAsync();
            await imagesStream.FlushAsync();
            csvFileHelperImages.FinishWrite();

            await WriteTaxaFileAsync(taxonIds);
        }
    }
}