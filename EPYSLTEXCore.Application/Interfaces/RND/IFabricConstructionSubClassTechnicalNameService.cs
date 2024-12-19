using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;

namespace EPYSLTEXCore.Application.Interfaces.RND
{
    public interface IFabricConstructionSubClassTechnicalNameService
    {
        Task<List<FabricConstructionSubClassTechnicalName>> GetFabricConstructionSubClassTechnicalNames();
    }
}
