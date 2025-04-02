using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MoneyEz.Services.Configuration;
using MoneyEz.Services.BusinessModels.WebhookModels;
using MoneyEz.Services.Services.Interfaces;

namespace MoneyEz.Services.Services.Implements
{
    public class WebhookHttpClient : IWebhookHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly WebhookSettings _settings;

        public WebhookHttpClient(
            IHttpClientFactory httpClientFactory,
            IOptions<WebhookSettings> settings)
        {
            _settings = settings.Value;
            _httpClient = httpClientFactory.CreateClient("WebhookClient");
            
            _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
        }

        public async Task<HttpResponseMessage> CancelWebhookAsync(string secret)
        {
            var endpoint = $"{_settings.BaseUrl}{_settings.RegistrationEndpoint}/cancel/{secret}";
            return  await _httpClient.DeleteAsync(endpoint);
        }

        public async Task<HttpResponseMessage> RegisterWebhookAsync(WebhookRequestModel request)
        {
            return await _httpClient.PostAsJsonAsync(_settings.RegistrationEndpoint, request);
        }
    }
}
