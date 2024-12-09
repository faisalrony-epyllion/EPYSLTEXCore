using Dapper;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Application.Interfaces.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.General.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;

namespace EPYSLTEXCore.Application.Services.Inventory
{
    public class ProjectionYarnPurchaseBookingService : IProjectionYarnBookingService
    {
        private readonly IDapperCRUDService<ProjectionYarnBookingMaster> _service;
        //private readonly ISignatureRepository _signatureRepository;
        private readonly SqlConnection _connection;
        //private readonly ItemMasterRepository<ProjectionYarnBookingItemChild> _itemMasterRepository;
        public ProjectionYarnPurchaseBookingService(IDapperCRUDService<ProjectionYarnBookingMaster> service
            //, ISignatureRepository signatureRepository
            , ISelect2Service select2Service
            //, IMapper mapper
            //, ItemMasterRepository<ProjectionYarnBookingItemChild> itemMasterRepository
            )
        {
            _service = service;
            _service.Connection = service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            //_signatureRepository = signatureRepository;
            _connection = service.Connection;
            //_itemMasterRepository = itemMasterRepository;
            //_itemMasterRepositoryDetails = itemMasterRepositoryDetails;
        }
        public async Task<List<ProjectionYarnBookingMaster>> GetPagedAsync(int departmentId, Status status, PaginationInfo paginationInfo, LoginUser AppUser)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By PYBookingID DESC, PYBookingNo Desc" : paginationInfo.OrderBy;
            string sql;

            string sqlDepWise = AppUser.IsSuperUser || AppUser.IsAdmin ? "" : $@" AND ISNULL(M.DepartmentID,0)={departmentId} ";
            string sqlBuyerPermission = "";
            string sqlBuyerPerInnerJoin = "";

            if (AppUser.DepertmentDescription == "Marketing & Merchandising") //M&M
            {
                sqlBuyerPermission = AppUser.IsSuperUser || AppUser.IsAdmin ? "" : $@" AB AS(
                                Select b.ContactID
                                From {DbNames.EPYSL}..EmployeeAssignContactTeam a
                                Inner Join {DbNames.EPYSL}..ContactAssignTeam b on b.CategoryTeamID = a.CategoryTeamID
                                Where a.IsActive = 1 And a.EmployeeCode = {AppUser.EmployeeCode} 
                                Group By b.ContactID

                                UNION

	                            Select ContactID = 0

                            ), PYB As (
                                Select a.PYBookingID
                                From {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} a
                                Inner Join AB On AB.ContactID = a.BuyerID
                                Group By a.PYBookingID
                            ), ";

                sqlBuyerPerInnerJoin = AppUser.IsSuperUser || AppUser.IsAdmin ? "" : $@" Inner Join PYB ON PYB.PYBookingID = M.PYBookingID ";
            }

            if (status == Status.Proposed)
            {
                sql =
                $@"WITH
                {sqlBuyerPermission}
                M AS (
	                   select M.PYBookingID, M.PYBookingNo, M.PYBookingDate,E.EmployeeName BookingByName,
                       R.EmployeeName RequiredByName, M.CompanyID,M.DepartmentID
                       ,(case when ED.DepertmentDescription In ('Production Management Control','Operation[Textile]','Operation','Planning, Monitoring & Control','Knitting') Then 'Textile' When ED.DepertmentDescription = 'Research & Development' Then 'R&D' Else ED.DepertmentDescription End) DepertmentDescription
                       From {TableNames.PROJECTION_YARN_BOOKING_MASTER} M
                       INNER Join {DbNames.EPYSL}..LoginUser L ON L.UserCode = M.BookingByID
					   LEFT Join {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = M.RequiredByID
					   LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
                       LEFT JOIN {DbNames.EPYSL}..Employee R ON R.EmployeeCode = LU.EmployeeCode
                       LEFT JOIN {DbNames.EPYSL}..EmployeeDepartment ED ON ED.DepertmentID = M.DepartmentID
                       {sqlBuyerPerInnerJoin}
                       WHERE M.IsApprove=0 AND M.IsAcknowledged=0 AND M.IsReject=0  AND M.IsCancel=0 AND M.SendToApprover=1 {sqlDepWise}
                    ),
                    B AS
                    (
	                    SELECT M.PYBookingID, Buyer = STRING_AGG(C.ShortName, ',')
	                    FROM M
	                    INNER JOIN {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} A ON A.PYBookingID = M.PYBookingID
	                    INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = A.BuyerID AND A.BuyerID > 0
	                    GROUP BY M.PYBookingID
                    ),
                    BT AS
                    (
	                    SELECT M.PYBookingID, BuyerTeam = STRING_AGG(C.ShortName, ',')
	                    FROM M
	                    INNER JOIN {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} A ON A.PYBookingID = M.PYBookingID
	                    INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = A.BuyerTeamID AND A.BuyerTeamID > 0
	                    GROUP BY M.PYBookingID
                    ),
                    FinalList AS
                    (
	                    SELECT M.*, 
	                    Buyer = CASE WHEN ISNULL(B.Buyer,'') <> '' THEN B.Buyer
				                     WHEN M.DepartmentID IN ({EnumDepertmentDescription.Knitting},{EnumDepertmentDescription.Operation},{EnumDepertmentDescription.OperationTextile},{EnumDepertmentDescription.PlanningMonitoringAndControl},{EnumDepertmentDescription.ProductionManagementControl}) THEN 'Textile Advance'
				                     WHEN M.DepartmentID IN ({EnumDepertmentDescription.ResearchAndDevelopment}) THEN 'R&D'
				                     ELSE B.Buyer
			                    END, 
	                    BuyerTeam = BT.BuyerTeam
	                    FROM M
	                    LEFT JOIN B ON B.PYBookingID = M.PYBookingID
	                    LEFT JOIN BT ON BT.PYBookingID = M.PYBookingID
                    )
                    SELECT *, Count(*) Over() TotalRows FROM FinalList ";
            }
            else if (status == Status.Approved)
            {
                sql =
                $@"WITH
                {sqlBuyerPermission}
                    M AS (
	                   select M.PYBookingID, M.PYBookingNo, M.PYBookingDate,E.EmployeeName BookingByName,
                       R.EmployeeName RequiredByName, M.CompanyID,M.DepartmentID
                       ,(case when ED.DepertmentDescription In ('Production Management Control','Operation[Textile]','Operation','Planning, Monitoring & Control','Knitting') Then 'Textile' When ED.DepertmentDescription = 'Research & Development' Then 'R&D' Else ED.DepertmentDescription End) DepertmentDescription
                       From {TableNames.PROJECTION_YARN_BOOKING_MASTER} M
                       INNER Join {DbNames.EPYSL}..LoginUser L ON L.UserCode = M.BookingByID
					   LEFT Join {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = M.RequiredByID
					   LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
                       LEFT JOIN {DbNames.EPYSL}..Employee R ON R.EmployeeCode = LU.EmployeeCode
                       LEFT JOIN {DbNames.EPYSL}..EmployeeDepartment ED ON ED.DepertmentID = M.DepartmentID
                       {sqlBuyerPerInnerJoin}
                       WHERE M.IsApprove=1  AND M.IsReject=0  AND M.IsCancel=0 {sqlDepWise}
                    ),
                    B AS
                    (
	                    SELECT M.PYBookingID, Buyer = STRING_AGG(C.ShortName, ',')
	                    FROM M
	                    INNER JOIN {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} A ON A.PYBookingID = M.PYBookingID
	                    INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = A.BuyerID AND A.BuyerID > 0
	                    GROUP BY M.PYBookingID
                    ),
                    BT AS
                    (
	                    SELECT M.PYBookingID, BuyerTeam = STRING_AGG(C.ShortName, ',')
	                    FROM M
	                    INNER JOIN {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} A ON A.PYBookingID = M.PYBookingID
	                    INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = A.BuyerTeamID AND A.BuyerTeamID > 0
	                    GROUP BY M.PYBookingID
                    ),
                    FinalList AS
                    (
	                    SELECT M.*, 
	                    Buyer = CASE WHEN ISNULL(B.Buyer,'') <> '' THEN B.Buyer
				                     WHEN M.DepartmentID IN ({EnumDepertmentDescription.Knitting},{EnumDepertmentDescription.Operation},{EnumDepertmentDescription.OperationTextile},{EnumDepertmentDescription.PlanningMonitoringAndControl},{EnumDepertmentDescription.ProductionManagementControl}) THEN 'Textile Advance'
				                     WHEN M.DepartmentID IN ({EnumDepertmentDescription.ResearchAndDevelopment}) THEN 'R&D'
				                     ELSE B.Buyer
			                    END, 
	                    BuyerTeam = BT.BuyerTeam
	                    FROM M
	                    LEFT JOIN B ON B.PYBookingID = M.PYBookingID
	                    LEFT JOIN BT ON BT.PYBookingID = M.PYBookingID
                    )
                    SELECT *, Count(*) Over() TotalRows FROM FinalList ";
            }
            else if (status == Status.PartiallyCompleted)
            {
                sql =
                $@"WITH
                {sqlBuyerPermission}
                M AS (
	                   select M.PYBookingID, M.PYBookingNo, M.PYBookingDate,E.EmployeeName BookingByName,
                       R.EmployeeName RequiredByName, M.CompanyID,M.DepartmentID
                       ,(case when ED.DepertmentDescription In ('Production Management Control','Operation[Textile]','Operation','Planning, Monitoring & Control','Knitting') Then 'Textile' When ED.DepertmentDescription = 'Research & Development' Then 'R&D' Else ED.DepertmentDescription End) DepertmentDescription
                       From {TableNames.PROJECTION_YARN_BOOKING_MASTER} M
                       INNER Join {DbNames.EPYSL}..LoginUser L ON L.UserCode = M.BookingByID
					   LEFT Join {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = M.RequiredByID
					   LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
                       LEFT JOIN {DbNames.EPYSL}..Employee R ON R.EmployeeCode = LU.EmployeeCode
                       LEFT JOIN {DbNames.EPYSL}..EmployeeDepartment ED ON ED.DepertmentID = M.DepartmentID
                       {sqlBuyerPerInnerJoin}
                       WHERE M.IsApprove=1 AND M.IsAcknowledged=0 AND M.IsUnacknowledge=0 AND M.IsReject=0  AND M.IsCancel=0 --AND M.DepartmentID !='11' {sqlDepWise}
                    ),
                    B AS
                    (
	                    SELECT M.PYBookingID, Buyer = STRING_AGG(C.ShortName, ',')
	                    FROM M
	                    INNER JOIN {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} A ON A.PYBookingID = M.PYBookingID
	                    INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = A.BuyerID AND A.BuyerID > 0
	                    GROUP BY M.PYBookingID
                    ),
                    BT AS
                    (
	                    SELECT M.PYBookingID, BuyerTeam = STRING_AGG(C.ShortName, ',')
	                    FROM M
	                    INNER JOIN {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} A ON A.PYBookingID = M.PYBookingID
	                    INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = A.BuyerTeamID AND A.BuyerTeamID > 0
	                    GROUP BY M.PYBookingID
                    ),
                    FinalList AS
                    (
	                    SELECT M.*, 
	                    Buyer = CASE WHEN ISNULL(B.Buyer,'') <> '' THEN B.Buyer
				                     WHEN M.DepartmentID IN ({EnumDepertmentDescription.Knitting},{EnumDepertmentDescription.Operation},{EnumDepertmentDescription.OperationTextile},{EnumDepertmentDescription.PlanningMonitoringAndControl},{EnumDepertmentDescription.ProductionManagementControl}) THEN 'Textile Advance'
				                     WHEN M.DepartmentID IN ({EnumDepertmentDescription.ResearchAndDevelopment}) THEN 'R&D'
				                     ELSE B.Buyer
			                    END, 
	                    BuyerTeam = BT.BuyerTeam
	                    FROM M
	                    LEFT JOIN B ON B.PYBookingID = M.PYBookingID
	                    LEFT JOIN BT ON BT.PYBookingID = M.PYBookingID
                    )
                    SELECT *, Count(*) Over() TotalRows FROM FinalList ";
            }
            else if (status == Status.Reject)
            {
                sql =
                $@"WITH
                {sqlBuyerPermission}
                M AS (
	                   select M.PYBookingID, M.PYBookingNo, M.PYBookingDate,E.EmployeeName BookingByName,
                       R.EmployeeName RequiredByName, M.CompanyID,M.DepartmentID
                       ,(case when ED.DepertmentDescription In ('Production Management Control','Operation[Textile]','Operation','Planning, Monitoring & Control','Knitting') Then 'Textile' When ED.DepertmentDescription = 'Research & Development' Then 'R&D' Else ED.DepertmentDescription End) DepertmentDescription
                       From {TableNames.PROJECTION_YARN_BOOKING_MASTER} M
                       INNER Join {DbNames.EPYSL}..LoginUser L ON L.UserCode = M.BookingByID
					   LEFT Join {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = M.RequiredByID
					   LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
                       LEFT JOIN {DbNames.EPYSL}..Employee R ON R.EmployeeCode = LU.EmployeeCode
                       LEFT JOIN {DbNames.EPYSL}..EmployeeDepartment ED ON ED.DepertmentID = M.DepartmentID
                       {sqlBuyerPerInnerJoin}
                       WHERE M.IsApprove=0 AND M.IsAcknowledged=0 AND M.IsReject=1  AND M.IsCancel=0 {sqlDepWise}
                    ),
                    B AS
                    (
	                    SELECT M.PYBookingID, Buyer = STRING_AGG(C.ShortName, ',')
	                    FROM M
	                    INNER JOIN {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} A ON A.PYBookingID = M.PYBookingID
	                    INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = A.BuyerID AND A.BuyerID > 0
	                    GROUP BY M.PYBookingID
                    ),
                    BT AS
                    (
	                    SELECT M.PYBookingID, BuyerTeam = STRING_AGG(C.ShortName, ',')
	                    FROM M
	                    INNER JOIN {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} A ON A.PYBookingID = M.PYBookingID
	                    INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = A.BuyerTeamID AND A.BuyerTeamID > 0
	                    GROUP BY M.PYBookingID
                    ),
                    FinalList AS
                    (
	                    SELECT M.*, 
	                    Buyer = CASE WHEN ISNULL(B.Buyer,'') <> '' THEN B.Buyer
				                     WHEN M.DepartmentID IN ({EnumDepertmentDescription.Knitting},{EnumDepertmentDescription.Operation},{EnumDepertmentDescription.OperationTextile},{EnumDepertmentDescription.PlanningMonitoringAndControl},{EnumDepertmentDescription.ProductionManagementControl}) THEN 'Textile Advance'
				                     WHEN M.DepartmentID IN ({EnumDepertmentDescription.ResearchAndDevelopment}) THEN 'R&D'
				                     ELSE B.Buyer
			                    END, 
	                    BuyerTeam = BT.BuyerTeam
	                    FROM M
	                    LEFT JOIN B ON B.PYBookingID = M.PYBookingID
	                    LEFT JOIN BT ON BT.PYBookingID = M.PYBookingID
                    )
                    SELECT *, Count(*) Over() TotalRows FROM FinalList ";
            }
            else if (status == Status.Acknowledge)
            {
                sql =
                $@"WITH
                {sqlBuyerPermission}
                M AS (
	                   select M.PYBookingID, M.PYBookingNo, M.PYBookingDate,E.EmployeeName BookingByName,
                       R.EmployeeName RequiredByName, M.CompanyID,M.DepartmentID
                       ,(case when ED.DepertmentDescription In ('Production Management Control','Operation[Textile]','Operation','Planning, Monitoring & Control','Knitting') Then 'Textile' When ED.DepertmentDescription = 'Research & Development' Then 'R&D' Else ED.DepertmentDescription End) DepertmentDescription
                       From {TableNames.PROJECTION_YARN_BOOKING_MASTER} M
                       INNER Join {DbNames.EPYSL}..LoginUser L ON L.UserCode = M.BookingByID
					   LEFT Join {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = M.RequiredByID
					   LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
                       LEFT JOIN {DbNames.EPYSL}..Employee R ON R.EmployeeCode = LU.EmployeeCode
                       LEFT JOIN {DbNames.EPYSL}..EmployeeDepartment ED ON ED.DepertmentID = M.DepartmentID
                       {sqlBuyerPerInnerJoin}
                       WHERE M.IsApprove=1 AND M.IsAcknowledged=1 AND M.IsReject=0  AND M.IsCancel=0 --{sqlDepWise}
                    ),
                    B AS
                    (
	                    SELECT M.PYBookingID, Buyer = STRING_AGG(C.ShortName, ',')
	                    FROM M
	                    INNER JOIN {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} A ON A.PYBookingID = M.PYBookingID
	                    INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = A.BuyerID AND A.BuyerID > 0
	                    GROUP BY M.PYBookingID
                    ),
                    BT AS
                    (
	                    SELECT M.PYBookingID, BuyerTeam = STRING_AGG(C.ShortName, ',')
	                    FROM M
	                    INNER JOIN {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} A ON A.PYBookingID = M.PYBookingID
	                    INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = A.BuyerTeamID AND A.BuyerTeamID > 0
	                    GROUP BY M.PYBookingID
                    ),
                    FinalList AS
                    (
	                    SELECT M.*, 
	                    Buyer = CASE WHEN ISNULL(B.Buyer,'') <> '' THEN B.Buyer
				                     WHEN M.DepartmentID IN ({EnumDepertmentDescription.Knitting},{EnumDepertmentDescription.Operation},{EnumDepertmentDescription.OperationTextile},{EnumDepertmentDescription.PlanningMonitoringAndControl},{EnumDepertmentDescription.ProductionManagementControl}) THEN 'Textile Advance'
				                     WHEN M.DepartmentID IN ({EnumDepertmentDescription.ResearchAndDevelopment}) THEN 'R&D'
				                     ELSE B.Buyer
			                    END, 
	                    BuyerTeam = BT.BuyerTeam
	                    FROM M
	                    LEFT JOIN B ON B.PYBookingID = M.PYBookingID
	                    LEFT JOIN BT ON BT.PYBookingID = M.PYBookingID
                    )
                    SELECT *, Count(*) Over() TotalRows FROM FinalList ";
            }
            else if (status == Status.UnAcknowledge)
            {
                sql =
                $@"WITH
                {sqlBuyerPermission}
                M AS (
	                   select M.PYBookingID, M.PYBookingNo, M.PYBookingDate,E.EmployeeName BookingByName,
                       R.EmployeeName RequiredByName, M.CompanyID,M.DepartmentID
                       ,(case when ED.DepertmentDescription In ('Production Management Control','Operation[Textile]','Operation','Planning, Monitoring & Control','Knitting') Then 'Textile' When ED.DepertmentDescription = 'Research & Development' Then 'R&D' Else ED.DepertmentDescription End) DepertmentDescription
                       From {TableNames.PROJECTION_YARN_BOOKING_MASTER} M
                       INNER Join {DbNames.EPYSL}..LoginUser L ON L.UserCode = M.BookingByID
					   LEFT Join {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = M.RequiredByID
					   LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
                       LEFT JOIN {DbNames.EPYSL}..Employee R ON R.EmployeeCode = LU.EmployeeCode
                       LEFT JOIN {DbNames.EPYSL}..EmployeeDepartment ED ON ED.DepertmentID = M.DepartmentID
                       {sqlBuyerPerInnerJoin}
                       WHERE M.IsApprove=1 AND M.IsAcknowledged=0 AND M.IsUnacknowledge=1 AND M.IsReject=0  AND M.IsCancel=0 {sqlDepWise}
                    ),
                    B AS
                    (
	                    SELECT M.PYBookingID, Buyer = STRING_AGG(C.ShortName, ',')
	                    FROM M
	                    INNER JOIN {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} A ON A.PYBookingID = M.PYBookingID
	                    INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = A.BuyerID AND A.BuyerID > 0
	                    GROUP BY M.PYBookingID
                    ),
                    BT AS
                    (
	                    SELECT M.PYBookingID, BuyerTeam = STRING_AGG(C.ShortName, ',')
	                    FROM M
	                    INNER JOIN {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} A ON A.PYBookingID = M.PYBookingID
	                    INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = A.BuyerTeamID AND A.BuyerTeamID > 0
	                    GROUP BY M.PYBookingID
                    ),
                    FinalList AS
                    (
	                    SELECT M.*, 
	                    Buyer = CASE WHEN ISNULL(B.Buyer,'') <> '' THEN B.Buyer
				                     WHEN M.DepartmentID IN ({EnumDepertmentDescription.Knitting},{EnumDepertmentDescription.Operation},{EnumDepertmentDescription.OperationTextile},{EnumDepertmentDescription.PlanningMonitoringAndControl},{EnumDepertmentDescription.ProductionManagementControl}) THEN 'Textile Advance'
				                     WHEN M.DepartmentID IN ({EnumDepertmentDescription.ResearchAndDevelopment}) THEN 'R&D'
				                     ELSE B.Buyer
			                    END, 
	                    BuyerTeam = BT.BuyerTeam
	                    FROM M
	                    LEFT JOIN B ON B.PYBookingID = M.PYBookingID
	                    LEFT JOIN BT ON BT.PYBookingID = M.PYBookingID
                    )
                    SELECT *, Count(*) Over() TotalRows FROM FinalList ";
            }
            else if (status == Status.Additional)
            {
                sql =
                $@"With 
                {sqlBuyerPermission}
                M As
                    (
					   select M.PYBookingID, M.PYBookingNo, M.PYBookingDate,E.EmployeeName BookingByName,R.EmployeeName RequiredByName, M.CompanyID,M.DepartmentID
                       ,(case when ED.DepertmentDescription In ('Production Management Control','Operation[Textile]','Operation','Planning, Monitoring & Control','Knitting') Then 'Textile' When ED.DepertmentDescription = 'Research & Development' Then 'R&D' Else ED.DepertmentDescription End) DepertmentDescription
                       From {TableNames.PROJECTION_YARN_BOOKING_MASTER} M
                       INNER Join {DbNames.EPYSL}..LoginUser L ON L.UserCode = M.BookingByID
					   LEFT Join {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = M.RequiredByID
					   LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
                       LEFT JOIN {DbNames.EPYSL}..Employee R ON R.EmployeeCode = LU.EmployeeCode
                       LEFT JOIN {DbNames.EPYSL}..EmployeeDepartment ED ON ED.DepertmentID = M.DepartmentID
                       {sqlBuyerPerInnerJoin}
                       WHERE M.IsApprove=0 AND M.IsAcknowledged=0 AND M.IsCancel=0 AND M.SendToApprover=0 {sqlDepWise}
                    ),
                    B AS
                    (
	                    SELECT M.PYBookingID, Buyer = STRING_AGG(C.ShortName, ',')
	                    FROM M
	                    INNER JOIN {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} A ON A.PYBookingID = M.PYBookingID
	                    INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = A.BuyerID AND A.BuyerID > 0
	                    GROUP BY M.PYBookingID
                    ),
                    BT AS
                    (
	                    SELECT M.PYBookingID, BuyerTeam = STRING_AGG(C.ShortName, ',')
	                    FROM M
	                    INNER JOIN {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} A ON A.PYBookingID = M.PYBookingID
	                    INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = A.BuyerTeamID AND A.BuyerTeamID > 0
	                    GROUP BY M.PYBookingID
                    ),
                    FinalList AS
                    (
	                    SELECT M.*, 
	                    Buyer = CASE WHEN ISNULL(B.Buyer,'') <> '' THEN B.Buyer
				                     WHEN M.DepartmentID IN ({EnumDepertmentDescription.Knitting},{EnumDepertmentDescription.Operation},{EnumDepertmentDescription.OperationTextile},{EnumDepertmentDescription.PlanningMonitoringAndControl},{EnumDepertmentDescription.ProductionManagementControl}) THEN 'Textile Advance'
				                     WHEN M.DepartmentID IN ({EnumDepertmentDescription.ResearchAndDevelopment}) THEN 'R&D'
				                     ELSE B.Buyer
			                    END, 
	                    BuyerTeam = BT.BuyerTeam
	                    FROM M
	                    LEFT JOIN B ON B.PYBookingID = M.PYBookingID
	                    LEFT JOIN BT ON BT.PYBookingID = M.PYBookingID
                    )
                    SELECT *, Count(*) Over() TotalRows FROM FinalList ";
            }
            else if (status == Status.AllStatus)
            {
                sqlDepWise = AppUser.IsSuperUser || AppUser.IsAdmin ? "" : $@" WHERE M.DepartmentID={departmentId} ";
                //if (AppUser.DepertmentDescription == "Marketing & Merchandising")
                //{
                //    sqlDepWise = "";
                //}

                sql =
                $@"With
                {sqlBuyerPermission}
                M As
                    (
					   select M.PYBookingID, M.PYBookingNo, M.PYBookingDate,E.EmployeeName BookingByName,R.EmployeeName RequiredByName,M.IsApprove,
                       M.IsAcknowledged,M.IsReject,M.IsCancel,M.SendToApprover, M.CompanyID,M.DepartmentID
                       ,(case when ED.DepertmentDescription In ('Production Management Control','Operation[Textile]','Operation','Planning, Monitoring & Control','Knitting') Then 'Textile' When ED.DepertmentDescription = 'Research & Development' Then 'R&D' Else ED.DepertmentDescription End) DepertmentDescription
                      ,( Case
					    when M.IsAcknowledged=1 AND M.IsApprove=1 AND M.SendToApprover=1 AND M.IsReject=0 then 'Acknowledged'
					   when M.SendToApprover=1 AND M.IsApprove=0 AND M.IsReject=0 then 'Send To Approver'
					   when M.IsApprove=1 AND M.IsReject=0 AND M.SendToApprover=1 then 'Approved'
					   when M.IsReject=1 AND M.IsApprove=0 AND  M.IsAcknowledged=0 AND M.SendToApprover=1  then 'Reject'
					   when M.SendToApprover=0 AND M.IsReject=1 AND M.IsApprove=0 AND M.IsAcknowledged=0 then 'Not Sending'
					   ELSE 'Not Sending'
					   END)Status
                       From {TableNames.PROJECTION_YARN_BOOKING_MASTER} M
                       INNER Join {DbNames.EPYSL}..LoginUser L ON L.UserCode = M.BookingByID
					   LEFT Join {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = M.RequiredByID
					   LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
                       LEFT JOIN {DbNames.EPYSL}..Employee R ON R.EmployeeCode = LU.EmployeeCode
                       LEFT JOIN {DbNames.EPYSL}..EmployeeDepartment ED ON ED.DepertmentID = M.DepartmentID
                       {sqlBuyerPerInnerJoin}
                       {sqlDepWise}
                    ),
                    B AS
                    (
	                    SELECT M.PYBookingID, Buyer = STRING_AGG(C.ShortName, ',')
	                    FROM M
	                    INNER JOIN {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} A ON A.PYBookingID = M.PYBookingID
	                    INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = A.BuyerID AND A.BuyerID > 0
	                    GROUP BY M.PYBookingID
                    ),
                    BT AS
                    (
	                    SELECT M.PYBookingID, BuyerTeam = STRING_AGG(C.ShortName, ',')
	                    FROM M
	                    INNER JOIN {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} A ON A.PYBookingID = M.PYBookingID
	                    INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = A.BuyerTeamID AND A.BuyerTeamID > 0
	                    GROUP BY M.PYBookingID
                    ),
                    FinalList AS
                    (
	                    SELECT M.*, 
	                    Buyer = CASE WHEN ISNULL(B.Buyer,'') <> '' THEN B.Buyer
				                     WHEN M.DepartmentID IN ({EnumDepertmentDescription.Knitting},{EnumDepertmentDescription.Operation},{EnumDepertmentDescription.OperationTextile},{EnumDepertmentDescription.PlanningMonitoringAndControl},{EnumDepertmentDescription.ProductionManagementControl}) THEN 'Textile Advance'
				                     WHEN M.DepartmentID IN ({EnumDepertmentDescription.ResearchAndDevelopment}) THEN 'R&D'
				                     ELSE B.Buyer
			                    END, 
	                    BuyerTeam = BT.BuyerTeam
	                    FROM M
	                    LEFT JOIN B ON B.PYBookingID = M.PYBookingID
	                    LEFT JOIN BT ON BT.PYBookingID = M.PYBookingID
                    )
                    SELECT *, Count(*) Over() TotalRows FROM FinalList ";
            }
            else
            {
                sql = $@"
                    With FR As
                    (
                        select M.PYBookingID, M.PYBookingNo, M.PYBookingDate,E.EmployeeName BookingByName,R.EmployeeName RequiredByName, M.CompanyID,M.DepartmentID
                           ,(case when ED.DepertmentDescription In ('Production Management Control','Operation[Textile]','Operation','Planning, Monitoring & Control','Knitting') Then 'Textile' When ED.DepertmentDescription = 'Research & Development' Then 'R&D' Else ED.DepertmentDescription End) DepertmentDescription
				        From {TableNames.PROJECTION_YARN_BOOKING_MASTER} M
				        INNER Join {DbNames.EPYSL}..LoginUser L ON L.UserCode = M.BookingByID
				        LEFT Join {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = M.RequiredByID
				        LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
                        LEFT JOIN {DbNames.EPYSL}..Employee R ON R.EmployeeCode = LU.EmployeeCode
                        LEFT JOIN {DbNames.EPYSL}..EmployeeDepartment ED ON ED.DepertmentID = M.DepartmentID
                        WHERE M.IsApprove=0 AND M.IsAcknowledged=0 AND M.IsReject=0  AND M.IsCancel=0 AND M.SendToApprover=0 {sqlDepWise}
                    ),
                    B AS
                    (
	                    SELECT M.PYBookingID, Buyer = STRING_AGG(C.ShortName, ',')
	                    FROM FR M
	                    INNER JOIN {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} A ON A.PYBookingID = M.PYBookingID
	                    INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = A.BuyerID AND A.BuyerID > 0
	                    GROUP BY M.PYBookingID
                    ),
                    BT AS
                    (
	                    SELECT M.PYBookingID, BuyerTeam = STRING_AGG(C.ShortName, ',')
	                    FROM FR M
	                    INNER JOIN {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} A ON A.PYBookingID = M.PYBookingID
	                    INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = A.BuyerTeamID AND A.BuyerTeamID > 0
	                    GROUP BY M.PYBookingID
                    ),
                    FinalList AS
                    (
	                    SELECT M.*, 
	                    Buyer = CASE WHEN ISNULL(B.Buyer,'') <> '' THEN B.Buyer
				                     WHEN M.DepartmentID IN ({EnumDepertmentDescription.Knitting},{EnumDepertmentDescription.Operation},{EnumDepertmentDescription.OperationTextile},{EnumDepertmentDescription.PlanningMonitoringAndControl},{EnumDepertmentDescription.ProductionManagementControl}) THEN 'Textile Advance'
				                     WHEN M.DepartmentID IN ({EnumDepertmentDescription.ResearchAndDevelopment}) THEN 'R&D'
				                     ELSE B.Buyer
			                    END, 
	                    BuyerTeam = BT.BuyerTeam
	                    FROM FR M
	                    LEFT JOIN B ON B.PYBookingID = M.PYBookingID
	                    LEFT JOIN BT ON BT.PYBookingID = M.PYBookingID
                    )
                    SELECT *, Count(*) Over() TotalRows FROM FinalList ";
            }
            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";
            return await _service.GetDataAsync<ProjectionYarnBookingMaster>(sql);
        }

        public async Task<List<ProjectionYarnBookingMaster>> GetPagedAsynci(int departmentId, string departmentName, int employeeCode, Status status, PaginationInfo paginationInfo, LoginUser AppUser)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By PYBookingID DESC, PYBookingNo Desc" : paginationInfo.OrderBy;
            string sql;

            string forRndDept = departmentName == "Research & Development" ? $@"Union All
							Select a.PYBookingID
							From {TableNames.PROJECTION_YARN_BOOKING_MASTER} a
							Left join {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} b on b.PYBookingID = a.PYBookingID
							Group By a.PYBookingID
							Having Max(Case When ISNULL(b.BuyerID,0) = 0 Then 0 Else 1 End) = 0" : "";

            string sqlBuyerPermission = AppUser.IsSuperUser || AppUser.IsAdmin ? "" : $@" AB AS(
                            Select b.ContactID
                            From {DbNames.EPYSL}..EmployeeAssignContactTeam a
                            Inner Join {DbNames.EPYSL}..ContactAssignTeam b on b.CategoryTeamID = a.CategoryTeamID
                            Where a.IsActive = 1 And a.EmployeeCode = {AppUser.EmployeeCode} 
                            Group By b.ContactID
                            
	                        UNION

	                        Select ContactID = 0

                        ), PYB As (
                            Select a.PYBookingID
                            From {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} a
                            Inner Join AB On AB.ContactID = a.BuyerID
                            Group By a.PYBookingID
                            {forRndDept}
                        ), ";

            string sqlBuyerPerInnerJoin = AppUser.IsSuperUser || AppUser.IsAdmin ? "" : $@" Inner Join PYB ON PYB.PYBookingID = M.PYBookingID ";

            string sqlDepWise = AppUser.IsSuperUser || AppUser.IsAdmin ? "" : $@" AND ISNULL(M.DepartmentID,0) = {departmentId} ";

            if (status == Status.UnAcknowledge && !AppUser.IsSuperUser && !AppUser.IsAdmin)
            {
                sqlDepWise = $@" AND (ISNULL(M.DepartmentID,0) = {departmentId} OR M.UnacknowledgeBy = {AppUser.UserCode}) ";
            }
            string sqlDepWise1 = "";//AppUser.IsSuperUser || AppUser.IsAdmin ? "" : $@" And Isnull(M.DepartmentID,0) = (Case When @DepartmentID IN(11,19,31,35,38) Then Isnull(M.DepartmentID,0) Else @DepartmentID End) ";

            if (status == Status.Proposed)
            {
                sql =
                $@" 
                ;WITH 
               {sqlBuyerPermission}
                M AS
                (
                    Select M.PYBookingID, M.PYBookingNo, M.PYBookingDate, E.EmployeeName BookingByName,
                    R.EmployeeName RequiredByName, M.CompanyID,M.DepartmentID, M.isMarketingFlag,
                    (case when ED.DepertmentDescription In ('Production Management Control','Operation[Textile]','Operation','Planning, Monitoring & Control','Knitting') Then 'Textile'
                    When ED.DepertmentDescription = 'Research & Development' Then 'R&D' Else ED.DepertmentDescription End) DepertmentDescription
                    From {TableNames.PROJECTION_YARN_BOOKING_MASTER} M
                    INNER Join {DbNames.EPYSL}..LoginUser L ON L.UserCode = M.BookingByID
                    LEFT Join {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = M.RequiredByID
                    LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
                    LEFT JOIN {DbNames.EPYSL}..Employee R ON R.EmployeeCode = LU.EmployeeCode
                    LEFT JOIN {DbNames.EPYSL}..EmployeeDepartment ED ON ED.DepertmentID = M.DepartmentID
                    {sqlBuyerPerInnerJoin}
                    WHERE M.IsApprove = 0 AND M.IsAcknowledged = 0 AND M.IsReject = 0 AND M.IsCancel = 0 
                    AND M.SendToApprover = 1 {sqlDepWise}

                    UNION

	                Select M.PYBookingID,M.PYBookingNo
	                , M.PYBookingDate, E.EmployeeName BookingByName,
                    R.EmployeeName RequiredByName, M.CompanyID,M.DepartmentID, M.isMarketingFlag,
                    (case when ED.DepertmentDescription In ('Production Management Control','Operation[Textile]','Operation','Planning, Monitoring & Control','Knitting') Then 'Textile'
                    When ED.DepertmentDescription = 'Research & Development' Then 'R&D' Else ED.DepertmentDescription End) DepertmentDescription
                    From {TableNames.PROJECTION_YARN_BOOKING_MASTER} M
                    INNER Join {DbNames.EPYSL}..LoginUser L ON L.UserCode = M.BookingByID
                    LEFT Join {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = M.RequiredByID
                    LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
                    LEFT JOIN {DbNames.EPYSL}..Employee R ON R.EmployeeCode = LU.EmployeeCode
                    LEFT JOIN {DbNames.EPYSL}..EmployeeDepartment ED ON ED.DepertmentID = M.DepartmentID
                    LEFT JOIN {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} BBT ON BBT.PYBookingID = M.PYBookingID
                    WHERE M.IsApprove = 0 AND M.IsAcknowledged = 0 AND M.IsReject = 0 AND M.IsCancel = 0 
                    AND M.SendToApprover = 1 AND BBT.PYBookingBuyerAndBuyerTeamID IS NULL {sqlDepWise}
                ),
                B AS
                (
	                SELECT M.PYBookingID, Buyer = STRING_AGG(C.ShortName, ',')
	                FROM M
	                INNER JOIN {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} A ON A.PYBookingID = M.PYBookingID
	                INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = A.BuyerID AND A.BuyerID > 0
	                GROUP BY M.PYBookingID
                ),
                BT AS
                (
	                SELECT M.PYBookingID, BuyerTeam = STRING_AGG(C.ShortName, ',')
	                FROM M
	                INNER JOIN {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} A ON A.PYBookingID = M.PYBookingID
	                INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = A.BuyerTeamID AND A.BuyerTeamID > 0
	                GROUP BY M.PYBookingID
                ),
                FinalList AS
                (
	                SELECT M.*, 
	                Buyer = CASE WHEN ISNULL(B.Buyer,'') <> '' THEN B.Buyer
				                 WHEN M.DepartmentID IN ({EnumDepertmentDescription.Knitting},{EnumDepertmentDescription.Operation},{EnumDepertmentDescription.OperationTextile},{EnumDepertmentDescription.PlanningMonitoringAndControl},{EnumDepertmentDescription.ProductionManagementControl}) THEN 'Textile Advance'
				                 WHEN M.DepartmentID IN ({EnumDepertmentDescription.ResearchAndDevelopment}) THEN 'R&D'
				                 ELSE B.Buyer
			                END, 
	                BuyerTeam = BT.BuyerTeam
	                FROM M
	                LEFT JOIN B ON B.PYBookingID = M.PYBookingID
	                LEFT JOIN BT ON BT.PYBookingID = M.PYBookingID
                )
                SELECT *, Count(*) Over() TotalRows FROM FinalList ";
            }
            else if (status == Status.Approved)
            {
                sql =
                $@"
                ;WITH
                {sqlBuyerPermission}
                M AS
                (
                    Select M.PYBookingID, M.PYBookingNo, M.PYBookingDate, E.EmployeeName BookingByName,
                    R.EmployeeName RequiredByName, M.CompanyID,M.DepartmentID, M.isMarketingFlag,
                    (case when ED.DepertmentDescription In ('Production Management Control','Operation[Textile]','Operation','Planning, Monitoring & Control','Knitting') Then 'Textile'
                    When ED.DepertmentDescription = 'Research & Development' Then 'R&D' Else ED.DepertmentDescription End) DepertmentDescription
                    From {TableNames.PROJECTION_YARN_BOOKING_MASTER} M
                    INNER Join {DbNames.EPYSL}..LoginUser L ON L.UserCode = M.BookingByID
                    LEFT Join {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = M.RequiredByID
                    LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
                    LEFT JOIN {DbNames.EPYSL}..Employee R ON R.EmployeeCode = LU.EmployeeCode
                    LEFT JOIN {DbNames.EPYSL}..EmployeeDepartment ED ON ED.DepertmentID = M.DepartmentID
                    {sqlBuyerPerInnerJoin}
                    WHERE M.IsApprove = 1 AND M.IsReject = 0 AND M.IsCancel = 0 {sqlDepWise}

                    UNION

	                Select M.PYBookingID,M.PYBookingNo
	                , M.PYBookingDate, E.EmployeeName BookingByName,
                    R.EmployeeName RequiredByName, M.CompanyID,M.DepartmentID, M.isMarketingFlag,
                    (case when ED.DepertmentDescription In ('Production Management Control','Operation[Textile]','Operation','Planning, Monitoring & Control','Knitting') Then 'Textile'
                    When ED.DepertmentDescription = 'Research & Development' Then 'R&D' Else ED.DepertmentDescription End) DepertmentDescription
                    From {TableNames.PROJECTION_YARN_BOOKING_MASTER} M
                    INNER Join {DbNames.EPYSL}..LoginUser L ON L.UserCode = M.BookingByID
                    LEFT Join {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = M.RequiredByID
                    LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
                    LEFT JOIN {DbNames.EPYSL}..Employee R ON R.EmployeeCode = LU.EmployeeCode
                    LEFT JOIN {DbNames.EPYSL}..EmployeeDepartment ED ON ED.DepertmentID = M.DepartmentID
                    LEFT JOIN {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} BBT ON BBT.PYBookingID = M.PYBookingID
                    WHERE M.IsApprove = 1 AND M.IsReject = 0 AND M.IsCancel = 0 AND BBT.PYBookingBuyerAndBuyerTeamID IS NULL {sqlDepWise}
                ),
                B AS
                (
	                SELECT M.PYBookingID, Buyer = STRING_AGG(C.ShortName, ',')
	                FROM M
	                INNER JOIN {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} A ON A.PYBookingID = M.PYBookingID
	                INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = A.BuyerID AND A.BuyerID > 0
	                GROUP BY M.PYBookingID
                ),
                BT AS
                (
	                SELECT M.PYBookingID, BuyerTeam = STRING_AGG(C.ShortName, ',')
	                FROM M
	                INNER JOIN {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} A ON A.PYBookingID = M.PYBookingID
	                INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = A.BuyerTeamID AND A.BuyerTeamID > 0
	                GROUP BY M.PYBookingID
                ),
                FinalList AS
                (
	                SELECT M.*, 
	                Buyer = CASE WHEN ISNULL(B.Buyer,'') <> '' THEN B.Buyer
				                 WHEN M.DepartmentID IN ({EnumDepertmentDescription.Knitting},{EnumDepertmentDescription.Operation},{EnumDepertmentDescription.OperationTextile},{EnumDepertmentDescription.PlanningMonitoringAndControl},{EnumDepertmentDescription.ProductionManagementControl}) THEN 'Textile Advance'
				                 WHEN M.DepartmentID IN ({EnumDepertmentDescription.ResearchAndDevelopment}) THEN 'R&D'
				                 ELSE B.Buyer
			                END, 
	                BuyerTeam = BT.BuyerTeam
	                FROM M
	                LEFT JOIN B ON B.PYBookingID = M.PYBookingID
	                LEFT JOIN BT ON BT.PYBookingID = M.PYBookingID
                )
                SELECT *, Count(*) Over() TotalRows FROM FinalList ";
            }
            else if (status == Status.PartiallyCompleted)
            {
                sql =
                $@"
                Declare @DepartmentID int = {departmentId} 

                ;WITH 
                {sqlBuyerPermission}
                M AS
                (
                    Select M.PYBookingID, M.PYBookingNo, M.PYBookingDate, E.EmployeeName BookingByName,
                    R.EmployeeName RequiredByName, M.CompanyID,M.DepartmentID, M.isMarketingFlag,
                    (case when ED.DepertmentDescription In ('Production Management Control','Operation[Textile]','Operation','Planning, Monitoring & Control','Knitting') Then 'Textile'
                    When ED.DepertmentDescription = 'Research & Development' Then 'R&D' Else ED.DepertmentDescription End) DepertmentDescription
                    From {TableNames.PROJECTION_YARN_BOOKING_MASTER} M
                    INNER Join {DbNames.EPYSL}..LoginUser L ON L.UserCode = M.BookingByID
                    LEFT Join {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = M.RequiredByID
                    LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
                    LEFT JOIN {DbNames.EPYSL}..Employee R ON R.EmployeeCode = LU.EmployeeCode
                    LEFT JOIN {DbNames.EPYSL}..EmployeeDepartment ED ON ED.DepertmentID = M.DepartmentID
                    {sqlBuyerPerInnerJoin}
                    WHERE M.IsApprove=1 AND M.IsAcknowledged=0 AND M.IsUnacknowledge=0 AND M.IsReject=0 AND M.IsCancel=0

                    UNION

	                Select M.PYBookingID, M.PYBookingNo, M.PYBookingDate, E.EmployeeName BookingByName,
                    R.EmployeeName RequiredByName, M.CompanyID,M.DepartmentID, M.isMarketingFlag,
                    (case when ED.DepertmentDescription In ('Production Management Control','Operation[Textile]','Operation','Planning, Monitoring & Control','Knitting') Then 'Textile'
                    When ED.DepertmentDescription = 'Research & Development' Then 'R&D' Else ED.DepertmentDescription End) DepertmentDescription
                    From {TableNames.PROJECTION_YARN_BOOKING_MASTER} M
                    INNER Join {DbNames.EPYSL}..LoginUser L ON L.UserCode = M.BookingByID
                    LEFT Join {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = M.RequiredByID
                    LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
                    LEFT JOIN {DbNames.EPYSL}..Employee R ON R.EmployeeCode = LU.EmployeeCode
                    LEFT JOIN {DbNames.EPYSL}..EmployeeDepartment ED ON ED.DepertmentID = M.DepartmentID
                    LEFT JOIN {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} BBT ON BBT.PYBookingID = M.PYBookingID
                    WHERE M.IsApprove=1 AND M.IsAcknowledged=0 AND M.IsUnacknowledge=0 AND M.IsReject=0 AND M.IsCancel=0 AND BBT.PYBookingBuyerAndBuyerTeamID IS NULL
                ),
                B AS
                (
	                SELECT M.PYBookingID, Buyer = STRING_AGG(C.ShortName, ',')
	                FROM M
	                INNER JOIN {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} A ON A.PYBookingID = M.PYBookingID
	                INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = A.BuyerID AND A.BuyerID > 0
	                GROUP BY M.PYBookingID
                ),
                BT AS
                (
	                SELECT M.PYBookingID, BuyerTeam = STRING_AGG(C.ShortName, ',')
	                FROM M
	                INNER JOIN {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} A ON A.PYBookingID = M.PYBookingID
	                INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = A.BuyerTeamID AND A.BuyerTeamID > 0
	                GROUP BY M.PYBookingID
                ),
                FinalList AS
                (
	                SELECT M.*, 
	                Buyer = CASE WHEN ISNULL(B.Buyer,'') <> '' THEN B.Buyer
				                 WHEN M.DepartmentID IN ({EnumDepertmentDescription.Knitting},{EnumDepertmentDescription.Operation},{EnumDepertmentDescription.OperationTextile},{EnumDepertmentDescription.PlanningMonitoringAndControl},{EnumDepertmentDescription.ProductionManagementControl}) THEN 'Textile Advance'
				                 WHEN M.DepartmentID IN ({EnumDepertmentDescription.ResearchAndDevelopment}) THEN 'R&D'
				                 ELSE B.Buyer
			                END, 
	                BuyerTeam = BT.BuyerTeam
	                FROM M
	                LEFT JOIN B ON B.PYBookingID = M.PYBookingID
	                LEFT JOIN BT ON BT.PYBookingID = M.PYBookingID
                )
                SELECT *, Count(*) Over() TotalRows FROM FinalList ";
            }
            else if (status == Status.Reject)
            {
                sql =
                $@"
                ;WITH 
                {sqlBuyerPermission}
                M AS
                (
                    Select M.PYBookingID, M.PYBookingNo, M.PYBookingDate, E.EmployeeName BookingByName,
                    R.EmployeeName RequiredByName, M.CompanyID,M.DepartmentID, M.isMarketingFlag,
                    (case when ED.DepertmentDescription In ('Production Management Control','Operation[Textile]','Operation','Planning, Monitoring & Control','Knitting') Then 'Textile'
                    When ED.DepertmentDescription = 'Research & Development' Then 'R&D' Else ED.DepertmentDescription End) DepertmentDescription
                    From {TableNames.PROJECTION_YARN_BOOKING_MASTER} M
                    INNER Join {DbNames.EPYSL}..LoginUser L ON L.UserCode = M.BookingByID
                    LEFT Join {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = M.RequiredByID
                    LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
                    LEFT JOIN {DbNames.EPYSL}..Employee R ON R.EmployeeCode = LU.EmployeeCode
                    LEFT JOIN {DbNames.EPYSL}..EmployeeDepartment ED ON ED.DepertmentID = M.DepartmentID
                    {sqlBuyerPerInnerJoin}
                    WHERE M.IsApprove=0 AND M.IsAcknowledged=0 AND M.IsReject=1 AND M.IsCancel=0 {sqlDepWise}

                    UNION

                    Select M.PYBookingID, M.PYBookingNo, M.PYBookingDate, E.EmployeeName BookingByName,
                    R.EmployeeName RequiredByName, M.CompanyID,M.DepartmentID, M.isMarketingFlag,
                    (case when ED.DepertmentDescription In ('Production Management Control','Operation[Textile]','Operation','Planning, Monitoring & Control','Knitting') Then 'Textile'
                    When ED.DepertmentDescription = 'Research & Development' Then 'R&D' Else ED.DepertmentDescription End) DepertmentDescription
                    From {TableNames.PROJECTION_YARN_BOOKING_MASTER} M
                    INNER Join {DbNames.EPYSL}..LoginUser L ON L.UserCode = M.BookingByID
                    LEFT Join {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = M.RequiredByID
                    LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
                    LEFT JOIN {DbNames.EPYSL}..Employee R ON R.EmployeeCode = LU.EmployeeCode
                    LEFT JOIN {DbNames.EPYSL}..EmployeeDepartment ED ON ED.DepertmentID = M.DepartmentID
                    LEFT JOIN {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} BBT ON BBT.PYBookingID = M.PYBookingID
                    WHERE M.IsApprove=0 AND M.IsAcknowledged=0 AND M.IsReject=1 AND M.IsCancel=0 AND BBT.PYBookingBuyerAndBuyerTeamID IS NULL {sqlDepWise}
                ),
                B AS
                (
	                SELECT M.PYBookingID, Buyer = STRING_AGG(C.ShortName, ',')
	                FROM M
	                INNER JOIN {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} A ON A.PYBookingID = M.PYBookingID
	                INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = A.BuyerID AND A.BuyerID > 0
	                GROUP BY M.PYBookingID
                ),
                BT AS
                (
	                SELECT M.PYBookingID, BuyerTeam = STRING_AGG(C.ShortName, ',')
	                FROM M
	                INNER JOIN {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} A ON A.PYBookingID = M.PYBookingID
	                INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = A.BuyerTeamID AND A.BuyerTeamID > 0
	                GROUP BY M.PYBookingID
                ),
                FinalList AS
                (
	                SELECT M.*, 
	                Buyer = CASE WHEN ISNULL(B.Buyer,'') <> '' THEN B.Buyer
				                 WHEN M.DepartmentID IN ({EnumDepertmentDescription.Knitting},{EnumDepertmentDescription.Operation},{EnumDepertmentDescription.OperationTextile},{EnumDepertmentDescription.PlanningMonitoringAndControl},{EnumDepertmentDescription.ProductionManagementControl}) THEN 'Textile Advance'
				                 WHEN M.DepartmentID IN ({EnumDepertmentDescription.ResearchAndDevelopment}) THEN 'R&D'
				                 ELSE B.Buyer
			                END, 
	                BuyerTeam = BT.BuyerTeam
	                FROM M
	                LEFT JOIN B ON B.PYBookingID = M.PYBookingID
	                LEFT JOIN BT ON BT.PYBookingID = M.PYBookingID
                )
                SELECT *, Count(*) Over() TotalRows FROM FinalList ";
            }
            else if (status == Status.Acknowledge)
            {
                sql =
                $@"
                Declare @DepartmentID int = {departmentId} 

                ;WITH 
                {sqlBuyerPermission}
                M AS
                (
                    Select M.PYBookingID, M.PYBookingNo, M.PYBookingDate, E.EmployeeName BookingByName,
                    R.EmployeeName RequiredByName, M.CompanyID,M.DepartmentID, M.isMarketingFlag,
                    (case when ED.DepertmentDescription In ('Production Management Control','Operation[Textile]','Operation','Planning, Monitoring & Control','Knitting') Then 'Textile'
                    When ED.DepertmentDescription = 'Research & Development' Then 'R&D' Else ED.DepertmentDescription End) DepertmentDescription
                    From {TableNames.PROJECTION_YARN_BOOKING_MASTER} M
                    INNER Join {DbNames.EPYSL}..LoginUser L ON L.UserCode = M.BookingByID
                    LEFT Join {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = M.RequiredByID
                    LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
                    LEFT JOIN {DbNames.EPYSL}..Employee R ON R.EmployeeCode = LU.EmployeeCode
                    LEFT JOIN {DbNames.EPYSL}..EmployeeDepartment ED ON ED.DepertmentID = M.DepartmentID
                    {sqlBuyerPerInnerJoin}
                    WHERE M.IsApprove = 1 AND M.IsAcknowledged=1 AND M.IsReject = 0  AND M.IsCancel = 0 {sqlDepWise}

                    UNION

                    Select M.PYBookingID, M.PYBookingNo, M.PYBookingDate, E.EmployeeName BookingByName,
                    R.EmployeeName RequiredByName, M.CompanyID,M.DepartmentID, M.isMarketingFlag,
                    (case when ED.DepertmentDescription In ('Production Management Control','Operation[Textile]','Operation','Planning, Monitoring & Control','Knitting') Then 'Textile'
                    When ED.DepertmentDescription = 'Research & Development' Then 'R&D' Else ED.DepertmentDescription End) DepertmentDescription
                    From {TableNames.PROJECTION_YARN_BOOKING_MASTER} M
                    INNER Join {DbNames.EPYSL}..LoginUser L ON L.UserCode = M.BookingByID
                    LEFT Join {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = M.RequiredByID
                    LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
                    LEFT JOIN {DbNames.EPYSL}..Employee R ON R.EmployeeCode = LU.EmployeeCode
                    LEFT JOIN {DbNames.EPYSL}..EmployeeDepartment ED ON ED.DepertmentID = M.DepartmentID
                    LEFT JOIN {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} BBT ON BBT.PYBookingID = M.PYBookingID
                    WHERE M.IsApprove = 1 AND M.IsAcknowledged=1 AND M.IsReject = 0  AND M.IsCancel = 0 AND BBT.PYBookingBuyerAndBuyerTeamID IS NULL {sqlDepWise}
                ),
                B AS
                (
	                SELECT M.PYBookingID, Buyer = STRING_AGG(C.ShortName, ',')
	                FROM M
	                INNER JOIN {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} A ON A.PYBookingID = M.PYBookingID
	                INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = A.BuyerID AND A.BuyerID > 0
	                GROUP BY M.PYBookingID
                ),
                BT AS
                (
	                SELECT M.PYBookingID, BuyerTeam = STRING_AGG(C.ShortName, ',')
	                FROM M
	                INNER JOIN {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} A ON A.PYBookingID = M.PYBookingID
	                INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = A.BuyerTeamID AND A.BuyerTeamID > 0
	                GROUP BY M.PYBookingID
                ),
                FinalList AS
                (
	                SELECT M.*, 
	                Buyer = CASE WHEN ISNULL(B.Buyer,'') <> '' THEN B.Buyer
				                 WHEN M.DepartmentID IN ({EnumDepertmentDescription.Knitting},{EnumDepertmentDescription.Operation},{EnumDepertmentDescription.OperationTextile},{EnumDepertmentDescription.PlanningMonitoringAndControl},{EnumDepertmentDescription.ProductionManagementControl}) THEN 'Textile Advance'
				                 WHEN M.DepartmentID IN ({EnumDepertmentDescription.ResearchAndDevelopment}) THEN 'R&D'
				                 ELSE B.Buyer
			                END, 
	                BuyerTeam = BT.BuyerTeam
	                FROM M
	                LEFT JOIN B ON B.PYBookingID = M.PYBookingID
	                LEFT JOIN BT ON BT.PYBookingID = M.PYBookingID
                )
                SELECT *, Count(*) Over() TotalRows FROM FinalList ";
            }
            else if (status == Status.UnAcknowledge)
            {
                sql =
                $@"Declare @DepartmentID int = {departmentId} 

                ;WITH 
                {sqlBuyerPermission}
                M AS
                (
                    Select M.PYBookingID, M.PYBookingNo, M.PYBookingDate, E.EmployeeName BookingByName,
                    R.EmployeeName RequiredByName, M.CompanyID,M.DepartmentID, M.isMarketingFlag,
                    (case when ED.DepertmentDescription In ('Production Management Control','Operation[Textile]','Operation','Planning, Monitoring & Control','Knitting') Then 'Textile'
                    When ED.DepertmentDescription = 'Research & Development' Then 'R&D' Else ED.DepertmentDescription End) DepertmentDescription
                    From {TableNames.PROJECTION_YARN_BOOKING_MASTER} M
                    INNER Join {DbNames.EPYSL}..LoginUser L ON L.UserCode = M.BookingByID
                    LEFT Join {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = M.RequiredByID
                    LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
                    LEFT JOIN {DbNames.EPYSL}..Employee R ON R.EmployeeCode = LU.EmployeeCode
                    LEFT JOIN {DbNames.EPYSL}..EmployeeDepartment ED ON ED.DepertmentID = M.DepartmentID
                    {sqlBuyerPerInnerJoin}
                    WHERE M.IsApprove = 1 AND M.IsAcknowledged = 0 AND M.IsUnacknowledge=1 AND M.IsReject=0 AND M.IsCancel=0 {sqlDepWise}

                    UNION

                    Select M.PYBookingID, M.PYBookingNo, M.PYBookingDate, E.EmployeeName BookingByName,
                    R.EmployeeName RequiredByName, M.CompanyID,M.DepartmentID, M.isMarketingFlag,
                    (case when ED.DepertmentDescription In ('Production Management Control','Operation[Textile]','Operation','Planning, Monitoring & Control','Knitting') Then 'Textile'
                    When ED.DepertmentDescription = 'Research & Development' Then 'R&D' Else ED.DepertmentDescription End) DepertmentDescription
                    From {TableNames.PROJECTION_YARN_BOOKING_MASTER} M
                    INNER Join {DbNames.EPYSL}..LoginUser L ON L.UserCode = M.BookingByID
                    LEFT Join {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = M.RequiredByID
                    LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
                    LEFT JOIN {DbNames.EPYSL}..Employee R ON R.EmployeeCode = LU.EmployeeCode
                    LEFT JOIN {DbNames.EPYSL}..EmployeeDepartment ED ON ED.DepertmentID = M.DepartmentID
                    LEFT JOIN {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} BBT ON BBT.PYBookingID = M.PYBookingID
                    WHERE M.IsApprove = 1 AND M.IsAcknowledged = 0 AND M.IsUnacknowledge=1 AND M.IsReject=0 AND M.IsCancel=0 AND BBT.PYBookingBuyerAndBuyerTeamID IS NULL {sqlDepWise}
                ),
                B AS
                (
	                SELECT M.PYBookingID, Buyer = STRING_AGG(C.ShortName, ',')
	                FROM M
	                INNER JOIN {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} A ON A.PYBookingID = M.PYBookingID
	                INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = A.BuyerID AND A.BuyerID > 0
	                GROUP BY M.PYBookingID
                ),
                BT AS
                (
	                SELECT M.PYBookingID, BuyerTeam = STRING_AGG(C.ShortName, ',')
	                FROM M
	                INNER JOIN {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} A ON A.PYBookingID = M.PYBookingID
	                INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = A.BuyerTeamID AND A.BuyerTeamID > 0
	                GROUP BY M.PYBookingID
                ),
                FinalList AS
                (
	                SELECT M.*, 
	                Buyer = CASE WHEN ISNULL(B.Buyer,'') <> '' THEN B.Buyer
				                 WHEN M.DepartmentID IN ({EnumDepertmentDescription.Knitting},{EnumDepertmentDescription.Operation},{EnumDepertmentDescription.OperationTextile},{EnumDepertmentDescription.PlanningMonitoringAndControl},{EnumDepertmentDescription.ProductionManagementControl}) THEN 'Textile Advance'
				                 WHEN M.DepartmentID IN ({EnumDepertmentDescription.ResearchAndDevelopment}) THEN 'R&D'
				                 ELSE B.Buyer
			                END, 
	                BuyerTeam = BT.BuyerTeam
	                FROM M
	                LEFT JOIN B ON B.PYBookingID = M.PYBookingID
	                LEFT JOIN BT ON BT.PYBookingID = M.PYBookingID
                )
                SELECT *, Count(*) Over() TotalRows FROM FinalList ";
            }
            else if (status == Status.Additional)
            {
                sql =
                $@"
                ;WITH 
                {sqlBuyerPermission}
                M AS
                (
                    Select M.PYBookingID, M.PYBookingNo, M.PYBookingDate, E.EmployeeName BookingByName,
                    R.EmployeeName RequiredByName, M.CompanyID,M.DepartmentID, M.isMarketingFlag,
                    (case when ED.DepertmentDescription In ('Production Management Control','Operation[Textile]','Operation','Planning, Monitoring & Control','Knitting') Then 'Textile'
                    When ED.DepertmentDescription = 'Research & Development' Then 'R&D' Else ED.DepertmentDescription End) DepertmentDescription
                    From {TableNames.PROJECTION_YARN_BOOKING_MASTER} M
                    INNER Join {DbNames.EPYSL}..LoginUser L ON L.UserCode = M.BookingByID
                    LEFT Join {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = M.RequiredByID
                    LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
                    LEFT JOIN {DbNames.EPYSL}..Employee R ON R.EmployeeCode = LU.EmployeeCode
                    LEFT JOIN {DbNames.EPYSL}..EmployeeDepartment ED ON ED.DepertmentID = M.DepartmentID
                    {sqlBuyerPerInnerJoin}
                    WHERE M.IsApprove = 0 AND M.IsAcknowledged = 0 AND M.IsReject = 0  AND M.IsCancel = 0 
                    AND M.SendToApprover = 0  {sqlDepWise}

                    UNION

                    Select M.PYBookingID, M.PYBookingNo, M.PYBookingDate, E.EmployeeName BookingByName,
                    R.EmployeeName RequiredByName, M.CompanyID,M.DepartmentID, M.isMarketingFlag,
                    (case when ED.DepertmentDescription In ('Production Management Control','Operation[Textile]','Operation','Planning, Monitoring & Control','Knitting') Then 'Textile'
                    When ED.DepertmentDescription = 'Research & Development' Then 'R&D' Else ED.DepertmentDescription End) DepertmentDescription
                    From {TableNames.PROJECTION_YARN_BOOKING_MASTER} M
                    INNER Join {DbNames.EPYSL}..LoginUser L ON L.UserCode = M.BookingByID
                    LEFT Join {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = M.RequiredByID
                    LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
                    LEFT JOIN {DbNames.EPYSL}..Employee R ON R.EmployeeCode = LU.EmployeeCode
                    LEFT JOIN {DbNames.EPYSL}..EmployeeDepartment ED ON ED.DepertmentID = M.DepartmentID
                    LEFT JOIN {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} BBT ON BBT.PYBookingID = M.PYBookingID
                    WHERE M.IsApprove = 0 AND M.IsAcknowledged = 0 AND M.IsReject = 0  AND M.IsCancel = 0 
                    AND M.SendToApprover = 0 AND BBT.PYBookingBuyerAndBuyerTeamID IS NULL {sqlDepWise}
                ),
                B AS
                (
	                SELECT M.PYBookingID, Buyer = STRING_AGG(C.ShortName, ',')
	                FROM M
	                INNER JOIN {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} A ON A.PYBookingID = M.PYBookingID
	                INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = A.BuyerID AND A.BuyerID > 0
	                GROUP BY M.PYBookingID
                ),
                BT AS
                (
	                SELECT M.PYBookingID, BuyerTeam = STRING_AGG(C.ShortName, ',')
	                FROM M
	                INNER JOIN {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} A ON A.PYBookingID = M.PYBookingID
	                INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = A.BuyerTeamID AND A.BuyerTeamID > 0
	                GROUP BY M.PYBookingID
                ),
                FinalList AS
                (
	                SELECT M.*, 
	                Buyer = CASE WHEN ISNULL(B.Buyer,'') <> '' THEN B.Buyer
				                 WHEN M.DepartmentID IN ({EnumDepertmentDescription.Knitting},{EnumDepertmentDescription.Operation},{EnumDepertmentDescription.OperationTextile},{EnumDepertmentDescription.PlanningMonitoringAndControl},{EnumDepertmentDescription.ProductionManagementControl}) THEN 'Textile Advance'
				                 WHEN M.DepartmentID IN ({EnumDepertmentDescription.ResearchAndDevelopment}) THEN 'R&D'
				                 ELSE B.Buyer
			                END, 
	                BuyerTeam = BT.BuyerTeam
	                FROM M
	                LEFT JOIN B ON B.PYBookingID = M.PYBookingID
	                LEFT JOIN BT ON BT.PYBookingID = M.PYBookingID
                )
                SELECT *, Count(*) Over() TotalRows FROM FinalList ";
            }
            else if (status == Status.AllStatus)
            {
                sqlDepWise1 = "";// AppUser.IsSuperUser || AppUser.IsAdmin ? "" : $@" WHERE Isnull(M.DepartmentID,0) = (Case When @DepartmentID IN(11,19,31,35,38) Then Isnull(M.DepartmentID,0) Else @DepartmentID End) ";

                sql =
                $@"Declare @DepartmentID int = {departmentId} 

                ;WITH 
                {sqlBuyerPermission}
                M AS
                (
                    Select M.PYBookingID, M.PYBookingNo, M.PYBookingDate, E.EmployeeName BookingByName,
                    R.EmployeeName RequiredByName, M.CompanyID,M.DepartmentID, M.isMarketingFlag,
                    (case when ED.DepertmentDescription In ('Production Management Control','Operation[Textile]','Operation','Planning, Monitoring & Control','Knitting') Then 'Textile'
                    When ED.DepertmentDescription = 'Research & Development' Then 'R&D' Else ED.DepertmentDescription End) DepertmentDescription,
	                M.IsApprove, M.IsAcknowledged, M.IsReject, M.IsCancel, M.SendToApprover,
	                (Case
		                when M.IsAcknowledged=1 AND M.IsApprove=1 AND M.SendToApprover=1 AND M.IsReject=0 then 'Acknowledged'
		                when M.SendToApprover=1 AND M.IsApprove=0 AND M.IsReject=0 then 'Send To Approver'
		                when M.IsApprove=1 AND M.IsReject=0 AND M.SendToApprover=1 then 'Approved'
		                when M.IsReject=1 AND M.IsApprove=0 AND  M.IsAcknowledged=0 AND M.SendToApprover=1  then 'Reject'
		                when M.SendToApprover=0 AND M.IsReject=1 AND M.IsApprove=0 AND M.IsAcknowledged=0 then 'Not Sending'
	                ELSE 'Not Sending'
	                END)Status

                    From {TableNames.PROJECTION_YARN_BOOKING_MASTER} M
                    INNER Join {DbNames.EPYSL}..LoginUser L ON L.UserCode = M.BookingByID
                    LEFT Join {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = M.RequiredByID
                    LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
                    LEFT JOIN {DbNames.EPYSL}..Employee R ON R.EmployeeCode = LU.EmployeeCode
                    LEFT JOIN {DbNames.EPYSL}..EmployeeDepartment ED ON ED.DepertmentID = M.DepartmentID
                    {sqlBuyerPerInnerJoin}
                    {sqlDepWise1}

                    UNION

                    Select M.PYBookingID, M.PYBookingNo, M.PYBookingDate, E.EmployeeName BookingByName,
                    R.EmployeeName RequiredByName, M.CompanyID,M.DepartmentID, M.isMarketingFlag,
                    (case when ED.DepertmentDescription In ('Production Management Control','Operation[Textile]','Operation','Planning, Monitoring & Control','Knitting') Then 'Textile'
                    When ED.DepertmentDescription = 'Research & Development' Then 'R&D' Else ED.DepertmentDescription End) DepertmentDescription,
	                M.IsApprove, M.IsAcknowledged, M.IsReject, M.IsCancel, M.SendToApprover,
	                (Case
		                when M.IsAcknowledged=1 AND M.IsApprove=1 AND M.SendToApprover=1 AND M.IsReject=0 then 'Acknowledged'
		                when M.SendToApprover=1 AND M.IsApprove=0 AND M.IsReject=0 then 'Send To Approver'
		                when M.IsApprove=1 AND M.IsReject=0 AND M.SendToApprover=1 then 'Approved'
		                when M.IsReject=1 AND M.IsApprove=0 AND  M.IsAcknowledged=0 AND M.SendToApprover=1  then 'Reject'
		                when M.SendToApprover=0 AND M.IsReject=1 AND M.IsApprove=0 AND M.IsAcknowledged=0 then 'Not Sending'
	                ELSE 'Not Sending'
	                END)Status

                    From {TableNames.PROJECTION_YARN_BOOKING_MASTER} M
                    INNER Join {DbNames.EPYSL}..LoginUser L ON L.UserCode = M.BookingByID
                    LEFT Join {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = M.RequiredByID
                    LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
                    LEFT JOIN {DbNames.EPYSL}..Employee R ON R.EmployeeCode = LU.EmployeeCode
                    LEFT JOIN {DbNames.EPYSL}..EmployeeDepartment ED ON ED.DepertmentID = M.DepartmentID
                    LEFT JOIN {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} BBT ON BBT.PYBookingID = M.PYBookingID
                    WHERE BBT.PYBookingBuyerAndBuyerTeamID IS NULL
                ),
                B AS
                (
	                SELECT M.PYBookingID, Buyer = STRING_AGG(C.ShortName, ',')
	                FROM M
	                INNER JOIN {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} A ON A.PYBookingID = M.PYBookingID
	                INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = A.BuyerID AND A.BuyerID > 0
	                GROUP BY M.PYBookingID
                ),
                BT AS
                (
	                SELECT M.PYBookingID, BuyerTeam = STRING_AGG(C.ShortName, ',')
	                FROM M
	                INNER JOIN {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} A ON A.PYBookingID = M.PYBookingID
	                INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = A.BuyerTeamID AND A.BuyerTeamID > 0
	                GROUP BY M.PYBookingID
                ),
                FinalList AS
                (
	                SELECT M.*, 
	                Buyer = CASE WHEN ISNULL(B.Buyer,'') <> '' THEN B.Buyer
				                 WHEN M.DepartmentID IN ({EnumDepertmentDescription.Knitting},{EnumDepertmentDescription.Operation},{EnumDepertmentDescription.OperationTextile},{EnumDepertmentDescription.PlanningMonitoringAndControl},{EnumDepertmentDescription.ProductionManagementControl}) THEN 'Textile Advance'
				                 WHEN M.DepartmentID IN ({EnumDepertmentDescription.ResearchAndDevelopment}) THEN 'R&D'
				                 ELSE B.Buyer
			                END, 
	                BuyerTeam = BT.BuyerTeam
	                FROM M
	                LEFT JOIN B ON B.PYBookingID = M.PYBookingID
	                LEFT JOIN BT ON BT.PYBookingID = M.PYBookingID
                )
                SELECT *, Count(*) Over() TotalRows FROM FinalList ";
            }
            else
            {
                sql = $@"
                With 
                {sqlBuyerPermission}
                FR As
                (
                    Select M.PYBookingID, M.PYBookingNo, M.PYBookingDate, E.EmployeeName BookingByName,
	                R.EmployeeName RequiredByName, M.CompanyID,M.DepartmentID, M.isMarketingFlag,
	                (case when ED.DepertmentDescription In ('Production Management Control','Operation[Textile]','Operation','Planning, Monitoring & Control','Knitting') Then 'Textile'
	                When ED.DepertmentDescription = 'Research & Development' Then 'R&D' Else ED.DepertmentDescription End) DepertmentDescription
	                From {TableNames.PROJECTION_YARN_BOOKING_MASTER} M
	                INNER Join {DbNames.EPYSL}..LoginUser L ON L.UserCode = M.BookingByID
	                LEFT Join {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = M.RequiredByID
	                LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
	                LEFT JOIN {DbNames.EPYSL}..Employee R ON R.EmployeeCode = LU.EmployeeCode
                    {sqlBuyerPerInnerJoin}
	                LEFT JOIN {DbNames.EPYSL}..EmployeeDepartment ED ON ED.DepertmentID = M.DepartmentID 
                    WHERE M.IsApprove = 0 AND M.IsAcknowledged = 0 AND M.IsReject = 0  
                    AND M.IsCancel = 0 AND M.SendToApprover = 0  {sqlDepWise}

                    UNION

                    Select M.PYBookingID, M.PYBookingNo, M.PYBookingDate, E.EmployeeName BookingByName,
	                R.EmployeeName RequiredByName, M.CompanyID,M.DepartmentID, M.isMarketingFlag,
	                (case when ED.DepertmentDescription In ('Production Management Control','Operation[Textile]','Operation','Planning, Monitoring & Control','Knitting') Then 'Textile'
	                When ED.DepertmentDescription = 'Research & Development' Then 'R&D' Else ED.DepertmentDescription End) DepertmentDescription
	                From {TableNames.PROJECTION_YARN_BOOKING_MASTER} M
	                INNER Join {DbNames.EPYSL}..LoginUser L ON L.UserCode = M.BookingByID
	                LEFT Join {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = M.RequiredByID
	                LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
	                LEFT JOIN {DbNames.EPYSL}..Employee R ON R.EmployeeCode = LU.EmployeeCode
                    LEFT JOIN {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} BBT ON BBT.PYBookingID = M.PYBookingID
	                LEFT JOIN {DbNames.EPYSL}..EmployeeDepartment ED ON ED.DepertmentID = M.DepartmentID 
                    WHERE M.IsApprove = 0 AND M.IsAcknowledged = 0 AND M.IsReject = 0  
                    AND M.IsCancel = 0 AND M.SendToApprover = 0 AND BBT.PYBookingBuyerAndBuyerTeamID IS NULL {sqlDepWise}
                ),
                B AS
                (
	                SELECT M.PYBookingID, Buyer = STRING_AGG(C.ShortName, ',')
	                FROM FR M
	                INNER JOIN {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} A ON A.PYBookingID = M.PYBookingID
	                INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = A.BuyerID AND A.BuyerID > 0
	                GROUP BY M.PYBookingID
                ),
                BT AS
                (
	                SELECT M.PYBookingID, BuyerTeam = STRING_AGG(C.ShortName, ',')
	                FROM FR M
	                INNER JOIN {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} A ON A.PYBookingID = M.PYBookingID
	                INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = A.BuyerTeamID AND A.BuyerTeamID > 0
	                GROUP BY M.PYBookingID
                ),
                FinalList AS
                (
	                SELECT M.*, 
	                Buyer = CASE WHEN ISNULL(B.Buyer,'') <> '' THEN B.Buyer
				                 WHEN M.DepartmentID IN ({EnumDepertmentDescription.Knitting},{EnumDepertmentDescription.Operation},{EnumDepertmentDescription.OperationTextile},{EnumDepertmentDescription.PlanningMonitoringAndControl},{EnumDepertmentDescription.ProductionManagementControl}) THEN 'Textile Advance'
				                 WHEN M.DepartmentID IN ({EnumDepertmentDescription.ResearchAndDevelopment}) THEN 'R&D'
				                 ELSE B.Buyer
			                END, 
	                BuyerTeam = BT.BuyerTeam
	                FROM FR M
	                LEFT JOIN B ON B.PYBookingID = M.PYBookingID
	                LEFT JOIN BT ON BT.PYBookingID = M.PYBookingID
                )
                SELECT *, Count(*) Over() TotalRows FROM FinalList ";
            }
            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";
            return await _service.GetDataAsync<ProjectionYarnBookingMaster>(sql);
        }
        public async Task<ProjectionYarnBookingMaster> GetAsync(int PYBookingID, int employeeCode)
        {
            //19-Sept-2022
            var segmentNames = new
            {
                SegmentNames = new string[]
                {
                    ItemSegmentNameConstants.YARN_SUBPROGRAM_NEW,
                    ItemSegmentNameConstants.YARN_CERTIFICATIONS
                }
            };
            //End-19-Sept-2022
            var sql = $@"--Master Information
                ;With M As (
	                Select * From  {TableNames.PROJECTION_YARN_BOOKING_MASTER} Where PYBookingID = {PYBookingID}
                )
                select M.PYBookingID,M.PYBookingNo, M.PYBookingDate,PT.PYBookingID,M.BookingByID, E.Name BookingByName,
                M.RequiredByID,R.EmployeeName RequiredByName, M.CompanyID, M.Remarks,M.DepartmentID,ED.DepertmentDescription,
                BuyerIDsList = String_Agg(PT.BuyerID,','),BuyerName = String_Agg(C.Name ,','),BuyerTeamIDsList = String_Agg(PT.BuyerTeamID,','),
                BuyerTeamName = String_Agg(CCT.TeamName ,','), M.FabricBookingStartMonth, M.FabricBookingEndMonth, M.SeasonID, 
                M.FinancialYearID, M.RevisionNo, M.RevisionDate, M.IsReject, M.RejectReason,
				(case when DepertmentDescription In ('Production Management Control','Operation[Textile]','Operation','Planning, Monitoring & Control','Knitting') Then {EnumPBookingType.Textile} When DepertmentDescription = 'Research & Development' Then {EnumPBookingType.Rnd} when DepertmentDescription='Supply Chain' then {EnumPBookingType.SupplyChain} when DepertmentDescription='Merchandiser [Fabric]' then {EnumPBookingType.Merchandising} when DepertmentDescription='Merchandising' then {EnumPBookingType.Merchandising} Else 0 End) [PBookingType]
                From  M
                LEFT JOIN {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} PT ON PT.PYBookingID=M.PYBookingID
                LEFT JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = PT.BuyerID AND C.ContactID > 0
	            LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT  ON CCT.CategoryTeamID = PT.BuyerTeamID
                LEFT JOIN {DbNames.EPYSL}..LoginUser E ON E.UserCode = M.BookingByID
                LEFT JOIN {DbNames.EPYSL}..Employee R ON R.EmployeeCode =M.RequiredByID
                LEFT JOIN {DbNames.EPYSL}..EmployeeDepartment ED ON ED.DepertmentID = M.DepartmentID
                GROUP BY M.PYBookingID,M.PYBookingNo, M.PYBookingDate,PT.PYBookingID,M.BookingByID, E.Name,
                M.RequiredByID,R.EmployeeName, M.CompanyID, M.Remarks,M.DepartmentID,ED.DepertmentDescription, 
                M.FabricBookingStartMonth, M.FabricBookingEndMonth, M.SeasonID, M.FinancialYearID, M.RevisionNo, M.RevisionDate, M.IsReject, M.RejectReason;

                /*
                -- Child Information
                ;With C As (
	                Select * From {TableNames.PROJECTION_YARN_BOOKING_ITEM_CHILD} Where PYBookingID = {PYBookingID}
                ),
                Select C.PYBBookingChildID, C.PYBookingID, C.ItemMasterID ItemMasterID, C.QTY, C.ReqCone, C.Remarks, C.UnitID, C.PPrice,
                (CASE WHEN C.UnitID=28 THEN 'Kg' ELSE 'Kg' END) AS DisplayUnitDesc, C.ShadeCode,C.ReceiveID, 
                Coalesce(IM.Segment1ValueID, C.SegmentValueId1) Segment1ValueId, 
                Coalesce(IM.Segment2ValueID, C.SegmentValueId2) Segment2ValueId, 
                Coalesce(IM.Segment3ValueID, C.SegmentValueId3) Segment3ValueId,
                Coalesce(IM.Segment4ValueID, C.SegmentValueId4) Segment4ValueId, 
                Coalesce(IM.Segment5ValueID, C.SegmentValueId5) Segment5ValueId,  
                Coalesce(IM.Segment6ValueID, C.SegmentValueId6) Segment6ValueId,
                Coalesce(IM.Segment7ValueID, C.SegmentValueId7) Segment7ValueId,  
                ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc,
                ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc,
                ISV7.SegmentValue Segment7ValueDesc, C.DayValidDurationId
                From C
                Left Join {DbNames.EPYSL}..ItemMaster IM On C.ItemMasterID = IM.ItemMasterID
                Left Join {DbNames.EPYSL}..ItemSegmentValue ISV1 On Coalesce(IM.Segment1ValueID, C.SegmentValueId1) = ISV1.SegmentValueID
                Left Join {DbNames.EPYSL}..ItemSegmentValue ISV2 On Coalesce(IM.Segment2ValueID, C.SegmentValueId2) = ISV2.SegmentValueID
                Left Join {DbNames.EPYSL}..ItemSegmentValue ISV3 On Coalesce(IM.Segment3ValueID, C.SegmentValueId3) = ISV3.SegmentValueID
                Left Join {DbNames.EPYSL}..ItemSegmentValue ISV4 On Coalesce(IM.Segment4ValueID, C.SegmentValueId4) = ISV4.SegmentValueID
                Left Join {DbNames.EPYSL}..ItemSegmentValue ISV5 On Coalesce(IM.Segment5ValueID, C.SegmentValueId5) = ISV5.SegmentValueID
                Left Join {DbNames.EPYSL}..ItemSegmentValue ISV6 On Coalesce(IM.Segment6ValueID, C.SegmentValueId6) = ISV6.SegmentValueID
                Left Join {DbNames.EPYSL}..ItemSegmentValue ISV7 On Coalesce(IM.Segment7ValueID, C.SegmentValueId7) = ISV7.SegmentValueID
                Left Join {DbNames.EPYSL}..Unit U On C.UnitID = U.UnitID;
                */

                -- Child Information
                ;With C As (
	                Select * From {TableNames.PROJECTION_YARN_BOOKING_ITEM_CHILD} Where PYBookingID = {PYBookingID}
                ),
                YRCV As
                (
	                Select C.PYBBookingChildID, ReceiveID = MAX(YRM.ReceiveID)
	                From C
	                Inner Join {TableNames.YARN_PR_CHILD} YPC ON YPC.PYBBookingChildID = C.PYBBookingChildID AND YPC.ItemMasterID = C.ItemMasterID
	                Inner Join {TableNames.YarnPOMaster} YPOM ON YPOM.PRMasterID=YPC.YarnPRMasterID
	                Inner Join {TableNames.YARN_RECEIVE_MASTER} YRM ON YRM.POID=YPOM.YPOMasterID
	                Inner Join {TableNames.YARN_RECEIVE_CHILD} YRC ON YRC.ReceiveID=YRM.ReceiveID AND YRC.ItemMasterID = C.ItemMasterID
	                GROUP BY C.PYBBookingChildID
                )
                Select C.PYBBookingChildID, C.PYBookingID, C.ItemMasterID ItemMasterID, C.QTY, C.ReqCone, C.Remarks, C.UnitID, C.PPrice,
                (CASE WHEN C.UnitID=28 THEN 'Kg' ELSE 'Kg' END) AS DisplayUnitDesc, C.ShadeCode,
                IsReceived = CASE When ISNULL(YRCV.ReceiveID,0) > 0 Then 1 Else 0 END,
                Coalesce(IM.Segment1ValueID, C.SegmentValueId1) Segment1ValueId, 
                Coalesce(IM.Segment2ValueID, C.SegmentValueId2) Segment2ValueId, 
                Coalesce(IM.Segment3ValueID, C.SegmentValueId3) Segment3ValueId,
                Coalesce(IM.Segment4ValueID, C.SegmentValueId4) Segment4ValueId, 
                Coalesce(IM.Segment5ValueID, C.SegmentValueId5) Segment5ValueId,  
                Coalesce(IM.Segment6ValueID, C.SegmentValueId6) Segment6ValueId,
                Coalesce(IM.Segment7ValueID, C.SegmentValueId7) Segment7ValueId,  
                ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc,
                ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc,
                ISV7.SegmentValue Segment7ValueDesc, C.DayValidDurationId
                From C
                LEFT JOIN YRCV ON YRCV.PYBBookingChildID = C.PYBBookingChildID
                Left Join {DbNames.EPYSL}..ItemMaster IM On C.ItemMasterID = IM.ItemMasterID
                Left Join {DbNames.EPYSL}..ItemSegmentValue ISV1 On Coalesce(IM.Segment1ValueID, C.SegmentValueId1) = ISV1.SegmentValueID
                Left Join {DbNames.EPYSL}..ItemSegmentValue ISV2 On Coalesce(IM.Segment2ValueID, C.SegmentValueId2) = ISV2.SegmentValueID
                Left Join {DbNames.EPYSL}..ItemSegmentValue ISV3 On Coalesce(IM.Segment3ValueID, C.SegmentValueId3) = ISV3.SegmentValueID
                Left Join {DbNames.EPYSL}..ItemSegmentValue ISV4 On Coalesce(IM.Segment4ValueID, C.SegmentValueId4) = ISV4.SegmentValueID
                Left Join {DbNames.EPYSL}..ItemSegmentValue ISV5 On Coalesce(IM.Segment5ValueID, C.SegmentValueId5) = ISV5.SegmentValueID
                Left Join {DbNames.EPYSL}..ItemSegmentValue ISV6 On Coalesce(IM.Segment6ValueID, C.SegmentValueId6) = ISV6.SegmentValueID
                Left Join {DbNames.EPYSL}..ItemSegmentValue ISV7 On Coalesce(IM.Segment7ValueID, C.SegmentValueId7) = ISV7.SegmentValueID
                Left Join {DbNames.EPYSL}..Unit U On C.UnitID = U.UnitID
				
                -- Child Details Information
                Select *
                From {TableNames.PROJECTION_YARN_BOOKING_ITEM_CHILDDETAILS}
                Where PYBookingID = {PYBookingID};

                -- Requisition By List
                 ;select UserCode [id],Name [text] from {DbNames.EPYSL}..LoginUser;

                -- Required By List
                /*;{CommonQueries.GetYarnAndCDAUsers()}*/

                /*;{CommonQueries.GetContactsByCategoryType(ContactCategoryNames.BUYER)};*/

                Select Cast(C.ContactID As varchar) [id], C.ShortName [text]
                From {DbNames.EPYSL}..EmployeeAssignContactTeam a
                Inner Join {DbNames.EPYSL}..ContactAssignTeam b on b.CategoryTeamID = a.CategoryTeamID
                Inner Join {DbNames.EPYSL}..Contacts C on C.ContactID = b.ContactID
                Inner Join {DbNames.EPYSL}..ContactCategoryChild CCC On C.ContactID = CCC.ContactID
                Inner Join {DbNames.EPYSL}..ContactCategoryHK CC ON CC.ContactCategoryID = CCC.ContactCategoryID
                Where CC.ContactCategoryName = 'Buyer' And C.Name != 'Select'
                And a.IsActive = 1 And a.EmployeeCode = {employeeCode}
                Group By C.ContactID, C.ShortName
                Order By C.ShortName;

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
                Select Cast (CE.CompanyID as varchar) [id], CE.CompanyName + '(' + CE.ShortName + ')' [text]
                From
                (
                    select SubGroupID, ContactID
                    From {DbNames.EPYSL}..SupplierItemGroupStatus Group By SubGroupID, ContactID
                ) SIGS
                Inner Join {DbNames.EPYSL}..Contacts C On SIGS.ContactID = C.ContactID
                Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = SIGS.SubGroupID
                Inner Join {DbNames.EPYSL}..ContactAdditionalInfo CAI On CAI.ContactID = SIGS.ContactID
                Inner Join {DbNames.EPYSL}..CompanyEntity CE On C.MappingCompanyID = CE.CompanyID
                Where ISG.SubGroupName = 'Fabric' And Isnull(CAI.InHouse,0) = 1 Group by CE.CompanyID, CE.CompanyName, CE.ShortName;

                select DepertmentID as [id],(case when DepertmentDescription In ('Production Management Control','Operation[Textile]','Operation','Planning, Monitoring & Control','Knitting') Then 'Textile' When DepertmentDescription = 'Research & Development' Then 'R&D' Else DepertmentDescription End) [text]
                From {DbNames.EPYSL}..EmployeeDepartment
                where DepertmentDescription in('Supply Chain','Merchandising','Marketing & Merchandising','Production Management Control','Operation[Textile]','Operation','Planning, Monitoring & Control','Knitting','Research & Development')

                /*Select Cast (CE.CompanyID as varchar) [id] , C.Name + '(' + C.ShortName + ')' [text]
                From {DbNames.EPYSL}..CompanyEntity CE Inner Join {DbNames.EPYSL}..Contacts C On C.MappingCompanyID = CE.CompanyID; */

                ;Select CAST(SeasonID AS VARCHAR) AS id, SeasonName AS text  from {DbNames.EPYSL}..ContactSeason;

                ;Select CAST(Max(FinancialYearID) AS VARCHAR) id, YearName AS text
                From {DbNames.EPYSL}..FinancialYear
                --Where YearNo >= {DateTime.Now.Year}
                Group By YearName;

                -- Item Segments
                {CommonQueries.GetCertifications()};

                -- Fabric Components
                {CommonQueries.GetFabricComponents(EntityTypeNameConstants.FABRIC_TYPE)};

                -- Shade book
                {CommonQueries.GetYarnShadeBooks()}

                --Item Segments
                {CommonQueries.GetSubPrograms()}; 

                -- DayValidDuration
                {CommonQueries.GetDayValidDurations()};

                --Fiber-SubProgram-Certifications Mapping Setup
                Select * FROM {TableNames.FIBER_SUBPROGRAM_CERTIFICATIONS_FILTER_SETUP}";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql, segmentNames);
                ProjectionYarnBookingMaster data = await records.ReadFirstOrDefaultAsync<ProjectionYarnBookingMaster>();
                Guard.Against.NullObject(data);

                data.ProjectionYarnBookingItemChilds = records.Read<ProjectionYarnBookingItemChild>().ToList();
                List<ProjectionYarnBookingItemChildDetails> PYBChildDetils = records.Read<ProjectionYarnBookingItemChildDetails>().ToList();
                data.BookingByList = await records.ReadAsync<Select2OptionModel>();
                data.BuyerList = await records.ReadAsync<Select2OptionModel>();
                data.CompanyList = await records.ReadAsync<Select2OptionModel>();
                data.DepartmentList = await records.ReadAsync<Select2OptionModel>();
                data.ProjectionYarnBookingItemChilds.ForEach(childDetails =>
                {
                    childDetails.PYBItemChildDetails = PYBChildDetils.Where(c => c.PYBBookingChildID == childDetails.PYBBookingChildID).ToList();
                });
                data.SeasonList = await records.ReadAsync<Select2OptionModel>();
                data.FinancialYearList = await records.ReadAsync<Select2OptionModel>();
                var _recvCertifications = await records.ReadAsync<Select2OptionModelExtended>();
                data.Certifications = _recvCertifications.Where(x => x.desc == ItemSegmentNameConstants.YARN_CERTIFICATIONS);
                data.FabricComponentsNew = await records.ReadAsync<Select2OptionModel>();
                data.YarnShadeBooks = await records.ReadAsync<Select2OptionModel>();
                var itemSegments = await records.ReadAsync<Select2OptionModel>();
                data.YarnSubProgramNews = itemSegments.Where(x => x.desc == ItemSegmentNameConstants.YARN_SUBPROGRAM_NEW);

                data.DayValidDurations = await records.ReadAsync<Select2OptionModel>();
                data.DayValidDurations = CommonFunction.GetDayValidDurations(data.DayValidDurations, string.Join(",", data.ProjectionYarnBookingItemChilds.Where(x => x.DayValidDurationId > 0).Select(x => x.DayValidDurationId).Distinct()));
                data.IsCheckDVD = data.PYBookingDate < CommonConstent.YarnSourcingModeImplementDate ? false : true;
                data.FabricComponentMappingSetupList = records.Read<FabricComponentMappingSetup>().ToList();

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

        public async Task<ProjectionYarnBookingMaster> GetBuyerTeamAsync(string buyerId, int employeeCode)
        {
            var sql = $@"   
            Select CAST(CCT.CategoryTeamID AS VARCHAR) AS id, CCT.TeamName AS text
            From {DbNames.EPYSL}..ContactCategoryTeam CCT
            Inner Join {DbNames.EPYSL}..ContactAssignTeam CAT ON CAT.CategoryTeamID = CCT.CategoryTeamID
            Inner Join {DbNames.EPYSL}..EmployeeAssignContactTeam EACT On EACT.CategoryTeamID = CCT.CategoryTeamID
            Where CAT.ContactID IN ({buyerId}) And EACT.EmployeeCode = {employeeCode} And EACT.IsActive = 1
            Group By CCT.CategoryTeamID, CCT.TeamName ";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                ProjectionYarnBookingMaster data = new ProjectionYarnBookingMaster();
                data.BuyerTeamList = await records.ReadAsync<Select2OptionModel>();
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

        public async Task<ProjectionYarnBookingMaster> GetNewAsync(int employeeCode)
        {
            //19-Sept-2022
            var segmentNames = new
            {
                SegmentNames = new string[]
                {
                    ItemSegmentNameConstants.YARN_SUBPROGRAM_NEW,
                    ItemSegmentNameConstants.YARN_CERTIFICATIONS
                }
            };
            //End-19-Sept-2022
            var query =
                $@"
                -- Requisition By
                select UserCode [id],Name [text] from {DbNames.EPYSL}..LoginUser;
                -- Required By
                /*{CommonQueries.GetYarnAndCDAUsers()};*/

                /*{CommonQueries.GetContactsByCategoryType(ContactCategoryNames.BUYER)}; */

                Select Cast(C.ContactID As varchar) [id], C.ShortName [text]
                From {DbNames.EPYSL}..EmployeeAssignContactTeam a
                Inner Join {DbNames.EPYSL}..ContactAssignTeam b on b.CategoryTeamID = a.CategoryTeamID
                Inner Join {DbNames.EPYSL}..Contacts C on C.ContactID = b.ContactID
                Inner Join {DbNames.EPYSL}..ContactCategoryChild CCC On C.ContactID = CCC.ContactID
                Inner Join {DbNames.EPYSL}..ContactCategoryHK CC ON CC.ContactCategoryID = CCC.ContactCategoryID
                Where CC.ContactCategoryName = 'Buyer' And C.Name != 'Select'
                And a.IsActive = 1 And a.EmployeeCode = {employeeCode}
                Group By C.ContactID, C.ShortName
                Order By C.ShortName;

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
                Select Cast (CE.CompanyID as varchar) [id], CE.CompanyName + '(' + CE.ShortName + ')' [text]
                From
                (
                    select SubGroupID, ContactID
                    From {DbNames.EPYSL}..SupplierItemGroupStatus Group By SubGroupID, ContactID
                ) SIGS
                Inner Join {DbNames.EPYSL}..Contacts C On SIGS.ContactID = C.ContactID
                Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = SIGS.SubGroupID
                Inner Join {DbNames.EPYSL}..ContactAdditionalInfo CAI On CAI.ContactID = SIGS.ContactID
                Inner Join {DbNames.EPYSL}..CompanyEntity CE On C.MappingCompanyID = CE.CompanyID
                Where ISG.SubGroupName = 'Fabric' And Isnull(CAI.InHouse,0) = 1 Group by CE.CompanyID, CE.CompanyName, CE.ShortName;

                Select DepertmentID as [id],(case when DepertmentDescription In ('Production Management Control','Operation[Textile]','Operation','Planning, Monitoring & Control','Knitting') Then 'Textile' When DepertmentDescription = 'Research & Development' Then 'R&D' Else DepertmentDescription End) [text]
                From {DbNames.EPYSL}..EmployeeDepartment
                Where DepertmentDescription in('Supply Chain','Merchandising','Marketing & Merchandising','Production Management Control','Operation[Textile]','Operation','Planning, Monitoring & Control','Knitting','Research & Development')
                /*Select Cast (CE.CompanyID as varchar) [id] , C.Name + '(' + C.ShortName + ')' [text]
                From {DbNames.EPYSL}..CompanyEntity CE Inner Join {DbNames.EPYSL}..Contacts C On C.MappingCompanyID = CE.CompanyID; */

                ;Select CAST(SeasonID AS VARCHAR) AS id, SeasonName AS text  from {DbNames.EPYSL}..ContactSeason;

                ;Select CAST(Max(FinancialYearID) AS VARCHAR) id, YearName AS text
                From {DbNames.EPYSL}..FinancialYear
                --Where YearNo >= {DateTime.Now.Year}
                Group By YearName;

                -- Item Segments
                {CommonQueries.GetCertifications()};

                ---- Fabric Components 
                {CommonQueries.GetFabricComponents(EntityTypeNameConstants.FABRIC_TYPE)};

                -- Shade book
                {CommonQueries.GetYarnShadeBooks()};

                -- Item Segments
                {CommonQueries.GetSubPrograms()};

                -- DayValidDuration
                {CommonQueries.GetDayValidDurations()};

                --Fiber - SubProgram - Certifications Mapping Setup
                Select* FROM {TableNames.FIBER_SUBPROGRAM_CERTIFICATIONS_FILTER_SETUP}";
            try
            {
                await _connection.OpenAsync();
                //var records = await _connection.QueryMultipleAsync(query);
                var records = await _connection.QueryMultipleAsync(query, segmentNames);
                ProjectionYarnBookingMaster data = new ProjectionYarnBookingMaster();
                data.BookingByList = await records.ReadAsync<Select2OptionModel>();
                //data.RequiredByList = await records.ReadAsync<Select2OptionModel>();
                data.BuyerList = await records.ReadAsync<Select2OptionModel>();
                data.CompanyList = await records.ReadAsync<Select2OptionModel>();
                data.DepartmentList = await records.ReadAsync<Select2OptionModel>();
                data.SeasonList = await records.ReadAsync<Select2OptionModel>();
                data.FinancialYearList = await records.ReadAsync<Select2OptionModel>();

                var _recvCertifications = await records.ReadAsync<Select2OptionModelExtended>();
                data.Certifications = _recvCertifications.Where(x => x.desc == ItemSegmentNameConstants.YARN_CERTIFICATIONS);
                data.FabricComponentsNew = await records.ReadAsync<Select2OptionModel>();
                data.YarnShadeBooks = await records.ReadAsync<Select2OptionModel>();
                var itemSegments = await records.ReadAsync<Select2OptionModel>();
                data.YarnSubProgramNews = itemSegments.Where(x => x.desc == ItemSegmentNameConstants.YARN_SUBPROGRAM_NEW);

                data.DayValidDurations = await records.ReadAsync<Select2OptionModel>();
                data.DayValidDurations = CommonFunction.GetDayValidDurations(data.DayValidDurations, string.Join(",", data.ProjectionYarnBookingItemChilds.Where(x => x.DayValidDurationId > 0).Select(x => x.DayValidDurationId).Distinct()));
                data.IsCheckDVD = true;
                data.FabricComponentMappingSetupList = records.Read<FabricComponentMappingSetup>().ToList();

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

        public async Task<ProjectionYarnBookingMaster> GetNewPYBookingID(string ids, int employeeCode)
        {
            var sql = $@"--Master Information
                ;With M As (
	                Select * From  {TableNames.PROJECTION_YARN_BOOKING_MASTER} Where PYBookingID = {ids}
                )
                select M.PYBookingID, M.PYBookingNo, M.PYBookingDate,M.BuyerID,C.Name Buyer,M.BookingByID, E.EmployeeName BookingByName,
                M.RequiredByID,R.EmployeeName RequiredByName, M.CompanyID, M.Remarks,M.DepartmentID,ED.DepertmentDescription, M.FabricBookingStartMonth,
                M.FabricBookingEndMonth, M.SeasonID, M.FinancialYearID
		        From  M
		        LEFT JOIN {DbNames.EPYSL}..Contacts  C ON C.ContactID = M.BuyerID
		        LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = M.BookingByID
		        LEFT JOIN {DbNames.EPYSL}..Employee R ON R.EmployeeCode = M.RequiredByID
                LEFT JOIN {DbNames.EPYSL}..EmployeeDepartment ED ON ED.DepertmentID = M.DepartmentID

                -- Child Information
                ;With C As (
	                Select * From {TableNames.PROJECTION_YARN_BOOKING_ITEM_CHILD} Where PYBookingID = {ids}
                )
                Select C.ItemMasterID ItemMasterID, C.QTY, C.ReqCone, C.Remarks, C.UnitID,(CASE WHEN C.UnitID=28 THEN 'Kg' ELSE 'Kg' END) AS DisplayUnitDesc,
                ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc,
                ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc,
                ISV7.SegmentValue Segment7ValueDesc, C.ShadeCode
                From C
                Left Join {DbNames.EPYSL}..ItemMaster IM On C.ItemMasterID = IM.ItemMasterID
                Left Join {DbNames.EPYSL}..ItemSegmentValue ISV1 On IM.Segment1ValueID = ISV1.SegmentValueID
                Left Join {DbNames.EPYSL}..ItemSegmentValue ISV2 On IM.Segment2ValueID = ISV2.SegmentValueID
                Left Join {DbNames.EPYSL}..ItemSegmentValue ISV3 On IM.Segment3ValueID = ISV3.SegmentValueID
                Left Join {DbNames.EPYSL}..ItemSegmentValue ISV4 On IM.Segment4ValueID = ISV4.SegmentValueID
                Left Join {DbNames.EPYSL}..ItemSegmentValue ISV5 On IM.Segment5ValueID = ISV5.SegmentValueID
                Left Join {DbNames.EPYSL}..ItemSegmentValue ISV6 On IM.Segment6ValueID = ISV6.SegmentValueID
                Left Join {DbNames.EPYSL}..ItemSegmentValue ISV7 On IM.Segment7ValueID = ISV7.SegmentValueID
                Left Join {DbNames.EPYSL}..Unit U On C.UnitID = U.UnitID

                -- Requisition By List
                ; select UserCode [id],Name [text] from {DbNames.EPYSL}..LoginUser;

                -- Required By List
                /*;{CommonQueries.GetYarnAndCDAUsers()}*/

                /* ;{CommonQueries.GetContactsByCategoryType(ContactCategoryNames.BUYER)}; */

                Select Cast(C.ContactID As varchar) [id], C.ShortName [text]
                From {DbNames.EPYSL}..EmployeeAssignContactTeam a
                Inner Join {DbNames.EPYSL}..ContactAssignTeam b on b.CategoryTeamID = a.CategoryTeamID
                Inner Join {DbNames.EPYSL}..Contacts C on C.ContactID = b.ContactID
                Inner Join {DbNames.EPYSL}..ContactCategoryChild CCC On C.ContactID = CCC.ContactID
                Inner Join {DbNames.EPYSL}..ContactCategoryHK CC ON CC.ContactCategoryID = CCC.ContactCategoryID
                Where CC.ContactCategoryName = 'Buyer' And C.Name != 'Select'
                And a.IsActive = 1 And a.EmployeeCode = {employeeCode}
                Group By C.ContactID, C.ShortName
                Order By C.ShortName;

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
                Select Cast (CE.CompanyID as varchar) [id], CE.CompanyName + '(' + CE.ShortName + ')' [text]
                From
                (
                    select SubGroupID, ContactID
                    From {DbNames.EPYSL}..SupplierItemGroupStatus Group By SubGroupID, ContactID
                ) SIGS
                Inner Join {DbNames.EPYSL}..Contacts C On SIGS.ContactID = C.ContactID
                Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = SIGS.SubGroupID
                Inner Join {DbNames.EPYSL}..ContactAdditionalInfo CAI On CAI.ContactID = SIGS.ContactID
                Inner Join {DbNames.EPYSL}..CompanyEntity CE On C.MappingCompanyID = CE.CompanyID
                Where ISG.SubGroupName = 'Fabric' And Isnull(CAI.InHouse,0) = 1 Group by CE.CompanyID, CE.CompanyName, CE.ShortName;

                Select DepertmentID as [id],(case when DepertmentDescription In ('Production Management Control','Operation[Textile]','Operation','Planning, Monitoring & Control','Knitting') Then 'Textile' When DepertmentDescription = 'Research & Development' Then 'R&D' Else DepertmentDescription End) [text]
                From {DbNames.EPYSL}..EmployeeDepartment
                Where DepertmentDescription in('Supply Chain','Merchandising','Marketing & Merchandising','Production Management Control','Operation[Textile]','Operation','Planning, Monitoring & Control','Knitting','Research & Development')
                /*Select Cast (CE.CompanyID as varchar) [id] , C.Name + '(' + C.ShortName + ')' [text]
                From {DbNames.EPYSL}..CompanyEntity CE Inner Join {DbNames.EPYSL}..Contacts C On C.MappingCompanyID = CE.CompanyID; */

                ;Select CAST(SeasonID AS VARCHAR) AS id, SeasonName AS text  from {DbNames.EPYSL}..ContactSeason;

                ;Select CAST(Max(FinancialYearID) AS VARCHAR) id, YearName AS text
                From {DbNames.EPYSL}..FinancialYear
                --Where YearNo >= {DateTime.Now.Year}
                Group By YearName;

                -- Shade book
                {CommonQueries.GetYarnShadeBooks()}

                -- DayValidDuration
                {CommonQueries.GetDayValidDurations()};
            ";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                ProjectionYarnBookingMaster data = await records.ReadFirstOrDefaultAsync<ProjectionYarnBookingMaster>();
                Guard.Against.NullObject(data);
                data.ProjectionYarnBookingItemChilds = records.Read<ProjectionYarnBookingItemChild>().ToList();
                data.BookingByList = await records.ReadAsync<Select2OptionModel>();
                //data.RequiredByList = await records.ReadAsync<Select2OptionModel>();
                data.BuyerList = await records.ReadAsync<Select2OptionModel>();
                data.CompanyList = await records.ReadAsync<Select2OptionModel>();
                data.DepartmentList = await records.ReadAsync<Select2OptionModel>();
                //data.DepartmentList.Where(x => x.text == "Production Management Control").ToList().ForEach(y => y.text = "Textile");

                data.SeasonList = await records.ReadAsync<Select2OptionModel>();
                data.FinancialYearList = await records.ReadAsync<Select2OptionModel>();
                data.YarnShadeBooks = await records.ReadAsync<Select2OptionModel>();

                data.DayValidDurations = await records.ReadAsync<Select2OptionModel>();
                data.DayValidDurations = CommonFunction.GetDayValidDurations(data.DayValidDurations, string.Join(",", data.ProjectionYarnBookingItemChilds.Where(x => x.DayValidDurationId > 0).Select(x => x.DayValidDurationId).Distinct()));
                data.IsCheckDVD = data.PYBookingDate < CommonConstent.YarnSourcingModeImplementDate ? false : true;

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

        public async Task<ProjectionYarnBookingMaster> GetAllAsync(int id)
        {
            var sql = $@"
            ;Select M.*,
            DepertmentDescription = (case when ED.DepertmentDescription In ('Production Management Control','Operation[Textile]','Operation','Planning, Monitoring & Control','Knitting') Then 'Textile' When ED.DepertmentDescription = 'Research & Development' Then 'R&D' Else ED.DepertmentDescription End)
            From {TableNames.PROJECTION_YARN_BOOKING_MASTER} M
            LEFT JOIN {DbNames.EPYSL}..EmployeeDepartment ED ON ED.DepertmentID = M.DepartmentID
            WHERE M.PYBookingID = {id}

            -- Child Information
	        ;Select IC.*
            ,Segment1ValueDesc = ISV1.SegmentValue
            ,Segment2ValueDesc = ISV2.SegmentValue
            ,Segment3ValueDesc = ISV3.SegmentValue
            ,Segment4ValueDesc = ISV4.SegmentValue
            ,Segment5ValueDesc = ISV5.SegmentValue
            ,Segment6ValueDesc = ISV6.SegmentValue
            From {TableNames.PROJECTION_YARN_BOOKING_ITEM_CHILD} IC
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IC.SegmentValueId1
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IC.SegmentValueId2
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IC.SegmentValueId3
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IC.SegmentValueId4
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IC.SegmentValueId5
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IC.SegmentValueId6
            Where IC.PYBookingID = {id};

            ;Select * From {TableNames.PROJECTION_YARN_BOOKING_ITEM_CHILDDETAILS} Where PYBookingID = {id}

            ;Select * From {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} Where PYBookingID = {id}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                ProjectionYarnBookingMaster data = await records.ReadFirstOrDefaultAsync<ProjectionYarnBookingMaster>();
                Guard.Against.NullObject(data);
                data.ProjectionYarnBookingItemChilds = records.Read<ProjectionYarnBookingItemChild>().ToList();
                data.PYBItemChildDetails = records.Read<ProjectionYarnBookingItemChildDetails>().ToList();
                data.PYBookingBuyerAndBuyerTeams = records.Read<PYBookingBuyerAndBuyerTeam>().ToList();
                data.ProjectionYarnBookingItemChilds.ForEach(x =>
                {
                    x.PYBItemChildDetails = data.PYBItemChildDetails.Where(c => c.PYBBookingChildID == x.PYBBookingChildID).ToList();
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
        public async Task<ProjectionYarnBookingMaster> GetPYBWithPRAsync(int pyBookingID, string pyBookingNo)
        {
            var sql = $@"
            ;Select M.*,
            DepertmentDescription = (case when ED.DepertmentDescription In ('Production Management Control','Operation[Textile]','Operation','Planning, Monitoring & Control','Knitting') Then 'Textile' When ED.DepertmentDescription = 'Research & Development' Then 'R&D' Else ED.DepertmentDescription End)
            From {TableNames.PROJECTION_YARN_BOOKING_MASTER} M
            LEFT JOIN {DbNames.EPYSL}..EmployeeDepartment ED ON ED.DepertmentID = M.DepartmentID
            WHERE M.PYBookingID = {pyBookingID}

            -- Child Information
	        ;Select IC.*
            ,Segment1ValueDesc = ISV1.SegmentValue
            ,Segment2ValueDesc = ISV2.SegmentValue
            ,Segment3ValueDesc = ISV3.SegmentValue
            ,Segment4ValueDesc = ISV4.SegmentValue
            ,Segment5ValueDesc = ISV5.SegmentValue
            ,Segment6ValueDesc = ISV6.SegmentValue
            From {TableNames.PROJECTION_YARN_BOOKING_ITEM_CHILD} IC
            INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = IC.ItemMasterID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
            Where IC.PYBookingID = {pyBookingID};

            ;Select * 
            From {TableNames.PROJECTION_YARN_BOOKING_ITEM_CHILDDETAILS} 
            Where PYBookingID = {pyBookingID}

            ;Select * 
            From {TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM} 
            Where PYBookingID = {pyBookingID}
            
            ;SELECT PRM.* 
            FROM {TableNames.YARN_PR_MASTER} PRM
            WHERE PRM.YDMaterialRequirementNo = '{pyBookingNo}'

            ;SELECT PRC.* 
            FROM {TableNames.YARN_PR_CHILD} PRC
            INNER JOIN {TableNames.YARN_PR_MASTER} PRM ON PRM.YarnPRMasterID = PRC.YarnPRMasterID
            WHERE PRM.YDMaterialRequirementNo = '{pyBookingNo}'

            ;SELECT POM.* 
            FROM {TableNames.YarnPOMaster} POM
            INNER JOIN {TableNames.YARN_PR_MASTER} PRM ON PRM.YarnPRMasterID = POM.PRMasterID
            WHERE PRM.YDMaterialRequirementNo = '{pyBookingNo}' AND POM.UnApprove = 1";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                ProjectionYarnBookingMaster data = await records.ReadFirstOrDefaultAsync<ProjectionYarnBookingMaster>();
                Guard.Against.NullObject(data);
                data.ProjectionYarnBookingItemChilds = records.Read<ProjectionYarnBookingItemChild>().ToList();
                data.PYBItemChildDetails = records.Read<ProjectionYarnBookingItemChildDetails>().ToList();
                data.PYBookingBuyerAndBuyerTeams = records.Read<PYBookingBuyerAndBuyerTeam>().ToList();
                data.ProjectionYarnBookingItemChilds.ForEach(x =>
                {
                    x.PYBItemChildDetails = data.PYBItemChildDetails.Where(c => c.PYBBookingChildID == x.PYBBookingChildID).ToList();
                });

                List<YarnPRMaster> yarnPRs = records.Read<YarnPRMaster>().ToList();
                List<YarnPRChild> yarnPRChilds = records.Read<YarnPRChild>().ToList();

                var pos = records.Read<YarnPOMaster>().ToList();

                yarnPRs.ForEach(x =>
                {
                    x.YarnPOMasters = pos.IsNull() ? new List<YarnPOMaster>() : pos.Where(y => y.PRMasterID == x.YarnPRMasterID).ToList();
                });

                if (yarnPRs != null && yarnPRs.Count() > 0)
                {
                    data.YarnPR = yarnPRs.First();
                    data.YarnPR.Childs = yarnPRChilds;
                }

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
        public async Task SaveAsync(ProjectionYarnBookingMaster entity, int userId)
        {
            SqlTransaction transaction = null;
            try
            {
                //Backup table data save before YDBookingMaster data update.
                if (entity.RevisionStatus == "Revision")
                {
                    await _service.ExecuteAsync(SPNames.spBackupProjectionYarnBooking, new { PYBookingID = entity.PYBookingID }, 30, CommandType.StoredProcedure);
                }

                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                int maxChildId = 0;
                int maxChildDetailsId = 0;
                int maxPYBookingBuyerAndBuyerTeamId = 0;

                switch (entity.EntityState)
                {
                    case EntityState.Added:
                        entity.PYBookingID = await _service.GetMaxIdAsync(TableNames.PROJECTION_YARN_BOOKING_MASTER);
                        entity.PYBookingNo = await _service.GetMaxNoAsync(TableNames.PROJECTION_BOOKING_NO);

                        maxChildId = await _service.GetMaxIdAsync(TableNames.PROJECTION_YARN_BOOKING_ITEM_CHILD, entity.ProjectionYarnBookingItemChilds.Count);
                        maxChildDetailsId = await _service.GetMaxIdAsync(TableNames.PROJECTION_YARN_BOOKING_ITEM_CHILDDETAILS, entity.ProjectionYarnBookingItemChilds.Sum(x => x.PYBItemChildDetails.Count));
                        maxPYBookingBuyerAndBuyerTeamId = await _service.GetMaxIdAsync(TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM, entity.PYBookingBuyerAndBuyerTeams.Count);

                        foreach (ProjectionYarnBookingItemChild item in entity.ProjectionYarnBookingItemChilds)
                        {
                            var Detailsid = item.PYBBookingChildID;
                            item.PYBBookingChildID = maxChildId++;
                            item.PYBookingID = entity.PYBookingID;
                            foreach (ProjectionYarnBookingItemChildDetails itemDtls in item.PYBItemChildDetails)
                            {
                                itemDtls.PYBBookingChildDetailsID = maxChildDetailsId++;
                                itemDtls.PYBBookingChildID = item.PYBBookingChildID;
                                itemDtls.PYBookingID = item.PYBookingID;
                            }
                        }

                        foreach (PYBookingBuyerAndBuyerTeam item in entity.PYBookingBuyerAndBuyerTeams)
                        {
                            var Detailsibt = item.PYBookingBuyerAndBuyerTeamID;
                            item.PYBookingBuyerAndBuyerTeamID = maxPYBookingBuyerAndBuyerTeamId++;
                            item.PYBookingID = entity.PYBookingID;
                        }
                        break;

                    case EntityState.Modified:
                        maxChildId = await _service.GetMaxIdAsync(TableNames.PROJECTION_YARN_BOOKING_ITEM_CHILD, entity.ProjectionYarnBookingItemChilds.FindAll(x => x.EntityState == EntityState.Added).Count);
                        maxChildDetailsId = await _service.GetMaxIdAsync(TableNames.PROJECTION_YARN_BOOKING_ITEM_CHILDDETAILS, entity.ProjectionYarnBookingItemChilds.Sum(x => x.PYBItemChildDetails.Where(y => y.EntityState == EntityState.Added).ToList().Count));
                        maxPYBookingBuyerAndBuyerTeamId = await _service.GetMaxIdAsync(TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM, entity.PYBookingBuyerAndBuyerTeams.Count);
                        foreach (var item in entity.ProjectionYarnBookingItemChilds)

                        {
                            switch (item.EntityState)
                            {
                                case EntityState.Added:
                                    item.PYBBookingChildID = maxChildId++;
                                    item.PYBookingID = entity.PYBookingID;
                                    foreach (ProjectionYarnBookingItemChildDetails itemDtls in item.PYBItemChildDetails.ToList())
                                    {
                                        itemDtls.PYBBookingChildDetailsID = maxChildDetailsId++;
                                        itemDtls.PYBBookingChildID = item.PYBBookingChildID;
                                        itemDtls.PYBookingID = item.PYBookingID;
                                        item.EntityState = EntityState.Added;
                                    }
                                    break;

                                case EntityState.Modified:
                                    foreach (ProjectionYarnBookingItemChildDetails itemDtls in item.PYBItemChildDetails.Where(y => y.EntityState == EntityState.Added).ToList())
                                    {
                                        itemDtls.PYBBookingChildDetailsID = maxChildDetailsId++;
                                        itemDtls.PYBBookingChildID = item.PYBBookingChildID;
                                        itemDtls.PYBookingID = item.PYBookingID;
                                        itemDtls.EntityState = EntityState.Added;
                                    }
                                    break;

                                case EntityState.Unchanged:
                                case EntityState.Deleted:
                                    item.EntityState = EntityState.Deleted;
                                    item.PYBItemChildDetails.SetDeleted();
                                    break;

                                default:
                                    break;
                            }
                        }
                        foreach (var item in entity.PYBookingBuyerAndBuyerTeams)
                        {
                            switch (item.EntityState)
                            {
                                case EntityState.Added:
                                    item.PYBookingBuyerAndBuyerTeamID = maxPYBookingBuyerAndBuyerTeamId++;
                                    item.PYBookingID = entity.PYBookingID;
                                    break;

                                case EntityState.Modified:
                                    item.PYBookingBuyerAndBuyerTeamID = maxPYBookingBuyerAndBuyerTeamId++;
                                    item.PYBookingID = entity.PYBookingID;
                                    break;

                                case EntityState.Unchanged:
                                case EntityState.Deleted:
                                    item.EntityState = EntityState.Deleted;
                                    //item.PYBookingBuyerAndBuyerTeams.SetDeleted();
                                    break;

                                default:
                                    break;
                            }
                        }
                        break;

                    case EntityState.Unchanged:
                    case EntityState.Deleted:
                        entity.EntityState = EntityState.Deleted;
                        entity.ProjectionYarnBookingItemChilds.SetDeleted();
                        break;

                    default:
                        break;
                }
                await _service.SaveSingleAsync(entity, transaction);
                await _service.SaveAsync(entity.ProjectionYarnBookingItemChilds, transaction);
                await _service.SaveAsync(entity.PYBookingBuyerAndBuyerTeams, transaction);

                List<ProjectionYarnBookingItemChildDetails> childRecorddetails = new List<ProjectionYarnBookingItemChildDetails>();
                entity.ProjectionYarnBookingItemChilds.ForEach(x =>
                {
                    childRecorddetails.AddRange(x.PYBItemChildDetails);
                });
                await _service.SaveAsync(childRecorddetails, transaction);
                foreach (ProjectionYarnBookingItemChildDetails item in childRecorddetails)
                {
                    //await _service.ValidationSingleAsync(item, transaction, "sp_Validation_ProjectionYarnBookingItemChildDetails", item.EntityState, userId, item.PYBBookingChildDetailsID);
                    await _connection.ExecuteAsync(SPNames.sp_Validation_ProjectionYarnBookingItemChildDetails, new { EntityState = item.EntityState, UserId = userId, PrimaryKeyId = item.PYBBookingChildDetailsID }, transaction, 30, CommandType.StoredProcedure);
                }
                transaction.Commit();
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                if (ex.Message.Contains('~')) throw new Exception(ex.Message.Split('~')[0]);
                throw ex;
            }
            finally
            {
                _connection.Close();
            }
        }

        public async Task UpdateEntityAsync(ProjectionYarnBookingMaster entity)
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
                if (ex.Message.Contains('~')) throw new Exception(ex.Message.Split('~')[0]);
                throw ex;
            }
            finally
            {
                _connection.Close();
            }
        }

        public async Task AcknowledgeEntityAsync(ProjectionYarnBookingMaster entity, YarnPRMaster yarnPRMaster)
        {
            SqlTransaction transaction = null;
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                await _service.SaveSingleAsync(entity, transaction);

                if (yarnPRMaster.EntityState == EntityState.Added)
                {
                    yarnPRMaster.YarnPRMasterID = await _service.GetMaxIdAsync(TableNames.YARN_PR_MASTER);
                    //yarnPRMaster.YarnPRNo = _signatureRepository.GetMaxNo(TableNames.YARN_PRNO);
                }

                int maxChildId = await _service.GetMaxIdAsync(TableNames.YARN_PR_CHILD, yarnPRMaster.Childs.Count(x => x.EntityState == EntityState.Added));
                foreach (YarnPRChild child in yarnPRMaster.Childs.Where(x => x.EntityState == EntityState.Added).ToList())
                {
                    child.YarnPRChildID = maxChildId++;
                    child.YarnPRMasterID = yarnPRMaster.YarnPRMasterID;
                }

                await _connection.ExecuteAsync(SPNames.spBackupYarnAutoPR, new { YarnPRMasterID = yarnPRMaster.YarnPRMasterID }, transaction, 30, CommandType.StoredProcedure);

                await _service.SaveSingleAsync(yarnPRMaster, transaction);
                await _service.SaveAsync(yarnPRMaster.Childs, transaction);
                foreach (YarnPRChild item in yarnPRMaster.Childs)
                {
                    if (yarnPRMaster.UpdatedBy.IsNull()) yarnPRMaster.UpdatedBy = 0;
                    int userId = yarnPRMaster.EntityState == EntityState.Added ? yarnPRMaster.AddedBy : (int)yarnPRMaster.UpdatedBy;
                    //await _service.ValidationSingleAsync(item, transaction, "sp_Validation_YarnPRChild_From_PYB", item.EntityState, userId, item.YarnPRChildID);
                    await _connection.ExecuteAsync(SPNames.sp_Validation_YarnPRChild_From_PYB, new { PrimaryKeyId = item.YarnPRChildID, UserId = userId, EntityState = item.EntityState }, transaction, 30, CommandType.StoredProcedure);
                }
                await _service.SaveAsync(yarnPRMaster.YarnPOMasters, transaction);
                transaction.Commit();
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                if (ex.Message.Contains('~')) throw new Exception(ex.Message.Split('~')[0]);
                throw ex;
            }
            finally
            {
                _connection.Close();
            }
        }

    }
}
