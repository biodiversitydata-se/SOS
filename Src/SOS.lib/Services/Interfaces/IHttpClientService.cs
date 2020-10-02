using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SOS.Lib.Services.Interfaces
{
    /// <summary>
    ///     Class for sending http requests
    /// </summary>
    public interface IHttpClientService : IDisposable
    {
        /// <summary>
        ///     Get requests
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="requestUri"></param>
        /// <returns></returns>
        Task<T> GetDataAsync<T>(Uri requestUri);

        /// <summary>
        ///     Get request with header data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="requestUri"></param>
        /// <param name="headerData"></param>
        /// <returns></returns>
        Task<T> GetDataAsync<T>(Uri requestUri, Dictionary<string, string> headerData);

        /// <summary>
        ///     Get file data stream
        /// </summary>
        /// <param name="requestUri"></param>
        /// <param name="headerData"></param>
        /// <returns></returns>
        Task<Stream> GetFileStreamAsync(Uri requestUri, Dictionary<string, string> headerData = null);

        /// <summary>
        /// Post request
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="requestUri"></param>
        /// <param name="model"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        Task<T> PostDataAsync<T>(Uri requestUri, object model);

        /// <summary>
        /// Post request with header data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="requestUri"></param>
        /// <param name="model"></param>
        /// <param name="headerData"></param>
        /// <returns></returns>
        Task<T> PostDataAsync<T>(Uri requestUri, object model, Dictionary<string, string> headerData);

        /// <summary>
        /// Post request with header data and content type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="requestUri"></param>
        /// <param name="model"></param>
        /// <param name="headerData"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        Task<T> PostDataAsync<T>(Uri requestUri, object model, Dictionary<string, string> headerData, string contentType);

        /// <summary>
        /// Put requests
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="requestUri"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<T> PutDataAsync<T>(Uri requestUri, object model);

        /// <summary>
        /// Put request
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="requestUri"></param>
        /// <param name="model"></param>
        /// <param name="headerData"></param>
        /// <returns></returns>
        Task<T> PutDataAsync<T>(Uri requestUri, object model, Dictionary<string, string> headerData);

        /// <summary>
        /// Put requests
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="requestUri"></param>
        /// <param name="model"></param>
        /// <param name="headerData"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        Task<T> PutDataAsync<T>(Uri requestUri, object model, Dictionary<string, string> headerData, string contentType);

        /// <summary>
        ///     Delete requests
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="requestUri"></param>
        /// <returns></returns>
        Task<T> DeleteDataAsync<T>(Uri requestUri);
    }
}