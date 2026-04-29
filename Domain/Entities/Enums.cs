namespace TaxiDispatcher.Domain.Entities
{
    public enum DriverStatus
    {
        Free = 0,
        Busy = 1,
        Offline = 2
    }

    public enum OrderStatus
    {
        New = 0,
        InWork = 1,
        Canceled = 2,
        Finished = 3
    }

    public enum TransactionType
    {
        OrderPaymentByClient = 0,
        DriverPayment = 1,
        DriverFine = 2
    }
}
