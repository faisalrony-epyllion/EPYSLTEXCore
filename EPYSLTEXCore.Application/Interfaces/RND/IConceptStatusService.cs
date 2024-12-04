using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Gmt.Booking;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Booking;
using EPYSLTEXCore.Infrastructure.Entities.Tex.CountEntities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Fabric;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Statics;
using System.Data.Entity;
using System.Data.SqlClient;

namespace EPYSLTEXCore.Application.Interfaces.RND
{
    public interface IConceptStatusService
    {
        Task<List<ConceptStatus>> GetByConceptIds(string ConceptIDs);
        Task<List<ConceptStatus>> GetByCPSIDs(string CPSIDs, string ConceptIDs);
    }
}
