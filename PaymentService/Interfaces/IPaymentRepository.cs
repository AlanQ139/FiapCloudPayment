using PaymentService.Models;

namespace PaymentService.Interfaces
{
    public interface IPaymentRepository
    {
        Task<IEnumerable<Payment>> GetAllAsync();
        Task<Payment?> GetByIdAsync(Guid id);
        Task AddAsync(Payment payment);
        Task UpdateAsync(Payment payment);
        Task DeleteAsync(Guid id);
    }
}
