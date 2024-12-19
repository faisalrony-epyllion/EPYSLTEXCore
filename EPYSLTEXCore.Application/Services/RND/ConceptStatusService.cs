using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Application.Interfaces.RND;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Static;
using Microsoft.Data.SqlClient;

namespace EPYSLTEXCore.Application.Services.RND
{
    public class ConceptStatusService : IConceptStatusService
    {
        private readonly IDapperCRUDService<ConceptStatus> _service;
        private readonly SqlConnection _connection;
        private SqlTransaction transaction;

        public ConceptStatusService(IDapperCRUDService<ConceptStatus> service)
        {
            _service = service;
            _service.Connection = service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _connection = service.Connection;
        }
        public async Task<List<ConceptStatus>> GetByConceptIds(string ConceptIDs)
        {
            if (string.IsNullOrEmpty(ConceptIDs)) return new List<ConceptStatus>();
            var sql = $@"SELECT * FROM {TableNames.RND_FREE_CONCEPT_STATUS} WHERE ConceptID IN ({ConceptIDs}) ";
            return await _service.GetDataAsync<ConceptStatus>(sql, _connection);
        }
        public async Task<List<ConceptStatus>> GetByCPSIDs(string CPSIDs, string ConceptIDs)
        {
            if (string.IsNullOrEmpty(CPSIDs) || string.IsNullOrEmpty(ConceptIDs)) return new List<ConceptStatus>();
            var sql = $@"SELECT * FROM {TableNames.RND_FREE_CONCEPT_STATUS} WHERE CPSID IN ({CPSIDs}) AND ConceptID IN ({ConceptIDs}) ";
            return await _service.GetDataAsync<ConceptStatus>(sql, _connection);
        }
    }
}
