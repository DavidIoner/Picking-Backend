using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Models;

namespace API.Repositories;

public class PostgresCartRepository : ICartRepository
{
    private readonly AppDbContext _context;

    public PostgresCartRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Cart> CreateAsync(Cart cart)
    {
        cart.data_criacao = DateTime.UtcNow;
        _context.Carts.Add(cart);
        await _context.SaveChangesAsync();
        Console.WriteLine($"---> Carrinho criado: {cart.nome}");
        return cart;
    }

    public async Task<IEnumerable<Cart>> GetAllAsync(bool includeInactive = false)
    {
        var query = _context.Carts.Include(c => c.Trays).AsQueryable();
        
        if (!includeInactive)
        {
            query = query.Where(c => c.ativo);
        }

        return await query.OrderByDescending(c => c.data_criacao).ToListAsync();
    }

    public async Task<Cart?> GetByIdAsync(int id)
    {
        return await _context.Carts
            .Include(c => c.Trays)
            .FirstOrDefaultAsync(c => c.id == id);
    }

    public async Task<Cart?> UpdateAsync(int id, Cart cart)
    {
        var existingCart = await _context.Carts.FindAsync(id);
        if (existingCart == null)
        {
            Console.WriteLine($"---> Carrinho com ID {id} não encontrado");
            return null;
        }

        existingCart.nome = cart.nome;
        existingCart.descricao = cart.descricao;
        existingCart.ativo = cart.ativo;
        existingCart.data_atualizacao = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        Console.WriteLine($"---> Carrinho {id} atualizado");
        return existingCart;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var cart = await _context.Carts.FindAsync(id);
        if (cart == null)
        {
            Console.WriteLine($"---> Carrinho com ID {id} não encontrado");
            return false;
        }

        _context.Carts.Remove(cart);
        await _context.SaveChangesAsync();
        Console.WriteLine($"---> Carrinho {id} removido");
        return true;
    }
}
