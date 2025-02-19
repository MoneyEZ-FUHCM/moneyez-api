using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Repositories.Implements;
using MoneyEz.Repositories.Repositories.Interfaces;
using MoneyEz.Repositories.UnitOfWork;
using MoneyEz.Services.Mappers;
using MoneyEz.Services.Services.Implements;
using MoneyEz.Services.Services.Interfaces;
using MoneyEz.Services.Settings;
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
                    ValidAudience = Environment.GetEnvironmentVariable("JWT__ValidAudience"),
                    ValidIssuer = Environment.GetEnvironmentVariable("JWT__ValidIssuer"),
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT__SecretKey"))),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
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
                        .AllowAnyMethod();
                    });
            });

            #endregion

            // config signalR
            services.AddSignalR(options =>
            {
                options.KeepAliveInterval = TimeSpan.FromSeconds(30);
                options.ClientTimeoutInterval = TimeSpan.FromMinutes(2);
            });

            // config mail setting
            services.Configure<MailSettings>(options =>
            {
                options.Mail = Environment.GetEnvironmentVariable("MailSettings__Mail");
                options.DisplayName = Environment.GetEnvironmentVariable("MailSettings__DisplayName");
                options.Password = Environment.GetEnvironmentVariable("MailSettings__Password");
                options.Host = Environment.GetEnvironmentVariable("MailSettings__Host");
                options.Port = int.Parse(Environment.GetEnvironmentVariable("MailSettings__Port"));
            });

            // config redis setting
            services.Configure<RedisSettings>(options =>
            {
                options.ConnectionString = Environment.GetEnvironmentVariable("RedisSettings__RedisConnectionString");
                options.InstanceName = Environment.GetEnvironmentVariable("RedisSettings__InstanceName");
                options.DefaultExpiryMinutes = int.Parse(Environment.GetEnvironmentVariable("RedisSettings__DefaultExpiryMinutes"));
            });


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
            services.AddScoped<IRedisService, RedisService>();

            // config user service
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserService, UserService>();

            //config spending model service
            services.AddScoped<ISpendingModelService, SpendingModelService>();
            services.AddScoped<ISpendingModelRepository, SpendingModelRepository>();

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
            services.AddScoped<IGroupMemberLogRepository, GroupMemberLogRepository>();

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

            services.AddSignalR();

            #endregion

            #region config database

            // config database

            services.AddDbContext<MoneyEzContext>(options =>
            {
                //options.UseSqlServer(Environment.GetEnvironmentVariable("ConnectionStrings__MoneyEzLocal"));
                options.UseSqlServer(Environment.GetEnvironmentVariable("ConnectionStrings__MoneyEzDbVps"));
            });

            #endregion

            #region config redis

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = Environment.GetEnvironmentVariable("RedisSettings__RedisConnectionString");
                options.InstanceName = Environment.GetEnvironmentVariable("RedisSettings__InstanceName");
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
