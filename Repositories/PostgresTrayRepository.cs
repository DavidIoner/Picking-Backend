using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Models;

namespace API.Repositories;

public class PostgresTrayRepository : ITrayRepository
{
    private readonly AppDbContext _context;

    public PostgresTrayRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Tray> CreateAsync(Tray tray)
    {
        tray.data_criacao = DateTime.UtcNow;
        _context.Trays.Add(tray);
        await _context.SaveChangesAsync();
        Console.WriteLine($"---> Bandeja criada: {tray.mac_address}");
        return tray;
    }

    public async Task<Tray?> GetByIdAsync(int id)
    {
        return await _context.Trays
            .Include(t => t.Cart)
            .FirstOrDefaultAsync(t => t.id == id);
    }

    public async Task<Tray?> GetByMacAddressAsync(string macAddress)
    {
        return await _context.Trays
            .Include(t => t.Cart)
            .FirstOrDefaultAsync(t => t.mac_address == macAddress);
    }

    public async Task<IEnumerable<Tray>> GetAllAsync(bool includeInactive = false)
    {
        var query = _context.Trays.Include(t => t.Cart).AsQueryable();
        
        if (!includeInactive)
        {
            query = query.Where(t => t.ativo);
        }

        return await query
            .OrderByDescending(t => t.data_criacao)
            .ToListAsync();
    }

    public async Task<IEnumerable<Tray>> GetByCartIdAsync(int cartId)
    {
        return await _context.Trays
            .Include(t => t.Cart)
            .Where(t => t.carrinho_id == cartId)
            .OrderBy(t => t.nome)
            .ToListAsync();
    }

    public async Task<IEnumerable<Tray>> GetUnassignedTraysAsync()
    {
        return await _context.Trays
            .Where(t => t.carrinho_id == null && t.ativo)
            .OrderByDescending(t => t.data_criacao)
            .ToListAsync();
    }

    public async Task<Tray?> UpdateAsync(int id, Tray tray)
    {
        var existingTray = await _context.Trays.FindAsync(id);
        if (existingTray == null)
        {
            Console.WriteLine($"---> Bandeja com ID {id} não encontrada");
            return null;
        }

        existingTray.nome = tray.nome;
        existingTray.descricao = tray.descricao;
        existingTray.carrinho_id = tray.carrinho_id;
        existingTray.blocos = tray.blocos;
        existingTray.ativo = tray.ativo;
        existingTray.data_atualizacao = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        Console.WriteLine($"---> Bandeja {id} atualizada");
        return existingTray;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var tray = await _context.Trays.FindAsync(id);
        if (tray == null)
        {
            Console.WriteLine($"---> Bandeja com ID {id} não encontrada");
            return false;
        }

        _context.Trays.Remove(tray);
        await _context.SaveChangesAsync();
        Console.WriteLine($"---> Bandeja {id} removida");
        return true;
    }

    public async Task<bool> MacAddressExistsAsync(string macAddress)
    {
        return await _context.Trays.AnyAsync(t => t.mac_address == macAddress);
    }
}
