using SOS.Lib.Models;

using SOS.Shared.Api.Dtos.Vocabulary;

namespace SOS.Shared.Api.Extensions.Dto
{
    public static class PropertyFieldExtensions
    {
        public static PropertyFieldDescriptionDto ToPropertyFieldDescriptionDto(this PropertyFieldDescription fieldDescription)
        {
            if (fieldDescription == null)
            {
                return null!;
            }

            return new PropertyFieldDescriptionDto
            {
                PropertyPath = fieldDescription.PropertyPath,
                DataType = fieldDescription.DataTypeEnum,
                DataTypeNullable = fieldDescription.DataTypeIsNullable.GetValueOrDefault(false),
                DwcIdentifier = fieldDescription.DwcIdentifier,
                DwcName = fieldDescription.DwcName,
                EnglishTitle = fieldDescription.GetEnglishTitle(),
                SwedishTitle = fieldDescription.GetSwedishTitle(),
                Name = fieldDescription.PropertyName,
                FieldSet = fieldDescription.FieldSetEnum,
                PartOfFieldSets = fieldDescription.FieldSets
            };
        }

        public static List<PropertyFieldDescriptionDto> ToPropertyFieldDescriptionDtos(this IEnumerable<PropertyFieldDescription> fieldDescriptions)
        {
            return fieldDescriptions.Select(fieldDescription => fieldDescription.ToPropertyFieldDescriptionDto()).ToList();
        }
    }
}