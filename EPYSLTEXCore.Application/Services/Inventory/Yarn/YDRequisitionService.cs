using Dapper;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Application.Interfaces.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Booking;
using EPYSLTEXCore.Infrastructure.Entities.Tex.General.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Yarn;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using System.Data;
using System.Data.Entity;
using Microsoft.Data.SqlClient;
using System.Transactions;

namespace EPYSLTEXCore.Application.Services.Inventory
{
    public class YDRequisitionService : IYDRequisitionService
    {
        private readonly IDapperCRUDService<YDReqMaster> _service;
        private readonly SqlConnection _connection;
        private readonly SqlConnection _connectionGmt;

        public YDRequisitionService(IDapperCRUDService<YDReqMaster> service)
        {
            _service = service;
            _service.Connection = service.GetConnection(AppConstants.GMT_CONNECTION);
            _connectionGmt = service.Connection;

            _service.Connection = service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _connection = service.Connection;
        }
        public async Task<List<YDReqMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By YDReqMasterID Desc" : paginationInfo.OrderBy;

            string sql;
            if (status == Status.Pending)
            {
                sql = $@";With 
                    BM As (
	                    Select YBM.YDBookingMasterID, YBM.YDBookingNo, YBM.YDBookingBy, YBM.YDBookingDate, YBM.RefBatchNo, YBM.SampleRefNo, YBM.SwatchFilePath,ConceptNo = YBM.GroupConceptNo,BM.YBookingNo
                        , YBM.PreviewTemplate, YBM.BuyerID, YBM.Remarks, YBM.IsApprove,YBM.IsAcknowledge,YBM.ConceptID,YBM.YBookingID,
					    (CASE WHEN YBM.BuyerID=0 THEN 'R&D' ELSE C.ShortName END) AS BuyerName,SUM(YC.BookingQty) ReqQty,RQ.RequestedQty
						,BookingByUser = LU.UserName --, FCM.IsBDS
                        FROM {TableNames.YD_BOOKING_MASTER} YBM
					    Inner JOIN {TableNames.YDBookingChild} YC On YBM.YDBookingMasterID = YC.YDBookingMasterID
					    --LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID=YBM.ConceptID
						LEFT JOIN {TableNames.YarnBookingMaster_New} BM ON YBM.YBookingID = BM.YBookingID
						LEFT JOIN {TableNames.YD_REQ_MASTER} YRM On YRM.YDBookingMasterID = YBM.YDBookingMasterID
						LEFT JOIN {DbNames.EPYSL}..Contacts C ON YBM.BuyerID = C.ContactID
						INNER JOIN {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = YBM.YDBookingBy
						CROSS APPLY(
						Select isnull(sum(CYRC.ReqQty),0)RequestedQty FROM {TableNames.YD_REQ_MASTER} CYRM
						INNER JOIN {TableNames.YD_REQ_CHILD} CYRC ON CYRC.YDReqMasterID=CYRM.YDReqMasterID
						where YDBookingMasterID=YBM.YDBookingMasterID
						)RQ
                        Where YBM.SendForApproval = 1 And YBM.IsApprove = 1 And YBM.IsAcknowledge = 1 AND YBM.IsYDBNoGenerated = 1--AND YRM.YDReqMasterID is null --And YBM.YDBookingMasterID Not In (Select YDBookingMasterID FROM {TableNames.YD_REQ_MASTER})
						Group By YBM.YDBookingMasterID, YDBookingNo, YBM.YDBookingBy, YDBookingDate, RefBatchNo, SampleRefNo,YBM.SwatchFilePath,YBM.GroupConceptNo,BM.YBookingNo
						, YBM.PreviewTemplate,YBM.BuyerID, YBM.Remarks, YBM.IsApprove,YBM.IsAcknowledge,YBM.ConceptID,YBM.YBookingID,C.ShortName,RQ.RequestedQty, LU.UserName --, FCM.IsBDS
                    ),
					BDSType AS
					(
						SELECT FCM.GroupConceptNo, FCM.IsBDS
						FROM BM
						INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.GroupConceptNo = BM.ConceptNo
						GROUP BY FCM.GroupConceptNo, FCM.IsBDS
					),
                    FinalList AS
                    (
                         SELECT YDBookingMasterID,YDBookingNo, Remarks, IsAcknowledge, YDBookingBy,BuyerID,ConceptNo,YBookingNo
	                    , YDBookingDate, RefBatchNo, SampleRefNo, SwatchFilePath, PreviewTemplate,ConceptID,YBookingID,BuyerName,ReqQty,RequestedQty,B.IsBDS, BookingByUser
                        FROM BM 
						LEFT JOIN BDSType B ON B.GroupConceptNo = BM.ConceptNo
						WHERE RequestedQty < ReqQty 
                    )
                    SELECT *,Count(*) Over() TotalRows FROM FinalList  ";

                orderBy = " ORDER BY YDBookingMasterID DESC";
            }
            else if (status == Status.PendingConfirmation)
            {
                sql = $@" ;With
                    M As (
	                    Select M.YDReqMasterID, YDReqNo, LU.Name ReqByUser, YDReqDate, BM.YDBookingNo,BU.Name BookingByUser, 
		                BuyerName = CASE WHEN ISNULL(YDB.BuyerID,0) > 0 THEN CT.ShortName 
										 ELSE Case When FM.IsBDS = 0 Then 'R&D' Else '' End 
										 END,
		                SUM(C.ReqQty) ReqQty,
		                ConceptNo = CASE WHEN ISNULL(FM.ConceptNo,'') = '' THEN YDB.GroupConceptNo ELSE FM.ConceptNo END,
		                BM.YDBookingDate 
	                    FROM {TableNames.YD_REQ_MASTER} M
	                    Inner JOIN {TableNames.YD_REQ_CHILD} C On M.YDReqMasterID = C.YDReqMasterID
		                LEFT JOIN {TableNames.YD_BOOKING_MASTER} YDB ON YDB.YDBookingMasterID = M.YDBookingMasterID
		                Inner Join {DbNames.EPYSL}..LoginUser LU On M.YDReqBy = LU.UserCode
	                    Inner JOIN {TableNames.YD_BOOKING_MASTER} BM On M.YDBookingMasterID = BM.YDBookingMasterID
                        Left Join {DbNames.EPYSL}..LoginUser BU On BM.YDBookingBy = BU.UserCode
		                Left JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FM ON FM.ConceptID = BM.ConceptID
	                    Left Join {DbNames.EPYSL}..Contacts CT On CT.ContactID = YDB.BuyerID
                        WHERE M.IsApprove = 0 AND M.IsReject = 0 AND M.IsAcknowledge = 0
	                    Group By M.YDReqMasterID, YDReqNo, LU.Name, YDReqDate, BM.YDBookingNo,BU.Name, FM.IsBDS, CT.ShortName,
		                FM.ConceptNo,BM.YDBookingDate,YDB.GroupConceptNo,ISNULL(YDB.BuyerID,0)
                    ),
                    FinalList AS
                    (
                        Select YDReqMasterID, YDReqNo, ReqByUser, YDReqDate, YDBookingNo, BuyerName, ReqQty,ConceptNo,YDBookingDate,BookingByUser
                        From M 
                    )
                    SELECT *, Count(*) Over() TotalRows FROM FinalList 
                    ";
            }
            else if (status == Status.Approved)
            {
                sql = $@" ;With
                    M As (
	                    Select M.YDReqMasterID, YDReqNo, LU.Name ReqByUser, YDReqDate, BM.YDBookingNo,BU.Name BookingByUser, 
		                BuyerName = CASE WHEN ISNULL(M.BuyerID,0) > 0 THEN CT.ShortName ELSE '' END,
		                SUM(C.ReqQty) ReqQty,
		                ConceptNo = CASE WHEN ISNULL(FM.ConceptNo,'') = '' THEN YDB.GroupConceptNo ELSE FM.ConceptNo END,
		                BM.YDBookingDate 
	                    FROM {TableNames.YD_REQ_MASTER} M
	                    Inner JOIN {TableNames.YD_REQ_CHILD} C On M.YDReqMasterID = C.YDReqMasterID
		                LEFT JOIN {TableNames.YD_BOOKING_MASTER} YDB ON YDB.YDBookingMasterID = M.YDBookingMasterID
		                Inner Join {DbNames.EPYSL}..LoginUser LU On M.YDReqBy = LU.UserCode
	                    Inner JOIN {TableNames.YD_BOOKING_MASTER} BM On M.YDBookingMasterID = BM.YDBookingMasterID
                        Left Join {DbNames.EPYSL}..LoginUser BU On BM.YDBookingBy = BU.UserCode
		                Left JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FM ON FM.ConceptID = BM.ConceptID
	                    Left Join {DbNames.EPYSL}..Contacts CT On M.BuyerID = CT.ContactID
                        WHERE M.IsApprove = 1 AND M.IsReject = 0 --AND M.IsAcknowledge = 0
	                    Group By M.YDReqMasterID, YDReqNo, LU.Name, YDReqDate, BM.YDBookingNo,BU.Name, CT.ShortName,
		                FM.ConceptNo,BM.YDBookingDate,YDB.GroupConceptNo,ISNULL(M.BuyerID,0)
                    ),
                    FinalList AS
                    (
                        Select YDReqMasterID, YDReqNo, ReqByUser, YDReqDate, YDBookingNo, BuyerName, ReqQty,ConceptNo,YDBookingDate,BookingByUser
                        From M 
                    )
                    SELECT *, Count(*) Over() TotalRows FROM FinalList ";
            }
            else
            {
                sql = $@" ;With
                    M As (
	                    Select M.YDReqMasterID, YDReqNo, LU.Name ReqByUser, YDReqDate, BM.YDBookingNo,BU.Name BookingByUser, CT.ShortName BuyerName, SUM(C.ReqQty) ReqQty,FM.ConceptNo,BM.YDBookingDate 
	                    FROM {TableNames.YD_REQ_MASTER} M
						Inner Join {DbNames.EPYSL}..LoginUser LU On M.YDReqBy = LU.UserCode
	                    Inner JOIN {TableNames.YD_REQ_CHILD} C On M.YDReqMasterID = C.YDReqMasterID
	                    Inner JOIN {TableNames.YD_BOOKING_MASTER} BM On M.YDBookingMasterID = BM.YDBookingMasterID
                        Left Join {DbNames.EPYSL}..LoginUser BU On BM.YDBookingBy = BU.UserCode
						Left JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FM ON FM.ConceptID = BM.ConceptID
	                    Left Join {DbNames.EPYSL}..Contacts CT On M.BuyerID = CT.ContactID
	                    Group By M.YDReqMasterID, YDReqNo, LU.Name,  YDReqDate, BM.YDBookingNo,BU.Name, CT.ShortName,FM.ConceptNo,BM.YDBookingDate
                    ),
                    FinalList AS
                    (
                        Select YDReqMasterID, YDReqNo, ReqByUser, YDReqDate, YDBookingNo, BuyerName, ReqQty,ConceptNo,YDBookingDate,BookingByUser
                        From M 
                    )
                    SELECT *, Count(*) Over() TotalRows FROM FinalList ";
            }
            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<YDReqMaster>(sql);
        }
        public async Task<YDReqMaster> GetNewAsync(int ydBookingMasterId, int isBDS)
        {
            var query = "";
            if (isBDS == EnumBDSType.Bulk)
            {
                query =
                $@"
                  -- Master Data
                Select YDBookingMasterID, YDBookingDate, YDBookingNo, M.BuyerID, CT.ShortName BuyerName,ConceptNo = M.GroupConceptNo,
				CompanyId = CASE WHEN ISNULL(BM.BookingNo,'') <> '' THEN MAX(BM.CompanyID) ELSE MAX(SBM.ExecutionCompanyID) END,
                CM.IsBDS, CM.ConceptID
                FROM {TableNames.YD_BOOKING_MASTER} M
                Inner Join {DbNames.EPYSL}..Contacts CT On CT.ContactID = M.BuyerID
                LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} CM ON CM.GroupConceptNo = M.GroupConceptNo
				LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster SBM ON SBM.BookingID=CM.BookingID 
				LEFT JOIN {DbNames.EPYSL}..BookingMaster BM ON BM.BookingID=CM.BookingID 
                LEFT JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID = BM.CompanyID
				LEFT JOIN {DbNames.EPYSL}..CompanyEntity SCOM ON SCOM.CompanyID = SBM.ExecutionCompanyID
                Where YDBookingMasterID = {ydBookingMasterId}
				GROUP BY YDBookingMasterID, YDBookingDate, YDBookingNo, M.BuyerID, CT.ShortName,M.GroupConceptNo,ISNULL(BM.BookingNo,''),CM.IsBDS, CM.ConceptID;

                
                -- Childs Data
                With 
                YDBC As(
	                Select YDBookingChildID = MIN(YDBC.YDBookingChildID),
					YDBC.YBChildItemID, YDBC.YDBookingMasterID, YDBC.ItemMasterID, YBCI.YItemMasterID, YDBC.Remarks, YDBC.YarnCategory, YDBC.NoOfThread, YarnDyedColorID, YDBC.ShadeCode 
                            ,YDBC.UnitID, BookingQty=YBCI.NetYarnReqQty, YDBC.YarnProgramID,YDBC.LotNo,YDBC.PhysicalCount,YDBC.SpinnerID 
	                        ,ReqQty=SUM(YDRC.ReqQty), YDM.GroupConceptNo, YDBC.AllocationChildItemID, YBM.BookingID
					FROM {TableNames.YDBookingChild} YDBC 
					INNER JOIN {TableNames.YD_BOOKING_MASTER} YDM ON YDM.YDBookingMasterID = YDBC.YDBookingMasterID
					INNER JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBCI.YBChildItemID = YDBC.YBChildItemID
					INNER JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID = YBCI.YBChildID
					INNER JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID = YBC.YBookingID
					LEFT JOIN {TableNames.YD_REQ_CHILD} YDRC ON YDRC.YDBookingChildID =YDBC.YDBookingChildID
	                Where YBCI.YD = 1 AND YDBC.YDBookingMasterID = {ydBookingMasterId}
					GROUP BY --YDBC.YDBookingChildID,
					YDBC.YBChildItemID, YDBC.YDBookingMasterID, YDBC.ItemMasterID, YBCI.YItemMasterID, YDBC.Remarks, YDBC.YarnCategory, YDBC.NoOfThread, YarnDyedColorID, YDBC.ShadeCode 
                    ,YDBC.UnitID, YBCI.NetYarnReqQty, YDBC.YarnProgramID,YDBC.LotNo,YDBC.PhysicalCount,YDBC.SpinnerID , YDM.GroupConceptNo, YDBC.AllocationChildItemID, YBM.BookingID
	                
                ),
                Allocated AS
                (
	                SELECT YBCI.YBChildItemID, YACI.TotalAllocationQty,YACI.ItemMasterID, YAC.AllocationChildID,
					BookingNo = Case When BM.BookingID Is Not Null Then BM.BookingNo Else SBM.BookingNo End, YACI.ShadeCode,
					YACI.AllocationChildItemID, BookingQty = YBCI.NetYarnReqQty , YBCI.NetYarnReqQty , YBCI.YItemMasterID, YACI.PhysicalCount, YACI.YarnLotNo, YACI.SpinnerId, YACI.YarnCategory
	                FROM YDBC
	                --INNER JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.YBChildItemID = YDBC.YBChildItemID
					--INNER JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.ItemMasterID = YDBC.ItemMasterID
					INNER JOIN {TableNames.YarnBookingMaster_New}  YBM ON YBM.BookingID = YDBC.BookingID
					INNER JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBookingID = YBM.YBookingID
					INNER JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBCI.YBChildID = YBC.YBChildID
					INNER JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.YBChildItemID = YBCI.YBChildItemID
					INNER JOIN {TableNames.YARN_ALLOCATION_CHILD_ITEM} YACI ON YACI.AllocationChildID = YAC.AllocationChildID
					--INNER JOIN {TableNames.YarnStockSet} YSS ON YSS.YarnStockSetId = YACI.YarnStockSetId
					LEFT JOIN {DbNames.EPYSL}..BookingMaster BM ON BM.BookingID = YBM.BookingID
					LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster SBM ON SBM.BookingID = YBM.BookingID
	                WHERE YBCI.YD = 1 AND YACI.Acknowledge = 1 AND YBCI.YItemMasterId in(Select YItemMasterID FROM YDBC)--AND YDBC.GroupConceptNo = Case When BM.BookingID Is Not Null Then BM.BookingNo Else SBM.BookingNo End 
	                GROUP BY YBCI.YBChildItemID, YACI.ItemMasterID, YAC.AllocationChildID,Case When BM.BookingID Is Not Null Then BM.BookingNo Else SBM.BookingNo End, YACI.ShadeCode, 
							 YACI.AllocationChildItemID, YBC.BookingQty, YACI.TotalAllocationQty, YBCI.NetYarnReqQty, YBCI.YItemMasterID, YACI.PhysicalCount, YACI.YarnLotNo, YACI.SpinnerId, YACI.YarnCategory
                ),--Select * FROM Allocated,
                MainList AS (
	                Select YAC.YBChildItemID, YDBC.YDBookingChildID, 
					YDBC.YDBookingMasterID, YDBC.YarnProgramID, IM.ItemMasterID, YDBC.UnitID, 'Kg' AS DisplayUnitDesc,
	                        YAC.BookingQty, YAC.NetYarnReqQty, YAC.NetYarnReqQty-ISNULL(YDBC.ReqQty,0) ReqQty, YDBC.Remarks, 
							YarnCategory = CASE WHEN ISNULL(YAC.AllocationChildID,0) > 0 THEN YAC.YarnCategory ELSE YDBC.YarnCategory END, 
							YDBC.NoOfThread, ISV1.SegmentValue AS Segment1ValueDesc, ISV2.SegmentValue AS Segment2ValueDesc, ISV3.SegmentValue AS Segment3ValueDesc,
	                        ISV4.SegmentValue AS Segment4ValueDesc, ISV5.SegmentValue AS Segment5ValueDesc, ISV6.SegmentValue AS Segment6ValueDesc,
	                        ISV7.SegmentValue AS Segment7ValueDesc, 
							ShadeCode = CASE WHEN ISNULL(YAC.AllocationChildID,0) > 0 THEN YAC.ShadeCode ELSE YDBC.ShadeCode END,
					        AllocatedQty = SUM(ISNULL(YAC.TotalAllocationQty,0)),
					        YAC.NetYarnReqQty-ISNULL(YDBC.ReqQty,0) AS PendingQty,
							PhysicalCount = CASE WHEN ISNULL(YAC.AllocationChildID,0) > 0 THEN YAC.PhysicalCount ELSE YDBC.PhysicalCount END, 
							LotNo = CASE WHEN ISNULL(YAC.AllocationChildID,0) > 0 THEN YAC.YarnLotNo ELSE YDBC.LotNo END, 
							SpinnerID = CASE WHEN ISNULL(YAC.SpinnerID,0) > 0 THEN YAC.SpinnerID ELSE YDBC.SpinnerID END, 
							YAC.AllocationChildID, YAC.AllocationChildItemID
	                From YDBC
	                LEFT JOIN Allocated YAC ON YAC.YItemMasterID = YDBC.YItemMasterID AND YAC.BookingNo = YDBC.GroupConceptNo AND YAC.YBChildItemID = YDBC.YBChildItemID
					AND YAC.AllocationChildItemID = Case When YDBC.AllocationChildItemID>0 Then YDBC.AllocationChildItemID Else YAC.AllocationChildItemID END
	                Left Join {DbNames.EPYSL}..EntityTypeValue EV On YDBC.YarnProgramID = EV.ValueID
	                Left Join {DbNames.EPYSL}..Unit U ON YDBC.UnitID = U.UnitID
	                Left Join {DbNames.EPYSL}..ItemMaster IM On  IM.ItemMasterID = YAC.ItemMasterID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
	                GROUP BY YDBC.YDBookingChildID, 
					YDBC.YDBookingMasterID, YDBC.YarnProgramID, IM.ItemMasterID, YDBC.UnitID,
	                YDBC.BookingQty, YAC.BookingQty, YAC.NetYarnReqQty, YDBC.Remarks, CASE WHEN ISNULL(YAC.AllocationChildID,0) > 0 THEN YAC.YarnCategory ELSE YDBC.YarnCategory END,
					YDBC.NoOfThread, ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue,
	                ISV4.SegmentValue, ISV5.SegmentValue, ISV6.SegmentValue,
	                ISV7.SegmentValue,CASE WHEN ISNULL(YAC.AllocationChildID,0) > 0 THEN YAC.ShadeCode ELSE YDBC.ShadeCode END,YDBC.BookingQty,YAC.YBChildItemID,YDBC.ReqQty,
					CASE WHEN ISNULL(YAC.AllocationChildID,0) > 0 THEN YAC.PhysicalCount ELSE YDBC.PhysicalCount END, 
					CASE WHEN ISNULL(YAC.AllocationChildID,0) > 0 THEN YAC.YarnLotNo ELSE YDBC.LotNo END, 
					CASE WHEN ISNULL(YAC.SpinnerID,0) > 0 THEN YAC.SpinnerID ELSE YDBC.SpinnerID END, 
					YAC.AllocationChildID, YAC.AllocationChildItemID
                ),--Select * FROM MainList,
     --           YSS AS
     --           (
	    --            SELECT  MainList.YDBookingChildID,CI.AllocationChildItemID, YAC.YBChildItemID, MainList.AllocationChildID,
     --                       PhysicalCount = MAX(YSS.PhysicalCount), 
					--        YarnLotNo = MAX(YSS.YarnLotNo), 
					--        SpinnerId = MAX(YSS.SpinnerId)
	    --            FROM MainList
					--Left JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.ItemMasterID = MainList.ItemMasterID AND YAC.AllocationChildID = MainList.AllocationChildID
	    --            Left JOIN {TableNames.YARN_ALLOCATION_CHILD_ITEM} CI ON CI.AllocationChildID = YAC.AllocationChildID
	    --            Left JOIN {TableNames.YarnStockChild} YSC ON YSC.StockFromTableId = 6 AND YSC.StockFromPKId = CI.AllocationChildItemID
	    --            Left JOIN {TableNames.YarnStockSet} YSS ON YSS.YarnStockSetId = YSC.YarnStockSetId
	    --            GROUP BY MainList.YDBookingChildID,CI.AllocationChildItemID, YSS.PhysicalCount, YSS.YarnLotNo, YSS.SpinnerId, YAC.YBChildItemID, MainList.AllocationChildID
     --           ),
                FinalList AS
                (
	                SELECT ML.YBChildItemID, ML.YDBookingChildID, ML.YDBookingMasterID, ML.YarnProgramID, ML.ItemMasterID, ML.UnitID,
	                        ML.BookingQty, ML.NetYarnReqQty, ML.ReqQty, ML.Remarks, ML.YarnCategory, ML.NoOfThread, 
	                        ML.Segment1ValueDesc, ML.Segment2ValueDesc, ML.Segment3ValueDesc,
	                        ML.Segment4ValueDesc, ML.Segment5ValueDesc, ML.Segment6ValueDesc,
	                        ML.Segment7ValueDesc,ML.ShadeCode,
					        ML.AllocatedQty,
					        PendingQty = isnull(ML.PendingQty,0),
							ML.PhysicalCount, --= CASE WHEN ISNULL(ML.YDBookingChildID,0) > 0 THEN YSS.PhysicalCount ELSE ML.PhysicalCount END,
	                        ML.LotNo,-- = CASE WHEN ISNULL(YSS.YDBookingChildID,0) > 0 THEN YSS.YarnLotNo ELSE ML.LotNo END,
	                        SpinnerName = S1.ShortName,--CASE WHEN ISNULL(ML.SpinnerID,0) > 0 THEN S2.ShortName ELSE S1.ShortName END,
                            ML.SpinnerID,-- = CASE WHEN ISNULL(YSS.SpinnerID,0) > 0 THEN YSS.SpinnerID ELSE ML.SpinnerID END,
							ML.AllocationChildItemID
	                FROM MainList ML
                    --LEFT JOIN YSS ON YSS.AllocationChildItemID = ML.AllocationChildItemID
	                LEFT Join {DbNames.EPYSL}..Contacts S1 On S1.ContactID = ML.SpinnerID 
	                --LEFT Join {DbNames.EPYSL}..Contacts S2 On S2.ContactID = ML.SpinnerID 
	                --LEFT JOIN YDBC ON YDBC.YBChildItemID = ML.YBChildItemID
	                --LEFT JOIN YSS ON YSS.YDBookingChildID = ML.YDBookingChildID AND YSS.YBChildItemID = ML.YBChildItemID 
					--			 AND YSS.AllocationChildID = ML.AllocationChildID AND YSS.AllocationChildItemID = ML.AllocationChildItemID

                )
                SELECT * FROM FinalList;

                /*-- Childs Data
                With 
                YDBC As(
	                Select YDBookingChildID = MIN(YDBC.YDBookingChildID),
					YDBC.YBChildItemID, YDBC.YDBookingMasterID, YDBC.ItemMasterID, YDBC.Remarks, YDBC.YarnCategory, YDBC.NoOfThread, YarnDyedColorID, YDBC.ShadeCode 
                            ,YDBC.UnitID, BookingQty=YBCI.NetYarnReqQty, YDBC.YarnProgramID,YDBC.LotNo,YDBC.PhysicalCount,YDBC.SpinnerID 
	                        ,ReqQty=SUM(YDRC.ReqQty), YDM.GroupConceptNo, YDBC.AllocationChildItemID, YBM.BookingID
					FROM {TableNames.YDBookingChild} YDBC 
					INNER JOIN {TableNames.YD_BOOKING_MASTER} YDM ON YDM.YDBookingMasterID = YDBC.YDBookingMasterID
					INNER JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBCI.YBChildItemID = YDBC.YBChildItemID
					INNER JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID = YBCI.YBChildID
					INNER JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID = YBC.YBookingID
					LEFT JOIN {TableNames.YD_REQ_CHILD} YDRC ON YDRC.YDBookingChildID =YDBC.YDBookingChildID
	                Where YBCI.YD = 1 AND YDBC.YDBookingMasterID = {ydBookingMasterId}
					GROUP BY --YDBC.YDBookingChildID,
					YDBC.YBChildItemID, YDBC.YDBookingMasterID, YDBC.ItemMasterID, YDBC.Remarks, YDBC.YarnCategory, YDBC.NoOfThread, YarnDyedColorID, YDBC.ShadeCode 
                    ,YDBC.UnitID, YBCI.NetYarnReqQty, YDBC.YarnProgramID,YDBC.LotNo,YDBC.PhysicalCount,YDBC.SpinnerID , YDM.GroupConceptNo, YDBC.AllocationChildItemID, YBM.BookingID
	                
                ),
                Allocated AS
                (
	                SELECT YBCI.YBChildItemID, YACI.TotalAllocationQty,YSS.ItemMasterID, YAC.AllocationChildID,
					BookingNo = Case When BM.BookingID Is Not Null Then BM.BookingNo Else SBM.BookingNo End, 
					YACI.AllocationChildItemID, BookingQty = YBCI.NetYarnReqQty , YBCI.NetYarnReqQty 
	                FROM YDBC
	                --INNER JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.YBChildItemID = YDBC.YBChildItemID
					--INNER JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.ItemMasterID = YDBC.ItemMasterID
					INNER JOIN {TableNames.YarnBookingMaster_New}  YBM ON YBM.BookingID = YDBC.BookingID
					INNER JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBookingID = YBM.YBookingID
					INNER JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBCI.YBChildID = YBC.YBChildID
					INNER JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.YBChildItemID = YBCI.YBChildItemID
					INNER JOIN {TableNames.YARN_ALLOCATION_CHILD_ITEM} YACI ON YACI.AllocationChildID = YAC.AllocationChildID
					INNER JOIN {TableNames.YarnStockSet} YSS ON YSS.YarnStockSetId = YACI.YarnStockSetId --AND YSS.ItemMasterId = YDBC.ItemMasterID
					LEFT JOIN {DbNames.EPYSL}..BookingMaster BM ON BM.BookingID = YBM.BookingID
					LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster SBM ON SBM.BookingID = YBM.BookingID
	                WHERE YBCI.YD =1 AND YACI.Acknowledge = 1 AND YSS.ItemMasterId in(Select ItemMasterID FROM YDBC)--AND YDBC.GroupConceptNo = Case When BM.BookingID Is Not Null Then BM.BookingNo Else SBM.BookingNo End 
	                GROUP BY YBCI.YBChildItemID, YSS.ItemMasterID, YAC.AllocationChildID,Case When BM.BookingID Is Not Null Then BM.BookingNo Else SBM.BookingNo End, YACI.AllocationChildItemID, YBC.BookingQty, YACI.TotalAllocationQty, YBCI.NetYarnReqQty
                ),
                MainList AS (
	                Select YAC.YBChildItemID, YDBC.YDBookingChildID, 
					YDBC.YDBookingMasterID, YDBC.YarnProgramID, IM.ItemMasterID, YDBC.UnitID, 'Kg' AS DisplayUnitDesc,
	                        YAC.BookingQty, YAC.NetYarnReqQty, YAC.NetYarnReqQty-ISNULL(YDBC.ReqQty,0) ReqQty, YDBC.Remarks, YDBC.YarnCategory, YDBC.NoOfThread, 
	
	                        ISV1.SegmentValue AS Segment1ValueDesc, ISV2.SegmentValue AS Segment2ValueDesc, ISV3.SegmentValue AS Segment3ValueDesc,
	                        ISV4.SegmentValue AS Segment4ValueDesc, ISV5.SegmentValue AS Segment5ValueDesc, ISV6.SegmentValue AS Segment6ValueDesc,
	                        ISV7.SegmentValue AS Segment7ValueDesc,YDBC.ShadeCode,
					        AllocatedQty = SUM(ISNULL(YAC.TotalAllocationQty,0)),
					        YAC.NetYarnReqQty-ISNULL(YDBC.ReqQty,0) AS PendingQty,
							YDBC.PhysicalCount, YDBC.LotNo, YDBC.SpinnerID, YAC.AllocationChildID, YAC.AllocationChildItemID
	                From YDBC
	                LEFT JOIN Allocated YAC ON YAC.ItemMasterID = YDBC.ItemMasterID AND YAC.BookingNo = YDBC.GroupConceptNo 
					AND YAC.AllocationChildItemID = Case When YDBC.AllocationChildItemID>0 Then YDBC.AllocationChildItemID Else YAC.AllocationChildItemID END
	                Left Join {DbNames.EPYSL}..EntityTypeValue EV On YDBC.YarnProgramID = EV.ValueID
	                Left Join {DbNames.EPYSL}..Unit U ON YDBC.UnitID = U.UnitID
	                Left Join {DbNames.EPYSL}..ItemMaster IM On YDBC.ItemMasterID = IM.ItemMasterID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
	                GROUP BY YDBC.YDBookingChildID, 
					YDBC.YDBookingMasterID, YDBC.YarnProgramID, IM.ItemMasterID, YDBC.UnitID,
	                YDBC.BookingQty, YAC.BookingQty, YAC.NetYarnReqQty, YDBC.Remarks, YDBC.YarnCategory, YDBC.NoOfThread,
	                ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue,
	                ISV4.SegmentValue, ISV5.SegmentValue, ISV6.SegmentValue,
	                ISV7.SegmentValue,YDBC.ShadeCode,YDBC.BookingQty,YAC.YBChildItemID,YDBC.ReqQty,YDBC.PhysicalCount, YDBC.LotNo, YDBC.SpinnerID, YAC.AllocationChildID,
					YAC.AllocationChildItemID
                ),
                YSS AS
                (
	                SELECT  MainList.YDBookingChildID,CI.AllocationChildItemID, YAC.YBChildItemID, MainList.AllocationChildID,
                            PhysicalCount = MAX(YSS.PhysicalCount), 
					        YarnLotNo = MAX(YSS.YarnLotNo), 
					        SpinnerId = MAX(YSS.SpinnerId)
	                FROM MainList
					Left JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.ItemMasterID = MainList.ItemMasterID AND YAC.AllocationChildID = MainList.AllocationChildID
	                Left JOIN {TableNames.YARN_ALLOCATION_CHILD_ITEM} CI ON CI.AllocationChildID = YAC.AllocationChildID
	                Left JOIN {TableNames.YarnStockChild} YSC ON YSC.StockFromTableId = 6 AND YSC.StockFromPKId = CI.AllocationChildItemID
	                Left JOIN {TableNames.YarnStockSet} YSS ON YSS.YarnStockSetId = YSC.YarnStockSetId
	                GROUP BY MainList.YDBookingChildID,CI.AllocationChildItemID, YSS.PhysicalCount, YSS.YarnLotNo, YSS.SpinnerId, YAC.YBChildItemID, MainList.AllocationChildID
                ),
                FinalList AS
                (
	                SELECT ML.YBChildItemID, ML.YDBookingChildID, ML.YDBookingMasterID, ML.YarnProgramID, ML.ItemMasterID, ML.UnitID,
	                        ML.BookingQty, ML.NetYarnReqQty, ML.ReqQty, ML.Remarks, ML.YarnCategory, ML.NoOfThread, 
	                        ML.Segment1ValueDesc, ML.Segment2ValueDesc, ML.Segment3ValueDesc,
	                        ML.Segment4ValueDesc, ML.Segment5ValueDesc, ML.Segment6ValueDesc,
	                        ML.Segment7ValueDesc,ML.ShadeCode,
					        ML.AllocatedQty,
					        PendingQty = isnull(ML.PendingQty,0),
							PhysicalCount = CASE WHEN ISNULL(YSS.YDBookingChildID,0) > 0 THEN YSS.PhysicalCount ELSE ML.PhysicalCount END,
	                        LotNo = CASE WHEN ISNULL(YSS.YDBookingChildID,0) > 0 THEN YSS.YarnLotNo ELSE ML.LotNo END,
	                        SpinnerName = CASE WHEN ISNULL(YSS.SpinnerID,0) > 0 THEN S2.ShortName ELSE S1.ShortName END,
                            SpinnerID = CASE WHEN ISNULL(YSS.SpinnerID,0) > 0 THEN YSS.SpinnerID ELSE ML.SpinnerID END,
							ML.AllocationChildItemID
	                FROM MainList ML
                    LEFT JOIN YSS ON YSS.AllocationChildItemID = ML.AllocationChildItemID
	                LEFT Join {DbNames.EPYSL}..Contacts S1 On S1.ContactID = ML.SpinnerID 
	                LEFT Join {DbNames.EPYSL}..Contacts S2 On S2.ContactID = YSS.SpinnerID 
	                --LEFT JOIN YDBC ON YDBC.YBChildItemID = ML.YBChildItemID
	                --LEFT JOIN YSS ON YSS.YDBookingChildID = ML.YDBookingChildID AND YSS.YBChildItemID = ML.YBChildItemID 
					--			 AND YSS.AllocationChildID = ML.AllocationChildID AND YSS.AllocationChildItemID = ML.AllocationChildItemID

                )
                SELECT * FROM FinalList;*/";
            }
            else
            {
                query =
                $@"
                 -- Master Data
                Select YDBookingMasterID, YDBookingDate, YDBookingNo, M.BuyerID, CT.ShortName BuyerName,ConceptNo = M.GroupConceptNo,
				CompanyId = CASE WHEN ISNULL(BM.BookingNo,'') <> '' THEN MAX(BM.CompanyID) ELSE MAX(SBM.ExecutionCompanyID) END,
                CM.IsBDS
                FROM {TableNames.YD_BOOKING_MASTER} M
                Inner Join {DbNames.EPYSL}..Contacts CT On CT.ContactID = M.BuyerID
                LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} CM ON CM.GroupConceptNo = M.GroupConceptNo
				LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster SBM ON SBM.BookingID=CM.BookingID 
				LEFT JOIN {DbNames.EPYSL}..BookingMaster BM ON BM.BookingID=CM.BookingID 
                LEFT JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID = BM.CompanyID
				LEFT JOIN {DbNames.EPYSL}..CompanyEntity SCOM ON SCOM.CompanyID = SBM.ExecutionCompanyID
                Where YDBookingMasterID = {ydBookingMasterId}
				GROUP BY YDBookingMasterID, YDBookingDate, YDBookingNo, M.BuyerID, CT.ShortName,M.GroupConceptNo,ISNULL(BM.BookingNo,''),CM.IsBDS;

                -- Childs Data
                With 
                YDBC As(
	                Select YDBC.YDBookingChildID,YDBC.YBChildItemID, YDBC.YDBookingMasterID, YDBC.ItemMasterID, YDBC.Remarks, YDBC.YarnCategory, YDBC.NoOfThread, YarnDyedColorID, ShadeCode =  YDBC.ColorCode
                            ,YDBC.UnitID, YDBC.BookingQty, YDBC.YarnProgramID,YDBC.LotNo,YDBC.PhysicalCount,YDBC.SpinnerID, FCMRC.YarnStockSetId,
	                        NetYarnReqQty = YDBC.BookingQty, ReqQty=SUM(ISNULL(YDRC.ReqQty,0)), AdvanceStockQty = ISNULL(YSM.AdvanceStockQty,0), SampleStockQty = ISNULL(YSM.SampleStockQty,0), FCMRC.YDItem
					FROM {TableNames.YDBookingChild} YDBC 
					LEFT JOIN {TableNames.YD_REQ_CHILD} YDRC ON YDRC.YDBookingChildID =YDBC.YDBookingChildID
					INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_CHILD} FCMRC ON FCMRC.FCMRChildID = YDBC.FCMRChildID
					LEFT JOIN {TableNames.YarnStockMaster_New} YSM ON YSM.ItemMasterID = YDBC.ItemMasterID 
														--AND YSM.SupplierID = YDRC.SupplierID 
														AND YSM.SpinnerID = YDBC.SpinnerID 
														AND ISNULL(YSM.YarnLotNo,'') = ISNULL(YDBC.LotNo,'')
														AND ISNULL(YSM.PhysicalCount,'') = ISNULL(YDBC.PhysicalCount,'')
														AND ISNULL(YSM.ShadeCode,'') = ISNULL(YDBC.ColorCode,'')
														--AND YSM.BookingID = YDRC.BookingID
														--AND YSM.LocationID = YDRC.LocationID
														--AND YSM.CompanyID = YDRC.CompanyID
	                Where YDBookingMasterID = {ydBookingMasterId}
					GROUP BY YDBC.YDBookingChildID,YDBC.YBChildItemID, YDBC.YDBookingMasterID, YDBC.ItemMasterID, YDBC.Remarks, YDBC.YarnCategory, YDBC.NoOfThread, YarnDyedColorID, YDBC.ColorCode 
                    ,YDBC.UnitID, YDBC.BookingQty, YDBC.YarnProgramID,YDBC.LotNo,YDBC.PhysicalCount,YDBC.SpinnerID, FCMRC.YarnStockSetId, YDBC.BookingQty, ISNULL(YSM.AdvanceStockQty,0), ISNULL(YSM.SampleStockQty,0), FCMRC.YDItem 
	                
                ),
                MainList AS (
	                Select YDBC.YBChildItemID, YDBC.YDBookingChildID, YDBC.YDBookingMasterID, YDBC.YarnProgramID, IM.ItemMasterID, YDBC.UnitID, 'Kg' AS DisplayUnitDesc,
	                        YDBC.BookingQty, YDBC.NetYarnReqQty, YDBC.BookingQty-YDBC.ReqQty ReqQty, YDBC.Remarks, YDBC.YarnCategory, YDBC.NoOfThread, 
	
	                        ISV1.SegmentValue AS Segment1ValueDesc, ISV2.SegmentValue AS Segment2ValueDesc, ISV3.SegmentValue AS Segment3ValueDesc,
	                        ISV4.SegmentValue AS Segment4ValueDesc, ISV5.SegmentValue AS Segment5ValueDesc, ISV6.SegmentValue AS Segment6ValueDesc,
	                        ISV7.SegmentValue AS Segment7ValueDesc,YDBC.ShadeCode,
					        AllocatedQty = 0, YDBC.YarnStockSetId,
					        YDBC.BookingQty-YDBC.ReqQty AS PendingQty,
							YDBC.PhysicalCount, YDBC.LotNo, YDBC.SpinnerID, YDBC.AdvanceStockQty, YDBC.SampleStockQty, YDBC.YDItem 
	                From YDBC
	                Left Join {DbNames.EPYSL}..EntityTypeValue EV On YDBC.YarnProgramID = EV.ValueID
	                Inner Join {DbNames.EPYSL}..Unit U ON YDBC.UnitID = U.UnitID
	                Inner Join {DbNames.EPYSL}..ItemMaster IM On YDBC.ItemMasterID = IM.ItemMasterID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
	                GROUP BY YDBC.YDBookingChildID, YDBC.YDBookingMasterID, YDBC.YarnProgramID, IM.ItemMasterID, YDBC.UnitID,
	                YDBC.BookingQty, YDBC.BookingQty, YDBC.Remarks, YDBC.YarnCategory, YDBC.NoOfThread,
	                ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue,
	                ISV4.SegmentValue, ISV5.SegmentValue, ISV6.SegmentValue,
	                ISV7.SegmentValue,YDBC.ShadeCode, YDBC.YarnStockSetId, YDBC.BookingQty, YDBC.NetYarnReqQty,YDBC.YBChildItemID,YDBC.ReqQty,YDBC.PhysicalCount, YDBC.LotNo, YDBC.SpinnerID, YDBC.AdvanceStockQty, YDBC.SampleStockQty, YDBC.YDItem 
                ),
                FinalList AS
                (
	                SELECT ML.YBChildItemID, ML.YDBookingChildID, ML.YDBookingMasterID, ML.YarnProgramID, ML.ItemMasterID, ML.UnitID,
	                        ML.BookingQty, ML.NetYarnReqQty, ML.ReqQty, ML.Remarks, ML.YarnCategory, ML.NoOfThread, 
	                        ML.Segment1ValueDesc, ML.Segment2ValueDesc, ML.Segment3ValueDesc,
	                        ML.Segment4ValueDesc, ML.Segment5ValueDesc, ML.Segment6ValueDesc,
	                        ML.Segment7ValueDesc,ML.ShadeCode,
					        ML.AllocatedQty, ML.YarnStockSetId,
					        PendingQty = isnull(ML.PendingQty,0),
							ML.PhysicalCount,
	                        ML.LotNo,
	                        SpinnerName = S1.Name,
                            SpinnerID = ML.SpinnerID,
							ML.AdvanceStockQty, ML.SampleStockQty, ML.YDItem 
	                FROM MainList ML
	                LEFT Join {DbNames.EPYSL}..Contacts S1 On S1.ContactID = ML.SpinnerID
                )
                SELECT * FROM FinalList;
				
				----StockTypes
                SELECT id = S.StockTypeId, text = S.Name
                FROM {TableNames.StockType} S
                WHERE StockTypeId NOT IN (9)  AND S.StockTypeId IN (3,5) 
                ORDER BY S.Name";
            }
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);

                YDReqMaster data = await records.ReadFirstOrDefaultAsync<YDReqMaster>();
                data.Childs = records.Read<YDReqChild>().ToList();
                if (isBDS != EnumBDSType.Bulk)
                {
                    data.StockTypeList.Add(new Select2OptionModel()
                    {
                        id = 0.ToString(),
                        text = "Select Stock Type"
                    });
                    data.StockTypeList.AddRange(records.Read<Select2OptionModel>().ToList());
                }
                data.Childs.ForEach(c =>
                {
                    if (data.IsBDS == EnumBDSType.Bulk)
                    {
                        c.StockTypeId = EnumStockType.AllocatedStock;
                        c.StockQty = c.AllocatedQty;
                    }
                    else
                    {
                        c.StockTypeId = EnumStockType.SampleStock;
                        c.StockQty = c.SampleStockQty;
                    }
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
        public async Task<YDReqMaster> GetAsync(int id)
        {
            var query =
                $@"
                -- Master Data
                Select M.YDReqMasterID, M.YDReqNo, M.YDReqBy, YDReqDate, M.YDBookingMasterID, BM.BuyerID, M.Remarks,BM.YDBookingDate, 
                BM.YDBookingNo, CT.ShortName BuyerName,FM.ConceptNo,FM.IsBDS
                FROM {TableNames.YD_REQ_MASTER} M
                Inner JOIN {TableNames.YD_BOOKING_MASTER} BM ON M.YDBookingMasterID = BM.YDBookingMasterID
                Inner Join {DbNames.EPYSL}..Contacts CT On CT.ContactID = BM.BuyerID
                Left JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FM ON FM.ConceptID = BM.ConceptID
                Where YDReqMasterID = {id};

			-- Childs Data
               With 
                YDBC As(
	                Select YDRC.YDReqChildID, YDRC.YDReqMasterID, YDRC.ItemMasterID, YDRC.Remarks, 
					YDRC.YarnCategory, YDRC.NoOfThread, YDRC.UnitID, YDRC.BookingQty, YDRC.ReqQty, 
					YDRC.YarnProgramID,YDRC.ReqCone,YDRC.ShadeCode ,YDRC.LotNo,YDRC.PhysicalCount, FCMRC.YarnStockSetId,
					YDRC.SpinnerID, YDBC.YBChildItemID, YDRC.AllocationChildItemID, NetYarnReqQty = YDRC.BookingQty, AdvanceStockQty = ISNULL(YSM.AdvanceStockQty,0), SampleStockQty = ISNULL(YSM.SampleStockQty,0), FCMRC.YDItem, YDRC.StockTypeId, StockTypeName = ST.Name   
	                From YDReqChild YDRC
					INNER JOIN {TableNames.YDBookingChild} YDBC ON YDBC.YDBookingChildID=YDRC.YDBookingChildID
					INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_CHILD} FCMRC ON FCMRC.FCMRChildID = YDBC.FCMRChildID
					--LEFT JOIN YarnStockMaster YSM ON YSM.YarnStockSetId = FCMRC.YarnStockSetId
					LEFT JOIN {TableNames.YarnStockMaster_New} YSM ON YSM.ItemMasterID = YDBC.ItemMasterID 
														--AND YSM.SupplierID = YDRC.SupplierID 
														AND YSM.SpinnerID = YDBC.SpinnerID 
														AND ISNULL(YSM.YarnLotNo,'') = ISNULL(YDBC.LotNo,'')
														AND ISNULL(YSM.PhysicalCount,'') = ISNULL(YDBC.PhysicalCount,'')
														AND ISNULL(YSM.ShadeCode,'') = ISNULL(YDBC.ColorCode,'')
														--AND YSM.BookingID = YDRC.BookingID
														--AND YSM.LocationID = YDRC.LocationID
														--AND YSM.CompanyID = YDRC.CompanyID
					LEFT JOIN StockType ST ON ST.StockTypeId = YDRC.StockTypeId
	                Where YDReqMasterID = {id}
                )

                Select YDBC.YDReqChildID, YDBC.YarnProgramID, IM.ItemMasterID, YDBC.UnitID, YDBC.BookingQty, YDBC.ReqQty, YDBC.Remarks, 
                YDBC.YarnCategory, YDBC.NoOfThread, 'Kg' As DisplayUnitDesc,YDBC.LotNo, YDBC.PhysicalCount, YDBC.SpinnerID, Spinner.ShortName [SpinnerName],
                ISV1.SegmentValue AS Segment1ValueDesc, ISV2.SegmentValue AS Segment2ValueDesc, ISV3.SegmentValue AS Segment3ValueDesc,
                ISV4.SegmentValue AS Segment4ValueDesc, ISV5.SegmentValue AS Segment5ValueDesc, ISV6.SegmentValue AS Segment6ValueDesc,
                ISV7.SegmentValue AS Segment7ValueDesc,YDBC.ReqCone,YDBC.ShadeCode,YDBC.YarnStockSetId,
				AllocatedQty = ISNULL(YACI.TotalAllocationQty,0),YDBC.YBChildItemID, PendingQty = YDBC.BookingQty, YDBC.NetYarnReqQty,
				YDBC.AdvanceStockQty, YDBC.SampleStockQty, YDBC.YDItem, YDBC.StockTypeId, YDBC.StockTypeName  

                From YDBC
				--LEFT JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.YBChildItemID = YDBC.YBChildItemID
				LEFT JOIN {TableNames.YARN_ALLOCATION_CHILD_ITEM} YACI ON YACI.AllocationChildItemID = YDBC.AllocationChildItemID
                Left Join {DbNames.EPYSL}..EntityTypeValue EV On YDBC.YarnProgramID = EV.ValueID
                Inner Join {DbNames.EPYSL}..Unit U ON YDBC.UnitID = U.UnitID
                Inner Join {DbNames.EPYSL}..ItemMaster IM On YDBC.ItemMasterID = IM.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
				LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
				LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
				LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                LEFT Join {DbNames.EPYSL}..Contacts Spinner On Spinner.ContactID = YDBC.SpinnerID
				GROUP BY YDBC.YDReqChildID, YDBC.YarnProgramID, IM.ItemMasterID, YDBC.UnitID, YDBC.BookingQty, YDBC.ReqQty, YDBC.Remarks, 
                YDBC.YarnCategory, YDBC.NoOfThread,YDBC.LotNo, YDBC.PhysicalCount, YDBC.SpinnerID, Spinner.ShortName,
                ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue,ISV4.SegmentValue, ISV5.SegmentValue, ISV6.SegmentValue,
                ISV7.SegmentValue,YDBC.ReqCone,YDBC.ShadeCode,YDBC.YarnStockSetId,YDBC.YBChildItemID,ISNULL(YACI.TotalAllocationQty,0), YDBC.NetYarnReqQty, YDBC.AdvanceStockQty, YDBC.SampleStockQty, YDBC.YDItem, YDBC.StockTypeId, YDBC.StockTypeName;

				----StockTypes
                SELECT id = S.StockTypeId, text = S.Name
                FROM {TableNames.StockType} S
                WHERE StockTypeId NOT IN (9)  AND S.StockTypeId IN (3,5) 
                ORDER BY S.Name";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);

                YDReqMaster data = await records.ReadFirstOrDefaultAsync<YDReqMaster>();
                data.Childs = records.Read<YDReqChild>().ToList();
                if (data.IsBDS != EnumBDSType.Bulk)
                {
                    data.StockTypeList.Add(new Select2OptionModel()
                    {
                        id = 0.ToString(),
                        text = "Select Stock Type"
                    });
                    data.StockTypeList.AddRange(records.Read<Select2OptionModel>().ToList());
                }
                data.Childs.ForEach(c =>
                {
                    if (data.IsBDS == EnumBDSType.Bulk)
                    {
                        //c.StockTypeId = EnumStockType.AllocatedStock;
                        //c.StockQty = c.AllocatedQty;
                    }
                    else
                    {
                        if (c.StockTypeId == EnumStockType.SampleStock)
                        {
                            c.StockQty = c.SampleStockQty;
                        }
                        if (c.StockTypeId == EnumStockType.AdvanceStock)
                        {
                            c.StockQty = c.AdvanceStockQty;
                        }
                    }
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
        public async Task<YDReqMaster> GetAllAsync(int id)
        {
            var sql = $@"
            ;Select * FROM {TableNames.YD_REQ_MASTER} Where YDReqMasterID = {id}
            ;Select * From {TableNames.YD_REQ_CHILD} Where YDReqMasterID = {id}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YDReqMaster data = await records.ReadFirstOrDefaultAsync<YDReqMaster>();
                Guard.Against.NullObject(data);
                data.Childs = records.Read<YDReqChild>().ToList();
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
        public async Task SaveAsync(YDReqMaster entity)
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
                        entity.YDReqMasterID = await _service.GetMaxIdAsync(TableNames.YD_REQ_MASTER, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        entity.YDReqNo = await _service.GetMaxNoAsync(TableNames.YD_REQ_NO, 1, RepeatAfterEnum.NoRepeat, "00000", transactionGmt, _connectionGmt);

                        maxChildId = await _service.GetMaxIdAsync(TableNames.YD_REQ_CHILD, entity.Childs.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

                        foreach (YDReqChild item in entity.Childs)
                        {
                            item.YDReqChildID = maxChildId++;
                            item.YDReqMasterID = entity.YDReqMasterID;
                        }
                        break;

                    case EntityState.Modified:
                        var addedChilds = entity.Childs.FindAll(x => x.EntityState == EntityState.Added);
                        maxChildId = await _service.GetMaxIdAsync(TableNames.YD_REQ_CHILD, addedChilds.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

                        foreach (var item in addedChilds)
                        {
                            item.YDReqChildID = maxChildId++;
                            item.YDReqMasterID = entity.YDReqMasterID;
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
                /*
                #region Stock Operation
                if (entity.IsApprove && entity.IsAcknowledge == false)
                {
                    if (entity.ApproveBy.IsNull()) entity.ApproveBy = 0;
                    int userId = entity.EntityState == EntityState.Added ? entity.AddedBy : entity.ApproveBy;
                    await _connection.ExecuteAsync("spYarnStockOperation", new { MasterID = entity.YDReqMasterID, FromMenuType = EnumFromMenuType.YDReqApp, UserId = userId }, transaction, 30, CommandType.StoredProcedure);
                }
                #endregion Stock Operation
				*/

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
                if (transaction != null) transaction.Dispose();
                if (transactionGmt != null) transactionGmt.Dispose();
                _connection.Close();
                _connectionGmt.Close();
            }
        }
    }
}
