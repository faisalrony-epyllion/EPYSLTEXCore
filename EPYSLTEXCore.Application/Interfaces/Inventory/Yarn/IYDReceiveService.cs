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
    public interface IYDReceiveService
    {
        Task<List<YDReceiveMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo);
        Task<YDReceiveMaster> GetAsync(int ydReceiveMasterID);
        Task<YDReceiveMaster> GetNewAsync(int ydReqIssueMasterID);
        Task<YDReceiveMaster> GetAllAsync(int ydReceiveMasterID);
        Task SaveAsync(YDReceiveMaster entity, int userId);
    }
}
