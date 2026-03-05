using API.Models;

namespace API.Services;

public interface IReadingService
{
    Task RegisterReadingAsync(ReadingPayload payload);
    Task<IEnumerable<WeightReading>> GetAllReadingsAsync();
    Task<WeightReading?> GetReadingByIdAsync(int id);
    Task<WeightReading?> GetLatestReadingByMacAsync(string macAddress);
    Task<IEnumerable<WeightReading>> GetReadingHistoryByMacAsync(string macAddress, int days);
    Task<WeightReading?> UpdateReadingAsync(int id, WeightReading reading);
    Task<bool> DeleteReadingAsync(int id);
}