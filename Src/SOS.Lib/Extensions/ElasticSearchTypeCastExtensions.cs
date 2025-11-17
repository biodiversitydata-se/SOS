using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Fluent;
using Elastic.Clients.Elasticsearch.QueryDsl;
using System.Collections.Generic;
using System.Linq;

namespace SOS.Lib.Extensions;

public static class ElasticSearchTypeCastExtensions
{
    extension<TKey, TValue>(IReadOnlyDictionary<TKey, TValue> readOnlyDictionary)
    {
        /// Cast Readonly dictionary to fluent dictionary
        public FluentDictionary<TKey, TValue> ToFluentDictionary()
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
    }

    extension(IReadOnlyDictionary<string, FieldValue> readOnlyDictionary)
    {
        public FluentDictionary<Field, FieldValue> ToFluentFieldDictionary()
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
    }

    extension(IEnumerable<bool> termList)
    {
        #region To Elastic Types
        /// <summary>
        /// Cast list of boolean to TermsQueryField
        /// </summary>
        /// <returns></returns>
        public TermsQueryField ToTermsQueryField()
        {
            return new TermsQueryField(termList?.Select(v => FieldValue.Boolean(v)).ToArray());
        }
    }

    extension(IEnumerable<double> termList)
    {
        /// <summary>
        /// Cast list of double to TermsQueryField
        /// </summary>
        /// <returns></returns>
        public TermsQueryField ToTermsQueryField()
        {
            return new TermsQueryField(termList?.Select(v => FieldValue.Double(v)).ToArray());
        }
    }

    extension(IEnumerable<long> termList)
    {
        /// <summary>
        /// Cast list of long to TermsQueryField
        /// </summary>
        /// <returns></returns>
        public TermsQueryField ToTermsQueryField()
        {
            return new TermsQueryField(termList?.Select(v => FieldValue.Long(v)).ToArray());
        }
    }

    extension(IEnumerable<string> termList)
    {
        /// <summary>
        /// Cast list of string to TermsQueryField
        /// </summary>
        /// <returns></returns>
        public TermsQueryField ToTermsQueryField()
        {
            return new TermsQueryField(termList?.Select(v => FieldValue.String(v)).ToArray());
        }
    }

    extension(string property)
    {
        /// <summary>
        /// Cast property to field
        /// </summary>
        /// <returns></returns>
        public Field ToField()
        {
            return new Field(string.Join('.', property.Split('.').Select(p => p
                .ToCamelCase()
            )));
        }
    }

    extension(IEnumerable<string> properties)
    {
        public Fields ToFields()
        {
            return Fields.FromFields(properties?.Select(s => s.ToField())?.ToArray());
        }
    }

    extension<TValue>(TValue value)
    {
        public FieldValue ToFieldValue()
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
                intValue != null ||
                shortValue != null ||
                byteValue != null)
            {
                return FieldValue.Long(longValue ?? intValue ?? shortValue ?? byteValue ?? 0);
            }
            return FieldValue.String(value?.ToString());
        }
    }

    extension<TValue>(IEnumerable<TValue> values)
    {
        public IReadOnlyCollection<FieldValue> ToFieldValues()
        {
            return values?.Select(v => v.ToFieldValue()).ToList();
        }
    }

    #endregion To Elastic Types
}
