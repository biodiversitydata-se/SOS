using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SOS.Observations.Api.Swagger
{
    /// <summary>
    /// This attribute is used to indicate that an endpoint belongs to the internal API.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class InternalApiAttribute : Attribute
    {
    }
}
