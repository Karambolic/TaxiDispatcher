namespace Domain.Entities
{
    public class Order
    {
        public int Id {  get; set; }
        public Client Client {  get; set; }
        public Dispatcher Dispatcher { get; set; }
        public Driver Driver { get; set; }
        public Tariff Tariff { get; set; }
        public Address AddressStart { get; set; }
        public Address AddressEnd { get; set; }
        public DateTime CreatedAt { get; set; }
        public OrderStatus Status { get; set; }
        public string Comment { get; set; }
        public int PassengerCount { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public float FinalDistanceKm { get; set; }
        public decimal FinalPrice { get; set; }
        public Order() { }

        public Order(Client client, Dispatcher dispatcher, Driver driver, Tariff tariff,Address addressStart, 
            Address addressEnd, string comment, int passengerCount, float finalDistanceKm,
            decimal finalPrice, OrderStatus status = OrderStatus.New, int id = 0,
            DateTime? startedAt = null, DateTime? finishedAt = null)
        {  
            this.Id = id;
            this.Client = client;
            this.Dispatcher = dispatcher;
            this.Driver = driver;
            this.Tariff = tariff;
            this.AddressStart = addressStart;
            this.AddressEnd = addressEnd;
            this.CreatedAt = DateTime.UtcNow;
            this.Status = status;
            this.Comment = comment;
            this.PassengerCount = passengerCount;
            this.StartedAt = startedAt;
            this.FinishedAt = finishedAt;
            this.FinalDistanceKm = finalDistanceKm;
            this.FinalPrice = finalPrice;        
        }
    }
}
