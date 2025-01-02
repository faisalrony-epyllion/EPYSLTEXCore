using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Application.Interfaces.RND;
using EPYSLTEXCore.Infrastructure.Data;
using Dapper;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Application.Interfaces.RND;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using System.Data;
using System.Data.Entity;
using Microsoft.Data.SqlClient;

namespace EPYSLTEXCore.Application.Services.RND
{
    public class DyeingBatchService : IDyeingBatchService
    {
        private readonly IDapperCRUDService<DyeingBatchMaster> _service;

        private readonly SqlConnection _connection;
        private readonly SqlConnection _connectionGmt;
        private SqlTransaction transaction;
        private SqlTransaction transactionGmt;
        public DyeingBatchService(IDapperCRUDService<DyeingBatchMaster> service)
            
        {
            _service = service;
            _service.Connection = service.GetConnection(AppConstants.GMT_CONNECTION);
            _connectionGmt = service.Connection;

            _service.Connection = service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _connection = service.Connection;
        }

        public async Task<List<DyeingBatchMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By DBatchID Desc" : paginationInfo.OrderBy;
            var sql = string.Empty;
            if (status == Status.Pending)
            {
                sql += $@";WITH DyeingBatch AS (
	                     SELECT DB.BatchID, DBM.DBatchID, DBM.DBatchNo, SUM(DB.BatchUseQtyKG) TotalBatchUseQtyKG, SUM(DB.BatchUseQtyPcs) TotalBatchUseQtyPcs
	                    ,BatchStatus = ISNULL(DBR.BatchStatus,0), IsNewBatch = ISNULL(DBR.IsNewBatch,0), IsNewRecipe = ISNULL(DBR.IsNewRecipe,0)
	                    FROM DyeingBatchWithBatchMaster DB
	                    LEFT JOIN DyeingBatchMaster DBM ON DBM.DBatchID = DB.DBatchID
	                    LEFT JOIN DyeingBatchRework DBR ON DBR.DBatchID = DB.DBatchID
	                    --WHERE BatchID IS NULL
	                    GROUP BY DB.BatchID, DBM.DBatchID, DBM.DBatchNo, ISNULL(DBR.BatchStatus,0), ISNULL(DBR.IsNewBatch,0), ISNULL(DBR.IsNewRecipe,0)
                    ),
                    M AS (
                        SELECT BM.*, DyeingBatch.DBatchID, DyeingBatch.TotalBatchUseQtyKG, FBA.SLNo, 
	                    BuyerName = CASE WHEN BM.BuyerID = 0 THEN 'R&D' ELSE CTO.ShortName END, 
	                    BuyerTeamName = CASE WHEN BM.BuyerTeamID = 0 THEN 'R&D' ELSE CCT.TeamName END,
	                    [Status] = 'New', ExistingDBatchNo = ''
                        FROM BatchMaster BM
                        LEFT JOIN BatchChild BC on BM.BatchID=BC.BatchID
                        LEFT JOIN FBookingAcknowledge FBA ON FBA.BookingID=BM.BookingID
	                    LEFT JOIN DyeingBatch ON DyeingBatch.BatchID = BM.BatchID
	                    LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = BM.BuyerID
	                    LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = BM.BuyerTeamID
	                    WHERE (DyeingBatch.BatchID IS NULL OR DyeingBatch.TotalBatchUseQtyKG < BM.BatchWeightKG)
						AND BC.GRollID not in (select GRollID from DyeingBatchItemRoll)
                    ),
                    ReworkDyeingBatch AS (
	                     SELECT DB.BatchID, DBM.DBatchID, DBM.DBatchNo, SUM(DB.BatchUseQtyKG) TotalBatchUseQtyKG, SUM(DB.BatchUseQtyPcs) TotalBatchUseQtyPcs
	                    ,BatchStatus = ISNULL(DBR.BatchStatus,0), IsNewBatch = ISNULL(DBR.IsNewBatch,0), IsNewRecipe = ISNULL(DBR.IsNewRecipe,0)
	                    ,[Status] = 'Rework', ExistingDBatchNo = DBM.DBatchNo
	                    FROM DyeingBatchWithBatchMaster DB 
	                    INNER JOIN DyeingBatchMaster DBM ON DBM.DBatchID = DB.DBatchID
	                    INNER JOIN DyeingBatchRework DBR ON DBR.DBatchID = DBM.DBatchID
	                    LEFT JOIN DyeingBatchMergeBatch DBB ON DBB.MergeDBatchID = DBM.DBatchID
	                    WHERE DBM.BatchStatus = 3 AND DBM.IsNewBatch = 1
	                    AND DBB.DBMID IS NULL
	                    GROUP BY DB.BatchID, DBM.DBatchID, DBM.DBatchNo, ISNULL(DBR.BatchStatus,0), ISNULL(DBR.IsNewBatch,0), ISNULL(DBR.IsNewRecipe,0)
                    ),
                    ReworkM AS (
                        SELECT BM.*, RDB.DBatchID, RDB.TotalBatchUseQtyKG, FBA.SLNo, 
	                    BuyerName = CASE WHEN BM.BuyerID = 0 THEN 'R&D' ELSE CTO.ShortName END, 
	                    BuyerTeamName = CASE WHEN BM.BuyerTeamID = 0 THEN 'R&D' ELSE CCT.TeamName END,
	                    RDB.[Status], ExistingDBatchNo
                        FROM BatchMaster BM
                        LEFT JOIN FBookingAcknowledge FBA ON FBA.BookingID=BM.BookingID
	                    INNER JOIN ReworkDyeingBatch RDB ON RDB.BatchID = BM.BatchID
	                    LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = BM.BuyerID
	                    LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = BM.BuyerTeamID
                    ),
                    FFF AS (
                        SELECT M.BatchID, M.BatchNo, M.BatchDate, DBatchID = ISNULL(M.DBatchID,0), M.RecipeID, FCM.ConceptID, M.BookingID, M.ColorID, M.CCColorID, M.BatchWeightKG, M.BatchWeightPcs BatchQtyPcs, M.Remarks,
                        M.ExportOrderID, M.BuyerID, M.BuyerTeamID, RDM.RecipeNo, RDM.RecipeDate, RDM.RecipeFor, COL.SegmentValue ColorName, M.BuyerName, M.BuyerTeamName,
                        FR.ValueName RecipeForName, M.TotalBatchUseQtyKG, FCM.GroupConceptNo ConceptNo, M.SLNo, M.MachineLoading, M.DyeingNozzleQty, M.DyeingMcCapacity, [Status],ExistingDBatchNo
                        FROM M
                        LEFT JOIN RecipeDefinitionMaster RDM ON RDM.RecipeID = M.RecipeID AND ISNULL(RDM.IsActive,0) = 1
                        INNER JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID = M.ColorID
                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue FR ON FR.ValueID = RDM.RecipeFor
	                    LEFT JOIN FreeConceptChildColor FCC ON FCC.CCColorID = M.CCColorID
	                    LEFT JOIN FreeConceptMaster FCM ON FCM.ConceptID = FCC.ConceptID

	                    UNION 

	                    SELECT M1.BatchID, M1.BatchNo, M1.BatchDate, DBatchID = ISNULL(M1.DBatchID,0), M1.RecipeID, FCM.ConceptID, M1.BookingID, M1.ColorID, M1.CCColorID, M1.BatchWeightKG, M1.BatchWeightPcs BatchQtyPcs, M1.Remarks,
                        M1.ExportOrderID, M1.BuyerID, M1.BuyerTeamID, RDM.RecipeNo, RDM.RecipeDate, RDM.RecipeFor, COL.SegmentValue ColorName, M1.BuyerName, M1.BuyerTeamName,
                        FR.ValueName RecipeForName, M1.TotalBatchUseQtyKG, FCM.GroupConceptNo ConceptNo, M1.SLNo, M1.MachineLoading, M1.DyeingNozzleQty, M1.DyeingMcCapacity, [Status],ExistingDBatchNo
                        FROM ReworkM M1
                        LEFT JOIN RecipeDefinitionMaster RDM ON RDM.RecipeID = M1.RecipeID AND ISNULL(RDM.IsActive,0) = 1
                        INNER JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID = M1.ColorID
                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue FR ON FR.ValueID = RDM.RecipeFor
	                    LEFT JOIN FreeConceptChildColor FCC ON FCC.CCColorID = M1.CCColorID
	                    LEFT JOIN FreeConceptMaster FCM ON FCM.ConceptID = FCC.ConceptID
                    ) 
                    Select *, Count(*) Over() TotalRows From FFF ";
                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? " Order By BatchID Desc " : paginationInfo.OrderBy;
            }
            else
            {
                sql += $@";WITH M AS (
                            SELECT DBM.*
                            FROM DyeingBatchMaster DBM
			                LEFT JOIN DyeingBatchMergeBatch DBB ON DBB.MergeDBatchID = DBM.DBatchID
			                WHERE DBB.DBMID IS NULL
                        ), FFF AS (
                            SELECT M.DBatchID, M.DBatchNo, M.DBatchDate, M.RecipeID, M.ColorID, M.CCColorID, M.BatchWeightKG, M.DMID, M.MachineLoading, M.DyeingNozzleQty,
                            M.ShiftID, M.OperatorID, M.BatchStatus, M.BatchStartTime, M.BatchEndTime, M.ProductionDate, M.Remarks, RDM.RecipeNo, RDM.RecipeDate, RDM.RecipeFor,
						    COL.SegmentValue ColorName, FR.ValueName RecipeForName, FC.GroupConceptNo ConceptNo, FC.GroupConceptNo SLNo,
                            FC.BuyerID, FC.BuyerTeamID, 
                            BuyerName = CASE WHEN FC.BuyerID = 0 THEN 'R&D' ELSE CTO.ShortName END, 
                            BuyerTeamName = CASE WHEN FC.BuyerTeamID = 0 THEN 'R&D' ELSE CCT.TeamName END,

                            (SELECT STUFF((
                            SELECT DISTINCT ', '+ BM.BatchNo
                            FROM DyeingBatchWithBatchMaster DB 
                            LEFT JOIN BatchMaster BM ON BM.BatchID = DB.BatchID
                            WHERE DB.DBatchID = M.DBatchID
                            FOR XML PATH('')),1,1,'')) BatchNo

                            FROM M
                            LEFT JOIN RecipeDefinitionMaster RDM ON RDM.RecipeID = M.RecipeID AND ISNULL(RDM.IsActive,0) = 1
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID = M.ColorID
						    LEFT JOIN {DbNames.EPYSL}..EntityTypeValue FR ON FR.ValueID = RDM.RecipeFor
						    LEFT JOIN FreeConceptChildColor FCC ON FCC.CCColorID = M.CCColorID
						    LEFT JOIN FreeConceptMaster FC ON FC.ConceptID = FCC.ConceptID
	                        LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = FC.BuyerID
	                        LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = FC.BuyerTeamID
                        ) 
                        Select *, Count(*) Over() TotalRows From FFF ";
            }

            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<DyeingBatchMaster>(sql);
        }
        public async Task<DyeingBatchMaster> GetNewAsync(int newId)
        {
            var sql = $@"
                        ;WITH M AS (
                            SELECT	Distinct BM.*, SLNo = Case When FCM.IsBDS = 0 Then FCM.GroupConceptNo Else FBA.SLNo End
                            FROM BatchMaster BM 
							Inner Join FreeConceptMaster FCM On FCM.GroupConceptNo = BM.GroupConceptNo
							LEFT JOIN FBookingAcknowledge FBA ON FBA.BookingID = FCM.BookingID
                            WHERE BM.BatchID = {newId}
                        )
                        SELECT M.BatchID, M.BatchNo, M.BatchDate, M.RecipeID, FCM.ConceptID, M.BookingID, M.ColorID, M.CCColorID, M.BatchWeightKG, M.BatchWeightPcs BatchQtyPcs, M.Remarks,
                        M.ExportOrderID, M.BuyerID, M.BuyerTeamID, RDM.RecipeNo, RDM.RecipeDate, RDM.RecipeFor, COL.SegmentValue ColorName,
						FR.ValueName RecipeForName, M.MachineLoading, M.DyeingNozzleQty, M.DyeingMcCapacity, M.DMID, DM.DyeingMcslNo DMNo, FCM.GroupConceptNo ConceptNo, M.SLNo
                        FROM M
                        LEFT JOIN RecipeDefinitionMaster RDM ON RDM.RecipeID = M.RecipeID
                        INNER JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID = M.ColorID
						LEFT JOIN {DbNames.EPYSL}..EntityTypeValue FR ON FR.ValueID = RDM.RecipeFor
						INNER JOIN DyeingMachine DM ON DM.DMID = M.DMID
                        LEFT JOIN FreeConceptChildColor FCC ON FCC.CCColorID = M.CCColorID
                        LEFT JOIN FreeConceptMaster FCM ON FCM.ConceptID=FCC.ConceptID;

                        -----for DyeingBatchItem
                        ;WITH M AS (
                            SELECT * FROM BatchItemRequirement BM WHERE BM.BatchID = {newId}
                        ),
                        ItemSegment As
                        (
	                        SELECT CAST(ISV.SegmentValueID As varchar) [id], ISV.SegmentValue [text], CAST(ISN.SegmentNameID As varchar) [desc]
	                        FROM {DbNames.EPYSL}..ItemSegmentName ISN
	                        INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISN.SegmentNameID = ISV.SegmentNameID
	                        WHERE ISNULL(ISV.SegmentValue, '') <> '' And SegmentValueID NOT IN(17725,5127,1616,1969,47221,2117,1616,1621,44382,44384,45969,46717,47220, 2155)
                        )
                        SELECT M.BItemReqID, M.BatchID, M.RecipeItemInfoID, M.ItemMasterID,M.ConceptID, M.Pcs QtyPcs, M.Qty, M.IsFloorRequistion, RDM.RecipeID, B.BookingID, CM.SubGroupID ItemSubGroupID,
                        B.BatchNo, B.BuyerID, B.BuyerTeamID, B.ExportOrderID, KnittingType.TypeName KnittingType,Composition.text FabricComposition, Construction.text FabricConstruction,
                        Technical.TechnicalName,Gsm.text FabricGsm, SG.SubGroupName ItemSubGroup
                        FROM M
                        LEFT JOIN RecipeDefinitionItemInfo RDM ON RDM.RecipeItemInfoID = M.RecipeItemInfoID
                        INNER JOIN BatchMaster B ON B.BatchID = M.BatchID
                        LEFT JOIN FreeConceptMaster CM ON CM.ConceptID=M.ConceptID
                        LEFT JOIN KnittingMachineType KnittingType ON KnittingType.TypeID=CM.KnittingTypeID
                        LEFT JOIN ItemSegment Composition ON Composition.id=CM.CompositionID
                        LEFT JOIN ItemSegment Construction ON Construction.id=CM.ConstructionID
                        LEFT JOIN ItemSegment Gsm ON Gsm.id=CM.GSMID
                        LEFT JOIN {DbNames.EPYSL}..ItemSubGroup SG ON SG.SubGroupID = CM.SubGroupID
                        LEFT JOIN FabricTechnicalName Technical ON Technical.TechnicalNameId=CM.TechnicalNameId;

                        ----for DyeingBatchItemRoll
                        ;SELECT C.BItemReqID, C.BatchID, C.GRollID, C.ItemMasterID, C.RollQty, C.RollQtyPcs, KP.RollNo
                        FROM BatchChild C
                        INNER JOIN KnittingProduction KP ON KP.GRollID = C.GRollID
                        WHERE C.BatchID = {newId};

                        --Childs
                        WITH M AS (
                            SELECT BC.*, RD.RecipeDInfoID, RD.Temperature FROM BatchWiseRecipeChild BC
							LEFT JOIN RecipeDefinitionChild RD ON RD.RecipeChildID = BC.RecipeChildID
							WHERE BC.BatchID = {newId}
                        )
                        SELECT DI.RecipeDInfoID, M.TempIn, M.BatchID, M.ProcessTime, EV.ValueName FiberPart, ISV.SegmentValue ColorName
					    FROM M
						LEFT JOIN RecipeDefinitionDyeingInfo DI ON DI.RecipeDInfoID = M.RecipeDInfoID
						INNER JOIN {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID = DI.FiberPartID
						INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = DI.ColorID
						GROUP BY DI.RecipeDInfoID, M.TempIn, M.BatchID, M.ProcessTime, EV.ValueName, ISV.SegmentValue;

                        --Def Childs
                        SELECT B.BRecipeChildID, B.BatchID, B.RecipeChildID, B.ProcessID, B.ParticularsID, B.RawItemID, B.Qty, B.UnitID, B.TempIn,
						B.TempOut, B.ProcessTime, C.RecipeID,I.ItemName, P.ValueName ProcessName,PL.ValueName ParticularsName,R.ItemName RawItemName,
						UU.DisplayUnitDesc Unit, C.IsPercentage, C.RecipeDInfoID, C.Temperature, EV.ValueName FiberPart, ISV.SegmentValue ColorName
						FROM BatchWiseRecipeChild B
						LEFT JOIN RecipeDefinitionChild C ON C.RecipeChildID = B.RecipeChildID
						LEFT JOIN {DbNames.EPYSL}..ItemMaster I ON I.ItemMasterID=C.RawItemID
						LEFT JOIN {DbNames.EPYSL}..EntityTypeValue P ON P.ValueID = C.ProcessID
						LEFT JOIN {DbNames.EPYSL}..EntityTypeValue PL ON PL.ValueID = C.ParticularsID
						LEFT JOIN {DbNames.EPYSL}..ItemMaster R ON R.ItemMasterID = C.RawItemID
						LEFT JOIN {DbNames.EPYSL}..Unit UU ON UU.UnitID = C.UnitID
						LEFT JOIN RecipeDefinitionDyeingInfo DI ON DI.RecipeDInfoID = C.RecipeDInfoID
						INNER JOIN {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID = DI.FiberPartID
						INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = DI.ColorID
						WHERE B.BatchID = {newId};
                    ";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                DyeingBatchMaster data = records.Read<DyeingBatchMaster>().FirstOrDefault();
                data.DyeingBatchItems = records.Read<DyeingBatchItem>().ToList();
                data.DyeingBatchItemRolls = records.Read<DyeingBatchItemRoll>().ToList();
                data.DyeingBatchRecipes = records.Read<DyeingBatchRecipe>().ToList();
                data.DefChilds = records.Read<RecipeDefinitionChild>().ToList();
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
        public async Task<List<DyeingBatchMaster>> GetNewMultiSelectAsync(string batchIDs)
        {
            var sql = $@"
                        ;WITH M AS (
                            SELECT	Distinct BM.*, SLNo = Case When FCM.IsBDS = 0 Then FCM.GroupConceptNo Else FBA.SLNo End
                            FROM BatchMaster BM 
							Inner Join FreeConceptMaster FCM On FCM.GroupConceptNo = BM.GroupConceptNo
							LEFT JOIN FBookingAcknowledge FBA ON FBA.BookingID = FCM.BookingID
                            WHERE BM.BatchID IN ({batchIDs})
                        )
                        SELECT M.BatchID, M.BatchNo, M.BatchDate, M.RecipeID, FCM.ConceptID, M.BookingID, M.ColorID, M.CCColorID, M.BatchWeightKG, M.BatchWeightPcs, M.Remarks,
                        M.ExportOrderID, M.BuyerID, M.BuyerTeamID, RDM.RecipeNo, RDM.RecipeDate, RDM.RecipeFor, COL.SegmentValue ColorName,
						FR.ValueName RecipeForName, M.MachineLoading, M.DyeingNozzleQty, M.DyeingMcCapacity, M.DMID, DM.DyeingMcslNo DMNo, FCM.GroupConceptNo ConceptNo, M.SLNo
                        FROM M
                        LEFT JOIN RecipeDefinitionMaster RDM ON RDM.RecipeID = M.RecipeID
                        INNER JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID = M.ColorID
						LEFT JOIN {DbNames.EPYSL}..EntityTypeValue FR ON FR.ValueID = RDM.RecipeFor
						INNER JOIN DyeingMachine DM ON DM.DMID = M.DMID
                        LEFT JOIN FreeConceptChildColor FCC ON FCC.CCColorID = M.CCColorID
                        LEFT JOIN FreeConceptMaster FCM ON FCM.ConceptID=FCC.ConceptID;

                        -----for DyeingBatchItem
                        ;WITH M AS (
                            SELECT * FROM BatchItemRequirement BM WHERE BM.BatchID IN ({batchIDs})
                        ),
                        ItemSegment As
                        (
	                        SELECT CAST(ISV.SegmentValueID As varchar) [id], ISV.SegmentValue [text], CAST(ISN.SegmentNameID As varchar) [desc]
	                        FROM {DbNames.EPYSL}..ItemSegmentName ISN
	                        INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISN.SegmentNameID = ISV.SegmentNameID
	                        WHERE ISNULL(ISV.SegmentValue, '') <> '' And SegmentValueID NOT IN(17725,5127,1616,1969,47221,2117,1616,1621,44382,44384,45969,46717,47220, 2155)
                        )
                        SELECT M.BItemReqID, M.BatchID, M.RecipeItemInfoID, M.ItemMasterID,M.ConceptID, M.Pcs QtyPcs, M.Qty, M.IsFloorRequistion, RDM.RecipeID, B.BookingID, CM.SubGroupID ItemSubGroupID,
                        B.BatchNo, B.BuyerID, B.BuyerTeamID, B.ExportOrderID, KnittingType.TypeName KnittingType,Composition.text FabricComposition, Construction.text FabricConstruction,
                        Technical.TechnicalName,Gsm.text FabricGsm, SG.SubGroupName ItemSubGroup
                        FROM M
                        LEFT JOIN RecipeDefinitionItemInfo RDM ON RDM.RecipeItemInfoID = M.RecipeItemInfoID
                        INNER JOIN BatchMaster B ON B.BatchID = M.BatchID
                        LEFT JOIN FreeConceptMaster CM ON CM.ConceptID=M.ConceptID
                        LEFT JOIN KnittingMachineType KnittingType ON KnittingType.TypeID=CM.KnittingTypeID
                        LEFT JOIN ItemSegment Composition ON Composition.id=CM.CompositionID
                        LEFT JOIN ItemSegment Construction ON Construction.id=CM.ConstructionID
                        LEFT JOIN ItemSegment Gsm ON Gsm.id=CM.GSMID
                        LEFT JOIN {DbNames.EPYSL}..ItemSubGroup SG ON SG.SubGroupID = CM.SubGroupID
                        LEFT JOIN FabricTechnicalName Technical ON Technical.TechnicalNameId=CM.TechnicalNameId;

                        ----for DyeingBatchItemRoll
                        /*;SELECT C.BItemReqID, C.BatchID, C.GRollID, C.ItemMasterID, C.RollQty, C.RollQtyPcs, KP.RollNo
                        FROM BatchChild C
                        INNER JOIN KnittingProduction KP ON KP.GRollID = C.GRollID
                        WHERE C.BatchID IN ({batchIDs});*/

                        ;WITH
            PendingRolls AS
            (
			SELECT FCM.ConceptID,RollID = KP.GRollID, FCM.GroupConceptNo, CR.SFDChildRollID,
			C.BItemReqID,C.BatchID,C.ItemMasterID
	            FROM KnittingProduction KP 
	            INNER JOIN FreeConceptMaster FCM ON FCM.ConceptID = KP.ConceptID
	            LEFT JOIN SFDChildRoll CR ON CR.RollID = KP.GRollID
				LEFT JOIN BatchChild C ON C.GRollID = KP.GRollID
	            WHERE --ISNULL(kP.InActive,0) = 0 AND 
	            CR.SFDChildRollID IS NULL
	            AND  C.BatchID IN ({batchIDs})
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
	            FCM.TechnicalNameId,kP.InActive,PR.BItemReqID,PR.BatchID
	            FROM KnittingProduction KP
	            INNER JOIN FreeConceptMaster FCM ON FCM.ConceptID = KP.ConceptID
	            INNER JOIN PendingRolls PR ON PR.RollID = KP.GRollID
				LEFT JOIN CC ON CC.ConceptID = PR.ConceptID
	            --WHERE ISNULL(kP.InActive,0) = 1 
	            GROUP BY KP.GRollID, CC.CCColorID, 
	            FCM.ConceptID, FCM.GroupConceptNo, FCM.SubGroupID, FCM.BookingChildID, 
	            FCM.BookingID, FCM.ConsumptionID, KP.RollNo, KP.RollQty, KP.RollQtyPcs,
	            FCM.ItemMasterID, KP.GRollID, KP.ParentGRollID,
	            KP.GRollID,  CC.ColorName,
	            FCM.TechnicalNameId,kP.InActive,PR.BItemReqID,PR.BatchID
            ),
            ActiveChildRolls As
            (
                Select SFDChildRollID = KP.GRollID, ARN.SFDChildID, 
                KP.ConceptID, ARN.GroupConceptNo, ARN.SubGroupID, ARN.BookingChildID, 
                ARN.BookingID, ARN.ConsumptionID, KP.RollNo, KP.RollQty, KP.RollQtyPcs,
                ARN.ItemMasterID, KP.GRollID, KP.ParentGRollID,
                RollID=KP.GRollID, ARN.CCColorID, ARN.ColorName, --ARN.BChildID, 
                ARN.TechnicalNameId,kP.InActive,ARN.BItemReqID,ARN.BatchID
                From KnittingProduction KP
                Inner Join AllRolls ARN  On ARN.ConceptID=KP.ConceptID AND ARN.GRollID=KP.GRollID
                Where  KP.InActive=0
                GROUP BY KP.GRollID, ARN.SFDChildID, 
                KP.ConceptID, ARN.GroupConceptNo, ARN.SubGroupID, ARN.BookingChildID, 
                ARN.BookingID, ARN.ConsumptionID, KP.RollNo, KP.RollQty, KP.RollQtyPcs,
                ARN.ItemMasterID, KP.GRollID, KP.ParentGRollID,
                KP.GRollID, ARN.CCColorID, ARN.ColorName,
                ARN.TechnicalNameId,kP.InActive,ARN.BItemReqID,ARN.BatchID
            ) 
			Select * From ActiveChildRolls ACR
		    --Where ACR.RollID Not IN(select BC.GRollID from BatchChild BC)
              Where ACR.RollID Not IN(select GRollID from DyeingBatchItemRoll)
            ;	


                        --Childs
                        WITH M AS (
                            SELECT BC.*, RD.RecipeDInfoID, RD.Temperature FROM BatchWiseRecipeChild BC
							LEFT JOIN RecipeDefinitionChild RD ON RD.RecipeChildID = BC.RecipeChildID
							WHERE BC.BatchID IN ({batchIDs})
                        )
                        SELECT DI.RecipeDInfoID, M.TempIn, M.BatchID, M.ProcessTime, EV.ValueName FiberPart, ISV.SegmentValue ColorName
					    FROM M
						LEFT JOIN RecipeDefinitionDyeingInfo DI ON DI.RecipeDInfoID = M.RecipeDInfoID
						INNER JOIN {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID = DI.FiberPartID
						INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = DI.ColorID
						GROUP BY DI.RecipeDInfoID, M.TempIn, M.BatchID, M.ProcessTime, EV.ValueName, ISV.SegmentValue;

                        --Def Childs
                        SELECT B.BRecipeChildID, B.BatchID, B.RecipeChildID, B.ProcessID, B.ParticularsID, B.RawItemID, B.Qty, B.UnitID, B.TempIn,
						B.TempOut, B.ProcessTime, C.RecipeID,I.ItemName, P.ValueName ProcessName,PL.ValueName ParticularsName,R.ItemName RawItemName,
						UU.DisplayUnitDesc Unit, C.IsPercentage, C.RecipeDInfoID, C.Temperature, EV.ValueName FiberPart, ISV.SegmentValue ColorName
						FROM BatchWiseRecipeChild B
						LEFT JOIN RecipeDefinitionChild C ON C.RecipeChildID = B.RecipeChildID
						LEFT JOIN {DbNames.EPYSL}..ItemMaster I ON I.ItemMasterID=C.RawItemID
						LEFT JOIN {DbNames.EPYSL}..EntityTypeValue P ON P.ValueID = C.ProcessID
						LEFT JOIN {DbNames.EPYSL}..EntityTypeValue PL ON PL.ValueID = C.ParticularsID
						LEFT JOIN {DbNames.EPYSL}..ItemMaster R ON R.ItemMasterID = C.RawItemID
						LEFT JOIN {DbNames.EPYSL}..Unit UU ON UU.UnitID = C.UnitID
						LEFT JOIN RecipeDefinitionDyeingInfo DI ON DI.RecipeDInfoID = C.RecipeDInfoID
						INNER JOIN {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID = DI.FiberPartID
						INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = DI.ColorID
						WHERE B.BatchID IN ({batchIDs});
                    ";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                List<DyeingBatchMaster> data = records.Read<DyeingBatchMaster>().ToList();
                var dyeingBatchItems = records.Read<DyeingBatchItem>().ToList();
                var dyeingBatchItemRolls = records.Read<DyeingBatchItemRoll>().ToList();
                var dyeingBatchRecipes = records.Read<DyeingBatchRecipe>().ToList();
                var defChilds = records.Read<RecipeDefinitionChild>().ToList();
                data.ForEach(m =>
                {
                    m.DyeingBatchItems = dyeingBatchItems.Where(x => x.BatchID == m.BatchID).ToList();
                    m.DyeingBatchItemRolls = dyeingBatchItemRolls.Where(x => x.BatchID == m.BatchID).ToList();
                    m.DyeingBatchItems.ForEach(mr =>
                    {
                        mr.DyeingBatchItemRolls = m.DyeingBatchItemRolls.Where(x => x.BItemReqID == mr.BItemReqID).ToList();
                        //if (mr.DyeingBatchItemRolls.Count == 0)
                        //{
                        //    m.DyeingBatchItems = new List<DyeingBatchItem>();
                        //}
                    });
                    m.DyeingBatchRecipes = dyeingBatchRecipes.Where(x => x.DBatchID == m.DBatchID).ToList();
                    m.DefChilds = defChilds.Where(x => x.DBatchID == m.DBatchID).ToList();
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
        public async Task<DyeingBatchMaster> GetAsync(int id)
        {
            var sql = $@"
                    ;WITH M AS (
                        SELECT	*
                        FROM DyeingBatchMaster DB WHERE DBatchID = {id}
                    )
                    SELECT M.DBatchID, M.DBatchNo, M.DBatchDate, M.RecipeID, M.ColorID, M.CCColorID, M.BatchWeightKG, M.BatchQtyPcs, M.DMID, M.MachineLoading, M.DyeingNozzleQty,
                    M.ShiftID, M.OperatorID, M.BatchStatus, M.PlanBatchStartTime, M.PlanBatchEndTime, M.ProductionDate, M.Remarks, RDM.RecipeNo, RDM.RecipeDate, RDM.RecipeFor,
					COL.SegmentValue ColorName, FR.ValueName RecipeForName, M.DMID, M.MachineLoading, M.DyeingNozzleQty, M.DyeingMcCapacity, DM.DyeingMcslNo DMNo, FC.GroupConceptNo ConceptNo, FC.GroupConceptNo SLNo
                    FROM M
                    LEFT JOIN RecipeDefinitionMaster RDM ON RDM.RecipeID = M.RecipeID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID = M.ColorID
					LEFT JOIN {DbNames.EPYSL}..EntityTypeValue FR ON FR.ValueID = RDM.RecipeFor
					LEFT JOIN DyeingMachine DM ON DM.DMID = M.DMID
					LEFT JOIN FreeConceptChildColor FCC ON FCC.CCColorID = M.CCColorID
					LEFT JOIN FreeConceptMaster FC ON FC.ConceptID = FCC.ConceptID;

                    -----DyeingBatchWithBatchMaster
                    ;WITH M AS (
                        SELECT * FROM DyeingBatchWithBatchMaster BM WHERE BM.DBatchID = {id}
                    )
		            SELECT M.DBBMID, M.DBatchID, M.BatchID, M.BatchUseQtyKG, M.BatchUseQtyPcs, B.BatchNo
		            FROM M
		            INNER JOIN BatchMaster B ON B.BatchID = M.BatchID;

                    ----for DyeingBatchItem
                    ;WITH M AS (
                        SELECT * FROM DyeingBatchItem BM WHERE BM.DBatchID = {id}
                    ),
                    ItemSegment As
                    (
	                    SELECT CAST(ISV.SegmentValueID As varchar) [id], ISV.SegmentValue [text], CAST(ISN.SegmentNameID As varchar) [desc]
	                    FROM {DbNames.EPYSL}..ItemSegmentName ISN
	                    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISN.SegmentNameID = ISV.SegmentNameID
	                    WHERE ISNULL(ISV.SegmentValue, '') <> '' And SegmentValueID NOT IN(17725,5127,1616,1969,47221,2117,1616,1621,44382,44384,45969,46717,47220, 2155)
                    )
                    SELECT M.DBIID, M.DBatchID, M.BatchID, M.ItemSubGroupID, M.ItemMasterID, M.RecipeID, M.ConceptID, M.BookingID, M.ExportOrderID,
		            M.BuyerID, M.BuyerTeamID, M.QtyPcs, M.Qty, M.IsFloorRequistion,
                    B.BatchNo,  KnittingType.TypeName KnittingType,Composition.text FabricComposition, Construction.text FabricConstruction,
                    Technical.TechnicalName,Gsm.text FabricGsm, SG.SubGroupName ItemSubGroup
                    FROM M
                    INNER JOIN BatchMaster B ON B.BatchID = M.BatchID
                    LEFT JOIN FreeConceptMaster CM ON CM.ConceptID = M.ConceptID
                    LEFT JOIN KnittingMachineType KnittingType ON KnittingType.TypeID=CM.KnittingTypeID
                    LEFT JOIN ItemSegment Composition ON Composition.id=CM.CompositionID
                    LEFT JOIN ItemSegment Construction ON Construction.id=CM.ConstructionID
                    LEFT JOIN ItemSegment Gsm ON Gsm.id=CM.GSMID
                    LEFT JOIN {DbNames.EPYSL}..ItemSubGroup SG ON SG.SubGroupID = CM.SubGroupID
                    LEFT JOIN FabricTechnicalName Technical ON Technical.TechnicalNameId=CM.TechnicalNameId;

                    ----for DyeingBatchItemRoll
                    ;SELECT C.DBIRollID, C.DBIID, C.DBatchID, C.GRollID, C.ItemMasterID, C.RollQty, C.RollQtyPcs, KP.RollNo
                    FROM DyeingBatchItemRoll C
                    INNER JOIN KnittingProduction KP ON KP.GRollID = C.GRollID
                    WHERE C.DBatchID = {id};

                    ----for DyeingBatchRecipe
                    ;WITH M AS (
                        SELECT	BM.*,RD.RecipeDInfoID
						FROM DyeingBatchRecipe BM
						LEFT JOIN RecipeDefinitionChild RD ON RD.RecipeChildID = BM.RecipeChildID
						WHERE BM.DBatchID = {id}
                    )
                    SELECT M.DBatchID, M.RecipeID, M.TempIn, M.ProcessTime,
					DI.RecipeDInfoID, EV.ValueName FiberPart,ISV.SegmentValue ColorName
					FROM M
					LEFT JOIN RecipeDefinitionDyeingInfo DI ON DI.RecipeDInfoID = M.RecipeDInfoID
					INNER JOIN {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID = DI.FiberPartID
					INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = DI.ColorID
					GROUP BY M.DBatchID, M.RecipeID, M.TempIn, M.ProcessTime,
					DI.RecipeDInfoID,EV.ValueName,ISV.SegmentValue;

                    SELECT B.DBRID, B.DBatchID, B.RecipeChildID, B.DBatchID, B.RecipeChildID, B.ProcessID, B.ParticularsID, B.RawItemID, B.Qty, B.UnitID, B.TempIn,
					B.TempOut, B.ProcessTime, C.RecipeID,I.ItemName, P.ValueName ProcessName,PL.ValueName ParticularsName,R.ItemName RawItemName,
					UU.DisplayUnitDesc Unit, C.IsPercentage, C.RecipeDInfoID, C.Temperature, EV.ValueName FiberPart, ISV.SegmentValue ColorName
					FROM DyeingBatchRecipe B
					LEFT JOIN RecipeDefinitionChild C ON C.RecipeChildID = B.RecipeChildID
					LEFT JOIN {DbNames.EPYSL}..ItemMaster I ON I.ItemMasterID=C.RawItemID
					LEFT JOIN {DbNames.EPYSL}..EntityTypeValue P ON P.ValueID = C.ProcessID
					LEFT JOIN {DbNames.EPYSL}..EntityTypeValue PL ON PL.ValueID = C.ParticularsID
					LEFT JOIN {DbNames.EPYSL}..ItemMaster R ON R.ItemMasterID = C.RawItemID
					LEFT JOIN {DbNames.EPYSL}..Unit UU ON UU.UnitID = C.UnitID
					LEFT JOIN RecipeDefinitionDyeingInfo DI ON DI.RecipeDInfoID = C.RecipeDInfoID
					INNER JOIN {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID = DI.FiberPartID
					INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = DI.ColorID
					WHERE B.DBatchID = {id};

                    ";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                DyeingBatchMaster data = records.Read<DyeingBatchMaster>().FirstOrDefault();
                data.DyeingBatchWithBatchMasters = records.Read<DyeingBatchWithBatchMaster>().ToList();
                data.DyeingBatchItems = records.Read<DyeingBatchItem>().ToList();
                data.DyeingBatchItemRolls = records.Read<DyeingBatchItemRoll>().ToList();
                data.DyeingBatchRecipes = records.Read<DyeingBatchRecipe>().ToList();
                data.DefChilds = records.Read<RecipeDefinitionChild>().ToList();
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
        public async Task<List<DyeingBatchMaster>> GetBatchListAsync(string batchIds)
        {
            var sql = $@"WITH M AS (
                        SELECT	*
                        FROM BatchMaster BM WHERE BM.BatchID NOT IN (SELECT BatchID FROM DyeingBatchWithBatchMaster)
                        AND BM.BatchID NOT IN ({batchIds}) --BM.IsApproved = 1 AND
                    )
                    SELECT M.BatchID, M.BatchNo, M.BatchDate, M.RecipeID, M.GroupConceptNo, M.BookingID, M.ColorID, M.BatchWeightKG, M.BatchWeightPcs BatchQtyPcs, M.Remarks,
                    M.ExportOrderID, M.BuyerID, M.BuyerTeamID, RDM.RecipeNo, RDM.RecipeDate, RDM.RecipeFor, COL.SegmentValue ColorName,
					FR.ValueName RecipeForName, Count(*) Over() TotalRows
                    FROM M
                    LEFT JOIN RecipeDefinitionMaster RDM ON RDM.RecipeID = M.RecipeID
                    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID = M.ColorID
					LEFT JOIN {DbNames.EPYSL}..EntityTypeValue FR ON FR.ValueID = RDM.RecipeFor";

            return await _service.GetDataAsync<DyeingBatchMaster>(sql);
        }
        public async Task<List<DyeingBatchMaster>> GetBatchDetails(string batchIds)
        {
            var sql = $@"
                    ;WITH M AS (
                        SELECT	*
                        FROM BatchMaster BM WHERE BM.BatchID IN ({batchIds})
                    )
                    SELECT M.BatchID, M.BatchNo, M.BatchDate, M.RecipeID, M.ConceptID, M.BookingID, M.ColorID, M.CCColorID, M.BatchWeightKG, M.BatchWeightPcs BatchQtyPcs, M.Remarks,
                    M.ExportOrderID, M.BuyerID, M.BuyerTeamID, RDM.RecipeNo, RDM.RecipeDate, RDM.RecipeFor, COL.SegmentValue ColorName,
					FR.ValueName RecipeForName, Count(*) Over() TotalRows
                    FROM M
                    LEFT JOIN RecipeDefinitionMaster RDM ON RDM.RecipeID = M.RecipeID
                    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID = M.ColorID
					LEFT JOIN {DbNames.EPYSL}..EntityTypeValue FR ON FR.ValueID = RDM.RecipeFor;

                    -----for DyeingBatchItem
                    ;WITH M AS (
                        SELECT * FROM BatchItemRequirement BM WHERE BM.BatchID IN ({batchIds})
                    ),
                    ItemSegment As
                    (
	                    SELECT CAST(ISV.SegmentValueID As varchar) [id], ISV.SegmentValue [text], CAST(ISN.SegmentNameID As varchar) [desc]
	                    FROM {DbNames.EPYSL}..ItemSegmentName ISN
	                    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISN.SegmentNameID = ISV.SegmentNameID
	                    WHERE ISNULL(ISV.SegmentValue, '') <> '' And SegmentValueID NOT IN(17725,5127,1616,1969,47221,2117,1616,1621,44382,44384,45969,46717,47220, 2155)
                    )
                    SELECT M.BItemReqID, M.BatchID, M.RecipeItemInfoID, M.ItemMasterID, M.ConceptID, M.QtyPcs, M.Qty, M.IsFloorRequistion, RDM.RecipeID, RDM.ConceptID, RDM.BookingID, RDM.SubGroupID ItemSubGroupID,
                    B.BatchNo, B.BuyerID, B.BuyerTeamID, B.ExportOrderID, KnittingType.TypeName KnittingType,Composition.text FabricComposition, Construction.text FabricConstruction,
                    Technical.TechnicalName,Gsm.text FabricGsm, SG.SubGroupName ItemSubGroup
                    FROM M
                    LEFT JOIN RecipeDefinitionItemInfo RDM ON RDM.RecipeItemInfoID = M.RecipeItemInfoID
                    INNER JOIN BatchMaster B ON B.BatchID = M.BatchID
                    LEFT JOIN FreeConceptMaster CM ON CM.ConceptID = B.ConceptID
                    LEFT JOIN KnittingMachineType KnittingType ON KnittingType.TypeID=CM.KnittingTypeID
                    LEFT JOIN ItemSegment Composition ON Composition.id=CM.CompositionID
                    LEFT JOIN ItemSegment Construction ON Construction.id=CM.ConstructionID
                    LEFT JOIN ItemSegment Gsm ON Gsm.id=CM.GSMID
                    LEFT JOIN {DbNames.EPYSL}..ItemSubGroup SG ON SG.SubGroupID = CM.SubGroupID
                    LEFT JOIN FabricTechnicalName Technical ON Technical.TechnicalNameId=CM.TechnicalNameId;

                    ----for DyeingBatchItemRoll
                    ;SELECT C.BItemReqID, C.BatchID, C.GRollID, C.ItemMasterID, C.RollQty, C.RollQtyPcs, KP.RollNo
                    FROM BatchChild C
                    INNER JOIN KnittingProduction KP ON KP.GRollID = C.GRollID
                    WHERE C.BatchID IN ({batchIds});

                    ----for DyeingBatchRecipe
                    ;WITH M AS (
                        SELECT	* FROM BatchWiseRecipeChild BM WHERE BM.BatchID IN ({batchIds})
                    )
                    SELECT M.RecipeChildID, M.ProcessID, M.ParticularsID, M.RawItemID, M.Qty, M.UnitID, M.TempIn, M.TempOut, M.ProcessTime, RDM.RecipeID,
					P.ValueName ProcessName,PL.ValueName ParticularsName,R.ItemName RawItemName, UU.DisplayUnitDesc Unit
                    FROM M
                    LEFT JOIN RecipeDefinitionChild RDM ON RDM.RecipeChildID = M.RecipeChildID
					LEFT JOIN {DbNames.EPYSL}..EntityTypeValue P ON P.ValueID = M.ProcessID
					LEFT JOIN {DbNames.EPYSL}..EntityTypeValue PL ON PL.ValueID = M.ParticularsID
					LEFT JOIN {DbNames.EPYSL}..ItemMaster R ON R.ItemMasterID = M.RawItemID
					LEFT JOIN {DbNames.EPYSL}..Unit UU ON UU.UnitID = M.UnitID;

                    ";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                List<DyeingBatchMaster> data = records.Read<DyeingBatchMaster>().ToList();
                List<DyeingBatchItem> dyeingBatchItems = records.Read<DyeingBatchItem>().ToList();
                List<DyeingBatchItemRoll> dyeingBatchItemRolls = records.Read<DyeingBatchItemRoll>().ToList();
                List<DyeingBatchRecipe> dyeingBatchRecipes = records.Read<DyeingBatchRecipe>().ToList();

                foreach (DyeingBatchMaster batch in data)
                {
                    batch.DyeingBatchItems = dyeingBatchItems.Where(x => x.BatchID == batch.BatchID).ToList();
                    foreach (DyeingBatchItem item in batch.DyeingBatchItems)
                    {
                        item.DyeingBatchItemRolls = dyeingBatchItemRolls.Where(x => x.BItemReqID == item.BItemReqID).ToList();
                    }

                    batch.DyeingBatchRecipes = dyeingBatchRecipes.Where(x => x.RecipeID == batch.RecipeID).ToList();
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
        public async Task<List<DyeingBatchChildFinishingProcess>> GetFinishingProcessAsync(int conceptId, int colorID)
        {
            var sql = $@"WITH FC AS (
	                SELECT FCM.ConceptID
	                FROM FreeConceptMaster FC
	                INNER JOIN FreeConceptMaster FCM ON FCM.GroupConceptNo = FC.GroupConceptNo
	                WHERE FC.ConceptID = {conceptId}
                    ),
                    FPC AS (
                    SELECT FPC.*, FP.ConceptID
                    FROM FinishingProcessChild FPC
                    INNER JOIN FinishingProcessMaster FP ON FP.FPMasterID = FPC.FPMasterID
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
                    INNER JOIN FinishingMachineProcess_HK FMP On FMP.FMProcessID = FPC.ProcessID
                    INNER JOIN FinishingMachineConfigurationMaster FMC ON FMC.FMCMasterID = FMP.FMCMasterID
                    LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ET ON ET.ValueID = FPC.ProcessTypeID
                    LEFT JOIN FinishingMachineSetup FMS On FMS.FMSID = FPC.FMSID
                    Left Join {DbNames.EPYSL}..EntityTypeValue b on b.ValueID = FMS.BrandID
                    Left Join KnittingUnit c on c.KnittingUnitID = FMS.UnitID
                    GROUP BY FPC.FPChildID, FPC.ConceptID, FPC.FPMasterID, FPC.ProcessID, FPC.SeqNo, FMP.ProcessName, C.ShortName,b.ValueName, FPC.ProcessTypeID, ET.ValueName,
                    FPC.IsPreProcess, FPC.FMSID,FMC.FMCMasterID, FMS.MachineNo, FMC.ProcessName, FPC.Remarks, FPC.Param1Value, FPC.Param2Value, FPC.Param3Value,
                    FPC.Param4Value, FPC.Param5Value, FPC.Param6Value, FPC.Param7Value, FPC.Param8Value, FPC.Param9Value, FPC.Param10Value, FPC.Param11Value,
                    FPC.Param12Value, FPC.Param13Value, FPC.Param14Value, FPC.Param15Value, FPC.Param16Value, FPC.Param17Value, FPC.Param18Value,
                    FPC.Param19Value, FPC.Param20Value, FMS.UnitID, FMS.BrandID
                    ORDER BY FPC.SeqNo ASC";

            return await _service.GetDataAsync<DyeingBatchChildFinishingProcess>(sql);
        }
        public async Task<List<DyeingBatchChildFinishingProcess>> GetFinishingProcessByDyeingBatchAsync(int dBatchID, int colorID)
        {
            var sql = $@";With A As(
							Select DBM.DBatchID, DBI.ConceptID, DBM.ColorID
							From DyeingBatchItem DBI
							Inner Join DyeingBatchMaster DBM ON DBM.DBatchID = DBI.DBatchID
							WHERE DBM.DBatchID = {dBatchID} And DBM.colorID = {colorID}
							Group By DBM.DBatchID, DBI.ConceptID, DBM.ColorID
						),FPC AS (
                    SELECT FPC.*, FP.ConceptID
                    FROM FinishingProcessChild FPC
                    INNER JOIN FinishingProcessMaster FP ON FP.FPMasterID = FPC.FPMasterID
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
                    INNER JOIN FinishingMachineProcess_HK FMP On FMP.FMProcessID = FPC.ProcessID
                    INNER JOIN FinishingMachineConfigurationMaster FMC ON FMC.FMCMasterID = FMP.FMCMasterID
                    LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ET ON ET.ValueID = FPC.ProcessTypeID
                    LEFT JOIN FinishingMachineSetup FMS On FMS.FMSID = FPC.FMSID
                    Left Join {DbNames.EPYSL}..EntityTypeValue b on b.ValueID = FMS.BrandID
                    Left Join KnittingUnit c on c.KnittingUnitID = FMS.UnitID
                    GROUP BY FPC.FPChildID, FPC.ConceptID, FPC.FPMasterID, FPC.ProcessID, FPC.SeqNo, FMP.ProcessName, C.ShortName,b.ValueName, FPC.ProcessTypeID, ET.ValueName,
                    FPC.IsPreProcess, FPC.FMSID,FMC.FMCMasterID, FMS.MachineNo, FMC.ProcessName, FPC.Remarks, FPC.Param1Value, FPC.Param2Value, FPC.Param3Value,
                    FPC.Param4Value, FPC.Param5Value, FPC.Param6Value, FPC.Param7Value, FPC.Param8Value, FPC.Param9Value, FPC.Param10Value, FPC.Param11Value,
                    FPC.Param12Value, FPC.Param13Value, FPC.Param14Value, FPC.Param15Value, FPC.Param16Value, FPC.Param17Value, FPC.Param18Value,
                    FPC.Param19Value, FPC.Param20Value, FMS.UnitID, FMS.BrandID
                    ORDER BY FPC.SeqNo ASC";

            return await _service.GetDataAsync<DyeingBatchChildFinishingProcess>(sql);
        }
        public async Task<List<DyeingBatchMaster>> GetDyeingBatchs(PaginationInfo paginationInfo, string colorName, string conceptNo)
        {
            if (colorName.IsNullOrEmpty() || conceptNo.IsNullOrEmpty()) return new List<DyeingBatchMaster>();

            paginationInfo.OrderBy = string.IsNullOrEmpty(paginationInfo.OrderBy) ? "ORDER BY ConceptNo, ColorName, DBatchNo" : paginationInfo.OrderBy;
            var sql = $@"
                SELECT DB.DBatchID, DB.DBatchNo, DB.DBatchDate, DB.CCColorID, DB.ColorID, COL.SegmentValue ColorName,FCM.GroupConceptNo [ConceptNo]
                FROM DyeingBatchMaster DB 
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID = DB.ColorID
                LEFT JOIN FreeConceptChildColor FCC ON FCC.CCColorID = DB.CCColorID
                LEFT JOIN FreeConceptMaster FCM ON FCM.ConceptID = FCC.ConceptID
                WHERE DB.RecipeID = 0 AND FCM.GroupConceptNo LIKE '%{conceptNo}%' AND FCC.ColorName LIKE '%{colorName}%'
                {paginationInfo.FilterBy}
                {paginationInfo.OrderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<DyeingBatchMaster>(sql);
        }
        public async Task SaveAsync(DyeingBatchMaster entity)
        {
            SqlTransaction transaction = null;
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                switch (entity.EntityState)
                {
                    case EntityState.Added:
                        entity = await AddAsync(entity);
                        break;

                    case EntityState.Modified:
                        entity = await UpdateAsync(entity);
                        break;

                    default:
                        break;
                }

                await _service.SaveSingleAsync(entity, transaction);
                await _service.SaveAsync(entity.DyeingBatchWithBatchMasters, transaction);
                await _service.SaveAsync(entity.DyeingBatchRecipes, transaction);
                await _service.SaveAsync(entity.DyeingBatchItems, transaction);
                List<DyeingBatchItemRoll> rollList = new List<DyeingBatchItemRoll>();
                entity.DyeingBatchItems.ForEach(x => rollList.AddRange(x.DyeingBatchItemRolls));
                await _service.SaveAsync(rollList, transaction);
                await _service.SaveAsync(entity.DyeingBatchChildFinishingProcesses, transaction);
                await _service.SaveAsync(entity.DyeingBatchMergeBatchs, transaction);
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
        public async Task SaveAsyncRecipeCopy(DyeingBatchMaster entity)
        {
            SqlTransaction transaction = null;
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                switch (entity.EntityState)
                {
                    case EntityState.Added:
                        entity = await AddAsync(entity);
                        break;

                    case EntityState.Modified:
                        entity = await UpdateAsync(entity);
                        break;

                    default:
                        break;
                }
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
        private async Task<DyeingBatchMaster> AddAsync(DyeingBatchMaster entity)
        {
            entity.DBatchID = await _service.GetMaxIdAsync(TableNames.DYEING_BATCH_MASTER);

            //entity.DBatchNo = await _signatureRepository.GetMaxNoAsync(TableNames.DYEING_BATCH_NO);

            int paddingValue = 3;

            int maxNo = await _service.GetMaxNoAsync(TableNames.DYEING_BATCH_MASTER, "DBatchNo", entity.SLNo, entity.SLNo.Length + paddingValue, _connectionGmt);
            //entity.DBatchNo = entity.SLNo + maxNo.ToString().PadLeft(paddingValue, '0');

            var maxChildId = await _service.GetMaxIdAsync(TableNames.DYEING_BATCH_WITH_BATCH_MASTER, entity.DyeingBatchWithBatchMasters.Count);
            var maxItemId = await _service.GetMaxIdAsync(TableNames.DYEING_BATCH_ITEM, entity.DyeingBatchItems.Count);
            var maxItemRollId = await _service.GetMaxIdAsync(TableNames.DYEING_BATCH_ITEM_ROLL, entity.DyeingBatchItems.Sum(x => x.DyeingBatchItemRolls.Count()));
            var maxRecipeId = await _service.GetMaxIdAsync(TableNames.DYEING_BATCH_RECIPE, entity.DyeingBatchRecipes.Count);
            var maxProcessId = await _service.GetMaxIdAsync(TableNames.DYEING_BATCH_CHILD_FINISHING_PROCESS, entity.DyeingBatchChildFinishingProcesses.Where(x => x.EntityState == EntityState.Added).Count());
            var maxMergeBatchId = await _service.GetMaxIdAsync(TableNames.DYEING_BATCH_MERGE_BATCH, entity.DyeingBatchMergeBatchs.Count());

            foreach (DyeingBatchWithBatchMaster item in entity.DyeingBatchWithBatchMasters)
            {
                item.DBBMID = maxChildId++;
                item.DBatchID = entity.DBatchID;
                item.EntityState = EntityState.Added;
            }
            if (entity.IsRework)
            {
                foreach (DyeingBatchMergeBatch item in entity.DyeingBatchMergeBatchs)
                {
                    item.DBMID = maxMergeBatchId++;
                    item.DBatchID = entity.DBatchID;
                    item.EntityState = EntityState.Added;
                }
            }
            foreach (DyeingBatchItem item in entity.DyeingBatchItems)
            {
                List<DyeingBatchChildFinishingProcess> lstDyeingBatchChildFinishingProcess = entity.DyeingBatchChildFinishingProcesses.ToList().Where(x => x.DBIID == item.DBIID).ToList();
                foreach (DyeingBatchChildFinishingProcess child in lstDyeingBatchChildFinishingProcess)
                {
                    switch (child.EntityState)
                    {
                        case EntityState.Added:
                            child.DBIID = maxItemId;
                            break;

                        default:
                            break;
                    }
                }

                item.DBIID = maxItemId++;
                item.DBatchID = entity.DBatchID;
                item.EntityState = EntityState.Added;
                foreach (DyeingBatchItemRoll child in item.DyeingBatchItemRolls)
                {
                    child.DBIRollID = maxItemRollId++;
                    child.DBIID = item.DBIID;
                    child.DBatchID = entity.DBatchID;
                    child.EntityState = EntityState.Added;
                }
            }
            foreach (DyeingBatchRecipe item in entity.DyeingBatchRecipes)
            {
                item.DBRID = maxRecipeId++;
                item.DBatchID = entity.DBatchID;
                item.EntityState = EntityState.Added;
            }
            foreach (DyeingBatchChildFinishingProcess item in entity.DyeingBatchChildFinishingProcesses)
            {
                item.DBCFPID = maxProcessId++;
                item.DBatchID = entity.DBatchID;
                item.EntityState = EntityState.Added;
            }
            return entity;
        }
        private async Task<DyeingBatchMaster> UpdateAsync(DyeingBatchMaster entity)
        {
            var maxChildId = await _service.GetMaxIdAsync(TableNames.DYEING_BATCH_WITH_BATCH_MASTER, entity.DyeingBatchWithBatchMasters.Where(x => x.EntityState == EntityState.Added).Count());
            var maxItemId = await _service.GetMaxIdAsync(TableNames.DYEING_BATCH_ITEM, entity.DyeingBatchItems.Where(x => x.EntityState == EntityState.Added).Count());
            var maxItemRollId = await _service.GetMaxIdAsync(TableNames.DYEING_BATCH_ITEM_ROLL, entity.DyeingBatchItems.Sum(x => x.DyeingBatchItemRolls.Where(y => y.EntityState == EntityState.Added).Count()));
            var maxRecipeId = await _service.GetMaxIdAsync(TableNames.DYEING_BATCH_RECIPE, entity.DyeingBatchRecipes.Where(x => x.EntityState == EntityState.Added).Count());
            var maxProcessId = await _service.GetMaxIdAsync(TableNames.DYEING_BATCH_CHILD_FINISHING_PROCESS, entity.DyeingBatchChildFinishingProcesses.Where(x => x.EntityState == EntityState.Added).Count());
            var maxMergeBatchId = await _service.GetMaxIdAsync(TableNames.DYEING_BATCH_MERGE_BATCH, entity.DyeingBatchMergeBatchs.Where(x => x.EntityState == EntityState.Added).Count());

            foreach (DyeingBatchWithBatchMaster item in entity.DyeingBatchWithBatchMasters.ToList())
            {
                switch (item.EntityState)
                {
                    case EntityState.Added:
                        item.DBBMID = maxChildId++;
                        item.DBatchID = entity.DBatchID;
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
            foreach (DyeingBatchItem item in entity.DyeingBatchItems.ToList())
            {
                foreach (DyeingBatchItemRoll child in item.DyeingBatchItemRolls.ToList())
                {
                    switch (child.EntityState)
                    {
                        case EntityState.Added:
                            child.DBIRollID = maxItemRollId++;
                            child.DBIID = maxItemId;
                            child.DBatchID = entity.DBatchID;
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
                        item.DBIID = maxItemId++;
                        item.DBatchID = entity.DBatchID;
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
            foreach (DyeingBatchRecipe item in entity.DyeingBatchRecipes.ToList())
            {
                switch (item.EntityState)
                {
                    case EntityState.Added:
                        item.DBRID = maxRecipeId++;
                        item.DBatchID = entity.DBatchID;
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
            foreach (DyeingBatchChildFinishingProcess item in entity.DyeingBatchChildFinishingProcesses.ToList())
            {
                switch (item.EntityState)
                {
                    case EntityState.Added:

                        item.DBCFPID = maxProcessId++;
                        item.DBatchID = entity.DBatchID;
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
            foreach (DyeingBatchMergeBatch item in entity.DyeingBatchMergeBatchs.Where(x => x.EntityState == EntityState.Added).ToList())
            {
                item.DBMID = maxMergeBatchId++;
                item.DBatchID = entity.DBatchID;
                item.EntityState = EntityState.Added;
            }
            return entity;
        }
        public async Task<DyeingBatchMaster> GetAllByIDAsync(int id)
        {
            string sql = $@"
            ;Select * From DyeingBatchMaster Where DBatchID = {id}

            ;Select * From DyeingBatchWithBatchMaster Where DBatchID = {id}

            ;Select * From DyeingBatchRecipe Where DBatchID = {id}

            ;Select * From DyeingBatchItem Where DBatchID = {id}

            ;Select * From DyeingBatchItemRoll Where DBatchID = {id}

            ;SELECT * FROM DyeingBatchChildFinishingProcess WHERE DBatchID = {id}

            ;Select * From DyeingBatchMergeBatch Where DBatchID = {id}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                DyeingBatchMaster data = records.Read<DyeingBatchMaster>().FirstOrDefault();
                Guard.Against.NullObject(data);
                data.DyeingBatchWithBatchMasters = records.Read<DyeingBatchWithBatchMaster>().ToList();
                data.DyeingBatchRecipes = records.Read<DyeingBatchRecipe>().ToList();
                data.DyeingBatchItems = records.Read<DyeingBatchItem>().ToList();
                data.DyeingBatchItemRolls = records.Read<DyeingBatchItemRoll>().ToList();
                foreach (DyeingBatchItem item in data.DyeingBatchItems)
                {
                    item.DyeingBatchItemRolls = data.DyeingBatchItemRolls.Where(x => x.DBIID == item.DBIID).ToList();
                }
                data.DyeingBatchChildFinishingProcesses = records.Read<DyeingBatchChildFinishingProcess>().ToList();
                data.DyeingBatchMergeBatchs = records.Read<DyeingBatchMergeBatch>().ToList();
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
        public async Task UpdateEntityAsync(DyeingBatchMaster entity)
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
