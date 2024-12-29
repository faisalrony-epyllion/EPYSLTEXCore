using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Statics;

namespace EPYSLTEXCore.Application.Interfaces.RND
{
    public interface ILabTestResultService
    {
        Task<List<LabTestRequisitionMaster>> GetPagedAsync(Status status, int offset = 0, int limit = 10, string filterBy = null, string orderBy = null);

        Task<LabTestRequisitionMaster> GetAsync(int id);

        Task<List<LabTestRequisitionBuyerParameter>> GetBuyerParameterByBuyerId(int id);

        Task<LabTestRequisitionMaster> GetAllByIDAsync(int id);

        Task UpdateBDSTNA_TestReportPlanAsync(int LTReqMasterID);

        Task SaveAsync(LabTestRequisitionMaster entity);
    }
}