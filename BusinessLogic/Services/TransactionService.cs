using Domain.Entities;
using Infrastructure;

namespace BusinessLogic.Services;

public class TransactionService(UnitOfWork unitOfWork)
{
    /// <summary>
    /// Get the full history of transactions, ordered by time created (in descending order - from newest to oldest)
    /// </summary>
    public List<Transaction> GetFullHistory()
    {
        return unitOfWork.Transactions.GetAll()
            .OrderByDescending(t => t.Timestamp)
            .ToList();
    }

    /// <summary>
    /// Get the full history of transactions for a specific client
    /// </summary>
    public List<Transaction> GetClientTransactionsFullHistory(int clientId)
    {
        return unitOfWork.Transactions.GetAll()
            .Where(t => t.ClientId == clientId)
            .OrderByDescending(t => t.Timestamp)
            .ToList();
    }

    /// <summary>
    /// Get the earnings of a specific driver
    /// </summary>
    public decimal GetDriverEarnings(int driverId)
    {
        return unitOfWork.Transactions.GetAll()
            .Where(t => t.DriverId == driverId)
            .Sum(t => t.Amount);
    }

    /// <summary>
    /// Get the total amount spent by a specific client
    /// </summary>
    public decimal GetClientTotalSpent(int clientId)
    {
        return unitOfWork.Transactions.GetAll()
            .Where(t => t.ClientId == clientId)
            .Sum(t => t.Amount);
    }

    /// <summary>
    /// Get the total sum of all transactions within a specific time range
    /// </summary>
    public decimal GetTotalTurnover(DateTime start, DateTime end)
    {
        return unitOfWork.Transactions.GetAll()
            .Where(t => t.Timestamp >= start && t.Timestamp <= end)
            .Sum(t => t.Amount);
    }
}