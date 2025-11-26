
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
