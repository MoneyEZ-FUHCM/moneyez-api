using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.AspNetCore.Mvc;
using MoneyEz.API;
using MoneyEz.API.Middlewares;
using MoneyEz.Services.BusinessModels.ResultModels;
using MoneyEz.Services.Hubs;
using MoneyEz.Services.Settings;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

DotNetEnv.Env.Load();

builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(m => m.Value.Errors.Count > 0)
            .SelectMany(m => m.Value.Errors)
            .Select(e => e.ErrorMessage)
            .ToList();

        var response = new BaseResultModel
        {
            Status = StatusCodes.Status400BadRequest,
            Message = string.Join("; ", errors)
        };

        return new BadRequestObjectResult(response);
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();

builder.Services.AddWebAPIService(builder);
builder.Services.AddInfractstructure(builder.Configuration);

// config sendgrid
using StreamReader reader = new("sendgrid-config.json");
var json = reader.ReadToEnd();
SendgridConfig sendgrid = JsonConvert.DeserializeObject<SendgridConfig>(json);
builder.Services.Configure<SendgridConfig>(options =>
{
    options.ApiKey = sendgrid.ApiKey;
    options.FromEmail = sendgrid.FromEmail;
    options.FromName = sendgrid.FromName;
});

// config firebase
FirebaseApp.Create(new AppOptions()
{
    Credential = GoogleCredential.FromFile("firebase-adminsdk.json")
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("app-cors");

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.UseWebSockets();

app.MapHub<ChatHub>("/chatHub");

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.Run();
