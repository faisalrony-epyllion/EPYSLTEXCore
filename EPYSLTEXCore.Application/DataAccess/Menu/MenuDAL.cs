using EPYSLTEXCore.Application.DataAccess.Interfaces;
using EPYSLTEXCore.Application.Entities;
using EPYSLTEXCore.Application.Interfaces;
using EPYSLTEXCore.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Application.DataAccess
{
    public class MenuDAL : IMenuDAL
    {
        public Task<Menu> AddAsync(Menu item)
        {
            throw new NotImplementedException();
        }

        public Task<string> DeleteAsync(string Id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Menu>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Menu> GetByIdAsync(string Id)
        {
            throw new NotImplementedException();
        }

        public Task<List<MenuDTO>> GetMenusAsync(int userId, int applicationId, int companyId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateAsync(long Id, Menu item)
        {
            throw new NotImplementedException();
        }

       
    }
}
