-- VERIFICACAO DAS CONFIGURACOES INICIAIS DA CARTEIRA DIGITAL---

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

-- 6. Verificar se tudo foi criado corretamente
SELECT 'CARTEIRA' as Tabela, COUNT(*) as Registros FROM carteira
UNION ALL
SELECT 'MOEDA' as Tabela, COUNT(*) as Registros FROM MOEDA  
UNION ALL
SELECT 'SALDO_CARTEIRA' as Tabela, COUNT(*) as Registros FROM SALDO_CARTEIRA
UNION ALL
SELECT 'DEPOSITO_SAQUE' as Tabela, COUNT(*) as Registros FROM DEPOSITO_SAQUE;


-- Mostrar as moedas cadastradas com seus tipos
SELECT 
    codigo_moeda,
    nome_moeda,
    tipo,
    ativo
FROM MOEDA
ORDER BY tipo, codigo_moeda;

---- Popular tabela MOEDA com as moedas solicitadas ----


INSERT INTO MOEDA (codigo_moeda, nome_moeda, tipo) VALUES 
('BTC', 'Bitcoin', 'Criptomoeda'),
('ETH', 'Ethereum', 'Criptomoeda'),
('SOL', 'Solana', 'Criptomoeda'),
('USD', 'US Dollar', 'Fiduciaria');
