using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using SOS.Export.Services.Interfaces;
using SOS.Lib.Configuration.Export;
using SOS.Lib.Enums;
using SOS.Lib.Helpers;

namespace SOS.Export.Services
{
    public class ZendToService : IZendToService
    {
        private readonly ZendToConfiguration _configuration;

        /// <summary>
        ///     Constructor
        /// </summary>
        public ZendToService(ZendToConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <inheritdoc />
        public async Task<bool> SendFile(string emailAddress, string description, string filePath, ExportFormat exportFormat)
        {
            using var form = new MultipartFormDataContent();
            //    form.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data"); //new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            DateTime fileCreationDate = File.GetCreationTime(filePath);
            string message = GetMessage(_configuration.Message, fileCreationDate);
            if (string.IsNullOrWhiteSpace(description))
            {
                description = "DwC-A file";
            }
            
            var formData = new[]
            {
                new KeyValuePair<string, string>("Action", "dropoff"),
                new KeyValuePair<string, string>("uname", _configuration.UserName),
                new KeyValuePair<string, string>("password", _configuration.Password),
                new KeyValuePair<string, string>("senderName", _configuration.SenderName),
                new KeyValuePair<string, string>("senderEmail", _configuration.SenderEmail),
                new KeyValuePair<string, string>("senderOrganization", _configuration.SenderOrganization),
                new KeyValuePair<string, string>("subject", _configuration.EmailSubject),
                new KeyValuePair<string, string>("note", message),
                new KeyValuePair<string, string>("confirmDelivery", "0"),
                new KeyValuePair<string, string>("informRecipients", "1"),
                new KeyValuePair<string, string>("informPasscode", "1"),
                new KeyValuePair<string, string>("checksumFiles", "0"),
                new KeyValuePair<string, string>("encryptFiles", "0"),
                new KeyValuePair<string, string>("recipient_1", "1"),
                new KeyValuePair<string, string>("recipEmail_1", emailAddress),
                new KeyValuePair<string, string>("desc_1", description)
            };

            // Add form data to form
            foreach (var item in formData)
            {
                form.Add(new StringContent(item.Value, Encoding.Default), item.Key);
            }

            await using var fileStream = File.OpenRead(filePath);
            using var fileContent = new StreamContent(File.OpenRead(filePath));
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
            string fileName = GetFilename(exportFormat, fileCreationDate);
            form.Add(fileContent, "file_1", fileName);
            
            // Post form to ZendTo
            using var client = new HttpClient();
            using var response = await client.PostAsync("https://zendto.artdata.slu.se/dropoff.php", form);

            return response.IsSuccessStatusCode && (response.ReasonPhrase?.Equals("OK", StringComparison.CurrentCultureIgnoreCase) ?? false);
        }

        private string GetMessage(string template, DateTime fileCreationDate)
        {
            string str = template
                .Replace("{swedishDate}", fileCreationDate.ToString("g", new CultureInfo("sv")))
                .Replace("{englishDate}", fileCreationDate.ToString("g", new CultureInfo("en")));
            
            return str;
        }

        private string GetFilename(ExportFormat exportFormat, DateTime fileCreationDate)
        {
            return FilenameHelper.CreateFilenameWithDate($"records-{exportFormat}", "zip", fileCreationDate);
        }
    }
}