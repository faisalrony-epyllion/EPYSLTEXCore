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
    public interface IFreeConceptService
    {
        Task<List<FreeConceptMaster>> GetPagedAsync(FreeConceptStatus status, PaginationInfo paginationInfo);

        Task<FreeConceptMaster> GetTechnicalNameList(int subClassId);

        Task<FreeConceptMaster> GetNewAsync();

        Task<FreeConceptMaster> GetRevisionListAsync(int Id, int subClassId);

        Task<FreeConceptMaster> GetAsync(int Id, int subClassId);

        Task<FreeConceptMaster> GetByGroupConceptAsync(string grpConceptNo, int conceptTypeID);
        Task<List<Select2OptionModel>> GetTechnicalNameByMC(int subclassId);
        Task<List<FreeConceptMaster>> GetByBookingIds(string bookingIds);

        Task<FreeConceptMaster> GetDetailsAsync(int id);

        Task<List<FreeConceptMaster>> GetDatasAsync(string grpConceptNo);

        Task<List<ConceptPendingStatus_HK>> GetPendingStatus();

        Task SaveAsync(FreeConceptMaster entity);

        Task SaveRevisionAsync(FreeConceptMaster revisionEntity, FreeConceptMaster newEntity, int userId);

        Task SaveManyAsync(List<FreeConceptMaster> entities, EntityState entityState);
        Task ReviseManyAsync(List<FreeConceptMaster> entities, string grpConceptNo, EntityState entityState);

        Task<List<FreeConceptChildColor>> GetChildColorDatasAsync(int conceptID);

        Task SaveAsyncChildColor(List<FreeConceptChildColor> entities);
    }
}
