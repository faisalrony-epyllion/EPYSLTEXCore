using EPYSLTEXCore.Infrastructure.Entities.Tex.Knitting;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Statics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Application.Interfaces.Knitting
{
    public interface IKYReqService
    {
        Task<List<KYReqMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo);
        Task<List<FreeConceptMRMaster>> GetMRs(string fcIds, PaginationInfo paginationInfo);
        Task<KYReqMaster> GetAsync(int id, int flag, Status status);
        Task<KYReqMaster> GetAsyncGroupBy(int id, int flag);
        Task<KYReqMaster> GetReviseAsync(int id, int flag, string mrId);
        Task<KYReqMaster> GetDetailsAsync(int id);
        Task<KYReqMaster> GetDetailsForReviseAsync(int id);
        Task SaveAsync(KYReqMaster entity);
        Task ReviseAsync(KYReqMaster entity);
        Task<KYReqMaster> GetFreeConceptMRData(string[] fcIds, string Status);
        Task<KYReqMaster> GetFreeConceptMRDataNew(List<KYReqMaster> entity);
        Task<KYReqMaster> GetNewWithoutKnittingInfo();
    }
}
