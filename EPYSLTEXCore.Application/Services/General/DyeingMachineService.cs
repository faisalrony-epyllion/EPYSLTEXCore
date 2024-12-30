using Dapper;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Application.Interfaces;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.General;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using System.Data.Entity;
using Microsoft.Data.SqlClient;
using System.Configuration;

namespace EPYSLTEXCore.Application.Services.General
{
    internal class DyeingMachineService : IDyeingMachineService
    {
        private readonly IDapperCRUDService<DyeingMachine> _service;
        private readonly SqlConnection _connection;
        private readonly SqlConnection _connectionGmt;

        public DyeingMachineService(IDapperCRUDService<DyeingMachine> service)
        {
            _service = service;
            _service.Connection = service.GetConnection(AppConstants.GMT_CONNECTION);
            _connectionGmt = service.Connection;

            _service.Connection = service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _connection = service.Connection;
        }

        public async Task<List<DyeingMachine>> GetPagedAsync(PaginationInfo paginationInfo)
        {
            paginationInfo.OrderBy = string.IsNullOrEmpty(paginationInfo.OrderBy) ? "ORDER BY DMID DESC" : paginationInfo.OrderBy;
            var sql = $@"
                WITH M AS (
                    SELECT DMID, D.IsCC, D.DyeingMcNameId, D.CompanyId, D.DyeingMcslNo, D.DyeingMcStatusId, D.DyeingMcBrandId, D.DyeingMcCapacity, D.DyeingNozzleQty,
                    DMCN.DyeingMCName DyeingMcName, C.UnitName Company, MT.DyeingMCStatus DyeingMcStatus, Brand.ValueName DyeingMCBrand
                    FROM {TableNames.DYEING_MACHINE} D
                    INNER JOIN {TableNames.DYEING_MC_NAME_SETUP} DMCN ON DMCN.DyeingMCNameID=D.DyeingMcNameId
                    LEFT JOIN {TableNames.KNITTING_UNIT} C ON C.KnittingUnitID =D.CompanyId
                    INNER JOIN {TableNames.DyeingMCStatusSetup} MT ON MT.DyeingMCStatusID = D.DyeingMcStatusId
                    LEFT JOIN {DbNames.EPYSL}..EntityTypeValue Brand ON Brand.ValueID=D.DyeingMcBrandId
			    )

			    SELECT *, Count(*) Over() TotalRows FROM M
                {paginationInfo.FilterBy}
                {paginationInfo.OrderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<DyeingMachine>(sql);
        }

        public async Task<List<DyeingMachine>> GetNozzleInfoAsync(PaginationInfo paginationInfo)
        {
            paginationInfo.OrderBy = string.IsNullOrEmpty(paginationInfo.OrderBy) ? "ORDER BY DMID DESC" : paginationInfo.OrderBy;
            var sql = $@"
                WITH D AS
                (
					SELECT * FROM {TableNames.DYEING_MACHINE}
			    )
			    SELECT DMID, D.IsCC, D.DyeingMcNameId, D.CompanyId, D.DyeingMcslNo, D.DyeingMcStatusId, D.DyeingMcBrandId, D.DyeingMcCapacity, D.DyeingNozzleQty,
                DMCN.DyeingMCName DyeingMcName, C.UnitName Company, MT.DyeingMCStatus DyeingMcStatus, Brand.ValueName DyeingMCBrand, Count(*) Over() TotalRows
                FROM D
                INNER JOIN {TableNames.DYEING_MC_NAME_SETUP} DMCN ON DMCN.DyeingMCNameID=D.DyeingMcNameId
                LEFT JOIN {TableNames.KNITTING_UNIT} C ON C.KnittingUnitID =D.CompanyId
                INNER JOIN {TableNames.DyeingMCStatusSetup} MT ON MT.DyeingMCStatusID = D.DyeingMcStatusId
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue Brand ON Brand.ValueID=D.DyeingMcBrandId
                {paginationInfo.FilterBy}
                {paginationInfo.OrderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<DyeingMachine>(sql);
        }

        public async Task<DyeingMachine> GetNewAsync()
        {
            var sql = $@"
                ----MC Name
                {CommonQueries.GetDyeingMCNames()};

                ----CompANY
                {CommonQueries.GetKnittingUnit()};

                ----MC status
                {CommonQueries.GetDyeingMachine()};

                ----MC Brand
                {CommonQueries.GetEntityTypesByEntityTypeName(EntityTypeNameConstants.DYEING_MACHINE_BRAND)};

                ----DyeProcessList
                {CommonQueries.GetEntityTypesByEntityTypeName("Dyeing Machine Dye Process")};";

            // var connection = _dbContext.Database.Connection;
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                DyeingMachine data = new DyeingMachine
                {
                    DyeingMcNameList = records.Read<Select2OptionModel>().ToList(),
                    CompanyList = records.Read<Select2OptionModel>().ToList(),
                    DyeingMcStatusList = records.Read<Select2OptionModel>().ToList(),
                    DyeingMcBrandList = records.Read<Select2OptionModel>().ToList(),
                    DyeProcessList = records.Read<Select2OptionModel>().ToList()
                };

                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _connection.Close();
            }
        }

        public async Task<DyeingMachine> GetAsync(int id)
        {
            var sql = $@"
                ;SELECT DMID, D.IsCC, D.DyeingMcNameId, D.CompanyId, D.DyeingMcslNo, D.DyeingMcStatusId, D.DyeingMcBrandId, D.DyeingMcCapacity, D.DyeingNozzleQty,
                DMCN.DyeingMCName DyeingMcName, C.UnitName Company, MT.DyeingMCStatus DyeingMcStatus, Brand.ValueName DyeingMCBrand, Count(*) Over() TotalRows
                FROM {TableNames.DYEING_MACHINE} D
                INNER JOIN {TableNames.DYEING_MC_NAME_SETUP} DMCN ON DMCN.DyeingMCNameID=D.DyeingMcNameId
                LEFT JOIN {TableNames.KNITTING_UNIT} C ON C.KnittingUnitID =D.CompanyId
                INNER JOIN {TableNames.DyeingMCStatusSetup} MT ON MT.DyeingMCStatusID = D.DyeingMcStatusId
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue Brand ON Brand.ValueID=D.DyeingMcBrandId
			    WHERE D.DMID = {id};

                ----Dye Process
                ;SELECT DMProcessID, DMID, DyeProcessID FROM {TableNames.DYEING_MACHINE}Process WHERE DMID = {id};

                ----MC Name
                {CommonQueries.GetDyeingMCNames()};

                ----CompANY
                {CommonQueries.GetKnittingUnit()};

                ----MC status
                {CommonQueries.GetDyeingMachine()};

                ----MC Brand
                {CommonQueries.GetEntityTypesByEntityTypeName(EntityTypeNameConstants.DYEING_MACHINE_BRAND)};

                ----DyeProcessList
                {CommonQueries.GetEntityTypesByEntityTypeName("Dyeing Machine Dye Process")};";

            //var connection = _dbContext.Database.Connection;
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                DyeingMachine data = records.Read<DyeingMachine>().FirstOrDefault();
                data.DyeingMachineProcesses = records.Read<DyeingMachineProcess>().ToList();
                data.DyeingMcNameList = records.Read<Select2OptionModel>().ToList();
                data.CompanyList = records.Read<Select2OptionModel>().ToList();
                data.DyeingMcStatusList = records.Read<Select2OptionModel>().ToList();
                data.DyeingMcBrandList = records.Read<Select2OptionModel>().ToList();
                data.DyeProcessList = records.Read<Select2OptionModel>().ToList();

                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _connection.Close();
            }
        }

        public async Task<List<DyeingMachine>> GetDyeingMachineByNozzleListAsync(int nozzle)
        {
            var sql = $@"
                WITH D AS
                (
					SELECT * FROM {TableNames.DYEING_MACHINE} WHERE DyeingNozzleQty = {nozzle}
			    )
			    SELECT DMID, D.IsCC, D.DyeingMcNameId, D.CompanyId, D.DyeingMcslNo, D.DyeingMcStatusId, D.DyeingMcBrandId, D.DyeingMcCapacity, D.DyeingNozzleQty,
                DMCN.DyeingMCName DyeingMcName, C.UnitName Company, MT.DyeingMCStatus DyeingMcStatus, Brand.ValueName DyeingMCBrand
                FROM D
                INNER JOIN {TableNames.DYEING_MC_NAME_SETUP} DMCN ON DMCN.DyeingMCNameID=D.DyeingMcNameId
                LEFT JOIN {TableNames.KNITTING_UNIT} C ON C.KnittingUnitID =D.CompanyId
                INNER JOIN {TableNames.DyeingMCStatusSetup} MT ON MT.DyeingMCStatusID = D.DyeingMcStatusId
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue Brand ON Brand.ValueID=D.DyeingMcBrandId";

            return await _service.GetDataAsync<DyeingMachine>(sql);
        }

        public async Task<DyeingMachine> GetAllAsync(int id)
        {
            var sql = $@"
            ;Select * FROM {TableNames.DYEING_MACHINE} Where DMID = {id}

            ;Select * FROM {TableNames.DYEING_MACHINE}Process Where DMID = {id}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                DyeingMachine data = await records.ReadFirstOrDefaultAsync<DyeingMachine>();
                Guard.Against.NullObject(data);
                data.DyeingMachineProcesses = records.Read<DyeingMachineProcess>().ToList();
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
        public async Task SaveAsync(DyeingMachine entity)
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
                List<DyeingMachineProcess> childRecords = entity.DyeingMachineProcesses;
                switch (entity.EntityState)
                {
                    case EntityState.Added:
                        entity.DMID = await _service.GetMaxIdAsync(TableNames.DYEING_MACHINE, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);


                        maxChildId = await _service.GetMaxIdAsync(TableNames.DYEING_MACHINE_PROCESS, entity.DyeingMachineProcesses.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        foreach (var item in entity.DyeingMachineProcesses)
                        {
                            item.DMProcessID = maxChildId++;
                            item.DMID = entity.DMID;
                        }
                        break;

                    case EntityState.Modified:
                        var addedChilds = entity.DyeingMachineProcesses.FindAll(x => x.EntityState == EntityState.Added);
                        maxChildId = await _service.GetMaxIdAsync(TableNames.DYEING_MACHINE_PROCESS, addedChilds.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

                        foreach (var item in addedChilds)
                        {
                            item.DMProcessID = maxChildId++;
                            item.DMID = entity.DMID;
                        }
                        break;

                    case EntityState.Unchanged:
                    case EntityState.Deleted:
                        entity.EntityState = EntityState.Deleted;
                        entity.DyeingMachineProcesses.SetDeleted();
                        break;

                    default:
                        break;
                }

                await _service.SaveSingleAsync(entity, transaction);
                await _service.SaveAsync(entity.DyeingMachineProcesses, transaction);

                transaction.Commit();
                transactionGmt.Commit();
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                if (transactionGmt != null) transactionGmt.Rollback();
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
