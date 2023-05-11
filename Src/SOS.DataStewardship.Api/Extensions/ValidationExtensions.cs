using FluentValidation;
using SOS.DataStewardship.Api.Contracts.Models;
using SOS.DataStewardship.Api.Validators;

namespace SOS.DataStewardship.Api.Extensions
{
    public static class ValidationExtensions
    {

        public static async Task<FluentValidation.Results.ValidationResult> ValidateAsync(this PagingParameters pagingParameters)
        {
            return await new PagingValidator().ValidateAsync(pagingParameters);            
        }

        public static async Task<FluentValidation.Results.ValidationResult> ValidateAsync<TValidator, TObject>(this TValidator validator, TObject obj)
            where TValidator : IValidator<TObject>
            where TObject : class
        {
            var validationResult = await validator.ValidateAsync(obj);
            return validationResult;
        }

        public static async ValueTask<object> ValidateAsync<TValidator, TValidatable>(this TValidator validator, 
            TValidatable validatable,
            EndpointFilterInvocationContext context, 
            EndpointFilterDelegate next)
                where TValidator : IValidator<TValidatable>
                where TValidatable : class
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