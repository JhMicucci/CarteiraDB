namespace CarteiraDB.Dtos
{
    public class OperacaoResponseDto
    {
        public int IdMovimento { get; set; }
        public string EnderecoCarteira { get; set; }
        public string CodigoMoeda { get; set; }
        public string NomeMoeda { get; set; }
        public string Tipo { get; set; }
        public decimal Valor { get; set; }
        public decimal TaxaValor { get; set; }
        public decimal SaldoAnterior { get; set; }
        public decimal SaldoAtual { get; set; }
        public DateTime DataHora { get; set; }
    }
}
