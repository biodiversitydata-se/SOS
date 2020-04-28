using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SOS.Import.Services.Interfaces
{
    /// <summary>
    /// Class for sending http requests
    /// </summary>
    public interface IHttpClientService
    {
        /// <summary>
        /// Get requests
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="requestUri"></param>
        /// <returns></returns>
        Task<T> GetDataAsync<T>(Uri requestUri);

        /// <summary>
        /// Get request with header data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="requestUri"></param>
        /// <param name="headerData"></param>
        /// <returns></returns>
        Task<T> GetDataAsync<T>(Uri requestUri, Dictionary<string, string> headerData);
        
        /// <summary>
        /// Post request
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="requestUri"></param>
        /// <param name="model"></param>
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
        /// Put requests
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="requestUri"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<T> PutDataAsync<T>(Uri requestUri, object model);

        /// <summary>
        /// Delete requests
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="requestUri"></param>
        /// <returns></returns>
        Task<T> DeleteDataAsync<T>(Uri requestUri);

        /// <summary>
        /// Get file data
        /// </summary>
        /// <param name="requestUri"></param>
        /// <returns></returns>
        Task<IEnumerable<byte>> ReadFileDataAsync(Uri requestUri);
    }
}