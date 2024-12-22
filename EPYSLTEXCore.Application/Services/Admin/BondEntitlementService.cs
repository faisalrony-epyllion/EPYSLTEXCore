using Dapper;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Application.Interfaces.Admin;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Admin;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Entity;

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
	                INNER JOIN {DbNames.EPYSL}..CompanyEntity C ON C.CompanyID = BEM.CompanyID 
	                LEFT JOIN {DbNames.EPYSL}..Currency CU ON CU.CurrencyID = BEM.CurrencyID
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

                SELECT IG.SubGroupID
                ,SubGroupName = TRIM(REPLACE(IG.SubGroupName,'LIVE',''))
                FROM {DbNames.EPYSL}..ItemSubGroup IG 
                WHERE IG.SubGroupName IN ('{SubGroupNames.YARNS}','{SubGroupNames.DYES}','{SubGroupNames.CHEMICALS}')
                ORDER BY IG.SubGroupName DESC;

                {CommonQueries.GetCompany()};

                --Dyes
                SELECT id = ISV1.SegmentValueID, text = ISV1.SegmentValue
                FROM {DbNames.EPYSL}..ItemMaster IM
                INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
                INNER JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID = IM.SubGroupID
                WHERE ISG.SubGroupName = '{SubGroupNames.DYES}' 
                GROUP BY ISV1.SegmentValue,ISV1.SegmentValueID
                ORDER BY ISV1.SegmentValue;

                --Chemicals
                SELECT id = ISV2.SegmentValueID, text = ISV2.SegmentValue
                FROM {DbNames.EPYSL}..ItemMaster IM
                INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                INNER JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID = IM.SubGroupID
                WHERE ISG.SubGroupName = '{SubGroupNames.CHEMICALS}' 
                GROUP BY ISV2.SegmentValue,ISV2.SegmentValueID
                ORDER BY ISV2.SegmentValue;

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
                data.Childs = records.Read<BondEntitlementChild>().ToList();
                data.CompanyList = await records.ReadAsync<Select2OptionModel>();
                data.Dyes = await records.ReadAsync<Select2OptionModel>();
                data.Chemicals = await records.ReadAsync<Select2OptionModel>();
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
        public async Task<BondEntitlementMaster> GetDetails(int id)
        {
            string sql = $@"
            Select M.* 
            From BondEntitlementMaster M
            Where M.BondEntitlementMasterID = {id};

            Select C.*, SubGroupName = TRIM(REPLACE(ISG.SubGroupName,'LIVE',''))
            From BondEntitlementMaster M
            INNER JOIN BondEntitlementChild C ON C.BondEntitlementMasterID = M.BondEntitlementMasterID
            INNER JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID = C.SubGroupID
            Where M.BondEntitlementMasterID = {id};

            Select CI.*, SegmentValue = ISV.SegmentValue
            From BondEntitlementMaster M
            INNER JOIN BondEntitlementChild C ON C.BondEntitlementMasterID = M.BondEntitlementMasterID
            INNER JOIN BondEntitlementChildItem CI ON CI.BondEntitlementChildID = C.BondEntitlementChildID
            INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = CI.SegmentValueID
            Where M.BondEntitlementMasterID = {id};

            SELECT id = IG.SubGroupID
            ,text = TRIM(REPLACE(IG.SubGroupName,'LIVE',''))
            FROM {DbNames.EPYSL}..ItemSubGroup IG 
            WHERE IG.SubGroupName IN ('{SubGroupNames.YARNS}','{SubGroupNames.DYES}','{SubGroupNames.CHEMICALS}')
            ORDER BY IG.SubGroupName DESC;

            {CommonQueries.GetCompany()};

            --Dyes
            SELECT id = ISV1.SegmentValueID, text = ISV1.SegmentValue
            FROM {DbNames.EPYSL}..ItemMaster IM
            INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
            INNER JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID = IM.SubGroupID
            WHERE ISG.SubGroupName = '{SubGroupNames.DYES}' 
            GROUP BY ISV1.SegmentValue,ISV1.SegmentValueID
            ORDER BY ISV1.SegmentValue;

            --Chemicals
            SELECT id = ISV2.SegmentValueID, text = ISV2.SegmentValue
            FROM {DbNames.EPYSL}..ItemMaster IM
            INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
            INNER JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID = IM.SubGroupID
            WHERE ISG.SubGroupName = '{SubGroupNames.CHEMICALS}' 
            GROUP BY ISV2.SegmentValue,ISV2.SegmentValueID
            ORDER BY ISV2.SegmentValue;

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
                data.Childs = records.Read<BondEntitlementChild>().ToList();
                var childItems = records.Read<BondEntitlementChildItem>().ToList();

                data.SubGroups = await records.ReadAsync<Select2OptionModel>();
                data.CompanyList = await records.ReadAsync<Select2OptionModel>();

                data.Dyes = await records.ReadAsync<Select2OptionModel>();
                data.Chemicals = await records.ReadAsync<Select2OptionModel>();

                data.CurrencyList = await records.ReadAsync<Select2OptionModel>();
                data.UnitList = await records.ReadAsync<Select2OptionModel>();

                int childId = 999;
                var childs = new List<BondEntitlementChild>();
                data.SubGroups.ToList().ForEach(sg =>
                {
                    var child = data.Childs.Find(c => c.SubGroupID == Convert.ToInt32(sg.id));
                    if (child.IsNull())
                    {
                        child = new BondEntitlementChild();
                        child.BondEntitlementChildID = childId++;
                        child.SubGroupID = Convert.ToInt32(sg.id);
                        child.SubGroupName = sg.text;
                    }
                    child.ChildItems = childItems.Where(ci => ci.BondEntitlementChildID == child.BondEntitlementChildID).ToList();
                    childs.Add(child);
                });
                data.Childs = childs;

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
                                childItem.BondEntitlementMasterID = item.BondEntitlementMasterID;
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
                                childItem.BondEntitlementMasterID = item.BondEntitlementMasterID;
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
