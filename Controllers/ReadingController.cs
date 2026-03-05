using Microsoft.AspNetCore.Mvc;
using API.Models;
using API.Services;

namespace API.Controllers;

[ApiController]
[Route("api/readings")] // Rota será /api/readings
public class ReadingController : ControllerBase
{
    private readonly IReadingService _readingService;

    public ReadingController(IReadingService readingService)
    {
        _readingService = readingService;
        Console.WriteLine("ReadingsController carregada!");

    }

    [HttpPost]
    public async Task<IActionResult> PostReading([FromBody] ReadingPayload payload)
    {   
        Console.WriteLine("---> Recebido POST /api/readings");
        if (payload == null || !payload.Readings.Any())
        {
            return BadRequest("Payload inválido ou sem leituras.");
        }

        await _readingService.RegisterReadingAsync(payload);

        // Retorna 202 Accepted, indicando que a requisição foi aceita
        // para processamento, mas o processamento não está completo.
        // É um bom status para cenários de IoT.
        return Accepted();
    }

    [HttpGet]
    public async Task<IActionResult> GetAllReadings()
    {
        var readings = await _readingService.GetAllReadingsAsync();
        return Ok(readings);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetReadingById(int id)
    {
        var reading = await _readingService.GetReadingByIdAsync(id);
        
        if (reading == null)
        {
            return NotFound($"Leitura com ID {id} não encontrada.");
        }

        return Ok(reading);
    }

    [HttpGet("mac/{macAddress}")]
    public async Task<IActionResult> GetLatestReadingByMac(string macAddress)
    {
        var reading = await _readingService.GetLatestReadingByMacAsync(macAddress);
        
        if (reading == null)
        {
            return NotFound($"Nenhuma leitura encontrada para MAC {macAddress}.");
        }

        return Ok(reading);
    }

    [HttpGet("mac/{macAddress}/history")]
    public async Task<IActionResult> GetReadingHistoryByMac(string macAddress, [FromQuery] int days = 7)
    {
        var history = await _readingService.GetReadingHistoryByMacAsync(macAddress, days);
        return Ok(history);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateReading(int id, [FromBody] WeightReading reading)
    {
        if (reading == null)
        {
            return BadRequest("Dados da leitura são obrigatórios.");
        }

        var updatedReading = await _readingService.UpdateReadingAsync(id, reading);
        
        if (updatedReading == null)
        {
            return NotFound($"Leitura com ID {id} não encontrada.");
        }

        return Ok(updatedReading);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReading(int id)
    {
        var deleted = await _readingService.DeleteReadingAsync(id);
        
        if (!deleted)
        {
            return NotFound($"Leitura com ID {id} não encontrada.");
        }

        return NoContent();
    }
}