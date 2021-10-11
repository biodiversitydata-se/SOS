using Microsoft.AspNetCore.Http;
using SOS.Lib.Security.Interfaces;

namespace SOS.Lib.Security
{
    /// <summary>
    /// CurrentUserAuthorization
    /// </summary>
    public class CurrentUserAuthorization : IAuthorizationProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        public CurrentUserAuthorization(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Create auth header string using current user.
        /// </summary>
        /// <returns></returns>
        public string GetAuthHeader()
        {
            return _httpContextAccessor?.HttpContext?.Request?.Headers["Authorization"];
        }
    }
}
