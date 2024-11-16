
using EPYSLTEXCore.Application.DataAccess.Interfaces;
using EPYSLTEXCore.Application.DTO;
using EPYSLTEXCore.Application.Entities;
using EPYSLTEXCore.Application.Interfaces;

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

   

        public Task<Menu> GetByIdAsync(string Id)
        {
            return _IMenuDAL.GetByIdAsync(Id);
        }

        public Task<List<MenuDTO>> GetAllAsync(int userId, int applicationId, int companyId)
        {
            return _IMenuDAL.GetAllAsync();
        }

        public Task<bool> UpdateAsync(long Id, Menu item)
        {
            return _IMenuDAL.UpdateAsync(Id,item);
        }

   

        Task<IEnumerable<Menu>> ICommonService<Menu>.GetAllAsync()
        {
            throw new NotImplementedException();
        }

        Task<Menu> ICommonService<Menu>.GetByIdAsync(string Id)
        {
            throw new NotImplementedException();
        }

        Task<List<MenuDTO>> IMenuService.GetMenusAsync(int userId, int applicationId, int companyId)
        {
            throw new NotImplementedException();
        }
    }
}
