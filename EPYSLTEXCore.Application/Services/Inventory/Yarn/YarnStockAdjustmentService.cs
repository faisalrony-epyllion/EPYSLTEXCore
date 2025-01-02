using Dapper;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using System.Data;
using System.Data.Entity;
using Microsoft.Data.SqlClient;
using EPYSLTEXCore.Application.Interfaces.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory;
using EPYSLTEXCore.Infrastructure.Entities.Tex;

namespace EPYSLTEXCore.Application.Services.Inventory.Yarn
{

    public class YarnStockAdjustmentService : IYarnStockAdjustmentService
    {
        private readonly IDapperCRUDService<YarnStockAdjustmentMaster> _service;
        private readonly SqlConnection _connection;
        private readonly SqlConnection _connectionGmt;
        private SqlTransaction transaction;
        private SqlTransaction transactionGmt;
        public YarnStockAdjustmentService(IDapperCRUDService<YarnStockAdjustmentMaster> service)
        {
            _service = service;

            _service.Connection = service.GetConnection(AppConstants.GMT_CONNECTION);
            _connectionGmt = service.Connection;

            _service.Connection = service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _connection = service.Connection;
        }

        public async Task<List<YarnStockAdjustmentMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? " Order By YarnCategory,SupplierName,SpinnerName,YarnLotNo,PhysicalCount,ShadeCode " : paginationInfo.OrderBy;
            string sql = "";
            if (status == Status.Pending)
            {
                sql = $@";
                WITH 
				PL AS
				(
					SELECT YarnStockSetId_PP = YSS.YarnStockSetId, YarnStockSetId_WP = YSSWP.YarnStockSetId, PipelineStockQty = ISNULL(SUM(YSM.TotalCurrentPipelineStockQty),0) - ISNULL(SUM(YSMWP.TotalCurrentStockQty),0)
					FROM YarnStockSet YSS
					INNER JOIN YarnStockMaster YSM ON YSM.YarnStockSetId = YSS.YarnStockSetId
					LEFT JOIN YarnStockSet YSSWP ON YSSWP.ItemMasterId = YSS.ItemMasterId
												AND YSSWP.SpinnerId = YSS.SpinnerId
												AND ISNULL(YSSWP.ShadeCode,'') = ISNULL(YSS.ShadeCode,'')
												AND YSSWP.SpinnerId > 0
					LEFT JOIN YarnStockMaster YSMWP ON YSMWP.YarnStockSetId = YSSWP.YarnStockSetId
					WHERE YSS.SpinnerId = 0
					GROUP BY YSS.YarnStockSetId, YSSWP.YarnStockSetId
				),
				FinalList AS
				(
					SELECT YSS.YarnStockSetId, YSS.ItemMasterId, 
					YSS.SupplierId, YSS.SpinnerId, YSS.YarnLotNo,
					YSS.ShadeCode, YSS.PhysicalCount, YSS.YarnCategory,
					SupplierName = CASE WHEN YSS.SupplierID > 0 THEN S.Name ELSE '' END,
					SpinnerName = CASE WHEN YSS.SpinnerID > 0 THEN SP.Name ELSE '' END,
					Composition = ISV1.SegmentValue,
					YarnType = ISV2.SegmentValue,
					ManufacturingProcess = ISV3.SegmentValue,
					SubProcess = ISV4.SegmentValue,
					QualityParameter = ISV5.SegmentValue,
					Count = ISV6.SegmentValue,

					IsPipelineRecord = CASE WHEN YSS.SpinnerID > 0 THEN 0 ELSE 1 END,

					PipelineStockQty = SUM(ISNULL(PL.PipelineStockQty,0)),
					QuarantineStockQty = SUM(ISNULL(YSM.QuarantineStockQty,0)),
					TotalIssueQty = SUM(ISNULL(YSM.TotalIssueQty,0)),
					AdvanceStockQty = SUM(ISNULL(YSM.AdvanceStockQty,0)),
					AllocatedStockQty = SUM(ISNULL(YSM.AllocatedStockQty,0)),
					SampleStockQty = SUM(ISNULL(YSM.SampleStockQty,0)),
					LeftoverStockQty = SUM(ISNULL(YSM.LeftoverStockQty,0)),
					LiabilitiesStockQty = SUM(ISNULL(YSM.LiabilitiesStockQty,0)),
					UnusableStockQty = SUM(ISNULL(YSM.UnusableStockQty,0)),
					BlockPipelineStockQty = SUM(ISNULL(YSM.BlockPipelineStockQty,0)),
					BlockAdvanceStockQty = SUM(ISNULL(YSM.BlockAdvanceStockQty,0)),
					BlockSampleStockQty = SUM(ISNULL(YSM.BlockSampleStockQty,0)),
					BlockLeftoverStockQty = SUM(ISNULL(YSM.BlockLeftoverStockQty,0)),
					BlockLiabilitiesStockQty = SUM(ISNULL(YSM.BlockLiabilitiesStockQty,0))
					FROM YarnStockSet YSS
					INNER JOIN YarnStockMaster YSM ON YSM.YarnStockSetId = YSS.YarnStockSetId
					LEFT JOIN PL ON PL.YarnStockSetId_PP = YSS.YarnStockSetId
					LEFT JOIN {DbNames.EPYSL}..Contacts S ON S.ContactID = YSS.SupplierID
					LEFT JOIN {DbNames.EPYSL}..Contacts SP ON SP.ContactID = YSS.SpinnerID

					INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterId = YSS.ItemMasterId
					LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
					LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
					LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
					LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
					LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
					LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
					GROUP BY YSS.YarnStockSetId, YSS.ItemMasterId, 
					YSS.SupplierId, YSS.SpinnerId, YSS.YarnLotNo,
					YSS.ShadeCode, YSS.PhysicalCount, YSS.YarnCategory,
					S.Name,SP.Name,ISV1.SegmentValue,ISV2.SegmentValue,ISV3.SegmentValue,ISV4.SegmentValue,ISV5.SegmentValue,ISV6.SegmentValue
				)
				SELECT *, Count(*) Over() TotalRows FROM FinalList ";
            }
            else if (status == Status.Draft)
            {
                sql = $@";
				WITH FinalList AS
				(
					SELECT YSA.*, YSS.YarnCategory, AR.Reason, 
					SupplierName = SP.ShortName, SpinnerName = SPN.ShortName,
					YSS.YarnLotNo, YSS.PhysicalCount, YSS.ShadeCode,
					Count = ISV.SegmentValue, Count(*) Over() TotalRows
					FROM YarnStockAdjustmentMaster YSA
					INNER JOIN YarnStockSet YSS ON YSS.YarnStockSetId = YSA.YarnStockSetId
					LEFT JOIN AdjustmentReason AR ON AR.AdjustmentReasonId = YSA.AdjustmentReasonId
					LEFT JOIN {DbNames.EPYSL}..Contacts SP ON SP.ContactID = YSS.SupplierId
					LEFT JOIN {DbNames.EPYSL}..Contacts SPN ON SPN.ContactID = YSS.SpinnerId
					INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YSS.ItemMasterId
					INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = IM.Segment6ValueID
					WHERE YSA.IsSendForApproval = 0
					AND YSA.IsApproved = 0
					AND YSA.IsReject = 0
				)
				SELECT * FROM FinalList";
            }
            else if (status == Status.ProposedForApproval)
            {
                sql = $@";
				WITH FinalList AS
				(
					SELECT YSA.*, YSS.YarnCategory, AR.Reason, 
					SupplierName = SP.ShortName, SpinnerName = SPN.ShortName,
					YSS.YarnLotNo, YSS.PhysicalCount, YSS.ShadeCode,
					Count = ISV.SegmentValue, Count(*) Over() TotalRows
					FROM YarnStockAdjustmentMaster YSA
					INNER JOIN YarnStockSet YSS ON YSS.YarnStockSetId = YSA.YarnStockSetId
					LEFT JOIN AdjustmentReason AR ON AR.AdjustmentReasonId = YSA.AdjustmentReasonId
					LEFT JOIN {DbNames.EPYSL}..Contacts SP ON SP.ContactID = YSS.SupplierId
					LEFT JOIN {DbNames.EPYSL}..Contacts SPN ON SPN.ContactID = YSS.SpinnerId
					INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YSS.ItemMasterId
					INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = IM.Segment6ValueID
					WHERE YSA.IsSendForApproval = 1
					AND YSA.IsApproved = 0
					AND YSA.IsReject = 0
				)
				SELECT * FROM FinalList";
            }
            else if (status == Status.Approved)
            {
                sql = $@";
				WITH FinalList AS
				(
					SELECT YSA.*, YSS.YarnCategory, AR.Reason, 
					SupplierName = SP.ShortName, SpinnerName = SPN.ShortName,
					YSS.YarnLotNo, YSS.PhysicalCount, YSS.ShadeCode,
					Count = ISV.SegmentValue, Count(*) Over() TotalRows
					FROM YarnStockAdjustmentMaster YSA
					INNER JOIN YarnStockSet YSS ON YSS.YarnStockSetId = YSA.YarnStockSetId
					LEFT JOIN AdjustmentReason AR ON AR.AdjustmentReasonId = YSA.AdjustmentReasonId
					LEFT JOIN {DbNames.EPYSL}..Contacts SP ON SP.ContactID = YSS.SupplierId
					LEFT JOIN {DbNames.EPYSL}..Contacts SPN ON SPN.ContactID = YSS.SpinnerId
					INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YSS.ItemMasterId
					INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = IM.Segment6ValueID
					WHERE YSA.IsApproved = 1
					AND YSA.IsReject = 0 
				)
				SELECT * FROM FinalList";
            }
            else if (status == Status.Reject)
            {
                sql = $@";
				WITH FinalList AS
				(
					SELECT YSA.*, YSS.YarnCategory, AR.Reason, 
					SupplierName = SP.ShortName, SpinnerName = SPN.ShortName,
					YSS.YarnLotNo, YSS.PhysicalCount, YSS.ShadeCode,
					Count = ISV.SegmentValue, Count(*) Over() TotalRows
					FROM YarnStockAdjustmentMaster YSA
					INNER JOIN YarnStockSet YSS ON YSS.YarnStockSetId = YSA.YarnStockSetId
					LEFT JOIN AdjustmentReason AR ON AR.AdjustmentReasonId = YSA.AdjustmentReasonId
					LEFT JOIN {DbNames.EPYSL}..Contacts SP ON SP.ContactID = YSS.SupplierId
					LEFT JOIN {DbNames.EPYSL}..Contacts SPN ON SPN.ContactID = YSS.SpinnerId
					INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YSS.ItemMasterId
					INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = IM.Segment6ValueID
					WHERE YSA.IsReject = 1 
				)
				SELECT * FROM FinalList";
            }

            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<YarnStockAdjustmentMaster>(sql);
        }

        /*
        public async Task<List<YarnStockAdjustmentMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? " Order By YSAMasterId DESC " : paginationInfo.OrderBy;
            string sql = "";
			if (status == Status.Pending)
			{
				sql = $@"
				;WITH
				FinalList AS
				(
					SELECT M.*
					FROM YarnStockAdjustmentMaster M
					WHERE M.IsSendForApproval = 1 AND M.IsApproved = 0 
				)
				SELECT *, Count(*) Over() TotalRows FROM FinalList ";
			}
			if (status == Status.Draft)
            {
                sql = $@"
				;WITH
				FinalList AS
				(
					SELECT M.*
					FROM YarnStockAdjustmentMaster M
					WHERE M.IsSendForApproval = 1 AND M.IsApproved = 0 
				)
				SELECT *, Count(*) Over() TotalRows FROM FinalList ";
            }
			else if (status == Status.Approved)
			{
				sql = $@"
				;WITH
				FinalList AS
				(
					SELECT M.*
					FROM YarnStockAdjustmentMaster M
					WHERE M.IsApproved = 1
				)
				SELECT *, Count(*) Over() TotalRows FROM FinalList ";
			}

			sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";
            return await _service.GetDataAsync<YarnStockAdjustmentMaster>(sql);
        }
		*/
        public async Task<YarnStockAdjustmentMaster> GetNewAsync()
        {
            var sql = $@"
					{CommonQueries.GetAdjustmentReason()};

					{CommonQueries.GetAdjustmentType()};
					";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YarnStockAdjustmentMaster data = new YarnStockAdjustmentMaster();
                Guard.Against.NullObject(data);
                data.AdjustmentReasonList = records.Read<Select2OptionModel>().ToList();
                data.AdjustmentTypeList = records.Read<Select2OptionModel>().ToList();
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
        public async Task<YarnStockAdjustmentMaster> GetAsync(bool isPipelineRecord, int itemMasterId, int supplierId, string shadeCode, int yarnStockSetId)
        {
            string ppSql = isPipelineRecord ? " YSS.SpinnerId = 0 " : " YSS.SpinnerId > 0 ";
            string sql = "";

            sql = $@";WITH 
						PL AS
						(
							SELECT YarnStockSetId_PP = YSS.YarnStockSetId, YarnStockSetId_WP = YSSWP.YarnStockSetId, PipelineStockQty = ISNULL(SUM(YSM.TotalCurrentPipelineStockQty),0) - ISNULL(SUM(YSMWP.TotalCurrentStockQty),0)
							,SpinnerId = ISNULL(YSSWP.SpinnerId, 0)
							FROM YarnStockSet YSS
							INNER JOIN YarnStockMaster YSM ON YSM.YarnStockSetId = YSS.YarnStockSetId
							LEFT JOIN YarnStockSet YSSWP ON YSSWP.ItemMasterId = YSS.ItemMasterId
														AND YSSWP.SpinnerId = YSS.SpinnerId
														AND ISNULL(YSSWP.ShadeCode,'') = ISNULL(YSS.ShadeCode,'')
														AND YSSWP.SpinnerId > 0
							LEFT JOIN YarnStockMaster YSMWP ON YSMWP.YarnStockSetId = YSSWP.YarnStockSetId
							LEFT JOIN {DbNames.EPYSL}..Contacts S ON S.ContactID = YSS.SupplierID
							LEFT JOIN {DbNames.EPYSL}..Contacts SP ON SP.ContactID = YSS.SpinnerID
							WHERE YSS.SpinnerId = 0
							AND YSS.ItemMasterId = {itemMasterId}
							AND YSS.SupplierID = {supplierId}
							AND ISNULL(YSS.ShadeCode,'') = TRIM('{shadeCode}')
							GROUP BY YSS.YarnStockSetId, YSSWP.YarnStockSetId, ISNULL(YSSWP.SpinnerId, 0)
						),
						FinalList AS
						(
							SELECT YSS.YarnStockSetId, YSS.ItemMasterId, 
							YSS.SupplierId, YSS.SpinnerId, YSS.YarnLotNo,
							YSS.ShadeCode, YSS.PhysicalCount, YSS.YarnCategory,
							SupplierName = CASE WHEN YSS.SupplierID > 0 THEN S.Name ELSE '' END,
							SpinnerName = CASE WHEN YSS.SpinnerID > 0 THEN SP.Name ELSE '' END,
							Composition = ISV1.SegmentValue,
							YarnType = ISV2.SegmentValue,
							ManufacturingProcess = ISV3.SegmentValue,
							SubProcess = ISV4.SegmentValue,
							QualityParameter = ISV5.SegmentValue,
							Count = ISV6.SegmentValue,

							PipelineStockQty = SUM(ISNULL(PL.PipelineStockQty,0)),
							QuarantineStockQty = SUM(ISNULL(YSM.QuarantineStockQty,0)),
							TotalIssueQty = SUM(ISNULL(YSM.TotalIssueQty,0)),
							AdvanceStockQty = SUM(ISNULL(YSM.AdvanceStockQty,0)),
							AllocatedStockQty = SUM(ISNULL(YSM.AllocatedStockQty,0)),
							SampleStockQty = SUM(ISNULL(YSM.SampleStockQty,0)),
							LeftoverStockQty = SUM(ISNULL(YSM.LeftoverStockQty,0)),
							LiabilitiesStockQty = SUM(ISNULL(YSM.LiabilitiesStockQty,0)),
							UnusableStockQty = SUM(ISNULL(YSM.UnusableStockQty,0)),
							BlockPipelineStockQty = SUM(ISNULL(YSM.BlockPipelineStockQty,0)),
							BlockAdvanceStockQty = SUM(ISNULL(YSM.BlockAdvanceStockQty,0)),
							BlockSampleStockQty = SUM(ISNULL(YSM.BlockSampleStockQty,0)),
							BlockLeftoverStockQty = SUM(ISNULL(YSM.BlockLeftoverStockQty,0)),
							BlockLiabilitiesStockQty = SUM(ISNULL(YSM.BlockLiabilitiesStockQty,0))
							FROM YarnStockSet YSS
							INNER JOIN YarnStockMaster YSM ON YSM.YarnStockSetId = YSS.YarnStockSetId
							LEFT JOIN PL ON PL.YarnStockSetId_PP = YSS.YarnStockSetId
							LEFT JOIN {DbNames.EPYSL}..Contacts S ON S.ContactID = YSS.SupplierID
							LEFT JOIN {DbNames.EPYSL}..Contacts SP ON SP.ContactID = YSS.SpinnerID

							INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterId = YSS.ItemMasterId
							LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
							LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
							LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
							LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
							LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
							LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID

							WHERE YSS.YarnStockSetId = {yarnStockSetId}

							--WHERE YSS.ItemMasterId = {itemMasterId}
							--AND YSS.SupplierID = {supplierId}
							--AND ISNULL(YSS.ShadeCode,'') = TRIM('{shadeCode}')
							--AND {ppSql}

							GROUP BY YSS.YarnStockSetId, YSS.ItemMasterId, 
							YSS.SupplierId, YSS.SpinnerId, YSS.YarnLotNo,
							YSS.ShadeCode, YSS.PhysicalCount, YSS.YarnCategory,
							S.Name,SP.Name,ISV1.SegmentValue,ISV2.SegmentValue,ISV3.SegmentValue,ISV4.SegmentValue,ISV5.SegmentValue,ISV6.SegmentValue
						)
						SELECT *, Count(*) Over() TotalRows FROM FinalList;

						SELECT Id=AdjustmentReasonId, Text=Reason FROM AdjustmentReason;";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YarnStockAdjustmentMaster data = await records.ReadFirstOrDefaultAsync<YarnStockAdjustmentMaster>();
                data.IsPipelineRecord = isPipelineRecord;
                data.AdjustmentReasonList = records.Read<Select2OptionModel>().ToList();
                Guard.Against.NullObject(data);
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
        public async Task SaveAsync(YarnStockAdjustmentMaster entity, int UserId)
        {
            {
               
                try
                {
                    await _connection.OpenAsync();
                    transaction = _connection.BeginTransaction();

                    await _connectionGmt.OpenAsync();
                    transactionGmt = _connectionGmt.BeginTransaction();

                    int maxChildId = 0;
                    int maxChildItemId = 0;

                    entity.Childs.ForEach(c =>
                    {
                        if (c.EntityState == EntityState.Added) maxChildId++;
                        c.ChildItems.ForEach(ci =>
                        {
                            if (ci.EntityState == EntityState.Added) maxChildItemId++;
                        });
                    });

                    switch (entity.EntityState)
                    {
                        case EntityState.Added:

                            entity.YSAMasterId = await _service.GetMaxIdAsync(TableNames.YARN_STOCK_ADJUSTMENT_MASTER, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                            entity.AdjustmentNo = await _service.GetMaxNoAsync(TableNames.YARN_ADJUSTMENT_NO, 1, RepeatAfterEnum.EveryYear);

                            maxChildId = await _service.GetMaxIdAsync(TableNames.YARN_STOCK_ADJUSTMENT_CHILD, maxChildId, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                            maxChildItemId = await _service.GetMaxIdAsync(TableNames.YARN_STOCK_ADJUSTMENT_CHILD_ITEM, maxChildItemId, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

                            foreach (var item in entity.Childs)
                            {
                                item.YSAChildId = maxChildId++;
                                item.YSAMasterId = entity.YSAMasterId;
                                item.EntityState = EntityState.Added;

                                foreach (var childItem in item.ChildItems)
                                {
                                    childItem.YSAChildItemId = maxChildItemId++;
                                    childItem.YSAChildId = item.YSAChildId;
                                    childItem.EntityState = EntityState.Added;
                                }
                            }

                            break;

                        case EntityState.Modified:

                            maxChildId = await _service.GetMaxIdAsync(TableNames.YARN_STOCK_ADJUSTMENT_CHILD, maxChildId, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                            maxChildItemId = await _service.GetMaxIdAsync(TableNames.YARN_STOCK_ADJUSTMENT_CHILD_ITEM, maxChildItemId, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

                            entity.Childs.ForEach(c =>
                            {
                                if (c.EntityState == EntityState.Added)
                                {
                                    c.YSAChildId = maxChildId++;
                                    c.YSAMasterId = entity.YSAMasterId;
                                    c.EntityState = EntityState.Added;
                                }
                                c.ChildItems.Where(x => x.EntityState == EntityState.Added).ToList().ForEach(o =>
                                {
                                    o.YSAChildItemId = maxChildItemId++;
                                    o.YSAChildId = c.YSAChildId;
                                    o.EntityState = EntityState.Added;
                                });
                            });

                            break;

                        case EntityState.Unchanged:
                        case EntityState.Deleted:
                            entity.EntityState = EntityState.Deleted;
                            entity.Childs.ForEach(c =>
                            {
                                c.EntityState = EntityState.Detached;
                                c.ChildItems.SetDeleted();
                            });
                            break;

                        default:
                            break;
                    }

                    List<YarnStockAdjustmentChild> childs = new List<YarnStockAdjustmentChild>();
                    List<YarnStockAdjustmentChildItem> childItems = new List<YarnStockAdjustmentChildItem>();

                    entity.Childs.ForEach(c =>
                    {
                        childs.Add(c);
                        childItems.AddRange(c.ChildItems);
                    });

                    await _service.SaveSingleAsync(entity, transaction);

                    if (!entity.IsApproved && !entity.IsReject)
                    {
                        await _service.SaveAsync(childs.Where(x => x.EntityState == EntityState.Deleted).ToList(), transaction);
                        await _service.SaveAsync(childItems.Where(x => x.EntityState == EntityState.Deleted).ToList(), transaction);

                        await _service.SaveAsync(childs.Where(x => x.EntityState != EntityState.Deleted).ToList(), transaction);
                        await _service.SaveAsync(childItems.Where(x => x.EntityState != EntityState.Deleted).ToList(), transaction);
                    }
                    #region Child Item Validation
                    for (int i = 0; i < childItems.Count(); i++)
                    {
                        var childItem = childItems[i];
                        //await _service.ValidationSingleAsync(childItem, transaction, "sp_Validation_YarnStockAdjustmentChildItem", childItem.EntityState, entity.AddedBy, childItem.YSAChildItemId);
                        await _connection.ExecuteAsync("sp_Validation_YarnStockAdjustmentChildItem", new { EntityState = childItem.EntityState, UserId = UserId, PrimaryKeyId = childItem.YSAChildItemId }, transaction, 30, CommandType.StoredProcedure);
                    }
                    #endregion

                    #region Stock Operation
                    if (entity.IsApproved)
                    {
                        //int userId = entity.EntityState == EntityState.Added ? entity.AddedBy : entity.UpdatedBy;
                        //await _connection.ExecuteAsync("spYarnStockOperation", new { MasterID = entity.YSAMasterId, FromMenuType = EnumFromMenuType.YarnReceive, UserId = userId }, transaction, 30, CommandType.StoredProcedure);
                    }
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
		public async Task ItemSaveAsync(List<YarnReceiveChild> items, int UserId)
		{
			try
			{
				await _connection.OpenAsync();
				transaction = _connection.BeginTransaction();

				await _connectionGmt.OpenAsync();
				transactionGmt = _connectionGmt.BeginTransaction();

				#region Child Item Validation
				for (int i = 0; i < items.Count(); i++)
				{
					var childItem = items[i];
					await _connection.ExecuteAsync("sp_YarnStock_New_Item", new
					{
						ItemMasterID = childItem.ItemMasterID,
						SupplierId = childItem.SupplierId,
						SpinnerID = childItem.SpinnerID,
						YarnLotNo = childItem.YarnLotNo,
						PhysicalCount = childItem.PhysicalCount,
						ShadeCode = childItem.ShadeCode,
						YarnCategory = childItem.YarnCategory,
						UserId = UserId
					}, transaction, 30, CommandType.StoredProcedure);
				}
				#endregion

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
       
        public async Task<List<YarnReceiveChildRackBin>> GetRackBin(int yarnStockSetId, int itemMasterId, int supplierId, int spinnerId, string yarnLotNo, string shadeCode, string physicalCount, string childRackIds)
        {
            string sql = "";
            if (yarnStockSetId > 0)
            {
                sql = $@"WITH RB AS
						(
							SELECT RB.ChildRackBinID, RB.RackID, YRC.YarnStockSetId, Rack.RackNo, RackBin.BinNo, LocationName = L.ShortName,
							RB.NoOfCartoon, RB.NoOfCone, RB.ReceiveQty
							FROM YarnReceiveChild YRC
							INNER JOIN YarnReceiveChildRackBin RB ON RB.ChildID = YRC.ChildID
							LEFT JOIN {DbNames.EPYSL}..Rack ON Rack.RackID = RB.RackID
							LEFT JOIN {DbNames.EPYSL}..RackBin ON RackBin.BinID = RB.BinID
							LEFT JOIN {DbNames.EPYSL}..Location L ON L.LocationID = RB.LocationID
							WHERE YRC.YarnStockSetId = {yarnStockSetId}

							UNION

							SELECT RB.ChildRackBinID, RB.RackID, YarnStockSetId = {yarnStockSetId}, Rack.RackNo, RackBin.BinNo, LocationName = L.ShortName,
							RB.NoOfCartoon, RB.NoOfCone, RB.ReceiveQty
							FROM YarnReceiveChildRackBin RB
							LEFT JOIN {DbNames.EPYSL}..Rack ON Rack.RackID = RB.RackID
							LEFT JOIN {DbNames.EPYSL}..RackBin ON RackBin.BinID = RB.BinID
							LEFT JOIN {DbNames.EPYSL}..Location L ON L.LocationID = RB.LocationID
							WHERE RB.ChildID = 0 
							AND ISNULL(RB.ItemMasterId,0) = {itemMasterId}
							AND ISNULL(RB.SupplierId,0) = {supplierId}
							AND ISNULL(RB.SpinnerId,0) = {spinnerId}
							AND ISNULL(RB.LotNo,'') = '{yarnLotNo}'
							AND ISNULL(RB.ShadeCode,'') = '{shadeCode}'
							AND ISNULL(RB.PhysicalCount,'') = '{physicalCount}'
						)
						SELECT * FROM RB
						ORDER BY RackNo, BinNo";
            }
            else
            {
                sql = $@"SELECT RB.ChildRackBinID, RB.RackID, YRC.YarnStockSetId, Rack.RackNo, RackBin.BinNo, LocationName = L.ShortName,
						RB.NoOfCartoon, RB.NoOfCone, RB.ReceiveQty
						FROM YarnReceiveChild YRC
						INNER JOIN YarnReceiveMaster YRM ON YRM.ReceiveID = YRC.ReceiveID
						INNER JOIN YarnReceiveChildRackBin RB ON RB.ChildID = YRC.ChildID
						LEFT JOIN {DbNames.EPYSL}..Rack ON Rack.RackID = RB.RackID
						LEFT JOIN {DbNames.EPYSL}..RackBin ON RackBin.BinID = RB.BinID
						LEFT JOIN {DbNames.EPYSL}..Location L ON L.LocationID = RB.LocationID
						WHERE YRM.SupplierID = {supplierId} 
						AND YRC.ItemMasterId = {itemMasterId}
						AND YRC.SpinnerID = {spinnerId}
						AND ISNULL(YRC.LotNo,'') = '{yarnLotNo}'
						AND ISNULL(YRC.ShadeCode,'') = '{shadeCode}'
						AND ISNULL(YRC.PhysicalCount,'') = '{physicalCount}'
						ORDER BY Rack.RackNo, RackBin.BinNo";
            }
            List<YarnReceiveChildRackBin> childRacks = await _service.GetDataAsync<YarnReceiveChildRackBin>(sql);
            if (childRackIds.IsNotNullOrEmpty())
            {
                sql = $@"SELECT YarnStockSetId = {yarnStockSetId}, RB.ChildRackBinID, RB.RackID, YRC.YarnStockSetId, Rack.RackNo, RackBin.BinNo, LocationName = L.ShortName,
						RB.NoOfCartoon, RB.NoOfCone, RB.ReceiveQty
						FROM YarnReceiveChild YRC
						INNER JOIN YarnReceiveMaster YRM ON YRM.ReceiveID = YRC.ReceiveID
						INNER JOIN YarnReceiveChildRackBin RB ON RB.ChildID = YRC.ChildID
						LEFT JOIN {DbNames.EPYSL}..Rack ON Rack.RackID = RB.RackID
						LEFT JOIN {DbNames.EPYSL}..RackBin ON RackBin.BinID = RB.BinID
						LEFT JOIN {DbNames.EPYSL}..Location L ON L.LocationID = RB.LocationID
						WHERE RB.ChildRackBinID IN ({childRackIds})
						ORDER BY Rack.RackNo, RackBin.BinNo;";

                List<YarnReceiveChildRackBin> tempChildRacks = await _service.GetDataAsync<YarnReceiveChildRackBin>(sql);

                tempChildRacks.ForEach(x =>
                {
                    var obj = childRacks.Find(y => y.ChildRackBinID == x.ChildRackBinID);
                    if (obj.IsNull())
                    {
                        x = CommonFunction.DeepClone(x);
                        childRacks.Add(x);
                    }
                });
            }
            return childRacks;

        }
        public async Task<YarnStockAdjustmentMaster> GetAsync(int id)
        {
            var sql = $@";

				SELECT YSA.* 
				FROM YarnStockAdjustmentMaster YSA
				WHERE YSA.YSAMasterId = {id}

				SELECT YSC.* 
				FROM YarnStockAdjustmentMaster YSA
				INNER JOIN YarnStockAdjustmentChild YSC ON YSC.YSAMasterId = YSA.YSAMasterId
				WHERE YSA.YSAMasterId = {id}

				SELECT YSCI.* 
				FROM YarnStockAdjustmentMaster YSA
				INNER JOIN YarnStockAdjustmentChild YSC ON YSC.YSAMasterId = YSA.YSAMasterId
				INNER JOIN YarnStockAdjustmentChildItem YSCI ON YSCI.YSAChildId = YSC.YSAChildId
				WHERE YSA.YSAMasterId = {id}";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YarnStockAdjustmentMaster data = await records.ReadFirstOrDefaultAsync<YarnStockAdjustmentMaster>();
                data.Childs = records.Read<YarnStockAdjustmentChild>().ToList();
                var childItems = records.Read<YarnStockAdjustmentChildItem>().ToList();
                data.Childs.ForEach(c =>
                {
                    c.ChildItems = childItems.Where(ci => ci.YSAChildId == c.YSAChildId).ToList();
                });
                Guard.Against.NullObject(data);
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
        public async Task<YarnStockAdjustmentMaster> GetAsync2(int id)
        {
            var sql = $@";

				;WITH
				SelectedYSS AS
				(
					SELECT YSS.*
					FROM YarnStockAdjustmentMaster YSA
					INNER JOIN YarnStockSet YSS ON YSS.YarnStockSetId = YSA.YarnStockSetId
					WHERE YSA.YSAMasterId = {id}
				),
				PL AS
				(
					SELECT YarnStockSetId_PP = YSS.YarnStockSetId, YarnStockSetId_WP = YSSWP.YarnStockSetId, PipelineStockQty = ISNULL(SUM(YSM.TotalCurrentPipelineStockQty),0) - ISNULL(SUM(YSMWP.TotalCurrentStockQty),0)
					,SpinnerId = ISNULL(YSSWP.SpinnerId, 0)
					FROM YarnStockSet YSS
					INNER JOIN SelectedYSS SYSS ON SYSS.YarnStockSetId = YSS.YarnStockSetId
					INNER JOIN YarnStockMaster YSM ON YSM.YarnStockSetId = YSS.YarnStockSetId
					LEFT JOIN YarnStockSet YSSWP ON YSSWP.ItemMasterId = YSS.ItemMasterId
												AND YSSWP.SpinnerId = YSS.SpinnerId
												AND ISNULL(YSSWP.ShadeCode,'') = ISNULL(YSS.ShadeCode,'')
												AND YSSWP.SpinnerId > 0
					LEFT JOIN YarnStockMaster YSMWP ON YSMWP.YarnStockSetId = YSSWP.YarnStockSetId
					LEFT JOIN {DbNames.EPYSL}..Contacts S ON S.ContactID = YSS.SupplierID
					LEFT JOIN {DbNames.EPYSL}..Contacts SP ON SP.ContactID = YSS.SpinnerID
					WHERE YSS.SpinnerId = 0
					AND YSS.ItemMasterId = SYSS.ItemMasterId
					AND YSS.SupplierID = SYSS.SupplierId
					AND ISNULL(YSS.ShadeCode,'') = TRIM(SYSS.ShadeCode)
					GROUP BY YSS.YarnStockSetId, YSSWP.YarnStockSetId, ISNULL(YSSWP.SpinnerId, 0)
				),
				FinalObj AS
				(
					SELECT YSS.YarnStockSetId, YSS.ItemMasterId, 
					YSS.SupplierId, YSS.SpinnerId, YSS.YarnLotNo,
					YSS.ShadeCode, YSS.PhysicalCount, YSS.YarnCategory,
					SupplierName = CASE WHEN YSS.SupplierID > 0 THEN S.Name ELSE '' END,
					SpinnerName = CASE WHEN YSS.SpinnerID > 0 THEN SP.Name ELSE '' END,
					Composition = ISV1.SegmentValue,
					YarnType = ISV2.SegmentValue,
					ManufacturingProcess = ISV3.SegmentValue,
					SubProcess = ISV4.SegmentValue,
					QualityParameter = ISV5.SegmentValue,
					Count = ISV6.SegmentValue,

					PipelineStockQty = SUM(ISNULL(PL.PipelineStockQty,0)),
					QuarantineStockQty = SUM(ISNULL(YSM.QuarantineStockQty,0)),
					TotalIssueQty = SUM(ISNULL(YSM.TotalIssueQty,0)),
					AdvanceStockQty = SUM(ISNULL(YSM.AdvanceStockQty,0)),
					AllocatedStockQty = SUM(ISNULL(YSM.AllocatedStockQty,0)),
					SampleStockQty = SUM(ISNULL(YSM.SampleStockQty,0)),
					LeftoverStockQty = SUM(ISNULL(YSM.LeftoverStockQty,0)),
					LiabilitiesStockQty = SUM(ISNULL(YSM.LiabilitiesStockQty,0)),
					UnusableStockQty = SUM(ISNULL(YSM.UnusableStockQty,0)),
					BlockPipelineStockQty = SUM(ISNULL(YSM.BlockPipelineStockQty,0)),
					BlockAdvanceStockQty = SUM(ISNULL(YSM.BlockAdvanceStockQty,0)),
					BlockSampleStockQty = SUM(ISNULL(YSM.BlockSampleStockQty,0)),
					BlockLeftoverStockQty = SUM(ISNULL(YSM.BlockLeftoverStockQty,0)),
					BlockLiabilitiesStockQty = SUM(ISNULL(YSM.BlockLiabilitiesStockQty,0))
					FROM YarnStockSet YSS
					INNER JOIN SelectedYSS SYSS ON SYSS.YarnStockSetId = YSS.YarnStockSetId
					INNER JOIN YarnStockMaster YSM ON YSM.YarnStockSetId = YSS.YarnStockSetId
					LEFT JOIN PL ON PL.YarnStockSetId_PP = YSS.YarnStockSetId
					LEFT JOIN {DbNames.EPYSL}..Contacts S ON S.ContactID = YSS.SupplierID
					LEFT JOIN {DbNames.EPYSL}..Contacts SP ON SP.ContactID = YSS.SpinnerID

					INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterId = YSS.ItemMasterId
					LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
					LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
					LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
					LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
					LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
					LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID

					WHERE YSS.ItemMasterId = SYSS.ItemMasterId
					AND YSS.SupplierID = SYSS.SupplierId
					AND ISNULL(YSS.ShadeCode,'') = TRIM(SYSS.ShadeCode)
					AND YSS.SpinnerId = SYSS.SpinnerId

					GROUP BY YSS.YarnStockSetId, YSS.ItemMasterId, 
					YSS.SupplierId, YSS.SpinnerId, YSS.YarnLotNo,
					YSS.ShadeCode, YSS.PhysicalCount, YSS.YarnCategory,
					S.Name,SP.Name,ISV1.SegmentValue,ISV2.SegmentValue,ISV3.SegmentValue,ISV4.SegmentValue,ISV5.SegmentValue,ISV6.SegmentValue
				)
				SELECT FO.*, YSA.* 
				FROM FinalObj FO
				INNER JOIN YarnStockAdjustmentMaster YSA ON YSA.YarnStockSetId = FO.YarnStockSetId
				WHERE YSA.YSAMasterId = {id}

				SELECT YSC.* 
				FROM YarnStockAdjustmentMaster YSA
				INNER JOIN YarnStockAdjustmentChild YSC ON YSC.YSAMasterId = YSA.YSAMasterId
				WHERE YSA.YSAMasterId = {id}

				SELECT YSCI.* 
				FROM YarnStockAdjustmentMaster YSA
				INNER JOIN YarnStockAdjustmentChild YSC ON YSC.YSAMasterId = YSA.YSAMasterId
				INNER JOIN YarnStockAdjustmentChildItem YSCI ON YSCI.YSAChildId = YSC.YSAChildId
				WHERE YSA.YSAMasterId = {id}

				SELECT Id=AdjustmentReasonId, Text=Reason FROM AdjustmentReason;";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YarnStockAdjustmentMaster data = await records.ReadFirstOrDefaultAsync<YarnStockAdjustmentMaster>();
                data.Childs = records.Read<YarnStockAdjustmentChild>().ToList();
                var childItems = records.Read<YarnStockAdjustmentChildItem>().ToList();
                data.AdjustmentReasonList = records.Read<Select2OptionModel>().ToList();
                data.Childs.ForEach(c =>
                {
                    c.ChildItems = childItems.Where(ci => ci.YSAChildId == c.YSAChildId).ToList();
                });
                Guard.Against.NullObject(data);
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
        public async Task<YarnStockAdjustmentMaster> GetRelatedList()
        {
            var sql = $@";
				SELECT id=ContactID,text=ShortName FROM {DbNames.EPYSL}..Contacts ORDER BY Name;

				-- Shade book
                {CommonQueries.GetYarnShadeBooks()};";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YarnStockAdjustmentMaster data = new YarnStockAdjustmentMaster();
                data.SupplierList = records.Read<Select2OptionModel>().ToList();
                data.SpinnerList = data.SupplierList;
                data.ShadeCodes = records.Read<Select2OptionModel>().ToList();
                Guard.Against.NullObject(data);
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

        #region Common Methods for Stocks
        public async Task<List<YarnStockAdjustmentMaster>> GetAllStocks(string yarnCategory,
            string physicalCount,
            string yarnLotNo,
            string shadeCode,
            string supplier,
            string spinner,
            string count,
            string otherQuery,
            bool isValidItem,
            PaginationInfo paginationInfo)
        {
            string tempGuid = CommonFunction.GetNewGuid();
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? " Order By YarnCategory DESC" : paginationInfo.OrderBy;

            yarnCategory = yarnCategory.IsNotNullOrEmpty() ? $@" AND YSS.YarnCategory = '{yarnCategory}'" : "";
            physicalCount = physicalCount.IsNotNullOrEmpty() ? $@" AND YSS.PhysicalCount = '{physicalCount}'" : "";
            yarnLotNo = yarnLotNo.IsNotNullOrEmpty() ? $@" AND YSS.YarnLotNo = '{yarnLotNo}'" : "";
            shadeCode = shadeCode.IsNotNullOrEmpty() ? $@" AND YSS.ShadeCode = '{shadeCode}'" : "";
            supplier = supplier.IsNotNullOrEmpty() ? $@" AND SU.ShortName = '{supplier}'" : "";
            spinner = spinner.IsNotNullOrEmpty() ? $@" AND SP.ShortName = '{spinner}'" : "";
            count = count.IsNotNullOrEmpty() ? $@" AND ISV6.SegmentValue = '{count}'" : "";

            string validQuery = isValidItem ? " AND YSS.IsInvalidItem = 0 " : "";


            var sql = $@"
                WITH 
                FinalList AS
				(
					 SELECT YSS.YarnStockSetId
                    ,YSS.ItemMasterId
					,YSS.SupplierId
					,YSS.SpinnerId
					,YSS.YarnCategory
					,YSS.PhysicalCount
					,YSS.YarnLotNo
					,YSS.ShadeCode
					,SupplierName = SU.ShortName
					,SpinnerName = SP.ShortName
					,YSM.QuarantineStockQty
					,YSM.TotalIssueQty
					,YSM.AdvanceStockQty
					,YSM.AllocatedStockQty
					,YSM.SampleStockQty
					,YSM.LiabilitiesStockQty
					,YSM.LeftoverStockQty
					,YSM.UnusableStockQty

					,YSM.BlockAdvanceStockQty
					,YSM.BlockAllocatedStockQty
					,YSM.BlockLeftoverStockQty
					,YSM.BlockLiabilitiesStockQty
					,YSM.BlockPipelineStockQty
					,YSM.BlockSampleStockQty

					,Segment1ValueId = ISV1.SegmentValueID
					,Segment1ValueDesc = ISV1.SegmentValue
					,Segment2ValueId = ISV2.SegmentValueID
					,Segment2ValueDesc = ISV2.SegmentValue
					,Segment3ValueId = ISV3.SegmentValueID
					,Segment3ValueDesc = ISV3.SegmentValue
					,Segment4ValueId = ISV4.SegmentValueID
					,Segment4ValueDesc = ISV4.SegmentValue
					,Segment5ValueId = ISV5.SegmentValueID
					,Segment5ValueDesc = ISV5.SegmentValue
					,Segment6ValueId = ISV6.SegmentValueID
					,Segment6ValueDesc = ISV6.SegmentValue

					,Composition = ISV1.SegmentValue
					,YarnType = ISV2.SegmentValue
					,ManufacturingProcess = ISV3.SegmentValue
					,SubProcess = ISV4.SegmentValue
					,QualityParameter = ISV5.SegmentValue
					,Count = ISV6.SegmentValue

					,YSS.IsInvalidItem
					,YSS.Note

					FROM YarnStockSet YSS
					INNER JOIN YarnStockMaster YSM ON YSM.YarnStockSetId = YSS.YarnStockSetId
					INNER JOIN {DbNames.EPYSL}..Contacts SU ON SU.ContactID = YSS.SupplierId
					INNER JOIN {DbNames.EPYSL}..Contacts SP ON SP.ContactID = YSS.SpinnerId
					INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YSS.ItemMasterId
					LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
					LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
					LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
					LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
					LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
					LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
					WHERE YSS.SpinnerId > 0 {validQuery}
                    {yarnCategory}
                    {physicalCount}
                    {yarnLotNo}
                    {shadeCode}
                    {supplier}
                    {spinner}
                    {count}
					{otherQuery}
				)
                SELECT * INTO #TempTable{tempGuid}
                FROM FinalList FL
                SELECT *, Count(*) Over() TotalRows FROM #TempTable{tempGuid}
                ";

            sql += $@"
                    {paginationInfo.FilterBy}
                    {orderBy}
                    {paginationInfo.PageBy}";

            sql += $@" DROP TABLE #TempTable{tempGuid}";

            try
            {
                List<YarnStockAdjustmentMaster> data = await _service.GetDataAsync<YarnStockAdjustmentMaster>(sql);
                if (data.IsNull()) data = new List<YarnStockAdjustmentMaster>();
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
        private string GetStockQuery(int itemMasterId, int supplierId, int spinnerId, string physicalCount, string yarnLotNo, string shadeCode)
        {
            string sql = $@"SELECT YSS.* 
							FROM YarnStockSet YSS
							WHERE YSS.ItemMasterId={itemMasterId}
							AND YSS.SupplierId={supplierId}
							AND YSS.SpinnerId={spinnerId}
							AND YSS.PhysicalCount='{physicalCount}'
							AND YSS.YarnLotNo='{yarnLotNo}'
							AND YSS.ShadeCode='{shadeCode}'";
            return sql;
        }
        public async Task<List<YarnStockAdjustmentMaster>> GetItemWithStockSetId(List<YarnStockAdjustmentMaster> stocks)
        {
            string sql = "";
            int count = 0;
            stocks.ForEach(s =>
            {
                sql += this.GetStockQuery(s.ItemMasterID, s.SupplierId, s.SpinnerId, s.PhysicalCount, s.YarnLotNo, s.ShadeCode);

                count++;
                if (count < stocks.Count())
                {
                    sql += " UNION ";
                }
            });
            try
            {
                List<YarnStockAdjustmentMaster> data = await _service.GetDataAsync<YarnStockAdjustmentMaster>(sql);
                if (data.IsNull()) data = new List<YarnStockAdjustmentMaster>();
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
        public async Task<List<YarnStockAdjustmentMaster>> GetAllStocks(string otherQuery)
        {
            var sql = $@"
                WITH 
                FinalList AS
				(
					 SELECT YSS.YarnStockSetId
                    ,YSS.ItemMasterId
					,YSS.SupplierId
					,YSS.SpinnerId
					,YSS.YarnCategory
					,YSS.PhysicalCount
					,YSS.YarnLotNo
					,YSS.ShadeCode
					,SupplierName = SU.ShortName
					,SpinnerName = SP.ShortName
					,YSM.QuarantineStockQty
					,YSM.AdvanceStockQty
					,YSM.AllocatedStockQty
					,YSM.SampleStockQty
					,YSM.LiabilitiesStockQty
					,YSM.LeftoverStockQty
					,YSM.UnusableStockQty

					,Segment1ValueId = ISV1.SegmentValueID
					,Segment1ValueDesc = ISV1.SegmentValue
					,Segment2ValueId = ISV2.SegmentValueID
					,Segment2ValueDesc = ISV2.SegmentValue
					,Segment3ValueId = ISV3.SegmentValueID
					,Segment3ValueDesc = ISV3.SegmentValue
					,Segment4ValueId = ISV4.SegmentValueID
					,Segment4ValueDesc = ISV4.SegmentValue
					,Segment5ValueId = ISV5.SegmentValueID
					,Segment5ValueDesc = ISV5.SegmentValue
					,Segment6ValueId = ISV6.SegmentValueID
					,Segment6ValueDesc = ISV6.SegmentValue

					,Composition = ISV1.SegmentValue
					,YarnType = ISV2.SegmentValue
					,ManufacturingProcess = ISV3.SegmentValue
					,SubProcess = ISV4.SegmentValue
					,QualityParameter = ISV5.SegmentValue
					,Count = ISV6.SegmentValue

					FROM YarnStockSet YSS
					INNER JOIN YarnStockMaster YSM ON YSM.YarnStockSetId = YSS.YarnStockSetId
					INNER JOIN {DbNames.EPYSL}..Contacts SU ON SU.ContactID = YSS.SupplierId
					INNER JOIN {DbNames.EPYSL}..Contacts SP ON SP.ContactID = YSS.SpinnerId
					INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YSS.ItemMasterId
					LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
					LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
					LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
					LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
					LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
					LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
					WHERE YSS.SpinnerId > 0
				)
                SELECT * FROM FinalList FL WHERE 1=1 {otherQuery}";

            return await _service.GetDataAsync<YarnStockAdjustmentMaster>(sql);
        }
        #endregion

        #region Others

        public async Task<List<Select2OptionModel>> GetStockTypes()
        {
            var objList = await _service.GetDataAsync<Select2OptionModel>(CommonQueries.GetStockTypes());
            objList.Insert(0, new Select2OptionModel()
            {
                id = 0.ToString(),
                text = "--Select Type--"
            });
            return objList;

        }
        #endregion
    }
}
