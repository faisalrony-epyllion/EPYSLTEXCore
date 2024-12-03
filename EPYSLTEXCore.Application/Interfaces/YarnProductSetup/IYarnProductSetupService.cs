using EPYSLTEXCore.Infrastructure.Entities;

namespace EPYSLTEXCore.Application.Interfaces
{
    public interface IYarnProductSetupService
    {

        

        Task<List<YarnProductSetup>> GetAllFiberType(PaginationInfo paginationInfo);
        Task<List<YarnProductSetupChild>> GetAlYarnProductSetupChildBySetupMasterID(int setupMasterID);  
    }
}
