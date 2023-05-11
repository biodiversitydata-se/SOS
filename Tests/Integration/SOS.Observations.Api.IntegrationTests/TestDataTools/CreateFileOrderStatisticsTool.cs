using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace SOS.Observations.Api.IntegrationTests.TestDataTools
{    
    public class CreateFileOrderStatisticsTool
    {        
        /// <summary>
        /// In order to run this test, first save the UserExport collection (MongoDB) to a JSON file.
        /// </summary>
        /// <returns></returns>
        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Create_file_order_stats()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------            
            string filePath = @"C:\temp\sos-file-order-stats.json";
            var jsonString = File.ReadAllText(filePath);
            var userExports =
                JsonSerializer.Deserialize<List<UserExportDto>>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------            
            var statistics = CreateFileOrderStatistics(userExports);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------            
            statistics.Should().NotBeNull();
        }

        private FileOrderStats CreateFileOrderStatistics(List<UserExportDto> userExports)
        {
            DateTime? firstDate = null;
            DateTime? lastDate = null;
            FileOrderStats fileOrderStats = new FileOrderStats();            
            foreach(var userExport in userExports)
            {
                if (userExport._Id == 0) continue; // AccountId used by integration tests.
                foreach(var job in userExport.Jobs)
                {
                    if (firstDate == null)
                    {
                        firstDate = lastDate = job.ProcessStartDate;
                    }
                    else
                    {
                        if (job.ProcessStartDate < firstDate) firstDate = job.ProcessStartDate;
                        if (job.ProcessStartDate > lastDate) lastDate = job.ProcessStartDate;
                    }

                    if (!fileOrderStats.StatsByFileType.ContainsKey(job.Format))
                        fileOrderStats.StatsByFileType.Add(job.Format, new FileOrderFormatStats());

                    fileOrderStats.StatsByFileType[job.Format].NrOrders++;
                    fileOrderStats.StatsByFileType[job.Format].NrObservations += job.NumberOfObservations;
                }
            }

            fileOrderStats.NrDays = (int)(lastDate - firstDate).Value.TotalDays;
            return fileOrderStats;
        }

        private class FileOrderStats
        {
            public Dictionary<string, FileOrderFormatStats> StatsByFileType { get; set; } = new Dictionary<string, FileOrderFormatStats>();
            public int NrDays { get; set; }
        }

        private class FileOrderFormatStats
        {
            public int NrOrders { get; set; }
            public int NrObservations { get; set; }

            public override string ToString()
            {
                return $"NrOrders={NrOrders}, NrObservations={NrObservations:N0}";
            }
        }

        private class UserExportDto
        {            
            /// <summary>
            /// Id
            /// </summary>
            public int _Id { get; set; }                        

            public List<ExportJobInfoDto> Jobs { get; set; }
        }

        private class ExportJobInfoDto
        {
            public string Id { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime? ProcessStartDate { get; set; }
            public DateTime? ProcessEndDate { get; set; }            
            public int NumberOfObservations { get; set; }
            public string Description { get; set; }            
            public string Format { get; set; }            
            public string Status { get; set; }
            public string ErrorMsg { get; set; }            
            public string? OutputFieldSet { get; set; }
        }
    }
}