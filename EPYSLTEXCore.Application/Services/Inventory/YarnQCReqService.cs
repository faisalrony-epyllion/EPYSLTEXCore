using Dapper;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Application.Interfaces;
using EPYSLTEXCore.Application.Interfaces.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Yarn;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using Newtonsoft.Json;
using System.ComponentModel.Design;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Transactions;
using static Dapper.SqlMapper;

namespace EPYSLTEXCore.Application.Services.Inventory
{
    public class YarnQCReqService : IYarnQCReqService
    {
        private readonly IDapperCRUDService<YarnQCReqMaster> _service;

        private readonly SqlConnection _connection;
        private readonly SqlConnection _connectionGmt;

        public YarnQCReqService(IDapperCRUDService<YarnQCReqMaster> service
            , IDapperCRUDService<YarnQCReqChild> itemMasterRepository)
        {
            _service = service;

            _service.Connection = service.GetConnection(AppConstants.GMT_CONNECTION);
            _connectionGmt = service.Connection;

            _service.Connection = service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _connection = service.Connection;
        }

        public async Task<List<YarnQCReqMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo)
        {
            string tempGuid = CommonFunction.GetNewGuid();
            string orderBy = "";
            var sql = string.Empty;
            if (status == Status.Pending)
            {
                sql = $@";
                      With M AS(
                        SELECT ReceiveChildID = RMC.ChildID,RMC.POChildID, RM.ReceiveID, RM.ReceiveDate, RM.ReceiveNo, RM.LocationID, RM.RCompanyID, RM.OCompanyID, RM.SupplierID, RM.LCNo, RM.LCDate, RM.Tolerance,
                        RM.InvoiceNo, RM.InvoiceDate, RM.BLNo, RM.BLDate, RM.BankBranchID, RM.PLNo, RM.PLDate, RM.ChallanNo, RM.ChallanDate, RM.GPNo, RM.GPDate, RM.TransportMode,
                        RM.TransportTypeID, RM.CContractorID, RM.VehicalNo, RM.ShipmentStatus, RM.Remarks, SpinnerID = CASE WHEN RMC.SpinnerID > 0 THEN RMC.SpinnerID ELSE RM.SpinnerID END, RM.POID, RM.CIID, RM.LCID, RM.ReceivedById, RM.GPTime,
                        RM.PONo, RM.PODate,RMC.ItemMasterID,RMC.ShadeCode,RMC.LotNo,RMC.ChallanLot, RMC.ReceiveQty, RMC.PhysicalCount,
                        RF.ReceiveForName, RMC.YarnCategory
		                FROM {TableNames.YARN_RECEIVE_CHILD} RMC
                        INNER JOIN {TableNames.YARN_RECEIVE_MASTER} RM ON RM.ReceiveID = RMC.ReceiveID
                        Left Join {TableNames.YARN_QC_REQ_CHILD} LTRM On LTRM.ReceiveID = RM.ReceiveID AND LTRM.ItemMasterID = RMC.ItemMasterID AND LTRM.PhysicalCount = RMC.PhysicalCount AND LTRM.LotNo = RMC.LotNo 
                        LEFT JOIN ReceiveFor RF ON RF.ReceiveForId = RMC.ReceiveForId
		                WHERE LTRM.ReceiveID IS NULL 
                        AND RM.IsCDA = 'False' AND RMC.IsNoTest = 0 AND RMC.TagYarnReceiveChildID=0
                    ),
					PrevReq As(
						Select YQC.LotNo, YQC.ItemMasterID,HasPrevQCReq = CONVERT(bit, Case When count(*)>0 Then 1 Else 0 End )
						from M
						Inner Join {TableNames.YARN_QC_REQ_CHILD} YQC ON YQC.LotNo = M.LotNo AND YQC.ItemMasterID = M.ItemMasterID
						GROUP BY YQC.LotNo, YQC.ItemMasterID
						),
                    POInfo AS
                    (
                        SELECT M.ReceiveChildID
	                    ,BuyerName = CASE WHEN ISNULL(PoC.BuyerID,0) > 0 THEN C1.ShortName ELSE STRING_AGG(C.ShortName,',') END
                        FROM M
                        INNER join {TableNames.YarnPOChild} PoC On Poc.YPOChildID = M.POChildID
                        LEFT Join {TableNames.YarnPOChildBuyer} YPB ON YPB.YPOChildID = PoC.YPOChildID
                        LEFT JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = YPB.BuyerID AND C.ContactID > 0
	                    LEFT JOIN {DbNames.EPYSL}..Contacts C1 ON C1.ContactID = PoC.BuyerID AND C1.ContactID > 0
                        GROUP BY M.ReceiveChildID,PoC.BuyerID,C1.ShortName
                    ),
                    AllocatedOrders AS
                    (
	                    SELECT M.ReceiveChildID,
	                    EWO = CASE WHEN ISNULL(M.POID,0) = 0 THEN M.ReceiveForName ELSE IsNull(YRCO.EWONo,IsNull(YPOC.BookingNo,IsNull(FCM.GroupConceptNo,IsNull(YPOCO.EWONo,IsNull(EOM.ExportOrderNo,''))))) END
	                    FROM M
	                    Left JOIN {TableNames.YarnPOChildOrder} YO ON YO.YPOChildID = M.POChildID
	                    Left Join {TableNames.YARN_RECEIVE_CHILD_ORDER} YRCO On YRCO.ReceiveChildID = M.ReceiveChildID
	                    Left Join {TableNames.YarnPOChildOrder} YPOCO ON YPOCO.YPOChildID = M.POChildID
	                    Left Join {DbNames.EPYSL}..ExportOrderMaster EOM On EOm.ExportOrderID = YPOCO.ExportOrderID
	                    Left Join {TableNames.YarnPOChild} YPOC ON YPOC.YPOMasterID = M.POID And YPOC.ItemMasterID = M.ItemMasterID And YPOC.YPOChildID = M.POChildID
	                    Left Join {TableNames.RND_FREE_CONCEPT_MASTER} FCM On FCM.ConceptID = YPOC.ConceptID
                        GROUP BY M.ReceiveChildID, M.ReceiveForName, ISNULL(M.POID,0), YRCO.EWONo, YPOC.BookingNo, FCM.GroupConceptNo, YPOCO.EWONo, EOM.ExportOrderNo
                    ),
                    AllocatedOrdersDistinct AS
                    (
	                    SELECT DISTINCT A.ReceiveChildID, A.EWO
	                    FROM AllocatedOrders A
                    ),
                    EWOInfo AS
                    (
                        Select M.ReceiveChildID, YPOChildID = M.POChildID, EWO = STRING_AGG(A.EWO,',')
                        From M
	                    LEFT JOIN AllocatedOrdersDistinct A ON A.ReceiveChildID = M.ReceiveChildID
	                    GROUP BY M.ReceiveChildID, M.POChildID
                    ),
                    POFor AS
                    (
                        SELECT M.ReceiveChildID, POFor = STRING_AGG(PO.ValueName,',') 
                        FROM M
                        INNER join {TableNames.YarnPOChild} PoC On Poc.YPOMasterID = M.POID And Poc.ItemMasterID = M.ItemMasterID
                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue PO ON PO.ValueID = PoC.POForID
                        GROUP BY M.ReceiveChildID
                    ),
                    RackBin AS
                    (
                        Select M.ReceiveChildID, RackNo = STRING_AGG(RC.RackNo,',')
                        From M
                        INNER JOIN {TableNames.YARN_RECEIVE_CHILD_RACK_BIN} RB ON RB.ChildID = M.ReceiveChildID
                        INNER JOIN {DbNames.EPYSL}..Rack RC ON RC.RackID = RB.RackID
                        GROUP BY M.ReceiveChildID
                    ),RackBinDate As(
                        Select M.ReceiveChildID,DateAdded = Max(RB.DateAdded)
                        From {TableNames.YARN_RECEIVE_CHILD_RACK_BIN} RB
                        INNER JOIN M  ON RB.ChildID = M.ReceiveChildID
                        GROUP BY M.ReceiveChildID

				    ),
                    FinalList AS
                    (
	                    SELECT M.ReceiveChildID, M.ReceiveID, M.ReceiveDate, M.ReceiveNo, M.LocationID, M.RCompanyID, M.OCompanyID, M.SupplierID, M.LCNo, M.LCDate, M.Tolerance,
	                    M.InvoiceNo, M.InvoiceDate, M.BLNo, M.BLDate, M.BankBranchID, M.PLNo, M.PLDate, M.ChallanNo, M.ChallanDate, M.GPNo, M.GPDate, M.TransportMode,
	                    M.TransportTypeID, M.CContractorID, M.VehicalNo, M.ShipmentStatus, M.Remarks, M.SpinnerID, M.POID, M.CIID, M.LCID, M.ReceivedById, M.GPTime,
	                    SupplierName = CASE WHEN ISNULL(M.SupplierID,0) > 0 THEN CC.[Name] ELSE '' END, 
	                    Spinner = CASE WHEN ISNULL(M.SpinnerID,0) > 0 THEN SS.[Name] ELSE '' END,BB.BranchName BankBranchName, COM.ShortName RCompany, M.PONo, M.PODate,M.ItemMasterID,
	                    ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc, 
	                    ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc,M.ShadeCode,M.LotNo,M.ChallanLot,
	                    ImportCategory = CASE WHEN ACI.InLand = 1 THEN 'Local' ELSE CASE WHEN ACI.InHouse = 1 THEN 'In house' ELSE 'Foreign' END END,
	                    ReceivedQtyInKg = M.ReceiveQty, POI.BuyerName,EWOI.EWO,
	                    POF.POFor, M.PhysicalCount, RB.RackNo,
	                    YarnDetail = M.YarnCategory, -- CONCAT(ISV6.SegmentValue,' ', ISV1.SegmentValue,' ', ISV3.SegmentValue,' ', ISV4.SegmentValue,' ', ISV2.SegmentValue,' ', ISV5.SegmentValue,' ', M.ShadeCode)
	                    RackBinDate  = RBD.DateAdded, HasPrevQCReq = ISNULL(PVR.HasPrevQCReq,0)
	                    FROM M 
						LEFT JOIN PrevReq PVR ON PVR.LotNo = M.LotNo AND PVR.ItemMasterID = M.ItemMasterID 
	                    Left Join {TableNames.YARN_RECEIVE_CHILD_ORDER} YRCO On YRCO.ReceiveChildID = M.ReceiveChildID
	                    Left Join {TableNames.YarnPOChild} YPOC ON YPOC.YPOMasterID = M.POID And YPOC.ItemMasterID = M.ItemMasterID And YPOC.YPOChildID = M.POChildID
	                    Left Join {TableNames.RND_FREE_CONCEPT_MASTER} FCM On FCM.ConceptID = YPOC.ConceptID
	                    LEFT JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = M.SupplierID
	                    LEFT JOIN {DbNames.EPYSL}..ContactAdditionalInfo ACI ON ACI.ContactID = CC.ContactID
	                    LEFT JOIN {DbNames.EPYSL}..BankBranch BB ON BB.BankBranchID = M.BankBranchID
	                    LEFT JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID = M.RCompanyID
	                    LEFT JOIN {DbNames.EPYSL}..Contacts SS ON SS.ContactID = M.SpinnerID
	                    INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = M.ItemMasterID 
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
	                    LEFT JOIN POInfo POI ON POI.ReceiveChildID = M.ReceiveChildID
	                    LEFT JOIN EWOInfo EWOI ON EWOI.ReceiveChildID = M.ReceiveChildID
	                    LEFT JOIN POFor POF ON POF.ReceiveChildID = M.ReceiveChildID
	                    INNER JOIN RackBin RB ON RB.ReceiveChildID = M.ReceiveChildID
	                    Left Join RackBinDate RBD On RBD.ReceiveChildID =  M.ReceiveChildID
                    )
                    SELECT *,Count(*) Over() TotalRows FROM FinalList";

                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By ReceiveID Desc,ItemMasterID" : paginationInfo.OrderBy;
            }
            else if (status == Status.Hold)
            {
                sql = $@";
                 With M AS(
                    SELECT ReceiveChildID = RMC.ChildID, RM.ReceiveID, RM.ReceiveDate, RM.ReceiveNo, RM.LocationID, RM.RCompanyID, RM.OCompanyID, RM.SupplierID, RM.LCNo, RM.LCDate, RM.Tolerance,
                    RM.InvoiceNo, RM.InvoiceDate, RM.BLNo, RM.BLDate, RM.BankBranchID, RM.PLNo, RM.PLDate, RM.ChallanNo, RM.ChallanDate, RM.GPNo, RM.GPDate, RM.TransportMode,
                    RM.TransportTypeID, RM.CContractorID, RM.VehicalNo, RM.ShipmentStatus, RM.Remarks, SpinnerID = CASE WHEN RMC.SpinnerID > 0 THEN RMC.SpinnerID ELSE RM.SpinnerID END, RM.POID, RM.CIID, RM.LCID, RM.ReceivedById, RM.GPTime,
                    RM.PONo, RM.PODate,RMC.ItemMasterID,RMC.ShadeCode,RMC.LotNo,RMC.ChallanLot, RMC.ReceiveQty, RMC.PhysicalCount, RMC.NoTestRemarks, RMC.YarnCategory,
                    RMC.POChildID
                    FROM {TableNames.YARN_RECEIVE_CHILD} RMC
	                INNER JOIN {TableNames.YARN_RECEIVE_MASTER} RM ON RM.ReceiveID = RMC.ReceiveID
                    Left Join {TableNames.YARN_QC_REQ_CHILD} LTRM On LTRM.ReceiveID = RM.ReceiveID AND LTRM.ItemMasterID = RMC.ItemMasterID AND LTRM.PhysicalCount = RMC.PhysicalCount AND LTRM.LotNo = RMC.LotNo 
                    WHERE LTRM.ReceiveID IS NULL
                    AND RM.IsCDA = 'False' AND RMC.IsNoTest = 1
                ),
                POInfo AS
                (
                    SELECT M.ReceiveChildID
	                ,BuyerName = CASE WHEN ISNULL(PoC.BuyerID,0) > 0 THEN C1.ShortName ELSE STRING_AGG(C.ShortName,',') END
                    FROM M
                    INNER join {TableNames.YarnPOChild} PoC On Poc.YPOChildID = M.POChildID
                    LEFT Join {TableNames.YarnPOChildBuyer} YPB ON YPB.YPOChildID = PoC.YPOChildID
                    LEFT JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = YPB.BuyerID AND C.ContactID > 0
	                LEFT JOIN {DbNames.EPYSL}..Contacts C1 ON C1.ContactID = PoC.BuyerID AND C1.ContactID > 0
                    GROUP BY M.ReceiveChildID,PoC.BuyerID,C1.ShortName
                ),
                EWOInfo AS
                (
	                Select M.ReceiveChildID, EWO = STRING_AGG(EOM.ExportOrderNo,',')
	                From M
	                INNER JOIN {TableNames.YarnPOChildOrder} YO ON YO.YPOMasterID = M.POID
	                LEFT JOIN {DbNames.EPYSL}..ExportOrderMaster EOM ON EOM.ExportOrderID = YO.ExportOrderID
	                GROUP BY M.ReceiveChildID
                ),
                POFor AS
                (
	                SELECT M.ReceiveChildID, POFor = STRING_AGG(PO.ValueName,',') 
	                FROM M
	                INNER join {TableNames.YarnPOChild} PoC On Poc.YPOMasterID = M.POID And Poc.ItemMasterID = M.ItemMasterID
	                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue PO ON PO.ValueID = PoC.POForID
	                GROUP BY M.ReceiveChildID
                ),
                RackBin AS
                (
	                Select M.ReceiveChildID, RackNo = STRING_AGG(RC.RackNo,',')
	                From M
	                INNER JOIN {TableNames.YARN_RECEIVE_CHILD_RACK_BIN} RB ON RB.ChildID = M.ReceiveChildID
	                INNER JOIN {DbNames.EPYSL}..Rack RC ON RC.RackID = RB.RackID
	                GROUP BY M.ReceiveChildID
                ),RackBinDate As(
                        Select M.ReceiveChildID,DateAdded = Max(RB.DateAdded)
                        From {TableNames.YARN_RECEIVE_CHILD_RACK_BIN} RB
                        INNER JOIN M  ON RB.ChildID = M.ReceiveChildID
                        GROUP BY M.ReceiveChildID

				),
                FinalList AS
                (
                    SELECT M.ReceiveChildID, M.ReceiveID, M.ReceiveDate, M.ReceiveNo, M.LocationID, M.RCompanyID, M.OCompanyID, M.SupplierID, M.LCNo, M.LCDate, M.Tolerance,
	                M.InvoiceNo, M.InvoiceDate, M.BLNo, M.BLDate, M.BankBranchID, M.PLNo, M.PLDate, M.ChallanNo, M.ChallanDate, M.GPNo, M.GPDate, M.TransportMode,
	                M.TransportTypeID, M.CContractorID, M.VehicalNo, M.ShipmentStatus, M.Remarks, M.SpinnerID, M.POID, M.CIID, M.LCID, M.ReceivedById, M.GPTime,
	                CC.[Name] SupplierName, SS.[Name] Spinner,BB.BranchName BankBranchName, COM.ShortName RCompany, M.PONo, M.PODate,M.ItemMasterID,
                    ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc, 
                    ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc,M.ShadeCode,M.LotNo,M.ChallanLot,
	                ImportCategory = CASE WHEN ACI.InLand = 1 THEN 'Local' ELSE CASE WHEN ACI.InHouse = 1 THEN 'In house' ELSE 'Foreign' END END,
	                ReceivedQtyInKg = M.ReceiveQty, POI.BuyerName, EWOI.EWO, POF.POFor, M.PhysicalCount, M.NoTestRemarks, RB.RackNo,
                    YarnDetail = M.YarnCategory, RackBinDate  = RBD.DateAdded
                    FROM M
                    LEFT JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = M.SupplierID
	                LEFT JOIN {DbNames.EPYSL}..ContactAdditionalInfo ACI ON ACI.ContactID = CC.ContactID
                    LEFT JOIN {DbNames.EPYSL}..BankBranch BB ON BB.BankBranchID = M.BankBranchID
                    LEFT JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID = M.RCompanyID
                    LEFT JOIN {DbNames.EPYSL}..Contacts SS ON SS.ContactID = M.SpinnerID
                    INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = M.ItemMasterID 
	                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
	                LEFT JOIN POInfo POI ON POI.ReceiveChildID = M.ReceiveChildID
	                LEFT JOIN EWOInfo EWOI ON EWOI.ReceiveChildID = M.ReceiveChildID
	                LEFT JOIN POFor POF ON POF.ReceiveChildID = M.ReceiveChildID
	                INNER JOIN RackBin RB ON RB.ReceiveChildID = M.ReceiveChildID
                    Left Join RackBinDate RBD On RBD.ReceiveChildID =  M.ReceiveChildID
                )
                SELECT * INTO #TempTable{tempGuid} FROM FinalList
				SELECT *,Count(*) Over() TotalRows FROM #TempTable{tempGuid}


                ";

                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By ReceiveID Desc,ItemMasterID" : paginationInfo.OrderBy;
            }
            else if (status == Status.ProposedForApproval)
            {
                sql = $@";
                ;With 
                Child AS
                (
	                SELECT RC.QCReqMasterID, ItemMasterID = MAX(RC.ItemMasterID)
	                FROM {TableNames.YARN_QC_REQ_CHILD} RC
                    INNER JOIN {TableNames.YARN_QC_REQ_MASTER} RCM ON RCM.QCReqMasterID = RC.QCReqMasterID
	                WHERE RCM.IsSendForApproval = 1 AND RCM.IsApprove = 0
	                GROUP BY RC.QCReqMasterID
                ),
                YQCReq As 
                (
	                Select M.QCReqMasterID, M.QCReqNo, U.Name QCReqByUser, M.QCReqDate, QCReqFor.ValueName QCReqFor, M.IsApprove, M.IsAcknowledge,
	                M.ReceiveID, RC.ItemMasterID,M.PhysicalCount,M.LotNo,RM.ChallanNo, RM.ChallanDate,RM.ReceiveNo,RM.ReceiveDate, M.RetestQCReqMasterID, M.IsRetestForRequisition, M.RetestForRequisitionQCReqMasterID
	                From {TableNames.YARN_QC_REQ_MASTER} M
	                INNER JOIN Child RC ON RC.QCReqMasterID = M.QCReqMasterID --Max can have 1 Child
	                LEFT JOIN {TableNames.YARN_RECEIVE_MASTER} RM ON RM.ReceiveID=M.ReceiveID
	                LEFT Join {DbNames.EPYSL}..EntityTypeValue QCReqFor On M.QCForID = QCReqFor.ValueID
	                LEFT Join {DbNames.EPYSL}..LoginUser U On M.QCReqBy = U.UserCode
                    WHERE M.IsSendForApproval = 1 AND M.IsApprove = 0
                ),
                RackBinDate As(
					
                        Select  RM.ReceiveID, RackBinDate = Max(RB.DateAdded)
                        From {TableNames.YARN_RECEIVE_CHILD_RACK_BIN} RB
						Inner Join {TableNames.YARN_RECEIVE_CHILD} RMC On RMC.ChildID = RB.ChildID
                        INNER JOIN {TableNames.YARN_RECEIVE_MASTER} RM ON RM.ReceiveID = RMC.ReceiveID
                        INNER JOIN YQCReq M  ON M.ReceiveID = RM.ReceiveID
                        GROUP BY RM.ReceiveID

				),
                FinalList AS
                (
                    Select QCReqMasterID, QCReqNo,	QCReqByUser, QCReqDate,	QCReqFor, IsApprove,IsAcknowledge, YQCReq.ReceiveID,
                    YQCReq.ItemMasterID,YQCReq.PhysicalCount,YQCReq.LotNo, YQCReq.ChallanNo, YQCReq.ChallanDate,YQCReq.ReceiveNo,YQCReq.ReceiveDate,
                    ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc, 
                    ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc,
                   Status = CASE WHEN YQCReq.RetestForRequisitionQCReqMasterID > 0 THEN 'Retest for requisition' 
								  WHEN YQCReq.RetestQCReqMasterID > 0 THEN 'Retest' 
							      ELSE '' END,RBD.RackBinDate
                    From YQCReq
                    INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YQCReq.ItemMasterID 
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
	                Left Join RackBinDate RBD On RBD.ReceiveID = YQCReq.ReceiveID
                )
                SELECT *,Count(*) Over() TotalRows FROM FinalList 
                ";

                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By QCReqMasterID Desc" : paginationInfo.OrderBy;
            }
            else if (status == Status.Approved || status == Status.ProposedForAcknowledge)
            {
                sql = $@";
                ;With 
                Child AS
                (
	                SELECT RC.QCReqMasterID, ItemMasterID = MAX(RC.ItemMasterID)
	                FROM {TableNames.YARN_QC_REQ_CHILD} RC
                    INNER JOIN {TableNames.YARN_QC_REQ_MASTER} RCM ON RCM.QCReqMasterID = RC.QCReqMasterID
	                WHERE RCM.IsApprove = 1 AND RCM.IsAcknowledge = 0
	                GROUP BY RC.QCReqMasterID
                ),
                YQCReq As 
                (
	                Select M.QCReqMasterID, M.QCReqNo, U.Name QCReqByUser, M.QCReqDate, QCReqFor.ValueName QCReqFor, M.IsApprove, M.IsAcknowledge,
	                M.ReceiveID, RC.ItemMasterID,M.PhysicalCount,M.LotNo,RM.ChallanNo, RM.ChallanDate,RM.ReceiveNo,RM.ReceiveDate, M.RetestQCReqMasterID, M.IsRetestForRequisition, M.RetestForRequisitionQCReqMasterID
	                From {TableNames.YARN_QC_REQ_MASTER} M
	                INNER JOIN Child RC ON RC.QCReqMasterID = M.QCReqMasterID --Max can have 1 Child
	                LEFT JOIN {TableNames.YARN_RECEIVE_MASTER} RM ON RM.ReceiveID=M.ReceiveID
	                LEFT Join {DbNames.EPYSL}..EntityTypeValue QCReqFor On M.QCForID = QCReqFor.ValueID
	                LEFT Join {DbNames.EPYSL}..LoginUser U On M.QCReqBy = U.UserCode
                    WHERE M.IsApprove = 1 AND M.IsAcknowledge = 0
                ),RackBinDate As(
					
                        Select  RM.ReceiveID, RackBinDate = Max(RB.DateAdded)
                        From {TableNames.YARN_RECEIVE_CHILD_RACK_BIN} RB
						Inner Join {TableNames.YARN_RECEIVE_CHILD} RMC On RMC.ChildID = RB.ChildID
                        INNER JOIN {TableNames.YARN_RECEIVE_MASTER} RM ON RM.ReceiveID = RMC.ReceiveID
                        INNER JOIN YQCReq M  ON M.ReceiveID = RM.ReceiveID
                        GROUP BY RM.ReceiveID

				),
                FinalList AS
                (
                    Select QCReqMasterID, QCReqNo,	QCReqByUser, QCReqDate,	QCReqFor, IsApprove,IsAcknowledge, YQCReq.ReceiveID,
                    YQCReq.ItemMasterID,YQCReq.PhysicalCount,YQCReq.LotNo,YQCReq.ChallanNo, YQCReq.ChallanDate,YQCReq.ReceiveNo,YQCReq.ReceiveDate,
                    ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc, 
                    ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc,
                    Status = CASE WHEN YQCReq.RetestForRequisitionQCReqMasterID > 0 THEN 'Retest for requisition' 
								  WHEN YQCReq.RetestQCReqMasterID > 0 THEN 'Retest' 
							      ELSE '' END,RBD.RackBinDate
                    From YQCReq
                    INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YQCReq.ItemMasterID 
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                    Left Join RackBinDate RBD On RBD.ReceiveID = YQCReq.ReceiveID
                )
                 SELECT *,Count(*) Over() TotalRows FROM FinalList 
                ";
                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By QCReqMasterID Desc" : paginationInfo.OrderBy;
            }
            else if (status == Status.Acknowledge)
            {
                sql = $@";
                ;With 
                Child AS
                (
	                SELECT RC.QCReqMasterID, ItemMasterID = MAX(RC.ItemMasterID)
	                FROM {TableNames.YARN_QC_REQ_CHILD} RC
                    INNER JOIN {TableNames.YARN_QC_REQ_MASTER} RCM ON RCM.QCReqMasterID = RC.QCReqMasterID
	                WHERE RCM.IsAcknowledge = 1
	                GROUP BY RC.QCReqMasterID
                ),
                YQCReq As 
                (
	                Select M.QCReqMasterID, M.QCReqNo, U.Name QCReqByUser, M.QCReqDate, QCReqFor.ValueName QCReqFor, M.IsApprove, M.IsAcknowledge,
	                M.ReceiveID, RC.ItemMasterID,M.PhysicalCount,M.LotNo,RM.ChallanNo, RM.ChallanDate,RM.ReceiveNo,RM.ReceiveDate, M.RetestQCReqMasterID, M.IsRetestForRequisition, M.RetestForRequisitionQCReqMasterID
	                From {TableNames.YARN_QC_REQ_MASTER} M
	                INNER JOIN Child RC ON RC.QCReqMasterID = M.QCReqMasterID --Max can have 1 Child
	                LEFT JOIN {TableNames.YARN_RECEIVE_MASTER} RM ON RM.ReceiveID=M.ReceiveID
	                LEFT Join {DbNames.EPYSL}..EntityTypeValue QCReqFor On M.QCForID = QCReqFor.ValueID
	                LEFT Join {DbNames.EPYSL}..LoginUser U On M.QCReqBy = U.UserCode
                    WHERE M.IsAcknowledge = 1
                ),RackBinDate As(
					
                        Select  RM.ReceiveID, RackBinDate = Max(RB.DateAdded)
                        From {TableNames.YARN_RECEIVE_CHILD_RACK_BIN} RB
						Inner Join {TableNames.YARN_RECEIVE_CHILD} RMC On RMC.ChildID = RB.ChildID
                        INNER JOIN {TableNames.YARN_RECEIVE_MASTER} RM ON RM.ReceiveID = RMC.ReceiveID
                        INNER JOIN YQCReq M  ON M.ReceiveID = RM.ReceiveID
                        GROUP BY RM.ReceiveID

				),
                FinalList AS
                (
                    Select QCReqMasterID, QCReqNo,	QCReqByUser, QCReqDate,	QCReqFor, IsApprove,IsAcknowledge, YQCReq.ReceiveID,
                    YQCReq.ItemMasterID,YQCReq.PhysicalCount,YQCReq.LotNo,YQCReq.ChallanNo, YQCReq.ChallanDate,YQCReq.ReceiveNo,YQCReq.ReceiveDate,
                    ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc, 
                    ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc,
                    Status = CASE WHEN YQCReq.RetestForRequisitionQCReqMasterID > 0 THEN 'Retest for requisition' 
								  WHEN YQCReq.RetestQCReqMasterID > 0 THEN 'Retest' 
							      ELSE '' END,RBD.RackBinDate
                    From YQCReq
                    INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YQCReq.ItemMasterID 
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                    Left Join RackBinDate RBD On RBD.ReceiveID = YQCReq.ReceiveID
                )
                 SELECT *,Count(*) Over() TotalRows FROM FinalList 
                ";
                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By QCReqMasterID Desc" : paginationInfo.OrderBy;
            }
            else if (status == Status.Reject)
            {
                sql = $@";
                ;With 
                Child AS
                (
	                SELECT RC.QCReqMasterID, ItemMasterID = MAX(RC.ItemMasterID)
	                FROM {TableNames.YARN_QC_REQ_CHILD} RC
                    INNER JOIN {TableNames.YARN_QC_REQ_MASTER} RCM ON RCM.QCReqMasterID = RC.QCReqMasterID
	                WHERE RCM.IsReject = 1
	                GROUP BY RC.QCReqMasterID
                ),
                YQCReq As 
                (
	                Select M.QCReqMasterID, M.QCReqNo, U.Name QCReqByUser, M.QCReqDate, QCReqFor.ValueName QCReqFor, M.IsApprove, M.IsAcknowledge,
	                M.ReceiveID, RC.ItemMasterID,M.PhysicalCount,M.LotNo,RM.ChallanNo, RM.ChallanDate,RM.ReceiveNo,RM.ReceiveDate, M.RetestQCReqMasterID, M.RetestForRequisitionQCReqMasterID,
                    M.RejectReason
	                From {TableNames.YARN_QC_REQ_MASTER} M
	                INNER JOIN Child RC ON RC.QCReqMasterID = M.QCReqMasterID --Max can have 1 Child
	                LEFT JOIN {TableNames.YARN_RECEIVE_MASTER} RM ON RM.ReceiveID=M.ReceiveID
	                LEFT Join {DbNames.EPYSL}..EntityTypeValue QCReqFor On M.QCForID = QCReqFor.ValueID
	                LEFT Join {DbNames.EPYSL}..LoginUser U On M.QCReqBy = U.UserCode
                    WHERE M.IsReject = 1
                ),
                RackBinDate As(
                        Select  RM.ReceiveID, RackBinDate = Max(RB.DateAdded)
                        From {TableNames.YARN_RECEIVE_CHILD_RACK_BIN} RB
						Inner Join {TableNames.YARN_RECEIVE_CHILD} RMC On RMC.ChildID = RB.ChildID
                        INNER JOIN {TableNames.YARN_RECEIVE_MASTER} RM ON RM.ReceiveID = RMC.ReceiveID
                        INNER JOIN YQCReq M  ON M.ReceiveID = RM.ReceiveID
                        GROUP BY RM.ReceiveID
				),
                FinalList AS
                (
                    Select QCReqMasterID, QCReqNo,	QCReqByUser, QCReqDate,	QCReqFor, IsApprove,IsAcknowledge, YQCReq.ReceiveID,
                    YQCReq.ItemMasterID,YQCReq.PhysicalCount,YQCReq.LotNo,YQCReq.ChallanNo, YQCReq.ChallanDate,YQCReq.ReceiveNo,YQCReq.ReceiveDate,
                    ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc, 
                    ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc,
                    Status = CASE WHEN YQCReq.RetestForRequisitionQCReqMasterID > 0 THEN 'Retest for requisition' 
								  WHEN YQCReq.RetestQCReqMasterID > 0 THEN 'Retest' 
							      ELSE '' END, RejectReason,RBD.RackBinDate
                    From YQCReq
                    LEFT JOIN RackBinDate RBD ON RBD.ReceiveID = YQCReq.ReceiveID
                    INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YQCReq.ItemMasterID 
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                )
                 SELECT *,Count(*) Over() TotalRows FROM FinalList 
                ";
                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By QCReqMasterID Desc" : paginationInfo.OrderBy;
            }
            else if (status == Status.ReTest)
            {
                sql = $@";
                With M AS(
                    SELECT QCRC.QCRemarksChildID, ReceiveChildID = RMC.ChildID, RM.ReceiveID, RMM2.QCReqMasterID, RMM2.QCReqNo, RM.ReceiveDate, RM.ReceiveNo, RM.LocationID, RM.RCompanyID, RM.OCompanyID, RM.SupplierID, RM.LCNo, RM.LCDate, RM.Tolerance,
                    RM.InvoiceNo, RM.InvoiceDate, RM.BLNo, RM.BLDate, RM.BankBranchID, RM.PLNo, RM.PLDate, RM.ChallanNo, RM.ChallanDate, RM.GPNo, RM.GPDate, RM.TransportMode,
                    RM.TransportTypeID, RM.CContractorID, RM.VehicalNo, RM.ShipmentStatus, RM.Remarks, SpinnerID = CASE WHEN RMC.SpinnerID > 0 THEN RMC.SpinnerID ELSE RM.SpinnerID END, RM.POID, RM.CIID, RM.LCID, RM.ReceivedById, RM.GPTime,
                    RM.PONo, RM.PODate,RMC.ItemMasterID,RMC.ShadeCode,RMC.LotNo,RMC.ChallanLot, RMC.ReceiveQty, RMC.PhysicalCount, QCRM.IsRetestForRequisition,
                    [Status] = CASE WHEN QCRM.IsRetestForRequisition = 1 THEN 'Retest for Requisition' 
	                                WHEN QCRC.Diagnostic = 1 
					                THEN 'Diagnostic' 
					                ELSE 'ReTest' END,
		            ParentQCRemarksNo = CASE WHEN ISNULL(QCRM.IsRetest,0) = 1 THEN QCRM.QCRemarksNo ELSE '' END, 
		            RetestParentQCRemarksMasterID = CASE WHEN ISNULL(QCRM.IsRetest,0) = 1 THEN QCRM.QCRemarksMasterID ELSE 0 END,
                    RMC.POChildID
                    FROM {TableNames.YARN_QC_REMARKS_CHILD} QCRC
		            INNER JOIN {TableNames.YARN_QC_REMARKS_MASTER} QCRM ON QCRM.QCRemarksMasterID = QCRC.QCRemarksMasterID
	                INNER JOIN {TableNames.YARN_QC_RECEIVE_CHILD} RC1 ON RC1.QCReceiveChildID = QCRC.QCReceiveChildID
	                INNER JOIN {TableNames.YARN_QC_ISSUE_CHILD} RIC ON RIC.QCIssueChildID = RC1.QCIssueChildID
	                INNER JOIN {TableNames.YARN_QC_REQ_CHILD} RMC2 ON RMC2.QCReqChildID = RIC.QCReqChildID
                    INNER JOIN {TableNames.YARN_QC_REQ_MASTER} RMM2 ON RMM2.QCReqMasterID = RMC2.QCReqMasterID
	                INNER JOIN {TableNames.YARN_RECEIVE_CHILD} RMC ON RMC.ChildID = RMC2.ReceiveChildID
	                INNER JOIN {TableNames.YARN_RECEIVE_MASTER} RM ON RM.ReceiveID = RMC.ReceiveID
					LEFT JOIN {TableNames.YARN_MRIR_CHILD} YMC ON YMC.ReceiveChildID=RMC.ChildID
                    WHERE QCRC.Diagnostic = 1 
	                OR QCRC.ReTest = 1 
	                OR QCRM.IsRetest = 1 
	                OR YMC.ReTest=1
	                OR QCRM.IsRetestForRequisition = 1
                ),
                POInfo AS
                (
                    SELECT M.ReceiveChildID
	                ,BuyerName = CASE WHEN ISNULL(PoC.BuyerID,0) > 0 THEN C1.ShortName ELSE STRING_AGG(C.ShortName,',') END
                    FROM M
                    INNER join {TableNames.YarnPOChild} PoC On Poc.YPOChildID = M.POChildID
                    LEFT Join {TableNames.YarnPOChildBuyer} YPB ON YPB.YPOChildID = PoC.YPOChildID
                    LEFT JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = YPB.BuyerID AND C.ContactID > 0
	                LEFT JOIN {DbNames.EPYSL}..Contacts C1 ON C1.ContactID = PoC.BuyerID AND C1.ContactID > 0
                    GROUP BY M.ReceiveChildID,PoC.BuyerID,C1.ShortName
                ),
                EWOInfo AS
                (
	                Select M.ReceiveChildID, EWO = STRING_AGG(EOM.ExportOrderNo,',')
	                From M
	                INNER JOIN {TableNames.YarnPOChildOrder} YO ON YO.YPOMasterID = M.POID
	                LEFT JOIN {DbNames.EPYSL}..ExportOrderMaster EOM ON EOM.ExportOrderID = YO.ExportOrderID
	                GROUP BY M.ReceiveChildID
                ),
                POFor AS
                (
	                SELECT M.ReceiveChildID, POFor = STRING_AGG(PO.ValueName,',') 
	                FROM M
	                INNER join {TableNames.YarnPOChild} PoC On Poc.YPOMasterID = M.POID And Poc.ItemMasterID = M.ItemMasterID
	                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue PO ON PO.ValueID = PoC.POForID
	                GROUP BY M.ReceiveChildID
                ),
                RackBin AS
                (
	                Select M.ReceiveChildID, RackNo = STRING_AGG(RC.RackNo,',')
	                From M
	                INNER JOIN {TableNames.YARN_RECEIVE_CHILD_RACK_BIN} RB ON RB.ChildID = M.ReceiveChildID
	                INNER JOIN {DbNames.EPYSL}..Rack RC ON RC.RackID = RB.RackID
	                GROUP BY M.ReceiveChildID
                ),
                RackBinDate As(
                        Select M.ReceiveChildID,DateAdded = Max(RB.DateAdded)
                        From {TableNames.YARN_RECEIVE_CHILD_RACK_BIN} RB
                        INNER JOIN M  ON RB.ChildID = M.ReceiveChildID
                        GROUP BY M.ReceiveChildID

				),
                FinalList AS
                (
                    SELECT M.QCRemarksChildID, M.ReceiveChildID, M.ReceiveID, M.QCReqMasterID, M.ReceiveDate, M.[Status], M.QCReqNo, M.ReceiveNo, M.LocationID, M.RCompanyID, M.OCompanyID, M.SupplierID, M.LCNo, M.LCDate, M.Tolerance,
	                M.InvoiceNo, M.InvoiceDate, M.BLNo, M.BLDate, M.BankBranchID, M.PLNo, M.PLDate, M.ChallanNo, M.ChallanDate, M.GPNo, M.GPDate, M.TransportMode,
	                M.TransportTypeID, M.CContractorID, M.VehicalNo, M.ShipmentStatus, M.Remarks, M.SpinnerID, M.POID, M.CIID, M.LCID, M.ReceivedById, M.GPTime,
	                CC.[Name] SupplierName, SS.[Name] Spinner,BB.BranchName BankBranchName, COM.ShortName RCompany, M.PONo, M.PODate,M.ItemMasterID,
                    ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc, 
                    ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc,M.ShadeCode,M.LotNo,M.ChallanLot,
	                ImportCategory = CASE WHEN ACI.InLand = 1 THEN 'Local' ELSE CASE WHEN ACI.InHouse = 1 THEN 'In house' ELSE 'Foreign' END END, M.IsRetestForRequisition,
	                ReceivedQtyInKg = M.ReceiveQty, POI.BuyerName, EWOI.EWO, POF.POFor, M.PhysicalCount, RB.RackNo, M.ParentQCRemarksNo, M.RetestParentQCRemarksMasterID, RackBinDate  = RBD.DateAdded
                    FROM M
                    LEFT JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = M.SupplierID
	                LEFT JOIN {DbNames.EPYSL}..ContactAdditionalInfo ACI ON ACI.ContactID = CC.ContactID
                    LEFT JOIN {DbNames.EPYSL}..BankBranch BB ON BB.BankBranchID = M.BankBranchID
                    LEFT JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID = M.RCompanyID
                    LEFT JOIN {DbNames.EPYSL}..Contacts SS ON SS.ContactID = M.SpinnerID
                    INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = M.ItemMasterID 
	                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
	                LEFT JOIN POInfo POI ON POI.ReceiveChildID = M.ReceiveChildID
	                LEFT JOIN EWOInfo EWOI ON EWOI.ReceiveChildID = M.ReceiveChildID
	                LEFT JOIN POFor POF ON POF.ReceiveChildID = M.ReceiveChildID
	                LEFT JOIN RackBin RB ON RB.ReceiveChildID = M.ReceiveChildID
					Left Join RackBinDate RBD On RBD.ReceiveChildID =  M.ReceiveChildID
                )
                SELECT *,Count(*) Over() TotalRows FROM FinalList
                ";
                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By ReceiveID Desc,ItemMasterID" : paginationInfo.OrderBy;
            }
            else
            {
                sql = $@";
                ;With 
                Child AS
                (
	                SELECT RC.QCReqMasterID, RC.ChallanLot, ItemMasterID = MAX(RC.ItemMasterID)
	                FROM {TableNames.YARN_QC_REQ_CHILD} RC
                    INNER JOIN {TableNames.YARN_QC_REQ_MASTER} RCM ON RCM.QCReqMasterID = RC.QCReqMasterID
	                WHERE RCM.IsSendForApproval = 0 AND RCM.IsApprove = 0 AND RCM.IsAcknowledge = 0
	                GROUP BY RC.QCReqMasterID, RC.ChallanLot
                ),
                YQCReq As 
                (
	                Select M.QCReqMasterID, M.QCReqNo, U.Name QCReqByUser, M.QCReqDate, QCReqFor.ValueName QCReqFor, M.IsApprove, M.IsAcknowledge,
	                M.ReceiveID, RC.ItemMasterID,M.PhysicalCount,M.LotNo,RC.ChallanLot,RM.ChallanNo, RM.ChallanDate,RM.ReceiveNo,RM.ReceiveDate, M.RetestQCReqMasterID, M.IsRetestForRequisition, M.RetestForRequisitionQCReqMasterID
	                From {TableNames.YARN_QC_REQ_MASTER} M
	                INNER JOIN Child RC ON RC.QCReqMasterID = M.QCReqMasterID --Max can have 1 Child
	                LEFT JOIN {TableNames.YARN_RECEIVE_MASTER} RM ON RM.ReceiveID=M.ReceiveID
	                LEFT Join {DbNames.EPYSL}..EntityTypeValue QCReqFor On M.QCForID = QCReqFor.ValueID
	                LEFT Join {DbNames.EPYSL}..LoginUser U On M.QCReqBy = U.UserCode
                    WHERE M.IsSendForApproval = 0 AND M.IsApprove = 0 AND M.IsAcknowledge = 0
                ),RackBinDate As(
					
                        Select  RM.ReceiveID, RackBinDate = Max(RB.DateAdded)
                        From {TableNames.YARN_RECEIVE_CHILD_RACK_BIN} RB
						Inner Join {TableNames.YARN_RECEIVE_CHILD} RMC On RMC.ChildID = RB.ChildID
                        INNER JOIN {TableNames.YARN_RECEIVE_MASTER} RM ON RM.ReceiveID = RMC.ReceiveID
                        INNER JOIN YQCReq M  ON M.ReceiveID = RM.ReceiveID
                        GROUP BY RM.ReceiveID

				),
                FinalList AS
                (
                    Select QCReqMasterID, QCReqNo,	QCReqByUser, QCReqDate,	QCReqFor, IsApprove,IsAcknowledge, YQCReq.ReceiveID,
                    YQCReq.ItemMasterID,YQCReq.PhysicalCount,YQCReq.LotNo,YQCReq.ChallanLot, YQCReq.ChallanNo, YQCReq.ChallanDate,YQCReq.ReceiveNo,YQCReq.ReceiveDate,
                    ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc, 
                    ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc,
                    Status = CASE WHEN YQCReq.RetestForRequisitionQCReqMasterID > 0 THEN 'Retest for requisition' 
								  WHEN YQCReq.RetestQCReqMasterID > 0 THEN 'Retest' 
							      ELSE '' END, RBD.RackBinDate
                    From YQCReq
                    INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YQCReq.ItemMasterID 
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID  
                    Left Join RackBinDate RBD On RBD.ReceiveID = YQCReq.ReceiveID
                )
                SELECT *,Count(*) Over() TotalRows FROM FinalList 
                ";

                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By QCReqMasterID Desc" : paginationInfo.OrderBy;
            }
            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            if (status == Status.Hold)
            {
                sql += $@" DROP TABLE #TempTable{tempGuid}";
            }

            var list = await _service.GetDataAsync<YarnQCReqMaster>(sql);
            list.ForEach(x =>
            {
                if (x.POFor.IsNotNull() && x.POFor != "")
                {
                    var propList = x.POFor.Split(',');
                    x.POFor = string.Join(",", propList.Distinct());
                }
            });
            return list;
        }
        public async Task<YarnQCReqMaster> GetNewAsync()
        {
            var query =
                $@"SELECT CAST(ValueID AS VARCHAR) id, ValueName text 
                FROM {DbNames.EPYSL}..EntityTypeValue 
                WHERE EntityTypeID = {EntityTypeConstants.YARN_QC_REQ_FOR} AND ValueName <> 'Select'
                ORDER BY ValueName;

                Select Cast(ReceiveID As varchar) [id], ReceiveNo [text], C.ShortName [desc] 
                From {TableNames.YARN_RECEIVE_MASTER} RM
                Inner Join {DbNames.EPYSL}..Contacts C On RM.SupplierID = C.ContactID
                Order By RM.DateAdded Desc";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                YarnQCReqMaster data = new YarnQCReqMaster
                {
                    ReceiveDate = null,
                    QCForList = records.Read<Select2OptionModel>().ToList(),
                    ReceiveList = records.Read<Select2OptionModel>().ToList()
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
        public async Task<YarnQCReqMaster> GetAsync(int id, Status status, int itemMasterID)
        {
            string itemCon = "";
            if (status == Status.ReTest)
            {
                itemCon = $@" AND RC.ItemMasterID = {itemMasterID} ";
            }
            var sql = string.Empty;
            if (status == Status.Pending)
            {
                sql = $@"
                ;WITH M AS (
	                SELECT RM.ReceiveID, RM.ReceiveDate, RM.ReceiveNo, RM.LocationID, RM.RCompanyID, RM.OCompanyID, RM.SupplierID, RM.LCNo, RM.LCDate, RM.Tolerance,
				    RM.InvoiceNo, RM.InvoiceDate, RM.BLNo, RM.BLDate, RM.BankBranchID, RM.PLNo, RM.PLDate, RM.ChallanNo, RM.ChallanDate, RM.GPNo, RM.GPDate, RM.TransportMode,
				    RM.TransportTypeID, RM.CContractorID, RM.VehicalNo, RM.ShipmentStatus, RM.Remarks, RM.SpinnerID, RM.POID, RM.CIID, RM.LCID, RM.ReceivedById, RM.GPTime, 
				    RM.CurrencyID, RM.PONo, RM.PODate, RM.ACompanyInvoice
                    FROM {TableNames.YARN_RECEIVE_MASTER} RM WHERE RM.ReceiveId = {id}
                )
                SELECT	M.ReceiveID, M.ReceiveDate, M.ReceiveNo, M.LocationID, M.RCompanyID, M.OCompanyID, M.SupplierID, M.LCNo, M.LCDate, M.Tolerance,
				M.InvoiceNo, M.InvoiceDate, M.BLNo, M.BLDate, M.BankBranchID, M.PLNo, M.PLDate, M.ChallanNo, M.ChallanDate, M.GPNo, M.GPDate, M.TransportMode,
				M.TransportTypeID, M.CContractorID, M.VehicalNo, M.ShipmentStatus, M.Remarks, M.SpinnerID, M.POID, M.CIID, M.LCID, M.ReceivedById, M.GPTime,
				 CC.[Name] Supplier, SS.[Name] Spinner, BB.BranchName BankBranchName, COM.CompanyName RCompany, M.CurrencyID, M.PONo, M.PODate, M.ACompanyInvoice
		        FROM M
                LEFT JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = M.SupplierID
                LEFT JOIN {DbNames.EPYSL}..Contacts SS ON SS.ContactID = M.SpinnerID
				LEFT JOIN {DbNames.EPYSL}..BankBranch BB ON BB.BankBranchID = M.BankBranchID
				LEFT JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID = M.RCompanyID;
                
                -----childs
                ;With 
                X AS (
	                SELECT RC.ChildID, RC.ReceiveID, RC.ItemMasterID, RC.InvoiceChildID, RC.POChildID, RC.UnitID, RC.InvoiceQty, RC.ChallanQty, RC.ShortQty, RC.ExcessQty,
	                RC.ReceiveQty, RC.Rate, RC.Remarks, RC.LotNo, RC.ChallanLot, RC.NoOfCartoon, RC.NoOfCone, RC.POQty, RC.YarnProgramId, RC.ChallanCount, RC.PhysicalCount, RC.ShadeCode
	                FROM {TableNames.YARN_RECEIVE_CHILD} RC 
                    WHERE RC.ReceiveID = {id}
                )
                SELECT X.ChildID, ReceiveChildID = X.ChildID, X.ReceiveID, X.ItemMasterID, X.InvoiceChildID, X.POChildID, X.UnitID, X.InvoiceQty, X.POQty, X.ChallanQty, X.ShortQty, X.ExcessQty, X.ReceiveQty, 
                X.Rate, X.Remarks, X.LotNo, X.ChallanLot, X.NoOfCartoon, X.NoOfCone, UU.DisplayUnitDesc, X.YarnProgramId, X.ChallanCount, X.PhysicalCount, 
				ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc, 
                ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc, X.ShadeCode
                FROM X
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = X.ItemMasterID 
				LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                Inner Join {DbNames.EPYSL}..Unit AS UU On UU.UnitID = X.UnitID;

                 ;SELECT CAST(ValueID AS VARCHAR) id, ValueName text 
                FROM {DbNames.EPYSL}..EntityTypeValue 
                WHERE EntityTypeID = {EntityTypeConstants.YARN_QC_REQ_FOR} AND ValueName <> 'Select'
                ORDER BY ValueName;

                ;Select Cast(ReceiveID As varchar) [id], ReceiveNo [text], C.ShortName [desc] 
                From {TableNames.YARN_RECEIVE_MASTER} RM
                Inner Join {DbNames.EPYSL}..Contacts C On RM.SupplierID = C.ContactID
                Order By RM.DateAdded Desc;";
            }
            else
            {
                sql =
               $@"
                ;WITH M AS (
                    Select NeedUSTER,NeedYarnTRF,NeedFabricTRF,QCReqMasterID, QCReqNo, QCReqBy, QCReqDate, QCForID, IsApprove, ApproveDate, ApproveBy, IsAcknowledge, AcknowledgeDate, AcknowledgeBy, ReceiveID, 
                    LocationID, RCompanyID, CompanyID, SupplierID, SpinnerID, RejectReason
                    From {TableNames.YARN_QC_REQ_MASTER} Where QCReqMasterID = {id}
                )
                SELECT M.NeedUSTER,M.NeedYarnTRF,M.NeedFabricTRF,M.QCReqMasterID, M.QCReqNo, M.QCReqBy, M.QCReqDate, QCForID, M.IsApprove, M.ApproveDate, M.ApproveBy, M.IsAcknowledge, M.AcknowledgeDate, M.AcknowledgeBy, M.ReceiveID, 
                M.LocationID, M.RCompanyID, M.CompanyID, M.SupplierID, M.SpinnerID, CC.[Name] Supplier, SS.[Name] Spinner, COM.CompanyName RCompany, RM.ReceiveNo, RM.ReceiveDate, M.RejectReason
                FROM M
				LEFT JOIN {TableNames.YARN_RECEIVE_MASTER} RM ON RM.ReceiveID = M.ReceiveID
                LEFT JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = M.SupplierID
                LEFT JOIN {DbNames.EPYSL}..Contacts SS ON SS.ContactID = M.SpinnerID
                LEFT JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID = M.RCompanyID;

                ;WITH YRC AS (
	                SELECT RC.QCReqChildID, RC.QCReqMasterID, RC.LotNo, RC.ChallanLot, RC.ItemMasterID, RC.ReqQty, YRC.ReceiveQty,  RC.ReqCone,RC.UnitID, RC.Rate,
	                RC.YarnProgramId, RC.ChallanCount, RC.POCount, YRC.PhysicalCount, RC.NoOfThread,RC.ShadeCode, 
		            RC.ReceiveChildID, YRM.ReceiveID, YRM.ReceiveNo, YRM.ReceiveDate,
                    RC.MachineTypeId, RC.TechnicalNameId, RC.BuyerID, RC.ReqBagPcs,
		            SpinnerID = CASE WHEN YRC.SpinnerID > 0 THEN YRC.SpinnerID ELSE YRM.SpinnerId END, 
		            YRM.SupplierID, YRM.POID, YRC.POChildID, YRC.NoOfCartoon, YRC.NoOfCone,RC.QCReqRemarks, YRC.YarnCategory
	                FROM {TableNames.YARN_QC_REQ_CHILD} RC
		            LEFT JOIN {TableNames.YARN_RECEIVE_CHILD} YRC ON YRC.ChildID = RC.ReceiveChildID
		            LEFT JOIN {TableNames.YARN_RECEIVE_MASTER} YRM ON YRM.ReceiveID = YRC.ReceiveID
		            WHERE RC.QCReqMasterID = {id} {itemCon}
                ),
	            POFor AS
                (
	                SELECT YRC.POChildID, POFor = STRING_AGG(PO.ValueName,','), Poc.YPOMasterID
	                FROM YRC
	                INNER join {TableNames.YarnPOChild} PoC ON Poc.YPOMasterID = YRC.POID And Poc.ItemMasterID = YRC.ItemMasterID
	                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue PO ON PO.ValueID = PoC.POForID
	                GROUP BY YRC.POChildID, Poc.YPOMasterID
                )
                SELECT YRC.QCReqChildID, YRC.QCReqMasterID, YRC.LotNo, YRC.ChallanLot, YRC.ItemMasterID, YRC.ReqQty, YRC.ReqCone,YRC.ReceiveQty,  YRC.UnitID, YRC.Rate, 
		            YRC.ReceiveID, YRC.ReceiveNo, YRC.ReceiveDate, YRC.ReceiveChildID,
                    YRC.MachineTypeId, YRC.TechnicalNameId, YRC.BuyerID, YRC.ReqBagPcs,
	                YRC.YarnProgramId, YRC.ChallanCount, YRC.POCount, YRC.PhysicalCount, YRC.YarnCategory, YRC.NoOfThread, UU.DisplayUnitDesc Uom, 
	                ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc, 
		            ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc,YRC.ShadeCode,
		            POF.POFor, YRC.NoOfCartoon, YRC.NoOfCone, UU.DisplayUnitDesc,
		            ImportCategory = CASE WHEN ACI.InLand = 1 THEN 'Local' ELSE CASE WHEN ACI.InHouse = 1 THEN 'In house' ELSE 'Foreign' END END,
		            SupplierName = CC.[Name], YRC.QCReqRemarks,
		            Spinner = CASE WHEN YRC.SpinnerID > 0 THEN SS.[Name] ELSE '' END,
		            YarnDetail = YRC.YarnCategory --CONCAT(ISV6.SegmentValue,' ', ISV1.SegmentValue,' ', ISV3.SegmentValue,' ', ISV4.SegmentValue,' ', ISV2.SegmentValue,' ', ISV5.SegmentValue,' ', YRC.ShadeCode)
                FROM YRC
	            LEFT JOIN POFor POF ON POF.POChildID = YRC.POChildID AND POF.YPOMasterID = YRC.POID
	            LEFT JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = YRC.SupplierID
	            LEFT JOIN {DbNames.EPYSL}..Contacts SS ON SS.ContactID = YRC.SpinnerID
	            LEFT JOIN {DbNames.EPYSL}..ContactAdditionalInfo ACI ON ACI.ContactID = CC.ContactID
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YRC.ItemMasterID 
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                LEFT Join {DbNames.EPYSL}..Unit AS UU On UU.UnitID = YRC.UnitID;

                ;SELECT CAST(ValueID AS VARCHAR) id, ValueName text 
                FROM {DbNames.EPYSL}..EntityTypeValue 
                WHERE EntityTypeID = {EntityTypeConstants.YARN_QC_REQ_FOR} AND ValueName <> 'Select'
                ORDER BY ValueName;

                ;Select Cast(ReceiveID As varchar) [id], ReceiveNo [text], C.ShortName [desc] 
                From {TableNames.YARN_RECEIVE_MASTER} RM
                Inner Join {DbNames.EPYSL}..Contacts C On RM.SupplierID = C.ContactID
                Order By RM.DateAdded Desc;

                --Machine Type
                ;SELECT CAST(a.MachineSubClassID AS varchar) [id], b.SubClassName [text], b.TypeID [desc], c.TypeName additionalValue
                FROM {TableNames.KNITTING_MACHINE} a
                INNER JOIN {TableNames.KNITTING_MACHINE_SUBCLASS} b ON b.SubClassID = a.MachineSubClassID
                Inner Join {TableNames.KNITTING_MACHINE_TYPE} c On c.TypeID = b.TypeID
                --Where c.TypeName != 'Flat Bed'
                GROUP BY a.MachineSubClassID, b.SubClassName, b.TypeID, c.TypeName;

                 --Technical Name
                SELECT Cast(T.TechnicalNameId As varchar) id, T.TechnicalName [text], ISNULL(ST.[Days], 0) [desc], Cast(SC.SubClassID as varchar) additionalValue
                FROM {TableNames.FabricTechnicalName} T
                LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME_KMACHINE_SUB_CLASS} SC ON SC.TechnicalNameID = T.TechnicalNameId
                LEFT JOIN {TableNames.KNITTING_MACHINE_STRUCTURE_TYPE_HK} ST ON ST.StructureTypeID = SC.StructureTypeID
                Group By T.TechnicalNameId, T.TechnicalName, ST.Days, SC.SubClassID;

                -- Buyers
                 {CommonQueries.GetContactsByCategoryType(ContactCategoryNames.BUYER)};";

            }
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YarnQCReqMaster data = await records.ReadFirstOrDefaultAsync<YarnQCReqMaster>();
                Guard.Against.NullObject(data);
                data.YarnQCReqChilds = records.Read<YarnQCReqChild>().ToList();
                data.QCForList = records.Read<Select2OptionModel>().ToList();
                data.ReceiveList = records.Read<Select2OptionModel>().ToList();
                data.MCTypeForFabricList = records.Read<Select2OptionModel>().ToList();
                data.TechnicalNameList = records.Read<Select2OptionModel>().ToList();
                data.BuyerList = records.Read<Select2OptionModel>().ToList();

                data.YarnQCReqChilds.ForEach(c =>
                {
                    if (c.MachineTypeId > 0) c.MachineType = data.MCTypeForFabricList.Find(m => m.id == c.MachineTypeId.ToString()).text;
                    if (c.TechnicalNameId > 0) c.TechnicalName = data.TechnicalNameList.Find(m => m.id == c.TechnicalNameId.ToString()).text;
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
        public async Task<YarnQCReqMaster> GetByRemarksChildId(int id, int qcRemarksChildID)
        {
            string sql =
             $@"
                ;WITH M AS (
                    SELECT M.NeedUSTER,M.NeedYarnTRF,M.NeedFabricTRF,M.QCReqMasterID,M.QCReqNo,M.QCReqBy,M.QCReqDate,M.QCForID,M.IsApprove,M.ApproveDate,M.ApproveBy,M.IsAcknowledge,M.AcknowledgeDate,M.AcknowledgeBy,M.ReceiveID,
                    M.LocationID, M.RCompanyID, M.CompanyID, M.SupplierID, M.SpinnerID 
                    FROM {TableNames.YARN_QC_REQ_MASTER} M
				    WHERE M.QCReqMasterID = {id}
                ),
			    RemarksMaster AS
			    (
				    SELECT M.QCReqMasterID, M.QCRemarksMasterID, M.IsRetestForRequisition
				    FROM {TableNames.YARN_QC_REMARKS_CHILD} C
				    INNER JOIN {TableNames.YARN_QC_REMARKS_MASTER} M ON M.QCRemarksMasterID = C.QCRemarksMasterID
				    WHERE C.QCRemarksChildID = {qcRemarksChildID} AND (M.IsRetest = 1 OR M.IsRetestForRequisition = 1)
			    )
                SELECT M.NeedUSTER,M.NeedYarnTRF,M.NeedFabricTRF,M.QCReqMasterID, M.QCReqNo, M.QCReqBy, M.QCReqDate, M.QCForID, M.IsApprove, M.ApproveDate, M.ApproveBy, M.IsAcknowledge, M.AcknowledgeDate, M.AcknowledgeBy, M.ReceiveID, 
                M.LocationID, M.RCompanyID, M.CompanyID, M.SupplierID, M.SpinnerID, CC.[Name] Supplier, SS.[Name] Spinner, COM.CompanyName RCompany, RM.ReceiveNo, RM.ReceiveDate,
			    RetestParentQCRemarksMasterID = ISNULL(REM.QCRemarksMasterID,0), IsRetestForRequisition = ISNULL(REM.IsRetestForRequisition,0)
                FROM M
			    LEFT JOIN RemarksMaster REM ON REM.QCReqMasterID = M.QCReqMasterID
			    LEFT JOIN {TableNames.YARN_RECEIVE_MASTER} RM ON RM.ReceiveID = M.ReceiveID
                LEFT JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = M.SupplierID
                LEFT JOIN {DbNames.EPYSL}..Contacts SS ON SS.ContactID = M.SpinnerID
                LEFT JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID = M.RCompanyID;

                ;WITH YRC AS (
	                SELECT RC.QCReqChildID, RC.QCReqMasterID, RC.LotNo, RC.ChallanLot, RC.ItemMasterID, RC.ReqQty,  RC.ReqCone,RC.UnitID, RC.Rate,
	                RC.YarnProgramId, RC.ChallanCount, RC.POCount, RC.PhysicalCount, RC.NoOfThread,RC.ShadeCode, 
		            RC.ReceiveChildID, YRM.ReceiveID, YRM.ReceiveNo, YRM.ReceiveDate,
                    RC.MachineTypeId, RC.TechnicalNameId, RC.BuyerID, RC.ReqBagPcs,
		            SpinnerID = CASE WHEN YRC.SpinnerID > 0 THEN YRC.SpinnerID ELSE YRM.SpinnerId END, 
		            YRM.SupplierID, YRM.POID, YRC.POChildID, YRC.NoOfCartoon, YRC.NoOfCone, YRC.YarnCategory
	                FROM {TableNames.YARN_QC_REMARKS_CHILD} QCRC
	                INNER JOIN {TableNames.YARN_QC_RECEIVE_CHILD} RC1 ON RC1.QCReceiveChildID = QCRC.QCReceiveChildID
	                INNER JOIN {TableNames.YARN_QC_ISSUE_CHILD} RIC ON RIC.QCIssueChildID = RC1.QCIssueChildID
	                INNER JOIN {TableNames.YARN_QC_REQ_CHILD} RC ON RC.QCReqChildID = RIC.QCReqChildID
	                LEFT JOIN {TableNames.YARN_RECEIVE_CHILD} YRC ON YRC.ChildID = RC.ReceiveChildID
	                LEFT JOIN {TableNames.YARN_RECEIVE_MASTER} YRM ON YRM.ReceiveID = YRC.ReceiveID
	                WHERE QCRC.QCRemarksChildID = {qcRemarksChildID}
                ),
	            POFor AS
                (
	                SELECT YRC.POChildID, POFor = STRING_AGG(PO.ValueName,','), Poc.YPOMasterID
	                FROM YRC
	                INNER join {TableNames.YarnPOChild} PoC ON Poc.YPOMasterID = YRC.POID And Poc.ItemMasterID = YRC.ItemMasterID
	                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue PO ON PO.ValueID = PoC.POForID
	                GROUP BY YRC.POChildID, Poc.YPOMasterID
                )
                SELECT YRC.QCReqChildID, YRC.QCReqMasterID, YRC.LotNo, YRC.ChallanLot, YRC.ItemMasterID, YRC.ReqQty, YRC.ReqCone,YRC.ReqQty ReceiveQty,  YRC.UnitID, YRC.Rate, 
		            YRC.ReceiveID, YRC.ReceiveNo, YRC.ReceiveDate, YRC.ReceiveChildID,
                    YRC.MachineTypeId, YRC.TechnicalNameId, YRC.BuyerID, YRC.ReqBagPcs,
	                YRC.YarnProgramId, YRC.ChallanCount, YRC.POCount, YRC.PhysicalCount, YRC.YarnCategory, YRC.NoOfThread, UU.DisplayUnitDesc Uom, 
	                ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc, 
		            ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc,YRC.ShadeCode,
		            POF.POFor, YRC.NoOfCartoon, YRC.NoOfCone, UU.DisplayUnitDesc,
		            ImportCategory = CASE WHEN ACI.InLand = 1 THEN 'Local' ELSE CASE WHEN ACI.InHouse = 1 THEN 'In house' ELSE 'Foreign' END END,
		            SupplierName = CC.[Name], 
		            Spinner = CASE WHEN YRC.SpinnerID > 0 THEN SS.[Name] ELSE '' END,
		            YarnDetail = YRC.YarnCategory --CONCAT(ISV6.SegmentValue,' ', ISV1.SegmentValue,' ', ISV3.SegmentValue,' ', ISV4.SegmentValue,' ', ISV2.SegmentValue,' ', ISV5.SegmentValue,' ', YRC.ShadeCode)
                FROM YRC
	            LEFT JOIN POFor POF ON POF.POChildID = YRC.POChildID AND POF.YPOMasterID = YRC.POID
	            LEFT JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = YRC.SupplierID
	            LEFT JOIN {DbNames.EPYSL}..Contacts SS ON SS.ContactID = YRC.SpinnerID
	            LEFT JOIN {DbNames.EPYSL}..ContactAdditionalInfo ACI ON ACI.ContactID = CC.ContactID
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YRC.ItemMasterID 
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                LEFT Join {DbNames.EPYSL}..Unit AS UU On UU.UnitID = YRC.UnitID;

                ;SELECT CAST(ValueID AS VARCHAR) id, ValueName text 
                FROM {DbNames.EPYSL}..EntityTypeValue 
                WHERE EntityTypeID = {EntityTypeConstants.YARN_QC_REQ_FOR} AND ValueName <> 'Select'
                ORDER BY ValueName;

                ;Select Cast(ReceiveID As varchar) [id], ReceiveNo [text], C.ShortName [desc] 
                From {TableNames.YARN_RECEIVE_MASTER} RM
                Inner Join {DbNames.EPYSL}..Contacts C On RM.SupplierID = C.ContactID
                Order By RM.DateAdded Desc;

                --Machine Type
                ;SELECT CAST(a.MachineSubClassID AS varchar) [id], b.SubClassName [text], b.TypeID [desc], c.TypeName additionalValue
                FROM {TableNames.KNITTING_MACHINE} a
                INNER JOIN {TableNames.KNITTING_MACHINE_SUBCLASS} b ON b.SubClassID = a.MachineSubClassID
                Inner Join {TableNames.KNITTING_MACHINE_TYPE} c On c.TypeID = b.TypeID
                --Where c.TypeName != 'Flat Bed'
                GROUP BY a.MachineSubClassID, b.SubClassName, b.TypeID, c.TypeName;

                 --Technical Name
                SELECT Cast(T.TechnicalNameId As varchar) id, T.TechnicalName [text], ISNULL(ST.[Days], 0) [desc], Cast(SC.SubClassID as varchar) additionalValue
                FROM {TableNames.FabricTechnicalName} T
                LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME_KMACHINE_SUB_CLASS} SC ON SC.TechnicalNameID = T.TechnicalNameId
                LEFT JOIN {TableNames.KNITTING_MACHINE_STRUCTURE_TYPE_HK} ST ON ST.StructureTypeID = SC.StructureTypeID
                Group By T.TechnicalNameId, T.TechnicalName, ST.Days, SC.SubClassID;

                -- Buyers
                 {CommonQueries.GetContactsByCategoryType(ContactCategoryNames.BUYER)};";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YarnQCReqMaster data = await records.ReadFirstOrDefaultAsync<YarnQCReqMaster>();
                Guard.Against.NullObject(data);
                data.YarnQCReqChilds = records.Read<YarnQCReqChild>().ToList();
                data.QCForList = records.Read<Select2OptionModel>().ToList();
                data.ReceiveList = records.Read<Select2OptionModel>().ToList();
                data.MCTypeForFabricList = records.Read<Select2OptionModel>().ToList();
                data.TechnicalNameList = records.Read<Select2OptionModel>().ToList();
                data.BuyerList = records.Read<Select2OptionModel>().ToList();

                data.YarnQCReqChilds.ForEach(c =>
                {
                    if (c.MachineTypeId > 0) c.MachineType = data.MCTypeForFabricList.Find(m => m.id == c.MachineTypeId.ToString()).text;
                    if (c.TechnicalNameId > 0) c.TechnicalName = data.TechnicalNameList.Find(m => m.id == c.TechnicalNameId.ToString()).text;
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
        public async Task<YarnQCReqMaster> GetReceiveData(int receiveId)
        {
            var query =

                $@"
                --master data
                ;WITH M AS (
                    SELECT RM.ReceiveID, RM.ReceiveDate, RM.ReceiveNo, RM.LocationID, RM.RCompanyID, RM.OCompanyID CompanyID, RM.SupplierID, RM.SpinnerID
                    FROM {TableNames.YARN_RECEIVE_MASTER} RM WHERE RM.ReceiveId = {receiveId}
                )

                SELECT	M.ReceiveID, M.ReceiveDate, M.ReceiveNo, M.LocationID, M.RCompanyID, M.CompanyID, M.SupplierID, M.SpinnerID,
                CC.[Name] Supplier, SS.[Name] Spinner, COM.CompanyName RCompany
                FROM M
                Left JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = M.SupplierID
                Left JOIN {DbNames.EPYSL}..Contacts SS ON SS.ContactID = M.SpinnerID
                Left JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID = M.RCompanyID;

                --child data
                ;WITH YRC AS (
	                SELECT RC.ChildID, RC.ReceiveID, RC.ItemMasterID, RC.InvoiceChildID, RC.POChildID, RC.UnitID, RC.InvoiceQty, RC.ChallanQty, RC.ShortQty, RC.ExcessQty,
	                RC.ReceiveQty, RC.Rate, RC.Remarks, RC.LotNo, RC.ChallanLot, RC.NoOfCartoon, RC.NoOfCone, RC.POQty, RC.YarnProgramId, RC.ChallanCount, RC.PhysicalCount
	                FROM {TableNames.YARN_RECEIVE_CHILD} RC WHERE RC.ReceiveID = {receiveId}
                )
                SELECT YRC.ItemMasterID, YRC.UnitID,  YRC.Rate, YRC.LotNo, YRC.ChallanLot, UU.DisplayUnitDesc Uom, YRC.YarnProgramId, YRC.ChallanCount, YRC.PhysicalCount, 
				YRC.PhysicalCount POCount, YRC.ReceiveQty, YRC.ReceiveQty ReqQty, 0 NoOfThread,''YarnCategory, ISV1.SegmentValue Segment1ValueDesc, 
				ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc, 
				ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc,YRC.ShadeCode
                FROM YRC
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YRC.ItemMasterID 
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                Inner Join {DbNames.EPYSL}..Unit AS UU On UU.UnitID = YRC.UnitID;

                 ----QC For List
               -- SELECT CAST(ValueID AS VARCHAR) id, ValueName text 
               -- FROM {DbNames.EPYSL}..EntityTypeValue 
                --WHERE EntityTypeID = {EntityTypeConstants.YARN_QC_REQ_FOR} AND ValueName <> 'Select'
               -- ORDER BY ValueName;
                ";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                YarnQCReqMaster data = await records.ReadFirstOrDefaultAsync<YarnQCReqMaster>();
                Guard.Against.NullObject(data);
                data.YarnQCReqChilds = records.Read<YarnQCReqChild>().ToList();
                //data.QCForList = records.Read<Select2OptionModel>().ToList();
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
        public async Task<YarnQCReqMaster> GetAllAsync(int id)
        {
            var sql = $@"
            ;Select * From {TableNames.YARN_QC_REQ_MASTER} Where QCReqMasterID = {id}

            ;Select * From {TableNames.YARN_QC_REQ_CHILD} Where QCReqMasterID = {id}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YarnQCReqMaster data = await records.ReadFirstOrDefaultAsync<YarnQCReqMaster>();
                Guard.Against.NullObject(data);
                data.YarnQCReqChilds = records.Read<YarnQCReqChild>().ToList();
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
        public async Task<YarnQCReqMaster> GetByReceiveChildIds(string receiveChildIds)
        {
            string sql = $@"
            ;WITH M AS (
	            SELECT RM.ReceiveID, RM.ReceiveDate, RM.ReceiveNo, RM.LocationID, RM.RCompanyID, RM.OCompanyID, RM.SupplierID, RM.LCNo, RM.LCDate, RM.Tolerance,
	            RM.InvoiceNo, RM.InvoiceDate, RM.BLNo, RM.BLDate, RM.BankBranchID, RM.PLNo, RM.PLDate, RM.ChallanNo, RM.ChallanDate, RM.GPNo, RM.GPDate, RM.TransportMode,
	            RM.TransportTypeID, RM.CContractorID, RM.VehicalNo, RM.ShipmentStatus, RM.Remarks, RM.SpinnerID, RM.POID, RM.CIID, RM.LCID, RM.ReceivedById, RM.GPTime, 
	            RM.CurrencyID, RM.PONo, RM.PODate, RM.ACompanyInvoice
	            FROM {TableNames.YARN_RECEIVE_CHILD} YRC
	            INNER JOIN {TableNames.YARN_RECEIVE_MASTER} RM ON RM.ReceiveId = YRC.ReceiveID 
	            WHERE YRC.ChildID IN ({receiveChildIds})
            )
            SELECT	M.ReceiveID, M.ReceiveDate, M.ReceiveNo, M.LocationID, M.RCompanyID, M.OCompanyID, M.SupplierID, M.LCNo, M.LCDate, M.Tolerance,
            M.InvoiceNo, M.InvoiceDate, M.BLNo, M.BLDate, M.BankBranchID, M.PLNo, M.PLDate, M.ChallanNo, M.ChallanDate, M.GPNo, M.GPDate, M.TransportMode,
            M.TransportTypeID, M.CContractorID, M.VehicalNo, M.ShipmentStatus, M.Remarks, M.SpinnerID, M.POID, M.CIID, M.LCID, M.ReceivedById, M.GPTime,
	            CC.[Name] Supplier, SS.[Name] Spinner, BB.BranchName BankBranchName, COM.CompanyName RCompany, M.CurrencyID, M.PONo, M.PODate, M.ACompanyInvoice
            FROM M
            LEFT JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = M.SupplierID
            LEFT JOIN {DbNames.EPYSL}..Contacts SS ON SS.ContactID = M.SpinnerID
            LEFT JOIN {DbNames.EPYSL}..BankBranch BB ON BB.BankBranchID = M.BankBranchID
            LEFT JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID = M.RCompanyID;
                
                        -----childs
            ;With 
            X AS (
	            SELECT ReceiveChildID = RC.ChildID, QCReqChildID = RC.ChildID, RC.ChildID, RC.ReceiveID, RM.ReceiveNo, RC.ItemMasterID, RC.InvoiceChildID, RC.POChildID, RC.UnitID, RC.InvoiceQty, RC.ChallanQty, RC.ShortQty, RC.ExcessQty,
	            RC.ReceiveQty, RC.Rate, RC.Remarks, RC.LotNo, RC.ChallanLot, RC.NoOfCartoon, RC.NoOfCone, RC.POQty, RC.YarnProgramId, RC.ChallanCount, RC.PhysicalCount, RC.ShadeCode,
	            SpinnerID = CASE WHEN RC.SpinnerID > 0 THEN RC.SpinnerID ELSE RM.SpinnerId END, 
				RM.SupplierID, RM.ReceiveDate, RM.POID, RC.YarnCategory
	            FROM {TableNames.YARN_RECEIVE_CHILD} RC
	            INNER JOIN {TableNames.YARN_RECEIVE_MASTER} RM ON RM.ReceiveID = RC.ReceiveID
	            WHERE RC.ChildID IN ({receiveChildIds})
            ),
			PrevReq As(
			    Select YQC.LotNo, YQC.ItemMasterID,HasPrevQCReq = CONVERT(bit, Case When count(*)>0 Then 1 Else 0 End )
			    from X
			    Inner Join {TableNames.YARN_QC_REQ_CHILD} YQC ON YQC.LotNo = X.LotNo AND YQC.ItemMasterID = X.ItemMasterID
			    GROUP BY YQC.LotNo, YQC.ItemMasterID
			),
            POFor AS
            (
	            SELECT X.POChildID, POFor = STRING_AGG(PO.ValueName,','), Poc.YPOMasterID, PoC.BuyerID, BuyerName = C.ShortName
	            FROM X
	            INNER join {TableNames.YarnPOChild} PoC ON Poc.YPOChildID = X.POChildID
	            LEFT JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = PoC.BuyerID AND C.ContactID > 0
	            LEFT JOIN {DbNames.EPYSL}..EntityTypeValue PO ON PO.ValueID = PoC.POForID
	            GROUP BY X.POChildID, Poc.YPOMasterID,C.ShortName,PoC.BuyerID
            )
            SELECT X.ReceiveChildID,X.QCReqChildID, X.ChildID, X.ReceiveID, X.ReceiveNo, X.ItemMasterID, X.InvoiceChildID, X.POChildID, X.UnitID, X.InvoiceQty, X.POQty, X.ChallanQty, X.ShortQty, X.ExcessQty, X.ReceiveQty, 
            X.Rate, X.Remarks, X.LotNo, X.ChallanLot, X.NoOfCartoon, X.NoOfCone, UU.DisplayUnitDesc, X.YarnProgramId, X.ChallanCount, X.PhysicalCount,
            X.SpinnerID, X.SupplierID, X.ReceiveDate,
            SupplierName = CC.[Name], 
			Spinner = CASE WHEN X.SpinnerID > 0 THEN SS.[Name] ELSE '' END,
            ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc, 
            ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc, X.ShadeCode,
            ImportCategory = CASE WHEN ACI.InLand = 1 THEN 'Local' ELSE CASE WHEN ACI.InHouse = 1 THEN 'In house' ELSE 'Foreign' END END,
            POF.POFor, PVR.HasPrevQCReq,
            YarnDetail = X.YarnCategory, --CONCAT(ISV6.SegmentValue,' ', ISV1.SegmentValue,' ', ISV3.SegmentValue,' ', ISV4.SegmentValue,' ', ISV2.SegmentValue,' ', ISV5.SegmentValue,' ', X.ShadeCode),
            POF.BuyerID, POF.BuyerName
            FROM X
            LEFT JOIN POFor POF ON POF.POChildID = X.POChildID AND POF.YPOMasterID = X.POID
			LEFT JOIN PrevReq PVR ON PVR.LotNo = X.LotNo AND PVR.ItemMasterID = X.ItemMasterID
            LEFT JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = X.SupplierID
            LEFT JOIN {DbNames.EPYSL}..Contacts SS ON SS.ContactID = X.SpinnerID
	        LEFT JOIN {DbNames.EPYSL}..ContactAdditionalInfo ACI ON ACI.ContactID = CC.ContactID
            LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = X.ItemMasterID 
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
            LEFT Join {DbNames.EPYSL}..Unit AS UU On UU.UnitID = X.UnitID;

            ;SELECT CAST(ValueID AS VARCHAR) id, ValueName text 
            FROM {DbNames.EPYSL}..EntityTypeValue 
            WHERE EntityTypeID = {EntityTypeConstants.YARN_QC_REQ_FOR} AND ValueName <> 'Select'
            ORDER BY ValueName;

            ;Select Cast(ReceiveID As varchar) [id], ReceiveNo [text], C.ShortName [desc] 
            From {TableNames.YARN_RECEIVE_MASTER} RM
            Inner Join {DbNames.EPYSL}..Contacts C On RM.SupplierID = C.ContactID
            Order By RM.DateAdded Desc;
            
            --Machine Type
            ;SELECT CAST(a.MachineSubClassID AS varchar) [id], b.SubClassName [text], b.TypeID [desc], c.TypeName additionalValue
            FROM {TableNames.KNITTING_MACHINE} a
            INNER JOIN {TableNames.KNITTING_MACHINE_SUBCLASS} b ON b.SubClassID = a.MachineSubClassID
            Inner Join {TableNames.KNITTING_MACHINE_TYPE} c On c.TypeID = b.TypeID
            --Where c.TypeName != 'Flat Bed'
            GROUP BY a.MachineSubClassID, b.SubClassName, b.TypeID, c.TypeName;

             --Technical Name
            SELECT Cast(T.TechnicalNameId As varchar) id, T.TechnicalName [text], ISNULL(ST.[Days], 0) [desc], Cast(SC.SubClassID as varchar) additionalValue
            FROM {TableNames.FabricTechnicalName} T
            LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME_KMACHINE_SUB_CLASS} SC ON SC.TechnicalNameID = T.TechnicalNameId
            LEFT JOIN {TableNames.KNITTING_MACHINE_STRUCTURE_TYPE_HK} ST ON ST.StructureTypeID = SC.StructureTypeID
            Group By T.TechnicalNameId, T.TechnicalName, ST.Days, SC.SubClassID;

            -- Buyers
             {CommonQueries.GetContactsByCategoryType(ContactCategoryNames.BUYER)};";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YarnQCReqMaster data = new YarnQCReqMaster();
                List<YarnQCReqMaster> datas = records.Read<YarnQCReqMaster>().ToList();
                data = datas.First();
                Guard.Against.NullObject(data);
                data.YarnQCReqChilds = records.Read<YarnQCReqChild>().ToList();
                data.QCForList = records.Read<Select2OptionModel>().ToList();
                data.ReceiveList = records.Read<Select2OptionModel>().ToList();
                data.MCTypeForFabricList = records.Read<Select2OptionModel>().ToList();
                data.TechnicalNameList = records.Read<Select2OptionModel>().ToList();
                data.BuyerList = records.Read<Select2OptionModel>().ToList();

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
        public async Task<YarnQCReqMaster> GetRetest(int id)
        {
            string sql =
              $@"
                ;WITH M AS (
                Select NeedUSTER,NeedYarnTRF,NeedFabricTRF,QCReqMasterID, QCReqNo, QCReqBy, QCReqDate, QCForID, IsApprove, ApproveDate, ApproveBy, IsAcknowledge, AcknowledgeDate, AcknowledgeBy, ReceiveID, 
                LocationID, RCompanyID, CompanyID, SupplierID, SpinnerID
                From {TableNames.YARN_QC_REQ_MASTER} Where QCReqMasterID = {id}
                )
                SELECT	NeedUSTER,NeedYarnTRF,NeedFabricTRF,QCReqMasterID, QCReqNo, QCReqBy, QCReqDate, QCForID, IsApprove, M.ApproveDate, M.ApproveBy, IsAcknowledge, AcknowledgeDate, AcknowledgeBy, M.ReceiveID, 
                M.LocationID, M.RCompanyID, M.CompanyID, M.SupplierID, M.SpinnerID, CC.[Name] Supplier, SS.[Name] Spinner, COM.CompanyName RCompany, RM.ReceiveNo, RM.ReceiveDate
                FROM M
				LEFT JOIN {TableNames.YARN_RECEIVE_MASTER} RM ON RM.ReceiveID = M.ReceiveID
                LEFT JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = M.SupplierID
                LEFT JOIN {DbNames.EPYSL}..Contacts SS ON SS.ContactID = M.SpinnerID
                LEFT JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID = M.RCompanyID;

                ;WITH YRC AS (
	                SELECT RC.QCReqChildID, RC.QCReqMasterID, RC.LotNo, RC.ChallanLot, RC.ItemMasterID, RC.ReqQty,  RC.ReqCone,RC.UnitID, RC.Rate,
	                RC.YarnProgramId, RC.ChallanCount, RC.POCount, RC.PhysicalCount, RC.YarnCategory, RC.NoOfThread,RC.ShadeCode, RC.ReceiveNo
	                FROM {TableNames.YARN_QC_REQ_CHILD} RC WHERE RC.QCReqMasterID = {id}
                )
                SELECT YRC.QCReqChildID, YRC.QCReqMasterID, YRC.LotNo, YRC.ChallanLot, YRC.ItemMasterID, YRC.ReqQty, YRC.ReqCone,YRC.ReqQty ReceiveQty,  YRC.UnitID, YRC.Rate, YRC.ReceiveNo,
	                YRC.YarnProgramId, YRC.ChallanCount, YRC.POCount, YRC.PhysicalCount, YRC.YarnCategory, YRC.NoOfThread, UU.DisplayUnitDesc Uom, 
	                ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc, 
					ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc,YRC.ShadeCode
                FROM YRC
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YRC.ItemMasterID 
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                Inner Join {DbNames.EPYSL}..Unit AS UU On UU.UnitID = YRC.UnitID;

                ;SELECT CAST(ValueID AS VARCHAR) id, ValueName text 
                FROM {DbNames.EPYSL}..EntityTypeValue 
                WHERE EntityTypeID = {EntityTypeConstants.YARN_QC_REQ_FOR} AND ValueName <> 'Select'
                ORDER BY ValueName;

                ;Select Cast(ReceiveID As varchar) [id], ReceiveNo [text], C.ShortName [desc] 
                From {TableNames.YARN_RECEIVE_MASTER} RM
                Inner Join {DbNames.EPYSL}..Contacts C On RM.SupplierID = C.ContactID
                Order By RM.DateAdded Desc;

                --Machine Type
                ;SELECT CAST(a.MachineSubClassID AS varchar) [id], b.SubClassName [text], b.TypeID [desc], c.TypeName additionalValue
                FROM {TableNames.KNITTING_MACHINE} a
                INNER JOIN {TableNames.KNITTING_MACHINE_SUBCLASS} b ON b.SubClassID = a.MachineSubClassID
                Inner Join {TableNames.KNITTING_MACHINE_TYPE} c On c.TypeID = b.TypeID
                --Where c.TypeName != 'Flat Bed'
                GROUP BY a.MachineSubClassID, b.SubClassName, b.TypeID, c.TypeName;

                 --Technical Name
                SELECT Cast(T.TechnicalNameId As varchar) id, T.TechnicalName [text], ISNULL(ST.[Days], 0) [desc], Cast(SC.SubClassID as varchar) additionalValue
                FROM {TableNames.FabricTechnicalName} T
                LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME_KMACHINE_SUB_CLASS} SC ON SC.TechnicalNameID = T.TechnicalNameId
                LEFT JOIN {TableNames.KNITTING_MACHINE_STRUCTURE_TYPE_HK} ST ON ST.StructureTypeID = SC.StructureTypeID
                Group By T.TechnicalNameId, T.TechnicalName, ST.Days, SC.SubClassID;

                -- Buyers
                 {CommonQueries.GetContactsByCategoryType(ContactCategoryNames.BUYER)};";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YarnQCReqMaster data = await records.ReadFirstOrDefaultAsync<YarnQCReqMaster>();
                Guard.Against.NullObject(data);
                data.YarnQCReqChilds = records.Read<YarnQCReqChild>().ToList();
                data.QCForList = records.Read<Select2OptionModel>().ToList();
                data.ReceiveList = records.Read<Select2OptionModel>().ToList();
                data.MCTypeForFabricList = records.Read<Select2OptionModel>().ToList();
                data.TechnicalNameList = records.Read<Select2OptionModel>().ToList();
                data.BuyerList = records.Read<Select2OptionModel>().ToList();
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
        public async Task SaveAsync(YarnQCReqMaster entity, YarnQCRemarksMaster entityQCRemarks)
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
                        entity.QCReqMasterID = await _service.GetMaxIdAsync(TableNames.YARN_QC_REQ_MASTER, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        entity.QCReqNo = await _service.GetMaxNoAsync(TableNames.YARN_QC_REQ_NO, 1, RepeatAfterEnum.NoRepeat, "00000", transactionGmt, _connectionGmt);

                        maxChildId = await _service.GetMaxIdAsync(TableNames.YARN_QC_REQ_CHILD, entity.YarnQCReqChilds.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        foreach (YarnQCReqChild item in entity.YarnQCReqChilds)
                        {
                            item.QCReqChildID = maxChildId++;
                            item.QCReqMasterID = entity.QCReqMasterID;
                        }
                        break;

                    case EntityState.Modified:
                        var addedChilds = entity.YarnQCReqChilds.FindAll(x => x.EntityState == EntityState.Added);
                        maxChildId = await _service.GetMaxIdAsync(TableNames.YARN_QC_REQ_CHILD, addedChilds.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

                        foreach (YarnQCReqChild item in addedChilds)
                        {
                            item.QCReqChildID = maxChildId++;
                            item.QCReqMasterID = entity.QCReqMasterID;
                        }
                        break;

                    case EntityState.Unchanged:
                    case EntityState.Deleted:
                        entity.EntityState = EntityState.Deleted;
                        entity.YarnQCReqChilds.SetDeleted();
                        break;

                    default:
                        break;
                }
                await _service.SaveSingleAsync(entity, transaction);
                await _service.SaveAsync(entity.YarnQCReqChilds, transaction);

                if (entityQCRemarks.QCRemarksMasterID > 0)
                {
                    await _service.SaveSingleAsync(entityQCRemarks, transaction);
                }

                if (entity.IsFromNoTest)
                {
                    await _service.SaveAsync(entity.YarnReceiveChilds, transaction);
                }
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
