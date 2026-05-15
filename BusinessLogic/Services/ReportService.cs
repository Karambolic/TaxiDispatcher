using Domain.DTO;
using Domain.Entities;
using Infrastructure;
using Infrastructure.Repositories;

namespace BusinessLogic.Services;

public class ReportService(UnitOfWork uow)
{
    public List<AutoDriverReport> GetFleetAnalytics()
        => ((ReportRepository)uow.Reports).GetFleetReport();

    public List<Client> GetMarketingClients(string providerMask)
        => ((ClientRepository)uow.Clients).GetClientByPhoneMask(providerMask);

    public string GetTopDispatcher()
    {
        var leader = ((ReportRepository)uow.Reports).GetTopDispatcher();
        return leader != null ? leader : "No data available.";
    }

    public string GetTotalClientsCount()
    {
        int clients = ((ClientRepository)uow.Clients).GetTotalClientCount();
        return $"Total clients registered: {clients}";
    }

    public List<OrderPeriodReport> GetPeriodReport(DateTime start, DateTime end)
        => ((OrderRepository)uow.Orders).GetOrdersByPeriod(start, end);

    public List<TariffAvgPriceReport> GetTariffPerformance()
        => ((ReportRepository)uow.Reports).GetTariffStatus();

    public List<ClientMaxOrderReport> GetHighValueOrders()
        => ((ReportRepository)uow.Reports).GetMaxOrdersPerClient();

    public List<IdleDriverReport> GetDriversOnStandby()
        => ((ReportRepository)uow.Reports).GetIdleDrivers();

    public List<TariffStatusReport> GetTariffUsage()
        => ((ReportRepository)uow.Reports).GetTariffUsageStatus();
}