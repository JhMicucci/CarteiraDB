namespace CarteiraDB.Controllers
{
    using CarteiraDB.Dtos;
    using CarteiraDB.Models;
    using CarteiraDB.Services;
  
    using Microsoft.AspNetCore.Mvc;
    using System.Collections.Generic;

    
        [ApiController]
        [Route("carteiras")]
        public class CarteirasController : ControllerBase
        {
            private readonly CarteiraService _service;

            public CarteirasController(CarteiraService service)
            {
                _service = service;
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
            [ProducesResponseType(typeof(SaldoCarteiraResponse), 200)]
            [ProducesResponseType(404)]
            public ActionResult<SaldoCarteiraResponse> BuscarSaldos(string enderecoCarteira)
            {
                try
                {
                    var saldos = _service.BuscarSaldos(enderecoCarteira);
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
            [ProducesResponseType(typeof(OperacaoResponse), 200)]
            [ProducesResponseType(400)]
            [ProducesResponseType(404)]
            public ActionResult<OperacaoResponse> ProcessarDeposito(string enderecoCarteira, [FromBody] DepositoRequest depositoRequest)
            {
                try
                {
                    if (!ModelState.IsValid)
                        return BadRequest(ModelState);

                    var operacao = _service.ProcessarDeposito(enderecoCarteira, depositoRequest);
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
            [ProducesResponseType(typeof(OperacaoResponse), 200)]
            [ProducesResponseType(400)]
            [ProducesResponseType(401)]
            [ProducesResponseType(404)]
            public ActionResult<OperacaoResponse> ProcessarSaque(string enderecoCarteira, [FromBody] SaqueRequest saqueRequest)
            {
                try
                {
                    if (!ModelState.IsValid)
                        return BadRequest(ModelState);

                    var operacao = _service.ProcessarSaque(enderecoCarteira, saqueRequest);
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
        public ActionResult<OperacaoConversaoResponse> ConverterMoeda(string enderecoCarteira, [FromBody] ConversaoRequest request)
        {
            try
            {
                var resultado = _service.ProcessarConversao(enderecoCarteira, request);
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
        public ActionResult<OperacaoTransferenciaResponse> Transferir(string enderecoOrigem, [FromBody] TransferenciaRequest request)
        {
            try
            {
                var resultado = _service.ProcessarTransferencia(enderecoOrigem, request);
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
