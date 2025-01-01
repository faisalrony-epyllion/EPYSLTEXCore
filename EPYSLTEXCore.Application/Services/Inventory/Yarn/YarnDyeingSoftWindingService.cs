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
    public class YarnDyeingSoftWindingService : IYarnDyeingSoftWindingService
    {
        private readonly IDapperCRUDService<YDReceiveMaster> _service;

        SqlTransaction transactionGmt = null;
        private SqlTransaction transaction = null;
        private readonly SqlConnection _connection;
        private readonly SqlConnection _connectionGmt;
        public YarnDyeingSoftWindingService(IDapperCRUDService<YDReceiveMaster> service)
        {


            _service = service;

            _service.Connection = service.GetConnection(AppConstants.GMT_CONNECTION);
            _connectionGmt = service.Connection;

            _service.Connection = service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _connection = service.Connection;

        }

        public async Task<List<SoftWindingMaster>> GetPagedAsync(Status status, string pageName, PaginationInfo paginationInfo)
        {
            string orderBy = "";
            int IsSendForApprove = -1, IsApprove = -1, IsAcknowledge = -1, IsReject = -1;

            if (pageName == "YarnDyeingSoftWinding")
            {
                if (status == Status.Pending || status == Status.Draft) //Requisition List
                {
                    IsSendForApprove = 0;
                }
                else if (status == Status.AwaitingPropose) //Pending for Approval, Pending Approval
                {
                    IsSendForApprove = 1;
                    IsApprove = 0;
                    IsReject = 0;
                }
                else if (status == Status.Approved) //Approve List, YD Yarn Requisiton
                {
                    IsApprove = 1;
                    IsAcknowledge = 0;
                }
                else if (status == Status.Reject) //Reject List
                {
                    IsReject = 1;
                }
            }
            else if (pageName == "YarnDyeingSoftWindingApproval")
            {
                if (status == Status.AwaitingPropose) //Pending for Approval, Pending Approval
                {
                    IsSendForApprove = 1;
                    IsApprove = 0;
                    IsReject = 0;
                }
                else if (status == Status.Approved) //Approve List, YD Yarn Requisiton
                {
                    IsApprove = 1;
                }
                else if (status == Status.Reject) //Reject List
                {
                    IsReject = 1;
                }
            }
            /*else if (pageName == "YDYarnAcknowledgement")
            {
                if (status == Status.Approved) //Approve List, YD Yarn Requisiton
                {
                    IsApprove = 1;
                    IsAcknowledge = 0;
                }
                if (status == Status.Acknowledge) //Acknowledgement List
                {
                    IsApprove = 1;
                    IsAcknowledge = 1;
                }
            }*/

            string sql;
            if (status == Status.Pending)
            {
                sql =
                $@"  
                /*
                ;With YDBC AS 
			   (
				Select YDBookingMasterID, TotalBookingQty = SUM(YDBC.BookingQty) FROM {TableNames.YDBookingChild} YDBC
				GROUP BY YDBookingMasterID
			   ),
			   YDRC AS 
			   (
				Select YDReceiveMasterID, ReceiveQty = SUM(ISNULL(YDRC.ReceiveQty,0)) FROM {TableNames.YD_RECEIVE_CHILD} YDRC
				GROUP BY YDReceiveMasterID
			   ),
			   SWC AS 
			   (
				Select SoftWindingMasterID, SoftWindingQty = SUM(ISNULL(SWC.Qty,0)) FROM {TableNames.SOFT_WINDING_CHILD} SWC
				GROUP BY SoftWindingMasterID
			   ),
			   A AS 
                ( 
                    Select YDBM.YDBatchID, YDBM.YDBatchNo, YDBM.YDBatchDate, YBM.YDBookingMasterID, YBM.YDBookingNo, YBM.YDBookingDate, FM.ConceptNo,YBM.ConceptID, YBM.Remarks, 
                    TotalBookingQty = ISNULL(YDBC.TotalBookingQty,0), ReceiveQty = ISNULL(YDRC.ReceiveQty,0), Qty= SUM(ISNULL(SWC.SoftWindingQty,0))
                    FROM {TableNames.YD_BATCH_MASTER} YDBM
					INNER JOIN {TableNames.YD_BATCH_ITEM_REQUIREMENT} IR ON IR.YDBatchID = YDBM.YDBatchID
					INNER JOIN {TableNames.YD_BOOKING_MASTER} YBM ON YBM.YDBookingMasterID = YDBM.YDBookingMasterID
					Inner JOIN YDBC ON YDBC.YDBookingMasterID = YBM.YDBookingMasterID  
					INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FM ON FM.ConceptID = YBM.ConceptID
					INNER JOIN {TableNames.YD_REQ_MASTER} YDR ON YDR.YDBookingMasterID = YBM.YDBookingMasterID
					INNER JOIN {TableNames.YD_REQ_ISSUE_MASTER} YDI ON YDI.YDReqMasterID = YDR.YDReqMasterID
					INNER JOIN {TableNames.YD_RECEIVE_MASTER} YDRM ON YDRM.YDReqIssueMasterID = YDI.YDReqIssueMasterID
					INNER JOIN YDRC ON YDRC.YDReceiveMasterID= YDRM.YDReceiveMasterID
                    Left JOIN {TableNames.SOFT_WINDING_MASTER} RM ON RM.YDBookingMasterID = YBM.YDBookingMasterID
					Left Join SWC ON SWC.SoftWindingMasterID = RM.SoftWindingMasterID
                    where YBM.IsAcknowledge = 1 AND ISNULL(YDRC.ReceiveQty,0) > ISNULL(SWC.SoftWindingQty,0)--AND RM.YDBookingMasterID IS NULL
					GROUP BY YDBM.YDBatchID, YDBM.YDBatchNo, YDBM.YDBatchDate, YBM.YDBookingMasterID, YBM.YDBookingNo, YBM.YDBookingDate, FM.ConceptNo,YBM.ConceptID, YBM.Remarks, 
                    YDBC.TotalBookingQty, YDRC.ReceiveQty
                )
                Select *, Count(*) Over() TotalRows From A 
                */

                ;With B AS 
                ( 
                    Select YDBM.YDBatchID, YDBM.YDBatchNo, YDBM.YDBatchDate, YBM.YDBookingMasterID, YBM.YDBookingNo, YBM.YDBookingDate, FM.ConceptNo,YBM.ConceptID, YBM.Remarks, 
                    TotalBookingQty = SUM(ISNULL(YDBC.BookingQty,0)), ReceiveQty = SUM(ISNULL(YDRVC.ReceiveQty,0)), Qty= SUM(ISNULL(SWC.Qty,0))
                    FROM {TableNames.YD_BATCH_MASTER} YDBM
					Left JOIN {TableNames.YD_BATCH_ITEM_REQUIREMENT} IR ON IR.YDBatchID = YDBM.YDBatchID
					Left JOIN {TableNames.YD_BOOKING_MASTER} YBM ON YBM.YDBookingMasterID = YDBM.YDBookingMasterID
					Left JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FM ON FM.ConceptID = YBM.ConceptID
					Left JOIN {TableNames.YDBookingChild} YDBC ON YDBC.YDBookingChildID = IR.YDBookingChildID 
					Left JOIN {TableNames.YD_REQ_CHILD} YDRQC ON YDRQC.YDBookingChildID = YDBC.YDBookingChildID
					Left JOIN {TableNames.YD_REQ_ISSUE_CHILD} YDRIC ON YDRIC.YDReqChildID = YDRQC.YDReqChildID
					Left JOIN {TableNames.YD_RECEIVE_CHILD} YDRVC ON YDRVC.YDReqIssueChildID = YDRIC.YDReqIssueChildID
					Left JOIN {TableNames.SOFT_WINDING_CHILD} SWC ON SWC.YDBookingChildID = IR.YDBookingChildID
                    where YBM.IsAcknowledge = 1
					GROUP BY YDBM.YDBatchID, YDBM.YDBatchNo, YDBM.YDBatchDate, YBM.YDBookingMasterID, YBM.YDBookingNo, YBM.YDBookingDate, FM.ConceptNo,YBM.ConceptID, YBM.Remarks                    
                ),
				A AS
				(
				Select * FROM B Where  ISNULL(ReceiveQty,0) > ISNULL(Qty,0)
				)
                Select *, Count(*) Over() TotalRows From A
";

                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By YDBatchID Desc" : paginationInfo.OrderBy;
            }
            else
            {
                sql = $@"
                ;With
                YRM As 
                (
	                Select *
	                FROM {TableNames.SOFT_WINDING_MASTER} 
	                Where IsSendForApprove = (Case when {IsSendForApprove} = -1 Then IsSendForApprove Else {IsSendForApprove} End)
	                AND IsApprove = (Case when {IsApprove} = -1 Then IsApprove Else {IsApprove} End) 
	                AND IsReject = (Case when {IsReject} = -1 Then IsReject Else {IsReject} End) 
                )
                Select YRM.SoftWindingMasterID, YRM.YDBookingMasterID, YRM.SoftWindingNo, YRM.SoftWindingDate, YDBM.BuyerID, C.ShortName AS BuyerName, YRM.Remarks, 
                YRM.IsSendForApprove, RL.Name As SendForApproveName,  YRM.SendForApproveDate, YRM.IsApprove, A.Name As ApproveName, YRM.ApproveDate, 
                YRM.IsReject, R.Name As RejectName, YRM.RejectDate, YRM.AddedBy,
                Qty = Sum(YRC.Qty), Cone = SUM(YRC.Cone), ReceiveQty = SUM(YDRVC.ReceiveQty)
                From YRM
                Inner JOIN {TableNames.SOFT_WINDING_CHILD} YRC ON YRC.SoftWindingMasterID = YRM.SoftWindingMasterID 
				Inner JOIN {TableNames.YD_BOOKING_MASTER} YDBM ON YDBM.YDBookingMasterID = YRM.YDBookingMasterID 
				INNER JOIN {TableNames.YDBookingChild} YDBC ON YDBC.YDBookingChildID = YRC.YDBookingChildID
				INNER JOIN {TableNames.YD_REQ_CHILD} YDRQC ON YDRQC.YDBookingChildID = YDBC.YDBookingChildID
				INNER JOIN {TableNames.YD_REQ_ISSUE_CHILD} YDRIC ON YDRIC.YDReqChildID = YDRQC.YDReqChildID
				INNER JOIN {TableNames.YD_RECEIVE_CHILD} YDRVC ON YDRVC.YDReqIssueChildID = YDRIC.YDReqIssueChildID
                Left join  {DbNames.EPYSL}..Contacts C ON c.ContactID = YDBM.BuyerID 
                LEFT Join  {DbNames.EPYSL}..LoginUser RL On RL.UserCode = YRM.SendForApproveBy 
                LEFT Join  {DbNames.EPYSL}..LoginUser A On A.UserCode = YRM.ApproveBy
                LEFT Join  {DbNames.EPYSL}..LoginUser R On R.UserCode = YRM.RejectBy 
                Group By YRM.SoftWindingMasterID, YRM.YDBookingMasterID, YRM.SoftWindingNo, YRM.SoftWindingDate, YDBM.BuyerID, C.ShortName, YRM.Remarks, 
                YRM.IsSendForApprove, RL.Name, YRM.SendForApproveDate, YRM.IsApprove, A.Name, YRM.ApproveDate, 
                YRM.IsReject, R.Name, YRM.RejectDate, YRM.AddedBy";

                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By YRM.SoftWindingMasterID Desc" : paginationInfo.OrderBy;
            }
            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<SoftWindingMaster>(sql);
        }
        public async Task<SoftWindingMaster> GetNewAsync(int YDBatchID)
        {
            var sql =
                $@"
                SELECT DISTINCT BM.YDBatchID, BM.YDBatchNo, BM.YDBatchDate ,YDBM.YDBookingMasterID, YDBM.YDBookingDate, FCM.BuyerID, C.ShortName BuyerName, FCM.CompanyID As ReqFromID, CE.ShortName As Company
                FROM {TableNames.YD_BATCH_MASTER} BM
				INNER JOIN {TableNames.YD_BOOKING_MASTER} YDBM ON YDBM.YDBookingMasterID = BM.YDBookingMasterID
                Inner JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM on FCM.ConceptID = YDBM.ConceptID 
				LEFT JOIN {DbNames.EPYSL}..Contacts C ON FCM.BuyerID=C.ContactID
				Inner Join {DbNames.EPYSL}..CompanyEntity CE On CE.CompanyID = FCM.CompanyID
                WHERE BM.YDBatchID = {YDBatchID};
 
                -- Child
                ;WITH X AS (
                    SELECT * FROM {TableNames.YD_BATCH_ITEM_REQUIREMENT} PC WHERE PC.YDBatchID = {YDBatchID}
                )
                SELECT YDRIC.YDReqIssueChildID, X.YDBItemReqID, X.YDBatchID, YDBC.YDBookingChildID, YDBC.YDBookingMasterID, YDBC.ItemMasterID, YDBC.ShadeCode, YDBC.NoOfThread, YDBC.ColorId As ColorID, 
                Color.SegmentValue AS ColorName, YDBC.BookingFor, YDF.YDyeingFor As BookingForName,YDBC.BookingQty, YDBC.UnitId As UnitID, 
                'Kg' As DisplayUnitDesc, YDBC.Remarks, 
                IM.Segment1ValueID Segment1ValueId, IM.Segment2ValueID Segment2ValueId, IM.Segment3ValueID Segment3ValueId, IM.Segment4ValueID Segment4ValueId,
                IM.Segment5ValueID Segment5ValueId, IM.Segment6ValueID Segment6ValueId, IM.Segment7ValueID Segment7ValueId, IM.Segment8ValueID Segment8ValueId,
                IM.Segment9ValueID Segment9ValueId, IM.Segment10ValueID Segment10ValueId, IM.Segment11ValueID Segment11ValueId, IM.Segment12ValueID Segment12ValueId,
                IM.Segment13ValueID Segment13ValueId, IM.Segment14ValueID Segment14ValueId, IM.Segment15ValueID Segment15ValueId,
                ISV1.SegmentValue AS Segment1ValueDesc, ISV2.SegmentValue AS Segment2ValueDesc, ISV3.SegmentValue AS Segment3ValueDesc,
                ISV4.SegmentValue AS Segment4ValueDesc, ISV5.SegmentValue AS Segment5ValueDesc, ISV6.SegmentValue AS Segment6ValueDesc,
                ISV7.SegmentValue AS Segment7ValueDesc, ISV8.SegmentValue AS Segment8ValueDesc, YDBC.YarnCategory,  
				ReceiveQty = SUM(ISNULL(YDRVC.ReceiveQty,0)), ReceiveCone = SUM(ISNULL(YDRVC.ReceiveCone,0)), ReceiveCarton = SUM(ISNULL(YDRVC.ReceiveCarton,0)),
				YDRVC.YDReceiveChildID, YDRVC.YDRICRBId
                FROM X
				INNER JOIN {TableNames.YD_BATCH_ITEM_REQUIREMENT} BI ON BI.YDBItemReqID = X.YDBItemReqID
				INNER JOIN {TableNames.YDBookingChild} YDBC ON YDBC.YDBookingChildID = X.YDBookingChildID 
				INNER JOIN {TableNames.YD_REQ_CHILD} YDRQC ON YDRQC.YDBookingChildID = YDBC.YDBookingChildID
				INNER JOIN {TableNames.YD_REQ_ISSUE_CHILD} YDRIC ON YDRIC.YDReqChildID = YDRQC.YDReqChildID
				INNER JOIN {TableNames.YD_RECEIVE_CHILD} YDRVC ON YDRVC.YDReqIssueChildID = YDRIC.YDReqIssueChildID
                LEFT JOIN {TableNames.DyeingProcessPart_HK} DP ON DP.DPID = YDBC.DPID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = YDBC.ColorId
                LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YDBC.ItemMasterID

                Left JOIN {TableNames.YarnDyeingFor_HK} YDF On YDF.YDyeingForID = YDBC.BookingFor
                LEFT Join {DbNames.EPYSL}..EntityTypeValue EV On YDBC.YarnProgramId = EV.ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV8 ON ISV8.SegmentValueID = IM.Segment8ValueID
                LEFT JOIN {DbNames.EPYSL}..Unit UN ON YDBC.UnitID = UN.UnitID
				GROUP BY YDRIC.YDReqIssueChildID, X.YDBItemReqID, X.YDBatchID, YDBC.YDBookingChildID, YDBC.YDBookingMasterID, YDBC.ItemMasterID, YDBC.ShadeCode, YDBC.NoOfThread, YDBC.ColorId, 
                Color.SegmentValue, YDBC.BookingFor, YDF.YDyeingFor, YDBC.BookingQty, YDBC.UnitId, 
                YDBC.Remarks, IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, IM.Segment4ValueID,
                IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID, IM.Segment8ValueID, IM.Segment9ValueID, IM.Segment10ValueID, IM.Segment11ValueID, 
				IM.Segment12ValueID, IM.Segment13ValueID, IM.Segment14ValueID, IM.Segment15ValueID, ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue,
                ISV4.SegmentValue, ISV5.SegmentValue, ISV6.SegmentValue, ISV7.SegmentValue, ISV8.SegmentValue, YDBC.YarnCategory, YDRVC.YDReceiveChildID, YDRVC.YDRICRBId;

";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                SoftWindingMaster data = await records.ReadFirstOrDefaultAsync<SoftWindingMaster>();
                data.Childs = records.Read<SoftWindingChild>().ToList();
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
        public async Task<SoftWindingMaster> GetAsync(int id)
        {
            var segmentNames = new
            {
                SegmentNames = new string[]
               {
                    ItemSegmentNameConstants.FABRIC_COLOR
               }
            };

            var query =
                $@"
                Select YRM.SoftWindingMasterID, YRM.YDBookingMasterID, YRM.SoftWindingNo, YRM.SoftWindingDate, C.ShortName, YRM.Remarks, FCM.CompanyID As ReqFromID, CE.ShortName As Company 
                FROM {TableNames.SOFT_WINDING_MASTER} YRM 
				Inner JOIN {TableNames.YD_BOOKING_MASTER} YDBM ON YDBM.YDBookingMasterID = YRM.YDBookingMasterID  
				Inner JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM on FCM.ConceptID = YDBM.ConceptID 
                Left join  {DbNames.EPYSL}..Contacts C ON c.ContactID = FCM.CompanyID 
				Inner Join {DbNames.EPYSL}..CompanyEntity CE On CE.CompanyID = FCM.CompanyID
                Where YRM.SoftWindingMasterID = {id};

                ;With
                YRC As 
                (
	                Select * FROM {TableNames.SOFT_WINDING_CHILD} Where SoftWindingMasterID = {id}
                )
                Select YRC.SoftWindingChildID, YRC.YDBookingChildID, YRC.SoftWindingMasterID, YRC.ItemMasterID, YRC.ColorID, YRC.Remarks,
                YRC.Qty, YRC.Cone, YRC.YarnCategory, Color.SegmentValue AS ColorName,
                IM.Segment1ValueID Segment1ValueId, IM.Segment2ValueID Segment2ValueId, IM.Segment3ValueID Segment3ValueId, IM.Segment4ValueID Segment4ValueId,
                IM.Segment5ValueID Segment5ValueId, IM.Segment6ValueID Segment6ValueId, IM.Segment7ValueID Segment7ValueId, IM.Segment8ValueID Segment8ValueId,
                IM.Segment9ValueID Segment9ValueId, IM.Segment10ValueID Segment10ValueId, IM.Segment11ValueID Segment11ValueId, IM.Segment12ValueID Segment12ValueId,
                IM.Segment13ValueID Segment13ValueId, IM.Segment14ValueID Segment14ValueId, IM.Segment15ValueID Segment15ValueId,
                ISV1.SegmentValue AS Segment1ValueDesc, ISV2.SegmentValue AS Segment2ValueDesc, ISV3.SegmentValue AS Segment3ValueDesc,
                ISV4.SegmentValue AS Segment4ValueDesc, ISV5.SegmentValue AS Segment5ValueDesc, ISV6.SegmentValue AS Segment6ValueDesc,
                ISV7.SegmentValue AS Segment7ValueDesc, ISV8.SegmentValue AS Segment8ValueDesc,  
				ReceiveQty = SUM(ISNULL(YDRVC.ReceiveQty,0)), ReceiveCone = SUM(ISNULL(YDRVC.ReceiveCone,0)), ReceiveCarton = SUM(ISNULL(YDRVC.ReceiveCarton,0)) 
                From YRC 
				INNER JOIN {TableNames.YDBookingChild} YDBC ON YDBC.YDBookingChildID = YRC.YDBookingChildID
				INNER JOIN {TableNames.YD_REQ_CHILD} YDRQC ON YDRQC.YDBookingChildID = YDBC.YDBookingChildID
				INNER JOIN {TableNames.YD_REQ_ISSUE_CHILD} YDRIC ON YDRIC.YDReqChildID = YDRQC.YDReqChildID
				INNER JOIN {TableNames.YD_RECEIVE_CHILD} YDRVC ON YDRVC.YDReqIssueChildID = YDRIC.YDReqIssueChildID
				LEFT JOIN {DbNames.EPYSL}..Unit U ON U.UnitID = YDBC.UnitID 
                INNER JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = YDBC.ColorId
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YRC.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV8 ON ISV8.SegmentValueID = IM.Segment8ValueID
                Group By YRC.SoftWindingChildID, YRC.YDBookingChildID, YRC.SoftWindingMasterID, YRC.ItemMasterID, YRC.ColorID, YRC.Remarks,
                YRC.Qty, YRC.Cone, YRC.YarnCategory, Color.SegmentValue,
                IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, IM.Segment4ValueID,
                IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID, IM.Segment8ValueID,
                IM.Segment9ValueID, IM.Segment10ValueID, IM.Segment11ValueID, IM.Segment12ValueID,
                IM.Segment13ValueID, IM.Segment14ValueID, IM.Segment15ValueID,
                ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue,
                ISV4.SegmentValue, ISV5.SegmentValue, ISV6.SegmentValue,
                ISV7.SegmentValue, ISV8.SegmentValue  ;
";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query, segmentNames);

                SoftWindingMaster data = records.Read<SoftWindingMaster>().FirstOrDefault();
                data.Childs = records.Read<SoftWindingChild>().ToList();

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
        public async Task<SoftWindingMaster> GetAllAsync(int id)
        {
            var sql = $@"
            ;Select * FROM {TableNames.SOFT_WINDING_MASTER} Where SoftWindingMasterID = {id}

            ;Select * FROM {TableNames.SOFT_WINDING_CHILD} Where SoftWindingMasterID = {id}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                SoftWindingMaster data = await records.ReadFirstOrDefaultAsync<SoftWindingMaster>();
                Guard.Against.NullObject(data);
                data.Childs = records.Read<SoftWindingChild>().ToList();
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

        public async Task SaveAsync(SoftWindingMaster entity)
        {
            SqlTransaction transaction = null;
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();
                await _connectionGmt.OpenAsync();
                transactionGmt = _connectionGmt.BeginTransaction();

                int maxChildId = 0;
                //List<SoftWindingChild> childRecords = entity.Childs;
                //_itemMasterRepository.GenerateItem(AppConstants.ITEM_SUB_GROUP_YARN_NEW, ref childRecords);

                switch (entity.EntityState)
                {
                    case EntityState.Added:
                        entity.SoftWindingMasterID = await _service.GetMaxIdAsync(TableNames.SOFT_WINDING_MASTER, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        entity.SoftWindingNo =await _service.GetMaxNoAsync(TableNames.SOFT_WINDING_NO,1, RepeatAfterEnum.NoRepeat,"0000000", transactionGmt, _connectionGmt);

                        maxChildId = await _service.GetMaxIdAsync(TableNames.SOFT_WINDING_CHILD, entity.Childs.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        foreach (var item in entity.Childs)
                        {
                            item.SoftWindingChildID = maxChildId++;
                            item.SoftWindingMasterID = entity.SoftWindingMasterID;
                        }
                        break;

                    case EntityState.Modified:
                        var addedChilds = entity.Childs.FindAll(x => x.EntityState == EntityState.Added);
                        maxChildId = await _service.GetMaxIdAsync(TableNames.YARN_YD_REQ_CHILD, addedChilds.Count,RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

                        foreach (var item in addedChilds)
                        {
                            item.SoftWindingChildID = maxChildId++;
                            item.SoftWindingMasterID = entity.SoftWindingMasterID;
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
        public async Task UpdateEntityAsync(SoftWindingMaster entity)
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
