# Carteira DB - Implementação de Saldos e Operações

## Implementação Realizada

### 1. Criação das Tabelas
- **MOEDA**: Armazena as moedas disponíveis (BTC, ETH, SOL, USD) com classificação por tipo
- **SALDO_CARTEIRA**: Armazena os saldos de cada carteira por moeda
- **DEPOSITO_SAQUE**: Armazena histórico de todas as operações de depósito e saque

### 2. Classificação de Moedas
A tabela MOEDA inclui um campo `tipo` (string) para classificar as moedas:
- **"Criptomoeda"**: Moedas digitais descentralizadas (BTC, ETH, SOL)
- **"Fiduciaria"**: Moedas tradicionais emitidas por governos (USD)

### 3. População Inicial
A tabela MOEDA é automaticamente populada com:
- BTC (Bitcoin) - "Criptomoeda"
- ETH (Ethereum) - "Criptomoeda"
- SOL (Solana) - "Criptomoeda"
- USD (US Dollar) - "Fiduciaria"

### 4. Funcionalidades Implementadas

#### Criação de Carteira
- Gera chave pública e privada
- Armazena apenas o HASH da chave privada
- Cria saldos iniciais (0) para todas as moedas ativas
- Retorna a chave privada apenas no momento da criação

#### Consulta de Saldos
- **GET /carteiras/{endereco}/saldos**
- Retorna todos os saldos de uma carteira específica
- Inclui código da moeda, nome da moeda, tipo da moeda, saldo e data de atualização
- Ordena por tipo e depois por código da moeda

#### Operações de Depósito
- **POST /carteiras/{endereco}/depositos**
- Credita valor sem taxa
- Atualiza saldo automaticamente
- Registra histórico na tabela DEPOSITO_SAQUE

#### Operações de Saque
- **POST /carteiras/{endereco}/saques**
- Debita valor + taxa (1%)
- Validação obrigatória da chave privada (hash)
- Verifica se há saldo suficiente
- Atualiza saldo automaticamente
- Registra histórico na tabela DEPOSITO_SAQUE

### 5. Estrutura das Requisições e Respostas

#### Depósito Request{
  "codigoMoeda": "BTC",
  "valor": 0.01000000
}
#### Saque Request{
  "codigoMoeda": "BTC", 
  "valor": 0.005000000,
  "chavePrivada": "abc123def456..."
}
#### Resposta de Operação{
  "idMovimento": 1,
  "enderecoCarteira": "a1b2c3d4...",
  "codigoMoeda": "BTC",
  "nomeMoeda": "Bitcoin",
  "tipo": "DEPOSITO",
  "valor": 0.01000000,
  "taxaValor": 0.00000000,
  "saldoAnterior": 0.00000000,
  "saldoAtual": 0.01000000,
  "dataHora": "2024-01-15T14:30:00"
}
#### Consulta de Saldos Response{
  "enderecoCarteira": "a1b2c3d4...",
  "saldos": [
    {
      "codigoMoeda": "BTC",
      "nomeMoeda": "Bitcoin",
      "tipo": "Criptomoeda",
      "saldo": 0.01000000,
      "dataAtualizacao": "2024-01-15T14:30:00"
    }
  ]
}
### 6. Como Executar

1. Execute o script SQL em `Scripts/DatabaseSetup.sql` no seu banco de dados (com suporte a migração)
2. Execute a aplicação
3. Acesse o Swagger em: `https://localhost:5001` ou `http://localhost:5000`
4. Use os endpoints disponíveis

### 7. Endpoints Disponíveis

- **POST /carteiras** - Criar nova carteira
- **GET /carteiras** - Listar todas as carteiras
- **GET /carteiras/{endereco}** - Buscar carteira específica
- **GET /carteiras/{endereco}/saldos** - Consultar saldos da carteira
- **POST /carteiras/{endereco}/depositos** - Realizar depósito
- **POST /carteiras/{endereco}/saques** - Realizar saque
- **DELETE /carteiras/{endereco}** - Bloquear carteira

### 8. Modelos Criados

- `Moeda.cs` - Representa uma moeda do sistema com tipo (string)
- `SaldoCarteira.cs` - Representa o saldo de uma carteira em uma moeda
- `SaldoCarteiraResponse.cs` - Modelo de resposta para consulta de saldos
- `SaldoMoeda.cs` - Detalhes do saldo por moeda incluindo tipo
- `DepositoSaque.cs` - Representa uma operação de depósito ou saque
- `OperacaoModels.cs` - Modelos de requisição e resposta para operações

### 9. Validações Implementadas

#### Depósitos
- Carteira deve existir e estar ativa
- Moeda deve ser válida e ativa
- Valor deve ser maior que zero

#### Saques
- Carteira deve existir e estar ativa
- Moeda deve ser válida e ativa
- Valor deve ser maior que zero
- Chave privada deve ser válida (validação por hash)
- Saldo deve ser suficiente para valor + taxa

### 10. Sistema de Taxas

- **Depósitos**: Sem taxa (0%)
- **Saques**: Taxa fixa de 1% sobre o valor sacado
- Taxa é calculada automaticamente e debitada junto com o valor

### 11. Segurança

- Validação obrigatória da chave privada para saques
- Hash SHA256 da chave privada armazenado no banco
- Transações atômicas (rollback em caso de erro)
- Verificação de saldo antes de débito

### 12. Observações Técnicas

- Todas