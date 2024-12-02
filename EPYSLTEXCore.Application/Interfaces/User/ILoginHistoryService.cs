using EPYSLTEXCore.Infrastructure.Entities;

namespace EPYSLTEX.Core.Services
{
    public interface ILoginHistoryService
    {
        Task SaveAsync(LoginHistory entity);
        Task<LoginHistory> GetAsync(LoginHistory entity);
    }
}
