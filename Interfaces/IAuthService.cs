using SecureAPIsPractice.Models;

namespace SecureAPIsPractice.Interfaces
{
    public interface IAuthService
    {
        Task<AuthModel> RegisterAsync(RegisterModel model);
        Task<AuthModel> GetToken(RequestTokenModel model);

        Task<string> AddRoleAsync(AddRoleModel model);

    }
}
