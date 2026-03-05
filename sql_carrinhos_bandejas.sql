-- ========================================
-- ESTRUTURA CORRIGIDA
-- ========================================
-- Bandeja (Tray) = Dispositivo físico com MAC Address
-- Carrinho (Cart) = Agrupamento lógico de várias bandejas
-- ========================================

-- Função para atualizar timestamp automaticamente
CREATE OR REPLACE FUNCTION trigger_set_timestamp()
RETURNS TRIGGER AS $$
BEGIN
    NEW.data_atualizacao = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- ========================================
-- TABELA: carrinhos (Cart)
-- ========================================
-- Carrinho é um agrupamento lógico de bandejas
CREATE TABLE IF NOT EXISTS carrinhos (
    id SERIAL PRIMARY KEY,
    nome VARCHAR(100) NOT NULL,
    descricao TEXT,
    ativo BOOLEAN DEFAULT TRUE NOT NULL,
    data_criacao TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    data_atualizacao TIMESTAMPTZ
);

-- Índices para carrinhos
CREATE INDEX IF NOT EXISTS idx_ativo_carrinho ON carrinhos(ativo);
CREATE INDEX IF NOT EXISTS idx_nome_carrinho ON carrinhos(nome);

-- Trigger para atualizar 'data_atualizacao' na tabela 'carrinhos'
DROP TRIGGER IF EXISTS set_timestamp_carrinhos ON carrinhos;
CREATE TRIGGER set_timestamp_carrinhos
BEFORE UPDATE ON carrinhos
FOR EACH ROW
EXECUTE PROCEDURE trigger_set_timestamp();

-- ========================================
-- TABELA: bandejas (Tray)
-- ========================================
-- Bandeja é o dispositivo físico identificado por MAC Address
CREATE TABLE IF NOT EXISTS bandejas (
    id SERIAL PRIMARY KEY,
    mac_address CHAR(17) NOT NULL UNIQUE,
    carrinho_id INTEGER,  -- Pode ser NULL (bandeja não atribuída a nenhum carrinho)
    nome VARCHAR(100),
    descricao TEXT,
    -- JSONB é ideal para armazenar configuração dos blocos/células
    blocos JSONB,
    ativo BOOLEAN DEFAULT TRUE NOT NULL,
    data_criacao TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    data_atualizacao TIMESTAMPTZ,
    -- Constraint para garantir formato do MAC Address
    CONSTRAINT chk_mac_format_bandeja CHECK (mac_address ~* '^([0-9A-F]{2}:){5}[0-9A-F]{2}$'),
    -- Foreign Key para carrinhos (CASCADE para remover associação se carrinho for deletado)
    FOREIGN KEY (carrinho_id) REFERENCES carrinhos(id) ON DELETE SET NULL
);

-- Índices para bandejas
CREATE INDEX IF NOT EXISTS idx_mac_address_bandeja ON bandejas(mac_address);
CREATE INDEX IF NOT EXISTS idx_carrinho_id_bandeja ON bandejas(carrinho_id);
CREATE INDEX IF NOT EXISTS idx_ativo_bandeja ON bandejas(ativo);

-- Trigger para atualizar 'data_atualizacao' na tabela 'bandejas'
DROP TRIGGER IF EXISTS set_timestamp_bandejas ON bandejas;
CREATE TRIGGER set_timestamp_bandejas
BEFORE UPDATE ON bandejas
FOR EACH ROW
EXECUTE PROCEDURE trigger_set_timestamp();

-- ========================================
-- TABELA: leituras_dispositivo (Weight Readings)
-- ========================================
-- Armazena leituras de peso enviadas pelas bandejas
CREATE TABLE IF NOT EXISTS leituras_dispositivo (
    id SERIAL PRIMARY KEY,
    mac_address CHAR(17) NOT NULL,
    -- Armazena o array de leituras. O índice do array corresponde à posição na bandeja.
    leituras FLOAT8[] NOT NULL,
    timestamp_leitura TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    -- Constraint para garantir formato do MAC Address
    CONSTRAINT chk_mac_format_leitura CHECK (mac_address ~* '^([0-9A-F]{2}:){5}[0-9A-F]{2}$'),
    -- Foreign Key para bandejas (pode ser NULL se bandeja ainda não foi cadastrada)
    FOREIGN KEY (mac_address) REFERENCES bandejas(mac_address) ON DELETE CASCADE
);

-- Índices para leituras_dispositivo
CREATE INDEX IF NOT EXISTS idx_mac_address_leituras ON leituras_dispositivo(mac_address);
CREATE INDEX IF NOT EXISTS idx_timestamp_leitura ON leituras_dispositivo(timestamp_leitura);

-- ========================================
-- TRIGGER: Auto-inserção de bandeja ao detectar novo MAC em leituras_dispositivo
-- ========================================

CREATE OR REPLACE FUNCTION insert_bandeja_if_not_exists() RETURNS TRIGGER AS $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM bandejas WHERE mac_address = NEW.mac_address) THEN
        INSERT INTO bandejas (mac_address, nome, ativo, data_criacao)
        VALUES (NEW.mac_address, 'Bandeja ' || NEW.mac_address, TRUE, CURRENT_TIMESTAMP);
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS trg_insert_bandeja_on_leitura ON leituras_dispositivo;
CREATE TRIGGER trg_insert_bandeja_on_leitura
AFTER INSERT ON leituras_dispositivo
FOR EACH ROW
EXECUTE PROCEDURE insert_bandeja_if_not_exists();

-- ========================================
-- COMENTÁRIOS
-- ========================================
COMMENT ON TABLE carrinhos IS 'Agrupamento lógico de bandejas. Um carrinho pode conter múltiplas bandejas.';
COMMENT ON TABLE bandejas IS 'Dispositivo físico identificado por MAC Address. Pode ser atribuído a um carrinho.';
COMMENT ON TABLE leituras_dispositivo IS 'Armazena leituras de peso. Cada registro é um evento de leitura com um array de valores posicionais vindos de uma bandeja.';

COMMENT ON COLUMN bandejas.mac_address IS 'Identificador único do dispositivo físico (bandeja)';
COMMENT ON COLUMN bandejas.carrinho_id IS 'ID do carrinho ao qual esta bandeja está atribuída (NULL se não atribuída)';
COMMENT ON COLUMN bandejas.blocos IS 'Configuração JSONB dos blocos/células da bandeja';
COMMENT ON COLUMN carrinhos.nome IS 'Nome identificador do carrinho (ex: "Carrinho Setor A", "Linha 1")';
COMMENT ON COLUMN leituras_dispositivo.mac_address IS 'MAC Address da bandeja que enviou a leitura';