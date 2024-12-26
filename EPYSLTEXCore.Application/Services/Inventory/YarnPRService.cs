using Dapper;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Application.Interfaces;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Entity;

namespace EPYSLTEX.Infrastructure.Services
{
    public class YarnPRService : IYarnPRService
    {
        private readonly IDapperCRUDService<YarnPRMaster> _service;

        private readonly SqlConnection _connection;
        private readonly SqlConnection _connectionGmt;

        public YarnPRService(IDapperCRUDService<YarnPRMaster> service
            , IDapperCRUDService<YarnPRChild> itemMasterRepository)
        {
            _service = service;

            _service.Connection = service.GetConnection(AppConstants.GMT_CONNECTION);
            _connectionGmt = service.Connection;

            _service.Connection = service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _connection = service.Connection;
        }

        public async Task<List<YarnPRMaster>> GetPagedAsync(Status status, string pageName, PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By YarnPRMasterID Desc" : paginationInfo.OrderBy;
            string sql;

            #region CPR Page

            if (pageName == PageNames.CPR)
            {
                if (status == Status.Pending)
                {
                    sql =
                    $@"WITH M AS (
	                    SELECT YarnPRMasterID, YarnPRDate ,YarnPRNo, YarnPRRequiredDate, L.Name [YarnPRByUser], A.Name[YpApproveBy],
                        R.Name[YpRejectBy], EV.ValueName TriggerPoint, Remarks, SendForApproval, Approve, ApproveBy,
                        ApproveDate, Reject, RejectBy, RejectDate, RejectReason, M.ConceptNo, M.BookingNo,
                        M.YarnPRFromID, M.RevisionNo,
                        Source = Case 
                        When M.YarnPRFromID={PRFrom.FABRIC_PROJECTION_YARN_BOOKING} then 'Fabric Projection Yarn Booking'
                        else YPFH.YarnPRName 
                        End,
                        M.YarnPRFromTableId,
	                    M.YarnPRFromMasterId
                        from {TableNames.YARN_PR_MASTER} M
                        Inner JOIN {TableNames.YarnPRFrom_HK} YPFH On M.YarnPRFromID = YPFH.YarnPRFromID
                        Inner Join {DbNames.EPYSL}..EntityTypeValue EV On EV.ValueID = M.TriggerPointID
                        LEFT Join {DbNames.EPYSL}..LoginUser L On M.YarnPRBy = L.UserCode
                        LEFT Join {DbNames.EPYSL}..LoginUser A On M.ApproveBy = A.UserCode
                        LEFT Join {DbNames.EPYSL}..LoginUser R On M.RejectBy = R.UserCode
                        WHERE M.SendForApproval = 1 And Approve = 1 And ISNULL(Reject, 0) = 0 And ISNULL(IsCPR,0) = 0 
                        And ISNULL(M.SendForCPRApproval,0)=0 And ISNULL(IsFPR, 0) = 0
                    ),
                    PB AS
                    (
	                    SELECT M.YarnPRMasterID, 
	                    Buyer = CASE WHEN ISNULL(STRING_AGG(B.ShortName, ','),'') <> '' THEN STRING_AGG(B.ShortName, ',')
				                     WHEN PYB.DepartmentID IN ({EnumDepertmentDescription.Knitting},{EnumDepertmentDescription.Operation},{EnumDepertmentDescription.OperationTextile},{EnumDepertmentDescription.PlanningMonitoringAndControl},{EnumDepertmentDescription.ProductionManagementControl}) THEN 'Textile Advance'
	                                 WHEN PYB.DepartmentID IN ({EnumDepertmentDescription.ResearchAndDevelopment}) THEN 'R&D'
				                     ELSE '-'
				                     END
			   
	                    FROM M
	                    INNER JOIN {TableNames.PROJECTION_YARN_BOOKING_MASTER} PYB ON PYB.PYBookingID = M.YarnPRFromMasterId
	                    LEFT JOIN {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} BBT ON BBT.PYBookingID = PYB.PYBookingID
	                    LEFT JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = BBT.BuyerID AND B.ContactID > 0
	                    WHERE M.YarnPRFromTableId = {YarnPRFromTable.ProjectionYarnBookingMaster}
	                    GROUP BY M.YarnPRMasterID, PYB.DepartmentID
                    ),
                    FinalList AS
                    (
	                    SELECT M.YarnPRMasterID, YarnPRNo, YarnPRRequiredDate, M.YarnPRByUser,
                        M.YpApproveBy, M.YpRejectBy, M.TriggerPoint, M.Remarks, SendForApproval, Approve,
                        ApproveBy, ApproveDate, Reject, RejectBy, RejectDate, RejectReason, M.ConceptNo, M.BookingNo,
                        M.YarnPRFromID, M.RevisionNo,
                        M.YarnPRDate,
                        M.Source,
	                    M.YarnPRFromTableId,
	                    M.YarnPRFromMasterId,
	                    Buyer = CASE 
			                    WHEN M.YarnPRFromTableId = {YarnPRFromTable.ProjectionYarnBookingMaster} THEN PB.Buyer
			                    WHEN C.[Name] ='Select' THEN ''
			                    WHEN C.[Name] is NULL THEN ''
			                    ELSE C.[Name]
			                    END
	                    FROM M
	                    INNER JOIN {TableNames.YarnPRChild} MC on MC.YarnPRMasterID = M.YarnPRMasterID
                        LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM on FCM.ConceptID = MC.ConceptID
                        LEFT JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = FCM.BuyerID AND C.ContactID > 0
	                    LEFT JOIN PB ON PB.YarnPRMasterID = M.YarnPRMasterID
	                    GROUP BY M.YarnPRMasterID, YarnPRNo, YarnPRRequiredDate, M.YarnPRByUser,
                        M.YpApproveBy, M.YpRejectBy, M.TriggerPoint, M.Remarks, SendForApproval, Approve,
                        ApproveBy, ApproveDate, Reject, RejectBy, RejectDate, RejectReason, M.ConceptNo, M.BookingNo,
                        M.YarnPRFromID, M.RevisionNo,
                        M.YarnPRDate,
                        M.Source,
	                    M.YarnPRFromTableId,
	                    M.YarnPRFromMasterId,
	                    CASE 
	                    WHEN M.YarnPRFromTableId = {YarnPRFromTable.ProjectionYarnBookingMaster} THEN PB.Buyer
	                    WHEN C.[Name] ='Select' THEN ''
	                    WHEN C.[Name] is NULL THEN ''
	                    ELSE C.[Name]
	                    END
                    )
                    SELECT *, Count(*) Over() TotalRows
                    FROM FinalList";
                }
                else if (status == Status.Active)
                {
                    sql =
                    $@"WITH M AS (
	                    SELECT YarnPRMasterID, YarnPRDate ,YarnPRNo, YarnPRRequiredDate, L.Name [YarnPRByUser], A.Name[YpApproveBy],
                        R.Name[YpRejectBy], EV.ValueName TriggerPoint, Remarks, SendForApproval, Approve, ApproveBy,
                        ApproveDate, Reject, RejectBy, RejectDate, RejectReason, M.ConceptNo, M.BookingNo,
                        M.YarnPRFromID, M.RevisionNo,
                        Source = Case 
                        When M.YarnPRFromID={PRFrom.FABRIC_PROJECTION_YARN_BOOKING} then 'Fabric Projection Yarn Booking'
                        else YPFH.YarnPRName 
                        End,
                        M.YarnPRFromTableId,
	                    M.YarnPRFromMasterId
                        from {TableNames.YARN_PR_MASTER} M
                        Inner JOIN {TableNames.YarnPRFrom_HK} YPFH On M.YarnPRFromID = YPFH.YarnPRFromID
                        Inner Join {DbNames.EPYSL}..EntityTypeValue EV On EV.ValueID = M.TriggerPointID
                        LEFT Join {DbNames.EPYSL}..LoginUser L On M.YarnPRBy = L.UserCode
                        LEFT Join {DbNames.EPYSL}..LoginUser A On M.ApproveBy = A.UserCode
                        LEFT Join {DbNames.EPYSL}..LoginUser R On M.RejectBy = R.UserCode
                        WHERE M.SendForApproval = 1 And Approve = 1 And ISNULL(Reject, 0) = 0 And IsCPR = 1
                        And ISNULL(M.SendForCPRApproval,0)=0 And ISNULL(IsFPR, 0) = 0
                    ),
                    PB AS
                    (
	                    SELECT M.YarnPRMasterID, 
	                    Buyer = CASE WHEN ISNULL(STRING_AGG(B.ShortName, ','),'') <> '' THEN STRING_AGG(B.ShortName, ',')
				                     WHEN PYB.DepartmentID IN ({EnumDepertmentDescription.Knitting},{EnumDepertmentDescription.Operation},{EnumDepertmentDescription.OperationTextile},{EnumDepertmentDescription.PlanningMonitoringAndControl},{EnumDepertmentDescription.ProductionManagementControl}) THEN 'Textile Advance'
	                                 WHEN PYB.DepartmentID IN ({EnumDepertmentDescription.ResearchAndDevelopment}) THEN 'R&D'
				                     ELSE '-'
				                     END
			   
	                    FROM M
	                    INNER JOIN {TableNames.PROJECTION_YARN_BOOKING_MASTER} PYB ON PYB.PYBookingID = M.YarnPRFromMasterId
	                    LEFT JOIN {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} BBT ON BBT.PYBookingID = PYB.PYBookingID
	                    LEFT JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = BBT.BuyerID AND B.ContactID > 0
	                    WHERE M.YarnPRFromTableId = {YarnPRFromTable.ProjectionYarnBookingMaster}
	                    GROUP BY M.YarnPRMasterID, PYB.DepartmentID
                    ),
                    FinalList AS
                    (
	                    SELECT M.YarnPRMasterID, YarnPRNo, YarnPRRequiredDate, M.YarnPRByUser,
                        M.YpApproveBy, M.YpRejectBy, M.TriggerPoint, M.Remarks, SendForApproval, Approve,
                        ApproveBy, ApproveDate, Reject, RejectBy, RejectDate, RejectReason, M.ConceptNo, M.BookingNo,
                        M.YarnPRFromID, M.RevisionNo,
                        M.YarnPRDate,
                        M.Source,
	                    M.YarnPRFromTableId,
	                    M.YarnPRFromMasterId,
	                    Buyer = CASE 
			                    WHEN M.YarnPRFromTableId = {YarnPRFromTable.ProjectionYarnBookingMaster} THEN PB.Buyer
			                    WHEN C.[Name] ='Select' THEN ''
			                    WHEN C.[Name] is NULL THEN ''
			                    ELSE C.[Name]
			                    END
	                    FROM M
	                    INNER JOIN {TableNames.YarnPRChild} MC on MC.YarnPRMasterID = M.YarnPRMasterID
                        LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM on FCM.ConceptID = MC.ConceptID
                        LEFT JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = FCM.BuyerID AND C.ContactID > 0
	                    LEFT JOIN PB ON PB.YarnPRMasterID = M.YarnPRMasterID
	                    GROUP BY M.YarnPRMasterID, YarnPRNo, YarnPRRequiredDate, M.YarnPRByUser,
                        M.YpApproveBy, M.YpRejectBy, M.TriggerPoint, M.Remarks, SendForApproval, Approve,
                        ApproveBy, ApproveDate, Reject, RejectBy, RejectDate, RejectReason, M.ConceptNo, M.BookingNo,
                        M.YarnPRFromID, M.RevisionNo,
                        M.YarnPRDate,
                        M.Source,
	                    M.YarnPRFromTableId,
	                    M.YarnPRFromMasterId,
	                    CASE 
	                    WHEN M.YarnPRFromTableId = {YarnPRFromTable.ProjectionYarnBookingMaster} THEN PB.Buyer
	                    WHEN C.[Name] ='Select' THEN ''
	                    WHEN C.[Name] is NULL THEN ''
	                    ELSE C.[Name]
	                    END
                    )
                    SELECT *, Count(*) Over() TotalRows
                    FROM FinalList";
                }
                else
                {
                    sql =
                    $@"WITH M AS (
	                    SELECT YarnPRMasterID, YarnPRDate ,YarnPRNo, YarnPRRequiredDate, L.Name [YarnPRByUser], A.Name[YpApproveBy],
                        R.Name[YpRejectBy], EV.ValueName TriggerPoint, Remarks, SendForApproval, Approve, ApproveBy,
                        ApproveDate, Reject, RejectBy, RejectDate, RejectReason, M.ConceptNo, M.BookingNo,
                        M.YarnPRFromID, M.RevisionNo,
                        Source = Case 
                        When M.YarnPRFromID={PRFrom.FABRIC_PROJECTION_YARN_BOOKING} then 'Fabric Projection Yarn Booking'
                        else YPFH.YarnPRName 
                        End,
                        M.YarnPRFromTableId,
	                    M.YarnPRFromMasterId
                        from {TableNames.YARN_PR_MASTER} M
                        Inner JOIN {TableNames.YarnPRFrom_HK} YPFH On M.YarnPRFromID = YPFH.YarnPRFromID
                        Inner Join {DbNames.EPYSL}..EntityTypeValue EV On EV.ValueID = M.TriggerPointID
                        LEFT Join {DbNames.EPYSL}..LoginUser L On M.YarnPRBy = L.UserCode
                        LEFT Join {DbNames.EPYSL}..LoginUser A On M.ApproveBy = A.UserCode
                        LEFT Join {DbNames.EPYSL}..LoginUser R On M.RejectBy = R.UserCode
                        WHERE M.SendForApproval = 1 And Approve = 1 And ISNULL(Reject, 0) = 0 AND IsCPR = 1
                    ),
                    PB AS
                    (
	                    SELECT M.YarnPRMasterID, 
	                    Buyer = CASE WHEN ISNULL(STRING_AGG(B.ShortName, ','),'') <> '' THEN STRING_AGG(B.ShortName, ',')
				                     WHEN PYB.DepartmentID IN ({EnumDepertmentDescription.Knitting},{EnumDepertmentDescription.Operation},{EnumDepertmentDescription.OperationTextile},{EnumDepertmentDescription.PlanningMonitoringAndControl},{EnumDepertmentDescription.ProductionManagementControl}) THEN 'Textile Advance'
	                                 WHEN PYB.DepartmentID IN ({EnumDepertmentDescription.ResearchAndDevelopment}) THEN 'R&D'
				                     ELSE '-'
				                     END
			   
	                    FROM M
	                    INNER JOIN {TableNames.PROJECTION_YARN_BOOKING_MASTER} PYB ON PYB.PYBookingID = M.YarnPRFromMasterId
	                    LEFT JOIN {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} BBT ON BBT.PYBookingID = PYB.PYBookingID
	                    LEFT JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = BBT.BuyerID AND B.ContactID > 0
	                    WHERE M.YarnPRFromTableId = {YarnPRFromTable.ProjectionYarnBookingMaster}
	                    GROUP BY M.YarnPRMasterID, PYB.DepartmentID
                    ),
                    FinalList AS
                    (
	                    SELECT M.YarnPRMasterID, YarnPRNo, YarnPRRequiredDate, M.YarnPRByUser,
                        M.YpApproveBy, M.YpRejectBy, M.TriggerPoint, M.Remarks, SendForApproval, Approve,
                        ApproveBy, ApproveDate, Reject, RejectBy, RejectDate, RejectReason, M.ConceptNo, M.BookingNo,
                        M.YarnPRFromID, M.RevisionNo,
                        M.YarnPRDate,
                        M.Source,
	                    M.YarnPRFromTableId,
	                    M.YarnPRFromMasterId,
	                    Buyer = CASE 
			                    WHEN M.YarnPRFromTableId = {YarnPRFromTable.ProjectionYarnBookingMaster} THEN PB.Buyer
			                    WHEN C.[Name] ='Select' THEN ''
			                    WHEN C.[Name] is NULL THEN ''
			                    ELSE C.[Name]
			                    END
	                    FROM M
	                    INNER JOIN {TableNames.YarnPRChild} MC on MC.YarnPRMasterID = M.YarnPRMasterID
                        LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM on FCM.ConceptID = MC.ConceptID
                        LEFT JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = FCM.BuyerID AND C.ContactID > 0
	                    LEFT JOIN PB ON PB.YarnPRMasterID = M.YarnPRMasterID
	                    GROUP BY M.YarnPRMasterID, YarnPRNo, YarnPRRequiredDate, M.YarnPRByUser,
                        M.YpApproveBy, M.YpRejectBy, M.TriggerPoint, M.Remarks, SendForApproval, Approve,
                        ApproveBy, ApproveDate, Reject, RejectBy, RejectDate, RejectReason, M.ConceptNo, M.BookingNo,
                        M.YarnPRFromID, M.RevisionNo,
                        M.YarnPRDate,
                        M.Source,
	                    M.YarnPRFromTableId,
	                    M.YarnPRFromMasterId,
	                    CASE 
	                    WHEN M.YarnPRFromTableId = {YarnPRFromTable.ProjectionYarnBookingMaster} THEN PB.Buyer
	                    WHEN C.[Name] ='Select' THEN ''
	                    WHEN C.[Name] is NULL THEN ''
	                    ELSE C.[Name]
	                    END
                    )
                    SELECT *, Count(*) Over() TotalRows
                    FROM FinalList";
                }
            }

            #endregion CPR Page

            #region FPR Page

            else if (pageName == PageNames.FPR)
            {
                if (status == Status.Pending)
                {
                    sql =
                    $@"WITH M AS (
	                    SELECT YarnPRMasterID, YarnPRDate ,YarnPRNo, YarnPRRequiredDate, L.Name [YarnPRByUser], A.Name[YpApproveBy],
                        R.Name[YpRejectBy], EV.ValueName TriggerPoint, Remarks, SendForApproval, Approve, ApproveBy, ApproveDate,
                        Reject, RejectBy, RejectDate, RejectReason, M.ConceptNo, M.BookingNo, M.YarnPRFromID, M.RevisionNo,
                        Source = Case 
                        When M.YarnPRFromID={PRFrom.FABRIC_PROJECTION_YARN_BOOKING} then 'Fabric Projection Yarn Booking'
                        else YPFH.YarnPRName
                        End,
                        M.YarnPRFromTableId,
	                    M.YarnPRFromMasterId
                        from {TableNames.YARN_PR_MASTER} M
                        Inner JOIN {TableNames.YarnPRFrom_HK} YPFH On M.YarnPRFromID = YPFH.YarnPRFromID
                        Inner Join {DbNames.EPYSL}..EntityTypeValue EV On EV.ValueID = M.TriggerPointID
                        LEFT Join {DbNames.EPYSL}..LoginUser L On M.YarnPRBy = L.UserCode
                        LEFT Join {DbNames.EPYSL}..LoginUser A On M.ApproveBy = A.UserCode
                        LEFT Join {DbNames.EPYSL}..LoginUser R On M.RejectBy = R.UserCode
                        WHERE M.SendForApproval = 1 And M.Approve = 1 And ISNULL(M.Reject, 0) = 0 And IsCPR = 1 
                        And ISNULL(M.SendForCPRApproval,0)=1 And ISNULL(IsFPR, 0) = 0
                    ),
                    PB AS
                    (
	                    SELECT M.YarnPRMasterID, 
	                    Buyer = CASE WHEN ISNULL(STRING_AGG(B.ShortName, ','),'') <> '' THEN STRING_AGG(B.ShortName, ',')
				                     WHEN PYB.DepartmentID IN ({EnumDepertmentDescription.Knitting},{EnumDepertmentDescription.Operation},{EnumDepertmentDescription.OperationTextile},{EnumDepertmentDescription.PlanningMonitoringAndControl},{EnumDepertmentDescription.ProductionManagementControl}) THEN 'Textile Advance'
	                                 WHEN PYB.DepartmentID IN ({EnumDepertmentDescription.ResearchAndDevelopment}) THEN 'R&D'
				                     ELSE '-'
				                     END
			   
	                    FROM M
	                    INNER JOIN {TableNames.PROJECTION_YARN_BOOKING_MASTER} PYB ON PYB.PYBookingID = M.YarnPRFromMasterId
	                    LEFT JOIN {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} BBT ON BBT.PYBookingID = PYB.PYBookingID
	                    LEFT JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = BBT.BuyerID AND B.ContactID > 0
	                    WHERE M.YarnPRFromTableId = {YarnPRFromTable.ProjectionYarnBookingMaster}
	                    GROUP BY M.YarnPRMasterID, PYB.DepartmentID
                    ),
                    FinalList AS
                    (
	                    SELECT M.YarnPRMasterID, YarnPRNo, YarnPRRequiredDate, M.YarnPRByUser,
                        M.YpApproveBy, M.YpRejectBy, M.TriggerPoint, M.Remarks, SendForApproval, Approve,
                        ApproveBy, ApproveDate, Reject, RejectBy, RejectDate, RejectReason, M.ConceptNo, M.BookingNo,
                        M.YarnPRFromID, M.RevisionNo,
                        M.YarnPRDate,
                        M.Source,
	                    M.YarnPRFromTableId,
	                    M.YarnPRFromMasterId,
	                    Buyer = CASE 
			                    WHEN M.YarnPRFromTableId = {YarnPRFromTable.ProjectionYarnBookingMaster} THEN PB.Buyer
			                    WHEN C.[Name] ='Select' THEN ''
			                    WHEN C.[Name] is NULL THEN ''
			                    ELSE C.[Name]
			                    END
	                    FROM M
	                    INNER JOIN {TableNames.YarnPRChild} MC on MC.YarnPRMasterID = M.YarnPRMasterID
                        LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM on FCM.ConceptID = MC.ConceptID
                        LEFT JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = FCM.BuyerID AND C.ContactID > 0
	                    LEFT JOIN PB ON PB.YarnPRMasterID = M.YarnPRMasterID
	                    GROUP BY M.YarnPRMasterID, YarnPRNo, YarnPRRequiredDate, M.YarnPRByUser,
                        M.YpApproveBy, M.YpRejectBy, M.TriggerPoint, M.Remarks, SendForApproval, Approve,
                        ApproveBy, ApproveDate, Reject, RejectBy, RejectDate, RejectReason, M.ConceptNo, M.BookingNo,
                        M.YarnPRFromID, M.RevisionNo,
                        M.YarnPRDate,
                        M.Source,
	                    M.YarnPRFromTableId,
	                    M.YarnPRFromMasterId,
	                    CASE 
	                    WHEN M.YarnPRFromTableId = {YarnPRFromTable.ProjectionYarnBookingMaster} THEN PB.Buyer
	                    WHEN C.[Name] ='Select' THEN ''
	                    WHEN C.[Name] is NULL THEN ''
	                    ELSE C.[Name]
	                    END
                    )
                    SELECT *, Count(*) Over() TotalRows
                    FROM FinalList";
                }
                else if (status == Status.AllStatus)
                {
                    sql =
                    $@"WITH M AS (
	                    SELECT YarnPRMasterID, YarnPRDate ,YarnPRNo, YarnPRRequiredDate, L.Name [YarnPRByUser], A.Name[YpApproveBy],
                        R.Name[YpRejectBy], EV.ValueName TriggerPoint, Remarks, SendForApproval, Approve, ApproveBy,
                        ApproveDate, Reject, RejectBy, RejectDate, RejectReason, M.ConceptNo, M.BookingNo,
                        M.YarnPRFromID, M.RevisionNo,
                        Source = Case 
                        When M.YarnPRFromID={PRFrom.FABRIC_PROJECTION_YARN_BOOKING} then 'Fabric Projection Yarn Booking'
                        else YPFH.YarnPRName 
                        End,
                        PRStatus= (CASE WHEN M.IsCPR=0 THEN 'Commercial PR Pending' WHEN IsFPR=0 THEN 'Acknowledge Pending' WHEN IsFPR > 0 THEN 'Acknowledged' ELSE '-' END),
                        M.YarnPRFromTableId,
	                    M.YarnPRFromMasterId
                        from {TableNames.YARN_PR_MASTER} M
                        Inner JOIN {TableNames.YarnPRFrom_HK} YPFH On M.YarnPRFromID = YPFH.YarnPRFromID
                        Inner Join {DbNames.EPYSL}..EntityTypeValue EV On EV.ValueID = M.TriggerPointID
                        LEFT Join {DbNames.EPYSL}..LoginUser L On M.YarnPRBy = L.UserCode
                        LEFT Join {DbNames.EPYSL}..LoginUser A On M.ApproveBy = A.UserCode
                        LEFT Join {DbNames.EPYSL}..LoginUser R On M.RejectBy = R.UserCode
                        WHERE M.Approve=1
                    ),
                    PB AS
                    (
	                    SELECT M.YarnPRMasterID, 
	                    Buyer = CASE WHEN ISNULL(STRING_AGG(B.ShortName, ','),'') <> '' THEN STRING_AGG(B.ShortName, ',')
				                     WHEN PYB.DepartmentID IN ({EnumDepertmentDescription.Knitting},{EnumDepertmentDescription.Operation},{EnumDepertmentDescription.OperationTextile},{EnumDepertmentDescription.PlanningMonitoringAndControl},{EnumDepertmentDescription.ProductionManagementControl}) THEN 'Textile Advance'
	                                 WHEN PYB.DepartmentID IN ({EnumDepertmentDescription.ResearchAndDevelopment}) THEN 'R&D'
				                     ELSE '-'
				                     END
			   
	                    FROM M
	                    INNER JOIN {TableNames.PROJECTION_YARN_BOOKING_MASTER} PYB ON PYB.PYBookingID = M.YarnPRFromMasterId
	                    LEFT JOIN {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} BBT ON BBT.PYBookingID = PYB.PYBookingID
	                    LEFT JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = BBT.BuyerID AND B.ContactID > 0
	                    WHERE M.YarnPRFromTableId = {YarnPRFromTable.ProjectionYarnBookingMaster}
	                    GROUP BY M.YarnPRMasterID, PYB.DepartmentID
                    ),
                    FinalList AS
                    (
	                    SELECT M.YarnPRMasterID, YarnPRNo, YarnPRRequiredDate, M.YarnPRByUser,
                        M.YpApproveBy, M.YpRejectBy, M.TriggerPoint, M.Remarks, SendForApproval, Approve,
                        ApproveBy, ApproveDate, Reject, RejectBy, RejectDate, RejectReason, M.ConceptNo, M.BookingNo,
                        M.YarnPRFromID, M.RevisionNo,
                        M.YarnPRDate,
                        M.Source,
	                    M.YarnPRFromTableId,
	                    M.YarnPRFromMasterId,
	                    Buyer = CASE 
			                    WHEN M.YarnPRFromTableId = {YarnPRFromTable.ProjectionYarnBookingMaster} THEN PB.Buyer
			                    WHEN C.[Name] ='Select' THEN ''
			                    WHEN C.[Name] is NULL THEN ''
			                    ELSE C.[Name]
			                    END
	                    FROM M
	                    INNER JOIN {TableNames.YarnPRChild} MC on MC.YarnPRMasterID = M.YarnPRMasterID
                        LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM on FCM.ConceptID = MC.ConceptID
                        LEFT JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = FCM.BuyerID AND C.ContactID > 0
	                    LEFT JOIN PB ON PB.YarnPRMasterID = M.YarnPRMasterID
	                    GROUP BY M.YarnPRMasterID, YarnPRNo, YarnPRRequiredDate, M.YarnPRByUser,
                        M.YpApproveBy, M.YpRejectBy, M.TriggerPoint, M.Remarks, SendForApproval, Approve,
                        ApproveBy, ApproveDate, Reject, RejectBy, RejectDate, RejectReason, M.ConceptNo, M.BookingNo,
                        M.YarnPRFromID, M.RevisionNo,
                        M.YarnPRDate,
                        M.Source,
	                    M.YarnPRFromTableId,
	                    M.YarnPRFromMasterId,
	                    CASE 
	                    WHEN M.YarnPRFromTableId = {YarnPRFromTable.ProjectionYarnBookingMaster} THEN PB.Buyer
	                    WHEN C.[Name] ='Select' THEN ''
	                    WHEN C.[Name] is NULL THEN ''
	                    ELSE C.[Name]
	                    END
                    )
                    SELECT *, Count(*) Over() TotalRows
                    FROM FinalList";
                }
                else
                {
                    sql =
                    $@"WITH M AS (
	                    SELECT YarnPRMasterID, YarnPRDate ,YarnPRNo, YarnPRRequiredDate, L.Name [YarnPRByUser], A.Name[YpApproveBy],
                        R.Name[YpRejectBy], EV.ValueName TriggerPoint, Remarks, SendForApproval, Approve, ApproveBy,
                        ApproveDate, Reject, RejectBy, RejectDate, RejectReason, M.ConceptNo, M.BookingNo,
                        M.YarnPRFromID, M.RevisionNo,
                        Source = Case 
                        When M.YarnPRFromID={PRFrom.FABRIC_PROJECTION_YARN_BOOKING} then 'Fabric Projection Yarn Booking'
                        else YPFH.YarnPRName 
                        End,
                        M.YarnPRFromTableId,
	                    M.YarnPRFromMasterId
                        from {TableNames.YARN_PR_MASTER} M
                        Inner JOIN {TableNames.YarnPRFrom_HK} YPFH On M.YarnPRFromID = YPFH.YarnPRFromID
                        Inner Join {DbNames.EPYSL}..EntityTypeValue EV On EV.ValueID = M.TriggerPointID
                        LEFT Join {DbNames.EPYSL}..LoginUser L On M.YarnPRBy = L.UserCode
                        LEFT Join {DbNames.EPYSL}..LoginUser A On M.ApproveBy = A.UserCode
                        LEFT Join {DbNames.EPYSL}..LoginUser R On M.RejectBy = R.UserCode
                        WHERE M.SendForApproval = 1 And Approve = 1 And IsCPR = 1 
                        And ISNULL(M.SendForCPRApproval,0)=1 And IsFPR = 1
                    ),
                    PB AS
                    (
	                    SELECT M.YarnPRMasterID, 
	                    Buyer = CASE WHEN ISNULL(STRING_AGG(B.ShortName, ','),'') <> '' THEN STRING_AGG(B.ShortName, ',')
				                     WHEN PYB.DepartmentID IN ({EnumDepertmentDescription.Knitting},{EnumDepertmentDescription.Operation},{EnumDepertmentDescription.OperationTextile},{EnumDepertmentDescription.PlanningMonitoringAndControl},{EnumDepertmentDescription.ProductionManagementControl}) THEN 'Textile Advance'
	                                 WHEN PYB.DepartmentID IN ({EnumDepertmentDescription.ResearchAndDevelopment}) THEN 'R&D'
				                     ELSE '-'
				                     END
			   
	                    FROM M
	                    INNER JOIN {TableNames.PROJECTION_YARN_BOOKING_MASTER} PYB ON PYB.PYBookingID = M.YarnPRFromMasterId
	                    LEFT JOIN {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} BBT ON BBT.PYBookingID = PYB.PYBookingID
	                    LEFT JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = BBT.BuyerID AND B.ContactID > 0
	                    WHERE M.YarnPRFromTableId = {YarnPRFromTable.ProjectionYarnBookingMaster}
	                    GROUP BY M.YarnPRMasterID, PYB.DepartmentID
                    ),
                    FinalList AS
                    (
	                    SELECT M.YarnPRMasterID, YarnPRNo, YarnPRRequiredDate, M.YarnPRByUser,
                        M.YpApproveBy, M.YpRejectBy, M.TriggerPoint, M.Remarks, SendForApproval, Approve,
                        ApproveBy, ApproveDate, Reject, RejectBy, RejectDate, RejectReason, M.ConceptNo, M.BookingNo,
                        M.YarnPRFromID, M.RevisionNo,
                        M.YarnPRDate,
                        M.Source,
	                    M.YarnPRFromTableId,
	                    M.YarnPRFromMasterId,
	                    Buyer = CASE 
			                    WHEN M.YarnPRFromTableId = {YarnPRFromTable.ProjectionYarnBookingMaster} THEN PB.Buyer
			                    WHEN C.[Name] ='Select' THEN ''
			                    WHEN C.[Name] is NULL THEN ''
			                    ELSE C.[Name]
			                    END
	                    FROM M
	                    INNER JOIN {TableNames.YarnPRChild} MC on MC.YarnPRMasterID = M.YarnPRMasterID
                        LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM on FCM.ConceptID = MC.ConceptID
                        LEFT JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = FCM.BuyerID AND C.ContactID > 0
	                    LEFT JOIN PB ON PB.YarnPRMasterID = M.YarnPRMasterID
	                    GROUP BY M.YarnPRMasterID, YarnPRNo, YarnPRRequiredDate, M.YarnPRByUser,
                        M.YpApproveBy, M.YpRejectBy, M.TriggerPoint, M.Remarks, SendForApproval, Approve,
                        ApproveBy, ApproveDate, Reject, RejectBy, RejectDate, RejectReason, M.ConceptNo, M.BookingNo,
                        M.YarnPRFromID, M.RevisionNo,
                        M.YarnPRDate,
                        M.Source,
	                    M.YarnPRFromTableId,
	                    M.YarnPRFromMasterId,
	                    CASE 
	                    WHEN M.YarnPRFromTableId = {YarnPRFromTable.ProjectionYarnBookingMaster} THEN PB.Buyer
	                    WHEN C.[Name] ='Select' THEN ''
	                    WHEN C.[Name] is NULL THEN ''
	                    ELSE C.[Name]
	                    END
                    )
                    SELECT *, Count(*) Over() TotalRows
                    FROM FinalList";
                }
            }

            #endregion FPR Page

            #region PR Page

            else
            {
                if (status == Status.Proposed)
                {
                    sql =
                    $@"WITH M AS
                    (
	                    SELECT M.YarnPRMasterID, YarnPRNo, YarnPRRequiredDate, L.Name [YarnPRByUser],
                        A.Name[YpApproveBy], R.Name[YpRejectBy], EV.ValueName TriggerPoint, M.Remarks, SendForApproval, Approve,
                        ApproveBy, ApproveDate, Reject, RejectBy, RejectDate, RejectReason, M.ConceptNo, M.BookingNo,
                        M.YarnPRFromID, M.RevisionNo,
                        CONVERT(DATETIME, CONVERT(CHAR(8), YarnPRDate, 112) + ' ' + CONVERT(CHAR(8), M.DateAdded, 108)) YarnPRDate,
                        Case 
                        When M.YarnPRFromID={PRFrom.FABRIC_PROJECTION_YARN_BOOKING} then 'Fabric Projection Yarn Booking'
                        else YPFH.YarnPRName 
                        End [Source],
                        CreateBy = CC.Name,
	                    M.YarnPRFromTableId,
	                    M.YarnPRFromMasterId
                       
	                    FROM {TableNames.YARN_PR_MASTER} M
                        Inner JOIN {TableNames.YarnPRFrom_HK} YPFH On M.YarnPRFromID = YPFH.YarnPRFromID
	                    Inner Join {DbNames.EPYSL}..EntityTypeValue EV On EV.ValueID = M.TriggerPointID
	                    LEFT Join {DbNames.EPYSL}..LoginUser L On M.YarnPRBy = L.UserCode
	                    LEFT Join {DbNames.EPYSL}..LoginUser A On M.ApproveBy = A.UserCode
	                    LEFT Join {DbNames.EPYSL}..LoginUser R On M.RejectBy = R.UserCode
                        LEFT Join {DbNames.EPYSL}..LoginUser CC On M.AddedBy = CC.UserCode
	                    WHERE M.SendForApproval = 1 And ISNULL(Approve, 0) = 0 And ISNULL(Reject, 0) = 0 AND M.NeedRevision = 0
                        Group by M.YarnPRMasterID, YarnPRDate ,YarnPRNo, YarnPRRequiredDate, L.Name,
                        A.Name, R.Name, EV.ValueName, M.Remarks, SendForApproval,
                        Approve, ApproveBy, ApproveDate, Reject, RejectBy, RejectDate, RejectReason, M.ConceptNo, M.BookingNo,
                        M.YarnPRFromID, M.RevisionNo,YPFH.YarnPRName,CC.Name,M.YarnPRFromTableId,
	                    M.YarnPRFromMasterId,M.DateAdded
                    ),
                    PB AS
                    (
	                    SELECT M.YarnPRMasterID, 
	                    Buyer = CASE WHEN ISNULL(STRING_AGG(B.ShortName, ','),'') <> '' THEN STRING_AGG(B.ShortName, ',')
				                     WHEN PYB.DepartmentID IN ({EnumDepertmentDescription.Knitting},{EnumDepertmentDescription.Operation},{EnumDepertmentDescription.OperationTextile},{EnumDepertmentDescription.PlanningMonitoringAndControl},{EnumDepertmentDescription.ProductionManagementControl}) THEN 'Textile Advance'
	                                 WHEN PYB.DepartmentID IN ({EnumDepertmentDescription.ResearchAndDevelopment}) THEN 'R&D'
				                     ELSE '-'
				                     END
			   
	                    FROM M
	                    INNER JOIN {TableNames.PROJECTION_YARN_BOOKING_MASTER} PYB ON PYB.PYBookingID = M.YarnPRFromMasterId
	                    LEFT JOIN {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} BBT ON BBT.PYBookingID = PYB.PYBookingID
	                    LEFT JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = BBT.BuyerID AND B.ContactID > 0
	                    WHERE M.YarnPRFromTableId = {YarnPRFromTable.ProjectionYarnBookingMaster} --ProjectionYarnBookingMaster
	                    GROUP BY M.YarnPRMasterID, PYB.DepartmentID
                    ),
                    FinalList AS
                    (
	                    SELECT M.YarnPRMasterID, YarnPRNo, YarnPRRequiredDate, M.YarnPRByUser,
                        M.YpApproveBy, M.YpRejectBy, M.TriggerPoint, M.Remarks, SendForApproval, Approve,
                        ApproveBy, ApproveDate, Reject, RejectBy, RejectDate, RejectReason, M.ConceptNo, M.BookingNo,
                        M.YarnPRFromID, M.RevisionNo,
                        M.YarnPRDate,
                        M.Source,
                        M.CreateBy,
	                    M.YarnPRFromTableId,
	                    M.YarnPRFromMasterId,
	                    Buyer = CASE 
			                    WHEN M.YarnPRFromTableId = {YarnPRFromTable.ProjectionYarnBookingMaster} THEN PB.Buyer
			                    WHEN C.[Name] ='Select' THEN ''
			                    WHEN C.[Name] is NULL THEN ''
			                    ELSE C.[Name]
			                    END
	                    FROM M
	                    INNER JOIN {TableNames.YarnPRChild} MC on MC.YarnPRMasterID = M.YarnPRMasterID
                        LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM on FCM.ConceptID = MC.ConceptID
                        LEFT JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = FCM.BuyerID AND C.ContactID > 0
	                    LEFT JOIN PB ON PB.YarnPRMasterID = M.YarnPRMasterID
	                    GROUP BY M.YarnPRMasterID, YarnPRNo, YarnPRRequiredDate, M.YarnPRByUser,
                        M.YpApproveBy, M.YpRejectBy, M.TriggerPoint, M.Remarks, SendForApproval, Approve,
                        ApproveBy, ApproveDate, Reject, RejectBy, RejectDate, RejectReason, M.ConceptNo, M.BookingNo,
                        M.YarnPRFromID, M.RevisionNo,
                        M.YarnPRDate,
                        M.Source,
                        M.CreateBy,
	                    M.YarnPRFromTableId,
	                    M.YarnPRFromMasterId,
	                    CASE 
	                    WHEN M.YarnPRFromTableId = {YarnPRFromTable.ProjectionYarnBookingMaster} THEN PB.Buyer
	                    WHEN C.[Name] ='Select' THEN ''
	                    WHEN C.[Name] is NULL THEN ''
	                    ELSE C.[Name]
	                    END
                    )

                    SELECT *, Count(*) Over() TotalRows
                    FROM FinalList";
                }
                else if (status == Status.Approved)
                {
                    sql =
                    $@"WITH M AS (
	                    SELECT M.YarnPRMasterID, YarnPRNo, YarnPRRequiredDate, L.Name [YarnPRByUser],
                        A.Name[YpApproveBy], R.Name[YpRejectBy], EV.ValueName TriggerPoint, M.Remarks, SendForApproval, Approve,
                        ApproveBy, ApproveDate, Reject, RejectBy, RejectDate, RejectReason, M.ConceptNo, M.BookingNo,
                        M.YarnPRFromID, M.RevisionNo,
                        CONVERT(DATETIME, CONVERT(CHAR(8), YarnPRDate, 112) + ' ' + CONVERT(CHAR(8), M.DateAdded, 108)) YarnPRDate,
                        Case 
                        When M.YarnPRFromID={PRFrom.FABRIC_PROJECTION_YARN_BOOKING} then 'Fabric Projection Yarn Booking'
                        else YPFH.YarnPRName 
                        End [Source],
                        CreateBy = CC.Name,
	                    M.YarnPRFromTableId,
	                    M.YarnPRFromMasterId
                        from {TableNames.YARN_PR_MASTER} M
                        Inner JOIN {TableNames.YarnPRFrom_HK} YPFH On M.YarnPRFromID = YPFH.YarnPRFromID
                        Inner Join {DbNames.EPYSL}..EntityTypeValue EV On EV.ValueID = M.TriggerPointID
                        LEFT Join {DbNames.EPYSL}..LoginUser L On M.YarnPRBy = L.UserCode
                        LEFT Join {DbNames.EPYSL}..LoginUser A On M.ApproveBy = A.UserCode
                        LEFT Join {DbNames.EPYSL}..LoginUser R On M.RejectBy = R.UserCode
                        LEFT Join {DbNames.EPYSL}..LoginUser CC On M.AddedBy = CC.UserCode
                        WHERE M.SendForApproval = 1 And Approve = 1 And ISNULL(Reject, 0) = 0 AND M.NeedRevision = 0
                        Group by M.YarnPRMasterID, YarnPRDate ,YarnPRNo, YarnPRRequiredDate, L.Name,
                        A.Name, R.Name, EV.ValueName, M.Remarks, SendForApproval, M.DateAdded,
                        Approve, ApproveBy, ApproveDate, Reject, RejectBy, RejectDate, RejectReason, M.ConceptNo, M.BookingNo,
                        M.YarnPRFromID, M.RevisionNo,YPFH.YarnPRName,CC.Name,M.YarnPRFromTableId,
	                    M.YarnPRFromMasterId
                        ORDER BY M.ApproveDate DESC OFFSET 0 ROWS
                    ),
                    PB AS
                    (
	                    SELECT M.YarnPRMasterID, 
	                    Buyer = CASE WHEN ISNULL(STRING_AGG(B.ShortName, ','),'') <> '' THEN STRING_AGG(B.ShortName, ',')
				                     WHEN PYB.DepartmentID IN ({EnumDepertmentDescription.Knitting},{EnumDepertmentDescription.Operation},{EnumDepertmentDescription.OperationTextile},{EnumDepertmentDescription.PlanningMonitoringAndControl},{EnumDepertmentDescription.ProductionManagementControl}) THEN 'Textile Advance'
	                                 WHEN PYB.DepartmentID IN ({EnumDepertmentDescription.ResearchAndDevelopment}) THEN 'R&D'
				                     ELSE '-'
				                     END
			   
	                    FROM M
	                    INNER JOIN {TableNames.PROJECTION_YARN_BOOKING_MASTER} PYB ON PYB.PYBookingID = M.YarnPRFromMasterId
	                    LEFT JOIN {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} BBT ON BBT.PYBookingID = PYB.PYBookingID
	                    LEFT JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = BBT.BuyerID AND B.ContactID > 0
	                    WHERE M.YarnPRFromTableId = {YarnPRFromTable.ProjectionYarnBookingMaster} --ProjectionYarnBookingMaster
	                    GROUP BY M.YarnPRMasterID, PYB.DepartmentID
                    ),
                    FinalList AS
                    (
	                    SELECT M.YarnPRMasterID, YarnPRNo, YarnPRRequiredDate, M.YarnPRByUser,
                        M.YpApproveBy, M.YpRejectBy, M.TriggerPoint, M.Remarks, SendForApproval, Approve,
                        ApproveBy, ApproveDate, Reject, RejectBy, RejectDate, RejectReason, M.ConceptNo, M.BookingNo,
                        M.YarnPRFromID, M.RevisionNo,
                        M.YarnPRDate,
                        M.Source,
                        M.CreateBy,
	                    M.YarnPRFromTableId,
	                    M.YarnPRFromMasterId,
	                    Buyer = CASE 
			                    WHEN M.YarnPRFromTableId = {YarnPRFromTable.ProjectionYarnBookingMaster} THEN PB.Buyer
			                    WHEN C.[Name] ='Select' THEN ''
			                    WHEN C.[Name] is NULL THEN ''
			                    ELSE C.[Name]
			                    END
	                    FROM M
	                    INNER JOIN {TableNames.YarnPRChild} MC on MC.YarnPRMasterID = M.YarnPRMasterID
                        LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM on FCM.ConceptID = MC.ConceptID
                        LEFT JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = FCM.BuyerID AND C.ContactID > 0
	                    LEFT JOIN PB ON PB.YarnPRMasterID = M.YarnPRMasterID
	                    GROUP BY M.YarnPRMasterID, YarnPRNo, YarnPRRequiredDate, M.YarnPRByUser,
                        M.YpApproveBy, M.YpRejectBy, M.TriggerPoint, M.Remarks, SendForApproval, Approve,
                        ApproveBy, ApproveDate, Reject, RejectBy, RejectDate, RejectReason, M.ConceptNo, M.BookingNo,
                        M.YarnPRFromID, M.RevisionNo,
                        M.YarnPRDate,
                        M.Source,
                        M.CreateBy,
	                    M.YarnPRFromTableId,
	                    M.YarnPRFromMasterId,
	                    CASE 
	                    WHEN M.YarnPRFromTableId = {YarnPRFromTable.ProjectionYarnBookingMaster} THEN PB.Buyer
	                    WHEN C.[Name] ='Select' THEN ''
	                    WHEN C.[Name] is NULL THEN ''
	                    ELSE C.[Name]
	                    END
                    )

                    SELECT *, Count(*) Over() TotalRows
                    FROM FinalList";
                }
                else if (status == Status.Reject)
                {
                    sql =
                    $@"WITH M AS (
	                    SELECT M.YarnPRMasterID, YarnPRDate ,YarnPRNo, YarnPRRequiredDate, L.Name [YarnPRByUser], A.Name[YpApproveBy],
                        R.Name[YpRejectBy], EV.ValueName TriggerPoint, M.Remarks, SendForApproval, Approve, ApproveBy,
                        ApproveDate, Reject, RejectBy, RejectDate, RejectReason, M.ConceptNo, M.BookingNo,
                        M.YarnPRFromID, M.RevisionNo,
                        Source = Case 
                        When M.YarnPRFromID={PRFrom.FABRIC_PROJECTION_YARN_BOOKING} then 'Fabric Projection Yarn Booking'
                        else YPFH.YarnPRName 
                        End,
                        CreateBy = CC.Name,
	                    M.YarnPRFromTableId,
	                    M.YarnPRFromMasterId
                        from {TableNames.YARN_PR_MASTER} M
                        Inner JOIN {TableNames.YarnPRFrom_HK} YPFH On M.YarnPRFromID = YPFH.YarnPRFromID
                        Inner Join {DbNames.EPYSL}..EntityTypeValue EV On EV.ValueID = M.TriggerPointID
                        LEFT Join {DbNames.EPYSL}..LoginUser L On M.YarnPRBy = L.UserCode
                        LEFT Join {DbNames.EPYSL}..LoginUser A On M.ApproveBy = A.UserCode
                        LEFT Join {DbNames.EPYSL}..LoginUser R On M.RejectBy = R.UserCode
                        LEFT Join {DbNames.EPYSL}..LoginUser CC On M.AddedBy = CC.UserCode
                        WHERE ISNULL(Approve, 0) = 0 And Reject = 1 AND M.NeedRevision = 0
                        Group by M.YarnPRMasterID, YarnPRDate ,YarnPRNo, YarnPRRequiredDate, L.Name,
                        A.Name, R.Name, EV.ValueName, M.Remarks, SendForApproval,
                        Approve, ApproveBy, ApproveDate, Reject, RejectBy, RejectDate, RejectReason, M.ConceptNo, M.BookingNo,
                        M.YarnPRFromID, M.RevisionNo,YPFH.YarnPRName,CC.Name,M.YarnPRFromTableId,
	                    M.YarnPRFromMasterId
                    ),
                    PB AS
                    (
	                    SELECT M.YarnPRMasterID, 
	                    Buyer = CASE WHEN ISNULL(STRING_AGG(B.ShortName, ','),'') <> '' THEN STRING_AGG(B.ShortName, ',')
				                     WHEN PYB.DepartmentID IN ({EnumDepertmentDescription.Knitting},{EnumDepertmentDescription.Operation},{EnumDepertmentDescription.OperationTextile},{EnumDepertmentDescription.PlanningMonitoringAndControl},{EnumDepertmentDescription.ProductionManagementControl}) THEN 'Textile Advance'
	                                 WHEN PYB.DepartmentID IN ({EnumDepertmentDescription.ResearchAndDevelopment}) THEN 'R&D'
				                     ELSE '-'
				                     END
			   
	                    FROM M
	                    INNER JOIN {TableNames.PROJECTION_YARN_BOOKING_MASTER} PYB ON PYB.PYBookingID = M.YarnPRFromMasterId
	                    LEFT JOIN {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} BBT ON BBT.PYBookingID = PYB.PYBookingID
	                    LEFT JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = BBT.BuyerID AND B.ContactID > 0
	                    WHERE M.YarnPRFromTableId = {YarnPRFromTable.ProjectionYarnBookingMaster} --ProjectionYarnBookingMaster
	                    GROUP BY M.YarnPRMasterID, PYB.DepartmentID
                    ),
                    FinalList AS
                    (
	                    SELECT M.YarnPRMasterID, YarnPRNo, YarnPRRequiredDate, M.YarnPRByUser,
                        M.YpApproveBy, M.YpRejectBy, M.TriggerPoint, M.Remarks, SendForApproval, Approve,
                        ApproveBy, ApproveDate, Reject, RejectBy, RejectDate, RejectReason, M.ConceptNo, M.BookingNo,
                        M.YarnPRFromID, M.RevisionNo,
                        M.YarnPRDate,
                        M.Source,
                        M.CreateBy,
	                    M.YarnPRFromTableId,
	                    M.YarnPRFromMasterId,
	                    Buyer = CASE 
			                    WHEN M.YarnPRFromTableId = {YarnPRFromTable.ProjectionYarnBookingMaster} THEN PB.Buyer
			                    WHEN C.[Name] ='Select' THEN ''
			                    WHEN C.[Name] is NULL THEN ''
			                    ELSE C.[Name]
			                    END
	                    FROM M
	                    INNER JOIN {TableNames.YarnPRChild} MC on MC.YarnPRMasterID = M.YarnPRMasterID
                        LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM on FCM.ConceptID = MC.ConceptID
                        LEFT JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = FCM.BuyerID AND C.ContactID > 0
	                    LEFT JOIN PB ON PB.YarnPRMasterID = M.YarnPRMasterID
	                    GROUP BY M.YarnPRMasterID, YarnPRNo, YarnPRRequiredDate, M.YarnPRByUser,
                        M.YpApproveBy, M.YpRejectBy, M.TriggerPoint, M.Remarks, SendForApproval, Approve,
                        ApproveBy, ApproveDate, Reject, RejectBy, RejectDate, RejectReason, M.ConceptNo, M.BookingNo,
                        M.YarnPRFromID, M.RevisionNo,
                        M.YarnPRDate,
                        M.Source,
                        M.CreateBy,
	                    M.YarnPRFromTableId,
	                    M.YarnPRFromMasterId,
	                    CASE 
	                    WHEN M.YarnPRFromTableId = {YarnPRFromTable.ProjectionYarnBookingMaster} THEN PB.Buyer
	                    WHEN C.[Name] ='Select' THEN ''
	                    WHEN C.[Name] is NULL THEN ''
	                    ELSE C.[Name]
	                    END
                    )

                    SELECT *, Count(*) Over() TotalRows
                    FROM FinalList";
                }
                else if (status == Status.Additional)
                {
                    sql =
                    $@"With
                    MRChild AS (
	                    SELECT C.FCMRChildID, C.FCMRMasterID
	                    FROM {TableNames.RND_FREE_CONCEPT_MR_CHILD} C WHERE C.IsPR = 1 AND C.Reject = 0 AND C.Acknowledge=0 AND C.FCMRChildID NOT IN (SELECT FCMRChildID FROM {TableNames.YarnPRChild})
                    )
                    ,F As (
                        Select MR.FCMRMasterID,M.ConceptId,ConceptNo,ConceptDate,M.TrialNo [Re-TrialNo],Qty,MR.Remarks
	                    ,KnittingType.TypeName KnittingType,Composition.SegmentValue Composition,Construction.SegmentValue Construction,Technical.TechnicalName,Gsm.SegmentValue Gsm
	                    ,F.ValueName ConceptForName,S.ValueName ConceptStatus, ISG.SubGroupName ItemSubGroup,M.AddedBy,L.Name
                        FROM {TableNames.RND_FREE_CONCEPT_MR_MASTER} MR
                        INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} M ON M.ConceptID=MR.ConceptID
                        LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID=M.KnittingTypeID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID=M.CompositionID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID=M.ConstructionID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID=M.GSMID
                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue F ON F.ValueID = M.ConceptFor
                        LEFT JOIN {TableNames.FabricTechnicalName} Technical ON Technical.TechnicalNameId=M.TechnicalNameId
                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue S ON S.ValueID = M.ConceptStatusID
                        LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID=M.SubGroupID
                        INNER JOIN MRChild ON MRChild.FCMRMasterID = MR.FCMRMasterID
                        LEFT JOIN  {DbNames.EPYSL}..LoginUser L ON L.UserCode = M.AddedBy
                        GROUP By MR.FCMRMasterID, M.ConceptId, ConceptNo, ConceptDate, M.TrialNo, Qty, MR.Remarks, KnittingType.TypeName, Composition.SegmentValue, Construction.SegmentValue,
                        Technical.TechnicalName, Gsm.SegmentValue, F.ValueName, S.ValueName, ISG.SubGroupName,M.AddedBy,L.Name
                    )

                    SELECT *, Count(*) Over() TotalRows
                    FROM F";

                    orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By FCMRMasterID Desc" : paginationInfo.OrderBy;
                }
                else if (status == Status.AwaitingPropose)
                {
                    sql =
                    $@"
                    With MRChild AS
                    (
                        Select FCMRChildID, FCMRMasterID, RevisionStatus From
                        (
                            Select FCC.FCMRChildID, FCC.FCMRMasterID,
                            Case
                                When YPC.ConceptID IS NULL Then 'New'
                                When (CONVERT(Varchar(10), FCM.ConceptID) + CONVERT(Varchar(10), FCM.RevisionNo)) =
                                (CONVERT(Varchar(10), YPC.ConceptID) + CONVERT(Varchar(10), YPM.RevisionNo)) Then 'New'
                                Else 'Revision Pending'
                            End As RevisionStatus

                            FROM {TableNames.RND_FREE_CONCEPT_MR_MASTER} FCM 
                            Inner JOIN {TableNames.RND_FREE_CONCEPT_MR_CHILD} FCC On FCM.FCMRMasterID = FCC.FCMRMasterID
                            Left JOIN {TableNames.YarnPRChild} YPC On YPC.ConceptID = FCM.ConceptID
                            Left JOIN {TableNames.YARN_PR_MASTER} YPM On YPM.YarnPRMasterID = YPC.YarnPRMasterID
                            Where FCC.FCMRChildID NOT IN (SELECT FCMRChildID FROM {TableNames.YarnPRChild})
                            AND FCC.IsPR = 1 AND FCC.Reject = 0 AND FCM.IsComplete=1 --AND FCC.Acknowledge = 1
                        )A Group By FCMRChildID, FCMRMasterID, RevisionStatus
                    ),
                    F As
                    (
                        Select MR.FCMRMasterID, MRChild.RevisionStatus, M.ConceptId, ConceptNo=M.GroupConceptNo, M.ConceptDate, M.TrialNo [Re-TrialNo],
                        M.Qty, MR.Remarks, KnittingType.TypeName KnittingType, Composition.SegmentValue Composition,
                        Construction.SegmentValue Construction, Technical.TechnicalName, Gsm.SegmentValue Gsm, F.ValueName ConceptForName, --S.ValueName ConceptStatus,
                        ISG.SubGroupName ItemSubGroup, M.AddedBy, L.Name,  0 BookingID, '' BookingNo, null BookingDate, Buyer = CASE WHEN M.BuyerID > 0 THEN C.[Name] ELSE '' END,
                        (Case When MR.IsBDS = 0 Then 'Concept' 
	                      When MR.IsBDS = 3 Then 'Fabric Projection Yarn Booking'
		                  Else 'BDS' End) [Source]
                        --'Concept' [Source]
                        FROM {TableNames.RND_FREE_CONCEPT_MR_MASTER} MR
                        INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} M ON M.ConceptID=MR.ConceptID
                        LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID=M.KnittingTypeID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID=M.CompositionID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID=M.ConstructionID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID=M.GSMID
                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue F ON F.ValueID = M.ConceptFor
                        LEFT JOIN {TableNames.FabricTechnicalName} Technical ON Technical.TechnicalNameId=M.TechnicalNameId
                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue S ON S.ValueID = M.ConceptStatusID
                        LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID=M.SubGroupID
                        INNER JOIN MRChild ON MRChild.FCMRMasterID = MR.FCMRMasterID
                        LEFT JOIN {DbNames.EPYSL}..LoginUser L ON L.UserCode = M.AddedBy
                        LEFT JOIN {DbNames.EPYSL}..Contacts  C ON C.ContactID = M.BuyerID
                        --WHERE M.ConceptNo = M.GroupConceptNo
                        GROUP By MR.FCMRMasterID, MRChild.RevisionStatus, M.ConceptId, M.GroupConceptNo, M.ConceptDate, M.TrialNo, M.Qty,
                        MR.Remarks, KnittingType.TypeName, Composition.SegmentValue, Construction.SegmentValue,
                        Technical.TechnicalName, Gsm.SegmentValue, F.ValueName, ISG.SubGroupName, M.AddedBy, L.Name, MR.IsBDS, C.[Name], M.BuyerID --, S.ValueName
                    ),
                    PYBookingChild AS
                    (
                        SELECT PYBC.PYBBookingChildID, PYBC.PYBookingID, SUM(PYBC.Qty)Qty, '' As RevisionStatus
                        FROM {TableNames.PROJECTION_YARN_BOOKING_ITEM_CHILD} PYBC
                        WHERE PYBC.PYBBookingChildID NOT IN (SELECT PYBBookingChildID FROM {TableNames.YarnPRChild})
                        Group By PYBC.PYBBookingChildID, PYBC.PYBookingID
                    ),
                    /*
                    M AS
                    (
                        SELECT 0 FCMRMasterID, PYBookingChild.RevisionStatus, 0 ConceptId, '' ConceptNo, null ConceptDate, 0 [Re-TrialNo],
                        PYBookingChild.Qty, '' Remarks, '' KnittingType, '' Composition, '' Construction, '' TechnicalName, '' Gsm,
                        '' ConceptForName, --'' ConceptStatus,
                        '' ItemSubGroup, 0 AddedBy, E.EmployeeName [Name], M.PYBookingID BookingID, M.PYBookingNo BookingNo,
                        M.PYBookingDate BookingDate, C.[Name] Buyer, 'Projection Yarn Booking' [Source]
                        From {TableNames.PROJECTION_YARN_BOOKING_MASTER} M
                        INNER JOIN PYBookingChild ON PYBookingChild.PYBookingID = M.PYBookingID
                        LEFT JOIN {DbNames.EPYSL}..Contacts  C ON C.ContactID = M.BuyerID
                        LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = M.BookingByID
                        WHERE M.IsApprove=1 AND M.IsAcknowledged=1 AND M.IsReject=0  AND M.IsCancel=0
                    ),
                    */
                    YMCI AS
                    (
	                    Select * FROM {TableNames.YD_MATERIAL_REQUIREMENT_CHILD_ITEM} Where IsPR = 1
                        And YDMaterialRequirementChildItemID NOT IN (SELECT YDMaterialRequirementChildItemID FROM {TableNames.YarnPRChild})
                    ),
                    YRC as
                    (
                        SELECT 0 FCMRMasterID, '' As RevisionStatus, 0 As ConceptId, '' As ConceptNo, Null As ConceptDate, 0 [Re-TrialNo],
                        SUM(YMCI.MRQty) Qty, YMRM.Remarks, '' As KnittingType, '' As Composition, '' As Construction,
                        YMC.FTechnicalName As TechnicalName, '' As Gsm, '' As ConceptForName, '' As ItemSubGroup, YMRM.AddedBy,
                        L.Name, YMRM.YDMaterialRequirementMasterID As BookingID, YMRM.YDMaterialRequirementNo BookingNo, YMRM.MaterialRequirementDate As BookingDate,
                        C.[Name] Buyer, 'Bulk Booking' As [Source]
	                    From YMCI 
                        Inner JOIN {TableNames.YD_MATERIAL_REQUIREMENT_CHILD} YMC On YMCI.YDMaterialRequirementChildID = YMC.YDMaterialRequirementChildID
	                    Inner JOIN {TableNames.YD_MATERIAL_REQUIREMENT_MASTER} YMRM On YMRM.YDMaterialRequirementMasterID = YMC.YDMaterialRequirementMasterID
	                    LEFT JOIN {DbNames.EPYSL}..LoginUser L ON L.UserCode = YMRM.AddedBy
	                    LEFT JOIN {DbNames.EPYSL}..Contacts  C ON C.ContactID = YMRM.BuyerID
	                    Where YMRM.Acknowledge = 1
	                    Group By YMRM.Remarks, YMC.FTechnicalName, YMRM.AddedBy, L.Name, YMRM.YDMaterialRequirementMasterID, YMRM.YDMaterialRequirementNo,
	                    YMRM.MaterialRequirementDate, C.[Name]
                    ),
                    TOTALROW AS
                    (
                        Select * From F
                        --UNION
                        --SELECT * FROM M
                        UNION
                        SELECT * FROM YRC
                    )
                    SELECT *, Count(*) Over() TotalRows
                    FROM TOTALROW ";
                    orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By FCMRMasterID, BookingDate Desc" : paginationInfo.OrderBy;
                }
                else if (status == Status.ROL_BASE_PENDING)
                {
                    sql =
                    $@"
                    With RO AS(
                        SELECT 0 FCMRMasterID, '' As RevisionStatus, 0 As ConceptId, '' As ConceptNo, Null As ConceptDate, 0 [Re-TrialNo],
                        0 Qty, '' Remarks, '' As KnittingType, '' As Composition, '' As Construction,
                        '' TechnicalName, '' As Gsm, '' As ConceptForName, ISG.SubGroupName ItemSubGroup, ROL.AddedBy,
                        L.Name, 0 BookingID, '' BookingNo, null BookingDate, IM.ItemName YarnCategory, ROL.ROSID,
                        '' Buyer, 'ROL Base Booking' As [Source], ROL.ReOrderQty, ROL.ItemMasterID,
                        StockQty = SUM(ISNULL(YSM.PipelineStockQty,0) + ISNULL(YSM.QuarantineStockQty,0) + ISNULL(YSM.AdvanceStockQty,0) + ISNULL(YSM.SampleStockQty,0) + ISNULL(YSM.LeftoverStockQty,0) + ISNULL(YSM.LiabilitiesStockQty,0))
                        --'Concept' [Source]
                        FROM {TableNames.ItemMasterReOrderStatus} ROL 
                        LEFT JOIN {TableNames.YarnStockMaster_New} YSM ON YSM.ItemMasterID = ROL.ItemMasterID AND YSM.CompanyID = ROL.CompanyID
                        INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = ROL.ItemMasterID 
                        LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID=IM.SubGroupID
                        LEFT JOIN {DbNames.EPYSL}..LoginUser L ON L.UserCode = ROL.AddedBy
                        --WHERE M.ConceptNo = M.GroupConceptNo
                        GROUP By L.Name,ISG.SubGroupName, ROL.AddedBy, IM.ItemName, ROL.ReOrderQty, ROL.ROSID, ROL.ItemMasterID
                        )
                        Select *, Count(*) Over() TotalRows FROM RO Where StockQty<=ReOrderQty";
                    orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By ROSID Desc" : paginationInfo.OrderBy;
                }
                else if (status == Status.Revise)
                {
                    sql =
                    $@"WITH M AS (
	                    SELECT M.YarnPRMasterID, YarnPRDate ,YarnPRNo, YarnPRRequiredDate, L.Name [YarnPRByUser], A.Name[YpApproveBy],
                        R.Name[YpRejectBy], EV.ValueName TriggerPoint, M.Remarks, SendForApproval, Approve, ApproveBy, ApproveDate,
                        Reject, RejectBy, RejectDate, RejectReason, M.ConceptNo, M.BookingNo,
                        M.YarnPRFromID, M.RevisionNo,
                        Source = Case 
                        When M.YarnPRFromID={PRFrom.FABRIC_PROJECTION_YARN_BOOKING} then 'Fabric Projection Yarn Booking'
                        else YPFH.YarnPRName 
                        End,
                        CreateBy = CC.Name,
                        M.YarnPRFromTableId,
	                    M.YarnPRFromMasterId
                        from {TableNames.YARN_PR_MASTER} M
                        Inner JOIN {TableNames.YarnPRFrom_HK} YPFH On M.YarnPRFromID = YPFH.YarnPRFromID
                        Inner Join {DbNames.EPYSL}..EntityTypeValue EV On EV.ValueID = M.TriggerPointID
                        LEFT Join {DbNames.EPYSL}..LoginUser L On M.YarnPRBy = L.UserCode
                        LEFT Join {DbNames.EPYSL}..LoginUser A On M.ApproveBy = A.UserCode
                        LEFT Join {DbNames.EPYSL}..LoginUser R On M.RejectBy = R.UserCode
                        LEFT Join {DbNames.EPYSL}..LoginUser CC On M.AddedBy = CC.UserCode
                        WHERE M.NeedRevision = 1
                        Group By M.YarnPRMasterID, YarnPRDate ,YarnPRNo, YarnPRRequiredDate, L.Name, A.Name,
                        R.Name, EV.ValueName, M.Remarks, SendForApproval, Approve, ApproveBy, ApproveDate,
                        Reject, RejectBy, RejectDate, RejectReason, M.ConceptNo, M.BookingNo,
                        M.YarnPRFromID, M.RevisionNo,YPFH.YarnPRName,CC.Name,M.YarnPRFromTableId,
	                    M.YarnPRFromMasterId
                    ),
                    PB AS
                    (
	                    SELECT M.YarnPRMasterID, 
	                    Buyer = CASE WHEN ISNULL(STRING_AGG(B.ShortName, ','),'') <> '' THEN STRING_AGG(B.ShortName, ',')
				                     WHEN PYB.DepartmentID IN ({EnumDepertmentDescription.Knitting},{EnumDepertmentDescription.Operation},{EnumDepertmentDescription.OperationTextile},{EnumDepertmentDescription.PlanningMonitoringAndControl},{EnumDepertmentDescription.ProductionManagementControl}) THEN 'Textile Advance'
	                                 WHEN PYB.DepartmentID IN ({EnumDepertmentDescription.ResearchAndDevelopment}) THEN 'R&D'
				                     ELSE '-'
				                     END
			   
	                    FROM M
	                    INNER JOIN {TableNames.PROJECTION_YARN_BOOKING_MASTER} PYB ON PYB.PYBookingID = M.YarnPRFromMasterId
	                    LEFT JOIN {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} BBT ON BBT.PYBookingID = PYB.PYBookingID
	                    LEFT JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = BBT.BuyerID AND B.ContactID > 0
	                    WHERE M.YarnPRFromTableId = {YarnPRFromTable.ProjectionYarnBookingMaster} --ProjectionYarnBookingMaster
	                    GROUP BY M.YarnPRMasterID, PYB.DepartmentID
                    ),
                    FinalList AS
                    (
	                    SELECT M.YarnPRMasterID, YarnPRNo, YarnPRRequiredDate, M.YarnPRByUser,
                        M.YpApproveBy, M.YpRejectBy, M.TriggerPoint, M.Remarks, SendForApproval, Approve,
                        ApproveBy, ApproveDate, Reject, RejectBy, RejectDate, RejectReason, M.ConceptNo, M.BookingNo,
                        M.YarnPRFromID, M.RevisionNo,
                        M.YarnPRDate,
                        M.Source,
                        M.CreateBy,
	                    M.YarnPRFromTableId,
	                    M.YarnPRFromMasterId,
	                    Buyer = CASE 
			                    WHEN M.YarnPRFromTableId = {YarnPRFromTable.ProjectionYarnBookingMaster} THEN PB.Buyer
			                    WHEN C.[Name] ='Select' THEN ''
			                    WHEN C.[Name] is NULL THEN ''
			                    ELSE C.[Name]
			                    END
	                    FROM M
	                    INNER JOIN {TableNames.YarnPRChild} MC on MC.YarnPRMasterID = M.YarnPRMasterID
                        LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM on FCM.ConceptID = MC.ConceptID
                        LEFT JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = FCM.BuyerID AND C.ContactID > 0
	                    LEFT JOIN PB ON PB.YarnPRMasterID = M.YarnPRMasterID
	                    GROUP BY M.YarnPRMasterID, YarnPRNo, YarnPRRequiredDate, M.YarnPRByUser,
                        M.YpApproveBy, M.YpRejectBy, M.TriggerPoint, M.Remarks, SendForApproval, Approve,
                        ApproveBy, ApproveDate, Reject, RejectBy, RejectDate, RejectReason, M.ConceptNo, M.BookingNo,
                        M.YarnPRFromID, M.RevisionNo,
                        M.YarnPRDate,
                        M.Source,
                        M.CreateBy,
	                    M.YarnPRFromTableId,
	                    M.YarnPRFromMasterId,
	                    CASE 
	                    WHEN M.YarnPRFromTableId = {YarnPRFromTable.ProjectionYarnBookingMaster} THEN PB.Buyer
	                    WHEN C.[Name] ='Select' THEN ''
	                    WHEN C.[Name] is NULL THEN ''
	                    ELSE C.[Name]
	                    END
                    )

                    SELECT *, Count(*) Over() TotalRows
                    FROM FinalList";
                }
                else
                {
                    sql =
                    $@"WITH M AS (
	                    SELECT M.YarnPRMasterID, YarnPRDate ,YarnPRNo, YarnPRRequiredDate, L.Name [YarnPRByUser], A.Name[YpApproveBy],
                        R.Name[YpRejectBy], EV.ValueName TriggerPoint, M.Remarks, SendForApproval, Approve, ApproveBy, ApproveDate,
                        Reject, RejectBy, RejectDate, RejectReason, M.ConceptNo, M.BookingNo,
                        M.YarnPRFromID, M.RevisionNo,
                        Source = Case 
                        When M.YarnPRFromID={PRFrom.FABRIC_PROJECTION_YARN_BOOKING} then 'Fabric Projection Yarn Booking'
                        else YPFH.YarnPRName 
                        End,
                        CreateBy = CC.Name,
	                    M.YarnPRFromTableId,
	                    M.YarnPRFromMasterId
                        from {TableNames.YARN_PR_MASTER} M
                        Inner JOIN {TableNames.YarnPRFrom_HK} YPFH On M.YarnPRFromID = YPFH.YarnPRFromID
                        Inner Join {DbNames.EPYSL}..EntityTypeValue EV On EV.ValueID = M.TriggerPointID
                        LEFT Join {DbNames.EPYSL}..LoginUser L On M.YarnPRBy = L.UserCode
                        LEFT Join {DbNames.EPYSL}..LoginUser A On M.ApproveBy = A.UserCode
                        LEFT Join {DbNames.EPYSL}..LoginUser R On M.RejectBy = R.UserCode
                        LEFT Join {DbNames.EPYSL}..LoginUser CC On M.AddedBy = CC.UserCode
                        WHERE (ISNULL(M.SendForApproval,0) = 0 AND ISNULL(M.Approve, 0) = 0 AND ISNULL(M.Reject, 0) = 0) OR (ISNULL(M.SendForApproval, 0) = 0 AND ISNULL(M.Reject, 0) = 1)
                        Group By M.YarnPRMasterID, YarnPRDate ,YarnPRNo, YarnPRRequiredDate, L.Name, A.Name,
                        R.Name, EV.ValueName, M.Remarks, SendForApproval, Approve, ApproveBy, ApproveDate,
                        Reject, RejectBy, RejectDate, RejectReason, M.ConceptNo, M.BookingNo,
                        M.YarnPRFromID, M.RevisionNo,YPFH.YarnPRName,CC.Name,	M.YarnPRFromTableId,
	                    M.YarnPRFromMasterId
                    ),
                    PB AS
                    (
	                    SELECT M.YarnPRMasterID, 
	                    Buyer = CASE WHEN ISNULL(STRING_AGG(B.ShortName, ','),'') <> '' THEN STRING_AGG(B.ShortName, ',')
				                     WHEN PYB.DepartmentID IN ({EnumDepertmentDescription.Knitting},{EnumDepertmentDescription.Operation},{EnumDepertmentDescription.OperationTextile},{EnumDepertmentDescription.PlanningMonitoringAndControl},{EnumDepertmentDescription.ProductionManagementControl}) THEN 'Textile Advance'
	                                 WHEN PYB.DepartmentID IN ({EnumDepertmentDescription.ResearchAndDevelopment}) THEN 'R&D'
				                     ELSE '-'
				                     END
			   
	                    FROM M
	                    INNER JOIN {TableNames.PROJECTION_YARN_BOOKING_MASTER} PYB ON PYB.PYBookingID = M.YarnPRFromMasterId
	                    LEFT JOIN {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} BBT ON BBT.PYBookingID = PYB.PYBookingID
	                    LEFT JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = BBT.BuyerID AND B.ContactID > 0
	                    WHERE M.YarnPRFromTableId = {YarnPRFromTable.ProjectionYarnBookingMaster} --ProjectionYarnBookingMaster
	                    GROUP BY M.YarnPRMasterID, PYB.DepartmentID
                    ),
                    FinalList AS
                    (
	                    SELECT M.YarnPRMasterID, YarnPRNo, YarnPRRequiredDate, M.YarnPRByUser,
                        M.YpApproveBy, M.YpRejectBy, M.TriggerPoint, M.Remarks, SendForApproval, Approve,
                        ApproveBy, ApproveDate, Reject, RejectBy, RejectDate, RejectReason, M.ConceptNo, M.BookingNo,
                        M.YarnPRFromID, M.RevisionNo,
                        M.YarnPRDate,
                        M.Source,
                        M.CreateBy,
	                    M.YarnPRFromTableId,
	                    M.YarnPRFromMasterId,
	                    Buyer = CASE 
			                    WHEN M.YarnPRFromTableId = {YarnPRFromTable.ProjectionYarnBookingMaster} THEN PB.Buyer
			                    WHEN C.[Name] ='Select' THEN ''
			                    WHEN C.[Name] is NULL THEN ''
			                    ELSE C.[Name]
			                    END
	                    FROM M
	                    INNER JOIN {TableNames.YarnPRChild} MC on MC.YarnPRMasterID = M.YarnPRMasterID
                        LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM on FCM.ConceptID = MC.ConceptID
                        LEFT JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = FCM.BuyerID AND C.ContactID > 0
	                    LEFT JOIN PB ON PB.YarnPRMasterID = M.YarnPRMasterID
	                    GROUP BY M.YarnPRMasterID, YarnPRNo, YarnPRRequiredDate, M.YarnPRByUser,
                        M.YpApproveBy, M.YpRejectBy, M.TriggerPoint, M.Remarks, SendForApproval, Approve,
                        ApproveBy, ApproveDate, Reject, RejectBy, RejectDate, RejectReason, M.ConceptNo, M.BookingNo,
                        M.YarnPRFromID, M.RevisionNo,
                        M.YarnPRDate,
                        M.Source,
                        M.CreateBy,
	                    M.YarnPRFromTableId,
	                    M.YarnPRFromMasterId,
	                    CASE 
	                    WHEN M.YarnPRFromTableId = {YarnPRFromTable.ProjectionYarnBookingMaster} THEN PB.Buyer
	                    WHEN C.[Name] ='Select' THEN ''
	                    WHEN C.[Name] is NULL THEN ''
	                    ELSE C.[Name]
	                    END
                    )

                    SELECT *, Count(*) Over() TotalRows
                    FROM FinalList";
                }
            }

            #endregion PR Page

            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<YarnPRMaster>(sql);
        }

        public async Task<YarnPRMaster> GetNewAsync()
        {
            string query =
                $@" 
                -- Requisition By
                {CommonQueries.GetYarnAndCDAUsersForYarnPR()};

                -- RM Trigger Points
                {CommonQueries.GetEntityTypesByEntityTypeName(EntityTypeNameConstants.RM_TRIGGER_POINTS)}

                ----Company
                ;With BFL As (
	                Select BondFinancialYearID, ImportLimit - Consumption As AvailableLimit
	                From {DbNames.EPYSL}..BondFinancialYearImportLimit
	                Where SubGroupID = 102
                )
                , Al As (
	                Select BFY.CompanyID, AvailableLimit
	                From {DbNames.EPYSL}..BondFinancialYear BFY
	                Inner Join  BFL On BFY.BondFinancialYearID = BFY.BondFinancialYearID
	                Group By BFY.CompanyID, AvailableLimit
                )
                Select Cast (CE.CompanyID as varchar) [id] , CE.ShortName [text]
                From (select SubGroupID, ContactID from {DbNames.EPYSL}..SupplierItemGroupStatus Group By SubGroupID, ContactID) SIGS
                Inner Join {DbNames.EPYSL}..Contacts C On SIGS.ContactID = C.ContactID
                Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = SIGS.SubGroupID
                Inner Join {DbNames.EPYSL}..ContactAdditionalInfo CAI On CAI.ContactID = SIGS.ContactID
                Inner Join {DbNames.EPYSL}..CompanyEntity CE On C.MappingCompanyID = CE.CompanyID
                Where ISG.SubGroupName = 'Fabric' And Isnull(CAI.InHouse,0) = 1 Group by CE.CompanyID, CE.CompanyName, CE.ShortName;

                {CommonQueries.GetYarnSpinners()}

                -- DayValidDuration
                {CommonQueries.GetDayValidDurations()}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);

                YarnPRMaster data = new YarnPRMaster
                {
                    YarnPRByList = await records.ReadAsync<Select2OptionModel>(),
                    TriggerPointList = await records.ReadAsync<Select2OptionModel>(),
                    CompanyList = await records.ReadAsync<Select2OptionModel>(),
                    RefSpinnerList = await records.ReadAsync<Select2OptionModel>()
                };

                data.DayValidDurations = await records.ReadAsync<Select2OptionModel>();
                data.DayValidDurations = CommonFunction.GetDayValidDurations(data.DayValidDurations, string.Join(",", data.Childs.Where(x => x.DayValidDurationId > 0).Select(x => x.DayValidDurationId).Distinct()));
                data.IsCheckDVD = true;


                data.Childs.ForEach(x =>
                {
                    var dayObj = data.DayValidDurations.ToList().Find(d => d.id == x.DayValidDurationId.ToString());
                    if (dayObj.IsNotNull())
                    {
                        x.DayValidDurationName = dayObj.text;
                        x.DayDuration = Convert.ToInt32(dayObj.additionalValue);
                    }
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

        public async Task<YarnPRMaster> GetPRByYarnPRFromTable(int yarnPRFromTableId, int yarnPRFromMasterId)
        {
            string query = $@"SELECT PRM.*
                            FROM {TableNames.YARN_PR_MASTER} PRM
                            WHERE PRM.YarnPRFromTableId = {yarnPRFromTableId}
                            AND PRM.YarnPRFromMasterId = {yarnPRFromMasterId}

                            ;SELECT PRC.*
                            FROM {TableNames.YarnPRChild} PRC
                            INNER JOIN {TableNames.YARN_PR_MASTER} PRM ON PRM.YarnPRMasterID = PRC.YarnPRMasterID
                            WHERE PRM.YarnPRFromTableId = {yarnPRFromTableId}
                            AND PRM.YarnPRFromMasterId = {yarnPRFromMasterId}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                YarnPRMaster data = records.Read<YarnPRMaster>().FirstOrDefault();
                if (data.IsNull()) data = new YarnPRMaster();
                data.Childs = records.Read<YarnPRChild>().ToList();
                if (data.Childs.IsNull()) data.Childs = new List<YarnPRChild>();
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
        public async Task<YarnPRMaster> GetNewForMR(string iDs, string source, string revisionstatus)
        {
            string sql = "";
            if ((source == PRFromName.CONCEPT || source == PRFromName.BDS || source == PRFromName.BULK_BOOKING || source == PRFromName.FABRIC_PROJECTION_YARN_BOOKING) && revisionstatus == "New")
            {
                sql = $@"-- Childs
                 Select ROW_NUMBER() Over(Order By(FCC.ItemMasterID)) YarnPRChildID, FCC.ItemMasterID, SUM(FCC.ReqQty) ReqQty, 
				MAX(FCC.ReqCone) ReqCone, PurchaseQty = SUM(FCC.ReqQty), FCC.ShadeCode, FCM.CompanyID As FPRCompanyID, CE.ShortName As FPRCompanyName, 
				FCM.ConceptID,FCC.FCMRChildID,
                IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, IM.Segment4ValueID, IM.Segment5ValueID, 
				IM.Segment6ValueID, IM.Segment7ValueID, ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, 
				ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc, 
				ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc, 'BDS' As [Source], FCM.ConceptNo, FCM.GroupConceptNo,
				FBA.BaseTypeId, FCC.DayValidDurationId
                FROM {TableNames.RND_FREE_CONCEPT_MR_CHILD} FCC
                INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_MASTER} FCMR ON FCMR.FCMRMasterID = FCC.FCMRMasterID
                INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = FCMR.ConceptID
				LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FBAC ON FBAC.BookingChildID = FCM.BookingChildID AND FBAC.BookingID = FCM.BookingID
                LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.FBAckID = FBAC.AcknowledgeID AND FBAC.BookingID = FCM.BookingID
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = FCC.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                Left Join {DbNames.EPYSL}..CompanyEntity CE On CE.CompanyID = FCM.CompanyID
                Where FCM.GroupConceptNo IN ({iDs}) AND FCC.IsPR = 1 AND FCC.FCMRChildID NOT IN (SELECT FCMRChildID FROM {TableNames.YarnPRChild})
				GROUP BY FCC.ItemMasterID, FCC.ShadeCode, FCM.CompanyID, FCM.ConceptNo, FCM.GroupConceptNo, CE.ShortName, IM.Segment1ValueID, IM.Segment2ValueID, 
				FCM.ConceptID,FCC.FCMRChildID,
                IM.Segment3ValueID, IM.Segment4ValueID, IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID, ISV1.SegmentValue, 
				ISV2.SegmentValue, ISV3.SegmentValue, ISV4.SegmentValue, ISV5.SegmentValue, ISV6.SegmentValue, ISV7.SegmentValue,FBA.BaseTypeId, FCC.DayValidDurationId;";
            }
            else if (source == PRFromName.PROJECTION_YARN_BOOKING)
            {
                sql = $@"-- Childs
                With C As (
	                Select * FROM {TableNames.PROJECTION_YARN_BOOKING_ITEM_CHILD} WHERE PYBookingID IN ({iDs})
                )
                SELECT C.PYBBookingChildID, C.ItemMasterID ItemMasterID, C.QTY ReqQty, C.Remarks, PM.CompanyID AS FPRCompanyID, CE.ShortName As FPRCompanyName
	                , C.UnitID, PM.PYBookingID BookingID, PM.PYBookingNo BookingNo, PM.PYBookingDate BookingDate
					, IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, IM.Segment4ValueID, IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID
	                , ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc
	                , ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc, PM.BaseTypeId,
                    '{source}' As [Source]
                FROM C
				INNER JOIN {TableNames.PROJECTION_YARN_BOOKING_MASTER} PM ON PM.PYBookingID = C.PYBookingID
                Left Join {DbNames.EPYSL}..ItemMaster IM On C.ItemMasterID = IM.ItemMasterID
                Left Join {DbNames.EPYSL}..ItemSegmentValue ISV1 On IM.Segment1ValueID = ISV1.SegmentValueID
                Left Join {DbNames.EPYSL}..ItemSegmentValue ISV2 On IM.Segment2ValueID = ISV2.SegmentValueID
                Left Join {DbNames.EPYSL}..ItemSegmentValue ISV3 On IM.Segment3ValueID = ISV3.SegmentValueID
                Left Join {DbNames.EPYSL}..ItemSegmentValue ISV4 On IM.Segment4ValueID = ISV4.SegmentValueID
                Left Join {DbNames.EPYSL}..ItemSegmentValue ISV5 On IM.Segment5ValueID = ISV5.SegmentValueID
                Left Join {DbNames.EPYSL}..ItemSegmentValue ISV6 On IM.Segment6ValueID = ISV6.SegmentValueID
                Left Join {DbNames.EPYSL}..ItemSegmentValue ISV7 On IM.Segment7ValueID = ISV7.SegmentValueID
                Left Join {DbNames.EPYSL}..Unit U On C.UnitID = U.UnitID
                Left Join {DbNames.EPYSL}..CompanyEntity CE On CE.CompanyID = PM.CompanyID ;";
            }
            else if (source == PRFromName.ROL_BASE_BOOKING)
            {
                sql = $@"-- Childs
                
                SELECT PYBBookingChildID = 0, IM.ItemMasterID, ReqQty = 0, Remarks = '', FPRCompanyID = ROL.CompanyID, CE.ShortName As FPRCompanyName
	                , UnitID = 28, BookingID = 0, BookingNo = 0, BookingDate = null
					, IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, IM.Segment4ValueID, IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID
	                , ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc
	                , ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc, BaseTypeId = 0,
                    ' ROL Base Booking' As [Source], ROL.MOQ
                    , StockQty = SUM(ISNULL(YSM.PipelineStockQty,0) + ISNULL(YSM.QuarantineStockQty,0) + ISNULL(YSM.AdvanceStockQty,0) + ISNULL(YSM.SampleStockQty,0) + ISNULL(YSM.LeftoverStockQty,0) + ISNULL(YSM.LiabilitiesStockQty,0))
                FROM {TableNames.ItemMasterReOrderStatus} ROL
                LEFT JOIN {TableNames.YarnStockMaster_New} YSM ON YSM.ItemMasterID = ROL.ItemMasterID AND YSM.CompanyID = ROL.CompanyID
				INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = ROL.ItemMasterID
                Left Join {DbNames.EPYSL}..ItemSegmentValue ISV1 On IM.Segment1ValueID = ISV1.SegmentValueID
                Left Join {DbNames.EPYSL}..ItemSegmentValue ISV2 On IM.Segment2ValueID = ISV2.SegmentValueID
                Left Join {DbNames.EPYSL}..ItemSegmentValue ISV3 On IM.Segment3ValueID = ISV3.SegmentValueID
                Left Join {DbNames.EPYSL}..ItemSegmentValue ISV4 On IM.Segment4ValueID = ISV4.SegmentValueID
                Left Join {DbNames.EPYSL}..ItemSegmentValue ISV5 On IM.Segment5ValueID = ISV5.SegmentValueID
                Left Join {DbNames.EPYSL}..ItemSegmentValue ISV6 On IM.Segment6ValueID = ISV6.SegmentValueID
                Left Join {DbNames.EPYSL}..ItemSegmentValue ISV7 On IM.Segment7ValueID = ISV7.SegmentValueID
                Left Join {DbNames.EPYSL}..CompanyEntity CE On CE.CompanyID = ROL.CompanyID 
				Where ROL.ROSID IN ({iDs})
                GROUP BY  IM.ItemMasterID, ROL.CompanyID, CE.ShortName
					, IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, IM.Segment4ValueID, IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID
	                , ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue, ISV4.SegmentValue
	                , ISV5.SegmentValue, ISV6.SegmentValue, ISV7.SegmentValue, ROL.MOQ;";
            }
            else if (source == PRFromName.CONCEPT && revisionstatus == "Revision Pending")
            {
                sql = $@"
                Select MMM.YarnPRMasterID, YarnPRDate, YarnPRRequiredDate, YarnPRNo, Remarks, YarnPRBy, SubGroupID,
                SendForApproval, Approve, Reject, RejectReason, TriggerPointID, IsRNDPR, YarnPRFromID, '{revisionstatus}' As RevisionStatus, AdditionalNo
                FROM {TableNames.YARN_PR_MASTER} MMM
                -- Where MMM.YarnPRMasterID = iDs;
                Where MMM.YarnPRMasterID In (Select Distinct YPC.YarnPRMasterID
                                            FROM {TableNames.RND_FREE_CONCEPT_MR_MASTER} FCM 
							                Inner Join {TableNames.RND_FREE_CONCEPT_MASTER} FC On FC.ConceptID = FCM.ConceptID
							                Inner Join {TableNames.YarnPRChild} YPC On FCM.ConceptID = YPC.ConceptID 
                                            Where FC.ConceptNo IN ({iDs})) ;

                -- Child Data
                ;WITH YRC As (
	                Select FCC.FCMRChildID As YarnPRChildID, FCC.FCMRChildID, FCM.FCMRMasterID As YarnPRMasterID, YPC.SetupChildID, FCM.ConceptID, FCC.ItemMasterID,
	                YPC.Remarks, (Case When Isnull(YPC.ReqQty, 0) = 0 Then FCC.ReqQty Else YPC.ReqQty End)ReqQty,
	                (Case When Isnull(YPC.ReqCone, 0) = 0 Then FCC.ReqCone Else YPC.ReqCone End)ReqCone, YPC.ShadeCode, FCC.UnitID, 'Kg' AS DisplayUnitDesc,
	                YPC.FPRCompanyID, YPC.PYBBookingChildID, BaseTypeId = ISNULL(YPC.BaseTypeId,0), FCC.DayValidDurationId
	                FROM {TableNames.RND_FREE_CONCEPT_MR_MASTER} FCM 
                    Inner JOIN {TableNames.RND_FREE_CONCEPT_MR_CHILD} FCC On FCM.FCMRMasterID = FCC.FCMRMasterID
                    Inner Join {TableNames.RND_FREE_CONCEPT_MASTER} FC On FC.ConceptID = FCM.ConceptID  
	                Left Join {TableNames.YarnPRChild} YPC ON YPC.FCMRChildID = FCC.FCMRChildID And YPC.ConceptID = FCM.ConceptID
	                Where FC.ConceptNo IN ({iDs}) AND FCC.IsPR = 1 --YarnPRMasterID = 45
                )
                Select YRC.YarnPRChildID, YRC.FCMRChildID, YRC.YarnPRMasterID, YRC.SetupChildID, YRC.ConceptID, YRC.ItemMasterID,
                YRC.Remarks, YRC.ReqQty, YRC.ReqCone, YRC.ShadeCode, YRC.UnitID, YRC.DisplayUnitDesc,
                YRC.FPRCompanyID, CE.ShortName FPRCompanyName, FCM.ConceptNo, FCM.GroupConceptNo, PYBM.PYBookingNo BookingNo,
                IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, IM.Segment4ValueID, IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID,
                IM.Segment8ValueID, IM.Segment9ValueID, IM.Segment10ValueID, IM.Segment11ValueID, IM.Segment12ValueID, IM.Segment13ValueID, IM.Segment14ValueID,
                IM.Segment15ValueID, ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc,
                ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc,
                '{source}' As [Source], YRC.BaseTypeId, YRC.DayValidDurationId
                From YRC
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YRC.ItemMasterID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                LEFT JOIN {DbNames.EPYSL}..CompanyEntity CE ON YRC.FPRCompanyID = CE.CompanyID
                LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = YRC.ConceptID
                LEFT JOIN {TableNames.PROJECTION_YARN_BOOKING_ITEM_CHILD} PYBC ON PYBC.PYBBookingChildID = YRC.PYBBookingChildID
                LEFT JOIN {TableNames.PROJECTION_YARN_BOOKING_MASTER} PYBM ON PYBM.PYBookingID = PYBC.PYBookingID;  ";
            }
            else if (source == PRFromName.BDS && revisionstatus == "Revision Pending")
            {
                sql = $@"
                Select MMM.YarnPRMasterID, YarnPRDate, YarnPRRequiredDate, YarnPRNo, Remarks, YarnPRBy, SubGroupID,
                SendForApproval, Approve, Reject, RejectReason, TriggerPointID, IsRNDPR, YarnPRFromID, '{revisionstatus}' As RevisionStatus, AdditionalNo
                FROM {TableNames.YARN_PR_MASTER} MMM
                -- Where MMM.YarnPRMasterID = iDs;
                Where MMM.YarnPRMasterID In (Select Distinct YPC.YarnPRMasterID
                                            FROM {TableNames.RND_FREE_CONCEPT_MR_MASTER} FCM 
							                Inner Join {TableNames.RND_FREE_CONCEPT_MASTER} FC On FC.ConceptID = FCM.ConceptID
							                Inner Join {TableNames.YarnPRChild} YPC On FCM.ConceptID = YPC.ConceptID 
                                            Where FC.ConceptNo IN ({iDs})) ;

                -- Child Data
                ;WITH YRC As (
	                Select FCC.FCMRChildID As YarnPRChildID, FCC.FCMRChildID, FCM.FCMRMasterID As YarnPRMasterID, YPC.SetupChildID, FCM.ConceptID, FCC.ItemMasterID,
	                YPC.Remarks, (Case When Isnull(YPC.ReqQty, 0) = 0 Then FCC.ReqQty Else YPC.ReqQty End)ReqQty,
	                (Case When Isnull(YPC.ReqCone, 0) = 0 Then FCC.ReqCone Else YPC.ReqCone End)ReqCone, YPC.ShadeCode, FCC.UnitID, 'Kg' AS DisplayUnitDesc,
	                YPC.FPRCompanyID, YPC.PYBBookingChildID, BaseTypeId = ISNULL(YPC.BaseTypeId,0), FCC.DayValidDurationId
	                FROM {TableNames.RND_FREE_CONCEPT_MR_MASTER} FCM 
	                Inner JOIN {TableNames.RND_FREE_CONCEPT_MR_CHILD} FCC On FCM.FCMRMasterID = FCC.FCMRMasterID
	                Inner Join {TableNames.RND_FREE_CONCEPT_MASTER} FC On FC.ConceptID = FCM.ConceptID  
	                Left Join {TableNames.YarnPRChild} YPC ON YPC.FCMRChildID = FCC.FCMRChildID And YPC.ConceptID = FCM.ConceptID  
	                Where FC.ConceptNo IN ({iDs}) AND FCC.IsPR = 1 --YarnPRMasterID = 45
                )
                Select YRC.YarnPRChildID, YRC.FCMRChildID, YRC.YarnPRMasterID, YRC.SetupChildID, YRC.ConceptID, YRC.ItemMasterID,
                YRC.Remarks, YRC.ReqQty, YRC.ReqCone, YRC.ShadeCode, YRC.UnitID, YRC.DisplayUnitDesc,
                YRC.FPRCompanyID, CE.ShortName FPRCompanyName, FCM.ConceptNo, FCM.GroupConceptNo, PYBM.PYBookingNo BookingNo,
                IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, IM.Segment4ValueID, IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID,
                IM.Segment8ValueID, IM.Segment9ValueID, IM.Segment10ValueID, IM.Segment11ValueID, IM.Segment12ValueID, IM.Segment13ValueID, IM.Segment14ValueID,
                IM.Segment15ValueID, ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc,
                ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc,
                '{source}' As [Source], YRC.BaseTypeId, YRC.DayValidDurationId
                From YRC
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YRC.ItemMasterID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                LEFT JOIN {DbNames.EPYSL}..CompanyEntity CE ON YRC.FPRCompanyID = CE.CompanyID
                LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = YRC.ConceptID
                LEFT JOIN {TableNames.PROJECTION_YARN_BOOKING_ITEM_CHILD} PYBC ON PYBC.PYBBookingChildID = YRC.PYBBookingChildID
                LEFT JOIN {TableNames.PROJECTION_YARN_BOOKING_MASTER} PYBM ON PYBM.PYBookingID = PYBC.PYBookingID;  ";
            }
            sql +=
                $@"
                -- Requisition By
                {CommonQueries.GetYarnAndCDAUsers()};

                -- RM Trigger Points
                {CommonQueries.GetEntityTypesByEntityTypeName(EntityTypeNameConstants.RM_TRIGGER_POINTS)}

                ----Company
                ;With BFL As (
	                Select BondFinancialYearID, ImportLimit - Consumption As AvailableLimit
	                From {DbNames.EPYSL}..BondFinancialYearImportLimit
	                Where SubGroupID = 102
                )
                , Al As (
	                Select BFY.CompanyID, AvailableLimit
	                From {DbNames.EPYSL}..BondFinancialYear BFY
	                Inner Join  BFL On BFY.BondFinancialYearID = BFY.BondFinancialYearID
	                Group By BFY.CompanyID, AvailableLimit
                )
                Select Cast (CE.CompanyID as varchar) [id] , CE.ShortName [text]
                From (select SubGroupID, ContactID from {DbNames.EPYSL}..SupplierItemGroupStatus Group By SubGroupID, ContactID) SIGS
                Inner Join {DbNames.EPYSL}..Contacts C On SIGS.ContactID = C.ContactID
                Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = SIGS.SubGroupID
                Inner Join {DbNames.EPYSL}..ContactAdditionalInfo CAI On CAI.ContactID = SIGS.ContactID
                Inner Join {DbNames.EPYSL}..CompanyEntity CE On C.MappingCompanyID = CE.CompanyID
                Where ISG.SubGroupName = 'Fabric' And Isnull(CAI.InHouse,0) = 1 Group by CE.CompanyID, CE.CompanyName, CE.ShortName;

                {CommonQueries.GetYarnSpinners()}

                -- DayValidDuration
                {CommonQueries.GetDayValidDurations()}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YarnPRMaster data = new YarnPRMaster();

                if (revisionstatus == "Revision Pending")
                {
                    data = records.Read<YarnPRMaster>().FirstOrDefault();
                    data.Childs = records.Read<YarnPRChild>().ToList();
                    data.ConceptNo = data.Childs.Count() > 0 ? data.Childs.First().ConceptNo : "";
                    data.GroupConceptNo = data.Childs.Count() > 0 ? data.Childs.First().GroupConceptNo : "";
                    data.YarnPRByList = await records.ReadAsync<Select2OptionModel>();
                    data.TriggerPointList = await records.ReadAsync<Select2OptionModel>();
                    data.CompanyList = await records.ReadAsync<Select2OptionModel>();
                    data.RefSpinnerList = await records.ReadAsync<Select2OptionModel>();
                }
                else
                {
                    data.Childs = records.Read<YarnPRChild>().ToList();
                    data.ConceptNo = data.Childs.Count() > 0 ? data.Childs.First().ConceptNo : "";
                    data.GroupConceptNo = data.Childs.Count() > 0 ? data.Childs.First().GroupConceptNo : "";
                    data.YarnPRByList = await records.ReadAsync<Select2OptionModel>();
                    data.TriggerPointList = await records.ReadAsync<Select2OptionModel>();
                    data.CompanyList = await records.ReadAsync<Select2OptionModel>();
                    data.RefSpinnerList = await records.ReadAsync<Select2OptionModel>();
                }

                data.DayValidDurations = await records.ReadAsync<Select2OptionModel>();
                data.DayValidDurations = CommonFunction.GetDayValidDurations(data.DayValidDurations, string.Join(",", data.Childs.Where(x => x.DayValidDurationId > 0).Select(x => x.DayValidDurationId).Distinct()));
                data.IsCheckDVD = data.YarnPRDate < CommonConstent.YarnSourcingModeImplementDate ? false : true;


                data.Childs.ForEach(x =>
                {
                    var dayObj = data.DayValidDurations.ToList().Find(d => d.id == x.DayValidDurationId.ToString());
                    x.Source = x.Source.IsNullOrEmpty() ? source : x.Source;
                    if (dayObj.IsNotNull())
                    {
                        x.DayValidDurationName = dayObj.text;
                        x.DayDuration = Convert.ToInt32(dayObj.additionalValue);
                    }
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

        public async Task<List<Select2OptionModel>> GetYarnCompositionsAsync(string fiberType, string yarnType)
        {
            var records = await _service.GetDataAsync<Select2OptionModel>(CommonQueries.GetItemSegmentValuesBySegmentName(ItemSegmentNameConstants.YARN_COMPOSITION));
            if (fiberType == ItemSegmentValueConstants.BLENDED || fiberType == ItemSegmentValueConstants.COLOR_MELLANGE)
            {
                var yarnTypes = yarnType.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                records = records.FindAll(x => yarnTypes.All(x.text.Contains));
            }
            else
            {
                records = records.FindAll(x => x.text.Trim().Equals("100%"));
            }

            return records;
        }

        public async Task<YarnPRMaster> GetAsync(int id, int prFromID, string source, bool isNewForPRAck)
        {
            string bulkCondition = "";
            //if (prFromID == 3)//Bulk Booking
            //{

            //}
            if (!isNewForPRAck)
            {
                bulkCondition = $@", PurchaseQty = SUM(YRC.ReqQty) ";
            }
            else
            {
                bulkCondition = $@", PurchaseQty = SUM(YRC.PurchaseQty), AllocationQty = SUM(YRC.AllocationQty)";
            }
            string query = $@"
                -- Master Data
                --Select MMM.YarnPRMasterID, YarnPRDate, YarnPRRequiredDate, YarnPRNo, ConceptNo,YarnPRNo [GroupConceptNo],Remarks, YarnPRBy, SubGroupID,
                --SendForApproval, Approve, Reject, RejectReason, TriggerPointID, IsRNDPR, YarnPRFromID,E.EmployeeName as YarnPRByName
                --FROM {TableNames.YARN_PR_MASTER} MMM
                --Inner Join {DbNames.EPYSL}..LoginUser L ON MMM.YarnPRBy=L.UserCode
                --Inner Join {DbNames.EPYSL}..Employee E on L.EmployeeCode=E.EmployeeCode
                --Where MMM.YarnPRMasterID = {id};

                Select MMM.YarnPRMasterID, YarnPRDate, YarnPRRequiredDate, YarnPRNo, MMM.ConceptNo,
                GroupConceptNo = CASE WHEN ISNULL(YC.ConceptID,0) > 0 THEN FCM.GroupConceptNo ELSE '' END,
                MMM.Remarks, YarnPRBy, MMM.SubGroupID,
                SendForApproval, Approve, Reject, RejectReason, TriggerPointID, IsRNDPR, YarnPRFromID,E.EmployeeName as YarnPRByName,
                Buyer = CASE WHEN ISNULL(FCM.BuyerID,0) > 0 THEN C.Name ELSE 'R&D' END,
                BuyerTeam = CASE WHEN ISNULL(FCM.BuyerTeamID,0) > 0 THEN CCT.TeamName ELSE 'R&D' END
                FROM {TableNames.YARN_PR_MASTER} MMM
                left Join {DbNames.EPYSL}..LoginUser L ON MMM.YarnPRBy=L.UserCode
                left Join {DbNames.EPYSL}..Employee E on L.EmployeeCode=E.EmployeeCode
				Inner Join {TableNames.YarnPRChild} YC on MMM.YarnPRMasterID=YC.YarnPRMasterID
				left Join {TableNames.RND_FREE_CONCEPT_MASTER} FCM on FCM.ConceptID=YC.ConceptID
                LEFT JOIN {DbNames.EPYSL}..Contacts  C ON C.ContactID = FCM.BuyerID
                LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT  ON CCT.CategoryTeamID = FCM.BuyerTeamID
                Where MMM.YarnPRMasterID = {id}
				Group by ISNULL(YC.ConceptID,0), ISNULL(FCM.BuyerID,0),ISNULL(FCM.BuyerTeamID,0),C.Name,CCT.TeamName,MMM.YarnPRMasterID, YarnPRDate, YarnPRRequiredDate, YarnPRNo, MMM.ConceptNo,FCM.GroupConceptNo,MMM.Remarks, YarnPRBy, MMM.SubGroupID,
                SendForApproval, Approve, Reject, RejectReason, TriggerPointID, IsRNDPR, YarnPRFromID,E.EmployeeName;

                -- Child Data
                ;WITH YRC As (
	                Select * From {TableNames.YarnPRChild} Where YarnPRMasterID = {id}
                )
                Select YRC.YarnPRChildID,YRC.YarnCategory, YRC.YarnPRMasterID, YRC.ConceptID, YRC.ItemMasterID, YRC.UnitID, YRC.Remarks, SUM(YRC.ReqQty) ReqQty, MAX(YRC.ReqCone) ReqCone
                    , YRC.ShadeCode, YRC.FPRCompanyID, CE.ShortName FPRCompanyName, IM.Segment1ValueID, IM.Segment2ValueID
					, IM.Segment3ValueID, IM.Segment4ValueID, IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID
					,ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc
					, ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc
					, ISV7.SegmentValue Segment7ValueDesc, FCM.ConceptNo, 
                    GroupConceptNo = CASE WHEN ISNULL(YRC.ConceptID,0) > 0 THEN FCM.GroupConceptNo ELSE '' END,
                    ISNULL(PYBM.PYBookingNo, YMRM.YDMaterialRequirementNo) BookingNo
                    , YRC.RefLotNo, YRC.RefSpinnerID, RefSpinner = CASE WHEN ISNULL(YRC.RefSpinnerID,0) > 0 THEN S.ShortName ELSE '' END {bulkCondition}
                    ,YRC.DayValidDurationId
                From YRC
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YRC.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                LEFT Join {DbNames.EPYSL}..Contacts S On S.ContactID = YRC.RefSpinnerID
                LEFT JOIN {DbNames.EPYSL}..Unit U ON U.UnitID = YRC.UnitID
                LEFT JOIN {DbNames.EPYSL}..CompanyEntity CE ON YRC.FPRCompanyID = CE.CompanyID
				LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = YRC.ConceptID
				LEFT JOIN {TableNames.PROJECTION_YARN_BOOKING_ITEM_CHILD} PYBC ON PYBC.PYBBookingChildID = YRC.PYBBookingChildID
				LEFT JOIN {TableNames.PROJECTION_YARN_BOOKING_MASTER} PYBM ON PYBM.PYBookingID = PYBC.PYBookingID
                LEFT JOIN {TableNames.YD_MATERIAL_REQUIREMENT_CHILD_ITEM} YMRCI ON YMRCI.YDMaterialRequirementChildItemID = YRC.YDMaterialRequirementChildItemID
                LEFT JOIN {TableNames.YD_MATERIAL_REQUIREMENT_MASTER} YMRM ON YMRM.YDMaterialRequirementMasterID = YMRCI.YDMaterialRequirementMasterID
				GROUP BY YRC.YarnPRChildID,YRC.YarnCategory, YRC.YarnPRMasterID, YRC.ConceptID, YRC.ItemMasterID, YRC.UnitID, YRC.Remarks, YRC.ShadeCode, YRC.FPRCompanyID
				, CE.ShortName, IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, IM.Segment4ValueID, IM.Segment5ValueID
				, IM.Segment6ValueID, IM.Segment7ValueID, ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue, ISV4.SegmentValue
				, ISV5.SegmentValue, ISV6.SegmentValue, ISV7.SegmentValue, FCM.ConceptNo, ISNULL(YRC.ConceptID,0), FCM.GroupConceptNo, PYBM.PYBookingNo, YMRM.YDMaterialRequirementNo
                , YRC.RefLotNo, YRC.RefSpinnerID, S.ShortName,YRC.DayValidDurationId;

                -- PR Company
                /*;Select C.YarnPRCompanyID, C.YarnPRChildID, C.YarnPRMasterID, C.CompanyID, C.IsCPR, CE.ShortName CompanyName
                FROM {TableNames.YARN_PR_COMPANY} C
                Inner Join {DbNames.EPYSL}..CompanyEntity CE On C.CompanyID = CE.CompanyID
                Where YarnPRMasterID = {id};*/

                -- Company
                ;With BFL As (
	                Select BondFinancialYearID, ImportLimit - Consumption As AvailableLimit
	                From {DbNames.EPYSL}..BondFinancialYearImportLimit
	                Where SubGroupID = {AppConstants.ITEM_SUB_GROUP_YARN_LIVE}
                )
                , Al As (
	                Select BFY.CompanyID, AvailableLimit
	                From {DbNames.EPYSL}..BondFinancialYear BFY
	                Inner Join  BFL On BFY.BondFinancialYearID = BFY.BondFinancialYearID
	                Group By BFY.CompanyID, AvailableLimit
                )
                Select Cast (CE.CompanyID as varchar) [id] , CE.ShortName [text]
                From (select SubGroupID, ContactID from {DbNames.EPYSL}..SupplierItemGroupStatus Group By SubGroupID, ContactID) SIGS
                Inner Join {DbNames.EPYSL}..Contacts C On SIGS.ContactID = C.ContactID
                Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = SIGS.SubGroupID
                Inner Join {DbNames.EPYSL}..ContactAdditionalInfo CAI On CAI.ContactID = SIGS.ContactID
                Inner Join {DbNames.EPYSL}..CompanyEntity CE On C.MappingCompanyID = CE.CompanyID
                Where ISG.SubGroupName = 'Fabric' And Isnull(CAI.InHouse,0) = 1 ------And CompanyID In (Select CompanyID From Al Where AvailableLimit >=  (Select SUM(ReqQty) ReqQty From {TableNames.YarnPRChild} Where YarnPRMasterID = {id}))
                Group by CE.CompanyID, CE.CompanyName, CE.ShortName;

                --Requisition By
                {CommonQueries.GetYarnAndCDAUsersForYarnPR()};

                -- RM Trigger Points
                {CommonQueries.GetEntityTypesByEntityTypeName(EntityTypeNameConstants.RM_TRIGGER_POINTS)}

                {CommonQueries.GetYarnSpinners()}

                -- DayValidDuration
                {CommonQueries.GetDayValidDurations()}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                YarnPRMaster data = records.Read<YarnPRMaster>().FirstOrDefault();
                if (data != null)
                {
                    data.Childs = records.Read<YarnPRChild>().ToList();
                    data.CompanyList = await records.ReadAsync<Select2OptionModel>();
                    data.YarnPRByList = await records.ReadAsync<Select2OptionModel>();
                    data.TriggerPointList = await records.ReadAsync<Select2OptionModel>();
                    data.RefSpinnerList = await records.ReadAsync<Select2OptionModel>();
                }
                if (data.GroupConceptNo == "")
                {
                    data.GroupConceptNo = data.Childs.Count() > 0 ? data.Childs.First().GroupConceptNo : "";
                    data.GroupConceptNo = data.GroupConceptNo == "" ? data.Childs.First().BookingNo : data.GroupConceptNo;
                }
                if (data.ConceptNo == "")
                {
                    data.ConceptNo = data.Childs.Count() > 0 ? data.Childs.First().ConceptNo : "";
                }
                data.DayValidDurations = await records.ReadAsync<Select2OptionModel>();
                data.DayValidDurations = CommonFunction.GetDayValidDurations(data.DayValidDurations, string.Join(",", data.Childs.Where(x => x.DayValidDurationId > 0).Select(x => x.DayValidDurationId).Distinct()));
                data.IsCheckDVD = data.YarnPRDate < CommonConstent.YarnSourcingModeImplementDate ? false : true;

                data.Childs.ForEach(x =>
                {
                    var dayObj = data.DayValidDurations.ToList().Find(d => d.id == x.DayValidDurationId.ToString());
                    x.Source = x.Source.IsNullOrEmpty() ? source : x.Source;
                    if (dayObj.IsNotNull())
                    {
                        x.DayValidDurationName = dayObj.text;
                        x.DayDuration = Convert.ToInt32(dayObj.additionalValue);
                    }
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
        public async Task<List<YarnPRMaster>> GetByPRNo(string prNo)
        {
            string sql =
                $@"SELECT PR.* 
                FROM {TableNames.YARN_PR_MASTER} PR
                WHERE PR.YarnPRNo = '{prNo}'";

            return await _service.GetDataAsync<YarnPRMaster>(sql);
        }
        public async Task<YarnPRMaster> GetForReviseAsync(int id, int prFromID, string source, string groupConceptNo)
        {
            string query = $@"
                -- Master Data
                Select MMM.YarnPRMasterID, YarnPRDate, YarnPRRequiredDate, YarnPRNo, ConceptNo,ConceptNo [GroupConceptNo],Remarks, YarnPRBy, SubGroupID,
                SendForApproval, Approve, Reject, RejectReason, TriggerPointID, IsRNDPR, YarnPRFromID
                FROM {TableNames.YARN_PR_MASTER} MMM
                Where MMM.YarnPRMasterID = {id};

                -- Child Data
                ;WITH 
                MRC AS
                (
	                SELECT FCMR.ConceptID
	                FROM {TableNames.RND_FREE_CONCEPT_MR_CHILD} FCC
	                INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_MASTER} FCMR ON FCMR.FCMRMasterID = FCC.FCMRMasterID
	                INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = FCMR.ConceptID
	                WHERE FCM.GroupConceptNo IN ('{groupConceptNo}') AND FCC.IsPR = 1
	                Group By FCMR.ConceptID
                ),
                T As (
	                Select ConceptID
	                From MRC
	                Group By ConceptID
                ), 
                FC As(
	                Select C.FCMRChildID, C.ItemMasterID, C.ReqQty, C.ReqCone, C.ShadeCode, T.ConceptID, C.SetupChildID,
	                C.UnitID, C.DayValidDurationId
	                From T
	                Inner JOIN {TableNames.RND_FREE_CONCEPT_MR_MASTER} F On F.ConceptID = T.ConceptID
	                Inner JOIN {TableNames.RND_FREE_CONCEPT_MR_CHILD} C On C.FCMRMasterID = F.FCMRMasterID
	                Where C.IsPR = 1
                ),			
                YRC As (
	                Select PC.YarnPRChildID, ISNULL(PC.YarnPRMasterID,{id}) YarnPRMasterID, FC.SetupChildID, FC.ConceptID, FC.FCMRChildID, ISNULL(PC.Remarks,'') Remarks, FC.ShadeCode, PC.HSCode, 
	                FC.UnitID, PC.FPRCompanyID, PC.YarnCategory, PC.PYBBookingChildID, PC.YDMaterialRequirementChildItemID,
	                FC.ItemMasterID, FC.ReqQty, FC.ReqCone, PC.RefLotNo, PC.RefSpinnerID, FC.DayValidDurationId
	                From FC
	                Left JOIN {TableNames.YarnPRChild} PC ON PC.FCMRChildID = FC.FCMRChildID AND PC.YarnPRMasterID = {id}
                )

                Select YarnPRChildID = ISNULL(YRC.YarnPRChildID,0), YRC.YarnPRMasterID,YRC.YarnCategory, YRC.ConceptID, YRC.ItemMasterID, YRC.UnitID, YRC.Remarks, SUM(YRC.ReqQty) ReqQty, MAX(YRC.ReqCone) ReqCone
                , YRC.ShadeCode, YRC.FPRCompanyID, CE.ShortName FPRCompanyName, IM.Segment1ValueID, IM.Segment2ValueID
                , IM.Segment3ValueID, IM.Segment4ValueID, IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID
                , ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc
                , ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc
                , ISV7.SegmentValue Segment7ValueDesc, FCM.ConceptNo, FCM.GroupConceptNo, ISNULL(PYBM.PYBookingNo, YMRM.YDMaterialRequirementNo) BookingNo
                , YRC.RefLotNo, YRC.RefSpinnerID, YRC.FCMRChildID, YRC.DayValidDurationId
                From YRC
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YRC.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                LEFT JOIN {DbNames.EPYSL}..Unit U ON U.UnitID = YRC.UnitID
                LEFT JOIN {DbNames.EPYSL}..CompanyEntity CE ON YRC.FPRCompanyID = CE.CompanyID
                LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = YRC.ConceptID
                LEFT JOIN {TableNames.PROJECTION_YARN_BOOKING_ITEM_CHILD} PYBC ON PYBC.PYBBookingChildID = YRC.PYBBookingChildID
                LEFT JOIN {TableNames.PROJECTION_YARN_BOOKING_MASTER} PYBM ON PYBM.PYBookingID = PYBC.PYBookingID
                LEFT JOIN {TableNames.YD_MATERIAL_REQUIREMENT_CHILD_ITEM} YMRCI ON YMRCI.YDMaterialRequirementChildItemID = YRC.YDMaterialRequirementChildItemID
                LEFT JOIN {TableNames.YD_MATERIAL_REQUIREMENT_MASTER} YMRM ON YMRM.YDMaterialRequirementMasterID = YMRCI.YDMaterialRequirementMasterID
                GROUP BY ISNULL(YRC.YarnPRChildID,0), YRC.YarnPRMasterID,YRC.YarnCategory, YRC.ConceptID, YRC.ItemMasterID, YRC.UnitID, YRC.Remarks, YRC.ShadeCode, YRC.FPRCompanyID
                , CE.ShortName, IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, IM.Segment4ValueID, IM.Segment5ValueID
                , IM.Segment6ValueID, IM.Segment7ValueID, ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue, ISV4.SegmentValue
                , ISV5.SegmentValue, ISV6.SegmentValue, ISV7.SegmentValue, FCM.ConceptNo, FCM.GroupConceptNo, PYBM.PYBookingNo, YMRM.YDMaterialRequirementNo
                , YRC.RefLotNo, YRC.RefSpinnerID, YRC.FCMRChildID, YRC.DayValidDurationId;    
            
                -- PR Company
                /*;Select C.YarnPRCompanyID, C.YarnPRChildID, C.YarnPRMasterID, C.CompanyID, C.IsCPR, CE.ShortName CompanyName
                FROM {TableNames.YARN_PR_COMPANY} C
                Inner Join {DbNames.EPYSL}..CompanyEntity CE On C.CompanyID = CE.CompanyID
                Where YarnPRMasterID = {id};*/

                -- Company
                ;With BFL As (
	                Select BondFinancialYearID, ImportLimit - Consumption As AvailableLimit
	                From {DbNames.EPYSL}..BondFinancialYearImportLimit
	                Where SubGroupID = {AppConstants.ITEM_SUB_GROUP_YARN_LIVE}
                )
                , Al As (
	                Select BFY.CompanyID, AvailableLimit
	                From {DbNames.EPYSL}..BondFinancialYear BFY
	                Inner Join  BFL On BFY.BondFinancialYearID = BFY.BondFinancialYearID
	                Group By BFY.CompanyID, AvailableLimit
                )
                Select Cast (CE.CompanyID as varchar) [id] , CE.ShortName [text]
                From (select SubGroupID, ContactID from {DbNames.EPYSL}..SupplierItemGroupStatus Group By SubGroupID, ContactID) SIGS
                Inner Join {DbNames.EPYSL}..Contacts C On SIGS.ContactID = C.ContactID
                Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = SIGS.SubGroupID
                Inner Join {DbNames.EPYSL}..ContactAdditionalInfo CAI On CAI.ContactID = SIGS.ContactID
                Inner Join {DbNames.EPYSL}..CompanyEntity CE On C.MappingCompanyID = CE.CompanyID
                Where ISG.SubGroupName = 'Fabric' And Isnull(CAI.InHouse,0) = 1 ------And CompanyID In (Select CompanyID From Al Where AvailableLimit >=  (Select SUM(ReqQty) ReqQty From {TableNames.YarnPRChild} Where YarnPRMasterID = {id}))
                Group by CE.CompanyID, CE.CompanyName, CE.ShortName;

                --Requisition By
                {CommonQueries.GetYarnAndCDAUsers()};

                -- RM Trigger Points
                {CommonQueries.GetEntityTypesByEntityTypeName(EntityTypeNameConstants.RM_TRIGGER_POINTS)}

                {CommonQueries.GetYarnSpinners()}

                -- DayValidDuration
                {CommonQueries.GetDayValidDurations()}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                YarnPRMaster data = records.Read<YarnPRMaster>().FirstOrDefault();
                data.Childs = records.Read<YarnPRChild>().ToList();
                int prChildId = 1;

                data.ConceptNo = data.Childs.Count() > 0 ? data.Childs.First().ConceptNo : "";
                data.GroupConceptNo = data.Childs.Count() > 0 ? data.Childs.First().GroupConceptNo : "";
                data.CompanyList = await records.ReadAsync<Select2OptionModel>();
                data.YarnPRByList = await records.ReadAsync<Select2OptionModel>();
                data.TriggerPointList = await records.ReadAsync<Select2OptionModel>();
                data.RefSpinnerList = await records.ReadAsync<Select2OptionModel>();

                data.DayValidDurations = await records.ReadAsync<Select2OptionModel>();
                data.DayValidDurations = CommonFunction.GetDayValidDurations(data.DayValidDurations, string.Join(",", data.Childs.Where(x => x.DayValidDurationId > 0).Select(x => x.DayValidDurationId).Distinct()));
                data.IsCheckDVD = data.YarnPRDate < CommonConstent.YarnSourcingModeImplementDate ? false : true;

                data.Childs.ForEach(x =>
                {
                    if (x.YarnPRChildID == 0) x.YarnPRChildID = prChildId++;
                    x.Source = x.Source.IsNullOrEmpty() ? source : x.Source;
                    x.YarnCategory = CommonFunction.GetYarnShortForm(x.Segment1ValueDesc, x.Segment2ValueDesc, x.Segment3ValueDesc, x.Segment4ValueDesc, x.Segment5ValueDesc, x.Segment6ValueDesc, x.ShadeCode);

                    var dayObj = data.DayValidDurations.ToList().Find(d => d.id == x.DayValidDurationId.ToString());
                    if (dayObj.IsNotNull())
                    {
                        x.DayValidDurationName = dayObj.text;
                        x.DayDuration = Convert.ToInt32(dayObj.additionalValue);
                    }
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

        public async Task<List<YarnPRChild>> GetCommercialCompany(int childPrId)
        {
            string sql =
                $@"SELECT CM.CompanyID, COM.ShortName FPRCompanyName
                FROM {TableNames.YARN_CPR_COMPANY} CM
                INNER JOIN {TableNames.YarnPRChild} C ON C.YarnPRChildID = CM.YarnPRChildID
                INNER JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID = CM.CompanyID
                WHERE C.YarnPRChildID={childPrId}";

            return await _service.GetDataAsync<YarnPRChild>(sql);
        }

        public async Task<List<YarnPRChild>> GetChilds(string conceptNos, string itemIds, string bookingNos)
        {
            string sql = $@"WITH M AS (
                Select FCC.FCMRChildID, FCC.FCMRMasterID, FCMR.ConceptID, FCM.ConceptNo, FCM.GroupConceptNo, 0 PYBBookingChildID, 0 BookingID, '' BookingNo,
                FCC.ItemMasterID, FCC.ReqQty,FCC.ReqCone, FCC.ShadeCode,
                FCM.CompanyID FPRCompanyID, CE.ShortName FPRCompanyName, IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID,
                IM.Segment4ValueID, IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID, IM.Segment8ValueID, ISV1.SegmentValue Segment1ValueDesc,
                ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc,
                ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc
                FROM {TableNames.RND_FREE_CONCEPT_MR_CHILD} FCC
                INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_MASTER} FCMR ON FCMR.FCMRMasterID = FCC.FCMRMasterID
                INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = FCMR.ConceptID
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = FCC.ItemMasterID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                Left Join {DbNames.EPYSL}..CompanyEntity CE On CE.CompanyID = FCM.CompanyID
                Where FCM.ConceptNo IN ({conceptNos}) AND FCC.ItemMasterId IN ({itemIds}) AND FCC.IsPR = 1
				),
				C As (
	                Select PC.PYBBookingChildID, PC.PYBookingID, PC.ItemMasterID, PC.QTY, PC.ShadeCode, PM.PYBookingNo, PM.CompanyID
					FROM {TableNames.PROJECTION_YARN_BOOKING_ITEM_CHILD} PC
					INNER JOIN {TableNames.PROJECTION_YARN_BOOKING_MASTER} PM ON PM.PYBookingID = PC.PYBookingID
					WHERE PM.PYBookingNo IN ({bookingNos}) AND PC.ItemMasterId IN ({itemIds})
                ),
                PY AS (
				SELECT 
				0 FCMRChildID, 0 FCMRMasterID, 0 ConceptID, '' ConceptNo, '' GroupConceptNo, C.PYBBookingChildID, C.PYBookingID BookingID, C.PYBookingNo BookingNo,
                C.ItemMasterID, C.QTY ReqQty, 0 ReqCone, C.ShadeCode,
                C.CompanyID FPRCompanyID, CE.ShortName FPRCompanyName, IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID,
                IM.Segment4ValueID, IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID, IM.Segment8ValueID, ISV1.SegmentValue Segment1ValueDesc,
                ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc,
                ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc
                FROM C
                Left Join {DbNames.EPYSL}..ItemMaster IM On C.ItemMasterID = IM.ItemMasterID
                Left Join {DbNames.EPYSL}..ItemSegmentValue ISV1 On IM.Segment1ValueID = ISV1.SegmentValueID
                Left Join {DbNames.EPYSL}..ItemSegmentValue ISV2 On IM.Segment2ValueID = ISV2.SegmentValueID
                Left Join {DbNames.EPYSL}..ItemSegmentValue ISV3 On IM.Segment3ValueID = ISV3.SegmentValueID
                Left Join {DbNames.EPYSL}..ItemSegmentValue ISV4 On IM.Segment4ValueID = ISV4.SegmentValueID
                Left Join {DbNames.EPYSL}..ItemSegmentValue ISV5 On IM.Segment5ValueID = ISV5.SegmentValueID
                Left Join {DbNames.EPYSL}..ItemSegmentValue ISV6 On IM.Segment6ValueID = ISV6.SegmentValueID
                Left Join {DbNames.EPYSL}..ItemSegmentValue ISV7 On IM.Segment7ValueID = ISV7.SegmentValueID
                Left Join {DbNames.EPYSL}..CompanyEntity CE On CE.CompanyID = C.CompanyID
				),
				CHILD AS (
				SELECT M.*, PC.YarnPRChildID, PC.YarnPRMasterID
				FROM M
				LEFT JOIN {TableNames.YarnPRChild} PC ON PC.FCMRChildID = M.FCMRChildID
				UNION
				SELECT PY.*, PC.YarnPRChildID, PC.YarnPRMasterID
				FROM PY
				LEFT JOIN {TableNames.YarnPRChild} PC ON PC.PYBBookingChildID = PY.PYBBookingChildID
				)
				SELECT *
				FROM CHILD";
            return await _service.GetDataAsync<YarnPRChild>(sql);
        }

        public async Task SaveAsync(YarnPRMaster yarnPRMaster, int userId)
        {
            SqlTransaction transaction = null;
            SqlTransaction transactionGmt = null;

            try
            {
                //Backup table data save before YarnPR data update.
                //await _service.ExecuteAsync("spBackupYarnPR", new { YarnPRMasterID = yarnPRMaster.YarnPRMasterID }, 30, CommandType.StoredProcedure);

                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                await _connectionGmt.OpenAsync();
                transactionGmt = _connectionGmt.BeginTransaction();

                switch (yarnPRMaster.EntityState)
                {
                    case EntityState.Added:
                        yarnPRMaster = await AddAsync(yarnPRMaster, transaction, transactionGmt, _connection, _connectionGmt);
                        break;

                    case EntityState.Modified:
                        yarnPRMaster = await UpdateAsync(yarnPRMaster, transaction, transactionGmt, _connection, _connectionGmt);
                        break;

                    default:
                        break;
                }

                if (yarnPRMaster.Status == Status.Revise)
                {
                    await _connection.ExecuteAsync(SPNames.spBackupYarnPR, new { YarnPRMasterID = yarnPRMaster.YarnPRMasterID }, transaction, 30, CommandType.StoredProcedure);
                }

                await _service.SaveSingleAsync(yarnPRMaster, transaction);
                await _service.SaveAsync(yarnPRMaster.Childs, transaction);
                if (yarnPRMaster.Status != Status.ROL_BASE_PENDING)
                {
                    foreach (YarnPRChild item in yarnPRMaster.Childs)
                    {

                        await _connection.ExecuteAsync(SPNames.sp_Validation_YarnPRChild, new { EntityState = item.EntityState, UserId = userId, PrimaryKeyId = item.YarnPRChildID }, transaction, 30, CommandType.StoredProcedure);

                    }
                }
                transaction.Commit();
                transactionGmt.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                transactionGmt.Rollback();
                throw ex;
            }
            finally
            {
                _connection.Close();
                _connectionGmt.Close();
            }
        }

        public async Task<YarnPRMaster> AddAsync(YarnPRMaster entity, SqlTransaction transaction, SqlTransaction transactionGmt, SqlConnection connection, SqlConnection connectionGmt)
        {
            entity.YarnPRMasterID = await _service.GetMaxIdAsync(TableNames.YARN_PR_MASTER, RepeatAfterEnum.NoRepeat, transactionGmt, connectionGmt);
            if (entity.Status == Status.ROL_BASE_PENDING)
            {
                entity.YarnPRNo = await _service.GetMaxNoAsync(TableNames.YARN_PRNO, 1, RepeatAfterEnum.NoRepeat, "00000", transactionGmt, _connectionGmt);
                /*
                var prNextNumber = await _service.GetUniqueCodeWithoutSignatureAsync(
                                   connection,
                                   transaction,
                                   TableNames.YARN_PR_MASTER,
                                   "YarnPRNo",
                                   entity.YarnPRMasterID.ToString());



                //var prNextNumber = await this.GetMaxReqNo(entity.GroupConceptNo, transaction);
                if (prNextNumber > 0)
                {
                    entity.YarnPRNo = entity.GroupConceptNo + "_" + prNextNumber;
                }
                else
                {
                    entity.YarnPRNo = entity.GroupConceptNo;
                }
                */
            }
            else if (!entity.IsAdditional)
            {
                var prNextNumber = await _service.GetUniqueCodeWithoutSignatureAsync(
                                   connection,
                                   transaction,
                                   TableNames.YARN_PR_MASTER,
                                   "YarnPRNo",
                                   entity.GroupConceptNo);



                //var prNextNumber = await this.GetMaxReqNo(entity.GroupConceptNo, transaction);
                if (prNextNumber > 0)
                {
                    entity.YarnPRNo = entity.GroupConceptNo + "_" + prNextNumber;
                }
                else
                {
                    entity.YarnPRNo = entity.GroupConceptNo;
                }
            }
            else
            {
                if (entity.YarnPRNo.IsNotNullOrEmpty())
                {
                    string parentRnDReqNo = entity.YarnPRNo;
                    //int maxCount = await this.GetMaxReqNo(parentRnDReqNo, transaction);
                    var maxCount = await _service.GetUniqueCodeWithoutSignatureAsync(
                                  connection,
                                  transaction,
                                  TableNames.YARN_PR_MASTER,
                                  "YarnPRNo",
                                  entity.GroupConceptNo);
                    entity.YarnPRNo = maxCount > 0 ? entity.YarnPRNo + "-Add-" + maxCount : entity.YarnPRNo;
                    entity.AdditionalNo = maxCount;
                }
            }
            int maxChildId = await _service.GetMaxIdAsync(TableNames.YARN_PR_CHILD, entity.Childs.Count, RepeatAfterEnum.NoRepeat, transactionGmt, connectionGmt);
            int maxCompanyId = await _service.GetMaxIdAsync(TableNames.YARN_PR_COMPANY, entity.Childs.Sum(x => x.YarnPRCompanies.Count()), RepeatAfterEnum.NoRepeat, transactionGmt, connectionGmt);

            foreach (YarnPRChild child in entity.Childs)
            {
                child.YarnPRChildID = maxChildId++;
                child.YarnPRMasterID = entity.YarnPRMasterID;
            }
            return entity;
        }
        /*
        private async Task<int> GetMaxReqNo(string rnDReqNo, SqlTransaction transaction)
        {
            int maxNo = 0;

            
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["TexConnection"].ConnectionString;
            var queryString = $"SELECT MaxValue=COUNT(*) FROM {TableNames.YARN_PR_MASTER} WHERE YarnPRNo LIKE '{rnDReqNo}%'";

            int maxNo = 0;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(queryString, connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                try
                {
                    while (reader.Read())
                    {
                        maxNo = Convert.ToInt32(reader["MaxValue"]);
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            
            if (maxNo == 0) maxNo = 1;
            return maxNo;
        }
        */

        public async Task<YarnPRMaster> UpdateAsync(YarnPRMaster entity, SqlTransaction transaction, SqlTransaction transactionGmt, SqlConnection connection, SqlConnection connectionGmt)
        {
            int maxChildId = await _service.GetMaxIdAsync(TableNames.YARN_PR_CHILD, entity.Childs.Count(x => x.EntityState == EntityState.Added), RepeatAfterEnum.NoRepeat, transactionGmt, connectionGmt);
            int maxCompanyId = await _service.GetMaxIdAsync(TableNames.YARN_PR_COMPANY, entity.Childs.Sum(x => x.YarnPRCompanies.Where(y => y.EntityState == EntityState.Added).Count()), RepeatAfterEnum.NoRepeat, transactionGmt, connectionGmt);

            foreach (YarnPRChild child in entity.Childs)
            {
                if (child.EntityState == EntityState.Added)
                {
                    child.YarnPRChildID = maxChildId++;
                    child.YarnPRMasterID = entity.YarnPRMasterID;

                    foreach (YarnPRCompany company in child.YarnPRCompanies)
                    {
                        company.YarnPRCompanyID = maxCompanyId++;
                        company.YarnPRChildID = child.YarnPRChildID;
                        company.YarnPRMasterID = entity.YarnPRMasterID;
                    }
                }
                else if (child.EntityState == EntityState.Modified)
                {
                    foreach (YarnPRCompany company in child.YarnPRCompanies.Where(x => x.EntityState == EntityState.Added))
                    {
                        company.YarnPRCompanyID = maxCompanyId++;
                        company.YarnPRChildID = child.YarnPRChildID;
                        company.YarnPRMasterID = entity.YarnPRMasterID;
                    }
                }
                else if (child.EntityState == EntityState.Deleted)
                {
                    child.YarnPRCompanies.SetDeleted();
                }
            }

            return entity;
        }

        public async Task SaveCPRAsync(YarnPRMaster yarnPRMaster, int userId)
        {
            SqlTransaction transaction = null;
            SqlTransaction transactionGmt = null;

            try
            {
               
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                await _connectionGmt.OpenAsync();
                transactionGmt = _connectionGmt.BeginTransaction();

                int newCompnayCount = yarnPRMaster.Childs.Sum(x => x.YarnPRCompanies.Where(c => c.EntityState == EntityState.Added).Count());
                int maxCompanyId = await _service.GetMaxIdAsync(TableNames.YARN_PR_COMPANY, newCompnayCount, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);


                foreach (YarnPRChild child in yarnPRMaster.Childs)
                {
                    if (child.EntityState == EntityState.Modified) yarnPRMaster.EntityState = EntityState.Modified;
                }

                await _service.SaveSingleAsync(yarnPRMaster, transaction);
                await _service.SaveAsync(yarnPRMaster.Childs, transaction);
                foreach (YarnPRChild item in yarnPRMaster.Childs)
                {
                    
                    await _connection.ExecuteAsync(SPNames.sp_Validation_YarnPRChild, new { EntityState = item.EntityState, UserId = userId, PrimaryKeyId = item.YarnPRChildID }, transaction, 30, CommandType.StoredProcedure);

                }
                transaction.Commit();
                transactionGmt.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                transactionGmt.Rollback();
                throw ex;
            }
            finally
            {
                _connection.Close();
                _connectionGmt.Close();
            }
        }

        public async Task SaveFPRAsync(YarnPRMaster entity, int userId)
        {
            SqlTransaction transaction = null;
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();
                await _service.SaveSingleAsync(entity, transaction);
                await _service.SaveAsync(entity.Childs, transaction);
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

        public async Task<YarnPRMaster> GetAllByIDAsync(int id)
        {
            string sql = $@"
            ;Select * FROM {TableNames.YARN_PR_MASTER} Where YarnPRMasterID = {id}

            ;Select * From {TableNames.YarnPRChild} Where YarnPRMasterID = {id}

            ;SELECT * FROM {TableNames.YarnPOMaster} WHERE PRMasterID = {id} AND UnApprove = 1

            ";

            //;Select * FROM {TableNames.YARN_PR_COMPANY} Where YarnPRMasterID = {id}

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YarnPRMaster data = records.Read<YarnPRMaster>().FirstOrDefault();
                Guard.Against.NullObject(data);
                data.Childs = records.Read<YarnPRChild>().ToList();
                data.YarnPOMasters = records.Read<YarnPOMaster>().ToList();
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

        public async Task UpdateEntityAsync(YarnPRMaster entity)
        {
            SqlTransaction transaction = null;
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                await _service.SaveSingleAsync(entity, transaction);
                if (entity.Approve == true)
                {
                    await _connection.ExecuteAsync("spBackupYarnPR_Approve", new { YarnPRMasterID = entity.YarnPRMasterID }, transaction, 30, CommandType.StoredProcedure);
                }
                if (entity.Reject == true)
                {
                    await _connection.ExecuteAsync("spBackupYarnPR_Reject", new { YarnPRMasterID = entity.YarnPRMasterID }, transaction, 30, CommandType.StoredProcedure);
                }
                await _service.SaveAsync(entity.YarnPOMasters, transaction);
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