using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOS.Lib.Extensions
{
    /// <summary>
    /// Extension methods for HttpContext
    /// </summary>
    public static class HttpContextExtensions
    {        
        /// <summary>
        /// Add Observation count to HttpContext.Items for later use when 
        /// logging request to Application Insights.
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="nrObservations"></param>
        public static void LogObservationCount(this HttpContext httpContext, int nrObservations)
        {
            if (httpContext == null) return;            

            if (!httpContext.Items.ContainsKey("Observation-count"))
            {
                httpContext.Items.Add("Observation-count", nrObservations);
            }
        }
    }
}