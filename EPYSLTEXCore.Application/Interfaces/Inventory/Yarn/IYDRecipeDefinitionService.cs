using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Statics;

namespace EPYSLTEXCore.Application.Interfaces.Inventory.Yarn
{
    public interface IYDRecipeDefinitionService
    {
        Task<List<YDRecipeDefinitionMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo); //YDRecipeReqMasterID

        Task<YDRecipeDefinitionMaster> GetNewAsync(int Id);

        Task<YDRecipeDefinitionMaster> GetAsync(int Id);

        Task SaveAsync(YDRecipeDefinitionMaster entity);

        Task<YDRecipeDefinitionMaster> GetAllByIDAsync(int id);
        Task<List<YDRecipeDefinitionMaster>> GetByRecipeReqNo(string recipeReqNo);

        Task UpdateEntityAsync(YDRecipeDefinitionMaster entity);
        Task UpdateRecipeId(YDRecipeDefinitionMaster entity, int YDDBatchID);
        Task UpdateRecipeWithBatchAsync(YDRecipeDefinitionMaster entity, YDDyeingBatchMaster dyeingBatchEntity, List<YDBatchMaster> batchEntities);

        Task<List<YDBatchMaster>> GetBatchDetails(string batchIds);

        Task<List<YDRecipeDefinitionMaster>> GetAllApproveListForCopy(string dpID, string buyer, string fabricComposition, string color, PaginationInfo paginationInfo);

        Task<List<YDRecipeDefinitionMaster>> GetConceptWiseRecipeForCopy(string fabricComposition, string color, string groupConceptNo, PaginationInfo paginationInfo);
        Task<YDRecipeDefinitionMaster> GetRecipeDyeingInfo(int Id);
        //
        Task<YDDyeingBatchMaster> GetAllByIDAsyncYDDBM(int id);
    }
}
