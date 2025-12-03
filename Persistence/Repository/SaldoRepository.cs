using Microsoft.Data.SqlClient;

namespace CarteiraDB.Persistence.Repository
{
    public class SaldoRepository
    {



        private readonly string _connectionString;
        public SaldoRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public decimal ObterSaldo(string enderecoCarteira, int idMoeda, SqlConnection conn, SqlTransaction transaction)
        {
            using (var cmd = new SqlCommand(@"
        SELECT saldo FROM SALDO_CARTEIRA 
        WHERE endereco_carteira = @endereco AND id_moeda = @idMoeda", conn, transaction))
            {
                cmd.Parameters.AddWithValue("@endereco", enderecoCarteira);
                cmd.Parameters.AddWithValue("@idMoeda", idMoeda);
                var result = cmd.ExecuteScalar();
                if (result == null)
                    throw new ArgumentException("Saldo não encontrado para esta carteira e moeda");
                return Convert.ToDecimal(result);
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


        public void AtualizarSaldo(string enderecoCarteira, int idMoeda, decimal novoSaldo, SqlConnection conn, SqlTransaction transaction)
        {
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
        }









    }
}
