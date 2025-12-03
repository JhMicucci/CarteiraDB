using System.ComponentModel.DataAnnotations;

namespace CarteiraDB.Dtos
{
    public class SaqueRequestDto
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
}
