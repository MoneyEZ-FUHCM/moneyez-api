using MoneyEz.Services.BusinessModels.ResultModels;
using MoneyEz.Services.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace MoneyEz.API.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Argument exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex, StatusCodes.Status400BadRequest);
            }
            catch (DefaultException ex)
            {
                _logger.LogWarning("Business exception occurred: {Message}, ErrorCode: {ErrorCode}", 
                    ex.Message, ex.ErrorCode);
                await HandleDefaultExceptionAsync(context, ex, StatusCodes.Status400BadRequest);
            }
            catch (NotExistException ex)
            {
                _logger.LogWarning("Not found exception occurred: {Message}, ErrorCode: {ErrorCode}", 
                    ex.Message, ex.ErrorCode);
                await HandleNotExistExceptionAsync(context, ex, StatusCodes.Status404NotFound);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred: {Message}\nPath: {Path}\nMethod: {Method}", 
                    ex.Message, context.Request.Path, context.Request.Method);
                await HandleExceptionAsync(context, ex, StatusCodes.Status500InternalServerError);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception, int statusCode)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var response = new BaseResultModel
            {
                Status = statusCode,
                Message = exception.Message,
            };

            var jsonResponse = JsonConvert.SerializeObject(response, new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                }
            });
            return context.Response.WriteAsync(jsonResponse);
        }

        private static Task HandleDefaultExceptionAsync(HttpContext context, DefaultException exception, int statusCode)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var response = new BaseResultModel
            {
                Status = statusCode,
                ErrorCode = exception.ErrorCode,
                Message = exception.Message,
            };

            var jsonResponse = JsonConvert.SerializeObject(response, new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                }
            });
            return context.Response.WriteAsync(jsonResponse);
        }

        private static Task HandleNotExistExceptionAsync(HttpContext context, NotExistException exception, int statusCode)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var response = new BaseResultModel
            {
                Status = statusCode,
                ErrorCode = exception.ErrorCode,
                Message = exception.Message,
            };

            var jsonResponse = JsonConvert.SerializeObject(response, new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                }
            });
            return context.Response.WriteAsync(jsonResponse);
        }
    }
}
