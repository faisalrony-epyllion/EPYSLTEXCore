using EPYSLTEXCore.Application.Interfaces;
using EPYSLTEXCore.Application.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPYSLTEXCore.Application.Entities.ReportAPI;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Application.DataAccess.Interfaces;
using EPYSLTEXCore.Application.DTO;

namespace EPYSLTEXCore.Application.Services
{
    public class MenuService : IMenuService
    {

        private readonly IMenuDAL _IMenuDAL;
        public MenuService(IMenuDAL IMenuDAL) 
        {
            _IMenuDAL = IMenuDAL;
        }

        public Task<Menu> AddAsync(Menu item)
        {
           return _IMenuDAL.AddAsync(item);
        }

        public Task<string> DeleteAsync(string Id)
        {
            return _IMenuDAL.DeleteAsync(Id);
        }

        public Task<IEnumerable<Menu>> GetAllAsync()
        {
            return _IMenuDAL.GetAllAsync();
        }

        public Task<Menu> GetByIdAsync(string Id)
        {
            return _IMenuDAL.GetByIdAsync(Id);
        }

        public Task<List<MenuDTO>> GetMenusAsync(int userId, int applicationId, int companyId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateAsync(long Id, Menu item)
        {
            return _IMenuDAL.UpdateAsync(Id,item);
        }

        Task<List<MenuDTO>> IMenuService.GetMenusAsync(int userId, int applicationId, int companyId)
        {
            throw new NotImplementedException();
        }
    }
}
