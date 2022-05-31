using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.ElasticSearch.Proxy.Configuration;
using SOS.Lib.Extensions;

namespace SOS.ElasticSearch.Proxy.Middleware
{
    public class RequestMiddleware
    {
        private readonly RequestDelegate _nextMiddleware;
        private readonly IProcessedObservationRepository _processedObservationRepository;
        private readonly ProxyConfiguration _proxyConfiguration;
        private readonly ILogger<RequestMiddleware> _logger;

        private HttpRequestMessage CreateTargetMessage(HttpContext context, Uri targetUri, string body)
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
                _logger.LogDebug($"Body: {body}");
                using var memStr = new MemoryStream(Encoding.UTF8.GetBytes(body));
                _logger.LogDebug($"memStr length={memStr.Length}");
                using var streamContent = new StreamContent(memStr);

                foreach (var header in context.Request.Headers)
                {
                    streamContent.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                    _logger.LogDebug($"Header: {header.Key}={header.Value}");
                }
                
                requestMessage.Content = streamContent;
            }
            
            return requestMessage;
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

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="nextMiddleware"></param>
        /// <param name="processedObservationRepository"></param>
        /// <param name="proxyConfiguration"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public RequestMiddleware(RequestDelegate nextMiddleware, 
            IProcessedObservationRepository processedObservationRepository,
            ProxyConfiguration proxyConfiguration,
            ILogger<RequestMiddleware> logger)
        {
            _nextMiddleware = nextMiddleware;
            _processedObservationRepository = processedObservationRepository ??
                                              throw new ArgumentNullException(nameof(processedObservationRepository));
            _proxyConfiguration = proxyConfiguration ??
                                  throw new ArgumentNullException(nameof(proxyConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _processedObservationRepository.LiveMode = true;
        }

        private string StreamToString(Stream stream)
        {
            var streamContent = new StreamContent(stream);
            var str = streamContent.ReadAsStringAsync().Result;
            return str;
        }

        /// <summary>
        /// Handle request
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            try
            {
                var requestStopwatch = Stopwatch.StartNew();
                var targetUri = BuildTargetUri(context.Request);

                context.Request.EnableBuffering();
                var query = await new StreamReader(context.Request.Body).ReadToEndAsync();
                context.Request.Body.Position = 0;

                // Rewrite sort by _id to use sort by event.endDate
                if (_proxyConfiguration.LogRequest)
                {
                    _logger.LogInformation($"Query before sort change: {query}");
                }
                query = query.Replace("\"sort\":[{\"_id\":{\"order\":\"asc\"}}",
                    "\"sort\":[{\"event.endDate\":{\"order\":\"desc\"}}");
                query = query.Replace("\"sort\":[{\"_id\":{\"order\":\"desc\"}}",
                    "\"sort\":[{\"event.endDate\":{\"order\":\"desc\"}}");
                if (_proxyConfiguration.LogRequest)
                {
                    _logger.LogInformation($"Query after sort change: {query}");
                }
                
                if (targetUri != null)
                {
                    if (_proxyConfiguration.ExcludeFieldsInElasticsearchQuery)
                    {
                        _logger.LogDebug($"OriginalQuery: {query}");
                        query = $"{{ {_proxyConfiguration.ExcludeFieldsQuery} {query.Substring(1, query.Length - 1)}";
                    }

                    _logger.LogDebug($"Target: {targetUri.AbsoluteUri}");
                    var targetRequestMessage = CreateTargetMessage(context, targetUri, query);
                    if (_proxyConfiguration.LogRequest && targetRequestMessage.Content != null)
                    {
                        _logger.LogInformation($"Request:\r\n{query}");
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
                        _logger.LogInformation($"Response:\r\n{response.WithMaxLength(_proxyConfiguration.LogResponseMaxCharacters)}");
                    }
                    await responseMessage.Content.CopyToAsync(context.Response.Body);

                    var match = Regex.Match(context.Request?.Path.Value ?? string.Empty, @"([^\/]+)$");
                    if (match?.Value?.ToLower()?.Equals("_search") ?? false && !context.Items.ContainsKey("Observation-count"))
                    {
                        // Estimate number of observations returned 
                        var observationCount = Math.Ceiling((context.Response.ContentLength ?? 0) / (double)_proxyConfiguration.AverageObservationSize);
                        context.Items.Add("Observation-count", observationCount);
                    }

                    requestStopwatch.Stop();
                    if (_proxyConfiguration.LogPerformance)
                    {
                        _logger.LogInformation($"Request time: {requestStopwatch.ElapsedMilliseconds}ms");
                    }
                    return;
                }

                await _nextMiddleware(context);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Errror in RequestMiddleware.Invoke()");
                throw;
            }
        }
    }
}