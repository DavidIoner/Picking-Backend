using API.Models;
using API.Repositories;

namespace API.Services;

public class TrayService : ITrayService
{
    private readonly ITrayRepository _trayRepository;
    private readonly ICartRepository _cartRepository;

    // Constantes da bandeja: 4 linhas x 6 colunas
    private const int TOTAL_ROWS = 4;
    private const int TOTAL_COLUMNS = 6;

    public TrayService(ITrayRepository trayRepository, ICartRepository cartRepository)
    {
        _trayRepository = trayRepository;
        _cartRepository = cartRepository;
    }

    public async Task<Tray> CreateTrayAsync(CreateTrayPayload payload)
    {
        // Validar MAC Address
        if (string.IsNullOrWhiteSpace(payload.MacAddress))
        {
            throw new ArgumentException("MAC Address é obrigatório");
        }

        // Verificar se já existe
        if (await _trayRepository.MacAddressExistsAsync(payload.MacAddress))
        {
            throw new InvalidOperationException($"Já existe uma bandeja com o MAC Address '{payload.MacAddress}'");
        }

        // Validar formato do MAC Address
        if (!IsValidMacAddress(payload.MacAddress))
        {
            throw new ArgumentException("MAC Address em formato inválido. Use o formato: XX:XX:XX:XX:XX:XX");
        }

        // Se cartId foi fornecido, verificar se existe
        if (payload.CartId.HasValue)
        {
            var cart = await _cartRepository.GetByIdAsync(payload.CartId.Value);
            if (cart == null)
            {
                throw new InvalidOperationException($"Carrinho com ID {payload.CartId} não encontrado");
            }
        }

        var tray = new Tray
        {
            mac_address = payload.MacAddress.ToUpper(),
            nome = payload.Name,
            descricao = payload.Description,
            carrinho_id = payload.CartId,
            ativo = true,
            data_criacao = DateTime.UtcNow
        };

        return await _trayRepository.CreateAsync(tray);
    }

    public async Task<Tray?> UpdateTrayAsync(int id, UpdateTrayPayload payload)
    {
        var existingTray = await _trayRepository.GetByIdAsync(id);
        if (existingTray == null)
        {
            return null;
        }

        // Se está mudando de carrinho, validar que o novo carrinho existe
        if (payload.CartId.HasValue && payload.CartId.Value != existingTray.carrinho_id)
        {
            var cart = await _cartRepository.GetByIdAsync(payload.CartId.Value);
            if (cart == null)
            {
                throw new InvalidOperationException($"Carrinho com ID {payload.CartId} não encontrado");
            }
            existingTray.carrinho_id = payload.CartId.Value;
        }

        if (!string.IsNullOrWhiteSpace(payload.Name))
        {
            existingTray.nome = payload.Name.Trim();
        }

        if (payload.Description != null)
        {
            existingTray.descricao = payload.Description;
        }

        if (payload.Blocks != null)
        {
            var blocks = payload.Blocks.Select(b => new TrayBlock
            {
                blockId = b.BlockId,
                startRow = b.StartRow,
                endRow = b.EndRow,
                startColumn = b.StartColumn,
                endColumn = b.EndColumn,
                label = b.Label,
                color = b.Color,
                sensorIndex = b.SensorIndex,
                targetWeight = b.TargetWeight
            }).ToList();

            if (!await ValidateTrayConfiguration(blocks))
            {
                throw new ArgumentException("Configuração da bandeja inválida. Verifique sobreposições e limites.");
            }

            existingTray.blocos = blocks;
        }

        if (payload.IsActive.HasValue)
        {
            existingTray.ativo = payload.IsActive.Value;
        }

        return await _trayRepository.UpdateAsync(id, existingTray);
    }

    [Obsolete("Use CreateTrayAsync or UpdateTrayAsync instead")]
    public async Task<Tray> ConfigureTrayAsync(TrayConfigPayload payload)
    {
        // Manter para compatibilidade
        var cart = await _cartRepository.GetByIdAsync(payload.CartId);
        if (cart == null)
        {
            throw new InvalidOperationException($"Carrinho com ID {payload.CartId} não encontrado");
        }

        var blocks = payload.Blocks.Select(b => new TrayBlock
        {
            blockId = b.BlockId,
            startRow = b.StartRow,
            endRow = b.EndRow,
            startColumn = b.StartColumn,
            endColumn = b.EndColumn,
            label = b.Label,
            color = b.Color,
            sensorIndex = b.SensorIndex,
            targetWeight = b.TargetWeight
        }).ToList();

        if (!await ValidateTrayConfiguration(blocks))
        {
            throw new ArgumentException("Configuração da bandeja inválida. Verifique sobreposições e limites.");
        }

        // Buscar a primeira bandeja do carrinho para atualizar
        var trays = await _trayRepository.GetByCartIdAsync(payload.CartId);
        var tray = trays.FirstOrDefault();

        if (tray != null)
        {
            tray.nome = payload.Name;
            tray.blocos = blocks;
            return (await _trayRepository.UpdateAsync(tray.id, tray))!;
        }

        throw new InvalidOperationException("Nenhuma bandeja encontrada para este carrinho");
    }

    public async Task<Tray?> GetTrayByIdAsync(int id)
    {
        return await _trayRepository.GetByIdAsync(id);
    }

    public async Task<Tray?> GetTrayByMacAddressAsync(string macAddress)
    {
        return await _trayRepository.GetByMacAddressAsync(macAddress.ToUpper());
    }

    public async Task<IEnumerable<Tray>> GetTraysByCartIdAsync(int cartId)
    {
        return await _trayRepository.GetByCartIdAsync(cartId);
    }

    public async Task<IEnumerable<Tray>> GetUnassignedTraysAsync()
    {
        return await _trayRepository.GetUnassignedTraysAsync();
    }

    public async Task<IEnumerable<Tray>> GetAllTraysAsync(bool includeInactive = false)
    {
        return await _trayRepository.GetAllAsync(includeInactive);
    }

    public async Task<bool> AssignTrayToCartAsync(int trayId, int cartId)
    {
        var tray = await _trayRepository.GetByIdAsync(trayId);
        if (tray == null)
        {
            return false;
        }

        var cart = await _cartRepository.GetByIdAsync(cartId);
        if (cart == null)
        {
            throw new InvalidOperationException($"Carrinho com ID {cartId} não encontrado");
        }

        tray.carrinho_id = cartId;
        await _trayRepository.UpdateAsync(trayId, tray);
        return true;
    }

    public async Task<bool> UnassignTrayFromCartAsync(int trayId)
    {
        var tray = await _trayRepository.GetByIdAsync(trayId);
        if (tray == null)
        {
            return false;
        }

        tray.carrinho_id = null;
        await _trayRepository.UpdateAsync(trayId, tray);
        return true;
    }

    public async Task<bool> DeleteTrayAsync(int id)
    {
        return await _trayRepository.DeleteAsync(id);
    }

    private bool IsValidMacAddress(string macAddress)
    {
        if (string.IsNullOrWhiteSpace(macAddress))
            return false;

        var parts = macAddress.Split(':');
        if (parts.Length != 6)
            return false;

        foreach (var part in parts)
        {
            if (part.Length != 2)
                return false;

            if (!System.Text.RegularExpressions.Regex.IsMatch(part, "^[0-9A-Fa-f]{2}$"))
                return false;
        }

        return true;
    }

    public Task<bool> ValidateTrayConfiguration(List<TrayBlock> blocks)
    {
        if (blocks == null || blocks.Count == 0)
        {
            return Task.FromResult(true); // Bandeja vazia é válida
        }

        // Matriz para rastrear ocupação: [linha][coluna]
        var occupiedCells = new bool[TOTAL_ROWS, TOTAL_COLUMNS];

        foreach (var block in blocks)
        {
            // Validar limites de linhas
            if (block.startRow < 0 || block.startRow >= TOTAL_ROWS)
            {
                Console.WriteLine($"---> Erro: startRow {block.startRow} fora dos limites (0-{TOTAL_ROWS - 1})");
                return Task.FromResult(false);
            }

            if (block.endRow < 0 || block.endRow >= TOTAL_ROWS)
            {
                Console.WriteLine($"---> Erro: endRow {block.endRow} fora dos limites (0-{TOTAL_ROWS - 1})");
                return Task.FromResult(false);
            }

            if (block.startRow > block.endRow)
            {
                Console.WriteLine($"---> Erro: startRow ({block.startRow}) maior que endRow ({block.endRow})");
                return Task.FromResult(false);
            }

            // Validar limites de colunas
            if (block.startColumn < 0 || block.startColumn >= TOTAL_COLUMNS)
            {
                Console.WriteLine($"---> Erro: startColumn {block.startColumn} fora dos limites (0-{TOTAL_COLUMNS - 1})");
                return Task.FromResult(false);
            }

            if (block.endColumn < 0 || block.endColumn >= TOTAL_COLUMNS)
            {
                Console.WriteLine($"---> Erro: endColumn {block.endColumn} fora dos limites (0-{TOTAL_COLUMNS - 1})");
                return Task.FromResult(false);
            }

            if (block.startColumn > block.endColumn)
            {
                Console.WriteLine($"---> Erro: startColumn ({block.startColumn}) maior que endColumn ({block.endColumn})");
                return Task.FromResult(false);
            }

            // Verificar sobreposição - itera sobre todas as linhas e colunas do bloco
            for (int row = block.startRow; row <= block.endRow; row++)
            {
                for (int col = block.startColumn; col <= block.endColumn; col++)
                {
                    if (occupiedCells[row, col])
                    {
                        Console.WriteLine($"---> Erro: Sobreposição detectada na linha {row}, coluna {col}");
                        return Task.FromResult(false);
                    }
                    occupiedCells[row, col] = true;
                }
            }
        }

        Console.WriteLine($"---> Configuração da bandeja válida com {blocks.Count} bloco(s)");
        return Task.FromResult(true);
    }
}
