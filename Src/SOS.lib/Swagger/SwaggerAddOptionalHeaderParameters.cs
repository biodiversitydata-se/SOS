﻿using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;

namespace SOS.Lib.Swagger
{
    /// <summary>
    /// Add optional headers to swagger
    /// </summary>
    public class SwaggerAddOptionalHeaderParameters : IOperationFilter
    {
        /// <summary>
        /// Apply headers
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="context"></param>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Parameters ??= new List<OpenApiParameter>();

            operation.Parameters.Add(new OpenApiParameter()
            {
                Name = "X-Requesting-System",
                Description = "Name of system doing the request",
                In = ParameterLocation.Header,
                Required = false,
                Schema = new OpenApiSchema()
                {
                    Type = "string"
                }
            });
        }
    }
}
