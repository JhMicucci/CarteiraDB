using System.ComponentModel.DataAnnotations;

namespace CarteiraDB.Models
{
    public class Carteira
    {


        [Required]
        public string EnderecoCarteira { get; set; }

        [Required]
        public DateTime DataCriacao { get; set; }

        [Required]
        public Status Status { get; set; }

    }
}
