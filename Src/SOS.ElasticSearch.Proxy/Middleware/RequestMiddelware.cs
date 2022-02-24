using System.Net.Http.Headers;
using SOS.Lib.Repositories.Processed.Interfaces;

namespace SOS.ElasticSearch.Proxy.Middleware
{
    public class RequestMiddelware
    {
        private readonly RequestDelegate _nextMiddleware;
        private readonly IProcessedObservationRepository _processedObservationRepository;

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
            var hostUrl = _processedObservationRepository.HostUrl;
            var index = _processedObservationRepository.PublicIndexName;
            var path = string.Join('/', request.Path.HasValue ? request.Path.Value.Split('/', StringSplitOptions.RemoveEmptyEntries).Skip(1) : Array.Empty<string>());
            return new Uri(hostUrl, $"{index}/{path}/{request.QueryString}");
        }

       /// <summary>
       /// Constructor
       /// </summary>
       /// <param name="nextMiddleware"></param>
       /// <param name="processedObservationRepository"></param>
       /// <exception cref="ArgumentNullException"></exception>
        public RequestMiddelware(RequestDelegate nextMiddleware, IProcessedObservationRepository processedObservationRepository)
        {
            _nextMiddleware = nextMiddleware;
            _processedObservationRepository = processedObservationRepository ??
                                              throw new ArgumentNullException(nameof(processedObservationRepository));
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
                var targetRequestMessage = CreateTargetMessage(context, targetUri);
                var httpClientHandler = new HttpClientHandler();
                httpClientHandler.ServerCertificateCustomValidationCallback += (sender, certificate, chain, errors) => true;

                using var httpClient = new HttpClient(httpClientHandler);
                using var responseMessage = await httpClient.SendAsync(targetRequestMessage,
                    HttpCompletionOption.ResponseHeadersRead, context.RequestAborted);
                
                    context.Response.StatusCode = (int)responseMessage.StatusCode;
                    CopyFromTargetResponseHeaders(context, responseMessage);
                    await responseMessage.Content.CopyToAsync(context.Response.Body);
                
                return;
            }
            await _nextMiddleware(context);
        }
    }
}
