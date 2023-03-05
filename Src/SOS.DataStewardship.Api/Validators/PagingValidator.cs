using FluentValidation;
using SOS.DataStewardship.Api.Models;

namespace SOS.DataStewardship.Api.Validators
{
    public class PagingValidator : AbstractValidator<PagingParameters>
    {
        public PagingValidator()
        {
            RuleFor(m => m.Skip)
                .GreaterThanOrEqualTo(0)                
                .WithMessage("Skip must be greater than or equal to 0");
            RuleFor(m => m.Skip)
                .LessThanOrEqualTo(100000)
                .WithMessage("Skip must be less than or equal to 100 000");

            RuleFor(m => m.Take)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Take must be greater than or equal to 0");
            RuleFor(m => m.Take)
                .LessThanOrEqualTo(1000)
                .WithMessage("Take must be less than or equal to 1 000");
        }
    }
}