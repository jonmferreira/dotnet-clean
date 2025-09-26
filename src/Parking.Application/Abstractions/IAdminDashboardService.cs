using Parking.Application.Dtos;

namespace Parking.Application.Abstractions;

public interface IAdminDashboardService
{
    Task<AdminDashboardDto> GetMetricsAsync(int year, int month, CancellationToken cancellationToken = default);

    Task<MonthlyTargetDto> SetMonthlyTargetAsync(int year, int month, int targetEntries, CancellationToken cancellationToken = default);
}
