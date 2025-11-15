using NeuroNexusBackend.Models;

namespace NeuroNexusBackend.Services
{
    public interface ITokenService
    {        string GenerateSessionToken(User user);

    }
}
