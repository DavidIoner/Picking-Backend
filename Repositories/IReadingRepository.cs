using API.Models;

namespace API.Repositories;

public interface IReadingRepository
{
    Task AddReadingAsync(WeightReading reading);
    Task<IEnumerable<WeightReading>> GetAllAsync();
    Task<WeightReading?> GetByIdAsync(int id);
    Task<WeightReading?> GetLatestByMacAsync(string macAddress);
    Task<IEnumerable<WeightReading>> GetHistoryByMacAsync(string macAddress, int days);
    Task<WeightReading?> UpdateAsync(int id, WeightReading reading);
    Task<bool> DeleteAsync(int id);
}