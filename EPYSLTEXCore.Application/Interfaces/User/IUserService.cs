using EPYSLTEX.Core.Entities.Gmt;
using EPYSLTEXCore.Application.Entities;

namespace EPYSLTEX.Core.Interfaces.Services
{
    public interface IUserService
    {
        Task<LoginUser> FindUserForLoginAsync(string username);
        Task<LoginUser> FindAsync(string username);

        Task<LoginUser> FindAsync(int userCode);

        LoginUser Find(int userCode);

        Task<bool> IsValidLoginAsync(string username, string password);

        Task<int> UpdatePasswordAsync(int userCode, string password);

        Task SaveAsync(LoginUser user);
    }
}