using Dapper;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Application.Interfaces.Admin;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using System.Data.Entity;
using Microsoft.Data.SqlClient;
using EPYSLTEXCore.Infrastructure.Entities.Tex.General.Yarn;

namespace EPYSLTEXCore.Application.Services.General
{
    public class YarnPropertiesMappingService : IYarnPropertiesMappingService
    {

        private readonly IDapperCRUDService<YarnPropertiesMapping> _service;
        private readonly SqlConnection _connection;
        private readonly SqlConnection _connectionGmt;

        public YarnPropertiesMappingService(IDapperCRUDService<YarnPropertiesMapping> service)
        {
            _service = service;
            _service.Connection = service.GetConnection(AppConstants.GMT_CONNECTION);
            _connectionGmt = service.Connection;

            _service.Connection = service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _connection = service.Connection;
        }

        public async Task<List<YarnPropertiesMapping>> GetPagedAsync(PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? " ORDER BY YarnPropertiesMappingID DESC" : paginationInfo.OrderBy;
            var sql = string.Empty;

            sql += $@"WITH
                        FinalList AS
                        (
					        SELECT * FROM {TableNames.YarnPropertiesMapping} 
			            )
                        SELECT *, Count(*) Over() TotalRows FROM FinalList";

            sql += $@"
                  {paginationInfo.FilterBy}
                  {orderBy}
                  {paginationInfo.PageBy}";
            return await _service.GetDataAsync<YarnPropertiesMapping>(sql);
        }
        public async Task<List<YarnPropertiesMapping>> GetAsync(YarnPropertiesMapping entitie)
        {
            var query =
                $@"
                select * from {TableNames.YarnPropertiesMapping} 
                where YarnPropertiesMappingID = {entitie.YarnPropertiesMappingID} AND ColorID = '';";
            return await _service.GetDataAsync<YarnPropertiesMapping>(query);
        }


        public async Task<YarnPropertiesMapping> GetAsync(int yarnPropertiesMappingID)
        {
            var sql =
                $@"SELECT * FROM {TableNames.YarnPropertiesMapping} 
							Where YarnPropertiesMappingID = {yarnPropertiesMappingID}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YarnPropertiesMapping data = records.Read<YarnPropertiesMapping>().FirstOrDefault();
                Guard.Against.NullObject(data);

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
        public async Task<YarnPropertiesMapping> GetMaster()
        {
            var sql =
                $@"--Fiber Type
                    SELECT id=SegmentValueID,text=SegmentValue FROM EPYSL..ItemSegmentValue 
                    WHERE SegmentNameID IN (261) AND SegmentValueID IN (58957,58956,58955) ORDER BY SegmentValue

                    --Blend Type
                    SELECT id=1, text='Non Blended'
                    UNION
                    SELECT id=2, text='Blended';

                    -- Fabric Components
                    --{CommonQueries.GetFabricComponents(EntityTypeNameConstants.FABRIC_TYPE)};

                    --Program
                    SELECT id=SegmentValueID, text=SegmentNameID FROM EPYSL..ItemSegmentValue WHERE SegmentNameID IN (262)
                    
                    --Sub-Program
                    SELECT id=1, text='Sub-Program-A'
                    UNION
                    SELECT id=2, text='Sub-Program-B'
                    UNION
                    SELECT id=3, text='Sub-Program-C';

                    --Certificate
                    SELECT id=1, text='Certificate-A'
                    UNION
                    SELECT id=2, text='Certificate-B'
                    UNION
                    SELECT id=3, text='Certificate-C';

                    --Technical Parameter
                    SELECT id=1, text='TP-1'
                    UNION
                    SELECT id=2, text='TP-2'
                    UNION
                    SELECT id=3, text='TP-3';

                    --Yarn Composition
                    SELECT id=1, text='Composition-A'
                    UNION
                    SELECT id=2, text='Composition-B'
                    UNION
                    SELECT id=3, text='Composition-C';

                    --Shade Reference
                    SELECT id=1, text='Shade Reference-A'
                    UNION
                    SELECT id=2, text='Shade Reference-B'
                    UNION
                    SELECT id=3, text='Shade Reference-C';

                    --Manufacturing Line
                    SELECT id=1, text='Manufacturing Line-A'
                    UNION
                    SELECT id=2, text='Manufacturing Line-B'
                    UNION
                    SELECT id=3, text='Manufacturing Line-C';

                    --Manufacturing Process
                    SELECT id=1, text='Manufacturing Process-A'
                    UNION
                    SELECT id=2, text='Manufacturing Process-B'
                    UNION
                    SELECT id=3, text='Manufacturing Process-C';

                    --Manufacturing Sub-process
                    SELECT id=1, text='Manufacturing Sub-process-A'
                    UNION
                    SELECT id=2, text='Manufacturing Sub-process-B'
                    UNION
                    SELECT id=3, text='Manufacturing Sub-process-C';

                    --Color
                    SELECT id=1, text='Color-1'
                    UNION
                    SELECT id=2, text='Color-2'
                    UNION
                    SELECT id=3, text='Color-3';

                    --Color Grade
                    SELECT id=1, text='Color Grade-1'
                    UNION
                    SELECT id=2, text='Color Grade-2'
                    UNION
                    SELECT id=3, text='Color Grade-3';

                    --Count
                    SELECT id=1, text='Count-A'
                    UNION
                    SELECT id=2, text='Count-B'
                    UNION
                    SELECT id=3, text='Count-C';";  
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YarnPropertiesMapping data = new YarnPropertiesMapping();
                data.FiberTypeList = records.Read<Select2OptionModel>().ToList();
                data.BlendTypeList = records.Read<Select2OptionModel>().ToList();
                data.YarnTypeList = records.Read<Select2OptionModel>().ToList();
                data.ProgramList = records.Read<Select2OptionModel>().ToList();
                data.SubProgramList = records.Read<Select2OptionModel>().ToList();
                data.CertificationList = records.Read<Select2OptionModel>().ToList();
                data.TechnicalParameterList = records.Read<Select2OptionModel>().ToList();
                data.YarnCompositionList = records.Read<Select2OptionModel>().ToList();
                data.ShadeReferenceList = records.Read<Select2OptionModel>().ToList();
                data.ManufacturingLineList = records.Read<Select2OptionModel>().ToList();
                data.ManufacturingProcessList = records.Read<Select2OptionModel>().ToList();
                data.ManufacturingSubProcessList = records.Read<Select2OptionModel>().ToList();
                data.ColorList = records.Read<Select2OptionModel>().ToList();
                data.ColorGradeList = records.Read<Select2OptionModel>().ToList();
                data.YarnCountList = records.Read<Select2OptionModel>().ToList();
                Guard.Against.NullObject(data);

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
        public async Task SaveAsync(YarnPropertiesMapping entity)
        {
            SqlTransaction transaction = null;
            SqlTransaction transactionGmt = null;
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                await _connectionGmt.OpenAsync();
                transactionGmt = _connectionGmt.BeginTransaction();

                switch (entity.EntityState)
                {
                    case EntityState.Added:
                        entity.YarnPropertiesMappingID = await _service.GetMaxIdAsync(TableNames.YarnPropertiesMapping, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        break;

                    //case EntityState.Modified:
                    //    await UpdateAsync(entity);
                    //    break;

                    default:
                        break;
                }

                await _service.SaveSingleAsync(entity, _connection, transaction);

                transaction.Commit();
                transactionGmt.Commit();
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                if (transactionGmt != null) transactionGmt.Rollback();
                throw (ex);
            }
            finally
            {
                if (transaction != null) transaction.Dispose();
                if (transactionGmt != null) transactionGmt.Dispose();
                _connection.Close();
                _connectionGmt.Close();
            }
        }
        private async Task<YarnPropertiesMapping> AddAsync(YarnPropertiesMapping entity, SqlTransaction transactionGmt)
        {
            entity.YarnPropertiesMappingID = await _service.GetMaxIdAsync(TableNames.YarnPropertiesMapping, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

            return entity;
        }
        
    }
}
