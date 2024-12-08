using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.DTOs;
using EPYSLTEXCore.Infrastructure.Entities.Gmt.General;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Statics;

namespace EPYSLTEXCore.Application.Interfaces
{
    public interface IFabricColorBookSetupService
    {
        Task<List<FabricColorBookSetupDTO>> GetPagedAsync(PaginationInfo paginationInfo);

        Task<List<FabricColorBookSetupDTO>> GetAllListAsync();

        Task<FabricColorBookSetupDTO> GetNewAsync();

        Task<FabricColorBookSetup> GetAsync(int id);

        Task<List<FabricColorBookSetupDTO>> GetAllColorAsync(PaginationInfo paginationInfo);

        Task SaveAsync(FabricColorBookSetup entity);
    }
}