using Dapper;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Application.Interfaces.RND;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.Data.SqlClient;
using System.Data.Entity;
using EPYSLTEXCore.Infrastructure.Exceptions;

namespace EPYSLTEXCore.Application.Services.RND
{
    public class YDDyeingBatchService : IYDDyeingBatchService
    {
        private readonly IDapperCRUDService<YDDyeingBatchMaster> _service;
        private readonly SqlConnection _connection;
        private readonly SqlConnection _connectionGmt;
        public YDDyeingBatchService(IDapperCRUDService<YDDyeingBatchMaster> service)
        {
            _service = service;
            _service.Connection = service.GetConnection(AppConstants.GMT_CONNECTION);
            _connectionGmt = service.Connection;

            _service.Connection = service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _connection = service.Connection;

        }

        public async Task<List<YDDyeingBatchMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By YDDBatchID Desc" : paginationInfo.OrderBy;
            var sql = string.Empty;
            if (status == Status.Pending)
            {
                sql += $@";WITH YDDyeingBatch AS (
	                     SELECT DB.YDBatchID, DBM.YDDBatchID, DBM.YDDBatchNo, SUM(DB.BatchUseQtyKG) TotalBatchUseQtyKG, SUM(DB.BatchUseQtyPcs) TotalBatchUseQtyPcs
	                    ,BatchStatus = ISNULL(DBR.BatchStatus,0), IsNewBatch = ISNULL(DBR.IsNewBatch,0), IsNewRecipe = ISNULL(DBR.IsNewRecipe,0)
	                    FROM {TableNames.YD_DYEING_BATCH_WITH_BATCH_MASTER} DB
	                    LEFT JOIN {TableNames.YD_DYEING_BATCH_MASTER} DBM ON DBM.YDDBatchID = DB.YDDBatchID
	                    LEFT JOIN {TableNames.YD_DYEING_BATCH_REWORK} DBR ON DBR.YDDBatchID = DB.YDDBatchID
	                    --WHERE BatchID IS NULL
	                    GROUP BY DB.YDBatchID, DBM.YDDBatchID, DBM.YDDBatchNo, ISNULL(DBR.BatchStatus,0), ISNULL(DBR.IsNewBatch,0), ISNULL(DBR.IsNewRecipe,0)
                    ),
                    M AS (
                        SELECT BM.*, YDDyeingBatch.YDDBatchID, YDDyeingBatch.TotalBatchUseQtyKG, --FBA.SLNo, 
	                    BuyerName = CASE WHEN BM.BuyerID = 0 THEN 'R&D' ELSE CTO.ShortName END, 
	                    BuyerTeamName = CASE WHEN BM.BuyerTeamID = 0 THEN 'R&D' ELSE CCT.TeamName END,
	                    [Status] = 'New', ExistingDBatchNo = ''
                        FROM {TableNames.YD_BATCH_MASTER} BM
                        LEFT JOIN {TableNames.YD_BATCH_CHILD} BC on BM.YDBatchID=BC.YDBatchID
						INNER JOIN {TableNames.SOFT_WINDING_MASTER} SWM ON SWM.YDBookingMasterID = BM.YDBookingMasterID
                        --LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.BookingID=BM.BookingID
	                    LEFT JOIN YDDyeingBatch ON YDDyeingBatch.YDBatchID = BM.YDBatchID
	                    LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = BM.BuyerID
	                    LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = BM.BuyerTeamID
	                    WHERE (YDDyeingBatch.YDBatchID IS NULL OR YDDyeingBatch.TotalBatchUseQtyKG < BM.BatchWeightKG)
						--AND BC.GRollID not in (select GRollID FROM {TableNames.YD_DYEING_BATCH_ITEM_ROLL})
                    ),
                    ReworkYDDyeingBatch AS (
	                     SELECT DB.YDBatchID, DBM.YDDBatchID, DBM.YDDBatchNo, SUM(DB.BatchUseQtyKG) TotalBatchUseQtyKG, SUM(DB.BatchUseQtyPcs) TotalBatchUseQtyPcs
	                    ,BatchStatus = ISNULL(DBR.BatchStatus,0), IsNewBatch = ISNULL(DBR.IsNewBatch,0), IsNewRecipe = ISNULL(DBR.IsNewRecipe,0)
	                    ,[Status] = 'Rework', ExistingDBatchNo = DBM.YDDBatchNo
	                    FROM {TableNames.YD_DYEING_BATCH_WITH_BATCH_MASTER} DB 
	                    INNER JOIN {TableNames.YD_DYEING_BATCH_MASTER} DBM ON DBM.YDDBatchID = DB.YDDBatchID
	                    INNER JOIN {TableNames.YD_DYEING_BATCH_REWORK} DBR ON DBR.YDDBatchID = DBM.YDDBatchID
	                    LEFT JOIN {TableNames.YD_DYEING_BATCH_MERGE_BATCH} DBB ON DBB.MergeDBatchID = DBM.YDDBatchID
	                    WHERE DBM.BatchStatus = 3 AND DBM.IsNewBatch = 1
	                    AND DBB.YDDBMID IS NULL
	                    GROUP BY DB.YDBatchID, DBM.YDDBatchID, DBM.YDDBatchNo, ISNULL(DBR.BatchStatus,0), ISNULL(DBR.IsNewBatch,0), ISNULL(DBR.IsNewRecipe,0)
                    ),
                    ReworkM AS (
                        SELECT BM.*, RDB.YDDBatchID, RDB.TotalBatchUseQtyKG, --FBA.SLNo, 
	                    BuyerName = CASE WHEN BM.BuyerID = 0 THEN 'R&D' ELSE CTO.ShortName END, 
	                    BuyerTeamName = CASE WHEN BM.BuyerTeamID = 0 THEN 'R&D' ELSE CCT.TeamName END,
	                    RDB.[Status], ExistingDBatchNo
                        FROM {TableNames.YD_BATCH_MASTER} BM
                        --LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.BookingID=BM.BookingID
	                    INNER JOIN ReworkYDDyeingBatch RDB ON RDB.YDBatchID = BM.YDBatchID
	                    LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = BM.BuyerID
	                    LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = BM.BuyerTeamID
                    ),
                    FFF AS (
                        SELECT M.YDBatchID, M.YDBatchNo, M.YDBatchDate, YDDBatchID = ISNULL(M.YDDBatchID,0), M.YDRecipeID, FCM.ConceptID, M.ColorID, M.CCColorID, M.BatchWeightKG, M.BatchWeightPcs BatchQtyPcs, M.Remarks,
                        M.ExportOrderID, M.BuyerID, M.BuyerTeamID, RDM.RecipeNo, RDM.RecipeDate, RDM.RecipeFor, COL.SegmentValue ColorName, M.BuyerName, M.BuyerTeamName,
                        FR.ValueName RecipeForName, M.TotalBatchUseQtyKG, FCM.GroupConceptNo ConceptNo, M.MachineLoading, M.DyeingNozzleQty, M.DyeingMcCapacity, [Status],ExistingDBatchNo
                        FROM M
                        LEFT JOIN {TableNames.RND_RECIPE_DEFINITION_MASTER} RDM ON RDM.RecipeID = M.YDRecipeID AND ISNULL(RDM.IsActive,0) = 1
                        INNER JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID = M.ColorID
                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue FR ON FR.ValueID = RDM.RecipeFor
	                    LEFT JOIN {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} FCC ON FCC.CCColorID = M.CCColorID
	                    LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = FCC.ConceptID

	                    UNION 

	                    SELECT M1.YDBatchID, M1.YDBatchNo, M1.YDBatchDate, YDDBatchID = ISNULL(M1.YDDBatchID,0), M1.YDRecipeID, FCM.ConceptID,  M1.ColorID, M1.CCColorID, M1.BatchWeightKG, M1.BatchWeightPcs BatchQtyPcs, M1.Remarks,
                        M1.ExportOrderID, M1.BuyerID, M1.BuyerTeamID, RDM.YDRecipeNo, RDM.RecipeDate, RDM.RecipeFor, COL.SegmentValue ColorName, M1.BuyerName, M1.BuyerTeamName,
                        FR.ValueName RecipeForName, M1.TotalBatchUseQtyKG, FCM.GroupConceptNo ConceptNo, M1.MachineLoading, M1.DyeingNozzleQty, M1.DyeingMcCapacity, [Status],ExistingDBatchNo
                        FROM ReworkM M1
                        LEFT JOIN {TableNames.YD_RECIPE_DEFINITION_MASTER} RDM ON RDM.YDRecipeID = M1.YDRecipeID AND ISNULL(RDM.IsActive,0) = 1
                        INNER JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID = M1.ColorID
                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue FR ON FR.ValueID = RDM.RecipeFor
	                    LEFT JOIN {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} FCC ON FCC.CCColorID = M1.CCColorID
	                    LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = FCC.ConceptID
                    ) 
                    Select *, Count(*) Over() TotalRows From FFF ";
                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? " Order By YDBatchID Desc " : paginationInfo.OrderBy;
            }
            else
            {
                sql += $@";WITH M AS (
                            SELECT DBM.*
                            FROM {TableNames.YD_DYEING_BATCH_MASTER} DBM
			                LEFT JOIN {TableNames.YD_DYEING_BATCH_MERGE_BATCH} DBB ON DBB.MergeDBatchID = DBM.YDDBatchID
			                WHERE DBB.YDDBMID IS NULL
                        ), FFF AS (
                            SELECT M.YDDBatchID, M.YDDBatchNo, M.YDDBatchDate, M.YDRecipeID, M.ColorID, M.CCColorID, M.BatchWeightKG, M.DMID, M.MachineLoading, M.DyeingNozzleQty,
                            M.ShiftID, M.OperatorID, M.BatchStatus, M.BatchStartTime, M.BatchEndTime, M.ProductionDate, M.Remarks, RDM.RecipeNo, RDM.RecipeDate, RDM.RecipeFor,
						    COL.SegmentValue ColorName, FR.ValueName RecipeForName, FC.GroupConceptNo ConceptNo, FC.GroupConceptNo SLNo,
                            FC.BuyerID, FC.BuyerTeamID, 
                            BuyerName = CASE WHEN FC.BuyerID = 0 THEN 'R&D' ELSE CTO.ShortName END, 
                            BuyerTeamName = CASE WHEN FC.BuyerTeamID = 0 THEN 'R&D' ELSE CCT.TeamName END,

                            (SELECT STUFF((
                            SELECT DISTINCT ', '+ BM.YDBatchNo
                            FROM {TableNames.YD_DYEING_BATCH_WITH_BATCH_MASTER} DB 
                            LEFT JOIN {TableNames.YD_BATCH_MASTER} BM ON BM.YDBatchID = DB.YDBatchID
                            WHERE DB.YDDBatchID = M.YDDBatchID
                            FOR XML PATH('')),1,1,'')) YDBatchNo

                            FROM M
                            LEFT JOIN {TableNames.RND_RECIPE_DEFINITION_MASTER} RDM ON RDM.RecipeID = M.YDRecipeID AND ISNULL(RDM.IsActive,0) = 1
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID = M.ColorID
						    LEFT JOIN {DbNames.EPYSL}..EntityTypeValue FR ON FR.ValueID = RDM.RecipeFor
						    LEFT JOIN {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} FCC ON FCC.CCColorID = M.CCColorID
						    LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FC ON FC.ConceptID = FCC.ConceptID
	                        LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = FC.BuyerID
	                        LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = FC.BuyerTeamID
                        ) 
                        Select *, Count(*) Over() TotalRows From FFF ";
            }

            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<YDDyeingBatchMaster>(sql);
        }
        public async Task<YDDyeingBatchMaster> GetNewAsync(int newId)
        {
            var sql = $@"
                        ;WITH M AS (
                            SELECT	Distinct BM.*, FCM.BookingID, SLNo = Case When FCM.IsBDS = 0 Then FCM.GroupConceptNo Else FBA.SLNo End
                            FROM {TableNames.YD_BATCH_MASTER} BM 
							Inner JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM On FCM.ConceptID = BM.ConceptID
							LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.BookingID = FCM.BookingID
                            WHERE BM.YDBatchID = {newId}
                        )
                        SELECT M.YDBatchID, M.YDBatchNo, M.YDBatchDate, M.YDRecipeID, FCM.ConceptID, M.BookingID, M.ColorID, M.CCColorID, M.BatchWeightKG, M.BatchWeightPcs BatchQtyPcs, M.Remarks,
                        M.ExportOrderID, M.BuyerID, M.BuyerTeamID, RDM.YDRecipeNo, YDRecipeDate = RDM.RecipeDate, YDRecipeFor = RDM.RecipeFor, COL.SegmentValue ColorName,
						FR.ValueName YDRecipeForName, M.MachineLoading, M.DyeingNozzleQty, M.DyeingMcCapacity, M.DMID, DM.DyeingMcslNo DMNo, FCM.GroupConceptNo ConceptNo, M.SLNo
                        FROM M
                        LEFT JOIN {TableNames.YD_RECIPE_DEFINITION_MASTER} RDM ON RDM.YDRecipeID = M.YDRecipeID
                        INNER JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID = M.ColorID
						LEFT JOIN {DbNames.EPYSL}..EntityTypeValue FR ON FR.ValueID = RDM.RecipeFor
						INNER JOIN {TableNames.DYEING_MACHINE} DM ON DM.DMID = M.DMID
                        LEFT JOIN {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} FCC ON FCC.CCColorID = M.CCColorID
                        LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID=FCC.ConceptID;

                        -----for YDDyeingBatchItem
                        ;WITH M AS (
                            SELECT * FROM {TableNames.YD_BATCH_ITEM_REQUIREMENT} BM WHERE BM.YDBatchID = {newId}
                        ),
                        ItemSegment As
                        (
	                        SELECT CAST(ISV.SegmentValueID As varchar) [id], ISV.SegmentValue [text], CAST(ISN.SegmentNameID As varchar) [desc]
	                        FROM {DbNames.EPYSL}..ItemSegmentName ISN
	                        INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISN.SegmentNameID = ISV.SegmentNameID
	                        WHERE ISNULL(ISV.SegmentValue, '') <> '' And SegmentValueID NOT IN(17725,5127,1616,1969,47221,2{newId}7,1616,1621,44382,44384,45969,46717,47220, 2155)
                        )
                        SELECT M.YDBItemReqID, M.YDBatchID, M.YDRecipeItemInfoID, M.ItemMasterID,M.ConceptID, M.Pcs QtyPcs, M.Qty, M.IsFloorRequistion, RDM.YDRecipeID, BookingID = 0, CM.SubGroupID ItemSubGroupID,
                        B.YDBatchNo, B.BuyerID, B.BuyerTeamID, B.ExportOrderID, KnittingType.TypeName KnittingType,Composition.text FabricComposition, Construction.text FabricConstruction,
                        Technical.TechnicalName,Gsm.text FabricGsm, SG.SubGroupName ItemSubGroup
                        FROM M
                        LEFT JOIN {TableNames.YD_RECIPE_DEFINITION_ITEM_INFO} RDM ON RDM.YDRecipeItemInfoID = M.YDRecipeItemInfoID
                        INNER JOIN {TableNames.YD_BATCH_MASTER} B ON B.YDBatchID = M.YDBatchID
                        LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} CM ON CM.ConceptID=M.ConceptID
                        LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID=CM.KnittingTypeID
                        LEFT JOIN ItemSegment Composition ON Composition.id=CM.CompositionID
                        LEFT JOIN ItemSegment Construction ON Construction.id=CM.ConstructionID
                        LEFT JOIN ItemSegment Gsm ON Gsm.id=CM.GSMID
                        LEFT JOIN {DbNames.EPYSL}..ItemSubGroup SG ON SG.SubGroupID = CM.SubGroupID
                        LEFT JOIN {TableNames.FabricTechnicalName} Technical ON Technical.TechnicalNameId=CM.TechnicalNameId;

                        ----for YDDyeingBatchItemRoll
                        ;SELECT C.BItemReqID, C.YDBatchID, C.GRollID, C.ItemMasterID, C.RollQty, C.RollQtyPcs, KP.RollNo
                        FROM {TableNames.YD_BATCH_CHILD} C
                        INNER JOIN {TableNames.RND_KNITTING_PRODUCTION} KP ON KP.GRollID = C.GRollID
                        WHERE C.YDBatchID = {newId};

                        --Childs
                        WITH M AS (
                            SELECT BC.*, RD.YDRecipeDInfoID, RD.Temperature FROM {TableNames.YD_BATCH_WISE_RECIPE_CHILD} BC
							LEFT JOIN {TableNames.YD_RECIPE_DEFINITION_CHILD} RD ON RD.YDRecipeChildID = BC.YDRecipeChildID
							WHERE BC.YDBatchID = {newId}
                        )
                        SELECT DI.YDRecipeDInfoID, M.TempIn, M.YDBatchID, M.ProcessTime, EV.ValueName FiberPart, ISV.SegmentValue ColorName
					    FROM M
						LEFT JOIN {TableNames.YD_RECIPE_DEFINITION_DYEING_INFO} DI ON DI.YDRecipeDInfoID = M.YDRecipeDInfoID
						INNER JOIN {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID = DI.FiberPartID
						INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = DI.ColorID
						GROUP BY DI.YDRecipeDInfoID, M.TempIn, M.YDBatchID, M.ProcessTime, EV.ValueName, ISV.SegmentValue;

                        --Def Childs
                        SELECT B.YDBRecipeChildID, B.YDBatchID, B.YDRecipeChildID, B.ProcessID, B.ParticularsID, B.RawItemID, B.Qty, B.UnitID, B.TempIn,
						B.TempOut, B.ProcessTime, C.YDRecipeID,I.ItemName, P.ValueName ProcessName,PL.ValueName ParticularsName,R.ItemName RawItemName,
						UU.DisplayUnitDesc Unit, C.IsPercentage, C.YDRecipeDInfoID, C.Temperature, EV.ValueName FiberPart, ISV.SegmentValue ColorName
						FROM {TableNames.YD_BATCH_WISE_RECIPE_CHILD} B
						LEFT JOIN {TableNames.YD_RECIPE_DEFINITION_CHILD} C ON C.YDRecipeChildID = B.YDRecipeChildID
						LEFT JOIN {DbNames.EPYSL}..ItemMaster I ON I.ItemMasterID=C.RawItemID
						LEFT JOIN {DbNames.EPYSL}..EntityTypeValue P ON P.ValueID = C.ProcessID
						LEFT JOIN {DbNames.EPYSL}..EntityTypeValue PL ON PL.ValueID = C.ParticularsID
						LEFT JOIN {DbNames.EPYSL}..ItemMaster R ON R.ItemMasterID = C.RawItemID
						LEFT JOIN {DbNames.EPYSL}..Unit UU ON UU.UnitID = C.UnitID
						LEFT JOIN {TableNames.YD_RECIPE_DEFINITION_DYEING_INFO} DI ON DI.YDRecipeDInfoID = C.YDRecipeDInfoID
						INNER JOIN {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID = DI.FiberPartID
						INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = DI.ColorID
						WHERE B.YDBatchID = {newId};
                    ";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YDDyeingBatchMaster data = records.Read<YDDyeingBatchMaster>().FirstOrDefault();
                data.YDDyeingBatchItems = records.Read<YDDyeingBatchItem>().ToList();
                data.YDDyeingBatchItemRolls = records.Read<YDDyeingBatchItemRoll>().ToList();
                data.YDDyeingBatchRecipes = records.Read<YDDyeingBatchRecipe>().ToList();
                data.DefChilds = records.Read<YDRecipeDefinitionChild>().ToList();
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
        public async Task<List<YDDyeingBatchMaster>> GetNewMultiSelectAsync(string batchIDs)
        {
            var sql = $@"
                        ;WITH M AS (
                            SELECT	Distinct BM.*, SLNo = Case When FCM.IsBDS = 0 Then FCM.GroupConceptNo Else FBA.SLNo End, FCM.BookingID
                            FROM {TableNames.YD_BATCH_MASTER} BM 
							Inner JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM On FCM.ConceptID = BM.ConceptID
							LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.BookingID = FCM.BookingID
                            WHERE BM.YDBatchID IN ({batchIDs})
                        )
                        SELECT M.YDBatchID, M.YDBatchNo, M.YDBatchDate, M.YDRecipeID, FCM.ConceptID, M.BookingID, M.ColorID, M.CCColorID, M.BatchWeightKG, M.BatchWeightPcs, M.Remarks,
                        M.ExportOrderID, M.BuyerID, M.BuyerTeamID, RDM.YDRecipeNo, YDRecipeDate = RDM.RecipeDate, YDRecipeFor = RDM.RecipeFor, COL.SegmentValue ColorName,
						FR.ValueName YDRecipeForName, M.MachineLoading, M.DyeingNozzleQty, M.DyeingMcCapacity, M.DMID, DM.DyeingMcslNo DMNo, FCM.GroupConceptNo ConceptNo, M.SLNo
                        FROM M
                        LEFT JOIN {TableNames.YD_RECIPE_DEFINITION_MASTER} RDM ON RDM.YDRecipeID = M.YDRecipeID
                        INNER JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID = M.ColorID
						LEFT JOIN {DbNames.EPYSL}..EntityTypeValue FR ON FR.ValueID = RDM.RecipeFor
						INNER JOIN {TableNames.DYEING_MACHINE} DM ON DM.DMID = M.DMID
                        LEFT JOIN {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} FCC ON FCC.CCColorID = M.CCColorID
                        LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID=FCC.ConceptID;

                        -----for YDDyeingBatchItem
                        ;WITH M AS (
                            SELECT BM.*, YDBC.YarnCategory FROM {TableNames.YD_BATCH_ITEM_REQUIREMENT} BM 
							INNER JOIN {TableNames.YDBookingChild} YDBC ON YDBC.YDBookingChildID = BM.YDBookingChildID 
                            WHERE BM.YDBatchID IN ({batchIDs})
                        ),
                        ItemSegment As
                        (
	                        SELECT CAST(ISV.SegmentValueID As varchar) [id], ISV.SegmentValue [text], CAST(ISN.SegmentNameID As varchar) [desc]
	                        FROM {DbNames.EPYSL}..ItemSegmentName ISN
	                        INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISN.SegmentNameID = ISV.SegmentNameID
	                        WHERE ISNULL(ISV.SegmentValue, '') <> '' And SegmentValueID NOT IN(17725,5127,1616,1969,47221,2{batchIDs}7,1616,1621,44382,44384,45969,46717,47220, 2155)
                        )
                        SELECT M.YDBItemReqID, M.YDBatchID, M.YDRecipeItemInfoID, M.ItemMasterID,M.ConceptID, M.Pcs QtyPcs, M.Qty, M.IsFloorRequistion, RDM.YDRecipeID, CM.BookingID, CM.SubGroupID ItemSubGroupID,
                        B.YDBatchNo, B.BuyerID, B.BuyerTeamID, B.ExportOrderID, KnittingType.TypeName KnittingType,Composition.text FabricComposition, Construction.text FabricConstruction,
                        Technical.TechnicalName,Gsm.text FabricGsm, SG.SubGroupName ItemSubGroup, M.YarnCategory, SWC.SoftWindingChildID, SWC.YDRICRBId
                        FROM M
						INNER JOIN {TableNames.SOFT_WINDING_CHILD} SWC ON SWC.YDBItemReqID = M.YDBItemReqID
                        LEFT JOIN {TableNames.YD_RECIPE_DEFINITION_ITEM_INFO} RDM ON RDM.YDRecipeItemInfoID = M.YDRecipeItemInfoID
                        INNER JOIN {TableNames.YD_BATCH_MASTER} B ON B.YDBatchID = M.YDBatchID
                        LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} CM ON CM.ConceptID=M.ConceptID
                        LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID=CM.KnittingTypeID
                        LEFT JOIN ItemSegment Composition ON Composition.id=CM.CompositionID
                        LEFT JOIN ItemSegment Construction ON Construction.id=CM.ConstructionID
                        LEFT JOIN ItemSegment Gsm ON Gsm.id=CM.GSMID
                        LEFT JOIN {DbNames.EPYSL}..ItemSubGroup SG ON SG.SubGroupID = CM.SubGroupID
                        LEFT JOIN {TableNames.FabricTechnicalName} Technical ON Technical.TechnicalNameId=CM.TechnicalNameId;

                        ;WITH
            PendingRolls AS
            (
			SELECT FCM.ConceptID,RollID = KP.GRollID, FCM.GroupConceptNo, CR.SFDChildRollID,
			C.BItemReqID,C.YDBatchID,C.ItemMasterID
	            FROM {TableNames.RND_KNITTING_PRODUCTION} KP 
	            INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = KP.ConceptID
	            LEFT JOIN {TableNames.SFD_CHILD_ROLL} CR ON CR.RollID = KP.GRollID
				LEFT JOIN {TableNames.YD_BATCH_CHILD} C ON C.GRollID = KP.GRollID
	            WHERE --ISNULL(kP.InActive,0) = 0 AND 
	            CR.SFDChildRollID IS NULL
	            AND  C.YDBatchID IN ({batchIDs})
	            ),
						CC as(
						select Max(CCColorID)CCColorID,Max(FCC.ConceptID)ConceptID,Max(FCC.ColorName)ColorName from FreeConceptChildColor FCC
						Group By FCC.ConceptID
						),
            AllRolls AS
            (
	            SELECT SFDChildID = PR.BItemReqID, 
	            FCM.ConceptID, FCM.GroupConceptNo, FCM.SubGroupID, FCM.BookingChildID, 
	            FCM.BookingID, FCM.ConsumptionID, KP.RollNo, RollQtyKg = KP.RollQty, RollQtyPcs = KP.RollQtyPcs,
	            FCM.ItemMasterID, KP.GRollID, KP.ParentGRollID,
	             CC.CCColorID, CC.ColorName, 
	            FCM.TechnicalNameId,kP.InActive,PR.BItemReqID,PR.YDBatchID
	            FROM {TableNames.RND_KNITTING_PRODUCTION} KP
	            INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = KP.ConceptID
	            INNER JOIN PendingRolls PR ON PR.RollID = KP.GRollID
				LEFT JOIN CC ON CC.ConceptID = PR.ConceptID
	            --WHERE ISNULL(kP.InActive,0) = 1 
	            GROUP BY KP.GRollID, CC.CCColorID, 
	            FCM.ConceptID, FCM.GroupConceptNo, FCM.SubGroupID, FCM.BookingChildID, 
	            FCM.BookingID, FCM.ConsumptionID, KP.RollNo, KP.RollQty, KP.RollQtyPcs,
	            FCM.ItemMasterID, KP.GRollID, KP.ParentGRollID,
	            KP.GRollID,  CC.ColorName,
	            FCM.TechnicalNameId,kP.InActive,PR.BItemReqID,PR.YDBatchID
            ),
            ActiveChildRolls As
            (
                Select SFDChildRollID = KP.GRollID, ARN.SFDChildID, 
                KP.ConceptID, ARN.GroupConceptNo, ARN.SubGroupID, ARN.BookingChildID, 
                ARN.BookingID, ARN.ConsumptionID, KP.RollNo, KP.RollQty, KP.RollQtyPcs,
                ARN.ItemMasterID, KP.GRollID, KP.ParentGRollID,
                RollID=KP.GRollID, ARN.CCColorID, ARN.ColorName, --ARN.BChildID, 
                ARN.TechnicalNameId,kP.InActive,ARN.BItemReqID,ARN.YDBatchID
                FROM {TableNames.RND_KNITTING_PRODUCTION} KP
                Inner Join AllRolls ARN  On ARN.ConceptID=KP.ConceptID AND ARN.GRollID=KP.GRollID
                Where  KP.InActive=0
                GROUP BY KP.GRollID, ARN.SFDChildID, 
                KP.ConceptID, ARN.GroupConceptNo, ARN.SubGroupID, ARN.BookingChildID, 
                ARN.BookingID, ARN.ConsumptionID, KP.RollNo, KP.RollQty, KP.RollQtyPcs,
                ARN.ItemMasterID, KP.GRollID, KP.ParentGRollID,
                KP.GRollID, ARN.CCColorID, ARN.ColorName,
                ARN.TechnicalNameId,kP.InActive,ARN.BItemReqID,ARN.YDBatchID
            ) 
			Select * From ActiveChildRolls ACR
		    --Where ACR.RollID Not IN(select BC.GRollID from BatchChild BC)
              Where ACR.RollID Not IN(select GRollID FROM {TableNames.YD_DYEING_BATCH_ITEM_ROLL})
            ;	


                        --Childs
                        WITH M AS (
                            --SELECT BC.*, RD.YDRecipeDInfoID, RD.Temperature FROM {TableNames.YD_BATCH_WISE_RECIPE_CHILD} BC
							--LEFT JOIN {TableNames.YD_RECIPE_DEFINITION_CHILD} RD ON RD.YDRecipeChildID = BC.YDRecipeChildID
							--WHERE BC.YDBatchID IN (29)
							SELECT RD.TempIn, BM.YDBatchID, RD.ProcessTime, RD.YDRecipeDInfoID, RD.Temperature FROM {TableNames.YD_BATCH_MASTER} BM
							INNER JOIN {TableNames.YD_BATCH_ITEM_REQUIREMENT} BC ON BC.YDBatchID = BM.YDBatchID
							INNER JOIN {TableNames.YD_RECIPE_REQ_MASTER} RR ON RR.YDBookingChildID = BC.YDBookingChildID
							INNER JOIN {TableNames.YD_RECIPE_DEFINITION_MASTER} RDM ON RDM.YDRecipeReqMasterID = RR.YDRecipeReqMasterID
							LEFT JOIN {TableNames.YD_RECIPE_DEFINITION_CHILD} RD ON RD.YDRecipeID = RDM.YDRecipeID
							WHERE BM.YDBatchID IN ({batchIDs})
                        )
                        SELECT DI.YDRecipeDInfoID, M.TempIn, M.YDBatchID, M.ProcessTime, EV.ValueName FiberPart, ISV.SegmentValue ColorName
					    FROM M
						LEFT JOIN {TableNames.YD_RECIPE_DEFINITION_DYEING_INFO} DI ON DI.YDRecipeDInfoID = M.YDRecipeDInfoID
						INNER JOIN {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID = DI.FiberPartID
						INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = DI.ColorID
						GROUP BY DI.YDRecipeDInfoID, M.TempIn, M.YDBatchID, M.ProcessTime, EV.ValueName, ISV.SegmentValue;

                        --Def Childs
                        SELECT RCD.YDBRecipeChildID, BM.YDBatchID, C.YDRecipeChildID, C.ProcessID, C.ParticularsID, C.RawItemID, C.Qty, C.UnitID, C.TempIn,
						C.TempOut, C.ProcessTime, C.YDRecipeID,I.ItemName, P.ValueName ProcessName,PL.ValueName ParticularsName,R.ItemName RawItemName,
						UU.DisplayUnitDesc Unit, C.IsPercentage, C.YDRecipeDInfoID, C.Temperature, EV.ValueName FiberPart, ISV.SegmentValue ColorName
						FROM {TableNames.YD_BATCH_MASTER} BM
						INNER JOIN {TableNames.YD_BATCH_ITEM_REQUIREMENT} BC ON BC.YDBatchID = BM.YDBatchID
						INNER JOIN {TableNames.YD_RECIPE_REQ_MASTER} RR ON RR.YDBookingChildID = BC.YDBookingChildID
						INNER JOIN {TableNames.YD_RECIPE_REQ_CHILD} RC ON RC.YDRecipeReqMasterID = RR.YDRecipeReqMasterID
						INNER JOIN {TableNames.YD_RECIPE_DEFINITION_MASTER} RDM ON RDM.YDRecipeReqMasterID = RR.YDRecipeReqMasterID
						LEFT JOIN {TableNames.YD_RECIPE_DEFINITION_CHILD} C ON C.YDRecipeID = RDM.YDRecipeID
						LEFT JOIN {TableNames.YD_BATCH_WISE_RECIPE_CHILD} RCD ON RCD.YDRecipeChildID = C.YDRecipeChildID
						LEFT JOIN {DbNames.EPYSL}..ItemMaster I ON I.ItemMasterID=C.RawItemID
						LEFT JOIN {DbNames.EPYSL}..EntityTypeValue P ON P.ValueID = C.ProcessID
						LEFT JOIN {DbNames.EPYSL}..EntityTypeValue PL ON PL.ValueID = C.ParticularsID
						LEFT JOIN {DbNames.EPYSL}..ItemMaster R ON R.ItemMasterID = C.RawItemID
						LEFT JOIN {DbNames.EPYSL}..Unit UU ON UU.UnitID = C.UnitID
						LEFT JOIN {TableNames.YD_RECIPE_DEFINITION_DYEING_INFO} DI ON DI.YDRecipeDInfoID = C.YDRecipeDInfoID
						INNER JOIN {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID = DI.FiberPartID
						INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = DI.ColorID
						WHERE BM.YDBatchID IN ({batchIDs});
                    ";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                List<YDDyeingBatchMaster> data = records.Read<YDDyeingBatchMaster>().ToList();
                var YDDyeingBatchItems = records.Read<YDDyeingBatchItem>().ToList();
                var YDDyeingBatchItemRolls = records.Read<YDDyeingBatchItemRoll>().ToList();
                var YDDyeingBatchRecipes = records.Read<YDDyeingBatchRecipe>().ToList();
                var defChilds = records.Read<YDRecipeDefinitionChild>().ToList();
                data.ForEach(m =>
                {
                    m.YDDyeingBatchItems = YDDyeingBatchItems.Where(x => x.YDBatchID == m.YDBatchID).ToList();
                    m.YDDyeingBatchItemRolls = YDDyeingBatchItemRolls.Where(x => x.YDBatchID == m.YDBatchID).ToList();
                    m.YDDyeingBatchItems.ForEach(mr =>
                    {
                        mr.YDDyeingBatchItemRolls = m.YDDyeingBatchItemRolls.Where(x => x.BItemReqID == mr.BItemReqID).ToList();
                        //if (mr.YDDyeingBatchItemRolls.Count == 0)
                        //{
                        //    m.YDDyeingBatchItems = new List<YDDyeingBatchItem>();
                        //}
                    });
                    m.YDDyeingBatchRecipes = YDDyeingBatchRecipes.Where(x => x.YDDBatchID == m.YDDBatchID).ToList();
                    m.DefChilds = defChilds.Where(x => x.YDDBatchID == m.YDDBatchID).ToList();
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
        public async Task<YDDyeingBatchMaster> GetAsync(int id)
        {
            var sql = $@"
                    ;WITH M AS (
                        SELECT	*
                        FROM {TableNames.YD_DYEING_BATCH_MASTER} DB WHERE YDDBatchID = {id}
                    )
                    SELECT M.YDDBatchID, M.YDDBatchNo, M.YDDBatchDate, M.YDRecipeID, M.ColorID, M.CCColorID, M.BatchWeightKG, M.BatchQtyPcs, M.DMID, M.MachineLoading, M.DyeingNozzleQty,
                    M.ShiftID, M.OperatorID, M.BatchStatus, M.PlanBatchStartTime, M.PlanBatchEndTime, M.ProductionDate, M.Remarks, RDM.YDRecipeNo, YDRecipeDate = RDM.RecipeDate, YDRecipeFor = RDM.RecipeFor,
					COL.SegmentValue ColorName, FR.ValueName YDRecipeForName, M.DMID, M.MachineLoading, M.DyeingNozzleQty, M.DyeingMcCapacity, DM.DyeingMcslNo DMNo, FC.GroupConceptNo ConceptNo, FC.GroupConceptNo SLNo
                    FROM M
                    LEFT JOIN {TableNames.YD_RECIPE_DEFINITION_MASTER} RDM ON RDM.YDRecipeID = M.YDRecipeID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID = M.ColorID
					LEFT JOIN {DbNames.EPYSL}..EntityTypeValue FR ON FR.ValueID = RDM.RecipeFor
					LEFT JOIN {TableNames.DYEING_MACHINE} DM ON DM.DMID = M.DMID
					LEFT JOIN {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} FCC ON FCC.CCColorID = M.CCColorID
					LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FC ON FC.ConceptID = FCC.ConceptID;

                    -----YDDyeingBatchWithBatchMaster
                    ;WITH M AS (
                        SELECT * FROM {TableNames.YD_DYEING_BATCH_WITH_BATCH_MASTER} BM WHERE BM.YDDBatchID = {id}
                    )
		            SELECT M.YDDBBMID, M.YDDBatchID, M.YDBatchID, M.BatchUseQtyKG, M.BatchUseQtyPcs, B.YDBatchNo
		            FROM M
		            INNER JOIN {TableNames.YD_BATCH_MASTER} B ON B.YDBatchID = M.YDBatchID;

                    ----for YDDyeingBatchItem
                    ;WITH M AS (
                        SELECT BM.*, YDBC.YarnCategory FROM {TableNames.YD_DYEING_BATCH_ITEM} BM 
						INNER JOIN {TableNames.YD_BATCH_ITEM_REQUIREMENT} YDBIR ON YDBIR.YDBItemReqID = BM.YDBItemReqID 
						INNER JOIN {TableNames.YDBookingChild} YDBC ON YDBC.YDBookingChildID = YDBIR.YDBookingChildID
                        WHERE BM.YDDBatchID = {id}
                    ),
                    ItemSegment As
                    (
	                    SELECT CAST(ISV.SegmentValueID As varchar) [id], ISV.SegmentValue [text], CAST(ISN.SegmentNameID As varchar) [desc]
	                    FROM {DbNames.EPYSL}..ItemSegmentName ISN
	                    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISN.SegmentNameID = ISV.SegmentNameID
	                    WHERE ISNULL(ISV.SegmentValue, '') <> '' And SegmentValueID NOT IN(17725,5127,1616,1969,47221,2{id}7,1616,1621,44382,44384,45969,46717,47220, 2155)
                    )
                    SELECT M.YDDBIID, M.YDDBatchID, M.YDBatchID, M.ItemSubGroupID, M.ItemMasterID, M.YDRecipeID, M.ConceptID, M.BookingID, M.ExportOrderID,
		            M.BuyerID, M.BuyerTeamID, M.QtyPcs, M.Qty, M.IsFloorRequistion,
                    B.YDBatchNo,  KnittingType.TypeName KnittingType,Composition.text FabricComposition, Construction.text FabricConstruction,
                    Technical.TechnicalName,Gsm.text FabricGsm, SG.SubGroupName ItemSubGroup, M.YarnCategory, M.YDBItemReqID
                    FROM M
                    INNER JOIN {TableNames.YD_BATCH_MASTER} B ON B.YDBatchID = M.YDBatchID
                    LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} CM ON CM.ConceptID = M.ConceptID
                    LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID=CM.KnittingTypeID
                    LEFT JOIN ItemSegment Composition ON Composition.id=CM.CompositionID
                    LEFT JOIN ItemSegment Construction ON Construction.id=CM.ConstructionID
                    LEFT JOIN ItemSegment Gsm ON Gsm.id=CM.GSMID
                    LEFT JOIN {DbNames.EPYSL}..ItemSubGroup SG ON SG.SubGroupID = CM.SubGroupID
                    LEFT JOIN {TableNames.FabricTechnicalName} Technical ON Technical.TechnicalNameId=CM.TechnicalNameId;

                    ----for YDDyeingBatchItemRoll
                    ;SELECT C.YDDBIRollID, C.YDDBIID, C.YDDBatchID, C.GRollID, C.ItemMasterID, C.RollQty, C.RollQtyPcs, KP.RollNo
                    FROM {TableNames.YD_DYEING_BATCH_ITEM_ROLL} C
                    INNER JOIN {TableNames.RND_KNITTING_PRODUCTION} KP ON KP.GRollID = C.GRollID
                    WHERE C.YDDBatchID = {id};

                    ----for YDDyeingBatchRecipe
                    ;WITH M AS (
                        SELECT	BM.*,RD.YDRecipeDInfoID
						FROM {TableNames.YD_DYEING_BATCH_RECIPE} BM
						LEFT JOIN {TableNames.YD_RECIPE_DEFINITION_CHILD} RD ON RD.YDRecipeChildID = BM.YDRecipeChildID
						WHERE BM.YDDBatchID = {id}
                    )
                    SELECT M.YDDBatchID, M.YDRecipeID, M.TempIn, M.ProcessTime,
					DI.YDRecipeDInfoID, EV.ValueName FiberPart,ISV.SegmentValue ColorName
					FROM M
					LEFT JOIN {TableNames.YD_RECIPE_DEFINITION_DYEING_INFO} DI ON DI.YDRecipeDInfoID = M.YDRecipeDInfoID
					INNER JOIN {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID = DI.FiberPartID
					INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = DI.ColorID
					GROUP BY M.YDDBatchID, M.YDRecipeID, M.TempIn, M.ProcessTime,
					DI.YDRecipeDInfoID,EV.ValueName,ISV.SegmentValue;

                    SELECT B.YDDBRID, B.YDDBatchID, B.YDRecipeChildID, B.YDDBatchID, B.YDRecipeChildID, B.ProcessID, B.ParticularsID, B.RawItemID, B.Qty, B.UnitID, B.TempIn,
					B.TempOut, B.ProcessTime, C.YDRecipeID,I.ItemName, P.ValueName ProcessName,PL.ValueName ParticularsName,R.ItemName RawItemName,
					UU.DisplayUnitDesc Unit, C.IsPercentage, C.YDRecipeDInfoID, C.Temperature, EV.ValueName FiberPart, ISV.SegmentValue ColorName
					FROM {TableNames.YD_DYEING_BATCH_RECIPE} B
					LEFT JOIN {TableNames.YD_RECIPE_DEFINITION_CHILD} C ON C.YDRecipeChildID = B.YDRecipeChildID
					LEFT JOIN {DbNames.EPYSL}..ItemMaster I ON I.ItemMasterID=C.RawItemID
					LEFT JOIN {DbNames.EPYSL}..EntityTypeValue P ON P.ValueID = C.ProcessID
					LEFT JOIN {DbNames.EPYSL}..EntityTypeValue PL ON PL.ValueID = C.ParticularsID
					LEFT JOIN {DbNames.EPYSL}..ItemMaster R ON R.ItemMasterID = C.RawItemID
					LEFT JOIN {DbNames.EPYSL}..Unit UU ON UU.UnitID = C.UnitID
					LEFT JOIN {TableNames.YD_RECIPE_DEFINITION_DYEING_INFO} DI ON DI.YDRecipeDInfoID = C.YDRecipeDInfoID
					INNER JOIN {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID = DI.FiberPartID
					INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = DI.ColorID
					WHERE B.YDDBatchID = {id};

                    ";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YDDyeingBatchMaster data = records.Read<YDDyeingBatchMaster>().FirstOrDefault();
                data.YDDyeingBatchWithBatchMasters = records.Read<YDDyeingBatchWithBatchMaster>().ToList();
                data.YDDyeingBatchItems = records.Read<YDDyeingBatchItem>().ToList();
                data.YDDyeingBatchItemRolls = records.Read<YDDyeingBatchItemRoll>().ToList();
                data.YDDyeingBatchRecipes = records.Read<YDDyeingBatchRecipe>().ToList();
                data.DefChilds = records.Read<YDRecipeDefinitionChild>().ToList();
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
        public async Task<List<YDDyeingBatchMaster>> GetBatchListAsync(string batchIds)
        {
            var sql = $@"WITH M AS (
                        SELECT	*
                        FROM {TableNames.YD_BATCH_MASTER} BM WHERE BM.YDBatchID NOT IN (SELECT YDBatchID FROM {TableNames.YD_DYEING_BATCH_WITH_BATCH_MASTER})
                        AND BM.YDBatchID NOT IN ({batchIds}) --BM.IsApproved = 1 AND
                    )
                    SELECT M.YDBatchID, M.YDBatchNo, M.YDBatchDate, M.YDRecipeID, M.GroupConceptNo, BookingID = 0, M.ColorID, M.BatchWeightKG, M.BatchWeightPcs BatchQtyPcs, M.Remarks,
                    M.ExportOrderID, M.BuyerID, M.BuyerTeamID, RDM.YDRecipeNo, YDRecipeDate = RDM.RecipeDate, YDRecipeFor = RDM.RecipeFor, COL.SegmentValue ColorName,
					FR.ValueName YDRecipeForName, Count(*) Over() TotalRows
                    FROM M
                    LEFT JOIN {TableNames.YD_RECIPE_DEFINITION_MASTER} RDM ON RDM.YDRecipeID = M.YDRecipeID
                    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID = M.ColorID
					LEFT JOIN {DbNames.EPYSL}..EntityTypeValue FR ON FR.ValueID = RDM.RecipeFor";

            return await _service.GetDataAsync<YDDyeingBatchMaster>(sql);
        }
        public async Task<List<YDDyeingBatchMaster>> GetBatchDetails(string batchIds)
        {
            var sql = $@"
                    ;WITH M AS (
                        SELECT	*
                        FROM {TableNames.YD_BATCH_MASTER} BM WHERE BM.YDBatchID IN ({batchIds})
                    )
                    SELECT M.YDBatchID, M.YDBatchNo, M.YDBatchDate, M.YDRecipeID, M.ConceptID, M.ColorID, M.CCColorID, M.BatchWeightKG, M.BatchWeightPcs BatchQtyPcs, M.Remarks,
                    M.ExportOrderID, M.BuyerID, M.BuyerTeamID, RDM.YDRecipeNo, YDRecipeDate = RDM.RecipeDate, YDRecipeFor = RDM.RecipeFor, COL.SegmentValue ColorName,
					FR.ValueName YDRecipeForName, Count(*) Over() TotalRows
                    FROM M
                    LEFT JOIN {TableNames.YD_RECIPE_DEFINITION_MASTER} RDM ON RDM.YDRecipeID = M.YDRecipeID
                    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID = M.ColorID
					LEFT JOIN {DbNames.EPYSL}..EntityTypeValue FR ON FR.ValueID = RDM.RecipeFor;

                    -----for YDDyeingBatchItem
                    ;WITH M AS (
                        SELECT BM.*, YDBC.YarnCategory FROM {TableNames.YD_BATCH_ITEM_REQUIREMENT} BM 
						INNER JOIN {TableNames.YDBookingChild} YDBC ON YDBC.YDBookingChildID = BM.YDBookingChildID
                        WHERE BM.YDBatchID IN ({batchIds})
                    ),
                    ItemSegment As
                    (
	                    SELECT CAST(ISV.SegmentValueID As varchar) [id], ISV.SegmentValue [text], CAST(ISN.SegmentNameID As varchar) [desc]
	                    FROM {DbNames.EPYSL}..ItemSegmentName ISN
	                    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISN.SegmentNameID = ISV.SegmentNameID
	                    WHERE ISNULL(ISV.SegmentValue, '') <> '' And SegmentValueID NOT IN(17725,5127,1616,1969,47221,2{batchIds}7,1616,1621,44382,44384,45969,46717,47220, 2155)
                    )
                    SELECT M.YDBItemReqID, M.YDBatchID, M.YDRecipeItemInfoID, M.ItemMasterID, M.ConceptID, QtyPcs = M.Pcs, M.Qty, M.IsFloorRequistion, RDM.YDRecipeID, CM.ConceptID, CM.BookingID, RDM.SubGroupID ItemSubGroupID,
                    B.YDBatchNo, B.BuyerID, B.BuyerTeamID, B.ExportOrderID, KnittingType.TypeName KnittingType,Composition.text FabricComposition, Construction.text FabricConstruction,
                    Technical.TechnicalName,Gsm.text FabricGsm, SG.SubGroupName ItemSubGroup, M.YarnCategory
                    FROM M
                    LEFT JOIN {TableNames.YD_RECIPE_DEFINITION_ITEM_INFO} RDM ON RDM.YDRecipeItemInfoID = M.YDRecipeItemInfoID
                    INNER JOIN {TableNames.YD_BATCH_MASTER} B ON B.YDBatchID = M.YDBatchID
                    LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} CM ON CM.ConceptID = B.ConceptID
                    LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID=CM.KnittingTypeID
                    LEFT JOIN ItemSegment Composition ON Composition.id=CM.CompositionID
                    LEFT JOIN ItemSegment Construction ON Construction.id=CM.ConstructionID
                    LEFT JOIN ItemSegment Gsm ON Gsm.id=CM.GSMID
                    LEFT JOIN {DbNames.EPYSL}..ItemSubGroup SG ON SG.SubGroupID = CM.SubGroupID
                    LEFT JOIN {TableNames.FabricTechnicalName} Technical ON Technical.TechnicalNameId=CM.TechnicalNameId;

                    ----for YDDyeingBatchItemRoll
                    ;SELECT C.BItemReqID, C.YDBatchID, C.GRollID, C.ItemMasterID, C.RollQty, C.RollQtyPcs, KP.RollNo
                    FROM {TableNames.YD_BATCH_CHILD} C
                    INNER JOIN {TableNames.RND_KNITTING_PRODUCTION} KP ON KP.GRollID = C.GRollID
                    WHERE C.YDBatchID IN ({batchIds});

                    ----for YDDyeingBatchRecipe
                    ;WITH M AS (
                        SELECT	* FROM {TableNames.YD_BATCH_WISE_RECIPE_CHILD} BM WHERE BM.YDBatchID IN ({batchIds})
                    )
                    SELECT M.YDRecipeChildID, M.ProcessID, M.ParticularsID, M.RawItemID, M.Qty, M.UnitID, M.TempIn, M.TempOut, M.ProcessTime, RDM.YDRecipeID,
					P.ValueName ProcessName,PL.ValueName ParticularsName,R.ItemName RawItemName, UU.DisplayUnitDesc Unit
                    FROM M
                    LEFT JOIN {TableNames.YD_RECIPE_DEFINITION_CHILD} RDM ON RDM.YDRecipeChildID = M.YDRecipeChildID
					LEFT JOIN {DbNames.EPYSL}..EntityTypeValue P ON P.ValueID = M.ProcessID
					LEFT JOIN {DbNames.EPYSL}..EntityTypeValue PL ON PL.ValueID = M.ParticularsID
					LEFT JOIN {DbNames.EPYSL}..ItemMaster R ON R.ItemMasterID = M.RawItemID
					LEFT JOIN {DbNames.EPYSL}..Unit UU ON UU.UnitID = M.UnitID;

                    ";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                List<YDDyeingBatchMaster> data = records.Read<YDDyeingBatchMaster>().ToList();
                List<YDDyeingBatchItem> YDDyeingBatchItems = records.Read<YDDyeingBatchItem>().ToList();
                List<YDDyeingBatchItemRoll> YDDyeingBatchItemRolls = records.Read<YDDyeingBatchItemRoll>().ToList();
                List<YDDyeingBatchRecipe> YDDyeingBatchRecipes = records.Read<YDDyeingBatchRecipe>().ToList();

                foreach (YDDyeingBatchMaster batch in data)
                {
                    batch.YDDyeingBatchItems = YDDyeingBatchItems.Where(x => x.YDBatchID == batch.YDBatchID).ToList();
                    foreach (YDDyeingBatchItem item in batch.YDDyeingBatchItems)
                    {
                        item.YDDyeingBatchItemRolls = YDDyeingBatchItemRolls.Where(x => x.BItemReqID == item.BItemReqID).ToList();
                    }

                    batch.YDDyeingBatchRecipes = YDDyeingBatchRecipes.Where(x => x.YDRecipeID == batch.YDRecipeID).ToList();
                }

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
        public async Task<List<YDDyeingBatchChildFinishingProcess>> GetFinishingProcessAsync(int conceptId, int colorID)
        {
            var sql = $@"WITH FC AS (
	                SELECT FCM.ConceptID
	                FROM {TableNames.RND_FREE_CONCEPT_MASTER} FC
	                INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.GroupConceptNo = FC.GroupConceptNo
	                WHERE FC.ConceptID = {conceptId}
                    ),
                    FPC AS (
                    SELECT FPC.*, FP.ConceptID
                    FROM {TableNames.FINISHING_PROCESS_CHILD} FPC
                    INNER JOIN {TableNames.FINISHING_PROCESS_MASTER} FP ON FP.FPMasterID = FPC.FPMasterID
	                INNER JOIN FC ON FC.ConceptID = FP.ConceptID
                    WHERE FPC.ColorID = {colorID} AND FPC.IsPreProcess = 0
                    )
                    SELECT ROW_NUMBER() OVER(ORDER BY FPC.SeqNo ASC) RFinishingID, FPC.ConceptID, FPC.FPChildID, FPC.FPMasterID, FPC.ProcessID, FPC.SeqNo, FMP.ProcessName,
                    C.ShortName UnitName,b.ValueName BrandName, FPC.ProcessTypeID, ET.ValueName ProcessType, FPC.IsPreProcess, FPC.FMSID,FMC.FMCMasterID,
                    FMS.MachineNo, MachineName=FMC.ProcessName, FPC.Remarks, FPC.Param1Value, FPC.Param2Value, FPC.Param3Value, FPC.Param4Value, FPC.Param5Value,
                    FPC.Param6Value, FPC.Param7Value, FPC.Param8Value, FPC.Param9Value, FPC.Param10Value, FPC.Param11Value, FPC.Param12Value, FPC.Param13Value,
                    FPC.Param14Value, FPC.Param15Value, FPC.Param16Value, FPC.Param17Value, FPC.Param18Value,
                    FPC.Param19Value, FPC.Param20Value, FMS.UnitID, FMS.BrandID
                    FROM FPC
                    INNER JOIN {TableNames.FinishingMachineProcess_HK} FMP On FMP.FMProcessID = FPC.ProcessID
                    INNER JOIN {TableNames.FINISHING_MACHINE_CONFIGURATION_MASTER} FMC ON FMC.FMCMasterID = FMP.FMCMasterID
                    LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ET ON ET.ValueID = FPC.ProcessTypeID
                    LEFT JOIN {TableNames.FINISHING_MACHINE_SETUP} FMS On FMS.FMSID = FPC.FMSID
                    Left Join {DbNames.EPYSL}..EntityTypeValue b on b.ValueID = FMS.BrandID
                    Left JOIN {TableNames.KNITTING_UNIT} c on c.KnittingUnitID = FMS.UnitID
                    GROUP BY FPC.FPChildID, FPC.ConceptID, FPC.FPMasterID, FPC.ProcessID, FPC.SeqNo, FMP.ProcessName, C.ShortName,b.ValueName, FPC.ProcessTypeID, ET.ValueName,
                    FPC.IsPreProcess, FPC.FMSID,FMC.FMCMasterID, FMS.MachineNo, FMC.ProcessName, FPC.Remarks, FPC.Param1Value, FPC.Param2Value, FPC.Param3Value,
                    FPC.Param4Value, FPC.Param5Value, FPC.Param6Value, FPC.Param7Value, FPC.Param8Value, FPC.Param9Value, FPC.Param10Value, FPC.Param11Value,
                    FPC.Param12Value, FPC.Param13Value, FPC.Param14Value, FPC.Param15Value, FPC.Param16Value, FPC.Param17Value, FPC.Param18Value,
                    FPC.Param19Value, FPC.Param20Value, FMS.UnitID, FMS.BrandID
                    ORDER BY FPC.SeqNo ASC";

            return await _service.GetDataAsync<YDDyeingBatchChildFinishingProcess>(sql);
        }
        public async Task<List<YDDyeingBatchChildFinishingProcess>> GetFinishingProcessByYDDyeingBatchAsync(int dBatchID, int colorID)
        {
            var sql = $@";With A As(
							Select DBM.YDDBatchID, DBI.ConceptID, DBM.ColorID
							FROM {TableNames.YD_DYEING_BATCH_ITEM} DBI
							Inner JOIN {TableNames.YD_DYEING_BATCH_MASTER} DBM ON DBM.YDDBatchID = DBI.YDDBatchID
							WHERE DBM.YDDBatchID = {dBatchID} And DBM.colorID = {colorID}
							Group By DBM.YDDBatchID, DBI.ConceptID, DBM.ColorID
						),FPC AS (
                    SELECT FPC.*, FP.ConceptID
                    FROM {TableNames.FINISHING_PROCESS_CHILD} FPC
                    INNER JOIN {TableNames.FINISHING_PROCESS_MASTER} FP ON FP.FPMasterID = FPC.FPMasterID
                    Inner Join A On A.ConceptID = FP.ConceptID And A.ColorID = FPC.ColorID
                    WHERE FPC.IsPreProcess = 0
                    )
                    SELECT ROW_NUMBER() OVER(ORDER BY FPC.SeqNo ASC) RFinishingID, FPC.ConceptID, FPC.FPChildID, FPC.FPMasterID, FPC.ProcessID, FPC.SeqNo, FMP.ProcessName,
                    C.ShortName UnitName,b.ValueName BrandName, FPC.ProcessTypeID, ET.ValueName ProcessType, FPC.IsPreProcess, FPC.FMSID,FMC.FMCMasterID,
                    FMS.MachineNo, MachineName=FMC.ProcessName, FPC.Remarks, FPC.Param1Value, FPC.Param2Value, FPC.Param3Value, FPC.Param4Value, FPC.Param5Value,
                    FPC.Param6Value, FPC.Param7Value, FPC.Param8Value, FPC.Param9Value, FPC.Param10Value, FPC.Param11Value, FPC.Param12Value, FPC.Param13Value,
                    FPC.Param14Value, FPC.Param15Value, FPC.Param16Value, FPC.Param17Value, FPC.Param18Value,
                    FPC.Param19Value, FPC.Param20Value, FMS.UnitID, FMS.BrandID
                    FROM FPC
                    INNER JOIN {TableNames.FinishingMachineProcess_HK} FMP On FMP.FMProcessID = FPC.ProcessID
                    INNER JOIN {TableNames.FINISHING_MACHINE_CONFIGURATION_MASTER} FMC ON FMC.FMCMasterID = FMP.FMCMasterID
                    LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ET ON ET.ValueID = FPC.ProcessTypeID
                    LEFT JOIN {TableNames.FINISHING_MACHINE_SETUP} FMS On FMS.FMSID = FPC.FMSID
                    Left Join {DbNames.EPYSL}..EntityTypeValue b on b.ValueID = FMS.BrandID
                    Left JOIN {TableNames.KNITTING_UNIT} c on c.KnittingUnitID = FMS.UnitID
                    GROUP BY FPC.FPChildID, FPC.ConceptID, FPC.FPMasterID, FPC.ProcessID, FPC.SeqNo, FMP.ProcessName, C.ShortName,b.ValueName, FPC.ProcessTypeID, ET.ValueName,
                    FPC.IsPreProcess, FPC.FMSID,FMC.FMCMasterID, FMS.MachineNo, FMC.ProcessName, FPC.Remarks, FPC.Param1Value, FPC.Param2Value, FPC.Param3Value,
                    FPC.Param4Value, FPC.Param5Value, FPC.Param6Value, FPC.Param7Value, FPC.Param8Value, FPC.Param9Value, FPC.Param10Value, FPC.Param11Value,
                    FPC.Param12Value, FPC.Param13Value, FPC.Param14Value, FPC.Param15Value, FPC.Param16Value, FPC.Param17Value, FPC.Param18Value,
                    FPC.Param19Value, FPC.Param20Value, FMS.UnitID, FMS.BrandID
                    ORDER BY FPC.SeqNo ASC";

            return await _service.GetDataAsync<YDDyeingBatchChildFinishingProcess>(sql);
        }
        public async Task<List<YDDyeingBatchMaster>> GetYDDyeingBatchs(PaginationInfo paginationInfo, string colorName, string conceptNo)
        {
            if (colorName.IsNullOrEmpty() || conceptNo.IsNullOrEmpty()) return new List<YDDyeingBatchMaster>();

            paginationInfo.OrderBy = string.IsNullOrEmpty(paginationInfo.OrderBy) ? "ORDER BY ConceptNo, ColorName, DBatchNo" : paginationInfo.OrderBy;
            var sql = $@"
                SELECT DB.YDDBatchID, DB.YDDBatchNo, DB.YDDBatchDate, DB.CCColorID, DB.ColorID, COL.SegmentValue ColorName,FCM.GroupConceptNo [ConceptNo]
                FROM {TableNames.YD_DYEING_BATCH_MASTER} DB 
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID = DB.ColorID
                LEFT JOIN {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} FCC ON FCC.CCColorID = DB.CCColorID
                LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = FCC.ConceptID
                WHERE DB.YDRecipeID = 0 AND FCM.GroupConceptNo LIKE '%{conceptNo}%' AND FCC.ColorName LIKE '%{colorName}%'
                {paginationInfo.FilterBy}
                {paginationInfo.OrderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<YDDyeingBatchMaster>(sql);
        }
        public async Task SaveAsync(YDDyeingBatchMaster entity)
        {
            SqlTransaction transaction = null;
            SqlTransaction transactionGmt = null;
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                await _connectionGmt.OpenAsync();
                transactionGmt = _connectionGmt.BeginTransaction();
                switch (entity.EntityState)
                {
                    case EntityState.Added:
                        entity = await AddAsync(entity, transactionGmt);
                        break;

                    case EntityState.Modified:
                        entity = await UpdateAsync(entity, transactionGmt);
                        break;

                    default:
                        break;
                }

                await _service.SaveSingleAsync(entity, transaction);
                await _service.SaveAsync(entity.YDDyeingBatchWithBatchMasters, transaction);
                await _service.SaveAsync(entity.YDDyeingBatchRecipes, transaction);
                await _service.SaveAsync(entity.YDDyeingBatchItems, transaction);
                List<YDDyeingBatchItemRoll> rollList = new List<YDDyeingBatchItemRoll>();
                entity.YDDyeingBatchItems.ForEach(x => rollList.AddRange(x.YDDyeingBatchItemRolls));
                await _service.SaveAsync(rollList, transaction);
                await _service.SaveAsync(entity.YDDyeingBatchChildFinishingProcesses, transaction);
                await _service.SaveAsync(entity.YDDyeingBatchMergeBatchs, transaction);
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
        public async Task SaveAsyncRecipeCopy(YDDyeingBatchMaster entity)
        {
            SqlTransaction transaction = null;
            SqlTransaction transactionGmt = null;
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                await _connectionGmt.OpenAsync();
                transactionGmt = _connectionGmt.BeginTransaction();
                switch (entity.EntityState)
                {
                    case EntityState.Added:
                        entity = await AddAsync(entity, transactionGmt);
                        break;

                    case EntityState.Modified:
                        entity = await UpdateAsync(entity, transactionGmt);
                        break;

                    default:
                        break;
                }
                await _service.SaveSingleAsync(entity, transaction);
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
        private async Task<YDDyeingBatchMaster> AddAsync(YDDyeingBatchMaster entity, SqlTransaction transactionGmt)
        {
            entity.YDDBatchID = await _service.GetMaxIdAsync(TableNames.YD_DYEING_BATCH_MASTER, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

            //entity.DBatchNo = await _signatureRepository.GetMaxNoAsync(TableNames.DYEING_BATCH_NO);

            int paddingValue = 3;
            int maxNo = await _service.GetMaxNoAsync(TableNames.YD_DYEING_BATCH_MASTER, "YDDBatchNo", entity.SLNo, entity.SLNo.Length + paddingValue, _connection);
            entity.YDDBatchNo = entity.SLNo + maxNo.ToString().PadLeft(paddingValue, '0');

            var maxChildId = await _service.GetMaxIdAsync(TableNames.YD_DYEING_BATCH_WITH_BATCH_MASTER, entity.YDDyeingBatchWithBatchMasters.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
            var maxItemId = await _service.GetMaxIdAsync(TableNames.YD_DYEING_BATCH_ITEM, entity.YDDyeingBatchItems.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
            var maxItemRollId = await _service.GetMaxIdAsync(TableNames.YD_DYEING_BATCH_ITEM_ROLL, entity.YDDyeingBatchItems.Sum(x => x.YDDyeingBatchItemRolls.Count()), RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
            var maxRecipeId = await _service.GetMaxIdAsync(TableNames.YD_DYEING_BATCH_RECIPE, entity.YDDyeingBatchRecipes.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
            var maxProcessId = await _service.GetMaxIdAsync(TableNames.YD_DYEING_BATCH_CHILD_FINISHING_PROCESS, entity.YDDyeingBatchChildFinishingProcesses.Where(x => x.EntityState == EntityState.Added).Count(), RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
            var maxMergeBatchId = await _service.GetMaxIdAsync(TableNames.YD_DYEING_BATCH_MERGE_BATCH, entity.YDDyeingBatchMergeBatchs.Count(), RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

            foreach (YDDyeingBatchWithBatchMaster item in entity.YDDyeingBatchWithBatchMasters)
            {
                item.YDDBBMID = maxChildId++;
                item.YDDBatchID = entity.YDDBatchID;
                item.EntityState = EntityState.Added;
            }
            if (entity.IsRework)
            {
                foreach (YDDyeingBatchMergeBatch item in entity.YDDyeingBatchMergeBatchs)
                {
                    item.YDDBMID = maxMergeBatchId++;
                    item.YDDBatchID = entity.YDDBatchID;
                    item.EntityState = EntityState.Added;
                }
            }
            foreach (YDDyeingBatchItem item in entity.YDDyeingBatchItems)
            {
                List<YDDyeingBatchChildFinishingProcess> lstYDDyeingBatchChildFinishingProcess = entity.YDDyeingBatchChildFinishingProcesses.ToList().Where(x => x.YDDBIID == item.YDDBIID).ToList();
                foreach (YDDyeingBatchChildFinishingProcess child in lstYDDyeingBatchChildFinishingProcess)
                {
                    switch (child.EntityState)
                    {
                        case EntityState.Added:
                            child.YDDBIID = maxItemId;
                            break;

                        default:
                            break;
                    }
                }

                item.YDDBIID = maxItemId++;
                item.YDDBatchID = entity.YDDBatchID;
                item.EntityState = EntityState.Added;
                foreach (YDDyeingBatchItemRoll child in item.YDDyeingBatchItemRolls)
                {
                    child.YDDBIRollID = maxItemRollId++;
                    child.YDDBIID = item.YDDBIID;
                    child.YDDBatchID = entity.YDDBatchID;
                    child.EntityState = EntityState.Added;
                }
            }
            foreach (YDDyeingBatchRecipe item in entity.YDDyeingBatchRecipes)
            {
                item.YDDBRID = maxRecipeId++;
                item.YDDBatchID = entity.YDDBatchID;
                item.EntityState = EntityState.Added;
            }
            foreach (YDDyeingBatchChildFinishingProcess item in entity.YDDyeingBatchChildFinishingProcesses)
            {
                item.YDDBCFPID = maxProcessId++;
                item.YDDBatchID = entity.YDDBatchID;
                item.EntityState = EntityState.Added;
            }
            return entity;
        }
        private async Task<YDDyeingBatchMaster> UpdateAsync(YDDyeingBatchMaster entity, SqlTransaction transactionGmt)
        {
            var maxChildId = await _service.GetMaxIdAsync(TableNames.YD_DYEING_BATCH_WITH_BATCH_MASTER, entity.YDDyeingBatchWithBatchMasters.Where(x => x.EntityState == EntityState.Added).Count(), RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
            var maxItemId = await _service.GetMaxIdAsync(TableNames.YD_DYEING_BATCH_ITEM, entity.YDDyeingBatchItems.Where(x => x.EntityState == EntityState.Added).Count(), RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
            var maxItemRollId = await _service.GetMaxIdAsync(TableNames.YD_DYEING_BATCH_ITEM_ROLL, entity.YDDyeingBatchItems.Sum(x => x.YDDyeingBatchItemRolls.Where(y => y.EntityState == EntityState.Added).Count()), RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
            var maxRecipeId = await _service.GetMaxIdAsync(TableNames.YD_DYEING_BATCH_RECIPE, entity.YDDyeingBatchRecipes.Where(x => x.EntityState == EntityState.Added).Count(), RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
            var maxProcessId = await _service.GetMaxIdAsync(TableNames.YD_DYEING_BATCH_CHILD_FINISHING_PROCESS, entity.YDDyeingBatchChildFinishingProcesses.Where(x => x.EntityState == EntityState.Added).Count(), RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
            var maxMergeBatchId = await _service.GetMaxIdAsync(TableNames.YD_DYEING_BATCH_MERGE_BATCH, entity.YDDyeingBatchMergeBatchs.Where(x => x.EntityState == EntityState.Added).Count(), RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

            foreach (YDDyeingBatchWithBatchMaster item in entity.YDDyeingBatchWithBatchMasters.ToList())
            {
                switch (item.EntityState)
                {
                    case EntityState.Added:
                        item.YDDBBMID = maxChildId++;
                        item.YDDBatchID = entity.YDDBatchID;
                        item.EntityState = EntityState.Added;
                        break;

                    case EntityState.Modified:
                        item.EntityState = EntityState.Modified;
                        break;

                    case EntityState.Unchanged:
                    case EntityState.Deleted:
                        item.EntityState = EntityState.Deleted;
                        break;

                    default:
                        break;
                }
            }
            foreach (YDDyeingBatchItem item in entity.YDDyeingBatchItems.ToList())
            {
                foreach (YDDyeingBatchItemRoll child in item.YDDyeingBatchItemRolls.ToList())
                {
                    switch (child.EntityState)
                    {
                        case EntityState.Added:
                            child.YDDBIRollID = maxItemRollId++;
                            child.YDDBIID = maxItemId;
                            child.YDDBatchID = entity.YDDBatchID;
                            child.EntityState = EntityState.Added;
                            break;

                        case EntityState.Deleted:
                        case EntityState.Unchanged:
                            child.EntityState = EntityState.Deleted;
                            break;

                        case EntityState.Modified:
                            child.EntityState = EntityState.Modified;
                            break;

                        default:
                            break;
                    }
                }
                switch (item.EntityState)
                {
                    case EntityState.Added:
                        item.YDDBIID = maxItemId++;
                        item.YDDBatchID = entity.YDDBatchID;
                        item.EntityState = EntityState.Added;
                        break;

                    case EntityState.Modified:
                        item.EntityState = EntityState.Modified;
                        break;

                    case EntityState.Deleted:
                    case EntityState.Unchanged:
                        item.EntityState = EntityState.Deleted;
                        break;

                    default:
                        break;
                }
            }
            foreach (YDDyeingBatchRecipe item in entity.YDDyeingBatchRecipes.ToList())
            {
                switch (item.EntityState)
                {
                    case EntityState.Added:
                        item.YDDBRID = maxRecipeId++;
                        item.YDDBatchID = entity.YDDBatchID;
                        item.EntityState = EntityState.Added;
                        break;

                    case EntityState.Modified:
                        item.EntityState = EntityState.Modified;
                        break;

                    case EntityState.Unchanged:
                    case EntityState.Deleted:
                        item.EntityState = EntityState.Deleted;
                        break;

                    default:
                        break;
                }
            }
            foreach (YDDyeingBatchChildFinishingProcess item in entity.YDDyeingBatchChildFinishingProcesses.ToList())
            {
                switch (item.EntityState)
                {
                    case EntityState.Added:

                        item.YDDBCFPID = maxProcessId++;
                        item.YDDBatchID = entity.YDDBatchID;
                        item.EntityState = EntityState.Added;
                        break;

                    case EntityState.Modified:
                        item.EntityState = EntityState.Modified;
                        break;

                    case EntityState.Unchanged:
                    case EntityState.Deleted:
                        item.EntityState = EntityState.Deleted;
                        break;

                    default:
                        break;
                }
            }
            foreach (YDDyeingBatchMergeBatch item in entity.YDDyeingBatchMergeBatchs.Where(x => x.EntityState == EntityState.Added).ToList())
            {
                item.YDDBMID = maxMergeBatchId++;
                item.YDDBatchID = entity.YDDBatchID;
                item.EntityState = EntityState.Added;
            }
            return entity;
        }
        public async Task<YDDyeingBatchMaster> GetAllByIDAsync(int id)
        {
            string sql = $@"
            ;Select * FROM {TableNames.YD_DYEING_BATCH_MASTER} Where YDDBatchID = {id}

            ;Select * FROM {TableNames.YD_DYEING_BATCH_WITH_BATCH_MASTER} Where YDDBatchID = {id}

            ;Select * FROM {TableNames.YD_DYEING_BATCH_RECIPE} Where YDDBatchID = {id}

            ;Select * FROM {TableNames.YD_DYEING_BATCH_ITEM} Where YDDBatchID = {id}

            ;Select * FROM {TableNames.YD_DYEING_BATCH_ITEM_ROLL} Where YDDBatchID = {id}

            ;SELECT * FROM {TableNames.YD_DYEING_BATCH_CHILD_FINISHING_PROCESS} WHERE YDDBatchID = {id}

            ;Select * From {TableNames.YD_DYEING_BATCH_MERGE_BATCH} Where YDDBatchID = {id}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YDDyeingBatchMaster data = records.Read<YDDyeingBatchMaster>().FirstOrDefault();
                Guard.Against.NullObject(data);
                data.YDDyeingBatchWithBatchMasters = records.Read<YDDyeingBatchWithBatchMaster>().ToList();
                data.YDDyeingBatchRecipes = records.Read<YDDyeingBatchRecipe>().ToList();
                data.YDDyeingBatchItems = records.Read<YDDyeingBatchItem>().ToList();
                data.YDDyeingBatchItemRolls = records.Read<YDDyeingBatchItemRoll>().ToList();
                foreach (YDDyeingBatchItem item in data.YDDyeingBatchItems)
                {
                    item.YDDyeingBatchItemRolls = data.YDDyeingBatchItemRolls.Where(x => x.YDDBIID == item.YDDBIID).ToList();
                }
                data.YDDyeingBatchChildFinishingProcesses = records.Read<YDDyeingBatchChildFinishingProcess>().ToList();
                data.YDDyeingBatchMergeBatchs = records.Read<YDDyeingBatchMergeBatch>().ToList();
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
        public async Task UpdateEntityAsync(YDDyeingBatchMaster entity)
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
