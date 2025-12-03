namespace CarteiraDB.Controllers
{
    using CarteiraDB.Dtos;
    using CarteiraDB.Models;
    using CarteiraDB.Service;
    using CarteiraDB.Services;
  
    using Microsoft.AspNetCore.Mvc;
    using System.Collections.Generic;

    
        [ApiController]
        [Route("carteiras")]
        public class CarteirasController : ControllerBase
        {
            private readonly CarteiraService _service;

            private readonly MoedaService _moedaService;

            private readonly SaldoService _saldoService;

            private readonly ConversaoService _conversaoService;

            private readonly Deposito_SaqueService _deposito_SaqueService;

            private readonly TransferenciaService _transferenciaService;




            public CarteirasController(CarteiraService service, MoedaService moedaService, SaldoService saldoService, ConversaoService conversaoService, Deposito_SaqueService deposito_SaqueService, TransferenciaService transferenciaService)
                    {
                        _service = service;
                        _moedaService = moedaService;
                        _saldoService = saldoService;
                        _conversaoService = conversaoService;
                        _deposito_SaqueService = deposito_SaqueService;
                        _transferenciaService = transferenciaService;

            }

            [HttpPost]
            [ProducesResponseType(typeof(CarteiraCriada), 201)]
            public ActionResult<CarteiraCriada> CriarCarteira()
            {
                try
                {
                    var carteira = _service.CriarCarteira();
                    return Created("", carteira);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, ex.Message);
                }
            }

            [HttpGet]
            [ProducesResponseType(typeof(IEnumerable<Carteira>), 200)]
            public ActionResult<IEnumerable<Carteira>> ListarCarteiras()
            {
                return Ok(_service.Listar());
            }

            [HttpGet("{enderecoCarteira}")]
            [ProducesResponseType(typeof(Carteira), 200)]
            [ProducesResponseType(404)]
            public ActionResult<Carteira> BuscarCarteira(string enderecoCarteira)
            {
                try
                {
                    var carteira = _service.BuscarPorEndereco(enderecoCarteira);
                    return Ok(carteira);
                }
                catch (KeyNotFoundException)
                {
                    return NotFound($"Carteira {enderecoCarteira} não encontrada.");
                }
                catch (Exception ex)
                {
                    return StatusCode(500, ex.Message);
                }
            }

            [HttpGet("{enderecoCarteira}/saldos")]
            [ProducesResponseType(typeof(SaldoCarteiraResponseDto), 200)]
            [ProducesResponseType(404)]
            public ActionResult<SaldoCarteiraResponseDto> BuscarSaldos(string enderecoCarteira)
            {
                try
                {
                    var saldos = _saldoService.BuscarSaldos(enderecoCarteira);
                    return Ok(saldos);
                }
                catch (KeyNotFoundException)
                {
                    return NotFound($"Carteira {enderecoCarteira} não encontrada.");
                }
                catch (Exception ex)
                {
                    return StatusCode(500, ex.Message);
                }
            }

            [HttpPost("{enderecoCarteira}/depositos")]
            [ProducesResponseType(typeof(OperacaoResponseDto), 200)]
            [ProducesResponseType(400)]
            [ProducesResponseType(404)]
            public ActionResult<OperacaoResponseDto> ProcessarDeposito(string enderecoCarteira, [FromBody] DepositoRequestDto depositoRequest)
            {
                try
                {
                    if (!ModelState.IsValid)
                        return BadRequest(ModelState);

                    var operacao = _deposito_SaqueService.ProcessarDeposito(enderecoCarteira, depositoRequest);
                    return Ok(operacao);
                }
                catch (KeyNotFoundException)
                {
                    return NotFound($"Carteira {enderecoCarteira} não encontrada.");
                }
                catch (ArgumentException ex)
                {
                    return BadRequest(ex.Message);
                }
                catch (InvalidOperationException ex)
                {
                    return BadRequest(ex.Message);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, ex.Message);
                }
            }

            [HttpPost("{enderecoCarteira}/saques")]
            [ProducesResponseType(typeof(OperacaoResponseDto), 200)]
            [ProducesResponseType(400)]
            [ProducesResponseType(401)]
            [ProducesResponseType(404)]
            public ActionResult<OperacaoResponseDto> ProcessarSaque(string enderecoCarteira, [FromBody] SaqueRequestDto saqueRequest)
            {
                try
                {
                    if (!ModelState.IsValid)
                        return BadRequest(ModelState);

                    var operacao = _deposito_SaqueService.ProcessarSaque(enderecoCarteira, saqueRequest);
                    return Ok(operacao);
                }
                catch (KeyNotFoundException)
                {
                    return NotFound($"Carteira {enderecoCarteira} não encontrada.");
                }
                catch (UnauthorizedAccessException ex)
                {
                    return Unauthorized(ex.Message);
                }
                catch (ArgumentException ex)
                {
                    return BadRequest(ex.Message);
                }
                catch (InvalidOperationException ex)
                {
                    return BadRequest(ex.Message);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, ex.Message);
                }
            }

            [HttpDelete("{enderecoCarteira}")]
            [ProducesResponseType(typeof(Carteira), 200)]
            [ProducesResponseType(404)]
            public ActionResult<Carteira> BloquearCarteira(string enderecoCarteira)
            {
                try
                {
                    var carteira = _service.Bloquear(enderecoCarteira);
                    return Ok(carteira);
                }
                catch (KeyNotFoundException)
                {
                    return NotFound($"Carteira {enderecoCarteira} não encontrada.");
                }
                catch (Exception ex)
                {
                    return StatusCode(500, ex.Message);
                }
            }


        [HttpPost("{enderecoCarteira}/conversoes")]
        public ActionResult<OperacaoConversaoResponseDto> ConverterMoeda(string enderecoCarteira, [FromBody] ConversaoRequestDto request)
        {
            try
            {
                var resultado = _conversaoService.ProcessarConversao(enderecoCarteira, request);
                return Ok(resultado);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensagem = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = "Erro interno", detalhe = ex.Message });
            }
        }

        [HttpPost("{enderecoOrigem}/transferencias")]
        public ActionResult<OperacaoTransferenciaResponseDto> Transferir(string enderecoOrigem, [FromBody] TransferenciaRequestDto request)
        {
            try
            {
                var resultado = _transferenciaService.ProcessarTransferencia(enderecoOrigem, request);
                return Ok(resultado);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensagem = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { mensagem = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = "Erro interno", detalhe = ex.Message });
            }
        }


    }




}
