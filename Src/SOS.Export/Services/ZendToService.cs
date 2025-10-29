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
        private const int ChunkSizeBytes = 50 * 1024 * 1024; // 50 MB

        public ZendToService(ZendToConfiguration configuration, ILogger<ZendToService> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ZendToResponse> SendFile(
            string emailAddress,
            string description,
            string filePath,
            ExportFormat exportFormat,
            bool informRecipients = true,
            bool informPasscode = true,
            bool encryptFile = false,
            string encryptPassword = null)
        {
            double? fileSizeMb = null;
            var fileInfo = new FileInfo(filePath);
            try
            {
                fileSizeMb = fileInfo.Length / (1024.0 * 1024.0);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not read file size for {filePath}", filePath);
            }

            _logger.LogInformation(
                "{filePath} is being sent to ZendTo with email address: {emailAddress}, exportFormat: {exportFormat}, fileSizeMB: {fileSizeMb}",
                filePath, emailAddress, exportFormat, fileSizeMb.HasValue ? fileSizeMb.Value.ToString("F2") : "N/A");

            // Create chunkName for complete file
            string chunkName = Guid.NewGuid().ToString("N");

            // Upload chunks
            int totalChunks = await UploadChunksAsync(filePath, chunkName);

            // When all chunks are done, make the dropoff call
            using var form = new MultipartFormDataContent();
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
                new KeyValuePair<string, string>("desc_1", description),                
                new KeyValuePair<string, string>("chunkName", chunkName),                
                new KeyValuePair<string, string>("sentInChunks", "1"),                
            };

            foreach (var item in formData)
            {
                form.Add(new StringContent(item.Value ?? string.Empty, Encoding.Default), item.Key);
            }

            // JSON metadata about the file
            string metadata = JsonSerializer.Serialize(new
            {
                name = Path.GetFileName(filePath),
                type = "application/octet-stream",
                size = fileInfo.Length.ToString(),
                tmp_name = "1",
                error = 0
            });
            form.Add(new StringContent(metadata, Encoding.UTF8, "application/json"), "file_1");            

            string fileName = Path.GetFileName(filePath);
            form.Add(new StringContent(fileName, Encoding.UTF8), "fileName_1");

            using var client = new HttpClient();
            using var response = await client.PostAsync("https://zendto.slu.se/dropoff.php", form);
            response.Headers.TryGetValues("X-ZendTo-Response", out var responseStrings);

            if (response.IsSuccessStatusCode && responseStrings != null)
            {
                foreach (var responseString in responseStrings)
                {
                    try
                    {
                        var zendToResponse = JsonSerializer.Deserialize<ZendToResponse>(
                            responseString,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        if (!zendToResponse.Success)
                        {
                            _logger.LogError("Failed to send file with ZendTo. Response: {@responseString}. Email: {emailAddress}, ExportFormat: {exportFormat}, FileSizeMB: {fileSizeMb}", responseString, emailAddress, exportFormat, fileSizeMb);
                        }

                        _logger.LogInformation("{filePath} has successfully been sent to ZendTo with email address: {emailAddress}, exportFormat: {exportFormat}, fileSizeMB: {fileSizeMb}",
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

        private async Task<int> UploadChunksAsync(string filePath, string chunkName)
        {
            int chunkIndex = 0;
            using var fs = File.OpenRead(filePath);
            byte[] buffer = new byte[ChunkSizeBytes];

            using var client = new HttpClient();

            while (true)
            {
                int bytesRead = await fs.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead <= 0)
                    break;

                using var content = new MultipartFormDataContent
                {
                    { new StringContent(chunkName), "chunkName" },
                    { new StringContent("1"), "chunkOf" },
                    { new StringContent(_configuration.UserName), "uname" },
                    { new StringContent(_configuration.Password), "password" }
                };

                var byteContent = new ByteArrayContent(buffer, 0, bytesRead);
                byteContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");                
                content.Add(byteContent, "chunkData", "blob");

                var resp = await client.PostAsync("https://zendto.slu.se/savechunk", content);
                resp.Headers.TryGetValues("X-ZendTo-Response", out var responseStrings);
                resp.EnsureSuccessStatusCode();
                _logger.LogInformation("UploadChunkAsync succeeded. chunkName={chunkName}, chunkIndex={chunkIndex}", chunkName, chunkIndex);
                chunkIndex++;
            }

            return chunkIndex;
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