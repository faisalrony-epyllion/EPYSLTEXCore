using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Statics;

namespace EPYSLTEXCore.Application.Interfaces.Inventory.Yarn
{
    public interface IYDLeftOverReturnService
    {
        Task<List<YDLeftOverReturnMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo);

        Task<YDLeftOverReturnMaster> GetNewAsync(string YDReqIssueMasterID);

        Task<YDLeftOverReturnMaster> GetAsync(int id);

        Task<YDLeftOverReturnMaster> GetAllByIDAsync(int id);

        Task SaveAsync(YDLeftOverReturnMaster entity);
    }
}
