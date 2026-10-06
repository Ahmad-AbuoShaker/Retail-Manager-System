using System;

namespace Domain.Services;

public static class TaxCalculator
{
    public static decimal CalculateTax(decimal amount, decimal rate)
    {
        if (amount < 0)
        {
            throw new ArgumentException("Amount cannot be negative.", nameof(amount));
        }

        if (rate < 0 || rate > 1)
        {
            throw new ArgumentException("Tax rate must be between 0 and 1.", nameof(rate));
        }

        return Math.Round(amount * rate, 2, MidpointRounding.AwayFromZero);
    }
}