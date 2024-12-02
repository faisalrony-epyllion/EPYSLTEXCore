using EPYSLTEXCore.Application.DTO;
using EPYSLTEXCore.Application.Entities;

namespace EPYSLTEXCore.Application.Interfaces.YarnProductSetup
{
    public interface IYarnProductSetupService
    {

        

        Task<List<YarnProductSetupFinder>> GetAllFiberType(PaginationInfo paginationInfo);
        Task<List<YarnProductSetupChild>> GetAlYarnProductSetupChildBySetupMasterID(int setupMasterID);  
    }
}
