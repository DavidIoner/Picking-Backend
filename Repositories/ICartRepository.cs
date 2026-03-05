using API.Models;

namespace API.Repositories;

public interface ICartRepository
{
    Task<Cart> CreateAsync(Cart cart);
    Task<IEnumerable<Cart>> GetAllAsync(bool includeInactive = false);
    Task<Cart?> GetByIdAsync(int id);
    Task<Cart?> UpdateAsync(int id, Cart cart);
    Task<bool> DeleteAsync(int id);
}
