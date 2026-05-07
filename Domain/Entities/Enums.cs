namespace Domain.Entities
{
    public enum DriverStatus
    {
        Free = 1,
        Busy = 2,
        Offline = 3,
        Suspended = 4
    }

    public enum OrderStatus
    {
        New = 1,
        InWork = 2,
        Canceled = 3,
        Finished = 4
    }

    public enum TransactionType
    {
        OrderPaymentByClient = 1,
        DriverPayment = 2,
        DriverFine = 3
    }
}
