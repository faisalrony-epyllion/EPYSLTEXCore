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
    public interface IYarnDyeingSoftWindingService
    {
        Task<List<SoftWindingMaster>> GetPagedAsync(Status status, string pageName, PaginationInfo paginationInfo);
        Task<SoftWindingMaster> GetNewAsync(int YDBatchID);
        Task<SoftWindingMaster> GetAsync(int id);
        Task<SoftWindingMaster> GetAllAsync(int id);
        Task SaveAsync(SoftWindingMaster entity);
        Task UpdateEntityAsync(SoftWindingMaster entity);
    }
}
