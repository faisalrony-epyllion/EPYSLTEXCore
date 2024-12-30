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
using EPYSLTEXCore.Application.Interfaces.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Exceptions;
using System.Data;

namespace EPYSLTEXCore.Application.Services.Inventory.Yarn
{
    public class YarnRnDReqService: IYarnRnDReqService
    {
       
        // private readonly ISignatureRepository _service;
        private readonly IDapperCRUDService<YarnRnDReqMaster> _service;

        SqlTransaction transactionGmt = null;
        private SqlTransaction transaction = null;
        private readonly SqlConnection _connection;
        private readonly SqlConnection _connectionGmt;
        public YarnRnDReqService(IDapperCRUDService<YarnRnDReqMaster> service)
        {
           

            _service = service;

            _service.Connection = service.GetConnection(AppConstants.GMT_CONNECTION);
            _connectionGmt = service.Connection;

            _service.Connection = service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _connection = service.Connection;

        }

        public async Task<List<YarnRnDReqMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo, string pageName, bool isReqForYDShow = true)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By ApproveDate Desc" : paginationInfo.OrderBy;
            var sql = string.Empty;
            if (status == Status.Proposed)
            {
                sql += $@"
            
                ;WITH
                    KPG As (
	                Select KPY.KPYarnID,KPG.GroupID ,KPG.GroupConceptNo,KPM.ConceptID, KPG.KnittingTypeID, 
	                Status = Case When YRC.KPYarnID IS NULL Then 'New'
				                When  ISNULL(KPM.RevisionNo, 0) = ISNULL(YRC.PreProcessRevNo,0) Then 'New'
				                Else 'Revision' End--,sum(YRC.ReqQty)ReqQty
	                from {TableNames.Knitting_Plan_Yarn} KPY
	                LEFT join {TableNames.Knitting_Plan_Master} KPM ON KPM.KPMasterID = KPY.KPMasterID
	                LEFT join {TableNames.Knitting_Plan_Group} KPG ON KPG.GroupID = KPM.PlanNo
	                Left join {TableNames.YARN_RnD_REQ_CHILD} YRC ON YRC.KPYarnID = KPY.KPYarnID AND YRC.ItemMasterID = KPY.ItemMasterID
                    Where KPM.IsBDS <> 2
					/*
					WHERE YRC.KPYarnID IS NULL
					GROUP BY KPY.KPYarnID,KPY.KPMasterID,KPY.YarnCountID,KPY.YarnTypeID,KPY.YarnLotNo,KPY.StitchLength,
					KPY.FCMRChildID,KPY.ItemMasterID,KPY.PhysicalCount,KPY.YarnBrandID,KPY.BatchNo,KPY.YarnPly,KPY.YDItem,KPY.GroupID,KPG.GroupConceptNo,KPM.ConceptID, KPG.KnittingTypeID,Case
                    When YRC.KPYarnID IS NULL Then 'New'
                    When  ISNULL(KPM.RevisionNo, 0) = ISNULL(YRC.PreProcessRevNo,0) Then 'New'
                    Else 'Revision' End
					*/
                ),
				A as(
					select YRC.KPYarnID,YRC.ItemMasterID,YRC.ConceptID,sum(YRC.ReqQty)ReqQty from {TableNames.YARN_RnD_REQ_CHILD} YRC
					--INNER JOIN  KPG ON YRC.KPYarnID = KPG.KPYarnID AND YRC.ItemMasterID = KPG.ItemMasterID
					GROUP BY YRC.KPYarnID,YRC.ItemMasterID,YRC.ConceptID
				),
				--select KPG.*,A.ReqQty from KPG Left Join A on  A.KPYarnID = KPG.KPYarnID AND A.ItemMasterID = KPG.ItemMasterID,
				B as(
					select KPYC.KPYarnID,KPYC.ItemMasterID,KPYC.ConceptID,sum(MRC.ReqQty)PlanQty FROM {TableNames.Knitting_Plan_Yarn_Child} KPYC
					LEFT JOIN {TableNames.RND_FREE_CONCEPT_MR_CHILD} MRC On MRC.FCMRChildID = KPYC.FCMRChildID
					--INNER JOIN  KPG ON KPG.KPYarnID = KPYC.KPYarnID AND KPG.ItemMasterID = KPYC.ItemMasterID AND KPG.ConceptID=KPYC.ConceptID
					GROUP BY KPYC.KPYarnID,KPYC.ItemMasterID,KPYC.ConceptID
				),
				L As
				(
	                Select KPG.GroupID, KPG.Status, ConceptNo = KPG.GroupConceptNo, CM.ConceptDate,  KT.TypeName KnittingType,
		            Composition.SegmentValue Composition,Construction.SegmentValue Construction,Technical.TechnicalName,Gsm.SegmentValue Gsm
		            ,F.ValueName ConceptForName,S.ValueName ConceptStatus,ISG.SubGroupName ItemSubGroup
		            ,Buyer = CASE WHEN CM.BuyerID > 0 THEN C.ShortName ELSE '' END,isnull(A.ReqQty,0)ReqQty
		            ,Color = CASE WHEN CM.SubGroupID = 1 THEN ISV3.SegmentValue ELSE ISV5.SegmentValue END
	                From KPG
	                Inner join {TableNames.RND_FREE_CONCEPT_MASTER} CM ON CM.ConceptID = KPG.ConceptID

		            LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = CM.ItemMasterID
		            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
		            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
		            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID


		            LEFT join {TableNames.KNITTING_MACHINE_TYPE} KT ON KT.TypeID = KPG.KnittingTypeID
		            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = CM.CompositionID
		            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = CM.ConstructionID
		            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = CM.GSMID
		            LEFT JOIN {DbNames.EPYSL}..EntityTypeValue F ON F.ValueID = CM.ConceptFor
		            LEFT join {TableNames.FABRIC_TECHNICAL_NAME} Technical ON Technical.TechnicalNameId = CM.TechnicalNameId
		            LEFT JOIN {DbNames.EPYSL}..EntityTypeValue S ON S.ValueID = CM.ConceptStatusID
		            LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID = CM.SubGroupID
		            LEFT JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = CM.BuyerID
		            LEFT JOIN A on A.KPYarnID = KPG.KPYarnID AND A.ConceptID=KPG.ConceptID
		            LEFT JOIN B on B.KPYarnID = KPG.KPYarnID AND B.ConceptID=KPG.ConceptID
		            WHERE  isnull(A.ReqQty,0)<isnull(B.PlanQty,0) AND [Status] != 'Revision'
		            GROUP BY KPG.GroupID, KPG.Status, KPG.GroupConceptNo, CM.ConceptDate,  KT.TypeName,
		            Composition.SegmentValue,Construction.SegmentValue,Technical.TechnicalName,Gsm.SegmentValue
		            ,F.ValueName,S.ValueName,ISG.SubGroupName
		            ,CASE WHEN CM.BuyerID > 0 THEN C.ShortName ELSE '' END,A.ReqQty,
		            CASE WHEN CM.SubGroupID = 1 THEN ISV3.SegmentValue ELSE ISV5.SegmentValue END
				)

				Select * , Count(*) Over() TotalRows from L ";
                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By GroupID Desc" : paginationInfo.OrderBy;
            }
            else if (status == Status.Pending)
            {
                sql += $@"
                  ;WITH M AS (
	                    SELECT RnDReqMasterID, RnDReqDate, RnDReqNo, Remarks, IsApprove, ApproveBy, RnDReqBy,
	                    ApproveDate, IsAcknowledge, AcknowledgeBy, AcknowledgeDate, RCompanyID, OCompanyID, ConceptNo , 
                        (CASE WHEN IsReqForYD=1 THEN 'Yes' ELSE 'No' END) AS YDStatus,
	                    PreProcessRevNo,RevisionNo, AddedBy, BuyerID, IsWOKnittingInfo,
                        YarnReqStatus = CASE WHEN ISNULL(IsWOKnittingInfo,0) = 1 THEN 'Independent' ELSE '' END,IsAdditional
	                    from {TableNames.YARN_RnD_REQ_MASTER}
	                    WHERE IsApprove = 0 and IsAcknowledge = 0
                        AND RnDReqDate > '11-APR-2020'

                    ), MRS As (
	                    SELECT M.RnDReqMasterID, M.RnDReqDate, M.RnDReqNo, M.Remarks, M.IsApprove, M.ApproveBy, M.ApproveDate, SUM(C.ReqQty) Qty,
	                    M.IsAcknowledge, M.AcknowledgeBy, M.AcknowledgeDate, CompanyName = COM.ShortName, M.ConceptNo,M.YDStatus,M.YarnReqStatus, 
                        M.RnDReqBy, E.EmployeeName RnDReqByName, M.PreProcessRevNo,M.RevisionNo
                        ,Buyer = CASE WHEN M.BuyerID > 0 THEN CT.ShortName ELSE '' END,M.IsAdditional
	                    From M
	                    INNER join {TableNames.YARN_RnD_REQ_CHILD} C ON C.RnDReqMasterID = M.RnDReqMasterID
	                    LEFT join {TableNames.Knitting_Plan_Yarn} KPY ON KPY.KPYarnID=C.KPYarnID
						LEFT join {TableNames.Knitting_Plan_Master} KP ON KP.KPMasterID =KPY.KPMasterID
	                    LEFT JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID=M.RCompanyID
                        LEFT JOIN {DbNames.EPYSL}..LoginUser L ON L.UserCode = M.AddedBy
	                    LEFT Join {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
                        LEFT JOIN {DbNames.EPYSL}..Contacts CT ON CT.ContactID = M.BuyerID
                        WHERE M.RnDReqDate > '11-APR-2020' AND ISNULL(C.PreProcessRevNo,0) = ISNULL(KP.RevisionNo,0)
	                    GROUP BY M.RnDReqMasterID, M.RnDReqDate, M.RnDReqNo, M.Remarks, M.IsApprove, M.ApproveBy, M.ApproveDate,M.YarnReqStatus,
	                    M.IsAcknowledge, M.AcknowledgeBy, M.AcknowledgeDate,COM.ShortName, M.ConceptNo,M.YDStatus,M.RnDReqBy, E.EmployeeName,
	                    M.PreProcessRevNo,M.RevisionNo,M.BuyerID,CT.ShortName,M.IsAdditional
                    )

                    Select *, TotalRows = Count(*) Over()
                    from MRS ";

                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By RnDReqDate Desc" : paginationInfo.OrderBy;
            }
            else if (status == Status.Approved)
            {
                string dateFilter = "11-APR-2020";

                sql += $@"
                   ;WITH M AS (
	                    SELECT RnDReqMasterID, RnDReqDate, RnDReqNo, Remarks, IsApprove, ApproveBy, RnDReqBy,
	                    ApproveDate, IsAcknowledge, AcknowledgeBy, AcknowledgeDate, RCompanyID, OCompanyID, ConceptNo, IsReqForYD,
                        PreProcessRevNo,RevisionNo, AddedBy, BuyerID, IsWOKnittingInfo,
                        YarnReqStatus = CASE WHEN ISNULL(IsWOKnittingInfo,0) = 1 THEN 'Independent' ELSE '' END,IsAdditional
	                    from {TableNames.YARN_RnD_REQ_MASTER}
	                    WHERE IsApprove = 1 and IsAcknowledge = 0
                        AND RnDReqDate > '{dateFilter}'
                    ), MRS As (
	                    SELECT M.RnDReqMasterID, M.RnDReqDate, M.RnDReqNo, M.Remarks, M.IsApprove, M.ApproveBy, M.ApproveDate, SUM(C.ReqQty) Qty, M.YarnReqStatus,
	                    M.IsAcknowledge, M.AcknowledgeBy, M.AcknowledgeDate, CompanyName = COM.ShortName, M.ConceptNo,M.IsReqForYD, M.RnDReqBy, E.EmployeeName RnDReqByName, AE.EmployeeName RnDApproveBy,
	                    M.PreProcessRevNo,M.RevisionNo
                        ,Buyer = CASE WHEN M.BuyerID > 0 THEN CT.ShortName ELSE '' END,M.IsAdditional
	                    From M
	                    INNER join {TableNames.YARN_RnD_REQ_CHILD} C ON C.RnDReqMasterID=M.RnDReqMasterID
	                    LEFT join {TableNames.Knitting_Plan_Yarn} KPY ON KPY.KPYarnID=C.KPYarnID
	                    LEFT join {TableNames.Knitting_Plan_Master} KP ON KP.KPMasterID =KPY.KPMasterID
	                    LEFT JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID=M.RCompanyID
                        LEFT JOIN {DbNames.EPYSL}..LoginUser L ON L.UserCode = M.AddedBy
	                    LEFT Join {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
	                    LEFT Join {DbNames.EPYSL}..Employee AE ON AE.EmployeeCode = M.ApproveBy
                        LEFT JOIN {DbNames.EPYSL}..Contacts CT ON CT.ContactID = M.BuyerID
                        WHERE ISNULL(C.PreProcessRevNo,0) = ISNULL(KP.RevisionNo,0)
	                    GROUP BY M.RnDReqMasterID, M.RnDReqDate, M.RnDReqNo, M.Remarks, M.IsApprove, M.ApproveBy, M.ApproveDate,M.YarnReqStatus,
	                    M.IsAcknowledge, M.AcknowledgeBy, M.AcknowledgeDate,COM.ShortName, M.ConceptNo,M.IsReqForYD,M.RnDReqBy, E.EmployeeName, AE.EmployeeName,
	                    M.PreProcessRevNo,M.RevisionNo,M.BuyerID,CT.ShortName,M.IsAdditional
                    ),
                    MY AS (
	                    SELECT KYReqMasterID, KYReqDate, KYReqNo, Remarks, Approve, ApproveBy, KYReqBy,
	                    ApproveDate, Acknowledge, AcknowledgeBy, AcknowledgeDate, RCompanyID, OCompanyID, ConceptNo, IsReqForYD,
                        PreProcessRevNo,RevisionNo, AddedBy, BuyerID, IsWOKnittingInfo,
                        YarnReqStatus = CASE WHEN ISNULL(IsWOKnittingInfo,0) = 1 THEN 'Independent' ELSE '' END,IsAdditional
	                    FROM {TableNames.KY_Req_Master}
	                    WHERE Approve = 1 and Acknowledge = 0 AND UnAcknowledge=0
                        AND KYReqDate > '{dateFilter}'
                    ), MRSY As (
	                    SELECT MY.KYReqMasterID, MY.KYReqDate, MY.KYReqNo, MY.Remarks, MY.Approve, MY.ApproveBy, MY.ApproveDate, SUM(C.ReqQty) Qty, MY.YarnReqStatus,
	                    MY.Acknowledge, MY.AcknowledgeBy, MY.AcknowledgeDate, CompanyName = COM.ShortName, MY.ConceptNo,MY.IsReqForYD, MY.KYReqBy, E.EmployeeName KYReqByName, AE.EmployeeName KYApproveBy,
	                    MY.PreProcessRevNo,MY.RevisionNo
                        ,Buyer = CASE WHEN MY.BuyerID > 0 THEN CT.ShortName ELSE '' END,MY.IsAdditional
	                    From MY
	                    INNER JOIN {TableNames.KY_Req_Child} C ON C.KYReqMasterID=MY.KYReqMasterID
	                    LEFT join {TableNames.Knitting_Plan_Yarn} KPY ON KPY.KPYarnID=C.KPYarnID
	                    LEFT join {TableNames.Knitting_Plan_Master} KP ON KP.KPMasterID =KPY.KPMasterID
	                    LEFT JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID=MY.RCompanyID
                        LEFT JOIN {DbNames.EPYSL}..LoginUser L ON L.UserCode = MY.AddedBy
	                    LEFT Join {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
	                    LEFT Join {DbNames.EPYSL}..Employee AE ON AE.EmployeeCode = MY.ApproveBy
                        LEFT JOIN {DbNames.EPYSL}..Contacts CT ON CT.ContactID = MY.BuyerID
                        WHERE ISNULL(C.PreProcessRevNo,0) = ISNULL(KP.RevisionNo,0)
	                    GROUP BY MY.KYReqMasterID, MY.KYReqDate, MY.KYReqNo, MY.Remarks, MY.Approve, MY.ApproveBy, MY.ApproveDate,MY.YarnReqStatus,
	                    MY.Acknowledge, MY.AcknowledgeBy, MY.AcknowledgeDate,COM.ShortName, MY.ConceptNo,MY.IsReqForYD,MY.KYReqBy, E.EmployeeName, AE.EmployeeName,
	                    MY.PreProcessRevNo,MY.RevisionNo,MY.BuyerID,CT.ShortName,MY.IsAdditional
                    )
                    Select *, TotalRows = Count(*) Over() from(
	                    Select *,'R&D' AS RequisitionType from MRS
	                    UNION
	                    Select *,'Bulk' AS RequisitionType from MRSY
                    )FD
                    ";
            }
            else if (status == Status.ProposedForAcknowledge)
            {
                string reqForYDConditionM = "";
                string reqForYDConditionMY = "";
                string dateFilter = "11-APR-2020";

                //if (!isReqForYDShow)
                {
                    reqForYDConditionM = pageName == PageNames.YDMRSACK ? " AND M.IsReqForYD = 1 " : " AND M.IsReqForYD = 0 ";
                    reqForYDConditionMY = pageName == PageNames.YDMRSACK ? " AND MY.IsReqForYD = 1 " : " AND MY.IsReqForYD = 0 ";
                    dateFilter = "25-May-2024";
                }
                sql += $@"
                   ;WITH M AS (
	                    SELECT RnDReqMasterID, RnDReqDate, RnDReqNo, Remarks, IsApprove, ApproveBy, RnDReqBy,
	                    ApproveDate, IsAcknowledge, AcknowledgeBy, AcknowledgeDate, RCompanyID, OCompanyID, ConceptNo, IsReqForYD,
                        PreProcessRevNo,RevisionNo, AddedBy, BuyerID, IsWOKnittingInfo,
                        YarnReqStatus = CASE WHEN ISNULL(IsWOKnittingInfo,0) = 1 THEN 'Independent' ELSE '' END,IsAdditional
	                    from {TableNames.YARN_RnD_REQ_MASTER}
	                    WHERE IsApprove = 1 and IsAcknowledge = 0
                        AND RnDReqDate > '{dateFilter}'
                    ), MRS As (
	                    SELECT M.RnDReqMasterID, M.RnDReqDate, M.RnDReqNo, M.Remarks, M.IsApprove, M.ApproveBy, M.ApproveDate, SUM(C.ReqQty) Qty, M.YarnReqStatus,
	                    M.IsAcknowledge, M.AcknowledgeBy, M.AcknowledgeDate, CompanyName = COM.ShortName, M.ConceptNo,M.IsReqForYD, M.RnDReqBy, E.EmployeeName RnDReqByName, AE.EmployeeName RnDApproveBy,
	                    M.PreProcessRevNo,M.RevisionNo
                        ,Buyer = CASE WHEN M.BuyerID > 0 THEN CT.ShortName ELSE '' END,M.IsAdditional
	                    From M
	                    INNER join {TableNames.YARN_RnD_REQ_CHILD} C ON C.RnDReqMasterID=M.RnDReqMasterID
	                    LEFT join {TableNames.Knitting_Plan_Yarn} KPY ON KPY.KPYarnID=C.KPYarnID
	                    LEFT join {TableNames.Knitting_Plan_Master} KP ON KP.KPMasterID =KPY.KPMasterID
	                    LEFT JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID=M.RCompanyID
                        LEFT JOIN {DbNames.EPYSL}..LoginUser L ON L.UserCode = M.AddedBy
	                    LEFT Join {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
	                    LEFT Join {DbNames.EPYSL}..Employee AE ON AE.EmployeeCode = M.ApproveBy
                        LEFT JOIN {DbNames.EPYSL}..Contacts CT ON CT.ContactID = M.BuyerID
                        WHERE ISNULL(C.PreProcessRevNo,0) = ISNULL(KP.RevisionNo,0) {reqForYDConditionM}
	                    GROUP BY M.RnDReqMasterID, M.RnDReqDate, M.RnDReqNo, M.Remarks, M.IsApprove, M.ApproveBy, M.ApproveDate,M.YarnReqStatus,
	                    M.IsAcknowledge, M.AcknowledgeBy, M.AcknowledgeDate,COM.ShortName, M.ConceptNo,M.IsReqForYD,M.RnDReqBy, E.EmployeeName, AE.EmployeeName,
	                    M.PreProcessRevNo,M.RevisionNo,M.BuyerID,CT.ShortName,M.IsAdditional
                    ),
                    MY AS (
	                    SELECT KYReqMasterID, KYReqDate, KYReqNo, Remarks, Approve, ApproveBy, KYReqBy,
	                    ApproveDate, Acknowledge, AcknowledgeBy, AcknowledgeDate, RCompanyID, OCompanyID, ConceptNo, IsReqForYD,
                        PreProcessRevNo,RevisionNo, AddedBy, BuyerID, IsWOKnittingInfo,
                        YarnReqStatus = CASE WHEN ISNULL(IsWOKnittingInfo,0) = 1 THEN 'Independent' ELSE '' END,IsAdditional
	                    FROM {TableNames.KY_Req_Master}
	                    WHERE Approve = 1 and Acknowledge = 0 AND UnAcknowledge=0
                        AND KYReqDate > '{dateFilter}'
                    ), MRSY As (
	                    SELECT MY.KYReqMasterID, MY.KYReqDate, MY.KYReqNo, MY.Remarks, MY.Approve, MY.ApproveBy, MY.ApproveDate, SUM(C.ReqQty) Qty, MY.YarnReqStatus,
	                    MY.Acknowledge, MY.AcknowledgeBy, MY.AcknowledgeDate, CompanyName = COM.ShortName, MY.ConceptNo,MY.IsReqForYD, MY.KYReqBy, E.EmployeeName KYReqByName, AE.EmployeeName KYApproveBy,
	                    MY.PreProcessRevNo,MY.RevisionNo
                        ,Buyer = CASE WHEN MY.BuyerID > 0 THEN CT.ShortName ELSE '' END,MY.IsAdditional
	                    From MY
	                    INNER JOIN {TableNames.KY_Req_Child} C ON C.KYReqMasterID=MY.KYReqMasterID
	                    LEFT join {TableNames.Knitting_Plan_Yarn} KPY ON KPY.KPYarnID=C.KPYarnID
	                    LEFT join {TableNames.Knitting_Plan_Master} KP ON KP.KPMasterID =KPY.KPMasterID
	                    LEFT JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID=MY.RCompanyID
                        LEFT JOIN {DbNames.EPYSL}..LoginUser L ON L.UserCode = MY.AddedBy
	                    LEFT Join {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
	                    LEFT Join {DbNames.EPYSL}..Employee AE ON AE.EmployeeCode = MY.ApproveBy
                        LEFT JOIN {DbNames.EPYSL}..Contacts CT ON CT.ContactID = MY.BuyerID
                        WHERE ISNULL(C.PreProcessRevNo,0) = ISNULL(KP.RevisionNo,0) {reqForYDConditionMY}
	                    GROUP BY MY.KYReqMasterID, MY.KYReqDate, MY.KYReqNo, MY.Remarks, MY.Approve, MY.ApproveBy, MY.ApproveDate,MY.YarnReqStatus,
	                    MY.Acknowledge, MY.AcknowledgeBy, MY.AcknowledgeDate,COM.ShortName, MY.ConceptNo,MY.IsReqForYD,MY.KYReqBy, E.EmployeeName, AE.EmployeeName,
	                    MY.PreProcessRevNo,MY.RevisionNo,MY.BuyerID,CT.ShortName,MY.IsAdditional
                    )
                    Select *, TotalRows = Count(*) Over() from(
	                    Select *,'R&D' AS RequisitionType from MRS
	                    UNION
	                    Select *,'Bulk' AS RequisitionType from MRSY
                    )FD
                    ";
            }
            else if (status == Status.Revise)
            {
                sql += $@";
                    WITH M AS 
                    (
                        SELECT M.RnDReqMasterID, M.RnDReqDate, M.RnDReqNo, M.Remarks, M.IsApprove, M.ApproveBy, M.ApproveDate, M.RnDReqBy, E.EmployeeName RnDReqByName, SUM(C.ReqQty) Qty,
                        M.IsAcknowledge, M.AcknowledgeBy, M.AcknowledgeDate, CompanyName = COM.ShortName, M.ConceptNo,(CASE WHEN IsReqForYD=1 THEN 'Yes' ELSE 'No' END) AS YDStatus
                        ,FCMRMasterIDs=STRING_AGG(C.FCMRMasterID, ',')
                        ,Buyer = CASE WHEN M.BuyerID > 0 THEN CT.ShortName ELSE '' END, M.IsWOKnittingInfo,
                        YarnReqStatus = CASE WHEN ISNULL(M.IsWOKnittingInfo,0) = 1 THEN 'Independent' ELSE '' END,M.IsAdditional
                        from {TableNames.YARN_RnD_REQ_MASTER} M
                        INNER join {TableNames.YARN_RnD_REQ_CHILD} C ON C.RnDReqMasterID = M.RnDReqMasterID
                        LEFT JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID=M.RCompanyID
                        LEFT join {TableNames.Knitting_Plan_Yarn} KPY ON KPY.KPYarnID=C.KPYarnID
                        LEFT join {TableNames.Knitting_Plan_Master} KP ON KP.KPMasterID = KPY.KPMasterID
                        LEFT join {TableNames.Knitting_Plan_Group} KPG ON KPG.GroupID = KPY.GroupID
                        LEFT JOIN {DbNames.EPYSL}..LoginUser L ON L.UserCode = M.AddedBy
                        LEFT Join {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
                        LEFT JOIN {DbNames.EPYSL}..Contacts CT ON CT.ContactID = M.BuyerID
                        WHERE (ISNULL(C.PreProcessRevNo,0) != ISNULL(KP.RevisionNo,0) OR ISNULL(KPY.KPYarnID,0) = 0)
                        GROUP BY M.RnDReqMasterID, M.RnDReqDate, M.RnDReqNo, M.Remarks, M.IsApprove, M.ApproveBy, M.ApproveDate, M.IsWOKnittingInfo,
                        M.IsAcknowledge, M.AcknowledgeBy, M.AcknowledgeDate,COM.ShortName, M.ConceptNo, IsReqForYD,M.RnDReqBy, E.EmployeeName,M.BuyerID,CT.ShortName,M.IsAdditional
                    )
                    Select *, Count(*) Over() TotalRows
                    from M";
                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By RnDReqMasterID Desc" : paginationInfo.OrderBy;
            }
            else
            {
                string dateFilter = "11-APR-2020";
                string reqForYDConditionM = pageName == PageNames.YDMRSACK ? " AND M.IsReqForYD = 1 " : " AND M.IsReqForYD = 0 ";
                string reqForYDConditionMY = pageName == PageNames.YDMRSACK ? " AND MY.IsReqForYD = 1 " : " AND MY.IsReqForYD = 0 ";
                sql += $@";
                    WITH M AS (
	                    SELECT YRRM.RnDReqMasterID, YRRM.RnDReqDate, YRRM.RnDReqNo, YRRM.Remarks, YRRM.IsApprove, YRRM.ApproveBy, YRRM.RnDReqBy,
	                    (CASE WHEN IsnUll(YRIM.RnDReqMasterID, 0) != 0 THEN 'Yes' ELSE 'No' END) AS IsIssue,
	                    YRRM.ApproveDate, YRRM.IsAcknowledge, YRRM.AcknowledgeBy, YRRM.AcknowledgeDate, YRRM.RCompanyID, YRRM.OCompanyID, YRRM.ConceptNo,
	                    (CASE WHEN YRRM.IsReqForYD=1 THEN 'Yes' ELSE 'No' END) AS YDStatus,
	                    PreProcessRevNo,RevisionNo, YRRM.AddedBy, YRRM.BuyerID, YRRM.IsWOKnittingInfo,
                        YarnReqStatus = CASE WHEN ISNULL(YRRM.IsWOKnittingInfo,0) = 1 THEN 'Independent' ELSE '' END,IsAdditional, YRRM.IsReqForYD
	                    from {TableNames.YARN_RnD_REQ_MASTER} as YRRM left JOIN {TableNames.YARN_RND_ISSUE_MASTER} as YRIM  on YRIM.RnDReqMasterID= YRRM.RnDReqMasterID
	                    WHERE IsApprove = 1 and IsAcknowledge = 1
	                    AND YRRM.RnDReqDate > '{dateFilter}'
                    ), 
                    MRS As (
	                    SELECT M.RnDReqMasterID, M.RnDReqDate, M.RnDReqNo, M.Remarks, M.IsApprove, M.ApproveBy, M.ApproveDate, SUM(C.ReqQty) Qty, M.YarnReqStatus, M.IsWOKnittingInfo,
	                    M.IsAcknowledge, M.AcknowledgeBy, M.AcknowledgeDate, CompanyName = COM.ShortName, M.ConceptNo,M.YDStatus,M.IsIssue,M.RnDReqBy, E.EmployeeName RnDReqByName,
	                    M.PreProcessRevNo,M.RevisionNo
                        ,Buyer = CASE WHEN M.BuyerID > 0 THEN CT.ShortName ELSE '' END,M.IsAdditional
	                    From M
	                    INNER join {TableNames.YARN_RnD_REQ_CHILD} C ON C.RnDReqMasterID=M.RnDReqMasterID
	                    LEFT join {TableNames.Knitting_Plan_Yarn} KPY ON KPY.KPYarnID=C.KPYarnID
						LEFT join {TableNames.Knitting_Plan_Master} KP ON KP.KPMasterID =KPY.KPMasterID
	                    LEFT JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID=M.RCompanyID
	                    LEFT JOIN {DbNames.EPYSL}..LoginUser L ON L.UserCode = M.AddedBy
	                    LEFT Join {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
                        LEFT JOIN {DbNames.EPYSL}..Contacts CT ON CT.ContactID = M.BuyerID
	                    WHERE ISNULL(C.PreProcessRevNo,0) = ISNULL(KP.RevisionNo,0) {reqForYDConditionM}
	                    GROUP BY M.RnDReqMasterID, M.RnDReqDate, M.RnDReqNo, M.Remarks, M.IsApprove, M.ApproveBy, M.ApproveDate,M.YarnReqStatus, M.IsWOKnittingInfo,
	                    M.IsAcknowledge, M.AcknowledgeBy, M.AcknowledgeDate,COM.ShortName, M.ConceptNo,M.YDStatus,M.IsIssue, M.RnDReqBy, E.EmployeeName,
	                    M.PreProcessRevNo,M.RevisionNo,M.BuyerID,CT.ShortName,M.IsAdditional
                    ),
					 MY AS (
	                    SELECT YRRM.KYReqMasterID, YRRM.KYReqDate, YRRM.KYReqNo, YRRM.Remarks, YRRM.Approve, YRRM.ApproveBy, YRRM.KYReqBy,
	                    (CASE WHEN IsnUll(YRIM.KYReqMasterID, 0) != 0 THEN 'Yes' ELSE 'No' END) AS IsIssue,
	                    YRRM.ApproveDate, YRRM.Acknowledge, YRRM.AcknowledgeBy, YRRM.AcknowledgeDate, YRRM.RCompanyID, YRRM.OCompanyID, YRRM.ConceptNo,
	                    (CASE WHEN YRRM.IsReqForYD=1 THEN 'Yes' ELSE 'No' END) AS YDStatus,
	                    PreProcessRevNo,RevisionNo, YRRM.AddedBy, YRRM.BuyerID, YRRM.IsWOKnittingInfo,
                        YarnReqStatus = CASE WHEN ISNULL(YRRM.IsWOKnittingInfo,0) = 1 THEN 'Independent' ELSE '' END,IsAdditional, YRRM.IsReqForYD
	                    FROM {TableNames.KY_Req_Master} as YRRM left join {TableNames.KY_Issue_Master} as YRIM  on YRIM.KYReqMasterID= YRRM.KYReqMasterID
	                    WHERE YRRM.Approve = 1 and YRRM.Acknowledge = 1
	                    AND YRRM.KYReqDate > '{dateFilter}'
                    ), 
                    MRSY As (
	                    SELECT MY.KYReqMasterID, MY.KYReqDate, MY.KYReqNo, MY.Remarks, MY.Approve, MY.ApproveBy, MY.ApproveDate, SUM(C.ReqQty) Qty, MY.YarnReqStatus, MY.IsWOKnittingInfo,
	                    MY.Acknowledge, MY.AcknowledgeBy, MY.AcknowledgeDate, CompanyName = COM.ShortName, MY.ConceptNo,MY.YDStatus,MY.IsIssue,MY.KYReqBy, E.EmployeeName RnDReqByName,
	                    MY.PreProcessRevNo,MY.RevisionNo
                        ,Buyer = CASE WHEN MY.BuyerID > 0 THEN CT.ShortName ELSE '' END,MY.IsAdditional
	                    From MY
	                    INNER JOIN {TableNames.KY_Req_Child} C ON C.KYReqMasterID=MY.KYReqMasterID
	                    LEFT join {TableNames.Knitting_Plan_Yarn} KPY ON KPY.KPYarnID=C.KPYarnID
						LEFT join {TableNames.Knitting_Plan_Master} KP ON KP.KPMasterID =KPY.KPMasterID
	                    LEFT JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID=MY.RCompanyID
	                    LEFT JOIN {DbNames.EPYSL}..LoginUser L ON L.UserCode = MY.AddedBy
	                    LEFT Join {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
                        LEFT JOIN {DbNames.EPYSL}..Contacts CT ON CT.ContactID = MY.BuyerID
	                    WHERE ISNULL(C.PreProcessRevNo,0) = ISNULL(KP.RevisionNo,0) {reqForYDConditionMY}
	                    GROUP BY MY.KYReqMasterID, MY.KYReqDate, MY.KYReqNo, MY.Remarks, MY.Approve, MY.ApproveBy, MY.ApproveDate,MY.YarnReqStatus, MY.IsWOKnittingInfo,
	                    MY.Acknowledge, MY.AcknowledgeBy, MY.AcknowledgeDate,COM.ShortName, MY.ConceptNo,MY.YDStatus,MY.IsIssue, MY.KYReqBy, E.EmployeeName,
	                    MY.PreProcessRevNo,MY.RevisionNo,MY.BuyerID,CT.ShortName,MY.IsAdditional
                    )
                    Select *, TotalRows = Count(*) Over() from(
                    Select *,'R&D' AS RequisitionType from MRS
                    UNION
					Select *,'Bulk' AS RequisitionType from MRSY
                    )FD
                    /*WITH M AS (
	                    SELECT YRRM.RnDReqMasterID, YRRM.RnDReqDate, YRRM.RnDReqNo, YRRM.Remarks, YRRM.IsApprove, YRRM.ApproveBy, YRRM.RnDReqBy,
	                    (CASE WHEN IsnUll(YRIM.RnDReqMasterID, 0) != 0 THEN 'Yes' ELSE 'No' END) AS IsIssue,
	                    YRRM.ApproveDate, YRRM.IsAcknowledge, YRRM.AcknowledgeBy, YRRM.AcknowledgeDate, YRRM.RCompanyID, YRRM.OCompanyID, YRRM.ConceptNo,
	                    (CASE WHEN YRRM.IsReqForYD=1 THEN 'Yes' ELSE 'No' END) AS YDStatus,
	                    PreProcessRevNo,RevisionNo, YRRM.AddedBy, YRRM.BuyerID, YRRM.IsWOKnittingInfo,
                        YarnReqStatus = CASE WHEN ISNULL(YRRM.IsWOKnittingInfo,0) = 1 THEN 'Independent' ELSE '' END,IsAdditional
	                    from {TableNames.YARN_RnD_REQ_MASTER} as YRRM left JOIN {TableNames.YARN_RND_ISSUE_MASTER} as YRIM  on YRIM.RnDReqMasterID= YRRM.RnDReqMasterID
	                    WHERE IsApprove = 1 and IsAcknowledge = 1
	                    AND YRRM.RnDReqDate > '{dateFilter}'
                    ), 
                    MRS As (
	                    SELECT M.RnDReqMasterID, M.RnDReqDate, M.RnDReqNo, M.Remarks, M.IsApprove, M.ApproveBy, M.ApproveDate, SUM(C.ReqQty) Qty, M.YarnReqStatus, M.IsWOKnittingInfo,
	                    M.IsAcknowledge, M.AcknowledgeBy, M.AcknowledgeDate, CompanyName = COM.ShortName, M.ConceptNo,M.YDStatus,M.IsIssue,M.RnDReqBy, E.EmployeeName RnDReqByName,
	                    M.PreProcessRevNo,M.RevisionNo
                        ,Buyer = CASE WHEN M.BuyerID > 0 THEN CT.ShortName ELSE '' END,M.IsAdditional
	                    From M
	                    INNER join {TableNames.YARN_RnD_REQ_CHILD} C ON C.RnDReqMasterID=M.RnDReqMasterID
	                    LEFT join {TableNames.Knitting_Plan_Yarn} KPY ON KPY.KPYarnID=C.KPYarnID
						LEFT join {TableNames.Knitting_Plan_Master} KP ON KP.KPMasterID =KPY.KPMasterID
	                    LEFT JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID=M.RCompanyID
	                    LEFT JOIN {DbNames.EPYSL}..LoginUser L ON L.UserCode = M.AddedBy
	                    LEFT Join {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
                        LEFT JOIN {DbNames.EPYSL}..Contacts CT ON CT.ContactID = M.BuyerID
	                    WHERE ISNULL(C.PreProcessRevNo,0) = ISNULL(KP.RevisionNo,0)
	                    GROUP BY M.RnDReqMasterID, M.RnDReqDate, M.RnDReqNo, M.Remarks, M.IsApprove, M.ApproveBy, M.ApproveDate,M.YarnReqStatus, M.IsWOKnittingInfo,
	                    M.IsAcknowledge, M.AcknowledgeBy, M.AcknowledgeDate,COM.ShortName, M.ConceptNo,M.YDStatus,M.IsIssue, M.RnDReqBy, E.EmployeeName,
	                    M.PreProcessRevNo,M.RevisionNo,M.BuyerID,CT.ShortName,M.IsAdditional
                    )
                    Select *, TotalRows = Count(*) Over()
                    from MRS*/";
            }

            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<YarnRnDReqMaster>(sql);
        }

        public async Task<List<FreeConceptMRMaster>> GetMRs(string fcIds, PaginationInfo paginationInfo)
        {
            paginationInfo.OrderBy = string.IsNullOrEmpty(paginationInfo.OrderBy) ? "ORDER BY FCMRMasterID DESC" : paginationInfo.OrderBy;
            var sql = $@"
                 Select MR.FCMRMasterID FCMRMasterID,M.ConceptId ConceptID,ConceptNo,ConceptDate,GroupID = KPM.PlanNo,
		        F.ValueName ConceptForName, Count(*) Over() TotalRows
		        from {TableNames.FreeConceptMRMaster} MR
		        INNER join {TableNames.RND_FREE_CONCEPT_MASTER} M ON M.ConceptID=MR.ConceptID
		        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue F ON F.ValueID = M.ConceptFor
		        LEFT join {TableNames.YARN_RnD_REQ_CHILD} AS YRC ON YRC.FCMRMasterID = MR.FCMRMasterID
                INNER join {TableNames.Knitting_Plan_Master} KPM ON KPM.ConceptID=MR.ConceptID
                WHERE  YRC.FCMRMasterID IS NULL AND KPM.PlanNo NOT IN ({fcIds})
                {paginationInfo.FilterBy}
                {paginationInfo.OrderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<FreeConceptMRMaster>(sql);
        }

        public async Task<YarnRnDReqMaster> GetFreeConceptMRData(string[] fcIds, string Status)
        {
            var query = "";
            if (Status == "New")
            {
                query =
                $@"-- Master Data
                WITH M As 
                (
	                Select *
	                from {TableNames.FreeConceptMRMaster}
	                Where FCMRMasterID IN ({fcIds})
                ) 
                Select M.FCMRMasterID FCMRMasterID, M.ConceptID, ConceptNo, ConceptDate, M.TrialNo, CM.TrialDate, M.ReqDate, 
                CM.ConceptFor, CM.CompanyID RCompanyID, COM.ShortName RCompanyName, CM.CompanyID OCompanyID, KnittingTypeID, 
                CM.ConstructionID, CM.TechnicalNameId, CompositionID, GSMID, Qty,ConceptStatusID, M.Remarks, E.ValueName ConceptForName,
	            '{Status}' As Status , KnittingType.TypeName KnittingType, Composition.SegmentValue Composition,
                Construction.SegmentValue Construction,FTN.TechnicalName, Gsm.SegmentValue GSM, KJCM.ContactID FloorID, 
                KU.ShortName FloorName
                From M
				Left join {TableNames.KNITTING_JOB_CARD_Master} KJCM ON M.ConceptID = KJCM.ConceptID
				LEFT join {TableNames.KNITTING_UNIT} KU ON KJCM.ContactID = KU.KnittingUnitID And KJCM.IsSubContact = 0
                LEFT join {TableNames.RND_FREE_CONCEPT_MASTER} CM ON M.ConceptID = CM.ConceptID
                LEFT JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID = CM.CompanyID
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue E ON E.ValueID = CM.ConceptFor
                LEFT join {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID = CM.KnittingTypeID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = CM.CompositionID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = CM.ConstructionID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = CM.GSMID
                LEFT join {TableNames.FABRIC_TECHNICAL_NAME} FTN ON CM.TechnicalNameId = FTN.TechnicalNameId;

                --Childs
                WITH
                FCMR As (
	                Select *
	                from {TableNames.FreeConceptMRMaster}
	                Where FCMRMasterID IN ({fcIds})
                )

				Select FCC.FCMRChildID FCMRChildID, FCC.FCMRMasterID, FCC.ItemMasterID, FCC.YD, FCC.ReqQty, FCMR.ConceptID,
                CM.ConceptNo, FCC.ShadeCode, IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, IM.Segment4ValueID, 
                IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID, IM.Segment8ValueID, ISV1.SegmentValue Segment1ValueDesc, 
                ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc, 
                ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc,
                IM.Segment8ValueID Segment8ValueDesc,KPY.YarnLotNo,KPY.YarnBrandID, C.ShortName YarnBrand, FCC.ReqCone, 
                KPY.PhysicalCount,KPY.KPYarnID
                From FCMR
                INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_CHILD} FCC ON FCC.FCMRMasterID = FCMR.FCMRMasterID
                INNER join {TableNames.RND_FREE_CONCEPT_MASTER} CM ON FCMR.ConceptID = CM.ConceptID
				LEFT join {TableNames.Knitting_Plan_Yarn} KPY ON FCC.FCMRChildID=KPY.FCMRChildID
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = FCC.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                Left Join {DbNames.EPYSL}..Contacts C ON KPY.YarnBrandID = C.ContactID
                Left join {TableNames.YARN_RnD_REQ_CHILD} YRC ON YRC.FCMRChildID = FCC.FCMRChildID

				Group by FCC.FCMRChildID, FCC.FCMRMasterID, FCC.ItemMasterID, FCC.YD, FCC.ReqQty,FCC.ReqCone,FCMR.ConceptID,
                CM.ConceptNo, FCC.ShadeCode, IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, IM.Segment4ValueID, 
                IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID, IM.Segment8ValueID, ISV1.SegmentValue, 
                ISV2.SegmentValue, ISV3.SegmentValue, ISV4.SegmentValue, ISV5.SegmentValue, ISV6.SegmentValue, 
                ISV7.SegmentValue, IM.Segment8ValueID,KPY.YarnLotNo,KPY.YarnBrandID, C.ShortName, FCC.ReqCone, KPY.PhysicalCount,KPY.KPYarnID;

                ---FCMR List
                WITH M As 
                (
	                Select *
	                from {TableNames.FreeConceptMRMaster}
	                Where FCMRMasterID IN ({fcIds})
                ) 
                Select M.FCMRMasterID FCMRMasterID, M.ConceptID, ConceptNo, ConceptDate, CM.ConceptFor,E.ValueName ConceptForName
                From M
                LEFT join {TableNames.RND_FREE_CONCEPT_MASTER} CM ON M.ConceptID = CM.ConceptID
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue E ON E.ValueID = CM.ConceptFor;
                ----Company
                {CommonQueries.GetCompany()};
                 ----Company
                {CommonQueries.GetYarnSpinners()}";
            }
            else
            {
                query =
                $@"
                WITH M As 
                (
                    Select RnDReqMasterID, RnDReqDate, RnDReqNo, RCompanyID, OCompanyID, IsApprove, IsAcknowledge, IsReqForYD ,'{Status}' As Status 
                    from {TableNames.YARN_RnD_REQ_MASTER} MMM
                    Where MMM.RnDReqMasterID In (Select Distinct YPC.RnDReqMasterID
                        from {TableNames.FreeConceptMRMaster} FCM Inner join {TableNames.YARN_RnD_REQ_CHILD} YPC On FCM.FCMRMasterID= YPC.FCMRMasterID
                        Where FCM.FCMRMasterID IN ({fcIds}))
                )
                Select * from M;

                -- Child Data
                ;WITH YRC As 
                (
                    Select FCC.FCMRChildID As RnDReqChildID, FCC.FCMRChildID, FCM.FCMRMasterID As YarnPRMasterID,FCM.ConceptID,
                    FCC.ItemMasterID,YPC.FCMRMasterID, YPC.BatchNo,YPC.YarnLotNo,YPC.YarnBrandID,YPC.PhysicalCount,
                    YM.RnDReqMasterID,YM.FloorID, YPC.Remarks, (Case When Isnull(YPC.ReqQty, 0) = 0 Then FCC.ReqQty Else YPC.ReqQty End)ReqQty,
                    (Case When Isnull(YPC.ReqCone, 0) = 0 Then FCC.ReqCone Else YPC.ReqCone End)ReqCone, YPC.ShadeCode, 
                    FCC.UnitID, 'Kg' AS DisplayUnitDesc
                    from {TableNames.FreeConceptMRMaster} FCM 
                    Inner JOIN {TableNames.RND_FREE_CONCEPT_MR_CHILD} FCC On FCM.FCMRMasterID = FCC.FCMRMasterID
                    Left join {TableNames.YARN_RnD_REQ_CHILD} YPC ON YPC.FCMRChildID = FCC.FCMRChildID And YPC.FCMRMasterID = FCM.FCMRMasterID
                    inner join {TableNames.YARN_RnD_REQ_MASTER} YM ON YM.RnDReqMasterID=YPC.RnDReqMasterID
                    Where FCM.FCMRMasterID IN ({fcIds})
                )
                Select YRC.RnDReqChildID, YRC.FCMRChildID FCMRChildID, YRC.FCMRMasterID, YRC.ItemMasterID,ReqQty,YRC.ConceptID,
                CM.ConceptNo, YRC.ShadeCode, YRC.BatchNo, IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, 
                IM.Segment4ValueID, IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID, IM.Segment8ValueID, 
                ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc,
                ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, 
                ISV7.SegmentValue Segment7ValueDesc, YRC.Remarks,YRC.YarnLotNo,YRC.YarnBrandID, C.ShortName YarnBrand,ReqCone,
                UN.DisplayUnitDesc, YRC.PhysicalCount, ShadeCode, FCMR.FloorID, KU.ShortName FloorName
                From YRC
                INNER join {TableNames.YARN_RnD_REQ_MASTER} FCMR ON YRC.RnDReqMasterID = FCMR.RnDReqMasterID
                INNER join {TableNames.RND_FREE_CONCEPT_MASTER} CM ON YRC.ConceptID = CM.ConceptID
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YRC.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
				LEFT JOIN {DbNames.EPYSL}..Unit UN ON YRC.UnitID=UN.UnitID
                LEFT JOIN {DbNames.EPYSL}..Contacts C ON YRC.YarnBrandID = C.ContactID
                LEFT join {TableNames.KNITTING_UNIT} KU ON YRC.FloorID = KU.KnittingUnitID;

                -- Free Concpet MR List
                Select distinct YRDC.FCMRMasterID,F.ValueName ConceptForName,FCM.ConceptNo,FCM.ConceptDate, Count(*) Over() TotalRows
                from {TableNames.YARN_RnD_REQ_CHILD} AS  YRDC
                INNER join {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID =YRDC.ConceptID
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue F ON F.ValueID = FCM.ConceptFor
                WHERE YRDC.RnDReqMasterID = {fcIds}
                -- Company
                {CommonQueries.GetCompany()};
                -- Brand
                {CommonQueries.GetYarnSpinners()};";
            }

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                YarnRnDReqMaster data = await records.ReadFirstOrDefaultAsync<YarnRnDReqMaster>();
                Guard.Against.NullObject(data);

                data.Childs = records.Read<YarnRnDReqChild>().ToList();
                data.FreeConceptMR = records.Read<FreeConceptMRMaster>().ToList();
                data.RCompanyList = records.Read<Select2OptionModel>().ToList();
                data.YarnBrandList = records.Read<Select2OptionModel>().ToList();
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

        public async Task<YarnRnDReqMaster> GetFreeConceptMRDataNew(List<YarnRnDReqMaster> entity)
        {
            var query = "";
            if (entity[0].Status == "New")
            {
                query =
                $@"-- Master Data
                 WITH M As 
                (
	                Select *
	                FROM {TableNames.Knitting_Plan_Group}
	                Where GroupID IN ({entity[0].GroupIDs})
                ) 
                Select M.GroupID, M.GroupConceptNo ConceptNo,  CM.ConceptDate, CM.TrialNo, CM.TrialDate, 
                CM.ConceptFor, CM.CompanyID RCompanyID, COM.ShortName RCompanyName, CM.CompanyID OCompanyID, M.KnittingTypeID, 
                Qty = SUM(Qty),ConceptStatusID, CM.Remarks, E.ValueName ConceptForName,
	            'New' As Status
                From M
                LEFT join {TableNames.RND_FREE_CONCEPT_MASTER} CM ON CM.GroupConceptNo = M.GroupConceptNo
                LEFT JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID = CM.CompanyID
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue E ON E.ValueID = CM.ConceptFor
                LEFT join {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID = CM.KnittingTypeID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = CM.CompositionID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = CM.GSMID
                LEFT join {TableNames.FABRIC_TECHNICAL_NAME} FTN ON CM.TechnicalNameId = FTN.TechnicalNameId
				GROUP BY M.GroupID, M.GroupConceptNo,  CM.ConceptDate, CM.TrialNo, CM.TrialDate, 
                CM.ConceptFor, CM.CompanyID, COM.ShortName, M.KnittingTypeID, 
                ConceptStatusID, CM.Remarks, E.ValueName;

                --Childs
                 ;WITH
                KPG As (
	                Select *
	                FROM {TableNames.Knitting_Plan_Group}
	                Where GroupID IN ({entity[0].GroupIDs})
                ),
				A as(
				    select YRC.KPYarnID,YRC.ItemMasterID,YRC.ConceptID,sum(YRC.ReqQty)ReqQty from {TableNames.YARN_RnD_REQ_CHILD} YRC
				    GROUP BY YRC.KPYarnID,YRC.ItemMasterID,YRC.ConceptID
				),
				L As(
	                Select KPG.GroupID, KPY.ItemMasterID, CM.ConceptID, CM.BookingID,
	                CM.ConceptNo, IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, IM.Segment4ValueID, 
	                IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID, IM.Segment8ValueID, ISV1.SegmentValue Segment1ValueDesc, 
	                ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc, 
	                ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc,
	                IM.Segment8ValueID Segment8ValueDesc,KPY.YarnLotNo,KPY.YarnBrandID, KPY.BatchNo, C.ShortName YarnBrand, 
	                KPY.PhysicalCount,CM.GroupConceptNo, CM.ConceptDate,KPY.KPYarnID, YRC.ShadeCode, KPYC.FCMRChildID, KPY.YDItem, KPM.RevisionNo
	                From KPG
	                Inner join {TableNames.Knitting_Plan_Master} KPM ON KPM.PlanNo = KPG.GroupID
	                Inner join {TableNames.Knitting_Plan_Yarn} KPY ON KPY.KPMasterID = KPM.KPMasterID
                    INNER JOIN (
		                SELECT KPYC.KPYarnID, KPYC.FCMRChildID
		                FROM {TableNames.Knitting_Plan_Yarn_Child} KPYC
		                INNER join {TableNames.Knitting_Plan_Yarn} KPY ON KPY.KPYarnID = KPYC.KPYarnID
		                WHERE KPY.GroupID IN ({entity[0].GroupIDs})
	                ) KPYC ON KPYC.KPYarnID = KPY.KPYarnID
	                Inner join {TableNames.RND_FREE_CONCEPT_MASTER} CM ON CM.ConceptID = KPM.ConceptID
	                LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = KPY.ItemMasterID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
	                Left Join {DbNames.EPYSL}..Contacts C ON KPY.YarnBrandID = C.ContactID
	                Left join {TableNames.YARN_RnD_REQ_CHILD} YRC ON YRC.KPYarnID = KPY.KPYarnID AND YRC.ItemMasterID = KPY.ItemMasterID
	                Group by KPG.GroupID, KPY.ItemMasterID, CM.ConceptID, CM.BookingID,
	                CM.ConceptNo, IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, IM.Segment4ValueID, 
	                IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID, IM.Segment8ValueID, ISV1.SegmentValue, 
	                ISV2.SegmentValue, ISV3.SegmentValue, ISV4.SegmentValue, 
	                ISV5.SegmentValue, ISV6.SegmentValue, ISV7.SegmentValue,
	                IM.Segment8ValueID,KPY.YarnLotNo,KPY.YarnBrandID, KPY.BatchNo, C.ShortName, 
	                KPY.PhysicalCount,CM.GroupConceptNo, CM.ConceptDate,KPY.KPYarnID, YRC.ShadeCode, KPYC.FCMRChildID, KPY.YDItem, KPM.RevisionNo
                )
                Select L.GroupID, L.ItemMasterID, L.ConceptID, L.BookingID, L.YDItem, PreProcessRevNo = L.RevisionNo,
	                L.ConceptNo, L.Segment1ValueID, L.Segment2ValueID, L.Segment3ValueID, L.Segment4ValueID, 
	                L.Segment5ValueID, L.Segment6ValueID, L.Segment7ValueID, L.Segment8ValueID, L.Segment1ValueDesc, 
	                L.Segment2ValueDesc, L.Segment3ValueDesc, L.Segment4ValueDesc, 
	                L.Segment5ValueDesc, L.Segment6ValueDesc, L.Segment7ValueDesc,
	                L.Segment8ValueDesc, L.YarnLotNo, L.YarnBrandID, L.BatchNo, L.YarnBrand, 
	                L.PhysicalCount, L.GroupConceptNo, L.ConceptDate, L.KPYarnID, L.ShadeCode,MRC.YarnCategory,  
	                Sum(MRC.ReqQty) YarnReqQty,isnull(A.ReqQty,0) UsedQty, MRC.ReqQty-isnull(A.ReqQty,0) PendingQty, 
	                MRC.ReqQty-isnull(A.ReqQty,0) ReqQty,CEILING( MRC.ReqQty-isnull(A.ReqQty,0) )MaxReqQty, Max(MRC.ReqCone) ReqCone,MRC.FCMRChildID,
	                YSM.YarnStockSetId, YSM.SampleStockQty, YSM.AdvanceStockQty, 
                    Spinner = CASE WHEN KPY.YDItem = 1 THEN SP1.ShortName ELSE SP.ShortName END
                From L
                Inner JOIN {TableNames.Knitting_Plan_Yarn_Child} KPYC ON KPYC.KPYarnID=L.KPYarnID AND KPYC.ConceptID=L.ConceptID
	            LEFT join {TableNames.Knitting_Plan_Yarn} KPY ON KPY.KPYarnID=KPYC.KPYarnID
                LEFT JOIN {TableNames.RND_FREE_CONCEPT_MR_CHILD} MRC On MRC.FCMRChildID = KPYC.FCMRChildID
                LEFT JOIN {TableNames.YarnStockMaster} YSM ON YSM.YarnStockSetId = KPY.YarnStockSetId
                LEFT JOIN {TableNames.YarnStockSet} YSS ON YSS.YarnStockSetId = KPY.YarnStockSetId
                LEFT JOIN {DbNames.EPYSL}..Contacts SP ON SP.ContactId = YSS.SpinnerId
                LEFT JOIN {DbNames.EPYSL}..Contacts SP1 ON SP1.ContactId = KPY.YarnBrandID
                LEFT JOIN A on A.KPYarnID = L.KPYarnID AND A.ItemMasterID = L.ItemMasterID AND A.ConceptID=L.ConceptID
                where ISNULL(A.ReqQty,0) < ISNULL(MRC.ReqQty,0)
                Group By L.GroupID, L.ItemMasterID, L.ConceptID, L.BookingID, L.YDItem, L.RevisionNo,
	                L.ConceptNo, L.Segment1ValueID, L.Segment2ValueID, L.Segment3ValueID, L.Segment4ValueID, 
	                L.Segment5ValueID, L.Segment6ValueID, L.Segment7ValueID, L.Segment8ValueID, L.Segment1ValueDesc, 
	                L.Segment2ValueDesc, L.Segment3ValueDesc, L.Segment4ValueDesc, 
	                L.Segment5ValueDesc, L.Segment6ValueDesc, L.Segment7ValueDesc,
	                L.Segment8ValueDesc, L.YarnLotNo, L.YarnBrandID, L.BatchNo, L.YarnBrand, 
	                L.PhysicalCount, L.GroupConceptNo, L.ConceptDate, L.KPYarnID, L.ShadeCode,MRC.YarnCategory,
                    A.ReqQty,MRC.ReqQty,MRC.FCMRChildID,YSM.YarnStockSetId,YSM.SampleStockQty, YSM.AdvanceStockQty,CASE WHEN KPY.YDItem = 1 THEN SP1.ShortName ELSE SP.ShortName END;
               
                    ----Company
                    {CommonQueries.GetCompany()};

                    -- Spinner / Brand
                    {CommonQueries.GetYarnSpinners()}

                    ----StockTypes
                    {CommonQueries.GetStockTypes("3,5")}";
            }
            else
            {
                query =
                $@"
                WITH M As 
                (
                    Select RnDReqMasterID, RnDReqDate, RnDReqNo, RCompanyID, OCompanyID, IsApprove, IsAcknowledge, 
                    IsReqForYD ,'Revision' As Status 
                    from {TableNames.YARN_RnD_REQ_MASTER} MMM
                    Where MMM.RnDReqMasterID 
					In (
						Select YRC.RnDReqMasterID
                        from {TableNames.Knitting_Plan_Yarn} KPY
						Inner join {TableNames.YARN_RnD_REQ_CHILD} YRC ON YRC.KPYarnID = KPY.KPYarnID AND YRC.ItemMasterID = KPY.ItemMasterID
                        Where KPY.GroupID IN ({entity[0].GroupIDs})
						GROUP BY YRC.RnDReqMasterID
					)
                )
                Select * from M;

                -- Child Data
                ;WITH YRC As 
                (
                    Select YM.RnDReqMasterID, FCC.FCMRChildID As RnDReqChildID, FCC.FCMRChildID, FCM.FCMRMasterID As YarnPRMasterID,FCM.ConceptID, FC.BookingID,
                    FCC.ItemMasterID,YPC.FCMRMasterID, YPC.BatchNo,YPC.YarnLotNo,YPC.YarnBrandID,YPC.PhysicalCount,
                    YM.FloorID, YPC.Remarks, (Case When Isnull(YPC.ReqQty, 0) = 0 Then FCC.ReqQty Else YPC.ReqQty End)ReqQty,
                    (Case When Isnull(YPC.ReqCone, 0) = 0 Then FCC.ReqCone Else YPC.ReqCone End)ReqCone, YPC.ShadeCode, 
                    FCC.UnitID, 'Kg' AS DisplayUnitDesc,KPY.KPYarnID
                    from {TableNames.FreeConceptMRMaster} FCM 
                    Inner JOIN {TableNames.RND_FREE_CONCEPT_MR_CHILD} FCC On FCM.FCMRMasterID = FCC.FCMRMasterID
					Left join {TableNames.Knitting_Plan_Yarn} KPY On KPY.FCMRChildID= FCC.FCMRChildID
                    Left join {TableNames.YARN_RnD_REQ_CHILD} YPC ON YPC.KPYarnID = KPY.KPYarnID AND YPC.ItemMasterID = KPY.ItemMasterID
                    inner join {TableNames.YARN_RnD_REQ_MASTER} YM ON YM.RnDReqMasterID = YPC.RnDReqMasterID
                    LEFT join {TableNames.RND_FREE_CONCEPT_MASTER} FC ON FC.ConceptID = FCM.ConceptID
                    Where KPY.GroupID IN ({entity[0].GroupIDs})
                ),
				A as(
				select YRC.KPYarnID,YRC.ItemMasterID,YRC.ConceptID,sum(YRC.ReqQty)ReqQty from {TableNames.YARN_RnD_REQ_CHILD} YRC
				GROUP BY YRC.KPYarnID,YRC.ItemMasterID,YRC.ConceptID
				)
                Select YRC.RnDReqChildID, YRC.FCMRChildID FCMRChildID, YRC.FCMRMasterID, YRC.ItemMasterID,YRC.ConceptID, YRC.BookingID,
                CM.ConceptNo, YRC.ShadeCode, YRC.BatchNo, IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, 
                IM.Segment4ValueID, IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID, IM.Segment8ValueID, 
                ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc,
                ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, 
                ISV7.SegmentValue Segment7ValueDesc, YRC.Remarks,YRC.YarnLotNo,YRC.YarnBrandID, C.ShortName YarnBrand,MRC.ReqCone,
                UN.DisplayUnitDesc, YRC.PhysicalCount, MRC.ShadeCode, FCMR.FloorID, KU.ShortName FloorName, CM.GroupConceptNo, CM.ConceptDate
				,Sum(MRC.ReqQty) YarnReqQty,isnull(A.ReqQty,0) UsedQty, MRC.ReqQty-isnull(A.ReqQty,0) PendingQty, MRC.ReqQty-isnull(A.ReqQty,0) ReqQty,CEILING( MRC.ReqQty-isnull(A.ReqQty,0) )MaxReqQty
                From YRC
                INNER join {TableNames.YARN_RnD_REQ_MASTER} FCMR ON YRC.RnDReqMasterID = FCMR.RnDReqMasterID
                INNER join {TableNames.RND_FREE_CONCEPT_MASTER} CM ON YRC.ConceptID = CM.ConceptID
				Inner JOIN {TableNames.Knitting_Plan_Yarn_Child} KPYC ON KPYC.KPYarnID=YRC.KPYarnID AND KPYC.ConceptID=YRC.ConceptID --AND KPYC.ItemMasterID=YRC.ItemMasterID--On KPYC.FCMRChildID = L.FCMRChildID
                LEFT JOIN {TableNames.RND_FREE_CONCEPT_MR_CHILD} MRC On MRC.FCMRChildID = KPYC.FCMRChildID
				LEFT JOIN A on  A.KPYarnID = YRC.KPYarnID AND A.ItemMasterID = YRC.ItemMasterID AND A.ConceptID=YRC.ConceptID
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YRC.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
				LEFT JOIN {DbNames.EPYSL}..Unit UN ON YRC.UnitID=UN.UnitID
                LEFT JOIN {DbNames.EPYSL}..Contacts C ON YRC.YarnBrandID = C.ContactID
                LEFT join {TableNames.KNITTING_UNIT} KU ON YRC.FloorID = KU.KnittingUnitID
				GROUP BY YRC.RnDReqChildID, YRC.FCMRChildID, YRC.FCMRMasterID, YRC.ItemMasterID,YRC.ConceptID, YRC.BookingID,
                CM.ConceptNo, YRC.ShadeCode, YRC.BatchNo, IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, 
                IM.Segment4ValueID, IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID, IM.Segment8ValueID, 
                ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue,
                ISV4.SegmentValue, ISV5.SegmentValue, ISV6.SegmentValue, 
                ISV7.SegmentValue, YRC.Remarks,YRC.YarnLotNo,YRC.YarnBrandID, C.ShortName,MRC.ReqCone,
                UN.DisplayUnitDesc, YRC.PhysicalCount, MRC.ShadeCode, FCMR.FloorID, KU.ShortName, CM.GroupConceptNo, CM.ConceptDate
				,MRC.ReqQty,isnull(A.ReqQty,0), MRC.ReqQty-isnull(A.ReqQty,0), MRC.ReqQty-isnull(A.ReqQty,0),CEILING( MRC.ReqQty-isnull(A.ReqQty,0) )

                -- Company
                {CommonQueries.GetCompany()};

                -- Spinner / Brand
                {CommonQueries.GetYarnSpinners()};

                ----StockTypes
                {CommonQueries.GetStockTypes("3,5")}";

            }

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                YarnRnDReqMaster data = await records.ReadFirstOrDefaultAsync<YarnRnDReqMaster>();
                Guard.Against.NullObject(data);

                data.Childs = records.Read<YarnRnDReqChild>().ToList();
                data.RCompanyList = records.Read<Select2OptionModel>().ToList();
                data.YarnBrandList = records.Read<Select2OptionModel>().ToList();
                data.StockTypeList.Add(new Select2OptionModel()
                {
                    id = 0.ToString(),
                    text = "Select Stock Type"
                });
                data.StockTypeList.AddRange(records.Read<Select2OptionModel>().ToList());

                if (data.Childs.Count() > 0)
                {
                    string groupConceptNos = string.Join(",", data.Childs.Where(x => x.GroupConceptNo.Trim() != "").Select(x => x.GroupConceptNo).Distinct());
                    string[] groupConceptNoList = groupConceptNos.Split(',');
                    foreach (string groupConceptNo in groupConceptNoList)
                    {
                        var concept = data.Childs.First(x => x.GroupConceptNo == groupConceptNo);
                        data.FreeConceptMR.Add(new FreeConceptMRMaster()
                        {
                            FCMRMasterID = concept.FCMRMasterID,
                            ConceptNo = groupConceptNo,
                            ConceptDate = concept.ConceptDate
                        });
                    }
                }
                data.Childs.ForEach(c =>
                {
                    c.StockTypeId = EnumStockType.SampleStock;
                    c.StockQty = c.SampleStockQty;
                });
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
        public async Task<YarnRnDReqMaster> GetNewWithoutKnittingInfo()
        {
            var query = $@"
                    --Company List
                    Select Cast (CE.CompanyID as varchar) [id] , CE.ShortName [text]
                    FROM {DbNames.EPYSL}..CompanyEntity CE 
                    WHERE CE.BusinessNature IN ('TEX','PC') ORDER BY CE.ShortName;

                    -- Brand
                    {CommonQueries.GetYarnSpinners()};

                    --Booking No
                    SELECT id = MIN(BookingID), text = BookingNo 
                    from {TableNames.FBBOOKING_ACKNOWLEDGE} 
                    WHERE ISNUMERIC(SUBSTRING(LTRIM(BookingNo), 1, 1)) = 1 
                    GROUP BY BookingNo
                    ORDER BY BookingNo DESC";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                YarnRnDReqMaster data = new YarnRnDReqMaster();
                Guard.Against.NullObject(data);
                data.RCompanyList = records.Read<Select2OptionModel>().ToList();
                data.YarnBrandList = records.Read<Select2OptionModel>().ToList();
                data.BookingList = records.Read<Select2OptionModel>().ToList();
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
        public async Task<YarnRnDReqMaster> GetAsync(int id, int flag, string requisitionType = null)
        {
            var query = "";
            if (requisitionType == "RnD" || requisitionType == "null" || String.IsNullOrEmpty(requisitionType))
            {
                query = $@"
                -- Master Data
                Select (case when {flag} = 1 Then '0' Else   MMM.RnDReqMasterID End)RnDReqMasterID, MMM.RnDReqDate, --RnDReqNo,
                (case when {flag} = 1 Then '<--New-->' Else RnDReqNo End) RnDReqNo,
                ParentRnDReqNo = RnDReqNo,
                MMM.RCompanyID, COM.ShortName RCompanyName, MMM.OCompanyID, MMM.IsApprove, MMM.IsAcknowledge, MMM.IsReqForYD,
                MMM.IsWOKnittingInfo,MMM.IsAdditional
                from {TableNames.YARN_RnD_REQ_MASTER} MMM
                LEFT JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID = MMM.RCompanyID
                Where MMM.RnDReqMasterID = {id};

                --Childs
                WITH 
                A as(
	                select KPYarnID,ConceptID,ItemMasterID,isnull(sum(FCC.ReqQty),0) UsedQty 
	                from {TableNames.YARN_RnD_REQ_CHILD} FCC 
	                INNER join {TableNames.YARN_RnD_REQ_MASTER} M ON M.RnDReqMasterID = FCC.RnDReqMasterID
	                Where M.IsAdditional=0
	                GROUP BY KPYarnID,ConceptID,ItemMasterID
                ),
                B as(
	                Select FCC.RnDReqChildID, FCC.FCMRChildID
	                ,MRM.FCMRMasterID, FCC.ItemMasterID, FCC.KPYarnID, FCC.PreProcessRevNo,FCC.ConceptID,
	                ConceptNo = CASE WHEN ISNULL(FCC.ConceptID,0) > 0 THEN CM.ConceptNo ELSE SBM.BookingNo END, MRC.ShadeCode, FCC.BatchNo,
	                IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, IM.Segment4ValueID, IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID,
	                IM.Segment8ValueID, ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc,
	                ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc,
	                FCC.Remarks,FCC.YarnLotNo,CM.GroupConceptNo, (case when {flag} = 1 Then 0 Else  FCC.ReqCone End)ReqCone,UN.DisplayUnitDesc, FCC.PhysicalCount, FCMR.FloorID, KU.ShortName FloorName,
	                Sum(MRC.ReqQty) YarnReqQty,isnull(sum(FCC.ReqQty),0) ReqQty, 
	    
		            Spinner = CASE WHEN ISNULL(KPY.YDItem,0) = 1 THEN 
		                                                         CASE WHEN ISNULL(KPY.YarnBrandID,0) > 0 THEN SP2.ShortName
													                  WHEN ISNULL(FCC.YarnBrandID,0) > 0 THEN SP1.ShortName
													                  ELSE '' END
		                           ELSE SP.ShortName END,
	    
		            YarnBrand = CASE WHEN ISNULL(KPY.YDItem,0) = 1 THEN 
		                                                         CASE WHEN ISNULL(KPY.YarnBrandID,0) > 0 THEN SP2.ShortName
													                  WHEN ISNULL(FCC.YarnBrandID,0) > 0 THEN SP1.ShortName
													                  ELSE '' END
		                           ELSE SP.ShortName END,

		            YarnBrandID = CASE WHEN ISNULL(KPY.YDItem,0) = 1 THEN 
		                                                         CASE WHEN ISNULL(KPY.YarnBrandID,0) > 0 THEN ISNULL(KPY.YarnBrandID,0)
													                  WHEN ISNULL(FCC.YarnBrandID,0) > 0 THEN ISNULL(FCC.YarnBrandID,0)
													                  ELSE 0 END
		                           ELSE YSS.SpinnerId END,

	                YSM.YarnStockSetId,YSM.SampleStockQty, YSM.AdvanceStockQty,
	                A.UsedQty,  case when Sum(MRC.ReqQty)-isnull(A.UsedQty,0) < 0 then 0 else Sum(MRC.ReqQty)-isnull(A.UsedQty,0)end PendingQty,CEILING(Sum(MRC.ReqQty))MaxReqQty,
                    FCC.YarnCategory, FCC.StockTypeId,
                    StockQty = CASE WHEN FCC.StockTypeId = {EnumStockType.AdvanceStock} THEN YSM.AdvanceStockQty ELSE YSM.SampleStockQty END,
                    StockTypeName = ST.Name, KPY.YDItem
	                from {TableNames.YARN_RnD_REQ_CHILD} FCC
	                INNER join {TableNames.YARN_RnD_REQ_MASTER} FCMR ON FCMR.RnDReqMasterID = FCC.RnDReqMasterID
	                LEFT JOIN {TableNames.RND_FREE_CONCEPT_MR_CHILD} MRC On MRC.FCMRChildID = FCC.FCMRChildID
	                LEFT join {TableNames.FreeConceptMRMaster} MRM On MRM.FCMRMasterID = MRC.FCMRMasterID
	                --INNER JOIN {TableNames.Knitting_Plan_Yarn_Child} KPYC ON KPYC.KPYarnID = FCC.KPYarnID AND KPYC.ConceptID = FCC.ConceptID
                    LEFT join {TableNames.Knitting_Plan_Yarn} KPY ON KPY.KPYarnID=FCC.KPYarnID
                    LEFT JOIN {TableNames.YarnStockMaster} YSM ON YSM.YarnStockSetId = KPY.YarnStockSetId
                    LEFT JOIN {TableNames.YarnStockSet} YSS ON YSS.YarnStockSetId = KPY.YarnStockSetId
                    LEFT JOIN {TableNames.StockType} ST ON ST.StockTypeId = FCC.StockTypeId
	                LEFT JOIN A ON A.KPYarnID = FCC.KPYarnID AND A.ConceptID = FCC.ConceptID and a.KPYarnID!=0
	                LEFT join {TableNames.RND_FREE_CONCEPT_MASTER} CM ON FCC.ConceptID = CM.ConceptID
	                LEFT JOIN {DbNames.EPYSL}..Contacts SP ON SP.ContactID = YSS.SpinnerId AND YSS.SpinnerId > 0
	                LEFT JOIN {DbNames.EPYSL}..Contacts SP1 ON SP1.ContactID = FCC.YarnBrandID AND FCC.YarnBrandID > 0
		            LEFT JOIN {DbNames.EPYSL}..Contacts SP2 ON SP2.ContactID = KPY.YarnBrandID AND KPY.YarnBrandID > 0
	                LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster SBM ON SBM.BookingID = FCC.BookingID
	                LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = MRC.ItemMasterID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
	                LEFT JOIN {DbNames.EPYSL}..Unit UN ON FCC.UnitID=UN.UnitID
	                LEFT join {TableNames.KNITTING_UNIT} KU ON FCMR.FloorID = KU.KnittingUnitID
	                Where FCMR.RnDReqMasterID = {id} 
                    GROUP BY FCC.RnDReqChildID, FCC.FCMRChildID
                    ,MRM.FCMRMasterID, FCC.ItemMasterID, FCC.KPYarnID, FCC.PreProcessRevNo,FCC.ConceptID,
                    CASE WHEN ISNULL(FCC.ConceptID,0) > 0 THEN CM.ConceptNo ELSE SBM.BookingNo END, MRC.ShadeCode, FCC.BatchNo,
                    IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, IM.Segment4ValueID, IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID,
                    IM.Segment8ValueID, ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue,
                    ISV4.SegmentValue, ISV5.SegmentValue, ISV6.SegmentValue, ISV7.SegmentValue,YSM.YarnStockSetId,YSM.SampleStockQty, YSM.AdvanceStockQty,
                    FCC.Remarks,FCC.YarnLotNo,FCC.YarnBrandID, CM.GroupConceptNo,(case when {flag} = 1 Then 0 Else  FCC.ReqCone End),UN.DisplayUnitDesc, FCC.PhysicalCount, FCC.ShadeCode, FCMR.FloorID, KU.ShortName,
                    --KPYC.ReqQty,
                    FCC.ReqQty,A.UsedQty,FCC.YarnCategory, FCC.StockTypeId,ST.Name, KPY.YDItem,
		            CASE WHEN ISNULL(KPY.YDItem,0) = 1 THEN 
		            CASE WHEN ISNULL(KPY.YarnBrandID,0) > 0 THEN SP2.ShortName
		            WHEN ISNULL(FCC.YarnBrandID,0) > 0 THEN SP1.ShortName
		            ELSE '' END
		            ELSE SP.ShortName END,
		            CASE WHEN ISNULL(KPY.YDItem,0) = 1 THEN 
		            CASE WHEN ISNULL(KPY.YarnBrandID,0) > 0 THEN ISNULL(KPY.YarnBrandID,0)
		            WHEN ISNULL(FCC.YarnBrandID,0) > 0 THEN ISNULL(FCC.YarnBrandID,0)
		            ELSE 0 END
		            ELSE YSS.SpinnerId END
                )
                select * from B where ReqQty <> 0



                -- Company
                Select Cast (CE.CompanyID as varchar) [id] , CE.ShortName [text]
                    FROM {DbNames.EPYSL}..CompanyEntity CE 
                    WHERE CE.BusinessNature IN ('TEX','PC') ORDER BY CE.ShortName;

               -- Brand
                {CommonQueries.GetYarnSpinners()};
                -- Free Concpet MR List
                Select distinct YRDC.FCMRMasterID,F.ValueName ConceptForName,FCM.ConceptNo,FCM.ConceptDate, Count(*) Over() TotalRows
                from {TableNames.YARN_RnD_REQ_CHILD} AS  YRDC
                INNER join {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID =YRDC.ConceptID
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue F ON F.ValueID = FCM.ConceptFor
                WHERE YRDC.RnDReqMasterID = {id}

                --Booking No
                SELECT id = MIN(BookingID), text = BookingNo 
                from {TableNames.FBBOOKING_ACKNOWLEDGE} 
                WHERE ISNUMERIC(SUBSTRING(LTRIM(BookingNo), 1, 1)) = 1 
                GROUP BY BookingNo
                ORDER BY BookingNo DESC

                 ----StockTypes
                {CommonQueries.GetStockTypes("3,5")}";

                //  query = $@"
                //  -- Master Data
                //  Select (case when {flag} = 1 Then '0' Else   MMM.RnDReqMasterID End)RnDReqMasterID, MMM.RnDReqDate, --RnDReqNo,
                //  (case when {flag} = 1 Then '<--New-->' Else RnDReqNo End) RnDReqNo,
                //  ParentRnDReqNo = RnDReqNo,
                //  MMM.RCompanyID, COM.ShortName RCompanyName, MMM.OCompanyID, MMM.IsApprove, MMM.IsAcknowledge, MMM.IsReqForYD,
                //  MMM.IsWOKnittingInfo,MMM.IsAdditional
                //  from {TableNames.YARN_RnD_REQ_MASTER} MMM
                //  LEFT JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID = MMM.RCompanyID
                //  Where MMM.RnDReqMasterID = {id};

                //  --Childs
                //  WITH 
                //  A as(
                //   select KPYarnID,ConceptID,ItemMasterID,isnull(sum(FCC.ReqQty),0) UsedQty 
                //   from {TableNames.YARN_RnD_REQ_CHILD} FCC 
                //   INNER join {TableNames.YARN_RnD_REQ_MASTER} M ON M.RnDReqMasterID = FCC.RnDReqMasterID
                //   Where M.IsAdditional=0
                //   GROUP BY KPYarnID,ConceptID,ItemMasterID
                //  ),
                //  B as(
                //   Select FCC.RnDReqChildID, FCC.FCMRChildID
                //   ,MRM.FCMRMasterID, FCC.ItemMasterID, FCC.KPYarnID, FCC.PreProcessRevNo,FCC.ConceptID,
                //   ConceptNo = CASE WHEN ISNULL(FCC.ConceptID,0) > 0 THEN CM.ConceptNo ELSE SBM.BookingNo END, MRC.ShadeCode, FCC.BatchNo,
                //   IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, IM.Segment4ValueID, IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID,
                //   IM.Segment8ValueID, ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc,
                //   ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc,
                //   FCC.Remarks,FCC.YarnLotNo,CM.GroupConceptNo, (case when {flag} = 1 Then 0 Else  FCC.ReqCone End)ReqCone,UN.DisplayUnitDesc, FCC.PhysicalCount, FCMR.FloorID, KU.ShortName FloorName,
                //   Sum(MRC.ReqQty) YarnReqQty,isnull(sum(FCC.ReqQty),0) ReqQty, 

                //Spinner = CASE WHEN ISNULL(KPY.YDItem,0) = 1 THEN 
                //                                             CASE WHEN ISNULL(KPY.YarnBrandID,0) > 0 THEN SP2.ShortName
                //                 WHEN ISNULL(FCC.YarnBrandID,0) > 0 THEN SP1.ShortName
                //                 ELSE '' END
                //               ELSE SP.ShortName END,

                //YarnBrand = CASE WHEN ISNULL(KPY.YDItem,0) = 1 THEN 
                //                                             CASE WHEN ISNULL(KPY.YarnBrandID,0) > 0 THEN SP2.ShortName
                //                 WHEN ISNULL(FCC.YarnBrandID,0) > 0 THEN SP1.ShortName
                //                 ELSE '' END
                //               ELSE SP.ShortName END,

                //YarnBrandID = CASE WHEN ISNULL(KPY.YDItem,0) = 1 THEN 
                //                                             CASE WHEN ISNULL(KPY.YarnBrandID,0) > 0 THEN ISNULL(KPY.YarnBrandID,0)
                //                 WHEN ISNULL(FCC.YarnBrandID,0) > 0 THEN ISNULL(FCC.YarnBrandID,0)
                //                 ELSE 0 END
                //               ELSE YSS.SpinnerId END,

                //   YSM.YarnStockSetId,YSM.SampleStockQty, YSM.AdvanceStockQty,
                //   A.UsedQty,  case when Sum(MRC.ReqQty)-isnull(A.UsedQty,0) < 0 then 0 else Sum(MRC.ReqQty)-isnull(A.UsedQty,0)end PendingQty,CEILING(Sum(MRC.ReqQty))MaxReqQty,
                //      FCC.YarnCategory, FCC.StockTypeId,
                //      StockQty = CASE WHEN FCC.StockTypeId = {EnumStockType.AdvanceStock} THEN YSM.AdvanceStockQty ELSE YSM.SampleStockQty END,
                //      StockTypeName = ST.Name, KPY.YDItem
                //   from {TableNames.YARN_RnD_REQ_CHILD} FCC
                //   INNER join {TableNames.YARN_RnD_REQ_MASTER} FCMR ON FCMR.RnDReqMasterID = FCC.RnDReqMasterID
                //   INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_CHILD} MRC On MRC.FCMRChildID = FCC.FCMRChildID
                //   INNER join {TableNames.FreeConceptMRMaster} MRM On MRM.FCMRMasterID = MRC.FCMRMasterID
                //   --INNER JOIN {TableNames.Knitting_Plan_Yarn_Child} KPYC ON KPYC.KPYarnID = FCC.KPYarnID AND KPYC.ConceptID = FCC.ConceptID
                //      LEFT join {TableNames.Knitting_Plan_Yarn} KPY ON KPY.KPYarnID=FCC.KPYarnID
                //      LEFT JOIN {TableNames.YarnStockMaster} YSM ON YSM.YarnStockSetId = KPY.YarnStockSetId
                //      LEFT JOIN {TableNames.YarnStockSet} YSS ON YSS.YarnStockSetId = KPY.YarnStockSetId
                //      LEFT JOIN {TableNames.StockType} ST ON ST.StockTypeId = FCC.StockTypeId
                //   LEFT JOIN A ON A.KPYarnID = FCC.KPYarnID AND A.ConceptID = FCC.ConceptID
                //   LEFT join {TableNames.RND_FREE_CONCEPT_MASTER} CM ON FCC.ConceptID = CM.ConceptID
                //   LEFT JOIN {DbNames.EPYSL}..Contacts SP ON SP.ContactID = YSS.SpinnerId AND YSS.SpinnerId > 0
                //   LEFT JOIN {DbNames.EPYSL}..Contacts SP1 ON SP1.ContactID = FCC.YarnBrandID AND FCC.YarnBrandID > 0
                //LEFT JOIN {DbNames.EPYSL}..Contacts SP2 ON SP2.ContactID = KPY.YarnBrandID AND KPY.YarnBrandID > 0
                //   LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster SBM ON SBM.BookingID = FCC.BookingID
                //   INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = MRC.ItemMasterID
                //   LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                //   LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                //   LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                //   LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                //   LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                //   LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                //   LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                //   LEFT JOIN {DbNames.EPYSL}..Unit UN ON FCC.UnitID=UN.UnitID
                //   LEFT join {TableNames.KNITTING_UNIT} KU ON FCMR.FloorID = KU.KnittingUnitID
                //   Where FCMR.RnDReqMasterID = {id} 
                //      GROUP BY FCC.RnDReqChildID, FCC.FCMRChildID
                //      ,MRM.FCMRMasterID, FCC.ItemMasterID, FCC.KPYarnID, FCC.PreProcessRevNo,FCC.ConceptID,
                //      CASE WHEN ISNULL(FCC.ConceptID,0) > 0 THEN CM.ConceptNo ELSE SBM.BookingNo END, MRC.ShadeCode, FCC.BatchNo,
                //      IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, IM.Segment4ValueID, IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID,
                //      IM.Segment8ValueID, ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue,
                //      ISV4.SegmentValue, ISV5.SegmentValue, ISV6.SegmentValue, ISV7.SegmentValue,YSM.YarnStockSetId,YSM.SampleStockQty, YSM.AdvanceStockQty,
                //      FCC.Remarks,FCC.YarnLotNo,FCC.YarnBrandID, CM.GroupConceptNo,(case when {flag} = 1 Then 0 Else  FCC.ReqCone End),UN.DisplayUnitDesc, FCC.PhysicalCount, FCC.ShadeCode, FCMR.FloorID, KU.ShortName,
                //      --KPYC.ReqQty,
                //      FCC.ReqQty,A.UsedQty,FCC.YarnCategory, FCC.StockTypeId,ST.Name, KPY.YDItem,
                //CASE WHEN ISNULL(KPY.YDItem,0) = 1 THEN 
                //CASE WHEN ISNULL(KPY.YarnBrandID,0) > 0 THEN SP2.ShortName
                //WHEN ISNULL(FCC.YarnBrandID,0) > 0 THEN SP1.ShortName
                //ELSE '' END
                //ELSE SP.ShortName END,
                //CASE WHEN ISNULL(KPY.YDItem,0) = 1 THEN 
                //CASE WHEN ISNULL(KPY.YarnBrandID,0) > 0 THEN ISNULL(KPY.YarnBrandID,0)
                //WHEN ISNULL(FCC.YarnBrandID,0) > 0 THEN ISNULL(FCC.YarnBrandID,0)
                //ELSE 0 END
                //ELSE YSS.SpinnerId END
                //  )
                //  select * from B where ReqQty <> 0



                //  -- Company
                //  Select Cast (CE.CompanyID as varchar) [id] , CE.ShortName [text]
                //      FROM {DbNames.EPYSL}..CompanyEntity CE 
                //      WHERE CE.BusinessNature IN ('TEX','PC') ORDER BY CE.ShortName;

                // -- Brand
                //  {CommonQueries.GetYarnSpinners()};
                //  -- Free Concpet MR List
                //  Select distinct YRDC.FCMRMasterID,F.ValueName ConceptForName,FCM.ConceptNo,FCM.ConceptDate, Count(*) Over() TotalRows
                //  from {TableNames.YARN_RnD_REQ_CHILD} AS  YRDC
                //  INNER join {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID =YRDC.ConceptID
                //  LEFT JOIN {DbNames.EPYSL}..EntityTypeValue F ON F.ValueID = FCM.ConceptFor
                //  WHERE YRDC.RnDReqMasterID = {id}

                //  --Booking No
                //  SELECT id = MIN(BookingID), text = BookingNo 
                //  from {TableNames.FBBOOKING_ACKNOWLEDGE} 
                //  WHERE ISNUMERIC(SUBSTRING(LTRIM(BookingNo), 1, 1)) = 1 
                //  GROUP BY BookingNo
                //  ORDER BY BookingNo DESC

                //   ----StockTypes
                //  {CommonQueries.GetStockTypes("3,5")}";
            }
            else if (requisitionType == "Bulk")
            {
                query = $@"
                 -- Master Data
                Select (case when {flag} = 1 Then '0' Else   MMM.KYReqMasterID End)RnDReqMasterID, MMM.KYReqDate RnDReqDate, --RnDReqNo,
                (case when {flag} = 1 Then '<--New-->' Else KYReqNo End) RnDReqNo,
                ParentRnDReqNo = KYReqNo,
                MMM.RCompanyID, COM.ShortName RCompanyName, MMM.OCompanyID, MMM.Approve IsApprove, MMM.Acknowledge IsAcknowledge, MMM.IsReqForYD,
                MMM.IsWOKnittingInfo,MMM.IsAdditional
                FROM {TableNames.KY_Req_Master} MMM
                LEFT JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID = MMM.RCompanyID
                Where MMM.KYReqMasterID = {id};

				--Childs
                WITH A as(
		            select KPYarnID,ConceptID,ItemMasterID,isnull(sum(FCC.ReqQty),0) UsedQty from {TableNames.KY_Req_Child} FCC 
		            INNER JOIN {TableNames.KY_Req_Master} M ON M.KYReqMasterID = FCC.KYReqMasterID
		            Where M.IsAdditional=0
		            GROUP BY KPYarnID,ConceptID,ItemMasterID
	            ),
	            B as(
		            Select FCC.KYReqChildID RnDReqChildID, FCC.FCMRChildID FCMRChildID, FCC.FCMRMasterID, FCC.ItemMasterID, FCC.KPYarnID, FCC.PreProcessRevNo,FCC.ConceptID,
		            ConceptNo = CASE WHEN ISNULL(FCC.ConceptID,0) > 0 THEN CM.ConceptNo ELSE SBM.BookingNo END, FCC.ShadeCode, FCC.BatchNo, FCC.YarnCategory,
		            IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, IM.Segment4ValueID, IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID,
		            IM.Segment8ValueID, ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc,
		            ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc,
		            FCC.Remarks,FCC.YarnLotNo,FCC.YarnBrandID, CM.GroupConceptNo, C.ShortName YarnBrand,(case when {flag} = 1 Then 0 Else  FCC.ReqCone End)ReqCone,UN.DisplayUnitDesc, FCC.PhysicalCount, FCMR.FloorID, KU.ShortName FloorName,
		            YarnReqQty = cast(ROUND((Sum(CI.NetYarnReqQty)/100) * ((KPC.BookingQty/CASE WHEN CM.IsBDS = 2 THEN ISNULL(FBC.GreyProdQty,0) ELSE CM.ProduceKnittingQty END)*100),2)as numeric(36,2)), 
					isnull(sum(FCC.ReqQty),0) ReqQty, FCC.ReqQty UsedQty, Spinner = C.ShortName,  
					PendingQty = case when cast(ROUND((Sum(CI.NetYarnReqQty)/100) * ((KPC.BookingQty/CASE WHEN CM.IsBDS = 2 THEN ISNULL(FBC.GreyProdQty,0) ELSE CM.ProduceKnittingQty END)*100),2)as numeric(36,2))-isnull(FCC.ReqQty,0)<0 
					then 0 
					else cast(ROUND((Sum(CI.NetYarnReqQty)/100) * ((KPC.BookingQty/CASE WHEN CM.IsBDS = 2 THEN ISNULL(FBC.GreyProdQty,0) ELSE CM.ProduceKnittingQty END)*100),2)as numeric(36,2))-isnull(FCC.ReqQty,0)
					end,
					MaxReqQty = CEILING(cast(ROUND((Sum(CI.NetYarnReqQty)/100) * ((KPC.BookingQty/CASE WHEN CM.IsBDS = 2 THEN ISNULL(FBC.GreyProdQty,0) ELSE CM.ProduceKnittingQty END)*100),2)as numeric(36,2))), KPY.YDItem
		            from {TableNames.KY_Req_Child} FCC
		            LEFT JOIN A ON A.KPYarnID=FCC.KPYarnID AND A.ConceptID=FCC.ConceptID AND A.ItemMasterID=FCC.ItemMasterID 
		            Inner JOIN {TableNames.Knitting_Plan_Yarn_Child} KPYC ON FCC.KPYarnID=KPYC.KPYarnID AND FCC.ConceptID=KPYC.ConceptID --AND FCC.ItemMasterID=KPYC.ItemMasterID
		            Inner join {TableNames.Knitting_Plan_Yarn} KPY ON KPY.KPYarnID = KPYC.KPYarnID
					LEFT JOIN {TableNames.RND_FREE_CONCEPT_MR_CHILD} MRC On MRC.FCMRChildID = KPYC.FCMRChildID
					Inner join {TableNames.Knitting_Plan_Master} KPM ON KPM.PlanNo = KPY.GroupID
					LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPMasterID = KPM.KPMasterID
		            LEFT JOIN {TableNames.YarnBookingChildItem_New} CI On CI.YBChildItemID = MRC.YBChildItemID
		            INNER JOIN {TableNames.KY_Req_Master} FCMR ON FCC.KYReqMasterID = FCMR.KYReqMasterID
		            LEFT join {TableNames.RND_FREE_CONCEPT_MASTER} CM ON FCC.ConceptID = CM.ConceptID
					LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FBC ON FBC.BookingChildID = CM.BookingChildID AND FBC.BookingID = CM.BookingID
		            LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster SBM ON SBM.BookingID = FCC.BookingID
		            INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = MRC.ItemMasterID
		            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
		            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
		            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
		            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
		            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
		            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
		            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
		            LEFT JOIN {DbNames.EPYSL}..Unit UN ON FCC.UnitID=UN.UnitID
		            LEFT JOIN {DbNames.EPYSL}..Contacts C ON FCC.YarnBrandID = C.ContactID
		            LEFT join {TableNames.KNITTING_UNIT} KU ON FCMR.FloorID = KU.KnittingUnitID
		            Where FCC.KYReqMasterID = {id} 
		            GROUP BY FCC.KYReqChildID, FCC.FCMRChildID , FCC.FCMRMasterID, FCC.ItemMasterID, FCC.KPYarnID, FCC.PreProcessRevNo,FCC.ConceptID,
		            CASE WHEN ISNULL(FCC.ConceptID,0) > 0 THEN CM.ConceptNo ELSE SBM.BookingNo END, FCC.ShadeCode, FCC.BatchNo, FCC.YarnCategory,
		            IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, IM.Segment4ValueID, IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID,
		            IM.Segment8ValueID, ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue,
		            ISV4.SegmentValue, ISV5.SegmentValue, ISV6.SegmentValue, ISV7.SegmentValue,
		            FCC.Remarks,FCC.YarnLotNo,FCC.YarnBrandID, CM.GroupConceptNo, C.ShortName,(case when {flag} = 1 Then 0 Else  FCC.ReqCone End),UN.DisplayUnitDesc, FCC.PhysicalCount, FCC.ShadeCode, FCMR.FloorID, KU.ShortName,
		            KPYC.ReqQty,FCC.ReqQty,A.UsedQty, KPC.BookingQty, FBC.GreyProdQty, CM.ProduceKnittingQty, CM.IsBDS, KPY.YDItem
	            )
	            select * from B where ReqQty<>0

                -- Company
                Select Cast (CE.CompanyID as varchar) [id] , CE.ShortName [text]
                FROM {DbNames.EPYSL}..CompanyEntity CE 
                WHERE CE.BusinessNature IN ('TEX','PC') ORDER BY CE.ShortName;

               -- Brand
                {CommonQueries.GetYarnSpinners()};
                -- Free Concpet MR List
                Select distinct YRDC.FCMRMasterID,F.ValueName ConceptForName,FCM.ConceptNo,FCM.ConceptDate, Count(*) Over() TotalRows
                from {TableNames.KY_Req_Child} AS  YRDC
                INNER join {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID =YRDC.ConceptID
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue F ON F.ValueID = FCM.ConceptFor
                WHERE YRDC.KYReqMasterID = {id}

                --Booking No
                SELECT id = MIN(BookingID), text = BookingNo 
                from {TableNames.FBBOOKING_ACKNOWLEDGE} 
                WHERE ISNUMERIC(SUBSTRING(LTRIM(BookingNo), 1, 1)) = 1 
                GROUP BY BookingNo
                ORDER BY BookingNo DESC

                 ----StockTypes
                {CommonQueries.GetStockTypes("3,5")}";
            }

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                YarnRnDReqMaster data = await records.ReadFirstOrDefaultAsync<YarnRnDReqMaster>();
                Guard.Against.NullObject(data);

                data.RequisitionType = requisitionType;
                data.Childs = records.Read<YarnRnDReqChild>().ToList();
                data.RCompanyList = records.Read<Select2OptionModel>().ToList();
                data.YarnBrandList = records.Read<Select2OptionModel>().ToList();
                data.FreeConceptMR = records.Read<FreeConceptMRMaster>().ToList();
                data.BookingList = records.Read<Select2OptionModel>().ToList();

                data.StockTypeList.Add(new Select2OptionModel()
                {
                    id = 0.ToString(),
                    text = "Select Stock Type"
                });
                data.StockTypeList.AddRange(records.Read<Select2OptionModel>().ToList());

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
        public async Task<YarnRnDReqMaster> GetAsyncGroupBy(int id, int flag, string requisitionType = null)
        {
            var query = "";
            if (requisitionType == null || requisitionType == "RnD")
            {
                query =
                $@"
                -- Master Data
                Select (case when {flag} = 1 Then '0' Else   MMM.RnDReqMasterID End)RnDReqMasterID, MMM.RnDReqDate, --RnDReqNo,
                (case when {flag} = 1 Then '<--New-->' Else RnDReqNo End) RnDReqNo,
                ParentRnDReqNo = RnDReqNo,
                MMM.RCompanyID, COM.ShortName RCompanyName, MMM.OCompanyID, MMM.IsApprove, MMM.IsAcknowledge, MMM.IsReqForYD,
                MMM.IsWOKnittingInfo
                from {TableNames.YARN_RnD_REQ_MASTER} MMM
                LEFT JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID = MMM.RCompanyID
                Where MMM.RnDReqMasterID = {id};

				-- Childs
                Select FCC.FCMRChildID FCMRChildID, FCC.FCMRMasterID, FCC.ItemMasterID, FCC.PreProcessRevNo, sum((case when {flag} = 1 Then 0 Else  FCC.ReqQty End))ReqQty,
                FCC.ShadeCode, FCC.BatchNo, ConceptNo = CM.GroupConceptNo,
                IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, IM.Segment4ValueID, IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID,
                IM.Segment8ValueID, ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc,
                ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc,
                FCC.Remarks,FCC.YarnLotNo,FCC.YarnBrandID, CM.GroupConceptNo, C.ShortName YarnBrand,sum((case when {flag} = 1 Then 0 Else  FCC.ReqCone End))ReqCone,UN.DisplayUnitDesc, FCC.PhysicalCount, MRC.ShadeCode, FCMR.FloorID, KU.ShortName FloorName,
                FCC.YarnCategory
                from {TableNames.YARN_RnD_REQ_CHILD} FCC
                INNER join {TableNames.YARN_RnD_REQ_MASTER} FCMR ON FCC.RnDReqMasterID = FCMR.RnDReqMasterID
				INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_CHILD} MRC On MRC.FCMRChildID = FCC.FCMRChildID
                LEFT join {TableNames.RND_FREE_CONCEPT_MASTER} CM ON FCC.ConceptID = CM.ConceptID
                LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster SBM ON SBM.BookingID = FCC.BookingID
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = FCC.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                LEFT JOIN {DbNames.EPYSL}..Unit UN ON FCC.UnitID=UN.UnitID
                LEFT JOIN {DbNames.EPYSL}..Contacts C ON FCC.YarnBrandID = C.ContactID
                LEFT join {TableNames.KNITTING_UNIT} KU ON FCMR.FloorID = KU.KnittingUnitID
                Where FCC.RnDReqMasterID = {id}
                GROUP BY FCC.FCMRChildID, FCC.FCMRMasterID, FCC.ItemMasterID, FCC.PreProcessRevNo,
                FCC.ShadeCode, FCC.BatchNo, CM.GroupConceptNo,
                IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, IM.Segment4ValueID, IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID,
                IM.Segment8ValueID, ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue,
                ISV4.SegmentValue, ISV5.SegmentValue, ISV6.SegmentValue, ISV7.SegmentValue,
                FCC.Remarks,FCC.YarnLotNo,FCC.YarnBrandID, CM.GroupConceptNo, C.ShortName,UN.DisplayUnitDesc, FCC.PhysicalCount, MRC.ShadeCode, FCMR.FloorID, KU.ShortName,
                FCC.YarnCategory;

                -- Company
                Select Cast (CE.CompanyID as varchar) [id] , CE.ShortName [text]
                    FROM {DbNames.EPYSL}..CompanyEntity CE 
                    WHERE CE.BusinessNature IN ('TEX','PC') ORDER BY CE.ShortName;

               -- Brand
                {CommonQueries.GetYarnSpinners()};

                -- Free Concpet MR List
                Select distinct YRDC.FCMRMasterID,F.ValueName ConceptForName,FCM.ConceptNo,FCM.ConceptDate, Count(*) Over() TotalRows
                from {TableNames.YARN_RnD_REQ_CHILD} AS  YRDC
                INNER join {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID =YRDC.ConceptID
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue F ON F.ValueID = FCM.ConceptFor
                WHERE YRDC.RnDReqMasterID = {id}

                --Booking No
                SELECT id = MIN(BookingID), text = BookingNo 
                from {TableNames.FBBOOKING_ACKNOWLEDGE} 
                WHERE ISNUMERIC(SUBSTRING(LTRIM(BookingNo), 1, 1)) = 1 
                GROUP BY BookingNo
                ORDER BY BookingNo DESC";
            }
            else if (requisitionType == "Bulk")
            {
                query =
                    $@"
                -- Master Data
                Select (case when {flag} = 1 Then '0' Else   MMM.KYReqMasterID End)RnDReqMasterID, MMM.KYReqDate RnDReqDate, --RnDReqNo,
                (case when {flag} = 1 Then '<--New-->' Else KYReqNo End) RnDReqNo,
                ParentRnDReqNo = KYReqNo,
                MMM.RCompanyID, COM.ShortName RCompanyName, MMM.OCompanyID, MMM.Approve IsApprove, MMM.Acknowledge IsAcknowledge, MMM.IsReqForYD,
                MMM.IsWOKnittingInfo
                FROM {TableNames.KY_Req_Master} MMM
                LEFT JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID = MMM.RCompanyID
                Where MMM.KYReqMasterID = {id};

				-- Childs
                Select FCC.FCMRChildID FCMRChildID, FCC.FCMRMasterID, FCC.ItemMasterID, FCC.PreProcessRevNo, sum((case when {flag} = 1 Then 0 Else  FCC.ReqQty End))ReqQty,
                FCC.ShadeCode, FCC.BatchNo, ConceptNo = CM.GroupConceptNo,
                IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, IM.Segment4ValueID, IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID,
                IM.Segment8ValueID, ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc,
                ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc,
                FCC.Remarks,FCC.YarnLotNo,FCC.YarnBrandID, CM.GroupConceptNo, C.ShortName YarnBrand,sum((case when {flag} = 1 Then 0 Else  FCC.ReqCone End))ReqCone,UN.DisplayUnitDesc, FCC.PhysicalCount, FCC.ShadeCode, FCMR.FloorID, KU.ShortName FloorName
                ,SUM(CI.NetYarnReqQty) YarnReqQty
                from {TableNames.KY_Req_Child} FCC
                INNER JOIN {TableNames.KY_Req_Master} FCMR ON FCC.KYReqMasterID = FCMR.KYReqMasterID
                LEFT JOIN {TableNames.RND_FREE_CONCEPT_MR_CHILD} MRC ON MRC.FCMRChildID = FCC.FCMRChildID
                LEFT JOIN {TableNames.YarnBookingChildItem_New} CI ON Ci.YBChildItemID = MRC.YBChildItemID
                LEFT join {TableNames.RND_FREE_CONCEPT_MASTER} CM ON FCC.ConceptID = CM.ConceptID
                LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster SBM ON SBM.BookingID = FCC.BookingID
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = FCC.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                LEFT JOIN {DbNames.EPYSL}..Unit UN ON FCC.UnitID=UN.UnitID
                LEFT JOIN {DbNames.EPYSL}..Contacts C ON FCC.YarnBrandID = C.ContactID
                LEFT join {TableNames.KNITTING_UNIT} KU ON FCMR.FloorID = KU.KnittingUnitID
                Where FCC.KYReqMasterID = {id}
                GROUP BY FCC.FCMRChildID, FCC.FCMRMasterID, FCC.ItemMasterID, FCC.PreProcessRevNo,
                FCC.ShadeCode, FCC.BatchNo, CM.GroupConceptNo,
                IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, IM.Segment4ValueID, IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID,
                IM.Segment8ValueID, ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue,
                ISV4.SegmentValue, ISV5.SegmentValue, ISV6.SegmentValue, ISV7.SegmentValue,
                FCC.Remarks,FCC.YarnLotNo,FCC.YarnBrandID, CM.GroupConceptNo, C.ShortName,UN.DisplayUnitDesc, FCC.PhysicalCount, FCC.ShadeCode, FCMR.FloorID, KU.ShortName;

                -- Company
                Select Cast (CE.CompanyID as varchar) [id] , CE.ShortName [text]
                    FROM {DbNames.EPYSL}..CompanyEntity CE 
                    WHERE CE.BusinessNature IN ('TEX','PC') ORDER BY CE.ShortName;

               -- Brand
                {CommonQueries.GetYarnSpinners()};
                -- Free Concpet MR List
                Select distinct YRDC.FCMRMasterID,F.ValueName ConceptForName,FCM.ConceptNo,FCM.ConceptDate, Count(*) Over() TotalRows
                from {TableNames.KY_Req_Child} AS  YRDC
                INNER join {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID =YRDC.ConceptID
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue F ON F.ValueID = FCM.ConceptFor
                WHERE YRDC.KYReqMasterID = {id}

                --Booking No
                SELECT id = MIN(BookingID), text = BookingNo 
                from {TableNames.FBBOOKING_ACKNOWLEDGE} 
                WHERE ISNUMERIC(SUBSTRING(LTRIM(BookingNo), 1, 1)) = 1 
                GROUP BY BookingNo
                ORDER BY BookingNo DESC";
            }
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                YarnRnDReqMaster data = await records.ReadFirstOrDefaultAsync<YarnRnDReqMaster>();
                Guard.Against.NullObject(data);

                data.Childs = records.Read<YarnRnDReqChild>().ToList();
                data.RCompanyList = records.Read<Select2OptionModel>().ToList();
                data.YarnBrandList = records.Read<Select2OptionModel>().ToList();
                data.FreeConceptMR = records.Read<FreeConceptMRMaster>().ToList();
                data.BookingList = records.Read<Select2OptionModel>().ToList();
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
        public async Task<YarnRnDReqMaster> GetReviseAsync(int id, int flag, string mrId)
        {
            var query =
                $@"
                -- Master Data
                Select MMM.RnDReqMasterID, MMM.RnDReqDate, MMM.RnDReqNo, MMM.RCompanyID, COM.ShortName RCompanyName, 
                MMM.OCompanyID, MMM.IsApprove, MMM.IsAcknowledge, MMM.IsReqForYD
                from {TableNames.YARN_RnD_REQ_MASTER} MMM 
                LEFT JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID = MMM.RCompanyID
                Where MMM.RnDReqMasterID = {id};

                -- Childs
                WITH
                AB AS
                (
	                Select C.RnDReqChildID, C.FCMRChildID, C.ConceptID, C.StockTypeId, C.KPYarnID, M.IsReqForYD,M.IsAdditional
	                from {TableNames.YARN_RnD_REQ_CHILD} C
	                INNER join {TableNames.YARN_RnD_REQ_MASTER} M ON M.RnDReqMasterID = C.RnDReqMasterID
	                Where C.RnDReqMasterID IN ({id})
                ), 
				BlockQty AS(
				Select AB.RnDReqChildID , BlockQty = BlockSampleStockQty + BlockAdvanceStockQty FROM AB
				INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_CHILD} FCMRC ON FCMRC.FCMRChildID = AB.FCMRChildID
				INNER JOIN {TableNames.YarnStockChild} YSC ON YSC.YarnStockSetId = FCMRC.YarnStockSetId AND YSC.StockFromTableId = {EnumStockFromTable.YarnRnDReqChild} AND YSC.StockFromPKId = AB.RnDReqChildID 
				),
                Con As(
	                Select ConceptID, IsReqForYD
	                From AB
	                Group By ConceptID,IsReqForYD
                ),
                A as(
	                select YRC.KPYarnID,YRC.ItemMasterID,YRC.ConceptID,sum(YRC.ReqQty)ReqQty from {TableNames.YARN_RnD_REQ_CHILD} YRC
	                INNER join {TableNames.YARN_RnD_REQ_MASTER} M ON M.RnDReqMasterID = YRC.RnDReqMasterID
	                Where M.IsAdditional=0
	                GROUP BY YRC.KPYarnID,YRC.ItemMasterID,YRC.ConceptID
                ),
                FCMR As (
	                Select YC.KPYarnID, MRC.ItemMasterID, YC.ConceptID, YD = c.YDItem, b.RevisionNo, 
	                ReqCone = MAX(MRC.ReqCone),Sum(MRC.ReqQty) YarnReqQty,isnull(A.ReqQty,0) UsedQty,case when (Sum(MRC.ReqQty)-isnull(A.ReqQty,0))<0 then 0 else Sum(MRC.ReqQty)-isnull(A.ReqQty,0)end PendingQty,
	                FCC.ReqQty ReqQty,CEILING(Sum(MRC.ReqQty)) MaxReqQty,
	                StockTypeId = ISNULL(AB.StockTypeId,0), SampleStockQty = ISNULL(YSM.SampleStockQty,0), AdvanceStockQty = ISNULL(YSM.AdvanceStockQty,0),
	                StockQty = CASE WHEN ISNULL(AB.StockTypeId,0) = 3 THEN ISNULL(YSM.AdvanceStockQty,0) ELSE ISNULL(YSM.SampleStockQty,0) END
	                --YSS.YarnCategory, YSS.ShadeCode, YarnBrandID = YSS.SpinnerId

                    ,YarnCategory = CASE WHEN ISNULL(MRC.YDItem,0) = 1 THEN MRC.YarnCategory ELSE YSS.YarnCategory END
                    ,ShadeCode = CASE WHEN ISNULL(MRC.YDItem,0) = 1 THEN MRC.ShadeCode ELSE YSS.ShadeCode END
                    ,YarnBrandID = CASE WHEN ISNULL(C.YDItem,0) = 1 THEN 
		                                    CASE WHEN ISNULL(C.YarnBrandID,0) > 0 THEN ISNULL(C.YarnBrandID,0)
							                    WHEN ISNULL(FCC.YarnBrandID,0) > 0 THEN ISNULL(FCC.YarnBrandID,0)
							                    ELSE 0 END
                                    ELSE YSS.SpinnerId END

                    from {TableNames.Knitting_Plan_Yarn_Child} YC
	                inner Join Con On Con.ConceptID = YC.ConceptID
	                Inner join {TableNames.Knitting_Plan_Master} b on b.ConceptID = YC.ConceptID
	                Inner join {TableNames.Knitting_Plan_Yarn} c on c.KPYarnID = YC.KPYarnID AND ISNULL(c.YDItem,0) = ISNULL(Con.IsReqForYD,0)
	                INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_CHILD} MRC ON MRC.FCMRChildID = YC.FCMRChildID
	                LEFT JOIN {TableNames.YarnStockMaster} YSM ON YSM.YarnStockSetId = c.YarnStockSetId
	                LEFT JOIN {TableNames.YarnStockSet} YSS ON YSS.YarnStockSetId = c.YarnStockSetId
	                LEFT JOIN AB ON AB.KPYarnID = YC.KPYarnID
	                LEFT join {TableNames.YARN_RnD_REQ_CHILD} FCC ON FCC.RnDReqChildID= AB.RnDReqChildID
	                LEFT JOIN A on  A.KPYarnID = YC.KPYarnID AND A.ItemMasterID = YC.ItemMasterID AND A.ConceptID=YC.ConceptID  
	                GROUP BY AB.IsAdditional,YC.KPYarnID, MRC.ItemMasterID, YC.ConceptID, c.YDItem, b.RevisionNo,isnull(A.ReqQty,0),MRC.ReqQty,FCC.ReqQty,
	                ISNULL(AB.StockTypeId,0), ISNULL(YSM.SampleStockQty,0), ISNULL(YSM.AdvanceStockQty,0),--YSS.YarnCategory, YSS.ShadeCode, YSS.SpinnerId
                    
	                CASE WHEN ISNULL(MRC.YDItem,0) = 1 THEN MRC.YarnCategory ELSE YSS.YarnCategory END,
	                CASE WHEN ISNULL(MRC.YDItem,0) = 1 THEN MRC.ShadeCode ELSE YSS.ShadeCode END,
	                CASE WHEN ISNULL(C.YDItem,0) = 1 THEN 
	                CASE WHEN ISNULL(C.YarnBrandID,0) > 0 THEN ISNULL(C.YarnBrandID,0)
	                WHEN ISNULL(FCC.YarnBrandID,0) > 0 THEN ISNULL(FCC.YarnBrandID,0)
	                ELSE 0 END
	                ELSE YSS.SpinnerId END
                )
                Select RnDReqChildID=ISNULL(AB.RnDReqChildID,0), FCMR.ItemMasterID, FCMR.YD, FCMR.ReqQty, FCMR.ReqCone, FCMR.ConceptID,
                CM.ConceptNo, IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, IM.Segment4ValueID, 
                IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID, IM.Segment8ValueID, ISV1.SegmentValue Segment1ValueDesc, 
                ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc, 
                ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc,
                IM.Segment8ValueID Segment8ValueDesc,KPY.YarnLotNo,FCMR.YarnBrandID, Spinner = C.ShortName, YarnBrand = C.ShortName,
                KPY.PhysicalCount,KPY.KPYarnID,KPY.BatchNo, PreProcessRevNo = FCMR.RevisionNo,FCMR.YarnReqQty,FCMR.UsedQty,FCMR.PendingQty,FCMR.MaxReqQty,
                FCMR.StockTypeId, 
                AdvanceStockQty = Case When FCMR.StockTypeId = {EnumStockType.AdvanceStock} Then FCMR.AdvanceStockQty + ISNULL(BQ.BlockQty,0) Else FCMR.AdvanceStockQty END, 
				SampleStockQty = Case When FCMR.StockTypeId = {EnumStockType.SampleStock} Then FCMR.SampleStockQty + ISNULL(BQ.BlockQty,0) Else FCMR.SampleStockQty END, 
				StockQty = FCMR.StockQty + ISNULL(BQ.BlockQty,0),  
                FCMR.YarnCategory,FCMR.ShadeCode
                From FCMR
                LEFT JOIN AB ON AB.KPYarnID = FCMR.KPYarnID
				LEFT JOIN BlockQty BQ ON BQ.RnDReqChildID = AB.RnDReqChildID
                LEFT join {TableNames.Knitting_Plan_Yarn} KPY ON KPY.KPYarnID = FCMR.KPYarnID
                Left join {TableNames.RND_FREE_CONCEPT_MASTER} CM On CM.ConceptID = FCMR.ConceptID
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = FCMR.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                Left Join {DbNames.EPYSL}..Contacts C ON C.ContactID = FCMR.YarnBrandID
                Group by ISNULL(AB.RnDReqChildID,0), FCMR.ItemMasterID, FCMR.YD, FCMR.ReqQty, FCMR.ReqCone, FCMR.ConceptID,
                CM.ConceptNo, IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, IM.Segment4ValueID, 
                IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID, IM.Segment8ValueID, ISV1.SegmentValue, 
                ISV2.SegmentValue, ISV3.SegmentValue, ISV4.SegmentValue,
                ISV5.SegmentValue, ISV6.SegmentValue, ISV7.SegmentValue,
                IM.Segment8ValueID,KPY.YarnLotNo,FCMR.YarnBrandID, C.ShortName, 
                KPY.PhysicalCount,KPY.KPYarnID,KPY.BatchNo,FCMR.RevisionNo,FCMR.YarnReqQty,FCMR.UsedQty,FCMR.PendingQty,FCMR.MaxReqQty
                ,FCMR.StockTypeId, FCMR.AdvanceStockQty, FCMR.SampleStockQty, FCMR.StockQty, FCMR.YarnCategory,FCMR.ShadeCode,BQ.BlockQty;

                -- Company
                {CommonQueries.GetCompany()};

               -- Brand
                {CommonQueries.GetYarnSpinners()};

                -- Free Concpet MR List
                Select distinct YRDC.FCMRMasterID,F.ValueName ConceptForName,FCM.ConceptNo,FCM.ConceptDate, Count(*) Over() TotalRows
                from {TableNames.YARN_RnD_REQ_CHILD} AS  YRDC
                INNER join {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID =YRDC.ConceptID
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue F ON F.ValueID = FCM.ConceptFor
                WHERE YRDC.RnDReqMasterID = {id};

                 ----StockTypes
                {CommonQueries.GetStockTypes("3,5")}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                YarnRnDReqMaster data = await records.ReadFirstOrDefaultAsync<YarnRnDReqMaster>();
                Guard.Against.NullObject(data);

                data.Childs = records.Read<YarnRnDReqChild>().ToList();
                data.RCompanyList = records.Read<Select2OptionModel>().ToList();
                data.YarnBrandList = records.Read<Select2OptionModel>().ToList();
                data.FreeConceptMR = records.Read<FreeConceptMRMaster>().ToList();
                data.StockTypeList.Add(new Select2OptionModel()
                {
                    id = 0.ToString(),
                    text = "Select Stock Type"
                });
                data.StockTypeList.AddRange(records.Read<Select2OptionModel>().ToList());
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
        public async Task<YarnRnDReqMaster> GetDetailsAsync(int id)
        {
            var query =
                $@"
                Select * from {TableNames.YARN_RnD_REQ_MASTER} Where RnDReqMasterID = {id}

                Select * from {TableNames.YARN_RnD_REQ_CHILD} Where RnDReqMasterID = {id}

                Select * FROM {TableNames.YarnRNDReqBuyerTeam} Where RnDReqMasterID = {id}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                YarnRnDReqMaster data = await records.ReadFirstOrDefaultAsync<YarnRnDReqMaster>();
                Guard.Against.NullObject(data);

                data.Childs = records.Read<YarnRnDReqChild>().ToList();
                data.YarnRnDReqBuyerTeams = records.Read<YarnRnDReqBuyerTeam>().ToList();
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
        public async Task<YarnRnDReqMaster> GetDetailsForReviseAsync(int id)
        {
            var query =
                $@"
                Select TOP(1)*,PreProcessRevNo=KP.RevisionNo 
                from {TableNames.YARN_RnD_REQ_MASTER} M
				LEFT join {TableNames.YARN_RnD_REQ_CHILD} C ON C.RnDReqMasterID = M.RnDReqMasterID
				LEFT join {TableNames.Knitting_Plan_Master} KP ON KP.ConceptID = C.ConceptID
				Where M.RnDReqMasterID = {id}

                Select * from {TableNames.YARN_RnD_REQ_CHILD} Where RnDReqMasterID = {id}

                Select * FROM {TableNames.YarnRNDReqBuyerTeam} Where RnDReqMasterID = {id}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                YarnRnDReqMaster data = await records.ReadFirstOrDefaultAsync<YarnRnDReqMaster>();
                Guard.Against.NullObject(data);

                data.Childs = records.Read<YarnRnDReqChild>().ToList();
                data.YarnRnDReqBuyerTeams = records.Read<YarnRnDReqBuyerTeam>().ToList();
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
        public async Task SaveAsync(YarnRnDReqMaster entity, int userId)
        {
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
                        entity.RnDReqMasterID = await _service.GetMaxIdAsync(TableNames.YARN_RnD_REQ_MASTER, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

                        if (!entity.IsAdditional)
                        {
                            entity.RnDReqNo = await _service.GetMaxNoAsync(TableNames.YARN_RnD_REQ_NO,entity.CompanyId,RepeatAfterEnum.NoRepeat,"0000000", transactionGmt, _connectionGmt);
                        }
                        else
                        {
                            if (entity.ParentRnDReqNo.IsNotNullOrEmpty())
                            {
                                string parentRnDReqNo = entity.ParentRnDReqNo;
                                parentRnDReqNo = parentRnDReqNo.Split('-')[0];
                                int maxCount = await _service.GetMaxIdAsync(TableNames.YARN_RnD_REQ_MASTER, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                                entity.RnDReqNo = parentRnDReqNo + "-Add-" + maxCount;
                            }
                        }

                        maxChildId = await _service.GetMaxIdAsync(TableNames.YARN_RnD_REQ_CHILD, entity.Childs.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        foreach (var item in entity.Childs)
                        {
                            item.RnDReqChildID = maxChildId++;
                            item.RnDReqMasterID = entity.RnDReqMasterID;
                        }
                        break;

                    case EntityState.Modified:
                        var addedChilds = entity.Childs.FindAll(x => x.EntityState == EntityState.Added);
                        maxChildId = await _service.GetMaxIdAsync(TableNames.YARN_RnD_REQ_CHILD, addedChilds.Count,RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        foreach (var item in addedChilds)
                        {
                            item.RnDReqChildID = maxChildId++;
                            item.RnDReqMasterID = entity.RnDReqMasterID;
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

                var childs = entity.Childs.Where(x => x.ItemMasterID == 0).ToList();
                if (childs.Count() > 0)
                {
                    throw new Exception("Item missing => SaveAsync => YarnRnDReqService");
                }

                await _service.SaveSingleAsync(entity, transaction);
                await _service.SaveAsync(entity.Childs, transaction);
                #region pendingwork
                //foreach (YarnRnDReqChild item in entity.Childs.Where(x => x.EntityState == EntityState.Added || x.EntityState == EntityState.Modified))
                //{

                //    await _connection.ExecuteAsync(SPNames.sp_Validation_YarnRnDReqChild, new { PrimaryKeyId = item.RnDReqChildID, SecondParamValue = item.KPYarnID, ThirdParamValue = item.ItemMasterID, UserId = userId, EntityState = item.EntityState }, transaction, 30, CommandType.StoredProcedure);
                //}
                
                //if (entity.IsApprove && entity.IsAcknowledge == false)
                //{
                //    if (entity.ApproveBy.IsNull()) entity.ApproveBy = 0;
                //    userId = entity.EntityState == EntityState.Added ? entity.AddedBy : (int)entity.ApproveBy;
                //    await _connection.ExecuteAsync(SPNames.spYarnStockOperation , new { MasterID = entity.RnDReqMasterID, FromMenuType = EnumFromMenuType.RnDYarnRequisitionApp, UserId = userId }, transaction, 30, CommandType.StoredProcedure);
                //}

                #endregion pendingwork
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
                transaction.Dispose();
                transactionGmt.Dispose();
                _connectionGmt.Close();
                _connection.Close();
            }
        }

        public async Task ReviseAsync(YarnRnDReqMaster entity, int userId)
        {
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                await _connection.ExecuteAsync(SPNames.spBackupYarnRndRequisition_Full, new { RnDReqMasterID = entity.RnDReqMasterID }, transaction, 30, CommandType.StoredProcedure);

                int maxChildId = 0;
                switch (entity.EntityState)
                {
                    case EntityState.Added:
                        entity.RnDReqMasterID = await _service.GetMaxIdAsync(TableNames.YARN_RnD_REQ_MASTER);
                        entity.RnDReqNo =await _service.GetMaxNoAsync(TableNames.YARN_RnD_REQ_NO);

                        maxChildId = await _service.GetMaxIdAsync(TableNames.YARN_RnD_REQ_CHILD, entity.Childs.Count);
                        foreach (var item in entity.Childs)
                        {
                            item.RnDReqChildID = maxChildId++;
                            item.RnDReqMasterID = entity.RnDReqMasterID;
                        }
                        break;

                    case EntityState.Modified:
                        var addedChilds = entity.Childs.FindAll(x => x.EntityState == EntityState.Added);
                        maxChildId = await _service.GetMaxIdAsync(TableNames.YARN_RnD_REQ_CHILD, addedChilds.Count);
                        foreach (var item in addedChilds)
                        {
                            item.RnDReqChildID = maxChildId++;
                            item.RnDReqMasterID = entity.RnDReqMasterID;
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

                var childs = entity.Childs.Where(x => x.ItemMasterID == 0).ToList();
                if (childs.Count() > 0)
                {
                    throw new Exception("Item missing => ReviseAsync => YarnRnDReqService");
                }


                await _service.SaveSingleAsync(entity, transaction);
                await _service.SaveAsync(entity.Childs, transaction);
                foreach (YarnRnDReqChild item in entity.Childs.Where(x => x.EntityState == EntityState.Added || x.EntityState == EntityState.Modified))
                {
                    //await _service.ValidationSingleAsync(item, transaction, SPNames.sp_Validation_YarnRnDReqChild, item.EntityState, userId, item.RnDReqChildID, item.KPYarnID, item.ItemMasterID);
                    await _connection.ExecuteAsync(SPNames.sp_Validation_YarnRnDReqChild, new { PrimaryKeyId = item.RnDReqChildID, SecondParamValue = item.KPYarnID, ThirdParamValue = item.ItemMasterID, UserId = userId, EntityState = item.EntityState }, transaction, 30, CommandType.StoredProcedure);

                }

                //if (entity.ApproveBy.IsNull()) entity.ApproveBy = 0;
                //int userId = entity.EntityState == EntityState.Added ? entity.AddedBy : (int)entity.ApproveBy;
                //await _connection.ExecuteAsync("spYarnStockOperation", new { MasterID = entity.RnDReqMasterID, FromMenuType = EnumFromMenuType.RnDYarnRequisitionApp, UserId = userId }, transaction, 30, CommandType.StoredProcedure);

                transaction.Commit();
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                if (ex.Message.Contains('~')) throw new Exception(ex.Message.Split('~')[0]);
                throw ex;
            }
            finally
            {
                transaction.Dispose();
                _connection.Close();
            }
        }
        private async Task<int> GetMaxRnDReqNo(string rnDReqNo)
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["TexConnection"].ConnectionString;
            var queryString = $"SELECT MaxValue=COUNT(*) from {TableNames.YARN_RnD_REQ_MASTER} WHERE RnDReqNo LIKE '{rnDReqNo}%'";

            int maxNo = 0;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(queryString, connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                try
                {
                    while (reader.Read())
                    {
                        maxNo = Convert.ToInt32(reader["MaxValue"]);
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            return maxNo;
        }

    }
}
