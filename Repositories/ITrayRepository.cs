using API.Models;

namespace API.Repositories;

public interface ITrayRepository
{
    Task<Tray> CreateAsync(Tray tray);
    Task<Tray?> GetByIdAsync(int id);
    Task<Tray?> GetByMacAddressAsync(string macAddress);
    Task<IEnumerable<Tray>> GetAllAsync(bool includeInactive = false);
    Task<IEnumerable<Tray>> GetByCartIdAsync(int cartId);
    Task<IEnumerable<Tray>> GetUnassignedTraysAsync();
    Task<Tray?> UpdateAsync(int id, Tray tray);
    Task<bool> DeleteAsync(int id);
    Task<bool> MacAddressExistsAsync(string macAddress);
}
