using Dapper;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using System.Data;
using System.Data.Entity;
using Microsoft.Data.SqlClient;


namespace EPYSLTEX.Infrastructure.Services
{
    public  class YarnRackBinAllocationService : IYarnRackBinAllocationService
    {
        private readonly IDapperCRUDService<YarnReceiveMaster> _service;

        private readonly SqlConnection _connection;
        private readonly SqlConnection _connectionGmt;

        public YarnRackBinAllocationService(IDapperCRUDService<YarnReceiveMaster> service)
        {
            _service = service;
            _service.Connection = service.GetConnection(AppConstants.GMT_CONNECTION);
            _connectionGmt = service.Connection;

            _service.Connection = service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _connection = service.Connection;
        }

        public async Task<List<YarnReceiveMaster>> GetPagedAsync(Status status, bool isCDAPage, PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By ReceiveDate Desc" : paginationInfo.OrderBy;
            var sql = string.Empty;
            if (status == Status.Pending)
            {

                sql += $@"With M AS(
                SELECT RM.ReceiveID, RM.ReceiveDate, RM.ReceiveNo, RM.LocationID, RM.RCompanyID, RM.OCompanyID, RM.SupplierID, RM.LCNo, 
                RM.LCDate, RM.Tolerance, RM.InvoiceNo, RM.InvoiceDate, RM.BLNo, RM.BLDate, RM.BankBranchID, RM.PLNo, RM.PLDate, RM.ChallanNo, 
                RM.ChallanDate, RM.GPNo, RM.GPDate, RM.TransportMode, RM.TransportTypeID, RM.CContractorID, RM.VehicalNo, RM.ShipmentStatus, 
                RM.Remarks, RM.SpinnerID, RM.POID, RM.CIID, RM.LCID, RM.ReceivedByID, RM.GPTime, RM.PONo, RM.PODate
                FROM {TableNames.YARN_RECEIVE_MASTER} RM WHERE RM.IsApproved = 1 AND RM.PartialAllocation = 0 AND RM.CompleteAllocation=0 AND IsCDA = '{isCDAPage}'
                )
                SELECT M.ReceiveID, M.ReceiveDate, M.ReceiveNo, M.LocationID, M.RCompanyID, M.OCompanyID, M.SupplierID, M.LCNo, M.LCDate, 
                M.Tolerance, M.InvoiceNo, M.InvoiceDate, M.BLNo, M.BLDate, M.BankBranchID, M.PLNo, M.PLDate, M.ChallanNo, M.ChallanDate, 
                M.GPNo, M.GPDate, M.TransportMode, M.TransportTypeID, M.CContractorID, M.VehicalNo, M.ShipmentStatus, M.Remarks, M.SpinnerID, 
                M.POID, M.CIID, M.LCID, M.ReceivedByID, M.GPTime, CC.[Name] SupplierName, BB.BranchName BankBranchName, COM.CompanyName RCompany,
                M.PONo, M.PODate, Count(*) Over() TotalRows   
		        FROM M
		        LEFT JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = M.SupplierID
				LEFT JOIN {DbNames.EPYSL}..BankBranch BB ON BB.BankBranchID = M.BankBranchID
				LEFT JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID = M.RCompanyID";

            }
            else if (status == Status.PartiallyCompleted)
            {
                sql += $@"With M AS(
                SELECT RM.ReceiveID, RM.ReceiveDate, RM.ReceiveNo, RM.LocationID, RM.RCompanyID, RM.OCompanyID, RM.SupplierID, RM.LCNo, RM.LCDate, RM.Tolerance,
				RM.InvoiceNo, RM.InvoiceDate, RM.BLNo, RM.BLDate, RM.BankBranchID, RM.PLNo, RM.PLDate, RM.ChallanNo, RM.ChallanDate, RM.GPNo, RM.GPDate, RM.TransportMode,
				RM.TransportTypeID, RM.CContractorID, RM.VehicalNo, RM.ShipmentStatus, RM.Remarks, RM.SpinnerID, RM.POID, RM.CIID, RM.LCID, RM.ReceivedByID, RM.GPTime,
				RM.PONo, RM.PODate
                FROM {TableNames.YARN_RECEIVE_MASTER} RM WHERE RM.IsApproved = 1 AND RM.PartialAllocation = 1 AND RM.CompleteAllocation=0  AND IsCDA = '{isCDAPage}'
                )
                SELECT	M.ReceiveID, M.ReceiveDate, M.ReceiveNo, M.LocationID, M.RCompanyID, M.OCompanyID, M.SupplierID, M.LCNo, M.LCDate, M.Tolerance,
				M.InvoiceNo, M.InvoiceDate, M.BLNo, M.BLDate, M.BankBranchID, M.PLNo, M.PLDate, M.ChallanNo, M.ChallanDate, M.GPNo, M.GPDate, M.TransportMode,
				M.TransportTypeID, M.CContractorID, M.VehicalNo, M.ShipmentStatus, M.Remarks, M.SpinnerID, M.POID, M.CIID, M.LCID, M.ReceivedByID, M.GPTime,
				CC.[Name] SupplierName, BB.BranchName BankBranchName, COM.CompanyName RCompany, M.PONo, M.PODate,
		        Count(*) Over() TotalRows  
		        FROM M
		        LEFT JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = M.SupplierID
				LEFT JOIN {DbNames.EPYSL}..BankBranch BB ON BB.BankBranchID = M.BankBranchID
				LEFT JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID = M.RCompanyID";
            }
            else if (status == Status.Completed)
            {
                sql += $@"With M AS(
                SELECT RM.ReceiveID, RM.ReceiveDate, RM.ReceiveNo, RM.LocationID, RM.RCompanyID, RM.OCompanyID, RM.SupplierID, RM.LCNo, RM.LCDate, RM.Tolerance,
				RM.InvoiceNo, RM.InvoiceDate, RM.BLNo, RM.BLDate, RM.BankBranchID, RM.PLNo, RM.PLDate, RM.ChallanNo, RM.ChallanDate, RM.GPNo, RM.GPDate, RM.TransportMode,
				RM.TransportTypeID, RM.CContractorID, RM.VehicalNo, RM.ShipmentStatus, RM.Remarks, RM.SpinnerID, RM.POID, RM.CIID, RM.LCID, RM.ReceivedByID, RM.GPTime,
				RM.PONo, RM.PODate
                FROM {TableNames.YARN_RECEIVE_MASTER} RM WHERE RM.IsApproved = 1 AND RM.PartialAllocation = 0 AND RM.CompleteAllocation=1  AND IsCDA = '{isCDAPage}'
                )
                SELECT	M.ReceiveID, M.ReceiveDate, M.ReceiveNo, M.LocationID, M.RCompanyID, M.OCompanyID, M.SupplierID, M.LCNo, M.LCDate, M.Tolerance,
				M.InvoiceNo, M.InvoiceDate, M.BLNo, M.BLDate, M.BankBranchID, M.PLNo, M.PLDate, M.ChallanNo, M.ChallanDate, M.GPNo, M.GPDate, M.TransportMode,
				M.TransportTypeID, M.CContractorID, M.VehicalNo, M.ShipmentStatus, M.Remarks, M.SpinnerID, M.POID, M.CIID, M.LCID, M.ReceivedByID, M.GPTime,
				CC.[Name] SupplierName, BB.BranchName BankBranchName, COM.CompanyName RCompany, M.PONo, M.PODate,
		        Count(*) Over() TotalRows  
		        FROM M
		        LEFT JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = M.SupplierID
				LEFT JOIN {DbNames.EPYSL}..BankBranch BB ON BB.BankBranchID = M.BankBranchID
				LEFT JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID = M.RCompanyID";
            }

            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";
            return await _service.GetDataAsync<YarnReceiveMaster>(sql);
        }

        public async Task<YarnReceiveMaster> GetAsync(int id, int companyId)
        {
            var sql = $@"
                ;WITH M AS (
	                SELECT RM.ReceiveID, RM.ReceiveDate, RM.ReceiveNo, RM.LocationID, RM.RCompanyID, RM.OCompanyID, RM.SupplierID, RM.LCNo, RM.LCDate, RM.Tolerance,
	                RM.InvoiceNo, RM.InvoiceDate, RM.BLNo, RM.BLDate, RM.BankBranchID, RM.PLNo, RM.PLDate, RM.ChallanNo, RM.ChallanDate, RM.GPNo, RM.GPDate, RM.TransportMode,
	                RM.TransportTypeID, RM.CContractorID, RM.VehicalNo, RM.ShipmentStatus, RM.Remarks, RM.SpinnerID, RM.POID, RM.CIID, RM.LCID, RM.ReceivedByID, RM.GPTime, 
	                RM.CurrencyID, RM.PONo, RM.PODate, RM.ACompanyInvoice
	                FROM {TableNames.YARN_RECEIVE_MASTER} RM WHERE RM.ReceiveId = {id}
                )
                SELECT	M.ReceiveID, M.ReceiveDate, M.ReceiveNo, M.RCompanyID, M.OCompanyID, M.SupplierID, M.LCNo, M.LCDate, M.Tolerance,
                M.InvoiceNo, M.InvoiceDate, M.BLNo, M.BLDate, M.BankBranchID, M.PLNo, M.PLDate, M.ChallanNo, M.ChallanDate, M.GPNo, M.GPDate, M.TransportMode,
                M.VehicalNo, M.ShipmentStatus, M.Remarks, M.SpinnerID, M.POID, M.CIID, M.LCID, M.ReceivedByID, M.GPTime,
                CC.[Name] SupplierName, BB.BranchName BankBranchName, COM.CompanyName RCompany, M.CurrencyID, M.PONo, M.PODate, M.ACompanyInvoice,
                TraMode.ValueName TransportModeName,
                ShipmentStatus.ValueName ShipmentStatusName, RBy.[Name] ReceivedBy, SPinner.[Name] SpinnerName,
                M.LocationID, LocationName = Loc.LocationName,M.TransportTypeID, 
                TransportTypeName = TraType.ValueName,M.CContractorID, TransportAgencyName = TraAgency.ShortName
                FROM M
                INNER JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = M.SupplierID
                INNER JOIN {DbNames.EPYSL}..BankBranch BB ON BB.BankBranchID = M.BankBranchID
                LEFT JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID = M.RCompanyID
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue TraMode ON TraMode.ValueID = M.TransportMode
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue TraType ON TraType.ValueID = M.TransportTypeID
                LEFT JOIN {DbNames.EPYSL}..Contacts TraAgency ON TraAgency.ContactID = M.CContractorID
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ShipmentStatus ON ShipmentStatus.ValueID = M.CContractorID
                LEFT JOIN {DbNames.EPYSL}..[location] Loc ON Loc.LocationID = M.LocationId
                LEFT Join {DbNames.EPYSL}..LoginUser RBy On RBy.UserCode = M.ReceivedByID
                INNER JOIN {DbNames.EPYSL}..Contacts SPinner ON SPinner.ContactID = M.SpinnerID;
                
                -----childs
                ;WITH X AS (
	                SELECT RC.ChildID, RC.ReceiveID, RC.ItemMasterID, RC.InvoiceChildID, RC.POChildID, RC.UnitID, RC.InvoiceQty, RC.ChallanQty, RC.ShortQty, RC.ExcessQty,
					RC.ReceiveQty, RC.Rate, RC.Remarks, RC.PhysicalCount, RC.LotNo, RC.ChallanLot, RC.NoOfCartoon, RC.NoOfCone, RC.POQty
	                FROM {TableNames.YARN_RECEIVE_CHILD} RC WHERE RC.ReceiveID = {id}
                ), Spiner As(
					Select RC.ChildID,
					SpinnerName = C.ShortName
					From {DbNames.EPYSL}..ContactSpinnerSetup CS
					Inner Join {TableNames.YARN_RECEIVE_CHILD} RC On RC.SpinnerID = CS.SpinnerID
					Inner Join {DbNames.EPYSL}..Contacts C ON C.ContactID = CS.SpinnerID
					 WHERE RC.ReceiveID = {id}

				),POChildBuyer As(
					Select RC.ChildID, a.BuyerID, ISNULL(C.[ShortName],'') BuyerName
	                From {TableNames.YarnPOChild} a
					Inner Join X RC on RC.POChildID = a.YPOChildID
	                LEFT JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = a.BuyerID and C.ContactID !=0
	                Where RC.ReceiveID = {id}
				),
				I As (
	                Select RC.ChildID, a.BuyerID, C.[ShortName] BuyerName
	                From {TableNames.YarnPOChildBuyer} a
					Inner Join X RC on RC.POChildID = a.YPOChildID
	                LEFT JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = a.BuyerID
	               Where RC.ReceiveID = {id}
                ), YPOB As (
	                Select ChildID
		                , STUFF(
		                (
			                SELECT ',' + CAST(I.BuyerID as varchar) AS [text()]
			                FROM I
			                WHERE I.ChildID = RC.ChildID
			                FOR XML PATH('')
		                ), 1, 1, '') As YarnChildPoBuyerIds
		                , STUFF(
		                (
			                SELECT  ', ' + I.BuyerName
			                FROM I
			                WHERE I.ChildID = RC.ChildID
			                FOR XML PATH(''), TYPE
		                ).value('.', 'VARCHAR(MAX)'), 1, 1, '') As BuyerNames
	                From {TableNames.YarnPOChildBuyer} O
					Inner Join X RC on RC.POChildID = O.YPOChildID
	                Where RC.ReceiveID = {id}
	                Group By ChildID
                )
                SELECT X.ChildID, X.ReceiveID, X.ItemMasterID, X.InvoiceChildID, X.POChildID, X.UnitID, X.InvoiceQty, X.POQty, X.ChallanQty, X.ShortQty, X.ExcessQty, X.ReceiveQty, 
				X.Rate, X.Remarks, X.PhysicalCount, X.LotNo, X.ChallanLot, X.NoOfCartoon, X.NoOfCone, UU.DisplayUnitDesc, ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, 
                    ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, 
                    ISV7.SegmentValue Segment7ValueDesc, ISV8.SegmentValue Segment8ValueDesc,
				YarnChildPoBuyerIds = IIF(ISNULL(YPOB.YarnChildPoBuyerIds,'') ='' , ISNULL(PB.BuyerID,'') ,ISNULL(YPOB.YarnChildPoBuyerIds,'')),
				BuyerNames = IIF(ISNULL(REPLACE(YPOB.BuyerNames,'amp;',''),'') ='' , ISNULL(PB.BuyerName,'') ,ISNULL(REPLACE(YPOB.BuyerNames,'amp;',''),'')),
                S.SpinnerName
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
				LEFT JOIN YPOB On YPOB.ChildID =X.ChildID
                LEFT JOIN POChildBuyer PB  On PB.ChildID = X.ChildID
	            Left Join Spiner S on S.ChildID = X.ChildID
                LEFT Join {DbNames.EPYSL}..Unit AS UU On UU.UnitID = X.UnitID
                Group By  X.ChildID, X.ReceiveID, X.ItemMasterID, X.InvoiceChildID, X.POChildID, X.UnitID, X.InvoiceQty, X.POQty, X.ChallanQty, X.ShortQty, X.ExcessQty, X.ReceiveQty, 
				X.Rate, X.Remarks, X.PhysicalCount, X.LotNo, X.ChallanLot, X.NoOfCartoon, X.NoOfCone, UU.DisplayUnitDesc, ISV1.SegmentValue, ISV2.SegmentValue, 
                    ISV3.SegmentValue, ISV4.SegmentValue, ISV5.SegmentValue, ISV6.SegmentValue, 
                    ISV7.SegmentValue, ISV8.SegmentValue,
				IIF(ISNULL(YPOB.YarnChildPoBuyerIds,'') ='' , ISNULL(PB.BuyerID,'') ,ISNULL(YPOB.YarnChildPoBuyerIds,'')),
				IIF(ISNULL(REPLACE(YPOB.BuyerNames,'amp;',''),'') ='' , ISNULL(PB.BuyerName,'') ,ISNULL(REPLACE(YPOB.BuyerNames,'amp;',''),''))
				,S.SpinnerName;

                --Rack
                SELECT RB.*, MinNoOfCartoon = RB.NoOfCartoon, MinNoOfCone = RB.NoOfCone, MinReceiveQty = RB.ReceiveQty,RC.RackNo 
                FROM {TableNames.YARN_RECEIVE_CHILD_RACK_BIN} RB
                INNER JOIN {TableNames.YARN_RECEIVE_CHILD} YRC ON YRC.ChildID = RB.ChildID
                INNER JOIN {TableNames.YARN_RECEIVE_MASTER} YRM ON YRM.ReceiveID = YRC.ReceiveID
                LEFT JOIN {DbNames.EPYSL}..Rack RC ON RC.RackID = RB.RackID
                WHERE YRM.ReceiveID = {id}

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
                YarnReceiveMaster data = records.Read<YarnReceiveMaster>().FirstOrDefault();
                Guard.Against.NullObject(data);
                data.YarnReceiveChilds = records.Read<YarnReceiveChild>().ToList();
                List<YarnReceiveChildRackBin> racks = records.Read<YarnReceiveChildRackBin>().ToList();
                data.LocationList = records.Read<Select2OptionModel>().ToList();
                data.RackList = records.Read<Select2OptionModel>().ToList();
                data.EmployeeList = records.Read<Select2OptionModel>().ToList();

                data.YarnReceiveChilds.ForEach(c =>
                {
                    c.YarnReceiveChildRackBins = racks.Where(r => r.ChildID == c.ChildID).ToList();
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
        public async Task<List<YarnReceiveChildRackBin>> GetYarnReceiveChildRackBinData(int childId)  //List<YarnReceiveChildRackBin>
        {
            var sql = string.Empty;
            sql += $@"
                SELECT a.ChildRackBinID, a.ChildID, a.LocationID, a.RackID, a.BinID, a.NoOfCartoon, a.NoOfCone, a.ReceiveQty, a.Remarks, a.EmployeeID--, Rack.RackNo, RackBin.BinNo
                FROM {TableNames.YARN_RECEIVE_CHILD_RACK_BIN} a
                WHERE ChildID={childId}";
            return await _service.GetDataAsync<YarnReceiveChildRackBin>(sql);
        }

        public async Task<List<YarnReceiveChildRackBin>> GetRackBin(int childId, int locationId, int qcReturnReceivedChildId = 0)  //List<YarnReceiveChildRackBin>
        {
            string sql = "";
            if (qcReturnReceivedChildId > 0)
            {
                sql = $@"
                WITH
                MailList AS
                (
	                SELECT a.ChildRackBinID, a.ChildID, a.LocationID, a.RackID, a.BinID, 
	                a.NoOfCartoon, a.NoOfCone, a.ReceiveQty, 
	                Rack.RackNo, RackBin.BinNo, LocationName = L.ShortName, YRC.YarnControlNo
	                FROM {TableNames.YARN_RECEIVE_CHILD_RACK_BIN} a
                    LEFT JOIN {TableNames.YARN_RECEIVE_CHILD} YRC ON YRC.ChildID = a.ChildID
	                LEFT JOIN {DbNames.EPYSL}..Rack ON Rack.RackID = a.RackID
	                LEFT JOIN {DbNames.EPYSL}..RackBin ON RackBin.BinID = a.BinID
	                LEFT JOIN {DbNames.EPYSL}..Location L ON L.LocationID = a.LocationID
	                WHERE a.ChildID = {childId}
                ),
                QCRRList AS
                (
	                SELECT a.ChildRackBinID, a.ChildID, a.LocationID, a.RackID, a.BinID, 
	                a.NoOfCartoon, a.NoOfCone, a.ReceiveQty, 
	                Rack.RackNo, RackBin.BinNo, LocationName = L.ShortName, YRC.YarnControlNo
	                FROM {TableNames.YARN_RECEIVE_CHILD_RACK_BIN} a
                    LEFT JOIN {TableNames.YARN_RECEIVE_CHILD} YRC ON YRC.ChildID = a.ChildID
	                LEFT JOIN {DbNames.EPYSL}..Rack ON Rack.RackID = a.RackID
	                LEFT JOIN {DbNames.EPYSL}..RackBin ON RackBin.BinID = a.BinID
	                LEFT JOIN {DbNames.EPYSL}..Location L ON L.LocationID = a.LocationID
	                INNER JOIN {TableNames.YARN_QC_RETURN_RECEIVE_CHILD_RACK_BIN_MAPPING} M ON M.ChildRackBinID = a.ChildRackBinID
	                LEFT JOIN MailList ML ON ML.ChildRackBinID = M.ChildRackBinID
	                WHERE M.QCReturnReceivedChildId = {qcReturnReceivedChildId} AND ML.ChildRackBinID IS NULL
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
                Rack.RackNo, RackBin.BinNo, LocationName = L.ShortName, YRC.YarnControlNo
                FROM {TableNames.YARN_RECEIVE_CHILD_RACK_BIN} a
                LEFT JOIN {TableNames.YARN_RECEIVE_CHILD} YRC ON YRC.ChildID = a.ChildID
                LEFT JOIN {DbNames.EPYSL}..Rack ON Rack.RackID = a.RackID
                LEFT JOIN {DbNames.EPYSL}..RackBin ON RackBin.BinID = a.BinID
                LEFT JOIN {DbNames.EPYSL}..Location L ON L.LocationID = a.LocationID
                WHERE a.ChildID = {childId}
                ORDER BY L.ShortName, Rack.RackNo";
            }
            return await _service.GetDataAsync<YarnReceiveChildRackBin>(sql);
        }
        public async Task<List<YarnReceiveChildRackBin>> GetRackBinForKnittingReturnRcv(int childId, int locationId, int kReturnReceivedChildId = 0)  //List<YarnReceiveChildRackBin>
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
	                FROM {TableNames.YARN_RECEIVE_CHILD_RACK_BIN} a
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
	                FROM {TableNames.YARN_RECEIVE_CHILD_RACK_BIN} a
	                LEFT JOIN {DbNames.EPYSL}..Rack ON Rack.RackID = a.RackID
	                LEFT JOIN {DbNames.EPYSL}..RackBin ON RackBin.BinID = a.BinID
	                LEFT JOIN {DbNames.EPYSL}..Location L ON L.LocationID = a.LocationID
	                INNER JOIN {TableNames.KYLO_RETURN_RECEIVE_CHILD_RACK_BIN_MAPPING} M ON M.ChildRackBinID = a.ChildRackBinID
                    LEFT JOIN {TableNames.YarnStockMaster} YSM ON YSM.YarnStockSetId = M.YarnStockSetId
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
                FROM {TableNames.YARN_RECEIVE_CHILD_RACK_BIN} a
                LEFT JOIN {DbNames.EPYSL}..Rack ON Rack.RackID = a.RackID
                LEFT JOIN {DbNames.EPYSL}..RackBin ON RackBin.BinID = a.BinID
                LEFT JOIN {DbNames.EPYSL}..Location L ON L.LocationID = a.LocationID
                WHERE a.ChildID = {childId}
                ORDER BY L.ShortName, Rack.RackNo";
            }
            return await _service.GetDataAsync<YarnReceiveChildRackBin>(sql);
        }
        public async Task<List<YarnReceiveChildRackBin>> GetRackBinForYDReturnRcv(int childId, int locationId, int kReturnReceivedChildId = 0)
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
	                FROM {TableNames.YARN_RECEIVE_CHILD_RACK_BIN} a
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
	                FROM {TableNames.YARN_RECEIVE_CHILD_RACK_BIN} a
	                LEFT JOIN {DbNames.EPYSL}..Rack ON Rack.RackID = a.RackID
	                LEFT JOIN {DbNames.EPYSL}..RackBin ON RackBin.BinID = a.BinID
	                LEFT JOIN {DbNames.EPYSL}..Location L ON L.LocationID = a.LocationID
	                INNER JOIN {TableNames.YD_Left_Over_Return_Receive_CHILD_RACK_BIN_MAPPING} M ON M.ChildRackBinID = a.ChildRackBinID
                    LEFT JOIN {TableNames.YarnStockMaster} YSM ON YSM.YarnStockSetId = M.YarnStockSetId
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
                FROM {TableNames.YARN_RECEIVE_CHILD_RACK_BIN} a
                LEFT JOIN {DbNames.EPYSL}..Rack ON Rack.RackID = a.RackID
                LEFT JOIN {DbNames.EPYSL}..RackBin ON RackBin.BinID = a.BinID
                LEFT JOIN {DbNames.EPYSL}..Location L ON L.LocationID = a.LocationID
                WHERE a.ChildID = {childId}
                ORDER BY L.ShortName, Rack.RackNo";
            }
            return await _service.GetDataAsync<YarnReceiveChildRackBin>(sql);
        }
        public async Task<List<YarnReceiveChildRackBin>> GetRackBinForSCReturnRcv(int childId, int locationId, int kReturnReceivedChildId = 0)
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
	                FROM {TableNames.YARN_RECEIVE_CHILD_RACK_BIN} a
					INNER JOIN {TableNames.KNITTING_SUB_CONTRACT_Return_Receive_CHILD_RACK_BIN_MAPPING} M ON M.ChildRackBinID = a.ChildRackBinID
					INNER JOIN {TableNames.YARN_RECEIVE_CHILD} YRC ON YRC.ChildID = a.ChildID
					INNER JOIN {TableNames.YARN_RECEIVE_MASTER} YRM ON YRM.ReceiveID = YRC.ReceiveID
	                LEFT JOIN {TableNames.YarnPOChild} POC ON POC.YPOChildID = YRC.POChildID
					LEFT JOIN {TableNames.YarnStockChild} YSC ON YSC.YarnStockSetId = M.YarnStockSetId AND YSC.StockFromTableId = 2 AND YSC.StockFromPKId = YRC.ChildID
	                LEFT JOIN {DbNames.EPYSL}..Rack ON Rack.RackID = a.RackID
	                LEFT JOIN {DbNames.EPYSL}..RackBin ON RackBin.BinID = a.BinID
	                LEFT JOIN {DbNames.EPYSL}..Location L ON L.LocationID = a.LocationID
                    LEFT JOIN {TableNames.YarnStockMaster} YSM ON YSM.YarnStockSetId = M.YarnStockSetId
					INNER JOIN {TableNames.YarnStockSet} YSS ON YSS.YarnStockSetId = M.YarnStockSetId
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
	                FROM {TableNames.YARN_RECEIVE_CHILD_RACK_BIN} a
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
	                FROM {TableNames.YARN_RECEIVE_CHILD_RACK_BIN} a
	                LEFT JOIN {DbNames.EPYSL}..Rack ON Rack.RackID = a.RackID
	                LEFT JOIN {DbNames.EPYSL}..RackBin ON RackBin.BinID = a.BinID
	                LEFT JOIN {DbNames.EPYSL}..Location L ON L.LocationID = a.LocationID
	                INNER JOIN {TableNames.KNITTING_SUB_CONTRACT_Return_Receive_CHILD_RACK_BIN_MAPPING} M ON M.ChildRackBinID = a.ChildRackBinID
                    LEFT JOIN {TableNames.YarnStockMaster} YSM ON YSM.YarnStockSetId = M.YarnStockSetId
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
                FROM {TableNames.YARN_RECEIVE_CHILD_RACK_BIN} a
                LEFT JOIN {DbNames.EPYSL}..Rack ON Rack.RackID = a.RackID
                LEFT JOIN {DbNames.EPYSL}..RackBin ON RackBin.BinID = a.BinID
                LEFT JOIN {DbNames.EPYSL}..Location L ON L.LocationID = a.LocationID
                WHERE a.ChildID = {childId}
                ORDER BY L.ShortName, Rack.RackNo";
            }
            return await _service.GetDataAsync<YarnReceiveChildRackBin>(sql);
        }
        public async Task<List<YarnReceiveChildRackBin>> GetRackBinForRNDReturnRcv(int childId, int locationId, int kReturnReceivedChildId = 0)
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
	                FROM {TableNames.YARN_RECEIVE_CHILD_RACK_BIN} a
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
	                FROM {TableNames.YARN_RECEIVE_CHILD_RACK_BIN} a
	                LEFT JOIN {DbNames.EPYSL}..Rack ON Rack.RackID = a.RackID
	                LEFT JOIN {DbNames.EPYSL}..RackBin ON RackBin.BinID = a.BinID
	                LEFT JOIN {DbNames.EPYSL}..Location L ON L.LocationID = a.LocationID
	                INNER JOIN {TableNames.YARN_RND_Return_Receive_CHILD_RACK_BIN_MAPPING} M ON M.ChildRackBinID = a.ChildRackBinID
                    LEFT JOIN {TableNames.YarnStockMaster} YSM ON YSM.YarnStockSetId = M.YarnStockSetId
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
                FROM {TableNames.YARN_RECEIVE_CHILD_RACK_BIN} a
                LEFT JOIN {DbNames.EPYSL}..Rack ON Rack.RackID = a.RackID
                LEFT JOIN {DbNames.EPYSL}..RackBin ON RackBin.BinID = a.BinID
                LEFT JOIN {DbNames.EPYSL}..Location L ON L.LocationID = a.LocationID
                WHERE a.ChildID = {childId}
                ORDER BY L.ShortName, Rack.RackNo";
            }
            return await _service.GetDataAsync<YarnReceiveChildRackBin>(sql);
        }
        public async Task<List<YarnReceiveChildRackBin>> GetRackBinForKnittingReturnRcvUnusable(int childId, int locationId, int kReturnReceivedChildId = 0)  //List<YarnReceiveChildRackBin>
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
	                FROM {TableNames.YARN_RECEIVE_CHILD_RACK_BIN} a
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
	                FROM {TableNames.YARN_RECEIVE_CHILD_RACK_BIN} a
	                LEFT JOIN {DbNames.EPYSL}..Rack ON Rack.RackID = a.RackID
	                LEFT JOIN {DbNames.EPYSL}..RackBin ON RackBin.BinID = a.BinID
	                LEFT JOIN {DbNames.EPYSL}..Location L ON L.LocationID = a.LocationID
	                INNER JOIN {TableNames.KYLO_RETURN_RECEIVE_CHILD_RACK_BIN_MAPPING} M ON M.ChildRackBinID = a.ChildRackBinID
                    LEFT JOIN {TableNames.YarnStockMaster} YSM ON YSM.YarnStockSetId = M.YarnStockSetId
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
                FROM {TableNames.YARN_RECEIVE_CHILD_RACK_BIN} a
                LEFT JOIN {DbNames.EPYSL}..Rack ON Rack.RackID = a.RackID
                LEFT JOIN {DbNames.EPYSL}..RackBin ON RackBin.BinID = a.BinID
                LEFT JOIN {DbNames.EPYSL}..Location L ON L.LocationID = a.LocationID
                WHERE a.ChildID = {childId}
                ORDER BY L.ShortName, Rack.RackNo";
            }
            return await _service.GetDataAsync<YarnReceiveChildRackBin>(sql);
        }
        public async Task<List<YarnReceiveChildRackBin>> GetRackBinForYDReturnRcvUnusable(int childId, int locationId, int kReturnReceivedChildId = 0)  //List<YarnReceiveChildRackBin>
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
	                FROM {TableNames.YARN_RECEIVE_CHILD_RACK_BIN} a
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
	                FROM {TableNames.YARN_RECEIVE_CHILD_RACK_BIN} a
	                LEFT JOIN {DbNames.EPYSL}..Rack ON Rack.RackID = a.RackID
	                LEFT JOIN {DbNames.EPYSL}..RackBin ON RackBin.BinID = a.BinID
	                LEFT JOIN {DbNames.EPYSL}..Location L ON L.LocationID = a.LocationID
	                INNER JOIN {TableNames.YD_Left_Over_Return_Receive_CHILD_RACK_BIN_MAPPING} M ON M.ChildRackBinID = a.ChildRackBinID
                    LEFT JOIN {TableNames.YarnStockMaster} YSM ON YSM.YarnStockSetId = M.YarnStockSetId
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
                FROM {TableNames.YARN_RECEIVE_CHILD_RACK_BIN} a
                LEFT JOIN {DbNames.EPYSL}..Rack ON Rack.RackID = a.RackID
                LEFT JOIN {DbNames.EPYSL}..RackBin ON RackBin.BinID = a.BinID
                LEFT JOIN {DbNames.EPYSL}..Location L ON L.LocationID = a.LocationID
                WHERE a.ChildID = {childId}
                ORDER BY L.ShortName, Rack.RackNo";
            }
            return await _service.GetDataAsync<YarnReceiveChildRackBin>(sql);
        }
        public async Task<List<YarnReceiveChildRackBin>> GetRackBinForSCReturnRcvUnusable(int childId, int locationId, int kReturnReceivedChildId = 0)
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
	                FROM {TableNames.YARN_RECEIVE_CHILD_RACK_BIN} a
					INNER JOIN {TableNames.KNITTING_SUB_CONTRACT_Return_Receive_CHILD_RACK_BIN_MAPPING} M ON M.ChildRackBinID = a.ChildRackBinID
					INNER JOIN {TableNames.YARN_RECEIVE_CHILD} YRC ON YRC.ChildID = a.ChildID
					INNER JOIN {TableNames.YARN_RECEIVE_MASTER} YRM ON YRM.ReceiveID = YRC.ReceiveID
	                LEFT JOIN {TableNames.YarnPOChild} POC ON POC.YPOChildID = YRC.POChildID
					LEFT JOIN {TableNames.YarnStockChild} YSC ON YSC.YarnStockSetId = M.YarnStockSetId AND YSC.StockFromTableId = 2 AND YSC.StockFromPKId = YRC.ChildID
	                LEFT JOIN {DbNames.EPYSL}..Rack ON Rack.RackID = a.RackID
	                LEFT JOIN {DbNames.EPYSL}..RackBin ON RackBin.BinID = a.BinID
	                LEFT JOIN {DbNames.EPYSL}..Location L ON L.LocationID = a.LocationID
                    LEFT JOIN {TableNames.YarnStockMaster} YSM ON YSM.YarnStockSetId = M.YarnStockSetId
					INNER JOIN {TableNames.YarnStockSet} YSS ON YSS.YarnStockSetId = M.YarnStockSetId
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
	                FROM {TableNames.YARN_RECEIVE_CHILD_RACK_BIN} a
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
	                FROM {TableNames.YARN_RECEIVE_CHILD_RACK_BIN} a
	                LEFT JOIN {DbNames.EPYSL}..Rack ON Rack.RackID = a.RackID
	                LEFT JOIN {DbNames.EPYSL}..RackBin ON RackBin.BinID = a.BinID
	                LEFT JOIN {DbNames.EPYSL}..Location L ON L.LocationID = a.LocationID
	                INNER JOIN {TableNames.KNITTING_SUB_CONTRACT_Return_Receive_CHILD_RACK_BIN_MAPPING} M ON M.ChildRackBinID = a.ChildRackBinID
                    LEFT JOIN {TableNames.YarnStockMaster} YSM ON YSM.YarnStockSetId = M.YarnStockSetId
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
                FROM {TableNames.YARN_RECEIVE_CHILD_RACK_BIN} a
                LEFT JOIN {DbNames.EPYSL}..Rack ON Rack.RackID = a.RackID
                LEFT JOIN {DbNames.EPYSL}..RackBin ON RackBin.BinID = a.BinID
                LEFT JOIN {DbNames.EPYSL}..Location L ON L.LocationID = a.LocationID
                WHERE a.ChildID = {childId}
                ORDER BY L.ShortName, Rack.RackNo";
            }
            return await _service.GetDataAsync<YarnReceiveChildRackBin>(sql);
        }
        public async Task<List<YarnReceiveChildRackBin>> GetRackBinForRNDReturnRcvUnusable(int childId, int locationId, int kReturnReceivedChildId = 0)
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
	                FROM {TableNames.YARN_RECEIVE_CHILD_RACK_BIN} a
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
	                FROM {TableNames.YARN_RECEIVE_CHILD_RACK_BIN} a
	                LEFT JOIN {DbNames.EPYSL}..Rack ON Rack.RackID = a.RackID
	                LEFT JOIN {DbNames.EPYSL}..RackBin ON RackBin.BinID = a.BinID
	                LEFT JOIN {DbNames.EPYSL}..Location L ON L.LocationID = a.LocationID
	                INNER JOIN {TableNames.YARN_RND_Return_Receive_CHILD_RACK_BIN_MAPPING} M ON M.ChildRackBinID = a.ChildRackBinID
                    LEFT JOIN {TableNames.YarnStockMaster} YSM ON YSM.YarnStockSetId = M.YarnStockSetId
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
                FROM {TableNames.YARN_RECEIVE_CHILD_RACK_BIN} a
                LEFT JOIN {DbNames.EPYSL}..Rack ON Rack.RackID = a.RackID
                LEFT JOIN {DbNames.EPYSL}..RackBin ON RackBin.BinID = a.BinID
                LEFT JOIN {DbNames.EPYSL}..Location L ON L.LocationID = a.LocationID
                WHERE a.ChildID = {childId}
                ORDER BY L.ShortName, Rack.RackNo";
            }
            return await _service.GetDataAsync<YarnReceiveChildRackBin>(sql);
        }
        public async Task<List<YarnReceiveChildRackBin>> GetAllRacks(PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? " Order By LocationName, RackNo" : paginationInfo.OrderBy;

            var query = $@"
                With F As (
                    SELECT a.ChildRackBinID, a.ChildID, a.LocationID, a.RackID, 
                    a.NoOfCartoon, a.NoOfCone, a.ReceiveQty, 
                    Rack.RackNo, LocationName = L.ShortName, YRC.YarnStockSetId
                    FROM {TableNames.YARN_RECEIVE_CHILD_RACK_BIN} a
					INNER JOIN {TableNames.YARN_RECEIVE_CHILD} YRC ON YRC.ChildID=a.ChildID
                    INNER JOIN {DbNames.EPYSL}..Rack ON Rack.RackID = a.RackID
                    INNER JOIN {DbNames.EPYSL}..Location L ON L.LocationID = a.LocationID
                    WHERE a.LocationID > 0 AND Rack.RackNo IS NOT NULL
                )
                Select *, COUNT(*) Over() TotalRows From F
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<YarnReceiveChildRackBin>(query);
        }
        public async Task<List<YarnReceiveChildRackBin>> GetRackBinById(string childRackBinIDs)
        {
            if (childRackBinIDs.IsNullOrEmpty()) return new List<YarnReceiveChildRackBin>();

            string sql = $@"
                SELECT a.*
                FROM {TableNames.YARN_RECEIVE_CHILD_RACK_BIN} a
                WHERE a.ChildRackBinID IN ({childRackBinIDs})";
            return await _service.GetDataAsync<YarnReceiveChildRackBin>(sql);
        }
        public async Task<YarnReceiveMaster> GetAllAsync(int id)
        {
            var sql = $@"
            ;Select * From {TableNames.YARN_RECEIVE_MASTER} Where ReceiveID = {id}
            ; Select * From {TableNames.YARN_RECEIVE_CHILD} Where ReceiveID = {id}
            ; Select * From {TableNames.YARN_RECEIVE_CHILD_RACK_BIN} Where ChildID In(Select ChildID From YarnReceiveChild Where ReceiveID = {id}) ";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YarnReceiveMaster data = records.Read<YarnReceiveMaster>().FirstOrDefault();
                Guard.Against.NullObject(data);
                data.YarnReceiveChilds = records.Read<YarnReceiveChild>().ToList();
                List<YarnReceiveChildRackBin> YarnChildRackBinList = records.Read<YarnReceiveChildRackBin>().ToList();
                data.YarnReceiveChilds.ForEach(x =>
                {
                    x.YarnReceiveChildRackBins = YarnChildRackBinList.Where(y => y.ChildID == x.ChildID).ToList();
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
        public List<YarnReceiveChildRackBin> GetRackBinWithUpdateValue(List<YarnReceiveChildRackBin> rackBins, int childRackBinID, EPYSLTEXCore.Infrastructure.Static.EnumRackBinOperationType rackBinOT, int noOfCone, int noOfCartoon, decimal qty)
        {
            int indexF = rackBins.FindIndex(x => x.ChildRackBinID == childRackBinID);
            if (indexF > -1)
            {
                if (rackBinOT == EPYSLTEXCore.Infrastructure.Static.EnumRackBinOperationType.Addition)
                {
                    rackBins[indexF].NoOfCone = rackBins[indexF].NoOfCone + noOfCone;
                    rackBins[indexF].NoOfCartoon = rackBins[indexF].NoOfCartoon + noOfCartoon;
                    rackBins[indexF].ReceiveQty = rackBins[indexF].ReceiveQty + qty;
                }
                else if (rackBinOT == EPYSLTEXCore.Infrastructure.Static.EnumRackBinOperationType.Deduction)
                {
                    rackBins[indexF].NoOfCone = rackBins[indexF].NoOfCone - noOfCone;
                    rackBins[indexF].NoOfCartoon = rackBins[indexF].NoOfCartoon - noOfCartoon;
                    rackBins[indexF].ReceiveQty = rackBins[indexF].ReceiveQty - qty;
                }
            }
            return rackBins;
        }
        public async Task SaveAsync(YarnReceiveMaster entity)
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
                        var addedChilds = entity.YarnReceiveChilds.FindAll(x => x.EntityState == EntityState.Added);
                        var count = 0;

                        entity.YarnReceiveChilds.ForEach(c =>
                        {
                            count += c.YarnReceiveChildRackBins.Where(y => y.EntityState == EntityState.Added).Count();
                        });

                        maxRackBinId = await _service.GetMaxIdAsync(TableNames.YARN_RECEIVE_CHILD_RACK_BIN, count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        //maxChildResultId = await _service.GetMaxIdAsync(TableNames.YARN_QC_REMARKS_CHILDRESULT, countChildResult, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt); // new 3/13/2023

                        List<YarnReceiveChildRackBin> yarnReceiveChildRackBins = new List<YarnReceiveChildRackBin>();
                        entity.YarnReceiveChilds.ForEach(item =>
                        {
                            yarnReceiveChildRackBins = new List<YarnReceiveChildRackBin>();
                            item.YarnReceiveChildRackBins.ToList().ForEach(itemc =>
                            {
                                if (itemc.EntityState == EntityState.Added)
                                {
                                    itemc.ChildRackBinID = maxRackBinId++;
                                    itemc.ChildID = item.ChildID;

                                    item.ReceiveID = entity.ReceiveID;
                                }
                                yarnReceiveChildRackBins.Add(CommonFunction.DeepClone(itemc));
                            });
                            item.YarnReceiveChildRackBins = yarnReceiveChildRackBins;
                        });
                        /*
                        foreach (var item in entity.YarnReceiveChilds)
                        {
                            foreach (var itemc in item.YarnReceiveChildRackBins)
                            {
                                switch (itemc.EntityState)
                                {
                                    case EntityState.Added:
                                        itemc.ChildRackBinID = maxRackBinId++;
                                        itemc.ChildID = item.ChildID;
                                        item.ReceiveID = entity.ReceiveID;
                                        break;
                                    case EntityState.Deleted:
                                        itemc.EntityState = EntityState.Deleted;
                                        break;
                                    case EntityState.Modified:
                                        itemc.EntityState = EntityState.Modified;
                                        break;
                                    default:
                                        break;
                                }
                            }
                            //await _service.SaveAsync(item.YarnReceiveChildRackBins, transaction);
                        }
                        */
                        break;

                    case EntityState.Unchanged:
                    case EntityState.Deleted:
                        entity.EntityState = EntityState.Deleted;
                        entity.YarnReceiveChilds.SetDeleted();
                        break;

                    default:
                        break;
                }

                await _service.SaveSingleAsync(entity, transaction);
                await _service.SaveAsync(entity.YarnReceiveChilds, transaction);
                List<YarnReceiveChildRackBin> YarnRackBinList = new List<YarnReceiveChildRackBin>();
                entity.YarnReceiveChilds.ForEach(x => YarnRackBinList.AddRange(x.YarnReceiveChildRackBins));

                var childs = YarnRackBinList.Where(x => x.ChildID == 0).ToList();
                if (childs.Count() > 0)
                {
                    throw new Exception("YarnReceiveChildId missing => SaveAsync => YarnRackBinAllocationService");
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
