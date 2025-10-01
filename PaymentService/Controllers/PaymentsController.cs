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
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var payment = await _service.GetByIdAsync(id);
            return payment == null ? NotFound() : Ok(payment);
        }
    }
}
