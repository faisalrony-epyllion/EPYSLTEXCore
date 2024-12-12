using Dapper;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Application.Interfaces.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Booking;
using EPYSLTEXCore.Infrastructure.Entities.Tex.General.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Yarn;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Transactions;

namespace EPYSLTEXCore.Application.Services.Inventory
{
    internal class YarnAllocationService : IYarnAllocationService
    {
        private readonly IDapperCRUDService<YarnAllocationMaster> _service;
        private readonly SqlConnection _connection;
        private readonly SqlConnection _connectionGmt;

        SqlCommand _sqlCom = null;
        YarnAllocationMaster _yarnAllocation = new YarnAllocationMaster();
        public YarnAllocationService(IDapperCRUDService<YarnAllocationMaster> service)
        {
            _service = service;

            _service.Connection = service.GetConnection(AppConstants.GMT_CONNECTION);
            _connectionGmt = service.Connection;

            _service.Connection = service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _connection = service.Connection;
        }
        public async Task<List<YarnAllocationMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo)
        {
            string tempGuid = CommonFunction.GetNewGuid();
            bool hasTempTable = false;

            bool isNeedImage = false;
            string orderBy = "";
            if (status == Status.Pending)
            {
                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? " Order By AcknowledgeDate Desc" : paginationInfo.OrderBy;
            }
            else
            {
                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? " Order By YarnAllocationID Desc" : paginationInfo.OrderBy;
            }
            string sql = "";

            switch (status)
            {
                case Status.Revise:
                    sql = $@"
                            With CutoffDate As
						(
						SELECT ETV.ValueID, ETV.ValueName
						FROM {DbNames.EPYSL}..EntityType ET
						INNER JOIN {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.EntityTypeID = ET.EntityTypeID
						WHERE ET.EntityTypeName = 'YarnAllocationCutOffDate'
						),
                        AQ AS
                            (
                                Select AllocationID, BookingNo = STRING_AGG(FBA.BookingNo,','),TotalAllocatedQty = SUM(YAC.TotalAllocationQty) 
                                FROM {TableNames.YARN_ALLOCATION_MASTER} YAM 
								INNER JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.AllocationID = YAM.YarnAllocationID
								INNER JOIN {TableNames.YARN_ALLOCATION_CHILD_ITEM} YACI ON YACI.AllocationChildID = YAC.AllocationChildID
								LEFT JOIN {TableNames.YARN_ALLOCATION_CHILD_PIPELINE_ITEM} YACPI ON YACPI.AllocationChildID = YAC.AllocationChildID
	                            INNER JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBCI.YBChildItemID = YAC.YBChildItemID
                                LEFT JOIN {TableNames.YarnBookingChildItem_New_Bk} YBCIB ON YBCIB.YBChildItemID = YAC.YBChildItemID
	                            INNER JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID = YBCI.YBChildID
	                            INNER JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID = YBC.YBookingID
	                            INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.BookingID = YBM.BookingID AND FBA.BuyerId IN (729,4) --Saif

	                            WHERE  ISNULL(YBM.PreProcessRevNo,0) = ISNULL(FBA.RevisionNo,0) AND
                                (ISNULL(YBM.RevisionNo,0) <> ISNULL(YACI.PreProcessRevNo,0) OR ISNULL(YACI.IsRevised,0) = 1 ) AND
                                (ISNULL(YBCI.NetYarnReqQty,0) <> ISNULL(YBCIB.NetYarnReqQty,0) OR YBCI.YItemMasterID<>YBCIB.YItemMasterID)
                                AND YBM.YBookingDate >=(Select ValueName From CutoffDate)
                                GROUP BY YAC.AllocationID
                            ),
                            FinalList AS
                            (
                                Select YarnAllocationID,YarnAllocationDate,YarnAllocationNo,AQ.TotalAllocatedQty,E.EmployeeName AllocatedBy, AQ.BookingNo
                                From {TableNames.YARN_ALLOCATION_MASTER} YAM
                                INNER JOIN AQ ON AQ.AllocationID=YAM.YarnAllocationID
                                Inner Join {DbNames.EPYSL}..LoginUser LU On YAM.AddedBy = LU.UserCode
                                Inner Join {DbNames.EPYSL}..Employee E On E.EmployeeCode = LU.EmployeeCode
                            )
                            SELECT *, Count(*) Over() TotalRows FROM FinalList ";
                    break;
                case Status.ProposedForApproval:
                    sql = $@"
                    WITH CutoffDate As
						(
						SELECT ETV.ValueID, ETV.ValueName
						FROM {DbNames.EPYSL}..EntityType ET
						INNER JOIN {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.EntityTypeID = ET.EntityTypeID
						WHERE ET.EntityTypeName = 'YarnAllocationCutOffDate'
						),
                        AQ AS(
                        Select AllocationID, BookingNo = STRING_AGG(FBA.BookingNo,','),TotalAllocatedQty = SUM(YAC.TotalAllocationQty),
                        IsInValidOperation = CASE WHEN ISNULL(YBM.RevisionNo,0) = ISNULL(YACI.PreProcessRevNo,0) THEN 0 ELSE 1 END
						FROM {TableNames.YARN_ALLOCATION_MASTER} YAM
						INNER JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.AllocationID = YAM.YarnAllocationID
                        LEFT JOIN {TableNames.YARN_ALLOCATION_CHILD_ITEM} YACI ON YACI.AllocationChildID = YAC.AllocationChildID
                        --FROM {TableNames.YARN_ALLOCATION_CHILD_ITEM} YACI
						--INNER JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.AllocationChildID = YACI.AllocationChildID
                        --INNER JOIN {TableNames.YARN_ALLOCATION_MASTER} YAM ON YAM.YarnAllocationID = YAC.AllocationID
	                    INNER JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBCI.YBChildItemID = YAC.YBChildItemID
	                    INNER JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID = YBCI.YBChildID
	                    INNER JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID = YBC.YBookingID
	                    INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.BookingID = YBM.BookingID AND FBA.BuyerId IN (729,4) --Saif
                        --WHERE ISNULL(YBM.RevisionNo,0) = ISNULL(YAC.PreRevisionNo,0)
                        WHERE YBM.YBookingDate >=(Select ValueName From CutoffDate)
                        GROUP BY YAC.AllocationID, ISNULL(YBM.RevisionNo,0),ISNULL(YACI.PreProcessRevNo,0)
                    ),
                    FinalList AS
                    (
                        Select YarnAllocationID,YarnAllocationDate,YarnAllocationNo,
	                    AQ.TotalAllocatedQty,AllocatedBy = E.EmployeeName, AQ.BookingNo, AQ.IsInValidOperation
                        FROM {TableNames.YARN_ALLOCATION_MASTER} YAM
                        INNER JOIN AQ ON AQ.AllocationID=YAM.YarnAllocationID
                        Inner Join {DbNames.EPYSL}..LoginUser LU On YAM.AddedBy = LU.UserCode
                        Inner Join {DbNames.EPYSL}..Employee E On E.EmployeeCode = LU.EmployeeCode
                        WHERE YAM.Propose = 1 AND YAM.Approve = 0 AND YAM.Reject = 0 AND YAM.UnAcknowledge = 0
                    )
                    SELECT *, Count(*) Over() TotalRows FROM FinalList ";
                    break;
                case Status.Approved:
                    sql = $@"
                             With CutoffDate As
						(
						SELECT ETV.ValueID, ETV.ValueName
						FROM {DbNames.EPYSL}..EntityType ET
						INNER JOIN {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.EntityTypeID = ET.EntityTypeID
						WHERE ET.EntityTypeName = 'YarnAllocationCutOffDate'
						),
                        AQ AS
                            (
                                Select AllocationID, BookingNo = STRING_AGG(FBA.BookingNo,','),TotalAllocatedQty = SUM(YAC.TotalAllocationQty),
                                IsInValidOperation = CASE WHEN ISNULL(YBM.RevisionNo,0) = ISNULL(YACI.PreProcessRevNo,0) THEN 0 ELSE 1 END
                                FROM {TableNames.YARN_ALLOCATION_CHILD_ITEM} YACI
								INNER JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.AllocationChildID = YACI.AllocationChildID
                                --IsInValidOperation = CASE WHEN ISNULL(YBM.RevisionNo,0) = ISNULL(YAC.PreRevisionNo,0) THEN 0 ELSE 1 END
                                --FROM {TableNames.YARN_ALLOCATION_CHILD} YAC
                                INNER JOIN {TableNames.YARN_ALLOCATION_MASTER} YAM ON YAM.YarnAllocationID = YAC.AllocationID
	                            INNER JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBCI.YBChildItemID = YAC.YBChildItemID
	                            INNER JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID = YBCI.YBChildID
	                            INNER JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID = YBC.YBookingID
	                            INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.BookingID = YBM.BookingID AND FBA.BuyerId IN (729,4) --Saif
                                --WHERE ISNULL(YBM.RevisionNo,0) = ISNULL(YAC.PreRevisionNo,0)
                                WHERE YBM.YBookingDate >=(Select ValueName From CutoffDate)
                                GROUP BY YAC.AllocationID, ISNULL(YBM.RevisionNo,0), ISNULL(YAC.PreRevisionNo,0),ISNULL(YACI.PreProcessRevNo,0)
                            ),
                            FinalList AS
                            (
                                Select YarnAllocationID,YarnAllocationDate,YarnAllocationNo,AQ.TotalAllocatedQty,E.EmployeeName AllocatedBy, AQ.BookingNo, AQ.IsInValidOperation
                                FROM {TableNames.YARN_ALLOCATION_MASTER} YAM
                                INNER JOIN AQ ON AQ.AllocationID=YAM.YarnAllocationID
                                Inner Join {DbNames.EPYSL}..LoginUser LU On YAM.AddedBy = LU.UserCode
                                Inner Join {DbNames.EPYSL}..Employee E On E.EmployeeCode = LU.EmployeeCode
                                WHERE YAM.Approve=1 AND YAM.Acknowledge = 0 AND YAM.UnAcknowledge = 0
                            )
                            SELECT *, Count(*) Over() TotalRows FROM FinalList  ";
                    break;
                case Status.Reject:
                    sql = $@"
                        With CutoffDate As
						(
						    SELECT ETV.ValueID, ETV.ValueName
						    FROM {DbNames.EPYSL}..EntityType ET
						    INNER JOIN {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.EntityTypeID = ET.EntityTypeID
						    WHERE ET.EntityTypeName = 'YarnAllocationCutOffDate'
						),
                        AQ AS(
                                Select AllocationID, BookingNo = STRING_AGG(FBA.BookingNo,','),TotalAllocatedQty = SUM(YAC.TotalAllocationQty),
                                IsInValidOperation = CASE WHEN ISNULL(YBM.RevisionNo,0) = ISNULL(YACI.PreProcessRevNo,0) THEN 0 ELSE 1 END
                                FROM {TableNames.YARN_ALLOCATION_CHILD_ITEM} YACI
								INNER JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.AllocationChildID = YACI.AllocationChildID
                                --IsInValidOperation = CASE WHEN ISNULL(YBM.RevisionNo,0) = ISNULL(YAC.PreRevisionNo,0) THEN 0 ELSE 1 END
                                --FROM {TableNames.YARN_ALLOCATION_CHILD} YAC
                                INNER JOIN {TableNames.YARN_ALLOCATION_MASTER} YAM ON YAM.YarnAllocationID = YAC.AllocationID
	                            INNER JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBCI.YBChildItemID = YAC.YBChildItemID
	                            INNER JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID = YBCI.YBChildID
	                            INNER JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID = YBC.YBookingID
	                            INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.BookingID = YBM.BookingID AND FBA.BuyerId IN (729,4) --Saif
                                --WHERE ISNULL(YBM.RevisionNo,0) = ISNULL(YAC.PreRevisionNo,0)
                                WHERE YBM.YBookingDate >=(Select ValueName From CutoffDate)
                                GROUP BY YAC.AllocationID, ISNULL(YBM.RevisionNo,0), ISNULL(YACI.PreProcessRevNo,0)
                            ),
                            FinalList AS
                            (
                                Select YarnAllocationID,YarnAllocationDate,YarnAllocationNo,AQ.TotalAllocatedQty,E.EmployeeName AllocatedBy, AQ.BookingNo, AQ.IsInValidOperation
                                FROM {TableNames.YARN_ALLOCATION_MASTER} YAM
                                INNER JOIN AQ ON AQ.AllocationID=YAM.YarnAllocationID
                                Inner Join {DbNames.EPYSL}..LoginUser LU On YAM.AddedBy = LU.UserCode
                                Inner Join {DbNames.EPYSL}..Employee E On E.EmployeeCode = LU.EmployeeCode
                                WHERE YAM.Reject = 1 AND YAM.UnAcknowledge = 0
                            )
                            SELECT *, Count(*) Over() TotalRows FROM FinalList ";
                    break;
                case Status.Acknowledge:
                    sql = $@"
                            With AQ AS(
                                Select AllocationID, BookingNo = STRING_AGG(FBA.BookingNo,','),TotalAllocatedQty = SUM(YAC.TotalAllocationQty),
                                IsInValidOperation = CASE WHEN ISNULL(YBM.RevisionNo,0) = ISNULL(YACI.PreProcessRevNo,0) THEN 0 ELSE 1 END
                                FROM {TableNames.YARN_ALLOCATION_CHILD_ITEM} YACI
								INNER JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.AllocationChildID = YACI.AllocationChildID
                                --IsInValidOperation = CASE WHEN ISNULL(YBM.RevisionNo,0) = ISNULL(YAC.PreRevisionNo,0) THEN 0 ELSE 1 END
                                --FROM {TableNames.YARN_ALLOCATION_CHILD} YAC
                                INNER JOIN {TableNames.YARN_ALLOCATION_MASTER} YAM ON YAM.YarnAllocationID = YAC.AllocationID
	                            INNER JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBCI.YBChildItemID = YAC.YBChildItemID
	                            INNER JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID = YBCI.YBChildID
	                            INNER JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID = YBC.YBookingID
	                            INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.BookingID = YBM.BookingID AND FBA.BuyerId IN (729,4) --Saif
                                --WHERE ISNULL(YBM.RevisionNo,0) = ISNULL(YAC.PreRevisionNo,0)
                                GROUP BY YAC.AllocationID, ISNULL(YBM.RevisionNo,0), ISNULL(YAC.PreRevisionNo,0)
                            ),
                            FinalList AS
                            (
                                Select YarnAllocationID,YarnAllocationDate,YarnAllocationNo,AQ.TotalAllocatedQty,E.EmployeeName AllocatedBy, AQ.BookingNo, AQ.IsInValidOperation
                                FROM {TableNames.YARN_ALLOCATION_MASTER} YAM
                                INNER JOIN AQ ON AQ.AllocationID=YAM.YarnAllocationID
                                Inner Join {DbNames.EPYSL}..LoginUser LU On YAM.AddedBy = LU.UserCode
                                Inner Join {DbNames.EPYSL}..Employee E On E.EmployeeCode = LU.EmployeeCode
                                WHERE YAM.Acknowledge = 1 AND YAM.UnAcknowledge = 0
                            )
                            SELECT *, Count(*) Over() TotalRows FROM FinalList ";
                    break;
                case Status.UnAcknowledge:
                    sql = $@"
                            With AQ AS(
                                Select AllocationID, BookingNo = STRING_AGG(FBA.BookingNo,','),TotalAllocatedQty = SUM(YAC.TotalAllocationQty),
                                IsInValidOperation = CASE WHEN ISNULL(YBM.RevisionNo,0) = ISNULL(YACI.PreProcessRevNo,0) THEN 0 ELSE 1 END
                                FROM {TableNames.YARN_ALLOCATION_CHILD_ITEM} YACI
								INNER JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.AllocationChildID = YACI.AllocationChildID
                                --IsInValidOperation = CASE WHEN ISNULL(YBM.RevisionNo,0) = ISNULL(YAC.PreRevisionNo,0) THEN 0 ELSE 1 END
                                --FROM {TableNames.YARN_ALLOCATION_CHILD} YAC
                                INNER JOIN {TableNames.YARN_ALLOCATION_MASTER} YAM ON YAM.YarnAllocationID = YAC.AllocationID
	                            INNER JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBCI.YBChildItemID = YAC.YBChildItemID
	                            INNER JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID = YBCI.YBChildID
	                            INNER JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID = YBC.YBookingID
	                            INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.BookingID = YBM.BookingID AND FBA.BuyerId IN (729,4) --Saif
                                --WHERE ISNULL(YBM.RevisionNo,0) = ISNULL(YAC.PreRevisionNo,0)
                                GROUP BY YAC.AllocationID, ISNULL(YBM.RevisionNo,0), ISNULL(YAC.PreRevisionNo,0)
                            ),
                            FinalList AS
                            (
                                Select YarnAllocationID,YarnAllocationDate,YarnAllocationNo,AQ.TotalAllocatedQty,E.EmployeeName AllocatedBy, AQ.BookingNo, AQ.IsInValidOperation
                                FROM {TableNames.YARN_ALLOCATION_MASTER} YAM
                                INNER JOIN AQ ON AQ.AllocationID=YAM.YarnAllocationID
                                Inner Join {DbNames.EPYSL}..LoginUser LU On YAM.AddedBy = LU.UserCode
                                Inner Join {DbNames.EPYSL}..Employee E On E.EmployeeCode = LU.EmployeeCode
                                WHERE YAM.UnAcknowledge = 1
                            )
                            SELECT *, Count(*) Over() TotalRows FROM FinalList ";
                    break;
                case Status.All:
                    isNeedImage = true;
                    sql = $@"
                            With CutoffDate As
						(
						SELECT ETV.ValueID, ETV.ValueName
						FROM {DbNames.EPYSL}..EntityType ET
						INNER JOIN {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.EntityTypeID = ET.EntityTypeID
						WHERE ET.EntityTypeName = 'YarnAllocationCutOffDate'
						),
                        AQ AS(
                                Select AllocationID, BookingNo = STRING_AGG(FBA.BookingNo,','),TotalAllocatedQty = SUM(TotalAllocationQty) 
                                FROM {TableNames.YARN_ALLOCATION_CHILD} YAC
                                INNER JOIN {TableNames.YARN_ALLOCATION_MASTER} YAM ON YAM.YarnAllocationID = YAC.AllocationID
	                            INNER JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBCI.YBChildItemID = YAC.YBChildItemID
	                            INNER JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID = YBCI.YBChildID
	                            INNER JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID = YBC.YBookingID
	                            INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.BookingID = YBM.BookingID AND FBA.BuyerId IN (729,4) --Saif
                                WHERE YBM.YBookingDate >=(Select ValueName From CutoffDate)
                                GROUP BY YAC.AllocationID
                            ),
                            FinalList AS
                            (
                                Select YarnAllocationID,YarnAllocationDate,YarnAllocationNo,AQ.TotalAllocatedQty,E.EmployeeName AllocatedBy, AQ.BookingNo
                                FROM {TableNames.YARN_ALLOCATION_MASTER} YAM
                                INNER JOIN AQ ON AQ.AllocationID=YAM.YarnAllocationID
                                Inner Join {DbNames.EPYSL}..LoginUser LU On YAM.AddedBy = LU.UserCode
                                Inner Join {DbNames.EPYSL}..Employee E On E.EmployeeCode = LU.EmployeeCode
                            )
                            SELECT *, Count(*) Over() TotalRows FROM FinalList ";
                    break;
                default: //Status.Pending:
                    sql = $@"
                        WITH CutoffDate As
                        (
	                        SELECT ETV.ValueID, ETV.ValueName
	                        FROM {DbNames.EPYSL}..EntityType ET
	                        INNER JOIN {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.EntityTypeID = ET.EntityTypeID
	                        WHERE ET.EntityTypeName = 'YarnAllocationCutOffDate'
                        ),
                        CQ AS(
	                        Select YAC.YBChildItemID
	                        ,TotalAllocatedQty = SUM(TotalAllocationQty) 
                            FROM {TableNames.YARN_ALLOCATION_CHILD} YAC
	                        INNER JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBCI.YBChildItemID = YAC.YBChildItemID
	                        INNER JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID = YBCI.YBChildID
	                        INNER JOIN {TableNames.YarnBookingMaster_New} YB ON YB.YBookingID = YBC.YBookingID
	                        INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FBC ON FBC.BookingChildID = YBC.BookingChildID AND FBC.BookingID = YBC.BookingID
	                        WHERE YB.Acknowledge = 1
                            AND YB.DateAdded >= (Select CAST(ValueName AS DATE) From CutoffDate)
	                        GROUP BY YAC.YBChildItemID
                        )
                        ,FBA AS (
	                        SELECT YBCI.YBChildItemID
	                        ,YBC.YBChildID
	                        ,YB.YBookingNo
	                        ,BuyerName = CTO.ShortName
	                        ,BuyerTeamName = CCT.TeamName
	                        ,CS.SeasonName
	                        ,YB.YRequiredDate
	                        ,InHouseDate = CASE WHEN a.IsSample = 1 THEN SBM.InHouseDate ELSE BM.InHouseDate END
	                        ,YBCI.YarnPly
	                        ,NumericCount = SV6.SegmentValue
	                        ,YBCI.YarnCategory
	                        ,YBCI.YarnLotNo
	                        ,SpinnerName = CASE WHEN YBCI.SpinnerID > 0 THEN CCS.ShortName ELSE '' END
	                        ,YarnReqQty = YBCI.NetYarnReqQty
	                        ,TotalAllocatedQty = ISNULL(CQ.TotalAllocatedQty,0)
	                        --,YarnStockQty = Stock.StockQty
	                        ,YarnCertification = ''
	                        ,BookingStatus = ''
	                        ,FabricType = ISV1.SegmentValue
	                        ,FabricQty = a.BookingQty
	                        ,ActualYarnBookingDate = MAX(a.BookingDate)
	                        ,YarnInhouseEndDate = NUll
	                        ,KnittingStartDate4P = NUll
	                        ,KnittingEndDate4P = NUll /*blank*/ 
	                        ,TNACalendarDays = EOM.CalendarDays,YB.RevisionReason
	                        ,FabricShade = CASE WHEN YB.SubGroupID = 1 THEN SV3.SegmentValue ELSE SV5.SegmentValue END 
	                        ,FabricGSM = CASE WHEN YB.SubGroupID = 1 THEN SV4.SegmentValue ELSE '' END
	                        ,YarnType = ISV3.SegmentValue -- Certificate
	                        ,YarnRequisitionType = case when YB.IsAddition=1 then 'Add-'+Convert(varchar(100),YB.AdditionNo) when YB.RevisionNo>0 then 'Rev-'+Convert(varchar(100),YB.RevisionNo) else 'Main' end
	                        ,RequiredYarnQuantityKG = ISNULL(YBCI.NetYarnReqQty,0)
	                        ,AllocationBalanceQTYKG =  ISNULL(YBCI.NetYarnReqQty,0) - ISNULL(CQ.TotalAllocatedQty,0)
	                        ,YDST = Case When YBCI.YD = 1 Then 'YES' Else 'NO' END /*YarnDyeingOrder */
	                        ,AcknowledgeDate = MAX(YB.AcknowledgeDate)
                            ,IsRevisionPending = CASE WHEN ISNULL(YB.PreProcessRevNo,0) <> ISNULL(a.RevisionNo,0) THEN 1 ELSE 0 END
	                        ,a.ExportOrderID
	                        ,YBCI.YItemMasterID
                            FROM {TableNames.YarnBookingChildItem_New} YBCI
	                        INNER JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID = YBCI.YBChildID
	                        INNER JOIN {TableNames.YarnBookingMaster_New} YB ON YB.YBookingID = YBC.YBookingID
	                        INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FBC ON FBC.BookingChildID = YBC.BookingChildID AND FBC.BookingID = YBC.BookingID AND FBC.BookingQty > 0
	                        LEFT JOIN CQ ON YBCI.YBChildItemID = CQ.YBChildItemID
	                        INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} a ON a.FBAckID = FBC.AcknowledgeID AND a.BuyerId IN (729,4) --Saif
	                        INNER JOIN {TableNames.FabricBookingAcknowledge} FBA ON FBA.BookingID = a.BookingID
	                        LEFT JOIN {DbNames.EPYSL}..BookingMaster BM ON BM.BookingID = FBA.BookingID
                            LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster SBM ON SBM.BookingID = FBA.BookingID
	                        LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} b ON b.BookingID = a.BookingID
	                        LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = a.BuyerID
	                        LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = a.BuyerTeamID
	                        LEFT JOIN {DbNames.EPYSL}..CompanyEntity C ON C.CompanyID = b.CompanyID
	                        LEFT JOIN {DbNames.EPYSL}..Contacts CCS ON CCS.ContactID = YBCI.SpinnerID
	                        LEFT JOIN {DbNames.EPYSL}..ExportOrderMaster EOM ON EOM.ExportOrderID = a.ExportOrderID
	                        Left Join {DbNames.EPYSL}..ItemMaster IM On IM.ItemMasterID = YBC.ItemMasterID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
	                        Left Join {DbNames.EPYSL}..ItemSegmentValue SV3 On SV3.SegmentValueID = IM.Segment3ValueID
	                        Left Join {DbNames.EPYSL}..ItemSegmentValue SV4 On SV4.SegmentValueID = IM.Segment4ValueID
	                        Left Join {DbNames.EPYSL}..ItemSegmentValue SV5 On SV5.SegmentValueID = IM.Segment5ValueID

                            Left Join {DbNames.EPYSL}..ItemMaster IMYarnCount On IMYarnCount.ItemMasterID = YBCI.YItemMasterID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IMYarnCount.Segment3ValueID
                            Left Join {DbNames.EPYSL}..ItemSegmentValue SV6 On SV6.SegmentValueID = IMYarnCount.Segment6ValueID

                            LEFT JOIN {DbNames.EPYSL}..StyleMaster SM ON SM.StyleMasterID = EOM.StyleMasterID
                            LEFT JOIN {DbNames.EPYSL}..ContactSeason CS ON CS.SeasonID = SM.SeasonID

	                        WHERE
                            YB.Acknowledge = 1
	                        AND ISNULL(CQ.TotalAllocatedQty,0) < ISNULL(YBCI.NetYarnReqQty,0)
                            AND YB.DateAdded >= (Select CAST(ValueName AS DATE) From CutoffDate)

                            GROUP BY YBCI.YBChildItemID, YBC.YBChildID, YB.YBookingNo, CTO.ShortName, CCT.TeamName, CS.SeasonName, YB.YRequiredDate,
	                        a.IsSample, SBM.InHouseDate, BM.InHouseDate, YBCI.YarnPly, YBCI.YarnCategory, YBCI.YarnLotNo,
	                        YBCI.SpinnerID, CCS.ShortName, YBCI.NetYarnReqQty
	                        ,ISNULL(CQ.TotalAllocatedQty,0)
	                        ,YBCI.YD, a.BookingQty,
	                        CASE WHEN a.IsSample = 1 THEN SBM.BookingDate ELSE BM.BookingDate END,
							YB.YBookingDate, EOM.CalendarDays,YB.RevisionReason,
							CASE WHEN YB.SubGroupID = 1 THEN SV3.SegmentValue ELSE SV5.SegmentValue END, 
							CASE WHEN YB.SubGroupID = 1 THEN SV4.SegmentValue ELSE '' END,
							case when YB.IsAddition=1 then 'Add-'+Convert(varchar(100),YB.AdditionNo) when YB.RevisionNo>0 then 'Rev-'+Convert(varchar(100),YB.RevisionNo) else 'Main' end,
							YBCI.NetYarnReqQty,SV6.SegmentValue,ISV1.SegmentValue,
							ISV3.SegmentValue,CASE WHEN ISNULL(YB.PreProcessRevNo,0) <> ISNULL(a.RevisionNo,0) THEN 1 ELSE 0 END
	                        ,YBCI.NetYarnReqQty
	                        ,SV6.SegmentValue
	                        ,ISV1.SegmentValue,
	                        ISV3.SegmentValue
	                        ,CASE WHEN ISNULL(YB.PreProcessRevNo,0) <> ISNULL(a.RevisionNo,0) THEN 1 ELSE 0 END
	                        ,a.ExportOrderID
	                        ,YBCI.YItemMasterID
                        )
                        SELECT * INTO #TempTable{tempGuid} FROM FBA 
	                    SELECT *,Count(*) Over() TotalRows FROM #TempTable{tempGuid}";

                    hasTempTable = true;

                    break;
            }
            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            if (hasTempTable)
            {
                sql += $@" DROP TABLE #TempTable{tempGuid}";
            }

            var finalList = await _service.GetDataAsync<YarnAllocationMaster>(sql);


            var exportOrderIds = string.Join(",", finalList.Select(x => x.ExportOrderID).Distinct());
            var eventList = await this.GetEvents(exportOrderIds);

            var ybChildItemIDs = string.Join(",", finalList.Select(x => x.YBChildItemID).Distinct());
            var stockInfoList = await this.GetStockInfo(ybChildItemIDs);

            finalList.ForEach(x =>
            {
                var bookingNos = CommonFunction.GetDefaultValueWhenInvalidS(x.BookingNo).Split(',');
                x.BookingNo = string.Join(",", bookingNos.Distinct());

                var events = eventList.Where(y => y.ExportOrderID == x.ExportOrderID).ToList();
                events.ForEach(e =>
                {
                    if (e.EventDescriptionId == 1) x.YarnBookingDate = e.EventDate;
                    else if (e.EventDescriptionId == 2) x.FabricsDeliveryStartDate = e.EventDate;
                    else if (e.EventDescriptionId == 3) x.FabricBookingDate = e.EventDate;
                    else if (e.EventDescriptionId == 4) x.YarnInhouseStartDate = e.EventDate;
                    else if (e.EventDescriptionId == 5) x.FabricsDeliveryEndDate = e.EventDate;
                });

                var stockInfos = stockInfoList.Where(y => y.YBChildItemID == x.YBChildItemID).ToList();
                stockInfos.ForEach(s =>
                {
                    x.YarnStockQty += s.StockQty;
                });
            });

            if (isNeedImage)
            {
                string bookingNos = string.Join("','", finalList.Select(x => x.BookingNo).Distinct());
                if (bookingNos.IsNotNullOrEmpty())
                {
                    var fBookingAcknowledgeImages = await _service.GetDataAsync<YarnAllocationChildItem>(CommonQueries.GetImagePathQuery(bookingNos));
                    fBookingAcknowledgeImages.ForEach(x =>
                    {
                        var obj = finalList.Find(y => y.BookingNo == x.BookingNo);
                        if (obj.IsNotNull())
                        {
                            finalList.Find(y => y.BookingNo == x.BookingNo).ImagePath = x.ImagePath;
                        }
                    });
                }
            }



            return finalList;
        }

        public async Task<List<YarnAllocationMaster>> GetPagedAsync2(Status status,
            string buyerIds,
            string buyerTeamIds,
            string countIds,
            string yBookingIds,
            string yItemMasterIds,
            string fabricShadeIds,
            string fabricTypeIds,
            string yarnTypeIds,
            string yarnRequisitionTypes,
            string fabricGSMIds,

            bool yBookingDateAsPerFR_Chk,
            string yBookingDateAsPerFR_From,
            string yBookingDateAsPerFR_To,

            bool actualYarnBookingDate_Chk,
            string actualYarnBookingDate_From,
            string actualYarnBookingDate_To,

            bool yarnInhouseStartDateAsPerFR_Chk,
            string yarnInhouseStartDateAsPerFR_From,
            string yarnInhouseStartDateAsPerFR_To,

            bool fabricDeliveryStartDate_Chk,
            string fabricDeliveryStartDate_From,
            string fabricDeliveryStartDate_To,

            bool fabricDeliveryEndDate_Chk,
            string fabricDeliveryEndDate_From,
            string fabricDeliveryEndDate_To,

            bool tNACalendarDays_Chk,
            string tNACalendarDays_From,
            string tNACalendarDays_To,

            PaginationInfo paginationInfo)
        {
            buyerIds = buyerIds.IsNotNullOrEmpty() ? $@" AND BuyerID IN ({buyerIds})" : "";
            buyerTeamIds = buyerTeamIds.IsNotNullOrEmpty() ? $@" AND BuyerTeamID IN ({buyerTeamIds})" : "";
            countIds = countIds.IsNotNullOrEmpty() ? $@" AND CountId IN ({countIds})" : "";
            yBookingIds = yBookingIds.IsNotNullOrEmpty() ? $@" AND YBookingID IN ({yBookingIds})" : "";
            yItemMasterIds = yItemMasterIds.IsNotNullOrEmpty() ? $@" AND YItemMasterID IN ({yItemMasterIds})" : "";

            fabricShadeIds = fabricShadeIds.IsNotNullOrEmpty() ? $@" AND FabricShadeId IN ({fabricShadeIds})" : "";
            fabricTypeIds = fabricTypeIds.IsNotNullOrEmpty() ? $@" AND FabricTypeId IN ({fabricTypeIds})" : "";
            yarnTypeIds = yarnTypeIds.IsNotNullOrEmpty() ? $@" AND YarnTypeId IN ({yarnTypeIds})" : "";
            yarnRequisitionTypes = yarnRequisitionTypes.IsNotNullOrEmpty() ? $@" AND YarnRequisitionType IN ({yarnRequisitionTypes})" : "";
            fabricGSMIds = fabricGSMIds.IsNotNullOrEmpty() ? $@" AND FabricGSMId IN ({fabricGSMIds})" : "";

            string actualYarnBookingDate_Date = actualYarnBookingDate_Chk && actualYarnBookingDate_From.IsNotNullOrEmpty() && actualYarnBookingDate_To.IsNotNullOrEmpty() ? $@" AND ActualYarnBookingDate BETWEEN {actualYarnBookingDate_From} AND {actualYarnBookingDate_To}" : "";

            string yBookingDateAsPerFR_Date = yBookingDateAsPerFR_Chk && yBookingDateAsPerFR_From.IsNotNullOrEmpty() && yBookingDateAsPerFR_To.IsNotNullOrEmpty() ? $@" AND D1.EventDate BETWEEN {yBookingDateAsPerFR_From} AND {yBookingDateAsPerFR_To}" : "";
            string yarnInhouseStartDateAsPerFR_Date = yarnInhouseStartDateAsPerFR_Chk && yarnInhouseStartDateAsPerFR_From.IsNotNullOrEmpty() && yarnInhouseStartDateAsPerFR_To.IsNotNullOrEmpty() ? $@" AND D2.EventDate BETWEEN {yarnInhouseStartDateAsPerFR_From} AND {yarnInhouseStartDateAsPerFR_To}" : "";
            string fabricDeliveryStartDate_Date = fabricDeliveryStartDate_Chk && fabricDeliveryStartDate_From.IsNotNullOrEmpty() && fabricDeliveryStartDate_To.IsNotNullOrEmpty() ? $@" AND D3.EventDate BETWEEN {fabricDeliveryStartDate_From} AND {fabricDeliveryStartDate_To}" : "";
            string fabricDeliveryEndDate_Date = fabricDeliveryEndDate_Chk && fabricDeliveryEndDate_From.IsNotNullOrEmpty() && fabricDeliveryEndDate_To.IsNotNullOrEmpty() ? $@" AND D4.EventDate BETWEEN {fabricDeliveryEndDate_From} AND {fabricDeliveryEndDate_To}" : "";

            string TNACalendarDays = tNACalendarDays_Chk && tNACalendarDays_From.IsNotNullOrEmpty() && tNACalendarDays_To.IsNotNullOrEmpty() ? $@" AND TNACalendarDays BETWEEN {tNACalendarDays_From} AND {tNACalendarDays_To}" : "";

            string tempGuid = CommonFunction.GetNewGuid();
            bool hasTempTable = false;

            bool isNeedImage = false;
            string orderBy = "";
            if (status == Status.Pending)
            {
                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? " Order By AcknowledgeDate Desc" : paginationInfo.OrderBy;
            }
            else
            {
                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? " Order By YarnAllocationID Desc" : paginationInfo.OrderBy;
            }
            string sql = "";

            switch (status)
            {
                case Status.Revise:
                    sql = $@"
                            With CutoffDate As
						(
						SELECT ETV.ValueID, ETV.ValueName
						FROM {DbNames.EPYSL}..EntityType ET
						INNER JOIN {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.EntityTypeID = ET.EntityTypeID
						WHERE ET.EntityTypeName = 'YarnAllocationCutOffDate'
						),
                        AQ AS
                            (
                                Select AllocationID, BookingNo = STRING_AGG(FBA.BookingNo,','),TotalAllocatedQty = SUM(YAC.TotalAllocationQty) 
                                FROM {TableNames.YARN_ALLOCATION_MASTER} YAM 
								INNER JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.AllocationID = YAM.YarnAllocationID
								INNER JOIN {TableNames.YARN_ALLOCATION_CHILD_ITEM} YACI ON YACI.AllocationChildID = YAC.AllocationChildID
								LEFT JOIN {TableNames.YARN_ALLOCATION_CHILD_PIPELINE_ITEM} YACPI ON YACPI.AllocationChildID = YAC.AllocationChildID
	                            INNER JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBCI.YBChildItemID = YAC.YBChildItemID
                                LEFT JOIN {TableNames.YarnBookingChildItem_New_Bk} YBCIB ON YBCIB.YBChildItemID = YAC.YBChildItemID
	                            INNER JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID = YBCI.YBChildID
	                            INNER JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID = YBC.YBookingID
	                            INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.BookingID = YBM.BookingID

	                            WHERE  ISNULL(YBM.PreProcessRevNo,0) = ISNULL(FBA.RevisionNo,0) AND
                                (ISNULL(YBM.RevisionNo,0) <> ISNULL(YACI.PreProcessRevNo,0) OR ISNULL(YACI.IsRevised,0) = 1 ) AND
                                (ISNULL(YBCI.NetYarnReqQty,0) <> ISNULL(YBCIB.NetYarnReqQty,0) OR YBCI.YItemMasterID<>YBCIB.YItemMasterID)
                                AND YBM.DateAdded >=(Select ValueName From CutoffDate)
                                GROUP BY YAC.AllocationID
                            ),
                            FinalList AS
                            (
                                Select YarnAllocationID,YarnAllocationDate,YarnAllocationNo,AQ.TotalAllocatedQty,E.EmployeeName AllocatedBy, AQ.BookingNo
                                FROM {TableNames.YARN_ALLOCATION_MASTER} YAM
                                INNER JOIN AQ ON AQ.AllocationID=YAM.YarnAllocationID
                                Inner Join {DbNames.EPYSL}..LoginUser LU On YAM.AddedBy = LU.UserCode
                                Inner Join {DbNames.EPYSL}..Employee E On E.EmployeeCode = LU.EmployeeCode
                            )
                            SELECT *, Count(*) Over() TotalRows FROM FinalList ";
                    break;
                case Status.ProposedForApproval:
                    sql = $@"
                    WITH CutoffDate As
						(
						SELECT ETV.ValueID, ETV.ValueName
						FROM {DbNames.EPYSL}..EntityType ET
						INNER JOIN {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.EntityTypeID = ET.EntityTypeID
						WHERE ET.EntityTypeName = 'YarnAllocationCutOffDate'
						),
                        AQ AS(
                        Select AllocationID, BookingNo = STRING_AGG(FBA.BookingNo,','),TotalAllocatedQty = SUM(YAC.TotalAllocationQty)
                        --,IsInValidOperation = CASE WHEN ISNULL(YBM.RevisionNo,0) = ISNULL(YACI.PreProcessRevNo,0) THEN 0 ELSE 1 END
                        FROM {TableNames.YARN_ALLOCATION_CHILD_ITEM} YACI
						INNER JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.AllocationChildID = YACI.AllocationChildID
                        --IsInValidOperation = CASE WHEN ISNULL(YBM.RevisionNo,0) = ISNULL(YAC.PreRevisionNo,0) THEN 0 ELSE 1 END
                        --FROM {TableNames.YARN_ALLOCATION_CHILD} YAC
                        INNER JOIN {TableNames.YARN_ALLOCATION_MASTER} YAM ON YAM.YarnAllocationID = YAC.AllocationID
	                    INNER JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBCI.YBChildItemID = YAC.YBChildItemID
	                    INNER JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID = YBCI.YBChildID
	                    INNER JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID = YBC.YBookingID
	                    INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.BookingID = YBM.BookingID
                        --WHERE ISNULL(YBM.RevisionNo,0) = ISNULL(YAC.PreRevisionNo,0)
                        WHERE YBM.DateAdded >=(Select ValueName From CutoffDate)
                        GROUP BY YAC.AllocationID--, ISNULL(YBM.RevisionNo,0),ISNULL(YACI.PreProcessRevNo,0)
                    ),
                    FinalList AS
                    (
                        Select YarnAllocationID,YarnAllocationDate,YarnAllocationNo,
	                    AQ.TotalAllocatedQty,AllocatedBy = E.EmployeeName, AQ.BookingNo--, AQ.IsInValidOperation
                        FROM {TableNames.YARN_ALLOCATION_MASTER} YAM
                        INNER JOIN AQ ON AQ.AllocationID=YAM.YarnAllocationID
                        Inner Join {DbNames.EPYSL}..LoginUser LU On YAM.AddedBy = LU.UserCode
                        Inner Join {DbNames.EPYSL}..Employee E On E.EmployeeCode = LU.EmployeeCode
                        WHERE YAM.Propose = 1 AND YAM.Approve = 0 AND YAM.Reject = 0 AND YAM.UnAcknowledge = 0
                    )
                    SELECT *, Count(*) Over() TotalRows FROM FinalList ";
                    break;
                case Status.Approved:
                    sql = $@"
                             With CutoffDate As
						(
						SELECT ETV.ValueID, ETV.ValueName
						FROM {DbNames.EPYSL}..EntityType ET
						INNER JOIN {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.EntityTypeID = ET.EntityTypeID
						WHERE ET.EntityTypeName = 'YarnAllocationCutOffDate'
						),
                        AQ AS
                            (
                                Select AllocationID, BookingNo = STRING_AGG(FBA.BookingNo,','),TotalAllocatedQty = SUM(YAC.TotalAllocationQty)
                                --,IsInValidOperation = CASE WHEN ISNULL(YBM.RevisionNo,0) = ISNULL(YACI.PreProcessRevNo,0) THEN 0 ELSE 1 END
                                FROM {TableNames.YARN_ALLOCATION_CHILD_ITEM} YACI
								INNER JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.AllocationChildID = YACI.AllocationChildID
                                --IsInValidOperation = CASE WHEN ISNULL(YBM.RevisionNo,0) = ISNULL(YAC.PreRevisionNo,0) THEN 0 ELSE 1 END
                                --FROM {TableNames.YARN_ALLOCATION_CHILD} YAC
                                INNER JOIN {TableNames.YARN_ALLOCATION_MASTER} YAM ON YAM.YarnAllocationID = YAC.AllocationID
	                            INNER JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBCI.YBChildItemID = YAC.YBChildItemID
	                            INNER JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID = YBCI.YBChildID
	                            INNER JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID = YBC.YBookingID
	                            INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.BookingID = YBM.BookingID
                                --WHERE ISNULL(YBM.RevisionNo,0) = ISNULL(YAC.PreRevisionNo,0)
                                WHERE YBM.DateAdded >=(Select ValueName From CutoffDate)
                                GROUP BY YAC.AllocationID--, ISNULL(YBM.RevisionNo,0), ISNULL(YAC.PreRevisionNo,0),ISNULL(YACI.PreProcessRevNo,0)
                            ),
                            FinalList AS
                            (
                                Select YarnAllocationID,YarnAllocationDate,YarnAllocationNo,AQ.TotalAllocatedQty,E.EmployeeName AllocatedBy, AQ.BookingNo--, AQ.IsInValidOperation
                                FROM {TableNames.YARN_ALLOCATION_MASTER} YAM
                                INNER JOIN AQ ON AQ.AllocationID=YAM.YarnAllocationID
                                Inner Join {DbNames.EPYSL}..LoginUser LU On YAM.AddedBy = LU.UserCode
                                Inner Join {DbNames.EPYSL}..Employee E On E.EmployeeCode = LU.EmployeeCode
                                WHERE YAM.Approve=1 AND YAM.Acknowledge = 0 AND YAM.UnAcknowledge = 0
                            )
                            SELECT *, Count(*) Over() TotalRows FROM FinalList  ";
                    break;
                case Status.Reject:
                    sql = $@"
                            With CutoffDate As
						(
						SELECT ETV.ValueID, ETV.ValueName
						FROM {DbNames.EPYSL}..EntityType ET
						INNER JOIN {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.EntityTypeID = ET.EntityTypeID
						WHERE ET.EntityTypeName = 'YarnAllocationCutOffDate'
						),
                        AQ AS(
                                Select AllocationID, BookingNo = STRING_AGG(FBA.BookingNo,','),TotalAllocatedQty = SUM(YAC.TotalAllocationQty)
                                --,IsInValidOperation = CASE WHEN ISNULL(YBM.RevisionNo,0) = ISNULL(YACI.PreProcessRevNo,0) THEN 0 ELSE 1 END
                                FROM {TableNames.YARN_ALLOCATION_CHILD_ITEM} YACI
								INNER JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.AllocationChildID = YACI.AllocationChildID
                                --IsInValidOperation = CASE WHEN ISNULL(YBM.RevisionNo,0) = ISNULL(YAC.PreRevisionNo,0) THEN 0 ELSE 1 END
                                --FROM {TableNames.YARN_ALLOCATION_CHILD} YAC
                                INNER JOIN {TableNames.YARN_ALLOCATION_MASTER} YAM ON YAM.YarnAllocationID = YAC.AllocationID
	                            INNER JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBCI.YBChildItemID = YAC.YBChildItemID
	                            INNER JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID = YBCI.YBChildID
	                            INNER JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID = YBC.YBookingID
	                            INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.BookingID = YBM.BookingID
                                --WHERE ISNULL(YBM.RevisionNo,0) = ISNULL(YAC.PreRevisionNo,0)
                                WHERE YBM.DateAdded >=(Select ValueName From CutoffDate)
                                GROUP BY YAC.AllocationID--, ISNULL(YBM.RevisionNo,0), ISNULL(YAC.PreRevisionNo,0)
                            ),
                            FinalList AS
                            (
                                Select YarnAllocationID,YarnAllocationDate,YarnAllocationNo,AQ.TotalAllocatedQty,E.EmployeeName AllocatedBy, AQ.BookingNo--, AQ.IsInValidOperation
                                FROM {TableNames.YARN_ALLOCATION_MASTER} YAM
                                INNER JOIN AQ ON AQ.AllocationID=YAM.YarnAllocationID
                                Inner Join {DbNames.EPYSL}..LoginUser LU On YAM.AddedBy = LU.UserCode
                                Inner Join {DbNames.EPYSL}..Employee E On E.EmployeeCode = LU.EmployeeCode
                                WHERE YAM.Reject = 1 AND YAM.UnAcknowledge = 0
                            )
                            SELECT *, Count(*) Over() TotalRows FROM FinalList ";
                    break;
                case Status.Acknowledge:
                    sql = $@"
                            With AQ AS(
                                Select AllocationID, BookingNo = STRING_AGG(FBA.BookingNo,','),TotalAllocatedQty = SUM(YAC.TotalAllocationQty)
                                --,IsInValidOperation = CASE WHEN ISNULL(YBM.RevisionNo,0) = ISNULL(YACI.PreProcessRevNo,0) THEN 0 ELSE 1 END
                                FROM {TableNames.YARN_ALLOCATION_CHILD_ITEM} YACI
								INNER JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.AllocationChildID = YACI.AllocationChildID
                                --IsInValidOperation = CASE WHEN ISNULL(YBM.RevisionNo,0) = ISNULL(YAC.PreRevisionNo,0) THEN 0 ELSE 1 END
                                --FROM {TableNames.YARN_ALLOCATION_CHILD} YAC
                                INNER JOIN {TableNames.YARN_ALLOCATION_MASTER} YAM ON YAM.YarnAllocationID = YAC.AllocationID
	                            INNER JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBCI.YBChildItemID = YAC.YBChildItemID
	                            INNER JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID = YBCI.YBChildID
	                            INNER JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID = YBC.YBookingID
	                            INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.BookingID = YBM.BookingID
                                --WHERE ISNULL(YBM.RevisionNo,0) = ISNULL(YAC.PreRevisionNo,0)
                                GROUP BY YAC.AllocationID--, ISNULL(YBM.RevisionNo,0), ISNULL(YAC.PreRevisionNo,0)
                            ),
                            FinalList AS
                            (
                                Select YarnAllocationID,YarnAllocationDate,YarnAllocationNo,AQ.TotalAllocatedQty,E.EmployeeName AllocatedBy, AQ.BookingNo--, AQ.IsInValidOperation
                                FROM {TableNames.YARN_ALLOCATION_MASTER} YAM
                                INNER JOIN AQ ON AQ.AllocationID=YAM.YarnAllocationID
                                Inner Join {DbNames.EPYSL}..LoginUser LU On YAM.AddedBy = LU.UserCode
                                Inner Join {DbNames.EPYSL}..Employee E On E.EmployeeCode = LU.EmployeeCode
                                WHERE YAM.Acknowledge = 1 AND YAM.UnAcknowledge = 0
                            )
                            SELECT *, Count(*) Over() TotalRows FROM FinalList ";
                    break;
                case Status.UnAcknowledge:
                    sql = $@"
                            With AQ AS(
                                Select AllocationID, BookingNo = STRING_AGG(FBA.BookingNo,','),TotalAllocatedQty = SUM(YAC.TotalAllocationQty)
                                --,IsInValidOperation = CASE WHEN ISNULL(YBM.RevisionNo,0) = ISNULL(YACI.PreProcessRevNo,0) THEN 0 ELSE 1 END
                                FROM {TableNames.YARN_ALLOCATION_CHILD_ITEM} YACI
								INNER JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.AllocationChildID = YACI.AllocationChildID
                                --IsInValidOperation = CASE WHEN ISNULL(YBM.RevisionNo,0) = ISNULL(YAC.PreRevisionNo,0) THEN 0 ELSE 1 END
                                --FROM {TableNames.YARN_ALLOCATION_CHILD} YAC
                                INNER JOIN {TableNames.YARN_ALLOCATION_MASTER} YAM ON YAM.YarnAllocationID = YAC.AllocationID
	                            INNER JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBCI.YBChildItemID = YAC.YBChildItemID
	                            INNER JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID = YBCI.YBChildID
	                            INNER JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID = YBC.YBookingID
	                            INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.BookingID = YBM.BookingID
                                --WHERE ISNULL(YBM.RevisionNo,0) = ISNULL(YAC.PreRevisionNo,0)
                                GROUP BY YAC.AllocationID--, ISNULL(YBM.RevisionNo,0), ISNULL(YAC.PreRevisionNo,0)
                            ),
                            FinalList AS
                            (
                                Select YarnAllocationID,YarnAllocationDate,YarnAllocationNo,AQ.TotalAllocatedQty,E.EmployeeName AllocatedBy, AQ.BookingNo--, AQ.IsInValidOperation
                                FROM {TableNames.YARN_ALLOCATION_MASTER} YAM
                                INNER JOIN AQ ON AQ.AllocationID=YAM.YarnAllocationID
                                Inner Join {DbNames.EPYSL}..LoginUser LU On YAM.AddedBy = LU.UserCode
                                Inner Join {DbNames.EPYSL}..Employee E On E.EmployeeCode = LU.EmployeeCode
                                WHERE YAM.UnAcknowledge = 1
                            )
                            SELECT *, Count(*) Over() TotalRows FROM FinalList ";
                    break;
                case Status.All:
                    isNeedImage = true;
                    sql = $@"
                            With CutoffDate As
						(
						SELECT ETV.ValueID, ETV.ValueName
						FROM {DbNames.EPYSL}..EntityType ET
						INNER JOIN {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.EntityTypeID = ET.EntityTypeID
						WHERE ET.EntityTypeName = 'YarnAllocationCutOffDate'
						),
                        AQ AS(
                                Select AllocationID, BookingNo = STRING_AGG(FBA.BookingNo,','),TotalAllocatedQty = SUM(TotalAllocationQty) 
                                FROM {TableNames.YARN_ALLOCATION_CHILD} YAC
                                INNER JOIN {TableNames.YARN_ALLOCATION_MASTER} YAM ON YAM.YarnAllocationID = YAC.AllocationID
	                            INNER JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBCI.YBChildItemID = YAC.YBChildItemID
	                            INNER JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID = YBCI.YBChildID
	                            INNER JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID = YBC.YBookingID
	                            INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.BookingID = YBM.BookingID
                                WHERE YBM.DateAdded >=(Select ValueName From CutoffDate)
                                GROUP BY YAC.AllocationID
                            ),
                            FinalList AS
                            (
                                Select YarnAllocationID,YarnAllocationDate,YarnAllocationNo,AQ.TotalAllocatedQty,E.EmployeeName AllocatedBy, AQ.BookingNo
                                FROM {TableNames.YARN_ALLOCATION_MASTER} YAM
                                INNER JOIN AQ ON AQ.AllocationID=YAM.YarnAllocationID
                                Inner Join {DbNames.EPYSL}..LoginUser LU On YAM.AddedBy = LU.UserCode
                                Inner Join {DbNames.EPYSL}..Employee E On E.EmployeeCode = LU.EmployeeCode
                            )
                            SELECT *, Count(*) Over() TotalRows FROM FinalList ";
                    break;
                default: //Status.Pending:
                    sql = $@"
                        WITH CutoffDate As
                        (
	                        SELECT ETV.ValueID, ETV.ValueName
	                        FROM {DbNames.EPYSL}..EntityType ET
	                        INNER JOIN {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.EntityTypeID = ET.EntityTypeID
	                        WHERE ET.EntityTypeName = 'YarnAllocationCutOffDate'
                        ),
                        CQ AS(
	                        Select YAC.YBChildItemID
	                        ,TotalAllocatedQty = SUM(TotalAllocationQty) 
                            FROM {TableNames.YARN_ALLOCATION_CHILD} YAC
	                        INNER JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBCI.YBChildItemID = YAC.YBChildItemID
	                        INNER JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID = YBCI.YBChildID
	                        INNER JOIN {TableNames.YarnBookingMaster_New} YB ON YB.YBookingID = YBC.YBookingID
	                        INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FBC ON FBC.BookingChildID = YBC.BookingChildID AND FBC.BookingID = YBC.BookingID
	                        WHERE YB.Acknowledge = 1
                            AND YB.DateAdded >= (Select CAST(ValueName AS DATE) From CutoffDate)
	                        GROUP BY YAC.YBChildItemID
                        )
                        ,FBA AS (
	                        SELECT YBCI.YBChildItemID
	                        ,YBC.YBChildID
	                        ,BuyerID = a.BuyerID
	                        ,BuyerName = CTO.ShortName
	                        ,BuyerTeamID = a.BuyerTeamID
	                        ,BuyerTeamName = CCT.TeamName
	                        ,CountId = IMYarnCount.Segment6ValueID
	                        ,NumericCount = SV6.SegmentValue
	                        ,CS.SeasonName
	                        ,YB.YRequiredDate
	                        ,InHouseDate = CASE WHEN a.IsSample = 1 THEN SBM.InHouseDate ELSE BM.InHouseDate END
	                        ,YBCI.YarnPly
	                        ,YBCI.YarnLotNo
	                        ,SpinnerName = CASE WHEN YBCI.SpinnerID > 0 THEN CCS.ShortName ELSE '' END
	                        ,YarnReqQty = YBCI.NetYarnReqQty
	                        ,TotalAllocatedQty = ISNULL(CQ.TotalAllocatedQty,0)
	                        --,YarnStockQty = Stock.StockQty
	                        ,YarnCertification = ''
	                        ,BookingStatus = ''
	                        ,FabricQty = a.BookingQty
	                        ,YarnInhouseEndDate = NUll
	                        ,KnittingStartDate4P = NUll
	                        ,KnittingEndDate4P = NUll /*blank*/ 
	                        ,TNACalendarDays = EOM.CalendarDays,YB.RevisionReason
	                        ,YarnRequisitionType = case when YB.IsAddition=1 then 'Add-'+Convert(varchar(100),YB.AdditionNo) when YB.RevisionNo>0 then 'Rev-'+Convert(varchar(100),YB.RevisionNo) else 'Main' end
	                        ,RequiredYarnQuantityKG = ISNULL(YBCI.NetYarnReqQty,0)
	                        ,AllocationBalanceQTYKG =  ISNULL(YBCI.NetYarnReqQty,0) - ISNULL(CQ.TotalAllocatedQty,0)
	                        ,YDST = Case When YBCI.YD = 1 Then 'YES' Else 'NO' END /*YarnDyeingOrder */
	                        ,AcknowledgeDate = MAX(YB.AcknowledgeDate)
                            ,IsRevisionPending = CASE WHEN ISNULL(YB.PreProcessRevNo,0) <> ISNULL(a.RevisionNo,0) THEN 1 ELSE 0 END
	                        ,a.ExportOrderID
	                        ,YBCI.YItemMasterId
	                        ,YBookingId = YBC.YBookingID
	                        ,YB.YBookingNo
	                        ,YBCI.YarnCategory

                            ,FabricShadeId = CASE WHEN YB.SubGroupID = 1 THEN SV3.SegmentValueID ELSE SV5.SegmentValueID END
                            ,FabricShade = CASE WHEN YB.SubGroupID = 1 THEN SV3.SegmentValue ELSE SV5.SegmentValue END

                            ,FabricTypeId = ISV1.SegmentValueID
                            ,FabricType = ISV1.SegmentValue

                            ,YarnTypeId = ISV3.SegmentValueID
                            ,YarnType = ISV3.SegmentValue

                            ,FabricGSMId = CASE WHEN YB.SubGroupID = 1 THEN SV4.SegmentValueID ELSE 0 END
                            ,FabricGSM = CASE WHEN YB.SubGroupID = 1 THEN SV4.SegmentValue ELSE '' END

	                        ,ActualYarnBookingDate = MAX(a.BookingDate)

                            FROM {TableNames.YarnBookingChildItem_New} YBCI
	                        INNER JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID = YBCI.YBChildID
	                        INNER JOIN {TableNames.YarnBookingMaster_New} YB ON YB.YBookingID = YBC.YBookingID
	                        INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FBC ON FBC.BookingChildID = YBC.BookingChildID AND FBC.BookingID = YBC.BookingID AND FBC.BookingQty > 0
	                        LEFT JOIN CQ ON YBCI.YBChildItemID = CQ.YBChildItemID
	                        INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} a ON a.FBAckID = FBC.AcknowledgeID
	                        INNER JOIN {TableNames.FabricBookingAcknowledge} FBA ON FBA.BookingID = a.BookingID
	                        LEFT JOIN {DbNames.EPYSL}..BookingMaster BM ON BM.BookingID = FBA.BookingID
                            LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster SBM ON SBM.BookingID = FBA.BookingID
	                        LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} b ON b.BookingID = a.BookingID
	                        LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = a.BuyerID
	                        LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = a.BuyerTeamID
	                        LEFT JOIN {DbNames.EPYSL}..CompanyEntity C ON C.CompanyID = b.CompanyID
	                        LEFT JOIN {DbNames.EPYSL}..Contacts CCS ON CCS.ContactID = YBCI.SpinnerID
	                        LEFT JOIN {DbNames.EPYSL}..ExportOrderMaster EOM ON EOM.ExportOrderID = a.ExportOrderID
	                        Left Join {DbNames.EPYSL}..ItemMaster IM On IM.ItemMasterID = YBC.ItemMasterID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
	                        Left Join {DbNames.EPYSL}..ItemSegmentValue SV3 On SV3.SegmentValueID = IM.Segment3ValueID
	                        Left Join {DbNames.EPYSL}..ItemSegmentValue SV4 On SV4.SegmentValueID = IM.Segment4ValueID
	                        Left Join {DbNames.EPYSL}..ItemSegmentValue SV5 On SV5.SegmentValueID = IM.Segment5ValueID

                            Left Join {DbNames.EPYSL}..ItemMaster IMYarnCount On IMYarnCount.ItemMasterID = YBCI.YItemMasterId
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IMYarnCount.Segment3ValueID
                            Left Join {DbNames.EPYSL}..ItemSegmentValue SV6 On SV6.SegmentValueID = IMYarnCount.Segment6ValueID

                            LEFT JOIN {DbNames.EPYSL}..StyleMaster SM ON SM.StyleMasterID = EOM.StyleMasterID
                            LEFT JOIN {DbNames.EPYSL}..ContactSeason CS ON CS.SeasonID = SM.SeasonID

	                        LEFT JOIN {DbNames.EPYSL}..OrderEventCalander OE On OE.ExportOrderID = EOM.ExportOrderID
	                        LEFT JOIN {DbNames.EPYSL}..TNAEvent TE On TE.EventID = OE.EventID AND TE.EventDescription = 'Fabric booking'
	                        LEFT JOIN {DbNames.EPYSL}..TNAEvent TE1 On TE1.EventID = OE.EventID AND TE1.EventDescription = 'Yarn Allocation'
	                        LEFT JOIN {DbNames.EPYSL}..TNAEvent TE2 On TE2.EventID = OE.EventID AND TE2.EventDescription = 'Fabric Delivery start'
	                        LEFT JOIN {DbNames.EPYSL}..TNAEvent TE3 On TE3.EventID = OE.EventID AND TE3.EventDescription = 'Fabric Delivery complete'

	                        WHERE
                            YB.Acknowledge = 1
	                        AND ISNULL(CQ.TotalAllocatedQty,0) < ISNULL(YBCI.NetYarnReqQty,0)
                            AND YB.DateAdded >= (Select CAST(ValueName AS DATE) From CutoffDate)

	                        GROUP BY YBCI.YBChildItemID, YBC.YBChildID, YBC.YBookingID, YB.YBookingNo, CTO.ShortName, CCT.TeamName, CS.SeasonName, YB.YRequiredDate,
	                        a.IsSample, SBM.InHouseDate, BM.InHouseDate, YBCI.YarnPly, YBCI.YarnCategory, YBCI.YarnLotNo,
	                        YBCI.SpinnerID, CCS.ShortName, YBCI.NetYarnReqQty
	                        ,ISNULL(CQ.TotalAllocatedQty,0)
	                        ,YBCI.YD, a.BookingQty,
	                        CASE WHEN a.IsSample = 1 THEN SBM.BookingDate ELSE BM.BookingDate END,
	                        EOM.CalendarDays,YB.RevisionReason,
	                        CASE WHEN YB.SubGroupID = 1 THEN SV3.SegmentValue ELSE SV5.SegmentValue END, 
	                        CASE WHEN YB.SubGroupID = 1 THEN SV4.SegmentValue ELSE '' END,
	                        case when YB.IsAddition=1 then 'Add-'+Convert(varchar(100),YB.AdditionNo) when YB.RevisionNo>0 then 'Rev-'+Convert(varchar(100),YB.RevisionNo) else 'Main' end,
	                        YBCI.NetYarnReqQty
	                        ,SV6.SegmentValue
	                        ,ISV1.SegmentValue,
	                        ISV3.SegmentValue
	                        ,CASE WHEN ISNULL(YB.PreProcessRevNo,0) <> ISNULL(a.RevisionNo,0) THEN 1 ELSE 0 END
	                        ,a.ExportOrderID
	                        ,YBCI.YItemMasterId,
	                        (CASE WHEN YB.SubGroupID = 1 THEN SV4.SegmentValueID ELSE 0 END),
                            a.BuyerID,a.BuyerTeamID,IMYarnCount.Segment6ValueID,
                            CASE WHEN YB.SubGroupID = 1 THEN SV3.SegmentValueID ELSE SV5.SegmentValueID END,
                            ISV1.SegmentValueID,ISV3.SegmentValueID
                        ),
                        FB_Date AS
                        (
	                        SELECT EOM.ExportOrderID, EOM.ExportOrderNo,OE.EventID, CDays = MIN(OE.CDays), TNADays = MAX(OE.TNADays)
	                        ,EventDate = MIN(OE.EventDate)
                            ,EventDescriptionId = TE.EventDescription
	                        FROM FBA
	                        INNER JOIN {DbNames.EPYSL}..ExportOrderMaster EOM ON EOM.ExportOrderID = FBA.ExportOrderID
	                        INNER JOIN {DbNames.EPYSL}..OrderEventCalander OE On OE.ExportOrderID = EOM.ExportOrderID
	                        INNER JOIN {DbNames.EPYSL}..TNAEvent TE On TE.EventID = OE.EventID
	                        WHERE TE.EventDescription IN ('Fabric booking') 
	                        AND EOM.EWOStatusID <> 131
	                        GROUP BY EOM.ExportOrderID, EOM.ExportOrderNo,OE.EventID,TE.EventDescription
                        ),
                        Actual_YA_Date AS
                        (
	                        SELECT EOM.ExportOrderID, EOM.ExportOrderNo,OE.EventID, CDays = MIN(OE.CDays), TNADays = MAX(OE.TNADays)
	                        ,EventDate = MIN(OE.EventDate)
                            ,EventDescriptionId = TE.EventDescription
	                        FROM FBA
	                        INNER JOIN {DbNames.EPYSL}..ExportOrderMaster EOM ON EOM.ExportOrderID = FBA.ExportOrderID
	                        INNER JOIN {DbNames.EPYSL}..OrderEventCalander OE On OE.ExportOrderID = EOM.ExportOrderID
	                        INNER JOIN {DbNames.EPYSL}..TNAEvent TE On TE.EventID = OE.EventID
	                        WHERE TE.EventDescription IN ('Yarn Allocation') 
	                        AND EOM.EWOStatusID <> 131
	                        GROUP BY EOM.ExportOrderID, EOM.ExportOrderNo,OE.EventID,TE.EventDescription
                        ),
                        FDS_Date AS
                        (
	                        SELECT EOM.ExportOrderID, EOM.ExportOrderNo,OE.EventID, CDays = MIN(OE.CDays), TNADays = MAX(OE.TNADays)
	                        ,EventDate = MIN(OE.EventDate)
                            ,EventDescriptionId = TE.EventDescription
	                        FROM FBA
	                        INNER JOIN {DbNames.EPYSL}..ExportOrderMaster EOM ON EOM.ExportOrderID = FBA.ExportOrderID
	                        INNER JOIN {DbNames.EPYSL}..OrderEventCalander OE On OE.ExportOrderID = EOM.ExportOrderID
	                        INNER JOIN {DbNames.EPYSL}..TNAEvent TE On TE.EventID = OE.EventID
	                        WHERE TE.EventDescription IN ('Fabric Delivery start') 
	                        AND EOM.EWOStatusID <> 131
	                        GROUP BY EOM.ExportOrderID, EOM.ExportOrderNo,OE.EventID,TE.EventDescription
                        ),
                        FDE_Date AS
                        (
	                        SELECT EOM.ExportOrderID, EOM.ExportOrderNo,OE.EventID, CDays = MIN(OE.CDays), TNADays = MAX(OE.TNADays)
	                        ,EventDate = MIN(OE.EventDate)
                            ,EventDescriptionId = TE.EventDescription
	                        FROM FBA
	                        INNER JOIN {DbNames.EPYSL}..ExportOrderMaster EOM ON EOM.ExportOrderID = FBA.ExportOrderID
	                        INNER JOIN {DbNames.EPYSL}..OrderEventCalander OE On OE.ExportOrderID = EOM.ExportOrderID
	                        INNER JOIN {DbNames.EPYSL}..TNAEvent TE On TE.EventID = OE.EventID
	                        WHERE TE.EventDescription IN ('Fabric Delivery complete') 
	                        AND EOM.EWOStatusID <> 131
	                        GROUP BY EOM.ExportOrderID, EOM.ExportOrderNo,OE.EventID,TE.EventDescription
                        ),
                        FinalList AS
                        (
	                        SELECT FBA.*	
	                        ,FabricBookingDate = D1.EventDate
	                        ,YarnInhouseStartDate = D2.EventDate
	                        ,FabricsDeliveryStartDate = D3.EventDate
	                        ,FabricsDeliveryEndDate = D4.EventDate

	                        FROM FBA
	                        LEFT JOIN FB_Date D1 ON D1.ExportOrderID = FBA.ExportOrderID
	                        LEFT JOIN Actual_YA_Date D2 ON D2.ExportOrderID = FBA.ExportOrderID
	                        LEFT JOIN FDS_Date D3 ON D3.ExportOrderID = FBA.ExportOrderID
	                        LEFT JOIN FDE_Date D4 ON D4.ExportOrderID = FBA.ExportOrderID

                            WHERE 1=1
                            {buyerIds}
                            {buyerTeamIds}
                            {countIds}
                            {yBookingIds}
                            {yItemMasterIds}
                            {fabricShadeIds}
                            {fabricTypeIds}
                            {yarnTypeIds}
                            {yarnRequisitionTypes}
                            {fabricGSMIds}
                            {actualYarnBookingDate_Date}
                            {yBookingDateAsPerFR_Date}
                            {yarnInhouseStartDateAsPerFR_Date}
                            {fabricDeliveryStartDate_Date}
                            {fabricDeliveryEndDate_Date}
                            {TNACalendarDays}
                        )
                        SELECT * INTO #TempTable{tempGuid} FROM FinalList 
	                    SELECT *,Count(*) Over() TotalRows FROM #TempTable{tempGuid}";

                    hasTempTable = true;

                    break;

            }
            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            if (hasTempTable)
            {
                sql += $@" DROP TABLE #TempTable{tempGuid}";
            }

            var finalList = await _service.GetDataAsync<YarnAllocationMaster>(sql);


            var exportOrderIds = string.Join(",", finalList.Select(x => x.ExportOrderID).Distinct());
            var eventList = await this.GetEvents(exportOrderIds);

            var ybChildItemIDs = string.Join(",", finalList.Select(x => x.YBChildItemID).Distinct());
            var stockInfoList = await this.GetStockInfo(ybChildItemIDs);

            finalList.ForEach(x =>
            {
                var bookingNos = CommonFunction.GetDefaultValueWhenInvalidS(x.BookingNo).Split(',');
                x.BookingNo = string.Join(",", bookingNos.Distinct());

                var events = eventList.Where(y => y.ExportOrderID == x.ExportOrderID).ToList();
                events.ForEach(e =>
                {
                    if (e.EventDescriptionId == 1) x.YarnBookingDate = e.EventDate;
                    else if (e.EventDescriptionId == 2) x.FabricsDeliveryStartDate = e.EventDate;
                    else if (e.EventDescriptionId == 3) x.FabricBookingDate = e.EventDate;
                    else if (e.EventDescriptionId == 4) x.YarnInhouseStartDate = e.EventDate;
                    else if (e.EventDescriptionId == 5) x.FabricsDeliveryEndDate = e.EventDate;
                });

                var stockInfos = stockInfoList.Where(y => y.YBChildItemID == x.YBChildItemID).ToList();
                stockInfos.ForEach(s =>
                {
                    x.YarnStockQty += s.StockQty;
                });
            });

            if (isNeedImage)
            {
                string bookingNos = string.Join("','", finalList.Select(x => x.BookingNo).Distinct());
                if (bookingNos.IsNotNullOrEmpty())
                {
                    var fBookingAcknowledgeImages = await _service.GetDataAsync<YarnAllocationChildItem>(CommonQueries.GetImagePathQuery(bookingNos));
                    fBookingAcknowledgeImages.ForEach(x =>
                    {
                        var obj = finalList.Find(y => y.BookingNo == x.BookingNo);
                        if (obj.IsNotNull())
                        {
                            finalList.Find(y => y.BookingNo == x.BookingNo).ImagePath = x.ImagePath;
                        }
                    });
                }
            }
            return finalList;
        }

        public async Task<List<YarnAllocationChildItem>> GetFilterListPending(Status status,
                int searchFieldNameId,
                string selectedBuyerIds,
                string selectedBuyerTeamIds,
                string selectedCountIds,
                string selectedYBookingIds,
                string selectedYItemMasterIds,
                string selectedFabricShadeIds,
                string selectedFabricTypeIds,
                string selectedYarnTypeIds,
                string selectedYarnRequisitionTypes,
                string selectedFabricGSMIds,
                PaginationInfo paginationInfo)
        {
            #region
            /*
             searchFieldNameId = 1 //Buyer
             searchFieldNameId = 2 //BuyerTeam
             searchFieldNameId = 3 //NumericCount
             searchFieldNameId = 4 //YarnBookingNo
             searchFieldNameId = 5 //YarnDescription
             searchFieldNameId = 6 //FabricShade
             searchFieldNameId = 7 //FabricType
             searchFieldNameId = 8 //YarnType
             searchFieldNameId = 9 //YarnRequisitionType
             searchFieldNameId = 10 //Fabric GSM
            */
            #endregion

            selectedBuyerIds = selectedBuyerIds.IsNotNullOrEmpty() ? $@" AND a.BuyerID IN ({selectedBuyerIds})" : "";
            selectedBuyerTeamIds = selectedBuyerTeamIds.IsNotNullOrEmpty() ? $@" AND a.BuyerTeamID IN ({selectedBuyerTeamIds})" : "";
            selectedCountIds = selectedCountIds.IsNotNullOrEmpty() ? $@" AND IMYarnCount.Segment6ValueID IN ({selectedCountIds})" : "";
            selectedYBookingIds = selectedYBookingIds.IsNotNullOrEmpty() ? $@" AND YB.YBookingID IN ({selectedYBookingIds})" : "";
            selectedYItemMasterIds = selectedYItemMasterIds.IsNotNullOrEmpty() ? $@" AND YBCI.YItemMasterID IN ({selectedYItemMasterIds})" : "";

            selectedFabricShadeIds = selectedFabricShadeIds.IsNotNullOrEmpty() ? $@" AND (CASE WHEN YB.SubGroupID = 1 THEN SV3.SegmentValueID ELSE SV5.SegmentValueID END) IN ({selectedFabricShadeIds})" : "";
            selectedFabricTypeIds = selectedFabricTypeIds.IsNotNullOrEmpty() ? $@" AND ISV1.SegmentValueID IN ({selectedFabricTypeIds})" : "";
            selectedYarnTypeIds = selectedYarnTypeIds.IsNotNullOrEmpty() ? $@" AND ISV3.SegmentValueID IN ({selectedYarnTypeIds})" : "";
            selectedYarnRequisitionTypes = selectedYarnRequisitionTypes.IsNotNullOrEmpty() ? $@" AND (case when YB.IsAddition=1 then 'Add-'+Convert(varchar(100),YB.AdditionNo) when YB.RevisionNo>0 then 'Rev-'+Convert(varchar(100),YB.RevisionNo) else 'Main' end) IN ({selectedYarnRequisitionTypes})" : "";
            selectedFabricGSMIds = selectedFabricGSMIds.IsNotNullOrEmpty() ? $@" AND (CASE WHEN YB.SubGroupID = 1 THEN SV4.SegmentValueID ELSE 0 END) IN ({selectedFabricGSMIds})" : "";

            string orderBy = "";
            string sql = "";

            string withQuery = $@"
                WITH 
                CutoffDate As
                (
	                SELECT ETV.ValueID, ETV.ValueName
	                FROM {DbNames.EPYSL}..EntityType ET
	                INNER JOIN {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.EntityTypeID = ET.EntityTypeID
	                WHERE ET.EntityTypeName = 'YarnAllocationCutOffDate'
                ),
                CQ AS(
	                Select YAC.YBChildItemID
	                ,TotalAllocatedQty = SUM(TotalAllocationQty)
                    FROM {TableNames.YARN_ALLOCATION_CHILD} YAC
	                INNER JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBCI.YBChildItemID = YAC.YBChildItemID
	                INNER JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID = YBCI.YBChildID
	                INNER JOIN {TableNames.YarnBookingMaster_New} YB ON YB.YBookingID = YBC.YBookingID
	                INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FBC ON FBC.BookingChildID = YBC.BookingChildID AND FBC.BookingID = YBC.BookingID
	                WHERE YB.Acknowledge = 1
                    AND YB.DateAdded >= (Select CAST(ValueName AS DATE) From CutoffDate)
	                GROUP BY YAC.YBChildItemID
                )";

            string fromQuery = $@" 
            FROM {TableNames.YarnBookingChildItem_New} YBCI
            INNER JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID = YBCI.YBChildID
            INNER JOIN {TableNames.YarnBookingMaster_New} YB ON YB.YBookingID = YBC.YBookingID
            INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FBC ON FBC.BookingChildID = YBC.BookingChildID AND FBC.BookingID = YBC.BookingID AND FBC.BookingQty > 0
            LEFT JOIN CQ ON YBCI.YBChildItemID = CQ.YBChildItemID
            INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} a ON a.FBAckID = FBC.AcknowledgeID
            INNER JOIN {TableNames.FabricBookingAcknowledge} FBA ON FBA.BookingID = a.BookingID
            LEFT JOIN {DbNames.EPYSL}..BookingMaster BM ON BM.BookingID = FBA.BookingID
            LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster SBM ON SBM.BookingID = FBA.BookingID
            LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} b ON b.BookingID = a.BookingID
            LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = a.BuyerID
            LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = a.BuyerTeamID
            LEFT JOIN {DbNames.EPYSL}..CompanyEntity C ON C.CompanyID = b.CompanyID
            LEFT JOIN {DbNames.EPYSL}..Contacts CCS ON CCS.ContactID = YBCI.SpinnerID
            LEFT JOIN {DbNames.EPYSL}..ExportOrderMaster EOM ON EOM.ExportOrderID = a.ExportOrderID
            Left Join {DbNames.EPYSL}..ItemMaster IM On IM.ItemMasterID = YBC.ItemMasterID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
            Left Join {DbNames.EPYSL}..ItemSegmentValue SV3 On SV3.SegmentValueID = IM.Segment3ValueID
            Left Join {DbNames.EPYSL}..ItemSegmentValue SV4 On SV4.SegmentValueID = IM.Segment4ValueID
            Left Join {DbNames.EPYSL}..ItemSegmentValue SV5 On SV5.SegmentValueID = IM.Segment5ValueID

            Left Join {DbNames.EPYSL}..ItemMaster IMYarnCount On IMYarnCount.ItemMasterID = YBCI.YItemMasterID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IMYarnCount.Segment3ValueID
            Left Join {DbNames.EPYSL}..ItemSegmentValue SV6 On SV6.SegmentValueID = IMYarnCount.Segment6ValueID

            LEFT JOIN {DbNames.EPYSL}..StyleMaster SM ON SM.StyleMasterID = EOM.StyleMasterID
            LEFT JOIN {DbNames.EPYSL}..ContactSeason CS ON CS.SeasonID = SM.SeasonID";

            string whereQueryBasic = $@"
                    WHERE
                    YB.Acknowledge = 1
	                AND ISNULL(CQ.TotalAllocatedQty,0) < ISNULL(YBCI.NetYarnReqQty,0)
                    AND YB.DateAdded >= (Select CAST(ValueName AS DATE) From CutoffDate)";

            switch (searchFieldNameId)
            {
                case 1:
                    orderBy = paginationInfo.OrderBy.NullOrEmpty() ? " ORDER BY BuyerName" : paginationInfo.OrderBy;

                    sql = $@"
                    {withQuery}
                    ,FBA AS (
	                    SELECT 
	                     BuyerId = a.BuyerID
	                    ,BuyerName = CTO.ShortName
                        ,CountItem = COUNT(DISTINCT YBCI.YBChildItemID)
	
	                    {fromQuery}
                        {whereQueryBasic}

	                    GROUP BY a.BuyerID,CTO.ShortName
                    )
                    SELECT *,Count(*) Over() FROM FBA";

                    break;

                case 2:
                    orderBy = paginationInfo.OrderBy.NullOrEmpty() ? " ORDER BY BuyerTeamName" : paginationInfo.OrderBy;

                    sql = $@"
                    {withQuery}
                    ,FBA AS (
	                    SELECT 
	                     BuyerTeamId = a.BuyerTeamID
	                    ,BuyerTeamName = CCT.TeamName
    
	                    {fromQuery}
                        {whereQueryBasic}

                        {selectedBuyerIds}

	                    GROUP BY a.BuyerTeamID,CCT.TeamName
                    )
                    SELECT *,Count(*) Over() FROM FBA";

                    break;

                case 3:
                    orderBy = paginationInfo.OrderBy.NullOrEmpty() ? " ORDER BY NumericCount" : paginationInfo.OrderBy;

                    sql = $@"
                    {withQuery}
                    ,FBA AS (
	                    SELECT 
	                     CountId = IMYarnCount.Segment6ValueID
	                    ,NumericCount = SV6.SegmentValue
    
	                    {fromQuery}
                        {whereQueryBasic}

                        {selectedBuyerIds}
                        {selectedBuyerTeamIds}
                        {selectedYBookingIds}
                        {selectedYItemMasterIds}

	                    GROUP BY IMYarnCount.Segment6ValueID,SV6.SegmentValue
                    )
                    SELECT *,Count(*) Over() FROM FBA";

                    break;

                case 4:
                    orderBy = paginationInfo.OrderBy.NullOrEmpty() ? " ORDER BY YBookingNo" : paginationInfo.OrderBy;

                    sql = $@"
                    {withQuery}
                    ,FBA AS (
	                    SELECT 
	                     YBookingId = YBC.YBookingID
	                    ,YB.YBookingNo
    
                        {fromQuery}
                        {whereQueryBasic}

                        {selectedBuyerIds}
                        {selectedBuyerTeamIds}
                        {selectedCountIds}
                        {selectedYItemMasterIds}

	                    GROUP BY YBC.YBookingID,YB.YBookingNo
                    )
                    SELECT *,Count(*) Over() FROM FBA";

                    break;

                case 5:
                    orderBy = paginationInfo.OrderBy.NullOrEmpty() ? " ORDER BY YarnCategory" : paginationInfo.OrderBy;

                    sql = $@"
                    {withQuery}
                    ,FBA AS (
	                    SELECT 
	                     YBCI.YItemMasterID
	                    ,YBCI.YarnCategory
    
                        {fromQuery}
                        {whereQueryBasic}

                        {selectedBuyerIds}
                        {selectedBuyerTeamIds}
                        {selectedCountIds}
                        {selectedYBookingIds}

	                    GROUP BY YBCI.YItemMasterID,YBCI.YarnCategory
                    )
                    SELECT *,Count(*) Over() FROM FBA";

                    break;

                case 6:
                    orderBy = paginationInfo.OrderBy.NullOrEmpty() ? " ORDER BY FabricShade" : paginationInfo.OrderBy;

                    sql = $@"
                    {withQuery}
                    ,FBA AS (
	                    SELECT 
                         FabricShadeId = CASE WHEN YB.SubGroupID = 1 THEN SV3.SegmentValueID ELSE SV5.SegmentValueID END
                        ,FabricShade = CASE WHEN YB.SubGroupID = 1 THEN SV3.SegmentValue ELSE SV5.SegmentValue END 
    
	                    {fromQuery}
                        {whereQueryBasic}

                        {selectedBuyerIds}
                        {selectedBuyerTeamIds}
                        {selectedCountIds}
                        {selectedYBookingIds}
                        {selectedYItemMasterIds}

                        {selectedFabricTypeIds}
                        {selectedYarnTypeIds}
                        {selectedYarnRequisitionTypes}
                        {selectedFabricGSMIds}

	                    GROUP BY CASE WHEN YB.SubGroupID = 1 THEN SV3.SegmentValueID ELSE SV5.SegmentValueID END,
                        CASE WHEN YB.SubGroupID = 1 THEN SV3.SegmentValue ELSE SV5.SegmentValue END
                    )
                    SELECT *,Count(*) Over() FROM FBA";

                    break;

                case 7:
                    orderBy = paginationInfo.OrderBy.NullOrEmpty() ? " ORDER BY FabricType" : paginationInfo.OrderBy;

                    sql = $@"
                    {withQuery}
                    ,FBA AS (
	                    SELECT 
                         FabricTypeId = ISV1.SegmentValueID
                        ,FabricType = ISV1.SegmentValue
    
	                    {fromQuery}
                        {whereQueryBasic}

                        {selectedBuyerIds}
                        {selectedBuyerTeamIds}
                        {selectedCountIds}
                        {selectedYBookingIds}
                        {selectedYItemMasterIds}

                        {selectedFabricShadeIds}
                        {selectedYarnTypeIds}
                        {selectedYarnRequisitionTypes}
                        {selectedFabricGSMIds}

	                    GROUP BY ISV1.SegmentValueID,ISV1.SegmentValue
                    )
                    SELECT *,Count(*) Over() FROM FBA";

                    break;

                case 8:
                    orderBy = paginationInfo.OrderBy.NullOrEmpty() ? " ORDER BY YarnType" : paginationInfo.OrderBy;

                    sql = $@" 
                    {withQuery}
                    ,FBA AS (
	                    SELECT 
                         YarnTypeId = ISV3.SegmentValueID
                        ,YarnType = ISV3.SegmentValue
    
	                    {fromQuery}
                        {whereQueryBasic}

                        {selectedBuyerIds}
                        {selectedBuyerTeamIds}
                        {selectedCountIds}
                        {selectedYBookingIds}
                        {selectedYItemMasterIds}

                        {selectedFabricShadeIds}
                        {selectedFabricTypeIds}
                        {selectedYarnRequisitionTypes}
                        {selectedFabricGSMIds}

	                    GROUP BY ISV3.SegmentValueID,ISV3.SegmentValue
                    )
                    SELECT *,Count(*) Over() FROM FBA";

                    break;

                case 9:
                    orderBy = paginationInfo.OrderBy.NullOrEmpty() ? " ORDER BY YarnRequisitionType" : paginationInfo.OrderBy;

                    sql = $@" 
                    {withQuery}
                    ,FBA AS (
	                    SELECT 
                        YarnRequisitionType = case when YB.IsAddition=1 then 'Add-'+Convert(varchar(100),YB.AdditionNo) when YB.RevisionNo>0 then 'Rev-'+Convert(varchar(100),YB.RevisionNo) else 'Main' end
    
	                    {fromQuery}
                        {whereQueryBasic}

                        {selectedBuyerIds}
                        {selectedBuyerTeamIds}
                        {selectedCountIds}
                        {selectedYBookingIds}
                        {selectedYItemMasterIds}

                        {selectedFabricShadeIds}
                        {selectedFabricTypeIds}
                        {selectedYarnTypeIds}
                        {selectedFabricGSMIds}

	                    GROUP BY case when YB.IsAddition=1 then 'Add-'+Convert(varchar(100),YB.AdditionNo) when YB.RevisionNo>0 then 'Rev-'+Convert(varchar(100),YB.RevisionNo) else 'Main' end
                    )
                    SELECT *,Count(*) Over() FROM FBA";

                    break;

                case 10:
                    orderBy = paginationInfo.OrderBy.NullOrEmpty() ? " ORDER BY FabricGSM" : paginationInfo.OrderBy;

                    sql = $@" 
                    {withQuery}
                    ,FBA AS (
	                    SELECT 
                         FabricGSMId = CASE WHEN YB.SubGroupID = 1 THEN SV4.SegmentValueID ELSE 0 END
                        ,FabricGSM = CASE WHEN YB.SubGroupID = 1 THEN SV4.SegmentValue ELSE '' END
    
	                    {fromQuery}
                        {whereQueryBasic}

                        {selectedBuyerIds}
                        {selectedBuyerTeamIds}
                        {selectedCountIds}
                        {selectedYBookingIds}
                        {selectedYItemMasterIds}

                        {selectedFabricShadeIds}
                        {selectedFabricTypeIds}
                        {selectedYarnTypeIds}
                        {selectedYarnRequisitionTypes}

	                    GROUP BY CASE WHEN YB.SubGroupID = 1 THEN SV4.SegmentValueID ELSE 0 END,
                        CASE WHEN YB.SubGroupID = 1 THEN SV4.SegmentValue ELSE '' END
                    )
                    SELECT *,Count(*) Over() FROM FBA";

                    break;


                default:
                    // code block
                    break;
            }

            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<YarnAllocationChildItem>(sql);
        }



        private async Task<List<YarnAllocationChildItem>> GetEvents(string exportOrderIds)
        {
            if (exportOrderIds.IsNullOrEmpty()) return new List<YarnAllocationChildItem>();
            string sql = $@"
	            SELECT EOM.ExportOrderID, EOM.ExportOrderNo,OE.EventID, CDays = MIN(OE.CDays), TNADays = MAX(OE.TNADays)
	            ,EventDate = MIN(OE.EventDate)
                ,EventDescriptionId = CASE WHEN TE.EventDescription = 'Yarn booking' THEN 1
							  WHEN TE.EventDescription = 'Fabric Delivery start' THEN 2
							  WHEN TE.EventDescription = 'Fabric booking' THEN 3
							  WHEN TE.EventDescription = 'Yarn Allocation' THEN 4
							  WHEN TE.EventDescription = 'Fabric Delivery complete' THEN 5
						      ELSE 0 END
	            FROM {DbNames.EPYSL}..ExportOrderMaster EOM
	            INNER JOIN {DbNames.EPYSL}..OrderEventCalander OE On OE.ExportOrderID = EOM.ExportOrderID
	            INNER JOIN {DbNames.EPYSL}..TNAEvent TE On TE.EventID = OE.EventID
	            WHERE TE.EventDescription IN ('Yarn booking','Fabric Delivery start','Fabric booking','Yarn Allocation','Fabric Delivery complete') 
	            AND EOM.EWOStatusID <> 131
	            AND EOM.ExportOrderID IN ({exportOrderIds})
	            GROUP BY EOM.ExportOrderID, EOM.ExportOrderNo,OE.EventID,TE.EventDescription";
            List<YarnAllocationChildItem> list = await _service.GetDataAsync<YarnAllocationChildItem>(sql);
            return list;
        }
        private async Task<List<YarnAllocationChildItem>> GetStockInfo(string ybChildItemIDs)
        {
            if (ybChildItemIDs.IsNullOrEmpty()) return new List<YarnAllocationChildItem>();

            string sql = $@"
	            	SELECT YBCI.YBChildItemID, StockQty = SUM(AdvanceStockQty + SampleStockQty + LeftoverStockQty + LiabilitiesStockQty) 
	                FROM {TableNames.YarnBookingChildItem_New} YBCI
	                INNER JOIN {TableNames.YarnStockSet} YSS ON YSS.ItemMasterId = YBCI.YItemMasterID 
	                INNER JOIN {TableNames.YarnStockMaster} YSM ON YSM.YarnStockSetId = YSS.YarnStockSetId
	                WHERE YBCI.YBChildItemID IN ({ybChildItemIDs})
	                GROUP BY YBCI.YBChildItemID";
            List<YarnAllocationChildItem> list = await _service.GetDataAsync<YarnAllocationChildItem>(sql);
            return list;
        }

        public async Task<List<YarnAllocationChildItem>> GetAckPagedAsync(Status status, PaginationInfo paginationInfo)
        {
            bool isNeedImage = false;
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? " Order By AllocationChildItemID Desc" : paginationInfo.OrderBy;

            string sql = "";

            switch (status)
            {
                case Status.Approved:
                    isNeedImage = true;

                    sql = $@"
                            With 
                        CutoffDate As
						(
						SELECT ETV.ValueID, ETV.ValueName
						FROM {DbNames.EPYSL}..EntityType ET
						INNER JOIN {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.EntityTypeID = ET.EntityTypeID
						WHERE ET.EntityTypeName = 'YarnAllocationCutOffDate'
						),
                            FinalList AS
                            (
                                Select YACI.AllocationChildItemID,FBA.BookingNo,YBCI.YarnCategory, YSS.PhysicalCount, YSS.ShadeCode,
	                            PhysicalLot = YSS.YarnLotNo, YACI.TotalAllocationQty,
	                            Spinner = C.ShortName,
	                            AllocatedBy = E.EmployeeName,
	                            BuyerName = B.ShortName,
	                            FabricType = ISV1.SegmentValue,
	                            LotRef = YBCI.YarnLotNo,
	                            [Status] = Case When YACI.IsUnAckRevise = 1 Then 'Un Ack Revision No - '+ Convert(varchar(100), UnAckReviseNo) When ISNULL(YBM.RevisionNo,0) > 0 Then 'Yarn Booking Revision No - '+Convert(varchar(100), YBM.RevisionNo) Else 'Main' END,
                                YAM.YarnAllocationNo, YAM.ApproveDate, 
                                IsAllocationInternalRevise = 
								Case When YBM.IsAddition = 0 Then
									Case When FBA.IsAllocationInternalRevise = 1 then 1 When YBM.RevisionNo <> YACI.PreProcessRevNo then 1 Else 0 END
								Else
									Case When YBM.IsAllocationInternalRevise_Additional = 1 then 1 When YBM.RevisionNo <> YACI.PreProcessRevNo then 1 Else 0 END
								End,
	                            FabricColor = CASE WHEN FBC.SubGroupID = 1 THEN ISV3F.SegmentValue ELSE ISV5F.SegmentValue END,
	                            ReqCount = ISV6YBCI.SegmentValue,
	                            AllocatedYarnDetails = YACI.YarnCategory,
	                            AllocatedCount = ISV6YACI.SegmentValue
	                            FROM {TableNames.YARN_ALLOCATION_CHILD_ITEM} YACI
	                            INNER JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.AllocationChildID=YACI.AllocationChildID
	                            INNER JOIN {TableNames.YARN_ALLOCATION_MASTER} YAM ON YAM.YarnAllocationID=YAC.AllocationID
	                            INNER JOIN {TableNames.YarnStockSet} YSS ON YSS.YarnStockSetId = YACI.YarnStockSetId
	                            INNER JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBCI.YBChildItemID = YAC.YBChildItemID
	                            INNER JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID = YBCI.YBChildID
	                            INNER JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID = YBC.YBookingID
	                            INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FBC ON FBC.BookingChildID = YBC.BookingChildID AND FBC.BookingID = YBM.BookingID
	                            INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.FBAckID = FBC.AcknowledgeID

	                            INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YBC.ItemMasterID
	                            INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID

	                            INNER JOIN {DbNames.EPYSL}..ItemMaster IMF ON IMF.ItemMasterID = FBC.ItemMasterID
	                            INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3F ON ISV3F.SegmentValueID = IMF.Segment3ValueID
	                            INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5F ON ISV5F.SegmentValueID = IMF.Segment5ValueID

	                            INNER JOIN {DbNames.EPYSL}..ItemMaster IMYBCI ON IMYBCI.ItemMasterID = YBCI.YItemMasterID
	                            INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6YBCI ON ISV6YBCI.SegmentValueID = IMYBCI.Segment6ValueID

	                            INNER JOIN {DbNames.EPYSL}..ItemMaster IMYACI ON IMYACI.ItemMasterID = YSS.ItemMasterID
	                            INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6YACI ON ISV6YACI.SegmentValueID = IMYACI.Segment6ValueID

	                            INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = YSS.SpinnerId
	                            INNER JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = YBM.BuyerID
	                            Inner Join {DbNames.EPYSL}..LoginUser LU On YAM.AddedBy = LU.UserCode
                                Inner Join {DbNames.EPYSL}..Employee E On E.EmployeeCode = LU.EmployeeCode
                                WHERE YAM.Approve=1 AND YACI.Acknowledge = 0 AND YACI.UnAcknowledge = 0
                                AND YBM.YBookingDate >=(Select ValueName From CutoffDate)
                            )
                            SELECT *, Count(*) Over() TotalRows FROM FinalList  ";
                    break;
                case Status.Acknowledge:
                    isNeedImage = true;

                    sql = $@"
                            With 
                        CutoffDate As
						(
						SELECT ETV.ValueID, ETV.ValueName
						FROM {DbNames.EPYSL}..EntityType ET
						INNER JOIN {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.EntityTypeID = ET.EntityTypeID
						WHERE ET.EntityTypeName = 'YarnAllocationCutOffDate'
						),
                            FinalList AS
                            (
                                Select YACI.AllocationChildItemID,FBA.BookingNo,YBCI.YarnCategory, YSS.PhysicalCount, YSS.ShadeCode,
	                            PhysicalLot = YSS.YarnLotNo, YACI.TotalAllocationQty,
	                            Spinner = C.ShortName,
	                            AllocatedBy = E.EmployeeName,
	                            BuyerName = B.ShortName,
	                            FabricType = ISV1.SegmentValue,
	                            LotRef = YBCI.YarnLotNo,
	                            [Status] = Case When YACI.IsUnAckRevise = 1 Then 'Un Ack Revision No - '+ Convert(varchar(100), UnAckReviseNo) When ISNULL(YBM.RevisionNo,0) > 0 Then 'Yarn Booking Revision No - '+Convert(varchar(100), YBM.RevisionNo) Else 'Main' END,
                                YAM.YarnAllocationNo, YAM.ApproveDate, 
                                IsAllocationInternalRevise = Case When FBA.IsAllocationInternalRevise = 1 then 1 When YBM.RevisionNo <> YACI.PreProcessRevNo then 1 Else 0 END,
	                            FabricColor = CASE WHEN FBC.SubGroupID = 1 THEN ISV3F.SegmentValue ELSE ISV5F.SegmentValue END,
	                            ReqCount = ISV6YBCI.SegmentValue,
	                            AllocatedYarnDetails = YACI.YarnCategory,
	                            AllocatedCount = ISV6YACI.SegmentValue
	                            FROM {TableNames.YARN_ALLOCATION_CHILD_ITEM} YACI
	                            INNER JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.AllocationChildID=YACI.AllocationChildID
	                            INNER JOIN {TableNames.YARN_ALLOCATION_MASTER} YAM ON YAM.YarnAllocationID=YAC.AllocationID
	                            INNER JOIN {TableNames.YarnStockSet} YSS ON YSS.YarnStockSetId = YACI.YarnStockSetId
	                            INNER JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBCI.YBChildItemID = YAC.YBChildItemID
	                            INNER JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID = YBCI.YBChildID
	                            INNER JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID = YBC.YBookingID
	                            INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FBC ON FBC.BookingChildID = YBC.BookingChildID AND FBC.BookingID = YBM.BookingID
	                            INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.FBAckID = FBC.AcknowledgeID

	                            INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YBC.ItemMasterID
	                            INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID

	                            INNER JOIN {DbNames.EPYSL}..ItemMaster IMF ON IMF.ItemMasterID = FBC.ItemMasterID
	                            INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3F ON ISV3F.SegmentValueID = IMF.Segment3ValueID
	                            INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5F ON ISV5F.SegmentValueID = IMF.Segment5ValueID

	                            INNER JOIN {DbNames.EPYSL}..ItemMaster IMYBCI ON IMYBCI.ItemMasterID = YBCI.YItemMasterID
	                            INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6YBCI ON ISV6YBCI.SegmentValueID = IMYBCI.Segment6ValueID

	                            INNER JOIN {DbNames.EPYSL}..ItemMaster IMYACI ON IMYACI.ItemMasterID = YSS.ItemMasterID
	                            INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6YACI ON ISV6YACI.SegmentValueID = IMYACI.Segment6ValueID

	                            INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = YSS.SpinnerId
	                            INNER JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = YBM.BuyerID
	                            Inner Join {DbNames.EPYSL}..LoginUser LU On YAM.AddedBy = LU.UserCode
                                Inner Join {DbNames.EPYSL}..Employee E On E.EmployeeCode = LU.EmployeeCode
                                WHERE YAM.Approve=1 AND YACI.Acknowledge = 1 AND YACI.UnAcknowledge = 0
                                    AND YBM.YBookingDate >=(Select ValueName From CutoffDate)
                            )
                            SELECT *, Count(*) Over() TotalRows FROM FinalList ";
                    break;
                case Status.UnAcknowledge:
                    isNeedImage = true;

                    sql = $@"
                            With 
                            CutoffDate As
						(
						SELECT ETV.ValueID, ETV.ValueName
						FROM {DbNames.EPYSL}..EntityType ET
						INNER JOIN {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.EntityTypeID = ET.EntityTypeID
						WHERE ET.EntityTypeName = 'YarnAllocationCutOffDate'
						),
                        FinalList AS
                            (
                                Select YACI.AllocationChildItemID,FBA.BookingNo,YBCI.YarnCategory, YSS.PhysicalCount, YSS.ShadeCode,
	                            PhysicalLot = YSS.YarnLotNo, YACI.TotalAllocationQty,
	                            Spinner = C.ShortName,
	                            AllocatedBy = E.EmployeeName,
	                            BuyerName = B.ShortName,
	                            FabricType = ISV1.SegmentValue,
	                            LotRef = YBCI.YarnLotNo,
	                            [Status] = Case When YACI.IsUnAckRevise = 1 Then 'Un Ack Revision No - '+ Convert(varchar(100), UnAckReviseNo) When ISNULL(YBM.RevisionNo,0) > 0 Then 'Yarn Booking Revision No - '+Convert(varchar(100), YBM.RevisionNo) Else 'Main' END,
                                YAM.YarnAllocationNo, YAM.ApproveDate, 
                                IsAllocationInternalRevise = Case When FBA.IsAllocationInternalRevise = 1 then 1 When YBM.RevisionNo <> YACI.PreProcessRevNo then 1 Else 0 END,
	                            FabricColor = CASE WHEN FBC.SubGroupID = 1 THEN ISV3F.SegmentValue ELSE ISV5F.SegmentValue END,
	                            ReqCount = ISV6YBCI.SegmentValue,
	                            AllocatedYarnDetails = YACI.YarnCategory,
	                            AllocatedCount = ISV6YACI.SegmentValue
	                            FROM {TableNames.YARN_ALLOCATION_CHILD_ITEM} YACI
	                            INNER JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.AllocationChildID=YACI.AllocationChildID
	                            INNER JOIN {TableNames.YARN_ALLOCATION_MASTER} YAM ON YAM.YarnAllocationID=YAC.AllocationID
	                            INNER JOIN {TableNames.YarnStockSet} YSS ON YSS.YarnStockSetId = YACI.YarnStockSetId
	                            INNER JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBCI.YBChildItemID = YAC.YBChildItemID
	                            INNER JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID = YBCI.YBChildID
	                            INNER JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID = YBC.YBookingID
	                            INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FBC ON FBC.BookingChildID = YBC.BookingChildID AND FBC.BookingID = YBM.BookingID
	                            INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.FBAckID = FBC.AcknowledgeID

	                            INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YBC.ItemMasterID
	                            INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID

	                            INNER JOIN {DbNames.EPYSL}..ItemMaster IMF ON IMF.ItemMasterID = FBC.ItemMasterID
	                            INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3F ON ISV3F.SegmentValueID = IMF.Segment3ValueID
	                            INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5F ON ISV5F.SegmentValueID = IMF.Segment5ValueID

	                            INNER JOIN {DbNames.EPYSL}..ItemMaster IMYBCI ON IMYBCI.ItemMasterID = YBCI.YItemMasterID
	                            INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6YBCI ON ISV6YBCI.SegmentValueID = IMYBCI.Segment6ValueID

	                            INNER JOIN {DbNames.EPYSL}..ItemMaster IMYACI ON IMYACI.ItemMasterID = YSS.ItemMasterID
	                            INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6YACI ON ISV6YACI.SegmentValueID = IMYACI.Segment6ValueID

	                            INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = YSS.SpinnerId
	                            INNER JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = YBM.BuyerID
	                            Inner Join {DbNames.EPYSL}..LoginUser LU On YAM.AddedBy = LU.UserCode
                                Inner Join {DbNames.EPYSL}..Employee E On E.EmployeeCode = LU.EmployeeCode
                                WHERE YAM.Approve=1 AND YACI.Acknowledge = 0 AND YACI.UnAcknowledge = 1
                                    AND YBM.YBookingDate >=(Select ValueName From CutoffDate)
                            )
                            SELECT *, Count(*) Over() TotalRows FROM FinalList ";
                    break;

                default: //Status.Pending:
                    sql = $@"";
                    break;
            }
            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            var objList = await _service.GetDataAsync<YarnAllocationChildItem>(sql);
            if (isNeedImage)
            {
                string bookingNos = string.Join("','", objList.Select(x => x.BookingNo).Distinct());
                if (bookingNos.IsNotNullOrEmpty())
                {
                    var fBookingAcknowledgeImages = await _service.GetDataAsync<YarnAllocationChildItem>(CommonQueries.GetImagePathQuery(bookingNos));
                    fBookingAcknowledgeImages.ForEach(x =>
                    {
                        var obj = objList.Find(y => y.BookingNo == x.BookingNo);
                        if (obj.IsNotNull())
                        {
                            objList.Find(y => y.BookingNo == x.BookingNo).ImagePath = x.ImagePath;
                        }
                    });
                }
            }
            return objList;
        }
        public async Task<List<YarnAllocationMaster>> GetPendingBookingAsync(string buyerIds, PaginationInfo paginationInfo)
        {
            buyerIds = buyerIds.IsNotNullOrEmpty() ? $@" AND YB.BuyerID IN ({buyerIds})" : "";

            string sql = $@"WITH FBA AS
                        (
                            SELECT YB.YBookingNo, BuyerName = CTO.ShortName, BuyerTeamName = CCT.TeamName,CS.SeasonName
		
                            FROM {TableNames.YarnBookingChildItem_New} YBCI
	                        INNER JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID = YBCI.YBChildID
	                        INNER JOIN {TableNames.YarnBookingMaster_New} YB ON YB.YBookingID = YBC.YBookingID
	                        INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} a ON a.BookingID = YB.BookingID
	                        INNER JOIN {TableNames.FabricBookingAcknowledge} FBA ON FBA.BookingID = a.BookingID
	                        LEFT JOIN {DbNames.EPYSL}..BookingMaster BM ON BM.BookingID = FBA.BookingID
                            LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster SBM ON SBM.BookingID = FBA.BookingID
	                        LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} b ON b.BookingID = a.BookingID
	                        LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = a.BuyerID
	                        LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = a.BuyerTeamID
	                        LEFT JOIN {DbNames.EPYSL}..CompanyEntity C ON C.CompanyID = b.CompanyID
	                        LEFT JOIN {DbNames.EPYSL}..ContactSeason CS ON CS.SeasonID = a.SeasonID
	                        LEFT JOIN {DbNames.EPYSL}..Contacts CCS ON CCS.ContactID = YBCI.SpinnerID

	                        INNER JOIN {DbNames.EPYSL}..ItemMaster IM2 ON IM2.ItemMasterID = YBC.ItemMasterID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM2.Segment1ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM2.Segment2ValueID
	                        Left Join {DbNames.EPYSL}..ItemSegmentValue ISV3 On ISV3.SegmentValueID = IM2.Segment3ValueID
	                        Left Join {DbNames.EPYSL}..ItemSegmentValue ISV4 On ISV4.SegmentValueID = IM2.Segment4ValueID
	                        Left Join {DbNames.EPYSL}..ItemSegmentValue ISV5 On ISV5.SegmentValueID = IM2.Segment5ValueID
                            WHERE a.IsUnAcknowledge = 0 
		                    AND a.IsKnittingComplete = 1 
		                    AND a.IsInternalRevise = 0
                            AND a.IsApprovedByAllowance = 1 
		                    AND a.IsCheckByKnittingHead = 1 
		                    AND a.IsApprovedByProdHead = 1 
		                    AND a.IsApprovedByPMC = 1
                            AND a.IsRejectByAllowance = 0 
		                    AND a.IsRejectByKnittingHead = 0 
		                    AND a.IsRejectByProdHead = 0 
		                    AND a.IsRejectByPMC = 0
                            AND a.ExportOrderID > 0
	                        AND a.PreRevisionNo = FBA.RevisionNo
	                        AND YB.Acknowledge = 1
                            {buyerIds}
                            
	                        GROUP BY YB.YBookingNo,CTO.ShortName,CCT.TeamName,CS.SeasonName
                        )
                        SELECT *,Count(*) Over() TotalRows FROM FBA";

            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? " Order By YBookingNo Desc" : paginationInfo.OrderBy;

            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<YarnAllocationMaster>(sql);
        }
        public async Task<List<YarnAllocationMaster>> GetYarnBookingWiseChildAsync(string BookingNos)
        {
            string sql = "";


            sql = $@"WITH FBA AS
                     (
	                    SELECT YBCI.YBChildItemID, YBC.YBChildID, YB.YBookingNo, BuyerName = CTO.ShortName, BuyerTeamName = CCT.TeamName,
	                    CS.SeasonName, YB.YRequiredDate, 
	                    InHouseDate = CASE WHEN a.IsSample = 1 THEN SBM.InHouseDate ELSE BM.InHouseDate END,
	                    YBCI.YarnPly, NumericCount = 0,
	                    YBCI.YarnReqQty, YarnStockQty = 0, YBCI.YD, YarnCertification = '', BookingStatus = ''
	                    ,FabricQty = a.BookingQty
	                    ,FabricType = ISV1.SegmentValue
	                    ,FabricShade = CASE WHEN YB.SubGroupID = 1 THEN ISV3.SegmentValue ELSE ISV5.SegmentValue END
	                    ,YBCI.YarnCategory
	                    ,YarnLotNo = YBCI.YarnLotNo
	                    ,SpinnerName = CASE WHEN YBCI.SpinnerID > 0 THEN CCS.ShortName ELSE '' END
		
                        FROM {TableNames.YarnBookingChildItem_New} YBCI
	                    INNER JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID = YBCI.YBChildID
	                    INNER JOIN {TableNames.YarnBookingMaster_New} YB ON YB.YBookingID = YBC.YBookingID
	                    INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} a ON a.BookingID = YB.BookingID
	                    INNER JOIN {TableNames.FabricBookingAcknowledge} FBA ON FBA.BookingID = a.BookingID
	                    LEFT JOIN {DbNames.EPYSL}..BookingMaster BM ON BM.BookingID = FBA.BookingID
                        LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster SBM ON SBM.BookingID = FBA.BookingID
	                    LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} b ON b.BookingID = a.BookingID
	                    LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = a.BuyerID
	                    LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = a.BuyerTeamID
	                    LEFT JOIN {DbNames.EPYSL}..CompanyEntity C ON C.CompanyID = b.CompanyID
	                    LEFT JOIN {DbNames.EPYSL}..ContactSeason CS ON CS.SeasonID = a.SeasonID
	                    LEFT JOIN {DbNames.EPYSL}..Contacts CCS ON CCS.ContactID = YBCI.SpinnerID

	                    INNER JOIN {DbNames.EPYSL}..ItemMaster IM2 ON IM2.ItemMasterID = YBC.ItemMasterID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM2.Segment1ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM2.Segment2ValueID
	                    Left Join {DbNames.EPYSL}..ItemSegmentValue ISV3 On ISV3.SegmentValueID = IM2.Segment3ValueID
	                    Left Join {DbNames.EPYSL}..ItemSegmentValue ISV4 On ISV4.SegmentValueID = IM2.Segment4ValueID
	                    Left Join {DbNames.EPYSL}..ItemSegmentValue ISV5 On ISV5.SegmentValueID = IM2.Segment5ValueID

	                    WHERE a.IsUnAcknowledge = 0 AND a.IsKnittingComplete = 1 AND a.IsInternalRevise = 0
                        AND a.IsApprovedByAllowance = 1 AND a.IsCheckByKnittingHead = 1 AND a.IsApprovedByProdHead = 1 AND a.IsApprovedByPMC = 1
                        AND a.IsRejectByAllowance = 0 AND a.IsRejectByKnittingHead = 0 AND a.IsRejectByProdHead = 0 AND a.IsRejectByPMC = 0
                        AND a.ExportOrderID > 0
	                    AND a.PreRevisionNo = FBA.RevisionNo
	                    AND YB.Acknowledge = 1
                        AND YB.YBookingNo in({BookingNos})
	                    GROUP BY YBCI.YBChildItemID, YBC.YBChildID, YB.YBookingNo, CTO.ShortName, CCT.TeamName,
	                    CS.SeasonName, YB.YRequiredDate, 
	                    CASE WHEN a.IsSample = 1 THEN SBM.InHouseDate ELSE BM.InHouseDate END,
	                    YBCI.YarnPly,
	                    YBCI.YarnReqQty,YBCI.YD
	                    ,a.BookingQty
	                    ,ISV1.SegmentValue
	                    ,CASE WHEN YB.SubGroupID = 1 THEN ISV3.SegmentValue ELSE ISV5.SegmentValue END
	                    ,YBCI.YarnCategory
	                    ,YBCI.YarnLotNo
	                    ,CASE WHEN YBCI.SpinnerID > 0 THEN CCS.ShortName ELSE '' END
                    )
                    SELECT *,Count(*) Over() TotalRows FROM FBA";


            return await _service.GetDataAsync<YarnAllocationMaster>(sql);
        }
        public async Task<YarnAllocationMaster> GetNewAsync(string ybChildItemID)
        {
            var sql = $@"
                    SELECT YBM.*
                    FROM {TableNames.YarnBookingMaster_New} YBM
                    INNER JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBookingID = YBM.YBookingID
                    INNER JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBCI.YBChildID=YBC.YBChildID
                    WHERE YBCI.YBChildItemID IN ({ybChildItemID});

                    SELECT FBA.BookingID, YBCI.YBChildItemID, YBC.YBChildID, YBM.YBookingID,YBM.YBookingNo,
                    BuyerName = CTO.ShortName, BuyerTeamName = CCT.TeamName, YBCI.YarnCategory,YBCI.YItemMasterID,YBCI.UnitID,
                    SpinnerName = CASE WHEN YBCI.SpinnerID > 0 THEN CCS.ShortName ELSE '' END,
                    YBCI.YarnReqQty,YBCI.YD, LotNo = YBCI.YarnLotNo,YBCI.Allowance,YBCI.YDAllowance,
                    Stock.StockQty, Allocated.AllocatedQty, YarnUtilizationQty = YBCI.GreyYarnUtilizationQty, 
                    AdvanceStockAllocationQty = 0, PipelineStockAllocationQty = 0, Allocated.PlannedPOQty,Allocated.PlannedPipelineQty, AllocationNo = '',
                    InhouseStartDate = GETDATE(), InhouseEndDate = GETDATE(), Remarks = '',YBCI.YItemMasterID AS ItemMasterID,
                    YBCI.NetYarnReqQty, YarnReqDate = TYB.EventDate, PendingAllocationQty = ISNULL(YBCI.NetYarnReqQty,0) - ISNULL(Allocated.AllocatedQty,0),
                    BalancePOQty =  ISNULL(YBCI.NetYarnReqQty,0) - ISNULL(Allocated.AllocatedQty,0) - ISNULL(Allocated.PlannedPOQty,0) - ISNULL(Allocated.PlannedPipelineQty,0),
					--BalancePOQty Formula
					--BalancePOQty =  Net Yarn Req Qty - Allocated Qty - Allocation Qty - Pipeline Qty - PlannedPOQty - PlannedPipelineQty,
                    YBC.BookingChildID, YarnCount = ISV6.SegmentValue, FabricType = ISV1.SegmentValue, FabricShade = CASE WHEN YBM.SubGroupID = 1 THEN ISV3.SegmentValue ELSE ISV5.SegmentValue END
                    FROM {TableNames.YarnBookingChildItem_New} YBCI
                    INNER JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID = YBCI.YBChildID
                    INNER JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID = YBC.YBookingID
                    INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.BookingID = YBM.BookingID
                    INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YBCI.YItemMasterID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
					INNER JOIN {DbNames.EPYSL}..ItemMaster IM2 ON IM2.ItemMasterID = YBC.ItemMasterID
					LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM2.Segment1ValueID
					LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM2.Segment2ValueID
					Left Join {DbNames.EPYSL}..ItemSegmentValue ISV3 On ISV3.SegmentValueID = IM2.Segment3ValueID
					Left Join {DbNames.EPYSL}..ItemSegmentValue ISV4 On ISV4.SegmentValueID = IM2.Segment4ValueID
					Left Join {DbNames.EPYSL}..ItemSegmentValue ISV5 On ISV5.SegmentValueID = IM2.Segment5ValueID
                    LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = YBM.BuyerID
                    LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = YBM.BuyerTeamID
                    LEFT JOIN {DbNames.EPYSL}..Contacts CCS ON CCS.ContactID = YBCI.SpinnerID
					CROSS APPLY(
					Select StockQty = SUM(AdvanceStockQty + SampleStockQty + LeftoverStockQty + LiabilitiesStockQty) from YarnStockMaster YSC
					INNER JOIN {TableNames.YarnStockSet} YSS ON YSS.YarnStockSetId=YSC.YarnStockSetId
					--INNER JOIN StockType ST ON ST.StockTypeId=YSC.StockTypeId
					WHERE YSS.ItemMasterID=YBCI.YItemMasterID 
					--AND ST.StockTypeId in(3,5,6,7)-- ST.Name='Advance Stock' OR ST.Name='Sample Stock' OR ST.Name='Leftover Stock' OR ST.Name='Liabilities Stock'
					)AS Stock
					CROSS APPLY(
					Select 
					AllocatedQty = SUM(AdvanceStockAllocationQty),
					PlannedPOQty = SUM(QtyForPO),
					PlannedPipelineQty = SUM(PipelineStockAllocationQty) 
					FROM {TableNames.YARN_ALLOCATION_CHILD} WHERE YBChildItemID=YBCI.YBChildItemID
					)Allocated
                    OUTER APPLY (
		                Select EventDate = Min(Convert(Date,OE.EventDate))
		                From {DbNames.EPYSL}..TNAEvent TE
		                Inner Join {DbNames.EPYSL}..OrderEventCalander OE On OE.EventID = TE.EventID
		                Where OE.ExportOrderID=YBM.ExportOrderID AND TE.EventID = 29
					)TYB
                    WHERE YBCI.YBChildItemID IN ({ybChildItemID})

                    SELECT YBM.YBookingNo, BuyerName = CTO.ShortName, BuyerTeamName = CCT.TeamName
                    FROM {TableNames.YarnBookingChildItem_New} YBCI
                    INNER JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID = YBCI.YBChildID
                    INNER JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID = YBC.YBookingID
                    INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.BookingID = YBM.BookingID
                    LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = YBM.BuyerID
                    LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = YBM.BuyerTeamID
                    LEFT JOIN {DbNames.EPYSL}..Contacts CCS ON CCS.ContactID = YBCI.SpinnerID
                    WHERE YBCI.YBChildItemID IN ({ybChildItemID})
                    GROUP BY YBM.YBookingNo, CTO.ShortName, CCT.TeamName

                    ---BulkBookingGreyYarnUtilization
                    Select 
                    ItemMasterID = YSS.ItemMasterId, 
                    SpinnerID = YSS.SpinnerId, Spinner = SPIN.ShortName, PhysicalLot = YSS.YarnLotNo,
                    YSS.PhysicalCount, Composition = ISV1.SegmentValue, NumaricCount = ISV6.SegmentValue,
                    YarnDetails = YSS.YarnCategory,
                    YSM.SampleStockQty, YSM.LiabilitiesStockQty, YSM.UnusableStockQty, YSM.LeftoverStockQty,
                    GYU.*
                    From BulkBookingGreyYarnUtilization GYU
                    INNER JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBCI.YBChildItemID = GYU.YBChildItemID
                    INNER JOIN {TableNames.YarnBookingChild_New} C ON C.YBChildID = YBCI.YBChildID
                    INNER JOIN {TableNames.YarnBookingMaster_New} M ON M.YBookingID = C.YBookingID
                    INNER JOIN {TableNames.YarnStockSet} YSS ON YSS.YarnStockSetId = GYU.YarnStockSetID
                    INNER JOIN {TableNames.YarnStockMaster} YSM ON YSM.YarnStockSetId = YSS.YarnStockSetId
                    INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YSS.ItemMasterId
                    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
                    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                    LEFT JOIN {DbNames.EPYSL}..Contacts SPIN On SPIN.ContactID = YSS.SpinnerId
                    Where YBCI.YBChildItemID IN ({ybChildItemID});";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YarnAllocationMaster data = await records.ReadFirstOrDefaultAsync<YarnAllocationMaster>();
                data.Childs = records.Read<YarnAllocationChild>().ToList();
                data.SummaryChilds = records.Read<YarnAllocationChild>().ToList();
                List<BulkBookingGreyYarnUtilization> gGreyYarnUtilizationList = records.Read<BulkBookingGreyYarnUtilization>().ToList();
                data.Childs.ForEach(CI =>
                {
                    CI.GreyYarnUtilizationPopUpList = gGreyYarnUtilizationList.Where(CItem => CItem.YBChildItemID == CI.YBChildItemID).ToList();
                    CI.BalancePOQty = CI.BalancePOQty < 0 ? 0 : CI.BalancePOQty;
                });
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

        public async Task<YarnAllocationMaster> GetAsync(int id)
        {
            var sql = $@"
                    SELECT * 
                    FROM {TableNames.YARN_ALLOCATION_MASTER} 
                    WHERE YarnAllocationID = {id};

                    
                    --Childs
                    WITH YAChild AS
                    (
	                    SELECT YAC.*,
	                    YAM.YarnAllocationID,FBA.BookingID, YBC.YBChildID, YBM.YBookingID,
	                    BuyerName = CTO.ShortName, BuyerTeamName = CCT.TeamName, YBCI.YarnCategory,
	                    SpinnerName = CASE WHEN YBCI.SpinnerID > 0 THEN CCS.ShortName ELSE '' END,
	                    YBCI.YarnReqQty,YBCI.YD, Stock.CompanyId,YBCI.Allowance,YBCI.YDAllowance,
	                    Stock.StockQty, --AllocatedQty = YAC.AdvanceStockAllocationQty + YAC.PipelineStockAllocationQty,
	                    YarnUtilizationQty = 0, 
	                    AllocationQty = 0, 
	                    PipeLineQty = 0, 
	                    AllocationNo = YAM.YarnAllocationNo,
                        YBCI.NetYarnReqQty, YarnCount = ISV.SegmentValue,
						FabricType = ISV1.SegmentValue, FabricShade = CASE WHEN YBM.SubGroupID = 1 THEN ISV3.SegmentValue ELSE ISV5.SegmentValue END
	                    FROM {TableNames.YARN_ALLOCATION_CHILD} YAC
	                    INNER JOIN {TableNames.YARN_ALLOCATION_MASTER} YAM ON YAM.YarnAllocationID = YAC.AllocationID
	                    INNER JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBCI.YBChildItemID = YAC.YBChildItemID
	                    INNER JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID = YBCI.YBChildID
	                    INNER JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID = YBC.YBookingID
	                    INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.BookingID = YBM.BookingID
                        INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YBCI.YItemMasterID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = IM.Segment6ValueID
						INNER JOIN {DbNames.EPYSL}..ItemMaster IM2 ON IM2.ItemMasterID = YBC.ItemMasterID
						LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM2.Segment1ValueID
						LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM2.Segment2ValueID
						Left Join {DbNames.EPYSL}..ItemSegmentValue ISV3 On ISV3.SegmentValueID = IM2.Segment3ValueID
						Left Join {DbNames.EPYSL}..ItemSegmentValue ISV4 On ISV4.SegmentValueID = IM2.Segment4ValueID
						Left Join {DbNames.EPYSL}..ItemSegmentValue ISV5 On ISV5.SegmentValueID = IM2.Segment5ValueID
	                    LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = YBM.BuyerID
	                    LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = YBM.BuyerTeamID
	                    LEFT JOIN {DbNames.EPYSL}..Contacts CCS ON CCS.ContactID = YBCI.SpinnerID
	                    CROSS APPLY(
		                    Select SUM(YSC.Qty)StockQty, CompanyId = MAX(YSC.CompanyId), YarnCategory = MAX(YSS.YarnCategory) 
		                    from YarnStockChild YSC
		                    INNER JOIN {TableNames.YarnStockSet} YSS ON YSS.YarnStockSetId = YSC.YarnStockSetId
		                    INNER JOIN StockType ST ON ST.StockTypeId=YSC.StockTypeId
		                    WHERE YSS.ItemMasterID = YBCI.YItemMasterID 
		                    AND ST.StockTypeId in(3,5,6,7) -- ST.Name='Advance Stock' OR ST.Name='Sample Stock' OR ST.Name='Leftover Stock' OR ST.Name='Liabilities Stock'
	                    )AS Stock
	                    WHERE YAM.YarnAllocationID = {id}
                    ),
                    YBChildItemWiseQty AS
                    (
	                    SELECT YBCI.YBChildItemID, AllocatedQty = SUM(ISNULL(YAC.AdvanceStockAllocationQty,0)),
                        PlannedPipelineQty = SUM(ISNULL(YAC.PipelineStockAllocationQty,0)),
						PlannedPOQty = SUM(ISNULL(YAC.QtyForPO,0))
	                    FROM {TableNames.YarnBookingChildItem_New} YBCI
	                    INNER JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.YBChildItemID = YBCI.YBChildItemID
	                    --WHERE YAC.AllocationChildID NOT IN (SELECT A.AllocationChildID FROM YAChild A)
	                    GROUP BY YBCI.YBChildItemID
                    ),
                    BN AS
                    (
	                    SELECT YAC.AllocationChildID, FBA.BookingNo
	                    FROM YAChild YAC
	                    INNER JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBCI.YBChildItemID = YAC.YBChildItemID
	                    INNER JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID = YBCI.YBChildID
	                    INNER JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID = YBC.YBookingID
	                    INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.BookingID = YBM.BookingID
	                    GROUP BY YAC.AllocationChildID, FBA.BookingNo
                    ),
                    YBCIS AS(
					    Select  distinct YAC.YBChildItemID FROM {TableNames.YARN_ALLOCATION_CHILD} YAC
	                    INNER JOIN {TableNames.YARN_ALLOCATION_MASTER} YAM ON YAM.YarnAllocationID = YAC.AllocationID
					    INNER JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBCI.YBChildItemID = YAC.YBChildItemID
					    LEFT JOIN {TableNames.YarnBookingChildItem_New_Bk} YBCIB ON YBCIB.YBChildItemID = YAC.YBChildItemID
					    WHERE YAM.YarnAllocationID = {id}
                        and (ISNULL(YBCI.NetYarnReqQty,0) <> ISNULL(YBCIB.NetYarnReqQty,0) OR YBCI.YItemMasterID<>YBCIB.YItemMasterID)
					),
					AQ AS(
					    Select YAC.YBChildItemID, TotalAllocatedQty = SUM(YAC.TotalAllocationQty) 
					    FROM {TableNames.YARN_ALLOCATION_CHILD} YAC
					    INNER JOIN YBCIS ON YBCIS.YBChildItemID = YAC.YBChildItemID
	                    INNER JOIN {TableNames.YARN_ALLOCATION_MASTER} YAM ON YAM.YarnAllocationID = YAC.AllocationID
					    INNER JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBCI.YBChildItemID = YAC.YBChildItemID
					    GROUP BY YAC.YBChildItemID
					),
                    FL AS
                    (
	                    SELECT YAC.*,AllocatedQty = ISNULL(YBC.AllocatedQty,0),PlannedPipelineQty = ISNULL(YBC.PlannedPipelineQty,0), BN.BookingNo,
                        BalancePOQty =  ISNULL(NetYarnReqQty,0) - ISNULL(YBC.AllocatedQty,0) - ISNULL(YBC.PlannedPipelineQty,0) - ISNULL(YBC.PlannedPOQty,0),
						PendingAllocationQty = ISNULL(YAC.NetYarnReqQty,0) - ISNULL(AQ.TotalAllocatedQty,0)
	                    FROM YAChild YAC
	                    INNER JOIN BN ON BN.AllocationChildID = YAC.AllocationChildID
	                    LEFT JOIN YBChildItemWiseQty YBC ON YBC.YBChildItemID = YAC.YBChildItemID
						LEFT JOIN AQ ON AQ.YBChildItemID = YAC.YBChildItemID
                    )
                    SELECT * FROM FL;

                    --Revised Childs
                    WITH YBCIS AS(
					    Select  distinct YAC.YBChildItemID FROM {TableNames.YARN_ALLOCATION_CHILD} YAC
	                    INNER JOIN {TableNames.YARN_ALLOCATION_MASTER} YAM ON YAM.YarnAllocationID = YAC.AllocationID
					    INNER JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBCI.YBChildItemID = YAC.YBChildItemID
					    LEFT JOIN {TableNames.YarnBookingChildItem_New_Bk} YBCIB ON YBCIB.YBChildItemID = YAC.YBChildItemID
					    WHERE YAM.YarnAllocationID = {id} 
                        and (ISNULL(YBCI.NetYarnReqQty,0) <> ISNULL(YBCIB.NetYarnReqQty,0) OR YBCI.YItemMasterID<>YBCIB.YItemMasterID)
					),
					AQ AS(
					    Select YAC.YBChildItemID, TotalAllocatedQty = SUM(YAC.TotalAllocationQty) 
					    FROM {TableNames.YARN_ALLOCATION_CHILD} YAC
					    INNER JOIN YBCIS ON YBCIS.YBChildItemID = YAC.YBChildItemID
	                    INNER JOIN {TableNames.YARN_ALLOCATION_MASTER} YAM ON YAM.YarnAllocationID = YAC.AllocationID
					    INNER JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBCI.YBChildItemID = YAC.YBChildItemID
					    GROUP BY YAC.YBChildItemID
					),
					YAChild AS
                    (
	                    SELECT YAC.*,
	                    YAM.YarnAllocationID,FBA.BookingID, YBC.YBChildID, YBM.YBookingID,
	                    BuyerName = CTO.ShortName, BuyerTeamName = CCT.TeamName, YBCI.YarnCategory,
	                    SpinnerName = CASE WHEN YBCI.SpinnerID > 0 THEN CCS.ShortName ELSE '' END,
	                    YBCI.YarnReqQty,YBCI.YD, Stock.CompanyId,YBCI.Allowance,YBCI.YDAllowance,
	                    Stock.StockQty, --AllocatedQty = YAC.AdvanceStockAllocationQty + YAC.PipelineStockAllocationQty,
	                    YarnUtilizationQty = 0, 
	                    AllocationQty = 0, 
	                    PipeLineQty = 0, 
	                    AllocationNo = YAM.YarnAllocationNo,
                        YBCI.NetYarnReqQty, PrevNetYarnReqQty = ISNULL(YBCIB.NetYarnReqQty,0),
                        YarnCount = ISV.SegmentValue,
						FabricType = ISV1.SegmentValue, FabricShade = CASE WHEN YBM.SubGroupID = 1 THEN ISV3.SegmentValue ELSE ISV5.SegmentValue END
	                    FROM {TableNames.YARN_ALLOCATION_CHILD} YAC
	                    INNER JOIN {TableNames.YARN_ALLOCATION_MASTER} YAM ON YAM.YarnAllocationID = YAC.AllocationID
	                    INNER JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBCI.YBChildItemID = YAC.YBChildItemID
	                    INNER JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID = YBCI.YBChildID
	                    INNER JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID = YBC.YBookingID
                        LEFT JOIN {TableNames.YarnBookingChildItem_New_Bk} YBCIB ON YBCIB.YBChildItemID = YAC.YBChildItemID AND YBCIB.RevisionNo = YBM.RevisionNo - 1
	                    INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.BookingID = YBM.BookingID
                        INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YBCI.YItemMasterID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = IM.Segment6ValueID
						INNER JOIN {DbNames.EPYSL}..ItemMaster IM2 ON IM2.ItemMasterID = YBC.ItemMasterID
						LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM2.Segment1ValueID
						LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM2.Segment2ValueID
						Left Join {DbNames.EPYSL}..ItemSegmentValue ISV3 On ISV3.SegmentValueID = IM2.Segment3ValueID
						Left Join {DbNames.EPYSL}..ItemSegmentValue ISV4 On ISV4.SegmentValueID = IM2.Segment4ValueID
						Left Join {DbNames.EPYSL}..ItemSegmentValue ISV5 On ISV5.SegmentValueID = IM2.Segment5ValueID
	                    LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = YBM.BuyerID
	                    LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = YBM.BuyerTeamID
	                    LEFT JOIN {DbNames.EPYSL}..Contacts CCS ON CCS.ContactID = YBCI.SpinnerID
	                    CROSS APPLY(
		                    Select SUM(YSC.Qty)StockQty, CompanyId = MAX(YSC.CompanyId), YarnCategory = MAX(YSS.YarnCategory) 
		                    from YarnStockChild YSC
		                    INNER JOIN {TableNames.YarnStockSet} YSS ON YSS.YarnStockSetId = YSC.YarnStockSetId
		                    INNER JOIN StockType ST ON ST.StockTypeId=YSC.StockTypeId
		                    WHERE YSS.ItemMasterID = YBCI.YItemMasterID 
		                    AND ST.StockTypeId in(3,5,6,7) -- ST.Name='Advance Stock' OR ST.Name='Sample Stock' OR ST.Name='Leftover Stock' OR ST.Name='Liabilities Stock'
	                    )AS Stock
	                    WHERE YAM.YarnAllocationID = {id} 
                        and (ISNULL(YBCI.NetYarnReqQty,0) <> ISNULL(YBCIB.NetYarnReqQty,0) OR YBCI.YItemMasterID<>YBCIB.YItemMasterID)
                    ),
                    YBChildItemWiseQty AS
                    (
	                    SELECT YBCI.YBChildItemID, AllocatedQty = SUM(ISNULL(YAC.AdvanceStockAllocationQty,0)),PlannedPipelineQty = SUM(ISNULL(YAC.PipelineStockAllocationQty,0))
	                    FROM {TableNames.YarnBookingChildItem_New} YBCI
	                    INNER JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.YBChildItemID = YBCI.YBChildItemID
	                    --WHERE YAC.AllocationChildID NOT IN (SELECT A.AllocationChildID FROM YAChild A)
	                    GROUP BY YBCI.YBChildItemID
                    ),
                    BN AS
                    (
	                    SELECT YAC.AllocationChildID, FBA.BookingNo
	                    FROM YAChild YAC
	                    INNER JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBCI.YBChildItemID = YAC.YBChildItemID
	                    INNER JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID = YBCI.YBChildID
	                    INNER JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID = YBC.YBookingID
	                    INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.BookingID = YBM.BookingID
	                    GROUP BY YAC.AllocationChildID, FBA.BookingNo
                    ),
                    FL AS
                    (
	                    SELECT distinct YAC.*,AllocatedQty = ISNULL(YBC.AllocatedQty,0),PlannedPipelineQty = ISNULL(YBC.PlannedPipelineQty,0), BN.BookingNo, AQ.TotalAllocatedQty,
						PendingAllocationQty = ISNULL(YAC.NetYarnReqQty,0) - ISNULL(AQ.TotalAllocatedQty,0)
	                    FROM YAChild YAC
	                    INNER JOIN BN ON BN.AllocationChildID = YAC.AllocationChildID
	                    LEFT JOIN YBChildItemWiseQty YBC ON YBC.YBChildItemID = YAC.YBChildItemID
						LEFT JOIN AQ ON AQ.YBChildItemID = YAC.YBChildItemID
                    )
                    SELECT * FROM FL;
                    
                    WITH
                    YTD AS(
	                    Select MAX(YRM.QCRemarksMasterID) QCRemarksMasterID,YRMC.LotNo,YRMC.ItemMasterId,YRM.SupplierId,YRM.SpinnerId,YRMC.ShadeCode,YRMC.PhysicalCount,
	                    Rate = ISNULL(POC.Rate,0) 
	                    from YarnQCRemarksChild YRMC
	                    INNER JOIN YarnQCRemarksMaster YRM ON YRM.QCRemarksMasterID = YRMC.QCRemarksMasterID
	                    INNER JOIN YarnQCReceiveChild RC ON RC.QCReceiveChildID = YRMC.QCReceiveChildID
	                    INNER JOIN YarnQCIssueChild IC ON IC.QCIssueChildID = RC.QCIssueChildID
	                    INNER JOIN YarnQCReqChild RC1 ON RC1.QCReqChildID = IC.QCReqChildID
	                    INNER JOIN YarnReceiveChild YRC ON YRC.ChildID = RC1.ReceiveChildID
	                    LEFT JOIN YarnPOChild POC ON POC.YPOChildID = YRC.POChildID
	                    GROUP BY YRMC.LotNo,YRMC.ItemMasterId,YRM.SupplierId,YRM.SpinnerId,YRMC.ShadeCode,YRMC.PhysicalCount,ISNULL(POC.Rate,0)
                    ),
                    YT AS(
	                    Select CASE WHEN YRMC.Approve = 1 THEN 'Approve'
				                    WHEN YRMC.Reject = 1 THEN 'Reject'
				                    WHEN YRMC.Retest = 1 THEN 'Retest'
				                    WHEN YRMC.Diagnostic = 1 THEN 'Diagnostic'
				                    WHEN YRMC.CommerciallyApprove = 1 THEN 'CommerciallyApprove' 
				                    ELSE '' END Status,
	                    YRMC.Remarks,YRMC.LotNo,YRMC.ItemMasterId,YRM.SupplierId,YRM.SpinnerId,YRMC.ShadeCode,YRMC.PhysicalCount,
	                    Rate = YTD.Rate
	                    from YarnQCRemarksChild YRMC
	                    INNER JOIN YarnQCRemarksMaster YRM ON YRM.QCRemarksMasterID = YRMC.QCRemarksMasterID
	                    INNER JOIN YTD ON YTD.QCRemarksMasterID = YRMC.QCRemarksMasterID 
	                    INNER JOIN YarnQCReceiveChild RC ON RC.QCReceiveChildID = YRMC.QCReceiveChildID
	                    INNER JOIN YarnQCIssueChild IC ON IC.QCIssueChildID = RC.QCIssueChildID
	                    INNER JOIN YarnQCReqChild RC1 ON RC1.QCReqChildID = IC.QCReqChildID
	                    INNER JOIN YarnReceiveChild YRC ON YRC.ChildID = RC1.ReceiveChildID
	                    LEFT JOIN YarnPOChild POC ON POC.YPOChildID = YRC.POChildID
                    ),
                
                    StockInfo AS
                    (
	                   SELECT YACI.AllocationChildItemID, YACI.AllocationChildID, YSS.ItemMasterId, YSS.YarnStockSetId,YSS.YarnCategory
			                    ,NumericCount = ISV6.SegmentValue, Spinner = CCS.ShortName,
			                    YSS.PhysicalCount, PhysicalLot = YSS.YarnLotNo, POPrice = YT.Rate,
			                    YarnAge = DATEDIFF(DAY, MAX(YSS.YarnApprovedDate), GETDATE()),
			                    YT.Status TestResult,YT.Remarks TestResultComments, YACI.AdvanceAllocationQty, YACI.SampleAllocationQty, YACI.LiabilitiesAllocationQty, YACI.LeftoverAllocationQty, YACI.TotalAllocationQty,
								AdvanceStockQty = SUM(YSM.AdvanceStockQty) + ISNULL(YACI.AdvanceAllocationQty,0),
								SampleStockQty = SUM(YSM.SampleStockQty) + ISNULL(YACI.SampleAllocationQty,0),
								LeftoverStockQty = SUM(YSM.LeftoverStockQty) + ISNULL(YACI.LeftoverAllocationQty,0),		
								LiabilitiesStockQty = SUM(YSM.LiabilitiesStockQty) + ISNULL(YACI.LiabilitiesAllocationQty,0)								
						FROM {TableNames.YARN_ALLOCATION_CHILD_ITEM} YACI
                        INNER JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.AllocationChildID = YACI.AllocationChildID
                        INNER JOIN {TableNames.YARN_ALLOCATION_MASTER} YAM ON YAM.YarnAllocationID = YAC.AllocationID
					    INNER JOIN {TableNames.YarnStockSet} YSS ON  YSS.YarnStockSetId = YACI.YarnStockSetId
						INNER JOIN {TableNames.YarnStockMaster} YSM ON YSM.YarnStockSetId = YSS.YarnStockSetId
					    INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YSS.ItemMasterID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
					    INNER JOIN {DbNames.EPYSL}..Contacts CCS ON CCS.ContactID = YSS.SpinnerID
					    LEFT JOIN YT ON YT.LotNo = YSS.YarnLotNo 
	                                AND YT.ItemMasterId = YSS.ItemMasterId 
				                    AND YT.SupplierId = YSS.SupplierId 
				                    AND YT.ShadeCode = YSS.ShadeCode 
				                    AND YT.PhysicalCount = YSS.PhysicalCount
                        WHERE YAM.YarnAllocationID = {id}
					    GROUP BY YACI.AllocationChildItemID, YACI.AllocationChildID, YSS.ItemMasterId, YSS.YarnStockSetId,YSS.YarnCategory
			                    ,ISV6.SegmentValue, CCS.ShortName,
			                    YSS.PhysicalCount, YSS.YarnLotNo, YT.Rate,
			                    YT.Status,YT.Remarks, YACI.AdvanceAllocationQty, YACI.SampleAllocationQty, YACI.LiabilitiesAllocationQty, YACI.LeftoverAllocationQty, YACI.TotalAllocationQty
                    ),
                    FinalList AS (
	                    SELECT SI.*
	                    FROM StockInfo SI
	                    --Where SI.AdvanceStockQty>0 OR SI.SampleStockQty>0 OR SI.LeftoverStockQty>0 OR SI.LiabilitiesStockQty>0
                    )
                    SELECT * FROM FinalList;

                    SELECT YACI.* 
                    FROM {TableNames.YARN_ALLOCATION_CHILD}PipelineItem YACI
                    INNER JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.AllocationChildID = YACI.AllocationChildID
                    INNER JOIN {TableNames.YARN_ALLOCATION_MASTER} YAM ON YAM.YarnAllocationID = YAC.AllocationID
                    WHERE YAM.YarnAllocationID = {id};

                    SELECT YBM.YBookingNo, BuyerName = CTO.ShortName, BuyerTeamName = CCT.TeamName
                    FROM {TableNames.YARN_ALLOCATION_MASTER} YAM
					INNER JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.AllocationID=YAM.YarnAllocationID
					INNER JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBCI.YBChildItemID=YAC.YBChildItemID
                    INNER JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID = YBCI.YBChildID
                    INNER JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID = YBC.YBookingID
                    INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.BookingID = YBM.BookingID
                    LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = YBM.BuyerID
                    LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = YBM.BuyerTeamID
                    LEFT JOIN {DbNames.EPYSL}..Contacts CCS ON CCS.ContactID = YBCI.SpinnerID
                    WHERE YAM.YarnAllocationID={id}
                    GROUP BY YBM.YBookingNo, CTO.ShortName, CCT.TeamName";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);

                YarnAllocationMaster data = await records.ReadFirstOrDefaultAsync<YarnAllocationMaster>();
                data.Childs = records.Read<YarnAllocationChild>().ToList();
                data.RevisedChilds = records.Read<YarnAllocationChild>().ToList();
                var childItems = records.Read<YarnAllocationChildItem>().ToList();
                var childPLItems = records.Read<YarnAllocationChildPipelineItem>().ToList();

                data.Childs.ForEach(c =>
                {
                    c.ChildItems = childItems.Where(ci => ci.AllocationChildID == c.AllocationChildID).ToList();
                    c.ChildPipelineItems = childPLItems.Where(ci => ci.AllocationChildID == c.AllocationChildID).ToList();
                    c.BalancePOQty = c.BalancePOQty < 0 ? 0 : c.BalancePOQty;
                });
                data.RevisedChilds.ForEach(c =>
                {
                    c.ChildItems = childItems.Where(ci => ci.AllocationChildID == c.AllocationChildID).ToList();
                    c.ChildPipelineItems = childPLItems.Where(ci => ci.AllocationChildID == c.AllocationChildID).ToList();
                    c.BalancePOQty = c.BalancePOQty < 0 ? 0 : c.BalancePOQty;
                });
                data.SummaryChilds = records.Read<YarnAllocationChild>().ToList();
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
        public async Task<YarnAllocationMaster> GetAsync2(int allocationId)
        {
            var sql = $@"
                    --Master
                    SELECT YAM.YarnAllocationID,YAM.YarnAllocationID, YAM.YarnAllocationDate,YAM.YarnAllocationNo,YAM.ExportOrderID,YAM.Approve,YAM.ApproveBy,YAM.ApproveDate
						,YAM.Acknowledge,YAM.AcknowledgeBy,YAM.AcknowledgeDate,YAM.UnAcknowledge,YAM.UnAcknowledgeBy,YAM.UnAcknowledgeDate
						,YAM.UnAcknowledgeReason,YAM.AddedBy,YAM.UpdatedBy,YAM.BookingNo,YAM.BuyerID,YAM.BuyerTeamID,YAM.EWONo,YAM.FabricDeliveryDate
						,YAM.TNA,YAM.BAnalysisID,YAM.BAnalysisNo,YAM.Propose,YAM.ProposeBy,YAM.ProposeDate,YAM.Reject,YAM.RejectBy,YAM.RejectDate
						,YAM.RejectReason,YAM.DateAdded,YAM.DateUpdated,YAM.RevisionNo, ConceptNo = STRING_AGG(FCM.ConceptNo, ',')  
                    FROM {TableNames.YARN_ALLOCATION_MASTER} YAM
					INNER JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.AllocationID = YAM.YarnAllocationID
					INNER JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBCI.YBChildItemID = YAC.YBChildItemID
					INNER JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID = YBCI.YBChildID
					--Inner JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConsumptionID = YBC.ConsumptionID AND FCM.ItemMasterID = YBC.ItemMasterID
                    INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.BookingID = YBC.BookingID AND FCM.BookingChildID = YBC.BookingChildID
                    WHERE YarnAllocationID = {allocationId}
					GROUP BY YAM.YarnAllocationID,YAM.YarnAllocationID, YAM.YarnAllocationDate,YAM.YarnAllocationNo,YAM.ExportOrderID,YAM.Approve,YAM.ApproveBy,YAM.ApproveDate
						   ,YAM.Acknowledge,YAM.AcknowledgeBy,YAM.AcknowledgeDate,YAM.UnAcknowledge,YAM.UnAcknowledgeBy,YAM.UnAcknowledgeDate
						   ,YAM.UnAcknowledgeReason,YAM.AddedBy,YAM.UpdatedBy,YAM.BookingNo,YAM.BuyerID,YAM.BuyerTeamID,YAM.EWONo,YAM.FabricDeliveryDate
						   ,YAM.TNA,YAM.BAnalysisID,YAM.BAnalysisNo,YAM.Propose,YAM.ProposeBy,YAM.ProposeDate,YAM.Reject,YAM.RejectBy,YAM.RejectDate
						   ,YAM.RejectReason,YAM.DateAdded,YAM.DateUpdated,YAM.RevisionNo;

                    --Childs
                    SELECT YAC.*,FBA.BookingNo, YBCI.YarnCategory, YBM.CompanyID, YBCI.ShadeCode, FCMRC.FCMRChildID, FCMRM.ConceptID, YBCI.SpinnerId
                    FROM {TableNames.YARN_ALLOCATION_CHILD} YAC
					LEFT JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBCI.YBChildItemID = YAC.YBChildItemID
					LEFT JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID = YBCI.YBChildID
					LEFT JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID = YBC.YBookingID
					LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.BookingID = YBM.BookingID
					LEFT JOIN FreeConceptMRChild FCMRC ON FCMRC.YBChildItemID = YBCI.YBChildItemID
					LEFT JOIN FreeConceptMRMaster FCMRM ON FCMRM.FCMRMasterID = FCMRC.FCMRMasterID
                    WHERE YAC.AllocationID = {allocationId};

                    --ChildItems
                    SELECT YACI.*,NumericCount = ISV6.SegmentValue
                    FROM {TableNames.YARN_ALLOCATION_CHILD_ITEM} YACI
                    INNER JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.AllocationChildID = YACI.AllocationChildID
                    INNER JOIN {TableNames.YarnStockSet} YSS ON  YSS.YarnStockSetId = YACI.YarnStockSetId
					INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YSS.ItemMasterID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                    WHERE YAC.AllocationID = {allocationId};

                    --ChildItems Pipeline
                    SELECT YACI.*
                    FROM {TableNames.YARN_ALLOCATION_CHILD}PipelineItem YACI
                    INNER JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.AllocationChildID = YACI.AllocationChildID
                    WHERE YAC.AllocationID = {allocationId};

                    --YarnPRMaster
                    SELECT YPRM.*
                    FROM YarnPRMaster YPRM
                    WHERE YPRM.YarnPRFromMasterId = {allocationId};

                    --YarnPRChild
                    SELECT YPRC.*
                    FROM YarnPRChild YPRC
                    INNER JOIN YarnPRMaster YPRM ON YPRM.YarnPRMasterID = YPRC.YarnPRMasterID
                    WHERE YPRM.YarnPRFromMasterId = {allocationId};

                    --YarnPRMaster
                    SELECT POM.*
                    FROM YarnPOMaster POM
                    INNER JOIN YarnPRMaster PRM ON PRM.YarnPRMasterID = POM.PRMasterID
                    WHERE PRM.YarnPRFromMasterId = {allocationId} AND POM.UnApprove = 1;";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);

                YarnAllocationMaster data = await records.ReadFirstOrDefaultAsync<YarnAllocationMaster>();
                Guard.Against.NullObject(data);

                data.Childs = records.Read<YarnAllocationChild>().ToList();
                var childItems = records.Read<YarnAllocationChildItem>().ToList();
                var childPLItems = records.Read<YarnAllocationChildPipelineItem>().ToList();

                data.YarnPRMaster = records.ReadFirstOrDefault<YarnPRMaster>();
                var pos = records.Read<YarnPOMaster>().ToList();
                if (data.YarnPRMaster != null)
                {
                    data.YarnPRMaster.YarnPOMasters = pos.IsNull() ? new List<YarnPOMaster>() : pos;
                }
                if (data.YarnPRMaster != null)
                {
                    data.YarnPRMaster.Childs = records.Read<YarnPRChild>().ToList();
                }
                else
                {
                    data.YarnPRMaster = new YarnPRMaster();
                }

                data.Childs.ForEach(c =>
                {
                    c.ChildItems = childItems.Where(ci => ci.AllocationChildID == c.AllocationChildID).ToList();
                    c.ChildPipelineItems = childPLItems.Where(ci => ci.AllocationChildID == c.AllocationChildID).ToList();
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
        public async Task<YarnAllocationMaster> GetAsyncRevised(int allocationId)
        {
            var sql = $@"
                    --Master
                    SELECT YAM.*, ConceptNo = STRING_AGG(FCM.ConceptNo, ',')  
                    FROM {TableNames.YARN_ALLOCATION_MASTER} YAM
					INNER JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.AllocationID = YAM.YarnAllocationID
					INNER JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBCI.YBChildItemID = YAC.YBChildItemID
					INNER JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID = YBCI.YBChildID
					Inner JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConsumptionID = YBC.ConsumptionID AND FCM.ItemMasterID = YBC.ItemMasterID
                    WHERE YarnAllocationID = {allocationId}
					GROUP BY YAM.YarnAllocationID,YAM.YarnAllocationID, YAM.YarnAllocationDate,YAM.YarnAllocationNo,YAM.ExportOrderID,YAM.Approve,YAM.ApproveBy,YAM.ApproveDate
						   ,YAM.Acknowledge,YAM.AcknowledgeBy,YAM.AcknowledgeDate,YAM.UnAcknowledge,YAM.UnAcknowledgeBy,YAM.UnAcknowledgeDate
						   ,YAM.UnAcknowledgeReason,YAM.AddedBy,YAM.UpdatedBy,YAM.BookingNo,YAM.BuyerID,YAM.BuyerTeamID,YAM.EWONo,YAM.FabricDeliveryDate
						   ,YAM.TNA,YAM.BAnalysisID,YAM.BAnalysisNo,YAM.Propose,YAM.ProposeBy,YAM.ProposeDate,YAM.Reject,YAM.RejectBy,YAM.RejectDate
						   ,YAM.RejectReason,YAM.DateAdded,YAM.DateUpdated,YAM.RevisionNo, YAM.IsAutoGenerate, YAM.MRIRChildID;

                    --Childs
                    SELECT YAC.AllocationChildID, YAC.AllocationID, ItemMasterID = YBCI.YItemMasterID, YAC.YBChildItemID, YAC.YBookingNo,
							 YAC.UnitID, YAC.SUnitID, YAC.LotNo, YAC.ReqQty, YAC.AdvanceStockAllocationQty, YAC.PipelineStockAllocationQty,
							 YAC.QtyForPO, YAC.InhouseStartDate, YAC.InhouseEndDate, YAC.TotalAllocationQty, YAC.Remarks, YAC.PreRevisionNo,
							 YAC.RevisionNo, YAC.RevisionBy, YAC.RevisionDate, YAC.QtyFromAutoGenerate,FBA.BookingNo
                    FROM {TableNames.YARN_ALLOCATION_CHILD} YAC
					LEFT JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBCI.YBChildItemID = YAC.YBChildItemID
					LEFT JOIN {TableNames.YarnBookingChildItem_New_Bk} YBCIBK ON YBCIBK.YBChildItemID = YAC.YBChildItemID
					LEFT JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID = YBCI.YBChildID
					LEFT JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID = YBC.YBookingID
					LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.BookingID = YBM.BookingID
                    WHERE YAC.AllocationID = {allocationId} --AND YBCI.NetYarnReqQty <> YBCIBK.NetYarnReqQty
                          AND (ISNULL(YBCI.NetYarnReqQty,0) <> ISNULL(YBCIBK.NetYarnReqQty,0) OR YBCI.YItemMasterID<>YBCIBK.YItemMasterID)
					GROUP BY YAC.AllocationChildID, YAC.AllocationID, YBCI.YItemMasterID, YAC.YBChildItemID, YAC.YBookingNo,
							 YAC.UnitID, YAC.SUnitID, YAC.LotNo, YAC.ReqQty, YAC.AdvanceStockAllocationQty, YAC.PipelineStockAllocationQty,
							 YAC.QtyForPO, YAC.InhouseStartDate, YAC.InhouseEndDate, YAC.TotalAllocationQty, YAC.Remarks, YAC.PreRevisionNo,
							 YAC.RevisionNo, YAC.RevisionBy, YAC.RevisionDate, YAC.QtyFromAutoGenerate,FBA.BookingNo;

                    --ChildItems
                    SELECT YACI.*,NumericCount = ISV6.SegmentValue
                    FROM {TableNames.YARN_ALLOCATION_CHILD_ITEM} YACI
                    INNER JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.AllocationChildID = YACI.AllocationChildID
                    INNER JOIN {TableNames.YarnStockSet} YSS ON  YSS.YarnStockSetId = YACI.YarnStockSetId
					INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YSS.ItemMasterID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                    WHERE YAC.AllocationID = {allocationId};

                    --ChildItems Pipeline
                    SELECT YACI.*
                    FROM {TableNames.YARN_ALLOCATION_CHILD}PipelineItem YACI
                    INNER JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.AllocationChildID = YACI.AllocationChildID
                    WHERE YAC.AllocationID = {allocationId};";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);

                YarnAllocationMaster data = await records.ReadFirstOrDefaultAsync<YarnAllocationMaster>();
                Guard.Against.NullObject(data);

                data.Childs = records.Read<YarnAllocationChild>().ToList();
                var childItems = records.Read<YarnAllocationChildItem>().ToList();
                var childPLItems = records.Read<YarnAllocationChildPipelineItem>().ToList();

                data.Childs.ForEach(c =>
                {
                    c.ChildItems = childItems.Where(ci => ci.AllocationChildID == c.AllocationChildID).ToList();
                    c.ChildPipelineItems = childPLItems.Where(ci => ci.AllocationChildID == c.AllocationChildID).ToList();
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
        public async Task<YarnAllocationMaster> GetChildItemByStockSetId(int allocationChildId, int yarnStockSetId)
        {
            var sql = $@"
                    --Master
                    SELECT YAM.* 
                    FROM {TableNames.YARN_ALLOCATION_MASTER} YAM
                    INNER JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.AllocationID = YAM.YarnAllocationID
                    WHERE YAC.AllocationChildID = {allocationChildId};

                    --Childs
                    SELECT YAC.*, YBCI.NetYarnReqQty
                    FROM {TableNames.YARN_ALLOCATION_CHILD} YAC
                    INNER JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBCI.YBChildItemID = YAC.YBChildItemID
                    WHERE YAC.AllocationChildID = {allocationChildId};

                    --ChildItems
                    SELECT YACI.*,NumericCount = ISV6.SegmentValue
                    FROM {TableNames.YARN_ALLOCATION_CHILD_ITEM} YACI
                    INNER JOIN {TableNames.YarnStockSet} YSS ON  YSS.YarnStockSetId = YACI.YarnStockSetId
					INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YSS.ItemMasterID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                    WHERE YACI.AllocationChildID = {allocationChildId};

                    --ChildPipelineItems
                    SELECT YACI.*
                    FROM {TableNames.YARN_ALLOCATION_CHILD}PipelineItem YACI
                    WHERE YACI.AllocationChildID = {allocationChildId}";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);

                YarnAllocationMaster data = await records.ReadFirstOrDefaultAsync<YarnAllocationMaster>();
                Guard.Against.NullObject(data);

                data.Childs = records.Read<YarnAllocationChild>().ToList();
                var childItems = records.Read<YarnAllocationChildItem>().ToList();
                var childPipelintItems = records.Read<YarnAllocationChildPipelineItem>().ToList();

                data.Childs.ForEach(c =>
                {
                    c.ChildItems = childItems.Where(ci => ci.AllocationChildID == c.AllocationChildID).ToList();
                    c.ChildPipelineItems = childPipelintItems.Where(ci => ci.AllocationChildID == c.AllocationChildID).ToList();
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
        public async Task<YarnAllocationChildItem> GetItemAsync(int id)
        {
            var sql = $@"SELECT * 
                    FROM {TableNames.YARN_ALLOCATION_CHILD_ITEM} 
                    WHERE AllocationChildItemID = {id};";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);

                YarnAllocationChildItem data = await records.ReadFirstOrDefaultAsync<YarnAllocationChildItem>();

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
        public async Task<List<YarnPRMaster>> GetPRMasterByBookingNoAsync(string bookingNo)
        {
            bool isAddition = bookingNo.Contains("-Add-");
            string sql = "";
            if (isAddition)
            {
                sql = $@"SELECT * 
                    FROM YarnPRMaster 
                    WHERE YarnPRno LIKE '{bookingNo}%';";
            }
            else
            {
                sql = $@"SELECT * 
                    FROM YarnPRMaster 
                    WHERE YarnPRno LIKE '{bookingNo}%' AND YarnPRno NOT LIKE '%-Add-%';";
            }
            return await _service.GetDataAsync<YarnPRMaster>(sql);
        }
        public async Task<List<FBookingAcknowledge>> GetFBookingAcknowledgeByBookingNo(string bookingNo)
        {
            var sql = $@"Select * FROM {TableNames.FBBOOKING_ACKNOWLEDGE} where BookingNo = '{bookingNo}';";
            try
            {
                List<FBookingAcknowledge> data = await _service.GetDataAsync<FBookingAcknowledge>(sql);
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
        public async Task<List<YarnBookingMaster>> GetYarnBookingMasterByBookingNo(string ybookingNo)
        {
            var sql = $@"Select * FROM {TableNames.YarnBookingMaster_New} where YBookingNo = '{ybookingNo}';";
            try
            {
                List<YarnBookingMaster> data = await _service.GetDataAsync<YarnBookingMaster>(sql);
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
        public async Task<YarnAllocationChild> GetChildAsync(int id)
        {
            var sql = $@"SELECT * 
                    FROM {TableNames.YARN_ALLOCATION_CHILD} 
                    WHERE AllocationChildID = {id};";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);

                YarnAllocationChild data = await records.ReadFirstOrDefaultAsync<YarnAllocationChild>();

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
        public async Task<YarnAllocationMaster> GetAckAsync(int id)
        {
            var sql = $@"
                    WITH CurrentChildItem AS
                    (
	                    SELECT YAM.*
	                    ,YarnReqQty = YBCI.NetYarnReqQty
	                    ,AllocatedQty = YAC.AdvanceStockAllocationQty
	                    ,PipelineQty = YAC.PipelineStockAllocationQty
	                    ,YAC.QtyForPO
	                    ,YBCI.YarnCategory
	                    ,YAC.YBChildItemID, YBM.IsAddition
	                    FROM {TableNames.YARN_ALLOCATION_MASTER} YAM 
	                    INNER JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.AllocationID = YAM.YarnAllocationID
	                    INNER JOIN {TableNames.YARN_ALLOCATION_CHILD_ITEM} YACI ON YACI.AllocationChildID = YAC.AllocationChildID
	                    INNER JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBCI.YBChildItemID = YAC.YBChildItemID
						INNER JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID = YBCI.YBChildID
						INNER JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID = YBC.YBookingID
	                    WHERE YACI.AllocationChildItemID = {id}
                    ),
                    QtyForPO AS
                    (
	                    SELECT CCI.YBChildItemID
	                    ,OtherAllocatedQty = SUM(YAC.QtyForPO)
	                    FROM CurrentChildItem CCI
	                    INNER JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.YBChildItemID = CCI.YBChildItemID
	                    GROUP BY CCI.YBChildItemID
                    ),
                    OtherChildItemsAdvanceAllocated AS
                    (
	                    SELECT CCI.YBChildItemID
	                    ,OtherAllocatedQty = SUM(YACI.AdvanceAllocationQty) + SUM(YACI.SampleAllocationQty) + SUM(YACI.LiabilitiesAllocationQty) + SUM(YACI.LeftoverAllocationQty)
	                    FROM CurrentChildItem CCI
	                    INNER JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.YBChildItemID = CCI.YBChildItemID
	                    INNER JOIN {TableNames.YARN_ALLOCATION_CHILD_ITEM} YACI ON YACI.AllocationChildID = YAC.AllocationChildID
	                    WHERE YACI.AllocationChildItemID <> {id}
	                    GROUP BY CCI.YBChildItemID
                    ),
                    OtherChildItemsPipelineAllocated AS
                    (
	                    SELECT CCI.YBChildItemID
	                    ,OtherAllocatedQty = SUM(YACPI.PipelineAllocationQty)
	                    FROM CurrentChildItem CCI
	                    INNER JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.YBChildItemID = CCI.YBChildItemID
	                    INNER JOIN {TableNames.YARN_ALLOCATION_CHILD_PIPELINE_ITEM} YACPI ON YACPI.AllocationChildID = YAC.AllocationChildID
	                    GROUP BY CCI.YBChildItemID
                    )
                    SELECT A.*, OtherAllocatedQty = B.OtherAllocatedQty + C.OtherAllocatedQty + D.OtherAllocatedQty
                    FROM CurrentChildItem A
                    LEFT JOIN OtherChildItemsAdvanceAllocated B ON B.YBChildItemID = A.YBChildItemID
                    LEFT JOIN OtherChildItemsPipelineAllocated C ON C.YBChildItemID = A.YBChildItemID
                    LEFT JOIN QtyForPO D ON D.YBChildItemID = A.YBChildItemID;

                    WITH FL AS
                    (
	                    Select YACI.AllocationChildItemID,YACI.AllocationChildID,YAC.ItemMasterId, YSS.ItemMasterId AllocationItemMasterId,FBA.BookingNo,YACI.YarnCategory, YSS.PhysicalCount, YSS.ShadeCode,
	                    PhysicalLot = YSS.YarnLotNo,YSS.YarnStockSetId,YACI.QuarantineAllocationQty,YACI.AdvanceAllocationQty,YACI.SampleAllocationQty,YACI.LiabilitiesAllocationQty,YACI.LeftoverAllocationQty, YACI.TotalAllocationQty,YACI.Remarks,YACI.UnAcknowledgeReason,
	                    Spinner = C.ShortName,
	                    AllocatedBy = E.EmployeeName,
	                    BuyerName = B.ShortName,
	                    FabricType = ISV1.SegmentValue,
	                    LotRef = YBCI.YarnLotNo, YC1.SegmentValue C1, YC2.SegmentValue C2, YC1.SegmentValue YarnCount,
	                    IsDifferentItem = Case When YC1.SegmentValue <> YC2.SegmentValue Then 1 Else 0 End,
	                    YBCI.NetYarnReqQty, YACI.PreProcessRevNo,NumericCount = YC1.SegmentValue,
	                    YAC.QtyForPO,
	                    YAC.AdvanceStockAllocationQty,
	                    YAC.PipelineStockAllocationQty,
	                    FabricColor = CASE WHEN FBC.SubGroupID = 1 THEN ISV3F.SegmentValue ELSE ISV5F.SegmentValue END,
	                    ReqCount = ISV6YBCI.SegmentValue,
	                    ReqYarnDetails = YBCI.YarnCategory,
	                    AllocatedCount = ISV6YACI.SegmentValue, YBM.YBookingNo
	                    FROM {TableNames.YARN_ALLOCATION_CHILD_ITEM} YACI
	                    INNER JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.AllocationChildID=YACI.AllocationChildID
	                    INNER JOIN {TableNames.YARN_ALLOCATION_MASTER} YAM ON YAM.YarnAllocationID=YAC.AllocationID
	                    INNER JOIN {TableNames.YarnStockSet} YSS ON YSS.YarnStockSetId = YACI.YarnStockSetId
	                    INNER JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBCI.YBChildItemID = YAC.YBChildItemID
	                    INNER JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID = YBCI.YBChildID
	                    INNER JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID = YBC.YBookingID
	                    INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FBC ON FBC.BookingChildID = YBC.BookingChildID AND FBC.BookingID = YBM.BookingID
	                    INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.FBAckID = FBC.AcknowledgeID

	                    INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YBC.ItemMasterID
	                    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID

	                    INNER JOIN {DbNames.EPYSL}..ItemMaster IM2 ON IM2.ItemMasterID = YAC.ItemMasterID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue YC1 ON YC1.SegmentValueID = IM2.Segment6ValueID

	                    INNER JOIN {DbNames.EPYSL}..ItemMaster IM3 ON IM3.ItemMasterID = YSS.ItemMasterID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue YC2 ON YC2.SegmentValueID = IM3.Segment6ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6YACI ON ISV6YACI.SegmentValueID = IM3.Segment6ValueID

	                    INNER JOIN {DbNames.EPYSL}..ItemMaster IMF ON IMF.ItemMasterID = FBC.ItemMasterID
	                    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3F ON ISV3F.SegmentValueID = IMF.Segment3ValueID
	                    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5F ON ISV5F.SegmentValueID = IMF.Segment5ValueID

	                    INNER JOIN {DbNames.EPYSL}..ItemMaster IMYBCI ON IMYBCI.ItemMasterID = YBCI.YItemMasterID
	                    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6YBCI ON ISV6YBCI.SegmentValueID = IMYBCI.Segment6ValueID


	                    INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID=YSS.SpinnerId
	                    INNER JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID=YAM.BuyerID
	                    Inner Join {DbNames.EPYSL}..LoginUser LU On YAM.AddedBy = LU.UserCode
	                    Inner Join {DbNames.EPYSL}..Employee E On E.EmployeeCode = LU.EmployeeCode
                        WHERE YACI.AllocationChildItemID = {id} --AND YAM.Approve=1 AND YACI.Acknowledge = 0 AND YACI.UnAcknowledge = 0
                    )
                    SELECT * FROM FL;

                    SELECT YBM.YBookingNo, BuyerName = CTO.ShortName, BuyerTeamName = CCT.TeamName
                    FROM {TableNames.YARN_ALLOCATION_MASTER} YAM
					INNER JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.AllocationID=YAM.YarnAllocationID
					INNER JOIN {TableNames.YARN_ALLOCATION_CHILD_ITEM} YACI  ON YAC.AllocationChildID=YACI.AllocationChildID
					INNER JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBCI.YBChildItemID=YAC.YBChildItemID
                    INNER JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID = YBCI.YBChildID
                    INNER JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID = YBC.YBookingID
                    INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.BookingID = YBM.BookingID
                    LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = YBM.BuyerID
                    LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = YBM.BuyerTeamID
                    LEFT JOIN {DbNames.EPYSL}..Contacts CCS ON CCS.ContactID = YBCI.SpinnerID
                    WHERE YACI.AllocationChildItemID = {id}
                    GROUP BY YBM.YBookingNo, CTO.ShortName, CCT.TeamName";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);

                YarnAllocationMaster data = await records.ReadFirstOrDefaultAsync<YarnAllocationMaster>();
                data.ChildItems = records.Read<YarnAllocationChildItem>().ToList();
                if (data.ChildItems.Count() > 0)
                {
                    data.AllocationChildItemID = data.ChildItems.First().AllocationChildItemID;
                }
                data.SummaryChilds = records.Read<YarnAllocationChild>().ToList();
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
        /* Previous
        public async Task<List<YarnAllocationChildItem>> GetStockAsync(int itemMasterId, int allocationChildID, string operationType, string yarnCount)
        {
            var sql = $@"
                    WITH
                YTD AS(
	                Select MAX(YRM.QCRemarksMasterID) QCRemarksMasterID,YRMC.LotNo,YRMC.ItemMasterId,YRM.SupplierId,YRM.SpinnerId,YRMC.ShadeCode,YRMC.PhysicalCount,
	                Rate = ISNULL(POC.Rate,0) 
	                from YarnQCRemarksChild YRMC
	                INNER JOIN YarnQCRemarksMaster YRM ON YRM.QCRemarksMasterID = YRMC.QCRemarksMasterID
	                INNER JOIN YarnQCReceiveChild RC ON RC.QCReceiveChildID = YRMC.QCReceiveChildID
	                INNER JOIN YarnQCIssueChild IC ON IC.QCIssueChildID = RC.QCIssueChildID
	                INNER JOIN YarnQCReqChild RC1 ON RC1.QCReqChildID = IC.QCReqChildID
	                INNER JOIN YarnReceiveChild YRC ON YRC.ChildID = RC1.ReceiveChildID
	                LEFT JOIN YarnPOChild POC ON POC.YPOChildID = YRC.POChildID
	                GROUP BY YRMC.LotNo,YRMC.ItemMasterId,YRM.SupplierId,YRM.SpinnerId,YRMC.ShadeCode,YRMC.PhysicalCount,ISNULL(POC.Rate,0)
                ),
                YT AS(
	                Select CASE WHEN YRMC.Approve = 1 THEN 'Approve'
				                WHEN YRMC.Reject = 1 THEN 'Reject'
				                WHEN YRMC.Retest = 1 THEN 'Retest'
				                WHEN YRMC.Diagnostic = 1 THEN 'Diagnostic'
				                WHEN YRMC.CommerciallyApprove = 1 THEN 'CommerciallyApprove' 
				                ELSE '' END Status,
	                YRMC.Remarks,YRMC.LotNo,YRMC.ItemMasterId,YRM.SupplierId,YRM.SpinnerId,YRMC.ShadeCode,YRMC.PhysicalCount,
	                Rate = YTD.Rate
	                from YarnQCRemarksChild YRMC
	                INNER JOIN YarnQCRemarksMaster YRM ON YRM.QCRemarksMasterID = YRMC.QCRemarksMasterID
	                INNER JOIN YTD ON YTD.QCRemarksMasterID = YRMC.QCRemarksMasterID 
	                INNER JOIN YarnQCReceiveChild RC ON RC.QCReceiveChildID = YRMC.QCReceiveChildID
	                INNER JOIN YarnQCIssueChild IC ON IC.QCIssueChildID = RC.QCIssueChildID
	                INNER JOIN YarnQCReqChild RC1 ON RC1.QCReqChildID = IC.QCReqChildID
	                INNER JOIN YarnReceiveChild YRC ON YRC.ChildID = RC1.ReceiveChildID
	                LEFT JOIN YarnPOChild POC ON POC.YPOChildID = YRC.POChildID
                ),
                YAC AS
				(
					SELECT YACI.YarnStockSetId 
					,AdvanceAllocationQty = ISNULL(SUM(YACI.AdvanceAllocationQty),0)
					,SampleAllocationQty = ISNULL(SUM(YACI.SampleAllocationQty),0)
					,LiabilitiesAllocationQty = ISNULL(SUM(YACI.LiabilitiesAllocationQty),0)
					,LeftoverAllocationQty = ISNULL(SUM(YACI.LeftoverAllocationQty),0)
					FROM {TableNames.YARN_ALLOCATION_CHILD_ITEM} YACI
					WHERE YACI.AllocationChildID = {allocationChildID}
					GROUP BY YACI.YarnStockSetId
				),
                StockInfo AS
                (
	                SELECT YSS.ItemMasterId, YSS.YarnStockSetId
			                ,YSS.YarnCategory
			                ,NumericCount = ISV6.SegmentValue, Spinner = CCS.ShortName,
			                YSS.PhysicalCount, PhysicalLot = YSS.YarnLotNo, POPrice = YT.Rate,
			                --ISNULL(YA.Age,0) YarnAge,
			                YarnAge = DATEDIFF(DAY, MAX(YSS.YarnApprovedDate), GETDATE()),
			                YT.Status TestResult,YT.Remarks TestResultComments,
			                AdvanceStockQty = SUM(YSM.AdvanceStockQty) + ISNULL(YAC.AdvanceAllocationQty,0),
			                SampleStockQty = SUM(YSM.SampleStockQty) + ISNULL(YAC.SampleAllocationQty,0),
			                LeftoverStockQty = SUM(YSM.LeftoverStockQty) + ISNULL(YAC.LeftoverAllocationQty,0),
			                LiabilitiesStockQty = SUM(YSM.LiabilitiesStockQty) + ISNULL(YAC.LiabilitiesAllocationQty,0)
	                FROM YarnStockSet YSS
	                INNER JOIN {TableNames.YarnStockMaster} YSM ON YSM.YarnStockSetId = YSS.YarnStockSetId
                    LEFT JOIN YAC ON YAC.YarnStockSetId = YSS.YarnStockSetId
	                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YSS.ItemMasterID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
	                INNER JOIN {DbNames.EPYSL}..Contacts CCS ON CCS.ContactID = YSS.SpinnerID
	                --LEFT JOIN YarnAge YA ON YA.LotNo = YSS.YarnLotNo
	                LEFT JOIN YT ON YT.LotNo = YSS.YarnLotNo 
	                            AND YT.ItemMasterId = YSS.ItemMasterId 
				                AND YT.SupplierId = YSS.SupplierId 
				                AND YT.ShadeCode = YSS.ShadeCode 
				                AND YT.PhysicalCount = YSS.PhysicalCount
	                WHERE YSS.ItemMasterId = {itemMasterId} AND YSS.SpinnerId > 0
	                GROUP BY YSS.ItemMasterId, YSS.YarnStockSetId
			                ,YSS.YarnCategory
			                ,ISV6.SegmentValue, CCS.ShortName
			                ,YSS.PhysicalCount, YSS.YarnLotNo,YT.Status,YT.Remarks,YT.Rate
                            ,ISNULL(YAC.AdvanceAllocationQty,0),ISNULL(YAC.SampleAllocationQty,0),ISNULL(YAC.LeftoverAllocationQty,0),ISNULL(YAC.LiabilitiesAllocationQty,0)
                ),
                FinalList AS (
	                SELECT SI.*
	                FROM StockInfo SI
	                Where SI.AdvanceStockQty>0 OR SI.SampleStockQty>0 OR SI.LeftoverStockQty>0 OR SI.LiabilitiesStockQty>0
                )
                SELECT * FROM FinalList";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                List<YarnAllocationChildItem> data = records.Read<YarnAllocationChildItem>().ToList();
                Guard.Against.NullObject(data);

                int allocationChildItemID = 999;
                data.Where(c => c.AllocationChildItemID == 0).ToList().ForEach(c =>
                {
                    c.AllocationChildItemID = allocationChildItemID++;
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
        
        public async Task<List<YarnAllocationChildPipelineItem>> GetPipelineStockAsync(int itemMasterId, int allocationChildID, string operationType, string yarnCount)
        {
            string tempQuery = "";
            if (operationType == "updateOperation")
            {
                tempQuery = " + ISNULL(YAC1.PipelineAllocationQty,0) ";
            }

            var sql = $@"
                WITH 
                YarnAge AS(
	                Select DATEDIFF(DD, MAX(YRM.ApprovedDate), GETDATE()) Age, YRC.LotNo 
	                from YarnReceiveChild YRC
	                INNER JOIN YarnReceiveMaster YRM ON YRM.ReceiveID = YRC.ReceiveID
	                WHERE YRC.ItemMasterID = {itemMasterId}
	                GROUP BY YRC.LotNo
                ),
                YTD AS(
	                Select QCRemarksMasterID = MAX(YRM.QCRemarksMasterID),YRMC.LotNo 
	                from YarnQCRemarksChild YRMC
	                INNER JOIN YarnQCRemarksMaster YRM ON YRM.QCRemarksMasterID = YRMC.QCRemarksMasterID
	                GROUP BY YRMC.LotNo
                ),
                YT AS(
	                Select CASE WHEN YRMC.Approve = 1 THEN 'Approve'
				                WHEN YRMC.Reject = 1 THEN 'Reject'
				                WHEN YRMC.Retest = 1 THEN 'Retest'
				                WHEN YRMC.Diagnostic = 1 THEN 'Diagnostic'
				                WHEN YRMC.CommerciallyApprove = 1 THEN 'CommerciallyApprove' 
				                ELSE '' END Status,YRMC.Remarks,YRMC.LotNo
	                from YarnQCRemarksChild YRMC
	                INNER JOIN YarnQCRemarksMaster YRM ON YRM.QCRemarksMasterID = YRMC.QCRemarksMasterID
	                INNER JOIN YTD ON YTD.LotNo=YRMC.LotNo AND YTD.QCRemarksMasterID = YRMC.QCRemarksMasterID 
                ),
                WithoutPipeLineRecord AS
                (
	                SELECT YSS.ItemMasterId, YSS.SupplierId, YSS.ShadeCode,
	                TotalCurrentStockQty = ISNULL(SUM(YSM.TotalCurrentStockQty),0),
	                TotalCurrentPipelineStockQty = ISNULL(SUM(YSM.TotalCurrentPipelineStockQty),0)
	                FROM YarnStockSet YSS
	                INNER JOIN {TableNames.YarnStockMaster} YSM ON YSM.YarnStockSetId = YSS.YarnStockSetId
	                WHERE YSS.ItemMasterId = {itemMasterId} AND YSS.SpinnerId > 0
	                GROUP BY YSS.ItemMasterId, YSS.SupplierId, YSS.ShadeCode
                ),
                PipeLineRecord AS
                (
	                SELECT YSS.YarnStockSetId, YSS.ItemMasterId, YSS.SupplierId, YSS.ShadeCode, YSS.YarnCategory,
	                PipelineStockQty = ISNULL(SUM(YSM.TotalCurrentPipelineStockQty),0) - ISNULL(SUM(WPL.TotalCurrentStockQty),0)
	                FROM YarnStockSet YSS
	                INNER JOIN {TableNames.YarnStockMaster} YSM ON YSM.YarnStockSetId = YSS.YarnStockSetId	
	                LEFT JOIN WithoutPipeLineRecord WPL ON WPL.ItemMasterId = YSS.ItemMasterId
									                    AND WPL.SupplierId = YSS.SupplierId
									                    AND WPL.ShadeCode = YSS.ShadeCode
	                WHERE YSS.ItemMasterId = {itemMasterId} AND YSS.SpinnerId = 0
	                GROUP BY YSS.YarnStockSetId, YSS.ItemMasterId, YSS.SupplierId, YSS.ShadeCode, YSS.YarnCategory
                ),
                YAC AS
				(
					SELECT YACI.YarnStockSetId, PipelineAllocationQty = ISNULL(SUM(YACI.PipelineAllocationQty),0)
					FROM {TableNames.YARN_ALLOCATION_CHILD}PipelineItem YACI
					INNER JOIN PipeLineRecord PLR ON PLR.YarnStockSetId = YACI.YarnStockSetId
					WHERE YACI.AllocationChildID <> {allocationChildID}
					GROUP BY YACI.YarnStockSetId
				),
                YAC1 AS
				(
					SELECT YACI.YarnStockSetId, PipelineAllocationQty = ISNULL(SUM(YACI.PipelineAllocationQty),0)
					FROM {TableNames.YARN_ALLOCATION_CHILD}PipelineItem YACI
					INNER JOIN PipeLineRecord PLR ON PLR.YarnStockSetId = YACI.YarnStockSetId
					WHERE YACI.AllocationChildID = {allocationChildID}
					GROUP BY YACI.YarnStockSetId
				),
                StockInfo AS
                (
	                SELECT PLR.YarnStockSetId,PLR.ItemMasterId,PLR.YarnCategory,PLR.ShadeCode, POPrice = 0,
                    PipelineStockQty = ISNULL(PLR.PipelineStockQty,0) - ISNULL(YAC.PipelineAllocationQty,0) {tempQuery}
	                ,NumericCount = ISV6.SegmentValue, 
	                Spinner = CASE WHEN YSSWPL.SpinnerID > 0 THEN CCS.ShortName ELSE '' END,
	                YSSWPL.PhysicalCount, PhysicalLot = YSSWPL.YarnLotNo, ISNULL(YA.Age,0) YarnAge,
	                TestResult = YT.Status,TestResultComments = YT.Remarks
	                FROM PipeLineRecord PLR
                    LEFT JOIN YAC ON YAC.YarnStockSetId = PLR.YarnStockSetId
                    LEFT JOIN YAC1 ON YAC1.YarnStockSetId = PLR.YarnStockSetId
	                LEFT JOIN {TableNames.YarnStockSet} YSSWPL ON YSSWPL.YarnStockSetId = PLR.YarnStockSetId
	                --LEFT JOIN {TableNames.YarnStockSet} YSSWPL ON YSSWPL.ItemMasterId = PLR.ItemMasterId
							          --          AND YSSWPL.SupplierId = PLR.SupplierId
							          --          AND YSSWPL.ShadeCode = PLR.ShadeCode
							          --          AND YSSWPL.SpinnerId > 0
	                LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = PLR.ItemMasterID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
	                LEFT JOIN {DbNames.EPYSL}..Contacts CCS ON CCS.ContactID = YSSWPL.SpinnerID
	                LEFT JOIN YarnAge YA ON YA.LotNo = YSSWPL.YarnLotNo
	                LEFT JOIN YT ON YT.LotNo = YSSWPL.YarnLotNo
	                WHERE PLR.ItemMasterId = {itemMasterId}
                ),
                FinalList AS (
	                SELECT SI.* 
	                FROM StockInfo SI
	                Where SI.PipelineStockQty > 0
                )
                SELECT * FROM FinalList";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                List<YarnAllocationChildPipelineItem> data = records.Read<YarnAllocationChildPipelineItem>().ToList();
                Guard.Against.NullObject(data);
                int allocationChildPLItemID = 999;
                data.Where(c => c.AllocationChildPLItemID == 0).ToList().ForEach(c =>
                {
                    c.AllocationChildPLItemID = allocationChildPLItemID++;
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
        */

        public async Task<List<YarnAllocationChildItem>> GetStockAsync(int itemMasterId, int allocationChildID, string operationType, string yarnCount, PaginationInfo paginationInfo)
        {
            var sql = $@"
               WITH
                YTD AS(
	                Select MAX(YRM.QCRemarksMasterID) QCRemarksMasterID,YRMC.LotNo,YRMC.ItemMasterId,YRM.SupplierId,YRM.SpinnerId,YRMC.ShadeCode,YRMC.PhysicalCount,
	                Rate = ISNULL(POC.Rate,0) 
	                from YarnQCRemarksChild YRMC
	                INNER JOIN YarnQCRemarksMaster YRM ON YRM.QCRemarksMasterID = YRMC.QCRemarksMasterID
	                INNER JOIN YarnQCReceiveChild RC ON RC.QCReceiveChildID = YRMC.QCReceiveChildID
	                INNER JOIN YarnQCIssueChild IC ON IC.QCIssueChildID = RC.QCIssueChildID
	                INNER JOIN YarnQCReqChild RC1 ON RC1.QCReqChildID = IC.QCReqChildID
	                INNER JOIN YarnReceiveChild YRC ON YRC.ChildID = RC1.ReceiveChildID
	                LEFT JOIN YarnPOChild POC ON POC.YPOChildID = YRC.POChildID
	                GROUP BY YRMC.LotNo,YRMC.ItemMasterId,YRM.SupplierId,YRM.SpinnerId,YRMC.ShadeCode,YRMC.PhysicalCount,ISNULL(POC.Rate,0)
                ),
                YT AS(
	                Select CASE WHEN YRMC.Approve = 1 THEN 'Approve'
				                WHEN YRMC.Reject = 1 THEN 'Reject'
				                WHEN YRMC.Retest = 1 THEN 'Retest'
				                WHEN YRMC.Diagnostic = 1 THEN 'Diagnostic'
				                WHEN YRMC.CommerciallyApprove = 1 THEN 'CommerciallyApprove' 
				                ELSE '' END Status,
	                YRMC.Remarks,YRMC.LotNo,YRMC.ItemMasterId,YRM.SupplierId,YRM.SpinnerId,YRMC.ShadeCode,YRMC.PhysicalCount,
	                Rate = YTD.Rate
	                from YarnQCRemarksChild YRMC
	                INNER JOIN YarnQCRemarksMaster YRM ON YRM.QCRemarksMasterID = YRMC.QCRemarksMasterID
	                INNER JOIN YTD ON YTD.QCRemarksMasterID = YRMC.QCRemarksMasterID 
	                INNER JOIN YarnQCReceiveChild RC ON RC.QCReceiveChildID = YRMC.QCReceiveChildID
	                INNER JOIN YarnQCIssueChild IC ON IC.QCIssueChildID = RC.QCIssueChildID
	                INNER JOIN YarnQCReqChild RC1 ON RC1.QCReqChildID = IC.QCReqChildID
	                INNER JOIN YarnReceiveChild YRC ON YRC.ChildID = RC1.ReceiveChildID
	                LEFT JOIN YarnPOChild POC ON POC.YPOChildID = YRC.POChildID
                ),
                YAC AS
                (
	                SELECT YACI.ItemMasterID, YACI.SupplierId, YACI.SpinnerID, YACI.YarnLotNo, 
					YACI.PhysicalCount, YACI.ShadeCode--, YACI.LocationID, YACI.BookingID, YACI.CompanyID
	                ,AdvanceAllocationQty = ISNULL(SUM(YACI.AdvanceAllocationQty),0)
	                ,SampleAllocationQty = ISNULL(SUM(YACI.SampleAllocationQty),0)
	                ,LiabilitiesAllocationQty = ISNULL(SUM(YACI.LiabilitiesAllocationQty),0)
	                ,LeftoverAllocationQty = ISNULL(SUM(YACI.LeftoverAllocationQty),0)
	                FROM {TableNames.YARN_ALLOCATION_CHILD_ITEM} YACI
	                WHERE YACI.AllocationChildID = 0
	                GROUP BY YACI.ItemMasterID, YACI.SupplierId, YACI.SpinnerID, YACI.YarnLotNo, 
					YACI.PhysicalCount, YACI.ShadeCode
                ),
                FinalList AS
                (
	                SELECT YSM.ItemMasterId--, YSS.YarnStockSetId
			                ,YSM.YarnCategory
			                ,NumericCount = ISV6.SegmentValue, Spinner = CCS.ShortName,
			                YSM.PhysicalCount, PhysicalLot = YSM.YarnLotNo, POPrice = YT.Rate,
			                --ISNULL(YA.Age,0) YarnAge,
			                YarnAge = 0,--DATEDIFF(DAY, MAX(YSS.YarnApprovedDate), GETDATE()),
			                YT.Status TestResult,YT.Remarks TestResultComments,
			                AdvanceStockQty = SUM(YSM.AdvanceStockQty) + ISNULL(YAC.AdvanceAllocationQty,0),
			                SampleStockQty = SUM(YSM.SampleStockQty) + ISNULL(YAC.SampleAllocationQty,0),
			                LeftoverStockQty = SUM(YSM.LeftoverStockQty) + ISNULL(YAC.LeftoverAllocationQty,0),
			                LiabilitiesStockQty = SUM(YSM.LiabilitiesStockQty) + ISNULL(YAC.LiabilitiesAllocationQty,0),
			                YarnCount = ISV6.SegmentValue
	                FROM YarnStockMaster_New YSM 
	                LEFT JOIN YAC ON YAC.ItemMasterID = YSM.ItemMasterID
									AND YAC.SupplierId = YSM.SupplierId
									AND YAC.SpinnerID = YSM.SpinnerID
									AND ISNULL(YAC.YarnLotNo,'') = ISNULL(YSM.YarnLotNo,'')
									AND ISNULL(YAC.PhysicalCount,'') = ISNULL(YSM.PhysicalCount,'')
									AND ISNULL(YAC.ShadeCode,'') = ISNULL(YSM.ShadeCode,'')
	                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YSM.ItemMasterID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
	                INNER JOIN {DbNames.EPYSL}..Contacts CCS ON CCS.ContactID = YSM.SpinnerID
	                --LEFT JOIN YarnAge YA ON YA.LotNo = YSS.YarnLotNo
	                LEFT JOIN YT ON YT.LotNo = YSM.YarnLotNo 
				                AND YT.ItemMasterId = YSM.ItemMasterId 
				                AND YT.SupplierId = YSM.SupplierId 
				                AND YT.ShadeCode = YSM.ShadeCode 
				                AND YT.PhysicalCount = YSM.PhysicalCount
	                WHERE YSM.SpinnerId > 0 
	                AND (YSM.AdvanceStockQty > 0 OR YSM.SampleStockQty > 0 OR YSM.LeftoverStockQty > 0 OR YSM.LiabilitiesStockQty > 0)
	                GROUP BY YSM.ItemMasterId--, YSS.YarnStockSetId
			                ,YSM.YarnCategory
			                ,ISV6.SegmentValue, CCS.ShortName
			                ,YSM.PhysicalCount, YSM.YarnLotNo,YT.Status,YT.Remarks,YT.Rate
			                ,ISNULL(YAC.AdvanceAllocationQty,0),ISNULL(YAC.SampleAllocationQty,0),ISNULL(YAC.LeftoverAllocationQty,0),ISNULL(YAC.LiabilitiesAllocationQty,0)
                )
                SELECT *, Count(*) Over() TotalRows FROM FinalList  ";

            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? " Order By YarnCount Desc" : paginationInfo.OrderBy;

            sql += $@"
                    {paginationInfo.FilterBy}
                    {orderBy}
                    {paginationInfo.PageBy}";

            try
            {
                List<YarnAllocationChildItem> data = await _service.GetDataAsync<YarnAllocationChildItem>(sql);
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
        public async Task<List<YarnAllocationChildPipelineItem>> GetPipelineStockAsync(int itemMasterId, int allocationChildID, string operationType, string yarnCount, PaginationInfo paginationInfo)
        {
            string tempQuery = "";
            if (operationType == "updateOperation")
            {
                tempQuery = " + ISNULL(YAC1.PipelineAllocationQty,0) ";
            }

            var sql = $@"
            WITH
            YTD AS(
	            Select QCRemarksMasterID = MAX(YRM.QCRemarksMasterID),YRMC.LotNo 
	            from YarnQCRemarksChild YRMC
	            INNER JOIN YarnQCRemarksMaster YRM ON YRM.QCRemarksMasterID = YRMC.QCRemarksMasterID
	            GROUP BY YRMC.LotNo
            ),
            YT AS(
	            Select CASE WHEN YRMC.Approve = 1 THEN 'Approve'
				            WHEN YRMC.Reject = 1 THEN 'Reject'
				            WHEN YRMC.Retest = 1 THEN 'Retest'
				            WHEN YRMC.Diagnostic = 1 THEN 'Diagnostic'
				            WHEN YRMC.CommerciallyApprove = 1 THEN 'CommerciallyApprove' 
				            ELSE '' END Status,YRMC.Remarks,YRMC.LotNo
	            from YarnQCRemarksChild YRMC
	            INNER JOIN YarnQCRemarksMaster YRM ON YRM.QCRemarksMasterID = YRMC.QCRemarksMasterID
	            INNER JOIN YTD ON YTD.LotNo=YRMC.LotNo AND YTD.QCRemarksMasterID = YRMC.QCRemarksMasterID 
            ),
            WithoutPipeLineRecord AS
            (
	            SELECT YSS.ItemMasterId, YSS.SupplierId, YSS.ShadeCode,
	            TotalCurrentStockQty = ISNULL(SUM(YSM.TotalCurrentStockQty),0),
	            TotalCurrentPipelineStockQty = ISNULL(SUM(YSM.TotalCurrentPipelineStockQty),0),
	            YarnAge = ISNULL(DATEDIFF(DD, YSS.YarnApprovedDate, GETDATE()),0)
	            FROM YarnStockSet YSS
	            INNER JOIN {TableNames.YarnStockMaster} YSM ON YSM.YarnStockSetId = YSS.YarnStockSetId
	            WHERE YSS.SpinnerId > 0
	            GROUP BY YSS.ItemMasterId, YSS.SupplierId, YSS.ShadeCode,DATEDIFF(DD, YSS.YarnApprovedDate, GETDATE())
            ),
            PipeLineRecord AS
            (
	            SELECT YSS.YarnStockSetId, YSS.ItemMasterId, YSS.SupplierId, YSS.ShadeCode, YSS.YarnCategory,
	            PipelineStockQty = ISNULL(SUM(YSM.TotalCurrentPipelineStockQty),0) - ISNULL(SUM(WPL.TotalCurrentStockQty),0),
	            WPL.YarnAge
	            FROM YarnStockSet YSS
	            INNER JOIN {TableNames.YarnStockMaster} YSM ON YSM.YarnStockSetId = YSS.YarnStockSetId	
	            LEFT JOIN WithoutPipeLineRecord WPL ON WPL.ItemMasterId = YSS.ItemMasterId
									                AND WPL.SupplierId = YSS.SupplierId
									                AND WPL.ShadeCode = YSS.ShadeCode
	            WHERE YSS.SpinnerId = 0
	            GROUP BY YSS.YarnStockSetId, YSS.ItemMasterId, YSS.SupplierId, YSS.ShadeCode, YSS.YarnCategory,WPL.YarnAge
            ),
			PO AS
			(
				SELECT PL.YarnStockSetId, 
				PONos = STRING_AGG(POM.PONo,','), 
				DeliveryStartDates =  STRING_AGG(POM.DeliveryStartDate,','),
				DeliveryEndDate =  STRING_AGG(POM.DeliveryEndDate,','),
				POFors = STRING_AGG(BT.ValueName,','),
				POCompanys = STRING_AGG(CE.ShortName,','),
				Suppliers = STRING_AGG(S.ShortName,','),
				POPrices = STRING_AGG(POC.Rate,',')
				FROM PipeLineRecord PL
				INNER JOIN YarnPOChild POC ON POC.YarnStockSetId = PL.YarnStockSetId
				INNER JOIN YarnPOMaster POM ON POM.YPOMasterID = POC.YPOMasterID
				INNER JOIN {DbNames.EPYSL}..Contacts S ON S.ContactID = POM.SupplierID
				LEFT JOIN {DbNames.EPYSL}..EntityTypeValue BT ON BT.ValueID = POC.POForID
				LEFT JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = POM.CompanyID
				GROUP BY PL.YarnStockSetId
			),
            YAC AS
            (
	            SELECT YACI.YarnStockSetId, PipelineAllocationQty = ISNULL(SUM(YACI.PipelineAllocationQty),0),
	            PLR.YarnAge
	            FROM {TableNames.YARN_ALLOCATION_CHILD}PipelineItem YACI
	            INNER JOIN PipeLineRecord PLR ON PLR.YarnStockSetId = YACI.YarnStockSetId
	            WHERE YACI.AllocationChildID <> 0
	            GROUP BY YACI.YarnStockSetId,PLR.YarnAge
            ),
            YAC1 AS
            (
	            SELECT YACI.YarnStockSetId, PipelineAllocationQty = ISNULL(SUM(YACI.PipelineAllocationQty),0),
	            PLR.YarnAge
	            FROM {TableNames.YARN_ALLOCATION_CHILD}PipelineItem YACI
	            INNER JOIN PipeLineRecord PLR ON PLR.YarnStockSetId = YACI.YarnStockSetId
	            WHERE YACI.AllocationChildID = 0
	            GROUP BY YACI.YarnStockSetId,PLR.YarnAge
            ),
            FinalList AS
            (
	            SELECT PLR.YarnStockSetId,PLR.ItemMasterId,PLR.YarnCategory,PLR.ShadeCode,
                PipelineStockQty = ISNULL(PLR.PipelineStockQty,0) - ISNULL(YAC.PipelineAllocationQty,0) 
	            ,NumericCount = ISV6.SegmentValue, 
	            Spinner = CASE WHEN YSSWPL.SpinnerID > 0 THEN CCS.ShortName ELSE '' END,
	            YSSWPL.PhysicalCount, PhysicalLot = YSSWPL.YarnLotNo, YarnAge = ISNULL(PLR.YarnAge,0),
	            TestResult = YT.Status,TestResultComments = YT.Remarks,
	            YarnCount = ISV6.SegmentValue,
				PO.PONos,
				PO.Suppliers,
				PO.DeliveryEndDate,
				PO.DeliveryStartDates,
				PO.POFors,
				PO.POCompanys,
				PO.POPrices 
	            FROM PipeLineRecord PLR
				INNER JOIN PO ON PO.YarnStockSetId = PLR.YarnStockSetId
                LEFT JOIN YAC ON YAC.YarnStockSetId = PLR.YarnStockSetId
                LEFT JOIN YAC1 ON YAC1.YarnStockSetId = PLR.YarnStockSetId
	            LEFT JOIN {TableNames.YarnStockSet} YSSWPL ON YSSWPL.YarnStockSetId = PLR.YarnStockSetId
	            LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = PLR.ItemMasterID
	            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
	            LEFT JOIN {DbNames.EPYSL}..Contacts CCS ON CCS.ContactID = YSSWPL.SpinnerID
	            LEFT JOIN YT ON YT.LotNo = YSSWPL.YarnLotNo
                WHERE ISNULL(PLR.PipelineStockQty,0) - ISNULL(YAC.PipelineAllocationQty,0) > 0
            )
            SELECT * FROM FinalList FL";

            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? " Order By YarnCount Desc" : paginationInfo.OrderBy;

            sql += $@"
            {paginationInfo.FilterBy}
            {orderBy}
            --ORDER BY CASE WHEN YarnCount = '{yarnCount}' THEN 0 ELSE 1 END
            {paginationInfo.PageBy}";

            try
            {
                List<YarnAllocationChildPipelineItem> data = await _service.GetDataAsync<YarnAllocationChildPipelineItem>(sql);
                data.ForEach(x =>
                {
                    var propValue = CommonFunction.GetDefaultValueWhenInvalidS(x.PONos).Split(',');
                    x.PONos = string.Join(",", propValue.Distinct());

                    propValue = CommonFunction.GetDefaultValueWhenInvalidS(x.Suppliers).Split(',');
                    x.Suppliers = string.Join(",", propValue.Distinct());

                    propValue = CommonFunction.GetDefaultValueWhenInvalidS(x.DeliveryEndDates).Split(',');
                    x.DeliveryEndDates = string.Join(",", propValue.Distinct());

                    propValue = CommonFunction.GetDefaultValueWhenInvalidS(x.DeliveryStartDates).Split(',');
                    x.DeliveryStartDates = string.Join(",", propValue.Distinct());

                    propValue = CommonFunction.GetDefaultValueWhenInvalidS(x.POFors).Split(',');
                    x.POFors = string.Join(",", propValue.Distinct());

                    propValue = CommonFunction.GetDefaultValueWhenInvalidS(x.POCompanys).Split(',');
                    x.POCompanys = string.Join(",", propValue.Distinct());

                    propValue = CommonFunction.GetDefaultValueWhenInvalidS(x.POPrices).Split(',');
                    x.POPrices = string.Join(",", propValue.Distinct());
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

        public async Task<List<YarnAllocationChildItem>> GetAllAllocationByYBChildItemId(int ybChildItemID, int allocationChildItemID, PaginationInfo paginationInfo)
        {
            string tempGuid = CommonFunction.GetNewGuid();
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? " Order By YarnAllocationNo Desc" : paginationInfo.OrderBy;

            var sql = $@"
                WITH CurrentChildItem AS
                (
	                SELECT YAM.*
	                ,YarnReqQty = YBCI.NetYarnReqQty
	                ,AllocatedQty = YAC.AdvanceStockAllocationQty
	                ,PipelineQty = YAC.PipelineStockAllocationQty
	                ,YAC.QtyForPO
	                ,YBCI.YarnCategory
	                ,YAC.YBChildItemID
	                FROM {TableNames.YARN_ALLOCATION_MASTER} YAM 
	                INNER JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.AllocationID = YAM.YarnAllocationID
	                INNER JOIN {TableNames.YARN_ALLOCATION_CHILD_ITEM} YACI ON YACI.AllocationChildID = YAC.AllocationChildID
	                INNER JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBCI.YBChildItemID = YAC.YBChildItemID
	                WHERE YACI.AllocationChildItemID = {allocationChildItemID}
                ),
                QtyForPO AS
                (
	                SELECT CCI.YBChildItemID, YAM.YarnAllocationNo
	                ,YAC.QtyForPO
	                ,AdvanceAllocationQty=0
	                ,SampleAllocationQty=0
	                ,LiabilitiesAllocationQty=0
	                ,LeftoverAllocationQty=0
	                ,PipelineAllocationQty=0
	                FROM CurrentChildItem CCI
	                INNER JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.YBChildItemID = CCI.YBChildItemID
	                INNER JOIN {TableNames.YARN_ALLOCATION_MASTER} YAM ON YAM.YarnAllocationID = YAC.AllocationID
                ),
                OtherChildItemsAdvanceAllocated AS
                (
	                SELECT CCI.YBChildItemID, YAM.YarnAllocationNo
	                ,QtyForPO=0
	                ,YACI.AdvanceAllocationQty
	                ,YACI.SampleAllocationQty
	                ,YACI.LiabilitiesAllocationQty
	                ,YACI.LeftoverAllocationQty
	                ,PipelineAllocationQty=0
	                FROM CurrentChildItem CCI
	                INNER JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.YBChildItemID = CCI.YBChildItemID
	                INNER JOIN {TableNames.YARN_ALLOCATION_MASTER} YAM ON YAM.YarnAllocationID = YAC.AllocationID
	                INNER JOIN {TableNames.YARN_ALLOCATION_CHILD_ITEM} YACI ON YACI.AllocationChildID = YAC.AllocationChildID
	                WHERE YACI.AllocationChildItemID <> {allocationChildItemID}
                ),
                OtherChildItemsPipelineAllocated AS
                (
	                SELECT CCI.YBChildItemID, YAM.YarnAllocationNo
	                ,QtyForPO=0
	                ,AdvanceAllocationQty=0
	                ,SampleAllocationQty=0
	                ,LiabilitiesAllocationQty=0
	                ,LeftoverAllocationQty=0
	                ,YACPI.PipelineAllocationQty
	                FROM CurrentChildItem CCI
	                INNER JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.YBChildItemID = CCI.YBChildItemID
	                INNER JOIN {TableNames.YARN_ALLOCATION_MASTER} YAM ON YAM.YarnAllocationID = YAC.AllocationID
	                INNER JOIN {TableNames.YARN_ALLOCATION_CHILD_PIPELINE_ITEM} YACPI ON YACPI.AllocationChildID = YAC.AllocationChildID
                ),
                FinalList AS
                (
	                SELECT * FROM QtyForPO
	                UNION
	                SELECT * FROM OtherChildItemsAdvanceAllocated
	                UNION
	                SELECT * FROM OtherChildItemsPipelineAllocated
                )
                SELECT * INTO #TempTable{tempGuid}
                FROM FinalList FL
                SELECT *, Count(*) Over() TotalRows FROM #TempTable{tempGuid}
                ";

            sql += $@"
                    {paginationInfo.FilterBy}
                    {orderBy}
                    {paginationInfo.PageBy}";

            sql += $@" DROP TABLE #TempTable{tempGuid}";

            try
            {
                List<YarnAllocationChildItem> data = await _service.GetDataAsync<YarnAllocationChildItem>(sql);
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

        public async Task<List<YarnAllocationChildItem>> GetAllocationsByBookingNo(string bookingNo, PaginationInfo paginationInfo)
        {
            string tempGuid = CommonFunction.GetNewGuid();
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? " Order By YarnAllocationNo DESC, AllocatedQty DESC" : paginationInfo.OrderBy;

            var sql = $@"
                WITH FBA AS
                (
	                SELECT YBCI.YBChildItemID, YBCI.YarnCategory, FBA.BookingNo
	                ,BuyerName = B.ShortName
	                ,FabricType = ISV1FBC.SegmentValue
	                FROM {TableNames.YarnBookingChildItem_New} YBCI
	                INNER JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID = YBCI.YBChildID
                    INNER JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID = YBC.YBookingID
                    INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FBC ON FBC.BookingChildID = YBC.BookingChildID AND FBC.BookingID = YBM.BookingID
                    INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.FBAckID = FBC.AcknowledgeID
	                LEFT JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = FBA.BuyerID
	                LEFT JOIN {DbNames.EPYSL}..ItemMaster IMFBC ON IMFBC.ItemMasterID = FBC.ItemMasterID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1FBC ON ISV1FBC.SegmentValueID = IMFBC.Segment1ValueID
	                WHERE FBA.BookingNo = '{bookingNo}'
	                GROUP BY YBCI.YBChildItemID, YBCI.YarnCategory, FBA.BookingNo, B.ShortName, ISV1FBC.SegmentValue
                ),
                FinalList AS
                (
	                SELECT YAM.YarnAllocationNo
                    ,FBA.BookingNo
                    ,FBA.BuyerName
                    ,FBA.FabricType
                    ,YSS.ShadeCode
                    ,YSS.PhysicalCount
                    ,PhysicalLot = YSS.YarnLotNo
                    ,Spinner = SP.ShortName
                    ,ReqYarnDetails = FBA.YarnCategory
                    ,AllocatedYarnDetails = STRING_AGG(YSS.YarnCategory,',')
	                ,AllocatedQty = ISNULL(SUM(YACI.AdvanceAllocationQty) + SUM(YACI.AdvanceAllocationQty) + SUM(YACI.SampleAllocationQty) + SUM(YACI.LiabilitiesAllocationQty) + SUM(YACI.LeftoverAllocationQty),0)
	                FROM FBA
	                LEFT JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.YBChildItemID = FBA.YBChildItemID
	                LEFT JOIN {TableNames.YARN_ALLOCATION_MASTER} YAM ON YAM.YarnAllocationID = YAC.AllocationID
	                LEFT JOIN {TableNames.YARN_ALLOCATION_CHILD_ITEM} YACI ON YACI.AllocationChildID = YAC.AllocationChildID
	                LEFT JOIN {TableNames.YarnStockSet} YSS ON YSS.YarnStockSetId = YACI.YarnStockSetId
	                LEFT JOIN {DbNames.EPYSL}..Contacts SP ON SP.ContactID = YSS.SpinnerId
	                GROUP BY YAM.YarnAllocationNo,FBA.BookingNo
                    ,FBA.BuyerName
                    ,FBA.FabricType
                    ,YSS.ShadeCode
                    ,YSS.PhysicalCount
                    ,YSS.YarnLotNo
                    ,SP.ShortName
                    ,FBA.YarnCategory
                )
                SELECT * INTO #TempTable{tempGuid}
                FROM FinalList FL
                SELECT *, Count(*) Over() TotalRows FROM #TempTable{tempGuid}
                ";

            sql += $@"
                    {paginationInfo.FilterBy}
                    {orderBy}
                    {paginationInfo.PageBy}";

            sql += $@" DROP TABLE #TempTable{tempGuid}";

            try
            {
                List<YarnAllocationChildItem> data = await _service.GetDataAsync<YarnAllocationChildItem>(sql);
                data.ForEach(x =>
                {
                    var values = CommonFunction.GetDefaultValueWhenInvalidS(x.AllocatedYarnDetails).Split(',');
                    x.AllocatedYarnDetails = string.Join(",", values.Distinct());
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

        /*
        public async Task<List<YarnAllocationChildPipelineItem>> GetPipelineStockAsync(int itemMasterId, int allocationChildID, string operationType, string yarnCount)
        {
            string tempGuid = CommonFunction.GetNewGuid();

            string tempQuery = "";
            if (operationType == "updateOperation")
            {
                tempQuery = " + ISNULL(YAC1.PipelineAllocationQty,0) ";
            }

            var sql = $@"
            WITH
            YTD AS(
	            Select QCRemarksMasterID = MAX(YRM.QCRemarksMasterID),YRMC.LotNo 
	            from YarnQCRemarksChild YRMC
	            INNER JOIN YarnQCRemarksMaster YRM ON YRM.QCRemarksMasterID = YRMC.QCRemarksMasterID
	            GROUP BY YRMC.LotNo
            ),
            YT AS(
	            Select CASE WHEN YRMC.Approve = 1 THEN 'Approve'
				            WHEN YRMC.Reject = 1 THEN 'Reject'
				            WHEN YRMC.Retest = 1 THEN 'Retest'
				            WHEN YRMC.Diagnostic = 1 THEN 'Diagnostic'
				            WHEN YRMC.CommerciallyApprove = 1 THEN 'CommerciallyApprove' 
				            ELSE '' END Status,YRMC.Remarks,YRMC.LotNo
	            from YarnQCRemarksChild YRMC
	            INNER JOIN YarnQCRemarksMaster YRM ON YRM.QCRemarksMasterID = YRMC.QCRemarksMasterID
	            INNER JOIN YTD ON YTD.LotNo=YRMC.LotNo AND YTD.QCRemarksMasterID = YRMC.QCRemarksMasterID 
            ),
            WithoutPipeLineRecord AS
            (
	            SELECT YSS.ItemMasterId, YSS.SupplierId, YSS.ShadeCode,
	            TotalCurrentStockQty = ISNULL(SUM(YSM.TotalCurrentStockQty),0),
	            TotalCurrentPipelineStockQty = ISNULL(SUM(YSM.TotalCurrentPipelineStockQty),0),
	            YarnAge = ISNULL(DATEDIFF(DD, YSS.YarnApprovedDate, GETDATE()),0)
	            FROM YarnStockSet YSS
	            INNER JOIN {TableNames.YarnStockMaster} YSM ON YSM.YarnStockSetId = YSS.YarnStockSetId
	            WHERE YSS.SpinnerId > 0
	            GROUP BY YSS.ItemMasterId, YSS.SupplierId, YSS.ShadeCode,DATEDIFF(DD, YSS.YarnApprovedDate, GETDATE())
            ),
            PipeLineRecord AS
            (
	            SELECT YSS.YarnStockSetId, YSS.ItemMasterId, YSS.SupplierId, YSS.ShadeCode, YSS.YarnCategory,
	            PipelineStockQty = ISNULL(SUM(YSM.TotalCurrentPipelineStockQty),0) - ISNULL(SUM(WPL.TotalCurrentStockQty),0),
	            WPL.YarnAge
	            FROM YarnStockSet YSS
	            INNER JOIN {TableNames.YarnStockMaster} YSM ON YSM.YarnStockSetId = YSS.YarnStockSetId	
	            LEFT JOIN WithoutPipeLineRecord WPL ON WPL.ItemMasterId = YSS.ItemMasterId
									                AND WPL.SupplierId = YSS.SupplierId
									                AND WPL.ShadeCode = YSS.ShadeCode
	            WHERE YSS.SpinnerId = 0
	            GROUP BY YSS.YarnStockSetId, YSS.ItemMasterId, YSS.SupplierId, YSS.ShadeCode, YSS.YarnCategory,WPL.YarnAge
            ),
            YAC AS
            (
	            SELECT YACI.YarnStockSetId, PipelineAllocationQty = ISNULL(SUM(YACI.PipelineAllocationQty),0),
	            PLR.YarnAge
	            FROM {TableNames.YARN_ALLOCATION_CHILD}PipelineItem YACI
	            INNER JOIN PipeLineRecord PLR ON PLR.YarnStockSetId = YACI.YarnStockSetId
	            WHERE YACI.AllocationChildID <> 0
	            GROUP BY YACI.YarnStockSetId,PLR.YarnAge
            ),
            YAC1 AS
            (
	            SELECT YACI.YarnStockSetId, PipelineAllocationQty = ISNULL(SUM(YACI.PipelineAllocationQty),0),
	            PLR.YarnAge
	            FROM {TableNames.YARN_ALLOCATION_CHILD}PipelineItem YACI
	            INNER JOIN PipeLineRecord PLR ON PLR.YarnStockSetId = YACI.YarnStockSetId
	            WHERE YACI.AllocationChildID = 0
	            GROUP BY YACI.YarnStockSetId,PLR.YarnAge
            ),
            FinalList AS
            (
	            SELECT PLR.YarnStockSetId,PLR.ItemMasterId,PLR.YarnCategory,PLR.ShadeCode, POPrice = 0,
                PipelineStockQty = ISNULL(PLR.PipelineStockQty,0) - ISNULL(YAC.PipelineAllocationQty,0) {tempQuery}
	            ,NumericCount = ISV6.SegmentValue, 
	            Spinner = CASE WHEN YSSWPL.SpinnerID > 0 THEN CCS.ShortName ELSE '' END,
	            YSSWPL.PhysicalCount, PhysicalLot = YSSWPL.YarnLotNo, YarnAge = ISNULL(PLR.YarnAge,0),
	            TestResult = YT.Status,TestResultComments = YT.Remarks,
	            YarnCount = ISV6.SegmentValue
	            FROM PipeLineRecord PLR
                LEFT JOIN YAC ON YAC.YarnStockSetId = PLR.YarnStockSetId
                LEFT JOIN YAC1 ON YAC1.YarnStockSetId = PLR.YarnStockSetId
	            LEFT JOIN {TableNames.YarnStockSet} YSSWPL ON YSSWPL.YarnStockSetId = PLR.YarnStockSetId
	            LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = PLR.ItemMasterID
	            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
	            LEFT JOIN {DbNames.EPYSL}..Contacts CCS ON CCS.ContactID = YSSWPL.SpinnerID
	            LEFT JOIN YT ON YT.LotNo = YSSWPL.YarnLotNo
            )
            SELECT * INTO #TempTable{tempGuid}
            FROM FinalList FL
            WHERE FL.PipelineStockQty > 0
            SELECT * FROM #TempTable{tempGuid}
            ORDER BY CASE WHEN YarnCount = '{yarnCount}' THEN 0 ELSE 1 END
            DROP TABLE #TempTable{tempGuid}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                List<YarnAllocationChildPipelineItem> data = records.Read<YarnAllocationChildPipelineItem>().ToList();
                Guard.Against.NullObject(data);
                int allocationChildPLItemID = 999;
                data.Where(c => c.AllocationChildPLItemID == 0).ToList().ForEach(c =>
                {
                    c.AllocationChildPLItemID = allocationChildPLItemID++;
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
        */

        public async Task<List<YarnAllocationChildItem>> GetAllocatedStockAsync(int id)
        {
            var sql = $@"With 
                    --YarnAge AS
                    --(
                    --	Select DATEDIFF(DD, MAX(YRM.ApproveDate), GETDATE()) Age,LotNo 
                    --	from YarnReceiveChild YRC
                    --	INNER JOIN YarnReceiveMaster YRM ON YRM.ReceiveID = YRC.ReceiveID
                    --	--WHERE YRC.ItemMasterID = 0
                    --	GROUP BY LotNo
                    --),
                    YTD AS
                    (
	                    Select MAX(YRM.QCRemarksMasterID) QCRemarksMasterID,LotNo 
	                    from YarnQCRemarksChild YRMC
	                    INNER JOIN YarnQCRemarksMaster YRM ON YRM.QCRemarksMasterID=YRMC.QCRemarksMasterID
	                    GROUP BY LotNo
                    ),
                    YT AS
                    (
	                    Select CASE WHEN Approve = 1 THEN 'Approve'
			                    WHEN Reject = 1 THEN 'Reject'
			                    WHEN Retest = 1 THEN 'Retest'
			                    WHEN Diagnostic = 1 THEN 'Diagnostic'
			                    WHEN CommerciallyApprove = 1 THEN 'CommerciallyApprove' 
			                    ELSE '' END Status,YRMC.Remarks,YRMC.LotNo
	                    from YarnQCRemarksChild YRMC
	                    INNER JOIN YarnQCRemarksMaster YRM ON YRM.QCRemarksMasterID=YRMC.QCRemarksMasterID
	                    INNER JOIN YTD ON YTD.LotNo=YRMC.LotNo AND YTD.QCRemarksMasterID=YRMC.QCRemarksMasterID 
                    ),
                    StockInfo AS
                    (
	                    SELECT YACI.YarnCategory, NumericCount = ISV6.SegmentValue, Spinner = CCS.ShortName,
	                    YSS.PhysicalCount, YSS.YarnLotNo PhysicalLot, POPrice = YSC.Rate,
                        YBCI.NetYarnReqQty,
	                    --YA.Age YarnAge,
	                    YarnAge = DATEDIFF(DAY, YSS.YarnApprovedDate, GETDATE()),
	                    YT.Status TestResult,
	                    YT.Remarks TestResultComments,YSS.YarnStockSetId,
                        YACI.QuarantineAllocationQty,
	                    YACI.AdvanceAllocationQty,
	                    YACI.SampleAllocationQty,
	                    YACI.LeftoverAllocationQty,
	                    YACI.LiabilitiesAllocationQty,
                        AdvanceStockQty = SUM(YSM.AdvanceStockQty) + ISNULL(YACI.AdvanceAllocationQty,0),
						SampleStockQty = SUM(YSM.SampleStockQty) + ISNULL(YACI.SampleAllocationQty,0),
						LeftoverStockQty = SUM(YSM.LeftoverStockQty) + ISNULL(YACI.LeftoverAllocationQty,0),		
						LiabilitiesStockQty = SUM(YSM.LiabilitiesStockQty) + ISNULL(YACI.LiabilitiesAllocationQty,0)
	                    FROM {TableNames.YARN_ALLOCATION_CHILD_ITEM} YACI
                        INNER JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.AllocationChildID = YACI.AllocationChildID
                        INNER JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBCI.YBChildItemID = YAC.YBChildItemID
	                    INNER JOIN {TableNames.YarnStockSet} YSS ON YSS.YarnStockSetId=YACI.YarnStockSetId
                        INNER JOIN {TableNames.YarnStockMaster} YSM ON YSM.YarnStockSetId = YSS.YarnStockSetId
	                    LEFT JOIN YarnStockChild YSC ON YSC.YarnStockSetId = YSS.YarnStockSetId AND YSC.StockFromTableId = {EnumStockFromTable.YarnAllocationChildItem} AND YSC.TransectionTypeId = {EnumTransectionType.Block} AND YSC.StockFromPKId = YACI.AllocationChildItemID
	                    INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YSS.ItemMasterID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
	                    INNER JOIN {DbNames.EPYSL}..Contacts CCS ON CCS.ContactID = YSS.SpinnerID
	                    --LEFT JOIN YarnAge YA ON YA.LotNo = YSS.YarnLotNo
	                    LEFT JOIN YT ON YT.LotNo = YSS.YarnLotNo
	                    WHERE YACI.AllocationChildID = {id}
                        GROUP BY  YACI.YarnCategory, ISV6.SegmentValue, CCS.ShortName,
	                    YSS.PhysicalCount, YSS.YarnLotNo, YSC.Rate,
                        YBCI.NetYarnReqQty,
	                    YSS.YarnApprovedDate, 
	                    YT.Status,
	                    YT.Remarks,YSS.YarnStockSetId,
                        YACI.QuarantineAllocationQty,
	                    YACI.AdvanceAllocationQty,
	                    YACI.SampleAllocationQty,
	                    YACI.LeftoverAllocationQty,
	                    YACI.LiabilitiesAllocationQty,
						YACI.AdvanceAllocationQty,
						YACI.SampleAllocationQty,
						YACI.LeftoverAllocationQty,		
						YACI.LiabilitiesAllocationQty
                    )
                    SELECT * FROM StockInfo";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                List<YarnAllocationChildItem> data = records.Read<YarnAllocationChildItem>().ToList();
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
        public async Task<List<YarnAllocationChildItem>> GetAllocatedStockByYBChildItemIDAsync(int yBChildItemID)
        {
            var sql = $@"With 
                    --YarnAge AS
                    --(
                    --	Select DATEDIFF(DD, MAX(YRM.ApproveDate), GETDATE()) Age,LotNo 
                    --	from YarnReceiveChild YRC
                    --	INNER JOIN YarnReceiveMaster YRM ON YRM.ReceiveID = YRC.ReceiveID
                    --	--WHERE YRC.ItemMasterID = 0
                    --	GROUP BY LotNo
                    --),
                    YTD AS
                    (
	                    Select MAX(YRM.QCRemarksMasterID) QCRemarksMasterID,LotNo 
	                    from YarnQCRemarksChild YRMC
	                    INNER JOIN YarnQCRemarksMaster YRM ON YRM.QCRemarksMasterID=YRMC.QCRemarksMasterID
	                    GROUP BY LotNo
                    ),
                    YT AS
                    (
	                    Select CASE WHEN Approve = 1 THEN 'Approve'
			                    WHEN Reject = 1 THEN 'Reject'
			                    WHEN Retest = 1 THEN 'Retest'
			                    WHEN Diagnostic = 1 THEN 'Diagnostic'
			                    WHEN CommerciallyApprove = 1 THEN 'CommerciallyApprove' 
			                    ELSE '' END Status,YRMC.Remarks,YRMC.LotNo
	                    from YarnQCRemarksChild YRMC
	                    INNER JOIN YarnQCRemarksMaster YRM ON YRM.QCRemarksMasterID=YRMC.QCRemarksMasterID
	                    INNER JOIN YTD ON YTD.LotNo=YRMC.LotNo AND YTD.QCRemarksMasterID=YRMC.QCRemarksMasterID 
                    ),
                    StockInfo AS
                    (
	                    SELECT YACI.YarnCategory, NumericCount = ISV6.SegmentValue, Spinner = CCS.ShortName,
	                    YSS.PhysicalCount, YSS.YarnLotNo PhysicalLot, POPrice = YSC.Rate,
	                    --YA.Age YarnAge,
	                    YarnAge = DATEDIFF(DAY, YSS.YarnApprovedDate, GETDATE()),
	                    YT.Status TestResult,
	                    YT.Remarks TestResultComments,YSS.YarnStockSetId,
	                    YACI.AdvanceAllocationQty,
	                    YACI.SampleAllocationQty,
	                    YACI.LeftoverAllocationQty,
	                    YACI.LiabilitiesAllocationQty,
                        AdvanceStockQty = SUM(YSM.AdvanceStockQty) + ISNULL(YACI.AdvanceAllocationQty,0),
						SampleStockQty = SUM(YSM.SampleStockQty) + ISNULL(YACI.SampleAllocationQty,0),
						LeftoverStockQty = SUM(YSM.LeftoverStockQty) + ISNULL(YACI.LeftoverAllocationQty,0),		
						LiabilitiesStockQty = SUM(YSM.LiabilitiesStockQty) + ISNULL(YACI.LiabilitiesAllocationQty,0)
	                    FROM {TableNames.YARN_ALLOCATION_CHILD_ITEM} YACI
                        INNER JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.AllocationChildID= YACI.AllocationChildID
	                    INNER JOIN {TableNames.YarnStockSet} YSS ON YSS.YarnStockSetId=YACI.YarnStockSetId
                        INNER JOIN {TableNames.YarnStockMaster} YSM ON YSM.YarnStockSetId = YSS.YarnStockSetId
	                    LEFT JOIN YarnStockChild YSC ON YSC.YarnStockSetId = YSS.YarnStockSetId AND YSC.StockFromTableId = {EnumStockFromTable.YarnAllocationChildItem} AND YSC.TransectionTypeId = {EnumTransectionType.Block} AND YSC.StockFromPKId = YACI.AllocationChildItemID
	                    INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YSS.ItemMasterID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
	                    INNER JOIN {DbNames.EPYSL}..Contacts CCS ON CCS.ContactID = YSS.SpinnerID
	                    --LEFT JOIN YarnAge YA ON YA.LotNo = YSS.YarnLotNo
	                    LEFT JOIN YT ON YT.LotNo = YSS.YarnLotNo
	                    WHERE YAC.YBChildItemID = {yBChildItemID}
                        GROUP BY  YACI.YarnCategory, ISV6.SegmentValue, CCS.ShortName,
	                    YSS.PhysicalCount, YSS.YarnLotNo, YSC.Rate,
	                    YSS.YarnApprovedDate, 
	                    YT.Status,
	                    YT.Remarks,YSS.YarnStockSetId,
	                    YACI.AdvanceAllocationQty,
	                    YACI.SampleAllocationQty,
	                    YACI.LeftoverAllocationQty,
	                    YACI.LiabilitiesAllocationQty,
						YACI.AdvanceAllocationQty,
						YACI.SampleAllocationQty,
						YACI.LeftoverAllocationQty,		
						YACI.LiabilitiesAllocationQty
                    )
                    SELECT * FROM StockInfo";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                List<YarnAllocationChildItem> data = records.Read<YarnAllocationChildItem>().ToList();
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
        public async Task<List<YarnAllocationChildPipelineItem>> GetAllocatedPipelineStockAsync(int id)
        {
            var sql = $@"
                    WITH
                    MainObj AS
                    (
	                    SELECT YACI.AllocationChildID, YAC.ItemMasterID
	                    FROM {TableNames.YARN_ALLOCATION_CHILD}PipelineItem YACI
	                    INNER JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.AllocationChildID = YACI.AllocationChildID
	                    WHERE YACI.AllocationChildID = {id}
                    ),
                    YarnAge AS(
	                    Select ISNULL(DATEDIFF(DD, MAX(YRM.ApprovedDate), GETDATE()),0) Age, YRC.LotNo 
	                    from YarnReceiveChild YRC
	                    INNER JOIN YarnReceiveMaster YRM ON YRM.ReceiveID = YRC.ReceiveID
	                    INNER JOIN MainObj MO ON MO.ItemMasterID = YRC.ItemMasterID
	                    GROUP BY YRC.LotNo
                    ),
                    YTD AS(
	                    Select QCRemarksMasterID = MAX(YRM.QCRemarksMasterID), YRMC.LotNo 
	                    from YarnQCRemarksChild YRMC
	                    INNER JOIN YarnQCRemarksMaster YRM ON YRM.QCRemarksMasterID = YRMC.QCRemarksMasterID
	                    INNER JOIN YarnReceiveChild YRC ON YRC.ChildID = YRMC.QCReceiveChildID
	                    INNER JOIN MainObj MO ON MO.ItemMasterID = YRC.ItemMasterID
	                    GROUP BY YRMC.LotNo
                    ),
                    YT AS(
	                    Select CASE WHEN YRMC.Approve = 1 THEN 'Approve'
				                    WHEN YRMC.Reject = 1 THEN 'Reject'
				                    WHEN YRMC.Retest = 1 THEN 'Retest'
				                    WHEN YRMC.Diagnostic = 1 THEN 'Diagnostic'
				                    WHEN YRMC.CommerciallyApprove = 1 THEN 'CommerciallyApprove' 
				                    ELSE '' END Status, YRMC.Remarks,YRMC.LotNo
	                    from YarnQCRemarksChild YRMC
	                    INNER JOIN YarnQCRemarksMaster YRM ON YRM.QCRemarksMasterID = YRMC.QCRemarksMasterID
	                    INNER JOIN YTD ON YTD.LotNo = YRMC.LotNo AND YTD.QCRemarksMasterID = YRMC.QCRemarksMasterID
	                    INNER JOIN YarnReceiveChild YRC ON YRC.ChildID = YRMC.QCReceiveChildID
	                    INNER JOIN MainObj MO ON MO.ItemMasterID = YRC.ItemMasterID
                    ),
                    StockInfo AS
                    (
                        SELECT YSS.YarnStockSetId, YACI.YarnCategory, NumericCount = ISV6.SegmentValue,
	                    Spinner = CASE WHEN ISNULL(YSS.SpinnerID,0) > 0 THEN CCS.ShortName ELSE '' END,
	                    YSS.PhysicalCount, PhysicalLot = YSS.YarnLotNo, POPrice = YSC.Rate, YarnAge = YA.Age,
	                    TestResult = YT.Status, TestResultComments = YT.Remarks,
	                    YACI.PipelineAllocationQty
	                    FROM {TableNames.YARN_ALLOCATION_CHILD}PipelineItem YACI
	                    INNER JOIN {TableNames.YarnStockSet} YSS ON YSS.YarnStockSetId = YACI.YarnStockSetId
	                    LEFT JOIN YarnStockChild YSC ON YSC.YarnStockSetId = YSS.YarnStockSetId AND YSC.StockFromTableId = 7 AND YSC.StockFromPKId = YACI.AllocationChildPLItemID
	                    INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YSS.ItemMasterID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
	                    INNER JOIN {DbNames.EPYSL}..Contacts CCS ON CCS.ContactID = YSS.SpinnerID
	                    LEFT JOIN YarnAge YA ON YA.LotNo = YSS.YarnLotNo
	                    LEFT JOIN YT ON YT.LotNo = YSS.YarnLotNo
	                    WHERE YACI.AllocationChildID = {id}

	                    /*SELECT YSS.YarnStockSetId, YACI.YarnCategory, NumericCount = ISV6.SegmentValue,
	                    Spinner = CASE WHEN ISNULL(YSSWPL.SpinnerID,0) > 0 THEN CCS.ShortName ELSE '' END,
	                    YSSWPL.PhysicalCount, PhysicalLot = YSSWPL.YarnLotNo, POPrice = YSC.Rate, YarnAge = YA.Age,
	                    TestResult = YT.Status, TestResultComments = YT.Remarks,
	                    YACI.PipelineAllocationQty
	                    FROM {TableNames.YARN_ALLOCATION_CHILD}PipelineItem YACI
	                    INNER JOIN {TableNames.YarnStockSet} YSS ON YSS.YarnStockSetId = YACI.YarnStockSetId
	                    LEFT JOIN {TableNames.YarnStockSet} YSSWPL ON YSSWPL.ItemMasterId = YSS.ItemMasterId
								                        AND YSSWPL.SupplierId = YSS.SupplierId
								                        AND YSSWPL.ShadeCode = YSS.ShadeCode
								                        AND YSSWPL.SpinnerId > 0
	                    LEFT JOIN YarnStockChild YSC ON YSC.YarnStockSetId = YSSWPL.YarnStockSetId AND YSC.StockFromTableId = {EnumStockFromTable.YarnAllocationChildPipelineItem} AND YSC.StockFromPKId = YACI.AllocationChildPLItemID
	                    INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YSS.ItemMasterID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
	                    INNER JOIN {DbNames.EPYSL}..Contacts CCS ON CCS.ContactID = YSSWPL.SpinnerID
	                    LEFT JOIN YarnAge YA ON YA.LotNo = YSSWPL.YarnLotNo
	                    LEFT JOIN YT ON YT.LotNo = YSSWPL.YarnLotNo
	                    WHERE YACI.AllocationChildID = {id}*/
                    )
                    SELECT * FROM StockInfo";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                List<YarnAllocationChildPipelineItem> data = records.Read<YarnAllocationChildPipelineItem>().ToList();
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
        public async Task<List<YarnAllocationChildPipelineItem>> GetAllocatedPipelineStockByYBChildItemIDAsync(int yBChildItemID)
        {
            var sql = $@"
                    WITH
                    MainObj AS
                    (
	                    SELECT YACI.AllocationChildID, YAC.ItemMasterID
	                    FROM {TableNames.YARN_ALLOCATION_CHILD}PipelineItem YACI
	                    INNER JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.AllocationChildID = YACI.AllocationChildID
	                    WHERE YAC.YBChildItemID = {yBChildItemID}
                    ),
                    YarnAge AS(
	                    Select ISNULL(DATEDIFF(DD, MAX(YRM.ApprovedDate), GETDATE()),0) Age, YRC.LotNo 
	                    from YarnReceiveChild YRC
	                    INNER JOIN YarnReceiveMaster YRM ON YRM.ReceiveID = YRC.ReceiveID
	                    INNER JOIN MainObj MO ON MO.ItemMasterID = YRC.ItemMasterID
	                    GROUP BY YRC.LotNo
                    ),
                    YTD AS(
	                    Select QCRemarksMasterID = MAX(YRM.QCRemarksMasterID), YRMC.LotNo 
	                    from YarnQCRemarksChild YRMC
	                    INNER JOIN YarnQCRemarksMaster YRM ON YRM.QCRemarksMasterID = YRMC.QCRemarksMasterID
	                    INNER JOIN YarnReceiveChild YRC ON YRC.ChildID = YRMC.QCReceiveChildID
	                    INNER JOIN MainObj MO ON MO.ItemMasterID = YRC.ItemMasterID
	                    GROUP BY YRMC.LotNo
                    ),
                    YT AS(
	                    Select CASE WHEN YRMC.Approve = 1 THEN 'Approve'
				                    WHEN YRMC.Reject = 1 THEN 'Reject'
				                    WHEN YRMC.Retest = 1 THEN 'Retest'
				                    WHEN YRMC.Diagnostic = 1 THEN 'Diagnostic'
				                    WHEN YRMC.CommerciallyApprove = 1 THEN 'CommerciallyApprove' 
				                    ELSE '' END Status, YRMC.Remarks,YRMC.LotNo
	                    from YarnQCRemarksChild YRMC
	                    INNER JOIN YarnQCRemarksMaster YRM ON YRM.QCRemarksMasterID = YRMC.QCRemarksMasterID
	                    INNER JOIN YTD ON YTD.LotNo = YRMC.LotNo AND YTD.QCRemarksMasterID = YRMC.QCRemarksMasterID
	                    INNER JOIN YarnReceiveChild YRC ON YRC.ChildID = YRMC.QCReceiveChildID
	                    INNER JOIN MainObj MO ON MO.ItemMasterID = YRC.ItemMasterID
                    ),
                    StockInfo AS
                    (
                        SELECT YSS.YarnStockSetId, YACI.YarnCategory, NumericCount = ISV6.SegmentValue,
	                    Spinner = CASE WHEN ISNULL(YSS.SpinnerID,0) > 0 THEN CCS.ShortName ELSE '' END,
	                    YSS.PhysicalCount, PhysicalLot = YSS.YarnLotNo, POPrice = YSC.Rate, YarnAge = YA.Age,
	                    TestResult = YT.Status, TestResultComments = YT.Remarks,
	                    YACI.PipelineAllocationQty
	                    FROM {TableNames.YARN_ALLOCATION_CHILD}PipelineItem YACI
                        INNER JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.AllocationChildID= YACI.AllocationChildID
	                    INNER JOIN {TableNames.YarnStockSet} YSS ON YSS.YarnStockSetId = YACI.YarnStockSetId
	                    LEFT JOIN YarnStockChild YSC ON YSC.YarnStockSetId = YSS.YarnStockSetId AND YSC.StockFromTableId = 7 AND YSC.StockFromPKId = YACI.AllocationChildPLItemID
	                    INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YSS.ItemMasterID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
	                    INNER JOIN {DbNames.EPYSL}..Contacts CCS ON CCS.ContactID = YSS.SpinnerID
	                    LEFT JOIN YarnAge YA ON YA.LotNo = YSS.YarnLotNo
	                    LEFT JOIN YT ON YT.LotNo = YSS.YarnLotNo
	                    WHERE YAC.YBChildItemID  = {yBChildItemID}

	                    
                    )
                    SELECT * FROM StockInfo";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                List<YarnAllocationChildPipelineItem> data = records.Read<YarnAllocationChildPipelineItem>().ToList();
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
        public async Task<List<YarnAllocationMaster>> GetByAllocationChildIds(string allocationChildIds)
        {
            var sql = $@"
                    SELECT YAM.*
                    FROM {TableNames.YARN_ALLOCATION_MASTER} YAM
                    INNER JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.AllocationID = YAM.YarnAllocationID
                    WHERE YAC.AllocationChildID IN ({allocationChildIds})

                    SELECT YAC.*
                    FROM {TableNames.YARN_ALLOCATION_CHILD} YAC
                    WHERE YAC.AllocationChildID IN ({allocationChildIds})

                    SELECT YACI.*
                    FROM {TableNames.YARN_ALLOCATION_CHILD_ITEM} YACI
                    INNER JOIN {TableNames.YARN_ALLOCATION_CHILD} YAC ON YAC.AllocationChildID = YACI.AllocationChildID
                    WHERE YAC.AllocationChildID IN ({allocationChildIds})";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                List<YarnAllocationMaster> yarnAllocations = records.Read<YarnAllocationMaster>().ToList();
                Guard.Against.NullObject(yarnAllocations);

                List<YarnAllocationChild> yarnAllocationChilds = records.Read<YarnAllocationChild>().ToList();
                List<YarnAllocationChildItem> yarnAllocationChildItems = records.Read<YarnAllocationChildItem>().ToList();

                yarnAllocations.ForEach(yam =>
                {
                    yam.Childs = yarnAllocationChilds.Where(x => x.AllocationID == yam.YarnAllocationID).ToList();
                    yam.Childs.ForEach(c =>
                    {
                        c.ChildItems = yarnAllocationChildItems.Where(x => x.AllocationChildID == c.AllocationChildID).ToList();
                    });
                });
                return yarnAllocations;
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
        public async Task SaveAsync(YarnAllocationMaster entity)
        {
            SqlTransaction transaction = null;
            try
            {
                _yarnAllocation = new YarnAllocationMaster();


                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                _sqlCom = new SqlCommand("CRUD_YarnAllocationMaster", _connection, transaction);
                _sqlCom.CommandType = CommandType.StoredProcedure;
                _sqlCom = this.SetParameters(_sqlCom, entity, entity.YarnAllocationID > 0 ? EnumOperationTypes.UPDATE : EnumOperationTypes.INSERT, userId);
                _connection.Close();

                _connection.Open();
                SqlDataReader reader = _sqlCom.ExecuteReader();
                while (reader.Read()) _yarnAllocation = this.Mapping(reader);


                transaction.Commit();
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                throw ex;
            }
            finally
            {
                transaction.Dispose();
                _connection.Close();
            }
        }

        public SqlCommand SetParameters(SqlCommand sqlCom, YarnAllocationMaster obj, EnumOperationTypes operationType, long userId)
        {
            sqlCom.Parameters.AddWithValue("@Id", oBasicPart.Id);
            sqlCom.Parameters.AddWithValue("@Name", oBasicPart.Name);
            sqlCom.Parameters.AddWithValue("@ShortName", oBasicPart.ShortName);
            sqlCom.Parameters.AddWithValue("@OperationType", operationType);
            return sqlCom;
        }
        */

        public async Task SaveAsync(YarnAllocationMaster entity)
        {
            SqlTransaction transaction = null;
            SqlTransaction transactionGmt = null;
            try
            {
                if (entity.IsRevise)
                {
                    await _service.ExecuteAsync("spBackupYarnAllocation_BK", new { YarnAllocationID = entity.YarnAllocationID }, 30, CommandType.StoredProcedure);
                }

                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                await _connectionGmt.OpenAsync();
                transactionGmt = _connectionGmt.BeginTransaction();

                #region Qty Validation Check

                bool hasError = false;
                string message = "";
                for (int i = 0; i < entity.Childs.Where(x => x.EntityState == EntityState.Added).Count(); i++)
                {
                    var child = entity.Childs[i];

                    #region Advance Qty Check
                    decimal childAdvanceQty = child.AdvanceStockAllocationQty;
                    decimal totalChildItemAdvanceQty = 0;
                    totalChildItemAdvanceQty += child.ChildItems.Where(x => x.EntityState == EntityState.Added).Sum(x => x.QuarantineAllocationQty);
                    totalChildItemAdvanceQty += child.ChildItems.Where(x => x.EntityState == EntityState.Added).Sum(x => x.AdvanceAllocationQty);
                    totalChildItemAdvanceQty += child.ChildItems.Where(x => x.EntityState == EntityState.Added).Sum(x => x.SampleAllocationQty);
                    totalChildItemAdvanceQty += child.ChildItems.Where(x => x.EntityState == EntityState.Added).Sum(x => x.LiabilitiesAllocationQty);
                    totalChildItemAdvanceQty += child.ChildItems.Where(x => x.EntityState == EntityState.Added).Sum(x => x.LeftoverAllocationQty);

                    if (childAdvanceQty != totalChildItemAdvanceQty)
                    {
                        hasError = true;
                        message = $@"Total allocation qty (KG) ({childAdvanceQty}) is mismatch with the total allocation summary qty ({totalChildItemAdvanceQty})";
                        break;
                    }
                    #endregion

                    #region Pipeline Qty Check
                    decimal childPipelineQty = child.PipelineStockAllocationQty;
                    decimal totalChildItemPipelineQty = 0;
                    totalChildItemPipelineQty += child.ChildPipelineItems.Where(x => x.EntityState == EntityState.Added).Sum(x => x.PipelineAllocationQty);
                    if (childPipelineQty != totalChildItemPipelineQty)
                    {
                        hasError = true;
                        message = $@"Total pipeline qty (KG) ({childPipelineQty}) is mismatch with the total pipeline allocation summary qty ({totalChildItemPipelineQty})";
                        break;
                    }
                    #endregion
                }
                if (hasError) throw new Exception(message);

                #endregion



                int maxChildId = 0;
                int maxChildDetailId = 0;
                var TotalChildDetails = 0;
                int maxChildPipelineDetailId = 0;
                var TotalChildPipelineDetails = 0;
                List<YarnAllocationChildItem> ChildDetails = new List<YarnAllocationChildItem>();
                List<YarnAllocationChildPipelineItem> ChildPipelineDetails = new List<YarnAllocationChildPipelineItem>();
                switch (entity.EntityState)
                {
                    case EntityState.Added:
                        entity.YarnAllocationID = await _service.GetMaxIdAsync(TableNames.YARN_ALLOCATION_MASTER, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        maxChildId = await _service.GetMaxIdAsync(TableNames.YARN_ALLOCATION_CHILD, entity.Childs.Count(), RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        entity.Childs.ForEach(m =>
                        {
                            if (m.AdvanceStockAllocationQty > 0 && m.ChildItems.Count == 0)
                            {
                                throw new Exception("Number of allocation popup item mismatch with allocation qty.");
                            }
                            else if (m.PipelineStockAllocationQty > 0 && m.ChildPipelineItems.Count == 0)
                            {
                                throw new Exception("Number of pipeline allocation popup item mismatch with pipeline allocation qty.");
                            }

                            TotalChildDetails += m.ChildItems.Count();
                            TotalChildPipelineDetails += m.ChildPipelineItems.Count();
                        });
                        maxChildDetailId = await _service.GetMaxIdAsync(TableNames.YARN_ALLOCATION_CHILD_ITEM, TotalChildDetails, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        maxChildPipelineDetailId = await _service.GetMaxIdAsync(TableNames.YARN_ALLOCATION_CHILD_PIPELINE_ITEM, TotalChildPipelineDetails, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

                        entity.Childs.ForEach(c =>
                        {
                            c.AllocationChildID = maxChildId++;
                            c.AllocationID = entity.YarnAllocationID;

                            c.ChildItems.ForEach(d =>
                            {
                                d.AllocationChildItemID = maxChildDetailId++;
                                d.AllocationChildID = c.AllocationChildID;
                                ChildDetails.Add(d);
                            });
                            c.ChildPipelineItems.ForEach(d =>
                            {
                                d.AllocationChildPLItemID = maxChildPipelineDetailId++;
                                d.AllocationChildID = c.AllocationChildID;
                                ChildPipelineDetails.Add(d);
                            });
                        });
                        break;

                    case EntityState.Modified:
                        maxChildId = await _service.GetMaxIdAsync(TableNames.YARN_ALLOCATION_CHILD, entity.Childs.Count(x => x.EntityState == EntityState.Added), RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        entity.Childs.ForEach(m =>
                        {
                            if (m.AdvanceStockAllocationQty > 0 && m.ChildItems.Count == 0)
                            {
                                throw new Exception("Number of allocation popup item mismatch with allocation qty.");
                            }
                            else if (m.PipelineStockAllocationQty > 0 && m.ChildPipelineItems.Count == 0)
                            {
                                throw new Exception("Number of pipeline allocation popup item mismatch with pipeline allocation qty.");
                            }

                            m.ChildItems.Where(x => x.EntityState == EntityState.Added).ToList().ForEach(md =>
                            {
                                TotalChildDetails += 1;
                            });
                            m.ChildPipelineItems.Where(x => x.EntityState == EntityState.Added).ToList().ForEach(md =>
                            {
                                TotalChildPipelineDetails += 1;
                            });
                        });
                        maxChildDetailId = await _service.GetMaxIdAsync(TableNames.YARN_ALLOCATION_CHILD_ITEM, TotalChildDetails, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        maxChildPipelineDetailId = await _service.GetMaxIdAsync(TableNames.YARN_ALLOCATION_CHILD_PIPELINE_ITEM, TotalChildPipelineDetails, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        entity.Childs.Where(x => x.EntityState == EntityState.Added).ToList().ForEach(c =>
                        {
                            c.AllocationChildID = maxChildId++;
                            c.AllocationID = entity.YarnAllocationID;
                            c.ChildItems.Where(x => x.EntityState == EntityState.Added).ToList().ForEach(d =>
                            {
                                d.AllocationChildItemID = maxChildDetailId++;
                                d.AllocationChildID = c.AllocationChildID;
                                ChildDetails.Add(d);
                            });
                            c.ChildItems.Where(x => x.EntityState == EntityState.Modified || x.EntityState == EntityState.Deleted).ToList().ForEach(d =>
                            {
                                ChildDetails.Add(d);
                            });
                            c.ChildPipelineItems.Where(x => x.EntityState == EntityState.Added).ToList().ForEach(d =>
                            {
                                d.AllocationChildPLItemID = maxChildPipelineDetailId++;
                                d.AllocationChildID = c.AllocationChildID;
                                ChildPipelineDetails.Add(d);
                            });
                            c.ChildPipelineItems.Where(x => x.EntityState == EntityState.Modified || x.EntityState == EntityState.Deleted).ToList().ForEach(d =>
                            {
                                ChildPipelineDetails.Add(d);
                            });
                        });
                        entity.Childs.Where(x => x.EntityState == EntityState.Modified).ToList().ForEach(c =>
                        {
                            c.ChildItems.Where(x => x.EntityState == EntityState.Added).ToList().ForEach(d =>
                            {
                                d.AllocationChildItemID = maxChildDetailId++;
                                d.AllocationChildID = c.AllocationChildID;
                                ChildDetails.Add(d);
                            });
                            c.ChildItems.Where(x => x.EntityState == EntityState.Modified || x.EntityState == EntityState.Deleted).ToList().ForEach(d =>
                            {
                                ChildDetails.Add(d);
                            });
                            c.ChildPipelineItems.Where(x => x.EntityState == EntityState.Added).ToList().ForEach(d =>
                            {
                                d.AllocationChildPLItemID = maxChildPipelineDetailId++;
                                d.AllocationChildID = c.AllocationChildID;
                                ChildPipelineDetails.Add(d);
                            });
                            c.ChildPipelineItems.Where(x => x.EntityState == EntityState.Modified || x.EntityState == EntityState.Deleted).ToList().ForEach(d =>
                            {
                                ChildPipelineDetails.Add(d);
                            });
                        });
                        break;

                    default:
                        break;
                }

                #region PR Operation
                if (entity.YarnPRMaster.EntityState == EntityState.Added)
                {
                    entity.YarnPRMaster.YarnPRMasterID = await _service.GetMaxIdAsync(TableNames.YARN_PR_MASTER, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                }

                int maxChildPRId = await _service.GetMaxIdAsync(TableNames.YARN_PR_CHILD, entity.YarnPRMaster.Childs.Count(x => x.EntityState == EntityState.Added), RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                foreach (YarnPRChild child in entity.YarnPRMaster.Childs.Where(x => x.EntityState == EntityState.Added).ToList())
                {
                    child.YarnPRChildID = maxChildPRId++;
                    child.YarnPRMasterID = entity.YarnPRMaster.YarnPRMasterID;
                }
                #endregion
                int userId = entity.EntityState == EntityState.Added ? entity.AddedBy : (int)entity.UpdatedBy;
                await _service.SaveSingleAsync(entity, transaction);

                await _service.SaveAsync(entity.Childs, transaction);
                //await _service.ValidationSingleAsync(entity, transaction, "sp_Validation_YarnAllocationMaster", entity.EntityState, userId, entity.YarnAllocationID);
                await _service.ExecuteAsync(SPNames.sp_Validation_YarnAllocationMaster, new { YarnAllocationID = entity.YarnAllocationID }, 30, CommandType.StoredProcedure);


                foreach (YarnAllocationChild item in entity.Childs.Where(x => x.EntityState == EntityState.Added || x.EntityState == EntityState.Modified))
                {
                    //await _service.ValidationSingleAsync(item, transaction, "sp_Validation_YarnAllocationChild", item.EntityState, userId, item.AllocationChildID);
                    await _service.ExecuteAsync(SPNames.sp_Validation_YarnAllocationChild, new { AllocationChildID = item.AllocationChildID }, 30, CommandType.StoredProcedure);
                }

                await _service.SaveAsync(ChildDetails, transaction);
                foreach (YarnAllocationChildItem item in ChildDetails.Where(x => x.EntityState == EntityState.Added || x.EntityState == EntityState.Modified))
                {
                    //await _service.ValidationSingleAsync(item, transaction, "sp_Validation_YarnAllocationChildItem", item.EntityState, userId, item.AllocationChildItemID);
                    await _service.ExecuteAsync(SPNames.sp_Validation_YarnAllocationChildItem, new { AllocationChildItemID = item.AllocationChildItemID }, 30, CommandType.StoredProcedure);
                }

                await _service.SaveAsync(ChildPipelineDetails, transaction);
                foreach (YarnAllocationChildPipelineItem item in ChildPipelineDetails.Where(x => x.EntityState == EntityState.Added || x.EntityState == EntityState.Modified))
                {
                    //await _service.ValidationSingleAsync(item, transaction, "sp_Validation_YarnAllocationChildPipelineItem", item.EntityState, userId, item.AllocationChildPLItemID);
                    await _service.ExecuteAsync(SPNames.sp_Validation_YarnAllocationChildPipelineItem, new { AllocationChildPLItemID = item.AllocationChildPLItemID }, 30, CommandType.StoredProcedure);
                }

                if (entity.YarnPRMaster.Childs.Count() > 0)
                {
                    await _connection.ExecuteAsync(SPNames.spBackupYarnAutoPR, new { YarnPRMasterID = entity.YarnPRMaster.YarnPRMasterID }, transaction, 30, CommandType.StoredProcedure);

                    await _service.SaveSingleAsync(entity.YarnPRMaster, transaction);
                    await _service.SaveAsync(entity.YarnPRMaster.Childs, transaction);
                    await _service.SaveAsync(entity.YarnPRMaster.YarnPOMasters, transaction);
                }

                #region Stock Operation

                if (entity.IsRevise)
                {
                    entity.Childs.ForEach(async x =>
                    {
                        await _connection.ExecuteAsync(SPNames.spUpdateAllocationChilditemPreProcessRevisionNo, new { AllocationID = x.AllocationID, YBookingNo = x.YBookingNo }, transaction, 30, CommandType.StoredProcedure);
                        //_connection.ExecuteAsync("spYarnBooking_BK", new { YBookingNo = yarnBookings[0].YBookingNo, IsFinalApprove = false, IsFinalReject = false, IsFabricRevision = true }, transaction, 30, CommandType.StoredProcedure);
                    });
                }

                if (entity.Reject)
                {
                    //userId = entity.EntityState == EntityState.Added ? entity.AddedBy : entity.RejectBy;
                    //await _connection.ExecuteAsync("spYarnStockOperation", new
                    //{
                    //    MasterID = entity.YarnAllocationID,
                    //    FromMenuType = EnumFromMenuType.YarnAllocationReject,
                    //    UserId = userId
                    //}, transaction, 30, CommandType.StoredProcedure);
                }
                else if (entity.IsRevise)
                {
                    //userId = entity.EntityState == EntityState.Added ? entity.AddedBy : entity.UpdatedBy;
                    //await _connection.ExecuteAsync("spYarnStockOperation", new { MasterID = entity.YarnAllocationID, FromMenuType = EnumFromMenuType.YarnAllocation, UserId = userId }, transaction, 30, CommandType.StoredProcedure);
                }
                else //if (entity.Approve)
                {
                    //userId = entity.EntityState == EntityState.Added ? entity.AddedBy : entity.ApproveBy;
                    //await _connection.ExecuteAsync("spYarnStockOperation", new { MasterID = entity.YarnAllocationID, FromMenuType = EnumFromMenuType.YarnAllocation, UserId = userId }, transaction, 30, CommandType.StoredProcedure);
                }
                #endregion Stock Operation

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
                transaction.Dispose();
                _connection.Close();
            }
        }
        public async Task SaveAllocation(YarnAllocationChild entityChild, bool IsReAllocation = false, bool IsUnAckRevise = false)
        {
            SqlTransaction transaction = null;
            SqlTransaction transactionGmt = null;
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                await _connectionGmt.OpenAsync();
                transactionGmt = _connectionGmt.BeginTransaction();
                if (entityChild.AdvanceStockAllocationQty > 0 && entityChild.ChildItems.Count == 0)
                {
                    throw new Exception("Number of allocation popup item mismatch with allocation qty.");
                }

                int maxChildItemId = await _service.GetMaxIdAsync(TableNames.YARN_ALLOCATION_CHILD_ITEM, entityChild.ChildItems.Count(x => x.EntityState == EntityState.Added), RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                entityChild.ChildItems.Where(x => x.EntityState == EntityState.Added).ToList().ForEach(x =>
                {
                    x.AllocationChildItemID = maxChildItemId++;
                });
                await _service.SaveSingleAsync(entityChild, transaction);

                if (IsUnAckRevise)
                {
                    entityChild.ChildItems.ForEach(x =>
                    {
                        if (x.EntityState == EntityState.Added) x.OperationType = (int)EnumOperationTypes.INSERT;
                        else if (x.EntityState == EntityState.Modified) x.OperationType = (int)EnumOperationTypes.UPDATE;
                        else if (x.EntityState == EntityState.Deleted) x.OperationType = (int)EnumOperationTypes.DELETE;
                    });
                }

                await _service.SaveAsync(entityChild.ChildItems, transaction);

                #region Stock Operation
                int userId = entityChild.UpdatedBy;
                if (IsReAllocation)
                {
                    //await _connection.ExecuteAsync("spYarnStockOperation", new { MasterID = entityChild.AllocationChildID, FromMenuType = EnumFromMenuType.YarnAllocationReallocation, UserId = userId }, transaction, 30, CommandType.StoredProcedure);
                }
                else if (IsUnAckRevise)
                {
                    //await _connection.ExecuteAsync("spYarnStockOperation", new { MasterID = entityChild.AllocationChildID, FromMenuType = EnumFromMenuType.YarnAllocationUnAckRevision, UserId = userId }, transaction, 30, CommandType.StoredProcedure);
                }
                #endregion Stock Operation

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
                transaction.Dispose();
                _connection.Close();
            }
        }
        public async Task UpdateItemAsync(YarnAllocationChildItem entity, List<FBookingAcknowledge> fbaList = null, List<YarnBookingMaster> ybmList = null, bool isAllocationInternalRevise = false)
        {
            SqlTransaction transaction = null;
            SqlTransaction transactionGmt = null;
            try
            {
                if (fbaList.IsNull()) fbaList = new List<FBookingAcknowledge>();
                if (ybmList.IsNull()) ybmList = new List<YarnBookingMaster>();
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                await _connectionGmt.OpenAsync();
                transactionGmt = _connectionGmt.BeginTransaction();
                await _service.SaveSingleAsync(entity, transaction);
                if (fbaList.Count() > 0)
                {
                    await _service.SaveAsync(fbaList, transaction);
                }
                if (ybmList.Count() > 0)
                {
                    await _service.SaveAsync(ybmList, transaction);
                }
                #region Stock Operation
                if (isAllocationInternalRevise == false)
                {
                    if (entity.Acknowledge)
                    {
                        //int userId = entity.AcknowledgeBy;
                        //await _connection.ExecuteAsync("spYarnStockOperation", new { MasterID = entity.AllocationChildItemID, FromMenuType = EnumFromMenuType.YarnAllocationAck, UserId = userId }, transaction, 30, CommandType.StoredProcedure);
                    }
                    else if (entity.UnAcknowledge)
                    {
                        //No need unacknowledge stock operation 

                        //int userId = entity.AcknowledgeBy;
                        //await _connection.ExecuteAsync("spYarnStockOperation", new { MasterID = entity.AllocationChildItemID, FromMenuType = EnumFromMenuType.YarnAllocationUnAck, UserId = userId }, transaction, 30, CommandType.StoredProcedure);
                    }
                }
                #endregion Stock Operation

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
                transaction.Dispose();
                _connection.Close();
            }
        }
        public async Task<string> GetMaxYarnAllocationNoAsync()
        {
            var id = await _service.GetMaxIdAsync(TableNames.YARN_ALLOCATION_NO, RepeatAfterEnum.EveryYear);
            var datePart = DateTime.Now.ToString("yyMMdd");
            return $@"{datePart}{id:00000}";
        }
    }
}
