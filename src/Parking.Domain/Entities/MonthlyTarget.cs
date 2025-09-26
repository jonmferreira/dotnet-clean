namespace Parking.Domain.Entities;

public class MonthlyTarget
{
    private MonthlyTarget()
    {
        // EF Core constructor
    }

    public MonthlyTarget(Guid id, int year, int month, int targetEntries)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Id must not be empty.", nameof(id));
        }

        ValidateYear(year);
        ValidateMonth(month);
        ValidateTarget(targetEntries);

        Id = id;
        Year = year;
        Month = month;
        TargetEntries = targetEntries;
    }

    public Guid Id { get; private set; }

    public int Year { get; private set; }

    public int Month { get; private set; }

    public int TargetEntries { get; private set; }

    public void UpdateTarget(int targetEntries)
    {
        ValidateTarget(targetEntries);
        TargetEntries = targetEntries;
    }

    public static void ValidateYear(int year)
    {
        if (year < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(year), "Year must be greater than zero.");
        }
    }

    public static void ValidateMonth(int month)
    {
        if (month is < 1 or > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(month), "Month must be between 1 and 12.");
        }
    }

    private static void ValidateTarget(int targetEntries)
    {
        if (targetEntries < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(targetEntries), "Target entries must be non-negative.");
        }
    }
}
