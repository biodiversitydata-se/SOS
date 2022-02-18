using System.Text;
using SOS.Lib.Configuration.Shared;
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
            CopyFromOriginalRequestContentAndHeaders(context, requestMessage);

            requestMessage.RequestUri = targetUri;
            requestMessage.Headers.Host = targetUri.Host;
            requestMessage.Method = GetMethod(context.Request.Method);

            return requestMessage;
        }

        private void CopyFromOriginalRequestContentAndHeaders(HttpContext context, HttpRequestMessage requestMessage)
        {
            var requestMethod = context.Request.Method;

            if (!HttpMethods.IsGet(requestMethod) &&
              !HttpMethods.IsHead(requestMethod) &&
              !HttpMethods.IsDelete(requestMethod) &&
              !HttpMethods.IsTrace(requestMethod))
            {
                var streamContent = new StreamContent(context.Request.Body);
                requestMessage.Content = streamContent;
            }
           
            foreach (var header in context.Request.Headers)
            {
                requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }

          /*  requestMessage.Headers.Remove("Authorization");
            var authenticationString = $"{_elasticSearchConfiguration.UserName}:{_elasticSearchConfiguration.Password}";
            var base64EncodedAuthenticationString = Convert.ToBase64String(ASCIIEncoding.UTF8.GetBytes(authenticationString));
            requestMessage.Headers.Add("Authorization", "Basic " + base64EncodedAuthenticationString);*/
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
            if (!request.Path.HasValue)
            {
                return null;
            }

            var hostUrl = _processedObservationRepository.HostUrl;
            var index = _processedObservationRepository.PublicIndexName;

            var url = new Uri(hostUrl, $"{index}/_search{request.QueryString}");

            return url;
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
