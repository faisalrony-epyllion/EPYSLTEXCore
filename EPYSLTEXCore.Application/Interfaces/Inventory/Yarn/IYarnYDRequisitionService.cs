using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Statics;

namespace EPYSLTEXCore.Application.Interfaces.Inventory.Yarn
{
    public interface IYarnYDRequisitionService
    {
        Task<List<YarnYDReqMaster>> GetPagedAsync(Status status, string pageName, PaginationInfo paginationInfo);
        Task<YarnYDReqMaster> GetNewAsync(int YDBookingMasterID);
        Task<YarnYDReqMaster> GetAsync(int id);
        Task<YarnYDReqMaster> GetAllAsync(int id);
        Task SaveAsync(YarnYDReqMaster entity);
        Task UpdateEntityAsync(YarnYDReqMaster entity);
    }
}
