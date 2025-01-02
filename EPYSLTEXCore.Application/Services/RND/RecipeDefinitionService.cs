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
    public class RecipeDefinitionService : IRecipeDefinitionService
    {
        private readonly IDapperCRUDService<RecipeDefinitionMaster> _service;
        private readonly SqlConnection _connection;
        private readonly SqlConnection _connectionGmt;
        private SqlTransaction transaction;
        private SqlTransaction transactionGmt;

        private readonly IDapperCRUDService<DyeingBatchMaster> _dyeingBatchService;
        private readonly IDapperCRUDService<BatchMaster> _batchService;
        private readonly IDapperCRUDService<DyeingBatchItem> _dyeingBatchItemService;
        public RecipeDefinitionService(IDapperCRUDService<RecipeDefinitionMaster> service,
            IDapperCRUDService<DyeingBatchMaster> dyeingBatchService,
            IDapperCRUDService<BatchMaster> batchService,
            IDapperCRUDService<DyeingBatchItem> dyeingBatchItemService
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

        public async Task<List<RecipeDefinitionMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By RecipeID Desc" : paginationInfo.OrderBy;

            string sql;
            if (status == Status.Pending)
            {
                sql =
                $@"WITH NewList AS
                (
	                select RM.RecipeReqMasterID, RM.RecipeReqNo, RM.CCColorID, RM.GroupConceptNo ConceptNo,M.ConceptDate,C.ColorCode,
	                Color.SegmentValue ColorName, DP.DPName, RM.DPProcessInfo, RM.AcknowledgeDate RequestAckDate, 
	                Replace(ISNULL(CT.ShortName,''),'Select','R&D') Buyer, ISNULL(CCT.TeamName,'R&D') BuyerTeam, 
	                ISNULL(FAC.LabDipNo,'') LabDipNo1, M.BookingID
	                ,[Status] = 'New'
	                from {TableNames.RND_RECIPE_REQ_MASTER} RM
	                INNER JOIN {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} C ON C.CCColorID = RM.CCColorID
	                LEFT JOIN {TableNames.DyeingProcessPart_HK} DP ON DP.DPID=RM.DPID
	                LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} M ON M.ConceptID = C.ConceptID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = RM.ColorID
	                LEFT JOIN {TableNames.RND_RECIPE_DEFINITION_MASTER} RD ON RD.CCColorID = RM.CCColorID
	                LEFT JOIN {DbNames.EPYSL}..Contacts CT ON CT.ContactID = M.BuyerID
	                LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = M.BuyerTeamID
	                LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FAC On FAC.BookingChildID = M.BookingChildID AND FAC.ConsumptionID = M.ConsumptionID AND FAC.BookingID = M.BookingID
                    LEFT JOIN {TableNames.RND_RECIPE_DEFINITION_MASTER} RDM ON RDM.RecipeReqMasterID = RM.RecipeReqMasterID 	                
                    WHERE ISNULL(RM.Acknowledge,0) = 1 AND RDM.RecipeID IS NULL --AND RD.CCColorID IS NULL
	                Group By RM.RecipeReqMasterID, RM.RecipeReqNo, RM.CCColorID, RM.GroupConceptNo,M.ConceptDate,C.ColorCode,
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
	                select RM.RecipeReqMasterID, RM.RecipeReqNo, RM.CCColorID, RM.GroupConceptNo ConceptNo,M.ConceptDate,C.ColorCode,
	                Color.SegmentValue ColorName, DP.DPName, RM.DPProcessInfo, RM.AcknowledgeDate RequestAckDate, 
	                Replace(ISNULL(CT.ShortName,''),'Select','R&D') Buyer, ISNULL(CCT.TeamName,'R&D') BuyerTeam, 
	                ISNULL(FAC.LabDipNo,'') LabDipNo1, M.BookingID
	                ,[Status] = 'Rework'
	                from {TableNames.RND_RECIPE_REQ_MASTER} RM
	                INNER JOIN {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} C ON C.CCColorID = RM.CCColorID
	                LEFT JOIN {TableNames.DyeingProcessPart_HK} DP ON DP.DPID=RM.DPID
	                LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} M ON M.ConceptID = C.ConceptID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = RM.ColorID
	                LEFT JOIN {TableNames.RND_RECIPE_DEFINITION_MASTER} RD ON RD.CCColorID = RM.CCColorID
	                LEFT JOIN {DbNames.EPYSL}..Contacts CT ON CT.ContactID = M.BuyerID
	                LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = M.BuyerTeamID
	                LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FAC On FAC.BookingChildID = M.BookingChildID AND FAC.ConsumptionID = M.ConsumptionID AND FAC.BookingID = M.BookingID
	                LEFT JOIN {TableNames.RND_RECIPE_DEFINITION_MASTER} RDM ON RDM.RecipeReqMasterID = RM.RecipeReqMasterID
	                INNER JOIN {TableNames.DYEING_BATCH_MASTER} DB ON DB.CCColorID = C.CCColorID
	                INNER JOIN {TableNames.DYEING_BATCH_REWORK} DBR ON DBR.DBatchID = DB.DBatchID
	                WHERE ISNULL(DBR.BatchStatus,0) = 3 AND ISNULL(DBR.IsNewBatch,0) = 1 AND ISNULL(DBR.IsNewRecipe,0) = 1
	                AND ISNULL(RM.Acknowledge,0) = 1 AND RDM.RecipeID IS NULL
	                Group By RM.RecipeReqMasterID, RM.RecipeReqNo, RM.CCColorID, RM.GroupConceptNo,M.ConceptDate,C.ColorCode,
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

                orderBy = " ORDER BY RecipeReqMasterID DESC";
            }
            else if (status == Status.PartiallyCompleted)
            {
                sql =
                $@"With RR AS (
	                select M.RecipeID, M.RecipeNo, M.RecipeReqMasterID, M.RecipeDate,RF.ValueName RecipeForName,M.BatchWeightKG,M.Remarks,C.SegmentValue ColorName,
	                M.ConceptID, RM.RecipeReqNo, RM.GroupConceptNo ConceptNo, M.Temperature, M.ProcessTime,DP.DPName,M.DPProcessInfo,
	                Replace(ISNULL(CT.ShortName,''),'Select','R&D') Buyer, ISNULL(CCT.TeamName,'R&D') BuyerTeam, ISNULL(FAC.LabDipNo,'') LabDipNo1, FCM.BookingID 
	                ,RecipeStatus = CASE WHEN ISNULL(M.IsActive,0) = 0 THEN 'Deactive' ELSE 'Active' END
	                from {TableNames.RND_RECIPE_DEFINITION_MASTER} M
	                INNER JOIN {TableNames.RND_RECIPE_REQ_MASTER} RM ON RM.RecipeReqMasterID = M.RecipeReqMasterID
	                LEFT JOIN {TableNames.DyeingProcessPart_HK} DP ON DP.DPID=M.DPID
	                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue RF ON RF.ValueID = M.RecipeFor
	                left JOIN {DbNames.EPYSL}..ItemSegmentValue C ON C.SegmentValueID = M.ColorID
	                INNER JOIN {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} CCC ON CCC.CCColorID = RM.CCColorID
	                LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = CCC.ConceptID
	                LEFT JOIN {DbNames.EPYSL}..Contacts CT ON CT.ContactID = FCM.BuyerID
	                LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = FCM.BuyerTeamID
	                LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FAC On FAC.BookingChildID = FCM.BookingChildID AND FAC.ConsumptionID = FCM.ConsumptionID AND FAC.BookingID = FCM.BookingID
	                WHERE ISNULL(M.IsApproved,0)=0 AND ISNULL(M.Acknowledged,0)=0
	                Group By M.RecipeID, M.RecipeNo, M.RecipeReqMasterID, M.RecipeDate,RF.ValueName,M.BatchWeightKG,M.Remarks,C.SegmentValue,
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
                    select M.RecipeID, M.RecipeNo, M.RecipeReqMasterID, M.RecipeDate,RF.ValueName RecipeForName,M.BatchWeightKG,M.Remarks,C.SegmentValue ColorName,
                    M.ConceptID, RM.RecipeReqNo, RM.GroupConceptNo ConceptNo, M.Temperature, M.ProcessTime,DP.DPName,M.DPProcessInfo,
                    Replace(ISNULL(CT.ShortName,''),'Select','R&D') Buyer, ISNULL(CCT.TeamName,'R&D') BuyerTeam, ISNULL(FAC.LabDipNo,'') LabDipNo1,FCM.BookingID 
                    ,RecipeStatus = CASE WHEN ISNULL(M.IsActive,0) = 0 THEN 'Deactive' ELSE 'Active' END
                    from {TableNames.RND_RECIPE_DEFINITION_MASTER} M
                    INNER JOIN {TableNames.RND_RECIPE_REQ_MASTER} RM ON RM.RecipeReqMasterID = M.RecipeReqMasterID
                    LEFT JOIN {TableNames.DyeingProcessPart_HK} DP ON DP.DPID=M.DPID
                    LEFT JOIN {DbNames.EPYSL}..EntityTypeValue RF ON RF.ValueID = M.RecipeFor
                    left JOIN {DbNames.EPYSL}..ItemSegmentValue C ON C.SegmentValueID = M.ColorID
                    INNER JOIN {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} CCC ON CCC.CCColorID = RM.CCColorID
                    LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = CCC.ConceptID
                    LEFT JOIN {DbNames.EPYSL}..Contacts CT ON CT.ContactID = FCM.BuyerID
                    LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = FCM.BuyerTeamID
                    LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FAC On FAC.BookingChildID = FCM.BookingChildID AND FAC.ConsumptionID = FCM.ConsumptionID AND FAC.BookingID = FCM.BookingID
                    WHERE ISNULL(M.IsApproved,0)=1 AND ISNULL(M.Acknowledged,0)=0
                    Group By M.RecipeID, M.RecipeNo, M.RecipeReqMasterID, M.RecipeDate,RF.ValueName,M.BatchWeightKG,M.Remarks,C.SegmentValue,
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
	                select M.RecipeID, M.RecipeNo, M.RecipeReqMasterID, M.RecipeDate,RF.ValueName RecipeForName,M.BatchWeightKG,M.Remarks,C.SegmentValue ColorName,
	                M.ConceptID, RM.RecipeReqNo, RM.GroupConceptNo ConceptNo, M.Temperature, M.ProcessTime,DP.DPName,M.DPProcessInfo,M.AcknowledgedDate,
	                Replace(ISNULL(CT.ShortName,''),'Select','R&D') Buyer, ISNULL(CCT.TeamName,'R&D') BuyerTeam, ISNULL(FAC.LabDipNo,'') LabDipNo1, FCM.BookingID
	                ,RecipeStatus = CASE WHEN ISNULL(M.IsActive,0) = 0 THEN 'Deactive' ELSE 'Active' END				
	                from {TableNames.RND_RECIPE_DEFINITION_MASTER} M
	                INNER JOIN {TableNames.RND_RECIPE_REQ_MASTER} RM ON RM.RecipeReqMasterID = M.RecipeReqMasterID
	                LEFT JOIN {TableNames.DyeingProcessPart_HK} DP ON DP.DPID=M.DPID
	                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue RF ON RF.ValueID = M.RecipeFor
	                left JOIN {DbNames.EPYSL}..ItemSegmentValue C ON C.SegmentValueID = M.ColorID
	                INNER JOIN {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} CCC ON CCC.CCColorID = RM.CCColorID
	                LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = CCC.ConceptID
	                LEFT JOIN {DbNames.EPYSL}..Contacts CT ON CT.ContactID = FCM.BuyerID
	                LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = FCM.BuyerTeamID
	                LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FAC On FAC.BookingChildID = FCM.BookingChildID AND FAC.ConsumptionID = FCM.ConsumptionID AND FAC.BookingID = FCM.BookingID
	                WHERE ISNULL(M.IsApproved,0)=1 AND ISNULL(M.Acknowledged,0)=1
	                Group By M.RecipeID, M.RecipeNo, M.RecipeReqMasterID, M.RecipeDate,RF.ValueName,M.BatchWeightKG,M.Remarks,C.SegmentValue,
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
	                select M.RecipeID, M.RecipeNo, M.RecipeReqMasterID, M.RecipeDate,RF.ValueName RecipeForName,M.BatchWeightKG,M.Remarks,C.SegmentValue ColorName,
	                M.ConceptID, RM.RecipeReqNo, RM.GroupConceptNo ConceptNo, M.Temperature, M.ProcessTime,DP.DPName,M.DPProcessInfo,
                    Replace(ISNULL(CT.ShortName,''),'Select','R&D') Buyer, ISNULL(CCT.TeamName,'R&D') BuyerTeam, ISNULL(FAC.LabDipNo,'') LabDipNo1, FCM.BookingID
                    ,RecipeStatus = CASE WHEN ISNULL(M.IsActive,0) = 0 THEN 'Deactive' ELSE 'Active' END				
                    from {TableNames.RND_RECIPE_DEFINITION_MASTER} M
	                INNER JOIN {TableNames.RND_RECIPE_REQ_MASTER} RM ON RM.RecipeReqMasterID = M.RecipeReqMasterID
                    LEFT JOIN {TableNames.DyeingProcessPart_HK} DP ON DP.DPID=M.DPID
	                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue RF ON RF.ValueID = M.RecipeFor
	                left JOIN {DbNames.EPYSL}..ItemSegmentValue C ON C.SegmentValueID = M.ColorID
	                INNER JOIN {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} CCC ON CCC.CCColorID = RM.CCColorID
                    LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = CCC.ConceptID
	                LEFT JOIN {DbNames.EPYSL}..Contacts CT ON CT.ContactID = FCM.BuyerID
	                LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = FCM.BuyerTeamID
	                LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FAC On FAC.BookingChildID = FCM.BookingChildID AND FAC.ConsumptionID = FCM.ConsumptionID AND FAC.BookingID = FCM.BookingID
                    Group By M.RecipeID, M.RecipeNo, M.RecipeReqMasterID, M.RecipeDate,RF.ValueName,M.BatchWeightKG,M.Remarks,C.SegmentValue,
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

            return await _service.GetDataAsync<RecipeDefinitionMaster>(sql);
        }

        public async Task<RecipeDefinitionMaster> GetNewAsync(int id)
        {
            var query =
                $@"
                WITH MainObj AS
                (
	                SELECT RM.RecipeReqMasterID, RM.RecipeReqNo, RM.CCColorID, RM.ColorID ,RM.GroupConceptNo ConceptNo, C.ColorCode, Color.SegmentValue ColorName, DP.DPName,RM.DPProcessInfo,RM.DPID,
	                Replace(ISNULL(CT.ShortName,''),'Select','R&D') Buyer, ISNULL(CCT.TeamName,'R&D') BuyerTeam,FAC.BookingID, ISNULL(FAC.LabDipNo,'') LabDipNo 
	                FROM {TableNames.RND_RECIPE_REQ_MASTER} RM
	                INNER JOIN {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} C ON C.CCColorID = RM.CCColorID
	                LEFT JOIN {TableNames.DyeingProcessPart_HK} DP ON DP.DPID=RM.DPID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = RM.ColorID
	                LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = C.ConceptID
	                LEFT JOIN {DbNames.EPYSL}..Contacts CT ON CT.ContactID = FCM.BuyerID
	                LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = FCM.BuyerTeamID
	                LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FAC On FAC.BookingChildID = FCM.BookingChildID AND FAC.ConsumptionID = FCM.ConsumptionID AND FAC.BookingID = FCM.BookingID
	                WHERE RM.RecipeReqMasterID={id}
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
                FROM {TableNames.RND_RECIPE_DEFINITION_DYEING_INFO} DI
                INNER JOIN {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID = DI.FiberPartID
                INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = DI.ColorID
                WHERE DI.RecipeReqMasterID = {id};

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
                WHERE RC.RecipeReqMasterID={id};

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
                RecipeDefinitionMaster data = records.Read<RecipeDefinitionMaster>().FirstOrDefault();
                data.Childs = records.Read<RecipeDefinitionChild>().ToList();
                if (data.Childs.IsNull()) data.Childs = new List<RecipeDefinitionChild>();
                data.RecipeDefinitionItemInfos = records.Read<RecipeDefinitionItemInfo>().ToList();
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

        public async Task<RecipeDefinitionMaster> GetAsync(int id)
        {
            var query =
                $@"
                WITH MainObj AS
                (
	                SELECT RD.RecipeID, RD.RecipeDate,RD.RecipeNo, RR.GroupConceptNo ConceptNo, RD.RecipeReqMasterID, RR.RecipeReqNo, RD.RecipeFor,RD.BatchWeightKG,RD.Remarks, C.CCColorID, C.ColorID, C.ColorCode,
	                Color.SegmentValue ColorName,RD.IsApproved,RD.Acknowledged, RD.Temperature, RD.ProcessTime,DP.DPName,RD.DPProcessInfo,RD.DPID,RD.IsArchive, RR.DBatchID, RR.IsBDS,
	                Replace(ISNULL(CT.ShortName,''),'Select','R&D') Buyer, ISNULL(CCT.TeamName,'R&D') BuyerTeam, FAC.BookingID, ISNULL(FAC.LabDipNo,'') LabDipNo 
	                FROM {TableNames.RND_RECIPE_DEFINITION_MASTER} RD
	                INNER JOIN {TableNames.RND_RECIPE_REQ_MASTER} RR ON RR.RecipeReqMasterID = RD.RecipeReqMasterID
	                LEFT JOIN {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} C ON C.CCColorID = RD.CCColorID
	                LEFT JOIN {TableNames.DyeingProcessPart_HK} DP ON DP.DPID=RD.DPID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = C.ColorID
	                LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = C.ConceptID
	                LEFT JOIN {DbNames.EPYSL}..Contacts CT ON CT.ContactID = FCM.BuyerID
	                LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = FCM.BuyerTeamID
	                LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FAC On FAC.BookingChildID = FCM.BookingChildID AND FAC.ConsumptionID = FCM.ConsumptionID AND FAC.BookingID = FCM.BookingID
	                WHERE RD.RecipeID={id}
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
                ; SELECT DI.RecipeDInfoID, C.Temperature, C.ProcessTime, EV.ValueName FiberPart, ISV.SegmentValue ColorName
				FROM {TableNames.RND_RECIPE_DEFINITION_CHILD} C
				INNER JOIN {TableNames.RND_RECIPE_DEFINITION_DYEING_INFO} DI ON DI.RecipeDInfoID = C.RecipeDInfoID
				INNER JOIN {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID = DI.FiberPartID
				INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = DI.ColorID
				WHERE C.RecipeID = {id}
				GROUP BY DI.RecipeDInfoID, C.Temperature, C.ProcessTime, EV.ValueName, ISV.SegmentValue;

                --Def Childs
				SELECT C.RecipeChildID, C.RecipeID,C.ProcessID,C.Qty,C.UnitID,C.TempIn,C.TempOut,C.ParticularsID,C.RawItemID,I.ItemName,C.ProcessTime,
				P.ValueName ProcessName,PL.ValueName ParticularsName,R.ItemName RawItemName, UU.DisplayUnitDesc Unit, C.IsPercentage,
				C.RecipeDInfoID, C.Temperature, EV.ValueName FiberPart, ISV.SegmentValue ColorName
				FROM {TableNames.RND_RECIPE_DEFINITION_CHILD} C
				LEFT JOIN {DbNames.EPYSL}..ItemMaster I ON I.ItemMasterID=C.RawItemID
				LEFT JOIN {DbNames.EPYSL}..EntityTypeValue P ON P.ValueID = C.ProcessID
				LEFT JOIN {DbNames.EPYSL}..EntityTypeValue PL ON PL.ValueID = C.ParticularsID
				LEFT JOIN {DbNames.EPYSL}..ItemMaster R ON R.ItemMasterID = C.RawItemID
				LEFT JOIN {DbNames.EPYSL}..Unit UU ON UU.UnitID = C.UnitID
				INNER JOIN {TableNames.RND_RECIPE_DEFINITION_DYEING_INFO} DI ON DI.RecipeDInfoID = C.RecipeDInfoID
				INNER JOIN {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID = DI.FiberPartID
				INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = DI.ColorID
				WHERE C.RecipeID = {id};

                /*
                --Def Childs
                SELECT C.RecipeChildID, C.RecipeID,C.ProcessID,C.Qty,C.UnitID,C.TempIn,C.TempOut,C.ParticularsID,C.RawItemID,
                I.ItemName,
                RawItemName = Case When SG.SubGroupName = 'Dyes' Then ISV2.SegmentValue Else I.ItemName End,
                C.ProcessTime,
                P.ValueName ProcessName,PL.ValueName ParticularsName,
                UU.DisplayUnitDesc Unit, C.IsPercentage,
                C.RecipeDInfoID, C.Temperature, EV.ValueName FiberPart, ISV.SegmentValue ColorName
                FROM {TableNames.RND_RECIPE_DEFINITION_CHILD} C
                LEFT JOIN {DbNames.EPYSL}..ItemMaster I ON I.ItemMasterID = C.RawItemID
                Left Join {DbNames.EPYSL}..ItemSegmentValue ISV2 On ISV2.SegmentValueID = I.Segment2ValueID
                LEFT Join {DbNames.EPYSL}..ItemSubGroup SG On SG.SubGroupID = I.SubGroupID
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue P ON P.ValueID = C.ProcessID
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue PL ON PL.ValueID = C.ParticularsID
                LEFT JOIN {DbNames.EPYSL}..Unit UU ON UU.UnitID = C.UnitID
                INNER JOIN {TableNames.RND_RECIPE_DEFINITION_DYEING_INFO} DI ON DI.RecipeDInfoID = C.RecipeDInfoID
                INNER JOIN {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID = DI.FiberPartID
                INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = DI.ColorID
				WHERE C.RecipeID = {id};
                */

                ----Item Info Child
                SELECT II.RecipeItemInfoID, II.RecipeID, II.RecipeReqChildID, II.BookingID, II.ItemMasterID, II.ConceptID, II.SubGroupID, II.Pcs, II.Qty,
                KnittingType.TypeName KnittingType,Composition.SegmentValue FabricComposition,Construction.SegmentValue FabricConstruction,M.ConstructionID,Gsm.SegmentValue FabricGsm,
                RRC.RecipeOn,
                M.SubGroupID SubGroupID, SG.SubGroupName SubGroup, M.TechnicalNameId, TT.TechnicalName
                FROM {TableNames.RND_RECIPE_DEFINITION_ITEM_INFO} II
                LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} M ON M.ConceptID = II.ConceptID
		        LEFT JOIN {TableNames.RND_RECIPE_REQ_CHILD} RRC ON RRC.RecipeReqChildID = II.RecipeReqChildID
                LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID=M.KnittingTypeID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID=M.CompositionID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID=M.ConstructionID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID=M.GSMID
                LEFT JOIN {DbNames.EPYSL}..ItemSubGroup SG ON SG.SubGroupID = M.SubGroupID
                LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME} TT ON TT.TechnicalNameId = M.TechnicalNameId
                WHERE II.RecipeID = {id};

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
                RecipeDefinitionMaster data = records.Read<RecipeDefinitionMaster>().FirstOrDefault();
                Guard.Against.NullObject(data.RecipeID);
                data.Childs = records.Read<RecipeDefinitionChild>().ToList();
                if (data.Childs.IsNull()) data.Childs = new List<RecipeDefinitionChild>();
                data.DefChilds = records.Read<RecipeDefinitionChild>().ToList();
                data.RecipeDefinitionItemInfos = records.Read<RecipeDefinitionItemInfo>().ToList();
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
        public async Task<List<RecipeDefinitionMaster>> GetByRecipeReqNo(string recipeReqNo)
        {
            string query = $@"SELECT RDM.* 
                            FROM {TableNames.RND_RECIPE_DEFINITION_MASTER} RDM
                            LEFT JOIN {TableNames.RND_RECIPE_REQ_MASTER} RRM ON RRM.RecipeReqMasterID = RDM.RecipeReqMasterID
                            WHERE RRM.RecipeReqNo = {recipeReqNo}";
            var list = await _service.GetDataAsync<RecipeDefinitionMaster>(query);
            if (list == null) return new List<RecipeDefinitionMaster>();
            return list;
        }

        public async Task SaveAsync(RecipeDefinitionMaster entity)
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
                        entity = await UpdateAsync(entity);
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

        private async Task<RecipeDefinitionMaster> AddAsync(RecipeDefinitionMaster entity)
        {
            entity.RecipeID = await _service.GetMaxIdAsync(TableNames.RND_RECIPE_DEFINITION_MASTER, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
            entity.RecipeNo = await _service.GetMaxNoAsync(TableNames.RND_RECIPE_DEFINITION_NO, 1, RepeatAfterEnum.NoRepeat, "00000", transactionGmt, _connectionGmt);
            var maxYRChildId = await _service.GetMaxIdAsync(TableNames.RND_RECIPE_DEFINITION_CHILD, entity.Childs.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
            var maxInfoChildId = await _service.GetMaxIdAsync(TableNames.RND_RECIPE_DEFINITION_ITEM_INFO, entity.RecipeDefinitionItemInfos.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

            foreach (var item in entity.Childs)
            {
                item.RecipeChildID = maxYRChildId++;
                item.RecipeId = entity.RecipeID;
                item.EntityState = EntityState.Added;
            }
            foreach (var item in entity.RecipeDefinitionItemInfos)
            {
                item.RecipeItemInfoID = maxInfoChildId++;
                item.RecipeID = entity.RecipeID;
                item.EntityState = EntityState.Added;
            }

            return entity;
        }

        private async Task<RecipeDefinitionMaster> UpdateAsync(RecipeDefinitionMaster entity)
        {
            var maxYRChildId = await _service.GetMaxIdAsync(TableNames.RND_RECIPE_DEFINITION_CHILD, entity.Childs.Where(x => x.EntityState == EntityState.Added).Count(), RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
            var maxInfoChildId = await _service.GetMaxIdAsync(TableNames.RND_RECIPE_DEFINITION_ITEM_INFO, entity.RecipeDefinitionItemInfos.Where(x => x.EntityState == EntityState.Added).Count(), RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

            foreach (var item in entity.Childs.ToList())
            {
                switch (item.EntityState)
                {
                    case EntityState.Added:
                        item.RecipeChildID = maxYRChildId++;
                        item.RecipeId = entity.RecipeID;
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
                        item.RecipeItemInfoID = maxInfoChildId++;
                        item.RecipeID = entity.RecipeID;
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

        public async Task<RecipeDefinitionMaster> GetAllByIDAsync(int id)
        {
            string sql = $@"
            ;Select RD.*, RR.DBatchID, RR.IsBDS, RR.GroupConceptNo
			From {TableNames.RND_RECIPE_DEFINITION_MASTER} RD
			INNER JOIN {TableNames.RND_RECIPE_REQ_MASTER} RR ON RR.RecipeReqMasterID = RD.RecipeReqMasterID
			Where RecipeID = {id}

            ;Select * From {TableNames.RND_RECIPE_DEFINITION_CHILD} Where RecipeID = {id}

            ;Select * From {TableNames.RND_RECIPE_DEFINITION_ITEM_INFO} Where RecipeID = {id}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                RecipeDefinitionMaster data = records.Read<RecipeDefinitionMaster>().FirstOrDefault();
                data.Childs = records.Read<RecipeDefinitionChild>().ToList();
                if (data.Childs.IsNull()) data.Childs = new List<RecipeDefinitionChild>();
                data.RecipeDefinitionItemInfos = records.Read<RecipeDefinitionItemInfo>().ToList();
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

        public async Task<List<BatchMaster>> GetBatchDetails(string batchIds)
        {
            var sql = $@"
                    ;SELECT	* FROM {TableNames.BATCH_MASTER} BM WHERE BM.BatchID IN ({batchIds});

                    ;SELECT * FROM {TableNames.BATCH_ITEM_REQUIREMENT} BM WHERE BM.BatchID IN ({batchIds});

                    ;SELECT * FROM {TableNames.BATCH_CHILD} BM WHERE BM.BatchID IN ({batchIds});

                    ;SELECT * FROM {TableNames.BATCH_WISE_RECIPE_CHILD} BM WHERE BM.BatchID IN ({batchIds});
                    ";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                List<BatchMaster> datas = records.Read<BatchMaster>().ToList();
                List<BatchItemRequirement> batchItemRequirements = records.Read<BatchItemRequirement>().ToList();
                List<BatchChild> batchChilds = records.Read<BatchChild>().ToList();
                List<BatchWiseRecipeChild> batchWiseRecipeChilds = records.Read<BatchWiseRecipeChild>().ToList();

                foreach (BatchMaster batch in datas)
                {
                    batch.BatchItemRequirements = batchItemRequirements.Where(x => x.BatchID == batch.BatchID).ToList();
                    foreach (BatchItemRequirement item in batch.BatchItemRequirements)
                    {
                        item.BatchChilds = batchChilds.Where(x => x.BItemReqID == item.BItemReqID).ToList();
                    }
                    batch.BatchWiseRecipeChilds = batchWiseRecipeChilds.Where(x => x.BatchID == batch.BatchID).ToList();
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
        public async Task UpdateRecipeId(RecipeDefinitionMaster entity, int DBatchID)
        {
            #region Update RecipeID (Tables : BatchMaster, DyeingBatchMaster, DyeingBatchItem)
            //if (!entity.GroupConceptNo.IsNullOrEmpty() && entity.ColorID > 0 && entity.RecipeReqMasterID > 0 && entity.RecipeID > 0)
            //{
            //    string query = $"UPDATE BatchMaster SET RecipeID={entity.RecipeID} WHERE RecipeID = 0 AND GroupConceptNo = '{entity.GroupConceptNo}' AND ColorID = {entity.ColorID} AND CCColorID = {entity.CCColorID};";
            //    await _batchService.ExecuteAsync(query, AppConstants.TEXTILE_CONNECTION);

            //    query = $@"UPDATE A
            //     SET A.RecipeID = {entity.RecipeID}
            //     FROM DyeingBatchMaster A
            //     LEFT JOIN DyeingBatchWithBatchMaster B ON B.DBatchID=A.DBatchID
            //                    LEFT JOIN BatchMaster BM ON BM.BatchID=B.BatchID AND BM.ColorID=A.ColorID
            //                    WHERE A.RecipeID = 0 AND BM.GroupConceptNo = '{entity.GroupConceptNo}' AND A.ColorID = {entity.ColorID} AND A.CCColorID = {entity.CCColorID};";
            //    await _dyeingBatchService.ExecuteAsync(query, AppConstants.TEXTILE_CONNECTION);

            //    query = $@"Update b Set RecipeID = c.RecipeID
            //                From DyeingBatchMaster a
            //                Inner Join DyeingBatchItem b On b.DBatchID = a.DBatchID
            //                Inner Join RecipeDefinitionMaster c ON c.CCColorID = a.CCColorID and c.ColorID = a.ColorID
            //                Inner Join RecipeRequestMaster d ON d.CCColorID = a.CCColorID and d.ColorID = a.ColorID
            //                Where b.RecipeID = 0 AND d.GroupConceptNo = '{entity.GroupConceptNo}' AND a.ColorID = {entity.ColorID} AND A.CCColorID={entity.CCColorID};";

            //    await _dyeingBatchItemService.ExecuteAsync(query, AppConstants.TEXTILE_CONNECTION);
            //}
            if (!entity.GroupConceptNo.IsNullOrEmpty() && entity.ColorID > 0 && entity.RecipeReqMasterID > 0 && entity.RecipeID > 0)
            {
                if (DBatchID == 0)
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

                    query = $@"Update b Set RecipeID = c.RecipeID
                            From {TableNames.DYEING_BATCH_MASTER} a
                            Inner Join {TableNames.DYEING_BATCH_ITEM} b On b.DBatchID = a.DBatchID
                            Inner Join {TableNames.RND_RECIPE_DEFINITION_MASTER} c ON c.CCColorID = a.CCColorID and c.ColorID = a.ColorID
                            Inner Join {TableNames.RND_RECIPE_REQ_MASTER} d ON d.CCColorID = a.CCColorID and d.ColorID = a.ColorID
                            Where b.RecipeID = 0 AND d.GroupConceptNo = '{entity.GroupConceptNo}' AND a.ColorID = {entity.ColorID} AND A.CCColorID={entity.CCColorID};";

                    await _dyeingBatchItemService.ExecuteAsync(query, AppConstants.TEXTILE_CONNECTION);
                }
                else
                {
                    string query = $@"declare @BatchID int;
                                      set @BatchID = (select distinct DBI.BatchID from {TableNames.DYEING_BATCH_ITEM} DBI where DBatchID = {DBatchID})
                                      UPDATE {TableNames.BATCH_MASTER} SET RecipeID={entity.RecipeID} WHERE RecipeID = 0 AND BatchID=@BatchID;";
                    await _batchService.ExecuteAsync(query, AppConstants.TEXTILE_CONNECTION);

                    //query = $@"Update DyeingBatchMaster Set RecipeID= {entity.RecipeID} where RecipeID = 0 AND  DBatchID={DBatchID}";
                    query = $@"UPDATE A
					            SET A.RecipeID = {entity.RecipeID}
					            FROM {TableNames.DYEING_BATCH_MASTER} A
					            LEFT JOIN {TableNames.DYEING_BATCH_WITH_BATCH_MASTER} B ON B.DBatchID=A.DBatchID
                                LEFT JOIN {TableNames.BATCH_MASTER} BM ON BM.BatchID=B.BatchID AND BM.ColorID=A.ColorID
                                WHERE A.RecipeID = 0 AND  A.DBatchID={DBatchID};";
                    await _dyeingBatchService.ExecuteAsync(query, AppConstants.TEXTILE_CONNECTION);

                    //query = $@"update DyeingBatchItem set RecipeID= {entity.RecipeID} where RecipeID = 0 AND  DBatchID={DBatchID}";
                    query = $@"Update b Set RecipeID = c.RecipeID
                            From {TableNames.DYEING_BATCH_MASTER} a
                            Inner Join {TableNames.DYEING_BATCH_ITEM} b On b.DBatchID = a.DBatchID
                            Inner Join {TableNames.RND_RECIPE_DEFINITION_MASTER} c ON c.CCColorID = a.CCColorID and c.ColorID = a.ColorID
                            Inner Join {TableNames.RND_RECIPE_REQ_MASTER} d ON d.CCColorID = a.CCColorID and d.ColorID = a.ColorID
                            Where b.RecipeID = 0 AND  b.DBatchID={DBatchID};";

                    await _dyeingBatchItemService.ExecuteAsync(query, AppConstants.TEXTILE_CONNECTION);
                }
            }
            #endregion
        }
        public async Task UpdateEntityAsync(RecipeDefinitionMaster entity)
        {
           
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                await _connectionGmt.OpenAsync();
                transactionGmt = _connectionGmt.BeginTransaction();

                await _service.SaveSingleAsync(entity, transaction);

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

        public async Task UpdateRecipeWithBatchAsync(RecipeDefinitionMaster entity, DyeingBatchMaster dyeingBatchEntity, List<BatchMaster> batchEntities)
        {
            int maxDyeingBatchRecipeID = await _service.GetMaxIdAsync(TableNames.DYEING_BATCH_RECIPE, entity.Childs.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
            int maxBatchWiseRecipeChildID = await _service.GetMaxIdAsync(TableNames.BATCH_WISE_RECIPE_CHILD, entity.Childs.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

           
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                //await _connectionGmt.OpenAsync();
                //transactionGmt = _connectionGmt.BeginTransaction();

                await _service.SaveSingleAsync(entity, transaction);

                ////DyeingBatchMaster
                dyeingBatchEntity.RecipeID = entity.RecipeID;
                dyeingBatchEntity.EntityState = EntityState.Modified;
                await _service.SaveSingleAsync(dyeingBatchEntity, transaction);
                ////DyeingBatchItem
                dyeingBatchEntity.DyeingBatchItems.ForEach(dbItem =>
                {
                    dbItem.RecipeID = entity.RecipeID;
                    dbItem.EntityState = EntityState.Modified;
                });
                await _service.SaveAsync(dyeingBatchEntity.DyeingBatchItems, transaction);
                ////DyeingBatchRecipe
                entity.Childs.ForEach(item =>
                {
                    DyeingBatchRecipe oBatchRecipe = new DyeingBatchRecipe();
                    oBatchRecipe.DBRID = maxDyeingBatchRecipeID++;
                    oBatchRecipe.DBatchID = entity.DBatchID;
                    oBatchRecipe.RecipeID = item.RecipeId;
                    oBatchRecipe.RecipeChildID = item.RecipeChildID;
                    oBatchRecipe.ProcessID = item.ProcessId;
                    oBatchRecipe.ParticularsID = item.ParticularsId;
                    oBatchRecipe.RawItemID = item.RawItemId;
                    oBatchRecipe.Qty = item.Qty;
                    oBatchRecipe.UnitID = item.UnitID;
                    oBatchRecipe.TempIn = item.TempIn;
                    oBatchRecipe.TempOut = item.TempOut;
                    oBatchRecipe.ProcessTime = item.ProcessTime;
                    oBatchRecipe.EntityState = EntityState.Added;
                    dyeingBatchEntity.DyeingBatchRecipes.Add(oBatchRecipe);
                });
                await _service.SaveAsync(dyeingBatchEntity.DyeingBatchRecipes, transaction);

                ////Batch Master
                batchEntities.ForEach(batch =>
                {
                    batch.RecipeID = entity.RecipeID;
                    batch.EntityState = EntityState.Modified;

                    //BatchItemRequirement
                    entity.RecipeDefinitionItemInfos.ForEach(rdItemInfo =>
                    {
                        List<BatchItemRequirement> bItemReqs = batch.BatchItemRequirements.Where(x => x.BatchID == batch.BatchID && x.ItemMasterID == rdItemInfo.ItemMasterID).ToList();
                        bItemReqs.ForEach(x =>
                        {
                            x.RecipeItemInfoID = rdItemInfo.RecipeItemInfoID;
                            x.EntityState = EntityState.Modified;
                        });
                    });

                    //BatchWiseRecipeChild
                    entity.Childs.ForEach(item =>
                    {
                        BatchWiseRecipeChild oBatchWiseRecipeChild = new BatchWiseRecipeChild();
                        oBatchWiseRecipeChild.BRecipeChildID = maxBatchWiseRecipeChildID++;
                        oBatchWiseRecipeChild.BatchID = batch.BatchID;
                        oBatchWiseRecipeChild.RecipeChildID = item.RecipeChildID;
                        oBatchWiseRecipeChild.ProcessID = item.ProcessId;
                        oBatchWiseRecipeChild.ParticularsID = item.ParticularsId;
                        oBatchWiseRecipeChild.RawItemID = item.RawItemId;
                        oBatchWiseRecipeChild.Qty = item.Qty;
                        oBatchWiseRecipeChild.UnitID = item.UnitID;
                        oBatchWiseRecipeChild.TempIn = item.TempIn;
                        oBatchWiseRecipeChild.TempOut = item.TempOut;
                        oBatchWiseRecipeChild.ProcessTime = item.ProcessTime;
                        oBatchWiseRecipeChild.EntityState = EntityState.Added;
                        batch.BatchWiseRecipeChilds.Add(oBatchWiseRecipeChild);
                    });

                });

                await _service.SaveAsync(batchEntities, transaction);
                List<BatchItemRequirement> batchItemRequirements = new List<BatchItemRequirement>();
                List<BatchWiseRecipeChild> batchWiseRecipeChilds = new List<BatchWiseRecipeChild>();
                batchEntities.ForEach(batch =>
                {
                    batchItemRequirements.AddRange(batch.BatchItemRequirements);
                    batchWiseRecipeChilds.AddRange(batch.BatchWiseRecipeChilds);
                });
                await _service.SaveAsync(batchItemRequirements, transaction);
                await _service.SaveAsync(batchWiseRecipeChilds, transaction);

                //UpdateRecipeId(entity);

                transaction.Commit();
                //transactionGmt.Commit();
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                //if (transactionGmt != null) transactionGmt.Rollback();
                throw ex;
            }
            finally
            {
                _connection.Close();
                //_connectionGmt.Close();
            }
        }

        public async Task<List<RecipeDefinitionMaster>> GetAllApproveListForCopy(string dpID, string buyer, string fabricComposition, string color, PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By RecipeID Desc" : paginationInfo.OrderBy;
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
                 Select RecipeID, RecipeNo, RecipeReqMasterID, RecipeDate, BatchWeightKG, Remarks, Temperature, ConceptID, ProcessTime, DPProcessInfo, DPID, ColorID, CCColorID, RecipeFor 
                 From {TableNames.RND_RECIPE_DEFINITION_MASTER}
                 Where IsApproved=1 And BuyerID = {buyer} And ColorID = {color}
                ), RR AS (
                 select M.RecipeID, M.RecipeNo, M.RecipeReqMasterID, M.RecipeDate,RF.ValueName RecipeForName,M.BatchWeightKG,M.Remarks,C.SegmentValue ColorName,
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
                 Group By M.RecipeID, M.RecipeNo, M.RecipeReqMasterID, M.RecipeDate,RF.ValueName,M.BatchWeightKG,M.Remarks,C.SegmentValue,
                 M.ConceptID, FCM.GroupConceptNo, M.Temperature, M.ProcessTime,DP.DPName,M.DPProcessInfo,
                 Replace(ISNULL(CT.ShortName,''),'Select','R&D'), ISNULL(CCT.TeamName,'R&D'), ISNULL(FAC.LabDipNo,'')
                )
                SELECT *, COUNT(*) OVER() TotalRows FROM RR
                ";
            //       string sql =
            //       $@";With M As(
            //            Select RDM.RecipeID, RDM.RecipeNo, RDM.RecipeReqMasterID, RDM.RecipeDate, RDM.BatchWeightKG, RDM.Remarks, RDM.Temperature, RDM.ConceptID, RDM.ProcessTime, RDM.DPProcessInfo, RDM.DPID, RDM.ColorID, RDM.CCColorID, RDM.RecipeFor 
            //            From RecipeDefinitionMaster RDM
            //inner join RecipeRequestMaster RRM on RRM.RecipeReqMasterID=RDM.RecipeReqMasterID
            //inner join FreeConceptMaster FCM on FCM.ConceptNo=RRM.GroupConceptNo
            //            Where IsApproved=1 And FCM.GroupConceptNo = {buyer} And RDM.ColorID = {color}
            //           ), RR AS (
            //            select M.RecipeID, M.RecipeNo, M.RecipeReqMasterID, M.RecipeDate,RF.ValueName RecipeForName,M.BatchWeightKG,M.Remarks,C.SegmentValue ColorName,
            //            M.ConceptID, FCM.GroupConceptNo ConceptNo, M.Temperature, M.ProcessTime,DP.DPName,M.DPProcessInfo,
            //            Replace(ISNULL(CT.ShortName,''),'Select','R&D') Buyer, ISNULL(CCT.TeamName,'R&D') BuyerTeam, ISNULL(FAC.LabDipNo,'') LabDipNo 
            //               ,KMT.TypeName KnittingType, Gsm.SegmentValue Gsm,TT.TechnicalName, Composition.SegmentValue Composition	                
            //               from M
            //            LEFT JOIN DyeingProcessPart_HK DP ON DP.DPID=M.DPID
            //            LEFT JOIN {DbNames.EPYSL}..EntityTypeValue RF ON RF.ValueID = M.RecipeFor
            //            left JOIN {DbNames.EPYSL}..ItemSegmentValue C ON C.SegmentValueID = M.ColorID
            //            INNER JOIN FreeConceptChildColor CCC ON CCC.CCColorID = M.CCColorID
            //            LEFT JOIN FreeConceptMaster FCM ON FCM.ConceptID = CCC.ConceptID
            //LEFT JOIN KnittingMachineType KMT ON FCM.KnittingTypeID=KMT.TypeID
            //LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID=FCM.GSMID
            //LEFT JOIN FabricTechnicalName TT ON TT.TechnicalNameId = FCM.TechnicalNameId
            //LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID=FCM.CompositionID
            //            LEFT JOIN {DbNames.EPYSL}..Contacts CT ON CT.ContactID = FCM.BuyerID
            //            LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = FCM.BuyerTeamID
            //            LEFT JOIN FBookingAcknowledgeChild FAC On FAC.BookingChildID = FCM.BookingChildID AND FAC.ConsumptionID = FCM.ConsumptionID AND FAC.BookingID = FCM.BookingID
            //            left JOIN {DbNames.EPYSL}..ItemSegmentValue com ON com.SegmentValueID = FCM.CompositionID
            //            WHERE com.SegmentValue in ({fComposition})
            //            Group By M.RecipeID, M.RecipeNo, M.RecipeReqMasterID, M.RecipeDate,RF.ValueName,M.BatchWeightKG,M.Remarks,C.SegmentValue,
            //            M.ConceptID, FCM.GroupConceptNo, M.Temperature, M.ProcessTime,DP.DPName,M.DPProcessInfo,
            //            Replace(ISNULL(CT.ShortName,''),'Select','R&D'), ISNULL(CCT.TeamName,'R&D'), ISNULL(FAC.LabDipNo,'')
            //               ,KMT.TypeName, Gsm.SegmentValue,TT.TechnicalName, Composition.SegmentValue
            //           )
            //           SELECT *, COUNT(*) OVER() TotalRows FROM RR
            //           ";
            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<RecipeDefinitionMaster>(sql);
        }
        public async Task<List<RecipeDefinitionMaster>> GetConceptWiseRecipeForCopy(string fabricComposition, string color, string GroupConceptNo, PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By RecipeID Desc" : paginationInfo.OrderBy;
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

            //string sql =
            //$@";With M As(
            //     Select RecipeID, RecipeNo, RecipeReqMasterID, RecipeDate, BatchWeightKG, Remarks, Temperature, ConceptID, ProcessTime, DPProcessInfo, DPID, ColorID, CCColorID, RecipeFor 
            //     From RecipeDefinitionMaster
            //     Where IsApproved=1 And BuyerID = {buyer} And ColorID = {color}
            //    ), RR AS (
            //     select M.RecipeID, M.RecipeNo, M.RecipeReqMasterID, M.RecipeDate,RF.ValueName RecipeForName,M.BatchWeightKG,M.Remarks,C.SegmentValue ColorName,
            //     M.ConceptID, FCM.GroupConceptNo ConceptNo, M.Temperature, M.ProcessTime,DP.DPName,M.DPProcessInfo,
            //     Replace(ISNULL(CT.ShortName,''),'Select','R&D') Buyer, ISNULL(CCT.TeamName,'R&D') BuyerTeam, ISNULL(FAC.LabDipNo,'') LabDipNo 
            //     from M
            //     LEFT JOIN DyeingProcessPart_HK DP ON DP.DPID=M.DPID
            //     LEFT JOIN {DbNames.EPYSL}..EntityTypeValue RF ON RF.ValueID = M.RecipeFor
            //     left JOIN {DbNames.EPYSL}..ItemSegmentValue C ON C.SegmentValueID = M.ColorID
            //     INNER JOIN FreeConceptChildColor CCC ON CCC.CCColorID = M.CCColorID
            //     LEFT JOIN FreeConceptMaster FCM ON FCM.ConceptID = CCC.ConceptID
            //     LEFT JOIN {DbNames.EPYSL}..Contacts CT ON CT.ContactID = FCM.BuyerID
            //     LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = FCM.BuyerTeamID
            //     LEFT JOIN FBookingAcknowledgeChild FAC On FAC.BookingChildID = FCM.BookingChildID AND FAC.ConsumptionID = FCM.ConsumptionID AND FAC.BookingID = FCM.BookingID
            //     left JOIN {DbNames.EPYSL}..ItemSegmentValue com ON com.SegmentValueID = FCM.CompositionID
            //     WHERE com.SegmentValue in ({fComposition})
            //     Group By M.RecipeID, M.RecipeNo, M.RecipeReqMasterID, M.RecipeDate,RF.ValueName,M.BatchWeightKG,M.Remarks,C.SegmentValue,
            //     M.ConceptID, FCM.GroupConceptNo, M.Temperature, M.ProcessTime,DP.DPName,M.DPProcessInfo,
            //     Replace(ISNULL(CT.ShortName,''),'Select','R&D'), ISNULL(CCT.TeamName,'R&D'), ISNULL(FAC.LabDipNo,'')
            //    )
            //    SELECT *, COUNT(*) OVER() TotalRows FROM RR
            //    ";
            string sql =
            $@";With M As(
	                Select RDM.RecipeID, RDM.RecipeNo, RDM.RecipeReqMasterID, RDM.RecipeDate, RDM.BatchWeightKG, RDM.Remarks, RDM.Temperature, RDM.ConceptID, RDM.ProcessTime, RDM.DPProcessInfo, RDM.DPID, RDM.ColorID, RDM.CCColorID, RDM.RecipeFor 
	                From {TableNames.RND_RECIPE_DEFINITION_MASTER} RDM
					inner join {TableNames.RND_RECIPE_REQ_MASTER} RRM on RRM.RecipeReqMasterID=RDM.RecipeReqMasterID
					inner join {TableNames.RND_FREE_CONCEPT_MASTER} FCM on FCM.ConceptNo=RRM.GroupConceptNo
	                Where IsApproved=1 And FCM.GroupConceptNo = '{GroupConceptNo}' And RDM.ColorID = {color}
                ), RR AS (
	                select M.RecipeID, M.RecipeNo, M.RecipeReqMasterID, M.RecipeDate,RF.ValueName RecipeForName,M.BatchWeightKG,M.Remarks,C.SegmentValue ColorName,
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
					LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME} TT ON TT.TechnicalNameId = FCM.TechnicalNameId
					LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID=FCM.CompositionID
	                LEFT JOIN {DbNames.EPYSL}..Contacts CT ON CT.ContactID = FCM.BuyerID
	                LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = FCM.BuyerTeamID
	                LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FAC On FAC.BookingChildID = FCM.BookingChildID AND FAC.ConsumptionID = FCM.ConsumptionID AND FAC.BookingID = FCM.BookingID
	                left JOIN {DbNames.EPYSL}..ItemSegmentValue com ON com.SegmentValueID = FCM.CompositionID
	                WHERE com.SegmentValue in ({fComposition})
	                Group By M.RecipeID, M.RecipeNo, M.RecipeReqMasterID, M.RecipeDate,RF.ValueName,M.BatchWeightKG,M.Remarks,C.SegmentValue,
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

            return await _service.GetDataAsync<RecipeDefinitionMaster>(sql);
        }
        public async Task<RecipeDefinitionMaster> GetRecipeDyeingInfo(int id)
        {
            var query =
                $@"
                    --Childs
                    /*
                    SELECT DI.*, EV.ValueName FiberPart, ISV.SegmentValue ColorName
                    FROM {TableNames.RND_RECIPE_DEFINITION_DYEING_INFO} DI
                    INNER JOIN {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID = DI.FiberPartID
                    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = DI.ColorID
                    WHERE DI.RecipeReqMasterID = {id};
                    */

                    --Childs
                    WITH 
					RD AS (
						SELECT DI.RecipeDInfoID, DI.RecipeReqMasterID, DI.CCColorID, DI.RecipeID, DI.FiberPartID,
						DI.ColorID, DI.RecipeOn,
						EV.ValueName FiberPart, ISV.SegmentValue ColorName
						FROM {TableNames.RND_RECIPE_DEFINITION_DYEING_INFO} DI
						INNER JOIN {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID = DI.FiberPartID
						INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = DI.ColorID
						WHERE DI.RecipeReqMasterID = {id}
					),
					RDC AS 
					(
						SELECT C.RecipeDInfoID, C.Temperature, C.ProcessTime
						FROM {TableNames.RND_RECIPE_DEFINITION_CHILD} C
						INNER JOIN RD ON RD.RecipeDInfoID = C.RecipeDInfoID
						GROUP BY C.RecipeDInfoID, C.Temperature, C.ProcessTime
					),
					FinalList AS
					(
						SELECT RD.*, RDC.Temperature, RDC.ProcessTime
						FROM RD
						LEFT JOIN RDC ON RDC.RecipeDInfoID = RD.RecipeDInfoID
					)
					SELECT * FROM FinalList

                    --Def Childs
                    SELECT C.RecipeChildID, C.RecipeID,C.ProcessID,C.Qty,C.UnitID,C.TempIn,C.TempOut,C.ParticularsID,C.RawItemID,I.ItemName,C.ProcessTime,
                    P.ValueName ProcessName,PL.ValueName ParticularsName,R.ItemName RawItemName, UU.DisplayUnitDesc Unit, C.IsPercentage,
                    C.RecipeDInfoID, C.Temperature, EV.ValueName FiberPart, ISV.SegmentValue ColorName
                    FROM {TableNames.RND_RECIPE_DEFINITION_CHILD} C
                    LEFT JOIN {DbNames.EPYSL}..ItemMaster I ON I.ItemMasterID=C.RawItemID
                    LEFT JOIN {DbNames.EPYSL}..EntityTypeValue P ON P.ValueID = C.ProcessID
                    LEFT JOIN {DbNames.EPYSL}..EntityTypeValue PL ON PL.ValueID = C.ParticularsID
                    LEFT JOIN {DbNames.EPYSL}..ItemMaster R ON R.ItemMasterID = C.RawItemID
                    LEFT JOIN {DbNames.EPYSL}..Unit UU ON UU.UnitID = C.UnitID
                    INNER JOIN {TableNames.RND_RECIPE_DEFINITION_DYEING_INFO} DI ON DI.RecipeDInfoID = C.RecipeDInfoID
                    INNER JOIN {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID = DI.FiberPartID
                    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = DI.ColorID
                    WHERE DI.RecipeReqMasterID = {id};
                   ";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                RecipeDefinitionMaster data = new RecipeDefinitionMaster();
                data.Childs = records.Read<RecipeDefinitionChild>().ToList();
                if (data.Childs.IsNull()) data.Childs = new List<RecipeDefinitionChild>();
                data.DefChilds = records.Read<RecipeDefinitionChild>().ToList();
                data.Childs.ForEach(c =>
                {
                    c.DefChilds = data.DefChilds.Where(d => d.RecipeDInfoID == c.RecipeDInfoID).ToList();
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
    }
}