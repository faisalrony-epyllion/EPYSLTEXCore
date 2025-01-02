using Dapper;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Application.Interfaces.CDA;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.CDA;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.Data.SqlClient;
using System.Data.Entity;

namespace EPYSLTEXCore.Application.Services.CDA
{
    public class CDAPRService: ICDAPRService
    {
        private readonly IDapperCRUDService<CDAPRMaster> _service;
        private readonly SqlConnection _connection;
        private readonly SqlConnection _gmtConnection;
        private SqlTransaction transaction;
        private SqlTransaction transactionGmt;

        public CDAPRService(IDapperCRUDService<CDAPRMaster> service)
        {
            _service = service;
            _service.Connection = _service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _connection = service.Connection;
            _gmtConnection = service.GetConnection(AppConstants.GMT_CONNECTION);
        }
        public async Task<CDAPRMaster> GetDyesChemicalsAsync()
        {
            var query = $@"{CommonQueries.GetDyesChemicals()}";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                CDAPRMaster data = new CDAPRMaster
                {
                    SubGroupList = await records.ReadAsync<Select2OptionModel>()
                };
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
        public async Task<List<CDAPRMaster>> GetPagedAsync(Status status, string pageName, PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By CDAPRMasterID Desc" : paginationInfo.OrderBy;
            var sql = string.Empty;
            if (pageName == "CDAPR" ||
                pageName == "CDAPRApproval" ||
                pageName == "CDAPurchaseRequisitionAcknowledgement" ||
                pageName == "CDAPRChecking")
            {
                if (status == Status.Pending)
                {
                    sql = $@"WITH M AS 
                        (
	                        SELECT CDAM.CDAPRMasterID, CDAM.SubGroupID, CDAM.CDAPRNo, CDAM.CDAPRDate, CDAM.TriggerPointID, EV.ValueName As TriggerPoint, 
                            CDAM.CDAPRRequiredDate, CDAM.CDAPRBy, L.Name As CDAPRByUser, CDAM.Remarks
	                        from CDAPRMaster CDAM
	                        Inner Join {DbNames.EPYSL}..EntityTypeValue EV On EV.ValueID = CDAM.TriggerPointID
	                        LEFT Join {DbNames.EPYSL}..LoginUser L On CDAM.CDAPRBy = L.UserCode 
	                        WHERE CDAM.IsSendForCheck = 0 And CDAM.IsCheck = 0 And CDAM.IsCheckReject = 0
                            AND CDAM.SendForApproval = 0 And CDAM.Approve = 0 And CDAM.Reject = 0
                        ),
                        R AS
                        (
	                        SELECT CDAM.CDAPRMasterID, CDAM.SubGroupID, CDAM.CDAPRNo, CDAM.CDAPRDate, CDAM.TriggerPointID, EV.ValueName As TriggerPoint, 
                            CDAM.CDAPRRequiredDate, CDAM.CDAPRBy, L.Name As CDAPRByUser, CDAM.Remarks
	                        from CDAPRMaster CDAM
	                        Inner Join {DbNames.EPYSL}..EntityTypeValue EV On EV.ValueID = CDAM.TriggerPointID
	                        LEFT Join {DbNames.EPYSL}..LoginUser L On CDAM.CDAPRBy = L.UserCode 
	                        WHERE CDAM.IsCheckReject=1 AND CDAM.IsCheck=0
                        ),
                        A AS 
                        (
	                        SELECT *, Count(*) Over() TotalRows FROM M
	                        UNION
	                        SELECT *, Count(*) Over() TotalRows FROM R
                        )
                        SELECT * FROM A";
                }
                else if (status == Status.Proposed)
                {
                    sql =
                   $@"WITH M AS 
                    (
	                    SELECT IndentNo = STUFF((SELECT DISTINCT ', ' + CDAIM.IndentNo 
                        FROM CDAPRChild CDAC
                        INNER JOIN CDAIndentChild CDAIC ON CDAC.CDAIndentChildID = CDAIC.CDAIndentChildID
                        INNER JOIN CDAIndentMaster CDAIM ON CDAIM.CDAIndentMasterID = CDAIC.CDAIndentMasterID
                        WHERE CDAC.CDAPRMasterID = CDAM.CDAPRMasterID
                        FOR XML PATH(''),TYPE).value('(./text())[1]','VARCHAR(MAX)'),1,2,''),
						CDAM.CDAPRMasterID, CDAM.SubGroupID, CDAM.CDAPRNo, CDAM.CDAPRDate, CDAM.TriggerPointID, EV.ValueName As TriggerPoint, 
                        CDAM.CDAPRRequiredDate, CDAM.CDAPRBy, L.Name As CDAPRByUser, CDAM.Remarks, Count(*) Over() TotalRows
	                    from CDAPRMaster CDAM
	                    Inner Join {DbNames.EPYSL}..EntityTypeValue EV On EV.ValueID = CDAM.TriggerPointID
	                    LEFT Join {DbNames.EPYSL}..LoginUser L On CDAM.CDAPRBy = L.UserCode 
						--LEFT Join CDAPRChild CDAC ON CDAC.CDAPRMasterID=CDAM.CDAPRMasterID
						--LEFT Join CDAIndentChild CDAIC ON CDAIC.CDAIndentChildID=CDAC.CDAIndentChildID
						--LEFT Join CDAIndentMaster CDAIM ON CDAIM.CDAIndentMasterID=CDAIC.CDAIndentMasterID
	                    WHERE CDAM.IsSendForCheck = 1 AND CDAM.IsCheck = 0 AND CDAM.IsCheckReject = 0
	                    AND CDAM.SendForApproval = 0 AND CDAM.Approve=0 AND CDAM.Reject=0
						Group By CDAM.CDAPRMasterID, CDAM.SubGroupID, CDAM.CDAPRNo, CDAM.CDAPRDate, CDAM.TriggerPointID, EV.ValueName, 
                        CDAM.CDAPRRequiredDate, CDAM.CDAPRBy, L.Name, CDAM.Remarks
                    )
                    SELECT * FROM M";

                    //sql =
                    //$@"WITH M AS 
                    //(
                    // SELECT CDAM.CDAPRMasterID, CDAM.SubGroupID, CDAM.CDAPRNo, CDAM.CDAPRDate, CDAM.TriggerPointID, EV.ValueName As TriggerPoint, 
                    //    CDAM.CDAPRRequiredDate, CDAM.CDAPRBy, L.Name As CDAPRByUser, CDAM.Remarks, Count(*) Over() TotalRows
                    // from CDAPRMaster CDAM
                    // Inner Join {DbNames.EPYSL}..EntityTypeValue EV On EV.ValueID = CDAM.TriggerPointID
                    // LEFT Join {DbNames.EPYSL}..LoginUser L On CDAM.CDAPRBy = L.UserCode 
                    // WHERE CDAM.SendForApproval = 1 And CDAM.Approve = 0 And CDAM.Reject = 0
                    //)
                    //SELECT * FROM M";
                }
                else if (status == Status.Check)
                {
                    sql =
                    $@"WITH M AS 
                    (
	                    SELECT IndentNo = STUFF((SELECT DISTINCT ', ' + CDAIM.IndentNo 
                        FROM CDAPRChild CDAC
                        INNER JOIN CDAIndentChild CDAIC ON CDAC.CDAIndentChildID = CDAIC.CDAIndentChildID
                        INNER JOIN CDAIndentMaster CDAIM ON CDAIM.CDAIndentMasterID = CDAIC.CDAIndentMasterID
                        WHERE CDAC.CDAPRMasterID = CDAM.CDAPRMasterID
                        FOR XML PATH(''),TYPE).value('(./text())[1]','VARCHAR(MAX)'),1,2,''),
						CDAM.CDAPRMasterID, CDAM.SubGroupID, CDAM.CDAPRNo, CDAM.CDAPRDate, CDAM.TriggerPointID, EV.ValueName As TriggerPoint, 
                        CDAM.CDAPRRequiredDate, CDAM.CDAPRBy, L.Name As CDAPRByUser, CDAM.Remarks, Count(*) Over() TotalRows
	                    from CDAPRMaster CDAM
	                    Inner Join {DbNames.EPYSL}..EntityTypeValue EV On EV.ValueID = CDAM.TriggerPointID
	                    LEFT Join {DbNames.EPYSL}..LoginUser L On CDAM.CDAPRBy = L.UserCode
						--LEFT Join CDAPRChild CDAC ON CDAC.CDAPRMasterID=CDAM.CDAPRMasterID
						--LEFT Join CDAIndentChild CDAIC ON CDAIC.CDAIndentChildID=CDAC.CDAIndentChildID
						--LEFT Join CDAIndentMaster CDAIM ON CDAIM.CDAIndentMasterID=CDAIC.CDAIndentMasterID
                        WHERE CDAM.IsSendForCheck = 1 And CDAM.IsCheck = 1 And CDAM.IsCheckReject = 0
						Group BY CDAM.CDAPRMasterID, CDAM.SubGroupID, CDAM.CDAPRNo, CDAM.CDAPRDate, CDAM.TriggerPointID, EV.ValueName, 
                        CDAM.CDAPRRequiredDate, CDAM.CDAPRBy, L.Name, CDAM.Remarks
                    )
                    SELECT * FROM M";
                }
                else if (status == Status.CheckReject)
                {
                    sql =
                    $@"WITH M AS 
                    (
	                    SELECT IndentNo = STUFF((SELECT DISTINCT ', ' + CDAIM.IndentNo 
                        FROM CDAPRChild CDAC
                        INNER JOIN CDAIndentChild CDAIC ON CDAC.CDAIndentChildID = CDAIC.CDAIndentChildID
                        INNER JOIN CDAIndentMaster CDAIM ON CDAIM.CDAIndentMasterID = CDAIC.CDAIndentMasterID
                        WHERE CDAC.CDAPRMasterID = CDAM.CDAPRMasterID
                        FOR XML PATH(''),TYPE).value('(./text())[1]','VARCHAR(MAX)'),1,2,''),
						CDAM.CDAPRMasterID, CDAM.SubGroupID, CDAM.CDAPRNo, CDAM.CDAPRDate, CDAM.TriggerPointID, EV.ValueName As TriggerPoint, 
                        CDAM.CDAPRRequiredDate, CDAM.CDAPRBy, L.Name As CDAPRByUser, CDAM.Remarks, Count(*) Over() TotalRows
	                    from CDAPRMaster CDAM
	                    Inner Join {DbNames.EPYSL}..EntityTypeValue EV On EV.ValueID = CDAM.TriggerPointID
	                    LEFT Join {DbNames.EPYSL}..LoginUser L On CDAM.CDAPRBy = L.UserCode 
						--LEFT Join CDAPRChild CDAC ON CDAC.CDAPRMasterID=CDAM.CDAPRMasterID
						--LEFT Join CDAIndentChild CDAIC ON CDAIC.CDAIndentChildID=CDAC.CDAIndentChildID
						--LEFT Join CDAIndentMaster CDAIM ON CDAIM.CDAIndentMasterID=CDAIC.CDAIndentMasterID
                        WHERE CDAM.IsCheckReject = 1
						Group By CDAM.CDAPRMasterID, CDAM.SubGroupID, CDAM.CDAPRNo, CDAM.CDAPRDate, CDAM.TriggerPointID, EV.ValueName, 
                        CDAM.CDAPRRequiredDate, CDAM.CDAPRBy, L.Name, CDAM.Remarks
                    )
                    SELECT * FROM M";
                }
                else if (status == Status.ProposedForApproval)
                {
                    sql =
                   $@"WITH M AS 
                    (
	                    SELECT IndentNo = STUFF((SELECT DISTINCT ', ' + CDAIM.IndentNo 
                        FROM CDAPRChild CDAC
                        INNER JOIN CDAIndentChild CDAIC ON CDAC.CDAIndentChildID = CDAIC.CDAIndentChildID
                        INNER JOIN CDAIndentMaster CDAIM ON CDAIM.CDAIndentMasterID = CDAIC.CDAIndentMasterID
                        WHERE CDAC.CDAPRMasterID = CDAM.CDAPRMasterID
                        FOR XML PATH(''),TYPE).value('(./text())[1]','VARCHAR(MAX)'),1,2,''),
						CDAM.CDAPRMasterID, CDAM.SubGroupID, CDAM.CDAPRNo, CDAM.CDAPRDate, CDAM.TriggerPointID, EV.ValueName As TriggerPoint, 
                        CDAM.CDAPRRequiredDate, CDAM.CDAPRBy, L.Name As CDAPRByUser, CDAM.Remarks, Count(*) Over() TotalRows
	                    from CDAPRMaster CDAM
	                    Inner Join {DbNames.EPYSL}..EntityTypeValue EV On EV.ValueID = CDAM.TriggerPointID
	                    LEFT Join {DbNames.EPYSL}..LoginUser L On CDAM.CDAPRBy = L.UserCode 
						--LEFT Join CDAPRChild CDAC ON CDAC.CDAPRMasterID=CDAM.CDAPRMasterID
						--LEFT Join CDAIndentChild CDAIC ON CDAIC.CDAIndentChildID=CDAC.CDAIndentChildID
						--LEFT Join CDAIndentMaster CDAIM ON CDAIM.CDAIndentMasterID=CDAIC.CDAIndentMasterID
	                    WHERE CDAM.IsCheck = 1 AND CDAM.IsCheckReject = 0
	                    AND CDAM.SendForApproval = 1 AND CDAM.Approve=0 AND CDAM.Reject=0
						Group BY CDAM.CDAPRMasterID, CDAM.SubGroupID, CDAM.CDAPRNo, CDAM.CDAPRDate, CDAM.TriggerPointID, EV.ValueName, 
                        CDAM.CDAPRRequiredDate, CDAM.CDAPRBy, L.Name, CDAM.Remarks
                    )
                    SELECT * FROM M";
                }
                else if (status == Status.Approved)
                {
                    sql =
                    $@"WITH M AS 
                    (
	                    SELECT IndentNo = STUFF((SELECT DISTINCT ', ' + CDAIM.IndentNo 
                        FROM CDAPRChild CDAC
                        INNER JOIN CDAIndentChild CDAIC ON CDAC.CDAIndentChildID = CDAIC.CDAIndentChildID
                        INNER JOIN CDAIndentMaster CDAIM ON CDAIM.CDAIndentMasterID = CDAIC.CDAIndentMasterID
                        WHERE CDAC.CDAPRMasterID = CDAM.CDAPRMasterID
                        FOR XML PATH(''),TYPE).value('(./text())[1]','VARCHAR(MAX)'),1,2,''),
						CDAM.CDAPRMasterID, CDAM.SubGroupID, CDAM.CDAPRNo, CDAM.CDAPRDate, CDAM.TriggerPointID, EV.ValueName As TriggerPoint, 
                        CDAM.CDAPRRequiredDate, CDAM.CDAPRBy, L.Name As CDAPRByUser, CDAM.Remarks, Count(*) Over() TotalRows
	                    from CDAPRMaster CDAM
	                    Inner Join {DbNames.EPYSL}..EntityTypeValue EV On EV.ValueID = CDAM.TriggerPointID
	                    LEFT Join {DbNames.EPYSL}..LoginUser L On CDAM.CDAPRBy = L.UserCode 
						--LEFT Join CDAPRChild CDAC ON CDAC.CDAPRMasterID=CDAM.CDAPRMasterID
						--LEFT Join CDAIndentChild CDAIC ON CDAIC.CDAIndentChildID=CDAC.CDAIndentChildID
						--LEFT Join CDAIndentMaster CDAIM ON CDAIM.CDAIndentMasterID=CDAIC.CDAIndentMasterID
                        WHERE CDAM.SendForApproval = 1 And CDAM.Approve = 1 And CDAM.Reject = 0
						Group BY CDAM.CDAPRMasterID, CDAM.SubGroupID, CDAM.CDAPRNo, CDAM.CDAPRDate, CDAM.TriggerPointID, EV.ValueName, 
                        CDAM.CDAPRRequiredDate, CDAM.CDAPRBy, L.Name, CDAM.Remarks
                    )
                    SELECT * FROM M";
                }
                else if (status == Status.Reject)
                {
                    sql =
                    $@"WITH M AS 
                    (
	                    SELECT CDAM.CDAPRMasterID, CDAM.SubGroupID, CDAM.CDAPRNo, CDAM.CDAPRDate, CDAM.TriggerPointID, EV.ValueName As TriggerPoint, 
                        CDAM.CDAPRRequiredDate, CDAM.CDAPRBy, L.Name As CDAPRByUser, CDAM.Remarks, Count(*) Over() TotalRows
	                    from CDAPRMaster CDAM
	                    Inner Join {DbNames.EPYSL}..EntityTypeValue EV On EV.ValueID = CDAM.TriggerPointID
	                    LEFT Join {DbNames.EPYSL}..LoginUser L On CDAM.CDAPRBy = L.UserCode 
                        WHERE CDAM.SendForApproval = 1 And CDAM.Approve = 0 And CDAM.Reject = 1
                    )
                    SELECT * FROM M";
                }
                else if (status == Status.PartiallyCompleted)
                {
                    sql =
                    $@"WITH M AS 
                    (
	                    SELECT IndentNo = STUFF((SELECT DISTINCT ', ' + CDAIM.IndentNo 
                        FROM CDAPRChild CDAC
                        INNER JOIN CDAIndentChild CDAIC ON CDAC.CDAIndentChildID = CDAIC.CDAIndentChildID
                        INNER JOIN CDAIndentMaster CDAIM ON CDAIM.CDAIndentMasterID = CDAIC.CDAIndentMasterID
                        WHERE CDAC.CDAPRMasterID = CDAM.CDAPRMasterID
                        FOR XML PATH(''),TYPE).value('(./text())[1]','VARCHAR(MAX)'),1,2,''),
						CDAM.CDAPRMasterID, CDAM.SubGroupID, CDAM.CDAPRNo, CDAM.CDAPRDate, CDAM.TriggerPointID, EV.ValueName As TriggerPoint, 
                        CDAM.CDAPRRequiredDate, CDAM.CDAPRBy, L.Name As CDAPRByUser, CDAM.Remarks, Count(*) Over() TotalRows
	                    from CDAPRMaster CDAM
	                    Inner Join {DbNames.EPYSL}..EntityTypeValue EV On EV.ValueID = CDAM.TriggerPointID
	                    LEFT Join {DbNames.EPYSL}..LoginUser L On CDAM.CDAPRBy = L.UserCode 
						--LEFT Join CDAPRChild CDAC ON CDAC.CDAPRMasterID=CDAM.CDAPRMasterID
						--LEFT Join CDAIndentChild CDAIC ON CDAIC.CDAIndentChildID=CDAC.CDAIndentChildID
						--LEFT Join CDAIndentMaster CDAIM ON CDAIM.CDAIndentMasterID=CDAIC.CDAIndentMasterID
                        WHERE CDAM.SendForApproval = 1 And CDAM.Approve = 1 And CDAM.Acknowledge = 0 And CDAM.Reject = 0
						Group By CDAM.CDAPRMasterID, CDAM.SubGroupID, CDAM.CDAPRNo, CDAM.CDAPRDate, CDAM.TriggerPointID, EV.ValueName, 
                        CDAM.CDAPRRequiredDate, CDAM.CDAPRBy, L.Name, CDAM.Remarks
                    )
                    SELECT * FROM M";
                }
                else if (status == Status.Acknowledge)
                {
                    sql =
                    $@"WITH M AS 
                    (
	                    SELECT IndentNo = STUFF((SELECT DISTINCT ', ' + CDAIM.IndentNo 
                        FROM CDAPRChild CDAC
                        INNER JOIN CDAIndentChild CDAIC ON CDAC.CDAIndentChildID = CDAIC.CDAIndentChildID
                        INNER JOIN CDAIndentMaster CDAIM ON CDAIM.CDAIndentMasterID = CDAIC.CDAIndentMasterID
                        WHERE CDAC.CDAPRMasterID = CDAM.CDAPRMasterID
                        FOR XML PATH(''),TYPE).value('(./text())[1]','VARCHAR(MAX)'),1,2,''),
						CDAM.CDAPRMasterID, CDAM.SubGroupID, CDAM.CDAPRNo, CDAM.CDAPRDate, CDAM.TriggerPointID, EV.ValueName As TriggerPoint, 
                        CDAM.CDAPRRequiredDate, CDAM.CDAPRBy, L.Name As CDAPRByUser, CDAM.Remarks, Count(*) Over() TotalRows
	                    from CDAPRMaster CDAM
	                    Inner Join {DbNames.EPYSL}..EntityTypeValue EV On EV.ValueID = CDAM.TriggerPointID
	                    LEFT Join {DbNames.EPYSL}..LoginUser L On CDAM.CDAPRBy = L.UserCode 
                        WHERE CDAM.SendForApproval = 1 And CDAM.Approve = 1 And CDAM.Reject = 0 And CDAM.Acknowledge = 1
						Group BY CDAM.CDAPRMasterID, CDAM.SubGroupID, CDAM.CDAPRNo, CDAM.CDAPRDate, CDAM.TriggerPointID, EV.ValueName, 
                        CDAM.CDAPRRequiredDate, CDAM.CDAPRBy, L.Name, CDAM.Remarks
                    )
                    SELECT * FROM M";
                }
            }
            else if (pageName == "CDACommercialPR")
            {
                if (status == Status.Pending)
                {
                    sql =
                    $@"WITH M AS 
                    (
	                    SELECT CDAM.CDAPRMasterID, CDAM.SubGroupID, CDAM.CDAPRNo, CDAM.CDAPRDate, CDAM.TriggerPointID, EV.ValueName As TriggerPoint, 
                        CDAM.CDAPRRequiredDate, CDAM.CDAPRBy, L.Name As CDAPRByUser, CDAM.Remarks, Count(*) Over() TotalRows
	                    from CDAPRMaster CDAM
	                    Inner Join {DbNames.EPYSL}..EntityTypeValue EV On EV.ValueID = CDAM.TriggerPointID
	                    LEFT Join {DbNames.EPYSL}..LoginUser L On CDAM.CDAPRBy = L.UserCode 
                        WHERE CDAM.SendForApproval = 1 And CDAM.Approve = 1 And CDAM.Acknowledge = 1 And CDAM.Reject = 0 AND CDAM.IsCPR = 0
                    )
                    SELECT * FROM M";
                }
                else if (status == Status.Completed)
                {
                    sql =
                    $@"WITH M AS 
                    (
	                    SELECT CDAM.CDAPRMasterID, CDAM.SubGroupID, CDAM.CDAPRNo, CDAM.CDAPRDate, CDAM.TriggerPointID, EV.ValueName As TriggerPoint, 
                        CDAM.CDAPRRequiredDate, CDAM.CDAPRBy, L.Name As CDAPRByUser, CDAM.Remarks, Count(*) Over() TotalRows
	                    from CDAPRMaster CDAM
	                    Inner Join {DbNames.EPYSL}..EntityTypeValue EV On EV.ValueID = CDAM.TriggerPointID
	                    LEFT Join {DbNames.EPYSL}..LoginUser L On CDAM.CDAPRBy = L.UserCode 
                        WHERE CDAM.SendForApproval = 1 And CDAM.Approve = 1 And CDAM.Acknowledge = 1 And CDAM.Reject = 0 And CDAM.IsCPR = 1 And CDAM.IsFPR = 0
                    )
                    SELECT * FROM M";
                }
            }
            else if (pageName == "CDAFinancePR")
            {
                if (status == Status.Pending)
                {
                    sql =
                        $@"WITH M AS 
                        (
	                        SELECT CDAM.CDAPRMasterID, CDAM.SubGroupID, CDAM.CDAPRNo, CDAM.CDAPRDate, CDAM.TriggerPointID, EV.ValueName As TriggerPoint, 
                            CDAM.CDAPRRequiredDate, CDAM.CDAPRBy, L.Name As CDAPRByUser, CDAM.Remarks, Count(*) Over() TotalRows
	                        from CDAPRMaster CDAM
	                        Inner Join {DbNames.EPYSL}..EntityTypeValue EV On EV.ValueID = CDAM.TriggerPointID
	                        LEFT Join {DbNames.EPYSL}..LoginUser L On CDAM.CDAPRBy = L.UserCode 
                            WHERE CDAM.SendForApproval = 1 And CDAM.Approve = 1 And CDAM.Acknowledge = 1 And CDAM.IsCPR = 1 And CDAM.IsFPR = 0
                        )
                        SELECT * FROM M";
                }
                if (status == Status.Completed)
                {
                    sql =
                        $@"WITH M AS 
                        (
	                        SELECT CDAM.CDAPRMasterID, CDAM.SubGroupID, CDAM.CDAPRNo, CDAM.CDAPRDate, CDAM.TriggerPointID, EV.ValueName As TriggerPoint, 
                            CDAM.CDAPRRequiredDate, CDAM.CDAPRBy, L.Name As CDAPRByUser, CDAM.Remarks, Count(*) Over() TotalRows
	                        from CDAPRMaster CDAM
	                        Inner Join {DbNames.EPYSL}..EntityTypeValue EV On EV.ValueID = CDAM.TriggerPointID
	                        LEFT Join {DbNames.EPYSL}..LoginUser L On CDAM.CDAPRBy = L.UserCode 
                            WHERE CDAM.SendForApproval = 1 And CDAM.Approve = 1 And CDAM.IsCPR = 1 And CDAM.IsFPR = 1
                        )
                        SELECT * FROM M";
                }
            }
            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";
            return await _service.GetDataAsync<CDAPRMaster>(sql);
        }
        public async Task<List<CDAIndentChild>> GetPendingIndentPagedAsync(Status status, string pageName, PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By CIC.CDAPRChildID Desc" : paginationInfo.OrderBy;
            var sql = string.Empty;
            sql = $@"
            ;WITH CIC As 
            (
	            Select CIC.CDAIndentChildID AS CDAPRChildID, CIM.SubGroupID, CIM.IndentNo, CIM.IndentDate, 0 As TriggerPoint, 
	            CIM.CIndentBy, CIM.AcknowledgeDate, CIM.TexAcknowledgeDate, CIC.CompanyID, CIC.ItemMasterID, CIC.UnitID, 
                CIC.Remarks, CIC.ReqQty, CIC.HSCode  
	            From CDAIndentChild CIC
	            Inner Join CDAIndentMaster CIM On CIM.CDAIndentMasterID = CIC.CDAIndentMasterID 
	            Left Join CDAPRChild CPC On CPC.CDAIndentChildID = CIC.CDAIndentChildID
	            Where CIM.TexAcknowledge = 1 And CIC.CDAIndentChildID != Isnull(CPC.CDAIndentChildID,0)
            )
            Select CIC.CDAPRChildID, CIC.SubGroupID, ISG.SubGroupName, CIC.IndentNo, CIC.IndentDate, CIC.TriggerPoint, 
            CIC.CIndentBy, L.Name As CDAIndentByUser, CIC.Remarks, CIC.AcknowledgeDate, CIC.TexAcknowledgeDate, 
            CIC.CompanyID, CE.ShortName As CompanyName, CIC.ItemMasterID, CIC.UnitID,CIC.Remarks, CIC.ReqQty, CIC.HSCode,  
            U.DisplayUnitDesc, ISV1.SegmentValue Segment1ValueDesc,  
            (Case When CIC.SubGroupID = 100 Then Null Else ISV2.SegmentValue End)Segment2ValueDesc, 
            (Case When CIC.SubGroupID = 100 Then ISV2.SegmentValue Else ISV3.SegmentValue End)Segment3ValueDesc, 
            (Case When CIC.SubGroupID = 100 Then Null Else ISV4.SegmentValue End)Segment4ValueDesc, 
            Count(*) Over() TotalRows   
            From CIC 
            INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = CIC.ItemMasterID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
            LEFT JOIN {DbNames.EPYSL}..CompanyEntity CE On CE.CompanyID = CIC.CompanyID
            LEFT JOIN {DbNames.EPYSL}..ITEMSUBGROUP ISG On ISG.SubGroupID = CIC.SubGroupID 
            LEFT Join {DbNames.EPYSL}..LoginUser L On CIC.CIndentBy = L.UserCode
            LEFT JOIN {DbNames.EPYSL}..Unit U ON U.UnitID = CIC.UnitID ";

            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";
            return await _service.GetDataAsync<CDAIndentChild>(sql);
        }
        public async Task<CDAPRMaster> GetNewAsync(string SubGroupName)
        {
            var query =
                $@"
                -- Requisition By
                {CommonQueries.GetYarnAndCDAUsers()};

                -- RM Trigger Points
                {CommonQueries.GetEntityTypesByEntityTypeName(EntityTypeNameConstants.RM_TRIGGER_POINTS)}

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
                Where ISG.SubGroupName = 'Fabric' And Isnull(CAI.InHouse,0) = 1 Group by CE.CompanyID, CE.CompanyName, CE.ShortName;

                --Items List for PopUp
                Select IM.ItemMasterID, IM.Segment1ValueID Segment1ValueId, ISV.SegmentValue As Segment1ValueDesc, 
                IM.Segment2ValueID Segment2ValueId, ISV2.SegmentValue AS Segment2ValueDesc,
                IM.Segment3ValueID Segment3ValueId, ISV3.SegmentValue AS Segment3ValueDesc,
                IM.Segment4ValueID Segment4ValueId, ISV4.SegmentValue AS Segment4ValueDesc
                From {DbNames.EPYSL}..ItemMaster IM 
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV On ISV.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 On ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 On ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 On ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentName ISN On ISN.SegmentNameID = ISV.SegmentNameID
                WHERE ISN.SegmentName In ('{SubGroupName}'); ";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                CDAPRMaster data = new CDAPRMaster
                {
                    CDAPRByList = await records.ReadAsync<Select2OptionModel>(),
                    TriggerPointList = await records.ReadAsync<Select2OptionModel>(),
                    CompanyList = await records.ReadAsync<Select2OptionModel>(),
                    ChildsItemSegments = records.Read<CDAPRChild>().ToList()
                };

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
        public async Task<CDAPRMaster> GetIndentNewAsync(string SubGroupName, string IDs)
        {
            var query =
                $@"
                -- Requisition By
                {CommonQueries.GetYarnAndCDAUsers()};

                -- RM Trigger Points
                {CommonQueries.GetEntityTypesByEntityTypeName(EntityTypeNameConstants.RM_TRIGGER_POINTS)}

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
                Where ISG.SubGroupName = 'Fabric' And Isnull(CAI.InHouse,0) = 1 Group by CE.CompanyID, CE.CompanyName, CE.ShortName;

                --Items List for PopUp
                Select IM.ItemMasterID, IM.Segment1ValueID Segment1ValueId, ISV.SegmentValue As Segment1ValueDesc, 
                IM.Segment2ValueID Segment2ValueId, ISV2.SegmentValue AS Segment2ValueDesc,
                IM.Segment3ValueID Segment3ValueId, ISV3.SegmentValue AS Segment3ValueDesc,
                IM.Segment4ValueID Segment4ValueId, ISV4.SegmentValue AS Segment4ValueDesc
                From {DbNames.EPYSL}..ItemMaster IM 
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV On ISV.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 On ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 On ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 On ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentName ISN On ISN.SegmentNameID = ISV.SegmentNameID
                WHERE ISN.SegmentName In ('{SubGroupName}'); 

                ;WITH YRC As 
                (
	                Select * From CDAIndentChild Where CDAIndentChildID IN({IDs})
                )
                Select YRC.CDAIndentChildID As CDAPRChildID, YRC.CDAIndentMasterID As CDAPRMasterID, YRC.CDAIndentChildID, YRC.ItemMasterID, 
                YRC.UnitID,YRC.Remarks, YRC.ReqQty, 0 As SuggestedQty, YRC.CompanyID, CE.ShortName CompanyName,  
                U.DisplayUnitDesc, IM.Segment1ValueId, IM.Segment2ValueId, IM.Segment3ValueId, IM.Segment4ValueId, 
                ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, 
                ISV4.SegmentValue Segment4ValueDesc
                From YRC
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YRC.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..Unit U ON U.UnitID = YRC.UnitID
                LEFT JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = YRC.CompanyID; ";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                CDAPRMaster data = new CDAPRMaster
                {
                    CDAPRByList = await records.ReadAsync<Select2OptionModel>(),
                    TriggerPointList = await records.ReadAsync<Select2OptionModel>(),
                    CompanyList = await records.ReadAsync<Select2OptionModel>(),
                    ChildsItemSegments = records.Read<CDAPRChild>().ToList(),
                    Childs = records.Read<CDAPRChild>().ToList()
                };
                data.Childs.ForEach(child =>
                {
                    child.ItemIDs += string.Join(",", data.Childs.Where(x => x.ItemMasterID == child.ItemMasterID).Select(y => y.ItemMasterID));
                });

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

        public async Task<CDAPRMaster> GetAsync(int id, string SubGroupName)
        {
            var query =
                $@"
                -- Master Data
                Select CDAM.CDAPRMasterID, CDAM.CDAPRDate, CDAM.CDAPRRequiredDate, CDAM.CDAPRNo, CDAM.Remarks, CDAM.CDAPRBy, CDAC.FPRCompanyID As CompanyID,
                CDAM.SendForApproval, CDAM.Approve, CDAM.Reject, CDAM.RejectReason, CDAM.TriggerPointID, CDAM.SubGroupID, SG.SubGroupName SubGroupName,
                CDAM.IsSendForCheck,CDAM.IsCheck,CDAM.IsCheckReject,CDAM.CheckRejectReason
                From CDAPRMaster CDAM INNER JOIN {DbNames.EPYSL}..ITEMSUBGROUP SG ON SG.SubGroupID = CDAM.SubGroupID
                Inner Join CDAPRChild CDAC On CDAC.CDAPRMasterID = CDAM.CDAPRMasterID
                Where CDAM.CDAPRMasterID = {id}
                Group By CDAM.CDAPRMasterID, CDAM.CDAPRDate, CDAM.CDAPRRequiredDate, CDAM.CDAPRNo, CDAM.Remarks, CDAM.CDAPRBy, CDAC.FPRCompanyID,
                CDAM.SendForApproval, CDAM.Approve, CDAM.Reject, CDAM.RejectReason, CDAM.TriggerPointID, CDAM.SubGroupID, SG.SubGroupName,
                CDAM.IsSendForCheck,CDAM.IsCheck,CDAM.IsCheckReject,CDAM.CheckRejectReason;

                --Child Data
                ;WITH YRC As (
	                 Select * From CDAPRChild Where CDAPRMasterID = {id}
                )

                Select YRC.CDAPRChildID, YRC.CDAPRMasterID, YRC.CDAIndentChildID, YRC.ItemMasterID, YRC.UnitID,YRC.Remarks, YRC.ReqQty, YRC.SuggestedQty, 
                YRC.UnitID, YRC.ItemMasterID ItemMasterID, YRC.FPRCompanyID, YRC.FPRCompanyID, CE.ShortName CompanyName, YRC.YarnCategory, 
                U.DisplayUnitDesc, IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, IM.Segment4ValueID, 
                ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, 
                ISV4.SegmentValue Segment4ValueDesc
                From YRC
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YRC.ItemMasterID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN  {DbNames.EPYSL}..Unit U ON U.UnitID = YRC.UnitID
                LEFT JOIN {DbNames.EPYSL}..CompanyEntity CE ON YRC.FPRCompanyID = CE.CompanyID;

                -- PR Company
                /*;Select C.CDAPRCompanyID, C.CDAPRChildID, C.CDAPRMasterID, C.CompanyID, C.IsCPR, CE.ShortName CompanyName
                From CDAPRCompany C
                Inner Join {DbNames.EPYSL}..CompanyEntity CE On C.CompanyID = CE.CompanyID
                Where CDAPRMasterID = {id}; */

                -- Company
                Select Cast (CE.CompanyID as varchar) [id] , C.ShortName [text]
                From (select SubGroupID, ContactID from {DbNames.EPYSL}..SupplierItemGroupStatus Group By SubGroupID, ContactID) SIGS
                Inner Join {DbNames.EPYSL}..Contacts C On SIGS.ContactID = C.ContactID
                Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = SIGS.SubGroupID
                Inner Join {DbNames.EPYSL}..ContactAdditionalInfo CAI On CAI.ContactID = SIGS.ContactID
                Inner Join {DbNames.EPYSL}..CompanyEntity CE On C.MappingCompanyID = CE.CompanyID
                Where ISG.SubGroupName = 'Fabric' And Isnull(CAI.InHouse,0) = 1
                Group by CE.CompanyID, C.Name, C.ShortName;

                --Requisition By
                {CommonQueries.GetYarnAndCDAUsers()};

                -- RM Trigger Points
                {CommonQueries.GetEntityTypesByEntityTypeName(EntityTypeNameConstants.RM_TRIGGER_POINTS)};

                --Items List for PopUp
                Select IM.ItemMasterID, IM.Segment1ValueID Segment1ValueId, ISV.SegmentValue As Segment1ValueDesc, 
                IM.Segment2ValueID Segment2ValueId, ISV2.SegmentValue AS Segment2ValueDesc,
                IM.Segment3ValueID Segment3ValueId, ISV3.SegmentValue AS Segment3ValueDesc,
                IM.Segment4ValueID Segment4ValueId, ISV4.SegmentValue AS Segment4ValueDesc
                From {DbNames.EPYSL}..ItemMaster IM 
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV On ISV.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 On ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 On ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 On ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentName ISN On ISN.SegmentNameID = ISV.SegmentNameID
                WHERE ISN.SegmentName In ('{SubGroupName}'); ";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                CDAPRMaster data = await records.ReadFirstOrDefaultAsync<CDAPRMaster>();
                data.Childs = records.Read<CDAPRChild>().ToList();
                data.Childs.ForEach(child =>
                {
                    child.ItemIDs += string.Join(",", data.Childs.Where(x => x.ItemMasterID == child.ItemMasterID).Select(y => y.ItemMasterID));
                });
                //var prCompanyList = records.Read<CDAPRCompany>().ToList();
                //foreach (var item in data.Childs)
                //{
                //    item.CompanyIDs = prCompanyList.Where(x => !x.IsCPR).Select(x => x.CompanyID).Distinct().ToArray();
                //    item.CompanyNames = string.Join(",", prCompanyList.Where(x => !x.IsCPR).Select(x => x.CompanyName).Distinct());
                //    item.CPRCompanyIDs = prCompanyList.Where(x => x.IsCPR).Select(x => x.CompanyID).Distinct().ToArray();
                //    item.CPRCompanyNames = string.Join(",", prCompanyList.Where(x => x.IsCPR).Select(x => x.CompanyName).Distinct());
                //}

                data.CompanyList = await records.ReadAsync<Select2OptionModel>();
                data.CDAPRByList = await records.ReadAsync<Select2OptionModel>();
                data.TriggerPointList = await records.ReadAsync<Select2OptionModel>();
                data.ChildsItemSegments = records.Read<CDAPRChild>().ToList();
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
        public async Task<CDAPRMaster> GetAllAsync(int id)
        {
            var sql = $@"
            ;Select * From CDAPRMaster Where CDAPRMasterID = {id} 
            ;Select * From CDAPRChild Where CDAPRMasterID = {id} 
            ; Select* From CDAPRCompany Where CDAPRMasterID = {id} ";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                CDAPRMaster data = records.Read<CDAPRMaster>().FirstOrDefault();
                Guard.Against.NullObject(data);
                data.Childs = records.Read<CDAPRChild>().ToList();
                List<CDAPRCompany> CDAPRCompanieList = records.Read<CDAPRCompany>().ToList();
                data.Childs.ForEach(x =>
                {
                    x.CDAPRCompanies = CDAPRCompanieList.Where(y => y.CDAPRChildID == x.CDAPRChildID).ToList();
                });
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
        public async Task SaveAsync(CDAPRMaster entity)
        {
            SqlTransaction transaction = null;
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();
                await _gmtConnection.OpenAsync();
                transactionGmt = _gmtConnection.BeginTransaction();
                int maxChildId = 0, maxCompanyId = 0;
                switch (entity.EntityState)
                {
                    case EntityState.Added:
                        entity.CDAPRMasterID = await _service.GetMaxIdAsync(TableNames.CDAPRMASTER);
                        entity.CDAPRNo = await _service.GetMaxNoAsync(TableNames.CDAPR_NO, 1, RepeatAfterEnum.NoRepeat, "00000", transactionGmt, _gmtConnection);
                        maxChildId = await _service.GetMaxIdAsync(TableNames.CDAPRCHILD, entity.Childs.Count);
                        //maxCompanyId = await _signatureRepository.GetMaxIdAsync(TableNames.CDA_PR_COMPANY, entity.Childs.Sum(x => x.CompanyIDs.Count()));
                        maxCompanyId = await _service.GetMaxIdAsync(TableNames.CDA_PR_COMPANY, entity.Childs.Sum(x => x.CDAPRCompanies.Count()));

                       

                        foreach (CDAPRChild item in entity.Childs)
                        {
                            item.CDAPRChildID = maxChildId++;
                            item.CDAPRMasterID = entity.CDAPRMasterID;
                            foreach (CDAPRCompany company in item.CDAPRCompanies)
                            {
                                company.CDAPRCompanyID = maxCompanyId++;
                                company.CDAPRChildID = item.CDAPRChildID;
                                company.CDAPRMasterID = entity.CDAPRMasterID;
                            }
                        }

                        break;

                    case EntityState.Modified:
                        maxChildId = await _service.GetMaxIdAsync(TableNames.CDAPRCHILD, entity.Childs.Count(x => x.EntityState == EntityState.Added));
                        maxCompanyId = await _service.GetMaxIdAsync(TableNames.CDA_PR_COMPANY, entity.Childs.Sum(x => x.CDAPRCompanies.Where(y => y.EntityState == EntityState.Added).Count()));

                        foreach (CDAPRChild child in entity.Childs)
                        {
                            if (child.EntityState == EntityState.Added)
                            {
                                child.CDAPRChildID = maxChildId++;
                                child.CDAPRMasterID = entity.CDAPRMasterID;

                                foreach (CDAPRCompany company in child.CDAPRCompanies)
                                {
                                    company.CDAPRCompanyID = maxCompanyId++;
                                    company.CDAPRChildID = child.CDAPRChildID;
                                    company.CDAPRMasterID = entity.CDAPRMasterID;
                                }
                            }
                            else if (child.EntityState == EntityState.Modified)
                            {
                                foreach (CDAPRCompany company in child.CDAPRCompanies.Where(x => x.EntityState == EntityState.Added))
                                {
                                    company.CDAPRCompanyID = maxCompanyId++;
                                    company.CDAPRChildID = child.CDAPRChildID;
                                    company.CDAPRMasterID = entity.CDAPRMasterID;
                                }
                            }
                            else if (child.EntityState == EntityState.Deleted)
                            {
                                child.CDAPRCompanies.SetDeleted();
                                List<CDAPRCompany> CompanyDel = new List<CDAPRCompany>();
                                entity.Childs.ForEach(x => CompanyDel.AddRange(x.CDAPRCompanies.Where(y => y.EntityState == EntityState.Deleted)));
                                await _service.SaveAsync(CompanyDel, transaction);
                            }
                        }
                        break;
                    default:
                        break;
                }
                await _service.SaveSingleAsync(entity, transaction);
                await _service.SaveAsync(entity.Childs, transaction);
                List<CDAPRCompany> companyList = new List<CDAPRCompany>();
                entity.Childs.ForEach(x => companyList.AddRange(x.CDAPRCompanies));
                await _service.SaveAsync(companyList, transaction);
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
        public async Task SaveCPRAsync(CDAPRMaster cdaPRMaster)
        {
            SqlTransaction transaction = null;
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();
                await _service.SaveSingleAsync(cdaPRMaster, transaction);
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
        public async Task SaveFPRAsync(CDAPRMaster entity)
        {
            SqlTransaction transaction = null;
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();
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
        public async Task UpdateEntityAsync(CDAPRMaster entity)
        {
            SqlTransaction transaction = null;
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();
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
    }
}
