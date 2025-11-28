using CarteiraDB.Dtos;
using CarteiraDB.Models;
using CarteiraDB.Persistence;
using CarteiraDB.Persistence.Repository;
using System;
using System.Collections.Generic;

namespace CarteiraDB.Services
{
    public class CarteiraService
    {
        private readonly CarteiraRepository _carteiraRepo;

        public CarteiraService(CarteiraRepository carteiraRepo)
        {
            _carteiraRepo = carteiraRepo;
        }

        public CarteiraCriada CriarCarteira()
        {
            var row = _carteiraRepo.Criar();
            return new CarteiraCriada
            {
                EnderecoCarteira = row["endereco_carteira"].ToString(),
                DataCriacao = Convert.ToDateTime(row["data_criacao"]),
                Status = Enum.Parse<Status>(row["status"].ToString()),
                ChavePrivada = row["chave_privada"].ToString()
            };
        }

        public Carteira BuscarPorEndereco(string enderecoCarteira)
        {
            var row = _carteiraRepo.BuscarPorEndereco(enderecoCarteira);
            if (row == null)
                throw new KeyNotFoundException("Carteira não encontrada");

            return new Carteira
            {
                EnderecoCarteira = row["endereco_carteira"].ToString(),
                DataCriacao = Convert.ToDateTime(row["data_criacao"]),
                Status = Enum.Parse<Status>(row["status"].ToString())
            };
        }

        public List<Carteira> Listar()
        {
            var rows = _carteiraRepo.Listar();
            var lista = new List<Carteira>();

            foreach (var r in rows)
            {
                lista.Add(new Carteira
                {
                    EnderecoCarteira = r["endereco_carteira"].ToString(),
                    DataCriacao = Convert.ToDateTime(r["data_criacao"]),
                    Status = Enum.Parse<Status>(r["status"].ToString())
                });
            }

            return lista;
        }

        public Carteira Bloquear(string enderecoCarteira)
        {
            var row = _carteiraRepo.AtualizarStatus(enderecoCarteira, "BLOQUEADA");
            if (row == null)
                throw new KeyNotFoundException("Carteira não encontrada");

            return new Carteira
            {
                EnderecoCarteira = row["endereco_carteira"].ToString(),
                DataCriacao = Convert.ToDateTime(row["data_criacao"]),
                Status = Enum.Parse<Status>(row["status"].ToString())
            };
        }

        public SaldoCarteiraResponse BuscarSaldos(string enderecoCarteira)
        {
            // Primeiro verifica se a carteira existe
            var carteira = _carteiraRepo.BuscarPorEndereco(enderecoCarteira);
            if (carteira == null)
                throw new KeyNotFoundException("Carteira não encontrada");

            var saldosRows = _carteiraRepo.BuscarSaldos(enderecoCarteira);
            
            var response = new SaldoCarteiraResponse
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

        public OperacaoResponse ProcessarDeposito(string enderecoCarteira, DepositoRequest depositoRequest)
        {
            // Primeiro verifica se a carteira existe
            var carteira = _carteiraRepo.BuscarPorEndereco(enderecoCarteira);
            if (carteira == null)
                throw new KeyNotFoundException("Carteira não encontrada");

            // Verificar se a carteira não está bloqueada
            var status = Enum.Parse<Status>(carteira["status"].ToString());
            if (status == Status.bloqueada)
                throw new InvalidOperationException("Carteira está bloqueada");

            var resultado = _carteiraRepo.ProcessarDeposito(enderecoCarteira, depositoRequest.CodigoMoeda, depositoRequest.Valor);

            return new OperacaoResponse
            {
                IdMovimento = Convert.ToInt32(resultado["id_movimento"]),
                EnderecoCarteira = resultado["endereco_carteira"].ToString(),
                CodigoMoeda = resultado["codigo_moeda"].ToString(),
                NomeMoeda = ObterNomeMoeda(depositoRequest.CodigoMoeda),
                Tipo = resultado["tipo"].ToString(),
                Valor = Convert.ToDecimal(resultado["valor"]),
                TaxaValor = Convert.ToDecimal(resultado["taxa_valor"]),
                SaldoAnterior = Convert.ToDecimal(resultado["saldo_anterior"]),
                SaldoAtual = Convert.ToDecimal(resultado["saldo_atual"]),
                DataHora = DateTime.Now
            };
        }

        public OperacaoResponse ProcessarSaque(string enderecoCarteira, SaqueRequest saqueRequest)
        {
            // Primeiro verifica se a carteira existe
            var carteira = _carteiraRepo.BuscarPorEndereco(enderecoCarteira);
            if (carteira == null)
                throw new KeyNotFoundException("Carteira não encontrada");

            // Verificar se a carteira não está bloqueada
            var status = Enum.Parse<Status>(carteira["status"].ToString());
            if (status == Status.bloqueada)
                throw new InvalidOperationException("Carteira está bloqueada");

            var resultado = _carteiraRepo.ProcessarSaque(enderecoCarteira, saqueRequest.CodigoMoeda, saqueRequest.Valor, saqueRequest.ChavePrivada);

            return new OperacaoResponse
            {
                IdMovimento = Convert.ToInt32(resultado["id_movimento"]),
                EnderecoCarteira = resultado["endereco_carteira"].ToString(),
                CodigoMoeda = resultado["codigo_moeda"].ToString(),
                NomeMoeda = ObterNomeMoeda(saqueRequest.CodigoMoeda),
                Tipo = resultado["tipo"].ToString(),
                Valor = Convert.ToDecimal(resultado["valor"]),
                TaxaValor = Convert.ToDecimal(resultado["taxa_valor"]),
                SaldoAnterior = Convert.ToDecimal(resultado["saldo_anterior"]),
                SaldoAtual = Convert.ToDecimal(resultado["saldo_atual"]),
                DataHora = DateTime.Now
            };
        }

        private string ObterNomeMoeda(string codigoMoeda)
        {
            return codigoMoeda switch
            {
                "BTC" => "Bitcoin",
                "ETH" => "Ethereum",
                "SOL" => "Solana",
                "USD" => "US Dollar",
                _ => codigoMoeda
            };
        }


        public OperacaoConversaoResponse ProcessarConversao(string enderecoCarteira, ConversaoRequest request)
        {
            var carteira = _carteiraRepo.BuscarPorEndereco(enderecoCarteira);
            if (carteira == null)
                throw new KeyNotFoundException("Carteira não encontrada");

            var status = Enum.Parse<Status>(carteira["status"].ToString());
            if (status == Status.bloqueada)
                throw new InvalidOperationException("Carteira está bloqueada");

            var resultado = _carteiraRepo.ProcessarConversao(enderecoCarteira, request.MoedaOrigem, request.MoedaDestino, request.ValorOrigem);

            return new OperacaoConversaoResponse
            {
                IdConversao = Convert.ToInt32(resultado["id_conversao"]),
                EnderecoCarteira = resultado["endereco_carteira"].ToString(),
                MoedaOrigem = resultado["moeda_origem"].ToString(),
                MoedaDestino = resultado["moeda_destino"].ToString(),
                ValorOrigem = Convert.ToDecimal(resultado["valor_origem"]),
                ValorDestino = Convert.ToDecimal(resultado["valor_destino"]),
                TaxaValor = Convert.ToDecimal(resultado["taxa_valor"]),
                Cotacao = Convert.ToDecimal(resultado["cotacao"]),
                DataHora = DateTime.Now
            };
        }



        public OperacaoTransferenciaResponse ProcessarTransferencia(string enderecoOrigem, TransferenciaRequest request)
        {
            var carteiraOrigem = _carteiraRepo.BuscarPorEndereco(enderecoOrigem);
            if (carteiraOrigem == null)
                throw new KeyNotFoundException("Carteira origem não encontrada");

            var carteiraDestino = _carteiraRepo.BuscarPorEndereco(request.EnderecoDestino);
            if (carteiraDestino == null)
                throw new KeyNotFoundException("Carteira destino não encontrada");

            var status = Enum.Parse<Status>(carteiraOrigem["status"].ToString());
            if (status == Status.bloqueada)
                throw new InvalidOperationException("Carteira origem está bloqueada");

            var resultado = _carteiraRepo.ProcessarTransferencia(enderecoOrigem, request.EnderecoDestino, request.CodigoMoeda, request.Valor, request.ChavePrivada);

            return new OperacaoTransferenciaResponse
            {
                IdTransferencia = Convert.ToInt32(resultado["id_transferencia"]),
                EnderecoOrigem = resultado["endereco_origem"].ToString(),
                EnderecoDestino = resultado["endereco_destino"].ToString(),
                CodigoMoeda = resultado["codigo_moeda"].ToString(),
                Valor = Convert.ToDecimal(resultado["valor"]),
                TaxaValor = Convert.ToDecimal(resultado["taxa_valor"]),
                DataHora = DateTime.Now
            };
        }


    }
}
