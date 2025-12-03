using CarteiraDB.Dtos;
using CarteiraDB.Models;
using CarteiraDB.Persistence.Repository;

namespace CarteiraDB.Service
{
    public class ConversaoService
    {

        private readonly ConversaoRepository _conversaoRepo;

        private readonly CarteiraRepository _carteiraRepo;

        public ConversaoService(ConversaoRepository conversaoRepo)
        {
            _conversaoRepo = conversaoRepo;
        }



        public OperacaoConversaoResponseDto ProcessarConversao(string enderecoCarteira, ConversaoRequestDto request)
        {
            var carteira = _carteiraRepo.BuscarPorEndereco(enderecoCarteira);
            if (carteira == null)
                throw new KeyNotFoundException("Carteira não encontrada");

            var status = Enum.Parse<Status>(carteira["status"].ToString());
            if (status == Status.bloqueada)
                throw new InvalidOperationException("Carteira está bloqueada");

            var resultado = _conversaoRepo.ProcessarConversao(enderecoCarteira, request.MoedaOrigem, request.MoedaDestino, request.ValorOrigem);

            return new OperacaoConversaoResponseDto
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

    }
}
