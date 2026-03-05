# ==========================================
# Stage 1: Build
# ==========================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar o arquivo de projeto e restaurar dependências
COPY API.csproj .
RUN dotnet restore "API.csproj"

# Copiar todo o código fonte e compilar
COPY . .
RUN dotnet build "API.csproj" -c Release -o /app/build

# ==========================================
# Stage 2: Publish
# ==========================================
FROM build AS publish
RUN dotnet publish "API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ==========================================
# Stage 3: Runtime
# ==========================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Criar usuário não-root para segurança
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copiar os arquivos publicados do stage anterior
COPY --from=publish /app/publish .

# Configurar permissões
RUN chown -R appuser:appuser /app

# Mudar para usuário não-root
USER appuser

# Expor a porta padrão da API
# Render usa porta 10000 por padrão
EXPOSE 8080
EXPOSE 10000

# Configurar variáveis de ambiente
# Para desenvolvimento local, usa 8080
# Para Render, será sobrescrito pela variável de ambiente
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Healthcheck para verificar se a API está saudável
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Comando para iniciar a aplicação
ENTRYPOINT ["dotnet", "API.dll"]
