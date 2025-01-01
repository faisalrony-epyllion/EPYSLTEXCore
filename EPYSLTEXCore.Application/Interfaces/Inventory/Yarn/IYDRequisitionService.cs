using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Knitting;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Statics;

namespace EPYSLTEXCore.Application.Interfaces.Inventory.Yarn
{
    public interface IYDRequisitionService
    {
        Task<List<YDReqMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo);
        Task<YDReqMaster> GetNewAsync(int ydBookingMasterId, int isBDS);
        Task<YDReqMaster> GetAsync(int id);
        Task<YDReqMaster> GetAllAsync(int id);
        Task SaveAsync(YDReqMaster entity);
    }
}
