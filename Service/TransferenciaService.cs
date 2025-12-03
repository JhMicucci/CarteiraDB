using CarteiraDB.Dtos;
using CarteiraDB.Models;
using CarteiraDB.Persistence.Repository;

namespace CarteiraDB.Service
{
    public class TransferenciaService
    {

        


        private readonly TransferenciaRepository _transferenciaRepo;

        private readonly CarteiraRepository _carteiraRepo;

        public TransferenciaService(TransferenciaRepository transferenciaRepo, CarteiraRepository carteiraRepo)
        {
            _transferenciaRepo = transferenciaRepo;
            _carteiraRepo = carteiraRepo;
        }





        public OperacaoTransferenciaResponseDto ProcessarTransferencia(string enderecoOrigem, TransferenciaRequestDto request)
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

            var resultado = _transferenciaRepo.ProcessarTransferencia(enderecoOrigem, request.EnderecoDestino, request.CodigoMoeda, request.Valor, request.ChavePrivada);

            return new OperacaoTransferenciaResponseDto
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
