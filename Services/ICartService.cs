using API.Models;

namespace API.Services;

public interface ICartService
{
    Task<Cart> CreateCartAsync(CreateCartPayload payload);
    Task<IEnumerable<Cart>> GetAllCartsAsync(bool includeInactive = false);
    Task<Cart?> GetCartByIdAsync(int id);
    Task<Cart?> UpdateCartAsync(int id, UpdateCartPayload payload);
    Task<bool> DeleteCartAsync(int id);
}
