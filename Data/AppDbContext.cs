using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;
using API.Models;

namespace API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<WeightReading> WeightReadings { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<Tray> Trays { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        // ========================================
        // Configuração da entidade WeightReading
        // ========================================
        modelBuilder.Entity<WeightReading>(entity =>
        {
            // Mapeia para a tabela leituras_dispositivo
            entity.ToTable("leituras_dispositivo");

            // Chave primária
            entity.HasKey(e => e.id);
            entity.Property(e => e.id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            // MAC Address da bandeja que enviou a leitura
            entity.Property(e => e.mac_address)
                .HasColumnName("mac_address")
                .HasColumnType("bpchar")
                .HasMaxLength(17)
                .IsRequired();

            // Array de leituras (double precision[])
            entity.Property(e => e.leituras)
                .HasColumnName("leituras")
                .HasColumnType("double precision[]")
                .IsRequired();

            // Timestamp da leitura (com timezone)
            entity.Property(e => e.timestamp_leitura)
                .HasColumnName("timestamp_leitura")
                .HasColumnType("timestamptz")
                .IsRequired();

            // Índices
            entity.HasIndex(e => e.mac_address)
                .HasDatabaseName("idx_mac_address_leituras");

            entity.HasIndex(e => e.timestamp_leitura)
                .HasDatabaseName("idx_timestamp_leitura");
        });

        // ========================================
        // Configuração da entidade Cart
        // ========================================
        modelBuilder.Entity<Cart>(entity =>
        {
            entity.ToTable("carrinhos");

            // Chave primária
            entity.HasKey(e => e.id);
            entity.Property(e => e.id)
                .HasColumnName("id")
                .HasColumnType("int4")
                .ValueGeneratedOnAdd();

            // Nome (varchar(100), not null)
            entity.Property(e => e.nome)
                .HasColumnName("nome")
                .HasColumnType("varchar")
                .HasMaxLength(100)
                .IsRequired();

            // Descrição (text, nullable)
            entity.Property(e => e.descricao)
                .HasColumnName("descricao")
                .HasColumnType("text");

            // Ativo (bool, not null, default true)
            entity.Property(e => e.ativo)
                .HasColumnName("ativo")
                .HasColumnType("bool")
                .IsRequired()
                .HasDefaultValue(true);

            // Data de criação (timestamptz, nullable, default CURRENT_TIMESTAMP)
            entity.Property(e => e.data_criacao)
                .HasColumnName("data_criacao")
                .HasColumnType("timestamptz")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Data de atualização (timestamptz, nullable)
            entity.Property(e => e.data_atualizacao)
                .HasColumnName("data_atualizacao")
                .HasColumnType("timestamptz");

            // Índices
            entity.HasIndex(e => e.nome)
                .HasDatabaseName("idx_nome_carrinho");

            entity.HasIndex(e => e.ativo)
                .HasDatabaseName("idx_ativo_carrinho");

            // Relacionamento 1:N com Tray (um carrinho tem múltiplas bandejas)
            entity.HasMany(e => e.Trays)
                .WithOne(t => t.Cart)
                .HasForeignKey(t => t.carrinho_id)
                .OnDelete(DeleteBehavior.SetNull); // Se carrinho for deletado, bandeja fica sem carrinho
        });

        // ========================================
        // Configuração da entidade Tray
        // ========================================
        modelBuilder.Entity<Tray>(entity =>
        {
            entity.ToTable("bandejas");

            // Chave primária
            entity.HasKey(e => e.id);
            entity.Property(e => e.id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            // MAC Address (bpchar(17), not null, unique)
            entity.Property(e => e.mac_address)
                .HasColumnName("mac_address")
                .HasColumnType("bpchar")
                .HasMaxLength(17)
                .IsRequired();

            // Carrinho ID (int, nullable - bandeja pode não estar atribuída)
            entity.Property(e => e.carrinho_id)
                .HasColumnName("carrinho_id");

            // Nome (varchar(100), nullable)
            entity.Property(e => e.nome)
                .HasColumnName("nome")
                .HasMaxLength(100);

            // Descrição (text, nullable)
            entity.Property(e => e.descricao)
                .HasColumnName("descricao")
                .HasColumnType("text");

            // Mapeia blocos (List<TrayBlock>?) para a coluna JSONB
            var blocksConverter = new ValueConverter<List<TrayBlock>?, string>(
                v => v == null ? "null" : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => v == "null" ? null : JsonSerializer.Deserialize<List<TrayBlock>>(v, (JsonSerializerOptions?)null)
            );

            var blocksComparer = new ValueComparer<List<TrayBlock>?>(
                (c1, c2) => JsonSerializer.Serialize(c1, (JsonSerializerOptions?)null) == JsonSerializer.Serialize(c2, (JsonSerializerOptions?)null),
                c => c == null ? 0 : c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c == null ? null : JsonSerializer.Deserialize<List<TrayBlock>>(JsonSerializer.Serialize(c, (JsonSerializerOptions?)null), (JsonSerializerOptions?)null)
            );

            entity.Property(e => e.blocos)
                .HasColumnName("blocos")
                .HasColumnType("jsonb")
                .HasConversion(blocksConverter)
                .Metadata.SetValueComparer(blocksComparer);

            // Ativo (bool, not null, default true)
            entity.Property(e => e.ativo)
                .HasColumnName("ativo")
                .HasColumnType("bool")
                .IsRequired()
                .HasDefaultValue(true);

            // Data de criação (timestamptz, default CURRENT_TIMESTAMP)
            entity.Property(e => e.data_criacao)
                .HasColumnName("data_criacao")
                .HasColumnType("timestamptz")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Data de atualização (timestamptz, nullable)
            entity.Property(e => e.data_atualizacao)
                .HasColumnName("data_atualizacao")
                .HasColumnType("timestamptz");

            // Índices
            entity.HasIndex(e => e.mac_address)
                .IsUnique()
                .HasDatabaseName("idx_mac_address_bandeja");

            entity.HasIndex(e => e.carrinho_id)
                .HasDatabaseName("idx_carrinho_id_bandeja");

            entity.HasIndex(e => e.ativo)
                .HasDatabaseName("idx_ativo_bandeja");
        });
    }
}
