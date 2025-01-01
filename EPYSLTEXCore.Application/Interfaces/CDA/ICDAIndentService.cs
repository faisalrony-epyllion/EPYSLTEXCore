using EPYSLTEX.Core.DTOs;
using EPYSLTEX.Core.Entities.Tex;
using EPYSLTEX.Core.Statics;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPYSLTEX.Core.Interfaces.Services
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