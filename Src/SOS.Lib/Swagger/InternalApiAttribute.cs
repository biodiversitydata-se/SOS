using System;

namespace SOS.Lib.Swagger
{
    /// <summary>
    /// This attribute is used to indicate that an endpoint belongs to the internal API.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class InternalApiAttribute : Attribute
    {
    }
}
