using Dapper;
using EPYSLTEXCore.Application.DataAccess.Interfaces;
using EPYSLTEXCore.Application.DTO;
using EPYSLTEXCore.Application.Entities;
using EPYSLTEXCore.Infrastructure.Static;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using System.Data.SqlClient;

namespace EPYSLTEXCore.Application.DataAccess
{
    public class MenuDAL : IMenuDAL
    {
        private readonly IConfiguration _configuration;
        private readonly SqlConnection _connection = null;

        public MenuDAL(IConfiguration configuration)
        {
            _configuration = configuration;
            _connection = new SqlConnection(_configuration.GetConnectionString(AppConstants.GMT_CONNECTION));
        }
        public Task<Menu> AddAsync(Menu item)
        {
            throw new NotImplementedException();
        }

        public Task<string> DeleteAsync(string Id)
        {
            throw new NotImplementedException();
        }

        public async Task<List<MenuDTO>> GetMenusAsync(int userId, int applicationId, int companyId)
        {
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync($@"[dbo].[spGetMenuListForApiModule] @UserCode, @ApplicationID, @CompanyID", new { UserCode = userId, ApplicationID = applicationId, CompanyID = companyId });

                var parentList = records.Read<MenuDTO>().OrderBy(x => x.SeqNo).ToList();
                var menuList = records.Read<MenuDTO>().OrderBy(x => x.SeqNo).ToList();
                PopulateMenus(ref parentList, menuList);
                return parentList;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                _connection.Close();
            }
        }

        public Task<Menu> GetByIdAsync(string Id)
        {
            throw new NotImplementedException();
        }

   

        public Task<bool> UpdateAsync(long Id, Menu item)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Menu>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        private void PopulateMenus(ref List<MenuDTO> parentList, List<MenuDTO> menuList)
        {
            foreach (var parentMenu in parentList)
            {
                parentMenu.Childs = menuList.FindAll(x => x.MenuId != parentMenu.ParentId && x.ParentId == parentMenu.MenuId).OrderBy(x => x.SeqNo).ToList();

                var subParents = parentMenu.Childs.FindAll(x => string.IsNullOrEmpty(x.PageName));
                foreach (var item in subParents) item.Childs = PopulateChildMenu(menuList, item.MenuId);
            }
        }

        private List<MenuDTO> PopulateChildMenu(List<MenuDTO> menuList, int parentId)
        {
            var childList = menuList.FindAll(x => x.ParentId == parentId).OrderBy(x => x.SeqNo).ToList();

            var subParents = childList.FindAll(x => string.IsNullOrEmpty(x.PageName));
            foreach (var childMenu in subParents) childMenu.Childs = PopulateChildMenu(menuList, childMenu.MenuId);

            return childList;
        }

    }
}
