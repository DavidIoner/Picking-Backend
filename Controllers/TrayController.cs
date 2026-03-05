using Microsoft.AspNetCore.Mvc;
using API.Models;
using API.Services;

namespace API.Controllers;

[ApiController]
[Route("api/trays")]
public class TrayController : ControllerBase
{
    private readonly ITrayService _trayService;

    public TrayController(ITrayService trayService)
    {
        _trayService = trayService;
    }

    /// <summary>
    /// Cadastra uma nova bandeja (dispositivo físico)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateTray([FromBody] CreateTrayPayload payload)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(payload.MacAddress))
            {
                return BadRequest("MAC Address é obrigatório");
            }

            var tray = await _trayService.CreateTrayAsync(payload);
            return CreatedAtAction(nameof(GetTrayById), new { id = tray.id }, tray);
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
    /// Atualiza uma bandeja existente
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTray(int id, [FromBody] UpdateTrayPayload payload)
    {
        try
        {
            var tray = await _trayService.UpdateTrayAsync(id, payload);
            if (tray == null)
            {
                return NotFound(new { message = $"Bandeja com ID {id} não encontrada" });
            }
            return Ok(tray);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Lista todas as bandejas
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllTrays([FromQuery] bool includeInactive = true)
    {
        var trays = await _trayService.GetAllTraysAsync(includeInactive);
        return Ok(trays);
    }

    /// <summary>
    /// Busca uma bandeja por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTrayById(int id)
    {
        var tray = await _trayService.GetTrayByIdAsync(id);
        if (tray == null)
        {
            return NotFound(new { message = $"Bandeja com ID {id} não encontrada" });
        }
        return Ok(tray);
    }

    /// <summary>
    /// Busca uma bandeja por MAC Address
    /// </summary>
    [HttpGet("mac/{macAddress}")]
    public async Task<IActionResult> GetTrayByMacAddress(string macAddress)
    {
        var tray = await _trayService.GetTrayByMacAddressAsync(macAddress);
        if (tray == null)
        {
            return NotFound(new { message = $"Bandeja com MAC '{macAddress}' não encontrada" });
        }
        return Ok(tray);
    }

    /// <summary>
    /// Lista as bandejas de um carrinho específico
    /// </summary>
    [HttpGet("cart/{cartId}")]
    public async Task<IActionResult> GetTraysByCartId(int cartId)
    {
        var trays = await _trayService.GetTraysByCartIdAsync(cartId);
        return Ok(trays);
    }

    /// <summary>
    /// Lista bandejas não atribuídas a nenhum carrinho
    /// </summary>
    [HttpGet("unassigned")]
    public async Task<IActionResult> GetUnassignedTrays()
    {
        var trays = await _trayService.GetUnassignedTraysAsync();
        return Ok(trays);
    }

    /// <summary>
    /// Atribui uma bandeja a um carrinho
    /// </summary>
    [HttpPut("{trayId}/assign/{cartId}")]
    public async Task<IActionResult> AssignTrayToCart(int trayId, int cartId)
    {
        try
        {
            var success = await _trayService.AssignTrayToCartAsync(trayId, cartId);
            if (!success)
            {
                return NotFound(new { message = $"Bandeja com ID {trayId} não encontrada" });
            }
            return Ok(new { message = "Bandeja atribuída ao carrinho com sucesso" });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Remove a atribuição de uma bandeja de seu carrinho
    /// </summary>
    [HttpPut("{trayId}/unassign")]
    public async Task<IActionResult> UnassignTrayFromCart(int trayId)
    {
        var success = await _trayService.UnassignTrayFromCartAsync(trayId);
        if (!success)
        {
            return NotFound(new { message = $"Bandeja com ID {trayId} não encontrada" });
        }
        return Ok(new { message = "Bandeja desatribuída do carrinho com sucesso" });
    }

    /// <summary>
    /// Configura ou atualiza a bandeja (LEGADO - mantido para compatibilidade)
    /// </summary>
    [HttpPost("configure")]
    [Obsolete("Use POST /api/trays or PUT /api/trays/{id} instead")]
    public async Task<IActionResult> ConfigureTray([FromBody] TrayConfigPayload payload)
    {
        try
        {
            if (payload.CartId <= 0)
            {
                return BadRequest("Cart ID é obrigatório");
            }

            var tray = await _trayService.ConfigureTrayAsync(payload);
            return Ok(tray);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Deleta uma bandeja
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTray(int id)
    {
        var deleted = await _trayService.DeleteTrayAsync(id);
        if (!deleted)
        {
            return NotFound(new { message = $"Bandeja com ID {id} não encontrada" });
        }
        return NoContent();
    }

    /// <summary>
    /// Valida uma configuração de bandeja sem salvar
    /// </summary>
    [HttpPost("validate")]
    public async Task<IActionResult> ValidateTrayConfiguration([FromBody] List<TrayBlockPayload> blocksPayload)
    {
        var blocks = blocksPayload.Select(b => new TrayBlock
        {
            blockId = b.BlockId,
            startRow = b.StartRow,
            endRow = b.EndRow,
            startColumn = b.StartColumn,
            endColumn = b.EndColumn,
            label = b.Label,
            color = b.Color,
            sensorIndex = b.SensorIndex
        }).ToList();

        var isValid = await _trayService.ValidateTrayConfiguration(blocks);
        
        if (isValid)
        {
            return Ok(new { valid = true, message = "Configuração válida" });
        }
        else
        {
            return BadRequest(new { valid = false, message = "Configuração inválida. Verifique os logs para detalhes." });
        }
    }
}
