
-- =========================================================
--  Script de criação da base, usuário
--  Projeto: Carteira Digital
--  Banco:   SQL Server
-- =========================================================

-- 1) Criar banco de dados
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'wallet_homolog')
BEGIN
    CREATE DATABASE wallet_homolog;
END
GO

-- 2) Criar login (usuário do servidor)
IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = 'wallet_api_homolog')
BEGIN
    CREATE LOGIN wallet_api_homolog WITH PASSWORD = 'api123@J';
END
GO

-- fazer os dois de cima PRIMEIRO-----------------------------------------------------------


-- 3) Criar usuário no banco e associar ao login
USE wallet_homolog;
GO

IF NOT EXISTS (SELECT name FROM sys.database_principals WHERE name = 'wallet_api_homolog')
BEGIN
    CREATE USER wallet_api_homolog FOR LOGIN wallet_api_homolog;
END
GO

-- 4) Conceder permissões (SELECT, INSERT, UPDATE, DELETE)
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA :: dbo TO wallet_api_homolog;
GO




-- 5) Criar tabelas necessárias--------------------------------------------------------------------

CREATE TABLE carteira (
    endereco_carteira VARCHAR(100) NOT NULL PRIMARY KEY,  -- PK
    hash_chave_privada VARCHAR(256) NOT NULL,            -- hash da chave privada
    data_criacao DATETIME NOT NULL DEFAULT GETDATE(),    -- data de criação
    status VARCHAR(20) NOT NULL DEFAULT 'ativa'          -- status da carteira
);

-- Nova tabela MOEDA
CREATE TABLE MOEDA (
    id INT IDENTITY(1,1) PRIMARY KEY,
    codigo_moeda VARCHAR(10) NOT NULL UNIQUE,  -- BTC, ETH, SOL, USD, BRL
    nome_moeda VARCHAR(50) NOT NULL,
    tipo VARCHAR(20) NOT NULL,  -- Criptomoeda, Fiduciaria, etc
    data_criacao DATETIME NOT NULL DEFAULT GETDATE(),
    ativo BIT NOT NULL DEFAULT 1
);

-- Nova tabela SALDO_CARTEIRA
CREATE TABLE SALDO_CARTEIRA (
    id INT IDENTITY(1,1) PRIMARY KEY,
    endereco_carteira VARCHAR(100) NOT NULL,
    id_moeda INT NOT NULL,
    saldo DECIMAL(18,8) NOT NULL DEFAULT 0,
    data_atualizacao DATETIME NOT NULL DEFAULT GETDATE(),
    
    -- Foreign Keys
    FOREIGN KEY (endereco_carteira) REFERENCES carteira(endereco_carteira),
    FOREIGN KEY (id_moeda) REFERENCES MOEDA(id),
    
    -- Constraint única: uma carteira só pode ter um saldo por moeda
    UNIQUE (endereco_carteira, id_moeda)
);

-- Nova tabela DEPOSITO_SAQUE
CREATE TABLE DEPOSITO_SAQUE (
    id_movimento INT IDENTITY(1,1) PRIMARY KEY,
    endereco_carteira VARCHAR(100) NOT NULL,
    id_moeda INT NOT NULL,
    tipo VARCHAR(20) NOT NULL,  -- 'DEPOSITO' ou 'SAQUE'
    valor DECIMAL(18,8) NOT NULL,
    taxa_valor DECIMAL(18,8) NOT NULL DEFAULT 0,
    data_hora DATETIME NOT NULL DEFAULT GETDATE(),
    
    -- Foreign Keys
    FOREIGN KEY (endereco_carteira) REFERENCES carteira(endereco_carteira),
    FOREIGN KEY (id_moeda) REFERENCES MOEDA(id),
    
    -- Check constraints
    CHECK (tipo IN ('DEPOSITO', 'SAQUE')),
    CHECK (valor > 0),
    CHECK (taxa_valor >= 0)
);






CREATE TABLE CONVERSAO (
    id_conversao BIGINT IDENTITY(1,1) PRIMARY KEY,
    endereco_carteira VARCHAR(100) NOT NULL,
    id_moeda_origem INT NOT NULL,
    id_moeda_destino INT NOT NULL,
    valor_origem DECIMAL(18,8) NOT NULL,
    valor_destino DECIMAL(18,8) NOT NULL,
    taxa_percentual DECIMAL(18,8) NOT NULL,
    taxa_valor DECIMAL(18,8) NOT NULL,
    cotacao_utilizada DECIMAL(18,8) NOT NULL,
    data_hora DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_CONVERSAO_CARTEIRA FOREIGN KEY (endereco_carteira) REFERENCES carteira(endereco_carteira),
    CONSTRAINT FK_CONVERSAO_MOEDA_ORIGEM FOREIGN KEY (id_moeda_origem) REFERENCES MOEDA(id),
    CONSTRAINT FK_CONVERSAO_MOEDA_DESTINO FOREIGN KEY (id_moeda_destino) REFERENCES MOEDA(id)
    );


CREATE TABLE TRANSFERENCIA(

    id_transferencia BIGINT IDENTITY(1,1) PRIMARY KEY,
    endereco_origem VARCHAR(100) NOT NULL,
    endereco_destino VARCHAR(100) NOT NULL,
    id_moeda INT NOT NULL,
    valor DECIMAL(18,8) NOT NULL,
    taxa_Valor DECIMAL(18,8) NOT NULL,
    data_hora DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_TRANSFERENCIA_CARTEIRA_ORIGEM FOREIGN KEY (endereco_origem) REFERENCES carteira(endereco_carteira),
    CONSTRAINT FK_TRANSFERENCIA_CARETEIRA_DESTINO FOREIGN KEY (endereco_destino) REFERENCES carteira(endereco_carteira),
    CONSTRAINT FK_TRANSFERENCIA_MOEDA FOREIGN KEY(id_moeda) REFERENCES MOEDA(id))
    