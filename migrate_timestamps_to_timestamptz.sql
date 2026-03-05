-- Script de migração: Converter colunas TIMESTAMP para TIMESTAMPTZ
-- Este script corrige o problema de timezone nas tabelas carrinhos e bandejas

-- ========================================
-- TABELA: carrinhos
-- ========================================

-- Alterar data_criacao de TIMESTAMP para TIMESTAMPTZ
ALTER TABLE carrinhos 
ALTER COLUMN data_criacao TYPE TIMESTAMPTZ USING data_criacao AT TIME ZONE 'UTC';

-- Alterar data_atualizacao de TIMESTAMP para TIMESTAMPTZ
ALTER TABLE carrinhos 
ALTER COLUMN data_atualizacao TYPE TIMESTAMPTZ USING data_atualizacao AT TIME ZONE 'UTC';

-- ========================================
-- TABELA: bandejas
-- ========================================

-- Alterar data_criacao de TIMESTAMP para TIMESTAMPTZ
ALTER TABLE bandejas 
ALTER COLUMN data_criacao TYPE TIMESTAMPTZ USING data_criacao AT TIME ZONE 'UTC';

-- Alterar data_atualizacao de TIMESTAMP para TIMESTAMPTZ
ALTER TABLE bandejas 
ALTER COLUMN data_atualizacao TYPE TIMESTAMPTZ USING data_atualizacao AT TIME ZONE 'UTC';

-- ========================================
-- VERIFICAÇÃO
-- ========================================

-- Verificar os tipos das colunas após a migração
SELECT 
    table_name,
    column_name,
    data_type,
    udt_name
FROM information_schema.columns
WHERE table_name IN ('carrinhos', 'bandejas')
AND column_name IN ('data_criacao', 'data_atualizacao')
ORDER BY table_name, column_name;

-- Resultado esperado:
-- table_name | column_name       | data_type                   | udt_name
-- -----------|-------------------|-----------------------------|---------
-- bandejas   | data_atualizacao  | timestamp with time zone    | timestamptz
-- bandejas   | data_criacao      | timestamp with time zone    | timestamptz
-- carrinhos  | data_atualizacao  | timestamp with time zone    | timestamptz
-- carrinhos  | data_criacao      | timestamp with time zone    | timestamptz
