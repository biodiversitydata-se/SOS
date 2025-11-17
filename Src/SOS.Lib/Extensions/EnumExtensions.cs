using System;
using System.Linq;
using System.Runtime.Serialization;

namespace SOS.Lib.Extensions;

public static class EnumExtensions
{
    extension(Enum @enum)
    {
        public string GetEnumMemberAttrValue()
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

    extension(string value)
    {
        /// <summary>
        /// Extension method to return an enum value of type T for the given string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T ToEnum<T>()
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }
    }

    extension(int value)
    {
        /// <summary>
        /// Extension method to return an enum value of type T for the given int.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T ToEnum<T>()
        {
            var name = Enum.GetName(typeof(T), value);
            return name.ToEnum<T>();
        }
    }
}
