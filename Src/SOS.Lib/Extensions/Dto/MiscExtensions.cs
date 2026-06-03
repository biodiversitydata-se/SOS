using SOS.Lib.Models.Search.Result;
using SOS.Lib.Dtos;

namespace SOS.Lib.Extensions.Dto;

public static class MiscExtensions
{
    extension<T>(IEnumerable<IdName<T>> idNames)
    {
        public IEnumerable<IdNameDto<T>> ToDtos()
        {
            return idNames.Select(item => item.ToDto());
        }
    }

    extension<T>(IdName<T> idName)
    {
        public IdNameDto<T> ToDto()
        {
            return new IdNameDto<T>
            {
                Id = idName.Id,
                Name = idName.Name
            };
        }
    }
}