-- Script completo para setup da base de dados
-- Execute este script em ordem na sua instância do SQL Server

-- 1. Criar tabela CARTEIRA (se ainda não existir)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='carteira' AND xtype='U')
BEGIN
    CREATE TABLE carteira (
        endereco_carteira VARCHAR(100) NOT NULL PRIMARY KEY,
        hash_chave_privada VARCHAR(256) NOT NULL,
        data_criacao DATETIME NOT NULL DEFAULT GETDATE(),
        status VARCHAR(20) NOT NULL DEFAULT 'ativa'
    );
    PRINT 'Tabela CARTEIRA criada com sucesso';
END
ELSE
BEGIN
    PRINT 'Tabela CARTEIRA já existe';
END

-- 2. Criar tabela MOEDA
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='MOEDA' AND xtype='U')
BEGIN
    CREATE TABLE MOEDA (
        id INT IDENTITY(1,1) PRIMARY KEY,
        codigo_moeda VARCHAR(10) NOT NULL UNIQUE,
        nome_moeda VARCHAR(50) NOT NULL,
        tipo VARCHAR(20) NOT NULL,  -- Criptomoeda, Fiduciaria, etc
        data_criacao DATETIME NOT NULL DEFAULT GETDATE(),
        ativo BIT NOT NULL DEFAULT 1
    );
    PRINT 'Tabela MOEDA criada com sucesso';
END


-- 3. Criar tabela SALDO_CARTEIRA
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SALDO_CARTEIRA' AND xtype='U')
BEGIN
    CREATE TABLE SALDO_CARTEIRA (
        id INT IDENTITY(1,1) PRIMARY KEY,
        endereco_carteira VARCHAR(100) NOT NULL,
        id_moeda INT NOT NULL,
        saldo DECIMAL(18,8) NOT NULL DEFAULT 0,
        data_atualizacao DATETIME NOT NULL DEFAULT GETDATE(),
        
        FOREIGN KEY (endereco_carteira) REFERENCES carteira(endereco_carteira),
        FOREIGN KEY (id_moeda) REFERENCES MOEDA(id),
        
        UNIQUE (endereco_carteira, id_moeda)
    );
    PRINT 'Tabela SALDO_CARTEIRA criada com sucesso';
END
ELSE
BEGIN
    PRINT 'Tabela SALDO_CARTEIRA já existe';
END

-- 4. Criar tabela DEPOSITO_SAQUE
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='DEPOSITO_SAQUE' AND xtype='U')
BEGIN
    CREATE TABLE DEPOSITO_SAQUE (
        id_movimento INT IDENTITY(1,1) PRIMARY KEY,
        endereco_carteira VARCHAR(100) NOT NULL,
        id_moeda INT NOT NULL,
        tipo VARCHAR(20) NOT NULL,  -- 'DEPOSITO' ou 'SAQUE'
        valor DECIMAL(18,8) NOT NULL,
        taxa_valor DECIMAL(18,8) NOT NULL DEFAULT 0,
        data_hora DATETIME NOT NULL DEFAULT GETDATE(),
        
        FOREIGN KEY (endereco_carteira) REFERENCES carteira(endereco_carteira),
        FOREIGN KEY (id_moeda) REFERENCES MOEDA(id),
        
        CHECK (tipo IN ('DEPOSITO', 'SAQUE')),
        CHECK (valor > 0),
        CHECK (taxa_valor >= 0)
    );
    PRINT 'Tabela DEPOSITO_SAQUE criada com sucesso';
END
ELSE
BEGIN
    PRINT 'Tabela DEPOSITO_SAQUE já existe';
END

-- 5. Popular tabela MOEDA (somente se estiver vazia)
IF NOT EXISTS (SELECT * FROM MOEDA)
BEGIN
    INSERT INTO MOEDA (codigo_moeda, nome_moeda, tipo) VALUES 
    ('BTC', 'Bitcoin', 'Criptomoeda'),
    ('ETH', 'Ethereum', 'Criptomoeda'),
    ('SOL', 'Solana', 'Criptomoeda'),
    ('USD', 'US Dollar', 'Fiduciaria');
    
    PRINT 'Dados iniciais inseridos na tabela MOEDA';
END
ELSE
BEGIN
    -- Se a tabela já tem dados, mas não tem os tipos definidos, vamos atualizar
    IF EXISTS (SELECT * FROM MOEDA WHERE tipo IS NULL OR tipo = '')
    BEGIN
        UPDATE MOEDA SET tipo = 'Criptomoeda' WHERE codigo_moeda IN ('BTC', 'ETH', 'SOL');
        UPDATE MOEDA SET tipo = 'Fiduciaria' WHERE codigo_moeda = 'USD';
        PRINT 'Tipos de moeda atualizados na tabela MOEDA';
    END
    ELSE
    BEGIN
        PRINT 'Tabela MOEDA já contém dados com tipos definidos';
    END
END

-- 6. Verificar se tudo foi criado corretamente
SELECT 'CARTEIRA' as Tabela, COUNT(*) as Registros FROM carteira
UNION ALL
SELECT 'MOEDA' as Tabela, COUNT(*) as Registros FROM MOEDA  
UNION ALL
SELECT 'SALDO_CARTEIRA' as Tabela, COUNT(*) as Registros FROM SALDO_CARTEIRA
UNION ALL
SELECT 'DEPOSITO_SAQUE' as Tabela, COUNT(*) as Registros FROM DEPOSITO_SAQUE;

-- 7. Mostrar as moedas cadastradas com seus tipos
SELECT 
    codigo_moeda,
    nome_moeda,
    tipo,
    ativo
FROM MOEDA
ORDER BY tipo, codigo_moeda;

PRINT 'Setup da base de dados concluído com sucesso!';