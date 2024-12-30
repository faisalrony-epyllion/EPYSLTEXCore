using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.DTOs;
using EPYSLTEXCore.Infrastructure.Entities.Gmt.General;
using EPYSLTEXCore.Infrastructure.Entities.Tex.General;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Statics;

namespace EPYSLTEXCore.Application.Interfaces
{
    public interface IDyeingMachineService
    {
        Task<List<DyeingMachine>> GetPagedAsync(PaginationInfo paginationInfo);

        Task<List<DyeingMachine>> GetNozzleInfoAsync(PaginationInfo paginationInfo);

        Task<DyeingMachine> GetNewAsync();

        Task<DyeingMachine> GetAsync(int id);

        Task<List<DyeingMachine>> GetDyeingMachineByNozzleListAsync(int nozzle);
        Task<DyeingMachine> GetAllAsync(int id);
        Task SaveAsync(DyeingMachine entity);
    }
}