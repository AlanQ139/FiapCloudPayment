using PaymentService.DTOs;

namespace PaymentService.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentResponseDto> ProcessPaymentAsync(PaymentRequestDto dto);
        Task<PaymentResponseDto?> GetByIdAsync(Guid id);
    }
}
