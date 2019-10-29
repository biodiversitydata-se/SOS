using System;
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
        /// Post request
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="requestUri"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<T> PostDataAsync<T>(Uri requestUri, object model);

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
    }
}