using Microsoft.OpenApi.Models;
using System;

namespace SOS.Lib.Swagger
{
    /// <summary>
    /// This attribute is used to force swagger to use aspecified schema.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class UseSchema : Attribute
    {
        /// <summary>
        /// Available schemas to implement
        /// </summary>
        public enum SchemaDataType
        {
            Date,
            DateTime,
            Integer,
            String,
        }

        /// <summary>
        /// Schema to use
        /// </summary>
        public OpenApiSchema Schema { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="description"></param>
        public UseSchema(SchemaDataType schema, string description = null)
        {
            Schema = schema switch {
                SchemaDataType.Date => new OpenApiSchema { Type = "string", Format = "date", Description = description },
                SchemaDataType.DateTime => new OpenApiSchema { Type = "string", Format = "date-time", Description = description },
                SchemaDataType.Integer => new OpenApiSchema { Type = "integer", Format = "int32", Description=description },
                _ => new OpenApiSchema { Type = "string", Format = "string", Description = description }
            };
        }
    }
}
