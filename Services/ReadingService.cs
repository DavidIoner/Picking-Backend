using API.Models;
using API.Repositories;

namespace API.Services;

public class ReadingService : IReadingService
{
    private readonly IReadingRepository _repository;

    public ReadingService(IReadingRepository repository)
    {
        _repository = repository;
    }

    public Task RegisterReadingAsync(ReadingPayload payload)
    {
        // Garante que o timestamp seja tratado como UTC
        var timestamp = payload.Timestamp.Kind == DateTimeKind.Unspecified 
            ? DateTime.SpecifyKind(payload.Timestamp, DateTimeKind.Utc)
            : payload.Timestamp.ToUniversalTime();

        var weightReading = new WeightReading
        {
            // Id será gerado automaticamente pelo banco (auto-increment)
            mac_address = payload.MacAddress,
            timestamp_leitura = timestamp,
            leituras = payload.Readings
        };

        return _repository.AddReadingAsync(weightReading);
    }

    public Task<IEnumerable<WeightReading>> GetAllReadingsAsync()
    {
        return _repository.GetAllAsync();
    }

    public Task<WeightReading?> GetReadingByIdAsync(int id)
    {
        return _repository.GetByIdAsync(id);
    }

    public Task<WeightReading?> GetLatestReadingByMacAsync(string macAddress)
    {
        return _repository.GetLatestByMacAsync(macAddress);
    }

    public Task<IEnumerable<WeightReading>> GetReadingHistoryByMacAsync(string macAddress, int days)
    {
        return _repository.GetHistoryByMacAsync(macAddress, days);
    }

    public Task<WeightReading?> UpdateReadingAsync(int id, WeightReading reading)
    {
        // Preserva o ID original
        reading.id = id;
        return _repository.UpdateAsync(id, reading);
    }

    public Task<bool> DeleteReadingAsync(int id)
    {
        return _repository.DeleteAsync(id);
    }
}