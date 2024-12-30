using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Statics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;

namespace EPYSLTEXCore.Application.Interfaces.Inventory.Yarn
{
    public interface IYarnRnDReqService
    {
        Task<List<YarnRnDReqMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo, string pageName, bool isReqForYDShow = true);
        Task<List<FreeConceptMRMaster>> GetMRs(string fcIds, PaginationInfo paginationInfo);
        Task<YarnRnDReqMaster> GetAsync(int id, int flag, string requisitionType = null);
        Task<YarnRnDReqMaster> GetAsyncGroupBy(int id, int flag, string requisitionType = null);
        Task<YarnRnDReqMaster> GetReviseAsync(int id, int flag, string mrId);
        Task<YarnRnDReqMaster> GetDetailsAsync(int id);
        Task<YarnRnDReqMaster> GetDetailsForReviseAsync(int id);
        Task SaveAsync(YarnRnDReqMaster entity, int userId);
        Task ReviseAsync(YarnRnDReqMaster entity, int userId);
        Task<YarnRnDReqMaster> GetFreeConceptMRData(string[] fcIds, string Status);
        Task<YarnRnDReqMaster> GetFreeConceptMRDataNew(List<YarnRnDReqMaster> entity);
        Task<YarnRnDReqMaster> GetNewWithoutKnittingInfo();
    }
}
