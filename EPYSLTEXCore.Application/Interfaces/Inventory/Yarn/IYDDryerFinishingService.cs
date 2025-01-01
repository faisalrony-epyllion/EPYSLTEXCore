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
    public interface IYDDryerFinishingService
    {
        Task<List<YDDryerFinishingMaster>> GetPagedAsync(Status status, string pageName, PaginationInfo paginationInfo);
        Task<YDDryerFinishingMaster> GetNewAsync(int YDBatchID);
        Task<YDDryerFinishingMaster> GetAsync(int id);
        Task<YDDryerFinishingMaster> GetAllAsync(int id);
        Task SaveAsync(YDDryerFinishingMaster entity);
        Task UpdateEntityAsync(YDDryerFinishingMaster entity);
    }
}
