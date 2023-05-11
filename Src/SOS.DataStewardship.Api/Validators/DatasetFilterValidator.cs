using FluentValidation;
using FluentValidation.Results;
using MimeKit.Cryptography;
using SOS.DataStewardship.Api.Contracts.Models;

namespace SOS.DataStewardship.Api.Validators
{
    public class DatasetFilterValidator : AbstractValidator<DatasetFilter>
    {
        public DatasetFilterValidator()
        {                        
            RuleFor(m => m.DateFilter).SetValidator(new DateFilterValidator())
                .When(m => m.DateFilter != null);

            RuleFor(m => m.Area).SetValidator(new GeographicsFilterValidator())
                .When(m => m.Area != null);
        }        
    }
}