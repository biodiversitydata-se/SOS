using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SOS.Export.Services.Interfaces;
using SOS.Lib.Configuration.Export;

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
        public async Task<bool> SendFile(string emailAddress, string note, string filePath)
        {
            using var form = new MultipartFormDataContent();
            //    form.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data"); //new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            var formData = new[]
            {
                new KeyValuePair<string, string>("Action", "dropoff"),
                new KeyValuePair<string, string>("uname", _configuration.UserName),
                new KeyValuePair<string, string>("password", _configuration.Password),
                new KeyValuePair<string, string>("senderName", _configuration.SenderName),
                new KeyValuePair<string, string>("senderEmail", _configuration.SenderEmail),
                new KeyValuePair<string, string>("senderOrganization", _configuration.SenderOrganization),
                new KeyValuePair<string, string>("subject", _configuration.EmailSubject),
                new KeyValuePair<string, string>("note", _configuration.Message),
                new KeyValuePair<string, string>("confirmDelivery", "0"),
                new KeyValuePair<string, string>("informRecipients", "1"),
                new KeyValuePair<string, string>("informPasscode", "0"),
                new KeyValuePair<string, string>("checksumFiles", "0"),
                new KeyValuePair<string, string>("encryptFiles", "0"),
                new KeyValuePair<string, string>("recipient_1", "1"),
                new KeyValuePair<string, string>("recipEmail_1", emailAddress),
                new KeyValuePair<string, string>("desc_1", note)
            };

            // Add form data to form
            foreach (var item in formData)
            {
                form.Add(new StringContent(item.Value, Encoding.Default), item.Key);
            }

            await using var fileStream = File.OpenRead(filePath);
            using var fileContent = new StreamContent(File.OpenRead(filePath));
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
            form.Add(fileContent, "file_1", Path.GetFileName(filePath));

            // Post form to ZendTo
            using var client = new HttpClient();
            using var response = await client.PostAsync("https://zendto.artdata.slu.se/dropoff.php", form);

            if (response.IsSuccessStatusCode)
            {
                var header = response.Headers.FirstOrDefault(h => h.Key == "X-ZendTo-Response");

                if (header.Key != null)
                {
                    dynamic responseData = JsonConvert.DeserializeObject(header.Value.FirstOrDefault());

                    return responseData.status.ToString() == "OK";
                }
            }

            return false;
        }
    }
}