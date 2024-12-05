using Dapper;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Application.Interfaces.Booking;
using EPYSLTEXCore.Application.Interfaces.Repositories;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Gmt.Booking;
using EPYSLTEXCore.Infrastructure.Entities.Gmt.General;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Booking;
using EPYSLTEXCore.Infrastructure.Entities.Tex.CountEntities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Fabric;
using EPYSLTEXCore.Infrastructure.Entities.Tex.General;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Yarn;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Application.Services.Booking
{
    public class FBookingAcknowledgeService : IFBookingAcknowledgeService
    {
        private readonly IDapperCRUDService<FBookingAcknowledge> _service;
        private readonly SqlConnection _connection;
        private readonly IDapperCRUDService<SampleBookingMaster> _gmtservice;
        string _startingDate = "17-Feb-2024";
        //
        public FBookingAcknowledgeService(IDapperCRUDService<SampleBookingMaster> gmtservice
            , IDapperCRUDService<FBookingAcknowledge> service)
        {
            _service = service;
            _service.Connection = service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _connection = service.Connection;
            _gmtservice = gmtservice;
            _gmtservice.Connection = service.GetConnection(AppConstants.GMT_CONNECTION);

#if DEBUG
            _startingDate = "17-Feb-2023";
#else
                _startingDate = "17-Feb-2024";
#endif
        }

        public async Task<List<FBookingAcknowledge>> GetPagedAsync(Status status, int isBDS, PaginationInfo paginationInfo, LoginUser AppUser)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By BookingID Desc" : paginationInfo.OrderBy;
            string queryIsProjectionAnd = "";
            string queryIsProjectionWhere = "";
            string queryProjConcept = "";
            string queryConceptJoin = "";

            if (isBDS == 3)
            {
                queryIsProjectionAnd = " AND FCM.IsBDS = 3 ";
                queryIsProjectionWhere = " WHERE FCM.IsBDS = 3 ";
                queryProjConcept = $@"FCM AS
					(
						SELECT FCM.BookingID 
						FROM {TableNames.RND_FREE_CONCEPT_MASTER} FCM 
						WHERE FCM.IsBDS = 3
						GROUP BY FCM.BookingID 
					),";
                queryConceptJoin = " INNER JOIN FCM ON FCM.BookingID = FBC.BookingID ";
            }

            string sql;
            if (status == Status.Pending)
            {
                if (isBDS != 3)
                {
                    sql = AppUser.IsSuperUser ?
                        $@"WITH BIMG As(
	                    Select BookingID, Min(ChildImgID) ChildImgID
	                    From {DbNames.EPYSL}..SampleBookingChildImage Group By BookingID
                    ), IMG As(
	                    Select I.BookingID, I.ImagePath
	                    From BIMG
	                    Inner Join {DbNames.EPYSL}..SampleBookingChildImage I On I.ChildImgID = BIMG.ChildImgID
                    ), SBM AS
                    (
                        Select a.BookingID, a.BookingNo, a.BookingDate, a.RevisionNo, a.SLNo, a.StyleNo, a.BuyerID, a.BuyerTeamID, a.SupplierID,
                        a.ExecutionCompanyID, a.SeasonID, a.Remarks, a.OrderQty, a.AddedBy, a.SampleID, ISNULL(IMG.ImagePath,'') ImagePath
                        From {DbNames.EPYSL}..SampleBookingMaster a
                        Left Join IMG ON IMG.BookingID = a.BookingID
                        INNER JOIN {DbNames.EPYSL}..SampleType ST ON ST.SampleTypeID = a.SampleID AND ST.SampleTypeName <> 'Projection Booking'
                        Where a.Proposed = 1 AND a.IsCancel=0 And a.BookingDate >= '04/11/2022' And a.ExportOrderID = 0 AND ((a.SwatchAttached=1 AND a.SwatchReceive=1) or (a.SwatchAttached=0 AND a.SwatchReceive=0))
                    ),"
                        :
                        $@"With BIMG As(
	                    Select BookingID, Min(ChildImgID) ChildImgID
	                    From {DbNames.EPYSL}..SampleBookingChildImage Group By BookingID
                    ), IMG As(
	                    Select I.BookingID, I.ImagePath
	                    From BIMG
	                    Inner Join {DbNames.EPYSL}..SampleBookingChildImage I On I.ChildImgID = BIMG.ChildImgID
                    ), BT As (
                    Select CategoryTeamID
                    From {DbNames.EPYSL}..EmployeeAssignContactTeam
                    Where EmployeeCode = {AppUser.EmployeeCode} AND IsActive = 1
                    Group By CategoryTeamID
                    ), B As (
                    Select C.ContactID
                    From {DbNames.EPYSL}..ContactAssignTeam C
                    Inner Join BT On BT.CategoryTeamID = C.CategoryTeamID
                    Group By C.ContactID
                    ), SBM AS (
                        Select A.BookingID, A.BookingNo, A.BookingDate, A.RevisionNo, A.SLNo, A.StyleNo, A.BuyerID, A.BuyerTeamID, A.SupplierID, A.ExecutionCompanyID, A.SeasonID, A.Remarks, A.OrderQty, A.AddedBy, A.SampleID, ISNULL(IMG.ImagePath,'') ImagePath
                        From {DbNames.EPYSL}..SampleBookingMaster A
                        INNER JOIN B ON B.ContactID = A.BuyerID
                        Left Join IMG ON IMG.BookingID = a.BookingID
                        INNER JOIN {DbNames.EPYSL}..SampleType ST ON ST.SampleTypeID = A.SampleID AND ST.SampleTypeName <> 'Projection Booking'
                        Where A.Proposed=1 AND a.IsCancel=0 And A.BookingDate >= '04/11/2022' And A.ExportOrderID = 0 AND ((A.SwatchAttached=1 AND A.SwatchReceive=1) or (A.SwatchAttached=0 AND A.SwatchReceive=0))
                    ),";
                }
                else
                {
                    sql = AppUser.IsSuperUser ?
                           $@"WITH BIMG As(
	                    Select BookingID, Min(ChildImgID) ChildImgID
	                    From {DbNames.EPYSL}..SampleBookingChildImage Group By BookingID
                    ), IMG As(
	                    Select I.BookingID, I.ImagePath
	                    From BIMG
	                    Inner Join {DbNames.EPYSL}..SampleBookingChildImage I On I.ChildImgID = BIMG.ChildImgID
                    ), SBM AS
                    (
                        Select a.BookingID, a.BookingNo, a.BookingDate, a.RevisionNo, a.SLNo, a.StyleNo, a.BuyerID, a.BuyerTeamID, a.SupplierID,
                        a.ExecutionCompanyID, a.SeasonID, a.Remarks, a.OrderQty, a.AddedBy, a.SampleID, ISNULL(IMG.ImagePath,'') ImagePath
                        From {DbNames.EPYSL}..SampleBookingMaster a
                        Left Join IMG ON IMG.BookingID = a.BookingID
                        INNER JOIN {DbNames.EPYSL}..SampleType ST ON ST.SampleTypeID = a.SampleID AND ST.SampleTypeName = 'Projection Booking'
                        Where a.Proposed = 1 AND a.IsCancel=0 And a.BookingDate >= '04/11/2022' And a.ExportOrderID = 0
                    ),"
                           :
                           $@"With BIMG As(
	                    Select BookingID, Min(ChildImgID) ChildImgID
	                    From {DbNames.EPYSL}..SampleBookingChildImage Group By BookingID
                    ), IMG As(
	                    Select I.BookingID, I.ImagePath
	                    From BIMG
	                    Inner Join {DbNames.EPYSL}..SampleBookingChildImage I On I.ChildImgID = BIMG.ChildImgID
                    ), BT As (
                    Select CategoryTeamID
                    From {DbNames.EPYSL}..EmployeeAssignContactTeam
                    Where EmployeeCode = {AppUser.EmployeeCode} AND IsActive = 1
                    Group By CategoryTeamID
                    ), B As (
                    Select C.ContactID
                    From {DbNames.EPYSL}..ContactAssignTeam C
                    Inner Join BT On BT.CategoryTeamID = C.CategoryTeamID
                    Group By C.ContactID
                    ), SBM AS (
                        Select A.BookingID, A.BookingNo, A.BookingDate, A.RevisionNo, A.SLNo, A.StyleNo, A.BuyerID, A.BuyerTeamID, A.SupplierID, A.ExecutionCompanyID, A.SeasonID, A.Remarks, A.OrderQty, A.AddedBy, A.SampleID, ISNULL(IMG.ImagePath,'') ImagePath
                        From {DbNames.EPYSL}..SampleBookingMaster A
                        INNER JOIN B ON B.ContactID = A.BuyerID
                        Left Join IMG ON IMG.BookingID = a.BookingID
                        INNER JOIN {DbNames.EPYSL}..SampleType ST ON ST.SampleTypeID = A.SampleID AND ST.SampleTypeName = 'Projection Booking'
                        Where A.Proposed=1 AND a.IsCancel=0 And A.BookingDate >= '04/11/2022' And A.ExportOrderID = 0
                    ),";
                }
                sql += $@"
                M AS (
	                 Select SBM.BookingID, SBM.BookingNo, SBM.BookingDate, SBM.SLNo, SBM.StyleNo, SBM.OrderQty, SBM.BuyerID, SBM.BuyerTeamID
		                ,SBM.SupplierID,SBM.ExecutionCompanyID,FBC.ItemMasterID,FBC.ConsumptionID,FBC.SubGroupID,SBM.Remarks,SBM.SeasonID, SBM.AddedBy BookingBy
                        ,PendingRevision= (CASE WHEN FBA.PreRevisionNo <> SBM.RevisionNo THEN 'Booking Revision No ' + CONVERT(VARCHAR(10),SBM.RevisionNo) ELSE '' END), SBM.ImagePath
	                From SBM
	                Inner Join {DbNames.EPYSL}..SampleType ST On SBM.SampleID = ST.SampleTypeID
					LEFT JOIN FBookingAcknowledgeChild FBC ON FBC.BookingID = SBM.BookingID
					LEFT JOIN FBookingAcknowledge FBA On FBA.BookingID = SBM.BookingID
                    LEFT JOIN FreeConceptMaster FCM ON FCM.BookingID = FBA.BookingID
                   -- Where ST.DisplayCode <> 'LR' AND FBA.BookingID IS NULL
                    Where ST.DisplayCode <> 'LR'  AND (FBA.BookingID IS NULL or FBA.PreRevisionNo <> SBM.RevisionNo)
                )
                /*, MF AS (
                    SELECT M.BookingID, M.BookingNo, M.BookingDate, M.SLNo, M.StyleNo, M.OrderQty, M.BuyerID, M.BuyerTeamID
		                , M.SupplierID,M.ExecutionCompanyID
                    FROM M
	                LEFT JOIN FBookingAcknowledgeChild FBC ON FBC.BookingID = M.BookingID
                    WHERE FBC.BookingID IS NULL
                )*/
                , F AS (
	                SELECT FBA.FBAckID, M.BookingID,M.BookingNo, M.BookingDate, M.SLNo, M.StyleNo, M.OrderQty BookingQty, M.BuyerID, M.BuyerTeamID, M.SupplierID
		                , M.ExecutionCompanyID, CTO.ShortName BuyerName, CCT.TeamName BuyerTeamName,C.CompanyName,M.Remarks, Supplier.ShortName [SupplierName],Season.SeasonName, M.BookingBy
                        ,M.PendingRevision, M.ImagePath, ISNULL(Supplier.MappingCompanyID,0) TextileCompanyID
	                FROM M
	                LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = M.BuyerID
	                LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = M.BuyerTeamID
	                LEFT JOIN {DbNames.EPYSL}..CompanyEntity C ON C.CompanyID = M.ExecutionCompanyID
                    LEFT JOIN FBookingAcknowledge FBA ON FBA.BookingID=M.BookingID AND FBA.IsUnAcknowledge=1
                    Inner Join {DbNames.EPYSL}..Contacts Supplier On M.SupplierID = Supplier.ContactID
                    LEFT Join {DbNames.EPYSL}..ContactSeason Season On M.SeasonID = Season.SeasonID
					Group By M.BookingID,M.BookingNo, M.BookingDate, M.SLNo, M.StyleNo, M.OrderQty, M.BuyerID, M.BuyerTeamID, M.SupplierID
		                , M.ExecutionCompanyID, CTO.ShortName , CCT.TeamName,C.CompanyName,M.Remarks, Supplier.ShortName,Season.SeasonName, M.BookingBy
                        ,M.PendingRevision,FBA.FBAckID, M.ImagePath, Supplier.MappingCompanyID
                )

                SELECT *,Count(*) Over() TotalRows FROM F ";
            }
            else if (status == Status.Reject)
            {
                sql = AppUser.IsSuperUser ?
                    $@"WITH BIMG As(
	                    Select BookingID, Min(ChildImgID) ChildImgID
	                    From {DbNames.EPYSL}..SampleBookingChildImage Group By BookingID
                    ), IMG As(
	                    Select I.BookingID, I.ImagePath
	                    From BIMG
	                    Inner Join {DbNames.EPYSL}..SampleBookingChildImage I On I.ChildImgID = BIMG.ChildImgID
                    ), SBM AS
                    (
                        Select a.BookingID, a.BookingNo, a.BookingDate, a.RevisionNo, a.SLNo, a.StyleNo, a.BuyerID, a.BuyerTeamID, a.SupplierID,
                        a.ExecutionCompanyID, a.SeasonID, a.Remarks, a.OrderQty, a.AddedBy, a.SampleID, ISNULL(IMG.ImagePath,'') ImagePath
                        From {DbNames.EPYSL}..SampleBookingMaster a
                        Left Join IMG ON IMG.BookingID = a.BookingID
                        Where a.Proposed = 1 And a.BookingDate >= '04/11/2022' And a.ExportOrderID = 0
                        AND ((a.SwatchAttached=1 AND a.SwatchReceive=1) or (a.SwatchAttached=0 AND a.SwatchReceive=0))
                        AND a.IsCancel = 1 AND a.Acknowledge = 0
                    ),"
                    :
                    $@"With BIMG As(
	                    Select BookingID, Min(ChildImgID) ChildImgID
	                    From {DbNames.EPYSL}..SampleBookingChildImage Group By BookingID
                    ), IMG As(
	                    Select I.BookingID, I.ImagePath
	                    From BIMG
	                    Inner Join {DbNames.EPYSL}..SampleBookingChildImage I On I.ChildImgID = BIMG.ChildImgID
                    ), BT As (
                    Select CategoryTeamID
                    From {DbNames.EPYSL}..EmployeeAssignContactTeam
                    Where EmployeeCode = {AppUser.EmployeeCode} AND IsActive = 1
                    Group By CategoryTeamID
                    ), B As (
                    Select C.ContactID
                    From {DbNames.EPYSL}..ContactAssignTeam C
                    Inner Join BT On BT.CategoryTeamID = C.CategoryTeamID
                    Group By C.ContactID
                    ), 
                    {queryProjConcept}
                    SBM AS (
                        Select A.BookingID, A.BookingNo, A.BookingDate, A.RevisionNo, A.SLNo, A.StyleNo, A.BuyerID, A.BuyerTeamID, A.SupplierID, A.ExecutionCompanyID, A.SeasonID, A.Remarks, A.OrderQty, A.AddedBy, A.SampleID, ISNULL(IMG.ImagePath,'') ImagePath
                        From {DbNames.EPYSL}..SampleBookingMaster A
                        INNER JOIN B ON B.ContactID = A.BuyerID
                        Left Join IMG ON IMG.BookingID = a.BookingID
                        Where A.Proposed=1 And A.BookingDate >= '04/11/2022' And A.ExportOrderID = 0
                        AND ((A.SwatchAttached=1 AND A.SwatchReceive=1) or (A.SwatchAttached=0 AND A.SwatchReceive=0))
                        AND IsCancel = 1 AND Acknowledge = 0
                    ),";

                sql += $@"
                M AS (
	                 Select SBM.BookingID, SBM.BookingNo, SBM.BookingDate, SBM.SLNo, SBM.StyleNo, SBM.OrderQty, SBM.BuyerID, SBM.BuyerTeamID
		                ,SBM.SupplierID,SBM.ExecutionCompanyID,FBC.ItemMasterID,FBC.ConsumptionID,FBC.SubGroupID,SBM.Remarks,SBM.SeasonID, SBM.AddedBy BookingBy
                        ,PendingRevision= (CASE WHEN FBA.PreRevisionNo <> SBM.RevisionNo THEN 'Booking Revision No ' + CONVERT(VARCHAR(10),SBM.RevisionNo) ELSE '' END), SBM.ImagePath
	                From SBM
	                Inner Join {DbNames.EPYSL}..SampleType ST On SBM.SampleID = ST.SampleTypeID
					LEFT JOIN FBookingAcknowledgeChild FBC ON FBC.BookingID = SBM.BookingID
					LEFT JOIN FBookingAcknowledge FBA On FBA.BookingID = SBM.BookingID
                    LEFT JOIN FreeConceptMaster FCM ON FCM.BookingID = FBA.BookingID
                   -- Where ST.DisplayCode <> 'LR' AND FBA.BookingID IS NULL
                    Where ST.DisplayCode <> 'LR' AND (FBA.BookingID IS NULL or FBA.PreRevisionNo <> SBM.RevisionNo)
                    {queryIsProjectionAnd}
                )
                /*, MF AS (
                    SELECT M.BookingID, M.BookingNo, M.BookingDate, M.SLNo, M.StyleNo, M.OrderQty, M.BuyerID, M.BuyerTeamID
		                , M.SupplierID,M.ExecutionCompanyID
                    FROM M
	                LEFT JOIN FBookingAcknowledgeChild FBC ON FBC.BookingID = M.BookingID
                    WHERE FBC.BookingID IS NULL
                )*/
                , F AS (
	                SELECT FBA.FBAckID, M.BookingID,M.BookingNo, M.BookingDate, M.SLNo, M.StyleNo, M.OrderQty BookingQty, M.BuyerID, M.BuyerTeamID, M.SupplierID
		                , M.ExecutionCompanyID, CTO.ShortName BuyerName, CCT.TeamName BuyerTeamName,C.CompanyName,M.Remarks, Supplier.ShortName [SupplierName],Season.SeasonName, M.BookingBy
                        ,M.PendingRevision, M.ImagePath, ISNULL(Supplier.MappingCompanyID,0) TextileCompanyID
	                FROM M
	                LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = M.BuyerID
	                LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = M.BuyerTeamID
	                LEFT JOIN {DbNames.EPYSL}..CompanyEntity C ON C.CompanyID = M.ExecutionCompanyID
                    LEFT JOIN FBookingAcknowledge FBA ON FBA.BookingID=M.BookingID AND FBA.IsUnAcknowledge=1
                    LEFT JOIN FreeConceptMaster FCM ON FCM.BookingID = FBA.BookingID
                    Inner Join {DbNames.EPYSL}..Contacts Supplier On M.SupplierID = Supplier.ContactID
                    LEFT Join {DbNames.EPYSL}..ContactSeason Season On M.SeasonID = Season.SeasonID
                    {queryIsProjectionWhere}
					Group By M.BookingID,M.BookingNo, M.BookingDate, M.SLNo, M.StyleNo, M.OrderQty, M.BuyerID, M.BuyerTeamID, M.SupplierID
		                , M.ExecutionCompanyID, CTO.ShortName , CCT.TeamName,C.CompanyName,M.Remarks, Supplier.ShortName,Season.SeasonName, M.BookingBy
                        ,M.PendingRevision,FBA.FBAckID, M.ImagePath, Supplier.MappingCompanyID
                )

                SELECT *,Count(*) Over() TotalRows FROM F ";
            }
            else if (status == Status.Active)
            {
                sql = AppUser.IsSuperUser ?
                    $@"WITH BIMG As(
	                    Select BookingID, Min(ChildImgID) ChildImgID
	                    From {DbNames.EPYSL}..SampleBookingChildImage Group By BookingID
                    ), IMG As(
	                    Select I.BookingID, I.ImagePath
	                    From BIMG
	                    Inner Join {DbNames.EPYSL}..SampleBookingChildImage I On I.ChildImgID = BIMG.ChildImgID
                    ), SBM AS (
                        Select a.BookingID, a.BookingNo, a.BookingDate, a.SLNo, a.StyleNo, a.BuyerID, a.BuyerTeamID, a.SupplierID, a.ExecutionCompanyID, a.SeasonID, a.Remarks, a.OrderQty, a.AddedBy, a.SampleID, ISNULL(IMG.ImagePath,'') ImagePath
	                    ,a.SwatchReceive
                        From {DbNames.EPYSL}..SampleBookingMaster a
                        Left Join IMG ON IMG.BookingID = a.BookingID
	                    Where a.Proposed=1 And a.BookingDate >= '04/11/2022' And a.ExportOrderID = 0 AND a.SwatchAttached=1 AND a.SwatchReceive=0
                    ),"
                    :
                    $@"With BIMG As(
	                    Select BookingID, Min(ChildImgID) ChildImgID
	                    From {DbNames.EPYSL}..SampleBookingChildImage Group By BookingID
                    ), IMG As(
	                    Select I.BookingID, I.ImagePath
	                    From BIMG
	                    Inner Join {DbNames.EPYSL}..SampleBookingChildImage I On I.ChildImgID = BIMG.ChildImgID
                    ), BT As (
                    Select CategoryTeamID
                    From {DbNames.EPYSL}..EmployeeAssignContactTeam
                    Where EmployeeCode = {AppUser.EmployeeCode} AND IsActive = 1
                    Group By CategoryTeamID
                    ), B As (
                    Select C.ContactID
                    From {DbNames.EPYSL}..ContactAssignTeam C
                    Inner Join BT On BT.CategoryTeamID = C.CategoryTeamID
                    Group By C.ContactID
                    ), SBM AS (
                        Select a.BookingID, a.BookingNo, a.BookingDate, a.SLNo, a.StyleNo, a.BuyerID, a.BuyerTeamID, a.SupplierID, a.ExecutionCompanyID, a.SeasonID, a.Remarks, a.OrderQty, a.AddedBy, a.SampleID, ISNULL(IMG.ImagePath,'') ImagePath
                        ,a.SwatchReceive	                    
                        From {DbNames.EPYSL}..SampleBookingMaster a
                        INNER JOIN B ON B.ContactID = a.BuyerID
                        Left Join IMG ON IMG.BookingID = a.BookingID
	                    Where a.Proposed=1 And a.BookingDate >= '04/11/2022' And a.ExportOrderID = 0 AND a.SwatchAttached=1 AND a.SwatchReceive=0
                    ),";

                sql += $@"
                M AS (
	                 Select SBM.BookingID, SBM.BookingNo, SBM.BookingDate, SBM.SLNo, SBM.StyleNo, SBM.OrderQty, SBM.BuyerID, SBM.BuyerTeamID
		                , SBM.SupplierID,SBM.ExecutionCompanyID,FBC.ItemMasterID,FBC.ConsumptionID,FBC.SubGroupID,SBM.Remarks,SBM.SeasonID, SBM.AddedBy BookingBy, SBM.ImagePath
	                From SBM
	                Inner Join {DbNames.EPYSL}..SampleType ST On SBM.SampleID = ST.SampleTypeID
					LEFT JOIN FBookingAcknowledgeChild FBC ON FBC.BookingID = SBM.BookingID
					LEFT JOIN FBookingAcknowledge FBA On FBA.BookingID = SBM.BookingID
                    LEFT JOIN FreeConceptMaster FCM ON FCM.BookingID = FBA.BookingID
                    Where ST.DisplayCode <> 'LR' AND (FBA.BookingID IS NULL OR SBM.SwatchReceive = 0)
                )
                , MF AS (
                    SELECT M.BookingID, M.BookingNo, M.BookingDate, M.SLNo, M.StyleNo, M.OrderQty, M.BuyerID, M.BuyerTeamID
		                , M.SupplierID,M.ExecutionCompanyID
                    FROM M
	                LEFT JOIN FBookingAcknowledgeChild FBC ON FBC.BookingID = M.BookingID
                    WHERE FBC.BookingID IS NULL
                )
                , F AS (
	                SELECT M.BookingID,M.ItemMasterID,M.ConsumptionID,M.SubGroupID, M.BookingNo, M.BookingDate, M.SLNo, M.StyleNo, M.OrderQty BookingQty, M.BuyerID, M.BuyerTeamID, M.SupplierID
		                , M.ExecutionCompanyID, CTO.ShortName BuyerName, CCT.TeamName BuyerTeamName,C.CompanyName,M.Remarks, Supplier.ShortName [SupplierName],Season.SeasonName, M.BookingBy, M.ImagePath, ISNULL(Supplier.MappingCompanyID,0) TextileCompanyID
	                FROM M
	                LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = M.BuyerID
	                LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = M.BuyerTeamID
	                LEFT JOIN {DbNames.EPYSL}..CompanyEntity C ON C.CompanyID = M.ExecutionCompanyID
                    Inner Join {DbNames.EPYSL}..Contacts Supplier On M.SupplierID = Supplier.ContactID
                    LEFT Join {DbNames.EPYSL}..ContactSeason Season On M.SeasonID = Season.SeasonID
                )

                SELECT *,Count(*) Over() TotalRows FROM F";
            }
            else if (status == Status.UnAcknowledge)
            {
                sql = AppUser.IsSuperUser ?
                    $@"WITH BIMG As(
	                    Select BookingID, Min(ChildImgID) ChildImgID
	                    From {DbNames.EPYSL}..SampleBookingChildImage Group By BookingID
                    ), IMG As(
	                    Select I.BookingID, I.ImagePath
	                    From BIMG
	                    Inner Join {DbNames.EPYSL}..SampleBookingChildImage I On I.ChildImgID = BIMG.ChildImgID
                    ), 
                    {queryProjConcept}
                    SBM AS (
                        SELECT FBC.FBAckID,	FBC.BookingID,FBC.BookingNo,FBC.BookingDate,FBC.SLNo,
                        FBC.StyleNo,FBC.BookingQty,FBC.BuyerID,FBC.BuyerTeamID,FBC.SupplierID,FBC.ExecutionCompanyID, 
                        FBC.UnAcknowledgeDate, BM.AddedBy BookingBy,
                        LU.Name UnAckByName,
                        ISNULL(IMG.ImagePath,'') ImagePath
                        FROM FBookingAcknowledge FBC
                        {queryConceptJoin}
                        Inner Join {DbNames.EPYSL}..SampleBookingMaster BM On BM.BookingID = FBC.BookingID
                        LEFT JOIN {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = FBC.AddedBy
                        Left Join IMG ON IMG.BookingID = BM.BookingID
                        Where FBC.IsUnAcknowledge=1
                    ),"
                    :
                    $@"With BIMG As(
	                    Select BookingID, Min(ChildImgID) ChildImgID
	                    From {DbNames.EPYSL}..SampleBookingChildImage Group By BookingID
                    ), IMG As(
	                    Select I.BookingID, I.ImagePath
	                    From BIMG
	                    Inner Join {DbNames.EPYSL}..SampleBookingChildImage I On I.ChildImgID = BIMG.ChildImgID
                    ), BT As (
                        Select CategoryTeamID
                        From {DbNames.EPYSL}..EmployeeAssignContactTeam
                        Where EmployeeCode = {AppUser.EmployeeCode} AND IsActive = 1
                        Group By CategoryTeamID
                        ), B As (
                        Select C.ContactID
                        From {DbNames.EPYSL}..ContactAssignTeam C
                        Inner Join BT On BT.CategoryTeamID = C.CategoryTeamID
                        Group By C.ContactID
                        ), 
                        {queryProjConcept}
                        SBM AS (
                        SELECT FBC.FBAckID,	FBC.BookingID,FBC.BookingNo,FBC.BookingDate,FBC.SLNo,
                        FBC.StyleNo,FBC.BookingQty,FBC.BuyerID,FBC.BuyerTeamID,FBC.SupplierID,FBC.ExecutionCompanyID, 
                        FBC.UnAcknowledgeDate, BM.AddedBy BookingBy,
                        LU.Name UnAckByName,
                        ISNULL(IMG.ImagePath,'') ImagePath
                        FROM FBookingAcknowledge FBC
                        {queryConceptJoin}
                        Inner Join {DbNames.EPYSL}..SampleBookingMaster BM On BM.BookingID = FBC.BookingID
                        LEFT JOIN {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = FBC.AddedBy
                        Left Join IMG ON IMG.BookingID = BM.BookingID
                        INNER JOIN B ON B.ContactID = FBC.BuyerID
                        Where FBC.IsUnAcknowledge=1
                    ),";

                sql += $@"
                F AS (
                SELECT SBM.FBAckID, SBM.BookingID,SBM.BookingNo,SBM.BookingDate,SBM.SLNo,SBM.StyleNo,SBM.BookingQty,SBM.BuyerID,SBM.BuyerTeamID,SBM.SupplierID,SBM.ExecutionCompanyID, SBM.UnAcknowledgeDate,
                SBM.UnAckByName, CTO.ShortName BuyerName, CCT.TeamName BuyerTeamName,C.CompanyName, SBM.BookingBy, SBM.ImagePath, ISNULL(Supplier.MappingCompanyID,0) TextileCompanyID
                FROM SBM
                LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = SBM.BuyerID
				LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = SBM.BuyerTeamID
				LEFT JOIN {DbNames.EPYSL}..CompanyEntity C ON C.CompanyID = SBM.ExecutionCompanyID
                Inner Join {DbNames.EPYSL}..Contacts Supplier On SBM.SupplierID = Supplier.ContactID
                )
				SELECT *,Count(*) Over() TotalRows FROM F";
            }
            else if (status == Status.Approved)
            {
                sql = AppUser.IsSuperUser ?
                  $@"WITH BIMG As(
	                    Select BookingID, Min(ChildImgID) ChildImgID
	                    From {DbNames.EPYSL}..SampleBookingChildImage Group By BookingID
                    ), IMG As(
	                    Select I.BookingID, I.ImagePath
	                    From BIMG
	                    Inner Join {DbNames.EPYSL}..SampleBookingChildImage I On I.ChildImgID = BIMG.ChildImgID
                    ),
                    {queryProjConcept}
                    SBM AS (
                        SELECT FBC.FBAckID,	FBC.BookingID,FBC.BookingNo,FBC.BookingDate,FBC.SLNo,FBC.StyleNo,FBC.BookingQty,FBC.BuyerID,FBC.BuyerTeamID,FBC.SupplierID,FBC.ExecutionCompanyID, SBM.AddedBy BookingBy, ISNULL(IMG.ImagePath,'') ImagePath
                        FROM FBookingAcknowledge FBC
                        {queryConceptJoin}
                        Inner Join {DbNames.EPYSL}..SampleBookingMaster SBM On SBM.BookingID = FBC.BookingID
                        Left Join IMG ON IMG.BookingID = SBM.BookingID
                        where FBC.IsUnAcknowledge !='1'
                    ),"
                  :
                  $@"With BIMG As(
	                    Select BookingID, Min(ChildImgID) ChildImgID
	                    From {DbNames.EPYSL}..SampleBookingChildImage Group By BookingID
                    ), IMG As(
	                    Select I.BookingID, I.ImagePath
	                    From BIMG
	                    Inner Join {DbNames.EPYSL}..SampleBookingChildImage I On I.ChildImgID = BIMG.ChildImgID
                    ), BT As (
                        Select CategoryTeamID
                        From {DbNames.EPYSL}..EmployeeAssignContactTeam
                        Where EmployeeCode = {AppUser.EmployeeCode} AND IsActive = 1
                        Group By CategoryTeamID
                        ), B As (
                        Select C.ContactID
                        From {DbNames.EPYSL}..ContactAssignTeam C
                        Inner Join BT On BT.CategoryTeamID = C.CategoryTeamID
                        Group By C.ContactID
                        ), 
                        {queryProjConcept}
                        SBM AS (
                        SELECT FBC.FBAckID,	FBC.BookingID,FBC.BookingNo,FBC.BookingDate,FBC.SLNo,FBC.StyleNo,FBC.BookingQty,FBC.BuyerID,FBC.BuyerTeamID,FBC.SupplierID,FBC.ExecutionCompanyID, SBM.AddedBy BookingBy, ISNULL(IMG.ImagePath,'') ImagePath
                        FROM FBookingAcknowledge FBC
                        {queryConceptJoin}
                        Inner Join {DbNames.EPYSL}..SampleBookingMaster SBM On SBM.BookingID = FBC.BookingID
                        Left Join IMG ON IMG.BookingID = SBM.BookingID
                        INNER JOIN B ON B.ContactID = FBC.BuyerID
                        where FBC.IsUnAcknowledge != '1'
                    ),";

                sql += $@"
                F AS (
                SELECT SBM.FBAckID, SBM.BookingID,SBM.BookingNo,SBM.BookingDate,SBM.SLNo,SBM.StyleNo,SBM.BookingQty,SBM.BuyerID,SBM.BuyerTeamID,SBM.SupplierID,SBM.ExecutionCompanyID,
                    CTO.ShortName BuyerName, CCT.TeamName BuyerTeamName,C.CompanyName, SBM.BookingBy, SBM.ImagePath, ISNULL(Supplier.MappingCompanyID,0) TextileCompanyID
                FROM SBM
                LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = SBM.BuyerID
				LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = SBM.BuyerTeamID
				LEFT JOIN {DbNames.EPYSL}..CompanyEntity C ON C.CompanyID = SBM.ExecutionCompanyID
                Inner Join {DbNames.EPYSL}..Contacts Supplier On SBM.SupplierID = Supplier.ContactID
                )
				SELECT *,Count(*) Over() TotalRows FROM F";
            }
            else
            {
                orderBy = "";
                string buyerTeamConQuery = "";
                string buyerTeamJoin = "";
                if (!AppUser.IsSuperUser)
                {
                    buyerTeamConQuery = $@" B As (
                            Select C.ContactID
                            From {DbNames.EPYSL}..EmployeeAssignContactTeam A
                            Inner Join {DbNames.EPYSL}..ContactAssignTeam C On C.CategoryTeamID = A.CategoryTeamID
                            Where A.EmployeeCode = {AppUser.EmployeeCode} AND A.IsActive = 1
                            Group By C.ContactID
                        ), ";
                    buyerTeamJoin = $@"  INNER JOIN B ON B.ContactID = FBC.BuyerID ";
                }

                sql = $@"With BIMG As(
                            Select BookingID, Min(ChildImgID) ChildImgID
                            From {DbNames.EPYSL}..SampleBookingChildImage
                            Group By BookingID
                        ), IMG As(
                            Select I.BookingID, I.ImagePath
                            From BIMG
                            Inner Join {DbNames.EPYSL}..SampleBookingChildImage I On I.ChildImgID = BIMG.ChildImgID
                        ),
                        {buyerTeamConQuery}
                        {queryProjConcept}
                        F AS (
                            SELECT FBC.FBAckID, FBC.BookingID, FBC.BookingNo, FBC.BookingDate, FBC.SLNo, FBC.StyleNo, FBC.BookingQty, FBC.BuyerID, FBC.BuyerTeamID,
                            FBC.SupplierID, FBC.ExecutionCompanyID, FBC.DateAdded, SBM.AddedBy BookingBy, LU.Name AckByName,
                            CTO.ShortName BuyerName, CCT.TeamName BuyerTeamName,C.CompanyName, ISNULL(IMG.ImagePath,'') ImagePath, ISNULL(Supplier.MappingCompanyID,0) TextileCompanyID
                            FROM FBookingAcknowledge FBC
                            Inner Join {DbNames.EPYSL}..SampleBookingMaster SBM On SBM.BookingID = FBC.BookingID
                            {buyerTeamJoin}
                            {queryConceptJoin}
                            LEFT JOIN {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = FBC.AddedBy
                            Left Join IMG ON IMG.BookingID = SBM.BookingID
                            LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = FBC.BuyerID
                            LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = FBC.BuyerTeamID
                            LEFT JOIN {DbNames.EPYSL}..CompanyEntity C ON C.CompanyID = FBC.ExecutionCompanyID
                            Inner Join {DbNames.EPYSL}..Contacts Supplier On Supplier.ContactID = FBC.SupplierID
                            where SBM.ExportOrderID = 0 And FBC.IsUnAcknowledge != 1 AND SBM.RevisionNo = FBC.PreRevisionNo
                        ),
                        R AS (
							SELECT R_No_New = Row_Number() Over(Order by BookingID Desc), *,  Count(*) Over() TotalRows 
							FROM F {paginationInfo.FilterBy} --WHERE BookingNo LIKE '%PPS-2302446-EFL%'
						)
                        SELECT * FROM R WHERE {paginationInfo.PageByNew}";
            }
            /*
            else
            {
                sql = AppUser.IsSuperUser ?
                    $@"WITH BIMG As(
	                    Select BookingID, Min(ChildImgID) ChildImgID
	                    From {DbNames.EPYSL}..SampleBookingChildImage Group By BookingID
                    ), IMG As(
	                    Select I.BookingID, I.ImagePath
	                    From BIMG
	                    Inner Join {DbNames.EPYSL}..SampleBookingChildImage I On I.ChildImgID = BIMG.ChildImgID
                    ), 
                    {queryProjConcept}
                    SBM AS (
                        SELECT FBC.FBAckID,	FBC.BookingID,FBC.BookingNo,FBC.BookingDate,
                        FBC.SLNo,FBC.StyleNo,FBC.BookingQty,FBC.BuyerID,FBC.BuyerTeamID,
                        FBC.SupplierID,FBC.ExecutionCompanyID, FBC.DateAdded, SBM.AddedBy BookingBy,
                        LU.Name AckByName,
                        ISNULL(IMG.ImagePath,'') ImagePath
                        FROM FBookingAcknowledge FBC
                        {queryConceptJoin}
                        Inner Join {DbNames.EPYSL}..SampleBookingMaster SBM On SBM.BookingID = FBC.BookingID
                        LEFT JOIN {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = FBC.AddedBy
                        Left Join IMG ON IMG.BookingID = SBM.BookingID
                        where FBC.IsUnAcknowledge !='1' AND SBM.RevisionNo = FBC.PreRevisionNo
                    ),"
                    :
                    $@"With BIMG As(
	                    Select BookingID, Min(ChildImgID) ChildImgID
	                    From {DbNames.EPYSL}..SampleBookingChildImage Group By BookingID
                    ), IMG As(
	                    Select I.BookingID, I.ImagePath
	                    From BIMG
	                    Inner Join {DbNames.EPYSL}..SampleBookingChildImage I On I.ChildImgID = BIMG.ChildImgID
                    ), BT As (
                        Select CategoryTeamID
                        From {DbNames.EPYSL}..EmployeeAssignContactTeam
                        Where EmployeeCode = {AppUser.EmployeeCode} AND IsActive = 1
                        Group By CategoryTeamID
                    ), B As (
                        Select C.ContactID
                        From {DbNames.EPYSL}..ContactAssignTeam C
                        Inner Join BT On BT.CategoryTeamID = C.CategoryTeamID
                        Group By C.ContactID
                    ), 
                    {queryProjConcept}
                    SBM AS (
                        SELECT FBC.FBAckID,	FBC.BookingID,FBC.BookingNo,FBC.BookingDate,
                        FBC.SLNo,FBC.StyleNo,FBC.BookingQty,FBC.BuyerID,FBC.BuyerTeamID,
                        FBC.SupplierID,FBC.ExecutionCompanyID, FBC.DateAdded, SBM.AddedBy BookingBy,
                        LU.Name AckByName,
                        ISNULL(IMG.ImagePath,'') ImagePath
                        FROM FBookingAcknowledge FBC
                        {queryConceptJoin}
                        Inner Join {DbNames.EPYSL}..SampleBookingMaster SBM On SBM.BookingID = FBC.BookingID
                        INNER JOIN B ON B.ContactID = FBC.BuyerID
                        LEFT JOIN {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = FBC.AddedBy
                        Left Join IMG ON IMG.BookingID = SBM.BookingID
                        where FBC.IsUnAcknowledge != '1' AND SBM.RevisionNo = FBC.PreRevisionNo
                    ),";

                sql += $@"
                F AS (
                SELECT SBM.FBAckID, SBM.BookingID,SBM.BookingNo,SBM.BookingDate,SBM.SLNo,SBM.StyleNo,SBM.BookingQty,SBM.BuyerID,SBM.BuyerTeamID,SBM.SupplierID,SBM.ExecutionCompanyID, SBM.DateAdded,
                       SBM.AckByName, CTO.ShortName BuyerName, CCT.TeamName BuyerTeamName,C.CompanyName, SBM.BookingBy, SBM.ImagePath, ISNULL(Supplier.MappingCompanyID,0) TextileCompanyID
                FROM SBM
                LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = SBM.BuyerID
				LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = SBM.BuyerTeamID
				LEFT JOIN {DbNames.EPYSL}..CompanyEntity C ON C.CompanyID = SBM.ExecutionCompanyID
                Inner Join {DbNames.EPYSL}..Contacts Supplier On SBM.SupplierID = Supplier.ContactID
                )
				SELECT *,Count(*) Over() TotalRows FROM F";
            }
            */
            if (status != Status.Completed)
            {
                sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";
            }

            return await _service.GetDataAsync<FBookingAcknowledge>(sql);
        }

        public async Task<List<FBookingAcknowledge>> GetBulkPagedAsync(Status status, PaginationInfo paginationInfo, LoginUser AppUser)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By BookingNo Desc" : paginationInfo.OrderBy;
            string sql;
            if (status == Status.Active)
            {
                sql = $@"WITH FBA AS
                    (
	                    SELECT BookingNo = b.GroupConceptNo, BookingDate = MIN(a.BookingDate), 
	                    a.ExportOrderID,a.BuyerID,a.BuyerTeamID, b.CompanyID
                        FROM FBookingAcknowledge a
	                    INNER JOIN FreeConceptMaster b ON b.BookingID = a.BookingID
	                    WHERE b.IsBDS = 2 AND (b.TechnicalNameId > 0 AND b.MCSubClassID > 0)
	                    GROUP BY b.GroupConceptNo, a.ExportOrderID,a.BuyerID,a.BuyerTeamID, b.CompanyID
                    ),
                    F AS(
	                    SELECT FBA.*, CTO.ContactDisplayCode AS BuyerName, CCT.DisplayCode AS BuyerTeamName,CompanyName = C.ShortName, EOM.ExportOrderNo
	                    FROM FBA
	                    LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = FBA.BuyerID
	                    LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = FBA.BuyerTeamID
	                    LEFT JOIN {DbNames.EPYSL}..CompanyEntity C ON C.CompanyID = FBA.CompanyID
	                    INNER JOIN {DbNames.EPYSL}..ExportOrderMaster EOM ON EOM.ExportOrderID = FBA.ExportOrderID
                    )
                    SELECT *,Count(*) Over() TotalRows FROM F";
            }
            else //Pending
            {
                sql = $@"WITH FBA AS
                        (
	                        SELECT  BookingNo = b.GroupConceptNo, BookingDate = MIN(a.BookingDate), 
	                        a.ExportOrderID,a.BuyerID,a.BuyerTeamID, b.CompanyID, a.FBAckID
	                        FROM FBookingAcknowledge a
	                        INNER JOIN FreeConceptMaster b ON b.BookingID = a.BookingID
	                        WHERE b.IsBDS = 2 AND (b.TechnicalNameId = 0 OR b.MCSubClassID = 0)
	                        GROUP BY b.GroupConceptNo, a.ExportOrderID,a.BuyerID,a.BuyerTeamID, b.CompanyID, a.FBAckID
                        ),
                        F AS(
	                        SELECT FBAckID, FBA.BookingNo, FBA.BookingDate, CTO.ContactDisplayCode AS BuyerName, CCT.DisplayCode AS BuyerTeamName,CompanyName = C.ShortName, EOM.ExportOrderNo
	                        FROM FBA
	                        LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = FBA.BuyerID
	                        LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = FBA.BuyerTeamID
	                        LEFT JOIN {DbNames.EPYSL}..CompanyEntity C ON C.CompanyID = FBA.CompanyID
	                        INNER JOIN {DbNames.EPYSL}..ExportOrderMaster EOM ON EOM.ExportOrderID = FBA.ExportOrderID
                        ),
                        YBN AS 
                        (
	                        SELECT FBAckID, BookingNo = CASE WHEN YBM.WithoutOB = 1 THEN SBM.BookingNo ELSE BM.BookingNo END, 
	                        BookingDate = CASE WHEN YBM.WithoutOB = 1 THEN SBM.BookingDate ELSE BM.BookingDate END, 
	                        CTO.ContactDisplayCode AS BuyerName, CCT.DisplayCode AS BuyerTeamName,CompanyName = C.ShortName, EOM.ExportOrderNo
	                        FROM YarnBookingMaster_New YBM
	                        LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = YBM.BuyerID
	                        LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = YBM.BuyerTeamID
	                        LEFT JOIN {DbNames.EPYSL}..CompanyEntity C ON C.CompanyID = YBM.CompanyID
	                        LEFT JOIN FBookingAcknowledge FBA ON FBA.BookingID = YBM.BookingID
	                        LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster SBM ON SBM.BookingID = YBM.BookingID
	                        LEFT JOIN {DbNames.EPYSL}..BookingMaster BM ON BM.BookingID = YBM.BookingID
	                        INNER JOIN {DbNames.EPYSL}..ExportOrderMaster EOM ON EOM.ExportOrderID = YBM.ExportOrderID
	                        WHERE YBM.Acknowledge = 1
                        ),
                        FinalList AS
                        (
	                        SELECT * FROM F
	                        UNION
	                        SELECT * FROM YBN
                        )
                        SELECT *,Count(*) Over() TotalRows FROM FinalList";
            }

            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            var fBookingAcknowledge = await _service.GetDataAsync<FBookingAcknowledge>(sql);
            return fBookingAcknowledge;
        }

        public async Task<List<FBookingAcknowledge>> GetBulkFabricAckPagedAsync(Status status, PaginationInfo paginationInfo, LoginUser AppUser)
        {
            bool isNeedImage = false;
            string tempGuid = CommonFunction.GetNewGuid();

            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By BKAcknowledgeDate Desc, FBAcknowledgeDate Desc" : paginationInfo.OrderBy;
            if (status == Status.UnAcknowledge)
            {
                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By UnAcknowledgeDate desc,BKAcknowledgeDate Desc, FBAcknowledgeDate Desc" : paginationInfo.OrderBy;
            }
            else if (status == Status.Completed)
            {
                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By AcknowledgeDate desc,BKAcknowledgeDate Desc, FBAcknowledgeDate Desc" : paginationInfo.OrderBy;
            }
            string sql = "";
            string sts = status == Status.New ? "N" : status == Status.Revise ? "R" : status == Status.Completed ? "A" : "";

            switch (status)
            {
                case Status.New:
                    sql = $@"With RunningEWO As
                    (
	                    Select EOM.ExportOrderID, EOM.ExportOrderNo, EOM.StyleMasterID, EOM.BuyerID, EOM.BuyerTeamID
	                    From {DbNames.EPYSL}..ExportOrderMaster EOM
	                    Where EOM.EWOStatusID = 130
                    ),
                    BM As 
                    (
	                    Select BM.BookingID, BM.BookingNo, BM.ExportOrderID, BM.SupplierID, BM.SubGroupID, WithOutOB = Convert(bit,0),
	                    EOM.ExportOrderNo, EOM.StyleMasterID, BM.RevisionNo, EOM.BuyerID, EOM.BuyerTeamID,
                        AddedBy = Case When ISNULL(BM.UpdatedBy, 0) = 0 Then ISNULL(BM.AddedBy, 0) Else ISNULL(BM.UpdatedBy, 0) END
	                    from {DbNames.EPYSL}..BookingMaster BM
	                    Inner Join RunningEWO EOM On EOM.ExportOrderID = BM.ExportOrderID
                        Inner Join {DbNames.EPYSL}..BookingItemAcknowledge Ack on ACK.BookingID = BM.BookingID
						LEFT JOIN {DbNames.EPYSL}..BookingChildImage BCI ON BCI.BookingID = BM.BookingID
	                    where BM.IsCancel = 0 And BM.SubGroupID in (1,11,12) AND ACK.UnAcknowledge = 0 AND BM.Proposed = 1
                        AND BM.BookingDate >= '{_startingDate}'
                    
	                    Union All

	                    Select BM.BookingID, BM.BookingNo, BM.ExportOrderID, SupplierID, 1 SubGroupID, WithOutOB = Convert(bit,1),
	                    EOM.ExportOrderNo, EOM.StyleMasterID, BM.RevisionNo, EOM.BuyerID, EOM.BuyerTeamID, 
                        AddedBy = Case When ISNULL(BM.UpdatedBy, 0) = 0 Then ISNULL(BM.AddedBy, 0) Else ISNULL(BM.UpdatedBy, 0) END
	                    from {DbNames.EPYSL}..SampleBookingMaster BM
	                    Inner Join RunningEWO EOM On EOM.ExportOrderID = BM.ExportOrderID
						LEFT JOIN {DbNames.EPYSL}..SampleBookingChildImage BCI ON BCI.BookingID = BM.BookingID
	                    where BM.IsCancel = 0 And BM.ExportOrderID <> 0 and BM.Proposed = 1 And IsCancel = 0 AND BM.UnAcknowledge=0
                        AND BM.BookingDate >= '{_startingDate}'
                    ),
                    ISG As
                    (
	                    Select * 
	                    from {DbNames.EPYSL}..ItemSubGroup
	                    Where SubGroupName in ('Fabric','Collar','Cuff')
                    ),
                    BKList As
                    (
	                    Select BM.BookingNo, BM.ExportOrderID, BM.ExportOrderNo, BookingID = Min(BM.BookingID),0 BOMMasterID, ISourcing = CAI.InHouse, BM.BuyerTeamID, ContactID = BM.SupplierID, BM.WithOutOB, RevisionNo = Max(BM.RevisionNo)
	                    , BM.BuyerID, AddedBy = MIN(BM.AddedBy)
	                    FROM BM
	                    Inner Join ISG On ISG.SubGroupID = BM.SubGroupID
	                    Left Join {DbNames.EPYSL}..ContactAdditionalInfo CAI On CAI.ContactID = BM.SupplierID
	                    Where ISNULL(CAI.InHouse,0) = 1
	                    Group By BM.BookingNo, BM.ExportOrderID, BM.ExportOrderNo, CAI.InHouse, BM.BuyerTeamID, BM.SupplierID, BM.WithOutOB, BM.BuyerID, BM.AddedBy
                    ),BAck as (
	                    Select BC.BookingID, BC.BookingNo, BC.ExportOrderID, BC.ExportOrderNo, BC.BOMMasterID,BC.ContactID, BC.BuyerTeamID, BC.ISourcing, BIA.RevisionNo, 
	                    SubGroupID = MIN(BIA.SubGroupID),
	                    ItemGroupID = MIN(BIA.ItemGroupID),
	                    BIA.AcknowledgeDate
	                    ,BC.BuyerID, BC.AddedBy,BC.WithOutOB
	                    FROM BKList BC
	                    Inner Join {DbNames.EPYSL}..BookingItemAcknowledge BIA On BIA.BookingID = BC.BookingID And BIA.WithoutOB = BC.WithOutOB
	                    Group By BC.BookingID, BC.BookingNo, BC.ExportOrderID, BC.ExportOrderNo, BC.BOMMasterID,BC.ContactID, BC.BuyerTeamID, BC.ISourcing, BIA.RevisionNo, BIA.AcknowledgeDate,BC.WithOutOB
	                    ,BC.BuyerID, BC.AddedBy
                    ),InHouseItemList as (
	                    SELECT BC.BOMMasterID,BC.ExportOrderID, BC.ExportOrderNo, BC.BookingID, BC.BookingNo, BC.BuyerTeamID, BC.SubGroupID, BC.ItemGroupID,BC.ContactID, ISourcing = Max(convert(int,BC.ISourcing)), BC.RevisionNo, AcknowledgeDate = Null, BC.WithOutOB,BKAcknowledgeDate = Max(BC.AcknowledgeDate)
	                    ,BC.BuyerID, BC.AddedBy
	                    FROM BAck BC
	                    Group By BC.BOMMasterID,BC.ExportOrderID, BC.ExportOrderNo, BC.BookingID, BC.BookingNo, BC.BuyerTeamID, BC.SubGroupID, BC.ItemGroupID,BC.ContactID, BC.ISourcing, BC.WithOutOB, BC.RevisionNo
	                    ,BC.BuyerID, BC.AddedBy
	                    having BC.ISourcing = 1
                    ),
                    PrevAckList AS(
                        Select OSI.BookingID, IsPrevAck = Case When FBAT.FBAckID is null then 0 Else 1 End 
                        FROM InHouseItemList OSI
                        INNER JOIN FBookingAcknowledge FBAT ON FBAT.BookingNo = OSI.BookingNo
                    ),
                    ACKList as
                    (
	                    Select OSI.ExportOrderID,OSI.ExportOrderNo,OSI.BOMMasterID,OSI.BookingNo, 
	                    RevisionNo = ISNULL(FBA.RevisionNo,0), OSI.BuyerTeamID,
	                    RevStatus = Case When OSI.WithOutOB = 1 then 1 
						                    When Isnull(EL.FabBSCAckStatus,'') = 'Acknowledged' And Isnull(EL.FabBSCAckStatus,'') <> '' Then 1 Else 0 End,
	                    BKAcknowledgeDate = Max(OSI.BKAcknowledgeDate), FBAcknowledgeDate = Max(FBA.AcknowledgeDate), OSI.WithoutOB
	                    ,OSI.BuyerID, OSI.AddedBy
	                    From InHouseItemList OSI
	                    Left Join {DbNames.EPYSL}..ExportWorkOrderLifeCycleChild EL On EL.ExportOrderID = OSI.ExportOrderID And EL.BookingID = OSI.BookingID And EL.ContactID = OSI.ContactID
	                    Left Join FabricBookingAcknowledge FBA On FBA.BookingID = OSI.BookingID  And FBA.SubGroupID = OSI.SubGroupID And FBA.ItemGroupID = OSI.ItemGroupID
                        Left Join PrevAckList PAL ON PAL.BookingID = OSI.BookingID
	                    Where (Case When FBA.AcknowledgeID IS NULL Then 0
					                    When FBA.AcknowledgeID IS NOT NULL And ISNULL(FBA.PreProcessRevNo,0) < ISNULL(OSI.RevisionNo,0) Then 2 Else 1 End 
			                    = Case When 'N'='N' Then 0
					                    When 'N'='N' Then 2 Else 1 End) AND ISNULL(PAL.IsPrevAck,0) = 0
		                    Group By OSI.ExportOrderID,OSI.ExportOrderNo,OSI.BOMMasterID,OSI.ExportOrderID,ISNULL(FBA.RevisionNo,0), OSI.BuyerTeamID,Isnull(EL.FabBSCAckStatus,''),
		                    OSI.BookingNo, OSI.WithoutOB,OSI.BuyerID,OSI.AddedBy
                    ),
                    F AS
                    (
	                    SELECT BK.ExportOrderID,BK.ExportOrderNo,BK.BOMMasterID,BK.RevisionNo, CCT.TeamName BuyerTeamName, CTO.ShortName BuyerName, BK.RevStatus, 
	                    BK.BKAcknowledgeDate, BK.FBAcknowledgeDate,BK.BookingNo, BK.WithoutOB,E.EmployeeName
	                    From ACKList BK
	                    LEFT Join {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = BK.BuyerID
	                    LEFT Join {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = BK.BuyerTeamID
	                    LEFT JOIN {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = BK.AddedBy
	                    LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = LU.EmployeeCode
	                    Group By BK.ExportOrderID,BK.ExportOrderNo,BK.BOMMasterID,BK.RevisionNo, CCT.TeamName,BK.RevStatus, 
	                    BK.BKAcknowledgeDate, BK.FBAcknowledgeDate,BK.BookingNo, BK.WithoutOB, CTO.ShortName,E.EmployeeName
                    )
                    SELECT * INTO #TempTable{tempGuid} FROM F
                    SELECT *,Count(*) Over() TotalRows FROM #TempTable{tempGuid}
                    ";

                    isNeedImage = true;

                    break;

                case Status.Revise:
                    sql = $@"
                     With RunningEWO As
                    (
	                    Select EOM.ExportOrderID, EOM.ExportOrderNo, EOM.StyleMasterID, EOM.BuyerID, EOM.BuyerTeamID
	                    From {DbNames.EPYSL}..ExportOrderMaster EOM
	                    Where EOM.EWOStatusID = 130
                    ),
                    BM As 
                    (
                        Select BM.BookingID, BM.BookingNo, BM.ExportOrderID, BM.SupplierID, BM.SubGroupID, WithOutOB = Convert(bit,0),
	                    EOM.ExportOrderNo, EOM.StyleMasterID, BM.RevisionNo, EOM.BuyerID, EOM.BuyerTeamID, AddedBy = ISNULL(BM.UpdatedBy, BM.AddedBy) 
	                    from {DbNames.EPYSL}..BookingMaster BM
	                    Inner Join RunningEWO EOM On EOM.ExportOrderID = BM.ExportOrderID
						LEFT JOIN {DbNames.EPYSL}..BookingChildImage BCI ON BCI.BookingID = BM.BookingID
	                    where BM.IsCancel = 0 And BM.SubGroupID in (1,11,12) AND BM.Proposed = 1
                        AND BM.BookingDate >= '{_startingDate}'

	                    Union All

	                    Select BM.BookingID, BM.BookingNo, BM.ExportOrderID, SupplierID, 1 SubGroupID, WithOutOB = Convert(bit,1),
	                    EOM.ExportOrderNo, EOM.StyleMasterID, BM.RevisionNo, EOM.BuyerID, EOM.BuyerTeamID, AddedBy = ISNULL(BM.UpdatedBy, BM.AddedBy)
	                    from {DbNames.EPYSL}..SampleBookingMaster BM
	                    Inner Join RunningEWO EOM On EOM.ExportOrderID = BM.ExportOrderID
						LEFT JOIN {DbNames.EPYSL}..SampleBookingChildImage BCI ON BCI.BookingID = BM.BookingID
	                    where BM.IsCancel = 0 And BM.ExportOrderID <> 0 And IsCancel = 0 AND BM.Proposed = 1
                        AND BM.BookingDate >= '{_startingDate}'
                    ),
                    ISG As
                    (
	                    Select * from {DbNames.EPYSL}..ItemSubGroup
	                    Where SubGroupName in ('Fabric','Collar','Cuff')
                    ),
                    BKList As
                    (
	                    Select BM.BookingNo, BM.ExportOrderID, BM.ExportOrderNo, BookingID = Min(BM.BookingID),0 BOMMasterID, ISourcing = CAI.InHouse, BM.BuyerTeamID, ContactID = BM.SupplierID, BM.WithOutOB, RevisionNo = Max(BM.RevisionNo)
	                    ,BM.BuyerID, AddedBy = MAX(BM.AddedBy)
		                FROM BM
	                    Inner Join ISG On ISG.SubGroupID = BM.SubGroupID
	                    Left Join {DbNames.EPYSL}..ContactAdditionalInfo CAI On CAI.ContactID = BM.SupplierID
	                    Where ISNULL(CAI.InHouse,0) = 1
	                    Group By BM.BookingNo, BM.ExportOrderID, BM.ExportOrderNo, CAI.InHouse, BM.BuyerTeamID, BM.SupplierID, BM.WithOutOB ,BM.BuyerID
                    ),BAck as (
	                    Select BC.BookingID, BC.BookingNo, BC.ExportOrderID, BC.ExportOrderNo, BC.BOMMasterID,BC.ContactID, BC.BuyerTeamID, BC.ISourcing, 
						RevisionNo = Case When BC.WithOutOB = 0  Then BIA.RevisionNo Else BC.RevisionNo END, 
						BIA.SubGroupID, BIA.ItemGroupID, BIA.WithoutOB,AcknowledgeDate = CASE WHEN BIA.UnAcknowledge = 1 THEN BIA.UnAcknowledgeDate ELSE BIA.AcknowledgeDate END
	                    ,BC.BuyerID, BC.AddedBy
		                FROM BKList BC
                        Inner Join {DbNames.EPYSL}..BookingItemAcknowledge BIA On BIA.BookingID = BC.BookingID And BIA.WithoutOB = BC.WithOutOB
	                    Group By BC.BookingID, BC.BookingNo, BC.ExportOrderID, BC.ExportOrderNo, BC.BOMMasterID,BC.ContactID, BC.BuyerTeamID, BC.ISourcing, Case When BC.WithOutOB = 0  Then BIA.RevisionNo Else BC.RevisionNo END, BIA.SubGroupID, BIA.ItemGroupID, CASE WHEN BIA.UnAcknowledge = 1 THEN BIA.UnAcknowledgeDate ELSE BIA.AcknowledgeDate END, BIA.WithoutOB
		                ,BC.BuyerID, BC.AddedBy
	                ),
					InHouseItemList as (
	                    SELECT BC.BOMMasterID,BC.ExportOrderID, BC.ExportOrderNo, BC.BookingID, BC.BookingNo, BC.BuyerTeamID, BC.SubGroupID, BC.ItemGroupID,BC.ContactID, ISourcing = Max(convert(int,BC.ISourcing)), BC.RevisionNo, AcknowledgeDate = Null, BC.WithOutOB,BKAcknowledgeDate = Max(BC.AcknowledgeDate)
	                    ,BC.BuyerID, BC.AddedBy
		                FROM BAck BC
	                    Group By BC.BOMMasterID,BC.ExportOrderID, BC.ExportOrderNo, BC.BookingID, BC.BookingNo, BC.BuyerTeamID, BC.SubGroupID, BC.ItemGroupID,BC.ContactID, BC.ISourcing, BC.WithOutOB, BC.RevisionNo
	                    ,BC.BuyerID, BC.AddedBy
		                having BC.ISourcing = 1
                    ),
                    ACKList as
                    (
                        Select OSI.ExportOrderID,OSI.ExportOrderNo,OSI.BOMMasterID,OSI.BookingNo, 
	                    RevNoValue = MAX(ISNULL(FB.RevisionNo,0)), OSI.BuyerTeamID,
	                    RevStatus = Case When OSI.WithOutOB = 1 then 1 
                                            When Isnull(EL.FabBSCAckStatus,'') = 'Acknowledged' And Isnull(EL.FabBSCAckStatus,'') <> '' Then 1 Else 0 End,
	                    BKAcknowledgeDate = Max(OSI.BKAcknowledgeDate), FBAcknowledgeDate = Max(FBA.AcknowledgeDate), OSI.WithoutOB
                        ,OSI.BuyerID, OSI.AddedBy
		                From InHouseItemList OSI
	                    Left Join {DbNames.EPYSL}..ExportWorkOrderLifeCycleChild EL On EL.ExportOrderID = OSI.ExportOrderID And EL.BookingID = OSI.BookingID And EL.ContactID = OSI.ContactID
                        Left Join EPYSLTEX..FabricBookingAcknowledge FBA On FBA.BookingID = OSI.BookingID  And FBA.SubGroupID = OSI.SubGroupID And FBA.ItemGroupID = OSI.ItemGroupID
						Left Join EPYSLTEX..FBookingAcknowledge FB On FB.BookingID = OSI.BookingID  And FB.SubGroupID = OSI.SubGroupID And FB.ItemGroupID = OSI.ItemGroupID
                        Left Join EPYSLTEX..FBookingAcknowledge FB2 On FB2.BookingNo = OSI.BookingNo--FB2.ExportOrderID = OSI.ExportOrderID
                        Where --FBA.UnAcknowledge = 0 AND 
                              Case When FB2.FBAckID IS NOT NULL AND FBA.PreProcessRevNo IS NULL AND OSI.RevisionNo IS NOT NULL Then 2
                                   When FBA.AcknowledgeID IS NULL Then 0
					               When FBA.AcknowledgeID IS NOT NULL And ISNULL(FBA.PreProcessRevNo,0) < ISNULL(OSI.RevisionNo,0) Then 2 Else 1 End 
			                        = Case When 'R'='N' Then 0
					                    When 'R'='R' Then 2 Else 1 End
		                    Group By OSI.ExportOrderID,OSI.ExportOrderNo,OSI.BOMMasterID,OSI.ExportOrderID, OSI.BuyerTeamID,Isnull(EL.FabBSCAckStatus,''),
		                    OSI.BookingNo, OSI.WithoutOB,OSI.BuyerID, OSI.AddedBy
                    ),
                    F AS
                    (
	                    SELECT BK.ExportOrderID,BK.ExportOrderNo,BK.BOMMasterID,BK.RevNoValue, CCT.TeamName BuyerTeamName, CTO.ShortName BuyerName,BK.RevStatus, 
	                    BK.BKAcknowledgeDate, BK.FBAcknowledgeDate,BK.BookingNo, BK.WithoutOB,E.EmployeeName
	                    From ACKList BK
	                    LEFT Join {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = BK.BuyerID
		                LEFT Join {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = BK.BuyerTeamID
		                LEFT JOIN {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = BK.AddedBy
		                LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = LU.EmployeeCode
	                    Group By BK.ExportOrderID,BK.ExportOrderNo,BK.BOMMasterID,BK.RevNoValue, CCT.TeamName,BK.RevStatus, 
	                    BK.BKAcknowledgeDate, BK.FBAcknowledgeDate,BK.BookingNo, BK.WithoutOB,CTO.ShortName,E.EmployeeName
                    )
                    SELECT * INTO #TempTable{tempGuid} FROM F
                    SELECT *,Count(*) Over() TotalRows FROM #TempTable{tempGuid}
                    ";

                    isNeedImage = true;

                    break;

                case Status.Completed:
                    sql = $@"With RunningEWO As
                    (
	                    Select EOM.ExportOrderID, EOM.ExportOrderNo, EOM.StyleMasterID, EOM.BuyerID, EOM.BuyerTeamID
	                    From {DbNames.EPYSL}..ExportOrderMaster EOM
	                    Where EOM.EWOStatusID = 130
                    ),
                    BM As 
                    (
	                    Select BM.BookingID, BM.BookingNo, BM.ExportOrderID, BM.SupplierID, BM.SubGroupID, WithOutOB = Convert(bit,0),
	                    EOM.ExportOrderNo, EOM.StyleMasterID, RevisionNo = BM.PreProcessRevNo, EOM.BuyerID, EOM.BuyerTeamID, AddedBy = ISNULL(BM.UpdatedBy, BM.AddedBy)
	                    from {DbNames.EPYSL}..BookingMaster BM
	                    Inner Join RunningEWO EOM On EOM.ExportOrderID = BM.ExportOrderID
	                    left join FBookingAcknowledge FBA ON FBA.BookingID = BM.BookingID
	                    where BM.IsCancel = 0 And BM.SubGroupID in (1,11,12) AND FBA.FBAckID IS NOT NULL
                        AND BM.BookingDate >= '{_startingDate}'

	                    Union All

	                    Select BM.BookingID, BM.BookingNo, BM.ExportOrderID, BM.SupplierID, 1 SubGroupID, WithOutOB = Convert(bit,1),
	                    EOM.ExportOrderNo, EOM.StyleMasterID, RevisionNo = BM.RevisionNo, EOM.BuyerID, EOM.BuyerTeamID, AddedBy = ISNULL(BM.UpdatedBy, BM.AddedBy)
	                    from {DbNames.EPYSL}..SampleBookingMaster BM
	                    Inner Join RunningEWO EOM On EOM.ExportOrderID = BM.ExportOrderID
	                    left join FBookingAcknowledge FBA ON FBA.BookingID = BM.BookingID
	                    where BM.IsCancel = 0 And BM.ExportOrderID <> 0 AND FBA.FBAckID IS NOT NULL --AND BM.Proposed = 1
                        AND BM.BookingDate >= '{_startingDate}'
                    ),
                    ISG As
                    (
	                    Select * from {DbNames.EPYSL}..ItemSubGroup
	                    Where SubGroupName in ('Fabric','Collar','Cuff')
                    ),
                    BKList As
                    (
	                    Select BM.BookingNo, BM.ExportOrderID, BM.ExportOrderNo, BookingID = Min(BM.BookingID),0 BOMMasterID, 
	                    ISourcing = CAI.InHouse, BM.BuyerID, BM.BuyerTeamID, ContactID = BM.SupplierID, AddedBy = MAX(BM.AddedBy),
	                    BM.WithOutOB, RevisionNo = Max(BM.RevisionNo)
	                    FROM BM
	                    Inner Join ISG On ISG.SubGroupID = BM.SubGroupID
	                    Left Join {DbNames.EPYSL}..ContactAdditionalInfo CAI On CAI.ContactID = BM.SupplierID
	                    Where ISNULL(CAI.InHouse,0) = 1
	                    Group By BM.BookingNo, BM.ExportOrderID, BM.ExportOrderNo, CAI.InHouse, BM.BuyerID, 
	                    BM.BuyerTeamID, BM.SupplierID, BM.WithOutOB
                    )
                    ,BAck as (
	                    Select BC.BookingID, BC.BookingNo, BC.ExportOrderID, BC.ExportOrderNo, BC.BOMMasterID,
	                    BC.ContactID, BC.BuyerID, BC.BuyerTeamID, BC.ISourcing, BC.RevisionNo, BIA.SubGroupID, 
	                    BIA.ItemGroupID, BIA.AcknowledgeDate, BIA.WithoutOB, BC.AddedBy
	                    FROM BKList BC
                        Inner Join {DbNames.EPYSL}..BookingItemAcknowledge BIA On BIA.BookingID = BC.BookingID And BIA.WithoutOB = BC.WithOutOB
	                    Group By BC.BookingID, BC.BookingNo, BC.ExportOrderID, BC.ExportOrderNo, BC.BOMMasterID,
	                    BC.ContactID, BC.BuyerID, BC.BuyerTeamID, BC.ISourcing, BC.RevisionNo, BIA.SubGroupID, 
	                    BIA.ItemGroupID, BIA.AcknowledgeDate, BIA.WithoutOB, BC.AddedBy
                    ),InHouseItemList as (
	                    SELECT BC.BOMMasterID,BC.ExportOrderID, BC.ExportOrderNo, BC.BookingID, BC.BookingNo, 
	                    BC.BuyerID, BC.BuyerTeamID, BC.SubGroupID, BC.ItemGroupID,BC.ContactID, BC.AddedBy,
	                    ISourcing = Max(convert(int,BC.ISourcing)), BC.RevisionNo, AcknowledgeDate = Null, 
	                    BC.WithOutOB,BKAcknowledgeDate = Max(BC.AcknowledgeDate)
	                    FROM BAck BC
	                    Group By BC.BOMMasterID,BC.ExportOrderID, BC.ExportOrderNo, BC.BookingID, BC.BookingNo, BC.BuyerID, 
	                    BC.BuyerTeamID, BC.SubGroupID, BC.ItemGroupID,BC.ContactID, BC.ISourcing, BC.WithOutOB, BC.RevisionNo,
	                    BC.AddedBy
	                    having BC.ISourcing = 1
                    ),
                    ACKList as
                    (
                        Select OSI.ExportOrderID,OSI.ExportOrderNo,OSI.BOMMasterID,OSI.BookingID,OSI.BookingNo, 
	                    RevisionNo = OSI.RevisionNo, OSI.BuyerID, OSI.BuyerTeamID,
	                    RevStatus = Case When OSI.WithOutOB = 1 then 1 
                                            When Isnull(EL.FabBSCAckStatus,'') = 'Acknowledged' And Isnull(EL.FabBSCAckStatus,'') <> '' Then 1 Else 0 End,
	                    BKAcknowledgeDate = Max(OSI.BKAcknowledgeDate), OSI.WithoutOB,E.EmployeeName, FBAcknowledgeDate = MAX(FBA.AcknowledgeDate)
                        From InHouseItemList OSI
	                    Left Join {DbNames.EPYSL}..ExportWorkOrderLifeCycleChild EL On EL.ExportOrderID = OSI.ExportOrderID And EL.BookingID = OSI.BookingID And EL.ContactID = OSI.ContactID
                        Inner Join FabricBookingAcknowledge FBA On FBA.BookingID = OSI.BookingID And FBA.SubGroupID = OSI.SubGroupID And FBA.ItemGroupID = OSI.ItemGroupID
                        Left Join FBookingAcknowledgeChild FBC On FBC.BookingID = OSI.BookingID
                        LEFT JOIN FBookingAcknowledge FBA1 ON FBA1.BookingID = FBA.BookingID
	                    LEFT JOIN {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = OSI.AddedBy -- FBA1.MerchandiserID
	                    LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = LU.EmployeeCode
	                    WHERE FBA.AcknowledgeDate >= '{_startingDate}' AND
	                    --FBA.PreProcessRevNo = OSI.RevisionNo AND 
	                    (IsNull(FBC.IsTxtAck,0)=1) And IsNull(FBC.IsMktUnAck,0)=0
	                    --AND FBA1.IsUnAcknowledge=0	                
                        Group By OSI.ExportOrderID,OSI.ExportOrderNo,OSI.BOMMasterID,OSI.ExportOrderID, OSI.BuyerID, OSI.BuyerTeamID,Isnull(EL.FabBSCAckStatus,''),
	                    OSI.BookingID,OSI.BookingNo, OSI.WithoutOB,E.EmployeeName,FBA1.MerchandiserID, OSI.RevisionNo
                    ),       
                    F AS(
	                    SELECT BK.ExportOrderID,BK.ExportOrderNo,BK.BOMMasterID,
                        RevNoValue = FBA.RevisionNo, --COALESCE(NULLIF(CAST(BK.RevisionNo AS VARCHAR(10)) ,'0'), ''),
                        BK.EmployeeName,
	                    BuyerName = ISNULL(CTO.ShortName,''),
	                    BuyerTeamName = ISNULL(CCT.TeamName,''),
	                    BK.RevStatus, 
	                    BK.BKAcknowledgeDate, BK.FBAcknowledgeDate,BK.BookingID,BK.BookingNo, 
	                    BK.WithoutOB, PMCApproveBy = E.EmployeeName,
	                    AcknowledgeDate = FBA.DateAdded,
	                    AcknowledgeByName = E1.EmployeeName,
	                    BK.RevisionNo,FBA.PreRevisionNo,
                        IsRevisionValid = CASE WHEN BK.RevisionNo = FBA.PreRevisionNo THEN 1 ELSE 0 END
	                    From FBookingAcknowledge FBA
                        INNER JOIN FabricBookingAcknowledge FBA1 ON FBA1.BookingID = FBA.BookingID AND FBA1.SubGroupID = FBA.SubGroupID
                        LEFT JOIN ACKList BK ON BK.BookingID = FBA.BookingID
                        LEFT JOIN {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = FBA.ApprovedByPMC
	                    LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = LU.EmployeeCode 
	                    Inner Join {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = BK.BuyerID
	                    Inner Join {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = BK.BuyerTeamID
	                    LEFT JOIN {DbNames.EPYSL}..LoginUser LU1 ON LU1.UserCode = FBA1.AddedBy
	                    LEFT JOIN {DbNames.EPYSL}..Employee E1 ON E1.EmployeeCode = LU1.EmployeeCode
                        WHERE FBA1.AcknowledgeDate >= '{_startingDate}'
                        --WHERE BK.RevisionNo = FBA.PreRevisionNo
	                    Group By BK.ExportOrderID,BK.ExportOrderNo,BK.BOMMasterID,BK.RevisionNo,ISNULL(CTO.ShortName,''),ISNULL(CCT.TeamName,''),BK.RevStatus, 
	                    BK.BKAcknowledgeDate, BK.BookingID,BK.BookingNo, BK.WithoutOB,BK.EmployeeName, E.EmployeeName, BK.FBAcknowledgeDate,
	                    FBA.DateAdded,E1.EmployeeName,FBA.PreRevisionNo,FBA.RevisionNo
                    )
                    SELECT * INTO #TempTable{tempGuid} FROM F
                    SELECT *,Count(*) Over() TotalRows FROM #TempTable{tempGuid}
                    ";

                    isNeedImage = true;

                    break;
                case Status.ProposedForAcknowledge:
                    sql = $@"With RunningEWO As
                (
	                Select EOM.ExportOrderID, EOM.ExportOrderNo, EOM.StyleMasterID
	                From {DbNames.EPYSL}..ExportOrderMaster EOM
	                Where EOM.EWOStatusID = 130
                ),
                BM As 
                (
                    
	                Select BM.BookingID, BM.BookingNo, BM.ExportOrderID, BM.SupplierID, BM.SubGroupID, WithOutOB = Convert(bit,0), AddedBy = ISNULL(BM.UpdatedBy, BM.AddedBy),
	                EOM.ExportOrderNo, EOM.StyleMasterID, BM.RevisionNo, BM.BuyerID, BM.BuyerTeamID
	                from {DbNames.EPYSL}..BookingMaster BM
	                Inner Join RunningEWO EOM On EOM.ExportOrderID = BM.ExportOrderID
	                left join FBookingAcknowledge FBA ON FBA.BookingID = BM.BookingID
                    LEFT JOIN {DbNames.EPYSL}..BookingChildImage BCI ON BCI.BookingID = BM.BookingID
	                where BM.IsCancel = 0 And BM.SubGroupID in (1,11,12)
                    AND BM.BookingDate >= '{_startingDate}'
                    
	                Union All

	                Select BM.BookingID, BM.BookingNo, BM.ExportOrderID, BM.SupplierID, 1 SubGroupID, WithOutOB = Convert(bit,1), AddedBy = ISNULL(BM.UpdatedBy, BM.AddedBy),
	                EOM.ExportOrderNo, EOM.StyleMasterID, BM.RevisionNo, BM.BuyerID, BM.BuyerTeamID
	                from {DbNames.EPYSL}..SampleBookingMaster BM
	                Inner Join RunningEWO EOM On EOM.ExportOrderID = BM.ExportOrderID
	                left join FBookingAcknowledge FBA ON FBA.BookingID = BM.BookingID
                    LEFT JOIN {DbNames.EPYSL}..SampleBookingChildImage BCI ON BCI.BookingID = BM.BookingID
	                where BM.IsCancel = 0 And BM.ExportOrderID <> 0 and BM.Proposed = 1
                    AND BM.BookingDate >= '{_startingDate}'
                    
                ),
                ISG As
                (
	                Select * from {DbNames.EPYSL}..ItemSubGroup
	                Where SubGroupName in ('Fabric','Collar','Cuff')
                ),
                BKList As
                (
	                Select BM.BookingNo, BM.ExportOrderID, BM.ExportOrderNo, BookingID = Min(BM.BookingID),0 BOMMasterID, 
	                ISourcing = CAI.InHouse, BM.BuyerID, BM.BuyerTeamID, ContactID = BM.SupplierID, BM.WithOutOB, 
	                RevisionNo = Max(BM.RevisionNo), BM.AddedBy
	                FROM BM
	                Inner Join ISG On ISG.SubGroupID = BM.SubGroupID
	                Left Join {DbNames.EPYSL}..ContactAdditionalInfo CAI On CAI.ContactID = BM.SupplierID
	                Where ISNULL(CAI.InHouse,0) = 1
	                Group By BM.BookingNo, BM.ExportOrderID, BM.ExportOrderNo, CAI.InHouse, 
	                BM.BuyerID, BM.BuyerTeamID, BM.SupplierID, BM.WithOutOB, BM.AddedBy
                ),BAck as (
	                Select BC.BookingID, BC.BookingNo, BC.ExportOrderID, BC.ExportOrderNo, BC.BOMMasterID,BC.ContactID, 
	                BC.BuyerID, BC.BuyerTeamID, BC.ISourcing, BIA.RevisionNo, BIA.SubGroupID, BIA.ItemGroupID, BIA.AcknowledgeDate, BC.AddedBy, 
	                BIA.WithoutOB
	                FROM BKList BC
                    Inner Join {DbNames.EPYSL}..BookingItemAcknowledge BIA On BIA.BookingID = BC.BookingID And BIA.WithoutOB = BC.WithOutOB
	                Group By BC.BookingID, BC.BookingNo, BC.ExportOrderID, BC.ExportOrderNo, BC.BOMMasterID,BC.ContactID, BC.BuyerID, 
	                BC.BuyerTeamID, BC.ISourcing, BIA.RevisionNo, BIA.SubGroupID, BIA.ItemGroupID, BIA.AcknowledgeDate, BIA.WithoutOB, BC.AddedBy
                ),InHouseItemList as (
	                SELECT BC.BOMMasterID,BC.ExportOrderID, BC.ExportOrderNo, BC.BookingID, BC.BookingNo, BC.BuyerID, BC.BuyerTeamID, 
	                BC.SubGroupID, BC.ItemGroupID,BC.ContactID, ISourcing = Max(convert(int,BC.ISourcing)), BC.RevisionNo, 
	                AcknowledgeDate = Null, BC.WithOutOB,BKAcknowledgeDate = Max(BC.AcknowledgeDate), BC.AddedBy
	                FROM BAck BC
	                Group By BC.BOMMasterID,BC.ExportOrderID, BC.ExportOrderNo, BC.BookingID, BC.BookingNo, BC.BuyerID, BC.BuyerTeamID, 
	                BC.SubGroupID, BC.ItemGroupID,BC.ContactID, BC.ISourcing, BC.WithOutOB, BC.RevisionNo, BC.AddedBy
	                having BC.ISourcing = 1
                ),
                ACKList as
                (
	                Select OSI.ExportOrderID,OSI.ExportOrderNo,OSI.BOMMasterID,OSI.BookingID,OSI.BookingNo,
	                RevisionNo = Max(ISNULL(FBA1.RevisionNo,0)), OSI.BuyerID, OSI.BuyerTeamID,
	                RevStatus = Case When OSI.WithOutOB = 1 then 1 
	                When Isnull(EL.FabBSCAckStatus,'') = 'Acknowledged' And Isnull(EL.FabBSCAckStatus,'') <> '' Then 1 Else 0 End,
	                BKAcknowledgeDate = Max(OSI.BKAcknowledgeDate), FBAcknowledgeDate = MAX(FBA.AcknowledgeDate), OSI.WithoutOB,E.EmployeeName
	                From InHouseItemList OSI
	                Left Join {DbNames.EPYSL}..ExportWorkOrderLifeCycleChild EL On EL.ExportOrderID = OSI.ExportOrderID And EL.BookingID = OSI.BookingID And EL.ContactID = OSI.ContactID
	                Inner Join FabricBookingAcknowledge FBA On FBA.BookingID = OSI.BookingID And FBA.SubGroupID = OSI.SubGroupID And FBA.ItemGroupID = OSI.ItemGroupID
	                LEFT JOIN FBookingAcknowledge FBA1 ON FBA1.BookingID = FBA.BookingID AND FBA1.SubGroupID = FBA.SubGroupID
	                LEFT JOIN {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = OSI.AddedBy
	                LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = LU.EmployeeCode
                    Where FBA.AcknowledgeDate >= '{_startingDate}' --AND IsNull(FBC.SendToMktAck,0)=1 And IsNull(FBC.IsMktAck,0)=0 And IsNull(FBC.IsMktUnAck,0)=0
                    AND FBA.RevisionNo = FBA1.PreRevisionNo
	                Group By OSI.ExportOrderID,OSI.ExportOrderNo,OSI.BOMMasterID,OSI.ExportOrderID, OSI.BuyerID, OSI.BuyerTeamID,Isnull(EL.FabBSCAckStatus,''),
	                OSI.BookingID,OSI.BookingNo, OSI.WithoutOB,E.EmployeeName,FBA1.MerchandiserID
                ),
	            FBC AS
	            (
		            SELECT AL.BookingNo
		            FROM ACKList AL
		            INNER JOIN FBookingAcknowledge FBA ON FBA.BookingID = AL.BookingID
		            INNER JOIN FBookingAcknowledgeChild FBC ON FBC.AcknowledgeID = FBA.FBAckID
		            WHERE IsNull(FBC.SendToMktAck,0)=1 And IsNull(FBC.IsMktAck,0)=0 And IsNull(FBC.IsMktUnAck,0)=0
		            GROUP BY AL.BookingNo
	            ),
                F AS(
	                SELECT BK.ExportOrderID,BK.ExportOrderNo,BK.BOMMasterID,
                    RevNoValue=COALESCE(NULLIF(CAST(BK.RevisionNo AS VARCHAR(10)) ,'0'), ''),BK.EmployeeName,
	                BuyerName = ISNULL(CTO.ShortName,''),
	                BuyerTeamName = ISNULL(CCT.TeamName,''),
	                BK.RevStatus, 
	                BK.BKAcknowledgeDate, BK.FBAcknowledgeDate,BK.BookingID,BK.BookingNo, BK.WithoutOB
	                From ACKList BK
		            INNER JOIN FBC ON FBC.BookingNo = BK.BookingNo
	                Inner Join {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = BK.BuyerID
	                Inner Join {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = BK.BuyerTeamID
	                Group By BK.ExportOrderID,BK.ExportOrderNo,BK.BOMMasterID,BK.RevisionNo, ISNULL(CTO.ShortName,''), ISNULL(CCT.TeamName,''),BK.RevStatus, 
	                BK.BKAcknowledgeDate, BK.FBAcknowledgeDate,BK.BookingID,BK.BookingNo, BK.WithoutOB,BK.EmployeeName
                )

                SELECT * INTO #TempTable{tempGuid} FROM F
                SELECT *,Count(*) Over() TotalRows FROM #TempTable{tempGuid}
                ";

                    isNeedImage = true;

                    break;
                case Status.UnAcknowledge:
                    sql = $@"With RunningEWO As
                (
	                Select EOM.ExportOrderID, EOM.ExportOrderNo, EOM.StyleMasterID, EOM.BuyerID, EOM.BuyerTeamID
	                From {DbNames.EPYSL}..ExportOrderMaster EOM
	                Where EOM.EWOStatusID = 130
                ),
                BM As 
                (
                    Select BM.BookingID, BM.BookingNo, BM.ExportOrderID, BM.SupplierID, BM.SubGroupID, WithOutOB = Convert(bit,0),
	                EOM.ExportOrderNo, EOM.StyleMasterID, BM.RevisionNo, EOM.BuyerID, EOM.BuyerTeamID,FBA.IsUnAcknowledge, AddedBy = ISNULL(BM.UpdatedBy, BM.AddedBy)
	                from {DbNames.EPYSL}..BookingMaster BM
	                Inner Join RunningEWO EOM On EOM.ExportOrderID = BM.ExportOrderID
	                left join FabricBookingAcknowledge FBAA ON FBAA.BookingID = BM.BookingID
	                left join FBookingAcknowledge FBA ON FBA.BookingID = BM.BookingID
                    LEFT JOIN {DbNames.EPYSL}..BookingChildImage BCI ON BCI.BookingID = BM.BookingID
	                where BM.IsCancel = 0 And BM.SubGroupID in (1,11,12)
                    AND BM.BookingDate >= '{_startingDate}'
                    
	                Union All

	                Select BM.BookingID, BM.BookingNo, BM.ExportOrderID, BM.SupplierID, 1 SubGroupID, WithOutOB = Convert(bit,1),
	                EOM.ExportOrderNo, EOM.StyleMasterID, BM.RevisionNo, EOM.BuyerID, EOM.BuyerTeamID,FBA.IsUnAcknowledge, AddedBy = ISNULL(BM.UpdatedBy, BM.AddedBy)
	                from {DbNames.EPYSL}..SampleBookingMaster BM
	                Inner Join RunningEWO EOM On EOM.ExportOrderID = BM.ExportOrderID
	                left join FabricBookingAcknowledge FBAA ON FBAA.BookingID = BM.BookingID
	                left join FBookingAcknowledge FBA ON FBA.BookingID = BM.BookingID
                    LEFT JOIN {DbNames.EPYSL}..SampleBookingChildImage BCI ON BCI.BookingID = BM.BookingID
	                where BM.IsCancel = 0 And BM.ExportOrderID <> 0 and BM.Proposed = 1 
                    AND BM.BookingDate >= '{_startingDate}'
                    
                ),
                ISG As
                (
	                Select * from {DbNames.EPYSL}..ItemSubGroup
	                Where SubGroupName in ('Fabric','Collar','Cuff')
                ),
                BKList As
                (
	                Select BM.BookingNo,UnAcknowledge=Max(Convert(int,BM.IsUnAcknowledge)), BM.ExportOrderID, BM.ExportOrderNo, 
	                BookingID = Min(BM.BookingID),0 BOMMasterID, ISourcing = CAI.InHouse, BM.BuyerID, BM.BuyerTeamID, 
	                ContactID = BM.SupplierID, BM.WithOutOB, RevisionNo = Max(BM.RevisionNo), BM.AddedBy
	                FROM BM
	                Inner Join ISG On ISG.SubGroupID = BM.SubGroupID
	                Left Join {DbNames.EPYSL}..ContactAdditionalInfo CAI On CAI.ContactID = BM.SupplierID
	                Where ISNULL(CAI.InHouse,0) = 1
	                Group By BM.BookingNo, BM.ExportOrderID, BM.ExportOrderNo, CAI.InHouse, BM.BuyerID, BM.BuyerTeamID, BM.SupplierID, BM.WithOutOB,BM.AddedBy
                ),BAck as (
	                Select BC.BookingID,BC.UnAcknowledge, BC.BookingNo, BC.ExportOrderID, BC.ExportOrderNo, 
	                BC.BOMMasterID,BC.ContactID, BC.BuyerID, BC.BuyerTeamID, BC.ISourcing, BIA.RevisionNo, BIA.SubGroupID, 
	                BIA.ItemGroupID, BIA.AcknowledgeDate, BIA.WithoutOB, BC.AddedBy
	                FROM BKList BC
                    Inner Join {DbNames.EPYSL}..BookingItemAcknowledge BIA On BIA.BookingID = BC.BookingID And BIA.WithoutOB = BC.WithOutOB
	                Group By BC.BookingID,BC.UnAcknowledge, BC.BookingNo, BC.ExportOrderID, BC.ExportOrderNo, 
	                BC.BOMMasterID,BC.ContactID, BC.BuyerID, BC.BuyerTeamID, BC.ISourcing, BIA.RevisionNo, BIA.SubGroupID, 
	                BIA.ItemGroupID, BIA.AcknowledgeDate, BIA.WithoutOB, BC.AddedBy
                ),InHouseItemList as (
	                SELECT BC.BOMMasterID,BC.UnAcknowledge,BC.ExportOrderID, BC.ExportOrderNo, BC.BookingID, BC.AddedBy, 
	                BC.BookingNo, BC.BuyerID, BC.BuyerTeamID, BC.SubGroupID, BC.ItemGroupID,BC.ContactID, 
	                ISourcing = Max(convert(int,BC.ISourcing)), BC.RevisionNo, AcknowledgeDate = Null, BC.WithOutOB,BKAcknowledgeDate = Max(BC.AcknowledgeDate)
	                FROM BAck BC
	                Group By BC.BOMMasterID,BC.UnAcknowledge,BC.ExportOrderID, BC.ExportOrderNo, BC.BookingID, 
	                BC.BookingNo, BC.BuyerID, BC.BuyerTeamID, BC.SubGroupID, BC.ItemGroupID,BC.ContactID, 
	                BC.ISourcing, BC.WithOutOB, BC.RevisionNo, BC.AddedBy
	                having BC.ISourcing = 1
                ),
                ACKList as
                (
                    Select OSI.ExportOrderID,OSI.ExportOrderNo,OSI.BOMMasterID,OSI.BookingID,OSI.BookingNo, 
                    RevisionNo = Max(ISNULL(FBA.RevisionNo,0)), OSI.BuyerID, OSI.BuyerTeamID,
                    RevStatus = Case When OSI.WithOutOB = 1 then 1 
                    When Isnull(EL.FabBSCAckStatus,'') = 'Acknowledged' And Isnull(EL.FabBSCAckStatus,'') <> '' Then 1 Else 0 End,
                    BKAcknowledgeDate = Max(OSI.BKAcknowledgeDate), 
	                FBAcknowledgeDate = MAX(FBA.AcknowledgeDate), 
	                OSI.WithoutOB,
	                E.EmployeeName,

	                FBA1.UnAcknowledgeDate,
	                UnAcknowledgeByName = E1.EmployeeName,
	                FBA1.UnAcknowledgeReason

                    From InHouseItemList OSI
                    Left Join {DbNames.EPYSL}..ExportWorkOrderLifeCycleChild EL On EL.ExportOrderID = OSI.ExportOrderID And EL.BookingID = OSI.BookingID And EL.ContactID = OSI.ContactID
                    Inner Join FabricBookingAcknowledge FBA On FBA.BookingID = OSI.BookingID And FBA.SubGroupID = OSI.SubGroupID And FBA.ItemGroupID = OSI.ItemGroupID
                    Left Join FBookingAcknowledgeChild FBC On FBC.BookingID = OSI.BookingID
                    LEFT JOIN FBookingAcknowledge FBA1 ON FBA1.BookingID = FBA.BookingID
	                LEFT JOIN {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = OSI.AddedBy
	                LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = LU.EmployeeCode
	                LEFT JOIN {DbNames.EPYSL}..LoginUser LU1 ON LU1.UserCode = FBA.UnAcknowledgeBy
	                LEFT JOIN {DbNames.EPYSL}..Employee E1 ON E1.EmployeeCode = LU1.EmployeeCode
	                Where Cast(FBA1.UnAcknowledgeDate As Date)>= CAST('{_startingDate}' AS DATE) 
                    --AND FBA.PreProcessRevNo = OSI.RevisionNo 
                    AND OSI.UnAcknowledge = 1 Or ( IsNull(FBC.IsTxtUnAck,0)=1 Or  ( IsNull(FBC.IsMktUnAck,0)=1 And IsNull(FBC.SendToTxtAck,0)=1 ))
                    Group By OSI.ExportOrderID,OSI.ExportOrderNo,OSI.BOMMasterID,OSI.ExportOrderID, OSI.BuyerID, OSI.BuyerTeamID,Isnull(EL.FabBSCAckStatus,''),
                    OSI.BookingID,OSI.BookingNo, OSI.WithoutOB,FBA1.MerchandiserID,E.EmployeeName, FBA1.UnAcknowledgeDate, E1.EmployeeName, FBA1.UnAcknowledgeReason
                ),       
                F AS(
                    SELECT BK.ExportOrderID,BK.ExportOrderNo,BK.BOMMasterID,
                    RevNoValue=COALESCE(NULLIF(CAST(BK.RevisionNo AS VARCHAR(10)) ,'0'), ''),BK.EmployeeName,
                    BuyerName = ISNULL(CTO.ShortName,''),
	                BuyerTeamName = ISNULL(CCT.TeamName,''),
                    BK.RevStatus, 
                    BK.BKAcknowledgeDate, BK.FBAcknowledgeDate,BK.BookingID,BK.BookingNo, 
	                BK.WithoutOB,
	                BK.UnAcknowledgeDate, BK.UnAcknowledgeByName, BK.UnAcknowledgeReason
                    From ACKList BK
                    --INNER JOIN FBookingAcknowledge FBA ON FBA.BookingID = BK.BookingID
                    Inner Join {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = BK.BuyerID
                    Inner Join {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = BK.BuyerTeamID
                    --WHERE BK.RevisionNo = FBA.PreRevisionNo
                    Group By BK.ExportOrderID,BK.ExportOrderNo,BK.BOMMasterID,BK.RevisionNo, ISNULL(CTO.ShortName,''), ISNULL(CCT.TeamName,''),BK.RevStatus, 
                    BK.BKAcknowledgeDate, BK.FBAcknowledgeDate,BK.BookingID,BK.BookingNo, BK.WithoutOB,BK.EmployeeName,
	                BK.UnAcknowledgeDate, BK.UnAcknowledgeByName, BK.UnAcknowledgeReason
                )

                SELECT * INTO #TempTable{tempGuid} FROM F
                SELECT *,Count(*) Over() TotalRows FROM #TempTable{tempGuid}
                ";

                    isNeedImage = true;

                    break;
                case Status.Reject:
                    sql = $@"
                    With RunningEWO As
                    (
	                    Select EOM.ExportOrderID, EOM.ExportOrderNo, EOM.StyleMasterID, EOM.EWOStatusID
	                    From {DbNames.EPYSL}..ExportOrderMaster EOM
	                    Where EOM.EWOStatusID IN (130,131)
                    ),
                    BM As 
                    (
	                    Select BM.BookingID, BM.BookingNo, BM.ExportOrderID, BM.SupplierID, BM.SubGroupID, WithOutOB = Convert(bit,0),
	                    EOM.ExportOrderNo, EOM.StyleMasterID, BM.RevisionNo, BM.BuyerID, BM.BuyerTeamID, AddedBy = ISNULL(BM.UpdatedBy, BM.AddedBy)
	                    from {DbNames.EPYSL}..BookingMaster BM
	                    INNER Join RunningEWO EOM On EOM.ExportOrderID = BM.ExportOrderID
	                    left join FBookingAcknowledge FBA ON FBA.BookingID = BM.BookingID
                        LEFT JOIN {DbNames.EPYSL}..BookingChildImage BCI ON BCI.BookingID = BM.BookingID
	                    where (BM.IsCancel = 1 OR EOM.EWOStatusID = 131) And BM.SubGroupID in (1,11,12)
                        AND BM.BookingDate >= '{_startingDate}'
                    
	                    Union All

	                    Select BM.BookingID, BM.BookingNo, BM.ExportOrderID, BM.SupplierID, 1 SubGroupID, WithOutOB = Convert(bit,1),
	                    EOM.ExportOrderNo, EOM.StyleMasterID, BM.RevisionNo, BM.BuyerID, BM.BuyerTeamID, AddedBy = ISNULL(BM.UpdatedBy, BM.AddedBy)
	                    from {DbNames.EPYSL}..SampleBookingMaster BM
	                    INNER Join RunningEWO EOM On EOM.ExportOrderID = BM.ExportOrderID
	                    left join FBookingAcknowledge FBA ON FBA.BookingID = BM.BookingID
                        LEFT JOIN {DbNames.EPYSL}..SampleBookingChildImage BCI ON BCI.BookingID = BM.BookingID
	                    where (BM.IsCancel = 1 OR EOM.EWOStatusID = 131) And BM.ExportOrderID <> 0 and BM.Proposed = 1 
                        AND BM.BookingDate >= '{_startingDate}'
                    ),
                    ISG As
                    (
	                    Select * from {DbNames.EPYSL}..ItemSubGroup
	                    Where SubGroupName in ('Fabric','Collar','Cuff')
                    ),
                    BKList As
                    (
	                    Select BM.BookingNo, BM.ExportOrderID, BM.ExportOrderNo, BookingID = Min(BM.BookingID),
	                    0 BOMMasterID, ISourcing = CAI.InHouse, BM.BuyerID, BM.BuyerTeamID, 
	                    ContactID = BM.SupplierID, BM.WithOutOB, RevisionNo = Max(BM.RevisionNo),
	                    BM.AddedBy
	                    FROM BM
	                    Inner Join ISG On ISG.SubGroupID = BM.SubGroupID
	                    Left Join {DbNames.EPYSL}..ContactAdditionalInfo CAI On CAI.ContactID = BM.SupplierID
	                    Where ISNULL(CAI.InHouse,0) = 1
	                    Group By BM.BookingNo, BM.ExportOrderID, BM.ExportOrderNo, CAI.InHouse, BM.BuyerID, 
	                    BM.BuyerTeamID, BM.SupplierID, BM.WithOutOB, BM.AddedBy
                    )
                    ,BAck as (
	                    Select BC.BookingID, BC.BookingNo, BC.ExportOrderID, BC.ExportOrderNo, BC.BOMMasterID, BC.AddedBy,
	                    BC.ContactID, BC.BuyerID, BC.BuyerTeamID, BC.ISourcing, BIA.RevisionNo, 
	                    BIA.SubGroupID, BIA.ItemGroupID, BIA.AcknowledgeDate, BIA.WithoutOB
	                    FROM BKList BC
                        Inner Join {DbNames.EPYSL}..BookingItemAcknowledge BIA On BIA.BookingID = BC.BookingID And BIA.WithoutOB = BC.WithOutOB
	                    Group By BC.BookingID, BC.BookingNo, BC.ExportOrderID, BC.ExportOrderNo, BC.BOMMasterID,
	                    BC.ContactID, BC.BuyerID, BC.BuyerTeamID, BC.ISourcing, BIA.RevisionNo, BIA.SubGroupID, 
	                    BIA.ItemGroupID, BIA.AcknowledgeDate, BIA.WithoutOB, BC.AddedBy
                    )
                    ,InHouseItemList as (
	                    SELECT BC.BOMMasterID,BC.ExportOrderID, BC.ExportOrderNo, BC.BookingID, BC.BookingNo, BC.AddedBy,
	                    BC.BuyerID, BC.BuyerTeamID, BC.SubGroupID, BC.ItemGroupID,BC.ContactID,
	                    ISourcing = Max(convert(int,BC.ISourcing)), BC.RevisionNo, AcknowledgeDate = Null, BC.WithOutOB,BKAcknowledgeDate = Max(BC.AcknowledgeDate)
	                    FROM BAck BC
	                    Group By BC.BOMMasterID,BC.ExportOrderID, BC.ExportOrderNo, BC.BookingID, BC.BookingNo, BC.BuyerID, BC.BuyerTeamID, 
	                    BC.SubGroupID, BC.ItemGroupID,BC.ContactID, BC.ISourcing, BC.WithOutOB, BC.RevisionNo, BC.AddedBy
	                    having BC.ISourcing = 1
                    ),
                    ACKList as
                    (
	                    Select OSI.ExportOrderID,OSI.ExportOrderNo,OSI.BOMMasterID,OSI.BookingID,OSI.BookingNo, 
	                    RevisionNo = Max(ISNULL(FBA.RevisionNo,0)), OSI.BuyerID, OSI.BuyerTeamID,
	                    RevStatus = Case When OSI.WithOutOB = 1 then 1 
	                    When Isnull(EL.FabBSCAckStatus,'') = 'Acknowledged' And Isnull(EL.FabBSCAckStatus,'') <> '' Then 1 Else 0 End,
	                    BKAcknowledgeDate = Max(OSI.BKAcknowledgeDate), FBAcknowledgeDate = MAX(FBA.AcknowledgeDate), OSI.WithoutOB,E.EmployeeName
	                    From InHouseItemList OSI
	                    Left Join {DbNames.EPYSL}..ExportWorkOrderLifeCycleChild EL On EL.ExportOrderID = OSI.ExportOrderID And EL.BookingID = OSI.BookingID And EL.ContactID = OSI.ContactID
	                    INNER Join FabricBookingAcknowledge FBA On FBA.BookingID = OSI.BookingID And FBA.SubGroupID = OSI.SubGroupID And FBA.ItemGroupID = OSI.ItemGroupID
	                    Left Join FBookingAcknowledgeChild FBC On FBC.BookingID = OSI.BookingID
	                    LEFT JOIN FBookingAcknowledge FBA1 ON FBA1.BookingID = FBA.BookingID
	                    LEFT JOIN {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = OSI.AddedBy -- FBA1.MerchandiserID
	                    LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = LU.EmployeeCode
                        WHERE FBA.AcknowledgeDate >= '{_startingDate}' AND FBA.RevisionNo = FBA1.PreRevisionNo
	                    Group By OSI.ExportOrderID,OSI.ExportOrderNo,OSI.BOMMasterID,OSI.ExportOrderID, OSI.BuyerID, OSI.BuyerTeamID,Isnull(EL.FabBSCAckStatus,''),
	                    OSI.BookingID,OSI.BookingNo, OSI.WithoutOB,E.EmployeeName,FBA1.MerchandiserID
                    ),       
                    F AS(
	                    SELECT BK.ExportOrderID,BK.ExportOrderNo,BK.BOMMasterID,
                        RevNoValue=COALESCE(NULLIF(CAST(BK.RevisionNo AS VARCHAR(10)) ,'0'), ''),BK.EmployeeName,
	                    BuyerName = ISNULL(CTO.ShortName,''),
	                    BuyerTeamName = ISNULL(CCT.TeamName,''),
	                    BK.RevStatus, 
	                    BK.BKAcknowledgeDate, BK.FBAcknowledgeDate,BK.BookingID,BK.BookingNo, BK.WithoutOB
	                    From ACKList BK
                        LEFT JOIN FBookingAcknowledge FBA ON FBA.BookingID = BK.BookingID
	                    Inner Join {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = BK.BuyerID
	                    Inner Join {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = BK.BuyerTeamID
	                    Group By BK.ExportOrderID,BK.ExportOrderNo,BK.BOMMasterID,BK.RevisionNo, ISNULL(CTO.ShortName,''), ISNULL(CCT.TeamName,''),BK.RevStatus, 
	                    BK.BKAcknowledgeDate, BK.FBAcknowledgeDate,BK.BookingID,BK.BookingNo, BK.WithoutOB,BK.EmployeeName
                    )

                    SELECT * INTO #TempTable{tempGuid} FROM F
                    SELECT *,Count(*) Over() TotalRows FROM #TempTable{tempGuid}
                    ";

                    isNeedImage = true;

                    break;

                case Status.All:
                    sql = this.GetAllList(tempGuid);
                    isNeedImage = true;
                    break;
                default:
                    sql = $@"WITH FBA1 AS
                        (
	                        SELECT a.YBookingID,a.YBookingNo,a.YBookingDate,a.BookingID,a.SubGroupID,
	                        a.Remarks,a.ExportOrderID,a.BuyerID,a.BuyerTeamID,a.CompanyID,
	                        b.BookingNo,b.BookingDate,b.ExportOrderNo,a.WithoutOB
                            FROM YarnBookingMaster_New a
	                        INNER JOIN {DbNames.EPYSL}..BookingMaster b ON b.BookingID = a.BookingID
	                        WHERE a.WithoutOB = 0 AND a.SubGroupID=1 AND a.BookingID NOT IN (SELECT AA.BookingID FROM FBookingAcknowledge AA)
                        ),
                        FBA2 AS
                        (
	                        SELECT a.YBookingID,a.YBookingNo,a.YBookingDate,a.BookingID,a.SubGroupID,
	                        a.Remarks,a.ExportOrderID,a.BuyerID,a.BuyerTeamID,a.CompanyID,
	                        b.BookingNo,b.BookingDate,ExportOrderNo='',a.WithoutOB
	                        FROM YarnBookingMaster_New a
	                        INNER JOIN {DbNames.EPYSL}..SampleBookingMaster b ON b.BookingID = a.BookingID
	                        WHERE a.WithoutOB = 1 AND a.SubGroupID=1 AND a.BookingID NOT IN (SELECT AA.BookingID FROM FBookingAcknowledge AA)
                        ),
                        FBA AS
                        (
	                        SELECT *FROM FBA1
	                        UNION
	                        SELECT *FROM FBA2
                        ),
                        F AS(
                        SELECT FBA.*,CTO.ContactDisplayCode AS BuyerName, CCT.DisplayCode AS BuyerTeamName,CompanyName = C.ShortName
                        FROM FBA
                        LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = FBA.BuyerID
                        LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = FBA.BuyerTeamID
                        LEFT JOIN {DbNames.EPYSL}..CompanyEntity C ON C.CompanyID = FBA.CompanyID)

                        SELECT * INTO #TempTable{tempGuid} FROM F
                        SELECT *,Count(*) Over() TotalRows FROM #TempTable{tempGuid}
                        ";
                    break;
            }
            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            sql += $@" DROP TABLE #TempTable{tempGuid} ";
            var fBookingAcknowledges = await _service.GetDataAsync<FBookingAcknowledge>(sql);

            if (isNeedImage)
            {
                string bookingNos = string.Join("','", fBookingAcknowledges.Select(x => x.BookingNo).Distinct());
                if (bookingNos.IsNotNullOrEmpty())
                {
                    fBookingAcknowledges.Where(x => x.ImagePath.IsNotNullOrEmpty()).ToList().ForEach(x => x.ImagePath = "");
                    fBookingAcknowledges.Where(x => x.ImagePath1.IsNotNullOrEmpty()).ToList().ForEach(x => x.ImagePath1 = "");

                    var fBookingAcknowledgeImages = await _service.GetDataAsync<FBookingAcknowledge>(CommonQueries.GetImagePathQuery(bookingNos, "TP"));
                    fBookingAcknowledgeImages.ForEach(x =>
                    {
                        var obj = fBookingAcknowledges.Find(y => y.BookingNo == x.BookingNo);
                        if (obj.IsNotNull()) fBookingAcknowledges.Find(y => y.BookingNo == x.BookingNo).ImagePath = x.ImagePath;
                    });
                    fBookingAcknowledgeImages = await _service.GetDataAsync<FBookingAcknowledge>(CommonQueries.GetImagePathQuery(bookingNos, "BK"));
                    fBookingAcknowledgeImages.ForEach(x =>
                    {
                        var obj = fBookingAcknowledges.Find(y => y.BookingNo == x.BookingNo);
                        if (obj.IsNotNull()) fBookingAcknowledges.Find(y => y.BookingNo == x.BookingNo).ImagePath1 = x.ImagePath;
                    });
                }
            }
            return fBookingAcknowledges;
        }

        private string GetAllList(string tempGuid)
        {
            string sql = $@"
                        With RunningEWO As
            (
	            Select EOM.ExportOrderID, EOM.ExportOrderNo, EOM.StyleMasterID, EOM.BuyerID, EOM.BuyerTeamID, EOM.EWOStatusID
	            From {DbNames.EPYSL}..ExportOrderMaster EOM
	            Where EOM.EWOStatusID = 130
            ),
            BM As 
            (
	            Select BM.BookingID, BookingNo, BM.ExportOrderID, SupplierID, BM.SubGroupID, WithOutOB = Convert(bit,0),
	            EOM.ExportOrderNo, EOM.StyleMasterID, BM.RevisionNo, EOM.BuyerID, EOM.BuyerTeamID, BM.AddedBy
	            from {DbNames.EPYSL}..BookingMaster BM
	            Inner Join RunningEWO EOM On EOM.ExportOrderID = BM.ExportOrderID
	            INNER join FabricBookingAcknowledge FBAA ON FBAA.BookingID = BM.BookingID
                where BM.IsCancel = 0 And BM.SubGroupID in (1,11,12)
				AND FBAA.AcknowledgeDate >= '{_startingDate}' OR FBAA.UnAcknowledgeDate >= '{_startingDate}'
	            Union All
	            Select BM.BookingID, BookingNo, BM.ExportOrderID, SupplierID, 1 SubGroupID, WithOutOB = Convert(bit,1),
	            EOM.ExportOrderNo, EOM.StyleMasterID, BM.RevisionNo, EOM.BuyerID, EOM.BuyerTeamID, BM.AddedBy
	            from {DbNames.EPYSL}..SampleBookingMaster BM
	            Inner Join RunningEWO EOM On EOM.ExportOrderID = BM.ExportOrderID
	            INNER join FabricBookingAcknowledge FBAA ON FBAA.BookingID = BM.BookingID
                where BM.IsCancel = 0 And BM.ExportOrderID <> 0 and BM.Proposed = 1 And IsCancel = 0
				AND FBAA.AcknowledgeDate >= '{_startingDate}' OR FBAA.UnAcknowledgeDate >= '{_startingDate}'
            ),
            ISG As
            (
	            Select * 
	            from {DbNames.EPYSL}..ItemSubGroup
	            Where SubGroupName in ('Fabric','Collar','Cuff')
            ),
            BKList As
            (
	            Select BM.BookingNo, BM.ExportOrderID, BM.ExportOrderNo, BookingID = Min(BM.BookingID),0 BOMMasterID, ISourcing = CAI.InHouse, BM.BuyerTeamID, ContactID = BM.SupplierID, BM.WithOutOB, RevisionNo = Max(BM.RevisionNo)
	            ,BM.AddedBy,BM.BuyerID
	            FROM BM
	            Inner Join ISG On ISG.SubGroupID = BM.SubGroupID
	            Left Join {DbNames.EPYSL}..ContactAdditionalInfo CAI On CAI.ContactID = BM.SupplierID
	            Where ISNULL(CAI.InHouse,0) = 1
	            Group By BM.BookingNo, BM.ExportOrderID, BM.ExportOrderNo, CAI.InHouse, BM.BuyerTeamID, BM.SupplierID, BM.WithOutOB,BM.AddedBy,BM.BuyerID
            ),BAck as (
	            Select BC.BookingID, BC.BookingNo, BC.ExportOrderID, BC.ExportOrderNo, BC.BOMMasterID,BC.ContactID, BC.BuyerTeamID, BC.ISourcing, BIA.RevisionNo, BIA.SubGroupID, BIA.ItemGroupID, BIA.AcknowledgeDate, BIA.WithoutOB,BC.BuyerID
	            ,BC.AddedBy
	            FROM BKList BC
                Inner Join {DbNames.EPYSL}..BookingItemAcknowledge BIA On BIA.BookingID = BC.BookingID And BIA.WithoutOB = BC.WithOutOB
	            Group By BC.BookingID, BC.BookingNo, BC.ExportOrderID, BC.ExportOrderNo, BC.BOMMasterID,BC.ContactID, BC.BuyerTeamID, BC.ISourcing, BIA.RevisionNo, BIA.SubGroupID, BIA.ItemGroupID, BIA.AcknowledgeDate, BIA.WithoutOB, BC.AddedBy,BC.BuyerID
            ),
			InHouseItemList as (
	            SELECT BC.BOMMasterID,BC.ExportOrderID, BC.ExportOrderNo, BC.BookingID, BC.BookingNo, BC.BuyerTeamID, BC.SubGroupID, BC.ItemGroupID,BC.ContactID, ISourcing = Max(convert(int,BC.ISourcing)), BC.RevisionNo, AcknowledgeDate = Null, BC.WithOutOB,BKAcknowledgeDate = Max(BC.AcknowledgeDate)
	            ,BC.AddedBy, BC.BuyerID
	            FROM BAck BC
	            Group By BC.BOMMasterID,BC.ExportOrderID, BC.ExportOrderNo, BC.BookingID, BC.BookingNo, BC.BuyerTeamID, BC.SubGroupID, BC.ItemGroupID,BC.ContactID, BC.ISourcing, BC.WithOutOB, BC.RevisionNo,BC.AddedBy, BC.BuyerID
	            having BC.ISourcing = 1
            ),
            ACKListPendingList as
            (
                Select OSI.ExportOrderID,OSI.ExportOrderNo,OSI.BOMMasterID,OSI.BookingNo,
	            RevisionNo = Max(ISNULL(FBA.RevisionNo,0)), OSI.BuyerTeamID,
	            StatusText = 'Pending for Fabric Booking Acknowledge',
	            BKAcknowledgeDate = Max(OSI.BKAcknowledgeDate), FBAcknowledgeDate = MAX(FBA.AcknowledgeDate), OSI.WithoutOB,E.EmployeeName
	            ,OSI.AddedBy, OSI.BuyerID
                From InHouseItemList OSI
	            Left Join {DbNames.EPYSL}..ExportWorkOrderLifeCycleChild EL On EL.ExportOrderID = OSI.ExportOrderID And EL.BookingID = OSI.BookingID And EL.ContactID = OSI.ContactID
                INNER Join FabricBookingAcknowledge FBA On FBA.BookingID = OSI.BookingID  And FBA.SubGroupID = OSI.SubGroupID And FBA.ItemGroupID = OSI.ItemGroupID
	            LEFT JOIN {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = OSI.AddedBy
	            LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = LU.EmployeeCode
                Where (FBA.AcknowledgeDate >= '{_startingDate}' OR FBA.UnAcknowledgeDate >= '{_startingDate}') AND
					Case When FBA.AcknowledgeID IS NULL Then 0
					            When FBA.AcknowledgeID IS NOT NULL And ISNULL(FBA.PreProcessRevNo,0) < ISNULL(OSI.RevisionNo,0) Then 2 Else 1 End 
			            = Case When 'N'='N' Then 0
					            When 'N'='N' Then 2 Else 1 End
		            Group By OSI.ExportOrderID,OSI.ExportOrderNo,OSI.BOMMasterID,OSI.ExportOrderID,ISNULL(FBA.RevisionNo,0), OSI.BuyerTeamID,Isnull(EL.FabBSCAckStatus,''),
		            OSI.BookingNo, OSI.WithoutOB, OSI.AddedBy, OSI.BuyerID,E.EmployeeName
            ),
            ACKListRevisionList as
            (
                Select OSI.ExportOrderID,OSI.ExportOrderNo,OSI.BOMMasterID,OSI.BookingNo,
	            RevisionNo = Max(ISNULL(FBA.RevisionNo,0)), OSI.BuyerTeamID,
	            StatusText = 'Pending for Revision Acknowledge',
	            BKAcknowledgeDate = Max(OSI.BKAcknowledgeDate), FBAcknowledgeDate = MAX(FBA.AcknowledgeDate), OSI.WithoutOB,E.EmployeeName
	            ,OSI.AddedBy, OSI.BuyerID
                From InHouseItemList OSI
	            Left Join {DbNames.EPYSL}..ExportWorkOrderLifeCycleChild EL On EL.ExportOrderID = OSI.ExportOrderID And EL.BookingID = OSI.BookingID And EL.ContactID = OSI.ContactID
                INNER Join FabricBookingAcknowledge FBA On FBA.BookingID = OSI.BookingID  And FBA.SubGroupID = OSI.SubGroupID And FBA.ItemGroupID = OSI.ItemGroupID
	            LEFT JOIN {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = OSI.AddedBy
	            LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = LU.EmployeeCode
                Where Case When FBA.AcknowledgeID IS NULL Then 0
					            When FBA.AcknowledgeID IS NOT NULL And ISNULL(FBA.PreProcessRevNo,0) < ISNULL(OSI.RevisionNo,0) Then 2 Else 1 End 
			            = Case When 'R'='N' Then 0
					            When 'R'='R' Then 2 Else 1 End
		            Group By OSI.ExportOrderID,OSI.ExportOrderNo,OSI.BOMMasterID,OSI.ExportOrderID,ISNULL(FBA.RevisionNo,0), OSI.BuyerTeamID,Isnull(EL.FabBSCAckStatus,''),
		            OSI.BookingNo, OSI.WithoutOB,OSI.AddedBy, OSI.BuyerID,E.EmployeeName
            ),
            PendingMrkingACKList as
            (
	            Select OSI.ExportOrderID,OSI.ExportOrderNo,OSI.BOMMasterID,OSI.BookingNo,
	            RevisionNo = Max(ISNULL(FBA.RevisionNo,0)), OSI.BuyerTeamID,
	            StatusText = 'Pending for Marketing Acknowledge',
	            BKAcknowledgeDate = Max(OSI.BKAcknowledgeDate), FBAcknowledgeDate = MAX(FBA.AcknowledgeDate), OSI.WithoutOB,E.EmployeeName
	            ,OSI.AddedBy, OSI.BuyerID
	            From InHouseItemList OSI
	            Left Join {DbNames.EPYSL}..ExportWorkOrderLifeCycleChild EL On EL.ExportOrderID = OSI.ExportOrderID And EL.BookingID = OSI.BookingID And EL.ContactID = OSI.ContactID
	            Inner Join FabricBookingAcknowledge FBA On FBA.BookingID = OSI.BookingID And FBA.SubGroupID = OSI.SubGroupID And FBA.ItemGroupID = OSI.ItemGroupID
	            Left Join FBookingAcknowledgeChild FBC On FBC.BookingID = OSI.BookingID
	            LEFT JOIN FBookingAcknowledge FBA1 ON FBA1.BookingID = FBA.BookingID
	            LEFT JOIN {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = OSI.AddedBy
	            LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = LU.EmployeeCode
                Where (FBA.AcknowledgeDate >= '{_startingDate}' OR FBA.UnAcknowledgeDate >= '{_startingDate}') AND IsNull(FBC.SendToMktAck,0) = 1 And IsNull(FBC.IsMktAck,0) = 0 And IsNull(FBC.IsMktUnAck,0) = 0
                AND FBA.RevisionNo = FBA1.PreRevisionNo
	            Group By OSI.ExportOrderID,OSI.ExportOrderNo,OSI.BOMMasterID,OSI.ExportOrderID, OSI.BuyerID, OSI.BuyerTeamID,Isnull(EL.FabBSCAckStatus,''),
	            OSI.BookingNo, OSI.WithoutOB,E.EmployeeName,FBA1.MerchandiserID,OSI.AddedBy, OSI.BuyerID
            ),
            UnACKList as
            (
                Select OSI.ExportOrderID,OSI.ExportOrderNo,OSI.BOMMasterID,OSI.BookingNo,
	            RevisionNo = Max(ISNULL(FBA.RevisionNo,0)), OSI.BuyerTeamID,
	            StatusText = 'Unacknowledged',
	            BKAcknowledgeDate = Max(OSI.BKAcknowledgeDate), FBAcknowledgeDate = MAX(FBA.AcknowledgeDate), OSI.WithoutOB,E.EmployeeName
	            ,OSI.AddedBy, OSI.BuyerID

                From InHouseItemList OSI
                Left Join {DbNames.EPYSL}..ExportWorkOrderLifeCycleChild EL On EL.ExportOrderID = OSI.ExportOrderID And EL.BookingID = OSI.BookingID And EL.ContactID = OSI.ContactID
                Inner Join FabricBookingAcknowledge FBA On FBA.BookingID = OSI.BookingID And FBA.SubGroupID = OSI.SubGroupID And FBA.ItemGroupID = OSI.ItemGroupID
                Left Join FBookingAcknowledgeChild FBC On FBC.BookingID = OSI.BookingID
                LEFT JOIN FBookingAcknowledge FBA1 ON FBA1.BookingID = FBA.BookingID
	            LEFT JOIN {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = OSI.AddedBy
	            LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = LU.EmployeeCode
	            LEFT JOIN {DbNames.EPYSL}..LoginUser LU1 ON LU1.UserCode = FBA.UnAcknowledgeBy
	            LEFT JOIN {DbNames.EPYSL}..Employee E1 ON E1.EmployeeCode = LU1.EmployeeCode
	            WHERE (FBA.AcknowledgeDate >= '{_startingDate}' OR FBA.UnAcknowledgeDate >= '{_startingDate}') AND FBA.PreProcessRevNo = OSI.RevisionNo AND FBA.UnAcknowledge = 1 Or ( IsNull(FBC.IsTxtUnAck,0)=1 Or  ( IsNull(FBC.IsMktUnAck,0)=1 And IsNull(FBC.SendToTxtAck,0)=1 ))
                Group By OSI.ExportOrderID,OSI.ExportOrderNo,OSI.BOMMasterID,OSI.ExportOrderID, OSI.BuyerID, OSI.BuyerTeamID,Isnull(EL.FabBSCAckStatus,''),
	            OSI.BookingNo, OSI.WithoutOB,E.EmployeeName,FBA1.MerchandiserID,OSI.AddedBy, OSI.BuyerID
            ),
            MainACKList as
            (
                Select OSI.ExportOrderID,OSI.ExportOrderNo,OSI.BOMMasterID,OSI.BookingNo,
	            RevisionNo = Max(ISNULL(FBA.RevisionNo,0)), OSI.BuyerTeamID,
	            StatusText = 'Acknowledged',
	            BKAcknowledgeDate = Max(OSI.BKAcknowledgeDate), FBAcknowledgeDate = MAX(FBA.AcknowledgeDate), OSI.WithoutOB,E.EmployeeName
	            ,OSI.AddedBy, OSI.BuyerID
                From InHouseItemList OSI
	            Left Join {DbNames.EPYSL}..ExportWorkOrderLifeCycleChild EL On EL.ExportOrderID = OSI.ExportOrderID And EL.BookingID = OSI.BookingID And EL.ContactID = OSI.ContactID
                Inner Join FabricBookingAcknowledge FBA On FBA.BookingID = OSI.BookingID And FBA.SubGroupID = OSI.SubGroupID And FBA.ItemGroupID = OSI.ItemGroupID
                Left Join FBookingAcknowledgeChild FBC On FBC.BookingID = OSI.BookingID
                LEFT JOIN FBookingAcknowledge FBA1 ON FBA1.BookingID = FBA.BookingID
	            LEFT JOIN {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = OSI.AddedBy -- FBA1.MerchandiserID
	            LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = LU.EmployeeCode
	            Where (FBA.AcknowledgeDate >= '{_startingDate}' OR FBA.UnAcknowledgeDate >= '{_startingDate}') AND   
                (IsNull(FBC.IsTxtAck,0)=1) And 
				(IsNull(FBC.IsMktUnAck,0)=0) AND
				FBA1.IsUnAcknowledge=0  
                Group By OSI.ExportOrderID,OSI.ExportOrderNo,OSI.BOMMasterID,OSI.ExportOrderID, OSI.BuyerID, OSI.BuyerTeamID,Isnull(EL.FabBSCAckStatus,''),
	            OSI.BookingNo, OSI.WithoutOB,E.EmployeeName,FBA1.MerchandiserID,OSI.AddedBy, OSI.BuyerID
            ),
			RunningEWO_WithCancel As
            (
	            Select EOM.ExportOrderID, EOM.ExportOrderNo, EOM.StyleMasterID, EOM.BuyerID, EOM.BuyerTeamID, EOM.EWOStatusID
	            From {DbNames.EPYSL}..ExportOrderMaster EOM
	            Where EOM.EWOStatusID IN (130,131)
            ),
			BM_WithCancel As 
            (
	            Select BM.BookingID, BookingNo, BM.ExportOrderID, SupplierID, BM.SubGroupID, WithOutOB = Convert(bit,0),
	            EOM.ExportOrderNo, EOM.StyleMasterID, BM.RevisionNo, EOM.BuyerID, EOM.BuyerTeamID, BM.AddedBy
	            from {DbNames.EPYSL}..BookingMaster BM
	            Inner Join RunningEWO_WithCancel EOM On EOM.ExportOrderID = BM.ExportOrderID
	            INNER join FabricBookingAcknowledge FBAA ON FBAA.BookingID = BM.BookingID
                where (FBAA.AcknowledgeDate >= '{_startingDate}' OR FBAA.UnAcknowledgeDate >= '{_startingDate}') AND (BM.IsCancel = 1 OR EOM.EWOStatusID = 131) And BM.SubGroupID in (1,11,12)
	            Union All
	            Select BM.BookingID, BookingNo, BM.ExportOrderID, SupplierID, 1 SubGroupID, WithOutOB = Convert(bit,1),
	            EOM.ExportOrderNo, EOM.StyleMasterID, BM.RevisionNo, EOM.BuyerID, EOM.BuyerTeamID, BM.AddedBy
	            from {DbNames.EPYSL}..SampleBookingMaster BM
	            Inner Join RunningEWO_WithCancel EOM On EOM.ExportOrderID = BM.ExportOrderID
	            INNER join FabricBookingAcknowledge FBAA ON FBAA.BookingID = BM.BookingID
                where (FBAA.AcknowledgeDate >= '{_startingDate}' OR FBAA.UnAcknowledgeDate >= '{_startingDate}') AND (BM.IsCancel = 1 OR EOM.EWOStatusID = 131) And BM.ExportOrderID <> 0 and BM.Proposed = 1 And IsCancel = 0/**/
            ),
			BKList_WithCancel As
            (
	            Select BM.BookingNo, BM.ExportOrderID, BM.ExportOrderNo, BookingID = Min(BM.BookingID),0 BOMMasterID, ISourcing = CAI.InHouse, BM.BuyerTeamID, ContactID = BM.SupplierID, BM.WithOutOB, RevisionNo = Max(BM.RevisionNo)
	            ,BM.AddedBy,BM.BuyerID
	            FROM BM_WithCancel BM
	            Inner Join ISG On ISG.SubGroupID = BM.SubGroupID
	            Left Join {DbNames.EPYSL}..ContactAdditionalInfo CAI On CAI.ContactID = BM.SupplierID
	            Where ISNULL(CAI.InHouse,0) = 1
	            Group By BM.BookingNo, BM.ExportOrderID, BM.ExportOrderNo, CAI.InHouse, BM.BuyerTeamID, BM.SupplierID, BM.WithOutOB,BM.AddedBy,BM.BuyerID
            ),
			BAck_WithCancel as (
	            Select BC.BookingID, BC.BookingNo, BC.ExportOrderID, BC.ExportOrderNo, BC.BOMMasterID,BC.ContactID, BC.BuyerTeamID, BC.ISourcing, BIA.RevisionNo, BIA.SubGroupID, BIA.ItemGroupID, BIA.AcknowledgeDate, BIA.WithoutOB,BC.BuyerID
	            ,BC.AddedBy
	            FROM BKList_WithCancel BC
                Inner Join {DbNames.EPYSL}..BookingItemAcknowledge BIA On BIA.BookingID = BC.BookingID And BIA.WithoutOB = BC.WithOutOB
	            Group By BC.BookingID, BC.BookingNo, BC.ExportOrderID, BC.ExportOrderNo, BC.BOMMasterID,BC.ContactID, BC.BuyerTeamID, BC.ISourcing, BIA.RevisionNo, BIA.SubGroupID, BIA.ItemGroupID, BIA.AcknowledgeDate, BIA.WithoutOB, BC.AddedBy,BC.BuyerID
            ),
			InHouseItemList_WithCancel as (
	            SELECT BC.BOMMasterID,BC.ExportOrderID, BC.ExportOrderNo, BC.BookingID, BC.BookingNo, BC.BuyerTeamID, BC.SubGroupID, BC.ItemGroupID,BC.ContactID, ISourcing = Max(convert(int,BC.ISourcing)), BC.RevisionNo, AcknowledgeDate = Null, BC.WithOutOB,BKAcknowledgeDate = Max(BC.AcknowledgeDate)
	            ,BC.AddedBy, BC.BuyerID
	            FROM BAck_WithCancel BC
	            Group By BC.BOMMasterID,BC.ExportOrderID, BC.ExportOrderNo, BC.BookingID, BC.BookingNo, BC.BuyerTeamID, BC.SubGroupID, BC.ItemGroupID,BC.ContactID, BC.ISourcing, BC.WithOutOB, BC.RevisionNo,BC.AddedBy, BC.BuyerID
	            having BC.ISourcing = 1
            ),
            MMCancelList as
            (
	            Select OSI.ExportOrderID,OSI.ExportOrderNo,OSI.BOMMasterID,OSI.BookingNo,
	            RevisionNo = Max(ISNULL(FBA.RevisionNo,0)), OSI.BuyerTeamID,
	            StatusText = 'M&M Cancel',
	            BKAcknowledgeDate = Max(OSI.BKAcknowledgeDate), FBAcknowledgeDate = MAX(FBA.AcknowledgeDate), OSI.WithoutOB,E.EmployeeName
	            ,OSI.AddedBy, OSI.BuyerID
	            From InHouseItemList_WithCancel OSI
	            Left Join {DbNames.EPYSL}..ExportWorkOrderLifeCycleChild EL On EL.ExportOrderID = OSI.ExportOrderID And EL.BookingID = OSI.BookingID And EL.ContactID = OSI.ContactID
	            Inner Join FabricBookingAcknowledge FBA On FBA.BookingID = OSI.BookingID And FBA.SubGroupID = OSI.SubGroupID And FBA.ItemGroupID = OSI.ItemGroupID
	            Left Join FBookingAcknowledgeChild FBC On FBC.BookingID = OSI.BookingID
	            LEFT JOIN FBookingAcknowledge FBA1 ON FBA1.BookingID = FBA.BookingID
	            LEFT JOIN {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = OSI.AddedBy -- FBA1.MerchandiserID
	            LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = LU.EmployeeCode
                WHERE (FBA.AcknowledgeDate >= '{_startingDate}' OR FBA.UnAcknowledgeDate >= '{_startingDate}') AND FBA.RevisionNo = FBA1.PreRevisionNo
	            Group By OSI.ExportOrderID,OSI.ExportOrderNo,OSI.BOMMasterID,OSI.ExportOrderID, OSI.BuyerID, OSI.BuyerTeamID,Isnull(EL.FabBSCAckStatus,''),
	            OSI.BookingNo, OSI.WithoutOB,E.EmployeeName,FBA1.MerchandiserID,OSI.AddedBy, OSI.BuyerID
            ),
            AllList AS
            (
	            SELECT * FROM ACKListPendingList
	            UNION
	            SELECT * FROM ACKListRevisionList
	            UNION
	            SELECT * FROM PendingMrkingACKList
				UNION
	            SELECT * FROM MainACKList
	            UNION
	            SELECT * FROM UnACKList
	            UNION
	            SELECT * FROM MMCancelList
            ),
            FinalList AS
            (
                SELECT BK.ExportOrderID,BK.ExportOrderNo,BK.BOMMasterID,BK.RevisionNo, CCT.TeamName BuyerTeamName, CTO.ShortName BuyerName,
	            BK.StatusText, 
	            BK.BKAcknowledgeDate, BK.FBAcknowledgeDate,BK.BookingNo, BK.WithoutOB,E.EmployeeName
	            From AllList BK
	            LEFT Join {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = BK.BuyerID
	            LEFT Join {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = BK.BuyerTeamID
				LEFT JOIN {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = BK.AddedBy
				LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = LU.EmployeeCode
				Group By BK.ExportOrderID,BK.ExportOrderNo,BK.BOMMasterID,BK.RevisionNo, CCT.TeamName, CTO.ShortName,
	            BK.StatusText,BK.BKAcknowledgeDate, BK.FBAcknowledgeDate,BK.BookingNo, BK.WithoutOB,E.EmployeeName
            )
            SELECT * INTO #TempTable{tempGuid} FROM FinalList
            SELECT *,Count(*) Over() TotalRows FROM #TempTable{tempGuid}";

            return sql;
        }

        public async Task<CountListItem> GetListCount()
        {
            string query = $@"
                With RunningEWO As
                (
	                Select EOM.ExportOrderID, EOM.ExportOrderNo, EOM.StyleMasterID, EOM.BuyerID, EOM.BuyerTeamID
	                From {DbNames.EPYSL}..ExportOrderMaster EOM
	                Where EOM.EWOStatusID = 130
                ),
				RunningEWOWithCancel As
                (
	                Select EOM.ExportOrderID, EOM.ExportOrderNo, EOM.StyleMasterID, EOM.BuyerID, EOM.BuyerTeamID, EOM.EWOStatusID
	                From {DbNames.EPYSL}..ExportOrderMaster EOM
	                Where EOM.EWOStatusID IN (130, 131)
                ),
                ISG As
                (
	                Select * 
	                from {DbNames.EPYSL}..ItemSubGroup
	                Where SubGroupName in ('Fabric','Collar','Cuff')
                ),
                BM22 As 
                (
	                Select BM.BookingID, BM.BookingNo, BM.ExportOrderID, BM.SupplierID, BM.SubGroupID, WithOutOB = Convert(bit,0),
	                EOM.ExportOrderNo, EOM.StyleMasterID, BM.RevisionNo, EOM.BuyerID, EOM.BuyerTeamID, AddedBy = ISNULL(BM.UpdatedBy, BM.AddedBy)
	                from {DbNames.EPYSL}..BookingMaster BM
	                Inner Join RunningEWO EOM On EOM.ExportOrderID = BM.ExportOrderID
                    Inner Join {DbNames.EPYSL}..BookingItemAcknowledge Ack on ACK.BookingID = BM.BookingID
	                LEFT JOIN {DbNames.EPYSL}..BookingChildImage BCI ON BCI.BookingID = BM.BookingID
	                where BM.IsCancel = 0 And BM.SubGroupID in (1,11,12) AND ACK.UnAcknowledge = 0 AND BM.Proposed = 1
                    AND BM.BookingDate >= '{_startingDate}'
                    
	                Union All

	                Select BM.BookingID, BM.BookingNo, BM.ExportOrderID, SupplierID, 1 SubGroupID, WithOutOB = Convert(bit,1),
	                EOM.ExportOrderNo, EOM.StyleMasterID, BM.RevisionNo, EOM.BuyerID, EOM.BuyerTeamID, AddedBy = ISNULL(BM.UpdatedBy, BM.AddedBy)
	                from {DbNames.EPYSL}..SampleBookingMaster BM
	                Inner Join RunningEWO EOM On EOM.ExportOrderID = BM.ExportOrderID
	                LEFT JOIN {DbNames.EPYSL}..SampleBookingChildImage BCI ON BCI.BookingID = BM.BookingID
	                where BM.IsCancel = 0 And BM.ExportOrderID <> 0 and BM.Proposed = 1 And IsCancel = 0 AND BM.UnAcknowledge=0
                    AND BM.BookingDate >= '{_startingDate}'
                ),
                BKList22 As
                (
	                Select BM.BookingNo, BM.ExportOrderID, BM.ExportOrderNo, BookingID = Min(BM.BookingID),0 BOMMasterID, ISourcing = CAI.InHouse, BM.BuyerTeamID, ContactID = BM.SupplierID, BM.WithOutOB, RevisionNo = Max(BM.RevisionNo)
	                , BM.BuyerID, AddedBy = MIN(BM.AddedBy)
	                FROM BM22 BM
	                Inner Join ISG On ISG.SubGroupID = BM.SubGroupID
	                Left Join {DbNames.EPYSL}..ContactAdditionalInfo CAI On CAI.ContactID = BM.SupplierID
	                Where ISNULL(CAI.InHouse,0) = 1
	                Group By BM.BookingNo, BM.ExportOrderID, BM.ExportOrderNo, CAI.InHouse, BM.BuyerTeamID, BM.SupplierID, BM.WithOutOB, BM.BuyerID, BM.AddedBy
                ),BAck22 as (
	                Select BC.BookingID, BC.BookingNo, BC.ExportOrderID, BC.ExportOrderNo, BC.BOMMasterID,BC.ContactID, BC.BuyerTeamID, BC.ISourcing, BIA.RevisionNo, 
	                SubGroupID = MIN(BIA.SubGroupID),
	                ItemGroupID = MIN(BIA.ItemGroupID),
	                BIA.AcknowledgeDate
	                ,BC.BuyerID, BC.AddedBy,BC.WithOutOB
	                FROM BKList22 BC
	                Inner Join {DbNames.EPYSL}..BookingItemAcknowledge BIA On BIA.BookingID = BC.BookingID And BIA.WithoutOB = BC.WithOutOB
	                Group By BC.BookingID, BC.BookingNo, BC.ExportOrderID, BC.ExportOrderNo, BC.BOMMasterID,BC.ContactID, BC.BuyerTeamID, BC.ISourcing, BIA.RevisionNo, BIA.AcknowledgeDate,BC.WithOutOB
	                ,BC.BuyerID, BC.AddedBy
                ),InHouseItemList22 as (
	                SELECT BC.BOMMasterID,BC.ExportOrderID, BC.ExportOrderNo, BC.BookingID, BC.BookingNo, BC.BuyerTeamID, BC.SubGroupID, BC.ItemGroupID,BC.ContactID, ISourcing = Max(convert(int,BC.ISourcing)), BC.RevisionNo, AcknowledgeDate = Null, BC.WithOutOB,BKAcknowledgeDate = Max(BC.AcknowledgeDate)
	                ,BC.BuyerID, BC.AddedBy
	                FROM BAck22 BC
	                Group By BC.BOMMasterID,BC.ExportOrderID, BC.ExportOrderNo, BC.BookingID, BC.BookingNo, BC.BuyerTeamID, BC.SubGroupID, BC.ItemGroupID,BC.ContactID, BC.ISourcing, BC.WithOutOB, BC.RevisionNo
	                ,BC.BuyerID, BC.AddedBy
	                having BC.ISourcing = 1
                ),
                PrevAckList AS(
                    Select OSI.BookingID, IsPrevAck = Case When FBAT.FBAckID is null then 0 Else 1 End 
                    FROM InHouseItemList22 OSI
                    INNER JOIN FBookingAcknowledge FBAT ON FBAT.BookingNo = OSI.BookingNo
                ),
                ACKList22 as
                (
                    Select OSI.ExportOrderID,OSI.ExportOrderNo,OSI.BOMMasterID,OSI.BookingNo, 
	                RevisionNo = ISNULL(FBA.RevisionNo,0), OSI.BuyerTeamID,
	                RevStatus = Case When OSI.WithOutOB = 1 then 1 
                                        When Isnull(EL.FabBSCAckStatus,'') = 'Acknowledged' And Isnull(EL.FabBSCAckStatus,'') <> '' Then 1 Else 0 End,
	                BKAcknowledgeDate = Max(OSI.BKAcknowledgeDate), FBAcknowledgeDate = Max(FBA.AcknowledgeDate), OSI.WithoutOB
                    From InHouseItemList22 OSI
	                Inner Join {DbNames.EPYSL}..BookingItemAcknowledge Ack on ACK.BookingID = OSI.BookingID
	                Left Join {DbNames.EPYSL}..ExportWorkOrderLifeCycleChild EL On EL.ExportOrderID = OSI.ExportOrderID And EL.BookingID = OSI.BookingID And EL.ContactID = OSI.ContactID
                    Left Join FabricBookingAcknowledge FBA On FBA.BookingID = OSI.BookingID  And FBA.SubGroupID = OSI.SubGroupID And FBA.ItemGroupID = OSI.ItemGroupID
	                Left Join PrevAckList PAL ON PAL.BookingID = OSI.BookingID
                   Where (Case When FBA.AcknowledgeID IS NULL Then 0
					                When FBA.AcknowledgeID IS NOT NULL And ISNULL(FBA.PreProcessRevNo,0) < ISNULL(OSI.RevisionNo,0) Then 2 Else 1 End 
			                = Case When 'N'='N' Then 0
					                When 'N'='N' Then 2 Else 1 End) AND ISNULL(PAL.IsPrevAck,0) = 0
		                Group By OSI.ExportOrderID,OSI.ExportOrderNo,OSI.BOMMasterID,OSI.ExportOrderID,ISNULL(FBA.RevisionNo,0), OSI.BuyerTeamID,Isnull(EL.FabBSCAckStatus,''),
		                OSI.BookingNo, OSI.WithoutOB,OSI.BuyerID,OSI.AddedBy
                ),
                F22 AS
                (
	                SELECT BK.ExportOrderID,BK.ExportOrderNo,BK.BOMMasterID,BK.RevisionNo, CCT.TeamName BuyerTeamName,BK.RevStatus, 
	                BK.BKAcknowledgeDate, BK.FBAcknowledgeDate,BK.BookingNo, BK.WithoutOB
	                From ACKList22 BK
	                Inner Join {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = BK.BuyerTeamID
	                Group By BK.ExportOrderID,BK.ExportOrderNo,BK.BOMMasterID,BK.RevisionNo, CCT.TeamName,BK.RevStatus, 
	                BK.BKAcknowledgeDate, BK.FBAcknowledgeDate,BK.BookingNo, BK.WithoutOB
                ),     
                NewList AS(
	                SELECT BK.BookingNo
	                From F22 BK
                ),
                BM33 As 
                (
	                Select BookingID, BookingNo, BM.ExportOrderID, SupplierID, SubGroupID, WithOutOB = Convert(bit,0),
	                EOM.ExportOrderNo, EOM.StyleMasterID, BM.RevisionNo, EOM.BuyerID, EOM.BuyerTeamID, AddedBy = ISNULL(BM.UpdatedBy, BM.AddedBy) 
	                from {DbNames.EPYSL}..BookingMaster BM
	                Inner Join RunningEWO EOM On EOM.ExportOrderID = BM.ExportOrderID
                    where BM.IsCancel = 0 And BM.SubGroupID in (1,11,12)
                    AND BM.BookingDate >= '{_startingDate}'

	                Union All

	                Select BookingID, BookingNo, BM.ExportOrderID, SupplierID, 1 SubGroupID, WithOutOB = Convert(bit,1),
	                EOM.ExportOrderNo, EOM.StyleMasterID, BM.RevisionNo, EOM.BuyerID, EOM.BuyerTeamID, AddedBy = ISNULL(BM.UpdatedBy, BM.AddedBy) 
	                from {DbNames.EPYSL}..SampleBookingMaster BM
	                Inner Join RunningEWO EOM On EOM.ExportOrderID = BM.ExportOrderID
                    where BM.IsCancel = 0 And BM.ExportOrderID <> 0 and BM.Proposed = 1 And IsCancel = 0
                    AND BM.BookingDate >= '{_startingDate}'
                ),
                BKList33 As
                (
	                Select BM.BookingNo, BM.ExportOrderID, BM.ExportOrderNo, BookingID = Min(BM.BookingID),0 BOMMasterID, ISourcing = CAI.InHouse, BM.BuyerTeamID, ContactID = BM.SupplierID, BM.WithOutOB, RevisionNo = Max(BM.RevisionNo)
	                ,BM.BuyerID, AddedBy = MIN(BM.AddedBy)
	                FROM BM33 BM
	                Inner Join ISG On ISG.SubGroupID = BM.SubGroupID
	                Left Join {DbNames.EPYSL}..ContactAdditionalInfo CAI On CAI.ContactID = BM.SupplierID
	                Where ISNULL(CAI.InHouse,0) = 1
	                Group By BM.BookingNo, BM.ExportOrderID, BM.ExportOrderNo, CAI.InHouse, BM.BuyerTeamID, BM.SupplierID, BM.WithOutOB ,BM.BuyerID
                ),BAck33 as (
	                Select BC.BookingID, BC.BookingNo, BC.ExportOrderID, BC.ExportOrderNo, BC.BOMMasterID,BC.ContactID, BC.BuyerTeamID, BC.ISourcing, 
					RevisionNo = Case When BC.WithOutOB = 0  Then BIA.RevisionNo Else BC.RevisionNo END,
					BIA.SubGroupID, BIA.ItemGroupID, BIA.AcknowledgeDate, BIA.WithoutOB
	                ,BC.BuyerID, BC.AddedBy
	                FROM BKList33 BC
                    Inner Join {DbNames.EPYSL}..BookingItemAcknowledge BIA On BIA.BookingID = BC.BookingID And BIA.WithoutOB = BC.WithOutOB
	                Group By BC.BookingID, BC.BookingNo, BC.ExportOrderID, BC.ExportOrderNo, BC.BOMMasterID,BC.ContactID, BC.BuyerTeamID, BC.ISourcing, BIA.SubGroupID, BIA.ItemGroupID, BIA.AcknowledgeDate, BIA.WithoutOB
	                ,BC.BuyerID, BC.AddedBy,Case When BC.WithOutOB = 0  Then BIA.RevisionNo Else BC.RevisionNo END
                ),
                InHouseItemList33 as (
	                SELECT BC.BOMMasterID,BC.ExportOrderID, BC.ExportOrderNo, BC.BookingID, BC.BookingNo, BC.BuyerTeamID, BC.SubGroupID, BC.ItemGroupID,BC.ContactID, ISourcing = Max(convert(int,BC.ISourcing)), BC.RevisionNo, AcknowledgeDate = Null, BC.WithOutOB,BKAcknowledgeDate = Max(BC.AcknowledgeDate)
	                ,BC.BuyerID, BC.AddedBy
	                FROM BAck33 BC
	                Group By BC.BOMMasterID,BC.ExportOrderID, BC.ExportOrderNo, BC.BookingID, BC.BookingNo, BC.BuyerTeamID, BC.SubGroupID, BC.ItemGroupID,BC.ContactID, BC.ISourcing, BC.WithOutOB, BC.RevisionNo
	                ,BC.BuyerID, BC.AddedBy
	                having BC.ISourcing = 1
                ),
                ACKList33 as
                (
                    Select OSI.ExportOrderID,OSI.ExportOrderNo,OSI.BOMMasterID,OSI.BookingNo, 
	                RevNoValue = MAX(ISNULL(FB.RevisionNo,0)), OSI.BuyerTeamID,
	                RevStatus = Case When OSI.WithOutOB = 1 then 1 
                                        When Isnull(EL.FabBSCAckStatus,'') = 'Acknowledged' And Isnull(EL.FabBSCAckStatus,'') <> '' Then 1 Else 0 End,
	                BKAcknowledgeDate = Max(OSI.BKAcknowledgeDate), FBAcknowledgeDate = Max(FBA.AcknowledgeDate), OSI.WithoutOB
                    ,OSI.BuyerID, OSI.AddedBy
	                From InHouseItemList33 OSI
	                Left Join {DbNames.EPYSL}..ExportWorkOrderLifeCycleChild EL On EL.ExportOrderID = OSI.ExportOrderID And EL.BookingID = OSI.BookingID And EL.ContactID = OSI.ContactID
                    Left Join EPYSLTEX..FabricBookingAcknowledge FBA On FBA.BookingID = OSI.BookingID  And FBA.SubGroupID = OSI.SubGroupID And FBA.ItemGroupID = OSI.ItemGroupID
	                Left Join EPYSLTEX..FBookingAcknowledge FB On FB.BookingID = OSI.BookingID  And FB.SubGroupID = OSI.SubGroupID And FB.ItemGroupID = OSI.ItemGroupID
                    Left Join EPYSLTEX..FBookingAcknowledge FB2 On FB2.BookingNo = OSI.BookingNo                    
                    Where --FBA.UnAcknowledge = 0 AND 
                           Case When FB2.FBAckID IS NOT NULL AND FBA.PreProcessRevNo IS NULL AND OSI.RevisionNo IS NOT NULL Then 2
                        When FBA.AcknowledgeID IS NULL Then 0
					    When FBA.AcknowledgeID IS NOT NULL And ISNULL(FBA.PreProcessRevNo,0) < ISNULL(OSI.RevisionNo,0) Then 2 Else 1 End 
			            = Case When 'R'='N' Then 0
					        When 'R'='R' Then 2 Else 1 End
		                Group By OSI.ExportOrderID,OSI.ExportOrderNo,OSI.BOMMasterID,OSI.ExportOrderID, OSI.BuyerTeamID,Isnull(EL.FabBSCAckStatus,''),
		                OSI.BookingNo, OSI.WithoutOB,OSI.BuyerID, OSI.AddedBy
                ),
                F33 AS
                (
	                SELECT BK.ExportOrderID,BK.ExportOrderNo,BK.BOMMasterID,BK.RevNoValue, CCT.TeamName BuyerTeamName, CTO.ShortName BuyerName,BK.RevStatus, 
	                BK.BKAcknowledgeDate, BK.FBAcknowledgeDate,BK.BookingNo, BK.WithoutOB,E.EmployeeName
	                From ACKList33 BK
	                LEFT Join {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = BK.BuyerID
	                LEFT Join {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = BK.BuyerTeamID
	                LEFT JOIN {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = BK.AddedBy
	                LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = LU.EmployeeCode
	                Group By BK.ExportOrderID,BK.ExportOrderNo,BK.BOMMasterID,BK.RevNoValue, CCT.TeamName,BK.RevStatus, 
	                BK.BKAcknowledgeDate, BK.FBAcknowledgeDate,BK.BookingNo, BK.WithoutOB,CTO.ShortName,E.EmployeeName
                ),
                RevisionList AS(
                    SELECT BK.BookingNo
                    From F33 BK
                ),
                BM2 As 
                (
	                Select BM.BookingID, BM.BookingNo, BM.ExportOrderID, BM.SupplierID, BM.SubGroupID, WithOutOB = Convert(bit,0),
	                EOM.ExportOrderNo, EOM.StyleMasterID, BM.RevisionNo, BM.BuyerID, BM.BuyerTeamID,ImagePath = (Select Top 1 ImagePath From {DbNames.EPYSL}..BookingChildImage Where BookingID=BM.BookingID)
	                from {DbNames.EPYSL}..BookingMaster BM
	                Inner Join RunningEWO EOM On EOM.ExportOrderID = BM.ExportOrderID
	                left join FBookingAcknowledge FBA ON FBA.BookingID = BM.BookingID
	                where BM.IsCancel = 0 And BM.SubGroupID in (1,11,12)
                    AND BM.BookingDate >= '{_startingDate}'

	                Union All

	                Select BM.BookingID, BM.BookingNo, BM.ExportOrderID, BM.SupplierID, 1 SubGroupID, WithOutOB = Convert(bit,1),
	                EOM.ExportOrderNo, EOM.StyleMasterID, BM.RevisionNo, BM.BuyerID, BM.BuyerTeamID,ImagePath = (Select Top 1 ImagePath From {DbNames.EPYSL}..SampleBookingChildImage Where BookingID=BM.BookingID)
	                from {DbNames.EPYSL}..SampleBookingMaster BM
	                Inner Join RunningEWO EOM On EOM.ExportOrderID = BM.ExportOrderID
	                left join FBookingAcknowledge FBA ON FBA.BookingID = BM.BookingID
	                where BM.IsCancel = 0 And BM.ExportOrderID <> 0 and BM.Proposed = 1 
                    AND BM.BookingDate >= '{_startingDate}'
                ),
                BKList2 As
                (
	                Select BM.BookingNo, BM.ExportOrderID, BM.ExportOrderNo, BookingID = Min(BM.BookingID),0 BOMMasterID, ISourcing = CAI.InHouse, BM.BuyerID, BM.BuyerTeamID, ContactID = BM.SupplierID, BM.WithOutOB, RevisionNo = Max(BM.RevisionNo)
	                FROM BM2 BM
	                Inner Join ISG On ISG.SubGroupID = BM.SubGroupID
	                Left Join {DbNames.EPYSL}..ContactAdditionalInfo CAI On CAI.ContactID = BM.SupplierID
	                Where ISNULL(CAI.InHouse,0) = 1
	                Group By BM.BookingNo, BM.ExportOrderID, BM.ExportOrderNo, CAI.InHouse, BM.BuyerID, BM.BuyerTeamID, BM.SupplierID, BM.WithOutOB
                ),BAck2 as (
	                Select BC.BookingID, BC.BookingNo, BC.ExportOrderID, BC.ExportOrderNo, BC.BOMMasterID,BC.ContactID, BC.BuyerID, BC.BuyerTeamID, BC.ISourcing, BIA.RevisionNo, BIA.SubGroupID, BIA.ItemGroupID, BIA.AcknowledgeDate, BIA.WithoutOB,ImagePath=Case When BIA.WithoutOB=1 Then (Select Top 1 ImagePath From {DbNames.EPYSL}..SampleBookingChildImage Where BookingID=BC.BookingID) Else (Select Top 1 ImagePath From {DbNames.EPYSL}..BookingChildImage Where BookingID=BC.BookingID) End
	                FROM BKList2 BC
                    Inner Join {DbNames.EPYSL}..BookingItemAcknowledge BIA On BIA.BookingID = BC.BookingID And BIA.WithoutOB = BC.WithOutOB
	                Group By BC.BookingID, BC.BookingNo, BC.ExportOrderID, BC.ExportOrderNo, BC.BOMMasterID,BC.ContactID, BC.BuyerID, BC.BuyerTeamID, BC.ISourcing, BIA.RevisionNo, BIA.SubGroupID, BIA.ItemGroupID, BIA.AcknowledgeDate, BIA.WithoutOB
                ),InHouseItemList2 as (
	                SELECT BC.BOMMasterID,BC.ExportOrderID, BC.ExportOrderNo, BC.BookingID, BC.BookingNo, BC.BuyerID, BC.BuyerTeamID, BC.SubGroupID, BC.ItemGroupID,BC.ContactID, ISourcing = Max(convert(int,BC.ISourcing)), BC.RevisionNo, AcknowledgeDate = Null, BC.WithOutOB,BKAcknowledgeDate = Max(BC.AcknowledgeDate),BC.ImagePath
	                FROM BAck2 BC
	                Group By BC.BOMMasterID,BC.ExportOrderID, BC.ExportOrderNo, BC.BookingID, BC.BookingNo, BC.BuyerID, BC.BuyerTeamID, BC.SubGroupID, BC.ItemGroupID,BC.ContactID, BC.ISourcing, BC.WithOutOB, BC.RevisionNo,BC.ImagePath
	                having BC.ISourcing = 1
                ),
                ACKList2 as
                (
	                Select OSI.ExportOrderID,OSI.ExportOrderNo,OSI.BOMMasterID,OSI.BookingID,OSI.BookingNo, 
	                RevisionNo = Max(ISNULL(FBA.RevisionNo,0)), OSI.BuyerID, OSI.BuyerTeamID,
	                RevStatus = Case When OSI.WithOutOB = 1 then 1 
	                When Isnull(EL.FabBSCAckStatus,'') = 'Acknowledged' And Isnull(EL.FabBSCAckStatus,'') <> '' Then 1 Else 0 End,
	                BKAcknowledgeDate = Max(OSI.BKAcknowledgeDate), FBAcknowledgeDate = Max(FBA.AcknowledgeDate), OSI.WithoutOB,OSI.ImagePath,E.EmployeeName
	                From InHouseItemList2 OSI
	                Left Join {DbNames.EPYSL}..ExportWorkOrderLifeCycleChild EL On EL.ExportOrderID = OSI.ExportOrderID And EL.BookingID = OSI.BookingID And EL.ContactID = OSI.ContactID
	                Inner Join FabricBookingAcknowledge FBA On FBA.BookingID = OSI.BookingID And FBA.SubGroupID = OSI.SubGroupID And FBA.ItemGroupID = OSI.ItemGroupID
	                Left Join FBookingAcknowledgeChild FBC On FBC.BookingID = OSI.BookingID
	                LEFT JOIN FBookingAcknowledge FBA1 ON FBA1.BookingID = FBA.BookingID
	                LEFT JOIN {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = FBA1.MerchandiserID
	                LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = LU.EmployeeCode
                    Where FBA.AcknowledgeDate >= '{_startingDate}' --AND IsNull(FBC.SendToMktAck,0)=1 And IsNull(FBC.IsMktAck,0)=0 And IsNull(FBC.IsMktUnAck,0)=0
                    AND FBA.RevisionNo = FBA1.PreRevisionNo
	                Group By OSI.ExportOrderID,OSI.ExportOrderNo,OSI.BOMMasterID,OSI.ExportOrderID, OSI.BuyerID, OSI.BuyerTeamID,Isnull(EL.FabBSCAckStatus,''),
	                OSI.BookingID,OSI.BookingNo, OSI.WithoutOB,OSI.ImagePath,E.EmployeeName,FBA1.MerchandiserID
                ),
                FBC AS
	            (
		            SELECT AL.BookingNo
		            FROM ACKList2 AL
		            INNER JOIN FBookingAcknowledge FBA ON FBA.BookingID = AL.BookingID
		            INNER JOIN FBookingAcknowledgeChild FBC ON FBC.AcknowledgeID = FBA.FBAckID
		            WHERE IsNull(FBC.SendToMktAck,0)=1 And IsNull(FBC.IsMktAck,0)=0 And IsNull(FBC.IsMktUnAck,0)=0
		            GROUP BY AL.BookingNo
	            ),
                PendingMKTList AS(
	                SELECT BK.BookingNo
	                From ACKList2 BK
                    INNER JOIN FBC ON FBC.BookingNo = BK.BookingNo
	                Inner Join {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = BK.BuyerID
	                Inner Join {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = BK.BuyerTeamID
	                Group By BK.ExportOrderID,BK.ExportOrderNo,BK.BOMMasterID,BK.RevisionNo, ISNULL(CTO.ShortName,''), ISNULL(CCT.TeamName,''),BK.RevStatus, 
	                BK.BKAcknowledgeDate, BK.FBAcknowledgeDate,BK.BookingID,BK.BookingNo, BK.WithoutOB,BK.ImagePath,BK.EmployeeName
                ),
                BM3 As 
                (
	                Select BM.BookingID, BM.BookingNo, BM.ExportOrderID, BM.SupplierID, BM.SubGroupID, WithOutOB = Convert(bit,0),
	                EOM.ExportOrderNo, EOM.StyleMasterID, RevisionNo = BM.PreProcessRevNo, EOM.BuyerID, EOM.BuyerTeamID, AddedBy = ISNULL(BM.UpdatedBy, BM.AddedBy)
	                from {DbNames.EPYSL}..BookingMaster BM
	                Inner Join RunningEWO EOM On EOM.ExportOrderID = BM.ExportOrderID
	                left join FBookingAcknowledge FBA ON FBA.BookingID = BM.BookingID
	                where BM.IsCancel = 0 And BM.SubGroupID in (1,11,12) AND FBA.FBAckID IS NOT NULL
                    AND BM.BookingDate >= '{_startingDate}'

	                Union All

	                Select BM.BookingID, BM.BookingNo, BM.ExportOrderID, BM.SupplierID, 1 SubGroupID, WithOutOB = Convert(bit,1),
	                EOM.ExportOrderNo, EOM.StyleMasterID, RevisionNo = BM.RevisionNo, EOM.BuyerID, EOM.BuyerTeamID, AddedBy = ISNULL(BM.UpdatedBy, BM.AddedBy)
	                from {DbNames.EPYSL}..SampleBookingMaster BM
	                Inner Join RunningEWO EOM On EOM.ExportOrderID = BM.ExportOrderID
	                left join FBookingAcknowledge FBA ON FBA.BookingID = BM.BookingID
	                where BM.IsCancel = 0 And BM.ExportOrderID <> 0 AND FBA.FBAckID IS NOT NULL --AND BM.Proposed = 1
                    AND BM.BookingDate >= '{_startingDate}'
                ),
                BKList3 As
                (
	                Select BM.BookingNo, BM.ExportOrderID, BM.ExportOrderNo, BookingID = Min(BM.BookingID),0 BOMMasterID, 
	                ISourcing = CAI.InHouse, BM.BuyerID, BM.BuyerTeamID, ContactID = BM.SupplierID, BM.AddedBy,
	                BM.WithOutOB, RevisionNo = Max(BM.RevisionNo)
	                FROM BM3 BM
	                Inner Join ISG On ISG.SubGroupID = BM.SubGroupID
	                Left Join {DbNames.EPYSL}..ContactAdditionalInfo CAI On CAI.ContactID = BM.SupplierID
	                Where ISNULL(CAI.InHouse,0) = 1
	                Group By BM.BookingNo, BM.ExportOrderID, BM.ExportOrderNo, CAI.InHouse, BM.BuyerID, 
	                BM.BuyerTeamID, BM.SupplierID, BM.WithOutOB, BM.AddedBy
                )
                ,BAck3 as (
	                Select BC.BookingID, BC.BookingNo, BC.ExportOrderID, BC.ExportOrderNo, BC.BOMMasterID,
	                BC.ContactID, BC.BuyerID, BC.BuyerTeamID, BC.ISourcing, BC.RevisionNo, BIA.SubGroupID, 
	                BIA.ItemGroupID, BIA.AcknowledgeDate, BIA.WithoutOB, BC.AddedBy
	                FROM BKList3 BC
                    Inner Join {DbNames.EPYSL}..BookingItemAcknowledge BIA On BIA.BookingID = BC.BookingID And BIA.WithoutOB = BC.WithOutOB
	                Group By BC.BookingID, BC.BookingNo, BC.ExportOrderID, BC.ExportOrderNo, BC.BOMMasterID,
	                BC.ContactID, BC.BuyerID, BC.BuyerTeamID, BC.ISourcing, BC.RevisionNo, BIA.SubGroupID, 
	                BIA.ItemGroupID, BIA.AcknowledgeDate, BIA.WithoutOB, BC.AddedBy
                ),InHouseItemList3 as (
	                SELECT BC.BOMMasterID,BC.ExportOrderID, BC.ExportOrderNo, BC.BookingID, BC.BookingNo, 
	                BC.BuyerID, BC.BuyerTeamID, BC.SubGroupID, BC.ItemGroupID,BC.ContactID, BC.AddedBy,
	                ISourcing = Max(convert(int,BC.ISourcing)), BC.RevisionNo, AcknowledgeDate = Null, 
	                BC.WithOutOB,BKAcknowledgeDate = Max(BC.AcknowledgeDate)
	                FROM BAck3 BC
	                Group By BC.BOMMasterID,BC.ExportOrderID, BC.ExportOrderNo, BC.BookingID, BC.BookingNo, BC.BuyerID, 
	                BC.BuyerTeamID, BC.SubGroupID, BC.ItemGroupID,BC.ContactID, BC.ISourcing, BC.WithOutOB, BC.RevisionNo,
	                BC.AddedBy
	                having BC.ISourcing = 1
                ),
                ACKList3 as
                (
                    Select OSI.ExportOrderID,OSI.ExportOrderNo,OSI.BOMMasterID,OSI.BookingID,OSI.BookingNo, 
	                RevisionNo = OSI.RevisionNo, OSI.BuyerID, OSI.BuyerTeamID,
	                RevStatus = Case When OSI.WithOutOB = 1 then 1 
                                        When Isnull(EL.FabBSCAckStatus,'') = 'Acknowledged' And Isnull(EL.FabBSCAckStatus,'') <> '' Then 1 Else 0 End,
	                BKAcknowledgeDate = Max(OSI.BKAcknowledgeDate), OSI.WithoutOB,E.EmployeeName, FBAcknowledgeDate = MAX(FBA.AcknowledgeDate)
                    From InHouseItemList3 OSI
	                Left Join {DbNames.EPYSL}..ExportWorkOrderLifeCycleChild EL On EL.ExportOrderID = OSI.ExportOrderID And EL.BookingID = OSI.BookingID And EL.ContactID = OSI.ContactID
                    Inner Join FabricBookingAcknowledge FBA On FBA.BookingID = OSI.BookingID And FBA.SubGroupID = OSI.SubGroupID And FBA.ItemGroupID = OSI.ItemGroupID
                    Left Join FBookingAcknowledgeChild FBC On FBC.BookingID = OSI.BookingID
                    LEFT JOIN FBookingAcknowledge FBA1 ON FBA1.BookingID = FBA.BookingID
	                LEFT JOIN {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = OSI.AddedBy -- FBA1.MerchandiserID
	                LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = LU.EmployeeCode
	                WHERE FBA.AcknowledgeDate >= '{_startingDate}' AND
	                --FBA.PreProcessRevNo = OSI.RevisionNo AND 
	                (IsNull(FBC.IsTxtAck,0)=1) And IsNull(FBC.IsMktUnAck,0)=0 
	                --AND FBA1.IsUnAcknowledge=0               
                    Group By OSI.ExportOrderID,OSI.ExportOrderNo,OSI.BOMMasterID,OSI.ExportOrderID, OSI.BuyerID, OSI.BuyerTeamID,Isnull(EL.FabBSCAckStatus,''),
	                OSI.BookingID,OSI.BookingNo, OSI.WithoutOB,E.EmployeeName,FBA1.MerchandiserID, OSI.RevisionNo
                ),       
                AcknowledgeList AS(
	                SELECT BK.ExportOrderID,BK.ExportOrderNo,BK.BOMMasterID,
                    RevNoValue = FBA.RevisionNo, --COALESCE(NULLIF(CAST(BK.RevisionNo AS VARCHAR(10)) ,'0'), ''),
                    BK.EmployeeName,
	                BuyerName = ISNULL(CTO.ShortName,''),
	                BuyerTeamName = ISNULL(CCT.TeamName,''),
	                BK.RevStatus, 
	                BK.BKAcknowledgeDate, BK.FBAcknowledgeDate,BK.BookingID,BK.BookingNo, 
	                BK.WithoutOB, PMCApproveBy = E.EmployeeName,
	                AcknowledgeDate = FBA.DateAdded,
	                AcknowledgeByName = E1.EmployeeName,
	                BK.RevisionNo,FBA.PreRevisionNo,
                    IsRevisionValid = CASE WHEN BK.RevisionNo = FBA.PreRevisionNo THEN 1 ELSE 0 END
	                From FBookingAcknowledge FBA
                    INNER JOIN FabricBookingAcknowledge FBA1 ON FBA1.BookingID = FBA.BookingID AND FBA1.SubGroupID = FBA.SubGroupID
                    LEFT JOIN ACKList3 BK ON BK.BookingID = FBA.BookingID
                    LEFT JOIN {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = FBA.ApprovedByPMC
	                LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = LU.EmployeeCode 
	                Inner Join {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = BK.BuyerID
	                Inner Join {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = BK.BuyerTeamID
	                LEFT JOIN {DbNames.EPYSL}..LoginUser LU1 ON LU1.UserCode = FBA1.AddedBy
	                LEFT JOIN {DbNames.EPYSL}..Employee E1 ON E1.EmployeeCode = LU1.EmployeeCode
                    WHERE FBA1.AcknowledgeDate >= '{_startingDate}'
                    --WHERE BK.RevisionNo = FBA.PreRevisionNo
	                Group By BK.ExportOrderID,BK.ExportOrderNo,BK.BOMMasterID,BK.RevisionNo,ISNULL(CTO.ShortName,''),ISNULL(CCT.TeamName,''),BK.RevStatus, 
	                BK.BKAcknowledgeDate, BK.BookingID,BK.BookingNo, BK.WithoutOB,BK.EmployeeName, E.EmployeeName, BK.FBAcknowledgeDate,
	                FBA.DateAdded,E1.EmployeeName,FBA.PreRevisionNo,FBA.RevisionNo
                ),
                BM4 As 
                (
                    Select BM.BookingID, BM.BookingNo, BM.ExportOrderID, BM.SupplierID, BM.SubGroupID, WithOutOB = Convert(bit,0),
	                EOM.ExportOrderNo, EOM.StyleMasterID, BM.RevisionNo, EOM.BuyerID, EOM.BuyerTeamID,FBA.IsUnAcknowledge, AddedBy = ISNULL(BM.UpdatedBy, BM.AddedBy)
	                from {DbNames.EPYSL}..BookingMaster BM
	                Inner Join RunningEWO EOM On EOM.ExportOrderID = BM.ExportOrderID
	                left join FabricBookingAcknowledge FBAA ON FBAA.BookingID = BM.BookingID
	                left join FBookingAcknowledge FBA ON FBA.BookingID = BM.BookingID
                    LEFT JOIN {DbNames.EPYSL}..BookingChildImage BCI ON BCI.BookingID = BM.BookingID
	                where BM.IsCancel = 0 And BM.SubGroupID in (1,11,12)
                    AND BM.BookingDate >= '{_startingDate}'
                    
	                Union All

	                Select BM.BookingID, BM.BookingNo, BM.ExportOrderID, BM.SupplierID, 1 SubGroupID, WithOutOB = Convert(bit,1),
	                EOM.ExportOrderNo, EOM.StyleMasterID, BM.RevisionNo, EOM.BuyerID, EOM.BuyerTeamID,FBA.IsUnAcknowledge, AddedBy = ISNULL(BM.UpdatedBy, BM.AddedBy)
	                from {DbNames.EPYSL}..SampleBookingMaster BM
	                Inner Join RunningEWO EOM On EOM.ExportOrderID = BM.ExportOrderID
	                left join FabricBookingAcknowledge FBAA ON FBAA.BookingID = BM.BookingID
	                left join FBookingAcknowledge FBA ON FBA.BookingID = BM.BookingID
                    LEFT JOIN {DbNames.EPYSL}..SampleBookingChildImage BCI ON BCI.BookingID = BM.BookingID
	                where BM.IsCancel = 0 And BM.ExportOrderID <> 0 and BM.Proposed = 1 
                    AND BM.BookingDate >= '{_startingDate}'
                ),
                BKList4 As
                (
	                Select BM.BookingNo,UnAcknowledge=Max(Convert(int,BM.IsUnAcknowledge)), BM.ExportOrderID, BM.ExportOrderNo, 
	                BookingID = Min(BM.BookingID),0 BOMMasterID, ISourcing = CAI.InHouse, BM.BuyerID, BM.BuyerTeamID, 
	                ContactID = BM.SupplierID, BM.WithOutOB, RevisionNo = Max(BM.RevisionNo),AddedBy = MIN(BM.AddedBy)
	                FROM BM4 BM
	                Inner Join ISG On ISG.SubGroupID = BM.SubGroupID
	                Left Join {DbNames.EPYSL}..ContactAdditionalInfo CAI On CAI.ContactID = BM.SupplierID
	                Where ISNULL(CAI.InHouse,0) = 1
	                Group By BM.BookingNo, BM.ExportOrderID, BM.ExportOrderNo, CAI.InHouse, BM.BuyerID, BM.BuyerTeamID, BM.SupplierID, BM.WithOutOB
                ),BAck4 as (
	                Select BC.BookingID,BC.UnAcknowledge, BC.BookingNo, BC.ExportOrderID, BC.ExportOrderNo, 
	                BC.BOMMasterID,BC.ContactID, BC.BuyerID, BC.BuyerTeamID, BC.ISourcing, BIA.RevisionNo, BIA.SubGroupID, 
	                BIA.ItemGroupID, BIA.AcknowledgeDate, BIA.WithoutOB, BC.AddedBy
	                FROM BKList4 BC
                    Inner Join {DbNames.EPYSL}..BookingItemAcknowledge BIA On BIA.BookingID = BC.BookingID And BIA.WithoutOB = BC.WithOutOB
	                Group By BC.BookingID,BC.UnAcknowledge, BC.BookingNo, BC.ExportOrderID, BC.ExportOrderNo, 
	                BC.BOMMasterID,BC.ContactID, BC.BuyerID, BC.BuyerTeamID, BC.ISourcing, BIA.RevisionNo, BIA.SubGroupID, 
	                BIA.ItemGroupID, BIA.AcknowledgeDate, BIA.WithoutOB, BC.AddedBy
                ),InHouseItemList4 as (
	                SELECT BC.BOMMasterID,BC.UnAcknowledge,BC.ExportOrderID, BC.ExportOrderNo, BC.BookingID, BC.AddedBy, 
	                BC.BookingNo, BC.BuyerID, BC.BuyerTeamID, BC.SubGroupID, BC.ItemGroupID,BC.ContactID, 
	                ISourcing = Max(convert(int,BC.ISourcing)), BC.RevisionNo, AcknowledgeDate = Null, BC.WithOutOB,BKAcknowledgeDate = Max(BC.AcknowledgeDate)
	                FROM BAck4 BC
	                Group By BC.BOMMasterID,BC.UnAcknowledge,BC.ExportOrderID, BC.ExportOrderNo, BC.BookingID, 
	                BC.BookingNo, BC.BuyerID, BC.BuyerTeamID, BC.SubGroupID, BC.ItemGroupID,BC.ContactID, 
	                BC.ISourcing, BC.WithOutOB, BC.RevisionNo, BC.AddedBy
	                having BC.ISourcing = 1
                ),
                ACKList4 as
                (
                    Select OSI.ExportOrderID,OSI.ExportOrderNo,OSI.BOMMasterID,OSI.BookingID,OSI.BookingNo, 
                    RevisionNo = Max(ISNULL(FBA.RevisionNo,0)), OSI.BuyerID, OSI.BuyerTeamID,
                    RevStatus = Case When OSI.WithOutOB = 1 then 1 
                    When Isnull(EL.FabBSCAckStatus,'') = 'Acknowledged' And Isnull(EL.FabBSCAckStatus,'') <> '' Then 1 Else 0 End,
                    BKAcknowledgeDate = Max(OSI.BKAcknowledgeDate), 
	                FBAcknowledgeDate = MAX(FBA.AcknowledgeDate), 
	                OSI.WithoutOB,
	                E.EmployeeName,

	                FBA1.UnAcknowledgeDate,
	                UnAcknowledgeByName = E1.EmployeeName,
	                FBA1.UnAcknowledgeReason

                    From InHouseItemList4 OSI
                    Left Join {DbNames.EPYSL}..ExportWorkOrderLifeCycleChild EL On EL.ExportOrderID = OSI.ExportOrderID And EL.BookingID = OSI.BookingID And EL.ContactID = OSI.ContactID
                    Inner Join FabricBookingAcknowledge FBA On FBA.BookingID = OSI.BookingID And FBA.SubGroupID = OSI.SubGroupID And FBA.ItemGroupID = OSI.ItemGroupID
                    Left Join FBookingAcknowledgeChild FBC On FBC.BookingID = OSI.BookingID
                    LEFT JOIN FBookingAcknowledge FBA1 ON FBA1.BookingID = FBA.BookingID
	                LEFT JOIN {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = OSI.AddedBy
	                LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = LU.EmployeeCode
	                LEFT JOIN {DbNames.EPYSL}..LoginUser LU1 ON LU1.UserCode = FBA.UnAcknowledgeBy
	                LEFT JOIN {DbNames.EPYSL}..Employee E1 ON E1.EmployeeCode = LU1.EmployeeCode
	                Where Cast(FBA1.UnAcknowledgeDate As Date)>= CAST('{_startingDate}' AS DATE) 
                    --AND FBA.PreProcessRevNo = OSI.RevisionNo 
                    AND OSI.UnAcknowledge = 1 Or ( IsNull(FBC.IsTxtUnAck,0)=1 Or  ( IsNull(FBC.IsMktUnAck,0)=1 And IsNull(FBC.SendToTxtAck,0)=1 ))
                    Group By OSI.ExportOrderID,OSI.ExportOrderNo,OSI.BOMMasterID,OSI.ExportOrderID, OSI.BuyerID, OSI.BuyerTeamID,Isnull(EL.FabBSCAckStatus,''),
                    OSI.BookingID,OSI.BookingNo, OSI.WithoutOB,FBA1.MerchandiserID,E.EmployeeName, FBA1.UnAcknowledgeDate, E1.EmployeeName, FBA1.UnAcknowledgeReason
                ),       
                UnAcknowledgeList AS(
                    SELECT BK.ExportOrderID,BK.ExportOrderNo,BK.BOMMasterID,
                    RevNoValue=COALESCE(NULLIF(CAST(BK.RevisionNo AS VARCHAR(10)) ,'0'), ''),BK.EmployeeName,
                    BuyerName = ISNULL(CTO.ShortName,''),
	                BuyerTeamName = ISNULL(CCT.TeamName,''),
                    BK.RevStatus, 
                    BK.BKAcknowledgeDate, BK.FBAcknowledgeDate,BK.BookingID,BK.BookingNo, 
	                BK.WithoutOB,
	                BK.UnAcknowledgeDate, BK.UnAcknowledgeByName, BK.UnAcknowledgeReason
                    From ACKList4 BK
                    --INNER JOIN FBookingAcknowledge FBA ON FBA.BookingID = BK.BookingID
                    Inner Join {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = BK.BuyerID
                    Inner Join {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = BK.BuyerTeamID
                    --WHERE BK.RevisionNo = FBA.PreRevisionNo
                    Group By BK.ExportOrderID,BK.ExportOrderNo,BK.BOMMasterID,BK.RevisionNo, ISNULL(CTO.ShortName,''), ISNULL(CCT.TeamName,''),BK.RevStatus, 
                    BK.BKAcknowledgeDate, BK.FBAcknowledgeDate,BK.BookingID,BK.BookingNo, BK.WithoutOB,BK.EmployeeName,
	                BK.UnAcknowledgeDate, BK.UnAcknowledgeByName, BK.UnAcknowledgeReason
                ),       

                BM5 As 
                (
	                Select BM.BookingID, BM.BookingNo, BM.ExportOrderID, BM.SupplierID, BM.SubGroupID, WithOutOB = Convert(bit,0),
	                EOM.ExportOrderNo, EOM.StyleMasterID, BM.RevisionNo, BM.BuyerID, BM.BuyerTeamID,ImagePath = (Select Top 1 ImagePath From {DbNames.EPYSL}..BookingChildImage Where BookingID=BM.BookingID)
	                from {DbNames.EPYSL}..BookingMaster BM
	                Inner Join RunningEWOWithCancel EOM On EOM.ExportOrderID = BM.ExportOrderID
	                left join FBookingAcknowledge FBA ON FBA.BookingID = BM.BookingID
	                where (BM.IsCancel = 1 OR EOM.EWOStatusID = 131) And BM.SubGroupID in (1,11,12)
                    AND BM.BookingDate >= '{_startingDate}'

	                Union All

	                Select BM.BookingID, BM.BookingNo, BM.ExportOrderID, BM.SupplierID, 1 SubGroupID, WithOutOB = Convert(bit,1),
	                EOM.ExportOrderNo, EOM.StyleMasterID, BM.RevisionNo, BM.BuyerID, BM.BuyerTeamID,ImagePath = (Select Top 1 ImagePath From {DbNames.EPYSL}..SampleBookingChildImage Where BookingID=BM.BookingID)
	                from {DbNames.EPYSL}..SampleBookingMaster BM
	                Inner Join RunningEWOWithCancel EOM On EOM.ExportOrderID = BM.ExportOrderID
	                left join FBookingAcknowledge FBA ON FBA.BookingID = BM.BookingID
	                where (BM.IsCancel = 1 OR EOM.EWOStatusID = 131) And BM.ExportOrderID <> 0 and BM.Proposed = 1 
                    AND BM.BookingDate >= '{_startingDate}'
                ),
                BKList5 As
                (
	                Select BM.BookingNo, BM.ExportOrderID, BM.ExportOrderNo, BookingID = Min(BM.BookingID),0 BOMMasterID, ISourcing = CAI.InHouse, BM.BuyerID, BM.BuyerTeamID, ContactID = BM.SupplierID, BM.WithOutOB, RevisionNo = Max(BM.RevisionNo)
	                FROM BM5 BM
	                Inner Join ISG On ISG.SubGroupID = BM.SubGroupID
	                Left Join {DbNames.EPYSL}..ContactAdditionalInfo CAI On CAI.ContactID = BM.SupplierID
	                Where ISNULL(CAI.InHouse,0) = 1
	                Group By BM.BookingNo, BM.ExportOrderID, BM.ExportOrderNo, CAI.InHouse, BM.BuyerID, BM.BuyerTeamID, BM.SupplierID, BM.WithOutOB
                ),BAck5 as (
	                Select BC.BookingID, BC.BookingNo, BC.ExportOrderID, BC.ExportOrderNo, BC.BOMMasterID,BC.ContactID, BC.BuyerID, BC.BuyerTeamID, BC.ISourcing, BIA.RevisionNo, BIA.SubGroupID, BIA.ItemGroupID, BIA.AcknowledgeDate, BIA.WithoutOB,ImagePath=Case When BIA.WithoutOB=1 Then (Select Top 1 ImagePath From {DbNames.EPYSL}..SampleBookingChildImage Where BookingID=BC.BookingID) Else (Select Top 1 ImagePath From {DbNames.EPYSL}..BookingChildImage Where BookingID=BC.BookingID) End
	                FROM BKList5 BC
	                Inner Join {DbNames.EPYSL}..BookingItemAcknowledge BIA On BIA.BookingID = BC.BookingID And BIA.WithoutOB = BC.WithOutOB
	                Group By BC.BookingID, BC.BookingNo, BC.ExportOrderID, BC.ExportOrderNo, BC.BOMMasterID,BC.ContactID, BC.BuyerID, BC.BuyerTeamID, BC.ISourcing, BIA.RevisionNo, BIA.SubGroupID, BIA.ItemGroupID, BIA.AcknowledgeDate, BIA.WithoutOB
                ),InHouseItemList5 as (
	                SELECT BC.BOMMasterID,BC.ExportOrderID, BC.ExportOrderNo, BC.BookingID, BC.BookingNo, BC.BuyerID, BC.BuyerTeamID, BC.SubGroupID, BC.ItemGroupID,BC.ContactID, ISourcing = Max(convert(int,BC.ISourcing)), BC.RevisionNo, AcknowledgeDate = Null, BC.WithOutOB,BKAcknowledgeDate = Max(BC.AcknowledgeDate),BC.ImagePath
	                FROM BAck5 BC
	                Group By BC.BOMMasterID,BC.ExportOrderID, BC.ExportOrderNo, BC.BookingID, BC.BookingNo, BC.BuyerID, BC.BuyerTeamID, BC.SubGroupID, BC.ItemGroupID,BC.ContactID, BC.ISourcing, BC.WithOutOB, BC.RevisionNo,BC.ImagePath
	                having BC.ISourcing = 1
                ),
                ACKList5 as
                (
	                Select OSI.ExportOrderID,OSI.ExportOrderNo,OSI.BOMMasterID,OSI.BookingID,OSI.BookingNo, 
	                RevisionNo = Max(ISNULL(FBA.RevisionNo,0)), OSI.BuyerID, OSI.BuyerTeamID,
	                RevStatus = Case When OSI.WithOutOB = 1 then 1 
	                When Isnull(EL.FabBSCAckStatus,'') = 'Acknowledged' And Isnull(EL.FabBSCAckStatus,'') <> '' Then 1 Else 0 End,
	                BKAcknowledgeDate = Max(OSI.BKAcknowledgeDate), FBAcknowledgeDate = Max(FBA.AcknowledgeDate), OSI.WithoutOB,OSI.ImagePath,E.EmployeeName
	                From InHouseItemList5 OSI
	                Left Join {DbNames.EPYSL}..ExportWorkOrderLifeCycleChild EL On EL.ExportOrderID = OSI.ExportOrderID And EL.BookingID = OSI.BookingID And EL.ContactID = OSI.ContactID
	                Left Join FabricBookingAcknowledge FBA On FBA.BookingID = OSI.BookingID And FBA.SubGroupID = OSI.SubGroupID And FBA.ItemGroupID = OSI.ItemGroupID
	                Left Join FBookingAcknowledgeChild FBC On FBC.BookingID = OSI.BookingID
	                LEFT JOIN FBookingAcknowledge FBA1 ON FBA1.BookingID = FBA.BookingID
	                LEFT JOIN {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = FBA1.MerchandiserID
	                LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = LU.EmployeeCode
	                WHERE FBA.AcknowledgeDate >= '{_startingDate}' AND FBA.RevisionNo = FBA1.PreRevisionNo
	                Group By OSI.ExportOrderID,OSI.ExportOrderNo,OSI.BOMMasterID,OSI.ExportOrderID, OSI.BuyerID, OSI.BuyerTeamID,Isnull(EL.FabBSCAckStatus,''),
	                OSI.BookingID,OSI.BookingNo, OSI.WithoutOB,OSI.ImagePath,E.EmployeeName,FBA1.MerchandiserID
                ),       
                CancelList AS(
	                SELECT BK.ExportOrderID,BK.ExportOrderNo,BK.BOMMasterID,
	                RevNoValue=COALESCE(NULLIF(CAST(BK.RevisionNo AS VARCHAR(10)) ,'0'), ''),BK.EmployeeName,
	                BuyerName = ISNULL(CTO.ShortName,''),
	                BuyerTeamName = ISNULL(CCT.TeamName,''),
	                BK.RevStatus, 
	                BK.BKAcknowledgeDate, BK.FBAcknowledgeDate,BK.BookingID,BookingNo, BK.WithoutOB,BK.ImagePath
	                From ACKList5 BK
	                Inner Join {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = BK.BuyerID
	                Inner Join {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = BK.BuyerTeamID
	                Group By BK.ExportOrderID,BK.ExportOrderNo,BK.BOMMasterID,BK.RevisionNo, ISNULL(CTO.ShortName,''), ISNULL(CCT.TeamName,''),BK.RevStatus, 
	                BK.BKAcknowledgeDate, BK.FBAcknowledgeDate,BK.BookingID,BK.BookingNo, BK.WithoutOB,BK.ImagePath,BK.EmployeeName
                ),


                BM As 
                (
	                Select BM.BookingID, BookingNo, BM.ExportOrderID, SupplierID, BM.SubGroupID, WithOutOB = Convert(bit,0),
	                EOM.ExportOrderNo, EOM.StyleMasterID, BM.RevisionNo, EOM.BuyerID, EOM.BuyerTeamID, BM.AddedBy
	                from {DbNames.EPYSL}..BookingMaster BM
	                Inner Join RunningEWO EOM On EOM.ExportOrderID = BM.ExportOrderID
	                INNER join FabricBookingAcknowledge FBAA ON FBAA.BookingID = BM.BookingID
                    where BM.IsCancel = 0 And BM.SubGroupID in (1,11,12)
	                AND FBAA.AcknowledgeDate >= '{_startingDate}' OR FBAA.UnAcknowledgeDate >= '{_startingDate}'
	                Union All
	                Select BM.BookingID, BookingNo, BM.ExportOrderID, SupplierID, 1 SubGroupID, WithOutOB = Convert(bit,1),
	                EOM.ExportOrderNo, EOM.StyleMasterID, BM.RevisionNo, EOM.BuyerID, EOM.BuyerTeamID, BM.AddedBy
	                from {DbNames.EPYSL}..SampleBookingMaster BM
	                Inner Join RunningEWO EOM On EOM.ExportOrderID = BM.ExportOrderID
	                INNER join FabricBookingAcknowledge FBAA ON FBAA.BookingID = BM.BookingID
                    where BM.IsCancel = 0 And BM.ExportOrderID <> 0 and BM.Proposed = 1 And IsCancel = 0
	                AND FBAA.AcknowledgeDate >= '{_startingDate}' OR FBAA.UnAcknowledgeDate >= '{_startingDate}'
                ),
                BKList As
                (
	                Select BM.BookingNo, BM.ExportOrderID, BM.ExportOrderNo, BookingID = Min(BM.BookingID),0 BOMMasterID, ISourcing = CAI.InHouse, BM.BuyerTeamID, ContactID = BM.SupplierID, BM.WithOutOB, RevisionNo = Max(BM.RevisionNo)
	                ,BM.AddedBy,BM.BuyerID
	                FROM BM
	                Inner Join ISG On ISG.SubGroupID = BM.SubGroupID
	                Left Join {DbNames.EPYSL}..ContactAdditionalInfo CAI On CAI.ContactID = BM.SupplierID
	                Where ISNULL(CAI.InHouse,0) = 1
	                Group By BM.BookingNo, BM.ExportOrderID, BM.ExportOrderNo, CAI.InHouse, BM.BuyerTeamID, BM.SupplierID, BM.WithOutOB,BM.AddedBy,BM.BuyerID
                ),BAck as (
	                Select BC.BookingID, BC.BookingNo, BC.ExportOrderID, BC.ExportOrderNo, BC.BOMMasterID,BC.ContactID, BC.BuyerTeamID, BC.ISourcing, BIA.RevisionNo, BIA.SubGroupID, BIA.ItemGroupID, BIA.AcknowledgeDate, BIA.WithoutOB,BC.BuyerID
	                ,BC.AddedBy
	                FROM BKList BC
                    Inner Join {DbNames.EPYSL}..BookingItemAcknowledge BIA On BIA.BookingID = BC.BookingID And BIA.WithoutOB = BC.WithOutOB
	                Group By BC.BookingID, BC.BookingNo, BC.ExportOrderID, BC.ExportOrderNo, BC.BOMMasterID,BC.ContactID, BC.BuyerTeamID, BC.ISourcing, BIA.RevisionNo, BIA.SubGroupID, BIA.ItemGroupID, BIA.AcknowledgeDate, BIA.WithoutOB, BC.AddedBy,BC.BuyerID
                ),
                InHouseItemList as (
	                SELECT BC.BOMMasterID,BC.ExportOrderID, BC.ExportOrderNo, BC.BookingID, BC.BookingNo, BC.BuyerTeamID, BC.SubGroupID, BC.ItemGroupID,BC.ContactID, ISourcing = Max(convert(int,BC.ISourcing)), BC.RevisionNo, AcknowledgeDate = Null, BC.WithOutOB,BKAcknowledgeDate = Max(BC.AcknowledgeDate)
	                ,BC.AddedBy, BC.BuyerID
	                FROM BAck BC
	                Group By BC.BOMMasterID,BC.ExportOrderID, BC.ExportOrderNo, BC.BookingID, BC.BookingNo, BC.BuyerTeamID, BC.SubGroupID, BC.ItemGroupID,BC.ContactID, BC.ISourcing, BC.WithOutOB, BC.RevisionNo,BC.AddedBy, BC.BuyerID
	                having BC.ISourcing = 1
                ),
                ACKListPendingList as
                (
	                Select OSI.ExportOrderID,OSI.ExportOrderNo,OSI.BOMMasterID,OSI.BookingNo,
	                RevisionNo = Max(ISNULL(FBA.RevisionNo,0)), OSI.BuyerTeamID,
	                StatusText = 'Pending for Fabric Booking Acknowledge',
	                BKAcknowledgeDate = Max(OSI.BKAcknowledgeDate), FBAcknowledgeDate = MAX(FBA.AcknowledgeDate), OSI.WithoutOB,E.EmployeeName
	                ,OSI.AddedBy, OSI.BuyerID
                    From InHouseItemList OSI
	                Left Join {DbNames.EPYSL}..ExportWorkOrderLifeCycleChild EL On EL.ExportOrderID = OSI.ExportOrderID And EL.BookingID = OSI.BookingID And EL.ContactID = OSI.ContactID
                    INNER Join FabricBookingAcknowledge FBA On FBA.BookingID = OSI.BookingID  And FBA.SubGroupID = OSI.SubGroupID And FBA.ItemGroupID = OSI.ItemGroupID
	                LEFT JOIN {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = OSI.AddedBy
	                LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = LU.EmployeeCode
                    Where (FBA.AcknowledgeDate >= '{_startingDate}' OR FBA.UnAcknowledgeDate >= '{_startingDate}') AND
		                Case When FBA.AcknowledgeID IS NULL Then 0
					                When FBA.AcknowledgeID IS NOT NULL And ISNULL(FBA.PreProcessRevNo,0) < ISNULL(OSI.RevisionNo,0) Then 2 Else 1 End 
			                = Case When 'N'='N' Then 0
					                When 'N'='N' Then 2 Else 1 End
		                Group By OSI.ExportOrderID,OSI.ExportOrderNo,OSI.BOMMasterID,OSI.ExportOrderID,ISNULL(FBA.RevisionNo,0), OSI.BuyerTeamID,Isnull(EL.FabBSCAckStatus,''),
		                OSI.BookingNo, OSI.WithoutOB, OSI.AddedBy, OSI.BuyerID,E.EmployeeName
                ),
                ACKListRevisionList as
                (
	                 Select OSI.ExportOrderID,OSI.ExportOrderNo,OSI.BOMMasterID,OSI.BookingNo,
	                RevisionNo = Max(ISNULL(FBA.RevisionNo,0)), OSI.BuyerTeamID,
	                StatusText = 'Pending for Revision Acknowledge',
	                BKAcknowledgeDate = Max(OSI.BKAcknowledgeDate), FBAcknowledgeDate = MAX(FBA.AcknowledgeDate), OSI.WithoutOB,E.EmployeeName
	                ,OSI.AddedBy, OSI.BuyerID
                    From InHouseItemList OSI
	                Left Join {DbNames.EPYSL}..ExportWorkOrderLifeCycleChild EL On EL.ExportOrderID = OSI.ExportOrderID And EL.BookingID = OSI.BookingID And EL.ContactID = OSI.ContactID
                    INNER Join FabricBookingAcknowledge FBA On FBA.BookingID = OSI.BookingID  And FBA.SubGroupID = OSI.SubGroupID And FBA.ItemGroupID = OSI.ItemGroupID
	                LEFT JOIN {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = OSI.AddedBy
	                LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = LU.EmployeeCode
                    Where Case When FBA.AcknowledgeID IS NULL Then 0
					                When FBA.AcknowledgeID IS NOT NULL And ISNULL(FBA.PreProcessRevNo,0) < ISNULL(OSI.RevisionNo,0) Then 2 Else 1 End 
			                = Case When 'R'='N' Then 0
					                When 'R'='R' Then 2 Else 1 End
		                Group By OSI.ExportOrderID,OSI.ExportOrderNo,OSI.BOMMasterID,OSI.ExportOrderID,ISNULL(FBA.RevisionNo,0), OSI.BuyerTeamID,Isnull(EL.FabBSCAckStatus,''),
		                OSI.BookingNo, OSI.WithoutOB,OSI.AddedBy, OSI.BuyerID,E.EmployeeName
                ),
                PendingMrkingACKList as
                (
	                Select OSI.ExportOrderID,OSI.ExportOrderNo,OSI.BOMMasterID,OSI.BookingNo,
	                RevisionNo = Max(ISNULL(FBA.RevisionNo,0)), OSI.BuyerTeamID,
	                StatusText = 'Pending for Marketing Acknowledge',
	                BKAcknowledgeDate = Max(OSI.BKAcknowledgeDate), FBAcknowledgeDate = MAX(FBA.AcknowledgeDate), OSI.WithoutOB,E.EmployeeName
	                ,OSI.AddedBy, OSI.BuyerID
	                From InHouseItemList OSI
	                Left Join {DbNames.EPYSL}..ExportWorkOrderLifeCycleChild EL On EL.ExportOrderID = OSI.ExportOrderID And EL.BookingID = OSI.BookingID And EL.ContactID = OSI.ContactID
	                Inner Join FabricBookingAcknowledge FBA On FBA.BookingID = OSI.BookingID And FBA.SubGroupID = OSI.SubGroupID And FBA.ItemGroupID = OSI.ItemGroupID
	                Left Join FBookingAcknowledgeChild FBC On FBC.BookingID = OSI.BookingID
	                LEFT JOIN FBookingAcknowledge FBA1 ON FBA1.BookingID = FBA.BookingID
	                LEFT JOIN {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = OSI.AddedBy
	                LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = LU.EmployeeCode
                    Where (FBA.AcknowledgeDate >= '{_startingDate}' OR FBA.UnAcknowledgeDate >= '{_startingDate}') AND IsNull(FBC.SendToMktAck,0) = 1 And IsNull(FBC.IsMktAck,0) = 0 And IsNull(FBC.IsMktUnAck,0) = 0
                    AND FBA.RevisionNo = FBA1.PreRevisionNo
	                Group By OSI.ExportOrderID,OSI.ExportOrderNo,OSI.BOMMasterID,OSI.ExportOrderID, OSI.BuyerID, OSI.BuyerTeamID,Isnull(EL.FabBSCAckStatus,''),
	                OSI.BookingNo, OSI.WithoutOB,E.EmployeeName,FBA1.MerchandiserID,OSI.AddedBy, OSI.BuyerID
                ),
                UnACKList as
                (
	                 Select OSI.ExportOrderID,OSI.ExportOrderNo,OSI.BOMMasterID,OSI.BookingNo,
	                RevisionNo = Max(ISNULL(FBA.RevisionNo,0)), OSI.BuyerTeamID,
	                StatusText = 'Unacknowledged',
	                BKAcknowledgeDate = Max(OSI.BKAcknowledgeDate), FBAcknowledgeDate = MAX(FBA.AcknowledgeDate), OSI.WithoutOB,E.EmployeeName
	                ,OSI.AddedBy, OSI.BuyerID

                    From InHouseItemList OSI
                    Left Join {DbNames.EPYSL}..ExportWorkOrderLifeCycleChild EL On EL.ExportOrderID = OSI.ExportOrderID And EL.BookingID = OSI.BookingID And EL.ContactID = OSI.ContactID
                    Inner Join FabricBookingAcknowledge FBA On FBA.BookingID = OSI.BookingID And FBA.SubGroupID = OSI.SubGroupID And FBA.ItemGroupID = OSI.ItemGroupID
                    Left Join FBookingAcknowledgeChild FBC On FBC.BookingID = OSI.BookingID
                    LEFT JOIN FBookingAcknowledge FBA1 ON FBA1.BookingID = FBA.BookingID
	                LEFT JOIN {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = OSI.AddedBy
	                LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = LU.EmployeeCode
	                LEFT JOIN {DbNames.EPYSL}..LoginUser LU1 ON LU1.UserCode = FBA.UnAcknowledgeBy
	                LEFT JOIN {DbNames.EPYSL}..Employee E1 ON E1.EmployeeCode = LU1.EmployeeCode
	                WHERE (FBA.AcknowledgeDate >= '{_startingDate}' OR FBA.UnAcknowledgeDate >= '{_startingDate}') AND FBA.PreProcessRevNo = OSI.RevisionNo AND FBA.UnAcknowledge = 1 Or ( IsNull(FBC.IsTxtUnAck,0)=1 Or  ( IsNull(FBC.IsMktUnAck,0)=1 And IsNull(FBC.SendToTxtAck,0)=1 ))
                    Group By OSI.ExportOrderID,OSI.ExportOrderNo,OSI.BOMMasterID,OSI.ExportOrderID, OSI.BuyerID, OSI.BuyerTeamID,Isnull(EL.FabBSCAckStatus,''),
	                OSI.BookingNo, OSI.WithoutOB,E.EmployeeName,FBA1.MerchandiserID,OSI.AddedBy, OSI.BuyerID
                ),
                MainACKList as
                (
                    Select OSI.ExportOrderID,OSI.ExportOrderNo,OSI.BOMMasterID,OSI.BookingNo,
	                RevisionNo = Max(ISNULL(FBA.RevisionNo,0)), OSI.BuyerTeamID,
	                StatusText = 'Acknowledged',
	                BKAcknowledgeDate = Max(OSI.BKAcknowledgeDate), FBAcknowledgeDate = MAX(FBA.AcknowledgeDate), OSI.WithoutOB,E.EmployeeName
	                ,OSI.AddedBy, OSI.BuyerID
                    From InHouseItemList OSI
	                Left Join {DbNames.EPYSL}..ExportWorkOrderLifeCycleChild EL On EL.ExportOrderID = OSI.ExportOrderID And EL.BookingID = OSI.BookingID And EL.ContactID = OSI.ContactID
                    Inner Join FabricBookingAcknowledge FBA On FBA.BookingID = OSI.BookingID And FBA.SubGroupID = OSI.SubGroupID And FBA.ItemGroupID = OSI.ItemGroupID
                    Left Join FBookingAcknowledgeChild FBC On FBC.BookingID = OSI.BookingID
                    LEFT JOIN FBookingAcknowledge FBA1 ON FBA1.BookingID = FBA.BookingID
	                LEFT JOIN {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = OSI.AddedBy -- FBA1.MerchandiserID
	                LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = LU.EmployeeCode
	                Where (FBA.AcknowledgeDate >= '{_startingDate}' OR FBA.UnAcknowledgeDate >= '{_startingDate}') AND   
                    (IsNull(FBC.IsTxtAck,0)=1) And 
	                (IsNull(FBC.IsMktUnAck,0)=0) AND
	                FBA1.IsUnAcknowledge=0  
                    Group By OSI.ExportOrderID,OSI.ExportOrderNo,OSI.BOMMasterID,OSI.ExportOrderID, OSI.BuyerID, OSI.BuyerTeamID,Isnull(EL.FabBSCAckStatus,''),
	                OSI.BookingNo, OSI.WithoutOB,E.EmployeeName,FBA1.MerchandiserID,OSI.AddedBy, OSI.BuyerID
                ),
                RunningEWO_WithCancel As
                (
	                Select EOM.ExportOrderID, EOM.ExportOrderNo, EOM.StyleMasterID, EOM.BuyerID, EOM.BuyerTeamID, EOM.EWOStatusID
	                From {DbNames.EPYSL}..ExportOrderMaster EOM
	                Where EOM.EWOStatusID IN (130,131)
                ),
                BM_WithCancel As 
                (
	                Select BM.BookingID, BookingNo, BM.ExportOrderID, SupplierID, BM.SubGroupID, WithOutOB = Convert(bit,0),
	                EOM.ExportOrderNo, EOM.StyleMasterID, BM.RevisionNo, EOM.BuyerID, EOM.BuyerTeamID, BM.AddedBy
	                from {DbNames.EPYSL}..BookingMaster BM
	                Inner Join RunningEWO_WithCancel EOM On EOM.ExportOrderID = BM.ExportOrderID
	                INNER join FabricBookingAcknowledge FBAA ON FBAA.BookingID = BM.BookingID
                    where (FBAA.AcknowledgeDate >= '{_startingDate}' OR FBAA.UnAcknowledgeDate >= '{_startingDate}') AND (BM.IsCancel = 1 OR EOM.EWOStatusID = 131) And BM.SubGroupID in (1,11,12)
	                Union All
	                Select BM.BookingID, BookingNo, BM.ExportOrderID, SupplierID, 1 SubGroupID, WithOutOB = Convert(bit,1),
	                EOM.ExportOrderNo, EOM.StyleMasterID, BM.RevisionNo, EOM.BuyerID, EOM.BuyerTeamID, BM.AddedBy
	                from {DbNames.EPYSL}..SampleBookingMaster BM
	                Inner Join RunningEWO_WithCancel EOM On EOM.ExportOrderID = BM.ExportOrderID
	                INNER join FabricBookingAcknowledge FBAA ON FBAA.BookingID = BM.BookingID
                    where (FBAA.AcknowledgeDate >= '{_startingDate}' OR FBAA.UnAcknowledgeDate >= '{_startingDate}') AND (BM.IsCancel = 1 OR EOM.EWOStatusID = 131) And BM.ExportOrderID <> 0 and BM.Proposed = 1 And IsCancel = 0/**/
                ),
                BKList_WithCancel As
                (
                Select BM.BookingNo, BM.ExportOrderID, BM.ExportOrderNo, BookingID = Min(BM.BookingID),0 BOMMasterID, ISourcing = CAI.InHouse, BM.BuyerTeamID, ContactID = BM.SupplierID, BM.WithOutOB, RevisionNo = Max(BM.RevisionNo)
	                ,BM.AddedBy,BM.BuyerID
	                FROM BM_WithCancel BM
	                Inner Join ISG On ISG.SubGroupID = BM.SubGroupID
	                Left Join {DbNames.EPYSL}..ContactAdditionalInfo CAI On CAI.ContactID = BM.SupplierID
	                Where ISNULL(CAI.InHouse,0) = 1
	                Group By BM.BookingNo, BM.ExportOrderID, BM.ExportOrderNo, CAI.InHouse, BM.BuyerTeamID, BM.SupplierID, BM.WithOutOB,BM.AddedBy,BM.BuyerID
                ),
                BAck_WithCancel as (
	                Select BC.BookingID, BC.BookingNo, BC.ExportOrderID, BC.ExportOrderNo, BC.BOMMasterID,BC.ContactID, BC.BuyerTeamID, BC.ISourcing, BIA.RevisionNo, BIA.SubGroupID, BIA.ItemGroupID, BIA.AcknowledgeDate, BIA.WithoutOB,BC.BuyerID
	                ,BC.AddedBy
	                FROM BKList_WithCancel BC
                    Inner Join {DbNames.EPYSL}..BookingItemAcknowledge BIA On BIA.BookingID = BC.BookingID And BIA.WithoutOB = BC.WithOutOB
	                Group By BC.BookingID, BC.BookingNo, BC.ExportOrderID, BC.ExportOrderNo, BC.BOMMasterID,BC.ContactID, BC.BuyerTeamID, BC.ISourcing, BIA.RevisionNo, BIA.SubGroupID, BIA.ItemGroupID, BIA.AcknowledgeDate, BIA.WithoutOB, BC.AddedBy,BC.BuyerID
                ),
                InHouseItemList_WithCancel as (
	                SELECT BC.BOMMasterID,BC.ExportOrderID, BC.ExportOrderNo, BC.BookingID, BC.BookingNo, BC.BuyerTeamID, BC.SubGroupID, BC.ItemGroupID,BC.ContactID, ISourcing = Max(convert(int,BC.ISourcing)), BC.RevisionNo, AcknowledgeDate = Null, BC.WithOutOB,BKAcknowledgeDate = Max(BC.AcknowledgeDate)
	                ,BC.AddedBy, BC.BuyerID
	                FROM BAck_WithCancel BC
	                Group By BC.BOMMasterID,BC.ExportOrderID, BC.ExportOrderNo, BC.BookingID, BC.BookingNo, BC.BuyerTeamID, BC.SubGroupID, BC.ItemGroupID,BC.ContactID, BC.ISourcing, BC.WithOutOB, BC.RevisionNo,BC.AddedBy, BC.BuyerID
	                having BC.ISourcing = 1
                ),
                MMCancelList as
                (
	                Select OSI.ExportOrderID,OSI.ExportOrderNo,OSI.BOMMasterID,OSI.BookingNo,
	                RevisionNo = Max(ISNULL(FBA.RevisionNo,0)), OSI.BuyerTeamID,
	                StatusText = 'M&M Cancel',
	                BKAcknowledgeDate = Max(OSI.BKAcknowledgeDate), FBAcknowledgeDate = MAX(FBA.AcknowledgeDate), OSI.WithoutOB,E.EmployeeName
	                ,OSI.AddedBy, OSI.BuyerID
	                From InHouseItemList_WithCancel OSI
	                Left Join {DbNames.EPYSL}..ExportWorkOrderLifeCycleChild EL On EL.ExportOrderID = OSI.ExportOrderID And EL.BookingID = OSI.BookingID And EL.ContactID = OSI.ContactID
	                Inner Join FabricBookingAcknowledge FBA On FBA.BookingID = OSI.BookingID And FBA.SubGroupID = OSI.SubGroupID And FBA.ItemGroupID = OSI.ItemGroupID
	                Left Join FBookingAcknowledgeChild FBC On FBC.BookingID = OSI.BookingID
	                LEFT JOIN FBookingAcknowledge FBA1 ON FBA1.BookingID = FBA.BookingID
	                LEFT JOIN {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = OSI.AddedBy -- FBA1.MerchandiserID
	                LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = LU.EmployeeCode
                    WHERE (FBA.AcknowledgeDate >= '{_startingDate}' OR FBA.UnAcknowledgeDate >= '{_startingDate}') AND FBA.RevisionNo = FBA1.PreRevisionNo
	                Group By OSI.ExportOrderID,OSI.ExportOrderNo,OSI.BOMMasterID,OSI.ExportOrderID, OSI.BuyerID, OSI.BuyerTeamID,Isnull(EL.FabBSCAckStatus,''),
	                OSI.BookingNo, OSI.WithoutOB,E.EmployeeName,FBA1.MerchandiserID,OSI.AddedBy, OSI.BuyerID
                ),
                AllList AS
                (
	                SELECT * FROM ACKListPendingList
	                UNION
	                SELECT * FROM ACKListRevisionList
	                UNION
	                SELECT * FROM PendingMrkingACKList
	                UNION
	                SELECT * FROM MainACKList
	                UNION
	                SELECT * FROM UnACKList
	                UNION
	                SELECT * FROM MMCancelList
                ),
                AllListFinal AS
                (
                    SELECT BK.ExportOrderID,BK.ExportOrderNo,BK.BOMMasterID,BK.RevisionNo, CCT.TeamName BuyerTeamName, CTO.ShortName BuyerName,
	                BK.StatusText, 
	                BK.BKAcknowledgeDate, BK.FBAcknowledgeDate,BK.BookingNo, BK.WithoutOB,E.EmployeeName
	                From AllList BK
	                LEFT Join {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = BK.BuyerID
	                LEFT Join {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = BK.BuyerTeamID
	                LEFT JOIN {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = BK.AddedBy
	                LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = LU.EmployeeCode
	                Group By BK.ExportOrderID,BK.ExportOrderNo,BK.BOMMasterID,BK.RevisionNo, CCT.TeamName, CTO.ShortName,
	                BK.StatusText,BK.BKAcknowledgeDate, BK.FBAcknowledgeDate,BK.BookingNo, BK.WithoutOB,E.EmployeeName
                ),

                FinalObj AS
                (
	                SELECT NewCount = COUNT(NewList.BookingNo), Revision = 0, Pending2 = 0,
	                Acknowledged = 0, UnAcknowledged = 0, Cancel = 0,
					AllCount = 0
	                FROM NewList
	                UNION
	                SELECT NewCount = 0, Revision = COUNT(RevisionList.BookingNo), Pending2 = 0,
	                Acknowledged = 0, UnAcknowledged = 0, Cancel = 0,
					AllCount = 0
	                FROM RevisionList
	                UNION
	                SELECT NewCount = 0, Revision = 0, Pending2 = COUNT(PendingMKTList.BookingNo),
	                Acknowledged = 0, UnAcknowledged = 0, Cancel = 0,
					AllCount = 0
	                FROM PendingMKTList
	                UNION
	                SELECT NewCount = 0, Revision = 0, Pending2 = 0,
	                Acknowledged = COUNT(AcknowledgeList.BookingNo), UnAcknowledged = 0, Cancel = 0,
					AllCount = 0
	                FROM AcknowledgeList
	                UNION
	                SELECT NewCount = 0, Revision = 0, Pending2 = 0,
	                Acknowledged = 0, UnAcknowledged = COUNT(UnAcknowledgeList.BookingNo), Cancel = 0,
					AllCount = 0
	                FROM UnAcknowledgeList
	                UNION
	                SELECT NewCount = 0, Revision = 0, Pending2 = 0,
	                Acknowledged = 0, UnAcknowledged = 0, Cancel = COUNT(CancelList.BookingNo),
					AllCount = 0
	                FROM CancelList
					UNION
	                SELECT NewCount = 0, Revision = 0, Pending2 = 0,
	                Acknowledged = 0, UnAcknowledged = 0, Cancel = 0,
					AllCount = COUNT(AllListFinal.BookingNo)
	                FROM AllListFinal
                )
                SELECT NewCount = SUM(NewCount), Revision = SUM(Revision), Pending2 = SUM(Pending2), Acknowledged = SUM(Acknowledged), UnAcknowledged = SUM(UnAcknowledged), Cancel = SUM(Cancel), AllCount = SUM(AllCount) FROM FinalObj
                                            ";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                CountListItem data = records.Read<CountListItem>().FirstOrDefault();
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
        public async Task<FBookingAcknowledge> GetNewAsync(int bookingId)
        {
            var query =
                $@"-- Master Data
                 WITH M AS (
                    SELECT SBM.BookingID,SBM.BookingNo,SBM.BookingDate,SBM.SLNo,SBM.StyleMasterID,SBM.StyleNo,SBM.SubGroupID,SBM.OrderQty,SBM.BuyerID,SBM.BuyerTeamID,SBM.SupplierID,SBM.ExportOrderID,SBM.ExecutionCompanyID,SBM.Remarks,SBM.SeasonID,Count(*) Over() TotalRows, SBM.AddedBy BookingBy
                    ,SBM.RevisionNo PreRevisionNo
                    FROM {DbNames.EPYSL}..SampleBookingMaster SBM
                    WHERE SBM.BookingID={bookingId}
                )
                SELECT FBA.FBAckID, M.BookingID,M.BookingNo,M.BookingDate,M.SLNo,M.StyleMasterID,M.StyleNo,M.OrderQty BookingQty,M.BuyerID,M.BuyerTeamID,M.SupplierID,M.ExportOrderID,M.ExecutionCompanyID,
                CTO.ShortName BuyerName, CCT.TeamName BuyerTeamName,C.CompanyName,M.SubGroupID,M.Remarks,Supplier.ShortName [SupplierName],Season.SeasonName, M.BookingBy, ISNULL(Supplier.MappingCompanyID,0) TextileCompanyID
                ,M.PreRevisionNo
                FROM M
                LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = M.BuyerID
				LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = M.BuyerTeamID
				LEFT JOIN {DbNames.EPYSL}..CompanyEntity C ON C.CompanyID = M.ExecutionCompanyID
		        LEFT JOIN FBookingAcknowledge FBA ON FBA.BookingID=M.BookingID
                LEFT Join {DbNames.EPYSL}..Contacts Supplier On M.SupplierID = Supplier.ContactID
                LEFT Join {DbNames.EPYSL}..ContactSeason Season On M.SeasonID = Season.SeasonID;

               ----- Sample Booking Consumption Child (Fabric)
                SELECT ROW_NUMBER() OVER(ORDER BY SBC.ReferenceSourceID ASC) AS BookingChildID, SBC.ReferenceSourceID,RSETV.ValueName ReferenceSourceName , SBC.ReferenceNo,SBC.ColorReferenceNo,SBC.YarnSourceID,SBC.ConsumptionID,SBC.BookingID,SBM.BookingNo,SBC.ItemGroupID,SBC.SubGroupID,SBC.Segment1Desc Construction,SBC.Segment2Desc Composition,SBC.Segment3Desc Color
                ,SBC.Segment4Desc GSM,SBC.Segment5Desc FabricWidth,SBC.Segment7Desc KnittingType,SBC.LengthYds,SBC.LengthInch,SBC.FUPartID,SBC.ConsumptionQty,SBC.OrderUnitID BookingUnitID
                ,SBC.A1ValueID,SBC.YarnBrandID,SBC.LabDipNo,SBC.Price,SBC.SuggestedPrice,SBC.RequiredQty BookingQty,CC.ItemMasterID,SBM.BuyerID ContactID,
                SBM.ExecutionCompanyID, 0 As TechnicalNameId,ISG.SubGroupName,'1' As ConceptTypeID,IM.Segment1ValueID ConstructionId, IM.Segment2ValueID CompositionId,
				IM.Segment3ValueID ColorId,IM.Segment7ValueID KnittingTypeId,IM.Segment4ValueID GSMId,
                ISV.SegmentValue YarnType, ETV.ValueName YarnProgram, SBC.Segment6Desc DyeingType, SBC.Remarks Instruction, SBC.ForBDSStyleNo, ISNULL(Supplier.MappingCompanyID,0) TextileCompanyID,LiabilitiesBookingQty=ISNULL(FBAC.LiabilitiesBookingQty,0),ActualBookingQty=ISNULL(FBAC.ActualBookingQty,0)
                FROM {DbNames.EPYSL}..SampleBookingConsumption SBC
                INNER JOIN {DbNames.EPYSL}..SampleBookingConsumptionChild CC ON CC.ConsumptionID = SBC.ConsumptionID
                INNER JOIN {DbNames.EPYSL}..SampleBookingMaster SBM ON SBM.BookingID=SBC.BookingID 
                INNER JOIN {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = SBC.SubGroupID
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = CC.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV On ISV.SegmentValueID = SBC.A1ValueID
				LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = SBC.YarnBrandID
                Left Join {DbNames.EPYSL}..EntityTypeValue RSETV ON RSETV.ValueID = SBC.ReferenceSourceID
                Left Join {DbNames.EPYSL}..EntityTypeValue YSETV ON YSETV.ValueID = SBC.YarnSourceID
                LEFT Join {DbNames.EPYSL}..Contacts Supplier On Supplier.ContactID = SBM.SupplierID
                LEFT JOIN FBookingAcknowledgeChild FBAC On FBAC.ConsumptionID = SBC.ConsumptionID And FBAC.ItemMasterID = CC.ItemMasterID
                WHERE SBC.BookingID={bookingId} AND SBC.SubGroupID=1;

                ----- Sample Booking Consumption Child (Collor & Cuff)
                SELECT ROW_NUMBER() OVER(ORDER BY ReferenceSourceID ASC) AS BookingChildID, SBC.ReferenceSourceID,RSETV.ValueName ReferenceSourceName , SBC.ReferenceNo,SBC.ColorReferenceNo,SBC.YarnSourceID,SBC.ConsumptionID,SBC.BookingID,SBM.BookingNo,
                SBC.ItemGroupID,SBC.SubGroupID,SBC.LengthYds,SBC.LengthInch,SBC.FUPartID,SBC.ConsumptionQty,SBC.OrderUnitID BookingUnitID, SBC.A1ValueID,SBC.YarnBrandID,SBC.LabDipNo,
                SBC.Price,SBC.SuggestedPrice,SBC.RequiredQty BookingQty,CC.ItemMasterID,SBM.BuyerID ContactID, SBM.ExecutionCompanyID, 0 As TechnicalNameId,
                ISG.SubGroupName,'1' As ConceptTypeID, SBC.Remarks Instruction, SBC.ForBDSStyleNo,
                IM.Segment1ValueID ConstructionId, IM.Segment2ValueID CompositionId, IM.Segment5ValueID ColorId,IM.Segment7ValueID KnittingTypeId,
                IM.Segment4ValueID GSMId, ISV.SegmentValue YarnType, ETV.ValueName YarnProgram, SBC.Segment6Desc DyeingType,
                SBC.Segment1Desc Description,SBC.Segment2Desc Type, SBC.Segment3Desc Length, SBC.Segment4Desc Height, SBC.Segment5Desc Color, ISNULL(Supplier.MappingCompanyID,0) TextileCompanyID

                FROM {DbNames.EPYSL}..SampleBookingConsumption SBC
                INNER JOIN {DbNames.EPYSL}..SampleBookingConsumptionChild CC ON CC.ConsumptionID = SBC.ConsumptionID
                INNER JOIN {DbNames.EPYSL}..SampleBookingMaster SBM ON SBM.BookingID=SBC.BookingID
                INNER JOIN {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = SBC.SubGroupID
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = CC.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV On ISV.SegmentValueID = SBC.A1ValueID
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = SBC.YarnBrandID
                Left Join {DbNames.EPYSL}..EntityTypeValue RSETV ON RSETV.ValueID = SBC.ReferenceSourceID
                Left Join {DbNames.EPYSL}..EntityTypeValue YSETV ON YSETV.ValueID = SBC.YarnSourceID
                LEFT Join {DbNames.EPYSL}..Contacts Supplier On Supplier.ContactID = SBM.SupplierID
                WHERE SBC.BookingID={bookingId} AND SBC.SubGroupID IN (11, 12);

               ----Sample Booking Consumption AddProcess
                SELECT  BookingID ,ProcessID,ConsumptionID
                FROM {DbNames.EPYSL}..SampleBookingConsumptionAddProcess where BookingID={bookingId};

                ----- Sample Booking Consumption Child Details
                SELECT ConsumptionID,BookingID,ItemGroupID,SubGroupID,ItemMasterID,RequiredQty BookingQty ,ConsumptionQty,RequiredUnitID BookingUnitID, 0 As TechnicalNameId
                FROM {DbNames.EPYSL}..SampleBookingConsumptionChild where BookingID={bookingId}

                ----- Sample Booking Consumption Garment Part
                SELECT  BookingID ,FUPartID,ConsumptionID
                 FROM {DbNames.EPYSL}..SampleBookingConsumptionGarmentPart where BookingID={bookingId}

                ---Sample Booking Consumption Process
                SELECT BookingID,ProcessID,ConsumptionID
                FROM {DbNames.EPYSL}..SampleBookingConsumptionProcess where BookingID={bookingId}

                ------ Sample Booking Consumption Text
                SELECT BookingID,UsesIn,AdditionalProcess,ApplicableProcess,YarnSubProgram,GarmentsColor GmtColor,ConsumptionID
                FROM {DbNames.EPYSL}..SampleBookingConsumptionText  where BookingID={bookingId}

                ------ Sample Booking Child Distribution 
                SELECT *
                FROM {DbNames.EPYSL}..SampleBookingChildDistribution  where BookingID={bookingId}

                ----- SampleBooking ConsumptionYarn Sub Brand
                SELECT SBKC.ReferenceSourceID,RSETV.ValueName ReferenceSourceName , SBKC.ReferenceNo,SBKC.ColorReferenceNo,SBKC.YarnSourceID,SCYS.BookingID, SCYS.YarnSubBrandID, SCYS.ConsumptionID, YSB.YarnSubBrandName
				FROM {DbNames.EPYSL}..SampleBookingConsumptionYarnSubBrand SCYS
				Inner Join {DbNames.EPYSL}..SampleBookingConsumption SBKC On SCYS.BookingID = SBKC.BookingID And SCYS.ConsumptionID = SBKC.ConsumptionID
                Inner Join {DbNames.EPYSL}..ItemGroup IG On IG.ItemGroupID = SBKC.ItemGroupID
                Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = SBKC.SubGroupID
                Left Join {DbNames.EPYSL}..EntityTypeValue RSETV ON RSETV.ValueID = SBKC.ReferenceSourceID
                Left Join  {DbNames.EPYSL}..EntityTypeValue YSETV ON YSETV.ValueID = SBKC.YarnSourceID
				Inner Join (
					Select ETV.ValueID YarnSubBrandID, ETV.ValueName YarnSubBrandName
					From {DbNames.EPYSL}..EntityTypeValue ETV
					Inner Join {DbNames.EPYSL}..EntityType ET On ET.EntityTypeID = ETV.EntityTypeID
				) YSB On YSB.YarnSubBrandID = SCYS.YarnSubBrandID
				Where SCYS.BookingID = {bookingId};

                ----- SampleBookingChildImage
                SELECT BookingID,ImagePath
                FROM {DbNames.EPYSL}..SampleBookingChildImage where BookingID={bookingId}

                --Free Concept
                Select SBC.ReferenceSourceID,RSETV.ValueName ReferenceSourceName , SBC.ReferenceNo,SBC.ColorReferenceNo,SBC.YarnSourceID,FB.BookingID, FB.BookingNo, FB.BookingDate ConceptDate, SBC.RequiredQty Qty,
                CC.ItemMasterID, IM.Segment1ValueID ConstructionId, IM.Segment2ValueID CompositionId, IM.Segment3ValueID ColorId, Color.SegmentValue ColorName, IM.Segment4ValueID GSMId,
                IM.Segment7ValueID KnittingTypeId, IM.SubGroupID,FB.ExecutionCompanyID CompanyID, ISNULL(Supplier.MappingCompanyID,0) TextileCompanyID
                FROM {DbNames.EPYSL}..SampleBookingConsumption SBC
                INNER JOIN {DbNames.EPYSL}..SampleBookingConsumptionChild CC ON CC.ConsumptionID = SBC.ConsumptionID
                INNER JOIN {DbNames.EPYSL}..SampleBookingMaster FB ON FB.BookingID = SBC.BookingID
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = CC.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Width ON Width.SegmentValueID = IM.Segment5ValueID
                --LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue DT ON DT.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue KT ON KT.SegmentValueID = IM.Segment7ValueID
                --LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID = FB.SubGroupID
                Left Join  {DbNames.EPYSL}..EntityTypeValue RSETV ON RSETV.ValueID = SBC.ReferenceSourceID
                Left Join  {DbNames.EPYSL}..EntityTypeValue YSETV ON YSETV.ValueID = SBC.YarnSourceID
                LEFT Join {DbNames.EPYSL}..Contacts Supplier On Supplier.ContactID = FB.SupplierID
                where FB.BookingID={bookingId};

                --Technical Name
                SELECT Cast(T.TechnicalNameId As varchar) id, T.TechnicalName [text], ISNULL(ST.[Days], 0) [desc], Cast(SC.SubClassID as varchar) additionalValue
                FROM FabricTechnicalName T
                LEFT JOIN FabricTechnicalNameKMachineSubClass SC ON SC.TechnicalNameID = T.TechnicalNameId
                LEFT JOIN KnittingMachineStructureType_HK ST ON ST.StructureTypeID = SC.StructureTypeID
                Group By T.TechnicalNameId, T.TechnicalName, ST.Days, SC.SubClassID;

                 --YarnSource data load
				Select Cast(a.ValueID as varchar) id, a.ValueName [text]
				From {DbNames.EPYSL}..EntityTypeValue a
				Inner Join {DbNames.EPYSL}..EntityType b on b.EntityTypeID = a.EntityTypeID
				Where b.EntityTypeName = 'Yarn Source'

                --M/c type
                ;SELECT CAST(a.MachineSubClassID AS varchar) [id], b.SubClassName [text], b.TypeID [desc], c.TypeName additionalValue
                FROM KnittingMachine a
                INNER JOIN KnittingMachineSubClass b ON b.SubClassID = a.MachineSubClassID
                Inner Join KnittingMachineType c On c.TypeID = b.TypeID
                --Where c.TypeName != 'Flat Bed'
                GROUP BY a.MachineSubClassID, b.SubClassName, b.TypeID, c.TypeName;

                --CriteriaNames
                 ;SELECT CriteriaName,CriteriaSeqNo,(CASE WHEN CriteriaName  IN('Batch Preparation','Quality Check') THEN '1'ELSE'0'END) AS TotalTime
                FROM BDSCriteria_HK --WHERE CriteriaName NOT IN('Batch Preparation','Testing')
                 GROUP BY CriteriaSeqNo,CriteriaName order by CriteriaSeqNo,CriteriaName;

                --FBAChildPlannings
               ;SELECT * FROM BDSCriteria_HK order by CriteriaSeqNo, OperationSeqNo, CriteriaName;

                --Liability Process
				        Select LChildID = 0,BookingChildID = 0,AcknowledgeID = 0,BookingID = 0,UnitID = 0, Cast(a.ValueID as varchar) LiabilitiesProcessID, a.ValueName LiabilitiesName,LiabilityQty=0
				        From {DbNames.EPYSL}..EntityTypeValue a
				        Inner Join {DbNames.EPYSL}..EntityType b on b.EntityTypeID = a.EntityTypeID
				        Where b.EntityTypeName = 'Process Liability'
                --Liability Process data load
				Select LChildID = IsNull(F.LChildID,0),BookingChildID = IsNull(F.BookingChildID,0),AcknowledgeID = IsNull(F.AcknowledgeID,0),BookingID = IsNull(F.BookingID,0),UnitID = IsNull(F.UnitID,0), Cast(a.ValueID as varchar) LiabilitiesProcessID, a.ValueName LiabilitiesName,LiabilityQty=IsNull(F.LiabilityQty,0)
				From {DbNames.EPYSL}..EntityTypeValue a
				Inner Join {DbNames.EPYSL}..EntityType b on b.EntityTypeID = a.EntityTypeID
				Left Join (Select LChildID,BookingChildID,AcknowledgeID,BookingID,LiabilitiesProcessID,UnitID,LiabilityQty From FBookingAcknowledgementLiabilityDistribution Where BookingID = {bookingId} Group By LChildID,BookingChildID,AcknowledgeID,BookingID,LiabilitiesProcessID,UnitID,LiabilityQty)F On F.LiabilitiesProcessID = a.ValueID
				Where b.EntityTypeName = 'Process Liability'";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                FBookingAcknowledge data = records.Read<FBookingAcknowledge>().FirstOrDefault();
                Guard.Against.NullObject(data);

                List<FBookingAcknowledgeChild> bookingChilds = records.Read<FBookingAcknowledgeChild>().ToList();
                List<FBookingAcknowledgeChild> bookingChildsCollarCuff = records.Read<FBookingAcknowledgeChild>().ToList();

                data.FBookingAcknowledgeChildAddProcess = records.Read<FBookingAcknowledgeChildAddProcess>().ToList();
                data.FBookingChildDetails = records.Read<FBookingAcknowledgeChildDetails>().ToList();
                data.FBookingAcknowledgeChildGarmentPart = records.Read<FBookingAcknowledgeChildGarmentPart>().ToList();
                data.FBookingAcknowledgeChildProcess = records.Read<FBookingAcknowledgeChildProcess>().ToList();
                data.FBookingAcknowledgeChildText = records.Read<FBookingAcknowledgeChildText>().ToList();
                data.FBookingAcknowledgeChildDistribution = records.Read<FBookingAcknowledgeChildDistribution>().ToList();
                data.FBookingAcknowledgeChildYarnSubBrand = records.Read<FBookingAcknowledgeChildYarnSubBrand>().ToList();
                data.FBookingAcknowledgeImage = records.Read<FBookingAcknowledgeImage>().ToList();
                data.FreeConcepts = records.Read<FreeConceptMaster>().ToList();
                data.TechnicalNameList = await records.ReadAsync<Select2OptionModel>();
                data.YarnSourceNameList = await records.ReadAsync<Select2OptionModel>();

                List<Select2OptionModel> mcTypeList = records.Read<Select2OptionModel>().ToList();
                data.MCTypeForFabricList = mcTypeList.Where(x => x.additionalValue != "Flat Bed");
                data.MCTypeForOtherList = mcTypeList.Where(x => x.additionalValue == "Flat Bed");

                List<FBookingAcknowledgeChild> criteriaNames = records.Read<FBookingAcknowledgeChild>().ToList();
                List<FBAChildPlanning> fbaChildPlannings = records.Read<FBAChildPlanning>().ToList();
                List<FBookingAcknowledgementLiabilityDistribution> LiabilityDistributionList = records.Read<FBookingAcknowledgementLiabilityDistribution>().ToList();
                List<FBookingAcknowledgementLiabilityDistribution> FBookingAckLiabilityDistributionList = records.Read<FBookingAcknowledgementLiabilityDistribution>().ToList();
                //criteriaNames.ForEach(cn =>
                //{
                //    cn.FBAChildPlannings = fbaChildPlannings.Where(x => x.CriteriaName == cn.CriteriaName).ToList();
                //});

                List<FBookingAcknowledgementLiabilityDistribution> tmpLB = new List<FBookingAcknowledgementLiabilityDistribution>();
                for (int i = 0; i < bookingChilds.Count; i++)
                {
                    tmpLB = new List<FBookingAcknowledgementLiabilityDistribution>();
                    tmpLB.AddRange(LiabilityDistributionList);
                    bookingChilds[i].CriteriaNames = criteriaNames;
                    bookingChilds[i].FBAChildPlannings = fbaChildPlannings;
                    bookingChilds[i].ChildAckLiabilityDetails = tmpLB.ConvertAll(x => new FBookingAcknowledgementLiabilityDistribution(x)).ToList();

                    foreach (FBookingAcknowledgementLiabilityDistribution obitem in bookingChilds[i].ChildAckLiabilityDetails)
                    {
                        List<FBookingAcknowledgementLiabilityDistribution> objLBList = FBookingAckLiabilityDistributionList.Where(j => j.BookingChildID == bookingChilds[i].BookingChildID && j.LiabilitiesProcessID == obitem.LiabilitiesProcessID).ToList();
                        foreach (FBookingAcknowledgementLiabilityDistribution objLB in objLBList)
                        {
                            if (objLB.IsNotNull())
                            {
                                obitem.LChildID = objLB.LChildID;
                                obitem.BookingChildID = objLB.BookingChildID;
                                obitem.AcknowledgeID = objLB.AcknowledgeID;
                                obitem.BookingID = objLB.BookingID;
                                obitem.UnitID = objLB.UnitID;
                                obitem.LiabilityQty = objLB.LiabilityQty;
                            }
                        }

                    }
                }

                tmpLB = new List<FBookingAcknowledgementLiabilityDistribution>();
                for (int i = 0; i < bookingChildsCollarCuff.Count; i++)
                {
                    tmpLB = new List<FBookingAcknowledgementLiabilityDistribution>();
                    tmpLB.AddRange(LiabilityDistributionList);
                    bookingChilds[i].CriteriaNames = criteriaNames;
                    bookingChilds[i].FBAChildPlannings = fbaChildPlannings;
                    bookingChilds[i].ChildAckLiabilityDetails = tmpLB.ConvertAll(x => new FBookingAcknowledgementLiabilityDistribution(x)).ToList();

                    foreach (FBookingAcknowledgementLiabilityDistribution obitem in bookingChilds[i].ChildAckLiabilityDetails)
                    {
                        List<FBookingAcknowledgementLiabilityDistribution> objLBList = FBookingAckLiabilityDistributionList.Where(j => j.BookingChildID == bookingChilds[i].BookingChildID && j.LiabilitiesProcessID == obitem.LiabilitiesProcessID).ToList();
                        foreach (FBookingAcknowledgementLiabilityDistribution objLB in objLBList)
                        {
                            if (objLB.IsNotNull())
                            {
                                obitem.LChildID = objLB.LChildID;
                                obitem.BookingChildID = objLB.BookingChildID;
                                obitem.AcknowledgeID = objLB.AcknowledgeID;
                                obitem.BookingID = objLB.BookingID;
                                obitem.UnitID = objLB.UnitID;
                                obitem.LiabilityQty = objLB.LiabilityQty;
                            }
                        }

                    }
                }

                data.FBookingChild = bookingChilds.Where(x => x.SubGroupID == 1).ToList();
                data.FBookingChildCollor = bookingChildsCollarCuff.Where(x => x.SubGroupID == 11).ToList();
                data.FBookingChildCuff = bookingChildsCollarCuff.Where(x => x.SubGroupID == 12).ToList();
                data.HasFabric = data.FBookingChild.Count() > 0 ? true : false;
                data.HasCollar = data.FBookingChildCollor.Count() > 0 ? true : false;
                data.HasCuff = data.FBookingChildCuff.Count() > 0 ? true : false;

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

        public async Task<FBookingAcknowledge> GetNewBulkAsync(int bookingId)
        {
            var query = $@"WITH FBA1 AS
                        (
                            SELECT a.BookingID,a.BOMMasterID,a.ItemGroupID,a.SubGroupID,a.Status,PreRevisionNo=a.PreProcessRevNo,
	                        b.BookingNo,b.BookingDate,b.Remarks,StyleNo='',b.ExportOrderID,SLNo='',StyleMasterID=0,
	                        b.BuyerID,b.BuyerTeamID,b.CompanyID,b.SupplierID,SeasonID=0,a.WithoutOB
                            FROM FabricBookingAcknowledge A
							INNER JOIN {DbNames.EPYSL}..BookingMaster b on b.BookingID = a.BookingID
                            WHERE A.BookingID={bookingId} AND a.WithoutOB = 0
                        ),
						FBA2 AS
                        (
                            SELECT a.BookingID,a.BOMMasterID,a.ItemGroupID,a.SubGroupID,a.Status,PreRevisionNo=a.PreProcessRevNo,
	                        b.BookingNo,b.BookingDate,b.Remarks,b.StyleNo,b.ExportOrderID,b.SLNo,b.StyleMasterID,
	                        b.BuyerID,b.BuyerTeamID,CompanyID=b.ExecutionCompanyID,b.SupplierID,b.SeasonID,a.WithoutOB
                            FROM FabricBookingAcknowledge A
							Inner Join {DbNames.EPYSL}..SampleBookingMaster b on b.BookingID = a.BookingID
                            WHERE A.BookingID={bookingId} AND a.WithoutOB = 1
                        ),
						FBA AS
                        (
	                        SELECT *FROM FBA1
	                        UNION
	                        SELECT *FROM FBA2
                        ),
                        F AS(
                        SELECT FBA.*,CTO.ContactDisplayCode AS BuyerName, CCT.DisplayCode AS BuyerTeamName,CompanyName = C.ShortName,
                        Supplier.ShortName [SupplierName],Season.SeasonName, ISNULL(Supplier.MappingCompanyID,0) TextileCompanyID
                        FROM FBA
                        LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = FBA.BuyerID
                        LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = FBA.BuyerTeamID
                        LEFT JOIN {DbNames.EPYSL}..CompanyEntity C ON C.CompanyID = FBA.CompanyID
                        LEFT Join {DbNames.EPYSL}..Contacts Supplier On Supplier.ContactID = FBA.SupplierID
                        LEFT Join {DbNames.EPYSL}..ContactSeason Season On Season.SeasonID = FBA.SeasonID)

						SELECT * FROM F;

                        ----- Sample Booking Consumption Child (Fabric)
                           WITH FBA1 AS
                        (
                            SELECT B.BookingID,B.BookingNo, B.BuyerID, ExecutionCompanyID=B.CompanyID,
							A.BookingChildID, A.ItemMasterID,A.SubGroupID,A.A1ValueID,A.YarnBrandID,ReferenceSourceID=0,YarnSourceID=0,
							ReferenceNo='',ColorReferenceNo='',A.ConsumptionID,A.ItemGroupID,
							C.Segment1Desc,C.Segment2Desc,C.Segment3Desc,
							C.Segment4Desc,C.Segment5Desc,C.Segment6Desc, C.Segment7Desc,
							A.LengthYds,A.LengthInch,A.FUPartID,A.ConsumptionQty,C.OrderUnitID,A.LabDipNo,A.Price,A.SuggestedPrice,
							RequiredQty=A.RequisitionQty,A.Remarks,ForBDSStyleNo='', B.SupplierID,LiabilitiesBookingQty=ISNULL(FBAC.LiabilitiesBookingQty,0),ActualBookingQty=ISNULL(FBAC.ActualBookingQty,0)
							FROM {DbNames.EPYSL}..BookingChild A
							LEFT JOIN {DbNames.EPYSL}..BookingMaster B ON B.BookingID=A.BookingID
							LEFT JOIN {DbNames.EPYSL}..BOMConsumption C ON C.ConsumptionID=A.ConsumptionID
                            LEFT JOIN FBookingAcknowledgeChild FBAC On FBAC.ConsumptionID = C.ConsumptionID And FBAC.ItemMasterID = A.ItemMasterID
							WHERE A.BookingID={bookingId} AND A.SubGroupID = 1
                        ),
						FBA2 AS
                        (
                           SELECT B.BookingID,B.BookingNo, B.BuyerID,B.ExecutionCompanyID,
						   BookingChildID=A.ConsumptionChildID, A.ItemMasterID,A.SubGroupID,C.A1ValueID,C.YarnBrandID,C.ReferenceSourceID,C.YarnSourceID,
						   C.ReferenceNo,C.ColorReferenceNo,A.ConsumptionID,A.ItemGroupID,
						   C.Segment1Desc,C.Segment2Desc,C.Segment3Desc,
						   C.Segment4Desc,C.Segment5Desc,C.Segment6Desc, C.Segment7Desc,
						   C.LengthYds,C.LengthInch,C.FUPartID,A.ConsumptionQty,C.OrderUnitID,C.LabDipNo,C.Price,C.SuggestedPrice,
						   A.RequiredQty,C.Remarks,C.ForBDSStyleNo, B.SupplierID,LiabilitiesBookingQty=ISNULL(FBAC.LiabilitiesBookingQty,0),ActualBookingQty=ISNULL(FBAC.ActualBookingQty,0)
						   FROM {DbNames.EPYSL}..SampleBookingConsumptionChild A
						   LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster B ON B.BookingID=A.BookingID
						   LEFT JOIN {DbNames.EPYSL}..SampleBookingConsumption C ON C.ConsumptionID=A.ConsumptionID
                           LEFT JOIN FBookingAcknowledgeChild FBAC On FBAC.ConsumptionID = C.ConsumptionID And FBAC.ItemMasterID = A.ItemMasterID
						   WHERE A.BookingID={bookingId} AND A.SubGroupID = 1
                        ),
						FBA AS
						(
							SELECT * FROM FBA1
							UNION
							SELECT * FROM FBA2
						),
						F AS (SELECT SBM.BookingChildID, SBM.ReferenceSourceID,RSETV.ValueName ReferenceSourceName , SBM.ReferenceNo,SBM.ColorReferenceNo,SBM.YarnSourceID,SBM.ConsumptionID,SBM.BookingID,SBM.BookingNo,SBM.ItemGroupID,SBM.SubGroupID,SBM.Segment1Desc Construction,SBM.Segment2Desc Composition,SBM.Segment3Desc Color
						,SBM.Segment4Desc GSM,SBM.Segment5Desc FabricWidth,SBM.Segment7Desc KnittingType,SBM.LengthYds,SBM.LengthInch,SBM.FUPartID,SBM.ConsumptionQty,SBM.OrderUnitID BookingUnitID
						,SBM.A1ValueID,SBM.YarnBrandID,SBM.LabDipNo,SBM.Price,SBM.SuggestedPrice,SBM.RequiredQty BookingQty,SBM.ItemMasterID,SBM.BuyerID ContactID,
						SBM.ExecutionCompanyID,
						0 As TechnicalNameId,ISG.SubGroupName,'1' As ConceptTypeID,IM.Segment1ValueID ConstructionId, IM.Segment2ValueID CompositionId,
						IM.Segment3ValueID ColorId,IM.Segment7ValueID KnittingTypeId,IM.Segment4ValueID GSMId,
						ISV.SegmentValue YarnType, ETV.ValueName YarnProgram, SBM.Segment6Desc DyeingType, SBM.Remarks Instruction, SBM.ForBDSStyleNo, ISNULL(Supplier.MappingCompanyID,0) TextileCompanyID
						FROM FBA SBM
                        LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = SBM.SubGroupID
                        LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = SBM.ItemMasterID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV On ISV.SegmentValueID = SBM.A1ValueID
				        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = SBM.YarnBrandID
                        Left Join {DbNames.EPYSL}..EntityTypeValue RSETV ON RSETV.ValueID = SBM.ReferenceSourceID
                        Left Join {DbNames.EPYSL}..EntityTypeValue YSETV ON YSETV.ValueID = SBM.YarnSourceID
                        LEFT Join {DbNames.EPYSL}..Contacts Supplier On Supplier.ContactID = SBM.SupplierID
                        WHERE SBM.BookingID={bookingId}
						)
						SELECT * FROM F;

                        ----- Sample Booking Consumption Child (Collor & Cuff)
                          WITH FBA1 AS
                        (
                            SELECT B.BookingID,B.BookingNo, B.BuyerID, ExecutionCompanyID=B.CompanyID,
							A.BookingChildID, A.ItemMasterID,A.SubGroupID,A.A1ValueID,A.YarnBrandID,ReferenceSourceID=0,YarnSourceID=0,
							ReferenceNo='',ColorReferenceNo='',A.ConsumptionID,A.ItemGroupID,
							C.Segment1Desc,C.Segment2Desc,C.Segment3Desc,
							C.Segment4Desc,C.Segment5Desc,C.Segment6Desc, C.Segment7Desc,
							A.LengthYds,A.LengthInch,A.FUPartID,A.ConsumptionQty,C.OrderUnitID,A.LabDipNo,A.Price,A.SuggestedPrice,
							RequiredQty=A.RequisitionQty,A.Remarks,ForBDSStyleNo='', B.SupplierID,LiabilitiesBookingQty=ISNULL(FBAC.LiabilitiesBookingQty,0),ActualBookingQty=ISNULL(FBAC.ActualBookingQty,0)
							FROM {DbNames.EPYSL}..BookingChild A
							LEFT JOIN {DbNames.EPYSL}..BookingMaster B ON B.BookingID=A.BookingID
							LEFT JOIN {DbNames.EPYSL}..BOMConsumption C ON C.ConsumptionID=A.ConsumptionID
                            LEFT JOIN FBookingAcknowledgeChild FBAC On FBAC.ConsumptionID = C.ConsumptionID And FBAC.ItemMasterID = A.ItemMasterID
							WHERE A.BookingID={bookingId} AND A.SubGroupID IN (11,12)
                        ),
						FBA2 AS
                        (
                           SELECT B.BookingID,B.BookingNo, B.BuyerID,B.ExecutionCompanyID,
						   BookingChildID=A.ConsumptionChildID, A.ItemMasterID,A.SubGroupID,C.A1ValueID,C.YarnBrandID,C.ReferenceSourceID,C.YarnSourceID,
						   C.ReferenceNo,C.ColorReferenceNo,A.ConsumptionID,A.ItemGroupID,
						   C.Segment1Desc,C.Segment2Desc,C.Segment3Desc,
						   C.Segment4Desc,C.Segment5Desc,C.Segment6Desc, C.Segment7Desc,
						   C.LengthYds,C.LengthInch,C.FUPartID,A.ConsumptionQty,C.OrderUnitID,C.LabDipNo,C.Price,C.SuggestedPrice,
						   A.RequiredQty,C.Remarks,C.ForBDSStyleNo, B.SupplierID,LiabilitiesBookingQty=ISNULL(FBAC.LiabilitiesBookingQty,0),ActualBookingQty=ISNULL(FBAC.ActualBookingQty,0)
						   FROM {DbNames.EPYSL}..SampleBookingConsumptionChild A
						   LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster B ON B.BookingID=A.BookingID
						   LEFT JOIN {DbNames.EPYSL}..SampleBookingConsumption C ON C.ConsumptionID=A.ConsumptionID
                            LEFT JOIN FBookingAcknowledgeChild FBAC On FBAC.ConsumptionID = C.ConsumptionID And FBAC.ItemMasterID = A.ItemMasterID
						   WHERE A.BookingID={bookingId} AND A.SubGroupID IN (11,12)
                        ),
						FBA AS
						(
							SELECT * FROM FBA1
							UNION
							SELECT * FROM FBA2
						),
						F AS (SELECT SBM.BookingChildID, SBM.ReferenceSourceID,RSETV.ValueName ReferenceSourceName , SBM.ReferenceNo,SBM.ColorReferenceNo,SBM.YarnSourceID,SBM.ConsumptionID,SBM.BookingID,SBM.BookingNo,SBM.ItemGroupID,SBM.SubGroupID,SBM.Segment1Desc Construction,SBM.Segment2Desc Composition,SBM.Segment3Desc Color
						,SBM.Segment4Desc GSM,SBM.Segment5Desc FabricWidth,SBM.Segment7Desc KnittingType,SBM.LengthYds,SBM.LengthInch,SBM.FUPartID,SBM.ConsumptionQty,SBM.OrderUnitID BookingUnitID
						,SBM.A1ValueID,SBM.YarnBrandID,SBM.LabDipNo,SBM.Price,SBM.SuggestedPrice,SBM.RequiredQty BookingQty,SBM.ItemMasterID,SBM.BuyerID ContactID,
						SBM.ExecutionCompanyID,
						0 As TechnicalNameId,ISG.SubGroupName,'1' As ConceptTypeID,IM.Segment1ValueID ConstructionId, IM.Segment2ValueID CompositionId,
						IM.Segment3ValueID ColorId,IM.Segment7ValueID KnittingTypeId,IM.Segment4ValueID GSMId,
						ISV.SegmentValue YarnType, ETV.ValueName YarnProgram, SBM.Segment6Desc DyeingType, SBM.Remarks Instruction, SBM.ForBDSStyleNo, ISNULL(Supplier.MappingCompanyID,0) TextileCompanyID,LiabilitiesBookingQty=ISNULL(SBM.LiabilitiesBookingQty,0),ActualBookingQty=ISNULL(SBM.ActualBookingQty,0)
						FROM FBA SBM
                        LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = SBM.SubGroupID
                        LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = SBM.ItemMasterID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV On ISV.SegmentValueID = SBM.A1ValueID
				        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = SBM.YarnBrandID
                        Left Join {DbNames.EPYSL}..EntityTypeValue RSETV ON RSETV.ValueID = SBM.ReferenceSourceID
                        Left Join {DbNames.EPYSL}..EntityTypeValue YSETV ON YSETV.ValueID = SBM.YarnSourceID
                        LEFT Join {DbNames.EPYSL}..Contacts Supplier On Supplier.ContactID = SBM.SupplierID
                        WHERE SBM.BookingID={bookingId}
						)
						SELECT * FROM F;

                       ----Sample Booking Consumption AddProcess
                        SELECT  BookingID ,ProcessID,ConsumptionID
                        FROM {DbNames.EPYSL}..SampleBookingConsumptionAddProcess where BookingID={bookingId};

                        ----- Sample Booking Consumption Child Details
                        SELECT ConsumptionID,BookingID,ItemGroupID,SubGroupID,ItemMasterID,RequiredQty BookingQty ,ConsumptionQty,RequiredUnitID BookingUnitID, 0 As TechnicalNameId
                        FROM {DbNames.EPYSL}..SampleBookingConsumptionChild where BookingID={bookingId}

                        ----- Sample Booking Consumption Garment Part
                        SELECT  BookingID ,FUPartID,ConsumptionID
                         FROM {DbNames.EPYSL}..SampleBookingConsumptionGarmentPart where BookingID={bookingId}

                        ---Sample Booking Consumption Process
                        SELECT BookingID,ProcessID,ConsumptionID
                        FROM {DbNames.EPYSL}..SampleBookingConsumptionProcess where BookingID={bookingId}

                        ------ Sample Booking Consumption Text
                        SELECT BookingID,UsesIn,AdditionalProcess,ApplicableProcess,YarnSubProgram,GarmentsColor GmtColor,ConsumptionID
                        FROM {DbNames.EPYSL}..SampleBookingConsumptionText  where BookingID={bookingId}

                        ------ Sample Booking Child Distribution 
                        SELECT *
                        FROM {DbNames.EPYSL}..SampleBookingChildDistribution  where BookingID={bookingId}

                        ----- SampleBooking ConsumptionYarn Sub Brand
                        SELECT SBKC.ReferenceSourceID,RSETV.ValueName ReferenceSourceName , SBKC.ReferenceNo,SBKC.ColorReferenceNo,SBKC.YarnSourceID,SCYS.BookingID, SCYS.YarnSubBrandID, SCYS.ConsumptionID, YSB.YarnSubBrandName
				        FROM {DbNames.EPYSL}..SampleBookingConsumptionYarnSubBrand SCYS
				        Inner Join {DbNames.EPYSL}..SampleBookingConsumption SBKC On SCYS.BookingID = SBKC.BookingID And SCYS.ConsumptionID = SBKC.ConsumptionID
                        Inner Join {DbNames.EPYSL}..ItemGroup IG On IG.ItemGroupID = SBKC.ItemGroupID
                        Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = SBKC.SubGroupID
                        Left Join {DbNames.EPYSL}..EntityTypeValue RSETV ON RSETV.ValueID = SBKC.ReferenceSourceID
                        Left Join  {DbNames.EPYSL}..EntityTypeValue YSETV ON YSETV.ValueID = SBKC.YarnSourceID
				        Inner Join (
					        Select ETV.ValueID YarnSubBrandID, ETV.ValueName YarnSubBrandName
					        From {DbNames.EPYSL}..EntityTypeValue ETV
					        Inner Join {DbNames.EPYSL}..EntityType ET On ET.EntityTypeID = ETV.EntityTypeID
				        ) YSB On YSB.YarnSubBrandID = SCYS.YarnSubBrandID
				        Where SCYS.BookingID = {bookingId};

                        ----- SampleBookingChildImage
                        SELECT BookingID,ImagePath
                        FROM {DbNames.EPYSL}..SampleBookingChildImage where BookingID={bookingId}

                        --Free Concept
                        Select SBC.ReferenceSourceID,RSETV.ValueName ReferenceSourceName , SBC.ReferenceNo,SBC.ColorReferenceNo,SBC.YarnSourceID,FB.BookingID, FB.BookingNo, FB.BookingDate ConceptDate, SBC.RequiredQty Qty,
                        CC.ItemMasterID, IM.Segment1ValueID ConstructionId, IM.Segment2ValueID CompositionId, IM.Segment3ValueID ColorId, Color.SegmentValue ColorName, IM.Segment4ValueID GSMId,
                        IM.Segment7ValueID KnittingTypeId, IM.SubGroupID,FB.ExecutionCompanyID CompanyID, ISNULL(Supplier.MappingCompanyID,0) TextileCompanyID
                        FROM {DbNames.EPYSL}..SampleBookingConsumption SBC
                        INNER JOIN {DbNames.EPYSL}..SampleBookingConsumptionChild CC ON CC.ConsumptionID = SBC.ConsumptionID
                        INNER JOIN {DbNames.EPYSL}..SampleBookingMaster FB ON FB.BookingID = SBC.BookingID
                        INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = CC.ItemMasterID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = IM.Segment1ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = IM.Segment2ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = IM.Segment3ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = IM.Segment4ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Width ON Width.SegmentValueID = IM.Segment5ValueID
                        --LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue DT ON DT.SegmentValueID = IM.Segment6ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue KT ON KT.SegmentValueID = IM.Segment7ValueID
                        --LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID = FB.SubGroupID
                        Left Join  {DbNames.EPYSL}..EntityTypeValue RSETV ON RSETV.ValueID = SBC.ReferenceSourceID
                        Left Join  {DbNames.EPYSL}..EntityTypeValue YSETV ON YSETV.ValueID = SBC.YarnSourceID
                        LEFT Join {DbNames.EPYSL}..Contacts Supplier On Supplier.ContactID = FB.SupplierID
                        where FB.BookingID={bookingId};

                        --Technical Name
                        SELECT Cast(T.TechnicalNameId As varchar) id, T.TechnicalName [text], ISNULL(ST.[Days], 0) [desc], Cast(SC.SubClassID as varchar) additionalValue
                        FROM FabricTechnicalName T
                        LEFT JOIN FabricTechnicalNameKMachineSubClass SC ON SC.TechnicalNameID = T.TechnicalNameId
                        LEFT JOIN KnittingMachineStructureType_HK ST ON ST.StructureTypeID = SC.StructureTypeID
                        Group By T.TechnicalNameId, T.TechnicalName, ST.Days, SC.SubClassID;

                         --YarnSource data load
				        Select Cast(a.ValueID as varchar) id, a.ValueName [text]
				        From {DbNames.EPYSL}..EntityTypeValue a
				        Inner Join {DbNames.EPYSL}..EntityType b on b.EntityTypeID = a.EntityTypeID
				        Where b.EntityTypeName = 'Yarn Source'

                        --M/c type
                        ;SELECT CAST(a.MachineSubClassID AS varchar) [id], b.SubClassName [text], b.TypeID [desc], c.TypeName additionalValue
                        FROM KnittingMachine a
                        INNER JOIN KnittingMachineSubClass b ON b.SubClassID = a.MachineSubClassID
                        Inner Join KnittingMachineType c On c.TypeID = b.TypeID
                        --Where c.TypeName != 'Flat Bed'
                        GROUP BY a.MachineSubClassID, b.SubClassName, b.TypeID, c.TypeName;

                        --CriteriaNames
                         ;SELECT CriteriaName,CriteriaSeqNo, (CASE WHEN CriteriaName  IN('Batch Preparation','Quality Check') THEN '1'ELSE'0'END) AS TotalTime
                        FROM BDSCriteria_HK --WHERE CriteriaName NOT IN('Batch Preparation','Testing')
                        GROUP BY CriteriaSeqNo,CriteriaName order by CriteriaSeqNo,CriteriaName;

                        --FBAChildPlannings
                        ;SELECT * FROM BDSCriteria_HK order by CriteriaSeqNo, OperationSeqNo, CriteriaName;
                        
                        --Liability Process
				        Select LChildID = 0,BookingChildID = 0,AcknowledgeID = 0,BookingID = 0,UnitID = 0, Cast(a.ValueID as varchar) LiabilitiesProcessID, a.ValueName LiabilitiesName,LiabilityQty=0
				        From {DbNames.EPYSL}..EntityTypeValue a
				        Inner Join {DbNames.EPYSL}..EntityType b on b.EntityTypeID = a.EntityTypeID
				        Where b.EntityTypeName = 'Process Liability'
                        
                        --Liability Process data load
				        Select LChildID = IsNull(F.LChildID,0),BookingChildID = IsNull(F.BookingChildID,0),AcknowledgeID = IsNull(F.AcknowledgeID,0),BookingID = IsNull(F.BookingID,0),UnitID = IsNull(F.UnitID,0), Cast(a.ValueID as varchar) LiabilitiesProcessID, a.ValueName LiabilitiesName,LiabilityQty=IsNull(F.LiabilityQty,0)
				        From {DbNames.EPYSL}..EntityTypeValue a
				        Inner Join {DbNames.EPYSL}..EntityType b on b.EntityTypeID = a.EntityTypeID
				        Left Join (Select LChildID,BookingChildID,AcknowledgeID,BookingID,LiabilitiesProcessID,UnitID,LiabilityQty From FBookingAcknowledgementLiabilityDistribution Where BookingID = {bookingId} Group By LChildID,BookingChildID,AcknowledgeID,BookingID,LiabilitiesProcessID,UnitID,LiabilityQty)F On F.LiabilitiesProcessID = a.ValueID
				        Where b.EntityTypeName = 'Process Liability'";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                FBookingAcknowledge data = records.Read<FBookingAcknowledge>().FirstOrDefault();
                Guard.Against.NullObject(data);

                List<FBookingAcknowledgeChild> bookingChilds = records.Read<FBookingAcknowledgeChild>().ToList();
                List<FBookingAcknowledgeChild> bookingChildsCollarCuff = records.Read<FBookingAcknowledgeChild>().ToList();

                data.FBookingAcknowledgeChildAddProcess = records.Read<FBookingAcknowledgeChildAddProcess>().ToList();
                data.FBookingChildDetails = records.Read<FBookingAcknowledgeChildDetails>().ToList();
                data.FBookingAcknowledgeChildGarmentPart = records.Read<FBookingAcknowledgeChildGarmentPart>().ToList();
                data.FBookingAcknowledgeChildProcess = records.Read<FBookingAcknowledgeChildProcess>().ToList();
                data.FBookingAcknowledgeChildText = records.Read<FBookingAcknowledgeChildText>().ToList();
                data.FBookingAcknowledgeChildDistribution = records.Read<FBookingAcknowledgeChildDistribution>().ToList();
                data.FBookingAcknowledgeChildYarnSubBrand = records.Read<FBookingAcknowledgeChildYarnSubBrand>().ToList();
                data.FBookingAcknowledgeImage = records.Read<FBookingAcknowledgeImage>().ToList();
                data.FreeConcepts = records.Read<FreeConceptMaster>().ToList();
                data.TechnicalNameList = await records.ReadAsync<Select2OptionModel>();
                data.YarnSourceNameList = await records.ReadAsync<Select2OptionModel>();

                List<Select2OptionModel> mcTypeList = records.Read<Select2OptionModel>().ToList();
                data.MCTypeForFabricList = mcTypeList.Where(x => x.additionalValue != "Flat Bed");
                data.MCTypeForOtherList = mcTypeList.Where(x => x.additionalValue == "Flat Bed");

                List<FBookingAcknowledgeChild> criteriaNames = records.Read<FBookingAcknowledgeChild>().ToList();
                List<FBAChildPlanning> fbaChildPlannings = records.Read<FBAChildPlanning>().ToList();
                List<FBookingAcknowledgementLiabilityDistribution> LiabilityDistributionList = records.Read<FBookingAcknowledgementLiabilityDistribution>().ToList();
                List<FBookingAcknowledgementLiabilityDistribution> FBookingAckLiabilityDistributionList = records.Read<FBookingAcknowledgementLiabilityDistribution>().ToList();
                //criteriaNames.ForEach(cn =>
                //{
                //    cn.FBAChildPlannings = fbaChildPlannings.Where(x => x.CriteriaName == cn.CriteriaName).ToList();
                //});

                List<FBookingAcknowledgementLiabilityDistribution> tmpLB = new List<FBookingAcknowledgementLiabilityDistribution>();
                for (int i = 0; i < bookingChilds.Count; i++)
                {
                    tmpLB = new List<FBookingAcknowledgementLiabilityDistribution>();
                    tmpLB.AddRange(LiabilityDistributionList);
                    bookingChilds[i].CriteriaNames = criteriaNames;
                    bookingChilds[i].FBAChildPlannings = fbaChildPlannings;
                    bookingChilds[i].ChildAckLiabilityDetails = tmpLB.ConvertAll(x => new FBookingAcknowledgementLiabilityDistribution(x)).ToList();

                    foreach (FBookingAcknowledgementLiabilityDistribution obitem in bookingChilds[i].ChildAckLiabilityDetails)
                    {
                        List<FBookingAcknowledgementLiabilityDistribution> objLBList = FBookingAckLiabilityDistributionList.Where(j => j.BookingChildID == bookingChilds[i].BookingChildID && j.LiabilitiesProcessID == obitem.LiabilitiesProcessID).ToList();
                        foreach (FBookingAcknowledgementLiabilityDistribution objLB in objLBList)
                        {
                            if (objLB.IsNotNull())
                            {
                                obitem.LChildID = objLB.LChildID;
                                obitem.BookingChildID = objLB.BookingChildID;
                                obitem.AcknowledgeID = objLB.AcknowledgeID;
                                obitem.BookingID = objLB.BookingID;
                                obitem.UnitID = objLB.UnitID;
                                obitem.LiabilityQty = objLB.LiabilityQty;
                            }
                        }

                    }
                }

                tmpLB = new List<FBookingAcknowledgementLiabilityDistribution>();
                for (int i = 0; i < bookingChildsCollarCuff.Count; i++)
                {
                    tmpLB = new List<FBookingAcknowledgementLiabilityDistribution>();
                    tmpLB.AddRange(LiabilityDistributionList);
                    bookingChilds[i].CriteriaNames = criteriaNames;
                    bookingChilds[i].FBAChildPlannings = fbaChildPlannings;
                    bookingChilds[i].ChildAckLiabilityDetails = tmpLB.ConvertAll(x => new FBookingAcknowledgementLiabilityDistribution(x)).ToList();

                    foreach (FBookingAcknowledgementLiabilityDistribution obitem in bookingChilds[i].ChildAckLiabilityDetails)
                    {
                        List<FBookingAcknowledgementLiabilityDistribution> objLBList = FBookingAckLiabilityDistributionList.Where(j => j.BookingChildID == bookingChilds[i].BookingChildID && j.LiabilitiesProcessID == obitem.LiabilitiesProcessID).ToList();
                        foreach (FBookingAcknowledgementLiabilityDistribution objLB in objLBList)
                        {
                            if (objLB.IsNotNull())
                            {
                                obitem.LChildID = objLB.LChildID;
                                obitem.BookingChildID = objLB.BookingChildID;
                                obitem.AcknowledgeID = objLB.AcknowledgeID;
                                obitem.BookingID = objLB.BookingID;
                                obitem.UnitID = objLB.UnitID;
                                obitem.LiabilityQty = objLB.LiabilityQty;
                            }
                        }

                    }
                }

                data.FBookingChild = bookingChilds.Where(x => x.SubGroupID == 1).ToList();
                data.FBookingChildCollor = bookingChildsCollarCuff.Where(x => x.SubGroupID == 11).ToList();
                data.FBookingChildCuff = bookingChildsCollarCuff.Where(x => x.SubGroupID == 12).ToList();
                data.HasFabric = data.FBookingChild.Count() > 0 ? true : false;
                data.HasCollar = data.FBookingChildCollor.Count() > 0 ? true : false;
                data.HasCuff = data.FBookingChildCuff.Count() > 0 ? true : false;

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
        public async Task<FBookingAcknowledge> GetNewBulkFAsync(string bookingNo)
        {
            var query = $@"
            -- Master Booking

            WITH 
            Booking As 
            (
	            Select BM.BookingNo, BuyerName = CTO.ShortName,
                BuyerTeamName = CCT.TeamName, StyleNo = SM.StyleNo,
                BM.BookingDate, CompanyName = C.CompanyName,
                BookingQty = 0,
                S.SeasonName, SupplierName = SP.ShortName,SM.SeasonID,
                Remarks = BM.Remarks, IsSample = 0,
	            GmtQtyPcs = Sum(OBPO.TotalPOQty)
                From {DbNames.EPYSL}..BookingMaster BM
	            LEFT JOIN {DbNames.EPYSL}..ExportOrderMaster EOM ON EOM.ExportOrderID = BM.ExportOrderID
	            LEFT JOIN {DbNames.EPYSL}..StyleMaster SM ON SM.StyleMasterID = EOM.StyleMasterID
                LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = BM.BuyerID
                LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = BM.BuyerTeamID
                LEFT JOIN {DbNames.EPYSL}..CompanyEntity C ON C.CompanyID = BM.CompanyID
                LEFT Join {DbNames.EPYSL}..Contacts SP On SP.ContactID = BM.SupplierID
                LEFT Join {DbNames.EPYSL}..ContactSeason S On S.SeasonID = SM.SeasonID
	            LEFT JOIN {DbNames.EPYSL}..OrderBankMaster OBM On OBM.StyleMasterID = EOM.StyleMasterID
	            LEFT JOIN {DbNames.EPYSL}..OrderBankPO OBPO On OBPO.OrderBankMasterID = OBM.OrderBankMasterID AND OBPO.IsActive = 1
                Where BM.BookingNo = '{bookingNo}'
                GROUP BY BM.BookingNo,CTO.ShortName,CCT.TeamName,
                BM.BookingDate, C.CompanyName, SP.ShortName, BM.Remarks,S.SeasonName,SM.SeasonID,
	            SM.StyleNo

                UNION

                Select BM.BookingNo, BuyerName = CTO.ShortName,
                BuyerTeamName = CCT.TeamName, StyleNo = SM.StyleNo,
                BM.BookingDate, CompanyName = C.CompanyName,
                BookingQty = 0,
                S.SeasonName, SupplierName = SP.ShortName,BM.SeasonID,
                Remarks = BM.Remarks, IsSample = 1,
	            GmtQtyPcs = Sum(OBPO.TotalPOQty)
                From {DbNames.EPYSL}..SampleBookingMaster BM
	            LEFT JOIN {DbNames.EPYSL}..StyleMaster SM ON SM.StyleMasterID = BM.StyleMasterID
                LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = BM.BuyerID
                LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = BM.BuyerTeamID
                LEFT JOIN {DbNames.EPYSL}..CompanyEntity C ON C.CompanyID = BM.ExecutionCompanyID
                LEFT Join {DbNames.EPYSL}..Contacts SP On SP.ContactID = BM.SupplierID
                LEFT Join {DbNames.EPYSL}..ContactSeason S On S.SeasonID = BM.SeasonID
	            LEFT JOIN {DbNames.EPYSL}..OrderBankMaster OBM On OBM.StyleMasterID = SM.StyleMasterID
	            LEFT JOIN {DbNames.EPYSL}..OrderBankPO OBPO On OBPO.OrderBankMasterID = OBM.OrderBankMasterID AND OBPO.IsActive = 1
                Where BM.BookingNo = '{bookingNo}'
	            GROUP BY BM.BookingNo, CTO.ShortName,
                CCT.TeamName, SM.StyleNo,
                BM.BookingDate, C.CompanyName,
                S.SeasonName, SP.ShortName,BM.SeasonID,
                BM.Remarks
            )
            SELECT Bk.BookingNo, Bk.BuyerName,
                Bk.BuyerTeamName, Bk.StyleNo,
                Bk.BookingDate, Bk.CompanyName,
                Bk.BookingQty, Bk.GmtQtyPcs,
                Bk.SeasonName, Bk.SupplierName,
                Bk.Remarks,Bk.IsSample,BK.SeasonID,
	            BK.GmtQtyPcs
            FROM Booking BK;


            --Booking Child (Fabric)
            Select PartName = EPYSL.dbo.fnBookingChildGarmentPart(BC.BOMMasterID,BC.ConsumptionID), BC.*, Instruction = BC.Remarks, Construction = ISV1.SegmentValue, 
	            Composition = ISV2.SegmentValue,
	            Color = CASE WHEN BC.SubGroupID = 1 THEN ISV3.SegmentValue ELSE ISV5.SegmentValue END, 
	            Gsm = CASE WHEN BC.SubGroupID = 1 THEN ISV4.SegmentValue ELSE '' END, 
	            DyeingType = CASE WHEN BC.SubGroupID = 1 THEN ISV6.SegmentValue ELSE '' END, 
	            KnittingType = CASE WHEN BC.SubGroupID = 1 THEN ISV7.SegmentValue ELSE '' END,
	            Length = CASE WHEN BC.SubGroupID = 1 THEN 0 ELSE CONVERT(decimal(18,2),ISV3.SegmentValue) END,
	            FabricWidth = CASE WHEN BC.SubGroupID = 1 THEN ISV5.SegmentValue ELSE CONVERT(decimal(18,2),ISV4.SegmentValue) END,
                RefSourceNo = CASE WHEN ISNULL(RS.RefSourceNo,'') = '' THEN 'New Item' ELSE RS.RefSourceNo END,
				BookingUOM = BU.DisplayUnitDesc
            From {DbNames.EPYSL}..BookingChild BC
            INNER JOIN {DbNames.EPYSL}..BookingMaster BM ON BM.BookingID = BC.BookingID
            LEFT JOIN {DbNames.EPYSL}..BookingChildReferenceSource RS ON RS.BookingID = BM.BookingID AND RS.ConsumptionID = BC.ConsumptionID
			LEFT JOIN {DbNames.EPYSL}..Unit BU On BU.UnitID = BC.BookingUnitID
            INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = BC.ItemMasterID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
            Where BM.BookingNo = '{bookingNo}' AND BC.SubGroupID = 1;

            --Sample Booking Child (Fabric)
            ;With PartName As(
				Select SCYS.ConsumptionID,SCYS.BookingID, PartName = STRING_AGG(YSB.PartName,',')
				From {DbNames.EPYSL}..SampleBookingConsumptionGarmentPart SCYS
				Inner Join {DbNames.EPYSL}..SampleBookingConsumption SBKC On SCYS.BookingID = SBKC.BookingID And SCYS.ConsumptionID = SBKC.ConsumptionID
				Inner Join {DbNames.EPYSL}..ItemGroup IG On IG.ItemGroupID = SBKC.ItemGroupID
				Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = SBKC.SubGroupID
				Inner Join {DbNames.EPYSL}..FabricUsedPart YSB On YSB.FUPartID = SCYS.FUPartID
				Inner Join {DbNames.EPYSL}..SampleBookingMaster BM ON BM.BookingID = SCYS.BookingID
				Where BM.BookingNo = '{bookingNo}' And ISG.SubGroupID = 1
				GROUP BY SCYS.ConsumptionID,SCYS.BookingID
			)
            Select  PN.PartName,BC.*, Instruction = SBC.Remarks, Construction = ISV1.SegmentValue, 
	            Composition = ISV2.SegmentValue,
	            Color = CASE WHEN BC.SubGroupID = 1 THEN ISV3.SegmentValue ELSE ISV5.SegmentValue END, 
	            Gsm = CASE WHEN BC.SubGroupID = 1 THEN ISV4.SegmentValue ELSE '' END, 
	            DyeingType = CASE WHEN BC.SubGroupID = 1 THEN ISV6.SegmentValue ELSE '' END, 
	            KnittingType = CASE WHEN BC.SubGroupID = 1 THEN ISV7.SegmentValue ELSE '' END,
	            Length = CASE WHEN BC.SubGroupID = 1 THEN 0 ELSE CONVERT(decimal(18,2),ISV3.SegmentValue) END,
	            FabricWidth = CASE WHEN BC.SubGroupID = 1 THEN ISV5.SegmentValue ELSE CONVERT(decimal(18,2),ISV4.SegmentValue) END,
                RefSourceNo = CASE WHEN ISNULL(RS.RefSourceNo,'') = '' THEN 'New Item' ELSE RS.RefSourceNo END,
				BookingUOM = BU.DisplayUnitDesc,
                BookingQty = BC.RequiredQty
            From {DbNames.EPYSL}..SampleBookingConsumptionChild BC
            INNER JOIN {DbNames.EPYSL}..SampleBookingConsumption SBC ON SBC.ConsumptionID = BC.ConsumptionID
            INNER JOIN {DbNames.EPYSL}..SampleBookingMaster BM ON BM.BookingID = BC.BookingID
			LEFT JOIN {DbNames.EPYSL}..Unit BU On BU.UnitID = BC.RequiredUnitID
            LEFT JOIN {DbNames.EPYSL}..BookingChildReferenceSource RS ON RS.BookingID = BM.BookingID AND RS.ConsumptionID = SBC.ConsumptionID
            INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = BC.ItemMasterID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
            Left Join PartName PN On PN.ConsumptionID = BC.ConsumptionID And PN.BookingID = BC.BookingID
            Where BM.BookingNo = '{bookingNo}' AND BC.SubGroupID = 1;
            
            --Booking Child (Collar & Cuff)
            Select BC.*, Instruction = BC.Remarks, 
	            Segment1Desc = ISV1.SegmentValue,
	            Segment2Desc = ISV2.SegmentValue,
	            Segment3Desc = ISV3.SegmentValue,
	            Segment4Desc = ISV4.SegmentValue,
	            Segment5Desc = ISV5.SegmentValue,
	            Segment6Desc = ISV6.SegmentValue,
	            Segment7Desc = ISV7.SegmentValue,
                RefSourceNo = CASE WHEN ISNULL(RS.RefSourceNo,'') = '' THEN 'New Item' ELSE RS.RefSourceNo END,
				BookingUOM = BU.DisplayUnitDesc, YarnType = ISV.SegmentValue, YarnProgram = ETV.ValueName
            From {DbNames.EPYSL}..BookingChild BC
            INNER JOIN {DbNames.EPYSL}..BookingMaster BM ON BM.BookingID = BC.BookingID
            LEFT JOIN {DbNames.EPYSL}..BookingChildReferenceSource RS ON RS.BookingID = BM.BookingID AND RS.ConsumptionID = BC.ConsumptionID
			LEFT JOIN {DbNames.EPYSL}..Unit BU On BU.UnitID = BC.BookingUnitID
			LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV On ISV.SegmentValueID = BC.A1ValueID
			LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = BC.YarnBrandID
            INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = BC.ItemMasterID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
            Where BM.BookingNo = '{bookingNo}' AND BC.SubGroupID IN (11,12);

            --Sample Booking Child (Collar & Cuff)
            Select BC.*, Instruction = SBC.Remarks, 
	            Segment1Desc = ISV1.SegmentValue,
	            Segment2Desc = ISV2.SegmentValue,
	            Segment3Desc = ISV3.SegmentValue,
	            Segment4Desc = ISV4.SegmentValue,
	            Segment5Desc = ISV5.SegmentValue,
	            Segment6Desc = ISV6.SegmentValue,
	            Segment7Desc = ISV7.SegmentValue,
                RefSourceNo = CASE WHEN ISNULL(RS.RefSourceNo,'') = '' THEN 'New Item' ELSE RS.RefSourceNo END,
				BookingUOM = BU.DisplayUnitDesc, YarnType = ISV.SegmentValue, YarnProgram = ETV.ValueName,
                BookingQty = BC.RequiredQty
            From {DbNames.EPYSL}..SampleBookingConsumptionChild BC
            INNER JOIN {DbNames.EPYSL}..SampleBookingConsumption SBC ON SBC.ConsumptionID = BC.ConsumptionID
            INNER JOIN {DbNames.EPYSL}..SampleBookingMaster BM ON BM.BookingID = BC.BookingID
			LEFT JOIN {DbNames.EPYSL}..Unit BU On BU.UnitID = BC.RequiredUnitID
			LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV On ISV.SegmentValueID = SBC.A1ValueID
			LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = SBC.YarnBrandID
            INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = BC.ItemMasterID
            LEFT JOIN {DbNames.EPYSL}..BookingChildReferenceSource RS ON RS.BookingID = BM.BookingID AND RS.ConsumptionID = SBC.ConsumptionID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
            Where BM.BookingNo = '{bookingNo}' AND BC.SubGroupID IN (11,12);
            ";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                FBookingAcknowledge data = records.Read<FBookingAcknowledge>().FirstOrDefault();

                List<FBookingAcknowledgeChild> bookingChildF = records.Read<FBookingAcknowledgeChild>().ToList();
                List<FBookingAcknowledgeChild> sampleBookingChildF = records.Read<FBookingAcknowledgeChild>().ToList();

                List<FBookingAcknowledgeChild> bookingChildCC = records.Read<FBookingAcknowledgeChild>().ToList();
                List<FBookingAcknowledgeChild> sampleBookingChildCC = records.Read<FBookingAcknowledgeChild>().ToList();

                if (data.IsSample)
                {
                    data.HasFabric = sampleBookingChildF.Count() > 0 ? true : false;
                    data.HasCollar = sampleBookingChildCC.Count(x => x.SubGroupID == 11) > 0 ? true : false;
                    data.HasCuff = sampleBookingChildCC.Count(x => x.SubGroupID == 12) > 0 ? true : false;

                    data.FBookingChild = sampleBookingChildF.Where(x => x.SubGroupID == 1).ToList();
                    data.BookingQty = Convert.ToInt32(data.FBookingChild.Sum(x => x.BookingQty));
                    data.FBookingChildCollor = sampleBookingChildCC.Where(x => x.SubGroupID == 11).ToList();
                    data.FBookingChildCuff = sampleBookingChildCC.Where(x => x.SubGroupID == 12).ToList();
                }
                else
                {
                    data.HasFabric = bookingChildF.Count() > 0 ? true : false;
                    data.HasCollar = bookingChildCC.Count(x => x.SubGroupID == 11) > 0 ? true : false;
                    data.HasCuff = bookingChildCC.Count(x => x.SubGroupID == 12) > 0 ? true : false;

                    data.FBookingChild = bookingChildF.Where(x => x.SubGroupID == 1).ToList();
                    data.BookingQty = Convert.ToInt32(data.FBookingChild.Sum(x => x.BookingQty));
                    data.FBookingChildCollor = bookingChildCC.Where(x => x.SubGroupID == 11).ToList();
                    data.FBookingChildCuff = bookingChildCC.Where(x => x.SubGroupID == 12).ToList();
                }

                /*
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                List<FabricBookingAcknowledge> fbaList = records.Read<FabricBookingAcknowledge>().ToList();
                FBookingAcknowledge data = records.Read<FBookingAcknowledge>().FirstOrDefault();
                data.IsSample = fbaList.Count() > 0 ? fbaList.First().IsSample : false;
                data.FBookingAcknowledgeList = records.Read<FBookingAcknowledge>().ToList();
                Guard.Against.NullObject(data);

                List<FBookingAcknowledgeChild> bookingChilds = records.Read<FBookingAcknowledgeChild>().ToList();
                List<FBookingAcknowledgeChild> bookingChildsCollarCuff = records.Read<FBookingAcknowledgeChild>().ToList();
                data.FabricBookingAcknowledgeList = fbaList;
                data.FBookingAcknowledgeChildAddProcess = records.Read<FBookingAcknowledgeChildAddProcess>().ToList();
                data.FBookingChildDetails = records.Read<FBookingAcknowledgeChildDetails>().ToList();
                data.FBookingAcknowledgeChildGarmentPart = records.Read<FBookingAcknowledgeChildGarmentPart>().ToList();
                data.FBookingAcknowledgeChildProcess = records.Read<FBookingAcknowledgeChildProcess>().ToList();
                data.FBookingAcknowledgeChildText = records.Read<FBookingAcknowledgeChildText>().ToList();
                data.FBookingAcknowledgeChildDistribution = records.Read<FBookingAcknowledgeChildDistribution>().ToList();
                data.FBookingAcknowledgeChildYarnSubBrand = records.Read<FBookingAcknowledgeChildYarnSubBrand>().ToList();
                data.FBookingAcknowledgeImage = records.Read<FBookingAcknowledgeImage>().ToList();
                data.FreeConcepts = records.Read<FreeConceptMaster>().ToList();
                data.TechnicalNameList = await records.ReadAsync<Select2OptionModel>();
                data.YarnSourceNameList = await records.ReadAsync<Select2OptionModel>();

                List<Select2OptionModel> mcTypeList = records.Read<Select2OptionModel>().ToList();
                data.MCTypeForFabricList = mcTypeList.Where(x => x.additionalValue != "Flat Bed");
                data.MCTypeForOtherList = mcTypeList.Where(x => x.additionalValue == "Flat Bed");

                List<FBookingAcknowledgeChild> criteriaNames = records.Read<FBookingAcknowledgeChild>().ToList();
                List<FBAChildPlanning> fbaChildPlannings = records.Read<FBAChildPlanning>().ToList();
                List<FBookingAcknowledgementLiabilityDistribution> LiabilityDistributionList = records.Read<FBookingAcknowledgementLiabilityDistribution>().ToList();
                List<FBookingAcknowledgementLiabilityDistribution> FBookingAckLiabilityDistributionList = records.Read<FBookingAcknowledgementLiabilityDistribution>().ToList();
                data.FBookingAcknowledgementYarnLiabilityList = records.Read<FBookingAcknowledgementYarnLiability>().ToList();


                List<FBookingAcknowledgementLiabilityDistribution> tmpLB = new List<FBookingAcknowledgementLiabilityDistribution>();
                for (int i = 0; i < bookingChilds.Count; i++)
                {
                    tmpLB = new List<FBookingAcknowledgementLiabilityDistribution>();
                    tmpLB.AddRange(LiabilityDistributionList);
                    bookingChilds[i].CriteriaNames = criteriaNames;
                    bookingChilds[i].FBAChildPlannings = fbaChildPlannings;
                    bookingChilds[i].ChildAckLiabilityDetails = tmpLB.ConvertAll(x => new FBookingAcknowledgementLiabilityDistribution(x)).ToList();

                    foreach (FBookingAcknowledgementLiabilityDistribution obitem in bookingChilds[i].ChildAckLiabilityDetails)
                    {
                        List<FBookingAcknowledgementLiabilityDistribution> objLBList = FBookingAckLiabilityDistributionList.Where(j => j.BookingChildID == bookingChilds[i].BookingChildID && j.LiabilitiesProcessID == obitem.LiabilitiesProcessID).ToList();
                        foreach (FBookingAcknowledgementLiabilityDistribution objLB in objLBList)
                        {
                            if (objLB.IsNotNull())
                            {
                                obitem.LChildID = objLB.LChildID;
                                obitem.BookingChildID = objLB.BookingChildID;
                                obitem.AcknowledgeID = objLB.AcknowledgeID;
                                obitem.BookingID = objLB.BookingID;
                                obitem.UnitID = objLB.UnitID;
                                obitem.LiabilityQty = objLB.LiabilityQty;
                            }
                        }
                    }
                }

                tmpLB = new List<FBookingAcknowledgementLiabilityDistribution>();
                for (int i = 0; i < bookingChilds.Count; i++)
                {
                    tmpLB = new List<FBookingAcknowledgementLiabilityDistribution>();
                    tmpLB.AddRange(LiabilityDistributionList);
                    bookingChilds[i].CriteriaNames = criteriaNames;
                    bookingChilds[i].FBAChildPlannings = fbaChildPlannings;
                    bookingChilds[i].ChildAckLiabilityDetails = tmpLB.ConvertAll(x => new FBookingAcknowledgementLiabilityDistribution(x)).ToList();

                    foreach (FBookingAcknowledgementLiabilityDistribution obitem in bookingChilds[i].ChildAckLiabilityDetails)
                    {
                        List<FBookingAcknowledgementLiabilityDistribution> objLBList = FBookingAckLiabilityDistributionList.Where(j => j.BookingChildID == bookingChilds[i].BookingChildID && j.LiabilitiesProcessID == obitem.LiabilitiesProcessID).ToList();
                        foreach (FBookingAcknowledgementLiabilityDistribution objLB in objLBList)
                        {
                            if (objLB.IsNotNull())
                            {
                                obitem.LChildID = objLB.LChildID;
                                obitem.BookingChildID = objLB.BookingChildID;
                                obitem.AcknowledgeID = objLB.AcknowledgeID;
                                obitem.BookingID = objLB.BookingID;
                                obitem.UnitID = objLB.UnitID;
                                obitem.LiabilityQty = objLB.LiabilityQty;
                            }
                        }

                    }
                }

                data.FBookingChild = bookingChilds.Where(x => x.SubGroupID == 1).ToList();
                data.FBookingChildCollor = bookingChildsCollarCuff.Where(x => x.SubGroupID == 11).ToList();
                data.FBookingChildCuff = bookingChildsCollarCuff.Where(x => x.SubGroupID == 12).ToList();
                data.HasFabric = data.FBookingChild.Count() > 0 ? true : false;
                data.HasCollar = data.FBookingChildCollor.Count() > 0 ? true : false;
                data.HasCuff = data.FBookingChildCuff.Count() > 0 ? true : false;


                */

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
        /*
        public async Task<FBookingAcknowledge> GetNewBulkFAsync(string bookingNo)
        {
            var query = $@"
                WITH 
            Bkk As 
            (
	            Select BookingNo From {DbNames.EPYSL}..BookingMaster Where BookingID = {bookingId}
	            Union
	            Select BookingNo From {DbNames.EPYSL}..SampleBookingMaster Where BookingID = {bookingId}
            ),
            BKKK As 
            (
	            Select BookingID From {DbNames.EPYSL}..BookingMaster Where BookingNo in (Select BookingNo From Bkk)
	            Union
	            Select BookingID From {DbNames.EPYSL}..SampleBookingMaster Where BookingNo in (Select BookingNo From Bkk)
            ),
            FBA1 AS
            (
	            SELECT B.BookingID,A.BOMMasterID,BC.ItemGroupID,BC.SubGroupID,a.Status,PreRevisionNo=a.PreProcessRevNo,
	            b.BookingNo,b.BookingDate,b.Remarks,b.ExportOrderID,SLNo='',
	            SM.StyleMasterID, SM.StyleNo, SM.SeasonID, SM.FinancialYearID,
	            b.BuyerID,b.BuyerTeamID,b.CompanyID,b.SupplierID,WithoutOB=0,BookingQty= SUM(BC.BookingQty), IsSample = 0
	            FROM {DbNames.EPYSL}..BookingMaster B
	            Inner Join {DbNames.EPYSL}..BookingChild BC On BC.BookingID = B.BookingID
	            LEFT JOIN FabricBookingAcknowledge A on b.BookingID = a.BookingID
	            LEFT JOIN {DbNames.EPYSL}..ExportOrderMaster EO ON EO.ExportOrderID = B.ExportOrderID
	            LEFT JOIN {DbNames.EPYSL}..StyleMaster SM ON SM.StyleMasterID = EO.StyleMasterID
	            WHERE B.BookingNo in (Select BookingNo From BKK)
	            Group By B.BookingID,A.BOMMasterID,BC.ItemGroupID,BC.SubGroupID,a.Status,a.PreProcessRevNo,
	            b.BookingNo,b.BookingDate,b.Remarks,b.ExportOrderID,
	            SM.StyleMasterID, SM.StyleNo, SM.SeasonID, SM.FinancialYearID,
	            b.BuyerID,b.BuyerTeamID,b.CompanyID,b.SupplierID,BC.BookingQty
            ),
            FBA2 AS
            (
	            SELECT Distinct b.BookingID,a.BOMMasterID,SBIG.ItemGroupID,SBIG.SubGroupID,a.Status,PreRevisionNo=a.PreProcessRevNo,
	            b.BookingNo,b.BookingDate,b.Remarks,b.ExportOrderID,b.SLNo,
	            b.StyleMasterID, StyleNo = CASE WHEN ISNULL(b.StyleMasterID,0) = 0 THEN b.StyleNo ELSE SM.StyleNo END, b.SeasonID, 
	            FinancialYearID = (Select top 1 FY.FinancialYearID From {DbNames.EPYSL}..FinancialYear FY Where b.BookingDate between FY.StartMonth and FY.EndMonth),
	            b.BuyerID,b.BuyerTeamID,CompanyID=b.ExecutionCompanyID,b.SupplierID,WithoutOB=1,BookingQty= SUM(c.RequiredQty), IsSample = 1
	            FROM {DbNames.EPYSL}..SampleBookingMaster b
	            Inner Join {DbNames.EPYSL}..SampleBookingItemGroup SBIG On SBIG.BookingID = b.BookingID
	            Inner Join {DbNames.EPYSL}..SampleBookingConsumptionChild c ON c.BookingID = b.BookingID
	            LEFT Join FabricBookingAcknowledge A on b.BookingID = a.BookingID
	            LEFT JOIN {DbNames.EPYSL}..StyleMaster SM ON SM.StyleMasterID = b.StyleMasterID
	            WHERE B.BookingNo in (Select BookingNo From BKK)
	            Group By b.BookingID,a.BOMMasterID,SBIG.ItemGroupID,SBIG.SubGroupID,a.Status,a.PreProcessRevNo,
	            b.BookingNo,b.BookingDate,b.Remarks,b.ExportOrderID,b.SLNo,
	            b.StyleMasterID,b.StyleMasterID,b.StyleNo,SM.StyleNo, b.SeasonID,
	            b.BuyerID,b.BuyerTeamID,b.ExecutionCompanyID,b.SupplierID
            ),
            FBA AS
            (
	            SELECT *FROM FBA1
	            UNION
	            SELECT *FROM FBA2
            ),
            F AS(
	            SELECT FBA.BookingID, FBA.BOMMasterID, FBA.ItemGroupID, FBA.SubGroupID, FBA.Status, FBA.PreRevisionNo, FBA.BookingNo, FBA.BookingDate, FBA.Remarks, FBA.ExportOrderID, FBA.SLNo, FBA.StyleMasterID, FBA.StyleNo, FBA.SeasonID, FBA.FinancialYearID, FBA.BuyerID, FBA.BuyerTeamID, FBA.CompanyID, FBA.SupplierID, FBA.WithoutOB, BookingQty
	            ,CTO.ContactDisplayCode AS BuyerName, CCT.DisplayCode AS BuyerTeamName,CompanyName = C.ShortName,
	            Supplier.ShortName [SupplierName],Season.SeasonName, ISNULL(Supplier.MappingCompanyID,0) TextileCompanyID,FBA.IsSample
	            FROM FBA
	            LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = FBA.BuyerID
	            LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = FBA.BuyerTeamID
	            LEFT JOIN {DbNames.EPYSL}..CompanyEntity C ON C.CompanyID = FBA.CompanyID
	            LEFT Join {DbNames.EPYSL}..Contacts Supplier On Supplier.ContactID = FBA.SupplierID
	            LEFT Join {DbNames.EPYSL}..ContactSeason Season On Season.SeasonID = FBA.SeasonID
	            Group By FBA.BookingID, FBA.BOMMasterID, FBA.ItemGroupID, FBA.SubGroupID, FBA.Status, FBA.PreRevisionNo, FBA.BookingNo, 
	            FBA.BookingDate, FBA.Remarks, FBA.ExportOrderID, FBA.SLNo, FBA.StyleMasterID, FBA.StyleNo, FBA.SeasonID, FBA.FinancialYearID, 
	            FBA.BuyerID, FBA.BuyerTeamID, FBA.CompanyID, FBA.SupplierID, FBA.WithoutOB
	            ,CTO.ContactDisplayCode, CCT.DisplayCode,C.ShortName,
	            Supplier.ShortName,Season.SeasonName, Supplier.MappingCompanyID,BookingQty,FBA.IsSample
            )

            SELECT * FROM F;
                        
                ;WITH 
                Bkk As 
                (
	                Select BookingNo From {DbNames.EPYSL}..BookingMaster Where BookingID = {bookingId}
                    Union
                    Select BookingNo From {DbNames.EPYSL}..SampleBookingMaster Where BookingID = {bookingId}
                ),
                BKKK As 
                (
                    Select BookingID From {DbNames.EPYSL}..BookingMaster Where BookingNo in (Select BookingNo From Bkk)
                    Union
                    Select BookingID From {DbNames.EPYSL}..SampleBookingMaster Where BookingNo in (Select BookingNo From Bkk)
                ),
                FBA1 AS
                (
                    SELECT B.BookingID,A.BOMMasterID,BC.ItemGroupID,BC.SubGroupID,a.Status,PreRevisionNo=a.PreProcessRevNo,
	                b.BookingNo,b.BookingDate,b.Remarks,b.ExportOrderID,SLNo='',
	                SM.StyleMasterID, SM.StyleNo, SM.SeasonID, SM.FinancialYearID,
	                b.BuyerID,b.BuyerTeamID,b.CompanyID,b.SupplierID,WithoutOB=0,BookingQty= SUM(BC.BookingQty)
                    FROM {DbNames.EPYSL}..BookingMaster B
                    Inner Join {DbNames.EPYSL}..BookingCHild BC On BC.BookingID = B.BookingID
	                LEFT JOIN FabricBookingAcknowledge A on b.BookingID = a.BookingID
	                LEFT JOIN {DbNames.EPYSL}..ExportOrderMaster EO ON EO.ExportOrderID = B.ExportOrderID
	                LEFT JOIN {DbNames.EPYSL}..StyleMaster SM ON SM.StyleMasterID = EO.StyleMasterID
                    WHERE B.BookingNo in (Select BookingNo From BKK)
                    Group By B.BookingID,A.BOMMasterID,BC.ItemGroupID,BC.SubGroupID,a.Status,a.PreProcessRevNo,
	                b.BookingNo,b.BookingDate,b.Remarks,b.ExportOrderID,
	                SM.StyleMasterID, SM.StyleNo, SM.SeasonID, SM.FinancialYearID,
	                b.BuyerID,b.BuyerTeamID,b.CompanyID,b.SupplierID
                ),
                FBA2 AS
                (
                    SELECT Distinct b.BookingID,a.BOMMasterID,SBIG.ItemGroupID,SBIG.SubGroupID,a.Status,PreRevisionNo=a.PreProcessRevNo,
	                b.BookingNo,b.BookingDate,b.Remarks,b.ExportOrderID,b.SLNo,
	                b.StyleMasterID, StyleNo = CASE WHEN ISNULL(b.StyleMasterID,0) = 0 THEN b.StyleNo ELSE SM.StyleNo END, b.SeasonID, 
                    FinancialYearID = (Select top 1 FY.FinancialYearID From {DbNames.EPYSL}..FinancialYear FY Where b.BookingDate between FY.StartMonth and FY.EndMonth),
	                b.BuyerID,b.BuyerTeamID,CompanyID=b.ExecutionCompanyID,b.SupplierID,WithoutOB=1,BookingQty= SUM(c.RequiredQty)
                    FROM {DbNames.EPYSL}..SampleBookingMaster b
	                Inner Join {DbNames.EPYSL}..SampleBookingItemGroup SBIG On SBIG.BookingID = b.BookingID
                    Inner Join {DbNames.EPYSL}..SampleBookingConsumptionChild c ON c.BookingID = b.BookingID
	                LEFT Join FabricBookingAcknowledge A on b.BookingID = a.BookingID
	                LEFT JOIN {DbNames.EPYSL}..StyleMaster SM ON SM.StyleMasterID = b.StyleMasterID
                    WHERE B.BookingNo in (Select BookingNo From BKK)
                    Group By b.BookingID,a.BOMMasterID,SBIG.ItemGroupID,SBIG.SubGroupID,a.Status,a.PreProcessRevNo,
	                b.BookingNo,b.BookingDate,b.Remarks,b.ExportOrderID,b.SLNo,
	                b.StyleMasterID,b.StyleNo,SM.StyleNo, b.SeasonID,
	                b.BuyerID,b.BuyerTeamID,b.ExecutionCompanyID,b.SupplierID
                ),
                FBA AS
                (
	                SELECT *FROM FBA1
	                UNION
	                SELECT *FROM FBA2
                ),
                F AS(
                SELECT FBA.*,CTO.ContactDisplayCode AS BuyerName, CCT.DisplayCode AS BuyerTeamName,CompanyName = C.ShortName,
                Supplier.ShortName [SupplierName],Season.SeasonName, ISNULL(Supplier.MappingCompanyID,0) TextileCompanyID,GmtQtyPcs = Sum(TotalPOQty)
                FROM FBA
                LEFT JOIN {DbNames.EPYSL}..OrderBankMaster OBM On OBM.StyleMasterID = FBA.StyleMasterID
	            LEFT JOIN {DbNames.EPYSL}..OrderBankPO OBPO On OBPO.OrderBankMasterID = OBM.OrderBankMasterID AND OBPO.IsActive = 1
                LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = FBA.BuyerID
                LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = FBA.BuyerTeamID
                LEFT JOIN {DbNames.EPYSL}..CompanyEntity C ON C.CompanyID = FBA.CompanyID
                LEFT Join {DbNames.EPYSL}..Contacts Supplier On Supplier.ContactID = FBA.SupplierID
                LEFT Join {DbNames.EPYSL}..ContactSeason Season On Season.SeasonID = FBA.SeasonID
                Group By FBA.BookingID,FBA.BOMMasterID,FBA.ItemGroupID,FBA.SubGroupID,FBA.Status,FBA.PreRevisionNo,FBA.BookingNo,FBA.StyleMasterID,
				FBA.BookingDate,FBA.Remarks,FBA.ExportOrderID,FBA.SLNo,FBA.StyleMasterID,FBA.StyleNo,FBA.SeasonID,FBA.FinancialYearID,
				FBA.BuyerID,FBA.BuyerTeamID,FBA.CompanyID,FBA.SupplierID,FBA.WithoutOB,FBA.BookingQty,
				CTO.ContactDisplayCode, CCT.DisplayCode,C.ShortName,Supplier.ShortName,Season.SeasonName, ISNULL(Supplier.MappingCompanyID,0),FBA.StyleMasterID
                )

                SELECT top 1 * FROM F;
                ;WITH 
                Bkk As 
                (
	                Select BookingNo From {DbNames.EPYSL}..BookingMaster Where BookingID = {bookingId}
                    Union
                    Select BookingNo From {DbNames.EPYSL}..SampleBookingMaster Where BookingID = {bookingId}
                ),
                BKKK As 
                (
                    Select BookingID From {DbNames.EPYSL}..BookingMaster Where BookingNo in (Select BookingNo From Bkk)
                    Union
                    Select BookingID From {DbNames.EPYSL}..SampleBookingMaster Where BookingNo in (Select BookingNo From Bkk)
                ),
                FBA1 AS
                (
                    SELECT B.BookingID,A.BOMMasterID,BC.ItemGroupID,BC.SubGroupID,a.Status,PreRevisionNo=a.PreProcessRevNo,
	                b.BookingNo,b.BookingDate,b.Remarks,b.ExportOrderID,SLNo='',
	                SM.StyleMasterID, SM.StyleNo, SM.SeasonID, SM.FinancialYearID,
	                b.BuyerID,b.BuyerTeamID,b.CompanyID,b.SupplierID,WithoutOB=0
                    FROM {DbNames.EPYSL}..BookingMaster B
                    Inner Join {DbNames.EPYSL}..BookingCHild BC On BC.BookingID = B.BookingID
	                LEFT JOIN FabricBookingAcknowledge A on b.BookingID = a.BookingID
	                LEFT JOIN {DbNames.EPYSL}..ExportOrderMaster EO ON EO.ExportOrderID = B.ExportOrderID
	                LEFT JOIN {DbNames.EPYSL}..StyleMaster SM ON SM.StyleMasterID = EO.StyleMasterID
                    WHERE B.BookingNo in (Select BookingNo From BKK)
                ),
                FBA2 AS
                (
                    SELECT Distinct b.BookingID,a.BOMMasterID,SBIG.ItemGroupID,SBIG.SubGroupID,a.Status,PreRevisionNo=a.PreProcessRevNo,
	                b.BookingNo,b.BookingDate,b.Remarks,b.ExportOrderID,b.SLNo,
	                b.StyleMasterID, StyleNo = CASE WHEN ISNULL(b.StyleMasterID,0) = 0 THEN b.StyleNo ELSE SM.StyleNo END, SM.SeasonID, 
                    FinancialYearID = (Select top 1 FY.FinancialYearID From {DbNames.EPYSL}..FinancialYear FY Where b.BookingDate between FY.StartMonth and FY.EndMonth),
	                b.BuyerID,b.BuyerTeamID,CompanyID=b.ExecutionCompanyID,b.SupplierID,WithoutOB=1
                    FROM {DbNames.EPYSL}..SampleBookingMaster b
	                Inner Join {DbNames.EPYSL}..SampleBookingItemGroup SBIG On SBIG.BookingID = b.BookingID
	                LEFT Join FabricBookingAcknowledge A on b.BookingID = a.BookingID
	                LEFT JOIN {DbNames.EPYSL}..StyleMaster SM ON SM.StyleMasterID = b.StyleMasterID
                    WHERE B.BookingNo in (Select BookingNo From BKK)
                ),
                FBA AS
                (
	                SELECT *FROM FBA1
	                UNION
	                SELECT *FROM FBA2
                ),
                F AS(
                SELECT FBA.*,CTO.ContactDisplayCode AS BuyerName, CCT.DisplayCode AS BuyerTeamName,CompanyName = C.ShortName,
                Supplier.ShortName [SupplierName],Season.SeasonName, ISNULL(Supplier.MappingCompanyID,0) TextileCompanyID
                FROM FBA
                LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = FBA.BuyerID
                LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = FBA.BuyerTeamID
                LEFT JOIN {DbNames.EPYSL}..CompanyEntity C ON C.CompanyID = FBA.CompanyID
                LEFT Join {DbNames.EPYSL}..Contacts Supplier On Supplier.ContactID = FBA.SupplierID
                LEFT Join {DbNames.EPYSL}..ContactSeason Season On Season.SeasonID = FBA.SeasonID)

                SELECT * FROM F;

                        ----- Sample Booking Consumption Child (Fabric)
                           ;With Bkk As (Select BookingNo From {DbNames.EPYSL}..BookingMaster Where BookingID = {bookingId}
                                        Union
                                        Select BookingNo From {DbNames.EPYSL}..SampleBookingMaster Where BookingID = {bookingId}
                        ),BKKK As (
                                        Select BookingID From {DbNames.EPYSL}..BookingMaster Where BookingNo in (Select BookingNo From Bkk)
                                        Union
                                        Select BookingID From {DbNames.EPYSL}..SampleBookingMaster Where BookingNo in (Select BookingNo From Bkk)
                        ),FBA1 AS
                        (
                            SELECT  B.BookingID,B.BookingNo, B.BuyerID, ExecutionCompanyID=B.CompanyID,
							A.BookingChildID, A.ItemMasterID,A.SubGroupID,A.A1ValueID,A.YarnBrandID,ReferenceSourceID=0,YarnSourceID=0,
							ReferenceNo='',ColorReferenceNo='',A.ConsumptionID,A.ItemGroupID,  B.RevisionNo,
							C.Segment1Desc,C.Segment2Desc,C.Segment3Desc,
							C.Segment4Desc,C.Segment5Desc,C.Segment6Desc, C.Segment7Desc,
							A.LengthYds,A.LengthInch,A.FUPartID,A.ConsumptionQty,C.OrderUnitID,A.LabDipNo,A.Price,A.SuggestedPrice,
							RequiredQty=A.RequisitionQty,A.Remarks,ForBDSStyleNo='', B.SupplierID,LiabilitiesBookingQty=ISNULL(FBAC.LiabilitiesBookingQty,0),ActualBookingQty=ISNULL(FBAC.ActualBookingQty,0),
							BookingUOM = BU.DisplayUnitDesc
							FROM {DbNames.EPYSL}..BookingChild A
							LEFT JOIN {DbNames.EPYSL}..Unit BU On BU.UnitID = A.BookingUnitID
							LEFT JOIN {DbNames.EPYSL}..BookingMaster B ON B.BookingID=A.BookingID
							LEFT JOIN {DbNames.EPYSL}..BOMConsumption C ON C.ConsumptionID=A.ConsumptionID
                            LEFT JOIN FBookingAcknowledgeChild FBAC On FBAC.ConsumptionID = C.ConsumptionID And FBAC.ItemMasterID = A.ItemMasterID
							WHERE B.BookingNo in (Select BookingNo From BKK) AND A.SubGroupID = 1
                        ),
						FBA2 AS
                        (
                           SELECT B.BookingID,B.BookingNo, B.BuyerID,B.ExecutionCompanyID,
						   BookingChildID=A.ConsumptionChildID, A.ItemMasterID,A.SubGroupID,C.A1ValueID,C.YarnBrandID,C.ReferenceSourceID,C.YarnSourceID,
						   C.ReferenceNo,C.ColorReferenceNo,A.ConsumptionID,A.ItemGroupID,  B.RevisionNo,
						   C.Segment1Desc,C.Segment2Desc,C.Segment3Desc,
						   C.Segment4Desc,C.Segment5Desc,C.Segment6Desc, C.Segment7Desc,
						   C.LengthYds,C.LengthInch,C.FUPartID,A.ConsumptionQty,C.OrderUnitID,C.LabDipNo,C.Price,C.SuggestedPrice,
						   A.RequiredQty,C.Remarks,C.ForBDSStyleNo, B.SupplierID,LiabilitiesBookingQty=ISNULL(FBAC.LiabilitiesBookingQty,0),ActualBookingQty=ISNULL(FBAC.ActualBookingQty,0)
						   ,BookingUOM = BU.DisplayUnitDesc
						   FROM {DbNames.EPYSL}..SampleBookingConsumptionChild A
						   LEFT JOIN {DbNames.EPYSL}..Unit BU On BU.UnitID = A.RequiredUnitID
						   LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster B ON B.BookingID=A.BookingID
						   LEFT JOIN {DbNames.EPYSL}..SampleBookingConsumption C ON C.ConsumptionID=A.ConsumptionID
                           LEFT JOIN FBookingAcknowledgeChild FBAC On FBAC.ConsumptionID = C.ConsumptionID And FBAC.ItemMasterID = A.ItemMasterID
						   WHERE B.BookingNo in (Select BookingNo From BKK) AND A.SubGroupID = 1
                        ),
						FBA AS
						(
							SELECT * FROM FBA1
							UNION
							SELECT * FROM FBA2
						),
						F AS (SELECT SBM.BookingChildID, SBM.ReferenceSourceID,RSETV.ValueName ReferenceSourceName , SBM.ReferenceNo,SBM.ColorReferenceNo,SBM.YarnSourceID,SBM.ConsumptionID,SBM.BookingID,SBM.BookingNo,SBM.ItemGroupID,SBM.SubGroupID,SBM.Segment1Desc Construction,SBM.Segment2Desc Composition,SBM.Segment3Desc Color
						,SBM.Segment4Desc GSM,SBM.Segment5Desc FabricWidth,SBM.Segment7Desc KnittingType,SBM.LengthYds,SBM.LengthInch,SBM.FUPartID,SBM.ConsumptionQty,SBM.OrderUnitID BookingUnitID
						,SBM.A1ValueID,SBM.YarnBrandID,SBM.LabDipNo,SBM.Price,SBM.SuggestedPrice,PreviousBookingQty=IsNull((Select Sum(BookingQty) From {DbNames.EPYSL}..BookingChild_Bk Where RevisionNo = SBM.RevisionNo-1 And BookingChildID =  SBM.BookingChildID),0),SBM.RequiredQty BookingQty,SBM.ItemMasterID,SBM.BuyerID ContactID,
						SBM.ExecutionCompanyID,
						0 As TechnicalNameId,ISG.SubGroupName,'1' As ConceptTypeID,IM.Segment1ValueID ConstructionId, IM.Segment2ValueID CompositionId,
						IM.Segment3ValueID ColorId,IM.Segment7ValueID KnittingTypeId,IM.Segment4ValueID GSMId,
						ISV.SegmentValue YarnType, ETV.ValueName YarnProgram, SBM.Segment6Desc DyeingType, SBM.Remarks Instruction, SBM.ForBDSStyleNo, ISNULL(Supplier.MappingCompanyID,0) TextileCompanyID,SBM.BookingUOM, 
                        RefSourceNo = CASE WHEN ISNULL(RS.RefSourceNo,'') = '' THEN 'New Item' ELSE RS.RefSourceNo END
						FROM FBA SBM
						LEFT JOIN {DbNames.EPYSL}..BookingChildReferenceSource RS ON RS.BookingID = SBM.BookingID AND RS.ConsumptionID = SBM.ConsumptionID
                        LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = SBM.SubGroupID
                        LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = SBM.ItemMasterID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV On ISV.SegmentValueID = SBM.A1ValueID
				        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = SBM.YarnBrandID
                        Left Join {DbNames.EPYSL}..EntityTypeValue RSETV ON RSETV.ValueID = SBM.ReferenceSourceID
                        Left Join {DbNames.EPYSL}..EntityTypeValue YSETV ON YSETV.ValueID = SBM.YarnSourceID
                        LEFT Join {DbNames.EPYSL}..Contacts Supplier On Supplier.ContactID = SBM.SupplierID
                        WHERE SBM.BookingNo in (Select BookingNo From BKK)
						)
						SELECT * FROM F;

                        ----- Sample Booking Consumption Child (Collor & Cuff)
                          ;With Bkk As (Select BookingNo From {DbNames.EPYSL}..BookingMaster Where BookingID = {bookingId}
                                        Union
                                        Select BookingNo From {DbNames.EPYSL}..SampleBookingMaster Where BookingID = {bookingId}
                        ),BKKK As (
                                        Select BookingID From {DbNames.EPYSL}..BookingMaster Where BookingNo in (Select BookingNo From Bkk)
                                        Union
                                        Select BookingID From {DbNames.EPYSL}..SampleBookingMaster Where BookingNo in (Select BookingNo From Bkk)
                        ),FBA1 AS
                        (
                            SELECT BM.BookingID
	                        ,B.BookingNo, B.BuyerID, ExecutionCompanyID=B.CompanyID,
	                        A.BookingChildID, A.ItemMasterID,A.SubGroupID,A.A1ValueID,A.YarnBrandID,ReferenceSourceID=0,YarnSourceID=0,
	                        ReferenceNo='',ColorReferenceNo='',A.ConsumptionID,A.ItemGroupID,  B.RevisionNo,
	                        C.Segment1Desc,C.Segment2Desc,C.Segment3Desc,
	                        C.Segment4Desc,C.Segment5Desc,C.Segment6Desc, C.Segment7Desc,
	                        A.LengthYds,A.LengthInch,A.FUPartID,A.ConsumptionQty,C.OrderUnitID,A.LabDipNo,A.Price,A.SuggestedPrice,
	                        RequiredQty=SUM(CD.BookingQty),A.Remarks,ForBDSStyleNo='', B.SupplierID,LiabilitiesBookingQty=ISNULL(FBAC.LiabilitiesBookingQty,0),ActualBookingQty=ISNULL(FBAC.ActualBookingQty,0)
	                        ,BookingUOM = BU.DisplayUnitDesc
	                        FROM {DbNames.EPYSL}..BookingChildDetails CD
	                        INNER JOIN {DbNames.EPYSL}..BookingChild A ON A.BookingChildID = CD.BookingChildID
	                        INNER JOIN {DbNames.EPYSL}..BookingMaster BM ON BM.BookingID = A.BookingID
	                        LEFT JOIN {DbNames.EPYSL}..Unit BU On BU.UnitID = A.BookingUnitID
	                        LEFT JOIN {DbNames.EPYSL}..BookingMaster B ON B.BookingID=A.BookingID
	                        LEFT JOIN {DbNames.EPYSL}..BOMConsumption C ON C.ConsumptionID=A.ConsumptionID
                            LEFT JOIN FBookingAcknowledgeChild FBAC On FBAC.ConsumptionID = C.ConsumptionID And FBAC.ItemMasterID = A.ItemMasterID
                            WHERE BM.BookingNo in (select BookingNo From {DbNames.EPYSL}..Bookingmaster Where BookingID = {bookingId}) AND A.SubGroupID IN (11,12)
	                        GROUP BY BM.BookingID,B.BookingNo, B.BuyerID, B.CompanyID,
	                        A.BookingChildID, A.ItemMasterID,A.SubGroupID,A.A1ValueID,A.YarnBrandID,
	                        A.ConsumptionID,A.ItemGroupID,  B.RevisionNo,
	                        C.Segment1Desc,C.Segment2Desc,C.Segment3Desc,
	                        C.Segment4Desc,C.Segment5Desc,C.Segment6Desc, C.Segment7Desc,
	                        A.LengthYds,A.LengthInch,A.FUPartID,A.ConsumptionQty,C.OrderUnitID,A.LabDipNo,A.Price,A.SuggestedPrice,
	                        A.Remarks,B.SupplierID,ISNULL(FBAC.LiabilitiesBookingQty,0),ISNULL(FBAC.ActualBookingQty,0)
	                        ,BU.DisplayUnitDesc
                        ),
						FBA2 AS
                        (
                           SELECT B.BookingID,B.BookingNo, B.BuyerID,B.ExecutionCompanyID,
						   BookingChildID=A.ConsumptionChildID, A.ItemMasterID,A.SubGroupID,C.A1ValueID,C.YarnBrandID,C.ReferenceSourceID,C.YarnSourceID,
						   C.ReferenceNo,C.ColorReferenceNo,A.ConsumptionID,A.ItemGroupID,  B.RevisionNo,
						   C.Segment1Desc,C.Segment2Desc,C.Segment3Desc,
						   C.Segment4Desc,C.Segment5Desc,C.Segment6Desc, C.Segment7Desc,
						   C.LengthYds,C.LengthInch,C.FUPartID,A.ConsumptionQty,C.OrderUnitID,C.LabDipNo,C.Price,C.SuggestedPrice,
						   A.RequiredQty,C.Remarks,C.ForBDSStyleNo, B.SupplierID,LiabilitiesBookingQty=ISNULL(FBAC.LiabilitiesBookingQty,0),ActualBookingQty=ISNULL(FBAC.ActualBookingQty,0)
						   ,BookingUOM = BU.DisplayUnitDesc
						   FROM {DbNames.EPYSL}..SampleBookingConsumptionChild A
						   LEFT JOIN {DbNames.EPYSL}..Unit BU On BU.UnitID = A.RequiredUnitID
						   LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster B ON B.BookingID=A.BookingID
						   LEFT JOIN {DbNames.EPYSL}..SampleBookingConsumption C ON C.ConsumptionID=A.ConsumptionID
                            LEFT JOIN FBookingAcknowledgeChild FBAC On FBAC.ConsumptionID = C.ConsumptionID And FBAC.ItemMasterID = A.ItemMasterID
						   WHERE B.BookingNo in (select BookingNo From {DbNames.EPYSL}..SampleBookingmaster Where BookingID = {bookingId}) AND A.SubGroupID IN (11,12)
                        ),
						FBA AS
						(
							SELECT * FROM FBA1
							UNION
							SELECT * FROM FBA2
						),
						F AS (SELECT SBM.BookingChildID, SBM.ReferenceSourceID,RSETV.ValueName ReferenceSourceName , SBM.ReferenceNo,SBM.ColorReferenceNo,SBM.YarnSourceID,SBM.ConsumptionID,SBM.BookingID,SBM.BookingNo,SBM.ItemGroupID,SBM.SubGroupID,SBM.Segment1Desc,SBM.Segment2Desc,SBM.Segment3Desc
						,SBM.Segment4Desc,SBM.Segment5Desc,SBM.Segment6Desc,SBM.Segment7Desc,SBM.LengthYds,SBM.LengthInch,SBM.FUPartID,SBM.ConsumptionQty,SBM.OrderUnitID BookingUnitID
						,SBM.A1ValueID,SBM.YarnBrandID,SBM.LabDipNo,SBM.Price,SBM.SuggestedPrice,SBM.RequiredQty BookingQty,SBM.ItemMasterID,SBM.BuyerID ContactID,
						SBM.ExecutionCompanyID,
						0 As TechnicalNameId,ISG.SubGroupName,'1' As ConceptTypeID,IM.Segment1ValueID, IM.Segment2ValueID,
						IM.Segment3ValueID,IM.Segment4ValueID,IM.Segment5ValueID,IM.Segment6ValueID,IM.Segment7ValueID,
						ISV.SegmentValue YarnType, ETV.ValueName YarnProgram, SBM.Segment6Desc DyeingType, SBM.Remarks Instruction, SBM.ForBDSStyleNo, ISNULL(Supplier.MappingCompanyID,0) TextileCompanyID,LiabilitiesBookingQty=ISNULL(SBM.LiabilitiesBookingQty,0),ActualBookingQty=ISNULL(SBM.ActualBookingQty,0)
						, SBM.BookingUOM
						FROM FBA SBM
                        LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = SBM.SubGroupID
                        LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = SBM.ItemMasterID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV On ISV.SegmentValueID = SBM.A1ValueID
				        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = SBM.YarnBrandID
                        Left Join {DbNames.EPYSL}..EntityTypeValue RSETV ON RSETV.ValueID = SBM.ReferenceSourceID
                        Left Join {DbNames.EPYSL}..EntityTypeValue YSETV ON YSETV.ValueID = SBM.YarnSourceID
                        LEFT Join {DbNames.EPYSL}..Contacts Supplier On Supplier.ContactID = SBM.SupplierID
						)
						SELECT * FROM F;

                       ----Sample Booking Consumption AddProcess
                        ;With Bkk As (Select BookingNo From {DbNames.EPYSL}..BookingMaster Where BookingID = {bookingId}
                                        Union
                                        Select BookingNo From {DbNames.EPYSL}..SampleBookingMaster Where BookingID = {bookingId}
                        ),BKKK As (
                                        Select BookingID From {DbNames.EPYSL}..BookingMaster Where BookingNo in (Select BookingNo From Bkk)
                                        Union
                                        Select BookingID From {DbNames.EPYSL}..SampleBookingMaster Where BookingNo in (Select BookingNo From Bkk)
                        )
						SELECT  BookingID ,ProcessID,ConsumptionID
                        FROM {DbNames.EPYSL}..SampleBookingConsumptionAddProcess where BookingID in (Select BookingID From BKKK);

                        ----- Sample Booking Consumption Child Details
                        ;With Bkk As (Select BookingNo From {DbNames.EPYSL}..BookingMaster Where BookingID = {bookingId}
                                        Union
                                        Select BookingNo From {DbNames.EPYSL}..SampleBookingMaster Where BookingID = {bookingId}
                        ),BKKK As (
                                        Select BookingID From {DbNames.EPYSL}..BookingMaster Where BookingNo in (Select BookingNo From Bkk)
                                        Union
                                        Select BookingID From {DbNames.EPYSL}..SampleBookingMaster Where BookingNo in (Select BookingNo From Bkk)
                        )
						SELECT ConsumptionID,BookingID,ItemGroupID,SubGroupID,ItemMasterID,RequiredQty BookingQty ,ConsumptionQty,RequiredUnitID BookingUnitID, 0 As TechnicalNameId
                        FROM {DbNames.EPYSL}..SampleBookingConsumptionChild where BookingID in (Select BookingID From BKKK)

                        ----- Sample Booking Consumption Garment Part
                        ;With Bkk As (Select BookingNo From {DbNames.EPYSL}..BookingMaster Where BookingID = {bookingId}
                                        Union
                                        Select BookingNo From {DbNames.EPYSL}..SampleBookingMaster Where BookingID = {bookingId}
                        ),BKKK As (
                                        Select BookingID From {DbNames.EPYSL}..BookingMaster Where BookingNo in (Select BookingNo From Bkk)
                                        Union
                                        Select BookingID From {DbNames.EPYSL}..SampleBookingMaster Where BookingNo in (Select BookingNo From Bkk)
                        )
						SELECT  BookingID ,FUPartID,ConsumptionID
                         FROM {DbNames.EPYSL}..SampleBookingConsumptionGarmentPart where BookingID in (Select BookingID From BKKK)

                        ---Sample Booking Consumption Process
                        ;With Bkk As (Select BookingNo From {DbNames.EPYSL}..BookingMaster Where BookingID = {bookingId}
                                        Union
                                        Select BookingNo From {DbNames.EPYSL}..SampleBookingMaster Where BookingID = {bookingId}
                        ),BKKK As (
                                        Select BookingID From {DbNames.EPYSL}..BookingMaster Where BookingNo in (Select BookingNo From Bkk)
                                        Union
                                        Select BookingID From {DbNames.EPYSL}..SampleBookingMaster Where BookingNo in (Select BookingNo From Bkk)
                        )
						SELECT BookingID,ProcessID,ConsumptionID
                        FROM {DbNames.EPYSL}..SampleBookingConsumptionProcess where BookingID in (Select BookingID From BKKK)

                        ------ Sample Booking Consumption Text
                        ;With Bkk As (Select BookingNo From {DbNames.EPYSL}..BookingMaster Where BookingID = {bookingId}
                                        Union
                                        Select BookingNo From {DbNames.EPYSL}..SampleBookingMaster Where BookingID = {bookingId}
                        ),BKKK As (
                                        Select BookingID From {DbNames.EPYSL}..BookingMaster Where BookingNo in (Select BookingNo From Bkk)
                                        Union
                                        Select BookingID From {DbNames.EPYSL}..SampleBookingMaster Where BookingNo in (Select BookingNo From Bkk)
                        )
						SELECT BookingID,UsesIn,AdditionalProcess,ApplicableProcess,YarnSubProgram,GarmentsColor GmtColor,ConsumptionID
                        FROM {DbNames.EPYSL}..SampleBookingConsumptionText  where BookingID in (Select BookingID From BKKK)

                        ------ Sample Booking Child Distribution 
                        ;With Bkk As (Select BookingNo From {DbNames.EPYSL}..BookingMaster Where BookingID = {bookingId}
                                        Union
                                        Select BookingNo From {DbNames.EPYSL}..SampleBookingMaster Where BookingID = {bookingId}
                        ),BKKK As (
                                        Select BookingID From {DbNames.EPYSL}..BookingMaster Where BookingNo in (Select BookingNo From Bkk)
                                        Union
                                        Select BookingID From {DbNames.EPYSL}..SampleBookingMaster Where BookingNo in (Select BookingNo From Bkk)
                        )
						SELECT *
                        FROM {DbNames.EPYSL}..SampleBookingChildDistribution  where BookingID in (Select BookingID From BKKK)

                        ----- SampleBooking ConsumptionYarn Sub Brand
                        ;With Bkk As (Select BookingNo From {DbNames.EPYSL}..BookingMaster Where BookingID = {bookingId}
                                        Union
                                        Select BookingNo From {DbNames.EPYSL}..SampleBookingMaster Where BookingID = {bookingId}
                        ),BKKK As (
                                        Select BookingID From {DbNames.EPYSL}..BookingMaster Where BookingNo in (Select BookingNo From Bkk)
                                        Union
                                        Select BookingID From {DbNames.EPYSL}..SampleBookingMaster Where BookingNo in (Select BookingNo From Bkk)
                        )
						SELECT SBKC.ReferenceSourceID,RSETV.ValueName ReferenceSourceName , SBKC.ReferenceNo,SBKC.ColorReferenceNo,SBKC.YarnSourceID,SCYS.BookingID, SCYS.YarnSubBrandID, SCYS.ConsumptionID, YSB.YarnSubBrandName
				        FROM {DbNames.EPYSL}..SampleBookingConsumptionYarnSubBrand SCYS
				        Inner Join {DbNames.EPYSL}..SampleBookingConsumption SBKC On SCYS.BookingID = SBKC.BookingID And SCYS.ConsumptionID = SBKC.ConsumptionID
                        Inner Join {DbNames.EPYSL}..ItemGroup IG On IG.ItemGroupID = SBKC.ItemGroupID
                        Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = SBKC.SubGroupID
                        Left Join {DbNames.EPYSL}..EntityTypeValue RSETV ON RSETV.ValueID = SBKC.ReferenceSourceID
                        Left Join  {DbNames.EPYSL}..EntityTypeValue YSETV ON YSETV.ValueID = SBKC.YarnSourceID
				        Inner Join (
					        Select ETV.ValueID YarnSubBrandID, ETV.ValueName YarnSubBrandName
					        From {DbNames.EPYSL}..EntityTypeValue ETV
					        Inner Join {DbNames.EPYSL}..EntityType ET On ET.EntityTypeID = ETV.EntityTypeID
				        ) YSB On YSB.YarnSubBrandID = SCYS.YarnSubBrandID
				        Where SCYS.BookingID in (Select BookingID From BKKK);

                        ----- SampleBookingChildImage
                        ;With Bkk As (Select BookingNo From {DbNames.EPYSL}..BookingMaster Where BookingID = {bookingId}
                                        Union
                                        Select BookingNo From {DbNames.EPYSL}..SampleBookingMaster Where BookingID = {bookingId}
                        ),BKKK As (
                                        Select BookingID From {DbNames.EPYSL}..BookingMaster Where BookingNo in (Select BookingNo From Bkk)
                                        Union
                                        Select BookingID From {DbNames.EPYSL}..SampleBookingMaster Where BookingNo in (Select BookingNo From Bkk)
                        )
						SELECT BookingID,ImagePath
                        FROM {DbNames.EPYSL}..SampleBookingChildImage where BookingID in (Select BookingID From BKKK)

                        --Free Concept
                        ;With Bkk As (Select BookingNo From {DbNames.EPYSL}..BookingMaster Where BookingID = {bookingId}
                                        Union
                                        Select BookingNo From {DbNames.EPYSL}..SampleBookingMaster Where BookingID = {bookingId}
                        ),BKKK As (
                                        Select BookingID From {DbNames.EPYSL}..BookingMaster Where BookingNo in (Select BookingNo From Bkk)
                                        Union
                                        Select BookingID From {DbNames.EPYSL}..SampleBookingMaster Where BookingNo in (Select BookingNo From Bkk)
                        )
						Select SBC.ReferenceSourceID,RSETV.ValueName ReferenceSourceName , SBC.ReferenceNo,SBC.ColorReferenceNo,SBC.YarnSourceID,FB.BookingID, FB.BookingNo, FB.BookingDate ConceptDate, SBC.RequiredQty Qty,
                        CC.ItemMasterID, IM.Segment1ValueID ConstructionId, IM.Segment2ValueID CompositionId, IM.Segment3ValueID ColorId, Color.SegmentValue ColorName, IM.Segment4ValueID GSMId,
                        IM.Segment7ValueID KnittingTypeId, IM.SubGroupID,FB.ExecutionCompanyID CompanyID, ISNULL(Supplier.MappingCompanyID,0) TextileCompanyID
                        FROM {DbNames.EPYSL}..SampleBookingConsumption SBC
                        INNER JOIN {DbNames.EPYSL}..SampleBookingConsumptionChild CC ON CC.ConsumptionID = SBC.ConsumptionID
                        INNER JOIN {DbNames.EPYSL}..SampleBookingMaster FB ON FB.BookingID = SBC.BookingID
                        INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = CC.ItemMasterID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = IM.Segment1ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = IM.Segment2ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = IM.Segment3ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = IM.Segment4ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Width ON Width.SegmentValueID = IM.Segment5ValueID
                        --LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue DT ON DT.SegmentValueID = IM.Segment6ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue KT ON KT.SegmentValueID = IM.Segment7ValueID
                        --LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID = FB.SubGroupID
                        Left Join  {DbNames.EPYSL}..EntityTypeValue RSETV ON RSETV.ValueID = SBC.ReferenceSourceID
                        Left Join  {DbNames.EPYSL}..EntityTypeValue YSETV ON YSETV.ValueID = SBC.YarnSourceID
                        LEFT Join {DbNames.EPYSL}..Contacts Supplier On Supplier.ContactID = FB.SupplierID
                        where FB.BookingID in (Select BookingID From BKKK)

                        --Technical Name
                        SELECT Cast(T.TechnicalNameId As varchar) id, T.TechnicalName [text], ISNULL(ST.[Days], 0) [desc], Cast(SC.SubClassID as varchar) additionalValue
                        FROM FabricTechnicalName T
                        LEFT JOIN FabricTechnicalNameKMachineSubClass SC ON SC.TechnicalNameID = T.TechnicalNameId
                        LEFT JOIN KnittingMachineStructureType_HK ST ON ST.StructureTypeID = SC.StructureTypeID
                        Group By T.TechnicalNameId, T.TechnicalName, ST.Days, SC.SubClassID;

                         --YarnSource data load
				        Select Cast(a.ValueID as varchar) id, a.ValueName [text]
				        From {DbNames.EPYSL}..EntityTypeValue a
				        Inner Join {DbNames.EPYSL}..EntityType b on b.EntityTypeID = a.EntityTypeID
				        Where b.EntityTypeName = 'Yarn Source'

                        --M/c type
                        ;SELECT CAST(a.MachineSubClassID AS varchar) [id], b.SubClassName [text], b.TypeID [desc], c.TypeName additionalValue
                        FROM KnittingMachine a
                        INNER JOIN KnittingMachineSubClass b ON b.SubClassID = a.MachineSubClassID
                        Inner Join KnittingMachineType c On c.TypeID = b.TypeID
                        --Where c.TypeName != 'Flat Bed'
                        GROUP BY a.MachineSubClassID, b.SubClassName, b.TypeID, c.TypeName;

                        --CriteriaNames
                         ;SELECT CriteriaName,CriteriaSeqNo, (CASE WHEN CriteriaName  IN('Batch Preparation','Quality Check') THEN '1'ELSE'0'END) AS TotalTime
                        FROM BDSCriteria_HK --WHERE CriteriaName NOT IN('Batch Preparation','Testing')
                        GROUP BY CriteriaSeqNo,CriteriaName order by CriteriaSeqNo,CriteriaName;

                        --FBAChildPlannings
                        ;SELECT * FROM BDSCriteria_HK order by CriteriaSeqNo, OperationSeqNo, CriteriaName;
                        --Liability Process
				        Select LChildID = 0,BookingChildID = 0,AcknowledgeID = 0,BookingID = 0,UnitID = 0, Cast(a.ValueID as varchar) LiabilitiesProcessID, a.ValueName LiabilitiesName,LiabilityQty=0
				        From {DbNames.EPYSL}..EntityTypeValue a
				        Inner Join {DbNames.EPYSL}..EntityType b on b.EntityTypeID = a.EntityTypeID
				        Where b.EntityTypeName = 'Process Liability'
                        --Liability Process data load
				        Select LChildID = IsNull(F.LChildID,0),BookingChildID = IsNull(F.BookingChildID,0),AcknowledgeID = IsNull(F.AcknowledgeID,0),BookingID = IsNull(F.BookingID,0),UnitID = IsNull(F.UnitID,0), Cast(a.ValueID as varchar) LiabilitiesProcessID, a.ValueName LiabilitiesName,LiabilityQty=IsNull(F.LiabilityQty,0)
				        From {DbNames.EPYSL}..EntityTypeValue a
				        Inner Join {DbNames.EPYSL}..EntityType b on b.EntityTypeID = a.EntityTypeID
				        Left Join (Select LChildID,BookingChildID,AcknowledgeID,BookingID,LiabilitiesProcessID,UnitID,LiabilityQty From FBookingAcknowledgementLiabilityDistribution Where BookingID = {bookingId} Group By LChildID,BookingChildID,AcknowledgeID,BookingID,LiabilitiesProcessID,UnitID,LiabilityQty)F On F.LiabilitiesProcessID = a.ValueID
				        Where b.EntityTypeName = 'Process Liability';
                        -- Yarn Booking Child Items
                    ;With Bkk As (Select BookingNo From {DbNames.EPYSL}..BookingMaster Where BookingID = {bookingId}
                                        Union
                                        Select BookingNo From {DbNames.EPYSL}..SampleBookingMaster Where BookingID = {bookingId}
                        ),BKKK As (
                                        Select BookingID From {DbNames.EPYSL}..BookingMaster Where BookingNo in (Select BookingNo From Bkk)
                                        Union
                                        Select BookingID From {DbNames.EPYSL}..SampleBookingMaster Where BookingNo in (Select BookingNo From Bkk)
                        ), YBM As
                        (
                            Select * From YarnBookingMaster Where BookingID in (Select BookingID From BKKK)
                        ), YBM_New As
                        (
                            Select * From YarnBookingMaster_New Where BookingID in (Select BookingID From BKKK)
                        ),
                        YBCI As (
                            Select YBCI.YItemMasterID As ItemMasterID, YBCI.UnitID, U.DisplayUnitDesc, YBCI.Blending,
                            (Case When Blending = 1 then 'Blend' else 'Non-Blend' End)BlendingName, YBCI.YarnCategory,  BookingQty=Sum(YBCI.BookingQty),
                            ShadeCode= IsNull(YBCI.ShadeCode,''), IsNull(Y.ShadeCode,'') as ShadeName,
                            YBCI.Remarks, YBCI.Specification, YBCI.YD, YDItem='', YBM.BookingID,
                            ISV1.SegmentValue AS Segment1ValueDesc, ISV2.SegmentValue AS Segment2ValueDesc, ISV3.SegmentValue AS Segment3ValueDesc,
                            ISV4.SegmentValue AS Segment4ValueDesc, ISV5.SegmentValue AS Segment5ValueDesc, ISV6.SegmentValue AS Segment6ValueDesc,
                            ISV7.SegmentValue AS Segment7ValueDesc, ISV8.SegmentValue AS Segment8ValueDesc, YBM.SubGroupID, ISG.SubGroupName,
                            LiabilityQty = IsNull(FBAY.LiabilityQty,0)
                            From YBM Inner Join YarnBookingChild YBC On YBM.YBookingID = YBC.YBookingID
                            Inner Join YarnBookingChildItem YBCI On YBCI.YBChildID = YBC.YBChildID
                            Left Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = YBM.SubGroupID
                            INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YBCI.YItemMasterID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV8 ON ISV8.SegmentValueID = IM.Segment8ValueID
                            LEFT JOIN {DbNames.EPYSL}..Unit U ON U.UnitID = YBCI.UnitID
                            LEFT JOIN YarnShadeBook Y ON Y.ShadeCode = YBCI.ShadeCode
                            LEFT Join YDBookingMaster YDBM ON YDBM.YBookingID = YBM.YBookingID And YDBM.YBookingID = YBCI.YBookingID
                            LEFT Join YDProductionMaster YPM ON YPM.YDBookingMasterID = YDBM.YDBookingMasterID
						    Left Join FBookingAcknowledgementYarnLiability FBAY On FBAY.BookingID = YBM.BookingID And FBAY.ItemMasterID = YBCI.YItemMasterID
						    Group By YBCI.YItemMasterID, YBCI.UnitID, U.DisplayUnitDesc, YBCI.Blending,
                            (Case When Blending = 1 then 'Blend' else 'Non-Blend' End), YBCI.YarnCategory, 
                            IsNull(YBCI.ShadeCode,''), Y.ShadeCode,
                            YBCI.Remarks, YBCI.Specification, YBCI.YD, YBM.BookingID,
                            ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue,
                            ISV4.SegmentValue, ISV5.SegmentValue, ISV6.SegmentValue,
                            ISV7.SegmentValue, ISV8.SegmentValue, YBM.SubGroupID, ISG.SubGroupName,
                            IsNull(FBAY.LiabilityQty,0)
                            Union
                            Select YBCI.YItemMasterID As ItemMasterID, YBCI.UnitID, U.DisplayUnitDesc, YBCI.Blending,
                            (Case When Blending = 1 then 'Blend' else 'Non-Blend' End)BlendingName, YBCI.YarnCategory,  BookingQty=Sum(YBCI.BookingQty),
                            ShadeCode= IsNull(YBCI.ShadeCode,''), IsNull(Y.ShadeCode,'') as ShadeName,
                            YBCI.Remarks, YBCI.Specification, YBCI.YD, YDItem='', YBM.BookingID,
                            ISV1.SegmentValue AS Segment1ValueDesc, ISV2.SegmentValue AS Segment2ValueDesc, ISV3.SegmentValue AS Segment3ValueDesc,
                            ISV4.SegmentValue AS Segment4ValueDesc, ISV5.SegmentValue AS Segment5ValueDesc, ISV6.SegmentValue AS Segment6ValueDesc,
                            ISV7.SegmentValue AS Segment7ValueDesc, ISV8.SegmentValue AS Segment8ValueDesc, YBM.SubGroupID, ISG.SubGroupName,
                            LiabilityQty = IsNull(FBAY.LiabilityQty,0)
                            From YBM_New YBM 
                            Inner Join YarnBookingChild_New YBC On YBM.YBookingID = YBC.YBookingID
                            Inner Join YarnBookingChildItem_New YBCI On YBCI.YBChildID = YBC.YBChildID
                            Left Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = YBM.SubGroupID
                            INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YBCI.YItemMasterID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV8 ON ISV8.SegmentValueID = IM.Segment8ValueID
                            LEFT JOIN {DbNames.EPYSL}..Unit U ON U.UnitID = YBCI.UnitID
                            LEFT JOIN YarnShadeBook Y ON Y.ShadeCode = YBCI.ShadeCode
                            LEFT Join YDBookingMaster YDBM ON YDBM.YBookingID = YBM.YBookingID And YDBM.YBookingID = YBCI.YBookingID
                            LEFT Join YDProductionMaster YPM ON YPM.YDBookingMasterID = YDBM.YDBookingMasterID
						    Left Join FBookingAcknowledgementYarnLiability FBAY On FBAY.BookingID = YBM.BookingID And FBAY.ItemMasterID = YBCI.YItemMasterID
						    Group By YBCI.YItemMasterID, YBCI.UnitID, U.DisplayUnitDesc, YBCI.Blending,
                            (Case When Blending = 1 then 'Blend' else 'Non-Blend' End), YBCI.YarnCategory, 
                            IsNull(YBCI.ShadeCode,''), Y.ShadeCode,
                            YBCI.Remarks, YBCI.Specification, YBCI.YD, YBM.BookingID,
                            ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue,
                            ISV4.SegmentValue, ISV5.SegmentValue, ISV6.SegmentValue,
                            ISV7.SegmentValue, ISV8.SegmentValue, YBM.SubGroupID, ISG.SubGroupName,
                            IsNull(FBAY.LiabilityQty,0)
                        )
					    Select *From YBCI;";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                List<FabricBookingAcknowledge> fbaList = records.Read<FabricBookingAcknowledge>().ToList();
                FBookingAcknowledge data = records.Read<FBookingAcknowledge>().FirstOrDefault();
                data.IsSample = fbaList.Count() > 0 ? fbaList.First().IsSample : false;
                data.FBookingAcknowledgeList = records.Read<FBookingAcknowledge>().ToList();
                Guard.Against.NullObject(data);

                List<FBookingAcknowledgeChild> bookingChilds = records.Read<FBookingAcknowledgeChild>().ToList();
                List<FBookingAcknowledgeChild> bookingChildsCollarCuff = records.Read<FBookingAcknowledgeChild>().ToList();
                data.FabricBookingAcknowledgeList = fbaList;
                data.FBookingAcknowledgeChildAddProcess = records.Read<FBookingAcknowledgeChildAddProcess>().ToList();
                data.FBookingChildDetails = records.Read<FBookingAcknowledgeChildDetails>().ToList();
                data.FBookingAcknowledgeChildGarmentPart = records.Read<FBookingAcknowledgeChildGarmentPart>().ToList();
                data.FBookingAcknowledgeChildProcess = records.Read<FBookingAcknowledgeChildProcess>().ToList();
                data.FBookingAcknowledgeChildText = records.Read<FBookingAcknowledgeChildText>().ToList();
                data.FBookingAcknowledgeChildDistribution = records.Read<FBookingAcknowledgeChildDistribution>().ToList();
                data.FBookingAcknowledgeChildYarnSubBrand = records.Read<FBookingAcknowledgeChildYarnSubBrand>().ToList();
                data.FBookingAcknowledgeImage = records.Read<FBookingAcknowledgeImage>().ToList();
                data.FreeConcepts = records.Read<FreeConceptMaster>().ToList();
                data.TechnicalNameList = await records.ReadAsync<Select2OptionModel>();
                data.YarnSourceNameList = await records.ReadAsync<Select2OptionModel>();

                List<Select2OptionModel> mcTypeList = records.Read<Select2OptionModel>().ToList();
                data.MCTypeForFabricList = mcTypeList.Where(x => x.additionalValue != "Flat Bed");
                data.MCTypeForOtherList = mcTypeList.Where(x => x.additionalValue == "Flat Bed");

                List<FBookingAcknowledgeChild> criteriaNames = records.Read<FBookingAcknowledgeChild>().ToList();
                List<FBAChildPlanning> fbaChildPlannings = records.Read<FBAChildPlanning>().ToList();
                List<FBookingAcknowledgementLiabilityDistribution> LiabilityDistributionList = records.Read<FBookingAcknowledgementLiabilityDistribution>().ToList();
                List<FBookingAcknowledgementLiabilityDistribution> FBookingAckLiabilityDistributionList = records.Read<FBookingAcknowledgementLiabilityDistribution>().ToList();
                data.FBookingAcknowledgementYarnLiabilityList = records.Read<FBookingAcknowledgementYarnLiability>().ToList();

                //criteriaNames.ForEach(cn =>
                //{
                //    cn.FBAChildPlannings = fbaChildPlannings.Where(x => x.CriteriaName == cn.CriteriaName).ToList();
                //});

                List<FBookingAcknowledgementLiabilityDistribution> tmpLB = new List<FBookingAcknowledgementLiabilityDistribution>();
                for (int i = 0; i < bookingChilds.Count; i++)
                {
                    tmpLB = new List<FBookingAcknowledgementLiabilityDistribution>();
                    tmpLB.AddRange(LiabilityDistributionList);
                    bookingChilds[i].CriteriaNames = criteriaNames;
                    bookingChilds[i].FBAChildPlannings = fbaChildPlannings;
                    bookingChilds[i].ChildAckLiabilityDetails = tmpLB.ConvertAll(x => new FBookingAcknowledgementLiabilityDistribution(x)).ToList();

                    foreach (FBookingAcknowledgementLiabilityDistribution obitem in bookingChilds[i].ChildAckLiabilityDetails)
                    {
                        List<FBookingAcknowledgementLiabilityDistribution> objLBList = FBookingAckLiabilityDistributionList.Where(j => j.BookingChildID == bookingChilds[i].BookingChildID && j.LiabilitiesProcessID == obitem.LiabilitiesProcessID).ToList();
                        foreach (FBookingAcknowledgementLiabilityDistribution objLB in objLBList)
                        {
                            if (objLB.IsNotNull())
                            {
                                obitem.LChildID = objLB.LChildID;
                                obitem.BookingChildID = objLB.BookingChildID;
                                obitem.AcknowledgeID = objLB.AcknowledgeID;
                                obitem.BookingID = objLB.BookingID;
                                obitem.UnitID = objLB.UnitID;
                                obitem.LiabilityQty = objLB.LiabilityQty;
                            }
                        }

                    }
                }

                tmpLB = new List<FBookingAcknowledgementLiabilityDistribution>();
                for (int i = 0; i < bookingChilds.Count; i++)
                {
                    tmpLB = new List<FBookingAcknowledgementLiabilityDistribution>();
                    tmpLB.AddRange(LiabilityDistributionList);
                    bookingChilds[i].CriteriaNames = criteriaNames;
                    bookingChilds[i].FBAChildPlannings = fbaChildPlannings;
                    bookingChilds[i].ChildAckLiabilityDetails = tmpLB.ConvertAll(x => new FBookingAcknowledgementLiabilityDistribution(x)).ToList();

                    foreach (FBookingAcknowledgementLiabilityDistribution obitem in bookingChilds[i].ChildAckLiabilityDetails)
                    {
                        List<FBookingAcknowledgementLiabilityDistribution> objLBList = FBookingAckLiabilityDistributionList.Where(j => j.BookingChildID == bookingChilds[i].BookingChildID && j.LiabilitiesProcessID == obitem.LiabilitiesProcessID).ToList();
                        foreach (FBookingAcknowledgementLiabilityDistribution objLB in objLBList)
                        {
                            if (objLB.IsNotNull())
                            {
                                obitem.LChildID = objLB.LChildID;
                                obitem.BookingChildID = objLB.BookingChildID;
                                obitem.AcknowledgeID = objLB.AcknowledgeID;
                                obitem.BookingID = objLB.BookingID;
                                obitem.UnitID = objLB.UnitID;
                                obitem.LiabilityQty = objLB.LiabilityQty;
                            }
                        }

                    }
                }

                data.FBookingChild = bookingChilds.Where(x => x.SubGroupID == 1).ToList();
                data.FBookingChildCollor = bookingChildsCollarCuff.Where(x => x.SubGroupID == 11).ToList();
                data.FBookingChildCuff = bookingChildsCollarCuff.Where(x => x.SubGroupID == 12).ToList();
                data.HasFabric = data.FBookingChild.Count() > 0 ? true : false;
                data.HasCollar = data.FBookingChildCollor.Count() > 0 ? true : false;
                data.HasCuff = data.FBookingChildCuff.Count() > 0 ? true : false;

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
        */
        public async Task<FBookingAcknowledge> GetNewBulkFabricAsync(int bookingId)
        {
            var query = $@"Select *From FabricBookingAcknowledge WHERE BookingID = {bookingId};

                        WITH FBA1 AS
                        (
                            SELECT a.BookingID,a.BOMMasterID,a.ItemGroupID,a.SubGroupID,a.Status,PreRevisionNo=a.PreProcessRevNo,RevisionNo=a.RevisionNo,
	                        b.BookingNo,b.BookingDate,b.Remarks,StyleNo='',b.ExportOrderID,SLNo='',StyleMasterID=0,
	                        b.BuyerID,b.BuyerTeamID,b.CompanyID,b.SupplierID,SeasonID=0,a.WithoutOB
                            FROM FabricBookingAcknowledge A
							INNER JOIN {DbNames.EPYSL}..BookingMaster b on b.BookingID = a.BookingID
                            WHERE A.BookingID = {bookingId}
                        ),
						FBA2 AS
                        (
                            SELECT a.BookingID,a.BOMMasterID,a.ItemGroupID,a.SubGroupID,a.Status,PreRevisionNo=a.PreProcessRevNo,RevisionNo=a.RevisionNo,
	                        b.BookingNo,b.BookingDate,b.Remarks,b.StyleNo,b.ExportOrderID,b.SLNo,b.StyleMasterID,
	                        b.BuyerID,b.BuyerTeamID,CompanyID=b.ExecutionCompanyID,b.SupplierID,b.SeasonID,a.WithoutOB
                            FROM FabricBookingAcknowledge A
							Inner Join {DbNames.EPYSL}..SampleBookingMaster b on b.BookingID = a.BookingID
                            WHERE A.BookingID = {bookingId}
                        ),
						FBA AS
                        (
	                        SELECT *FROM FBA1
	                        UNION
	                        SELECT *FROM FBA2
                        ),
                        F AS(
                        SELECT FBA.*,CTO.ContactDisplayCode AS BuyerName, CCT.DisplayCode AS BuyerTeamName,CompanyName = C.ShortName,
                        Supplier.ShortName [SupplierName],Season.SeasonName, ISNULL(Supplier.MappingCompanyID,0) TextileCompanyID
                        FROM FBA
                        LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = FBA.BuyerID
                        LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = FBA.BuyerTeamID
                        LEFT JOIN {DbNames.EPYSL}..CompanyEntity C ON C.CompanyID = FBA.CompanyID
                        LEFT Join {DbNames.EPYSL}..Contacts Supplier On Supplier.ContactID = FBA.SupplierID
                        LEFT Join {DbNames.EPYSL}..ContactSeason Season On Season.SeasonID = FBA.SeasonID)

						SELECT * FROM F;

                        ----- Sample Booking Consumption Child (Fabric)
                           WITH FBA1 AS
                        (
                            SELECT B.BookingID,B.BookingNo, B.BuyerID, ExecutionCompanyID=B.CompanyID,
							A.BookingChildID, A.ItemMasterID,A.SubGroupID,A.A1ValueID,A.YarnBrandID,ReferenceSourceID=0,YarnSourceID=0,
							ReferenceNo='',ColorReferenceNo='',A.ConsumptionID,A.ItemGroupID, B.RevisionNo,
							C.Segment1Desc,C.Segment2Desc,C.Segment3Desc,
							C.Segment4Desc,C.Segment5Desc,C.Segment6Desc, C.Segment7Desc,
							A.LengthYds,A.LengthInch,A.FUPartID,A.ConsumptionQty,C.OrderUnitID,A.LabDipNo,A.Price,A.SuggestedPrice,
							RequiredQty=A.RequisitionQty,A.Remarks,ForBDSStyleNo='', B.SupplierID,LiabilitiesBookingQty=ISNULL(FBAC.LiabilitiesBookingQty,0),ActualBookingQty=ISNULL(FBAC.ActualBookingQty,0)
							FROM {DbNames.EPYSL}..BookingChild A
							LEFT JOIN {DbNames.EPYSL}..BookingMaster B ON B.BookingID=A.BookingID
							LEFT JOIN {DbNames.EPYSL}..BOMConsumption C ON C.ConsumptionID=A.ConsumptionID
                            LEFT JOIN FBookingAcknowledgeChild FBAC On FBAC.ConsumptionID = C.ConsumptionID And FBAC.ItemMasterID = A.ItemMasterID
							WHERE A.BookingID = {bookingId} AND A.SubGroupID = 1
                        ),
						FBA2 AS
                        (
                           SELECT B.BookingID,B.BookingNo, B.BuyerID,B.ExecutionCompanyID,
						   BookingChildID=A.ConsumptionChildID, A.ItemMasterID,A.SubGroupID,C.A1ValueID,C.YarnBrandID,C.ReferenceSourceID,C.YarnSourceID,
						   C.ReferenceNo,C.ColorReferenceNo,A.ConsumptionID,A.ItemGroupID, B.RevisionNo,
						   C.Segment1Desc,C.Segment2Desc,C.Segment3Desc,
						   C.Segment4Desc,C.Segment5Desc,C.Segment6Desc, C.Segment7Desc,
						   C.LengthYds,C.LengthInch,C.FUPartID,A.ConsumptionQty,C.OrderUnitID,C.LabDipNo,C.Price,C.SuggestedPrice,
						   A.RequiredQty,C.Remarks,C.ForBDSStyleNo, B.SupplierID,LiabilitiesBookingQty=ISNULL(FBAC.LiabilitiesBookingQty,0),ActualBookingQty=ISNULL(FBAC.ActualBookingQty,0)
						   FROM {DbNames.EPYSL}..SampleBookingConsumptionChild A
						   LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster B ON B.BookingID=A.BookingID
						   LEFT JOIN {DbNames.EPYSL}..SampleBookingConsumption C ON C.ConsumptionID=A.ConsumptionID
                           LEFT JOIN FBookingAcknowledgeChild FBAC On FBAC.ConsumptionID = C.ConsumptionID And FBAC.ItemMasterID = A.ItemMasterID
						   WHERE A.BookingID = {bookingId} AND A.SubGroupID = 1
                        ),
						FBA AS
						(
							SELECT * FROM FBA1
							UNION
							SELECT * FROM FBA2
						),
						F AS (SELECT SBM.BookingChildID, SBM.ReferenceSourceID,RSETV.ValueName ReferenceSourceName , SBM.ReferenceNo,SBM.ColorReferenceNo,SBM.YarnSourceID,SBM.ConsumptionID,SBM.BookingID,SBM.BookingNo,SBM.ItemGroupID,SBM.SubGroupID,SBM.Segment1Desc Construction,SBM.Segment2Desc Composition,SBM.Segment3Desc Color
						,SBM.Segment4Desc GSM,SBM.Segment5Desc FabricWidth,SBM.Segment7Desc KnittingType,SBM.LengthYds,SBM.LengthInch,SBM.FUPartID,SBM.ConsumptionQty,SBM.OrderUnitID BookingUnitID
						,SBM.A1ValueID,SBM.YarnBrandID,SBM.LabDipNo,SBM.Price,SBM.SuggestedPrice,PreviousBookingQty=IsNull((Select Sum(BookingQty) From {DbNames.EPYSL}..BookingChild_Bk Where RevisionNo = SBM.RevisionNo-1 And ConsumptionID =  SBM.ConsumptionID),0),SBM.RequiredQty BookingQty,SBM.ItemMasterID,SBM.BuyerID ContactID,
						SBM.ExecutionCompanyID,
						0 As TechnicalNameId,ISG.SubGroupName,'1' As ConceptTypeID,IM.Segment1ValueID ConstructionId, IM.Segment2ValueID CompositionId,
						IM.Segment3ValueID ColorId,IM.Segment7ValueID KnittingTypeId,IM.Segment4ValueID GSMId,
						ISV.SegmentValue YarnType, ETV.ValueName YarnProgram, SBM.Segment6Desc DyeingType, SBM.Remarks Instruction, SBM.ForBDSStyleNo, ISNULL(Supplier.MappingCompanyID,0) TextileCompanyID,LiabilitiesBookingQty=ISNULL(SBM.LiabilitiesBookingQty,0),ActualBookingQty=ISNULL(SBM.ActualBookingQty,0)
						FROM FBA SBM
                        LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = SBM.SubGroupID
                        LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = SBM.ItemMasterID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV On ISV.SegmentValueID = SBM.A1ValueID
				        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = SBM.YarnBrandID
                        Left Join {DbNames.EPYSL}..EntityTypeValue RSETV ON RSETV.ValueID = SBM.ReferenceSourceID
                        Left Join {DbNames.EPYSL}..EntityTypeValue YSETV ON YSETV.ValueID = SBM.YarnSourceID
                        LEFT Join {DbNames.EPYSL}..Contacts Supplier On Supplier.ContactID = SBM.SupplierID
                        WHERE SBM.BookingID = {bookingId}
						)
						SELECT * FROM F;

                        ----- Sample Booking Consumption Child (Collor & Cuff)
                          WITH FBA1 AS
                        (
                            SELECT B.BookingID,B.BookingNo, B.BuyerID, ExecutionCompanyID=B.CompanyID,
							A.BookingChildID, A.ItemMasterID,A.SubGroupID,A.A1ValueID,A.YarnBrandID,ReferenceSourceID=0,YarnSourceID=0,
							ReferenceNo='',ColorReferenceNo='',A.ConsumptionID,A.ItemGroupID, B.RevisionNo,
							C.Segment1Desc,C.Segment2Desc,C.Segment3Desc,
							C.Segment4Desc,C.Segment5Desc,C.Segment6Desc, C.Segment7Desc,
							A.LengthYds,A.LengthInch,A.FUPartID,A.ConsumptionQty,C.OrderUnitID,A.LabDipNo,A.Price,A.SuggestedPrice,
							RequiredQty=A.RequisitionQty,A.Remarks,ForBDSStyleNo='', B.SupplierID
							FROM {DbNames.EPYSL}..BookingChild A
							LEFT JOIN {DbNames.EPYSL}..BookingMaster B ON B.BookingID=A.BookingID
							LEFT JOIN {DbNames.EPYSL}..BOMConsumption C ON C.ConsumptionID=A.ConsumptionID
							WHERE B.BookingNo in (select BookingNo From {DbNames.EPYSL}..Bookingmaster Where BookingID = {bookingId}) AND A.SubGroupID IN (11,12)
                        ),
						FBA2 AS
                        (
                           SELECT B.BookingID,B.BookingNo, B.BuyerID,B.ExecutionCompanyID,
						   BookingChildID=A.ConsumptionChildID, A.ItemMasterID,A.SubGroupID,C.A1ValueID,C.YarnBrandID,C.ReferenceSourceID,C.YarnSourceID,
						   C.ReferenceNo,C.ColorReferenceNo,A.ConsumptionID,A.ItemGroupID, B.RevisionNo,
						   C.Segment1Desc,C.Segment2Desc,C.Segment3Desc,
						   C.Segment4Desc,C.Segment5Desc,C.Segment6Desc, C.Segment7Desc,
						   C.LengthYds,C.LengthInch,C.FUPartID,A.ConsumptionQty,C.OrderUnitID,C.LabDipNo,C.Price,C.SuggestedPrice,
						   A.RequiredQty,C.Remarks,C.ForBDSStyleNo, B.SupplierID
						   FROM {DbNames.EPYSL}..SampleBookingConsumptionChild A
						   LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster B ON B.BookingID=A.BookingID
						   LEFT JOIN {DbNames.EPYSL}..SampleBookingConsumption C ON C.ConsumptionID=A.ConsumptionID
						   WHERE B.BookingNo in (select BookingNo From {DbNames.EPYSL}..Bookingmaster Where BookingID = {bookingId}) AND A.SubGroupID IN (11,12)
                        ),
						FBA AS
						(
							SELECT * FROM FBA1
							UNION
							SELECT * FROM FBA2
						),
						F AS (SELECT SBM.BookingChildID, SBM.ReferenceSourceID,RSETV.ValueName ReferenceSourceName , SBM.ReferenceNo,SBM.ColorReferenceNo,SBM.YarnSourceID,SBM.ConsumptionID,SBM.BookingID,SBM.BookingNo,SBM.ItemGroupID,SBM.SubGroupID,SBM.Segment1Desc,SBM.Segment2Desc,SBM.Segment3Desc
						,SBM.Segment4Desc,SBM.Segment5Desc,SBM.Segment7Desc,SBM.LengthYds,SBM.LengthInch,SBM.FUPartID,SBM.ConsumptionQty,SBM.OrderUnitID BookingUnitID
						,SBM.A1ValueID,SBM.YarnBrandID,SBM.LabDipNo,SBM.Price,SBM.SuggestedPrice,PreviousBookingQty=IsNull((Select Sum(BookingQty) From {DbNames.EPYSL}..BookingChild_Bk Where RevisionNo = SBM.RevisionNo-1 And ConsumptionID =  SBM.ConsumptionID),0),SBM.RequiredQty BookingQty,SBM.ItemMasterID,SBM.BuyerID ContactID,
						SBM.ExecutionCompanyID,
						0 As TechnicalNameId,ISG.SubGroupName,'1' As ConceptTypeID,IM.Segment1ValueID, IM.Segment2ValueID,
						IM.Segment3ValueID,IM.Segment4ValueID,IM.Segment5ValueID,IM.Segment6ValueID,IM.Segment7ValueID,
						ISV.SegmentValue YarnType, ETV.ValueName YarnProgram, SBM.Segment6Desc DyeingType, SBM.Remarks Instruction, SBM.ForBDSStyleNo, ISNULL(Supplier.MappingCompanyID,0) TextileCompanyID
						FROM FBA SBM
                        LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = SBM.SubGroupID
                        LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = SBM.ItemMasterID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV On ISV.SegmentValueID = SBM.A1ValueID
				        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = SBM.YarnBrandID
                        Left Join {DbNames.EPYSL}..EntityTypeValue RSETV ON RSETV.ValueID = SBM.ReferenceSourceID
                        Left Join {DbNames.EPYSL}..EntityTypeValue YSETV ON YSETV.ValueID = SBM.YarnSourceID
                        LEFT Join {DbNames.EPYSL}..Contacts Supplier On Supplier.ContactID = SBM.SupplierID
						)
						SELECT * FROM F;

                       ----Sample Booking Consumption AddProcess
                        SELECT  BookingID ,ProcessID,ConsumptionID
                        FROM {DbNames.EPYSL}..SampleBookingConsumptionAddProcess where BookingID={bookingId};

                        ----- Sample Booking Consumption Child Details
                        SELECT ConsumptionID,BookingID,ItemGroupID,SubGroupID,ItemMasterID,RequiredQty BookingQty ,ConsumptionQty,RequiredUnitID BookingUnitID, 0 As TechnicalNameId
                        FROM {DbNames.EPYSL}..SampleBookingConsumptionChild where BookingID={bookingId}

                        ----- Sample Booking Consumption Garment Part
                        SELECT  BookingID ,FUPartID,ConsumptionID
                         FROM {DbNames.EPYSL}..SampleBookingConsumptionGarmentPart where BookingID={bookingId}

                        ---Sample Booking Consumption Process
                        SELECT BookingID,ProcessID,ConsumptionID
                        FROM {DbNames.EPYSL}..SampleBookingConsumptionProcess where BookingID={bookingId}

                        ------ Sample Booking Consumption Text
                        SELECT BookingID,UsesIn,AdditionalProcess,ApplicableProcess,YarnSubProgram,GarmentsColor GmtColor,ConsumptionID
                        FROM {DbNames.EPYSL}..SampleBookingConsumptionText  where BookingID={bookingId}

                        ------ Sample Booking Child Distribution 
                        SELECT *
                        FROM {DbNames.EPYSL}..SampleBookingChildDistribution  where BookingID={bookingId}

                        ----- SampleBooking ConsumptionYarn Sub Brand
                        SELECT SBKC.ReferenceSourceID,RSETV.ValueName ReferenceSourceName , SBKC.ReferenceNo,SBKC.ColorReferenceNo,SBKC.YarnSourceID,SCYS.BookingID, SCYS.YarnSubBrandID, SCYS.ConsumptionID, YSB.YarnSubBrandName
				        FROM {DbNames.EPYSL}..SampleBookingConsumptionYarnSubBrand SCYS
				        Inner Join {DbNames.EPYSL}..SampleBookingConsumption SBKC On SCYS.BookingID = SBKC.BookingID And SCYS.ConsumptionID = SBKC.ConsumptionID
                        Inner Join {DbNames.EPYSL}..ItemGroup IG On IG.ItemGroupID = SBKC.ItemGroupID
                        Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = SBKC.SubGroupID
                        Left Join {DbNames.EPYSL}..EntityTypeValue RSETV ON RSETV.ValueID = SBKC.ReferenceSourceID
                        Left Join  {DbNames.EPYSL}..EntityTypeValue YSETV ON YSETV.ValueID = SBKC.YarnSourceID
				        Inner Join (
					        Select ETV.ValueID YarnSubBrandID, ETV.ValueName YarnSubBrandName
					        From {DbNames.EPYSL}..EntityTypeValue ETV
					        Inner Join {DbNames.EPYSL}..EntityType ET On ET.EntityTypeID = ETV.EntityTypeID
				        ) YSB On YSB.YarnSubBrandID = SCYS.YarnSubBrandID
				        Where SCYS.BookingID = {bookingId};

                        ----- SampleBookingChildImage
                        SELECT BookingID,ImagePath
                        FROM {DbNames.EPYSL}..SampleBookingChildImage where BookingID={bookingId}

                        --Free Concept
                        Select SBC.ReferenceSourceID,RSETV.ValueName ReferenceSourceName , SBC.ReferenceNo,SBC.ColorReferenceNo,SBC.YarnSourceID,FB.BookingID, FB.BookingNo, FB.BookingDate ConceptDate, SBC.RequiredQty Qty,
                        CC.ItemMasterID, IM.Segment1ValueID ConstructionId, IM.Segment2ValueID CompositionId, IM.Segment3ValueID ColorId, Color.SegmentValue ColorName, IM.Segment4ValueID GSMId,
                        IM.Segment7ValueID KnittingTypeId, IM.SubGroupID,FB.ExecutionCompanyID CompanyID, ISNULL(Supplier.MappingCompanyID,0) TextileCompanyID
                        FROM {DbNames.EPYSL}..SampleBookingConsumption SBC
                        INNER JOIN {DbNames.EPYSL}..SampleBookingConsumptionChild CC ON CC.ConsumptionID = SBC.ConsumptionID
                        INNER JOIN {DbNames.EPYSL}..SampleBookingMaster FB ON FB.BookingID = SBC.BookingID
                        INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = CC.ItemMasterID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = IM.Segment1ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = IM.Segment2ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = IM.Segment3ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = IM.Segment4ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Width ON Width.SegmentValueID = IM.Segment5ValueID
                        --LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue DT ON DT.SegmentValueID = IM.Segment6ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue KT ON KT.SegmentValueID = IM.Segment7ValueID
                        --LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID = FB.SubGroupID
                        Left Join  {DbNames.EPYSL}..EntityTypeValue RSETV ON RSETV.ValueID = SBC.ReferenceSourceID
                        Left Join  {DbNames.EPYSL}..EntityTypeValue YSETV ON YSETV.ValueID = SBC.YarnSourceID
                        LEFT Join {DbNames.EPYSL}..Contacts Supplier On Supplier.ContactID = FB.SupplierID
                        where FB.BookingID={bookingId};

                        --Technical Name
                        SELECT Cast(T.TechnicalNameId As varchar) id, T.TechnicalName [text], ISNULL(ST.[Days], 0) [desc], Cast(SC.SubClassID as varchar) additionalValue
                        FROM FabricTechnicalName T
                        LEFT JOIN FabricTechnicalNameKMachineSubClass SC ON SC.TechnicalNameID = T.TechnicalNameId
                        LEFT JOIN KnittingMachineStructureType_HK ST ON ST.StructureTypeID = SC.StructureTypeID
                        Group By T.TechnicalNameId, T.TechnicalName, ST.Days, SC.SubClassID;

                         --YarnSource data load
				        Select Cast(a.ValueID as varchar) id, a.ValueName [text]
				        From {DbNames.EPYSL}..EntityTypeValue a
				        Inner Join {DbNames.EPYSL}..EntityType b on b.EntityTypeID = a.EntityTypeID
				        Where b.EntityTypeName = 'Yarn Source'

                        --M/c type
                        ;SELECT CAST(a.MachineSubClassID AS varchar) [id], b.SubClassName [text], b.TypeID [desc], c.TypeName additionalValue
                        FROM KnittingMachine a
                        INNER JOIN KnittingMachineSubClass b ON b.SubClassID = a.MachineSubClassID
                        Inner Join KnittingMachineType c On c.TypeID = b.TypeID
                        --Where c.TypeName != 'Flat Bed'
                        GROUP BY a.MachineSubClassID, b.SubClassName, b.TypeID, c.TypeName;

                        --CriteriaNames
                         ;SELECT CriteriaName,CriteriaSeqNo, (CASE WHEN CriteriaName  IN('Batch Preparation','Quality Check') THEN '1'ELSE'0'END) AS TotalTime
                        FROM BDSCriteria_HK --WHERE CriteriaName NOT IN('Batch Preparation','Testing')
                        GROUP BY CriteriaSeqNo,CriteriaName order by CriteriaSeqNo,CriteriaName;

                        --FBAChildPlannings
                        ;SELECT * FROM BDSCriteria_HK order by CriteriaSeqNo, OperationSeqNo, CriteriaName;
                        
                        --Liability Process
				        Select LChildID = 0,BookingChildID = 0,AcknowledgeID = 0,BookingID = 0,UnitID = 0, Cast(a.ValueID as varchar) LiabilitiesProcessID, a.ValueName LiabilitiesName,LiabilityQty=0
				        From {DbNames.EPYSL}..EntityTypeValue a
				        Inner Join {DbNames.EPYSL}..EntityType b on b.EntityTypeID = a.EntityTypeID
				        Where b.EntityTypeName = 'Process Liability'
                        --Liability Process data load
				        Select LChildID = IsNull(F.LChildID,0),BookingChildID = IsNull(F.BookingChildID,0),AcknowledgeID = IsNull(F.AcknowledgeID,0),BookingID = IsNull(F.BookingID,0),UnitID = IsNull(F.UnitID,0), Cast(a.ValueID as varchar) LiabilitiesProcessID, a.ValueName LiabilitiesName,LiabilityQty=IsNull(F.LiabilityQty,0)
				        From {DbNames.EPYSL}..EntityTypeValue a
				        Inner Join {DbNames.EPYSL}..EntityType b on b.EntityTypeID = a.EntityTypeID
				        Left Join (Select LChildID,BookingChildID,AcknowledgeID,BookingID,LiabilitiesProcessID,UnitID,LiabilityQty From FBookingAcknowledgementLiabilityDistribution Where BookingID = {bookingId} Group By LChildID,BookingChildID,AcknowledgeID,BookingID,LiabilitiesProcessID,UnitID,LiabilityQty)F On F.LiabilitiesProcessID = a.ValueID
				        Where b.EntityTypeName = 'Process Liability';
                    

                    ;With YBM As
                    (
                        Select * From YarnBookingMaster Where BookingID = {bookingId}
                    ), YBM_New As
                    (
                        Select * From YarnBookingMaster_New Where BookingID = {bookingId}
                    ),
                    YBCI As (
                        Select YBCI.YItemMasterID As ItemMasterID, YBCI.UnitID, U.DisplayUnitDesc, YBCI.Blending,
                        (Case When Blending = 1 then 'Blend' else 'Non-Blend' End)BlendingName, YBCI.YarnCategory,  BookingQty=Sum(YBCI.BookingQty),
                        ShadeCode= IsNull(YBCI.ShadeCode,''), IsNull(Y.ShadeCode,'') as ShadeName,
                        YBCI.Remarks, YBCI.Specification, YBCI.YD, YBCI.YDItem, YBM.BookingID,
                        ISV1.SegmentValue As _segment1ValueDesc, ISV2.SegmentValue As _segment2ValueDesc, ISV3.SegmentValue As _segment3ValueDesc,
                        ISV4.SegmentValue As _segment4ValueDesc, ISV5.SegmentValue As _segment5ValueDesc, ISV6.SegmentValue As _segment6ValueDesc,
                        ISV7.SegmentValue As _segment7ValueDesc, ISV8.SegmentValue As _segment8ValueDesc, YBM.SubGroupID, ISG.SubGroupName,
                        LiabilityQty = IsNull(FBAY.LiabilityQty,0)
                        From YBM Inner Join YarnBookingChild YBC On YBM.YBookingID = YBC.YBookingID
                        Inner Join YarnBookingChildItem YBCI On YBCI.YBChildID = YBC.YBChildID
                        Left Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = YBM.SubGroupID
                        INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YBCI.YItemMasterID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV8 ON ISV8.SegmentValueID = IM.Segment8ValueID
                        LEFT JOIN {DbNames.EPYSL}..Unit U ON U.UnitID = YBCI.UnitID
                        LEFT JOIN YarnShadeBook Y ON Y.ShadeCode = YBCI.ShadeCode
                        LEFT Join YDBookingMaster YDBM ON YDBM.YBookingID = YBM.YBookingID And YDBM.YBookingID = YBCI.YBookingID
                        LEFT Join YDProductionMaster YPM ON YPM.YDBookingMasterID = YDBM.YDBookingMasterID
						Left Join FBookingAcknowledgementYarnLiability FBAY On FBAY.BookingID = YBM.BookingID And FBAY.ItemMasterID = YBCI.YItemMasterID
						Group By YBCI.YItemMasterID, YBCI.UnitID, U.DisplayUnitDesc, YBCI.Blending,
                        (Case When Blending = 1 then 'Blend' else 'Non-Blend' End), YBCI.YarnCategory, 
                        IsNull(YBCI.ShadeCode,''), Y.ShadeCode,
                        YBCI.Remarks, YBCI.Specification, YBCI.YD, YBCI.YDItem, YBM.BookingID,
                        ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue,
                        ISV4.SegmentValue, ISV5.SegmentValue, ISV6.SegmentValue,
                        ISV7.SegmentValue, ISV8.SegmentValue, YBM.SubGroupID, ISG.SubGroupName,
                        IsNull(FBAY.LiabilityQty,0)
                        Union
                        Select YBCI.YItemMasterID As ItemMasterID, YBCI.UnitID, U.DisplayUnitDesc, YBCI.Blending,
                        (Case When Blending = 1 then 'Blend' else 'Non-Blend' End)BlendingName, YBCI.YarnCategory,  BookingQty=Sum(YBCI.BookingQty),
                        ShadeCode= IsNull(YBCI.ShadeCode,''), IsNull(Y.ShadeCode,'') as ShadeName,
                        YBCI.Remarks, YBCI.Specification, YBCI.YD, YBCI.YDItem, YBM.BookingID,
                        ISV1.SegmentValue As _segment1ValueDesc, ISV2.SegmentValue As _segment2ValueDesc, ISV3.SegmentValue As _segment3ValueDesc,
                        ISV4.SegmentValue As _segment4ValueDesc, ISV5.SegmentValue As _segment5ValueDesc, ISV6.SegmentValue As _segment6ValueDesc,
                        ISV7.SegmentValue As _segment7ValueDesc, ISV8.SegmentValue As _segment8ValueDesc, YBM.SubGroupID, ISG.SubGroupName,
                        LiabilityQty = IsNull(FBAY.LiabilityQty,0)
                        From YBM_New YBM
                        Inner Join YarnBookingChild_New YBC On YBM.YBookingID = YBC.YBookingID
                        Inner Join YarnBookingChildItem_New YBCI On YBCI.YBChildID = YBC.YBChildID
                        LEFT JOIN YarnItemPrice YIP ON YIP.YBChildItemID = YBCI.YBChildItemID AND ISNULL(YIP.IsTextileERP,0) = 1
                        Left Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = YBM.SubGroupID
                        INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YBCI.YItemMasterID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV8 ON ISV8.SegmentValueID = IM.Segment8ValueID
                        LEFT JOIN {DbNames.EPYSL}..Unit U ON U.UnitID = YBCI.UnitID
                        LEFT JOIN YarnShadeBook Y ON Y.ShadeCode = YBCI.ShadeCode
                        LEFT Join YDBookingMaster YDBM ON YDBM.YBookingID = YBM.YBookingID And YDBM.YBookingID = YBCI.YBookingID
                        LEFT Join YDProductionMaster YPM ON YPM.YDBookingMasterID = YDBM.YDBookingMasterID
						Left Join FBookingAcknowledgementYarnLiability FBAY On FBAY.BookingID = YBM.BookingID And FBAY.ItemMasterID = YBCI.YItemMasterID
						Group By YBCI.YItemMasterID, YBCI.UnitID, U.DisplayUnitDesc, YBCI.Blending,
                        (Case When Blending = 1 then 'Blend' else 'Non-Blend' End), YBCI.YarnCategory, 
                        IsNull(YBCI.ShadeCode,''), Y.ShadeCode,
                        YBCI.Remarks, YBCI.Specification, YBCI.YD, YBCI.YDItem, YBM.BookingID,
                        ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue,
                        ISV4.SegmentValue, ISV5.SegmentValue, ISV6.SegmentValue,
                        ISV7.SegmentValue, ISV8.SegmentValue, YBM.SubGroupID, ISG.SubGroupName,
                        IsNull(FBAY.LiabilityQty,0)
                    )
					Select *From YBCI";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                List<FabricBookingAcknowledge> fbaList = records.Read<FabricBookingAcknowledge>().ToList();
                FBookingAcknowledge data = records.Read<FBookingAcknowledge>().FirstOrDefault();
                Guard.Against.NullObject(data);

                List<FBookingAcknowledgeChild> bookingChilds = records.Read<FBookingAcknowledgeChild>().ToList();
                List<FBookingAcknowledgeChild> bookingChildsCollarCuff = records.Read<FBookingAcknowledgeChild>().ToList();
                data.FabricBookingAcknowledgeList = fbaList;
                data.FBookingAcknowledgeChildAddProcess = records.Read<FBookingAcknowledgeChildAddProcess>().ToList();
                data.FBookingChildDetails = records.Read<FBookingAcknowledgeChildDetails>().ToList();
                data.FBookingAcknowledgeChildGarmentPart = records.Read<FBookingAcknowledgeChildGarmentPart>().ToList();
                data.FBookingAcknowledgeChildProcess = records.Read<FBookingAcknowledgeChildProcess>().ToList();
                data.FBookingAcknowledgeChildText = records.Read<FBookingAcknowledgeChildText>().ToList();
                data.FBookingAcknowledgeChildDistribution = records.Read<FBookingAcknowledgeChildDistribution>().ToList();
                data.FBookingAcknowledgeChildYarnSubBrand = records.Read<FBookingAcknowledgeChildYarnSubBrand>().ToList();
                data.FBookingAcknowledgeImage = records.Read<FBookingAcknowledgeImage>().ToList();
                data.FreeConcepts = records.Read<FreeConceptMaster>().ToList();
                data.TechnicalNameList = await records.ReadAsync<Select2OptionModel>();
                data.YarnSourceNameList = await records.ReadAsync<Select2OptionModel>();

                List<Select2OptionModel> mcTypeList = records.Read<Select2OptionModel>().ToList();
                data.MCTypeForFabricList = mcTypeList.Where(x => x.additionalValue != "Flat Bed");
                data.MCTypeForOtherList = mcTypeList.Where(x => x.additionalValue == "Flat Bed");

                List<FBookingAcknowledgeChild> criteriaNames = records.Read<FBookingAcknowledgeChild>().ToList();
                List<FBAChildPlanning> fbaChildPlannings = records.Read<FBAChildPlanning>().ToList();
                List<FBookingAcknowledgementLiabilityDistribution> LiabilityDistributionList = records.Read<FBookingAcknowledgementLiabilityDistribution>().ToList();
                List<FBookingAcknowledgementLiabilityDistribution> FBookingAckLiabilityDistributionList = records.Read<FBookingAcknowledgementLiabilityDistribution>().ToList();
                data.FBookingAcknowledgementYarnLiabilityList = records.Read<FBookingAcknowledgementYarnLiability>().ToList();
                //criteriaNames.ForEach(cn =>
                //{
                //    cn.FBAChildPlannings = fbaChildPlannings.Where(x => x.CriteriaName == cn.CriteriaName).ToList();
                //});
                List<FBookingAcknowledgementLiabilityDistribution> tmpLB = new List<FBookingAcknowledgementLiabilityDistribution>();
                for (int i = 0; i < bookingChilds.Count; i++)
                {
                    tmpLB = new List<FBookingAcknowledgementLiabilityDistribution>();
                    tmpLB.AddRange(LiabilityDistributionList);
                    bookingChilds[i].CriteriaNames = criteriaNames;
                    bookingChilds[i].FBAChildPlannings = fbaChildPlannings;
                    bookingChilds[i].ChildAckLiabilityDetails = tmpLB.ConvertAll(x => new FBookingAcknowledgementLiabilityDistribution(x)).ToList();

                    foreach (FBookingAcknowledgementLiabilityDistribution obitem in bookingChilds[i].ChildAckLiabilityDetails)
                    {
                        List<FBookingAcknowledgementLiabilityDistribution> objLBList = FBookingAckLiabilityDistributionList.Where(j => j.BookingChildID == bookingChilds[i].BookingChildID && j.LiabilitiesProcessID == obitem.LiabilitiesProcessID).ToList();
                        foreach (FBookingAcknowledgementLiabilityDistribution objLB in objLBList)
                        {
                            if (objLB.IsNotNull())
                            {
                                obitem.LChildID = objLB.LChildID;
                                obitem.BookingChildID = objLB.BookingChildID;
                                obitem.AcknowledgeID = objLB.AcknowledgeID;
                                obitem.BookingID = objLB.BookingID;
                                obitem.UnitID = objLB.UnitID;
                                obitem.LiabilityQty = objLB.LiabilityQty;
                            }
                        }

                    }
                }

                tmpLB = new List<FBookingAcknowledgementLiabilityDistribution>();
                int m = 0;
                for (m = 0; m < bookingChildsCollarCuff.Count; m++)
                {
                    tmpLB = new List<FBookingAcknowledgementLiabilityDistribution>();
                    tmpLB.AddRange(LiabilityDistributionList);
                    bookingChildsCollarCuff[m].CriteriaNames = criteriaNames;
                    bookingChildsCollarCuff[m].FBAChildPlannings = fbaChildPlannings;
                    bookingChildsCollarCuff[m].ChildAckLiabilityDetails = tmpLB.ConvertAll(x => new FBookingAcknowledgementLiabilityDistribution(x)).ToList();

                    foreach (FBookingAcknowledgementLiabilityDistribution obitem in bookingChildsCollarCuff[m].ChildAckLiabilityDetails)
                    {
                        List<FBookingAcknowledgementLiabilityDistribution> objLBList = FBookingAckLiabilityDistributionList.Where(j => j.BookingChildID == bookingChildsCollarCuff[m].BookingChildID && j.LiabilitiesProcessID == obitem.LiabilitiesProcessID).ToList();
                        foreach (FBookingAcknowledgementLiabilityDistribution objLB in objLBList)
                        {
                            if (objLB.IsNotNull())
                            {
                                obitem.LChildID = objLB.LChildID;
                                obitem.BookingChildID = objLB.BookingChildID;
                                obitem.AcknowledgeID = objLB.AcknowledgeID;
                                obitem.BookingID = objLB.BookingID;
                                obitem.UnitID = objLB.UnitID;
                                obitem.LiabilityQty = objLB.LiabilityQty;
                            }
                        }

                    }
                }

                data.FBookingChild = bookingChilds.Where(x => x.SubGroupID == 1).ToList();
                data.FBookingChildCollor = bookingChildsCollarCuff.Where(x => x.SubGroupID == 11).ToList();
                data.FBookingChildCuff = bookingChildsCollarCuff.Where(x => x.SubGroupID == 12).ToList();
                data.HasFabric = data.FBookingChild.Count() > 0 ? true : false;
                data.HasCollar = data.FBookingChildCollor.Count() > 0 ? true : false;
                data.HasCuff = data.FBookingChildCuff.Count() > 0 ? true : false;

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

        public async Task<FBookingAcknowledge> GetSavedBulkFabricAsync(string bookingId)
        {
            string parentQuery = $@" WITH A AS
	                                (
		                                SELECT A.AcknowledgeID,A.AcknowledgeDate,A.AddedBy,A.BOMMasterID,A.BookingID,A.DateAdded,
		                                A.DateUpdated,A.ItemGroupID,A.PreProcessRevNo,FBA.RevisionNo,A.Status,A.SubGroupID,A.UnAcknowledge,
		                                A.UnAcknowledgeBy,A.UnAcknowledgeDate,A.UpdatedBy,A.WithoutOB, FBA.UnAcknowledgeReason 
		                                FROM FabricBookingAcknowledge A
		                                LEFT JOIN FBookingAcknowledge FBA ON FBA.BookingID = A.BookingID
		                                WHERE A.BookingID in ({bookingId})
	                                )
	                                SELECT * FROM A";

            var query = $@"
                        {parentQuery};

                        ;WITH FBA1 AS
                        (
                            SELECT a.BookingID,a.BOMMasterID,a.ItemGroupID,a.SubGroupID,a.Status,PreRevisionNo=a.PreProcessRevNo,RevisionNo=a.RevisionNo,
	                        b.BookingNo,b.BookingDate,b.Remarks,SM.StyleNo,b.ExportOrderID,SLNo='',SM.StyleMasterID,
	                        b.BuyerID,b.BuyerTeamID,b.CompanyID,b.SupplierID,SM.SeasonID,a.WithoutOB,BookingQty= SUM(BC.BookingQty)
                            FROM FabricBookingAcknowledge A
							INNER JOIN {DbNames.EPYSL}..BookingMaster b on b.BookingID = a.BookingID
                            Inner Join {DbNames.EPYSL}..BookingChild BC On BC.BookingID = A.BookingID
							LEFT JOIN {DbNames.EPYSL}..ExportOrderMaster EO ON EO.ExportOrderID = B.ExportOrderID
							LEFT JOIN {DbNames.EPYSL}..StyleMaster SM ON SM.StyleMasterID = EO.StyleMasterID
    
                            WHERE A.BookingID in ({bookingId})
                            Group BY a.BookingID,a.BOMMasterID,a.ItemGroupID,a.SubGroupID,a.Status,a.PreProcessRevNo,a.RevisionNo,
	                        b.BookingNo,b.BookingDate,b.Remarks,SM.StyleNo,b.ExportOrderID,SM.StyleMasterID,
	                        b.BuyerID,b.BuyerTeamID,b.CompanyID,b.SupplierID,SM.SeasonID,a.WithoutOB
                        ),
						FBA2 AS
                        (
                            SELECT a.BookingID,a.BOMMasterID,a.ItemGroupID,a.SubGroupID,a.Status,PreRevisionNo=a.PreProcessRevNo,RevisionNo=a.RevisionNo,
	                        b.BookingNo,b.BookingDate,b.Remarks,b.StyleNo,b.ExportOrderID,b.SLNo,b.StyleMasterID,
	                        b.BuyerID,b.BuyerTeamID,CompanyID=b.ExecutionCompanyID,b.SupplierID,b.SeasonID,a.WithoutOB,BookingQty= SUM(c.RequiredQty)
                            FROM FabricBookingAcknowledge A
							Inner Join {DbNames.EPYSL}..SampleBookingMaster b on b.BookingID = a.BookingID
							Inner Join {DbNames.EPYSL}..SampleBookingConsumptionChild c ON c.BookingID = a.BookingID
                            WHERE A.BookingID in ({bookingId})
							Group By a.BookingID,a.BOMMasterID,a.ItemGroupID,a.SubGroupID,a.Status,a.PreProcessRevNo,a.RevisionNo,
	                        b.BookingNo,b.BookingDate,b.Remarks,b.StyleNo,b.ExportOrderID,b.SLNo,b.StyleMasterID,
	                        b.BuyerID,b.BuyerTeamID,b.ExecutionCompanyID,b.SupplierID,b.SeasonID,a.WithoutOB
                        ),
						FBA AS
                        (
	                        SELECT *FROM FBA1
	                        UNION
	                        SELECT *FROM FBA2
                        ),
                        F AS
                        (
	                        SELECT FBA.BookingID,FBA.BOMMasterID,FBA.ItemGroupID,FBA.SubGroupID,FBA.Status,FBA.PreRevisionNo,FBA.RevisionNo,
	                        FBA.BookingNo,FBA.BookingDate,FBA.Remarks,FBA.StyleNo,FBA.ExportOrderID,FBA.StyleMasterID,
	                        FBA.BuyerID,FBA.BuyerTeamID,FBA.CompanyID,FBA.SupplierID,FBA.SeasonID,FBA.WithoutOB,
	                        CTO.ContactDisplayCode AS BuyerName, CCT.DisplayCode AS BuyerTeamName,CompanyName = C.ShortName,
	                        Supplier.ShortName [SupplierName],Season.SeasonName, ISNULL(Supplier.MappingCompanyID,0) TextileCompanyID,
	                        GmtQtyPcs = Sum(TotalPOQty)
	                        FROM FBA
	                        LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = FBA.BuyerID
	                        LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = FBA.BuyerTeamID
	                        LEFT JOIN {DbNames.EPYSL}..CompanyEntity C ON C.CompanyID = FBA.CompanyID
	                        LEFT Join {DbNames.EPYSL}..Contacts Supplier On Supplier.ContactID = FBA.SupplierID
	                        LEFT Join {DbNames.EPYSL}..ContactSeason Season On Season.SeasonID = FBA.SeasonID
	                        LEFT JOIN {DbNames.EPYSL}..OrderBankMaster OBM On OBM.StyleMasterID = FBA.StyleMasterID
	                        LEFT JOIN {DbNames.EPYSL}..OrderBankPO OBPO On OBPO.OrderBankMasterID = OBM.OrderBankMasterID AND OBPO.IsActive = 1
	                        GROUP BY FBA.BookingID,FBA.BOMMasterID,FBA.ItemGroupID,FBA.SubGroupID,FBA.Status,FBA.PreRevisionNo,FBA.RevisionNo,
	                        FBA.BookingNo,FBA.BookingDate,FBA.Remarks,FBA.StyleNo,FBA.ExportOrderID,FBA.StyleMasterID,
	                        FBA.BuyerID,FBA.BuyerTeamID,FBA.CompanyID,FBA.SupplierID,FBA.SeasonID,FBA.WithoutOB,
	                        CTO.ContactDisplayCode, CCT.DisplayCode,C.ShortName,
	                        Supplier.ShortName,Season.SeasonName, ISNULL(Supplier.MappingCompanyID,0)
                        )

						SELECT top 1 * FROM F;

                        ;WITH FBA1 AS
                        (
                            SELECT a.BookingID,a.BOMMasterID,a.ItemGroupID,a.SubGroupID,a.Status,PreRevisionNo=a.PreProcessRevNo,RevisionNo=a.RevisionNo,
	                        b.BookingNo,b.BookingDate,b.Remarks,StyleNo='',b.ExportOrderID,SLNo='',StyleMasterID=0,
	                        b.BuyerID,b.BuyerTeamID,b.CompanyID,b.SupplierID,SeasonID=0,a.WithoutOB
                            FROM FabricBookingAcknowledge A
							INNER JOIN {DbNames.EPYSL}..BookingMaster b on b.BookingID = a.BookingID
                            WHERE A.BookingID in ({bookingId})
                        ),
						FBA2 AS
                        (
                            SELECT a.BookingID,a.BOMMasterID,a.ItemGroupID,a.SubGroupID,a.Status,PreRevisionNo=a.PreProcessRevNo,RevisionNo=a.RevisionNo,
	                        b.BookingNo,b.BookingDate,b.Remarks,b.StyleNo,b.ExportOrderID,b.SLNo,b.StyleMasterID,
	                        b.BuyerID,b.BuyerTeamID,CompanyID=b.ExecutionCompanyID,b.SupplierID,b.SeasonID,a.WithoutOB
                            FROM FabricBookingAcknowledge A
							Inner Join {DbNames.EPYSL}..SampleBookingMaster b on b.BookingID = a.BookingID
                            WHERE A.BookingID in ({bookingId})
                        ),
						FBA AS
                        (
	                        SELECT *FROM FBA1
	                        UNION
	                        SELECT *FROM FBA2
                        ),
                        F AS
                        (
	                        SELECT FBA.BookingID,FBA.BOMMasterID,FBA.ItemGroupID,FBA.SubGroupID,FBA.Status,FBA.PreRevisionNo,FBA.RevisionNo,
	                        FBA.BookingNo,FBA.BookingDate,FBA.Remarks,FBA.StyleNo,FBA.ExportOrderID,FBA.StyleMasterID,
	                        FBA.BuyerID,FBA.BuyerTeamID,FBA.CompanyID,FBA.SupplierID,FBA.SeasonID,FBA.WithoutOB,
	                        CTO.ContactDisplayCode AS BuyerName, CCT.DisplayCode AS BuyerTeamName,CompanyName = C.ShortName,
	                        Supplier.ShortName [SupplierName],Season.SeasonName, ISNULL(Supplier.MappingCompanyID,0) TextileCompanyID,
	                        GmtQtyPcs = Sum(TotalPOQty)
	                        FROM FBA
	                        LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = FBA.BuyerID
	                        LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = FBA.BuyerTeamID
	                        LEFT JOIN {DbNames.EPYSL}..CompanyEntity C ON C.CompanyID = FBA.CompanyID
	                        LEFT Join {DbNames.EPYSL}..Contacts Supplier On Supplier.ContactID = FBA.SupplierID
	                        LEFT Join {DbNames.EPYSL}..ContactSeason Season On Season.SeasonID = FBA.SeasonID
	                        LEFT JOIN {DbNames.EPYSL}..OrderBankMaster OBM On OBM.StyleMasterID = FBA.StyleMasterID
	                        LEFT JOIN {DbNames.EPYSL}..OrderBankPO OBPO On OBPO.OrderBankMasterID = OBM.OrderBankMasterID AND OBPO.IsActive = 1
	                        GROUP BY FBA.BookingID,FBA.BOMMasterID,FBA.ItemGroupID,FBA.SubGroupID,FBA.Status,FBA.PreRevisionNo,FBA.RevisionNo,
	                        FBA.BookingNo,FBA.BookingDate,FBA.Remarks,FBA.StyleNo,FBA.ExportOrderID,FBA.StyleMasterID,
	                        FBA.BuyerID,FBA.BuyerTeamID,FBA.CompanyID,FBA.SupplierID,FBA.SeasonID,FBA.WithoutOB,
	                        CTO.ContactDisplayCode, CCT.DisplayCode,C.ShortName,
	                        Supplier.ShortName,Season.SeasonName, ISNULL(Supplier.MappingCompanyID,0)
                        )
						SELECT * FROM F;

                        ----- Sample Booking Consumption Child (Fabric)
                        ;With PartName As(
	                        Select SCYS.ConsumptionID,SCYS.BookingID, PartName = STRING_AGG(YSB.PartName,',')
	                        From FBookingAcknowledgeChildGarmentPart SCYS
	                        Inner Join FBookingAcknowledgeChild SBKC On SBKC.BookingID = SCYS.BookingID And SBKC.ConsumptionID = SCYS.ConsumptionID
	                        Inner Join {DbNames.EPYSL}..FabricUsedPart YSB On YSB.FUPartID = SCYS.FUPartID
	                        Where SBKC.BookingID in ({bookingId}) And SBKC.SubGroupID = 1
	                        GROUP BY SCYS.ConsumptionID,SCYS.BookingID

                        ), 
                        FBA AS
                        (
	                        SELECT FBA.BookingID,FBA.BookingNo, FBA.BuyerID, FBA.ExecutionCompanyID,
	                        --FBAC.BookingChildID, 
                            AcknowledgeID = FBA.FBAckID,
                            BookingChildID = ISNULL(FBAC.BookingChildID,0), 
                            ConsumptionID = ISNULL(FBAC.ConsumptionID,0), 
                            FBAC.ItemMasterID,FBAC.SubGroupID,FBAC.A1ValueID,FBAC.YarnBrandID,ReferenceSourceID=0,YarnSourceID=0,
	                        ReferenceNo='',ColorReferenceNo='',FBAC.ItemGroupID, FBA.RevisionNo,
	                        FBAC.LengthYds,FBAC.LengthInch,FBAC.FUPartID,FBAC.ConsumptionQty,FBAC.LabDipNo,FBAC.Price,FBAC.SuggestedPrice,
	                        RequiredQty = FBAC.BookingQty,FBAC.Remarks,FBA.SupplierID,FBAC.BookingUnitID, BookingUOM = BU.DisplayUnitDesc,
                            --PreviousBookingQty =ISNULL((Select BookingQty from {DbNames.EPYSL}..BookingChild_Bk where BookingID = FBAC.BookingID and ConsumptionID = FBAC.ConsumptionID and  RevisionNo= (Select Max(RevisionNo)from {DbNames.EPYSL}..BookingChild_Bk Where  BookingID = FBAC.BookingID)),0)
                            PreviousBookingQty =  IIF(FBA.WithoutOB = 0,
	                            ISNULL((Select BookingQty from {DbNames.EPYSL}..BookingChild_Bk where BookingID = FBAC.BookingID and ConsumptionID = FBAC.ConsumptionID AND ItemMasterId = FBAC.ItemMasterId and RevisionNo= (Select Max(RevisionNo)from {DbNames.EPYSL}..BookingChild_Bk Where BookingID = FBAC.BookingID)),0),
	                            IsNull((Select Sum(MM.RequiredQty) From {DbNames.EPYSL}..SampleBookingConsumptionChild_Bk MM Where BookingID = FBAC.BookingID And MM.ConsumptionID = FBAC.ConsumptionID AND MM.ItemMasterId = FBAC.ItemMasterId AND MM.RevisionNo = (Select Max(RevisionNo)from {DbNames.EPYSL}..SampleBookingConsumptionChild_Bk V Where V.BookingID = FBAC.BookingID)),0)
                            )
                            ,FBAC.LiabilitiesBookingQty, PN.PartName 
                            ,RevisionNoWhenDeleted = ISNULL(FBAC.RevisionNoWhenDeleted,-1)
	                        ,FBA.IsSample
	                        FROM FBookingAcknowledgeChild FBAC 
	                        INNER JOIN FBookingAcknowledge FBA ON FBA.FBAckID = FBAC.AcknowledgeID
	                        LEFT JOIN {DbNames.EPYSL}..Unit BU On BU.UnitID = FBAC.BookingUnitID
	                        left Join {DbNames.EPYSL}..BookingChild BC On BC.BookingID = FBA.BookingID AND BC.ItemMasterID = FBAC.ItemMasterID AND BC.ConsumptionID = FBAC.ConsumptionID
                            LEFT JOIN {DbNames.EPYSL}..SampleBookingConsumption B ON B.BookingID = FBA.BookingID AND B.ConsumptionID = FBAC.ConsumptionID AND B.SubGroupID = FBAC.SubGroupID
                            LEFT JOIN {DbNames.EPYSL}..SampleBookingConsumptionChild BCC ON BCC.ConsumptionID = B.ConsumptionID AND BCC.ItemMasterID = FBAC.ItemMasterID AND BCC.SubGroupID = FBAC.SubGroupID                            
                            LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster SBM ON SBM.BookingID = B.BookingID
	                        Left Join PartName PN On PN.BookingID = FBA.BookingID And PN.ConsumptionID = FBAC.ConsumptionID
	                        WHERE FBAC.BookingID in ({bookingId}) AND FBAC.SubGroupID = 1
                        ),
                        TS As(
	                        ---- Fabric
	                        Select ISF.BookingID,ISF.ItemMasterID,
	                        FinishFabricStockQty_KG  = SUM(Case When IM.SubGroupID = 1 Then ISNULL(ISF.RollQtyInKG,0) Else ISNULL(ISF.RollQtyInKGPcs,0) End),
                            FinishFabricStockQty_PCS = SUM(Case When IM.SubGroupID = 1 Then 0 Else ISNULL(ISF.RollQtyInKG,0) End)
	                        From EPYSLTEX..ItemFinishStockRoll ISF
	                        Inner Join {DbNames.EPYSL}..ItemMaster IM On IM.ItemMasterID = ISF.ItemMasterID
	                        Left Join {DbNames.EPYSL}..ItemSegmentValue ISV4 On ISV4.SegmentValueID = IM.Segment4ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
	                        --Inner Join EPYSLTEX..FBookingAcknowledgeChild FBAC on FBAC.BookingID = ISF.BookingID AND FBAC.ItemMasterID = ISF.ItemMasterID
	                        Inner Join EPYSLTEX..FBookingAcknowledgeChildDetails FBAC on FBAC.BookingID = ISF.BookingID AND FBAC.ItemMasterID = ISF.ItemMasterID AND FBAC.ColorID = ISF.CCID4
                            Where FBAC.BookingID in ({bookingId})  And FBAC.SubGroupID = 1 AND ISF.Issued = 0
	                        Group by ISF.BookingID,ISF.ItemMasterID					
                        ),
                        DC_Bulk AS
                        (
	                        SELECT FBA.BookingID, FBA.ConsumptionID,
	                        DCKgQty = Sum(Case When BC.SubGroupID = 1 Then (IsNull(PDC.RollQtyInKG,0)-IsNull(PDC.AckShortQtyInKG,0)) Else (IsNull(PDC.RollQtyInKGPcs,0)-IsNull(PDC.AckShortQtyInKGPcs,0)) End),
                            DCPcsQty = Sum(Case When BC.SubGroupID = 1 Then 0 Else (IsNull(PDC.RollQtyInKG,0)-IsNull(PDC.AckShortQtyInKG,0)) End)
	                        FROM FBA
	                        Left JOIN {DbNames.EPYSL}..BookingChild BC ON BC.BookingID = FBA.BookingID AND BC.ConsumptionID = FBA.ConsumptionID
	                        LEFT JOIN {DbNames.EPYSL}..BookingMaster BM ON BM.BookingId = BC.BookingID
	                        Left Join {DbNames.EPYSL}..ItemMaster IM On IM.ItemMasterID = BC.ItemMasterID
	                        Left Join {DbNames.EPYSL}..ItemSegmentValue ISV3 On ISV3.SegmentValueID = IM.Segment3ValueID
	                        Left Join {DbNames.EPYSL}..BookingChildDetails BCD On BCD.BookingID = FBA.BookingID and BCD.BookingChildID = BC.BookingChildID and BCD.ItemMasterID = BC.ItemMasterID
	                        Left Join PDChild DC On DC.BookingID = BC.BookingID 
							                              and DC.ItemMasterID = BC.ItemMasterID 
								  
								                          and DC.AOPItem = (SELECT Max(Case When ISNULL(TPM.ProcessName,'') = 'AOP' Then 1 Else 0 End)
													                        FROM {DbNames.EPYSL}..BookingChildProcess BCP
													                        Left Join {DbNames.EPYSL}..ProcessMaster TPM On TPM.ProcessID = BCP.ProcessID
													                        WHERE BCP.BookingChildID = BC.BookingChildID)
								  
								                          and IsNull(DC.CCID4,0) = Case When ISV3.SegmentValue = 'Semi Bleach' OR ISV3.SegmentValue = 'RFD' Then ISNULL(BCD.ColorID,0)
								                          Else 0 End
	                        Left Join PDMaster DM On DM.ExportOrderID = BM.ExportOrderID  and DM.PDID = DC.PDID 
	                        Left Join PDChildRoll PDC On PDC.PDID = DM.PDID and PDC.PDChildID = DC.PDChildID
	                        WHERE FBA.IsSample = 0
	                        GROUP BY FBA.BookingID, FBA.ConsumptionID
                        ),
                        DC_SMS AS
                        (
	                        SELECT FBA.BookingID, FBA.ConsumptionID,
	                        DCKgQty = Sum(Case When BC.SubGroupID = 1 Then (IsNull(PDC.RollQtyInKG,0)-IsNull(PDC.AckShortQtyInKG,0)) Else (IsNull(PDC.RollQtyInKGPcs,0)-IsNull(PDC.AckShortQtyInKGPcs,0)) End),
                            DCPcsQty = Sum(Case When BC.SubGroupID = 1 Then 0 Else (IsNull(PDC.RollQtyInKG,0)-IsNull(PDC.AckShortQtyInKG,0)) End)
	                        FROM FBA
	                        Left JOIN {DbNames.EPYSL}..SampleBookingConsumptionChild BC1 ON BC1.BookingID = FBA.BookingID AND BC1.ConsumptionID = FBA.ConsumptionID
	                        Left JOIN {DbNames.EPYSL}..SampleBookingConsumption BC ON BC.BookingID = FBA.BookingID AND BC.ConsumptionID = FBA.ConsumptionID
	                        LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster BM ON BM.BookingId = BC.BookingID
	                        Left Join {DbNames.EPYSL}..ItemMaster IM On IM.ItemMasterID = BC1.ItemMasterID
	                        Left Join {DbNames.EPYSL}..ItemSegmentValue ISV3 On ISV3.SegmentValueID = IM.Segment3ValueID
	                        Left Join {DbNames.EPYSL}..SampleBookingConsumptionColor SBCC On SBCC.BookingID = BC.BookingID and SBCC.ConsumptionID = BC.ConsumptionID 
	                        Left Join PDChild DC On DC.BookingID = BC.BookingID 
	                                            and DC.ItemMasterID = BC1.ItemMasterID 

						                        and DC.AOPItem = (SELECT Max(Case When ISNULL(TPM.ProcessName,'') = 'AOP' Then 1 Else 0 End) 
										                          FROM {DbNames.EPYSL}..SampleBookingConsumptionProcess BCP
										                          Left Join {DbNames.EPYSL}..ProcessMaster TPM On TPM.ProcessID = BCP.ProcessID
										                          WHERE BCP.ConsumptionID = BC1.ConsumptionID)

						                        and IsNull(DC.CCID4,0) = Case When ISV3.SegmentValue = 'Semi Bleach' OR ISV3.SegmentValue = 'RFD' Then ISNULL(SBCC.ColorID,0) 
						                        Else 0 End
	                        Left Join PDMaster DM On DM.ExportOrderID = BM.ExportOrderID  and DM.PDID = DC.PDID 
	                        Left Join PDChildRoll PDC On PDC.PDID = DM.PDID and PDC.PDChildID = DC.PDChildID
	                        WHERE FBA.IsSample = 1
	                        GROUP BY FBA.BookingID, FBA.ConsumptionID
                        ),
                        ALL_DC AS
                        (
	                        SELECT * FROM DC_Bulk
	                        UNION
	                        SELECT * FROM DC_SMS
                        ),
                        F AS 
                        (
	                        SELECT TotalFinishFabricStockQty = TS.FinishFabricStockQty_KG,SBM.PartName,SBM.PreviousBookingQty,SBM.BookingChildID, SBM.ReferenceSourceID,RSETV.ValueName ReferenceSourceName , SBM.ReferenceNo,SBM.ColorReferenceNo,SBM.YarnSourceID,SBM.ConsumptionID,SBM.BookingID,SBM.BookingNo,SBM.ItemGroupID,SBM.SubGroupID,
	                        ISV1.SegmentValue Construction,ISV2.SegmentValue Composition,ISV3.SegmentValue Color
	                        ,ISV4.SegmentValue GSM,ISV5.SegmentValue FabricWidth, ISV6.SegmentValue DyeingType,ISV7.SegmentValue KnittingType,SBM.LengthYds,SBM.LengthInch,SBM.FUPartID,SBM.ConsumptionQty,SBM.BookingUnitID
	                        ,SBM.A1ValueID,SBM.YarnBrandID,SBM.LabDipNo,SBM.Price,SBM.SuggestedPrice,SBM.RequiredQty BookingQty,SBM.ItemMasterID,SBM.BuyerID ContactID,
	                        SBM.ExecutionCompanyID,
	                        0 As TechnicalNameId,ISG.SubGroupName,'1' As ConceptTypeID,IM.Segment1ValueID ConstructionId, IM.Segment2ValueID CompositionId,
	                        IM.Segment3ValueID ColorId,IM.Segment7ValueID KnittingTypeId,IM.Segment4ValueID GSMId,
	                        ISV.SegmentValue YarnType, ETV.ValueName YarnProgram, SBM.Remarks Instruction, ISNULL(Supplier.MappingCompanyID,0) TextileCompanyID,
                            RefSourceNo = CASE WHEN ISNULL(RS.RefSourceNo,'') = '' THEN 'New Item' ELSE RS.RefSourceNo END,
	                        SBM.BookingUOM, SBM.LiabilitiesBookingQty, SBM.AcknowledgeID,
                            SBM.RevisionNoWhenDeleted,
	                        DeliveredQtyForLiability = CASE WHEN SBM.SubGroupID = 1 THEN ISNULL(DC.DCKgQty,0) ELSE ISNULL(DC.DCPcsQty,0) END

	                        FROM FBA SBM
                            LEFT JOIN {DbNames.EPYSL}..BookingChildReferenceSource RS ON RS.BookingID = SBM.BookingID AND RS.ConsumptionID = SBM.ConsumptionID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = SBM.SubGroupID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV On ISV.SegmentValueID = SBM.A1ValueID
	                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = SBM.YarnBrandID
	                        Left Join {DbNames.EPYSL}..EntityTypeValue RSETV ON RSETV.ValueID = SBM.ReferenceSourceID
	                        Left Join {DbNames.EPYSL}..EntityTypeValue YSETV ON YSETV.ValueID = SBM.YarnSourceID
	                        LEFT Join {DbNames.EPYSL}..Contacts Supplier On Supplier.ContactID = SBM.SupplierID

	                        INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = SBM.ItemMasterID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                            Left Join TS On TS.ItemMasterID = SBM.ItemMasterID And TS.BookingID = SBM.BookingID
	                        LEFT JOIN ALL_DC DC ON DC.BookingID = SBM.BookingID AND DC.ConsumptionID = SBm.ConsumptionID
	                        WHERE SBM.BookingID in ({bookingId})
                        )
                        SELECT * FROM F
                        ORDER BY F.Color, F.GSM, F.FabricWidth;

                        ----- Sample Booking Consumption Child (Collor & Cuff)
                        WITH FBA AS
                        (
	                        SELECT FBA.BookingID,FBA.BookingNo, FBA.BuyerID, FBAC.ExecutionCompanyID,
                            AcknowledgeID = FBA.FBAckID,
	                        BookingChildID = ISNULL(FBAC.BookingChildID,0),
	                        ConsumptionID = ISNULL(FBAC.ConsumptionID,0),
                            FBAC.ItemMasterID,FBAC.SubGroupID,FBAC.A1ValueID,FBAC.YarnBrandID,ReferenceSourceID=0,YarnSourceID=0,
	                        ReferenceNo='',ColorReferenceNo='',FBAC.ItemGroupID, FBA.RevisionNo,
	                        FBAC.LengthYds,FBAC.LengthInch,FBAC.FUPartID,FBAC.ConsumptionQty,FBAC.LabDipNo,FBAC.Price,FBAC.SuggestedPrice,
	                        RequiredQty = FBAC.BookingQty,FBAC.Remarks,FBA.SupplierID, FBAC.BookingUnitID, BookingUOM = BU.DisplayUnitDesc,
                            PreviousBookingQty =  IIF(FBA.WithoutOB = 0,
	                            ISNULL((Select BookingQty from {DbNames.EPYSL}..BookingChild_Bk where BookingID = FBAC.BookingID and ConsumptionID = FBAC.ConsumptionID AND ItemMasterId = FBAC.ItemMasterId and RevisionNo= (Select Max(RevisionNo)from {DbNames.EPYSL}..BookingChild_Bk Where BookingID = FBAC.BookingID)),0),
	                            IsNull((Select Sum(MM.RequiredQty) From {DbNames.EPYSL}..SampleBookingConsumptionChild_Bk MM Where BookingID = FBAC.BookingID And MM.ConsumptionID = FBAC.ConsumptionID AND MM.ItemMasterId = FBAC.ItemMasterId AND MM.RevisionNo = (Select Max(RevisionNo)from {DbNames.EPYSL}..SampleBookingConsumptionChild_Bk V Where V.BookingID = FBAC.BookingID)),0)
                            )
                            ,FBAC.LiabilitiesBookingQty
                            ,RevisionNoWhenDeleted = ISNULL(FBAC.RevisionNoWhenDeleted,-1)
	                        ,FBA.IsSample
	                        FROM FBookingAcknowledgeChild FBAC
	                        INNER JOIN FBookingAcknowledge FBA ON FBA.FBAckID = FBAC.AcknowledgeID
	                        LEFT JOIN {DbNames.EPYSL}..BookingChild BC ON BC.BookingID = FBA.BookingID AND BC.ItemMasterID = FBAC.ItemMasterID AND BC.ConsumptionID = FBAC.ConsumptionID
	                        LEFT JOIN {DbNames.EPYSL}..SampleBookingConsumption SBC ON SBC.BookingID = FBA.BookingID AND SBC.ConsumptionID = FBAC.ConsumptionID
                            LEFT JOIN {DbNames.EPYSL}..Unit BU On BU.UnitID = FBAC.BookingUnitID                       
                            WHERE FBAC.BookingID in ({bookingId}) AND FBAC.SubGroupID IN (11,12)
                        ),
                        TS As(
	                        ---- Collar, Cuff
	                        Select  ISF.BookingID,ISF.ItemMasterID,FBAC.BookingChildID,  
	                        FinishFabricStockQty_KG  = SUM(Case When IM.SubGroupID = 1 Then ISNULL(ISF.RollQtyInKG,0) Else ISNULL(ISF.RollQtyInKGPcs,0) End),
                            FinishFabricStockQty_PCS = SUM(Case When IM.SubGroupID = 1 Then 0 Else ISNULL(ISF.RollQtyInKG,0) End)
	                        From EPYSLTEX..ItemFinishStockRoll ISF
	                        Inner Join {DbNames.EPYSL}..ItemMaster IM On IM.ItemMasterID = ISF.ItemMasterID
	                        Inner Join EPYSLTEX..FBookingAcknowledgeChild FBAC on FBAC.BookingID = ISF.BookingID AND FBAC.ItemMasterID = ISF.ItemMasterID
	                        Left Join {DbNames.EPYSL}..ItemSegmentValue ISV4 On ISV4.SegmentValueID = IM.Segment4ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
	                        Where FBAC.BookingID in ({bookingId}) And FBAC.SubGroupID in(11,12) AND ISF.Issued = 0
	                        Group by ISF.BookingID,ISF.ItemMasterID,FBAC.BookingChildID
                        ),
                        DC_Bulk AS
                        (
	                        SELECT FBA.BookingID, FBA.ConsumptionID,
	                        DCKgQty = Sum(Case When BC.SubGroupID = 1 Then (IsNull(PDC.RollQtyInKG,0)-IsNull(PDC.AckShortQtyInKG,0)) Else (IsNull(PDC.RollQtyInKGPcs,0)-IsNull(PDC.AckShortQtyInKGPcs,0)) End),
                            DCPcsQty = Sum(IsNull(PDC.RollQtyInKG,0)-IsNull(PDC.AckShortQtyInKG,0))
	                        FROM FBA
	                        Left JOIN {DbNames.EPYSL}..BookingChild BC ON BC.BookingID = FBA.BookingID AND BC.ConsumptionID = FBA.ConsumptionID
	                        LEFT JOIN {DbNames.EPYSL}..BookingMaster BM ON BM.BookingId = BC.BookingID
	                        Left Join {DbNames.EPYSL}..ItemMaster IM On IM.ItemMasterID = BC.ItemMasterID
	                        Left Join {DbNames.EPYSL}..ItemSegmentValue ISV3 On ISV3.SegmentValueID = IM.Segment3ValueID
	                        Left Join {DbNames.EPYSL}..BookingChildDetails BCD On BCD.BookingID = FBA.BookingID and BCD.BookingChildID = BC.BookingChildID and BCD.ItemMasterID = BC.ItemMasterID
	                        Left Join PDChild DC On DC.BookingID = BC.BookingID 
							                              and DC.ItemMasterID = BC.ItemMasterID
								                          and IsNull(DC.CCID4,0) = Case When ISV3.SegmentValue = 'Semi Bleach' OR ISV3.SegmentValue = 'RFD' Then ISNULL(BCD.ColorID,0)
								                          Else 0 End
	                        Left Join PDMaster DM On DM.ExportOrderID = BM.ExportOrderID  and DM.PDID = DC.PDID 
	                        Left Join PDChildRoll PDC On PDC.PDID = DM.PDID and PDC.PDChildID = DC.PDChildID
	                        WHERE FBA.IsSample = 0
	                        GROUP BY FBA.BookingID, FBA.ConsumptionID
                        ),
                        DC_SMS AS
                        (
	                        SELECT FBA.BookingID, FBA.ConsumptionID,
	                        DCKgQty = Sum(Case When BC.SubGroupID = 1 Then (IsNull(PDC.RollQtyInKG,0)-IsNull(PDC.AckShortQtyInKG,0)) Else (IsNull(PDC.RollQtyInKGPcs,0)-IsNull(PDC.AckShortQtyInKGPcs,0)) End),
                            DCPcsQty = Sum(IsNull(PDC.RollQtyInKG,0)-IsNull(PDC.AckShortQtyInKG,0))
	                        FROM FBA
	                        Left JOIN {DbNames.EPYSL}..SampleBookingConsumptionChild BC1 ON BC1.BookingID = FBA.BookingID AND BC1.ConsumptionID = FBA.ConsumptionID
	                        Left JOIN {DbNames.EPYSL}..SampleBookingConsumption BC ON BC.BookingID = FBA.BookingID AND BC.ConsumptionID = FBA.ConsumptionID
	                        LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster BM ON BM.BookingId = BC.BookingID
	                        Left Join {DbNames.EPYSL}..ItemMaster IM On IM.ItemMasterID = BC1.ItemMasterID
	                        Left Join {DbNames.EPYSL}..ItemSegmentValue ISV3 On ISV3.SegmentValueID = IM.Segment3ValueID
	                        Left Join {DbNames.EPYSL}..SampleBookingConsumptionColor SBCC On SBCC.BookingID = BC.BookingID and SBCC.ConsumptionID = BC.ConsumptionID 
	                        Left Join PDChild DC On DC.BookingID = BC.BookingID 
	                                            and DC.ItemMasterID = BC1.ItemMasterID
						                        and IsNull(DC.CCID4,0) = Case When ISV3.SegmentValue = 'Semi Bleach' OR ISV3.SegmentValue = 'RFD' Then ISNULL(SBCC.ColorID,0) 
						                        Else 0 End
	                        Left Join PDMaster DM On DM.ExportOrderID = BM.ExportOrderID  and DM.PDID = DC.PDID 
	                        Left Join PDChildRoll PDC On PDC.PDID = DM.PDID and PDC.PDChildID = DC.PDChildID
	                        WHERE FBA.IsSample = 1
	                        GROUP BY FBA.BookingID, FBA.ConsumptionID
                        ),
                        ALL_DC AS
                        (
	                        SELECT * FROM DC_Bulk
	                        UNION
	                        SELECT * FROM DC_SMS
                        ),
                        F AS 
                        (
	                        SELECT TotalFinishFabricStockQty = TS.FinishFabricStockQty_PCS,SBM.PreviousBookingQty, SBM.BookingChildID, SBM.AcknowledgeID, SBM.ReferenceSourceID,RSETV.ValueName ReferenceSourceName , SBM.ReferenceNo,SBM.ColorReferenceNo,SBM.YarnSourceID,SBM.ConsumptionID,SBM.BookingID,SBM.BookingNo,SBM.ItemGroupID,SBM.SubGroupID,
	
                             Segment1ValueID = ISV1.SegmentValueID
	                        ,Segment2ValueID = ISV2.SegmentValueID
	                        ,Segment3ValueID = ISV3.SegmentValueID
	                        ,Segment4ValueID = ISV4.SegmentValueID
	                        ,Segment5ValueID = ISV5.SegmentValueID
	                        ,Segment6ValueID = ISV6.SegmentValueID

	                        ,ISV1.SegmentValue Segment1Desc
	                        ,ISV2.SegmentValue Segment2Desc
	                        ,ISV3.SegmentValue Segment3Desc
	                        ,ISV4.SegmentValue Segment4Desc
	                        ,ISV5.SegmentValue Segment5Desc
	                        ,ISV6.SegmentValue Segment6Desc
	
	                        ,SBM.LengthYds,SBM.LengthInch,SBM.FUPartID,SBM.ConsumptionQty,SBM.BookingUnitID
	                        ,SBM.A1ValueID,SBM.YarnBrandID,SBM.LabDipNo,SBM.Price,SBM.SuggestedPrice,SBM.RequiredQty BookingQty,SBM.ItemMasterID,SBM.BuyerID ContactID,
	                        SBM.ExecutionCompanyID,
	                        0 As TechnicalNameId,ISG.SubGroupName,'1' As ConceptTypeID,IM.Segment1ValueID ConstructionId, IM.Segment2ValueID CompositionId,
	                        IM.Segment3ValueID ColorId,IM.Segment7ValueID KnittingTypeId,IM.Segment4ValueID GSMId,
	                        ISV.SegmentValue YarnType, ETV.ValueName YarnProgram, ISV6.SegmentValue DyeingType, SBM.Remarks Instruction, ISNULL(Supplier.MappingCompanyID,0) TextileCompanyID
	                        ,RefSourceNo = CASE WHEN ISNULL(RS.RefSourceNo,'') = '' THEN 'New Item' ELSE RS.RefSourceNo END,
	                        SBM.BookingUOM, SBM.LiabilitiesBookingQty,
                            SBM.RevisionNoWhenDeleted,
	                        DeliveredQtyForLiability = CASE WHEN SBM.SubGroupID = 1 THEN ISNULL(DC.DCKgQty,0) ELSE ISNULL(DC.DCPcsQty,0) END

                            FROM FBA SBM
                            LEFT JOIN {DbNames.EPYSL}..BookingChildReferenceSource RS ON RS.BookingID = SBM.BookingID AND RS.ConsumptionID = SBM.ConsumptionID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = SBM.SubGroupID

	                        LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = SBM.ItemMasterID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID

	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV On ISV.SegmentValueID = SBM.A1ValueID
	                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = SBM.YarnBrandID
	                        Left Join {DbNames.EPYSL}..EntityTypeValue RSETV ON RSETV.ValueID = SBM.ReferenceSourceID
	                        Left Join {DbNames.EPYSL}..EntityTypeValue YSETV ON YSETV.ValueID = SBM.YarnSourceID
	                        LEFT Join {DbNames.EPYSL}..Contacts Supplier On Supplier.ContactID = SBM.SupplierID
                            Left Join TS On TS.ItemMasterID = SBM.ItemMasterID And TS.BookingID = SBM.BookingID
	                        LEFT JOIN ALL_DC DC ON DC.BookingID = SBM.BookingID AND DC.ConsumptionID = SBm.ConsumptionID
	                        WHERE SBM.BookingID in ({bookingId})
                        )
                        SELECT * FROM F
                        ORDER BY F.Segment5Desc, F.Segment1Desc,F.Segment2Desc;

                       ----Sample Booking Consumption AddProcess
                        SELECT  BookingID ,ProcessID,ConsumptionID
                        FROM {DbNames.EPYSL}..SampleBookingConsumptionAddProcess where BookingID in ({bookingId});

                        ----- Sample Booking Consumption Child Details
                        SELECT ConsumptionID,BookingID,ItemGroupID,SubGroupID,ItemMasterID,RequiredQty BookingQty ,ConsumptionQty,RequiredUnitID BookingUnitID, 0 As TechnicalNameId
                        FROM {DbNames.EPYSL}..SampleBookingConsumptionChild where BookingID in ({bookingId})

                        ----- Sample Booking Consumption Garment Part
                        SELECT  BookingID ,FUPartID,ConsumptionID
                         FROM {DbNames.EPYSL}..SampleBookingConsumptionGarmentPart where BookingID in ({bookingId})

                        ---Sample Booking Consumption Process
                        SELECT BookingID,ProcessID,ConsumptionID
                        FROM {DbNames.EPYSL}..SampleBookingConsumptionProcess where BookingID in ({bookingId})

                        ------ Sample Booking Consumption Text
                        SELECT BookingID,UsesIn,AdditionalProcess,ApplicableProcess,YarnSubProgram,GarmentsColor GmtColor,ConsumptionID
                        FROM {DbNames.EPYSL}..SampleBookingConsumptionText where BookingID in ({bookingId})

                        ------ Sample Booking Child Distribution 
                        SELECT *
                        FROM {DbNames.EPYSL}..SampleBookingChildDistribution where BookingID in ({bookingId})

                        ----- SampleBooking ConsumptionYarn Sub Brand
                        SELECT SBKC.ReferenceSourceID,RSETV.ValueName ReferenceSourceName , SBKC.ReferenceNo,SBKC.ColorReferenceNo,SBKC.YarnSourceID,SCYS.BookingID, SCYS.YarnSubBrandID, SCYS.ConsumptionID, YSB.YarnSubBrandName
				        FROM {DbNames.EPYSL}..SampleBookingConsumptionYarnSubBrand SCYS
				        Inner Join {DbNames.EPYSL}..SampleBookingConsumption SBKC On SCYS.BookingID = SBKC.BookingID And SCYS.ConsumptionID = SBKC.ConsumptionID
                        Inner Join {DbNames.EPYSL}..ItemGroup IG On IG.ItemGroupID = SBKC.ItemGroupID
                        Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = SBKC.SubGroupID
                        Left Join {DbNames.EPYSL}..EntityTypeValue RSETV ON RSETV.ValueID = SBKC.ReferenceSourceID
                        Left Join  {DbNames.EPYSL}..EntityTypeValue YSETV ON YSETV.ValueID = SBKC.YarnSourceID
				        Inner Join (
					        Select ETV.ValueID YarnSubBrandID, ETV.ValueName YarnSubBrandName
					        From {DbNames.EPYSL}..EntityTypeValue ETV
					        Inner Join {DbNames.EPYSL}..EntityType ET On ET.EntityTypeID = ETV.EntityTypeID
				        ) YSB On YSB.YarnSubBrandID = SCYS.YarnSubBrandID
				        Where SCYS.BookingID in ({bookingId});

                        ----- SampleBookingChildImage
                        SELECT BookingID,ImagePath
                        FROM {DbNames.EPYSL}..SampleBookingChildImage where BookingID in ({bookingId})

                        --Free Concept
                        Select SBC.ReferenceSourceID,RSETV.ValueName ReferenceSourceName , SBC.ReferenceNo,SBC.ColorReferenceNo,SBC.YarnSourceID,FB.BookingID, FB.BookingNo, FB.BookingDate ConceptDate, SBC.RequiredQty Qty,
                        CC.ItemMasterID, IM.Segment1ValueID ConstructionId, IM.Segment2ValueID CompositionId, IM.Segment3ValueID ColorId, Color.SegmentValue ColorName, IM.Segment4ValueID GSMId,
                        IM.Segment7ValueID KnittingTypeId, IM.SubGroupID,FB.ExecutionCompanyID CompanyID, ISNULL(Supplier.MappingCompanyID,0) TextileCompanyID
                        FROM {DbNames.EPYSL}..SampleBookingConsumption SBC
                        INNER JOIN {DbNames.EPYSL}..SampleBookingConsumptionChild CC ON CC.ConsumptionID = SBC.ConsumptionID
                        INNER JOIN {DbNames.EPYSL}..SampleBookingMaster FB ON FB.BookingID = SBC.BookingID
                        INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = CC.ItemMasterID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = IM.Segment1ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = IM.Segment2ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = IM.Segment3ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = IM.Segment4ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Width ON Width.SegmentValueID = IM.Segment5ValueID
                        --LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue DT ON DT.SegmentValueID = IM.Segment6ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue KT ON KT.SegmentValueID = IM.Segment7ValueID
                        --LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID = FB.SubGroupID
                        Left Join  {DbNames.EPYSL}..EntityTypeValue RSETV ON RSETV.ValueID = SBC.ReferenceSourceID
                        Left Join  {DbNames.EPYSL}..EntityTypeValue YSETV ON YSETV.ValueID = SBC.YarnSourceID
                        LEFT Join {DbNames.EPYSL}..Contacts Supplier On Supplier.ContactID = FB.SupplierID
                        where FB.BookingID in ({bookingId});

                        --Technical Name
                        SELECT Cast(T.TechnicalNameId As varchar) id, T.TechnicalName [text], ISNULL(ST.[Days], 0) [desc], Cast(SC.SubClassID as varchar) additionalValue
                        FROM FabricTechnicalName T
                        LEFT JOIN FabricTechnicalNameKMachineSubClass SC ON SC.TechnicalNameID = T.TechnicalNameId
                        LEFT JOIN KnittingMachineStructureType_HK ST ON ST.StructureTypeID = SC.StructureTypeID
                        Group By T.TechnicalNameId, T.TechnicalName, ST.Days, SC.SubClassID;

                         --YarnSource data load
				        Select Cast(a.ValueID as varchar) id, a.ValueName [text]
				        From {DbNames.EPYSL}..EntityTypeValue a
				        Inner Join {DbNames.EPYSL}..EntityType b on b.EntityTypeID = a.EntityTypeID
				        Where b.EntityTypeName = 'Yarn Source'

                        --M/c type
                        ;SELECT CAST(a.MachineSubClassID AS varchar) [id], b.SubClassName [text], b.TypeID [desc], c.TypeName additionalValue
                        FROM KnittingMachine a
                        INNER JOIN KnittingMachineSubClass b ON b.SubClassID = a.MachineSubClassID
                        Inner Join KnittingMachineType c On c.TypeID = b.TypeID
                        --Where c.TypeName != 'Flat Bed'
                        GROUP BY a.MachineSubClassID, b.SubClassName, b.TypeID, c.TypeName;

                        --CriteriaNames
                         ;SELECT CriteriaName,CriteriaSeqNo, (CASE WHEN CriteriaName  IN('Batch Preparation','Quality Check') THEN '1'ELSE'0'END) AS TotalTime
                        FROM BDSCriteria_HK --WHERE CriteriaName NOT IN('Batch Preparation','Testing')
                        GROUP BY CriteriaSeqNo,CriteriaName order by CriteriaSeqNo,CriteriaName;

                        --FBAChildPlannings
                        ;SELECT * FROM BDSCriteria_HK order by CriteriaSeqNo, OperationSeqNo, CriteriaName;
                        
                        --Liability Process
				        Select LChildID = 0,BookingChildID = 0,AcknowledgeID = 0,BookingID = 0,UnitID = 0, Cast(a.ValueID as varchar) LiabilitiesProcessID, a.ValueName LiabilitiesName,LiabilityQty=0
				        From {DbNames.EPYSL}..EntityTypeValue a
				        Inner Join {DbNames.EPYSL}..EntityType b on b.EntityTypeID = a.EntityTypeID
				        Where b.EntityTypeName = 'Process Liability';

                        --Liability Process data load
				        WITH LD AS
						(
							SELECT D.*
							FROM FBookingAcknowledgementLiabilityDistribution D
							WHERE D.BookingID IN ({bookingId})
						)
						SELECT LChildID = ISNULL(LD.LChildID,0),  BookingChildID = ISNULL(LD.BookingChildID,0), 
						AcknowledgeID = ISNULL(LD.AcknowledgeID,0), BookingID = ISNULL(LD.BookingID,0), UnitID = ISNULL(LD.UnitID,0), LiabilitiesProcessID = a.ValueID,
						LiabilityQty = ISNULL(LD.LiabilityQty,0), ConsumedQty = ISNULL(LD.ConsumedQty,0), SuggestedLiabilityQty = ISNULL(LD.SuggestedLiabilityQty,0), Rate = ISNULL(LD.Rate,0)
						FROM {DbNames.EPYSL}..EntityType b
						INNER JOIN {DbNames.EPYSL}..EntityTypeValue a ON a.EntityTypeID = b.EntityTypeID
						LEFT JOIN LD ON LD.LiabilitiesProcessID = a.ValueID
						WHERE b.EntityTypeName = 'Process Liability';
                    

                    ;With YBM As
                    (
                        Select * From YarnBookingMaster Where BookingID in ({bookingId})
                    ),
                    YBM_New As
                    (
                        Select * From YarnBookingMaster_New Where BookingID in ({bookingId})
                    ),
                    YBCI As (
                        Select ChildID = Row_Number() Over (Order By (Select 0)), YBCI.YItemMasterID As ItemMasterID,YBC.ConsumptionID, YBCI.UnitID, U.DisplayUnitDesc, YBCI.Blending,
                        (Case When Blending = 1 then 'Blend' else 'Non-Blend' End)BlendingName, YBCI.YarnCategory,  BookingQty=Sum(YBCI.BookingQty),
                        ShadeCode= IsNull(YBCI.ShadeCode,''), IsNull(Y.ShadeCode,'') as ShadeName,
                        YBCI.Remarks, YBCI.Specification, YBCI.YD, YDItem='', YBM.BookingID,
                        ISV1.SegmentValue As _segment1ValueDesc, ISV2.SegmentValue As _segment2ValueDesc, ISV3.SegmentValue As _segment3ValueDesc,
                        ISV4.SegmentValue As _segment4ValueDesc, ISV5.SegmentValue As _segment5ValueDesc, ISV6.SegmentValue As _segment6ValueDesc,
                        ISV7.SegmentValue As _segment7ValueDesc, ISV8.SegmentValue As _segment8ValueDesc, YBM.SubGroupID, ISG.SubGroupName,
                        LiabilityQty = IsNull(FBAY.LiabilityQty,0),ReqQty = YBCI.NetYarnReqQty,
                        YBCI.YBChildItemID, Rate = ISNULL(YIP.SourcingRate,0)
                        From YBM_New YBM
                        Inner Join YarnBookingChild_New YBC On YBM.YBookingID = YBC.YBookingID
                        Inner Join YarnBookingChildItem_New YBCI On YBCI.YBChildID = YBC.YBChildID
		                LEFT JOIN YarnItemPrice YIP ON YIP.YBChildItemID = YBCI.YBChildItemID AND ISNULL(YIP.IsTextileERP,0) = 1
                        Left Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = YBM.SubGroupID
                        INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YBCI.YItemMasterID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV8 ON ISV8.SegmentValueID = IM.Segment8ValueID
                        LEFT JOIN {DbNames.EPYSL}..Unit U ON U.UnitID = YBCI.UnitID
                        LEFT JOIN YarnShadeBook Y ON Y.ShadeCode = YBCI.ShadeCode
                        LEFT Join YDBookingMaster YDBM ON YDBM.YBookingID = YBM.YBookingID And YDBM.YBookingID = YBCI.YBookingID
                        LEFT Join YDProductionMaster YPM ON YPM.YDBookingMasterID = YDBM.YDBookingMasterID
						Left Join FBookingAcknowledgementYarnLiability FBAY On FBAY.BookingID = YBM.BookingID And FBAY.ConsumptionID = YBC.ConsumptionID And FBAY.ItemMasterID = YBCI.YItemMasterID
						Group By YBCI.YItemMasterID,YBC.ConsumptionID, YBCI.UnitID, U.DisplayUnitDesc, YBCI.Blending,
                        (Case When Blending = 1 then 'Blend' else 'Non-Blend' End), YBCI.YarnCategory, 
                        IsNull(YBCI.ShadeCode,''), Y.ShadeCode,
                        YBCI.Remarks, YBCI.Specification, YBCI.YD, YBM.BookingID,
                        ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue,
                        ISV4.SegmentValue, ISV5.SegmentValue, ISV6.SegmentValue,
                        ISV7.SegmentValue, ISV8.SegmentValue, YBM.SubGroupID, ISG.SubGroupName,
                        IsNull(FBAY.LiabilityQty,0),YBCI.RequiredQty,YBCI.YBChildItemID,ISNULL(YIP.SourcingRate,0),
                        YBCI.NetYarnReqQty
                    ),
                    Issued AS
                    (
	                    SELECT YACI.AllocationChildItemID, TotalIssueQty = SUM(YSC.Qty)
	                    FROM YBCI
	                    INNER JOIN YarnAllocationChild YAC ON YAC.YBChildItemID = YBCI.YBChildItemID
	                    INNER JOIN YarnAllocationChildItem YACI ON YACI.AllocationChildID = YAC.AllocationChildID
	                    INNER JOIN YarnStockChild YSC ON YSC.StockFromTableId = 6 AND YSC.StockFromPKId = YACI.AllocationChildItemID AND YSC.TransectionTypeId = 2 AND YSC.IsInactive = 0
	                    WHERE YACI.Acknowledge = 1
	                    GROUP BY YACI.AllocationChildItemID
                    ),
                    Allcated AS
                    (
	                    SELECT YBCI.YBChildItemID, YAC.AllocationChildID, TotalIssueQty = ISNULL(I.TotalIssueQty,0), TotalAllocationQty = ISNULL(SUM(YACI.AdvanceAllocationQty + YACI.SampleAllocationQty + YACI.LiabilitiesAllocationQty + YACI.LeftoverAllocationQty),0) - ISNULL(I.TotalIssueQty,0)
	                    FROM YBCI
	                    INNER JOIN YarnAllocationChild YAC ON YAC.YBChildItemID = YBCI.YBChildItemID
	                    INNER JOIN YarnAllocationChildItem YACI ON YACI.AllocationChildID = YAC.AllocationChildID
	                    LEFT JOIN Issued I ON I.AllocationChildItemID = YACI.AllocationChildItemID
	                    INNER JOIN YarnStockMaster YSM ON YSM.YarnStockSetId = YACI.YarnStockSetId
	                    WHERE YACI.Acknowledge = 1
	                    GROUP BY YACI.AllocationChildItemID, YBCI.YBChildItemID, YAC.AllocationChildID,ISNULL(I.TotalIssueQty,0)
                    ),
                    FinalList AS
                    (
	                    SELECT YBCI.*, A.AllocationChildID, AllocatedQty = A.TotalAllocationQty, A.TotalIssueQty
	                    FROM YBCI
	                    LEFT JOIN Allcated A ON A.YBChildItemID = YBCI.YBChildItemID
                    )
                    Select * From FinalList

                    --FBookingAcknowledgementLiabilityDistribution
                    SELECT D.*, LiabilitiesName = ETV.ValueName
                    FROM FBookingAcknowledgementLiabilityDistribution D
                    LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = D.LiabilitiesProcessID
                    WHERE D.BookingID in ({bookingId})

                    --FBookingAcknowledgementYarnLiability
                    SELECT L.*,
                    ISN1.SegmentName _segment1ValueDesc,
                    ISN2.SegmentName _segment2ValueDesc,
                    ISN3.SegmentName _segment3ValueDesc,
                    ISN4.SegmentName _segment4ValueDesc,
                    ISN5.SegmentName _segment5ValueDesc,
                    ISN6.SegmentName _segment6ValueDesc

                    FROM FBookingAcknowledgementYarnLiability L
                    INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = L.ItemMasterID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentName ISN1 ON ISN1.SegmentNameID = ISV1.SegmentNameID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentName ISN2 ON ISN2.SegmentNameID = ISV2.SegmentNameID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentName ISN3 ON ISN3.SegmentNameID = ISV3.SegmentNameID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentName ISN4 ON ISN4.SegmentNameID = ISV4.SegmentNameID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentName ISN5 ON ISN5.SegmentNameID = ISV5.SegmentNameID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentName ISN6 ON ISN6.SegmentNameID = ISV6.SegmentNameID
                    WHERE L.BookingID in ({bookingId})

                    --Process Liability
                    SELECT LiabilitiesProcessID = ETV.ValueID, LiabilitiesName = ETV.ValueName
                    FROM {DbNames.EPYSL}..EntityTypeValue ETV
                    INNER JOIN {DbNames.EPYSL}..EntityType ET ON ET.EntityTypeID = ETV.EntityTypeID
                    WHERE ET.EntityTypeName = 'Process Liability';";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                List<FabricBookingAcknowledge> fbaList = records.Read<FabricBookingAcknowledge>().ToList();
                FBookingAcknowledge data = records.Read<FBookingAcknowledge>().FirstOrDefault();
                data.IsSample = fbaList.Count() > 0 ? fbaList.First().IsSample : false;
                data.RevisionNo = fbaList.First().RevisionNo;
                string unAcknowledgeReason = fbaList.Count() > 0 ? fbaList.FirstOrDefault().UnAcknowledgeReason : "";
                data.FBookingAcknowledgeList = records.Read<FBookingAcknowledge>().ToList();
                Guard.Against.NullObject(data);

                List<FBookingAcknowledgeChild> bookingChilds = records.Read<FBookingAcknowledgeChild>().ToList();
                List<FBookingAcknowledgeChild> bookingChildsCollarCuff = records.Read<FBookingAcknowledgeChild>().ToList();

                if (data.RevisionNo > 0)
                {
                    //int previsousRevisionNo = data.RevisionNo - 1;
                    bookingChilds = bookingChilds.Where(x => x.BookingQty > 0 || (x.BookingQty == 0 && x.RevisionNoWhenDeleted == data.RevisionNo)).ToList();
                    bookingChildsCollarCuff = bookingChildsCollarCuff.Where(x => x.BookingQty > 0 || (x.BookingQty == 0 && x.RevisionNoWhenDeleted == data.RevisionNo)).ToList();
                }

                data.FabricBookingAcknowledgeList = fbaList;
                data.FBookingAcknowledgeChildAddProcess = records.Read<FBookingAcknowledgeChildAddProcess>().ToList();
                data.FBookingChildDetails = records.Read<FBookingAcknowledgeChildDetails>().ToList();
                data.FBookingAcknowledgeChildGarmentPart = records.Read<FBookingAcknowledgeChildGarmentPart>().ToList();
                data.FBookingAcknowledgeChildProcess = records.Read<FBookingAcknowledgeChildProcess>().ToList();
                data.FBookingAcknowledgeChildText = records.Read<FBookingAcknowledgeChildText>().ToList();
                data.FBookingAcknowledgeChildDistribution = records.Read<FBookingAcknowledgeChildDistribution>().ToList();
                data.FBookingAcknowledgeChildYarnSubBrand = records.Read<FBookingAcknowledgeChildYarnSubBrand>().ToList();
                data.FBookingAcknowledgeImage = records.Read<FBookingAcknowledgeImage>().ToList();
                data.FreeConcepts = records.Read<FreeConceptMaster>().ToList();
                data.TechnicalNameList = await records.ReadAsync<Select2OptionModel>();
                data.YarnSourceNameList = await records.ReadAsync<Select2OptionModel>();

                List<Select2OptionModel> mcTypeList = records.Read<Select2OptionModel>().ToList();
                data.MCTypeForFabricList = mcTypeList.Where(x => x.additionalValue != "Flat Bed");
                data.MCTypeForOtherList = mcTypeList.Where(x => x.additionalValue == "Flat Bed");

                List<FBookingAcknowledgeChild> criteriaNames = records.Read<FBookingAcknowledgeChild>().ToList();
                List<FBAChildPlanning> fbaChildPlannings = records.Read<FBAChildPlanning>().ToList();

                List<FBookingAcknowledgementLiabilityDistribution> LiabilityDistributionList = records.Read<FBookingAcknowledgementLiabilityDistribution>().ToList();
                List<FBookingAcknowledgementLiabilityDistribution> FBookingAckLiabilityDistributionList = records.Read<FBookingAcknowledgementLiabilityDistribution>().ToList();
                data.FBookingAcknowledgementYarnLiabilityList = records.Read<FBookingAcknowledgementYarnLiability>().ToList();

                List<FBookingAcknowledgementLiabilityDistribution> distributionList = records.Read<FBookingAcknowledgementLiabilityDistribution>().ToList();
                List<FBookingAcknowledgementYarnLiability> yarnLiabilityList = records.Read<FBookingAcknowledgementYarnLiability>().ToList();

                List<FBookingAcknowledgementLiabilityDistribution> defaultProcessLiabilities = records.Read<FBookingAcknowledgementLiabilityDistribution>().ToList();

                data.FBookingChild = bookingChilds.Where(x => x.SubGroupID == 1).ToList();
                data.BookingQty = Convert.ToInt32(data.FBookingChild.Sum(x => x.BookingQty));

                data.FBookingChildCollor = bookingChildsCollarCuff.Where(x => x.SubGroupID == 11).ToList();
                data.FBookingChildCuff = bookingChildsCollarCuff.Where(x => x.SubGroupID == 12).ToList();

                data.FBookingChild.ForEach(x =>
                {
                    x.ChildAckLiabilityDetails = new List<FBookingAcknowledgementLiabilityDistribution>();
                    defaultProcessLiabilities.ForEach(d =>
                    {
                        if (x.BookingChildID > 0)
                        {
                            var dObj = distributionList.Find(y => y.BookingChildID == x.BookingChildID && y.BookingID == x.BookingID && y.LiabilitiesProcessID == d.LiabilitiesProcessID);
                            if (dObj.IsNotNull()) x.ChildAckLiabilityDetails.Add(dObj);
                            else x.ChildAckLiabilityDetails.Add(d);
                        }
                        else if (x.ConsumptionID > 0)
                        {
                            var dObj = distributionList.Find(y => y.ConsumptionID == x.ConsumptionID && y.BookingID == x.BookingID && y.LiabilitiesProcessID == d.LiabilitiesProcessID);
                            if (dObj.IsNotNull()) x.ChildAckLiabilityDetails.Add(dObj);
                            else x.ChildAckLiabilityDetails.Add(d);
                        }
                    });

                    if (x.BookingChildID > 0)
                    {
                        x.ChildAckYarnLiabilityDetails = yarnLiabilityList.Where(y => y.BookingChildID == x.BookingChildID && y.BookingID == x.BookingID).ToList();
                        if (x.ChildAckYarnLiabilityDetails.Count() == 0) x.ChildAckYarnLiabilityDetails = data.FBookingAcknowledgementYarnLiabilityList.Where(y => y.ConsumptionID == x.ConsumptionID && y.BookingID == x.BookingID).ToList();
                    }
                    else if (x.ConsumptionID > 0)
                    {
                        x.ChildAckYarnLiabilityDetails = yarnLiabilityList.Where(y => y.ConsumptionID == x.ConsumptionID && y.BookingID == x.BookingID).ToList();
                        if (x.ChildAckYarnLiabilityDetails.Count() == 0) x.ChildAckYarnLiabilityDetails = data.FBookingAcknowledgementYarnLiabilityList.Where(y => y.ConsumptionID == x.ConsumptionID && y.BookingID == x.BookingID).ToList();
                    }

                });
                data.FBookingChildCollor.ForEach(x =>
                {
                    x.ChildAckLiabilityDetails = new List<FBookingAcknowledgementLiabilityDistribution>();
                    defaultProcessLiabilities.ForEach(d =>
                    {
                        if (x.BookingChildID > 0)
                        {
                            var dObj = distributionList.Find(y => y.BookingChildID == x.BookingChildID && y.BookingID == x.BookingID && y.LiabilitiesProcessID == d.LiabilitiesProcessID);
                            if (dObj.IsNotNull()) x.ChildAckLiabilityDetails.Add(dObj);
                            else x.ChildAckLiabilityDetails.Add(d);
                        }
                        else if (x.ConsumptionID > 0)
                        {
                            var dObj = distributionList.Find(y => y.ConsumptionID == x.ConsumptionID && y.BookingID == x.BookingID && y.LiabilitiesProcessID == d.LiabilitiesProcessID);
                            if (dObj.IsNotNull()) x.ChildAckLiabilityDetails.Add(dObj);
                            else x.ChildAckLiabilityDetails.Add(d);
                        }
                    });
                    if (x.BookingChildID > 0)
                    {
                        x.ChildAckYarnLiabilityDetails = yarnLiabilityList.Where(y => y.BookingChildID == x.BookingChildID && y.BookingID == x.BookingID).ToList();
                        if (x.ChildAckYarnLiabilityDetails.Count() == 0) x.ChildAckYarnLiabilityDetails = data.FBookingAcknowledgementYarnLiabilityList.Where(y => y.ConsumptionID == x.ConsumptionID && y.BookingID == x.BookingID).ToList();
                    }
                    else if (x.ConsumptionID > 0)
                    {
                        x.ChildAckYarnLiabilityDetails = yarnLiabilityList.Where(y => y.ConsumptionID == x.ConsumptionID && y.BookingID == x.BookingID).ToList();
                        if (x.ChildAckYarnLiabilityDetails.Count() == 0) x.ChildAckYarnLiabilityDetails = data.FBookingAcknowledgementYarnLiabilityList.Where(y => y.ConsumptionID == x.ConsumptionID && y.BookingID == x.BookingID).ToList();
                    }
                });
                data.FBookingChildCuff.ForEach(x =>
                {
                    x.ChildAckLiabilityDetails = new List<FBookingAcknowledgementLiabilityDistribution>();
                    defaultProcessLiabilities.ForEach(d =>
                    {
                        if (x.BookingChildID > 0)
                        {
                            var dObj = distributionList.Find(y => y.BookingChildID == x.BookingChildID && y.BookingID == x.BookingID && y.LiabilitiesProcessID == d.LiabilitiesProcessID);
                            if (dObj.IsNotNull()) x.ChildAckLiabilityDetails.Add(dObj);
                            else x.ChildAckLiabilityDetails.Add(d);
                        }
                        else if (x.ConsumptionID > 0)
                        {
                            var dObj = distributionList.Find(y => y.ConsumptionID == x.ConsumptionID && y.BookingID == x.BookingID && y.LiabilitiesProcessID == d.LiabilitiesProcessID);
                            if (dObj.IsNotNull()) x.ChildAckLiabilityDetails.Add(dObj);
                            else x.ChildAckLiabilityDetails.Add(d);
                        }
                    });

                    if (x.BookingChildID > 0)
                    {
                        x.ChildAckYarnLiabilityDetails = yarnLiabilityList.Where(y => y.BookingChildID == x.BookingChildID && y.BookingID == x.BookingID).ToList();
                        if (x.ChildAckYarnLiabilityDetails.Count() == 0) x.ChildAckYarnLiabilityDetails = data.FBookingAcknowledgementYarnLiabilityList.Where(y => y.ConsumptionID == x.ConsumptionID && y.BookingID == x.BookingID).ToList();
                    }
                    else if (x.ConsumptionID > 0)
                    {
                        x.ChildAckYarnLiabilityDetails = yarnLiabilityList.Where(y => y.ConsumptionID == x.ConsumptionID && y.BookingID == x.BookingID).ToList();
                        if (x.ChildAckYarnLiabilityDetails.Count() == 0) x.ChildAckYarnLiabilityDetails = data.FBookingAcknowledgementYarnLiabilityList.Where(y => y.ConsumptionID == x.ConsumptionID && y.BookingID == x.BookingID).ToList();
                    }
                });

                data.HasFabric = data.FBookingChild.Count() > 0 ? true : false;
                data.HasCollar = data.FBookingChildCollor.Count() > 0 ? true : false;
                data.HasCuff = data.FBookingChildCuff.Count() > 0 ? true : false;
                data.UnAcknowledgeReason = unAcknowledgeReason;
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
        public async Task<FBookingAcknowledge> GetSavedBulkFabricRevisionAsync(string bookingId)
        {
            string parentQuery = $@"
                        WITH
                        SBM AS
                        (
	                        SELECT SBM.BookingID, SBM.RevisionNo
	                        FROM {DbNames.EPYSL}..SampleBookingMaster SBM
	                        WHERE SBM.BookingID IN ({bookingId})
                        ),
                        BM AS
                        (
	                        SELECT BM.BookingID, RevisionNo = MAX(BM.RevisionNo)
	                        FROM {DbNames.EPYSL}..BookingMaster BM
	                        WHERE BM.BookingID IN ({bookingId})
	                        GROUP BY BM.BookingID
                        )
                        Select FBA.AcknowledgeID, FBA.BookingID, FBA.BOMMasterID, FBA.ItemGroupID, FBA.SubGroupID, FBA.Status, 
                        FBA.AcknowledgeDate, FBA.AddedBy, FBA.DateAdded, FBA.UpdatedBy, FBA.DateUpdated, FBA.WithoutOB,
                        FBA.UnAcknowledge, FBA.UnAcknowledgeDate, FBA.UnAcknowledge, FBA.RevisionNo, 
                        PreProcessRevNo = CASE WHEN ISNULL(BM.BookingID,0) = 0 THEN ISNULL(SBM.RevisionNo,0) ELSE ISNULL(BM.RevisionNo,0) END,
                        IsSample = CASE WHEN ISNULL(SBM.BookingID, 0) > 0 THEN 1 ELSE 0 END
                        From FabricBookingAcknowledge FBA
                        LEFT JOIN SBM ON SBM.BookingID = FBA.BookingID
                        LEFT JOIN BM ON BM.BookingID = FBA.BookingID
                        WHERE FBA.BookingID in ({bookingId});";


            var query = $@"
                        {parentQuery};

                        ;WITH FBA1 AS
                        (
                            SELECT a.BookingID,a.BOMMasterID,a.ItemGroupID,a.SubGroupID,a.Status,PreRevisionNo=a.PreProcessRevNo,RevisionNo=a.RevisionNo,
	                        b.BookingNo,b.BookingDate,b.Remarks,SM.StyleNo,b.ExportOrderID,SLNo='',SM.StyleMasterID,
	                        b.BuyerID,b.BuyerTeamID,b.CompanyID,b.SupplierID,SM.SeasonID,a.WithoutOB,BookingQty= SUM(BC.BookingQty)
                            FROM FabricBookingAcknowledge A
							INNER JOIN {DbNames.EPYSL}..BookingMaster b on b.BookingID = a.BookingID
                            Inner Join {DbNames.EPYSL}..BookingChild BC On BC.BookingID = A.BookingID
							LEFT JOIN {DbNames.EPYSL}..ExportOrderMaster EO ON EO.ExportOrderID = B.ExportOrderID
							LEFT JOIN {DbNames.EPYSL}..StyleMaster SM ON SM.StyleMasterID = EO.StyleMasterID
                            WHERE A.BookingID in ({bookingId})
                            Group BY a.BookingID,a.BOMMasterID,a.ItemGroupID,a.SubGroupID,a.Status,a.PreProcessRevNo,a.RevisionNo,
	                        b.BookingNo,b.BookingDate,b.Remarks,SM.StyleNo,b.ExportOrderID,SM.StyleMasterID,
	                        b.BuyerID,b.BuyerTeamID,b.CompanyID,b.SupplierID,SM.SeasonID,a.WithoutOB
                        ),
						FBA2 AS
                        (
                            SELECT a.BookingID,a.BOMMasterID,a.ItemGroupID,a.SubGroupID,a.Status,PreRevisionNo=a.PreProcessRevNo,RevisionNo=a.RevisionNo,
	                        b.BookingNo,b.BookingDate,b.Remarks,b.StyleNo,b.ExportOrderID,b.SLNo,b.StyleMasterID,
	                        b.BuyerID,b.BuyerTeamID,CompanyID=b.ExecutionCompanyID,b.SupplierID,b.SeasonID,a.WithoutOB,BookingQty= SUM(c.RequiredQty)
                            FROM FabricBookingAcknowledge A
							Inner Join {DbNames.EPYSL}..SampleBookingMaster b on b.BookingID = a.BookingID
							Inner Join {DbNames.EPYSL}..SampleBookingConsumptionChild c ON c.BookingID = a.BookingID
                            WHERE A.BookingID in ({bookingId})
							Group By a.BookingID,a.BOMMasterID,a.ItemGroupID,a.SubGroupID,a.Status,a.PreProcessRevNo,a.RevisionNo,
	                        b.BookingNo,b.BookingDate,b.Remarks,b.StyleNo,b.ExportOrderID,b.SLNo,b.StyleMasterID,
	                        b.BuyerID,b.BuyerTeamID,b.ExecutionCompanyID,b.SupplierID,b.SeasonID,a.WithoutOB
                        ),
						FBA AS
                        (
	                        SELECT *FROM FBA1
	                        UNION
	                        SELECT *FROM FBA2
                        ),
                        F AS
                        (
	                        SELECT FBA.BookingID,FBA.BOMMasterID,FBA.ItemGroupID,FBA.SubGroupID,FBA.Status,FBA.PreRevisionNo,FBA.RevisionNo,
	                        FBA.BookingNo,FBA.BookingDate,FBA.Remarks,FBA.StyleNo,FBA.ExportOrderID,FBA.StyleMasterID,
	                        FBA.BuyerID,FBA.BuyerTeamID,FBA.CompanyID,FBA.SupplierID,FBA.SeasonID,FBA.WithoutOB,
	                        CTO.ContactDisplayCode AS BuyerName, CCT.DisplayCode AS BuyerTeamName,CompanyName = C.ShortName,
	                        Supplier.ShortName [SupplierName],Season.SeasonName, ISNULL(Supplier.MappingCompanyID,0) TextileCompanyID,
	                        GmtQtyPcs = Sum(TotalPOQty)
	                        FROM FBA
	                        LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = FBA.BuyerID
	                        LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = FBA.BuyerTeamID
	                        LEFT JOIN {DbNames.EPYSL}..CompanyEntity C ON C.CompanyID = FBA.CompanyID
	                        LEFT Join {DbNames.EPYSL}..Contacts Supplier On Supplier.ContactID = FBA.SupplierID
	                        LEFT Join {DbNames.EPYSL}..ContactSeason Season On Season.SeasonID = FBA.SeasonID
	                        LEFT JOIN {DbNames.EPYSL}..OrderBankMaster OBM On OBM.StyleMasterID = FBA.StyleMasterID
	                        LEFT JOIN {DbNames.EPYSL}..OrderBankPO OBPO On OBPO.OrderBankMasterID = OBM.OrderBankMasterID AND OBPO.IsActive = 1
	                        GROUP BY FBA.BookingID,FBA.BOMMasterID,FBA.ItemGroupID,FBA.SubGroupID,FBA.Status,FBA.PreRevisionNo,FBA.RevisionNo,
	                        FBA.BookingNo,FBA.BookingDate,FBA.Remarks,FBA.StyleNo,FBA.ExportOrderID,FBA.StyleMasterID,
	                        FBA.BuyerID,FBA.BuyerTeamID,FBA.CompanyID,FBA.SupplierID,FBA.SeasonID,FBA.WithoutOB,
	                        CTO.ContactDisplayCode, CCT.DisplayCode,C.ShortName,
	                        Supplier.ShortName,Season.SeasonName, ISNULL(Supplier.MappingCompanyID,0)
                        )

						SELECT top 1 * FROM F;

                        ;WITH FBA1 AS
                        (
                            SELECT a.BookingID,a.BOMMasterID,a.ItemGroupID,a.SubGroupID,a.Status,PreRevisionNo=a.PreProcessRevNo,RevisionNo=a.RevisionNo,
	                        b.BookingNo,b.BookingDate,b.Remarks,StyleNo='',b.ExportOrderID,SLNo='',StyleMasterID=0,
	                        b.BuyerID,b.BuyerTeamID,b.CompanyID,b.SupplierID,SeasonID=0,a.WithoutOB
                            FROM FabricBookingAcknowledge A
							INNER JOIN {DbNames.EPYSL}..BookingMaster b on b.BookingID = a.BookingID
                            WHERE A.BookingID in ({bookingId})
                        ),
						FBA2 AS
                        (
                            SELECT a.BookingID,a.BOMMasterID,a.ItemGroupID,a.SubGroupID,a.Status,PreRevisionNo=a.PreProcessRevNo,RevisionNo=a.RevisionNo,
	                        b.BookingNo,b.BookingDate,b.Remarks,b.StyleNo,b.ExportOrderID,b.SLNo,b.StyleMasterID,
	                        b.BuyerID,b.BuyerTeamID,CompanyID=b.ExecutionCompanyID,b.SupplierID,b.SeasonID,a.WithoutOB
                            FROM FabricBookingAcknowledge A
							Inner Join {DbNames.EPYSL}..SampleBookingMaster b on b.BookingID = a.BookingID
                            WHERE A.BookingID in ({bookingId})
                        ),
						FBA AS
                        (
	                        SELECT *FROM FBA1
	                        UNION
	                        SELECT *FROM FBA2
                        ),
                        F AS
                        (
	                        SELECT FBA.BookingID,FBA.BOMMasterID,FBA.ItemGroupID,FBA.SubGroupID,FBA.Status,FBA.PreRevisionNo,FBA.RevisionNo,
	                        FBA.BookingNo,FBA.BookingDate,FBA.Remarks,FBA.StyleNo,FBA.ExportOrderID,FBA.StyleMasterID,
	                        FBA.BuyerID,FBA.BuyerTeamID,FBA.CompanyID,FBA.SupplierID,FBA.SeasonID,FBA.WithoutOB,
	                        CTO.ContactDisplayCode AS BuyerName, CCT.DisplayCode AS BuyerTeamName,CompanyName = C.ShortName,
	                        Supplier.ShortName [SupplierName],Season.SeasonName, ISNULL(Supplier.MappingCompanyID,0) TextileCompanyID,
	                        GmtQtyPcs = Sum(TotalPOQty)
	                        FROM FBA
	                        LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = FBA.BuyerID
	                        LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = FBA.BuyerTeamID
	                        LEFT JOIN {DbNames.EPYSL}..CompanyEntity C ON C.CompanyID = FBA.CompanyID
	                        LEFT Join {DbNames.EPYSL}..Contacts Supplier On Supplier.ContactID = FBA.SupplierID
	                        LEFT Join {DbNames.EPYSL}..ContactSeason Season On Season.SeasonID = FBA.SeasonID
	                        LEFT JOIN {DbNames.EPYSL}..OrderBankMaster OBM On OBM.StyleMasterID = FBA.StyleMasterID
	                        LEFT JOIN {DbNames.EPYSL}..OrderBankPO OBPO On OBPO.OrderBankMasterID = OBM.OrderBankMasterID AND OBPO.IsActive = 1
	                        GROUP BY FBA.BookingID,FBA.BOMMasterID,FBA.ItemGroupID,FBA.SubGroupID,FBA.Status,FBA.PreRevisionNo,FBA.RevisionNo,
	                        FBA.BookingNo,FBA.BookingDate,FBA.Remarks,FBA.StyleNo,FBA.ExportOrderID,FBA.StyleMasterID,
	                        FBA.BuyerID,FBA.BuyerTeamID,FBA.CompanyID,FBA.SupplierID,FBA.SeasonID,FBA.WithoutOB,
	                        CTO.ContactDisplayCode, CCT.DisplayCode,C.ShortName,
	                        Supplier.ShortName,Season.SeasonName, ISNULL(Supplier.MappingCompanyID,0)
                        )

						SELECT * FROM F;

                        ----- Sample Booking Consumption Child (Fabric)
	
                        WITH 
                        BPartName As(
	                        Select SCYS.BookingChildID,SCYS.BookingID, PartName = STRING_AGG(YSB.PartName,',')
	                        From {DbNames.EPYSL}..BookingChildGarmentPart SCYS
	                        Inner Join {DbNames.EPYSL}..BookingChild SBKC On SCYS.BookingID = SBKC.BookingID And SCYS.BookingChildID = SBKC.BookingChildID
	                        Inner Join {DbNames.EPYSL}..ItemGroup IG On IG.ItemGroupID = SBKC.ItemGroupID
	                        Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = SBKC.SubGroupID
	                        Inner Join {DbNames.EPYSL}..FabricUsedPart YSB On YSB.FUPartID = SCYS.FUPartID
	                        Inner Join {DbNames.EPYSL}..SampleBookingMaster BM ON BM.BookingID = SCYS.BookingID
	                        Where BM.BookingID in ({bookingId}) And ISG.SubGroupID = 1
	                        GROUP BY SCYS.BookingChildID,SCYS.BookingID
                        ),
                        FBA11 AS
                        (
	                        SELECT PN.PartName, 
	                        B.BookingID
	                        ,B.BookingNo, B.BuyerID, ExecutionCompanyID=B.CompanyID,
	                        FBAC.BookingChildID, A.ConsumptionID, A.ItemMasterID,A.SubGroupID,A.A1ValueID,A.YarnBrandID,ReferenceSourceID=0,YarnSourceID=0,
	                        ReferenceNo='',ColorReferenceNo='',A.ItemGroupID, B.RevisionNo,
	                        ISV1.SegmentValue Segment1Desc,ISV2.SegmentValue Segment2Desc,ISV3.SegmentValue Segment3Desc,
	                        ISV4.SegmentValue Segment4Desc,ISV5.SegmentValue Segment5Desc,ISV6.SegmentValue Segment6Desc, ISV7.SegmentValue Segment7Desc,
	                        A.LengthYds,A.LengthInch,A.FUPartID,A.ConsumptionQty,C.OrderUnitID,A.LabDipNo,A.Price,A.SuggestedPrice,
	                        RequiredQty=A.BookingQty,A.Remarks,ForBDSStyleNo='', B.SupplierID,
	                        LiabilitiesBookingQty=ISNULL(FBAC.LiabilitiesBookingQty,0),
	                        ActualBookingQty=ISNULL(FBAC.ActualBookingQty,0),BookingUOM = BU.DisplayUnitDesc,
	                        B.ExportOrderID,
                            MaxRevisionNoBk = (SELECT MAX(ISNULL(RevisionNo,0)) FROM {DbNames.EPYSL}..BookingChild_Bk Bk WHERE Bk.BookingID = A.BookingID),
                            FBAC.AcknowledgeID,
                            IsDeletedItem = CASE WHEN ISNULL(FBAC.BookingChildID,0) > 0 AND ISNULL(FBAC.BookingQty,0) = 0 THEN 1 ELSE 0 END
	                        FROM {DbNames.EPYSL}..BookingChild A 
	                        LEFT JOIN FBookingAcknowledgeChild FBAC ON A.BookingID = FBAC.BookingID AND FBAC.ConsumptionID = A.ConsumptionID AND FBAC.ItemMasterID = A.ItemMasterID AND FBAC.RevisionNoWhenDeleted = -1
	                        LEFT JOIN FBookingAcknowledge FBA ON FBA.FBAckID = FBAC.AcknowledgeID
	                        LEFT JOIN {DbNames.EPYSL}..Unit BU On BU.UnitID = A.BookingUnitID
	                        INNER JOIN {DbNames.EPYSL}..BookingMaster B ON B.BookingID = A.BookingID
	                        LEFT JOIN {DbNames.EPYSL}..BOMConsumption C ON C.ConsumptionID = A.ConsumptionID
	                        INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = A.ItemMasterID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
	                        Left Join BPartName PN On PN.BookingChildID = A.BookingChildID And PN.BookingID = A.BookingID
	                        WHERE A.BookingID in ({bookingId}) AND A.SubGroupID = 1
                        ),
                        RecordFromBK_BULK_Temp AS
                        (
	                        SELECT FBA.ConsumptionID, FBA.BookingID, FBA.ItemMasterID, MaxRevisionNoBk = MAX(FBA.MaxRevisionNoBk)
	                        FROM FBA11 FBA
	                        GROUP BY FBA.ConsumptionID, FBA.BookingID, FBA.ItemMasterID
                        ),
                        RecordFromBK_BULK AS
                        (
	                        SELECT FBA.ConsumptionID, FBA.BookingID
	                        ,PreviousBookingQty = Sum(ISNULL(BK.RequisitionQty,0))
	                        ,PreviousConsumptionQty = Sum(ISNULL(BK.RequisitionQty,0))
	                        ,PreviousBookingUnitID = MAX(ISNULL(BK.BookingUnitID,0))
	                        ,PreviousRequisitionQty = Sum(ISNULL(BK.RequisitionQty,0))
	                        ,PreviousPrice = Sum(ISNULL(BK.Price,0))
	                        ,PreviousSuggestedPrice = Sum(ISNULL(BK.SuggestedPrice,0))
	                        FROM RecordFromBK_BULK_Temp FBA
	                        INNER JOIN {DbNames.EPYSL}..BookingChild_Bk BK ON BK.BookingID = FBA.BookingID AND BK.ConsumptionID = FBA.ConsumptionID AND BK.ItemMasterID = FBA.ItemMasterID AND BK.RevisionNo = FBA.MaxRevisionNoBk
	                        GROUP BY FBA.ConsumptionID, FBA.BookingID
                        ),
                        FBA1 AS
                        (
	                        SELECT FBA.*
	                        ,BK.PreviousBookingQty
	                        ,BK.PreviousConsumptionQty
	                        ,BK.PreviousBookingUnitID
	                        ,BK.PreviousRequisitionQty
	                        ,BK.PreviousPrice
	                        ,BK.PreviousSuggestedPrice
	                        FROM FBA11 FBA
	                        LEFT JOIN RecordFromBK_BULK BK ON BK.BookingID = FBA.BookingID AND BK.ConsumptionID = FBA.ConsumptionID
                        ),
                        SPartName As(
	                        Select SCYS.ConsumptionID,SCYS.BookingID, PartName = STRING_AGG(YSB.PartName,',')
	                        From {DbNames.EPYSL}..SampleBookingConsumptionGarmentPart SCYS
	                        Inner Join {DbNames.EPYSL}..SampleBookingConsumption SBKC On SCYS.BookingID = SBKC.BookingID And SCYS.ConsumptionID = SBKC.ConsumptionID
	                        Inner Join {DbNames.EPYSL}..ItemGroup IG On IG.ItemGroupID = SBKC.ItemGroupID
	                        Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = SBKC.SubGroupID
	                        Inner Join {DbNames.EPYSL}..FabricUsedPart YSB On YSB.FUPartID = SCYS.FUPartID
	                        Inner Join {DbNames.EPYSL}..SampleBookingMaster BM ON BM.BookingID = SCYS.BookingID
	                        Where BM.BookingID in ({bookingId}) And ISG.SubGroupID = 1
	                        GROUP BY SCYS.ConsumptionID,SCYS.BookingID
                        ),
                        FBA22 AS
                        (
	                        SELECT PN.PartName, B.BookingID,B.BookingNo, B.BuyerID,B.ExecutionCompanyID,
                            BookingChildID = FBAC.BookingChildID, C.ConsumptionID, A.ItemMasterID,A.SubGroupID,C.A1ValueID,C.YarnBrandID,C.ReferenceSourceID,C.YarnSourceID,
                            C.ReferenceNo,C.ColorReferenceNo,A.ItemGroupID, B.RevisionNo,
                            C.Segment1Desc,C.Segment2Desc,C.Segment3Desc,
                            C.Segment4Desc,C.Segment5Desc,C.Segment6Desc, C.Segment7Desc,
                            C.LengthYds,C.LengthInch,C.FUPartID,A.ConsumptionQty,C.OrderUnitID,C.LabDipNo,C.Price,C.SuggestedPrice,
                            RequiredQty = A.RequiredQty,C.Remarks,C.ForBDSStyleNo, B.SupplierID,
                            LiabilitiesBookingQty=ISNULL(FBAC.LiabilitiesBookingQty,0),ActualBookingQty=ISNULL(FBAC.ActualBookingQty,0),BookingUOM = BU.DisplayUnitDesc,
	                        B.ExportOrderID,
                            MaxRevisionNoBk = (SELECT MAX(ISNULL(RevisionNo,0)) FROM {DbNames.EPYSL}..SampleBookingConsumptionChild_Bk Bk WHERE Bk.BookingID = A.BookingID),
                            FBAC.AcknowledgeID,
                            IsDeletedItem = CASE WHEN ISNULL(FBAC.BookingChildID,0) > 0 AND ISNULL(FBAC.BookingQty,0) = 0 THEN 1 ELSE 0 END
                            FROM {DbNames.EPYSL}..SampleBookingConsumptionChild A
	                        INNER JOIN {DbNames.EPYSL}..SampleBookingConsumption C ON C.ConsumptionID = A.ConsumptionID
	                        LEFT JOIN FBookingAcknowledgeChild FBAC ON A.ConsumptionID = FBAC.ConsumptionID AND A.ItemMasterID = FBAC.ItemMasterID AND FBAC.BookingID = C.BookingID AND FBAC.RevisionNoWhenDeleted = -1
	                        LEFT JOIN FBookingAcknowledge FBA ON FBA.FBAckID = FBAC.AcknowledgeID
                            LEFT JOIN {DbNames.EPYSL}..Unit BU On BU.UnitID = A.RequiredUnitID
                            LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster B ON B.BookingID = A.BookingID
                            Left Join SPartName PN On PN.ConsumptionID = A.ConsumptionID And PN.BookingID = A.BookingID
                            WHERE A.BookingID in ({bookingId}) AND A.SubGroupID = 1
                        ),
                        RecordFromBK_SMS AS
                        (
	                        SELECT FBA.ConsumptionID, FBA.BookingID
	                        ,PreviousBookingQty = Sum(ISNULL(BK.RequiredQty,0))
	                        ,PreviousConsumptionQty = Sum(ISNULL(BK.ConsumptionQty,0))
	                        ,PreviousBookingUnitID = MAX(ISNULL(BK.RequiredUnitID,0))
	                        ,PreviousRequisitionQty = Sum(ISNULL(BK.RequiredQty,0))
	                        ,PreviousPrice = 0
	                        ,PreviousSuggestedPrice = 0
	                        FROM FBA22 FBA
	                        INNER JOIN {DbNames.EPYSL}..SampleBookingConsumptionChild_Bk BK ON BK.BookingID = FBA.BookingID AND BK.ConsumptionID = FBA.ConsumptionID AND BK.ItemMasterID = FBA.ItemMasterID AND BK.RevisionNo = FBA.MaxRevisionNoBk
	                        WHERE FBA.IsDeletedItem = 0
                            GROUP BY FBA.ConsumptionID, FBA.BookingID
                        ),
                        FBA2 AS
                        (
	                        SELECT FBA.*
	                        ,BK.PreviousBookingQty
	                        ,BK.PreviousConsumptionQty
	                        ,BK.PreviousBookingUnitID
	                        ,BK.PreviousRequisitionQty
	                        ,BK.PreviousPrice
	                        ,BK.PreviousSuggestedPrice
	                        FROM FBA22 FBA
	                        LEFT JOIN RecordFromBK_SMS BK ON BK.BookingID = FBA.BookingID AND BK.ConsumptionID = FBA.ConsumptionID
                        ),
                        FBA AS
                        (
	                        SELECT * FROM FBA1
	                        UNION
	                        SELECT * FROM FBA2
                        ),
                        TS As(
	                        ---- Fabric
	                        Select ISF.BookingID,ISF.ItemMasterID,
	                        FinishFabricStockQty_KG  = SUM(Case When IM.SubGroupID = 1 Then ISNULL(ISF.RollQtyInKG,0) Else ISNULL(ISF.RollQtyInKGPcs,0) End),
                            FinishFabricStockQty_PCS = SUM(Case When IM.SubGroupID = 1 Then 0 Else ISNULL(ISF.RollQtyInKG,0) End)
	                        From EPYSLTEX..ItemFinishStockRoll ISF
	                        Inner Join {DbNames.EPYSL}..ItemMaster IM On IM.ItemMasterID = ISF.ItemMasterID
	                        Left Join {DbNames.EPYSL}..ItemSegmentValue ISV4 On ISV4.SegmentValueID = IM.Segment4ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
	                        --Inner Join EPYSLTEX..FBookingAcknowledgeChild FBAC on FBAC.BookingID = ISF.BookingID AND FBAC.ItemMasterID = ISF.ItemMasterID
                            Inner Join EPYSLTEX..FBookingAcknowledgeChildDetails FBAC on FBAC.BookingID = ISF.BookingID AND FBAC.ItemMasterID = ISF.ItemMasterID AND FBAC.ColorID = ISF.CCID4
	                        Where FBAC.BookingID in ({bookingId})  And FBAC.SubGroupID = 1 AND ISF.Issued = 0
	                        Group by ISF.BookingID,ISF.ItemMasterID
                        ),
                        DC_Bulk AS
                        (
	                        SELECT FBA1.BookingID, FBA1.ConsumptionID,
	                        DCKgQty = Sum(Case When FBA1.SubGroupID = 1 Then (IsNull(PDC.RollQtyInKG,0)-IsNull(PDC.AckShortQtyInKG,0)) Else (IsNull(PDC.RollQtyInKGPcs,0)-IsNull(PDC.AckShortQtyInKGPcs,0)) End),
                            DCPcsQty = Sum(Case When FBA1.SubGroupID = 1 Then 0 Else (IsNull(PDC.RollQtyInKG,0)-IsNull(PDC.AckShortQtyInKG,0)) End)
	                        FROM FBA1
	                        Inner Join {DbNames.EPYSL}..ItemMaster IM On IM.ItemMasterID = FBA1.ItemMasterID
	                        Left Join {DbNames.EPYSL}..ItemSegmentValue ISV3 On ISV3.SegmentValueID = IM.Segment3ValueID
	                        Left Join {DbNames.EPYSL}..BookingChildDetails BCD On BCD.BookingID = FBA1.BookingID and BCD.BookingChildID = FBA1.BookingChildID and BCD.ItemMasterID = FBA1.ItemMasterID
	                        Left Join PDChild DC On DC.BookingID = FBA1.BookingID 
							                              and DC.ItemMasterID = FBA1.ItemMasterID 
								  
								                          and DC.AOPItem = (SELECT Max(Case When ISNULL(TPM.ProcessName,'') = 'AOP' Then 1 Else 0 End)
													                        FROM {DbNames.EPYSL}..BookingChildProcess BCP
													                        Left Join {DbNames.EPYSL}..ProcessMaster TPM On TPM.ProcessID = BCP.ProcessID
													                        WHERE BCP.BookingChildID = FBA1.BookingChildID)
								  
								                          and IsNull(DC.CCID4,0) = Case When ISV3.SegmentValue = 'Semi Bleach' OR ISV3.SegmentValue = 'RFD' Then ISNULL(BCD.ColorID,0)
								                          Else 0 End
	                        Left Join PDMaster DM On DM.ExportOrderID = FBA1.ExportOrderID  and DM.PDID = DC.PDID 
	                        Left Join PDChildRoll PDC On PDC.PDID = DM.PDID and PDC.PDChildID = DC.PDChildID
	                        GROUP BY FBA1.BookingID, FBA1.ConsumptionID
                        ),
                        DC_SMS AS
                        (
	                        SELECT FBA2.BookingID, FBA2.ConsumptionID,
	                        DCKgQty = Sum(Case When FBA2.SubGroupID = 1 Then (IsNull(PDC.RollQtyInKG,0)-IsNull(PDC.AckShortQtyInKG,0)) Else (IsNull(PDC.RollQtyInKGPcs,0)-IsNull(PDC.AckShortQtyInKGPcs,0)) End),
                            DCPcsQty = Sum(Case When FBA2.SubGroupID = 1 Then 0 Else (IsNull(PDC.RollQtyInKG,0)-IsNull(PDC.AckShortQtyInKG,0)) End)
	                        FROM FBA2
	                        Inner Join {DbNames.EPYSL}..ItemMaster IM On IM.ItemMasterID = FBA2.ItemMasterID
	                        Left Join {DbNames.EPYSL}..ItemSegmentValue ISV3 On ISV3.SegmentValueID = IM.Segment3ValueID
	                        Left Join {DbNames.EPYSL}..SampleBookingConsumptionColor SBCC On SBCC.BookingID = FBA2.BookingID and SBCC.ConsumptionID = FBA2.ConsumptionID 
	                        Left Join PDChild DC On DC.BookingID = FBA2.BookingID 
	                                            and DC.ItemMasterID = FBA2.ItemMasterID 

						                        and DC.AOPItem = (SELECT Max(Case When ISNULL(TPM.ProcessName,'') = 'AOP' Then 1 Else 0 End) 
										                          FROM {DbNames.EPYSL}..SampleBookingConsumptionProcess BCP
										                          Left Join {DbNames.EPYSL}..ProcessMaster TPM On TPM.ProcessID = BCP.ProcessID
										                          WHERE BCP.ConsumptionID = FBA2.ConsumptionID)

						                        and IsNull(DC.CCID4,0) = Case When ISV3.SegmentValue = 'Semi Bleach' OR ISV3.SegmentValue = 'RFD' Then ISNULL(SBCC.ColorID,0) 
						                        Else 0 End
	                        Left Join PDMaster DM On DM.ExportOrderID = FBA2.ExportOrderID  and DM.PDID = DC.PDID 
	                        Left Join PDChildRoll PDC On PDC.PDID = DM.PDID and PDC.PDChildID = DC.PDChildID
	                        GROUP BY FBA2.BookingID, FBA2.ConsumptionID
                        ),
                        ALL_DC AS
                        (
	                        SELECT * FROM DC_Bulk
	                        UNION
	                        SELECT * FROM DC_SMS
                        ),
                        F AS 
                        (
	                        SELECT TotalFinishFabricStockQty = TS.FinishFabricStockQty_KG, SBM.PartName,SBM.BookingChildID, SBM.ReferenceSourceID,RSETV.ValueName ReferenceSourceName , SBM.ReferenceNo,SBM.ColorReferenceNo,SBM.YarnSourceID,SBM.ConsumptionID,SBM.BookingID,SBM.BookingNo,SBM.ItemGroupID,SBM.SubGroupID,
	                        ISV1.SegmentValue Construction,ISV2.SegmentValue Composition,ISV3.SegmentValue Color
	                        ,ISV4.SegmentValue GSM,ISV5.SegmentValue FabricWidth,ISV7.SegmentValue KnittingType,SBM.LengthYds,SBM.LengthInch,SBM.FUPartID,SBM.ConsumptionQty,SBM.OrderUnitID BookingUnitID
	                        ,SBM.A1ValueID,SBM.YarnBrandID,SBM.LabDipNo,SBM.Price,SBM.SuggestedPrice,SBM.RequiredQty BookingQty,SBM.ItemMasterID,SBM.BuyerID ContactID,
	                        SBM.ExecutionCompanyID,
	                        0 As TechnicalNameId,ISG.SubGroupName,'1' As ConceptTypeID,IM.Segment1ValueID ConstructionId, IM.Segment2ValueID CompositionId,
	                        IM.Segment3ValueID ColorId,IM.Segment7ValueID KnittingTypeId,IM.Segment4ValueID GSMId,
	                        ISV.SegmentValue YarnType, ETV.ValueName YarnProgram, ISV6.SegmentValue DyeingType, SBM.Remarks Instruction, SBM.ForBDSStyleNo, ISNULL(Supplier.MappingCompanyID,0) TextileCompanyID,PreviousBookingQty=ISNULL(SBM.PreviousBookingQty,0),
	                        PreviousConsumptionQty=ISNULL(SBM.PreviousConsumptionQty,0),PreviousRequisitionQty=ISNULL(SBM.PreviousRequisitionQty,0),PreviousBookingUnitID=ISNULL(SBM.PreviousBookingUnitID,0),PreviousPrice=ISNULL(SBM.PreviousPrice,0),PreviousSuggestedPrice=ISNULL(SBM.PreviousSuggestedPrice,0),
	                        LiabilitiesBookingQty=ISNULL(SBM.LiabilitiesBookingQty,0),ActualBookingQty=ISNULL(SBM.ActualBookingQty,0),SBM.BookingUOM,
                            RefSourceNo = CASE WHEN ISNULL(RS.RefSourceNo,'') = '' THEN 'New Item' ELSE RS.RefSourceNo END,
	                        DeliveredQtyForLiability = CASE WHEN SBM.SubGroupID = 1 THEN ISNULL(DC.DCKgQty,0) ELSE ISNULL(DC.DCPcsQty,0) END,
                            SBM.AcknowledgeID,
                            SBM.IsDeletedItem

	                        FROM FBA SBM
                            LEFT JOIN {DbNames.EPYSL}..BookingChildReferenceSource RS ON RS.BookingID = SBM.BookingID AND RS.ConsumptionID = SBM.ConsumptionID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = SBM.SubGroupID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV On ISV.SegmentValueID = SBM.A1ValueID
	                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = SBM.YarnBrandID
	                        Left Join {DbNames.EPYSL}..EntityTypeValue RSETV ON RSETV.ValueID = SBM.ReferenceSourceID
	                        Left Join {DbNames.EPYSL}..EntityTypeValue YSETV ON YSETV.ValueID = SBM.YarnSourceID
	                        LEFT Join {DbNames.EPYSL}..Contacts Supplier On Supplier.ContactID = SBM.SupplierID

	                        LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = SBM.ItemMasterID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                            Left Join TS On TS.ItemMasterID = SBM.ItemMasterID And TS.BookingID = SBM.BookingID
	                        LEFT JOIN ALL_DC DC ON DC.BookingID = SBM.BookingID AND DC.ConsumptionID = SBm.ConsumptionID
	                        WHERE SBM.BookingID in ({bookingId})
                            GROUP BY TS.FinishFabricStockQty_KG,SBM.PartName,SBM.BookingChildID, SBM.ReferenceSourceID,RSETV.ValueName , SBM.ReferenceNo,SBM.ColorReferenceNo,SBM.YarnSourceID,SBM.ConsumptionID,SBM.BookingID,SBM.BookingNo,SBM.ItemGroupID,SBM.SubGroupID,
	                        ISV1.SegmentValue,ISV2.SegmentValue,ISV3.SegmentValue
	                        ,ISV4.SegmentValue,ISV5.SegmentValue,ISV7.SegmentValue,SBM.LengthYds,SBM.LengthInch,SBM.FUPartID,SBM.ConsumptionQty,SBM.OrderUnitID
	                        ,SBM.A1ValueID,SBM.YarnBrandID,SBM.LabDipNo,SBM.Price,SBM.SuggestedPrice,SBM.RequiredQty,SBM.ItemMasterID,SBM.BuyerID,
	                        SBM.ExecutionCompanyID,
	                        ISG.SubGroupName,IM.Segment1ValueID, IM.Segment2ValueID,
	                        IM.Segment3ValueID,IM.Segment7ValueID,IM.Segment4ValueID,
	                        ISV.SegmentValue, ETV.ValueName, ISV6.SegmentValue, SBM.Remarks, SBM.ForBDSStyleNo, ISNULL(Supplier.MappingCompanyID,0),ISNULL(SBM.PreviousBookingQty,0),
	                        ISNULL(SBM.PreviousConsumptionQty,0),ISNULL(SBM.PreviousRequisitionQty,0),ISNULL(SBM.PreviousBookingUnitID,0),ISNULL(SBM.PreviousPrice,0),ISNULL(SBM.PreviousSuggestedPrice,0),
	                        ISNULL(SBM.LiabilitiesBookingQty,0),ISNULL(SBM.ActualBookingQty,0),SBM.BookingUOM,
                            CASE WHEN ISNULL(RS.RefSourceNo,'') = '' THEN 'New Item' ELSE RS.RefSourceNo END,
	                        CASE WHEN SBM.SubGroupID = 1 THEN ISNULL(DC.DCKgQty,0) ELSE ISNULL(DC.DCPcsQty,0) END,
                            SBM.AcknowledgeID,SBM.IsDeletedItem
                        )
                        SELECT * FROM F
                        ORDER BY F.Color, F.GSM, F.FabricWidth;

                        ----- Sample Booking Consumption Child (Collor & Cuff)
                        WITH FBA11 AS
                        (
	                        SELECT B.BookingID,B.BookingNo, B.BuyerID, ExecutionCompanyID=B.CompanyID,
	                        FBAC.BookingChildID, A.ConsumptionID, A.ItemMasterID,A.SubGroupID,A.A1ValueID,A.YarnBrandID,ReferenceSourceID=0,YarnSourceID=0,
	                        ReferenceNo='',ColorReferenceNo='',A.ItemGroupID, B.RevisionNo,
	                        C.Segment1Desc,C.Segment2Desc,C.Segment3Desc,
	                        C.Segment4Desc,C.Segment5Desc,C.Segment6Desc, C.Segment7Desc,
	                        A.LengthYds,A.LengthInch,A.FUPartID,A.ConsumptionQty,C.OrderUnitID,A.LabDipNo,A.Price,A.SuggestedPrice,
	                        RequiredQty=A.BookingQty,A.Remarks,ForBDSStyleNo='', B.SupplierID,
	                        LiabilitiesBookingQty=ISNULL(FBAC.LiabilitiesBookingQty,0),ActualBookingQty=ISNULL(FBAC.ActualBookingQty,0),BookingUOM = BU.DisplayUnitDesc,
	                        B.ExportOrderID,
                            MaxRevisionNoBk = (SELECT MAX(ISNULL(RevisionNo,0)) FROM {DbNames.EPYSL}..BookingChild_Bk Bk WHERE Bk.BookingID = A.BookingID),
                            FBAC.AcknowledgeID,
                            IsDeletedItem = CASE WHEN ISNULL(FBAC.BookingChildID,0) > 0 AND ISNULL(FBAC.BookingQty,0) = 0 THEN 1 ELSE 0 END

	                        FROM {DbNames.EPYSL}..BookingChild A
	                        LEFT JOIN FBookingAcknowledgeChild FBAC ON A.BookingID = FBAC.BookingID AND A.ConsumptionID = FBAC.ConsumptionID
	                        LEFT JOIN FBookingAcknowledge FBA ON FBA.FBAckID = FBAC.AcknowledgeID
	                        LEFT JOIN {DbNames.EPYSL}..Unit BU On BU.UnitID = A.BookingUnitID
	                        LEFT JOIN {DbNames.EPYSL}..BookingMaster B ON B.BookingID = A.BookingID
	                        LEFT JOIN {DbNames.EPYSL}..BOMConsumption C ON C.ConsumptionID = A.ConsumptionID
	                        WHERE A.BookingID in ({bookingId}) AND A.SubGroupID IN (11,12)
                        ),
                        RecordFromBK_BULK_Temp AS
                        (
	                        SELECT FBA.ConsumptionID, FBA.BookingID, FBA.ItemMasterID, MaxRevisionNoBk = MAX(FBA.MaxRevisionNoBk)
	                        FROM FBA11 FBA
	                        GROUP BY FBA.ConsumptionID, FBA.BookingID, FBA.ItemMasterID
                        ),
                        RecordFromBK_BULK AS
                        (
	                        SELECT FBA.ConsumptionID, FBA.BookingID
	                        ,PreviousBookingQty = Sum(ISNULL(BK.RequisitionQty,0))
	                        FROM RecordFromBK_BULK_Temp FBA
	                        INNER JOIN {DbNames.EPYSL}..BookingChild_Bk BK ON BK.BookingID = FBA.BookingID AND BK.ConsumptionID = FBA.ConsumptionID AND BK.ItemMasterID = FBA.ItemMasterID AND BK.RevisionNo = FBA.MaxRevisionNoBk
	                        GROUP BY FBA.ConsumptionID, FBA.BookingID
                        ),
                        FBA1 AS
                        (
	                        SELECT FBA.*
	                        ,BK.PreviousBookingQty
	                        FROM FBA11 FBA
	                        LEFT JOIN RecordFromBK_BULK BK ON BK.BookingID = FBA.BookingID AND BK.ConsumptionID = FBA.ConsumptionID
                        ),
                        FBA22 AS
                        (
	                        SELECT B.BookingID
	                        ,B.BookingNo, B.BuyerID,B.ExecutionCompanyID,
	                        BookingChildID = FBAC.BookingChildID, C.ConsumptionID, A.ItemMasterID,A.SubGroupID,C.A1ValueID,C.YarnBrandID,C.ReferenceSourceID,C.YarnSourceID,
	                        C.ReferenceNo,C.ColorReferenceNo,A.ItemGroupID, B.RevisionNo,
	                        C.Segment1Desc,C.Segment2Desc,C.Segment3Desc,
	                        C.Segment4Desc,C.Segment5Desc,C.Segment6Desc, C.Segment7Desc,
	                        C.LengthYds,C.LengthInch,C.FUPartID,C.ConsumptionQty,C.OrderUnitID,C.LabDipNo,C.Price,C.SuggestedPrice,
	                        RequiredQty = A.RequiredQty,C.Remarks,C.ForBDSStyleNo, B.SupplierID,
	                        LiabilitiesBookingQty=ISNULL(FBAC.LiabilitiesBookingQty,0),ActualBookingQty=ISNULL(FBAC.ActualBookingQty,0),BookingUOM = BU.DisplayUnitDesc,
	                        B.ExportOrderID,
                            MaxRevisionNoBk = (SELECT MAX(ISNULL(RevisionNo,0)) FROM {DbNames.EPYSL}..SampleBookingConsumptionChild_Bk Bk WHERE Bk.BookingID = A.BookingID),
                            FBAC.AcknowledgeID,
                            IsDeletedItem = CASE WHEN ISNULL(FBAC.BookingChildID,0) > 0 AND ISNULL(FBAC.BookingQty,0) = 0 THEN 1 ELSE 0 END

	                        FROM {DbNames.EPYSL}..SampleBookingConsumptionChild A
	                        INNER JOIN {DbNames.EPYSL}..SampleBookingConsumption C ON C.ConsumptionID = A.ConsumptionID
	                        LEFT JOIN FBookingAcknowledgeChild FBAC ON A.ConsumptionID = FBAC.ConsumptionID AND A.ItemMasterID = FBAC.ItemMasterID AND FBAC.BookingID = C.BookingID
	                        LEFT JOIN FBookingAcknowledge FBA ON FBA.FBAckID = FBAC.AcknowledgeID
	                        LEFT JOIN {DbNames.EPYSL}..Unit BU On BU.UnitID = A.RequiredUnitID
	                        LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster B ON B.BookingID = C.BookingID
	                        WHERE A.BookingID in ({bookingId}) AND A.SubGroupID IN (11,12)
                        ),
                        RecordFromBK_SMS AS
                        (
	                        SELECT FBA.ConsumptionID, FBA.BookingID
	                        ,PreviousBookingQty = Sum(ISNULL(BK.RequiredQty,0))
	                        FROM FBA22 FBA
	                        INNER JOIN {DbNames.EPYSL}..SampleBookingConsumptionChild_Bk BK ON BK.BookingID = FBA.BookingID AND BK.ConsumptionID = FBA.ConsumptionID AND BK.ItemMasterID = FBA.ItemMasterID AND BK.RevisionNo = FBA.MaxRevisionNoBk
	                        WHERE FBA.IsDeletedItem = 0
                            GROUP BY FBA.ConsumptionID, FBA.BookingID
                        ),
                        FBA2 AS
                        (
	                        SELECT FBA.*
	                        ,BK.PreviousBookingQty
	                        FROM FBA22 FBA
	                        LEFT JOIN RecordFromBK_SMS BK ON BK.BookingID = FBA.BookingID AND BK.ConsumptionID = FBA.ConsumptionID
                        ),
                        FBA AS
                        (
	                        SELECT * FROM FBA1
	                        UNION
	                        SELECT * FROM FBA2
                        ),
                        TS As(
	                        ---- Collar, Cuff
	                        Select  ISF.BookingID,ISF.ItemMasterID,FBAC.BookingChildID,  
	                        FinishFabricStockQty_KG  = SUM(Case When IM.SubGroupID = 1 Then ISNULL(ISF.RollQtyInKG,0) Else ISNULL(ISF.RollQtyInKGPcs,0) End),
                            FinishFabricStockQty_PCS = SUM(Case When IM.SubGroupID = 1 Then 0 Else ISNULL(ISF.RollQtyInKG,0) End)
	                        From EPYSLTEX..ItemFinishStockRoll ISF
	                        Inner Join {DbNames.EPYSL}..ItemMaster IM On IM.ItemMasterID = ISF.ItemMasterID
	                        Inner Join EPYSLTEX..FBookingAcknowledgeChild FBAC on FBAC.BookingID = ISF.BookingID AND FBAC.ItemMasterID = ISF.ItemMasterID
	                        Left Join {DbNames.EPYSL}..ItemSegmentValue ISV4 On ISV4.SegmentValueID = IM.Segment4ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
	                        Where FBAC.BookingID in ({bookingId}) And FBAC.SubGroupID in(11,12) AND ISF.Issued = 0
	                        Group by ISF.BookingID,ISF.ItemMasterID,FBAC.BookingChildID					
                        ),
                        DC_Bulk AS
                        (
	                        SELECT FBA1.BookingID, FBA1.ConsumptionID,
	                        DCKgQty = Sum(Case When FBA1.SubGroupID = 1 Then (IsNull(PDC.RollQtyInKG,0)-IsNull(PDC.AckShortQtyInKG,0)) Else (IsNull(PDC.RollQtyInKGPcs,0)-IsNull(PDC.AckShortQtyInKGPcs,0)) End),
                            DCPcsQty = Sum(Case When FBA1.SubGroupID = 1 Then 0 Else (IsNull(PDC.RollQtyInKG,0)-IsNull(PDC.AckShortQtyInKG,0)) End)
	                        FROM FBA1
	                        Inner Join {DbNames.EPYSL}..ItemMaster IM On IM.ItemMasterID = FBA1.ItemMasterID
	                        Left Join {DbNames.EPYSL}..ItemSegmentValue ISV3 On ISV3.SegmentValueID = IM.Segment3ValueID
	                        Left Join {DbNames.EPYSL}..BookingChildDetails BCD On BCD.BookingID = FBA1.BookingID and BCD.BookingChildID = FBA1.BookingChildID and BCD.ItemMasterID = FBA1.ItemMasterID
	                        Left Join PDChild DC On DC.BookingID = FBA1.BookingID 
							                              and DC.ItemMasterID = FBA1.ItemMasterID
								                          and IsNull(DC.CCID4,0) = Case When ISV3.SegmentValue = 'Semi Bleach' OR ISV3.SegmentValue = 'RFD' Then ISNULL(BCD.ColorID,0)
								                          Else 0 End
	                        Left Join PDMaster DM On DM.ExportOrderID = FBA1.ExportOrderID  and DM.PDID = DC.PDID 
	                        Left Join PDChildRoll PDC On PDC.PDID = DM.PDID and PDC.PDChildID = DC.PDChildID
	                        GROUP BY FBA1.BookingID, FBA1.ConsumptionID
                        ),
                        DC_SMS AS
                        (
	                        SELECT FBA2.BookingID, FBA2.ConsumptionID,
	                        DCKgQty = Sum(Case When FBA2.SubGroupID = 1 Then (IsNull(PDC.RollQtyInKG,0)-IsNull(PDC.AckShortQtyInKG,0)) Else (IsNull(PDC.RollQtyInKGPcs,0)-IsNull(PDC.AckShortQtyInKGPcs,0)) End),
                            DCPcsQty = Sum(Case When FBA2.SubGroupID = 1 Then 0 Else (IsNull(PDC.RollQtyInKG,0)-IsNull(PDC.AckShortQtyInKG,0)) End)
	                        FROM FBA2
	                        Inner Join {DbNames.EPYSL}..ItemMaster IM On IM.ItemMasterID = FBA2.ItemMasterID
	                        Left Join {DbNames.EPYSL}..ItemSegmentValue ISV3 On ISV3.SegmentValueID = IM.Segment3ValueID
	                        Left Join {DbNames.EPYSL}..SampleBookingConsumptionColor SBCC On SBCC.BookingID = FBA2.BookingID and SBCC.ConsumptionID = FBA2.ConsumptionID 
	                        Left Join PDChild DC On DC.BookingID = FBA2.BookingID 
	                                            and DC.ItemMasterID = FBA2.ItemMasterID
						                        and IsNull(DC.CCID4,0) = Case When ISV3.SegmentValue = 'Semi Bleach' OR ISV3.SegmentValue = 'RFD' Then ISNULL(SBCC.ColorID,0) 
						                        Else 0 End
	                        Left Join PDMaster DM On DM.ExportOrderID = FBA2.ExportOrderID  and DM.PDID = DC.PDID 
	                        Left Join PDChildRoll PDC On PDC.PDID = DM.PDID and PDC.PDChildID = DC.PDChildID
	                        GROUP BY FBA2.BookingID, FBA2.ConsumptionID
                        ),
                        ALL_DC AS
                        (
	                        SELECT * FROM DC_Bulk
	                        UNION
	                        SELECT * FROM DC_SMS
                        ),
                        F AS 
                        (
	                        SELECT TotalFinishFabricStockQty = TS.FinishFabricStockQty_PCS, SBM.BookingChildID, SBM.ReferenceSourceID,RSETV.ValueName ReferenceSourceName , SBM.ReferenceNo,SBM.ColorReferenceNo,SBM.YarnSourceID,SBM.ConsumptionID,SBM.BookingID,SBM.BookingNo,SBM.ItemGroupID,SBM.SubGroupID,
	
	                        ISV1.SegmentValue Segment1Desc,
	                        ISV2.SegmentValue Segment2Desc,
	                        ISV3.SegmentValue Segment3Desc,
	                        ISV4.SegmentValue Segment4Desc,
	                        ISV5.SegmentValue Segment5Desc,
	                        ISV6.SegmentValue Segment6Desc,
	
	                        SBM.LengthYds,SBM.LengthInch,SBM.FUPartID,SBM.ConsumptionQty,BookingUnitID = 1
	                        ,SBM.A1ValueID,SBM.YarnBrandID,SBM.LabDipNo,SBM.Price,SBM.SuggestedPrice,SBM.RequiredQty BookingQty,SBM.ItemMasterID,SBM.BuyerID ContactID,
	                        SBM.ExecutionCompanyID,
	                        0 As TechnicalNameId,ISG.SubGroupName,'1' As ConceptTypeID,IM.Segment1ValueID ConstructionId, IM.Segment2ValueID CompositionId,
	                        IM.Segment3ValueID ColorId,IM.Segment7ValueID KnittingTypeId,IM.Segment4ValueID GSMId,
	                        ISV.SegmentValue YarnType, ETV.ValueName YarnProgram, SBM.Segment6Desc DyeingType, SBM.Remarks Instruction, SBM.ForBDSStyleNo, ISNULL(Supplier.MappingCompanyID,0) TextileCompanyID,PreviousBookingQty=ISNULL(SBM.PreviousBookingQty,0),LiabilitiesBookingQty=ISNULL(SBM.LiabilitiesBookingQty,0),ActualBookingQty=ISNULL(SBM.ActualBookingQty,0),SBM.BookingUOM
	                        ,RefSourceNo = CASE WHEN ISNULL(RS.RefSourceNo,'') = '' THEN 'New Item' ELSE RS.RefSourceNo END
	                        ,DeliveredQtyForLiability = CASE WHEN SBM.SubGroupID = 1 THEN ISNULL(DC.DCKgQty,0) ELSE ISNULL(DC.DCPcsQty,0) END,
                            SBM.AcknowledgeID,
                            SBM.IsDeletedItem

                            FROM FBA SBM
                            LEFT JOIN {DbNames.EPYSL}..BookingChildReferenceSource RS ON RS.BookingID = SBM.BookingID AND RS.ConsumptionID = SBM.ConsumptionID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = SBM.SubGroupID
	                        LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = SBM.ItemMasterID

	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID

	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV On ISV.SegmentValueID = SBM.A1ValueID
	                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = SBM.YarnBrandID
	                        Left Join {DbNames.EPYSL}..EntityTypeValue RSETV ON RSETV.ValueID = SBM.ReferenceSourceID
	                        Left Join {DbNames.EPYSL}..EntityTypeValue YSETV ON YSETV.ValueID = SBM.YarnSourceID
	                        LEFT Join {DbNames.EPYSL}..Contacts Supplier On Supplier.ContactID = SBM.SupplierID
	                        Left Join TS On TS.ItemMasterID = SBM.ItemMasterID And TS.BookingID = SBM.BookingID
	                        LEFT JOIN ALL_DC DC ON DC.BookingID = SBM.BookingID AND DC.ConsumptionID = SBm.ConsumptionID
	                        WHERE SBM.BookingID in ({bookingId})
                            GROUP BY TS.FinishFabricStockQty_PCS,SBM.BookingChildID, SBM.ReferenceSourceID,RSETV.ValueName, SBM.ReferenceNo,SBM.ColorReferenceNo,SBM.YarnSourceID,SBM.ConsumptionID,SBM.BookingID,SBM.BookingNo,SBM.ItemGroupID,SBM.SubGroupID,
	
	                        ISV1.SegmentValue,
	                        ISV2.SegmentValue,
	                        ISV3.SegmentValue,
	                        ISV4.SegmentValue,
	                        ISV5.SegmentValue,
	                        ISV6.SegmentValue,
	
	                        SBM.LengthYds,SBM.LengthInch,SBM.FUPartID,SBM.ConsumptionQty
	                        ,SBM.A1ValueID,SBM.YarnBrandID,SBM.LabDipNo,SBM.Price,SBM.SuggestedPrice,SBM.RequiredQty,SBM.ItemMasterID,SBM.BuyerID,
	                        SBM.ExecutionCompanyID,
	                        ISG.SubGroupName,IM.Segment1ValueID, IM.Segment2ValueID,
	                        IM.Segment3ValueID,IM.Segment7ValueID,IM.Segment4ValueID,
	                        ISV.SegmentValue, ETV.ValueName, SBM.Segment6Desc,SBM.Remarks, SBM.ForBDSStyleNo, ISNULL(Supplier.MappingCompanyID,0),ISNULL(SBM.PreviousBookingQty,0),ISNULL(SBM.LiabilitiesBookingQty,0),ISNULL(SBM.ActualBookingQty,0),SBM.BookingUOM
	                        ,CASE WHEN ISNULL(RS.RefSourceNo,'') = '' THEN 'New Item' ELSE RS.RefSourceNo END
	                        ,CASE WHEN SBM.SubGroupID = 1 THEN ISNULL(DC.DCKgQty,0) ELSE ISNULL(DC.DCPcsQty,0) END
                            ,SBM.AcknowledgeID,SBM.IsDeletedItem
                        )
                        SELECT * FROM F
                        ORDER BY F.Segment5Desc, F.Segment1Desc,F.Segment2Desc;

                       ----Sample Booking Consumption AddProcess
                        SELECT  BookingID ,ProcessID,ConsumptionID
                        FROM {DbNames.EPYSL}..SampleBookingConsumptionAddProcess where BookingID in ({bookingId});

                        ----- Sample Booking Consumption Child Details
                        SELECT ConsumptionID,BookingID,ItemGroupID,SubGroupID,ItemMasterID,RequiredQty BookingQty ,ConsumptionQty,RequiredUnitID BookingUnitID, 0 As TechnicalNameId
                        FROM {DbNames.EPYSL}..SampleBookingConsumptionChild where BookingID in ({bookingId})

                        ----- Sample Booking Consumption Garment Part
                        SELECT  BookingID ,FUPartID,ConsumptionID
                         FROM {DbNames.EPYSL}..SampleBookingConsumptionGarmentPart where BookingID in ({bookingId})

                        ---Sample Booking Consumption Process
                        SELECT BookingID,ProcessID,ConsumptionID
                        FROM {DbNames.EPYSL}..SampleBookingConsumptionProcess where BookingID in ({bookingId})

                        ------ Sample Booking Consumption Text
                        SELECT BookingID,UsesIn,AdditionalProcess,ApplicableProcess,YarnSubProgram,GarmentsColor GmtColor,ConsumptionID
                        FROM {DbNames.EPYSL}..SampleBookingConsumptionText  where BookingID in ({bookingId})

                        ------ Sample Booking Child Distribution 
                        SELECT *
                        FROM {DbNames.EPYSL}..SampleBookingChildDistribution  where BookingID in ({bookingId})

                        ----- SampleBooking ConsumptionYarn Sub Brand
                        SELECT SBKC.ReferenceSourceID,RSETV.ValueName ReferenceSourceName , SBKC.ReferenceNo,SBKC.ColorReferenceNo,SBKC.YarnSourceID,SCYS.BookingID, SCYS.YarnSubBrandID, SCYS.ConsumptionID, YSB.YarnSubBrandName
				        FROM {DbNames.EPYSL}..SampleBookingConsumptionYarnSubBrand SCYS
				        Inner Join {DbNames.EPYSL}..SampleBookingConsumption SBKC On SCYS.BookingID = SBKC.BookingID And SCYS.ConsumptionID = SBKC.ConsumptionID
                        Inner Join {DbNames.EPYSL}..ItemGroup IG On IG.ItemGroupID = SBKC.ItemGroupID
                        Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = SBKC.SubGroupID
                        Left Join {DbNames.EPYSL}..EntityTypeValue RSETV ON RSETV.ValueID = SBKC.ReferenceSourceID
                        Left Join  {DbNames.EPYSL}..EntityTypeValue YSETV ON YSETV.ValueID = SBKC.YarnSourceID
				        Inner Join (
					        Select ETV.ValueID YarnSubBrandID, ETV.ValueName YarnSubBrandName
					        From {DbNames.EPYSL}..EntityTypeValue ETV
					        Inner Join {DbNames.EPYSL}..EntityType ET On ET.EntityTypeID = ETV.EntityTypeID
				        ) YSB On YSB.YarnSubBrandID = SCYS.YarnSubBrandID
				        Where SCYS.BookingID in ({bookingId});

                        ----- SampleBookingChildImage
                        SELECT BookingID,ImagePath
                        FROM {DbNames.EPYSL}..SampleBookingChildImage where BookingID in ({bookingId})

                        --Free Concept
                        Select SBC.ReferenceSourceID,RSETV.ValueName ReferenceSourceName , SBC.ReferenceNo,SBC.ColorReferenceNo,SBC.YarnSourceID,FB.BookingID, FB.BookingNo, FB.BookingDate ConceptDate, SBC.RequiredQty Qty,
                        CC.ItemMasterID, IM.Segment1ValueID ConstructionId, IM.Segment2ValueID CompositionId, IM.Segment3ValueID ColorId, Color.SegmentValue ColorName, IM.Segment4ValueID GSMId,
                        IM.Segment7ValueID KnittingTypeId, IM.SubGroupID,FB.ExecutionCompanyID CompanyID, ISNULL(Supplier.MappingCompanyID,0) TextileCompanyID
                        FROM {DbNames.EPYSL}..SampleBookingConsumption SBC
                        INNER JOIN {DbNames.EPYSL}..SampleBookingConsumptionChild CC ON CC.ConsumptionID = SBC.ConsumptionID
                        INNER JOIN {DbNames.EPYSL}..SampleBookingMaster FB ON FB.BookingID = SBC.BookingID
                        INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = CC.ItemMasterID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = IM.Segment1ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = IM.Segment2ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = IM.Segment3ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = IM.Segment4ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Width ON Width.SegmentValueID = IM.Segment5ValueID
                        --LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue DT ON DT.SegmentValueID = IM.Segment6ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue KT ON KT.SegmentValueID = IM.Segment7ValueID
                        --LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID = FB.SubGroupID
                        Left Join  {DbNames.EPYSL}..EntityTypeValue RSETV ON RSETV.ValueID = SBC.ReferenceSourceID
                        Left Join  {DbNames.EPYSL}..EntityTypeValue YSETV ON YSETV.ValueID = SBC.YarnSourceID
                        LEFT Join {DbNames.EPYSL}..Contacts Supplier On Supplier.ContactID = FB.SupplierID
                        where FB.BookingID in ({bookingId});

                        --Technical Name
                        SELECT Cast(T.TechnicalNameId As varchar) id, T.TechnicalName [text], ISNULL(ST.[Days], 0) [desc], Cast(SC.SubClassID as varchar) additionalValue
                        FROM FabricTechnicalName T
                        LEFT JOIN FabricTechnicalNameKMachineSubClass SC ON SC.TechnicalNameID = T.TechnicalNameId
                        LEFT JOIN KnittingMachineStructureType_HK ST ON ST.StructureTypeID = SC.StructureTypeID
                        Group By T.TechnicalNameId, T.TechnicalName, ST.Days, SC.SubClassID;

                         --YarnSource data load
				        Select Cast(a.ValueID as varchar) id, a.ValueName [text]
				        From {DbNames.EPYSL}..EntityTypeValue a
				        Inner Join {DbNames.EPYSL}..EntityType b on b.EntityTypeID = a.EntityTypeID
				        Where b.EntityTypeName = 'Yarn Source'

                        --M/c type
                        ;SELECT CAST(a.MachineSubClassID AS varchar) [id], b.SubClassName [text], b.TypeID [desc], c.TypeName additionalValue
                        FROM KnittingMachine a
                        INNER JOIN KnittingMachineSubClass b ON b.SubClassID = a.MachineSubClassID
                        Inner Join KnittingMachineType c On c.TypeID = b.TypeID
                        --Where c.TypeName != 'Flat Bed'
                        GROUP BY a.MachineSubClassID, b.SubClassName, b.TypeID, c.TypeName;

                        --CriteriaNames
                         ;SELECT CriteriaName,CriteriaSeqNo, (CASE WHEN CriteriaName  IN('Batch Preparation','Quality Check') THEN '1'ELSE'0'END) AS TotalTime
                        FROM BDSCriteria_HK --WHERE CriteriaName NOT IN('Batch Preparation','Testing')
                        GROUP BY CriteriaSeqNo,CriteriaName order by CriteriaSeqNo,CriteriaName;

                        --FBAChildPlannings
                        ;SELECT * FROM BDSCriteria_HK order by CriteriaSeqNo, OperationSeqNo, CriteriaName;
                        
                        --Liability Process
				        Select LChildID = 0,BookingChildID = 0,AcknowledgeID = 0,BookingID = 0,UnitID = 0, Cast(a.ValueID as varchar) LiabilitiesProcessID, a.ValueName LiabilitiesName,LiabilityQty=0
				        From {DbNames.EPYSL}..EntityTypeValue a
				        Inner Join {DbNames.EPYSL}..EntityType b on b.EntityTypeID = a.EntityTypeID
				        Where b.EntityTypeName = 'Process Liability'
                        --Liability Process data load
				        Select LChildID = IsNull(F.LChildID,0),BookingChildID = IsNull(F.BookingChildID,0),AcknowledgeID = IsNull(F.AcknowledgeID,0),BookingID = IsNull(F.BookingID,0),UnitID = IsNull(F.UnitID,0), Cast(a.ValueID as varchar) LiabilitiesProcessID, a.ValueName LiabilitiesName,LiabilityQty=IsNull(F.LiabilityQty,0)
				        From {DbNames.EPYSL}..EntityTypeValue a
				        Inner Join {DbNames.EPYSL}..EntityType b on b.EntityTypeID = a.EntityTypeID
				        Left Join (Select LChildID,BookingChildID,AcknowledgeID,BookingID,LiabilitiesProcessID,UnitID,LiabilityQty From FBookingAcknowledgementLiabilityDistribution Where BookingID in ({bookingId}) Group By LChildID,BookingChildID,AcknowledgeID,BookingID,LiabilitiesProcessID,UnitID,LiabilityQty)F On F.LiabilitiesProcessID = a.ValueID
				        Where b.EntityTypeName = 'Process Liability';
                    

                    ;With YBM As
                    (
                        Select * From YarnBookingMaster Where BookingID in ({bookingId})
                    ),
                    YBM_New As
                    (
                        Select * From YarnBookingMaster_New Where BookingID in ({bookingId})
                    ),
                    YBCI As (
                        /*
                        Select ChildID = Row_Number() Over (Order By (Select 0)), YBCI.YItemMasterID As ItemMasterID,YBC.ConsumptionID, YBCI.UnitID, U.DisplayUnitDesc, YBCI.Blending,
                        (Case When Blending = 1 then 'Blend' else 'Non-Blend' End)BlendingName, YBCI.YarnCategory,  BookingQty=Sum(YBCI.BookingQty),
                        ShadeCode= IsNull(YBCI.ShadeCode,''), IsNull(Y.ShadeCode,'') as ShadeName,
                        YBCI.Remarks, YBCI.Specification, YBCI.YD, YDItem='', YBM.BookingID,
                        ISV1.SegmentValue As _segment1ValueDesc, ISV2.SegmentValue As _segment2ValueDesc, ISV3.SegmentValue As _segment3ValueDesc,
                        ISV4.SegmentValue As _segment4ValueDesc, ISV5.SegmentValue As _segment5ValueDesc, ISV6.SegmentValue As _segment6ValueDesc,
                        ISV7.SegmentValue As _segment7ValueDesc, ISV8.SegmentValue As _segment8ValueDesc, YBM.SubGroupID, ISG.SubGroupName,
                        LiabilityQty = IsNull(FBAY.LiabilityQty,0),ReqQty = YBCI.NetYarnReqQty,
                        YBCI.YBChildItemID, AllocatedQty = 0
                        From YBM 
                        Inner Join YarnBookingChild YBC On YBM.YBookingID = YBC.YBookingID
                        Inner Join YarnBookingChildItem YBCI On YBCI.YBChildID = YBC.YBChildID
                        Left Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = YBM.SubGroupID
                        INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YBCI.YItemMasterID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV8 ON ISV8.SegmentValueID = IM.Segment8ValueID
                        LEFT JOIN {DbNames.EPYSL}..Unit U ON U.UnitID = YBCI.UnitID
                        LEFT JOIN YarnShadeBook Y ON Y.ShadeCode = YBCI.ShadeCode
                        LEFT Join YDBookingMaster YDBM ON YDBM.YBookingID = YBM.YBookingID And YDBM.YBookingID = YBCI.YBookingID
                        LEFT Join YDProductionMaster YPM ON YPM.YDBookingMasterID = YDBM.YDBookingMasterID
						Left Join FBookingAcknowledgementYarnLiability FBAY On FBAY.BookingID = YBM.BookingID And FBAY.ConsumptionID = YBC.ConsumptionID And FBAY.ItemMasterID = YBCI.YItemMasterID
						Group By YBCI.YItemMasterID,YBC.ConsumptionID, YBCI.UnitID, U.DisplayUnitDesc, YBCI.Blending,
                        (Case When Blending = 1 then 'Blend' else 'Non-Blend' End), YBCI.YarnCategory, YBCI.NetYarnReqQty,
                        IsNull(YBCI.ShadeCode,''), Y.ShadeCode,
                        YBCI.Remarks, YBCI.Specification, YBCI.YD, YBM.BookingID,
                        ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue,
                        ISV4.SegmentValue, ISV5.SegmentValue, ISV6.SegmentValue,
                        ISV7.SegmentValue, ISV8.SegmentValue, YBM.SubGroupID, ISG.SubGroupName,
                        IsNull(FBAY.LiabilityQty,0)
                        Union
                        */
                        Select ChildID = Row_Number() Over (Order By (Select 0)), YBCI.YItemMasterID As ItemMasterID,YBC.ConsumptionID, YBCI.UnitID, U.DisplayUnitDesc, YBCI.Blending,
                        (Case When Blending = 1 then 'Blend' else 'Non-Blend' End)BlendingName, YBCI.YarnCategory,  BookingQty=Sum(YBCI.BookingQty),
                        ShadeCode= IsNull(YBCI.ShadeCode,''), IsNull(Y.ShadeCode,'') as ShadeName,
                        YBCI.Remarks, YBCI.Specification, YBCI.YD, YDItem='', YBM.BookingID,
                        ISV1.SegmentValue As _segment1ValueDesc, ISV2.SegmentValue As _segment2ValueDesc, ISV3.SegmentValue As _segment3ValueDesc,
                        ISV4.SegmentValue As _segment4ValueDesc, ISV5.SegmentValue As _segment5ValueDesc, ISV6.SegmentValue As _segment6ValueDesc,
                        ISV7.SegmentValue As _segment7ValueDesc, ISV8.SegmentValue As _segment8ValueDesc, YBM.SubGroupID, ISG.SubGroupName,
                        LiabilityQty = IsNull(FBAY.LiabilityQty,0),ReqQty = YBCI.NetYarnReqQty,
                        YBCI.YBChildItemID, Rate = ISNULL(YIP.SourcingRate,0)
                        From YBM_New YBM
                        Inner Join YarnBookingChild_New YBC On YBM.YBookingID = YBC.YBookingID
                        Inner Join YarnBookingChildItem_New YBCI On YBCI.YBChildID = YBC.YBChildID
                        LEFT JOIN YarnItemPrice YIP ON YIP.YBChildItemID = YBCI.YBChildItemID AND ISNULL(YIP.IsTextileERP,0) = 1
                        Left Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = YBM.SubGroupID
                        INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YBCI.YItemMasterID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV8 ON ISV8.SegmentValueID = IM.Segment8ValueID
                        LEFT JOIN {DbNames.EPYSL}..Unit U ON U.UnitID = YBCI.UnitID
                        LEFT JOIN YarnShadeBook Y ON Y.ShadeCode = YBCI.ShadeCode
                        LEFT Join YDBookingMaster YDBM ON YDBM.YBookingID = YBM.YBookingID And YDBM.YBookingID = YBCI.YBookingID
                        LEFT Join YDProductionMaster YPM ON YPM.YDBookingMasterID = YDBM.YDBookingMasterID
						Left Join FBookingAcknowledgementYarnLiability FBAY On FBAY.BookingID = YBM.BookingID And FBAY.ConsumptionID = YBC.ConsumptionID And FBAY.ItemMasterID = YBCI.YItemMasterID
						Group By YBCI.YItemMasterID,YBC.ConsumptionID, YBCI.UnitID, U.DisplayUnitDesc, YBCI.Blending,
                        (Case When Blending = 1 then 'Blend' else 'Non-Blend' End), YBCI.YarnCategory, YBCI.NetYarnReqQty,
                        IsNull(YBCI.ShadeCode,''), Y.ShadeCode,
                        YBCI.Remarks, YBCI.Specification, YBCI.YD, YBM.BookingID,
                        ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue,
                        ISV4.SegmentValue, ISV5.SegmentValue, ISV6.SegmentValue,
                        ISV7.SegmentValue, ISV8.SegmentValue, YBM.SubGroupID, ISG.SubGroupName,
                        IsNull(FBAY.LiabilityQty,0),YBCI.YBChildItemID,ISNULL(YIP.SourcingRate,0)
                    ),
                    Issued AS
                    (
	                    SELECT YACI.AllocationChildItemID, TotalIssueQty = SUM(YSC.Qty)
	                    FROM YBCI
	                    INNER JOIN YarnAllocationChild YAC ON YAC.YBChildItemID = YBCI.YBChildItemID
	                    INNER JOIN YarnAllocationChildItem YACI ON YACI.AllocationChildID = YAC.AllocationChildID
	                    INNER JOIN YarnStockChild YSC ON YSC.StockFromTableId = 6 AND YSC.StockFromPKId = YACI.AllocationChildItemID AND YSC.TransectionTypeId = 2 AND YSC.IsInactive = 0
	                    WHERE YACI.Acknowledge = 1
	                    GROUP BY YACI.AllocationChildItemID
                    ),
                    Allcated AS
                    (
	                    SELECT YBCI.YBChildItemID, YAC.AllocationChildID, TotalIssueQty = ISNULL(I.TotalIssueQty,0), TotalAllocationQty = ISNULL(SUM(YACI.AdvanceAllocationQty + YACI.SampleAllocationQty + YACI.LiabilitiesAllocationQty + YACI.LeftoverAllocationQty),0) - ISNULL(I.TotalIssueQty,0)
	                    FROM YBCI
	                    INNER JOIN YarnAllocationChild YAC ON YAC.YBChildItemID = YBCI.YBChildItemID
	                    INNER JOIN YarnAllocationChildItem YACI ON YACI.AllocationChildID = YAC.AllocationChildID
	                    LEFT JOIN Issued I ON I.AllocationChildItemID = YACI.AllocationChildItemID
	                    INNER JOIN YarnStockMaster YSM ON YSM.YarnStockSetId = YACI.YarnStockSetId
	                    WHERE YACI.Acknowledge = 1
	                    GROUP BY YACI.AllocationChildItemID, YBCI.YBChildItemID, YAC.AllocationChildID,ISNULL(I.TotalIssueQty,0)
                    ),
                    FinalList AS
                    (
	                    SELECT YBCI.*, A.AllocationChildID, AllocatedQty = A.TotalAllocationQty, A.TotalIssueQty
	                    FROM YBCI
	                    LEFT JOIN Allcated A ON A.YBChildItemID = YBCI.YBChildItemID
                    )
                    Select * From FinalList

                    --FBookingAcknowledgementLiabilityDistribution
                    SELECT D.*, LiabilitiesName = ETV.ValueName
                    FROM FBookingAcknowledgementLiabilityDistribution D
                    LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = D.LiabilitiesProcessID
                    WHERE D.BookingID in ({bookingId})

                    --FBookingAcknowledgementYarnLiability
                    SELECT L.*,
                    ISN1.SegmentName _segment1ValueDesc,
                    ISN2.SegmentName _segment2ValueDesc,
                    ISN3.SegmentName _segment3ValueDesc,
                    ISN4.SegmentName _segment4ValueDesc,
                    ISN5.SegmentName _segment5ValueDesc,
                    ISN6.SegmentName _segment6ValueDesc

                    FROM FBookingAcknowledgementYarnLiability L
                    INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = L.ItemMasterID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentName ISN1 ON ISN1.SegmentNameID = ISV1.SegmentNameID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentName ISN2 ON ISN2.SegmentNameID = ISV2.SegmentNameID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentName ISN3 ON ISN3.SegmentNameID = ISV3.SegmentNameID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentName ISN4 ON ISN4.SegmentNameID = ISV4.SegmentNameID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentName ISN5 ON ISN5.SegmentNameID = ISV5.SegmentNameID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentName ISN6 ON ISN6.SegmentNameID = ISV6.SegmentNameID
                    WHERE L.BookingID in ({bookingId})

                    --Process Liability
                    SELECT LiabilitiesProcessID = ETV.ValueID, LiabilitiesName = ETV.ValueName
                    FROM {DbNames.EPYSL}..EntityTypeValue ETV
                    INNER JOIN {DbNames.EPYSL}..EntityType ET ON ET.EntityTypeID = ETV.EntityTypeID
                    WHERE ET.EntityTypeName = 'Process Liability';";

            query += $@"
                         
                DECLARE
                @RevisionNo int = (SELECT TOP(1) RevisionNo FROM {DbNames.EPYSL}..BookingMaster WHERE BookingID IN (Select * from [dbo].[my_string_split]('{bookingId}',','))) - 1
                IF(@RevisionNo IS NULL OR @RevisionNo <- 1)
                BEGIN
	                SET @RevisionNo = (SELECT TOP(1) RevisionNo FROM {DbNames.EPYSL}..SampleBookingMaster WHERE BookingID IN (Select * from [dbo].[my_string_split]('{bookingId}',','))) - 1
                END 


                  ;WITH
                DeletedChildsBB1 AS
                (
	                SELECT BCBK.BookingChildID,BCBK.BookingID,BCBK.ConsumptionChildID,BCBK.ConsumptionID,
	                BCBK.BOMMasterID,BCBK.ExportOrderID,BCBK.ItemGroupID,BCBK.SubGroupID,BCBK.ItemMasterID,
	                BCBK.OrderBankPOID,BCBK.ColorID,BCBK.SizeID,BCBK.TechPackID,BCBK.ConsumptionQty,BookingQty = 0, --BCBK.BookingQty,
	                BCBK.BookingUnitID,BCBK.RequisitionQty,BCBK.ISourcing,BCBK.Remarks,BCBK.LengthYds,BCBK.LengthInch,
	                BCBK.FUPartID,BCBK.A1ValueID,BCBK.YarnBrandID,BCBK.ContactID,BCBK.AddedBy,BCBK.DateAdded,
	                BCBK.UpdatedBy,BCBK.DateUpdated,BCBK.ExecutionCompanyID,BCBK.BlockBookingQty,BCBK.AdjustQty,BCBK.AutoAgree,
	                BCBK.Price,BCBK.SuggestedPrice, Status = 'Child Deleted',

	                ISV1.SegmentValue Segment1Desc,
	                ISV2.SegmentValue Segment2Desc,
	                ISV3.SegmentValue Segment3Desc,
	                ISV4.SegmentValue Segment4Desc,
	                ISV5.SegmentValue Segment5Desc,
	                ISV6.SegmentValue Segment6Desc,
                    ISV7.SegmentValue Segment7Desc,

	                ISV.SegmentValue YarnType,
	                ETV.ValueName YarnProgram,
                    PreviousBookingQty1 = IsNull((Select Sum(BookingQty) From {DbNames.EPYSL}..BookingChild_Bk Where RevisionNo = ISNULL(@RevisionNo,0) And BookingChildID =  BCBK.BookingChildID),0),
                    PreviousBookingQty2 = IsNull((Select Sum(BookingQty) From {DbNames.EPYSL}..BookingChild_Bk Where BookingChildID =  BCBK.BookingChildID),0),
                    RevisionNoWhenDeleted = -1

	                FROM {DbNames.EPYSL}..BookingChild_Bk BCBK
	                LEFT JOIN {DbNames.EPYSL}..BookingChild BC ON BC.ConsumptionID = BCBK.ConsumptionID 
                                                              AND BC.BookingID = BCBK.BookingID 
                                                              AND BC.SubGroupID = BCBK.SubGroupID
                                                              AND ISNULL(BC.ItemMasterID,0) = ISNULL(BCBK.ItemMasterID,0) 

                    LEFT JOIN  {DbNames.EPYSL}..BookingMaster SBM ON SBM.BookingID = BC.BookingID
	                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = BCBK.ItemMasterID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV On ISV.SegmentValueID = BCBK.A1ValueID
	                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = BCBK.YarnBrandID
	                WHERE BCBK.BookingID IN (Select * from [dbo].[my_string_split]('{bookingId}',',')) AND BC.BookingChildID IS NULL
                    AND BCBK.RevisionNo = @RevisionNo
                ),
                DeletedChildsBB AS
                (
                    SELECT *, PreviousBookingQty = CASE WHEN PreviousBookingQty1 = 0 AND PreviousBookingQty2 > 0 THEN PreviousBookingQty2 ELSE PreviousBookingQty1 END
				    FROM DeletedChildsBB1
                ),
                DeletedChildsSB1 AS
                (
	                	SELECT BookingChildID = MAX(FBC.BookingChildID)
	                    ,BCBK.BookingID,ConsumptionChildID=0,BCBK.ConsumptionID,
	                    BOMMasterID=0,SBM.ExportOrderID,BCBK.ItemGroupID,BCBK.SubGroupID,IM.ItemMasterID,
	                    OrderBankPOID=0,ColorID=0,SizeID=0,TechPackID=0,BCBK.ConsumptionQty,BookingQty=0,
	                    BookingUnitID=0,RequisitionQty=0,ISourcing=0,BCBK.Remarks,BCBK.LengthYds,BCBK.LengthInch,
	                    BCBK.FUPartID,BCBK.A1ValueID,BCBK.YarnBrandID,ContactID=0,SBM.AddedBy,SBM.DateAdded,
	                    SBM.UpdatedBy,SBM.DateUpdated,SBM.ExecutionCompanyID,BlockBookingQty=0,AdjustQty=0,BCBK.AutoAgree,
	                    BCBK.Price,BCBK.SuggestedPrice, Status = 'Child Deleted',
	
	                    ISV1.SegmentValue Segment1Desc,
	                    ISV2.SegmentValue Segment2Desc,
	                    ISV3.SegmentValue Segment3Desc,
	                    ISV4.SegmentValue Segment4Desc,
	                    ISV5.SegmentValue Segment5Desc,
	                    ISV6.SegmentValue Segment6Desc,
                        ISV7.SegmentValue Segment7Desc,
	
	                    ISV.SegmentValue YarnType,
	                    ETV.ValueName YarnProgram,
                        PreviousBookingQty1 = IsNull((Select Sum(MM.RequiredQty) From {DbNames.EPYSL}..SampleBookingConsumptionChild_Bk MM Where MM.RevisionNo = ISNULL(@RevisionNo,0) And MM.ConsumptionID =  BCBK.ConsumptionID),0),
                        PreviousBookingQty2 = IsNull((Select Sum(RequiredQty) From {DbNames.EPYSL}..SampleBookingConsumptionChild_Bk Where ConsumptionID =  BCBK.ConsumptionID),0),
                        RevisionNoWhenDeleted = ISNULL(FBC.RevisionNoWhenDeleted,-1)

	                    FROM {DbNames.EPYSL}..SampleBookingConsumption_Bk BCBK
	                    LEFT JOIN {DbNames.EPYSL}..SampleBookingConsumption BC ON BC.ConsumptionID = BCBK.ConsumptionID 
								                    AND BC.BookingID = BCBK.BookingID 
								                    AND BC.SubGroupID = BCBK.SubGroupID 
								                    AND ISNULL(BC.Segment1ValueID,0) = ISNULL(BCBK.Segment1ValueID,0)
								                    AND ISNULL(BC.Segment2ValueID,0) = ISNULL(BCBK.Segment2ValueID,0) 
								                    AND ISNULL(BC.Segment3ValueID,0) = ISNULL(BCBK.Segment3ValueID,0) 
								                    AND ISNULL(BC.Segment4ValueID,0) = ISNULL(BCBK.Segment4ValueID,0) 
								                    AND ISNULL(BC.Segment5ValueID,0) = ISNULL(BCBK.Segment5ValueID,0) 
								                    AND ISNULL(BC.Segment6ValueID,0) = ISNULL(BCBK.Segment6ValueID,0) 
								                    AND ISNULL(BC.Segment7ValueID,0) = ISNULL(BCBK.Segment7ValueID,0) 
	
	                    LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster SBM ON SBM.BookingID = BC.BookingID
                        LEFT JOIN FBookingAcknowledgeChild FBC ON FBC.ConsumptionID = BCBK.ConsumptionID AND FBC.BookingID = BCBK.BookingID AND FBC.SubGroupID = BCBK.SubGroupID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = BCBK.Segment1ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = BCBK.Segment2ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = BCBK.Segment3ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = BCBK.Segment4ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = BCBK.Segment5ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = BCBK.Segment6ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = BCBK.Segment7ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV On ISV.SegmentValueID = BCBK.A1ValueID
	                    LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = BCBK.YarnBrandID
	                    LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON ISNULL(IM.Segment1ValueID,0) = ISNULL(BCBK.Segment1ValueID,0) 
									                    AND ISNULL(IM.Segment2ValueID,0) = ISNULL(BCBK.Segment2ValueID,0)
									                    AND ISNULL(IM.Segment3ValueID,0) = ISNULL(BCBK.Segment3ValueID,0)
									                    AND ISNULL(IM.Segment4ValueID,0) = ISNULL(BCBK.Segment4ValueID,0)
									                    AND ISNULL(IM.Segment5ValueID,0) = ISNULL(BCBK.Segment5ValueID,0)
									                    AND ISNULL(IM.Segment6ValueID,0) = ISNULL(BCBK.Segment6ValueID,0)
									                    AND ISNULL(IM.Segment7ValueID,0) = ISNULL(BCBK.Segment7ValueID,0)
	                    WHERE BCBK.BookingID IN (Select * from [dbo].[my_string_split]('{bookingId}',',')) AND BC.ConsumptionID IS NULL
                        AND BCBK.RevisionNo = @RevisionNo
	                    GROUP BY BCBK.BookingID,BCBK.ConsumptionID,
	                    SBM.ExportOrderID,BCBK.ItemGroupID,BCBK.SubGroupID,IM.ItemMasterID,
	                    BCBK.ConsumptionQty,
	                    BCBK.Remarks,BCBK.LengthYds,BCBK.LengthInch,
	                    BCBK.FUPartID,BCBK.A1ValueID,BCBK.YarnBrandID,SBM.AddedBy,SBM.DateAdded,
	                    SBM.UpdatedBy,SBM.DateUpdated,SBM.ExecutionCompanyID,BCBK.AutoAgree,
	                    BCBK.Price,BCBK.SuggestedPrice,
	
	                    ISV1.SegmentValue,
	                    ISV2.SegmentValue,
	                    ISV3.SegmentValue,
	                    ISV4.SegmentValue,
	                    ISV5.SegmentValue,
	                    ISV6.SegmentValue,
                        ISV7.SegmentValue,
	
	                    ISV.SegmentValue,
	                    ETV.ValueName,
                        ISNULL(FBC.RevisionNoWhenDeleted,-1)
                ),
                DeletedChildsSB AS
                (
                    SELECT *, PreviousBookingQty = CASE WHEN PreviousBookingQty1 = 0 AND PreviousBookingQty2 > 0 THEN PreviousBookingQty2 ELSE PreviousBookingQty1 END
				    FROM DeletedChildsSB1
                ),
                NewChildsBB AS
                (
	                SELECT BC.BookingChildID,BC.BookingID,BC.ConsumptionChildID,BC.ConsumptionID,
	                BC.BOMMasterID,BC.ExportOrderID,BC.ItemGroupID,BC.SubGroupID,BC.ItemMasterID,
	                BC.OrderBankPOID,BC.ColorID,BC.SizeID,BC.TechPackID,BC.ConsumptionQty,BC.BookingQty,
	                BC.BookingUnitID,BC.RequisitionQty,BC.ISourcing,BC.Remarks,BC.LengthYds,BC.LengthInch,
	                BC.FUPartID,BC.A1ValueID,BC.YarnBrandID,BC.ContactID,BC.AddedBy,BC.DateAdded,
	                BC.UpdatedBy,BC.DateUpdated,BC.ExecutionCompanyID,BC.BlockBookingQty,BC.AdjustQty,BC.AutoAgree,
	                BC.Price,BC.SuggestedPrice, Status = 'New Child',

	                ISV1.SegmentValue Segment1Desc,
	                ISV2.SegmentValue Segment2Desc,
	                ISV3.SegmentValue Segment3Desc,
	                ISV4.SegmentValue Segment4Desc,
	                ISV5.SegmentValue Segment5Desc,
	                ISV6.SegmentValue Segment6Desc,
                    ISV7.SegmentValue Segment7Desc,

	                ISV.SegmentValue YarnType,
	                ETV.ValueName YarnProgram,
					PreviousBookingQty1 = 0,
					PreviousBookingQty2 = 0,
                    PreviousBookingQty = IsNull((Select Sum(BookingQty) From {DbNames.EPYSL}..BookingChild_Bk Where RevisionNo = ISNULL(@RevisionNo,0)-1 And ConsumptionID =  BC.ConsumptionID),0),
                    RevisionNoWhenDeleted = -1

	                FROM {DbNames.EPYSL}..BookingChild BC
	                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = BC.ItemMasterID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV On ISV.SegmentValueID = BC.A1ValueID
	                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = BC.YarnBrandID
	                WHERE BC.BookingID IN (Select * from [dbo].[my_string_split]('{bookingId}',',')) 
	                AND BC.ConsumptionID NOT IN 
	                (SELECT BCBK.ConsumptionID FROM {DbNames.EPYSL}..BookingChild_Bk BCBK 
	                WHERE BCBK.BookingID IN (Select * from [dbo].[my_string_split]('{bookingId}',',')) AND BCBK.RevisionNo = @RevisionNo)
                ),
                NewChildsSB AS
                (
	                SELECT BookingChildID=0,BC.BookingID,ConsumptionChildID=0,BC.ConsumptionID,
	                BOMMasterID=0,SBM.ExportOrderID,BC.ItemGroupID,BC.SubGroupID,IM.ItemMasterID,
	                OrderBankPOID=0,ColorID=0,SizeID=0,TechPackID=0,BC.ConsumptionQty,BookingQty=BC.RequiredQty,
	                BookingUnitID=0,RequisitionQty=0,ISourcing=0,BC.Remarks,BC.LengthYds,BC.LengthInch,
	                BC.FUPartID,BC.A1ValueID,BC.YarnBrandID,ContactID=0,SBM.AddedBy,SBM.DateAdded,
	                SBM.UpdatedBy,SBM.DateUpdated,SBM.ExecutionCompanyID,BlockBookingQty=0,AdjustQty=0,BC.AutoAgree,
	                BC.Price,BC.SuggestedPrice, Status = 'New Child',

	                ISV1.SegmentValue Segment1Desc,
	                ISV2.SegmentValue Segment2Desc,
	                ISV3.SegmentValue Segment3Desc,
	                ISV4.SegmentValue Segment4Desc,
	                ISV5.SegmentValue Segment5Desc,
	                ISV6.SegmentValue Segment6Desc,
                    ISV7.SegmentValue Segment7Desc,

	                ISV.SegmentValue YarnType,
	                ETV.ValueName YarnProgram,
					PreviousBookingQty1 = 0,
					PreviousBookingQty2 = 0,
                    PreviousBookingQty = IsNull((Select Sum(MM.RequiredQty) From {DbNames.EPYSL}..SampleBookingConsumptionChild_Bk MM Where MM.RevisionNo = ISNULL(@RevisionNo,0)-1 And MM.ConsumptionID =  BC.ConsumptionID),0),
                    RevisionNoWhenDeleted = -1


	                FROM {DbNames.EPYSL}..SampleBookingConsumption BC
	                LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster SBM ON SBM.BookingID = BC.BookingID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = BC.Segment1ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = BC.Segment2ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = BC.Segment3ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = BC.Segment4ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = BC.Segment5ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = BC.Segment6ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = BC.Segment7ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV On ISV.SegmentValueID = BC.A1ValueID
	                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = BC.YarnBrandID
	                LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON ISNULL(IM.Segment1ValueID,0) = ISNULL(BC.Segment1ValueID,0) 
									                AND ISNULL(IM.Segment2ValueID,0) = ISNULL(BC.Segment2ValueID,0)
									                AND ISNULL(IM.Segment3ValueID,0) = ISNULL(BC.Segment3ValueID,0)
									                AND ISNULL(IM.Segment4ValueID,0) = ISNULL(BC.Segment4ValueID,0)
									                AND ISNULL(IM.Segment5ValueID,0) = ISNULL(BC.Segment5ValueID,0)
									                AND ISNULL(IM.Segment6ValueID,0) = ISNULL(BC.Segment6ValueID,0)
									                AND ISNULL(IM.Segment7ValueID,0) = ISNULL(BC.Segment7ValueID,0)
	                WHERE BC.BookingID IN (Select * from [dbo].[my_string_split]('{bookingId}',',')) 
	                AND BC.ConsumptionID NOT IN 
	                (SELECT BCBK.ConsumptionID FROM {DbNames.EPYSL}..SampleBookingConsumption_Bk BCBK 
	                WHERE BCBK.BookingID IN (Select * from [dbo].[my_string_split]('{bookingId}',',')) AND BCBK.RevisionNo = @RevisionNo)
                ),
                QtyChangedChildsBB AS
                (
	                SELECT BCBK.BookingChildID,BCBK.BookingID,BCBK.ConsumptionChildID,BCBK.ConsumptionID,
	                BCBK.BOMMasterID,BCBK.ExportOrderID,BCBK.ItemGroupID,BCBK.SubGroupID,BCBK.ItemMasterID,
	                BCBK.OrderBankPOID,BCBK.ColorID,BCBK.SizeID,BCBK.TechPackID,BCBK.ConsumptionQty,BCBK.BookingQty,
	                BCBK.BookingUnitID,BCBK.RequisitionQty,BCBK.ISourcing,BCBK.Remarks,BCBK.LengthYds,BCBK.LengthInch,
	                BCBK.FUPartID,BCBK.A1ValueID,BCBK.YarnBrandID,BCBK.ContactID,BCBK.AddedBy,BCBK.DateAdded,
	                BCBK.UpdatedBy,BCBK.DateUpdated,BCBK.ExecutionCompanyID,BCBK.BlockBookingQty,BCBK.AdjustQty,BCBK.AutoAgree,
	                BCBK.Price,BCBK.SuggestedPrice, Status = 'Qty Changed',

	                ISV1.SegmentValue Segment1Desc,
	                ISV2.SegmentValue Segment2Desc,
	                ISV3.SegmentValue Segment3Desc,
	                ISV4.SegmentValue Segment4Desc,
	                ISV5.SegmentValue Segment5Desc,
	                ISV6.SegmentValue Segment6Desc,
                    ISV7.SegmentValue Segment7Desc,

	                ISV.SegmentValue YarnType,
	                ETV.ValueName YarnProgram,
					PreviousBookingQty1 = 0,
					PreviousBookingQty2 = 0,
                    PreviousBookingQty = IsNull((Select Sum(BookingQty) From {DbNames.EPYSL}..BookingChild_Bk Where RevisionNo = ISNULL(@RevisionNo,0)-1 And BookingChildID =  BCBK.BookingChildID),0),
                    RevisionNoWhenDeleted = -1

	                FROM {DbNames.EPYSL}..BookingChild_Bk BCBK
	                INNER JOIN {DbNames.EPYSL}..BookingChild BC ON BC.BookingChildID = BCBK.BookingChildID AND BC.ItemMasterID = BCBK.ItemMasterID
	                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = BCBK.ItemMasterID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV On ISV.SegmentValueID = BCBK.A1ValueID
	                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = BCBK.YarnBrandID
	                WHERE BCBK.BookingID IN (Select * from [dbo].[my_string_split]('{bookingId}',',')) AND BCBK.RevisionNo = @RevisionNo AND BCBK.BookingQty <> BC.BookingQty
                ),
                QtyChangedChildsSB AS
                (
	                SELECT BookingChildID=0,BC.BookingID,ConsumptionChildID=0,BC.ConsumptionID,
	                BOMMasterID=0,SBM.ExportOrderID,BC.ItemGroupID,BC.SubGroupID,IM.ItemMasterID,
	                OrderBankPOID=0,ColorID=0,SizeID=0,TechPackID=0,BC.ConsumptionQty,BookingQty=BC.RequiredQty,
	                BookingUnitID=0,RequisitionQty=0,ISourcing=0,BC.Remarks,BC.LengthYds,BC.LengthInch,
	                BC.FUPartID,BC.A1ValueID,BC.YarnBrandID,ContactID=0,SBM.AddedBy,SBM.DateAdded,
	                SBM.UpdatedBy,SBM.DateUpdated,SBM.ExecutionCompanyID,BlockBookingQty=0,AdjustQty=0,BC.AutoAgree,
	                BC.Price,BC.SuggestedPrice, Status = 'Qty Changed',

	                ISV1.SegmentValue Segment1Desc,
	                ISV2.SegmentValue Segment2Desc,
	                ISV3.SegmentValue Segment3Desc,
	                ISV4.SegmentValue Segment4Desc,
	                ISV5.SegmentValue Segment5Desc,
	                ISV6.SegmentValue Segment6Desc,
                    ISV7.SegmentValue Segment7Desc,

	                ISV.SegmentValue YarnType,
	                ETV.ValueName YarnProgram,
					PreviousBookingQty1 = 0,
					PreviousBookingQty2 = 0,
                    PreviousBookingQty = IsNull((Select Sum(MM.RequiredQty) From {DbNames.EPYSL}..SampleBookingConsumptionChild_Bk MM Where MM.RevisionNo = ISNULL(@RevisionNo,0)-1 And MM.ConsumptionID =  BCBK.ConsumptionID),0),
                    RevisionNoWhenDeleted = -1


	                FROM {DbNames.EPYSL}..SampleBookingConsumption_Bk BCBK
	                INNER JOIN {DbNames.EPYSL}..SampleBookingConsumption BC ON BC.ConsumptionID = BCBK.ConsumptionID
	                LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster SBM ON SBM.BookingID = BC.BookingID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = BC.Segment1ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = BC.Segment2ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = BC.Segment3ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = BC.Segment4ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = BC.Segment5ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = BC.Segment6ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = BC.Segment7ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV On ISV.SegmentValueID = BCBK.A1ValueID
	                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = BCBK.YarnBrandID
	                LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON ISNULL(IM.Segment1ValueID,0) = ISNULL(BC.Segment1ValueID,0) 
									                AND ISNULL(IM.Segment2ValueID,0) = ISNULL(BC.Segment2ValueID,0)
									                AND ISNULL(IM.Segment3ValueID,0) = ISNULL(BC.Segment3ValueID,0)
									                AND ISNULL(IM.Segment4ValueID,0) = ISNULL(BC.Segment4ValueID,0)
									                AND ISNULL(IM.Segment5ValueID,0) = ISNULL(BC.Segment5ValueID,0)
									                AND ISNULL(IM.Segment6ValueID,0) = ISNULL(BC.Segment6ValueID,0)
									                AND ISNULL(IM.Segment7ValueID,0) = ISNULL(BC.Segment7ValueID,0)
	                WHERE BCBK.BookingID IN (Select * from [dbo].[my_string_split]('{bookingId}',',')) AND BCBK.RevisionNo = @RevisionNo AND BCBK.ConsumptionQty <> BC.ConsumptionQty
                ),
                FinalList AS
                (
	                SELECT * FROM DeletedChildsBB
	                UNION
	                SELECT * FROM NewChildsBB
	                UNION
	                SELECT * FROM QtyChangedChildsBB
	                UNION
	                SELECT * FROM DeletedChildsSB
	                UNION
	                SELECT * FROM NewChildsSB
	                UNION
	                SELECT * FROM QtyChangedChildsSB
                )
                SELECT * FROM FinalList ORDER BY Segment3Desc, Segment4Desc, Segment5Desc, Segment1Desc, Segment2Desc";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                List<FabricBookingAcknowledge> fbaList = records.Read<FabricBookingAcknowledge>().ToList();
                FBookingAcknowledge data = records.Read<FBookingAcknowledge>().FirstOrDefault();
                data.IsSample = fbaList.Count() > 0 ? fbaList.First().IsSample : false;
                string unAcknowledgeReason = fbaList.Count() > 0 ? fbaList.FirstOrDefault().UnAcknowledgeReason : "";
                data.FBookingAcknowledgeList = records.Read<FBookingAcknowledge>().ToList();
                Guard.Against.NullObject(data);

                List<FBookingAcknowledgeChild> bookingChilds = records.Read<FBookingAcknowledgeChild>().ToList();
                List<FBookingAcknowledgeChild> bookingChildsCollarCuff = records.Read<FBookingAcknowledgeChild>().ToList();


                bookingChilds = bookingChilds.Where(x => x.IsDeletedItem == false).ToList();
                bookingChildsCollarCuff = bookingChildsCollarCuff.Where(x => x.IsDeletedItem == false).ToList();

                data.FabricBookingAcknowledgeList = fbaList;
                data.FBookingAcknowledgeChildAddProcess = records.Read<FBookingAcknowledgeChildAddProcess>().ToList();
                data.FBookingChildDetails = records.Read<FBookingAcknowledgeChildDetails>().ToList();
                data.FBookingAcknowledgeChildGarmentPart = records.Read<FBookingAcknowledgeChildGarmentPart>().ToList();
                data.FBookingAcknowledgeChildProcess = records.Read<FBookingAcknowledgeChildProcess>().ToList();
                data.FBookingAcknowledgeChildText = records.Read<FBookingAcknowledgeChildText>().ToList();
                data.FBookingAcknowledgeChildDistribution = records.Read<FBookingAcknowledgeChildDistribution>().ToList();
                data.FBookingAcknowledgeChildYarnSubBrand = records.Read<FBookingAcknowledgeChildYarnSubBrand>().ToList();
                data.FBookingAcknowledgeImage = records.Read<FBookingAcknowledgeImage>().ToList();
                data.FreeConcepts = records.Read<FreeConceptMaster>().ToList();
                data.TechnicalNameList = await records.ReadAsync<Select2OptionModel>();
                data.YarnSourceNameList = await records.ReadAsync<Select2OptionModel>();

                List<Select2OptionModel> mcTypeList = records.Read<Select2OptionModel>().ToList();
                data.MCTypeForFabricList = mcTypeList.Where(x => x.additionalValue != "Flat Bed");
                data.MCTypeForOtherList = mcTypeList.Where(x => x.additionalValue == "Flat Bed");

                List<FBookingAcknowledgeChild> criteriaNames = records.Read<FBookingAcknowledgeChild>().ToList();
                List<FBAChildPlanning> fbaChildPlannings = records.Read<FBAChildPlanning>().ToList();

                List<FBookingAcknowledgementLiabilityDistribution> LiabilityDistributionList = records.Read<FBookingAcknowledgementLiabilityDistribution>().ToList();
                List<FBookingAcknowledgementLiabilityDistribution> FBookingAckLiabilityDistributionList = records.Read<FBookingAcknowledgementLiabilityDistribution>().ToList();
                data.FBookingAcknowledgementYarnLiabilityList = records.Read<FBookingAcknowledgementYarnLiability>().ToList();

                List<FBookingAcknowledgementLiabilityDistribution> distributionList = records.Read<FBookingAcknowledgementLiabilityDistribution>().ToList();
                List<FBookingAcknowledgementYarnLiability> yarnLiabilityList = records.Read<FBookingAcknowledgementYarnLiability>().ToList();

                List<FBookingAcknowledgementLiabilityDistribution> defaultProcessLiabilities = records.Read<FBookingAcknowledgementLiabilityDistribution>().ToList();

                data.ChangesChilds = records.Read<FBookingAcknowledgeChild>().ToList();

                #region only show immediate previous revision's deleted items
                if (data.RevisionNo > 0)
                {
                    data.ChangesChilds = data.ChangesChilds.Where(x => x.BookingQty == 0 && x.RevisionNoWhenDeleted == -1).ToList();
                }
                #endregion

                data.FBookingChild = bookingChilds.Where(x => x.SubGroupID == 1).ToList();
                data.FBookingChildCollor = bookingChildsCollarCuff.Where(x => x.SubGroupID == 11).ToList();
                data.FBookingChildCuff = bookingChildsCollarCuff.Where(x => x.SubGroupID == 12).ToList();

                int childId = 1;
                data.FBookingChild.ForEach(x =>
                {
                    x.ChildAckLiabilityDetails = new List<FBookingAcknowledgementLiabilityDistribution>();
                    x.LiabilitiesBookingQty = 0;
                    if (x.BookingQty - x.PreviousBookingQty < 0)
                    {
                        defaultProcessLiabilities.ForEach(d =>
                        {
                            decimal qtyDiff = x.BookingQty - x.PreviousBookingQty;

                            decimal differentQty = Math.Abs(CommonFunction.DeepClone(qtyDiff));
                            decimal liabilityQty = CommonFunction.DeepClone(differentQty > x.TotalFinishFabricStockQty ? x.TotalFinishFabricStockQty : differentQty);

                            var dObj = distributionList.Find(y => y.BookingChildID == x.BookingChildID && y.BookingID == x.BookingID && y.LiabilitiesProcessID == d.LiabilitiesProcessID);
                            if (dObj.IsNotNull())
                            {
                                if (d.LiabilitiesName == "Finished Qty")
                                {
                                    dObj.LiabilityQty = CommonFunction.DeepClone(this.GetFinishedQtyLiability(x.TotalFinishFabricStockQty, x.DeliveredQtyForLiability, x.BookingQty, x.PreviousBookingQty)); //CommonFunction.DeepClone(liabilityQty);
                                    dObj.MaxFinishQtyLiability = dObj.LiabilityQty;
                                }
                                x.ChildAckLiabilityDetails.Add(CommonFunction.DeepClone(dObj));
                            }
                            else
                            {
                                if (d.LiabilitiesName == "Finished Qty")
                                {
                                    d.LiabilityQty = CommonFunction.DeepClone(this.GetFinishedQtyLiability(x.TotalFinishFabricStockQty, x.DeliveredQtyForLiability, x.BookingQty, x.PreviousBookingQty)); //CommonFunction.DeepClone(liabilityQty);
                                    d.MaxFinishQtyLiability = d.LiabilityQty;
                                }
                                x.ChildAckLiabilityDetails.Add(CommonFunction.DeepClone(d));
                            }
                        });
                        x.LiabilitiesBookingQty = x.ChildAckLiabilityDetails.Sum(y => y.LiabilityQty);

                        x.ChildAckYarnLiabilityDetails = yarnLiabilityList.Where(y => y.BookingChildID == x.BookingChildID && y.BookingID == x.BookingID).ToList();
                        if (x.ChildAckYarnLiabilityDetails.Count() == 0)
                        {
                            x.ChildAckYarnLiabilityDetails = data.FBookingAcknowledgementYarnLiabilityList.Where(y => y.ConsumptionID == x.ConsumptionID && y.BookingID == x.BookingID).ToList();
                            x.ChildAckYarnLiabilityDetails.ForEach(y =>
                            {
                                y.ChildID = childId++;
                            });
                        }
                    }
                });
                data.FBookingChildCollor.ForEach(x =>
                {
                    x.ChildAckLiabilityDetails = new List<FBookingAcknowledgementLiabilityDistribution>();
                    x.LiabilitiesBookingQty = 0;
                    if (x.BookingQty - x.PreviousBookingQty < 0)
                    {
                        defaultProcessLiabilities.ForEach(d =>
                        {
                            decimal qtyDiff = x.BookingQty - x.PreviousBookingQty;

                            decimal differentQty = Math.Abs(CommonFunction.DeepClone(qtyDiff));
                            decimal liabilityQty = CommonFunction.DeepClone(differentQty > x.TotalFinishFabricStockQty ? x.TotalFinishFabricStockQty : differentQty);

                            var dObj = distributionList.Find(y => y.BookingChildID == x.BookingChildID && y.BookingID == x.BookingID && y.LiabilitiesProcessID == d.LiabilitiesProcessID);
                            if (dObj.IsNotNull())
                            {
                                if (d.LiabilitiesName == "Finished Qty")
                                {
                                    dObj.LiabilityQty = CommonFunction.DeepClone(this.GetFinishedQtyLiability(x.TotalFinishFabricStockQty, x.DeliveredQtyForLiability, x.BookingQty, x.PreviousBookingQty)); //CommonFunction.DeepClone(liabilityQty);
                                    dObj.MaxFinishQtyLiability = dObj.LiabilityQty;
                                }
                                x.ChildAckLiabilityDetails.Add(CommonFunction.DeepClone(dObj));
                            }
                            else
                            {
                                if (d.LiabilitiesName == "Finished Qty")
                                {
                                    d.LiabilityQty = CommonFunction.DeepClone(this.GetFinishedQtyLiability(x.TotalFinishFabricStockQty, x.DeliveredQtyForLiability, x.BookingQty, x.PreviousBookingQty)); //CommonFunction.DeepClone(liabilityQty);
                                    d.MaxFinishQtyLiability = d.LiabilityQty;
                                }
                                x.ChildAckLiabilityDetails.Add(CommonFunction.DeepClone(d));
                            }
                        });
                        x.LiabilitiesBookingQty = x.ChildAckLiabilityDetails.Sum(y => y.LiabilityQty);

                        x.ChildAckYarnLiabilityDetails = yarnLiabilityList.Where(y => y.ConsumptionID == x.ConsumptionID && y.BookingID == x.BookingID).ToList();
                        if (x.ChildAckYarnLiabilityDetails.Count() == 0)
                        {
                            x.ChildAckYarnLiabilityDetails = data.FBookingAcknowledgementYarnLiabilityList.Where(y => y.ConsumptionID == x.ConsumptionID && y.BookingID == x.BookingID).ToList();
                            x.ChildAckYarnLiabilityDetails.ForEach(y =>
                            {
                                y.ChildID = childId++;
                            });
                        }
                    }
                });
                data.FBookingChildCuff.ForEach(x =>
                {
                    if (x.BookingQty - x.PreviousBookingQty < 0)
                    {
                        x.ChildAckLiabilityDetails = new List<FBookingAcknowledgementLiabilityDistribution>();
                        x.LiabilitiesBookingQty = 0;
                        defaultProcessLiabilities.ForEach(d =>
                        {
                            var DifferentQty = Math.Abs(CommonFunction.DeepClone(x.BookingQty - x.PreviousBookingQty));
                            var LiabilityQty = CommonFunction.DeepClone(DifferentQty > x.TotalFinishFabricStockQty ? x.TotalFinishFabricStockQty : DifferentQty);

                            var dObj = distributionList.Find(y => y.BookingChildID == x.BookingChildID && y.BookingID == x.BookingID && y.LiabilitiesProcessID == d.LiabilitiesProcessID);
                            if (dObj.IsNotNull())
                            {
                                if (d.LiabilitiesName == "Finished Qty")
                                {
                                    dObj.LiabilityQty = CommonFunction.DeepClone(LiabilityQty);
                                }
                                x.ChildAckLiabilityDetails.Add(CommonFunction.DeepClone(dObj));
                            }
                            else
                            {
                                if (d.LiabilitiesName == "Finished Qty")
                                {
                                    d.LiabilityQty = CommonFunction.DeepClone(LiabilityQty);
                                }
                                x.ChildAckLiabilityDetails.Add(CommonFunction.DeepClone(d));
                            }
                        });
                        x.LiabilitiesBookingQty = x.ChildAckLiabilityDetails.Sum(y => y.LiabilityQty);

                        x.ChildAckYarnLiabilityDetails = yarnLiabilityList.Where(y => y.ConsumptionID == x.ConsumptionID && y.BookingID == x.BookingID).ToList();
                        if (x.ChildAckYarnLiabilityDetails.Count() == 0)
                        {
                            x.ChildAckYarnLiabilityDetails = data.FBookingAcknowledgementYarnLiabilityList.Where(y => y.ConsumptionID == x.ConsumptionID && y.BookingID == x.BookingID).ToList();
                            x.ChildAckYarnLiabilityDetails.ForEach(y =>
                            {
                                y.ChildID = childId++;
                            });
                        }
                    }
                });

                data.ChangesChilds.ForEach(x =>
                {
                    x.ChildAckLiabilityDetails = new List<FBookingAcknowledgementLiabilityDistribution>();
                    defaultProcessLiabilities.ForEach(d =>
                    {
                        var dObj = distributionList.Find(y => y.BookingChildID == x.BookingChildID && y.BookingID == x.BookingID && y.LiabilitiesProcessID == d.LiabilitiesProcessID);
                        if (dObj.IsNotNull()) x.ChildAckLiabilityDetails.Add(dObj);
                        else x.ChildAckLiabilityDetails.Add(d);
                    });

                    x.ChildAckYarnLiabilityDetails = yarnLiabilityList.Where(y => y.ConsumptionID == x.ConsumptionID && y.BookingID == x.BookingID).ToList();
                    if (x.ChildAckYarnLiabilityDetails.Count() == 0)
                    {
                        x.ChildAckYarnLiabilityDetails = data.FBookingAcknowledgementYarnLiabilityList.Where(y => y.ConsumptionID == x.ConsumptionID && y.BookingID == x.BookingID).ToList();
                        x.ChildAckYarnLiabilityDetails.ForEach(y =>
                        {
                            y.ChildID = childId++;
                        });
                    }
                });

                data.HasFabric = data.FBookingChild.Count() > 0 ? true : false;
                data.HasCollar = data.FBookingChildCollor.Count() > 0 ? true : false;
                data.HasCuff = data.FBookingChildCuff.Count() > 0 ? true : false;
                data.UnAcknowledgeReason = unAcknowledgeReason;

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
        public async Task<FBookingAcknowledge> GetDataAsync(int fbAckId)
        {
            var query =
                $@"-- Master Data
                WITH M AS (
                    SELECT FBA.FBAckID,	FBA.BookingID,FBA.BookingNo,FBA.BookingDate,FBA.SLNo,FBA.StyleMasterID,FBA.StyleNo,FBA.SubGroupID,
	                FBA.BuyerID,FBA.BuyerTeamID,FBA.SupplierID,FBA.ExportOrderID,FBA.ExecutionCompanyID,
					OrderQty = CASE WHEN FBA.WithoutOB=1 THEN SBM.OrderQty ELSE BM.RePurchaseQty END,
					Remarks = CASE WHEN FBA.WithoutOB=1 THEN SBM.Remarks ELSE BM.Remarks END,
					SeasonID = CASE WHEN FBA.WithoutOB=1 THEN SBM.SeasonID ELSE 0 END,
					BookingBy = CASE WHEN FBA.WithoutOB=1 THEN SBM.AddedBy ELSE BM.AddedBy END

                    FROM FBookingAcknowledge FBA
	                LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster SBM ON SBM.BookingID = FBA.BookingID
					LEFT JOIN {DbNames.EPYSL}..BookingMaster BM ON BM.BookingID = FBA.BookingID
                    WHERE FBA.FBAckID={fbAckId}
                )
                SELECT M.FBAckID, M.BookingID,M.BookingNo,M.BookingDate,M.SLNo,M.StyleMasterID,M.StyleNo,M.OrderQty BookingQty,M.BuyerID,M.BuyerTeamID,
                M.SupplierID,M.ExportOrderID,M.ExecutionCompanyID, CTO.ShortName BuyerName, CCT.TeamName BuyerTeamName,C.CompanyName,M.SubGroupID,M.Remarks,
                Supplier.ShortName [SupplierName],Season.SeasonName, M.BookingBy, ISNULL(Supplier.MappingCompanyID,0) TextileCompanyID
                FROM M
                LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = M.BuyerID
                LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = M.BuyerTeamID
                LEFT JOIN {DbNames.EPYSL}..CompanyEntity C ON C.CompanyID = M.ExecutionCompanyID
                LEFT JOIN {DbNames.EPYSL}..Contacts Supplier On M.SupplierID = Supplier.ContactID
                LEFT JOIN {DbNames.EPYSL}..ContactSeason Season On M.SeasonID = Season.SeasonID;

                -- Booking Acknowledge Child (Fabric)
                SELECT BAC.BookingChildID, BAC.AcknowledgeID, BAC.BookingID, BAC.ConsumptionChildID, BAC.ConsumptionID, BAC.BOMMasterID,
                BAC.ExportOrderID, BAC.ItemGroupID, BAC.SubGroupID, BAC.ItemMasterID, BAC.OrderBankPOID, BAC.ColorID, BAC.SizeID, BAC.TechPackID,
                BAC.ConsumptionQty, BAC.BookingQty, FCM.TotalQty, BAC.BookingUnitID, BAC.RequisitionQty, BAC.ISourcing, BAC.Remarks, BAC.LengthYds, BAC.LengthInch,
                BAC.FUPartID, BAC.A1ValueID, BAC.YarnBrandID, BAC.ContactID, BAC.LabDipNo, BAC.AddedBy, BAC.DateAdded, BAC.UpdatedBy, BAC.DateUpdated,
                BAC.ExecutionCompanyID, BAC.BlockBookingQty, BAC.AdjustQty, BAC.AutoAgree, BAC.Price, BAC.SuggestedPrice, BAC.LabdipUpdateDate,
                BAC.IsCompleteReceive, BAC.IsCompleteDelivery, BAC.LastDCDate, BAC.ClosingRemarks, BAC.ToItemMasterID, BAC.TechnicalNameID,
                BAC.MachineTypeId, BAC.IsSubContact, BAC.TotalDays, BAC.DeliveryDate, BAC.BrandID, BAC.MachineGauge,BAC.MachineDia, IM.Segment1ValueID ConstructionId, IM.Segment2ValueID CompositionId,
                IM.Segment3ValueID ColorId,IM.Segment7ValueID KnittingTypeId,IM.Segment4ValueID GSMId, ISG.SubGroupName,
                ISV.SegmentValue YarnType, ETV.ValueName YarnProgram, ETV2.ValueName Brand,

				BookingNo=CASE WHEN BA.WithoutOB = 0 THEN SBM.BookingNo ELSE BM.BookingNo END,
				ContactID=CASE WHEN BA.WithoutOB = 0 THEN SBM.BuyerID ELSE BM.BuyerID END,
				ExecutionCompanyID=CASE WHEN BA.WithoutOB = 0 THEN SBM.ExecutionCompanyID ELSE BM.CompanyID END,

                Construction=CASE WHEN BA.WithoutOB = 0 THEN SBC.Segment1Desc ELSE BCCC.Segment1Desc END,
				Composition=CASE WHEN BA.WithoutOB = 0 THEN SBC.Segment2Desc ELSE BCCC.Segment2Desc END,
				Color=CASE WHEN BA.WithoutOB = 0 THEN SBC.Segment3Desc ELSE BCCC.Segment3Desc END,
				GSM=CASE WHEN BA.WithoutOB = 0 THEN SBC.Segment4Desc ELSE BCCC.Segment4Desc END,
				FabricWidth=CASE WHEN BA.WithoutOB = 0 THEN SBC.Segment5Desc ELSE BCCC.Segment5Desc END,
				KnittingType=CASE WHEN BA.WithoutOB = 0 THEN SBC.Segment7Desc ELSE BCCC.Segment7Desc END,
				DyeingType=CASE WHEN BA.WithoutOB = 0 THEN SBC.Segment6Desc ELSE BCCC.Segment6Desc END,
				Instruction=CASE WHEN BA.WithoutOB = 0 THEN SBC.Remarks ELSE BCC.Remarks END,
				ForBDSStyleNo=CASE WHEN BA.WithoutOB = 0 THEN SBC.ForBDSStyleNo ELSE '' END,
				T.TechnicalName, KMS.SubClassName MachineType,
                BAC.TestReportDays,BAC.FinishingDays,BAC.DyeingDays,BAC.BatchPreparationDays,BAC.KnittingDays,BAC.MaterialDays,
                BAC.StructureDays, ISNULL(Supplier.MappingCompanyID,0) TextileCompanyID, KMS.TypeID As KTypeId

                FROM FBookingAcknowledgeChild BAC
				LEFT JOIN FBookingAcknowledge BA ON BA.FBAckID=BAC.AcknowledgeID
                LEFT JOIN FreeConceptMaster FCM ON FCM.BookingChildID = BAC.BookingChildID  AND FCM.ConsumptionID = BAC.ConsumptionID

                LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster SBM ON SBM.BookingID = BAC.BookingID
                LEFT JOIN {DbNames.EPYSL}..SampleBookingConsumption SBC ON SBC.BookingID = SBM.BookingID AND SBC.ConsumptionID = BAC.ConsumptionID

				LEFT JOIN {DbNames.EPYSL}..BookingMaster BM ON BM.BookingID = BAC.BookingID
				LEFT JOIN {DbNames.EPYSL}..BookingChild BCC ON BCC.BookingID = BM.BookingID AND BCC.ConsumptionID=BAC.ConsumptionID
				LEFT JOIN {DbNames.EPYSL}..BOMConsumption BCCC ON BCCC.ConsumptionID=BAC.ConsumptionID

                LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = BAC.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = BAC.SubGroupID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV On ISV.SegmentValueID = BAC.A1ValueID
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = BAC.YarnBrandID
                LEFT JOIN FabricTechnicalName T ON T.TechnicalNameId = BAC.TechnicalNameID
                LEFT JOIN KnittingMachineSubClass KMS ON KMS.SubClassID = BAC.MachineTypeId
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ETV2 ON ETV2.ValueID = BAC.BrandID
                LEFT Join {DbNames.EPYSL}..Contacts Supplier On Supplier.ContactID = SBM.SupplierID
                WHERE BAC.AcknowledgeID = {fbAckId} AND BAC.SubGroupID=1;

                -- Booking Acknowledge Child (Collar & Cuff)
                SELECT BAC.BookingChildID, BAC.AcknowledgeID, BAC.BookingID, BAC.ConsumptionChildID, BAC.ConsumptionID, BAC.BOMMasterID,
                BAC.ExportOrderID, BAC.ItemGroupID, BAC.SubGroupID, BAC.ItemMasterID, BAC.OrderBankPOID, BAC.ColorID, BAC.SizeID, BAC.TechPackID,
                BAC.ConsumptionQty, BAC.BookingQty, FCM.TotalQty, BAC.BookingUnitID, BAC.RequisitionQty, BAC.ISourcing, BAC.Remarks, BAC.LengthYds, BAC.LengthInch,
                BAC.FUPartID, BAC.A1ValueID, BAC.YarnBrandID, BAC.ContactID, BAC.LabDipNo, BAC.AddedBy, BAC.DateAdded, BAC.UpdatedBy, BAC.DateUpdated,
                BAC.ExecutionCompanyID, BAC.BlockBookingQty, BAC.AdjustQty, BAC.AutoAgree, BAC.Price, BAC.SuggestedPrice, BAC.LabdipUpdateDate,
                BAC.IsCompleteReceive, BAC.IsCompleteDelivery, BAC.LastDCDate, BAC.ClosingRemarks, BAC.ToItemMasterID, BAC.TechnicalNameID,
                BAC.MachineTypeId, BAC.IsSubContact, BAC.TotalDays, BAC.DeliveryDate, BAC.BrandID, BAC.MachineGauge,BAC.MachineDia, IM.Segment1ValueID ConstructionId, IM.Segment2ValueID CompositionId,
                IM.Segment3ValueID ColorId,IM.Segment7ValueID KnittingTypeId,IM.Segment4ValueID GSMId, ISG.SubGroupName,
                ISV.SegmentValue YarnType, ETV.ValueName YarnProgram, ETV2.ValueName Brand,

				BookingNo=CASE WHEN BA.WithoutOB = 0 THEN SBM.BookingNo ELSE BM.BookingNo END,
				ContactID=CASE WHEN BA.WithoutOB = 0 THEN SBM.BuyerID ELSE BM.BuyerID END,
				ExecutionCompanyID=CASE WHEN BA.WithoutOB = 0 THEN SBM.ExecutionCompanyID ELSE BM.CompanyID END,

                Construction=CASE WHEN BA.WithoutOB = 0 THEN SBC.Segment1Desc ELSE BCCC.Segment1Desc END,
				Composition=CASE WHEN BA.WithoutOB = 0 THEN SBC.Segment2Desc ELSE BCCC.Segment2Desc END,
				Color=CASE WHEN BA.WithoutOB = 0 THEN SBC.Segment5Desc ELSE BCCC.Segment5Desc END,
				--GSM=CASE WHEN BA.WithoutOB = 0 THEN SBC.Segment4Desc ELSE BCCC.Segment4Desc END,
                GSM = 0,
				FabricWidth=CASE WHEN BA.WithoutOB = 0 THEN SBC.Segment5Desc ELSE BCCC.Segment5Desc END,
				KnittingType=CASE WHEN BA.WithoutOB = 0 THEN SBC.Segment7Desc ELSE BCCC.Segment7Desc END,
				DyeingType=CASE WHEN BA.WithoutOB = 0 THEN SBC.Segment6Desc ELSE BCCC.Segment6Desc END,
				Instruction=CASE WHEN BA.WithoutOB = 0 THEN SBC.Remarks ELSE BCC.Remarks END,
				ForBDSStyleNo=CASE WHEN BA.WithoutOB = 0 THEN SBC.ForBDSStyleNo ELSE '' END,
				T.TechnicalName, KMS.SubClassName MachineType,
                BAC.TestReportDays,BAC.FinishingDays,BAC.DyeingDays,BAC.BatchPreparationDays,BAC.KnittingDays,BAC.MaterialDays,
                BAC.StructureDays, ISNULL(Supplier.MappingCompanyID,0) TextileCompanyID, KMS.TypeID As KTypeId

                FROM FBookingAcknowledgeChild BAC
				LEFT JOIN FBookingAcknowledge BA ON BA.FBAckID=BAC.AcknowledgeID
                LEFT JOIN FreeConceptMaster FCM ON FCM.BookingChildID = BAC.BookingChildID  AND FCM.ConsumptionID = BAC.ConsumptionID

                LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster SBM ON SBM.BookingID = BAC.BookingID
                LEFT JOIN {DbNames.EPYSL}..SampleBookingConsumption SBC ON SBC.BookingID = SBM.BookingID AND SBC.ConsumptionID = BAC.ConsumptionID

				LEFT JOIN {DbNames.EPYSL}..BookingMaster BM ON BM.BookingID = BAC.BookingID
				LEFT JOIN {DbNames.EPYSL}..BookingChild BCC ON BCC.BookingID = BM.BookingID AND BCC.ConsumptionID=BAC.ConsumptionID
				LEFT JOIN {DbNames.EPYSL}..BOMConsumption BCCC ON BCCC.ConsumptionID=BAC.ConsumptionID

                LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = BAC.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = BAC.SubGroupID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV On ISV.SegmentValueID = BAC.A1ValueID
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = BAC.YarnBrandID
                LEFT JOIN FabricTechnicalName T ON T.TechnicalNameId = BAC.TechnicalNameID
                LEFT JOIN KnittingMachineSubClass KMS ON KMS.SubClassID = BAC.MachineTypeId
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ETV2 ON ETV2.ValueID = BAC.BrandID
                LEFT Join {DbNames.EPYSL}..Contacts Supplier On Supplier.ContactID = SBM.SupplierID
                WHERE BAC.AcknowledgeID = {fbAckId} AND BAC.SubGroupID IN (11, 12);

                ----FBAChildPlanning
                --SELECT CP.*, CR.CriteriaName, CR.ProcessTime
                --FROM FBAChildPlanning CP
                --INNER JOIN BDSCriteria_HK CR ON CR.CriteriaID = CP.CriteriaID
                --WHERE CP.AcknowledgeID = {fbAckId}

               --;SELECT cp.BookingChildID, CR.CriteriaName, Sum(CR.ProcessTime)TotalTime, CriteriaIDs = String_Agg(CR.CriteriaID,',')
               -- FROM FBAChildPlanning CP
               -- INNER JOIN BDSCriteria_HK CR ON CR.CriteriaID = CP.CriteriaID and CP.AcknowledgeID =  {fbAckId}
               -- GROUP BY cp.BookingChildID, CR.CriteriaName;

                ;With A As(
	                select Min(FBAChildPlanningID) FBAChildPlanningID, BookingChildID, AcknowledgeID, CriteriaID
	                From FBAChildPlanning
	                Where AcknowledgeID = {fbAckId}
	                Group By BookingChildID, AcknowledgeID, CriteriaID
                )
                SELECT cp.BookingChildID, CR.CriteriaName, Sum(CR.ProcessTime)TotalTime, CriteriaIDs = String_Agg(CR.CriteriaID,',')
                FROM A CP
                INNER JOIN BDSCriteria_HK CR ON CR.CriteriaID = CP.CriteriaID
                GROUP BY cp.BookingChildID, CR.CriteriaName

                --Technical Name
                SELECT Cast(T.TechnicalNameId As varchar) id, T.TechnicalName [text], ISNULL(ST.[Days], 0) [desc], Cast(SC.SubClassID as varchar) additionalValue
                FROM FabricTechnicalName T
                LEFT JOIN FabricTechnicalNameKMachineSubClass SC ON SC.TechnicalNameID = T.TechnicalNameId
                LEFT JOIN KnittingMachineStructureType_HK ST ON ST.StructureTypeID = SC.StructureTypeID
                Group By T.TechnicalNameId, T.TechnicalName, ST.Days, SC.SubClassID;

                --M/c type
                ;SELECT CAST(a.MachineSubClassID AS varchar) [id], b.SubClassName [text], b.TypeID [desc], c.TypeName additionalValue
                FROM KnittingMachine a
                INNER JOIN KnittingMachineSubClass b ON b.SubClassID = a.MachineSubClassID
                Inner Join KnittingMachineType c On c.TypeID = b.TypeID
                --Where c.TypeName != 'Flat Bed'
                GROUP BY a.MachineSubClassID, b.SubClassName, b.TypeID, c.TypeName;

                --CriteriaNames
                 ;SELECT CriteriaName,CriteriaSeqNo,(CASE WHEN CriteriaName  IN('Batch Preparation','Quality Check') THEN '1'ELSE'0'END) AS TotalTime
                FROM BDSCriteria_HK --WHERE CriteriaName NOT IN('Batch Preparation','Testing')
                GROUP BY CriteriaSeqNo,CriteriaName order by CriteriaSeqNo,CriteriaName;

                --FBAChildPlannings
                ;SELECT * FROM BDSCriteria_HK order by CriteriaSeqNo, OperationSeqNo, CriteriaName;

	            --FBookingAcknowledgeChildDetails (Fabric)
				SELECT *
				FROM FBookingAcknowledgeChildDetails A
				LEFT JOIN FBookingAcknowledgeChild B ON B.BookingChildID=A.BookingChildID
				WHERE B.AcknowledgeID={fbAckId} AND B.SubGroupID IN (1);

			    --FBookingAcknowledgeChildDetails (Collar & Cuff)
				SELECT *
				FROM FBookingAcknowledgeChildDetails A
				LEFT JOIN FBookingAcknowledgeChild B ON B.BookingChildID=A.BookingChildID
				WHERE B.AcknowledgeID={fbAckId} AND B.SubGroupID IN (11,12);

                --Brand List
				;SELECT DISTINCT(KM.BrandID) [id], EV.ValueName [text]
				FROM KnittingMachine KM
				LEFT JOIN KnittingUnit KU ON KU.KnittingUnitID = KM.KnittingUnitID
				LEFT JOIN {DbNames.EPYSL}..EntityTypeValue EV ON ValueID = KM.BrandID
				ORDER BY [text];";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                FBookingAcknowledge data = records.Read<FBookingAcknowledge>().FirstOrDefault();
                Guard.Against.NullObject(data);

                List<FBookingAcknowledgeChild> bookingChilds = records.Read<FBookingAcknowledgeChild>().ToList();
                List<FBookingAcknowledgeChild> bookingChildsCollarCuff = records.Read<FBookingAcknowledgeChild>().ToList();
                //List<FBAChildPlanning> childPlanningCriterias = records.Read<FBAChildPlanning>().ToList();
                List<FBAChildPlanning> childPlanningCriteriaNameWithIds = records.Read<FBAChildPlanning>().ToList();

                data.TechnicalNameList = await records.ReadAsync<Select2OptionModel>();
                List<Select2OptionModel> mcTypeList = records.Read<Select2OptionModel>().ToList();
                data.MCTypeForFabricList = mcTypeList.Where(x => x.additionalValue != "Flat Bed");
                data.MCTypeForOtherList = mcTypeList.Where(x => x.additionalValue == "Flat Bed");

                List<FBookingAcknowledgeChild> criteriaNames = records.Read<FBookingAcknowledgeChild>().ToList();
                List<FBAChildPlanning> fbaChildPlannings = records.Read<FBAChildPlanning>().ToList();

                var fabricChildDetails = records.Read<FBookingAcknowledgeChildDetails>().ToList();
                var collarCuffChildDetails = records.Read<FBookingAcknowledgeChildDetails>().ToList();
                data.KnittingMachines = records.Read<KnittingMachine>().ToList();

                bookingChilds.ForEach(bc =>
                {
                    bc.CriteriaNames = criteriaNames;
                    bc.FBAChildPlannings = fbaChildPlannings;
                    bc.FBAChildPlanningsWithIds = childPlanningCriteriaNameWithIds.Where(x => x.BookingChildID == bc.BookingChildID).ToList();
                    bc.ChildDetails = fabricChildDetails.Where(y => y.BookingChildID == bc.BookingChildID).ToList();
                    //bc.CriteriaIDs = string.Join(",", childPlanningCriterias.Where(x => x.BookingChildID == bc.BookingChildID).ToList().Select(y => y.CriteriaID));
                });
                bookingChildsCollarCuff.ForEach(bc =>
                {
                    bc.CriteriaNames = criteriaNames;
                    bc.FBAChildPlannings = fbaChildPlannings;
                    bc.FBAChildPlanningsWithIds = childPlanningCriteriaNameWithIds.Where(x => x.BookingChildID == bc.BookingChildID).ToList();
                    bc.ChildDetails = collarCuffChildDetails.Where(y => y.BookingChildID == bc.BookingChildID).ToList();
                    //bc.CriteriaIDs = string.Join(",", childPlanningCriterias.Where(x => x.BookingChildID == bc.BookingChildID).ToList().Select(y => y.CriteriaID));
                });

                data.FBookingChild = bookingChilds.Where(x => x.SubGroupID == 1).ToList();
                data.FBookingChildCollor = bookingChildsCollarCuff.Where(x => x.SubGroupID == 11).ToList();
                data.FBookingChildCuff = bookingChildsCollarCuff.Where(x => x.SubGroupID == 12).ToList();
                data.HasFabric = data.FBookingChild.Count() > 0 ? true : false;
                data.HasCollar = data.FBookingChildCollor.Count() > 0 ? true : false;
                data.HasCuff = data.FBookingChildCuff.Count() > 0 ? true : false;

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
        public async Task<FBookingAcknowledge> GetFBAcknowledge(int fbAckId)
        {
            var query =
                $@"SELECT FBA.*
                    FROM FBookingAcknowledge FBA
                    LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster SBM ON SBM.BookingID = FBA.BookingID
                    LEFT JOIN {DbNames.EPYSL}..BookingMaster BM ON BM.BookingID = FBA.BookingID
                    WHERE FBA.FBAckID={fbAckId};

                    SELECT BAC.*
                    FROM FBookingAcknowledgeChild BAC
                    LEFT JOIN FBookingAcknowledge BA ON BA.FBAckID=BAC.AcknowledgeID
                    WHERE BAC.AcknowledgeID = {fbAckId} AND BAC.SubGroupID IN (1,11,12);

                    SELECT A.*
                    FROM FBookingAcknowledgeChildAddProcess A
                    LEFT JOIN FBookingAcknowledgeChild BAC ON BAC.BookingChildID=A.BookingChildID
                    WHERE BAC.AcknowledgeID = {fbAckId} AND BAC.SubGroupID IN (1,11,12);

                    SELECT A.*
                    FROM FBookingAcknowledgeChildDetails A
                    LEFT JOIN FBookingAcknowledgeChild BAC ON BAC.BookingChildID=A.BookingChildID
                    WHERE BAC.AcknowledgeID = {fbAckId} AND BAC.SubGroupID IN (1,11,12);

                    SELECT A.*
                    FROM FBookingAcknowledgeChildGarmentPart A
                    LEFT JOIN FBookingAcknowledgeChild BAC ON BAC.BookingChildID=A.BookingChildID
                    WHERE BAC.AcknowledgeID = {fbAckId} AND BAC.SubGroupID IN (1,11,12);

                    SELECT A.*
                    FROM FBookingAcknowledgeChildProcess A
                    LEFT JOIN FBookingAcknowledgeChild BAC ON BAC.BookingChildID=A.BookingChildID
                    WHERE BAC.AcknowledgeID = {fbAckId} AND BAC.SubGroupID IN (1,11,12);

                    SELECT A.*
                    FROM FBookingAcknowledgeChildDistribution A
                    LEFT JOIN FBookingAcknowledgeChild BAC ON BAC.BookingChildID=A.BookingChildID
                    WHERE BAC.AcknowledgeID = {fbAckId} AND BAC.SubGroupID IN (1,11,12);

                    SELECT A.*
                    FROM FBookingAcknowledgeChildText A
                    LEFT JOIN FBookingAcknowledgeChild BAC ON BAC.BookingChildID=A.BookingChildID
                    WHERE BAC.AcknowledgeID = {fbAckId} AND BAC.SubGroupID IN (1,11,12);

                    SELECT A.*
                    FROM FBookingAcknowledgeChildYarnSubBrand A
                    LEFT JOIN FBookingAcknowledgeChild BAC ON BAC.BookingChildID=A.BookingChildID
                    WHERE BAC.AcknowledgeID = {fbAckId} AND BAC.SubGroupID IN (1,11,12);

                    SELECT A.*
                    FROM BDSDependentTNACalander A
                    LEFT JOIN FBookingAcknowledgeChild BAC ON BAC.BookingChildID=A.BookingChildID
                    WHERE BAC.AcknowledgeID = {fbAckId} AND BAC.SubGroupID IN (1,11,12);

                    SELECT A.*
                    FROM FBookingAcknowledgeImage A
                    LEFT JOIN FBookingAcknowledge BA ON BA.BookingID=A.BookingID
                    WHERE BA.FBAckID={fbAckId};

                    SELECT FBAC.*
                    FROM FBAChildPlanning FBAC
                    WHERE FBAC.AcknowledgeID = {fbAckId};

                    --All FBAChildPlannings
                    ;SELECT * FROM BDSCriteria_HK order by CriteriaSeqNo, OperationSeqNo, CriteriaName;";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                FBookingAcknowledge data = records.Read<FBookingAcknowledge>().FirstOrDefault();
                Guard.Against.NullObject(data);

                data.FBookingChild = records.Read<FBookingAcknowledgeChild>().ToList();
                data.FBookingAcknowledgeChildAddProcess = records.Read<FBookingAcknowledgeChildAddProcess>().ToList();
                data.FBookingChildDetails = records.Read<FBookingAcknowledgeChildDetails>().ToList();
                data.FBookingAcknowledgeChildGarmentPart = records.Read<FBookingAcknowledgeChildGarmentPart>().ToList();
                data.FBookingAcknowledgeChildProcess = records.Read<FBookingAcknowledgeChildProcess>().ToList();
                data.FBookingAcknowledgeChildText = records.Read<FBookingAcknowledgeChildText>().ToList();
                data.FBookingAcknowledgeChildDistribution = records.Read<FBookingAcknowledgeChildDistribution>().ToList();
                data.FBookingAcknowledgeChildYarnSubBrand = records.Read<FBookingAcknowledgeChildYarnSubBrand>().ToList();
                data.BDSDependentTNACalander = records.Read<BDSDependentTNACalander>().ToList();
                data.FBookingAcknowledgeImage = records.Read<FBookingAcknowledgeImage>().ToList();
                var fBAChildPlannings = records.Read<FBAChildPlanning>().ToList();
                data.FBookingChild.ForEach(x =>
                {
                    x.FBAChildPlannings = fBAChildPlannings.Where(y => y.BookingChildID == x.BookingChildID).ToList();
                    x.CriteriaIDs = string.Join(",", x.FBAChildPlannings.Select(y => y.CriteriaID).Distinct());
                });
                data.AllChildPlannings = records.Read<FBAChildPlanning>().ToList();
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
        public async Task<FabricBookingAcknowledge> GetAllSavedFBAcknowledgeByBookingID(String BookingID, bool isRevised = false)
        {
            string parentQuery = $@"SELECT * FROM FabricBookingAcknowledge WHERE BookingID in ({BookingID});";

            if (isRevised)
            {
                parentQuery = $@"
                    WITH
                    SBM AS
                    (
	                    SELECT SBM.BookingID, SBM.RevisionNo
	                    FROM {DbNames.EPYSL}..SampleBookingMaster SBM
	                    WHERE SBM.BookingID IN ({BookingID})
                    ),
                    BM AS
                    (
	                    SELECT BM.BookingID, RevisionNo = MAX(BM.RevisionNo)
	                    FROM {DbNames.EPYSL}..BookingMaster BM
	                    WHERE BM.BookingID IN ({BookingID})
	                    GROUP BY BM.BookingID
                    )
                    Select FBA.AcknowledgeID, FBA.BookingID, FBA.BOMMasterID, FBA.ItemGroupID, FBA.SubGroupID, FBA.Status, 
                    FBA.AcknowledgeDate, FBA.AddedBy, FBA.DateAdded, FBA.UpdatedBy, FBA.DateUpdated, FBA.WithoutOB,
                    FBA.UnAcknowledge, FBA.UnAcknowledgeDate, FBA.UnAcknowledge, FBA.RevisionNo, 
                    PreProcessRevNo = CASE WHEN ISNULL(BM.BookingID,0) = 0 THEN ISNULL(SBM.RevisionNo,0) ELSE ISNULL(BM.RevisionNo,0) END
                    From FabricBookingAcknowledge FBA
                    LEFT JOIN SBM ON SBM.BookingID = FBA.BookingID
                    LEFT JOIN BM ON BM.BookingID = FBA.BookingID
                    WHERE FBA.BookingID in ({BookingID});";
            }

            var query =
                $@" {parentQuery}

                    SELECT FBA.*
                    FROM FBookingAcknowledge FBA
                    WHERE FBA.BookingID in ({BookingID});                    

                    SELECT BAC.*
                    FROM FBookingAcknowledgeChild BAC
                    LEFT JOIN FBookingAcknowledge BA ON BA.FBAckID=BAC.AcknowledgeID
                    WHERE BAC.BookingID in ({BookingID}) AND BAC.SubGroupID IN (1,11,12);

                    SELECT A.*
                    FROM FBookingAcknowledgeChildAddProcess A
                    LEFT JOIN FBookingAcknowledgeChild BAC ON BAC.BookingChildID=A.BookingChildID
                    WHERE BAC.BookingID in ({BookingID}) AND BAC.SubGroupID IN (1,11,12);

                    SELECT A.*
                    FROM FBookingAcknowledgeChildDetails A
                    LEFT JOIN FBookingAcknowledgeChild BAC ON BAC.BookingChildID=A.BookingChildID
                    WHERE BAC.BookingID in ({BookingID}) AND BAC.SubGroupID IN (1,11,12);

                    SELECT A.*
                    FROM FBookingAcknowledgeChildGarmentPart A
                    LEFT JOIN FBookingAcknowledgeChild BAC ON BAC.BookingChildID=A.BookingChildID
                    WHERE BAC.BookingID in ({BookingID}) AND BAC.SubGroupID IN (1,11,12);

                    SELECT A.*
                    FROM FBookingAcknowledgeChildProcess A
                    LEFT JOIN FBookingAcknowledgeChild BAC ON BAC.BookingChildID=A.BookingChildID
                    WHERE BAC.BookingID in ({BookingID}) AND BAC.SubGroupID IN (1,11,12);

                    SELECT A.*
                    FROM FBookingAcknowledgeChildDistribution A
                    LEFT JOIN FBookingAcknowledgeChild BAC ON BAC.BookingChildID=A.BookingChildID
                    WHERE BAC.BookingID in ({BookingID}) AND BAC.SubGroupID IN (1,11,12);

                    SELECT A.*
                    FROM FBookingAcknowledgeChildText A
                    LEFT JOIN FBookingAcknowledgeChild BAC ON BAC.BookingChildID=A.BookingChildID
                    WHERE BAC.BookingID in ({BookingID}) AND BAC.SubGroupID IN (1,11,12);

                    SELECT A.*
                    FROM FBookingAcknowledgeChildYarnSubBrand A
                    LEFT JOIN FBookingAcknowledgeChild BAC ON BAC.BookingChildID=A.BookingChildID
                    WHERE BAC.BookingID in ({BookingID}) AND BAC.SubGroupID IN (1,11,12);

                    SELECT A.*
                    FROM BDSDependentTNACalander A
                    LEFT JOIN FBookingAcknowledgeChild BAC ON BAC.BookingChildID=A.BookingChildID
                    WHERE BAC.BookingID in ({BookingID}) AND BAC.SubGroupID IN (1,11,12);

                    SELECT A.*
                    FROM FBookingAcknowledgeImage A
                    LEFT JOIN FBookingAcknowledge BA ON BA.BookingID=A.BookingID
                    WHERE BA.BookingID in ({BookingID});

                    SELECT FBAC.*
                    FROM FBAChildPlanning FBAC
                    LEFT JOIN FBookingAcknowledgeChild BAC ON BAC.BookingChildID=FBAC.BookingChildID
                    WHERE BAC.BookingID in ({BookingID})

                    --All FBAChildPlannings
                    ;SELECT * FROM BDSCriteria_HK order by CriteriaSeqNo, OperationSeqNo, CriteriaName;

                    ;Select FBAC.*
					From FBookingAcknowledgementLiabilityDistribution FBAC
					Where FBAC.BookingID in ({BookingID})

					;Select FBAC.*
					From FBookingAcknowledgementYarnLiability FBAC
					Where FBAC.BookingID in ({BookingID})";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                FabricBookingAcknowledge data = new FabricBookingAcknowledge();
                data.FabricBookingAcknowledgeList = records.Read<FabricBookingAcknowledge>().ToList();

                if (data.IsNull()) return null;

                data.FBookingAcknowledgeList = records.Read<FBookingAcknowledge>().ToList();
                data.FBookingChild = records.Read<FBookingAcknowledgeChild>().ToList();
                data.FBookingAcknowledgeChildAddProcess = records.Read<FBookingAcknowledgeChildAddProcess>().ToList();
                data.FBookingChildDetails = records.Read<FBookingAcknowledgeChildDetails>().ToList();
                data.FBookingAcknowledgeChildGarmentPart = records.Read<FBookingAcknowledgeChildGarmentPart>().ToList();
                data.FBookingAcknowledgeChildProcess = records.Read<FBookingAcknowledgeChildProcess>().ToList();
                data.FBookingAcknowledgeChildText = records.Read<FBookingAcknowledgeChildText>().ToList();
                data.FBookingAcknowledgeChildDistribution = records.Read<FBookingAcknowledgeChildDistribution>().ToList();
                data.FBookingAcknowledgeChildYarnSubBrand = records.Read<FBookingAcknowledgeChildYarnSubBrand>().ToList();
                data.BDSDependentTNACalander = records.Read<BDSDependentTNACalander>().ToList();
                data.FBookingAcknowledgeImage = records.Read<FBookingAcknowledgeImage>().ToList();
                var fBAChildPlannings = records.Read<FBAChildPlanning>().ToList();
                data.FBookingChild.ForEach(x =>
                {
                    x.FBAChildPlannings = fBAChildPlannings.Where(y => y.BookingChildID == x.BookingChildID).ToList();
                    x.CriteriaIDs = string.Join(",", x.FBAChildPlannings.Select(y => y.CriteriaID).Distinct());
                });

                data.AllChildPlannings = records.Read<FBAChildPlanning>().ToList();
                data.FBookingAckLiabilityDistributionList = records.Read<FBookingAcknowledgementLiabilityDistribution>().ToList();
                data.FBookingAcknowledgementYarnLiabilityList = records.Read<FBookingAcknowledgementYarnLiability>().ToList();

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
        public async Task<FBookingAcknowledge> GetFBAcknowledgeByBookingID(int BookingID)
        {
            var query =
                $@"SELECT FBA.*
                    FROM FBookingAcknowledge FBA
                    WHERE FBA.BookingID={BookingID};
                    
                    SELECT FBA.*
                    FROM FabricBookingAcknowledge FBA
                    WHERE FBA.BookingID={BookingID};

                    SELECT BAC.*
                    FROM FBookingAcknowledgeChild BAC
                    LEFT JOIN FBookingAcknowledge BA ON BA.FBAckID=BAC.AcknowledgeID
                    WHERE BAC.BookingID = {BookingID} AND BAC.SubGroupID IN (1,11,12);

                    SELECT A.*
                    FROM FBookingAcknowledgeChildAddProcess A
                    LEFT JOIN FBookingAcknowledgeChild BAC ON BAC.BookingChildID=A.BookingChildID
                    WHERE BAC.BookingID = {BookingID} AND BAC.SubGroupID IN (1,11,12);

                    SELECT A.*
                    FROM FBookingAcknowledgeChildDetails A
                    LEFT JOIN FBookingAcknowledgeChild BAC ON BAC.BookingChildID=A.BookingChildID
                    WHERE BAC.BookingID = {BookingID} AND BAC.SubGroupID IN (1,11,12);

                    SELECT A.*
                    FROM FBookingAcknowledgeChildGarmentPart A
                    LEFT JOIN FBookingAcknowledgeChild BAC ON BAC.BookingChildID=A.BookingChildID
                    WHERE BAC.BookingID = {BookingID} AND BAC.SubGroupID IN (1,11,12);

                    SELECT A.*
                    FROM FBookingAcknowledgeChildProcess A
                    LEFT JOIN FBookingAcknowledgeChild BAC ON BAC.BookingChildID=A.BookingChildID
                    WHERE BAC.BookingID = {BookingID} AND BAC.SubGroupID IN (1,11,12);

                    SELECT A.*
                    FROM FBookingAcknowledgeChildDistribution A
                    LEFT JOIN FBookingAcknowledgeChild BAC ON BAC.BookingChildID=A.BookingChildID
                    WHERE BAC.BookingID = {BookingID} AND BAC.SubGroupID IN (1,11,12);

                    SELECT A.*
                    FROM FBookingAcknowledgeChildText A
                    LEFT JOIN FBookingAcknowledgeChild BAC ON BAC.BookingChildID=A.BookingChildID
                    WHERE BAC.BookingID = {BookingID} AND BAC.SubGroupID IN (1,11,12);

                    SELECT A.*
                    FROM FBookingAcknowledgeChildYarnSubBrand A
                    LEFT JOIN FBookingAcknowledgeChild BAC ON BAC.BookingChildID=A.BookingChildID
                    WHERE BAC.BookingID = {BookingID} AND BAC.SubGroupID IN (1,11,12);

                    SELECT A.*
                    FROM BDSDependentTNACalander A
                    LEFT JOIN FBookingAcknowledgeChild BAC ON BAC.BookingChildID=A.BookingChildID
                    WHERE BAC.BookingID = {BookingID} AND BAC.SubGroupID IN (1,11,12);

                    SELECT A.*
                    FROM FBookingAcknowledgeImage A
                    LEFT JOIN FBookingAcknowledge BA ON BA.BookingID=A.BookingID
                    WHERE BA.BookingID={BookingID};

                    SELECT FBAC.*
                    FROM FBAChildPlanning FBAC
                    LEFT JOIN FBookingAcknowledgeChild BAC ON BAC.BookingChildID=FBAC.BookingChildID
                    WHERE BAC.BookingID = {BookingID};

                    --All FBAChildPlannings
                    ;SELECT * FROM BDSCriteria_HK order by CriteriaSeqNo, OperationSeqNo, CriteriaName;";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                FBookingAcknowledge data = records.Read<FBookingAcknowledge>().FirstOrDefault();
                //Guard.Against.NullObject(data);
                if (data.IsNull())
                    return null;
                data.FabricBookingAcknowledgeList = records.Read<FabricBookingAcknowledge>().ToList();
                data.FBookingChild = records.Read<FBookingAcknowledgeChild>().ToList();
                data.FBookingAcknowledgeChildAddProcess = records.Read<FBookingAcknowledgeChildAddProcess>().ToList();
                data.FBookingChildDetails = records.Read<FBookingAcknowledgeChildDetails>().ToList();
                data.FBookingAcknowledgeChildGarmentPart = records.Read<FBookingAcknowledgeChildGarmentPart>().ToList();
                data.FBookingAcknowledgeChildProcess = records.Read<FBookingAcknowledgeChildProcess>().ToList();
                data.FBookingAcknowledgeChildText = records.Read<FBookingAcknowledgeChildText>().ToList();
                data.FBookingAcknowledgeChildDistribution = records.Read<FBookingAcknowledgeChildDistribution>().ToList();
                data.FBookingAcknowledgeChildYarnSubBrand = records.Read<FBookingAcknowledgeChildYarnSubBrand>().ToList();
                data.BDSDependentTNACalander = records.Read<BDSDependentTNACalander>().ToList();
                data.FBookingAcknowledgeImage = records.Read<FBookingAcknowledgeImage>().ToList();
                var fBAChildPlannings = records.Read<FBAChildPlanning>().ToList();
                data.FBookingChild.ForEach(x =>
                {
                    x.FBAChildPlannings = fBAChildPlannings.Where(y => y.BookingChildID == x.BookingChildID).ToList();
                    x.CriteriaIDs = string.Join(",", x.FBAChildPlannings.Select(y => y.CriteriaID).Distinct());
                });
                data.AllChildPlannings = records.Read<FBAChildPlanning>().ToList();
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
        public async Task<FBookingAcknowledge> GetDataByBookingNo(string bookingNo)
        {
            var query =
                $@"-- Master Data
                    WITH M AS (
                        SELECT FBA.FBAckID,	FBA.BookingID,FBA.BookingNo,FBA.BookingDate,FBA.SLNo,FBA.StyleMasterID,FBA.StyleNo,FBA.SubGroupID,
	                    FBA.BuyerID,FBA.BuyerTeamID,FBA.SupplierID,FBA.ExportOrderID,FBA.ExecutionCompanyID,
	                    OrderQty = CASE WHEN FBA.WithoutOB=1 THEN SBM.OrderQty ELSE BM.RePurchaseQty END,
	                    Remarks = CASE WHEN FBA.WithoutOB=1 THEN SBM.Remarks ELSE BM.Remarks END,
	                    SeasonID = CASE WHEN FBA.WithoutOB=1 THEN SBM.SeasonID ELSE 0 END,
	                    BookingBy = CASE WHEN FBA.WithoutOB=1 THEN SBM.AddedBy ELSE BM.AddedBy END

                        FROM FBookingAcknowledge FBA
	                    LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster SBM ON SBM.BookingID = FBA.BookingID
	                    LEFT JOIN {DbNames.EPYSL}..BookingMaster BM ON BM.BookingID = FBA.BookingID
                        WHERE FBA.BookingNo = '{bookingNo}'
                    )
                    SELECT M.FBAckID, M.BookingID,M.BookingNo,M.BookingDate,M.SLNo,M.StyleMasterID,M.StyleNo,M.OrderQty BookingQty,M.BuyerID,M.BuyerTeamID,
                    M.SupplierID,M.ExportOrderID,M.ExecutionCompanyID, CTO.ShortName BuyerName, CCT.TeamName BuyerTeamName,C.CompanyName,M.SubGroupID,M.Remarks,
                    Supplier.ShortName [SupplierName],Season.SeasonName, M.BookingBy, ISNULL(Supplier.MappingCompanyID,0) TextileCompanyID
                    FROM M
                    LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = M.BuyerID
                    LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = M.BuyerTeamID
                    LEFT JOIN {DbNames.EPYSL}..CompanyEntity C ON C.CompanyID = M.ExecutionCompanyID
                    LEFT JOIN {DbNames.EPYSL}..Contacts Supplier On M.SupplierID = Supplier.ContactID
                    LEFT JOIN {DbNames.EPYSL}..ContactSeason Season On M.SeasonID = Season.SeasonID;

                    -- Booking Acknowledge Child (Fabric)
                    WITH F AS
                    (
	                    SELECT BAC.BookingChildID, BAC.AcknowledgeID, BAC.BookingID, BAC.ConsumptionChildID, BAC.ConsumptionID, BAC.BOMMasterID,
	                    BAC.ExportOrderID, BAC.ItemGroupID, BAC.SubGroupID, BAC.ItemMasterID, BAC.OrderBankPOID, BAC.SizeID, BAC.TechPackID,
	                    BAC.ConsumptionQty, BAC.BookingQty, FCM.TotalQty, BAC.BookingUnitID, BAC.RequisitionQty, BAC.ISourcing, BAC.Remarks, BAC.LengthYds, BAC.LengthInch,
	                    BAC.FUPartID, BAC.A1ValueID, BAC.YarnBrandID, BAC.LabDipNo, BAC.AddedBy, BAC.DateAdded, BAC.UpdatedBy, BAC.DateUpdated,
	                    BAC.BlockBookingQty, BAC.AdjustQty, BAC.AutoAgree, BAC.Price, BAC.SuggestedPrice, BAC.LabdipUpdateDate,
	                    BAC.IsCompleteReceive, BAC.IsCompleteDelivery, BAC.LastDCDate, BAC.ClosingRemarks, BAC.ToItemMasterID, BAC.TechnicalNameID,
	                    BAC.MachineTypeId, BAC.IsSubContact, BAC.TotalDays, BAC.DeliveryDate, BAC.BrandID, BAC.MachineGauge,BAC.MachineDia, IM.Segment1ValueID ConstructionId, IM.Segment2ValueID CompositionId,
	                    IM.Segment3ValueID ColorId,IM.Segment7ValueID KnittingTypeId,IM.Segment4ValueID GSMId, ISG.SubGroupName,
	                    ISV.SegmentValue YarnType, ETV.ValueName YarnProgram, ETV2.ValueName Brand, BA.BookingNo,
	                    ContactID = CASE WHEN BA.WithoutOB = 0 THEN SBM.BuyerID ELSE BM.BuyerID END,
	                    ExecutionCompanyID = CASE WHEN BA.WithoutOB = 0 THEN SBM.ExecutionCompanyID ELSE BM.CompanyID END,

	                    Construction=CASE WHEN BA.WithoutOB = 0 THEN SBC.Segment1Desc ELSE BCCC.Segment1Desc END,
	                    Composition=CASE WHEN BA.WithoutOB = 0 THEN SBC.Segment2Desc ELSE BCCC.Segment2Desc END,
	                    Color=CASE WHEN BA.WithoutOB = 0 THEN SBC.Segment3Desc ELSE BCCC.Segment3Desc END,
	                    GSM=CASE WHEN BA.WithoutOB = 0 THEN SBC.Segment4Desc ELSE BCCC.Segment4Desc END,
	                    FabricWidth=CASE WHEN BA.WithoutOB = 0 THEN SBC.Segment5Desc ELSE BCCC.Segment5Desc END,
	                    KnittingType=CASE WHEN BA.WithoutOB = 0 THEN SBC.Segment7Desc ELSE BCCC.Segment7Desc END,
	                    DyeingType=CASE WHEN BA.WithoutOB = 0 THEN SBC.Segment6Desc ELSE BCCC.Segment6Desc END,
	                    Instruction=CASE WHEN BA.WithoutOB = 0 THEN SBC.Remarks ELSE BCC.Remarks END,
	                    ForBDSStyleNo=CASE WHEN BA.WithoutOB = 0 THEN SBC.ForBDSStyleNo ELSE '' END,
	                    T.TechnicalName, KMS.SubClassName MachineType,
	                    BAC.TestReportDays,BAC.FinishingDays,BAC.DyeingDays,BAC.BatchPreparationDays,BAC.KnittingDays,BAC.MaterialDays,
	                    BAC.StructureDays, ISNULL(Supplier.MappingCompanyID,0) TextileCompanyID, KMS.TypeID As KTypeId

	                    FROM FBookingAcknowledgeChild BAC
	                    LEFT JOIN FBookingAcknowledge BA ON BA.FBAckID=BAC.AcknowledgeID
	                    LEFT JOIN FreeConceptMaster FCM ON FCM.BookingChildID = BAC.BookingChildID AND FCM.ConsumptionID = BAC.ConsumptionID

	                    LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster SBM ON SBM.BookingID = BAC.BookingID
	                    LEFT JOIN {DbNames.EPYSL}..SampleBookingConsumption SBC ON SBC.BookingID = SBM.BookingID AND SBC.ConsumptionID = BAC.ConsumptionID

	                    LEFT JOIN {DbNames.EPYSL}..BookingMaster BM ON BM.BookingID = BAC.BookingID
	                    LEFT JOIN {DbNames.EPYSL}..BookingChild BCC ON BCC.BookingID = BM.BookingID AND BCC.ConsumptionID=BAC.ConsumptionID
	                    LEFT JOIN {DbNames.EPYSL}..BOMConsumption BCCC ON BCCC.ConsumptionID=BAC.ConsumptionID

	                    LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = BAC.ItemMasterID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = BAC.SubGroupID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV On ISV.SegmentValueID = BAC.A1ValueID
	                    LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = BAC.YarnBrandID
	                    LEFT JOIN FabricTechnicalName T ON T.TechnicalNameId = BAC.TechnicalNameID
	                    LEFT JOIN KnittingMachineSubClass KMS ON KMS.SubClassID = BAC.MachineTypeId
	                    LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ETV2 ON ETV2.ValueID = BAC.BrandID
	                    LEFT Join {DbNames.EPYSL}..Contacts Supplier On Supplier.ContactID = SBM.SupplierID
                    )
                    SELECT * FROM F WHERE BookingNo = '{bookingNo}' AND SubGroupID = 1;

                    -- Booking Acknowledge Child (Collar & Cuff)
                    WITH F AS
                    (
	                    SELECT BAC.BookingChildID, BAC.AcknowledgeID, BAC.BookingID, BAC.ConsumptionChildID, BAC.ConsumptionID, BAC.BOMMasterID,
	                    BAC.ExportOrderID, BAC.ItemGroupID, BAC.SubGroupID, BAC.ItemMasterID, BAC.OrderBankPOID, BAC.SizeID, BAC.TechPackID,
	                    BAC.ConsumptionQty, BAC.BookingQty, FCM.TotalQty, BAC.BookingUnitID, BAC.RequisitionQty, BAC.ISourcing, BAC.Remarks, BAC.LengthYds, BAC.LengthInch,
	                    BAC.FUPartID, BAC.A1ValueID, BAC.YarnBrandID, BAC.LabDipNo, BAC.AddedBy, BAC.DateAdded, BAC.UpdatedBy, BAC.DateUpdated,
	                    BAC.BlockBookingQty, BAC.AdjustQty, BAC.AutoAgree, BAC.Price, BAC.SuggestedPrice, BAC.LabdipUpdateDate,
	                    BAC.IsCompleteReceive, BAC.IsCompleteDelivery, BAC.LastDCDate, BAC.ClosingRemarks, BAC.ToItemMasterID, BAC.TechnicalNameID,
	                    BAC.MachineTypeId, BAC.IsSubContact, BAC.TotalDays, BAC.DeliveryDate, BAC.BrandID, BAC.MachineGauge,BAC.MachineDia, IM.Segment1ValueID ConstructionId, IM.Segment2ValueID CompositionId,
	                    IM.Segment3ValueID ColorId,IM.Segment7ValueID KnittingTypeId,IM.Segment4ValueID GSMId, ISG.SubGroupName,
	                    ISV.SegmentValue YarnType, ETV.ValueName YarnProgram, ETV2.ValueName Brand, BA.BookingNo,
	                    ContactID = CASE WHEN BA.WithoutOB = 0 THEN SBM.BuyerID ELSE BM.BuyerID END,
	                    ExecutionCompanyID = CASE WHEN BA.WithoutOB = 0 THEN SBM.ExecutionCompanyID ELSE BM.CompanyID END,

	                    Construction=CASE WHEN BA.WithoutOB = 0 THEN SBC.Segment1Desc ELSE BCCC.Segment1Desc END,
	                    Composition=CASE WHEN BA.WithoutOB = 0 THEN SBC.Segment2Desc ELSE BCCC.Segment2Desc END,
	                    Color=CASE WHEN BA.WithoutOB = 0 THEN SBC.Segment5Desc ELSE BCCC.Segment5Desc END,
	                    --GSM=CASE WHEN BA.WithoutOB = 0 THEN SBC.Segment4Desc ELSE BCCC.Segment4Desc END,
	                    GSM = 0,
	                    FabricWidth=CASE WHEN BA.WithoutOB = 0 THEN SBC.Segment5Desc ELSE BCCC.Segment5Desc END,
	                    KnittingType=CASE WHEN BA.WithoutOB = 0 THEN SBC.Segment7Desc ELSE BCCC.Segment7Desc END,
	                    DyeingType=CASE WHEN BA.WithoutOB = 0 THEN SBC.Segment6Desc ELSE BCCC.Segment6Desc END,
	                    Instruction=CASE WHEN BA.WithoutOB = 0 THEN SBC.Remarks ELSE BCC.Remarks END,
	                    ForBDSStyleNo=CASE WHEN BA.WithoutOB = 0 THEN SBC.ForBDSStyleNo ELSE '' END,
	                    T.TechnicalName, KMS.SubClassName MachineType,
	                    BAC.TestReportDays,BAC.FinishingDays,BAC.DyeingDays,BAC.BatchPreparationDays,BAC.KnittingDays,BAC.MaterialDays,
	                    BAC.StructureDays, ISNULL(Supplier.MappingCompanyID,0) TextileCompanyID, KMS.TypeID As KTypeId

	                    FROM FBookingAcknowledgeChild BAC
	                    LEFT JOIN FBookingAcknowledge BA ON BA.FBAckID=BAC.AcknowledgeID
	                    LEFT JOIN FreeConceptMaster FCM ON FCM.BookingChildID = BAC.BookingChildID  AND FCM.ConsumptionID = BAC.ConsumptionID

	                    LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster SBM ON SBM.BookingID = BAC.BookingID
	                    LEFT JOIN {DbNames.EPYSL}..SampleBookingConsumption SBC ON SBC.BookingID = SBM.BookingID AND SBC.ConsumptionID = BAC.ConsumptionID

	                    LEFT JOIN {DbNames.EPYSL}..BookingMaster BM ON BM.BookingID = BAC.BookingID
	                    LEFT JOIN {DbNames.EPYSL}..BookingChild BCC ON BCC.BookingID = BM.BookingID AND BCC.ConsumptionID=BAC.ConsumptionID
	                    LEFT JOIN {DbNames.EPYSL}..BOMConsumption BCCC ON BCCC.ConsumptionID=BAC.ConsumptionID

	                    LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = BAC.ItemMasterID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = BAC.SubGroupID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV On ISV.SegmentValueID = BAC.A1ValueID
	                    LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = BAC.YarnBrandID
	                    LEFT JOIN FabricTechnicalName T ON T.TechnicalNameId = BAC.TechnicalNameID
	                    LEFT JOIN KnittingMachineSubClass KMS ON KMS.SubClassID = BAC.MachineTypeId
	                    LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ETV2 ON ETV2.ValueID = BAC.BrandID
	                    LEFT Join {DbNames.EPYSL}..Contacts Supplier On Supplier.ContactID = SBM.SupplierID
                    )
                    SELECT * FROM F WHERE BookingNo = '{bookingNo}' AND SubGroupID IN (11, 12);

                    ;With A As(
	                    select Min(FBP.FBAChildPlanningID) FBAChildPlanningID, FBP.BookingChildID, FBP.AcknowledgeID, FBP.CriteriaID
	                    From FBAChildPlanning FBP
	                    LEFT JOIN FBookingAcknowledge FBA ON FBA.FBAckID = FBP.AcknowledgeID
	                    Where FBA.BookingNo = '{bookingNo}'
	                    Group By BookingChildID, AcknowledgeID, CriteriaID
                    )
                    SELECT cp.BookingChildID, CR.CriteriaName, Sum(CR.ProcessTime)TotalTime, CriteriaIDs = String_Agg(CR.CriteriaID,',')
                    FROM A CP
                    INNER JOIN BDSCriteria_HK CR ON CR.CriteriaID = CP.CriteriaID
                    GROUP BY cp.BookingChildID, CR.CriteriaName

                    --Technical Name
                    SELECT Cast(T.TechnicalNameId As varchar) id, T.TechnicalName [text], ISNULL(ST.[Days], 0) [desc], Cast(SC.SubClassID as varchar) additionalValue
                    FROM FabricTechnicalName T
                    LEFT JOIN FabricTechnicalNameKMachineSubClass SC ON SC.TechnicalNameID = T.TechnicalNameId
                    LEFT JOIN KnittingMachineStructureType_HK ST ON ST.StructureTypeID = SC.StructureTypeID
                    Group By T.TechnicalNameId, T.TechnicalName, ST.Days, SC.SubClassID;

                    --M/c type
                    ;SELECT CAST(a.MachineSubClassID AS varchar) [id], b.SubClassName [text], b.TypeID [desc], c.TypeName additionalValue
                    FROM KnittingMachine a
                    INNER JOIN KnittingMachineSubClass b ON b.SubClassID = a.MachineSubClassID
                    Inner Join KnittingMachineType c On c.TypeID = b.TypeID
                    --Where c.TypeName != 'Flat Bed'
                    GROUP BY a.MachineSubClassID, b.SubClassName, b.TypeID, c.TypeName;

                    --CriteriaNames
                        ;SELECT CriteriaName,CriteriaSeqNo,(CASE WHEN CriteriaName  IN('Batch Preparation','Quality Check') THEN '1'ELSE'0'END) AS TotalTime
                    FROM BDSCriteria_HK --WHERE CriteriaName NOT IN('Batch Preparation','Testing')
                    GROUP BY CriteriaSeqNo,CriteriaName order by CriteriaSeqNo,CriteriaName;

                    --FBAChildPlannings
                    ;SELECT * FROM BDSCriteria_HK order by CriteriaSeqNo, OperationSeqNo, CriteriaName;

                    --FBookingAcknowledgeChildDetails (Fabric)
                    SELECT *
                    FROM FBookingAcknowledgeChildDetails A
                    LEFT JOIN FBookingAcknowledgeChild B ON B.BookingChildID=A.BookingChildID
                    LEFT JOIN FBookingAcknowledge FBA ON FBA.FBAckID=B.AcknowledgeID
                    WHERE FBA.BookingNo = '{bookingNo}' AND B.SubGroupID IN (1);

                    --FBookingAcknowledgeChildDetails (Collar & Cuff)
                    SELECT *
                    FROM FBookingAcknowledgeChildDetails A
                    LEFT JOIN FBookingAcknowledgeChild B ON B.BookingChildID=A.BookingChildID
                    LEFT JOIN FBookingAcknowledge FBA ON FBA.FBAckID=B.AcknowledgeID
                    WHERE FBA.BookingNo = '{bookingNo}' AND B.SubGroupID IN (11,12);

                    --Brand List
                    ;SELECT DISTINCT(KM.BrandID) [id], EV.ValueName [text]
                    FROM KnittingMachine KM
                    LEFT JOIN KnittingUnit KU ON KU.KnittingUnitID = KM.KnittingUnitID
                    LEFT JOIN {DbNames.EPYSL}..EntityTypeValue EV ON ValueID = KM.BrandID
                    ORDER BY [text];";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                FBookingAcknowledge data = records.Read<FBookingAcknowledge>().FirstOrDefault();
                Guard.Against.NullObject(data);

                List<FBookingAcknowledgeChild> bookingChilds = records.Read<FBookingAcknowledgeChild>().ToList();
                List<FBookingAcknowledgeChild> bookingChildsCollarCuff = records.Read<FBookingAcknowledgeChild>().ToList();
                //List<FBAChildPlanning> childPlanningCriterias = records.Read<FBAChildPlanning>().ToList();
                List<FBAChildPlanning> childPlanningCriteriaNameWithIds = records.Read<FBAChildPlanning>().ToList();

                data.TechnicalNameList = await records.ReadAsync<Select2OptionModel>();
                List<Select2OptionModel> mcTypeList = records.Read<Select2OptionModel>().ToList();
                data.MCTypeForFabricList = mcTypeList.Where(x => x.additionalValue != "Flat Bed");
                data.MCTypeForOtherList = mcTypeList.Where(x => x.additionalValue == "Flat Bed");

                List<FBookingAcknowledgeChild> criteriaNames = records.Read<FBookingAcknowledgeChild>().ToList();
                List<FBAChildPlanning> fbaChildPlannings = records.Read<FBAChildPlanning>().ToList();

                var fabricChildDetails = records.Read<FBookingAcknowledgeChildDetails>().ToList();
                var collarCuffChildDetails = records.Read<FBookingAcknowledgeChildDetails>().ToList();
                data.KnittingMachines = records.Read<KnittingMachine>().ToList();

                bookingChilds.ForEach(bc =>
                {
                    bc.CriteriaNames = criteriaNames;
                    bc.FBAChildPlannings = fbaChildPlannings;
                    bc.FBAChildPlanningsWithIds = childPlanningCriteriaNameWithIds.Where(x => x.BookingChildID == bc.BookingChildID).ToList();
                    bc.ChildDetails = fabricChildDetails.Where(y => y.BookingChildID == bc.BookingChildID).ToList();
                    //bc.CriteriaIDs = string.Join(",", childPlanningCriterias.Where(x => x.BookingChildID == bc.BookingChildID).ToList().Select(y => y.CriteriaID));
                });
                bookingChildsCollarCuff.ForEach(bc =>
                {
                    bc.CriteriaNames = criteriaNames;
                    bc.FBAChildPlannings = fbaChildPlannings;
                    bc.FBAChildPlanningsWithIds = childPlanningCriteriaNameWithIds.Where(x => x.BookingChildID == bc.BookingChildID).ToList();
                    bc.ChildDetails = collarCuffChildDetails.Where(y => y.BookingChildID == bc.BookingChildID).ToList();
                    //bc.CriteriaIDs = string.Join(",", childPlanningCriterias.Where(x => x.BookingChildID == bc.BookingChildID).ToList().Select(y => y.CriteriaID));
                });

                data.FBookingChild = bookingChilds.Where(x => x.SubGroupID == 1).ToList();
                data.FBookingChildCollor = bookingChildsCollarCuff.Where(x => x.SubGroupID == 11).ToList();
                data.FBookingChildCuff = bookingChildsCollarCuff.Where(x => x.SubGroupID == 12).ToList();
                data.HasFabric = data.FBookingChild.Count() > 0 ? true : false;
                data.HasCollar = data.FBookingChildCollor.Count() > 0 ? true : false;
                data.HasCuff = data.FBookingChildCuff.Count() > 0 ? true : false;

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
        public async Task<List<FBookingAcknowledgeChild>> GetDataForAcknowledgColourAsync(int bookingId)
        {
            //var query =
            //   $@";With A AS(
            //Select BookingID, BookingChildID, ItemMasterID, DeliveryDate
            //From FBookingAcknowledgeChild
            //Where BookingID ={bookingId}
            //   )
            //   Select ISV.SegmentValue Color, Max(A.DeliveryDate) DeliveryDate
            //   From A
            //   Inner Join FBookingAcknowledgeChildDetails B ON B.BookingChildID = A.BookingChildID
            //   Inner Join {DbNames.EPYSL}..ItemSegmentValue ISV On ISV.SegmentValueID = B.ColorID
            //   Group by ISV.SegmentValue;";

            var query =
              $@";With A AS(
	                Select BookingID, BookingChildID, ItemMasterID, DeliveryDate
	                From FBookingAcknowledgeChild
	                Where BookingID = {bookingId}
                ),
                R AS
                (
	                Select Color = CASE WHEN FBA.IsSample = 1 THEN SBC.GarmentsColor ELSE BCT.GmtColor END, Max(A.DeliveryDate) DeliveryDate
	                From A
	                Inner Join FBookingAcknowledgeChildDetails B ON B.BookingChildID = A.BookingChildID
	                LEFT JOIN FBookingAcknowledgeChild FBC ON FBC.BookingChildID = B.BookingChildID
	                LEFT JOIN FBookingAcknowledge FBA ON FBA.FBAckID = FBC.AcknowledgeID
	                LEFT JOIN {DbNames.EPYSL}..BookingChildText BCT On BCT.BookingID = B.BookingID and BCT.BookingChildID = B.BookingChildID
	                LEFT JOIN {DbNames.EPYSL}..SampleBookingConsumptionText SBC On SBC.BookingID = B.BookingID and SBC.ConsumptionID = B.ConsumptionID
	                Group by SBC.GarmentsColor, BCT.GmtColor, FBA.IsSample
                )
                SELECT * FROM R WHERE ISNULL(Color,'') <> ''";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                List<FBookingAcknowledgeChild> data = records.Read<FBookingAcknowledgeChild>().ToList();

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
        public async Task SaveAsync(int userId, FBookingAcknowledge entity, List<FBookingAcknowledgeChild> entityChilds,
            List<FBookingAcknowledgeChildAddProcess> entityChildAddProcess, List<FBookingAcknowledgeChildDetails> entityChildDetails,
            List<FBookingAcknowledgeChildGarmentPart> entityChildsGpart, List<FBookingAcknowledgeChildProcess> entityChildsProcess,
            List<FBookingAcknowledgeChildText> entityChildsText, List<FBookingAcknowledgeChildDistribution> entityChildsDistribution, List<FBookingAcknowledgeChildYarnSubBrand> entityChildsYarnSubBrand,
            List<FBookingAcknowledgeImage> entityChildsImage, List<BDSDependentTNACalander> BDCalander, int isBDS,
            List<FreeConceptMaster> entityFreeConcepts = null, List<FreeConceptMRMaster> entityFreeMRs = null, List<FBookingAcknowledgementLiabilityDistribution> entityChildsLiabilitiesDistribution = null, List<FabricBookingAcknowledge> entityFBA = null, List<FBookingAcknowledgementYarnLiability> entityFBYL = null)
        {
            SqlTransaction transaction = null;
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                if (entity.FBAckID > 0) entity.EntityState = EntityState.Modified;
                else entity.FBAckID = await _service.GetMaxIdAsync(TableNames.FBBOOKING_ACKNOWLEDGE);

                if (entityFBA == null) entityFBA = new List<FabricBookingAcknowledge>();
                if (entityChildsLiabilitiesDistribution == null) entityChildsLiabilitiesDistribution = new List<FBookingAcknowledgementLiabilityDistribution>();

                if (entityFBA.Count > 0)
                {
                    if (entityFBA[0].AcknowledgeID > 0) entity.EntityState = EntityState.Modified;
                    else entityFBA[0].AcknowledgeID = await _service.GetMaxIdAsync(TableNames.FabricBookingAcknowledge);
                }
                if (entityFBYL == null) entityFBYL = new List<FBookingAcknowledgementYarnLiability>();
                int maxConceptMRId = 0;
                int maxConceptMRChildId = 0;
                int maxFBAckChildDetailId = 0;
                int maxConceptId = 0;
                int maxConceptChildId = 0;

                List<FreeConceptChildColor> childColors = new List<FreeConceptChildColor>();
                List<FreeConceptMRChild> childMRs = new List<FreeConceptMRChild>();

                if (entity.EntityState == EntityState.Modified)
                {
                    List<FreeConceptMaster> newFreeConceptMasterList = new List<FreeConceptMaster>();
                    List<FreeConceptChildColor> newFreeConceptChildColorList = new List<FreeConceptChildColor>();
                    List<FreeConceptMRMaster> newFreeConceptMRList = new List<FreeConceptMRMaster>();
                    List<FreeConceptMRChild> newFreeConceptMRChildList = new List<FreeConceptMRChild>();

                    int maxChildId = await _service.GetMaxIdAsync(TableNames.FBBOOKING_ACKNOWLEDGE_CHILD, entityChilds.Count(x => x.EntityState == EntityState.Added));
                    int maxChildAddProcessId = await _service.GetMaxIdAsync(TableNames.FBBOOKING_ACKNOWLEDGE_CHILD_ADD_PROCESS, entityChildAddProcess.Count(x => x.EntityState == EntityState.Added));
                    int maxChildDetailsId = await _service.GetMaxIdAsync(TableNames.FBBOOKING_ACKNOWLEDGE_CHILD_DETAILS, entityChildDetails.Count(x => x.EntityState == EntityState.Added));
                    int maxChildGarmentPartId = await _service.GetMaxIdAsync(TableNames.FBBOOKING_ACKNOWLEDGE_CHILD_GARMENT_PART, entityChildsGpart.Count(x => x.EntityState == EntityState.Added));
                    int maxChildProcessId = await _service.GetMaxIdAsync(TableNames.FBBOOKING_ACKNOWLEDGE_CHILD_PROCESS, entityChildsProcess.Count(x => x.EntityState == EntityState.Added));
                    int maxChildTextId = await _service.GetMaxIdAsync(TableNames.FBBOOKING_ACKNOWLEDGE_CHILD_TEXT, entityChildsText.Count(x => x.EntityState == EntityState.Added));
                    int maxChildDistributionId = await _service.GetMaxIdAsync(TableNames.FBBOOKING_ACKNOWLEDGE_CHILD_DISTRIBUTION, entityChildsDistribution.Count(x => x.EntityState == EntityState.Added));
                    int maxChildYarnSubBrandId = await _service.GetMaxIdAsync(TableNames.FBBOOKING_ACKNOWLEDGE_CHILD_YARN_SUB_BRAND, entityChildsYarnSubBrand.Count(x => x.EntityState == EntityState.Added));
                    int maxPlanningId = await _service.GetMaxIdAsync(TableNames.FBOOKING_ACKNOWLEDGE_CHILD_PLANNING, entityChilds.Sum(x => x.FBAChildPlannings.Count(y => y.EntityState == EntityState.Added)));
                    int maxBDSEventID = await _service.GetMaxIdAsync(TableNames.BDS_Dependent_TNA_Calander, BDCalander.Count(x => x.EntityState == EntityState.Added));
                    int maxLCID = await _service.GetMaxIdAsync(TableNames.FBBOOKING_ACKNOWLEDGE_LIABILITIES_DISTRIBUTION, entityChildsLiabilitiesDistribution.Count(x => x.EntityState == EntityState.Added));
                    int maxYLID = await _service.GetMaxIdAsync(TableNames.FBBOOKING_ACKNOWLEDGE_YARN_LIABILITIES, entityChildsLiabilitiesDistribution.Count(x => x.EntityState == EntityState.Added));

                    maxConceptId = await _service.GetMaxIdAsync(TableNames.RND_FREE_CONCEPT_MASTER, entityChilds.Count(x => x.EntityState == EntityState.Added));
                    maxConceptChildId = await _service.GetMaxIdAsync(TableNames.RND_FREE_CONCEPT_CHILD_COLOR, entityChilds.Count(x => x.EntityState == EntityState.Added));

                    foreach (var item in entityChilds)
                    {
                        int bookingChildId = 0;
                        if (item.EntityState == EntityState.Added) bookingChildId = item.BookingChildID;
                        entityChildAddProcess.Where(x => x.BookingChildID == item.BookingChildID && x.EntityState == EntityState.Added).ToList().ForEach(x =>
                        {
                            x.BookingCAddProcessID = maxChildAddProcessId++;
                            x.BookingChildID = bookingChildId > 0 ? bookingChildId : item.BookingChildID;
                        });
                        entityChildDetails.Where(x => x.BookingChildID == item.BookingChildID && x.EntityState == EntityState.Added).ToList().ForEach(x =>
                        {
                            x.BookingCDetailsID = maxChildDetailsId++;
                            x.BookingChildID = bookingChildId > 0 ? bookingChildId : item.BookingChildID;
                        });
                        entityChildsGpart.Where(x => x.BookingChildID == item.BookingChildID && x.EntityState == EntityState.Added).ToList().ForEach(x =>
                        {
                            x.BookingCGPID = maxChildGarmentPartId++;
                            x.BookingChildID = bookingChildId > 0 ? bookingChildId : item.BookingChildID;
                        });
                        entityChildsProcess.Where(x => x.BookingChildID == item.BookingChildID && x.EntityState == EntityState.Added).ToList().ForEach(x =>
                        {
                            x.BookingCProcessID = maxChildProcessId++;
                            x.BookingChildID = bookingChildId > 0 ? bookingChildId : item.BookingChildID;
                        });
                        entityChildsText.Where(x => x.BookingChildID == item.BookingChildID && x.EntityState == EntityState.Added).ToList().ForEach(x =>
                        {
                            x.TextID = maxChildTextId++;
                            x.BookingChildID = bookingChildId > 0 ? bookingChildId : item.BookingChildID;
                        });
                        entityChildsDistribution.Where(x => x.BookingChildID == item.BookingChildID && x.EntityState == EntityState.Added).ToList().ForEach(x =>
                        {
                            x.DistributionID = maxChildDistributionId++;
                            x.BookingChildID = bookingChildId > 0 ? bookingChildId : item.BookingChildID;
                        });
                        entityChildsYarnSubBrand.Where(x => x.BookingChildID == item.BookingChildID && x.EntityState == EntityState.Added).ToList().ForEach(x =>
                        {
                            x.BookingCYSubBrandID = maxChildYarnSubBrandId++;
                            x.BookingChildID = bookingChildId > 0 ? bookingChildId : item.BookingChildID;
                        });
                        entityChildsLiabilitiesDistribution.Where(x => x.BookingChildID == item.BookingChildID && x.EntityState == EntityState.Added).ToList().ForEach(x =>
                        {
                            x.LChildID = maxLCID++;
                            //x.BookingChildID = bookingChildId > 0 ? bookingChildId : item.BookingChildID;
                        });
                        item.FBAChildPlannings.Where(x => x.BookingChildID == item.BookingChildID && x.EntityState == EntityState.Added).ToList().ForEach(x =>
                        {
                            x.FBAChildPlanningID = maxPlanningId++;
                            x.BookingChildID = bookingChildId > 0 ? bookingChildId : item.BookingChildID;
                        });

                        if (item.EntityState == EntityState.Added) item.BookingChildID = bookingChildId;

                        #region FreeConcept & FreeConceptMR
                        if (item.EntityState != EntityState.Deleted)
                        {
                            int conceptID = 0;
                            string conceptNo = "";

                            if (entity.PageName == "BulkBookingKnittingInfo")
                            {
                                var freeConceptList = entityFreeConcepts.Where(x => x.ItemMasterID == item.ItemMasterID).ToList();
                                foreach (FreeConceptMaster fcm in freeConceptList)
                                {
                                    conceptID = fcm.ConceptID;
                                    conceptNo = fcm.ConceptNo;
                                    var childs = fcm.ChildColors;

                                    var newFCM = this.GetFreeConceptMaster(item, entity, isBDS, conceptID, conceptNo);

                                    childs.ForEach(colorChild =>
                                    {
                                        colorChild = this.GetFreeConceptColorChild(item, newFCM, colorChild.CCColorID);
                                        colorChild.EntityState = EntityState.Modified;
                                        newFreeConceptChildColorList.Add(colorChild);
                                    });

                                    fcm.EntityState = EntityState.Modified;
                                    newFreeConceptMasterList.Add(fcm);
                                }
                            }
                            else
                            {
                                var obj = entityFreeConcepts.Find(x => x.BookingChildID == item.BookingChildID);
                                int i = entityFreeConcepts.Count();
                                if (obj != null)
                                {
                                    conceptID = obj.ConceptID;
                                    conceptNo = obj.ConceptNo;
                                    var childs = obj.ChildColors;

                                    obj = this.GetFreeConceptMaster(item, entity, isBDS, conceptID, conceptNo);

                                    childs.ForEach(colorChild =>
                                    {
                                        colorChild = this.GetFreeConceptColorChild(item, obj, colorChild.CCColorID);
                                        colorChild.EntityState = EntityState.Modified;
                                        newFreeConceptChildColorList.Add(colorChild);
                                    });

                                    obj.EntityState = EntityState.Modified;
                                    newFreeConceptMasterList.Add(obj);
                                }
                                else
                                {
                                    if (i > 0) i++;
                                    conceptNo = (i > 0 ? entity.BookingNo + '_' + i : entity.BookingNo);
                                    conceptID = maxConceptId++;
                                    obj = this.GetFreeConceptMaster(item, entity, isBDS, conceptID, conceptNo);
                                    obj.EntityState = EntityState.Added;
                                    newFreeConceptMasterList.Add(obj);

                                    if (!string.IsNullOrEmpty(item.Color))
                                    {
                                        int conceptChildId = maxConceptChildId++;
                                        var colorChild = this.GetFreeConceptColorChild(item, obj, conceptChildId);
                                        colorChild.EntityState = EntityState.Added;
                                        newFreeConceptChildColorList.Add(colorChild);
                                    }
                                }
                            }
                        }
                        #endregion FreeConcept & FreeConceptMR
                    }
                    entityFBYL.Where(x => x.EntityState == EntityState.Added).ToList().ForEach(x =>
                    {
                        x.YLChildID = maxYLID++;

                    });
                    BDCalander.Where(x => x.EntityState == EntityState.Added).ToList().ForEach(x =>
                    {
                        x.BDSEventID = maxBDSEventID++;
                    });
                    entityFreeConcepts = newFreeConceptMasterList;

                    List<FreeConceptMaster> lFrreCM = new List<FreeConceptMaster>();
                    foreach (var items in entityFreeConcepts)
                    {
                        var objFrreCM = entityChilds.Find(x => x.BookingChildID == items.BookingChildID);
                        if (objFrreCM == null)
                        {
                            items.IsActive = true;
                            items.EntityState = EntityState.Modified;
                            lFrreCM.Add(items);
                        }
                    }

                    childColors = newFreeConceptChildColorList;
                    entityFreeMRs = newFreeConceptMRList;
                    childMRs = newFreeConceptMRChildList;
                }
                else
                {
                    int maxChildId = await _service.GetMaxIdAsync(TableNames.FBBOOKING_ACKNOWLEDGE_CHILD, entityChilds.Count);
                    int maxChildAddProcessId = await _service.GetMaxIdAsync(TableNames.FBBOOKING_ACKNOWLEDGE_CHILD_ADD_PROCESS, entityChildAddProcess.Count);
                    int maxChildDetailsId = await _service.GetMaxIdAsync(TableNames.FBBOOKING_ACKNOWLEDGE_CHILD_DETAILS, entityChildDetails.Count);
                    int maxChildGarmentPartId = await _service.GetMaxIdAsync(TableNames.FBBOOKING_ACKNOWLEDGE_CHILD_GARMENT_PART, entityChildsGpart.Count);
                    int maxChildProcessId = await _service.GetMaxIdAsync(TableNames.FBBOOKING_ACKNOWLEDGE_CHILD_PROCESS, entityChildsProcess.Count);
                    int maxChildTextId = await _service.GetMaxIdAsync(TableNames.FBBOOKING_ACKNOWLEDGE_CHILD_TEXT, entityChildsText.Count);
                    int maxChildDistributionId = await _service.GetMaxIdAsync(TableNames.FBBOOKING_ACKNOWLEDGE_CHILD_DISTRIBUTION, entityChildsDistribution.Count);
                    int maxChildYarnSubBrandId = await _service.GetMaxIdAsync(TableNames.FBBOOKING_ACKNOWLEDGE_CHILD_YARN_SUB_BRAND, entityChildsYarnSubBrand.Count);
                    int maxImageId = await _service.GetMaxIdAsync(TableNames.FBBOOKING_ACKNOWLEDGE_IMAGE, entityChildsImage.Count);
                    int maxLCID = await _service.GetMaxIdAsync(TableNames.FBBOOKING_ACKNOWLEDGE_LIABILITIES_DISTRIBUTION, entityChildsLiabilitiesDistribution.Count(x => x.EntityState == EntityState.Added));
                    int maxPlanningId = await _service.GetMaxIdAsync(TableNames.FBOOKING_ACKNOWLEDGE_CHILD_PLANNING, entityChilds.Sum(x => x.FBAChildPlannings.Count));
                    int maxBDSEventID = await _service.GetMaxIdAsync(TableNames.BDS_Dependent_TNA_Calander, BDCalander.Count);
                    int maxYLID = await _service.GetMaxIdAsync(TableNames.FBBOOKING_ACKNOWLEDGE_YARN_LIABILITIES, entityChildsLiabilitiesDistribution.Count(x => x.EntityState == EntityState.Added));
                    if (isBDS == 1 || isBDS == 3)
                    {
                        maxConceptId = await _service.GetMaxIdAsync(TableNames.RND_FREE_CONCEPT_MASTER, entityChilds.Count);
                        maxConceptChildId = await _service.GetMaxIdAsync(TableNames.RND_FREE_CONCEPT_CHILD_COLOR, entityChilds.Count);
                    }
                    entityFreeConcepts = new List<FreeConceptMaster>();
                    entityFreeMRs = new List<FreeConceptMRMaster>();
                    childColors = new List<FreeConceptChildColor>();
                    childMRs = new List<FreeConceptMRChild>();

                    int i = 0;
                    foreach (FBookingAcknowledgeChild item in entityChilds)
                    {
                        int bChildID = maxChildId++; //item.BookingChildID;
                        item.AcknowledgeID = entity.FBAckID;
                        item.AddedBy = entity.AddedBy;
                        if (item.SubGroupID == 11 || item.SubGroupID == 12) item.MachineDia = 0;

                        #region BDSDependentTNACalander

                        foreach (var addProcess in BDCalander.Where(x => x.BookingChildID == item.BookingChildID).ToList())
                        {
                            addProcess.BDSEventID = maxBDSEventID++;
                            addProcess.BookingID = item.BookingID;
                            addProcess.BookingChildID = bChildID;
                        }

                        #endregion BDSDependentTNACalander

                        //   item.BookingChildID = bChildID;

                        #region FBookingAcknowledgeChildAddProcess

                        foreach (var addProcess in entityChildAddProcess)
                        {
                            if (item.ConsumptionID == addProcess.ConsumptionID && item.BookingID == addProcess.BookingID)
                            {
                                addProcess.BookingCAddProcessID = maxChildAddProcessId++;
                                addProcess.BookingChildID = item.BookingChildID;
                            }
                        }

                        #endregion FBookingAcknowledgeChildAddProcess
                        entityChildsLiabilitiesDistribution.Where(x => x.LChildID == 0 && x.EntityState == EntityState.Added).ToList().ForEach(x =>
                        {
                            x.LChildID = maxLCID++;
                        });
                        #region FBookingAcknowledgeChildDetails

                        foreach (var details in entityChildDetails)
                        {
                            if (item.ConsumptionID == details.ConsumptionID && item.BookingID == details.BookingID)
                            {
                                details.BookingCDetailsID = maxChildDetailsId++;
                                details.AddedBy = entity.AddedBy;
                                details.DateAdded = DateTime.Now;
                                details.BookingChildID = item.BookingChildID;
                            }
                        }

                        #endregion FBookingAcknowledgeChildDetails

                        #region FBookingAcknowledgeChildGarmentPart

                        foreach (var garmentPart in entityChildsGpart)
                        {
                            if (item.ConsumptionID == garmentPart.ConsumptionID && item.BookingID == garmentPart.BookingID)
                            {
                                garmentPart.BookingCGPID = maxChildGarmentPartId++;
                                garmentPart.BookingChildID = item.BookingChildID;
                            }
                        }

                        #endregion FBookingAcknowledgeChildGarmentPart

                        #region FBookingAcknowledgeChildProcess

                        foreach (var process in entityChildsProcess)
                        {
                            if (item.ConsumptionID == process.ConsumptionID && item.BookingID == process.BookingID)
                            {
                                process.BookingCProcessID = maxChildProcessId++;
                                process.BookingChildID = item.BookingChildID;
                            }
                        }

                        #endregion FBookingAcknowledgeChildProcess

                        #region FBookingAcknowledgeChildText

                        foreach (var text in entityChildsText)
                        {
                            if (item.ConsumptionID == text.ConsumptionID && item.BookingID == text.BookingID)
                            {
                                text.TextID = maxChildTextId++;
                                text.BookingChildID = item.BookingChildID;
                            }
                        }

                        #endregion FBookingAcknowledgeChildText

                        #region FBookingAcknowledgeChildDistribution

                        foreach (var distribution in entityChildsDistribution)
                        {
                            if (item.ConsumptionID == distribution.ConsumptionID && item.BookingID == distribution.BookingID)
                            {
                                distribution.DistributionID = maxChildDistributionId++;
                                distribution.BookingChildID = item.BookingChildID;
                            }
                        }

                        #endregion FBookingAcknowledgeChildDistribution

                        #region FBookingAcknowledgeChildYarnSubBrand

                        foreach (var subBrand in entityChildsYarnSubBrand)
                        {
                            if (item.ConsumptionID == subBrand.ConsumptionID && item.BookingID == subBrand.BookingID)
                            {
                                subBrand.BookingCYSubBrandID = maxChildYarnSubBrandId++;
                                subBrand.BookingChildID = item.BookingChildID;
                            }
                        }

                        #endregion FBookingAcknowledgeChildYarnSubBrand

                        #region FBAChildPlanning

                        foreach (FBAChildPlanning planning in item.FBAChildPlannings)
                        {
                            planning.FBAChildPlanningID = maxPlanningId++;
                            planning.BookingChildID = item.BookingChildID;
                            planning.AcknowledgeID = entity.FBAckID;
                        }

                        #endregion FBAChildPlanning

                        if (item.EntityState != EntityState.Deleted)
                        {
                            if (entity.EntityState == EntityState.Added && (isBDS == 1 || isBDS == 3))
                            {
                                FreeConceptMaster concept = new FreeConceptMaster();
                                FreeConceptMRMaster conceptMR = new FreeConceptMRMaster();

                                int conceptID = maxConceptId++;
                                string conceptNo = (i > 0 ? entity.BookingNo + '_' + i : entity.BookingNo);
                                var freeConcept = this.GetFreeConceptMaster(item, entity, isBDS, conceptID, conceptNo);
                                entityFreeConcepts.Add(freeConcept);

                                if (!string.IsNullOrEmpty(freeConcept.Color))
                                {
                                    int conceptChildId = maxConceptChildId++;
                                    var colorChild = this.GetFreeConceptColorChild(item, freeConcept, conceptChildId);
                                    childColors.Add(colorChild);
                                }

                                if (isBDS == 2 && item.ChildItems.Count() > 0)
                                {
                                    int fCMRMasterID = maxConceptMRId++;
                                    var freeConceptMR = this.GetFreeConceptMRMaster(entity, fCMRMasterID, conceptID, isBDS);
                                    entityFreeMRs.Add(freeConceptMR);

                                    item.ChildItems.ForEach(ybci =>
                                    {
                                        int mRChildId = maxConceptMRChildId++;
                                        var fBAChildDetail = this.GetFBookingAcknowledgeChildDetail(maxFBAckChildDetailId++, item, entity, ybci);
                                        entityChildDetails.Add(fBAChildDetail);
                                        var mrChild = this.GetFreeConceptMRChild(mRChildId, freeConceptMR, ybci);
                                        childMRs.Add(mrChild);
                                    });
                                }
                                i++;
                            }
                        }
                    }
                    entityFBYL.Where(x => x.EntityState == EntityState.Added).ToList().ForEach(x =>
                    {
                        x.YLChildID = maxYLID++;

                    });
                    if (isBDS == 2 && entity.EntityState == EntityState.Added)
                    {
                        var tempObjList = new List<dynamic>();
                        var maxChildCount = 0;

                        foreach (FBookingAcknowledgeChild item in entityChilds)
                        {
                            maxChildCount += item.ChildItems.Count();

                            var tObj = tempObjList.Find(x => x.BookingID == item.BookingID &&
                                                           x.ItemMasterID == item.ItemMasterID &&
                                                           x.SubGroupID == item.SubGroupID);
                            if (tObj == null)
                            {
                                dynamic tempObj = new System.Dynamic.ExpandoObject();
                                tempObj.BookingID = item.BookingID;
                                tempObj.ItemMasterID = item.ItemMasterID;
                                tempObj.SubGroupID = item.SubGroupID;
                                tempObjList.Add(tempObj);
                            }
                        }

                        maxConceptId = await _service.GetMaxIdAsync(TableNames.RND_FREE_CONCEPT_MASTER, tempObjList.Count);
                        maxConceptChildId = await _service.GetMaxIdAsync(TableNames.RND_FREE_CONCEPT_CHILD_COLOR, tempObjList.Count);

                        if (isBDS == 2)
                        {
                            entityChildDetails = new List<FBookingAcknowledgeChildDetails>();
                            maxConceptMRId = await _service.GetMaxIdAsync(TableNames.RND_FREE_CONCEPT_MR_MASTER, tempObjList.Count);
                            maxConceptMRChildId = await _service.GetMaxIdAsync(TableNames.RND_FREE_CONCEPT_MR_CHILD, maxChildCount);
                            maxFBAckChildDetailId = await _service.GetMaxIdAsync(TableNames.FBBOOKING_ACKNOWLEDGE_CHILD_DETAILS, maxChildCount);
                        }
                        i = 0;
                        foreach (var tempObj in tempObjList)
                        {
                            var fbaChilds = entityChilds.Where(x => x.BookingID == tempObj.BookingID &&
                                                                    x.ItemMasterID == tempObj.ItemMasterID &&
                                                                    x.SubGroupID == tempObj.SubGroupID).ToList();

                            List<YarnBookingChildItem> ChildItems = new List<YarnBookingChildItem>();
                            fbaChilds.ForEach(x => ChildItems.AddRange(x.ChildItems));

                            #region FreeConceptMaster

                            int conceptID = maxConceptId++;
                            string conceptNo = (i > 0 ? entity.BookingNo + '_' + i : entity.BookingNo);

                            decimal height = 0,
                                    length = 0;

                            fbaChilds.ForEach(childObj =>
                            {
                                height += childObj.Height.IsNotNullOrEmpty() ? Convert.ToDecimal(childObj.Height) : 0;
                                length += childObj.Length.IsNotNullOrEmpty() ? Convert.ToDecimal(childObj.Length) : 0;
                            });

                            FBookingAcknowledgeChild fbaChild = new FBookingAcknowledgeChild();
                            fbaChild = fbaChilds.FirstOrDefault();
                            fbaChild.BookingQty = fbaChilds.Sum(x => x.BookingQty);
                            fbaChild.Height = height.ToString();
                            fbaChild.Length = length.ToString();

                            var freeConcept = this.GetFreeConceptMaster(fbaChild, entity, isBDS, conceptID, conceptNo, true); //true means group by ItemMasterID and BookingID
                            entityFreeConcepts.Add(freeConcept);

                            #endregion FreeConceptMaster

                            #region FreeConceptColorChild

                            if (!string.IsNullOrEmpty(freeConcept.Color))
                            {
                                int conceptChildId = maxConceptChildId++;
                                var colorChild = this.GetFreeConceptColorChild(fbaChild, freeConcept, conceptChildId);
                                childColors.Add(colorChild);
                            }

                            #endregion FreeConceptColorChild

                            if (isBDS == 2 && ChildItems.Count() > 0)
                            {
                                int fCMRMasterID = maxConceptMRId++;
                                var freeConceptMR = this.GetFreeConceptMRMaster(entity, fCMRMasterID, conceptID, isBDS);
                                entityFreeMRs.Add(freeConceptMR);

                                ChildItems.ForEach(ybci =>
                                {
                                    int mRChildId = maxConceptMRChildId++;
                                    var fBAChildDetail = this.GetFBookingAcknowledgeChildDetail(maxFBAckChildDetailId++, fbaChild, entity, ybci);
                                    entityChildDetails.Add(fBAChildDetail);
                                    ybci.ItemMasterID = tempObj.ItemMasterID;
                                    var mrChild = this.GetFreeConceptMRChild(mRChildId, freeConceptMR, ybci);
                                    childMRs.Add(mrChild);
                                });
                            }
                            i++;
                        }
                    }

                    #region FBookingAcknowledgeImage

                    foreach (var img in entityChildsImage)
                    {
                        img.ChildImgID = maxImageId++;
                    }

                    #endregion FBookingAcknowledgeImage
                }

                //entity.BookingDate = DateTime.Now;
                entity.WithoutOB = false;
                if (isBDS == 1 || isBDS == 3)
                {
                    entity.WithoutOB = false;
                }
                else if (isBDS == 2 && entity.ExportOrderID == 0)
                {
                    entity.WithoutOB = false;
                }
                else if (isBDS == 2 && entity.ExportOrderID > 0)
                {
                    entity.WithoutOB = true;
                }

                await _service.SaveSingleAsync(entity, transaction);

                List<FBookingAcknowledgeChild> SaveEntityChilds = new List<FBookingAcknowledgeChild>();
                List<FBookingAcknowledgeChild> DeleteEntityChilds = new List<FBookingAcknowledgeChild>();
                SaveEntityChilds = entityChilds.Where(x => x.EntityState != EntityState.Deleted).ToList();
                DeleteEntityChilds = entityChilds.Where(x => x.EntityState == EntityState.Deleted).ToList();

                List<FBookingAcknowledgeChildAddProcess> SaveEntityChildAddProcess = new List<FBookingAcknowledgeChildAddProcess>();
                List<FBookingAcknowledgeChildAddProcess> DeleteEntityChildAddProcess = new List<FBookingAcknowledgeChildAddProcess>();
                SaveEntityChildAddProcess = entityChildAddProcess.Where(x => x.EntityState != EntityState.Deleted).ToList();
                DeleteEntityChildAddProcess = entityChildAddProcess.Where(x => x.EntityState == EntityState.Deleted).ToList();

                List<FBookingAcknowledgeChildDetails> SaveEntityChildDetails = new List<FBookingAcknowledgeChildDetails>();
                List<FBookingAcknowledgeChildDetails> DeleteEntityChildDetails = new List<FBookingAcknowledgeChildDetails>();
                SaveEntityChildDetails = entityChildDetails.Where(x => x.EntityState != EntityState.Deleted).ToList();
                DeleteEntityChildDetails = entityChildDetails.Where(x => x.EntityState == EntityState.Deleted).ToList();

                List<FBookingAcknowledgeChildGarmentPart> SaveEntityChildsGpart = new List<FBookingAcknowledgeChildGarmentPart>();
                List<FBookingAcknowledgeChildGarmentPart> DeleteEntityChildsGpart = new List<FBookingAcknowledgeChildGarmentPart>();
                SaveEntityChildsGpart = entityChildsGpart.Where(x => x.EntityState != EntityState.Deleted).ToList();
                DeleteEntityChildsGpart = entityChildsGpart.Where(x => x.EntityState == EntityState.Deleted).ToList();

                List<FBookingAcknowledgeChildProcess> SaveEntityChildsProcess = new List<FBookingAcknowledgeChildProcess>();
                List<FBookingAcknowledgeChildProcess> DeleteEntityChildsProcess = new List<FBookingAcknowledgeChildProcess>();
                SaveEntityChildsProcess = entityChildsProcess.Where(x => x.EntityState != EntityState.Deleted).ToList();
                DeleteEntityChildsProcess = entityChildsProcess.Where(x => x.EntityState == EntityState.Deleted).ToList();

                List<FBookingAcknowledgeChildText> SaveEntityChildsText = new List<FBookingAcknowledgeChildText>();
                List<FBookingAcknowledgeChildText> DeleteEntityChildsText = new List<FBookingAcknowledgeChildText>();
                SaveEntityChildsText = entityChildsText.Where(x => x.EntityState != EntityState.Deleted).ToList();
                DeleteEntityChildsText = entityChildsText.Where(x => x.EntityState == EntityState.Deleted).ToList();

                List<FBookingAcknowledgeChildDistribution> SaveEntityChildsDistribution = new List<FBookingAcknowledgeChildDistribution>();
                List<FBookingAcknowledgeChildDistribution> DeleteEntityChildsDistribution = new List<FBookingAcknowledgeChildDistribution>();
                SaveEntityChildsDistribution = entityChildsDistribution.Where(x => x.EntityState != EntityState.Deleted).ToList();
                DeleteEntityChildsDistribution = entityChildsDistribution.Where(x => x.EntityState == EntityState.Deleted).ToList();

                List<FBookingAcknowledgementYarnLiability> SaveEntityChildsLiability = new List<FBookingAcknowledgementYarnLiability>();
                List<FBookingAcknowledgementYarnLiability> DeleteEntityChildsLiability = new List<FBookingAcknowledgementYarnLiability>();
                SaveEntityChildsLiability = entityFBYL.Where(x => x.EntityState != EntityState.Deleted).ToList();
                DeleteEntityChildsLiability = entityFBYL.Where(x => x.EntityState == EntityState.Deleted).ToList();

                List<FBookingAcknowledgeChildYarnSubBrand> SaveEntityChildsYarnSubBrand = new List<FBookingAcknowledgeChildYarnSubBrand>();
                List<FBookingAcknowledgeChildYarnSubBrand> DeleteEntityChildsYarnSubBrand = new List<FBookingAcknowledgeChildYarnSubBrand>();
                SaveEntityChildsYarnSubBrand = entityChildsYarnSubBrand.Where(x => x.EntityState != EntityState.Deleted).ToList();
                DeleteEntityChildsYarnSubBrand = entityChildsYarnSubBrand.Where(x => x.EntityState == EntityState.Deleted).ToList();

                List<FBookingAcknowledgementLiabilityDistribution> SaveFBookingAcknowledgementLiabilityDistribution = new List<FBookingAcknowledgementLiabilityDistribution>();
                List<FBookingAcknowledgementLiabilityDistribution> DeleteFBookingAcknowledgementLiabilityDistribution = new List<FBookingAcknowledgementLiabilityDistribution>();
                SaveFBookingAcknowledgementLiabilityDistribution = entityChildsLiabilitiesDistribution.Where(x => x.EntityState != EntityState.Deleted).ToList();
                DeleteFBookingAcknowledgementLiabilityDistribution = entityChildsLiabilitiesDistribution.Where(x => x.EntityState == EntityState.Deleted).ToList();

                List<FabricBookingAcknowledge> SaveFabricBookingAcknowledge = new List<FabricBookingAcknowledge>();
                List<FabricBookingAcknowledge> DeleteFabricBookingAcknowledge = new List<FabricBookingAcknowledge>();

                if (entityFBA.IsNotNull())
                {
                    SaveFabricBookingAcknowledge = entityFBA.Where(x => x.EntityState != EntityState.Deleted).ToList();
                    DeleteFabricBookingAcknowledge = entityFBA.Where(x => x.EntityState == EntityState.Deleted).ToList();
                }

                List<FBookingAcknowledgeImage> SaveEntityChildsImage = new List<FBookingAcknowledgeImage>();
                List<FBookingAcknowledgeImage> DeleteEntityChildsImage = new List<FBookingAcknowledgeImage>();
                SaveEntityChildsImage = entityChildsImage.Where(x => x.EntityState != EntityState.Deleted).ToList();
                DeleteEntityChildsImage = entityChildsImage.Where(x => x.EntityState == EntityState.Deleted).ToList();

                List<FreeConceptMaster> SaveEntityFreeConcepts = new List<FreeConceptMaster>();
                List<FreeConceptMaster> DeleteEntityFreeConcepts = new List<FreeConceptMaster>();
                SaveEntityFreeConcepts = entityFreeConcepts.Where(x => x.EntityState != EntityState.Deleted).ToList();
                DeleteEntityFreeConcepts = entityFreeConcepts.Where(x => x.EntityState == EntityState.Deleted).ToList();

                List<FreeConceptChildColor> SaveChildColors = new List<FreeConceptChildColor>();
                List<FreeConceptChildColor> DeleteChildColors = new List<FreeConceptChildColor>();
                SaveChildColors = childColors.Where(x => x.EntityState != EntityState.Deleted).ToList();
                DeleteChildColors = childColors.Where(x => x.EntityState == EntityState.Deleted).ToList();

                List<FreeConceptMRMaster> SaveEntityFreeMRs = new List<FreeConceptMRMaster>();
                List<FreeConceptMRMaster> DeleteEntityFreeMRs = new List<FreeConceptMRMaster>();
                SaveEntityFreeMRs = entityFreeMRs.Where(x => x.EntityState != EntityState.Deleted).ToList();
                DeleteEntityFreeMRs = entityFreeMRs.Where(x => x.EntityState == EntityState.Deleted).ToList();

                List<FreeConceptMRChild> SaveChildMRs = new List<FreeConceptMRChild>();
                List<FreeConceptMRChild> DeleteChildMRs = new List<FreeConceptMRChild>();
                SaveChildMRs = childMRs.Where(x => x.EntityState != EntityState.Deleted).ToList();
                DeleteChildMRs = childMRs.Where(x => x.EntityState == EntityState.Deleted).ToList();

                List<BDSDependentTNACalander> SaveBDCalander = new List<BDSDependentTNACalander>();
                List<BDSDependentTNACalander> DeleteBDCalander = new List<BDSDependentTNACalander>();
                SaveBDCalander = BDCalander.Where(x => x.EntityState != EntityState.Deleted).ToList();
                DeleteBDCalander = BDCalander.Where(x => x.EntityState == EntityState.Deleted).ToList();

                List<FBAChildPlanning> plannings = new List<FBAChildPlanning>();
                entityChilds.ForEach(child =>
                {
                    plannings.AddRange(child.FBAChildPlannings);
                });
                List<FBAChildPlanning> SavePlannings = new List<FBAChildPlanning>();
                List<FBAChildPlanning> DeletePlannings = new List<FBAChildPlanning>();
                SavePlannings = plannings.Where(x => x.EntityState != EntityState.Deleted).ToList();
                DeletePlannings = plannings.Where(x => x.EntityState == EntityState.Deleted).ToList();


                //Delete
                if (DeleteFabricBookingAcknowledge.Count() > 0)
                {
                    await _service.SaveAsync(DeleteFabricBookingAcknowledge, transaction);
                }
                await _service.SaveAsync(DeleteEntityChilds, transaction);
                await _service.SaveAsync(DeleteEntityChildAddProcess, transaction);
                await _service.SaveAsync(DeleteEntityChildDetails, transaction);
                await _service.SaveAsync(DeleteEntityChildsGpart, transaction);
                await _service.SaveAsync(DeleteEntityChildsProcess, transaction);
                await _service.SaveAsync(DeleteEntityChildsText, transaction);
                await _service.SaveAsync(DeleteEntityChildsDistribution, transaction);
                await _service.SaveAsync(DeleteEntityChildsYarnSubBrand, transaction);
                await _service.SaveAsync(DeleteFBookingAcknowledgementLiabilityDistribution, transaction);
                await _service.SaveAsync(DeleteEntityChildsLiability, transaction);
                await _service.SaveAsync(DeleteEntityChildsImage, transaction);
                await _service.SaveAsync(DeleteEntityFreeConcepts, transaction);
                await _service.SaveAsync(DeleteChildColors, transaction);
                if (isBDS == 2)
                {
                    await _service.SaveAsync(DeleteEntityFreeMRs, transaction);
                    await _service.SaveAsync(DeleteChildMRs, transaction);
                }
                await _service.SaveAsync(DeleteBDCalander, transaction);
                await _service.SaveAsync(DeletePlannings, transaction);

                //Save
                if (SaveFabricBookingAcknowledge.Count() > 0)
                {
                    await _service.SaveAsync(SaveFabricBookingAcknowledge, transaction);
                }

                //foreach (var child in SaveEntityChilds)
                //{
                //    await _service.SaveSingleAsync(child, transaction);
                //}

                await _service.SaveAsync(SaveEntityChilds, transaction);
                await _service.SaveAsync(SaveEntityChildAddProcess, transaction);
                await _service.SaveAsync(SaveEntityChildDetails, transaction);
                await _service.SaveAsync(SaveEntityChildsGpart, transaction);
                await _service.SaveAsync(SaveEntityChildsProcess, transaction);
                await _service.SaveAsync(SaveEntityChildsText, transaction);
                await _service.SaveAsync(SaveEntityChildsDistribution, transaction);
                await _service.SaveAsync(SaveEntityChildsYarnSubBrand, transaction);
                await _service.SaveAsync(SaveFBookingAcknowledgementLiabilityDistribution, transaction);
                await _service.SaveAsync(SaveEntityChildsLiability, transaction);
                await _service.SaveAsync(SaveEntityChildsImage, transaction);
                await _service.SaveAsync(SaveEntityFreeConcepts, transaction);
                await _service.SaveAsync(SaveChildColors, transaction);
                if (isBDS == 2)
                {
                    await _service.SaveAsync(SaveEntityFreeMRs, transaction);
                    await _service.SaveAsync(SaveChildMRs, transaction);
                }
                await _service.SaveAsync(SaveBDCalander, transaction);
                await _service.SaveAsync(SavePlannings, transaction);

                #region Sample Booking Master

                var query = "";
                if (entity.IsUnAcknowledge)
                    query = $@"Update {DbNames.EPYSL}..SampleBookingMaster Set UnAcknowledge = 1, UnAcknowledgeReason = '{entity.UnAcknowledgeReason}' Where BookingID = {entity.BookingID}";
                else
                    query = $@"Update {DbNames.EPYSL}..SampleBookingMaster Set Acknowledge = 1 Where BookingID ={entity.BookingID}";
                await _gmtservice.ExecuteAsync(query, AppConstants.GMT_CONNECTION);

                #endregion Sample Booking Master

                //query = $@"Insert into {DbNames.EPYSLTEX}..HostApplication Values( '{hostName}')";
                //await _gmtservice.ExecuteAsync(query, AppConstants.GMT_CONNECTION);

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


        public async Task SaveAsync(int userId, List<BookingItemAcknowledge> entityBIA = null, List<FabricBookingAcknowledge> entityFBA = null, List<FBookingAcknowledge> entityFBookingA = null, List<BookingMaster> entityBM = null, String WithoutOB = "0", bool isRevise = false, string SaveType = null)
        {
            SqlTransaction transaction = null;
            SqlTransaction transaction1 = null;

            try
            {
                await _service.Connection.OpenAsync();
                await _gmtservice.Connection.OpenAsync();
                transaction = _service.Connection.BeginTransaction();
                transaction1 = _gmtservice.Connection.BeginTransaction();

                if (isRevise)
                {
                    string bookingNo = "";
                    if (entityBM.Count() > 0) bookingNo = entityBM.First().BookingNo;
                    if (bookingNo.IsNullOrEmpty())
                    {
                        throw new Exception("Booking No missing (BUlk) => SaveAsync => FBookingAcknowledgeService.cs");
                    }
                    if (SaveType != "UA")
                    {
                        await _connection.ExecuteAsync("spFBookingAcknowledge_BK", new { BookingNo = bookingNo }, transaction, 30, CommandType.StoredProcedure);
                    }
                }

                foreach (BookingItemAcknowledge item in entityBIA)
                {
                    item.EntityState = EntityState.Modified;
                }
                foreach (FabricBookingAcknowledge item in entityFBA)
                {
                    if (item.AcknowledgeID == 0)
                    {
                        item.AcknowledgeID = await _service.GetMaxIdAsync(TableNames.FabricBookingAcknowledge);
                        item.EntityState = EntityState.Added;
                    }
                    else
                    {
                        item.EntityState = EntityState.Modified;
                    }

                }
                foreach (BookingMaster item in entityBM)
                {
                    item.EntityState = EntityState.Modified;
                }

                int maxFBId = 0, maxFBAId = 0, maxFBCId = 0, maxLDId = 0, maxYLDId = 0;
                Boolean IsNew = false;
                if (entityFBookingA.Count(x => x.FBAckID == 0) > 0)
                {
                    maxFBAId = await _service.GetMaxIdAsync(TableNames.FBBOOKING_ACKNOWLEDGE, entityFBookingA.Count(x => x.FBAckID == 0));
                }
                foreach (FBookingAcknowledge item in entityFBookingA)
                {
                    if (item.FBAckID == 0)
                    {
                        item.FBAckID = maxFBAId;

                        item.EntityState = EntityState.Added;
                        IsNew = true;
                        maxFBAId++;
                    }
                    else
                    {
                        item.EntityState = EntityState.Modified;
                    }
                }

                if (entityFBookingA.Count() > 0 && entityFBookingA.First().IsSample)
                {
                    FBookingAcknowledge fBooking = CommonFunction.DeepClone(entityFBookingA.First());
                    entityFBookingA = new List<FBookingAcknowledge>();
                    entityFBookingA.Add(CommonFunction.DeepClone(fBooking));
                }

                entityFBookingA.ForEach(x =>
                {
                    x.IsKnittingComplete = false;
                });

                if (entityFBA.Count() > 0)
                {
                    int revisionNo = entityFBA.FirstOrDefault().RevisionNo;
                    entityFBookingA.ForEach(x => x.PreRevisionNo = revisionNo);
                }

                await _service.SaveAsync(entityFBookingA, transaction);
                foreach (FBookingAcknowledge item in entityFBookingA)
                {
                    //await _service.ValidationSingleAsync(item, transaction, "sp_Add_RevisionWiseAcknowledge", item.EntityState, userId, item.FBAckID); //OFF FOR CORE
                    await _connection.ExecuteAsync("sp_Add_RevisionWiseAcknowledge", new { EntityState = item.EntityState, UserId = userId, PrimaryKeyId = item.FBAckID }, transaction, 30, CommandType.StoredProcedure);
                }

                await _gmtservice.SaveAsync(entityBIA, transaction1);
                await _service.SaveAsync(entityFBA, transaction);
                await _gmtservice.SaveAsync(entityBM, transaction1);
                String selectedbookingID = String.Empty;
                var strArr = entityFBA.Select(i => i.BookingID.ToString()).Distinct().ToArray();
                selectedbookingID += string.Join(",", strArr.ToArray());

                //if (entityBIA.Count > 0 && entityFBA.Count > 0)
                if (entityBIA.Count > 0)
                {
                    String strSql = String.Format(@"Update EPYSLTEX..FBookingAcknowledgeChild Set IstxtUnack=1 Where BookingID in ({0})", selectedbookingID == "" ? "0" : selectedbookingID);
                    var records = _service.ExecuteWithTransactionAsync(strSql, ref transaction);
                    bool WithoutOBBool = WithoutOB == "0" ? false : true;
                    RollBackFabricBookingData(entityBM[0].BookingNo, WithoutOBBool, ref transaction1);
                }
                transaction.Commit();
                transaction1.Commit();
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                if (transaction1 != null) transaction1.Rollback();
                throw ex;
            }
            finally
            {
                _service.Connection.Close(); _gmtservice.Connection.Close();
            }
        }
        public async Task SaveAsync(int userId, List<BookingItemAcknowledge> entityBIA = null, List<FabricBookingAcknowledge> entityFBA = null, List<FBookingAcknowledge> entityFBookingA = null, List<SampleBookingMaster> entityBM = null, bool isRevise = false, string SaveType = null)
        {
            SqlTransaction transaction = null;
            SqlTransaction transaction1 = null;
            try
            {
                await _service.Connection.OpenAsync();
                await _gmtservice.Connection.OpenAsync();
                transaction = _service.Connection.BeginTransaction();
                transaction1 = _gmtservice.Connection.BeginTransaction();

                if (isRevise)
                {
                    string bookingNo = "";
                    if (entityBM.Count() > 0) bookingNo = entityBM.First().BookingNo;
                    if (bookingNo.IsNullOrEmpty() && entityFBookingA.Count() > 0) bookingNo = entityFBookingA.First().BookingNo;

                    if (bookingNo.IsNullOrEmpty())
                    {
                        throw new Exception("Booking No missing (Sample) => SaveAsync => FBookingAcknowledgeService.cs");
                    }
                    if (SaveType != "UA")
                    {
                        await _connection.ExecuteAsync("spFBookingAcknowledge_BK", new { BookingNo = bookingNo }, transaction, 30, CommandType.StoredProcedure);
                    }
                }

                foreach (BookingItemAcknowledge item in entityBIA)
                {
                    item.EntityState = EntityState.Modified;
                }
                foreach (FabricBookingAcknowledge item in entityFBA)
                {
                    if (item.AcknowledgeID == 0)
                    {
                        item.AcknowledgeID = await _service.GetMaxIdAsync(TableNames.FabricBookingAcknowledge);
                        item.EntityState = EntityState.Added;
                    }
                    else
                    {
                        item.EntityState = EntityState.Modified;
                    }

                }
                foreach (SampleBookingMaster item in entityBM)
                {
                    item.EntityState = EntityState.Modified;
                }
                //SaveFabricBookingItemAcknowledgeBackup(entityBM[0].BookingNo, entityFBA[0].WithoutOB, ref transaction);
                //await _gmtservice.SaveAsync(entityBIA, transaction1); //Must open if garments acknowledge off

                /////////////////////////////////////////////////////////////////////////////////////////
                int maxFBId = 0, maxFBAId = 0, maxFBCId = 0, maxLDId = 0, maxYLDId = 0;
                Boolean IsNew = false;
                if (entityFBookingA.Count(x => x.FBAckID == 0) > 0)
                {
                    maxFBAId = await _service.GetMaxIdAsync(TableNames.FBBOOKING_ACKNOWLEDGE, entityFBookingA.Count(x => x.FBAckID == 0));
                }
                foreach (FBookingAcknowledge item in entityFBookingA)
                {
                    if (item.FBAckID == 0)
                    {
                        item.FBAckID = maxFBAId;

                        item.EntityState = EntityState.Added;
                        IsNew = true;
                        maxFBAId++;
                    }
                    else
                    {
                        item.EntityState = EntityState.Modified;
                    }

                }

                if (entityFBookingA.Count() > 0 && entityFBookingA.First().IsSample)
                {
                    FBookingAcknowledge fBooking = CommonFunction.DeepClone(entityFBookingA.First());
                    entityFBookingA = new List<FBookingAcknowledge>();
                    entityFBookingA.Add(CommonFunction.DeepClone(fBooking));
                }

                entityFBookingA.ForEach(x =>
                {
                    x.IsKnittingComplete = false;
                });

                if (isRevise)
                {
                    if (entityFBA.Count() > 0)
                    {
                        entityFBookingA.ForEach(x => x.PreRevisionNo = entityFBA.First().RevisionNo);
                    }
                }
                if (entityFBA.Count() > 0)
                {
                    int revisionNo = entityFBA.FirstOrDefault().RevisionNo;
                    entityFBookingA.ForEach(x => x.PreRevisionNo = revisionNo);
                }

                await _service.SaveAsync(entityFBookingA, transaction);
                foreach (FBookingAcknowledge item in entityFBookingA)
                {
                    //await _service.ValidationSingleAsync(item, transaction, "sp_Add_RevisionWiseAcknowledge", item.EntityState, userId, item.FBAckID); //OFF FOR CORE
                    await _connection.ExecuteAsync("sp_Add_RevisionWiseAcknowledge", new { EntityState = item.EntityState, UserId = userId, PrimaryKeyId = item.FBAckID }, transaction, 30, CommandType.StoredProcedure);
                }

                await _service.SaveAsync(entityFBA, transaction);
                //await _gmtservice.SaveAsync(entityBM, transaction1); //Must open if garments acknowledge off

                String selectedbookingID = String.Empty;
                var strArr = entityFBA.Select(i => i.BookingID.ToString()).Distinct().ToArray();
                selectedbookingID += string.Join(",", strArr.ToArray());


                //if (entityBIA.Count > 0 && entityFBA.Count > 0)
                if (entityFBA.Count > 0)
                {
                    String strSql = String.Format(@"Update EPYSLTEX..FBookingAcknowledgeChild Set IstxtUnack=1 Where BookingID in ({0})", selectedbookingID == "" ? "0" : selectedbookingID);
                    var records = _service.ExecuteWithTransactionAsync(strSql, ref transaction);

                    if (isRevise)
                    {
                        strSql = String.Format(@"Update EPYSLTEX..FBookingAcknowledge Set PreRevisionNo={0} Where BookingID in ({0})", entityFBA.First().RevisionNo, selectedbookingID == "" ? "0" : selectedbookingID);
                        _service.ExecuteWithTransactionAsync(strSql, ref transaction);
                    }
                    RollBackFabricBookingData(entityBM[0].BookingNo, true, ref transaction1);
                }
                transaction.Commit();
                transaction1.Commit();
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                if (transaction1 != null) transaction1.Rollback();
                throw ex;
            }
            finally
            {
                _service.Connection.Close(); _gmtservice.Connection.Close();
            }
        }
        public async Task SaveAsync(int userId, List<FabricBookingAcknowledge> entityFBA = null)
        {
            SqlTransaction transaction = null;
            try
            {
                await _service.Connection.OpenAsync();
                transaction = _service.Connection.BeginTransaction();

                foreach (FabricBookingAcknowledge item in entityFBA)
                {
                    if (item.AcknowledgeID == 0)
                    {
                        item.AcknowledgeID = await _service.GetMaxIdAsync(TableNames.FabricBookingAcknowledge);

                        item.EntityState = EntityState.Added;
                    }
                    else
                    {
                        item.EntityState = EntityState.Modified;
                    }

                }



                await _service.SaveAsync(entityFBA, transaction);


                transaction.Commit();
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                throw ex;
            }
            finally
            {
                _service.Connection.Close();
            }
        }
        public async Task SaveAsync(int userId, List<FBookingAcknowledge> entityFBA = null)
        {
            List<FBookingAcknowledgeChild> entityFBC = new List<FBookingAcknowledgeChild>();
            List<FBookingAcknowledgementLiabilityDistribution> entityFBCLD = new List<FBookingAcknowledgementLiabilityDistribution>();
            SqlTransaction transaction = null;
            try
            {
                await _service.Connection.OpenAsync();
                transaction = _service.Connection.BeginTransaction();
                bool IsNew = false;
                foreach (FBookingAcknowledge item in entityFBA)
                {
                    if (item.FBAckID == 0)
                    {
                        item.FBAckID = await _service.GetMaxIdAsync(TableNames.FBBOOKING_ACKNOWLEDGE);

                        item.EntityState = EntityState.Added;
                        IsNew = true;
                    }
                    else
                    {

                    }
                    entityFBC.AddRange(item.FBookingChild);
                }

                foreach (FBookingAcknowledgeChild item in entityFBC)
                {
                    if (item.BookingChildID > 0 && item.AcknowledgeID == 0)
                    {
                        //item.FBAckID = await _service.GetMaxIdAsync(TableNames.FBBOOKING_ACKNOWLEDGE);
                        FBookingAcknowledge objItem = entityFBA.Find(j => j.BookingID == item.BookingID);
                        item.AcknowledgeID = objItem.IsNotNull() ? objItem.FBAckID : entityFBA[0].FBAckID;
                        item.EntityState = EntityState.Added;
                    }
                    else
                    {
                        item.EntityState = EntityState.Modified;
                    }
                    entityFBCLD.AddRange(item.ChildAckLiabilityDetails);
                }

                foreach (FBookingAcknowledgementLiabilityDistribution item in entityFBCLD)
                {
                    if (item.BookingChildID > 0 && item.LChildID == 0)
                    {
                        item.LChildID = await _service.GetMaxIdAsync(TableNames.FBBOOKING_ACKNOWLEDGE_LIABILITIES_DISTRIBUTION);
                        FBookingAcknowledge objItem = entityFBA.Find(j => j.BookingID == item.BookingID);
                        item.AcknowledgeID = objItem.IsNotNull() ? objItem.FBAckID : entityFBA[0].FBAckID;
                        item.EntityState = EntityState.Added;
                    }
                    else
                    {
                        item.EntityState = EntityState.Modified;
                    }
                }
                await _service.SaveAsync(entityFBA, transaction);
                await _service.SaveAsync(entityFBC, transaction);

                await _service.SaveAsync(entityFBCLD, transaction);

                transaction.Commit();
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                throw ex;
            }
            finally
            {
                _service.Connection.Close();
            }
        }

        public async Task SaveAsync(int userId, String EditType, String BookingNo, List<FabricBookingAcknowledge> entityFB, List<FBookingAcknowledge> entityFBA, List<FBookingAcknowledgeChild> entityFBC, List<FBookingAcknowledgementLiabilityDistribution> entityLD, List<FBookingAcknowledgementYarnLiability> entityYLD, bool isRevised, string WithoutOB, int styleMasterId, int bomMasterId, int UserCode, string SaveType = null)
        {
            bool hasError = false;
            int userCode = UserCode;
            SqlTransaction transaction = null;
            try
            {

                await _service.Connection.OpenAsync();
                transaction = _service.Connection.BeginTransaction();


                if (isRevised)
                {
                    if (BookingNo.IsNullOrEmpty())
                    {
                        throw new Exception("Booking No missing => SaveAsync => FBookingAcknowledgeService.cs");
                    }
                    if (SaveType != "UA")
                    {
                        await _connection.ExecuteAsync("spFBookingAcknowledge_BK", new { BookingNo = BookingNo }, transaction, 30, CommandType.StoredProcedure);
                    }
                }

                /*if (isRevised)
                {
                    await _connection.ExecuteAsync("spBackupFabricBookingAcknowledge_Full", new { BookingID = entityFBA.FirstOrDefault().BookingID }, transaction, 30, CommandType.StoredProcedure);
                }*/
                bool IsNew = false;

                #region Get Unique ID
                int maxFBId = 0, maxFBAId = 0, maxFBCId = 0, maxLDId = 0, maxYLDId = 0;

                #endregion
                maxFBId = await _service.GetMaxIdAsync(TableNames.FabricBookingAcknowledge, entityFB.Count(x => x.AcknowledgeID == 0));
                entityFB.ForEach(item =>
                {
                    if (item.AcknowledgeID == 0)
                    {
                        item.AcknowledgeID = maxFBId;

                        item.EntityState = EntityState.Added;
                        IsNew = true;
                        maxFBId++;
                    }
                    else
                    {
                        item.EntityState = EntityState.Modified;
                    }
                });

                maxFBAId = await _service.GetMaxIdAsync(TableNames.FBBOOKING_ACKNOWLEDGE, entityFBA.Count(x => x.EntityState == EntityState.Added));
                for (int i = 0; i < entityFBA.Count(); i++)
                {
                    var item = entityFBA[i];
                    if (item.EntityState == EntityState.Added && item.FBAckID == 0)
                    {
                        item.FBAckID = maxFBAId++;
                        item.EntityState = EntityState.Added;
                        IsNew = true;
                    }
                    else
                    {
                        item.EntityState = EntityState.Modified;
                    }

                    if (item.IsSample) break;
                }
                //entityFBA.ForEach(item =>
                //{
                //    if (item.EntityState == EntityState.Added && item.FBAckID == 0)
                //    {
                //        item.FBAckID = maxFBAId++;
                //        item.EntityState = EntityState.Added;
                //        IsNew = true;
                //    }
                //    else
                //    {
                //        item.EntityState = EntityState.Modified;
                //    }
                //});

                maxFBCId = await _service.GetMaxIdAsync(TableNames.FBBOOKING_ACKNOWLEDGE_CHILD, entityFBC.Count(x => x.EntityState == EntityState.Added));
                entityFBC.Where(x => x.EntityState != EntityState.Deleted).ToList().ForEach(item =>
                {
                    if (item.EntityState == EntityState.Added)
                    {
                        item.BookingChildID = maxFBCId++;
                        item.EntityState = EntityState.Added;
                    }
                    else
                    {
                        item.EntityState = EntityState.Modified;
                    }
                    FBookingAcknowledge objItem = entityFBA.Find(j => j.BookingID == item.BookingID);
                    item.AcknowledgeID = objItem.IsNotNull() ? objItem.FBAckID : entityFBA[0].FBAckID;
                });

                maxLDId = await _service.GetMaxIdAsync(TableNames.FBBOOKING_ACKNOWLEDGE_LIABILITIES_DISTRIBUTION, entityLD.Count(x => x.LChildID == 0));
                entityLD.Where(x => (x.BookingChildID > 0 || x.ConsumptionID > 0) && x.LChildID == 0).ToList().ForEach(item =>
                {
                    item.LChildID = maxLDId;
                    FBookingAcknowledge objItem = entityFBA.Find(j => j.BookingID == item.BookingID);
                    item.AcknowledgeID = objItem.IsNotNull() ? objItem.FBAckID : entityFBA[0].FBAckID;
                    item.EntityState = EntityState.Added;
                    maxLDId++;
                });

                maxYLDId = await _service.GetMaxIdAsync(TableNames.FBBOOKING_ACKNOWLEDGE_YARN_LIABILITIES, entityYLD.Count(x => x.YLChildID == 0));
                entityYLD.Where(x => (x.BookingChildID > 0 || x.ConsumptionID > 0) && x.YLChildID == 0).ToList().ForEach(item =>
                {
                    item.YLChildID = maxYLDId;
                    FBookingAcknowledge objItem = entityFBA.Find(j => j.BookingID == item.BookingID);
                    item.AcknowledgeID = objItem.IsNotNull() ? objItem.FBAckID : entityFBA[0].FBAckID;
                    item.EntityState = EntityState.Added;
                    maxYLDId++;
                });

                if (entityFBA.Count() > 0 && entityFBA.First().IsSample)
                {
                    FBookingAcknowledge fBooking = CommonFunction.DeepClone(entityFBA.First());
                    entityFBA = new List<FBookingAcknowledge>();
                    entityFBA.Add(CommonFunction.DeepClone(fBooking));
                }

                if (EditType == "R")
                {
                    SaveFabricBookingItemAcknowledgeBackup(BookingNo, entityFBA[0].WithoutOB, ref transaction);
                    entityFBC.Where(x => x.IsDeleted == true).ToList().ForEach(x =>
                    {
                        x.BookingQty = 0;
                        x.ConsumptionQty = 0;
                    });
                }

                entityFBA.ForEach(x =>
                {
                    x.IsKnittingComplete = false;
                });

                #region For sample booking when booking ID is same but childs has multiple Sub Group ID
                if (entityFBA.Count() > 0 && entityFBC.Where(x => x.EntityState != EntityState.Deleted).Count() > 0)
                {
                    List<int> subGroupIds = entityFBC.Where(x => x.EntityState != EntityState.Deleted).Select(x => x.SubGroupID).Distinct().ToList();
                    List<int> bookingIds = entityFBA.Select(x => x.BookingID).Distinct().ToList();
                    if (bookingIds.Count() == 1 && subGroupIds.Count() > 1)
                    {
                        List<FBookingAcknowledge> tempList = new List<FBookingAcknowledge>();
                        entityFBA.First().SubGroupID = entityFBC.Where(x => x.EntityState != EntityState.Deleted).OrderBy(x => x.SubGroupID).First().SubGroupID;
                        tempList.Add(entityFBA.First());
                        entityFBA = tempList;
                    }
                }
                #endregion

                if (isRevised)
                {
                    if (entityFB.Count() > 0)
                    {
                        entityFBA.ForEach(x => x.PreRevisionNo = entityFB.First().RevisionNo);
                    }
                }

                var entityFBWithoutBOMMasterID = entityFB.Where(x => x.BOMMasterID == 0 && WithoutOB == "0").ToList();
                if (entityFBWithoutBOMMasterID.Count() > 0)
                {
                    throw new Exception("BOMMasterID missing => SaveAsync => FBookingAcknowledgeService");
                }

                await _service.SaveAsync(entityFB, transaction);

                if (entityFB.Count() > 0)
                {
                    int revisionNo = entityFB.FirstOrDefault().RevisionNo;
                    entityFBA.ForEach(x => x.PreRevisionNo = revisionNo);
                }
                await _service.SaveAsync(entityFBA, transaction);
                foreach (FBookingAcknowledge item in entityFBA)
                {
                    //await _service.ValidationSingleAsync(item, transaction, "sp_Validation_FBookingAcknowledge_1", item.EntityState, userId, item.FBAckID); //OFF FOR CORE
                    //await _service.ValidationSingleAsync(item, transaction, "sp_Add_RevisionWiseAcknowledge", item.EntityState, userId, item.FBAckID); //OFF FOR CORE

                    await _connection.ExecuteAsync("sp_Validation_FBookingAcknowledge_1", new { EntityState = item.EntityState, UserId = userId, PrimaryKeyId = item.FBAckID }, transaction, 30, CommandType.StoredProcedure);
                    await _connection.ExecuteAsync("sp_Add_RevisionWiseAcknowledge", new { EntityState = item.EntityState, UserId = userId, PrimaryKeyId = item.FBAckID }, transaction, 30, CommandType.StoredProcedure);
                }

                var aaa = entityFBC.Where(x => x.SubGroupID == 11).ToList();

                await _service.SaveAsync(entityFBC, transaction);
                string sql = "";
                foreach (FBookingAcknowledgeChild item in entityFBC)
                {
                    sql = $"exec sp_Validation_FBookingAcknowledgeChild_1 {item.EntityState}, {userId},{item.BookingChildID}, {item.ConsumptionID}, {item.BookingID}, {item.ItemMasterID}, {item.AcknowledgeID}";
                    //await _service.ValidationSingleAsync(item, transaction, "sp_Validation_FBookingAcknowledgeChild_1", item.EntityState, userId, item.BookingChildID, item.ConsumptionID, item.BookingID, item.ItemMasterID, item.AcknowledgeID); //OFF FOR CORE
                    await _connection.ExecuteAsync("sp_Validation_FBookingAcknowledgeChild_1", new { EntityState = item.EntityState, UserId = userId, PrimaryKeyId = item.BookingChildID, SecondParamValue = item.ConsumptionID, ThirdParamValu = item.BookingID, ForthParamValue = item.ItemMasterID, FifthParamValue = item.AcknowledgeID }, transaction, 30, CommandType.StoredProcedure);
                }
                foreach (FBookingAcknowledge item in entityFBA)
                {
                    //await _service.ValidationSingleAsync(item, transaction, "sp_Validation_FBookingAcknowledge_FBA", item.EntityState, userId, item.FBAckID); //OFF FOR CORE
                    await _connection.ExecuteAsync("sp_Validation_FBookingAcknowledge_FBA", new { EntityState = item.EntityState, UserId = userId, PrimaryKeyId = item.FBAckID }, transaction, 30, CommandType.StoredProcedure);
                }

                await _service.SaveAsync(entityLD, transaction);
                foreach (FBookingAcknowledgementLiabilityDistribution item in entityLD)
                {
                    //await _service.ValidationSingleAsync(item, transaction, "sp_Validation_FBookingAcknowledgementLiabilityDistribution_1", item.EntityState, userId, item.LChildID); //OFF FOR CORE
                    await _connection.ExecuteAsync("sp_Validation_FBookingAcknowledgementLiabilityDistribution_1", new { PrimaryKeyId = item.LChildID, UserId = userId, EntityState = item.EntityState }, transaction, 30, CommandType.StoredProcedure);
                }
                await _service.SaveAsync(entityYLD, transaction);
                foreach (FBookingAcknowledgementYarnLiability item in entityYLD)
                {
                    //await _service.ValidationSingleAsync(item, transaction, "sp_Validation_FBookingAcknowledgementYarnLiability_1", item.EntityState, userId, item.YLChildID); //OFF FOR CORE
                    await _connection.ExecuteAsync("sp_Validation_FBookingAcknowledgementYarnLiability_1", new { PrimaryKeyId = item.YLChildID, UserId = userId, EntityState = item.EntityState }, transaction, 30, CommandType.StoredProcedure);
                }

                if (entityYLD.Count() > 0)
                {
                    int userId1 = entityFB.First().EntityState == EntityState.Added ? entityFB.First().AddedBy : entityFB.First().UpdatedBy;
                    if (userId1.IsNull()) userId1 = 0;
                    await _connection.ExecuteAsync("spYarnStockOperation", new { MasterID = entityFB.First().BookingID, FromMenuType = EnumFromMenuType.FBookingAcknowledgementYarnLiability, UserId = userId1 }, transaction, 30, CommandType.StoredProcedure);
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                hasError = true;
                if (transaction != null) transaction.Rollback();
                if (ex.Message.Contains('~')) throw new Exception(ex.Message.Split('~')[0]);
                throw ex;
            }
            finally
            {
                _service.Connection.Close();

                if (!hasError)
                {
                    #region UpdateOrderEventCalander
                    try
                    {
                        await UpdateOrderEventCalander(isRevised, WithoutOB, styleMasterId, bomMasterId, entityFB.First().BookingID);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    #endregion

                    #region System Notification
                    try
                    {
                        await SystemNotification(entityFB, userCode);
                    }
                    catch (Exception ex)
                    {

                        throw ex;
                    }
                    #endregion
                }
            }
        }
        private async Task<List<BookingMaster>> GetAllBookingMasterByID(string bookingID)
        {
            string sql = $@"With BOM as (
                                Select SubGroupID=1, IsApproved= IsNull(Min(Convert(int,IsApproved)),0),RevisionNo=(Sum(ISNULL(BIG.RevisionNo,0)))
                                From {DbNames.EPYSL}..BOMItemGroup BIG
                                LEFT Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = BIG.SubGroupID
                                LEFT Join {DbNames.EPYSL}..BOMMaster BOM On BOM.BOMMasterID = BIG.BOMMasterID
                                LEFT Join {DbNames.EPYSL}..BookingMaster BM On BM.ExportOrderID = BOM.ExportOrderID
                                Where BM.BookingID IN ({bookingID})  And ISG.SubGroupName in ('Fabric','Collar','Cuff')
                            ) 
                            Select BOMI.BOMMasterID, BM.BookingNo, BOMM.StyleMasterID, 
                            BM.ExportOrderID, BM.ExportOrderNo, SupplierID = ISNULL(BC.ContactID,0),CS.Name SupplierName
                            from {DbNames.EPYSL}..BOMItemGroup BOMI
                            Inner Join {DbNames.EPYSL}..BOMMaster BOMM On BOMM.BOMMasterID = BOMI.BOMMasterID 
                            Left Join {DbNames.EPYSL}..BookingChild BC On BC.BOMMasterID = BOMM.BOMMasterID and BC.SubGroupID = BOMI.SubGroupID
                            Left Join {DbNames.EPYSL}..BookingMaster BM ON BM.BookingID = BC.BookingID and BM.ExportOrderID = BOMM.ExportOrderID
                            Left Join {DbNames.EPYSL}..Contacts CS On CS.ContactID = BC.ContactID
                            Where IsNull(BM.BookingID,-1) IN ({bookingID})
                            GROUP BY BOMI.BOMMasterID, BM.BookingNo, BOMM.StyleMasterID, 
                            BM.ExportOrderID, BM.ExportOrderNo, ISNULL(BC.ContactID,0),CS.Name";

            try
            {
                await _service.Connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                List<BookingMaster> obj = records.Read<BookingMaster>().ToList();
                return obj;
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
        private async Task<List<SampleBookingMaster>> GetAllSampleBookingMasterByID(string bookingID)
        {
            string sql = $@"SELECT BOMMasterID = 0, SPB.BookingNo, SPB.StyleMasterID, 
                        SPB.ExportOrderID, EM.ExportOrderNo, SupplierID = ISNULL(C.ContactID,0),C.Name SupplierName
                        FROM  {DbNames.EPYSL}..SampleBookingMaster SPB
                        Inner Join {DbNames.EPYSL}..SampleType ST On ST.SampleTypeID = SPB.SampleID
                        Inner Join {DbNames.EPYSL}..Contacts C On C.ContactID = SPB.SupplierID
                        Inner Join {DbNames.EPYSL}..ContactCategoryTeam CCT On CCT.CategoryTeamID = SPB.BuyerTeamID
                        Left Join {DbNames.EPYSL}..ExportOrderMaster EM On EM.ExportOrderID = SPB.ExportOrderID
                        Inner Join {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = SPB.AddedBy
                        Inner Join {DbNames.EPYSL}..Employee EMP ON EMP.EmployeeCode = LU.EmployeeCode   
                        Where SPB.BookingID = {bookingID}";

            try
            {
                await _service.Connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                List<SampleBookingMaster> obj = records.Read<SampleBookingMaster>().ToList();
                return obj;
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
        private async Task<List<BookingMaster>> GetAllExportOrderMasterForYarnBookingInformation(string bookingID)
        {
            string sql = $@"Select SM.BuyerID, HasYarnBooking = Case When YB.BookingID IS NULL then 0 Else 1 End,BM.BookingID,BM.BookingNo,
                EOM.ExportOrderID,EOM.ExportOrderNo,BOMM.BOMMasterID
                From {DbNames.EPYSL}..ExportOrderMaster EOM
                Inner Join {DbNames.EPYSL}..BOMMaster BOMM On BOMM.ExportOrderID = EOM.ExportOrderID
                Inner Join {DbNames.EPYSL}..BookingMaster BM On BM.ExportOrderID = EOM.ExportOrderID
                Inner Join {DbNames.EPYSL}..StyleMaster SM On SM.StyleMasterID = EOM.StyleMasterID
                Left Join (Select * from EPYSLTEX..YarnBookingMaster Where WithoutOB = 0) YB On YB.ExportOrderID = BM.ExportOrderID And YB.BookingID = BM.BookingID
                Where BM.BookingID IN ({bookingID})
                GROUP BY SM.BuyerID, Case When YB.BookingID IS NULL then 0 Else 1 End,BM.BookingID,BM.BookingNo,
                EOM.ExportOrderID,EOM.ExportOrderNo,BOMM.BOMMasterID";

            try
            {
                await _service.Connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                List<BookingMaster> obj = records.Read<BookingMaster>().ToList();
                return obj;
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
        private async Task<List<BookingMaster>> GetAllExportOrderMasterForYarnBookingInformationForSampleBooking(string bookingID)
        {
            string sql = $@"Select SM.BuyerID, HasYarnBooking = Case When YB.BookingID IS NULL then 0 Else 1 End,
                            EOM.ExportOrderID,EOM.ExportOrderNo,BOMMasterID = 0,BM.BookingID,BM.BookingNo
                            From {DbNames.EPYSL}..SampleBookingMaster BM
                            Inner Join {DbNames.EPYSL}..ExportOrderMaster EOM On BM.SLNo = EOM.ExportOrderNo
                            Inner Join {DbNames.EPYSL}..StyleMaster SM On SM.StyleMasterID = EOM.StyleMasterID
                            Left Join (Select * from EPYSLTEX..YarnBookingMaster Where WithoutOB = 1) YB On YB.ExportOrderID = EOM.ExportOrderID And YB.BookingID = BM.BookingID
                            Where BM.BookingID = {bookingID}
                            GROUP BY SM.BuyerID, Case When YB.BookingID IS NULL then 0 Else 1 End,
                            EOM.ExportOrderID,EOM.ExportOrderNo,BM.BookingID,BM.BookingNo";

            try
            {
                await _service.Connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                List<BookingMaster> obj = records.Read<BookingMaster>().ToList();
                return obj;
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
        private async Task<List<MessageQueue>> GetParentManuInfoByMenuID(string menuId)
        {
            string sql = $@"SELECT ParentColumnId = MenuID
                                        FROM {DbNames.EPYSL}..MenuDependence
                                        WHERE (DependentMenuID = {menuId})";

            try
            {
                await _service.Connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                List<MessageQueue> obj = records.Read<MessageQueue>().ToList();
                return obj;
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
        private async Task<List<MessageQueue>> GetAllMessageQueueByMenuID(string MenuID, string ForMenuID, string EventID, string ParentColumnID)
        {
            string sql = $@"Select ForMenuId = ForMenuID,Id =MessageID, SenderId = SenderID,  * from {DbNames.EPYSL}..MessageQueue
					Where MenuID = {MenuID} And ForMenuID = {ForMenuID} And EventID = {EventID} And AutoReceived = 0 And Received = 0 And ParentColumnID in (Select _ID From dbo.fnReturnStringArray('{ParentColumnID}',','))";

            try
            {
                await _service.Connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                List<MessageQueue> obj = records.Read<MessageQueue>().ToList();
                return obj;
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

        private async Task<List<LoginHistory>> GetLoginHistoryByUserCode(int UserCode)
        {
            string sql = $@"SELECT TOP 1 LH.* 
                            FROM LoginHistory LH
                            Where LH.UserCode = {UserCode}
                            Order By LoginHistoryID DESC";

            try
            {
                await _service.Connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                List<LoginHistory> obj = records.Read<LoginHistory>().ToList();
                return obj;
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

        private async Task SystemNotification(List<FabricBookingAcknowledge> saveFabricBookingItemAcknowledgeList, int UserCode, String SaveType = "S")
        {
            int userCode = UserCode;
            string SenderIPAddress = "";
            saveFabricBookingItemAcknowledgeList.ForEach(x => x.MenuId = 110);  // GMT Fabric booking ck menu id  =110 thats whay hard code 
            try
            {
                String WithoutOB = "0", SupplierID = String.Empty, Supplier = String.Empty, BOMMasterID = String.Empty, BookingNo = String.Empty, ExportOrderID = String.Empty, StyleMasterID = String.Empty, ExportOrderNo = String.Empty;

                if (saveFabricBookingItemAcknowledgeList.Count > 0)
                {
                    WithoutOB = saveFabricBookingItemAcknowledgeList[0].WithoutOB == true ? "1" : "0";

                    List<BookingMaster> yiList = new List<BookingMaster>();

                    if (WithoutOB == "0")
                    {
                        List<BookingMaster> bmList = await this.GetAllBookingMasterByID(saveFabricBookingItemAcknowledgeList[0].BookingID.ToString());

                        if (bmList.Count > 0)
                        {
                            BookingNo = bmList[0].BookingNo.ToString();
                            BOMMasterID = bmList[0].BOMMasterID.ToString();
                            StyleMasterID = bmList[0].StyleMasterID.ToString();
                            ExportOrderID = bmList[0].ExportOrderID.ToString();
                            ExportOrderNo = bmList[0].ExportOrderNo.ToString();
                            SupplierID = bmList[0].SupplierID.ToString();
                            Supplier = bmList[0].SupplierID == 0 ? "" : " " + bmList[0].SupplierName;
                        }

                        yiList = await this.GetAllExportOrderMasterForYarnBookingInformation(saveFabricBookingItemAcknowledgeList[0].BookingID.ToString());
                    }
                    else
                    {
                        List<SampleBookingMaster> bmList = await this.GetAllSampleBookingMasterByID(saveFabricBookingItemAcknowledgeList[0].BookingID.ToString());
                        if (bmList.Count > 0)
                        {
                            BookingNo = bmList[0].BookingNo.ToString();
                            BOMMasterID = "0";
                            StyleMasterID = bmList[0].StyleMasterID.ToString();
                            ExportOrderID = bmList[0].ExportOrderID.ToString();
                            ExportOrderNo = bmList[0].ExportOrderNo.ToString();
                            SupplierID = bmList[0].SupplierID.ToString();
                            Supplier = bmList[0].SupplierID == 0 ? "" : " " + bmList[0].SupplierName;
                        }

                        yiList = await this.GetAllExportOrderMasterForYarnBookingInformationForSampleBooking(saveFabricBookingItemAcknowledgeList[0].BookingID.ToString());
                    }

                    Int32 SubGroupID = 0, BuyerID = 0, buyerTeamId = 0;
                    String SubGroupName = "Fabric";

                    SubGroupID = 1;

                    if (yiList.Count > 0)
                    {
                        BuyerID = yiList[0].BuyerID;
                        buyerTeamId = yiList[0].BuyerTeamID;
                    }

                    String EditType = saveFabricBookingItemAcknowledgeList[0].RevisionNo > 0 ? "Revise " : "";
                    String YarnEditType = saveFabricBookingItemAcknowledgeList[0].RevisionNo > 0 && yiList[0].HasYarnBooking == 1 ? "Revise " : "";

                    #region Load Depandent Menu
                    List<MessageQueue> parentMenu = await this.GetParentManuInfoByMenuID(saveFabricBookingItemAcknowledgeList.First().MenuId.ToString());
                    #endregion

                    #region Get IP Address
                    List<LoginHistory> loginHisList = await this.GetLoginHistoryByUserCode(userCode);
                    if (loginHisList.Count > 0)
                    {
                        SenderIPAddress = loginHisList.First().IPAddress;
                    }

                    #endregion

                    #region Create System Notification
                    if (SaveType != "UA")
                    {
                        String message = String.Empty, subject = String.Empty;

                        if (WithoutOB == "0")
                        {
                            message = String.Format(@"{0}Booking No. <b>{1}</b> has been acknowledged by textile department.", EditType, BookingNo, Supplier);
                            subject = String.Format(@"{0}Fabric Booking Acknowledged", EditType);
                        }
                        else
                        {
                            message = String.Format(@"{0}Sample Booking No. <b>{1}</b> has been acknowledged by textile department.", EditType, BookingNo, Supplier);
                            subject = String.Format(@"{0}Sample Fabric Booking Acknowledged", EditType);
                        }

                        await this.CreateNotification(saveFabricBookingItemAcknowledgeList.First().MenuId, "Add", saveFabricBookingItemAcknowledgeList[0].BookingID,
                                    BuyerID, buyerTeamId, BookingNo + "____" + BOMMasterID + "____" + ExportOrderNo + "____" + EditType.Trim() + "____" + WithoutOB.Trim(),
                                    userCode, DateTime.Now,
                                    subject,
                                    message, SenderIPAddress, "", 1, 0, SubGroupID, SupplierID.ToInt(), StyleMasterID.ToInt(), ExportOrderID.ToInt(), BOMMasterID.ToInt(), saveFabricBookingItemAcknowledgeList[0].BookingID, 0, 0);

                        message = String.Empty;
                        subject = String.Empty;

                        if (WithoutOB == "0")
                        {
                            message = String.Format(@"{0}Booking No. <b>{1}</b> has been acknowledged and waiting for Yarn Booking Information.", YarnEditType, yiList[0].BookingNo);
                            subject = String.Format(@"Awaiting For {0}Yarn Booking", YarnEditType);
                        }
                        else
                        {
                            message = String.Format(@"{0}Sample Booking No. <b>{1}</b> has been acknowledged and waiting for Yarn Booking Information.", YarnEditType, yiList[0].BookingNo);
                            subject = String.Format(@"Awaiting For {0}Yarn Booking", YarnEditType);
                        }

                        await this.CreateNotification(saveFabricBookingItemAcknowledgeList.First().MenuId, "Add", saveFabricBookingItemAcknowledgeList[0].BookingID, BuyerID,
                                     buyerTeamId, yiList[0].ExportOrderNo + "____" + yiList[0].BookingNo + "____" + yiList[0].BOMMasterID.ToString() + "____" + YarnEditType.Trim() + "____" + WithoutOB.Trim(),
                                     userCode, DateTime.Now,
                                     subject,
                                     message, SenderIPAddress, "FBI", 0, 0, SubGroupID, SupplierID.ToInt(), StyleMasterID.ToInt(), ExportOrderID.ToInt(), BOMMasterID.ToInt(), saveFabricBookingItemAcknowledgeList[0].BookingID, 0, 0);

                    }
                    #endregion

                    #region Receive System Notification and update status
                    if (parentMenu.Count > 0)
                    {
                        foreach (FabricBookingAcknowledge itemFabricBookingAcknowledge in saveFabricBookingItemAcknowledgeList)
                        {
                            List<MessageQueue> messageQueue = await this.GetAllMessageQueueByMenuID(parentMenu[0].ParentColumnId.ToString(), saveFabricBookingItemAcknowledgeList.First().MenuId.ToString(), "1", itemFabricBookingAcknowledge.BookingID.ToString());
                            foreach (MessageQueue itemMessageQueue in messageQueue)
                            {
                                await this.UpdateNotification(95, itemMessageQueue.ForMenuId, "Add", itemFabricBookingAcknowledge.BookingID, itemMessageQueue.ReferenceValue, itemMessageQueue.SenderId, itemMessageQueue.SendDate, userCode, DateTime.Now, SenderIPAddress, true, 0, SubGroupID, SupplierID.ToInt(), StyleMasterID.ToInt(),
                                ExportOrderID.ToInt(), BOMMasterID.ToInt(), itemFabricBookingAcknowledge.BookingID, 0, 0, itemMessageQueue.Id);

                            }
                            messageQueue = await this.GetAllMessageQueueByMenuID("375", saveFabricBookingItemAcknowledgeList.First().MenuId.ToString(), "1", itemFabricBookingAcknowledge.BookingID.ToString());
                            foreach (MessageQueue itemMessageQueue in messageQueue)
                            {
                                await this.UpdateNotification(95, itemMessageQueue.ForMenuId, "Add", itemFabricBookingAcknowledge.BookingID, itemMessageQueue.ReferenceValue, itemMessageQueue.SenderId, itemMessageQueue.SendDate, userCode, DateTime.Now, SenderIPAddress, true, 0, SubGroupID, SupplierID.ToInt(), StyleMasterID.ToInt(),
                                                                ExportOrderID.ToInt(), BOMMasterID.ToInt(), itemFabricBookingAcknowledge.BookingID, 0, 0, itemMessageQueue.Id);
                            }
                        }

                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        private async Task CreateNotification(Int32 MenuID, String Event, Int32 ParameterColumn0, Int32 ParameterColumn1, Int32 ParentColumnID, String ReferenceValue, Int32 SenderID, DateTime SendDate,
                        String Subject, String Message, String SenderIPAddress, String RefNo, Int32 Readable, int NotifyFor, int SubGroupID, int SupplierID, int StyleMasterID, int ExportOrderID, int BOMMasterID, int BookingID, int SPOMasterID, int PIMasterID, int YBookingID = 0, int CompanyID = 0)
        {
            try
            {
                String strSql = String.Format(@"exec EPYSL..spCreateNotification {0},'{1}',{2},{3},{4},'{5}',{6},'{7}','{8}','{9}','{10}','{11}',{12},{13},{14},{15},'{16}','{17}','{18}','{19}','{20}','{21}','0',0", MenuID, Event, ParameterColumn0, ParameterColumn1, ParentColumnID, ReferenceValue, SenderID, SendDate, Subject, Message, SenderIPAddress, RefNo, Readable, NotifyFor, SubGroupID, SupplierID, StyleMasterID, ExportOrderID, BOMMasterID, BookingID, SPOMasterID, PIMasterID);

                if (strSql.IsNotNullOrEmpty())
                {
                    await _gmtservice.ExecuteAsync(strSql, AppConstants.GMT_CONNECTION);
                }

            }
            catch (Exception ex)
            {
                //Dont know why reuse this SP
                //#region Create Error Notification
                //ErrorCreateNotification(MenuID, Event, ParameterColumn0, ParameterColumn1, ParentColumnID, ReferenceValue, SenderID, SendDate, Subject, Message, SenderIPAddress, RefNo, Readable,
                //    NotifyFor, SubGroupID, SupplierID, StyleMasterID, ExportOrderID, BOMMasterID, BookingID, SPOMasterID, PIMasterID);
                //#endregion

                throw new Exception("Notification System Not Configured Properly");
            }
            finally
            {
                if (_gmtservice.Connection.State == System.Data.ConnectionState.Open) _gmtservice.Connection.Close();
            }



        }


        private async Task UpdateNotification(Int32 MenuID, Int32 ForMenuID, String Event, Int32 ParentColumnID, String ReferenceValue, Int32 SenderID, DateTime SendDate,
                                              Int32 ReceiverID, DateTime ReceivedDate, String ReceiverIPAddress, bool AutoReceived, int NotifyFor, int SubGroupID, int SupplierID, int StyleMasterID, int ExportOrderID, int BOMMasterID, int BookingID, int SPOMasterID, int PIMasterID, int MessageID)
        {
            String strSql = String.Format(@"exec EPYSL..spUpdateNotification {0},{1},'{2}',{3},'{4}',{5},'{6}',{7},'{8}','{9}',{10},{11},{12},{13},'{14}','{15}','{16}','{17}','{18}','{19}','{20}'", MenuID, ForMenuID, Event, ParentColumnID, ReferenceValue, SenderID, SendDate, ReceiverID, ReceivedDate, ReceiverIPAddress, AutoReceived, NotifyFor, SubGroupID, SupplierID, StyleMasterID, ExportOrderID, BOMMasterID, BookingID, SPOMasterID, PIMasterID, MessageID);

            if (strSql.IsNotNullOrEmpty())
            {
                await _gmtservice.ExecuteAsync(strSql, AppConstants.GMT_CONNECTION);
            }

        }




        //private async Task ErrorCreateNotification(Int32 MenuID, String Event, Int32 ParameterColumn0, Int32 ParameterColumn1, Int32 ParentColumnID, String ReferenceValue, Int32 SenderID, DateTime SendDate,
        //        String Subject, String Message, String SenderIPAddress, String RefNo, Int32 Readable, int NotifyFor, int SubGroupID, int SupplierID, int StyleMasterID, int ExportOrderID, int BOMMasterID, int BookingID, int SPOMasterID, int PIMasterID)
        //{
        //    ConnectionManager objCon = new ConnectionManager(ConnectionName.EPYSL);
        //    bool blnTranStarted = false;
        //    try
        //    {
        //        String strSql = String.Format(@"exec spCreateNotification {0},'{1}',{2},{3},{4},'{5}',{6},'{7}','{8}','{9}','{10}','{11}',{12},{13},{14},{15},'{16}','{17}','{18}','{19}','{20}','{21}','0',0", MenuID, Event, ParameterColumn0, ParameterColumn1, ParentColumnID, ReferenceValue, SenderID, SendDate, Subject, Message, SenderIPAddress, RefNo, Readable, NotifyFor, SubGroupID, SupplierID, StyleMasterID, ExportOrderID, BOMMasterID, BookingID, SPOMasterID, PIMasterID);
        //        blnTranStarted = true;
        //        objCon.BeginTransaction();
        //        objCon.ExecuteScalarWrapper(strSql, blnTranStarted);
        //        blnTranStarted = false;
        //        objCon.CommitTransaction();
        //        objCon.Dispose();

        //    }
        //    catch (Exception ex)
        //    {
        //        if (blnTranStarted == true)
        //        {

        //            objCon.RollBack();
        //            objCon.Dispose();
        //        }

        //        throw new Exception("Error Notification Not Saved");
        //    }
        //    finally
        //    {
        //        objCon.Dispose();
        //    }
        //}
        private async Task UpdateOrderEventCalander(bool isRevise, string WithoutOB, int styleMasterId, int bomMasterId, int bookingId)
        {
            //SqlTransaction transaction = null;
            try
            {
                //await _gmtservice.Connection.OpenAsync();
                //transaction = _gmtservice.Connection.BeginTransaction();

                var bomMasters = await this.GetBOMMasterByID(bomMasterId);
                if (bomMasters.IsNotNull() && bomMasters.Count() > 0 && WithoutOB == "0")
                {
                    List<FBookingAcknowledge> objOrderBankMaster = await this.GetOrderBankMasterByStyleMasterID(bomMasters.First().StyleMasterID);
                    if (objOrderBankMaster.IsNotNull() && objOrderBankMaster.Count() > 0)
                    {
                        String EventName = "Fabric booking Acknowledge";
                        String CDaysList = "0";
                        String IsRevise = isRevise ? "Y" : "N";
                        String HasRevision = "N";
                        String HasCDays = "N";

                        String strEvent = String.Format(@"exec EPYSL..spUpdateOrderEventCalanderStatus '{0}','{1}','{2}','{3}','{4}','{5}'", objOrderBankMaster.First().OrderBankMasterID.ToString(), EventName, CDaysList, HasCDays, HasRevision, IsRevise);

                        if (strEvent.IsNotNullOrEmpty())
                        {
                            await _gmtservice.ExecuteAsync(strEvent, AppConstants.GMT_CONNECTION);
                        }

                        //await _gmtservice.Connection.ExecuteAsync("spUpdateOrderEventCalanderStatus", new
                        //{
                        //    OrderBankMasterID = objOrderBankMaster.First().OrderBankMasterID.ToString(),
                        //    EventName = EventName,
                        //    CDaysList = CDaysList,
                        //    HasCDays = HasCDays,
                        //    HasRevision = HasRevision,
                        //    IsRevise = IsRevise
                        //}, transaction, 30, CommandType.StoredProcedure);

                        if (IsRevise == "Y")
                        {
                            EventName = "Yarn booking";
                            IsRevise = "N";
                            HasRevision = "Y";

                            strEvent = String.Format(@"exec EPYSL..spUpdateOrderEventCalanderStatus '{0}','{1}','{2}','{3}','{4}','{5}'", objOrderBankMaster.First().OrderBankMasterID.ToString(), EventName, CDaysList, HasCDays, HasRevision, IsRevise);


                            if (strEvent.IsNotNullOrEmpty())
                            {
                                await _gmtservice.ExecuteAsync(strEvent, AppConstants.GMT_CONNECTION);
                            }

                            //await _gmtservice.Connection.ExecuteAsync("spUpdateOrderEventCalanderStatus", new
                            //{
                            //    OrderBankMasterID = objOrderBankMaster.First().OrderBankMasterID.ToString(),
                            //    EventName = EventName,
                            //    CDaysList = CDaysList,
                            //    HasCDays = HasCDays,
                            //    HasRevision = HasRevision,
                            //    IsRevise = IsRevise
                            //}, transaction, 30, CommandType.StoredProcedure);
                        }
                    }
                }


                // =================================

                #region Update ExportWorkOrderLifeCycleMaster for Export Order ID
                if (WithoutOB == "0")
                {
                    BookingChild objBookingChild = await this.GetBookingChildyBookingID(bookingId);
                    if (objBookingChild.IsNotNull())
                    {
                        try
                        {
                            if (bomMasters.IsNotNull() && bomMasters.Count() > 0)
                            {
                                String str = String.Format(@"exec EPYSL..spExportOrderLifeCycleMasterUpdate {0},'{1}','{2}',{3},{4}", bomMasters.First().ExportOrderID, objBookingChild.LabDipNo, "FabricItemAcknowledge", objBookingChild.ContactId, objBookingChild.BookingId);

                                if (str.IsNotNullOrEmpty())
                                {
                                    await _gmtservice.ExecuteAsync(str, AppConstants.GMT_CONNECTION);
                                }
                                //await _gmtservice.Connection.ExecuteAsync("spExportOrderLifeCycleMasterUpdate", new
                                //{
                                //    ExportOrderID = bomMasters.First().ExportOrderID,
                                //    SubGroupName = objBookingChild.LabDipNo,
                                //    FormName = "FabricItemAcknowledge",
                                //    ContactID = objBookingChild.ContactId,
                                //    BookingID = objBookingChild.BookingId
                                //}, transaction, 30, CommandType.StoredProcedure);
                            }
                        }
                        catch
                        {

                        }
                    }
                }
                else
                {
                    SampleBookingMaster objSPBooking = await this.GetSampleBookingMasterByBookingID(bookingId);
                    if (objSPBooking.IsNotNull())
                    {
                        String str = String.Format(@"exec EPYSL..spExportOrderLifeCycleMasterUpdate {0},'{1}','{2}',{3},{4}", objSPBooking.ExportOrderID, "Fabric", "FabricItemAcknowledge", objSPBooking.SupplierID, objSPBooking.BookingID);

                        if (str.IsNotNullOrEmpty())
                        {
                            await _gmtservice.ExecuteAsync(str, AppConstants.GMT_CONNECTION);
                        }


                        //await _gmtservice.Connection.ExecuteAsync("spExportOrderLifeCycleMasterUpdate", new
                        //{
                        //    ExportOrderID = bomMasters.First().ExportOrderID,
                        //    SubGroupName = "Fabric",
                        //    FormName = "FabricItemAcknowledge",
                        //    ContactID = objSPBooking.SupplierID,
                        //    BookingID = objSPBooking.BookingID
                        //}, transaction, 30, CommandType.StoredProcedure);
                    }
                }
                #endregion

                //transaction.Commit();
            }

            catch (Exception ex)
            {
                //if (transaction != null) transaction.Rollback();
                throw ex;
            }
            finally
            {
                if (_gmtservice.Connection.State == System.Data.ConnectionState.Open) _gmtservice.Connection.Close();
            }
        }
        private async Task<List<FBookingAcknowledge>> GetBOMMasterByID(int BOMMasterID)
        {
            string sql = $@"Select BM.ExportOrderID,BM.StyleMasterID, CE.CompanyID,BomMasterId = BM.BOMMasterID,BM.EWOStatusID
                            From {DbNames.EPYSL}..BOMMaster BM
                            Inner Join {DbNames.EPYSL}..StyleMaster SM On SM.StyleMasterID = BM.StyleMasterID
                            Inner Join {DbNames.EPYSL}..ContactCategoryTeam CCT On CCT.CategoryTeamID = SM.BuyerTeamID
                            Inner Join {DbNames.EPYSL}..CompanyEntity CE On CE.CompanyID = BM.CompanyID
                            Where BM.BOMMasterID = {BOMMasterID}";

            try
            {
                await _service.Connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                List<FBookingAcknowledge> obj = records.Read<FBookingAcknowledge>().ToList();
                return obj;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _service.Connection.Close();
            }
        }
        private async Task<List<FBookingAcknowledge>> GetOrderBankMasterByStyleMasterID(int styleMasterId)
        {
            string sql = $@"Select OrderBankMasterID 
                            From {DbNames.EPYSL}..OrderBankMaster
                            Where StyleMasterID = {styleMasterId}";

            try
            {
                await _service.Connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                List<FBookingAcknowledge> obj = records.Read<FBookingAcknowledge>().ToList();
                return obj;
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

        private async Task<BookingChild> GetBookingChildyBookingID(int bookingId)
        {
            string sql = $@"Select BookingId = BC.BookingID, BomMasterId = BC.BOMMasterID, ContactId = BC.ContactID, LabDipNo = IGS.SubGroupName
                            From {DbNames.EPYSL}..BookingChild BC           
                            Inner Join {DbNames.EPYSL}..ItemSubGroup IGS On IGS.SubGroupID = BC.SubGroupID                  
                            Where BC.BookingID = {bookingId}
                            Group By BC.BookingID,BC.BOMMasterID,BC.ContactID,IGS.SubGroupName";

            try
            {
                await _service.Connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                List<BookingChild> obj = records.Read<BookingChild>().ToList();
                return obj.First();
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
        private async Task<SampleBookingMaster> GetSampleBookingMasterByBookingID(int bookingId)

        {
            string sql = $@"SELECT SPB.ExportOrderID,SPB.SupplierID,SPB.BookingID
                            FROM  {DbNames.EPYSL}..SampleBookingMaster SPB
                            Inner Join {DbNames.EPYSL}..Contacts C On C.ContactID = SPB.SupplierID
                            Inner Join {DbNames.EPYSL}..SampleType ST On ST.SampleTypeID = SPB.SampleID
							Left Join {DbNames.EPYSL}..StyleMaster SM On SM.StyleMasterID = SPB.StyleMasterID
							Left Join (Select StyleMasterID, ExportOrderNo = Max(ExportOrderNo) from {DbNames.EPYSL}..ExportOrderMaster Group by StyleMasterID) EM On EM.StyleMasterID = SPB.StyleMasterID
							Left Join {DbNames.EPYSL}..SampleBookingStyleNo SPBR On SPBR.BDSStyleID = SPB.BDSStyleID And SPBR.BookingID = SPB.BookingID
							Left Join {DbNames.EPYSL}..CompanyEntity CE On CE.CompanyID = SPB.ExecutionCompanyID
					        Where SPB.BookingID = {bookingId}";

            try
            {
                await _service.Connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                List<SampleBookingMaster> obj = records.Read<SampleBookingMaster>().ToList();
                return obj.First();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _service.Connection.Close();
            }
        }
        /*
         * 
         * public static BookingChild GetBookingChildyBookingID(String BookingID)
        {
            ConnectionManager conManager = new ConnectionManager(ConnectionName.EPYSL);
            BookingChild newBookingChild = null;
            IDataReader reader = null;
            String sql = String.Format(@"Select BC.BookingID,BC.BOMMasterID,BC.ContactID,IGS.SubGroupName
                                        From BookingChild BC           
                                        Inner Join ItemSubGroup IGS On IGS.SubGroupID = BC.SubGroupID                  
                                        Where BC.BookingID = {0}
                                        Group By BC.BookingID,BC.BOMMasterID,BC.ContactID,IGS.SubGroupName", BookingID.ToInt());
            try
            {
                conManager.OpenDataReader(sql, out reader);
                while (reader.Read())
                {
                    newBookingChild = new BookingChild();
                    newBookingChild._BookingID = reader.GetInt32("BookingID");
                    newBookingChild._BOMMasterID = reader.GetInt32("BOMMasterID");
                    newBookingChild._ContactID = reader.GetInt32("ContactID");
                    newBookingChild._SubGroupName = reader.GetString("SubGroupName");
                    newBookingChild.SetUnchanged();

                }

                return newBookingChild;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                if (reader != null && !reader.IsClosed)
                    reader.Close();
            }
        }

        */
        public async Task<SampleBookingMaster> GetAllAsync(int id)
        {
            var sql = $@"
               WITH UNACK AS
                (
	                SELECT FBA.BookingNo, UnAcknowledgeReason = MAX(FBA.UnAcknowledgeReason), UnAcknowledgeBy = MAX(FBA.UnAcknowledgeBy)
	                FROM FBookingAcknowledge FBA
	                WHERE FBA.BookingID = {id}
	                GROUP BY FBA.BookingNo
                )
                Select SBM.*, C.ShortName BuyerName, CCT.TeamName BuyerTeamName, 
                UnAcknowledgeReason = CASE WHEN ISNULL(SBM.UnAcknowledgeReason,'') = '' THEN UNACK.UnAcknowledgeReason ELSE SBM.UnAcknowledgeReason END,
                LabdipUnAcknowledgeBY = UNACK.UnAcknowledgeBy
                From {DbNames.EPYSL}..SampleBookingMaster SBM
                Inner Join {DbNames.EPYSL}..Contacts C On C.ContactID = SBM.BuyerID
                Inner Join {DbNames.EPYSL}..ContactCategoryTeam CCT On CCT.CategoryTeamID = SBM.BuyerTeamID
                LEFT JOIN UNACK ON UNACK.BookingNo = SBM.BookingNo
                Where SBM.BookingID = {id}";

            try
            {
                await _gmtservice.Connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                SampleBookingMaster data = await records.ReadFirstOrDefaultAsync<SampleBookingMaster>();
                Guard.Against.NullObject(data);
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _gmtservice.Connection.Close();
            }
        }

        public async Task<BookingMaster> GetAllBookingAsync(int id)
        {
            var sql = $@"WITH UNACK AS
            (
	            SELECT FBA.BookingNo, UnAcknowledgeReason = MAX(FBA.UnAcknowledgeReason), UnAcknowledgeBy = MAX(FBA.UnAcknowledgeBy)
	            FROM FBookingAcknowledge FBA
	            WHERE FBA.BookingID = {id}
	            GROUP BY FBA.BookingNo
            )
            Select BM.*, BAR.ReasonName, CancelReasonName = CR.ReasonName, BM.PriceReProposeReasonID, PriceReProposeReasonName = PPR.ReasonName, CS.Name SupplierName,ContactPerson = ISNULL(SMI.Name,''), 
            IsAcknowledged = Case When ISnull(BM.RevisionNo,0) = ISnull(BIA.PreProcessRevNo,0) then 1 else 0 end, C.ShortName BuyerName, CCT.TeamName BuyerTeamName,
            SubGroupName = ISG.SubGroupName,
            UnAcknowledgeReason = CASE WHEN ISNULL(BM.UnAcknowledgeReason,'') = '' THEN UNACK.UnAcknowledgeReason ELSE BM.UnAcknowledgeReason END,
            OrderBankMasterID = UNACK.UnAcknowledgeBy
            from {DbNames.EPYSL}..BookingMaster BM
            Inner Join (Select BOMMasterID,BookingID From {DbNames.EPYSL}..BookingChild 
	            Where BookingID = {id}
	            Group By BOMMasterID,BookingID
            ) BC On BC.BookingID = BM.BookingID 
            Inner Join {DbNames.EPYSL}..BOMMaster BOM On BOM.BOMMasterID = BC.BOMMasterID
            Inner Join {DbNames.EPYSL}..ExportOrderMaster EOM On EOM.ExportOrderID = BOM.ExportOrderID And BOM.StyleMasterID = EOM.StyleMasterID
            Inner Join {DbNames.EPYSL}..StyleMaster SM On SM.StyleMasterID = EOM.StyleMasterID And BOM.StyleMasterID = SM.StyleMasterID
            Inner Join {DbNames.EPYSL}..Contacts C On C.ContactID = SM.BuyerID
            Inner Join {DbNames.EPYSL}..ContactCategoryTeam CCT On CCT.CategoryTeamID = SM.BuyerTeamID
            Left Join {DbNames.EPYSL}..Contacts CS On CS.ContactID = BM.SupplierID
            Left Join {DbNames.EPYSL}..BookingAdditionalReason BAR On BAR.ReasonID = BM.ReasonID
            Left Join {DbNames.EPYSL}..CancelReason CR On CR.ReasonID = BM.CancelReasonID
            Left Join {DbNames.EPYSL}..PriceReProposeReason PPR On PPR.ReasonID = BM.PriceReProposeReasonID
            left join {DbNames.EPYSL}..BookingItemAcknowledge BIA On BIA.BookingID = BM.BookingID 
            Left Join {DbNames.EPYSL}..ContactManagementInfo SMI On SMI.ManagementID = BM.ContactPersonID
            Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = BM.SubGroupID
            LEFT JOIN UNACK ON UNACK.BookingNo = BM.BookingNo
            Where BM.BookingID = {id}";

            try
            {

                await _gmtservice.Connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                BookingMaster data = await records.ReadFirstOrDefaultAsync<BookingMaster>();
                Guard.Against.NullObject(data);
                return data;
            }
            catch (Exception ex)
            {

                throw ex;
            }
            finally
            {
                _gmtservice.Connection.Close();
            }
        }

        public async Task<List<SampleBookingMaster>> GetAllSampleBookingByIDAsync(string id)
        {
            var sql = $@"
            ;Select * From {DbNames.EPYSL}..SampleBookingMaster Where BookingID in ({id})";

            try
            {
                await _gmtservice.Connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                List<SampleBookingMaster> obj = records.Read<SampleBookingMaster>().ToList();
                Guard.Against.NullObject(obj);
                return obj;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _gmtservice.Connection.Close();
            }
        }

        public async Task<List<BookingMaster>> GetAllBookingMasterByIDAsync(string id)
        {
            var sql = $@"Select  BM.* from {DbNames.EPYSL}..BookingMaster BM Where BM.BookingID in ({id})";

            try
            {
                await _gmtservice.Connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                List<BookingMaster> obj = records.Read<BookingMaster>().ToList();
                Guard.Against.NullObject(obj);

                return obj;
            }
            catch (Exception ex)
            {

                throw ex;
            }
            finally
            {
                _gmtservice.Connection.Close();
            }
        }

        public async Task<List<BookingChild>> GetAllInHouseBookingByBookingNo(string bookingno)
        {
            var sql = $@"Select BKM.BookingID, BKM.BookingNo, BKC.BOMMasterID, BKC.ItemGroupID, BKC.SubGroupID, ISG.SubGroupName GroupName, BKC.ContactID, RevisionNo = MAX(ISNULL(BIA.RevisionNo,0)), TechPackId = MAX(ISNULL(BIA.RevisionNo,0))
                ,SM.StyleMasterID, SM.StyleNo, LengthYds= SM.SeasonID, SM.FinancialYearID, BKM.Remarks, FBA.UnAcknowledgeReason, YarnType = ISV.SegmentValue, YarnProgram = ETV.ValueName
                From {DbNames.EPYSL}..BookingMaster BKM
                Inner Join {DbNames.EPYSL}..BookingChild BKC On BKC.BookingID = BKM.BookingID
                Inner Join {DbNames.EPYSL}..BookingItemAcknowledge BIA On BIA.BookingID = BKM.BookingID And BIA.SubGroupID = BKC.SubGroupID
                Left Join {DbNames.EPYSL}..ContactAdditionalInfo CAI On CAI.ContactID = BKC.ContactID
                Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = BKC.SubGroupID
                LEFT JOIN {DbNames.EPYSL}..ExportOrderMaster EO ON EO.ExportOrderID = BKM.ExportOrderID
                LEFT JOIN {DbNames.EPYSL}..StyleMaster SM ON SM.StyleMasterID = EO.StyleMasterID
                LEFT JOIN FBookingAcknowledge FBA ON FBA.BookingID = BKM.BookingID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV On ISV.SegmentValueID = BKC.A1ValueID
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = BKC.YarnBrandID
                Where BIA.WithoutOB = 0 And BKM.BookingNo = '{bookingno}' And ISNULL(CAI.InHouse,0) = 1
                Group By BKM.BookingID, BKM.BookingNo, BKC.BOMMasterID, BKC.ItemGroupID, BKC.SubGroupID, ISG.SubGroupName, BKC.ContactID,
                SM.StyleMasterID, SM.StyleNo, SM.SeasonID, SM.FinancialYearID, BKM.Remarks, FBA.UnAcknowledgeReason,ISV.SegmentValue, ETV.ValueName";

            try
            {
                await _gmtservice.Connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                List<BookingChild> obj = records.Read<BookingChild>().ToList();
                return obj;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _gmtservice.Connection.Close();
            }
        }
        public async Task<List<BookingMaster>> GetBookingMasterByNo(string bookingno)
        {
            var sql = $@"SELECT * 
                         FROM {DbNames.EPYSL}..BookingMaster 
                         WHERE BookingNo = '{bookingno}';";

            try
            {
                await _gmtservice.Connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                List<BookingMaster> obj = records.Read<BookingMaster>().ToList();
                return obj;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _gmtservice.Connection.Close();
            }
        }
        public async Task<List<SampleBookingMaster>> GetBookingMasterByNoSample(string bookingno)
        {
            var sql = $@"SELECT * 
                         FROM {DbNames.EPYSL}..SampleBookingMaster 
                         WHERE BookingNo = '{bookingno}'

                        SELECT SBC.*
	                    FROM {DbNames.EPYSL}..SampleBookingConsumption SBC
	                    INNER JOIN {DbNames.EPYSL}..SampleBookingMaster SBM ON SBM.BookingID = SBC.BookingID
	                    WHERE SBM.BookingNo = '{bookingno}'";

            try
            {
                await _gmtservice.Connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                List<SampleBookingMaster> obj = records.Read<SampleBookingMaster>().ToList();
                List<SampleBookingConsumption> childs = records.Read<SampleBookingConsumption>().ToList();
                if (childs.IsNotNull() && childs.Count() > 0)
                {
                    obj.ForEach(x =>
                    {
                        var childObj = childs.Where(y => y.BookingID == x.BookingID).OrderBy(y => y.SubGroupID);
                        x.SubGroupID = childObj.IsNotNull() ? childObj.First().SubGroupID : x.SubGroupID;
                    });
                }
                return obj;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _gmtservice.Connection.Close();
            }
        }

        public async Task<List<BookingChild>> GetAllInHouseSampleBookingByBookingNo(string bookingno)
        {
            var sql = $@"Select BKM.BookingID, BKM.BookingNo, 0 BOMMasterID, BKC.ItemGroupID, BKC.SubGroupID, ISG.SubGroupName GroupName, 0 ContactID, TechPackId = MAX(ISNULL(BKM.RevisionNo,0)),
                        LengthYds = SM.SeasonID
                        From {DbNames.EPYSL}..SampleBookingMaster BKM
                        Inner Join {DbNames.EPYSL}..SampleBookingConsumptionChild BKC On BKC.BookingID = BKM.BookingID
						Left Join (Select BookingID,RevisionNo, WithoutOB from {DbNames.EPYSL}..BookingItemAcknowledge group By BookingID,RevisionNo, WithoutOB)BIA On BIA.BookingID = BKM.BookingID --And BIA.SubGroupID = BKC.SubGroupID
						Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = BKC.SubGroupID
                        LEFT JOIN {DbNames.EPYSL}..StyleMaster SM ON SM.StyleMasterID = BKM.StyleMasterID
                        Where BIA.WithoutOB = 1 And BKM.BookingNo = '{bookingno}'
	                    Group By BKM.BookingID, BKM.BookingNo, BKC.ItemGroupID, BKC.SubGroupID, ISG.SubGroupName,SM.SeasonID";

            //var sql = $@"WITH
            //        TechRev AS
            //        (
            //         SELECT BKM.BookingID, TechPackId = MAX(ISNULL(BKM.RevisionNo,0)), RevisionNo = MAX(ISNULL(BKM.RevisionNo,0))
            //         FROM {DbNames.EPYSL}..SampleBookingMaster BKM
            //         WHERE BKM.BookingNo = '{bookingno}' 
            //         GROUP BY BKM.BookingID
            //        )
            //        SELECT BKM.BookingID, BKM.BookingNo, 0 BOMMasterID,  BKC.ItemGroupID, BKM.StyleNo, BKC.SubGroupID, GroupName = ISG.SubGroupName, 0 ContactID, TechPackId = TR.TechPackId, RevisionNo = TR.RevisionNo
            //        ,BKM.StyleMasterID, StyleNo = CASE WHEN ISNULL(BKM.StyleMasterID,0) = 0 THEN BKM.StyleNo ELSE SM.StyleNo END, SM.SeasonID,  BKM.Remarks,FBA.UnAcknowledgeReason, 
            //        FinancialYearID = (Select top 1 FY.FinancialYearID From {DbNames.EPYSL}..FinancialYear FY Where BKM.BookingDate between FY.StartMonth and FY.EndMonth)
            //        FROM {DbNames.EPYSL}..SampleBookingMaster BKM
            //        INNER Join {DbNames.EPYSL}..SampleBookingConsumptionChild BKC On BKC.BookingID = BKM.BookingID
            //        LEFT JOIN TechRev TR ON TR.BookingID = BKM.BookingID
            //        LEFT Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = BKC.SubGroupID
            //        LEFT JOIN {DbNames.EPYSL}..StyleMaster SM ON SM.StyleMasterID = BKM.StyleMasterID
            //        LEFT JOIN FBookingAcknowledge FBA ON FBA.BookingID = BKM.BookingID
            //        WHERE BKM.BookingNo = '{bookingno}';";

            try
            {
                await _gmtservice.Connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                List<BookingChild> obj = records.Read<BookingChild>().ToList();
                return obj;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _gmtservice.Connection.Close();
            }
        }

        public async Task<List<FabricBookingAcknowledge>> GetAllFabricBookingAcknowledgeByBookingNoAndGroupName(string bookingno)
        {
            var sql = $@"Select BIA.*,ISG.SubGroupName 
                                        From {DbNames.EPYSLTEX}..FabricBookingAcknowledge BIA
                                        Inner Join {DbNames.EPYSL}..BookingMaster BM On BM.BookingID = BIA.BookingID
                                        Inner Join {DbNames.EPYSL}..ItemGroup IG On IG.ItemGroupID = BIA.ItemGroupID
                                        Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = BIA.SubGroupID
                                        Inner Join {DbNames.EPYSL}..ItemGroupSubType IGST On IGST.GroupSubTypeID = IG.GroupSubTypeID
                                        Where BIA.WithoutOB = 0 And BM.BookingNo = '{bookingno}' And ISG.SubGroupName In ('Fabric','Collar','Cuff')";

            try
            {
                await _gmtservice.Connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                List<FabricBookingAcknowledge> obj = records.Read<FabricBookingAcknowledge>().ToList();
                return obj;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _gmtservice.Connection.Close();
            }
        }
        public async Task<List<FabricBookingAcknowledge>> GetAllSampleFabricBookingAcknowledgeByBookingNoAndGroupName(string bookingno, string subgroupname)
        {
            var sql = $@"Select BIA.*,ISG.SubGroupName 
                        From {DbNames.EPYSLTEX}..FabricBookingAcknowledge BIA
                        Inner Join {DbNames.EPYSL}..SampleBookingMaster BM On BM.BookingID = BIA.BookingID
                        Inner Join {DbNames.EPYSL}..ItemGroup IG On IG.ItemGroupID = BIA.ItemGroupID
                        Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = BIA.SubGroupID
                        Inner Join {DbNames.EPYSL}..ItemGroupSubType IGST On IGST.GroupSubTypeID = IG.GroupSubTypeID
                        Where BIA.WithoutOB = 1 And BM.BookingNo = '{bookingno}' And ISG.SubGroupName In ('Fabric','Collar','Cuff')";

            try
            {
                await _gmtservice.Connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                List<FabricBookingAcknowledge> obj = records.Read<FabricBookingAcknowledge>().ToList();
                return obj;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _gmtservice.Connection.Close();
            }
        }

        public async Task<List<BookingItemAcknowledge>> GetAllBookingItemAcknowledgeByBookingNo(string bookingno)
        {
            var sql = $@"Select BIA.*,UnAcknowledgeBy=  IIF(ISNULL(FBA.UnAcknowledgeBy,0) = 0, ISNULL(FA.RejectByPMC,0),ISNULL(FBA.UnAcknowledgeBy,0)),
                        UnAcknowledgeDate=  ISNULL(FBA.UnAcknowledgeDate,FA.RejectDatePMC)
                        From {DbNames.EPYSL}..BookingItemAcknowledge BIA
                        Inner Join {DbNames.EPYSLTEX}..FabricBookingAcknowledge FBA On FBA.BookingID = BIA.BookingID 
                        INNER JOIN {DbNames.EPYSLTEX}..FBookingAcknowledge FA ON FA.BookingID = BIA.BookingID
                        Inner Join {DbNames.EPYSL}..BookingMaster BM On BM.BookingID = BIA.BookingID
                        Inner Join {DbNames.EPYSL}..ItemGroup IG On IG.ItemGroupID = BIA.ItemGroupID
                        Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = BIA.SubGroupID
                        Inner Join {DbNames.EPYSL}..ItemGroupSubType IGST On IGST.GroupSubTypeID = IG.GroupSubTypeID
                        Where BM.BookingNo = '{bookingno}'";

            try
            {
                await _gmtservice.Connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                List<BookingItemAcknowledge> obj = records.Read<BookingItemAcknowledge>().ToList();
                return obj;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _gmtservice.Connection.Close();
            }
        }
        public async Task<List<BookingItemAcknowledge>> GetAllBookingItemAcknowledgeByBookingIDAndWithOutOB(string bookingID)
        {
            var sql = $@"Select BIA.* ,UnAcknowledgeBy = IIF(ISNULL(FBA.UnAcknowledgeBy,0) = 0, ISNULL(FA.RejectByPMC,0),ISNULL(FBA.UnAcknowledgeBy,0)),
                        UnAcknowledgeDate=  ISNULL(FBA.UnAcknowledgeDate,FA.RejectDatePMC)
                        From {DbNames.EPYSL}..BookingItemAcknowledge BIA
                        Inner Join {DbNames.EPYSLTEX}..FabricBookingAcknowledge FBA On FBA.BookingID = BIA.BookingID 
                        INNER JOIN {DbNames.EPYSLTEX}..FBookingAcknowledge FA ON FA.BookingID = BIA.BookingID
                        Inner Join {DbNames.EPYSL}..ItemGroup IG On IG.ItemGroupID = BIA.ItemGroupID
                        Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = BIA.SubGroupID
                        Where BIA.WithoutOB = 1 And BIA.BookingID in ({bookingID})";

            try
            {
                await _gmtservice.Connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                List<BookingItemAcknowledge> obj = records.Read<BookingItemAcknowledge>().ToList();
                return obj;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _gmtservice.Connection.Close();
            }
        }
        public async Task<FreeConceptMaster> GetAllAsyncR(int id)
        {
            var sql = $@"
            ;select item.BookingID from FreeConceptMaster FC
			INNER JOIN FBookingAcknowledge FA ON FA.BookingID = item.BookingID
        where item.BookingID= {id}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                FreeConceptMaster data = await records.ReadFirstOrDefaultAsync<FreeConceptMaster>();
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

        public async Task<List<FBookingAcknowledgeChildColor>> GetAllAsyncColorIDs(string colorIDs)
        {
            var sql = $@"
            ;select a.ColorCode,b.SegmentValue ColorName from {DbNames.EPYSL}..FabricColorBookSetup a
            inner join {DbNames.EPYSL}..ItemSegmentValue b on b.SegmentValueID=a.ColorID
            where SegmentValue  IN(Select * From [dbo].[fnReturnStringArray] ('{colorIDs}', ','))";

            try
            {
                await _gmtservice.Connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                List<FBookingAcknowledgeChildColor> obj = records.Read<FBookingAcknowledgeChildColor>().ToList();
                return obj;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _gmtservice.Connection.Close();
            }
        }
        public async Task<int> CheckIsBookingApprovedAsync(string bookingNo)
        {
            SqlTransaction transaction = null;
            try
            {
                await _service.Connection.OpenAsync();
                transaction = _service.Connection.BeginTransaction();

                YarnAllocationChild data = new YarnAllocationChild();
                //await _service.ExecuteAsync("sp_CheckIsBookingApproved", new { bookingNo = bookingNo }, 30, CommandType.StoredProcedure, transaction);
                await _connection.ExecuteAsync("sp_CheckIsBookingApproved", new { bookingNo = bookingNo }, transaction, 30, CommandType.StoredProcedure);

                transaction.Commit();

                return 0;
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                if (ex.Message.Contains('~')) throw new Exception(ex.Message.Split('~')[0]);
                throw ex;
            }
            finally
            {
                _service.Connection.Close();
            }

        }
        public async Task UpdateEntityAsync(SampleBookingMaster entity)
        {
            SqlTransaction transaction = null;
            try
            {
                await _gmtservice.Connection.OpenAsync();
                transaction = _gmtservice.Connection.BeginTransaction();

                await _gmtservice.SaveSingleAsync(entity, transaction);

                transaction.Commit();
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                throw ex;
            }
            finally
            {
                _gmtservice.Connection.Close();
            }
        }

        public async Task UpdateEntityAsyncR(FreeConceptMaster entity)
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
                if (_connection.State == System.Data.ConnectionState.Open) _connection.Close();
            }
        }

        public async Task<BDSDependentTNACalander> GetAllAsyncBDSTNAEvent_HK()
        {
            var sql = $@"
            ;Select * From BDSTNAEvent_HK";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                BDSDependentTNACalander data = new BDSDependentTNACalander();
                data.BDSTNAEvent_HKNames = records.Read<BDSTNAEvent_HK>().ToList();
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

        public async Task<List<BDSDependentTNACalander>> GetPagedAsyncTNA(PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By BDSEventID ASC" : paginationInfo.OrderBy;

            var sql = $@"--Master Information
                        Select * From (SELECT BT.BDSEventID, FA.BookingNo, BH.EventDescription,CTO.ShortName BuyerName,
                        CCT.TeamName BuyerTeamName,BT.BookingID, FA.DateAdded AcknowledgeDate,
                        BT.BookingChildID,BT.EventID,BT.TNADays,BT.BookingDate,BT.EventDate,BT.CompleteDate,
                        BT.RevisionPending,BT.RevisionCompleteDate,BT.SeqNo,BT.SystemEvent,BT.HasDependent,
                        BT.IsHoliDay,BT.IsPass,IM.ItemName,SBC.Segment1Desc Construction,SBC.Segment2Desc Composition,
                        SBC.Segment3Desc Color,SBC.Segment4Desc GSM,SBC.Segment5Desc FabricWidth,T.TechnicalName,
                        KMS.SubClassName MachineType,BC.LengthYds,BC.LengthInch,SBC.Segment6Desc DyeingType,SBC.Remarks Instruction,Count(*) Over() TotalRows
                        from BDSDependentTNACalander BT
                        INNER JOIN BDSTNAEvent_HK BH ON BH.EventID=BT.EventID
                        INNER JOIN FBookingAcknowledge FA ON FA.BookingID=BT.BookingID
                        INNER JOIN FBookingAcknowledgeChild BC ON BC.BookingChildID=BT.BookingChildID
                        INNER JOIN {DbNames.EPYSL}..SampleBookingMaster SBM ON SBM.BookingID = BC.BookingID
                        INNER JOIN {DbNames.EPYSL}..SampleBookingConsumption SBC ON SBC.BookingID = SBM.BookingID AND SBC.ConsumptionID = BC.ConsumptionID
                        INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = BC.ItemMasterID
                        INNER JOIN {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = BC.SubGroupID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV On ISV.SegmentValueID = BC.A1ValueID
                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = BC.YarnBrandID
                        LEFT JOIN FabricTechnicalName T ON T.TechnicalNameId = BC.TechnicalNameID
                        LEFT JOIN KnittingMachineSubClass KMS ON KMS.SubClassID = BC.MachineTypeId
                        LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = FA.BuyerID
                        LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = FA.BuyerTeamID
                    ) A ";

            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<BDSDependentTNACalander>(sql);
        }

        public async Task<List<BDSDependentTNACalander>> GetPagedAsyncEventlist(PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By EventID ASC" : paginationInfo.OrderBy;

            var sql = $@"--Master Information
                        SELECT EventID, EventDescription  from BDSTNAEvent_HK ";

            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<BDSDependentTNACalander>(sql);
        }

        public async Task<List<BDSDependentTNACalander>> GetBoookingList(PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By BookingID ASC" : paginationInfo.OrderBy;

            var sql =
                $@"--BookingList
                    ;Select * From (SELECT FA.BookingID,FA.BookingNo
                    From FBookingAcknowledge FA
                    Group By FA.BookingID,FA.BookingNo
                ) A ";

            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<BDSDependentTNACalander>(sql);
        }

        public async Task<List<BDSDependentTNACalander>> GetbookingWiseList(PaginationInfo paginationInfo, DateTime FromDate, DateTime ToDate)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By BDSEventID ASC" : paginationInfo.OrderBy;

            var sql = $@"
            ;Select * From (SELECT BT.BDSEventID, FA.BookingNo, BH.EventDescription,CTO.ShortName BuyerName,
                        CCT.TeamName BuyerTeamName,BT.BookingID, FA.DateAdded AcknowledgeDate,
                        BT.BookingChildID,BT.EventID,BT.TNADays,BT.BookingDate,BT.EventDate,BT.CompleteDate,
                        BT.RevisionPending,BT.RevisionCompleteDate,BT.SeqNo,BT.SystemEvent,BT.HasDependent,
                        BT.IsHoliDay,BT.IsPass,IM.ItemName,SBC.Segment1Desc Construction,SBC.Segment2Desc Composition,
                        SBC.Segment3Desc Color,SBC.Segment4Desc GSM,SBC.Segment5Desc FabricWidth,T.TechnicalName,
                        KMS.SubClassName MachineType,BC.LengthYds,BC.LengthInch,SBC.Segment6Desc DyeingType,SBC.Remarks Instruction,Count(*) Over() TotalRows
                        from BDSDependentTNACalander BT
                        INNER JOIN BDSTNAEvent_HK BH ON BH.EventID=BT.EventID
                        INNER JOIN FBookingAcknowledge FA ON FA.BookingID=BT.BookingID
                        INNER JOIN FBookingAcknowledgeChild BC ON BC.BookingChildID=BT.BookingChildID
                        INNER JOIN {DbNames.EPYSL}..SampleBookingMaster SBM ON SBM.BookingID = BC.BookingID
                        INNER JOIN {DbNames.EPYSL}..SampleBookingConsumption SBC ON SBC.BookingID = SBM.BookingID AND SBC.ConsumptionID = BC.ConsumptionID
                        INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = BC.ItemMasterID
                        INNER JOIN {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = BC.SubGroupID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV On ISV.SegmentValueID = BC.A1ValueID
                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = BC.YarnBrandID
                        LEFT JOIN FabricTechnicalName T ON T.TechnicalNameId = BC.TechnicalNameID
                        LEFT JOIN KnittingMachineSubClass KMS ON KMS.SubClassID = BC.MachineTypeId
                        LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = FA.BuyerID
                        LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = FA.BuyerTeamID
                        WHERE BT.EventDate between '{FromDate.ToShortDateString()}' AND '{ToDate.ToShortDateString()}'
            ) A ";
            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<BDSDependentTNACalander>(sql);
            //try
            //{
            //    await _connection.OpenAsync();
            //    var records = await _connection.QueryMultipleAsync(sql);
            //    BDSDependentTNACalander data = await records.ReadFirstOrDefaultAsync<BDSDependentTNACalander>();
            //    Guard.Against.NullObject(data);
            //    return data;
            //}
            //catch (Exception ex)
            //{
            //    throw ex;
            //}
            //finally
            //{
            //    _connection.Close();
            //}
        }

        public async Task<List<BDSDependentTNACalander>> GetbookingWiseTNAList(PaginationInfo paginationInfo, String ListData)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By BDSEventID ASC" : paginationInfo.OrderBy;

            var sql = $@"
            ;Select * From (SELECT BT.BDSEventID, FA.BookingNo, BH.EventDescription,CTO.ShortName BuyerName,
                        CCT.TeamName BuyerTeamName,BT.BookingID, FA.DateAdded AcknowledgeDate,
                        BT.BookingChildID,BT.EventID,BT.TNADays,BT.BookingDate,BT.EventDate,BT.CompleteDate,
                        BT.RevisionPending,BT.RevisionCompleteDate,BT.SeqNo,BT.SystemEvent,BT.HasDependent,
                        BT.IsHoliDay,BT.IsPass,IM.ItemName,SBC.Segment1Desc Construction,SBC.Segment2Desc Composition,
                        SBC.Segment3Desc Color,SBC.Segment4Desc GSM,SBC.Segment5Desc FabricWidth,T.TechnicalName,
                        KMS.SubClassName MachineType,BC.LengthYds,BC.LengthInch,SBC.Segment6Desc DyeingType,SBC.Remarks Instruction,Count(*) Over() TotalRows
                        from BDSDependentTNACalander BT
                        INNER JOIN BDSTNAEvent_HK BH ON BH.EventID=BT.EventID
                        INNER JOIN FBookingAcknowledge FA ON FA.BookingID=BT.BookingID
                        INNER JOIN FBookingAcknowledgeChild BC ON BC.BookingChildID=BT.BookingChildID
                        INNER JOIN {DbNames.EPYSL}..SampleBookingMaster SBM ON SBM.BookingID = BC.BookingID
                        INNER JOIN {DbNames.EPYSL}..SampleBookingConsumption SBC ON SBC.BookingID = SBM.BookingID AND SBC.ConsumptionID = BC.ConsumptionID
                        INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = BC.ItemMasterID
                        INNER JOIN {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = BC.SubGroupID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV On ISV.SegmentValueID = BC.A1ValueID
                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = BC.YarnBrandID
                        LEFT JOIN FabricTechnicalName T ON T.TechnicalNameId = BC.TechnicalNameID
                        LEFT JOIN KnittingMachineSubClass KMS ON KMS.SubClassID = BC.MachineTypeId
                        LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = FA.BuyerID
                        LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = FA.BuyerTeamID
                        WHERE BT.BookingID IN ({ListData})
                ) A ";
            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<BDSDependentTNACalander>(sql);
        }

        public async Task<List<FBookingAcknowledge>> GetBookingByBookingNo(string bookingNo)
        {
            var sql = $@"
            SELECT A.* 
            FROM FBookingAcknowledge A
            WHERE A.BookingNo LIKE '%{bookingNo}%'";

            return await _service.GetDataAsync<FBookingAcknowledge>(sql);
        }


        public async Task<List<BDSDependentTNACalander>> GetEventWiseTNA(PaginationInfo paginationInfo, String EventListData)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By BDSEventID ASC" : paginationInfo.OrderBy;

            var sql = $@"
            ;Select * From (SELECT BT.BDSEventID, FA.BookingNo, BH.EventDescription,CTO.ShortName BuyerName,
                        CCT.TeamName BuyerTeamName,BT.BookingID, FA.DateAdded AcknowledgeDate,
                        BT.BookingChildID,BT.EventID,BT.TNADays,BT.BookingDate,BT.EventDate,BT.CompleteDate,
                        BT.RevisionPending,BT.RevisionCompleteDate,BT.SeqNo,BT.SystemEvent,BT.HasDependent,
                        BT.IsHoliDay,BT.IsPass,IM.ItemName,SBC.Segment1Desc Construction,SBC.Segment2Desc Composition,
                        SBC.Segment3Desc Color,SBC.Segment4Desc GSM,SBC.Segment5Desc FabricWidth,T.TechnicalName,
                        KMS.SubClassName MachineType,BC.LengthYds,BC.LengthInch,SBC.Segment6Desc DyeingType,SBC.Remarks Instruction,Count(*) Over() TotalRows
                        from BDSDependentTNACalander BT
                        INNER JOIN BDSTNAEvent_HK BH ON BH.EventID=BT.EventID
                        INNER JOIN FBookingAcknowledge FA ON FA.BookingID=BT.BookingID
                        INNER JOIN FBookingAcknowledgeChild BC ON BC.BookingChildID=BT.BookingChildID
                        INNER JOIN {DbNames.EPYSL}..SampleBookingMaster SBM ON SBM.BookingID = BC.BookingID
                        INNER JOIN {DbNames.EPYSL}..SampleBookingConsumption SBC ON SBC.BookingID = SBM.BookingID AND SBC.ConsumptionID = BC.ConsumptionID
                        INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = BC.ItemMasterID
                        INNER JOIN {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = BC.SubGroupID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV On ISV.SegmentValueID = BC.A1ValueID
                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = BC.YarnBrandID
                        LEFT JOIN FabricTechnicalName T ON T.TechnicalNameId = BC.TechnicalNameID
                        LEFT JOIN KnittingMachineSubClass KMS ON KMS.SubClassID = BC.MachineTypeId
                        LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = FA.BuyerID
                        LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = FA.BuyerTeamID
                        WHERE BT.EventID IN ({EventListData})
                ) A ";
            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<BDSDependentTNACalander>(sql);
        }

        public async Task<List<BDSDependentTNACalander>> GetEventWiseList(PaginationInfo paginationInfo, int eventID)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By DepenEventID ASC" : paginationInfo.OrderBy;

            var sql = $@"Select * From (
                SELECT BDE.DepenEventID,BDH.EventDescription
                FROM BDSDependentTNAEvent_HK BDE
                INNER JOIN BDSTNAEvent_HK BDH ON BDH.EventID=BDE.DepenEventID
                WHERE BDE.EventID='{eventID}'
            ) A ";
            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<BDSDependentTNACalander>(sql);
        }

        private FreeConceptMaster GetFreeConceptMaster(FBookingAcknowledgeChild item, FBookingAcknowledge entity, int isBDS, int conceptId, string conceptNo, bool isGroupBy = false)
        {
            if (!item.Length.IsNotNullOrEmpty()) item.Length = 0.ToString();
            if (!item.Height.IsNotNullOrEmpty()) item.Height = 0.ToString();

            var freeConcept = new FreeConceptMaster()
            {
                ConceptID = conceptId,
                ConceptNo = conceptNo,
                ConceptDate = DateTime.Now,
                TrialNo = 0,
                TrialDate = null,
                ConceptFor = 1093,
                //KnittingTypeID = item.KnittingTypeId,
                KnittingTypeID = item.KTypeId,
                ConstructionId = item.ConstructionId,
                CompositionId = item.CompositionId,
                GSMId = item.GSMId,
                Qty = item.BookingQty,
                AddedBy = entity.AddedBy,
                DateAdded = DateTime.Now,
                TechnicalNameId = entity.PageName == "FabricBookingAcknowledge" ? 0 : item.TechnicalNameId,
                SubGroupID = item.SubGroupID,
                MCSubClassID = entity.PageName == "FabricBookingAcknowledge" ? 0 : item.MachineTypeId,
                CompanyID = item.TextileCompanyID,
                IsBDS = isBDS,
                ItemMasterID = item.ItemMasterID, //Use for Join For Bulk
                BookingID = item.BookingID, //Use for Join For Bulk
                BookingChildID = isGroupBy ? 0 : item.BookingChildID, //No Need For Bulk
                ConsumptionID = isGroupBy ? 0 : item.ConsumptionID, //No Need For Bulk
                ExportOrderID = item.ExportOrderID,
                BuyerID = entity.BuyerID,
                BuyerTeamID = entity.BuyerTeamID,
                MachineGauge = item.MachineGauge,
                MachineDia = item.MachineDia,
                BrandID = item.BrandID,
                PreProcessRevNo = entity.PreRevisionNo,
                GroupConceptNo = entity.BookingNo,

                ConceptTypeID = 1,
                FUPartID = item.FUPartID,
                ColorID = item.ColorID,
                Color = item.Color,

                Length = item.SubGroupID == 1 ? 0 : Convert.ToDecimal(item.Length),
                Width = item.SubGroupID == 1 ? 0 : Convert.ToDecimal(item.Height),

                QtyInKG = (Convert.ToDecimal(item.Length) *
                           Convert.ToDecimal(item.Height) *
                           Convert.ToDecimal(0.045) *
                           item.BookingQty) / 420,

                ExcessPercentage = item.ExcessPercentage,
                ExcessQty = item.ExcessQty,
                ExcessQtyInKG = item.ExcessQtyInKG,
                TotalQty = item.TotalQty,
                TotalQtyInKG = (Convert.ToDecimal(item.Length) *
                                Convert.ToDecimal(item.Height) *
                                Convert.ToDecimal(0.045) *
                                item.TotalQty) / 420 //item.TotalQtyInKG
            };
            return freeConcept;
        }

        private FreeConceptChildColor GetFreeConceptColorChild(FBookingAcknowledgeChild item, FreeConceptMaster freeConcept, int conceptChildId)
        {
            var colorChild = new FreeConceptChildColor()
            {
                CCColorID = conceptChildId,
                ColorId = item.ColorID,
                ColorName = item.Color,
                ConceptID = freeConcept.ConceptID,
                ColorCode = item.ColorCode
            };
            return colorChild;
        }

        private FreeConceptMRMaster GetFreeConceptMRMaster(FBookingAcknowledge entity, int fCMRMasterID, int conceptID, int isBDS)
        {
            var freeConceptMR = new FreeConceptMRMaster()
            {
                FCMRMasterID = fCMRMasterID,
                ReqDate = DateTime.Now,
                ConceptID = conceptID,
                TrialNo = 0,
                PreProcessRevNo = entity.PreRevisionNo,
                RevisionNo = entity.RevisionNo,
                RevisionBy = entity.AddedBy,
                RevisionDate = DateTime.Now,
                IsBDS = isBDS,
                FabricID = 0
            };
            return freeConceptMR;
        }

        private FreeConceptMRChild GetFreeConceptMRChild(int fCMRChildID, FreeConceptMRMaster freeConceptMR, YarnBookingChildItem ybci)
        {
            var mrChild = new FreeConceptMRChild()
            {
                FCMRChildID = fCMRChildID,
                FCMRMasterID = freeConceptMR.FCMRMasterID,
                ItemMasterID = ybci.YItemMasterID,
                YD = ybci.YD,
                ReqQty = ybci.NetYarnReqQty,
                UnitID = ybci.UnitID,
                Remarks = ybci.Remarks,
                ReqCone = 0,
                ShadeCode = ybci.ShadeCode,
                Distribution = ybci.Distribution,
                BookingQty = ybci.BookingQty,
                Allowance = ybci.Allowance,
                YarnCategory = CommonFunction.GetYarnShortForm(ybci.Segment1ValueDesc, ybci.Segment2ValueDesc, ybci.Segment3ValueDesc, ybci.Segment4ValueDesc, ybci.Segment5ValueDesc, ybci.Segment6ValueDesc, ybci.ShadeCode)
            };
            return mrChild;
        }

        private FBookingAcknowledgeChildDetails GetFBookingAcknowledgeChildDetail(int bookingCDetailsID, FBookingAcknowledgeChild item, FBookingAcknowledge entity, YarnBookingChildItem ybci)
        {
            var fBAChildDetail = new FBookingAcknowledgeChildDetails()
            {
                BookingCDetailsID = bookingCDetailsID,
                BookingChildID = item.BookingChildID,
                BookingID = entity.BookingID,
                ConsumptionID = item.ConsumptionID,
                ItemGroupID = item.ItemGroupID,
                SubGroupID = ybci.SubGroupId,
                ItemMasterID = ybci.YItemMasterID,
                OrderBankPOID = item.OrderBankPOID,
                ColorID = item.ColorID,
                SizeID = item.SizeID,
                TechPackID = item.TechPackID,
                ConsumptionQty = item.ConsumptionQty,
                BookingQty = ybci.BookingQty,
                BookingUnitID = item.BookingUnitID,
                RequisitionQty = ybci.RequiredQty,
                AddedBy = entity.AddedBy,
                DateAdded = DateTime.Now,
                ExecutionCompanyID = item.ExecutionCompanyID,
                DeliveryStart = null,
                DeliveryEnd = null,
                SecondarySizeID = item.SizeID,
                TechnicalNameId = item.TechnicalNameId,
                StitchLength = ybci.StitchLength
            };
            return fBAChildDetail;
        }

        public async Task<String> GetSampleTypeByBookingID(int bookingID)
        {
            var query = $@"{CommonQueries.GetSampleTypeByBookingID(bookingID)}";
            String SampleType = String.Empty;
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                Select2OptionModel data = records.Read<Select2OptionModel>().FirstOrDefault();
                if (data.IsNotNull())
                {
                    SampleType = data.text;
                }

                return SampleType;

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

        public async Task<FBookingAcknowledge> GetAllRevisionStatusByExportOrderIDAndSubGroupID(String ExportOrderNo, String SubGroupID)
        {
            var query =
               $@"With EWO as(Select *From {DbNames.EPYSL}..ExportOrderMaster Where ExportOrderNo='{ExportOrderNo}'),
                                    BOM as (
									
									    Select BMM.ExportOrderID,BMM.BOMMasterID,SubGroupID=1,BIGC.ContactID,RevisionNo=Sum(BIGC.RevisionNo), IsApproved= IsNull(Min(Convert(int,IsApproved)),0),
                                                IsSkipRevision = Max(Convert(int,IsNull(BIG.IsSkipRevision,0)))
                                                From {DbNames.EPYSL}..BOMItemGroup BIG
                                                Inner Join {DbNames.EPYSL}..BOMItemGroupContact BIGC On BIGC.BOMMasterID = BIG.BOMMasterID And BIGC.SubGroupID = BIG.SubGroupID And BIGC.ItemGroupID = BIG.ItemGroupID
										        Inner Join {DbNames.EPYSL}..BOMMaster BMM On BMM.BOMMasterID = BIG.BOMMasterID
                                                Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = BIG.SubGroupID
	                                            left Join {DbNames.EPYSL}..ContactAdditionalInfo SAI On SAI.ContactID = BIGC.ContactID
                                                Where BMM.ExportOrderID = (Select Top 1 ExportOrderID From EWO)  And ISG.SubGroupName in ('Fabric','Collar','Cuff')
                                                And Isnull(SAI.InHouse,0) = 1
												Group by BMM.ExportOrderID,BMM.BOMMasterID,BIGC.ContactID								
										
										),
                                        HasAB As (
			                                    Select A.ExportOrderID,B.BookingID,B.SupplierID,1 SubGroupID, Max(A.AdditionalBooking) AdditionalBooking,Max(A.PreProcessRevNo) PreProcessRevNo
									            From {DbNames.EPYSL}..BookingMaster  B
                                                Inner Join {DbNames.EPYSL}..BookingChild BC On BC.BookingID = B.BookingID 
									            Inner Join (
                                                            Select BIG.ExportOrderID,SupplierID,PreProcessRevNo = Case When Max(Convert(int,IsNull(BIG.IsSkipRevision,0))) = 1 then Max(BIG.RevisionNo) else Max(BM.PreProcessRevNo) end,
						                                    AdditionalBooking = Max(AdditionalBooking)
									                        From {DbNames.EPYSL}..BookingMaster BM
									                        Inner Join {DbNames.EPYSL}..BookingChild BC On BC.BookingID = BM.BookingID 
                                                            Inner Join BOM BIG On BIG.BOMMasterID = BC.BOMMasterID And BIG.SubGroupID = BC.SubGroupID And BIG.ContactID = BC.ContactID
									                        left Join {DbNames.EPYSL}..ContactAdditionalInfo SAI On SAI.ContactID = BM.SupplierID
									                        Where BM.ExportOrderID =  (Select Top 1 ExportOrderID From EWO) And BC.SubGroupID = {SubGroupID} And Isnull(SAI.InHouse,0) = 1
									                        Group By BIG.ExportOrderID,SupplierID
									                ) A On  A.ExportOrderID = B.ExportOrderID And A.SupplierID= B.SupplierID	
									            Where  B.ExportOrderID =  (Select Top 1 ExportOrderID From EWO)	 And BC.SubGroupID = {SubGroupID} 		
									            Group By A.ExportOrderID,B.BookingID,B.SupplierID
                                    )
                                    Select BOMStatus =Case When BMM.RevisionNo > BM.PreProcessRevNo And BIG.IsApproved=0 Then 'BOMRevisionPending'
                                                            When BIG.IsSkipRevision = 0 And BM.RevisionNeed=1 Then 'BookingRevisionPending'
							                                When BM.RevisionNo > BIA.PreProcessRevNo And BIG.IsApproved=1 And BM.Proposed=1 Then 'BookingIARevisionPending'
                                                            When BIG.IsSkipRevision = 0 And BM.RevisionNeed = 1 Then 'BookingRevisionPending' 
						                                   Else 'FBARevisionPending' End

                                    FROM {DbNames.EPYSL}..ExportOrderMaster EOM
                                    Inner Join BOM BMM On BMM.ExportOrderID = EOM.ExportOrderID
                                    Inner Join {DbNames.EPYSL}..BOMItemGroup BIG On BIG.BOMMasterID = BMM.BOMMasterID                                  
                                    Inner Join (
												Select BM.BookingID,BM.ExportOrderID,HAB.PreProcessRevNo,BM.RevisionNo,BM.Proposed,Convert(int,BM.RevisionNeed) RevisionNeed
			                                        From {DbNames.EPYSL}..BookingMaster BM
			                                        Inner Join HasAB HAB On HAB.ExportOrderID = BM.ExportOrderID And BM.AdditionalBooking = HAB.AdditionalBooking And BM.SupplierID = HAB.SupplierID  And BM.SupplierID = HAB.SupplierID And BM.BookingID = HAB.BookingID  
			                                        Where BM.ExportOrderID = (Select Top 1 ExportOrderID From EWO)
                                                    Group By BM.BookingID,BM.ExportOrderID,HAB.PreProcessRevNo,BM.RevisionNo,BM.Proposed,Convert(int,BM.RevisionNeed)
												
												) BM On BM.ExportOrderID = EOM.ExportOrderID
                                    Inner Join {DbNames.EPYSL}..BookingItemAcknowledge BIA On BIA.BookingID = BM.BookingID
                                    Left Join FabricBookingAcknowledge FBA On FBA.BookingID = BIA.BookingID
                                    Where EOM.ExportOrderID = (Select Top 1 ExportOrderID From EWO) And BIG.SubGroupID = {SubGroupID} And BIG.IsOnlineBooking  in (0,2)";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                FBookingAcknowledge data = records.Read<FBookingAcknowledge>().FirstOrDefault();
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

        public void SaveFabricBookingItemAcknowledgeBackup(String BookingNo, bool WithoutOB, ref SqlTransaction transaction)
        {

            String strSql = "";

            try
            {
                strSql = String.Format(@"exec spBackupFabricBookingItemAcknowledge '{0}','{1}'", BookingNo, WithoutOB ? "1" : "0");
                var records = _service.ExecuteWithTransactionAsync(strSql, ref transaction);
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                throw (ex);
            }


        }

        public void RollBackFabricBookingData(String BookingNo, bool WithoutOB, ref SqlTransaction transaction)
        {

            String strSql = "";

            try
            {
                //if (WithoutOB == false)
                //{
                strSql = String.Format(@"exec spRollbackBooking_Full '{0}'", BookingNo);
                var records = _gmtservice.ExecuteWithTransactionAsync(strSql, ref transaction);
                //}
                //else
                //{
                //    //strSql = String.Format(@"exec spRollbackBooking_Full '{0}'", BookingNo);
                //}



            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                throw (ex);
            }


        }
        public async Task<List<FBookingAcknowledgementLiabilityDistribution>> GetAllFBookingAckLiabilityByIDAsync(String BookingID)
        {
            var query =
               $@"WITH
                BM AS
                (
	                Select ISG.SubGroupName,LiabilitiesName=ETV.ValueName,UOM=U.DisplayUnitDesc,LiabilityQty=Sum(FALD.LiabilityQty), TotalValue = CONVERT(DECIMAL(18,4),Sum(FALD.LiabilityQty * FALD.Rate))
                    From FBookingAcknowledgementLiabilityDistribution FALD
                    INNER JOIN {DbNames.EPYSL}..BookingMaster BM ON BM.BookingID = FALD.BookingID
                    INNER Join {DbNames.EPYSL}..EntityTypeValue ETV On ETV.ValueID = FALD.LiabilitiesProcessID
                    LEFT Join {DbNames.EPYSL}..Unit U On U.UnitID = FALD.UnitID
                    Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = BM.SubGroupID
                    Where BM.BookingID in ({BookingID})
                    Group By ISG.SubGroupName,
                    ETV.ValueName,U.DisplayUnitDesc
                ),
                SBM AS
                (
	                Select ISG.SubGroupName,LiabilitiesName=ETV.ValueName,UOM=U.DisplayUnitDesc,LiabilityQty=Sum(FALD.LiabilityQty), TotalValue = CONVERT(DECIMAL(18,4),Sum(FALD.LiabilityQty * FALD.Rate))
	                From {DbNames.EPYSL}..SampleBookingMaster BM
	                Inner Join {DbNames.EPYSL}..SampleBookingConsumption BC On BC.BookingID = BM.BookingID
	                INNER JOIN FBookingAcknowledgementLiabilityDistribution FALD ON FALD.BookingID = BM.BookingID AND FALD.ConsumptionID = BC.ConsumptionID
	                INNER Join {DbNames.EPYSL}..EntityTypeValue ETV On ETV.ValueID = FALD.LiabilitiesProcessID
	                LEFT Join {DbNames.EPYSL}..Unit U On U.UnitID = FALD.UnitID
	                Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = BC.SubGroupID
                    Where BM.BookingID in ({BookingID})
	                Group By ISG.SubGroupName,
	                ETV.ValueName,U.DisplayUnitDesc
                ),
                FinalList AS
                (
	                SELECT * FROM BM
	                UNION
	                SELECT * FROM SBM
                )
                SELECT * FROM FinalList";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                List<FBookingAcknowledgementLiabilityDistribution> data = records.Read<FBookingAcknowledgementLiabilityDistribution>().ToList();
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
        public async Task<List<FBookingAcknowledgementYarnLiability>> GetAllFBookingAckYarnLiabilityByIDAsync(String BookingID)
        {
            var query =
               $@";With YBM As
                (
                    Select * From YarnBookingMaster Where BookingID in ({BookingID})
                ),
                YBM_New As
                (
	                Select * From YarnBookingMaster_New Where BookingID in ({BookingID})
                )
                ,
                YBCI As (
                    Select YBCI.YItemMasterID As ItemMasterID, U.DisplayUnitDesc,  
                    ShadeCode= IsNull(YBCI.ShadeCode,''), 
                        YBCI.YD, YBM.BookingID,
                    ISV1.SegmentValue As _segment1ValueDesc, ISV2.SegmentValue As _segment2ValueDesc, ISV3.SegmentValue As _segment3ValueDesc,
                    ISV4.SegmentValue As _segment4ValueDesc, ISV5.SegmentValue As _segment5ValueDesc, ISV6.SegmentValue As _segment6ValueDesc,
                    ISV7.SegmentValue As _segment7ValueDesc, ISV8.SegmentValue As _segment8ValueDesc, YBM.SubGroupID, ISG.SubGroupName,
                    LiabilityQty = Sum(IsNull(FBAY.LiabilityQty,0)), TotalValue = CONVERT(DECIMAL(18,4),SUM(FBAY.LiabilityQty * FBAY.Rate)),Rate = ISNULL(FBAY.Rate,0)
                    From YBM 
                    Inner Join YarnBookingChild YBC On YBM.YBookingID = YBC.YBookingID
                    Inner Join YarnBookingChildItem YBCI On YBCI.YBChildID = YBC.YBChildID
                    Left Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = YBM.SubGroupID
                    INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YBCI.YItemMasterID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV8 ON ISV8.SegmentValueID = IM.Segment8ValueID
                    LEFT JOIN {DbNames.EPYSL}..Unit U ON U.UnitID = YBCI.UnitID
                    LEFT JOIN YarnShadeBook Y ON Y.ShadeCode = YBCI.ShadeCode
                    LEFT Join YDBookingMaster YDBM ON YDBM.YBookingID = YBM.YBookingID And YDBM.YBookingID = YBCI.YBookingID
                    LEFT Join YDProductionMaster YPM ON YPM.YDBookingMasterID = YDBM.YDBookingMasterID
	                Inner Join FBookingAcknowledgementYarnLiability FBAY On FBAY.BookingID = YBM.BookingID And FBAY.ItemMasterID = YBCI.YItemMasterID And FBAY.ConsumptionID = YBC.ConsumptionID
	                Group By YBCI.YItemMasterID, U.DisplayUnitDesc, YBCI.Remarks,
                    IsNull(YBCI.ShadeCode,''), YBCI.YD, YBM.BookingID,
                    ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue,
                    ISV4.SegmentValue, ISV5.SegmentValue, ISV6.SegmentValue,
                    ISV7.SegmentValue, ISV8.SegmentValue, YBM.SubGroupID, ISG.SubGroupName,ISNULL(FBAY.Rate,0)
	                Union
	                Select YBCI.YItemMasterID As ItemMasterID, U.DisplayUnitDesc,  
                    ShadeCode= IsNull(YBCI.ShadeCode,''), 
                    YBCI.YD, YBM.BookingID,
                    ISV1.SegmentValue As _segment1ValueDesc, ISV2.SegmentValue As _segment2ValueDesc, ISV3.SegmentValue As _segment3ValueDesc,
                    ISV4.SegmentValue As _segment4ValueDesc, ISV5.SegmentValue As _segment5ValueDesc, ISV6.SegmentValue As _segment6ValueDesc,
                    ISV7.SegmentValue As _segment7ValueDesc, ISV8.SegmentValue As _segment8ValueDesc, YBM.SubGroupID, ISG.SubGroupName,
                    LiabilityQty = Sum(IsNull(FBAY.LiabilityQty,0)), TotalValue = CONVERT(DECIMAL(18,4),SUM(FBAY.LiabilityQty * FBAY.Rate)), Rate = ISNULL(FBAY.Rate,0)
                    From YBM_New YBM
                    Inner Join YarnBookingChild_New YBC On YBM.YBookingID = YBC.YBookingID
                    Inner Join YarnBookingChildItem_New YBCI On YBCI.YBChildID = YBC.YBChildID
                    LEFT JOIN YarnItemPrice YIP ON YIP.YBChildItemID = YBCI.YBChildItemID AND ISNULL(YIP.IsTextileERP,0) = 1
                    Left Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = YBM.SubGroupID
                    INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YBCI.YItemMasterID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV8 ON ISV8.SegmentValueID = IM.Segment8ValueID
                    LEFT JOIN {DbNames.EPYSL}..Unit U ON U.UnitID = YBCI.UnitID
                    LEFT JOIN YarnShadeBook Y ON Y.ShadeCode = YBCI.ShadeCode
                    LEFT Join YDBookingMaster YDBM ON YDBM.YBookingID = YBM.YBookingID And YDBM.YBookingID = YBCI.YBookingID
                    LEFT Join YDProductionMaster YPM ON YPM.YDBookingMasterID = YDBM.YDBookingMasterID
	                Inner Join FBookingAcknowledgementYarnLiability FBAY On FBAY.BookingID = YBM.BookingID And FBAY.ItemMasterID = YBCI.YItemMasterID And FBAY.ConsumptionID = YBC.ConsumptionID
	                Group By YBCI.YItemMasterID, U.DisplayUnitDesc, YBCI.Remarks,
                    IsNull(YBCI.ShadeCode,''), YBCI.YD, YBM.BookingID,
                    ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue,
                    ISV4.SegmentValue, ISV5.SegmentValue, ISV6.SegmentValue,
                    ISV7.SegmentValue, ISV8.SegmentValue, YBM.SubGroupID, ISG.SubGroupName,ISNULL(FBAY.Rate,0)
                )
                Select *From YBCI";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                List<FBookingAcknowledgementYarnLiability> data = records.Read<FBookingAcknowledgementYarnLiability>().ToList();
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
        public async Task<List<FabricBookingAcknowledge>> GetAllBuyerTeamHeadByBOMMasterID(String BOMMasterID)
        {
            var query =
               $@"Select E.EmailID
                From {DbNames.EPYSL}..BOMMaster BOM
                Inner Join {DbNames.EPYSL}..StyleMaster SM On SM.StyleMasterID = BOM.StyleMasterID
                Inner Join {DbNames.EPYSL}..EmployeeTeamAssignContactTeam ETACT On ETACT.CategoryTeamID = SM.BuyerTeamID
                Inner Join {DbNames.EPYSL}..Employee E On E.EmployeeCode = ETACT.BuyerTeamHeadCode
                Where E.IsRegular = 1 and BOM.BOMMasterID = {BOMMasterID}
                Group By E.EmailID";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                List<FabricBookingAcknowledge> data = records.Read<FabricBookingAcknowledge>().ToList();
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

        public async Task<List<FabricBookingAcknowledge>> GetAllEmployeeMailSetupByUserCodeAndSetupForName(String UserCode, String SetupForName)
        {
            var query =
               $@"Select EMS.ToMailID,EMS.CCMailID,EMS.BCCMailID
				From {DbNames.EPYSL}..EmployeeMailSetup EMS
				Inner Join {DbNames.EPYSL}..Employee E On E.EmployeeCode = EMS.EmployeeCode
				Inner Join {DbNames.EPYSL}..LoginUser LU On LU.EmployeeCode = EMS.EmployeeCode
				Inner Join {DbNames.EPYSL}..MailSetupFor MSF on MSF.SetupForID = EMS.SetupForID
				Where LU.UserCode = {UserCode} And MSF.SetupForName in (Select _ID From dbo.fnReturnStringArray('{SetupForName}',','))";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                List<FabricBookingAcknowledge> data = records.Read<FabricBookingAcknowledge>().ToList();
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

        public async Task<List<FBookingAcknowledge>> GetRevMktAckAndRevisionAck(string BookingNo, string ExportOrderID)
        {
            string sql = $@"--- Revision Acknowledge List
                            ;With RunningEWO As(
	                            Select EOM.ExportOrderID, EOM.ExportOrderNo, EOM.StyleMasterID, EOM.BuyerID, EOM.BuyerTeamID
	                            From {DbNames.EPYSL}..ExportOrderMaster EOM
	                            Where EOM.EWOStatusID = 130 And EOM.ExportOrderID = '{ExportOrderID}'
                            ),BM As (
                                Select BM.BookingID, BM.BookingNo, BM.ExportOrderID, BM.SupplierID, BM.SubGroupID, WithOutOB = Convert(bit,0),
	                            EOM.ExportOrderNo, EOM.StyleMasterID, BM.RevisionNo, EOM.BuyerID, EOM.BuyerTeamID, BM.AddedBy 
	                            from {DbNames.EPYSL}..BookingMaster BM
	                            Inner Join RunningEWO EOM On EOM.ExportOrderID = BM.ExportOrderID
	                            LEFT JOIN {DbNames.EPYSL}..BookingChildImage BCI ON BCI.BookingID = BM.BookingID
	                            where BM.IsCancel = 0 And BM.SubGroupID in (1,11,12) And BM.BookingNo = '{BookingNo}'
	                            Union All
	                            Select BM.BookingID, BM.BookingNo, BM.ExportOrderID, SupplierID, 1 SubGroupID, WithOutOB = Convert(bit,1),
	                            EOM.ExportOrderNo, EOM.StyleMasterID, BM.RevisionNo, EOM.BuyerID, EOM.BuyerTeamID, BM.AddedBy
	                            from {DbNames.EPYSL}..SampleBookingMaster BM
	                            Inner Join RunningEWO EOM On EOM.ExportOrderID = BM.ExportOrderID
	                            LEFT JOIN {DbNames.EPYSL}..SampleBookingChildImage BCI ON BCI.BookingID = BM.BookingID
	                            where BM.IsCancel = 0 And BM.ExportOrderID <> 0 and BM.Proposed = 1 And IsCancel = 0 And BM.BookingNo = '{BookingNo}'
                            ),ISG As
                            (
	                            Select * from {DbNames.EPYSL}..ItemSubGroup
	                            Where SubGroupName in ('Fabric','Collar','Cuff')
                            )
                            ,BKList As
                            (
	                            Select BM.BookingNo, BM.ExportOrderID, BM.ExportOrderNo, BookingID = Min(BM.BookingID),0 BOMMasterID, ISourcing = CAI.InHouse, BM.BuyerTeamID, ContactID = BM.SupplierID, BM.WithOutOB, RevisionNo = Max(BM.RevisionNo)
	                            ,BM.BuyerID, BM.AddedBy
	                            FROM BM
	                            Inner Join ISG On ISG.SubGroupID = BM.SubGroupID
	                            Left Join {DbNames.EPYSL}..ContactAdditionalInfo CAI On CAI.ContactID = BM.SupplierID
	                            Where ISNULL(CAI.InHouse,0) = 1
	                            Group By BM.BookingNo, BM.ExportOrderID, BM.ExportOrderNo, CAI.InHouse, BM.BuyerTeamID, BM.SupplierID, BM.WithOutOB ,BM.BuyerID, BM.AddedBy
                            ),BAck as (
	                            Select BC.BookingID, BC.BookingNo, BC.ExportOrderID, BC.ExportOrderNo, BC.BOMMasterID,BC.ContactID, BC.BuyerTeamID, BC.ISourcing, BIA.RevisionNo, BIA.SubGroupID, BIA.ItemGroupID, BIA.AcknowledgeDate, BIA.WithoutOB
	                            ,BC.BuyerID, BC.AddedBy
	                            FROM BKList BC
                                Inner Join {DbNames.EPYSL}..BookingItemAcknowledge BIA On BIA.BookingID = BC.BookingID And BIA.WithoutOB = BC.WithOutOB
	                            Group By BC.BookingID, BC.BookingNo, BC.ExportOrderID, BC.ExportOrderNo, BC.BOMMasterID,BC.ContactID, BC.BuyerTeamID, BC.ISourcing, BIA.RevisionNo, BIA.SubGroupID, BIA.ItemGroupID, BIA.AcknowledgeDate, BIA.WithoutOB
	                            ,BC.BuyerID, BC.AddedBy
                            ),InHouseItemList as (
	                            SELECT BC.BOMMasterID,BC.ExportOrderID, BC.ExportOrderNo, BC.BookingID, BC.BookingNo, BC.BuyerTeamID, BC.SubGroupID, BC.ItemGroupID,BC.ContactID, ISourcing = Max(convert(int,BC.ISourcing)), BC.RevisionNo, AcknowledgeDate = Null, BC.WithOutOB,BKAcknowledgeDate = Max(BC.AcknowledgeDate)
	                            ,BC.BuyerID, BC.AddedBy
	                            FROM BAck BC
	                            Group By BC.BOMMasterID,BC.ExportOrderID, BC.ExportOrderNo, BC.BookingID, BC.BookingNo, BC.BuyerTeamID, BC.SubGroupID, BC.ItemGroupID,BC.ContactID, BC.ISourcing, BC.WithOutOB, BC.RevisionNo
	                            ,BC.BuyerID, BC.AddedBy
	                            having BC.ISourcing = 1
                            ),RevisionAckList As(
	                            Select OSI.ExportOrderID,OSI.ExportOrderNo,OSI.BookingNo
	                            From EPYSLTEX..FabricBookingAcknowledge FBA 
                                Inner Join  InHouseItemList OSI1 On FBA.BookingID = OSI1.BookingID  And FBA.SubGroupID = OSI1.SubGroupID And FBA.ItemGroupID = OSI1.ItemGroupID
	                            Left Join BM OSI On FBA.BookingID = OSI.BookingID  And FBA.SubGroupID = OSI.SubGroupID 
	                             Where Case When FBA.AcknowledgeID IS NULL Then 0
					                                                When FBA.AcknowledgeID IS NOT NULL And ISNULL(FBA.PreProcessRevNo,0) < ISNULL(OSI1.RevisionNo,0) Then 2 Else 1 End 
			                                                = Case When 'R'='N' Then 0
					                                                When 'R'='R' Then 2 Else 1 End
	                            Group By OSI.ExportOrderID,OSI.ExportOrderNo,OSI.BookingNo
                            ), RevMktAck As(
	                            Select OSI.ExportOrderID,OSI.ExportOrderNo,OSI.BookingNo
	                            From EPYSLTEX..FabricBookingAcknowledge FBA
	                            Left Join BM OSI On FBA.BookingID = OSI.BookingID  And FBA.SubGroupID = OSI.SubGroupID 
	                            LEFT JOIN FBookingAcknowledge FBA1 ON FBA1.BookingID = FBA.BookingID AND FBA1.SubGroupID = FBA.SubGroupID
	                            INNER JOIN FBookingAcknowledgeChild FBC ON FBC.AcknowledgeID = FBA1.FBAckID
	                            Where FBA.AcknowledgeDate >= '{_startingDate}'
	                            AND FBA.RevisionNo = FBA1.PreRevisionNo AND IsNull(FBC.SendToMktAck,0) = 1 AND ISNULL(FBC.IsMktAck,0) = 0
	                            Group By OSI.ExportOrderID,OSI.ExportOrderNo,OSI.BookingNo
                            )

                            Select 
                            EOM.ExportOrderID, 
                            IsRevMktAck = IIF(ISNULL(RMA.BookingNo,'') = '', 'N' , 'Y'),
                            IsRevisionAck = IIF(ISNULL(RA.BookingNo,'') = '' , 'N' , 'Y')
                            From {DbNames.EPYSL}..ExportOrderMaster EOM
                            Left Join RevMktAck RMA On RMA.ExportOrderID = EOM.ExportOrderID
                            Left Join RevisionAckList RA On RA.ExportOrderID = EOM.ExportOrderID
                            Where EOM.ExportOrderID = '{ExportOrderID}' ";

            try
            {
                await _service.Connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                List<FBookingAcknowledge> obj = records.Read<FBookingAcknowledge>().ToList();
                return obj;
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
        public async Task UnAckFabricBooking(String Sql)
        {
            try
            {
                if (Sql.IsNotNullOrEmpty())
                {
                    await _service.ExecuteAsync(Sql, AppConstants.TEXTILE_CONNECTION);
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        private decimal GetFinishedQtyLiability(decimal stockQty, decimal deliveredQty, decimal currentBookingQty, decimal previousBookingQty)
        {
            decimal finishedQty = 0;

            if (currentBookingQty > previousBookingQty) return 0;
            if (stockQty + deliveredQty > previousBookingQty) return previousBookingQty - currentBookingQty;
            if (stockQty + deliveredQty > currentBookingQty) return (stockQty + deliveredQty) - currentBookingQty;

            return finishedQty;
        }
        public async Task<List<FabricWastageGrid>> GetFabricWastageGridAsync(string wastageFor)
        {
            var sql = $@"Select FWG.FWGID, FWG.WastageFor, FWG.IsFabric, FWG.GSMFrom, FWG.GSMTo, FWG.BookingQtyFrom, FWG.BookingQtyTo, FWG.FixedQty, FWG.ExcessQty, FWG.ExcessPercentage
                        From FabricWastageGrid FWG WHERE wastageFor='{wastageFor}'";

            return await _service.GetDataAsync<FabricWastageGrid>(sql);
        }
    }
}
