using Dapper;
using EPYSLTEX.Core.Entities;
using EPYSLTEX.Core.Interfaces.Repositories;
using EPYSLTEX.Core.Services;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using System.Data.Entity;
using System.Data.SqlClient;

namespace EPYSLTEX.Infrastructure.Services
{
    public class LoginHistoryService : ILoginHistoryService
    {
        private readonly IDapperCRUDService<LoginHistory> _service;
        private readonly ISignatureRepository _signatureRepository;
        private readonly SqlConnection _connection;

        public LoginHistoryService(IDapperCRUDService<LoginHistory> service
           , ISignatureRepository signatureRepository)
        {
            _service = service;
            _signatureRepository = signatureRepository;
            _connection = service.Connection;
        }

        public async Task SaveAsync(LoginHistory entity)
        {
            SqlTransaction transaction = null;
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();
                if (entity.EntityState == EntityState.Added)
                {
                    entity.LoginHistoryID = await _signatureRepository.GetMaxIdAsync(TableNames.LOGIN_HISTORY);
                }
                await _service.SaveSingleAsync(entity, transaction);
                transaction.Commit();
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                throw ex;
            }
            finally
            {
                _connection.Close();
            }
        }
        public async Task<LoginHistory> GetAsync(LoginHistory loginHistory)
        {
            var sql = $@"
            SELECT TOP(1)* FROM LoginHistory WHERE
            IPAddress = '{loginHistory.IPAddress}' AND
            MachineName = '{loginHistory.MachineName}' AND
            MachineUserName = '{loginHistory.MachineUserName}' AND
            OpenPortNo = {loginHistory.OpenPortNo}
            ORDER BY LoginHistoryID DESC";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                LoginHistory data = await records.ReadFirstOrDefaultAsync<LoginHistory>();
               // Guard.Against.NullObject(data);
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open) _connection.Close();
            }
        }
    }
}
