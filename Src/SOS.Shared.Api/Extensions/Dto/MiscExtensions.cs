using SOS.Lib.Models.Search.Result;
using SOS.Shared.Api.Dtos;

namespace SOS.Shared.Api.Extensions.Dto;

public static class MiscExtensions
{
    public static IEnumerable<IdNameDto<T>> ToDtos<T>(this IEnumerable<IdName<T>> idNames)
    {
        return idNames.Select(item => item.ToDto());
    }

    public static IdNameDto<T> ToDto<T>(this IdName<T> idName)
    {
        return new IdNameDto<T>
        {
            Id = idName.Id,
            Name = idName.Name
        };
    }
}