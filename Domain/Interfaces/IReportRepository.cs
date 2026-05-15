using Domain.DTO;

namespace Domain.Interfaces;

public interface IReportRepository
{
    List<AutoDriverReport> GetFleetReport();
    List<TariffAvgPriceReport> GetTariffStatus();
    string? GetTopDispatcher();
    List<ClientMaxOrderReport> GetMaxOrdersPerClient();
    List<IdleDriverReport> GetIdleDrivers();
    List<TariffStatusReport> GetTariffUsageStatus();
}