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
    public class BondEntitlementService : IBondEntitlementService
    {
        private readonly IDapperCRUDService<BondEntitlementMaster> _service;

        private readonly SqlConnection _connection;
        private readonly SqlConnection _connectionGmt;

        public BondEntitlementService(IDapperCRUDService<BondEntitlementMaster> service)
        {
            _service = service;

            _service.Connection = service.GetConnection(AppConstants.GMT_CONNECTION);
            _connectionGmt = service.Connection;

            _service.Connection = service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _connection = service.Connection;
        }

        public async Task<List<BondEntitlementMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? " ORDER BY CompanyName, BondEntitlementMasterID DESC " : paginationInfo.OrderBy;
            string sql = "";

            if (status == Status.All)
            {
                sql = $@"WITH FinalList AS
                (
	                SELECT BEM.BondEntitlementMasterID
	                ,C.CompanyID
	                ,C.CompanyName
	                ,BondLicenceNo = ISNULL(BEM.BondLicenceNo,'')
	                ,EBINNo = ISNULL(BEM.EBINNo,'')
	                ,BEM.FromDate
	                ,BEM.ToDate
	                ,CurrencyID = ISNULL(BEM.CurrencyID,0)
	                ,CurrencyName = ISNULL(CU.CurrencyCode,'')
	                FROM BondEntitlementMaster BEM
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

            return await _service.GetDataAsync<BondEntitlementMaster>(sql);
        }

        public async Task<BondEntitlementMaster> GetNewAsync()
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

                BondEntitlementMaster data = new BondEntitlementMaster();
                data.CompanyList = await records.ReadAsync<Select2OptionModel>();
                var itemSegmentNames = await records.ReadAsync<Select2OptionModel>();
                var itemSegmentValues = await records.ReadAsync<Select2OptionModel>();
                data.CurrencyList = await records.ReadAsync<Select2OptionModel>();
                data.UnitList = await records.ReadAsync<Select2OptionModel>();

                data.Childs = new List<BondEntitlementChild>();

                int childId = 10000;
                int childItemId = 10000;
                foreach (var isn in itemSegmentNames)
                {
                    var tempItemSegmentValues = itemSegmentValues.Where(x => x.additionalValue == isn.id).ToList();

                    BondEntitlementChild child = new BondEntitlementChild();
                    child.BondEntitlementChildID = childId++;
                    child.SegmentNameID = Convert.ToInt32(isn.id);
                    child.SegmentName = isn.text;
                    child.ChildItems = new List<BondEntitlementChildItem>();

                    foreach (var isv in tempItemSegmentValues)
                    {
                        BondEntitlementChildItem childItem = new BondEntitlementChildItem();
                        childItem.BondEntitlementChildItemID = childItemId++;
                        childItem.BondEntitlementChildID = child.BondEntitlementChildID;
                        childItem.SegmentValueID = Convert.ToInt32(isv.id);
                        childItem.SegmentValue = isv.text;
                        child.ChildItems.Add(childItem);
                    }
                    data.Childs.Add(child);
                }
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
            /*
            public async Task<List<BondEntitlementMaster>> GetAsync()
            {
                string query =
                    $@"
                    SELECT BondEntitlementMasterID = ISNULL(BEM.BondEntitlementMasterID,0)
                    ,C.CompanyID
                    ,C.CompanyName
                    ,BondLicenceNo = ISNULL(BEM.BondLicenceNo,'')
                    ,EBINNo = ISNULL(BEM.EBINNo,'')
                    ,BEM.FromDate
                    ,BEM.ToDate
                    ,CurrencyID = ISNULL(BEM.CurrencyID,0)
                    ,CurrencyName = ISNULL(CU.CurrencyCode,'')
                    FROM {DbNames.EPYSL}..CompanyEntity C
                    LEFT JOIN BondEntitlementMaster BEM ON BEM.CompanyID = C.CompanyID
                    LEFT JOIN {DbNames.EPYSL}..Currency CU ON CU.CurrencyID = BEM.CurrencyID
                    WHERE C.CompanyID IN (8,6)
                    ORDER BY C.CompanyName;

                    SELECT BondEntitlementChildID = ISNULL(BEC.BondEntitlementChildID,0)
                    ,BondEntitlementMasterID = ISNULL(BEC.BondEntitlementMasterID,0)
                    ,SegmentNameID = ISNULL(ISN.SegmentNameID,0)
                    ,HSCode = ISNULL(BEC.HSCode,'')
                    ,SegmentName = ISN.SegmentName 
                    ,UnitID = ISNULL(UnitID,0)
                    ,BankFacilityAmount = ISNULL(BankFacilityAmount,0)
                    FROM {DbNames.EPYSL}..ItemSegmentName ISN
                    LEFT JOIN BondEntitlementChild BEC ON BEC.SegmentNameID = ISN.SegmentNameID
                    WHERE ISN.SegmentNameID IN (270,273)

                    SELECT BondEntitlementChildItemID = ISNULL(BECI.BondEntitlementChildItemID,0)
                    ,BondEntitlementChildID = ISNULL(BECI.BondEntitlementChildID,0)
                    ,SegmentNameID = ISNULL(ISN.SegmentNameID,0)
                    ,SegmentValueID = ISV.SegmentValueID
                    ,HSCode = ISNULL(BEC.HSCode,'')
                    ,BankFacilityAmount = ISNULL(BankFacilityAmount,0)
                    ,SegmentValue = ISN.SegmentValue 
                    FROM {DbNames.EPYSL}..ItemSegmentValue ISV
                    INNER JOIN {DbNames.EPYSL}..ItemSegmentName ISN ON ISN.SegmentNameID = ISV.SegmentNameID
                    LEFT JOIN BondEntitlementChildItem BECI ON BECI.SegmentValueID = ISV.SegmentValueID
                    WHERE ISN.SegmentNameID IN (270,273)

                  ";

                try
                {
                    await _connection.OpenAsync();
                    var records = await _connection.QueryMultipleAsync(query);

                    var datas = await records.ReadAsync<BondEntitlementMaster>();
                    var childs = await records.ReadAsync<BondEntitlementChild>();
                    var childItems = await records.ReadAsync<BondEntitlementChildItem>();

                    foreach (var company in companys)
                    {
                        BondEntitlementMaster data = new BondEntitlementMaster();
                        data.CompanyID = Convert.ToInt32(company.id);
                        data.Childs = new List<BondEntitlementChild>();

                        foreach (var isn in itemSegmentNames)
                        {
                            var tempItemSegmentValues = itemSegmentValues.Where(x => x.additionalValue == isn.id);

                            BondEntitlementChild child = new BondEntitlementChild();
                            child.SegmentNameID = Convert.ToInt32(isn.id);
                            child.ChildItems = new List<BondEntitlementChildItem>();

                            foreach (var isv in tempItemSegmentValues)
                            {
                                BondEntitlementChildItem childItem = new BondEntitlementChildItem();
                                childItem.BondEntitlementChildID = child.BondEntitlementChildID;
                                childItem.SegmentValueID = Convert.ToInt32(isv.id);

                                child.ChildItems.Add(childItem);
                            }
                            data.Childs.Add(child);
                        }
                    }
                    return datas;
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
            */
        }
        public async Task<BondEntitlementMaster> GetDetails(int id)
        {
            string sql = $@"
            Select M.* 
            From BondEntitlementMaster M
            Where M.BondEntitlementMasterID = {id};

            Select C.* 
            From BondEntitlementMaster M
            INNER JOIN BondEntitlementChild C ON C.BondEntitlementMasterID = M.BondEntitlementMasterID
            Where M.BondEntitlementMasterID = {id};

            Select CI.* 
            From BondEntitlementMaster M
            INNER JOIN BondEntitlementChild C ON C.BondEntitlementMasterID = M.BondEntitlementMasterID
            INNER JOIN BondEntitlementChildItem CI ON CI.BondEntitlementChildID = C.BondEntitlementChildID
            Where M.BondEntitlementMasterID = {id};

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
                BondEntitlementMaster data = records.Read<BondEntitlementMaster>().FirstOrDefault();
                var childs = records.Read<BondEntitlementChild>().ToList();
                var childItems = records.Read<BondEntitlementChildItem>().ToList();

                data.CompanyList = await records.ReadAsync<Select2OptionModel>();
                var itemSegmentNames = await records.ReadAsync<Select2OptionModel>();
                var itemSegmentValues = await records.ReadAsync<Select2OptionModel>();
                data.CurrencyList = await records.ReadAsync<Select2OptionModel>();
                data.UnitList = await records.ReadAsync<Select2OptionModel>();

                int childId = 10000;
                int childItemId = 10000;
                foreach (var isn in itemSegmentNames)
                {
                    var tempItemSegmentValues = itemSegmentValues.Where(x => x.additionalValue == isn.id).ToList();

                    BondEntitlementChild child = childs.FirstOrDefault(x => x.SegmentNameID.ToString() == isn.id);
                    if (child.IsNull())
                    {
                        child = new BondEntitlementChild();
                        child.BondEntitlementChildID = childId++;
                    }
                    child.SegmentNameID = Convert.ToInt32(isn.id);
                    child.SegmentName = isn.text;
                    //child.ChildItems = new List<BondEntitlementChildItem>();

                    List<BondEntitlementChildItem> tempChildItems = new List<BondEntitlementChildItem>();
                    foreach (var isv in tempItemSegmentValues)
                    {
                        BondEntitlementChildItem childItem = childItems.FirstOrDefault(x => x.SegmentValueID.ToString() == isv.id && x.BondEntitlementChildID == child.BondEntitlementChildID);
                        if (childItem.IsNull())
                        {
                            childItem = new BondEntitlementChildItem();
                            childItem.BondEntitlementChildItemID = childItemId++;
                        }
                        childItem.BondEntitlementChildID = child.BondEntitlementChildID;
                        childItem.SegmentValueID = Convert.ToInt32(isv.id);
                        childItem.SegmentValue = isv.text;
                        child.ChildItems.Add(childItem);
                    }
                    data.Childs.Add(child);
                }

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
        public async Task<BondEntitlementMaster> GetById(int id)
        {
            string sql = $@"
            Select M.* 
            From BondEntitlementMaster M
            Where M.BondEntitlementMasterID = {id};

            Select C.* 
            From BondEntitlementMaster M
            INNER JOIN BondEntitlementChild C ON C.BondEntitlementMasterID = M.BondEntitlementMasterID
            Where M.BondEntitlementMasterID = {id};

            Select CI.* 
            From BondEntitlementMaster M
            INNER JOIN BondEntitlementChild C ON C.BondEntitlementMasterID = M.BondEntitlementMasterID
            INNER JOIN BondEntitlementChildItem CI ON CI.BondEntitlementChildID = C.BondEntitlementChildID
            Where M.BondEntitlementMasterID = {id};

            ";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                BondEntitlementMaster data = records.Read<BondEntitlementMaster>().FirstOrDefault();
                data.Childs = records.Read<BondEntitlementChild>().ToList();
                var childItems = records.Read<BondEntitlementChildItem>().ToList();

                foreach (var item in data.Childs)
                {
                    item.ChildItems = childItems.Where(x => x.BondEntitlementChildID == item.BondEntitlementChildID).ToList();
                }
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
        public async Task SaveAsync(BondEntitlementMaster entity)
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

                List<BondEntitlementChildItem> childItems = new List<BondEntitlementChildItem>();
                foreach (var item in entity.Childs)
                {
                    childItems.AddRange(item.ChildItems);
                }

                switch (entity.EntityState)
                {
                    case EntityState.Added:

                        entity.BondEntitlementMasterID = await _service.GetMaxIdAsync(TableNames.BondEntitlementMaster, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        maxChildId = await _service.GetMaxIdAsync(TableNames.BondEntitlementChild, entity.Childs.Count(x => x.EntityState == EntityState.Added), RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        maxChildItemId = await _service.GetMaxIdAsync(TableNames.BondEntitlementChildItem, childItems.Count(x => x.EntityState == EntityState.Added), RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

                        foreach (var item in entity.Childs)
                        {
                            item.BondEntitlementChildID = maxChildId++;
                            item.BondEntitlementMasterID = entity.BondEntitlementMasterID;
                            item.EntityState = EntityState.Added;

                            foreach (var childItem in item.ChildItems)
                            {
                                childItem.BondEntitlementChildItemID = maxChildItemId++;
                                childItem.BondEntitlementChildID = item.BondEntitlementChildID;
                                childItem.EntityState = EntityState.Added;
                            }
                        }

                        break;

                    case EntityState.Modified:

                        maxChildId = await _service.GetMaxIdAsync(TableNames.BondEntitlementChild, entity.Childs.Count(x => x.EntityState == EntityState.Added), RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        maxChildItemId = await _service.GetMaxIdAsync(TableNames.BondEntitlementChildItem, childItems.Count(x => x.EntityState == EntityState.Added), RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

                        foreach (var item in entity.Childs)
                        {
                            if (item.EntityState == EntityState.Added)
                            {
                                item.BondEntitlementChildID = maxChildId++;
                                item.BondEntitlementMasterID = entity.BondEntitlementMasterID;
                                item.EntityState = EntityState.Added;
                            }

                            foreach (var childItem in item.ChildItems.Where(x => x.EntityState == EntityState.Added).ToList())
                            {
                                childItem.BondEntitlementChildItemID = maxChildItemId++;
                                childItem.BondEntitlementChildID = item.BondEntitlementChildID;
                                childItem.EntityState = EntityState.Added;
                            }
                        }


                        break;

                    case EntityState.Unchanged:
                    case EntityState.Deleted:
                        entity.EntityState = EntityState.Deleted;
                        entity.Childs.ForEach(c =>
                        {
                            c.EntityState = EntityState.Detached;
                            c.ChildItems.SetDeleted();
                        });
                        break;

                    default:
                        break;
                }

                childItems = new List<BondEntitlementChildItem>();
                foreach (var item in entity.Childs)
                {
                    childItems.AddRange(item.ChildItems);
                }

                await _service.SaveSingleAsync(entity, transaction);
                await _service.SaveAsync(childItems.Where(x => x.EntityState == EntityState.Deleted).ToList(), transaction);
                await _service.SaveAsync(entity.Childs, transaction);
                await _service.SaveAsync(childItems.Where(x => x.EntityState != EntityState.Deleted).ToList(), transaction);

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
