using System.ComponentModel.DataAnnotations;

namespace CarteiraDB.Models
{
    public class SaldoCarteira
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string EnderecoCarteira { get; set; }

        [Required]
        public int IdMoeda { get; set; }

        [Required]
        public decimal Saldo { get; set; }

        public DateTime DataAtualizacao { get; set; }

    
    }
}