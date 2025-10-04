namespace PaymentService.Services
{
    public class UserClient
    {
        private readonly HttpClient _httpClient;

        public UserClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Exemplo de chamada para buscar usuário pelo ID
        public async Task<UserDto?> GetUserByIdAsync(Guid id)
        {
            var response = await _httpClient.GetAsync($"/api/users/{id}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) return null; // ou lançar um erro customizado
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<UserDto>();
        }
    }

    // DTO usado só para comunicação com o UsersService
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
    }
}
