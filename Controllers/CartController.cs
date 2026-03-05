using Microsoft.AspNetCore.Mvc;
using API.Models;
using API.Services;

namespace API.Controllers;

[ApiController]
[Route("api/carts")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    /// <summary>
    /// Cadastra um novo carrinho (agrupamento lógico)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateCart([FromBody] CreateCartPayload payload)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(payload.Name))
            {
                return BadRequest("Nome é obrigatório");
            }

            var cart = await _cartService.CreateCartAsync(payload);
            return CreatedAtAction(nameof(GetCartById), new { id = cart.id }, cart);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Lista todos os carrinhos
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllCarts([FromQuery] bool includeInactive = true)
    {
        var carts = await _cartService.GetAllCartsAsync(includeInactive);
        return Ok(carts);
    }

    /// <summary>
    /// Busca um carrinho por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCartById(int id)
    {
        var cart = await _cartService.GetCartByIdAsync(id);
        if (cart == null)
        {
            return NotFound(new { message = $"Carrinho com ID {id} não encontrado" });
        }
        return Ok(cart);
    }

    /// <summary>
    /// Atualiza um carrinho
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCart(int id, [FromBody] UpdateCartPayload payload)
    {
        var cart = await _cartService.UpdateCartAsync(id, payload);
        if (cart == null)
        {
            return NotFound(new { message = $"Carrinho com ID {id} não encontrado" });
        }
        return Ok(cart);
    }

    /// <summary>
    /// Deleta um carrinho
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCart(int id)
    {
        try
        {
            var deleted = await _cartService.DeleteCartAsync(id);
            if (!deleted)
            {
                return NotFound(new { message = $"Carrinho com ID {id} não encontrado" });
            }
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
