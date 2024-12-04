using System.Text;
using L_Bank_W_Backend.DbAccess;
using L_Bank_W_Backend.DbAccess.EFRepositories;
using L_Bank_W_Backend.DbAccess.Interfaces;
using L_Bank_W_Backend.DbAccess.interfaces;
using L_Bank_W_Backend.Interfaces;
using L_Bank.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace L_Bank_W_Backend
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(
                    "AllowAll",
                    builder =>
                    {
                        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                    }
                );
            });

            builder.Services.Configure<JwtSettings>(
                builder.Configuration.GetSection("JwtSettings")
            );

            builder.Services.Configure<DatabaseSettings>(
                builder.Configuration.GetSection("DatabaseSettings")
            );

            var dbSettings =
                builder.Configuration.GetSection("DatabaseSettings").Get<DatabaseSettings>()
                ?? throw new InvalidOperationException();

            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options
                    .UseSqlServer(
                        dbSettings.ConnectionString ?? "",
                        x =>
                        {
                            x.MigrationsAssembly("L-Bank.DbAccess");
                        }
                    )
                    .UseSeeding(
                        (context, _) =>
                        {
                            SeedData.Seed(context);
                        }
                    );
            });

            builder
                .Services.AddAuthentication(x =>
                {
                    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(x =>
                {
                    var jwtSettings = builder
                        .Configuration.GetSection("JwtSettings")
                        .Get<JwtSettings>();

                    x.RequireHttpsMetadata = false;
                    x.SaveToken = true;
                    if (jwtSettings?.PrivateKey != null)
                    {
                        x.TokenValidationParameters = new TokenValidationParameters
                        {
                            IssuerSigningKey = new SymmetricSecurityKey(
                                Encoding.ASCII.GetBytes(jwtSettings.PrivateKey)
                            ),
                            ValidateIssuer = false,
                            ValidateAudience = false,
                        };
                    }
                });

            builder.Services.AddAuthorization();

            builder.Services.AddTransient<IEFLedgerRepository, EFLedgerRepository>();
            builder.Services.AddTransient<IEFUserRepository, EFUserRepository>();
            builder.Services.AddTransient<IEFBookingRepository, EFBookingRepository>();
            builder.Services.AddTransient<IEFDepositRepository, EFDepositRepository>();
            builder.Services.AddTransient<IAuthService, AuthService>();
            builder.Services.AddTransient<IBankService, BankService>();

            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo() { Title = "My API - V1", Version = "v1" });

                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = "JWT Authentication",
                    Description = "Enter your JWT token in this field",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                };

                c.AddSecurityDefinition("Bearer", securityScheme);

                var securityRequirement = new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer",
                            },
                        },
                        new string[] { }
                    },
                };

                c.AddSecurityRequirement(securityRequirement);
            });

            var app = builder.Build();
            app.UseCors("AllowAll");

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            else
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                    options.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
                });
            }

            // Configure the HTTP request pipeline.
           // app.UseHttpsRedirection();

            // For index.html
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
