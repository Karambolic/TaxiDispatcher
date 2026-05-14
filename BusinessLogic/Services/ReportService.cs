using Domain.DTO;
using Domain.Entities;
using Infrastructure.Repositories;

namespace BusinessLogic.Services;

public class ReportService(ReportRepository reportRepo, ClientRepository clientRepo, OrderRepository orderRepo)
{
    // Pass-through for the fleet report
    public List<AutoDriverReport> GetFleetAnalytics() => reportRepo.GetFleetReport();

    // Pass-through for report of clients having phone numbers matching a specific provider mask
    public List<Client> GetMarketingClients(string providerMask) => clientRepo.GetClientsByPhoneMask(providerMask);

    // Pass-through for most effective dispatcher statistics
    public string GetTopDispatcher()
    {
        var leader = reportRepo.GetTopDispatcher();
        return leader != null ? leader : "No data available.";
    }

    public string GetTotalClientsCount()
    {
        int clients = clientRepo.GetTotalClientsCount();
        return $"Total clients registered: {clients}";
    }

    public List<OrderPeriodReport> GetPeriodReport(DateTime start, DateTime end) => orderRepo.GetOrdersByPeriod(start, end);
    public List<TariffAvgPriceReport> GetTariffPerformance() => reportRepo.GetTariffStats();
    public List<ClientMaxOrderReport> GetHighValueOrders() => reportRepo.GetMaxOrdersPerClient();
    public List<IdleDriverReport> GetDriversOnStandby() => reportRepo.GetIdleDrivers();
    public List<TariffStatusReport> GetTariffUsage() => reportRepo.GetTariffUsageStatus();
}
