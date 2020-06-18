using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SOS.Observations.Api.Services.Interfaces;

namespace SOS.Observations.Api.Services
{
    /// <inheritdoc />
    public class HttpClientService : IHttpClientService
    {
        private readonly ILogger<HttpClientService> _logger;

        /// <summary>
        ///     Disposed
        /// </summary>
        private bool _disposed;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="logger"></param>
        public HttpClientService(ILogger<HttpClientService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        ///     Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public async Task<T> GetDataAsync<T>(Uri requestUri)
        {
            return await GetDataAsync<T>(requestUri, null);
        }

        /// <inheritdoc />
        public async Task<T> GetDataAsync<T>(Uri requestUri, Dictionary<string, string> headerData)
        {
            var attempts = 0;
            while (attempts < 3)
            {
                var httpClient = GetClient(headerData);
                try
                {
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
                finally
                {
                    httpClient.Dispose();
                }
            }

            return default;
        }

        /// <inheritdoc />
        public async Task<Stream> GetFileStreamAsync(Uri requestUri, Dictionary<string, string> headerData = null)
        {
            var httpClient = GetClient(headerData);

            var response = await httpClient.GetAsync(requestUri);
            return response.StatusCode == HttpStatusCode.OK ? await response.Content.ReadAsStreamAsync() : null;
        }

        /// <inheritdoc />
        public async Task<T> PostDataAsync<T>(Uri requestUri, object model)
        {
            return await PostDataAsync<T>(requestUri, model, null);
        }

        /// <inheritdoc />
        public async Task<T> PostDataAsync<T>(Uri requestUri, object model, Dictionary<string, string> headerData)
        {
            var attempts = 0;
            while (attempts < 3)
            {
                var httpClient = GetClient(headerData);
                try
                {
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
                finally
                {
                    httpClient.Dispose();
                }
            }

            return default;
        }

        /// <inheritdoc />
        public async Task<T> PutDataAsync<T>(Uri requestUri, object model)
        {
            var attempts = 0;
            while (attempts < 3)
            {
                var httpClient = GetClient();
                try
                {
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
                finally
                {
                    httpClient.Dispose();
                }
            }

            return default;
        }

        /// <inheritdoc />
        public async Task<T> DeleteDataAsync<T>(Uri requestUri)
        {
            var attempts = 0;
            while (attempts < 3)
            {
                var httpClient = GetClient();
                try
                {
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
                finally
                {
                    httpClient.Dispose();
                }
            }

            return default;
        }

        private HttpClient GetClient(Dictionary<string, string> headerData = null)
        {
            var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(30)
            };

            if (headerData?.Any() ?? false)
            {
                foreach (var data in headerData)
                {
                    httpClient.DefaultRequestHeaders.Add(data.Key, data.Value);
                }
            }

            return httpClient;
        }

        /// <summary>
        ///     Dispose
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
            }

            _disposed = true;
        }
    }

    /// <summary>
    ///     Create json conetent
    /// </summary>
    public class JsonContent : StringContent
    {
        /// <summary>
        ///     Serialize object to json
        /// </summary>
        /// <param name="obj"></param>
        public JsonContent(object obj) :
            base(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json")
        {
        }
    }
}