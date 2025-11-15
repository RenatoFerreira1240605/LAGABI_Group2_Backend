using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NeuroNexusBackend.Config;
using NeuroNexusBackend.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NeuroNexusBackend.Services
{
    public class TokenService : ITokenService
    {
        private readonly JwtOptions _options;
        private readonly byte[] _key;

        public TokenService(IOptions<JwtOptions> options)
        {
            _options = options.Value;

            if (string.IsNullOrWhiteSpace(_options.Secret))
                throw new InvalidOperationException("JWT Secret is not configured.");

            _key = Encoding.UTF8.GetBytes(_options.Secret);
        }

        public string GenerateSessionToken(User user)
        {
            var claims = new List<Claim>
            {
                // ID interno do utilizador
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("handle", user.Handle)
            };

            if (!string.IsNullOrWhiteSpace(user.ExternalProvider))
            {
                claims.Add(new Claim("provider", user.ExternalProvider!));
            }

            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                claims.Add(new Claim(ClaimTypes.Email, user.Email!));
            }

            var signingKey = new SymmetricSecurityKey(_key);
            var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var now = DateTime.UtcNow;
            var expires = now.AddMinutes(_options.ExpirationMinutes);

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                notBefore: now,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}