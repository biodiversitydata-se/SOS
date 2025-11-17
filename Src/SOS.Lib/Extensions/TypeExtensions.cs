using System;
using System.Diagnostics;
using System.Linq;

namespace SOS.Lib.Extensions;
public static class TypeExtensions
{
    extension(Type type)
    {
        /// <summary>
        /// Returns the type name. If this is a generic type, appends
        /// the list of generic type arguments between angle brackets.
        /// (Does not account for embedded / inner generic arguments.)
        /// </summary>
        /// <returns>System.String.</returns>
        public string GetFormattedName()
        {
            if (type.IsGenericType)
            {
                string genericArguments = type.GetGenericArguments()
                                    .Select(x => x.GetFormattedName())
                                    .Aggregate((x1, x2) => $"{x1}, {x2}");
                return $"{type.Name.Substring(0, type.Name.IndexOf("`"))}<{genericArguments}>";
            }

            return type.Name;
        }
    }
}
