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
                    ValidAudience = builder.Configuration["JWT:ValidAudience"],
                    ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:SecretKey"])),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notificationHub"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
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
            services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));

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

            //config spending model category service
            services.AddScoped<ISpendingModelCategoryRepository, SpendingModelCategoryRepository>();

            // config category service
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<ICategoriesRepository, CategoriesRepository>();

            // config sub category service
            services.AddScoped<ISubcategoryService, SubcategoryService>();
            services.AddScoped<ISubcategoryRepository, SubcategoryRepository>();

            // config mail service
            services.AddScoped<IMailService, MailService>();

            // config otp service
            services.AddScoped<IOtpService, OtpService>();

            // config group service
            services.AddScoped<IGroupRepository, GroupRepository>();
            services.AddScoped<IGroupFundsService, GroupFundsService>();


            // config group log service
            services.AddScoped<IGroupFundLogRepository, GroupFundLogRepository>();

            //config group member service
            services.AddScoped<IGroupMemberRepository, GroupMemberRepository>();
            services.AddScoped<IGroupMemberService, GroupMemberService>();

            services.AddSignalR();

            #endregion

            #region config database

            // config database

            services.AddDbContext<MoneyEzContext>(options =>
            {
                //options.UseSqlServer(config.GetConnectionString("MoneyEzLocal"));
                options.UseSqlServer(config.GetConnectionString("MoneyEzDbVps"));
                // options.UseSqlServer(config.GetConnectionString("MoneyEzLocal"));
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
