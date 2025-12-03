using CarteiraDB.Dtos;
using CarteiraDB.Models;
using CarteiraDB.Persistence.Repository;

namespace CarteiraDB.Service
{
    public class SaldoService
    {

        private readonly SaldoRepository _saldoRepo;

        private readonly CarteiraRepository _carteiraRepo;




        public SaldoService(SaldoRepository saldoRepo, CarteiraRepository carteiraRepo)
        {
            _saldoRepo = saldoRepo;
            _carteiraRepo = carteiraRepo;
        }



        public SaldoMoedaResponseDto BuscarSaldos(string enderecoCarteira)
        {
            // Primeiro verifica se a carteira existe
            var carteira = _carteiraRepo.BuscarPorEndereco(enderecoCarteira);
            if (carteira == null)
                throw new KeyNotFoundException("Carteira não encontrada");

            var saldosRows = _saldoRepo.BuscarSaldos(enderecoCarteira);

            var response = new SaldoMoedaResponseDto
            {
                EnderecoCarteira = enderecoCarteira,
                Saldos = new List<SaldoMoeda>()
            };

            foreach (var row in saldosRows)
            {
                response.Saldos.Add(new SaldoMoeda
                {
                    CodigoMoeda = row["codigo_moeda"].ToString(),
                    NomeMoeda = row["nome_moeda"].ToString(),
                    Tipo = row["tipo"].ToString(),
                    Saldo = Convert.ToDecimal(row["saldo"]),
                    DataAtualizacao = Convert.ToDateTime(row["data_atualizacao"])
                });
            }

            return response;
        }

    }
}
