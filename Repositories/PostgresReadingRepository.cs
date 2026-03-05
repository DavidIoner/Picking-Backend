using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Models;
namespace API.Repositories;

public class PostgresReadingRepository : IReadingRepository
{
    private readonly AppDbContext _context;

    public PostgresReadingRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddReadingAsync(WeightReading reading)
    {
        _context.WeightReadings.Add(reading);
        await _context.SaveChangesAsync();
        Console.WriteLine($"---> Leitura adicionada com {reading.leituras.Count} valores. ID: {reading.id}");
    }

    public async Task<IEnumerable<WeightReading>> GetAllAsync()
    {
        Console.WriteLine($"---> Buscando todas as leituras no banco PostgreSQL");
        return await _context.WeightReadings
            .OrderByDescending(r => r.timestamp_leitura)
            .ToListAsync();
    }

    public async Task<WeightReading?> GetByIdAsync(int id)
    {
        Console.WriteLine($"---> Buscando leitura por ID: {id}");
        return await _context.WeightReadings.FindAsync(id);
    }

    public async Task<WeightReading?> GetLatestByMacAsync(string macAddress)
    {
        Console.WriteLine($"---> Buscando última leitura para MAC: {macAddress}");
        return await _context.WeightReadings
            .Where(r => r.mac_address == macAddress)
            .OrderByDescending(r => r.timestamp_leitura)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<WeightReading>> GetHistoryByMacAsync(string macAddress, int days)
    {
        Console.WriteLine($"---> Buscando histórico de {days} dias para MAC: {macAddress}");
        var startDate = DateTime.UtcNow.AddDays(-days);
        
        // Busca as leituras agrupadas por dia e pega a última de cada dia
        var readings = await _context.WeightReadings
            .Where(r => r.mac_address == macAddress && r.timestamp_leitura >= startDate)
            .OrderBy(r => r.timestamp_leitura)
            .ToListAsync();

        // Agrupa por dia e pega a última leitura de cada dia
        var dailyReadings = readings
            .GroupBy(r => r.timestamp_leitura.Date)
            .Select(g => g.OrderByDescending(r => r.timestamp_leitura).First())
            .OrderBy(r => r.timestamp_leitura)
            .ToList();

        Console.WriteLine($"---> Encontradas {dailyReadings.Count} leituras diárias");
        return dailyReadings;
    }

    public async Task<WeightReading?> UpdateAsync(int id, WeightReading reading)
    {
        var existingReading = await _context.WeightReadings.FindAsync(id);
        if (existingReading == null)
        {
            Console.WriteLine($"---> Leitura com ID {id} não encontrada para atualização");
            return null;
        }

        existingReading.mac_address = reading.mac_address;
        existingReading.leituras = reading.leituras;
        existingReading.timestamp_leitura = reading.timestamp_leitura;

        await _context.SaveChangesAsync();
        Console.WriteLine($"---> Leitura com ID {id} atualizada com sucesso");
        return existingReading;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var reading = await _context.WeightReadings.FindAsync(id);
        if (reading == null)
        {
            Console.WriteLine($"---> Leitura com ID {id} não encontrada para exclusão");
            return false;
        }

        _context.WeightReadings.Remove(reading);
        await _context.SaveChangesAsync();
        Console.WriteLine($"---> Leitura com ID {id} removida");
        return true;
    }
}
