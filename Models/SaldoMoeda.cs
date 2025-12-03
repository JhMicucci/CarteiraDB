namespace CarteiraDB.Models
{
    

    public class SaldoMoeda
    {
        public string CodigoMoeda { get; set; }
        public string NomeMoeda { get; set; }
        public string Tipo { get; set; }
        public decimal Saldo { get; set; }
        public DateTime DataAtualizacao { get; set; }
    }
}