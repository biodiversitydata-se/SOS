using System;
using System.Linq;
using System.Runtime.Serialization;

namespace SOS.Lib.Extensions
{
    public static class EnumExtensions
    {
        public static string GetEnumMemberAttrValue(this Enum @enum)
        {
            var attr =
                @enum.GetType().GetMember(@enum.ToString()).FirstOrDefault()?.
                    GetCustomAttributes(false).OfType<EnumMemberAttribute>().
                    FirstOrDefault();
            if (attr == null)
                return @enum.ToString();
            return attr.Value;
        }
    }
}
