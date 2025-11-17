using SOS.Lib.Models;

using SOS.Shared.Api.Dtos.Vocabulary;

namespace SOS.Shared.Api.Extensions.Dto;

public static class PropertyFieldExtensions
{
    extension(PropertyFieldDescription fieldDescription)
    {
        public PropertyFieldDescriptionDto ToPropertyFieldDescriptionDto()
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
    }

    extension(IEnumerable<PropertyFieldDescription> fieldDescriptions)
    {
        public List<PropertyFieldDescriptionDto> ToPropertyFieldDescriptionDtos()
        {
            return fieldDescriptions.Select(fieldDescription => fieldDescription.ToPropertyFieldDescriptionDto()).ToList();
        }
    }
}