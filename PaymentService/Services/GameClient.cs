namespace PaymentService.Services
{
    public class GameClient
    {
        private readonly HttpClient _httpClient;

        public GameClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<GameDto?> GetGameByIdAsync(Guid id)
        {
            var response = await _httpClient.GetAsync($"/api/Games/{id}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) return null; // ou lançar um erro customizado
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<GameDto>();
        }
    }

    public class GameDto
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public decimal Preco { get; set; }
    }

}
