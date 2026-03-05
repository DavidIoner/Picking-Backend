.PHONY: help build up down restart logs clean rebuild

# Variáveis
COMPOSE_FILE = docker-compose.yml
API_CONTAINER = cnh-api
DB_CONTAINER = cnh-postgres

# ==========================================
# Help - Mostra todos os comandos disponíveis
# ==========================================
help:
	@echo "📦 CNH Metodologia Picking - Docker Commands"
	@echo ""
	@echo "Comandos disponíveis:"
	@echo "  make build       - Build das imagens Docker"
	@echo "  make up          - Inicia os containers"
	@echo "  make down        - Para e remove os containers"
	@echo "  make restart     - Reinicia os containers"
	@echo "  make logs        - Mostra logs da API"
	@echo "  make logs-db     - Mostra logs do PostgreSQL"
	@echo "  make logs-all    - Mostra logs de todos os containers"
	@echo "  make clean       - Remove containers, networks e volumes"
	@echo "  make rebuild     - Rebuild completo (down + build + up)"
	@echo "  make shell       - Acessa o shell do container da API"
	@echo "  make shell-db    - Acessa o PostgreSQL"
	@echo "  make ps          - Lista containers em execução"
	@echo "  make backup      - Faz backup do banco de dados"
	@echo ""

# ==========================================
# Build - Constrói as imagens
# ==========================================
build:
	@echo "🔨 Building Docker images..."
	docker-compose -f $(COMPOSE_FILE) build

# ==========================================
# Up - Inicia os containers
# ==========================================
up:
	@echo "🚀 Starting containers..."
	docker-compose -f $(COMPOSE_FILE) up -d
	@echo "✅ Containers started!"
	@echo "📍 API: http://localhost:8080"
	@echo "📍 Swagger: http://localhost:8080/swagger"
	@echo "📍 Health: http://localhost:8080/health"

# ==========================================
# Down - Para os containers
# ==========================================
down:
	@echo "🛑 Stopping containers..."
	docker-compose -f $(COMPOSE_FILE) down
	@echo "✅ Containers stopped!"

# ==========================================
# Restart - Reinicia os containers
# ==========================================
restart:
	@echo "🔄 Restarting containers..."
	docker-compose -f $(COMPOSE_FILE) restart
	@echo "✅ Containers restarted!"

# ==========================================
# Logs - Mostra logs da API
# ==========================================
logs:
	@echo "📋 API Logs (Ctrl+C para sair)..."
	docker-compose -f $(COMPOSE_FILE) logs -f $(API_CONTAINER)

# ==========================================
# Logs DB - Mostra logs do PostgreSQL
# ==========================================
logs-db:
	@echo "📋 PostgreSQL Logs (Ctrl+C para sair)..."
	docker-compose -f $(COMPOSE_FILE) logs -f $(DB_CONTAINER)

# ==========================================
# Logs All - Mostra todos os logs
# ==========================================
logs-all:
	@echo "📋 All Logs (Ctrl+C para sair)..."
	docker-compose -f $(COMPOSE_FILE) logs -f

# ==========================================
# Clean - Remove tudo (incluindo volumes)
# ==========================================
clean:
	@echo "🧹 Cleaning up..."
	docker-compose -f $(COMPOSE_FILE) down -v
	@echo "✅ Cleanup complete!"

# ==========================================
# Rebuild - Rebuild completo
# ==========================================
rebuild:
	@echo "🔄 Full rebuild..."
	@make down
	@make build
	@make up
	@echo "✅ Rebuild complete!"

# ==========================================
# Shell - Acessa o shell da API
# ==========================================
shell:
	@echo "🐚 Accessing API container shell..."
	docker exec -it $(API_CONTAINER) bash

# ==========================================
# Shell DB - Acessa o PostgreSQL
# ==========================================
shell-db:
	@echo "🐚 Accessing PostgreSQL..."
	docker exec -it $(DB_CONTAINER) psql -U cnh_user -d cnh_metodologia_picking

# ==========================================
# PS - Lista containers
# ==========================================
ps:
	@echo "📊 Container status:"
	docker-compose -f $(COMPOSE_FILE) ps

# ==========================================
# Backup - Backup do banco de dados
# ==========================================
backup:
	@echo "💾 Creating database backup..."
	@mkdir -p backups
	docker exec $(DB_CONTAINER) pg_dump -U cnh_user cnh_metodologia_picking > backups/backup_$$(date +%Y%m%d_%H%M%S).sql
	@echo "✅ Backup created in backups/ folder"

# ==========================================
# Dev - Modo desenvolvimento (só DB)
# ==========================================
dev:
	@echo "🔧 Starting PostgreSQL only (for local development)..."
	docker-compose -f $(COMPOSE_FILE) up -d $(DB_CONTAINER)
	@echo "✅ PostgreSQL started!"
	@echo "📍 Connection: localhost:5432"
	@echo "📍 Database: cnh_metodologia_picking"
	@echo "📍 User: cnh_user"
