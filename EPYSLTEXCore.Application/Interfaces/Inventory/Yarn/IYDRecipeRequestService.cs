using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Statics;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;

namespace EPYSLTEXCore.Application.Interfaces.Inventory.Yarn
{
    public interface IYDRecipeRequestService
    {
        Task<List<YDRecipeRequestMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo);
        Task<YDRecipeRequestMaster> GetNewAsync(int ccColorID, int YDBookingChildID, string grpConceptNo, int isBDS, int YDDBatchID = 0);
        Task<YDRecipeRequestMaster> GetAsync(int Id, string groupConceptNo);
        Task<List<YDRecipeRequestChild>> GetItems(string conceptNo, int colorID, int isBDS);
        Task<YDRecipeRequestMaster> GetAllByIDAsync(int ydRecipeRequestMasterID);
        Task SaveAsync(YDRecipeRequestMaster entity);
        Task UpdateEntityAsync(YDRecipeRequestMaster entity);
    }
}
