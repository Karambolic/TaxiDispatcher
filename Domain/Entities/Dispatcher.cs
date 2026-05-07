namespace Domain.Entities
{
    public class Dispatcher : Person
    {
        public Dispatcher() { }

        public Dispatcher(string firstName, string lastName, string phoneNumber, int id = 0)
            : base(firstName, lastName, phoneNumber, id) { }
    }
}