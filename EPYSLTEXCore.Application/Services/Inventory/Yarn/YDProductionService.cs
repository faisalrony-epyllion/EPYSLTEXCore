using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Static;
using Microsoft.Data.SqlClient;
using EPYSLTEXCore.Application.Interfaces.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using Dapper;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Statics;
using System.Data.Entity;
using EPYSLTEXCore.Infrastructure.Exceptions;


namespace EPYSLTEXCore.Application.Services.Inventory.Yarn
{
    public class YDProductionService: IYDProductionService
    {
        private readonly IDapperCRUDService<YDProductionMaster> _service;

        SqlTransaction transactionGmt = null;
        private SqlTransaction transaction = null;
        private readonly SqlConnection _connection;
        private readonly SqlConnection _connectionGmt;
        public YDProductionService(IDapperCRUDService<YDProductionMaster> service)
        {


            _service = service;

            _service.Connection = service.GetConnection(AppConstants.GMT_CONNECTION);
            _connectionGmt = service.Connection;

            _service.Connection = service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _connection = service.Connection;

        }


        public async Task<List<YDProductionMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By YDProductionMasterID Desc" : paginationInfo.OrderBy;
            string sql;
            #region Previous SQL
            //        if (status == Status.Pending)
            //        {
            //            sql = $@";WITH M AS 
            //            (
            //                    --Select YDBCT.YDBookingMasterID, BM.YDBookingNo, BM.YDBookingDate, BM.BuyerID, BM.GroupConceptNo, 
            //                    --BM.Remarks, YDBCT.ColorId, YDBCT.ColorCode, YDBCT.ItemMasterID, YDBCT.UnitID
            //                    --From YDBookingChildTwisting YDBCT
            //                    --INNER Join YDBookingMaster BM On BM.YDBookingMasterID = YDBCT.YDBookingMasterID
            //                    --LEFT JOIN YDProductionChild YDC ON YDC.YDBCTwistingID = YDBCT.YDBCTwistingID
            //                    --Where BM.IsAcknowledge = 1 And YDC.YDBCTwistingID IS NULL
            //		Select YRDM.YDRecipeNo,YRRM.RecipeReqNo,YDBCT.YDBookingMasterID, BM.YDBookingNo, BM.YDBookingDate, BM.BuyerID, YRRM.GroupConceptNo, 
            //                 BM.Remarks, YDBCT.ColorId, YDBCT.ColorCode, YDBCT.ItemMasterID, YDBCT.UnitID
            //                    FROM YDRecipeDefinitionMaster YRDM
            //		LEFT JOIN YDRecipeRequestMaster YRRM ON YRRM.YDRecipeReqMasterID = YRDM.YDRecipeReqMasterID
            //		LEFT JOIN YDBookingChild YBC ON YBC.YDBookingChildID = YRRM.YDBookingChildID
            //		LEFT JOIN YDBookingMaster BM ON BM.YDBookingMasterID = YBC.YDBookingMasterID
            //		LEFT JOIN YDBookingChildTwisting YDBCT ON YDBCT.YDBookingMasterID = BM.YDBookingMasterID
            //		LEFT JOIN YDProductionChild YDC ON YDC.YDBCTwistingID = YDBCT.YDBCTwistingID
            //		WHERE YRDM.Acknowledged=1
            //            ),
            //            A AS
            //            (
            //                SELECT M.YDRecipeNo,M.RecipeReqNo,M.YDBookingMasterID, M.YDBookingNo, M.YDBookingDate, M.BuyerID, M.Remarks, CTO.Name Buyer,
            //                M.GroupConceptNo, M.ColorID, Color.SegmentValue AS ColorName, IM.ItemMasterID,
            //                ISV1.SegmentValueID Segment1ValueId, ISV2.SegmentValueID Segment2ValueId, ISV3.SegmentValueID Segment3ValueId, 
            //                ISV4.SegmentValueID Segment4ValueId, ISV5.SegmentValueID Segment5ValueId, ISV6.SegmentValueID Segment6ValueId, 
            //                ISV7.SegmentValueID Segment7ValueId, ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, 
            //                ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc, 
            //                ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc
            //                FROM M
            //                LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = M.BuyerID AND CTO.ContactID > 0
            //    --LEFT JOIN FreeConceptMaster FM ON FM.GroupConceptNo = M.GroupConceptNo
            //                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = M.ColorId
            //    LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = M.ItemMasterID
            //    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
            //    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
            //    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
            //    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
            //    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
            //    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
            //    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
            //    LEFT JOIN {DbNames.EPYSL}..Unit UN ON M.UnitID = UN.UnitID
            //                Group By M.YDRecipeNo,M.RecipeReqNo,M.YDBookingMasterID, M.YDBookingNo, M.YDBookingDate, M.BuyerID, M.Remarks, CTO.Name,
            //             M.GroupConceptNo, M.ColorID, Color.SegmentValue,IM.ItemMasterID, ISV1.SegmentValueID, ISV2.SegmentValueID,
            //                ISV3.SegmentValueID, ISV4.SegmentValueID, ISV5.SegmentValueID, ISV6.SegmentValueID, ISV7.SegmentValueID,
            //             ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue, ISV4.SegmentValue, ISV5.SegmentValue, ISV6.SegmentValue, 
            //                ISV7.SegmentValue 
            //            )
            //Select *, Count(*) Over() TotalRows from A ";

            //            orderBy = "ORDER BY YDBookingMasterID DESC";
            //        }
            //        else if (status == Status.UnApproved)
            //        {
            //            sql = $@"
            //            WITH M AS 
            //            (
            //             SELECT	PM.YDProductionMasterID, PM.YDProductionNo, PM.YDProductionDate, PM.YDBookingMasterID, 
            //             PM.BuyerID, PM.Remarks, PM.IsApprove, PM.IsAcknowledge,
            //             PM.ShiftID, PM.DMID, PM.OperatorID, PM.BatchNo
            //             FROM YDProductionMaster PM WHERE ISNULL(PM.IsApprove,0) = 0 AND ISNULL(PM.IsAcknowledge,0) = 0 
            //            ),  
            //            CHILD AS 
            //            (
            //             SELECT C.YDProductionChildID, C.YDProductionMasterID, C.ItemMasterID, C.ColorId, Color.SegmentValue ColorName,
            //             ISV1.SegmentValueID Segment1ValueId, ISV2.SegmentValueID Segment2ValueId, ISV3.SegmentValueID Segment3ValueId, 
            //                ISV4.SegmentValueID Segment4ValueId,
            //             ISV5.SegmentValueID Segment5ValueId, ISV6.SegmentValueID Segment6ValueId, ISV7.SegmentValueID Segment7ValueId,
            //             ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc,
            //             ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc
            //             FROM YDProductionChild C
            //             INNER JOIN M ON M.YDProductionMasterID = C.YDProductionMasterID
            //             LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = C.ColorId
            //             LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = C.ItemMasterID
            //             LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
            //             LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
            //             LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
            //             LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
            //             LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
            //             LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
            //             LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
            //             GROUP BY C.YDProductionChildID, C.YDProductionMasterID, C.ItemMasterID, C.ColorId, Color.SegmentValue,
            //             ISV1.SegmentValueID, ISV2.SegmentValueID, ISV3.SegmentValueID, ISV4.SegmentValueID,	ISV5.SegmentValueID,
            //             ISV6.SegmentValueID, ISV7.SegmentValueID, ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue,
            //             ISV4.SegmentValue,	ISV5.SegmentValue, ISV6.SegmentValue, ISV7.SegmentValue
            //            ), 
            //            A AS 
            //            (
            //             SELECT M.YDProductionMasterID, M.YDProductionNo, M.YDProductionDate, M.YDBookingMasterID, M.BuyerID, 
            //             M.Remarks, M.IsApprove, M.IsAcknowledge, FM.GroupConceptNo,
            //             BM.YDBookingNo, BM.YDBookingDate, CTO.Name Buyer, DM.DyeingMcslNo MCSLNo, M.BatchNo, E.DisplayEmployeeCode +' '+EmployeeName Operator, S.ShortName [Shift],
            //             STRING_AGG(CHILD.ColorName,',') ColorName, STRING_AGG(CHILD.Segment1ValueDesc,',') Segment1ValueDesc, STRING_AGG(CHILD.Segment2ValueDesc,',') Segment2ValueDesc,
            //             STRING_AGG(CHILD.Segment3ValueDesc,',') Segment3ValueDesc, STRING_AGG(CHILD.Segment4ValueDesc,',') Segment4ValueDesc, STRING_AGG(CHILD.Segment5ValueDesc,',') Segment5ValueDesc,
            //             STRING_AGG(CHILD.Segment6ValueDesc,',') Segment6ValueDesc, STRING_AGG(CHILD.Segment7ValueDesc,',') Segment7ValueDesc	
            //             FROM M
            //             INNER JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = M.BuyerID
            //             INNER JOIN YDBookingMaster BM ON BM.YDBookingMasterID = M.YDBookingMasterID
            //             LEFT JOIN FreeConceptMaster FM ON FM.GroupConceptNo = BM.GroupConceptNo
            //             INNER JOIN DyeingMachine DM ON DM.DMID = M.DMID
            //             INNER JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = M.OperatorID
            //             INNER JOIN {DbNames.EPYSL}..ShiftInfo S ON S.ShiftId = M.ShiftID
            //             INNER JOIN CHILD ON CHILD.YDProductionMasterID = M.YDProductionMasterID
            //             GROUP BY M.YDProductionMasterID, M.YDProductionNo, M.YDProductionDate, M.YDBookingMasterID, 
            //             M.BuyerID, M.Remarks, M.IsApprove, M.IsAcknowledge, FM.GroupConceptNo,
            //             BM.YDBookingNo, BM.YDBookingDate, CTO.[Name], DM.DyeingMcslNo, M.BatchNo, E.DisplayEmployeeCode, EmployeeName, 
            //             S.ShortName
            //            )
            //            Select *, Count(*) Over() TotalRows From A ";
            //        }
            //        else if (status == Status.Approved)
            //        {
            //            sql = $@"WITH M AS 
            //            (
            //                SELECT	PM.YDProductionMasterID, PM.YDProductionNo, PM.YDProductionDate, PM.YDBookingMasterID, 
            //                PM.BuyerID, PM.Remarks, PM.IsApprove, PM.IsAcknowledge, PM.ShiftID, PM.DMID, PM.OperatorID, PM.BatchNo
            //                FROM YDProductionMaster PM 
            //                WHERE ISNULL(PM.IsApprove,0) = 1 AND ISNULL(PM.IsAcknowledge,0) = 0
            //            ),  
            //            CHILD AS 
            //            (
            //             SELECT C.YDProductionChildID, C.YDProductionMasterID, C.ItemMasterID, C.ColorId, Color.SegmentValue ColorName,
            //             ISV1.SegmentValueID Segment1ValueId, ISV2.SegmentValueID Segment2ValueId, ISV3.SegmentValueID Segment3ValueId, 
            //                ISV4.SegmentValueID Segment4ValueId, ISV5.SegmentValueID Segment5ValueId, ISV6.SegmentValueID Segment6ValueId, 
            //                ISV7.SegmentValueID Segment7ValueId, ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, 
            //                ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc, 
            //                ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc
            //             FROM YDProductionChild C
            //             INNER JOIN M ON M.YDProductionMasterID = C.YDProductionMasterID
            //             LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = C.ColorId
            //             LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = C.ItemMasterID
            //             LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
            //             LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
            //             LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
            //             LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
            //             LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
            //             LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
            //             LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
            //             GROUP BY C.YDProductionChildID, C.YDProductionMasterID, C.ItemMasterID, C.ColorId, Color.SegmentValue,
            //             ISV1.SegmentValueID, ISV2.SegmentValueID, ISV3.SegmentValueID, ISV4.SegmentValueID,	ISV5.SegmentValueID,
            //             ISV6.SegmentValueID, ISV7.SegmentValueID, ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue,
            //             ISV4.SegmentValue,	ISV5.SegmentValue, ISV6.SegmentValue, ISV7.SegmentValue
            //            ), 
            //            A AS 
            //            (
            //             SELECT M.YDProductionMasterID, M.YDProductionNo, M.YDProductionDate, M.YDBookingMasterID, M.BuyerID, 
            //                M.Remarks, M.IsApprove, M.IsAcknowledge,FM.GroupConceptNo, BM.YDBookingNo, BM.YDBookingDate, CTO.Name Buyer, 
            //                DM.DyeingMcslNo MCSLNo, M.BatchNo, E.DisplayEmployeeCode +' '+EmployeeName Operator, S.ShortName [Shift],
            //             STRING_AGG(CHILD.ColorName,',') ColorName, STRING_AGG(CHILD.Segment1ValueDesc,',') Segment1ValueDesc, 
            //                STRING_AGG(CHILD.Segment2ValueDesc,',') Segment2ValueDesc, STRING_AGG(CHILD.Segment3ValueDesc,',') Segment3ValueDesc, 
            //                STRING_AGG(CHILD.Segment4ValueDesc,',') Segment4ValueDesc, STRING_AGG(CHILD.Segment5ValueDesc,',') Segment5ValueDesc,
            //             STRING_AGG(CHILD.Segment6ValueDesc,',') Segment6ValueDesc, STRING_AGG(CHILD.Segment7ValueDesc,',') Segment7ValueDesc
            //             FROM M
            //             INNER JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = M.BuyerID
            //             INNER JOIN YDBookingMaster BM ON BM.YDBookingMasterID = M.YDBookingMasterID
            //             LEFT JOIN FreeConceptMaster FM ON FM.GroupConceptNo = BM.GroupConceptNo
            //             INNER JOIN DyeingMachine DM ON DM.DMID = M.DMID
            //             INNER JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = M.OperatorID
            //             INNER JOIN {DbNames.EPYSL}..ShiftInfo S ON S.ShiftId = M.ShiftID
            //             INNER JOIN CHILD ON CHILD.YDProductionMasterID = M.YDProductionMasterID
            //             GROUP BY M.YDProductionMasterID, M.YDProductionNo, M.YDProductionDate, M.YDBookingMasterID, M.BuyerID, 
            //                M.Remarks, M.IsApprove, M.IsAcknowledge,FM.GroupConceptNo, BM.YDBookingNo, BM.YDBookingDate, CTO.[Name], 
            //                DM.DyeingMcslNo, M.BatchNo, E.DisplayEmployeeCode, EmployeeName, S.ShortName
            //            )
            //            Select *, Count(*) Over() TotalRows from A";
            //        }
            //        else if (status == Status.Acknowledge)
            //        {
            //            sql = $@"WITH M AS 
            //            (
            //                SELECT	PM.YDProductionMasterID, PM.YDProductionNo, PM.YDProductionDate, PM.YDBookingMasterID, 
            //                PM.BuyerID, PM.Remarks, PM.IsApprove, PM.IsAcknowledge, PM.ShiftID, PM.DMID, PM.OperatorID, PM.BatchNo
            //                FROM YDProductionMaster PM 
            //                WHERE ISNULL(PM.IsApprove,0) = 1 AND ISNULL(PM.IsAcknowledge,0) = 1
            //            ),  
            //            CHILD AS 
            //            (
            //             SELECT C.YDProductionChildID, C.YDProductionMasterID, C.ItemMasterID, C.ColorId, Color.SegmentValue ColorName,
            //             ISV1.SegmentValueID Segment1ValueId, ISV2.SegmentValueID Segment2ValueId, ISV3.SegmentValueID Segment3ValueId, ISV4.SegmentValueID Segment4ValueId,
            //             ISV5.SegmentValueID Segment5ValueId, ISV6.SegmentValueID Segment6ValueId, ISV7.SegmentValueID Segment7ValueId,
            //             ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc,
            //             ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc
            //             FROM YDProductionChild C
            //             INNER JOIN M ON M.YDProductionMasterID = C.YDProductionMasterID
            //             LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = C.ColorId
            //             LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = C.ItemMasterID
            //             LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
            //             LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
            //             LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
            //             LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
            //             LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
            //             LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
            //             LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
            //             GROUP BY C.YDProductionChildID, C.YDProductionMasterID, C.ItemMasterID, C.ColorId, Color.SegmentValue,
            //             ISV1.SegmentValueID, ISV2.SegmentValueID, ISV3.SegmentValueID, ISV4.SegmentValueID,	ISV5.SegmentValueID,
            //             ISV6.SegmentValueID, ISV7.SegmentValueID, ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue,
            //             ISV4.SegmentValue,	ISV5.SegmentValue, ISV6.SegmentValue, ISV7.SegmentValue
            //            ), 
            //            A AS 
            //            (
            //             SELECT M.YDProductionMasterID, M.YDProductionNo, M.YDProductionDate, M.YDBookingMasterID, M.BuyerID, 
            //                M.Remarks, M.IsApprove, M.IsAcknowledge, FM.GroupConceptNo, BM.YDBookingNo, BM.YDBookingDate, 
            //                CTO.Name Buyer, DM.DyeingMcslNo MCSLNo, M.BatchNo, E.DisplayEmployeeCode +' '+EmployeeName Operator, S.ShortName [Shift],
            //             STRING_AGG(CHILD.ColorName,',') ColorName, STRING_AGG(CHILD.Segment1ValueDesc,',') Segment1ValueDesc, STRING_AGG(CHILD.Segment2ValueDesc,',') Segment2ValueDesc,
            //             STRING_AGG(CHILD.Segment3ValueDesc,',') Segment3ValueDesc, STRING_AGG(CHILD.Segment4ValueDesc,',') Segment4ValueDesc, STRING_AGG(CHILD.Segment5ValueDesc,',') Segment5ValueDesc,
            //             STRING_AGG(CHILD.Segment6ValueDesc,',') Segment6ValueDesc, STRING_AGG(CHILD.Segment7ValueDesc,',') Segment7ValueDesc
            //             FROM M
            //             INNER JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = M.BuyerID
            //             INNER JOIN YDBookingMaster BM ON BM.YDBookingMasterID = M.YDBookingMasterID
            //             LEFT JOIN FreeConceptMaster FM ON FM.GroupConceptNo = BM.GroupConceptNo
            //             INNER JOIN DyeingMachine DM ON DM.DMID = M.DMID
            //             INNER JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = M.OperatorID
            //             INNER JOIN {DbNames.EPYSL}..ShiftInfo S ON S.ShiftId = M.ShiftID
            //             INNER JOIN CHILD ON CHILD.YDProductionMasterID = M.YDProductionMasterID
            //             GROUP BY M.YDProductionMasterID, M.YDProductionNo, M.YDProductionDate, M.YDBookingMasterID, M.BuyerID, 
            //                M.Remarks, M.IsApprove, M.IsAcknowledge,FM.GroupConceptNo,
            //             BM.YDBookingNo, BM.YDBookingDate, CTO.[Name], DM.DyeingMcslNo, M.BatchNo, E.DisplayEmployeeCode, EmployeeName, S.ShortName
            //            )
            //            Select *, Count(*) Over() TotalRows from A";
            //        }
            //        else
            //        {
            //            sql = $@"WITH M AS 
            //            (
            //                SELECT	PM.YDProductionMasterID, PM.YDProductionNo, PM.YDProductionDate, PM.YDBookingMasterID, PM.BuyerID, PM.Remarks, PM.IsApprove, PM.IsAcknowledge,
            //             PM.ShiftID, PM.DMID, PM.OperatorID, PM.BatchNo
            //                FROM YDProductionMaster PM 
            //                WHERE ISNULL(PM.IsApprove,0) = 1 AND ISNULL(PM.IsAcknowledge,0) = 1
            //            ),  
            //            CHILD AS 
            //            (
            //             SELECT C.YDProductionChildID, C.YDProductionMasterID, C.ItemMasterID, C.ColorId, Color.SegmentValue ColorName,
            //             ISV1.SegmentValueID Segment1ValueId, ISV2.SegmentValueID Segment2ValueId, ISV3.SegmentValueID Segment3ValueId, ISV4.SegmentValueID Segment4ValueId,
            //             ISV5.SegmentValueID Segment5ValueId, ISV6.SegmentValueID Segment6ValueId, ISV7.SegmentValueID Segment7ValueId,
            //             ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc,
            //             ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc
            //             FROM YDProductionChild C
            //             INNER JOIN M ON M.YDProductionMasterID = C.YDProductionMasterID
            //             LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = C.ColorId
            //             LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = C.ItemMasterID
            //             LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
            //             LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
            //             LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
            //             LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
            //             LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
            //             LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
            //             LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
            //             GROUP BY C.YDProductionChildID, C.YDProductionMasterID, C.ItemMasterID, C.ColorId, Color.SegmentValue,
            //             ISV1.SegmentValueID, ISV2.SegmentValueID, ISV3.SegmentValueID, ISV4.SegmentValueID,	ISV5.SegmentValueID,
            //             ISV6.SegmentValueID, ISV7.SegmentValueID, ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue,
            //             ISV4.SegmentValue,	ISV5.SegmentValue, ISV6.SegmentValue, ISV7.SegmentValue
            //            ), 
            //            A AS 
            //            (
            //             SELECT M.YDProductionMasterID, M.YDProductionNo, M.YDProductionDate, M.YDBookingMasterID, M.BuyerID, 
            //                M.Remarks, M.IsApprove, M.IsAcknowledge,FM.GroupConceptNo,
            //             BM.YDBookingNo, BM.YDBookingDate, CTO.Name Buyer, DM.DyeingMcslNo MCSLNo, M.BatchNo, E.DisplayEmployeeCode +' '+EmployeeName Operator, S.ShortName [Shift],
            //             STRING_AGG(CHILD.ColorName,',') ColorName, STRING_AGG(CHILD.Segment1ValueDesc,',') Segment1ValueDesc, STRING_AGG(CHILD.Segment2ValueDesc,',') Segment2ValueDesc,
            //             STRING_AGG(CHILD.Segment3ValueDesc,',') Segment3ValueDesc, STRING_AGG(CHILD.Segment4ValueDesc,',') Segment4ValueDesc, STRING_AGG(CHILD.Segment5ValueDesc,',') Segment5ValueDesc,
            //             STRING_AGG(CHILD.Segment6ValueDesc,',') Segment6ValueDesc, STRING_AGG(CHILD.Segment7ValueDesc,',') Segment7ValueDesc
            //             FROM M
            //             INNER JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = M.BuyerID
            //             INNER JOIN YDBookingMaster BM ON BM.YDBookingMasterID = M.YDBookingMasterID
            //             LEFT JOIN FreeConceptMaster FM ON FM.GroupConceptNo = BM.GroupConceptNo
            //             INNER JOIN DyeingMachine DM ON DM.DMID = M.DMID
            //             INNER JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = M.OperatorID
            //             INNER JOIN {DbNames.EPYSL}..ShiftInfo S ON S.ShiftId = M.ShiftID
            //             INNER JOIN CHILD ON CHILD.YDProductionMasterID = M.YDProductionMasterID
            //             GROUP BY M.YDProductionMasterID, M.YDProductionNo, M.YDProductionDate, M.YDBookingMasterID, M.BuyerID, 
            //                M.Remarks, M.IsApprove, M.IsAcknowledge,FM.GroupConceptNo,
            //             BM.YDBookingNo, BM.YDBookingDate, CTO.[Name], DM.DyeingMcslNo, M.BatchNo, E.DisplayEmployeeCode, EmployeeName, S.ShortName
            //            )
            //            Select *, Count(*) Over() TotalRows from A";
            //        }
            #endregion

            if (status == Status.Pending)
            {
                sql = $@";WITH M AS 
                (
                        SELECT 
                        YRDM.YDRecipeNo,YRRM.RecipeReqNo,YRDM.YDRecipeID,
                        YDBM.YDDBatchNo,YDBM.YDDBatchID,
                        YBC.YDBookingMasterID, BM.YDBookingNo, BM.YDBookingDate, BM.BuyerID, YRRM.GroupConceptNo, 
                        BM.Remarks, YBC.ColorId, YBC.ColorCode, YBC.ItemMasterID, YBC.UnitID 
                        FROM YDBookingMaster BM
                        INNER JOIN YDBookingChild YBC ON YBC.YDBookingMasterID = BM.YDBookingMasterID
                        INNER JOIN YDRecipeRequestMaster YRRM ON YRRM.YDBookingChildID = YBC.YDBookingChildID
                        INNER JOIN YDRecipeDefinitionMaster YRDM ON YRDM.YDRecipeReqMasterID = YRRM.YDRecipeReqMasterID
                        INNER JOIN YDBatchItemRequirement YDBIR ON YDBIR.YDBookingChildID=YBC.YDBookingChildID
                        INNER JOIN YDDyeingBatchItem YDDBI ON YDDBI.YDBItemReqID=YDBIR.YDBItemReqID
                        INNER JOIN YDDyeingBatchMaster YDBM ON YDBM.YDDBatchID = YDDBI.YDDBatchID
                        LEFT JOIN YDProductionMaster YDM ON YDM.YDDBatchID = YDBM.YDDBatchID
                        WHERE YRDM.Acknowledged=1 AND YDM.YDProductionMasterID IS NULL
                ),
                A AS
                (
                    SELECT M.YDDBatchNo,M.YDDBatchID,M.YDRecipeID,M.YDRecipeNo,M.RecipeReqNo,M.YDBookingMasterID, M.YDBookingNo, M.YDBookingDate, M.BuyerID, M.Remarks, CTO.Name Buyer,
                    M.GroupConceptNo, M.ColorID, Color.SegmentValue AS ColorName, IM.ItemMasterID,
                    ISV1.SegmentValueID Segment1ValueId, ISV2.SegmentValueID Segment2ValueId, ISV3.SegmentValueID Segment3ValueId, 
                    ISV4.SegmentValueID Segment4ValueId, ISV5.SegmentValueID Segment5ValueId, ISV6.SegmentValueID Segment6ValueId, 
                    ISV7.SegmentValueID Segment7ValueId, ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, 
                    ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc, 
                    ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc
                    FROM M
                    LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = M.BuyerID AND CTO.ContactID > 0
				    --LEFT JOIN FreeConceptMaster FM ON FM.GroupConceptNo = M.GroupConceptNo
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = M.ColorId
				    LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = M.ItemMasterID
				    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
				    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
				    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
				    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
				    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
				    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
				    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
				    LEFT JOIN {DbNames.EPYSL}..Unit UN ON M.UnitID = UN.UnitID
                    Group By M.YDDBatchNo,M.YDDBatchID,M.YDRecipeID,M.YDRecipeNo,M.RecipeReqNo,M.YDBookingMasterID, M.YDBookingNo, M.YDBookingDate, M.BuyerID, M.Remarks, CTO.Name,
	                M.GroupConceptNo, M.ColorID, Color.SegmentValue,IM.ItemMasterID, ISV1.SegmentValueID, ISV2.SegmentValueID,
                    ISV3.SegmentValueID, ISV4.SegmentValueID, ISV5.SegmentValueID, ISV6.SegmentValueID, ISV7.SegmentValueID,
	                ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue, ISV4.SegmentValue, ISV5.SegmentValue, ISV6.SegmentValue, 
                    ISV7.SegmentValue 
                )
				Select *, Count(*) Over() TotalRows from A ";

                orderBy = "ORDER BY YDBookingMasterID DESC";
            }
            else if (status == Status.UnApproved)
            {
                sql = $@"
                WITH M AS 
                (
	                SELECT	PM.YDProductionMasterID, 
					YRDM.YDRecipeNo,YRDM.YDRecipeID,YDBM.YDDBatchNo,PM.YDDBatchID,
					PM.YDProductionNo, PM.YDProductionDate, PM.YDBookingMasterID, 
	                PM.BuyerID, PM.Remarks, PM.IsApprove, PM.IsAcknowledge,
	                PM.ShiftID, PM.DMID, PM.OperatorID, PM.BatchNo
	                FROM YDProductionMaster PM 
					--INNER JOIN YDBookingChild YBC ON YBC.YDBookingMasterID = PM.YDBookingMasterID
					INNER JOIN YDRecipeDefinitionMaster YRDM ON YRDM.YDRecipeID= PM.YDRecipeID
					INNER JOIN YDDyeingBatchMaster YDBM ON YDBM.YDDBatchID = PM.YDDBatchID
					INNER JOIN YDProductionMaster YDM ON YDM.YDDBatchID = YDBM.YDDBatchID
					WHERE ISNULL(PM.IsApprove,0) = 0 AND ISNULL(PM.IsAcknowledge,0) = 0 
                ),  
                CHILD AS 
                (
	                SELECT C.YDProductionChildID, C.YDProductionMasterID, C.ItemMasterID, C.ColorId, Color.SegmentValue ColorName,
	                ISV1.SegmentValueID Segment1ValueId, ISV2.SegmentValueID Segment2ValueId, ISV3.SegmentValueID Segment3ValueId, 
                    ISV4.SegmentValueID Segment4ValueId,
	                ISV5.SegmentValueID Segment5ValueId, ISV6.SegmentValueID Segment6ValueId, ISV7.SegmentValueID Segment7ValueId,
	                ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc,
	                ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc
	                FROM YDProductionChild C
	                INNER JOIN M ON M.YDProductionMasterID = C.YDProductionMasterID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = C.ColorId
	                LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = C.ItemMasterID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
	                GROUP BY C.YDProductionChildID, C.YDProductionMasterID, C.ItemMasterID, C.ColorId, Color.SegmentValue,
	                ISV1.SegmentValueID, ISV2.SegmentValueID, ISV3.SegmentValueID, ISV4.SegmentValueID,	ISV5.SegmentValueID,
	                ISV6.SegmentValueID, ISV7.SegmentValueID, ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue,
	                ISV4.SegmentValue,	ISV5.SegmentValue, ISV6.SegmentValue, ISV7.SegmentValue
                ), 
                A AS 
                (
	                SELECT M.YDProductionMasterID, 
					M.YDRecipeNo,M.YDRecipeID,M.YDDBatchNo,M.YDDBatchID,
					M.YDProductionNo, M.YDProductionDate, M.YDBookingMasterID, M.BuyerID, 
	                M.Remarks, M.IsApprove, M.IsAcknowledge, BM.GroupConceptNo,
	                BM.YDBookingNo, BM.YDBookingDate, CTO.Name Buyer, DM.DyeingMcslNo MCSLNo, M.BatchNo, E.DisplayEmployeeCode +' '+EmployeeName Operator, S.ShortName [Shift],
	                STRING_AGG(CHILD.ColorName,',') ColorName, STRING_AGG(CHILD.Segment1ValueDesc,',') Segment1ValueDesc, STRING_AGG(CHILD.Segment2ValueDesc,',') Segment2ValueDesc,
	                STRING_AGG(CHILD.Segment3ValueDesc,',') Segment3ValueDesc, STRING_AGG(CHILD.Segment4ValueDesc,',') Segment4ValueDesc, STRING_AGG(CHILD.Segment5ValueDesc,',') Segment5ValueDesc,
	                STRING_AGG(CHILD.Segment6ValueDesc,',') Segment6ValueDesc, STRING_AGG(CHILD.Segment7ValueDesc,',') Segment7ValueDesc	
	                FROM M
	                LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = M.BuyerID AND CTO.ContactID > 0
	                INNER JOIN YDBookingMaster BM ON BM.YDBookingMasterID = M.YDBookingMasterID
	                --LEFT JOIN FreeConceptMaster FM ON FM.GroupConceptNo = BM.GroupConceptNo
	                INNER JOIN DyeingMachine DM ON DM.DMID = M.DMID
	                INNER JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = M.OperatorID
	                INNER JOIN {DbNames.EPYSL}..ShiftInfo S ON S.ShiftId = M.ShiftID
	                INNER JOIN CHILD ON CHILD.YDProductionMasterID = M.YDProductionMasterID
	                GROUP BY M.YDProductionMasterID, M.YDRecipeNo,M.YDRecipeID,M.YDDBatchNo,M.YDDBatchID,M.YDProductionNo, M.YDProductionDate, M.YDBookingMasterID, 
	                M.BuyerID, M.Remarks, M.IsApprove, M.IsAcknowledge, BM.GroupConceptNo,
	                BM.YDBookingNo, BM.YDBookingDate, CTO.[Name], DM.DyeingMcslNo, M.BatchNo, E.DisplayEmployeeCode, EmployeeName, 
	                S.ShortName
                )
                Select *, Count(*) Over() TotalRows From A  ";
            }
            else if (status == Status.Approved)
            {
                sql = $@"WITH M AS 
                (
	                SELECT	PM.YDProductionMasterID, 
					YRDM.YDRecipeNo,YRDM.YDRecipeID,YDBM.YDDBatchNo,PM.YDDBatchID,
					PM.YDProductionNo, PM.YDProductionDate, PM.YDBookingMasterID, 
	                PM.BuyerID, PM.Remarks, PM.IsApprove, PM.IsAcknowledge,
	                PM.ShiftID, PM.DMID, PM.OperatorID, PM.BatchNo
	                FROM YDProductionMaster PM 
					--INNER JOIN YDBookingChild YBC ON YBC.YDBookingMasterID = PM.YDBookingMasterID
					INNER JOIN YDRecipeDefinitionMaster YRDM ON YRDM.YDRecipeID= PM.YDRecipeID
					INNER JOIN YDDyeingBatchMaster YDBM ON YDBM.YDDBatchID = PM.YDDBatchID
					INNER JOIN YDProductionMaster YDM ON YDM.YDDBatchID = YDBM.YDDBatchID
					WHERE ISNULL(PM.IsApprove,0) = 1 AND ISNULL(PM.IsAcknowledge,0) = 0 
                ),  
                CHILD AS 
                (
	                SELECT C.YDProductionChildID, C.YDProductionMasterID, C.ItemMasterID, C.ColorId, Color.SegmentValue ColorName,
	                ISV1.SegmentValueID Segment1ValueId, ISV2.SegmentValueID Segment2ValueId, ISV3.SegmentValueID Segment3ValueId, 
                    ISV4.SegmentValueID Segment4ValueId,
	                ISV5.SegmentValueID Segment5ValueId, ISV6.SegmentValueID Segment6ValueId, ISV7.SegmentValueID Segment7ValueId,
	                ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc,
	                ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc
	                FROM YDProductionChild C
	                INNER JOIN M ON M.YDProductionMasterID = C.YDProductionMasterID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = C.ColorId
	                LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = C.ItemMasterID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
	                GROUP BY C.YDProductionChildID, C.YDProductionMasterID, C.ItemMasterID, C.ColorId, Color.SegmentValue,
	                ISV1.SegmentValueID, ISV2.SegmentValueID, ISV3.SegmentValueID, ISV4.SegmentValueID,	ISV5.SegmentValueID,
	                ISV6.SegmentValueID, ISV7.SegmentValueID, ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue,
	                ISV4.SegmentValue,	ISV5.SegmentValue, ISV6.SegmentValue, ISV7.SegmentValue
                ), 
                A AS 
                (
	                SELECT M.YDProductionMasterID, 
					M.YDRecipeNo,M.YDRecipeID,M.YDDBatchNo,M.YDDBatchID,
					M.YDProductionNo, M.YDProductionDate, M.YDBookingMasterID, M.BuyerID, 
	                M.Remarks, M.IsApprove, M.IsAcknowledge, BM.GroupConceptNo,
	                BM.YDBookingNo, BM.YDBookingDate, CTO.Name Buyer, DM.DyeingMcslNo MCSLNo, M.BatchNo, E.DisplayEmployeeCode +' '+EmployeeName Operator, S.ShortName [Shift],
	                STRING_AGG(CHILD.ColorName,',') ColorName, STRING_AGG(CHILD.Segment1ValueDesc,',') Segment1ValueDesc, STRING_AGG(CHILD.Segment2ValueDesc,',') Segment2ValueDesc,
	                STRING_AGG(CHILD.Segment3ValueDesc,',') Segment3ValueDesc, STRING_AGG(CHILD.Segment4ValueDesc,',') Segment4ValueDesc, STRING_AGG(CHILD.Segment5ValueDesc,',') Segment5ValueDesc,
	                STRING_AGG(CHILD.Segment6ValueDesc,',') Segment6ValueDesc, STRING_AGG(CHILD.Segment7ValueDesc,',') Segment7ValueDesc	
	                FROM M
	                LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = M.BuyerID AND CTO.ContactID > 0
	                INNER JOIN YDBookingMaster BM ON BM.YDBookingMasterID = M.YDBookingMasterID
	                --LEFT JOIN FreeConceptMaster FM ON FM.GroupConceptNo = BM.GroupConceptNo
	                INNER JOIN DyeingMachine DM ON DM.DMID = M.DMID
	                INNER JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = M.OperatorID
	                INNER JOIN {DbNames.EPYSL}..ShiftInfo S ON S.ShiftId = M.ShiftID
	                INNER JOIN CHILD ON CHILD.YDProductionMasterID = M.YDProductionMasterID
	                GROUP BY M.YDProductionMasterID, M.YDRecipeNo,M.YDRecipeID,M.YDDBatchNo,M.YDDBatchID,M.YDProductionNo, M.YDProductionDate, M.YDBookingMasterID, 
	                M.BuyerID, M.Remarks, M.IsApprove, M.IsAcknowledge, BM.GroupConceptNo,
	                BM.YDBookingNo, BM.YDBookingDate, CTO.[Name], DM.DyeingMcslNo, M.BatchNo, E.DisplayEmployeeCode, EmployeeName, 
	                S.ShortName
                )
                Select *, Count(*) Over() TotalRows From A ";
            }
            else if (status == Status.Acknowledge)
            {
                sql = $@"WITH M AS 
                (
	                SELECT	PM.YDProductionMasterID, 
					YRDM.YDRecipeNo,YRDM.YDRecipeID,YDBM.YDDBatchNo,PM.YDDBatchID,
					PM.YDProductionNo, PM.YDProductionDate, PM.YDBookingMasterID, 
	                PM.BuyerID, PM.Remarks, PM.IsApprove, PM.IsAcknowledge,
	                PM.ShiftID, PM.DMID, PM.OperatorID, PM.BatchNo
	                FROM YDProductionMaster PM 
					--INNER JOIN YDBookingChild YBC ON YBC.YDBookingMasterID = PM.YDBookingMasterID
					INNER JOIN YDRecipeDefinitionMaster YRDM ON YRDM.YDRecipeID= PM.YDRecipeID
					INNER JOIN YDDyeingBatchMaster YDBM ON YDBM.YDDBatchID = PM.YDDBatchID
					INNER JOIN YDProductionMaster YDM ON YDM.YDDBatchID = YDBM.YDDBatchID
					WHERE ISNULL(PM.IsApprove,0) = 1 AND ISNULL(PM.IsAcknowledge,0) = 1
                ),  
                CHILD AS 
                (
	                SELECT C.YDProductionChildID, C.YDProductionMasterID, C.ItemMasterID, C.ColorId, Color.SegmentValue ColorName,
	                ISV1.SegmentValueID Segment1ValueId, ISV2.SegmentValueID Segment2ValueId, ISV3.SegmentValueID Segment3ValueId, 
                    ISV4.SegmentValueID Segment4ValueId,
	                ISV5.SegmentValueID Segment5ValueId, ISV6.SegmentValueID Segment6ValueId, ISV7.SegmentValueID Segment7ValueId,
	                ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc,
	                ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc
	                FROM YDProductionChild C
	                INNER JOIN M ON M.YDProductionMasterID = C.YDProductionMasterID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = C.ColorId
	                LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = C.ItemMasterID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
	                GROUP BY C.YDProductionChildID, C.YDProductionMasterID, C.ItemMasterID, C.ColorId, Color.SegmentValue,
	                ISV1.SegmentValueID, ISV2.SegmentValueID, ISV3.SegmentValueID, ISV4.SegmentValueID,	ISV5.SegmentValueID,
	                ISV6.SegmentValueID, ISV7.SegmentValueID, ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue,
	                ISV4.SegmentValue,	ISV5.SegmentValue, ISV6.SegmentValue, ISV7.SegmentValue
                ), 
                A AS 
                (
	                SELECT M.YDProductionMasterID, 
					M.YDRecipeNo,M.YDRecipeID,M.YDDBatchNo,M.YDDBatchID,
					M.YDProductionNo, M.YDProductionDate, M.YDBookingMasterID, M.BuyerID, 
	                M.Remarks, M.IsApprove, M.IsAcknowledge, BM.GroupConceptNo,
	                BM.YDBookingNo, BM.YDBookingDate, CTO.Name Buyer, DM.DyeingMcslNo MCSLNo, M.BatchNo, E.DisplayEmployeeCode +' '+EmployeeName Operator, S.ShortName [Shift],
	                STRING_AGG(CHILD.ColorName,',') ColorName, STRING_AGG(CHILD.Segment1ValueDesc,',') Segment1ValueDesc, STRING_AGG(CHILD.Segment2ValueDesc,',') Segment2ValueDesc,
	                STRING_AGG(CHILD.Segment3ValueDesc,',') Segment3ValueDesc, STRING_AGG(CHILD.Segment4ValueDesc,',') Segment4ValueDesc, STRING_AGG(CHILD.Segment5ValueDesc,',') Segment5ValueDesc,
	                STRING_AGG(CHILD.Segment6ValueDesc,',') Segment6ValueDesc, STRING_AGG(CHILD.Segment7ValueDesc,',') Segment7ValueDesc	
	                FROM M
	                LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = M.BuyerID AND CTO.ContactID > 0
	                INNER JOIN YDBookingMaster BM ON BM.YDBookingMasterID = M.YDBookingMasterID
	                --LEFT JOIN FreeConceptMaster FM ON FM.GroupConceptNo = BM.GroupConceptNo
	                INNER JOIN DyeingMachine DM ON DM.DMID = M.DMID
	                INNER JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = M.OperatorID
	                INNER JOIN {DbNames.EPYSL}..ShiftInfo S ON S.ShiftId = M.ShiftID
	                INNER JOIN CHILD ON CHILD.YDProductionMasterID = M.YDProductionMasterID
	                GROUP BY M.YDProductionMasterID, M.YDRecipeNo,M.YDRecipeID,M.YDDBatchNo,M.YDDBatchID,M.YDProductionNo, M.YDProductionDate, M.YDBookingMasterID, 
	                M.BuyerID, M.Remarks, M.IsApprove, M.IsAcknowledge, BM.GroupConceptNo,
	                BM.YDBookingNo, BM.YDBookingDate, CTO.[Name], DM.DyeingMcslNo, M.BatchNo, E.DisplayEmployeeCode, EmployeeName, 
	                S.ShortName
                )
                Select *, Count(*) Over() TotalRows From A ";
            }
            else
            {
                sql = $@"WITH M AS 
                (
	                SELECT	PM.YDProductionMasterID, 
					YRDM.YDRecipeNo,YRDM.YDRecipeID,YDBM.YDDBatchNo,PM.YDDBatchID,
					PM.YDProductionNo, PM.YDProductionDate, PM.YDBookingMasterID, 
	                PM.BuyerID, PM.Remarks, PM.IsApprove, PM.IsAcknowledge,
	                PM.ShiftID, PM.DMID, PM.OperatorID, PM.BatchNo
	                FROM YDProductionMaster PM 
					--INNER JOIN YDBookingChild YBC ON YBC.YDBookingMasterID = PM.YDBookingMasterID
					INNER JOIN YDRecipeDefinitionMaster YRDM ON YRDM.YDRecipeID= PM.YDRecipeID
					INNER JOIN YDDyeingBatchMaster YDBM ON YDBM.YDDBatchID = PM.YDDBatchID
					INNER JOIN YDProductionMaster YDM ON YDM.YDDBatchID = YDBM.YDDBatchID
					WHERE ISNULL(PM.IsApprove,0) = 1 AND ISNULL(PM.IsAcknowledge,0) = 1
                ),  
                CHILD AS 
                (
	                SELECT C.YDProductionChildID, C.YDProductionMasterID, C.ItemMasterID, C.ColorId, Color.SegmentValue ColorName,
	                ISV1.SegmentValueID Segment1ValueId, ISV2.SegmentValueID Segment2ValueId, ISV3.SegmentValueID Segment3ValueId, 
                    ISV4.SegmentValueID Segment4ValueId,
	                ISV5.SegmentValueID Segment5ValueId, ISV6.SegmentValueID Segment6ValueId, ISV7.SegmentValueID Segment7ValueId,
	                ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc,
	                ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc
	                FROM YDProductionChild C
	                INNER JOIN M ON M.YDProductionMasterID = C.YDProductionMasterID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = C.ColorId
	                LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = C.ItemMasterID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
	                GROUP BY C.YDProductionChildID, C.YDProductionMasterID, C.ItemMasterID, C.ColorId, Color.SegmentValue,
	                ISV1.SegmentValueID, ISV2.SegmentValueID, ISV3.SegmentValueID, ISV4.SegmentValueID,	ISV5.SegmentValueID,
	                ISV6.SegmentValueID, ISV7.SegmentValueID, ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue,
	                ISV4.SegmentValue,	ISV5.SegmentValue, ISV6.SegmentValue, ISV7.SegmentValue
                ), 
                A AS 
                (
	                SELECT M.YDProductionMasterID, 
					M.YDRecipeNo,M.YDRecipeID,M.YDDBatchNo,M.YDDBatchID,
					M.YDProductionNo, M.YDProductionDate, M.YDBookingMasterID, M.BuyerID, 
	                M.Remarks, M.IsApprove, M.IsAcknowledge, BM.GroupConceptNo,
	                BM.YDBookingNo, BM.YDBookingDate, CTO.Name Buyer, DM.DyeingMcslNo MCSLNo, M.BatchNo, E.DisplayEmployeeCode +' '+EmployeeName Operator, S.ShortName [Shift],
	                STRING_AGG(CHILD.ColorName,',') ColorName, STRING_AGG(CHILD.Segment1ValueDesc,',') Segment1ValueDesc, STRING_AGG(CHILD.Segment2ValueDesc,',') Segment2ValueDesc,
	                STRING_AGG(CHILD.Segment3ValueDesc,',') Segment3ValueDesc, STRING_AGG(CHILD.Segment4ValueDesc,',') Segment4ValueDesc, STRING_AGG(CHILD.Segment5ValueDesc,',') Segment5ValueDesc,
	                STRING_AGG(CHILD.Segment6ValueDesc,',') Segment6ValueDesc, STRING_AGG(CHILD.Segment7ValueDesc,',') Segment7ValueDesc	
	                FROM M
	                LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = M.BuyerID AND CTO.ContactID > 0
	                INNER JOIN YDBookingMaster BM ON BM.YDBookingMasterID = M.YDBookingMasterID
	                --LEFT JOIN FreeConceptMaster FM ON FM.GroupConceptNo = BM.GroupConceptNo
	                INNER JOIN DyeingMachine DM ON DM.DMID = M.DMID
	                INNER JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = M.OperatorID
	                INNER JOIN {DbNames.EPYSL}..ShiftInfo S ON S.ShiftId = M.ShiftID
	                INNER JOIN CHILD ON CHILD.YDProductionMasterID = M.YDProductionMasterID
	                GROUP BY M.YDProductionMasterID, M.YDRecipeNo,M.YDRecipeID,M.YDDBatchNo,M.YDDBatchID,M.YDProductionNo, M.YDProductionDate, M.YDBookingMasterID, 
	                M.BuyerID, M.Remarks, M.IsApprove, M.IsAcknowledge, BM.GroupConceptNo,
	                BM.YDBookingNo, BM.YDBookingDate, CTO.[Name], DM.DyeingMcslNo, M.BatchNo, E.DisplayEmployeeCode, EmployeeName, 
	                S.ShortName
                )
                Select *, Count(*) Over() TotalRows From A ";
            }

            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<YDProductionMaster>(sql);
        }

        //public async Task<YDProductionMaster> GetNewAsync(int newId, int itemMasterID, int colorID)
        public async Task<YDProductionMaster> GetNewAsync(int newId, int itemMasterID, int colorID, int ydDBatchID)
        {
            #region Previous SQL
            //var sql = $@"
            //    ;WITH M AS (
            //     SELECT	BM.YDBookingMasterID, BM.YDBookingNo, BM.YDBookingDate, BM.BuyerID, BM.GroupConceptNo
            //     FROM YDBookingMaster BM
            //        INNER Join YDBookingChild YDBCT On BM.YDBookingMasterID = YDBCT.YDBookingMasterID
            //        --INNER Join YDBookingChildTwisting YDBCT On BM.YDBookingMasterID = YDBCT.YDBookingMasterID
            //        WHERE BM.YDBookingMasterID = {newId} AND YDBCT.ItemMasterID={itemMasterID}
            //    )
            //    SELECT M.YDBookingMasterID, M.YDBookingNo, M.YDBookingDate, M.BuyerID, CTO.Name Buyer,M.GroupConceptNo--,FC.ColorName
            //    FROM M
            //    LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = M.BuyerID AND CTO.ContactID > 0
            //    --LEFT JOIN FreeConceptMaster FM ON FM.GroupConceptNo = M.GroupConceptNo
            //    --LEFT JOIN FreeConceptChildColor FC ON FC.ConceptID = FM.ConceptID
            //    GROUP BY M.YDBookingMasterID, M.YDBookingNo, M.YDBookingDate, M.BuyerID, CTO.Name,M.GroupConceptNo; --,FC.ColorName;

            //    -- Childs
            //    ;SELECT YDBC.YDBookingChildID,YDBC.ItemMasterID ItemMasterID,YDBC.BookingQty BookingQty,
            //    YDBC.BookingQty ProducedQty,YDBC.UnitID UnitID,YDBC.ProgramName, UN.DisplayUnitDesc,YDBM.BuyerID,
            //    YDBC.NoOfThread, IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, IM.Segment4ValueID, 
            //    IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID, IM.Segment8ValueID, ISV1.SegmentValue Segment1ValueDesc, 
            //    ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc,
            //    ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc,
            //    YDBC.ShadeCode ShadeCode,Color.SegmentValue AS ColorName,YDBC.ColorId,YDBC.ColorCode,YDBC.DPID, YDBC.DPProcessInfo,
            //    DP.DPName, YDBC.NoOfCone BookingConeQty
            //    FROM YDBookingChild YDBC
            //    INNER JOIN YDBookingMaster YDBM ON YDBC.YDBookingMasterID=YDBM.YDBookingMasterID
            //    LEFT JOIN DyeingProcessPart_HK DP ON DP.DPID=YDBC.DPID
            //    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = YDBC.ColorId
            //    INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YDBC.ItemMasterID
            //    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
            //    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
            //    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
            //    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
            //    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
            //    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
            //    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
            //    LEFT JOIN {DbNames.EPYSL}..Unit UN ON YDBC.UnitID = UN.UnitID
            //    WHERE YDBC.YDBookingMasterID = {newId} AND YDBC.ColorID = {colorID};

            //    -- Operators
            //    ;SELECT E.EmployeeCode id, E.DisplayEmployeeCode +' '+EmployeeName text
            //    from {DbNames.EPYSL}..Employee E
            //    INNER JOIN {DbNames.EPYSL}..EmployeeDesignation D ON D.DesigID=E.DesigID
            //    INNER JOIN {DbNames.EPYSL}..EmployeeDepartment ED ON ED.DepertmentID= E.DepertmentID
            //    where (D.Designation like '%operator%' or D.Designation = 'Helper' and ED.DepertmentDescription ='Knitting');

            //    -- Shifts
            //    ;SELECT CAST(ShiftId AS VARCHAR) id, ShortName text, CAST(FromHour AS nvarchar(6)) [desc],
            //    CAST(ToHour AS nvarchar(6)) additionalValue
            //    FROM {DbNames.EPYSL}..ShiftInfo
            //    Where CompanyID=6
            //    order by SeqNo;";
            #endregion
            var sql = $@"
                ;WITH M AS (
	                SELECT	YRDM.YDRecipeID,YDBM.YDDBatchID,BM.YDBookingMasterID, YRDM.YDRecipeNo,YDBM.YDDBatchNo,BM.YDBookingNo, BM.YDBookingDate, BM.BuyerID, BM.GroupConceptNo
	                FROM YDBookingMaster BM
                    INNER Join YDBookingChild YDBCT On BM.YDBookingMasterID = YDBCT.YDBookingMasterID
					INNER JOIN YDRecipeRequestMaster YRRM ON YRRM.YDBookingChildID = YDBCT.YDBookingChildID
					INNER JOIN YDRecipeDefinitionMaster YRDM ON YRDM.YDRecipeReqMasterID = YRRM.YDRecipeReqMasterID
					INNER JOIN YDBatchItemRequirement YDBIR ON YDBIR.YDBookingChildID=YDBCT.YDBookingChildID
					INNER JOIN YDDyeingBatchItem YDDBI ON YDDBI.YDBItemReqID=YDBIR.YDBItemReqID
					INNER JOIN YDDyeingBatchMaster YDBM ON YDBM.YDDBatchID = YDDBI.YDDBatchID
					--YDBookingChildTwisting YDBCT On BM.YDBookingMasterID = YDBCT.YDBookingMasterID
                    WHERE BM.YDBookingMasterID = {newId} AND YDBCT.ItemMasterID={itemMasterID} AND YDBM.YDDBatchID={ydDBatchID}
                )
                SELECT M.YDRecipeID,M.YDDBatchID,M.YDBookingMasterID,M.YDRecipeNo,M.YDDBatchNo, M.YDBookingNo, M.YDBookingDate, M.BuyerID, CTO.Name Buyer,M.GroupConceptNo--,FC.ColorName
                FROM M
                LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = M.BuyerID AND CTO.ContactID > 0
                --LEFT JOIN FreeConceptMaster FM ON FM.GroupConceptNo = M.GroupConceptNo
                --LEFT JOIN FreeConceptChildColor FC ON FC.ConceptID = FM.ConceptID
                GROUP BY M.YDRecipeID,M.YDDBatchID,M.YDBookingMasterID, M.YDRecipeNo,M.YDDBatchNo,M.YDBookingNo, M.YDBookingDate, M.BuyerID, CTO.Name,M.GroupConceptNo; --,FC.ColorName;
                   
                -- Childs
                ;SELECT YDBC.YDBookingChildID,YDDBI.YDDBatchID,YDDBI.YDDBIID,YDBC.ItemMasterID ItemMasterID,YDBC.BookingQty BookingQty,
                YDBC.BookingQty ProducedQty,YDBC.UnitID UnitID,YDBC.ProgramName, UN.DisplayUnitDesc,YDBM.BuyerID,
                YDBC.NoOfThread, IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, IM.Segment4ValueID, 
                IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID, IM.Segment8ValueID, ISV1.SegmentValue Segment1ValueDesc, 
                ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc,
                ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc,
                YDBC.ShadeCode ShadeCode,Color.SegmentValue AS ColorName,YDBC.ColorId,YDBC.ColorCode,YDBC.DPID, YDBC.DPProcessInfo,
                DP.DPName, YDBC.NoOfCone BookingConeQty,YDDBI.YDRICRBId
                FROM
				YDBookingChild YDBC
				--YDBookingChildTwisting YDBC
				INNER JOIN YDBatchItemRequirement YDBIR ON YDBIR.YDBookingChildID=YDBC.YDBookingChildID
				INNER JOIN YDDyeingBatchItem YDDBI ON YDDBI.YDBItemReqID=YDBIR.YDBItemReqID
                INNER JOIN YDBookingMaster YDBM ON YDBC.YDBookingMasterID=YDBM.YDBookingMasterID
                LEFT JOIN DyeingProcessPart_HK DP ON DP.DPID=YDBC.DPID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = YDBC.ColorId
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YDBC.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                LEFT JOIN {DbNames.EPYSL}..Unit UN ON YDBC.UnitID = UN.UnitID
                WHERE YDBC.YDBookingMasterID = {newId} AND YDBC.ColorID = {colorID} AND YDDBI.YDDBatchID={ydDBatchID};

                -- Operators
                ;SELECT E.EmployeeCode id, E.DisplayEmployeeCode +' '+EmployeeName text
                from {DbNames.EPYSL}..Employee E
                INNER JOIN {DbNames.EPYSL}..EmployeeDesignation D ON D.DesigID=E.DesigID
                INNER JOIN {DbNames.EPYSL}..EmployeeDepartment ED ON ED.DepertmentID= E.DepertmentID
                where (D.Designation like '%operator%' or D.Designation = 'Helper' and ED.DepertmentDescription ='Knitting');

                -- Shifts
                ;SELECT CAST(ShiftId AS VARCHAR) id, ShortName text, CAST(FromHour AS nvarchar(6)) [desc],
                CAST(ToHour AS nvarchar(6)) additionalValue
                FROM {DbNames.EPYSL}..ShiftInfo
                Where CompanyID=6
                order by SeqNo;";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YDProductionMaster data = await records.ReadFirstOrDefaultAsync<YDProductionMaster>();
                Guard.Against.NullObject(data);
                data.Childs = records.Read<YDProductionChild>().ToList();
                data.OperatorList = records.Read<Select2OptionModel>().ToList();
                data.ShiftList = records.Read<Select2OptionModel>().ToList();

                var currentTime = Convert.ToDecimal($"{DateTime.Now.Hour}.{DateTime.Now.Minute}");
                int.TryParse(data.ShiftList.FirstOrDefault(x => currentTime >= Convert.ToDecimal(x.desc) && currentTime <= Convert.ToDecimal(x.additionalValue))?.id, out int shiftId);
                data.ShiftID = shiftId;

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

        public async Task<YDProductionMaster> GetAsync(int id)
        {
            var sql = $@"
            ;WITH M AS 
            (
                SELECT	PM.YDProductionMasterID, 
				YRDM.YDRecipeNo,YRDM.YDRecipeID,YDBM.YDDBatchNo,PM.YDDBatchID,
				PM.YDProductionNo, PM.YDProductionDate, PM.YDBookingMasterID, PM.BuyerID, 
	            PM.Remarks, PM.IsApprove, PM.IsAcknowledge, PM.ShiftID, PM.DMID, PM.OperatorID, PM.BatchNo
                FROM YDProductionMaster PM
				--INNER JOIN YDBookingChild YBC ON YBC.YDBookingMasterID = PM.YDBookingMasterID
				INNER JOIN YDRecipeDefinitionMaster YRDM ON YRDM.YDRecipeID= PM.YDRecipeID
				INNER JOIN YDDyeingBatchMaster YDBM ON YDBM.YDDBatchID = PM.YDDBatchID
				INNER JOIN YDProductionMaster YDM ON YDM.YDDBatchID = YDBM.YDDBatchID
	            WHERE PM.YDProductionMasterID = {id}
            )
            SELECT M.YDProductionMasterID, M.YDRecipeNo,M.YDRecipeID,M.YDDBatchNo,M.YDDBatchID,M.YDProductionNo, M.YDProductionDate, M.YDBookingMasterID, M.BuyerID, 
            M.Remarks, M.IsApprove, M.IsAcknowledge, M.ShiftID, M.DMID, M.OperatorID, M.BatchNo, DM.DyeingMcslNo MCSLNo, 
            BM.YDBookingNo, BM.YDBookingDate, CTO.Name Buyer,FM.GroupConceptNo,Count(*) Over() TotalRows
            FROM M
            INNER JOIN DyeingMachine DM ON DM.DMID = M.DMID
            INNER JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = M.BuyerID
            INNER JOIN YDBookingMaster BM ON BM.YDBookingMasterID = M.YDBookingMasterID
            LEFT JOIN FreeConceptMaster FM ON FM.GroupConceptNo = BM.GroupConceptNo
            Group By M.YDProductionMasterID,M.YDRecipeNo,M.YDRecipeID,M.YDDBatchNo,M.YDDBatchID, M.YDProductionNo, M.YDProductionDate, M.YDBookingMasterID, M.BuyerID, 
            M.Remarks, M.IsApprove, M.IsAcknowledge, M.ShiftID, M.DMID, M.OperatorID, M.BatchNo, DM.DyeingMcslNo, 
            BM.YDBookingNo, BM.YDBookingDate, CTO.Name,FM.GroupConceptNo; 

            -- Childs
            SELECT YDBC.YDProductionMasterID, YDBC.YDDBIID,YDBC.YDProductionChildID, YDBC.YDBookingChildID, YDBC.ItemMasterID ItemMasterID, YDBC.BookingQty BookingQty,
            YDBC.UnitID UnitID, YDBC.ProgramName, UN.DisplayUnitDesc, YDBM.BuyerID, YDBC.NoOfThread,IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID,
            IM.Segment4ValueID, IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID, IM.Segment8ValueID, ISV1.SegmentValue Segment1ValueDesc,
            ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc,
            ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc, YDBC.ShadeCode ShadeCode,Color.SegmentValue ColorName,YDBC.ColorId,
            YDBC.ColorCode,YDBC.DPID,YDBC.DPProcessInfo,DP.DPName,YDBC.ProducedQty,YDBC.ProducedCone, YDBCT.NoOfCone BookingConeQty,YDBC.YDRICRBId
            FROM YDProductionChild YDBC
            INNER JOIN YDProductionMaster YDBM ON YDBC.YDProductionMasterID = YDBM.YDProductionMasterID
            INNER JOIN YDBookingChild YDBCT ON YDBCT.YDBookingChildID = YDBC.YDBookingChildID
            LEFT JOIN DyeingProcessPart_HK DP ON DP.DPID = YDBC.DPID
            Left JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = YDBC.ColorId
            INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON YDBC.ItemMasterID = IM.ItemMasterID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
            LEFT JOIN {DbNames.EPYSL}..Unit UN ON YDBC.UnitID = UN.UnitID
            WHERE YDBC.YDProductionMasterID = {id};

            --Operators
            ;SELECT E.EmployeeCode id, E.DisplayEmployeeCode +' '+EmployeeName text
            from {DbNames.EPYSL}..Employee E
            INNER JOIN {DbNames.EPYSL}..EmployeeDesignation D ON D.DesigID=E.DesigID
            INNER JOIN {DbNames.EPYSL}..EmployeeDepartment ED ON ED.DepertmentID= E.DepertmentID
            where (D.Designation like '%operator%' or D.Designation = 'Helper' and ED.DepertmentDescription ='Knitting');

            --Shifts
            ;SELECT CAST(ShiftId AS VARCHAR) id, ShortName text, CAST(FromHour AS nvarchar(6)) [desc],CAST(ToHour AS nvarchar(6)) additionalValue
            FROM {DbNames.EPYSL}..ShiftInfo
            Where CompanyID=6
            order by SeqNo;";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YDProductionMaster data = await records.ReadFirstOrDefaultAsync<YDProductionMaster>();
                Guard.Against.NullObject(data);
                data.Childs = records.Read<YDProductionChild>().ToList();
                data.OperatorList = records.Read<Select2OptionModel>().ToList();
                data.ShiftList = records.Read<Select2OptionModel>().ToList();
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

        public async Task<YDProductionMaster> GetAllAsync(int id)
        {
            var sql = $@"
            ;Select * From YDProductionMaster Where YDProductionMasterID = {id}

            ;Select * From YDProductionChild Where YDProductionMasterID = {id}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YDProductionMaster data = await records.ReadFirstOrDefaultAsync<YDProductionMaster>();
                Guard.Against.NullObject(data);
                data.Childs = records.Read<YDProductionChild>().ToList();
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

        public async Task SaveAsync(YDProductionMaster entity)
        {
            SqlTransaction transaction = null;
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();
                await _connectionGmt.OpenAsync();
                transactionGmt = _connectionGmt.BeginTransaction();

                int maxChildId = 0;
                switch (entity.EntityState)
                {
                    case EntityState.Added:
                        entity.YDProductionMasterID = await _service.GetMaxIdAsync(TableNames.YD_PRODUCTION_MASTER, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        entity.YDProductionNo = await _service.GetMaxNoAsync(TableNames.YD_PRODUCTION_NO,1, RepeatAfterEnum.NoRepeat,"000000", transactionGmt, _connectionGmt);

                        maxChildId = await _service.GetMaxIdAsync(TableNames.YD_PRODUCTION_CHILD, entity.Childs.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        foreach (YDProductionChild item in entity.Childs)
                        {
                            item.YDProductionChildID = maxChildId++;
                            item.YDProductionMasterID = entity.YDProductionMasterID;
                            item.EntityState = EntityState.Added;
                        }
                        break;

                    case EntityState.Modified:
                        var addedChilds = entity.Childs.FindAll(x => x.EntityState == EntityState.Added);
                        maxChildId = await _service.GetMaxIdAsync(TableNames.YD_PRODUCTION_CHILD, addedChilds.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

                        foreach (YDProductionChild item in addedChilds)
                        {
                            item.YDProductionChildID = maxChildId++;
                            item.YDProductionMasterID = entity.YDProductionMasterID;
                            item.EntityState = EntityState.Added;
                        }
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

        public async Task UpdateEntityAsync(YDProductionMaster entity)
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
}
