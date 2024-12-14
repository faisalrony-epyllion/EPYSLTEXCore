using Dapper;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Application.Interfaces;
using EPYSLTEXCore.Application.Interfaces.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex;
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

namespace EPYSLTEXCore.Application.Services.Inventory
{
    public class YarnQCIssueService : IYarnQCIssueService
    {
        //: DapperBaseEntity
        private readonly IDapperCRUDService<YarnQCIssueMaster> _service;

        private readonly SqlConnection _connection;
        private readonly SqlConnection _connectionGmt;

        public YarnQCIssueService(IDapperCRUDService<YarnQCIssueMaster> service
            , IDapperCRUDService<YarnQCIssueChild> itemMasterRepository)
        {
            _service = service;

            _service.Connection = service.GetConnection(AppConstants.GMT_CONNECTION);
            _connectionGmt = service.Connection;

            _service.Connection = service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _connection = service.Connection;
        }

        public async Task<List<YarnQCIssueMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By QCIssueMasterID Desc" : paginationInfo.OrderBy;

            string sql;
            if (status == Status.Pending)
            {
                sql = $@"With YQCReqPending As(
	                        Select M.QCReqMasterID, IsPending = MAX(Case When IC.QCIssueChildID IS NULL Then 1 Else 0 End)
	                        From {TableNames.YARN_QC_REQ_MASTER} M
	                        INNER JOIN {TableNames.YARN_QC_REQ_CHILD} QCRC ON QCRC.QCReqMasterID = M.QCReqMasterID
	                        INNER JOIN {TableNames.YARN_RECEIVE_MASTER} RM ON RM.ReceiveID = M.ReceiveID
	                        LEFT JOIN {TableNames.YARN_QC_ISSUE_CHILD} IC ON IC.QCReqChildID = QCRC.QCReqChildID
	                        WHERE M.IsApprove = 1 AND M.IsAcknowledge = 1 AND RM.ApprovedDate >= '{CommonConstent.StockMigrationDate}'

	                        GROUP BY M.QCReqMasterID
                        ),
                        YQCReq As 
                        (
	                        Select M.QCReqMasterID, M.QCReqNo, U.Name QCReqByUser, M.QCReqDate, QCReqFor.ValueName QCReqFor, 
	                        M.IsApprove, M.IsAcknowledge,
	                        M.ReceiveID, RM.ReceiveNo, P.IsPending,
	                        IsMRIRCompleted = MAX(Case When YMC.MRIRChildID is not NULL Then 1 Else 0 END) 
	                        From {TableNames.YARN_QC_REQ_MASTER} M
	                        LEFT JOIN {TableNames.YARN_QC_REQ_CHILD} QCRC ON QCRC.QCReqMasterID = M.QCReqMasterID
	                        LEFT JOIN {TableNames.YARN_RECEIVE_MASTER} RM ON RM.ReceiveID = M.ReceiveID
	                        LEFT JOIN {TableNames.YARN_MRIR_CHILD} YMC ON YMC.ReceiveChildID = QCRC.ReceiveChildID
	                        LEFT Join {DbNames.EPYSL}..EntityTypeValue QCReqFor On M.QCForID = QCReqFor.ValueID
	                        LEFT Join {DbNames.EPYSL}..LoginUser U On M.QCReqBy = U.UserCode 
	                        LEFT JOIN {TableNames.YARN_QC_ISSUE_MASTER} IM ON IM.QCReqMasterID = M.QCReqMasterID
	                        INNER JOIN YQCReqPending P ON P.QCReqMasterID = M.QCReqMasterID
	                        WHERE M.IsApprove = 1 AND M.IsAcknowledge = 1 AND P.IsPending = 1--AND IM.QCReqMasterID IS NULL  
	                        AND RM.ApprovedDate >= '{CommonConstent.StockMigrationDate}'
	                        GROUP BY M.QCReqMasterID, M.QCReqNo, U.Name, M.QCReqDate, QCReqFor.ValueName, 
	                        M.IsApprove, M.IsAcknowledge, M.ReceiveID, RM.ReceiveNo, P.IsPending
                        )
                        Select *, Count(*) Over() TotalRows
                        From YQCReq ";
                orderBy = "ORDER BY QCReqMasterID DESC";
            }
            else if (status == Status.Approved)
            {
                sql = $@"
                With 
                M As (
	                SELECT M.QCIssueMasterID, M.QCIssueNo, M.QCIssueDate, M.QCIssueBy, M.QCReqMasterID, 
	                M.Approve, M.Reject, M.LocationId, M.RCompanyId, M.CompanyId, M.SupplierId, M.SpinnerId,
	                IU.Name QCIssueByUser, RM.QCReqNo, RM.QCReqDate, RM.QCForID, RU.Name QCReqByUser, 
                    QCReqFor.ValueName QCReqFor, RM1.ReceiveNo
	                FROM {TableNames.YARN_QC_ISSUE_MASTER} M
	                Inner Join {DbNames.EPYSL}..LoginUser IU On IU.UserCode = M.QCIssueBy
	                Inner Join {TableNames.YARN_QC_REQ_MASTER} RM On RM.QCReqMasterID = M.QCReqMasterID
	                Inner Join {DbNames.EPYSL}..LoginUser RU On RU.UserCode = RM.QCReqBy 
	                Left Join {DbNames.EPYSL}..EntityTypeValue QCReqFor On RM.QCForID = QCReqFor.ValueID
                    LEFT JOIN {TableNames.YARN_RECEIVE_MASTER} RM1 ON RM1.ReceiveID = M.ReceiveID
	                Where M.Approve = 1 AND M.Reject = 0
                )
                SELECT *, Count(*) Over() TotalRows FROM M";
            }
            else if (status == Status.Reject)
            {
                sql = $@"
                With 
                M As (
	                SELECT M.QCIssueMasterID, M.QCIssueNo, M.QCIssueDate, M.QCIssueBy, M.QCReqMasterID, 
	                M.Approve, M.Reject, M.LocationId, M.RCompanyId, M.CompanyId, M.SupplierId, M.SpinnerId,
	                IU.Name QCIssueByUser, RM.QCReqNo, RM.QCReqDate, RM.QCForID, RU.Name QCReqByUser, 
                    QCReqFor.ValueName QCReqFor, RM1.ReceiveNo
	                FROM {TableNames.YARN_QC_ISSUE_MASTER} M
	                Inner Join {DbNames.EPYSL}..LoginUser IU On IU.UserCode = M.QCIssueBy
	                Inner Join {TableNames.YARN_QC_REQ_MASTER} RM On RM.QCReqMasterID = M.QCReqMasterID
	                Inner Join {DbNames.EPYSL}..LoginUser RU On RU.UserCode = RM.QCReqBy 
	                Left Join {DbNames.EPYSL}..EntityTypeValue QCReqFor On RM.QCForID = QCReqFor.ValueID
                    LEFT JOIN {TableNames.YARN_RECEIVE_MASTER} RM1 ON RM1.ReceiveID = M.ReceiveID
	                Where M.Approve = 0 AND M.Reject = 1
                )
                SELECT *, Count(*) Over() TotalRows FROM M";
            }
            else
            {
                sql = $@"
                With 
                M As (
	                SELECT M.QCIssueMasterID, M.QCIssueNo, M.QCIssueDate, M.QCIssueBy, M.QCReqMasterID, 
	                M.Approve, M.Reject, M.LocationId, M.RCompanyId, M.CompanyId, M.SupplierId, M.SpinnerId,
	                IU.Name QCIssueByUser, RM.QCReqNo, RM.QCReqDate, RM.QCForID, RU.Name QCReqByUser, 
                    QCReqFor.ValueName QCReqFor, RM1.ReceiveNo
	                FROM {TableNames.YARN_QC_ISSUE_MASTER} M
	                INNER JOIN {TableNames.YARN_QC_REQ_MASTER} RCM ON RCM.QCReqMasterID = M.QCReqMasterID
                    INNER JOIN {TableNames.YARN_RECEIVE_MASTER} RM1 ON RM1.ReceiveID = RCM.ReceiveID
	                Inner Join {DbNames.EPYSL}..LoginUser IU On IU.UserCode = M.QCIssueBy
	                Inner Join {TableNames.YARN_QC_REQ_MASTER} RM On RM.QCReqMasterID = M.QCReqMasterID
	                Inner Join {DbNames.EPYSL}..LoginUser RU On RU.UserCode = RM.QCReqBy 
	                Left Join {DbNames.EPYSL}..EntityTypeValue QCReqFor On RM.QCForID = QCReqFor.ValueID
	                Where M.Approve = 0 AND M.Reject = 0 AND RM1.ApprovedDate >= '{CommonConstent.StockMigrationDate}'
                )
                SELECT *, Count(*) Over() TotalRows FROM M";
            }

            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<YarnQCIssueMaster>(sql);
        }

        public async Task<YarnQCIssueMaster> GetNewAsync(int qcReqMasterId)
        {
            var query =
                $@"
                Select QCReqMasterID, RM.QCReqNo, RM.QCReqDate, QCReqFor.ValueName QCReqFor, RU.Name QCReqByUser,
				RM.ReceiveID, RM.LocationId, RM.RCompanyId, RM.CompanyId, RM.SupplierId, RM.SpinnerId
                From {TableNames.YARN_QC_REQ_MASTER} RM
                Left Join {DbNames.EPYSL}..EntityTypeValue QCReqFor On RM.QCForID = QCReqFor.ValueID
                Inner Join {DbNames.EPYSL}..LoginUser RU On RM.QCReqBy = RU.UserCode 
                Where QCReqMasterID = {qcReqMasterId};

                ;Select QCIssueChildID = YRC.QCReqChildID, YRC.QCReqChildID, YRC.QCReqMasterID, YRC.ReceiveChildID, YRC.LotNo, YRC.ChallanLot, YRC.ReqQty, YRC.ReqBagPcs, YRC.ReqCone ReqQtyCone,YRC.ItemMasterID, YRC.UnitID, YRC.Rate,YRC.YarnProgramId, 
                YRC.ChallanCount, YRC.POCount, YRC.PhysicalCount, 
                SupplierName = Case When IsNull(YRM.SupplierID,0) = 0 Then '' Else CC.[Name] End,
                Spinner = Case When IsNull(YR.SpinnerID,0) = 0 Then '' Else Spinner.Name End, 
                YRC.YarnCategory, YRC.NoOfThread, UU.DisplayUnitDesc Uom, ISV1.SegmentValue Segment1ValueDesc, 
                ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc, 
                ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc,YRC.ShadeCode, YRC.QCReqRemarks, IC.QCIssueChildID
                From {TableNames.YARN_QC_REQ_CHILD} YRC
				LEFT JOIN {TableNames.YARN_QC_ISSUE_CHILD} IC ON IC.QCReqChildID = YRC.QCReqChildID
                LEFT Join {TableNames.YARN_RECEIVE_CHILD} YR ON YR.ChildID=YRC.ReceiveChildID
                LEFT Join {TableNames.YARN_RECEIVE_MASTER} YRM On YRM.ReceiveID=YRC.ReceiveID
                LEFT JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = YRM.SupplierID
                LEFT Join {DbNames.EPYSL}..Contacts Spinner ON Spinner.ContactID = YR.SpinnerID
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YRC.ItemMasterID 
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                LEFT Join {DbNames.EPYSL}..Unit AS UU On UU.UnitID = YRC.UnitID
                Where YRC.QCReqMasterID = {qcReqMasterId} AND IC.QCIssueChildID IS NULL
                Group By YRC.QCReqChildID, YRC.QCReqMasterID, YRC.ReceiveChildID, YRC.LotNo, YRC.ChallanLot, YRC.ReqQty, YRC.ReqBagPcs, YRC.ReqCone,YRC.ItemMasterID, YRC.UnitID, YRC.Rate,YRC.YarnProgramId, 
                YRC.ChallanCount, YRC.POCount, YRC.PhysicalCount, YRC.YarnCategory, YRC.NoOfThread, UU.DisplayUnitDesc, ISV1.SegmentValue, YR.SpinnerID, Spinner.Name,
                ISV2.SegmentValue, ISV3.SegmentValue, ISV4.SegmentValue, ISV5.SegmentValue, ISV6.SegmentValue, ISV7.SegmentValue,YRC.ShadeCode,
                IsNull(YRM.SupplierID,0), CC.[Name], YRC.QCReqRemarks, IC.QCIssueChildID
";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                YarnQCIssueMaster data = await records.ReadFirstOrDefaultAsync<YarnQCIssueMaster>();
                Guard.Against.NullObject(data);
                data.YarnQCIssueChilds = records.Read<YarnQCIssueChild>().ToList();
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

        public async Task<YarnQCIssueMaster> GetAsync(int id)
        {
            var query =
                $@"
                With 
                M As (
                    Select * From {TableNames.YARN_QC_ISSUE_MASTER} Where QCIssueMasterID = {id}
                )
                SELECT M.QCIssueMasterID, M.QCIssueNo, M.QCIssueDate, M.QCIssueBy, M.QCReqMasterID, M.ReceiveID, 
                M.Approve, M.Reject, M.LocationId, M.RCompanyId, M.CompanyId, M.SupplierId, M.SpinnerId,
                IU.Name QCIssueByUser, RM.QCReqNo, RM.QCReqDate, RM.QCForID, RU.Name QCReqByUser, QCReqFor.ValueName QCReqFor
                FROM M
                Inner Join {DbNames.EPYSL}..LoginUser IU On IU.UserCode = M.QCIssueBy
                Inner Join {TableNames.YARN_QC_REQ_MASTER} RM On RM.QCReqMasterID = M.QCReqMasterID
                Inner Join {DbNames.EPYSL}..LoginUser RU On RU.UserCode = RM.QCReqBy 
                left Join {DbNames.EPYSL}..EntityTypeValue QCReqFor On RM.QCForID = QCReqFor.ValueID;

                ;Select YRC.QCIssueChildID, YRC.QCIssueMasterID, YRC.ReceiveChildID, YRC.LotNo, YRC.ChallanLot, ReqC.ReqBagPcs, YRC.ReqQty, YRC.ReqQtyCone, YRC.ReqQtyCarton, YRC.IssueQty, YRC.IssueQtyCone, YRC.IssueQtyCarton,
                YRC.ItemMasterID, YRC.UnitID, YRC.Rate,YRC.YarnProgramId, YRC.ChallanCount, YRC.POCount, YRC.PhysicalCount, YRC.YarnCategory, YRC.NoOfThread, ReqC.QCReqRemarks,
                UU.DisplayUnitDesc Uom, ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, 
                ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc, YRC.ShadeCode
                From {TableNames.YARN_QC_ISSUE_CHILD} YRC
				INNER JOIN {TableNames.YARN_QC_ISSUE_MASTER} YRM ON YRM.QCIssueMasterID = YRC.QCIssueMasterID
				INNER JOIN {TableNames.YARN_QC_REQ_CHILD} ReqC ON ReqC.QCReqChildID = YRC.QCReqChildID
                INNER JOIN {TableNames.YARN_QC_REQ_MASTER} QCRM ON QCRM.QCReqMasterID = ReqC.QCReqMasterID
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YRC.ItemMasterID 
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                LEFT Join {DbNames.EPYSL}..Unit AS UU On UU.UnitID = YRC.UnitID
                Where YRC.QCIssueMasterID = {id}
                Group By YRC.QCIssueChildID, YRC.QCIssueMasterID, YRC.ReceiveChildID, YRC.LotNo, YRC.ChallanLot, ReqC.ReqBagPcs, YRC.ReqQty, YRC.ReqQtyCone, YRC.ReqQtyCarton, YRC.IssueQty, YRC.IssueQtyCone, YRC.IssueQtyCarton,
                YRC.ItemMasterID, YRC.UnitID, YRC.Rate,YRC.YarnProgramId, YRC.ChallanCount, YRC.POCount, YRC.PhysicalCount, YRC.YarnCategory, YRC.NoOfThread, ReqC.QCReqRemarks,
                UU.DisplayUnitDesc,ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue, 
                ISV4.SegmentValue, ISV5.SegmentValue, ISV6.SegmentValue, ISV7.SegmentValue, 
                YRC.ShadeCode,YRC.ReqQty, YRC.ReqQtyCone, YRC.ReqQtyCarton;

                --Rack bins Mapping
                SELECT M.*
				FROM {TableNames.YARN_QC_ISSUE_CHILD_CHILD_RACK_BIN_MAPPING} M
				INNER JOIN {TableNames.YARN_QC_ISSUE_CHILD} PRC ON PRC.QCIssueChildID = M.QCIssueChildID
				WHERE PRC.QCIssueMasterID = {id};";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                YarnQCIssueMaster data = await records.ReadFirstOrDefaultAsync<YarnQCIssueMaster>();
                Guard.Against.NullObject(data);
                data.YarnQCIssueChilds = records.Read<YarnQCIssueChild>().ToList();

                var rackBinMappingList = records.Read<YarnQCIssueChildRackBinMapping>().ToList();
                if (rackBinMappingList == null) rackBinMappingList = new List<YarnQCIssueChildRackBinMapping>();

                data.YarnQCIssueChilds.ForEach(c =>
                {
                    c.ChildRackBins = rackBinMappingList.Where(y => y.QCIssueChildID == c.QCIssueChildID).ToList();
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

        public async Task<YarnQCIssueMaster> GetAllAsync(int id)
        {
            var sql = $@"
            ;Select * From {TableNames.YARN_QC_ISSUE_MASTER} Where QCIssueMasterID = {id}

            ;Select * From {TableNames.YARN_QC_ISSUE_CHILD} Where QCIssueMasterID = {id}

            ;SELECT M.*
			FROM {TableNames.YARN_QC_ISSUE_CHILD_CHILD_RACK_BIN_MAPPING} M
			INNER JOIN {TableNames.YARN_QC_ISSUE_CHILD} PRC ON PRC.QCIssueChildID = M.QCIssueChildID
			WHERE PRC.QCIssueMasterID = {id}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YarnQCIssueMaster data = await records.ReadFirstOrDefaultAsync<YarnQCIssueMaster>();
                Guard.Against.NullObject(data);
                data.YarnQCIssueChilds = records.Read<YarnQCIssueChild>().ToList();

                var rackBinMappingList = records.Read<YarnQCIssueChildRackBinMapping>().ToList();
                if (rackBinMappingList == null) rackBinMappingList = new List<YarnQCIssueChildRackBinMapping>();

                data.YarnQCIssueChilds.ForEach(c =>
                {
                    c.ChildRackBins = rackBinMappingList.Where(y => y.QCIssueChildID == c.QCIssueChildID).ToList();
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

        public async Task SaveAsync(YarnQCIssueMaster entity, List<YarnReceiveChildRackBin> rackBins)
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
                int maxYQCICRBId = 0;

                entity.YarnQCIssueChilds.ForEach(x =>
                {
                    maxYQCICRBId += x.ChildRackBins.Count(y => y.EntityState == EntityState.Added);
                });

                switch (entity.EntityState)
                {
                    case EntityState.Added:
                        entity.QCIssueMasterID = await _service.GetMaxIdAsync(TableNames.YARN_PR_MASTER, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        entity.QCIssueNo = await _service.GetMaxNoAsync(TableNames.YARN_QC_ISSUE_NO, 1, RepeatAfterEnum.NoRepeat, "00000", transactionGmt, _connectionGmt);

                        //maxChildId = await _service.GetMaxIdAsync(TableNames.YARN_QC_ISSUE_CHILD, entity.YarnQCIssueChilds.Count);
                        maxChildId = await _service.GetMaxIdAsync(TableNames.YARN_QC_ISSUE_CHILD, entity.YarnQCIssueChilds.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        //maxYQCICRBId = await _service.GetMaxIdAsync(TableNames.YARN_QC_ISSUE_CHILD_CHILD_RACK_BIN_MAPPING, maxYQCICRBId);
                        maxYQCICRBId = await _service.GetMaxIdAsync(TableNames.YARN_QC_ISSUE_CHILD_CHILD_RACK_BIN_MAPPING, maxYQCICRBId, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

                        foreach (YarnQCIssueChild item in entity.YarnQCIssueChilds)
                        {
                            item.QCIssueChildID = maxChildId++;
                            item.QCIssueMasterID = entity.QCIssueMasterID;
                            item.EntityState = EntityState.Added;

                            foreach (var itemObj in item.ChildRackBins)
                            {
                                itemObj.YQCICRBId = maxYQCICRBId++;
                                itemObj.QCIssueChildID = item.QCIssueChildID;
                                itemObj.EntityState = EntityState.Added;
                            }
                        }
                        break;

                    case EntityState.Modified:
                        maxChildId = await _service.GetMaxIdAsync(TableNames.YARN_QC_ISSUE_CHILD, entity.YarnQCIssueChilds.Count(x => x.EntityState == EntityState.Added), RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        maxYQCICRBId = await _service.GetMaxIdAsync(TableNames.YARN_QC_ISSUE_CHILD_CHILD_RACK_BIN_MAPPING, maxYQCICRBId, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

                        entity.YarnQCIssueChilds.ForEach(c =>
                        {
                            if (c.EntityState == EntityState.Added)
                            {
                                c.QCIssueChildID = maxChildId++;
                                c.QCIssueMasterID = entity.QCIssueMasterID;
                                c.EntityState = EntityState.Added;
                            }
                            foreach (var itemObj in c.ChildRackBins.Where(x => x.EntityState == EntityState.Added).ToList())
                            {
                                itemObj.YQCICRBId = maxYQCICRBId++;
                                itemObj.QCIssueChildID = c.QCIssueChildID;
                                itemObj.EntityState = EntityState.Added;
                            }
                        });
                        break;

                    case EntityState.Unchanged:
                    case EntityState.Deleted:
                        entity.EntityState = EntityState.Deleted;
                        entity.YarnQCIssueChilds.SetDeleted();
                        entity.YarnQCIssueChilds.ForEach(x => x.ChildRackBins.SetDeleted());
                        break;

                    default:
                        break;
                }

                List<YarnQCIssueChildRackBinMapping> rackBinList = new List<YarnQCIssueChildRackBinMapping>();
                entity.YarnQCIssueChilds.ForEach(x =>
                {
                    rackBinList.AddRange(x.ChildRackBins);
                });

                await _service.SaveSingleAsync(entity, transaction);
                await _service.SaveAsync(rackBinList.Where(x => x.EntityState == EntityState.Deleted).ToList(), transaction);
                await _service.SaveAsync(entity.YarnQCIssueChilds, transaction);
                await _service.SaveAsync(rackBinList.Where(x => x.EntityState != EntityState.Deleted).ToList(), transaction);

                await _service.SaveAsync(rackBins, transaction);

                #region Stock Operation
                //if (entity.Approve)
                //{
                //    int userId = entity.EntityState == EntityState.Added ? entity.AddedBy : (int)entity.UpdatedBy;
                //    await _connection.ExecuteAsync("spYarnStockOperation", new { MasterID = entity.QCIssueMasterID, FromMenuType = EnumFromMenuType.YarnQCIssue, UserId = userId }, transaction, 30, CommandType.StoredProcedure);
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
