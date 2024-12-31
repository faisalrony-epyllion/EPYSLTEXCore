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
using Microsoft.Data.SqlClient;
using System.Transactions;

namespace EPYSLTEXCore.Application.Services.Inventory
{
    public class YDRecipeDefinitionService : IYDRecipeDefinitionService
    {
        private readonly IDapperCRUDService<YDRecipeDefinitionMaster> _service;
        private readonly SqlConnection _connection;
        private readonly SqlConnection _connectionGmt;

        private readonly IDapperCRUDService<YDDyeingBatchMaster> _dyeingBatchService;
        private readonly IDapperCRUDService<YDBatchMaster> _batchService;
        private readonly IDapperCRUDService<YDDyeingBatchItem> _dyeingBatchItemService;
        public YDRecipeDefinitionService(IDapperCRUDService<YDRecipeDefinitionMaster> service,
            IDapperCRUDService<YDDyeingBatchMaster> dyeingBatchService,
            IDapperCRUDService<YDBatchMaster> batchService,
            IDapperCRUDService<YDDyeingBatchItem> dyeingBatchItemService
            )
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
        public async Task<List<YDRecipeDefinitionMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By YDRecipeID Desc" : paginationInfo.OrderBy;

            string sql;
            if (status == Status.Pending)
            {
                sql =
                $@"WITH NewList AS
                (
	                select RM.YDRecipeReqMasterID, RM.RecipeReqNo, RM.CCColorID, RM.GroupConceptNo ConceptNo,M.ConceptDate,C.ColorCode,
	                Color.SegmentValue ColorName, DP.DPName, RM.DPProcessInfo, RM.AcknowledgeDate RequestAckDate, 
	                Replace(ISNULL(CT.ShortName,''),'Select','R&D') Buyer, ISNULL(CCT.TeamName,'R&D') BuyerTeam, 
	                ISNULL(FAC.LabDipNo,'') LabDipNo1, M.BookingID
	                ,[Status] = 'New'
	                FROM {TableNames.YD_RECIPE_REQ_MASTER} RM
	                INNER JOIN {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} C ON C.CCColorID = RM.CCColorID
	                LEFT JOIN {TableNames.DyeingProcessPart_HK} DP ON DP.DPID=RM.DPID
	                LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} M ON M.ConceptID = C.ConceptID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = RM.ColorID
	                LEFT JOIN {TableNames.YD_RECIPE_DEFINITION_MASTER} RD ON RD.CCColorID = RM.CCColorID
	                LEFT JOIN {DbNames.EPYSL}..Contacts CT ON CT.ContactID = M.BuyerID
	                LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = M.BuyerTeamID
	                LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FAC On FAC.BookingChildID = M.BookingChildID AND FAC.ConsumptionID = M.ConsumptionID AND FAC.BookingID = M.BookingID
                    LEFT JOIN {TableNames.YD_RECIPE_DEFINITION_MASTER} RDM ON RDM.YDRecipeReqMasterID = RM.YDRecipeReqMasterID 	                
                    WHERE ISNULL(RM.Acknowledge,0) = 1 AND RDM.YDRecipeID IS NULL --AND RD.CCColorID IS NULL
	                Group By RM.YDRecipeReqMasterID, RM.RecipeReqNo, RM.CCColorID, RM.GroupConceptNo,M.ConceptDate,C.ColorCode,
	                Color.SegmentValue, DP.DPName, RM.DPProcessInfo, RM.AcknowledgeDate, Replace(ISNULL(CT.ShortName,''),'Select','R&D'), ISNULL(CCT.TeamName,'R&D'), ISNULL(FAC.LabDipNo,''), M.BookingID
                ),
                FBC_NL AS
                (
	                SELECT FAC.LabDipNo, T.BookingID
	                FROM {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD_TEXT} T
	                INNER JOIN NewList MO ON MO.BookingID = T.BookingID
	                INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FAC On FAC.BookingChildID = T.BookingChildID AND FAC.ConsumptionID = T.ConsumptionID AND FAC.BookingID = T.BookingID
	                WHERE T.UsesIn Like '%Body%'
	                GROUP BY FAC.LabDipNo, T.BookingID
                ),
                ReworkList AS
                (
	                select RM.YDRecipeReqMasterID, RM.RecipeReqNo, RM.CCColorID, RM.GroupConceptNo ConceptNo,M.ConceptDate,C.ColorCode,
	                Color.SegmentValue ColorName, DP.DPName, RM.DPProcessInfo, RM.AcknowledgeDate RequestAckDate, 
	                Replace(ISNULL(CT.ShortName,''),'Select','R&D') Buyer, ISNULL(CCT.TeamName,'R&D') BuyerTeam, 
	                ISNULL(FAC.LabDipNo,'') LabDipNo1, M.BookingID
	                ,[Status] = 'Rework'
	                FROM {TableNames.YD_RECIPE_REQ_MASTER} RM
	                INNER JOIN {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} C ON C.CCColorID = RM.CCColorID
	                LEFT JOIN {TableNames.DyeingProcessPart_HK} DP ON DP.DPID=RM.DPID
	                LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} M ON M.ConceptID = C.ConceptID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = RM.ColorID
	                LEFT JOIN {TableNames.YD_RECIPE_DEFINITION_MASTER} RD ON RD.CCColorID = RM.CCColorID
	                LEFT JOIN {DbNames.EPYSL}..Contacts CT ON CT.ContactID = M.BuyerID
	                LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = M.BuyerTeamID
	                LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FAC On FAC.BookingChildID = M.BookingChildID AND FAC.ConsumptionID = M.ConsumptionID AND FAC.BookingID = M.BookingID
	                LEFT JOIN {TableNames.YD_RECIPE_DEFINITION_MASTER} RDM ON RDM.YDRecipeReqMasterID = RM.YDRecipeReqMasterID
	                INNER JOIN {TableNames.DYEING_BATCH_MASTER} DB ON DB.CCColorID = C.CCColorID
	                INNER JOIN {TableNames.DYEING_BATCH_REWORK} DBR ON DBR.DBatchID = DB.DBatchID
	                WHERE ISNULL(DBR.BatchStatus,0) = 3 AND ISNULL(DBR.IsNewBatch,0) = 1 AND ISNULL(DBR.IsNewRecipe,0) = 1
	                AND ISNULL(RM.Acknowledge,0) = 1 AND RDM.YDRecipeID IS NULL
	                Group By RM.YDRecipeReqMasterID, RM.RecipeReqNo, RM.CCColorID, RM.GroupConceptNo,M.ConceptDate,C.ColorCode,
	                Color.SegmentValue, DP.DPName, RM.DPProcessInfo, RM.AcknowledgeDate, Replace(ISNULL(CT.ShortName,''),'Select','R&D'), ISNULL(CCT.TeamName,'R&D'), ISNULL(FAC.LabDipNo,''),M.BookingID
                ),
                FBC_RL AS
                (
	                SELECT FAC.LabDipNo, T.BookingID
	                FROM {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD_TEXT} T
	                INNER JOIN ReworkList MO ON MO.BookingID = T.BookingID
	                INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FAC On FAC.BookingChildID = T.BookingChildID AND FAC.ConsumptionID = T.ConsumptionID AND FAC.BookingID = T.BookingID
	                WHERE T.UsesIn Like '%Body%'
	                GROUP BY FAC.LabDipNo, T.BookingID
                ),
                FinalList AS
                (
	                SELECT NL.*,LabDipNo = CASE WHEN ISNULL(FBC.LabDipNo,'') <> '' THEN FBC.LabDipNo ELSE NL.LabDipNo1 END
	                FROM NewList NL
	                LEFT JOIN FBC_NL FBC ON FBC.BookingID = NL.BookingID

	                UNION

	                SELECT RL.*,LabDipNo = CASE WHEN ISNULL(FBC.LabDipNo,'') <> '' THEN FBC.LabDipNo ELSE RL.LabDipNo1 END
	                FROM ReworkList RL
	                LEFT JOIN FBC_RL FBC ON FBC.BookingID = RL.BookingID
                )
                SELECT *, Count(*) Over() TotalRows FROM FinalList";

                orderBy = " ORDER BY YDRecipeReqMasterID DESC";
            }
            else if (status == Status.PartiallyCompleted)
            {
                sql =
                $@"With RR AS (
	                select M.YDRecipeID, M.YDRecipeNo, M.YDRecipeReqMasterID, M.RecipeDate,RF.ValueName RecipeForName,M.BatchWeightKG,M.Remarks,C.SegmentValue ColorName,
	                M.ConceptID, RM.RecipeReqNo, RM.GroupConceptNo ConceptNo, M.Temperature, M.ProcessTime,DP.DPName,M.DPProcessInfo,
	                Replace(ISNULL(CT.ShortName,''),'Select','R&D') Buyer, ISNULL(CCT.TeamName,'R&D') BuyerTeam, ISNULL(FAC.LabDipNo,'') LabDipNo1, FCM.BookingID 
	                ,RecipeStatus = CASE WHEN ISNULL(M.IsActive,0) = 0 THEN 'Deactive' ELSE 'Active' END
	                FROM {TableNames.YD_RECIPE_DEFINITION_MASTER} M
	                INNER JOIN {TableNames.YD_RECIPE_REQ_MASTER} RM ON RM.YDRecipeReqMasterID = M.YDRecipeReqMasterID
	                LEFT JOIN {TableNames.DyeingProcessPart_HK} DP ON DP.DPID=M.DPID
	                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue RF ON RF.ValueID = M.RecipeFor
	                left JOIN {DbNames.EPYSL}..ItemSegmentValue C ON C.SegmentValueID = M.ColorID
	                INNER JOIN {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} CCC ON CCC.CCColorID = RM.CCColorID
	                LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = CCC.ConceptID
	                LEFT JOIN {DbNames.EPYSL}..Contacts CT ON CT.ContactID = FCM.BuyerID
	                LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = FCM.BuyerTeamID
	                LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FAC On FAC.BookingChildID = FCM.BookingChildID AND FAC.ConsumptionID = FCM.ConsumptionID AND FAC.BookingID = FCM.BookingID
	                WHERE ISNULL(M.IsApproved,0)=0 AND ISNULL(M.Acknowledged,0)=0
	                Group By M.YDRecipeID, M.YDRecipeNo, M.YDRecipeReqMasterID, M.RecipeDate,RF.ValueName,M.BatchWeightKG,M.Remarks,C.SegmentValue,
	                M.ConceptID, RM.RecipeReqNo, RM.GroupConceptNo, M.Temperature, M.ProcessTime,DP.DPName,M.DPProcessInfo,ISNULL(M.IsActive,0),
	                Replace(ISNULL(CT.ShortName,''),'Select','R&D'), ISNULL(CCT.TeamName,'R&D'), ISNULL(FAC.LabDipNo,''), FCM.BookingID
                ),
                FBC AS
                (
	                SELECT FAC.LabDipNo, T.BookingID
	                FROM {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD_TEXT} T
	                INNER JOIN RR MO ON MO.BookingID = T.BookingID
	                INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FAC On FAC.BookingChildID = T.BookingChildID AND FAC.ConsumptionID = T.ConsumptionID AND FAC.BookingID = T.BookingID
	                WHERE T.UsesIn Like '%Body%'
	                GROUP BY FAC.LabDipNo, T.BookingID
                ),
                FinalList AS
                (
	                SELECT NL.*,LabDipNo = CASE WHEN ISNULL(FBC.LabDipNo,'') <> '' THEN FBC.LabDipNo ELSE NL.LabDipNo1 END
	                FROM RR NL
	                LEFT JOIN FBC ON FBC.BookingID = NL.BookingID
                )
                SELECT *, COUNT(*) OVER() TotalRows FROM FinalList
                ";
            }
            else if (status == Status.Approved)
            {
                sql =
                $@"With RR AS (
                    select M.YDRecipeID, M.YDRecipeNo, M.YDRecipeReqMasterID, M.RecipeDate,RF.ValueName RecipeForName,M.BatchWeightKG,M.Remarks,C.SegmentValue ColorName,
                    M.ConceptID, RM.RecipeReqNo, RM.GroupConceptNo ConceptNo, M.Temperature, M.ProcessTime,DP.DPName,M.DPProcessInfo,
                    Replace(ISNULL(CT.ShortName,''),'Select','R&D') Buyer, ISNULL(CCT.TeamName,'R&D') BuyerTeam, ISNULL(FAC.LabDipNo,'') LabDipNo1,FCM.BookingID 
                    ,RecipeStatus = CASE WHEN ISNULL(M.IsActive,0) = 0 THEN 'Deactive' ELSE 'Active' END
                    FROM {TableNames.YD_RECIPE_DEFINITION_MASTER} M
                    INNER JOIN {TableNames.YD_RECIPE_REQ_MASTER} RM ON RM.YDRecipeReqMasterID = M.YDRecipeReqMasterID
                    LEFT JOIN {TableNames.DyeingProcessPart_HK} DP ON DP.DPID=M.DPID
                    LEFT JOIN {DbNames.EPYSL}..EntityTypeValue RF ON RF.ValueID = M.RecipeFor
                    left JOIN {DbNames.EPYSL}..ItemSegmentValue C ON C.SegmentValueID = M.ColorID
                    INNER JOIN {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} CCC ON CCC.CCColorID = RM.CCColorID
                    LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = CCC.ConceptID
                    LEFT JOIN {DbNames.EPYSL}..Contacts CT ON CT.ContactID = FCM.BuyerID
                    LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = FCM.BuyerTeamID
                    LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FAC On FAC.BookingChildID = FCM.BookingChildID AND FAC.ConsumptionID = FCM.ConsumptionID AND FAC.BookingID = FCM.BookingID
                    WHERE ISNULL(M.IsApproved,0)=1 AND ISNULL(M.Acknowledged,0)=0
                    Group By M.YDRecipeID, M.YDRecipeNo, M.YDRecipeReqMasterID, M.RecipeDate,RF.ValueName,M.BatchWeightKG,M.Remarks,C.SegmentValue,
                    M.ConceptID, RM.RecipeReqNo, RM.GroupConceptNo, M.Temperature, M.ProcessTime,DP.DPName,M.DPProcessInfo,ISNULL(M.IsActive,0),
                    Replace(ISNULL(CT.ShortName,''),'Select','R&D'), ISNULL(CCT.TeamName,'R&D'), ISNULL(FAC.LabDipNo,''),FCM.BookingID 
                ),
                FBC AS
                (
	                SELECT FAC.LabDipNo, T.BookingID
	                FROM {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD_TEXT} T
	                INNER JOIN RR MO ON MO.BookingID = T.BookingID
	                INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FAC On FAC.BookingChildID = T.BookingChildID AND FAC.ConsumptionID = T.ConsumptionID AND FAC.BookingID = T.BookingID
	                WHERE T.UsesIn Like '%Body%'
	                GROUP BY FAC.LabDipNo, T.BookingID
                ),
                FinalList AS
                (
	                SELECT NL.*,LabDipNo = CASE WHEN ISNULL(FBC.LabDipNo,'') <> '' THEN FBC.LabDipNo ELSE NL.LabDipNo1 END
	                FROM RR NL
	                LEFT JOIN FBC ON FBC.BookingID = NL.BookingID
                )
                SELECT *, COUNT(*) OVER() TotalRows FROM FinalList
                ";
            }
            else if (status == Status.Acknowledge)
            {
                sql =
                $@"With RR AS (
	                select M.YDRecipeID, M.YDRecipeNo, M.YDRecipeReqMasterID, M.RecipeDate,RF.ValueName RecipeForName,M.BatchWeightKG,M.Remarks,C.SegmentValue ColorName,
	                M.ConceptID, RM.RecipeReqNo, RM.GroupConceptNo ConceptNo, M.Temperature, M.ProcessTime,DP.DPName,M.DPProcessInfo,M.AcknowledgedDate,
	                Replace(ISNULL(CT.ShortName,''),'Select','R&D') Buyer, ISNULL(CCT.TeamName,'R&D') BuyerTeam, ISNULL(FAC.LabDipNo,'') LabDipNo1, FCM.BookingID
	                ,RecipeStatus = CASE WHEN ISNULL(M.IsActive,0) = 0 THEN 'Deactive' ELSE 'Active' END				
	                FROM {TableNames.YD_RECIPE_DEFINITION_MASTER} M
	                INNER JOIN {TableNames.YD_RECIPE_REQ_MASTER} RM ON RM.YDRecipeReqMasterID = M.YDRecipeReqMasterID
	                LEFT JOIN {TableNames.DyeingProcessPart_HK} DP ON DP.DPID=M.DPID
	                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue RF ON RF.ValueID = M.RecipeFor
	                left JOIN {DbNames.EPYSL}..ItemSegmentValue C ON C.SegmentValueID = M.ColorID
	                INNER JOIN {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} CCC ON CCC.CCColorID = RM.CCColorID
	                LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = CCC.ConceptID
	                LEFT JOIN {DbNames.EPYSL}..Contacts CT ON CT.ContactID = FCM.BuyerID
	                LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = FCM.BuyerTeamID
	                LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FAC On FAC.BookingChildID = FCM.BookingChildID AND FAC.ConsumptionID = FCM.ConsumptionID AND FAC.BookingID = FCM.BookingID
	                WHERE ISNULL(M.IsApproved,0)=1 AND ISNULL(M.Acknowledged,0)=1
	                Group By M.YDRecipeID, M.YDRecipeNo, M.YDRecipeReqMasterID, M.RecipeDate,RF.ValueName,M.BatchWeightKG,M.Remarks,C.SegmentValue,
	                M.ConceptID, RM.RecipeReqNo, RM.GroupConceptNo, M.Temperature, M.ProcessTime,DP.DPName,M.DPProcessInfo,M.AcknowledgedDate,ISNULL(M.IsActive,0),
	                Replace(ISNULL(CT.ShortName,''),'Select','R&D'), ISNULL(CCT.TeamName,'R&D'), ISNULL(FAC.LabDipNo,''), FCM.BookingID
                ),
                FBC AS
                (
	                SELECT FAC.LabDipNo, T.BookingID
	                FROM {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD_TEXT} T
	                INNER JOIN RR MO ON MO.BookingID = T.BookingID
	                INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FAC On FAC.BookingChildID = T.BookingChildID AND FAC.ConsumptionID = T.ConsumptionID AND FAC.BookingID = T.BookingID
	                WHERE T.UsesIn Like '%Body%'
	                GROUP BY FAC.LabDipNo, T.BookingID
                ),
                FinalList AS
                (
	                SELECT NL.*,LabDipNo = CASE WHEN ISNULL(FBC.LabDipNo,'') <> '' THEN FBC.LabDipNo ELSE NL.LabDipNo1 END
	                FROM RR NL
	                LEFT JOIN FBC ON FBC.BookingID = NL.BookingID
                )
                SELECT *, COUNT(*) OVER() TotalRows FROM FinalList
                ";
                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By AcknowledgedDate Desc" : paginationInfo.OrderBy;
            }
            else
            {
                sql = $@"
                With RR AS (
	                select M.YDRecipeID, M.YDRecipeNo, M.YDRecipeReqMasterID, M.RecipeDate,RF.ValueName RecipeForName,M.BatchWeightKG,M.Remarks,C.SegmentValue ColorName,
	                M.ConceptID, RM.RecipeReqNo, RM.GroupConceptNo ConceptNo, M.Temperature, M.ProcessTime,DP.DPName,M.DPProcessInfo,
                    Replace(ISNULL(CT.ShortName,''),'Select','R&D') Buyer, ISNULL(CCT.TeamName,'R&D') BuyerTeam, ISNULL(FAC.LabDipNo,'') LabDipNo1, FCM.BookingID
                    ,RecipeStatus = CASE WHEN ISNULL(M.IsActive,0) = 0 THEN 'Deactive' ELSE 'Active' END				
                    FROM {TableNames.YD_RECIPE_DEFINITION_MASTER} M
	                INNER JOIN {TableNames.YD_RECIPE_REQ_MASTER} RM ON RM.YDRecipeReqMasterID = M.YDRecipeReqMasterID
                    LEFT JOIN {TableNames.DyeingProcessPart_HK} DP ON DP.DPID=M.DPID
	                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue RF ON RF.ValueID = M.RecipeFor
	                left JOIN {DbNames.EPYSL}..ItemSegmentValue C ON C.SegmentValueID = M.ColorID
	                INNER JOIN {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} CCC ON CCC.CCColorID = RM.CCColorID
                    LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = CCC.ConceptID
	                LEFT JOIN {DbNames.EPYSL}..Contacts CT ON CT.ContactID = FCM.BuyerID
	                LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = FCM.BuyerTeamID
	                LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FAC On FAC.BookingChildID = FCM.BookingChildID AND FAC.ConsumptionID = FCM.ConsumptionID AND FAC.BookingID = FCM.BookingID
                    Group By M.YDRecipeID, M.YDRecipeNo, M.YDRecipeReqMasterID, M.RecipeDate,RF.ValueName,M.BatchWeightKG,M.Remarks,C.SegmentValue,
	                M.ConceptID, RM.RecipeReqNo, RM.GroupConceptNo, M.Temperature, M.ProcessTime,DP.DPName,M.DPProcessInfo,ISNULL(M.IsActive,0),
	                Replace(ISNULL(CT.ShortName,''),'Select','R&D'), ISNULL(CCT.TeamName,'R&D'), ISNULL(FAC.LabDipNo,''), FCM.BookingID
                ),
                FBC AS
                (
	                SELECT FAC.LabDipNo, T.BookingID
	                FROM {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD_TEXT} T
	                INNER JOIN RR MO ON MO.BookingID = T.BookingID
	                INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FAC On FAC.BookingChildID = T.BookingChildID AND FAC.ConsumptionID = T.ConsumptionID AND FAC.BookingID = T.BookingID
	                WHERE T.UsesIn Like '%Body%'
	                GROUP BY FAC.LabDipNo, T.BookingID
                ),
                FinalList AS
                (
	                SELECT NL.*,LabDipNo = CASE WHEN ISNULL(FBC.LabDipNo,'') <> '' THEN FBC.LabDipNo ELSE NL.LabDipNo1 END
	                FROM RR NL
	                LEFT JOIN FBC ON FBC.BookingID = NL.BookingID
                )
                SELECT *, COUNT(*) OVER() TotalRows FROM FinalList
                ";
            }

            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<YDRecipeDefinitionMaster>(sql);
        }
        public async Task<YDRecipeDefinitionMaster> GetNewAsync(int id)
        {
            var query =
                $@"
                WITH MainObj AS
                (
	                SELECT RM.YDRecipeReqMasterID, RM.RecipeReqNo, RM.CCColorID, RM.ColorID ,RM.GroupConceptNo ConceptNo, C.ColorCode, Color.SegmentValue ColorName, DP.DPName,RM.DPProcessInfo,RM.DPID,
	                Replace(ISNULL(CT.ShortName,''),'Select','R&D') Buyer, ISNULL(CCT.TeamName,'R&D') BuyerTeam,FAC.BookingID, ISNULL(FAC.LabDipNo,'') LabDipNo 
	                FROM {TableNames.YD_RECIPE_REQ_MASTER} RM
	                INNER JOIN {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} C ON C.CCColorID = RM.CCColorID
	                LEFT JOIN {TableNames.DyeingProcessPart_HK} DP ON DP.DPID=RM.DPID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = RM.ColorID
	                LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = C.ConceptID
	                LEFT JOIN {DbNames.EPYSL}..Contacts CT ON CT.ContactID = FCM.BuyerID
	                LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = FCM.BuyerTeamID
	                LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FAC On FAC.BookingChildID = FCM.BookingChildID AND FAC.ConsumptionID = FCM.ConsumptionID AND FAC.BookingID = FCM.BookingID
	                WHERE RM.YDRecipeReqMasterID={id}
                ),
                FBC AS
                (
	                SELECT FAC.LabDipNo, T.BookingID
	                FROM {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD_TEXT} T
	                INNER JOIN MainObj MO ON MO.BookingID = T.BookingID
	                INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FAC On FAC.BookingChildID = T.BookingChildID AND FAC.ConsumptionID = T.ConsumptionID AND FAC.BookingID = T.BookingID
	                WHERE T.UsesIn Like '%Body%'
	                GROUP BY FAC.LabDipNo, T.BookingID
                )
                SELECT MO.*,LabDipNo = CASE WHEN ISNULL(FBC.LabDipNo,'') <> '' THEN FBC.LabDipNo ELSE MO.LabDipNo END
                FROM MainObj MO
                LEFT JOIN FBC ON FBC.BookingID = MO.BookingID;

                --Childs
                ;SELECT DI.*, EV.ValueName FiberPart, ISV.SegmentValue ColorName
                FROM {TableNames.YD_RECIPE_DEFINITION_DYEING_INFO} DI
                INNER JOIN {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID = DI.FiberPartID
                INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = DI.ColorID
                WHERE DI.YDRecipeReqMasterID = {id};

                SELECT RC.YDRecipeReqChildID, RC.YDRecipeReqMasterID, RC.ConceptID, RC.BookingID, RC.ItemMasterID, RC.SubGroupID, RC.RecipeOn, M.GroupConceptNo, M.ConceptNo, M.ConceptDate,
                KnittingType.TypeName KnittingType,Composition.SegmentValue FabricComposition, M.ConstructionID, Construction.SegmentValue Construction, Gsm.SegmentValue FabricGsm, 
                M.SubGroupID SubGroupID, SG.SubGroupName SubGroup, M.TechnicalNameId, TT.TechnicalName,M.Qty
                FROM {TableNames.YD_RECIPE_REQ_CHILD} RC
                LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} M ON M.ConceptID = RC.ConceptID
                LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID=M.KnittingTypeID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID=M.CompositionID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID=M.ConstructionID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID=M.GSMID
                LEFT JOIN {DbNames.EPYSL}..ItemSubGroup SG ON SG.SubGroupID = M.SubGroupID
                LEFT JOIN {TableNames.FabricTechnicalName} TT ON TT.TechnicalNameId = M.TechnicalNameId
                WHERE RC.YDRecipeReqMasterID={id};

                --Recipe Definition For
                ;SELECT CAST(ValueID AS VARCHAR) id, ValueName text
                FROM {DbNames.EPYSL}..EntityTypeValue
                WHERE EntityTypeID = {EntityTypeConstants.RECIPE_DEFINITION_FOR} AND ValueName <> 'Select'
                ORDER BY ValueName;

                --Recipe Definition Process
                ;SELECT CAST(ValueID AS VARCHAR) id, ValueName text
                FROM {DbNames.EPYSL}..EntityTypeValue
                WHERE EntityTypeID = {EntityTypeConstants.RECIPE_DEFINITION_PROCESS} AND ValueName <> 'Select'
                ORDER BY ValueName;

                --Recipe Definition Particulars
                ;SELECT CAST(ValueID AS VARCHAR) id, ValueName text
                FROM {DbNames.EPYSL}..EntityTypeValue
                WHERE EntityTypeID = {EntityTypeConstants.RECIPE_DEFINITION_PARTICULARS} AND ValueName <> 'Select'
                ORDER BY ValueName;

                 --Dyeing Process
                ;SELECT CAST(DPID AS VARCHAR) AS id, DPName AS text
                FROM {TableNames.DyeingProcessPart_HK};

                --Row Item List
                With
                SG As(
	                Select SubGroupID, SubGroupName, ParticularID = EV.ValueID
	                From {DbNames.EPYSL}..ItemSubGroup ISG
	                Inner Join {DbNames.EPYSL}..EntityTypeValue EV On ISG.SubGroupName Like '%' + EV.ValueName + '%'
	                Where EV.ValueID IN (1105,1106,1107)
	                Group By SubGroupID, SubGroupName, EV.ValueID
                ),
                FR AS
                (
	                SELECT CAST(ItemMasterID AS VARCHAR) id, 
	                text = Case When SG.SubGroupName = 'Dyes' Then ISV2.SegmentValue Else ItemName End,
	                additionalValue = SG.ParticularID
	                FROM {DbNames.EPYSL}..ItemMaster IM
	                Left Join {DbNames.EPYSL}..ItemSegmentValue ISV2 On ISV2.SegmentValueID = Segment2ValueID
	                Inner Join SG On SG.SubGroupID = IM.SubGroupID
                )
                Select *, COUNT(*) Over() TotalRows
                From FR";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                YDRecipeDefinitionMaster data = records.Read<YDRecipeDefinitionMaster>().FirstOrDefault();
                data.Childs = records.Read<YDRecipeDefinitionChild>().ToList();
                if (data.Childs.IsNull()) data.Childs = new List<YDRecipeDefinitionChild>();
                data.RecipeDefinitionItemInfos = records.Read<YDRecipeDefinitionItemInfo>().ToList();
                data.RecipeForList = records.Read<Select2OptionModel>().ToList();
                data.ProcessList = records.Read<Select2OptionModel>().ToList();
                data.ParticularsList = records.Read<Select2OptionModel>().ToList();
                data.DPList = records.Read<Select2OptionModel>().ToList();
                data.RawItemList = records.Read<Select2OptionModel>().ToList();
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
        public async Task<YDRecipeDefinitionMaster> GetAsync(int id)
        {
            var query =
                $@"
                WITH MainObj AS
                (
	                SELECT RD.YDRecipeID, RD.RecipeDate,RD.YDRecipeNo, RR.GroupConceptNo ConceptNo, RD.YDRecipeReqMasterID, RR.RecipeReqNo, RD.RecipeFor,RD.BatchWeightKG,RD.Remarks, C.CCColorID, C.ColorID, C.ColorCode,
	                Color.SegmentValue ColorName,RD.IsApproved,RD.Acknowledged, RD.Temperature, RD.ProcessTime,DP.DPName,RD.DPProcessInfo,RD.DPID,RD.IsArchive, RR.YDDBatchID, RR.IsBDS,
	                Replace(ISNULL(CT.ShortName,''),'Select','R&D') Buyer, ISNULL(CCT.TeamName,'R&D') BuyerTeam, FAC.BookingID, ISNULL(FAC.LabDipNo,'') LabDipNo 
	                FROM {TableNames.YD_RECIPE_DEFINITION_MASTER} RD
	                INNER JOIN {TableNames.YD_RECIPE_REQ_MASTER} RR ON RR.YDRecipeReqMasterID = RD.YDRecipeReqMasterID
	                LEFT JOIN {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} C ON C.CCColorID = RD.CCColorID
	                LEFT JOIN {TableNames.DyeingProcessPart_HK} DP ON DP.DPID=RD.DPID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = C.ColorID
	                LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = C.ConceptID
	                LEFT JOIN {DbNames.EPYSL}..Contacts CT ON CT.ContactID = FCM.BuyerID
	                LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = FCM.BuyerTeamID
	                LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FAC On FAC.BookingChildID = FCM.BookingChildID AND FAC.ConsumptionID = FCM.ConsumptionID AND FAC.BookingID = FCM.BookingID
	                WHERE RD.YDRecipeID={id}
                ),
                FBC AS
                (
	                SELECT FAC.LabDipNo, T.BookingID
	                FROM {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD_TEXT} T
	                INNER JOIN MainObj MO ON MO.BookingID = T.BookingID
	                INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FAC On FAC.BookingChildID = T.BookingChildID AND FAC.ConsumptionID = T.ConsumptionID AND FAC.BookingID = T.BookingID
	                WHERE T.UsesIn Like '%Body%'
	                GROUP BY FAC.LabDipNo, T.BookingID
                )
                SELECT MO.*,LabDipNo = CASE WHEN ISNULL(FBC.LabDipNo,'') <> '' THEN FBC.LabDipNo ELSE MO.LabDipNo END
                FROM MainObj MO
                LEFT JOIN FBC ON FBC.BookingID = MO.BookingID;

                --Childs
                ; SELECT DI.YDRecipeDInfoID, C.Temperature, C.ProcessTime, EV.ValueName FiberPart, ISV.SegmentValue ColorName
				FROM {TableNames.YD_RECIPE_DEFINITION_CHILD} C
				INNER JOIN {TableNames.YD_RECIPE_DEFINITION_DYEING_INFO} DI ON DI.YDRecipeDInfoID = C.YDRecipeDInfoID
				INNER JOIN {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID = DI.FiberPartID
				INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = DI.ColorID
				WHERE C.YDRecipeID = {id}
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
				WHERE C.YDRecipeID = {id};

                
                ----Item Info Child
                SELECT II.YDRecipeItemInfoID, II.YDRecipeID, II.YDRecipeReqChildID, II.BookingID, II.ItemMasterID, II.ConceptID, II.SubGroupID, II.Pcs, II.Qty,
                KnittingType.TypeName KnittingType,Composition.SegmentValue FabricComposition,Construction.SegmentValue FabricConstruction,M.ConstructionID,Gsm.SegmentValue FabricGsm,
                RRC.RecipeOn,
                M.SubGroupID SubGroupID, SG.SubGroupName SubGroup, M.TechnicalNameId, TT.TechnicalName
                FROM {TableNames.YD_RECIPE_DEFINITION_ITEM_INFO} II
                LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} M ON M.ConceptID = II.ConceptID
		        LEFT JOIN {TableNames.YD_RECIPE_REQ_CHILD} RRC ON RRC.YDRecipeReqChildID = II.YDRecipeReqChildID
                LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID=M.KnittingTypeID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID=M.CompositionID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID=M.ConstructionID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID=M.GSMID
                LEFT JOIN {DbNames.EPYSL}..ItemSubGroup SG ON SG.SubGroupID = M.SubGroupID
                LEFT JOIN {TableNames.FabricTechnicalName} TT ON TT.TechnicalNameId = M.TechnicalNameId
                WHERE II.YDRecipeID = {id};

                --Recipe Definition For
                ;SELECT CAST(ValueID AS VARCHAR) id, ValueName text
                FROM {DbNames.EPYSL}..EntityTypeValue
                WHERE EntityTypeID = {EntityTypeConstants.RECIPE_DEFINITION_FOR} AND ValueName <> 'Select'
                ORDER BY ValueName;

                --Recipe Definition Process
                ;SELECT CAST(ValueID AS VARCHAR) id, ValueName text
                FROM {DbNames.EPYSL}..EntityTypeValue
                WHERE EntityTypeID = {EntityTypeConstants.RECIPE_DEFINITION_PROCESS} AND ValueName <> 'Select'
                ORDER BY ValueName;

                --Recipe Definition Particulars
                ;SELECT CAST(ValueID AS VARCHAR) id, ValueName text
                FROM {DbNames.EPYSL}..EntityTypeValue
                WHERE EntityTypeID = {EntityTypeConstants.RECIPE_DEFINITION_PARTICULARS} AND ValueName <> 'Select'
                ORDER BY ValueName;

                --Dyeing Process
                ;SELECT CAST(DPID AS VARCHAR) AS id, DPName AS text
                FROM {TableNames.DyeingProcessPart_HK};

                --Row Item List
                With
                SG As(
	                Select SubGroupID, SubGroupName, ParticularID = EV.ValueID
	                From {DbNames.EPYSL}..ItemSubGroup ISG
	                Inner Join {DbNames.EPYSL}..EntityTypeValue EV On ISG.SubGroupName Like '%' + EV.ValueName + '%'
	                Where EV.ValueID IN (1105,1106,1107)
	                Group By SubGroupID, SubGroupName, EV.ValueID
                ),
                FR AS
                (
	                SELECT CAST(ItemMasterID AS VARCHAR) id, 
	                text = Case When SG.SubGroupName = 'Dyes' Then ISV2.SegmentValue Else ItemName End,
	                additionalValue = SG.ParticularID
	                FROM {DbNames.EPYSL}..ItemMaster IM
	                Left Join {DbNames.EPYSL}..ItemSegmentValue ISV2 On ISV2.SegmentValueID = Segment2ValueID
	                Inner Join SG On SG.SubGroupID = IM.SubGroupID
                )
                Select *, COUNT(*) Over() TotalRows
                From FR";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                YDRecipeDefinitionMaster data = records.Read<YDRecipeDefinitionMaster>().FirstOrDefault();
                Guard.Against.NullObject(data.YDRecipeID);
                data.Childs = records.Read<YDRecipeDefinitionChild>().ToList();
                if (data.Childs.IsNull()) data.Childs = new List<YDRecipeDefinitionChild>();
                data.DefChilds = records.Read<YDRecipeDefinitionChild>().ToList();
                data.RecipeDefinitionItemInfos = records.Read<YDRecipeDefinitionItemInfo>().ToList();
                data.RecipeForList = records.Read<Select2OptionModel>().ToList();
                data.ProcessList = records.Read<Select2OptionModel>().ToList();
                data.ParticularsList = records.Read<Select2OptionModel>().ToList();
                data.DPList = records.Read<Select2OptionModel>().ToList();
                data.RawItemList = records.Read<Select2OptionModel>().ToList();
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
        public async Task<List<YDRecipeDefinitionMaster>> GetByRecipeReqNo(string recipeReqNo)
        {
            string query = $@"SELECT RDM.* 
                            FROM {TableNames.YD_RECIPE_DEFINITION_MASTER} RDM
                            LEFT JOIN {TableNames.YD_RECIPE_REQ_MASTER} RRM ON RRM.YDRecipeReqMasterID = RDM.YDRecipeReqMasterID
                            WHERE RRM.RecipeReqNo = '{recipeReqNo}'";
            var list = await _service.GetDataAsync<YDRecipeDefinitionMaster>(query);
            if (list == null) return new List<YDRecipeDefinitionMaster>();
            return list;
        }
        public async Task SaveAsync(YDRecipeDefinitionMaster entity)
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
                await _service.SaveAsync(entity.Childs, transaction);
                await _service.SaveAsync(entity.RecipeDefinitionItemInfos, transaction);

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
        private async Task<YDRecipeDefinitionMaster> AddAsync(YDRecipeDefinitionMaster entity, SqlTransaction transactionGmt)
        {
            entity.YDRecipeID = await _service.GetMaxIdAsync(TableNames.YD_RECIPE_DEFINITION_MASTER, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
            entity.YDRecipeNo = await _service.GetMaxNoAsync(TableNames.YD_RECIPE_DEFINITION_NO, 1, RepeatAfterEnum.NoRepeat, "00000", transactionGmt, _connectionGmt);
            var maxYRChildId = await _service.GetMaxIdAsync(TableNames.YD_RECIPE_DEFINITION_CHILD, entity.Childs.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
            var maxInfoChildId = await _service.GetMaxIdAsync(TableNames.YD_RECIPE_DEFINITION_ITEM_INFO, entity.RecipeDefinitionItemInfos.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

            foreach (var item in entity.Childs)
            {
                item.YDRecipeChildID = maxYRChildId++;
                item.YDRecipeId = entity.YDRecipeID;
                item.EntityState = EntityState.Added;
            }
            foreach (var item in entity.RecipeDefinitionItemInfos)
            {
                item.YDRecipeItemInfoID = maxInfoChildId++;
                item.YDRecipeID = entity.YDRecipeID;
                item.EntityState = EntityState.Added;
            }

            return entity;
        }

        private async Task<YDRecipeDefinitionMaster> UpdateAsync(YDRecipeDefinitionMaster entity, SqlTransaction transactionGmt)
        {
            var maxYRChildId = await _service.GetMaxIdAsync(TableNames.YD_RECIPE_DEFINITION_CHILD, entity.Childs.Where(x => x.EntityState == EntityState.Added).Count(), RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
            var maxInfoChildId = await _service.GetMaxIdAsync(TableNames.YD_RECIPE_DEFINITION_ITEM_INFO, entity.RecipeDefinitionItemInfos.Where(x => x.EntityState == EntityState.Added).Count(), RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

            foreach (var item in entity.Childs.ToList())
            {
                switch (item.EntityState)
                {
                    case EntityState.Added:
                        item.YDRecipeChildID = maxYRChildId++;
                        item.YDRecipeId = entity.YDRecipeID;
                        item.EntityState = EntityState.Added;
                        break;

                    case EntityState.Deleted:
                        item.EntityState = EntityState.Deleted;
                        break;

                    case EntityState.Modified:
                        item.EntityState = EntityState.Modified;
                        break;

                    default:
                        break;
                }
            }

            foreach (var item in entity.RecipeDefinitionItemInfos.ToList())
            {
                switch (item.EntityState)
                {
                    case EntityState.Added:
                        item.YDRecipeItemInfoID = maxInfoChildId++;
                        item.YDRecipeID = entity.YDRecipeID;
                        item.EntityState = EntityState.Added;
                        break;

                    case EntityState.Deleted:
                    case EntityState.Unchanged:
                        item.EntityState = EntityState.Deleted;
                        break;

                    case EntityState.Modified:
                        item.EntityState = EntityState.Modified;
                        break;

                    default:
                        break;
                }
            }

            return entity;
        }
        public async Task<YDRecipeDefinitionMaster> GetAllByIDAsync(int id)
        {
            string sql = $@"
            ;Select RD.*, RR.YDDBatchID, RR.IsBDS, RR.GroupConceptNo
			FROM {TableNames.YD_RECIPE_DEFINITION_MASTER} RD
			INNER JOIN {TableNames.YD_RECIPE_REQ_MASTER} RR ON RR.YDRecipeReqMasterID = RD.YDRecipeReqMasterID
			Where YDRecipeID = {id}

            ;Select * FROM {TableNames.YD_RECIPE_DEFINITION_CHILD} Where YDRecipeID = {id}

            ;Select * FROM {TableNames.YD_RECIPE_DEFINITION_ITEM_INFO} Where YDRecipeID = {id}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YDRecipeDefinitionMaster data = records.Read<YDRecipeDefinitionMaster>().FirstOrDefault();
                data.Childs = records.Read<YDRecipeDefinitionChild>().ToList();
                if (data.Childs.IsNull()) data.Childs = new List<YDRecipeDefinitionChild>();
                data.RecipeDefinitionItemInfos = records.Read<YDRecipeDefinitionItemInfo>().ToList();
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
        public async Task<List<YDBatchMaster>> GetBatchDetails(string batchIds)
        {
            var sql = $@"
                    ;SELECT	* FROM {TableNames.YD_BATCH_MASTER} BM WHERE BM.YDBatchID IN ({batchIds});

                    ;SELECT * FROM {TableNames.YD_BATCH_ITEM_REQUIREMENT} BM WHERE BM.YDBatchID IN ({batchIds});

                    ;SELECT * FROM {TableNames.YD_BATCH_CHILD} BM WHERE BM.YDBatchID IN ({batchIds});

                    ;SELECT * FROM {TableNames.YD_BATCH_WISE_RECIPE_CHILD} BM WHERE BM.YDBatchID IN ({batchIds});
                    ";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                List<YDBatchMaster> datas = records.Read<YDBatchMaster>().ToList();
                List<YDBatchItemRequirement> batchItemRequirements = records.Read<YDBatchItemRequirement>().ToList();
                List<YDBatchChild> batchChilds = records.Read<YDBatchChild>().ToList();
                List<YDBatchWiseRecipeChild> batchWiseRecipeChilds = records.Read<YDBatchWiseRecipeChild>().ToList();

                foreach (YDBatchMaster batch in datas)
                {
                    batch.YDBatchItemRequirements = batchItemRequirements.Where(x => x.YDBatchID == batch.YDBatchID).ToList();
                    foreach (YDBatchItemRequirement item in batch.YDBatchItemRequirements)
                    {
                        item.YDBatchChilds = batchChilds.Where(x => x.BItemReqID == item.YDBItemReqID).ToList();
                    }
                    batch.YDBatchWiseRecipeChilds = batchWiseRecipeChilds.Where(x => x.YDBatchID == batch.YDBatchID).ToList();
                }

                return datas;
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
        public async Task UpdateRecipeId(YDRecipeDefinitionMaster entity, int YDDBatchID)
        {
            #region Update RecipeID (Tables : YDBatchMaster, YDDyeingBatchMaster, YDDyeingBatchItem)

            if (!entity.GroupConceptNo.IsNullOrEmpty() && entity.ColorID > 0 && entity.YDRecipeReqMasterID > 0 && entity.YDRecipeID > 0)
            {
                if (YDDBatchID == 0)
                {
                    string query = $"UPDATE {TableNames.YD_BATCH_MASTER} SET YDRecipeID={entity.YDRecipeID} WHERE YDRecipeID = 0 AND GroupConceptNo = '{entity.GroupConceptNo}' AND ColorID = {entity.ColorID} AND CCColorID = {entity.CCColorID};";
                    await _batchService.ExecuteAsync(query, AppConstants.TEXTILE_CONNECTION);

                    query = $@"UPDATE A
					            SET A.YDRecipeID = {entity.YDRecipeID}
					            FROM {TableNames.YD_DYEING_BATCH_MASTER} A
					            LEFT JOIN {TableNames.YD_DYEING_BATCH_WITH_BATCH_MASTER} B ON B.YDDBatchID=A.YDDBatchID
                                LEFT JOIN {TableNames.YD_BATCH_MASTER} BM ON BM.YDBatchID=B.YDBatchID AND BM.ColorID=A.ColorID
                                WHERE A.YDRecipeID = 0 AND BM.GroupConceptNo = '{entity.GroupConceptNo}' AND A.ColorID = {entity.ColorID} AND A.CCColorID = {entity.CCColorID};";
                    await _dyeingBatchService.ExecuteAsync(query, AppConstants.TEXTILE_CONNECTION);

                    query = $@"Update b Set YDRecipeID = c.YDRecipeID
                            FROM {TableNames.YD_DYEING_BATCH_MASTER} a
                            Inner Join {TableNames.YD_DYEING_BATCH_ITEM} b On b.YDDBatchID = a.YDDBatchID
                            Inner JOIN {TableNames.YD_RECIPE_DEFINITION_MASTER} c ON c.CCColorID = a.CCColorID and c.ColorID = a.ColorID
                            Inner JOIN {TableNames.YD_RECIPE_REQ_MASTER} d ON d.CCColorID = a.CCColorID and d.ColorID = a.ColorID
                            Where b.YDRecipeID = 0 AND d.GroupConceptNo = '{entity.GroupConceptNo}' AND a.ColorID = {entity.ColorID} AND A.CCColorID={entity.CCColorID};";

                    await _dyeingBatchItemService.ExecuteAsync(query, AppConstants.TEXTILE_CONNECTION);
                }
                else
                {
                    string query = $@"declare @BatchID int;
                                      set @BatchID = (select distinct DBI.YDBatchID from YDDyeingBatchItem DBI where YDDBatchID = {YDDBatchID})
                                      UPDATE {TableNames.YD_BATCH_MASTER} SET YDRecipeID={entity.YDRecipeID} WHERE YDRecipeID = 0 AND YDBatchID=@BatchID;";
                    await _batchService.ExecuteAsync(query, AppConstants.TEXTILE_CONNECTION);

                    //query = $@"Update DyeingBatchMaster Set RecipeID= {entity.RecipeID} where RecipeID = 0 AND  DBatchID={DBatchID}";
                    query = $@"UPDATE A
					            SET A.YDRecipeID = {entity.YDRecipeID}
					            FROM {TableNames.YD_DYEING_BATCH_MASTER} A
					            LEFT JOIN {TableNames.YD_DYEING_BATCH_WITH_BATCH_MASTER} B ON B.YDDBatchID=A.YDDBatchID
                                LEFT JOIN {TableNames.YD_BATCH_MASTER} BM ON BM.YDBatchID=B.YDBatchID AND BM.ColorID=A.ColorID
                                WHERE A.YDRecipeID = 0 AND  A.YDDBatchID={YDDBatchID};";
                    await _dyeingBatchService.ExecuteAsync(query, AppConstants.TEXTILE_CONNECTION);

                    query = $@"Update b Set YDRecipeID = c.YDRecipeID
                            FROM {TableNames.YD_DYEING_BATCH_MASTER} a
                            Inner Join {TableNames.YD_DYEING_BATCH_ITEM} b On b.YDDBatchID = a.YDDBatchID
                            Inner JOIN {TableNames.YD_RECIPE_DEFINITION_MASTER} c ON c.CCColorID = a.CCColorID and c.ColorID = a.ColorID
                            Inner JOIN {TableNames.YD_RECIPE_REQ_MASTER} d ON d.CCColorID = a.CCColorID and d.ColorID = a.ColorID
                            Where b.YDRecipeID = 0 AND  b.YDDBatchID={YDDBatchID};";

                    await _dyeingBatchItemService.ExecuteAsync(query, AppConstants.TEXTILE_CONNECTION);
                }
            }
            #endregion
        }
        public async Task UpdateEntityAsync(YDRecipeDefinitionMaster entity)
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
        public async Task UpdateRecipeWithBatchAsync(YDRecipeDefinitionMaster entity, YDDyeingBatchMaster dyeingBatchEntity, List<YDBatchMaster> batchEntities)
        {
            SqlTransaction transaction = null;
            SqlTransaction transactionGmt = null;
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                await _connectionGmt.OpenAsync();
                transactionGmt = _connectionGmt.BeginTransaction();

                int maxDyeingBatchRecipeID = await _service.GetMaxIdAsync(TableNames.DYEING_BATCH_RECIPE, entity.Childs.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                int maxBatchWiseRecipeChildID = await _service.GetMaxIdAsync(TableNames.YD_RECIPE_DEFINITION_ITEM_INFO, entity.Childs.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

                await _service.SaveSingleAsync(entity, transaction);

                ////DyeingBatchMaster
                dyeingBatchEntity.YDRecipeID = entity.YDRecipeID;
                dyeingBatchEntity.EntityState = EntityState.Modified;
                await _service.SaveSingleAsync(dyeingBatchEntity, transaction);
                ////DyeingBatchItem
                dyeingBatchEntity.YDDyeingBatchItems.ForEach(dbItem =>
                {
                    dbItem.YDRecipeID = entity.YDRecipeID;
                    dbItem.EntityState = EntityState.Modified;
                });
                await _service.SaveAsync(dyeingBatchEntity.YDDyeingBatchItems, transaction);
                ////DyeingBatchRecipe
                entity.Childs.ForEach(item =>
                {
                    YDDyeingBatchRecipe oBatchRecipe = new YDDyeingBatchRecipe();
                    oBatchRecipe.YDDBRID = maxDyeingBatchRecipeID++;
                    oBatchRecipe.YDDBatchID = entity.YDDBatchID;
                    oBatchRecipe.YDRecipeID = item.YDRecipeId;
                    oBatchRecipe.YDRecipeChildID = item.YDRecipeChildID;
                    oBatchRecipe.ProcessID = item.ProcessID;
                    oBatchRecipe.ParticularsID = item.ParticularsID;
                    oBatchRecipe.RawItemID = item.RawItemID;
                    oBatchRecipe.Qty = item.Qty;
                    oBatchRecipe.UnitID = item.UnitID;
                    oBatchRecipe.TempIn = item.TempIn;
                    oBatchRecipe.TempOut = item.TempOut;
                    oBatchRecipe.ProcessTime = item.ProcessTime;
                    oBatchRecipe.EntityState = EntityState.Added;
                    dyeingBatchEntity.YDDyeingBatchRecipes.Add(oBatchRecipe);
                });
                await _service.SaveAsync(dyeingBatchEntity.YDDyeingBatchRecipes, transaction);

                ////Batch Master
                batchEntities.ForEach(batch =>
                {
                    batch.YDRecipeID = entity.YDRecipeID;
                    batch.EntityState = EntityState.Modified;

                    //BatchItemRequirement
                    entity.RecipeDefinitionItemInfos.ForEach(rdItemInfo =>
                    {
                        List<YDBatchItemRequirement> bItemReqs = batch.YDBatchItemRequirements.Where(x => x.YDBatchID == batch.YDBatchID && x.ItemMasterID == rdItemInfo.ItemMasterID).ToList();
                        bItemReqs.ForEach(x =>
                        {
                            x.YDRecipeItemInfoID = rdItemInfo.YDRecipeItemInfoID;
                            x.EntityState = EntityState.Modified;
                        });
                    });

                    //BatchWiseRecipeChild
                    entity.Childs.ForEach(item =>
                    {
                        YDBatchWiseRecipeChild oBatchWiseRecipeChild = new YDBatchWiseRecipeChild();
                        oBatchWiseRecipeChild.YDBRecipeChildID = maxBatchWiseRecipeChildID++;
                        oBatchWiseRecipeChild.YDBatchID = batch.YDBatchID;
                        oBatchWiseRecipeChild.YDRecipeChildID = item.YDRecipeChildID;
                        oBatchWiseRecipeChild.ProcessID = item.ProcessID;
                        oBatchWiseRecipeChild.ParticularsID = item.ParticularsID;
                        oBatchWiseRecipeChild.RawItemID = item.RawItemID;
                        oBatchWiseRecipeChild.Qty = item.Qty;
                        oBatchWiseRecipeChild.UnitID = item.UnitID;
                        oBatchWiseRecipeChild.TempIn = item.TempIn;
                        oBatchWiseRecipeChild.TempOut = item.TempOut;
                        oBatchWiseRecipeChild.ProcessTime = item.ProcessTime;
                        oBatchWiseRecipeChild.EntityState = EntityState.Added;
                        batch.YDBatchWiseRecipeChilds.Add(oBatchWiseRecipeChild);
                    });

                });

                await _service.SaveAsync(batchEntities, transaction);
                List<YDBatchItemRequirement> batchItemRequirements = new List<YDBatchItemRequirement>();
                List<YDBatchWiseRecipeChild> batchWiseRecipeChilds = new List<YDBatchWiseRecipeChild>();
                batchEntities.ForEach(batch =>
                {
                    batchItemRequirements.AddRange(batch.YDBatchItemRequirements);
                    batchWiseRecipeChilds.AddRange(batch.YDBatchWiseRecipeChilds);
                });
                await _service.SaveAsync(batchItemRequirements, transaction);
                await _service.SaveAsync(batchWiseRecipeChilds, transaction);

                //UpdateRecipeId(entity);

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
        public async Task<List<YDRecipeDefinitionMaster>> GetAllApproveListForCopy(string dpID, string buyer, string fabricComposition, string color, PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By YDRecipeID Desc" : paginationInfo.OrderBy;
            string fComposition = fabricComposition;

            if (fabricComposition.IsNotNullOrEmpty())
            {
                fComposition = "";
                fabricComposition.Split(',').ToList().ForEach(c =>
                {
                    fComposition += "'" + c + "',";
                });
                fComposition = fComposition.TrimEnd(',');
            }

            string sql =
            $@";With M As(
                 Select YDRecipeID, YDRecipeNo, YDRecipeReqMasterID, RecipeDate, BatchWeightKG, Remarks, Temperature, ConceptID, ProcessTime, DPProcessInfo, DPID, ColorID, CCColorID, RecipeFor 
                 FROM {TableNames.YD_RECIPE_DEFINITION_MASTER}
                 Where IsApproved=1 And BuyerID = {buyer} And ColorID = {color}
                ), RR AS (
                 select M.YDRecipeID, M.YDRecipeNo, M.YDRecipeReqMasterID, M.RecipeDate,RF.ValueName RecipeForName,M.BatchWeightKG,M.Remarks,C.SegmentValue ColorName,
                 M.ConceptID, FCM.GroupConceptNo ConceptNo, M.Temperature, M.ProcessTime,DP.DPName,M.DPProcessInfo,
                 Replace(ISNULL(CT.ShortName,''),'Select','R&D') Buyer, ISNULL(CCT.TeamName,'R&D') BuyerTeam, ISNULL(FAC.LabDipNo,'') LabDipNo 
                 from M
                 LEFT JOIN {TableNames.DyeingProcessPart_HK} DP ON DP.DPID=M.DPID
                 LEFT JOIN {DbNames.EPYSL}..EntityTypeValue RF ON RF.ValueID = M.RecipeFor
                 left JOIN {DbNames.EPYSL}..ItemSegmentValue C ON C.SegmentValueID = M.ColorID
                 INNER JOIN {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} CCC ON CCC.CCColorID = M.CCColorID
                 LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = CCC.ConceptID
                 LEFT JOIN {DbNames.EPYSL}..Contacts CT ON CT.ContactID = FCM.BuyerID
                 LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = FCM.BuyerTeamID
                 LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FAC On FAC.BookingChildID = FCM.BookingChildID AND FAC.ConsumptionID = FCM.ConsumptionID AND FAC.BookingID = FCM.BookingID
                 left JOIN {DbNames.EPYSL}..ItemSegmentValue com ON com.SegmentValueID = FCM.CompositionID
                 WHERE com.SegmentValue in ({fComposition})
                 Group By M.YDRecipeID, M.YDRecipeNo, M.YDRecipeReqMasterID, M.RecipeDate,RF.ValueName,M.BatchWeightKG,M.Remarks,C.SegmentValue,
                 M.ConceptID, FCM.GroupConceptNo, M.Temperature, M.ProcessTime,DP.DPName,M.DPProcessInfo,
                 Replace(ISNULL(CT.ShortName,''),'Select','R&D'), ISNULL(CCT.TeamName,'R&D'), ISNULL(FAC.LabDipNo,'')
                )
                SELECT *, COUNT(*) OVER() TotalRows FROM RR
                ";

            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<YDRecipeDefinitionMaster>(sql);
        }
        public async Task<List<YDRecipeDefinitionMaster>> GetConceptWiseRecipeForCopy(string fabricComposition, string color, string groupConceptNo, PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By YDRecipeID Desc" : paginationInfo.OrderBy;
            string fComposition = fabricComposition;

            if (fabricComposition.IsNotNullOrEmpty())
            {
                fComposition = "";
                fabricComposition.Split(',').ToList().ForEach(c =>
                {
                    fComposition += "'" + c + "',";
                });
                fComposition = fComposition.TrimEnd(',');
            }

            string sql =
            $@";With M As(
	                Select RDM.YDRecipeID, RDM.YDRecipeNo, RDM.YDRecipeReqMasterID, RDM.RecipeDate, RDM.BatchWeightKG, RDM.Remarks, RDM.Temperature, RDM.ConceptID, RDM.ProcessTime, RDM.DPProcessInfo, RDM.DPID, RDM.ColorID, RDM.CCColorID, RDM.RecipeFor 
	                FROM {TableNames.YD_RECIPE_DEFINITION_MASTER} RDM
					inner JOIN {TableNames.YD_RECIPE_REQ_MASTER} RRM on RRM.YDRecipeReqMasterID=RDM.YDRecipeReqMasterID
					inner JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM on FCM.ConceptNo=RRM.GroupConceptNo
	                Where IsApproved=1 And FCM.GroupConceptNo = '{groupConceptNo}' And RDM.ColorID = '{color}'
                ), RR AS (
	                select M.YDRecipeID, M.YDRecipeNo, M.YDRecipeReqMasterID, M.RecipeDate,RF.ValueName RecipeForName,M.BatchWeightKG,M.Remarks,C.SegmentValue ColorName,
	                M.ConceptID, FCM.GroupConceptNo ConceptNo, M.Temperature, M.ProcessTime,DP.DPName,M.DPProcessInfo,
	                Replace(ISNULL(CT.ShortName,''),'Select','R&D') Buyer, ISNULL(CCT.TeamName,'R&D') BuyerTeam, ISNULL(FAC.LabDipNo,'') LabDipNo 
                    ,KMT.TypeName KnittingType, Gsm.SegmentValue Gsm,TT.TechnicalName, Composition.SegmentValue Composition	                
                    from M
	                LEFT JOIN {TableNames.DyeingProcessPart_HK} DP ON DP.DPID=M.DPID
	                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue RF ON RF.ValueID = M.RecipeFor
	                left JOIN {DbNames.EPYSL}..ItemSegmentValue C ON C.SegmentValueID = M.ColorID
	                INNER JOIN {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} CCC ON CCC.CCColorID = M.CCColorID
	                LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = CCC.ConceptID
					LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KMT ON FCM.KnittingTypeID=KMT.TypeID
					LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID=FCM.GSMID
					LEFT JOIN {TableNames.FabricTechnicalName} TT ON TT.TechnicalNameId = FCM.TechnicalNameId
					LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID=FCM.CompositionID
	                LEFT JOIN {DbNames.EPYSL}..Contacts CT ON CT.ContactID = FCM.BuyerID
	                LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = FCM.BuyerTeamID
	                LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FAC On FAC.BookingChildID = FCM.BookingChildID AND FAC.ConsumptionID = FCM.ConsumptionID AND FAC.BookingID = FCM.BookingID
	                left JOIN {DbNames.EPYSL}..ItemSegmentValue com ON com.SegmentValueID = FCM.CompositionID
	                WHERE com.SegmentValue in ({fComposition})
	                Group By M.YDRecipeID, M.YDRecipeNo, M.YDRecipeReqMasterID, M.RecipeDate,RF.ValueName,M.BatchWeightKG,M.Remarks,C.SegmentValue,
	                M.ConceptID, FCM.GroupConceptNo, M.Temperature, M.ProcessTime,DP.DPName,M.DPProcessInfo,
	                Replace(ISNULL(CT.ShortName,''),'Select','R&D'), ISNULL(CCT.TeamName,'R&D'), ISNULL(FAC.LabDipNo,'')
                    ,KMT.TypeName, Gsm.SegmentValue,TT.TechnicalName, Composition.SegmentValue
                )
                SELECT *, COUNT(*) OVER() TotalRows FROM RR
                ";
            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<YDRecipeDefinitionMaster>(sql);
        }
        public async Task<YDRecipeDefinitionMaster> GetRecipeDyeingInfo(int id)
        {
            var query =
                $@"
                    --Childs
                    WITH 
					RD AS (
						SELECT DI.YDRecipeDInfoID, DI.YDRecipeReqMasterID, DI.CCColorID, DI.YDRecipeID, DI.FiberPartID,
						DI.ColorID, DI.RecipeOn,
						EV.ValueName FiberPart, ISV.SegmentValue ColorName
						FROM {TableNames.YD_RECIPE_DEFINITION_DYEING_INFO} DI
						INNER JOIN {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID = DI.FiberPartID
						INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = DI.ColorID
						WHERE DI.YDRecipeReqMasterID = {id}
					),
					RDC AS 
					(
						SELECT C.YDRecipeDInfoID, C.Temperature, C.ProcessTime
						FROM {TableNames.YD_RECIPE_DEFINITION_CHILD} C
						INNER JOIN RD ON RD.YDRecipeDInfoID = C.YDRecipeDInfoID
						GROUP BY C.YDRecipeDInfoID, C.Temperature, C.ProcessTime
					),
					FinalList AS
					(
						SELECT RD.*, RDC.Temperature, RDC.ProcessTime
						FROM RD
						LEFT JOIN RDC ON RDC.YDRecipeDInfoID = RD.YDRecipeDInfoID
					)
					SELECT * FROM FinalList

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
                    WHERE DI.YDRecipeReqMasterID = {id};
                   ";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                YDRecipeDefinitionMaster data = new YDRecipeDefinitionMaster();
                data.Childs = records.Read<YDRecipeDefinitionChild>().ToList();
                if (data.Childs.IsNull()) data.Childs = new List<YDRecipeDefinitionChild>();
                data.DefChilds = records.Read<YDRecipeDefinitionChild>().ToList();
                data.Childs.ForEach(c =>
                {
                    c.DefChilds = data.DefChilds.Where(d => d.YDRecipeDInfoID == c.YDRecipeDInfoID).ToList();
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

        //
        public async Task<YDDyeingBatchMaster> GetAllByIDAsyncYDDBM(int id)
        {
            string sql = $@"
            ;Select * FROM {TableNames.YD_DYEING_BATCH_MASTER} Where YDDBatchID = {id}

            ;Select * From {TableNames.YD_DYEING_BATCH_WITH_BATCH_MASTER} Where YDDBatchID = {id}

            ;Select * From {TableNames.YD_DYEING_BATCH_RECIPE} Where YDDBatchID = {id}

            ;Select * From {TableNames.YD_DYEING_BATCH_ITEM} Where YDDBatchID = {id}

            ;Select * From {TableNames.YD_DYEING_BATCH_ITEM_ROLL} Where YDDBatchID = {id}

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
    }
}
