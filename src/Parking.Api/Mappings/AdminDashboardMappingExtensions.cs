using System.Linq;
using Parking.Api.Models.Responses;
using Parking.Application.Dtos;

namespace Parking.Api.Mappings;

public static class AdminDashboardMappingExtensions
{
    public static AdminDashboardResponse ToResponse(this AdminDashboardDto dto)
    {
        return new AdminDashboardResponse
        {
            WeeklyEntries = dto.WeeklyEntries.Select(ToResponse).ToArray(),
            EntriesByDayOfWeek = dto.EntriesByDayOfWeek.Select(ToResponse).ToArray(),
            MonthlyEntries = dto.MonthlyEntries.Select(ToResponse).ToArray(),
            YearlyEntries = dto.YearlyEntries.Select(ToResponse).ToArray(),
            TrafficByPeriod = dto.TrafficByPeriod.Select(ToResponse).ToArray(),
            MonthlyTarget = dto.MonthlyTarget.ToResponse(),
        };
    }

    public static DashboardChartPointResponse ToResponse(this DashboardChartPointDto dto)
    {
        return new DashboardChartPointResponse
        {
            Name = dto.Name,
            Value = dto.Value,
        };
    }

    public static DashboardPeriodResponse ToResponse(this DashboardPeriodDto dto)
    {
        return new DashboardPeriodResponse
        {
            Name = dto.Name,
            Entries = dto.Entries,
        };
    }

    public static MonthlyTargetResponse ToResponse(this MonthlyTargetDto dto)
    {
        return new MonthlyTargetResponse
        {
            Year = dto.Year,
            Month = dto.Month,
            TargetEntries = dto.TargetEntries,
            ActualEntries = dto.ActualEntries,
            Difference = dto.Difference,
        };
    }
}
