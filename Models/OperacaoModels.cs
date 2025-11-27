using System.ComponentModel.DataAnnotations;

namespace CarteiraDB.Models
{
    public class DepositoRequest
    {
        [Required]
        [StringLength(10)]
        public string CodigoMoeda { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Valor deve ser maior que zero")]
        public decimal Valor { get; set; }
    }

    public class SaqueRequest
    {
        [Required]
        [StringLength(10)]
        public string CodigoMoeda { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Valor deve ser maior que zero")]
        public decimal Valor { get; set; }

        [Required]
        [StringLength(256)]
        public string ChavePrivada { get; set; }
    }

    public class OperacaoResponse
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