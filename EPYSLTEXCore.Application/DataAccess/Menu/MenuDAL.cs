using EPYSLEMSCore.Application.DataAccess.Interfaces;
using EPYSLEMSCore.Application.Entities;
using EPYSLTEXCore.Application.DTO;
using EPYSLTEXCore.Infrastructure.Static;
using System.Configuration;
using System.Data.SqlClient;

namespace EPYSLTEXCore.Application.DataAccess
{
    public class MenuDAL : IMenuDAL
    {
        private readonly SqlConnection _connection = null;

        public MenuDAL()
        {
            _connection = new SqlConnection(ConfigurationManager.ConnectionStrings[AppConstants.GMT_CONNECTION].ConnectionString);
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

        public Task<List<MenuDTO>> GetMenusAsync(int userId, int applicationId, int companyId)
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
    }
}
