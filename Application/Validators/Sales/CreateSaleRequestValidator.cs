using Application.DTOs.Sales;
using FluentValidation;

namespace Application.Validators.Sales;

public class CreateSaleRequestValidator : AbstractValidator<CreateSaleRequest>
{
    public CreateSaleRequestValidator()
    {
        RuleFor(x => x.Items)
            .NotNull().WithMessage("Items are required.")
            .NotEmpty().WithMessage("At least one item is required.");

        RuleForEach(x => x.Items)
            .SetValidator(new CreateSaleItemRequestValidator());
    }
}