using EPYSLTEXCore.Application.DataAccess;
using EPYSLTEXCore.Application.Entities;
using EPYSLTEXCore.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Application.Interfaces
{
    public interface IMenuService:ICommonService<Menu>
    {
        Task<List<MenuDTO>> GetMenusAsync(int userId, int applicationId, int companyId);
    }
}
