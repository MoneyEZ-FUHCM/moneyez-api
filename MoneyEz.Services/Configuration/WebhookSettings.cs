namespace MoneyEz.Services.Configuration
{
    public class WebhookSettings
    {
        public string EndpointApi { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public string RegistrationEndpoint { get; set; } = string.Empty;

        public string ValidateEndpoint { get; set; } = string.Empty;
    }
}
