using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using API.Models;
using API.Options;
using API.Services;
using Microsoft.Extensions.Options;
using MQTTnet;

namespace API.Clients;

/// <summary>
/// Cliente MQTT que se inscreve em tópicos e processa mensagens de peso recebidas
/// </summary>
public class MqttClientService : BackgroundService
{
    private readonly ILogger<MqttClientService> _logger;
    private readonly MqttOptions _options;
    private readonly IServiceProvider _serviceProvider;
    private IMqttClient? _mqttClient;

    public MqttClientService(
        ILogger<MqttClientService> logger,
        IOptions<MqttOptions> options,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _options = options.Value;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Cliente MQTT está desabilitado na configuração");
            return;
        }

        _logger.LogInformation("Iniciando cliente MQTT...");

        try
        {
            var factory = new MqttClientFactory();
            _mqttClient = factory.CreateMqttClient();

            // Configurar opções de conexão
            var clientOptionsBuilder = new MqttClientOptionsBuilder()
                .WithTcpServer(_options.Server, _options.Port)
                .WithClientId(_options.ClientId);

            // Adicionar credenciais se configuradas
            if (!string.IsNullOrEmpty(_options.Username) && !string.IsNullOrEmpty(_options.Password))
            {
                clientOptionsBuilder.WithCredentials(_options.Username, _options.Password);
            }

            var clientOptions = clientOptionsBuilder.Build();

            // Configurar handler para quando conectar
            _mqttClient.ConnectedAsync += async e =>
            {
                _logger.LogInformation("✅ Conectado ao broker MQTT em {Server}:{Port}", _options.Server, _options.Port);

                // Inscrever no tópico
                var subscribeOptions = factory.CreateSubscribeOptionsBuilder()
                    .WithTopicFilter(f => f.WithTopic(_options.Topic))
                    .Build();

                await _mqttClient.SubscribeAsync(subscribeOptions, stoppingToken);

                _logger.LogInformation("📡 Inscrito no tópico: {Topic}", _options.Topic);
            };

            // Configurar handler para quando desconectar
            _mqttClient.DisconnectedAsync += async e =>
            {
                _logger.LogWarning("⚠️  Desconectado do broker MQTT. Razão: {Reason}", e.Reason);

                // Aguardar antes de tentar reconectar
                if (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

                    try
                    {
                        _logger.LogInformation("🔄 Tentando reconectar ao broker MQTT...");
                        await _mqttClient.ConnectAsync(clientOptions, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ Erro ao tentar reconectar ao broker MQTT");
                    }
                }
            };

            _mqttClient.ApplicationMessageReceivedAsync += async e =>
            {
                try
                {
                    await HandleMessageAsync(e);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Erro ao processar mensagem MQTT do tópico {Topic}", e.ApplicationMessage.Topic);
                }
            };

            _logger.LogInformation("🔌 Conectando ao broker MQTT {Server}:{Port}...", _options.Server, _options.Port);
            await _mqttClient.ConnectAsync(clientOptions, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro fatal no cliente MQTT");
        }
    }

    private async Task HandleMessageAsync(MqttApplicationMessageReceivedEventArgs e)
    {
        var topic = e.ApplicationMessage.Topic;
        var payload = e.ApplicationMessage.ConvertPayloadToString();

        _logger.LogInformation("📨 Mensagem recebida - Tópico: {Topic}, Payload: {Payload}", topic, payload);

        // Extrair MAC address do tópico (formato: devices/{mac}/weight)
        var mac = ExtractMacFromTopic(topic);
        if (string.IsNullOrEmpty(mac))
        {
            _logger.LogWarning("⚠️  MAC address não encontrado no tópico: {Topic}", topic);
            return;
        }

        try
        {

            var message = JsonSerializer.Deserialize<MqttWeightMessage>(payload);
            if (message == null || message.Weights == null || message.Weights.Count == 0)
            {
                _logger.LogWarning("⚠️  Payload inválido ou vazio: {Payload}", payload);
                return;
            }

            var timestamp = DateTimeOffset.FromUnixTimeSeconds((long)message.Timestamp).UtcDateTime;

            _logger.LogInformation(
                "⚖️  Processando leitura - MAC: {Mac}, Timestamp: {Timestamp}, Leituras: {Count}",
                mac, timestamp, message.Weights.Count);
            
            using var scope = _serviceProvider.CreateScope();
            var readingService = scope.ServiceProvider.GetRequiredService<IReadingService>();

            var readingPayload = new ReadingPayload
            {
                MacAddress = mac,
                Timestamp = timestamp,
                Readings = message.Weights
            };

            await readingService.RegisterReadingAsync(readingPayload);

            _logger.LogInformation("✅ Leitura salva com sucesso - MAC: {Mac}", mac);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "❌ Erro ao deserializar JSON: {Payload}", payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao salvar leitura no banco de dados - MAC: {Mac}", mac);
        }
    }

    /// <summary>
    /// Extrai o MAC address do tópico MQTT (formato: devices/{mac}/weight)
    /// </summary>
    private string? ExtractMacFromTopic(string topic)
    {
        var match = Regex.Match(topic, @"devices/([^/]+)/weight");
        return match.Success ? match.Groups[1].Value : null;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("🛑 Parando cliente MQTT...");

        if (_mqttClient is not null)
        {
            try
            {
                var disconnectOptions = new MqttClientDisconnectOptionsBuilder()
                    .WithReason(MqttClientDisconnectOptionsReason.NormalDisconnection)
                    .Build();

                await _mqttClient.DisconnectAsync(disconnectOptions, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao desconectar cliente MQTT");
            }
            finally
            {
                _mqttClient.Dispose();
            }
        }

        await base.StopAsync(cancellationToken);
    }
}
