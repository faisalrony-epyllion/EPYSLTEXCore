using Dapper;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Application.Interfaces.Admin;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Admin;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using System.Data.Entity;
using Microsoft.Data.SqlClient;

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
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? " ORDER BY BankLimitMasterID DESC, BankLimitChildID  " : paginationInfo.OrderBy;
            string sql = "";

            if (status == Status.All)
            {
                sql = $@"
                WITH FinalList AS
                (
                    SELECT M.BankLimitMasterID
                    ,M.CompanyID
                    ,M.CurrencyID
	                ,M.BankID
	                ,M.BankFacilityTypeID
	                ,M.AccumulatedLimit

	                ,CompanyName = ISNULL(CE.ShortName,'')
	                ,CurrencyName = ISNULL(CU.CurrencyCode,'')
	                ,BankName = ISNULL(B.BankMasterName,'')
	                ,BankFacilityTypeName = ISNULL(BFT.Name, '')

	                ,FormBankFacilityName = ISNULL(BF.Name, '')
	                ,LiabilityTypeName = ISNULL(LT.Name, '')

	                ,C.BankLimitChildID
	                ,C.FromTenureDay
	                ,C.ToTenureDay
	                ,C.MaxLimit
	                ,C.LCOpened
	                ,C.LCAcceptenceGiven
	                ,C.PaymentOnMaturity

                    FROM BankLimitMaster M
	                INNER JOIN BankLimitChild C ON C.BankLimitMasterID = M.BankLimitMasterID
                    INNER JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = M.CompanyID 
                    LEFT JOIN {DbNames.EPYSL}..Currency CU ON CU.CurrencyID = M.CurrencyID
	                LEFT JOIN {DbNames.EPYSL}..BankMaster B ON B.BankMasterID = M.BankID
	                LEFT JOIN BankFacilityType BFT ON BFT.BankFacilityTypeID = M.BankFacilityTypeID
	                LEFT JOIN FormBankFacility BF ON BF.FormBankFacilityID = C.FormBankFacilityID
	                LEFT JOIN LiabilityType LT ON LT.LiabilityTypeID = C.LiabilityTypeID
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

                --Currency
                SELECT id = CurrencyID, text = CurrencyCode FROM {DbNames.EPYSL}..Currency;
                
                --Bank
                 {CommonQueries.GetBanks()};

                --BankFacility
                 SELECT id = BankFacilityTypeID, text = [Name]
                 FROM BankFacilityType
                 ORDER BY [Name];

                --FormBankFacility
                 SELECT id = FormBankFacilityID, text = [Name]
                 FROM FormBankFacility
                 ORDER BY [Name];
                
                --LiabilityType
                 SELECT id = LiabilityTypeID, text = [Name]
                 FROM LiabilityType
                 ORDER BY [Name];

              ";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);

                BankLimitMaster data = new BankLimitMaster();
                data.CompanyList = await records.ReadAsync<Select2OptionModel>();
                data.CurrencyList = await records.ReadAsync<Select2OptionModel>();
                data.BankList = await records.ReadAsync<Select2OptionModel>();
                data.BankFacilityTypeList = await records.ReadAsync<Select2OptionModel>();
                data.FormBankFacilityList = await records.ReadAsync<Select2OptionModel>();
                data.LiabilityTypeList = await records.ReadAsync<Select2OptionModel>();

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

            {CommonQueries.GetCompany()};

            --Currency
            SELECT id = CurrencyID, text = CurrencyCode FROM {DbNames.EPYSL}..Currency;
                
            --Bank
            {CommonQueries.GetBanks()};

            --BankFacility
            SELECT id = BankFacilityTypeID, text = [Name]
            FROM BankFacilityType
            ORDER BY [Name];

            --FormBankFacility
            SELECT id = FormBankFacilityID, text = [Name]
            FROM FormBankFacility
            ORDER BY [Name];
                
            --LiabilityType
            SELECT id = LiabilityTypeID, text = [Name]
            FROM LiabilityType
            ORDER BY [Name];

            ";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                BankLimitMaster data = records.Read<BankLimitMaster>().FirstOrDefault();
                data.Childs = records.Read<BankLimitChild>().ToList();
                
                data.CompanyList = await records.ReadAsync<Select2OptionModel>();
                data.CurrencyList = await records.ReadAsync<Select2OptionModel>();
                data.BankList = await records.ReadAsync<Select2OptionModel>();
                data.BankFacilityTypeList = await records.ReadAsync<Select2OptionModel>();
                data.FormBankFacilityList = await records.ReadAsync<Select2OptionModel>();
                data.LiabilityTypeList = await records.ReadAsync<Select2OptionModel>();

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
