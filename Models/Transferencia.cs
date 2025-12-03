namespace CarteiraDB.Models
{
    public class Transferencia


    {


        public int IdTransferencia { get; set; }
        public string EnderecoOrigem { get; set; } = string.Empty;
        public string EnderecoDestino { get; set; } = string.Empty;
        public string CodigoMoeda { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public decimal TaxaValor { get; set; }
        public DateTime DataHora { get; set; }

    }



}
