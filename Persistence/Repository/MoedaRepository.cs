using Microsoft.Data.SqlClient;

namespace CarteiraDB.Persistence.Repository
{
    public class MoedaRepository
    {

        private readonly string _connectionString;

        public MoedaRepository(string connectionString)
        {
            _connectionString = connectionString;
        }



        public int ObterIdMoeda(string codigoMoeda, SqlConnection conn, SqlTransaction transaction)
        {
            using (var cmd = new SqlCommand("SELECT id FROM MOEDA WHERE codigo_moeda = @codigo AND ativo = 1", conn, transaction))
            {
                cmd.Parameters.AddWithValue("@codigo", codigoMoeda);
                var result = cmd.ExecuteScalar();
                if (result == null)
                    throw new ArgumentException($"Moeda {codigoMoeda} não encontrada ou inativa");
                return Convert.ToInt32(result);
            }
        }



        public decimal ObterCotacaoCoinbase(string moedaOrigem, string moedaDestino)
        {
            using (var client = new HttpClient())
            {
                string url = $"https://api.coinbase.com/v2/exchange-rates?currency={moedaOrigem}";
                var response = client.GetAsync(url).Result;
                response.EnsureSuccessStatusCode();

                var json = response.Content.ReadAsStringAsync().Result;
                dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                string rateStr = data.data.rates[moedaDestino];
                return decimal.Parse(rateStr, System.Globalization.CultureInfo.InvariantCulture);
            }
        }
    }
}
