
namespace SOS.Lib.Security.Interfaces
{
    public interface IAuthorizationProvider
    {
        /// <summary>
        /// Create authorization header string.
        /// </summary>
        /// <returns>Auth header string.</returns>
        string GetAuthHeader();
    }
}
