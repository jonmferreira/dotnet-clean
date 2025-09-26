using System.Globalization;
using System.Linq;
using Parking.Application.Abstractions;
using Parking.Application.Dtos;
using Parking.Domain.Entities;
using Parking.Domain.Repositories;

namespace Parking.Application.Services;

public sealed class AdminDashboardService : IAdminDashboardService
{
    private static readonly string[] PeriodNames = ["Manhã", "Tarde", "Noite"];

    private readonly IParkingTicketRepository _ticketRepository;
    private readonly IMonthlyTargetRepository _monthlyTargetRepository;

    public AdminDashboardService(IParkingTicketRepository ticketRepository, IMonthlyTargetRepository monthlyTargetRepository)
    {
        _ticketRepository = ticketRepository ?? throw new ArgumentNullException(nameof(ticketRepository));
        _monthlyTargetRepository = monthlyTargetRepository ?? throw new ArgumentNullException(nameof(monthlyTargetRepository));
    }

    public async Task<AdminDashboardDto> GetMetricsAsync(int year, int month, CancellationToken cancellationToken = default)
    {
        MonthlyTarget.ValidateYear(year);
        MonthlyTarget.ValidateMonth(month);

        var monthRange = GetMonthRange(year, month);
        var yearRange = GetYearRange(year);

        var monthTicketsTask = _ticketRepository.GetByPeriodAsync(monthRange.start, monthRange.end, cancellationToken);
        var yearTicketsTask = _ticketRepository.GetByPeriodAsync(yearRange.start, yearRange.end, cancellationToken);
        var allTicketsTask = _ticketRepository.GetAllAsync(cancellationToken);

        await Task.WhenAll(monthTicketsTask, yearTicketsTask, allTicketsTask);

        var monthTickets = monthTicketsTask.Result;
        var yearTickets = yearTicketsTask.Result;
        var allTickets = allTicketsTask.Result;

        var weeklyEntries = BuildWeeklyEntries(monthTickets);
        var dayOfWeekEntries = BuildDayOfWeekEntries(monthTickets);
        var monthlyEntries = BuildMonthlyEntries(yearTickets);
        var yearlyEntries = BuildYearlyEntries(allTickets);
        var periodEntries = BuildPeriodEntries(monthTickets);

        var target = await _monthlyTargetRepository.GetAsync(year, month, cancellationToken);
        var monthlyTargetDto = CreateMonthlyTargetDto(year, month, target?.TargetEntries ?? 0, monthTickets.Count);

        return new AdminDashboardDto
        {
            WeeklyEntries = weeklyEntries,
            EntriesByDayOfWeek = dayOfWeekEntries,
            MonthlyEntries = monthlyEntries,
            YearlyEntries = yearlyEntries,
            TrafficByPeriod = periodEntries,
            MonthlyTarget = monthlyTargetDto,
        };
    }

    public async Task<MonthlyTargetDto> SetMonthlyTargetAsync(int year, int month, int targetEntries, CancellationToken cancellationToken = default)
    {
        MonthlyTarget.ValidateYear(year);
        MonthlyTarget.ValidateMonth(month);

        if (targetEntries < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(targetEntries), "Target entries must be non-negative.");
        }

        var monthRange = GetMonthRange(year, month);
        var monthTickets = await _ticketRepository.GetByPeriodAsync(monthRange.start, monthRange.end, cancellationToken);

        var target = await _monthlyTargetRepository.UpsertAsync(year, month, targetEntries, cancellationToken);
        return CreateMonthlyTargetDto(target.Year, target.Month, target.TargetEntries, monthTickets.Count);
    }

    private static (DateTimeOffset start, DateTimeOffset end) GetMonthRange(int year, int month)
    {
        var start = new DateTimeOffset(year, month, 1, 0, 0, 0, TimeSpan.Zero);
        var end = start.AddMonths(1);
        return (start, end);
    }

    private static (DateTimeOffset start, DateTimeOffset end) GetYearRange(int year)
    {
        var start = new DateTimeOffset(year, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var end = start.AddYears(1);
        return (start, end);
    }

    private static IReadOnlyCollection<DashboardChartPointDto> BuildWeeklyEntries(IEnumerable<ParkingTicket> tickets)
    {
        var culture = CultureInfo.GetCultureInfo("pt-BR");
        return tickets
            .GroupBy(ticket => GetWeekStart(ticket.EntryAt))
            .OrderBy(group => group.Key)
            .Select(group =>
            {
                var start = group.Key;
                var end = start.AddDays(6);
                var label = $"{start.ToString("dd/MM", culture)} - {end.ToString("dd/MM", culture)}";
                return new DashboardChartPointDto { Name = label, Value = group.Count() };
            })
            .ToArray();
    }

    private static IReadOnlyCollection<DashboardChartPointDto> BuildDayOfWeekEntries(IEnumerable<ParkingTicket> tickets)
    {
        var culture = CultureInfo.GetCultureInfo("pt-BR");
        var groups = tickets
            .GroupBy(ticket => ticket.EntryAt.DayOfWeek)
            .ToDictionary(group => group.Key, group => group.Count());

        var orderedDays = new[]
        {
            DayOfWeek.Monday,
            DayOfWeek.Tuesday,
            DayOfWeek.Wednesday,
            DayOfWeek.Thursday,
            DayOfWeek.Friday,
            DayOfWeek.Saturday,
            DayOfWeek.Sunday,
        };

        return orderedDays
            .Select(day => new DashboardChartPointDto
            {
                Name = culture.DateTimeFormat.GetDayName(day),
                Value = groups.TryGetValue(day, out var count) ? count : 0,
            })
            .ToArray();
    }

    private static IReadOnlyCollection<DashboardChartPointDto> BuildMonthlyEntries(IEnumerable<ParkingTicket> tickets)
    {
        var culture = CultureInfo.GetCultureInfo("pt-BR");
        var groups = tickets
            .GroupBy(ticket => ticket.EntryAt.Month)
            .ToDictionary(group => group.Key, group => group.Count());

        return Enumerable.Range(1, 12)
            .Select(month => new DashboardChartPointDto
            {
                Name = culture.DateTimeFormat.GetAbbreviatedMonthName(month),
                Value = groups.TryGetValue(month, out var count) ? count : 0,
            })
            .ToArray();
    }

    private static IReadOnlyCollection<DashboardChartPointDto> BuildYearlyEntries(IEnumerable<ParkingTicket> tickets)
    {
        return tickets
            .GroupBy(ticket => ticket.EntryAt.Year)
            .OrderBy(group => group.Key)
            .Select(group => new DashboardChartPointDto
            {
                Name = group.Key.ToString(CultureInfo.InvariantCulture),
                Value = group.Count(),
            })
            .ToArray();
    }

    private static IReadOnlyCollection<DashboardPeriodDto> BuildPeriodEntries(IEnumerable<ParkingTicket> tickets)
    {
        var groups = tickets
            .GroupBy(ticket => GetPeriod(ticket.EntryAt))
            .ToDictionary(group => group.Key, group => group.Count());

        return PeriodNames
            .Select(period => new DashboardPeriodDto
            {
                Name = period,
                Entries = groups.TryGetValue(period, out var count) ? count : 0,
            })
            .ToArray();
    }

    private static string GetPeriod(DateTimeOffset entryAt)
    {
        var localTime = entryAt.ToLocalTime();
        var hour = localTime.Hour;

        return hour switch
        {
            >= 6 and < 12 => PeriodNames[0],
            >= 12 and < 18 => PeriodNames[1],
            _ => PeriodNames[2],
        };
    }

    private static DateOnly GetWeekStart(DateTimeOffset date)
    {
        var dateOnly = DateOnly.FromDateTime(date.Date);
        var dayOfWeek = (int)dateOnly.DayOfWeek;
        var offset = dayOfWeek == 0 ? 6 : dayOfWeek - 1;
        return dateOnly.AddDays(-offset);
    }

    private static MonthlyTargetDto CreateMonthlyTargetDto(int year, int month, int targetEntries, int actualEntries)
    {
        return new MonthlyTargetDto
        {
            Year = year,
            Month = month,
            TargetEntries = targetEntries,
            ActualEntries = actualEntries,
        };
    }
}
