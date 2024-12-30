using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Statics;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;

namespace EPYSLTEXCore.Application.Interfaces.Inventory.Yarn
{
    public interface IYDBatchService
    {
        Task<List<YDBatchMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo);

        Task<YDBatchMaster> GetNewAsync(int YDBookingMasterID, int recipeID, string conceptNo, int bookingId, int isBDS, int colorID);

        Task<YDBatchMaster> GetAsync(int id, string conceptNo);

        Task<YDBatchMaster> GetAllAsync(int id);

        Task<YDBatchItemRequirement> GetYDBatchItemRequirementAsync(int id);
        Task<List<YDBatchItemRequirement>> GetOtherItems(PaginationInfo paginationInfo, string yDBookingChildIds, int colorId, int yDBookingMasterID);
        Task SaveAsync(YDBatchMaster entity);
        Task SaveAsyncRecipeCopy(YDBatchMaster entity);
        Task UpdateBDSTNA_YDBatchPreparationPlanAsync(int BatchID);
        Task<List<YDBatchMaster>> GetAllBatchByColorAsync(PaginationInfo paginationInfo, int colorID);
        Task<YDBatchMaster> GetYDBatchNo(int yDBookingMasterID, int colorID);
    }
}
