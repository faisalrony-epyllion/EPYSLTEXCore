using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Statics;

namespace EPYSLTEXCore.Application.Interfaces.Inventory.Yarn
{
    public interface IProjectionYarnBookingService
    {
        Task<List<ProjectionYarnBookingMaster>> GetPagedAsync(int departmentId, Status status, PaginationInfo paginationInfo, LoginUser AppUser);
        Task<List<ProjectionYarnBookingMaster>> GetPagedAsynci(int departmentId, string departmentName, int employeeCode, Status status, PaginationInfo paginationInfo, LoginUser AppUser);
        Task<ProjectionYarnBookingMaster> GetNewAsync(int employeeCode);
        Task<ProjectionYarnBookingMaster> GetAsync(int PYBookingID, int employeeCode);
        Task<ProjectionYarnBookingMaster> GetBuyerTeamAsync(string buyerId, int employeeCode);
        Task<ProjectionYarnBookingMaster> GetNewPYBookingID(string ids, int employeeCode);
        Task<ProjectionYarnBookingMaster> GetAllAsync(int id);
        Task<ProjectionYarnBookingMaster> GetPYBWithPRAsync(int pyBookingID, string pyBookingNo);
        Task SaveAsync(ProjectionYarnBookingMaster model, int userId);
        Task UpdateEntityAsync(ProjectionYarnBookingMaster entity);
        Task AcknowledgeEntityAsync(ProjectionYarnBookingMaster entity, YarnPRMaster yarnPRMaster);
    }
}
