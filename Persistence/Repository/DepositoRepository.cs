using Microsoft.Data.SqlClient;

namespace CarteiraDB.Persistence.Repository
{
    public class DepositoRepository
    {
        private readonly string _connectionString;

        private readonly CarteiraRepository _carteiraRepo;
        public DepositoRepository(IConfiguration configuration, CarteiraRepository carteiraRepo)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _carteiraRepo = carteiraRepo;
        }


        public Dictionary<string, object> ProcessarDeposito(string enderecoCarteira, string codigoMoeda, decimal valor)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. Buscar ID da moeda
                        int idMoeda;
                        using (var cmd = new SqlCommand("SELECT id FROM MOEDA WHERE codigo_moeda = @codigo AND ativo = 1", conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@codigo", codigoMoeda);
                            var result = cmd.ExecuteScalar();
                            if (result == null)
                                throw new ArgumentException("Moeda não encontrada ou inativa");
                            idMoeda = (int)result;
                        }

                        // 2. Buscar saldo atual
                        decimal saldoAnterior;
                        using (var cmd = new SqlCommand(@"
                        SELECT saldo FROM SALDO_CARTEIRA 
                        WHERE endereco_carteira = @endereco AND id_moeda = @idMoeda", conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@endereco", enderecoCarteira);
                            cmd.Parameters.AddWithValue("@idMoeda", idMoeda);
                            var result = cmd.ExecuteScalar();
                            if (result == null)
                                throw new ArgumentException("Saldo não encontrado para esta carteira e moeda");
                            saldoAnterior = (decimal)result;
                        }

                        // 3. Inserir movimento
                        int idMovimento;
                        using (var cmd = new SqlCommand(@"
                        INSERT INTO DEPOSITO_SAQUE (endereco_carteira, id_moeda, tipo, valor, taxa_valor)
                        VALUES (@endereco, @idMoeda, 'DEPOSITO', @valor, 0);
                        SELECT SCOPE_IDENTITY();", conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@endereco", enderecoCarteira);
                            cmd.Parameters.AddWithValue("@idMoeda", idMoeda);
                            cmd.Parameters.AddWithValue("@valor", valor);
                            idMovimento = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        // 4. Atualizar saldo
                        decimal novoSaldo = saldoAnterior + valor;
                        using (var cmd = new SqlCommand(@"
                        UPDATE SALDO_CARTEIRA 
                        SET saldo = @novoSaldo, data_atualizacao = GETDATE()
                        WHERE endereco_carteira = @endereco AND id_moeda = @idMoeda", conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@novoSaldo", novoSaldo);
                            cmd.Parameters.AddWithValue("@endereco", enderecoCarteira);
                            cmd.Parameters.AddWithValue("@idMoeda", idMoeda);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();

                        return new Dictionary<string, object>
                        {
                            ["id_movimento"] = idMovimento,
                            ["endereco_carteira"] = enderecoCarteira,
                            ["codigo_moeda"] = codigoMoeda,
                            ["tipo"] = "DEPOSITO",
                            ["valor"] = valor,
                            ["taxa_valor"] = 0m,
                            ["saldo_anterior"] = saldoAnterior,
                            ["saldo_atual"] = novoSaldo
                        };
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }


        public Dictionary<string, object> ProcessarSaque(string enderecoCarteira, string codigoMoeda, decimal valor, string chavePrivada)
        {
            // Taxa fixa de 1% para saques
            decimal taxaPercentual = 0.01m;
            decimal taxaValor = valor * taxaPercentual;
            decimal valorTotalDebito = valor + taxaValor;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. Validar chave privada
                        if (!_carteiraRepo.ValidarChavePrivada(enderecoCarteira, chavePrivada))
                            throw new UnauthorizedAccessException("Chave privada inválida");

                        // 2. Buscar ID da moeda
                        int idMoeda;
                        using (var cmd = new SqlCommand("SELECT id FROM MOEDA WHERE codigo_moeda = @codigo AND ativo = 1", conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@codigo", codigoMoeda);
                            var result = cmd.ExecuteScalar();
                            if (result == null)
                                throw new ArgumentException("Moeda não encontrada ou inativa");
                            idMoeda = (int)result;
                        }

                        // 3. Buscar saldo atual e validar
                        decimal saldoAnterior;
                        using (var cmd = new SqlCommand(@"
                        SELECT saldo FROM SALDO_CARTEIRA 
                        WHERE endereco_carteira = @endereco AND id_moeda = @idMoeda", conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@endereco", enderecoCarteira);
                            cmd.Parameters.AddWithValue("@idMoeda", idMoeda);
                            var result = cmd.ExecuteScalar();
                            if (result == null)
                                throw new ArgumentException("Saldo não encontrado para esta carteira e moeda");
                            saldoAnterior = (decimal)result;
                        }

                        if (saldoAnterior < valorTotalDebito)
                            throw new InvalidOperationException($"Saldo insuficiente. Saldo atual: {saldoAnterior:F8}, Necessário: {valorTotalDebito:F8} (valor + taxa)");

                        // 4. Inserir movimento
                        int idMovimento;
                        using (var cmd = new SqlCommand(@"
                        INSERT INTO DEPOSITO_SAQUE (endereco_carteira, id_moeda, tipo, valor, taxa_valor)
                        VALUES (@endereco, @idMoeda, 'SAQUE', @valor, @taxaValor);
                        SELECT SCOPE_IDENTITY();", conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@endereco", enderecoCarteira);
                            cmd.Parameters.AddWithValue("@idMoeda", idMoeda);
                            cmd.Parameters.AddWithValue("@valor", valor);
                            cmd.Parameters.AddWithValue("@taxaValor", taxaValor);
                            idMovimento = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        // 5. Atualizar saldo
                        decimal novoSaldo = saldoAnterior - valorTotalDebito;
                        using (var cmd = new SqlCommand(@"
                        UPDATE SALDO_CARTEIRA 
                        SET saldo = @novoSaldo, data_atualizacao = GETDATE()
                        WHERE endereco_carteira = @endereco AND id_moeda = @idMoeda", conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@novoSaldo", novoSaldo);
                            cmd.Parameters.AddWithValue("@endereco", enderecoCarteira);
                            cmd.Parameters.AddWithValue("@idMoeda", idMoeda);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();

                        return new Dictionary<string, object>
                        {
                            ["id_movimento"] = idMovimento,
                            ["endereco_carteira"] = enderecoCarteira,
                            ["codigo_moeda"] = codigoMoeda,
                            ["tipo"] = "SAQUE",
                            ["valor"] = valor,
                            ["taxa_valor"] = taxaValor,
                            ["saldo_anterior"] = saldoAnterior,
                            ["saldo_atual"] = novoSaldo
                        };
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}
