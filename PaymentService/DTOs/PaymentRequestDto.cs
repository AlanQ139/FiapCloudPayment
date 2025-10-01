namespace PaymentService.DTOs
{
    public class PaymentRequestDto
    {
        public Guid UserId { get; set; }
        public Guid GameId { get; set; }
        public decimal Amount { get; set; }
    }
}
