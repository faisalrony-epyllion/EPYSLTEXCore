using Dapper;
using EPYSLTEX.Core.DTOs;
using EPYSLTEX.Core.Entities;
using EPYSLTEX.Core.Entities.Tex;
using EPYSLTEX.Core.GuardClauses;
using EPYSLTEX.Core.Interfaces.Repositories;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEX.Core.Statics;
using EPYSLTEX.Infrastrucure.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
public class CDALoanReturnService : ICDALoanReturnService
{
	private readonly IDapperCRUDService<CDALoanReturnMaster> _service;
	private readonly ISignatureRepository _signatureRepository;
	private readonly SqlConnection _connection;
	private SqlTransaction transaction;
	public CDALoanReturnService(IDapperCRUDService<CDALoanReturnMaster> service, ISignatureRepository signatureRepository)
	{
		_service = service; ;
		_signatureRepository = signatureRepository;
		_connection = service.Connection;
	}

	public async Task<List<CDALoanReturnMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo, string Flag)
	{
		string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By CLRM.CDALRetuenMasterID Desc" : paginationInfo.OrderBy;
		var sql = string.Empty;

		if (status == Status.Pending && Flag == "DC") //Delivery Challan Pending
		{
			sql =
			$@"Select CLRM.CDALRetuenMasterID, CLRM.LReturnDate, CLRM.LReturnNo, CLRM.LocationID, L.LocationName, 
				CLRM.CompanyID, CE.ShortName As CompanyName, ChallanNo, ChallanDate, Remarks 
				From CDALoanReturnMaster CLRM 
				Left Join {DbNames.EPYSL}..Location L On L.LocationID = CLRM.LocationID And L.LocationID IN(3,5,10) 
				Left Join {DbNames.EPYSL}..CompanyEntity CE On CE.CompanyID = CLRM.CompanyID
				Where CLRM.DCSendForApproval = 0 And CLRM.DCApprove = 0 And CLRM.GPSendForApproval = 0
				And CLRM.GPApprove = 0 And CLRM.DCCheckOut = 0 ";
		}
		else if (status == Status.ProposedForApproval && Flag == "DC") //Delivery Challan Send For Approval
		{
			sql =
			$@"Select CLRM.CDALRetuenMasterID, CLRM.LReturnDate, CLRM.LReturnNo, CLRM.LocationID, L.LocationName, 
				CLRM.CompanyID, CE.ShortName As CompanyName, ChallanNo, ChallanDate, Remarks 
				From CDALoanReturnMaster CLRM 
				Left Join {DbNames.EPYSL}..Location L On L.LocationID = CLRM.LocationID And L.LocationID IN(3,5,10) 
				Left Join {DbNames.EPYSL}..CompanyEntity CE On CE.CompanyID = CLRM.CompanyID
				Where CLRM.DCSendForApproval = 1 And CLRM.DCApprove = 0 And CLRM.GPSendForApproval = 0 
                And CLRM.GPApprove = 0 And CLRM.DCCheckOut = 0 "; 
        }
		else if (status == Status.Approved && Flag == "DC") //Delivery Challan Approval
		{
			sql =
			$@"Select CLRM.CDALRetuenMasterID, CLRM.LReturnDate, CLRM.LReturnNo, CLRM.LocationID, L.LocationName, 
				CLRM.CompanyID, CE.ShortName As CompanyName, ChallanNo, ChallanDate, Remarks 
				From CDALoanReturnMaster CLRM 
				Left Join {DbNames.EPYSL}..Location L On L.LocationID = CLRM.LocationID And L.LocationID IN(3,5,10) 
				Left Join {DbNames.EPYSL}..CompanyEntity CE On CE.CompanyID = CLRM.CompanyID
				Where CLRM.DCSendForApproval = 1 And CLRM.DCApprove = 1 -- And CLRM.GPSendForApproval = 0 And CLRM.GPApprove = 0 And CLRM.DCCheckOut = 0 				 
				";
		}
		else if (status == Status.Pending && Flag == "GP") //GP Pending
		{
			sql =
			$@"Select CLRM.CDALRetuenMasterID, CLRM.LReturnDate, CLRM.LReturnNo, CLRM.LocationID, L.LocationName, 
				CLRM.CompanyID, CE.ShortName As CompanyName, ChallanNo, ChallanDate, Remarks 
				From CDALoanReturnMaster CLRM 
				Left Join {DbNames.EPYSL}..Location L On L.LocationID = CLRM.LocationID And L.LocationID IN(3,5,10) 
				Left Join {DbNames.EPYSL}..CompanyEntity CE On CE.CompanyID = CLRM.CompanyID
			    Where CLRM.DCSendForApproval = 1 And CLRM.DCApprove = 1 And CLRM.GPSendForApproval = 0
				And CLRM.GPApprove = 0 And CLRM.DCCheckOut = 0 ";
		}
		else if (status == Status.ProposedForApproval && Flag == "GP") //GP Send For Approval
		{
			sql =
			$@"Select CLRM.CDALRetuenMasterID, CLRM.LReturnDate, CLRM.LReturnNo, CLRM.LocationID, L.LocationName, 
				CLRM.CompanyID, CE.ShortName As CompanyName, ChallanNo, ChallanDate, Remarks 
				From CDALoanReturnMaster CLRM 
				Left Join {DbNames.EPYSL}..Location L On L.LocationID = CLRM.LocationID And L.LocationID IN(3,5,10) 
				Left Join {DbNames.EPYSL}..CompanyEntity CE On CE.CompanyID = CLRM.CompanyID
				Where CLRM.DCSendForApproval = 1 And CLRM.DCApprove = 1 And CLRM.GPSendForApproval = 1
				And CLRM.GPApprove = 0 And CLRM.DCCheckOut = 0 ";
		}
		else if (status == Status.Approved && Flag == "GP") //GatePass Approved
		{
			sql =
			$@"Select CLRM.CDALRetuenMasterID, CLRM.LReturnDate, CLRM.LReturnNo, CLRM.LocationID, L.LocationName, 
				CLRM.CompanyID, CE.ShortName As CompanyName, ChallanNo, ChallanDate, Remarks 
				From CDALoanReturnMaster CLRM 
				Left Join {DbNames.EPYSL}..Location L On L.LocationID = CLRM.LocationID And L.LocationID IN(3,5,10) 
				Left Join {DbNames.EPYSL}..CompanyEntity CE On CE.CompanyID = CLRM.CompanyID
				Where CLRM.DCSendForApproval = 1 And CLRM.DCApprove = 1 And CLRM.GPSendForApproval = 1
				And CLRM.GPApprove = 1 --And CLRM.DCCheckOut = 0 
				";
		}
		else if (status == Status.Pending && Flag == "CHK") //Checkout Pending
		{
			sql =
			$@"Select CLRM.CDALRetuenMasterID, CLRM.LReturnDate, CLRM.LReturnNo, CLRM.LocationID, L.LocationName, 
				CLRM.CompanyID, CE.ShortName As CompanyName, ChallanNo, ChallanDate, Remarks 
				From CDALoanReturnMaster CLRM 
				Left Join {DbNames.EPYSL}..Location L On L.LocationID = CLRM.LocationID And L.LocationID IN(3,5,10) 
				Left Join {DbNames.EPYSL}..CompanyEntity CE On CE.CompanyID = CLRM.CompanyID
				Where CLRM.DCSendForApproval = 1 And CLRM.DCApprove = 1 And CLRM.GPSendForApproval = 1
				And CLRM.GPApprove = 1 And CLRM.DCCheckOut = 0 ";
		}
		else if (status == Status.Check && Flag == "CHK") //Checkout Approved
		{
			sql =
			$@"Select CLRM.CDALRetuenMasterID, CLRM.LReturnDate, CLRM.LReturnNo, CLRM.LocationID, L.LocationName, 
				CLRM.CompanyID, CE.ShortName As CompanyName, ChallanNo, ChallanDate, Remarks 
				From CDALoanReturnMaster CLRM 
				Left Join {DbNames.EPYSL}..Location L On L.LocationID = CLRM.LocationID And L.LocationID IN(3,5,10) 
				Left Join {DbNames.EPYSL}..CompanyEntity CE On CE.CompanyID = CLRM.CompanyID 
				Where CLRM.DCSendForApproval = 1 And CLRM.DCApprove = 1 And CLRM.GPSendForApproval = 1
				And CLRM.GPApprove = 1 And CLRM.DCCheckOut = 1 ";
		}
		else
		{
			sql =
			$@"Select CLRM.CDALRetuenMasterID, CLRM.LReturnDate, CLRM.LReturnNo, CLRM.LocationID, L.LocationName, 
				CLRM.CompanyID, CE.ShortName As CompanyName, ChallanNo, ChallanDate, Remarks 
				From CDALoanReturnMaster CLRM 
				Left Join {DbNames.EPYSL}..Location L On L.LocationID = CLRM.LocationID And L.LocationID IN(3,5,10) 
				Left Join {DbNames.EPYSL}..CompanyEntity CE On CE.CompanyID = CLRM.CompanyID 
				Where CLRM.DCSendForApproval = 0 And CLRM.DCApprove = 0 And CLRM.GPSendForApproval = 0
				And CLRM.GPApprove = 0 And CLRM.DCCheckOut = 0 ";
		} 

		sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

		return await _service.GetDataAsync<CDALoanReturnMaster>(sql);
	}

	public async Task<CDALoanReturnMaster> GetNewAsync()
	{
		var query =

		$@"
		-- Item Adjustment 
		With CE As 
		(
			Select CLRC.CDALReceiveChildID As CDALReturnAdjID, CLRC.CDALReceiveChildID, CLRC.ItemMasterID, CLRC.BatchNo, CLRC.ExpiryDate, 
			CLRC.UnitID, CLRC.ReceiveQty, CLRC.Rate, (CLRC.ReceiveQty * CLRC.Rate)TotalValue, U.DisplayUnitDesc, 
			ISV1.SegmentValue AS Segment1ValueDesc, ISV2.SegmentValue AS Segment2ValueDesc, ISV3.SegmentValue AS Segment3ValueDesc,
			ISV4.SegmentValue AS Segment4ValueDesc, ISV5.SegmentValue AS Segment5ValueDesc, ISV6.SegmentValue AS Segment6ValueDesc,
			ISV7.SegmentValue AS Segment7ValueDesc 
			From CDALoanReceiveChild CLRC
			INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = CLRC.ItemMasterID 
			LEFT JOIN {DbNames.EPYSL}..Unit U ON U.UnitID = CLRC.UnitID
			LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
			LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
			LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
			LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
			LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
			LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
			LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID 
			Group By CLRC.CDALReceiveChildID, CLRC.ItemMasterID, CLRC.BatchNo, CLRC.ExpiryDate, 
			CLRC.UnitID, CLRC.ReceiveQty, CLRC.Rate, CLRC.Remarks, U.DisplayUnitDesc, 
			ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue, ISV4.SegmentValue, ISV5.SegmentValue,
			ISV6.SegmentValue, ISV7.SegmentValue
		),
		CRET As 
		( 
			Select CDALReceiveChildID, ItemMasterID, SUM((AdjustQty * Rate))TotalValue 	
			From CDALoanReturnChildAdjutment
			Group By CDALReceiveChildID, ItemMasterID
		)
		Select CE.CDALReturnAdjID, CE.CDALReceiveChildID, CE.ItemMasterID, CE.BatchNo, CE.ExpiryDate, CE.UnitID, 
		CE.ReceiveQty As AdjustQty, CE.Rate, CE.TotalValue, Isnull(CRET.TotalValue,0)TotalValue, CE.DisplayUnitDesc, CE.Segment1ValueDesc, CE.Segment2ValueDesc, 
		CE.Segment3ValueDesc, CE.Segment4ValueDesc, CE.Segment5ValueDesc, CE.Segment6ValueDesc,
		CE.Segment7ValueDesc  
		From CE 
		Left Join CRET On CRET.CDALReceiveChildID = CE.CDALReceiveChildID And CRET.ItemMasterID = CE.ItemMasterID
		Where CE.TotalValue > Isnull(CRET.TotalValue,0);

        -- Entity Value Type
        SELECT CAST(ValueID AS VARCHAR) id, ValueName [text], EntityTypeID [desc]
        FROM {DbNames.EPYSL}..EntityTypeValue
        WHERE ValueName <> 'Select' AND ValueID NOT IN(42,86,216)
        ORDER BY ValueName; 

        -- CompanyList
        Select Cast (CE.CompanyID as varchar) [id] , CE.ShortName [text]
        From {DbNames.EPYSL}..CompanyEntity CE 
        Where Isnull(CE.BusinessNature,'') = 'TEX'
        Group by CE.CompanyID, CE.ShortName;

        -- Store Location
        SELECT CAST(LocationID AS VARCHAR) id, LocationName text
        FROM {DbNames.EPYSL}..Location
        WHERE LocationID IN(3,5,10);

        -- LoanProviderList
        SELECT CAST(C.ContactID AS VARCHAR) AS id, C.Name AS text
        FROM {DbNames.EPYSL}..Contacts C
        INNER JOIN {DbNames.EPYSL}..ContactAdditionalInfo CAI ON CAI.ContactID = C.ContactID
        INNER JOIN {DbNames.EPYSL}..ContactCategoryChild CCC ON CCC.ContactID = C.ContactID
        INNER JOIN {DbNames.EPYSL}..ContactCategoryHK HK ON HK.ContactCategoryID = CCC.ContactCategoryID
		INNER JOIN {DbNames.EPYSL}..SupplierItemGroupStatus SIGS ON SIGS.ContactID = C.ContactID
        WHERE HK.ContactCategoryID = 2 AND SIGS.SubGroupID = 39 AND CAI.InLand = 1 AND CAI.EPZ = 0
        ORDER BY C.Name;

		-- Vehicle Number 
        Select CAST(VehicleID AS VARCHAR) id, VehicleNumber [text]
        From {DbNames.EPYSL}..VehicleDetails; ";

		try
		{
			await _connection.OpenAsync();
			var records = await _connection.QueryMultipleAsync(query);
			CDALoanReturnMaster data = new CDALoanReturnMaster();
			data.ChildAdjutment = records.Read<CDALoanReturnChildAdjutment>().ToList(); 
			var entityTypes = await records.ReadAsync<Select2OptionModel>();
			data.TransportModeList = entityTypes.Where(x => x.desc == EntityTypeConstants.TRANSPORT_MODE.ToString());
			data.TransportTypeList = entityTypes.Where(x => x.desc == EntityTypeConstants.TRANSPORT_TYPE.ToString());
			data.CompanyList = await records.ReadAsync<Select2OptionModel>();
			data.IssueFromCompanyList = data.CompanyList;
			data.LocationList = await records.ReadAsync<Select2OptionModel>();
			data.LoanProviderList = await records.ReadAsync<Select2OptionModel>();
			data.VehichleList = await records.ReadAsync<Select2OptionModel>();
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

	public async Task<CDALoanReturnMaster> GetAsync(int id)
	{
		var query =
			$@"
            -- Master Data
            Select * From CDALoanReturnMaster Where CDALRetuenMasterID = {id};

			-- Child
            ;With
			CLRC As (
				Select * From CDALoanReturnChild Where CDALRetuenMasterID = {id}
			)
			Select CLRC.CDALReturnChildID, CLRC.CDALRetuenMasterID, CLRC.ItemMasterID, CLRC.BatchNo, 
			CLRC.ExpiryDate, CLRC.UnitID, CLRC.ReturnQty, CLRC.Rate, CLRC.Remarks, U.DisplayUnitDesc, 
			IM.Segment1ValueID Segment1ValueId, 
			IM.Segment2ValueID Segment2ValueId, IM.Segment3ValueID Segment3ValueId, IM.Segment4ValueID Segment4ValueId, 
			IM.Segment5ValueID Segment5ValueId, IM.Segment6ValueID Segment6ValueId, IM.Segment7ValueID Segment7ValueId, 
			ISV1.SegmentValue AS Segment1ValueDesc, ISV2.SegmentValue AS Segment2ValueDesc, ISV3.SegmentValue AS Segment3ValueDesc,
			ISV4.SegmentValue AS Segment4ValueDesc, ISV5.SegmentValue AS Segment5ValueDesc, ISV6.SegmentValue AS Segment6ValueDesc,
			ISV7.SegmentValue AS Segment7ValueDesc 
			From CLRC
			INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = CLRC.ItemMasterID 
			LEFT JOIN {DbNames.EPYSL}..Unit U ON U.UnitID = CLRC.UnitID
			LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
			LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
			LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
			LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
			LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
			LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
			LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID 
			Group By CLRC.CDALReturnChildID, CLRC.CDALRetuenMasterID, CLRC.ItemMasterID, CLRC.BatchNo, 
			CLRC.ExpiryDate, CLRC.UnitID, CLRC.ReturnQty, CLRC.Rate, CLRC.Remarks, U.DisplayUnitDesc, IM.Segment1ValueID, 
			IM.Segment2ValueID, IM.Segment3ValueID, IM.Segment4ValueID, IM.Segment5ValueID, IM.Segment6ValueID, 
			IM.Segment7ValueID, ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue, ISV4.SegmentValue, ISV5.SegmentValue,
			ISV6.SegmentValue, ISV7.SegmentValue;

			-- Chlild Adjustment
			Select CLRC.CDALReceiveChildID, Isnull(CLRCA.CDALReturnAdjID,0) As CDALReturnAdjID, CLRCA.CDALRetuenMasterID,
			CLRCA.CDALReturnChildID, CLRC.ItemMasterID, CLRC.BatchNo, CLRC.ExpiryDate, 
			CLRC.UnitID, CLRC.ReceiveQty, CLRCA.AdjustQty, CLRC.Rate, (CLRC.ReceiveQty * CLRC.Rate)TotalValue, U.DisplayUnitDesc, 
			ISV1.SegmentValue AS Segment1ValueDesc, ISV2.SegmentValue AS Segment2ValueDesc, ISV3.SegmentValue AS Segment3ValueDesc,
			ISV4.SegmentValue AS Segment4ValueDesc, ISV5.SegmentValue AS Segment5ValueDesc, ISV6.SegmentValue AS Segment6ValueDesc,
			ISV7.SegmentValue AS Segment7ValueDesc 
			From CDALoanReceiveChild CLRC
			INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = CLRC.ItemMasterID 
			LEFT JOIN {DbNames.EPYSL}..Unit U ON U.UnitID = CLRC.UnitID
			LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
			LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
			LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
			LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
			LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
			LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
			LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
			Left Join CDALoanReturnChildAdjutment CLRCA On CLRCA.CDALReceiveChildID = CLRC.CDALReceiveChildID
			Group By CLRC.CDALReceiveChildID, CLRCA.CDALReturnAdjID, CLRCA.CDALRetuenMasterID,
			CLRCA.CDALReturnChildID, CLRC.ItemMasterID, CLRC.BatchNo, CLRC.ExpiryDate, 
			CLRC.UnitID, CLRC.ReceiveQty, CLRCA.AdjustQty, CLRC.Rate, CLRC.Remarks, U.DisplayUnitDesc, 
			ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue, ISV4.SegmentValue, ISV5.SegmentValue,
			ISV6.SegmentValue, ISV7.SegmentValue;

            -- Entity Value Type
            SELECT CAST(ValueID AS VARCHAR) id, ValueName [text], EntityTypeID [desc]
            FROM {DbNames.EPYSL}..EntityTypeValue
            WHERE ValueName <> 'Select' AND ValueID NOT IN(42,86,216)
            ORDER BY ValueName; 

            -- CompanyList
            Select Cast (CE.CompanyID as varchar) [id] , CE.ShortName [text]
            From {DbNames.EPYSL}..CompanyEntity CE 
            Where Isnull(CE.BusinessNature,'') = 'TEX'
            Group by CE.CompanyID, CE.ShortName;

            -- Store Location
            SELECT CAST(LocationID AS VARCHAR) id, LocationName text
            FROM {DbNames.EPYSL}..Location
            WHERE LocationID IN(3,5,10);

            -- LoanProviderList
            SELECT CAST(C.ContactID AS VARCHAR) AS id, C.Name AS text
            FROM {DbNames.EPYSL}..Contacts C
            INNER JOIN {DbNames.EPYSL}..ContactAdditionalInfo CAI ON CAI.ContactID = C.ContactID
            INNER JOIN {DbNames.EPYSL}..ContactCategoryChild CCC ON CCC.ContactID = C.ContactID
            INNER JOIN {DbNames.EPYSL}..ContactCategoryHK HK ON HK.ContactCategoryID = CCC.ContactCategoryID
            INNER JOIN {DbNames.EPYSL}..SupplierItemGroupStatus SIGS ON SIGS.ContactID = C.ContactID
            WHERE HK.ContactCategoryID = 2 AND SIGS.SubGroupID = 39 AND CAI.InLand = 1 AND CAI.EPZ = 0
            ORDER BY C.Name;

			-- Vehicle Number 
			Select CAST(VehicleID AS VARCHAR) id, VehicleNumber [text]
			From {DbNames.EPYSL}..VehicleDetails; ";

		try
		{
			await _connection.OpenAsync();
			var records = await _connection.QueryMultipleAsync(query);
			CDALoanReturnMaster data = await records.ReadFirstOrDefaultAsync<CDALoanReturnMaster>();
			data.Childs = records.Read<CDALoanReturnChild>().ToList();
			data.ChildAdjutment = records.Read<CDALoanReturnChildAdjutment>().ToList(); 
			var entityTypes = await records.ReadAsync<Select2OptionModel>();
			data.TransportModeList = entityTypes.Where(x => x.desc == EntityTypeConstants.TRANSPORT_MODE.ToString());
			data.TransportTypeList = entityTypes.Where(x => x.desc == EntityTypeConstants.TRANSPORT_TYPE.ToString());
			data.CompanyList = await records.ReadAsync<Select2OptionModel>();
			data.IssueFromCompanyList = data.CompanyList;
			data.LocationList = await records.ReadAsync<Select2OptionModel>();
			data.LoanProviderList = await records.ReadAsync<Select2OptionModel>();
			data.VehichleList = await records.ReadAsync<Select2OptionModel>();
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
	public async Task<CDALoanReturnMaster> GetAllAsync(int id)
	{
		var sql = $@"
            ;Select * From CDALoanReturnMaster Where CDALRetuenMasterID = {id}
            ;Select * From CDALoanReturnChild Where CDALRetuenMasterID = {id}
			;Select * From CDALoanReturnChildAdjutment Where CDALRetuenMasterID = {id}";

		try
		{
			await _connection.OpenAsync();
			var records = await _connection.QueryMultipleAsync(sql);
			CDALoanReturnMaster data = await records.ReadFirstOrDefaultAsync<CDALoanReturnMaster>();
			Guard.Against.NullObject(data);
			data.Childs = records.Read<CDALoanReturnChild>().ToList();
			data.ChildAdjutment = records.Read<CDALoanReturnChildAdjutment>().ToList(); 
			data.Childs.ForEach(x =>
			{
				x.ChildAdjutment = data.ChildAdjutment.Where(c => c.CDALReturnChildID == x.CDALReturnChildID).ToList();
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
	public async Task SaveAsync(CDALoanReturnMaster entity)
	{
		try
		{
			await _connection.OpenAsync();
			transaction = _connection.BeginTransaction();

			int maxChildId = 0, maxChildAdjId = 0;
			switch (entity.EntityState)
			{
				case EntityState.Added:
					entity.CDALRetuenMasterID = await _signatureRepository.GetMaxIdAsync(TableNames.CDA_LOAN_RETURN_MASTER);
					entity.LReturnNo = _signatureRepository.GetMaxNo(TableNames.CDA_LOAN_RETURN_NO);
					entity.ChallanNo = _signatureRepository.GetMaxNo(TableNames.CDA_LOAN_RETURN_CHALLAN_NO); 
					maxChildId = await _signatureRepository.GetMaxIdAsync(TableNames.CDA_LOAN_RETURN_CHILD, entity.Childs.Count);
					maxChildAdjId = await _signatureRepository.GetMaxIdAsync(TableNames.CDA_LOAN_RETURN_CHILD_ADJUTMENT, entity.Childs.Sum(x => x.ChildAdjutment.Count)); 
					 
					foreach (var item in entity.Childs)
					{
						item.CDALReturnChildID = maxChildId++;
						item.CDALRetuenMasterID = entity.CDALRetuenMasterID;

						foreach (CDALoanReturnChildAdjutment itemDtls in item.ChildAdjutment)
						{
							itemDtls.CDALReturnAdjID = maxChildAdjId++;
							itemDtls.CDALReturnChildID = item.CDALReturnChildID;
							itemDtls.CDALRetuenMasterID = item.CDALRetuenMasterID;
						}
					}
					 
					break;

				case EntityState.Modified:
					//var addedChilds = entity.Childs.FindAll(x => x.EntityState == EntityState.Added);
					//maxChildId = await _signatureRepository.GetMaxIdAsync(TableNames.CDA_LOAN_RETURN_CHILD, addedChilds.Count);

					//var addedChildsAdj = entity.Childs.FindAll(x => x.EntityState == EntityState.Added);
					//maxChildAdjId = await _signatureRepository.GetMaxIdAsync(TableNames.CDA_LOAN_RETURN_CHILD_ADJUTMENT, addedChilds.Count);

					maxChildId = await _signatureRepository.GetMaxIdAsync(TableNames.CDA_LOAN_RETURN_CHILD, entity.Childs.Count(x => x.EntityState == EntityState.Added));
					maxChildAdjId = await _signatureRepository.GetMaxIdAsync(TableNames.CDA_LOAN_RETURN_CHILD_ADJUTMENT, entity.Childs.Sum(x => x.ChildAdjutment.Where(y => y.EntityState == EntityState.Added).ToList().Count));

					if (entity.GPFlag)
					{
						entity.GPNo = _signatureRepository.GetMaxNo(TableNames.CDA_LOAN_RETURN_GATE_PASS_NO);
					}
					else
					{
						entity.GPNo = "";
						entity.GPDate = null;
					}

					foreach (CDALoanReturnChild child in entity.Childs)
					{
						if (child.EntityState == EntityState.Added)
						{
							child.CDALReturnChildID = maxChildId++;
							child.CDALRetuenMasterID = entity.CDALRetuenMasterID;

							foreach (CDALoanReturnChildAdjutment itemDtls in child.ChildAdjutment.ToList())
							{
								itemDtls.CDALReturnAdjID = maxChildAdjId++;
								itemDtls.CDALReturnChildID = child.CDALReturnChildID;
								itemDtls.CDALRetuenMasterID = child.CDALRetuenMasterID;
								child.EntityState = EntityState.Added;
							} 
						}
						else if (child.EntityState == EntityState.Modified)
						{
							foreach (CDALoanReturnChildAdjutment itemDtls in child.ChildAdjutment.Where(y => y.EntityState == EntityState.Added).ToList())
							{
								itemDtls.CDALReturnAdjID = maxChildAdjId++;
								itemDtls.CDALReturnChildID = child.CDALReturnChildID;
								itemDtls.CDALRetuenMasterID = child.CDALRetuenMasterID;
								child.EntityState = EntityState.Added;
							} 
						}
						else if (child.EntityState == EntityState.Deleted)
						{
							child.ChildAdjutment.SetDeleted();
							List<CDALoanReturnChildAdjutment> cItems = new List<CDALoanReturnChildAdjutment>();
							entity.Childs.ForEach(x => cItems.AddRange(x.ChildAdjutment.Where(y => y.EntityState == EntityState.Deleted)));
							await _service.SaveAsync(cItems, transaction); 
						}
					} 
					break;

				default:
					break;
			}

			await _service.SaveSingleAsync(entity, transaction);
			await _service.SaveAsync(entity.Childs, transaction);
			List<CDALoanReturnChildAdjutment> childItems = new List<CDALoanReturnChildAdjutment>();
			entity.Childs.ForEach(x =>
			{
				childItems.AddRange(x.ChildAdjutment);
			});
			await _service.SaveAsync(childItems, transaction);

			transaction.Commit();
		}
		catch (Exception ex)
		{
			if (transaction != null) transaction.Rollback();
			throw ex;
		}
		finally
		{
			if (transaction != null) transaction.Dispose();
			_connection.Close();
		}
	}
	public async Task UpdateEntityAsync(CDALoanReturnMaster entity)
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
