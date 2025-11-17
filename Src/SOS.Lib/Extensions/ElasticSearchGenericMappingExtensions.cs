using Elastic.Clients.Elasticsearch.Mapping;
using SOS.Lib.Enums;
using System;
using System.Linq.Expressions;

namespace SOS.Lib.Extensions;

internal static class ElasticSearchGenericMappingExtensions
{
    /// <summary>
    /// Get index settings
    /// </summary>
    /// <param name="indexSetting"></param>
    /// <returns></returns>
    private static (bool IndexForSearch, bool IndexForSortAndAggregate) GetIndexSettings(IndexSetting indexSetting)
    {
        return indexSetting switch
        {
            IndexSetting.None => (false, false),
            IndexSetting.SearchOnly => (true, false),
            _ => (true, true)
        };
    }

    /// <summary>
    /// Map boolean property
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="propertiesDescriptor"></param>
    /// <param name="propertyName"></param>
    /// <param name="indexSetting"></param>
    /// <returns></returns>
    public static PropertiesDescriptor<T> BooleanVal<T, TValue>(this PropertiesDescriptor<T> propertiesDescriptor,
        Expression<Func<T, TValue>> propertyName,
        IndexSetting indexSetting = IndexSetting.SearchSortAggregate) where T : class
    {
        var indexSettings = GetIndexSettings(indexSetting);
        return propertiesDescriptor
            .Boolean(propertyName, b => b
                .Index(indexSettings.IndexForSearch)
                .DocValues(indexSettings.IndexForSortAndAggregate)
            );
    }

    /// <summary>
    /// Map date property
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="propertiesDescriptor"></param>
    /// <param name="propertyName"></param>
    /// <param name="indexSetting"></param>
    /// <returns></returns>
    public static PropertiesDescriptor<T> DateVal<T, TValue>(this PropertiesDescriptor<T> propertiesDescriptor,
        Expression<Func<T, TValue>> propertyName,
        IndexSetting indexSetting = IndexSetting.SearchSortAggregate) where T : class
    {
        var indexSettings = GetIndexSettings(indexSetting);
        return propertiesDescriptor
            .Date(propertyName, n => n
                .Index(indexSettings.IndexForSearch)
                .DocValues(indexSettings.IndexForSortAndAggregate)
            );
    }

    /// <summary>
    /// Map keword property
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="propertiesDescriptor"></param>
    /// <param name="propertyName"></param>
    /// <param name="indexSetting"></param>
    /// <param name="normalizer"></param>
    /// <param name="ignoreAbove"></param>
    /// <returns></returns>
    public static PropertiesDescriptor<T> KeywordVal<T, TValue>(this PropertiesDescriptor<T> propertiesDescriptor,
        Expression<Func<T, TValue>> propertyName,
        IndexSetting indexSetting = IndexSetting.SearchSortAggregate,
        Normalizer? normalizer = Normalizer.LowerCase,
        int? ignoreAbove = null) where T : class
    {
        var indexSettings = GetIndexSettings(indexSetting);
        return propertiesDescriptor
            .Keyword(propertyName, c => c
                .DocValues(indexSettings.IndexForSortAndAggregate)
                .IgnoreAbove(ignoreAbove)
                .Index(indexSettings.IndexForSearch)
                .IndexOptions(IndexOptions.Docs)
                .Normalizer(normalizer switch { Normalizer.LowerCase => "lowercase", _ => null })
            );
    }

    /// <summary>
    /// Map number property
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="propertiesDescriptor"></param>
    /// <param name="propertyName"></param>
    /// <param name="indexSetting"></param>
    /// <param name="numberType"></param>
    /// <returns></returns>
    public static PropertiesDescriptor<T> NumberVal<T, TValue>(this PropertiesDescriptor<T> propertiesDescriptor,
        Expression<Func<T, TValue>> propertyName,
        IndexSetting indexSetting,
        NumberType numberType) where T : class
    {
        var indexSettings = GetIndexSettings(indexSetting);
        return numberType switch
        {
            NumberType.Byte => propertiesDescriptor
                .ByteNumber(propertyName, n => n
                    .Index(indexSettings.IndexForSearch)
                    .DocValues(indexSettings.IndexForSortAndAggregate)
                ),
            NumberType.Double => propertiesDescriptor
                .DoubleNumber(propertyName, n => n
                    .Index(indexSettings.IndexForSearch)
                    .DocValues(indexSettings.IndexForSortAndAggregate)
                ),
            NumberType.Float => propertiesDescriptor
                .FloatNumber(propertyName, n => n
                    .Index(indexSettings.IndexForSearch)
                    .DocValues(indexSettings.IndexForSortAndAggregate)
                ),
            NumberType.Long => propertiesDescriptor
                .LongNumber(propertyName, n => n
                    .Index(indexSettings.IndexForSearch)
                    .DocValues(indexSettings.IndexForSortAndAggregate)
                ),
            NumberType.Short => propertiesDescriptor
                .ShortNumber(propertyName, n => n
                    .Index(indexSettings.IndexForSearch)
                    .DocValues(indexSettings.IndexForSortAndAggregate)
                ),
            _ => propertiesDescriptor
                .IntegerNumber(propertyName, n => n
                    .Index(indexSettings.IndexForSearch)
                    .DocValues(indexSettings.IndexForSortAndAggregate)
                )
        };
    }

    /// <summary>
    /// Map text field
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="propertiesDescriptor"></param>
    /// <param name="propertyName"></param>
    /// <param name="index"></param>
    /// <param name="ignoreAbove"></param>
    /// <returns></returns>
    public static PropertiesDescriptor<T> TextVal<T, TValue>(this PropertiesDescriptor<T> propertiesDescriptor,
        Expression<Func<T, TValue>> propertyName,
        bool index = true,
        int? ignoreAbove = null) where T : class
    {
        return propertiesDescriptor
            .Text(propertyName, c => c
            .IgnoreAbove(ignoreAbove)
                .Index(index)
                .IndexOptions(IndexOptions.Docs)
            );
    }
}
