using Dapper;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using System.Data;
using System.Data.Entity;
using Microsoft.Data.SqlClient;
using EPYSLTEXCore.Application.Interfaces.Inventory.Yarn.Knitting;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn.Knitting;

namespace EPYSLTEXCore.Application.Services.Inventory.Knitting
{
    public class KnittingSubContractIssueService : IKnittingSubContractIssueService
    {
        private readonly IDapperCRUDService<KnittingSubContractIssueMaster> _service;
        private readonly SqlConnection _connection;
        private readonly SqlConnection _connectionGmt;

        public KnittingSubContractIssueService(IDapperCRUDService<KnittingSubContractIssueMaster> service)
        {
            _service = service;
            _service.Connection = service.GetConnection(AppConstants.GMT_CONNECTION);
            _connectionGmt = service.Connection;

            _service.Connection = service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _connection = service.Connection;
        }

        public async Task<List<KnittingSubContractIssueMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By KSCIssueMasterID Desc" : paginationInfo.OrderBy;
            var sql = string.Empty;

            switch (status)
            {
                case Status.Draft:
                    sql += $@"
                      ;With M As (
	                    Select KSCIssueMasterID, KSCIssueNo, KSCIssueDate, KSCIssueByID, KSCMasterID, KSCReqMasterID, Remarks, ChallanNo, GPNo = ''
	                    FROM {TableNames.KNITTING_SUB_CONTRACT_ISSUE_MASTER} Where IsSendForApprove=0
                    ),
					RQ As(
					Select M.*, ReqQty = SUM(KSCRC.ReqQty) 
					FROM {TableNames.KNITTING_SUB_CONTRACT_REQ_CHILD} KSCRC
					INNER JOIN M ON M.KSCReqMasterID = KSCRC.KSCReqMasterID
					GROUP BY M.KSCIssueMasterID, M.KSCIssueNo, M.KSCIssueDate, M.KSCIssueByID, M.KSCMasterID, M.KSCReqMasterID, M.Remarks, M.ChallanNo, M.GPNo
					),
					MD AS(
                    Select RQ.KSCIssueMasterID, RQ.KSCIssueNo, RQ.KSCIssueDate, RQ.KSCIssueByID, RQ.KSCReqMasterID, RQ.Remarks,
					KSRC.KSCReqNo, KSRC.KSCReqDate,BookingQty = SUM(KSC.BookingQty), SCQty = SUM(KSC.SCQty),FCM.GroupConceptNo, 
					ProgramName = CASE WHEN ISNULL(RC.YBChildItemID,0) > 0 THEN 'BULK' ELSE 'RND' END,
                    ReqType = 'SC', KSM.KSCNo, LU.UserName ReqByUser,
				    (CASE WHEN FCM.ConceptID > 0 THEN FCM.GroupConceptNo  ELSE KPM.YBookingNo END) AS CorBookingNo,
				    FCM.SubGroupID, ISG.SubGroupName, --KT.TypeName KnittingType, Technical.TechnicalName, 
                    --ISV.SegmentValue Composition, GSV.SegmentValue Gsm, FCM.Length, FCM.Width, 
					CE2.ShortName KSCUnit, Company = Case when CM.ShortName is not null then CM.ShortName else CM2.ShortName end,
					RQ.ChallanNo, RQ.GPNo, RQ.ReqQty
                    From RQ
					Inner JOIN {TableNames.KNITTING_SUB_CONTRACT_REQ_MASTER} KSRC On KSRC.KSCReqMasterID = RQ.KSCReqMasterID
                    INNER JOIN {TableNames.KNITTING_SUB_CONTRACT_REQ_CHILD} RC ON RC.KSCReqMasterID = KSRC.KSCReqMasterID AND KSRC.IsApprove = 1 AND KSRC.IsReject = 0 
					LEFT JOIN {TableNames.KNITTING_SUB_CONTRACT_CHILD} KSC ON KSRC.KSCMasterID=KSC.KSCMasterID
                    LEFT JOIN {TableNames.KNITTING_SUB_CONTRACT_MASTER} KSM ON KSM.KSCMasterID=KSC.KSCMasterID
					LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = KSC.ConceptID
					LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPChildID=KSC.KPChildID
					LEFT JOIN {TableNames.Knitting_Plan_Master} KPM ON KPM.KPMasterID=KSC.KPMasterID
					LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KT ON KT.TypeID = FCM.KnittingTypeID 
                    LEFT JOIN {TableNames.FabricTechnicalName} Technical ON Technical.TechnicalNameId = FCM.TechnicalNameId
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = FCM.CompositionID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue GSV ON GSV.SegmentValueID = FCM.GSMID 
                    LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID = FCM.SubGroupID
	                Inner Join {DbNames.EPYSL}..LoginUser LU On KSRC.KSCReqByID = LU.UserCode
                    LEFT JOIN {TableNames.KNITTING_MACHINE} KM ON KM.KnittingMachineID = KSM.KnittingMachineID
	                LEFT JOIN {TableNames.KNITTING_UNIT} KU ON KU.KnittingUnitID = KM.KnittingUnitID
					Left Join {DbNames.EPYSL}..BookingMaster BM On BM.BookingNo = KSM.BookingNo
					Left Join {DbNames.EPYSL}..SampleBookingMaster SBM On SBM.BookingNo = KSM.BookingNo
					LEFT JOIN {DbNames.EPYSL}..Contacts CM ON CM.ContactID = BM.SupplierID
					LEFT JOIN {DbNames.EPYSL}..Contacts CM2 ON CM2.ContactID = SBM.SupplierID
					Left Join {DbNames.EPYSL}..Contacts CE2 On CE2.ContactID = KSM.SubContactID
					GROUP BY RQ.KSCIssueMasterID, RQ.KSCIssueNo, RQ.KSCIssueDate, RQ.KSCIssueByID, RQ.KSCReqMasterID, RQ.Remarks,
					KSRC.KSCReqNo, KSRC.KSCReqDate,FCM.GroupConceptNo, 
					CASE WHEN ISNULL(RC.YBChildItemID,0) > 0 THEN 'BULK' ELSE 'RND' END,
                    KSM.KSCNo, LU.UserName,
				    (CASE WHEN FCM.ConceptID > 0 THEN FCM.GroupConceptNo  ELSE KPM.YBookingNo END),
				    FCM.SubGroupID, ISG.SubGroupName, 
					--KT.TypeName, Technical.TechnicalName, ISV.SegmentValue, 
					CE2.ShortName, 
					Case when CM.ShortName is not null then CM.ShortName else CM2.ShortName end,
					RQ.ChallanNo, RQ.GPNo, RQ.ReqQty
					),
					MY As (
	                    Select YDReqIssueMasterID, YDReqIssueNo, YDReqIssueDate, YDReqIssueBy, YDReqMasterID, Remarks, ChallanNo, GPNo = ''
	                   FROM {TableNames.YD_REQ_ISSUE_MASTER} Where IsSendForApprove=0
                    ),
					RQY As(
					Select MY.*, ReqQty = SUM(KSCRC.ReqQty) 
					FROM {TableNames.YD_REQ_CHILD} KSCRC
					INNER JOIN MY ON MY.YDReqMasterID = KSCRC.YDReqMasterID
					GROUP BY MY.YDReqIssueMasterID, MY.YDReqIssueNo, MY.YDReqIssueDate, MY.YDReqIssueBy, MY.YDReqMasterID, MY.Remarks, MY.ChallanNo, MY.GPNo
					),
					MDY AS(
                    Select RQY.YDReqIssueMasterID, RQY.YDReqIssueNo, RQY.YDReqIssueDate, RQY.YDReqIssueBy, RQY.YDReqMasterID, RQY.Remarks,
					YRM.YDReqNo, YRM.YDReqDate,Sum(YBC.BookingQty)BookingQty,sum(YRC.ReqQty)ReqQty, ConceptNo = YBM.GroupConceptNo, 
					ProgramName = CASE WHEN ISNULL(YBM.ExportOrderID,0) > 0 THEN 'BULK' ELSE 'RND' END, ReqType = 'YD', YBM.YDBookingNo, LU.UserName ReqByUser,
				    YBM.GroupConceptNo AS CorBookingNo,
				    FCM.SubGroupID, ISG.SubGroupName, --KT.TypeName KnittingType, Technical.TechnicalName, 
                    --ISV.SegmentValue Composition, GSV.SegmentValue Gsm, FCM.Length, FCM.Width,
					'' KSCUnit, Company = Case when FCM.IsBDS=0 then 'EFL' else Case when isnull(CM.ShortName,'')<>'' then CM.ShortName else CM2.ShortName end end,
					RQY.ChallanNo, RQY.GPNo, RQY.ReqQty TReqQty
                    From RQY
					Inner JOIN {TableNames.YD_REQ_MASTER} YRM On YRM.YDReqMasterID = RQY.YDReqMasterID
					Inner JOIN {TableNames.YD_REQ_CHILD} YRC On YRC.YDReqMasterID = YRM.YDReqMasterID
					LEFT JOIN {TableNames.YDBookingChild} YBC ON YBC.YDBookingMasterID=YRM.YDBookingMasterID
					LEFT JOIN {TableNames.YD_BOOKING_MASTER} YBM ON YBM.YDBookingMasterID=YBC.YDBookingMasterID
					LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = YBM.ConceptID
					--LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPChildID=KSC.KPChildID
					--LEFT JOIN {TableNames.Knitting_Plan_Master} KPM ON KPM.KPMasterID=KSC.KPMasterID
					LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KT ON KT.TypeID = FCM.KnittingTypeID 
                    LEFT JOIN {TableNames.FabricTechnicalName} Technical ON Technical.TechnicalNameId = FCM.TechnicalNameId
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = FCM.CompositionID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue GSV ON GSV.SegmentValueID = FCM.GSMID 
                    LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID = FCM.SubGroupID
	                Inner Join {DbNames.EPYSL}..LoginUser LU On YRM.YDReqBy = LU.UserCode
                    Left Join {DbNames.EPYSL}..BookingMaster BM On BM.BookingNo = YBM.GroupConceptNo
					Left Join {DbNames.EPYSL}..SampleBookingMaster SBM On SBM.BookingNo = YBM.GroupConceptNo
					LEFT JOIN {DbNames.EPYSL}..Contacts CM ON CM.ContactID = BM.SupplierID
					LEFT JOIN {DbNames.EPYSL}..Contacts CM2 ON CM2.ContactID = SBM.SupplierID   
					GROUP BY RQY.YDReqIssueMasterID, RQY.YDReqIssueNo, RQY.YDReqIssueDate, RQY.YDReqIssueBy, RQY.YDReqMasterID, RQY.Remarks,
					YRM.YDReqNo, YRM.YDReqDate,FCM.ConceptNo,YBM.GroupConceptNo,FCM.SubGroupID, ISG.SubGroupName, 
					--KT.TypeName, Technical.TechnicalName, ISV.SegmentValue, 
					ISNULL(YBM.ExportOrderID,0), YBM.YDBookingNo, LU.UserName,
					FCM.IsBDS, CM.ShortName, CM2.ShortName, RQY.ChallanNo, RQY.GPNo, RQY.ReqQty
					),
					Final AS(
					Select * from MD 
					Union
					Select * from MDY
					)
					Select *,Count(*) Over() TotalRows  from Final";
                    break;
                case Status.ProposedForApproval:
                    sql += $@"
                    ;With M As (
	                    Select KSCIssueMasterID, KSCIssueNo, KSCIssueDate, KSCIssueByID, KSCMasterID, KSCReqMasterID, Remarks, ChallanNo, GPNo = ''
	                    FROM {TableNames.KNITTING_SUB_CONTRACT_ISSUE_MASTER} 
                        Where IsSendForApprove=1 AND IsApprove=0 AND IsReject=0
                    ),
					MD AS(
                        Select M.KSCIssueMasterID, M.KSCIssueNo, M.KSCIssueDate, M.KSCIssueByID, M.KSCReqMasterID, M.Remarks,
					    KSRC.KSCReqNo, KSRC.KSCReqDate, BookingQty = SUM(KSC.BookingQty), SCQty = SUM(KSC.SCQty), ConceptNo = FCM.GroupConceptNo, 
					    ProgramName = case when FCM.IsBDS=0 Then 'RND' When FCM.IsBDS=1 Then 'RND' When FCM.IsBDS=2 Then 'Bulk' Else '' End,
                        ReqType = 'SC', KSM.KSCNo, LU.UserName ReqByUser, 
				        (CASE WHEN FCM.ConceptID > 0 THEN FCM.GroupConceptNo  ELSE KPM.YBookingNo END) AS CorBookingNo,
				        FCM.SubGroupID, ISG.SubGroupName, 
						--KT.TypeName KnittingType, Technical.TechnicalName, ISV.SegmentValue Composition, GSV.SegmentValue Gsm, FCM.Length, FCM.Width, 
						CE2.ShortName KSCUnit, 
					    Case when CM.ShortName is not null then CM.ShortName else CM2.ShortName end Company,
						M.ChallanNo, M.GPNo
                        From M
					    Inner JOIN {TableNames.KNITTING_SUB_CONTRACT_REQ_MASTER} KSRC On KSRC.KSCReqMasterID = M.KSCReqMasterID
					    LEFT JOIN {TableNames.KNITTING_SUB_CONTRACT_CHILD} KSC ON KSRC.KSCMasterID=KSC.KSCMasterID
                        LEFT JOIN {TableNames.KNITTING_SUB_CONTRACT_MASTER} KSM ON KSM.KSCMasterID=KSC.KSCMasterID
					    LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = KSC.ConceptID
					    LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPChildID=KSC.KPChildID
					    LEFT JOIN {TableNames.Knitting_Plan_Master} KPM ON KPM.KPMasterID=KSC.KPMasterID
					    LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KT ON KT.TypeID = FCM.KnittingTypeID 
                        LEFT JOIN {TableNames.FabricTechnicalName} Technical ON Technical.TechnicalNameId = FCM.TechnicalNameId
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = FCM.CompositionID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue GSV ON GSV.SegmentValueID = FCM.GSMID 
                        LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID = FCM.SubGroupID
	                    Inner Join {DbNames.EPYSL}..LoginUser LU On KSRC.KSCReqByID = LU.UserCode
                        LEFT JOIN {TableNames.KNITTING_MACHINE} KM ON KM.KnittingMachineID = KSM.KnittingMachineID
	                    LEFT JOIN {TableNames.KNITTING_UNIT} KU ON KU.KnittingUnitID = KM.KnittingUnitID
						Left Join {DbNames.EPYSL}..BookingMaster BM On BM.BookingNo = KSM.BookingNo
						Left Join {DbNames.EPYSL}..SampleBookingMaster SBM On SBM.BookingNo = KSM.BookingNo
						LEFT JOIN {DbNames.EPYSL}..Contacts CM ON CM.ContactID = BM.SupplierID
						LEFT JOIN {DbNames.EPYSL}..Contacts CM2 ON CM2.ContactID = SBM.SupplierID
						Left Join {DbNames.EPYSL}..Contacts CE2 On CE2.ContactID = KSM.SubContactID
						GROUP BY M.KSCIssueMasterID, M.KSCIssueNo, M.KSCIssueDate, M.KSCIssueByID, M.KSCReqMasterID, M.Remarks,
					    KSRC.KSCReqNo, KSRC.KSCReqDate, FCM.GroupConceptNo, 
					    case when FCM.IsBDS=0 Then 'RND' When FCM.IsBDS=1 Then 'RND' When FCM.IsBDS=2 Then 'Bulk' Else '' End,
                        KSM.KSCNo, LU.UserName, 
				        (CASE WHEN FCM.ConceptID > 0 THEN FCM.GroupConceptNo  ELSE KPM.YBookingNo END),
				        FCM.SubGroupID, ISG.SubGroupName, CE2.ShortName, 
					    Case when CM.ShortName is not null then CM.ShortName else CM2.ShortName end,
						M.ChallanNo, M.GPNo
					),
					MY As (
	                    Select YDReqIssueMasterID, YDReqIssueNo, YDReqIssueDate, YDReqIssueBy, YDReqMasterID, Remarks, ChallanNo, GPNo = ''
	                    FROM {TableNames.YD_REQ_ISSUE_MASTER} 
                        Where IsSendForApprove=1 AND IsApprove=0 AND IsReject=0
                    ),
					MDY AS(
                        Select MY.YDReqIssueMasterID, MY.YDReqIssueNo, MY.YDReqIssueDate, MY.YDReqIssueBy, MY.YDReqMasterID, MY.Remarks,
					    YRM.YDReqNo, YRM.YDReqDate, BookingQty = Sum(YBC.BookingQty), ReqQty = sum(YRC.ReqQty), ConceptNo = YBM.GroupConceptNo, 
					    ProgramName = CASE WHEN ISNULL(YBM.ExportOrderID,0) > 0 THEN 'BULK' ELSE 'RND' END, ReqType = 'YD', YBM.YDBookingNo, LU.UserName ReqByUser,
				        YBM.GroupConceptNo AS CorBookingNo,
				        FCM.SubGroupID, ISG.SubGroupName, 
						--KT.TypeName KnittingType, Technical.TechnicalName, ISV.SegmentValue Composition, GSV.SegmentValue Gsm, FCM.Length, FCM.Width,
						KSCUnit = '',
						Company = Case when FCM.IsBDS=0 then 'EFL' else Case when isnull(CM.ShortName,'')<>'' then CM.ShortName else CM2.ShortName end end  ,
						MY.ChallanNo, MY.GPNo
                        From MY
					    Inner JOIN {TableNames.YD_REQ_MASTER} YRM On YRM.YDReqMasterID = MY.YDReqMasterID
					    Inner JOIN {TableNames.YD_REQ_CHILD} YRC On YRC.YDReqMasterID = YRM.YDReqMasterID
					    LEFT JOIN {TableNames.YDBookingChild} YBC ON YBC.YDBookingMasterID=YRM.YDBookingMasterID
					    LEFT JOIN {TableNames.YD_BOOKING_MASTER} YBM ON YBM.YDBookingMasterID=YBC.YDBookingMasterID
					    LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = YBM.ConceptID
					    --LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPChildID=KSC.KPChildID
					    --LEFT JOIN {TableNames.Knitting_Plan_Master} KPM ON KPM.KPMasterID=KSC.KPMasterID
					    LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KT ON KT.TypeID = FCM.KnittingTypeID 
                        LEFT JOIN {TableNames.FabricTechnicalName} Technical ON Technical.TechnicalNameId = FCM.TechnicalNameId
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = FCM.CompositionID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue GSV ON GSV.SegmentValueID = FCM.GSMID 
                        LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID = FCM.SubGroupID
	                    Inner Join {DbNames.EPYSL}..LoginUser LU On YRM.YDReqBy = LU.UserCode
                        Left Join {DbNames.EPYSL}..BookingMaster BM On BM.BookingNo = YBM.GroupConceptNo
						Left Join {DbNames.EPYSL}..SampleBookingMaster SBM On SBM.BookingNo = YBM.GroupConceptNo
						LEFT JOIN {DbNames.EPYSL}..Contacts CM ON CM.ContactID = BM.SupplierID
						LEFT JOIN {DbNames.EPYSL}..Contacts CM2 ON CM2.ContactID = SBM.SupplierID   
					    GROUP BY MY.YDReqIssueMasterID, MY.YDReqIssueNo, MY.YDReqIssueDate, MY.YDReqIssueBy, MY.YDReqMasterID, MY.Remarks,
					    YRM.YDReqNo, YRM.YDReqDate,FCM.ConceptNo,YBM.GroupConceptNo,FCM.SubGroupID, ISG.SubGroupName, 
						--KT.TypeName, Technical.TechnicalName, ISV.SegmentValue, GSV.SegmentValue, FCM.Length, FCM.Width, 
						ISNULL(YBM.ExportOrderID,0), YBM.YDBookingNo, LU.UserName,
						FCM.IsBDS, CM.ShortName, CM2.ShortName, MY.ChallanNo, MY.GPNo
					),
					Final AS(
					    Select * from MD 
					    Union
					    Select * from MDY
					)
					Select *,Count(*) Over() TotalRows from Final  ";
                    break;
                case Status.Approved:
                    sql += $@"
                    ;With M As (
	                    Select KSCIssueMasterID, KSCIssueNo, KSCIssueDate, KSCIssueByID, KSCMasterID, KSCReqMasterID, Remarks, ChallanNo, GPNo = ''
	                    FROM {TableNames.KNITTING_SUB_CONTRACT_ISSUE_MASTER} 
                        Where IsSendForApprove=1 AND IsApprove=1 AND IsReject=0 AND IsGPApprove=0
                    ),
					MD AS(
                        Select M.KSCIssueMasterID, M.KSCIssueNo, M.KSCIssueDate, M.KSCIssueByID, M.KSCReqMasterID, M.Remarks,
					    KSRC.KSCReqNo, KSRC.KSCReqDate,KSC.BookingQty,KSC.SCQty,KSC.Rate,FCM.ConceptNo, 
					    (Case When KPM.ConceptID > 0 Then 'RND' When KPM.ExportOrderID > 0 Then 'Bulk' Else 'RND' End) ProgramName,
                        ReqType = 'SC', KSM.KSCNo, LU.UserName ReqByUser, 
				        (CASE WHEN FCM.ConceptID > 0 THEN FCM.ConceptNo  ELSE KPM.YBookingNo END) AS CorBookingNo,
				        FCM.SubGroupID, ISG.SubGroupName, KT.TypeName KnittingType, Technical.TechnicalName, 
                        ISV.SegmentValue Composition, GSV.SegmentValue Gsm, FCM.Length, FCM.Width, CE2.ShortName KSCUnit, 
					    Case when CM.ShortName is not null then CM.ShortName else CM2.ShortName end Company,
						M.ChallanNo, M.GPNo
                        From M
					    Inner JOIN {TableNames.KNITTING_SUB_CONTRACT_REQ_MASTER} KSRC On KSRC.KSCReqMasterID = M.KSCReqMasterID
					    LEFT JOIN {TableNames.KNITTING_SUB_CONTRACT_CHILD} KSC ON KSRC.KSCMasterID=KSC.KSCMasterID
                        LEFT JOIN {TableNames.KNITTING_SUB_CONTRACT_MASTER} KSM ON KSM.KSCMasterID=KSC.KSCMasterID
					    LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = KSC.ConceptID
					    LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPChildID=KSC.KPChildID
					    LEFT JOIN {TableNames.Knitting_Plan_Master} KPM ON KPM.KPMasterID=KSC.KPMasterID
					    LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KT ON KT.TypeID = FCM.KnittingTypeID 
                        LEFT JOIN {TableNames.FabricTechnicalName} Technical ON Technical.TechnicalNameId = FCM.TechnicalNameId
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = FCM.CompositionID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue GSV ON GSV.SegmentValueID = FCM.GSMID 
                        LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID = FCM.SubGroupID
	                    Inner Join {DbNames.EPYSL}..LoginUser LU On KSRC.KSCReqByID = LU.UserCode
                        LEFT JOIN {TableNames.KNITTING_MACHINE} KM ON KM.KnittingMachineID = KSM.KnittingMachineID
	                    LEFT JOIN {TableNames.KNITTING_UNIT} KU ON KU.KnittingUnitID = KM.KnittingUnitID
						Left Join {DbNames.EPYSL}..BookingMaster BM On BM.BookingNo = KSM.BookingNo
						Left Join {DbNames.EPYSL}..SampleBookingMaster SBM On SBM.BookingNo = KSM.BookingNo
						LEFT JOIN {DbNames.EPYSL}..Contacts CM ON CM.ContactID = BM.SupplierID
						LEFT JOIN {DbNames.EPYSL}..Contacts CM2 ON CM2.ContactID = SBM.SupplierID
						Left Join {DbNames.EPYSL}..Contacts CE2 On CE2.ContactID = KSM.SubContactID
					),
					MY As (
	                    Select YDReqIssueMasterID, YDReqIssueNo, YDReqIssueDate, YDReqIssueBy, YDReqMasterID, Remarks, ChallanNo, GPNo = ''
	                    FROM {TableNames.YD_REQ_ISSUE_MASTER} 
                        Where IsApprove=1 AND IsReject=0 AND IsGPApprove=0
                    ),
					MDY AS(
                        Select MY.YDReqIssueMasterID, MY.YDReqIssueNo, MY.YDReqIssueDate, MY.YDReqIssueBy, MY.YDReqMasterID, MY.Remarks,
					    YRM.YDReqNo, YRM.YDReqDate,Sum(YBC.BookingQty)BookingQty,sum(YRC.ReqQty)ReqQty,0 Rate, ConceptNo = YBM.GroupConceptNo,  
					    ProgramName = CASE WHEN ISNULL(YBM.ExportOrderID,0) > 0 THEN 'BULK' ELSE 'RND' END, ReqType = 'YD', YBM.YDBookingNo, LU.UserName ReqByUser,
				        YBM.GroupConceptNo AS CorBookingNo,
				        FCM.SubGroupID, ISG.SubGroupName, KT.TypeName KnittingType, Technical.TechnicalName, 
                        ISV.SegmentValue Composition, GSV.SegmentValue Gsm, FCM.Length, FCM.Width,'' KSCUnit,
						Company = Case when FCM.IsBDS=0 then 'EFL' else Case when isnull(CM.ShortName,'')<>'' then CM.ShortName else CM2.ShortName end end,
						MY.ChallanNo, MY.GPNo
                        From MY
					    Inner JOIN {TableNames.YD_REQ_MASTER} YRM On YRM.YDReqMasterID = MY.YDReqMasterID
					    Inner JOIN {TableNames.YD_REQ_CHILD} YRC On YRC.YDReqMasterID = YRM.YDReqMasterID
					    LEFT JOIN {TableNames.YDBookingChild} YBC ON YBC.YDBookingMasterID=YRM.YDBookingMasterID
					    LEFT JOIN {TableNames.YD_BOOKING_MASTER} YBM ON YBM.YDBookingMasterID=YBC.YDBookingMasterID
					    LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = YBM.ConceptID
					    --LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPChildID=KSC.KPChildID
					    --LEFT JOIN {TableNames.Knitting_Plan_Master} KPM ON KPM.KPMasterID=KSC.KPMasterID
					    LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KT ON KT.TypeID = FCM.KnittingTypeID 
                        LEFT JOIN {TableNames.FabricTechnicalName} Technical ON Technical.TechnicalNameId = FCM.TechnicalNameId
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = FCM.CompositionID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue GSV ON GSV.SegmentValueID = FCM.GSMID 
                        LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID = FCM.SubGroupID
	                    Inner Join {DbNames.EPYSL}..LoginUser LU On YRM.YDReqBy = LU.UserCode
                        Left Join {DbNames.EPYSL}..BookingMaster BM On BM.BookingNo = YBM.GroupConceptNo
						Left Join {DbNames.EPYSL}..SampleBookingMaster SBM On SBM.BookingNo = YBM.GroupConceptNo
						LEFT JOIN {DbNames.EPYSL}..Contacts CM ON CM.ContactID = BM.SupplierID
						LEFT JOIN {DbNames.EPYSL}..Contacts CM2 ON CM2.ContactID = SBM.SupplierID   
					    GROUP BY MY.YDReqIssueMasterID, MY.YDReqIssueNo, MY.YDReqIssueDate, MY.YDReqIssueBy, MY.YDReqMasterID, MY.Remarks,
					    YRM.YDReqNo, YRM.YDReqDate,FCM.ConceptNo,YBM.GroupConceptNo,FCM.SubGroupID, ISG.SubGroupName, KT.TypeName, Technical.TechnicalName, 
                        ISV.SegmentValue, GSV.SegmentValue, FCM.Length, FCM.Width,ISNULL(YBM.ExportOrderID,0), YBM.YDBookingNo, LU.UserName,
						FCM.IsBDS, CM.ShortName, CM2.ShortName, MY.ChallanNo, MY.GPNo
					),
					Final AS(
					    Select * from MD 
					    Union
					    Select * from MDY
					)
					Select *,Count(*) Over() TotalRows from Final ";
                    break;
                case Status.Reject:
                    sql += $@"
                    ;With M As (
	                    Select KSCIssueMasterID, KSCIssueNo, KSCIssueDate, KSCIssueByID, KSCMasterID, KSCReqMasterID, Remarks, ChallanNo, GPNo = ''
	                    FROM {TableNames.KNITTING_SUB_CONTRACT_ISSUE_MASTER} 
                        Where IsSendForApprove=1 AND IsApprove=0 AND IsReject=1
                    ),
					MD AS(
                        Select M.KSCIssueMasterID, M.KSCIssueNo, M.KSCIssueDate, M.KSCIssueByID, M.KSCReqMasterID, M.Remarks,
					    KSRC.KSCReqNo, KSRC.KSCReqDate,KSC.BookingQty,KSC.SCQty,KSC.Rate,FCM.ConceptNo, 
					    (Case When KPM.ConceptID > 0 Then 'RND' When KPM.ExportOrderID > 0 Then 'Bulk' Else 'RND' End) ProgramName,
                        ReqType = 'SC', KSM.KSCNo, LU.UserName ReqByUser, 
				        (CASE WHEN FCM.ConceptID > 0 THEN FCM.ConceptNo  ELSE KPM.YBookingNo END) AS CorBookingNo,
				        FCM.SubGroupID, ISG.SubGroupName, KT.TypeName KnittingType, Technical.TechnicalName, 
                        ISV.SegmentValue Composition, GSV.SegmentValue Gsm, FCM.Length, FCM.Width, CE2.ShortName KSCUnit, 
					    Case when CM.ShortName is not null then CM.ShortName else CM2.ShortName end Company,
						M.ChallanNo, M.GPNo
                        From M
					    Inner JOIN {TableNames.KNITTING_SUB_CONTRACT_REQ_MASTER} KSRC On KSRC.KSCReqMasterID = M.KSCReqMasterID
					    LEFT JOIN {TableNames.KNITTING_SUB_CONTRACT_CHILD} KSC ON KSRC.KSCMasterID=KSC.KSCMasterID
                        LEFT JOIN {TableNames.KNITTING_SUB_CONTRACT_MASTER} KSM ON KSM.KSCMasterID=KSC.KSCMasterID
					    LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = KSC.ConceptID
					    LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPChildID=KSC.KPChildID
					    LEFT JOIN {TableNames.Knitting_Plan_Master} KPM ON KPM.KPMasterID=KSC.KPMasterID
					    LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KT ON KT.TypeID = FCM.KnittingTypeID 
                        LEFT JOIN {TableNames.FabricTechnicalName} Technical ON Technical.TechnicalNameId = FCM.TechnicalNameId
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = FCM.CompositionID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue GSV ON GSV.SegmentValueID = FCM.GSMID 
                        LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID = FCM.SubGroupID 
	                    Inner Join {DbNames.EPYSL}..LoginUser LU On KSRC.KSCReqByID = LU.UserCode
                        LEFT JOIN {TableNames.KNITTING_MACHINE} KM ON KM.KnittingMachineID = KSM.KnittingMachineID
	                    LEFT JOIN {TableNames.KNITTING_UNIT} KU ON KU.KnittingUnitID = KM.KnittingUnitID
						Left Join {DbNames.EPYSL}..BookingMaster BM On BM.BookingNo = KSM.BookingNo
						Left Join {DbNames.EPYSL}..SampleBookingMaster SBM On SBM.BookingNo = KSM.BookingNo
						LEFT JOIN {DbNames.EPYSL}..Contacts CM ON CM.ContactID = BM.SupplierID
						LEFT JOIN {DbNames.EPYSL}..Contacts CM2 ON CM2.ContactID = SBM.SupplierID
						Left Join {DbNames.EPYSL}..Contacts CE2 On CE2.ContactID = KSM.SubContactID
					),
					MY As (
	                    Select YDReqIssueMasterID, YDReqIssueNo, YDReqIssueDate, YDReqIssueBy, YDReqMasterID, Remarks, ChallanNo, GPNo = ''
	                    FROM {TableNames.YD_REQ_ISSUE_MASTER} 
                        Where IsSendForApprove=1 AND IsApprove=0 AND IsReject=1
                    ),
					MDY AS(
                        Select MY.YDReqIssueMasterID, MY.YDReqIssueNo, MY.YDReqIssueDate, MY.YDReqIssueBy, MY.YDReqMasterID, MY.Remarks,
					    YRM.YDReqNo, YRM.YDReqDate,Sum(YBC.BookingQty)BookingQty,sum(YRC.ReqQty)ReqQty,0 Rate, ConceptNo = YBM.GroupConceptNo, 
					    ProgramName = CASE WHEN ISNULL(YBM.ExportOrderID,0) > 0 THEN 'BULK' ELSE 'RND' END, ReqType = 'YD', YBM.YDBookingNo, LU.UserName ReqByUser,
				        YBM.GroupConceptNo AS CorBookingNo,
				        FCM.SubGroupID, ISG.SubGroupName, KT.TypeName KnittingType, Technical.TechnicalName, 
                        ISV.SegmentValue Composition, GSV.SegmentValue Gsm, FCM.Length, FCM.Width,'' KSCUnit,
						Company = Case when FCM.IsBDS=0 then 'EFL' else Case when isnull(CM.ShortName,'')<>'' then CM.ShortName else CM2.ShortName end end,
						MY.ChallanNo, MY.GPNo
                        From MY
					    Inner JOIN {TableNames.YD_REQ_MASTER} YRM On YRM.YDReqMasterID = MY.YDReqMasterID
					    Inner JOIN {TableNames.YD_REQ_CHILD} YRC On YRC.YDReqMasterID = YRM.YDReqMasterID
					    LEFT JOIN {TableNames.YDBookingChild} YBC ON YBC.YDBookingMasterID=YRM.YDBookingMasterID
					    LEFT JOIN {TableNames.YD_BOOKING_MASTER} YBM ON YBM.YDBookingMasterID=YBC.YDBookingMasterID
					    LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = YBM.ConceptID
					    --LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPChildID=KSC.KPChildID
					    --LEFT JOIN {TableNames.Knitting_Plan_Master} KPM ON KPM.KPMasterID=KSC.KPMasterID
					    LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KT ON KT.TypeID = FCM.KnittingTypeID 
                        LEFT JOIN {TableNames.FabricTechnicalName} Technical ON Technical.TechnicalNameId = FCM.TechnicalNameId
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = FCM.CompositionID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue GSV ON GSV.SegmentValueID = FCM.GSMID 
                        LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID = FCM.SubGroupID
	                    Inner Join {DbNames.EPYSL}..LoginUser LU On YRM.YDReqBy = LU.UserCode
                        Left Join {DbNames.EPYSL}..BookingMaster BM On BM.BookingNo = YBM.GroupConceptNo
						Left Join {DbNames.EPYSL}..SampleBookingMaster SBM On SBM.BookingNo = YBM.GroupConceptNo
						LEFT JOIN {DbNames.EPYSL}..Contacts CM ON CM.ContactID = BM.SupplierID
						LEFT JOIN {DbNames.EPYSL}..Contacts CM2 ON CM2.ContactID = SBM.SupplierID   
					    GROUP BY MY.YDReqIssueMasterID, MY.YDReqIssueNo, MY.YDReqIssueDate, MY.YDReqIssueBy, MY.YDReqMasterID, MY.Remarks,
					    YRM.YDReqNo, YRM.YDReqDate,FCM.ConceptNo,YBM.GroupConceptNo,FCM.SubGroupID, ISG.SubGroupName, KT.TypeName, Technical.TechnicalName, 
                        ISV.SegmentValue, GSV.SegmentValue, FCM.Length, FCM.Width, ISNULL(YBM.ExportOrderID,0), YBM.YDBookingNo, LU.UserName,
						FCM.IsBDS, CM.ShortName, CM2.ShortName, MY.ChallanNo, MY.GPNo
					),
					Final AS(
					    Select * from MD 
					    Union
					    Select * from MDY
					)
					Select *,Count(*) Over() TotalRows from Final ";
                    break;
                case Status.Approved2:
                    sql += $@"
                    ;With M As (
	                    Select KSCIssueMasterID, KSCIssueNo, KSCIssueDate, KSCIssueByID, KSCMasterID, KSCReqMasterID, Remarks, ChallanNo, GPNo
	                    FROM {TableNames.KNITTING_SUB_CONTRACT_ISSUE_MASTER} 
                        Where IsSendForApprove=1 AND IsApprove=1 AND IsReject=0 AND IsGPApprove=1
                    ),
					MD AS(
                        Select M.KSCIssueMasterID, M.KSCIssueNo, M.KSCIssueDate, M.KSCIssueByID, M.KSCReqMasterID, M.Remarks,
					    KSRC.KSCReqNo, KSRC.KSCReqDate,KSC.BookingQty,KSC.SCQty,KSC.Rate,FCM.ConceptNo, 
					    (Case When KPM.ConceptID > 0 Then 'RND' When KPM.ExportOrderID > 0 Then 'Bulk' Else 'RND' End) ProgramName,
                        ReqType = 'SC', KSM.KSCNo, LU.UserName ReqByUser, 
				        (CASE WHEN FCM.ConceptID > 0 THEN FCM.ConceptNo  ELSE KPM.YBookingNo END) AS CorBookingNo,
				        FCM.SubGroupID, ISG.SubGroupName, KT.TypeName KnittingType, Technical.TechnicalName, 
                        ISV.SegmentValue Composition, GSV.SegmentValue Gsm, FCM.Length, FCM.Width, CE2.ShortName KSCUnit, 
					    Case when CM.ShortName is not null then CM.ShortName else CM2.ShortName end Company,
                        M.ChallanNo, M.GPNo
                        From M
					    Inner JOIN {TableNames.KNITTING_SUB_CONTRACT_REQ_MASTER} KSRC On KSRC.KSCReqMasterID = M.KSCReqMasterID
					    LEFT JOIN {TableNames.KNITTING_SUB_CONTRACT_CHILD} KSC ON KSRC.KSCMasterID=KSC.KSCMasterID
                        LEFT JOIN {TableNames.KNITTING_SUB_CONTRACT_MASTER} KSM ON KSM.KSCMasterID=KSC.KSCMasterID
					    LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = KSC.ConceptID
					    LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPChildID=KSC.KPChildID
					    LEFT JOIN {TableNames.Knitting_Plan_Master} KPM ON KPM.KPMasterID=KSC.KPMasterID
					    LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KT ON KT.TypeID = FCM.KnittingTypeID 
                        LEFT JOIN {TableNames.FabricTechnicalName} Technical ON Technical.TechnicalNameId = FCM.TechnicalNameId
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = FCM.CompositionID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue GSV ON GSV.SegmentValueID = FCM.GSMID 
                        LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID = FCM.SubGroupID
	                    Inner Join {DbNames.EPYSL}..LoginUser LU On KSRC.KSCReqByID = LU.UserCode
                        LEFT JOIN {TableNames.KNITTING_MACHINE} KM ON KM.KnittingMachineID = KSM.KnittingMachineID
	                    LEFT JOIN {TableNames.KNITTING_UNIT} KU ON KU.KnittingUnitID = KM.KnittingUnitID
						Left Join {DbNames.EPYSL}..BookingMaster BM On BM.BookingNo = KSM.BookingNo
						Left Join {DbNames.EPYSL}..SampleBookingMaster SBM On SBM.BookingNo = KSM.BookingNo
						LEFT JOIN {DbNames.EPYSL}..Contacts CM ON CM.ContactID = BM.SupplierID
						LEFT JOIN {DbNames.EPYSL}..Contacts CM2 ON CM2.ContactID = SBM.SupplierID
						Left Join {DbNames.EPYSL}..Contacts CE2 On CE2.ContactID = KSM.SubContactID
					),
					MY As (
	                    Select YDReqIssueMasterID, YDReqIssueNo, YDReqIssueDate, YDReqIssueBy, YDReqMasterID, Remarks, ChallanNo, GPNo
	                    FROM {TableNames.YD_REQ_ISSUE_MASTER} 
                        Where IsSendForApprove=1 AND IsApprove=1 AND IsReject=0 AND IsGPApprove=1
                    ),
					MDY AS(
                        Select MY.YDReqIssueMasterID, MY.YDReqIssueNo, MY.YDReqIssueDate, MY.YDReqIssueBy, MY.YDReqMasterID, MY.Remarks,
					    YRM.YDReqNo, YRM.YDReqDate,Sum(YBC.BookingQty)BookingQty,sum(YRC.ReqQty)ReqQty,0 Rate, ConceptNo = YBM.GroupConceptNo, 
					    ProgramName = CASE WHEN ISNULL(YBM.ExportOrderID,0) > 0 THEN 'BULK' ELSE 'RND' END, ReqType = 'YD', YBM.YDBookingNo, LU.UserName ReqByUser,
				        YBM.GroupConceptNo AS CorBookingNo,
				        FCM.SubGroupID, ISG.SubGroupName, KT.TypeName KnittingType, Technical.TechnicalName, 
                        ISV.SegmentValue Composition, GSV.SegmentValue Gsm, FCM.Length, FCM.Width,'' KSCUnit,
						Company = Case when FCM.IsBDS=0 then 'EFL' else Case when isnull(CM.ShortName,'')<>'' then CM.ShortName else CM2.ShortName end end,
                        MY.ChallanNo, MY.GPNo
                        From MY
					    Inner JOIN {TableNames.YD_REQ_MASTER} YRM On YRM.YDReqMasterID = MY.YDReqMasterID
					    Inner JOIN {TableNames.YD_REQ_CHILD} YRC On YRC.YDReqMasterID = YRM.YDReqMasterID
					    LEFT JOIN {TableNames.YDBookingChild} YBC ON YBC.YDBookingMasterID=YRM.YDBookingMasterID
					    LEFT JOIN {TableNames.YD_BOOKING_MASTER} YBM ON YBM.YDBookingMasterID=YBC.YDBookingMasterID
					    LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = YBM.ConceptID
					    --LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPChildID=KSC.KPChildID
					    --LEFT JOIN {TableNames.Knitting_Plan_Master} KPM ON KPM.KPMasterID=KSC.KPMasterID
					    LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KT ON KT.TypeID = FCM.KnittingTypeID 
                        LEFT JOIN {TableNames.FabricTechnicalName} Technical ON Technical.TechnicalNameId = FCM.TechnicalNameId
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = FCM.CompositionID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue GSV ON GSV.SegmentValueID = FCM.GSMID 
                        LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID = FCM.SubGroupID
	                    Inner Join {DbNames.EPYSL}..LoginUser LU On YRM.YDReqBy = LU.UserCode
                        Left Join {DbNames.EPYSL}..BookingMaster BM On BM.BookingNo = YBM.GroupConceptNo
						Left Join {DbNames.EPYSL}..SampleBookingMaster SBM On SBM.BookingNo = YBM.GroupConceptNo
						LEFT JOIN {DbNames.EPYSL}..Contacts CM ON CM.ContactID = BM.SupplierID
						LEFT JOIN {DbNames.EPYSL}..Contacts CM2 ON CM2.ContactID = SBM.SupplierID
					    GROUP BY MY.YDReqIssueMasterID, MY.YDReqIssueNo, MY.YDReqIssueDate, MY.YDReqIssueBy, MY.YDReqMasterID, MY.Remarks,
					    YRM.YDReqNo, YRM.YDReqDate,FCM.ConceptNo,YBM.GroupConceptNo,FCM.SubGroupID, ISG.SubGroupName, KT.TypeName, Technical.TechnicalName, 
                        ISV.SegmentValue, GSV.SegmentValue, FCM.Length, FCM.Width, ISNULL(YBM.ExportOrderID,0), YBM.YDBookingNo, LU.UserName,
						FCM.IsBDS, CM.ShortName, CM2.ShortName, MY.ChallanNo, MY.GPNo
					),
					Final AS(
					    Select * from MD 
					    Union
					    Select * from MDY
					)
					Select *,Count(*) Over() TotalRows from Final ";
                    break;
                default: //Status.Pending
                    sql += $@";With A As (
	                    SELECT RM.KSCReqMasterID, RM.KSCReqNo, RM.KSCReqDate, RM.KSCMasterID, RC.ReqQty TotalReqQty, SUM(ISNULL(IC.IssueQty,0)) TotalIssueQty,
	                    KSC.BookingQty,KSC.SCQty,KSC.Rate,FCM.GroupConceptNo, 
	                    ProgramName = CASE WHEN ISNULL(RC.YBChildItemID,0) > 0 THEN 'BULK' ELSE 'RND' END,
                        --(Case When KPM.ConceptID > 0 Then 'RND' When KPM.ExportOrderID > 0 Then 'Bulk' Else 'RND' End) ProgramName,
                        ReqType = 'SC', 
	                    (CASE WHEN FCM.ConceptID > 0 THEN FCM.GroupConceptNo  ELSE KPM.YBookingNo END) AS CorBookingNo,
	                    FCM.SubGroupID, ISG.SubGroupName, KT.TypeName KnittingType, Technical.TechnicalName,CE2.ShortName KSCUnit, 
	                    ISV.SegmentValue Composition, GSV.SegmentValue Gsm, FCM.Length, FCM.Width,KSM.KSCNo,LU.UserName ReqByUser,CT.ShortName SCByUser,
						Case when CM.ShortName is not null then CM.ShortName else CM2.ShortName end Company,
	                    IM.IsCompleted
	                    FROM {TableNames.KNITTING_SUB_CONTRACT_REQ_MASTER} RM 
	                    INNER JOIN {TableNames.KNITTING_SUB_CONTRACT_REQ_CHILD} RC ON RC.KSCReqMasterID = RM.KSCReqMasterID AND RM.IsApprove = 1 AND RM.IsReject = 0 
	                    LEFT JOIN {TableNames.KNITTING_SUB_CONTRACT_ISSUE_MASTER} IM ON IM.KSCReqMasterID = RM.KSCReqMasterID
	                    LEFT JOIN {TableNames.KNITTING_SUB_CONTRACT_ISSUE_CHILD} IC ON IC.KSCIssueMasterID = IM.KSCIssueMasterID
	                    LEFT JOIN {TableNames.KNITTING_SUB_CONTRACT_CHILD} KSC ON RM.KSCMasterID=KSC.KSCMasterID
	                    LEFT JOIN {TableNames.KNITTING_SUB_CONTRACT_MASTER} KSM ON KSM.KSCMasterID=KSC.KSCMasterID
	                    LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = KSC.ConceptID
	                    LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPChildID=KSC.KPChildID
	                    LEFT JOIN {TableNames.Knitting_Plan_Master} KPM ON KPM.KPMasterID=KSC.KPMasterID
	                    LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KT ON KT.TypeID = FCM.KnittingTypeID 
	                    LEFT JOIN {TableNames.FabricTechnicalName} Technical ON Technical.TechnicalNameId = FCM.TechnicalNameId
						LEFT JOIN {TableNames.KNITTING_MACHINE} KM ON KM.KnittingMachineID = KSM.KnittingMachineID
	                    LEFT JOIN {TableNames.KNITTING_UNIT} KU ON KU.KnittingUnitID = KM.KnittingUnitID
						Left Join {DbNames.EPYSL}..BookingMaster BM On BM.BookingNo = KSM.BookingNo
						Left Join {DbNames.EPYSL}..SampleBookingMaster SBM On SBM.BookingNo = KSM.BookingNo
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = FCM.CompositionID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue GSV ON GSV.SegmentValueID = FCM.GSMID 
	                    LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID = FCM.SubGroupID 
	                    Inner Join {DbNames.EPYSL}..LoginUser LU On RM.KSCReqByID = LU.UserCode
	                    Left Join {DbNames.EPYSL}..Contacts CT On CT.ContactID = KSM.SubContactID
						LEFT JOIN {DbNames.EPYSL}..Contacts CM ON CM.ContactID = BM.SupplierID
						LEFT JOIN {DbNames.EPYSL}..Contacts CM2 ON CM2.ContactID = SBM.SupplierID
	                    Left Join {DbNames.EPYSL}..Contacts CE2 On CE2.ContactID = KSM.SubContactID
                        WHERE RM.IsAcknowledge = 1
	                    GROUP BY RM.KSCReqMasterID, RM.KSCReqNo, RM.KSCReqDate, RM.KSCMasterID, RC.ReqQty,
	                    KSC.BookingQty,KSC.SCQty,KSC.Rate,FCM.GroupConceptNo, (Case When KPM.ConceptID > 0 Then 'RND' When KPM.ExportOrderID > 0 Then 'Bulk' Else 'RND' End),
	                    (CASE WHEN FCM.ConceptID > 0 THEN FCM.GroupConceptNo  ELSE KPM.YBookingNo END),
	                    FCM.SubGroupID, ISG.SubGroupName, KT.TypeName, Technical.TechnicalName,CE2.ShortName,
	                    ISV.SegmentValue, GSV.SegmentValue, FCM.Length, FCM.Width,KSM.KSCNo,KPM.ConceptID,KPM.ExportOrderID,LU.UserName,CT.ShortName,
	                    IM.IsCompleted, CASE WHEN ISNULL(RC.YBChildItemID,0) > 0 THEN 'BULK' ELSE 'RND' END,CM.ShortName,CM2.ShortName
                    ), B AS(
	                    SELECT KSCReqMasterID, KSCReqNo, KSCReqDate, KSCMasterID, SUM(TotalReqQty) TotalReqQty, TotalIssueQty,
	                    A.BookingQty,A.SCQty,A.Rate,A.GroupConceptNo,A.ProgramName,A.ReqType,A.CorBookingNo,A.SubGroupID, A.SubGroupName, A.KnittingType, A.TechnicalName, 
	                    A.Composition Composition, A.Gsm Gsm, A.Length, A.Width,A.KSCNo,A.ReqByUser,A.SCByUser,A.Company,KSCUnit,isnull(IsCompleted,0)IsCompleted 
	                    FROM A
	                    GROUP BY KSCReqMasterID, KSCReqNo, KSCReqDate, KSCMasterID, TotalIssueQty,
	                    A.BookingQty,A.SCQty,A.Rate,A.GroupConceptNo,A.ProgramName,A.ReqType,A.CorBookingNo,A.SubGroupID, A.SubGroupName, A.KnittingType, A.TechnicalName, 
	                    A.Composition, A.Gsm, A.Length, A.Width,A.KSCNo,A.ReqByUser,A.SCByUser,A.Company,KSCUnit,IsCompleted 
                    ),
                    C AS(
	                    SELECT B.KSCReqMasterID, B.KSCReqNo, B.KSCReqDate,ReqQty = B.TotalReqQty,
	                    ConceptNo = B.GroupConceptNo,B.ProgramName,B.ReqType,B.KSCNo,B.ReqByUser,B.SCByUser,B.Company,KSCUnit
	                    FROM B
	                    Left JOIN {TableNames.KNITTING_SUB_CONTRACT_LOR_MASTER} LOR ON LOR.KSCReqMasterID = B.KSCReqMasterID
	                    WHERE (isnull(TotalReqQty,0) > isnull(TotalIssueQty,0) AND isnull(IsCompleted,0)=0) AND  LOR.KSCReqMasterID IS NULL
                    ),
                    M As 
                    (
	                    Select M.YDReqMasterID, YDReqNo, BM.YDBookingNo, LU.Name ReqByUser, YDReqDate, CT.ShortName BuyerName,
	                    SUM(YRC.ReqQty) ReqQty,sum(YRIC.IssueQty)IssueQty,BM.GroupConceptNo,
						Company = Case when FCM.IsBDS=0 then 'EFL' else Case when isnull(CM.ShortName,'')<>'' then CM.ShortName else CM2.ShortName end end ,
						YRIM.IsCompleted, BM.ConceptID, BM.ExportOrderID
	                    FROM {TableNames.YD_REQ_MASTER} M 
	                    Inner JOIN {TableNames.YD_REQ_CHILD} YRC On M.YDReqMasterID = YRC.YDReqMasterID
	                    Left JOIN {TableNames.YD_REQ_ISSUE_MASTER} YRIM ON YRIM.YDReqMasterID=M.YDReqMasterID
	                    Left JOIN {TableNames.YD_REQ_ISSUE_CHILD} YRIC ON YRIC.YDReqIssueMasterID=YRIM.YDReqIssueMasterID
	                    Inner JOIN {TableNames.YD_BOOKING_MASTER} BM ON M.YDBookingMasterID = BM.YDBookingMasterID
						Left Join {DbNames.EPYSL}..BookingMaster BOM On BOM.BookingNo = BM.GroupConceptNo
						Left Join {DbNames.EPYSL}..SampleBookingMaster SBM On SBM.BookingNo = BM.GroupConceptNo
						Left JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM On FCM.ConceptID = BM.ConceptID
						LEFT JOIN {DbNames.EPYSL}..Contacts CM ON CM.ContactID = BOM.SupplierID
						LEFT JOIN {DbNames.EPYSL}..Contacts CM2 ON CM2.ContactID = SBM.SupplierID
	                    Inner Join {DbNames.EPYSL}..LoginUser LU On M.YDReqBy = LU.UserCode
	                    Left Join {DbNames.EPYSL}..Contacts CT On BM.BuyerID = CT.ContactID 
	                    Left Join {DbNames.EPYSL}..CompanyEntity CE On CE.CompanyID = M.CompanyID
	                    Where ISNULL(YRIM.YDReqIssueMasterID,0) = 0 AND M.IsAcknowledge = 1
	                    Group By M.YDReqMasterID, YDReqNo, BM.YDBookingNo, LU.Name, YDReqDate, CT.ShortName,BM.GroupConceptNo,CE.DisplayCompanyName,
						YRIM.IsCompleted,BM.ConceptID, BM.ExportOrderID,CM.ShortName, CM2.ShortName,FCM.IsBDS
                    ),
                    N AS(
	                    Select YDReqMasterID, YDReqNo,YDReqDate, ReqQty, GroupConceptNo = CASE WHEN EOM.ExportOrderID > 0 THEN EOM.ExportOrderNo ELSE M.GroupConceptNo END,
	                    ProgrammeName = CASE WHEN M.ExportOrderID > 0 THEN 'BULK' ELSE 'RND' END,
                        ReqType = 'YD', YDBookingNo, ReqByUser, '' SCBy,Company,'' KSCUnit
	                    From M 
	                    LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = M.ConceptID
	                    LEFT JOIN {DbNames.EPYSL}..ExportOrderMaster EOM ON EOM.ExportOrderID = M.ExportOrderID
	                    Where (isnull(IsCompleted,0)=0 OR isnull(ReqQty,0)>isnull(IssueQty,0))
                    ),
                    Final AS(
	                    Select * from C
	                    Union 
	                    Select * from N
                    )
                    Select *,Count(*) Over() TotalRows from Final";

                    orderBy = " ORDER BY KSCReqMasterID DESC ";
                    break;
            }
            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<KnittingSubContractIssueMaster>(sql);
        }

        public async Task<KnittingSubContractIssueMaster> GetNewAsync(int id, string reqType, string programName = null)
        {
            var query = "";
            if (reqType != "YD" || String.IsNullOrEmpty(reqType))
            {
                query = $@"
                -- Master Info
                ;Select RM.KSCReqMasterID, RM.KSCReqNo, RM.KSCReqDate,
				KSC.BookingQty,KSC.SCQty,KSC.Rate,FCM.ConceptNo, 'SC' ReqType,
                ProgramName = (Case When KPM.ConceptID > 0 Then 'RND' When KPM.ExportOrderID > 0 Then 'Bulk' Else 'RND' End),
				(CASE WHEN FCM.ConceptID > 0 THEN FCM.ConceptNo  ELSE KPM.YBookingNo END) AS CorBookingNo,
				FCM.SubGroupID, ISG.SubGroupName, KT.TypeName KnittingType, Technical.TechnicalName, 
                ISV.SegmentValue Composition, GSV.SegmentValue Gsm, FCM.Length, FCM.Width,CO.ShortName AS ServiceProvider,CE.ShortName Company  
	            FROM {TableNames.KNITTING_SUB_CONTRACT_REQ_MASTER} RM
				LEFT JOIN {TableNames.KNITTING_SUB_CONTRACT_CHILD} KSC ON RM.KSCMasterID=KSC.KSCMasterID
				LEFT JOIN {TableNames.KNITTING_SUB_CONTRACT_MASTER} KSM ON KSM.KSCMasterID=KSC.KSCMasterID
				LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = KSC.ConceptID
				LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FBAC ON FBAC.BookingChildID = FCM.BookingChildID
				LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.FBAckID = FBAC.AcknowledgeID
				LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPChildID=KSC.KPChildID
				LEFT JOIN {TableNames.Knitting_Plan_Master} KPM ON KPM.KPMasterID=KSC.KPMasterID
				LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KT ON KT.TypeID = FCM.KnittingTypeID 
                LEFT JOIN {TableNames.FabricTechnicalName} Technical ON Technical.TechnicalNameId = FCM.TechnicalNameId
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = FCM.CompositionID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue GSV ON GSV.SegmentValueID = FCM.GSMID 
                LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID = FCM.SubGroupID 
				LEFT JOIN {DbNames.EPYSL}..Contacts CE ON CE.ContactID = FBA.SupplierID
				LEFT JOIN {DbNames.EPYSL}..Contacts CO ON CO.ContactID=KSM.SubContactID
                Where KSCReqMasterID ={id}

                
                -- Child Info
                ;WITH M As (
	                Select KSCReqMasterID,KSCMasterID
	                FROM {TableNames.KNITTING_SUB_CONTRACT_REQ_MASTER} 
	                Where KSCReqMasterID = {id}
                ), 
                BD As(
                Select YBCI.YBChildItemID,KSCRC.KSCReqChildID, SpinnerID = ISNULL(YSS.SpinnerId,0), YBCI.StitchLength, ISV6.SegmentValueID YarnCountID, 
                ISV2.SegmentValueID YarnTypeID, ISV2.SegmentValue YarnType, ISV6.SegmentValue YarnCount, KSCRC.ReqQty, KSCRC.ReqCone,
				AllocatedQty = isnull(YACI.TotalAllocationQty,0), YSS.ItemMasterID, YSS.YarnCategory,
				YarnLotNo = YSS.YarnLotNo,
				PhysicalCount = YSS.PhysicalCount,
				SpinnerName = SP.ShortName,
				StockFromTableId = 20, --KnittingSubContractReqChild
				StockFromPKId = KSCRC.KSCReqChildID, KSCRC.StockTypeId
                from M
                Inner JOIN {TableNames.KNITTING_SUB_CONTRACT_REQ_CHILD} KSCRC ON KSCRC.KSCReqMasterID = M.KSCReqMasterID
				INNER JOIN {TableNames.KNITTING_SUB_CONTRACT_CHILD}Item KSCI ON KSCI.KSCChildItemID = KSCRC.KSCChildItemID
				INNER JOIN {TableNames.KNITTING_SUB_CONTRACT_CHILD} KSC ON M.KSCMasterID=KSC.KSCMasterID
				INNER JOIN {TableNames.KNITTING_SUB_CONTRACT_MASTER} KSM ON KSM.KSCMasterID=KSC.KSCMasterID
				INNER JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBCI.YBChildItemID = KSCRC.YBChildItemID AND YBCI.YBChildItemID <> 0
                INNER JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID = YBCI.YBChildID
                INNER JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID = YBC.YBookingID
                LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.BookingChildID = YBC.BookingChildID AND FCM.BookingID = YBM.BookingID
				--LEFT JOIN {TableNames.RND_FREE_CONCEPT_MR_CHILD} FCMRC ON FCMRC.FCMRChildID = KSCRC.FCMRChildID
				LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FBAC ON FBAC.BookingChildID = FCM.BookingChildID AND FBAC.ItemMasterID=FCM.ItemMasterID
				LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.FBAckID = FBAC.AcknowledgeID
				LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPChildID=KSC.KPChildID
				LEFT JOIN {TableNames.Knitting_Plan_Master} KPM ON KPM.KPMasterID=KSC.KPMasterID
				Left Join {DbNames.EPYSL}..Contacts C ON C.ContactID = YBCI.SpinnerID
				LEFT JOIN {TableNames.YARN_ALLOCATION_CHILD_ITEM} YACI ON YACI.AllocationChildItemID = KSCI.AllocationChildItemID
				LEFT JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.AllocationChildID = YACI.AllocationChildID
				LEFT JOIN {TableNames.YarnStockSet} YSS ON YSS.YarnStockSetId = YACI.YarnStockSetId
				 INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YSS.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
				LEFT JOIN {DbNames.EPYSL}..Contacts SP ON SP.ContactID = YSS.SpinnerId 
				GROUP BY YBCI.YBChildItemID,KSCRC.KSCReqChildID, ISNULL(YSS.SpinnerId,0), YBCI.StitchLength, ISV6.SegmentValueID, 
                ISV2.SegmentValueID, ISV2.SegmentValue, ISV6.SegmentValue, KSCRC.ReqQty, KSCRC.ReqCone,
				YACI.TotalAllocationQty, YSS.ItemMasterID, YSS.YarnCategory,
				YSS.YarnLotNo, YSS.PhysicalCount,YSS.SpinnerID, SP.ShortName, KSCRC.StockTypeId
				),
				SD As(
                Select KSCRC.YBChildItemID,KSCRC.KSCReqChildID, SpinnerID = ISNULL(KPY.YarnBrandID,0), KPY.StitchLength, ISV6.SegmentValueID YarnCountID, 
                ISV2.SegmentValueID YarnTypeID, ISV2.SegmentValue YarnType, ISV6.SegmentValue YarnCount, KSCRC.ReqQty, KSCRC.ReqCone,
				AllocatedQty = 0, IM.ItemMasterID, KSCRC.YarnCategory,
				YarnLotNo = KPY.YarnLotNo,
				PhysicalCount = KPY.PhysicalCount,
				SpinnerName = SP.ShortName,
				StockFromTableId = 20, --KnittingSubContractReqChild
				StockFromPKId = KSCRC.KSCReqChildID, KSCRC.StockTypeId
                from M
                Inner JOIN {TableNames.KNITTING_SUB_CONTRACT_REQ_CHILD} KSCRC ON KSCRC.KSCReqMasterID = M.KSCReqMasterID
				INNER JOIN {TableNames.KNITTING_SUB_CONTRACT_CHILD}Item KSCI ON KSCI.KSCChildItemID = KSCRC.KSCChildItemID
				INNER JOIN {TableNames.KNITTING_SUB_CONTRACT_CHILD} KSC ON M.KSCMasterID=KSC.KSCMasterID
				INNER JOIN {TableNames.KNITTING_SUB_CONTRACT_MASTER} KSM ON KSM.KSCMasterID=KSC.KSCMasterID
				INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_CHILD} FCMRC ON FCMRC.FCMRChildID = KSCRC.FCMRChildID AND FCMRC.FCMRChildID <> 0
				LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = KSC.ConceptID
				LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FBAC ON FBAC.BookingChildID = FCM.BookingChildID AND FBAC.ItemMasterID=FCM.ItemMasterID
				LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.FBAckID = FBAC.AcknowledgeID
				LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPChildID=KSC.KPChildID
				LEFT JOIN {TableNames.Knitting_Plan_Master} KPM ON KPM.KPMasterID=KSC.KPMasterID
				INNER JOIN {TableNames.Knitting_Plan_Yarn} KPY ON KPY.KPYarnID = KSCRC.KPYarnID
				LEFT JOIN {DbNames.EPYSL}..Contacts SP ON SP.ContactID = KPY.YarnBrandID
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = FCMRC.ItemMasterId
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
				GROUP BY KSCRC.YBChildItemID,KSCRC.KSCReqChildID, ISNULL(KPY.YarnBrandID,0), KPY.StitchLength, ISV6.SegmentValueID, 
                ISV2.SegmentValueID, ISV2.SegmentValue, ISV6.SegmentValue, KSCRC.ReqQty, KSCRC.ReqCone,
				IM.ItemMasterID, KSCRC.YarnCategory,KPY.YarnLotNo,KPY.PhysicalCount,SP.ShortName, KSCRC.StockTypeId
				),
				FR As(
					SELECT BD.*
					FROM BD
					WHERE EXISTS (SELECT 1 FROM BD)

					UNION ALL

					SELECT SD.*
					FROM SD
					WHERE NOT EXISTS (SELECT 1 FROM BD)
				)
				Select * FROm FR


                ;Select Cast(C.ContactID As varchar) [id], C.Name [text]
                From {DbNames.EPYSL}..Contacts C
                Inner Join {DbNames.EPYSL}..ContactCategoryChild CCC On C.ContactID = CCC.ContactID
                Inner Join {DbNames.EPYSL}..ContactCategoryHK CC ON CC.ContactCategoryID = CCC.ContactCategoryID
                Where CC.ContactCategoryName = '{ContactCategoryNames.SPINNER}'; 
                
                ----TransportTypeList
                {CommonQueries.GetEntityTypesByEntityTypeId(EntityTypeConstants.TRANSPORT_TYPE)};

                ----TransportAgencyList
                {CommonQueries.GetContactsByCategoryType(ContactCategoryNames.CARRYING_CONTRACTOR)};";
            }
            else
            {
                query = $@"
                -- Master Info
                ;Select RM.YDReqMasterID KSCReqMasterID, RM.YDReqNo KSCReqNo, RM.YDReqDate KSCReqDate,
				sum(YBC.BookingQty)BookingQty,sum(YRC.ReqQty) SCQty,0 Rate,YBM.GroupConceptNo ConceptNo,'YD' ReqType,
                ProgramName = (Case When FCM.ConceptID > 0 Then 'Concept' When FBA.ExportOrderID > 0 Then 'Bulk' Else 'BDS' End),
				YBM.GroupConceptNo CorBookingNo,
				IM.SubGroupID, ISG.SubGroupName, KT.TypeName KnittingType, Technical.TechnicalName, 
                ISV.SegmentValue Composition, GSV.SegmentValue Gsm, FCM.Length, FCM.Width,CE.ServiceProvider,COE.ShortName Company 
	            FROM {TableNames.YD_REQ_MASTER} RM
				LEFT JOIN {TableNames.YD_REQ_CHILD} YRC ON YRC.YDReqMasterID=RM.YDReqMasterID
				LEFT JOIN {TableNames.YDBookingChild} YBC ON YBC.YDBookingChildID=YRC.YDBookingChildID
				LEFT JOIN {TableNames.YD_BOOKING_MASTER} YBM ON YBM.YDBookingMasterID=YBC.YDBookingMasterID
				LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = YBM.ConceptID
				LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FBAC ON FBAC.BookingChildID = FCM.BookingChildID
				LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.FBAckID = FBAC.AcknowledgeID
				INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YRC.ItemMasterID
				--LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPChildID=KSC.KPChildID
				--LEFT JOIN {TableNames.Knitting_Plan_Master} KPM ON KPM.KPMasterID=KSC.KPMasterID
				LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KT ON KT.TypeID = FCM.KnittingTypeID 
                LEFT JOIN {TableNames.FabricTechnicalName} Technical ON Technical.TechnicalNameId = FCM.TechnicalNameId
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = FCM.CompositionID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue GSV ON GSV.SegmentValueID = FCM.GSMID 
                LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID = IM.SubGroupID
				LEFT JOIN {DbNames.EPYSL}..CompanyEntity COE ON COE.CompanyID=RM.CompanyId
				Outer Apply (Select ShortName AS ServiceProvider from {DbNames.EPYSL}..CompanyEntity Where CompanyID=6) AS CE
				Where RM.YDReqMasterID ={id}
                GROUP BY RM.YDReqMasterID, RM.YDReqNo, RM.YDReqDate,YBM.GroupConceptNo,
				IM.SubGroupID, ISG.SubGroupName, KT.TypeName, Technical.TechnicalName, 
                ISV.SegmentValue, GSV.SegmentValue, FCM.Length, FCM.Width,CE.ServiceProvider,COE.ShortName,FCM.ConceptID,FBA.ExportOrderID
                
                -- Child Info
                ;WITH M As (
	                Select YDReqMasterID
	                FROM {TableNames.YD_REQ_MASTER} 
	                Where YDReqMasterID = {id}
                ) 
                Select KSCRC.YDReqChildID KSCReqChildID, KSCRC.SpinnerID, 
                SpinnerName = CASE WHEN ISNULL(KSCRC.SpinnerID,0) > 0 THEN C.Name ELSE '' END,
                0 StitchLength, ISV6.SegmentValueID YarnCountID, 
                ISV2.SegmentValueID YarnTypeID, ISV2.SegmentValue YarnType, ISV6.SegmentValue YarnCount, KSCRC.ReqQty, KSCRC.ReqCone, KSCRC.ItemMasterID,
                isnull(YACI.TotalAllocationQty,0)AllocatedQty,Case When FCM.ConceptID > 0 Then KSCRC.LotNo When FBA.ExportOrderID > 0 Then YSS.YarnLotNo else KSCRC.LotNo end YarnLotNo,--For Bulk LotNo get from Allocation
				Case When FCM.ConceptID > 0 Then KSCRC.PhysicalCount When FBA.ExportOrderID > 0 Then YSS.PhysicalCount else KSCRC.PhysicalCount end PhysicalCount, YAC.YBChildItemID, YDBC.YarnCategory,
                StockFromTableId = 21, --YDReqChild
				StockFromPKId = KSCRC.YDReqChildID, KSCRC.StockTypeId
				from M
                Inner JOIN {TableNames.YD_REQ_CHILD} KSCRC ON KSCRC.YDReqMasterID = M.YDReqMasterID
				LEFT JOIN {TableNames.YDBookingChild} YDBC ON YDBC.YDBookingChildID=KSCRC.YDBookingChildID
				LEFT JOIN {TableNames.YD_BOOKING_MASTER} YBM ON YBM.YDBookingMasterID=YDBC.YDBookingMasterID
				LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = YBM.ConceptID
				LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FBAC ON FBAC.BookingChildID = FCM.BookingChildID AND FBAC.ItemMasterID=FCM.ItemMasterID
				LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.FBAckID = FBAC.AcknowledgeID
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = KSCRC.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                Left Join {DbNames.EPYSL}..Contacts C ON KSCRC.SpinnerID = C.ContactID 
				LEFT JOIN {TableNames.YARN_ALLOCATION_CHILD_ITEM} YACI ON YACI.AllocationChildItemID = KSCRC.AllocationChildItemID
				LEFT JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.AllocationChildID = YACI.AllocationChildID
				LEFT JOIN {TableNames.YarnStockSet} YSS ON YSS.YarnStockSetId = YACI.YarnStockSetId
				LEFT JOIN {DbNames.EPYSL}..Contacts SP ON SP.ContactID = YSS.SpinnerId

                ;Select Cast(C.ContactID As varchar) [id], C.Name [text]
                From {DbNames.EPYSL}..Contacts C
                Inner Join {DbNames.EPYSL}..ContactCategoryChild CCC On C.ContactID = CCC.ContactID
                Inner Join {DbNames.EPYSL}..ContactCategoryHK CC ON CC.ContactCategoryID = CCC.ContactCategoryID
                Where CC.ContactCategoryName = '{ContactCategoryNames.SPINNER}' ; 
                
                ----TransportTypeList
                {CommonQueries.GetEntityTypesByEntityTypeId(EntityTypeConstants.TRANSPORT_TYPE)};

                ----TransportAgencyList
                {CommonQueries.GetContactsByCategoryType(ContactCategoryNames.CARRYING_CONTRACTOR)};";
            }
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                KnittingSubContractIssueMaster data = await records.ReadFirstOrDefaultAsync<KnittingSubContractIssueMaster>();
                Guard.Against.NullObject(data);
                data.Childs = records.Read<KnittingSubContractIssueChild>().ToList();
                data.SpinnerList = await records.ReadAsync<Select2OptionModel>();
                data.TransportTypeList = await records.ReadAsync<Select2OptionModel>();
                data.TransportAgencyList = await records.ReadAsync<Select2OptionModel>();
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
        public async Task<KnittingSubContractIssueMaster> GetAsync(int id, string reqType, string programName = null)
        {
            var query = "";
            if (reqType != "YD" || String.IsNullOrEmpty(reqType))
            {
                query = $@"
                -- Master Info
                ;With
                    M As (
	                    Select KSCIssueMasterID, KSCIssueNo,ChallanNo,ChallanDate,GPNo,GPDate,TransportTypeID,TransportAgencyID,VehicleNo,DriverName,ContactNo,IsCompleted, KSCIssueDate, KSCReqMasterID, Remarks, KSCIssueByID, IsSendForApprove
	                    FROM {TableNames.KNITTING_SUB_CONTRACT_ISSUE_MASTER}  WHERE KSCIssueMasterID ={id}
                    )
                    Select M.KSCIssueMasterID, M.KSCIssueNo, M.KSCIssueDate, M.KSCIssueByID, M.KSCReqMasterID,M.ChallanNo,M.ChallanDate,M.GPNo,GPDate,M.TransportTypeID,M.TransportAgencyID,M.VehicleNo,M.DriverName,M.ContactNo,M.IsCompleted, M.Remarks,RM.KSCReqNo,RM.KSCReqDate,
					KSC.BookingQty,KSC.SCQty,KSC.Rate,FCM.ConceptNo, 'SC' ReqType, M.IsSendForApprove,
					(Case When FCM.IsBDS = 0 Then 'RND' When FCM.IsBDS = 1 Then 'RND' When FCM.IsBDS = 2 Then 'Bulk' Else '' End) ProgramName,
				    (CASE WHEN FCM.ConceptID > 0 THEN FCM.ConceptNo  ELSE KPM.YBookingNo END) AS CorBookingNo,
				    FCM.SubGroupID, ISG.SubGroupName, KT.TypeName KnittingType, Technical.TechnicalName, 
                    ISV.SegmentValue Composition, GSV.SegmentValue Gsm, FCM.Length, FCM.Width,CE.ShortName ServiceProvider,COE.ShortName Company
                    From M
					INNER JOIN {TableNames.KNITTING_SUB_CONTRACT_REQ_MASTER} RM On RM.KSCReqMasterID = M.KSCReqMasterID
					INNER JOIN {TableNames.KNITTING_SUB_CONTRACT_CHILD} KSC ON KSC.KSCMasterID=RM.KSCMasterID
					LEFT JOIN {TableNames.KNITTING_SUB_CONTRACT_MASTER} KSM ON KSM.KSCMasterID=KSC.KSCMasterID
					LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = KSC.ConceptID
					LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FBAC ON FBAC.BookingChildID = FCM.BookingChildID
					LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.FBAckID = FBAC.AcknowledgeID
					LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPChildID=KSC.KPChildID
					LEFT JOIN {TableNames.Knitting_Plan_Master} KPM ON KPM.KPMasterID=KSC.KPMasterID
					LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KT ON KT.TypeID = FCM.KnittingTypeID 
                    LEFT JOIN {TableNames.FabricTechnicalName} Technical ON Technical.TechnicalNameId = FCM.TechnicalNameId
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = FCM.CompositionID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue GSV ON GSV.SegmentValueID = FCM.GSMID 
                    LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID = FCM.SubGroupID
					LEFT JOIN {DbNames.EPYSL}..Contacts COE ON COE.ContactID = FBA.SupplierID
					LEFT JOIN {DbNames.EPYSL}..Contacts CE ON CE.ContactID=KSM.SubContactID; 

                 -- Child Info
               ;WITH M As (
	                Select KSCIssueMasterID
	                FROM {TableNames.KNITTING_SUB_CONTRACT_ISSUE_MASTER} 
	                Where KSCIssueMasterID = {id}
                )

                Select KSCIC.KSCIssueChildID, KSCIC.KSCReqChildID, 
				SpinnerID = Case When CI.AllocationChildItemID > 0 Then YSS.SpinnerID Else KPY.YarnBrandID END, 
                SpinnerName = Case When CI.AllocationChildItemID > 0 Then SP.ShortName Else SP2.ShortName END, 
                YarnLotNo = Case When CI.AllocationChildItemID > 0 Then YSS.YarnLotNo Else KPY.YarnLotNo END,  
				PhysicalCount = Case When CI.AllocationChildItemID > 0 Then YSS.PhysicalCount Else KPY.PhysicalCount END,   
				AllocatedQty = Case When CI.AllocationChildItemID > 0 Then ISNULL(YACI.TotalAllocationQty,0) Else 0 END, 
                StitchLength = Case When CI.AllocationChildItemID > 0 Then YBCI.StitchLength Else KPY.StitchLength END, 
				ISV6.SegmentValueID YarnCountID, ISV2.SegmentValueID YarnTypeID, ISV2.SegmentValue YarnType, ISV6.SegmentValue YarnCount, KSCIC.IssueQty, 
                KSCIC.IssueQtyCone, KSCIC.IssueQtyCarton, KSCIC.Remarks, YSS.ItemMasterID, YSS.YarnCategory, KSCRC.YBChildItemID, KSCRC.ReqQty, KSCRC.ReqCone
                from M
                Inner JOIN {TableNames.KNITTING_SUB_CONTRACT_ISSUE_CHILD} KSCIC ON KSCIC.KSCIssueMasterID = M.KSCIssueMasterID
                Inner JOIN {TableNames.KNITTING_SUB_CONTRACT_REQ_CHILD} KSCRC ON KSCRC.KSCReqChildID = KSCIC.KSCReqChildID
				INNER JOIN {TableNames.KNITTING_SUB_CONTRACT_CHILD}Item CI ON CI.KSCChildItemID = KSCRC.KSCChildItemID
				LEFT JOIN {TableNames.YARN_ALLOCATION_CHILD_ITEM} YACI ON YACI.AllocationChildItemID = CI.AllocationChildItemID
				LEFT JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.AllocationChildID = YACI.AllocationChildID
				LEFT JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBCI.YBChildItemID=YAC.YBChildItemID
				LEFT JOIN {TableNames.RND_FREE_CONCEPT_MR_CHILD} FCMRC ON FCMRC.FCMRChildID = KSCRC.FCMRChildID
				LEFT JOIN {TableNames.YarnStockSet} YSS ON YSS.YarnStockSetId = Case When YACI.YarnStockSetId > 0 Then YACI.YarnStockSetId Else FCMRC.YarnStockSetId END
				LEFT JOIN {DbNames.EPYSL}..Contacts SP ON SP.ContactID = YSS.SpinnerId
				LEFT JOIN {TableNames.Knitting_Plan_Yarn} KPY ON KPY.KPYarnID = KSCRC.KPYarnID
				LEFT JOIN {DbNames.EPYSL}..Contacts SP2 ON SP2.ContactID = KPY.YarnBrandID
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YSS.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                Left Join {DbNames.EPYSL}..Contacts C ON YSS.SpinnerID = C.ContactID;                

                --Rack bins Mapping
                SELECT M.*
				FROM {TableNames.KNITTING_SUB_CONTRACT_ISSUE_CHILD_CHILD_RACK_BIN_MAPPING} M
				INNER JOIN {TableNames.KNITTING_SUB_CONTRACT_ISSUE_CHILD} PRC ON PRC.KSCIssueChildID = M.KSCIssueChildID
				WHERE PRC.KSCIssueMasterID = {id};

                ;Select Cast(C.ContactID As varchar) [id], C.Name [text]
                From {DbNames.EPYSL}..Contacts C
                Inner Join {DbNames.EPYSL}..ContactCategoryChild CCC On C.ContactID = CCC.ContactID
                Inner Join {DbNames.EPYSL}..ContactCategoryHK CC ON CC.ContactCategoryID = CCC.ContactCategoryID
                Where CC.ContactCategoryName = '{ContactCategoryNames.SPINNER}';
                
                ----TransportTypeList
                {CommonQueries.GetEntityTypesByEntityTypeId(EntityTypeConstants.TRANSPORT_TYPE)};

                ----TransportAgencyList
                {CommonQueries.GetContactsByCategoryType(ContactCategoryNames.CARRYING_CONTRACTOR)};";
            }
            else
            {
                query = $@"
                -- Master Info
                ;With
                    M As (
	                    Select YDReqIssueMasterID KSCIssueMasterID, YDReqIssueNo KSCIssueNo,ChallanNo,ChallanDate,GPNo,GPDate,TransportTypeID,TransportAgencyID,VehicleNo,DriverName,ContactNo,IsCompleted, YDReqIssueDate KSCIssueDate, YDReqMasterID KSCReqMasterID, Remarks, YDReqIssueBy KSCIssueByID
	                    FROM {TableNames.YD_REQ_ISSUE_MASTER}  WHERE YDReqIssueMasterID ={id}
                    )
                    Select M.KSCIssueMasterID, M.KSCIssueNo, M.KSCIssueDate, M.KSCIssueByID, M.KSCReqMasterID,M.ChallanNo,M.ChallanDate,M.GPNo,GPDate,M.TransportTypeID,M.TransportAgencyID,M.VehicleNo,M.DriverName,M.ContactNo,M.IsCompleted, M.Remarks,RM.YDReqNo KSCReqNo,RM.YDReqDate KSCReqDate,
					sum(YBC.BookingQty)BookingQty,sum(YRC.ReqQty) SCQty,0 Rate,YBM.GroupConceptNo ConceptNo, 'YD' ReqType,
					(Case When FCM.IsBDS = 0 Then 'RND' When FCM.IsBDS = 1 Then 'RND' When FCM.IsBDS = 2 Then 'Bulk' Else '' End) ProgramName,
				    YBM.GroupConceptNo AS CorBookingNo,
				    FCM.SubGroupID, ISG.SubGroupName, KT.TypeName KnittingType, Technical.TechnicalName, 
                    ISV.SegmentValue Composition, GSV.SegmentValue Gsm, FCM.Length, FCM.Width,CE.ServiceProvider,COE.ShortName Company
                    From M
					Inner JOIN {TableNames.YD_REQ_MASTER} RM On RM.YDReqMasterID = M.KSCReqMasterID
					LEFT JOIN {TableNames.YD_REQ_CHILD} YRC ON YRC.YDReqMasterID=RM.YDReqMasterID
					LEFT JOIN {TableNames.YDBookingChild} YBC ON YBC.YDBookingChildID=YRC.YDBookingChildID
					LEFT JOIN {TableNames.YD_BOOKING_MASTER} YBM ON YBM.YDBookingMasterID=YBC.YDBookingMasterID
					LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = YBM.ConceptID
					LEFT FROM {TableNames.FreeConceptMRMaster} FCMRM ON FCMRM.ConceptID = YBM.ConceptID
					LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FBAC ON FBAC.BookingChildID = FCM.BookingChildID
					LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.FBAckID = FBAC.AcknowledgeID
					INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YRC.ItemMasterID
					--LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPChildID=KSC.KPChildID
					--LEFT JOIN {TableNames.Knitting_Plan_Master} KPM ON KPM.KPMasterID=KSC.KPMasterID
					LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KT ON KT.TypeID = FCM.KnittingTypeID 
                    LEFT JOIN {TableNames.FabricTechnicalName} Technical ON Technical.TechnicalNameId = FCM.TechnicalNameId
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = FCM.CompositionID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue GSV ON GSV.SegmentValueID = FCM.GSMID 
                    LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID = FCM.SubGroupID
					LEFT JOIN {DbNames.EPYSL}..CompanyEntity COE ON COE.CompanyID=RM.CompanyId
					Outer Apply (Select ShortName AS ServiceProvider from {DbNames.EPYSL}..CompanyEntity Where CompanyID=6) AS CE 
					GROUP BY M.KSCIssueMasterID, M.KSCIssueNo, M.KSCIssueDate, M.KSCIssueByID, M.KSCReqMasterID,M.ChallanNo,M.ChallanDate,M.GPNo,GPDate,M.TransportTypeID,M.TransportAgencyID,M.VehicleNo,M.DriverName,M.ContactNo,M.IsCompleted, M.Remarks,RM.YDReqNo,RM.YDReqDate,
					YBM.GroupConceptNo,FCM.SubGroupID, ISG.SubGroupName, KT.TypeName, Technical.TechnicalName, 
                    ISV.SegmentValue, GSV.SegmentValue, FCM.Length, FCM.Width,CE.ServiceProvider,COE.ShortName,FCMRM.IsBDS, FCM.IsBDS;

                 -- Child Info
               ;WITH M As (
	                Select YDReqIssueMasterID,YDReqMasterID
	                FROM {TableNames.YD_REQ_ISSUE_MASTER} 
	                Where YDReqIssueMasterID = {id}
                )

                Select KSCIC.YDReqIssueChildID KSCIssueChildID, KSCIC.YDReqChildID KSCReqChildID, KSCRC.SpinnerID, 
                SpinnerName = CASE WHEN ISNULL(KSCRC.SpinnerID,0) > 0 THEN C.Name ELSE '' END,
                KSCIC.LotNo YarnLotNo, KSCIC.PhysicalCount, 
                0 As StitchLength, ISV6.SegmentValueID YarnCountID, ISV2.SegmentValueID YarnTypeID, ISV2.SegmentValue YarnType, ISV6.SegmentValue YarnCount, KSCIC.IssueQty, 
                KSCIC.IssueQtyCone, KSCIC.IssueQtyCarton, KSCIC.Remarks,KSCRC.ReqQty,KSCRC.ReqCone, AllocatedQty = ISNULL(YACI.TotalAllocationQty,0), IM.ItemMasterID, KSCIC.YarnCategory, YDBC.YBChildItemID
                FROM M
                INNER JOIN {TableNames.YD_REQ_ISSUE_CHILD} KSCIC ON KSCIC.YDReqIssueMasterID = M.YDReqIssueMasterID
				INNER JOIN {TableNames.YD_REQ_CHILD} KSCRC ON KSCRC.YDReqChildID = KSCIC.YDReqChildID
				LEFT JOIN {TableNames.YDBookingChild} YDBC ON YDBC.YDBookingChildID = KSCRC.YDBookingChildID
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = KSCRC.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
				LEFT JOIN {DbNames.EPYSL}..Contacts C ON KSCRC.SpinnerID = C.ContactID
				LEFT JOIN {TableNames.YARN_ALLOCATION_CHILD_ITEM} YACI ON YACI.AllocationChildItemID = KSCRC.AllocationChildItemID
				LEFT JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.AllocationChildID = YACI.AllocationChildID
				LEFT JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBCI.YBChildItemID=YAC.YBChildItemID
				LEFT JOIN {TableNames.YarnStockSet} YSS ON YSS.YarnStockSetId = YACI.YarnStockSetId
				LEFT JOIN {DbNames.EPYSL}..Contacts SP ON SP.ContactID = YSS.SpinnerId

                --Rack bins Mapping
                SELECT M.YDRICRBId AS KSCICRBId,M.YDReqIssueChildID AS KSCIssueChildID,M.ChildRackBinID,M.IssueCartoon,M.IssueQtyCone,M.IssueQtyKg
				FROM {TableNames.YD_REQ_ISSUE_CHILD_CHILD_RACK_BIN_MAPPING} M
				INNER JOIN {TableNames.YD_REQ_ISSUE_CHILD} PRC ON PRC.YDReqIssueChildID = M.YDReqIssueChildID
				WHERE PRC.YDReqIssueMasterID = {id};

                ;Select Cast(C.ContactID As varchar) [id], C.Name [text]
                From {DbNames.EPYSL}..Contacts C
                Inner Join {DbNames.EPYSL}..ContactCategoryChild CCC On C.ContactID = CCC.ContactID
                Inner Join {DbNames.EPYSL}..ContactCategoryHK CC ON CC.ContactCategoryID = CCC.ContactCategoryID
                Where CC.ContactCategoryName = '{ContactCategoryNames.SPINNER}';
                
                ----TransportTypeList
                {CommonQueries.GetEntityTypesByEntityTypeId(EntityTypeConstants.TRANSPORT_TYPE)};

                ----TransportAgencyList
                {CommonQueries.GetContactsByCategoryType(ContactCategoryNames.CARRYING_CONTRACTOR)};";
            }
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                KnittingSubContractIssueMaster data = await records.ReadFirstOrDefaultAsync<KnittingSubContractIssueMaster>();
                Guard.Against.NullObject(data);
                data.Childs = records.Read<KnittingSubContractIssueChild>().ToList();

                var rackBinMappingList = records.Read<KnittingSubContractIssueChildRackBinMapping>().ToList();
                if (rackBinMappingList == null) rackBinMappingList = new List<KnittingSubContractIssueChildRackBinMapping>();

                data.Childs.ForEach(c =>
                {
                    c.ChildRackBins = rackBinMappingList.Where(y => y.KSCIssueChildID == c.KSCIssueChildID).ToList();
                });

                data.SpinnerList = await records.ReadAsync<Select2OptionModel>();
                data.TransportTypeList = await records.ReadAsync<Select2OptionModel>();
                data.TransportAgencyList = await records.ReadAsync<Select2OptionModel>();
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
        public async Task<KnittingSubContractIssueMaster> GetAllAsync(int id)
        {
            var sql = $@"
            ;Select * FROM {TableNames.KNITTING_SUB_CONTRACT_ISSUE_MASTER} Where KSCIssueMasterID = {id}

            ;Select * FROM {TableNames.KNITTING_SUB_CONTRACT_ISSUE_CHILD} Where KSCIssueMasterID = {id}

            ;Select RBM.* FROM {TableNames.KNITTING_SUB_CONTRACT_ISSUE_CHILD_CHILD_RACK_BIN_MAPPING} RBM
            INNER JOIN {TableNames.KNITTING_SUB_CONTRACT_ISSUE_CHILD} KSCIC ON KSCIC.KSCIssueChildID = RBM.KSCIssueChildID
            Where KSCIC.KSCIssueMasterID = {id}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                KnittingSubContractIssueMaster data = await records.ReadFirstOrDefaultAsync<KnittingSubContractIssueMaster>();
                Guard.Against.NullObject(data);
                data.Childs = records.Read<KnittingSubContractIssueChild>().ToList();
                List<KnittingSubContractIssueChildRackBinMapping> CIList = records.Read<KnittingSubContractIssueChildRackBinMapping>().ToList();
                data.Childs.ForEach(ci =>
                {
                    ci.ChildRackBins = CIList.Where(x => x.KSCIssueChildID == ci.KSCIssueChildID).ToList();
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

        public async Task SaveAsync(KnittingSubContractIssueMaster entity, List<YarnReceiveChildRackBin> rackBins = null)
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
                switch (entity.EntityState)
                {
                    case EntityState.Added:
                        entity.KSCIssueMasterID = await _service.GetMaxIdAsync(TableNames.KNITTING_SUB_CONTRACT_ISSUE_MASTER, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        entity.KSCIssueNo = await _service.GetMaxNoAsync(TableNames.KSC_ISSUE_NO, 1, RepeatAfterEnum.NoRepeat, "00000", transactionGmt, _connectionGmt);

                        maxChildId = await _service.GetMaxIdAsync(TableNames.KNITTING_SUB_CONTRACT_ISSUE_CHILD, entity.Childs.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        foreach (var item in entity.Childs)
                        {
                            item.KSCIssueChildID = maxChildId++;
                            item.KSCIssueMasterID = entity.KSCIssueMasterID;
                        }
                        break;

                    case EntityState.Modified:
                        var addedChilds = entity.Childs.FindAll(x => x.EntityState == EntityState.Added);
                        maxChildId = await _service.GetMaxIdAsync(TableNames.KNITTING_SUB_CONTRACT_ISSUE_CHILD, addedChilds.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

                        foreach (var item in addedChilds)
                        {
                            item.KSCIssueChildID = maxChildId++;
                            item.KSCIssueMasterID = entity.KSCIssueMasterID;
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

                await _service.SaveSingleAsync(entity, transaction);
                await _service.SaveAsync(entity.Childs, transaction);
                if (rackBins != null)
                {
                    await _service.SaveAsync(rackBins, transaction);
                }

                #region Stock Operation

                //if (entity.IsApprove && entity.IsGPApprove == false && entity.IsAcknowledge == false)
                //{
                //    if (entity.ApproveBy.IsNull()) entity.ApproveBy = 0;
                //    int userId = entity.EntityState == EntityState.Added ? entity.AddedBy : entity.ApproveBy;
                //    await _connection.ExecuteAsync("spYarnStockOperation", new { MasterID = entity.KSCIssueMasterID, FromMenuType = EnumFromMenuType.KnittingSubContractIssueApp, UserId = userId }, transaction, 30, CommandType.StoredProcedure);
                //}
                #endregion Stock Operation


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
        public async Task SaveAsyncSC(KnittingSubContractIssueMaster entity, List<YarnReceiveChildRackBin> rackBins)
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
                int maxKSCICRBId = 0;
                entity.Childs.ForEach(x =>
                {
                    maxKSCICRBId += x.ChildRackBins.Count(y => y.EntityState == EntityState.Added);
                });
                switch (entity.EntityState)
                {
                    case EntityState.Added:
                        entity.KSCIssueMasterID = await _service.GetMaxIdAsync(TableNames.KNITTING_SUB_CONTRACT_ISSUE_MASTER, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        entity.KSCIssueNo = await _service.GetMaxNoAsync(TableNames.KSC_ISSUE_NO, 1, RepeatAfterEnum.NoRepeat, "00000", transactionGmt, _connectionGmt);

                        maxChildId = await _service.GetMaxIdAsync(TableNames.KNITTING_SUB_CONTRACT_ISSUE_CHILD, entity.Childs.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        maxKSCICRBId = await _service.GetMaxIdAsync(TableNames.KNITTING_SUB_CONTRACT_ISSUE_CHILD_CHILD_RACK_BIN_MAPPING, maxKSCICRBId, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

                        foreach (var item in entity.Childs)
                        {
                            item.KSCIssueChildID = maxChildId++;
                            item.KSCIssueMasterID = entity.KSCIssueMasterID;
                            item.EntityState = EntityState.Added;

                            foreach (var itemObj in item.ChildRackBins)
                            {
                                itemObj.KSCICRBId = maxKSCICRBId++;
                                itemObj.KSCIssueChildID = item.KSCIssueChildID;
                                itemObj.EntityState = EntityState.Added;
                            }
                        }
                        break;

                    case EntityState.Modified:
                        var addedChilds = entity.Childs.FindAll(x => x.EntityState == EntityState.Added);
                        maxChildId = await _service.GetMaxIdAsync(TableNames.KNITTING_SUB_CONTRACT_ISSUE_CHILD, addedChilds.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        maxKSCICRBId = await _service.GetMaxIdAsync(TableNames.KNITTING_SUB_CONTRACT_ISSUE_CHILD_CHILD_RACK_BIN_MAPPING, maxKSCICRBId, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

                        foreach (var item in addedChilds)
                        {
                            item.KSCIssueChildID = maxChildId++;
                            item.KSCIssueMasterID = entity.KSCIssueMasterID;
                        }
                        entity.Childs.ForEach(c =>
                        {
                            if (c.EntityState == EntityState.Added)
                            {
                                c.KSCIssueChildID = maxChildId++;
                                c.KSCIssueMasterID = entity.KSCIssueMasterID;
                                c.EntityState = EntityState.Added;
                            }
                            foreach (var itemObj in c.ChildRackBins.Where(x => x.EntityState == EntityState.Added).ToList())
                            {
                                itemObj.KSCICRBId = maxKSCICRBId++;
                                itemObj.KSCIssueChildID = c.KSCIssueChildID;
                                itemObj.EntityState = EntityState.Added;
                            }
                        });
                        break;

                    case EntityState.Unchanged:
                    case EntityState.Deleted:
                        entity.EntityState = EntityState.Deleted;
                        entity.Childs.SetDeleted();
                        entity.Childs.ForEach(x => x.ChildRackBins.SetDeleted());
                        break;

                    default:
                        break;
                }
                List<KnittingSubContractIssueChildRackBinMapping> rackBinList = new List<KnittingSubContractIssueChildRackBinMapping>();
                entity.Childs.ForEach(x =>
                {
                    rackBinList.AddRange(x.ChildRackBins);
                });

                await _service.SaveSingleAsync(entity, transaction);
                await _service.SaveAsync(rackBinList.Where(x => x.EntityState == EntityState.Deleted).ToList(), transaction);
                await _service.SaveAsync(entity.Childs, transaction);
                await _service.SaveAsync(rackBinList.Where(x => x.EntityState != EntityState.Deleted).ToList(), transaction);

                await _service.SaveAsync(rackBins, transaction);

                #region Stock Operation
                /*OLD if (entity.IsApprove)
                {
                    int userId = entity.EntityState == EntityState.Added ? entity.AddedBy : entity.ApproveBy;

                    if (entity.ProgramName.ToUpper() == "RND")
                    {
                        await _connection.ExecuteAsync("spYarnStockOperation", new { MasterID = entity.KSCIssueMasterID, FromMenuType = EnumFromMenuType.KnittingSubContractIssueRAndD, UserId = userId }, transaction, 30, CommandType.StoredProcedure);
                    }
                    else if (entity.ProgramName.ToUpper() == "BULK")
                    {
                        await _connection.ExecuteAsync("spYarnStockOperation", new { MasterID = entity.KSCIssueMasterID, FromMenuType = EnumFromMenuType.KnittingSubContractIssueBulk, UserId = userId }, transaction, 30, CommandType.StoredProcedure);
                    }
                }*/
                //if (entity.IsApprove && entity.IsAcknowledge == false)
                //{
                //    if (entity.ApproveBy.IsNull()) entity.ApproveBy = 0;
                //    int userId = entity.EntityState == EntityState.Added ? entity.AddedBy : entity.ApproveBy;
                //    await _connection.ExecuteAsync("spYarnStockOperation", new { MasterID = entity.KSCIssueMasterID, FromMenuType = EnumFromMenuType.KnittingSubContractIssueApp, UserId = userId }, transaction, 30, CommandType.StoredProcedure);
                //}
                #endregion Stock Operation

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
