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
    public class YarnQCReturnReceiveService : IYarnQCReturnReceiveService
    {
        private readonly IDapperCRUDService<YarnQCReturnReceivedMaster> _service;
        private readonly SqlConnection _connection;
        private readonly SqlConnection _connectionGmt;

        public YarnQCReturnReceiveService(IDapperCRUDService<YarnQCReturnReceivedMaster> service
            , IDapperCRUDService<YarnQCReturnReceivedMaster> itemMasterRepository)
        {
            _service = service;

            _service.Connection = service.GetConnection(AppConstants.GMT_CONNECTION);
            _connectionGmt = service.Connection;

            _service.Connection = service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _connection = service.Connection;
        }


        public async Task<List<YarnQCReturnReceivedMaster>> GetPagedAsync(Status status, int offset = 0, int limit = 10, string filterBy = null, string orderBy = null)
        {
            filterBy = string.IsNullOrEmpty(filterBy) ? string.Empty : "Where " + filterBy;
            orderBy = string.IsNullOrEmpty(orderBy) ? "ORDER BY QCReturnMasterID DESC" : orderBy;
            var pageBy = string.Format(@"OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", offset, limit);
            string sql;
            if (status == Status.Pending)
            {
                sql = $@"
                With
                RM As (
	                Select *
	                From {TableNames.YARN_QC_RETURN_MASTER} C
	                Where IsApprove = 0 
                )
                ,PR As (
	                Select RM.QCReturnMasterID,RM.QCReturnNo, RU.Name as QCReturnByUser, RM.QCReturnDate, RM.QCRemarksMasterID, RM.QCReqMasterID, REQ.QCReqNo, REQ.QCReqDate,REQ.QCReqBy, RCV.QCReceivedBy,RCV.QCReceiveDate
		                , SUM(RC.ReceiveQtyCarton) ReceiveQtyCarton, SUM(RC.ReceiveQtyCone) ReceiveQtyCone, SUM(RCVC.ReqQtyCone) ReqQty, SUM(RC.ReturnQtyCarton) ReturnQtyCarton, SUM(RC.ReturnQtyCone) ReturnQtyCone
	                From RM
	                inner JOIN {TableNames.YARN_QC_RETURN_CHILD} RC ON RM.QCReturnMasterID = RC.QCReturnMasterID
	                inner JOIN {TableNames.YARN_QC_REQ_MASTER} REQ ON RM.QCReqMasterID = REQ.QCReqMasterID
	                LEFT JOIN {TableNames.YARN_QC_REMARKS_MASTER} RMK ON RM.QCRemarksMasterID = RMK.QCRemarksMasterID
	                LEFT JOIN {TableNames.YARN_QC_RECEIVE_MASTER} RCV ON RMK.QCReceiveMasterID = RCV.QCReceiveMasterID
	                LEFT JOIN {TableNames.YARN_QC_RECEIVE_CHILD} RCVC ON RCV.QCReceiveMasterID = RCVC.QCReceiveMasterID
	                Inner Join {DbNames.EPYSL}..LoginUser RU On RM.QCReturnBy = RU.UserCode
	                left Join {TableNames.YARN_QC_RETURNRECEIVED_MASTER} RR On RM.QCReturnMasterID = RR.QCReturnMasterID
	                where IsNull(RR.QCReturnMasterID,0)= 0
	                GROUP BY RM.QCReturnMasterID,RM.QCReturnNo, RU.Name , RM.QCReturnDate, RM.QCRemarksMasterID, RM.QCReqMasterID, REQ.QCReqNo, REQ.QCReqDate,REQ.QCReqBy, RCV.QCReceivedBy,RCV.QCReceiveDate
                )

                Select QCReturnMasterID,QCReturnNo, QCReturnByUser,QCReturnDate,QCRemarksMasterID, QCReqMasterID, QCReqNo, QCReqDate,ReqQty, QCReqBy, QCReceivedBy, QCReceiveDate, ReceiveQtyCarton, ReceiveQtyCone,ReturnQtyCarton,ReturnQtyCone
                From PR";
            }
            else
            {



                sql = $@"
                  With
                RM As (
	                Select *
	                From {TableNames.YARN_QC_RETURNRECEIVED_MASTER} C
	                Where IsApprove = 0 
                )
                ,PR As (
	                Select RM.QCReturnReceivedMasterID, RM.QCReturnReceivedDate,RM.QCReturnReceivedBy,RRU.Name ReturnReceivedByUser, RM.QCReturnMasterID,RM.QCReturnNo, RU.Name as QCReturnByUser, RM.QCReturnDate, RM.QCRemarksMasterID, RM.QCReqMasterID, REQ.QCReqNo, REQ.QCReqDate,REQ.QCReqBy, RCV.QCReceivedBy,RCV.QCReceiveDate
		            , SUM(RC.ReceiveQty) ReceiveQty , SUM(RC.ReceiveQtyCarton) ReceiveQtyCarton, SUM(RC.ReceiveQtyCone) ReceiveQtyCone, SUM(RCVC.ReqQtyCone) ReqQty, SUM(RC.ReturnQty) ReturnQty, SUM(RC.ReturnQtyCarton) ReturnQtyCarton, SUM(RC.ReturnQtyCone) ReturnQtyCone
                    , YRM.ReceiveNo, QCIM.QCIssueNo,RCV.QCReceiveNo	                
                    From RM
	                inner JOIN {TableNames.YARN_QC_RETURNRECEIVED_CHILD} RC ON RM.QCReturnReceivedMasterID = RC.QCReturnReceivedMasterID
					inner JOIN {TableNames.YARN_QC_RETURN_CHILD} YQCRC ON YQCRC.QCReturnChildID = RC.QCReturnChildID
					LEFT JOIN {TableNames.YARN_QC_REMARKS_MASTER} RMK ON RM.QCRemarksMasterID = RMK.QCRemarksMasterID
					INNER JOIN {TableNames.YARN_QC_RECEIVE_CHILD} RCVC ON RCVC.QCReceiveChildID = YQCRC.QCReceiveChildID
					INNER JOIN {TableNames.YARN_QC_RECEIVE_MASTER} RCV ON RCV.QCReceiveMasterID = RCVC.QCReceiveMasterID 
					INNER JOIN {TableNames.YARN_QC_ISSUE_CHILD} QCIC On QCIC.QCIssueChildID = RCVC.QCIssueChildID
	                INNER JOIN {TableNames.YARN_QC_ISSUE_MASTER} QCIM On QCIM.QCIssueMasterID = QCIC.QCIssueMasterID
					INNER JOIN {TableNames.YARN_QC_REQ_CHILD} YQCReqC On YQCReqC.QCReqChildID = QCIC.QCReqChildID
					inner JOIN {TableNames.YARN_QC_REQ_MASTER} REQ ON REQ.QCReqMasterID = YQCReqC.QCReqMasterID
					INNER JOIN {TableNames.YARN_RECEIVE_CHILD} YRC On YRC.ChildID = YQCReqC.ReceiveChildID
					INNER JOIN {TableNames.YARN_RECEIVE_MASTER} YRM On YRM.ReceiveID = YRC.ReceiveID
					Inner Join {DbNames.EPYSL}..LoginUser RU On RM.QCReturnBy = RU.UserCode
					Inner Join {DbNames.EPYSL}..LoginUser RRU On RM.QCReturnReceivedBy = RRU.UserCode

	                GROUP BY RM.QCReturnReceivedMasterID, RM.QCReturnReceivedDate,RM.QCReturnReceivedBy,RRU.Name ,RM.QCReturnMasterID,RM.QCReturnNo, RU.Name , RM.QCReturnDate, RM.QCRemarksMasterID, RM.QCReqMasterID, REQ.QCReqNo, REQ.QCReqDate,REQ.QCReqBy, RCV.QCReceivedBy,RCV.QCReceiveDate
                    , YRM.ReceiveNo, QCIM.QCIssueNo,RCV.QCReceiveNo                
                )

                Select QCReturnReceivedMasterID, QCReturnReceivedDate,QCReturnReceivedBy,ReturnReceivedByUser,QCReturnMasterID ,QCReturnNo, QCReturnByUser,QCReturnDate,QCRemarksMasterID, QCReqMasterID, QCReqNo, QCReqDate,ReqQty, QCReqBy, QCReceivedBy, QCReceiveDate,ReceiveQty, ReceiveQtyCarton, ReceiveQtyCone,ReturnQty,ReturnQtyCarton,ReturnQtyCone
                ,ReceiveNo, QCIssueNo,QCReceiveNo                
                From PR";

            }

            sql += $@"{Environment.NewLine}{filterBy}{Environment.NewLine}{orderBy}{Environment.NewLine}{pageBy}";

            return await _service.GetDataAsync<YarnQCReturnReceivedMaster>(sql);
        }

        public async Task<YarnQCReturnReceivedMaster> GetNewAsync(int QCReturnMasterID)
        {
            var query =
                $@"
                With R As 
                (
	                Select QCRemarksMasterID,QCReqMasterID,QCReturnNo,QCReturnBy,QCReturnDate,QCReturnMasterID
	                From {TableNames.YARN_QC_RETURN_MASTER}
	                Where QCReturnMasterID = {QCReturnMasterID}
                )

                Select R.QCRemarksMasterID,QCReqFor.ValueName QCReqFor ,QCReceiveDate ,QCReceiveNo ,QCReqNo ,QCReqDate,R.QCReqMasterID,R.QCReturnNo,R.QCReturnDate,R.QCReturnMasterID,R.QCReturnBy
                From R
                LEFT JOIN {TableNames.YARN_QC_REMARKS_MASTER} RMK On R.QCRemarksMasterID = RMK.QCRemarksMasterID
                LEFT JOIN {TableNames.YARN_QC_RECEIVE_MASTER} RM On RMK.QCReceiveMasterID = RM.QCReceiveMasterID
                Inner Join {TableNames.YARN_QC_REQ_MASTER} RQ ON R.QCReqMasterID = RQ.QCReqMasterID
                Left Join {DbNames.EPYSL}..EntityTypeValue QCReqFor On RQ.QCForID = QCReqFor.ValueID;

                Select YIC.QCReturnChildID, 
                RM1.SupplierID, Supplier = CASE WHEN ISNULL(RM1.SupplierID,0) > 0 THEN Supplier.ShortName ELSE '' END,
                RC.SpinnerId, Spinner = CASE WHEN ISNULL(RC.SpinnerId,0) > 0 THEN Spinner.ShortName ELSE '' END
                ,YarnType.SegmentValue [YarnType], YarnCount.SegmentValue [YarnCount], YarnComposition.SegmentValue [YarnComposition]
                ,Shade.SegmentValue [Shade], YarnColor.SegmentValue [YarnColor], IM.ItemMasterID, RC.LotNo, RC.ChallanLot
                ,YIC.ReturnQtyCarton, YIC.ReturnQtyCone, RM.QCRemarksMasterID,YIC.ReturnQty
                ,RM1.LocationID, YIC.ReceiveChildID, RC.YarnCategory, RC.ChallanCount, RC.PhysicalCount, U.DisplayUnitDesc
                From {TableNames.YARN_QC_RETURN_CHILD} YIC
                INNER JOIN {TableNames.YARN_QC_RETURN_MASTER} RM on YIC.QCReturnMasterID = RM.QCReturnMasterID
				
				INNER JOIN {TableNames.YARN_QC_RECEIVE_CHILD} QCRC ON QCRC.QCReceiveChildID = YIC.QCReceiveChildID
				INNER JOIN {TableNames.YARN_QC_ISSUE_CHILD} QCIC ON QCIC.QCIssueChildID = QCRC.QCIssueChildID
				INNER JOIN {TableNames.YARN_QC_REQ_CHILD} QCRC1 ON QCRC1.QCReqChildID = QCIC.QCReqChildID

                INNER JOIN {TableNames.YARN_RECEIVE_CHILD} RC on RC.ChildID = QCRC1.ReceiveChildID
                INNER JOIN {TableNames.YARN_RECEIVE_MASTER} RM1 on RM1.ReceiveID = RC.ReceiveID

                INNER JOIN {DbNames.EPYSL}..Contacts Supplier On Supplier.ContactID = RM1.SupplierID
                INNER JOIN {DbNames.EPYSL}..Contacts Spinner On Spinner.ContactID = RC.SpinnerId
                LEFT JOIN {DbNames.EPYSL}..Unit U On U.UnitID = RC.UnitID
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM On IM.ItemMasterID = RC.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue YarnType On IM.Segment1ValueID = YarnType.SegmentValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue YarnCount On IM.Segment2ValueID = YarnCount.SegmentValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue YarnComposition On IM.Segment3ValueID = YarnComposition.SegmentValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Shade On IM.Segment4ValueID = Shade.SegmentValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue YarnColor On IM.Segment5ValueID = YarnColor.SegmentValueID
                Where YIC.QCReturnMasterID = {QCReturnMasterID}
                Group By YIC.QCReturnChildID, RM1.SupplierID, Supplier.ShortName, RC.SpinnerId, Spinner.ShortName
                ,YarnType.SegmentValue, YarnCount.SegmentValue, YarnComposition.SegmentValue
                ,Shade.SegmentValue, YarnColor.SegmentValue, IM.ItemMasterID, RC.LotNo, RC.ChallanLot, 
                YIC.ReceiveQtyCarton, YIC.ReceiveQtyCone, YIC.ReturnQtyCarton, YIC.ReturnQtyCone,YIC.Remarks, 
                RM.QCRemarksMasterID,YIC.ReturnQty,RM1.LocationID, YIC.ReceiveChildID, 
                RC.YarnCategory, RC.ChallanCount, RC.PhysicalCount, U.DisplayUnitDesc;

                --Locations
                SELECT id = L.LocationID, text = L.LocationName 
                FROM {TableNames.YARN_RECEIVE_CHILD_RACK_BIN} RB
                INNER JOIN {DbNames.EPYSL}..Location L ON L.LocationID = RB.LocationID
                GROUP BY L.LocationID, L.LocationName
                ORDER BY L.LocationName";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);

                YarnQCReturnReceivedMaster data = records.Read<YarnQCReturnReceivedMaster>().FirstOrDefault();
                data.Childs = records.Read<YarnQCReturnReceivedChild>().ToList();
                int qcReturnReceivedChildID = 1;
                data.Childs.ForEach(x =>
                {
                    x.QCReturnReceivedChildID = qcReturnReceivedChildID++;
                });
                data.LocationList = records.Read<Select2OptionModel>().ToList();
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _connection.Close();
            }
        }

        public async Task<YarnQCReturnReceivedMaster> GetAsync(int id)
        {
            var query =
                 $@"
                With R As 
                (
	                Select *
	                From {TableNames.YARN_QC_RETURNRECEIVED_MASTER}
	                Where QCReturnReceivedMasterID = {id}
                )

                Select R.QCReturnReceivedMasterID,QCReturnReceivedDate,R.QCReturnMasterID, QCReturnNo, QCreturnBy, QCReturnDate, RMK.QCReceiveMasterID,QCReqFor.ValueName QCReqFor ,QCReceiveDate ,QCReceiveNo ,QCReqNo ,QCReqDate,RMK.QCRemarksMasterID
                From R
				Inner Join {TableNames.YARN_QC_RETURNRECEIVED_CHILD} RC ON RC.QCReturnReceivedMasterID = R.QCReturnReceivedMasterID
				inner JOIN {TableNames.YARN_QC_RETURN_CHILD} YQCRC ON YQCRC.QCReturnChildID = RC.QCReturnChildID
				INNER JOIN {TableNames.YARN_QC_RECEIVE_CHILD} RCVC ON RCVC.QCReceiveChildID = YQCRC.QCReceiveChildID
                INNER JOIN {TableNames.YARN_QC_RECEIVE_MASTER} RM ON RM.QCReceiveMasterID = RCVC.QCReceiveMasterID 
				Left Join {TableNames.YARN_QC_REMARKS_CHILD} RMC On RMC.QCReceiveChildID = RCVC.QCReceiveChildID
                Left Join {TableNames.YARN_QC_REMARKS_MASTER} RMK On RMK.QCRemarksMasterID = RMC.QCRemarksMasterID
                INNER JOIN {TableNames.YARN_QC_ISSUE_CHILD} QCIC On QCIC.QCIssueChildID = RCVC.QCIssueChildID
	            INNER JOIN {TableNames.YARN_QC_ISSUE_MASTER} QCIM On QCIM.QCIssueMasterID = QCIC.QCIssueMasterID
				INNER JOIN {TableNames.YARN_QC_REQ_CHILD} YQCReqC On YQCReqC.QCReqChildID = QCIC.QCReqChildID
				inner JOIN {TableNames.YARN_QC_REQ_MASTER} RQ ON RQ.QCReqMasterID = YQCReqC.QCReqMasterID
                Left Join {DbNames.EPYSL}..EntityTypeValue QCReqFor On RQ.QCForID = QCReqFor.ValueID;

                Select YRC.QCReturnReceivedChildID, YRC.QCReturnChildID, 
                RM1.SupplierID, Supplier = CASE WHEN ISNULL(RM1.SupplierID,0) > 0 THEN Supplier.ShortName ELSE '' END, 
                RC1.SpinnerId, Spinner = CASE WHEN ISNULL(RC1.SpinnerId,0) > 0 THEN Spinner.ShortName ELSE '' END
                ,YarnType.SegmentValue [YarnType], YarnCount.SegmentValue [YarnCount], YarnComposition.SegmentValue [YarnComposition]
                ,Shade.SegmentValue [Shade], YarnColor.SegmentValue [YarnColor], IM.ItemMasterID, YRC.LotNo, YRC.ChallanLot
                ,YRC.ReceiveQty, YRC.ReceiveQtyCarton, YRC.ReceiveQtyCone, YRC.ReturnQty, YRC.ReturnQtyCarton, YRC.ReturnQtyCone, 
                YRC.Remarks,YR.QCRemarksMasterID,RM1.LocationID, RC.ReceiveChildID, RC1.YarnCategory, RC1.ChallanCount, RC1.PhysicalCount, U.DisplayUnitDesc
                From {TableNames.YARN_QC_RETURNRECEIVED_CHILD} YRC
                INNER JOIN {TableNames.YARN_QC_RETURNRECEIVED_MASTER} YRM ON YRM.QCReturnReceivedMasterID = YRC.QCReturnReceivedMasterID
                LEFT JOIN {TableNames.YARN_QC_RETURN_MASTER} YR on YRC.QCReturnMasterID = YRM.QCReturnMasterID
                INNER JOIN {TableNames.YARN_QC_RETURN_CHILD} RC on RC.QCReturnChildID = YRC.QCReturnChildID
                INNER JOIN {TableNames.YARN_RECEIVE_CHILD} RC1 on RC1.ChildID = RC.ReceiveChildID
                INNER JOIN {TableNames.YARN_RECEIVE_MASTER} RM1 on RM1.ReceiveID = RC1.ReceiveID
                LEFT JOIN {DbNames.EPYSL}..Unit U On U.UnitID = RC1.UnitID
                INNER JOIN {DbNames.EPYSL}..Contacts Supplier On Supplier.ContactID = RM1.SupplierID
                INNER JOIN {DbNames.EPYSL}..Contacts Spinner On Spinner.ContactID = RC1.SpinnerId
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM On YRC.ItemMasterID = IM.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue YarnType On IM.Segment1ValueID = YarnType.SegmentValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue YarnCount On IM.Segment2ValueID = YarnCount.SegmentValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue YarnComposition On IM.Segment3ValueID = YarnComposition.SegmentValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Shade On IM.Segment4ValueID = Shade.SegmentValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue YarnColor On IM.Segment5ValueID = YarnColor.SegmentValueID
                Where YRC.QCReturnReceivedMasterID = {id}
                Group By YRC.QCReturnReceivedChildID, YRC.QCReturnChildID, RM1.SupplierID, Supplier.ShortName, RC1.SpinnerId, Spinner.ShortName
                ,YarnType.SegmentValue, YarnCount.SegmentValue, YarnComposition.SegmentValue
                ,Shade.SegmentValue, YarnColor.SegmentValue, IM.ItemMasterID, YRC.LotNo, YRC.ChallanLot
                ,YRC.ReturnQty, YRC.ReturnQtyCone, YRC.ReturnQtyCarton, YRC.ReceiveQty, YRC.ReceiveQtyCone
                ,YRC.ReceiveQtyCarton,YRC.Remarks,YR.QCRemarksMasterID,RM1.LocationID, RC.ReceiveChildID,ISNULL(RC1.SpinnerId,0)
                ,RC1.YarnCategory, RC1.ChallanCount, RC1.PhysicalCount, U.DisplayUnitDesc;

                --Rack bins Mapping
                SELECT M.*
				FROM {TableNames.YARN_QC_RETURN_RECEIVE_CHILD_RACK_BIN_MAPPING} M
				INNER JOIN {TableNames.YARN_QC_RETURNRECEIVED_CHILD} PRC ON PRC.QCReturnReceivedChildID = M.QCReturnReceivedChildID
				WHERE PRC.QCReturnReceivedMasterID = {id};

                --Locations
                SELECT id = L.LocationID, text = L.LocationName 
                FROM {TableNames.YARN_RECEIVE_CHILD_RACK_BIN} RB
                INNER JOIN {DbNames.EPYSL}..Location L ON L.LocationID = RB.LocationID
                GROUP BY L.LocationID, L.LocationName
                ORDER BY L.LocationName";

            //var connection = _dbContext.Database.Connection;

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);

                YarnQCReturnReceivedMaster data = records.Read<YarnQCReturnReceivedMaster>().FirstOrDefault();
                data.Childs = records.Read<YarnQCReturnReceivedChild>().ToList();

                var rackBinMappingList = records.Read<YarnQCReturnReceiveChildRackBinMapping>().ToList();
                if (rackBinMappingList == null) rackBinMappingList = new List<YarnQCReturnReceiveChildRackBinMapping>();

                data.Childs.ForEach(c =>
                {
                    c.ChildRackBins = rackBinMappingList.Where(y => y.QCReturnReceivedChildID == c.QCReturnReceivedChildID).ToList();
                });
                data.LocationList = records.Read<Select2OptionModel>().ToList();

                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _connection.Close();
            }
        }
        public async Task<YarnQCReturnReceivedMaster> GetAllAsync(int id)
        {
            var sql = $@"
            ;Select * From {TableNames.YARN_QC_RETURNRECEIVED_MASTER} Where QCReturnReceivedMasterID = {id}

            ;Select * From {TableNames.YARN_QC_RETURNRECEIVED_CHILD} Where QCReturnReceivedMasterID = {id}

            --Rack bins Mapping
            ;SELECT M.*
		    FROM {TableNames.YARN_QC_RETURN_RECEIVE_CHILD_RACK_BIN_MAPPING} M
		    INNER JOIN {TableNames.YARN_QC_RETURNRECEIVED_CHILD} PRC ON PRC.QCReturnReceivedChildID = M.QCReturnReceivedChildID
		    WHERE PRC.QCReturnReceivedMasterID = {id}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YarnQCReturnReceivedMaster data = await records.ReadFirstOrDefaultAsync<YarnQCReturnReceivedMaster>();
                Guard.Against.NullObject(data);

                data.Childs = records.Read<YarnQCReturnReceivedChild>().ToList();

                var rackBinMappingList = records.Read<YarnQCReturnReceiveChildRackBinMapping>().ToList();
                if (rackBinMappingList == null) rackBinMappingList = new List<YarnQCReturnReceiveChildRackBinMapping>();

                data.Childs.ForEach(c =>
                {
                    c.ChildRackBins = rackBinMappingList.Where(y => y.QCReturnReceivedChildID == c.QCReturnReceivedChildID).ToList();
                });

                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _connection.Close();
            }
        }
        public async Task SaveAsync(YarnQCReturnReceivedMaster entity, List<YarnReceiveChildRackBin> rackBins)
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
                int maxYQCRRId = 0;
                //int maxHistoryId = await _signatureRepository.GetMaxIdAsync(TableNames.YARN_RECEIVE_CHILD_RACK_BIN_HISTORY, rackBinsHistories.Count()); ;

                entity.Childs.ForEach(x =>
                {
                    maxYQCRRId += x.ChildRackBins.Count(y => y.EntityState == EntityState.Added);
                });

                List<YarnQCReturnReceivedChild> childRecords = entity.Childs;

                switch (entity.EntityState)
                {
                    case EntityState.Added:
                        entity.QCReturnReceivedMasterID = await _service.GetMaxIdAsync(TableNames.YARN_QC_RETURNRECEIVED_MASTER);
                        maxChildId = await _service.GetMaxIdAsync(TableNames.YARN_QC_RETURNRECEIVED_CHILD, entity.Childs.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        maxYQCRRId = await _service.GetMaxIdAsync(TableNames.YARN_QC_RETURN_RECEIVE_CHILD_RACK_BIN_MAPPING, maxYQCRRId, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

                        foreach (var item in entity.Childs)
                        {
                            item.QCReturnReceivedChildID = maxChildId++;
                            item.QCReturnReceivedMasterID = entity.QCReturnReceivedMasterID;

                            foreach (var itemObj in item.ChildRackBins)
                            {
                                itemObj.YQCRRId = maxYQCRRId++;
                                itemObj.QCReturnReceivedChildID = item.QCReturnReceivedChildID;
                                itemObj.EntityState = EntityState.Added;
                            }
                        }
                        break;

                    case EntityState.Modified:
                        var addedChilds = entity.Childs.FindAll(x => x.EntityState == EntityState.Added);
                        maxChildId = await _service.GetMaxIdAsync(TableNames.YARN_QC_RETURNRECEIVED_CHILD, addedChilds.Count(x => x.EntityState == EntityState.Added), RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        maxYQCRRId = await _service.GetMaxIdAsync(TableNames.YARN_QC_ISSUE_CHILD_CHILD_RACK_BIN_MAPPING, maxYQCRRId, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

                        entity.Childs.ForEach(c =>
                        {
                            if (c.EntityState == EntityState.Added)
                            {
                                c.QCReturnReceivedChildID = maxChildId++;
                                c.QCReturnReceivedMasterID = entity.QCReturnReceivedMasterID;
                                c.EntityState = EntityState.Added;
                            }
                            foreach (var itemObj in c.ChildRackBins.Where(x => x.EntityState == EntityState.Added).ToList())
                            {
                                itemObj.YQCRRId = maxYQCRRId++;
                                itemObj.QCReturnReceivedChildID = c.QCReturnReceivedChildID;
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

                List<YarnQCReturnReceiveChildRackBinMapping> rackBinList = new List<YarnQCReturnReceiveChildRackBinMapping>();
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
                int userId = entity.EntityState == EntityState.Added ? entity.AddedBy : (int)entity.UpdatedBy;
                await _connection.ExecuteAsync("spYarnStockOperation", new { MasterID = entity.QCReturnReceivedMasterID, FromMenuType = EnumFromMenuType.YarnQCReturnReceive, UserId = userId }, transaction, 30, CommandType.StoredProcedure);
                #endregion Stock Operation

                transaction.Commit();
                transactionGmt.Commit();
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                if (ex.Message.Contains('~')) throw new Exception(ex.Message.Split('~')[0]);
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
