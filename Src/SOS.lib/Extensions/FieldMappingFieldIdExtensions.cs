using System;
using SOS.Lib.Enums;

namespace SOS.Lib.Extensions
{
    public static class FieldMappingFieldIdExtensions
    {
        public static bool IsGeographicRegionField(this FieldMappingFieldId fieldMappingFieldId)
        {
            return fieldMappingFieldId == FieldMappingFieldId.County
                   || fieldMappingFieldId == FieldMappingFieldId.Municipality
                   || fieldMappingFieldId == FieldMappingFieldId.Parish
                   || fieldMappingFieldId == FieldMappingFieldId.Province;
        }

        public static AreaType GetAreaType(this FieldMappingFieldId fieldMappingFieldId)
        {
            switch (fieldMappingFieldId)
            {
                case FieldMappingFieldId.County:
                    return AreaType.County;
                case FieldMappingFieldId.Municipality:
                    return AreaType.Municipality;
                case FieldMappingFieldId.Parish:
                    return AreaType.Parish;
                case FieldMappingFieldId.Province:
                    return AreaType.Province;
                default:
                    throw new ArgumentException($"GetAreaType(): '{fieldMappingFieldId}' is not mapped to an AreaType");
            }
        }
    }
}