using API.Data;
using API.Repositories;
using API.Services;
using API.Clients;
using API.Options;
using Microsoft.EntityFrameworkCore;

// Configurar Npgsql para tratar DateTime como UTC
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", false);

var builder = WebApplication.CreateBuilder(args);

// Adicionar serviços ao container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configurar serialização JSON para suportar arrays de double
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        
        // Ignorar ciclos de referência (ex: Cart -> Trays -> Cart)
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configurar DbContext com PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseNpgsql(connectionString);
});

// Configurar opções MQTT
builder.Services.Configure<MqttOptions>(builder.Configuration.GetSection(MqttOptions.SectionName));

// Nossas injeções de dependência
// Repositórios PostgreSQL
builder.Services.AddScoped<IReadingRepository, PostgresReadingRepository>();
// builder.Services.AddSingleton<IReadingRepository, InMemoryReadingRepository>(); // Descomente para usar InMemory
builder.Services.AddScoped<IReadingService, ReadingService>();

// Serviços de Carrinho e Bandeja
builder.Services.AddScoped<ICartRepository, PostgresCartRepository>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<ITrayRepository, PostgresTrayRepository>();
builder.Services.AddScoped<ITrayService, TrayService>();

// Cliente MQTT como serviço em background
builder.Services.AddHostedService<MqttClientService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.WithOrigins(
                      "https://cnh-front-end.onrender.com", // A URL de produção do seu Front
                      "http://localhost:5173"               // A URL local do seu Front (ajuste a porta se for diferente)
                  ) 
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Habilitar CORS
app.UseCors();
app.UseAuthorization();

// Health check endpoint para Docker
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
   .WithName("HealthCheck")
   .WithOpenApi();

app.MapControllers();

app.Run();

