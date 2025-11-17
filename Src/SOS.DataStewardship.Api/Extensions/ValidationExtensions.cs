using FluentValidation;
using SOS.DataStewardship.Api.Contracts.Models;
using SOS.DataStewardship.Api.Validators;

namespace SOS.DataStewardship.Api.Extensions;

public static class ValidationExtensions
{

    extension(PagingParameters pagingParameters)
    {
        public async Task<FluentValidation.Results.ValidationResult> ValidateAsync()
        {
            return await new PagingValidator().ValidateAsync(pagingParameters);
        }
    }

    extension<TValidator, TObject>(TValidator validator)
        where TValidator : IValidator<TObject>
        where TObject : class
    {
        public async Task<FluentValidation.Results.ValidationResult> ValidateAsync(TObject obj)
        {
            var validationResult = await validator.ValidateAsync(obj);
            return validationResult;
        }
    }

    extension<TValidator, TValidatable>(TValidator validator)
        where TValidator : IValidator<TValidatable>
        where TValidatable : class
    {
        public async ValueTask<object> ValidateAsync(TValidatable validatable,
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
        {
            if (validatable is null)
            {
                return Results.BadRequest();
            }

            var validationResult = await validator.ValidateAsync(validatable);

            if (!validationResult.IsValid)
            {
                return Results.BadRequest(validationResult.Errors);
            }

            return await next(context);
        }
    }
}