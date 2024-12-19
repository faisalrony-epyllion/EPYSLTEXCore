using Dapper;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Application.Interfaces.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Yarn;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Entity;

namespace EPYSLTEXCore.Application.Services.Inventory
{
    public class YarnMRIRService : IYarnMRIRService
    {
        private readonly IDapperCRUDService<YarnMRIRMaster> _service;
        private readonly IYarnAllocationService _yarnAllocationService;
        private readonly SqlConnection _connection;
        private readonly SqlConnection _connectionGmt;
        
        public YarnMRIRService(IDapperCRUDService<YarnMRIRMaster> service
            , IYarnAllocationService yarnAllocationService
			)
        {
            _service = service;
            //_connection = service.Connection;
            _yarnAllocationService = yarnAllocationService;
            _service.Connection = service.GetConnection(AppConstants.GMT_CONNECTION);
            _connectionGmt = service.Connection;

            _service.Connection = service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _connection = service.Connection;
        }

        public async Task<List<YarnMRIRChild>> GetPagedAsync(Status status, PaginationInfo paginationInfo, LoginUser AppUser)
        {
            string tempGuid = CommonFunction.GetNewGuid();

            string orderBy = "";
            if (status == Status.Pending || status == Status.Pending2 || status == Status.Pending3)
                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By ReceiveChildID Desc" : paginationInfo.OrderBy;
            else
                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By MRIRMasterID Desc" : paginationInfo.OrderBy;
            string sql = String.Empty;
            if (status == Status.Pending)
            {
                sql = $@"With YQCRemarks As 
				(
					Select C.QCRemarksChildID, M.QCRemarksMasterID, M.QCRemarksNo, M.QCRemarksDate, ER.EmployeeName QCRemarksByUser
					, M.QCReceiveMasterID, M.QCIssueMasterID, M.QCReqMasterID, ERCU.EmployeeName QCReceiveByUser, QCReqFor.ValueName QCReqFor
					, YRC2.ReceiveQty, C.ReceiveQtyCone, C.ReceiveQtyCarton,
					ISV1.SegmentValue Segment1ValueDesc,ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc,
					ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc,ISV6.SegmentValue Segment6ValueDesc,
					ISV7.SegmentValue Segment7ValueDesc,C.ShadeCode, RM.QCReqNo, RCM.QCReceiveNo, QCIM.QCIssueNo,
					C.Remarks, 
					YarnStatus = Z.YarnStatus,
					[Status] = CASE WHEN C.Approve = 1 THEN 'Approve'
						WHEN C.Reject = 1 THEN 'Reject'
						WHEN C.Retest = 1 THEN 'Retest'
						WHEN C.Diagnostic = 1 THEN 'Diagnostic'
						WHEN C.CommerciallyApprove = 1 THEN 'CommerciallyApprove' 
						ELSE '' END,
					TestType=CASE WHEN ((C.Approve=1 OR C.CommerciallyApprove=1 OR C.Diagnostic=1) AND M.IsApproved = 1) THEN 'Tested'
								WHEN YRC2.IsNoTest=1 THEN 'No Test Required' 
								WHEN C.ReTest=1 OR M.IsRetest=1 OR YMC.ReTest=1 THEN 'Re-Test' End,
					YRM.ReceiveNo, YRM.ReceiveDate,
					SpinnerID = CASE WHEN YRC2.SpinnerID > 0 THEN YRC2.SpinnerID ELSE YRM.SpinnerID END,
					YRC2.LotNo, YRC2.ChallanLot,
					YarnDetail = CONCAT(ISV6.SegmentValue,' ', ISV1.SegmentValue,' ', ISV3.SegmentValue,' ', ISV4.SegmentValue,' ', ISV2.SegmentValue,' ', ISV5.SegmentValue,' ', C.ShadeCode),
					TechnicalName = T.TechnicalName,
					BuyerName = CASE WHEN RMC1.BuyerID > 0 THEN B.ShortName ELSE '' END,
					YRM.PONo,YPC.POQty,YRM.InvoiceNo,YRM.ChallanNo,C.POCount,YRC2.PhysicalCount,YRC2.YarnControlNo,
					RM.LocationId RackLocationId,YRC2.ChildID ReceiveChildID,YRM.VehicalNo,CE.ShortName POUnit,CSU.ShortName Supplier,
					YPM.CompanyID,YPM.CompanyID UnitId,YRM.RCompanyID,YRM.SupplierID,YRC2.ItemMasterId,YRC2.Rate,YRC2.YarnProgramId,
					YRC2.ChallanCount,YRC2.YarnCategory,RMC1.NoOfThread,YMC.ReceiveQty ReceiveNoteQty
					,AllocationChildID = ISNULL(PRC.AllocationChildID,0), RFH.ReceiveFrom
					From {TableNames.YarnReceiveChild} YRC2 
					--INNER JOIN {TableNames.YarnStockSet} YSS ON YSS.YarnStockSetId = YRC2.YarnStockSetId
					INNER JOIN {TableNames.YarnReceiveMaster} YRM ON YRM.ReceiveID = YRC2.ReceiveID
					LEFT Join {TableNames.YarnQCReqChild} RMC1 On RMC1.ReceiveChildID = YRC2.ChildID
					LEFT Join {TableNames.YarnQCReqMaster} RM On RM.QCReqMasterID = RMC1.QCReqMasterID
					LEFT JOIN {TableNames.YarnQCIssueChild} QCIC ON QCIC.QCReqChildID = RMC1.QCReqChildID
					LEFT JOIN {TableNames.YarnQCIssueMaster} QCIM ON QCIM.QCIssueMasterID = QCIC.QCIssueMasterID
					LEFT JOIN {TableNames.YarnQCReceiveChild} QCRC ON QCRC.QCIssueChildID = QCIC.QCIssueChildID
					LEFT Join {TableNames.YarnQCReceiveMaster} RCM On RCM.QCReceiveMasterID = QCRC.QCReceiveMasterID
					LEFT Join {TableNames.YarnQCRemarksChild} C ON C.QCReceiveChildID=QCRC.QCReceiveChildID
					LEFT Join {TableNames.YarnQCRemarksMaster} M On M.QCRemarksMasterID = C.QCRemarksMasterID
					LEFT JOIN {TableNames.YarnPOMaster} YPM ON YPM.YPOMasterID=YRM.POID
					LEFT JOIN {TableNames.YarnPOChild} YPC ON YPC.YPOChildID=YRC2.POChildID
					LEFT JOIN {TableNames.YarnPRChild} PRC ON PRC.YarnPRChildID = YPC.PRChildID
					LEFT JOIN {TableNames.YarnMRIRChild} YMC ON YMC.ReceiveChildID=YRC2.ChildID--YMC.QCRemarksChildID=C.QCRemarksChildID
					LEFT JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID=YPM.CompanyID
					Left Join {DbNames.EPYSL}..EntityTypeValue QCReqFor On RM.QCForID = QCReqFor.ValueID
					LEFT Join {DbNames.EPYSL}..LoginUser RU On M.QCRemarksBy = RU.UserCode 
					LEFT Join {DbNames.EPYSL}..Employee ER On ER.EmployeeCode = RU.EmployeeCode
					LEFT Join {DbNames.EPYSL}..LoginUser RCU On RCM.QCReceivedBy = RCU.UserCode
					LEFT Join {DbNames.EPYSL}..Employee ERCU On ERCU.EmployeeCode = RCU.EmployeeCode
					LEFT JOIN {TableNames.FabricTechnicalName} T ON T.TechnicalNameId = RMC1.TechnicalNameId
					LEFT JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = RMC1.BuyerID
					LEFT JOIN YarnAssessmentStatus Z ON Z.YarnStatusID = C.YarnStatusID
					LEFT Join {DbNames.EPYSL}..ItemMaster IM On IM.ItemMasterID = YRC2.ItemMasterID
					LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
					LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
					LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
					LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
					LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
					LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
					LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
					LEFT JOIN {DbNames.EPYSL}..Contacts CSU ON CSU.ContactID = YRM.SupplierID
					LEFT JOIN ReceiveFrom_HK RFH ON RFH.ReceiveFromID = YRM.ReceiveFromID
					Where YRM.ApprovedDate >= '{CommonConstent.StockMigrationDate}'
					AND (YPM.CompanyID in(6,8) OR ISNULL(YRM.RCompanyID,0) in (6,8)) 
					AND (((C.Approve=1 OR C.CommerciallyApprove=1) AND M.IsApproved = 1) OR YRC2.IsNoTest=1) 
					AND YMC.MRIRChildID is null
				),
				YQCTaggedRemarks As 
				(
					Select C.QCRemarksChildID, M.QCRemarksMasterID, M.QCRemarksNo, M.QCRemarksDate, ER.EmployeeName QCRemarksByUser
					, M.QCReceiveMasterID, M.QCIssueMasterID, M.QCReqMasterID, ERCU.EmployeeName QCReceiveByUser, QCReqFor.ValueName QCReqFor
					, YRC.ReceiveQty, C.ReceiveQtyCone, C.ReceiveQtyCarton,
					ISV1.SegmentValue Segment1ValueDesc,ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc,
					ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc,ISV6.SegmentValue Segment6ValueDesc,
					ISV7.SegmentValue Segment7ValueDesc,C.ShadeCode, RM.QCReqNo, RCM.QCReceiveNo, QCIM.QCIssueNo,
					C.Remarks, 
					YarnStatus = Z.YarnStatus,
					[Status] = CASE WHEN C.Approve = 1 THEN 'Approve'
						WHEN C.Reject = 1 THEN 'Reject'
						WHEN C.Retest = 1 THEN 'Retest'
						WHEN C.Diagnostic = 1 THEN 'Diagnostic'
						WHEN C.CommerciallyApprove = 1 THEN 'CommerciallyApprove' 
						ELSE '' END,
					TestType=CASE WHEN ((C.Approve=1 OR C.CommerciallyApprove=1 OR C.Diagnostic=1) AND M.IsApproved = 1) THEN 'Tested'
								WHEN YRC.IsNoTest=1 THEN 'No Test Required' 
								WHEN C.ReTest=1 OR M.IsRetest=1 OR YMC.ReTest=1 THEN 'Re-Test' End,
					YRM.ReceiveNo, YRM.ReceiveDate,
					SpinnerID = CASE WHEN YRC.SpinnerID > 0 THEN YRC.SpinnerID ELSE YRM.SpinnerID END,
					YRC.LotNo, YRC.ChallanLot,
					YarnDetail = CONCAT(ISV6.SegmentValue,' ', ISV1.SegmentValue,' ', ISV3.SegmentValue,' ', ISV4.SegmentValue,' ', ISV2.SegmentValue,' ', ISV5.SegmentValue,' ', C.ShadeCode),
					TechnicalName = T.TechnicalName,
					BuyerName = CASE WHEN RMC1.BuyerID > 0 THEN B.ShortName ELSE '' END,
					YRM.PONo,YPC.POQty,YRM.InvoiceNo,YRM.ChallanNo,C.POCount,YRC.PhysicalCount,YRC.YarnControlNo,
					RM.LocationId RackLocationId,YRC.ChildID ReceiveChildID,YRM.VehicalNo,CE.ShortName POUnit,CSU.ShortName Supplier,
					YPM.CompanyID,YPM.CompanyID UnitId,YRM.RCompanyID,YRM.SupplierID,YRC.ItemMasterId,YRC.Rate,YRC.YarnProgramId,
					YRC.ChallanCount,YRC.YarnCategory,RMC1.NoOfThread,YMC.ReceiveQty ReceiveNoteQty
					,AllocationChildID = ISNULL(PRC.AllocationChildID,0), RFH.ReceiveFrom
					From {TableNames.YarnReceiveChild} YRC
					--INNER JOIN YarnStockSet YSS ON YSS.YarnStockSetId = YRC.YarnStockSetId
					INNER JOIN {TableNames.YarnReceiveChild} YRC2 ON YRC2.ChildID = YRC.TagYarnReceiveChildID
					INNER JOIN {TableNames.YarnReceiveMaster} YRM ON YRM.ReceiveID = YRC.ReceiveID
					LEFT Join {TableNames.YarnQCReqChild} RMC1 On RMC1.ReceiveChildID = YRC2.ChildID
					LEFT Join {TableNames.YarnQCReqMaster} RM On RM.QCReqMasterID = RMC1.QCReqMasterID
					LEFT JOIN {TableNames.YarnQCIssueChild} QCIC ON QCIC.QCReqChildID = RMC1.QCReqChildID
					LEFT JOIN {TableNames.YarnQCIssueMaster} QCIM ON QCIM.QCIssueMasterID = QCIC.QCIssueMasterID
					LEFT JOIN {TableNames.YarnQCReceiveChild} QCRC ON QCRC.QCIssueChildID = QCIC.QCIssueChildID
					LEFT Join {TableNames.YarnQCReceiveMaster} RCM On RCM.QCReceiveMasterID = QCRC.QCReceiveMasterID
					LEFT Join {TableNames.YarnQCRemarksChild} C ON C.QCReceiveChildID=QCRC.QCReceiveChildID
					LEFT Join {TableNames.YarnQCRemarksMaster} M On M.QCRemarksMasterID = C.QCRemarksMasterID
					LEFT JOIN {TableNames.YarnPOMaster} YPM ON YPM.YPOMasterID=YRM.POID
					LEFT JOIN {TableNames.YarnPOChild} YPC ON YPC.YPOChildID=YRC.POChildID
					LEFT JOIN {TableNames.YarnPRChild} PRC ON PRC.YarnPRChildID = YPC.PRChildID
					LEFT JOIN {TableNames.YarnMRIRChild} YMC ON YMC.ReceiveChildID=YRC.ChildID
					LEFT JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID=YPM.CompanyID
					Left Join {DbNames.EPYSL}..EntityTypeValue QCReqFor On RM.QCForID = QCReqFor.ValueID
					LEFT Join {DbNames.EPYSL}..LoginUser RU On M.QCRemarksBy = RU.UserCode 
					LEFT Join {DbNames.EPYSL}..Employee ER On ER.EmployeeCode = RU.EmployeeCode
					LEFT Join {DbNames.EPYSL}..LoginUser RCU On RCM.QCReceivedBy = RCU.UserCode
					LEFT Join {DbNames.EPYSL}..Employee ERCU On ERCU.EmployeeCode = RCU.EmployeeCode
					LEFT JOIN {TableNames.FabricTechnicalName} T ON T.TechnicalNameId = RMC1.TechnicalNameId
					LEFT JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = RMC1.BuyerID
					LEFT JOIN YarnAssessmentStatus Z ON Z.YarnStatusID = C.YarnStatusID
					LEFT Join {DbNames.EPYSL}..ItemMaster IM On IM.ItemMasterID = YRC.ItemMasterID
					LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
					LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
					LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
					LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
					LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
					LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
					LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
					LEFT JOIN {DbNames.EPYSL}..Contacts CSU ON CSU.ContactID = YRM.SupplierID
					LEFT JOIN ReceiveFrom_HK RFH ON RFH.ReceiveFromID = YRM.ReceiveFromID
					Where YRM.ApprovedDate >= '{CommonConstent.StockMigrationDate}'
					AND (YPM.CompanyID in(6,8) OR ISNULL(YRM.RCompanyID,0) in (6,8)) 
					AND (C.Approve=1 OR C.CommerciallyApprove=1) AND M.IsApproved = 1
					AND YMC.MRIRChildID is null
				),
				FList AS
				(
					SELECT FL.*,Spinner = CASE WHEN FL.SpinnerID > 0 THEN CCS.ShortName ELSE '' END
					FROM YQCRemarks FL
					LEFT JOIN {DbNames.EPYSL}..Contacts CCS ON CCS.ContactID = FL.SpinnerID
				),
				FListTagged AS
				(
					SELECT FL.*,Spinner = CASE WHEN FL.SpinnerID > 0 THEN CCS.ShortName ELSE '' END
					FROM YQCTaggedRemarks FL
					LEFT JOIN {DbNames.EPYSL}..Contacts CCS ON CCS.ContactID = FL.SpinnerID
				),
				FinalList As(
					Select * from FList
					Union All
					Select * from FListTagged
				)
					SELECT * INTO #TempTable{tempGuid} FROM FinalList
                    SELECT *,Count(*) Over() TotalRows FROM #TempTable{tempGuid}";
            }
            else if (status == Status.Pending2)
            {
                sql = $@" With YQCRemarks As 
                    (
	                   Select C.QCRemarksChildID, M.QCRemarksMasterID, M.QCRemarksNo, M.QCRemarksDate, ER.EmployeeName QCRemarksByUser
	                    , M.QCReceiveMasterID, M.QCIssueMasterID, M.QCReqMasterID, ERCU.EmployeeName QCReceiveByUser, QCReqFor.ValueName QCReqFor
	                    , YRC2.ReceiveQty, C.ReceiveQtyCone, C.ReceiveQtyCarton,
	                    ISV1.SegmentValue Segment1ValueDesc,ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc,
	                    ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc,ISV6.SegmentValue Segment6ValueDesc,
	                    ISV7.SegmentValue Segment7ValueDesc,C.ShadeCode, RM.QCReqNo, RCM.QCReceiveNo, QCIM.QCIssueNo,
	                    C.Remarks, 
	                    YarnStatus = Z.YarnStatus,
	                    [Status] = CASE WHEN C.Approve = 1 THEN 'Approve'
					        WHEN C.Reject = 1 THEN 'Reject'
					        WHEN C.Retest = 1 THEN 'Retest'
					        WHEN C.Diagnostic = 1 THEN 'Diagnostic'
						    WHEN C.CommerciallyApprove = 1 THEN 'CommerciallyApprove' 
						    ELSE '' END,
					    TestType=CASE WHEN ((C.Approve=1 OR C.CommerciallyApprove=1 OR C.Diagnostic=1) AND M.IsApproved = 1) THEN 'Tested'
						         WHEN YRC2.IsNoTest=1 THEN 'No Test Required' 
								 WHEN C.ReTest=1 OR M.IsRetest=1 OR YMC.ReTest=1 THEN 'Re-Test' End,
	                    YRM.ReceiveNo, YRM.ReceiveDate,
	                    SpinnerID = CASE WHEN YRC2.SpinnerID > 0 THEN YRC2.SpinnerID ELSE YRM.SpinnerID END,
	                    YRC2.LotNo, YRC2.ChallanLot,
	                    YarnDetail = CONCAT(ISV6.SegmentValue,' ', ISV1.SegmentValue,' ', ISV3.SegmentValue,' ', ISV4.SegmentValue,' ', ISV2.SegmentValue,' ', ISV5.SegmentValue,' ', C.ShadeCode),
	                    TechnicalName = T.TechnicalName,
	                    BuyerName = CASE WHEN RMC1.BuyerID > 0 THEN B.ShortName ELSE '' END,
						YRM.PONo,YPC.POQty,YRM.InvoiceNo,YRM.ChallanNo,C.POCount,C.PhysicalCount,YRC2.YarnControlNo,
						RM.LocationId RackLocationId,YRC2.ChildID ReceiveChildID,YRM.VehicalNo,CE.ShortName POUnit,CSU.ShortName Supplier,
						YPM.CompanyID,YPM.CompanyID UnitId,YRM.RCompanyID,YRM.SupplierID,YRC2.ItemMasterId,YRC2.Rate,YRC2.YarnProgramId,
						YRC2.ChallanCount,YRC2.YarnCategory,RMC1.NoOfThread,YMC.ReceiveQty ReceiveNoteQty, RFH.ReceiveFrom

	                    From {TableNames.YarnReceiveChild} YRC2 
						LEFT JOIN {TableNames.YarnReceiveMaster} YRM ON YRM.ReceiveID = YRC2.ReceiveID
						LEFT Join {TableNames.YarnQCReqChild} RMC1 On RMC1.ReceiveChildID = YRC2.ChildID
	                    LEFT Join {TableNames.YarnQCReqMaster} RM On RM.QCReqMasterID = RMC1.QCReqMasterID
						LEFT JOIN {TableNames.YarnQCIssueChild} QCIC ON QCIC.QCReqChildID = RMC1.QCReqChildID
		                LEFT JOIN {TableNames.YarnQCIssueMaster} QCIM ON QCIM.QCIssueMasterID = QCIC.QCIssueMasterID
						LEFT JOIN {TableNames.YarnQCReceiveChild} QCRC ON QCRC.QCIssueChildID = QCIC.QCIssueChildID
	                    LEFT Join {TableNames.YarnQCReceiveMaster} RCM On RCM.QCReceiveMasterID = QCRC.QCReceiveMasterID
						LEFT Join {TableNames.YarnQCRemarksChild} C ON C.QCReceiveChildID=QCRC.QCReceiveChildID
						LEFT Join {TableNames.YarnQCRemarksMaster} M On M.QCRemarksMasterID = C.QCRemarksMasterID
						LEFT JOIN {TableNames.YarnPOMaster} YPM ON YPM.YPOMasterID=YRM.POID
                        LEFT JOIN {TableNames.YarnPOChild} YPC ON YPC.YPOChildID=YRC2.POChildID
                        LEFT JOIN {TableNames.YarnMRIRChild} YMC ON YMC.ReceiveChildID=YRC2.ChildID--YMC.QCRemarksChildID=C.QCRemarksChildID
						LEFT JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID=YPM.CompanyID
	                    Left Join {DbNames.EPYSL}..EntityTypeValue QCReqFor On RM.QCForID = QCReqFor.ValueID
	                    LEFT Join {DbNames.EPYSL}..LoginUser RU On M.QCRemarksBy = RU.UserCode 
					    LEFT Join {DbNames.EPYSL}..Employee ER On ER.EmployeeCode = RU.EmployeeCode
	                    LEFT Join {DbNames.EPYSL}..LoginUser RCU On RCM.QCReceivedBy = RCU.UserCode
					    LEFT Join {DbNames.EPYSL}..Employee ERCU On ERCU.EmployeeCode = RCU.EmployeeCode
	                    LEFT JOIN {TableNames.FabricTechnicalName} T ON T.TechnicalNameId = RMC1.TechnicalNameId
	                    LEFT JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = RMC1.BuyerID
	                    LEFT JOIN YarnAssessmentStatus Z ON Z.YarnStatusID = C.YarnStatusID
	                    LEFT Join {DbNames.EPYSL}..ItemMaster IM On IM.ItemMasterID = YRC2.ItemMasterID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
						LEFT JOIN {DbNames.EPYSL}..Contacts CSU ON CSU.ContactID = YRM.SupplierID
						LEFT JOIN ReceiveFrom_HK RFH ON RFH.ReceiveFromID = YRM.ReceiveFromID
                        Where (YPM.CompanyID in(11) OR YRM.RCompanyID in(11)) AND--(11=ESL)
						(((C.Approve=1 OR C.CommerciallyApprove=1) AND M.IsApproved = 1) OR YRC2.IsNoTest=1) AND
						YMC.MRIRChildID is null
	                    
                    ),
                    YQCTaggedRemarks As 
                    (
	                   Select C.QCRemarksChildID, M.QCRemarksMasterID, M.QCRemarksNo, M.QCRemarksDate, ER.EmployeeName QCRemarksByUser
	                    , M.QCReceiveMasterID, M.QCIssueMasterID, M.QCReqMasterID, ERCU.EmployeeName QCReceiveByUser, QCReqFor.ValueName QCReqFor
	                    , YRC.ReceiveQty, C.ReceiveQtyCone, C.ReceiveQtyCarton,
	                    ISV1.SegmentValue Segment1ValueDesc,ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc,
	                    ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc,ISV6.SegmentValue Segment6ValueDesc,
	                    ISV7.SegmentValue Segment7ValueDesc,C.ShadeCode, RM.QCReqNo, RCM.QCReceiveNo, QCIM.QCIssueNo,
	                    C.Remarks, 
	                    YarnStatus = Z.YarnStatus,
	                    [Status] = CASE WHEN C.Approve = 1 THEN 'Approve'
					        WHEN C.Reject = 1 THEN 'Reject'
					        WHEN C.Retest = 1 THEN 'Retest'
					        WHEN C.Diagnostic = 1 THEN 'Diagnostic'
						    WHEN C.CommerciallyApprove = 1 THEN 'CommerciallyApprove' 
						    ELSE '' END,
					    TestType=CASE WHEN ((C.Approve=1 OR C.CommerciallyApprove=1 OR C.Diagnostic=1) AND M.IsApproved = 1) THEN 'Tested'
						         WHEN YRC.IsNoTest=1 THEN 'No Test Required' 
								 WHEN C.ReTest=1 OR M.IsRetest=1 OR YMC.ReTest=1 THEN 'Re-Test' End,
	                    YRM.ReceiveNo, YRM.ReceiveDate,
	                    SpinnerID = CASE WHEN YRC.SpinnerID > 0 THEN YRC.SpinnerID ELSE YRM.SpinnerID END,
	                    YRC.LotNo, YRC.ChallanLot,
	                    YarnDetail = CONCAT(ISV6.SegmentValue,' ', ISV1.SegmentValue,' ', ISV3.SegmentValue,' ', ISV4.SegmentValue,' ', ISV2.SegmentValue,' ', ISV5.SegmentValue,' ', C.ShadeCode),
	                    TechnicalName = T.TechnicalName,
	                    BuyerName = CASE WHEN RMC1.BuyerID > 0 THEN B.ShortName ELSE '' END,
						YRM.PONo,YPC.POQty,YRM.InvoiceNo,YRM.ChallanNo,C.POCount,C.PhysicalCount,YRC.YarnControlNo,
						RM.LocationId RackLocationId,YRC.ChildID ReceiveChildID,YRM.VehicalNo,CE.ShortName POUnit,CSU.ShortName Supplier,
						YPM.CompanyID,YPM.CompanyID UnitId,YRM.RCompanyID,YRM.SupplierID,YRC.ItemMasterId,YRC.Rate,YRC.YarnProgramId,
						YRC.ChallanCount,YRC.YarnCategory,RMC1.NoOfThread,YMC.ReceiveQty ReceiveNoteQty, RFH.ReceiveFrom

	                    From {TableNames.YarnReceiveChild} YRC
						INNER JOIN {TableNames.YarnReceiveChild} YRC2 ON YRC2.ChildID = YRC.TagYarnReceiveChildID
						LEFT JOIN {TableNames.YarnReceiveMaster} YRM ON YRM.ReceiveID = YRC.ReceiveID
						LEFT Join {TableNames.YarnQCReqChild} RMC1 On RMC1.ReceiveChildID = YRC2.ChildID
	                    LEFT Join {TableNames.YarnQCReqMaster} RM On RM.QCReqMasterID = RMC1.QCReqMasterID
						LEFT JOIN {TableNames.YarnQCIssueChild} QCIC ON QCIC.QCReqChildID = RMC1.QCReqChildID
		                LEFT JOIN {TableNames.YarnQCIssueMaster} QCIM ON QCIM.QCIssueMasterID = QCIC.QCIssueMasterID
						LEFT JOIN {TableNames.YarnQCReceiveChild} QCRC ON QCRC.QCIssueChildID = QCIC.QCIssueChildID
	                    LEFT Join {TableNames.YarnQCReceiveMaster} RCM On RCM.QCReceiveMasterID = QCRC.QCReceiveMasterID
						LEFT Join {TableNames.YarnQCRemarksChild} C ON C.QCReceiveChildID=QCRC.QCReceiveChildID
						LEFT Join {TableNames.YarnQCRemarksMaster} M On M.QCRemarksMasterID = C.QCRemarksMasterID
						LEFT JOIN {TableNames.YarnPOMaster} YPM ON YPM.YPOMasterID=YRM.POID
                        LEFT JOIN {TableNames.YarnPOChild} YPC ON YPC.YPOChildID=YRC.POChildID
                        LEFT JOIN {TableNames.YarnMRIRChild} YMC ON YMC.ReceiveChildID=YRC.ChildID--YMC.QCRemarksChildID=C.QCRemarksChildID
						LEFT JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID=YPM.CompanyID
	                    Left Join {DbNames.EPYSL}..EntityTypeValue QCReqFor On RM.QCForID = QCReqFor.ValueID
	                    LEFT Join {DbNames.EPYSL}..LoginUser RU On M.QCRemarksBy = RU.UserCode 
					    LEFT Join {DbNames.EPYSL}..Employee ER On ER.EmployeeCode = RU.EmployeeCode
	                    LEFT Join {DbNames.EPYSL}..LoginUser RCU On RCM.QCReceivedBy = RCU.UserCode
					    LEFT Join {DbNames.EPYSL}..Employee ERCU On ERCU.EmployeeCode = RCU.EmployeeCode
	                    LEFT JOIN {TableNames.FabricTechnicalName} T ON T.TechnicalNameId = RMC1.TechnicalNameId
	                    LEFT JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = RMC1.BuyerID
	                    LEFT JOIN YarnAssessmentStatus Z ON Z.YarnStatusID = C.YarnStatusID
	                    LEFT Join {DbNames.EPYSL}..ItemMaster IM On IM.ItemMasterID = YRC.ItemMasterID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
						LEFT JOIN {DbNames.EPYSL}..Contacts CSU ON CSU.ContactID = YRM.SupplierID
						LEFT JOIN ReceiveFrom_HK RFH ON RFH.ReceiveFromID = YRM.ReceiveFromID
                        Where (YPM.CompanyID in(11) OR YRM.RCompanyID in(11)) AND--(11=ESL)
						(C.Approve=1 OR C.CommerciallyApprove=1) AND M.IsApproved = 1 AND
						--(((C.Approve=1 OR C.CommerciallyApprove=1) AND M.IsApproved = 1) OR YRC2.IsNoTest=1) AND
						YMC.MRIRChildID is null
	                    
                    ),
                    FList AS
                    (
	                    SELECT FL.*,Spinner = CASE WHEN FL.SpinnerID > 0 THEN CCS.ShortName ELSE '' END
	                    FROM YQCRemarks FL
	                    LEFT JOIN {DbNames.EPYSL}..Contacts CCS ON CCS.ContactID = FL.SpinnerID
                    )
					,
                    FListTagged AS
                    (
	                    SELECT FL.*,Spinner = CASE WHEN FL.SpinnerID > 0 THEN CCS.ShortName ELSE '' END
	                    FROM YQCTaggedRemarks FL
	                    LEFT JOIN {DbNames.EPYSL}..Contacts CCS ON CCS.ContactID = FL.SpinnerID
                    ),
					FinalList As(
					Select * from FList
					Union All
					Select * from FListTagged
					)
                    SELECT * INTO #TempTable{tempGuid} FROM FinalList
                    SELECT *,Count(*) Over() TotalRows FROM #TempTable{tempGuid}";
            }
            else if (status == Status.Pending3)
            {
                sql = $@" With YQCRemarks As 
                    (
	                   Select C.QCRemarksChildID, M.QCRemarksMasterID, M.QCRemarksNo, M.QCRemarksDate, ER.EmployeeName QCRemarksByUser
	                    , M.QCReceiveMasterID, M.QCIssueMasterID, M.QCReqMasterID, ERCU.EmployeeName QCReceiveByUser, QCReqFor.ValueName QCReqFor
	                    , YRC2.ReceiveQty, C.ReceiveQtyCone, C.ReceiveQtyCarton,
	                    ISV1.SegmentValue Segment1ValueDesc,ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc,
	                    ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc,ISV6.SegmentValue Segment6ValueDesc,
	                    ISV7.SegmentValue Segment7ValueDesc,C.ShadeCode, RM.QCReqNo, RCM.QCReceiveNo, QCIM.QCIssueNo,
	                    C.Remarks, 
	                    YarnStatus = Z.YarnStatus,
	                    [Status] = CASE WHEN C.Approve = 1 THEN 'Approve'
					        WHEN C.Reject = 1 THEN 'Reject'
					        WHEN C.Retest = 1 THEN 'Retest'
					        WHEN C.Diagnostic = 1 THEN 'Diagnostic'
						    WHEN C.CommerciallyApprove = 1 THEN 'CommerciallyApprove' 
						    ELSE '' END,
					    TestType=CASE WHEN ((C.Approve=1 OR C.CommerciallyApprove=1 OR C.Diagnostic=1) AND M.IsApproved = 1) THEN 'Tested'
						         WHEN YRC2.IsNoTest=1 THEN 'No Test Required' 
								 WHEN C.ReTest=1 OR M.IsRetest=1 OR YMC.ReTest=1 THEN 'Re-Test' End,
	                    YRM.ReceiveNo, YRM.ReceiveDate,
	                    SpinnerID = CASE WHEN YRC2.SpinnerID > 0 THEN YRC2.SpinnerID ELSE YRM.SpinnerID END,
	                    YRC2.LotNo, YRC2.ChallanLot,
	                    YarnDetail = CONCAT(ISV6.SegmentValue,' ', ISV1.SegmentValue,' ', ISV3.SegmentValue,' ', ISV4.SegmentValue,' ', ISV2.SegmentValue,' ', ISV5.SegmentValue,' ', C.ShadeCode),
	                    TechnicalName = T.TechnicalName,
	                    BuyerName = CASE WHEN RMC1.BuyerID > 0 THEN B.ShortName ELSE '' END,
						YRM.PONo,YPC.POQty,YRM.InvoiceNo,YRM.ChallanNo,C.POCount,C.PhysicalCount,YRC2.YarnControlNo,
						RM.LocationId RackLocationId,YRC2.ChildID ReceiveChildID,YRM.VehicalNo,CE.ShortName POUnit,CSU.ShortName Supplier,
						YPM.CompanyID,YPM.CompanyID UnitId,YRM.RCompanyID,YRM.SupplierID,YRC2.ItemMasterId,YRC2.Rate,YRC2.YarnProgramId,
						YRC2.ChallanCount,YRC2.YarnCategory,RMC1.NoOfThread,YMC.ReceiveQty ReceiveNoteQty, RFH.ReceiveFrom

	                    From {TableNames.YarnReceiveChild} YRC2 
						LEFT JOIN {TableNames.YarnReceiveMaster} YRM ON YRM.ReceiveID = YRC2.ReceiveID
						LEFT JOIN {TableNames.YarnQCReqChild} RMC1 On RMC1.ReceiveChildID = YRC2.ChildID
	                    LEFT JOIN {TableNames.YarnQCReqMaster} RM On RM.QCReqMasterID = RMC1.QCReqMasterID
						LEFT JOIN {TableNames.YarnQCIssueChild} QCIC ON QCIC.QCReqChildID = RMC1.QCReqChildID
		                LEFT JOIN {TableNames.YarnQCIssueMaster} QCIM ON QCIM.QCIssueMasterID = QCIC.QCIssueMasterID
						LEFT JOIN {TableNames.YarnQCReceiveChild} QCRC ON QCRC.QCIssueChildID = QCIC.QCIssueChildID
	                    LEFT JOIN {TableNames.YarnQCReceiveMaster} RCM On RCM.QCReceiveMasterID = QCRC.QCReceiveMasterID
						LEFT JOIN {TableNames.YarnQCRemarksChild} C ON C.QCReceiveChildID=QCRC.QCReceiveChildID
						LEFT JOIN {TableNames.YarnQCRemarksMaster} M On M.QCRemarksMasterID = C.QCRemarksMasterID
						LEFT JOIN {TableNames.YarnPOMaster} YPM ON YPM.YPOMasterID=YRM.POID
                        LEFT JOIN {TableNames.YarnPOChild} YPC ON YPC.YPOChildID=YRC2.POChildID
                        LEFT JOIN {TableNames.YarnMRIRChild} YMC ON YMC.ReceiveChildID=YRC2.ChildID--YMC.QCRemarksChildID=C.QCRemarksChildID
						LEFT JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID=YPM.CompanyID
	                    Left Join {DbNames.EPYSL}..EntityTypeValue QCReqFor On RM.QCForID = QCReqFor.ValueID
	                    LEFT JOIN {DbNames.EPYSL}..LoginUser RU On M.QCRemarksBy = RU.UserCode 
					    LEFT JOIN {DbNames.EPYSL}..Employee ER On ER.EmployeeCode = RU.EmployeeCode
	                    LEFT JOIN {DbNames.EPYSL}..LoginUser RCU On RCM.QCReceivedBy = RCU.UserCode
					    LEFT JOIN {DbNames.EPYSL}..Employee ERCU On ERCU.EmployeeCode = RCU.EmployeeCode
	                    LEFT JOIN {TableNames.FabricTechnicalName} T ON T.TechnicalNameId = RMC1.TechnicalNameId
	                    LEFT JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = RMC1.BuyerID
	                    LEFT JOIN YarnAssessmentStatus Z ON Z.YarnStatusID = C.YarnStatusID
	                    LEFT JOIN {DbNames.EPYSL}..ItemMaster IM On IM.ItemMasterID = YRC2.ItemMasterID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
						LEFT JOIN {DbNames.EPYSL}..Contacts CSU ON CSU.ContactID = YRM.SupplierID
						LEFT JOIN ReceiveFrom_HK RFH ON RFH.ReceiveFromID = YRM.ReceiveFromID
                        Where (YPM.CompanyID in(6,8,11) OR YRM.RCompanyID in(6,8,11)) AND--(6=EFL,8=EKL,11=ESL)
						C.Diagnostic=1 AND M.IsApproved = 1 AND
						YMC.MRIRChildID is null
	                    
                    ),
                    YQCTaggedRemarks As 
                    (
	                   Select C.QCRemarksChildID, M.QCRemarksMasterID, M.QCRemarksNo, M.QCRemarksDate, ER.EmployeeName QCRemarksByUser
	                    , M.QCReceiveMasterID, M.QCIssueMasterID, M.QCReqMasterID, ERCU.EmployeeName QCReceiveByUser, QCReqFor.ValueName QCReqFor
	                    , YRC.ReceiveQty, C.ReceiveQtyCone, C.ReceiveQtyCarton,
	                    ISV1.SegmentValue Segment1ValueDesc,ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc,
	                    ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc,ISV6.SegmentValue Segment6ValueDesc,
	                    ISV7.SegmentValue Segment7ValueDesc,C.ShadeCode, RM.QCReqNo, RCM.QCReceiveNo, QCIM.QCIssueNo,
	                    C.Remarks, 
	                    YarnStatus = Z.YarnStatus,
	                    [Status] = CASE WHEN C.Approve = 1 THEN 'Approve'
					        WHEN C.Reject = 1 THEN 'Reject'
					        WHEN C.Retest = 1 THEN 'Retest'
					        WHEN C.Diagnostic = 1 THEN 'Diagnostic'
						    WHEN C.CommerciallyApprove = 1 THEN 'CommerciallyApprove' 
						    ELSE '' END,
					    TestType=CASE WHEN ((C.Approve=1 OR C.CommerciallyApprove=1 OR C.Diagnostic=1) AND M.IsApproved = 1) THEN 'Tested'
						         WHEN YRC.IsNoTest=1 THEN 'No Test Required' 
								 WHEN C.ReTest=1 OR M.IsRetest=1 OR YMC.ReTest=1 THEN 'Re-Test' End,
	                    YRM.ReceiveNo, YRM.ReceiveDate,
	                    SpinnerID = CASE WHEN YRC.SpinnerID > 0 THEN YRC.SpinnerID ELSE YRM.SpinnerID END,
	                    YRC.LotNo, YRC.ChallanLot,
	                    YarnDetail = CONCAT(ISV6.SegmentValue,' ', ISV1.SegmentValue,' ', ISV3.SegmentValue,' ', ISV4.SegmentValue,' ', ISV2.SegmentValue,' ', ISV5.SegmentValue,' ', C.ShadeCode),
	                    TechnicalName = T.TechnicalName,
	                    BuyerName = CASE WHEN RMC1.BuyerID > 0 THEN B.ShortName ELSE '' END,
						YRM.PONo,YPC.POQty,YRM.InvoiceNo,YRM.ChallanNo,C.POCount,C.PhysicalCount,YRC.YarnControlNo,
						RM.LocationId RackLocationId,YRC.ChildID ReceiveChildID,YRM.VehicalNo,CE.ShortName POUnit,CSU.ShortName Supplier,
						YPM.CompanyID,YPM.CompanyID UnitId,YRM.RCompanyID,YRM.SupplierID,YRC.ItemMasterId,YRC.Rate,YRC.YarnProgramId,
						YRC.ChallanCount,YRC.YarnCategory,RMC1.NoOfThread,YMC.ReceiveQty ReceiveNoteQty, RFH.ReceiveFrom

	                    From {TableNames.YarnReceiveChild} YRC
						INNER JOIN {TableNames.YarnReceiveChild} YRC2 ON YRC2.ChildID = YRC.TagYarnReceiveChildID
						LEFT JOIN {TableNames.YarnReceiveMaster} YRM ON YRM.ReceiveID = YRC.ReceiveID
						LEFT Join {TableNames.YarnQCReqChild} RMC1 On RMC1.ReceiveChildID = YRC2.ChildID
	                    LEFT Join {TableNames.YarnQCReqMaster} RM On RM.QCReqMasterID = RMC1.QCReqMasterID
						LEFT JOIN {TableNames.YarnQCIssueChild} QCIC ON QCIC.QCReqChildID = RMC1.QCReqChildID
		                LEFT JOIN {TableNames.YarnQCIssueMaster} QCIM ON QCIM.QCIssueMasterID = QCIC.QCIssueMasterID
						LEFT JOIN {TableNames.YarnQCReceiveChild} QCRC ON QCRC.QCIssueChildID = QCIC.QCIssueChildID
	                    LEFT Join {TableNames.YarnQCReceiveMaster} RCM On RCM.QCReceiveMasterID = QCRC.QCReceiveMasterID
						LEFT Join {TableNames.YarnQCRemarksChild} C ON C.QCReceiveChildID=QCRC.QCReceiveChildID
						LEFT Join {TableNames.YarnQCRemarksMaster} M On M.QCRemarksMasterID = C.QCRemarksMasterID
						LEFT JOIN {TableNames.YarnPOMaster} YPM ON YPM.YPOMasterID=YRM.POID
                        LEFT JOIN {TableNames.YarnPOChild} YPC ON YPC.YPOChildID=YRC.POChildID
                        LEFT JOIN {TableNames.YarnMRIRChild} YMC ON YMC.ReceiveChildID=YRC.ChildID--YMC.QCRemarksChildID=C.QCRemarksChildID
						LEFT JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID=YPM.CompanyID
	                    Left Join {DbNames.EPYSL}..EntityTypeValue QCReqFor On RM.QCForID = QCReqFor.ValueID
	                    LEFT Join {DbNames.EPYSL}..LoginUser RU On M.QCRemarksBy = RU.UserCode 
					    LEFT Join {DbNames.EPYSL}..Employee ER On ER.EmployeeCode = RU.EmployeeCode
	                    LEFT Join {DbNames.EPYSL}..LoginUser RCU On RCM.QCReceivedBy = RCU.UserCode
					    LEFT Join {DbNames.EPYSL}..Employee ERCU On ERCU.EmployeeCode = RCU.EmployeeCode
	                    LEFT JOIN {TableNames.FabricTechnicalName} T ON T.TechnicalNameId = RMC1.TechnicalNameId
	                    LEFT JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = RMC1.BuyerID
	                    LEFT JOIN YarnAssessmentStatus Z ON Z.YarnStatusID = C.YarnStatusID
	                    LEFT Join {DbNames.EPYSL}..ItemMaster IM On IM.ItemMasterID = YRC.ItemMasterID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
						LEFT JOIN {DbNames.EPYSL}..Contacts CSU ON CSU.ContactID = YRM.SupplierID
						LEFT JOIN ReceiveFrom_HK RFH ON RFH.ReceiveFromID = YRM.ReceiveFromID
                        Where (YPM.CompanyID in(6,8,11) OR YRM.RCompanyID in(6,8,11)) AND--(6=EFL,8=EKL,11=ESL)
						C.Diagnostic=1 AND M.IsApproved = 1 AND
						YMC.MRIRChildID is null
	                    
                    ),
                    FList AS
                    (
	                    SELECT FL.*,Spinner = CASE WHEN FL.SpinnerID > 0 THEN CCS.ShortName ELSE '' END
	                    FROM YQCRemarks FL
	                    LEFT JOIN {DbNames.EPYSL}..Contacts CCS ON CCS.ContactID = FL.SpinnerID
                    )
					,
                    FListTagged AS
                    (
	                    SELECT FL.*,Spinner = CASE WHEN FL.SpinnerID > 0 THEN CCS.ShortName ELSE '' END
	                    FROM YQCTaggedRemarks FL
	                    LEFT JOIN {DbNames.EPYSL}..Contacts CCS ON CCS.ContactID = FL.SpinnerID
                    ),
					FinalList As(
					Select * from FList
					Union All
					Select * from FListTagged
					)
                    SELECT * INTO #TempTable{tempGuid} FROM FinalList
                    SELECT *,Count(*) Over() TotalRows FROM #TempTable{tempGuid}";
            }
            else if (status == Status.Completed)
            {
                sql = $@" With YQCRemarks As 
                    (
	                   Select YMM.MRIRMasterID,YMM.MRNNo,YRM.PONo,sum(YPC.POQty)POQty,YRM.InvoiceNo,YRM.ReceiveNo,YRM.ReceiveDate,sum(YRC2.ReceiveQty)ReceiveQty,sum(YMC.ReceiveQty)ReceiveNoteQty,YRM.ChallanNo,
					   YRM.VehicalNo,CE.ShortName POUnit,CSU.ShortName Supplier,
					   SpinnerID = CASE WHEN YRC2.SpinnerID > 0 THEN YRC2.SpinnerID ELSE YRM.SpinnerID END, RFH.ReceiveFrom


	                    From {TableNames.YarnMRIRChild} YMC
						LEFT JOIN {TableNames.YarnMRIRMaster} YMM ON YMM.MRIRMasterId=YMC.MRIRMasterId
						LEFT JOIN {TableNames.YarnReceiveChild} YRC2 ON YRC2.ChildID = YMC.ReceiveChildID--YMC.QCRemarksChildID=C.QCRemarksChildID
						LEFT JOIN {TableNames.YarnReceiveMaster} YRM ON YRM.ReceiveID = YRC2.ReceiveID
						LEFT JOIN {TableNames.YarnQCReqChild} RMC1 On RMC1.ReceiveChildID = YRC2.ChildID
	                    LEFT JOIN {TableNames.YarnQCReqMaster} RM On RM.QCReqMasterID = RMC1.QCReqMasterID
						LEFT JOIN {TableNames.YarnQCIssueChild} QCIC ON QCIC.QCReqChildID = RMC1.QCReqChildID
		                LEFT JOIN {TableNames.YarnQCIssueMaster} QCIM ON QCIM.QCIssueMasterID = QCIC.QCIssueMasterID
						LEFT JOIN {TableNames.YarnQCReceiveChild} QCRC ON QCRC.QCIssueChildID = QCIC.QCIssueChildID
	                    LEFT JOIN {TableNames.YarnQCReceiveMaster} RCM On RCM.QCReceiveMasterID = QCRC.QCReceiveMasterID
						LEFT JOIN {TableNames.YarnQCRemarksChild} C ON C.QCReceiveChildID=QCRC.QCReceiveChildID
						LEFT JOIN {TableNames.YarnQCRemarksMaster} M On M.QCRemarksMasterID = C.QCRemarksMasterID
						LEFT JOIN {TableNames.YarnPOMaster} YPM ON YPM.YPOMasterID=YRM.POID
                        LEFT JOIN {TableNames.YarnPOChild} YPC ON YPC.YPOChildID=YRC2.POChildID
						LEFT JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID=YPM.CompanyID
	                    LEFT JOIN {DbNames.EPYSL}..Contacts CSU ON CSU.ContactID = YRM.SupplierID
						LEFT JOIN ReceiveFrom_HK RFH ON RFH.ReceiveFromID = YRM.ReceiveFromID
                        Where (YPM.CompanyID in(6,8,11) OR YRM.RCompanyID in(6,8,11)) AND--(6=EFL,8=EKL,11=ESL)
						C.Diagnostic=1 AND M.IsApproved = 1 AND YMM.Returned=0 AND YMM.ReTest=0 AND
						YMC.MRIRChildID is not null
						GROUP BY YMM.MRIRMasterID,YMM.MRNNo,YRM.PONo,YRM.InvoiceNo,YRM.ReceiveNo,YRM.ReceiveDate,YRM.ChallanNo,
					    YRM.VehicalNo,CE.ShortName,CSU.ShortName,YRC2.SpinnerID,YRM.SpinnerID, RFH.ReceiveFrom
                    ),
                    YQCTaggedRemarks As 
                    (
	                   Select YMM.MRIRMasterID,YMM.MRNNo,YRM.PONo,sum(YPC.POQty)POQty,YRM.InvoiceNo,YRM.ReceiveNo,YRM.ReceiveDate,sum(YRC2.ReceiveQty)ReceiveQty,sum(YMC.ReceiveQty)ReceiveNoteQty,YRM.ChallanNo,
					   YRM.VehicalNo,CE.ShortName POUnit,CSU.ShortName Supplier,
					   SpinnerID = CASE WHEN YRC.SpinnerID > 0 THEN YRC.SpinnerID ELSE YRM.SpinnerID END, RFH.ReceiveFrom


	                    From {TableNames.YarnMRIRChild} YMC
						INNER JOIN {TableNames.YarnMRIRMaster} YMM ON YMM.MRIRMasterId=YMC.MRIRMasterId
						INNER JOIN {TableNames.YarnReceiveChild} YRC ON YRC.ChildID = YMC.ReceiveChildID
						INNER JOIN {TableNames.YarnReceiveChild} YRC2 ON YRC2.ChildID = YRC.TagYarnReceiveChildID
						INNER JOIN {TableNames.YarnReceiveMaster} YRM ON YRM.ReceiveID = YRC.ReceiveID
						LEFT JOIN {TableNames.YarnQCReqChild} RMC1 On RMC1.ReceiveChildID = YRC2.ChildID
	                    LEFT JOIN {TableNames.YarnQCReqMaster} RM On RM.QCReqMasterID = RMC1.QCReqMasterID
						LEFT JOIN {TableNames.YarnQCIssueChild} QCIC ON QCIC.QCReqChildID = RMC1.QCReqChildID
		                LEFT JOIN {TableNames.YarnQCIssueMaster} QCIM ON QCIM.QCIssueMasterID = QCIC.QCIssueMasterID
						LEFT JOIN {TableNames.YarnQCReceiveChild} QCRC ON QCRC.QCIssueChildID = QCIC.QCIssueChildID
	                    LEFT JOIN {TableNames.YarnQCReceiveMaster} RCM On RCM.QCReceiveMasterID = QCRC.QCReceiveMasterID
						LEFT JOIN {TableNames.YarnQCRemarksChild} C ON C.QCReceiveChildID=QCRC.QCReceiveChildID
						LEFT JOIN {TableNames.YarnQCRemarksMaster} M On M.QCRemarksMasterID = C.QCRemarksMasterID
						LEFT JOIN {TableNames.YarnPOMaster} YPM ON YPM.YPOMasterID=YRM.POID
                        LEFT JOIN {TableNames.YarnPOChild} YPC ON YPC.YPOChildID=YRC.POChildID
						LEFT JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID=YPM.CompanyID
	                    LEFT JOIN {DbNames.EPYSL}..Contacts CSU ON CSU.ContactID = YRM.SupplierID
						LEFT JOIN ReceiveFrom_HK RFH ON RFH.ReceiveFromID = YRM.ReceiveFromID
                        Where (YPM.CompanyID in(6,8,11) OR YRM.RCompanyID in(6,8,11)) AND--(6=EFL,8=EKL,11=ESL)
						C.Diagnostic=1 AND M.IsApproved = 1 AND YMM.Returned=0 AND YMM.ReTest=0 AND
						YMC.MRIRChildID is not null
						GROUP BY YMM.MRIRMasterID,YMM.MRNNo,YRM.PONo,YRM.InvoiceNo,YRM.ReceiveNo,YRM.ReceiveDate,YRM.ChallanNo,
					    YRM.VehicalNo,CE.ShortName,CSU.ShortName,YRC.SpinnerID,YRM.SpinnerID, RFH.ReceiveFrom
	                    
                    ),
                    FList AS
                    (
	                    SELECT FL.*,Spinner = CASE WHEN FL.SpinnerID > 0 THEN CCS.ShortName ELSE '' END
	                    FROM YQCRemarks FL
	                    LEFT JOIN {DbNames.EPYSL}..Contacts CCS ON CCS.ContactID = FL.SpinnerID
                    )
					,
                    FListTagged AS
                    (
	                    SELECT FL.*,Spinner = CASE WHEN FL.SpinnerID > 0 THEN CCS.ShortName ELSE '' END
	                    FROM YQCTaggedRemarks FL
	                    LEFT JOIN {DbNames.EPYSL}..Contacts CCS ON CCS.ContactID = FL.SpinnerID
                    ),
					FinalList As(
					Select * from FList
					Union All
					Select * from FListTagged
					)
                    SELECT * INTO #TempTable{tempGuid} FROM FinalList
                    SELECT *,Count(*) Over() TotalRows FROM #TempTable{tempGuid}";
            }
            else if (status == Status.Completed2)
            {
                sql = $@" With YQCRemarks As 
                    (
	                   Select YMM.MRIRMasterID,YMM.GRNNo,YRM.PONo,sum(YPC.POQty)POQty,YRM.InvoiceNo,YRM.ReceiveNo,YRM.ReceiveDate,sum(YRC2.ReceiveQty)ReceiveQty,sum(YMC.ReceiveQty)ReceiveNoteQty,YRM.ChallanNo,
					   YRM.VehicalNo,CE.ShortName POUnit,CSU.ShortName Supplier,
					   SpinnerID = CASE WHEN YRC2.SpinnerID > 0 THEN YRC2.SpinnerID ELSE YRM.SpinnerID END, RFH.ReceiveFrom


	                    From {TableNames.YarnMRIRChild} YMC
						LEFT JOIN {TableNames.YarnMRIRMaster} YMM ON YMM.MRIRMasterId=YMC.MRIRMasterId
						LEFT JOIN {TableNames.YarnReceiveChild} YRC2 ON YRC2.ChildID = YMC.ReceiveChildID--YMC.QCRemarksChildID=C.QCRemarksChildID
						LEFT JOIN {TableNames.YarnReceiveMaster} YRM ON YRM.ReceiveID = YRC2.ReceiveID
						LEFT JOIN {TableNames.YarnQCReqChild} RMC1 On RMC1.ReceiveChildID = YRC2.ChildID
	                    LEFT JOIN {TableNames.YarnQCReqMaster} RM On RM.QCReqMasterID = RMC1.QCReqMasterID
						LEFT JOIN {TableNames.YarnQCIssueChild} QCIC ON QCIC.QCReqChildID = RMC1.QCReqChildID
		                LEFT JOIN {TableNames.YarnQCIssueMaster} QCIM ON QCIM.QCIssueMasterID = QCIC.QCIssueMasterID
						LEFT JOIN {TableNames.YarnQCReceiveChild} QCRC ON QCRC.QCIssueChildID = QCIC.QCIssueChildID
	                    LEFT JOIN {TableNames.YarnQCReceiveMaster} RCM On RCM.QCReceiveMasterID = QCRC.QCReceiveMasterID
						LEFT JOIN {TableNames.YarnQCRemarksChild} C ON C.QCReceiveChildID=QCRC.QCReceiveChildID
						LEFT JOIN {TableNames.YarnQCRemarksMaster} M On M.QCRemarksMasterID = C.QCRemarksMasterID
						LEFT JOIN {TableNames.YarnPOMaster} YPM ON YPM.YPOMasterID=YRM.POID
                        LEFT JOIN {TableNames.YarnPOChild} YPC ON YPC.YPOChildID=YRC2.POChildID
						LEFT JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID=YPM.CompanyID
	                    LEFT JOIN {DbNames.EPYSL}..Contacts CSU ON CSU.ContactID = YRM.SupplierID
						LEFT JOIN ReceiveFrom_HK RFH ON RFH.ReceiveFromID = YRM.ReceiveFromID
                         Where (YPM.CompanyID in(11) OR YRM.RCompanyID in(11)) AND--(11=ESL)
						(((C.Approve=1 OR C.CommerciallyApprove=1) AND M.IsApproved = 1) OR YRC2.IsNoTest=1) AND
						YMC.MRIRChildID is not null  AND (YMM.MRIRNo is null OR YMM.MRIRNo='')
						GROUP BY YMM.MRIRMasterID,YMM.GRNNo,YRM.PONo,YRM.InvoiceNo,YRM.ReceiveNo,YRM.ReceiveDate,YRM.ChallanNo,
					    YRM.VehicalNo,CE.ShortName,CSU.ShortName,YRC2.SpinnerID,YRM.SpinnerID, RFH.ReceiveFrom
                    ),
                    YQCTaggedRemarks As 
                    (
	                   Select YMM.MRIRMasterID,YMM.GRNNo,YRM.PONo,sum(YPC.POQty)POQty,YRM.InvoiceNo,YRM.ReceiveNo,YRM.ReceiveDate,sum(YRC2.ReceiveQty)ReceiveQty,sum(YMC.ReceiveQty)ReceiveNoteQty,YRM.ChallanNo,
					   YRM.VehicalNo,CE.ShortName POUnit,CSU.ShortName Supplier,
					   SpinnerID = CASE WHEN YRC.SpinnerID > 0 THEN YRC.SpinnerID ELSE YRM.SpinnerID END, RFH.ReceiveFrom


	                    From {TableNames.YarnMRIRChild} YMC
						INNER JOIN {TableNames.YarnMRIRMaster} YMM ON YMM.MRIRMasterId=YMC.MRIRMasterId
						INNER JOIN {TableNames.YarnReceiveChild} YRC ON YRC.ChildID = YMC.ReceiveChildID
						INNER JOIN {TableNames.YarnReceiveChild} YRC2 ON YRC2.ChildID = YRC.TagYarnReceiveChildID
						INNER JOIN {TableNames.YarnReceiveMaster} YRM ON YRM.ReceiveID = YRC.ReceiveID
						LEFT JOIN {TableNames.YarnQCReqChild} RMC1 On RMC1.ReceiveChildID = YRC2.ChildID
	                    LEFT JOIN {TableNames.YarnQCReqMaster} RM On RM.QCReqMasterID = RMC1.QCReqMasterID
						LEFT JOIN {TableNames.YarnQCIssueChild} QCIC ON QCIC.QCReqChildID = RMC1.QCReqChildID
		                LEFT JOIN {TableNames.YarnQCIssueMaster} QCIM ON QCIM.QCIssueMasterID = QCIC.QCIssueMasterID
						LEFT JOIN {TableNames.YarnQCReceiveChild} QCRC ON QCRC.QCIssueChildID = QCIC.QCIssueChildID
	                    LEFT JOIN {TableNames.YarnQCReceiveMaster} RCM On RCM.QCReceiveMasterID = QCRC.QCReceiveMasterID
						LEFT JOIN {TableNames.YarnQCRemarksChild} C ON C.QCReceiveChildID=QCRC.QCReceiveChildID
						LEFT JOIN {TableNames.YarnQCRemarksMaster} M On M.QCRemarksMasterID = C.QCRemarksMasterID
						LEFT JOIN {TableNames.YarnPOMaster} YPM ON YPM.YPOMasterID=YRM.POID
                        LEFT JOIN {TableNames.YarnPOChild} YPC ON YPC.YPOChildID=YRC.POChildID
						LEFT JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID=YPM.CompanyID
	                    LEFT JOIN {DbNames.EPYSL}..Contacts CSU ON CSU.ContactID = YRM.SupplierID
						LEFT JOIN ReceiveFrom_HK RFH ON RFH.ReceiveFromID = YRM.ReceiveFromID
                        Where (YPM.CompanyID in(11) OR YRM.RCompanyID in(11)) AND--(11=ESL)
						(((C.Approve=1 OR C.CommerciallyApprove=1) AND M.IsApproved = 1) OR YRC2.IsNoTest=1) AND
						YMC.MRIRChildID is not null  AND (YMM.MRIRNo is null OR YMM.MRIRNo='')
						GROUP BY YMM.MRIRMasterID,YMM.GRNNo,YRM.PONo,YRM.InvoiceNo,YRM.ReceiveNo,YRM.ReceiveDate,YRM.ChallanNo,
					    YRM.VehicalNo,CE.ShortName,CSU.ShortName,YRC.SpinnerID,YRM.SpinnerID, RFH.ReceiveFrom
	                    
                    ),
                    FList AS
                    (
	                    SELECT FL.*,Spinner = CASE WHEN FL.SpinnerID > 0 THEN CCS.ShortName ELSE '' END
	                    FROM YQCRemarks FL
	                    LEFT JOIN {DbNames.EPYSL}..Contacts CCS ON CCS.ContactID = FL.SpinnerID
                    )
					,
                    FListTagged AS
                    (
	                    SELECT FL.*,Spinner = CASE WHEN FL.SpinnerID > 0 THEN CCS.ShortName ELSE '' END
	                    FROM YQCTaggedRemarks FL
	                    LEFT JOIN {DbNames.EPYSL}..Contacts CCS ON CCS.ContactID = FL.SpinnerID
                    ),
					FinalList As(
					Select * from FList
					Union All
					Select * from FListTagged
					)
                    SELECT * INTO #TempTable{tempGuid} FROM FinalList
                    SELECT *,Count(*) Over() TotalRows FROM #TempTable{tempGuid}";
            }
            else if (status == Status.Completed3)
            {
                sql = $@"With YQCRemarks As 
                    (
	                   Select YMM.MRIRMasterID,YMM.MRIRNo,YMM.GRNNo,YRM.PONo,sum(YPC.POQty)POQty,YRM.InvoiceNo,YRM.ReceiveNo,YRM.ReceiveDate,sum(YRC2.ReceiveQty)ReceiveQty,sum(YMC.ReceiveQty)ReceiveNoteQty,YRM.ChallanNo,
					   YRM.VehicalNo,CE.ShortName POUnit,CSU.ShortName Supplier,
					   SpinnerID = CASE WHEN YRC2.SpinnerID > 0 THEN YRC2.SpinnerID ELSE YRM.SpinnerID END, RFH.ReceiveFrom


	                    From {TableNames.YarnMRIRChild} YMC
						LEFT JOIN {TableNames.YarnMRIRMaster} YMM ON YMM.MRIRMasterId=YMC.MRIRMasterId
						LEFT JOIN {TableNames.YarnReceiveChild} YRC2 ON YRC2.ChildID = YMC.ReceiveChildID--YMC.QCRemarksChildID=C.QCRemarksChildID
						LEFT JOIN {TableNames.YarnReceiveMaster} YRM ON YRM.ReceiveID = YRC2.ReceiveID
						LEFT JOIN {TableNames.YarnQCReqChild} RMC1 On RMC1.ReceiveChildID = YRC2.ChildID
	                    LEFT JOIN {TableNames.YarnQCReqMaster} RM On RM.QCReqMasterID = RMC1.QCReqMasterID
						LEFT JOIN {TableNames.YarnQCIssueChild} QCIC ON QCIC.QCReqChildID = RMC1.QCReqChildID
		                LEFT JOIN {TableNames.YarnQCIssueMaster} QCIM ON QCIM.QCIssueMasterID = QCIC.QCIssueMasterID
						LEFT JOIN {TableNames.YarnQCReceiveChild} QCRC ON QCRC.QCIssueChildID = QCIC.QCIssueChildID
	                    LEFT JOIN {TableNames.YarnQCReceiveMaster} RCM On RCM.QCReceiveMasterID = QCRC.QCReceiveMasterID
						LEFT JOIN {TableNames.YarnQCRemarksChild} C ON C.QCReceiveChildID=QCRC.QCReceiveChildID
						LEFT JOIN {TableNames.YarnQCRemarksMaster} M On M.QCRemarksMasterID = C.QCRemarksMasterID
						LEFT JOIN {TableNames.YarnPOMaster} YPM ON YPM.YPOMasterID=YRM.POID
                        LEFT JOIN {TableNames.YarnPOChild} YPC ON YPC.YPOChildID=YRC2.POChildID
						LEFT JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID=YPM.CompanyID
	                    LEFT JOIN {DbNames.EPYSL}..Contacts CSU ON CSU.ContactID = YRM.SupplierID
						LEFT JOIN ReceiveFrom_HK RFH ON RFH.ReceiveFromID = YRM.ReceiveFromID
                         Where (YPM.CompanyID in(6,8,11) OR YRM.RCompanyID in(6,8,11)) AND--(6=EFL,8=EKL,11=ESL)
						(((C.Approve=1 OR C.CommerciallyApprove=1) AND M.IsApproved = 1) OR YRC2.IsNoTest=1) AND
						YMC.MRIRChildID is not null AND YMM.MRIRNo is not null AND YMM.MRIRNo<>''
						GROUP BY YMM.MRIRMasterID,YMM.MRIRNo,YMM.GRNNo,YRM.PONo,YRM.InvoiceNo,YRM.ReceiveNo,YRM.ReceiveDate,YRM.ChallanNo,
					    YRM.VehicalNo,CE.ShortName,CSU.ShortName,YRC2.SpinnerID,YRM.SpinnerID, RFH.ReceiveFrom
                    ),
                    YQCTaggedRemarks As 
                    (
	                   Select YMM.MRIRMasterID,YMM.MRIRNo,YMM.GRNNo,YRM.PONo,sum(YPC.POQty)POQty,YRM.InvoiceNo,YRM.ReceiveNo,YRM.ReceiveDate,sum(YRC2.ReceiveQty)ReceiveQty,sum(YMC.ReceiveQty)ReceiveNoteQty,YRM.ChallanNo,
					   YRM.VehicalNo,CE.ShortName POUnit,CSU.ShortName Supplier,
					   SpinnerID = CASE WHEN YRC.SpinnerID > 0 THEN YRC.SpinnerID ELSE YRM.SpinnerID END, RFH.ReceiveFrom


	                    From {TableNames.YarnMRIRChild} YMC
						INNER JOIN {TableNames.YarnMRIRMaster} YMM ON YMM.MRIRMasterId=YMC.MRIRMasterId
						INNER JOIN {TableNames.YarnReceiveChild} YRC ON YRC.ChildID = YMC.ReceiveChildID
						INNER JOIN {TableNames.YarnReceiveChild} YRC2 ON YRC2.ChildID = YRC.TagYarnReceiveChildID
						INNER JOIN {TableNames.YarnReceiveMaster} YRM ON YRM.ReceiveID = YRC.ReceiveID
						LEFT JOIN {TableNames.YarnQCReqChild} RMC1 On RMC1.ReceiveChildID = YRC2.ChildID
	                    LEFT JOIN {TableNames.YarnQCReqMaster} RM On RM.QCReqMasterID = RMC1.QCReqMasterID
						LEFT JOIN {TableNames.YarnQCIssueChild} QCIC ON QCIC.QCReqChildID = RMC1.QCReqChildID
		                LEFT JOIN {TableNames.YarnQCIssueMaster} QCIM ON QCIM.QCIssueMasterID = QCIC.QCIssueMasterID
						LEFT JOIN {TableNames.YarnQCReceiveChild} QCRC ON QCRC.QCIssueChildID = QCIC.QCIssueChildID
	                    LEFT JOIN {TableNames.YarnQCReceiveMaster} RCM On RCM.QCReceiveMasterID = QCRC.QCReceiveMasterID
						LEFT JOIN {TableNames.YarnQCRemarksChild} C ON C.QCReceiveChildID=QCRC.QCReceiveChildID
						LEFT JOIN {TableNames.YarnQCRemarksMaster} M On M.QCRemarksMasterID = C.QCRemarksMasterID
						LEFT JOIN {TableNames.YarnPOMaster} YPM ON YPM.YPOMasterID=YRM.POID
                        LEFT JOIN {TableNames.YarnPOChild} YPC ON YPC.YPOChildID=YRC.POChildID
						LEFT JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID=YPM.CompanyID
	                    LEFT JOIN {DbNames.EPYSL}..Contacts CSU ON CSU.ContactID = YRM.SupplierID
						LEFT JOIN ReceiveFrom_HK RFH ON RFH.ReceiveFromID = YRM.ReceiveFromID
                        Where (YPM.CompanyID in(6,8,11) OR YRM.RCompanyID in(6,8,11)) AND--(6=EFL,8=EKL,11=ESL)
						(((C.Approve=1 OR C.CommerciallyApprove=1) AND M.IsApproved = 1) OR YRC2.IsNoTest=1) AND
						YMC.MRIRChildID is not null AND YMM.MRIRNo is not null AND YMM.MRIRNo<>''
						GROUP BY YMM.MRIRMasterID,YMM.MRIRNo,YMM.GRNNo,YRM.PONo,YRM.InvoiceNo,YRM.ReceiveNo,YRM.ReceiveDate,YRM.ChallanNo,
					    YRM.VehicalNo,CE.ShortName,CSU.ShortName,YRC.SpinnerID,YRM.SpinnerID, RFH.ReceiveFrom
	                    
                    ),
                    FList AS
                    (
	                    SELECT FL.*,Spinner = CASE WHEN FL.SpinnerID > 0 THEN CCS.ShortName ELSE '' END
	                    FROM YQCRemarks FL
	                    LEFT JOIN {DbNames.EPYSL}..Contacts CCS ON CCS.ContactID = FL.SpinnerID
                    )
					,
                    FListTagged AS
                    (
	                    SELECT FL.*,Spinner = CASE WHEN FL.SpinnerID > 0 THEN CCS.ShortName ELSE '' END
	                    FROM YQCTaggedRemarks FL
	                    LEFT JOIN {DbNames.EPYSL}..Contacts CCS ON CCS.ContactID = FL.SpinnerID
                    ),
					FinalList As(
					Select * from FList
					Union All
					Select * from FListTagged
					)
                    SELECT * INTO #TempTable{tempGuid} FROM FinalList
                    SELECT *,Count(*) Over() TotalRows FROM #TempTable{tempGuid}";
            }
            else if (status == Status.All)
            {
                sql = $@"With YQCRemarks As 
                    (
	                   Select YMM.MRIRMasterID,YMM.MRIRNo,YMM.GRNNo,YMM.MRNNo,YRM.PONo,sum(YPC.POQty)POQty,YRM.InvoiceNo,YRM.ReceiveNo,YRM.ReceiveDate,sum(YRC2.ReceiveQty)ReceiveQty,sum(YMC.ReceiveQty)ReceiveNoteQty,YRM.ChallanNo,
					   YRM.VehicalNo,CE.ShortName POUnit,CSU.ShortName Supplier,
					   SpinnerID = CASE WHEN YRC2.SpinnerID > 0 THEN YRC2.SpinnerID ELSE YRM.SpinnerID END, RFH.ReceiveFrom


	                    From {TableNames.YarnReceiveChild} YRC2 
						LEFT JOIN {TableNames.YarnReceiveMaster} YRM ON YRM.ReceiveID = YRC2.ReceiveID
						LEFT JOIN {TableNames.YarnQCReqChild} RMC1 On RMC1.ReceiveChildID = YRC2.ChildID
	                    LEFT JOIN {TableNames.YarnQCReqMaster} RM On RM.QCReqMasterID = RMC1.QCReqMasterID
						LEFT JOIN {TableNames.YarnQCIssueChild} QCIC ON QCIC.QCReqChildID = RMC1.QCReqChildID
		                LEFT JOIN {TableNames.YarnQCIssueMaster} QCIM ON QCIM.QCIssueMasterID = QCIC.QCIssueMasterID
						LEFT JOIN {TableNames.YarnQCReceiveChild} QCRC ON QCRC.QCIssueChildID = QCIC.QCIssueChildID
	                    LEFT JOIN {TableNames.YarnQCReceiveMaster} RCM On RCM.QCReceiveMasterID = QCRC.QCReceiveMasterID
						LEFT JOIN {TableNames.YarnQCRemarksChild} C ON C.QCReceiveChildID=QCRC.QCReceiveChildID
						LEFT JOIN {TableNames.YarnQCRemarksMaster} M On M.QCRemarksMasterID = C.QCRemarksMasterID
						LEFT JOIN {TableNames.YarnPOMaster} YPM ON YPM.YPOMasterID=YRM.POID
                        LEFT JOIN {TableNames.YarnPOChild} YPC ON YPC.YPOChildID=YRC2.POChildID
						LEFT JOIN {TableNames.YarnMRIRChild} YMC ON YMC.ReceiveChildID=YRC2.ChildID--YMC.QCRemarksChildID=C.QCRemarksChildID
						LEFT JOIN {TableNames.YarnMRIRMaster} YMM ON YMM.MRIRMasterId=YMC.MRIRMasterId
						LEFT JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID=YPM.CompanyID
	                    LEFT JOIN {DbNames.EPYSL}..Contacts CSU ON CSU.ContactID = YRM.SupplierID
						LEFT JOIN ReceiveFrom_HK RFH ON RFH.ReceiveFromID = YRM.ReceiveFromID
                         Where YMC.MRIRChildID is not null
						GROUP BY YMM.MRIRMasterID,YMM.MRIRNo,YMM.GRNNo,YMM.MRNNo,YRM.PONo,YRM.InvoiceNo,YRM.ReceiveNo,YRM.ReceiveDate,YRM.ChallanNo,
					    YRM.VehicalNo,CE.ShortName,CSU.ShortName,YRC2.SpinnerID,YRM.SpinnerID, RFH.ReceiveFrom
                    ),
                    FinalList AS
                    (
	                    SELECT FL.*,Spinner = CASE WHEN FL.SpinnerID > 0 THEN CCS.ShortName ELSE '' END
	                    FROM YQCRemarks FL
	                    LEFT JOIN {DbNames.EPYSL}..Contacts CCS ON CCS.ContactID = FL.SpinnerID
                    )
                    SELECT * INTO #TempTable{tempGuid} FROM FinalList
                    SELECT *,Count(*) Over() TotalRows FROM #TempTable{tempGuid}";
            }
            else if (status == Status.PendingConfirmation)
            {
                sql = $@" With YQCRemarks As 
                    (
	                   Select YMM.MRIRMasterID,YMM.GRNNo,YRM.PONo,sum(YPC.POQty)POQty,YRM.InvoiceNo,YRM.ReceiveNo,YRM.ReceiveDate,sum(YRC2.ReceiveQty)ReceiveQty,sum(YMC.ReceiveQty)ReceiveNoteQty,YRM.ChallanNo,
					   YRM.VehicalNo,CE.ShortName POUnit,CSU.ShortName Supplier,
					   SpinnerID = CASE WHEN YRC2.SpinnerID > 0 THEN YRC2.SpinnerID ELSE YRM.SpinnerID END, RFH.ReceiveFrom


	                    From {TableNames.YarnMRIRChild} YMC
						LEFT JOIN {TableNames.YarnMRIRMaster} YMM ON YMM.MRIRMasterId=YMC.MRIRMasterId
						LEFT JOIN {TableNames.YarnReceiveChild} YRC2 ON YRC2.ChildID = YMC.ReceiveChildID
						LEFT JOIN {TableNames.YarnReceiveMaster} YRM ON YRM.ReceiveID = YRC2.ReceiveID
						LEFT JOIN {TableNames.YarnQCReqChild} RMC1 On RMC1.ReceiveChildID = YRC2.ChildID
	                    LEFT JOIN {TableNames.YarnQCReqMaster} RM On RM.QCReqMasterID = RMC1.QCReqMasterID
						LEFT JOIN {TableNames.YarnQCIssueChild} QCIC ON QCIC.QCReqChildID = RMC1.QCReqChildID
		                LEFT JOIN {TableNames.YarnQCIssueMaster} QCIM ON QCIM.QCIssueMasterID = QCIC.QCIssueMasterID
						LEFT JOIN {TableNames.YarnQCReceiveChild} QCRC ON QCRC.QCIssueChildID = QCIC.QCIssueChildID
	                    LEFT JOIN {TableNames.YarnQCReceiveMaster} RCM On RCM.QCReceiveMasterID = QCRC.QCReceiveMasterID
						LEFT JOIN {TableNames.YarnQCRemarksChild} C ON C.QCReceiveChildID=QCRC.QCReceiveChildID
						LEFT JOIN {TableNames.YarnQCRemarksMaster} M On M.QCRemarksMasterID = C.QCRemarksMasterID
						LEFT JOIN {TableNames.YarnPOMaster} YPM ON YPM.YPOMasterID=YRM.POID
                        LEFT JOIN {TableNames.YarnPOChild} YPC ON YPC.YPOChildID=YRC2.POChildID
						LEFT JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID=YPM.CompanyID
	                    LEFT JOIN {DbNames.EPYSL}..Contacts CSU ON CSU.ContactID = YRM.SupplierID
						LEFT JOIN ReceiveFrom_HK RFH ON RFH.ReceiveFromID = YRM.ReceiveFromID
                         Where (YPM.CompanyID in(11) OR YRM.RCompanyID in(11)) AND--(11=ESL)
						(((C.Approve=1 OR C.CommerciallyApprove=1) AND M.IsApproved = 1) OR YRC2.IsNoTest=1) AND
						YMC.MRIRChildID is not null AND (YMM.MRIRNo is null OR YMM.MRIRNo='')
						GROUP BY YMM.MRIRMasterID,YMM.GRNNo,YRM.PONo,YRM.InvoiceNo,YRM.ReceiveNo,YRM.ReceiveDate,YRM.ChallanNo,
					    YRM.VehicalNo,CE.ShortName,CSU.ShortName,YRC2.SpinnerID,YRM.SpinnerID, RFH.ReceiveFrom
                    ),
                    YQCTaggedRemarks As 
                    (
	                   Select YMM.MRIRMasterID,YMM.GRNNo,YRM.PONo,sum(YPC.POQty)POQty,YRM.InvoiceNo,YRM.ReceiveNo,YRM.ReceiveDate,sum(YRC2.ReceiveQty)ReceiveQty,sum(YMC.ReceiveQty)ReceiveNoteQty,YRM.ChallanNo,
					   YRM.VehicalNo,CE.ShortName POUnit,CSU.ShortName Supplier,
					   SpinnerID = CASE WHEN YRC.SpinnerID > 0 THEN YRC.SpinnerID ELSE YRM.SpinnerID END, RFH.ReceiveFrom


	                    From {TableNames.YarnMRIRChild} YMC
						INNER JOIN {TableNames.YarnMRIRMaster} YMM ON YMM.MRIRMasterId=YMC.MRIRMasterId
						INNER JOIN {TableNames.YarnReceiveChild} YRC ON YRC.ChildID = YMC.ReceiveChildID
						INNER JOIN {TableNames.YarnReceiveChild} YRC2 ON YRC2.ChildID = YRC.TagYarnReceiveChildID
						INNER JOIN {TableNames.YarnReceiveMaster} YRM ON YRM.ReceiveID = YRC.ReceiveID
						LEFT JOIN {TableNames.YarnQCReqChild} RMC1 On RMC1.ReceiveChildID = YRC2.ChildID
	                    LEFT JOIN {TableNames.YarnQCReqMaster} RM On RM.QCReqMasterID = RMC1.QCReqMasterID
						LEFT JOIN {TableNames.YarnQCIssueChild} QCIC ON QCIC.QCReqChildID = RMC1.QCReqChildID
		                LEFT JOIN {TableNames.YarnQCIssueMaster} QCIM ON QCIM.QCIssueMasterID = QCIC.QCIssueMasterID
						LEFT JOIN {TableNames.YarnQCReceiveChild} QCRC ON QCRC.QCIssueChildID = QCIC.QCIssueChildID
	                    LEFT JOIN {TableNames.YarnQCReceiveMaster} RCM On RCM.QCReceiveMasterID = QCRC.QCReceiveMasterID
						LEFT JOIN {TableNames.YarnQCRemarksChild} C ON C.QCReceiveChildID=QCRC.QCReceiveChildID
						LEFT JOIN {TableNames.YarnQCRemarksMaster} M On M.QCRemarksMasterID = C.QCRemarksMasterID
						LEFT JOIN {TableNames.YarnPOMaster} YPM ON YPM.YPOMasterID=YRM.POID
                        LEFT JOIN {TableNames.YarnPOChild} YPC ON YPC.YPOChildID=YRC.POChildID
						LEFT JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID=YPM.CompanyID
	                    LEFT JOIN {DbNames.EPYSL}..Contacts CSU ON CSU.ContactID = YRM.SupplierID
						LEFT JOIN ReceiveFrom_HK RFH ON RFH.ReceiveFromID = YRM.ReceiveFromID
                        Where (YPM.CompanyID in(11) OR YRM.RCompanyID in(11)) AND--(11=ESL)
						(((C.Approve=1 OR C.CommerciallyApprove=1) AND M.IsApproved = 1) OR YRC2.IsNoTest=1) AND
						YMC.MRIRChildID is not null AND (YMM.MRIRNo is null OR YMM.MRIRNo='')
						GROUP BY YMM.MRIRMasterID,YMM.GRNNo,YRM.PONo,YRM.InvoiceNo,YRM.ReceiveNo,YRM.ReceiveDate,YRM.ChallanNo,
					    YRM.VehicalNo,CE.ShortName,CSU.ShortName,YRC.SpinnerID,YRM.SpinnerID, RFH.ReceiveFrom
	                    
                    ),
                    FList AS
                    (
	                    SELECT FL.*,Spinner = CASE WHEN FL.SpinnerID > 0 THEN CCS.ShortName ELSE '' END
	                    FROM YQCRemarks FL
	                    LEFT JOIN {DbNames.EPYSL}..Contacts CCS ON CCS.ContactID = FL.SpinnerID
                    )
					,
                    FListTagged AS
                    (
	                    SELECT FL.*,Spinner = CASE WHEN FL.SpinnerID > 0 THEN CCS.ShortName ELSE '' END
	                    FROM YQCTaggedRemarks FL
	                    LEFT JOIN {DbNames.EPYSL}..Contacts CCS ON CCS.ContactID = FL.SpinnerID
                    ),
					FinalList As(
					Select * from FList
					Union All
					Select * from FListTagged
					)
                    SELECT * INTO #TempTable{tempGuid} FROM FinalList
                    SELECT *,Count(*) Over() TotalRows FROM #TempTable{tempGuid}";
            }
            else if (status == Status.YDPComplete)
            {
                sql = $@" With YQCRemarks As 
                    (
	                   Select YMM.MRIRMasterID,YMM.GRNNo,YMM.MRIRNo,YRM.PONo,sum(YPC.POQty)POQty,YRM.InvoiceNo,YRM.ReceiveNo,YRM.ReceiveDate,sum(YRC2.ReceiveQty)ReceiveQty,sum(YMC.ReceiveQty)ReceiveNoteQty,YRM.ChallanNo,
					   YRM.VehicalNo,CE.ShortName POUnit,CSU.ShortName Supplier,
					   SpinnerID = CASE WHEN YRC2.SpinnerID > 0 THEN YRC2.SpinnerID ELSE YRM.SpinnerID END, RFH.ReceiveFrom


	                    From {TableNames.YarnMRIRChild} YMC
						LEFT JOIN {TableNames.YarnMRIRMaster} YMM ON YMM.MRIRMasterId=YMC.MRIRMasterId
						LEFT JOIN {TableNames.YarnReceiveChild} YRC2 ON YRC2.ChildID = YMC.ReceiveChildID
						LEFT JOIN {TableNames.YarnReceiveMaster} YRM ON YRM.ReceiveID = YRC2.ReceiveID
						LEFT JOIN {TableNames.YarnQCReqChild} RMC1 On RMC1.ReceiveChildID = YRC2.ChildID
	                    LEFT JOIN {TableNames.YarnQCReqMaster} RM On RM.QCReqMasterID = RMC1.QCReqMasterID
						LEFT JOIN {TableNames.YarnQCIssueChild} QCIC ON QCIC.QCReqChildID = RMC1.QCReqChildID
		                LEFT JOIN {TableNames.YarnQCIssueMaster} QCIM ON QCIM.QCIssueMasterID = QCIC.QCIssueMasterID
						LEFT JOIN {TableNames.YarnQCReceiveChild} QCRC ON QCRC.QCIssueChildID = QCIC.QCIssueChildID
	                    LEFT JOIN {TableNames.YarnQCReceiveMaster} RCM On RCM.QCReceiveMasterID = QCRC.QCReceiveMasterID
						LEFT JOIN {TableNames.YarnQCRemarksChild} C ON C.QCReceiveChildID=QCRC.QCReceiveChildID
						LEFT JOIN {TableNames.YarnQCRemarksMaster} M On M.QCRemarksMasterID = C.QCRemarksMasterID
						LEFT JOIN {TableNames.YarnPOMaster} YPM ON YPM.YPOMasterID=YRM.POID
                        LEFT JOIN {TableNames.YarnPOChild} YPC ON YPC.YPOChildID=YRC2.POChildID
						LEFT JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID=YPM.CompanyID
	                    LEFT JOIN {DbNames.EPYSL}..Contacts CSU ON CSU.ContactID = YRM.SupplierID
						LEFT JOIN ReceiveFrom_HK RFH ON RFH.ReceiveFromID = YRM.ReceiveFromID
                         Where (YPM.CompanyID in(11) OR YRM.RCompanyID in(11)) AND--(11=ESL)
						(((C.Approve=1 OR C.CommerciallyApprove=1) AND M.IsApproved = 1) OR YRC2.IsNoTest=1) AND
						YMC.MRIRChildID is not null AND YMM.MRIRNo is not null AND YMM.MRIRNo<>''
						GROUP BY YMM.MRIRMasterID,YMM.GRNNo,YMM.MRIRNo,YRM.PONo,YRM.InvoiceNo,YRM.ReceiveNo,YRM.ReceiveDate,YRM.ChallanNo,
					    YRM.VehicalNo,CE.ShortName,CSU.ShortName,YRC2.SpinnerID,YRM.SpinnerID, RFH.ReceiveFrom
                    ),
                    YQCTaggedRemarks As 
                    (
	                   Select YMM.MRIRMasterID,YMM.GRNNo,YMM.MRIRNo,YRM.PONo,sum(YPC.POQty)POQty,YRM.InvoiceNo,YRM.ReceiveNo,YRM.ReceiveDate,sum(YRC2.ReceiveQty)ReceiveQty,sum(YMC.ReceiveQty)ReceiveNoteQty,YRM.ChallanNo,
					   YRM.VehicalNo,CE.ShortName POUnit,CSU.ShortName Supplier,
					   SpinnerID = CASE WHEN YRC.SpinnerID > 0 THEN YRC.SpinnerID ELSE YRM.SpinnerID END, RFH.ReceiveFrom


	                    From {TableNames.YarnMRIRChild} YMC
						INNER JOIN {TableNames.YarnMRIRMaster} YMM ON YMM.MRIRMasterId=YMC.MRIRMasterId
						INNER JOIN {TableNames.YarnReceiveChild} YRC ON YRC.ChildID = YMC.ReceiveChildID
						INNER JOIN {TableNames.YarnReceiveChild} YRC2 ON YRC2.ChildID = YRC.TagYarnReceiveChildID
						INNER JOIN {TableNames.YarnReceiveMaster} YRM ON YRM.ReceiveID = YRC.ReceiveID
						LEFT JOIN {TableNames.YarnQCReqChild} RMC1 On RMC1.ReceiveChildID = YRC2.ChildID
	                    LEFT JOIN {TableNames.YarnQCReqMaster} RM On RM.QCReqMasterID = RMC1.QCReqMasterID
						LEFT JOIN {TableNames.YarnQCIssueChild} QCIC ON QCIC.QCReqChildID = RMC1.QCReqChildID
		                LEFT JOIN {TableNames.YarnQCIssueMaster} QCIM ON QCIM.QCIssueMasterID = QCIC.QCIssueMasterID
						LEFT JOIN {TableNames.YarnQCReceiveChild} QCRC ON QCRC.QCIssueChildID = QCIC.QCIssueChildID
	                    LEFT JOIN {TableNames.YarnQCReceiveMaster} RCM On RCM.QCReceiveMasterID = QCRC.QCReceiveMasterID
						LEFT JOIN {TableNames.YarnQCRemarksChild} C ON C.QCReceiveChildID=QCRC.QCReceiveChildID
						LEFT JOIN {TableNames.YarnQCRemarksMaster} M On M.QCRemarksMasterID = C.QCRemarksMasterID
						LEFT JOIN {TableNames.YarnPOMaster} YPM ON YPM.YPOMasterID=YRM.POID
                        LEFT JOIN {TableNames.YarnPOChild} YPC ON YPC.YPOChildID=YRC.POChildID
						LEFT JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID=YPM.CompanyID
	                    LEFT JOIN {DbNames.EPYSL}..Contacts CSU ON CSU.ContactID = YRM.SupplierID
						LEFT JOIN ReceiveFrom_HK RFH ON RFH.ReceiveFromID = YRM.ReceiveFromID
                        Where (YPM.CompanyID in(11) OR YRM.RCompanyID in(11)) AND--(11=ESL)
						(((C.Approve=1 OR C.CommerciallyApprove=1) AND M.IsApproved = 1) OR YRC2.IsNoTest=1) AND
						YMC.MRIRChildID is not null AND YMM.MRIRNo is not null AND YMM.MRIRNo<>''
						GROUP BY YMM.MRIRMasterID,YMM.GRNNo,YMM.MRIRNo,YRM.PONo,YRM.InvoiceNo,YRM.ReceiveNo,YRM.ReceiveDate,YRM.ChallanNo,
					    YRM.VehicalNo,CE.ShortName,CSU.ShortName,YRC.SpinnerID,YRM.SpinnerID, RFH.ReceiveFrom
	                    
                    ),
                    FList AS
                    (
	                    SELECT FL.*,Spinner = CASE WHEN FL.SpinnerID > 0 THEN CCS.ShortName ELSE '' END
	                    FROM YQCRemarks FL
	                    LEFT JOIN {DbNames.EPYSL}..Contacts CCS ON CCS.ContactID = FL.SpinnerID
                    )
					,
                    FListTagged AS
                    (
	                    SELECT FL.*,Spinner = CASE WHEN FL.SpinnerID > 0 THEN CCS.ShortName ELSE '' END
	                    FROM YQCTaggedRemarks FL
	                    LEFT JOIN {DbNames.EPYSL}..Contacts CCS ON CCS.ContactID = FL.SpinnerID
                    ),
					FinalList As(
					Select * from FList
					Union All
					Select * from FListTagged
					)
                    SELECT * INTO #TempTable{tempGuid} FROM FinalList
                    SELECT *,Count(*) Over() TotalRows FROM #TempTable{tempGuid}";
            }
            else if (status == Status.Return)
            {
                sql = $@" With YQCRemarks As 
                    (
	                   Select YMM.MRIRMasterID,YMM.MRNNo,YRM.PONo,sum(YPC.POQty)POQty,YRM.InvoiceNo,YRM.ReceiveNo,YRM.ReceiveDate,sum(YRC2.ReceiveQty)ReceiveQty,sum(YMC.ReceiveQty)ReceiveNoteQty,YRM.ChallanNo,
					   YRM.VehicalNo,CE.ShortName POUnit,CSU.ShortName Supplier,
					   SpinnerID = CASE WHEN YRC2.SpinnerID > 0 THEN YRC2.SpinnerID ELSE YRM.SpinnerID END


	                    From {TableNames.YarnReceiveChild} YRC2 
						LEFT JOIN {TableNames.YarnReceiveMaster} YRM ON YRM.ReceiveID = YRC2.ReceiveID
						LEFT JOIN {TableNames.YarnQCReqChild} RMC1 On RMC1.ReceiveChildID = YRC2.ChildID
	                    LEFT JOIN {TableNames.YarnQCReqMaster} RM On RM.QCReqMasterID = RMC1.QCReqMasterID
						LEFT JOIN {TableNames.YarnQCIssueChild} QCIC ON QCIC.QCReqChildID = RMC1.QCReqChildID
		                LEFT JOIN {TableNames.YarnQCIssueMaster} QCIM ON QCIM.QCIssueMasterID = QCIC.QCIssueMasterID
						LEFT JOIN {TableNames.YarnQCReceiveChild} QCRC ON QCRC.QCIssueChildID = QCIC.QCIssueChildID
	                    LEFT JOIN {TableNames.YarnQCReceiveMaster} RCM On RCM.QCReceiveMasterID = QCRC.QCReceiveMasterID
						LEFT JOIN {TableNames.YarnQCRemarksChild} C ON C.QCReceiveChildID=QCRC.QCReceiveChildID
						LEFT JOIN {TableNames.YarnQCRemarksMaster} M On M.QCRemarksMasterID = C.QCRemarksMasterID
						LEFT JOIN {TableNames.YarnPOMaster} YPM ON YPM.YPOMasterID=YRM.POID
                        LEFT JOIN {TableNames.YarnPOChild} YPC ON YPC.YPOChildID=YRC2.POChildID
						LEFT JOIN {TableNames.YarnMRIRChild} YMC ON YMC.ReceiveChildID=YRC2.ChildID--YMC.QCRemarksChildID=C.QCRemarksChildID
						LEFT JOIN {TableNames.YarnMRIRMaster} YMM ON YMM.MRIRMasterId=YMC.MRIRMasterId
						LEFT JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID=YPM.CompanyID
	                    LEFT JOIN {DbNames.EPYSL}..Contacts CSU ON CSU.ContactID = YRM.SupplierID
                        Where (YPM.CompanyID in(6,8,11) OR YRM.RCompanyID in(6,8,11)) AND--(6=EFL,8=EKL,11=ESL)
						C.Diagnostic=1 AND M.IsApproved = 1 AND
						YMC.MRIRChildID is not null AND YMM.Returned=1 AND YMM.Retest=0
						GROUP BY YMM.MRIRMasterID,YMM.MRNNo,YRM.PONo,YRM.InvoiceNo,YRM.ReceiveNo,YRM.ReceiveDate,YRM.ChallanNo,
					    YRM.VehicalNo,CE.ShortName,CSU.ShortName,YRC2.SpinnerID,YRM.SpinnerID
                    ),
                    FinalList AS
                    (
	                    SELECT FL.*,Spinner = CASE WHEN FL.SpinnerID > 0 THEN CCS.ShortName ELSE '' END
	                    FROM YQCRemarks FL
	                    LEFT JOIN {DbNames.EPYSL}..Contacts CCS ON CCS.ContactID = FL.SpinnerID
                    )
                    SELECT * INTO #TempTable{tempGuid} FROM FinalList
                    SELECT *,Count(*) Over() TotalRows FROM #TempTable{tempGuid}";
            }
            else if (status == Status.ReTest)
            {
                sql = $@" With YQCRemarks As 
                    (
	                   Select YMM.MRIRMasterID,YMM.MRNNo,YRM.PONo,sum(YPC.POQty)POQty,YRM.InvoiceNo,YRM.ReceiveNo,YRM.ReceiveDate,sum(YRC2.ReceiveQty)ReceiveQty,sum(YMC.ReceiveQty)ReceiveNoteQty,YRM.ChallanNo,
					   YRM.VehicalNo,CE.ShortName POUnit,CSU.ShortName Supplier,
					   SpinnerID = CASE WHEN YRC2.SpinnerID > 0 THEN YRC2.SpinnerID ELSE YRM.SpinnerID END


	                    From {TableNames.YarnReceiveChild} YRC2 
						LEFT JOIN {TableNames.YarnReceiveMaster} YRM ON YRM.ReceiveID = YRC2.ReceiveID
						LEFT JOIN {TableNames.YarnQCReqChild} RMC1 On RMC1.ReceiveChildID = YRC2.ChildID
	                    LEFT JOIN {TableNames.YarnQCReqMaster} RM On RM.QCReqMasterID = RMC1.QCReqMasterID
						LEFT JOIN {TableNames.YarnQCIssueChild} QCIC ON QCIC.QCReqChildID = RMC1.QCReqChildID
		                LEFT JOIN {TableNames.YarnQCIssueMaster} QCIM ON QCIM.QCIssueMasterID = QCIC.QCIssueMasterID
						LEFT JOIN {TableNames.YarnQCReceiveChild} QCRC ON QCRC.QCIssueChildID = QCIC.QCIssueChildID
	                    LEFT JOIN {TableNames.YarnQCReceiveMaster} RCM On RCM.QCReceiveMasterID = QCRC.QCReceiveMasterID
						LEFT JOIN {TableNames.YarnQCRemarksChild} C ON C.QCReceiveChildID=QCRC.QCReceiveChildID
						LEFT JOIN {TableNames.YarnQCRemarksMaster} M On M.QCRemarksMasterID = C.QCRemarksMasterID
						LEFT JOIN {TableNames.YarnPOMaster} YPM ON YPM.YPOMasterID=YRM.POID
                        LEFT JOIN {TableNames.YarnPOChild} YPC ON YPC.YPOChildID=YRC2.POChildID
						LEFT JOIN {TableNames.YarnMRIRChild} YMC ON YMC.ReceiveChildID=YRC2.ChildID--YMC.QCRemarksChildID=C.QCRemarksChildID
						LEFT JOIN {TableNames.YarnMRIRMaster} YMM ON YMM.MRIRMasterId=YMC.MRIRMasterId
						LEFT JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID=YPM.CompanyID
	                    LEFT JOIN {DbNames.EPYSL}..Contacts CSU ON CSU.ContactID = YRM.SupplierID
                        Where (YPM.CompanyID in(6,8,11) OR YRM.RCompanyID in(6,8,11)) AND--(6=EFL,8=EKL,11=ESL)
						C.Diagnostic=1 AND M.IsApproved = 1 AND
						YMC.MRIRChildID is not null AND YMM.Returned=0 AND YMM.Retest=1
						GROUP BY YMM.MRIRMasterID,YMM.MRNNo,YRM.PONo,YRM.InvoiceNo,YRM.ReceiveNo,YRM.ReceiveDate,YRM.ChallanNo,
					    YRM.VehicalNo,CE.ShortName,CSU.ShortName,YRC2.SpinnerID,YRM.SpinnerID
                    ),
                    FinalList AS
                    (
	                    SELECT FL.*,Spinner = CASE WHEN FL.SpinnerID > 0 THEN CCS.ShortName ELSE '' END
	                    FROM YQCRemarks FL
	                    LEFT JOIN {DbNames.EPYSL}..Contacts CCS ON CCS.ContactID = FL.SpinnerID
                    )
                    SELECT * INTO #TempTable{tempGuid} FROM FinalList
                    SELECT *,Count(*) Over() TotalRows FROM #TempTable{tempGuid}";
            }
            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            sql += $@" DROP TABLE #TempTable{tempGuid} ";

            var data = await _service.GetDataAsync<YarnMRIRChild>(sql);
            return data;
        }
        public async Task<YarnMRIRMaster> GetMRIRDetailsAsync(int MRIRMasterID)
        {
            string condition = "";

            var query =
                $@"-- Master Data
					Select YMM.MRIRMasterID,YMM.MRIRNo,YMM.GRNNo,YMM.MRNNo,YRM.PONo,YRM.InvoiceNo,YRM.ReceiveNo,YRM.ReceiveDate,sum(YMC.ReceiveQty)ReceiveQty,sum(YMC.ReceiveQty)ReceiveNoteQty,YRM.ChallanNo,
					YRM.VehicalNo,CE.ShortName POUnit,CSU.ShortName Supplier,
					SpinnerID = CASE WHEN YRC2.SpinnerID > 0 THEN YRC2.SpinnerID ELSE YRM.SpinnerID END
					From {TableNames.YarnReceiveChild} YRC2 
					LEFT JOIN {TableNames.YarnReceiveMaster} YRM ON YRM.ReceiveID = YRC2.ReceiveID
					LEFT JOIN {TableNames.YarnQCReqChild} RMC1 On RMC1.ReceiveChildID = YRC2.ChildID
					LEFT JOIN {TableNames.YarnQCReqMaster} RM On RM.QCReqMasterID = RMC1.QCReqMasterID
					LEFT JOIN {TableNames.YarnQCIssueChild} QCIC ON QCIC.QCReqChildID = RMC1.QCReqChildID
					LEFT JOIN {TableNames.YarnQCIssueMaster} QCIM ON QCIM.QCIssueMasterID = QCIC.QCIssueMasterID
					LEFT JOIN {TableNames.YarnQCReceiveChild} QCRC ON QCRC.QCIssueChildID = QCIC.QCIssueChildID
					LEFT JOIN {TableNames.YarnQCReceiveMaster} RCM On RCM.QCReceiveMasterID = QCRC.QCReceiveMasterID
					LEFT JOIN {TableNames.YarnQCRemarksChild} C ON C.QCReceiveChildID=QCRC.QCReceiveChildID
					LEFT JOIN {TableNames.YarnQCRemarksMaster} M On M.QCRemarksMasterID = C.QCRemarksMasterID
					LEFT JOIN {TableNames.YarnPOMaster} YPM ON YPM.YPOMasterID=YRM.POID
					LEFT JOIN {TableNames.YarnMRIRChild} YMC ON YMC.ReceiveChildID=YRC2.ChildID
					LEFT JOIN {TableNames.YarnMRIRMaster} YMM ON YMM.MRIRMasterId=YMC.MRIRMasterId
					LEFT JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID=YPM.CompanyID
					LEFT JOIN {DbNames.EPYSL}..Contacts CSU ON CSU.ContactID = YRM.SupplierID
					Where YMM.MRIRMasterId={MRIRMasterID}
					GROUP BY YMM.MRIRMasterID,YMM.MRIRNo,YMM.GRNNo,YMM.MRNNo,YRM.PONo,YRM.InvoiceNo,YRM.ReceiveNo,YRM.ReceiveDate,YRM.ChallanNo,
					YRM.VehicalNo,CE.ShortName,CSU.ShortName,YRC2.SpinnerID,YRM.SpinnerID;

					--Child Data
					With YQCRemarks As 
					(
					Select C.QCRemarksChildID, M.QCRemarksMasterID, M.QCRemarksNo, M.QCRemarksDate, ER.EmployeeName QCRemarksByUser
					, M.QCReceiveMasterID, M.QCIssueMasterID, M.QCReqMasterID, ERCU.EmployeeName QCReceiveByUser, QCReqFor.ValueName QCReqFor
					, YRC.ReceiveQty, C.ReceiveQtyCone, C.ReceiveQtyCarton,C.ShadeCode, RM.QCReqNo, RCM.QCReceiveNo, QCIM.QCIssueNo,
					C.Remarks, 
					YarnStatus = Z.YarnStatus,
					[Status] = CASE WHEN C.Approve = 1 THEN 'Approve'
					WHEN C.Reject = 1 THEN 'Reject'
					WHEN C.Retest = 1 THEN 'Retest'
					WHEN C.Diagnostic = 1 THEN 'Diagnostic'
					WHEN C.CommerciallyApprove = 1 THEN 'CommerciallyApprove' 
					ELSE '' END,
					TestType=CASE WHEN ((C.Approve=1 OR C.CommerciallyApprove=1 OR C.Diagnostic=1) AND M.IsApproved = 1) THEN 'Tested'
					WHEN YRC2.IsNoTest=1 THEN 'No Test Required' 
					WHEN C.ReTest=1 OR M.IsRetest=1 OR YMC.ReTest=1 OR YMM.ReTest=1 THEN 'Re-Test' End,
					YRM.ReceiveNo, YRM.ReceiveDate,
					SpinnerID = CASE WHEN YRC.SpinnerID > 0 THEN YRC.SpinnerID ELSE YRM.SpinnerID END,
					YRC.LotNo, YRC.ChallanLot,
					BuyerName = CASE WHEN RMC1.BuyerID > 0 THEN B.ShortName ELSE '' END,
					YRM.PONo,YRM.InvoiceNo,YRM.ChallanNo,C.POCount,C.PhysicalCount,YRC.YarnControlNo,
					RM.LocationId RackLocationId,YRC.ChildID ReceiveChildID,YRM.VehicalNo,CE.ShortName POUnit,CSU.ShortName Supplier,
					YPM.CompanyID,YPM.CompanyID UnitId,YRM.RCompanyID,YRM.SupplierID,YRC.ItemMasterId,YRC.Rate,YRC.YarnProgramId,
					YRC.ChallanCount,YRC.YarnCategory,RMC1.NoOfThread,YMC.MRIRChildID,YMC.MRIRMasterID,YMC.ReceiveQty ReceiveNoteQty

					From {TableNames.YarnMRIRChild} YMC
					LEFT JOIN {TableNames.YarnMRIRMaster} YMM ON YMM.MRIRMasterId=YMC.MRIRMasterId
					LEFT JOIN {TableNames.YarnReceiveChild} YRC ON YRC.ChildID = YMC.ReceiveChildID
					LEFT JOIN {TableNames.YarnReceiveChild} YRC2 ON YRC2.ChildID = CASE WHEN YRC.TagYarnReceiveChildID>0 THEN YRC.TagYarnReceiveChildID ELSE YMC.ReceiveChildID END
					LEFT JOIN {TableNames.YarnReceiveMaster} YRM ON YRM.ReceiveID = YRC.ReceiveID
					LEFT JOIN {TableNames.YarnQCReqChild} RMC1 On RMC1.ReceiveChildID = YRC2.ChildID
					LEFT JOIN {TableNames.YarnQCReqMaster} RM On RM.QCReqMasterID = RMC1.QCReqMasterID
					LEFT JOIN {TableNames.YarnQCIssueChild} QCIC ON QCIC.QCReqChildID = RMC1.QCReqChildID
					LEFT JOIN {TableNames.YarnQCIssueMaster} QCIM ON QCIM.QCIssueMasterID = QCIC.QCIssueMasterID
					LEFT JOIN {TableNames.YarnQCReceiveChild} QCRC ON QCRC.QCIssueChildID = QCIC.QCIssueChildID
					LEFT JOIN {TableNames.YarnQCReceiveMaster} RCM On RCM.QCReceiveMasterID = QCRC.QCReceiveMasterID
					LEFT JOIN {TableNames.YarnQCRemarksChild} C ON C.QCReceiveChildID=QCRC.QCReceiveChildID
					LEFT JOIN {TableNames.YarnQCRemarksMaster} M On M.QCRemarksMasterID = C.QCRemarksMasterID
					LEFT JOIN {TableNames.YarnPOMaster} YPM ON YPM.YPOMasterID=YRM.POID
					LEFT JOIN {DbNames.EPYSL}..ItemMaster IM On IM.ItemMasterID = YRC.ItemMasterID
					LEFT JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID=YPM.CompanyID
					LEFT JOIN {DbNames.EPYSL}..EntityTypeValue QCReqFor On RM.QCForID = QCReqFor.ValueID
					LEFT JOIN {DbNames.EPYSL}..LoginUser RU On M.QCRemarksBy = RU.UserCode 
					LEFT JOIN {DbNames.EPYSL}..Employee ER On ER.EmployeeCode = RU.EmployeeCode
					LEFT JOIN {DbNames.EPYSL}..LoginUser RCU On RCM.QCReceivedBy = RCU.UserCode
					LEFT JOIN {DbNames.EPYSL}..Employee ERCU On ERCU.EmployeeCode = RCU.EmployeeCode
					LEFT JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = RMC1.BuyerID
					LEFT JOIN YarnAssessmentStatus Z ON Z.YarnStatusID = C.YarnStatusID
					LEFT JOIN {DbNames.EPYSL}..Contacts CSU ON CSU.ContactID = YRM.SupplierID
					Where  YMM.MRIRMasterId={MRIRMasterID}
					)
					SELECT TOP(1) FL.*,Spinner = CASE WHEN FL.SpinnerID > 0 THEN CCS.ShortName ELSE '' END
					FROM YQCRemarks FL
					LEFT JOIN {DbNames.EPYSL}..Contacts CCS ON CCS.ContactID = FL.SpinnerID
					ORDER BY FL.QCRemarksChildID DESC";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                YarnMRIRMaster data = await records.ReadFirstOrDefaultAsync<YarnMRIRMaster>();
                Guard.Against.NullObject(data);

                data.YarnMRIRChilds = records.Read<YarnMRIRChild>().ToList();

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
        public async Task<YarnMRIRMaster> GetNewAsync(int qcRemarksMasterId)
        {
            var query =
                $@"
                -- Master Data
                With 
                M As (
	                Select * From {TableNames.YarnQCRemarksMaster}
	                Where QCRemarksMasterID = {qcRemarksMasterId}
                )

                Select M.QCRemarksMasterID, M.QCRemarksNo, M.QCRemarksDate, M.QCReqMasterID, M.QCIssueMasterID, M.LocationId, M.ReceiveID, M.CompanyId, M.RCompanyId, M.SupplierId, M.SpinnerId
                 , QCReqFor.ValueName QCReqFor, RM.QCReqDate, RM.QCReqNo
                From M
                Inner Join {TableNames.YarnQCReqMaster} RM On M.QCReqMasterID = RM.QCReqMasterID
                Inner Join {DbNames.EPYSL}..EntityTypeValue QCReqFor On RM.QCForID = QCReqFor.ValueID;

                -- Child Data
                 With
                C As (
	                Select * 
	                From {TableNames.YarnQCRemarksChild}
	                Where QCRemarksMasterID = {qcRemarksMasterId}
                )

                Select C.YarnProgramId, C.ChallanCount, C.POCount, C.PhysicalCount, C.YarnCategory, C.NoOfThread, C.UnitID, C.ItemMasterID
	                , YarnType.SegmentValue [YarnType], YarnCount.SegmentValue [YarnCount], YarnComposition.SegmentValue [YarnComposition]
	                , Shade.SegmentValue [Shade], YarnColor.SegmentValue [YarnColor], IM.ItemMasterID, C.LotNo
	                , C.ReceiveQty, C.ReceiveQtyCone, C.ReceiveQtyCarton ReceiveQtyCarton, U.DisplayUnitDesc UOM, C.Remarks, ISNULL(C.YarnCategory, '') YarnCategory
                From C
                Inner Join {DbNames.EPYSL}..Unit U On C.UnitID = U.UnitID
                Inner Join {DbNames.EPYSL}..ItemMaster IM On C.ItemMasterID = IM.ItemMasterID
                Left Join {DbNames.EPYSL}..ItemSegmentValue YarnType On IM.Segment1ValueID = YarnType.SegmentValueID
                Left Join {DbNames.EPYSL}..ItemSegmentValue YarnCount On IM.Segment2ValueID = YarnCount.SegmentValueID
                Left Join {DbNames.EPYSL}..ItemSegmentValue YarnComposition On IM.Segment3ValueID = YarnComposition.SegmentValueID
                Left Join {DbNames.EPYSL}..ItemSegmentValue Shade On IM.Segment4ValueID = Shade.SegmentValueID
                Left Join {DbNames.EPYSL}..ItemSegmentValue YarnColor On IM.Segment5ValueID = YarnColor.SegmentValueID";

            //var connection = _dbContext.Database.Connection;

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);

                YarnMRIRMaster data = await records.ReadFirstOrDefaultAsync<YarnMRIRMaster>();
                data.Childs = records.Read<YarnMRIRChild>().ToList();

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
        public async Task<YarnMRIRMaster> GetAsync(int id)
        {
            var query =
                $@"Select * From {TableNames.YarnMRIRMaster} 
	                Where MRIRMasterID = {id}";

            //var connection = _dbContext.Database.Connection;

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);

                YarnMRIRMaster data = await records.ReadFirstOrDefaultAsync<YarnMRIRMaster>();

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
        public async Task<List<YarnReceiveChild>> GetByYarnReceiveChildByChildIds(string reciveChildIds)
        {
            var sql = $@"
                    SELECT YRC.* 
                    FROM {TableNames.YarnReceiveChild} YRC
                    WHERE YRC.ChildId IN ({reciveChildIds})";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                List<YarnReceiveChild> yarnReceiveChilds = records.Read<YarnReceiveChild>().ToList();
                Guard.Against.NullObject(yarnReceiveChilds);
                return yarnReceiveChilds;
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

        /*public async Task<YarnMRIRMaster> GetAsync(int id)
        {
            var query =
                $@"
                With 
                M AS (
	                Select * From YarnMRIRMaster 
	                Where MRIRMasterID = {id} 
                )

                Select M.MRIRMasterID, M.MRIRNo, M.MRIRDate, M.QCRemarksMasterID, LU.Name MRIRBYUser, RM.QCRemarksNo, RM.QCRemarksDate	                
	                , QCReqFor.ValueName [QCReqFor], M.LocationId, M.ReceiveID, M.CompanyId, M.RCompanyId, M.SupplierId, M.SpinnerId, Supplier.ShortName [Suupplier], Spinner.ShortName [Spinner]
                From  M
                Inner Join YarnQCRemarksMaster RM On M.QCRemarksMasterID = RM.QCRemarksMasterID
                Inner Join {TableNames.YarnQCReqMaster} RQM On RM.QCReqMasterID = RQM.QCReqMasterID
                Left Join {DbNames.EPYSL}..Contacts Supplier On M.SupplierID = Supplier.ContactID
                Left Join {DbNames.EPYSL}..Contacts Spinner On M.SpinnerId = Spinner.ContactID
				Left Join {DbNames.EPYSL}..LoginUser LU On M.MRIRBy = LU.UserCode
				Inner Join {DbNames.EPYSL}..EntityTypeValue QCReqFor On RQM.QCForID = QCReqFor.ValueID;

                With 
                YRC AS (
	                Select * From YarnMRIRChild
	                Where MRIRMasterID = {id} 
                )

                Select YRC.MRIRChildID, YRC.YarnProgramId, YRC.ChallanCount, YRC.POCount, YRC.PhysicalCount, YRC.YarnCategory
	                , YRC.NoOfThread, YRC.UnitID, YRC.ItemMasterID, YRC.Remarks, ISNULL(YRC.YarnCategory, '') YarnCategory
                    , YarnType.SegmentValue YarnType, YarnCount.SegmentValue YarnCount, YarnComposition.SegmentValue YarnComposition
	                , Shade.SegmentValue Shade, YarnColor.SegmentValue YarnColor, IM.ItemMasterID, YRC.LotNo
	                , YRC.ReceiveQty, YRC.ReceiveQtyCone, YRC.ReceiveQtyCarton, U.DisplayUnitDesc UOM
                From YRC
                Inner Join {DbNames.EPYSL}..Unit U On YRC.UnitID = U.UnitID
                Inner Join {DbNames.EPYSL}..ItemMaster IM On YRC.ItemMasterID = IM.ItemMasterID
                Left Join {DbNames.EPYSL}..ItemSegmentValue YarnType On IM.Segment1ValueID = YarnType.SegmentValueID
                Left Join {DbNames.EPYSL}..ItemSegmentValue YarnCount On IM.Segment2ValueID = YarnCount.SegmentValueID
                Left Join {DbNames.EPYSL}..ItemSegmentValue YarnComposition On IM.Segment3ValueID = YarnComposition.SegmentValueID
                Left Join {DbNames.EPYSL}..ItemSegmentValue Shade On IM.Segment4ValueID = Shade.SegmentValueID
                Left Join {DbNames.EPYSL}..ItemSegmentValue YarnColor On IM.Segment5ValueID = YarnColor.SegmentValueID;";

            //var connection = _dbContext.Database.Connection;

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);

                YarnMRIRMaster data = await records.ReadFirstOrDefaultAsync<YarnMRIRMaster>();
                data.Childs = records.Read<YarnMRIRChild>().ToList();

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
        }*/
        public async Task<string> SaveAsync(YarnMRIRMaster entity, ReceiveNoteType receiveNoteType)
        {
            SqlTransaction transaction = null;
            SqlTransaction transactionGmt = null;
            try
            {
                List<YarnAllocationMaster> yarnAllocations = new List<YarnAllocationMaster>();
                List<YarnAllocationChild> yarnAllocationChilds = new List<YarnAllocationChild>();
                List<YarnAllocationChildItem> yarnAllocationChildItems = new List<YarnAllocationChildItem>();

                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();
                await _connectionGmt.OpenAsync();
                transactionGmt = _connectionGmt.BeginTransaction();
				string returnString = "";
                if (entity.EntityState == EntityState.Added)
                {
                    int maxChildId = 0;
                    entity.MRIRMasterId = await _service.GetMaxIdAsync(TableNames.YARN_MRIR_MASTER, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
					if (receiveNoteType == ReceiveNoteType.MRIR)
					{
                        entity.MRIRNo = await GetMaxMRIRNoAsync(transactionGmt);
						returnString = entity.MRIRNo;
                    }
                    if (receiveNoteType == ReceiveNoteType.GRN)
					{
                        entity.GRNNo = await GetMaxGRNNoAsync(transactionGmt);
						returnString = entity.GRNNo;
                    }
                    if (receiveNoteType == ReceiveNoteType.MRN)
					{
                        entity.MRNNo = await GetMaxMRNNoAsync(transactionGmt);
						returnString = entity.MRNNo;
                    }
                    //entity.MRIRNo = _signatureRepository.GetMaxNo(TableNames.YARN_MRIR_NO);
                    maxChildId = await _service.GetMaxIdAsync(TableNames.YARN_MRIR_CHILD, entity.YarnMRIRChilds.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);


                    int countAllocation = entity.YarnMRIRChilds.Count(x => x.AllocationChildID > 0);
                    int countAllocationMaster = 0;
                    int countAllocationChild = 0;
                    int countAllocationChildItem = 0;

                    if (countAllocation > 0)
                    {
                        countAllocationMaster = entity.YarnMRIRChilds.Count(x => x.AllocationChildID > 0);
                        countAllocationChild = 0;
                        countAllocationChildItem = 0;
                        entity.YarnMRIRChilds.Where(x => x.AllocationChildID > 0).ToList().ForEach(x =>
                        {
                            countAllocationChild += x.YarnAllocation.Childs.Count();
                            x.YarnAllocation.Childs.ForEach(c =>
                            {
                                countAllocationChildItem += c.ChildItems.Count();
                            });
                        });
                        countAllocationMaster = await _service.GetMaxIdAsync(TableNames.YARN_ALLOCATION_MASTER, countAllocationMaster, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        countAllocationChild = await _service.GetMaxIdAsync(TableNames.YARN_ALLOCATION_CHILD, countAllocationChild, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        countAllocationChildItem = await _service.GetMaxIdAsync(TableNames.YARN_ALLOCATION_CHILD_ITEM, countAllocationChildItem, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                    }

                    foreach (var item in entity.YarnMRIRChilds)
                    {
                        item.MRIRChildID = maxChildId++;
                        item.MRIRMasterId = entity.MRIRMasterId;

                        if (item.AllocationChildID > 0)
                        {
                            item.YarnAllocation.MRIRChildID = item.MRIRChildID;
                            item.YarnAllocation.YarnAllocationID = countAllocationMaster++;
                            //item.YarnAllocation.YarnAllocationNo = CommonFunction.DeepClone(await _yarnAllocationService.GetMaxYarnAllocationNoAsync());
                            item.YarnAllocation.YarnAllocationNo = CommonFunction.DeepClone(await GetMaxYarnAllocationNoAsync(transactionGmt));
                            item.YarnAllocation.Childs.ForEach(c =>
                            {
                                c.AllocationID = item.YarnAllocation.YarnAllocationID;
                                c.AllocationChildID = countAllocationChild++;
                                yarnAllocationChilds.Add(c);

                                c.ChildItems.ForEach(ci =>
                                {
                                    ci.AllocationChildItemID = countAllocationChildItem++;
                                    ci.AllocationChildID = c.AllocationChildID;
                                    yarnAllocationChildItems.Add(ci);
                                });
                            });
                            yarnAllocations.Add(item.YarnAllocation);
                        }
                    }
                }

                await _service.SaveSingleAsync(entity, transaction);
                await _service.SaveAsync(entity.YarnMRIRChilds, transaction);

                if (yarnAllocations.Count() > 0)
                {
                    var objList = yarnAllocations.Where(x => x.YarnAllocationNo.IsNullOrEmpty() == true).ToList();
                    if (objList.Count() > 0)
                    {
                        throw new Exception("Yarn Allocation no missing (MRIRService => SaveAsync).");
                    }
                    await _service.SaveAsync(yarnAllocations, transaction);
                    await _service.SaveAsync(yarnAllocationChilds, transaction);
                    await _service.SaveAsync(yarnAllocationChildItems, transaction);
                }
                /*
				#region Stock Operation
				if (entity.ReceiveNoteType == (int)ReceiveNoteType.MRIR)
				{
					int userId = entity.EntityState == EntityState.Added ? entity.AddedBy : entity.UpdatedBy;
					await _connection.ExecuteAsync("spYarnStockOperation", new { MasterID = entity.MRIRMasterId, FromMenuType = EnumFromMenuType.MRIR, UserId = userId }, transaction, 30, CommandType.StoredProcedure);
				}
				#endregion Stock Operation
				*/
                transaction.Commit();
                transactionGmt.Commit();
				return returnString;
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                if (transactionGmt != null) transactionGmt.Rollback();
                if (ex.Message.Contains('~')) throw new Exception(ex.Message.Split('~')[0]);
                throw ex;
            }
            finally
            {
                _connection.Close();
                _connectionGmt.Close();
            }
        }

        private async Task<string> GetMaxYarnAllocationNoAsync(SqlTransaction transactionGmt)
        {
            var id = await _service.GetMaxIdAsync(TableNames.YARN_ALLOCATION_NO, RepeatAfterEnum.EveryYear, transactionGmt, _connectionGmt);
            var datePart = DateTime.Now.ToString("yyMMdd");
            return $@"{datePart}{id:00000}";
        }
        private async Task<string> GetMaxMRIRNoAsync(SqlTransaction transaction)
        {
            var id = await _service.GetMaxIdAsync(TableNames.MRIR_No, RepeatAfterEnum.EveryYear, transaction, _connectionGmt);
            var datePart = DateTime.Now.ToString("yyMMdd");
            return $@"MRIR{datePart}{id:00000}";
        }
        private async Task<string> GetMaxGRNNoAsync(SqlTransaction transaction)
        {
            var id = await _service.GetMaxIdAsync(TableNames.GRN_No, RepeatAfterEnum.EveryYear, transaction, _connectionGmt);
            var datePart = DateTime.Now.ToString("yyMMdd");
            return $@"GRN{datePart}{id:00000}";
        }
        private async Task<string> GetMaxMRNNoAsync(SqlTransaction transaction)
        {
            var id = await _service.GetMaxIdAsync(TableNames.MRN_No, RepeatAfterEnum.EveryYear, transaction, _connectionGmt);
            var datePart = DateTime.Now.ToString("yyMMdd");
            return $@"MRN{datePart}{id:00000}";
        }
    }
}
