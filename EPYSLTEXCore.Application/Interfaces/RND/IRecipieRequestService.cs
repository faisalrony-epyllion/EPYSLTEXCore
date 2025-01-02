using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Statics;


namespace EPYSLTEXCore.Application.Interfaces.RND
{
    public interface IRecipieRequestService
    {
        Task<List<RecipeRequestMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo);

        Task<RecipeRequestMaster> GetNewAsync(int ccColorId, string grpConceptNo, int isBDS, int DBatchID = 0);

        Task<RecipeRequestMaster> GetAsync(int Id, string groupConceptNo);

        Task<List<RecipeRequestChild>> GetItems(string conceptNo, int colorID, int isBDS);
        Task<RecipeRequestMaster> GetAllByIDAsync(int id);

        Task SaveAsync(RecipeRequestMaster entity);
        Task RevisionAsync(RecipeRequestMaster entity);

        Task UpdateEntityAsync(RecipeRequestMaster entity);
    }
}