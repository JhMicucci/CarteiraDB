CREATE TABLE carteira (
    endereco_carteira VARCHAR(100) NOT NULL PRIMARY KEY,  -- PK
    hash_chave_privada VARCHAR(256) NOT NULL,            -- hash da chave privada
    data_criacao DATETIME NOT NULL DEFAULT GETDATE(),    -- data de criação
    status VARCHAR(20) NOT NULL DEFAULT 'ativa'          -- status da carteira
);

-- Nova tabela MOEDA
CREATE TABLE MOEDA (
    id INT IDENTITY(1,1) PRIMARY KEY,
    codigo_moeda VARCHAR(10) NOT NULL UNIQUE,  -- BTC, ETH, SOL, USD
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

-- Popular tabela MOEDA com as moedas solicitadas
INSERT INTO MOEDA (codigo_moeda, nome_moeda, tipo) VALUES 
('BTC', 'Bitcoin', 'Criptomoeda'),
('ETH', 'Ethereum', 'Criptomoeda'),
('SOL', 'Solana', 'Criptomoeda'),
('USD', 'US Dollar', 'Fiduciaria');

