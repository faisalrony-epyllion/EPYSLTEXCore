using Dapper;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Application.Interfaces.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Yarn;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;

namespace EPYSLTEXCore.Application.Services.Inventory
{
    public class YDBatchService : IYDBatchService
    {
        private readonly IDapperCRUDService<YDBatchMaster> _service;
        private readonly SqlConnection _connection;
        private readonly SqlConnection _connectionGmt;
        public YDBatchService(IDapperCRUDService<YDBatchMaster> service)
        {
            _service = service;
            _service.Connection = service.GetConnection(AppConstants.GMT_CONNECTION);
            _connectionGmt = service.Connection;

            _service.Connection = service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _connection = service.Connection;
        }

        public async Task<List<YDBatchMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy;

            var sql = string.Empty;
            if (status == Status.Pending)
            {
                sql += $@"WITH BDS AS (
	                SELECT YDBC.YDBookingMasterID, YDB.YDBookingNo, FCM.ConceptID, FCM.BookingID, FCM.Remarks, FCM.GroupConceptNo, YDBC.ColorID, FCM.IsBDS, FCM.TotalQty,
                    FCM.ExportOrderID,FCM.BuyerID,FCM.BuyerTeamID
                    FROM {TableNames.YDBookingChild} YDBC 
					INNER JOIN {TableNames.YD_BOOKING_MASTER} YDB ON YDB.YDBookingMasterID = YDBC.YDBookingMasterID
					LEFT JOIN {TableNames.RND_FREE_CONCEPT_MR_CHILD} FCMRC ON FCMRC.FCMRChildID = YDBC.FCMRChildID --OR FCMRC.YBChildItemID = YDBC.YBChildItemID
					LEFT JOIN {TableNames.RND_FREE_CONCEPT_MR_MASTER} FCMR ON FCMR.FCMRMasterID = FCMRC.FCMRMasterID
					LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = FCMR.ConceptID
					LEFT JOIN {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} FCC ON FCC.ConceptID = FCM.ConceptID
                    LEFT JOIN {TableNames.Knitting_Plan_Master} KPM ON KPM.ConceptID = FCM.ConceptID
                    LEFT JOIN {TableNames.YD_BATCH_ITEM_REQUIREMENT} BIR ON BIR.ConceptID = FCM.ConceptID
                    LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster SBM ON SBM.BookingID = FCM.BookingID
                    WHERE YDB.IsAcknowledge = 1 AND YDB.IsYDBNoGenerated = 1 AND FCM.IsBDS <> 0 AND FCM.IsBDS <> 2 AND ISNULL(SBM.SampleID,0) <> 13 --AND KPM.IsConfirm=1
                    GROUP BY YDBC.YDBookingMasterID, YDB.YDBookingNo, FCM.ConceptID, FCM.BookingID,  FCM.Remarks, FCM.GroupConceptNo, YDBC.ColorID, FCM.IsBDS, FCM.TotalQty,
                    FCM.ExportOrderID,FCM.BuyerID,FCM.BuyerTeamID, YDBC.BookingQty
                    Having YDBC.BookingQty > ISNULL(Sum(Case when FCM.SubGroupID = 1 Then BIR.Qty Else BIR.Pcs End) ,0)
                ),
				Blk AS (
	                SELECT YDBC.YDBookingMasterID, YDB.YDBookingNo, FCM.ConceptID, FCM.BookingID, FCM.Remarks, FCM.GroupConceptNo, YDBC.ColorID, FCM.IsBDS, FCM.TotalQty,
                    FCM.ExportOrderID,FCM.BuyerID,FCM.BuyerTeamID
                    FROM {TableNames.YDBookingChild} YDBC 
					INNER JOIN {TableNames.YD_BOOKING_MASTER} YDB ON YDB.YDBookingMasterID = YDBC.YDBookingMasterID
					INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_CHILD} FCMRC ON FCMRC.YBChildItemID = YDBC.YBChildItemID
					INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_MASTER} FCMR ON FCMR.FCMRMasterID = FCMRC.FCMRMasterID
					INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = FCMR.ConceptID
					LEFT JOIN {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} FCC ON FCC.ConceptID = FCM.ConceptID
                    LEFT JOIN {TableNames.Knitting_Plan_Master} KPM ON KPM.ConceptID = FCM.ConceptID
                    LEFT JOIN {TableNames.YD_BATCH_ITEM_REQUIREMENT} BIR ON BIR.ConceptID = FCM.ConceptID
                    LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster SBM ON SBM.BookingID = FCM.BookingID
                    WHERE YDB.IsAcknowledge = 1 AND YDB.IsYDBNoGenerated = 1 AND FCM.IsBDS = 2 AND YDBC.YBChildItemID <> 0 AND ISNULL(SBM.SampleID,0) <> 13 --AND KPM.IsConfirm=1
                    GROUP BY YDBC.YDBookingMasterID, YDB.YDBookingNo, FCM.ConceptID, FCM.BookingID,  FCM.Remarks, FCM.GroupConceptNo, YDBC.ColorID, FCM.IsBDS, FCM.TotalQty,
                    FCM.ExportOrderID,FCM.BuyerID,FCM.BuyerTeamID, YDBC.BookingQty
                    Having YDBC.BookingQty > ISNULL(Sum(Case when FCM.SubGroupID = 1 Then BIR.Qty Else BIR.Pcs End) ,0)
                ),
                M AS (

                    SELECT	YDBC.YDBookingMasterID, YDB.YDBookingNo, DM.YDRecipeID, DM.YDRecipeNo, DM.RecipeDate, DM.BookingID,
	                ColorID = YDBC.ColorId,--CASE WHEN DM.ColorID > 0 THEN DM.ColorID ELSE FCC.ColorID END,
	                DM.RecipeFor, DM.BatchWeightKG, DM.Remarks,
	                DM.ExportOrderID, DM.BuyerID, DM.BuyerTeamID, 
	                FCM.ConceptID, FCM.GroupConceptNo ConceptNo, FCM.IsBDS
	                FROM {TableNames.YDBookingChild} YDBC 
					INNER JOIN {TableNames.YD_BOOKING_MASTER} YDB ON YDB.YDBookingMasterID = YDBC.YDBookingMasterID
					LEFT JOIN {TableNames.RND_FREE_CONCEPT_MR_CHILD} FCMRC ON FCMRC.FCMRChildID = YDBC.FCMRChildID
					LEFT JOIN {TableNames.RND_FREE_CONCEPT_MR_MASTER} FCMR ON FCMR.FCMRMasterID = FCMRC.FCMRMasterID
					LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = FCMR.ConceptID
					LEFT JOIN {TableNames.RND_KNITTING_PRODUCTION} KP ON FCM.ConceptID = KP.ConceptID
					LEFT JOIN {TableNames.YD_BATCH_CHILD} BC ON BC.GRollID = KP.GRollID
	                INNER JOIN {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} FCC ON FCC.ConceptID = FCM.ConceptID
	                LEFT JOIN {TableNames.YD_RECIPE_REQ_CHILD} RC ON RC.ConceptID = FCM.ConceptID
	                LEFT JOIN {TableNames.YD_RECIPE_DEFINITION_MASTER} DM ON DM.YDRecipeReqMasterID = RC.YDRecipeReqMasterID
	                LEFT JOIN {TableNames.YD_BATCH_MASTER} BM ON BM.CCColorID = FCC.CCColorID
					--LEFT Join BatchChild BC ON BC.BatchID = BM.BatchID
                    LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster SBM ON SBM.BookingID = FCM.BookingID
	                WHERE YDB.IsAcknowledge = 1 AND YDB.IsYDBNoGenerated = 1 AND --BC.GRollID IS NULL AND InActive=0 AND
					FCM.IsBDS = 0 AND 
					--BM.BatchID IS NULL AND 
					ISNULL(DM.IsArchive,0) = 0 AND 
					ISNULL(SBM.SampleID,0) <> 13
                    --AND DM.IsApproved = 1 --AND FCM.ConceptNo=FCM.GroupConceptNo
	                GROUP BY YDBC.YDBookingMasterID, YDB.YDBookingNo, DM.YDRecipeID, DM.YDRecipeNo, DM.RecipeDate, DM.BookingID, DM.RecipeFor, DM.BatchWeightKG, DM.Remarks,
	                DM.ExportOrderID, DM.BuyerID, DM.BuyerTeamID, FCM.ConceptID, FCM.GroupConceptNo, FCM.IsBDS, YDBC.ColorID
                ),
                CONCEPT_BDS AS (
                    SELECT YDBookingMasterID, YDBookingNo, YDRecipeID = ISNULL(M.YDRecipeID,0), M.YDRecipeNo, M.RecipeDate, M.ConceptID, M.BookingID, M.ColorID, M.RecipeFor, M.BatchWeightKG, M.Remarks,
	                M.ExportOrderID, M.BuyerID, M.BuyerTeamID, '' RecipeForName, M.ConceptNo, M.IsBDS
	                FROM M
	                UNION
	                SELECT YDBookingMasterID, YDBookingNo, 0 RecipeID, '' YDRecipeNo, null RecipeDate, BDS.ConceptID, BDS.BookingID, BDS.ColorID, 0 RecipeFor, 0 BatchWeightKG, BDS.Remarks,
	                ExportOrderID, BuyerID, BuyerTeamID, '' RecipeForName, BDS.GroupConceptNo ConceptNo, BDS.IsBDS
	                FROM BDS
					UNION
	                SELECT YDBookingMasterID, YDBookingNo, 0 RecipeID, '' YDRecipeNo, null RecipeDate, Blk.ConceptID, Blk.BookingID, Blk.ColorID, 0 RecipeFor, 0 BatchWeightKG, Blk.Remarks,
	                ExportOrderID, BuyerID, BuyerTeamID, '' RecipeForName, Blk.GroupConceptNo ConceptNo, Blk.IsBDS
	                FROM Blk
                ), FFF As (
                    SELECT B.YDBookingMasterID, YDBookingNo, B.YDRecipeID, B.YDRecipeNo, B.RecipeDate, B.BookingID, B.ColorID, B.RecipeFor, B.BatchWeightKG, B.Remarks,
                    B.ExportOrderID, B.BuyerID, B.BuyerTeamID, RecFor.ValueName RecipeForName, COL.SegmentValue ColorName, B.ConceptNo,
                    BuyerName = CASE WHEN B.BuyerID = 0 THEN 'R&D' ELSE CTO.ShortName END, 
					BuyerTeamName = CASE WHEN B.BuyerTeamID = 0 THEN 'R&D' ELSE CCT.TeamName END,
				    B.IsBDS
                    FROM CONCEPT_BDS B
                    LEFT JOIN {DbNames.EPYSL}..EntityTypeValue RecFor ON RecFor.ValueID = B.RecipeFor
                    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID = B.ColorID
                    LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = B.BuyerID
                    LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = B.BuyerTeamID
                    GROUP BY B.YDBookingMasterID, YDBookingNo, B.YDRecipeID, B.YDRecipeNo, B.RecipeDate, B.BookingID, B.ColorID, B.RecipeFor, B.BatchWeightKG, B.Remarks,
                    B.ExportOrderID, B.BuyerID, B.BuyerTeamID, RecFor.ValueName, COL.SegmentValue, B.ConceptNo,CTO.ShortName, CCT.TeamName, B.IsBDS
                )
                Select *, Count(*) Over() TotalRows From FFF ";
                if (orderBy.NullOrEmpty()) orderBy = "ORDER BY YDBookingMasterID DESC";
            }
            else if (status == Status.Completed)
            {
                sql += $@";WITH M AS (
                            SELECT	YDBM.YDBookingMasterID, YDBM.YDBookingNo, BM.YDRecipeID, BM.ColorID, BM.CCColorID, BM.BuyerID, BM.BuyerTeamID,
							BM.ExportOrderID, BM.YDBatchID, BM.YDBatchNo, BM.YDBatchDate, BM.BatchWeightKG, BM.BatchWeightPcs, BM.Remarks
                            FROM {TableNames.YD_BATCH_MASTER} BM 
							INNER JOIN {TableNames.YD_BOOKING_MASTER} YDBM ON YDBM.YDBookingMasterID = BM.YDBookingMasterID
							WHERE BM.IsApproved = 1
                        ), FFF As (
                                SELECT M.YDBookingMasterID, M.YDBookingNo, M.YDBatchID, M.YDBatchNo, M.YDBatchDate, M.YDRecipeID,M.ColorID, M.BatchWeightKG, M.BatchWeightPcs, M.Remarks,
                                M.ExportOrderID, M.BuyerID, M.BuyerTeamID, RDM.YDRecipeNo, RDM.RecipeDate, COL.SegmentValue ColorName, FM.GroupConceptNo ConceptNo,
                                BuyerName = CASE WHEN M.BuyerID = 0 THEN 'R&D' ELSE CTO.ShortName END, 
                                BuyerTeamName = CASE WHEN M.BuyerTeamID = 0 THEN 'R&D' ELSE CCT.TeamName END,
                                Count(*) Over() TotalRows
                                FROM M
                                LEFT JOIN {TableNames.YD_RECIPE_DEFINITION_MASTER} RDM ON RDM.YDRecipeID = M.YDRecipeID
                                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID = M.ColorID
						        INNER JOIN {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} CC ON CC.CCColorID = M.CCColorID
	                            INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FM ON FM.ConceptID = CC.ConceptID
                                LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = M.BuyerID
                                LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = M.BuyerTeamID
                        )
                        Select * From FFF ";

                if (orderBy.NullOrEmpty()) orderBy = "ORDER BY YDBatchID DESC";
            }
            else if (status == Status.Approved)
            {
                sql += $@";WITH M AS (
                            SELECT	*
                            FROM {TableNames.YD_BATCH_MASTER} BM WHERE BM.IsApproved = 1
                        ), FFF AS (
                            SELECT M.YDBatchID, M.YDBatchNo, M.YDBatchDate, M.YDRecipeID, M.ColorID, M.BatchWeightKG, M.BatchWeightPcs, M.Remarks,
                            M.ExportOrderID, M.BuyerID, M.BuyerTeamID, RDM.YDRecipeNo, RDM.RecipeDate, COL.SegmentValue ColorName, FM.GroupConceptNo ConceptNo,
                            BuyerName = CASE WHEN M.BuyerID = 0 THEN 'R&D' ELSE CTO.ShortName END, 
                            BuyerTeamName = CASE WHEN M.BuyerTeamID = 0 THEN 'R&D' ELSE CCT.TeamName END,
                            Count(*) Over() TotalRows
                            FROM M
                            INNER JOIN {TableNames.YD_RECIPE_DEFINITION_MASTER} RDM ON RDM.YDRecipeID = M.YDRecipeID
                            INNER JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID = M.ColorID
						    INNER JOIN {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} CC ON CC.CCColorID = M.CCColorID
	                        INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FM ON FM.ConceptID = CC.ConceptID
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

            return await _service.GetDataAsync<YDBatchMaster>(sql);
        }

        public async Task<YDBatchMaster> GetNewAsync(int yDBookingMasterID, int recipeID, string conceptNo, int bookingID, int isBDS, int colorID)
        {
            string sql = "";
            if (!string.IsNullOrEmpty(conceptNo))
            {
                if (isBDS == 0) //Concept
                {
                    sql = $@"SELECT YDBC.YDBookingMasterID, DM.YDRecipeID, DM.YDRecipeNo, DM.RecipeDate, FM.ConceptID, FM.BookingID, YDBC.ItemMasterID, YDBC.ColorID, FCC.CCColorID, DM.RecipeFor, DM.BatchWeightKG, DM.Remarks,
                        FM.ExportOrderID, FM.BuyerID, FM.BuyerTeamID, RecFor.ValueName RecipeForName, Color.SegmentValue ColorName, FM.GroupConceptNo ConceptNo, FM.GroupConceptNo SLNo
                        FROM {TableNames.YDBookingChild} YDBC 
						INNER JOIN {TableNames.YD_BOOKING_MASTER} YDB ON YDB.YDBookingMasterID = YDBC.YDBookingMasterID
						LEFT JOIN {TableNames.RND_FREE_CONCEPT_MR_CHILD} FCMRC ON FCMRC.FCMRChildID = YDBC.FCMRChildID
						LEFT JOIN {TableNames.FreeConceptMRMaster} FCMR ON FCMR.FCMRMasterID = FCMRC.FCMRMasterID
						LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FM ON FM.ConceptID = FCMR.ConceptID
						LEFT Join {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} FCC ON FCC.ConceptID = FM.ConceptID
						LEFT JOIN {TableNames.YD_RECIPE_DEFINITION_MASTER} DM ON DM.ConceptID = FM.ConceptID
	                    LEFT Join {TableNames.YD_RECIPE_REQ_CHILD} RC ON RC.YDRecipeReqMasterID = DM.YDRecipeReqMasterID
                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue RecFor ON RecFor.ValueID = DM.RecipeFor
						LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = YDBC.ColorId
	                    WHERE YDBC.ColorID = {colorID} AND YDBC.YDBookingMasterID = {yDBookingMasterID};

                        -----BatchItemRequirement
                        With PB As(
		                    Select b.YDBookingChildID, Sum(b.Pcs) Pcs, Sum(b.Qty) Qty
		                    FROM {TableNames.YD_BATCH_MASTER} a
		                    Inner JOIN {TableNames.YD_BATCH_ITEM_REQUIREMENT} b on b.YDBatchID = a.YDBatchID
		                    Where a.ColorID = {colorID} AND a.YDBookingMasterID = {yDBookingMasterID}
		                    Group By b.YDBookingChildID
	                    )
                        SELECT YDBC.YDBookingChildID, II.YDRecipeItemInfoID, M.ConceptID, YDBC.ItemMasterID,

                        ConceptOrSampleQtyKg = YDBC.BookingQty,
	                    ConceptOrSampleQtyPcs = 0,

	                    Qty = 0,
	                    Pcs = 0,
					
	                    PlannedBatchQtyKg = ISNULL(PB.Qty,0),
	                    PlannedBatchQtyPcs = ISNULL(PB.Pcs,0),

                        KnittingType.TypeName KnittingType,Composition.SegmentValue FabricComposition,
                        Construction.SegmentValue FabricConstruction,Technical.TechnicalName,Gsm.SegmentValue FabricGsm, M.SubGroupID SubGroupID, SG.SubGroupName SubGroup,
                        M.Length, M.Width, M.FUPartID, FU.PartName FUPartName,
                        ColorName = color.SegmentValue
                        FROM {TableNames.YDBookingChild} YDBC 
						INNER JOIN {TableNames.YD_BOOKING_MASTER} YDB ON YDB.YDBookingMasterID = YDBC.YDBookingMasterID
						LEFT JOIN {TableNames.RND_FREE_CONCEPT_MR_CHILD} FCMRC ON FCMRC.FCMRChildID = YDBC.FCMRChildID
						LEFT JOIN {TableNames.FreeConceptMRMaster} FCMR ON FCMR.FCMRMasterID = FCMRC.FCMRMasterID
						LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} M ON M.ConceptID = FCMR.ConceptID
					    LEFT JOIN {TableNames.YD_RECIPE_DEFINITION_ITEM_INFO} II ON II.YDBookingChildID = YDBC.YDBookingChildID
                        LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID = M.KnittingTypeID
                        LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = M.ItemMasterID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = M.CompositionID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = M.ConstructionID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = M.GSMID
                        LEFT JOIN {DbNames.EPYSL}..ItemSubGroup SG ON SG.SubGroupID = M.SubGroupID
                        LEFT JOIN {TableNames.FabricTechnicalName} Technical ON Technical.TechnicalNameId=M.TechnicalNameId
						LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = YDBC.ColorId
                        LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID=M.ConceptID
	                    LEFT JOIN {DbNames.EPYSL}..FabricUsedPart FU ON FU.FUPartID = M.FUPartID
	                    LEFT JOIN PB on PB.YDBookingChildID = YDBC.YDBookingChildID
                        WHERE YDBC.ColorID = {colorID} AND YDBC.YDBookingMasterID = {yDBookingMasterID} AND YDBC.BookingQty > ISNULL(PB.Qty,0);

                    --Childs
                    ;SELECT DI.YDRecipeDInfoID, C.Temperature TempIn, C.ProcessTime, EV.ValueName FiberPart, ISV.SegmentValue ColorName
                    FROM {TableNames.YD_RECIPE_DEFINITION_CHILD} C
                    INNER JOIN {TableNames.YD_RECIPE_DEFINITION_DYEING_INFO} DI ON DI.YDRecipeDInfoID = C.YDRecipeDInfoID
                    INNER JOIN {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID = DI.FiberPartID
                    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = DI.ColorID
                    WHERE C.YDRecipeID = {recipeID}
                    GROUP BY DI.YDRecipeDInfoID, C.Temperature, C.ProcessTime, EV.ValueName, ISV.SegmentValue;

                    --Def Childs
				    SELECT C.YDRecipeChildID, C.YDRecipeID,C.ProcessID,C.Qty,C.UnitID,C.TempIn,C.TempOut,C.ParticularsID,C.RawItemID,I.ItemName,C.ProcessTime,
				    P.ValueName ProcessName,PL.ValueName ParticularsName,R.ItemName RawItemName, UU.DisplayUnitDesc Unit, C.IsPercentage,
				    C.YDRecipeDInfoID, C.Temperature, EV.ValueName FiberPart, ISV.SegmentValue ColorName
				    FROM {TableNames.YD_RECIPE_DEFINITION_CHILD} C
				    LEFT JOIN {DbNames.EPYSL}..ItemMaster I ON I.ItemMasterID=C.RawItemID
				    LEFT JOIN {DbNames.EPYSL}..EntityTypeValue P ON P.ValueID = C.ProcessID
				    LEFT JOIN {DbNames.EPYSL}..EntityTypeValue PL ON PL.ValueID = C.ParticularsID
				    LEFT JOIN {DbNames.EPYSL}..ItemMaster R ON R.ItemMasterID = C.RawItemID
				    LEFT JOIN {DbNames.EPYSL}..Unit UU ON UU.UnitID = C.UnitID
				    INNER JOIN {TableNames.YD_RECIPE_DEFINITION_DYEING_INFO} DI ON DI.YDRecipeDInfoID = C.YDRecipeDInfoID
				    INNER JOIN {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID = DI.FiberPartID
				    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = DI.ColorID
				    WHERE C.YDRecipeID = {recipeID}

                    -----K Production
                    ; WITH M AS(
                            SELECT KP.*
                            FROM {TableNames.RND_KNITTING_PRODUCTION} KP
							INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FC ON FC.ConceptID = KP.ConceptID
							WHERE FC.GroupConceptNo = '{conceptNo}' AND KP.InActive = 0
                    )
                    SELECT M.GRollID, M.KJobCardMasterID, M.ProductionDate, M.ConceptID, M.OperatorID, M.ShiftID, M.RollSeqNo, M.RollNo, M.RollQty, M.RollQtyPcs, M.ProductionGSM, M.ProductionWidth,
                    M.FirstRollCheck, M.FirstRollCheckBy, M.FirstRollCheckDate, M.FirstRollPass, M.SendforQC, M.SendQCDate, M.SendQCBy, M.QCComplete, M.QCCompleteDate, M.QCCompleteBy,
                    M.QCWidth, M.QCGSM, M.QCPass, M.QCPassQty, M.AddedBy, M.UpdatedBy, M.ProdComplete, M.ProdQty, M.Hole, M.Loop, M.SetOff, M.LycraOut, M.LycraDrop, M.OilSpot, M.Slub, M.FlyingDust,
                    M.MissingYarn, M.Knot, M.DropStitch, M.YarnContra, M.NeddleBreakage, M.Defected, M.WrongDesign, M.Patta, M.ShinkerMark, M.NeddleMark, M.EdgeMark, M.WheelFree, M.CountMix,
                    M.ThickAndThin, M.LineStar, M.QCOthers, M.Comment, M.CalculateValue, M.Grade, M.RollLength, M.Hold, M.QCBy, M.QCShiftID, M.BookingID, M.DateAdded, M.DateUpdated,
                    M.ExportOrderID, M.BuyerID, M.BuyerTeamID, M.ParentGRollID, M.InActive, M.InActiveBy, M.InActiveDate, M.InActiveReason, M.BatchID
                    FROM M
					Left Join {TableNames.YD_BATCH_CHILD} BC ON BC.GRollID = M.GRollID
					where BC.GRollID IS NULL;

                    ----Unit
                    ;SELECT CAST(UnitID AS VARCHAR) AS id, DisplayUnitDesc AS text
                    FROM {DbNames.EPYSL}..Unit;";
                }
                else if (isBDS == 1) //BDS
                {
                    sql = $@"
                     ;SELECT YDBC.YDBookingMasterID, FCM.ConceptID, FCM.BookingID, FCM.ItemMasterID, FCM.Remarks, FCM.GroupConceptNo ConceptNo, FCC.CCColorID, YDBC.ColorID, COL.SegmentValue ColorName, FBA.SLNo,
                    FCM.ExportOrderID,FCM.BuyerID,FCM.BuyerTeamID
                    FROM {TableNames.YDBookingChild} YDBC 
					LEFT JOIN {TableNames.RND_FREE_CONCEPT_MR_CHILD} FCMRC ON FCMRC.FCMRChildID = YDBC.FCMRChildID
					LEFT JOIN {TableNames.FreeConceptMRMaster} FCMR ON FCMR.FCMRMasterID = FCMRC.FCMRMasterID
					LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = FCMR.ConceptID
					LEFT Join {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} FCC ON FCC.ConceptID = FCM.ConceptID
					Inner JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.BookingID = FCM.BookingID
                    LEFT JOIN {TableNames.Knitting_Plan_Master} KPM ON KPM.ConceptID = FCM.ConceptID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID = YDBC.ColorID
                    WHERE YDBC.ColorID =  {colorID} AND YDBC.YDBookingMasterID = {yDBookingMasterID} AND FCM.IsBDS=1
                    GROUP BY YDBC.YDBookingMasterID, FCM.ConceptID, FCM.BookingID, FCM.ItemMasterID, FCM.Remarks, FCM.GroupConceptNo, FCC.CCColorID, YDBC.ColorID, COL.SegmentValue, FBA.SLNo,FCM.ExportOrderID,FCM.BuyerID,FCM.BuyerTeamID;


                    -----BatchItemRequirement
                    WITH PB As(
						Select b.YDBookingChildID, Sum(b.Pcs) Pcs, Sum(b.Qty) Qty
						FROM {TableNames.YD_BATCH_MASTER} a
						Inner JOIN {TableNames.YD_BATCH_ITEM_REQUIREMENT} b on b.YDBatchID = a.YDBatchID
						Where a.ColorID =  {colorID} AND a.YDBookingMasterID = {yDBookingMasterID}
						Group By b.YDBookingChildID
					)
                    SELECT YDBC.YDBookingChildID, M.ConceptID, M.ItemMasterID, YDBC.YarnCategory,  
                    
                    ConceptOrSampleQtyKg = YDBC.BookingQty,
					ConceptOrSampleQtyPcs = 0,

				    Qty = 0, --(CASE WHEN FCM.SubGroupID = 1 THEN ISNULL(FCM.TotalQty,0) ELSE ISNULL(FCM.TotalQtyInKG,0) END) - ISNULL(PB.Qty,0),
					Pcs = 0, --(CASE WHEN FCM.SubGroupID <> 1 THEN ISNULL(FCM.TotalQty,0) ELSE 0 END) - ISNULL(PB.Pcs,0),
					
					PlannedBatchQtyKg = ISNULL(PB.Qty,0),
					PlannedBatchQtyPcs = ISNULL(PB.Pcs,0),

                    KnittingType.TypeName KnittingType,Composition.SegmentValue FabricComposition,                    
                    Construction.SegmentValue FabricConstruction,Technical.TechnicalName,Gsm.SegmentValue FabricGsm, M.SubGroupID SubGroupID, 
                    SG.SubGroupName SubGroup, M.Length, M.Width, M.FUPartID, FU.PartName FUPartName,
                    ColorName = CASE WHEN M.SubGroupID=1 THEN Color.SegmentValue ELSE CollarColor.SegmentValue END
                    FROM {TableNames.YDBookingChild} YDBC 
					INNER JOIN {TableNames.YD_BOOKING_MASTER} YDB ON YDB.YDBookingMasterID = YDBC.YDBookingMasterID
					LEFT JOIN {TableNames.RND_FREE_CONCEPT_MR_CHILD} FCMRC ON FCMRC.FCMRChildID = YDBC.FCMRChildID
					LEFT JOIN {TableNames.FreeConceptMRMaster} FCMR ON FCMR.FCMRMasterID = FCMRC.FCMRMasterID
					LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} M ON M.ConceptID = FCMR.ConceptID
                    LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID = M.KnittingTypeID
                    LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = M.ItemMasterID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = M.CompositionID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = M.ConstructionID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = M.GSMID
                    LEFT JOIN {DbNames.EPYSL}..ItemSubGroup SG ON SG.SubGroupID = M.SubGroupID
                    LEFT JOIN {TableNames.FabricTechnicalName} Technical ON Technical.TechnicalNameId=M.TechnicalNameId
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = IM.Segment3ValueID
					LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue CollarColor ON CollarColor.SegmentValueID = IM.Segment5ValueID
                    LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID=M.ConceptID
                    LEFT JOIN {DbNames.EPYSL}..FabricUsedPart FU ON FU.FUPartID = M.FUPartID
                    LEFT JOIN PB on PB.YDBookingChildID = YDBC.YDBookingChildID
                    WHERE YDBC.ColorID = {colorID} AND YDBC.YDBookingMasterID = {yDBookingMasterID} AND FCM.IsBDS=1
					AND YDBC.BookingQty > ISNULL(PB.Qty,0);

                    -----K Production
                    ; WITH M AS(
                            SELECT KP.*
                            FROM {TableNames.RND_KNITTING_PRODUCTION} KP
							INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FC ON FC.ConceptID = KP.ConceptID
							WHERE FC.GroupConceptNo = '{conceptNo}' AND KP.InActive = 0
                    )
                    SELECT M.GRollID, M.KJobCardMasterID, M.ProductionDate, M.ConceptID, M.OperatorID, M.ShiftID, M.RollSeqNo, M.RollNo, M.RollQty, M.RollQtyPcs, M.ProductionGSM, M.ProductionWidth,
                    M.FirstRollCheck, M.FirstRollCheckBy, M.FirstRollCheckDate, M.FirstRollPass, M.SendforQC, M.SendQCDate, M.SendQCBy, M.QCComplete, M.QCCompleteDate, M.QCCompleteBy,
                    M.QCWidth, M.QCGSM, M.QCPass, M.QCPassQty, M.AddedBy, M.UpdatedBy, M.ProdComplete, M.ProdQty, M.Hole, M.Loop, M.SetOff, M.LycraOut, M.LycraDrop, M.OilSpot, M.Slub, M.FlyingDust,
                    M.MissingYarn, M.Knot, M.DropStitch, M.YarnContra, M.NeddleBreakage, M.Defected, M.WrongDesign, M.Patta, M.ShinkerMark, M.NeddleMark, M.EdgeMark, M.WheelFree, M.CountMix,
                    M.ThickAndThin, M.LineStar, M.QCOthers, M.Comment, M.CalculateValue, M.Grade, M.RollLength, M.Hold, M.QCBy, M.QCShiftID, M.BookingID, M.DateAdded, M.DateUpdated,
                    M.ExportOrderID, M.BuyerID, M.BuyerTeamID, M.ParentGRollID, M.InActive, M.InActiveBy, M.InActiveDate, M.InActiveReason, M.BatchID
                    FROM M
					Left Join {TableNames.YD_BATCH_CHILD} BC ON BC.GRollID = M.GRollID
					where BC.GRollID IS NULL;

                    ----Unit
                    ;SELECT CAST(UnitID AS VARCHAR) AS id, DisplayUnitDesc AS text
                    FROM {DbNames.EPYSL}..Unit;";
                }
                else if (isBDS == 2) //BULK
                {
                    sql = $@"
                ;SELECT YDBC.YDBookingMasterID, FCM.ConceptID, FCM.BookingID, FCM.ItemMasterID, FCM.Remarks, FCM.GroupConceptNo ConceptNo, FCC.CCColorID, YDBC.ColorID, COL.SegmentValue ColorName, FBA.SLNo,
                    FCM.ExportOrderID,FCM.BuyerID,FCM.BuyerTeamID
                    FROM {TableNames.YDBookingChild} YDBC 
					INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_CHILD} FCMRC ON FCMRC.YBChildItemID = YDBC.YBChildItemID
					INNER JOIN {TableNames.FreeConceptMRMaster} FCMR ON FCMR.FCMRMasterID = FCMRC.FCMRMasterID
					INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = FCMR.ConceptID
					INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.BookingID = FCM.BookingID
                    LEFT Join {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} FCC ON FCC.ConceptID = FCM.ConceptID
                    LEFT JOIN {TableNames.Knitting_Plan_Master} KPM ON KPM.ConceptID = FCM.ConceptID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID = YDBC.ColorID
                    WHERE YDBC.YDBookingMasterID = {yDBookingMasterID} AND YDBC.ColorID= {colorID} AND FCM.IsBDS=2
                    GROUP BY YDBC.YDBookingMasterID, FCM.ConceptID, FCM.BookingID, FCM.ItemMasterID, FCM.Remarks, FCM.GroupConceptNo, FCC.CCColorID, YDBC.ColorID, COL.SegmentValue, FBA.SLNo,FCM.ExportOrderID,FCM.BuyerID,FCM.BuyerTeamID;

                    -----BatchItemRequirement
                    WITH PB As(
						Select b.YDBookingChildID, Sum(b.Pcs) Pcs, Sum(b.Qty) Qty
						FROM {TableNames.YD_BATCH_MASTER} a
						Inner JOIN {TableNames.YD_BATCH_ITEM_REQUIREMENT} b on b.YDBatchID = a.YDBatchID
						Where a.YDBookingMasterID = {yDBookingMasterID} AND a.ColorID= {colorID}
						Group By b.YDBookingChildID
					)
                    SELECT YDBC.YDBookingChildID, M.ConceptID, M.ItemMasterID,  YDBC.YarnCategory, 
                    
                    ConceptOrSampleQtyKg = YDBC.BookingQty,
					ConceptOrSampleQtyPcs = 0,

					Qty = 0, --(CASE WHEN FCM.SubGroupID = 1 THEN ISNULL(FCM.TotalQty,0) ELSE ISNULL(FCM.TotalQtyInKG,0) END) - ISNULL(PB.Qty,0),
					Pcs = 0, --(CASE WHEN FCM.SubGroupID <> 1 THEN ISNULL(FCM.TotalQty,0) ELSE 0 END) - ISNULL(PB.Pcs,0),
					
					PlannedBatchQtyKg = ISNULL(PB.Qty,0),
					PlannedBatchQtyPcs = ISNULL(PB.Pcs,0),

                    KnittingType.TypeName KnittingType,Composition.SegmentValue FabricComposition,                    
                    Construction.SegmentValue FabricConstruction,Technical.TechnicalName,Gsm.SegmentValue FabricGsm, M.SubGroupID SubGroupID, 
                    SG.SubGroupName SubGroup, M.Length, M.Width, M.FUPartID, FU.PartName FUPartName,
                    ColorName = CASE WHEN M.SubGroupID=1 THEN Color.SegmentValue ELSE CollarColor.SegmentValue END
                    FROM {TableNames.YDBookingChild} YDBC 
					INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_CHILD} FCMRC ON FCMRC.YBChildItemID = YDBC.YBChildItemID
					INNER JOIN {TableNames.FreeConceptMRMaster} FCMR ON FCMR.FCMRMasterID = FCMRC.FCMRMasterID
					INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} M ON M.ConceptID = FCMR.ConceptID
                    LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID = M.KnittingTypeID
                    LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = M.ItemMasterID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = M.CompositionID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = M.ConstructionID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = M.GSMID
                    LEFT JOIN {DbNames.EPYSL}..ItemSubGroup SG ON SG.SubGroupID = M.SubGroupID
                    LEFT JOIN {TableNames.FabricTechnicalName} Technical ON Technical.TechnicalNameId=M.TechnicalNameId
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = IM.Segment3ValueID
					LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue CollarColor ON CollarColor.SegmentValueID = IM.Segment5ValueID
                    LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID=M.ConceptID
                    LEFT JOIN {DbNames.EPYSL}..FabricUsedPart FU ON FU.FUPartID = M.FUPartID
                    LEFT JOIN PB on PB.YDBookingChildID = YDBC.YDBookingChildID
                    WHERE YDBC.YDBookingMasterID = {yDBookingMasterID} AND YDBC.ColorID= {colorID} AND YDBC.BookingQty > ISNULL(PB.Qty,0);

                    -----K Production
                    ; WITH M AS(
                            SELECT KP.*
                            FROM {TableNames.RND_KNITTING_PRODUCTION} KP
							INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FC ON FC.ConceptID = KP.ConceptID
							WHERE FC.GroupConceptNo = '{conceptNo}' AND KP.InActive = 0
                    )
                    SELECT M.GRollID, M.KJobCardMasterID, M.ProductionDate, M.ConceptID, M.OperatorID, M.ShiftID, M.RollSeqNo, M.RollNo, M.RollQty, M.RollQtyPcs, M.ProductionGSM, M.ProductionWidth,
                    M.FirstRollCheck, M.FirstRollCheckBy, M.FirstRollCheckDate, M.FirstRollPass, M.SendforQC, M.SendQCDate, M.SendQCBy, M.QCComplete, M.QCCompleteDate, M.QCCompleteBy,
                    M.QCWidth, M.QCGSM, M.QCPass, M.QCPassQty, M.AddedBy, M.UpdatedBy, M.ProdComplete, M.ProdQty, M.Hole, M.Loop, M.SetOff, M.LycraOut, M.LycraDrop, M.OilSpot, M.Slub, M.FlyingDust,
                    M.MissingYarn, M.Knot, M.DropStitch, M.YarnContra, M.NeddleBreakage, M.Defected, M.WrongDesign, M.Patta, M.ShinkerMark, M.NeddleMark, M.EdgeMark, M.WheelFree, M.CountMix,
                    M.ThickAndThin, M.LineStar, M.QCOthers, M.Comment, M.CalculateValue, M.Grade, M.RollLength, M.Hold, M.QCBy, M.QCShiftID, M.BookingID, M.DateAdded, M.DateUpdated,
                    M.ExportOrderID, M.BuyerID, M.BuyerTeamID, M.ParentGRollID, M.InActive, M.InActiveBy, M.InActiveDate, M.InActiveReason, M.BatchID
                    FROM M
					Left Join {TableNames.YD_BATCH_CHILD} BC ON BC.GRollID = M.GRollID
					where BC.GRollID IS NULL;

                    ----Unit
                    ;SELECT CAST(UnitID AS VARCHAR) AS id, DisplayUnitDesc AS text
                    FROM {DbNames.EPYSL}..Unit;";
                }
            }
            else
            {
                sql = $@"
                    WITH M AS (
                            SELECT	*
                            FROM {TableNames.YD_RECIPE_DEFINITION_MASTER} DM
	                        WHERE DM.YDRecipeID = {recipeID}
                        )
                        SELECT M.YDRecipeID, M.YDRecipeNo, M.RecipeDate, M.ConceptID, M.BookingID, M.ItemMasterID, M.ColorID, M.RecipeFor, M.BatchWeightKG, M.Remarks,
                        M.ExportOrderID, M.BuyerID, M.BuyerTeamID, RecFor.ValueName RecipeForName, COL.SegmentValue ColorName,FM.GroupConceptNo ConceptNo,M.CCColorID
                        FROM M
                        INNER JOIN {DbNames.EPYSL}..EntityTypeValue RecFor ON RecFor.ValueID = M.RecipeFor
                        INNER JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID = M.ColorID
                        INNER Join {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} CC ON CC.CCColorID = M.CCColorID
	                    INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FM ON FM.ConceptID = CC.ConceptID;

                    -----BatchItemRequirement
                    WITH II AS (
                    SELECT * FROM {TableNames.YD_RECIPE_DEFINITION_ITEM_INFO} WHERE YDRecipeID =  {recipeID}
                    )

                    SELECT II.YDRecipeItemInfoID, RDM.ConceptID, II.ItemMasterID,
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
					INNER JOIN {TableNames.YD_RECIPE_DEFINITION_MASTER} RDM ON RDM.YDRecipeID = II.YDRecipeID 
                    LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} M ON M.ConceptID = RDM.ConceptID
                    LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID = M.KnittingTypeID
                    LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = M.ItemMasterID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = M.CompositionID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = M.ConstructionID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = M.GSMID
                    LEFT JOIN {DbNames.EPYSL}..ItemSubGroup SG ON SG.SubGroupID = M.SubGroupID
                    LEFT JOIN {TableNames.FabricTechnicalName} Technical ON Technical.TechnicalNameId=M.TechnicalNameId
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = IM.Segment3ValueID
					LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue CollarColor ON CollarColor.SegmentValueID = IM.Segment5ValueID
                    LEFT JOIN {TableNames.KNITTING_JOB_CARD_Master} KJ ON KJ.ConceptID=M.ConceptID
					LEFT JOIN {DbNames.EPYSL}..FabricUsedPart FU ON FU.FUPartID = M.FUPartID;

                  --Childs
                 ;WITH M AS (
                        SELECT * FROM {TableNames.YD_RECIPE_DEFINITION_CHILD} WHERE YDRecipeID = {recipeID}
                    )
                    SELECT  M.ProcessTime, M.Temperature TempIn,EV.ValueName FiberPart, ISV.SegmentValue ColorName,
					P.ValueName ProcessName,PL.ValueName ParticularsName, UU.DisplayUnitDesc Unit
					FROM M
					LEFT JOIN {DbNames.EPYSL}..ItemMaster I ON I.ItemMasterID=M.RawItemID
					LEFT JOIN {DbNames.EPYSL}..EntityTypeValue P ON P.ValueID = M.ProcessID
					LEFT JOIN {DbNames.EPYSL}..EntityTypeValue PL ON PL.ValueID = M.ParticularsID
					LEFT JOIN {DbNames.EPYSL}..ItemMaster R ON R.ItemMasterID = M.RawItemID
					INNER JOIN {TableNames.RND_RECIPE_DEFINITION_DYEING_INFO} DI ON DI.RecipeDInfoID = M.RecipeDInfoID
					INNER JOIN {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID = DI.FiberPartID
					INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = DI.ColorID
                    LEFT JOIN {DbNames.EPYSL}..Unit UU ON UU.UnitID = M.UnitID
					group by M.ProcessTime, M.Temperature,
					P.ValueName ,PL.ValueName ,UU.DisplayUnitDesc ,EV.ValueName,ISV.SegmentValue
					;

                --Def Childs
				    SELECT C.YDRecipeChildID, C.YDRecipeID,C.ProcessID,C.Qty,C.UnitID,C.TempIn,C.TempOut,C.ParticularsID,C.RawItemID,I.ItemName,C.ProcessTime,
				    P.ValueName ProcessName,PL.ValueName ParticularsName,R.ItemName RawItemName, UU.DisplayUnitDesc Unit, C.IsPercentage,
				    C.RecipeDInfoID, C.Temperature, EV.ValueName FiberPart, ISV.SegmentValue ColorName
				    FROM {TableNames.YD_RECIPE_DEFINITION_CHILD} C
				    LEFT JOIN {DbNames.EPYSL}..ItemMaster I ON I.ItemMasterID=C.RawItemID
				    LEFT JOIN {DbNames.EPYSL}..EntityTypeValue P ON P.ValueID = C.ProcessID
				    LEFT JOIN {DbNames.EPYSL}..EntityTypeValue PL ON PL.ValueID = C.ParticularsID
				    LEFT JOIN {DbNames.EPYSL}..ItemMaster R ON R.ItemMasterID = C.RawItemID
				    LEFT JOIN {DbNames.EPYSL}..Unit UU ON UU.UnitID = C.UnitID
				    INNER JOIN {TableNames.RND_RECIPE_DEFINITION_DYEING_INFO} DI ON DI.RecipeDInfoID = C.RecipeDInfoID
				    INNER JOIN {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID = DI.FiberPartID
				    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = DI.ColorID
				    WHERE C.YDRecipeID = {recipeID};

                    -----childs
                    ;WITH M AS (
                        SELECT * FROM {TableNames.RND_KNITTING_PRODUCTION} WHERE BookingID = {bookingID}
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
                YDBatchMaster data = records.Read<YDBatchMaster>().FirstOrDefault();
                List<YDBatchItemRequirement> YDBatchItemRequirements = records.Read<YDBatchItemRequirement>().ToList();
                data.YDBatchItemRequirements = YDBatchItemRequirements.Where(x => x.SubGroupID == 1).ToList();
                data.YDBatchOtherItemRequirements = YDBatchItemRequirements.Where(x => x.SubGroupID != 1).ToList();

                int recipeItemInfoID = 1;
                data.YDBatchItemRequirements.ForEach(x => x.YDRecipeItemInfoID = recipeItemInfoID++);
                data.YDBatchOtherItemRequirements.ForEach(x => x.YDRecipeItemInfoID = recipeItemInfoID++);

                if (isBDS == 0) // Concept
                {
                    data.YDBatchWiseRecipeChilds = records.Read<YDBatchWiseRecipeChild>().ToList();
                    data.YDDefChilds = records.Read<YDRecipeDefinitionChild>().ToList();
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
        public async Task<YDBatchMaster> GetAsync(int id, string conceptNo)
        {
            var sql = $@"
                        ;WITH M AS (
                            SELECT	*
                            FROM {TableNames.YD_BATCH_MASTER} BM WHERE BM.YDBatchID = {id}
                        )
                        SELECT M.YDBatchID, M.YDBatchNo, M.YDBatchDate, M.YDRecipeID, M.ColorID, M.CCColorID, M.BatchWeightKG, M.BatchWeightPcs, M.Remarks,
                        M.ExportOrderID, M.BuyerID, M.BuyerTeamID, RDM.YDRecipeNo, RDM.RecipeDate, COL.SegmentValue ColorName, M.IsApproved,M.DMID, M.MachineLoading,
                        M.DyeingNozzleQty, M.DyeingMcCapacity, M.DMID, DM.DyeingMcslNo DMNo, FM.GroupConceptNo ConceptNo
                        FROM M
                        LEFT JOIN {TableNames.YD_RECIPE_DEFINITION_MASTER} RDM ON RDM.YDRecipeID = M.YDRecipeID
                        INNER JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID = M.ColorID
						LEFT JOIN {TableNames.DYEING_MACHINE} DM ON DM.DMID = M.DMID
                        LEFT Join {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} CC ON CC.CCColorID = M.CCColorID
	                    LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FM ON FM.ConceptID = CC.ConceptID;

                        -----BatchItemRequirement
                        ;WITH II AS (
                            SELECT R.*, YDBC.YarnCategory, YDBC.BookingQty
							FROM {TableNames.YD_BATCH_ITEM_REQUIREMENT} R
							INNER JOIN {TableNames.YD_BATCH_MASTER} B ON B.YDBatchID = R.YDBatchID
							INNER JOIN {TableNames.YDBookingChild} YDBC ON YDBC.YDBookingChildID = R.YDBookingChildID
							WHERE R.YDBatchID = {id}
                        ),
					    ItemSegment As
					    (
						    SELECT CAST(ISV.SegmentValueID As varchar) [id], ISV.SegmentValue [text], CAST(ISN.SegmentNameID As varchar) [desc]
						    FROM {DbNames.EPYSL}..ItemSegmentName ISN
						    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISN.SegmentNameID = ISV.SegmentNameID
						    WHERE ISNULL(ISV.SegmentValue, '') <> '' And SegmentValueID NOT IN(17725,5127,1616,1{id}6{id},47221,2117,1616,1621,44382,44384,45{id}6{id},46717,47220, 2155) --2155 for Ring Yarn Type which is not Needed by Supply Chain dept dated 11-02-2020
					    ),
                        PB As(
	                        Select b.YDBookingChildID, Sum(b.Pcs) Pcs, Sum(b.Qty) Qty
	                        FROM {TableNames.YD_BATCH_ITEM_REQUIREMENT} b
	                        INNER JOIN {TableNames.YD_BATCH_ITEM_REQUIREMENT} c on c.ConceptID=b.ConceptID AND c.YDBatchID={id}
	                        WHERE b.YDBatchID!={id}
	                        Group By b.YDBookingChildID
                        )
                        SELECT II.YDBItemReqID, II.YDBatchID, II.YDRecipeItemInfoID, II.ItemMasterID, II.YarnCategory, II.YDBookingChildID,
                        
                        ConceptOrSampleQtyKg = II.BookingQty,
					    ConceptOrSampleQtyPcs = 0,

						Qty = II.Qty,
						Pcs = II.Pcs,
					
						PlannedBatchQtyKg = ISNULL(PB.Qty,0),
						PlannedBatchQtyPcs = ISNULL(PB.Pcs,0),

                        II.IsFloorRequistion, KnittingType.TypeName KnittingType,
                        Composition.text FabricComposition, Construction.text FabricConstruction,Technical.TechnicalName,Gsm.text FabricGsm, M.SubGroupID SubGroupID,
                        SG.SubGroupName SubGroup, M.Length, M.Width, M.ConceptID, M.FUPartID, FU.PartName FUPartName,
                        ColorName = CASE WHEN M.SubGroupID=1 THEN Color.SegmentValue ELSE CollarColor.SegmentValue END
					    FROM II
					    LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} M ON M.ConceptID=II.ConceptID
                        LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = M.ItemMasterID
					    LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID=M.KnittingTypeID
					    LEFT JOIN ItemSegment Composition ON Composition.id=M.CompositionID
					    LEFT JOIN ItemSegment Construction ON Construction.id=M.ConstructionID
					    LEFT JOIN ItemSegment Gsm ON Gsm.id=M.GSMID
					    LEFT JOIN {DbNames.EPYSL}..ItemSubGroup SG ON SG.SubGroupID = M.SubGroupID
                        LEFT JOIN {TableNames.FabricTechnicalName} Technical ON Technical.TechnicalNameId=M.TechnicalNameId
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = IM.Segment3ValueID
						LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue CollarColor ON CollarColor.SegmentValueID = IM.Segment5ValueID
                        LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID=M.ConceptID
                        LEFT JOIN {DbNames.EPYSL}..FabricUsedPart FU ON FU.FUPartID = M.FUPartID
                        LEFT JOIN PB on PB.YDBookingChildID = II.YDBookingChildID;

                        ----Batch child
                        
                        ;SELECT C.YDBatchChildID, C.BItemReqID, C.YDBatchID, C.GRollID, C.ItemMasterID, C.RollQty, C.RollQtyPcs, KP.RollNo
                        FROM {TableNames.YD_BATCH_CHILD} C
                        INNER JOIN {TableNames.RND_KNITTING_PRODUCTION} KP ON KP.GRollID = C.GRollID
                        WHERE C.YDBatchID = {id};

                        
                        

                         -----BatchWiseRecipeChild
                        ;WITH M AS (
                            SELECT RD.YDRecipeDInfoID, RD.Temperature, RD.TempIn, BM.YDBatchID, RD.ProcessTime 
						FROM {TableNames.YD_BATCH_MASTER} BM
						INNER JOIN {TableNames.YD_BATCH_ITEM_REQUIREMENT} BC ON BC.YDBatchID = BM.YDBatchID
						INNER JOIN {TableNames.YD_RECIPE_REQ_MASTER} RR ON RR.YDBookingChildID = BC.YDBookingChildID
						--INNER Join {TableNames.YD_RECIPE_REQ_CHILD} RC ON RC.YDRecipeReqMasterID = RR.YDRecipeReqMasterID
						INNER JOIN {TableNames.YD_RECIPE_DEFINITION_MASTER} RDM ON RDM.YDRecipeReqMasterID = RR.YDRecipeReqMasterID
						LEFT JOIN {TableNames.YD_RECIPE_DEFINITION_CHILD} RD ON RD.YDRecipeID = RDM.YDRecipeID
							WHERE BC.YDBatchID = {id}
                        )
                        SELECT DI.YDRecipeDInfoID, M.TempIn, M.YDBatchID, M.ProcessTime, EV.ValueName FiberPart, ISV.SegmentValue ColorName
					    FROM M
						INNER JOIN {TableNames.YD_RECIPE_DEFINITION_DYEING_INFO} DI ON DI.YDRecipeDInfoID = M.YDRecipeDInfoID
						INNER JOIN {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID = DI.FiberPartID
						INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = DI.ColorID
						GROUP BY DI.YDRecipeDInfoID, M.TempIn, M.YDBatchID, M.ProcessTime, EV.ValueName, ISV.SegmentValue;

                        ----Def Childs
                        SELECT BM.YDBatchID, RD.YDRecipeChildID, RD.ProcessID, RD.ParticularsID, RD.RawItemID, RD.Qty, RD.UnitID, RD.TempIn,
						RD.TempOut, RD.ProcessTime, RD.YDRecipeID,I.ItemName, P.ValueName ProcessName,PL.ValueName ParticularsName,R.ItemName RawItemName,
						UU.DisplayUnitDesc Unit, RD.IsPercentage, RD.YDRecipeDInfoID, RD.Temperature, EV.ValueName FiberPart, ISV.SegmentValue ColorName
						FROM {TableNames.YD_BATCH_MASTER} BM
						INNER JOIN {TableNames.YD_BATCH_ITEM_REQUIREMENT} BC ON BC.YDBatchID = BM.YDBatchID
						INNER JOIN {TableNames.YD_RECIPE_REQ_MASTER} RR ON RR.YDBookingChildID = BC.YDBookingChildID
						--INNER Join {TableNames.YD_RECIPE_REQ_CHILD} RC ON RC.YDRecipeReqMasterID = RR.YDRecipeReqMasterID
						INNER JOIN {TableNames.YD_RECIPE_DEFINITION_MASTER} RDM ON RDM.YDRecipeReqMasterID = RR.YDRecipeReqMasterID
						LEFT JOIN {TableNames.YD_RECIPE_DEFINITION_CHILD} RD ON RD.YDRecipeID = RDM.YDRecipeID
						LEFT JOIN {DbNames.EPYSL}..ItemMaster I ON I.ItemMasterID=RD.RawItemID
						LEFT JOIN {DbNames.EPYSL}..EntityTypeValue P ON P.ValueID = RD.ProcessID
						LEFT JOIN {DbNames.EPYSL}..EntityTypeValue PL ON PL.ValueID = RD.ParticularsID
						LEFT JOIN {DbNames.EPYSL}..ItemMaster R ON R.ItemMasterID = RD.RawItemID
						LEFT JOIN {DbNames.EPYSL}..Unit UU ON UU.UnitID = RD.UnitID
						INNER JOIN {TableNames.YD_RECIPE_DEFINITION_DYEING_INFO} DI ON DI.YDRecipeDInfoID = RD.YDRecipeDInfoID
						INNER JOIN {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID = DI.FiberPartID
						INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = DI.ColorID
						WHERE BM.YDBatchID = {id};
						

                        ----Knitting Production
                        ;WITH M AS(
                            SELECT KP.* FROM {TableNames.RND_KNITTING_PRODUCTION} KP
							INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FC ON FC.ConceptID = KP.ConceptID
							WHERE FC.GroupConceptNo = '{conceptNo}' AND KP.InActive = 0
                        )
                        SELECT M.GRollID, M.KJobCardMasterID, M.ProductionDate, M.ConceptID, M.OperatorID, M.ShiftID, M.RollSeqNo, M.RollNo, M.RollQty, M.RollQtyPcs, M.ProductionGSM, M.ProductionWidth,
                        M.FirstRollCheck, M.FirstRollCheckBy, M.FirstRollCheckDate, M.FirstRollPass, M.SendforQC, M.SendQCDate, M.SendQCBy, M.QCComplete, M.QCCompleteDate, M.QCCompleteBy,
                        M.QCWidth, M.QCGSM, M.QCPass, M.QCPassQty, M.AddedBy, M.UpdatedBy, M.ProdComplete, M.ProdQty, M.Hole, M.Loop, M.SetOff, M.LycraOut, M.LycraDrop, M.OilSpot, M.Slub, M.FlyingDust,
                        M.MissingYarn, M.Knot, M.DropStitch, M.YarnContra, M.NeddleBreakage, M.Defected, M.WrongDesign, M.Patta, M.ShinkerMark, M.NeddleMark, M.EdgeMark, M.WheelFree, M.CountMix,
                        M.ThickAndThin, M.LineStar, M.QCOthers, M.Comment, M.CalculateValue, M.Grade, M.RollLength, M.Hold, M.QCBy, M.QCShiftID, M.BookingID, M.DateAdded, M.DateUpdated,
                        M.ExportOrderID, M.BuyerID, M.BuyerTeamID, M.ParentGRollID, M.InActive, M.InActiveBy, M.InActiveDate, M.InActiveReason, M.BatchID
                        FROM M
					    Left Join {TableNames.YD_BATCH_CHILD} BC ON BC.GRollID = M.GRollID
					    where BC.GRollID IS NULL;

                        --Unit
                        ;SELECT CAST(UnitID AS VARCHAR) AS id, DisplayUnitDesc AS text
                        FROM {DbNames.EPYSL}..Unit;";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YDBatchMaster data = records.Read<YDBatchMaster>().FirstOrDefault();
                Guard.Against.NullObject(data);

                List<YDBatchItemRequirement> YDBatchItemRequirements = records.Read<YDBatchItemRequirement>().ToList();
                List<YDBatchChild> batchChilds = records.Read<YDBatchChild>().ToList();
                YDBatchItemRequirements.ForEach(x => x.YDBatchChilds = batchChilds.FindAll(c => c.BItemReqID == x.YDBItemReqID));

                data.YDBatchItemRequirements = YDBatchItemRequirements.Where(x => x.SubGroupID == 1).ToList();
                data.YDBatchOtherItemRequirements = YDBatchItemRequirements.Where(x => x.SubGroupID != 1).ToList();

                data.YDBatchWiseRecipeChilds = records.Read<YDBatchWiseRecipeChild>().ToList();
                data.YDDefChilds = records.Read<YDRecipeDefinitionChild>().ToList();
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

        public async Task<YDBatchMaster> GetAllAsync(int id)
        {
            var sql = $@"
                SELECT * FROM {TableNames.YD_BATCH_MASTER} WHERE YDBatchID = {id};

                Select * FROM {TableNames.YD_BATCH_ITEM_REQUIREMENT} Where YDBatchID = {id};

                Select * FROM {TableNames.YD_BATCH_CHILD} Where YDBatchID = {id};

                Select * FROM {TableNames.YD_BATCH_WISE_RECIPE_CHILD} Where YDBatchID = {id};";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YDBatchMaster data = await records.ReadFirstOrDefaultAsync<YDBatchMaster>();
                Guard.Against.NullObject(data);

                data.YDBatchItemRequirements = records.Read<YDBatchItemRequirement>().ToList();
                List<YDBatchChild> batchChilds = records.Read<YDBatchChild>().ToList();
                data.YDBatchItemRequirements.ForEach(x => x.YDBatchChilds = batchChilds.FindAll(c => c.BItemReqID == x.YDBItemReqID));
                data.YDBatchWiseRecipeChilds = records.Read<YDBatchWiseRecipeChild>().ToList();

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

        public async Task<YDBatchItemRequirement> GetYDBatchItemRequirementAsync(int id)
        {
            var query = $@"Select * FROM {TableNames.YD_BATCH_ITEM_REQUIREMENT} Where YDBItemReqID = {id}";
            var record = await _service.GetFirstOrDefaultAsync<YDBatchItemRequirement>(query);
            Guard.Against.NullObject(record);
            return record;
        }
        public async Task<List<YDBatchItemRequirement>> GetOtherItems(PaginationInfo paginationInfo, string yDBookingChildIds, int colorId, int yDBookingMasterID)
        {
            if (colorId == 0 || yDBookingMasterID == 0) return new List<YDBatchItemRequirement>();
            string conceptIdCon = "";
            if (yDBookingChildIds != "-" && !yDBookingChildIds.IsNullOrEmpty())
            {
                conceptIdCon = $@" AND YDBC.YDBookingChildID NOT IN ({yDBookingChildIds})";
            }

            paginationInfo.OrderBy = string.IsNullOrEmpty(paginationInfo.OrderBy) ? "ORDER BY ConceptID DESC" : paginationInfo.OrderBy;

            var sql = $@"
                    WITH M AS (
	                    SELECT YDBC.YDBookingChildID, FCM.ConceptID, FCM.ItemMasterID,FCM.Length, FCM.Width, FCM.FUPartID, FCM.SubGroupID, FCM.KnittingTypeID, 
						FCM.CompositionID, FCM.ConstructionID, FCM.GSMID, FCM.TechnicalNameId, YDBC.YarnCategory, YDBC.BookingQty
	                    FROM {TableNames.YDBookingChild} YDBC 
						INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_CHILD} FCMRC ON FCMRC.FCMRChildID = YDBC.FCMRChildID
						INNER JOIN {TableNames.FreeConceptMRMaster} FCMR ON FCMR.FCMRMasterID = FCMRC.FCMRMasterID
						INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = FCMR.ConceptID
						--INNER Join {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} FCC ON FCC.ColorID = FCM.ConceptID
	                    WHERE YDBC.ColorID={colorId}
	                    AND YDBC.YDBookingMasterID = {yDBookingMasterID} {conceptIdCon}
                    )
                    SELECT M.YDBookingChildID, M.ConceptID, M.ItemMasterID, ConceptOrSampleQtyKg = ISNULL(M.BookingQty,0), ProdQty = ISNULL(KJ.ProdQty,0),
                    ProdQtyPcs = ISNULL(KJ.ProdQtyPcs,0), KnittingType.TypeName KnittingType,Composition.SegmentValue FabricComposition,
                    Construction.SegmentValue FabricConstruction,Technical.TechnicalName,Gsm.SegmentValue FabricGsm, M.SubGroupID SubGroupID, 
                    SG.SubGroupName SubGroup, M.Length, M.Width, M.FUPartID, FU.PartName FUPartName, M.YarnCategory
                    FROM M
                    LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID = M.KnittingTypeID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = M.CompositionID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = M.ConstructionID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = M.GSMID
                    LEFT JOIN {DbNames.EPYSL}..ItemSubGroup SG ON SG.SubGroupID = M.SubGroupID
                    LEFT JOIN {TableNames.FabricTechnicalName} Technical ON Technical.TechnicalNameId=M.TechnicalNameId
                    LEFT JOIN {TableNames.KNITTING_JOB_CARD_Master} KJ ON KJ.ConceptID=M.ConceptID
                    LEFT JOIN {DbNames.EPYSL}..FabricUsedPart FU ON FU.FUPartID = M.FUPartID
                    {paginationInfo.FilterBy}
                    {paginationInfo.OrderBy}
                    {paginationInfo.PageBy}";

            var record = await _service.GetDataAsync<YDBatchItemRequirement>(sql);
            Guard.Against.NullObject(record);
            return record;
        }

        public async Task<List<YDBatchMaster>> GetAllBatchByColorAsync(PaginationInfo paginationInfo, int colorID)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By YDBatchID ASC" : paginationInfo.OrderBy;

            var query = $@"
                With F As (
                    Select YDBatchID, YDBatchNo FROM {TableNames.YD_BATCH_MASTER}
                    --Where ColorID = {colorID}
                )

                Select *, COUNT(*) Over() TotalRows  From F
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<YDBatchMaster>(query);
        }
        public async Task SaveAsync(YDBatchMaster entity)
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
                        await UpdateAsync(entity, transactionGmt);
                        break;

                    default:
                        break;
                }

                List<YDBatchChild> batchChilds = new List<YDBatchChild>();
                entity.YDBatchItemRequirements.ForEach(x => batchChilds.AddRange(x.YDBatchChilds));

                await _service.SaveSingleAsync(entity, transaction);
                await _service.SaveAsync(entity.YDBatchItemRequirements, transaction);
                await _service.SaveAsync(batchChilds, transaction);
                await _service.SaveAsync(entity.YDBatchWiseRecipeChilds, transaction);

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
                throw (ex);
            }
            finally
            {
                if (transaction != null) transaction.Dispose();
                if (transactionGmt != null) transactionGmt.Dispose();
                _connection.Close();
                _connectionGmt.Close();
            }
        }
        public async Task SaveAsyncRecipeCopy(YDBatchMaster entity)
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
                        await UpdateAsync(entity, transactionGmt);
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
                throw (ex);
            }
            finally
            {
                if (transaction != null) transaction.Dispose();
                if (transactionGmt != null) transactionGmt.Dispose();
                _connection.Close();
                _connectionGmt.Close();
            }
        }
        public async Task UpdateBDSTNA_YDBatchPreparationPlanAsync(int BatchID)
        {
            await _service.ExecuteAsync(SPNames.spUpdateBDSTNA_BatchPreparationPlan, new { BatchID = BatchID }, 30, CommandType.StoredProcedure);
        }

        #region Helpers

        private async Task<YDBatchMaster> AddAsync(YDBatchMaster entity, SqlTransaction transactionGmt)
        {
            entity.YDBatchID = await _service.GetMaxIdAsync(TableNames.YD_BATCH_MASTER, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
            int paddingValue = 3;
            //int maxNo = await _signatureRepository.GetMaxNoAsync(TableNames.YD_BATCH_MASTER, "YDBatchNo", entity.SLNo, entity.SLNo.Length + paddingValue);
            //entity.YDBatchNo = entity.SLNo + maxNo.ToString().PadLeft(paddingValue, '0');
            // YDBatchMaster ydbm = await GetYDBatchNo(entity.YDBookingMasterID, entity.ColorID);
            //entity.YDBatchNo = ydbm.YDBatchNo;

            var maxReqChildId = await _service.GetMaxIdAsync(TableNames.YD_BATCH_ITEM_REQUIREMENT, entity.YDBatchItemRequirements.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
            var maxRecipeChildId = await _service.GetMaxIdAsync(TableNames.YD_BATCH_WISE_RECIPE_CHILD, entity.YDBatchWiseRecipeChilds.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
            var maxChildId = await _service.GetMaxIdAsync(TableNames.YD_BATCH_CHILD, entity.YDBatchItemRequirements.Sum(x => x.YDBatchChilds.Count()));

            foreach (var item in entity.YDBatchItemRequirements)
            {
                item.YDBItemReqID = maxReqChildId++;
                item.YDBatchID = entity.YDBatchID;

                foreach (var child in item.YDBatchChilds)
                {
                    child.YDBatchChildID = maxChildId++;
                    child.YDBatchID = entity.YDBatchID;
                    child.BItemReqID = item.YDBItemReqID;
                }
            }

            foreach (var item in entity.YDBatchWiseRecipeChilds)
            {
                item.YDBRecipeChildID = maxRecipeChildId++;
                item.YDBatchID = entity.YDBatchID;
            }

            return entity;
        }

        private async Task UpdateAsync(YDBatchMaster entity, SqlTransaction transactionGmt)
        {
            var maxReqChildId = await _service.GetMaxIdAsync(TableNames.YD_BATCH_ITEM_REQUIREMENT, entity.YDBatchItemRequirements.Where(x => x.EntityState == EntityState.Added).Count(), RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
            var maxRecipeChildId = await _service.GetMaxIdAsync(TableNames.YD_BATCH_WISE_RECIPE_CHILD, entity.YDBatchWiseRecipeChilds.Where(x => x.EntityState == EntityState.Added).Count(), RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
            var maxChildId = await _service.GetMaxIdAsync(TableNames.YD_BATCH_CHILD, entity.YDBatchItemRequirements.Sum(x => x.YDBatchChilds.Where(y => y.EntityState == EntityState.Added).Count()), RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

            foreach (var item in entity.YDBatchItemRequirements)
            {
                foreach (var child in item.YDBatchChilds)
                {
                    switch (child.EntityState)
                    {
                        case EntityState.Added:
                            child.YDBatchChildID = maxChildId++;
                            child.YDBatchID = entity.YDBatchID;
                            child.BItemReqID = item.YDBItemReqID;
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
                        item.YDBItemReqID = maxReqChildId++;
                        item.YDBatchID = entity.YDBatchID;
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

            foreach (var item in entity.YDBatchWiseRecipeChilds)
            {
                switch (item.EntityState)
                {
                    case EntityState.Added:
                        item.YDBRecipeChildID = maxRecipeChildId++;
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
        public async Task<YDBatchMaster> GetYDBatchNo(int yDBookingMasterID, int colorID)
        {
            var query = $@"
        WITH YDB AS
        (
            SELECT YDBNo 
            FROM YDBookingMaster 
            WHERE YDBookingMasterID = {yDBookingMasterID}
        ),
        ColorNoAll AS
        (
            SELECT 
                CASE 
                    WHEN ROW_NUMBER() OVER (ORDER BY Color.SegmentValue) < 10 
                    THEN '0' + CAST(ROW_NUMBER() OVER (ORDER BY Color.SegmentValue) AS VARCHAR) 
                    ELSE CAST(ROW_NUMBER() OVER (ORDER BY Color.SegmentValue) AS VARCHAR) 
                END AS ColorSerial,
                YDBC.YDBookingChildID, 
                YDBC.ItemMasterID, 
                YDBC.ColorId, 
                Color.SegmentValue AS ColorName
            FROM {TableNames.YDBookingChild} YDBC
            LEFT JOIN EPYSL..ItemSegmentValue Color ON Color.SegmentValueID = YDBC.ColorId
            WHERE YDBC.YDBookingMasterID = {yDBookingMasterID}
        ),
        Color AS
        (
            SELECT ColorSerial 
            FROM ColorNoAll 
            WHERE ColorId = {colorID}
        ),
        YS AS
        (
            SELECT YDB.YDBNo + Color.ColorSerial AS YDBNoColor
            FROM YDB, Color
        ),
        NN AS
        (
            SELECT COUNT(YDBatchNo) AS Cnt 
            FROM {TableNames.YD_BATCH_MASTER} 
            WHERE YDBatchNo LIKE (SELECT YDBNoColor FROM YS) + '%'
        )
        SELECT 
            (SELECT YDBNoColor FROM YS) 
            + CASE 
                WHEN (SELECT Cnt FROM NN) + 1 < 10 
                THEN '0' + CAST((SELECT Cnt FROM NN) + 1 AS VARCHAR) 
                ELSE CAST((SELECT Cnt FROM NN) + 1 AS VARCHAR) 
              END AS YDBatchNo;";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                YDBatchMaster data = await records.ReadFirstOrDefaultAsync<YDBatchMaster>();
                Guard.Against.NullObject(data);

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
        #endregion Helpers
    }
}
