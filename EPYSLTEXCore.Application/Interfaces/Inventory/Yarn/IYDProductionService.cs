using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Statics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Application.Interfaces.Inventory.Yarn
{
    public interface IYDProductionService
    {
        Task<List<YDProductionMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo);
        Task<YDProductionMaster> GetNewAsync(int newId, int ItemMasterID, int colorID, int ydDBatchID);
        Task<YDProductionMaster> GetAsync(int id);
        Task<YDProductionMaster> GetAllAsync(int id);
        Task SaveAsync(YDProductionMaster entity);
        Task UpdateEntityAsync(YDProductionMaster entity);
    }
}
