namespace CarteiraDB.Dtos
{

    public class TransferenciaRequestDto
    {
        public string EnderecoDestino { get; set; } = string.Empty;
        public string CodigoMoeda { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public string ChavePrivada { get; set; } = string.Empty;
    }

    

}
