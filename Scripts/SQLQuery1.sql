
CREATE TABLE carteira (
    endereco_carteira VARCHAR(100) NOT NULL PRIMARY KEY,  -- PK
    hash_chave_privada VARCHAR(256) NOT NULL,            -- hash da chave privada
    data_criacao DATETIME NOT NULL DEFAULT GETDATE(),    -- data de criação
    status VARCHAR(20) NOT NULL DEFAULT 'ativa'          -- status da carteira
);

