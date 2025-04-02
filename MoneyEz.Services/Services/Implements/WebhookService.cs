using MoneyEz.Services.BusinessModels.ResultModels;
using MoneyEz.Services.BusinessModels.WebhookModels;
using MoneyEz.Services.Services.Interfaces;
using MoneyEz.Services.Utils;
using MoneyEz.Repositories.UnitOfWork;
using System.Net;
using Microsoft.AspNetCore.Http;
using MoneyEz.Services.Exceptions;
using MoneyEz.Services.Constants;
using Microsoft.Extensions.Options;
using MoneyEz.Services.Settings;
using MoneyEz.Services.Configuration;
using System.Runtime;

namespace MoneyEz.Services.Services.Implements
{
    public class WebhookService : IWebhookService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebhookHttpClient _webhookClient;
        private readonly WebhookSettings _settings;

        public WebhookService(IUnitOfWork unitOfWork, IWebhookHttpClient webhookClient, IOptions<WebhookSettings> settings)
        {
            _unitOfWork = unitOfWork;
            _webhookClient = webhookClient;
            _settings = settings.Value;
        }

        public async Task<BaseResultModel> CancelWebhookAsync(Guid accountBankId, string serverUri)
        {
            // Find the bank account
            var bankAccount = await _unitOfWork.BankAccountRepository.GetByIdAsync(accountBankId);
            if (bankAccount == null)
            {
                throw new NotExistException("Bank account not found", MessageConstants.BANK_ACCOUNT_NOT_FOUND);
            }

            // Check if the bank account has a webhook secret key (means it has a webhook registered)
            if (string.IsNullOrEmpty(bankAccount.WebhookSecretKey))
            {
                throw new DefaultException("No webhook is registered for this bank account", MessageConstants.WEBHOOK_NOT_REGISTERED);
            }

            // Build webhook cancellation request
            var secretKey = bankAccount.WebhookSecretKey;

            // Call webhook cancellation endpoint
            var response = await _webhookClient.CancelWebhookAsync(secretKey);

            if (response.IsSuccessStatusCode)
            {
                // Update bank account to remove webhook information
                bankAccount.WebhookSecretKey = null;
                bankAccount.WebhookUrl = null;
                _unitOfWork.BankAccountRepository.UpdateAsync(bankAccount);
                await _unitOfWork.SaveAsync();

                return new BaseResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Message = "Webhook cancelled successfully."
                };
            }
            else
            {
                // Read the error response
                var errorMessage = await response.Content.ReadAsStringAsync();

                return new BaseResultModel
                {
                    Status = (int)response.StatusCode,
                    ErrorCode = MessageConstants.WEBHOOK_CANCELLATION_FAILED,
                    Message = $"Failed to cancel webhook: {errorMessage}"
                };
            }

            throw new DefaultException("Failed to cancel webhook", MessageConstants.WEBHOOK_CANCELLATION_FAILED);
        }

        public async Task<BaseResultModel> RegisterWebhookAsync(Guid accountBankId, string serverUri)
        {
            // Get bank account
            var bankAccount = await _unitOfWork.BankAccountRepository.GetByIdAsync(accountBankId)
                ?? throw new NotExistException(MessageConstants.BANK_ACCOUNT_NOT_FOUND);

            if (bankAccount.BankShortName != "EZB")
            {
                throw new DefaultException("Webhook is not supported for this bank", MessageConstants.WEBHOOK_NOT_SUPPORTED);
            }

            // Generate secret key
            string secretKey = SecretKeyGenerator.GenerateSecretKey();
            string webhookUrl = _settings.BaseUrl;

            // Create webhook request
            var webhookRequest = new WebhookRequestModel
            {
                Url = $"https://{serverUri}{_settings.EndpointApi}",
                Secret = secretKey,
                AccountNumber = bankAccount.AccountNumber,
                AccountHolder = bankAccount.AccountHolderName
            };

            // Send registration request
            var response = await _webhookClient.RegisterWebhookAsync(webhookRequest);

            if (response.IsSuccessStatusCode)
            {
                // Update bank account with secret key
                bankAccount.WebhookSecretKey = secretKey;
                bankAccount.WebhookUrl = webhookUrl;
                _unitOfWork.BankAccountRepository.UpdateAsync(bankAccount);
                await _unitOfWork.SaveAsync();

                return new BaseResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Message = "Webhook registered successfully"
                };
            }

            throw new DefaultException($"Failed to register webhook: {await response.Content.ReadAsStringAsync()}",
                MessageConstants.WEBHOOK_REGISTRATION_FAILED);
        }
    }
}
