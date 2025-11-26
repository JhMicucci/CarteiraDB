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

                // 2) INSERT
                using (var cmd = new SqlCommand(@"
                INSERT INTO carteira (endereco_carteira, hash_chave_privada)
                VALUES (@endereco, @hash_privada)", conn))
                {
                    cmd.Parameters.AddWithValue("@endereco", endereco);
                    cmd.Parameters.AddWithValue("@hash_privada", hashPrivada);
                    cmd.ExecuteNonQuery();
                }

                // 3) SELECT
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
