using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Fluent;
using Elastic.Clients.Elasticsearch.QueryDsl;
using System.Collections.Generic;
using System.Linq;

namespace SOS.Lib.Extensions
{
    public static class ElasticSearchTypeCastExtensions
    {
        /// Cast Readonly dictionary to fluent dictionary
        public static FluentDictionary<TKey, TValue> ToFluentDictionary<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> readOnlyDictionary)
        {
            if ((readOnlyDictionary?.Count ?? 0) == 0)
            {
                return null;
            }

            var fluentDictionary = new FluentDictionary<TKey, TValue>();
            foreach (var item in readOnlyDictionary)
            {
                fluentDictionary.Add(item.Key, item.Value);
            }

            return fluentDictionary;
        }

        public static FluentDictionary<Field, FieldValue> ToFluentFieldDictionary(this IReadOnlyDictionary<string, FieldValue> readOnlyDictionary)
        {
            if ((readOnlyDictionary?.Count ?? 0) == 0)
            {
                return null;
            }

            var fluentDictionary = new FluentDictionary<Field, FieldValue>();
            foreach (var item in readOnlyDictionary)
            {
                fluentDictionary.Add(item.Key, item.Value);
            }

            return fluentDictionary;
        }

        #region To Elastic Types
        /// <summary>
        /// Cast list of boolean to TermsQueryField
        /// </summary>
        /// <param name="termList"></param>
        /// <returns></returns>
        public static TermsQueryField ToTermsQueryField(this IEnumerable<bool> termList)
        {
            return new TermsQueryField(termList?.Select(v => FieldValue.Boolean(v)).ToArray());
        }

        /// <summary>
        /// Cast list of double to TermsQueryField
        /// </summary>
        /// <param name="termList"></param>
        /// <returns></returns>
        public static TermsQueryField ToTermsQueryField(this IEnumerable<double> termList)
        {
            return new TermsQueryField(termList?.Select(v => FieldValue.Double(v)).ToArray());
        }

        /// <summary>
        /// Cast list of long to TermsQueryField
        /// </summary>
        /// <param name="termList"></param>
        /// <returns></returns>
        public static TermsQueryField ToTermsQueryField(this IEnumerable<long> termList)
        {
            return new TermsQueryField(termList?.Select(v => FieldValue.Long(v)).ToArray());
        }

        /// <summary>
        /// Cast list of string to TermsQueryField
        /// </summary>
        /// <param name="termList"></param>
        /// <returns></returns>
        public static TermsQueryField ToTermsQueryField(this IEnumerable<string> termList)
        {
            return new TermsQueryField(termList?.Select(v => FieldValue.String(v)).ToArray());
        }
        
  
        /// <summary>
        /// Cast property to field
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static Field ToField(this string property)
        {
            return new Field(string.Join('.', property.Split('.').Select(p => p
                .ToCamelCase()
            )));
        }

        public static Fields ToFields(this IEnumerable<string> properties)
        {
            return Fields.FromFields(properties?.Select(s => s.ToField())?.ToArray());
        }

        public static FieldValue ToFieldValue<TValue>(this TValue value)
        {
            if (value == null)
            {
                return null;
            }
            var boolValue = value as bool?;
            if (boolValue ?? false)
            {
                return FieldValue.Boolean(boolValue ?? false);
            }
            var doubleValue = value as double?;
            if (doubleValue != null)
            {
                return FieldValue.Double(doubleValue ?? 0);
            }
            var longValue = value as long?;
            var intValue = value as int?;
            var shortValue = value as long?;
            var byteValue = value as long?;
            if (longValue != null ||
                intValue != 0 ||
                shortValue != 0 ||
                byteValue != null)
            {
                return FieldValue.Long(longValue ?? intValue ?? shortValue ?? byteValue ?? 0);
            }
            return FieldValue.String(value?.ToString());
        }

        public static IReadOnlyCollection<FieldValue> ToFieldValues<TValue>(this IEnumerable<TValue> values)
        {
            return values?.Select(v => v.ToFieldValue()).ToList();
        }

        #endregion To Elastic Types
    }

}
