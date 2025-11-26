using System.ComponentModel.DataAnnotations;

namespace CarteiraDB.Models
{
    public class CarteiraCriada : Carteira
    {

       
            [Required]
            public string ChavePrivada { get; set; }
        

    }
}
