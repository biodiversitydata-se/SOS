﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SOS.ElasticSearch.Proxy.Configuration;
using SOS.ElasticSearch.Proxy.Extensions;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Repositories.Processed.Interfaces;
using System.Diagnostics;
using System.Dynamic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace SOS.ElasticSearch.Proxy.Middleware
{
    public class RequestMiddleware
    {
        private readonly RequestDelegate _nextMiddleware;
        private readonly IProcessedObservationCoreRepository _processedObservationRepository;
        private readonly IDataProviderCache _dataProviderCache;
        private readonly ProxyConfiguration _proxyConfiguration;
        private readonly ILogger<RequestMiddleware> _logger;
        private static readonly SemaphoreSlim _requestSemaphore = new SemaphoreSlim(4);

        /// <summary>
        /// Build target uri
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private Uri? BuildTargetUri(HttpRequest request)
        {
            var uriParts = new HashSet<string>
            {
                _processedObservationRepository.PublicIndexName
            };

            if (request.Path.HasValue)
            {
                uriParts.Add(string.Join('/',
                    request.Path.Value.Split('/', StringSplitOptions.RemoveEmptyEntries).Skip(1)));
            }

            if (request.QueryString.HasValue)
            {
                uriParts.Add(request.QueryString.Value ?? string.Empty);
            }

            return new Uri(_processedObservationRepository.HostUrl, string.Join('/', uriParts.Where(p => !string.IsNullOrEmpty(p))));
        }

        private async Task<(HttpRequestMessage Message, string Body)> CreateTargetMessageAsync(HttpContext context, Uri targetUri, string body)
        {
            var requestMessage = new HttpRequestMessage();
            foreach (var header in context.Request.Headers)
            {
                requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }
            requestMessage.Headers.Host = targetUri.Host;
            requestMessage.RequestUri = targetUri;
            requestMessage.Method = GetMethod(context.Request.Method);

            if (!HttpMethods.IsGet(requestMessage.Method.Method) &&
                !HttpMethods.IsHead(requestMessage.Method.Method) &&
                !HttpMethods.IsDelete(requestMessage.Method.Method) &&
                !HttpMethods.IsTrace(requestMessage.Method.Method))
            {
                body = await RewriteBodyAsync(body);

                var memStr = new MemoryStream(Encoding.UTF8.GetBytes(body));
                var streamContent = new StreamContent(memStr);

                foreach (var header in context.Request.Headers)
                {
                    if (header.Key.ToLower() == "content-length")
                    {
                        streamContent.Headers.TryAddWithoutValidation("Content-Length", memStr.Length.ToString());
                    }
                    else
                    {
                        streamContent.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                    }
                }

                requestMessage.Content = streamContent;
            }

            return (requestMessage, body);
        }

        private void CopyFromTargetResponseHeaders(HttpContext context, HttpResponseMessage responseMessage)
        {
            foreach (var header in responseMessage.Headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }

            foreach (var header in responseMessage.Content.Headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }
            context.Response.Headers.Remove("transfer-encoding");
        }

        /// <summary>
        /// Get http method
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        private static HttpMethod GetMethod(string method)
        {
            return new HttpMethod(method);
        }

        private async Task<string> RewriteBodyAsync(string body)
        {
            if (string.IsNullOrEmpty(body))
            {
                return body;
            }

            var bodyDictionary = (IDictionary<string, Object>)JsonConvert.DeserializeObject<ExpandoObject>(body, new ExpandoObjectConverter())!;
            bodyDictionary.UpdateQuery(await _dataProviderCache.GetDefaultIdsAsync());
            if (_proxyConfiguration.ExcludeFieldsInElasticsearchQuery)
            {
                bodyDictionary.UpdateExclude(_proxyConfiguration.ExcludeFields!);
            }
            bodyDictionary.UpdateSort();
            return JsonConvert.SerializeObject(bodyDictionary);// jsonBody.ToString(); 
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="nextMiddleware"></param>
        /// <param name="processedObservationRepository"></param>
        /// <param name="proxyConfiguration"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public RequestMiddleware(RequestDelegate nextMiddleware,
            IProcessedObservationCoreRepository processedObservationRepository,
            IDataProviderCache dataProviderCache,
            ProxyConfiguration proxyConfiguration,
            ILogger<RequestMiddleware> logger)
        {
            _nextMiddleware = nextMiddleware;
            _processedObservationRepository = processedObservationRepository ??
                                              throw new ArgumentNullException(nameof(processedObservationRepository));
            _dataProviderCache = dataProviderCache ?? throw new ArgumentNullException(nameof(dataProviderCache));
            _proxyConfiguration = proxyConfiguration ??
                                  throw new ArgumentNullException(nameof(proxyConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _processedObservationRepository.LiveMode = true;
        }

        /// <summary>
        /// Handle request
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            string originalBody = null!;
            string body = null!;
            var semaphoreTime = Stopwatch.StartNew();
            if (_requestSemaphore.CurrentCount == 0)
            {
                _logger.LogDebug("All semaphore slots are occupied. Request will be queued. UserType={@userType}", ApiUserType.Unknown);
                context.Items["SemaphoreStatus"] = "Wait";
            }

            if (!await _requestSemaphore.WaitAsync(TimeSpan.FromSeconds(50)))
            {
                _logger.LogError("Too many requests. Semaphore limit reached. UserType={@userType}", ApiUserType.Unknown);
                context.Items["SemaphoreStatus"] = "Timeout";
                context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                await context.Response.WriteAsync("Too many requests. Please try again later.");
                return;
            }
            
            semaphoreTime.Stop();
            if (semaphoreTime.ElapsedMilliseconds > 1000)
            {
                context.Items["SemaphoreWaitSeconds"] = (int)Math.Round(semaphoreTime.ElapsedMilliseconds / 1000.0);
            }

            try
            {
                var requestStopwatch = Stopwatch.StartNew();
                var targetUri = BuildTargetUri(context.Request);

                context.Request.EnableBuffering();
                body = await new StreamReader(context.Request.Body).ReadToEndAsync();
                context.Request.Body.Position = 0;
                LogHelper.AddHttpContextItems(context);
                if (_proxyConfiguration.LogOriginalQuery)
                {
                    _logger.LogInformation("Original body: {@requestBody}", body);
                }

                context.Items.Add("Requesting-System", "WFS");
                context.Items["ApiUserType"] = ApiUserType.Unknown;

                if (targetUri != null)
                {
                    _logger.LogDebug($"Target: {targetUri.AbsoluteUri}");
                    originalBody = body;
                    var createMessageResponse = await CreateTargetMessageAsync(context, targetUri, body);
                    var targetRequestMessage = createMessageResponse.Message;
                    body = createMessageResponse.Body;
                    if (_proxyConfiguration.LogRequest && targetRequestMessage.Content != null)
                    {
                        _logger.LogInformation("Request body: {@requestBody}", body);
                    }
                    var httpClientHandler = new HttpClientHandler();
                    httpClientHandler.ServerCertificateCustomValidationCallback += (sender, certificate, chain, errors) => true;

                    using var httpClient = new HttpClient(httpClientHandler);
                    using var responseMessage = await httpClient.SendAsync(targetRequestMessage,
                        HttpCompletionOption.ResponseHeadersRead, context.RequestAborted);

                    context.Response.StatusCode = (int)responseMessage.StatusCode;
                    CopyFromTargetResponseHeaders(context, responseMessage);
                    if (_proxyConfiguration.LogResponse)
                    {
                        string response = await responseMessage.Content.ReadAsStringAsync();
                        _logger.LogInformation("Response body:{@responseBody}", response.WithMaxLength(_proxyConfiguration.LogResponseMaxCharacters));
                    }
                    await responseMessage.Content.CopyToAsync(context.Response.Body);

                    var match = Regex.Match(context.Request?.Path.Value ?? string.Empty, @"([^\/]+)$");
                    if (match?.Value?.ToLower()?.Equals("_search") ?? false && !context.Items.ContainsKey("Observation-count"))
                    {
                        // Estimate number of observations returned 
                        var observationCount = 0;
                        //try // todo - this gives exception
                        //{
                        //    observationCount = Convert.ToInt32(Math.Ceiling(context.Response.Body.Length / (double)_proxyConfiguration.AverageObservationSize));
                        //}
                        //catch 
                        //{
                        //    observationCount = 0;
                        //}
                        
                        context.Items.Add("Observation-count", observationCount);
                    }

                    requestStopwatch.Stop();
                    if (_proxyConfiguration.LogPerformance)
                    {
                        _logger.LogInformation("Request time: {@requestTimeMs}ms", requestStopwatch.ElapsedMilliseconds);
                    }
                    return;
                }

                await _nextMiddleware(context);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in RequestMiddleware.Invoke(). Original body: {@originalRequestBody}, New body={@requestBody}", originalBody, body);
                throw;
            }
            finally
            {
                _requestSemaphore.Release();
            }
        }
    }
}