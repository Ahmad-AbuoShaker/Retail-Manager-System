using Application.DTOs.Inventory;
using FluentValidation;

namespace Application.Validators.Inventory;

public class CreateInventoryRequestValidator : AbstractValidator<CreateInventoryRequest>
{
    public CreateInventoryRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("ProductId must be greater than 0.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0.");

        RuleFor(x => x.PurchasePrice)
            .GreaterThanOrEqualTo(0).WithMessage("PurchasePrice must be greater than or equal to 0.");

        RuleFor(x => x.PurchaseDate)
            .LessThanOrEqualTo(DateTime.UtcNow).When(x => x.PurchaseDate.HasValue)
            .WithMessage("PurchaseDate cannot be in the future.");
    }
}