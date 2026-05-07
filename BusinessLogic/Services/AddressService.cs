using Domain.Entities;
using Infrastructure;

namespace BusinessLogic.Services;

public class AddressService(UnitOfWork unitOfWork)
{
    /// <summary>
    /// Gett all the addresses from the database
    /// </summary>
    public List<Address> GetAllAddresses()
    {
        return unitOfWork.Addresses.GetAll();
    }

    /// <summary>
    /// Find an address by street name and house number
    /// </summary>
    public Address? FindAddress(string streetName, string streetNumber)
    {
        return unitOfWork.Addresses.GetAll().FirstOrDefault(
            a => a.StreetName.Equals(streetName, StringComparison.OrdinalIgnoreCase)
            && a.StreetNumber.Equals(streetNumber, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Get or create a new address
    /// </summary>
    public Address GetOrCreateAddress(string streetName, string streetNumber)
    {
        var existing = FindAddress(streetName, streetNumber);
        if (existing != null) 
            return existing;

        var newAddress = new Address
        {
            StreetName = streetName,
            StreetNumber = streetNumber
        };

        unitOfWork.Addresses.Add(newAddress);
        return newAddress;
    }
}