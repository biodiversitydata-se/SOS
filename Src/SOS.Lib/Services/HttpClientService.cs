﻿using Microsoft.Extensions.Logging;
using Polly;
using SOS.Lib.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace SOS.Lib.Services
{
    public static class JsonSerializationHelper
    {
        /// <summary>
        /// Serialization options
        /// </summary>
        public static JsonSerializerOptions SerializerOptions
        {
            get
            {
                var options = new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    PropertyNamingPolicy = null,
                    WriteIndented = true,
                    PropertyNameCaseInsensitive = true
                };
                options.Converters.Add(new JsonStringEnumConverter(null));

                return options;
            }
        }
    }

    /// <inheritdoc />
    public class HttpClientService : IHttpClientService
    {
        private readonly ILogger<HttpClientService> _logger;

        /// <summary>
        ///     Disposed
        /// </summary>
        private bool _disposed;

        private HttpClient GetClient(IDictionary<string, string> headerData = null, bool disableCertificateValidation = false)
        {

            var httpClient = disableCertificateValidation ?
                    new HttpClient(new HttpClientHandler
                    {
                        ClientCertificateOptions = ClientCertificateOption.Manual,
                        ServerCertificateCustomValidationCallback =
                            (httpRequestMessage, cert, cetChain, policyErrors) => true
                    }) :
                    new HttpClient();

            httpClient.Timeout = TimeSpan.FromMinutes(30);
            httpClient.DefaultRequestHeaders.Add("requesting-system", "SOS");

            if (!headerData?.Any() ?? true)
            {
                return httpClient;
            }

            var userName = headerData
                .FirstOrDefault(hd => hd.Key.Equals("username", StringComparison.CurrentCultureIgnoreCase)).Value;
            var password = headerData
                .FirstOrDefault(hd => hd.Key.Equals("password", StringComparison.CurrentCultureIgnoreCase)).Value;

            // Add basic authentication if user name and password is passed in header data
            if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    AuthenticationSchemes.Basic.ToString(),
                    Convert.ToBase64String(Encoding.UTF8.GetBytes($"{userName}:{password}"))
                );
            }

            var personalAccessToken = headerData
                .FirstOrDefault(hd => hd.Key.Equals("personalAccessToken", StringComparison.CurrentCultureIgnoreCase)).Value;
            if (!string.IsNullOrEmpty(personalAccessToken))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(
                        ASCIIEncoding.ASCII.GetBytes(
                            string.Format("{0}:{1}", "", personalAccessToken)
                        )
                    )
                );
            }

            foreach (var data in headerData.Where(
                    hd => !
                    (
                        hd.Key.Equals("username", StringComparison.CurrentCultureIgnoreCase) ||
                        hd.Key.Equals("password", StringComparison.CurrentCultureIgnoreCase))
                )
            )
            {
                httpClient.DefaultRequestHeaders.Add(data.Key, data.Value);
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
        public async Task<T> GetDataAsync<T>(
            Uri requestUri,
            IDictionary<string, string> headerData,
            TimeSpan? timeout = null,
            int? retryCount = null)
        {
            var httpClient = GetClient(headerData);
            var responsePhrase = string.Empty;

            try
            {
                HttpResponseMessage response;

                if (timeout.HasValue || retryCount.HasValue)
                {                    
                    var effectiveTimeout = timeout ?? TimeSpan.FromSeconds(30);
                    var effectiveRetryCount = retryCount ?? 0;
                    var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(effectiveTimeout);
                    var retryPolicy = Policy
                        .Handle<Exception>()
                        .WaitAndRetryAsync(effectiveRetryCount,
                            retryAttempt => TimeSpan.FromSeconds(retryAttempt), // 1s, 2s, 3s, ...
                            (exception, timeSpan, retry, context) =>
                            {
                                _logger.LogWarning(exception, "Retry {Retry} after {Delay}s for request {Uri}", retry, timeSpan.TotalSeconds, requestUri);
                            });
                    var policyWrap = retryPolicy.WrapAsync(timeoutPolicy);
                    response = await policyWrap.ExecuteAsync(async ct =>
                    {
                        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                        return await httpClient.SendAsync(request, ct);
                    }, CancellationToken.None);
                }
                else
                {
                    // No resilience
                    response = await httpClient.GetAsync(requestUri);
                }

                responsePhrase = response.ReasonPhrase;
                response.EnsureSuccessStatusCode();

                return await JsonSerializer.DeserializeAsync<T>(
                    await response.Content.ReadAsStreamAsync(),
                    JsonSerializationHelper.SerializerOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HttpClient.GetDataAsync() failed. RequestUri={@requestUri}", requestUri.AbsolutePath);
            }
            finally
            {
                httpClient.Dispose();
            }

            return default;
        }

        /// <inheritdoc />
        public async Task<Stream> GetFileStreamAsync(Uri requestUri, IDictionary<string, string> headerData = null)
        {
            var httpClient = GetClient(headerData, true);

            var response = await httpClient.GetAsync(requestUri, HttpCompletionOption.ResponseContentRead);

            return response.StatusCode == HttpStatusCode.OK ? await response.Content.ReadAsStreamAsync() : null;
        }

        /// <inheritdoc />
        public async Task<T> PostDataAsync<T>(Uri requestUri, object model)
        {
            return await PostDataAsync<T>(requestUri, model, null, "application/json");
        }

        /// <inheritdoc />
        public async Task<T> PostDataAsync<T>(Uri requestUri, object model, IDictionary<string, string> headerData)
        {
            return await PostDataAsync<T>(requestUri, model, headerData, "application/json");
        }

        /// <inheritdoc />
        public async Task<T> PostDataAsync<T>(Uri requestUri, object model, IDictionary<string, string> headerData, string contentType)
        {
            var httpClient = GetClient(headerData);
            try
            {
                var httpResponseMessage = await httpClient.PostAsync(requestUri, new JsonContent(model, contentType));
                httpResponseMessage.EnsureSuccessStatusCode();

                return await JsonSerializer.DeserializeAsync<T>(await httpResponseMessage.Content.ReadAsStreamAsync(), JsonSerializationHelper.SerializerOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Post to {@requestUri} failed", requestUri.AbsolutePath);
            }
            finally
            {
                httpClient.Dispose();
            }

            return default;
        }

        /// <inheritdoc />
        public async Task<T> PutDataAsync<T>(Uri requestUri, object model)
        {
            return await PutDataAsync<T>(requestUri, model, null, "application/json");
        }

        /// <inheritdoc />
        public async Task<T> PutDataAsync<T>(Uri requestUri, object model, IDictionary<string, string> headerData)
        {
            return await PutDataAsync<T>(requestUri, model, headerData, "application/json");
        }

        /// <inheritdoc />
        public async Task<T> PutDataAsync<T>(Uri requestUri, object model, IDictionary<string, string> headerData, string contentType)
        {
            var httpClient = GetClient(headerData);
            try
            {
                var httpResponseMessage = await httpClient.PutAsync(requestUri, new JsonContent(model, contentType));
                httpResponseMessage.EnsureSuccessStatusCode();

                return await JsonSerializer.DeserializeAsync<T>(await httpResponseMessage.Content.ReadAsStreamAsync(), JsonSerializationHelper.SerializerOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HtppClient.PutDataAsync() failed. RequestUri={@requestUri}", requestUri.AbsolutePath);
            }
            finally
            {
                httpClient.Dispose();
            }


            return default;
        }

        /// <inheritdoc />
        public async Task<T> DeleteDataAsync<T>(Uri requestUri)
        {
            var httpClient = GetClient();
            try
            {
                var httpResponseMessage = await httpClient.DeleteAsync(requestUri);
                return await JsonSerializer.DeserializeAsync<T>(await httpResponseMessage.Content.ReadAsStreamAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HtppClient.DeleteDataAsync() failed. RequestUri={@requestUri}", requestUri.AbsolutePath);
                Thread.Sleep(1000);
            }
            finally
            {
                httpClient.Dispose();
            }

            return default;
        }
    }

    /// <summary>
    ///     Create json conetent
    /// </summary>
    public class JsonContent : StringContent
    {
        /// <summary>
        ///  Serialize object to json
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="contentType"></param>
        public JsonContent(object obj, string contentType) :
            base(JsonSerializer.Serialize(obj, JsonSerializationHelper.SerializerOptions), Encoding.UTF8, contentType)
        {

        }
    }
}