using EPYSLTEX.Core.Entities.Tex;
using EPYSLTEXCore.Infrastructure.Statics;

namespace EPYSLTEXCore.Application.Interfaces.Admin
{
    public interface IFinishingMachineSetupService
    {
        Task<List<FinishingMachineConfigurationMaster>> GetPagedAsync(Status status, int offset = 0, int limit = 10, string filterBy = null, string orderBy = null);

        Task<FinishingMachineConfigurationMaster> GetNewAsync(int newId);

        Task<FinishingMachineConfigurationMaster> GetAsync(int id);

        Task<List<FinishingMachineSetup>> GetAsyncFinishingMachineConfiguration(int processId, int processTypeId);

        Task SaveAsync(FinishingMachineConfigurationMaster entity);
        Task<FinishingMachineConfigurationMaster> GetAllAsync(int id);
    }
}