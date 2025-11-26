
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
    }
}
