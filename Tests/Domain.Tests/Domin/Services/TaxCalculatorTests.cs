using Domain.Services;
using Xunit;

namespace Tests.Domin.Services;

public class TaxCalculatorTests
{
    [Fact]
    public void CalculateTax_NormalRate_ReturnsCorrectTax()
    {
        var tax = TaxCalculator.CalculateTax(100m, 0.0875m);

        Assert.Equal(8.75m, tax);
    }

    [Fact]
    public void CalculateTax_Midpoint_RoundsUp()
    {
        var tax = TaxCalculator.CalculateTax(0.50m, 0.01m);

        Assert.Equal(0.01m, tax);
    }

    [Fact]
    public void CalculateTax_RateAsPercentage_Throws()
    {
        Assert.Throws<ArgumentException>(() => TaxCalculator.CalculateTax(100m, 8.75m));
    }

    [Fact]
    public void CalculateTax_NegativeAmount_Throws()
    {
        Assert.Throws<ArgumentException>(() => TaxCalculator.CalculateTax(-10m, 0.0875m));
    }

    [Fact]
    public void CalculateTax_ZeroAmount_ReturnsZero()
    {
        var tax = TaxCalculator.CalculateTax(0m, 0.0875m);

        Assert.Equal(0m, tax);
    }

    [Fact]
    public void CalculateTax_ZeroRate_ReturnsZero()
    {
        var tax = TaxCalculator.CalculateTax(100m, 0m);

        Assert.Equal(0m, tax);
    }

    [Fact]
    public void CalculateTax_MaxRate_ReturnsFullAmount()
    {
        var tax = TaxCalculator.CalculateTax(100m, 1m);

        Assert.Equal(100m, tax);
    }
}