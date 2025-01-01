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
    internal class SendToYDStoreService : ISendToYDStoreService
    {
        private readonly IDapperCRUDService<SendToYDStoreMaster> _service;
        private readonly SqlConnection _connection;
        private readonly SqlConnection _connectionGmt;

        public SendToYDStoreService(IDapperCRUDService<SendToYDStoreMaster> service)
        {
            _service = service;
            _service.Connection = service.GetConnection(AppConstants.GMT_CONNECTION);
            _connectionGmt = service.Connection;

            _service.Connection = service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _connection = service.Connection;
        }

        public async Task<List<SendToYDStoreMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By YDBatchDate Desc" : paginationInfo.OrderBy;
            var sql = string.Empty;
            if (status == Status.Pending)
            {
                sql += $@"
               ;WITH YBM AS 
			   (
				Select YDBM.YDBookingMasterID, YDBM.BuyerID, YDBM.YDBookingNo, YDBM.YDBookingDate, TotalBookingQty = SUM(YDBC.BookingQty) 
				FROM YDBookingChild YDBC
				INNER JOIN YDBookingMaster YDBM ON YDBM.YDBookingMasterID = YDBC.YDBookingMasterID
				GROUP BY YDBM.YDBookingMasterID, YDBM.YDBookingMasterID, YDBM.BuyerID, YDBM.YDBookingNo, YDBM.YDBookingDate
			   ),
			   YDQC AS 
			   (
			   Select IR.YDBatchID,QCQty = SUM(ISNULL(QCC.ProductionQty,0)), QCCone = SUM(ISNULL(QCC.ConeQty,0)), QCPacket = SUM(ISNULL(QCC.PacketQty,0))
			   From YDBatchItemRequirement IR
			   --INNER JOIN YDDyeingBatchItem BI ON BI.YDBItemReqID = IR.YDBItemReqID
			   --INNER JOIN YDProductionChild PC ON PC.YDDBIID = BI.YDDBIID
			   --INNER JOIN YDDryerFinishingChild DFC ON DFC.YDBItemReqID = IR.YDBItemReqID
			   --INNER JOIN HardWindingChild HWC ON HWC.YDBItemReqID = IR.YDBItemReqID
			   INNER JOIN YDQCChild QCC ON QCC.YDBItemReqID = IR.YDBItemReqID
			   GROUP BY IR.YDBatchID
			   ),
			   N AS (
                    SELECT	YDBM.YDBatchID, YDBM.YDBatchNo, YDBM.YDBatchDate, YBM.YDBookingMasterID, YBM.BuyerID, YDBM.Remarks, YDBM.IsApproved,
					YBM.YDBookingNo, YBM.YDBookingDate, YDQC.QCQty, SendQty = SUM(ISNULL(YDSC.SendQty,0))
                    FROM YDBatchMaster YDBM
					INNER JOIN YDBatchItemRequirement IR ON IR.YDBatchID = YDBM.YDBatchID
					INNER JOIN YBM ON YBM.YDBookingMasterID = YDBM.YDBookingMasterID
					INNER JOIN YDQC ON YDQC.YDBatchID = YDBM.YDBatchID
					LEFT JOIN SendToYDStoreChild YDSC ON YDSC.YDBItemReqID = IR.YDBItemReqID
					Where ISNULL(YDBM.IsApproved,0) = 1
					GROUP BY YDBM.YDBatchID, YDBM.YDBatchNo, YDBM.YDBatchDate, YBM.YDBookingMasterID, YBM.BuyerID, YDBM.Remarks, YDBM.IsApproved,
					YBM.YDBookingNo, YBM.YDBookingDate, YDQC.QCQty
                ),
				M AS 
				(
				Select * FROM N Where QCQty > SendQty
				)
                SELECT M.YDBatchID, M.YDBatchNo, M.YDBatchDate, M.YDBookingMasterID, M.BuyerID, M.Remarks, M.IsApproved,
                M.YDBookingNo, M.YDBookingDate, CTO.Name Buyer, M.QCQty, M.SendQty, Count(*) Over() TotalRows
                FROM M
                INNER JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = M.BuyerID";

                orderBy = string.IsNullOrEmpty(orderBy) ? "ORDER BY YDBatchDate DESC" : orderBy;
            }
            else if (status == Status.Draft)
            {
                sql += $@";WITH M AS (
                    SELECT	QM.YDBatchID, QM.SendToYDStoreMasterID, QM.SendToYDStoreNo, QM.SendToYDStoreDate, QM.YDBookingMasterID, QM.Remarks
                    FROM SendToYDStoreMaster QM
                    Where QM.IsSendForApprove = 0
                )
                SELECT M.SendToYDStoreMasterID, M.SendToYDStoreNo, M.SendToYDStoreDate, M.YDBookingMasterID, M.Remarks, BTM.YDBatchID, BTM.YDBatchNo, BTM.YDBatchDate,
                BM.YDBookingNo, BM.YDBookingDate, Count(*) Over() TotalRows
                FROM M
                INNER JOIN YDBookingMaster BM ON BM.YDBookingMasterID = M.YDBookingMasterID
                --INNER JOIN YDProductionMaster PM ON PM.YDProductionMasterID = M.YDProductionMasterID
				INNER JOIN YDBatchMaster BTM ON BTM.YDBatchID = M.YDBatchID ";
                orderBy = string.IsNullOrEmpty(orderBy) ? "ORDER BY SendToYDStoreDate DESC" : orderBy;
            }
            else if (status == Status.AwaitingPropose)
            {
                sql += $@";WITH M AS (
                    SELECT	QM.YDBatchID, QM.SendToYDStoreMasterID, QM.SendToYDStoreNo, QM.SendToYDStoreDate, QM.YDBookingMasterID, QM.Remarks
                    FROM SendToYDStoreMaster QM
                    Where QM.IsSendForApprove = 1 AND QM.IsApprove = 0 AND QM.IsReject = 0
                )
                SELECT M.SendToYDStoreMasterID, M.SendToYDStoreNo, M.SendToYDStoreDate, M.YDBookingMasterID, M.Remarks, BTM.YDBatchID, BTM.YDBatchNo, BTM.YDBatchDate,
                BM.YDBookingNo, BM.YDBookingDate, Count(*) Over() TotalRows
                FROM M
                INNER JOIN YDBookingMaster BM ON BM.YDBookingMasterID = M.YDBookingMasterID
                --INNER JOIN YDProductionMaster PM ON PM.YDProductionMasterID = M.YDProductionMasterID
				INNER JOIN YDBatchMaster BTM ON BTM.YDBatchID = M.YDBatchID ";

                orderBy = string.IsNullOrEmpty(orderBy) ? "ORDER BY SendToYDStoreDate DESC" : orderBy;
            }
            else if (status == Status.Approved)
            {
                sql += $@";WITH M AS (
                    SELECT	QM.YDBatchID, QM.SendToYDStoreMasterID, QM.SendToYDStoreNo, QM.SendToYDStoreDate, QM.YDBookingMasterID, QM.Remarks
                    FROM SendToYDStoreMaster QM
                    Where QM.IsSendForApprove = 1 AND QM.IsApprove = 1 AND QM.IsReject = 0
                )
                SELECT M.SendToYDStoreMasterID, M.SendToYDStoreNo, M.SendToYDStoreDate, M.YDBookingMasterID, M.Remarks, BTM.YDBatchID, BTM.YDBatchNo, BTM.YDBatchDate,
                BM.YDBookingNo, BM.YDBookingDate, Count(*) Over() TotalRows
                FROM M
                INNER JOIN YDBookingMaster BM ON BM.YDBookingMasterID = M.YDBookingMasterID
                --INNER JOIN YDProductionMaster PM ON PM.YDProductionMasterID = M.YDProductionMasterID
				INNER JOIN YDBatchMaster BTM ON BTM.YDBatchID = M.YDBatchID ";

                orderBy = string.IsNullOrEmpty(orderBy) ? "ORDER BY SendToYDStoreDate DESC" : orderBy;
            }
            else if (status == Status.Reject)
            {
                sql += $@";WITH M AS (
                    SELECT	QM.YDBatchID, QM.SendToYDStoreMasterID, QM.SendToYDStoreNo, QM.SendToYDStoreDate, QM.YDBookingMasterID, QM.Remarks
                    FROM SendToYDStoreMaster QM
                    Where QM.IsSendForApprove = 1 AND QM.IsApprove = 0 AND QM.IsReject = 1
                )
                SELECT M.SendToYDStoreMasterID, M.SendToYDStoreNo, M.SendToYDStoreDate, M.YDBookingMasterID, M.Remarks, BTM.YDBatchID, BTM.YDBatchNo, BTM.YDBatchDate,
                BM.YDBookingNo, BM.YDBookingDate, Count(*) Over() TotalRows
                FROM M
                INNER JOIN YDBookingMaster BM ON BM.YDBookingMasterID = M.YDBookingMasterID
                --INNER JOIN YDProductionMaster PM ON PM.YDProductionMasterID = M.YDProductionMasterID
				INNER JOIN YDBatchMaster BTM ON BTM.YDBatchID = M.YDBatchID ";

                orderBy = string.IsNullOrEmpty(orderBy) ? "ORDER BY SendToYDStoreDate DESC" : orderBy;
            }
            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<SendToYDStoreMaster>(sql);
        }

        public async Task<SendToYDStoreMaster> GetNewAsync(int newId)
        {
            var sql = $@" ;WITH M AS (
                        SELECT	PM.YDBatchID, PM.YDBatchNo, PM.YDBatchDate, PM.YDBookingMasterID, PM.BuyerID, PM.Remarks, PM.IsApproved
                        FROM YDBatchMaster PM WHERE PM.YDBatchID = {newId}
                    )
                    SELECT M.YDBatchID, M.YDBatchNo, M.YDBatchDate, M.YDBookingMasterID, M.BuyerID,
                    BM.YDBookingNo, BM.YDBookingDate, CTO.Name Buyer
                    FROM M
                    INNER JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = M.BuyerID
                    INNER JOIN YDBookingMaster BM ON BM.YDBookingMasterID = M.YDBookingMasterID;

                      -----childs
                    ;WITH X AS (
                        SELECT * FROM YDBatchItemRequirement PC WHERE PC.YDBatchID = {newId}
                    ),
					 YDQC AS 
					(
					Select X.YDBItemReqID, QCC.YDQCChildID, QCC.YDRICRBId, QCQty = SUM(ISNULL(QCC.ProductionQty,0)), QCCone = SUM(ISNULL(QCC.ConeQty,0)), QCPacket = SUM(ISNULL(QCC.PacketQty,0))
					From X
					INNER JOIN YDQCChild QCC ON QCC.YDBItemReqID = X.YDBItemReqID
					GROUP BY X.YDBItemReqID, QCC.YDQCChildID, QCC.YDRICRBId
					),
					 SYD AS 
					(
					Select X.YDBItemReqID, SendQty = SUM(ISNULL(YDSC.SendQty,0)), SendConeQty = SUM(ISNULL(YDSC.SendConeQty,0)), SendPacketQty = SUM(ISNULL(YDSC.SendPacketQty,0))
					From X
					INNER JOIN SendToYDStoreChild YDSC ON YDSC.YDBItemReqID = X.YDBItemReqID
					GROUP BY X.YDBItemReqID
					),
					FL AS
					(
                    SELECT X.YDBItemReqID, X.YDBatchID, YDBC.YDBookingChildID, YDBC.YDBookingMasterID, YDBC.ItemMasterID, YDBC.UnitID, YDBC.ShadeCode, YDBC.NoOfThread, 
						YDBC.ColorId As ColorID, YDBC.BookingQty,
	                    YDBC.YarnCategory, QCQty = SUM(ISNULL(YDQC.ProductionQty,0)), QCCone = SUM(ISNULL(YDQC.ConeQty,0)), QCPacket = SUM(ISNULL(YDQC.PacketQty,0)), YDBC.YarnProgramID,
	                    UU.DisplayUnitDesc Uom, ISV1.SegmentValue YarnType, ISV2.SegmentValue YarnCount, ISV3.SegmentValue YarnComposition,
	                    ISV4.SegmentValue YarnShade, ISV5.SegmentValue YarnColor,
						BalanceSendQty = SUM(ISNULL(YDQC.ProductionQty,0)) - ISNULL(SYD.SendQty,0), BalanceSendConeQty = SUM(ISNULL(YDQC.ConeQty,0))- ISNULL(SYD.SendConeQty,0), BalanceSendPacketQty = SUM(ISNULL(YDQC.PacketQty,0)) - ISNULL(SYD.SendPacketQty,0),
						YDQC.YDQCChildID, YDQC.YDRICRBId, SendQty = ISNULL(SYD.SendQty,0)
                    FROM X
					INNER JOIN YDQCChild YDQC ON YDQC.YDBItemReqID = X.YDBItemReqID
					LEFT JOIN SYD ON SYD.YDBItemReqID = X.YDBItemReqID
					INNER JOIN YDBookingChild YDBC ON YDBC.YDBookingChildID = X.YDBookingChildID
                    INNER JOIN EPYSL..ItemMaster IM ON IM.ItemMasterID = YDBC.ItemMasterID
                    LEFT JOIN  EPYSL..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                    LEFT JOIN  EPYSL..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                    LEFT JOIN  EPYSL..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                    LEFT JOIN  EPYSL..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                    LEFT JOIN  EPYSL..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                    LEFT Join EPYSL..Unit AS UU On UU.UnitID = YDBC.UnitID
					--Where YDQC.QCQty - ISNULL(SYD.SendQty,0) > 0;
					GROUP BY X.YDBItemReqID, X.YDBatchID, YDBC.YDBookingChildID, YDBC.YDBookingMasterID, YDBC.ItemMasterID, YDBC.UnitID, YDBC.ShadeCode, YDBC.NoOfThread, 
						YDBC.ColorId, YDBC.BookingQty, YDBC.YarnCategory, YDBC.NoOfThread, YDBC.YarnProgramID, UU.DisplayUnitDesc, ISV1.SegmentValue, ISV2.SegmentValue, 
						ISV3.SegmentValue, ISV4.SegmentValue, ISV5.SegmentValue, ISNULL(SYD.SendQty,0), ISNULL(SYD.SendConeQty,0), ISNULL(SYD.SendPacketQty,0),
						YDQC.YDQCChildID, YDQC.YDRICRBId, SYD.SendQty
					)
					Select * FROM FL Where ISNULL(FL.QCQty,0) - ISNULL(FL.SendQty,0) > 0;";

            //var connection = _dbContext.Database.Connection;
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                SendToYDStoreMaster data = records.Read<SendToYDStoreMaster>().FirstOrDefault();
                data.Childs = records.Read<SendToYDStoreChild>().ToList();

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

        public async Task<SendToYDStoreMaster> GetAsync(int id)
        {
            var sql = $@";WITH M AS (
                        SELECT	QM.SendToYDStoreMasterID, QM.SendToYDStoreNo, QM.SendToYDStoreDate, QM.YDBookingMasterID,QM.Remarks, QM.YDBatchID
                        FROM SendToYDStoreMaster QM WHERE QM.SendToYDStoreMasterID = {id}
                    )
                    SELECT M.SendToYDStoreMasterID, M.SendToYDStoreNo, M.SendToYDStoreDate, M.YDBookingMasterID, M.Remarks,
                    BM.YDBookingNo, BM.YDBookingDate, PM.YDBatchNo, PM.YDBatchDate, M.YDBatchID
                    FROM M
                    INNER JOIN YDBookingMaster BM ON BM.YDBookingMasterID = M.YDBookingMasterID
                    INNER JOIN YDBatchMaster PM ON PM.YDBatchID = M.YDBatchID;

                    -----childs
                    ;WITH X AS (
                        SELECT YDSC.SendToYDStoreChildID, YDSC.SendToYDStoreMasterID, YDSC.UnitID, YDSC.ItemMasterID, YDSC.YarnProgramID, YDSC.BookingQty, YDSC.SendQty,
	                    YDSC.SendConeQty,YDSC.SendPacketQty, YDSC.Remarks, YDSC.YDBItemReqID
                        FROM SendToYDStoreChild YDSC WHERE YDSC.SendToYDStoreMasterID = {id}
                    )
                    SELECT X.SendToYDStoreChildID, X.SendToYDStoreMasterID, X.UnitID, X.ItemMasterID, X.YarnProgramID, X.BookingQty, X.SendQty,
	                X.SendConeQty,X.SendPacketQty, X.Remarks,UU.DisplayUnitDesc Uom, ISV1.SegmentValue YarnType, ISV2.SegmentValue YarnCount, ISV3.SegmentValue YarnComposition,
	                ISV4.SegmentValue YarnShade, ISV5.SegmentValue YarnColor, YDBC.YarnCategory, BI.YDBatchID
                    FROM X
					INNER JOIN YDDyeingBatchItem BI ON BI.YDBItemReqID = X.YDBItemReqID
					INNER JOIN YDProductionChild PC ON PC.YDDBIID = BI.YDDBIID
					INNER JOIN YDBookingChild YDBC ON YDBC.YDBookingChildID = PC.YDBookingChildID
                    INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = X.ItemMasterID
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                    Inner Join {DbNames.EPYSL}..Unit AS UU On UU.UnitID = X.UnitID;
                    ";

            //   var connection = _dbContext.Database.Connection;
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                SendToYDStoreMaster data = records.Read<SendToYDStoreMaster>().FirstOrDefault();
                data.Childs = records.Read<SendToYDStoreChild>().ToList();

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

        public async Task<SendToYDStoreMaster> GetAllAsync(int id)
        {
            var sql = $@"
            ;Select * From SendToYDStoreMaster Where SendToYDStoreMasterID = {id}

            ;Select * From SendToYDStoreChild Where SendToYDStoreMasterID = {id}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                SendToYDStoreMaster data = await records.ReadFirstOrDefaultAsync<SendToYDStoreMaster>();
                Guard.Against.NullObject(data);
                data.Childs = records.Read<SendToYDStoreChild>().ToList();
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

        public async Task SaveAsync(SendToYDStoreMaster entity)
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
                        entity.SendToYDStoreMasterID = await _service.GetMaxIdAsync(TableNames.SendToYDStoreMaster, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        entity.SendToYDStoreNo = await _service.GetMaxNoAsync(TableNames.SendToYDStoreNo, 1, RepeatAfterEnum.NoRepeat, "00000", transactionGmt, _connectionGmt);

                        maxChildId = await _service.GetMaxIdAsync(TableNames.SendToYDStoreChild, entity.Childs.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        foreach (SendToYDStoreChild item in entity.Childs)
                        {
                            item.SendToYDStoreChildID = maxChildId++;
                            item.SendToYDStoreMasterID = entity.SendToYDStoreMasterID;
                        }
                        break;

                    case EntityState.Modified:
                        var addedChilds = entity.Childs.FindAll(x => x.EntityState == EntityState.Added);
                        maxChildId = await _service.GetMaxIdAsync(TableNames.SendToYDStoreChild, addedChilds.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

                        foreach (SendToYDStoreChild item in addedChilds)
                        {
                            item.SendToYDStoreChildID = maxChildId++;
                            item.SendToYDStoreMasterID = entity.SendToYDStoreMasterID;
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
    }
}
