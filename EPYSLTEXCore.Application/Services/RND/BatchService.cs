using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Application.Interfaces.RND;
using EPYSLTEXCore.Infrastructure.Data;
using Dapper;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using System.Data;
using System.Data.Entity;
using Microsoft.Data.SqlClient;
using System.Data.Entity.Validation;

namespace EPYSLTEXCore.Application.Services.RND
{
    public class BatchService : IBatchService
    {
        private readonly IDapperCRUDService<BatchMaster> _service;

        private readonly SqlConnection _connection;
        private readonly SqlConnection _connectionGmt;
        private SqlTransaction transaction;
        private SqlTransaction transactionGmt;
        public BatchService(IDapperCRUDService<BatchMaster> service)
        {
            _service = service;
            _service.Connection = service.GetConnection(AppConstants.GMT_CONNECTION);
            _connectionGmt = service.Connection;

            _service.Connection = service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _connection = service.Connection;
        }

        public async Task<List<BatchMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy;

            var sql = string.Empty;
            if (status == Status.Pending)
            {
                sql += $@";
                WITH FPM AS (
	                SELECT M.FPMasterID, M.ConceptID
	                FROM FinishingProcessChild C
	                INNER JOIN FinishingProcessMaster M ON M.FPMasterID = C.FPMasterID
	                GROUP BY M.FPMasterID, M.ConceptID
                ), 
                BDS AS (
	                SELECT FCM.ConceptID, FCM.BookingID, FCM.Remarks, FCM.GroupConceptNo, FCC.ColorID, FCM.IsBDS, FCM.TotalQty,
                    FCM.ExportOrderID,FCM.BuyerID,FCM.BuyerTeamID
                    FROM FreeConceptMaster FCM
                    INNER JOIN FreeConceptChildColor FCC ON FCC.ConceptID = FCM.ConceptID
                    LEFT JOIN KnittingPlanMaster KPM ON KPM.ConceptID = FCM.ConceptID
                    LEFT JOIN BatchItemRequirement BIR ON BIR.ConceptID = FCM.ConceptID
                    LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster SBM ON SBM.BookingID = FCM.BookingID
                    WHERE FCM.IsBDS <> 0 AND ISNULL(SBM.SampleID,0) <> 13 AND KPM.IsConfirm=1 --AND BIR.ConceptID IS NULL
                    GROUP BY FCM.ConceptID, FCM.BookingID,  FCM.Remarks, FCM.GroupConceptNo, FCC.ColorID, FCM.IsBDS, FCM.TotalQty,
                    FCM.ExportOrderID,FCM.BuyerID,FCM.BuyerTeamID
                    Having FCM.TotalQty > ISNULL(Sum(Case when FCM.SubGroupID = 1 Then BIR.Qty Else BIR.Pcs End) ,0)
                ),
                M AS (
	                /*SELECT	DM.RecipeID, DM.RecipeNo, DM.RecipeDate, DM.BookingID,
	                ColorID = CASE WHEN DM.ColorID > 0 THEN DM.ColorID ELSE FCC.ColorID END,
	                DM.RecipeFor, DM.BatchWeightKG, DM.Remarks,
	                DM.ExportOrderID, DM.BuyerID, DM.BuyerTeamID, 
	                FCM.ConceptID, FCM.GroupConceptNo ConceptNo, FCM.IsBDS
	                FROM FreeConceptMaster FCM
	                INNER JOIN FreeConceptChildColor FCC ON FCC.ConceptID = FCM.ConceptID
	                LEFT JOIN RecipeRequestChild RC ON RC.ConceptID = FCM.ConceptID
	                LEFT JOIN RecipeDefinitionMaster DM ON DM.RecipeReqMasterID = RC.RecipeReqMasterID
	                LEFT JOIN BatchMaster BM ON BM.CCColorID = FCC.CCColorID
                    LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster SBM ON SBM.BookingID = FCM.BookingID
	                WHERE FCM.IsBDS = 0 AND BM.BatchID IS NULL AND ISNULL(DM.IsArchive,0) = 0 AND ISNULL(SBM.SampleID,0) <> 13
                    --AND DM.IsApproved = 1 --AND FCM.ConceptNo=FCM.GroupConceptNo
	                GROUP BY DM.RecipeID, DM.RecipeNo, DM.RecipeDate, DM.BookingID, DM.ColorID, DM.RecipeFor, DM.BatchWeightKG, DM.Remarks,
	                DM.ExportOrderID, DM.BuyerID, DM.BuyerTeamID, FCM.ConceptID, FCM.GroupConceptNo, FCM.IsBDS, FCC.ColorID*/

                    SELECT	DM.RecipeID, DM.RecipeNo, DM.RecipeDate, DM.BookingID,
	                ColorID = CASE WHEN DM.ColorID > 0 THEN DM.ColorID ELSE FCC.ColorID END,
	                DM.RecipeFor, DM.BatchWeightKG, DM.Remarks,
	                DM.ExportOrderID, DM.BuyerID, DM.BuyerTeamID, 
	                FCM.ConceptID, FCM.GroupConceptNo ConceptNo, FCM.IsBDS
	                FROM FreeConceptMaster FCM
					LEFT JOIN KnittingProduction KP ON FCM.ConceptID = KP.ConceptID
					LEFT Join BatchChild BC ON BC.GRollID = KP.GRollID
	                INNER JOIN FreeConceptChildColor FCC ON FCC.ConceptID = FCM.ConceptID
	                LEFT JOIN RecipeRequestChild RC ON RC.ConceptID = FCM.ConceptID
	                LEFT JOIN RecipeDefinitionMaster DM ON DM.RecipeReqMasterID = RC.RecipeReqMasterID
	                LEFT JOIN BatchMaster BM ON BM.CCColorID = FCC.CCColorID
					--LEFT Join BatchChild BC ON BC.BatchID = BM.BatchID
                    LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster SBM ON SBM.BookingID = FCM.BookingID
	                WHERE BC.GRollID IS NULL AND InActive=0 AND
					FCM.IsBDS = 0 AND 
					--BM.BatchID IS NULL AND 
					ISNULL(DM.IsArchive,0) = 0 AND 
					ISNULL(SBM.SampleID,0) <> 13
                    --AND DM.IsApproved = 1 --AND FCM.ConceptNo=FCM.GroupConceptNo
	                GROUP BY DM.RecipeID, DM.RecipeNo, DM.RecipeDate, DM.BookingID, DM.ColorID, DM.RecipeFor, DM.BatchWeightKG, DM.Remarks,
	                DM.ExportOrderID, DM.BuyerID, DM.BuyerTeamID, FCM.ConceptID, FCM.GroupConceptNo, FCM.IsBDS, FCC.ColorID
                ),
                CONCEPT_BDS AS (
                    SELECT M.RecipeID, M.RecipeNo, M.RecipeDate, M.ConceptID, M.BookingID, M.ColorID, M.RecipeFor, M.BatchWeightKG, M.Remarks,
	                M.ExportOrderID, M.BuyerID, M.BuyerTeamID, '' RecipeForName, M.ConceptNo, M.IsBDS
	                FROM M
	                UNION
	                SELECT 0 RecipeID, '' RecipeNo, null RecipeDate, BDS.ConceptID, BDS.BookingID, BDS.ColorID, 0 RecipeFor, 0 BatchWeightKG, BDS.Remarks,
	                ExportOrderID, BuyerID, BuyerTeamID, '' RecipeForName, BDS.GroupConceptNo ConceptNo, BDS.IsBDS
	                FROM BDS
                ), FFF As (
                    SELECT B.RecipeID, B.RecipeNo, B.RecipeDate, B.BookingID, B.ColorID, B.RecipeFor, B.BatchWeightKG, B.Remarks,
                    B.ExportOrderID, B.BuyerID, B.BuyerTeamID, RecFor.ValueName RecipeForName, COL.SegmentValue ColorName, B.ConceptNo,
                    BuyerName = CASE WHEN B.BuyerID = 0 THEN 'R&D' ELSE CTO.ShortName END, 
					BuyerTeamName = CASE WHEN B.BuyerTeamID = 0 THEN 'R&D' ELSE CCT.TeamName END,
				    B.IsBDS
                    FROM CONCEPT_BDS B
                    LEFT JOIN {DbNames.EPYSL}..EntityTypeValue RecFor ON RecFor.ValueID = B.RecipeFor
                    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID = B.ColorID
                    LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = B.BuyerID
                    LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = B.BuyerTeamID
                    GROUP BY B.RecipeID, B.RecipeNo, B.RecipeDate, B.BookingID, B.ColorID, B.RecipeFor, B.BatchWeightKG, B.Remarks,
                    B.ExportOrderID, B.BuyerID, B.BuyerTeamID, RecFor.ValueName, COL.SegmentValue, B.ConceptNo,CTO.ShortName, CCT.TeamName, B.IsBDS
                )
                Select *, Count(*) Over() TotalRows From FFF ";

                if (orderBy.NullOrEmpty()) orderBy = "ORDER BY ConceptNo DESC";
            }
            else if (status == Status.Completed)
            {
                sql += $@";WITH M AS (
                            SELECT	*
                            FROM BatchMaster BM WHERE BM.IsApproved = 1
                        ), FFF As (
                                SELECT M.BatchID, M.BatchNo, M.BatchDate, M.RecipeID, M.BookingID, M.ColorID, M.BatchWeightKG, M.BatchWeightPcs, M.Remarks,
                                M.ExportOrderID, M.BuyerID, M.BuyerTeamID, RDM.RecipeNo, RDM.RecipeDate, COL.SegmentValue ColorName, FM.GroupConceptNo ConceptNo,
                                BuyerName = CASE WHEN M.BuyerID = 0 THEN 'R&D' ELSE CTO.ShortName END, 
                                BuyerTeamName = CASE WHEN M.BuyerTeamID = 0 THEN 'R&D' ELSE CCT.TeamName END,
                                Count(*) Over() TotalRows
                                FROM M
                                LEFT JOIN RecipeDefinitionMaster RDM ON RDM.RecipeID = M.RecipeID
                                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID = M.ColorID
						        INNER JOIN FreeConceptChildColor CC ON CC.CCColorID = M.CCColorID
	                            INNER JOIN FreeConceptMaster FM ON FM.ConceptID = CC.ConceptID
                                LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = M.BuyerID
                                LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = M.BuyerTeamID
                        )
                        Select * From FFF ";

                if (orderBy.NullOrEmpty()) orderBy = "ORDER BY BatchID DESC";
            }
            else if (status == Status.Approved)
            {
                sql += $@";WITH M AS (
                            SELECT	*
                            FROM BatchMaster BM WHERE BM.IsApproved = 1
                        ), FFF AS (
                            SELECT M.BatchID, M.BatchNo, M.BatchDate, M.RecipeID, M.BookingID, M.ColorID, M.BatchWeightKG, M.BatchWeightPcs, M.Remarks,
                            M.ExportOrderID, M.BuyerID, M.BuyerTeamID, RDM.RecipeNo, RDM.RecipeDate, COL.SegmentValue ColorName, FM.GroupConceptNo ConceptNo,
                            BuyerName = CASE WHEN M.BuyerID = 0 THEN 'R&D' ELSE CTO.ShortName END, 
                            BuyerTeamName = CASE WHEN M.BuyerTeamID = 0 THEN 'R&D' ELSE CCT.TeamName END,
                            Count(*) Over() TotalRows
                            FROM M
                            INNER JOIN RecipeDefinitionMaster RDM ON RDM.RecipeID = M.RecipeID
                            INNER JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID = M.ColorID
						    INNER JOIN FreeConceptChildColor CC ON CC.CCColorID = M.CCColorID
	                        INNER JOIN FreeConceptMaster FM ON FM.ConceptID = CC.ConceptID
                            LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = M.BuyerID
                            LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = M.BuyerTeamID
                        )
                        Select * From FFF ";

                if (orderBy.NullOrEmpty()) orderBy = "ORDER BY BatchID DESC";
            }

            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<BatchMaster>(sql);
        }

        public async Task<BatchMaster> GetNewAsync(int recipeID, string conceptNo, int bookingID, int isBDS, int colorID)
        {
            string sql = "";
            if (!string.IsNullOrEmpty(conceptNo))
            {
                if (isBDS == 0) //Concept
                {
                    sql = $@"
                        SELECT DM.RecipeID, DM.RecipeNo, DM.RecipeDate, FM.ConceptID, FM.BookingID, DM.ItemMasterID, 
	                    FCC.ColorID, FCC.CCColorID, DM.RecipeFor, DM.BatchWeightKG, DM.Remarks,
                        FM.ExportOrderID, FM.BuyerID, FM.BuyerTeamID, RecFor.ValueName RecipeForName, COL.SegmentValue ColorName, FM.GroupConceptNo ConceptNo, FM.GroupConceptNo SLNo
                        FROM FreeConceptChildColor FCC
	                    LEFT JOIN FreeConceptMaster FM ON FM.ConceptID = FCC.ConceptID
	                    LEFT JOIN RecipeDefinitionMaster DM ON DM.ConceptID = FM.ConceptID
	                    LEFT JOIN RecipeRequestChild RC ON RC.RecipeReqMasterID = DM.RecipeReqMasterID
                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue RecFor ON RecFor.ValueID = DM.RecipeFor
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID = FCC.ColorID
	                    WHERE FCC.ColorID = {colorID} AND FM.GroupConceptNo = '{conceptNo}';

                        -----BatchItemRequirement
                        /*
                        WITH F AS(
		                    SELECT FCM.ConceptID, FCM.ItemMasterID, FCM.SubGroupID, FCM.Length, FCM.Width, FCM.FUPartID
		                    FROM FreeConceptChildColor FCC
		                    INNER JOIN FreeConceptMaster FCM ON FCM.ConceptID = FCC.ConceptID
		                    WHERE FCC.ColorID = {colorID} 
		                    AND FCM.IsBDS = 0
		                    AND FCM.GroupConceptNo = '{conceptNo}'
		                    AND FCM.SubGroupID = 1
	                    ),
	                    CC AS (
		                    SELECT FCM.ConceptID, FCM.ItemMasterID, FCM.SubGroupID, FCM.Length, FCM.Width, FCM.FUPartID
		                    FROM FreeConceptMaster FCM
		                    WHERE FCM.IsBDS = 0 
		                    AND FCM.GroupConceptNo = '{conceptNo}'
		                    AND FCM.SubGroupID IN (11,12)
	                    ),
	                    FCC AS (
		                    SELECT * FROM F
		                    UNION
		                    SELECT * FROM CC
	                    ),
                        */

                        WITH F AS(
	                        SELECT FCM.GroupConceptNo
	                        FROM FreeConceptChildColor FCC
	                        INNER JOIN FreeConceptMaster FCM ON FCM.ConceptID = FCC.ConceptID
	                        WHERE FCC.ColorID = {colorID} 
	                        AND FCM.IsBDS = 0
	                        AND FCM.GroupConceptNo = '{conceptNo}'
	                        AND FCM.SubGroupID = 1
                        ),
                        AllFabrics AS (
	                        SELECT FCM.ConceptID, FCM.ItemMasterID, FCM.SubGroupID, FCM.Length, FCM.Width, FCM.FUPartID
	                        FROM FreeConceptMaster FCM
	                        INNER JOIN F ON F.GroupConceptNo = FCM.GroupConceptNo
	                        WHERE FCM.SubGroupID = 1
                        ),CC AS (
	                        SELECT FCM.ConceptID, FCM.ItemMasterID, FCM.SubGroupID, FCM.Length, FCM.Width, FCM.FUPartID
	                        FROM FreeConceptMaster FCM
	                        WHERE FCM.IsBDS = 0 
	                        AND FCM.GroupConceptNo = '{conceptNo}'
	                        AND FCM.SubGroupID IN (11,12)
                        ),
                        FCC AS (
	                        SELECT * FROM AllFabrics
	                        UNION
	                        SELECT * FROM CC
                        ),
                        PB As(
		                    Select b.ConceptID, Sum(b.Pcs) Pcs, Sum(b.Qty) Qty
		                    From BatchMaster a
		                    Inner Join BatchItemRequirement b on b.BatchID = a.BatchID
		                    Where a.ColorID = {colorID} And a.GroupConceptNo = '{conceptNo}'
		                    Group By b.ConceptID
	                    )
                        SELECT II.RecipeItemInfoID, M.ConceptID, M.ItemMasterID,

                        ConceptOrSampleQtyKg = CASE WHEN FCM.SubGroupID = 1 THEN ISNULL(FCM.TotalQty,0) ELSE ISNULL(FCM.TotalQtyInKG,0) END,
	                    ConceptOrSampleQtyPcs = CASE WHEN FCM.SubGroupID <> 1 THEN ISNULL(FCM.TotalQty,0) ELSE 0 END,

	                    Qty = 0, --(CASE WHEN FCM.SubGroupID = 1 THEN ISNULL(FCM.TotalQty,0) ELSE ISNULL(FCM.TotalQtyInKG,0) END) - ISNULL(PB.Qty,0),
	                    Pcs = 0, --(CASE WHEN FCM.SubGroupID <> 1 THEN ISNULL(FCM.TotalQty,0) ELSE 0 END) - ISNULL(PB.Pcs,0),
					
	                    PlannedBatchQtyKg = ISNULL(PB.Qty,0),
	                    PlannedBatchQtyPcs = ISNULL(PB.Pcs,0),

                        KnittingType.TypeName KnittingType,Composition.SegmentValue FabricComposition,
                        Construction.SegmentValue FabricConstruction,Technical.TechnicalName,Gsm.SegmentValue FabricGsm, M.SubGroupID SubGroupID, SG.SubGroupName SubGroup,
                        M.Length, M.Width, M.FUPartID, FU.PartName FUPartName,
                        ColorName = CASE WHEN M.SubGroupID=1 THEN Color.SegmentValue ELSE CollarColor.SegmentValue END
                        FROM FCC
                        LEFT JOIN FreeConceptMaster M ON M.ConceptID = FCC.ConceptID
	                    LEFT JOIN RecipeDefinitionItemInfo II ON II.ConceptID = M.ConceptID
                        LEFT JOIN KnittingMachineType KnittingType ON KnittingType.TypeID = M.KnittingTypeID
                        LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = M.ItemMasterID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = M.CompositionID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = M.ConstructionID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = M.GSMID
                        LEFT JOIN {DbNames.EPYSL}..ItemSubGroup SG ON SG.SubGroupID = M.SubGroupID
                        LEFT JOIN FabricTechnicalName Technical ON Technical.TechnicalNameId=M.TechnicalNameId
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = IM.Segment3ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue CollarColor ON CollarColor.SegmentValueID = IM.Segment5ValueID
                        LEFT JOIN FreeConceptMaster FCM ON FCM.ConceptID=M.ConceptID
	                    LEFT JOIN {DbNames.EPYSL}..FabricUsedPart FU ON FU.FUPartID = M.FUPartID
	                    LEFT JOIN PB on PB.ConceptID = M.ConceptID
                        WHERE (CASE WHEN FCM.SubGroupID = 1 THEN ISNULL(FCM.TotalQty,0) ELSE ISNULL(FCM.TotalQtyInKG,0) END) > ISNULL(PB.Qty,0);

                    --Childs
                    ;SELECT DI.RecipeDInfoID, C.Temperature TempIn, C.ProcessTime, EV.ValueName FiberPart, ISV.SegmentValue ColorName
                    FROM RecipeDefinitionChild C
                    INNER JOIN RecipeDefinitionDyeingInfo DI ON DI.RecipeDInfoID = C.RecipeDInfoID
                    INNER JOIN {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID = DI.FiberPartID
                    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = DI.ColorID
                    WHERE C.RecipeID = {recipeID}
                    GROUP BY DI.RecipeDInfoID, C.Temperature, C.ProcessTime, EV.ValueName, ISV.SegmentValue;

                    --Def Childs
				    SELECT C.RecipeChildID, C.RecipeID,C.ProcessID,C.Qty,C.UnitID,C.TempIn,C.TempOut,C.ParticularsID,C.RawItemID,I.ItemName,C.ProcessTime,
				    P.ValueName ProcessName,PL.ValueName ParticularsName,R.ItemName RawItemName, UU.DisplayUnitDesc Unit, C.IsPercentage,
				    C.RecipeDInfoID, C.Temperature, EV.ValueName FiberPart, ISV.SegmentValue ColorName
				    FROM RecipeDefinitionChild C
				    LEFT JOIN {DbNames.EPYSL}..ItemMaster I ON I.ItemMasterID=C.RawItemID
				    LEFT JOIN {DbNames.EPYSL}..EntityTypeValue P ON P.ValueID = C.ProcessID
				    LEFT JOIN {DbNames.EPYSL}..EntityTypeValue PL ON PL.ValueID = C.ParticularsID
				    LEFT JOIN {DbNames.EPYSL}..ItemMaster R ON R.ItemMasterID = C.RawItemID
				    LEFT JOIN {DbNames.EPYSL}..Unit UU ON UU.UnitID = C.UnitID
				    INNER JOIN RecipeDefinitionDyeingInfo DI ON DI.RecipeDInfoID = C.RecipeDInfoID
				    INNER JOIN {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID = DI.FiberPartID
				    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = DI.ColorID
				    WHERE C.RecipeID = {recipeID}

                    -----K Production
                    ; WITH M AS(
                            SELECT KP.*
                            FROM KnittingProduction KP
							INNER JOIN FreeConceptMaster FC ON FC.ConceptID = KP.ConceptID
							WHERE FC.GroupConceptNo = '{conceptNo}' AND KP.InActive = 0
                    )
                    SELECT M.GRollID, M.KJobCardMasterID, M.ProductionDate, M.ConceptID, M.OperatorID, M.ShiftID, M.RollSeqNo, M.RollNo, M.RollQty, M.RollQtyPcs, M.ProductionGSM, M.ProductionWidth,
                    M.FirstRollCheck, M.FirstRollCheckBy, M.FirstRollCheckDate, M.FirstRollPass, M.SendforQC, M.SendQCDate, M.SendQCBy, M.QCComplete, M.QCCompleteDate, M.QCCompleteBy,
                    M.QCWidth, M.QCGSM, M.QCPass, M.QCPassQty, M.AddedBy, M.UpdatedBy, M.ProdComplete, M.ProdQty, M.Hole, M.Loop, M.SetOff, M.LycraOut, M.LycraDrop, M.OilSpot, M.Slub, M.FlyingDust,
                    M.MissingYarn, M.Knot, M.DropStitch, M.YarnContra, M.NeddleBreakage, M.Defected, M.WrongDesign, M.Patta, M.ShinkerMark, M.NeddleMark, M.EdgeMark, M.WheelFree, M.CountMix,
                    M.ThickAndThin, M.LineStar, M.QCOthers, M.Comment, M.CalculateValue, M.Grade, M.RollLength, M.Hold, M.QCBy, M.QCShiftID, M.BookingID, M.DateAdded, M.DateUpdated,
                    M.ExportOrderID, M.BuyerID, M.BuyerTeamID, M.ParentGRollID, M.InActive, M.InActiveBy, M.InActiveDate, M.InActiveReason, M.BatchID
                    FROM M
					Left Join BatchChild BC ON BC.GRollID = M.GRollID
					where BC.GRollID IS NULL;

                    ----Unit
                    ;SELECT CAST(UnitID AS VARCHAR) AS id, DisplayUnitDesc AS text
                    FROM {DbNames.EPYSL}..Unit;";

                    /*
                    sql = $@"
                        ;WITH M AS (
                            SELECT DM.RecipeID, DM.RecipeNo, DM.RecipeDate, DM.BookingID, DM.ItemMasterID, DM.ColorID, DM.CCColorID, DM.RecipeFor, DM.BatchWeightKG, DM.Remarks,
							DM.ExportOrderID, DM.BuyerID, DM.BuyerTeamID, FPM.ConceptID
                            FROM RecipeDefinitionMaster DM
							INNER JOIN RecipeRequestChild RC ON RC.RecipeReqMasterID = DM.RecipeReqMasterID
							LEFT JOIN FinishingProcessMaster FPM ON FPM.ConceptID = RC.ConceptID
	                        WHERE DM.RecipeID = {recipeID}
							GROUP BY DM.RecipeID, DM.RecipeNo, DM.RecipeDate, DM.BookingID, DM.ItemMasterID, DM.ColorID, DM.CCColorID, DM.RecipeFor, DM.BatchWeightKG, DM.Remarks,
							DM.ExportOrderID, DM.BuyerID, DM.BuyerTeamID, FPM.ConceptID
                        )
                        SELECT M.RecipeID, M.RecipeNo, M.RecipeDate, M.ConceptID, M.BookingID, M.ItemMasterID, M.ColorID, M.CCColorID, M.RecipeFor, M.BatchWeightKG, M.Remarks,
                        M.ExportOrderID, M.BuyerID, M.BuyerTeamID, RecFor.ValueName RecipeForName, COL.SegmentValue ColorName, FM.GroupConceptNo ConceptNo, FM.GroupConceptNo SLNo
                        FROM M
                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue RecFor ON RecFor.ValueID = M.RecipeFor
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID = M.ColorID
                        LEFT JOIN FreeConceptChildColor FCC ON FCC.CCColorID = M.CCColorID
                        LEFT JOIN FreeConceptMaster FM ON FM.ConceptID = FCC.ConceptID;

                        -----BatchItemRequirement
                        WITH II AS (
							SELECT * FROM RecipeDefinitionItemInfo WHERE RecipeID =  {recipeID}
                        ),
						PB As(
							Select b.ConceptID, Sum(b.Pcs) Pcs, Sum(b.Qty) Qty
							From BatchMaster a
							Inner Join BatchItemRequirement b on b.BatchID = a.BatchID
							Where a.ColorID={colorID} And a.GroupConceptNo = '{conceptNo}'
							Group By b.ConceptID
						)
                        SELECT II.RecipeItemInfoID, II.ConceptID, II.ItemMasterID,

                        ConceptOrSampleQtyKg = CASE WHEN FCM.SubGroupID = 1 THEN ISNULL(FCM.TotalQty,0) ELSE ISNULL(FCM.TotalQtyInKG,0) END,
					    ConceptOrSampleQtyPcs = CASE WHEN FCM.SubGroupID <> 1 THEN ISNULL(FCM.TotalQty,0) ELSE 0 END,

						Qty = (CASE WHEN FCM.SubGroupID = 1 THEN ISNULL(FCM.TotalQty,0) ELSE ISNULL(FCM.TotalQtyInKG,0) END) - ISNULL(PB.Qty,0),
					    Pcs = (CASE WHEN FCM.SubGroupID <> 1 THEN ISNULL(FCM.TotalQty,0) ELSE 0 END) - ISNULL(PB.Pcs,0),
					
						PlannedBatchQtyKg = ISNULL(PB.Qty,0),
						PlannedBatchQtyPcs = ISNULL(PB.Pcs,0),

                        KnittingType.TypeName KnittingType,Composition.SegmentValue FabricComposition,
                        Construction.SegmentValue FabricConstruction,Technical.TechnicalName,Gsm.SegmentValue FabricGsm, M.SubGroupID SubGroupID, SG.SubGroupName SubGroup,
                        M.Length, M.Width, M.FUPartID, FU.PartName FUPartName,
                        ColorName = CASE WHEN M.SubGroupID=1 THEN Color.SegmentValue ELSE CollarColor.SegmentValue END
                        FROM II
                        LEFT JOIN FreeConceptMaster M ON M.ConceptID = II.ConceptID
                        LEFT JOIN KnittingMachineType KnittingType ON KnittingType.TypeID = M.KnittingTypeID
                        LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = M.ItemMasterID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = M.CompositionID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = M.ConstructionID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = M.GSMID
                        LEFT JOIN {DbNames.EPYSL}..ItemSubGroup SG ON SG.SubGroupID = M.SubGroupID
                        LEFT JOIN FabricTechnicalName Technical ON Technical.TechnicalNameId=M.TechnicalNameId
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = IM.Segment3ValueID
					    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue CollarColor ON CollarColor.SegmentValueID = IM.Segment5ValueID
                        LEFT JOIN FreeConceptMaster FCM ON FCM.ConceptID=M.ConceptID
						LEFT JOIN {DbNames.EPYSL}..FabricUsedPart FU ON FU.FUPartID = M.FUPartID
						LEFT JOIN PB on PB.ConceptID = M.ConceptID
                        WHERE (CASE WHEN FCM.SubGroupID = 1 THEN ISNULL(FCM.TotalQty,0) ELSE ISNULL(FCM.TotalQtyInKG,0) END) > ISNULL(PB.Qty,0);

                    --Childs
                    ;SELECT DI.RecipeDInfoID, C.Temperature TempIn, C.ProcessTime, EV.ValueName FiberPart, ISV.SegmentValue ColorName
                    FROM RecipeDefinitionChild C
                    INNER JOIN RecipeDefinitionDyeingInfo DI ON DI.RecipeDInfoID = C.RecipeDInfoID
                    INNER JOIN {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID = DI.FiberPartID
                    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = DI.ColorID
                    WHERE C.RecipeID = {recipeID}
                    GROUP BY DI.RecipeDInfoID, C.Temperature, C.ProcessTime, EV.ValueName, ISV.SegmentValue;

                    --Def Childs
				    SELECT C.RecipeChildID, C.RecipeID,C.ProcessID,C.Qty,C.UnitID,C.TempIn,C.TempOut,C.ParticularsID,C.RawItemID,I.ItemName,C.ProcessTime,
				    P.ValueName ProcessName,PL.ValueName ParticularsName,R.ItemName RawItemName, UU.DisplayUnitDesc Unit, C.IsPercentage,
				    C.RecipeDInfoID, C.Temperature, EV.ValueName FiberPart, ISV.SegmentValue ColorName
				    FROM RecipeDefinitionChild C
				    LEFT JOIN {DbNames.EPYSL}..ItemMaster I ON I.ItemMasterID=C.RawItemID
				    LEFT JOIN {DbNames.EPYSL}..EntityTypeValue P ON P.ValueID = C.ProcessID
				    LEFT JOIN {DbNames.EPYSL}..EntityTypeValue PL ON PL.ValueID = C.ParticularsID
				    LEFT JOIN {DbNames.EPYSL}..ItemMaster R ON R.ItemMasterID = C.RawItemID
				    LEFT JOIN {DbNames.EPYSL}..Unit UU ON UU.UnitID = C.UnitID
				    INNER JOIN RecipeDefinitionDyeingInfo DI ON DI.RecipeDInfoID = C.RecipeDInfoID
				    INNER JOIN {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID = DI.FiberPartID
				    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = DI.ColorID
				    WHERE C.RecipeID = {recipeID}

                    -----K Production
                    ; WITH M AS(
                            SELECT KP.*
                            FROM KnittingProduction KP
							INNER JOIN FreeConceptMaster FC ON FC.ConceptID = KP.ConceptID
							WHERE FC.GroupConceptNo = '{conceptNo}' AND KP.InActive = 0
                    )
                    SELECT M.GRollID, M.KJobCardMasterID, M.ProductionDate, M.ConceptID, M.OperatorID, M.ShiftID, M.RollSeqNo, M.RollNo, M.RollQty, M.RollQtyPcs, M.ProductionGSM, M.ProductionWidth,
                    M.FirstRollCheck, M.FirstRollCheckBy, M.FirstRollCheckDate, M.FirstRollPass, M.SendforQC, M.SendQCDate, M.SendQCBy, M.QCComplete, M.QCCompleteDate, M.QCCompleteBy,
                    M.QCWidth, M.QCGSM, M.QCPass, M.QCPassQty, M.AddedBy, M.UpdatedBy, M.ProdComplete, M.ProdQty, M.Hole, M.Loop, M.SetOff, M.LycraOut, M.LycraDrop, M.OilSpot, M.Slub, M.FlyingDust,
                    M.MissingYarn, M.Knot, M.DropStitch, M.YarnContra, M.NeddleBreakage, M.Defected, M.WrongDesign, M.Patta, M.ShinkerMark, M.NeddleMark, M.EdgeMark, M.WheelFree, M.CountMix,
                    M.ThickAndThin, M.LineStar, M.QCOthers, M.Comment, M.CalculateValue, M.Grade, M.RollLength, M.Hold, M.QCBy, M.QCShiftID, M.BookingID, M.DateAdded, M.DateUpdated,
                    M.ExportOrderID, M.BuyerID, M.BuyerTeamID, M.ParentGRollID, M.InActive, M.InActiveBy, M.InActiveDate, M.InActiveReason, M.BatchID
                    FROM M
					Left Join BatchChild BC ON BC.GRollID = M.GRollID
					where BC.GRollID IS NULL;

                    ----Unit
                    ;SELECT CAST(UnitID AS VARCHAR) AS id, DisplayUnitDesc AS text
                    FROM {DbNames.EPYSL}..Unit;";
                    */
                }
                else if (isBDS == 1) //BDS
                {
                    sql = $@"
                     ;SELECT FCM.ConceptID, FCM.BookingID, FCM.ItemMasterID, FCM.Remarks, FCM.GroupConceptNo ConceptNo, FCC.CCColorID, FCC.ColorID, COL.SegmentValue ColorName, FBA.SLNo,
                    FCM.ExportOrderID,FCM.BuyerID,FCM.BuyerTeamID
                    FROM FreeConceptMaster FCM
					Inner Join FBookingAcknowledge FBA ON FBA.BookingID = FCM.BookingID
                    INNER JOIN FreeConceptChildColor FCC ON FCC.ConceptID = FCM.ConceptID
                    LEFT JOIN KnittingPlanMaster KPM ON KPM.ConceptID = FCM.ConceptID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID = FCC.ColorID
                    WHERE FCM.GroupConceptNo = '{conceptNo}' AND FCC.ColorID={colorID} AND FCM.IsBDS=1
                    GROUP BY FCM.ConceptID, FCM.BookingID, FCM.ItemMasterID, FCM.Remarks, FCM.GroupConceptNo, FCC.CCColorID, FCC.ColorID, COL.SegmentValue, FBA.SLNo,FCM.ExportOrderID,FCM.BuyerID,FCM.BuyerTeamID;

                    -----BatchItemRequirement
                    WITH F AS(
	                    SELECT FCM.*
	                    FROM FreeConceptChildColor FCC
	                    INNER JOIN FreeConceptMaster FCM ON FCM.ConceptID = FCC.ConceptID
	                    WHERE FCC.ColorID={colorID} 
	                    AND FCM.IsBDS=1 
	                    AND FCM.GroupConceptNo = '{conceptNo}'
	                    AND FCM.SubGroupID=1
                    ),
                    CC AS (
	                    SELECT FCM.*
	                    FROM FreeConceptChildColor FCC
	                    INNER JOIN FreeConceptMaster FCM ON FCM.ConceptID = FCC.ConceptID
	                    WHERE FCM.IsBDS=1 
	                    AND FCM.GroupConceptNo = '{conceptNo}'
	                    AND FCM.SubGroupID<>1
                    ),
                    M AS (
	                    SELECT * FROM F
	                    UNION
	                    SELECT * FROM CC
                    ),
                    PB As(
						Select b.ConceptID, Sum(b.Pcs) Pcs, Sum(b.Qty) Qty
						From BatchMaster a
						Inner Join BatchItemRequirement b on b.BatchID = a.BatchID
						Where a.ColorID={colorID} And a.GroupConceptNo = '{conceptNo}'
						Group By b.ConceptID
					)
                    SELECT M.ConceptID, M.ItemMasterID, 
                    
                    ConceptOrSampleQtyKg = CASE WHEN FCM.SubGroupID = 1 THEN ISNULL(FCM.TotalQty,0) ELSE ISNULL(FCM.TotalQtyInKG,0) END,
					ConceptOrSampleQtyPcs = CASE WHEN FCM.SubGroupID <> 1 THEN ISNULL(FCM.TotalQty,0) ELSE 0 END,

				    Qty = 0, --(CASE WHEN FCM.SubGroupID = 1 THEN ISNULL(FCM.TotalQty,0) ELSE ISNULL(FCM.TotalQtyInKG,0) END) - ISNULL(PB.Qty,0),
					Pcs = 0, --(CASE WHEN FCM.SubGroupID <> 1 THEN ISNULL(FCM.TotalQty,0) ELSE 0 END) - ISNULL(PB.Pcs,0),
					
					PlannedBatchQtyKg = ISNULL(PB.Qty,0),
					PlannedBatchQtyPcs = ISNULL(PB.Pcs,0),

                    KnittingType.TypeName KnittingType,Composition.SegmentValue FabricComposition,                    
                    Construction.SegmentValue FabricConstruction,Technical.TechnicalName,Gsm.SegmentValue FabricGsm, M.SubGroupID SubGroupID, 
                    SG.SubGroupName SubGroup, M.Length, M.Width, M.FUPartID, FU.PartName FUPartName,
                    ColorName = CASE WHEN M.SubGroupID=1 THEN Color.SegmentValue ELSE CollarColor.SegmentValue END
                    FROM M
                    LEFT JOIN KnittingMachineType KnittingType ON KnittingType.TypeID = M.KnittingTypeID
                    LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = M.ItemMasterID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = M.CompositionID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = M.ConstructionID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = M.GSMID
                    LEFT JOIN {DbNames.EPYSL}..ItemSubGroup SG ON SG.SubGroupID = M.SubGroupID
                    LEFT JOIN FabricTechnicalName Technical ON Technical.TechnicalNameId=M.TechnicalNameId
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = IM.Segment3ValueID
					LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue CollarColor ON CollarColor.SegmentValueID = IM.Segment5ValueID
                    LEFT JOIN FreeConceptMaster FCM ON FCM.ConceptID=M.ConceptID
                    LEFT JOIN {DbNames.EPYSL}..FabricUsedPart FU ON FU.FUPartID = M.FUPartID
                    LEFT JOIN PB on PB.ConceptID = M.ConceptID
                    WHERE (CASE WHEN FCM.SubGroupID = 1 THEN ISNULL(FCM.TotalQty,0) ELSE ISNULL(FCM.TotalQtyInKG,0) END) > ISNULL(PB.Qty,0);

                    -----K Production
                    ; WITH M AS(
                            SELECT KP.*
                            FROM KnittingProduction KP
							INNER JOIN FreeConceptMaster FC ON FC.ConceptID = KP.ConceptID
							WHERE FC.GroupConceptNo = '{conceptNo}' AND KP.InActive = 0
                    )
                    SELECT M.GRollID, M.KJobCardMasterID, M.ProductionDate, M.ConceptID, M.OperatorID, M.ShiftID, M.RollSeqNo, M.RollNo, M.RollQty, M.RollQtyPcs, M.ProductionGSM, M.ProductionWidth,
                    M.FirstRollCheck, M.FirstRollCheckBy, M.FirstRollCheckDate, M.FirstRollPass, M.SendforQC, M.SendQCDate, M.SendQCBy, M.QCComplete, M.QCCompleteDate, M.QCCompleteBy,
                    M.QCWidth, M.QCGSM, M.QCPass, M.QCPassQty, M.AddedBy, M.UpdatedBy, M.ProdComplete, M.ProdQty, M.Hole, M.Loop, M.SetOff, M.LycraOut, M.LycraDrop, M.OilSpot, M.Slub, M.FlyingDust,
                    M.MissingYarn, M.Knot, M.DropStitch, M.YarnContra, M.NeddleBreakage, M.Defected, M.WrongDesign, M.Patta, M.ShinkerMark, M.NeddleMark, M.EdgeMark, M.WheelFree, M.CountMix,
                    M.ThickAndThin, M.LineStar, M.QCOthers, M.Comment, M.CalculateValue, M.Grade, M.RollLength, M.Hold, M.QCBy, M.QCShiftID, M.BookingID, M.DateAdded, M.DateUpdated,
                    M.ExportOrderID, M.BuyerID, M.BuyerTeamID, M.ParentGRollID, M.InActive, M.InActiveBy, M.InActiveDate, M.InActiveReason, M.BatchID
                    FROM M
					Left Join BatchChild BC ON BC.GRollID = M.GRollID
					where BC.GRollID IS NULL;

                    ----Unit
                    ;SELECT CAST(UnitID AS VARCHAR) AS id, DisplayUnitDesc AS text
                    FROM {DbNames.EPYSL}..Unit;";
                }
                else if (isBDS == 2) //BULK
                {
                    sql = $@"
                     ;SELECT FCM.ConceptID, FCM.BookingID, FCM.ItemMasterID, FCM.Remarks, FCM.GroupConceptNo ConceptNo, FCC.CCColorID, FCC.ColorID, COL.SegmentValue ColorName, FBA.SLNo,
                    FCM.ExportOrderID,FCM.BuyerID,FCM.BuyerTeamID
                    FROM FreeConceptMaster FCM
					Inner Join FBookingAcknowledge FBA ON FBA.BookingID = FCM.BookingID
                    INNER JOIN FreeConceptChildColor FCC ON FCC.ConceptID = FCM.ConceptID
                    LEFT JOIN KnittingPlanMaster KPM ON KPM.ConceptID = FCM.ConceptID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID = FCC.ColorID
                    WHERE FCM.GroupConceptNo = '{conceptNo}' AND FCC.ColorID={colorID} AND FCM.IsBDS=2
                    GROUP BY FCM.ConceptID, FCM.BookingID, FCM.ItemMasterID, FCM.Remarks, FCM.GroupConceptNo, FCC.CCColorID, FCC.ColorID, COL.SegmentValue, FBA.SLNo,FCM.ExportOrderID,FCM.BuyerID,FCM.BuyerTeamID;

                    -----BatchItemRequirement
                    WITH F AS(
	                    SELECT FCM.*
	                    FROM FreeConceptChildColor FCC
	                    INNER JOIN FreeConceptMaster FCM ON FCM.ConceptID = FCC.ConceptID
	                    WHERE FCC.ColorID={colorID} 
	                    AND FCM.IsBDS=2
	                    AND FCM.GroupConceptNo = '{conceptNo}'
	                    AND FCM.SubGroupID=1
                    ),
                    CC AS (
	                    SELECT FCM.*
	                    FROM FreeConceptChildColor FCC
	                    INNER JOIN FreeConceptMaster FCM ON FCM.ConceptID = FCC.ConceptID
	                    WHERE FCM.IsBDS=2
	                    AND FCM.GroupConceptNo = '{conceptNo}'
	                    AND FCM.SubGroupID<>1
                    ),
                    M AS (
	                    SELECT * FROM F
	                    UNION
	                    SELECT * FROM CC
                    ),
                    PB As(
						Select b.ConceptID, Sum(b.Pcs) Pcs, Sum(b.Qty) Qty
						From BatchMaster a
						Inner Join BatchItemRequirement b on b.BatchID = a.BatchID
						Where a.ColorID={colorID} And a.GroupConceptNo = '{conceptNo}'
						Group By b.ConceptID
					)
                    SELECT M.ConceptID, M.ItemMasterID, 
                    
                    ConceptOrSampleQtyKg = CASE WHEN FCM.SubGroupID = 1 THEN ISNULL(FCM.TotalQty,0) ELSE ISNULL(FCM.TotalQtyInKG,0) END,
					ConceptOrSampleQtyPcs = CASE WHEN FCM.SubGroupID <> 1 THEN ISNULL(FCM.TotalQty,0) ELSE 0 END,

					Qty = 0, --(CASE WHEN FCM.SubGroupID = 1 THEN ISNULL(FCM.TotalQty,0) ELSE ISNULL(FCM.TotalQtyInKG,0) END) - ISNULL(PB.Qty,0),
					Pcs = 0, --(CASE WHEN FCM.SubGroupID <> 1 THEN ISNULL(FCM.TotalQty,0) ELSE 0 END) - ISNULL(PB.Pcs,0),
					
					PlannedBatchQtyKg = ISNULL(PB.Qty,0),
					PlannedBatchQtyPcs = ISNULL(PB.Pcs,0),

                    KnittingType.TypeName KnittingType,Composition.SegmentValue FabricComposition,                    
                    Construction.SegmentValue FabricConstruction,Technical.TechnicalName,Gsm.SegmentValue FabricGsm, M.SubGroupID SubGroupID, 
                    SG.SubGroupName SubGroup, M.Length, M.Width, M.FUPartID, FU.PartName FUPartName,
                    ColorName = CASE WHEN M.SubGroupID=1 THEN Color.SegmentValue ELSE CollarColor.SegmentValue END
                    FROM M
                    LEFT JOIN KnittingMachineType KnittingType ON KnittingType.TypeID = M.KnittingTypeID
                    LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = M.ItemMasterID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = M.CompositionID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = M.ConstructionID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = M.GSMID
                    LEFT JOIN {DbNames.EPYSL}..ItemSubGroup SG ON SG.SubGroupID = M.SubGroupID
                    LEFT JOIN FabricTechnicalName Technical ON Technical.TechnicalNameId=M.TechnicalNameId
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = IM.Segment3ValueID
					LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue CollarColor ON CollarColor.SegmentValueID = IM.Segment5ValueID
                    LEFT JOIN FreeConceptMaster FCM ON FCM.ConceptID=M.ConceptID
                    LEFT JOIN {DbNames.EPYSL}..FabricUsedPart FU ON FU.FUPartID = M.FUPartID
                    LEFT JOIN PB on PB.ConceptID = M.ConceptID
                    WHERE (CASE WHEN FCM.SubGroupID = 1 THEN ISNULL(FCM.TotalQty,0) ELSE ISNULL(FCM.TotalQtyInKG,0) END) > ISNULL(PB.Qty,0);

                    -----K Production
                    ; WITH M AS(
                            SELECT KP.*
                            FROM KnittingProduction KP
							INNER JOIN FreeConceptMaster FC ON FC.ConceptID = KP.ConceptID
							WHERE FC.GroupConceptNo = '{conceptNo}' AND KP.InActive = 0
                    )
                    SELECT M.GRollID, M.KJobCardMasterID, M.ProductionDate, M.ConceptID, M.OperatorID, M.ShiftID, M.RollSeqNo, M.RollNo, M.RollQty, M.RollQtyPcs, M.ProductionGSM, M.ProductionWidth,
                    M.FirstRollCheck, M.FirstRollCheckBy, M.FirstRollCheckDate, M.FirstRollPass, M.SendforQC, M.SendQCDate, M.SendQCBy, M.QCComplete, M.QCCompleteDate, M.QCCompleteBy,
                    M.QCWidth, M.QCGSM, M.QCPass, M.QCPassQty, M.AddedBy, M.UpdatedBy, M.ProdComplete, M.ProdQty, M.Hole, M.Loop, M.SetOff, M.LycraOut, M.LycraDrop, M.OilSpot, M.Slub, M.FlyingDust,
                    M.MissingYarn, M.Knot, M.DropStitch, M.YarnContra, M.NeddleBreakage, M.Defected, M.WrongDesign, M.Patta, M.ShinkerMark, M.NeddleMark, M.EdgeMark, M.WheelFree, M.CountMix,
                    M.ThickAndThin, M.LineStar, M.QCOthers, M.Comment, M.CalculateValue, M.Grade, M.RollLength, M.Hold, M.QCBy, M.QCShiftID, M.BookingID, M.DateAdded, M.DateUpdated,
                    M.ExportOrderID, M.BuyerID, M.BuyerTeamID, M.ParentGRollID, M.InActive, M.InActiveBy, M.InActiveDate, M.InActiveReason, M.BatchID
                    FROM M
					Left Join BatchChild BC ON BC.GRollID = M.GRollID
					where BC.GRollID IS NULL;

                    ----Unit
                    ;SELECT CAST(UnitID AS VARCHAR) AS id, DisplayUnitDesc AS text
                    FROM {DbNames.EPYSL}..Unit;";
                }
            }
            else
            {
                sql = $@"
                    ;WITH M AS (
                            SELECT	*
                            FROM RecipeDefinitionMaster DM
	                        WHERE DM.RecipeID = {recipeID}
                        )
                        SELECT M.RecipeID, M.RecipeNo, M.RecipeDate, M.ConceptID, M.BookingID, M.ItemMasterID, M.ColorID, M.RecipeFor, M.BatchWeightKG, M.Remarks,
                        M.ExportOrderID, M.BuyerID, M.BuyerTeamID, RecFor.ValueName RecipeForName, COL.SegmentValue ColorName,FM.GroupConceptNo ConceptNo,M.CCColorID
                        FROM M
                        INNER JOIN {DbNames.EPYSL}..EntityTypeValue RecFor ON RecFor.ValueID = M.RecipeFor
                        INNER JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID = M.ColorID
                        INNER JOIN FreeConceptChildColor CC ON CC.CCColorID = M.CCColorID
	                    INNER JOIN FreeConceptMaster FM ON FM.ConceptID = CC.ConceptID;

                    -----BatchItemRequirement
                    WITH II AS (
                    SELECT * FROM RecipeDefinitionItemInfo WHERE RecipeID =  {recipeID}
                    )

                    SELECT II.RecipeItemInfoID, II.ConceptID, II.ItemMasterID,
                    Qty = 0, --(CASE WHEN M.SubGroupID != 1 THEN 0 ELSE II.Qty END),
                    Pcs = 0, --(CASE WHEN M.SubGroupID != 1 THEN II.Qty ELSE II.Pcs END),
                    ConceptOrSampleQty = ISNULL(KJ.KJobCardQty,0),
                    ProdQty = ISNULL(KJ.ProdQty,0),
                    ProdQtyPcs = ISNULL(KJ.ProdQtyPcs,0),
                    KnittingType.TypeName KnittingType,Composition.SegmentValue FabricComposition,
                    Construction.SegmentValue FabricConstruction,Technical.TechnicalName,Gsm.SegmentValue FabricGsm, M.SubGroupID SubGroupID, SG.SubGroupName SubGroup,
                    M.Length, M.Width, M.FUPartID, FU.PartName FUPartName,
                    ColorName = CASE WHEN M.SubGroupID=1 THEN Color.SegmentValue ELSE CollarColor.SegmentValue END
                    FROM II
                    LEFT JOIN FreeConceptMaster M ON M.ConceptID = II.ConceptID
                    LEFT JOIN KnittingMachineType KnittingType ON KnittingType.TypeID = M.KnittingTypeID
                    LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = M.ItemMasterID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = M.CompositionID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = M.ConstructionID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = M.GSMID
                    LEFT JOIN {DbNames.EPYSL}..ItemSubGroup SG ON SG.SubGroupID = M.SubGroupID
                    LEFT JOIN FabricTechnicalName Technical ON Technical.TechnicalNameId=M.TechnicalNameId
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = IM.Segment3ValueID
					LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue CollarColor ON CollarColor.SegmentValueID = IM.Segment5ValueID
                    LEFT JOIN KJobCardMaster KJ ON KJ.ConceptID=M.ConceptID
					LEFT JOIN {DbNames.EPYSL}..FabricUsedPart FU ON FU.FUPartID = M.FUPartID;

                  --Childs
                 ;WITH M AS (
                        SELECT * FROM RecipeDefinitionChild WHERE RecipeID = {recipeID}
                    )
                    SELECT  M.ProcessTime, M.Temperature TempIn,EV.ValueName FiberPart, ISV.SegmentValue ColorName,
					P.ValueName ProcessName,PL.ValueName ParticularsName, UU.DisplayUnitDesc Unit
					FROM M
					LEFT JOIN {DbNames.EPYSL}..ItemMaster I ON I.ItemMasterID=M.RawItemID
					LEFT JOIN {DbNames.EPYSL}..EntityTypeValue P ON P.ValueID = M.ProcessID
					LEFT JOIN {DbNames.EPYSL}..EntityTypeValue PL ON PL.ValueID = M.ParticularsID
					LEFT JOIN {DbNames.EPYSL}..ItemMaster R ON R.ItemMasterID = M.RawItemID
					INNER JOIN RecipeDefinitionDyeingInfo DI ON DI.RecipeDInfoID = M.RecipeDInfoID
					INNER JOIN {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID = DI.FiberPartID
					INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = DI.ColorID
                    LEFT JOIN {DbNames.EPYSL}..Unit UU ON UU.UnitID = M.UnitID
					group by M.ProcessTime, M.Temperature,
					P.ValueName ,PL.ValueName ,UU.DisplayUnitDesc ,EV.ValueName,ISV.SegmentValue
					;

                --Def Childs
				    SELECT C.RecipeChildID, C.RecipeID,C.ProcessID,C.Qty,C.UnitID,C.TempIn,C.TempOut,C.ParticularsID,C.RawItemID,I.ItemName,C.ProcessTime,
				    P.ValueName ProcessName,PL.ValueName ParticularsName,R.ItemName RawItemName, UU.DisplayUnitDesc Unit, C.IsPercentage,
				    C.RecipeDInfoID, C.Temperature, EV.ValueName FiberPart, ISV.SegmentValue ColorName
				    FROM RecipeDefinitionChild C
				    LEFT JOIN {DbNames.EPYSL}..ItemMaster I ON I.ItemMasterID=C.RawItemID
				    LEFT JOIN {DbNames.EPYSL}..EntityTypeValue P ON P.ValueID = C.ProcessID
				    LEFT JOIN {DbNames.EPYSL}..EntityTypeValue PL ON PL.ValueID = C.ParticularsID
				    LEFT JOIN {DbNames.EPYSL}..ItemMaster R ON R.ItemMasterID = C.RawItemID
				    LEFT JOIN {DbNames.EPYSL}..Unit UU ON UU.UnitID = C.UnitID
				    INNER JOIN RecipeDefinitionDyeingInfo DI ON DI.RecipeDInfoID = C.RecipeDInfoID
				    INNER JOIN {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID = DI.FiberPartID
				    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = DI.ColorID
				    WHERE C.RecipeID = {recipeID};
                   /*  -----Item Requirement Childs
                   ;WITH M AS (
                        SELECT * FROM RecipeDefinitionChild WHERE RecipeID = {recipeID}
                    )
                    SELECT M.RecipeChildID,M.ProcessID, M.ParticularsID, M.RawItemID, M.Qty, M.UnitID, M.TempOut, M.ProcessTime, M.Temperature TempIn,
					P.ValueName ProcessName,PL.ValueName ParticularsName,R.ItemName RawItemName, UU.DisplayUnitDesc Unit
					FROM M
					LEFT JOIN {DbNames.EPYSL}..ItemMaster I ON I.ItemMasterID=M.RawItemID
					LEFT JOIN {DbNames.EPYSL}..EntityTypeValue P ON P.ValueID = M.ProcessID
					LEFT JOIN {DbNames.EPYSL}..EntityTypeValue PL ON PL.ValueID = M.ParticularsID
					LEFT JOIN {DbNames.EPYSL}..ItemMaster R ON R.ItemMasterID = M.RawItemID
					LEFT JOIN {DbNames.EPYSL}..Unit UU ON UU.UnitID = M.UnitID;*/

                    -----childs
                    ;WITH M AS (
                        SELECT * FROM KnittingProduction WHERE BookingID = {bookingID}
                    )
                    SELECT GRollID, KJobCardMasterID, ProductionDate, ConceptID, OperatorID, ShiftID, RollSeqNo, RollNo, RollQty, RollQtyPcs, ProductionGSM, ProductionWidth,
                    FirstRollCheck, FirstRollCheckBy, FirstRollCheckDate, FirstRollPass, SendforQC, SendQCDate, SendQCBy, QCComplete, QCCompleteDate, QCCompleteBy,
                    QCWidth, QCGSM, QCPass, QCPassQty, AddedBy, UpdatedBy, ProdComplete, ProdQty, Hole, Loop, SetOff, LycraOut, LycraDrop, OilSpot, Slub, FlyingDust,
                    MissingYarn, Knot, DropStitch, YarnContra, NeddleBreakage, Defected, WrongDesign, Patta, ShinkerMark, NeddleMark, EdgeMark, WheelFree, CountMix,
                    ThickAndThin, LineStar, QCOthers, Comment, CalculateValue, Grade, RollLength, Hold, QCBy, QCShiftID, BookingID, DateAdded, DateUpdated,
                    ExportOrderID, BuyerID, BuyerTeamID, ParentGRollID, InActive, InActiveBy, InActiveDate, InActiveReason, BatchID
                    FROM M;

                    ----Unit
                    ;SELECT CAST(UnitID AS VARCHAR) AS id, DisplayUnitDesc AS text
                    FROM {DbNames.EPYSL}..Unit;";
            }

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                BatchMaster data = records.Read<BatchMaster>().FirstOrDefault();
                List<BatchItemRequirement> batchItemRequirements = records.Read<BatchItemRequirement>().ToList();
                data.BatchItemRequirements = batchItemRequirements.Where(x => x.SubGroupID == 1).ToList();
                data.BatchOtherItemRequirements = batchItemRequirements.Where(x => x.SubGroupID != 1).ToList();

                int recipeItemInfoID = 1;
                data.BatchItemRequirements.ForEach(x => x.RecipeItemInfoID = recipeItemInfoID++);
                data.BatchOtherItemRequirements.ForEach(x => x.RecipeItemInfoID = recipeItemInfoID++);

                if (isBDS == 0) // Concept
                {
                    data.BatchWiseRecipeChilds = records.Read<BatchWiseRecipeChild>().ToList();
                    data.DefChilds = records.Read<RecipeDefinitionChild>().ToList();
                }

                data.KnittingProductions = records.Read<KnittingProduction>().ToList();
                data.UnitList = records.Read<Select2OptionModel>().ToList();

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
        public async Task<BatchMaster> GetAsync(int id, string conceptNo, int bookingID)
        {
            var sql = $@"
                        ;WITH M AS (
                            SELECT	*
                            FROM BatchMaster BM WHERE BM.BatchID = {id}
                        )
                        SELECT M.BatchID, M.BatchNo, M.BatchDate, M.RecipeID, M.BookingID, M.ColorID, M.CCColorID, M.BatchWeightKG, M.BatchWeightPcs, M.Remarks,
                        M.ExportOrderID, M.BuyerID, M.BuyerTeamID, RDM.RecipeNo, RDM.RecipeDate, COL.SegmentValue ColorName, M.IsApproved,M.DMID, M.MachineLoading,
                        M.DyeingNozzleQty, M.DyeingMcCapacity, M.DMID, DM.DyeingMcslNo DMNo, FM.GroupConceptNo ConceptNo
                        FROM M
                        LEFT JOIN RecipeDefinitionMaster RDM ON RDM.RecipeID = M.RecipeID
                        INNER JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID = M.ColorID
						LEFT JOIN DyeingMachine DM ON DM.DMID = M.DMID
                        LEFT JOIN FreeConceptChildColor CC ON CC.CCColorID = M.CCColorID
	                    LEFT JOIN FreeConceptMaster FM ON FM.ConceptID = CC.ConceptID;

                        -----BatchItemRequirement
                        ;WITH II AS (
                            SELECT R.*, B.GroupConceptNo
							FROM BatchItemRequirement R
							INNER JOIN BatchMaster B ON B.BatchID = R.BatchID
							WHERE R.BatchID = {id}
                        ),
					    ItemSegment As
					    (
						    SELECT CAST(ISV.SegmentValueID As varchar) [id], ISV.SegmentValue [text], CAST(ISN.SegmentNameID As varchar) [desc]
						    FROM {DbNames.EPYSL}..ItemSegmentName ISN
						    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISN.SegmentNameID = ISV.SegmentNameID
						    WHERE ISNULL(ISV.SegmentValue, '') <> '' And SegmentValueID NOT IN(17725,5127,1616,1969,47221,2117,1616,1621,44382,44384,45969,46717,47220, 2155) --2155 for Ring Yarn Type which is not Needed by Supply Chain dept dated 11-02-2020
					    ),
                        PB As(
	                        Select b.ConceptID, Sum(b.Pcs) Pcs, Sum(b.Qty) Qty
	                        FROM BatchItemRequirement b
	                        INNER JOIN BatchItemRequirement c on c.ConceptID=b.ConceptID AND c.BatchID={id}
	                        WHERE b.BatchID!={id}
	                        Group By b.ConceptID
                        )
                        SELECT II.BItemReqID, II.BatchID, II.RecipeItemInfoID, II.ItemMasterID,
                        
                        ConceptOrSampleQtyKg = CASE WHEN FCM.SubGroupID = 1 THEN FORMAT(ISNULL(FCM.TotalQty,0),'N2') ELSE FORMAT(ISNULL(FCM.TotalQtyInKG,0),'N2') END,
					    ConceptOrSampleQtyPcs = CASE WHEN FCM.SubGroupID <> 1 THEN ISNULL(FCM.TotalQty,0) ELSE 0 END,

						Qty = II.Qty,
						Pcs = II.Pcs,
					
						PlannedBatchQtyKg = ISNULL(PB.Qty,0),
						PlannedBatchQtyPcs = ISNULL(PB.Pcs,0),

                        II.IsFloorRequistion, KnittingType.TypeName KnittingType,
                        Composition.text FabricComposition, Construction.text FabricConstruction,Technical.TechnicalName,Gsm.text FabricGsm, M.SubGroupID SubGroupID,
                        SG.SubGroupName SubGroup, M.Length, M.Width, M.ConceptID, M.FUPartID, FU.PartName FUPartName,
                        ColorName = CASE WHEN M.SubGroupID=1 THEN Color.SegmentValue ELSE CollarColor.SegmentValue END
					    FROM II
					    LEFT JOIN FreeConceptMaster M ON M.ConceptID=II.ConceptID
                        LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = M.ItemMasterID
					    LEFT JOIN KnittingMachineType KnittingType ON KnittingType.TypeID=M.KnittingTypeID
					    LEFT JOIN ItemSegment Composition ON Composition.id=M.CompositionID
					    LEFT JOIN ItemSegment Construction ON Construction.id=M.ConstructionID
					    LEFT JOIN ItemSegment Gsm ON Gsm.id=M.GSMID
					    LEFT JOIN {DbNames.EPYSL}..ItemSubGroup SG ON SG.SubGroupID = M.SubGroupID
                        LEFT JOIN FabricTechnicalName Technical ON Technical.TechnicalNameId=M.TechnicalNameId
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = IM.Segment3ValueID
						LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue CollarColor ON CollarColor.SegmentValueID = IM.Segment5ValueID
                        LEFT JOIN FreeConceptMaster FCM ON FCM.ConceptID=M.ConceptID
                        LEFT JOIN {DbNames.EPYSL}..FabricUsedPart FU ON FU.FUPartID = M.FUPartID
                        LEFT JOIN PB on PB.ConceptID = M.ConceptID;

                        ----Batch child
                        
                        ;SELECT C.BChildID, C.BItemReqID, C.BatchID, C.GRollID, C.ItemMasterID, C.RollQty, C.RollQtyPcs, KP.RollNo
                        FROM BatchChild C
                        INNER JOIN KnittingProduction KP ON KP.GRollID = C.GRollID
                        WHERE C.BatchID = {id};

                        
                        

                        -----BatchWiseRecipeChild
                        ;WITH M AS (
                            SELECT BC.*, RD.RecipeDInfoID, RD.Temperature FROM BatchWiseRecipeChild BC
							INNER JOIN RecipeDefinitionChild RD ON RD.RecipeChildID = BC.RecipeChildID
							WHERE BC.BatchID = {id}
                        )
                        SELECT DI.RecipeDInfoID, M.TempIn, M.BatchID, M.ProcessTime, EV.ValueName FiberPart, ISV.SegmentValue ColorName
					    FROM M
						INNER JOIN RecipeDefinitionDyeingInfo DI ON DI.RecipeDInfoID = M.RecipeDInfoID
						INNER JOIN {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID = DI.FiberPartID
						INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = DI.ColorID
						GROUP BY DI.RecipeDInfoID, M.TempIn, M.BatchID, M.ProcessTime, EV.ValueName, ISV.SegmentValue;

                        ----Def Childs
                        SELECT B.BRecipeChildID, B.BatchID, B.RecipeChildID, B.ProcessID, B.ParticularsID, B.RawItemID, B.Qty, B.UnitID, B.TempIn,
						B.TempOut, B.ProcessTime, C.RecipeID,I.ItemName, P.ValueName ProcessName,PL.ValueName ParticularsName,R.ItemName RawItemName,
						UU.DisplayUnitDesc Unit, C.IsPercentage, C.RecipeDInfoID, C.Temperature, EV.ValueName FiberPart, ISV.SegmentValue ColorName
						FROM BatchWiseRecipeChild B
						INNER JOIN RecipeDefinitionChild C ON C.RecipeChildID = B.RecipeChildID
						LEFT JOIN {DbNames.EPYSL}..ItemMaster I ON I.ItemMasterID=C.RawItemID
						LEFT JOIN {DbNames.EPYSL}..EntityTypeValue P ON P.ValueID = C.ProcessID
						LEFT JOIN {DbNames.EPYSL}..EntityTypeValue PL ON PL.ValueID = C.ParticularsID
						LEFT JOIN {DbNames.EPYSL}..ItemMaster R ON R.ItemMasterID = C.RawItemID
						LEFT JOIN {DbNames.EPYSL}..Unit UU ON UU.UnitID = C.UnitID
						INNER JOIN RecipeDefinitionDyeingInfo DI ON DI.RecipeDInfoID = C.RecipeDInfoID
						INNER JOIN {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID = DI.FiberPartID
						INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = DI.ColorID
						WHERE B.BatchID = {id};

                        ----Knitting Production
                        ;WITH M AS(
                            SELECT KP.* FROM KnittingProduction KP
							INNER JOIN FreeConceptMaster FC ON FC.ConceptID = KP.ConceptID
							WHERE FC.GroupConceptNo = '{conceptNo}' AND KP.InActive = 0
                        )
                        SELECT M.GRollID, M.KJobCardMasterID, M.ProductionDate, M.ConceptID, M.OperatorID, M.ShiftID, M.RollSeqNo, M.RollNo, M.RollQty, M.RollQtyPcs, M.ProductionGSM, M.ProductionWidth,
                        M.FirstRollCheck, M.FirstRollCheckBy, M.FirstRollCheckDate, M.FirstRollPass, M.SendforQC, M.SendQCDate, M.SendQCBy, M.QCComplete, M.QCCompleteDate, M.QCCompleteBy,
                        M.QCWidth, M.QCGSM, M.QCPass, M.QCPassQty, M.AddedBy, M.UpdatedBy, M.ProdComplete, M.ProdQty, M.Hole, M.Loop, M.SetOff, M.LycraOut, M.LycraDrop, M.OilSpot, M.Slub, M.FlyingDust,
                        M.MissingYarn, M.Knot, M.DropStitch, M.YarnContra, M.NeddleBreakage, M.Defected, M.WrongDesign, M.Patta, M.ShinkerMark, M.NeddleMark, M.EdgeMark, M.WheelFree, M.CountMix,
                        M.ThickAndThin, M.LineStar, M.QCOthers, M.Comment, M.CalculateValue, M.Grade, M.RollLength, M.Hold, M.QCBy, M.QCShiftID, M.BookingID, M.DateAdded, M.DateUpdated,
                        M.ExportOrderID, M.BuyerID, M.BuyerTeamID, M.ParentGRollID, M.InActive, M.InActiveBy, M.InActiveDate, M.InActiveReason, M.BatchID
                        FROM M
					    Left Join BatchChild BC ON BC.GRollID = M.GRollID
					    where BC.GRollID IS NULL;

                        --Unit
                        ;SELECT CAST(UnitID AS VARCHAR) AS id, DisplayUnitDesc AS text
                        FROM {DbNames.EPYSL}..Unit;
                        ";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                BatchMaster data = records.Read<BatchMaster>().FirstOrDefault();
                Guard.Against.NullObject(data);

                List<BatchItemRequirement> batchItemRequirements = records.Read<BatchItemRequirement>().ToList();
                List<BatchChild> batchChilds = records.Read<BatchChild>().ToList();
                batchItemRequirements.ForEach(x => x.BatchChilds = batchChilds.FindAll(c => c.BItemReqID == x.BItemReqID));

                data.BatchItemRequirements = batchItemRequirements.Where(x => x.SubGroupID == 1).ToList();
                data.BatchOtherItemRequirements = batchItemRequirements.Where(x => x.SubGroupID != 1).ToList();

                data.BatchWiseRecipeChilds = records.Read<BatchWiseRecipeChild>().ToList();
                data.DefChilds = records.Read<RecipeDefinitionChild>().ToList();
                data.KnittingProductions = records.Read<KnittingProduction>().ToList();
                data.UnitList = records.Read<Select2OptionModel>().ToList();

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

        public async Task<BatchMaster> GetAllAsync(int id)
        {
            var sql = $@"
                SELECT * FROM BatchMaster WHERE BatchID = {id};

                Select * From BatchItemRequirement Where BatchID = {id};

                Select * From BatchChild Where BatchID = {id};

                Select * From BatchWiseRecipeChild Where BatchID = {id};";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                BatchMaster data = await records.ReadFirstOrDefaultAsync<BatchMaster>();
                Guard.Against.NullObject(data);

                data.BatchItemRequirements = records.Read<BatchItemRequirement>().ToList();
                List<BatchChild> batchChilds = records.Read<BatchChild>().ToList();
                data.BatchItemRequirements.ForEach(x => x.BatchChilds = batchChilds.FindAll(c => c.BItemReqID == x.BItemReqID));
                data.BatchWiseRecipeChilds = records.Read<BatchWiseRecipeChild>().ToList();

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

        public async Task<BatchItemRequirement> GetBatchItemRequirementAsync(int id)
        {
            var query = $@"Select * from BatchItemRequirement Where BItemReqID = {id}";
            var record = await _service.GetFirstOrDefaultAsync<BatchItemRequirement>(query);
            Guard.Against.NullObject(record);
            return record;
        }
        public async Task<List<BatchItemRequirement>> GetOtherItems(PaginationInfo paginationInfo, string conceptIds, int colorId, string groupConceptNo)
        {
            if (colorId == 0 || groupConceptNo.IsNullOrEmpty()) return new List<BatchItemRequirement>();
            string conceptIdCon = "";
            if (conceptIds != "-" && !conceptIds.IsNullOrEmpty())
            {
                conceptIdCon = $@" AND FCM.ConceptID NOT IN ({conceptIds})";
            }

            paginationInfo.OrderBy = string.IsNullOrEmpty(paginationInfo.OrderBy) ? "ORDER BY ConceptID DESC" : paginationInfo.OrderBy;

            var sql = $@"
                    WITH M AS (
	                    SELECT FCM.*
	                    FROM FreeConceptChildColor FCC
	                    INNER JOIN FreeConceptMaster FCM ON FCM.ConceptID = FCC.ConceptID
	                    WHERE FCC.ColorID={colorId} 
	                    AND FCM.GroupConceptNo = '{groupConceptNo}'
	                    AND FCM.SubGroupID=1
                        {conceptIdCon}
                    )
                    SELECT M.ConceptID, M.ItemMasterID, ConceptOrSampleQty = ISNULL(KJ.KJobCardQty,0), ProdQty = ISNULL(KJ.ProdQty,0),
                    ProdQtyPcs = ISNULL(KJ.ProdQtyPcs,0), KnittingType.TypeName KnittingType,Composition.SegmentValue FabricComposition,
                    Construction.SegmentValue FabricConstruction,Technical.TechnicalName,Gsm.SegmentValue FabricGsm, M.SubGroupID SubGroupID, 
                    SG.SubGroupName SubGroup, M.Length, M.Width, M.FUPartID, FU.PartName FUPartName
                    FROM M
                    LEFT JOIN KnittingMachineType KnittingType ON KnittingType.TypeID = M.KnittingTypeID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = M.CompositionID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = M.ConstructionID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = M.GSMID
                    LEFT JOIN {DbNames.EPYSL}..ItemSubGroup SG ON SG.SubGroupID = M.SubGroupID
                    LEFT JOIN FabricTechnicalName Technical ON Technical.TechnicalNameId=M.TechnicalNameId
                    LEFT JOIN KJobCardMaster KJ ON KJ.ConceptID=M.ConceptID
                    LEFT JOIN {DbNames.EPYSL}..FabricUsedPart FU ON FU.FUPartID = M.FUPartID
                    {paginationInfo.FilterBy}
                    {paginationInfo.OrderBy}
                    {paginationInfo.PageBy}";

            var record = await _service.GetDataAsync<BatchItemRequirement>(sql);
            Guard.Against.NullObject(record);
            return record;
        }
        public async Task SaveAsync(BatchMaster entity)
        {
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                await _connectionGmt.OpenAsync();
                transactionGmt = _connectionGmt.BeginTransaction();

                switch (entity.EntityState)
                {
                    case EntityState.Added:
                        entity = await AddAsync(entity);
                        break;

                    case EntityState.Modified:
                        await UpdateAsync(entity);
                        break;

                    default:
                        break;
                }

                List<BatchChild> batchChilds = new List<BatchChild>();
                entity.BatchItemRequirements.ForEach(x => batchChilds.AddRange(x.BatchChilds));

                await _service.SaveSingleAsync(entity, transaction);
                await _service.SaveAsync(entity.BatchItemRequirements, transaction);
                await _service.SaveAsync(batchChilds, transaction);
                await _service.SaveAsync(entity.BatchWiseRecipeChilds, transaction);

                transaction.Commit();
                transactionGmt.Commit();

                //#region Update TNA
                //if (entity.BatchID > 0)
                //{
                //    await UpdateBDSTNA_BatchPreparationPlanAsync(entity.BatchID);
                //}
                //#endregion
            }
            catch (DbEntityValidationException ex)
            {
                var fex = new FormattedDbEntityValidationException(ex);
                throw new Exception(fex.Message);
            }
            catch(Exception ex)
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
        public async Task SaveAsyncRecipeCopy(BatchMaster entity)
        {
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                await _connectionGmt.OpenAsync();
                transactionGmt = _connectionGmt.BeginTransaction();

                switch (entity.EntityState)
                {
                    case EntityState.Added:
                        entity = await AddAsync(entity);
                        break;

                    case EntityState.Modified:
                        await UpdateAsync(entity);
                        break;

                    default:
                        break;
                }

                await _service.SaveSingleAsync(entity, transaction);

                transaction.Commit();
                transactionGmt.Commit();

                //#region Update TNA
                //if (entity.BatchID > 0)
                //{
                //    await UpdateBDSTNA_BatchPreparationPlanAsync(entity.BatchID);
                //}
                //#endregion
            }
            catch (DbEntityValidationException ex)
            {
                var fex = new FormattedDbEntityValidationException(ex);
                throw new Exception(fex.Message);
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
        public async Task UpdateBDSTNA_BatchPreparationPlanAsync(int BatchID)
        {
            await _service.ExecuteAsync("spUpdateBDSTNA_BatchPreparationPlan", new { BatchID = BatchID }, 30, CommandType.StoredProcedure);
        }

        #region Helpers

        private async Task<BatchMaster> AddAsync(BatchMaster entity)
        {
            entity.BatchID = await _service.GetMaxIdAsync(TableNames.BATCH_MASTER);
            int paddingValue = 3;
            
            int maxNo = await _service.GetMaxNoAsync(TableNames.BATCH_MASTER, "BatchNo", entity.SLNo, entity.SLNo.Length + paddingValue, _connectionGmt);
            //entity.BatchNo = entity.SLNo + maxNo.ToString().PadLeft(paddingValue, '0');

            var maxReqChildId = await _service.GetMaxIdAsync(TableNames.BATCH_ITEM_REQUIREMENT, entity.BatchItemRequirements.Count);
            var maxRecipeChildId = await _service.GetMaxIdAsync(TableNames.BATCH_WISE_RECIPE_CHILD, entity.BatchWiseRecipeChilds.Count);
            var maxChildId = await _service.GetMaxIdAsync(TableNames.BATCH_CHILD, entity.BatchItemRequirements.Sum(x => x.BatchChilds.Count()));

            foreach (var item in entity.BatchItemRequirements)
            {
                item.BItemReqID = maxReqChildId++;
                item.BatchID = entity.BatchID;

                foreach (var child in item.BatchChilds)
                {
                    child.BChildID = maxChildId++;
                    child.BatchID = entity.BatchID;
                    child.BItemReqID = item.BItemReqID;
                }
            }

            foreach (var item in entity.BatchWiseRecipeChilds)
            {
                item.BRecipeChildID = maxRecipeChildId++;
                item.BatchID = entity.BatchID;
            }

            return entity;
        }

        private async Task UpdateAsync(BatchMaster entity)
        {
            var maxReqChildId = await _service.GetMaxIdAsync(TableNames.BATCH_ITEM_REQUIREMENT, entity.BatchItemRequirements.Where(x => x.EntityState == EntityState.Added).Count());
            var maxRecipeChildId = await _service.GetMaxIdAsync(TableNames.BATCH_WISE_RECIPE_CHILD, entity.BatchWiseRecipeChilds.Where(x => x.EntityState == EntityState.Added).Count());
            var maxChildId = await _service.GetMaxIdAsync(TableNames.BATCH_CHILD, entity.BatchItemRequirements.Sum(x => x.BatchChilds.Where(y => y.EntityState == EntityState.Added).Count()));

            foreach (var item in entity.BatchItemRequirements)
            {
                foreach (var child in item.BatchChilds)
                {
                    switch (child.EntityState)
                    {
                        case EntityState.Added:
                            child.BChildID = maxChildId++;
                            child.BatchID = entity.BatchID;
                            child.BItemReqID = item.BItemReqID;
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
                        item.BItemReqID = maxReqChildId++;
                        item.BatchID = entity.BatchID;
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

            foreach (var item in entity.BatchWiseRecipeChilds)
            {
                switch (item.EntityState)
                {
                    case EntityState.Added:
                        item.BRecipeChildID = maxRecipeChildId++;
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
        }

        #endregion Helpers
    }
}