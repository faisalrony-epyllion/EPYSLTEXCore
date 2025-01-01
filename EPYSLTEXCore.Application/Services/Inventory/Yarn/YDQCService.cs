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

namespace EPYSLTEXCore.Application.Services.Inventory
{
    internal class YDQCService : IYDQCService
    {
        private readonly IDapperCRUDService<YDQCMaster> _service;
        private readonly SqlConnection _connection;
        private readonly SqlConnection _connectionGmt;

        public YDQCService(IDapperCRUDService<YDQCMaster> service)
        {
            _service = service;
            _service.Connection = service.GetConnection(AppConstants.GMT_CONNECTION);
            _connectionGmt = service.Connection;

            _service.Connection = service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _connection = service.Connection;
        }

        public async Task<List<YDQCMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By YDBatchDate Desc" : paginationInfo.OrderBy;
            var sql = string.Empty;
            if (status == Status.Pending)
            {
                sql += $@";WITH YDBC AS 
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
			   M AS (
                    SELECT	YDBM.YDBatchID, YDBM.YDBatchNo, YDBM.YDBatchDate, YBM.YDBookingMasterID, YBM.BuyerID, YDBM.Remarks, YDBM.IsApproved
                    FROM {TableNames.YD_BATCH_MASTER} YDBM
					INNER JOIN {TableNames.YD_BATCH_ITEM_REQUIREMENT} IR ON IR.YDBatchID = YDBM.YDBatchID
					INNER JOIN {TableNames.YD_BOOKING_MASTER} YBM ON YBM.YDBookingMasterID = YDBM.YDBookingMasterID
					INNER JOIN YDBC ON YDBC.YDBookingMasterID = YBM.YDBookingMasterID 
					--INNER JOIN {TableNames.YD_DYEING_BATCH_ITEM} BI ON BI.YDBItemReqID = YDBC.BItemReqID
					--INNER JOIN {TableNames.YD_PRODUCTION_MASTER} YDPM ON YDPM.YDBookingMasterID = YBM.YDBookingMasterID 
					INNER JOIN YDRC ON YDRC.YDBatchID = YDBM.YDBatchID
                    INNER JOIN {TableNames.HardWindingMaster} HWM ON HWM.YDBatchID = YDBM.YDBatchID
					LEFT JOIN {TableNames.YD_QC_MASTER} QC ON QC.YDBatchID = YDBM.YDBatchID
					WHERE ISNULL(YDBM.IsApproved,0) = 1 AND QC.YDProductionMasterID IS NULL
					GROUP BY YDBM.YDBatchID, YDBM.YDBatchNo, YDBM.YDBatchDate, YBM.YDBookingMasterID, YBM.BuyerID, YDBM.Remarks, YDBM.IsApproved
                )
                SELECT M.YDBatchID, M.YDBatchNo, M.YDBatchDate, M.YDBookingMasterID, M.BuyerID, M.Remarks, M.IsApproved,
                BM.YDBookingNo, BM.YDBookingDate, CTO.Name Buyer, Count(*) Over() TotalRows
                FROM M
                INNER JOIN EPYSL..Contacts CTO ON CTO.ContactID = M.BuyerID
                INNER JOIN {TableNames.YD_BOOKING_MASTER} BM ON BM.YDBookingMasterID = M.YDBookingMasterID";

                orderBy = string.IsNullOrEmpty(orderBy) ? "ORDER BY YDBatchDate DESC" : orderBy;
            }
            else if (status == Status.Draft)
            {
                sql += $@"WITH M AS (
                    SELECT	QM.YDBatchID, QM.YDQCMasterID, QM.YDQCNo, QM.YDQCDate, QM.YDBookingMasterID, QM.YDProductionMasterID, QM.Remarks
                    FROM {TableNames.YD_QC_MASTER} QM
                    Where QM.IsSendForApprove = 0
                )
                SELECT M.YDQCMasterID, M.YDQCNo, M.YDQCDate, M.YDBookingMasterID, M.YDProductionMasterID, M.Remarks, BTM.YDBatchID, BTM.YDBatchNo, BTM.YDBatchDate,
                BM.YDBookingNo, BM.YDBookingDate, Count(*) Over() TotalRows
                FROM M
                INNER JOIN {TableNames.YD_BOOKING_MASTER} BM ON BM.YDBookingMasterID = M.YDBookingMasterID
                --INNER JOIN {TableNames.YD_PRODUCTION_MASTER} PM ON PM.YDProductionMasterID = M.YDProductionMasterID
				INNER JOIN {TableNames.YD_BATCH_MASTER} BTM ON BTM.YDBatchID = M.YDBatchID ";
                orderBy = string.IsNullOrEmpty(orderBy) ? "ORDER BY YDQCDate DESC" : orderBy;
            }
            else if (status == Status.AwaitingPropose)
            {
                sql += $@"WITH M AS (
                    SELECT	QM.YDBatchID, QM.YDQCMasterID, QM.YDQCNo, QM.YDQCDate, QM.YDBookingMasterID, QM.YDProductionMasterID, QM.Remarks
                    FROM {TableNames.YD_QC_MASTER} QM
                    Where QM.IsSendForApprove = 1 AND QM.IsApprove = 0 AND QM.IsReject = 0
                )
                SELECT M.YDQCMasterID, M.YDQCNo, M.YDQCDate, M.YDBookingMasterID, M.YDProductionMasterID, M.Remarks, BTM.YDBatchID, BTM.YDBatchNo, BTM.YDBatchDate,
                BM.YDBookingNo, BM.YDBookingDate, Count(*) Over() TotalRows
                FROM M
                INNER JOIN {TableNames.YD_BOOKING_MASTER} BM ON BM.YDBookingMasterID = M.YDBookingMasterID
                --INNER JOIN {TableNames.YD_PRODUCTION_MASTER} PM ON PM.YDProductionMasterID = M.YDProductionMasterID
				INNER JOIN {TableNames.YD_BATCH_MASTER} BTM ON BTM.YDBatchID = M.YDBatchID ";
                orderBy = string.IsNullOrEmpty(orderBy) ? "ORDER BY YDQCDate DESC" : orderBy;
            }
            else if (status == Status.Approved)
            {
                sql += $@"WITH M AS (
                    SELECT	QM.YDBatchID, QM.YDQCMasterID, QM.YDQCNo, QM.YDQCDate, QM.YDBookingMasterID, QM.YDProductionMasterID, QM.Remarks
                    FROM {TableNames.YD_QC_MASTER} QM
                    Where QM.IsSendForApprove = 1 AND QM.IsApprove = 1 AND QM.IsReject = 0
                )
                SELECT M.YDQCMasterID, M.YDQCNo, M.YDQCDate, M.YDBookingMasterID, M.YDProductionMasterID, M.Remarks, BTM.YDBatchID, BTM.YDBatchNo, BTM.YDBatchDate,
                BM.YDBookingNo, BM.YDBookingDate, Count(*) Over() TotalRows
                FROM M
                INNER JOIN {TableNames.YD_BOOKING_MASTER} BM ON BM.YDBookingMasterID = M.YDBookingMasterID
                --INNER JOIN {TableNames.YD_PRODUCTION_MASTER} PM ON PM.YDProductionMasterID = M.YDProductionMasterID
				INNER JOIN {TableNames.YD_BATCH_MASTER} BTM ON BTM.YDBatchID = M.YDBatchID ";
                orderBy = string.IsNullOrEmpty(orderBy) ? "ORDER BY YDQCDate DESC" : orderBy;
            }
            else if (status == Status.Reject)
            {
                sql += $@"WITH M AS (
                    SELECT	QM.YDBatchID, QM.YDQCMasterID, QM.YDQCNo, QM.YDQCDate, QM.YDBookingMasterID, QM.YDProductionMasterID, QM.Remarks
                    FROM {TableNames.YD_QC_MASTER} QM
                    Where QM.IsSendForApprove = 1 AND QM.IsApprove = 0 AND QM.IsReject = 1
                )
                SELECT M.YDQCMasterID, M.YDQCNo, M.YDQCDate, M.YDBookingMasterID, M.YDProductionMasterID, M.Remarks, BTM.YDBatchID, BTM.YDBatchNo, BTM.YDBatchDate,
                BM.YDBookingNo, BM.YDBookingDate, Count(*) Over() TotalRows
                FROM M
                INNER JOIN {TableNames.YD_BOOKING_MASTER} BM ON BM.YDBookingMasterID = M.YDBookingMasterID
                --INNER JOIN {TableNames.YD_PRODUCTION_MASTER} PM ON PM.YDProductionMasterID = M.YDProductionMasterID
				INNER JOIN {TableNames.YD_BATCH_MASTER} BTM ON BTM.YDBatchID = M.YDBatchID ";
                orderBy = string.IsNullOrEmpty(orderBy) ? "ORDER BY YDQCDate DESC" : orderBy;
            }
            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<YDQCMaster>(sql);
        }

        public async Task<YDQCMaster> GetNewAsync(int newId)
        {
            var sql = $@"
/*
                    ;WITH M AS (
                        SELECT	PM.YDProductionMasterID, PM.YDProductionNo, PM.YDProductionDate, PM.YDBookingMasterID, PM.BuyerID, PM.Remarks, PM.IsApprove, PM.IsAcknowledge
                        FROM {TableNames.YD_PRODUCTION_MASTER} PM WHERE PM.YDProductionMasterID = {newId}
                    )
                    SELECT M.YDProductionMasterID, M.YDProductionNo, M.YDProductionDate, M.YDBookingMasterID, M.BuyerID,
                    BM.YDBookingNo, BM.YDBookingDate, CTO.Name Buyer
                    FROM M
                    INNER JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = M.BuyerID
                    INNER JOIN {TableNames.YD_BOOKING_MASTER} BM ON BM.YDBookingMasterID = M.YDBookingMasterID;

                    -----childs
                    ;WITH X AS (
                        SELECT PC.YDProductionChildID, PC.YDProductionMasterID,PC.ItemMasterID, PC.UnitID, PC.YarnDyedColorID, PC.BookingQty,
	                    PC.Remarks, PC.YarnCategory, PC.NoOfThread, PC.ProducedQty, PC.TodayProductionQty, PC.YarnProgramID
                        FROM {TableNames.YD_PRODUCTION_CHILD} PC WHERE PC.YDProductionMasterID = {newId}
                    )
                    SELECT X.YDProductionChildID, X.YDProductionMasterID, X.ItemMasterID, YDBC.UnitID, X.YarnDyedColorID, X.BookingQty,
	                    X.YarnCategory, X.NoOfThread, X.ProducedQty, X.TodayProductionQty ProductionQty, X.YarnProgramID,
	                    UU.DisplayUnitDesc Uom, ISV1.SegmentValue YarnType, ISV2.SegmentValue YarnCount, ISV3.SegmentValue YarnComposition,
	                    ISV4.SegmentValue YarnShade, ISV5.SegmentValue YarnColor
                    FROM X
                    INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = X.ItemMasterID
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                    Inner Join {DbNames.EPYSL}..Unit AS UU On UU.UnitID = YDBC.UnitID;
*/
                    ;WITH M AS (
                        SELECT	PM.YDBatchID, PM.YDBatchNo, PM.YDBatchDate, PM.YDBookingMasterID, PM.BuyerID, PM.Remarks, PM.IsApproved
                        FROM {TableNames.YD_BATCH_MASTER} PM WHERE PM.YDBatchID = {newId}
                    )
                    SELECT M.YDBatchID, M.YDBatchNo, M.YDBatchDate, M.YDBookingMasterID, M.BuyerID,
                    BM.YDBookingNo, BM.YDBookingDate, CTO.Name Buyer
                    FROM M
                    INNER JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = M.BuyerID
                    INNER JOIN {TableNames.YD_BOOKING_MASTER} BM ON BM.YDBookingMasterID = M.YDBookingMasterID;

                    -----childs
                    ;WITH X AS (
                        SELECT * FROM {TableNames.YD_BATCH_ITEM_REQUIREMENT} PC WHERE PC.YDBatchID = {newId}
                    ),
					PC AS 
					(
					Select BI.YDDBIID,ProducedQty = SUM(ISNULL(PC.ProducedQty,0)), TodayProductionQty = SUM(ISNULL(PC.TodayProductionQty,0)) 
					FROM {TableNames.YD_DYEING_BATCH_ITEM} BI 
					INNER JOIN {TableNames.YD_PRODUCTION_CHILD} PC ON PC.YDDBIID = BI.YDDBIID
					GROUP BY BI.YDDBIID
					)
                    SELECT X.YDBItemReqID, X.YDBatchID, YDBC.YDBookingChildID, YDBC.YDBookingMasterID, YDBC.ItemMasterID, YDBC.UnitID, YDBC.ShadeCode, YDBC.NoOfThread, 
						YDBC.ColorId As ColorID, YDBC.BookingQty,
	                    YDBC.YarnCategory, YDBC.NoOfThread, PC.ProducedQty, PC.TodayProductionQty ProductionQty, YDBC.YarnProgramID,
	                    UU.DisplayUnitDesc Uom, ISV1.SegmentValue YarnType, ISV2.SegmentValue YarnCount, ISV3.SegmentValue YarnComposition,
	                    ISV4.SegmentValue YarnShade, ISV5.SegmentValue YarnColor, HWC.HardWindingChildID, HWC.YDRICRBId
                    FROM X
					INNER JOIN {TableNames.YD_DYEING_BATCH_ITEM} BI ON BI.YDBItemReqID = X.YDBItemReqID
					INNER JOIN PC ON PC.YDDBIID = BI.YDDBIID
					INNER JOIN {TableNames.HardWindingChild} HWC ON HWC.YDBItemReqID = X.YDBItemReqID
					INNER JOIN {TableNames.YDBookingChild} YDBC ON YDBC.YDBookingChildID = X.YDBookingChildID
                    INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YDBC.ItemMasterID
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                    LEFT Join {DbNames.EPYSL}..Unit AS UU On UU.UnitID = YDBC.UnitID;
                    -----SupplierList
                    {CommonQueries.GetContactsByCategoryType(ContactCategoryNames.SUPPLIER)};

                    -----SpinnerList
                    {CommonQueries.GetContactsByCategoryType(ContactCategoryNames.SUPPLIER)};
                    ";

            //var connection = _dbContext.Database.Connection;
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YDQCMaster data = records.Read<YDQCMaster>().FirstOrDefault();
                data.Childs = records.Read<YDQCChild>().ToList();
                data.SupplierList = records.Read<Select2OptionModel>().ToList();
                data.SpinnerList = records.Read<Select2OptionModel>().ToList();

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

        public async Task<YDQCMaster> GetAsync(int id)
        {
            var sql = $@";WITH M AS (
                        SELECT	QM.YDQCMasterID, QM.YDQCNo, QM.YDQCDate, QM.YDBookingMasterID, QM.YDProductionMasterID, QM.Remarks, QM.YDBatchID
                        FROM {TableNames.YD_QC_MASTER} QM WHERE QM.YDQCMasterID = {id}
                    )
                    SELECT M.YDQCMasterID, M.YDQCNo, M.YDQCDate, M.YDBookingMasterID, M.YDProductionMasterID, M.Remarks,
                    BM.YDBookingNo, BM.YDBookingDate, PM.YDBatchNo, PM.YDBatchDate, M.YDBatchID
                    FROM M
                    INNER JOIN {TableNames.YD_BOOKING_MASTER} BM ON BM.YDBookingMasterID = M.YDBookingMasterID
                    INNER JOIN {TableNames.YD_BATCH_MASTER} PM ON PM.YDBatchID = M.YDBatchID;

                    -----childs
                    ;WITH X AS (
                        SELECT QC.YDQCChildID, QC.YDQCMasterID, QC.SupplierID, QC.SpinnerID, QC.LotNo, QC.UnitID, QC.ItemMasterID, QC.YarnProgramID, QC.BookingQty, QC.ProducedQty,
	                    QC.ProductionQty, QC.Remarks, QC.QCPass, QC.QCFail, QC.ReTest, QC.YDBItemReqID, QC.ConeQty, QC.PacketQty
                        FROM {TableNames.YD_QC_CHILD} QC WHERE QC.YDQCMasterID = {id}
                    )
                    SELECT X.YDQCChildID, X.YDQCMasterID, X.SupplierID, X.SpinnerID, X.LotNo, X.UnitID, X.ItemMasterID, X.YarnProgramID, X.BookingQty, X.ProducedQty, X.ProductionQty,
	                X.Remarks, X.QCPass, X.QCFail, X.ReTest, UU.DisplayUnitDesc Uom, ISV1.SegmentValue YarnType, ISV2.SegmentValue YarnCount, ISV3.SegmentValue YarnComposition,
	                ISV4.SegmentValue YarnShade, ISV5.SegmentValue YarnColor, YDBC.YarnCategory, X.ConeQty, X.PacketQty, BI.YDBatchID
                    FROM X
					INNER JOIN {TableNames.YD_DYEING_BATCH_ITEM} BI ON BI.YDBItemReqID = X.YDBItemReqID
					INNER JOIN {TableNames.YD_PRODUCTION_CHILD} PC ON PC.YDDBIID = BI.YDDBIID
					INNER JOIN {TableNames.YDBookingChild} YDBC ON YDBC.YDBookingChildID = PC.YDBookingChildID
                    INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = X.ItemMasterID
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                    Inner Join {DbNames.EPYSL}..Unit AS UU On UU.UnitID = X.UnitID;

                    -----SupplierList
                    {CommonQueries.GetContactsByCategoryType(ContactCategoryNames.SUPPLIER)};

                    -----SpinnerList
                    {CommonQueries.GetContactsByCategoryType(ContactCategoryNames.SUPPLIER)};
                    ";

            //   var connection = _dbContext.Database.Connection;
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YDQCMaster data = records.Read<YDQCMaster>().FirstOrDefault();
                data.Childs = records.Read<YDQCChild>().ToList();
                data.SupplierList = records.Read<Select2OptionModel>().ToList();
                data.SpinnerList = records.Read<Select2OptionModel>().ToList();

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

        public async Task<YDQCMaster> GetAllAsync(int id)
        {
            var sql = $@"
            ;Select * FROM {TableNames.YD_QC_MASTER} Where YDQCMasterID = {id}

            ;Select * FROM {TableNames.YD_QC_CHILD} Where YDQCMasterID = {id}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YDQCMaster data = await records.ReadFirstOrDefaultAsync<YDQCMaster>();
                Guard.Against.NullObject(data);
                data.Childs = records.Read<YDQCChild>().ToList();
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

        public async Task SaveAsync(YDQCMaster entity)
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
                        entity.YDQCMasterID = await _service.GetMaxIdAsync(TableNames.YD_QC_MASTER, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        entity.YDQCNo = await _service.GetMaxNoAsync(TableNames.YD_QC_NO, 1, RepeatAfterEnum.NoRepeat, "00000", transactionGmt, _connectionGmt);

                        maxChildId = await _service.GetMaxIdAsync(TableNames.YD_QC_CHILD, entity.Childs.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        foreach (YDQCChild item in entity.Childs)
                        {
                            item.YDQCChildID = maxChildId++;
                            item.YDQCMasterID = entity.YDQCMasterID;
                        }
                        break;

                    case EntityState.Modified:
                        var addedChilds = entity.Childs.FindAll(x => x.EntityState == EntityState.Added);
                        maxChildId = await _service.GetMaxIdAsync(TableNames.YD_QC_CHILD, addedChilds.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

                        foreach (YDQCChild item in addedChilds)
                        {
                            item.YDQCChildID = maxChildId++;
                            item.YDQCMasterID = entity.YDQCMasterID;
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
