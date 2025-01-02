using Dapper;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using System.Data.Entity;
using Microsoft.Data.SqlClient;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Exceptions;
using System.Data;
using EPYSLTEXCore.Application.Interfaces.Knitting;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Knitting;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;

namespace EPYSLTEXCore.Application.Services.Knitting
{
    public class KYReqService: IKYReqService
    {
        private readonly IDapperCRUDService<KYReqMaster> _service;

        private SqlTransaction transaction = null;
        private readonly SqlConnection _connection;
        private readonly SqlConnection _connectionGmt;

        public KYReqService(IDapperCRUDService<KYReqMaster> service)
        {

            _service = service;

            _service.Connection = service.GetConnection(AppConstants.GMT_CONNECTION);
            _connectionGmt = service.Connection;

            _service.Connection = service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _connection = service.Connection;
        }


        public async Task<List<KYReqMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By KYReqMasterID Desc" : paginationInfo.OrderBy;
            var sql = string.Empty;
            if (status == Status.Proposed)
            {
                sql += $@"
                ;WITH
                M As 
                (
	                Select KP.KPMasterID, KP.ConceptID, KP.PreProcessRevNo, KP.RevisionPending, KP.Active, SUM(ISNULL(KPC.BookingQty,0)) PlanQty, 
                    KP.PlanNo, KP.RevisionNo, KP.BuyerID, KP.BuyerTeamID, KP.ExportOrderID, KPC.SubGroupID,
	                --Contact = CASE WHEN ISNULL(KJC.IsSubContact,0)=1 THEN C.ShortName  ELSE CC.UnitName END
					Contact = CASE WHEN CC.UnitName is not null THEN CC.UnitName ELSE C.ShortName END
	                FROM {TableNames.Knitting_Plan_Master} KP
	                LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPMasterID = KP.KPMasterID
	                LEFT JOIN {TableNames.KNITTING_JOB_CARD_Master} KJC ON KJC.GroupID = KPC.PlanNo AND KJC.ConceptID = KP.ConceptID AND KJC.GroupID NOT IN (1,0)
	                LEFT JOIN {TableNames.KNITTING_UNIT} CC ON CC.KnittingUnitID = KJC.ContactID
	                LEFT JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = KJC.ContactID
                    Where KP.Active = 1 And IsBDS = 2 AND KP.IsSubContact=0
	                GROUP BY KP.KPMasterID, KP.ConceptID, KP.PreProcessRevNo, KP.RevisionPending, KP.Active, KP.PlanNo, KP.RevisionNo, 
	                KP.BuyerID, KP.BuyerTeamID, KP.ExportOrderID, KPC.SubGroupID,ISNULL(KJC.IsSubContact,0),C.ShortName,CC.UnitName
                ),
                TotalKPDone AS
		        (
			        SELECT M.ConceptID, RemainingPlanQty = SUM(M.PlanQty)
			        FROM M
			        GROUP BY M.ConceptID
		        ),
				 A as(
                    select YRC.ConceptID,SUM(YRC.ReqQty)ReqQty 
                    FROM {TableNames.KY_Req_Child} YRC --where ConceptID=73493
                    GROUP BY YRC.ConceptID
                ),
                F AS 
                (
                    SELECT KPMasterID, M.RevisionPending, M.ConceptID, FCM.ConceptNo, FCM.ConceptDate, FCM.Qty, FCM.Remarks, 
					M.PlanQty, M.PlanNo, M.PlanNo GroupID,Status = 'New',
					Technical.TechnicalName, 
					F.ValueName ConceptForName, S.ValueName ConceptStatus, M.Active, E.EmployeeName UserName, ISG.SubGroupName, 
					M.BuyerID, M.BuyerTeamID, M.ExportOrderID, 
                    Buyer = CASE WHEN M.BuyerID > 0 THEN ISNULL(CTO.ShortName,'') ELSE 'R&D' END, 
                    BuyerTeam = CASE WHEN M.BuyerTeamID > 0 THEN ISNULL(CCT.TeamName,'') ELSE 'R&D' END,
                    FCM.GroupConceptNo, BookingQty = FCM.Qty, M.Contact, NetYarnReqQty=SUM(CI.NetYarnReqQty), A.ReqQty,
                    --ProduceKnittingQty = CASE WHEN FCM.IsBDS = 2 THEN ISNULL(FBC.GreyProdQty,0) ELSE FCM.ProduceKnittingQty END,
					ProduceKnittingQty = CASE WHEN FCM.IsBDS = 2 THEN 
							CASE WHEN FCM.SubGroupID = 1 Then FBC.GreyProdQty Else Cast (Round((FBC.BookingQty/FBC.BookingQtyKG)*FBC.GreyProdQty,0) As int) END 
						ELSE FCM.ProduceKnittingQty
						END,
                    --RemainingPlanQty = FCM.ProduceKnittingQty - ISNULL(KPD.RemainingPlanQty,0), 
                    --RemainingPlanQty = ISNULL(CASE WHEN FCM.IsBDS = 2 THEN ISNULL(FBC.GreyProdQty,0) ELSE FCM.ProduceKnittingQty END,0) - ISNULL(KPD.RemainingPlanQty,0), 
		            	RemainingPlanQty =  
						CASE WHEN FCM.IsBDS = 2 THEN 
							CASE WHEN FCM.SubGroupID = 1 Then FBC.GreyProdQty Else Cast (Round((FBC.BookingQty/FBC.BookingQtyKG)*FBC.GreyProdQty,0) As int) END 
						ELSE FCM.Qty 
						END
                        - M.PlanQty,
					Uom = CASE WHEN FCM.SubGroupId = 1 THEN 'Kg' ELSE 'Pcs' END,

					Construction = ISV1.SegmentValue, 
                    Composition = CASE WHEN M.SubGroupID = 1 THEN ISV2.SegmentValue ELSE '' END,
					ColorName = CASE WHEN M.SubGroupID = 1 THEN ISV3.SegmentValue ELSE ISV5.SegmentValue END, 
                    GSM = CASE WHEN M.SubGroupID = 1 THEN ISV4.SegmentValue ELSE '' END, 
                    DyeingType = CASE WHEN M.SubGroupID = 1 THEN ISV6.SegmentValue ELSE '' END, 
                    KnittingType = CASE WHEN M.SubGroupID = 1 THEN ISV7.SegmentValue ELSE '' END,
                    Length = CASE WHEN M.SubGroupID = 1 THEN 0 ELSE CONVERT(decimal(18,2),ISV3.SegmentValue) END,
                    Width = CASE WHEN M.SubGroupID = 1 THEN 0 ELSE CONVERT(decimal(18,2),ISV4.SegmentValue) END,
					Size = CASE WHEN M.SubGroupID <> 1 THEN ISV3.SegmentValue + ' X ' + ISV4.SegmentValue ELSE '' END

                    FROM M
					LEFT JOIN A ON A.ConceptID = M.ConceptID
                    INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = M.ConceptID
					INNER JOIN {TableNames.FreeConceptMRMaster} MR ON MR.ConceptID = M.ConceptID
                    LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FBC ON FBC.BookingChildID = FCM.BookingChildID AND FBC.BookingID = FCM.BookingID AND FBC.BookingQty <> 0
					LEFT JOIN {TableNames.YarnBookingChild_New} C ON C.BookingChildID = FBC.BookingChildID
					LEFT JOIN {TableNames.YarnBookingChildItem_New} CI ON CI.YBChildID = C.YBChildID
                    LEFT JOIN {TableNames.TotalKPDone} KPD ON KPD.ConceptID = FCM.ConceptID
                    LEFT JOIN {TableNames.FabricTechnicalName} Technical ON Technical.TechnicalNameId=FCM.TechnicalNameId
                    LEFT JOIN {DbNames.EPYSL}..EntityTypeValue F ON F.ValueID = FCM.ConceptFor
                    LEFT JOIN {DbNames.EPYSL}..EntityTypeValue S ON S.ValueID = FCM.ConceptStatusID
                    LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG On FCM.SubGroupID = ISG.SubGroupID
                    LEFT JOIN {DbNames.EPYSL}..LoginUser L ON L.UserCode = FCM.AddedBy
                    LEFT Join {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
					LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = M.BuyerID
					LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = M.BuyerTeamID

					LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = FCM.ItemMasterID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
					WHERE MR.RevisionNo = M.PreProcessRevNo AND FCM.IsBDS = 2
					GROUP BY KPMasterID, M.RevisionPending, M.ConceptID, FCM.ConceptNo, FCM.ConceptDate, FCM.Qty, FCM.Remarks, 
					M.PlanQty, M.PlanNo, Technical.TechnicalName, F.ValueName, S.ValueName, M.Active, E.EmployeeName, ISG.SubGroupName, 
					M.BuyerID, M.BuyerTeamID, M.ExportOrderID, CTO.ShortName, M.BuyerTeamID,CCT.TeamName, FCM.GroupConceptNo, FCM.Qty, 
					M.Contact, A.ReqQty,  FCM.IsBDS, FBC.BookingQty,FBC.BookingQtyKG, FBC.GreyProdQty, FCM.ProduceKnittingQty, KPD.RemainingPlanQty, FCM.SubGroupId,ISV1.SegmentValue, 
					M.SubGroupID, ISV2.SegmentValue, ISV3.SegmentValue, ISV5.SegmentValue,ISV4.SegmentValue,ISV6.SegmentValue, ISV7.SegmentValue
                   
                ),
				FinalResult As (
				Select * from F where NetYarnReqQty > ISNULL(ReqQty,0)
				)
                Select *, Count(*) Over() TotalRows  From FinalResult";
                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By ConceptID Desc" : paginationInfo.OrderBy;

            }
            else if (status == Status.Pending)
            {
                sql += $@"
                    ;WITH M AS (
	                    SELECT KYReqMasterID, KYReqDate, KYReqNo, Remarks, Approve, ApproveBy, KYReqBy,
	                    ApproveDate, Acknowledge, AcknowledgeBy, AcknowledgeDate, RCompanyID, OCompanyID, ConceptNo , 
                        (CASE WHEN IsReqForYD=1 THEN 'Yes' ELSE 'No' END) AS YDStatus,
	                    PreProcessRevNo,RevisionNo, AddedBy, BuyerID, IsWOKnittingInfo,
                        YarnReqStatus = CASE WHEN ISNULL(IsWOKnittingInfo,0) = 1 THEN 'Independent' ELSE '' END,IsAdditional
	                    From KYReqMaster
	                    WHERE Approve = 0 and Acknowledge = 0
                        AND KYReqDate > '11-APR-2020'

                    ), MRS As (
	                    SELECT M.KYReqMasterID, M.KYReqDate, M.KYReqNo, M.Remarks, M.Approve, M.ApproveBy, M.ApproveDate, SUM(C.ReqQty) Qty,
	                    M.Acknowledge, M.AcknowledgeBy, M.AcknowledgeDate, CompanyName = COM.ShortName, M.ConceptNo,M.YDStatus,M.YarnReqStatus, 
                        M.KYReqBy, E.EmployeeName KYReqByName, M.PreProcessRevNo,M.RevisionNo
                        ,Buyer = CASE WHEN KP.BuyerID > 0 THEN CT.ShortName ELSE '' END
						,M.IsAdditional,Contact = CASE WHEN ISNULL(KJC.IsSubContact,0)=1 THEN CO.ShortName  ELSE CC.UnitName END
	                    From M
	                    INNER JOIN {TableNames.KY_Req_Child} C ON C.KYReqMasterID = M.KYReqMasterID
	                    LEFT JOIN {TableNames.Knitting_Plan_Yarn} KPY ON KPY.KPYarnID=C.KPYarnID
						LEFT JOIN {TableNames.Knitting_Plan_Master} KP ON KP.KPMasterID =KPY.KPMasterID
						LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPMasterID = KP.KPMasterID
						LEFT JOIN {TableNames.KNITTING_JOB_CARD_Master} KJC ON KJC.GroupID = KPC.PlanNo AND KJC.ConceptID = KP.ConceptID AND KJC.GroupID NOT IN (1,0)
						LEFT JOIN {TableNames.KNITTING_UNIT} CC ON CC.KnittingUnitID = KJC.ContactID
					    LEFT JOIN {DbNames.EPYSL}..Contacts CO ON CO.ContactID = KJC.ContactID
	                    LEFT JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID=M.RCompanyID
                        LEFT JOIN {DbNames.EPYSL}..LoginUser L ON L.UserCode = M.AddedBy
	                    LEFT Join {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
                        LEFT JOIN {DbNames.EPYSL}..Contacts CT ON CT.ContactID = KP.BuyerID
                        WHERE M.KYReqDate > '11-APR-2020' AND ISNULL(C.PreProcessRevNo,0) = ISNULL(KP.RevisionNo,0)
	                    GROUP BY M.KYReqMasterID, M.KYReqDate, M.KYReqNo, M.Remarks, M.Approve, M.ApproveBy, M.ApproveDate,M.YarnReqStatus,
	                    M.Acknowledge, M.AcknowledgeBy, M.AcknowledgeDate,COM.ShortName, M.ConceptNo,M.YDStatus,M.KYReqBy, E.EmployeeName,
	                    M.PreProcessRevNo,M.RevisionNo,KP.BuyerID,CT.ShortName,M.IsAdditional,KJC.IsSubContact,CO.ShortName,CC.UnitName
                    ),
					FinalList AS(
					SELECT MRS.KYReqMasterID, MRS.KYReqDate, MRS.KYReqNo, MRS.Remarks, MRS.Approve, MRS.ApproveBy, MRS.ApproveDate, Qty=sum(MRS.Qty),
	                    MRS.Acknowledge, MRS.AcknowledgeBy, MRS.AcknowledgeDate, MRS.ConceptNo,MRS.YDStatus,MRS.YarnReqStatus, 
                        MRS.KYReqBy, MRS.KYReqByName, MRS.PreProcessRevNo,MRS.RevisionNo
                        ,Buyer=STRING_AGG(MRS.Buyer, ',')
						,MRS.IsAdditional,CompanyName=STRING_AGG(MRS.Contact, ',')
						FROM MRS
						GROUP BY MRS.KYReqMasterID, MRS.KYReqDate, MRS.KYReqNo, MRS.Remarks, MRS.Approve, MRS.ApproveBy, MRS.ApproveDate,
	                    MRS.Acknowledge, MRS.AcknowledgeBy, MRS.AcknowledgeDate, MRS.CompanyName, MRS.ConceptNo,MRS.YDStatus,MRS.YarnReqStatus, 
                        MRS.KYReqBy, MRS.KYReqByName, MRS.PreProcessRevNo,MRS.RevisionNo
						,MRS.IsAdditional
					)

                    Select *, TotalRows = Count(*) Over()
                    from FinalList   ";
            }
            else if (status == Status.Approved || status == Status.ProposedForAcknowledge)
            {
                sql += $@";
                    WITH M AS (
	                    SELECT KYReqMasterID, KYReqDate, KYReqNo, Remarks, Approve, ApproveBy, KYReqBy,
	                    ApproveDate, Acknowledge, AcknowledgeBy, AcknowledgeDate, RCompanyID, OCompanyID, ConceptNo, (CASE WHEN IsReqForYD=1 THEN 'Yes' ELSE 'No' END) AS YDStatus,
                        PreProcessRevNo,RevisionNo, AddedBy, BuyerID, IsWOKnittingInfo,
                        YarnReqStatus = CASE WHEN ISNULL(IsWOKnittingInfo,0) = 1 THEN 'Independent' ELSE '' END,IsAdditional
	                    From KYReqMaster
	                    WHERE Approve = 1 and Acknowledge = 0
                        AND KYReqDate > '11-APR-2020'
                    ), MRS As (
	                    SELECT M.KYReqMasterID, M.KYReqDate, M.KYReqNo, M.Remarks, M.Approve, M.ApproveBy, M.ApproveDate, SUM(C.ReqQty) Qty, M.YarnReqStatus,
	                    M.Acknowledge, M.AcknowledgeBy, M.AcknowledgeDate, CompanyName = COM.ShortName, M.ConceptNo,M.YDStatus, M.KYReqBy, E.EmployeeName KYReqByName, AE.EmployeeName KYApproveBy,
	                    M.PreProcessRevNo,M.RevisionNo
                        ,Buyer = CASE WHEN M.BuyerID > 0 THEN CT.ShortName ELSE '' END,M.IsAdditional
	                    From M
	                    INNER JOIN {TableNames.KY_Req_Child} C ON C.KYReqMasterID=M.KYReqMasterID
	                    LEFT JOIN {TableNames.Knitting_Plan_Yarn} KPY ON KPY.KPYarnID=C.KPYarnID
						LEFT JOIN {TableNames.Knitting_Plan_Master} KP ON KP.KPMasterID =KPY.KPMasterID
	                    LEFT JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID=M.RCompanyID
                        LEFT JOIN {DbNames.EPYSL}..LoginUser L ON L.UserCode = M.AddedBy
	                    LEFT Join {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
	                    LEFT Join {DbNames.EPYSL}..Employee AE ON AE.EmployeeCode = M.ApproveBy
                        LEFT JOIN {DbNames.EPYSL}..Contacts CT ON CT.ContactID = M.BuyerID
                        WHERE ISNULL(C.PreProcessRevNo,0) = ISNULL(KP.RevisionNo,0)
	                    GROUP BY M.KYReqMasterID, M.KYReqDate, M.KYReqNo, M.Remarks, M.Approve, M.ApproveBy, M.ApproveDate,M.YarnReqStatus,
	                    M.Acknowledge, M.AcknowledgeBy, M.AcknowledgeDate,COM.ShortName, M.ConceptNo,M.YDStatus,M.KYReqBy, E.EmployeeName, AE.EmployeeName,
	                    M.PreProcessRevNo,M.RevisionNo,M.BuyerID,CT.ShortName,M.IsAdditional
                    )
                    Select *, TotalRows = Count(*) Over()
                    from MRS";
            }
            else if (status == Status.Revise)
            {
                sql += $@";
                    WITH M AS 
                    (
                        SELECT M.KYReqMasterID, M.KYReqDate, M.KYReqNo, M.Remarks, M.Approve, M.ApproveBy, M.ApproveDate, M.KYReqBy, E.EmployeeName KYReqByName, SUM(C.ReqQty) Qty,
                        M.Acknowledge, M.AcknowledgeBy, M.AcknowledgeDate, CompanyName = COM.ShortName, M.ConceptNo,(CASE WHEN IsReqForYD=1 THEN 'Yes' ELSE 'No' END) AS YDStatus
                        ,FCMRMasterIDs=STRING_AGG(C.FCMRMasterID, ',')
                        ,Buyer = CASE WHEN M.BuyerID > 0 THEN CT.ShortName ELSE '' END, M.IsWOKnittingInfo,
                        YarnReqStatus = CASE WHEN ISNULL(M.IsWOKnittingInfo,0) = 1 THEN 'Independent' ELSE '' END,M.IsAdditional
                        From KYReqMaster M
                        INNER JOIN {TableNames.KY_Req_Child} C ON C.KYReqMasterID = M.KYReqMasterID
                        LEFT JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID=M.RCompanyID
                        LEFT JOIN {TableNames.Knitting_Plan_Yarn} KPY ON KPY.KPYarnID=C.KPYarnID
                        LEFT JOIN {TableNames.Knitting_Plan_Master} KP ON KP.KPMasterID = KPY.KPMasterID
                        LEFT JOIN KnittingPlanGroup KPG ON KPG.GroupID = KPY.GroupID
                        LEFT JOIN {DbNames.EPYSL}..LoginUser L ON L.UserCode = M.AddedBy
                        LEFT Join {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
                        LEFT JOIN {DbNames.EPYSL}..Contacts CT ON CT.ContactID = M.BuyerID
                        WHERE ISNULL(C.PreProcessRevNo,0) != ISNULL(KP.RevisionNo,0)
                        GROUP BY M.KYReqMasterID, M.KYReqDate, M.KYReqNo, M.Remarks, M.Approve, M.ApproveBy, M.ApproveDate, M.IsWOKnittingInfo,
                        M.Acknowledge, M.AcknowledgeBy, M.AcknowledgeDate,COM.ShortName, M.ConceptNo, IsReqForYD,M.KYReqBy, E.EmployeeName,M.BuyerID,CT.ShortName,M.IsAdditional
                    )
                    Select *, Count(*) Over() TotalRows
                    from M";
                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By KYReqMasterID Desc" : paginationInfo.OrderBy;
            }
            else if (status == Status.Draft)
            {
                sql += $@";
                    WITH M AS (
	                    SELECT YRRM.KYReqMasterID, YRRM.KYReqDate, YRRM.KYReqNo, YRRM.Remarks, YRRM.Approve, YRRM.ApproveBy, YRRM.KYReqBy,
	                    (CASE WHEN IsnUll(YRIM.KYReqMasterID, 0) != 0 THEN 'Yes' ELSE 'No' END) AS IsIssue,
	                    YRRM.ApproveDate, YRRM.Acknowledge, YRRM.AcknowledgeBy, YRRM.AcknowledgeDate, YRRM.RCompanyID, YRRM.OCompanyID, YRRM.ConceptNo,
	                    (CASE WHEN YRRM.IsReqForYD=1 THEN 'Yes' ELSE 'No' END) AS YDStatus,
	                    PreProcessRevNo,RevisionNo, YRRM.AddedBy, YRRM.BuyerID, YRRM.IsWOKnittingInfo,
                        YarnReqStatus = CASE WHEN ISNULL(YRRM.IsWOKnittingInfo,0) = 1 THEN 'Independent' ELSE '' END,IsAdditional
	                    From KYReqMaster as YRRM left join KYIssueMaster as YRIM  on YRIM.KYReqMasterID= YRRM.KYReqMasterID
	                    WHERE YRRM.Approve = 0 and YRRM.Acknowledge = 0
	                    AND YRRM.KYReqDate > '11-APR-2020'
                    ), 
                    MRS As (
	                    SELECT M.KYReqMasterID, M.KYReqDate, M.KYReqNo, M.Remarks, M.Approve, M.ApproveBy, M.ApproveDate, SUM(C.ReqQty) YarnReqQty, M.YarnReqStatus, M.IsWOKnittingInfo,
	                    M.Acknowledge, M.AcknowledgeBy, M.AcknowledgeDate, CompanyName = COM.ShortName, M.ConceptNo,M.YDStatus,M.IsIssue,M.KYReqBy, E.EmployeeName KYReqByName,
	                    M.PreProcessRevNo,M.RevisionNo
                        ,Buyer = CASE WHEN M.BuyerID > 0 THEN CT.ShortName ELSE '' END,M.IsAdditional
	                    From M
	                    INNER JOIN {TableNames.KY_Req_Child} C ON C.KYReqMasterID=M.KYReqMasterID
	                    LEFT JOIN {TableNames.Knitting_Plan_Yarn} KPY ON KPY.KPYarnID=C.KPYarnID
						LEFT JOIN {TableNames.Knitting_Plan_Master} KP ON KP.KPMasterID =KPY.KPMasterID
	                    LEFT JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID=M.RCompanyID
	                    LEFT JOIN {DbNames.EPYSL}..LoginUser L ON L.UserCode = M.AddedBy
	                    LEFT Join {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
                        LEFT JOIN {DbNames.EPYSL}..Contacts CT ON CT.ContactID = M.BuyerID
	                    WHERE ISNULL(C.PreProcessRevNo,0) = ISNULL(KP.RevisionNo,0)
	                    GROUP BY M.KYReqMasterID, M.KYReqDate, M.KYReqNo, M.Remarks, M.Approve, M.ApproveBy, M.ApproveDate,M.YarnReqStatus, M.IsWOKnittingInfo,
	                    M.Acknowledge, M.AcknowledgeBy, M.AcknowledgeDate,COM.ShortName, M.ConceptNo,M.YDStatus,M.IsIssue, M.KYReqBy, E.EmployeeName,
	                    M.PreProcessRevNo,M.RevisionNo,M.BuyerID,CT.ShortName,M.IsAdditional
                    )
                    Select *, TotalRows = Count(*) Over()
                    from MRS";
            }
            else
            {
                sql += $@";
                    WITH M AS (
	                    SELECT YRRM.KYReqMasterID, YRRM.KYReqDate, YRRM.KYReqNo, YRRM.Remarks, YRRM.Approve, YRRM.ApproveBy, YRRM.KYReqBy,
	                    (CASE WHEN IsnUll(YRIM.KYReqMasterID, 0) != 0 THEN 'Yes' ELSE 'No' END) AS IsIssue,
	                    YRRM.ApproveDate, YRRM.Acknowledge, YRRM.AcknowledgeBy, YRRM.AcknowledgeDate, YRRM.RCompanyID, YRRM.OCompanyID, YRRM.ConceptNo,
	                    (CASE WHEN YRRM.IsReqForYD=1 THEN 'Yes' ELSE 'No' END) AS YDStatus,
	                    PreProcessRevNo,RevisionNo, YRRM.AddedBy, YRRM.BuyerID, YRRM.IsWOKnittingInfo,
                        YarnReqStatus = CASE WHEN ISNULL(YRRM.IsWOKnittingInfo,0) = 1 THEN 'Independent' ELSE '' END,IsAdditional
	                    From KYReqMaster as YRRM left join KYIssueMaster as YRIM  on YRIM.KYReqMasterID= YRRM.KYReqMasterID
	                    WHERE YRRM.Approve = 1 and YRRM.Acknowledge = 1
	                    AND YRRM.KYReqDate > '11-APR-2020'
                    ), 
                    MRS As (
	                    SELECT M.KYReqMasterID, M.KYReqDate, M.KYReqNo, M.Remarks, M.Approve, M.ApproveBy, M.ApproveDate, SUM(C.ReqQty) Qty, M.YarnReqStatus, M.IsWOKnittingInfo,
	                    M.Acknowledge, M.AcknowledgeBy, M.AcknowledgeDate, CompanyName = COM.ShortName, M.ConceptNo,M.YDStatus,M.IsIssue,M.KYReqBy, E.EmployeeName KYReqByName,
	                    M.PreProcessRevNo,M.RevisionNo
                        ,Buyer = CASE WHEN M.BuyerID > 0 THEN CT.ShortName ELSE '' END,M.IsAdditional
	                    From M
	                    INNER JOIN {TableNames.KY_Req_Child} C ON C.KYReqMasterID=M.KYReqMasterID
	                    LEFT JOIN {TableNames.Knitting_Plan_Yarn} KPY ON KPY.KPYarnID=C.KPYarnID
						LEFT JOIN {TableNames.Knitting_Plan_Master} KP ON KP.KPMasterID =KPY.KPMasterID
	                    LEFT JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID=M.RCompanyID
	                    LEFT JOIN {DbNames.EPYSL}..LoginUser L ON L.UserCode = M.AddedBy
	                    LEFT Join {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
                        LEFT JOIN {DbNames.EPYSL}..Contacts CT ON CT.ContactID = M.BuyerID
	                    WHERE ISNULL(C.PreProcessRevNo,0) = ISNULL(KP.RevisionNo,0)
	                    GROUP BY M.KYReqMasterID, M.KYReqDate, M.KYReqNo, M.Remarks, M.Approve, M.ApproveBy, M.ApproveDate,M.YarnReqStatus, M.IsWOKnittingInfo,
	                    M.Acknowledge, M.AcknowledgeBy, M.AcknowledgeDate,COM.ShortName, M.ConceptNo,M.YDStatus,M.IsIssue, M.KYReqBy, E.EmployeeName,
	                    M.PreProcessRevNo,M.RevisionNo,M.BuyerID,CT.ShortName,M.IsAdditional
                    )
                    Select *, TotalRows = Count(*) Over()
                    from MRS";
            }

            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<KYReqMaster>(sql);
        }

        public async Task<List<FreeConceptMRMaster>> GetMRs(string fcIds, PaginationInfo paginationInfo)
        {
            paginationInfo.OrderBy = string.IsNullOrEmpty(paginationInfo.OrderBy) ? "ORDER BY FCMRMasterID DESC" : paginationInfo.OrderBy;
            var sql = $@"
                 Select MR.FCMRMasterID FCMRMasterID,M.ConceptId ConceptID,ConceptNo,ConceptDate,GroupID = KPM.PlanNo,
		        F.ValueName ConceptForName, Count(*) Over() TotalRows
		        JOIN {TableNames.FreeConceptMRMaster} MR
		        INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} M ON M.ConceptID=MR.ConceptID
		        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue F ON F.ValueID = M.ConceptFor
		        LEFT JOIN {TableNames.KY_Req_Child} AS YRC ON YRC.FCMRMasterID = MR.FCMRMasterID
                INNER JOIN {TableNames.Knitting_Plan_Master} KPM ON KPM.ConceptID=MR.ConceptID
                WHERE  YRC.FCMRMasterID IS NULL AND KPM.PlanNo NOT IN ({fcIds})
                {paginationInfo.FilterBy}
                {paginationInfo.OrderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<FreeConceptMRMaster>(sql);
        }

        public async Task<KYReqMaster> GetFreeConceptMRData(string[] fcIds, string Status)
        {
            var query = "";
            if (Status == "New")
            {
                query =
                $@"-- Master Data
                WITH M As 
                (
	                Select *
	                JOIN {TableNames.FreeConceptMRMaster}
	                Where FCMRMasterID IN ({fcIds})
                ) 
                Select M.FCMRMasterID FCMRMasterID, M.ConceptID, ConceptNo, ConceptDate, M.TrialNo, CM.TrialDate, M.ReqDate, 
                CM.ConceptFor, CM.CompanyID RCompanyID, COM.ShortName RCompanyName, CM.CompanyID OCompanyID, KnittingTypeID, 
                CM.ConstructionID, CM.TechnicalNameId, CompositionID, GSMID, Qty,ConceptStatusID, M.Remarks, E.ValueName ConceptForName,
	            '{Status}' As Status , KnittingType.TypeName KnittingType, Composition.SegmentValue Composition,
                Construction.SegmentValue Construction,FTN.TechnicalName, Gsm.SegmentValue GSM, KJCM.ContactID FloorID, 
                KU.ShortName FloorName
                From M
				Left JOIN {TableNames.KNITTING_JOB_CARD_Master} KJCM ON M.ConceptID = KJCM.ConceptID
				LEFT JOIN {TableNames.KNITTING_UNIT} KU ON KJCM.ContactID = KU.KnittingUnitID And KJCM.IsSubContact = 0
                LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} CM ON M.ConceptID = CM.ConceptID
                LEFT JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID = CM.CompanyID
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue E ON E.ValueID = CM.ConceptFor
                LEFT JOIN KnittingMachineType KnittingType ON KnittingType.TypeID = CM.KnittingTypeID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = CM.CompositionID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = CM.ConstructionID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = CM.GSMID
                LEFT JOIN {TableNames.FabricTechnicalName} FTN ON CM.TechnicalNameId = FTN.TechnicalNameId;

                --Childs
                WITH
                FCMR As (
	                Select *
	                JOIN {TableNames.FreeConceptMRMaster}
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
                INNER JOIN FreeConceptMRChild FCC ON FCC.FCMRMasterID = FCMR.FCMRMasterID
                INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} CM ON FCMR.ConceptID = CM.ConceptID
				LEFT JOIN {TableNames.Knitting_Plan_Yarn} KPY ON FCC.FCMRChildID=KPY.FCMRChildID
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = FCC.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                Left Join {DbNames.EPYSL}..Contacts C ON KPY.YarnBrandID = C.ContactID
                Left JOIN {TableNames.KY_Req_Child} YRC ON YRC.FCMRChildID = FCC.FCMRChildID

				Group by FCC.FCMRChildID, FCC.FCMRMasterID, FCC.ItemMasterID, FCC.YD, FCC.ReqQty,FCC.ReqCone,FCMR.ConceptID,
                CM.ConceptNo, FCC.ShadeCode, IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, IM.Segment4ValueID, 
                IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID, IM.Segment8ValueID, ISV1.SegmentValue, 
                ISV2.SegmentValue, ISV3.SegmentValue, ISV4.SegmentValue, ISV5.SegmentValue, ISV6.SegmentValue, 
                ISV7.SegmentValue, IM.Segment8ValueID,KPY.YarnLotNo,KPY.YarnBrandID, C.ShortName, FCC.ReqCone, KPY.PhysicalCount,KPY.KPYarnID;

                ---FCMR List
                WITH M As 
                (
	                Select *
	                JOIN {TableNames.FreeConceptMRMaster}
	                Where FCMRMasterID IN ({fcIds})
                ) 
                Select M.FCMRMasterID FCMRMasterID, M.ConceptID, ConceptNo, ConceptDate, CM.ConceptFor,E.ValueName ConceptForName
                From M
                LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} CM ON M.ConceptID = CM.ConceptID
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
                    Select KYReqMasterID, KYReqDate, KYReqNo, RCompanyID, OCompanyID, Approve, Acknowledge, IsReqForYD ,'{Status}' As Status 
                    From KYReqMaster MMM
                    Where MMM.KYReqMasterID In (Select Distinct YPC.KYReqMasterID
                        JOIN {TableNames.FreeConceptMRMaster} FCM Inner JOIN {TableNames.KY_Req_Child} YPC On FCM.FCMRMasterID= YPC.FCMRMasterID
                        Where FCM.FCMRMasterID IN ({fcIds}))
                )
                Select * from M;

                -- Child Data
                ;WITH YRC As 
                (
                    Select FCC.FCMRChildID As KYReqChildID, FCC.FCMRChildID, FCM.FCMRMasterID As YarnPRMasterID,FCM.ConceptID,
                    FCC.ItemMasterID,YPC.FCMRMasterID, YPC.BatchNo,YPC.YarnLotNo,YPC.YarnBrandID,YPC.PhysicalCount,
                    YM.KYReqMasterID,YM.FloorID, YPC.Remarks, (Case When Isnull(YPC.ReqQty, 0) = 0 Then FCC.ReqQty Else YPC.ReqQty End)ReqQty,
                    (Case When Isnull(YPC.ReqCone, 0) = 0 Then FCC.ReqCone Else YPC.ReqCone End)ReqCone, YPC.ShadeCode, 
                    FCC.UnitID, 'Kg' AS DisplayUnitDesc
                    JOIN {TableNames.FreeConceptMRMaster} FCM 
                    Inner Join FreeConceptMRChild FCC On FCM.FCMRMasterID = FCC.FCMRMasterID
                    Left JOIN {TableNames.KY_Req_Child} YPC ON YPC.FCMRChildID = FCC.FCMRChildID And YPC.FCMRMasterID = FCM.FCMRMasterID
                    inner join KYReqMaster YM ON YM.KYReqMasterID=YPC.KYReqMasterID
                    Where FCM.FCMRMasterID IN ({fcIds})
                )
                Select YRC.KYReqChildID, YRC.FCMRChildID FCMRChildID, YRC.FCMRMasterID, YRC.ItemMasterID,ReqQty,YRC.ConceptID,
                CM.ConceptNo, YRC.ShadeCode, YRC.BatchNo, IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, 
                IM.Segment4ValueID, IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID, IM.Segment8ValueID, 
                ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc,
                ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, 
                ISV7.SegmentValue Segment7ValueDesc, YRC.Remarks,YRC.YarnLotNo,YRC.YarnBrandID, C.ShortName YarnBrand,ReqCone,
                UN.DisplayUnitDesc, YRC.PhysicalCount, ShadeCode, FCMR.FloorID, KU.ShortName FloorName
                From YRC
                INNER JOIN KYReqMaster FCMR ON YRC.KYReqMasterID = FCMR.KYReqMasterID
                INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} CM ON YRC.ConceptID = CM.ConceptID
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
                LEFT JOIN {TableNames.KNITTING_UNIT} KU ON YRC.FloorID = KU.KnittingUnitID;

                -- Free Concpet MR List
                Select distinct YRDC.FCMRMasterID,F.ValueName ConceptForName,FCM.ConceptNo,FCM.ConceptDate, Count(*) Over() TotalRows
                FROM {TableNames.KY_Req_Child} AS  YRDC
                INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID =YRDC.ConceptID
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue F ON F.ValueID = FCM.ConceptFor
                WHERE YRDC.KYReqMasterID = {fcIds}
                -- Company
                {CommonQueries.GetCompany()};
                -- Brand
                {CommonQueries.GetYarnSpinners()};";
            }

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                KYReqMaster data = await records.ReadFirstOrDefaultAsync<KYReqMaster>();
                Guard.Against.NullObject(data);

                data.Childs = records.Read<KYReqChild>().ToList();
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

        public async Task<KYReqMaster> GetFreeConceptMRDataNew(List<KYReqMaster> entity)
        {
            var query = "";
            if (entity[0].Status == "New")
            {
                query =
                $@"-- Master Data
                WITH M As 
                (
	                Select *
	                From KnittingPlanGroup
	                Where GroupID IN ({entity[0].GroupIDs})
                ) 
                Select M.GroupID, M.GroupConceptNo ConceptNo,  CM.ConceptDate, CM.TrialNo, CM.TrialDate,M.BuyerID, 
                CM.ConceptFor, 
				RCompanyID = case when isnull(BM.BookingID,0)>0 then BM.CompanyID else SBM.ExecutionCompanyID end, 
				RCompanyName=  case when isnull(BM.BookingID,0)>0 then COM.ShortName else SCOM.ShortName end, 
				OCompanyID = case when isnull(BM.BookingID,0)>0 then BM.CompanyID else SBM.ExecutionCompanyID end, 
				M.KnittingTypeID, 
                Qty = SUM(Qty),ConceptStatusID, CM.Remarks, E.ValueName ConceptForName, CM.IsBDS,BM.BookingID,SBM.BookingID SBMBookingID,
	            'New' As Status
                From M
                LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} CM ON CM.GroupConceptNo = M.GroupConceptNo
				LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster SBM ON SBM.BookingID=CM.BookingID 
				LEFT JOIN {DbNames.EPYSL}..BookingMaster BM ON BM.BookingID=CM.BookingID 
                LEFT JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID = BM.CompanyID
				LEFT JOIN {DbNames.EPYSL}..CompanyEntity SCOM ON SCOM.CompanyID = SBM.ExecutionCompanyID
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue E ON E.ValueID = CM.ConceptFor
                LEFT JOIN KnittingMachineType KnittingType ON KnittingType.TypeID = CM.KnittingTypeID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = CM.CompositionID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = CM.GSMID
                LEFT JOIN {TableNames.FabricTechnicalName} FTN ON CM.TechnicalNameId = FTN.TechnicalNameId
				GROUP BY M.GroupID, M.GroupConceptNo,  CM.ConceptDate, CM.TrialNo, CM.TrialDate,M.BuyerID, 
                CM.ConceptFor, BM.CompanyID,SBM.ExecutionCompanyID, COM.ShortName, SCOM.ShortName, M.KnittingTypeID, 
                ConceptStatusID, CM.Remarks, E.ValueName, CM.IsBDS,BM.BookingID,SBM.BookingID;

                --Childs

                ;WITH
                KPG As (
	                Select *
	                From KnittingPlanGroup
	                Where GroupID IN ({entity[0].GroupIDs})
                ),
                A as(
                    select YRC.KPYarnID,YRC.ItemMasterID,YRC.ConceptID,SUM(YRC.ReqQty)ReqQty 
                    FROM {TableNames.KY_Req_Child} YRC
                    GROUP BY YRC.KPYarnID,YRC.ItemMasterID,YRC.ConceptID
                ),
                L As(
	                   Select KPG.GroupID, KPY.ItemMasterID, CM.ConceptID, CM.BookingID,
	                CM.ConceptNo, IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, IM.Segment4ValueID, 
	                IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID, IM.Segment8ValueID, ISV1.SegmentValue Segment1ValueDesc, 
	                ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc, 
	                ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc,
	                IM.Segment8ValueID Segment8ValueDesc,KPY.YarnLotNo,KPY.YarnBrandID, KPY.BatchNo, C.ShortName YarnBrand, 
	                KPY.PhysicalCount,CM.GroupConceptNo, CM.ConceptDate,KPY.KPYarnID, YRC.ShadeCode, KPYC.FCMRChildID, KPY.YDItem, KPM.RevisionNo,
	                --ProduceKnittingQty = CASE WHEN CM.IsBDS = 2 THEN ISNULL(FBC.GreyProdQty,0) ELSE CM.ProduceKnittingQty END,
					ProduceKnittingQty = CASE WHEN CM.IsBDS = 2 THEN 
							CASE WHEN CM.SubGroupID = 1 Then FBC.GreyProdQty Else Cast (Round((FBC.BookingQty/FBC.BookingQtyKG)*FBC.GreyProdQty,0) As int) END 
						ELSE CM.ProduceKnittingQty
						END,
					PlanQty = ISNULL(KPC.BookingQty,0) 
					From KPG
	                Inner JOIN {TableNames.Knitting_Plan_Master} KPM ON KPM.PlanNo = KPG.GroupID
					LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPMasterID = KPM.KPMasterID
	                Inner JOIN {TableNames.Knitting_Plan_Yarn} KPY ON KPY.KPMasterID = KPM.KPMasterID
                    INNER JOIN (
		                SELECT KPYC.KPYarnID, KPYC.FCMRChildID
		                FROM KnittingPlanYarnChild KPYC
		                INNER JOIN {TableNames.Knitting_Plan_Yarn} KPY ON KPY.KPYarnID = KPYC.KPYarnID
		                WHERE KPY.GroupID IN ({entity[0].GroupIDs})
	                ) KPYC ON KPYC.KPYarnID = KPY.KPYarnID
	                Inner JOIN {TableNames.RND_FREE_CONCEPT_MASTER} CM ON CM.ConceptID = KPM.ConceptID
					LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FBC ON FBC.BookingChildID = CM.BookingChildID AND FBC.BookingID = CM.BookingID
	                LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = KPY.ItemMasterID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
	                Left Join {DbNames.EPYSL}..Contacts C ON KPY.YarnBrandID = C.ContactID
	                Left JOIN {TableNames.KY_Req_Child} YRC ON YRC.KPYarnID = KPY.KPYarnID AND YRC.ItemMasterID = KPY.ItemMasterID
	                Group by KPG.GroupID, KPY.ItemMasterID, CM.ConceptID, CM.BookingID,
	                CM.ConceptNo, IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, IM.Segment4ValueID, 
	                IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID, IM.Segment8ValueID, ISV1.SegmentValue, 
	                ISV2.SegmentValue, ISV3.SegmentValue, ISV4.SegmentValue, 
	                ISV5.SegmentValue, ISV6.SegmentValue, ISV7.SegmentValue,
	                IM.Segment8ValueID,KPY.YarnLotNo,KPY.YarnBrandID, KPY.BatchNo, C.ShortName, 
	                KPY.PhysicalCount,CM.GroupConceptNo, CM.ConceptDate,KPY.KPYarnID, YRC.ShadeCode, KPYC.FCMRChildID, KPY.YDItem, KPM.RevisionNo,
					CM.IsBDS, FBC.GreyProdQty, CM.ProduceKnittingQty, KPC.BookingQty, FBC.BookingQty, FBC.BookingQtyKG, CM.SubGroupID  
                 
                ),
                FinalList AS
                (
                    Select L.GroupID, YSS.ItemMasterID, L.ConceptID, L.BookingID, L.YDItem, PreProcessRevNo = L.RevisionNo,
	                    L.ConceptNo, L.Segment1ValueID, L.Segment2ValueID, L.Segment3ValueID, L.Segment4ValueID, 
	                    L.Segment5ValueID, L.Segment6ValueID, L.Segment7ValueID, L.Segment8ValueID, L.Segment1ValueDesc, 
	                    L.Segment2ValueDesc, L.Segment3ValueDesc, L.Segment4ValueDesc, 
	                    L.Segment5ValueDesc, L.Segment6ValueDesc, L.Segment7ValueDesc,
	                    L.Segment8ValueDesc, YSS.SpinnerId YarnBrandID, L.BatchNo, 
	                    L.GroupConceptNo, L.ConceptDate, L.KPYarnID, YSS.ShadeCode,
	                    YarnReqQty = cast(ROUND((Sum(CI.NetYarnReqQty)/100) * ((L.PlanQty/L.ProduceKnittingQty)*100),2)as numeric(36,2)), 
					    UsedQty = isnull(A.ReqQty,0), 
					    PendingQty = cast(ROUND((Sum(CI.NetYarnReqQty)/100) * ((L.PlanQty/L.ProduceKnittingQty)*100),2)as numeric(36,2))-isnull(A.ReqQty,0),
					    ReqQty = cast(ROUND((Sum(CI.NetYarnReqQty)/100) * ((L.PlanQty/L.ProduceKnittingQty)*100),2)as numeric(36,2)) - isnull(A.ReqQty,0),
					    MaxReqQty = CEILING(cast(ROUND((Sum(CI.NetYarnReqQty)/100) * ((L.PlanQty/L.ProduceKnittingQty)*100),2)as numeric(36,2))-isnull(A.ReqQty,0) ), 
					    Max(MRC.ReqCone) ReqCone,MRC.FCMRChildID,MRC.YBChildItemID,SUM(isnull(YACI.TotalAllocationQty,0))AllocatedQty,
	                    YSS.PhysicalCount,YSS.YarnLotNo,C.ShortName YarnBrand,YSS.YarnCategory, YACI.AllocationChildItemID, L.ProduceKnittingQty, L.PlanQty
                    From L
                    --Inner Join FreeConceptMRChild MRC On MRC.FCMRChildID = L.FCMRChildID
                    Inner Join KnittingPlanYarnChild KPYC ON KPYC.KPYarnID=L.KPYarnID AND KPYC.ConceptID=L.ConceptID --AND KPYC.ItemMasterID=L.ItemMasterID--On KPYC.FCMRChildID = L.FCMRChildID
                    Inner Join FreeConceptMRChild MRC On MRC.FCMRChildID = KPYC.FCMRChildID
                    LEFT JOIN {TableNames.YarnBookingChildItem_New} CI ON CI.YBChildItemID = MRC.YBChildItemID
                    --LEFT JOIN {TableNames.KY_Req_Child} YRC ON YRC.KPYarnID=KPYC.KPYarnID AND YRC.ConceptID=KPYC.ConceptID --AND YRC.ItemMasterID=KPYC.ItemMasterID--On YRC.FCMRChildID=L.FCMRChildID
                    LEFT JOIN A on  A.KPYarnID = L.KPYarnID AND A.ConceptID=L.ConceptID--AND A.ItemMasterID = L.ItemMasterID 
                    LEFT JOIN YarnAllocationChild YAC ON YAC.YBChildItemID=MRC.YBChildItemID 
                    LEFT JOIN YarnAllocationChildItem YACI ON YACI.AllocationChildID=YAC.AllocationChildID AND YACI.Acknowledge = 1
                    LEFT JOIN YarnStockSet YSS ON YSS.YarnStockSetId=YACI.YarnStockSetId
                    LEFT JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID=YSS.SpinnerId
                    where isnull(A.ReqQty,0) < CI.NetYarnReqQty			
                    Group By L.GroupID, YSS.ItemMasterID, L.ConceptID, L.BookingID, L.YDItem, L.RevisionNo,
                    L.ConceptNo, L.Segment1ValueID, L.Segment2ValueID, L.Segment3ValueID, L.Segment4ValueID, 
                    L.Segment5ValueID, L.Segment6ValueID, L.Segment7ValueID, L.Segment8ValueID, L.Segment1ValueDesc, 
                    L.Segment2ValueDesc, L.Segment3ValueDesc, L.Segment4ValueDesc, 
                    L.Segment5ValueDesc, L.Segment6ValueDesc, L.Segment7ValueDesc,
                    L.Segment8ValueDesc, YSS.SpinnerId, L.BatchNo, 
                    L.GroupConceptNo, L.ConceptDate, L.KPYarnID, YSS.ShadeCode,A.ReqQty,CI.NetYarnReqQty,MRC.FCMRChildID,MRC.YBChildItemID,
                    YSS.PhysicalCount,YSS.YarnLotNo,C.ShortName,YSS.YarnCategory,YACI.AllocationChildItemID, L.ProduceKnittingQty, L.PlanQty
                )
                SELECT * FROM FinalList

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
                    Select KYReqMasterID, KYReqDate, KYReqNo, RCompanyID, OCompanyID, Approve, Acknowledge, 
                    IsReqForYD ,'Revision' As Status 
                    From KYReqMaster MMM
                    Where MMM.KYReqMasterID 
					In (
						Select YRC.KYReqMasterID
                        FROM {TableNames.Knitting_Plan_Yarn} KPY
						Inner JOIN {TableNames.KY_Req_Child} YRC ON YRC.KPYarnID = KPY.KPYarnID AND YRC.ItemMasterID = KPY.ItemMasterID
                        Where KPY.GroupID IN ({entity[0].GroupIDs})
						GROUP BY YRC.KYReqMasterID
					)
                )
                Select * from M;

                -- Child Data
                ;WITH YRC As 
                (
                    Select YM.KYReqMasterID, FCC.FCMRChildID As KYReqChildID, FCC.FCMRChildID, FCM.FCMRMasterID As YarnPRMasterID,FCM.ConceptID, FC.BookingID,
                    FCC.ItemMasterID,YPC.FCMRMasterID, YPC.BatchNo,YPC.YarnLotNo,YPC.YarnBrandID,YPC.PhysicalCount,
                    YM.FloorID, YPC.Remarks, (Case When Isnull(YPC.ReqQty, 0) = 0 Then FCC.ReqQty Else YPC.ReqQty End)ReqQty,
                    (Case When Isnull(YPC.ReqCone, 0) = 0 Then FCC.ReqCone Else YPC.ReqCone End)ReqCone, YPC.ShadeCode, 
                    FCC.UnitID, 'Kg' AS DisplayUnitDesc,KPY.KPYarnID
                    JOIN {TableNames.FreeConceptMRMaster} FCM 
                    Inner Join FreeConceptMRChild FCC On FCM.FCMRMasterID = FCC.FCMRMasterID
					Left JOIN {TableNames.Knitting_Plan_Yarn} KPY On KPY.FCMRChildID= FCC.FCMRChildID
                    Left JOIN {TableNames.KY_Req_Child} YPC ON YPC.KPYarnID = KPY.KPYarnID AND YPC.ItemMasterID = KPY.ItemMasterID
                    inner join KYReqMaster YM ON YM.KYReqMasterID = YPC.KYReqMasterID
                    LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FC ON FC.ConceptID = FCM.ConceptID
                    Where KPY.GroupID IN ({entity[0].GroupIDs})
                ),
					A as(
				select YRC.KPYarnID,YRC.ItemMasterID,YRC.ConceptID,sum(YRC.ReqQty)ReqQty FROM {TableNames.KY_Req_Child} YRC
				GROUP BY YRC.KPYarnID,YRC.ItemMasterID,YRC.ConceptID
				)
                Select YRC.KYReqChildID, YRC.FCMRChildID FCMRChildID, YRC.FCMRMasterID, YRC.ItemMasterID,YRC.ConceptID, YRC.BookingID,
                CM.ConceptNo, YRC.ShadeCode, YRC.BatchNo, IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, 
                IM.Segment4ValueID, IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID, IM.Segment8ValueID, 
                ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc,
                ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, 
                ISV7.SegmentValue Segment7ValueDesc, YRC.Remarks,YRC.YarnLotNo,YRC.YarnBrandID, C.ShortName YarnBrand,MRC.ReqCone,
                UN.DisplayUnitDesc, YRC.PhysicalCount, MRC.ShadeCode, FCMR.FloorID, KU.ShortName FloorName, CM.GroupConceptNo, CM.ConceptDate
				,Sum(MRC.ReqQty) YarnReqQty,isnull(A.ReqQty,0) UsedQty, MRC.ReqQty-isnull(A.ReqQty,0) PendingQty, MRC.ReqQty-isnull(A.ReqQty,0) ReqQty,CEILING( MRC.ReqQty-isnull(A.ReqQty,0) )MaxReqQty
                From YRC
                INNER JOIN KYReqMaster FCMR ON YRC.KYReqMasterID = FCMR.KYReqMasterID
                INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} CM ON YRC.ConceptID = CM.ConceptID
				Inner Join KnittingPlanYarnChild KPYC ON KPYC.KPYarnID=YRC.KPYarnID AND KPYC.ConceptID=YRC.ConceptID --AND KPYC.ItemMasterID=YRC.ItemMasterID--On KPYC.FCMRChildID = L.FCMRChildID
                LEFT Join FreeConceptMRChild MRC On MRC.FCMRChildID = KPYC.FCMRChildID
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
                LEFT JOIN {TableNames.KNITTING_UNIT} KU ON YRC.FloorID = KU.KnittingUnitID
				GROUP BY YRC.KYReqChildID, YRC.FCMRChildID, YRC.FCMRMasterID, YRC.ItemMasterID,YRC.ConceptID, YRC.BookingID,
                CM.ConceptNo, YRC.ShadeCode, YRC.BatchNo, IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, 
                IM.Segment4ValueID, IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID, IM.Segment8ValueID, 
                ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue,
                ISV4.SegmentValue, ISV5.SegmentValue, ISV6.SegmentValue, 
                ISV7.SegmentValue, YRC.Remarks,YRC.YarnLotNo,YRC.YarnBrandID, C.ShortName,MRC.ReqCone,
                UN.DisplayUnitDesc, YRC.PhysicalCount, MRC.ShadeCode, FCMR.FloorID, KU.ShortName, CM.GroupConceptNo, CM.ConceptDate
				,MRC.ReqQty,isnull(A.ReqQty,0), MRC.ReqQty-isnull(A.ReqQty,0), MRC.ReqQty-isnull(A.ReqQty,0),CEILING( MRC.ReqQty-isnull(A.ReqQty,0) )

                -- Company
                {CommonQueries.GetCompany()};
                -- Brand
                {CommonQueries.GetYarnSpinners()};";
            }

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                KYReqMaster data = await records.ReadFirstOrDefaultAsync<KYReqMaster>();
                Guard.Against.NullObject(data);

                data.Childs = records.Read<KYReqChild>().ToList();
                data.RCompanyList = records.Read<Select2OptionModel>().ToList();
                data.YarnBrandList = records.Read<Select2OptionModel>().ToList();

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
        public async Task<KYReqMaster> GetNewWithoutKnittingInfo()
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
                    FROM FBookingAcknowledge 
                    WHERE ISNUMERIC(SUBSTRING(LTRIM(BookingNo), 1, 1)) = 1 
                    GROUP BY BookingNo
                    ORDER BY BookingNo DESC";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                KYReqMaster data = new KYReqMaster();
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
        public async Task<KYReqMaster> GetAsync(int id, int flag, Status status)
        {

            var query =
                $@"
                -- Master Data
                Select (case when {flag} = 1 Then '0' Else   MMM.KYReqMasterID End)KYReqMasterID, MMM.KYReqDate, --KYReqNo,
                (case when {flag} = 1 Then '<--New-->' Else KYReqNo End) KYReqNo,
                ParentKYReqNo = KYReqNo,
                MMM.RCompanyID, COM.ShortName RCompanyName, MMM.OCompanyID, MMM.Approve, MMM.Acknowledge, MMM.IsReqForYD,
                MMM.IsWOKnittingInfo,MMM.IsAdditional
                From KYReqMaster MMM
                LEFT JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID = MMM.RCompanyID
                Where MMM.KYReqMasterID = {id};

                --Childs
                 WITH
                --A as(
                --	select FCC.KPYarnID,FCC.ConceptID,FCC.ItemMasterID,FCC.AllocationChildItemID,isnull(sum(FCC.ReqQty),0) UsedQty 
                --	FROM {TableNames.KY_Req_Child} FCC
                --	INNER JOIN KYReqMaster M ON M.KYReqMasterID = FCC.KYReqMasterID
                --	Where M.IsAdditional=0 AND FCC.KYReqChildID IN (78,79)
                --	GROUP BY FCC.KPYarnID,FCC.ConceptID,FCC.ItemMasterID,FCC.AllocationChildItemID
                --),
                B as(
	                Select FCC.KYReqChildID,
	                FCC.AllocationChildItemID, 
	                FCC.FCMRChildID,
	                FCC.FCMRMasterID, 
	                FCC.ItemMasterID, 
	                FCC.KPYarnID, 
	                FCC.PreProcessRevNo,
	                FCC.ConceptID,
	                ConceptNo = CASE WHEN ISNULL(FCC.ConceptID,0) > 0 THEN CM.ConceptNo ELSE SBM.BookingNo END, 
	                FCC.ShadeCode, 
	                FCC.BatchNo,
	                IM.Segment1ValueID, 
	                IM.Segment2ValueID, 
	                IM.Segment3ValueID, 
	                IM.Segment4ValueID, 
	                IM.Segment5ValueID, 
	                IM.Segment6ValueID, 
	                IM.Segment7ValueID,
	                IM.Segment8ValueID, 
	                ISV1.SegmentValue Segment1ValueDesc, 
	                ISV2.SegmentValue Segment2ValueDesc, 
	                ISV3.SegmentValue Segment3ValueDesc,
	                ISV4.SegmentValue Segment4ValueDesc, 
	                ISV5.SegmentValue Segment5ValueDesc, 
	                ISV6.SegmentValue Segment6ValueDesc, 
	                ISV7.SegmentValue Segment7ValueDesc,
	                FCC.Remarks,
	                FCC.YarnLotNo,
	                FCC.YarnBrandID, 
	                CM.GroupConceptNo, 
	                C.ShortName YarnBrand,
	                FCC.ReqCone,
	                UN.DisplayUnitDesc, 
	                FCC.PhysicalCount, 
	                FCMR.FloorID, 
	                KU.ShortName FloorName,

                    YarnReqQty = cast(ROUND((Sum(CI.NetYarnReqQty)/100) * ((KPC.BookingQty/CASE WHEN CM.IsBDS = 2 THEN ISNULL(FBC.GreyProdQty,0) ELSE CM.ProduceKnittingQty END)*100),2)as numeric(36,2)), 
                    --isnull(sum(FCC.ReqQty),0) ReqQty
                    FCC.ReqQty, FCC.ReqQty UsedQty,  
                    PendingQty = case when cast(ROUND((Sum(CI.NetYarnReqQty)/100) * ((KPC.BookingQty/CASE WHEN CM.IsBDS = 2 THEN ISNULL(FBC.GreyProdQty,0) ELSE CM.ProduceKnittingQty END)*100),2)as numeric(36,2))-SUM(isnull(FCC2.ReqQty,0))<0 
                    then 0 
                    else cast(ROUND((Sum(CI.NetYarnReqQty)/100) * ((KPC.BookingQty/CASE WHEN CM.IsBDS = 2 THEN ISNULL(FBC.GreyProdQty,0) ELSE CM.ProduceKnittingQty END)*100),2)as numeric(36,2))-SUM(isnull(FCC2.ReqQty,0))
                    end,
                    MaxReqQty = CEILING(cast(ROUND((Sum(CI.NetYarnReqQty)/100) * ((KPC.BookingQty/CASE WHEN CM.IsBDS = 2 THEN ISNULL(FBC.GreyProdQty,0) ELSE CM.ProduceKnittingQty END)*100),2)as numeric(36,2))),

					AllocatedQty = SUM(isnull(YACI.TotalAllocationQty,0)), KPY.YDItem


	                FROM {TableNames.KY_Req_Child} FCC
					LEFT JOIN {TableNames.KY_Req_Child} FCC2 ON FCC2.FCMRChildID = FCC.FCMRChildID AND FCC2.KYReqMasterID <> FCC.KYReqMasterID
	                --LEFT JOIN A ON A.KPYarnID = FCC.KPYarnID AND A.ConceptID=FCC.ConceptID AND A.ItemMasterID=FCC.ItemMasterID 
	                Inner Join KnittingPlanYarnChild KPYC ON FCC.KPYarnID=KPYC.KPYarnID AND FCC.ConceptID=KPYC.ConceptID --AND FCC.ItemMasterID=KPYC.ItemMasterID
	                Inner JOIN {TableNames.Knitting_Plan_Yarn} KPY ON KPY.KPYarnID = KPYC.KPYarnID
					LEFT Join FreeConceptMRChild MRC On MRC.FCMRChildID = KPYC.FCMRChildID
					Inner JOIN {TableNames.Knitting_Plan_Master} KPM ON KPM.PlanNo = KPY.GroupID
					LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPMasterID = KPM.KPMasterID
	                LEFT JOIN {TableNames.YarnBookingChildItem_New} CI ON CI.YBChildItemID = MRC.YBChildItemID
	                INNER JOIN YarnAllocationChildItem YACI ON YACI.AllocationChildItemID = FCC.AllocationChildItemID
	                INNER JOIN KYReqMaster FCMR ON FCC.KYReqMasterID = FCMR.KYReqMasterID
	                LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} CM ON FCC.ConceptID = CM.ConceptID
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
	                LEFT JOIN {TableNames.KNITTING_UNIT} KU ON FCMR.FloorID = KU.KnittingUnitID
	                Where FCC.KYReqMasterID = {id} 
	                GROUP BY FCC.KYReqChildID,
	                FCC.AllocationChildItemID, 
	                FCC.FCMRChildID , FCC.FCMRMasterID, FCC.ItemMasterID, FCC.KPYarnID, FCC.PreProcessRevNo,FCC.ConceptID,
	                CASE WHEN ISNULL(FCC.ConceptID,0) > 0 THEN CM.ConceptNo ELSE SBM.BookingNo END, FCC.ShadeCode, FCC.BatchNo,
	                IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, IM.Segment4ValueID, IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID,
	                IM.Segment8ValueID, ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue,
	                ISV4.SegmentValue, ISV5.SegmentValue, ISV6.SegmentValue, ISV7.SegmentValue,
	                FCC.Remarks,FCC.YarnLotNo,FCC.YarnBrandID, CM.GroupConceptNo, C.ShortName,(case when 2 = 1 Then 0 Else  FCC.ReqCone End),UN.DisplayUnitDesc, FCC.PhysicalCount, FCC.ShadeCode, FCMR.FloorID, KU.ShortName,
	                KPYC.ReqQty,FCC.ReqQty, KPC.BookingQty, FBC.GreyProdQty, CM.ProduceKnittingQty, CM.IsBDS, KPY.YDItem
                )
                select * from B --where ReqQty<>0

                -- Company
                Select Cast (CE.CompanyID as varchar) [id] , CE.ShortName [text]
                FROM {DbNames.EPYSL}..CompanyEntity CE 
                WHERE CE.BusinessNature IN ('TEX','PC') ORDER BY CE.ShortName;

               -- Brand
                {CommonQueries.GetYarnSpinners()};

                -- Free Concpet MR List
                Select distinct YRDC.FCMRMasterID,F.ValueName ConceptForName,FCM.ConceptNo,FCM.ConceptDate, Count(*) Over() TotalRows
                FROM {TableNames.KY_Req_Child} AS  YRDC
                INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID =YRDC.ConceptID
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue F ON F.ValueID = FCM.ConceptFor
                WHERE YRDC.KYReqMasterID = {id}

                --Booking No
                SELECT id = MIN(BookingID), text = BookingNo 
                FROM FBookingAcknowledge 
                WHERE ISNUMERIC(SUBSTRING(LTRIM(BookingNo), 1, 1)) = 1 
                GROUP BY BookingNo
                ORDER BY BookingNo DESC";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                KYReqMaster data = await records.ReadFirstOrDefaultAsync<KYReqMaster>();
                Guard.Against.NullObject(data);

                data.Childs = records.Read<KYReqChild>().ToList();
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
        public async Task<KYReqMaster> GetAsyncGroupBy(int id, int flag)
        {
            var query =
                $@"
                -- Master Data
                Select (case when {flag} = 1 Then '0' Else   MMM.KYReqMasterID End)KYReqMasterID, MMM.KYReqDate, --KYReqNo,
                (case when {flag} = 1 Then '<--New-->' Else KYReqNo End) KYReqNo,
                ParentKYReqNo = KYReqNo,
                MMM.RCompanyID, COM.ShortName RCompanyName, MMM.OCompanyID, MMM.Approve, MMM.Acknowledge, MMM.IsReqForYD,
                MMM.IsWOKnittingInfo
                From KYReqMaster MMM
                LEFT JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID = MMM.RCompanyID
                Where MMM.KYReqMasterID = {id};

				-- Childs
                Select FCC.FCMRChildID FCMRChildID, FCC.FCMRMasterID, FCC.ItemMasterID, FCC.PreProcessRevNo, sum((case when 2 = 1 Then 0 Else  FCC.ReqQty End))ReqQty,
FCC.ShadeCode, FCC.BatchNo, ConceptNo = CM.GroupConceptNo,
IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, IM.Segment4ValueID, IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID,
IM.Segment8ValueID, ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc,
ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc,
FCC.Remarks,FCC.YarnLotNo,FCC.YarnBrandID, CM.GroupConceptNo, C.ShortName YarnBrand,sum((case when 2 = 1 Then 0 Else  FCC.ReqCone End))ReqCone,UN.DisplayUnitDesc, FCC.PhysicalCount, ShadeCode, FCMR.FloorID, KU.ShortName FloorName
FROM {TableNames.KY_Req_Child} FCC
INNER JOIN KYReqMaster FCMR ON FCC.KYReqMasterID = FCMR.KYReqMasterID
LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} CM ON FCC.ConceptID = CM.ConceptID
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
LEFT JOIN {TableNames.KNITTING_UNIT} KU ON FCMR.FloorID = KU.KnittingUnitID
Where FCC.KYReqMasterID = {id}
GROUP BY FCC.FCMRChildID, FCC.FCMRMasterID, FCC.ItemMasterID, FCC.PreProcessRevNo,
FCC.ShadeCode, FCC.BatchNo, CM.GroupConceptNo,
IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, IM.Segment4ValueID, IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID,
IM.Segment8ValueID, ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue,
ISV4.SegmentValue, ISV5.SegmentValue, ISV6.SegmentValue, ISV7.SegmentValue,
FCC.Remarks,FCC.YarnLotNo,FCC.YarnBrandID, CM.GroupConceptNo, C.ShortName,UN.DisplayUnitDesc, FCC.PhysicalCount, ShadeCode, FCMR.FloorID, KU.ShortName;

                -- Company
                Select Cast (CE.CompanyID as varchar) [id] , CE.ShortName [text]
                    FROM {DbNames.EPYSL}..CompanyEntity CE 
                    WHERE CE.BusinessNature IN ('TEX','PC') ORDER BY CE.ShortName;

               -- Brand
                {CommonQueries.GetYarnSpinners()};
                -- Free Concpet MR List
                Select distinct YRDC.FCMRMasterID,F.ValueName ConceptForName,FCM.ConceptNo,FCM.ConceptDate, Count(*) Over() TotalRows
                FROM {TableNames.KY_Req_Child} AS  YRDC
                INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID =YRDC.ConceptID
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue F ON F.ValueID = FCM.ConceptFor
                WHERE YRDC.KYReqMasterID = {id}

                --Booking No
                SELECT id = MIN(BookingID), text = BookingNo 
                FROM FBookingAcknowledge 
                WHERE ISNUMERIC(SUBSTRING(LTRIM(BookingNo), 1, 1)) = 1 
                GROUP BY BookingNo
                ORDER BY BookingNo DESC";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                KYReqMaster data = await records.ReadFirstOrDefaultAsync<KYReqMaster>();
                Guard.Against.NullObject(data);

                data.Childs = records.Read<KYReqChild>().ToList();
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
        public async Task<KYReqMaster> GetReviseAsync(int id, int flag, string mrId)
        {
            var query =
                $@"
                -- Master Data
                Select MMM.KYReqMasterID, MMM.KYReqDate, MMM.KYReqNo, MMM.RCompanyID, COM.ShortName RCompanyName, 
                MMM.OCompanyID, MMM.Approve, MMM.Acknowledge, MMM.IsReqForYD
                From KYReqMaster MMM 
                LEFT JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID = MMM.RCompanyID
                Where MMM.KYReqMasterID = {id};

                  -- Childs
                 WITH
                AB AS
                (
	                Select C.ConceptID, C.KPYarnID, C.KYReqChildID, M.IsReqForYD,M.IsAdditional
	                FROM {TableNames.KY_Req_Child} C
	                INNER JOIN KYReqMaster M ON M.KYReqMasterID = C.KYReqMasterID
	                Where C.KYReqMasterID IN ({id})
                ), Con As(
	                Select ConceptID, IsReqForYD
	                From AB
	                Group By ConceptID,IsReqForYD
                ),
				A as(
				select YRC.KPYarnID,YRC.ItemMasterID,YRC.ConceptID,sum(YRC.ReqQty)ReqQty FROM {TableNames.KY_Req_Child} YRC
				INNER JOIN KYReqMaster M ON M.KYReqMasterID = YRC.KYReqMasterID
				Where M.IsAdditional=0
				GROUP BY YRC.KPYarnID,YRC.ItemMasterID,YRC.ConceptID
				),
                FCMR As (
	                Select YC.KPYarnID, MRC.ItemMasterID, YC.ConceptID, YD = c.YDItem, b.RevisionNo, 
					--ReqQty = SUM(YC.ReqQty), 
					ReqCone = MAX(MRC.ReqCone),Sum(MRC.ReqQty) YarnReqQty,isnull(A.ReqQty,0) UsedQty,case when (Sum(MRC.ReqQty)-isnull(A.ReqQty,0))<0 then 0 else Sum(MRC.ReqQty)-isnull(A.ReqQty,0)end PendingQty,
					--case when AB.IsAdditional=1 then FCC.ReqQty else FCC.ReqQty+(Sum(MRC.ReqQty)-isnull(A.ReqQty,0))end ReqQty,CEILING(case when AB.IsAdditional=1 then FCC.ReqQty else FCC.ReqQty+(Sum(MRC.ReqQty)-isnull(A.ReqQty,0))end)MaxReqQty
	                FCC.ReqQty ReqQty,CEILING(Sum(MRC.ReqQty))MaxReqQty --CEILING(case when AB.IsAdditional=1 then FCC.ReqQty else FCC.ReqQty+(Sum(MRC.ReqQty)-isnull(A.ReqQty,0))end)MaxReqQty
                    From  KnittingPlanYarnChild YC
	                inner Join Con On Con.ConceptID = YC.ConceptID
	                Inner JOIN {TableNames.Knitting_Plan_Master} b on b.ConceptID = YC.ConceptID
	                Inner JOIN {TableNames.Knitting_Plan_Yarn} c on c.KPYarnID = YC.KPYarnID AND ISNULL(c.YDItem,0) = ISNULL(Con.IsReqForYD,0)
	                INNER JOIN FreeConceptMRChild MRC ON MRC.FCMRChildID = YC.FCMRChildID
				    LEFT JOIN AB ON AB.KPYarnID = YC.KPYarnID
					LEFT JOIN {TableNames.KY_Req_Child} FCC ON FCC.KYReqChildID= AB.KYReqChildID
					LEFT JOIN A on  A.KPYarnID = YC.KPYarnID AND A.ItemMasterID = YC.ItemMasterID AND A.ConceptID=YC.ConceptID  
	                GROUP BY AB.IsAdditional,YC.KPYarnID, MRC.ItemMasterID, YC.ConceptID, c.YDItem, b.RevisionNo,isnull(A.ReqQty,0),MRC.ReqQty,FCC.ReqQty
                )
                Select KYReqChildID=ISNULL(AB.KYReqChildID,0), FCMR.ItemMasterID, FCMR.YD, FCMR.ReqQty, FCMR.ReqCone, FCMR.ConceptID,
                CM.ConceptNo, IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, IM.Segment4ValueID, 
                IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID, IM.Segment8ValueID, ISV1.SegmentValue Segment1ValueDesc, 
                ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc, 
                ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc,
                IM.Segment8ValueID Segment8ValueDesc,KPY.YarnLotNo,KPY.YarnBrandID, C.ShortName YarnBrand,
                KPY.PhysicalCount,KPY.KPYarnID,KPY.BatchNo, PreProcessRevNo = FCMR.RevisionNo,FCMR.YarnReqQty,FCMR.UsedQty,FCMR.PendingQty,FCMR.MaxReqQty
                From FCMR
                LEFT JOIN AB ON AB.KPYarnID = FCMR.KPYarnID
                LEFT JOIN {TableNames.Knitting_Plan_Yarn} KPY ON KPY.KPYarnID = FCMR.KPYarnID
                Left JOIN {TableNames.RND_FREE_CONCEPT_MASTER} CM On CM.ConceptID = FCMR.ConceptID
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = FCMR.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                Left Join {DbNames.EPYSL}..Contacts C ON KPY.YarnBrandID = C.ContactID

                Group by ISNULL(AB.KYReqChildID,0), FCMR.ItemMasterID, FCMR.YD, FCMR.ReqQty, FCMR.ReqCone, FCMR.ConceptID,
                CM.ConceptNo, IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, IM.Segment4ValueID, 
                IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID, IM.Segment8ValueID, ISV1.SegmentValue, 
                ISV2.SegmentValue, ISV3.SegmentValue, ISV4.SegmentValue,
                ISV5.SegmentValue, ISV6.SegmentValue, ISV7.SegmentValue,
                IM.Segment8ValueID,KPY.YarnLotNo,KPY.YarnBrandID, C.ShortName, 
                KPY.PhysicalCount,KPY.KPYarnID,KPY.BatchNo,FCMR.RevisionNo,FCMR.YarnReqQty,FCMR.UsedQty,FCMR.PendingQty,FCMR.MaxReqQty;


                -- Company
                {CommonQueries.GetCompany()};

               -- Brand
                {CommonQueries.GetYarnSpinners()};

                -- Free Concpet MR List
                Select distinct YRDC.FCMRMasterID,F.ValueName ConceptForName,FCM.ConceptNo,FCM.ConceptDate, Count(*) Over() TotalRows
                FROM {TableNames.KY_Req_Child} AS  YRDC
                INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID =YRDC.ConceptID
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue F ON F.ValueID = FCM.ConceptFor
                WHERE YRDC.KYReqMasterID = {id}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                KYReqMaster data = await records.ReadFirstOrDefaultAsync<KYReqMaster>();
                Guard.Against.NullObject(data);

                data.Childs = records.Read<KYReqChild>().ToList();
                data.RCompanyList = records.Read<Select2OptionModel>().ToList();
                data.YarnBrandList = records.Read<Select2OptionModel>().ToList();
                data.FreeConceptMR = records.Read<FreeConceptMRMaster>().ToList();
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
        public async Task<KYReqMaster> GetDetailsAsync(int id)
        {
            var query =
                $@"
                Select * From KYReqMaster Where KYReqMasterID = {id}

                Select * FROM {TableNames.KY_Req_Child} Where KYReqMasterID = {id}

                Select * FROM {TableNames.KYReqBuyerTeam} Where KYReqMasterID = {id}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                KYReqMaster data = await records.ReadFirstOrDefaultAsync<KYReqMaster>();
                Guard.Against.NullObject(data);

                data.Childs = records.Read<KYReqChild>().ToList();
                data.KYReqBuyerTeams = records.Read<KYReqBuyerTeam>().ToList();
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
        public async Task<KYReqMaster> GetDetailsForReviseAsync(int id)
        {
            var query =
                $@"
                Select TOP(1)*,PreProcessRevNo=KP.RevisionNo 
                From KYReqMaster M
				LEFT JOIN {TableNames.KY_Req_Child} C ON C.KYReqMasterID = M.KYReqMasterID
				LEFT JOIN {TableNames.Knitting_Plan_Master} KP ON KP.ConceptID = C.ConceptID
				Where M.KYReqMasterID = {id}

                Select * FROM {TableNames.KY_Req_Child} Where KYReqMasterID = {id}

                Select * FROM {TableNames.KYReqBuyerTeam} Where KYReqMasterID = {id}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                KYReqMaster data = await records.ReadFirstOrDefaultAsync<KYReqMaster>();
                Guard.Against.NullObject(data);

                data.Childs = records.Read<KYReqChild>().ToList();
                data.KYReqBuyerTeams = records.Read<KYReqBuyerTeam>().ToList();
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
        public async Task SaveAsync(KYReqMaster entity)
        {
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                int maxChildId = 0;
                switch (entity.EntityState)
                {
                    case EntityState.Added:
                        entity.KYReqMasterID = await _service.GetMaxIdAsync(TableNames.KY_Req_Master);

                        if (!entity.IsAdditional)
                        {
                            entity.KYReqNo =await _service.GetMaxNoAsync(TableNames.KY_Req_No);
                        }
                        else
                        {
                            if (entity.ParentKYReqNo.IsNotNullOrEmpty())
                            {
                                string parentKYReqNo = entity.ParentKYReqNo;
                                parentKYReqNo = parentKYReqNo.Split('-')[0];
                                int maxCount = await this.GetMaxKYReqNo(parentKYReqNo);
                                entity.KYReqNo = parentKYReqNo + "-Add-" + maxCount;
                            }
                        }

                        maxChildId = await _service.GetMaxIdAsync(TableNames.KY_Req_Child, entity.Childs.Count);
                        foreach (var item in entity.Childs)
                        {
                            item.KYReqChildID = maxChildId++;
                            item.KYReqMasterID = entity.KYReqMasterID;
                        }
                        break;

                    case EntityState.Modified:
                        var addedChilds = entity.Childs.FindAll(x => x.EntityState == EntityState.Added);
                        maxChildId = await _service.GetMaxIdAsync(TableNames.KY_Req_Child, addedChilds.Count);
                        foreach (var item in addedChilds)
                        {
                            item.KYReqChildID = maxChildId++;
                            item.KYReqMasterID = entity.KYReqMasterID;
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

                //var childs = entity.Childs.Where(x => x.ItemMasterID == 0).ToList();
                //if (childs.Count() > 0)
                //{
                //    throw new Exception("Item missing => SaveAsync => KYReqService");
                //}

                await _service.SaveSingleAsync(entity, transaction);
                await _service.SaveAsync(entity.Childs, transaction);

                int userId = entity.EntityState == EntityState.Added ? entity.AddedBy : (int)entity.UpdatedBy;
               // await _service.ValidationSingleAsync(entity, transaction, "sp_Validation_KYReqMaster", entity.EntityState, userId, entity.KYReqMasterID);
                await _connection.ExecuteAsync(SPNames.sp_Validation_KYReqMaster, new { PrimaryKeyId = entity.KYReqMasterID, UserId = userId, EntityState = entity.EntityState }, transaction, 30, CommandType.StoredProcedure);

                foreach (KYReqChild item in entity.Childs.Where(x => x.EntityState == EntityState.Added || x.EntityState == EntityState.Modified))
                {
                   // await _service.ValidationSingleAsync(item, transaction, "sp_Validation_KYReqChild", item.EntityState, userId, item.KYReqChildID);
                    await _connection.ExecuteAsync(SPNames.sp_Validation_KYReqChild, new { PrimaryKeyId = item.KYReqChildID, UserId = userId, EntityState = item.EntityState }, transaction, 30, CommandType.StoredProcedure);
                }

                if (entity.Approve && entity.Childs[0].YDItem == false && entity.Acknowledge == false)
                {
                    if (entity.ApproveBy.IsNull()) entity.ApproveBy = 0;
                    userId = entity.EntityState == EntityState.Added ? entity.AddedBy : (int)entity.ApproveBy;
                   // await _connection.ExecuteAsync("spYarnStockOperation", new { MasterID = entity.KYReqMasterID, FromMenuType = EnumFromMenuType.KYReqMasterApp, UserId = userId }, transaction, 30, CommandType.StoredProcedure);
                    await _connection.ExecuteAsync(SPNames.spYarnStockOperation, new { MasterID = entity.KYReqMasterID, FromMenuType = EnumFromMenuType.RnDYarnRequisitionApp, UserId = userId }, transaction, 30, CommandType.StoredProcedure);

                }

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

        public async Task ReviseAsync(KYReqMaster entity)
        {
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                //await _connection.ExecuteAsync("spBackupYarnRndRequisition_Full", new { KYReqMasterID = entity.KYReqMasterID }, transaction, 30, CommandType.StoredProcedure);

                int maxChildId = 0;
                switch (entity.EntityState)
                {
                    case EntityState.Added:
                        entity.KYReqMasterID = await _service.GetMaxIdAsync(TableNames.KY_Req_Master);
                        entity.KYReqNo =await _service.GetMaxNoAsync(TableNames.KY_Req_No);

                        maxChildId = await _service.GetMaxIdAsync(TableNames.KY_Req_Child, entity.Childs.Count);
                        foreach (var item in entity.Childs)
                        {
                            item.KYReqChildID = maxChildId++;
                            item.KYReqMasterID = entity.KYReqMasterID;
                        }
                        break;

                    case EntityState.Modified:
                        var addedChilds = entity.Childs.FindAll(x => x.EntityState == EntityState.Added);
                        maxChildId = await _service.GetMaxIdAsync(TableNames.KY_Req_Child, addedChilds.Count);
                        foreach (var item in addedChilds)
                        {
                            item.KYReqChildID = maxChildId++;
                            item.KYReqMasterID = entity.KYReqMasterID;
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
                    throw new Exception("Item missing => ReviseAsync => KYReqService");
                }


                await _service.SaveSingleAsync(entity, transaction);
                await _service.SaveAsync(entity.Childs, transaction);

                transaction.Commit();
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                throw ex;
            }
            finally
            {
                transaction.Dispose();
                _connection.Close();
            }
        }
        private async Task<int> GetMaxKYReqNo(string KYReqNo)
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["TexConnection"].ConnectionString;
            var queryString = $"SELECT MaxValue=COUNT(*) FROM KYReqMaster WHERE KYReqNo LIKE '{KYReqNo}%'";

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
