using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PaymentService.Controllers;
using PaymentService.DTOs;
using PaymentService.Interfaces;
using Xunit;

namespace PaymentServiceTests
{
    public class PaymentsControllerTests
    {
        private readonly Mock<IPaymentService> _serviceMock;
        private readonly PaymentsController _controller;

        public PaymentsControllerTests()
        {
            _serviceMock = new Mock<IPaymentService>();
            _controller = new PaymentsController(_serviceMock.Object);
        }

        [Fact]
        public async Task ProcessPayment_ReturnsOk_WhenPaymentIsProcessed()
        {
            var dto = new PaymentRequestDto();
            var response = new PaymentResponseDto();

            _serviceMock.Setup(s => s.ProcessPaymentAsync(dto)).ReturnsAsync(response);

            var result = await _controller.ProcessPayment(dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(response, okResult.Value);
        }

        [Fact]
        public async Task ProcessPayment_ReturnsNotFound_WhenKeyNotFoundExceptionThrown()
        {
            var dto = new PaymentRequestDto();
            var errorMsg = "User not found";

            _serviceMock.Setup(s => s.ProcessPaymentAsync(dto)).ThrowsAsync(new KeyNotFoundException(errorMsg));

            var result = await _controller.ProcessPayment(dto);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(errorMsg, ((dynamic)notFoundResult.Value).error);
        }

        [Fact]
        public async Task ProcessPayment_ReturnsBadRequest_WhenInvalidOperationExceptionThrown()
        {
            var dto = new PaymentRequestDto();
            var errorMsg = "Invalid payment";

            _serviceMock.Setup(s => s.ProcessPaymentAsync(dto)).ThrowsAsync(new InvalidOperationException(errorMsg));

            var result = await _controller.ProcessPayment(dto);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(errorMsg, ((dynamic)badRequestResult.Value).error);
        }

        [Fact]
        public async Task ProcessPayment_ReturnsServerError_WhenExceptionThrown()
        {
            var dto = new PaymentRequestDto();
            var errorMsg = "Unexpected error";

            _serviceMock.Setup(s => s.ProcessPaymentAsync(dto)).ThrowsAsync(new Exception(errorMsg));

            var result = await _controller.ProcessPayment(dto);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal(errorMsg, ((dynamic)objectResult.Value).error);
        }

        [Fact]
        public async Task GetById_ReturnsOk_WhenPaymentExists()
        {
            var id = Guid.NewGuid();
            var payment = new PaymentResponseDto();

            _serviceMock.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(payment);

            var result = await _controller.GetById(id);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(payment, okResult.Value);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenPaymentDoesNotExist()
        {
            var id = Guid.NewGuid();

            _serviceMock.Setup(s => s.GetByIdAsync(id)).ReturnsAsync((PaymentResponseDto)null);

            var result = await _controller.GetById(id);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetAll_ReturnsOk_WithPaymentsList()
        {
            var payments = new List<PaymentResponseDto> { new PaymentResponseDto() };

            _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(payments);

            var result = await _controller.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(payments, okResult.Value);
        }
    }
}
