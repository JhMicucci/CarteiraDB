using System.ComponentModel.DataAnnotations;

namespace CarteiraDB.Models
{
    public class Moeda
    {
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string CodigoMoeda { get; set; }

        [Required]
        [StringLength(50)]
        public string NomeMoeda { get; set; }

        [Required]
        [StringLength(20)]
        public string Tipo { get; set; }

        public DateTime DataCriacao { get; set; }

        public bool Ativo { get; set; }
    }
}