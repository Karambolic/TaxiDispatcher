namespace TaxiDispatcher.Domain.Entities
{
    public class Address
    {
        public int Id { get; set; }
        public string StreetName { get; set; }
        public string StreetNumber { get; set; }

        public Address() { }
        public Address(string streetName, string streetNumber, int id = 0)
        {
            StreetName = streetName;
            StreetNumber = streetNumber; 
            Id = id;
        }
    }
}
