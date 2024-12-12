using Dapper;
using EPYSLTEX.Core.Interfaces.Repositories;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Application.Interfaces;
using EPYSLTEXCore.Application.Interfaces.Repositories;
using EPYSLTEXCore.Application.Interfaces.RND;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.DTOs;
using EPYSLTEXCore.Infrastructure.Entities.Gmt.General;
using EPYSLTEXCore.Infrastructure.Entities.Tex.General;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Knitting;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace EPYSLTEXCore.Application.Services.General
{
    public class SpinnerWiseYarnPackingHKService : ISpinnerWiseYarnPackingHKService
    {

        private readonly IDapperCRUDService<SpinnerWiseYarnPackingHK> _service;
        //private readonly IDapperCRUDService<SpinnerWiseYarnPackingHK> _gmtservice;
        //private readonly ISqlQueryRepository<SpinnerWiseYarnPackingHK> _sqlQueryRepository;
        //private readonly ISignatureRepository _signatureRepository;
        //private SqlTransaction _transaction = null;
        //private SqlTransaction _transactionGmt = null;
        private readonly SqlConnection _connection;
        private readonly SqlConnection _connectionGmt;

        public SpinnerWiseYarnPackingHKService(IDapperCRUDService<SpinnerWiseYarnPackingHK> service
            //, ISqlQueryRepository<SpinnerWiseYarnPackingHK> sqlQueryRepository
            //, ISignatureRepository signatureRepository
            )
        {
            //_service = service;
            //_sqlQueryRepository = sqlQueryRepository;
            ////_signatureRepository = signatureRepository;
            //_connection = _service.Connection;
            ////_gmtservice = gmtservice;
            ////_gmtservice.Connection = service.GetConnection(AppConstants.GMT_CONNECTION);

            _service = service;
            //_sqlQueryRepository = sqlQueryRepository;
            _service.Connection = service.GetConnection(AppConstants.GMT_CONNECTION);
            _connectionGmt = service.Connection;

            _service.Connection = service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _connection = service.Connection;
        }

        public async Task<List<SpinnerWiseYarnPackingHK>> GetPagedAsync(PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? " ORDER BY YarnPackingID DESC" : paginationInfo.OrderBy;
            var sql = string.Empty;

            sql += $@"WITH
                        FinalList AS
                        (
					        select YP.*,C.ShortName Spinner
			                from {TableNames.SpinnerWiseYarnPackingHK} YP
			                LEFT JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID=YP.SpinnerID 
			            )
                        SELECT *, Count(*) Over() TotalRows FROM FinalList";

            sql += $@"
                  {paginationInfo.FilterBy}
                  {orderBy}
                  {paginationInfo.PageBy}";
            return await _service.GetDataAsync<SpinnerWiseYarnPackingHK>(sql);
        }
        public async Task<List<SpinnerWiseYarnPackingHK>> GetAsync(SpinnerWiseYarnPackingHK entitie)
        {
            var query =
                $@"
                select * from {TableNames.SpinnerWiseYarnPackingHK} 
                where SpinnerID = {entitie.SpinnerID} AND PackNo = '{entitie.PackNo}';";

            //return await _sqlQueryRepository.GetDataDapperAsync<SpinnerWiseYarnPackingHK>(query);
            return await _service.GetDataAsync<SpinnerWiseYarnPackingHK>(query);
        }


        public async Task<SpinnerWiseYarnPackingHK> GetAsync(int yarnPackingID)
        {
            var sql =
                $@"select YP.*,C.ShortName Spinner
			                from {TableNames.SpinnerWiseYarnPackingHK} YP
			                LEFT JOIN EPYSL..Contacts C ON C.ContactID=YP.SpinnerID
							Where YP.YarnPackingID = {yarnPackingID}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                SpinnerWiseYarnPackingHK data = records.Read<SpinnerWiseYarnPackingHK>().FirstOrDefault();
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
        //public async Task<List<Select2OptionModel>> GetAllSpinner()
        //{
        //    var sql =
        //        $@"--SpinnerList
        //            {CommonQueries.GetYarnSpinners()};";
        //    return await _service.GetDataAsync<Select2OptionModel>(sql);
        //}
        public async Task<SpinnerWiseYarnPackingHK> GetMaster()
        {
            var sql =
                $@"--SpinnerList
                    {CommonQueries.GetYarnSpinners()};";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                SpinnerWiseYarnPackingHK data = new SpinnerWiseYarnPackingHK();
                data.SpinnerList = records.Read<Select2OptionModel>().ToList();
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
        public async Task SaveAsync(SpinnerWiseYarnPackingHK entity)
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
                        //entity = await AddAsync(entity, _transactionGmt);
                        entity.YarnPackingID = await _service.GetMaxIdAsync(TableNames.SpinnerWiseYarnPackingHK, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        break;

                    //case EntityState.Modified:
                    //    await UpdateAsync(entity);
                    //    break;

                    default:
                        break;
                }

                await _service.SaveSingleAsync(entity, transaction);

                transaction.Commit();
                transactionGmt.Commit();
            }
            //catch (DbEntityValidationException ex)
            //{
            //    var fex = new FormattedDbEntityValidationException(ex);
            //    throw new Exception(fex.Message);
            //}
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
        private async Task<SpinnerWiseYarnPackingHK> AddAsync(SpinnerWiseYarnPackingHK entity, SqlTransaction transactionGmt)
        {
            entity.YarnPackingID = await _service.GetMaxIdAsync(TableNames.SpinnerWiseYarnPackingHK, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);


            return entity;
        }

        //private async Task UpdateAsync(SegmentValueYarnTypeMappingSetup entity)
        //{
        //    var maxReqChildId = await _signatureRepository.GetMaxIdAsync(TableNames.BATCH_ITEM_REQUIREMENT, entity.BatchItemRequirements.Where(x => x.EntityState == EntityState.Added).Count());
        //    var maxRecipeChildId = await _signatureRepository.GetMaxIdAsync(TableNames.BATCH_WISE_RECIPE_CHILD, entity.BatchWiseRecipeChilds.Where(x => x.EntityState == EntityState.Added).Count());
        //    var maxChildId = await _signatureRepository.GetMaxIdAsync(TableNames.BATCH_CHILD, entity.BatchItemRequirements.Sum(x => x.BatchChilds.Where(y => y.EntityState == EntityState.Added).Count()));

        //    foreach (var item in entity.BatchItemRequirements)
        //    {
        //        foreach (var child in item.BatchChilds)
        //        {
        //            switch (child.EntityState)
        //            {
        //                case EntityState.Added:
        //                    child.BChildID = maxChildId++;
        //                    child.BatchID = entity.BatchID;
        //                    child.BItemReqID = item.BItemReqID;
        //                    break;

        //                case EntityState.Deleted:
        //                case EntityState.Unchanged:
        //                    child.EntityState = EntityState.Deleted;
        //                    break;

        //                case EntityState.Modified:
        //                    child.EntityState = EntityState.Modified;
        //                    break;

        //                default:
        //                    break;
        //            }
        //        }

        //        switch (item.EntityState)
        //        {
        //            case EntityState.Added:
        //                item.BItemReqID = maxReqChildId++;
        //                item.BatchID = entity.BatchID;
        //                break;

        //            case EntityState.Modified:
        //                item.EntityState = EntityState.Modified;
        //                break;

        //            case EntityState.Deleted:
        //            case EntityState.Unchanged:
        //                item.EntityState = EntityState.Deleted;
        //                break;

        //            default:
        //                break;
        //        }
        //    }

        //    foreach (var item in entity.BatchWiseRecipeChilds)
        //    {
        //        switch (item.EntityState)
        //        {
        //            case EntityState.Added:
        //                item.BRecipeChildID = maxRecipeChildId++;
        //                break;

        //            case EntityState.Modified:
        //                item.EntityState = EntityState.Modified;
        //                break;

        //            case EntityState.Deleted:
        //            case EntityState.Unchanged:
        //                item.EntityState = EntityState.Deleted;
        //                break;

        //            default:
        //                break;
        //        }
        //    }
        //}
    }
}
