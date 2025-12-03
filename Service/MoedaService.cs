using CarteiraDB.Persistence.Repository;

namespace CarteiraDB.Service
{
    public class MoedaService
    {

        private readonly MoedaRepository _moedaRepo;

        public MoedaService(MoedaRepository moedaRepo)
        {
            _moedaRepo = moedaRepo;
        }




        public string ObterNomeMoeda(string codigoMoeda)
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
    }
}
