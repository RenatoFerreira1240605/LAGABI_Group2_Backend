using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NeuroNexusBackend.Config;
using NeuroNexusBackend.Data;
using NeuroNexusBackend.DTOs;
using NeuroNexusBackend.Google;
using NeuroNexusBackend.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;

namespace NeuroNexusBackend.Services
{
    public class GoogleDeviceAuthService : IGoogleDeviceAuthService
    {
        private readonly AppDbContext _db;
        private readonly IAuthService _authService;
        private readonly ITokenService _tokenService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly GoogleDeviceOAuthOptions _options;

        private const string DeviceCodeEndpoint = "https://oauth2.googleapis.com/device/code";
        private const string TokenEndpoint = "https://oauth2.googleapis.com/token";

        public GoogleDeviceAuthService(
            AppDbContext db,
            IAuthService authService,
            ITokenService tokenService,
            IHttpClientFactory httpClientFactory,
            IOptions<GoogleDeviceOAuthOptions> options)
        {
            _db = db;
            _authService = authService;
            _tokenService = tokenService;
            _httpClientFactory = httpClientFactory;
            _options = options.Value;
        }

        public async Task<DeviceStartResponseDTO> StartAsync(CancellationToken ct)
        {
            var client = _httpClientFactory.CreateClient();

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string,string>("client_id", _options.ClientId),
                new KeyValuePair<string,string>("scope", "openid email profile")
            });

            using var resp = await client.PostAsync(DeviceCodeEndpoint, content);
            resp.EnsureSuccessStatusCode();

            var json = await resp.Content.ReadAsStringAsync();
            var deviceResp = JsonSerializer.Deserialize<GoogleDeviceCodeResponse>(json)
                             ?? throw new InvalidOperationException("Invalid response from Google device/code.");

            
            var entity = new DeviceLoginRequest
            {
                Id = Guid.NewGuid(),
                DeviceCode = deviceResp.DeviceCode,
                UserCode = deviceResp.UserCode,
                VerificationUrl = deviceResp.VerificationUrl,
                ExpiresAt = DateTime.UtcNow.AddSeconds(deviceResp.ExpiresIn),
                IntervalSeconds = deviceResp.Interval,
                Status = "Pending"
            };

            _db.DeviceLoginRequests.Add(entity);
            await _db.SaveChangesAsync();

            return new DeviceStartResponseDTO
            {
                LoginRequestId = entity.Id,
                UserCode = entity.UserCode,
                VerificationUrl = entity.VerificationUrl,
                ExpiresIn = deviceResp.ExpiresIn,
                PollInterval = deviceResp.Interval
            };
        }

        public async Task<DevicePollResponseDTO> PollAsync(Guid loginRequestId)
        {
            var req = await _db.DeviceLoginRequests
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == loginRequestId);

            if (req == null)
            {
                return new DevicePollResponseDTO
                {
                    Status = "error",
                    Message = "invalid_request"
                };
            }

            if (req.Status == "Expired")
            {
                return new DevicePollResponseDTO { Status = "expired" };
            }

            if (req.Status == "Completed")
            {
                // Idempotente: se já completou, devolve novo token para o mesmo user
                if (req.UserId == null)
                {
                    return new DevicePollResponseDTO
                    {
                        Status = "error",
                        Message = "user_missing"
                    };
                }

                var existingUser = req.User ?? await _db.Users.FirstAsync(u => u.Id == req.UserId.Value);
                var token = _tokenService.GenerateSessionToken(existingUser);

                return new DevicePollResponseDTO
                {
                    Status = "ok",
                    SessionToken = token,
                    DisplayName = existingUser.DisplayName,
                    Email = existingUser.Email,
                    UserId = existingUser.Id
                };
            }

            // Se passou do prazo, marca como expirado
            if (DateTime.UtcNow >= req.ExpiresAt)
            {
                req.Status = "Expired";
                await _db.SaveChangesAsync();
                return new DevicePollResponseDTO { Status = "expired" };
            }

            // Ainda pendente: chamar token endpoint da Google
            var client = _httpClientFactory.CreateClient();

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string,string>("client_id", _options.ClientId),
                new KeyValuePair<string,string>("client_secret", _options.ClientSecret),
                new KeyValuePair<string,string>("device_code", req.DeviceCode),
                new KeyValuePair<string,string>("grant_type", "urn:ietf:params:oauth:grant-type:device_code")
            });

            using var resp = await client.PostAsync(TokenEndpoint, content);
            var json = await resp.Content.ReadAsStringAsync();

            var tokenResp = JsonSerializer.Deserialize<GoogleTokenResponse>(json)
                            ?? throw new InvalidOperationException("Invalid response from Google token endpoint.");

            if (!string.IsNullOrEmpty(tokenResp.Error))
            {
                // Erros esperados do device flow
                if (tokenResp.Error == "authorization_pending" || tokenResp.Error == "slow_down")
                {
                    return new DevicePollResponseDTO { Status = "pending" };
                }

                if (tokenResp.Error == "expired_token")
                {
                    req.Status = "Expired";
                    await _db.SaveChangesAsync();
                    return new DevicePollResponseDTO { Status = "expired" };
                }

                if (tokenResp.Error == "access_denied")
                {
                    req.Status = "Error";
                    await _db.SaveChangesAsync();
                    return new DevicePollResponseDTO
                    {
                        Status = "error",
                        Message = "access_denied"
                    };
                }

                // Outro erro qualquer
                req.Status = "Error";
                await _db.SaveChangesAsync();
                return new DevicePollResponseDTO
                {
                    Status = "error",
                    Message = tokenResp.Error
                };
            }

            if (string.IsNullOrEmpty(tokenResp.IdToken))
            {
                return new DevicePollResponseDTO
                {
                    Status = "error",
                    Message = "id_token_missing"
                };
            }

            // ATENÇÃO: aqui estou apenas a ler o JWT sem validação criptográfica completa.
            // Em produção, deves validar assinatura, issuer e audience com as chaves JWKS da Google.
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(tokenResp.IdToken);

            var sub = jwt.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            var email = jwt.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
            var name = jwt.Claims.FirstOrDefault(c => c.Type == "name")?.Value;

            if (string.IsNullOrEmpty(sub))
            {
                return new DevicePollResponseDTO
                {
                    Status = "error",
                    Message = "sub_missing"
                };
            }

            // Criar / obter utilizador
            var user = await _authService.GetOrCreateExternalUserAsync(
                provider: "google",
                subject: sub,
                email: email,
                displayName: name);

            req.Status = "Completed";
            req.UserId = user.Id;
            await _db.SaveChangesAsync();

            var sessionToken = _tokenService.GenerateSessionToken(user);

            return new DevicePollResponseDTO
            {
                Status = "ok",
                SessionToken = sessionToken,
                DisplayName = user.DisplayName,
                Email = user.Email,
                UserId = user.Id
            };
        }
    }
}
