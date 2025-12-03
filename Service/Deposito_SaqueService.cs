using CarteiraDB.Dtos;
using CarteiraDB.Models;
using CarteiraDB.Persistence.Repository;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace CarteiraDB.Service
{
    public class Deposito_SaqueService
    {

        private readonly DepositoRepository _depositoRepo;

        private readonly CarteiraRepository _carteiraRepo;

        private readonly MoedaRepository _moedaRepo;

        private readonly MoedaService _moedaService;

        public Deposito_SaqueService(DepositoRepository depositoRepo, CarteiraRepository carteiraRepo, MoedaRepository moedaRepo, MoedaService moedaService)
        {
            _depositoRepo = depositoRepo;
            _carteiraRepo = carteiraRepo;
            _moedaRepo = moedaRepo;
            _moedaService = moedaService;
        }










        public OperacaoResponseDto ProcessarDeposito(string enderecoCarteira, DepositoRequestDto depositoRequest)
        {
            // Primeiro verifica se a carteira existe
            var carteira = _carteiraRepo.BuscarPorEndereco(enderecoCarteira);
            if (carteira == null)
                throw new KeyNotFoundException("Carteira não encontrada");

            // Verificar se a carteira não está bloqueada
            var status = Enum.Parse<Status>(carteira["status"].ToString());
            if (status == Status.bloqueada)
                throw new InvalidOperationException("Carteira está bloqueada");

            var resultado = _depositoRepo.ProcessarDeposito(enderecoCarteira, depositoRequest.CodigoMoeda, depositoRequest.Valor);

            return new OperacaoResponseDto
            {
                IdMovimento = Convert.ToInt32(resultado["id_movimento"]),
                EnderecoCarteira = resultado["endereco_carteira"].ToString(),
                CodigoMoeda = resultado["codigo_moeda"].ToString(),
                NomeMoeda = _moedaService.ObterNomeMoeda(depositoRequest.CodigoMoeda),
                Tipo = resultado["tipo"].ToString(),
                Valor = Convert.ToDecimal(resultado["valor"]),
                TaxaValor = Convert.ToDecimal(resultado["taxa_valor"]),
                SaldoAnterior = Convert.ToDecimal(resultado["saldo_anterior"]),
                SaldoAtual = Convert.ToDecimal(resultado["saldo_atual"]),
                DataHora = DateTime.Now
            };
        }

        public OperacaoResponseDto ProcessarSaque(string enderecoCarteira, SaqueRequestDto saqueRequest)
        {
            // Primeiro verifica se a carteira existe
            var carteira = _carteiraRepo.BuscarPorEndereco(enderecoCarteira);
            if (carteira == null)
                throw new KeyNotFoundException("Carteira não encontrada");

            // Verificar se a carteira não está bloqueada
            var status = Enum.Parse<Status>(carteira["status"].ToString());
            if (status == Status.bloqueada)
                throw new InvalidOperationException("Carteira está bloqueada");

            var resultado = _depositoRepo.ProcessarSaque(enderecoCarteira, saqueRequest.CodigoMoeda, saqueRequest.Valor, saqueRequest.ChavePrivada);

            return new OperacaoResponseDto
            {
                IdMovimento = Convert.ToInt32(resultado["id_movimento"]),
                EnderecoCarteira = resultado["endereco_carteira"].ToString(),
                CodigoMoeda = resultado["codigo_moeda"].ToString(),
                NomeMoeda = _moedaService.ObterNomeMoeda(saqueRequest.CodigoMoeda),
                Tipo = resultado["tipo"].ToString(),
                Valor = Convert.ToDecimal(resultado["valor"]),
                TaxaValor = Convert.ToDecimal(resultado["taxa_valor"]),
                SaldoAnterior = Convert.ToDecimal(resultado["saldo_anterior"]),
                SaldoAtual = Convert.ToDecimal(resultado["saldo_atual"]),
                DataHora = DateTime.Now
            };
        }
    }
}
