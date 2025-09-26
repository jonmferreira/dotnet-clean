namespace Parking.Domain.Services;

public interface IParkingFeeCalculator
{
    decimal CalculateFee(TimeSpan duration);
}
