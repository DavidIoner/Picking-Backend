using API.Models;
using API.Repositories;

namespace API.Services;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly ITrayRepository _trayRepository;

    public CartService(ICartRepository cartRepository, ITrayRepository trayRepository)
    {
        _cartRepository = cartRepository;
        _trayRepository = trayRepository;
    }

    public async Task<Cart> CreateCartAsync(CreateCartPayload payload)
    {
        // Validar nome
        if (string.IsNullOrWhiteSpace(payload.Name))
        {
            throw new ArgumentException("Nome é obrigatório");
        }

        var cart = new Cart
        {
            nome = payload.Name.Trim(),
            descricao = payload.Description,
            ativo = true,
            data_criacao = DateTime.UtcNow
        };

        return await _cartRepository.CreateAsync(cart);
    }

    public async Task<IEnumerable<Cart>> GetAllCartsAsync(bool includeInactive = false)
    {
        return await _cartRepository.GetAllAsync(includeInactive);
    }

    public async Task<Cart?> GetCartByIdAsync(int id)
    {
        return await _cartRepository.GetByIdAsync(id);
    }

    public async Task<Cart?> UpdateCartAsync(int id, UpdateCartPayload payload)
    {
        var existingCart = await _cartRepository.GetByIdAsync(id);
        if (existingCart == null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(payload.Name))
        {
            existingCart.nome = payload.Name.Trim();
        }

        if (payload.Description != null)
        {
            existingCart.descricao = payload.Description;
        }

        if (payload.IsActive.HasValue)
        {
            existingCart.ativo = payload.IsActive.Value;
        }

        return await _cartRepository.UpdateAsync(id, existingCart);
    }

    public async Task<bool> DeleteCartAsync(int id)
    {
        // Verificar se o carrinho existe
        var cart = await _cartRepository.GetByIdAsync(id);
        if (cart == null)
        {
            return false;
        }

        // Verificar se há bandejas associadas
        var trays = await _trayRepository.GetByCartIdAsync(id);
        if (trays.Any())
        {
            throw new InvalidOperationException($"Não é possível excluir o carrinho pois ele possui {trays.Count()} bandeja(s) associada(s). Remova as bandejas primeiro.");
        }

        return await _cartRepository.DeleteAsync(id);
    }
}
