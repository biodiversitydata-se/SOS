using FluentValidation;

namespace SOS.DataStewardship.Api.Filters;

public class ValidatorFilter<T> : IEndpointFilter where T : class
{
    private readonly IValidator<T> _validator;

    public ValidatorFilter(IValidator<T> validator)
    {
        _validator = validator;
    }

    public async ValueTask<object> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var validatable = context.Arguments
            .FirstOrDefault(v => v.GetType() == typeof(T)) as T;        

        if (validatable is null)
        {            
            return Results.ValidationProblem(new Dictionary<string, string[]>());
        }

        var validationResult = await _validator.ValidateAsync(validatable);

        if (!validationResult.IsValid)
        {            
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        return await next(context);
    }
}