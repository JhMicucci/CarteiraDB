using CarteiraDB.Models;

namespace CarteiraDB.Dtos
{
    
    public class SaldoMoedaResponseDto
    {
        public string EnderecoCarteira { get; set; }
        public List<SaldoMoeda> Saldos { get; set; } = new List<SaldoMoeda>();
    }
}
