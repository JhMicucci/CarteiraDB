# Exemplos de Uso da API - Carteira DB

## 1. Criar uma Carteira

**POST** `/carteiras`

```bash
curl -X POST "https://localhost:5001/carteiras" \
  -H "Content-Type: application/json"
```

**Resposta:**
```json
{
  "enderecoCarteira": "a1b2c3d4e5f6...",
  "dataCriacao": "2024-01-15T10:00:00",
  "status": 0,
  "chavePrivada": "abc123def456..."
}
```

?? **IMPORTANTE**: A chave privada só é retornada neste momento. Guarde-a com segurança!

## 2. Consultar Saldos

**GET** `/carteiras/{endereco}/saldos`

```bash
curl -X GET "https://localhost:5001/carteiras/a1b2c3d4e5f6.../saldos"
```

**Resposta:**
```json
{
  "enderecoCarteira": "a1b2c3d4e5f6...",
  "saldos": [
    {
      "codigoMoeda": "BTC",
      "nomeMoeda": "Bitcoin",
      "tipo": "Criptomoeda",
      "saldo": 0.00000000,
      "dataAtualizacao": "2024-01-15T10:00:00"
    },
    {
      "codigoMoeda": "ETH",
      "nomeMoeda": "Ethereum", 
      "tipo": "Criptomoeda",
      "saldo": 0.00000000,
      "dataAtualizacao": "2024-01-15T10:00:00"
    },
    {
      "codigoMoeda": "SOL",
      "nomeMoeda": "Solana",
      "tipo": "Criptomoeda", 
      "saldo": 0.00000000,
      "dataAtualizacao": "2024-01-15T10:00:00"
    },
    {
      "codigoMoeda": "USD",
      "nomeMoeda": "US Dollar",
      "tipo": "Fiduciaria",
      "saldo": 0.00000000,
      "dataAtualizacao": "2024-01-15T10:00:00"
    }
  ]
}
```

## 3. Fazer um Depósito

**POST** `/carteiras/{endereco}/depositos`

```bash
curl -X POST "https://localhost:5001/carteiras/a1b2c3d4e5f6.../depositos" \
  -H "Content-Type: application/json" \
  -d '{
    "codigoMoeda": "BTC",
    "valor": 0.01000000
  }'
```

**Resposta:**
```json
{
  "idMovimento": 1,
  "enderecoCarteira": "a1b2c3d4e5f6...",
  "codigoMoeda": "BTC",
  "nomeMoeda": "Bitcoin",
  "tipo": "DEPOSITO",
  "valor": 0.01000000,
  "taxaValor": 0.00000000,
  "saldoAnterior": 0.00000000,
  "saldoAtual": 0.01000000,
  "dataHora": "2024-01-15T14:30:00"
}
```

## 4. Fazer um Saque

**POST** `/carteiras/{endereco}/saques`

```bash
curl -X POST "https://localhost:5001/carteiras/a1b2c3d4e5f6.../saques" \
  -H "Content-Type: application/json" \
  -d '{
    "codigoMoeda": "BTC",
    "valor": 0.005000000,
    "chavePrivada": "abc123def456..."
  }'
```

**Resposta:**
```json
{
  "idMovimento": 2,
  "enderecoCarteira": "a1b2c3d4e5f6...",
  "codigoMoeda": "BTC", 
  "nomeMoeda": "Bitcoin",
  "tipo": "SAQUE",
  "valor": 0.00500000,
  "taxaValor": 0.00005000,
  "saldoAnterior": 0.01000000,
  "saldoAtual": 0.00495000,
  "dataHora": "2024-01-15T14:35:00"
}
```

**Cálculo do Saque:**
- Valor solicitado: 0.00500000 BTC
- Taxa (1%): 0.00005000 BTC  
- Total debitado: 0.00505000 BTC
- Saldo após saque: 0.00495000 BTC

## 5. Casos de Erro

### 5.1 Carteira não encontrada
```json
{
  "error": "Carteira abc123 não encontrada."
}
```

### 5.2 Chave privada inválida
```json
{
  "error": "Chave privada inválida"
}
```

### 5.3 Saldo insuficiente
```json
{
  "error": "Saldo insuficiente. Saldo atual: 0.00100000, Necessário: 0.00505000 (valor + taxa)"
}
```

### 5.4 Moeda inválida
```json
{
  "error": "Moeda não encontrada ou inativa"
}
```

### 5.5 Carteira bloqueada
```json
{
  "error": "Carteira está bloqueada"
}
```

## 6. Fluxo Completo de Teste

```bash
# 1. Criar carteira
CARTEIRA_RESPONSE=$(curl -s -X POST "https://localhost:5001/carteiras")
ENDERECO=$(echo $CARTEIRA_RESPONSE | jq -r '.enderecoCarteira')
CHAVE_PRIVADA=$(echo $CARTEIRA_RESPONSE | jq -r '.chavePrivada')

echo "Carteira criada: $ENDERECO"
echo "Chave privada: $CHAVE_PRIVADA"

# 2. Consultar saldos iniciais
curl -X GET "https://localhost:5001/carteiras/$ENDERECO/saldos"

# 3. Fazer depósito
curl -X POST "https://localhost:5001/carteiras/$ENDERECO/depositos" \
  -H "Content-Type: application/json" \
  -d '{
    "codigoMoeda": "BTC",
    "valor": 0.01000000
  }'

# 4. Consultar saldos após depósito
curl -X GET "https://localhost:5001/carteiras/$ENDERECO/saldos"

# 5. Fazer saque
curl -X POST "https://localhost:5001/carteiras/$ENDERECO/saques" \
  -H "Content-Type: application/json" \
  -d "{
    \"codigoMoeda\": \"BTC\",
    \"valor\": 0.005000000,
    \"chavePrivada\": \"$CHAVE_PRIVADA\"
  }"

# 6. Consultar saldos finais
curl -X GET "https://localhost:5001/carteiras/$ENDERECO/saldos"
```

## 7. Testando no Swagger UI

1. Acesse `https://localhost:5001` 
2. Expanda os endpoints disponíveis
3. Use "Try it out" para testar cada operação
4. Os modelos de requisição são preenchidos automaticamente
5. Copie o endereço da carteira e chave privada do primeiro teste para usar nos demais