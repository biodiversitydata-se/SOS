using System;
using System.Text.RegularExpressions;
using SOS.Lib.Enums;
using SOS.Lib.Models.Interfaces;

namespace SOS.Lib.Models.Shared
{
    /// <summary>
    /// Report.
    /// </summary>
    public class Report : IEntity<string>
    {
        public Report(string id)
        {
            Id = id;
            CreatedDate = DateTime.UtcNow;
        }
        
        public string Id { get; set; }
        public string Name { get; set; }
        public string CreatedBy { get; set; }
        public ReportType Type { get; set; }
        public long FileSizeInKb { get; set; }
        public string FileExtension { get; set; }
        public DateTime CreatedDate { get; set; }

        public static string CreateReportId()
        {
            return Regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()), "[/+=]", "");
        }
    }
}