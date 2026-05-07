namespace Domain.Entities
{
    public class Driver : Person
    {
        public DriverStatus Status { get; set; }
        public Driver() : base()
        {
            Status = DriverStatus.Offline;
        }
        public Driver(string firstName, string lastName, string phoneNumber, int id = 0, DriverStatus status = DriverStatus.Offline)
             : base(firstName, lastName, phoneNumber, id)
        {
            Status = status;
        }
    }
}
