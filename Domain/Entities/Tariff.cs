namespace Domain.Entities
{
    public class Tariff
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal PricePerKm { get; set; }

        public Tariff() { }
        public Tariff(string name, decimal pricePerKm, int id = 0)
        {
            Name = name;
            PricePerKm = pricePerKm;
            Id = id;
        }
    }
}
