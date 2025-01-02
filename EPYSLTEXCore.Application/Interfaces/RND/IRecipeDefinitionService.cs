using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Statics;


namespace EPYSLTEXCore.Application.Interfaces.RND
{
    public interface IRecipeDefinitionService
    {
        Task<List<RecipeDefinitionMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo); //RecipeReqMasterID

        Task<RecipeDefinitionMaster> GetNewAsync(int Id);

        Task<RecipeDefinitionMaster> GetAsync(int Id);

        Task SaveAsync(RecipeDefinitionMaster entity);

        Task<RecipeDefinitionMaster> GetAllByIDAsync(int id);
        Task<List<RecipeDefinitionMaster>> GetByRecipeReqNo(string recipeReqNo);

        Task UpdateEntityAsync(RecipeDefinitionMaster entity);
        Task UpdateRecipeId(RecipeDefinitionMaster entity, int DBatchID);
        Task UpdateRecipeWithBatchAsync(RecipeDefinitionMaster entity, DyeingBatchMaster dyeingBatchEntity, List<BatchMaster> batchEntities);

        Task<List<BatchMaster>> GetBatchDetails(string batchIds);

        Task<List<RecipeDefinitionMaster>> GetAllApproveListForCopy(string dpID, string buyer, string fabricComposition, string color, PaginationInfo paginationInfo);

        Task<List<RecipeDefinitionMaster>> GetConceptWiseRecipeForCopy(string fabricComposition, string color, string GroupConceptNo, PaginationInfo paginationInfo);
        Task<RecipeDefinitionMaster> GetRecipeDyeingInfo(int Id);

    }
}
