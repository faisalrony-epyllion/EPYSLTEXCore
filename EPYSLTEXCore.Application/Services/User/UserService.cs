using Dapper;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Static;
using Microsoft.Data.SqlClient;

namespace EPYSLTEX.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly IDapperCRUDService<LoginUser> _service;
        private SqlTransaction transaction;
        private readonly SqlConnection _connection;


        public UserService(IDapperCRUDService<LoginUser> service)
        {
            _service = service;
            service.Connection = _service.GetConnection(AppConstants.GMT_CONNECTION);
        }

        public async Task<LoginUser> FindUserForLoginAsync(string username)
        {
            var query = $@"
            Select LU.*, CE.CompanyName, E.EmployeeName, A.ApplicationName, E.DepertmentID, ED.DepertmentDescription, E.DesigID DesignationID, EDG.Designation
            From LoginUser LU
            INNER JOIN CompanyEntity CE On CE.CompanyID = LU.CompanyID
            INNER JOIN Employee E On E.EmployeeCode = LU.EmployeeCode
            INNER JOIN Application A On A.ApplicationID =  LU.DefaultApplicationID
            INNER JOIN EmployeeDepartment ED On ED.DepertmentID =E.DepertmentID
			LEFT JOIN EmployeeDesignation EDG On EDG.DesigID = E.DesigID
            Where LU.IsActive = 1 And LU.UserName = @Username";
            var segmentNames = new
            {
                Username = new string[]
                {
                    username
                }
            };
            return await _service.GetFirstOrDefaultAsync(query, segmentNames);
        }
        public async Task<LoginUser> FindAsync(string username)
        {
            var query = $@"
            Select LU.*, CE.CompanyName, E.EmployeeName, A.ApplicationName, E.DepertmentID, ED.DepertmentDescription, E.DesigID DesignationID, EDG.Designation
            From LoginUser LU
            INNER JOIN CompanyEntity CE On CE.CompanyID = LU.CompanyID
            INNER JOIN Employee E On E.EmployeeCode = LU.EmployeeCode
            INNER JOIN Application A On A.ApplicationID =  LU.DefaultApplicationID
            INNER JOIN EmployeeDepartment ED On ED.DepertmentID =E.DepertmentID
			LEFT JOIN EmployeeDesignation EDG On EDG.DesigID = E.DesigID
            Where LU.UserName = '{username}'";

            return await _service.GetFirstOrDefaultAsync(query);
        }

        public async Task<LoginUser> FindAsync(int userCode)
        {
            var query = $@"
            Select LU.*, CE.CompanyName, E.EmployeeName, A.ApplicationName, E.DepertmentID, ED.DepertmentDescription, E.DesigID DesignationID, EDG.Designation
            From LoginUser LU
            INNER JOIN CompanyEntity CE On CE.CompanyID = LU.CompanyID
            INNER JOIN Employee E On E.EmployeeCode = LU.EmployeeCode
            INNER JOIN Application A On A.ApplicationID =  LU.DefaultApplicationID
            INNER JOIN EmployeeDepartment ED On ED.DepertmentID =E.DepertmentID
			LEFT JOIN EmployeeDesignation EDG On EDG.DesigID = E.DesigID
            Where LU.UserCode = {userCode}";

            return await _service.GetFirstOrDefaultAsync(query);
        }

        public LoginUser Find(int userCode)
        {
            var query = $@"
            Select LU.*, CE.CompanyName, E.EmployeeName, A.ApplicationName, E.DepertmentID, ED.DepertmentDescription, LU.IsSuperUser, E.DesigID DesignationID, EDG.Designation
            From LoginUser LU
            INNER JOIN CompanyEntity CE On CE.CompanyID = LU.CompanyID
            INNER JOIN Employee E On E.EmployeeCode = LU.EmployeeCode
            INNER JOIN Application A On A.ApplicationID =  LU.DefaultApplicationID
            INNER JOIN EmployeeDepartment ED On ED.DepertmentID =E.DepertmentID
			LEFT JOIN EmployeeDesignation EDG On EDG.DesigID = E.DesigID
            Where LU.UserCode = {userCode}";

            return _service.Connection.QueryFirstOrDefault<LoginUser>(query);
        }

        public async Task<bool> IsValidLoginAsync(string username, string password)
        {
            var query = $@"
            Select LU.UserCode
            From LoginUser LU
            INNER JOIN CompanyEntity CE On CE.CompanyID = LU.CompanyID
            INNER JOIN Employee E On E.EmployeeCode = LU.EmployeeCode
            INNER JOIN Application A On A.ApplicationID =  LU.DefaultApplicationID
            Where LU.IsSuperUser = 1 OR (LU.UserName = '{username}' And LU.Password = '{password}' And LU.IsActive = 1 And E.IsSupplier = 1)";

            var userCode = await _service.GetSingleIntFieldAsync(query);
            return userCode > 0;
        }

        public async Task SaveAsync(LoginUser user)
        {
            try
            {
                await _service.Connection.OpenAsync();
                transaction = _service.Connection.BeginTransaction();
                await _service.SaveSingleAsync(user, transaction);
                transaction.Commit();
            }
            catch (System.Exception)
            {
                transaction.Rollback();
                throw;
            }
            finally
            {
                _service.Connection.Close();
            }
        }

        public async Task<int> UpdateUserPasswordAsync(int userCode, string password)
        {
            var query = $@"
            Update LoginUser Set Password = '{password}' Where UserCode = @userCode";

            return await _service.ExecuteAsync(query, new { userCode});
        }

        public async Task<int> UpdateEmailPasswordAsync(int userCode, string password)
        {
            var query = $@"
            Update LoginUser Set EmailPassword = '{password}' Where UserCode = @userCode";

            return await _service.ExecuteAsync(query, new { userCode });
        }
    }
}
