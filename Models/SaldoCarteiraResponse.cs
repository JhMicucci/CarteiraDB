namespace CarteiraDB.Models
{
    public class SaldoCarteiraResponse
    {
        public string EnderecoCarteira { get; set; }
        public List<SaldoMoeda> Saldos { get; set; } = new List<SaldoMoeda>();
    }

    public class SaldoMoeda
    {
        public string CodigoMoeda { get; set; }
        public string NomeMoeda { get; set; }
        public string Tipo { get; set; }
        public decimal Saldo { get; set; }
        public DateTime DataAtualizacao { get; set; }
    }
}