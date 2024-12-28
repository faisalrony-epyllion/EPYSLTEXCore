using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.General.Yarn;

namespace EPYSLTEXCore.Application.Interfaces.Admin
{
    public interface IYarnRMPropertiesService
    {
        Task<List<YarnRMProperties>> GetPagedAsync(PaginationInfo paginationInfo);
        Task<List<YarnRMProperties>> GetAsync(YarnRMProperties entitie);
        Task<YarnRMProperties> GetById(int id);
        Task SaveAsync(YarnRMProperties entitie);
        Task<YarnRMProperties> GetNewAsync();
        Task<YarnRMProperties> GetDetails(int yrmpID);
        Task<bool> CheckDuplicateValue(YarnRMProperties model);
    }
}