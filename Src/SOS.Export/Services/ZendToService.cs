using Microsoft.Extensions.Logging;
using SOS.Export.Models.ZendTo;
using SOS.Export.Services.Interfaces;
using SOS.Lib.Configuration.Export;
using SOS.Lib.Enums;
using SOS.Lib.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SOS.Export.Services
{
    public class ZendToService : IZendToService
    {
        private readonly ZendToConfiguration _configuration;
        private readonly ILogger<ZendToService> _logger;
        /// <summary>
        ///     Constructor
        /// </summary>
        public ZendToService(ZendToConfiguration configuration, ILogger<ZendToService> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<ZendToResponse> SendFile(string emailAddress, string description, string filePath, ExportFormat exportFormat,
            bool informRecipients = true, bool informPasscode = true, bool encryptFile = false, string encryptPassword = null)
        {
            double? fileSizeMb = null;

            try
            {
                var fileInfo = new FileInfo(filePath);
                fileSizeMb = fileInfo.Length / (1024.0 * 1024.0);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not read file size for {filePath}", filePath);
            }

            _logger.LogInformation(
                "{filePath} is being sent to ZendTo with email address: {emailAddress}, exportFormat: {exportFormat}, fileSizeMB: {fileSizeMb}",
                filePath, emailAddress, exportFormat, fileSizeMb.HasValue ? fileSizeMb.Value.ToString("F2") : "N/A");

            using var form = new MultipartFormDataContent();
            //    form.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data"); //new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            DateTime fileCreationDate = File.GetCreationTime(filePath);
            string message = GetMessage(_configuration.Message, fileCreationDate);
            if (string.IsNullOrWhiteSpace(description))
            {
                description = "Observations data file";
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
            new KeyValuePair<string, string>("informRecipients", informRecipients ? "1" : "0"),
            new KeyValuePair<string, string>("informPasscode", informPasscode ? "1" : "0"),
            new KeyValuePair<string, string>("checksumFiles", "0"),
            new KeyValuePair<string, string>("encryptFiles", encryptFile ? "1" : "0"),
            new KeyValuePair<string, string>("encryptPassword", encryptPassword),
            new KeyValuePair<string, string>("recipient_1", "1"),
            new KeyValuePair<string, string>("recipEmail_1", emailAddress),
            new KeyValuePair<string, string>("desc_1", description)
        };

            // Add form data to form
            foreach (var item in formData)
            {
                form.Add(new StringContent(item.Value ?? String.Empty, Encoding.Default), item.Key);
            }

            await using var fileStream = File.OpenRead(filePath);
            using var fileContent = new StreamContent(File.OpenRead(filePath));
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
           // string fileName = GetFilename(exportFormat, fileCreationDate);
            var fileName = filePath.Substring(filePath.LastIndexOf(@"\") + 1);
            form.Add(fileContent, "file_1", fileName);

            // Post form to ZendTo
            using var client = new HttpClient();
            using var response = await client.PostAsync("https://zendto.slu.se/dropoff.php", form);
            response.Headers.TryGetValues("X-ZendTo-Response", out var responseStrings);

            if (response.IsSuccessStatusCode && responseStrings != null)
            {
                foreach (var responseString in responseStrings)
                {
                    try
                    {
                        var zendToResponse = JsonSerializer.Deserialize<ZendToResponse>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        if (!zendToResponse.Success)
                        {
                            _logger.LogError("Failed to send file with ZendTo. Response: {@responseString}. Email: {emailAddress}, ExportFormat: {exportFormat}, FileSizeMB: {fileSizeMb}", responseString, emailAddress, exportFormat, fileSizeMb);
                        }

                        _logger.LogInformation("{filePath} has successfully being sent to ZendTo with email address: {emailAddress}, exportFormat: {exportFormat}, fileSizeMB: {fileSizeMb}",
                            filePath, emailAddress, exportFormat, fileSizeMb.HasValue ? fileSizeMb.Value.ToString("F2") : "N/A");
                        return zendToResponse;
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Failed to deserialize ZendTo response: {@responseString}", responseString);
                        continue;
                    }
                }
            }

            _logger.LogError("Failed to send file using ZendTo: {@responseReasonPhrase}, Email: {emailAddress}, ExportFormat: {exportFormat}, FileSizeMB: {fileSizeMb}", response.ReasonPhrase, emailAddress, exportFormat, fileSizeMb);
            return new ZendToResponse();
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