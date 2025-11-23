using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NeuroNexusBackend.Config;
using NeuroNexusBackend.Data;
using NeuroNexusBackend.Repos;
using NeuroNexusBackend.Services;
using Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite; // <- para UseNetTopologySuite()
using System;
using System.Text;

namespace NeuroNexusBackend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Connection string: env DATABASE_URL > appsettings > fallback local
            var conn = //Environment.GetEnvironmentVariable("DATABASE_URL") ??
                      builder.Configuration.GetConnectionString("Default")
                      ?? "Host=localhost;Port=5432;Database=neuronexus;Username=postgres;Password=postgres";

            // GoogleAuth Configuration
            builder.Services.Configure<GoogleDeviceOAuthOptions>(
                builder.Configuration.GetSection("GoogleOAuth"));

            // JWTConfiguration
            builder.Services.Configure<JwtOptions>(
            builder.Configuration.GetSection("Jwt"));

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
                {
                    var jwtSection = builder.Configuration.GetSection("Jwt");
                    var secret = jwtSection.GetValue<string>("Secret");
                    var issuer = jwtSection.GetValue<string>("Issuer");
                    var audience = jwtSection.GetValue<string>("Audience");

                    var key = Encoding.UTF8.GetBytes(secret ?? "");

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = issuer,

                        ValidateAudience = true,
                        ValidAudience = audience,

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),

                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromMinutes(1)
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = ctx =>
                        {
                            Console.WriteLine("[JWT] Authentication failed: " + ctx.Exception.Message);
                            if (ctx.Exception.InnerException != null)
                                Console.WriteLine("[JWT] Inner: " + ctx.Exception.InnerException.Message);
                            return Task.CompletedTask;
                        },
                        OnChallenge = ctx =>
                        {
                            Console.WriteLine("[JWT] OnChallenge: " + ctx.Error + " - " + ctx.ErrorDescription);
                            return Task.CompletedTask;
                        }
                    };
                });

            builder.Services.AddHttpClient(); // para usar HttpClient

            // Registo dos serviços (mais abaixo vamos criar estas interfaces/classes)
            builder.Services.AddScoped<IGoogleDeviceAuthService, GoogleDeviceAuthService>();
            builder.Services.AddScoped<ITokenService, TokenService>();

            // DbContext: Npgsql + NetTopologySuite (spatial)
            builder.Services.AddDbContext<AppDbContext>(opt =>
                opt.UseNpgsql(conn, npgsql => npgsql.UseNetTopologySuite()));

            // Repositories
            builder.Services.AddScoped<IUserRepo, UserRepo>();
            builder.Services.AddScoped<IDeckRepo, DeckRepo>();
            builder.Services.AddScoped<ISpawnRepo, SpawnRepo>();
            builder.Services.AddScoped<ICardRepo, CardRepo>();

            // Services
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IDeckService, DeckService>();
            builder.Services.AddScoped<ISpawnService, SpawnService>();
            builder.Services.AddScoped<IMmrService, MmrService>();
            builder.Services.AddScoped<ICardService, CardService>();
            builder.Services.AddScoped<IUserService, UserService>();

            // Controllers + Swagger
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "NeuroNexus API", Version = "v1" });

                var jwtSecurityScheme = new OpenApiSecurityScheme
                {
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Description = "Inserir apenas o token JWT (sem 'Bearer ' no início).",

                    Reference = new OpenApiReference
                    {
                        Id = JwtBearerDefaults.AuthenticationScheme,
                        Type = ReferenceType.SecurityScheme
                    }
                };

                c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { jwtSecurityScheme, Array.Empty<string>() }
                });
            });

            var app = builder.Build();

            // Swagger SEMPRE (inclusive Production na Render)
            app.UseSwagger();
            app.UseSwaggerUI();

            // CORS
            app.UseCors();

            // NÃO forçar HTTPS dentro do container na Render.
            // if (app.Environment.IsDevelopment()) app.UseHttpsRedirection();
            // Caso contrário, remover:
            /// app.UseHttpsRedirection();
            /// 
            app.UseAuthentication();   // IMPORTANTE: antes de UseAuthorization
            app.UseAuthorization();
            app.MapControllers();


            app.Run();
        }
    }
}
