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
        public UseSchema(SchemaDataType schema)
        {
            Schema = schema switch {
                SchemaDataType.Date => new OpenApiSchema { Type = "string", Format = "date" },
                SchemaDataType.DateTime => new OpenApiSchema { Type = "string", Format = "date-time" },
                SchemaDataType.Integer => new OpenApiSchema { Type = "integer", Format = "int32" },
                _ => new OpenApiSchema { Type = "string", Format = "string" }
            };
        }
    }
}
