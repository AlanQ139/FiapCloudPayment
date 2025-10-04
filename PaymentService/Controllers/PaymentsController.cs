using Microsoft.AspNetCore.Mvc;
using PaymentService.DTOs;
using PaymentService.Interfaces;

namespace PaymentService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _service;

        public PaymentsController(IPaymentService service) => _service = service;

        [HttpPost]
        public async Task<IActionResult> ProcessPayment(PaymentRequestDto dto)
        {
            try
            {
                var result = await _service.ProcessPaymentAsync(dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) // ex: usuário ou jogo não encontrado
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex) // ex: pagamento inválido
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex) // fallback para erros inesperados
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var payment = await _service.GetByIdAsync(id);
            return payment == null ? NotFound() : Ok(payment);
        }
    }
    public class PaymentRequest
    {
        public Guid UserId { get; set; }
        public Guid GameId { get; set; }
    }
}
