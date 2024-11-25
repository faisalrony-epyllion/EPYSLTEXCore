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

        public async Task<List<MenuDTO>> GetAllMenuReport(int userId, int applicationId, int companyId)
        {
            var menuList = new List<MenuDTO>();
            var parentList = new List<MenuDTO>();
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync($@"[dbo].[spGetReportMenuListForApi] @UserCode, @ApplicationID, @CompanyID", new { UserCode = userId, ApplicationID = applicationId, CompanyID = companyId });

                parentList = records.Read<MenuDTO>().ToList();

                menuList = parentList.FindAll(x => x.ReportId != x.Parent_Key);
                parentList=parentList.FindAll(x => x.ReportId == x.Parent_Key);

                PopulateReportMenus(ref parentList, menuList);
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

        private void PopulateReportMenus(ref List<MenuDTO> parentList, List<MenuDTO> menuList)
        {
            foreach (var parentMenu in parentList)
            {
                parentMenu.Childs = menuList.FindAll(x => x.ReportId != parentMenu.Parent_Key && x.Parent_Key == parentMenu.ReportId).OrderBy(x => x.SeqNo).ToList();

                var subParents = parentMenu.Childs.FindAll(x => string.IsNullOrEmpty(x.Report_Name));
                foreach (var item in subParents)
                    item.Childs = PopulateChildReportMenu(menuList, item.ReportId);
            }
        }

        private List<MenuDTO> PopulateChildReportMenu(List<MenuDTO> menuList, int parentId)
        {
            var childList = menuList.FindAll(x => x.Parent_Key == parentId).OrderBy(x => x.SeqNo).ToList();

            var subParents = childList.FindAll(x => string.IsNullOrEmpty(x.Report_Name));
            foreach (var childMenu in subParents)
                childMenu.Childs = PopulateChildMenu(menuList, childMenu.ReportId);

            return childList;
        }
    }
}
