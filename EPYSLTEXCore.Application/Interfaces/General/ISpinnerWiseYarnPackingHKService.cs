using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.DTOs;
using EPYSLTEXCore.Infrastructure.Entities.Gmt.General;
using EPYSLTEXCore.Infrastructure.Entities.Tex.General;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Statics;

namespace EPYSLTEXCore.Application.Interfaces
{
    public interface ISpinnerWiseYarnPackingHKService
    {
        Task<List<SpinnerWiseYarnPackingHK>> GetPagedAsync(PaginationInfo paginationInfo);
        Task<List<SpinnerWiseYarnPackingHK>> GetAsync(SpinnerWiseYarnPackingHK entitie);
        Task<SpinnerWiseYarnPackingHK> GetAsync(int setupID);
        //Task<YarnProductSetupSupplierDTO> GetDataAsync(int fiberTypeID);
        //Task<YarnProductSetupMasterDTO> GetProductSetup(int fiberTypeId);
        Task SaveAsync(SpinnerWiseYarnPackingHK entitie);
        Task<SpinnerWiseYarnPackingHK> GetMaster();
    }
}