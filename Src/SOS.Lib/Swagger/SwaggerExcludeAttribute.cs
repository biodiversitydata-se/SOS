using System;

namespace SOS.Lib.Swagger
{
    /// <summary>
    /// This attribute can be used to exclude properties from the generated Swagger schema.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class SwaggerExcludeAttribute : Attribute
    {
    }
}
