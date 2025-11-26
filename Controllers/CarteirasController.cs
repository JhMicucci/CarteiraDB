namespace CarteiraDB.Controllers
{

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
                var carteira = _service.BuscarPorEndereco(enderecoCarteira);
                if (carteira == null)
                    return NotFound($"Carteira {enderecoCarteira} não encontrada.");
                return Ok(carteira);
            }

            [HttpDelete("{enderecoCarteira}")]
            [ProducesResponseType(typeof(Carteira), 200)]
            [ProducesResponseType(404)]
            public ActionResult<Carteira> BloquearCarteira(string enderecoCarteira)
            {
                var carteira = _service.Bloquear(enderecoCarteira);
                if (carteira == null)
                    return NotFound($"Carteira {enderecoCarteira} não encontrada.");
                return Ok(carteira);
            }
        }
    

}
