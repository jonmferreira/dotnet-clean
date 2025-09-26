using Parking.Domain.Services;

namespace Parking.UnitTests.Services;

public class ParkingFeeCalculatorTests
{
    private readonly ParkingFeeCalculator _calculator = new();

    [Fact]
    public void CalculateFee_WhenDurationIsZero_ReturnsZero()
    {
        var amount = _calculator.CalculateFee(TimeSpan.Zero);
        Assert.Equal(0m, amount);
    }

    [Theory]
    [InlineData(10, 5.00)]
    [InlineData(25, 13.00)]
    [InlineData(60, 25.00)]
    [InlineData(70, 45.00)]
    [InlineData(180, 77.00)]
    [InlineData(600, 177.00)]
    public void CalculateFee_ForVariousDurations_ReturnsExpectedAmount(int minutes, decimal expected)
    {
        var amount = _calculator.CalculateFee(TimeSpan.FromMinutes(minutes));
        Assert.Equal(expected, amount);
    }
}
