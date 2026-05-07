namespace Domain.Entities
{
    public class Transaction
    {
        public int Id { get; set; }
        public required TransactionType TransactionType { get; set; }
        public int DriverId { get; set; }
        public int ClientId { get; set; }
        public required decimal Amount { get; set; }
        public required string Comment { get; set; }
        public DateTime Timestamp { get; init; }

        public Transaction() { }
        public Transaction(TransactionType transactionType, int driverId, int clientId, decimal amount, string comment, int id = 0)
        {
            TransactionType = transactionType;
            DriverId = driverId;
            ClientId = clientId;
            Amount = amount;
            Comment = comment;
            Id = id;
        }

        public Transaction(int id,TransactionType transactionType, int driverId, int clientId, decimal amount, string comment, DateTime timestamp)
        {
            Id = id;
            TransactionType = transactionType;
            DriverId = driverId;
            ClientId = clientId;
            Amount = amount;
            Comment = comment;
            Timestamp = timestamp;
        }
    }
}
