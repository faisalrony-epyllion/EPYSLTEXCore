using Dapper;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Application.Interfaces;
using EPYSLTEXCore.Application.Interfaces.Admin;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Gmt.General.Item;
using EPYSLTEXCore.Infrastructure.Entities.Tex;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Admin;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Transactions;

namespace EPYSLTEXCore.Application.Services.Admin
{
    public class BankLimitService : IBankLimitService
    {
        private readonly IDapperCRUDService<BankLimitMaster> _service;

        private readonly SqlConnection _connection;
        private readonly SqlConnection _connectionGmt;

        public BankLimitService(IDapperCRUDService<BankLimitMaster> service)
        {
            _service = service;

            _service.Connection = service.GetConnection(AppConstants.GMT_CONNECTION);
            _connectionGmt = service.Connection;

            _service.Connection = service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _connection = service.Connection;
        }

        public async Task<List<BankLimitMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? " ORDER BY CompanyName, BankLimitMasterID DESC " : paginationInfo.OrderBy;
            string sql = "";

            if (status == Status.All)
            {
                sql = $@"WITH FinalList AS
                (
	                SELECT BEM.BankLimitMasterID
	                ,C.CompanyID
	                ,C.CompanyName
	                ,BondLicenceNo = ISNULL(BEM.BondLicenceNo,'')
	                ,EBINNo = ISNULL(BEM.EBINNo,'')
	                ,BEM.FromDate
	                ,BEM.ToDate
	                ,CurrencyID = ISNULL(BEM.CurrencyID,0)
	                ,CurrencyName = ISNULL(CU.CurrencyCode,'')
	                FROM BankLimitMaster BEM
	                INNER JOIN EPYSL..CompanyEntity C ON C.CompanyID = BEM.CompanyID 
	                LEFT JOIN EPYSL..Currency CU ON CU.CurrencyID = BEM.CurrencyID
	                WHERE C.CompanyID IN(8,6)
                )
                SELECT *, Count(*) Over() TotalRows FROM FinalList";
            }



            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<BankLimitMaster>(sql);
        }

        public async Task<BankLimitMaster> GetNewAsync()
        {
            string query =
                $@"
                {CommonQueries.GetCompany()};

                SELECT id = ISNULL(ISN.SegmentNameID,0)
                ,text = ISN.SegmentName 
                FROM {DbNames.EPYSL}..ItemSegmentName ISN
                WHERE ISN.SegmentNameID IN (270,273);

                SELECT id = ISV.SegmentValueID
                ,text = ISV.SegmentValue 
                ,additionalValue = ISNULL(ISN.SegmentNameID,0)
                FROM {DbNames.EPYSL}..ItemSegmentValue ISV
                INNER JOIN {DbNames.EPYSL}..ItemSegmentName ISN ON ISN.SegmentNameID = ISV.SegmentNameID
                WHERE ISN.SegmentNameID IN (270,273);

                --Currency
                SELECT id = CurrencyID, text = CurrencyCode FROM {DbNames.EPYSL}..Currency;
                
                --Unit
                SELECT id = UnitID, text = DisplayUnitDesc FROM {DbNames.EPYSL}..Unit;

              ";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);

                BankLimitMaster data = new BankLimitMaster();
                data.CompanyList = await records.ReadAsync<Select2OptionModel>();
                var itemSegmentNames = await records.ReadAsync<Select2OptionModel>();
                var itemSegmentValues = await records.ReadAsync<Select2OptionModel>();
                data.CurrencyList = await records.ReadAsync<Select2OptionModel>();
                data.UnitList = await records.ReadAsync<Select2OptionModel>();

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
        public async Task<BankLimitMaster> GetDetails(int id)
        {
            string sql = $@"
            Select M.* 
            From BankLimitMaster M
            Where M.BankLimitMasterID = {id};

            Select C.* 
            From BankLimitMaster M
            INNER JOIN BankLimitChild C ON C.BankLimitMasterID = M.BankLimitMasterID
            Where M.BankLimitMasterID = {id};

            Select CI.* 
            From BankLimitMaster M
            INNER JOIN BankLimitChild C ON C.BankLimitMasterID = M.BankLimitMasterID
            INNER JOIN BankLimitChildItem CI ON CI.BankLimitChildID = C.BankLimitChildID
            Where M.BankLimitMasterID = {id};

            {CommonQueries.GetCompany()};

            SELECT id = ISNULL(ISN.SegmentNameID,0)
            ,text = ISN.SegmentName 
            FROM {DbNames.EPYSL}..ItemSegmentName ISN
            WHERE ISN.SegmentNameID IN (270,273);

            SELECT id = ISV.SegmentValueID
            ,text = ISV.SegmentValue 
            ,additionalValue = ISNULL(ISN.SegmentNameID,0)
            FROM {DbNames.EPYSL}..ItemSegmentValue ISV
            INNER JOIN {DbNames.EPYSL}..ItemSegmentName ISN ON ISN.SegmentNameID = ISV.SegmentNameID
            WHERE ISN.SegmentNameID IN (270,273);

            --Currency
            SELECT id = CurrencyID, text = CurrencyCode FROM {DbNames.EPYSL}..Currency;
                
            --Unit
            SELECT id = UnitID, text = DisplayUnitDesc FROM {DbNames.EPYSL}..Unit;

            ";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                BankLimitMaster data = records.Read<BankLimitMaster>().FirstOrDefault();
                var childs = records.Read<BankLimitChild>().ToList();

                data.CompanyList = await records.ReadAsync<Select2OptionModel>();
                data.CurrencyList = await records.ReadAsync<Select2OptionModel>();
                data.UnitList = await records.ReadAsync<Select2OptionModel>();

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
        public async Task<BankLimitMaster> GetById(int id)
        {
            string sql = $@"
            Select M.* 
            From BankLimitMaster M
            Where M.BankLimitMasterID = {id};

            Select C.* 
            From BankLimitMaster M
            INNER JOIN BankLimitChild C ON C.BankLimitMasterID = M.BankLimitMasterID
            Where M.BankLimitMasterID = {id};

            ";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                BankLimitMaster data = records.Read<BankLimitMaster>().FirstOrDefault();
                data.Childs = records.Read<BankLimitChild>().ToList();
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
        public async Task SaveAsync(BankLimitMaster entity)
        {
            SqlTransaction transaction = null;
            SqlTransaction transactionGmt = null;

            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                await _connectionGmt.OpenAsync();
                transactionGmt = _connectionGmt.BeginTransaction();

                int maxChildId = 0;
                int maxChildItemId = 0;

                switch (entity.EntityState)
                {
                    case EntityState.Added:

                        entity.BankLimitMasterID = await _service.GetMaxIdAsync(TableNames.BankLimitMaster, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        maxChildId = await _service.GetMaxIdAsync(TableNames.BankLimitChild, entity.Childs.Count(x => x.EntityState == EntityState.Added), RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

                        foreach (var item in entity.Childs)
                        {
                            item.BankLimitChildID = maxChildId++;
                            item.BankLimitMasterID = entity.BankLimitMasterID;
                            item.EntityState = EntityState.Added;
                        }

                        break;

                    case EntityState.Modified:

                        maxChildId = await _service.GetMaxIdAsync(TableNames.BankLimitChild, entity.Childs.Count(x => x.EntityState == EntityState.Added), RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

                        foreach (var item in entity.Childs)
                        {
                            if (item.EntityState == EntityState.Added)
                            {
                                item.BankLimitChildID = maxChildId++;
                                item.BankLimitMasterID = entity.BankLimitMasterID;
                                item.EntityState = EntityState.Added;
                            }
                        }


                        break;

                    case EntityState.Unchanged:
                    case EntityState.Deleted:
                        entity.EntityState = EntityState.Deleted;
                        entity.Childs.SetDeleted();
                        break;

                    default:
                        break;
                }

                await _service.SaveSingleAsync(entity, transaction);
                await _service.SaveAsync(entity.Childs, transaction);

                transaction.Commit();
                transactionGmt.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                transactionGmt.Rollback();
                throw ex;
            }
            finally
            {
                _connection.Close();
                _connectionGmt.Close();
            }
        }
    }
}
