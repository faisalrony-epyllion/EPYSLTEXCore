
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Static;
using Microsoft.Data.SqlClient;
using EPYSLTEXCore.Application.Interfaces.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using Dapper;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Statics;
using System.Data.Entity;
using EPYSLTEXCore.Infrastructure.Exceptions;

namespace EPYSLTEXCore.Application.Services.Inventory.Yarn
{
    public class YDReceiveService: IYDReceiveService
    {
        private readonly IDapperCRUDService<YDReceiveMaster> _service;

        SqlTransaction transactionGmt = null;
        private SqlTransaction transaction = null;
        private readonly SqlConnection _connection;
        private readonly SqlConnection _connectionGmt;
        public YDReceiveService(IDapperCRUDService<YDReceiveMaster> service)
        {


            _service = service;

            _service.Connection = service.GetConnection(AppConstants.GMT_CONNECTION);
            _connectionGmt = service.Connection;

            _service.Connection = service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _connection = service.Connection;

        }

        public async Task<List<YDReceiveMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo)
        {
            string orderBy = "";
            string sql;
            if (status == Status.Pending)
            {
                sql = $@"
                With 
                M AS (
                    SELECT YDRM.YDReceiveMasterID,YDIM.YDReqIssueMasterID,YDIM.YDReqIssueNo,YDIM.YDReqIssueDate,ChallanNo=ISNULL(YDIM.ChallanNo,''),YDIM.ChallanDate,GPNo=ISNULL(YDIM.GPNo,''),YDIM.GPDate,YDIM.IsApprove
	                FROM {TableNames.YD_REQ_ISSUE_MASTER} YDIM
	                LEFT JOIN {TableNames.YD_RECEIVE_MASTER} YDRM ON YDIM.YDReqIssueMasterID=YDRM.YDReqIssueMasterID 
	                WHERE YDIM.IsApprove = 1 AND YDRM.YDReceiveMasterID IS NULL
                )
                SELECT *,COUNT(*) OVER() TotalRows FROM M ";

                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? " Order By M.YDReqIssueDate Desc" : paginationInfo.OrderBy;
            }
            else
            {
                sql = $@"
                With 
                M AS (
                    SELECT 
	                YDRM.YDReceiveMasterID,YDRM.YDReceiveNo,YDRM.YDReqIssueMasterID,YDRM.YDReceiveDate,YDRM.YDReceiveBy,YDRM.CompanyID,C.ShortName CompanyName,
	                YDIM.YDReqIssueNo,YDIM.YDReqIssueDate,ChallanNo=ISNULL(YDIM.ChallanNo,''),YDIM.ChallanDate,GPNo=ISNULL(YDIM.GPNo,''),YDIM.GPDate,YDIM.IsApprove
	                FROM {TableNames.YD_RECEIVE_MASTER} YDRM
	                LEFT JOIN {TableNames.YD_REQ_ISSUE_MASTER} YDIM ON YDIM.YDReqIssueMasterID=YDRM.YDReqIssueMasterID 
	                LEFT JOIN {DbNames.EPYSL}..CompanyEntity C ON YDRM.CompanyId=C.CompanyID
	                WHERE YDIM.IsApprove = 1
                )
                 SELECT *,COUNT(*) OVER() TotalRows FROM M";
                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? " Order By M.YDReceiveDate Desc" : paginationInfo.OrderBy;
            }

            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<YDReceiveMaster>(sql);
        }
        public async Task<YDReceiveMaster> GetNewAsync(int ydReqIssueMasterID)
        {
            var sql = $@"
                -------Master------
	                SELECT  
                    YDIM.YDReqIssueMasterID,YDIM.YDReqMasterID,YDIM.YDReqIssueNo,YDIM.YDReqIssueDate,ChallanNo=ISNULL(YDIM.ChallanNo,''),YDIM.ChallanDate,GPNo=ISNULL(YDIM.GPNo,''),YDIM.GPDate,YDIM.IsApprove,
                    YDIM.CompanyId,C.ShortName CompanyName
                    FROM {TableNames.YD_REQ_ISSUE_MASTER} YDIM
                    LEFT JOIN {TableNames.YD_RECEIVE_MASTER} YDRM ON YDIM.YDReqIssueMasterID=YDRM.YDReqIssueMasterID
                    LEFT JOIN {DbNames.EPYSL}..CompanyEntity C ON YDRM.CompanyId=C.CompanyID
                    WHERE YDIM.IsApprove=1 AND YDIM.YDReqIssueMasterID={ydReqIssueMasterID};

                -------Child------
	                --SELECT YDICRBM.YDRICRBId,YDIC.YDReqChildID,YDIC.YDReqIssueChildID,IM.ItemMasterID,UN.UnitID
                    --,UN.DisplayUnitDesc Unit,
                    --YDIC.YarnCategory,YDIC.ReqQty,YDIC.IssueQty,YDICRBM.IssueQtyCone ,IssueQtyCarton = YDICRBM.IssueCartoon,
                    --YDIC.ReqQty ReceiveQty,YDICRBM.IssueQtyCone ReceiveCone,YDICRBM.IssueCartoon ReceiveCarton,
					--L.LocationName,R.RackNo
                    --FROM {TableNames.YD_REQ_ISSUE_CHILD_CHILD_RACK_BIN_MAPPING} YDICRBM
					--INNER JOIN {TableNames.YD_REQ_ISSUE_CHILD} YDIC ON YDIC.YDReqIssueChildID=YDICRBM.YDReqIssueChildID
					--LEFT JOIN {TableNames.YARN_RECEIVE_CHILD_RACK_BIN} YDRCRB ON YDRCRB.ChildRackBinID=YDICRBM.ChildRackBinID
                    --LEFT JOIN {DbNames.EPYSL}..Location L ON L.LocationID = YDRCRB.LocationID
					--LEFT JOIN {DbNames.EPYSL}..Rack R ON R.RackID = YDRCRB.RackID
					--LEFT JOIN {DbNames.EPYSL}..Unit UN ON YDIC.UnitID = UN.UnitID
                    --LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YDIC.ItemMasterID
                    SELECT YDIC.YDReqChildID,YDIC.YDReqIssueChildID,IM.ItemMasterID,UN.UnitID
                    ,UN.DisplayUnitDesc Unit,
                    YDIC.YarnCategory,YDIC.ReqQty,YDIC.IssueQty,YDIC.IssueQtyCone ,YDIC.IssueQtyCarton,
                    YDIC.ReqQty ReceiveQty,YDIC.IssueQtyCone ReceiveCone,YDIC.IssueQtyCarton ReceiveCarton
                    FROM {TableNames.YD_REQ_ISSUE_CHILD} YDIC
					LEFT JOIN {DbNames.EPYSL}..Unit UN ON YDIC.UnitID = UN.UnitID
                    LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YDIC.ItemMasterID
                    WHERE YDIC.YDReqIssueMasterID={ydReqIssueMasterID};

                -------Company List------
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
                Where ISG.SubGroupName = 'Fabric' And Isnull(CAI.InHouse,0) = 1 Group by CE.CompanyID, CE.CompanyName, CE.ShortName;

                ";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YDReceiveMaster data = await records.ReadFirstOrDefaultAsync<YDReceiveMaster>();
                Guard.Against.NullObject(data);
                data.Childs = records.Read<YDReceiveChild>().ToList();
                data.CompanyList = records.Read<Select2OptionModel>().ToList();
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
        public async Task<YDReceiveMaster> GetAsync(int ydReceiveMasterID)
        {
            var sql = $@"
                -------Master------
	                SELECT  
                    YDRM.YDReceiveMasterID,YDRM.YDReceiveNo,YDRM.YDReqIssueMasterID,YDRM.YDReceiveDate,C.CompanyID,C.ShortName CompanyName,YDRM.Remarks,
                    YDRM.YDReqMasterID,YDIM.YDReqIssueNo,YDIM.YDReqIssueDate,ChallanNo=ISNULL(YDIM.ChallanNo,''),YDIM.ChallanDate,GPNo=ISNULL(YDIM.GPNo,''),YDIM.GPDate,YDIM.IsApprove
                    FROM {TableNames.YD_RECEIVE_MASTER} YDRM
                    LEFT JOIN {TableNames.YD_REQ_ISSUE_MASTER} YDIM ON YDIM.YDReqIssueMasterID=YDRM.YDReqIssueMasterID
                    LEFT JOIN {DbNames.EPYSL}..CompanyEntity C ON YDRM.CompanyId=C.CompanyID
                    WHERE YDIM.IsApprove=1 AND YDRM.YDReceiveMasterID={ydReceiveMasterID};

                -------Child------
	                --SELECT YDRC.YDReceiveChildID,YDRC.YDReceiveMasterID,YDRC.YDReqChildID,YDRC.YDReqIssueChildID,IM.ItemMasterID,UN.UnitID
                    --,UN.DisplayUnitDesc Unit,YDRC.YDRICRBId,
                    --YDIC.YarnCategory,YDIC.ReqQty,YDIC.IssueQty,YDICRBM.IssueQtyCone ,IssueQtyCarton = YDICRBM.IssueCartoon,
                    --YDRC.ReceiveQty,YDRC.ReceiveCone,YDRC.ReceiveCarton,YDRC.Remarks,
					--L.LocationName,R.RackNo
                    --FROM {TableNames.YD_RECEIVE_CHILD} YDRC
					--INNER JOIN {TableNames.YD_REQ_ISSUE_CHILD_CHILD_RACK_BIN_MAPPING} YDICRBM ON YDICRBM.YDRICRBId=YDRC.YDRICRBId
                    --LEFT JOIN {TableNames.YD_REQ_ISSUE_CHILD} YDIC ON YDRC.YDReqIssueChildID=YDIC.YDReqIssueChildID
					--LEFT JOIN {TableNames.YARN_RECEIVE_CHILD_RACK_BIN} YDRCRB ON YDRCRB.ChildRackBinID=YDICRBM.ChildRackBinID
                    --LEFT JOIN {DbNames.EPYSL}..Location L ON L.LocationID = YDRCRB.LocationID
					--LEFT JOIN {DbNames.EPYSL}..Rack R ON R.RackID = YDRCRB.RackID
                    --LEFT JOIN {DbNames.EPYSL}..Unit UN ON YDRC.UnitID = UN.UnitID
                    --LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YDRC.ItemMasterID
                    SELECT YDRC.YDReceiveChildID,YDRC.YDReceiveMasterID,YDRC.YDReqChildID,YDRC.YDReqIssueChildID,IM.ItemMasterID,UN.UnitID
                    ,UN.DisplayUnitDesc Unit,
                    YDIC.YarnCategory,YDIC.ReqQty,YDIC.IssueQty,YDIC.IssueQtyCone ,YDIC.IssueQtyCarton,
                    YDRC.ReceiveQty,YDRC.ReceiveCone,YDRC.ReceiveCarton,YDRC.Remarks
                    FROM {TableNames.YD_RECEIVE_CHILD} YDRC
                    LEFT JOIN {TableNames.YD_REQ_ISSUE_CHILD} YDIC ON YDRC.YDReqIssueChildID=YDIC.YDReqIssueChildID
                    LEFT JOIN {DbNames.EPYSL}..Unit UN ON YDRC.UnitID = UN.UnitID
                    LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YDRC.ItemMasterID
                    WHERE YDRC.YDReceiveMasterID={ydReceiveMasterID};

                -------Company List------
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
                Where ISG.SubGroupName = 'Fabric' And Isnull(CAI.InHouse,0) = 1 Group by CE.CompanyID, CE.CompanyName, CE.ShortName;

                ";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YDReceiveMaster data = await records.ReadFirstOrDefaultAsync<YDReceiveMaster>();
                Guard.Against.NullObject(data);
                data.Childs = records.Read<YDReceiveChild>().ToList();
                data.CompanyList = records.Read<Select2OptionModel>().ToList();
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
        public async Task<YDReceiveMaster> GetAllAsync(int ydReceiveMasterID)
        {
            var sql = $@"
                ----- Master ------
            SELECT * FROM {TableNames.YD_RECEIVE_MASTER} WHERE YDReceiveMasterID = {ydReceiveMasterID};
            
                ----- Child ------
            SELECT *
            FROM {TableNames.YD_RECEIVE_CHILD} WHERE YDReceiveMasterID = {ydReceiveMasterID};
            ";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YDReceiveMaster data = await records.ReadFirstOrDefaultAsync<YDReceiveMaster>();
                Guard.Against.NullObject(data);
                data.Childs = records.Read<YDReceiveChild>().ToList();

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
        public async Task SaveAsync(YDReceiveMaster entity, int userId)
        {
            SqlTransaction transaction = null;
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();
                await _connectionGmt.OpenAsync();
                transactionGmt = _connectionGmt.BeginTransaction();


                int maxChildId = 0;

                switch (entity.EntityState)
                {
                    case EntityState.Added:

                        entity.YDReceiveMasterID = await _service.GetMaxIdAsync(TableNames.YD_RECEIVE_MASTER,RepeatAfterEnum.NoRepeat, transactionGmt,_connectionGmt);
                        entity.YDReceiveNo = await _service.GetMaxNoAsync(TableNames.YD_RECEIVE_NO, 1, RepeatAfterEnum.EveryYear,"000000",transactionGmt,_connectionGmt);

                        maxChildId = await _service.GetMaxIdAsync(TableNames.YD_RECEIVE_CHILD, entity.Childs.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

                        foreach (var item in entity.Childs)
                        {
                            item.YDReceiveChildID = maxChildId++;
                            item.YDReceiveMasterID = entity.YDReceiveMasterID;
                            item.EntityState = EntityState.Added;
                        }

                        break;

                    case EntityState.Modified:
                        var addedChilds = entity.Childs.FindAll(x => x.EntityState == EntityState.Added);
                        maxChildId = await _service.GetMaxIdAsync(TableNames.YD_RECEIVE_CHILD, addedChilds.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

                        entity.Childs.ForEach(c =>
                        {
                            if (c.EntityState == EntityState.Added)
                            {
                                c.YDReceiveChildID = maxChildId++;
                                c.YDReceiveMasterID = entity.YDReceiveMasterID;
                                c.EntityState = EntityState.Added;
                            }
                        });

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
                if (transaction != null) transaction.Rollback();
                if (transactionGmt != null) transactionGmt.Rollback();
                if (ex.Message.Contains('~')) throw new Exception(ex.Message.Split('~')[0]);
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
