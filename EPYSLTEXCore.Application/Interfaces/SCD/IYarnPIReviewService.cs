using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Statics;

namespace EPYSLTEXCore.Interfaces.Services
{
    public interface IYarnPIReviewService
    {
        Task<List<YarnPIReceiveMaster>> GetPagedAsync(Status status, bool isCDAPage, PaginationInfo paginationInfo);
        Task<YarnPIReceiveMaster> GetAsync(int id, int supplierId, int companyId, bool isCDAPage);
        Task<YarnPIReceiveMaster> GetDetailsAsync(int id);
        Task SaveAsync(YarnPIReceiveMaster entity);
    }
}