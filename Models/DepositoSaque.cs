using System.ComponentModel.DataAnnotations;

namespace CarteiraDB.Models
{
    public class DepositoSaque
    {
        public int IdMovimento { get; set; }

        [Required]
        [StringLength(100)]
        public string EnderecoCarteira { get; set; }

        [Required]
        public int IdMoeda { get; set; }

        [Required]
        [StringLength(20)]
        public string Tipo { get; set; } // "DEPOSITO" ou "SAQUE"

        [Required]
        public decimal Valor { get; set; }

        public decimal TaxaValor { get; set; }

        public DateTime DataHora { get; set; }

        // Propriedades de navegação (opcionais)
        public string? CodigoMoeda { get; set; }
        public string? NomeMoeda { get; set; }
    }
}