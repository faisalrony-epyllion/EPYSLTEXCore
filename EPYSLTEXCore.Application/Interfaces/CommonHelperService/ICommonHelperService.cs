using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.General;

namespace EPYSLTEX.Core.Interfaces
{
    public interface ICommonHelperService
    {
        #region Common Interface

        #endregion

        #region MachineGaugeSetup
        Task<List<MachineGaugeSetup>> GetMachineGaugeAsync(PaginationInfo paginationInfo);
        Task<MachineGaugeSetup> GetMachineGaugeSetupDetailsAsync(int id);
        
        #endregion MachineGaugeSetup

        #region FabricTechnicalName
        Task<List<FabricTechnicalName>> GetFabricTechnicalNameAsync(PaginationInfo paginationInfo);
        Task<FabricTechnicalName> GetAllAsyncFabricTechnicalName(int id);
        
        #endregion FabricTechnicalName

        #region FabricColorShade
        Task<List<FabricColorShade>> GetFabricColorShadeAsync(PaginationInfo paginationInfo);
        Task<FabricColorShade> GetAllAsyncFabricColorShade(int id);
        
        #endregion FabricColorShade

        #region TextileProcessMaster
        Task<List<TextileProcessMaster>> GetTextileProcessAsync(PaginationInfo paginationInfo);
        Task<List<TextileProcessMaster>> GetTextileProcessListAsync(PaginationInfo paginationInfo);
        Task<TextileProcessMaster> GetAllAsyncTextileProcessMaster(int id);
        
        #endregion TextileProcessMaster

        Task<List<TextileProcessUserDTO>> GetTextileProcessUser(PaginationInfo paginationInfo);
        #region FabricTechnicalNameOther
        Task<List<FabricTechnicalNameOther>> GetFabricTechnicalNameOthers(PaginationInfo paginationInfo);
        Task<FabricTechnicalNameOther> GetFabricTechnicalNameOtherAsync(int id);
        
        #endregion FabricTechnicalNameOther
        Task<List<FabricWastageGrid>> GetFabricWastageGridAsync(string wastageFor);
    }
}