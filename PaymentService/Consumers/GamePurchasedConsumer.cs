using MassTransit;
using Microsoft.ApplicationInsights;
using PaymentService.Data;
using PaymentService.Models;
using PaymentService.Services;
using Shared.Contracts;

namespace PaymentService.Consumers;

/// <summary>
/// Consumer que processa compras de jogos e executa pagamentos
/// </summary>
public class GamePurchasedConsumer : IConsumer<IGamePurchased>
{
    private readonly PaymentDbContext _context;
    private readonly UserClient _userClient;
    private readonly GameClient _gameClient;
    private readonly TelemetryClient _telemetry;
    private readonly ILogger<GamePurchasedConsumer> _logger;

    public GamePurchasedConsumer(
        PaymentDbContext context,
        UserClient userClient,
        GameClient gameClient,
        TelemetryClient telemetry,
        ILogger<GamePurchasedConsumer> logger)
    {
        _context = context;
        _userClient = userClient;
        _gameClient = gameClient;
        _telemetry = telemetry;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<IGamePurchased> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Compra recebida via MassTransit: Purchase={PurchaseId}, User={UserId}, Game={GameId}, Amount={Amount}",
            message.PurchaseId, message.UserId, message.GameId, message.GamePrice);

        PaymentStatus paymentStatus;
        string? errorMessage = null;
        Guid paymentId = Guid.NewGuid();

        try
        {
            // Valida usuário (HTTP ainda necessário para validação)
            var user = await _userClient.GetUserByIdAsync(message.UserId);
            if (user == null)
            {
                throw new InvalidOperationException("Usuário não encontrado");
            }

            // Valida jogo
            var game = await _gameClient.GetGameByIdAsync(message.GameId);
            if (game == null)
            {
                throw new InvalidOperationException("Jogo não encontrado");
            }

            // Simula processamento de pagamento
            // Aqui entraria integração com gateway real (Stripe, PagSeguro, etc)
            _logger.LogInformation("Processando pagamento no gateway...");
            await Task.Delay(2000); // Simula latência do gateway

            // Simula sucesso/falha (90% sucesso)
            var random = new Random();
            var success = random.Next(100) < 90;

            if (!success)
            {
                throw new PaymentGatewayException("Cartão recusado pela operadora");
            }

            // Cria registro de pagamento
            var payment = new Payment
            {
                Id = paymentId,
                UserId = message.UserId,
                GameId = message.GameId,
                Amount = message.GamePrice,
                Status = "Paid",
                CreatedAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            paymentStatus = PaymentStatus.Paid;

            _logger.LogInformation("Pagamento processado: Payment={PaymentId}", payment.Id);

            // Registra telemetria
            _telemetry.TrackEvent("PaymentCompleted", new Dictionary<string, string>
            {
                { "PaymentId", payment.Id.ToString() },
                { "PurchaseId", message.PurchaseId.ToString() },
                { "UserId", message.UserId.ToString() },
                { "GameId", message.GameId.ToString() },
                { "Amount", message.GamePrice.ToString() }
            });
        }
        catch (PaymentGatewayException ex)
        {
            _logger.LogWarning(ex, "Pagamento recusado: {ErrorMessage}", ex.Message);
            paymentStatus = PaymentStatus.Failed;
            errorMessage = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar pagamento da compra {PurchaseId}",
                message.PurchaseId);

            paymentStatus = PaymentStatus.Failed;
            errorMessage = "Erro interno ao processar pagamento";

            // MassTransit vai tentar novamente automaticamente (se configurado retry)
            throw;
        }

        // PUBLICA EVENTO de resposta
        await context.Publish<IPaymentProcessed>(new
        {
            PaymentId = paymentId,
            message.PurchaseId,
            message.UserId,
            message.GameId,
            Amount = message.GamePrice,
            Status = paymentStatus,
            ProcessedAt = DateTime.UtcNow,
            ErrorMessage = errorMessage
        });

        _logger.LogInformation(
            "Evento PaymentProcessed publicado: Status={Status}",
            paymentStatus);
    }
}

/// <summary>
/// Exceção customizada para erros de gateway de pagamento
/// </summary>
public class PaymentGatewayException : Exception
{
    public PaymentGatewayException(string message) : base(message) { }
}