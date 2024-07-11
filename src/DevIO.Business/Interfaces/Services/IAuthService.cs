using DevIO.Business.Models.Auth;

namespace DevIO.Business.Interfaces.Services;

public interface IAuthService
{
    Task<UserLogin> CreateAsync(string email, string password);
    Task<UserLogin> LoginAsync(string email, string password);
}