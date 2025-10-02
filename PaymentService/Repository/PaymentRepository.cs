using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using PaymentService.Interfaces;
using PaymentService.Models;

namespace PaymentService.Repository
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly PaymentDbContext _context;

        public PaymentRepository(PaymentDbContext context) => _context = context;

        public async Task<IEnumerable<Payment>> GetAllAsync() => 
            (IEnumerable<Payment>)await _context.Payments.FindAsync();

        //public async Task<ActionResult<IEnumerable<Payment>>> GetAllAsync()
        //{
        //    var getAllAsync = await _context.Payments.FindAsync();
        //    return getAllAsync;
        //}

        public async Task<Payment?> GetByIdAsync(Guid id) =>
            await _context.Payments.FindAsync(id);

        public async Task AddAsync(Payment payment)
        {
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Payment payment)
        {
            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var payment = await GetByIdAsync(id);
            if (payment != null)
            {
                _context.Payments.Remove(payment);
                await _context.SaveChangesAsync();
            }
        }
    }
}
