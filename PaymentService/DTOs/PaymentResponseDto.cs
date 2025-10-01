namespace PaymentService.DTOs
{
    public class PaymentResponseDto
    {
        public Guid PaymentId { get; set; }
        public Guid UserId { get; set; }
        public Guid GameId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; }
    }
}
