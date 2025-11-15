using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NeuroNexusBackend.Config;
using NeuroNexusBackend.Data;
using NeuroNexusBackend.Repos;
using NeuroNexusBackend.Services;
using Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite; // <- para UseNetTopologySuite()
using System;

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

            // Controllers + Swagger
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("XUser", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Name = "X-User",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                    Description = "User ID (long) para dev"
                });
                c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    { new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        { Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "XUser" } },
                    Array.Empty<string>() }
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

            app.UseAuthorization();
            app.MapControllers();


            app.Run();
        }
    }
}
