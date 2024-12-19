using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Application.Interfaces.RND;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Static;

namespace EPYSLTEXCore.Application.Services.RND
{
    public class FabricConstructionSubClassTechnicalNameService : IFabricConstructionSubClassTechnicalNameService
    {
        private readonly IDapperCRUDService<FabricConstructionSubClassTechnicalName> _service;
        public FabricConstructionSubClassTechnicalNameService(IDapperCRUDService<FabricConstructionSubClassTechnicalName> service)
        {
            _service = service;
            _service.Connection = service.GetConnection(AppConstants.TEXTILE_CONNECTION);
        }

        public async Task<List<FabricConstructionSubClassTechnicalName>> GetFabricConstructionSubClassTechnicalNames()
        {
            var query = $@"SELECT * FROM {TableNames.FabricConstructionSubClassTechnicalName}";
            return await _service.GetDataAsync<FabricConstructionSubClassTechnicalName>(query);
        }
    }
}
