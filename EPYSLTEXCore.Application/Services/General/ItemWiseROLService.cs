﻿using Dapper;
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

namespace EPYSLTEXCore.Application.Services.General
{
    public class ItemWiseROLService : IItemWiseROLService
    {

        private readonly IDapperCRUDService<ItemMasterReOrderStatus> _service;
        private readonly SqlConnection _connection;
        private readonly SqlConnection _connectionGmt;

        public ItemWiseROLService(IDapperCRUDService<ItemMasterReOrderStatus> service)
        {
            _service = service;
            _service.Connection = service.GetConnection(AppConstants.GMT_CONNECTION);
            _connectionGmt = service.Connection;

            _service.Connection = service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _connection = service.Connection;
        }

        public async Task<List<ItemMasterReOrderStatus>> GetPagedAsync(PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? " ORDER BY ROSID DESC" : paginationInfo.OrderBy;
            var sql = string.Empty;

            sql += $@"WITH
                        FinalList AS
                        (
					        SELECT IMROL.ROSID,IMROL.ItemMasterID,IMROL.SubGroupID,IMROL.CompanyID,MonthlyAvgConsumptionLP,MonthlyAvgConsumptionFP,
                            ROLLocalPurchase,ROLForeignPurchase,ReOrderQty,MaximumPRQtyLP,MaximumPRQtyFP,MOQ,ValidDate,
                            IM.ItemName,ISG.SubGroupName,CE.ShortName CompanyName
                            FROM {TableNames.ItemMasterReOrderStatus} IMROL
                            INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID=IMROL.ItemMasterID
                            INNER JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID=IMROL.SubGroupID
                            LEFT JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID=IMROL.CompanyID
			            )
                        SELECT *, Count(*) Over() TotalRows FROM FinalList";

            sql += $@"
                  {paginationInfo.FilterBy}
                  {orderBy}
                  {paginationInfo.PageBy}";
            return await _service.GetDataAsync<ItemMasterReOrderStatus>(sql);
        }
        
        public async Task<ItemMasterReOrderStatus> GetAsync(int rosid)
        {
            var sql =
                $@"SELECT *
			                FROM {TableNames.ItemMasterReOrderStatus} 
							WHERE ROSID = {rosid}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                ItemMasterReOrderStatus data = records.Read<ItemMasterReOrderStatus>().FirstOrDefault();
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
        public async Task<ItemMasterReOrderStatus> GetMaster()
        {
            var sql =
                $@"
                --Item Sub-Group---
                SELECT id=SubGroupID,text=SubGroupName FROM {DbNames.EPYSL}..ItemSubGroup
                ----Company
                ;With BFL As (
	                Select BondFinancialYearID, ImportLimit - Consumption As AvailableLimit
	                From {DbNames.EPYSL}..BondFinancialYearImportLimit
	                Where SubGroupID = 102
                )
                , Al As (
	                Select BFY.CompanyID, AvailableLimit
	                From {DbNames.EPYSL}..BondFinancialYear BFY
	                Inner Join  BFL On BFY.BondFinancialYearID = BFY.BondFinancialYearID
	                Group By BFY.CompanyID, AvailableLimit
                )
                Select Cast (CE.CompanyID as varchar) [id] , CE.ShortName [text]
                From (select SubGroupID, ContactID from {DbNames.EPYSL}..SupplierItemGroupStatus Group By SubGroupID, ContactID) SIGS
                Inner Join {DbNames.EPYSL}..Contacts C On SIGS.ContactID = C.ContactID
                Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = SIGS.SubGroupID
                Inner Join {DbNames.EPYSL}..ContactAdditionalInfo CAI On CAI.ContactID = SIGS.ContactID
                Inner Join {DbNames.EPYSL}..CompanyEntity CE On C.MappingCompanyID = CE.CompanyID
                Where ISG.SubGroupName = 'Fabric' And Isnull(CAI.InHouse,0) = 1 Group by CE.CompanyID, CE.CompanyName, CE.ShortName;";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                ItemMasterReOrderStatus data = new ItemMasterReOrderStatus();
                data.SubGroupList = records.Read<Select2OptionModel>().ToList();
                data.CompanyList = records.Read<Select2OptionModel>().ToList();
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
        public async Task SaveAsync(ItemMasterReOrderStatus entity)
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
                        entity.ROSID = await _service.GetMaxIdAsync(TableNames.ItemMasterReOrderStatus, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
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
        public async Task<List<ItemMasterReOrderStatus>> GetItemMasterDataAsync(PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By ItemName" : paginationInfo.OrderBy;

            var query = $@"
                WITH FinalList AS
                (
                    SELECT IM.ItemMasterID, ISG.SubGroupID , ISG.SubGroupName, IM.ItemName
				    FROM {DbNames.EPYSL}..ItemMaster IM
				    INNER JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID=IM.SubGroupID
                    WHERE ISNULL(IM.ItemName,'') <> '' AND ISG.SubGroupName = 'Yarn Live'
                )
                SELECT *, COUNT(*) Over() TotalRows FROM FinalList

                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<ItemMasterReOrderStatus>(query);
        }
        public async Task<bool> CheckDuplicateValue(ItemMasterReOrderStatus model)
        {
            var condition = model.ROSID > 0 ? $" AND ROSID!={model.ROSID}" : "";
            var sql = $@"SELECT *
			                FROM {TableNames.ItemMasterReOrderStatus} 
							WHERE ItemMasterID = {model.ItemMasterID} AND CompanyID = {model.CompanyID}"+ condition;

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                ItemMasterReOrderStatus data = records.Read<ItemMasterReOrderStatus>().FirstOrDefault();
                return data == null ? false : true;
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
