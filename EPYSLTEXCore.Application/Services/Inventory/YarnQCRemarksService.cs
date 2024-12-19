using Dapper;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Application.Interfaces;
using EPYSLTEXCore.Application.Interfaces.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;
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
using System.Security.Cryptography.Xml;
using System.Transactions;

namespace EPYSLTEXCore.Application.Services.Inventory
{
    public class YarnQCRemarksService : IYarnQCRemarksService
    {
        private readonly IDapperCRUDService<YarnQCRemarksMaster> _service;

        private readonly SqlConnection _connection;
        private readonly SqlConnection _connectionGmt;

        public YarnQCRemarksService(IDapperCRUDService<YarnQCRemarksMaster> service
            , IDapperCRUDService<YarnQCRemarksChild> itemMasterRepository)
        {
            _service = service;

            _service.Connection = service.GetConnection(AppConstants.GMT_CONNECTION);
            _connectionGmt = service.Connection;

            _service.Connection = service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _connection = service.Connection;
        }

        public async Task<List<YarnQCRemarksMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By QCRemarksMasterID Desc" : paginationInfo.OrderBy;

            string sql = "";

            switch (status)
            {
                case Status.Draft:
                    sql = $@"
                    With 
                    C As (
	                    Select * From {TableNames.YARN_QC_REMARKS_CHILD}
                    ), 
                    YQCRemarks As 
                    (
	                  Select C.QCRemarksChildID, M.QCRemarksMasterID, M.QCRemarksNo, M.QCRemarksDate, ER.EmployeeName QCRemarksByUser
	                    , M.QCReceiveMasterID, M.QCIssueMasterID, M.QCReqMasterID, ERCU.EmployeeName QCReceiveByUser, QCReqFor.ValueName QCReqFor
	                    , SUM(C.ReceiveQty) ReceiveQty, SUM(C.ReceiveQtyCone) ReceiveQtyCone, SUM(C.ReceiveQtyCarton) ReceiveQtyCarton,
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

	                    YRM.ReceiveNo, YRM.ReceiveDate,
	                    SpinnerID = CASE WHEN YRC2.SpinnerID > 0 THEN YRC2.SpinnerID ELSE YRM.SpinnerID END,
	                    YRC2.LotNo, YRC2.ChallanLot,
	                    YarnDetail = CONCAT(ISV6.SegmentValue,' ', ISV1.SegmentValue,' ', ISV3.SegmentValue,' ', ISV4.SegmentValue,' ', ISV2.SegmentValue,' ', ISV5.SegmentValue,' ', C.ShadeCode),
	                    TechnicalName = T.TechnicalName,
	                    BuyerName = CASE WHEN RMC1.BuyerID > 0 THEN B.ShortName ELSE '' END

	                    From C
	                    Inner Join {TableNames.YARN_QC_REMARKS_MASTER} M On M.QCRemarksMasterID = C.QCRemarksMasterID
		                INNER JOIN {TableNames.YARN_QC_RECEIVE_CHILD} QCRC ON QCRC.QCReceiveChildID = C.QCReceiveChildID
	                    Inner Join {TableNames.YARN_QC_RECEIVE_MASTER} RCM On RCM.QCReceiveMasterID = M.QCReceiveMasterID
		                INNER JOIN {TableNames.YARN_QC_ISSUE_CHILD} QCIC ON QCIC.QCIssueChildID = QCRC.QCIssueChildID
		                INNER JOIN {TableNames.YARN_QC_ISSUE_MASTER} QCIM ON QCIM.QCIssueMasterID = M.QCIssueMasterID
	                    Inner Join {TableNames.YARN_QC_REQ_CHILD} RMC1 On RMC1.QCReqChildID = QCIC.QCReqChildID
	                    Inner Join {TableNames.YARN_QC_REQ_MASTER} RM On RM.QCReqMasterID = M.QCReqMasterID
	                    INNER JOIN {TableNames.YARN_RECEIVE_CHILD} YRC2 ON YRC2.ChildID = RMC1.ReceiveChildID
	                    INNER JOIN {TableNames.YARN_RECEIVE_MASTER} YRM ON YRM.ReceiveID = M.ReceiveID
	                    Left Join {DbNames.EPYSL}..EntityTypeValue QCReqFor On RM.QCForID = QCReqFor.ValueID
	                    Inner Join {DbNames.EPYSL}..LoginUser RU On M.QCRemarksBy = RU.UserCode 
					    Inner Join {DbNames.EPYSL}..Employee ER On ER.EmployeeCode = RU.EmployeeCode
	                    Inner Join {DbNames.EPYSL}..LoginUser RCU On RCM.QCReceivedBy = RCU.UserCode
					    Inner Join {DbNames.EPYSL}..Employee ERCU On ERCU.EmployeeCode = RCU.EmployeeCode
	                    LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME} T ON T.TechnicalNameId = RMC1.TechnicalNameId
	                    LEFT JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = RMC1.BuyerID
	                    LEFT JOIN {TableNames.YarnAssessmentStatus} Z ON Z.YarnStatusID = C.YarnStatusID
	                    Inner Join {DbNames.EPYSL}..ItemMaster IM On IM.ItemMasterID = C.ItemMasterID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                        Where  M.IsSendForApproval=0 And M.IsApproved=0
	                    Group By C.QCRemarksChildID, M.QCRemarksMasterID, M.QCRemarksNo, M.QCRemarksDate, ER.EmployeeName,
	                    M.QCReceiveMasterID, M.QCIssueMasterID, M.QCReqMasterID, ERCU.EmployeeName, QCReqFor.ValueName,
	                    ISV1.SegmentValue,ISV2.SegmentValue, ISV3.SegmentValue,
	                    ISV4.SegmentValue, ISV5.SegmentValue,ISV6.SegmentValue,
	                    ISV7.SegmentValue,C.ShadeCode,YRM.ReceiveNo,YRM.ReceiveDate,YRC2.SpinnerID,YRM.SpinnerID,
	                    YRC2.LotNo,YRC2.ChallanLot,T.TechnicalName,RMC1.BuyerID,B.ShortName,C.Remarks,
	                    Z.YarnStatus,C.Approve,C.Reject,C.Retest,C.Diagnostic,C.CommerciallyApprove, RCM.QCReceiveNo, 
		                RM.QCReqNo, RCM.QCReceiveNo, QCIM.QCIssueNo
                    ),
                    FinalList AS
                    (
	                    SELECT FL.*,Spinner = CASE WHEN FL.SpinnerID > 0 THEN CCS.ShortName ELSE '' END
	                    FROM YQCRemarks FL
	                    LEFT JOIN {DbNames.EPYSL}..Contacts CCS ON CCS.ContactID = FL.SpinnerID
                    )
                    Select *, Count(*) Over() TotalRows From FinalList";
                    break;
                case Status.ReTest:
                    sql = $@"
                    With 
                    C As (
	                    Select C.* 
                        From {TableNames.YARN_QC_REMARKS_CHILD} C
                        INNER JOIN {TableNames.YARN_QC_REMARKS_MASTER} M ON M.QCRemarksMasterID = C.QCRemarksMasterID
						LEFT JOIN {TableNames.YARN_MRIR_CHILD} YMC ON YMC.QCRemarksChildID=C.QCRemarksChildID 
						LEFT JOIN {TableNames.YARN_MRIR_MASTER} YMM ON YMM.MRIRMasterId=YMC.MRIRMasterID
	                    Where (C.Diagnostic = 0 AND C.ReTest = 1 And C.Approve = 0 And C.Reject = 0) OR M.IsRetest = 1 OR YMM.ReTest=1
                    ), 
                    YQCRemarks As 
                    (
	                  Select C.QCRemarksChildID, M.QCRemarksMasterID, M.QCRemarksNo, M.QCRemarksDate, ER.EmployeeName QCRemarksByUser
	                    , M.QCReceiveMasterID, M.QCIssueMasterID, M.QCReqMasterID, ERCU.EmployeeName QCReceiveByUser, QCReqFor.ValueName QCReqFor
	                    , SUM(C.ReceiveQty) ReceiveQty, SUM(C.ReceiveQtyCone) ReceiveQtyCone, SUM(C.ReceiveQtyCarton) ReceiveQtyCarton,
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

	                    YRM.ReceiveNo, YRM.ReceiveDate,
	                    SpinnerID = CASE WHEN YRC2.SpinnerID > 0 THEN YRC2.SpinnerID ELSE YRM.SpinnerID END,
	                    YRC2.LotNo, YRC2.ChallanLot,
	                    YarnDetail = CONCAT(ISV6.SegmentValue,' ', ISV1.SegmentValue,' ', ISV3.SegmentValue,' ', ISV4.SegmentValue,' ', ISV2.SegmentValue,' ', ISV5.SegmentValue,' ', C.ShadeCode),
	                    TechnicalName = T.TechnicalName,
	                    BuyerName = CASE WHEN RMC1.BuyerID > 0 THEN B.ShortName ELSE '' END,
						ERS.EmployeeName RetestReqBy,YMM.RetestReason   ,M.ApprovedDate

	                    From C
	                    Inner Join {TableNames.YARN_QC_REMARKS_MASTER} M On M.QCRemarksMasterID = C.QCRemarksMasterID
		                INNER JOIN {TableNames.YARN_QC_RECEIVE_CHILD} QCRC ON QCRC.QCReceiveChildID = C.QCReceiveChildID
	                    Inner Join {TableNames.YARN_QC_RECEIVE_MASTER} RCM On RCM.QCReceiveMasterID = M.QCReceiveMasterID
		                INNER JOIN {TableNames.YARN_QC_ISSUE_CHILD} QCIC ON QCIC.QCIssueChildID = QCRC.QCIssueChildID
		                INNER JOIN {TableNames.YARN_QC_ISSUE_MASTER} QCIM ON QCIM.QCIssueMasterID = M.QCIssueMasterID
	                    Inner Join {TableNames.YARN_QC_REQ_CHILD} RMC1 On RMC1.QCReqChildID = QCIC.QCReqChildID
	                    Inner Join {TableNames.YARN_QC_REQ_MASTER} RM On RM.QCReqMasterID = M.QCReqMasterID
	                    INNER JOIN {TableNames.YARN_RECEIVE_CHILD} YRC2 ON YRC2.ChildID = RMC1.ReceiveChildID
	                    INNER JOIN {TableNames.YARN_RECEIVE_MASTER} YRM ON YRM.ReceiveID = YRC2.ReceiveID
						LEFT JOIN {TableNames.YARN_MRIR_CHILD} YMC ON YMC.QCRemarksChildID=C.QCRemarksChildID 
						LEFT JOIN {TableNames.YARN_MRIR_MASTER} YMM ON YMM.MRIRMasterId=YMC.MRIRMasterID
	                    Left Join {DbNames.EPYSL}..EntityTypeValue QCReqFor On RM.QCForID = QCReqFor.ValueID
	                    Inner Join {DbNames.EPYSL}..LoginUser RU On M.QCRemarksBy = RU.UserCode 
					    Inner Join {DbNames.EPYSL}..Employee ER On ER.EmployeeCode = RU.EmployeeCode
						LEFT JOIN {DbNames.EPYSL}..LoginUser RUS On RUS.UserCode = YMM.ReTestBy
					    LEFT JOIN {DbNames.EPYSL}..Employee ERS On ERS.EmployeeCode = RUS.EmployeeCode
	                    Inner Join {DbNames.EPYSL}..LoginUser RCU On RCM.QCReceivedBy = RCU.UserCode
					    Inner Join {DbNames.EPYSL}..Employee ERCU On ERCU.EmployeeCode = RCU.EmployeeCode
	                    LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME} T ON T.TechnicalNameId = RMC1.TechnicalNameId
	                    LEFT JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = RMC1.BuyerID
	                    LEFT JOIN {TableNames.YarnAssessmentStatus} Z ON Z.YarnStatusID = C.YarnStatusID
	                    Inner Join {DbNames.EPYSL}..ItemMaster IM On IM.ItemMasterID = C.ItemMasterID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                        Where YMM.ReTest=1 OR M.IsApproved=1
	                    Group By C.QCRemarksChildID, M.QCRemarksMasterID, M.QCRemarksNo, M.QCRemarksDate, ER.EmployeeName,
	                    M.QCReceiveMasterID, M.QCIssueMasterID, M.QCReqMasterID, ERCU.EmployeeName, QCReqFor.ValueName,
	                    ISV1.SegmentValue,ISV2.SegmentValue, ISV3.SegmentValue,
	                    ISV4.SegmentValue, ISV5.SegmentValue,ISV6.SegmentValue,
	                    ISV7.SegmentValue,C.ShadeCode,YRM.ReceiveNo, YRM.ReceiveDate,YRC2.SpinnerID,YRM.SpinnerID,
	                    YRC2.LotNo,YRC2.ChallanLot,T.TechnicalName,RMC1.BuyerID,B.ShortName,C.Remarks,
	                    Z.YarnStatus,C.Approve,C.Reject,C.Retest,C.Diagnostic,C.CommerciallyApprove, RCM.QCReceiveNo, 
		                RM.QCReqNo, RCM.QCReceiveNo, QCIM.QCIssueNo,ERS.EmployeeName,YMM.RetestReason   ,M.ApprovedDate
                    ),
                    FinalList AS
                    (
	                    SELECT FL.*,Spinner = CASE WHEN FL.SpinnerID > 0 THEN CCS.ShortName ELSE '' END
	                    FROM YQCRemarks FL
	                    LEFT JOIN {DbNames.EPYSL}..Contacts CCS ON CCS.ContactID = FL.SpinnerID
                    )
                    Select *, Count(*) Over() TotalRows From FinalList";
                    break;
                case Status.CheckReject:
                    sql = $@"
                    With 
                    C As (
	                    Select * From {TableNames.YARN_QC_REMARKS_CHILD}
	                    Where Diagnostic = 1
                    ), 
                    YQCRemarks As 
                    (
	                   Select C.QCRemarksChildID, M.QCRemarksMasterID, M.QCRemarksNo, M.QCRemarksDate, ER.EmployeeName QCRemarksByUser
	                    , M.QCReceiveMasterID, M.QCIssueMasterID, M.QCReqMasterID, ERCU.EmployeeName QCReceiveByUser, QCReqFor.ValueName QCReqFor
	                    , SUM(C.ReceiveQty) ReceiveQty, SUM(C.ReceiveQtyCone) ReceiveQtyCone, SUM(C.ReceiveQtyCarton) ReceiveQtyCarton,
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

	                    YRM.ReceiveNo, YRM.ReceiveDate,
	                    SpinnerID = CASE WHEN YRC2.SpinnerID > 0 THEN YRC2.SpinnerID ELSE YRM.SpinnerID END,
	                    YRC2.LotNo,YRC2.ChallanLot,
	                    YarnDetail = CONCAT(ISV6.SegmentValue,' ', ISV1.SegmentValue,' ', ISV3.SegmentValue,' ', ISV4.SegmentValue,' ', ISV2.SegmentValue,' ', ISV5.SegmentValue,' ', C.ShadeCode),
	                    TechnicalName = T.TechnicalName,
	                    BuyerName = CASE WHEN RMC1.BuyerID > 0 THEN B.ShortName ELSE '' END   ,M.ApprovedDate

	                    From C

                        Inner Join {TableNames.YARN_QC_REMARKS_MASTER} M On M.QCRemarksMasterID = C.QCRemarksMasterID
		                INNER JOIN {TableNames.YARN_QC_RECEIVE_CHILD} QCRC ON QCRC.QCReceiveChildID = C.QCReceiveChildID
	                    Inner Join {TableNames.YARN_QC_RECEIVE_MASTER} RCM On RCM.QCReceiveMasterID = M.QCReceiveMasterID
		                INNER JOIN {TableNames.YARN_QC_ISSUE_CHILD} QCIC ON QCIC.QCIssueChildID = QCRC.QCIssueChildID
		                INNER JOIN {TableNames.YARN_QC_ISSUE_MASTER} QCIM ON QCIM.QCIssueMasterID = M.QCIssueMasterID
	                    Inner Join {TableNames.YARN_QC_REQ_CHILD} RMC1 On RMC1.QCReqChildID = QCIC.QCReqChildID
	                    Inner Join {TableNames.YARN_QC_REQ_MASTER} RM On RM.QCReqMasterID = M.QCReqMasterID
	                    INNER JOIN {TableNames.YARN_RECEIVE_CHILD} YRC2 ON YRC2.ChildID = RMC1.ReceiveChildID
	                    INNER JOIN {TableNames.YARN_RECEIVE_MASTER} YRM ON YRM.ReceiveID = M.ReceiveID
	                    Left Join {DbNames.EPYSL}..EntityTypeValue QCReqFor On RM.QCForID = QCReqFor.ValueID
	                     Inner Join {DbNames.EPYSL}..LoginUser RU On M.QCRemarksBy = RU.UserCode 
					    Inner Join {DbNames.EPYSL}..Employee ER On ER.EmployeeCode = RU.EmployeeCode
	                    Inner Join {DbNames.EPYSL}..LoginUser RCU On RCM.QCReceivedBy = RCU.UserCode
					    Inner Join {DbNames.EPYSL}..Employee ERCU On ERCU.EmployeeCode = RCU.EmployeeCode
	                    LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME} T ON T.TechnicalNameId = RMC1.TechnicalNameId
	                    LEFT JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = RMC1.BuyerID
	                    LEFT JOIN {TableNames.YarnAssessmentStatus} Z ON Z.YarnStatusID = C.YarnStatusID
	                    Inner Join {DbNames.EPYSL}..ItemMaster IM On IM.ItemMasterID = C.ItemMasterID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                        Where M.IsApproved=1
	                    Group By C.QCRemarksChildID, M.QCRemarksMasterID, M.QCRemarksNo, M.QCRemarksDate, ER.EmployeeName,
	                    M.QCReceiveMasterID, M.QCIssueMasterID, M.QCReqMasterID, ERCU.EmployeeName, QCReqFor.ValueName,
	                    ISV1.SegmentValue,ISV2.SegmentValue, ISV3.SegmentValue,
	                    ISV4.SegmentValue, ISV5.SegmentValue,ISV6.SegmentValue,
	                    ISV7.SegmentValue,C.ShadeCode,YRM.ReceiveNo, YRM.ReceiveDate,YRC2.SpinnerID,YRM.SpinnerID,
	                    YRC2.LotNo,YRC2.ChallanLot,T.TechnicalName,RMC1.BuyerID,B.ShortName,C.Remarks,
	                    Z.YarnStatus,C.Approve,C.Reject,C.Retest,C.Diagnostic,C.CommerciallyApprove, RCM.QCReceiveNo, 
		                RM.QCReqNo, RCM.QCReceiveNo, QCIM.QCIssueNo   ,M.ApprovedDate
                    ),
                    FinalList AS
                    (
	                    SELECT FL.*,Spinner = CASE WHEN FL.SpinnerID > 0 THEN CCS.ShortName ELSE '' END
	                    FROM YQCRemarks FL
	                    LEFT JOIN {DbNames.EPYSL}..Contacts CCS ON CCS.ContactID = FL.SpinnerID
                    )
                    Select *, Count(*) Over() TotalRows From FinalList";
                    break;
                case Status.Approved:
                    sql = $@"
                    With 
                    C As (
	                    Select * From {TableNames.YARN_QC_REMARKS_CHILD}
	                    Where Approve = 1 And Reject = 0
                    ), 
                    YQCRemarks As 
                    (
	                   Select C.QCRemarksChildID, M.QCRemarksMasterID, M.QCRemarksNo, M.QCRemarksDate, ER.EmployeeName QCRemarksByUser
	                    , M.QCReceiveMasterID, M.QCIssueMasterID, M.QCReqMasterID, ERCU.EmployeeName QCReceiveByUser, QCReqFor.ValueName QCReqFor
	                    , SUM(C.ReceiveQty) ReceiveQty, SUM(C.ReceiveQtyCone) ReceiveQtyCone, SUM(C.ReceiveQtyCarton) ReceiveQtyCarton,
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

	                    YRM.ReceiveNo, YRM.ReceiveDate,
	                    SpinnerID = CASE WHEN YRC2.SpinnerID > 0 THEN YRC2.SpinnerID ELSE YRM.SpinnerID END,
	                    YRC2.LotNo, YRC2.ChallanLot,
	                    YarnDetail = CONCAT(ISV6.SegmentValue,' ', ISV1.SegmentValue,' ', ISV3.SegmentValue,' ', ISV4.SegmentValue,' ', ISV2.SegmentValue,' ', ISV5.SegmentValue,' ', C.ShadeCode),
	                    TechnicalName = T.TechnicalName,
	                    BuyerName = CASE WHEN RMC1.BuyerID > 0 THEN B.ShortName ELSE '' END
                        ,M.ApprovedDate
	                    From C

                        Inner Join {TableNames.YARN_QC_REMARKS_MASTER} M On M.QCRemarksMasterID = C.QCRemarksMasterID
		                INNER JOIN {TableNames.YARN_QC_RECEIVE_CHILD} QCRC ON QCRC.QCReceiveChildID = C.QCReceiveChildID
	                    Inner Join {TableNames.YARN_QC_RECEIVE_MASTER} RCM On RCM.QCReceiveMasterID = M.QCReceiveMasterID
		                INNER JOIN {TableNames.YARN_QC_ISSUE_CHILD} QCIC ON QCIC.QCIssueChildID = QCRC.QCIssueChildID
		                INNER JOIN {TableNames.YARN_QC_ISSUE_MASTER} QCIM ON QCIM.QCIssueMasterID = M.QCIssueMasterID
	                    Inner Join {TableNames.YARN_QC_REQ_CHILD} RMC1 On RMC1.QCReqChildID = QCIC.QCReqChildID
	                    Inner Join {TableNames.YARN_QC_REQ_MASTER} RM On RM.QCReqMasterID = M.QCReqMasterID
	                    INNER JOIN {TableNames.YARN_RECEIVE_CHILD} YRC2 ON YRC2.ChildID = RMC1.ReceiveChildID
	                    INNER JOIN {TableNames.YARN_RECEIVE_MASTER} YRM ON YRM.ReceiveID = M.ReceiveID
	                    Left Join {DbNames.EPYSL}..EntityTypeValue QCReqFor On RM.QCForID = QCReqFor.ValueID
	                    Inner Join {DbNames.EPYSL}..LoginUser RU On M.QCRemarksBy = RU.UserCode 
					    Inner Join {DbNames.EPYSL}..Employee ER On ER.EmployeeCode = RU.EmployeeCode
	                    Inner Join {DbNames.EPYSL}..LoginUser RCU On RCM.QCReceivedBy = RCU.UserCode
					    Inner Join {DbNames.EPYSL}..Employee ERCU On ERCU.EmployeeCode = RCU.EmployeeCode
	                    LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME} T ON T.TechnicalNameId = RMC1.TechnicalNameId
	                    LEFT JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = RMC1.BuyerID
	                    LEFT JOIN {TableNames.YarnAssessmentStatus} Z ON Z.YarnStatusID = C.YarnStatusID
	                    Inner Join {DbNames.EPYSL}..ItemMaster IM On IM.ItemMasterID = C.ItemMasterID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                        Where M.IsApproved=1
	                    Group By C.QCRemarksChildID, M.QCRemarksMasterID, M.QCRemarksNo, M.QCRemarksDate, ER.EmployeeName,
	                    M.QCReceiveMasterID, M.QCIssueMasterID, M.QCReqMasterID, ERCU.EmployeeName, QCReqFor.ValueName,
	                    ISV1.SegmentValue,ISV2.SegmentValue, ISV3.SegmentValue,
	                    ISV4.SegmentValue, ISV5.SegmentValue,ISV6.SegmentValue,
	                    ISV7.SegmentValue,C.ShadeCode,YRM.ReceiveNo, YRM.ReceiveDate,YRC2.SpinnerID,YRM.SpinnerID,
	                    YRC2.LotNo, YRC2.ChallanLot,T.TechnicalName,RMC1.BuyerID,B.ShortName,C.Remarks,
	                    Z.YarnStatus,C.Approve,C.Reject,C.Retest,C.Diagnostic,C.CommerciallyApprove, RCM.QCReceiveNo, 
		                RM.QCReqNo, RCM.QCReceiveNo, QCIM.QCIssueNo   ,M.ApprovedDate
                    ),
                    FinalList AS
                    (
	                    SELECT FL.*,Spinner = CASE WHEN FL.SpinnerID > 0 THEN CCS.ShortName ELSE '' END
	                    FROM YQCRemarks FL
	                    LEFT JOIN {DbNames.EPYSL}..Contacts CCS ON CCS.ContactID = FL.SpinnerID
                    )
                    Select *, Count(*) Over() TotalRows From FinalList
                    ";
                    break;
                case Status.ProposedForApproval:
                    sql = $@"
                    With 
                    YQCRemarks As 
                    (
	                   Select C.QCRemarksChildID, M.QCRemarksMasterID, M.QCRemarksNo, M.QCRemarksDate, ER.EmployeeName QCRemarksByUser
	                    , M.QCReceiveMasterID, M.QCIssueMasterID, M.QCReqMasterID, ERCU.EmployeeName QCReceiveByUser, QCReqFor.ValueName QCReqFor
	                    , SUM(C.ReceiveQty) ReceiveQty, SUM(C.ReceiveQtyCone) ReceiveQtyCone, SUM(C.ReceiveQtyCarton) ReceiveQtyCarton,
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

	                    YRM.ReceiveNo, YRM.ReceiveDate,
	                    SpinnerID = CASE WHEN YRC2.SpinnerID > 0 THEN YRC2.SpinnerID ELSE YRM.SpinnerID END,
	                    YRC2.LotNo, YRC2.ChallanLot,
	                    YarnDetail = CONCAT(ISV6.SegmentValue,' ', ISV1.SegmentValue,' ', ISV3.SegmentValue,' ', ISV4.SegmentValue,' ', ISV2.SegmentValue,' ', ISV5.SegmentValue,' ', C.ShadeCode),
	                    TechnicalName = T.TechnicalName,
	                    BuyerName = CASE WHEN RMC1.BuyerID > 0 THEN B.ShortName ELSE '' END

	                    From {TableNames.YARN_QC_REMARKS_CHILD} C
	                    Inner Join {TableNames.YARN_QC_REMARKS_MASTER} M On M.QCRemarksMasterID = C.QCRemarksMasterID
		                INNER JOIN {TableNames.YARN_QC_RECEIVE_CHILD} QCRC ON QCRC.QCReceiveChildID = C.QCReceiveChildID
	                    Inner Join {TableNames.YARN_QC_RECEIVE_MASTER} RCM On RCM.QCReceiveMasterID = M.QCReceiveMasterID
		                INNER JOIN {TableNames.YARN_QC_ISSUE_CHILD} QCIC ON QCIC.QCIssueChildID = QCRC.QCIssueChildID
		                INNER JOIN {TableNames.YARN_QC_ISSUE_MASTER} QCIM ON QCIM.QCIssueMasterID = M.QCIssueMasterID
	                    Inner Join {TableNames.YARN_QC_REQ_CHILD} RMC1 On RMC1.QCReqChildID = QCIC.QCReqChildID
	                    Inner Join {TableNames.YARN_QC_REQ_MASTER} RM On RM.QCReqMasterID = M.QCReqMasterID
	                    INNER JOIN {TableNames.YARN_RECEIVE_CHILD} YRC2 ON YRC2.ChildID = RMC1.ReceiveChildID
	                    INNER JOIN {TableNames.YARN_RECEIVE_MASTER} YRM ON YRM.ReceiveID = M.ReceiveID
	                    Left Join {DbNames.EPYSL}..EntityTypeValue QCReqFor On RM.QCForID = QCReqFor.ValueID
	                    Inner Join {DbNames.EPYSL}..LoginUser RU On M.QCRemarksBy = RU.UserCode 
		                Inner Join {DbNames.EPYSL}..Employee ER On ER.EmployeeCode = RU.EmployeeCode
	                    Inner Join {DbNames.EPYSL}..LoginUser RCU On RCM.QCReceivedBy = RCU.UserCode
		                Inner Join {DbNames.EPYSL}..Employee ERCU On ERCU.EmployeeCode = RCU.EmployeeCode
	                    LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME} T ON T.TechnicalNameId = RMC1.TechnicalNameId
	                    LEFT JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = RMC1.BuyerID
	                    LEFT JOIN {TableNames.YarnAssessmentStatus} Z ON Z.YarnStatusID = C.YarnStatusID
	                    Inner Join {DbNames.EPYSL}..ItemMaster IM On IM.ItemMasterID = C.ItemMasterID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                        Where M.IsSendForApproval = 1 And M.IsApproved = 0
	                    Group By C.QCRemarksChildID, M.QCRemarksMasterID, M.QCRemarksNo, M.QCRemarksDate, ER.EmployeeName,
	                    M.QCReceiveMasterID, M.QCIssueMasterID, M.QCReqMasterID, ERCU.EmployeeName, QCReqFor.ValueName,
	                    ISV1.SegmentValue,ISV2.SegmentValue, ISV3.SegmentValue,
	                    ISV4.SegmentValue, ISV5.SegmentValue,ISV6.SegmentValue,
	                    ISV7.SegmentValue,C.ShadeCode,YRM.ReceiveNo, YRM.ReceiveDate,YRC2.SpinnerID,YRM.SpinnerID,
	                    YRC2.LotNo, YRC2.ChallanLot,T.TechnicalName,RMC1.BuyerID,B.ShortName,C.Remarks,
	                    Z.YarnStatus,C.Approve,C.Reject,C.Retest,C.Diagnostic,C.CommerciallyApprove, RCM.QCReceiveNo, 
		                RM.QCReqNo, RCM.QCReceiveNo, QCIM.QCIssueNo
                    ),
                    FinalList AS
                    (
	                    SELECT FL.*,Spinner = CASE WHEN FL.SpinnerID > 0 THEN CCS.ShortName ELSE '' END
	                    FROM YQCRemarks FL
	                    LEFT JOIN {DbNames.EPYSL}..Contacts CCS ON CCS.ContactID = FL.SpinnerID
                    )
                    Select *, Count(*) Over() TotalRows From FinalList
                ";
                    break;
                case Status.ApprovedDone:
                    sql = $@"
                    With 
                    C As (
	                    Select * From {TableNames.YARN_QC_REMARKS_CHILD}
	                
                    ), 
                    YQCRemarks As 
                    (
	                   Select C.QCRemarksChildID, M.QCRemarksMasterID, M.QCRemarksNo, M.QCRemarksDate, ER.EmployeeName QCRemarksByUser
	                    , M.QCReceiveMasterID, M.QCIssueMasterID, M.QCReqMasterID, ERCU.EmployeeName QCReceiveByUser, QCReqFor.ValueName QCReqFor
	                    , SUM(C.ReceiveQty) ReceiveQty, SUM(C.ReceiveQtyCone) ReceiveQtyCone, SUM(C.ReceiveQtyCarton) ReceiveQtyCarton,
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

	                    YRM.ReceiveNo, YRM.ReceiveDate,
	                    SpinnerID = CASE WHEN YRC2.SpinnerID > 0 THEN YRC2.SpinnerID ELSE YRM.SpinnerID END,
	                    YRC2.LotNo, YRC2.ChallanLot,
	                    YarnDetail = CONCAT(ISV6.SegmentValue,' ', ISV1.SegmentValue,' ', ISV3.SegmentValue,' ', ISV4.SegmentValue,' ', ISV2.SegmentValue,' ', ISV5.SegmentValue,' ', C.ShadeCode),
	                    TechnicalName = T.TechnicalName,
	                    BuyerName = CASE WHEN RMC1.BuyerID > 0 THEN B.ShortName ELSE '' END
                        ,M.ApprovedDate
	                    From C

                        Inner Join {TableNames.YARN_QC_REMARKS_MASTER} M On M.QCRemarksMasterID = C.QCRemarksMasterID
		                INNER JOIN {TableNames.YARN_QC_RECEIVE_CHILD} QCRC ON QCRC.QCReceiveChildID = C.QCReceiveChildID
	                    Inner Join {TableNames.YARN_QC_RECEIVE_MASTER} RCM On RCM.QCReceiveMasterID = M.QCReceiveMasterID
		                INNER JOIN {TableNames.YARN_QC_ISSUE_CHILD} QCIC ON QCIC.QCIssueChildID = QCRC.QCIssueChildID
		                INNER JOIN {TableNames.YARN_QC_ISSUE_MASTER} QCIM ON QCIM.QCIssueMasterID = QCIC.QCIssueMasterID
	                    Inner Join {TableNames.YARN_QC_REQ_CHILD} RMC1 On RMC1.QCReqChildID = QCIC.QCReqChildID
	                    Inner Join {TableNames.YARN_QC_REQ_MASTER} RM On RM.QCReqMasterID = RMC1.QCReqMasterID
	                    INNER JOIN {TableNames.YARN_RECEIVE_CHILD} YRC2 ON YRC2.ChildID = RMC1.ReceiveChildID
	                    INNER JOIN {TableNames.YARN_RECEIVE_MASTER} YRM ON YRM.ReceiveID = YRC2.ReceiveID		             
	                    Left Join {DbNames.EPYSL}..EntityTypeValue QCReqFor On RM.QCForID = QCReqFor.ValueID
	                    Inner Join {DbNames.EPYSL}..LoginUser RU On M.QCRemarksBy = RU.UserCode 
					    Inner Join {DbNames.EPYSL}..Employee ER On ER.EmployeeCode = RU.EmployeeCode
	                    Inner Join {DbNames.EPYSL}..LoginUser RCU On RCM.QCReceivedBy = RCU.UserCode
					    Inner Join {DbNames.EPYSL}..Employee ERCU On ERCU.EmployeeCode = RCU.EmployeeCode
	                    LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME} T ON T.TechnicalNameId = RMC1.TechnicalNameId
	                    LEFT JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = RMC1.BuyerID
	                    LEFT JOIN {TableNames.YarnAssessmentStatus} Z ON Z.YarnStatusID = C.YarnStatusID
	                    Inner Join {DbNames.EPYSL}..ItemMaster IM On IM.ItemMasterID = C.ItemMasterID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                        Where M.IsApproved = 1
	                    Group By C.QCRemarksChildID, M.QCRemarksMasterID, M.QCRemarksNo, M.QCRemarksDate, ER.EmployeeName,
	                    M.QCReceiveMasterID, M.QCIssueMasterID, M.QCReqMasterID, ERCU.EmployeeName, QCReqFor.ValueName,
	                    ISV1.SegmentValue,ISV2.SegmentValue, ISV3.SegmentValue,
	                    ISV4.SegmentValue, ISV5.SegmentValue,ISV6.SegmentValue,
	                    ISV7.SegmentValue,C.ShadeCode,YRM.ReceiveNo, YRM.ReceiveDate,YRC2.SpinnerID,YRM.SpinnerID,
	                    YRC2.LotNo, YRC2.ChallanLot,T.TechnicalName,RMC1.BuyerID,B.ShortName,C.Remarks,
	                    Z.YarnStatus,C.Approve,C.Reject,C.Retest,C.Diagnostic,C.CommerciallyApprove, RCM.QCReceiveNo, 
		                RM.QCReqNo, RCM.QCReceiveNo, QCIM.QCIssueNo,M.ApprovedDate
                    ),
                    FinalList AS
                    (
	                    SELECT FL.*,Spinner = CASE WHEN FL.SpinnerID > 0 THEN CCS.ShortName ELSE '' END
	                    FROM YQCRemarks FL
	                    LEFT JOIN {DbNames.EPYSL}..Contacts CCS ON CCS.ContactID = FL.SpinnerID
                    )
                    Select *, Count(*) Over() TotalRows From FinalList
                ";
                    break;
                case Status.Approved2:
                    sql = $@"
                    With 
                    C As (
	                    Select * From {TableNames.YARN_QC_REMARKS_CHILD}
	                    Where CommerciallyApprove = 1
                    ), 
                    YQCRemarks As 
                    (
	                   Select C.QCRemarksChildID, M.QCRemarksMasterID, M.QCRemarksNo, M.QCRemarksDate, ER.EmployeeName QCRemarksByUser
	                    , M.QCReceiveMasterID, M.QCIssueMasterID, M.QCReqMasterID, ERCU.EmployeeName QCReceiveByUser, QCReqFor.ValueName QCReqFor
	                    , SUM(C.ReceiveQty) ReceiveQty, SUM(C.ReceiveQtyCone) ReceiveQtyCone, SUM(C.ReceiveQtyCarton) ReceiveQtyCarton,
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

	                    YRM.ReceiveNo, YRM.ReceiveDate,
	                    SpinnerID = CASE WHEN YRC2.SpinnerID > 0 THEN YRC2.SpinnerID ELSE YRM.SpinnerID END,
	                    YRC2.LotNo,YRC2.ChallanLot,
	                    YarnDetail = CONCAT(ISV6.SegmentValue,' ', ISV1.SegmentValue,' ', ISV3.SegmentValue,' ', ISV4.SegmentValue,' ', ISV2.SegmentValue,' ', ISV5.SegmentValue,' ', C.ShadeCode),
	                    TechnicalName = T.TechnicalName,
	                    BuyerName = CASE WHEN RMC1.BuyerID > 0 THEN B.ShortName ELSE '' END
                        ,M.ApprovedDate
	                    From C

                        Inner Join {TableNames.YARN_QC_REMARKS_MASTER} M On M.QCRemarksMasterID = C.QCRemarksMasterID
		                INNER JOIN {TableNames.YARN_QC_RECEIVE_CHILD} QCRC ON QCRC.QCReceiveChildID = C.QCReceiveChildID
	                    Inner Join {TableNames.YARN_QC_RECEIVE_MASTER} RCM On RCM.QCReceiveMasterID = M.QCReceiveMasterID
		                INNER JOIN {TableNames.YARN_QC_ISSUE_CHILD} QCIC ON QCIC.QCIssueChildID = QCRC.QCIssueChildID
		                INNER JOIN {TableNames.YARN_QC_ISSUE_MASTER} QCIM ON QCIM.QCIssueMasterID = M.QCIssueMasterID
	                    Inner Join {TableNames.YARN_QC_REQ_CHILD} RMC1 On RMC1.QCReqChildID = QCIC.QCReqChildID
	                    Inner Join {TableNames.YARN_QC_REQ_MASTER} RM On RM.QCReqMasterID = M.QCReqMasterID
	                    INNER JOIN {TableNames.YARN_RECEIVE_CHILD} YRC2 ON YRC2.ChildID = RMC1.ReceiveChildID
	                    INNER JOIN {TableNames.YARN_RECEIVE_MASTER} YRM ON YRM.ReceiveID = M.ReceiveID	
	                    Left Join {DbNames.EPYSL}..EntityTypeValue QCReqFor On RM.QCForID = QCReqFor.ValueID
	                     Inner Join {DbNames.EPYSL}..LoginUser RU On M.QCRemarksBy = RU.UserCode 
					    Inner Join {DbNames.EPYSL}..Employee ER On ER.EmployeeCode = RU.EmployeeCode
	                    Inner Join {DbNames.EPYSL}..LoginUser RCU On RCM.QCReceivedBy = RCU.UserCode
					    Inner Join {DbNames.EPYSL}..Employee ERCU On ERCU.EmployeeCode = RCU.EmployeeCode
	                    LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME} T ON T.TechnicalNameId = RMC1.TechnicalNameId
	                    LEFT JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = RMC1.BuyerID
	                    LEFT JOIN {TableNames.YarnAssessmentStatus} Z ON Z.YarnStatusID = C.YarnStatusID
	                    Inner Join {DbNames.EPYSL}..ItemMaster IM On IM.ItemMasterID = C.ItemMasterID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                        Where M.IsApproved=1
	                    Group By C.QCRemarksChildID, M.QCRemarksMasterID, M.QCRemarksNo, M.QCRemarksDate, ER.EmployeeName,
	                    M.QCReceiveMasterID, M.QCIssueMasterID, M.QCReqMasterID, ERCU.EmployeeName, QCReqFor.ValueName,
	                    ISV1.SegmentValue,ISV2.SegmentValue, ISV3.SegmentValue,
	                    ISV4.SegmentValue, ISV5.SegmentValue,ISV6.SegmentValue,
	                    ISV7.SegmentValue,C.ShadeCode,YRM.ReceiveNo, YRM.ReceiveDate,YRC2.SpinnerID,YRM.SpinnerID,
	                    YRC2.LotNo,YRC2.ChallanLot,T.TechnicalName,RMC1.BuyerID,B.ShortName,C.Remarks,
	                    Z.YarnStatus,C.Approve,C.Reject,C.Retest,C.Diagnostic,C.CommerciallyApprove, RCM.QCReceiveNo, 
		                RM.QCReqNo, RCM.QCReceiveNo, QCIM.QCIssueNo,C.CommerciallyApprove   ,M.ApprovedDate
                    ),
                    FinalList AS
                    (
	                    SELECT FL.*,Spinner = CASE WHEN FL.SpinnerID > 0 THEN CCS.ShortName ELSE '' END
	                    FROM YQCRemarks FL
	                    LEFT JOIN {DbNames.EPYSL}..Contacts CCS ON CCS.ContactID = FL.SpinnerID
                    )
                    Select *, Count(*) Over() TotalRows From FinalList";
                    break;
                case Status.All:
                    sql = $@"
                    WITH 
                    YQCRemarks As 
                    (
	                    SELECT 
	                    YarnDetail = YRC.YarnCategory, YRM.ReceiveDate, YRC.LotNo, TechnicalName = T.TechnicalName,
	                    BuyerName = CASE WHEN RC.BuyerID > 0 THEN B.ShortName ELSE '' END, C.Remarks,
	                    YRM.ReceiveNo, RCM.QCReceiveNo, QCIM.QCIssueNo, RM.QCReqNo,
	                    YarnStatus = Z.YarnStatus,YRC.ChallanLot,
	                    SpinnerID = CASE WHEN YRC.SpinnerID > 0 THEN YRC.SpinnerID ELSE YRM.SpinnerID END,
                        QCRemarksNo = ISNULL(M.QCRemarksNo,''),
	                    QCRemarksMasterID = ISNULL(M.QCRemarksMasterID,0),QCReceiveMasterID = ISNULL(RCM.QCReceiveMasterID,0),
	                    QCIssueMasterID = ISNULL(QCIM.QCIssueMasterID,0), QCReqMasterID = ISNULL(RM.QCReqMasterID,0), ReceiveID = ISNULL(YRM.ReceiveID,0),

	                    [Status] = CASE WHEN ISNULL(YRC.IsNoTest,0) = 1 THEN 'No Test'
	                    WHEN ISNULL(M.QCRemarksMasterID,0) > 0 AND ISNULL(C.Approve,0) = 1 THEN 'Assessment Approved'
	                    WHEN ISNULL(M.QCRemarksMasterID,0) > 0 AND ISNULL(C.Diagnostic,0) = 1 THEN 'Assessment Diagonostic' 
	                    WHEN ISNULL(M.QCRemarksMasterID,0) > 0 AND ISNULL(C.Reject,0) = 1 THEN 'Assessment Rejected' 
	                    WHEN ISNULL(M.QCRemarksMasterID,0) > 0 AND ISNULL(C.ReTest,0) = 1 THEN 'Waiting For Re-test' 
	                    WHEN ISNULL(RCM.QCReceiveMasterID,0) > 0 THEN 'Waiting for Yarn Assessment'
	                    WHEN ISNULL(QCIM.QCIssueMasterID,0) > 0 THEN 'Waiting for Yarn Receive'
	                    WHEN ISNULL(RM.IsApprove,0) = 1 THEN 'Waiting for Yarn Issue'
	                    WHEN ISNULL(RM.QCReqMasterID,0) > 0 THEN 'Waiting for Requisition Approval'
	                    ELSE 'Waiting for Requisition' END   ,M.ApprovedDate

	                    From {TableNames.YARN_QC_REQ_CHILD} RC
		                INNER JOIN {TableNames.YARN_QC_RECEIVE_MASTER} RM On RM.QCReqMasterID = RC.QCReqMasterID

		                LEFT JOIN {TableNames.YARN_QC_ISSUE_CHILD} QCIC ON QCIC.QCReqChildID = RC.QCReqChildID
		                LEFT JOIN {TableNames.YARN_QC_ISSUE_MASTER} QCIM ON QCIM.QCIssueMasterID = QCIC.QCIssueMasterID

		                LEFT JOIN {TableNames.YARN_QC_RECEIVE_CHILD} QCRC ON QCRC.QCIssueChildID = QCIC.QCIssueChildID
		                LEFT JOIN {TableNames.YARN_QC_RECEIVE_MASTER} RCM On RCM.QCReceiveMasterID = QCRC.QCReceiveMasterID

		                LEFT JOIN {TableNames.YARN_QC_REMARKS_CHILD} C ON C.QCReceiveChildID = QCRC.QCReceiveChildID
		                LEFT JOIN {TableNames.YARN_QC_REMARKS_MASTER} M On M.QCRemarksMasterID = C.QCRemarksMasterID

		                LEFT JOIN {TableNames.YARN_RECEIVE_CHILD} YRC ON YRC.ChildID = RC.ReceiveChildID
		                LEFT JOIN {TableNames.YARN_RECEIVE_MASTER} YRM ON YRM.ReceiveID = YRC.ReceiveID
	
	                    LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME} T ON T.TechnicalNameId = RC.TechnicalNameId
	                    LEFT JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = RC.BuyerID
	                    LEFT JOIN {TableNames.YarnAssessmentStatus} Z ON Z.YarnStatusID = C.YarnStatusID

                        INNER JOIN {TableNames.YARN_RECEIVE_CHILD_RACK_BIN} RB ON RB.ChildID = YRC.ChildID
                    ),
                    FinalList AS
                    (
	                    SELECT FL.*,Spinner = CASE WHEN FL.SpinnerID > 0 THEN CCS.ShortName ELSE '' END
	                    FROM YQCRemarks FL
	                    LEFT JOIN {DbNames.EPYSL}..Contacts CCS ON CCS.ContactID = FL.SpinnerID
                    )
                    Select *, Count(*) Over() TotalRows From FinalList
                    ";

                    orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "ORDER BY ReceiveDate DESC, QCRemarksMasterID DESC,QCReceiveMasterID DESC,QCIssueMasterID DESC,QCReqMasterID DESC,ReceiveID DESC" : paginationInfo.OrderBy;
                    break;
                case Status.Reject:
                    sql = $@"
                        With 
                        C As (
	                        Select * From {TableNames.YARN_QC_REMARKS_CHILD}
	                        Where Reject = 1
                        ), 
                        YQCRemarks As 
                        (
	                        Select C.QCRemarksChildID, M.QCRemarksMasterID, M.QCRemarksNo, M.QCRemarksDate, ER.EmployeeName QCRemarksByUser
	                        , M.QCReceiveMasterID, M.QCIssueMasterID, M.QCReqMasterID, ERCU.EmployeeName QCReceiveByUser, QCReqFor.ValueName QCReqFor
	                        , SUM(C.ReceiveQty) ReceiveQty, SUM(C.ReceiveQtyCone) ReceiveQtyCone, SUM(C.ReceiveQtyCarton) ReceiveQtyCarton,
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

	                        YRM.ReceiveNo, YRM.ReceiveDate,
	                        SpinnerID = CASE WHEN YRC2.SpinnerID > 0 THEN YRC2.SpinnerID ELSE YRM.SpinnerID END,
	                        YRC2.LotNo, YRC2.ChallanLot,
	                        YarnDetail = CONCAT(ISV6.SegmentValue,' ', ISV1.SegmentValue,' ', ISV3.SegmentValue,' ', ISV4.SegmentValue,' ', ISV2.SegmentValue,' ', ISV5.SegmentValue,' ', C.ShadeCode),
	                        TechnicalName = T.TechnicalName,
	                        BuyerName = CASE WHEN RMC1.BuyerID > 0 THEN B.ShortName ELSE '' END

	                        From C
                            Inner Join {TableNames.YARN_QC_REMARKS_MASTER} M On M.QCRemarksMasterID = C.QCRemarksMasterID
		                    INNER JOIN {TableNames.YARN_QC_RECEIVE_CHILD} QCRC ON QCRC.QCReceiveChildID = C.QCReceiveChildID
	                        Inner Join {TableNames.YARN_QC_RECEIVE_MASTER} RCM On RCM.QCReceiveMasterID = QCRC.QCReceiveMasterID
		                    INNER JOIN {TableNames.YARN_QC_ISSUE_CHILD} QCIC ON QCIC.QCIssueChildID = QCRC.QCIssueChildID
		                    INNER JOIN {TableNames.YARN_QC_ISSUE_MASTER} QCIM ON QCIM.QCIssueMasterID = QCIC.QCIssueMasterID
	                        Inner Join {TableNames.YARN_QC_REQ_CHILD} RMC1 On RMC1.QCReqChildID = QCIC.QCReqChildID
	                        Inner Join {TableNames.YARN_QC_REQ_MASTER} RM On RM.QCReqMasterID = RMC1.QCReqMasterID
	                        INNER JOIN {TableNames.YARN_RECEIVE_CHILD} YRC2 ON YRC2.ChildID = RMC1.ReceiveChildID
	                        INNER JOIN {TableNames.YARN_RECEIVE_MASTER} YRM ON YRM.ReceiveID = YRC2.ReceiveID
	                        Left Join {DbNames.EPYSL}..EntityTypeValue QCReqFor On RM.QCForID = QCReqFor.ValueID
	                        Inner Join {DbNames.EPYSL}..LoginUser RU On M.QCRemarksBy = RU.UserCode 
					        Inner Join {DbNames.EPYSL}..Employee ER On ER.EmployeeCode = RU.EmployeeCode
	                        Inner Join {DbNames.EPYSL}..LoginUser RCU On RCM.QCReceivedBy = RCU.UserCode
					        Inner Join {DbNames.EPYSL}..Employee ERCU On ERCU.EmployeeCode = RCU.EmployeeCode
	                        LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME} T ON T.TechnicalNameId = RMC1.TechnicalNameId
	                        LEFT JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = RMC1.BuyerID
	                        LEFT JOIN {TableNames.YarnAssessmentStatus} Z ON Z.YarnStatusID = C.YarnStatusID
	                        Inner Join {DbNames.EPYSL}..ItemMaster IM On IM.ItemMasterID = C.ItemMasterID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                            WHERE M.IsApproved = 1
	                        Group By C.QCRemarksChildID, M.QCRemarksMasterID, M.QCRemarksNo, M.QCRemarksDate, ER.EmployeeName,
	                        M.QCReceiveMasterID, M.QCIssueMasterID, M.QCReqMasterID, ERCU.EmployeeName, QCReqFor.ValueName,
	                        ISV1.SegmentValue,ISV2.SegmentValue, ISV3.SegmentValue,
	                        ISV4.SegmentValue, ISV5.SegmentValue,ISV6.SegmentValue,
	                        ISV7.SegmentValue,C.ShadeCode,YRM.ReceiveNo, YRM.ReceiveDate,YRC2.SpinnerID,YRM.SpinnerID,
	                        YRC2.LotNo, YRC2.ChallanLot,T.TechnicalName,RMC1.BuyerID,B.ShortName,C.Remarks,
	                        Z.YarnStatus,C.Approve,C.Reject,C.Retest,C.Diagnostic,C.CommerciallyApprove, RCM.QCReceiveNo, 
		                    RM.QCReqNo, RCM.QCReceiveNo, QCIM.QCIssueNo
                        ),
                        FinalList AS
                        (
	                        SELECT FL.*,Spinner = CASE WHEN FL.SpinnerID > 0 THEN CCS.ShortName ELSE '' END
	                        FROM YQCRemarks FL
	                        LEFT JOIN {DbNames.EPYSL}..Contacts CCS ON CCS.ContactID = FL.SpinnerID
                        )
                        Select *, Count(*) Over() TotalRows From FinalList";
                    break;
                default:
                    sql = $@"
                       WITH FL AS 
                        (
	                        SELECT RC.QCReceiveChildID, M.QCReceiveMasterID, M.QCReceiveNo, M.QCReceiveDate, QCReceivedByUser = RU.Name,
	                        M.QCIssueMasterID, QCIM.QCIssueNo, QCIM.QCIssueDate, IU.Name QCIssueByUser, RM.QCReqNo, RM.QCReqDate,
	                        QCReqFor = QCReqFor.ValueName,
	                        ISV1.SegmentValue Segment1ValueDesc,ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc,
	                        ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc,ISV6.SegmentValue Segment6ValueDesc,
	                        ISV7.SegmentValue Segment7ValueDesc,

	                        YRM.ReceiveNo, YRM.ReceiveDate,
	                        SpinnerID = CASE WHEN YRC.SpinnerID > 0 THEN YRC.SpinnerID ELSE YRM.SpinnerID END,
	                        YRC.LotNo, YRC.ChallanLot,
	                        YarnDetail = CONCAT(ISV6.SegmentValue,' ', ISV1.SegmentValue,' ', ISV3.SegmentValue,' ', ISV4.SegmentValue,' ', ISV2.SegmentValue,' ', ISV5.SegmentValue,' ', RC.ShadeCode),
	                        TechnicalName = T.TechnicalName,
	                        BuyerName = CASE WHEN RMC1.BuyerID > 0 THEN B.ShortName ELSE '' END

	                        FROM {TableNames.YARN_QC_RECEIVE_CHILD} RC
	                        INNER JOIN {TableNames.YARN_QC_RECEIVE_MASTER} M ON M.QCReceiveMasterID = RC.QCReceiveMasterID
	                        INNER JOIN {TableNames.YARN_QC_ISSUE_CHILD} QCIC On QCIC.QCIssueChildID = RC.QCIssueChildID
	                        INNER JOIN {TableNames.YARN_QC_ISSUE_MASTER} QCIM On QCIM.QCIssueMasterID = QCIC.QCIssueMasterID
	                        INNER JOIN {TableNames.YARN_QC_REQ_CHILD} RMC1 ON RMC1.QCReqChildID = QCIC.QCReqChildID
	                        INNER JOIN {TableNames.YARN_QC_REQ_MASTER} RM On RM.QCReqMasterID = RMC1.QCReqMasterID
	                        LEFT JOIN {TableNames.YARN_RECEIVE_CHILD} YRC On YRC.ChildID = RC.ReceiveChildID
	                        LEFT JOIN {TableNames.YARN_RECEIVE_MASTER} YRM On YRM.ReceiveID = YRC.ReceiveID
	                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue QCReqFor On RM.QCForID = QCReqFor.ValueID
	                        INNER JOIN {DbNames.EPYSL}..LoginUser RU On M.QCReceivedBy = RU.UserCode 
	                        INNER JOIN {DbNames.EPYSL}..LoginUser IU On QCIM.QCIssueBy = IU.UserCode
	                        LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME} T ON T.TechnicalNameId = RMC1.TechnicalNameId
	                        LEFT JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = RMC1.BuyerID

	                        INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = RC.ItemMasterID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID

	                        LEFT JOIN {TableNames.YARN_QC_REMARKS_CHILD} RMC ON RMC.QCReceiveChildID = RC.QCReceiveChildID
	                        WHERE RMC.QCRemarksChildID IS NULL
                        ),
                        FinalList AS
                        (
	                        SELECT FL.*,Spinner = CASE WHEN FL.SpinnerID > 0 THEN CCS.ShortName ELSE '' END
	                        FROM FL
	                        LEFT JOIN {DbNames.EPYSL}..Contacts CCS ON CCS.ContactID = FL.SpinnerID
                        )
                        Select *, Count(*) Over() TotalRows FROM FinalList";

                    orderBy = "ORDER BY QCReceiveChildID DESC";
                    break;
            }

            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<YarnQCRemarksMaster>(sql);
        }
        public async Task<YarnQCRemarksMaster> GetNew2Async(int qcReceiveChildID)
        {
            var query =
                $@"
                -- Master Data
                With 
                RC AS
                (
	                SELECT C.QCReceiveMasterID, C.QCReceiveChildID
	                FROM {TableNames.YARN_QC_RECEIVE_CHILD} C
	                WHERE C.QCReceiveChildID = {qcReceiveChildID}
                ),
                M As (
	                Select M.* 
	                From {TableNames.YARN_QC_RECEIVE_MASTER} M
	                INNER JOIN RC ON RC.QCReceiveMasterID = M.QCReceiveMasterID 
                )
                Select M.QCReceiveMasterID, M.QCReceiveNo, M.QCReceiveDate, M.QCReqMasterID, M.QCIssueMasterID, M.LocationId, M.ReceiveID, M.CompanyId, M.RCompanyId, M.SupplierId, M.SpinnerId
                    , QCReqFor.ValueName QCReqFor, RM.QCReqDate, RM.QCReqNo
                From M
                Inner Join {TableNames.YARN_QC_REQ_MASTER} RM On M.QCReqMasterID = RM.QCReqMasterID
                Left Join {DbNames.EPYSL}..EntityTypeValue QCReqFor On RM.QCForID = QCReqFor.ValueID;

                --Technical Name
                SELECT Cast(T.TechnicalNameId As varchar) id, T.TechnicalName [text]
                FROM {TableNames.FABRIC_TECHNICAL_NAME} T;

                --Dyeing Process List
                SELECT Cast(D.DPID as varchar) id,D.DPName [text]
                FROM {TableNames.DyeingProcessPart_HK} D;

                -- Child Data
                With
                C As (
	                Select QCRemarksChildID = RCVC.QCReceiveChildID, RCVC.QCReceiveChildID, RCVC.ReceiveChildID, RCVC.YarnProgramId,  RCVC.ChallanCount, RCVC.POCount, RCVC.PhysicalCount, RMC.YarnCategory, RCVC.NoOfThread, RCVC.UnitID,
	                RCVC.ItemMasterID, RCVC.ReceiveQty, RCVC.ReceiveQtyCone, RCVC.ReceiveQtyCarton, RCVC.ShadeCode,RCVC.LotNo,RCVC.ChallanLot,
	                RMC.NoOfCartoon, RMC.NoOfCone, ReceiveQtyYS = RMC.ReceiveQty,

	                ReceiveDate = RM.ReceiveDate,
	                SpinnerID =  CASE WHEN RMC.SpinnerID > 0 THEN RMC.SpinnerID ELSE RMM2.SpinnerID END,
	                Spinner = CASE WHEN RMC.SpinnerID > 0 THEN CCS.ShortName ELSE CCSM.ShortName END,
	                SupplierName = CC.ShortName,
	                TechnicalName = T.TechnicalName,
                    
	                BuyerName = CASE WHEN RMC2.BuyerID > 0 THEN B.ShortName ELSE '' END,RMC.Rate

	                From {TableNames.YARN_QC_RECEIVE_CHILD} RCVC
	                INNER JOIN {TableNames.YARN_QC_ISSUE_CHILD} RIC ON RIC.QCIssueChildID = RCVC.QCIssueChildID
	                INNER JOIN {TableNames.YARN_QC_REQ_CHILD} RMC2 ON RMC2.QCReqChildID = RIC.QCReqChildID
	                INNER JOIN {TableNames.YARN_QC_REQ_MASTER} RMM2 ON RMM2.QCReqMasterID = RMC2.QCReqMasterID
	                INNER JOIN {TableNames.YARN_RECEIVE_CHILD} RMC ON RMC.ChildID = RMC2.ReceiveChildID
	                INNER JOIN {TableNames.YARN_RECEIVE_MASTER} RM ON RM.ReceiveID = RMC.ReceiveID
	                LEFT JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = RM.SupplierID
                    LEFT JOIN {DbNames.EPYSL}..Contacts CCS ON CCS.ContactID = RMC.SpinnerID
	                LEFT JOIN {DbNames.EPYSL}..Contacts CCSM ON CCSM.ContactID = RMM2.SpinnerID
	                LEFT JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = RMC2.BuyerID
	                LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME} T ON T.TechnicalNameId = RMC2.TechnicalNameId
	                Left Join {TableNames.YARN_QC_REMARKS_CHILD} RMKC ON RMKC.QCReceiveChildID = RCVC.QCReceiveChildID
	                Where RCVC.QCReceiveChildID = {qcReceiveChildID}
                )
                Select C.QCRemarksChildID, C.QCReceiveChildID, C.ReceiveChildID, C.YarnProgramId, C.ChallanCount, C.POCount, C.PhysicalCount, C.YarnCategory, C.NoOfThread, C.UnitID, C.ItemMasterID
                ,C.LotNo, C.ChallanLot, C.ReceiveQty, C.ReceiveQtyCone, C.ReceiveQtyCarton ReceiveQtyCarton, U.DisplayUnitDesc UOM
                ,C.NoOfCartoon, C.NoOfCone, C.ReceiveQtyYS

                ,ISNULL(ISV1.SegmentValue,'') Segment1ValueDesc
                ,ISNULL(ISV2.SegmentValue,'') Segment2ValueDesc
                ,ISNULL(ISV3.SegmentValue,'') Segment3ValueDesc
                ,ISNULL(ISV4.SegmentValue,'') Segment4ValueDesc
                ,ISNULL(ISV5.SegmentValue,'') Segment5ValueDesc
                ,ISNULL(ISV6.SegmentValue,'') Segment6ValueDesc
                ,ISNULL(ISV7.SegmentValue,'') Segment7ValueDesc
                ,ISNULL(ShadeCode,'') ShadeCode,
                C.ReceiveDate,
                Spinner = CASE WHEN C.SpinnerID > 0 THEN C.Spinner ELSE '' END,
                C.SupplierName,
                C.TechnicalName,C.BuyerName, 
                YarnDetail = CONCAT(ISV6.SegmentValue,' ', ISV1.SegmentValue,' ', ISV3.SegmentValue,' ', ISV4.SegmentValue,' ', ISV2.SegmentValue,' ', ISV5.SegmentValue,' ', C.ShadeCode),Rate
                From C
                Left Join {DbNames.EPYSL}..Unit U On C.UnitID = U.UnitID
                Inner Join {DbNames.EPYSL}..ItemMaster IM On C.ItemMasterID = IM.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID;

                -- YarnAssessmentStatus List
                Select Cast(ETV.YarnStatusID As varchar) [id], ETV.YarnStatus [text]
                From {TableNames.YarnAssessmentStatus} as ETV;

                -- YarnAssessmentStatus List
                {CommonQueries.GetYarnAssessmentStatus()}

                --TestParam
                SELECT Cast(TP.ResultTypeID as varchar) id,TP.ResultType [text],TP.ResultSet additionalValue
                FROM {TableNames.YarnTestParamResultSet_HK} TP;

                --Buyer List
                {CommonQueries.GetContactsByCategoryId(ContactCategoryConstants.CONTACT_CATEGORY_BUYER)};

                -- Fabric Components
                Select id = EV.ValueID, text = EV.ValueName
                From {DbNames.EPYSL}..EntityTypeValue EV
                Inner Join {DbNames.EPYSL}..EntityType ET On EV.EntityTypeID = ET.EntityTypeID
                Where ET.EntityTypeName = 'Fabric Type';

            ";


            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                YarnQCRemarksMaster data = await records.ReadFirstOrDefaultAsync<YarnQCRemarksMaster>();
                Guard.Against.NullObject(data);

                data.TechnicalNameList = await records.ReadAsync<Select2OptionModel>();
                data.DPList = await records.ReadAsync<Select2OptionModel>();
                data.YarnQCRemarksChilds = records.Read<YarnQCRemarksChild>().ToList();

                data.YarnAssessmentZoneList = await records.ReadAsync<Select2OptionModel>();
                data.YarnAssessmentStatusList = await records.ReadAsync<Select2OptionModel>();

                data.TestParamSetList = records.Read<Select2OptionModel>().ToList();
                if (data.TestParamSetList == null) data.TestParamSetList = new List<Select2OptionModel>();

                data.BuyerList = records.Read<Select2OptionModel>().ToList();
                data.BuyerList.Insert(0, new Select2OptionModel()
                {
                    id = 0.ToString(),
                    text = "Empty"
                });
                data.FabricComponents = records.Read<Select2OptionModel>().ToList();

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
        public async Task<YarnQCRemarksMaster> GetNewAsync(int qcReceiveMasterId)
        {
            var query =
                $@"
                -- Master Data
                With 
                M As (
	                Select * From {TableNames.YARN_QC_RECEIVE_MASTER}
	                Where QCReceiveMasterID = {qcReceiveMasterId}
                )

                Select M.QCReceiveMasterID, M.QCReceiveNo, M.QCReceiveDate, M.QCReqMasterID, M.QCIssueMasterID, M.LocationId, M.ReceiveID, M.CompanyId, M.RCompanyId, M.SupplierId, M.SpinnerId
                 , QCReqFor.ValueName QCReqFor, RM.QCReqDate, RM.QCReqNo
                From M
                Inner Join {TableNames.YARN_QC_REQ_MASTER} RM On M.QCReqMasterID = RM.QCReqMasterID
                Left Join {DbNames.EPYSL}..EntityTypeValue QCReqFor On RM.QCForID = QCReqFor.ValueID;




                -- Child Data
               With
                C As (
	                Select RCVC.QCReceiveChildID,RCVC.YarnProgramId,  RCVC.ChallanCount, RCVC.POCount, RCVC.PhysicalCount, RCVC.YarnCategory, RCVC.NoOfThread, RCVC.UnitID,
					RCVC.ItemMasterID, RCVC.ReceiveQty, RCVC.ReceiveQtyCone, RCVC.ReceiveQtyCarton, RCVC.ShadeCode,RCVC.LotNo,RCVC.ChallanLot,
	                From {TableNames.YARN_QC_RECEIVE_CHILD} RCVC
					Left Join {TableNames.YARN_QC_REMARKS_CHILD} RMKC ON RMKC.QCReceiveChildID = RCVC.QCReceiveChildID
	                Where RMKC.QCReceiveChildID IS NULL AND QCReceiveMasterID = {qcReceiveMasterId}
                )

                Select C.QCReceiveChildID,C.YarnProgramId, C.ChallanCount, C.POCount, C.PhysicalCount, C.YarnCategory, C.NoOfThread, C.UnitID, C.ItemMasterID
	                
	                , IM.ItemMasterID, C.LotNo, C.ChallanLot,
	                , C.ReceiveQty, C.ReceiveQtyCone, C.ReceiveQtyCarton ReceiveQtyCarton, U.DisplayUnitDesc UOM
					,ISV1.SegmentValue Segment1ValueDesc,ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc,
                ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc,ISV6.SegmentValue Segment6ValueDesc,
				ISV7.SegmentValue Segment7ValueDesc,C.ShadeCode
                From C
                LEFT Join {DbNames.EPYSL}..Unit U On C.UnitID = U.UnitID
                Inner Join {DbNames.EPYSL}..ItemMaster IM On C.ItemMasterID = IM.ItemMasterID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID;

                -- YarnAssessmentStatus List
                Select Cast(ETV.YarnStatusID As varchar) [id], ETV.YarnStatus [text]
                From {TableNames.YarnAssessmentStatus} as ETV;

                -- YarnAssessmentStatus List
                 {CommonQueries.GetYarnAssessmentStatus()}";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                YarnQCRemarksMaster data = await records.ReadFirstOrDefaultAsync<YarnQCRemarksMaster>();
                Guard.Against.NullObject(data);
                data.YarnQCRemarksChilds = records.Read<YarnQCRemarksChild>().ToList();
                data.YarnAssessmentZoneList = await records.ReadAsync<Select2OptionModel>();
                data.YarnAssessmentStatusList = await records.ReadAsync<Select2OptionModel>();
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
        public async Task<YarnQCRemarksMaster> Get2Async(int qcRemarksChildID)
        {
            var query =
                $@"
                With
                RC AS
                (
	                SELECT C.QCRemarksMasterID, C.QCRemarksChildID
	                FROM {TableNames.YARN_QC_REMARKS_CHILD} C
	                WHERE C.QCRemarksChildID = {qcRemarksChildID}
                ),
                M AS (
	                Select M.*,RC.QCRemarksChildID
	                From {TableNames.YARN_QC_REMARKS_MASTER} M
	                INNER JOIN RC ON RC.QCRemarksMasterID = M.QCRemarksMasterID
                )
                Select M.QCRemarksMasterID, M.QCReqMasterID, M.QCIssueMasterID, M.QCReceiveMasterID, M.QCRemarksNo, M.QCRemarksBy, M.QCRemarksDate
                    , M.QCRemarksChildID
	                , RM.QCReceiveNo, RM.QCReceiveDate, RQM.QCReqNo, RQM.QCReqDate
	                , M.LocationId, M.ReceiveID, M.CompanyId, M.RCompanyId, M.SupplierId, M.SpinnerId, Supplier.ShortName [Suupplier], Spinner.ShortName [Spinner]
                From  M
                Inner Join {TableNames.YARN_QC_RECEIVE_MASTER} RM On M.QCReceiveMasterID = RM.QCReceiveMasterID
                Inner Join {TableNames.YARN_QC_REQ_MASTER} RQM On M.QCReqMasterID = RQM.QCReqMasterID
                Inner Join {DbNames.EPYSL}..Contacts Supplier On M.SupplierID = Supplier.ContactID
                Inner Join {DbNames.EPYSL}..Contacts Spinner On M.SpinnerId = Spinner.ContactID;

                --Technical Name
                SELECT Cast(T.TechnicalNameId As varchar) id, T.TechnicalName [text]
                FROM {TableNames.FABRIC_TECHNICAL_NAME} T;

                --Dyeing Process List
                SELECT Cast(D.DPID as varchar) id,D.DPName [text]
                FROM {TableNames.DyeingProcessPart_HK} D;

                -- Childs
                With 
                YRC AS (
	                Select * 
                    From {TableNames.YARN_QC_REMARKS_CHILD}
	                Where QCRemarksChildID = {qcRemarksChildID}
                )

                Select YRC.YarnStatusID,YRC.Approve,YRC.Reject,YRC.ReTest,YRC.Diagnostic,YRC.CommerciallyApprove, YRC.QCRemarksChildID, YRC.QCReceiveChildID, YRC.ReceiveChildID, YRC.YarnProgramId, YRC.ChallanCount, YRC.POCount, YRC.PhysicalCount, YRC.YarnCategory
                , YRC.NoOfThread, YRC.UnitID, YRC.ItemMasterID,YRC.Remarks
                ,ISV1.SegmentValue Segment1ValueDesc,ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc,
                ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc,ISV6.SegmentValue Segment6ValueDesc,
                ISV7.SegmentValue Segment7ValueDesc,YRC.ShadeCode,
                IM.ItemMasterID, YRC.LotNo, YRC.ChallanLot, YRC.ReceiveQty, YRC.ReceiveQtyCone, YRC.ReceiveQtyCarton, U.DisplayUnitDesc UOM,
                ReceiveDate = RM.ReceiveDate,
                SpinnerID =  CASE WHEN RMC.SpinnerID > 0 THEN RMC.SpinnerID ELSE RMM2.SpinnerID END,
                Spinner = CASE WHEN RMC.SpinnerID > 0 THEN CCS.ShortName ELSE CASE WHEN RMM2.SpinnerID = 0 THEN '' ELSE CCSM.ShortName END END,
                SupplierName = CC.ShortName,
                YarnDetail = CONCAT(ISV6.SegmentValue,' ', ISV1.SegmentValue,' ', ISV3.SegmentValue,' ', ISV4.SegmentValue,' ', ISV2.SegmentValue,' ', ISV5.SegmentValue,' ', YRC.ShadeCode),
                TechnicalName = T.TechnicalName,TechnicalNameID=T.TechnicalNameID,
                BuyerName = CASE WHEN RMC2.BuyerID > 0 THEN B.ShortName ELSE '' END,
                RMC.NoOfCartoon,RMC.NoOfCone,ReceiveQtyYS = RMC.ReceiveQty
                From YRC
                INNER JOIN {TableNames.YARN_QC_RECEIVE_CHILD} RC1 ON RC1.QCReceiveChildID = YRC.QCReceiveChildID
                INNER JOIN {TableNames.YARN_QC_ISSUE_CHILD} RIC ON RIC.QCIssueChildID = RC1.QCIssueChildID
                INNER JOIN {TableNames.YARN_QC_REQ_CHILD} RMC2 ON RMC2.QCReqChildID = RIC.QCReqChildID
                INNER JOIN {TableNames.YARN_QC_REQ_MASTER} RMM2 ON RMM2.QCReqMasterID = RMC2.QCReqMasterID
                INNER JOIN {TableNames.YARN_RECEIVE_CHILD} RMC ON RMC.ChildID = RMC2.ReceiveChildID
                INNER JOIN {TableNames.YARN_RECEIVE_MASTER} RM ON RM.ReceiveID = RMC.ReceiveID
                LEFT JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = RM.SupplierID
                LEFT JOIN {DbNames.EPYSL}..Contacts CCS ON CCS.ContactID = RMC.SpinnerID
                LEFT JOIN {DbNames.EPYSL}..Contacts CCSM ON CCSM.ContactID = RMM2.SpinnerID
                LEFT JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = RMC2.BuyerID
                LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME} T ON T.TechnicalNameId = YRC.TechnicalNameId
                LEFT Join {DbNames.EPYSL}..Unit U On YRC.UnitID = U.UnitID
                Inner Join {DbNames.EPYSL}..ItemMaster IM On YRC.ItemMasterID = IM.ItemMasterID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID;
                
                --ChildResult
                Select YCR.*, ColorName =  ISV.SegmentValue, TechnicalName=FTN.TechnicalName
                From {TableNames.YARN_QC_REMARKS_CHILDRESULT} YCR
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV On ISV.SegmentValueID = YCR.FabricColorID
                LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME} FTN On FTN.TechnicalNameId=YCR.TechnicalNameID
                Where YCR.QCRemarksChildID= {qcRemarksChildID};

                --ChildFiber
                Select YQRF.*,id=EV.ValueID,text=EV.ValueName
				From {TableNames.YARN_QC_REMARKS_CHILDFIBER} YQRF
				Left Join {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID=YQRF.ComponentID
                Where YQRF.QCRemarksChildID= {qcRemarksChildID};

                -- YarnAssessmentZone List
                Select Cast(YarnStatusID As varchar) [id],YarnStatus [text]
                From {TableNames.YarnAssessmentStatus};

                --YarnAssessmentStatus List
                {CommonQueries.GetYarnAssessmentStatus()}

                --TestParam
                SELECT Cast(TP.ResultTypeID as varchar) id,TP.ResultType [text],TP.ResultSet additionalValue
                FROM {TableNames.YarnTestParamResultSet_HK} TP;

                --Buyer List
                {CommonQueries.GetContactsByCategoryId(ContactCategoryConstants.CONTACT_CATEGORY_BUYER)};

                -- Fabric Components
                Select id = EV.ValueID, text = EV.ValueName
                From {DbNames.EPYSL}..EntityTypeValue EV
                Inner Join {DbNames.EPYSL}..EntityType ET On EV.EntityTypeID = ET.EntityTypeID
                Where ET.EntityTypeName = 'Fabric Type';
                ";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                YarnQCRemarksMaster data = await records.ReadFirstOrDefaultAsync<YarnQCRemarksMaster>();
                Guard.Against.NullObject(data);
                data.TechnicalNameList = await records.ReadAsync<Select2OptionModel>();
                data.DPList = await records.ReadAsync<Select2OptionModel>();
                data.YarnQCRemarksChilds = records.Read<YarnQCRemarksChild>().ToList();
                var yarnQCRemarksChildResults = records.Read<YarnQCRemarksChildResult>().ToList();
                var yarnQCRemarksChildFibers = records.Read<YarnQCRemarksChildFiber>().ToList();
                data.YarnQCRemarksChilds.ForEach(c =>
                {
                    if (c.Approve) c.id = "Approve";
                    //else if (c.Reject) c.id = "Reject";
                    else if (c.ReTest) c.id = "ReTest";
                    else if (c.Diagnostic) c.id = "Diagnostic";
                    else if (c.CommerciallyApprove) c.id = "CommerciallyApprove";

                    c.YarnQCRemarksChildResults = yarnQCRemarksChildResults.Where(x => x.QCRemarksChildID == c.QCRemarksChildID).ToList();
                    c.YarnQCRemarksChildFibers = yarnQCRemarksChildFibers.Where(x => x.QCRemarksChildID == c.QCRemarksChildID).ToList();
                });
                data.YarnAssessmentZoneList = await records.ReadAsync<Select2OptionModel>();
                data.YarnAssessmentStatusList = await records.ReadAsync<Select2OptionModel>();
                data.TestParamSetList = records.Read<Select2OptionModel>().ToList();
                if (data.TestParamSetList == null) data.TestParamSetList = new List<Select2OptionModel>();

                data.BuyerList = records.Read<Select2OptionModel>().ToList();
                data.BuyerList.Insert(0, new Select2OptionModel()
                {
                    id = 0.ToString(),
                    text = "Empty"
                });
                data.FabricComponents = records.Read<Select2OptionModel>().ToList();

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
        public async Task<YarnQCRemarksMaster> GetAsync(int id)
        {
            var query =
                $@"
                Select M.*,
	                RM.QCReceiveNo, RM.QCReceiveDate, RQM.QCReqNo, RQM.QCReqDate,
	                Supplier.ShortName [Suupplier], Spinner.ShortName [Spinner]
                From {TableNames.YARN_QC_REMARKS_MASTER} M
                Inner Join {TableNames.YARN_QC_RECEIVE_MASTER} RM On M.QCReceiveMasterID = RM.QCReceiveMasterID
                Inner Join {TableNames.YARN_QC_REQ_MASTER} RQM On M.QCReqMasterID = RQM.QCReqMasterID
                Inner Join {DbNames.EPYSL}..Contacts Supplier On M.SupplierID = Supplier.ContactID
                Inner Join {DbNames.EPYSL}..Contacts Spinner On M.SpinnerId = Spinner.ContactID
                 Where QCRemarksMasterID = {id};

                -- Childs
                With 
                YRC AS (
	                Select * From {TableNames.YARN_QC_REMARKS_CHILD}
	                Where QCRemarksMasterID = {id} 
                )

                Select YRC.YarnStatusID,YRC.Approve,YRC.Reject,YRC.ReTest,YRC.Diagnostic, YRC.QCRemarksChildID, YRC.YarnProgramId, YRC.ChallanCount, YRC.POCount, YRC.PhysicalCount, YRC.YarnCategory
                , YRC.NoOfThread, YRC.UnitID, YRC.ItemMasterID,YRC.Remarks
                ,ISV1.SegmentValue Segment1ValueDesc,ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc,
                ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc,ISV6.SegmentValue Segment6ValueDesc,
                ISV7.SegmentValue Segment7ValueDesc,YRC.ShadeCode,
                IM.ItemMasterID, YRC.LotNo, YRC.ChallanLot,
                YRC.ReceiveQty, YRC.ReceiveQtyCone, YRC.ReceiveQtyCarton, U.DisplayUnitDesc UOM
                From YRC
                LEFT Join {DbNames.EPYSL}..Unit U On YRC.UnitID = U.UnitID
                Inner Join {DbNames.EPYSL}..ItemMaster IM On YRC.ItemMasterID = IM.ItemMasterID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID;
                
                -- YarnAssessmentZone List
                Select Cast(YarnStatusID As varchar) [id],YarnStatus [text]
                From {TableNames.YarnAssessmentStatus};

               -- YarnAssessmentStatus List
                {CommonQueries.GetYarnAssessmentStatus()}
            ";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                YarnQCRemarksMaster data = await records.ReadFirstOrDefaultAsync<YarnQCRemarksMaster>();
                Guard.Against.NullObject(data);
                data.YarnQCRemarksChilds = records.Read<YarnQCRemarksChild>().ToList();
                data.YarnQCRemarksChilds.ForEach(c =>
                {
                    if (c.Approve) c.id = "Approve";
                    //else if (c.Reject) c.id = "Reject";
                    else if (c.ReTest) c.id = "ReTest";
                    else if (c.Diagnostic) c.id = "Diagnostic";
                    else if (c.CommerciallyApprove) c.id = "CommerciallyApprove";
                });
                data.YarnAssessmentZoneList = await records.ReadAsync<Select2OptionModel>();
                data.YarnAssessmentStatusList = await records.ReadAsync<Select2OptionModel>();
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

        public async Task<YarnQCRemarksMaster> GetAllAsync(int id)
        {
            var sql = $@"
            ;Select * From {TableNames.YARN_QC_REMARKS_MASTER} Where QCRemarksMasterID = {id}

            ;Select * From {TableNames.YARN_QC_REMARKS_CHILD} Where QCRemarksMasterID = {id}

            ;Select YRCR.* 
            From {TableNames.YARN_QC_REMARKS_CHILDRESULT} YRCR 
            Inner Join {TableNames.YARN_QC_REMARKS_CHILD} YRC 
            ON YRC.QCRemarksChildID = YRCR.QCRemarksChildID 
            Where YRC.QCRemarksMasterID = {id}
            
            ;Select YRCF.*
			From {TableNames.YARN_QC_REMARKS_CHILDFIBER} YRCF
			Inner Join {TableNames.YARN_QC_REMARKS_CHILD} YRC 
            ON YRC.QCRemarksChildID = YRCF.QCRemarksChildID 
			Where YRC.QCRemarksMasterID = {id}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YarnQCRemarksMaster data = await records.ReadFirstOrDefaultAsync<YarnQCRemarksMaster>();
                Guard.Against.NullObject(data);
                data.YarnQCRemarksChilds = records.Read<YarnQCRemarksChild>().ToList();
                var yarnQCRemarksChildResults = records.Read<YarnQCRemarksChildResult>().ToList();
                var yarnQCRemarksChildFibers = records.Read<YarnQCRemarksChildFiber>().ToList();
                data.YarnQCRemarksChilds.ForEach(c =>
                {
                    c.YarnQCRemarksChildResults = yarnQCRemarksChildResults.Where(x => x.QCRemarksChildID == c.QCRemarksChildID).ToList();
                    c.YarnQCRemarksChildFibers = yarnQCRemarksChildFibers.Where(x => x.QCRemarksChildID == c.QCRemarksChildID).ToList();
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

        public async Task SaveAsync(YarnQCRemarksMaster entity)
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
                int maxChildResultId = 0;
                int maxChildFiberId = 0;

                int countChildResult = 0;
                int countChildFiber = 0;

                entity.YarnQCRemarksChilds.ForEach(x =>
                {
                    countChildResult += x.YarnQCRemarksChildResults.Where(c => c.EntityState == EntityState.Added).Count();
                    countChildFiber += x.YarnQCRemarksChildFibers.Where(c => c.EntityState == EntityState.Added).Count();
                });
                switch (entity.EntityState)
                {
                    case EntityState.Added:
                        entity.QCRemarksMasterID = await _service.GetMaxIdAsync(TableNames.YARN_QC_REMARKS_MASTER, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        entity.QCRemarksNo = await _service.GetMaxNoAsync(TableNames.YARN_QC_REMARKS_NO, 1, RepeatAfterEnum.NoRepeat, "00000", transactionGmt, _connectionGmt);


                        maxChildId = await _service.GetMaxIdAsync(TableNames.YARN_QC_REMARKS_CHILD, entity.YarnQCRemarksChilds.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        maxChildResultId = await _service.GetMaxIdAsync(TableNames.YARN_QC_REMARKS_CHILDRESULT, countChildResult, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt); // new 3/13/2023
                        maxChildFiberId = await _service.GetMaxIdAsync(TableNames.YARN_QC_REMARKS_CHILDFIBER, countChildFiber, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

                        foreach (YarnQCRemarksChild item in entity.YarnQCRemarksChilds)
                        {
                            item.QCRemarksChildID = maxChildId++;
                            item.QCRemarksMasterId = entity.QCRemarksMasterID;

                            foreach (YarnQCRemarksChildResult itemResult in item.YarnQCRemarksChildResults)
                            {
                                itemResult.QCRemarksChildResultID = maxChildResultId++;
                                itemResult.QCRemarksChildID = item.QCRemarksChildID;
                            }
                            foreach (YarnQCRemarksChildFiber itemResult in item.YarnQCRemarksChildFibers)
                            {
                                itemResult.QCRemarksChildFiberID = maxChildFiberId++;
                                itemResult.QCRemarksChildID = item.QCRemarksChildID;
                            }
                        }
                        break;

                    case EntityState.Modified:

                        maxChildId = await _service.GetMaxIdAsync(TableNames.YARN_QC_REMARKS_CHILD, entity.YarnQCRemarksChilds.Where(x => x.EntityState == EntityState.Added).Count(), RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        maxChildResultId = await _service.GetMaxIdAsync(TableNames.YARN_QC_REMARKS_CHILDRESULT, countChildResult, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt); // new 3/13/2023
                        maxChildFiberId = await _service.GetMaxIdAsync(TableNames.YARN_QC_REMARKS_CHILDFIBER, countChildFiber, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

                        foreach (YarnQCRemarksChild item in entity.YarnQCRemarksChilds)
                        {
                            if (item.EntityState == EntityState.Added)
                            {
                                item.QCRemarksChildID = maxChildId++;
                            }
                            item.QCRemarksMasterId = entity.QCRemarksMasterID;

                            foreach (YarnQCRemarksChildResult itemResult in item.YarnQCRemarksChildResults.Where(x => x.EntityState == EntityState.Added).ToList())
                            {
                                itemResult.QCRemarksChildResultID = maxChildResultId++;
                                itemResult.QCRemarksChildID = item.QCRemarksChildID;
                            }
                            foreach (YarnQCRemarksChildFiber itemResult in item.YarnQCRemarksChildFibers.Where(x => x.EntityState == EntityState.Added).ToList())
                            {
                                itemResult.QCRemarksChildFiberID = maxChildFiberId++;
                                itemResult.QCRemarksChildID = item.QCRemarksChildID;
                            }
                        }
                        break;

                    case EntityState.Unchanged:
                    case EntityState.Deleted:
                        entity.EntityState = EntityState.Deleted;
                        entity.YarnQCRemarksChilds.SetDeleted();
                        break;

                    default:
                        break;
                }
                //

                List<YarnQCRemarksChildResult> results = new List<YarnQCRemarksChildResult>();
                List<YarnQCRemarksChildFiber> fibers = new List<YarnQCRemarksChildFiber>();

                entity.YarnQCRemarksChilds.ForEach(x =>
                {
                    results.AddRange(x.YarnQCRemarksChildResults);
                    fibers.AddRange(x.YarnQCRemarksChildFibers);
                });

                await _service.SaveSingleAsync(entity, transaction);
                await _service.SaveAsync(entity.YarnQCRemarksChilds, transaction);
                await _service.SaveAsync(results, transaction);
                await _service.SaveAsync(fibers, transaction);

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

        public async Task ApproveAsync(YarnQCRemarksMaster entity)
        {
            SqlTransaction transaction = null;
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                await _service.SaveSingleAsync(entity, transaction);
                await _service.SaveAsync(entity.YarnQCRemarksChilds, transaction);

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
