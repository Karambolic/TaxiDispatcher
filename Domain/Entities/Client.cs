namespace TaxiDispatcher.Domain.Entities
{
    public class Client : Person
    {
        public Client() { }
        public Client(string firstName, string lastName, string phoneNumber, int id = 0)
            : base(firstName, lastName, phoneNumber, id){ }
    }
}
