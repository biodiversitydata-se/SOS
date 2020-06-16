using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;

namespace SOS.Observations.Api.Controllers
{
    public class BaseController : ControllerBase
    {
        protected string ReplaceDomain(string str, string domain, string path)
        {
            // This is a bad solution to fix problems when behind load balancer...
            return Regex.Replace(str, string.Format(@"(https?:\/\/.*?)(\/{0}\/v2)?(\/.*)", path), m => domain + m.Groups[3].Value);
        }

        /// <summary>
        /// Current user id
        /// </summary>
        protected int CurrentUserId => int.Parse(User?.Claims?.FirstOrDefault(c => c?.Type == "sub")?.Value ?? "0");
    }
}
