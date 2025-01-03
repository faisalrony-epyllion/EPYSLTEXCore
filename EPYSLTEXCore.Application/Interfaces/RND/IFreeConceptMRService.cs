using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Gmt.General.Item;
using EPYSLTEXCore.Infrastructure.Entities.Tex;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Statics;
using System.Data.Entity;

namespace EPYSLTEXCore.Application.Interfaces
{
    public interface IFreeConceptMRService
    {
        Task<List<FreeConceptMRMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo);

        Task<FreeConceptMRMaster> GetNewAsync(int conceptId);

        Task<FreeConceptMRMaster> GetAsync(int Id);

        Task<FreeConceptMRMaster> GetByGroupConceptAsync(string grpConceptNo, int conceptTypeID);

        Task<FreeConceptMRMaster> GetMultipleAsync(string grpConceptNo, int conceptTypeID);

        Task<FreeConceptMRMaster> GetMultipleAsyncRevision(string grpConceptNo, int conceptTypeID);
        Task<List<FreeConceptMRMaster>> GetByGroupConceptNo(string groupConceptNo);
        Task<FreeConceptMRMaster> GetDetailsAsync(int id);
        Task<FreeConceptMRMaster> GetMRByConceptNo(string conceptNo);
        Task<List<FreeConceptMRChild>> GetMRChildByBookingNo(string bookingNo);
        Task<List<FreeConceptMRChild>> GetMRChildByBookingNoWithRevision(string bookingNo);
        Task<bool> ExistsAsync(int conceptID, int trialNo);

        Task SaveAsync(FreeConceptMRMaster entity, int userId);

        Task<List<FreeConceptMRMaster>> GetMultiDetailsAsync(string grpConceptNo);

        Task SaveMultipleAsync(List<FreeConceptMRMaster> entities, EntityState entityState);

        Task ReviseAsync(List<FreeConceptMRMaster> entities, string grpConceptNo, int userId, string fcmrChildIds);
        Task SaveBlendTypeName(CompositionBlendType entity);
        Task<List<CT>> GetDataDapperAsync<CT>(string query) where CT : class;
        Task<bool> ExistsAsync(int SegmentNameID, string SegmentValue);
        Task<ItemSegmentName> FindAsync(string SegmentName);
        Task AddAsync(ItemSegmentValue entity, string iTEM_SEGMENT_VALUE);
        //Task<ItemSegmentName> FindAsync(Func<object, bool> value);


        #region Composition Part
        Task<List<Select2OptionModel>> GetYarnTypes();
        Task<List<Select2OptionModel>> GetYarnSubProgramNews(string yarnTypeId);
        Task<List<Select2OptionModel>> GetCertifications(string yarnTypeId, string yarnSubProgranNewId);
        #endregion
    }
}