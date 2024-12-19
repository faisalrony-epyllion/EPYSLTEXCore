using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.General.Yarn;

namespace EPYSLTEXCore.Application.Interfaces.Admin
{
    public interface IYarnPropertiesMappingService
    {
        Task<List<YarnPropertiesMapping>> GetPagedAsync(PaginationInfo paginationInfo);
        Task<List<YarnPropertiesMapping>> GetAsync(YarnPropertiesMapping entitie);
        Task<YarnPropertiesMapping> GetAsync(int setupID);
        Task SaveAsync(YarnPropertiesMapping entitie);
        Task<YarnPropertiesMapping> GetMaster();
    }
}