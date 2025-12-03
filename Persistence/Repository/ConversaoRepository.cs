using Microsoft.Data.SqlClient;

namespace CarteiraDB.Persistence.Repository
{
    public class ConversaoRepository
    {

        private readonly string _connectionString;
        private readonly MoedaRepository _moedaRepository;
        private readonly SaldoRepository _saldoRepository;
        



        public ConversaoRepository(IConfiguration configuration, MoedaRepository moedaRepository, SaldoRepository saldoRepository)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _moedaRepository = moedaRepository;
            _saldoRepository = saldoRepository;
        }


        public Dictionary<string, object> ProcessarConversao(string enderecoCarteira, string moedaOrigem, string moedaDestino, decimal valorOrigem)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. Buscar IDs das moedas
                        int idMoedaOrigem = _moedaRepository.ObterIdMoeda(moedaOrigem, conn, transaction);
                        int idMoedaDestino = _moedaRepository.ObterIdMoeda(moedaDestino, conn, transaction);

                        // 2. Buscar saldo da moeda de origem
                        decimal saldoOrigem = _saldoRepository.ObterSaldo(enderecoCarteira, idMoedaOrigem, conn, transaction);
                        if (saldoOrigem < valorOrigem)
                            throw new InvalidOperationException("Saldo insuficiente para conversão");

                        // 3. Obter cotação da API Coinbase
                        decimal cotacao = _moedaRepository.ObterCotacaoCoinbase(moedaOrigem, moedaDestino);

                        // 4. Calcular valor convertido e taxa
                        decimal taxaPercentual = 0.005m; // Exemplo: 0,5%
                        decimal taxaValor = valorOrigem * taxaPercentual;
                        decimal valorDestino = (valorOrigem - taxaValor) * cotacao;

                        // 5. Atualizar saldos
                        _saldoRepository.AtualizarSaldo(enderecoCarteira, idMoedaOrigem, saldoOrigem - valorOrigem, conn, transaction);
                        decimal saldoDestino = _saldoRepository.ObterSaldo(enderecoCarteira, idMoedaDestino, conn, transaction);
                        _saldoRepository.AtualizarSaldo(enderecoCarteira, idMoedaDestino, saldoDestino + valorDestino, conn, transaction);

                        // 6. Registrar conversão
                        int idConversao;
                        using (var cmd = new SqlCommand(@"
                    INSERT INTO CONVERSAO (endereco_carteira, id_moeda_origem, id_moeda_destino, valor_origem, valor_destino, taxa_percentual, taxa_valor, cotacao_utilizada, data_hora)
                    VALUES (@endereco, @idOrigem, @idDestino, @valorOrigem, @valorDestino, @taxaPercentual, @taxaValor, @cotacao, GETDATE());
                    SELECT SCOPE_IDENTITY();", conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@endereco", enderecoCarteira);
                            cmd.Parameters.AddWithValue("@idOrigem", idMoedaOrigem);
                            cmd.Parameters.AddWithValue("@idDestino", idMoedaDestino);
                            cmd.Parameters.AddWithValue("@valorOrigem", valorOrigem);
                            cmd.Parameters.AddWithValue("@valorDestino", valorDestino);
                            cmd.Parameters.AddWithValue("@taxaPercentual", taxaPercentual);
                            cmd.Parameters.AddWithValue("@taxaValor", taxaValor);
                            cmd.Parameters.AddWithValue("@cotacao", cotacao);
                            idConversao = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        transaction.Commit();

                        return new Dictionary<string, object>
                        {
                            ["id_conversao"] = idConversao,
                            ["endereco_carteira"] = enderecoCarteira,
                            ["moeda_origem"] = moedaOrigem,
                            ["moeda_destino"] = moedaDestino,
                            ["valor_origem"] = valorOrigem,
                            ["valor_destino"] = valorDestino,
                            ["taxa_valor"] = taxaValor,
                            ["cotacao"] = cotacao
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
