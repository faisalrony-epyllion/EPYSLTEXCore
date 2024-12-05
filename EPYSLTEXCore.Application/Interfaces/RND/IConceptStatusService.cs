using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;

namespace EPYSLTEXCore.Application.Interfaces.RND
{
    public interface IConceptStatusService
    {
        Task<List<ConceptStatus>> GetByConceptIds(string ConceptIDs);
        Task<List<ConceptStatus>> GetByCPSIDs(string CPSIDs, string ConceptIDs);
    }
}
