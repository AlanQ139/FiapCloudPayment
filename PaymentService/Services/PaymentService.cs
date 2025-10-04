using Microsoft.EntityFrameworkCore;
using PaymentService.DTOs;
using PaymentService.Interfaces;
using PaymentService.Models;

namespace PaymentService.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _repo;
        private readonly UserClient _userClient;
        private readonly GameClient _gameClient;
        public PaymentService(IPaymentRepository repo, UserClient userClient, GameClient gameClient)
        {
            _repo = repo;
            _userClient = userClient;
            _gameClient = gameClient;
        }
        public async Task<PaymentResponseDto> ProcessPaymentAsync(PaymentRequestDto dto)
        {
            // 1. Validar usuário
            var user = await _userClient.GetUserByIdAsync(dto.UserId);
            if (user == null)
                throw new KeyNotFoundException("Usuário inválido ou não encontrado");

            // 2. Validar jogo
            var game = await _gameClient.GetGameByIdAsync(dto.GameId);
            if (game == null)
                throw new KeyNotFoundException("Jogo inválido ou não encontrado");

            // 3. Criar pagamento
            var payment = new Payment
            {
                UserId = dto.UserId,
                GameId = dto.GameId,
                Amount = dto.Amount,
                Status = "Paid"
            };

            await _repo.AddAsync(payment);

            return new PaymentResponseDto
            {
                PaymentId = payment.Id,
                UserId = payment.UserId,
                GameId = payment.GameId,
                Amount = payment.Amount,
                Status = payment.Status,
                CreatedAt = payment.CreatedAt
            };
        }
        public async Task<PaymentResponseDto?> GetByIdAsync(Guid id)
        {
            var payment = await _repo.GetByIdAsync(id);
            if (payment == null) return null;

            return new PaymentResponseDto
            {
                PaymentId = payment.Id,
                UserId = payment.UserId,
                GameId = payment.GameId,
                Amount = payment.Amount,
                Status = payment.Status,
                CreatedAt = payment.CreatedAt
            };
        }
        public async Task<IEnumerable<PaymentResponseDto>> GetAllAsync()
        {
            var items = await _repo.GetAllAsync();
            return items.Select(p => new PaymentResponseDto
            {
                PaymentId = p.Id,
                UserId = p.UserId,
                GameId = p.GameId,
                Amount = p.Amount,
                Status = p.Status,
                CreatedAt = p.CreatedAt
            });
        }
    }
}
