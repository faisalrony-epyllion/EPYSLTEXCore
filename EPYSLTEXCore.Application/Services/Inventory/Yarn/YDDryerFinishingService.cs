using Dapper;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Application.Interfaces.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.Data.SqlClient;
using System.Data.Entity;

namespace EPYSLTEXCore.Application.Services.Inventory.Yarn
{
    public class YDDryerFinishingService: IYDDryerFinishingService
    {
        private readonly IDapperCRUDService<YDDryerFinishingMaster> _service;
        private readonly SqlConnection _connection;
        private readonly SqlConnection _connectionGmt;

        public YDDryerFinishingService(IDapperCRUDService<YDDryerFinishingMaster> service)
        {
            _service = service;
            _service.Connection = service.GetConnection(AppConstants.GMT_CONNECTION);
            _connectionGmt = service.Connection;

            _service.Connection = service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _connection = service.Connection;
        }

        public async Task<List<YDDryerFinishingMaster>> GetPagedAsync(Status status, string pageName, PaginationInfo paginationInfo)
        {
            string orderBy = "";
            int IsSendForApprove = -1, IsApprove = -1, IsAcknowledge = -1, IsReject = -1;

            if (pageName == "YDDryerFinishing")
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
            else if (pageName == "YDDryerFinishingApproval")
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
                $@";With YDBC AS 
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
			   A AS 
                ( 
                    Select YBM.YDBookingMasterID, YBM.YDBookingNo, YBM.YDBookingDate, YDBM.YDBatchID, YDBM.YDBatchNo, YDBM.YDBatchDate, BatchQty = SUM(IR.Qty), YBM.Remarks, 
                    TotalBookingQty = ISNULL(YDBC.TotalBookingQty,0), ProducedQty = ISNULL(YDRC.ProducedQty,0), Qty= SUM(ISNULL(FC.Qty,0))
                    FROM {TableNames.YD_BATCH_MASTER} YDBM
					INNER JOIN {TableNames.YD_BATCH_ITEM_REQUIREMENT} IR ON IR.YDBatchID = YDBM.YDBatchID
					INNER JOIN {TableNames.YD_BOOKING_MASTER} YBM ON YBM.YDBookingMasterID = YDBM.YDBookingMasterID
					INNER JOIN YDBC ON YDBC.YDBookingMasterID = YBM.YDBookingMasterID 
					--INNER JOIN {TableNames.YD_DYEING_BATCH_ITEM} BI ON BI.YDBItemReqID = YDBC.BItemReqID
					--INNER JOIN {TableNames.YD_PRODUCTION_MASTER} YDPM ON YDPM.YDBookingMasterID = YBM.YDBookingMasterID 
					INNER JOIN YDRC ON YDRC.YDBatchID = YDBM.YDBatchID
					Left JOIN {TableNames.YD_DRYER_FINISHING_CHILD} FC ON FC.YDBItemReqID = IR.YDBItemReqID
                    Left JOIN {TableNames.YD_DRYER_FINISHING_MASTER} RM ON RM.YDDryerFinishingMasterID = FC.YDDryerFinishingMasterID
                    where YBM.IsAcknowledge = 1 --AND RM.YDBookingMasterID IS NULL
					GROUP BY YBM.YDBookingMasterID, YBM.YDBookingNo, YBM.YDBookingDate, YDBM.YDBatchID, YDBM.YDBatchNo, YDBM.YDBatchDate, YBM.Remarks, 
                    YDBC.TotalBookingQty, YDRC.ProducedQty
                ),
				B As 
				(
				Select * FROM A Where ISNULL(ProducedQty,0) > ISNULL(Qty,0)
				)
                Select *, Count(*) Over() TotalRows From B";

                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By YDBatchID Desc" : paginationInfo.OrderBy;
            }
            else
            {
                sql = $@"
                ;With
                YRM As 
                (
	                Select *
	                FROM {TableNames.YD_DRYER_FINISHING_MASTER} 
	                Where IsSendForApprove = (Case when {IsSendForApprove} = -1 Then IsSendForApprove Else {IsSendForApprove} End)
	                AND IsApprove = (Case when {IsApprove} = -1 Then IsApprove Else {IsApprove} End) 
	                AND IsReject = (Case when {IsReject} = -1 Then IsReject Else {IsReject} End) 
                )
                Select YRM.YDDryerFinishingMasterID, YRM.YDBookingMasterID, YRM.YDDryerFinishingNo, YRM.YDDryerFinishingDate, BM.YDBatchID, BM.YDBatchNo, BM.YDBatchDate, 
				YDBM.BuyerID, C.ShortName AS BuyerName, YRM.Remarks, 
                YRM.IsSendForApprove, RL.Name As SendForApproveName,  YRM.SendForApproveDate, YRM.IsApprove, A.Name As ApproveName, YRM.ApproveDate, 
                YRM.IsReject, R.Name As RejectName, YRM.RejectDate, YRM.AddedBy,
                Qty = Sum(YRC.Qty), Cone = SUM(YRC.Cone), ProducedQty = SUM(PC.ProducedQty)
                From YRM
                Inner JOIN {TableNames.YD_DRYER_FINISHING_CHILD} YRC ON YRC.YDDryerFinishingMasterID = YRM.YDDryerFinishingMasterID 
				INNER JOIN {TableNames.YD_BATCH_ITEM_REQUIREMENT} IR ON IR.YDBItemReqID = YRC.YDBItemReqID
				INNER JOIN {TableNames.YD_BATCH_MASTER} BM ON BM.YDBatchID = IR.YDBatchID
				INNER JOIN {TableNames.YD_DYEING_BATCH_ITEM} BI ON BI.YDBItemReqID = YRC.YDBItemReqID
				INNER JOIN {TableNames.YD_PRODUCTION_CHILD} PC ON PC.YDDBIID = BI.YDDBIID
				Inner JOIN {TableNames.YD_BOOKING_MASTER} YDBM ON YDBM.YDBookingMasterID = YRM.YDBookingMasterID 
                Left join  {DbNames.EPYSL}..Contacts C ON c.ContactID = YDBM.BuyerID 
                LEFT Join  {DbNames.EPYSL}..LoginUser RL On RL.UserCode = YRM.SendForApproveBy 
                LEFT Join  {DbNames.EPYSL}..LoginUser A On A.UserCode = YRM.ApproveBy
                LEFT Join  {DbNames.EPYSL}..LoginUser R On R.UserCode = YRM.RejectBy 
                Group By YRM.YDDryerFinishingMasterID, YRM.YDBookingMasterID, YRM.YDDryerFinishingNo, YRM.YDDryerFinishingDate, BM.YDBatchID, BM.YDBatchNo, BM.YDBatchDate, YDBM.BuyerID, C.ShortName, YRM.Remarks, 
                YRM.IsSendForApprove, RL.Name, YRM.SendForApproveDate, YRM.IsApprove, A.Name, YRM.ApproveDate, 
                YRM.IsReject, R.Name, YRM.RejectDate, YRM.AddedBy";

                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By YRM.YDDryerFinishingMasterID Desc" : paginationInfo.OrderBy;
            }
            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<YDDryerFinishingMaster>(sql);
        }
        public async Task<YDDryerFinishingMaster> GetNewAsync(int YDBatchID)
        {
            var sql =
                $@"
                SELECT DISTINCT YDBM.YDBookingMasterID, YDBM.YDBookingDate, YDBM.YDBookingNo, BM.YDBatchID, BM.YDBatchNo, BM.YDBatchDate, YDBM.BuyerID, C.ShortName BuyerName, FCM.CompanyID As ReqFromID, CE.ShortName As Company
                FROM {TableNames.YD_BATCH_MASTER} BM 
				LEFT join {TableNames.YD_BOOKING_MASTER} YDBM ON YDBM.YDBookingMasterID = BM.YDBookingMasterID
				LEFT JOIN {DbNames.EPYSL}..Contacts C ON YDBM.BuyerID=C.ContactID
                LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM on FCM.ConceptID = YDBM.ConceptID 
				LEFT JOIN {DbNames.EPYSL}..CompanyEntity CE On CE.CompanyID = FCM.CompanyID
                WHERE BM.YDBatchID = {YDBatchID};

                -- Child
                ;With 
                IR As (
	                Select * FROM {TableNames.YD_BATCH_ITEM_REQUIREMENT} Where YDBatchID = {YDBatchID}
                )

                SELECT YDBC.YDBookingChildID, YDBC.YDBookingMasterID, IR.YDBItemReqID, YDBC.ItemMasterID, YDBC.ShadeCode, YDBC.NoOfThread, YDBC.ColorId As ColorID, 
                Color.SegmentValue AS ColorName, YDBC.BookingFor, YDF.YDyeingFor As BookingForName,YDBC.BookingQty, YDBC.UnitId As UnitID, 
                'Kg' As DisplayUnitDesc, YDBC.Remarks, 
                IM.Segment1ValueID Segment1ValueId, IM.Segment2ValueID Segment2ValueId, IM.Segment3ValueID Segment3ValueId, IM.Segment4ValueID Segment4ValueId,
                IM.Segment5ValueID Segment5ValueId, IM.Segment6ValueID Segment6ValueId, IM.Segment7ValueID Segment7ValueId, IM.Segment8ValueID Segment8ValueId,
                IM.Segment9ValueID Segment9ValueId, IM.Segment10ValueID Segment10ValueId, IM.Segment11ValueID Segment11ValueId, IM.Segment12ValueID Segment12ValueId,
                IM.Segment13ValueID Segment13ValueId, IM.Segment14ValueID Segment14ValueId, IM.Segment15ValueID Segment15ValueId,
                ISV1.SegmentValue AS Segment1ValueDesc, ISV2.SegmentValue AS Segment2ValueDesc, ISV3.SegmentValue AS Segment3ValueDesc,
                ISV4.SegmentValue AS Segment4ValueDesc, ISV5.SegmentValue AS Segment5ValueDesc, ISV6.SegmentValue AS Segment6ValueDesc,
                ISV7.SegmentValue AS Segment7ValueDesc, ISV8.SegmentValue AS Segment8ValueDesc, YDBC.YarnCategory,  
				ProducedQty = SUM(ISNULL(PC.ProducedQty,0)), ProducedCone = SUM(ISNULL(PC.ProducedCone,0)), PC.YDProductionChildID, PC.YDRICRBId
                FROM IR 
				INNER join {TableNames.YD_DYEING_BATCH_ITEM} BI ON BI.YDBItemReqID = IR.YDBItemReqID
				INNER JOIN {TableNames.YD_PRODUCTION_CHILD} PC ON PC.YDDBIID = BI.YDDBIID
				INNER JOIN {TableNames.YDBookingChild} YDBC ON YDBC.YDBookingChildID = IR.YDBookingChildID
                LEFT JOIN {TableNames.DyeingProcessPart_HK} DP ON DP.DPID = YDBC.DPID
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
				GROUP BY YDBC.YDBookingChildID, YDBC.YDBookingMasterID, IR.YDBItemReqID, YDBC.ItemMasterID, YDBC.ShadeCode, YDBC.NoOfThread, YDBC.ColorId, 
                Color.SegmentValue, YDBC.BookingFor, YDF.YDyeingFor, YDBC.BookingQty, YDBC.UnitId, 
                YDBC.Remarks, IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, IM.Segment4ValueID,
                IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID, IM.Segment8ValueID, IM.Segment9ValueID, IM.Segment10ValueID, IM.Segment11ValueID, 
				IM.Segment12ValueID, IM.Segment13ValueID, IM.Segment14ValueID, IM.Segment15ValueID, ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue,
                ISV4.SegmentValue, ISV5.SegmentValue, ISV6.SegmentValue, ISV7.SegmentValue, ISV8.SegmentValue, YDBC.YarnCategory, PC.YDProductionChildID, PC.YDRICRBId;

";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YDDryerFinishingMaster data = await records.ReadFirstOrDefaultAsync<YDDryerFinishingMaster>();
                data.Childs = records.Read<YDDryerFinishingChild>().ToList();
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
        public async Task<YDDryerFinishingMaster> GetAsync(int id)
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
                Select YRM.YDDryerFinishingMasterID, YRM.YDBookingMasterID, YRM.YDDryerFinishingNo, YRM.YDDryerFinishingDate, YRM.YDBatchID, BM.YDBatchNo, C.ShortName, YRM.Remarks, FCM.CompanyID As ReqFromID, CE.ShortName As Company 
                FROM {TableNames.YD_DRYER_FINISHING_MASTER} YRM 
				INNER JOIN {TableNames.YD_BATCH_MASTER} BM ON BM.YDBatchID = YRM.YDBatchID
				Inner join {TableNames.YD_BOOKING_MASTER} YDBM ON YDBM.YDBookingMasterID = YRM.YDBookingMasterID  
				Inner JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM on FCM.ConceptID = YDBM.ConceptID 
                Left join  {DbNames.EPYSL}..Contacts C ON c.ContactID = FCM.CompanyID 
				Inner Join {DbNames.EPYSL}..CompanyEntity CE On CE.CompanyID = FCM.CompanyID
                Where YRM.YDDryerFinishingMasterID = {id};

                ;With
                YRC As 
                (
	                Select * FROM {TableNames.YD_DRYER_FINISHING_CHILD} Where YDDryerFinishingMasterID = {id}
                )
                Select IR.YDBItemReqID, YRC.YDDryerFinishingChildID, IR.YDBookingChildID, YRC.YDDryerFinishingMasterID, YRC.ItemMasterID, YRC.ColorID, YRC.Remarks,
                YRC.Qty, YRC.Cone, YRC.YarnCategory, Color.SegmentValue AS ColorName,
                IM.Segment1ValueID Segment1ValueId, IM.Segment2ValueID Segment2ValueId, IM.Segment3ValueID Segment3ValueId, IM.Segment4ValueID Segment4ValueId,
                IM.Segment5ValueID Segment5ValueId, IM.Segment6ValueID Segment6ValueId, IM.Segment7ValueID Segment7ValueId, IM.Segment8ValueID Segment8ValueId,
                IM.Segment9ValueID Segment9ValueId, IM.Segment10ValueID Segment10ValueId, IM.Segment11ValueID Segment11ValueId, IM.Segment12ValueID Segment12ValueId,
                IM.Segment13ValueID Segment13ValueId, IM.Segment14ValueID Segment14ValueId, IM.Segment15ValueID Segment15ValueId,
                ISV1.SegmentValue AS Segment1ValueDesc, ISV2.SegmentValue AS Segment2ValueDesc, ISV3.SegmentValue AS Segment3ValueDesc,
                ISV4.SegmentValue AS Segment4ValueDesc, ISV5.SegmentValue AS Segment5ValueDesc, ISV6.SegmentValue AS Segment6ValueDesc,
                ISV7.SegmentValue AS Segment7ValueDesc, ISV8.SegmentValue AS Segment8ValueDesc,  
				ProducedQty = SUM(ISNULL(PC.ProducedQty,0)), ProducedCone = SUM(ISNULL(PC.ProducedCone,0))
                From YRC 
				INNER JOIN {TableNames.YD_BATCH_ITEM_REQUIREMENT} IR ON IR.YDBItemReqID = YRC.YDBItemReqID
				INNER join {TableNames.YD_DYEING_BATCH_ITEM} BI ON BI.YDBItemReqID = IR.YDBItemReqID
				INNER JOIN {TableNames.YD_PRODUCTION_CHILD} PC ON PC.YDDBIID = BI.YDDBIID
				INNER JOIN {TableNames.YDBookingChild} YDBC ON YDBC.YDBookingChildID = IR.YDBookingChildID
				LEFT JOIN {DbNames.EPYSL}..Unit U ON U.UnitID = YDBC.UnitID 
                INNER JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = YDBC.ColorId
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YDBC.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV8 ON ISV8.SegmentValueID = IM.Segment8ValueID
                Group By IR.YDBItemReqID, YRC.YDDryerFinishingChildID, IR.YDBookingChildID, YRC.YDDryerFinishingMasterID, YRC.ItemMasterID, YRC.ColorID, YRC.Remarks,
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

                YDDryerFinishingMaster data = records.Read<YDDryerFinishingMaster>().FirstOrDefault();
                data.Childs = records.Read<YDDryerFinishingChild>().ToList();

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
        public async Task<YDDryerFinishingMaster> GetAllAsync(int id)
        {
            var sql = $@"
            ;Select * FROM {TableNames.YD_DRYER_FINISHING_MASTER} Where YDDryerFinishingMasterID = {id}

            ;Select * FROM {TableNames.YD_DRYER_FINISHING_CHILD} Where YDDryerFinishingMasterID = {id}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YDDryerFinishingMaster data = await records.ReadFirstOrDefaultAsync<YDDryerFinishingMaster>();
                Guard.Against.NullObject(data);
                data.Childs = records.Read<YDDryerFinishingChild>().ToList();
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

        public async Task SaveAsync(YDDryerFinishingMaster entity)
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
                //List<YDDryerFinishingChild> childRecords = entity.Childs;
                //_itemMasterRepository.GenerateItem(AppConstants.ITEM_SUB_GROUP_YARN_NEW, ref childRecords);

                switch (entity.EntityState)
                {
                    case EntityState.Added:
                        entity.YDDryerFinishingMasterID = await _service.GetMaxIdAsync(TableNames.YD_DRYER_FINISHING_MASTER,RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        entity.YDDryerFinishingNo =await _service.GetMaxNoAsync(TableNames.YD_DRYER_FINISHING_NO,1,RepeatAfterEnum.NoRepeat,"",transactionGmt,_connectionGmt);

                        maxChildId = await _service.GetMaxIdAsync(TableNames.YD_DRYER_FINISHING_CHILD, entity.Childs.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        foreach (var item in entity.Childs)
                        {
                            item.YDDryerFinishingChildID = maxChildId++;
                            item.YDDryerFinishingMasterID = entity.YDDryerFinishingMasterID;
                        }
                        break;

                    case EntityState.Modified:
                        var addedChilds = entity.Childs.FindAll(x => x.EntityState == EntityState.Added);
                        maxChildId = await _service.GetMaxIdAsync(TableNames.YD_DRYER_FINISHING_CHILD, addedChilds.Count);

                        foreach (var item in addedChilds)
                        {
                            item.YDDryerFinishingChildID = maxChildId++;
                            item.YDDryerFinishingMasterID = entity.YDDryerFinishingMasterID;
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
        public async Task UpdateEntityAsync(YDDryerFinishingMaster entity)
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
