namespace CarteiraDB.Dtos
{
    public class ConversaoRequest
    {



        
            public string MoedaOrigem { get; set; } = string.Empty;
            public string MoedaDestino { get; set; } = string.Empty;
            public decimal ValorOrigem { get; set; }
        }

        public class OperacaoConversaoResponse
        {
            public int IdConversao { get; set; }
            public string EnderecoCarteira { get; set; } = string.Empty;
            public string MoedaOrigem { get; set; } = string.Empty;
            public string MoedaDestino { get; set; } = string.Empty;
            public decimal ValorOrigem { get; set; }
            public decimal ValorDestino { get; set; }
            public decimal TaxaValor { get; set; }
            public decimal Cotacao { get; set; }
            public DateTime DataHora { get; set; }
        }
    }



