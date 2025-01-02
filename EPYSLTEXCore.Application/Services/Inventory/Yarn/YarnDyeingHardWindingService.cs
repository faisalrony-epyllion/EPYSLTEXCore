using Dapper;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Application.Interfaces.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using System.Data.Entity;
using Microsoft.Data.SqlClient;
using EPYSLTEXCore.Application.Interfaces;

namespace EPYSLTEXCore.Application.Services.Inventory
{
    public class YarnDyeingHardWindingService : IYarnDyeingHardWindingService
    {
        private readonly IDapperCRUDService<HardWindingMaster> _service;
        private readonly SqlConnection _connection;
        private readonly SqlConnection _connectionGmt;
        private readonly IItemMasterService<HardWindingChild> _itemMasterRepository;
        public YarnDyeingHardWindingService(IDapperCRUDService<HardWindingMaster> service,
            IItemMasterService<HardWindingChild> itemMasterRepository)
        {
            _service = service;
            _service.Connection = service.GetConnection(AppConstants.GMT_CONNECTION);
            _connectionGmt = service.Connection;

            _service.Connection = service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _connection = service.Connection;
            _itemMasterRepository = itemMasterRepository;
        }

        public async Task<List<HardWindingMaster>> GetPagedAsync(Status status, string pageName, PaginationInfo paginationInfo)
        {
            string orderBy = "";
            int IsSendForApprove = -1, IsApprove = -1, IsAcknowledge = -1, IsReject = -1;

            if (pageName == "YarnDyeingHardWinding")
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
            else if (pageName == "YarnDyeingHardWindingApproval")
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
                $@";WITH YDBC AS 
			           (
				        Select YDBookingMasterID, TotalBookingQty = SUM(YDBC.BookingQty) FROM {TableNames.YDBookingChild} YDBC
				        GROUP BY YDBookingMasterID
			           ),
			           YDRC AS 
			           (
			           Select IR.YDBatchID,ProducedQty = SUM(ISNULL(PC.ProducedQty,0)) FROM {TableNames.YD_BATCH_ITEM_REQUIREMENT} IR
			           INNER JOIN {TableNames.YD_DYEING_BATCH_ITEM} BI ON BI.YDBItemReqID = IR.YDBItemReqID
			           INNER JOIN {TableNames.YD_PRODUCTION_CHILD} PC ON PC.YDDBIID = BI.YDDBIID
			           GROUP BY IR.YDBatchID
			           ),
			           HWC AS 
			           (
				        Select HardWindingMasterID, HardWindingQty = SUM(ISNULL(SWC.Qty,0)) FROM {TableNames.HardWindingChild} SWC
				        GROUP BY HardWindingMasterID
			           ),
			           M AS (
                            SELECT	YDBM.YDBatchID, YDBM.YDBatchNo, YDBM.YDBatchDate, YBM.YDBookingMasterID, YBM.BuyerID, YDBM.Remarks, YDBM.IsApproved,
					        YBM.YDBookingNo,YBM.YDBookingDate,FM.ConceptNo,YBM.ConceptID,TotalBookingQty = ISNULL(YDBC.TotalBookingQty,0),
					        ProducedQty = ISNULL(YDRC.ProducedQty,0), Qty= SUM(ISNULL(HWC.HardWindingQty,0))
                            FROM {TableNames.YD_BATCH_MASTER} YDBM
					        --INNER JOIN {TableNames.YD_BATCH_ITEM_REQUIREMENT} IR ON IR.YDBatchID = YDBM.YDBatchID
					        INNER JOIN {TableNames.YD_BOOKING_MASTER} YBM ON YBM.YDBookingMasterID = YDBM.YDBookingMasterID
					        INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FM ON FM.ConceptID = YBM.ConceptID
					        INNER JOIN YDBC ON YDBC.YDBookingMasterID = YBM.YDBookingMasterID 
					        INNER JOIN YDRC ON YDRC.YDBatchID = YDBM.YDBatchID
					        INNER JOIN {TableNames.YD_DRYER_FINISHING_MASTER} YDDFM ON YDDFM.YDBatchID = YDBM.YDBatchID
					        LEFT JOIN {TableNames.HardWindingMaster} HWM ON HWM.YDBatchID = YDBM.YDBatchID
					        LEFT JOIN HWC ON HWC.HardWindingMasterID = HWM.HardWindingMasterID
					        WHERE ISNULL(YDBM.IsApproved,0) = 1 AND HWM.HardWindingMasterID IS NULL
					        GROUP BY YDBM.YDBatchID, YDBM.YDBatchNo, YDBM.YDBatchDate, YBM.YDBookingMasterID, YBM.BuyerID, YDBM.Remarks, YDBM.IsApproved,
					        YBM.YDBookingNo,YBM.YDBookingDate,FM.ConceptNo,YBM.ConceptID,YDBC.TotalBookingQty,YDRC.ProducedQty,HWC.HardWindingQty
                        )
                    SELECT M.YDBatchID, M.YDBatchNo, M.YDBatchDate, M.YDBookingMasterID, M.BuyerID, M.Remarks, M.IsApproved,
                    BM.YDBookingNo,BM.YDBNo, BM.YDBookingDate, CTO.Name Buyer,M.ConceptNo,M.ConceptID,M.Remarks,M.TotalBookingQty,M.ProducedQty,M.Qty, 
				    Count(*) Over() TotalRows
                    FROM M
                    INNER JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = M.BuyerID
                    INNER JOIN {TableNames.YD_BOOKING_MASTER} BM ON BM.YDBookingMasterID = M.YDBookingMasterID
                ";

                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By YDBookingMasterID Desc" : paginationInfo.OrderBy;
            }
            else
            {
                sql = $@"
                ;WITH YDBC AS 
			           (
				        Select YDBookingMasterID, TotalBookingQty = SUM(YDBC.BookingQty) FROM {TableNames.YDBookingChild} YDBC
				        GROUP BY YDBookingMasterID
			           ),
			           YDRC AS 
			           (
			           Select IR.YDBatchID,ProducedQty = SUM(ISNULL(PC.ProducedQty,0)) FROM {TableNames.YD_BATCH_ITEM_REQUIREMENT} IR
			           INNER JOIN {TableNames.YD_DYEING_BATCH_ITEM} BI ON BI.YDBItemReqID = IR.YDBItemReqID
			           INNER JOIN {TableNames.YD_PRODUCTION_CHILD} PC ON PC.YDDBIID = BI.YDDBIID
			           GROUP BY IR.YDBatchID
			           ),
			           HWC AS 
			           (
				        Select HardWindingMasterID, HardWindingQty = SUM(ISNULL(SWC.Qty,0)) FROM {TableNames.HardWindingChild} SWC
				        GROUP BY HardWindingMasterID
			           ),
					   YRM As 
						(
							Select *
							FROM {TableNames.HardWindingMaster} 
							Where IsSendForApprove = (Case when {IsSendForApprove} = -1 Then IsSendForApprove Else {IsSendForApprove} End)
							AND IsApprove = (Case when {IsApprove} = -1 Then IsApprove Else {IsApprove} End) 
							AND IsReject = (Case when {IsReject} = -1 Then IsReject Else {IsReject} End) 
						),
						M AS (
                            SELECT HWM.HardWindingMasterID,HWM.HardWindingNo,YDBM.YDBatchID, YDBM.YDBatchNo, YDBM.YDBatchDate, YBM.YDBookingMasterID, YBM.BuyerID, YDBM.Remarks, YDBM.IsApproved,
					        YBM.YDBookingNo,YBM.YDBookingDate,FM.ConceptNo,YBM.ConceptID,TotalBookingQty = ISNULL(YDBC.TotalBookingQty,0),
					        ProducedQty = ISNULL(YDRC.ProducedQty,0), Qty= SUM(ISNULL(HWC.HardWindingQty,0)),
							HWM.IsSendForApprove,HWM.SendForApproveDate,HWM.IsReject, RejectName = R.Name,
							HWM.RejectDate,HWM.IsApprove,HWM.ApproveDate
                            FROM YRM HWM
							INNER JOIN HWC ON HWC.HardWindingMasterID=HWM.HardWindingMasterID
					        INNER JOIN {TableNames.YD_BATCH_MASTER} YDBM ON YDBM.YDBatchID = HWM.YDBatchID
					        INNER JOIN {TableNames.YD_BOOKING_MASTER} YBM ON YBM.YDBookingMasterID = YDBM.YDBookingMasterID
					        INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FM ON FM.ConceptID = YBM.ConceptID
					        INNER JOIN YDBC ON YDBC.YDBookingMasterID = YBM.YDBookingMasterID 
					        INNER JOIN YDRC ON YDRC.YDBatchID = YDBM.YDBatchID
					        INNER JOIN {TableNames.YD_DRYER_FINISHING_MASTER} YDDFM ON YDDFM.YDBatchID = YDBM.YDBatchID
							Left join  {DbNames.EPYSL}..Contacts C ON c.ContactID = YDBM.BuyerID 
							LEFT Join  {DbNames.EPYSL}..LoginUser RL On RL.UserCode = HWM.SendForApproveBy 
							LEFT Join  {DbNames.EPYSL}..LoginUser A On A.UserCode = HWM.ApproveBy
							LEFT Join  {DbNames.EPYSL}..LoginUser R On R.UserCode = HWM.RejectBy 
					        WHERE ISNULL(YDBM.IsApproved,0) = 1
					        GROUP BY HWM.HardWindingMasterID,HWM.HardWindingNo,YDBM.YDBatchID, YDBM.YDBatchNo, YDBM.YDBatchDate, YBM.YDBookingMasterID, YBM.BuyerID, YDBM.Remarks, YDBM.IsApproved,
					        YBM.YDBookingNo,YBM.YDBookingDate,FM.ConceptNo,YBM.ConceptID,YDBC.TotalBookingQty,YDRC.ProducedQty,HWC.HardWindingQty,
							HWM.IsSendForApprove,HWM.SendForApproveDate,HWM.IsReject, R.Name,HWM.RejectDate,HWM.IsApprove,HWM.ApproveDate
                        )
                    SELECT M.HardWindingMasterID,M.HardWindingNo,M.YDBatchID, M.YDBatchNo, M.YDBatchDate, M.YDBookingMasterID, M.BuyerID, M.Remarks, M.IsApproved,
                    BM.YDBookingNo,BM.YDBNo, BM.YDBookingDate, CTO.Name Buyer,M.ConceptNo,M.ConceptID,M.Remarks,M.TotalBookingQty,M.ProducedQty,M.Qty,
					M.IsSendForApprove,M.SendForApproveDate,M.IsReject, M.RejectName,M.RejectDate,M.IsApprove,M.ApproveDate,
				    Count(*) Over() TotalRows
                    FROM M
                    INNER JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = M.BuyerID
                    INNER JOIN {TableNames.YD_BOOKING_MASTER} BM ON BM.YDBookingMasterID = M.YDBookingMasterID";

                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By M.HardWindingMasterID Desc" : paginationInfo.OrderBy;
            }
            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<HardWindingMaster>(sql);
        }
        public async Task<HardWindingMaster> GetNewAsync(int YDBookingMasterID, int YDBatchID)
        {
            var sql =
                $@"
                SELECT DISTINCT YDBM.YDBatchID,YDBatchNo,YDBM.YDBookingMasterID, YBM.YDBookingDate, YDBM.BuyerID, C.ShortName BuyerName, FCM.CompanyID As ReqFromID, CE.ShortName As Company
                FROM {TableNames.YD_BATCH_MASTER} YDBM
				INNER JOIN {TableNames.YD_BOOKING_MASTER} YBM ON YBM.YDBookingMasterID = YDBM.YDBookingMasterID
				LEFT JOIN {DbNames.EPYSL}..Contacts C ON YDBM.BuyerID=C.ContactID
                Inner JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM on FCM.ConceptID = YDBM.ConceptID 
				Inner Join {DbNames.EPYSL}..CompanyEntity CE On CE.CompanyID = FCM.CompanyID
                WHERE --YDBM.YDBookingMasterID = 2923 AND 
				YDBM.YDBatchID = {YDBatchID};
 
                -- Child
                ;With 
                YDBC As (
	                Select C.* FROM {TableNames.YDBookingChild} C
					INNER JOIN {TableNames.YD_BATCH_ITEM_REQUIREMENT} R ON C.YDBookingChildID=R.YDBookingChildID
					Where R.YDBatchID = {YDBatchID}
                ),
				IR As (
	                Select R.* FROM {TableNames.YD_BATCH_ITEM_REQUIREMENT} R 
					INNER JOIN {TableNames.YDBookingChild} C ON C.YDBookingChildID=R.YDBookingChildID
					Where --C.YDBookingMasterID = 2923 AND 
					R.YDBatchID = {YDBatchID}
                )
				--DF As (
				--             Select M.YDBatchID,M.YDBookingMasterID,C.* From YDDryerFinishingChild C 
				--	INNER JOIN {TableNames.YD_DRYER_FINISHING_MASTER} M ON M.YDDryerFinishingMasterID=C.YDDryerFinishingMasterID
				--	Where --C.YDBookingMasterID = 2923 AND 
				--	M.YDBatchID = {YDBatchID}
				--            )

                SELECT YDBC.YDBookingChildID,IR.YDBatchID, YDBC.YDBookingMasterID, YDBC.ItemMasterID, YDBC.ShadeCode, YDBC.NoOfThread, YDBC.ColorId As ColorID, 
                Color.SegmentValue AS ColorName, YDBC.BookingFor, YDF.YDyeingFor As BookingForName,YDBC.BookingQty, YDBC.UnitId As UnitID, DF.YDDryerFinishingMasterID,
                'Kg' As DisplayUnitDesc, YDBC.Remarks,IR.YDBItemReqID, 
                IM.Segment1ValueID Segment1ValueId, IM.Segment2ValueID Segment2ValueId, IM.Segment3ValueID Segment3ValueId, IM.Segment4ValueID Segment4ValueId,
                IM.Segment5ValueID Segment5ValueId, IM.Segment6ValueID Segment6ValueId, IM.Segment7ValueID Segment7ValueId, IM.Segment8ValueID Segment8ValueId,
                IM.Segment9ValueID Segment9ValueId, IM.Segment10ValueID Segment10ValueId, IM.Segment11ValueID Segment11ValueId, IM.Segment12ValueID Segment12ValueId,
                IM.Segment13ValueID Segment13ValueId, IM.Segment14ValueID Segment14ValueId, IM.Segment15ValueID Segment15ValueId,
                ISV1.SegmentValue AS Segment1ValueDesc, ISV2.SegmentValue AS Segment2ValueDesc, ISV3.SegmentValue AS Segment3ValueDesc,
                ISV4.SegmentValue AS Segment4ValueDesc, ISV5.SegmentValue AS Segment5ValueDesc, ISV6.SegmentValue AS Segment6ValueDesc,
                ISV7.SegmentValue AS Segment7ValueDesc, ISV8.SegmentValue AS Segment8ValueDesc, YDBC.YarnCategory,  
				BacthQty = SUM(ISNULL(IR.Qty,0)), 
				--BatchCone = Convert(int,SUM(ISNULL(
				--	(CASE WHEN YDBC.NoOfCone=0 
				--	THEN IR.Qty/1.5 
				--	ELSE 
				--	CASE WHEN YDBC.BookingQty=0 THEN 0 ELSE (IR.Qty/YDBC.BookingQty)*YDBC.NoOfCone END END),0)
				--)),
				BatchCone = CASE WHEN YDBC.PerConeKG = 0 THEN 0 ELSE Round(SUM(IR.Qty/ (Case When ISNULL(YDBC.PerConeKG,0) = 0 Then 1.5 Else  YDBC.PerConeKG END)),0) END,
				DryerFinishQty = SUM(ISNULL(DF.Qty,0)),
				DryerFinishCone = SUM(ISNULL(DF.Cone,0)),
				ReceiveCarton = 0,
                DF.YDDryerFinishingChildID,DF.YDRICRBId  
                FROM YDBC
				INNER JOIN IR ON IR.YDBookingChildID = YDBC.YDBookingChildID
				--INNER JOIN YDBC ON YDBC.YDBookingChildID=IR.YDBookingChildID
				INNER JOIN {TableNames.YD_DRYER_FINISHING_CHILD} DF ON DF.YDBItemReqID=IR.YDBItemReqID
				--INNER JOIN {TableNames.YD_REQ_ISSUE_CHILD} YDRIC ON YDRIC.YDReqChildID = YDRQC.YDReqChildID
				--INNER JOIN {TableNames.YD_RECEIVE_CHILD} YDRVC ON YDRVC.YDReqIssueChildID = YDRIC.YDReqIssueChildID
                --LEFT JOIN {TableNames.DyeingProcessPart_HK} DP ON DP.DPID = YDBC.DPID
                INNER JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = YDBC.ColorId
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YDBC.ItemMasterID

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
				GROUP BY YDBC.YDBookingChildID,IR.YDBatchID, YDBC.YDBookingMasterID, YDBC.ItemMasterID, YDBC.ShadeCode, YDBC.NoOfThread, YDBC.ColorId, 
                Color.SegmentValue, YDBC.BookingFor, YDF.YDyeingFor, YDBC.BookingQty, YDBC.UnitId,DF.YDDryerFinishingMasterID, 
                YDBC.Remarks,IR.YDBItemReqID,IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, IM.Segment4ValueID,
                IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID, IM.Segment8ValueID, IM.Segment9ValueID, IM.Segment10ValueID, IM.Segment11ValueID, 
				IM.Segment12ValueID, IM.Segment13ValueID, IM.Segment14ValueID, IM.Segment15ValueID, ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue,
                ISV4.SegmentValue, ISV5.SegmentValue, ISV6.SegmentValue, ISV7.SegmentValue, ISV8.SegmentValue, YDBC.YarnCategory,YDBC.PerConeKG,DF.YDDryerFinishingChildID,DF.YDRICRBId;

                ";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                HardWindingMaster data = await records.ReadFirstOrDefaultAsync<HardWindingMaster>();
                data.Childs = records.Read<HardWindingChild>().ToList();
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
        public async Task<HardWindingMaster> GetAsync(int id)
        {
            var segmentNames = new
            {
                SegmentNames = new string[]
               {
                    ItemSegmentNameConstants.FABRIC_COLOR
               }
            };

            var query = $@"---- Master ----
                            Select YRM.HardWindingMasterID, YRM.YDBatchID,YRM.YDBookingMasterID, YRM.HardWindingNo, YRM.HardWindingDate, C.ShortName, YRM.Remarks, FCM.CompanyID As ReqFromID, CE.ShortName As Company 
                            FROM {TableNames.HardWindingMaster} YRM 
                            Inner JOIN {TableNames.YD_BOOKING_MASTER} YDBM ON YDBM.YDBookingMasterID = YRM.YDBookingMasterID  
                            Inner JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM on FCM.ConceptID = YDBM.ConceptID 
                            Left join {DbNames.EPYSL}..Contacts C ON c.ContactID = FCM.CompanyID 
                            Inner Join {DbNames.EPYSL}..CompanyEntity CE On CE.CompanyID = FCM.CompanyID
                            Where YRM.HardWindingMasterID = {id};

                            ---- Child ----
                            SELECT C.HardWindingChildID,C.YDBItemReqID,C.YDBookingChildID,C.HardWindingMasterID,C.ItemMasterID,C.ColorID,M.Remarks,
                            C.Qty,C.Cone,C.YarnCategory,ColorName = Color.SegmentValue,
                            IM.Segment1ValueID Segment1ValueId, IM.Segment2ValueID Segment2ValueId, IM.Segment3ValueID Segment3ValueId, IM.Segment4ValueID Segment4ValueId,
                            IM.Segment5ValueID Segment5ValueId, IM.Segment6ValueID Segment6ValueId, IM.Segment7ValueID Segment7ValueId, IM.Segment8ValueID Segment8ValueId,
                            IM.Segment9ValueID Segment9ValueId, IM.Segment10ValueID Segment10ValueId, IM.Segment11ValueID Segment11ValueId, IM.Segment12ValueID Segment12ValueId,
                            IM.Segment13ValueID Segment13ValueId, IM.Segment14ValueID Segment14ValueId, IM.Segment15ValueID Segment15ValueId,
                            Segment1ValueDesc = ISV1.SegmentValue, Segment2ValueDesc = ISV2.SegmentValue, Segment3ValueDesc = ISV3.SegmentValue,
                            Segment4ValueDesc = ISV4.SegmentValue, Segment5ValueDesc = ISV5.SegmentValue, Segment6ValueDesc = ISV6.SegmentValue,
                            Segment7ValueDesc = ISV7.SegmentValue, Segment8ValueDesc = ISV8.SegmentValue,
                            BacthQty = SUM(ISNULL(IR.Qty,0)),
                            --BatchCone = CASE WHEN YDBC.PerConeKG = 0 THEN 0 ELSE Round(SUM(IR.Qty/YDBC.PerConeKG),0) END,
                            BatchCone = CASE WHEN YDBC.PerConeKG = 0 THEN 0 ELSE Round(SUM(IR.Qty/ (Case When ISNULL(YDBC.PerConeKG,0) = 0 Then 1.5 Else  YDBC.PerConeKG END)),0) END,
                            DryerFinishQty = SUM(ISNULL(DF.Qty,0)),
                            DryerFinishCone = SUM(ISNULL(DF.Cone,0)),ReceiveCarton = 0,
                            DF.YDDryerFinishingChildID, DF.YDRICRBId
                            FROM {TableNames.HardWindingChild} C 
                            INNER JOIN {TableNames.HardWindingMaster} M ON C.HardWindingMasterID=M.HardWindingMasterID
                            INNER JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = C.ColorId
                            INNER JOIN {TableNames.YDBookingChild} YDBC ON YDBC.YDBookingChildID = C.YDBookingChildID
                            INNER JOIN {TableNames.YD_BOOKING_MASTER} YDBM ON YDBM.YDBookingMasterID = YDBC.YDBookingMasterID
                            INNER JOIN {TableNames.YD_BATCH_ITEM_REQUIREMENT} IR ON IR.YDBItemReqID=C.YDBItemReqID
                            INNER JOIN {TableNames.YD_DRYER_FINISHING_CHILD} DF ON DF.YDBItemReqID=IR.YDBItemReqID
                            INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = C.ItemMasterID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV8 ON ISV8.SegmentValueID = IM.Segment8ValueID
                            WHERE C.HardWindingMasterID = {id}
                            GROUP BY C.HardWindingChildID,C.YDBItemReqID,C.YDBookingChildID,C.HardWindingMasterID,C.ItemMasterID,C.ColorID,M.Remarks,
                            C.Qty,C.Cone,C.YarnCategory,Color.SegmentValue,YDBC.PerConeKG,IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, 
                            IM.Segment4ValueID, IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID, IM.Segment8ValueID,IM.Segment9ValueID, 
                            IM.Segment10ValueID, IM.Segment11ValueID, IM.Segment12ValueID,IM.Segment13ValueID, IM.Segment14ValueID, IM.Segment15ValueID,
                            ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue,ISV4.SegmentValue, ISV5.SegmentValue, ISV6.SegmentValue,
                            ISV7.SegmentValue, ISV8.SegmentValue, DF.YDDryerFinishingChildID, DF.YDRICRBId    

                            ";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query, segmentNames);

                HardWindingMaster data = records.Read<HardWindingMaster>().FirstOrDefault();
                data.Childs = records.Read<HardWindingChild>().ToList();

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
        public async Task<HardWindingMaster> GetAllAsync(int id)
        {
            var sql = $@"
            ;Select * FROM {TableNames.HardWindingMaster} Where HardWindingMasterID = {id}

            ;Select * FROM {TableNames.HardWindingChild} Where HardWindingMasterID = {id}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                HardWindingMaster data = await records.ReadFirstOrDefaultAsync<HardWindingMaster>();
                Guard.Against.NullObject(data);
                data.Childs = records.Read<HardWindingChild>().ToList();
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

        public async Task SaveAsync(HardWindingMaster entity)
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
                //List<HardWindingChild> childRecords = entity.Childs;
                //_itemMasterRepository.GenerateItem(AppConstants.ITEM_SUB_GROUP_YARN_NEW, ref childRecords);

                switch (entity.EntityState)
                {
                    case EntityState.Added:
                        entity.HardWindingMasterID = await _service.GetMaxIdAsync(TableNames.HardWindingMaster, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        entity.HardWindingNo = await _service.GetMaxNoAsync(TableNames.HardWindingNo, 1, RepeatAfterEnum.NoRepeat, "00000", transactionGmt, _connectionGmt);

                        maxChildId = await _service.GetMaxIdAsync(TableNames.HardWindingChild, entity.Childs.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        foreach (var item in entity.Childs)
                        {
                            item.HardWindingChildID = maxChildId++;
                            item.HardWindingMasterID = entity.HardWindingMasterID;
                        }
                        break;

                    case EntityState.Modified:
                        var addedChilds = entity.Childs.FindAll(x => x.EntityState == EntityState.Added);
                        maxChildId = await _service.GetMaxIdAsync(TableNames.YARN_YD_REQ_CHILD, addedChilds.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

                        foreach (var item in addedChilds)
                        {
                            item.HardWindingChildID = maxChildId++;
                            item.HardWindingMasterID = entity.HardWindingMasterID;
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
        public async Task UpdateEntityAsync(HardWindingMaster entity)
        {
            SqlTransaction transaction = null;
            SqlTransaction transactionGmt = null;
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                await _connectionGmt.OpenAsync();
                transactionGmt = _connectionGmt.BeginTransaction();

                await _service.SaveSingleAsync(entity, transaction);

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
