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
    public class RecipieRequestService : IRecipieRequestService
    {
        private readonly IDapperCRUDService<RecipeRequestMaster> _service;
        private readonly SqlConnection _connection;
        private readonly SqlConnection _connectionGmt;
        private SqlTransaction transaction;
        private SqlTransaction transactionGmt;


        private readonly IDapperCRUDService<DyeingBatchMaster> _dyeingBatchService;
        private readonly IDapperCRUDService<BatchMaster> _batchService;
        private readonly IDapperCRUDService<DyeingBatchItem> _dyeingBatchItemService;
        public RecipieRequestService(IDapperCRUDService<RecipeRequestMaster> service,
            IDapperCRUDService<DyeingBatchMaster> dyeingBatchService,
            IDapperCRUDService<BatchMaster> batchService,
            IDapperCRUDService<DyeingBatchItem> dyeingBatchItemService)
            
        {
            _service = service;
            _service.Connection = service.GetConnection(AppConstants.GMT_CONNECTION);
            _connectionGmt = service.Connection;

            _service.Connection = service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _connection = service.Connection;

            _dyeingBatchService = dyeingBatchService;
            _batchService = batchService;
            _dyeingBatchItemService = dyeingBatchItemService;
        }

        public async Task<List<RecipeRequestMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By CCColorId Desc" : paginationInfo.OrderBy;
            string sql;
            if (status == Status.Pending)
            {
                sql = $@"
                 ;WITH 
                FDI As
                (
                    Select DI.CCColorID, DI.FiberPartID, Min(Convert(int, ISNULL(DI.RecipeOn, 0))) RecipeOn
                    From {TableNames.RND_RECIPE_DEFINITION_DYEING_INFO} DI
                    LEFT JOIN {TableNames.RND_RECIPE_DEFINITION_MASTER} DM ON DM.RecipeReqMasterID = DI.RecipeReqMasterID
                    WHERE ISNULL(DM.IsArchive, 0) = 0
                    Group By DI.CCColorID, DI.FiberPartID
                ),
                C AS ----Free Concept
                (
                    SELECT a.CCColorID, a.ColorId, a.ColorCode, FCM.GroupConceptNo,
                    FCM.IsBDS, 'R&D' Buyer, 'R&D' BuyerTeam, '' LabDipNo1, Color.SegmentValue ColorName, FCM.BookingID
                    FROM {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} a
                    INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = a.ConceptID
	                Left JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = a.ColorID
                    Left Join {DbNames.EPYSL}..SampleBookingMaster SBM on SBM.BookingID = FCM.BookingID
                    Inner Join {TableNames.KNITTING_JOB_CARD_Master} JC ON JC.ConceptID = FCM.ConceptID
                    LEFT JOIN {TableNames.RND_RECIPE_REQ_CHILD} RR ON RR.CCColorID = a.CCColorID And RR.ItemMasterID = FCM.ItemMasterID
                    Left Join FDI On FDI.CCColorID = a.CCColorID
                    where JC.GrayFabricOK = 1 And RR.CCColorID IS NULL AND FCM.IsBDS = 0 And ISNULL(SBM.SampleID,0) <> 13
                    Group By a.CCColorID, a.ColorId, a.ColorCode, FCM.GroupConceptNo,FCM.IsBDS, Color.SegmentValue, FCM.BookingID
                    Having Min(Convert(int, ISNULL(FDI.RecipeOn, 0))) = 0
                ),
                FBC_C AS
                (
	                SELECT FAC.LabDipNo, T.BookingID
	                FROM {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD_TEXT} T
	                INNER JOIN C MO ON MO.BookingID = T.BookingID
	                INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FAC On FAC.BookingChildID = T.BookingChildID AND FAC.ConsumptionID = T.ConsumptionID AND FAC.BookingID = T.BookingID
	                WHERE T.UsesIn Like '%Body%'
	                GROUP BY FAC.LabDipNo, T.BookingID
                ),
                B AS ----Sample & Bulk
                (
                    SELECT a.CCColorID, a.ColorId, a.ColorCode, FCM.GroupConceptNo,FCM.IsBDS, 
	                Buyer = CASE WHEN ISNULL(FCM.BuyerID,0) > 0 THEN C.ShortName ELSE 'R&D' END, 
	                BuyerTeam = CASE WHEN ISNULL(FCM.BuyerTeamID,0) > 0 THEN CCT.TeamName ELSE 'R&D' END, 
	                ISNULL(FAC.LabDipNo, '') LabDipNo1, Color.SegmentValue ColorName, FCM.BookingID
                    FROM {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} a
                    INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = a.ConceptID
	                Left JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = a.ColorID
                    Left Join {DbNames.EPYSL}..SampleBookingMaster SBM on SBM.BookingID = FCM.BookingID
                    INNER JOIN {TableNames.DYEING_BATCH_MASTER} DB ON DB.CCColorID = a.CCColorID
                    LEFT JOIN {TableNames.RND_RECIPE_REQ_CHILD} RR ON RR.CCColorID = a.CCColorID And RR.ItemMasterID = FCM.ItemMasterID
                    LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FAC On FAC.BookingChildID = FCM.BookingChildID AND FAC.ConsumptionID = FCM.ConsumptionID AND FAC.BookingID = FCM.BookingID
                    LEFT JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = FCM.BuyerID
                    LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = FCM.BuyerTeamID
                    Left Join FDI On FDI.CCColorID = a.CCColorID
                    where RR.CCColorID IS NULL AND DB.RecipeID = 0 AND FCM.IsBDS <> 0 And ISNULL(SBM.SampleID,0) <> 13
                    Group By a.CCColorID, a.ColorId, a.ColorCode, FCM.GroupConceptNo,
                    FCM.IsBDS, C.ShortName, CCT.TeamName, ISNULL(FAC.LabDipNo, ''),
	                ISNULL(FCM.BuyerID,0),ISNULL(FCM.BuyerTeamID,0),Color.SegmentValue, FCM.BookingID
                    Having Min(Convert(int, ISNULL(FDI.RecipeOn,0))) = 0
                ),
                FBC_B AS
                (
	                SELECT FAC.LabDipNo, T.BookingID
	                FROM {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD_TEXT} T
	                INNER JOIN B MO ON MO.BookingID = T.BookingID
	                INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FAC On FAC.BookingChildID = T.BookingChildID AND FAC.ConsumptionID = T.ConsumptionID AND FAC.BookingID = T.BookingID
	                WHERE T.UsesIn Like '%Body%'
	                GROUP BY FAC.LabDipNo, T.BookingID
                ),
                ReworkList AS
                (
                    SELECT RRM.RecipeReqNo, a.CCColorID, a.ColorId, a.ColorCode, FCM.GroupConceptNo,
                    FCM.IsBDS, LabDipNo1 = '', ExistingDBatchNo = DB.DBatchNo,
                    Buyer = CASE WHEN ISNULL(FCM.BuyerID,0) > 0 THEN C.ShortName ELSE 'R&D' END, 
	                BuyerTeam = CASE WHEN ISNULL(FCM.BuyerTeamID,0) > 0 THEN CCT.TeamName ELSE 'R&D' END, Color.SegmentValue ColorName, FCM.BookingID
                    FROM {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} a
                    INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = a.ConceptID
                    INNER JOIN {TableNames.DYEING_BATCH_MASTER} DB ON DB.CCColorID = a.CCColorID
                    INNER JOIN {TableNames.DYEING_BATCH_REWORK} DBR ON DBR.DBatchID = DB.DBatchID
	                Left JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = a.ColorID
                    LEFT JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = FCM.BuyerID
                    LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = FCM.BuyerTeamID
                    LEFT JOIN {TableNames.RND_RECIPE_REQ_MASTER} RRM ON RRM.GroupConceptNo = FCM.GroupConceptNo AND RRM.CCColorID = a.CCColorID
                    LEFT JOIN {TableNames.RND_RECIPE_DEFINITION_MASTER} RDM ON RDM.RecipeReqMasterID = RRM.RecipeReqMasterID
                    LEFT JOIN {TableNames.DYEING_BATCH_REWORK_RECIPE} DBRR ON DBRR.RecipeReqMasterID = RRM.RecipeReqMasterID
                    WHERE ISNULL(DBR.BatchStatus,0) = 3 AND ISNULL(DBR.IsNewRecipe,0) = 1--AND ISNULL(DBR.IsNewBatch,0) = 1
                    AND DBRR.DBRRID IS NULL AND ISNULL(RDM.IsActive, 0) = 1 AND ISNULL(RDM.IsArchive,0) = 0
                ),
                FBC_ReworkList AS
                (
	                SELECT FAC.LabDipNo, T.BookingID
	                FROM {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD_TEXT} T
	                INNER JOIN ReworkList MO ON MO.BookingID = T.BookingID
	                INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FAC On FAC.BookingChildID = T.BookingChildID AND FAC.ConsumptionID = T.ConsumptionID AND FAC.BookingID = T.BookingID
	                WHERE T.UsesIn Like '%Body%'
	                GROUP BY FAC.LabDipNo, T.BookingID
                ),
                D AS ----Batch
                (
                    SELECT DB.RecipeID, DB.DBatchID,DB.DBatchNo,DB.CCColorID, DB.ColorId, FCC.ColorCode, FCM.GroupConceptNo,FCM.IsBDS, 
	                Buyer = CASE WHEN ISNULL(FCM.BuyerID,0) > 0 THEN C.ShortName ELSE 'R&D' END, 
	                BuyerTeam = CASE WHEN ISNULL(FCM.BuyerTeamID,0) > 0 THEN CCT.TeamName ELSE 'R&D' END,  
	                '' LabDipNo1, Color.SegmentValue ColorName, FCM.BookingID
	                FROM {TableNames.DYEING_BATCH_MASTER} DB
	                INNER JOIN {TableNames.DYEING_BATCH_ITEM} DBI ON DB.DBatchID = DBI.DBatchID
	                LEFT JOIN {TableNames.RND_RECIPE_REQ_MASTER} RRM ON RRM.DBatchID = DB.DBatchID
                    --LEFT JOIN {TableNames.RND_RECIPE_REQ_CHILD} RR ON RR.RecipeReqMasterID = RRM.RecipeReqMasterID
	                INNER JOIN {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} FCC ON FCC.ConceptID = DBI.ConceptID AND FCC.ColorID = DB.ColorID --AND FCC.CCColorID = DB.CCColorID
	                Left JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = FCC.ColorID
	                INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = FCC.ConceptID AND FCM.ItemMasterID = DBI.ItemMasterID
	                Left Join {DbNames.EPYSL}..SampleBookingMaster SBM on SBM.BookingID = FCM.BookingID
	                LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FAC On FAC.BookingChildID = FCM.BookingChildID AND FAC.ConsumptionID = FCM.ConsumptionID AND FAC.BookingID = FCM.BookingID
	                LEFT JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = FCM.BuyerID
	                LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = FCM.BuyerTeamID
	                where DB.RecipeID = 0 AND RRM.RecipeReqMasterID IS NULL
	                GROUP BY DB.RecipeID,DB.DBatchID,DB.DBatchNo,DB.CCColorID, DB.ColorId, FCC.ColorCode, FCM.GroupConceptNo,
	                FCM.IsBDS, ISNULL(FCM.BuyerID,0),C.ShortName, ISNULL(FCM.BuyerTeamID,0), CCT.TeamName, Color.SegmentValue, FCM.BookingID
                ),
                FBC_D AS
                (
	                SELECT FAC.LabDipNo, T.BookingID
	                FROM {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD_TEXT} T
	                INNER JOIN D MO ON MO.BookingID = T.BookingID
	                INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FAC On FAC.BookingChildID = T.BookingChildID AND FAC.ConsumptionID = T.ConsumptionID AND FAC.BookingID = T.BookingID
	                WHERE T.UsesIn Like '%Body%'
	                GROUP BY FAC.LabDipNo, T.BookingID
                ),
                F AS(
                    SELECT 0 DBatchID,'' DBatchNo,C.CCColorID, C.ColorId, C.ColorCode, [Status] = 'New', ExistingDBatchNo = '', RecipeReqNo = '',
                    C.IsBDS, C.GroupConceptNo, C.Buyer, C.BuyerTeam, C.ColorName
	                --,LabDipNo = CASE WHEN ISNULL(FBC.LabDipNo,'') <> '' THEN FBC.LabDipNo ELSE C.LabDipNo1 END
                    FROM C
	                --LEFT JOIN FBC_C FBC ON FBC.BookingID = C.BookingID
	                where GroupConceptNo not in (select GroupConceptNo from D)

                    UNION

                    SELECT 0 DBatchID,'' DBatchNo,B.CCColorID, B.ColorId, B.ColorCode, [Status] = 'New', ExistingDBatchNo = '', RecipeReqNo = '',
                    B.IsBDS, B.GroupConceptNo, B.Buyer, B.BuyerTeam, B.ColorName
	                --,LabDipNo = CASE WHEN ISNULL(FBC.LabDipNo,'') <> '' THEN FBC.LabDipNo ELSE B.LabDipNo1 END
                    FROM B
	                --LEFT JOIN FBC_B FBC ON FBC.BookingID = B.BookingID
	                where GroupConceptNo not in (select GroupConceptNo from D)

                    UNION

                    SELECT D.DBatchID,D.DBatchNo,D.CCColorID, D.ColorId, D.ColorCode, [Status] = 'New', ExistingDBatchNo = '', RecipeReqNo = '',
                    D.IsBDS, D.GroupConceptNo, D.Buyer, D.BuyerTeam, D.ColorName
	                --,LabDipNo = CASE WHEN ISNULL(FBC.LabDipNo,'') <> '' THEN FBC.LabDipNo ELSE D.LabDipNo1 END
                    FROM D
	                --LEFT JOIN FBC_D FBC ON FBC.BookingID = D.BookingID

                    UNION

                    SELECT 0 DBatchID,'' DBatchNo,RL.CCColorID, RL.ColorId, RL.ColorCode, [Status] = 'Rework', RL.ExistingDBatchNo, RL.RecipeReqNo,
                    RL.IsBDS, RL.GroupConceptNo, RL.Buyer, RL.BuyerTeam, RL.ColorName
	                --,LabDipNo = CASE WHEN ISNULL(FBC.LabDipNo,'') <> '' THEN FBC.LabDipNo ELSE RL.LabDipNo1 END
                    FROM ReworkList RL
	                --LEFT JOIN FBC_ReworkList FBC ON FBC.BookingID = RL.BookingID
	                where GroupConceptNo not in (select GroupConceptNo from D)
                )
                SELECT *, Count(*) Over() TotalRows
                FROM F
                ";
                #region Old Code
                /*
                sql = $@"
                ; WITH 
                FDI As
                (
                    Select DI.CCColorID, DI.FiberPartID, Min(Convert(int, ISNULL(DI.RecipeOn, 0))) RecipeOn
                    From RecipeDefinitionDyeingInfo DI
                    LEFT JOIN RecipeDefinitionMaster DM ON DM.RecipeReqMasterID = DI.RecipeReqMasterID
                    WHERE ISNULL(DM.IsArchive, 0) = 0
                    Group By DI.CCColorID, DI.FiberPartID
                ),
                C AS ----Free Concept
                (
                    SELECT a.CCColorID, a.ColorId, a.ColorCode, FCM.GroupConceptNo,
                    FCM.IsBDS, 'R&D' Buyer, 'R&D' BuyerTeam, '' LabDipNo
                    FROM FreeConceptChildColor a
                    INNER JOIN FreeConceptMaster FCM ON FCM.ConceptID = a.ConceptID
                    Left Join {DbNames.EPYSL}..SampleBookingMaster SBM on SBM.BookingID = FCM.BookingID
                    Inner Join KJobCardMaster JC ON JC.ConceptID = FCM.ConceptID
                    LEFT JOIN RecipeRequestChild RR ON RR.CCColorID = a.CCColorID And RR.ItemMasterID = FCM.ItemMasterID
                    Left Join FDI On FDI.CCColorID = a.CCColorID
                    where JC.GrayFabricOK = 1 And RR.CCColorID IS NULL AND FCM.IsBDS = 0 And ISNULL(SBM.SampleID,0) <> 13
                    Group By a.CCColorID, a.ColorId, a.ColorCode, FCM.GroupConceptNo,FCM.IsBDS
                    Having Min(Convert(int, ISNULL(FDI.RecipeOn, 0))) = 0

                ),
                CC AS ----Free Concept Others
                (
                    SELECT a.CCColorID, a.ColorId, a.ColorCode, FCM.GroupConceptNo,
                    FCM.IsBDS, 'R&D' Buyer, 'R&D' BuyerTeam, '' LabDipNo
                    FROM FreeConceptMaster FCM --ON FCM.ConceptID = a.ConceptID
	                INNER JOIN C ON C.GroupConceptNo = FCM.GroupConceptNo
	                LEFT JOIN FreeConceptChildColor a ON a.ConceptID = FCM.ConceptID
                    Left Join {DbNames.EPYSL}..SampleBookingMaster SBM on SBM.BookingID = FCM.BookingID
                    Inner Join KJobCardMaster JC ON JC.ConceptID = FCM.ConceptID
                    LEFT JOIN RecipeRequestChild RR ON RR.ConceptID = FCM.ConceptID
                    Left Join FDI On FDI.CCColorID = a.CCColorID
                    where JC.GrayFabricOK = 1 
	                And RR.RecipeReqChildID IS NULL 
	                AND FCM.IsBDS = 0 
	                And ISNULL(SBM.SampleID,0) <> 13
	                AND FCM.SubGroupID = 1
                    Group By a.CCColorID, a.ColorId, a.ColorCode, FCM.GroupConceptNo,FCM.IsBDS
                    Having Min(Convert(int, ISNULL(FDI.RecipeOn, 0))) = 0
                ),
                B AS ----Sample & Bulk
                (
                    SELECT a.CCColorID, a.ColorId, a.ColorCode, FCM.GroupConceptNo,FCM.IsBDS, 
	                Buyer = CASE WHEN ISNULL(FCM.BuyerID,0) > 0 THEN C.ShortName ELSE 'R&D' END, 
	                BuyerTeam = CASE WHEN ISNULL(FCM.BuyerTeamID,0) > 0 THEN CCT.TeamName ELSE 'R&D' END, 
	                ISNULL(FAC.LabDipNo, '') LabDipNo
                    FROM FreeConceptChildColor a
                    INNER JOIN FreeConceptMaster FCM ON FCM.ConceptID = a.ConceptID
                    Left Join {DbNames.EPYSL}..SampleBookingMaster SBM on SBM.BookingID = FCM.BookingID
                    INNER JOIN DyeingBatchMaster DB ON DB.CCColorID = a.CCColorID
                    LEFT JOIN RecipeRequestChild RR ON RR.CCColorID = a.CCColorID And RR.ItemMasterID = FCM.ItemMasterID
                    LEFT JOIN FBookingAcknowledgeChild FAC On FAC.BookingChildID = FCM.BookingChildID AND FAC.ConsumptionID = FCM.ConsumptionID AND FAC.BookingID = FCM.BookingID
                    LEFT JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = FCM.BuyerID
                    LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = FCM.BuyerTeamID
                    Left Join FDI On FDI.CCColorID = a.CCColorID
                    where RR.CCColorID IS NULL AND DB.RecipeID = 0 AND FCM.IsBDS <> 0 And ISNULL(SBM.SampleID,0) <> 13
                    Group By a.CCColorID, a.ColorId, a.ColorCode, FCM.GroupConceptNo,
                    FCM.IsBDS, C.ShortName, CCT.TeamName, ISNULL(FAC.LabDipNo, ''),
	                ISNULL(FCM.BuyerID,0),ISNULL(FCM.BuyerTeamID,0)
                    Having Min(Convert(int, ISNULL(FDI.RecipeOn,0))) = 0
                ),
                ReworkList AS
                (
                    SELECT RRM.RecipeReqNo, a.CCColorID, a.ColorId, a.ColorCode, FCM.GroupConceptNo,
                    FCM.IsBDS, LabDipNo = '', ExistingDBatchNo = DB.DBatchNo,
                    Buyer = CASE WHEN ISNULL(FCM.BuyerID,0) > 0 THEN C.ShortName ELSE 'R&D' END, 
	                BuyerTeam = CASE WHEN ISNULL(FCM.BuyerTeamID,0) > 0 THEN CCT.TeamName ELSE 'R&D' END
                    FROM FreeConceptChildColor a
                    INNER JOIN FreeConceptMaster FCM ON FCM.ConceptID = a.ConceptID
                    INNER JOIN DyeingBatchMaster DB ON DB.CCColorID = a.CCColorID
                    INNER JOIN DyeingBatchRework DBR ON DBR.DBatchID = DB.DBatchID
                    LEFT JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = FCM.BuyerID
                    LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = FCM.BuyerTeamID
                    LEFT JOIN RecipeRequestMaster RRM ON RRM.GroupConceptNo = FCM.GroupConceptNo AND RRM.CCColorID = a.CCColorID
                    LEFT JOIN RecipeDefinitionMaster RDM ON RDM.RecipeReqMasterID = RRM.RecipeReqMasterID
                    LEFT JOIN DyeingBatchReworkRecipe DBRR ON DBRR.RecipeReqMasterID = RRM.RecipeReqMasterID
                    WHERE ISNULL(DBR.BatchStatus,0) = 3 AND ISNULL(DBR.IsNewRecipe,0) = 1--AND ISNULL(DBR.IsNewBatch,0) = 1
                    AND DBRR.DBRRID IS NULL AND ISNULL(RDM.IsActive, 0) = 1 AND ISNULL(RDM.IsArchive,0) = 0
                ),
                D AS ----Batch
                (
                    SELECT DB.RecipeID, DB.DBatchID,DB.DBatchNo,DB.CCColorID, DB.ColorId, FCC.ColorCode, FCM.GroupConceptNo,FCM.IsBDS, 
	                Buyer = CASE WHEN ISNULL(FCM.BuyerID,0) > 0 THEN C.ShortName ELSE 'R&D' END, 
	                BuyerTeam = CASE WHEN ISNULL(FCM.BuyerTeamID,0) > 0 THEN CCT.TeamName ELSE 'R&D' END,  
	                '' LabDipNo
	                FROM DyeingBatchMaster DB
	                INNER JOIN DyeingBatchItem DBI ON DB.DBatchID = DBI.DBatchID
	                LEFT JOIN RecipeRequestMaster RRM ON RRM.DBatchID = DB.DBatchID
                    --LEFT JOIN RecipeRequestChild RR ON RR.RecipeReqMasterID = RRM.RecipeReqMasterID
	                INNER JOIN FreeConceptChildColor FCC ON FCC.ConceptID = DBI.ConceptID AND FCC.ColorID = DB.ColorID --AND FCC.CCColorID = DB.CCColorID
	                INNER JOIN FreeConceptMaster FCM ON FCM.ConceptID = FCC.ConceptID AND FCM.ItemMasterID = DBI.ItemMasterID
	                Left Join {DbNames.EPYSL}..SampleBookingMaster SBM on SBM.BookingID = FCM.BookingID
	                LEFT JOIN FBookingAcknowledgeChild FAC On FAC.BookingChildID = FCM.BookingChildID AND FAC.ConsumptionID = FCM.ConsumptionID AND FAC.BookingID = FCM.BookingID
	                LEFT JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = FCM.BuyerID
	                LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = FCM.BuyerTeamID
	                where DB.RecipeID = 0 AND RRM.RecipeReqMasterID IS NULL
	                GROUP BY DB.RecipeID,DB.DBatchID,DB.DBatchNo,DB.CCColorID, DB.ColorId, FCC.ColorCode, FCM.GroupConceptNo,
	                FCM.IsBDS, ISNULL(FCM.BuyerID,0),C.ShortName, ISNULL(FCM.BuyerTeamID,0), CCT.TeamName
                ),
                F AS(
                    SELECT 0 DBatchID,'' DBatchNo,C.CCColorID, C.ColorId, C.ColorCode, [Status] = 'New', ExistingDBatchNo = '', RecipeReqNo = '',
                    C.IsBDS, C.GroupConceptNo, C.Buyer, C.BuyerTeam, C.LabDipNo
                    FROM C where GroupConceptNo not in (select GroupConceptNo from D)
                    UNION
                    SELECT 0 DBatchID,'' DBatchNo,CC.CCColorID, CC.ColorId, CC.ColorCode, [Status] = 'New', ExistingDBatchNo = '', RecipeReqNo = '',
                    CC.IsBDS, CC.GroupConceptNo, CC.Buyer, CC.BuyerTeam, CC.LabDipNo
                    FROM CC where GroupConceptNo not in (select GroupConceptNo from D)
                    UNION
                    SELECT 0 DBatchID,'' DBatchNo,B.CCColorID, B.ColorId, B.ColorCode, [Status] = 'New', ExistingDBatchNo = '', RecipeReqNo = '',
                    B.IsBDS, B.GroupConceptNo, B.Buyer, B.BuyerTeam, B.LabDipNo
                    FROM B where GroupConceptNo not in (select GroupConceptNo from D)
                    UNION
                    SELECT D.DBatchID,D.DBatchNo,D.CCColorID, D.ColorId, D.ColorCode, [Status] = 'New', ExistingDBatchNo = '', RecipeReqNo = '',
                    D.IsBDS, D.GroupConceptNo, D.Buyer, D.BuyerTeam, D.LabDipNo
                    FROM D
                    UNION
                    SELECT 0 DBatchID,'' DBatchNo,RL.CCColorID, RL.ColorId, RL.ColorCode, [Status] = 'Rework', RL.ExistingDBatchNo, RL.RecipeReqNo,
                    RL.IsBDS, RL.GroupConceptNo, RL.Buyer, RL.BuyerTeam, RL.LabDipNo
                    FROM ReworkList RL where GroupConceptNo not in (select GroupConceptNo from D)
                ),
                ALL_DATA AS(
                    SELECT F.DBatchID, F.DBatchNo, F.CCColorID, F.ColorId, F.ColorCode, Color.SegmentValue ColorName, F.[Status], F.ExistingDBatchNo, F.RecipeReqNo,
                    F.IsBDS, F.GroupConceptNo, F.Buyer, F.BuyerTeam, F.LabDipNo
                    FROM F
                    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = F.ColorID
                    GROUP BY F.DBatchID,F.DBatchNo,F.CCColorID, F.ColorId, F.ColorCode, Color.SegmentValue,F.[Status], F.ExistingDBatchNo, F.RecipeReqNo,
                    F.IsBDS, F.GroupConceptNo, F.Buyer, F.BuyerTeam, F.LabDipNo
                )
                
                SELECT *, Count(*) Over() TotalRows
                FROM F";
                */

                /*
                sql = $@"
                ;WITH FDI As(
                    Select DI.CCColorID, DI.FiberPartID, Min(Convert(int,ISNULL(DI.RecipeOn,0))) RecipeOn
                    From RecipeDefinitionDyeingInfo DI
                 LEFT JOIN RecipeDefinitionMaster DM ON DM.RecipeReqMasterID = DI.RecipeReqMasterID
                 WHERE ISNULL(DM.IsArchive,0) = 0
                    Group By DI.CCColorID, DI.FiberPartID

                ),C AS (----Free Concept
                    SELECT a.CCColorID, a.ColorId, a.ColorCode, a.ConceptID, FCM.GroupConceptNo, FCM.ConceptDate, FCM.TrialNo, FCM.TrialDate, FCM.ConceptFor, FCM.KnittingTypeID, FCM.ConstructionID,
                    FCM.CompositionID, FCM.GSMID, FCM.Qty, FCM.ConceptStatusID, FCM.TechnicalNameId, FCM.AddedBy, FCM.ItemMasterID, FCM.IsBDS, 'R&D' Buyer, 'R&D' BuyerTeam, '' LabDipNo
                    FROM FreeConceptChildColor a
                    INNER JOIN FreeConceptMaster FCM ON FCM.ConceptID = a.ConceptID
                    Left Join {DbNames.EPYSL}..SampleBookingMaster SBM on SBM.BookingID = FCM.BookingID
                    Inner Join KJobCardMaster JC ON JC.ConceptID = FCM.ConceptID
                    LEFT JOIN RecipeRequestChild RR ON RR.CCColorID = a.CCColorID And RR.ItemMasterID = FCM.ItemMasterID
                    Left Join FDI On FDI.CCColorID = a.CCColorID
                    where JC.GrayFabricOK = 1 And RR.CCColorID IS NULL AND FCM.IsBDS = 0 And ISNULL(SBM.SampleID,0) <> 13
                    Group By a.CCColorID, a.ColorId, a.ColorCode, a.ConceptID, FCM.GroupConceptNo, FCM.ConceptDate, FCM.TrialNo, FCM.TrialDate, FCM.ConceptFor, FCM.KnittingTypeID, FCM.ConstructionID,
                    FCM.CompositionID, FCM.GSMID, FCM.Qty, FCM.ConceptStatusID, FCM.TechnicalNameId, FCM.AddedBy, FCM.ItemMasterID, FCM.IsBDS
                    Having Min(Convert(int,ISNULL(FDI.RecipeOn,0))) = 0
                ),B AS (----Sample & Bulk
                    SELECT a.CCColorID, a.ColorId, a.ColorCode, a.ConceptID, FCM.GroupConceptNo, FCM.ConceptDate, FCM.TrialNo, FCM.TrialDate, FCM.ConceptFor, FCM.KnittingTypeID, FCM.ConstructionID,
                    FCM.CompositionID, FCM.GSMID, FCM.Qty, FCM.ConceptStatusID, FCM.TechnicalNameId, FCM.AddedBy, FCM.ItemMasterID, FCM.IsBDS, C.ShortName Buyer, CCT.TeamName BuyerTeam, ISNULL(FAC.LabDipNo,'') LabDipNo
                    FROM FreeConceptChildColor a
                    INNER JOIN FreeConceptMaster FCM ON FCM.ConceptID = a.ConceptID
                    Left Join {DbNames.EPYSL}..SampleBookingMaster SBM on SBM.BookingID = FCM.BookingID
                    INNER JOIN DyeingBatchMaster DB ON DB.CCColorID = a.CCColorID
                    LEFT JOIN RecipeRequestChild RR ON RR.CCColorID = a.CCColorID And RR.ItemMasterID = FCM.ItemMasterID
                    LEFT JOIN FBookingAcknowledgeChild FAC On FAC.BookingChildID = FCM.BookingChildID AND FAC.ConsumptionID = FCM.ConsumptionID AND FAC.BookingID = FCM.BookingID
                    LEFT JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = FCM.BuyerID
                    LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = FCM.BuyerTeamID
                    Left Join FDI On FDI.CCColorID = a.CCColorID
                    where RR.CCColorID IS NULL AND DB.RecipeID = 0 AND FCM.IsBDS <> 0 And ISNULL(SBM.SampleID,0) <> 13
                    Group By a.CCColorID, a.ColorId, a.ColorCode, a.ConceptID, FCM.GroupConceptNo, FCM.ConceptDate, FCM.TrialNo, FCM.TrialDate, FCM.ConceptFor, FCM.KnittingTypeID, FCM.ConstructionID,
                    FCM.CompositionID, FCM.GSMID, FCM.Qty, FCM.ConceptStatusID, FCM.TechnicalNameId, FCM.AddedBy, FCM.ItemMasterID, FCM.IsBDS, C.ShortName, CCT.TeamName, ISNULL(FAC.LabDipNo,'')
                    Having Min(Convert(int,ISNULL(FDI.RecipeOn,0))) = 0
                ),
                ReworkList AS
                (
                 SELECT RRM.RecipeReqNo, a.CCColorID, a.ColorId, a.ColorCode, a.ConceptID, FCM.GroupConceptNo, FCM.ConceptDate, FCM.TrialNo, FCM.TrialDate, FCM.ConceptFor, FCM.KnittingTypeID, FCM.ConstructionID,
                    FCM.CompositionID, FCM.GSMID, FCM.Qty, FCM.ConceptStatusID, FCM.TechnicalNameId, FCM.AddedBy, FCM.ItemMasterID, FCM.IsBDS, LabDipNo = '', ExistingDBatchNo = DB.DBatchNo
                 ,Buyer = CASE WHEN FCM.BuyerID > 0 THEN C.ShortName ELSE 'R&D' END
                 ,BuyerTeam = CASE WHEN FCM.BuyerTeamID > 0 THEN CCT.TeamName ELSE 'R&D' END
                 FROM FreeConceptChildColor a
                 INNER JOIN FreeConceptMaster FCM ON FCM.ConceptID = a.ConceptID
                 INNER JOIN DyeingBatchMaster DB ON DB.CCColorID = a.CCColorID
                 INNER JOIN DyeingBatchRework DBR ON DBR.DBatchID = DB.DBatchID
                 LEFT JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = FCM.BuyerID
                 LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = FCM.BuyerTeamID
                    LEFT JOIN RecipeRequestMaster RRM ON RRM.GroupConceptNo = FCM.GroupConceptNo AND RRM.CCColorID = a.CCColorID
                    LEFT JOIN RecipeDefinitionMaster RDM ON RDM.RecipeReqMasterID = RRM.RecipeReqMasterID
                    LEFT JOIN DyeingBatchReworkRecipe DBRR ON DBRR.RecipeReqMasterID = RRM.RecipeReqMasterID
                    WHERE ISNULL(DBR.BatchStatus,0) = 3 AND ISNULL(DBR.IsNewRecipe,0) = 1 --AND ISNULL(DBR.IsNewBatch,0) = 1
                    AND DBRR.DBRRID IS NULL AND ISNULL(RDM.IsActive,0) = 1 AND ISNULL(RDM.IsArchive,0) = 0
                ),
                F AS (
                    SELECT C.CCColorID, C.ColorId, C.ColorCode, C.ConceptDate, C.TrialNo, C.TrialDate, [Status] = 'New', ExistingDBatchNo = '', RecipeReqNo = '',
                    C.ConceptFor, C.KnittingTypeID, C.ConstructionID, C.CompositionID, C.GSMID, C.Qty, C.ConceptStatusID, C.IsBDS, C.GroupConceptNo, C.Buyer, C.BuyerTeam, C.LabDipNo
                    FROM C
                    UNION
                    SELECT B.CCColorID, B.ColorId, B.ColorCode, B.ConceptDate, B.TrialNo, B.TrialDate, [Status] = 'New', ExistingDBatchNo = '', RecipeReqNo = '',
                    B.ConceptFor, B.KnittingTypeID, B.ConstructionID, B.CompositionID, B.GSMID, B.Qty, B.ConceptStatusID, B.IsBDS, B.GroupConceptNo, B.Buyer, B.BuyerTeam, B.LabDipNo
                    FROM B
                 UNION
                    SELECT RL.CCColorID, RL.ColorId, RL.ColorCode, RL.ConceptDate, RL.TrialNo, RL.TrialDate, [Status] = 'Rework', RL.ExistingDBatchNo, RL.RecipeReqNo,
                    RL.ConceptFor, RL.KnittingTypeID, RL.ConstructionID, RL.CompositionID, RL.GSMID, RL.Qty, RL.ConceptStatusID, RL.IsBDS, RL.GroupConceptNo, RL.Buyer, RL.BuyerTeam, RL.LabDipNo
                    FROM ReworkList RL
                ),
                ALL_DATA AS (
                 SELECT F.CCColorID, F.ColorId, F.ColorCode, Color.SegmentValue ColorName, F.ConceptDate, F.TrialNo, F.TrialDate, F.[Status], F.ExistingDBatchNo, F.RecipeReqNo,
                 F.ConceptFor, F.KnittingTypeID, F.ConstructionID, F.CompositionID, F.GSMID, F.Qty, F.ConceptStatusID, F.IsBDS, F.GroupConceptNo, F.Buyer, F.BuyerTeam, F.LabDipNo
                 FROM F
                 INNER JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = F.ColorID
                 GROUP BY F.CCColorID, F.ColorId, F.ColorCode, Color.SegmentValue, F.ConceptDate, F.TrialNo, F.TrialDate,F.[Status], F.ExistingDBatchNo, F.RecipeReqNo,
                 F.ConceptFor, F.KnittingTypeID, F.ConstructionID, F.CompositionID, F.GSMID, F.Qty, F.ConceptStatusID, F.IsBDS, F.GroupConceptNo, F.Buyer, F.BuyerTeam, F.LabDipNo
                )
                SELECT *, Count(*) Over() TotalRows
                FROM ALL_DATA";
                */
                #endregion
            }
            else if (status == Status.Completed)
            {
                sql = $@"
                ;With C As (
	                SELECT RR.*, RecipeStatus = CASE WHEN RDM.IsActive = 0 THEN 'Deactive' ELSE 'Active' END, CC.ConceptID, CC.ColorCode, 
	                Replace(ISNULL(C.ShortName,''),'Select','R&D') Buyer, ISNULL(CCT.TeamName,'R&D') 
	                BuyerTeam, ISNULL(FAC.LabDipNo,'') LabDipNo1, FCM.BookingID
	                FROM {TableNames.RND_RECIPE_REQ_MASTER} RR
	                INNER JOIN {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} CC ON CC.CCColorID = RR.CCColorID
	                INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = CC.ConceptID
	                LEFT JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = FCM.BuyerID
	                LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = FCM.BuyerTeamID
	                LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FAC On FAC.BookingChildID = FCM.BookingChildID AND FAC.ConsumptionID = FCM.ConsumptionID AND FAC.BookingID = FCM.BookingID
	                LEFT JOIN {TableNames.RND_RECIPE_DEFINITION_MASTER} RDM ON RDM.RecipeReqMasterID = RR.RecipeReqMasterID
                    WHERE RR.Acknowledge = 0 AND RR.UnAcknowledge=0
                ),
                FBC AS
                (
	                SELECT FAC.LabDipNo, T.BookingID
	                FROM {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD_TEXT} T
	                INNER JOIN C MO ON MO.BookingID = T.BookingID
	                INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FAC On FAC.BookingChildID = T.BookingChildID AND FAC.ConsumptionID = T.ConsumptionID AND FAC.BookingID = T.BookingID
	                WHERE T.UsesIn Like '%Body%'
	                GROUP BY FAC.LabDipNo, T.BookingID
                ),
                F AS (
                    SELECT C.RecipeReqMasterID, C.RecipeReqNo, C.RecipeReqDate, CONVERT(varchar, C.RecipeReqDate,103) + ' ' + CONVERT(varchar, C.DateAdded, 108) RecipeReqTime,C.GroupConceptNo ConceptNo, C.GroupConceptNo, C.PreProcessRevNo, C.RevisionNo, C.RevisionDate, C.RevisionBy,
	                C.RevisionReason, C.CCColorID, C.ColorID, C.DPID, C.Remarks, C.IsBDS, C.IsBulkBooking, C.Acknowledge, C.AcknowledgeBy, C.AcknowledgeDate,
	                C.ConceptID, C.ColorCode, C.RecipeStatus, Color.SegmentValue ColorName, DP.DPName, C.DPProcessInfo, C.Approved, C.Buyer, C.BuyerTeam,C.RecipeFor,
	                LabDipNo = CASE WHEN ISNULL(FBC.LabDipNo,'') <> '' THEN FBC.LabDipNo ELSE C.LabDipNo1 END
                    FROM C
	                LEFT JOIN FBC ON FBC.BookingID = C.BookingID
                    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = C.ColorID
                    LEFT JOIN {TableNames.DyeingProcessPart_HK} DP ON DP.DPID=C.DPID
	                GROUP BY C.RecipeReqMasterID, C.RecipeReqNo, C.RecipeReqDate, C.DateAdded,C.GroupConceptNo, C.PreProcessRevNo, C.RevisionNo, C.RevisionDate, C.RevisionBy,
	                C.RevisionReason, C.CCColorID, C.ColorID, C.DPID, C.Remarks, C.IsBDS, C.IsBulkBooking, C.Acknowledge, C.AcknowledgeBy, C.AcknowledgeDate,
	                C.ConceptID, C.ColorCode, C.RecipeStatus, Color.SegmentValue, DP.DPName, C.DPProcessInfo, C.Approved, C.Buyer, C.BuyerTeam, C.RecipeFor,
	                CASE WHEN ISNULL(FBC.LabDipNo,'') <> '' THEN FBC.LabDipNo ELSE C.LabDipNo1 END
                )
                SELECT *, Count(*) Over() TotalRows FROM F";
                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By F.RecipeReqMasterID Desc" : paginationInfo.OrderBy;
            }

            else if (status == Status.UnAcknowledge)
            {
                sql = $@"
                ;With C As (
	                SELECT RR.*, RecipeStatus = CASE WHEN RDM.IsActive = 0 THEN 'Deactive' ELSE 'Active' END, CC.ConceptID, CC.ColorCode, 
	                Replace(ISNULL(C.ShortName,''),'Select','R&D') Buyer, ISNULL(CCT.TeamName,'R&D') 
	                BuyerTeam, ISNULL(FAC.LabDipNo,'') LabDipNo1, FCM.BookingID
	                FROM {TableNames.RND_RECIPE_REQ_MASTER} RR
	                INNER JOIN {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} CC ON CC.CCColorID = RR.CCColorID
	                INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = CC.ConceptID
	                LEFT JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = FCM.BuyerID
	                LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = FCM.BuyerTeamID
	                LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FAC On FAC.BookingChildID = FCM.BookingChildID AND FAC.ConsumptionID = FCM.ConsumptionID AND FAC.BookingID = FCM.BookingID
	                LEFT JOIN {TableNames.RND_RECIPE_DEFINITION_MASTER} RDM ON RDM.RecipeReqMasterID = RR.RecipeReqMasterID
                   WHERE RR.Acknowledge = 0 AND RR.UnAcknowledge = 1
                ),
                FBC AS
                (
	                SELECT FAC.LabDipNo, T.BookingID
	                FROM {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD_TEXT} T
	                INNER JOIN C MO ON MO.BookingID = T.BookingID
	                INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FAC On FAC.BookingChildID = T.BookingChildID AND FAC.ConsumptionID = T.ConsumptionID AND FAC.BookingID = T.BookingID
	                WHERE T.UsesIn Like '%Body%'
	                GROUP BY FAC.LabDipNo, T.BookingID
                ),
                F AS (
                    SELECT C.RecipeReqMasterID, C.RecipeReqNo, C.RecipeReqDate, CONVERT(varchar, C.RecipeReqDate,103) + ' ' + CONVERT(varchar, C.DateAdded, 108) RecipeReqTime,C.GroupConceptNo ConceptNo, C.GroupConceptNo, C.PreProcessRevNo, C.RevisionNo, C.RevisionDate, C.RevisionBy,
	                C.RevisionReason, C.CCColorID, C.ColorID, C.DPID, C.Remarks, C.IsBDS, C.IsBulkBooking, C.Acknowledge, C.AcknowledgeBy, C.AcknowledgeDate,
	                C.ConceptID, C.ColorCode, C.RecipeStatus, Color.SegmentValue ColorName, DP.DPName, C.DPProcessInfo, C.Approved, C.Buyer, C.BuyerTeam,C.RecipeFor,
	                LabDipNo = CASE WHEN ISNULL(FBC.LabDipNo,'') <> '' THEN FBC.LabDipNo ELSE C.LabDipNo1 END
                    FROM C
	                LEFT JOIN FBC ON FBC.BookingID = C.BookingID
                    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = C.ColorID
                    LEFT JOIN {TableNames.DyeingProcessPart_HK} DP ON DP.DPID=C.DPID
	                GROUP BY C.RecipeReqMasterID, C.RecipeReqNo, C.RecipeReqDate, C.DateAdded,C.GroupConceptNo, C.PreProcessRevNo, C.RevisionNo, C.RevisionDate, C.RevisionBy,
	                C.RevisionReason, C.CCColorID, C.ColorID, C.DPID, C.Remarks, C.IsBDS, C.IsBulkBooking, C.Acknowledge, C.AcknowledgeBy, C.AcknowledgeDate,
	                C.ConceptID, C.ColorCode, C.RecipeStatus, Color.SegmentValue, DP.DPName, C.DPProcessInfo, C.Approved, C.Buyer, C.BuyerTeam, C.RecipeFor,
	                CASE WHEN ISNULL(FBC.LabDipNo,'') <> '' THEN FBC.LabDipNo ELSE C.LabDipNo1 END
                )
                SELECT *, Count(*) Over() TotalRows FROM F";
                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By F.RecipeReqMasterID Desc" : paginationInfo.OrderBy;
            }
            else
            {
                sql = $@"
                ;With C As (
	                SELECT RR.*, RecipeStatus = CASE WHEN RDM.IsActive = 0 THEN 'Deactive' ELSE 'Active' END, CC.ConceptID, CC.ColorCode, 
	                Replace(ISNULL(C.ShortName,''),'Select','R&D') Buyer, ISNULL(CCT.TeamName,'R&D') 
	                BuyerTeam, ISNULL(FAC.LabDipNo,'') LabDipNo1, FCM.BookingID
	                FROM {TableNames.RND_RECIPE_REQ_MASTER} RR
	                INNER JOIN {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} CC ON CC.CCColorID = RR.CCColorID
	                INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = CC.ConceptID
	                LEFT JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = FCM.BuyerID
	                LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = FCM.BuyerTeamID
	                LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FAC On FAC.BookingChildID = FCM.BookingChildID AND FAC.ConsumptionID = FCM.ConsumptionID AND FAC.BookingID = FCM.BookingID
	                LEFT JOIN {TableNames.RND_RECIPE_DEFINITION_MASTER} RDM ON RDM.RecipeReqMasterID = RR.RecipeReqMasterID
                     WHERE RR.Acknowledge = 1
                ),
                FBC AS
                (
	                SELECT FAC.LabDipNo, T.BookingID
	                FROM {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD_TEXT} T
	                INNER JOIN C MO ON MO.BookingID = T.BookingID
	                INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FAC On FAC.BookingChildID = T.BookingChildID AND FAC.ConsumptionID = T.ConsumptionID AND FAC.BookingID = T.BookingID
	                WHERE T.UsesIn Like '%Body%'
	                GROUP BY FAC.LabDipNo, T.BookingID
                ),
                F AS (
                    SELECT C.RecipeReqMasterID, C.RecipeReqNo, C.RecipeReqDate, CONVERT(varchar, C.RecipeReqDate,103) + ' ' + CONVERT(varchar, C.DateAdded, 108) RecipeReqTime,C.GroupConceptNo ConceptNo, C.GroupConceptNo, C.PreProcessRevNo, C.RevisionNo, C.RevisionDate, C.RevisionBy,
	                C.RevisionReason, C.CCColorID, C.ColorID, C.DPID, C.Remarks, C.IsBDS, C.IsBulkBooking, C.Acknowledge, C.AcknowledgeBy, C.AcknowledgeDate,
	                C.ConceptID, C.ColorCode, C.RecipeStatus, Color.SegmentValue ColorName, DP.DPName, C.DPProcessInfo, C.Approved, C.Buyer, C.BuyerTeam,C.RecipeFor,
	                LabDipNo = CASE WHEN ISNULL(FBC.LabDipNo,'') <> '' THEN FBC.LabDipNo ELSE C.LabDipNo1 END
                    FROM C
	                LEFT JOIN FBC ON FBC.BookingID = C.BookingID
                    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = C.ColorID
                    LEFT JOIN {TableNames.DyeingProcessPart_HK} DP ON DP.DPID=C.DPID
	                GROUP BY C.RecipeReqMasterID, C.RecipeReqNo, C.RecipeReqDate, C.DateAdded,C.GroupConceptNo, C.PreProcessRevNo, C.RevisionNo, C.RevisionDate, C.RevisionBy,
	                C.RevisionReason, C.CCColorID, C.ColorID, C.DPID, C.Remarks, C.IsBDS, C.IsBulkBooking, C.Acknowledge, C.AcknowledgeBy, C.AcknowledgeDate,
	                C.ConceptID, C.ColorCode, C.RecipeStatus, Color.SegmentValue, DP.DPName, C.DPProcessInfo, C.Approved, C.Buyer, C.BuyerTeam, C.RecipeFor,
	                CASE WHEN ISNULL(FBC.LabDipNo,'') <> '' THEN FBC.LabDipNo ELSE C.LabDipNo1 END
                )
                SELECT *, Count(*) Over() TotalRows FROM F";
                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By F.RecipeReqMasterID Desc" : paginationInfo.OrderBy;
            }


            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<RecipeRequestMaster>(sql);
        }

        public async Task<RecipeRequestMaster> GetNewAsync(int ccolorId, string grpConceptNo, int isBDS, int DBatchID = 0)
        {
            string strBDS = isBDS == 1 ? " M.ConceptNo" : " M.GroupConceptNo";

            string dyeingQuery = "";
            /*
            if (isBDS == 1)
            {
                dyeingQuery = $@"
                        SELECT DB.DBatchID, DB.DBatchNo, DB.DBatchDate, DB.CCColorID, DB.ColorID, COL.SegmentValue ColorName,FCM.GroupConceptNo [ConceptNo]
                        FROM DyeingBatchMaster DB 
						LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID = DB.ColorID
                        LEFT JOIN FreeConceptChildColor FCC ON FCC.CCColorID = DB.CCColorID
                        LEFT JOIN FreeConceptMaster FCM ON FCM.ConceptID = FCC.ConceptID
                        WHERE DB.RecipeID = 0 
                        ORDER BY ConceptNo, ColorName, DBatchNo
                        ";
            }
            */
            var query = "";
            if (DBatchID == 0)
            {
                query =
                   $@"
                SELECT C.CCColorID, C.ColorID ,M.ConceptID,M.GroupConceptNo ConceptNo,M.GroupConceptNo,M.ConceptDate, C.ColorCode,Color.SegmentValue ColorName,
				C.DPID,C.DPProcessInfo,DP.DPName, C.Remarks, Composition.SegmentValue Composition, M.IsBDS, Replace(ISNULL(CT.ShortName,''),'Select','R&D') Buyer, ISNULL(CCT.TeamName,'R&D') BuyerTeam, ISNULL(FAC.LabDipNo,'') LabDipNo,M.BuyerID
                FROM {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} C
				LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} M ON M.ConceptID = C.ConceptID
				LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = C.ColorID
				LEFT JOIN {TableNames.DyeingProcessPart_HK} DP ON DP.DPID=C.DPID
				LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID=M.CompositionID
	            LEFT JOIN {DbNames.EPYSL}..Contacts CT ON CT.ContactID = M.BuyerID
	            LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = M.BuyerTeamID
                LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FAC On FAC.BookingChildID = M.BookingChildID AND FAC.ConsumptionID = M.ConsumptionID AND FAC.BookingID = M.BookingID
				WHERE C.CCColorID={ccolorId};
                
                /*
                ;With M AS (
				    SELECT C.CCColorID, C.ColorID ,M.ConceptID,M.GroupConceptNo ConceptNo,M.GroupConceptNo,M.ConceptDate, C.ColorCode,Color.SegmentValue ColorName,
				    C.DPID,C.DPProcessInfo,DP.DPName, C.Remarks, Composition.SegmentValue Composition, M.IsBDS, Replace(ISNULL(CT.ShortName,''),'Select','R&D') Buyer, ISNULL(CCT.TeamName,'R&D') BuyerTeam, ISNULL(FAC.LabDipNo,'') LabDipNo,
				    M.Qty,M.ItemMasterId,M.SubGroupID,M.KnittingTypeID,M.CompositionID,M.ConstructionID,M.GSMID,M.TechnicalNameId
                    FROM {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} C
				    LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} M ON M.ConceptID = C.ConceptID
				    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = C.ColorID
				    LEFT JOIN {TableNames.DyeingProcessPart_HK} DP ON DP.DPID=C.DPID
				    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID=M.CompositionID
	                LEFT JOIN {DbNames.EPYSL}..Contacts CT ON CT.ContactID = M.BuyerID
	                LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = M.BuyerTeamID
                    LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FAC On FAC.BookingChildID = M.BookingChildID AND FAC.ConsumptionID = M.ConsumptionID AND FAC.BookingID = M.BookingID
				    INNER JOIN {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} CC ON CC.ColorID = C.ColorID
		            WHERE CC.CCColorID = {ccolorId}
				)

                SELECT M.ConceptID, M.ConceptNo, M.ConceptDate, KnittingType.TypeName KnittingType, Composition.SegmentValue Composition, M.ConstructionID,
				Construction.SegmentValue Construction, Gsm.SegmentValue GSM, M.SubGroupID SubGroupID, SG.SubGroupName SubGroup, M.TechnicalNameId,
				TT.TechnicalName,M.Qty, M.ItemMasterId
                --FROM {TableNames.RND_FREE_CONCEPT_MASTER} M
				FROM M
                LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID=M.KnittingTypeID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID=M.CompositionID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID=M.ConstructionID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID=M.GSMID
                LEFT JOIN {DbNames.EPYSL}..ItemSubGroup SG ON SG.SubGroupID = M.SubGroupID
                LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME} TT ON TT.TechnicalNameId = M.TechnicalNameId
                WHERE {strBDS}='{grpConceptNo}';
                */

                ;With M AS (
	                SELECT M.GroupConceptNo, C.ColorID, C.DPID, C.CCColorID, C.ColorCode, C.DPProcessInfo, C.Remarks
                    FROM {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} C
	                LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} M ON M.ConceptID = C.ConceptID
	                WHERE C.CCColorID = {ccolorId}
                ),
                AllFabrics AS
                (
	                SELECT M.CCColorID, M.ColorID ,FCM.ConceptID, FCM.GroupConceptNo ConceptNo, FCM.GroupConceptNo, FCM.ConceptDate, 
	                M.ColorCode, Color.SegmentValue ColorName,
	                M.DPID, M.DPProcessInfo, DP.DPName, M.Remarks, Composition.SegmentValue Composition, FCM.IsBDS, Replace(ISNULL(CT.ShortName,''),'Select','R&D') Buyer, ISNULL(CCT.TeamName,'R&D') BuyerTeam, ISNULL(FAC.LabDipNo,'') LabDipNo,
	                FCM.Qty,FCM.ItemMasterId,FCM.SubGroupID,FCM.KnittingTypeID,FCM.CompositionID,FCM.ConstructionID,FCM.GSMID,FCM.TechnicalNameId
	                FROM {TableNames.RND_FREE_CONCEPT_MASTER} FCM
	                INNER JOIN M ON M.GroupConceptNo = FCM.GroupConceptNo
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = M.ColorID
	                LEFT JOIN {TableNames.DyeingProcessPart_HK} DP ON DP.DPID = M.DPID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = FCM.CompositionID
	                LEFT JOIN {DbNames.EPYSL}..Contacts CT ON CT.ContactID = FCM.BuyerID
	                LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = FCM.BuyerTeamID
                    LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FAC On FAC.BookingChildID = FCM.BookingChildID AND FAC.ConsumptionID = FCM.ConsumptionID AND FAC.BookingID = FCM.BookingID
	                WHERE FCM.SubGroupID = 1
                )
                SELECT M.ConceptID, M.ConceptNo, M.ConceptDate, KnittingType.TypeName KnittingType, Composition.SegmentValue Composition, M.ConstructionID,
                Construction.SegmentValue Construction, Gsm.SegmentValue GSM, M.SubGroupID SubGroupID, SG.SubGroupName SubGroup, M.TechnicalNameId,
                TT.TechnicalName,M.Qty, M.ItemMasterId
                --FROM {TableNames.RND_FREE_CONCEPT_MASTER} M
                FROM AllFabrics M
                LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID=M.KnittingTypeID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID=M.CompositionID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID=M.ConstructionID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = M.GSMID
                LEFT JOIN {DbNames.EPYSL}..ItemSubGroup SG ON SG.SubGroupID = M.SubGroupID
                LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME} TT ON TT.TechnicalNameId = M.TechnicalNameId
                WHERE {strBDS}='{grpConceptNo}';

                --Dyeing Process
                ;SELECT CAST(DPID AS VARCHAR) AS id, DPName AS text
                FROM {TableNames.DyeingProcessPart_HK};

                --RecipeDefinitionDyeingInfo
                ;SELECT DI.*, EV.ValueName FiberPart, ISV.SegmentValue ColorName
                FROM {TableNames.RND_RECIPE_DEFINITION_DYEING_INFO} DI
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID = DI.FiberPartID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = DI.ColorID
                WHERE CCColorID = {ccolorId};

                --Recipe Definition Item
                SELECT RC.RecipeReqChildID, RC.RecipeReqMasterID, RC.ConceptID, RC.BookingID, RC.ItemMasterID, RC.SubGroupID, RC.RecipeOn, M.GroupConceptNo, M.ConceptNo, M.ConceptDate,
                KnittingType.TypeName KnittingType,Composition.SegmentValue FabricComposition, M.ConstructionID, Construction.SegmentValue Construction, Gsm.SegmentValue FabricGsm, 
                M.SubGroupID SubGroupID, SG.SubGroupName SubGroup, M.TechnicalNameId, TT.TechnicalName,M.Qty
                FROM {TableNames.RND_RECIPE_REQ_CHILD} RC
                LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} M ON M.ConceptID = RC.ConceptID
                LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID=M.KnittingTypeID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID=M.CompositionID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID=M.ConstructionID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID=M.GSMID
                LEFT JOIN {DbNames.EPYSL}..ItemSubGroup SG ON SG.SubGroupID = M.SubGroupID
                LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME} TT ON TT.TechnicalNameId = M.TechnicalNameId
                WHERE  M.GroupConceptNo='{grpConceptNo}'; 

                --Fiber List
                ;Select CAST(EV.ValueID AS VARCHAR) AS id, EV.ValueName AS text
                From {DbNames.EPYSL}..EntityTypeValue EV
                Inner Join {DbNames.EPYSL}..EntityType ET On EV.EntityTypeID = ET.EntityTypeID
                Where ET.EntityTypeName = '{EntityTypeNameConstants.FABRIC_TYPE}'
                Group By EV.ValueID, EV.ValueName;

                --Fiber List (Collar & Cuff)
                ;With GC AS(
                    Select *
                    From {TableNames.RND_FREE_CONCEPT_MASTER}
                    Where GroupConceptNo = '{grpConceptNo}' And SubGroupID <> 1
                ), I As(
                    Select 1 As ID, MRC.ItemMasterId, ISV.SegmentValue
                    From GC
                    Inner Join {TableNames.RND_FREE_CONCEPT_MR_MASTER} MR On MR.ConceptID = GC.ConceptID
                    Inner Join {TableNames.RND_FREE_CONCEPT_MR_CHILD} MRC On MRC.FCMRMasterID = MR.FCMRMasterID
                    Inner Join {DbNames.EPYSL}..ItemMaster IM On IM.ItemMasterID = MRC.ItemMasterId
                    Inner Join {DbNames.EPYSL}..ItemSegmentValue ISV On ISV.SegmentValueID = IM.Segment1ValueID
                    Group By MRC.ItemMasterId, ISV.SegmentValue
                ), FC As(
                    Select 1 As ID, EV.ValueID, EV.ValueName
                    From {DbNames.EPYSL}..EntityTypeValue EV
                    Inner Join {DbNames.EPYSL}..EntityType ET On EV.EntityTypeID = ET.EntityTypeID
                    Where ET.EntityTypeName = 'FABRIC TYPE'
                    Group By EV.ValueID, EV.ValueName
                )
                Select FC.ValueID AS id, FC.ValueName AS text
                From FC
                Inner Join I On I.ID = FC.ID
                Where CHARINDEX(' ' + FC.ValueName, I.SegmentValue,0) > 0
                ";
            }
            else
            {
                query =
                                   $@"
                SELECT C.CCColorID, C.ColorID ,M.ConceptID,M.GroupConceptNo ConceptNo,M.GroupConceptNo,M.ConceptDate, C.ColorCode,Color.SegmentValue ColorName,
				C.DPID,C.DPProcessInfo,DP.DPName, C.Remarks, Composition.SegmentValue Composition, M.IsBDS, Replace(ISNULL(CT.ShortName,''),'Select','R&D') Buyer, ISNULL(CCT.TeamName,'R&D') BuyerTeam, ISNULL(FAC.LabDipNo,'') LabDipNo,M.BuyerID
                FROM {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} C
				LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} M ON M.ConceptID = C.ConceptID
				LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = C.ColorID
				LEFT JOIN {TableNames.DyeingProcessPart_HK} DP ON DP.DPID=C.DPID
				LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID=M.CompositionID
	            LEFT JOIN {DbNames.EPYSL}..Contacts CT ON CT.ContactID = M.BuyerID
	            LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = M.BuyerTeamID
                LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FAC On FAC.BookingChildID = M.BookingChildID AND FAC.ConsumptionID = M.ConsumptionID AND FAC.BookingID = M.BookingID
				WHERE C.CCColorID={ccolorId};
                
                ;With M AS (
				    SELECT distinct C.CCColorID, C.ColorID ,M.ConceptID,M.GroupConceptNo ConceptNo,M.GroupConceptNo,M.ConceptDate, C.ColorCode,Color.SegmentValue ColorName,
				    C.DPID,C.DPProcessInfo,DP.DPName, C.Remarks, Composition.SegmentValue Composition, M.IsBDS, Replace(ISNULL(CT.ShortName,''),'Select','R&D') Buyer, ISNULL(CCT.TeamName,'R&D') BuyerTeam, ISNULL(FAC.LabDipNo,'') LabDipNo,
				    M.Qty,M.ItemMasterId,M.SubGroupID,M.KnittingTypeID,M.CompositionID,M.ConstructionID,M.GSMID,M.TechnicalNameId
                    FROM {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} C
				    LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} M ON M.ConceptID = C.ConceptID
                    LEFT JOIN {TableNames.DYEING_BATCH_ITEM} DBI ON M.ConceptID = DBI.ConceptID
				    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = C.ColorID
				    LEFT JOIN {TableNames.DyeingProcessPart_HK} DP ON DP.DPID=C.DPID
				    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID=M.CompositionID
	                LEFT JOIN {DbNames.EPYSL}..Contacts CT ON CT.ContactID = M.BuyerID
	                LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = M.BuyerTeamID
                    LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FAC On FAC.BookingChildID = M.BookingChildID AND FAC.ConsumptionID = M.ConsumptionID AND FAC.BookingID = M.BookingID
				    INNER JOIN {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} CC ON CC.ColorID = C.ColorID
		            WHERE CC.CCColorID = {ccolorId} and isnull(DBatchID,0)={DBatchID}
				)

                SELECT M.ConceptID, M.ConceptNo, M.ConceptDate, KnittingType.TypeName KnittingType, Composition.SegmentValue Composition, M.ConstructionID,
				Construction.SegmentValue Construction, Gsm.SegmentValue GSM, M.SubGroupID SubGroupID, SG.SubGroupName SubGroup, M.TechnicalNameId,
				TT.TechnicalName,M.Qty, M.ItemMasterId
                --FROM {TableNames.RND_FREE_CONCEPT_MASTER} M
				FROM M
                LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID=M.KnittingTypeID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID=M.CompositionID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID=M.ConstructionID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID=M.GSMID
                LEFT JOIN {DbNames.EPYSL}..ItemSubGroup SG ON SG.SubGroupID = M.SubGroupID
                LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME} TT ON TT.TechnicalNameId = M.TechnicalNameId
                WHERE {strBDS}='{grpConceptNo}';

                --Dyeing Process
                ;SELECT CAST(DPID AS VARCHAR) AS id, DPName AS text
                FROM {TableNames.DyeingProcessPart_HK};

                --RecipeDefinitionDyeingInfo
                ;SELECT DI.*, EV.ValueName FiberPart, ISV.SegmentValue ColorName
                FROM {TableNames.RND_RECIPE_DEFINITION_DYEING_INFO} DI
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID = DI.FiberPartID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = DI.ColorID
                WHERE CCColorID = {ccolorId};

                            --Recipe Definition Item
                SELECT RC.RecipeReqChildID, RC.RecipeReqMasterID, RC.ConceptID, RC.BookingID, RC.ItemMasterID, RC.SubGroupID, RC.RecipeOn, M.GroupConceptNo, M.ConceptNo, M.ConceptDate,
                KnittingType.TypeName KnittingType,Composition.SegmentValue FabricComposition, M.ConstructionID, Construction.SegmentValue Construction, Gsm.SegmentValue FabricGsm, 
                M.SubGroupID SubGroupID, SG.SubGroupName SubGroup, M.TechnicalNameId, TT.TechnicalName,M.Qty
                FROM {TableNames.RND_RECIPE_REQ_CHILD} RC
                LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} M ON M.ConceptID = RC.ConceptID
                LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID=M.KnittingTypeID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID=M.CompositionID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID=M.ConstructionID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID=M.GSMID
                LEFT JOIN {DbNames.EPYSL}..ItemSubGroup SG ON SG.SubGroupID = M.SubGroupID
                LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME} TT ON TT.TechnicalNameId = M.TechnicalNameId
                WHERE  M.GroupConceptNo='{grpConceptNo}'; 

                --Fiber List
                ;Select CAST(EV.ValueID AS VARCHAR) AS id, EV.ValueName AS text
                From {DbNames.EPYSL}..EntityTypeValue EV
                Inner Join {DbNames.EPYSL}..EntityType ET On EV.EntityTypeID = ET.EntityTypeID
                Where ET.EntityTypeName = '{EntityTypeNameConstants.FABRIC_TYPE}'
                Group By EV.ValueID, EV.ValueName;

                --Fiber List (Collar & Cuff)
                ;With GC AS(
                    Select *
                    From {TableNames.RND_FREE_CONCEPT_MASTER}
                    Where GroupConceptNo = '{grpConceptNo}' And SubGroupID <> 1
                ), I As(
                    Select 1 As ID, MRC.ItemMasterId, ISV.SegmentValue
                    From GC
                    Inner Join {TableNames.RND_FREE_CONCEPT_MR_MASTER} MR On MR.ConceptID = GC.ConceptID
                    Inner Join {TableNames.RND_FREE_CONCEPT_MR_CHILD} MRC On MRC.FCMRMasterID = MR.FCMRMasterID
                    Inner Join {DbNames.EPYSL}..ItemMaster IM On IM.ItemMasterID = MRC.ItemMasterId
                    Inner Join {DbNames.EPYSL}..ItemSegmentValue ISV On ISV.SegmentValueID = IM.Segment1ValueID
                    Group By MRC.ItemMasterId, ISV.SegmentValue
                ), FC As(
                    Select 1 As ID, EV.ValueID, EV.ValueName
                    From {DbNames.EPYSL}..EntityTypeValue EV
                    Inner Join {DbNames.EPYSL}..EntityType ET On EV.EntityTypeID = ET.EntityTypeID
                    Where ET.EntityTypeName = 'FABRIC TYPE'
                    Group By EV.ValueID, EV.ValueName
                )
                Select FC.ValueID AS id, FC.ValueName AS text
                From FC
                Inner Join I On I.ID = FC.ID
                Where CHARINDEX(' ' + FC.ValueName, I.SegmentValue,0) > 0
                ";
            }
            //if (isBDS == 1)
            //    query += dyeingQuery;

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                RecipeRequestMaster data = records.Read<RecipeRequestMaster>().FirstOrDefault();
                data.RecipeRequestChilds = records.Read<RecipeRequestChild>().ToList();

                data.DPList = records.Read<Select2OptionModel>().ToList();
                data.DPList.Insert(0, new Select2OptionModel()
                {
                    id = 0.ToString(),
                    text = "--Select dyeing process--"
                });
                data.RecipeDefinitionDyeingInfos = records.Read<RecipeDefinitionDyeingInfo>().ToList();
                data.RecipeDefinitionItemInfos = records.Read<RecipeDefinitionItemInfo>().ToList();
                data.FiberPartList = records.Read<Select2OptionModel>().ToList();
                if (data.RecipeRequestChilds.Count == 1) data.RecipeRequestChilds.FirstOrDefault().RecipeOn = true;
                data.FiberPartListCC = records.Read<Select2OptionModel>().ToList();

                //if (isBDS == 1)
                //    data.DyeingBatchList = records.Read<DyeingBatchMaster>().ToList();

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

        public async Task<RecipeRequestMaster> GetAsync(int id, string groupConceptNo)
        {
            var query =
                $@"
                ;SELECT RM.RecipeReqMasterID, RM.RecipeReqNo, RM.RecipeReqDate, RM.CCColorID, RM.ColorID, RM.GroupConceptNo ConceptNo, M.ConceptID, C.ColorCode,
				Color.SegmentValue ColorName, RM.DPID, RM.DPProcessInfo, DP.DPName, RM.Remarks, Composition.SegmentValue Composition, RM.IsBDS, RM.DBatchID, Replace(ISNULL(CT.ShortName,''),'Select','R&D') Buyer, ISNULL(CCT.TeamName,'R&D') BuyerTeam, ISNULL(FAC.LabDipNo,'') LabDipNo,RM.UnAcknowledgeReason
                FROM {TableNames.RND_RECIPE_REQ_MASTER} RM
				INNER JOIN {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} C ON C.CCColorID = RM.CCColorID
				LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} M ON M.ConceptID = C.ConceptID
				LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = C.ColorID
				LEFT JOIN {TableNames.DyeingProcessPart_HK} DP ON DP.DPID=RM.DPID
				LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID=M.CompositionID
	            LEFT JOIN {DbNames.EPYSL}..Contacts CT ON CT.ContactID = M.BuyerID
	            LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = M.BuyerTeamID
                LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FAC On FAC.BookingChildID = M.BookingChildID AND FAC.ConsumptionID = M.ConsumptionID AND FAC.BookingID = M.BookingID
				WHERE RM.RecipeReqMasterID={id};

                --Child
                SELECT RC.RecipeReqChildID, RC.RecipeReqMasterID, RC.ConceptID, RC.BookingID, RC.SubGroupID, RC.ItemMasterID, RC.RecipeOn, M.ConceptNo, M.ConceptDate,
				KnittingType.TypeName KnittingType, Composition.SegmentValue Composition, M.ConstructionID, Construction.SegmentValue Construction,
				Gsm.SegmentValue GSM, SG.SubGroupName SubGroup, M.TechnicalNameId, TT.TechnicalName
				FROM {TableNames.RND_RECIPE_REQ_CHILD} RC
				INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} M ON M.ConceptID = RC.ConceptID
				LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID=M.KnittingTypeID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID=M.CompositionID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID=M.ConstructionID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID=M.GSMID
                LEFT JOIN {DbNames.EPYSL}..ItemSubGroup SG ON SG.SubGroupID = M.SubGroupID
                LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME} TT ON TT.TechnicalNameId = M.TechnicalNameId
				WHERE RecipeReqMasterID = {id};

                --RecipeDefinitionDyeingInfo
                ;SELECT DI.*, EV.ValueName FiberPart, ISV.SegmentValue ColorName
                FROM {TableNames.RND_RECIPE_DEFINITION_DYEING_INFO} DI
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID = DI.FiberPartID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = DI.ColorID
                WHERE RecipeReqMasterID = {id};

                --Dyeing Process
                ;SELECT CAST(DPID AS VARCHAR) AS id, DPName AS text
                FROM {TableNames.DyeingProcessPart_HK};

                --Recipe Definition Item
                SELECT RC.RecipeReqChildID, RC.RecipeReqMasterID, RC.ConceptID, RC.BookingID, RC.ItemMasterID, RC.SubGroupID, RC.RecipeOn, M.GroupConceptNo, M.ConceptNo, M.ConceptDate,
                KnittingType.TypeName KnittingType,Composition.SegmentValue FabricComposition, M.ConstructionID, Construction.SegmentValue Construction, Gsm.SegmentValue FabricGsm, 
                M.SubGroupID SubGroupID, SG.SubGroupName SubGroup, M.TechnicalNameId, TT.TechnicalName,M.Qty
                FROM {TableNames.RND_RECIPE_REQ_CHILD} RC
                LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} M ON M.ConceptID = RC.ConceptID
                LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID=M.KnittingTypeID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID=M.CompositionID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID=M.ConstructionID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID=M.GSMID
                LEFT JOIN {DbNames.EPYSL}..ItemSubGroup SG ON SG.SubGroupID = M.SubGroupID
                LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME} TT ON TT.TechnicalNameId = M.TechnicalNameId
                WHERE  M.GroupConceptNo='{groupConceptNo}'; 

                --Fiber List
                ;Select CAST(EV.ValueID AS VARCHAR) AS id, EV.ValueName AS text
                From {DbNames.EPYSL}..EntityTypeValue EV
                Inner Join {DbNames.EPYSL}..EntityType ET On EV.EntityTypeID = ET.EntityTypeID
                Where ET.EntityTypeName = '{EntityTypeNameConstants.FABRIC_TYPE}'
                Group By EV.ValueID, EV.ValueName;

                --Fiber List (Collar & Cuff)
                ;With GC AS(
                    Select *
                    From {TableNames.RND_FREE_CONCEPT_MASTER}
                    Where GroupConceptNo = '{groupConceptNo}' And SubGroupID <> 1
                ), I As(
                    Select 1 As ID, MRC.ItemMasterId, ISV.SegmentValue
                    From GC
                    Inner Join {TableNames.RND_FREE_CONCEPT_MR_MASTER} MR On MR.ConceptID = GC.ConceptID
                    Inner Join {TableNames.RND_FREE_CONCEPT_MR_CHILD} MRC On MRC.FCMRMasterID = MR.FCMRMasterID
                    Inner Join {DbNames.EPYSL}..ItemMaster IM On IM.ItemMasterID = MRC.ItemMasterId
                    Inner Join {DbNames.EPYSL}..ItemSegmentValue ISV On ISV.SegmentValueID = IM.Segment1ValueID
                    Group By MRC.ItemMasterId, ISV.SegmentValue
                ), FC As(
                    Select 1 As ID, EV.ValueID, EV.ValueName
                    From {DbNames.EPYSL}..EntityTypeValue EV
                    Inner Join {DbNames.EPYSL}..EntityType ET On EV.EntityTypeID = ET.EntityTypeID
                    Where ET.EntityTypeName = 'FABRIC TYPE'
                    Group By EV.ValueID, EV.ValueName
                )
                Select FC.ValueID AS id, FC.ValueName AS text
                From FC
                Inner Join I On I.ID = FC.ID
                Where CHARINDEX(' ' + FC.ValueName, I.SegmentValue,0) > 0;

                /*
                SELECT DB.DBatchID, DB.DBatchNo, DB.DBatchDate, DB.CCColorID, DB.ColorID, COL.SegmentValue ColorName,FCM.GroupConceptNo [ConceptNo]
                FROM {TableNames.DYEING_BATCH_MASTER} DB 
				LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID = DB.ColorID
                LEFT JOIN {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} FCC ON FCC.CCColorID = DB.CCColorID
                LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = FCC.ConceptID
                WHERE DB.RecipeID = 0
                */

                ";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                RecipeRequestMaster data = records.Read<RecipeRequestMaster>().FirstOrDefault();
                data.RecipeRequestChilds = records.Read<RecipeRequestChild>().ToList();
                data.RecipeDefinitionDyeingInfos = records.Read<RecipeDefinitionDyeingInfo>().ToList();
                data.DPList = records.Read<Select2OptionModel>().ToList();
                data.DPList.Insert(0, new Select2OptionModel()
                {
                    id = 0.ToString(),
                    text = "--Select dyeing process--"
                });
                data.RecipeDefinitionItemInfos = records.Read<RecipeDefinitionItemInfo>().ToList();
                data.FiberPartList = records.Read<Select2OptionModel>().ToList();
                data.FiberPartListCC = records.Read<Select2OptionModel>().ToList();
                //data.DyeingBatchList = records.Read<DyeingBatchMaster>().ToList();
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

        public async Task<RecipeRequestMaster> GetAllByIDAsync(int id)
        {
            string sql = $@"
            ;Select A.*,B.RecipeID 
            From {TableNames.RND_RECIPE_REQ_MASTER} A
            LEFT JOIN {TableNames.RND_RECIPE_DEFINITION_MASTER} B ON B.RecipeReqMasterID = A.RecipeReqMasterID
            Where A.RecipeReqMasterID = {id};

            ;Select * From {TableNames.RND_RECIPE_REQ_CHILD} Where RecipeReqMasterID = {id}

            ;Select * From {TableNames.RND_RECIPE_DEFINITION_DYEING_INFO} Where RecipeReqMasterID = {id}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                RecipeRequestMaster data = records.Read<RecipeRequestMaster>().FirstOrDefault();
                Guard.Against.NullObject(data);
                data.RecipeRequestChilds = records.Read<RecipeRequestChild>().ToList();
                data.RecipeDefinitionDyeingInfos = records.Read<RecipeDefinitionDyeingInfo>().ToList();
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

        public async Task<List<RecipeRequestChild>> GetItems(string conceptNo, int colorID, int isBDS)
        {
            string sql;
            if (isBDS == 1)
            {
                sql = $@"With M As(
                        Select *
                        From EPYSLTEX..{TableNames.RND_FREE_CONCEPT_MASTER}
                        Where GroupConceptNo = '{conceptNo}'
                        )
                        Select M.ConceptID, M.ConceptNo, M.ConceptDate, KnittingType.TypeName KnittingType, Composition.SegmentValue Composition, M.ConstructionID,
                        Construction.SegmentValue Construction, Gsm.SegmentValue GSM, M.SubGroupID SubGroupID, SG.SubGroupName SubGroup, M.TechnicalNameId,
                        TT.TechnicalName,M.Qty, M.ItemMasterId
                        From M
                        Inner Join EPYSLTEX..{TableNames.RND_FREE_CONCEPT_CHILD_COLOR} CMC ON CMC.ConceptID = M.ConceptID
                        LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID=M.KnittingTypeID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID=M.CompositionID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID=M.ConstructionID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID=M.GSMID
                        LEFT JOIN {DbNames.EPYSL}..ItemSubGroup SG ON SG.SubGroupID = M.SubGroupID
                        LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME} TT ON TT.TechnicalNameId = M.TechnicalNameId
                        Where CMC.ColorID = {colorID};";
            }
            else
            {
                sql = $@"Select M.ConceptID, M.ConceptNo, M.ConceptDate, KnittingType.TypeName KnittingType, Composition.SegmentValue Composition, M.ConstructionID,
                        Construction.SegmentValue Construction, Gsm.SegmentValue GSM, M.SubGroupID SubGroupID, SG.SubGroupName SubGroup, M.TechnicalNameId,
                        TT.TechnicalName,M.Qty, M.ItemMasterId
                        From EPYSLTEX..{TableNames.RND_FREE_CONCEPT_MASTER} M
                        LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID=M.KnittingTypeID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID=M.CompositionID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID=M.ConstructionID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID=M.GSMID
                        LEFT JOIN {DbNames.EPYSL}..ItemSubGroup SG ON SG.SubGroupID = M.SubGroupID
                        LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME} TT ON TT.TechnicalNameId = M.TechnicalNameId
                        Where GroupConceptNo = '{conceptNo}'";
            }
            return await _service.GetDataAsync<RecipeRequestChild>(sql);
        }

        public async Task<FreeConceptChildColor> GetFreeConceptChildAsync(int id)
        {
            string sql = $@"
            ;SELECT * FROM {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} WHERE CCColorID = {id}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                FreeConceptChildColor data = records.Read<FreeConceptChildColor>().FirstOrDefault();
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

        public async Task SaveAsync(RecipeRequestMaster entity)
        {
            FreeConceptChildColor childColor = await GetFreeConceptChildAsync(entity.CCColorID);
            
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
                        entity = await UpdateAsync(entity);
                        break;

                    default:
                        break;
                }

                await _service.SaveSingleAsync(entity, transaction);
                await _service.SaveAsync(entity.RecipeRequestChilds, transaction);
                await _service.SaveAsync(entity.RecipeDefinitionDyeingInfos, transaction);

                if (entity.IsRework)
                {
                    await _service.SaveAsync(entity.RecipeDefinitions, transaction);
                    await _service.SaveSingleAsync(entity.DyeingBatchReworkRecipe, transaction);
                }

                childColor.RequestRecipe = true;
                childColor.RequestBy = entity.AddedBy;
                childColor.DPID = entity.DPID;
                childColor.DPProcessInfo = entity.DPProcessInfo;
                childColor.RequestDate = DateTime.Now;
                childColor.Remarks = entity.Remarks;
                childColor.EntityState = EntityState.Modified;
                await _service.SaveSingleAsync(childColor, transaction);

                #region Update RecipeID (Tables : BatchMaster, DyeingBatchMaster, DyeingBatchItem)
                if (!entity.GroupConceptNo.IsNullOrEmpty() && entity.ColorID > 0 && entity.RecipeReqMasterID > 0 && entity.RecipeID > 0)
                {
                    string query = $"UPDATE {TableNames.BATCH_MASTER} SET RecipeID={entity.RecipeID} WHERE RecipeID = 0 AND GroupConceptNo = '{entity.GroupConceptNo}' AND ColorID = {entity.ColorID} AND CCColorID = {entity.CCColorID};";
                    await _batchService.ExecuteAsync(query, AppConstants.TEXTILE_CONNECTION);

                    query = $@"UPDATE A
					            SET A.RecipeID = {entity.RecipeID}
					            FROM {TableNames.DYEING_BATCH_MASTER} A
					            LEFT JOIN {TableNames.DYEING_BATCH_WITH_BATCH_MASTER} B ON B.DBatchID=A.DBatchID
                                LEFT JOIN {TableNames.BATCH_MASTER} BM ON BM.BatchID=B.BatchID AND BM.ColorID=A.ColorID
                                WHERE A.RecipeID = 0 AND BM.GroupConceptNo = '{entity.GroupConceptNo}' AND A.ColorID = {entity.ColorID} AND A.CCColorID = {entity.CCColorID};";
                    await _dyeingBatchService.ExecuteAsync(query, AppConstants.TEXTILE_CONNECTION);

                    //query = $@"UPDATE A
                    //        SET A.RecipeID = {entity.RecipeID}
                    //        FROM DyeingBatchItem A
                    //        LEFT JOIN BatchMaster BM ON BM.BatchID = A.BatchID
                    //        WHERE A.RecipeID = 0 AND BM.GroupConceptNo = '{entity.GroupConceptNo}' AND BM.ColorID = {entity.ColorID}; 
                    //        ";


                    query = $@"Update b Set RecipeID = c.RecipeID
                            From {TableNames.DYEING_BATCH_MASTER} a
                            Inner Join {TableNames.DYEING_BATCH_ITEM} b On b.DBatchID = a.DBatchID
                            Inner Join {TableNames.RND_RECIPE_DEFINITION_MASTER} c ON c.CCColorID = a.CCColorID and c.ColorID = a.ColorID
                            Inner Join {TableNames.RND_RECIPE_REQ_MASTER} d ON d.CCColorID = a.CCColorID and d.ColorID = a.ColorID
                            Where b.RecipeID = 0 AND d.GroupConceptNo = '{entity.GroupConceptNo}' AND a.ColorID = {entity.ColorID} AND A.CCColorID={entity.CCColorID};";

                    await _dyeingBatchItemService.ExecuteAsync(query, AppConstants.TEXTILE_CONNECTION);
                }
                #endregion

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

        private async Task<RecipeRequestMaster> AddAsync(RecipeRequestMaster entity)
        {
            entity.RecipeReqMasterID = await _service.GetMaxIdAsync(TableNames.RND_RECIPE_REQ_MASTER, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
            entity.RecipeReqNo = entity.IsRework == false ? await _service.GetMaxNoAsync(TableNames.RND_RECIPE_REQ_NO, 1, RepeatAfterEnum.NoRepeat, "00000", transactionGmt, _connectionGmt) : entity.RecipeReqNo;
            var maxChildId = await _service.GetMaxIdAsync(TableNames.RND_RECIPE_REQ_CHILD, entity.RecipeRequestChilds.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
            var maxDyingInfoChildId = await _service.GetMaxIdAsync(TableNames.RND_RECIPE_DEFINITION_DYEING_INFO, entity.RecipeDefinitionDyeingInfos.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

            foreach (RecipeRequestChild item in entity.RecipeRequestChilds)
            {
                item.RecipeReqChildID = maxChildId++;
                item.RecipeReqMasterID = entity.RecipeReqMasterID;
                item.CCColorID = entity.CCColorID;
                item.EntityState = EntityState.Added;
            }
            foreach (RecipeDefinitionDyeingInfo item in entity.RecipeDefinitionDyeingInfos)
            {
                item.RecipeDInfoID = maxDyingInfoChildId++;
                item.RecipeReqMasterID = entity.RecipeReqMasterID;
                item.EntityState = EntityState.Added;
            }
            if (entity.IsRework)
            {
                entity.DyeingBatchReworkRecipe = new DyeingBatchReworkRecipe();
                entity.DyeingBatchReworkRecipe.DBRRID = await _service.GetMaxIdAsync(TableNames.DYEING_BATCH_REWORK_RECIPE, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                entity.DyeingBatchReworkRecipe.DBatchID = entity.DBatchID;
                entity.DyeingBatchReworkRecipe.DBRID = 0;
                entity.DyeingBatchReworkRecipe.RecipeReqMasterID = entity.RecipeReqMasterID;
                entity.DyeingBatchReworkRecipe.EntityState = EntityState.Added;
            }
            return entity;
        }

        private async Task<RecipeRequestMaster> UpdateAsync(RecipeRequestMaster entity)
        {
            var maxChildId = await _service.GetMaxIdAsync(TableNames.RND_RECIPE_REQ_CHILD, entity.RecipeRequestChilds.Where(x => x.EntityState == EntityState.Added).Count(), RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
            var maxDyingInfoChildId = await _service.GetMaxIdAsync(TableNames.RND_RECIPE_DEFINITION_DYEING_INFO, entity.RecipeDefinitionDyeingInfos.Where(x => x.EntityState == EntityState.Added).Count(), RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

            foreach (var item in entity.RecipeRequestChilds.Where(x => x.EntityState == EntityState.Added).ToList())
            {
                item.RecipeReqChildID = maxChildId++;
                item.RecipeReqMasterID = entity.RecipeReqMasterID;
                item.CCColorID = entity.CCColorID;
            }
            foreach (var item in entity.RecipeDefinitionDyeingInfos.Where(x => x.EntityState == EntityState.Added).ToList())
            {
                item.RecipeDInfoID = maxDyingInfoChildId++;
                item.RecipeReqMasterID = entity.RecipeReqMasterID;
            }
            return entity;
        }

        public async Task UpdateEntityAsync(RecipeRequestMaster entity)
        {
            FreeConceptChildColor childColor = await GetFreeConceptChildAsync(entity.CCColorID);
            
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                await _connectionGmt.OpenAsync();
                transactionGmt = _connectionGmt.BeginTransaction();

                await _service.SaveSingleAsync(entity, transaction);
                await _service.SaveAsync(entity.RecipeDefinitionDyeingInfos, transaction);

                childColor.RequestAck = true;
                childColor.RequestAckBy = entity.AddedBy;
                childColor.RequestAckDate = DateTime.Now;
                childColor.EntityState = EntityState.Modified;
                await _service.SaveSingleAsync(childColor, transaction);

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

        public async Task RevisionAsync(RecipeRequestMaster entity)
        {
            FreeConceptChildColor childColor = await GetFreeConceptChildAsync(entity.CCColorID);
            
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                await _connectionGmt.OpenAsync();
                transactionGmt = _connectionGmt.BeginTransaction();

                //only for revision after UnAcknowledge
                await _connection.ExecuteAsync("spBackupRecipeRequestMaster_Full", new { RecipeReqMasterID = entity.RecipeReqMasterID }, transaction, 30, CommandType.StoredProcedure);
                //end only for revision after UnAcknowledge

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
                await _service.SaveAsync(entity.RecipeRequestChilds, transaction);
                await _service.SaveAsync(entity.RecipeDefinitionDyeingInfos, transaction);

                if (entity.IsRework)
                {
                    await _service.SaveAsync(entity.RecipeDefinitions, transaction);
                    await _service.SaveSingleAsync(entity.DyeingBatchReworkRecipe, transaction);
                }

                childColor.RequestRecipe = true;
                childColor.RequestBy = entity.AddedBy;
                childColor.DPID = entity.DPID;
                childColor.DPProcessInfo = entity.DPProcessInfo;
                childColor.RequestDate = DateTime.Now;
                childColor.Remarks = entity.Remarks;
                childColor.EntityState = EntityState.Modified;
                await _service.SaveSingleAsync(childColor, transaction);

                #region Update RecipeID (Tables : BatchMaster, DyeingBatchMaster, DyeingBatchItem)
                if (!entity.GroupConceptNo.IsNullOrEmpty() && entity.ColorID > 0 && entity.RecipeReqMasterID > 0 && entity.RecipeID > 0)
                {
                    string query = $"UPDATE {TableNames.BATCH_MASTER} SET RecipeID={entity.RecipeID} WHERE RecipeID = 0 AND GroupConceptNo = '{entity.GroupConceptNo}' AND ColorID = {entity.ColorID} AND CCColorID = {entity.CCColorID};";
                    await _batchService.ExecuteAsync(query, AppConstants.TEXTILE_CONNECTION);

                    query = $@"UPDATE A
					            SET A.RecipeID = {entity.RecipeID}
					            FROM {TableNames.DYEING_BATCH_MASTER} A
					            LEFT JOIN {TableNames.DYEING_BATCH_WITH_BATCH_MASTER} B ON B.DBatchID=A.DBatchID
                                LEFT JOIN {TableNames.BATCH_MASTER} BM ON BM.BatchID=B.BatchID AND BM.ColorID=A.ColorID
                                WHERE A.RecipeID = 0 AND BM.GroupConceptNo = '{entity.GroupConceptNo}' AND A.ColorID = {entity.ColorID} AND A.CCColorID = {entity.CCColorID};";
                    await _dyeingBatchService.ExecuteAsync(query, AppConstants.TEXTILE_CONNECTION);

                    //query = $@"UPDATE A
                    //        SET A.RecipeID = {entity.RecipeID}
                    //        FROM DyeingBatchItem A
                    //        LEFT JOIN BatchMaster BM ON BM.BatchID = A.BatchID
                    //        WHERE A.RecipeID = 0 AND BM.GroupConceptNo = '{entity.GroupConceptNo}' AND BM.ColorID = {entity.ColorID}; 
                    //        ";


                    query = $@"Update b Set RecipeID = c.RecipeID
                            From {TableNames.DYEING_BATCH_MASTER} a
                            Inner Join {TableNames.DYEING_BATCH_ITEM} b On b.DBatchID = a.DBatchID
                            Inner Join {TableNames.RND_RECIPE_DEFINITION_MASTER} c ON c.CCColorID = a.CCColorID and c.ColorID = a.ColorID
                            Inner Join {TableNames.RND_RECIPE_REQ_MASTER} d ON d.CCColorID = a.CCColorID and d.ColorID = a.ColorID
                            Where b.RecipeID = 0 AND d.GroupConceptNo = '{entity.GroupConceptNo}' AND a.ColorID = {entity.ColorID} AND A.CCColorID={entity.CCColorID};";

                    await _dyeingBatchItemService.ExecuteAsync(query, AppConstants.TEXTILE_CONNECTION);
                }
                #endregion

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
    }
}