﻿using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SOS.Import.Services
{
    /// <inheritdoc />
    public class HttpClientService : Interfaces.IHttpClientService
    {
        private readonly ILogger<HttpClientService> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        public HttpClientService(ILogger<HttpClientService> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<T> GetDataAsync<T>(Uri requestUri)
        {
            var attempts = 0;
            while (attempts < 3)
            {
                try
                {
                    using var httpClient = new HttpClient
                    {
                        Timeout = TimeSpan.FromMinutes(30)
                    };
                    
                    var httpResponseMessage = await httpClient.GetAsync(requestUri);
                    var jsonString = await httpResponseMessage.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<T>(jsonString);

                }
                catch (Exception ex)
                {
                    _logger.LogError($"{requestUri.PathAndQuery} \n{ex.Message}");

                    Thread.Sleep(1000);
                    attempts++;                   
                }
            }
            return default(T);
        }

        /// <inheritdoc />
        public async Task<T> PostDataAsync<T>(Uri requestUri, object model)
        {
            var attempts = 0;
            while (attempts < 3)
            {
                try
                {
                    using var httpClient = new HttpClient
                    {
                        Timeout = TimeSpan.FromMinutes(30)
                    };

                    httpClient.DefaultRequestHeaders.Add("Culture", "sv-SE");
                    var httpResponseMessage = await httpClient.PostAsync(requestUri, new JsonContent(model));
                    var jsonString = await httpResponseMessage.Content.ReadAsStringAsync();

                    return JsonConvert.DeserializeObject<T>(jsonString);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Attempt {attempts} to {requestUri.AbsolutePath} failed");

                    // If last attempt log error
                    if (attempts == 2)
                    {
                        _logger.LogError(ex.Message);
                    }

                    Thread.Sleep(1000);
                    attempts++;
                }
            }

            return default(T);
        }

        /// <inheritdoc />
        public async Task<T> PutDataAsync<T>(Uri requestUri, object model)
        {
            var attempts = 0;
            while (attempts < 3)
            {
                try
                {
                    using var httpClient = new HttpClient
                    {
                        Timeout = TimeSpan.FromMinutes(30)
                    };
                    
                    var httpResponseMessage = await httpClient.PutAsync(requestUri, new JsonContent(model));
                    var jsonString = await httpResponseMessage.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<T>(jsonString);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"{requestUri.PathAndQuery} \n{ex.Message}");

                    Thread.Sleep(1000);
                    attempts++;
                }
            }

            return default(T);
        }

        /// <inheritdoc />
        public async Task<T> DeleteDataAsync<T>(Uri requestUri)
        {
            var attempts = 0;
            while (attempts < 3)
            {
                try
                {
                    using var httpClient = new HttpClient
                    {
                        Timeout = TimeSpan.FromMinutes(30)
                    };
                    var httpResponseMessage = await httpClient.DeleteAsync(requestUri);
                    var jsonString = await httpResponseMessage.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<T>(jsonString);

                }
                catch (Exception ex)
                {
                    _logger.LogError($"{requestUri.PathAndQuery} \n{ex.Message}");

                    Thread.Sleep(1000);
                    attempts++;
                }
            }

            return default(T);
        }
    }

    /// <summary>
    /// Create json conetent
    /// </summary>
    public class JsonContent : StringContent
    {
        /// <summary>
        /// Serialize object to json
        /// </summary>
        /// <param name="obj"></param>
        public JsonContent(object obj) :
            base(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json")
        { }
    }
}
