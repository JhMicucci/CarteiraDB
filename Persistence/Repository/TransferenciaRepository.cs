using Microsoft.Data.SqlClient;

namespace CarteiraDB.Persistence.Repository
{
    public class TransferenciaRepository

    {

        private readonly string _connectionString;

        private readonly CarteiraRepository _carteiraRepo;

        private readonly MoedaRepository _moedaRepo;

        private readonly SaldoRepository _saldoRepo;

        public TransferenciaRepository(IConfiguration configuration, MoedaRepository moedaRepo, CarteiraRepository carteiraRepo, SaldoRepository saldoRepo)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _moedaRepo = moedaRepo;
            _carteiraRepo = carteiraRepo;
            _saldoRepo = saldoRepo;
        }



        public Dictionary<string, object> ProcessarTransferencia(string enderecoOrigem, string enderecoDestino, string codigoMoeda, decimal valor, string chavePrivada)
        {
            decimal taxaPercentual = 0.005m; // 0,5%
            decimal taxaValor = valor * taxaPercentual;
            decimal valorDebito = valor + taxaValor;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. Validar chave privada da origem
                        if (_carteiraRepo.ValidarChavePrivada(enderecoOrigem, chavePrivada))
                            throw new UnauthorizedAccessException("Chave privada inválida");

                        // 2. Buscar ID da moeda
                        int idMoeda = _moedaRepo.ObterIdMoeda(codigoMoeda, conn, transaction);

                        // 3. Buscar saldo origem
                        decimal saldoOrigem = _saldoRepo.ObterSaldo(enderecoOrigem, idMoeda, conn, transaction);
                        if (saldoOrigem < valorDebito)
                            throw new InvalidOperationException($"Saldo insuficiente. Saldo atual: {saldoOrigem:F8}, Necessário: {valorDebito:F8}");

                        // 4. Buscar saldo destino
                        decimal saldoDestino = _saldoRepo.ObterSaldo(enderecoDestino, idMoeda, conn, transaction);

                        // 5. Atualizar saldos
                        _saldoRepo.AtualizarSaldo(enderecoOrigem, idMoeda, saldoOrigem - valorDebito, conn, transaction);
                        _saldoRepo.AtualizarSaldo(enderecoDestino, idMoeda, saldoDestino + valor, conn, transaction);

                        // 6. Registrar transferência
                        int idTransferencia;
                        using (var cmd = new SqlCommand(@"
                    INSERT INTO TRANSFERENCIA (endereco_origem, endereco_destino, id_moeda, valor, taxa_valor, data_hora)
                    VALUES (@origem, @destino, @idMoeda, @valor, @taxaValor, GETDATE());
                    SELECT SCOPE_IDENTITY();", conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@origem", enderecoOrigem);
                            cmd.Parameters.AddWithValue("@destino", enderecoDestino);
                            cmd.Parameters.AddWithValue("@idMoeda", idMoeda);
                            cmd.Parameters.AddWithValue("@valor", valor);
                            cmd.Parameters.AddWithValue("@taxaValor", taxaValor);
                            idTransferencia = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        transaction.Commit();

                        return new Dictionary<string, object>
                        {
                            ["id_transferencia"] = idTransferencia,
                            ["endereco_origem"] = enderecoOrigem,
                            ["endereco_destino"] = enderecoDestino,
                            ["codigo_moeda"] = codigoMoeda,
                            ["valor"] = valor,
                            ["taxa_valor"] = taxaValor
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
