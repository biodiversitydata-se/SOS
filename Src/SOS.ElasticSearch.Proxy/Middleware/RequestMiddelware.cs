using System.Text.RegularExpressions;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.ElasticSearch.Proxy.Configuration;

namespace SOS.ElasticSearch.Proxy.Middleware
{
    public class RequestMiddelware
    {
        private readonly RequestDelegate _nextMiddleware;
        private readonly IProcessedObservationRepository _processedObservationRepository;
        private readonly int _averageObservationSize;
        private readonly ILogger<RequestMiddelware> _logger;

        private HttpRequestMessage CreateTargetMessage(HttpContext context, Uri targetUri)
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
                var streamContent = new StreamContent(context.Request.Body);

                foreach (var header in context.Request.Headers)
                {
                    streamContent.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
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
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public RequestMiddelware(RequestDelegate nextMiddleware, 
            IProcessedObservationRepository processedObservationRepository,
            ProxyConfiguration proxyConfiguration,
            ILogger<RequestMiddelware> logger)
        {
            _nextMiddleware = nextMiddleware;
            _processedObservationRepository = processedObservationRepository ??
                                              throw new ArgumentNullException(nameof(processedObservationRepository));
            _averageObservationSize = proxyConfiguration?.AvrageObservationSize ?? throw new ArgumentNullException(nameof(proxyConfiguration));
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
            var targetUri = BuildTargetUri(context.Request);

            if (targetUri != null)
            {
                _logger.LogDebug($"Target: {targetUri.AbsoluteUri}");
                var targetRequestMessage = CreateTargetMessage(context, targetUri);
                string body = await targetRequestMessage.Content?.ReadAsStringAsync();
                if (body != null)
                {
                    _logger.LogDebug($"Body: {body}");
                }
                var httpClientHandler = new HttpClientHandler();
                httpClientHandler.ServerCertificateCustomValidationCallback += (sender, certificate, chain, errors) => true;

                using var httpClient = new HttpClient(httpClientHandler);
                using var responseMessage = await httpClient.SendAsync(targetRequestMessage,
                    HttpCompletionOption.ResponseHeadersRead, context.RequestAborted);
                
                context.Response.StatusCode = (int)responseMessage.StatusCode;
                CopyFromTargetResponseHeaders(context, responseMessage);
                await responseMessage.Content.CopyToAsync(context.Response.Body);
                string response = await responseMessage.Content.ReadAsStringAsync();
                if (response != null)
                {
                    _logger.LogDebug($"Response: {response}");
                }

                var match = Regex.Match(context.Request?.Path.Value ?? string.Empty, @"([^\/]+)$");
                if (match?.Value?.ToLower()?.Equals("_search") ?? false && !context.Items.ContainsKey("Observation-count"))
                {
                    // Estimate number of observations returned 
                    var observationCount = Math.Ceiling((double)((context.Response.ContentLength ?? 0) / _averageObservationSize));
                    context.Items.Add("Observation-count", observationCount);
                }
                return;
            }
            await _nextMiddleware(context);
        }
    }
}
