﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MoneyEz.API.RunSchedule.Setup;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Repositories.Implements;
using MoneyEz.Repositories.Repositories.Interfaces;
using MoneyEz.Repositories.UnitOfWork;
using MoneyEz.Services.Configuration;
using MoneyEz.Services.Mappers;
using MoneyEz.Services.Services.Implements;
using MoneyEz.Services.Services.Interfaces;
using MoneyEz.Services.Settings;
using Quartz;
using System.Text;

namespace MoneyEz.API
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddWebAPIService(this IServiceCollection services, WebApplicationBuilder builder)
        {
            // config swagger

            #region config swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "MoneyEz API", Version = "v.1.0" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter a valid token!",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type=ReferenceType.SecurityScheme,
                                Id="Bearer"
                            }
                        },
                        new string[]{}
                    }
                });
            });

            #endregion

            // config authentication

            #region config authen

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["JWT:ValidAudience"],
                    ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:SecretKey"])),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                //options.Events = new JwtBearerEvents
                //{
                //    OnMessageReceived = context =>
                //    {
                //        var accessToken = context.Request.Query["access_token"];
                //        var path = context.HttpContext.Request.Path;
                //        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chatHub"))
                //        {
                //            context.Token = accessToken;
                //        }
                //        return Task.CompletedTask;
                //    }
                //};
            });

            #endregion

            // config CORS

            #region config CORS

            services.AddCors(options =>
            {
                options.AddPolicy("app-cors",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .WithExposedHeaders("X-Pagination")
                        .WithExposedHeaders("X-Webhook-Secret")
                        .WithExposedHeaders("X-External-Secret")
                        .AllowAnyMethod();
                    });
            });

            #endregion

            // config signalR
            #region config signalR
            services.AddSignalR(options =>
            {
                options.KeepAliveInterval = TimeSpan.FromSeconds(30);
                options.ClientTimeoutInterval = TimeSpan.FromMinutes(2);
            });
            #endregion

            // config mail setting
            #region config mail setting
            services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
            #endregion

            // config quartz
            #region config quartz
            services.AddQuartz(option =>
            {
                option.UseMicrosoftDependencyInjectionJobFactory();
            });

            services.AddQuartzHostedService(option =>
            {
                option.WaitForJobsToComplete = true;
            });

            services.ConfigureOptions<SampleJobSetup>();
            services.ConfigureOptions<ScanUserSpendingModelJobSetup>();
            #endregion

            // config webhook setting
            services.Configure<WebhookSettings>(builder.Configuration.GetSection("WebhookSettings"));

            return services;
        }

        public static IServiceCollection AddInfractstructure(this IServiceCollection services, IConfiguration config)
        {

            #region config services

            // config UnitOfWork
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddAutoMapper(typeof(MapperConfig).Assembly);

            // config claim service
            services.AddScoped<IClaimsService, ClaimsService>();

            // config redis service
            services.Configure<RedisSettings>(config.GetSection("RedisSettings"));
            services.AddScoped<IRedisService, RedisService>();

            // config user service
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserService, UserService>();

            //config spending model service
            services.AddScoped<ISpendingModelService, SpendingModelService>();
            services.AddScoped<ISpendingModelRepository, SpendingModelRepository>();

            //config user spending model service
            services.AddScoped<IUserSpendingModelService, UserSpendingModelService>();
            services.AddScoped<IUserSpendingModelRepository, UserSpendingModelRepository>();

            //config financial goal
            services.AddScoped<IFinancialGoalRepository, FinancialGoalRepository>();
            services.AddScoped<IFinancialGoalService, FinancialGoalService>();
            services.AddScoped<IGoalPredictionService, GoalPredictionService>();

            //financial report
            services.AddScoped<IFinancialReportRepository, FinancialReportRepository>();
            services.AddScoped<IFinancialReportService, FinancialReportService>();

            //config spending model category service
            services.AddScoped<ISpendingModelCategoryRepository, SpendingModelCategoryRepository>();

            // config category service
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<ICategoriesRepository, CategoriesRepository>();

            //config subcategory service
            services.AddScoped<ISubcategoryService, SubcategoryService>();
            services.AddScoped<ISubcategoryRepository, SubcategoryRepository>();

            //config categorysubcategory service
            services.AddScoped<ICategorySubcategoryRepository, CategorySubcategoryRepository>();

            // config transaction service
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<ITransactionNotificationService, TransactionNotificationService>();

            // vote
            services.AddScoped<ITransactionVoteRepository, TransactionVoteRepository>();
            // config image service
            services.AddScoped<IImageRepository, ImageRepository>();

            // config mail service
            services.AddScoped<IMailService, MailService>();

            // config otp service
            services.AddScoped<IOtpService, OtpService>();

            // config group service
            services.AddScoped<IGroupFundRepository, GroupRepository>();
            services.AddScoped<IGroupFundsService, GroupFundsService>();

            // config group log service
            services.AddScoped<IGroupFundLogRepository, GroupFundLogRepository>();

            //config group member service
            services.AddScoped<IGroupMemberRepository, GroupMemberRepository>();
            services.AddScoped<IGroupMemberService, GroupMemberService>();

            // config chat service
            services.AddScoped<IChatHistoryRepository, ChatHistoryRepository>();
            services.AddScoped<IChatHistoryService, ChatHistoryService>();
            services.AddScoped<IChatMessageRepository, ChatMessageRepository>();

            //config asset and liability service
            services.AddScoped<IAssetRepository, AssetRepository>();
            services.AddScoped<IAssetService, AssetService>();
            services.AddScoped<ILiabilityRepository, LiabilityRepository>();
            services.AddScoped<ILiabilityService, LiabilityService>();

            // config notification service
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<INotificationService, NotificationService>();

            // config bank account service
            services.AddScoped<IBankAccountRepository, BankAccountRepository>();
            services.AddScoped<IBankAccountService, BankAccountService>();
            // config chat service
            services.AddScoped<IChatService, ChatService>();

            // config external service
            services.AddScoped<IExternalApiService, ExternalApiService>();
            services.AddScoped<IAIKnowledgeService, AIKnowledgeService>();

            // Register HTTP client with optional configuration
            services.AddHttpClient<IExternalApiService, ExternalApiService>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            // Register services
            services.AddScoped<IChatService, ChatService>();

            services.AddSignalR();

            // config webhook service
            services.AddHttpClient("WebhookClient");
            services.AddScoped<IWebhookHttpClient, WebhookHttpClient>();
            services.AddScoped<IWebhookService, WebhookService>();

            // config quiz service
            services.AddScoped<IQuizService, QuizService>();
            services.AddScoped<IQuizRepository, QuizRepository>();
            services.AddScoped<IUserQuizAnswerRepository, UserQuizAnswerRepository>();
            services.AddScoped<IUserQuizResultRepository, UserQuizResultRepository>();
            services.AddScoped<IAnswerOptionRepository, AnswerOptionRepository>();
            services.AddScoped<IQuestionRepository, QuestionRepository>();
            #endregion

            #region config database

            // config database

            services.AddDbContext<MoneyEzContext>(options =>
            {
                //options.UseSqlServer(config.GetConnectionString("MoneyEzLocal"));
                options.UseSqlServer(config.GetConnectionString("MoneyEzDbVps"));
            });

            #endregion

            #region config redis

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = config.GetSection("RedisSettings:RedisConnectionString").Value;
                options.InstanceName = config.GetSection("RedisSettings:InstanceName").Value;
                options.ConfigurationOptions = new StackExchange.Redis.ConfigurationOptions()
                {
                    AbortOnConnectFail = true,
                    EndPoints = { options.Configuration }
                };
            });

            #endregion

            return services;
        }
    }
}
