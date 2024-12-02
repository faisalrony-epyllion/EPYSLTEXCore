using EPYSLTEXCore.Application.Entities;
using EPYSLTEXCore.Application.Entities.PaginationInfo;
using EPYSLTEXCore.Infrastructure.Entities;

namespace EPYSLTEXCore.Application.Interfaces.YarnProductSetup
{
    public interface IYarnProductSetupService
    {

        

        Task<List<YarnProductSetupFinder>> GetAllFiberType(PaginationInfo paginationInfo);
        Task<List<YarnProductSetupChild>> GetAlYarnProductSetupChildBySetupMasterID(int setupMasterID);  
    }
}
