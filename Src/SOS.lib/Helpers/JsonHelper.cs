using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace SOS.Lib.Helpers
{
    /// <summary>
    /// Helper class for serializing objects to JSON.
    /// </summary>
    public static class JsonHelper
    {
        /// <summary>
        /// Serialize to JSON and remove properties containing default values.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="deleteProperties">Properties that should be excluded.</param>
        /// <param name="keepPropertyNames">Properties that should be kept even if they have default value.</param>
        /// <returns></returns>
        public static string SerializeToMinimalJson(object obj, IList<string> deleteProperties = null, IList<string> keepPropertyNames = null)
        {
            var jToken = JToken.FromObject(obj).RemoveEmptyChildren(keepPropertyNames);
            if (deleteProperties != null)
            {
                foreach (var deleteProperty in deleteProperties)
                {
                    jToken.SelectTokens($"[*].{deleteProperty}").ToList().ForEach(attr => attr.Parent.Remove());
                }
            }

            return jToken.ToString(Formatting.Indented);
        }


        private static JToken RemoveEmptyChildren(this JToken token, IList<string> keepPropertyNames = null)
        {
            if (token.Type == JTokenType.Object)
            {
                var copy = new JObject();
                foreach (var prop in token.Children<JProperty>())
                {
                    var child = prop.Value;
                    if (child.HasValues)
                    {
                        child = child.RemoveEmptyChildren(keepPropertyNames);
                    }

                    if (!child.IsEmptyOrDefault(keepPropertyNames))
                    {
                        copy.Add(prop.Name, child);
                    }
                }

                return copy;
            }

            if (token.Type == JTokenType.Array)
            {
                var copy = new JArray();
                foreach (var item in token.Children())
                {
                    var child = item;
                    if (child.HasValues)
                    {
                        child = child.RemoveEmptyChildren(keepPropertyNames);
                    }

                    if (!child.IsEmptyOrDefault(keepPropertyNames))
                    {
                        copy.Add(child);
                    }
                }

                return copy;
            }

            return token;
        }

        private static bool IsEmptyOrDefault(this JToken token, IList<string> keepPropertyNames = null)
        {
            if (AlwaysKeepProperty(token, keepPropertyNames)) return false;
            return token.Type == JTokenType.Array && !token.HasValues ||
                   token.Type == JTokenType.Object && !token.HasValues ||
                   token.Type == JTokenType.String && token.ToString() == string.Empty ||
                   token.Type == JTokenType.Boolean && token.Value<bool>() == false ||
                   token.Type == JTokenType.Integer && token.Value<int>() == 0 ||
                   token.Type == JTokenType.Float && Math.Abs(token.Value<double>()) < 0.001 ||
                   token.Type == JTokenType.Null;

            // Use the following code if you want to honor the [DefaultValue] attribute:
            // return (token.Type == JTokenType.Array && !token.HasValues) ||
            //        (token.Type == JTokenType.Object && !token.HasValues);
        }

        private static bool AlwaysKeepProperty(JToken token, IList<string> keepPropertyNames)
        {
            if (keepPropertyNames == null) return false;
            if (token.Parent == null) return false;
            if (!(token.Parent is JProperty parent)) return false;
            return keepPropertyNames.Contains(parent.Name);
        }
    }
}