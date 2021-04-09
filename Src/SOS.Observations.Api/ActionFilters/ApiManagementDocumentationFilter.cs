using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SOS.Observations.Api.ActionFilters
{
    /// <summary>
    /// Operation filter that modifies the output of the swagger file to 
    /// be a better fit in Azure Api Management.
    /// </summary>
    public class ApiManagementDocumentationFilter : IOperationFilter
    {
        /// <summary>
        /// Move operation ID to summary and summary to description.
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="context"></param>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            //var summary = operation.Summary;
            //operation.Summary = operation.OperationId;
            //operation.Description = summary;
            operation.Description = operation.Summary;
            operation.Summary = null;
        }
    }
}
