
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



        public Task<bool> UpdateAsync(long Id, Menu item)
        {
            return _IMenuDAL.UpdateAsync(Id,item);
        }        
               
        public Task<IEnumerable<Menu>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<List<MenuDTO>> GetMenusAsync(int userId, int applicationId, int companyId)
        {
            return _IMenuDAL.GetMenusAsync(userId, applicationId, companyId);
        }
    }
}
