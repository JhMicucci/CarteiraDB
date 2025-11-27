namespace CarteiraDB.Persistence.Repository
{

    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Security.Cryptography;
    using System.Text;
    using Microsoft.Data.SqlClient;

    public class CarteiraRepository
    {
        private readonly string _connectionString;

        public CarteiraRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public Dictionary<string, object> Criar()
        {
            // 1) Geração das chaves
            int privateKeySize = int.Parse(Environment.GetEnvironmentVariable("PRIVATE_KEY_SIZE") ?? "32");
            int publicKeySize = int.Parse(Environment.GetEnvironmentVariable("PUBLIC_KEY_SIZE") ?? "32");

            string chavePrivada = GerarHexAleatorio(privateKeySize);
            string endereco = GerarHexAleatorio(publicKeySize);
            string hashPrivada = GerarHashSha256(chavePrivada);

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // 2) INSERT carteira
                        using (var cmd = new SqlCommand(@"
                        INSERT INTO carteira (endereco_carteira, hash_chave_privada)
                        VALUES (@endereco, @hash_privada)", conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@endereco", endereco);
                            cmd.Parameters.AddWithValue("@hash_privada", hashPrivada);
                            cmd.ExecuteNonQuery();
                        }

                        // 3) Criar saldos iniciais para todas as moedas
                        using (var cmd = new SqlCommand(@"
                        INSERT INTO SALDO_CARTEIRA (endereco_carteira, id_moeda, saldo)
                        SELECT @endereco, id, 0 FROM MOEDA WHERE ativo = 1", conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@endereco", endereco);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();

                        // 4) SELECT para retornar dados
                        using (var cmd = new SqlCommand(@"
                        SELECT endereco_carteira, data_criacao, status, hash_chave_privada
                        FROM carteira
                        WHERE endereco_carteira = @endereco", conn))
                        {
                            cmd.Parameters.AddWithValue("@endereco", endereco);
                            using (var reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    var carteira = new Dictionary<string, object>
                                    {
                                        ["endereco_carteira"] = reader["endereco_carteira"],
                                        ["data_criacao"] = reader["data_criacao"],
                                        ["status"] = reader["status"],
                                        ["hash_chave_privada"] = reader["hash_chave_privada"],
                                        ["chave_privada"] = chavePrivada
                                    };
                                    return carteira;
                                }
                            }
                        }
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }

            return null!;
        }

        public Dictionary<string, object>? BuscarPorEndereco(string enderecoCarteira)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
                SELECT endereco_carteira, data_criacao, status, hash_chave_privada
                FROM carteira
                WHERE endereco_carteira = @endereco", conn))
                {
                    cmd.Parameters.AddWithValue("@endereco", enderecoCarteira);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Dictionary<string, object>
                            {
                                ["endereco_carteira"] = reader["endereco_carteira"],
                                ["data_criacao"] = reader["data_criacao"],
                                ["status"] = reader["status"],
                                ["hash_chave_privada"] = reader["hash_chave_privada"]
                            };
                        }
                    }
                }
            }
            return null;
        }

        public List<Dictionary<string, object>> Listar()
        {
            var lista = new List<Dictionary<string, object>>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
                SELECT endereco_carteira, data_criacao, status, hash_chave_privada
                FROM carteira", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new Dictionary<string, object>
                            {
                                ["endereco_carteira"] = reader["endereco_carteira"],
                                ["data_criacao"] = reader["data_criacao"],
                                ["status"] = reader["status"],
                                ["hash_chave_privada"] = reader["hash_chave_privada"]
                            });
                        }
                    }
                }
            }
            return lista;
        }

        public Dictionary<string, object>? AtualizarStatus(string enderecoCarteira, string status)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new SqlCommand(@"
                UPDATE carteira SET status = @status
                WHERE endereco_carteira = @endereco", conn))
                {
                    cmd.Parameters.AddWithValue("@status", status);
                    cmd.Parameters.AddWithValue("@endereco", enderecoCarteira);
                    cmd.ExecuteNonQuery();
                }

                return BuscarPorEndereco(enderecoCarteira);
            }
        }

        public List<Dictionary<string, object>> BuscarSaldos(string enderecoCarteira)
        {
            var saldos = new List<Dictionary<string, object>>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
                SELECT sc.endereco_carteira, m.codigo_moeda, m.nome_moeda, m.tipo, sc.saldo, sc.data_atualizacao
                FROM SALDO_CARTEIRA sc
                INNER JOIN MOEDA m ON sc.id_moeda = m.id
                WHERE sc.endereco_carteira = @endereco AND m.ativo = 1
                ORDER BY m.tipo, m.codigo_moeda", conn))
                {
                    cmd.Parameters.AddWithValue("@endereco", enderecoCarteira);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            saldos.Add(new Dictionary<string, object>
                            {
                                ["endereco_carteira"] = reader["endereco_carteira"],
                                ["codigo_moeda"] = reader["codigo_moeda"],
                                ["nome_moeda"] = reader["nome_moeda"],
                                ["tipo"] = reader["tipo"],
                                ["saldo"] = reader["saldo"],
                                ["data_atualizacao"] = reader["data_atualizacao"]
                            });
                        }
                    }
                }
            }
            return saldos;
        }

        public bool ValidarChavePrivada(string enderecoCarteira, string chavePrivada)
        {
            string hashFornecido = GerarHashSha256(chavePrivada);
            
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
                SELECT COUNT(*) FROM carteira 
                WHERE endereco_carteira = @endereco AND hash_chave_privada = @hash", conn))
                {
                    cmd.Parameters.AddWithValue("@endereco", enderecoCarteira);
                    cmd.Parameters.AddWithValue("@hash", hashFornecido);
                    
                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
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
                        if (!ValidarChavePrivada(enderecoCarteira, chavePrivada))
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

        
        private string GerarHexAleatorio(int tamanhoBytes)
        {
            byte[] buffer = new byte[tamanhoBytes];
            RandomNumberGenerator.Fill(buffer);
            return BitConverter.ToString(buffer).Replace("-", "").ToLower();
        }

        private string GerarHashSha256(string texto)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(texto));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }
    }

}
