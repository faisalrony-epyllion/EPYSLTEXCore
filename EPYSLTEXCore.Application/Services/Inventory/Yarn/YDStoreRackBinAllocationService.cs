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
    class YDStoreRackBinAllocationService : IYDStoreRackBinAllocationService
    {
        private readonly IDapperCRUDService<YDStoreReceiveMaster> _service;
        private readonly SqlConnection _connection;
        private readonly SqlConnection _connectionGmt;

        public YDStoreRackBinAllocationService(IDapperCRUDService<YDStoreReceiveMaster> service)
        {
            _service = service;
            _service.Connection = service.GetConnection(AppConstants.GMT_CONNECTION);
            _connectionGmt = service.Connection;

            _service.Connection = service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _connection = service.Connection;
        }

        public async Task<List<YDStoreReceiveMaster>> GetPagedAsync(Status status, bool isCDAPage, PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By YDStoreReceiveDate Desc" : paginationInfo.OrderBy;
            var sql = string.Empty;
            if (status == Status.Pending)
            {

                sql += $@"With M AS(
                SELECT RM.YDStoreReceiveMasterID, RM.YDStoreReceiveDate, RM.YDStoreReceiveNo, RM.LocationID, RM.CompanyID, RM.SupplierID, 
                RM.Remarks, SpinnerID = 0, RM.YDStoreReceiveBy
                FROM YDStoreReceiveMaster RM Where RM.PartialAllocation = 0 AND RM.CompleteAllocation=0
                )
                SELECT M.YDStoreReceiveMasterID, M.YDStoreReceiveDate, M.YDStoreReceiveNo, M.LocationID, M.CompanyID, M.SupplierID, M.Remarks, M.SpinnerID, 
                M.YDStoreReceiveBy, CC.[Name] SupplierName, COM.CompanyName CompanyName,
                Count(*) Over() TotalRows   
		        FROM M
		        LEFT JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = M.SupplierID
				LEFT JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID = M.CompanyID";

            }
            else if (status == Status.PartiallyCompleted)
            {
                sql += $@"With M AS(
                SELECT RM.YDStoreReceiveMasterID, RM.YDStoreReceiveDate, RM.YDStoreReceiveNo, RM.LocationID, RM.CompanyID, RM.SupplierID,
				RM.Remarks, SpinnerID = 0, RM.YDStoreReceiveBy
                FROM YDStoreReceiveMaster RM WHERE RM.PartialAllocation = 1 AND RM.CompleteAllocation=0
                )
                SELECT M.YDStoreReceiveMasterID, M.YDStoreReceiveDate, M.YDStoreReceiveNo, M.LocationID, M.CompanyID, M.SupplierID, M.Remarks, M.SpinnerID, 
                M.YDStoreReceiveBy, CC.[Name] SupplierName, COM.CompanyName CompanyName,
                Count(*) Over() TotalRows   
		        FROM M
		        LEFT JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = M.SupplierID
				LEFT JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID = M.CompanyID";
            }
            else if (status == Status.Completed)
            {
                sql += $@"With M AS(
                SELECT RM.YDStoreReceiveMasterID, RM.YDStoreReceiveDate, RM.YDStoreReceiveNo, RM.LocationID, RM.CompanyID, RM.SupplierID,
				RM.Remarks, SpinnerID = 0, RM.YDStoreReceiveBy
                FROM YDStoreReceiveMaster RM WHERE RM.PartialAllocation = 0 AND RM.CompleteAllocation = 1
                )
                SELECT M.YDStoreReceiveMasterID, M.YDStoreReceiveDate, M.YDStoreReceiveNo, M.LocationID, M.CompanyID, M.SupplierID, M.Remarks, M.SpinnerID, 
                M.YDStoreReceiveBy, CC.[Name] SupplierName, COM.CompanyName CompanyName,
                Count(*) Over() TotalRows   
		        FROM M
		        LEFT JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = M.SupplierID
				LEFT JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID = M.CompanyID";
            }

            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";
            return await _service.GetDataAsync<YDStoreReceiveMaster>(sql);
        }

        public async Task<YDStoreReceiveMaster> GetAsync(int id, int companyId)
        {
            var sql = $@"
                ;WITH M AS (
	                SELECT RM.YDStoreReceiveMasterID, RM.YDStoreReceiveDate, RM.YDStoreReceiveNo, RM.LocationID, RM.CompanyID, RM.SupplierID,
	                RM.Remarks, SpinnerID = 0,RM.YDStoreReceiveBy
	                FROM YDStoreReceiveMaster RM WHERE RM.YDStoreReceiveMasterID = {id}
                )
                SELECT	M.YDStoreReceiveMasterID, M.YDStoreReceiveDate, M.YDStoreReceiveNo, M.CompanyID, M.SupplierID,
                M.Remarks, M.SpinnerID, M.YDStoreReceiveBy,
                CC.[Name] SupplierName,COM.CompanyName RCompany,
                RBy.[Name] ReceivedBy, SPinner.[Name] SpinnerName,
                M.LocationID, LocationName = Loc.LocationName
                FROM M
                INNER JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = M.SupplierID
                LEFT JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID = M.CompanyID
                LEFT JOIN {DbNames.EPYSL}..[location] Loc ON Loc.LocationID = M.LocationId
                LEFT Join {DbNames.EPYSL}..LoginUser RBy On RBy.UserCode = M.YDStoreReceiveBy
                INNER JOIN {DbNames.EPYSL}..Contacts SPinner ON SPinner.ContactID = M.SpinnerID;
                
                -----childs
                ;WITH X AS (
	                SELECT RC.YDStoreReceiveChildID, RC.YDStoreReceiveMasterID, RC.ItemMasterID, RC.UnitID,
					RC.ReceiveQty, RC.Remarks, RC.ReceiveCarton, RC.ReceiveCone, RC.SupplierID, RC.SpinnerID, RC.LotNo, RC.PhysicalCount, RC.ShadeCode, RC.BookingID, RC.YarnCategory
	                FROM YDStoreReceiveChild RC 
					INNER JOIN YDBatchItemRequirement IR ON IR.YDBItemReqID = RC.YDBItemReqID
					INNER JOIN YDBookingChild YDBC ON YDBC.YDBookingChildID = IR.YDBookingChildID 
					INNER JOIN YDReqChild YDRC ON YDRC.YDBookingChildID = YDBC.YDBookingChildID
					INNER JOIN YDReqIssueChild IC ON IC.YDReqChildID = YDRC.YDReqChildID
                    WHERE RC.YDStoreReceiveMasterID = {id}
                ), Spiner As(
					Select RC.YDStoreReceiveChildID,
					SpinnerName = C.ShortName
					From {DbNames.EPYSL}..ContactSpinnerSetup CS
					Inner Join YDStoreReceiveChild RC On RC.SpinnerID = CS.SpinnerID
					Inner Join {DbNames.EPYSL}..Contacts C ON C.ContactID = CS.SpinnerID
					 WHERE RC.YDStoreReceiveMasterID = {id}

				)
                SELECT X.YDStoreReceiveChildID, X.YDStoreReceiveMasterID, X.ItemMasterID, X.UnitID, X.ReceiveQty, 
				X.Remarks, X.ReceiveCarton, X.ReceiveCone, UU.DisplayUnitDesc, ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, 
                    ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, 
                    ISV7.SegmentValue Segment7ValueDesc, ISV8.SegmentValue Segment8ValueDesc, S.SpinnerName, X.SupplierID, X.SpinnerID, X.LotNo, X.PhysicalCount, X.ShadeCode, X.BookingID, X.YarnCategory
                FROM X
                Inner Join {DbNames.EPYSL}..ItemMaster AS IM On IM.ItemMasterID = X.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV8 ON ISV8.SegmentValueID = IM.Segment8ValueID
	            Left Join Spiner S on S.YDStoreReceiveChildID = X.YDStoreReceiveChildID
                LEFT Join {DbNames.EPYSL}..Unit AS UU On UU.UnitID = X.UnitID
                Group By X.YDStoreReceiveChildID, X.YDStoreReceiveMasterID, X.ItemMasterID, X.UnitID, X.ReceiveQty, 
				X.Remarks, X.ReceiveCarton, X.ReceiveCone, UU.DisplayUnitDesc, ISV1.SegmentValue, ISV2.SegmentValue, 
                    ISV3.SegmentValue, ISV4.SegmentValue, ISV5.SegmentValue, ISV6.SegmentValue, 
                    ISV7.SegmentValue, ISV8.SegmentValue, S.SpinnerName, X.SupplierID, X.SpinnerID, X.LotNo, X.PhysicalCount, X.ShadeCode, X.BookingID, X.YarnCategory;

                --Rack
                SELECT RB.*, MinNoOfCartoon = RB.NoOfCartoon, MinNoOfCone = RB.NoOfCone, MinReceiveQty = RB.ReceiveQty,RC.RackNo 
                FROM YDStoreReceiveChildRackBin RB
                INNER JOIN YDStoreReceiveChild YRC ON YRC.YDStoreReceiveChildID = RB.ChildID
                INNER JOIN YDStoreReceiveMaster YRM ON YRM.YDStoreReceiveMasterID = YRC.YDStoreReceiveMasterID
                LEFT JOIN {DbNames.EPYSL}..Rack RC ON RC.RackID = RB.RackID
                WHERE YRM.YDStoreReceiveMasterID = {id}

                ----LocationList
                {CommonQueries.GetLocationListByCompanyID(companyId, true)};

                ----RackList
                {CommonQueries.GetRackListByCompanyID(companyId, "Y")};

                ----Employee List
                {CommonQueries.GetEmployeeByDepartmentAndSectionList("Material Control", "Yarn Store")};
                ";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YDStoreReceiveMaster data = records.Read<YDStoreReceiveMaster>().FirstOrDefault();
                Guard.Against.NullObject(data);
                data.Childs = records.Read<YDStoreReceiveChild>().ToList();
                List<YDStoreReceiveChildRackBin> racks = records.Read<YDStoreReceiveChildRackBin>().ToList();
                data.LocationList = records.Read<Select2OptionModel>().ToList();
                data.RackList = records.Read<Select2OptionModel>().ToList();
                data.EmployeeList = records.Read<Select2OptionModel>().ToList();

                data.Childs.ForEach(c =>
                {
                    c.YDStoreReceiveChildRackBins = racks.Where(r => r.ChildID == c.YDStoreReceiveChildID).ToList();
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
        public async Task<List<YDStoreReceiveChildRackBin>> GetYarnReceiveChildRackBinData(int childId)  //List<YarnReceiveChildRackBin>
        {
            var sql = string.Empty;
            sql += $@"
                SELECT a.ChildRackBinID, a.ChildID, a.LocationID, a.RackID, a.BinID, a.NoOfCartoon, a.NoOfCone, a.ReceiveQty, a.Remarks, a.EmployeeID--, Rack.RackNo, RackBin.BinNo
                FROM YDStoreReceiveChildRackBin a
                --LEFT JOIN {DbNames.EPYSL}..Rack ON Rack.RackID = a.RackID
                --LEFT JOIN {DbNames.EPYSL}..RackBin ON RackBin.BinID = a.BinID
                WHERE ChildID={childId}";
            return await _service.GetDataAsync<YDStoreReceiveChildRackBin>(sql);
        }

        public async Task<List<YDStoreReceiveChildRackBin>> GetRackBin(int childId, int locationId, int qcReturnReceivedChildId = 0)  //List<YarnReceiveChildRackBin>
        {
            string sql = "";
            if (qcReturnReceivedChildId > 0)
            {
                sql = $@"
                WITH
                FinalList AS
                (
	                SELECT a.ChildRackBinID, a.ChildID, a.LocationID, a.RackID, a.BinID, 
	                a.NoOfCartoon, a.NoOfCone, a.ReceiveQty, 
	                Rack.RackNo, RackBin.BinNo, LocationName = L.ShortName
	                FROM YDStoreReceiveChildRackBin a
                    LEFT JOIN YDStoreReceiveChild YRC ON YRC.YDStoreReceiveChildID = a.ChildID
	                LEFT JOIN {DbNames.EPYSL}..Rack ON Rack.RackID = a.RackID
	                LEFT JOIN {DbNames.EPYSL}..RackBin ON RackBin.BinID = a.BinID
	                LEFT JOIN {DbNames.EPYSL}..Location L ON L.LocationID = a.LocationID
	                WHERE a.ChildID = {childId}
                )
                
                SELECT * FROM FinalList ORDER BY LocationName, RackNo";
            }
            else
            {
                sql = $@"
                SELECT a.ChildRackBinID, a.ChildID, a.LocationID, a.RackID, a.BinID, 
                a.NoOfCartoon, a.NoOfCone, a.ReceiveQty, 
                Rack.RackNo, RackBin.BinNo, LocationName = L.ShortName
                FROM YDStoreReceiveChildRackBin a
                LEFT JOIN YDStoreReceiveChild YRC ON YRC.YDStoreReceiveChildID = a.ChildID
                LEFT JOIN {DbNames.EPYSL}..Rack ON Rack.RackID = a.RackID
                LEFT JOIN {DbNames.EPYSL}..RackBin ON RackBin.BinID = a.BinID
                LEFT JOIN {DbNames.EPYSL}..Location L ON L.LocationID = a.LocationID
                WHERE a.ChildID = {childId}
                ORDER BY L.ShortName, Rack.RackNo";
            }
            return await _service.GetDataAsync<YDStoreReceiveChildRackBin>(sql);
        }
        public async Task<List<YDStoreReceiveChildRackBin>> GetRackBinForKnittingReturnRcv(int childId, int locationId, int kReturnReceivedChildId = 0)  //List<YarnReceiveChildRackBin>
        {
            string sql = "";
            if (kReturnReceivedChildId > 0)
            {
                sql = $@"
                WITH
                MailList AS
                (
	                SELECT a.ChildRackBinID, a.ChildID, a.LocationID, a.RackID, a.BinID, 
	                a.NoOfCartoon, a.NoOfCone, a.ReceiveQty, 
	                Rack.RackNo, RackBin.BinNo, LocationName = L.ShortName,
                    YarnStockSetId = 0
	                FROM YDStoreReceiveChildRackBin a
	                LEFT JOIN {DbNames.EPYSL}..Rack ON Rack.RackID = a.RackID
	                LEFT JOIN {DbNames.EPYSL}..RackBin ON RackBin.BinID = a.BinID
	                LEFT JOIN {DbNames.EPYSL}..Location L ON L.LocationID = a.LocationID
	                WHERE a.ChildID = {childId}
                ),
                QCRRList AS
                (
	                SELECT a.ChildRackBinID, a.ChildID, a.LocationID, a.RackID, a.BinID, 
	                a.NoOfCartoon, a.NoOfCone, ReceiveQty = YSM.AllocatedStockQty, 
	                Rack.RackNo, RackBin.BinNo, LocationName = L.ShortName,
                    M.YarnStockSetId
	                FROM YDStoreReceiveChildRackBin a
	                LEFT JOIN {DbNames.EPYSL}..Rack ON Rack.RackID = a.RackID
	                LEFT JOIN {DbNames.EPYSL}..RackBin ON RackBin.BinID = a.BinID
	                LEFT JOIN {DbNames.EPYSL}..Location L ON L.LocationID = a.LocationID
	                INNER JOIN KYLOReturnReceiveChildRackBinMapping M ON M.ChildRackBinID = a.ChildRackBinID
                    LEFT JOIN YarnStockMaster YSM ON YSM.YarnStockSetId = M.YarnStockSetId
	                LEFT JOIN MailList ML ON ML.ChildRackBinID = M.ChildRackBinID
	                WHERE M.KYLOReturnReceiveChildId = {kReturnReceivedChildId} AND M.IsUsable=1 AND ML.ChildRackBinID IS NULL
                ),
                FinalList AS
                (
	                SELECT * FROM MailList
	                UNION
	                SELECT * FROM QCRRList
                )
                SELECT * FROM FinalList ORDER BY LocationName, RackNo";
            }
            else
            {
                sql = $@"
                SELECT a.ChildRackBinID, a.ChildID, a.LocationID, a.RackID, a.BinID, 
                a.NoOfCartoon, a.NoOfCone, a.ReceiveQty, 
                Rack.RackNo, RackBin.BinNo, LocationName = L.ShortName
                FROM YDStoreReceiveChildRackBin a
                LEFT JOIN {DbNames.EPYSL}..Rack ON Rack.RackID = a.RackID
                LEFT JOIN {DbNames.EPYSL}..RackBin ON RackBin.BinID = a.BinID
                LEFT JOIN {DbNames.EPYSL}..Location L ON L.LocationID = a.LocationID
                WHERE a.ChildID = {childId}
                ORDER BY L.ShortName, Rack.RackNo";
            }
            return await _service.GetDataAsync<YDStoreReceiveChildRackBin>(sql);
        }
        public async Task<List<YDStoreReceiveChildRackBin>> GetRackBinForYDReturnRcv(int childId, int locationId, int kReturnReceivedChildId = 0)
        {
            string sql = "";
            if (kReturnReceivedChildId > 0)
            {
                sql = $@"
                WITH
                MailList AS
                (
	                SELECT a.ChildRackBinID, a.ChildID, a.LocationID, a.RackID, a.BinID, 
	                a.NoOfCartoon, a.NoOfCone, a.ReceiveQty, 
	                Rack.RackNo, RackBin.BinNo, LocationName = L.ShortName,
                    YarnStockSetId = 0
	                FROM YDStoreReceiveChildRackBin a
	                LEFT JOIN {DbNames.EPYSL}..Rack ON Rack.RackID = a.RackID
	                LEFT JOIN {DbNames.EPYSL}..RackBin ON RackBin.BinID = a.BinID
	                LEFT JOIN {DbNames.EPYSL}..Location L ON L.LocationID = a.LocationID
	                WHERE a.ChildID = {childId}
                ),
                QCRRList AS
                (
	                SELECT a.ChildRackBinID, a.ChildID, a.LocationID, a.RackID, a.BinID, 
	                a.NoOfCartoon, a.NoOfCone, ReceiveQty = YSM.AllocatedStockQty, 
	                Rack.RackNo, RackBin.BinNo, LocationName = L.ShortName,
                    M.YarnStockSetId
	                FROM YDStoreReceiveChildRackBin a
	                LEFT JOIN {DbNames.EPYSL}..Rack ON Rack.RackID = a.RackID
	                LEFT JOIN {DbNames.EPYSL}..RackBin ON RackBin.BinID = a.BinID
	                LEFT JOIN {DbNames.EPYSL}..Location L ON L.LocationID = a.LocationID
	                INNER JOIN YDLeftOverReturnReceiveChildRackBinMapping M ON M.ChildRackBinID = a.ChildRackBinID
                    LEFT JOIN YarnStockMaster YSM ON YSM.YarnStockSetId = M.YarnStockSetId
	                LEFT JOIN MailList ML ON ML.ChildRackBinID = M.ChildRackBinID
	                WHERE M.YDLeftOverReturnReceiveChildId = {kReturnReceivedChildId} AND M.IsUsable=1 AND ML.ChildRackBinID IS NULL
                ),
                FinalList AS
                (
	                SELECT * FROM MailList
	                UNION
	                SELECT * FROM QCRRList
                )
                SELECT * FROM FinalList ORDER BY LocationName, RackNo";
            }
            else
            {
                sql = $@"
                SELECT a.ChildRackBinID, a.ChildID, a.LocationID, a.RackID, a.BinID, 
                a.NoOfCartoon, a.NoOfCone, a.ReceiveQty, 
                Rack.RackNo, RackBin.BinNo, LocationName = L.ShortName
                FROM YDStoreReceiveChildRackBin a
                LEFT JOIN {DbNames.EPYSL}..Rack ON Rack.RackID = a.RackID
                LEFT JOIN {DbNames.EPYSL}..RackBin ON RackBin.BinID = a.BinID
                LEFT JOIN {DbNames.EPYSL}..Location L ON L.LocationID = a.LocationID
                WHERE a.ChildID = {childId}
                ORDER BY L.ShortName, Rack.RackNo";
            }
            return await _service.GetDataAsync<YDStoreReceiveChildRackBin>(sql);
        }
        public async Task<List<YDStoreReceiveChildRackBin>> GetRackBinForSCReturnRcv(int childId, int locationId, int kReturnReceivedChildId = 0)
        {
            string sql = "";
            if (kReturnReceivedChildId > 0)
            {
                sql = $@"
                    SELECT YSS.ItemMasterID,YSS.YarnStockSetId, YSS.SupplierID,YSS.SpinnerID,LotNo = YSS.YarnLotNo,YSS.ShadeCode,YSS.PhysicalCount,
					a.ChildRackBinID, a.ChildID, a.LocationID, a.RackID, a.BinID, 
	                a.NoOfCartoon, a.NoOfCone, ReceiveQty = YSM.AllocatedStockQty, 
	                Rack.RackNo, RackBin.BinNo, LocationName = L.ShortName, M.YarnStockSetId,
					Rate = ISNULL(YSC.Rate,ISNULL(POC.Rate,0)),
					SpinnerName = C.ShortName, YRC.YarnControlNo, RackQty = a.ReceiveQty,
					AvgCartoonWeight = Cast(ROUND(YRC.ReceiveQty/YRC.NoOfCartoon,2) AS DECIMAL(18, 2))
	                FROM YDStoreReceiveChildRackBin a
					INNER JOIN KSCLOReturnReceiveChildRackBinMapping M ON M.ChildRackBinID = a.ChildRackBinID
					INNER JOIN YDStoreReceiveChild YRC ON YRC.ChildID = a.ChildID
					INNER JOIN YDStoreReceiveMaster YRM ON YRM.ReceiveID = YRC.ReceiveID
	                LEFT JOIN YarnPOChild POC ON POC.YPOChildID = YRC.POChildID
					LEFT JOIN YarnStockChild YSC ON YSC.YarnStockSetId = M.YarnStockSetId AND YSC.StockFromTableId = 2 AND YSC.StockFromPKId = YRC.ChildID
	                LEFT JOIN {DbNames.EPYSL}..Rack ON Rack.RackID = a.RackID
	                LEFT JOIN {DbNames.EPYSL}..RackBin ON RackBin.BinID = a.BinID
	                LEFT JOIN {DbNames.EPYSL}..Location L ON L.LocationID = a.LocationID
                    LEFT JOIN YarnStockMaster YSM ON YSM.YarnStockSetId = M.YarnStockSetId
					INNER JOIN YarnStockSet YSS ON YSS.YarnStockSetId = M.YarnStockSetId
					LEFT JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = YSS.SpinnerId
	                WHERE M.KSCLOReturnReceiveChildId = {kReturnReceivedChildId} AND M.IsUsable=1
					ORDER BY ChildID DESC;

	            /*WITH
                MailList AS
                (
	                SELECT a.ChildRackBinID, a.ChildID, a.LocationID, a.RackID, a.BinID, 
	                a.NoOfCartoon, a.NoOfCone, a.ReceiveQty, 
	                Rack.RackNo, RackBin.BinNo, LocationName = L.ShortName,
                    YarnStockSetId = 0
	                FROM YDStoreReceiveChildRackBin a
	                LEFT JOIN {DbNames.EPYSL}..Rack ON Rack.RackID = a.RackID
	                LEFT JOIN {DbNames.EPYSL}..RackBin ON RackBin.BinID = a.BinID
	                LEFT JOIN {DbNames.EPYSL}..Location L ON L.LocationID = a.LocationID
	                WHERE a.ChildID = {childId}
                ),
                QCRRList AS
                (
	                SELECT a.ChildRackBinID, a.ChildID, a.LocationID, a.RackID, a.BinID, 
	                a.NoOfCartoon, a.NoOfCone, ReceiveQty = YSM.AllocatedStockQty, 
	                Rack.RackNo, RackBin.BinNo, LocationName = L.ShortName,
                    M.YarnStockSetId
	                FROM YDStoreReceiveChildRackBin a
	                LEFT JOIN {DbNames.EPYSL}..Rack ON Rack.RackID = a.RackID
	                LEFT JOIN {DbNames.EPYSL}..RackBin ON RackBin.BinID = a.BinID
	                LEFT JOIN {DbNames.EPYSL}..Location L ON L.LocationID = a.LocationID
	                INNER JOIN KSCLOReturnReceiveChildRackBinMapping M ON M.ChildRackBinID = a.ChildRackBinID
                    LEFT JOIN YarnStockMaster YSM ON YSM.YarnStockSetId = M.YarnStockSetId
	                LEFT JOIN MailList ML ON ML.ChildRackBinID = M.ChildRackBinID
	                WHERE M.KSCLOReturnReceiveChildId = {kReturnReceivedChildId} AND M.IsUsable=1 AND ML.ChildRackBinID IS NULL
                ),
                FinalList AS
                (
	                SELECT * FROM MailList
	                UNION
	                SELECT * FROM QCRRList
                )
                SELECT * FROM FinalList ORDER BY ChildID DESC--LocationName, RackNo*/";
            }
            else
            {
                sql = $@"
                SELECT a.ChildRackBinID, a.ChildID, a.LocationID, a.RackID, a.BinID, 
                a.NoOfCartoon, a.NoOfCone, a.ReceiveQty, 
                Rack.RackNo, RackBin.BinNo, LocationName = L.ShortName
                FROM YDStoreReceiveChildRackBin a
                LEFT JOIN {DbNames.EPYSL}..Rack ON Rack.RackID = a.RackID
                LEFT JOIN {DbNames.EPYSL}..RackBin ON RackBin.BinID = a.BinID
                LEFT JOIN {DbNames.EPYSL}..Location L ON L.LocationID = a.LocationID
                WHERE a.ChildID = {childId}
                ORDER BY L.ShortName, Rack.RackNo";
            }
            return await _service.GetDataAsync<YDStoreReceiveChildRackBin>(sql);
        }
        public async Task<List<YDStoreReceiveChildRackBin>> GetRackBinForRNDReturnRcv(int childId, int locationId, int kReturnReceivedChildId = 0)
        {
            string sql = "";
            if (kReturnReceivedChildId > 0)
            {
                sql = $@"
                WITH
                MailList AS
                (
	                SELECT a.ChildRackBinID, a.ChildID, a.LocationID, a.RackID, a.BinID, 
	                a.NoOfCartoon, a.NoOfCone, a.ReceiveQty, 
	                Rack.RackNo, RackBin.BinNo, LocationName = L.ShortName,
                    YarnStockSetId = 0
	                FROM YDStoreReceiveChildRackBin a
	                LEFT JOIN {DbNames.EPYSL}..Rack ON Rack.RackID = a.RackID
	                LEFT JOIN {DbNames.EPYSL}..RackBin ON RackBin.BinID = a.BinID
	                LEFT JOIN {DbNames.EPYSL}..Location L ON L.LocationID = a.LocationID
	                WHERE a.ChildID = {childId}
                ),
                QCRRList AS
                (
	                SELECT a.ChildRackBinID, a.ChildID, a.LocationID, a.RackID, a.BinID, 
	                a.NoOfCartoon, a.NoOfCone, ReceiveQty = YSM.AllocatedStockQty, 
	                Rack.RackNo, RackBin.BinNo, LocationName = L.ShortName,
                    M.YarnStockSetId
	                FROM YDStoreReceiveChildRackBin a
	                LEFT JOIN {DbNames.EPYSL}..Rack ON Rack.RackID = a.RackID
	                LEFT JOIN {DbNames.EPYSL}..RackBin ON RackBin.BinID = a.BinID
	                LEFT JOIN {DbNames.EPYSL}..Location L ON L.LocationID = a.LocationID
	                INNER JOIN YarnRNDReturnReceiveChildRackBinMapping M ON M.ChildRackBinID = a.ChildRackBinID
                    LEFT JOIN YarnStockMaster YSM ON YSM.YarnStockSetId = M.YarnStockSetId
	                LEFT JOIN MailList ML ON ML.ChildRackBinID = M.ChildRackBinID
	                WHERE M.RNDLOReturnReceiveChildId = {kReturnReceivedChildId} AND M.IsUsable=1 AND ML.ChildRackBinID IS NULL
                ),
                FinalList AS
                (
	                SELECT * FROM MailList
	                UNION
	                SELECT * FROM QCRRList
                )
                SELECT * FROM FinalList ORDER BY LocationName, RackNo";
            }
            else
            {
                sql = $@"
                SELECT a.ChildRackBinID, a.ChildID, a.LocationID, a.RackID, a.BinID, 
                a.NoOfCartoon, a.NoOfCone, a.ReceiveQty, 
                Rack.RackNo, RackBin.BinNo, LocationName = L.ShortName
                FROM YDStoreReceiveChildRackBin a
                LEFT JOIN {DbNames.EPYSL}..Rack ON Rack.RackID = a.RackID
                LEFT JOIN {DbNames.EPYSL}..RackBin ON RackBin.BinID = a.BinID
                LEFT JOIN {DbNames.EPYSL}..Location L ON L.LocationID = a.LocationID
                WHERE a.ChildID = {childId}
                ORDER BY L.ShortName, Rack.RackNo";
            }
            return await _service.GetDataAsync<YDStoreReceiveChildRackBin>(sql);
        }
        public async Task<List<YDStoreReceiveChildRackBin>> GetRackBinForKnittingReturnRcvUnusable(int childId, int locationId, int kReturnReceivedChildId = 0)  //List<YarnReceiveChildRackBin>
        {
            string sql = "";
            if (kReturnReceivedChildId > 0)
            {
                sql = $@"
                WITH
                MailList AS
                (
	                SELECT a.ChildRackBinID, a.ChildID, a.LocationID, a.RackID, a.BinID, 
	                a.NoOfCartoon, a.NoOfCone, a.ReceiveQty, 
	                Rack.RackNo, RackBin.BinNo, LocationName = L.ShortName,
                    YarnStockSetId = 0
	                FROM YDStoreReceiveChildRackBin a
	                LEFT JOIN {DbNames.EPYSL}..Rack ON Rack.RackID = a.RackID
	                LEFT JOIN {DbNames.EPYSL}..RackBin ON RackBin.BinID = a.BinID
	                LEFT JOIN {DbNames.EPYSL}..Location L ON L.LocationID = a.LocationID
	                WHERE a.ChildID = {childId}
                ),
                QCRRList AS
                (
	                SELECT a.ChildRackBinID, a.ChildID, a.LocationID, a.RackID, a.BinID, 
	                a.NoOfCartoon, a.NoOfCone, ReceiveQty = YSM.AllocatedStockQty,  
	                Rack.RackNo, RackBin.BinNo, LocationName = L.ShortName,
                    M.YarnStockSetId
	                FROM YDStoreReceiveChildRackBin a
	                LEFT JOIN {DbNames.EPYSL}..Rack ON Rack.RackID = a.RackID
	                LEFT JOIN {DbNames.EPYSL}..RackBin ON RackBin.BinID = a.BinID
	                LEFT JOIN {DbNames.EPYSL}..Location L ON L.LocationID = a.LocationID
	                INNER JOIN KYLOReturnReceiveChildRackBinMapping M ON M.ChildRackBinID = a.ChildRackBinID
                    LEFT JOIN YarnStockMaster YSM ON YSM.YarnStockSetId = M.YarnStockSetId
	                LEFT JOIN MailList ML ON ML.ChildRackBinID = M.ChildRackBinID
	                WHERE M.KYLOReturnReceiveChildId = {kReturnReceivedChildId} AND M.IsUsable=0 AND ML.ChildRackBinID IS NULL
                ),
                FinalList AS
                (
	                SELECT * FROM MailList
	                UNION
	                SELECT * FROM QCRRList
                )
                SELECT * FROM FinalList ORDER BY LocationName, RackNo";
            }
            else
            {
                sql = $@"
                SELECT a.ChildRackBinID, a.ChildID, a.LocationID, a.RackID, a.BinID, 
                a.NoOfCartoon, a.NoOfCone, a.ReceiveQty, 
                Rack.RackNo, RackBin.BinNo, LocationName = L.ShortName
                FROM YDStoreReceiveChildRackBin a
                LEFT JOIN {DbNames.EPYSL}..Rack ON Rack.RackID = a.RackID
                LEFT JOIN {DbNames.EPYSL}..RackBin ON RackBin.BinID = a.BinID
                LEFT JOIN {DbNames.EPYSL}..Location L ON L.LocationID = a.LocationID
                WHERE a.ChildID = {childId}
                ORDER BY L.ShortName, Rack.RackNo";
            }
            return await _service.GetDataAsync<YDStoreReceiveChildRackBin>(sql);
        }
        public async Task<List<YDStoreReceiveChildRackBin>> GetRackBinForYDReturnRcvUnusable(int childId, int locationId, int kReturnReceivedChildId = 0)  //List<YarnReceiveChildRackBin>
        {
            string sql = "";
            if (kReturnReceivedChildId > 0)
            {
                sql = $@"
                WITH
                MailList AS
                (
	                SELECT a.ChildRackBinID, a.ChildID, a.LocationID, a.RackID, a.BinID, 
	                a.NoOfCartoon, a.NoOfCone, a.ReceiveQty, 
	                Rack.RackNo, RackBin.BinNo, LocationName = L.ShortName,
                    YarnStockSetId = 0
	                FROM YDStoreReceiveChildRackBin a
	                LEFT JOIN {DbNames.EPYSL}..Rack ON Rack.RackID = a.RackID
	                LEFT JOIN {DbNames.EPYSL}..RackBin ON RackBin.BinID = a.BinID
	                LEFT JOIN {DbNames.EPYSL}..Location L ON L.LocationID = a.LocationID
	                WHERE a.ChildID = {childId}
                ),
                QCRRList AS
                (
	                SELECT a.ChildRackBinID, a.ChildID, a.LocationID, a.RackID, a.BinID, 
	                a.NoOfCartoon, a.NoOfCone, ReceiveQty = YSM.AllocatedStockQty,  
	                Rack.RackNo, RackBin.BinNo, LocationName = L.ShortName,
                    M.YarnStockSetId
	                FROM YDStoreReceiveChildRackBin a
	                LEFT JOIN {DbNames.EPYSL}..Rack ON Rack.RackID = a.RackID
	                LEFT JOIN {DbNames.EPYSL}..RackBin ON RackBin.BinID = a.BinID
	                LEFT JOIN {DbNames.EPYSL}..Location L ON L.LocationID = a.LocationID
	                INNER JOIN YDLeftOverReturnReceiveChildRackBinMapping M ON M.ChildRackBinID = a.ChildRackBinID
                    LEFT JOIN YarnStockMaster YSM ON YSM.YarnStockSetId = M.YarnStockSetId
	                LEFT JOIN MailList ML ON ML.ChildRackBinID = M.ChildRackBinID
	                WHERE M.YDLeftOverReturnReceiveChildId = {kReturnReceivedChildId} AND M.IsUsable=0 AND ML.ChildRackBinID IS NULL
                ),
                FinalList AS
                (
	                SELECT * FROM MailList
	                UNION
	                SELECT * FROM QCRRList
                )
                SELECT * FROM FinalList ORDER BY LocationName, RackNo";
            }
            else
            {
                sql = $@"
                SELECT a.ChildRackBinID, a.ChildID, a.LocationID, a.RackID, a.BinID, 
                a.NoOfCartoon, a.NoOfCone, a.ReceiveQty, 
                Rack.RackNo, RackBin.BinNo, LocationName = L.ShortName
                FROM YDStoreReceiveChildRackBin a
                LEFT JOIN {DbNames.EPYSL}..Rack ON Rack.RackID = a.RackID
                LEFT JOIN {DbNames.EPYSL}..RackBin ON RackBin.BinID = a.BinID
                LEFT JOIN {DbNames.EPYSL}..Location L ON L.LocationID = a.LocationID
                WHERE a.ChildID = {childId}
                ORDER BY L.ShortName, Rack.RackNo";
            }
            return await _service.GetDataAsync<YDStoreReceiveChildRackBin>(sql);
        }
        public async Task<List<YDStoreReceiveChildRackBin>> GetRackBinForSCReturnRcvUnusable(int childId, int locationId, int kReturnReceivedChildId = 0)
        {
            string sql = "";
            if (kReturnReceivedChildId > 0)
            {
                sql = $@"
                    SELECT  YSS.ItemMasterID,YSS.YarnStockSetId, YSS.SupplierID,YSS.SpinnerID,LotNo = YSS.YarnLotNo,YSS.ShadeCode,YSS.PhysicalCount,
					a.ChildRackBinID, a.ChildID, a.LocationID, a.RackID, a.BinID, 
	                a.NoOfCartoon, a.NoOfCone, ReceiveQty = YSM.AllocatedStockQty, 
	                Rack.RackNo, RackBin.BinNo, LocationName = L.ShortName, M.YarnStockSetId,
					Rate = ISNULL(YSC.Rate,ISNULL(POC.Rate,0)),
					SpinnerName = C.ShortName, YRC.YarnControlNo, RackQty = a.ReceiveQty,
					AvgCartoonWeight = Cast(ROUND(YRC.ReceiveQty/YRC.NoOfCartoon,2) AS DECIMAL(18, 2))
	                FROM YDStoreReceiveChildRackBin a
					INNER JOIN KSCLOReturnReceiveChildRackBinMapping M ON M.ChildRackBinID = a.ChildRackBinID
					INNER JOIN YDStoreReceiveChild YRC ON YRC.ChildID = a.ChildID
					INNER JOIN YDStoreReceiveMaster YRM ON YRM.ReceiveID = YRC.ReceiveID
	                LEFT JOIN YarnPOChild POC ON POC.YPOChildID = YRC.POChildID
					LEFT JOIN YarnStockChild YSC ON YSC.YarnStockSetId = M.YarnStockSetId AND YSC.StockFromTableId = 2 AND YSC.StockFromPKId = YRC.ChildID
	                LEFT JOIN {DbNames.EPYSL}..Rack ON Rack.RackID = a.RackID
	                LEFT JOIN {DbNames.EPYSL}..RackBin ON RackBin.BinID = a.BinID
	                LEFT JOIN {DbNames.EPYSL}..Location L ON L.LocationID = a.LocationID
                    LEFT JOIN YarnStockMaster YSM ON YSM.YarnStockSetId = M.YarnStockSetId
					INNER JOIN YarnStockSet YSS ON YSS.YarnStockSetId = M.YarnStockSetId
					LEFT JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = YSS.SpinnerId
	                WHERE M.KSCLOReturnReceiveChildId = {kReturnReceivedChildId} AND M.IsUsable=0 
				   ORDER BY LocationName, RackNo

                /*WITH
                MailList AS
                (
	                SELECT a.ChildRackBinID, a.ChildID, a.LocationID, a.RackID, a.BinID, 
	                a.NoOfCartoon, a.NoOfCone, a.ReceiveQty, 
	                Rack.RackNo, RackBin.BinNo, LocationName = L.ShortName,
                    YarnStockSetId = 0
	                FROM YDStoreReceiveChildRackBin a
	                LEFT JOIN {DbNames.EPYSL}..Rack ON Rack.RackID = a.RackID
	                LEFT JOIN {DbNames.EPYSL}..RackBin ON RackBin.BinID = a.BinID
	                LEFT JOIN {DbNames.EPYSL}..Location L ON L.LocationID = a.LocationID
	                WHERE a.ChildID = {childId}
                ),
                QCRRList AS
                (
	                SELECT a.ChildRackBinID, a.ChildID, a.LocationID, a.RackID, a.BinID, 
	                a.NoOfCartoon, a.NoOfCone, ReceiveQty = YSM.AllocatedStockQty, 
	                Rack.RackNo, RackBin.BinNo, LocationName = L.ShortName,
                    M.YarnStockSetId
	                FROM YDStoreReceiveChildRackBin a
	                LEFT JOIN {DbNames.EPYSL}..Rack ON Rack.RackID = a.RackID
	                LEFT JOIN {DbNames.EPYSL}..RackBin ON RackBin.BinID = a.BinID
	                LEFT JOIN {DbNames.EPYSL}..Location L ON L.LocationID = a.LocationID
	                INNER JOIN KSCLOReturnReceiveChildRackBinMapping M ON M.ChildRackBinID = a.ChildRackBinID
                    LEFT JOIN YarnStockMaster YSM ON YSM.YarnStockSetId = M.YarnStockSetId
	                LEFT JOIN MailList ML ON ML.ChildRackBinID = M.ChildRackBinID
	                WHERE M.KSCLOReturnReceiveChildId = {kReturnReceivedChildId} AND M.IsUsable=0 AND ML.ChildRackBinID IS NULL
                ),
                FinalList AS
                (
	                SELECT * FROM MailList
	                UNION
	                SELECT * FROM QCRRList
                )
                SELECT * FROM FinalList ORDER BY LocationName, RackNo*/";
            }
            else
            {
                sql = $@"
                SELECT a.ChildRackBinID, a.ChildID, a.LocationID, a.RackID, a.BinID, 
                a.NoOfCartoon, a.NoOfCone, a.ReceiveQty, 
                Rack.RackNo, RackBin.BinNo, LocationName = L.ShortName
                FROM YDStoreReceiveChildRackBin a
                LEFT JOIN {DbNames.EPYSL}..Rack ON Rack.RackID = a.RackID
                LEFT JOIN {DbNames.EPYSL}..RackBin ON RackBin.BinID = a.BinID
                LEFT JOIN {DbNames.EPYSL}..Location L ON L.LocationID = a.LocationID
                WHERE a.ChildID = {childId}
                ORDER BY L.ShortName, Rack.RackNo";
            }
            return await _service.GetDataAsync<YDStoreReceiveChildRackBin>(sql);
        }
        public async Task<List<YDStoreReceiveChildRackBin>> GetRackBinForRNDReturnRcvUnusable(int childId, int locationId, int kReturnReceivedChildId = 0)
        {
            string sql = "";
            if (kReturnReceivedChildId > 0)
            {
                sql = $@"
                WITH
                MailList AS
                (
	                SELECT a.ChildRackBinID, a.ChildID, a.LocationID, a.RackID, a.BinID, 
	                a.NoOfCartoon, a.NoOfCone, a.ReceiveQty, 
	                Rack.RackNo, RackBin.BinNo, LocationName = L.ShortName,
                    YarnStockSetId = 0
	                FROM YDStoreReceiveChildRackBin a
	                LEFT JOIN {DbNames.EPYSL}..Rack ON Rack.RackID = a.RackID
	                LEFT JOIN {DbNames.EPYSL}..RackBin ON RackBin.BinID = a.BinID
	                LEFT JOIN {DbNames.EPYSL}..Location L ON L.LocationID = a.LocationID
	                WHERE a.ChildID = {childId}
                ),
                QCRRList AS
                (
	                SELECT a.ChildRackBinID, a.ChildID, a.LocationID, a.RackID, a.BinID, 
	                a.NoOfCartoon, a.NoOfCone, ReceiveQty = YSM.AllocatedStockQty, 
	                Rack.RackNo, RackBin.BinNo, LocationName = L.ShortName,
                    M.YarnStockSetId
	                FROM YDStoreReceiveChildRackBin a
	                LEFT JOIN {DbNames.EPYSL}..Rack ON Rack.RackID = a.RackID
	                LEFT JOIN {DbNames.EPYSL}..RackBin ON RackBin.BinID = a.BinID
	                LEFT JOIN {DbNames.EPYSL}..Location L ON L.LocationID = a.LocationID
	                INNER JOIN YarnRNDReturnReceiveChildRackBinMapping M ON M.ChildRackBinID = a.ChildRackBinID
                    LEFT JOIN YarnStockMaster YSM ON YSM.YarnStockSetId = M.YarnStockSetId
	                LEFT JOIN MailList ML ON ML.ChildRackBinID = M.ChildRackBinID
	                WHERE M.RNDLOReturnReceiveChildId = {kReturnReceivedChildId} AND M.IsUsable=0 AND ML.ChildRackBinID IS NULL
                ),
                FinalList AS
                (
	                SELECT * FROM MailList
	                UNION
	                SELECT * FROM QCRRList
                )
                SELECT * FROM FinalList ORDER BY LocationName, RackNo";
            }
            else
            {
                sql = $@"
                SELECT a.ChildRackBinID, a.ChildID, a.LocationID, a.RackID, a.BinID, 
                a.NoOfCartoon, a.NoOfCone, a.ReceiveQty, 
                Rack.RackNo, RackBin.BinNo, LocationName = L.ShortName
                FROM YDStoreReceiveChildRackBin a
                LEFT JOIN {DbNames.EPYSL}..Rack ON Rack.RackID = a.RackID
                LEFT JOIN {DbNames.EPYSL}..RackBin ON RackBin.BinID = a.BinID
                LEFT JOIN {DbNames.EPYSL}..Location L ON L.LocationID = a.LocationID
                WHERE a.ChildID = {childId}
                ORDER BY L.ShortName, Rack.RackNo";
            }
            return await _service.GetDataAsync<YDStoreReceiveChildRackBin>(sql);
        }
        public async Task<List<YDStoreReceiveChildRackBin>> GetAllRacks(PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? " Order By LocationName, RackNo" : paginationInfo.OrderBy;

            var query = $@"
                With F As (
                    SELECT a.ChildRackBinID, a.ChildID, a.LocationID, a.RackID, 
                    a.NoOfCartoon, a.NoOfCone, a.ReceiveQty, 
                    Rack.RackNo, LocationName = L.ShortName, YRC.YarnStockSetId
                    FROM YDStoreReceiveChildRackBin a
					INNER JOIN YDStoreReceiveChild YRC ON YRC.ChildID=a.ChildID
                    INNER JOIN {DbNames.EPYSL}..Rack ON Rack.RackID = a.RackID
                    INNER JOIN {DbNames.EPYSL}..Location L ON L.LocationID = a.LocationID
                    WHERE a.LocationID > 0 AND Rack.RackNo IS NOT NULL
                )
                Select *, COUNT(*) Over() TotalRows From F
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<YDStoreReceiveChildRackBin>(query);
        }
        public async Task<List<YDStoreReceiveChildRackBin>> GetRackBinById(string childRackBinIDs)
        {
            if (childRackBinIDs.IsNullOrEmpty()) return new List<YDStoreReceiveChildRackBin>();

            string sql = $@"
                SELECT a.*
                FROM YDStoreReceiveChildRackBin a
                WHERE a.ChildRackBinID IN ({childRackBinIDs})";
            return await _service.GetDataAsync<YDStoreReceiveChildRackBin>(sql);
        }
        public async Task<YDStoreReceiveMaster> GetAllAsync(int id)
        {
            var sql = $@"
            ;Select * From YDStoreReceiveMaster Where YDStoreReceiveMasterID = {id}
            ; Select * From YDStoreReceiveChild Where YDStoreReceiveMasterID = {id}
            ; Select * From YDStoreReceiveChildRackBin Where ChildID In(Select YDStoreReceiveChildID From YDStoreReceiveChild Where YDStoreReceiveMasterID = {id}) ";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YDStoreReceiveMaster data = records.Read<YDStoreReceiveMaster>().FirstOrDefault();
                Guard.Against.NullObject(data);
                data.Childs = records.Read<YDStoreReceiveChild>().ToList();
                List<YDStoreReceiveChildRackBin> YarnChildRackBinList = records.Read<YDStoreReceiveChildRackBin>().ToList();
                data.Childs.ForEach(x =>
                {
                    x.YDStoreReceiveChildRackBins = YarnChildRackBinList.Where(y => y.ChildID == x.YDStoreReceiveChildID).ToList();
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
        public List<YDStoreReceiveChildRackBin> GetRackBinWithUpdateValue(List<YDStoreReceiveChildRackBin> rackBins, int childRackBinID, Infrastructure.Statics.EnumRackBinOperationType rackBinOT, int noOfCone, int noOfCartoon, decimal qty)
        {
            int indexF = rackBins.FindIndex(x => x.ChildRackBinID == childRackBinID);
            if (indexF > -1)
            {
                if (rackBinOT == Infrastructure.Statics.EnumRackBinOperationType.Addition)
                {
                    rackBins[indexF].NoOfCone = rackBins[indexF].NoOfCone + noOfCone;
                    rackBins[indexF].NoOfCartoon = rackBins[indexF].NoOfCartoon + noOfCartoon;
                    rackBins[indexF].ReceiveQty = rackBins[indexF].ReceiveQty + qty;
                }
                else if (rackBinOT == Infrastructure.Statics.EnumRackBinOperationType.Deduction)
                {
                    rackBins[indexF].NoOfCone = rackBins[indexF].NoOfCone - noOfCone;
                    rackBins[indexF].NoOfCartoon = rackBins[indexF].NoOfCartoon - noOfCartoon;
                    rackBins[indexF].ReceiveQty = rackBins[indexF].ReceiveQty - qty;
                }
            }
            return rackBins;
        }
        public async Task SaveAsync(YDStoreReceiveMaster entity)
        {
            SqlTransaction transaction = null;
            SqlTransaction transactionGmt = null;
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                await _connectionGmt.OpenAsync();
                transactionGmt = _connectionGmt.BeginTransaction();

                int maxRackBinId = 0;
                switch (entity.EntityState)
                {
                    case EntityState.Modified:
                        var addedChilds = entity.Childs.FindAll(x => x.EntityState == EntityState.Added);
                        var count = 0;

                        entity.Childs.ForEach(c =>
                        {
                            count += c.YDStoreReceiveChildRackBins.Where(y => y.EntityState == EntityState.Added).Count();
                        });

                        maxRackBinId = await _service.GetMaxIdAsync(TableNames.YDStoreReceiveChildRackBin, count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

                        List<YDStoreReceiveChildRackBin> yDStoreReceiveChildRackBins = new List<YDStoreReceiveChildRackBin>();
                        entity.Childs.ForEach(item =>
                        {
                            yDStoreReceiveChildRackBins = new List<YDStoreReceiveChildRackBin>();
                            item.YDStoreReceiveChildRackBins.ToList().ForEach(itemc =>
                            {
                                if (itemc.EntityState == EntityState.Added)
                                {
                                    itemc.ChildRackBinID = maxRackBinId++;
                                    itemc.ChildID = item.YDStoreReceiveChildID;

                                    item.YDStoreReceiveMasterID = entity.YDStoreReceiveMasterID;
                                }
                                yDStoreReceiveChildRackBins.Add(CommonFunction.DeepClone(itemc));
                            });
                            item.YDStoreReceiveChildRackBins = yDStoreReceiveChildRackBins;
                        });

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
                List<YDStoreReceiveChildRackBin> YarnRackBinList = new List<YDStoreReceiveChildRackBin>();
                entity.Childs.ForEach(x => YarnRackBinList.AddRange(x.YDStoreReceiveChildRackBins));

                var childs = YarnRackBinList.Where(x => x.ChildID == 0).ToList();
                if (childs.Count() > 0)
                {
                    throw new Exception("YDStoreReceiveChildId missing => SaveAsync => YDStoreRackBinAllocationService");
                }
                await _service.SaveAsync(YarnRackBinList, transaction);
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
