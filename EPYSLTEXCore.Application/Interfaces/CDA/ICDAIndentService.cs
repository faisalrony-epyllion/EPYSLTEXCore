using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.CDA;
using EPYSLTEXCore.Infrastructure.Statics;

namespace EPYSLTEXCore.Application.Services
{
    public interface ICDAIndentService 
    {
        Task<CDAIndentMaster> GetDyesChemicalsAsync();

        Task<List<CDAIndentMaster>> GetPagedAsync(Status status, string pageName, PaginationInfo paginationInfo);

        Task<CDAIndentMaster> GetNewAsync(string SubGroupName);

        Task<CDAIndentMaster> GetAsync(int id, string SubGroupName);

        Task<CDAIndentMaster> GetAllAsync(int id);

        Task SaveAsync(CDAIndentMaster entity);

        Task UpdateEntityAsync(CDAIndentMaster entity);
    }
}