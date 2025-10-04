using System.Net.Http.Headers;

namespace PaymentService.Services
{
    public class BearerTokenHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BearerTokenHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = _httpContextAccessor.HttpContext?.Request?.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrEmpty(token))
            {
                // token já vem como "Bearer xxx"
                request.Headers.Authorization = AuthenticationHeaderValue.Parse(token);
            }
            return base.SendAsync(request, cancellationToken);
        }
    }
}
