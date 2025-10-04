using PaymentService.DTOs;
using PaymentService.Interfaces;
using PaymentService.Models;

namespace PaymentService.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _repo;
        //private readonly HttpClient _httpClient;
        private readonly UserClient _userClient;
        private readonly GameClient _gameClient;

        //public PaymentService(IPaymentRepository repo, IHttpClientFactory httpClientFactory)
        //{
        //    _repo = repo;
        //    _httpClient = httpClientFactory.CreateClient();
        //}
        public PaymentService(IPaymentRepository repo, UserClient userClient, GameClient gameClient)
        {
            _repo = repo;
            _userClient = userClient;
            _gameClient = gameClient;
        }

        //public async Task<PaymentResponseDto> ProcessPaymentAsync(PaymentRequestDto dto)
        //{
        //    // 1. Validar usuário
        //    var userResponse = await _httpClient.GetAsync($"https://localhost:7126/api/Users/{dto.UserId}");
        //    if (!userResponse.IsSuccessStatusCode)
        //        throw new Exception("Usuário inválido");

        //    // 2. Validar jogo
        //    var gameResponse = await _httpClient.GetAsync($"https://localhost:7093/api/Games/{dto.GameId}");
        //    if (!gameResponse.IsSuccessStatusCode)
        //        throw new Exception("Jogo inválido ou indisponível");

        //    // 3. Criar pagamento
        //    var payment = new Payment
        //    {
        //        UserId = dto.UserId,
        //        GameId = dto.GameId,
        //        Amount = dto.Amount,
        //        Status = "Paid"
        //    };

        //    await _repo.AddAsync(payment);

        //    return new PaymentResponseDto
        //    {
        //        PaymentId = payment.Id,
        //        UserId = payment.UserId,
        //        GameId = payment.GameId,
        //        Amount = payment.Amount,
        //        Status = payment.Status,
        //        CreatedAt = payment.CreatedAt
        //    };
        //}

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
    }
}
