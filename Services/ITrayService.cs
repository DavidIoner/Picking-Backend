using API.Models;

namespace API.Services;

public interface ITrayService
{
    Task<Tray> CreateTrayAsync(CreateTrayPayload payload);
    Task<Tray?> UpdateTrayAsync(int id, UpdateTrayPayload payload);
    Task<Tray?> GetTrayByIdAsync(int id);
    Task<Tray?> GetTrayByMacAddressAsync(string macAddress);
    Task<IEnumerable<Tray>> GetTraysByCartIdAsync(int cartId);
    Task<IEnumerable<Tray>> GetUnassignedTraysAsync();
    Task<IEnumerable<Tray>> GetAllTraysAsync(bool includeInactive = false);
    Task<bool> AssignTrayToCartAsync(int trayId, int cartId);
    Task<bool> UnassignTrayFromCartAsync(int trayId);
    Task<bool> DeleteTrayAsync(int id);
    Task<bool> ValidateTrayConfiguration(List<TrayBlock> blocks);
    
    [Obsolete("Use CreateTrayAsync or UpdateTrayAsync instead")]
    Task<Tray> ConfigureTrayAsync(TrayConfigPayload payload);
}
