namespace TaxiDispatcher.Domain.Entities
{
    public class Person
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }

        public Person() { }
        public Person(string firstName, string lastName, string phoneNumber, int id = 0)
        {
            this.FirstName = firstName;
            this.LastName = lastName;
            this.PhoneNumber = phoneNumber;
            this.Id = id;
        }
    }
}
