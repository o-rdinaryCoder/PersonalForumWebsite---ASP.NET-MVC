namespace Forum.Services
{

    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;

    public class PostmailService
    {
        private readonly HttpClient _httpClient;
        private readonly string _accessToken;
        private readonly string _apiUrl;

        public PostmailService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            var settings = configuration.GetSection("PostmailSettings");
            _accessToken = settings["AccessToken"];
            _apiUrl = settings["ApiUrl"];
        }

        public async Task<bool> SendEmailAsync(string recipientEmail, string subject, string message)
        {
            var payload = new
            {
                access_token = _accessToken,
                subject = subject,
                text = message,
                recipient = recipientEmail
            };

            var jsonContent = new StringContent(
                Newtonsoft.Json.JsonConvert.SerializeObject(payload),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync(_apiUrl, jsonContent);
            return response.IsSuccessStatusCode;
        }
    }

}
