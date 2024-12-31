using Dapper;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Application.Interfaces.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Entity;

namespace EPYSLTEXCore.Application.Services.Inventory
{
    public class YDRecipeRequestService : IYDRecipeRequestService
    {
        private readonly IDapperCRUDService<YDRecipeMaster> _service;
        private readonly IDapperCRUDService<YDBatchMaster> _batchService;
        private readonly IDapperCRUDService<YDDyeingBatchMaster> _dyeingBatchService;
        private readonly IDapperCRUDService<DyeingBatchItem> _dyeingBatchItemService;//Need to Check
        private readonly SqlConnection _connection;
        private readonly SqlConnection _connectionGmt;

        public YDRecipeRequestService(IDapperCRUDService<YDRecipeMaster> service, IDapperCRUDService<YDDyeingBatchMaster> dyeingBatchService, IDapperCRUDService<DyeingBatchItem> dyeingBatchItemService
            , IDapperCRUDService<YDBatchMaster> batchService)
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

        public async Task<List<YDRecipeRequestMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By YDBookingDate Desc" : paginationInfo.OrderBy;
            string sql;
            if (status == Status.Pending)
            {
                sql = $@"
               ;WITH A AS(
	                SELECT YDM.YDBookingNo, YDM.YDBookingDate, FCC.CCColorID,YDC.ColorID,FCC.ColorName,YDC.ColorCode,FCM.GroupConceptNo,FCM.IsBDS,FCM.BuyerID,YDC.YDBookingChildID,
	                Buyer = CASE WHEN ISNULL(FCM.BuyerID,0) > 0 THEN C.ShortName ELSE '' END, 
	                BuyerTeam = CASE WHEN ISNULL(FCM.BuyerTeamID,0) > 0 THEN CCT.TeamName ELSE '' END
	                FROM {TableNames.YD_BOOKING_MASTER} YDM
	                INNER JOIN {TableNames.YDBookingChild} YDC ON YDC.YDBookingMasterID = YDM.YDBookingMasterID
	                INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_CHILD} MRC ON MRC.FCMRChildID = YDC.FCMRChildID
	                INNER JOIN {TableNames.FreeConceptMRMaster} MRM ON MRM.FCMRMasterID = MRC.FCMRMasterID
	                INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = MRM.ConceptID
	                INNER JOIN {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} FCC ON FCC.ConceptID = FCM.ConceptID
	                LEFT JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = FCM.BuyerID
	                LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = FCM.BuyerTeamID
	                WHERE YDM.IsAcknowledge = 1 AND YDM.IsYDBNoGenerated = 1
	                GROUP BY YDM.YDBookingNo, YDM.YDBookingDate, FCC.CCColorID,YDC.ColorID,FCC.ColorName,FCM.GroupConceptNo,FCM.IsBDS,FCM.BuyerID,C.ShortName,FCM.BuyerTeamID,CCT.TeamName,YDC.ColorCode,YDC.YDBookingChildID
                ),
                G AS (
	                SELECT YDRecipeReqMasterID, GroupConceptNo, CCColorID, ColorID
	                FROM {TableNames.YD_RECIPE_REQ_MASTER}
                )
                SELECT A.*,G.YDRecipeReqMasterID,COUNT(*) OVER() TotalRows FROM A 
                LEFT JOIN G ON A.GroupConceptNo=G.GroupConceptNo AND A.ColorID=G.ColorID AND A.CCColorID=G.CCColorID
                WHERE G.YDRecipeReqMasterID IS NULL";
                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By A.YDBookingNo Desc" : paginationInfo.OrderBy;
            }
            else if (status == Status.Completed)
            {
                sql = $@"
               ;WITH A AS(
	                SELECT 
					YDM.YDBookingNo, YDM.YDBookingDate, FCC.CCColorID,YDC.ColorID,FCC.ColorName,YDC.ColorCode,FCM.GroupConceptNo,FCM.IsBDS,FCM.BuyerID,YDC.YDBookingChildID,
	                Buyer = CASE WHEN ISNULL(FCM.BuyerID,0) > 0 THEN C.ShortName ELSE '' END, 
	                BuyerTeam = CASE WHEN ISNULL(FCM.BuyerTeamID,0) > 0 THEN CCT.TeamName ELSE '' END
	                FROM {TableNames.YD_BOOKING_MASTER} YDM
	                INNER JOIN {TableNames.YDBookingChild} YDC ON YDC.YDBookingMasterID = YDM.YDBookingMasterID
	                INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_CHILD} MRC ON MRC.FCMRChildID = YDC.FCMRChildID
	                INNER JOIN {TableNames.FreeConceptMRMaster} MRM ON MRM.FCMRMasterID = MRC.FCMRMasterID
	                INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = MRM.ConceptID
	                INNER JOIN {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} FCC ON FCC.ConceptID = FCM.ConceptID
	                LEFT JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = FCM.BuyerID
	                LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = FCM.BuyerTeamID
	                WHERE YDM.IsAcknowledge = 1
	                GROUP BY YDM.YDBookingNo, YDM.YDBookingDate, FCC.CCColorID,YDC.ColorID,FCC.ColorName,FCM.GroupConceptNo,FCM.IsBDS,FCM.BuyerID,C.ShortName,FCM.BuyerTeamID,CCT.TeamName,YDC.ColorCode,YDC.YDBookingChildID
                ),
                G AS (
	                SELECT RR.YDRecipeReqMasterID,RR.RecipeReqNo,RR.RecipeReqDate,RecipeStatus = CASE WHEN RDM.IsActive = 0 THEN 'Deactive' ELSE 'Active' END, GroupConceptNo, RR.CCColorID, RR.ColorID
	                FROM {TableNames.YD_RECIPE_REQ_MASTER} RR
					LEFT JOIN {TableNames.YD_RECIPE_DEFINITION_MASTER} RDM ON RDM.YDRecipeReqMasterID = RR.YDRecipeReqMasterID
                    WHERE RR.Acknowledge = 0 AND RR.UnAcknowledge = 0
                )
                SELECT G.YDRecipeReqMasterID,RecipeStatus,RecipeReqNo,RecipeReqDate,A.*,COUNT(*) OVER() TotalRows FROM A 
                LEFT JOIN G ON A.GroupConceptNo=G.GroupConceptNo AND A.ColorID=G.ColorID AND A.CCColorID=G.CCColorID
                WHERE G.YDRecipeReqMasterID IS NOT NULL ";
                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By G.YDRecipeReqMasterID Desc" : paginationInfo.OrderBy;
            }
            else if (status == Status.UnAcknowledge)
            {
                sql = $@"
                ;WITH A AS(
	                SELECT 
					YDM.YDBookingNo, YDM.YDBookingDate, FCC.CCColorID,YDC.ColorID,FCC.ColorName,YDC.ColorCode,FCM.GroupConceptNo,FCM.IsBDS,FCM.BuyerID,YDC.YDBookingChildID,
	                Buyer = CASE WHEN ISNULL(FCM.BuyerID,0) > 0 THEN C.ShortName ELSE '' END, 
	                BuyerTeam = CASE WHEN ISNULL(FCM.BuyerTeamID,0) > 0 THEN CCT.TeamName ELSE '' END
	                FROM {TableNames.YD_BOOKING_MASTER} YDM
	                INNER JOIN {TableNames.YDBookingChild} YDC ON YDC.YDBookingMasterID = YDM.YDBookingMasterID
	                INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_CHILD} MRC ON MRC.FCMRChildID = YDC.FCMRChildID
	                INNER JOIN {TableNames.FreeConceptMRMaster} MRM ON MRM.FCMRMasterID = MRC.FCMRMasterID
	                INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = MRM.ConceptID
	                INNER JOIN {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} FCC ON FCC.ConceptID = FCM.ConceptID
	                LEFT JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = FCM.BuyerID
	                LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = FCM.BuyerTeamID
	                WHERE YDM.IsAcknowledge = 1
	                GROUP BY YDM.YDBookingNo, YDM.YDBookingDate, FCC.CCColorID,YDC.ColorID,FCC.ColorName,FCM.GroupConceptNo,FCM.IsBDS,FCM.BuyerID,C.ShortName,FCM.BuyerTeamID,CCT.TeamName,YDC.ColorCode,YDC.YDBookingChildID
                ),
                G AS (
	                SELECT RR.YDRecipeReqMasterID,RR.RecipeReqNo,RR.RecipeReqDate,RecipeStatus = CASE WHEN RDM.IsActive = 0 THEN 'Deactive' ELSE 'Active' END, GroupConceptNo, RR.CCColorID, RR.ColorID
	                FROM {TableNames.YD_RECIPE_REQ_MASTER} RR
					LEFT JOIN {TableNames.YD_RECIPE_DEFINITION_MASTER} RDM ON RDM.YDRecipeReqMasterID = RR.YDRecipeReqMasterID
                    WHERE RR.UnAcknowledge = 1
                )
                SELECT G.YDRecipeReqMasterID,RecipeStatus,RecipeReqNo,RecipeReqDate,A.*,COUNT(*) OVER() TotalRows FROM A 
                LEFT JOIN G ON A.GroupConceptNo=G.GroupConceptNo AND A.ColorID=G.ColorID AND A.CCColorID=G.CCColorID
                WHERE G.YDRecipeReqMasterID IS NOT NULL ";
                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By G.YDRecipeReqMasterID Desc" : paginationInfo.OrderBy;
            }
            else
            {
                sql = $@"
               ;WITH A AS(
	                SELECT 
					YDM.YDBookingNo, YDM.YDBookingDate, FCC.CCColorID,YDC.ColorID,FCC.ColorName,YDC.ColorCode,FCM.GroupConceptNo,FCM.IsBDS,FCM.BuyerID,YDC.YDBookingChildID,
	                Buyer = CASE WHEN ISNULL(FCM.BuyerID,0) > 0 THEN C.ShortName ELSE '' END, 
	                BuyerTeam = CASE WHEN ISNULL(FCM.BuyerTeamID,0) > 0 THEN CCT.TeamName ELSE '' END
	                FROM {TableNames.YD_BOOKING_MASTER} YDM
	                INNER JOIN {TableNames.YDBookingChild} YDC ON YDC.YDBookingMasterID = YDM.YDBookingMasterID
	                INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_CHILD} MRC ON MRC.FCMRChildID = YDC.FCMRChildID
	                INNER JOIN {TableNames.FreeConceptMRMaster} MRM ON MRM.FCMRMasterID = MRC.FCMRMasterID
	                INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = MRM.ConceptID
	                INNER JOIN {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} FCC ON FCC.ConceptID = FCM.ConceptID
	                LEFT JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = FCM.BuyerID
	                LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = FCM.BuyerTeamID
	                WHERE YDM.IsAcknowledge = 1
	                GROUP BY YDM.YDBookingNo, YDM.YDBookingDate, FCC.CCColorID,YDC.ColorID,FCC.ColorName,FCM.GroupConceptNo,FCM.IsBDS,FCM.BuyerID,C.ShortName,FCM.BuyerTeamID,CCT.TeamName,YDC.ColorCode,YDC.YDBookingChildID
                ),
                G AS (
	                SELECT RR.YDRecipeReqMasterID,RR.RecipeReqNo,RR.RecipeReqDate,RecipeStatus = CASE WHEN RDM.IsActive = 0 THEN 'Deactive' ELSE 'Active' END, GroupConceptNo, RR.CCColorID, RR.ColorID
	                FROM {TableNames.YD_RECIPE_REQ_MASTER} RR
					LEFT JOIN {TableNames.YD_RECIPE_DEFINITION_MASTER} RDM ON RDM.YDRecipeReqMasterID = RR.YDRecipeReqMasterID
                    WHERE RR.Acknowledge = 1
                )
                SELECT G.YDRecipeReqMasterID,RecipeStatus,RecipeReqNo,RecipeReqDate,A.*,COUNT(*) OVER() TotalRows FROM A 
                LEFT JOIN G ON A.GroupConceptNo=G.GroupConceptNo AND A.ColorID=G.ColorID AND A.CCColorID=G.CCColorID
                WHERE G.YDRecipeReqMasterID IS NOT NULL  ";
                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By G.YDRecipeReqMasterID Desc" : paginationInfo.OrderBy;
            }
            string filter = paginationInfo.FilterBy == "" ? paginationInfo.FilterBy : " AND " + paginationInfo.FilterBy.Replace("Where", "");
            sql += $@"
                {filter}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<YDRecipeRequestMaster>(sql);
        }

        public async Task<YDRecipeRequestMaster> GetNewAsync(int ccColorID, int YDBookingChildID, string grpConceptNo, int isBDS, int YDDBatchID = 0)
        {
            string strBDS = isBDS == 1 ? " M.ConceptNo" : " M.GroupConceptNo";

            string dyeingQuery = "";
            var query = "";
            if (YDDBatchID == 0)
            {
                query =
                   $@"
                        SELECT C.CCColorID, YDBC.ColorID ,M.ConceptID,M.GroupConceptNo ConceptNo,M.GroupConceptNo,M.ConceptDate, YDBC.ColorCode,Color.SegmentValue ColorName,
                        C.DPID,C.DPProcessInfo,DP.DPName, C.Remarks, Composition.SegmentValue Composition, M.IsBDS, Replace(ISNULL(CT.ShortName,''),'Select','R&D') Buyer, ISNULL(CCT.TeamName,'R&D') BuyerTeam, ISNULL(FAC.LabDipNo,'') LabDipNo,M.BuyerID
                        FROM {TableNames.YDBookingChild} YDBC 
                        INNER JOIN {TableNames.YD_BOOKING_MASTER} YDB ON YDB.YDBookingMasterID = YDBC.YDBookingMasterID
                        LEFT JOIN {TableNames.RND_FREE_CONCEPT_MR_CHILD} FCMRC1 ON FCMRC1.YBChildItemID = YDBC.YBChildItemID AND YDBC.YBChildItemID <> 0
						LEFT JOIN {TableNames.RND_FREE_CONCEPT_MR_CHILD} FCMRC2 ON FCMRC2.FCMRChildID = YDBC.FCMRChildID
						INNER JOIN {TableNames.FreeConceptMRMaster} FCMR ON FCMR.FCMRMasterID = Case When ISNULL(FCMRC1.FCMRMasterID,0) = 0 Then FCMRC2.FCMRMasterID Else FCMRC1.FCMRMasterID END
                        INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = FCMR.ConceptID
                        LEFT JOIN {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} C ON C.ConceptID = FCM.ConceptID
                        LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} M ON M.ConceptID = C.ConceptID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = C.ColorID
                        LEFT JOIN {TableNames.DyeingProcessPart_HK} DP ON DP.DPID=C.DPID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID=M.CompositionID
                        LEFT JOIN {DbNames.EPYSL}..Contacts CT ON CT.ContactID = M.BuyerID
                        LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = M.BuyerTeamID
                        LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FAC On FAC.BookingChildID = M.BookingChildID AND FAC.ConsumptionID = M.ConsumptionID AND FAC.BookingID = M.BookingID
                        WHERE YDBC.YDBookingChildID = {YDBookingChildID};
                
                        ;With M AS (
	                        SELECT YDBC.FCMRChildID,FCMR.ConceptID, YDBC.ColorID, C.DPID, C.CCColorID, YDBC.ColorCode, C.DPProcessInfo, C.Remarks,YDBC.ItemMasterID
	                        ,FCM.GroupConceptNo, FCM.ConceptDate, FCM.Qty, FCM.SubGroupID,FCM.KnittingTypeID,FCM.CompositionID, FCM.ConstructionID,FCM.GSMID, FCM.TechnicalNameId,FCM.IsBDS
	                        ,FCM.BuyerID,FCM.BuyerTeamID,FCM.ConsumptionID, FCM.BookingID, FCM.BookingChildID
                            FROM {TableNames.YDBookingChild} YDBC 
	                        INNER JOIN {TableNames.YD_BOOKING_MASTER} YDB ON YDB.YDBookingMasterID = YDBC.YDBookingMasterID
	                        LEFT JOIN {TableNames.RND_FREE_CONCEPT_MR_CHILD} FCMRC1 ON FCMRC1.YBChildItemID = YDBC.YBChildItemID AND YDBC.YBChildItemID <> 0
	                        LEFT JOIN {TableNames.RND_FREE_CONCEPT_MR_CHILD} FCMRC2 ON FCMRC2.FCMRChildID = YDBC.FCMRChildID
	                        INNER JOIN {TableNames.FreeConceptMRMaster} FCMR ON FCMR.FCMRMasterID = Case When ISNULL(FCMRC1.FCMRMasterID,0) = 0 Then FCMRC2.FCMRMasterID Else FCMRC1.FCMRMasterID END
	                        INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = FCMR.ConceptID
	                        LEFT JOIN {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} C ON C.ConceptID = FCM.ConceptID
	                        LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} M ON M.ConceptID = C.ConceptID
	                        WHERE YDBC.YDBookingChildID = {YDBookingChildID}
                        ),
                        AllFabrics AS
                        (
	                        SELECT M.CCColorID, M.ColorID,M.ConceptID, M.GroupConceptNo ConceptNo, M.GroupConceptNo, M.ConceptDate, 
	                        M.ColorCode, Color.SegmentValue ColorName,
	                        M.DPID, M.DPProcessInfo, DP.DPName, M.Remarks, Composition.SegmentValue Composition, M.IsBDS, Replace(ISNULL(CT.ShortName,''),'Select','R&D') Buyer, ISNULL(CCT.TeamName,'R&D') BuyerTeam, ISNULL(FAC.LabDipNo,'') LabDipNo,
	                        M.Qty,M.ItemMasterId,M.SubGroupID,M.KnittingTypeID,M.CompositionID,M.ConstructionID,M.GSMID,M.TechnicalNameId
	                        FROM M
	                        LEFT JOIN EPYSL..ItemSegmentValue Color ON Color.SegmentValueID = M.ColorID
	                        LEFT JOIN {TableNames.DyeingProcessPart_HK} DP ON DP.DPID = M.DPID
	                        LEFT JOIN EPYSL..ItemSegmentValue Composition ON Composition.SegmentValueID = M.CompositionID
	                        LEFT JOIN EPYSL..Contacts CT ON CT.ContactID = M.BuyerID
	                        LEFT JOIN EPYSL..ContactCategoryTeam CCT ON CCT.CategoryTeamID = M.BuyerTeamID
                            LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FAC On FAC.BookingChildID = M.BookingChildID AND FAC.ConsumptionID = M.ConsumptionID AND FAC.BookingID = M.BookingID
	                        --WHERE M.SubGroupID = 1
                        )
                        SELECT M.ConceptID, M.ConceptNo, M.ConceptDate, KnittingType.TypeName KnittingType, Composition.SegmentValue Composition, M.ConstructionID,
                        Construction.SegmentValue Construction, Gsm.SegmentValue GSM, M.SubGroupID SubGroupID, SG.SubGroupName SubGroup, M.TechnicalNameId,
                        TT.TechnicalName,M.Qty, M.ItemMasterId
                        --FROM {TableNames.RND_FREE_CONCEPT_MASTER} M
                        FROM AllFabrics M
                        LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID=M.KnittingTypeID
                        LEFT JOIN EPYSL..ItemSegmentValue Composition ON Composition.SegmentValueID=M.CompositionID
                        LEFT JOIN EPYSL..ItemSegmentValue Construction ON Construction.SegmentValueID=M.ConstructionID
                        LEFT JOIN EPYSL..ItemSegmentValue Gsm ON Gsm.SegmentValueID = M.GSMID
                        LEFT JOIN EPYSL..ItemSubGroup SG ON SG.SubGroupID = M.SubGroupID
                        LEFT JOIN {TableNames.FabricTechnicalName} TT ON TT.TechnicalNameId = M.TechnicalNameId
                            WHERE  {strBDS}='{grpConceptNo}';

                        --Dyeing Process
                        ;SELECT CAST(DPID AS VARCHAR) AS id, DPName AS text
                        FROM {TableNames.DyeingProcessPart_HK};

                        --YDRecipeDefinitionDyeingInfo
                        ;SELECT DI.*, EV.ValueName FiberPart, ISV.SegmentValue ColorName
                        FROM {TableNames.YD_RECIPE_DEFINITION_DYEING_INFO} DI
                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID = DI.FiberPartID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = DI.ColorID
                        WHERE CCColorID = {ccColorID};

                        --Recipe Definition Item
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
                            FROM {TableNames.RND_FREE_CONCEPT_MASTER}
                            Where GroupConceptNo = '{grpConceptNo}' And SubGroupID <> 1
                        ), I As(
                            Select 1 As ID, MRC.ItemMasterId, ISV.SegmentValue
                            From GC
                            Inner JOIN {TableNames.FreeConceptMRMaster} MR On MR.ConceptID = GC.ConceptID
                            Inner JOIN {TableNames.RND_FREE_CONCEPT_MR_CHILD} MRC On MRC.FCMRMasterID = MR.FCMRMasterID
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
                query = $@"";
            }
            //if (isBDS == 1)
            //    query += dyeingQuery;

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                YDRecipeRequestMaster data = records.Read<YDRecipeRequestMaster>().FirstOrDefault();
                data.YDRecipeRequestChilds = records.Read<YDRecipeRequestChild>().ToList();

                data.DPList = records.Read<Select2OptionModel>().ToList();
                data.DPList.Insert(0, new Select2OptionModel()
                {
                    id = 0.ToString(),
                    text = "--Select dyeing process--"
                });
                data.YDRecipeDefinitionDyeingInfos = records.Read<YDRecipeDefinitionDyeingInfo>().ToList();
                data.YDRecipeDefinitionItemInfos = records.Read<YDRecipeDefinitionItemInfo>().ToList();
                data.FiberPartList = records.Read<Select2OptionModel>().ToList();
                if (data.YDRecipeRequestChilds.Count == 1) data.YDRecipeRequestChilds.FirstOrDefault().RecipeOn = true;
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
        public async Task<YDRecipeRequestMaster> GetAsync(int id, string groupConceptNo)
        {
            var query =
                $@"
                ;SELECT RM.YDRecipeReqMasterID, RM.RecipeReqNo, RM.RecipeReqDate, RM.CCColorID, RM.ColorID, RM.GroupConceptNo ConceptNo, M.ConceptID, C.ColorCode,RM.YDBookingChildID,
				Color.SegmentValue ColorName, RM.DPID, RM.DPProcessInfo, DP.DPName, RM.Remarks, Composition.SegmentValue Composition, RM.IsBDS, RM.YDDBatchID, Replace(ISNULL(CT.ShortName,''),'Select','R&D') Buyer, ISNULL(CCT.TeamName,'R&D') BuyerTeam, ISNULL(FAC.LabDipNo,'') LabDipNo,RM.UnAcknowledgeReason
                FROM {TableNames.YD_RECIPE_REQ_MASTER} RM
				INNER JOIN {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} C ON C.CCColorID = RM.CCColorID
				LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} M ON M.ConceptID = C.ConceptID
				LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = C.ColorID
				LEFT JOIN {TableNames.DyeingProcessPart_HK} DP ON DP.DPID=RM.DPID
				LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID=M.CompositionID
	            LEFT JOIN {DbNames.EPYSL}..Contacts CT ON CT.ContactID = M.BuyerID
	            LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = M.BuyerTeamID
                LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FAC On FAC.BookingChildID = M.BookingChildID AND FAC.ConsumptionID = M.ConsumptionID AND FAC.BookingID = M.BookingID
				WHERE RM.YDRecipeReqMasterID = {id};

                --Child
                SELECT RC.YDRecipeReqChildID, RC.YDRecipeReqMasterID, RC.ConceptID, RC.BookingID, RC.SubGroupID, RC.ItemMasterID, RC.RecipeOn, M.ConceptNo, M.ConceptDate,
				KnittingType.TypeName KnittingType, Composition.SegmentValue Composition, M.ConstructionID, Construction.SegmentValue Construction,
				Gsm.SegmentValue GSM, SG.SubGroupName SubGroup, M.TechnicalNameId, TT.TechnicalName
				FROM {TableNames.YD_RECIPE_REQ_CHILD} RC
				INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} M ON M.ConceptID = RC.ConceptID
				LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID=M.KnittingTypeID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID=M.CompositionID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID=M.ConstructionID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID=M.GSMID
                LEFT JOIN {DbNames.EPYSL}..ItemSubGroup SG ON SG.SubGroupID = M.SubGroupID
                LEFT JOIN {TableNames.FabricTechnicalName} TT ON TT.TechnicalNameId = M.TechnicalNameId
				WHERE YDRecipeReqMasterID = {id};

                --RecipeDefinitionDyeingInfo
                ;SELECT DI.*, EV.ValueName FiberPart, ISV.SegmentValue ColorName
                FROM {TableNames.YD_RECIPE_DEFINITION_DYEING_INFO} DI
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID = DI.FiberPartID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = DI.ColorID
                WHERE YDRecipeReqMasterID = {id};

                --Dyeing Process
                ;SELECT CAST(DPID AS VARCHAR) AS id, DPName AS text
                FROM {TableNames.DyeingProcessPart_HK};

                --Recipe Definition Item
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
                WHERE  M.GroupConceptNo ='{groupConceptNo}'; 

                --Fiber List
                ;Select CAST(EV.ValueID AS VARCHAR) AS id, EV.ValueName AS text
                From {DbNames.EPYSL}..EntityTypeValue EV
                Inner Join {DbNames.EPYSL}..EntityType ET On EV.EntityTypeID = ET.EntityTypeID
                Where ET.EntityTypeName = '{EntityTypeNameConstants.FABRIC_TYPE}'
                Group By EV.ValueID, EV.ValueName;

                --Fiber List (Collar & Cuff)
                ;With GC AS(
                    Select *
                    FROM {TableNames.RND_FREE_CONCEPT_MASTER}
                    Where GroupConceptNo = '{groupConceptNo}' And SubGroupID <> 1
                ), I As(
                    Select 1 As ID, MRC.ItemMasterId, ISV.SegmentValue
                    From GC
                    Inner JOIN {TableNames.FreeConceptMRMaster} MR On MR.ConceptID = GC.ConceptID
                    Inner JOIN {TableNames.RND_FREE_CONCEPT_MR_CHILD} MRC On MRC.FCMRMasterID = MR.FCMRMasterID
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
                ";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                YDRecipeRequestMaster data = records.Read<YDRecipeRequestMaster>().FirstOrDefault();
                data.YDRecipeRequestChilds = records.Read<YDRecipeRequestChild>().ToList();
                data.YDRecipeDefinitionDyeingInfos = records.Read<YDRecipeDefinitionDyeingInfo>().ToList();
                data.DPList = records.Read<Select2OptionModel>().ToList();
                data.DPList.Insert(0, new Select2OptionModel()
                {
                    id = 0.ToString(),
                    text = "--Select dyeing process--"
                });
                data.YDRecipeDefinitionItemInfos = records.Read<YDRecipeDefinitionItemInfo>().ToList();
                data.FiberPartList = records.Read<Select2OptionModel>().ToList();
                data.FiberPartListCC = records.Read<Select2OptionModel>().ToList();
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
        public async Task<List<YDRecipeRequestChild>> GetItems(string conceptNo, int colorID, int isBDS)
        {
            string sql;
            if (isBDS == 1)
            {
                sql = $@"With M As(
                        Select *
                        From EPYSLTEX..FreeConceptMaster
                        Where GroupConceptNo = '{conceptNo}'
                        )
                        Select M.ConceptID, M.ConceptNo, M.ConceptDate, KnittingType.TypeName KnittingType, Composition.SegmentValue Composition, M.ConstructionID,
                        Construction.SegmentValue Construction, Gsm.SegmentValue GSM, M.SubGroupID SubGroupID, SG.SubGroupName SubGroup, M.TechnicalNameId,
                        TT.TechnicalName,M.Qty, M.ItemMasterId
                        From M
                        Inner Join {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} CMC ON CMC.ConceptID = M.ConceptID
                        LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID=M.KnittingTypeID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID=M.CompositionID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID=M.ConstructionID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID=M.GSMID
                        LEFT JOIN {DbNames.EPYSL}..ItemSubGroup SG ON SG.SubGroupID = M.SubGroupID
                        LEFT JOIN {TableNames.FabricTechnicalName} TT ON TT.TechnicalNameId = M.TechnicalNameId
                        Where CMC.ColorID = {colorID};";
            }
            else
            {
                sql = $@"Select M.ConceptID, M.ConceptNo, M.ConceptDate, KnittingType.TypeName KnittingType, Composition.SegmentValue Composition, M.ConstructionID,
                        Construction.SegmentValue Construction, Gsm.SegmentValue GSM, M.SubGroupID SubGroupID, SG.SubGroupName SubGroup, M.TechnicalNameId,
                        TT.TechnicalName,M.Qty, M.ItemMasterId
                        From {TableNames.RND_FREE_CONCEPT_MASTER} M
                        LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID=M.KnittingTypeID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID=M.CompositionID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID=M.ConstructionID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID=M.GSMID
                        LEFT JOIN {DbNames.EPYSL}..ItemSubGroup SG ON SG.SubGroupID = M.SubGroupID
                        LEFT JOIN {TableNames.FabricTechnicalName} TT ON TT.TechnicalNameId = M.TechnicalNameId
                        Where GroupConceptNo = '{conceptNo}'";
            }
            return await _service.GetDataAsync<YDRecipeRequestChild>(sql);
        }
        public async Task<YDRecipeRequestMaster> GetAllByIDAsync(int ydRecipeRequestMasterID)
        {
            string sql = $@"
            ;Select A.*,B.YDRecipeID 
            FROM {TableNames.YD_RECIPE_REQ_MASTER} A
            LEFT JOIN {TableNames.YD_RECIPE_DEFINITION_MASTER} B ON B.YDRecipeReqMasterID = A.YDRecipeReqMasterID
            Where A.YDRecipeReqMasterID = {ydRecipeRequestMasterID};

            ;Select * FROM {TableNames.YD_RECIPE_REQ_CHILD} Where YDRecipeReqMasterID = {ydRecipeRequestMasterID}

            ;Select * FROM {TableNames.YD_RECIPE_DEFINITION_DYEING_INFO} Where YDRecipeReqMasterID = {ydRecipeRequestMasterID}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YDRecipeRequestMaster data = records.Read<YDRecipeRequestMaster>().FirstOrDefault();
                Guard.Against.NullObject(data);
                data.YDRecipeRequestChilds = records.Read<YDRecipeRequestChild>().ToList();
                data.YDRecipeDefinitionDyeingInfos = records.Read<YDRecipeDefinitionDyeingInfo>().ToList();
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
        public async Task SaveAsync(YDRecipeRequestMaster entity)
        {
            FreeConceptChildColor childColor = await GetFreeConceptChildAsync(entity.CCColorID);
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
                await _service.SaveAsync(entity.YDRecipeRequestChilds, transaction);
                await _service.SaveAsync(entity.YDRecipeDefinitionDyeingInfos, transaction);

                if (entity.IsRework)
                {
                    await _service.SaveAsync(entity.YDRecipeDefinitions, transaction);
                    await _service.SaveSingleAsync(entity.YDDyeingBatchReworkRecipe, transaction);
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
                if (!entity.GroupConceptNo.IsNullOrEmpty() && entity.ColorID > 0 && entity.YDRecipeReqMasterID > 0 && entity.YDRecipeID > 0)
                {
                    string query = $"UPDATE {TableNames.YD_BATCH_MASTER} SET YDRecipeID={entity.YDRecipeID} WHERE YDRecipeID = 0 AND GroupConceptNo = '{entity.GroupConceptNo}' AND ColorID = {entity.ColorID} AND CCColorID = {entity.CCColorID};";
                    await _batchService.ExecuteAsync(query, AppConstants.TEXTILE_CONNECTION);

                    query = $@"UPDATE A
					            SET A.RecipeID = {entity.YDRecipeID}
					            FROM {TableNames.YD_DYEING_BATCH_MASTER} A
					            LEFT JOIN {TableNames.YD_DYEING_BATCH_WITH_BATCH_MASTER} B ON B.YDDBatchID=A.YDDBatchID
                                LEFT JOIN {TableNames.YD_BATCH_MASTER} BM ON BM.YDBatchID=B.YDBatchID AND BM.ColorID=A.ColorID
                                WHERE A.RecipeID = 0 AND BM.GroupConceptNo = '{entity.GroupConceptNo}' AND A.ColorID = {entity.ColorID} AND A.CCColorID = {entity.CCColorID};";
                    await _dyeingBatchService.ExecuteAsync(query, AppConstants.TEXTILE_CONNECTION);


                    query = $@"Update b Set RecipeID = c.RecipeID
                               FROM {TableNames.YD_DYEING_BATCH_MASTER} a
                               Inner JOIN {TableNames.YD_DYEING_BATCH_ITEM} b On b.YDDBatchID = a.YDDBatchID
                               Inner JOIN {TableNames.YD_RECIPE_DEFINITION_MASTER} c ON c.CCColorID = a.CCColorID and c.ColorID = a.ColorID
                               Inner JOIN {TableNames.YD_RECIPE_REQ_MASTER} d ON d.CCColorID = a.CCColorID and d.ColorID = a.ColorID
                               Where b.YDRecipeID = 0 AND d.GroupConceptNo = '{entity.GroupConceptNo}' AND a.ColorID = {entity.ColorID} AND A.CCColorID={entity.CCColorID};";

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
        private async Task<FreeConceptChildColor> GetFreeConceptChildAsync(int id)
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
        private async Task<YDRecipeRequestMaster> AddAsync(YDRecipeRequestMaster entity, SqlTransaction transactionGmt)
        {
            entity.YDRecipeReqMasterID = await _service.GetMaxIdAsync(TableNames.YD_RECIPE_REQ_MASTER, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
            //entity.RecipeReqNo = entity.IsRework == false ? await _service.GetMaxNoAsync(TableNames.YD_RECIPE_REQ_NO) : entity.RecipeReqNo;
            entity.RecipeReqNo = entity.IsRework == false ? await _service.GetMaxNoAsync(TableNames.YD_RECIPE_REQ_NO, 1, RepeatAfterEnum.NoRepeat, "00000", transactionGmt, _connectionGmt) : entity.RecipeReqNo;
            var maxChildId = await _service.GetMaxIdAsync(TableNames.YD_RECIPE_REQ_CHILD, entity.YDRecipeRequestChilds.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
            var maxDyingInfoChildId = await _service.GetMaxIdAsync(TableNames.YD_RECIPE_DEFINITION_DYEING_INFO, entity.YDRecipeDefinitionDyeingInfos.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

            foreach (YDRecipeRequestChild item in entity.YDRecipeRequestChilds)
            {
                item.YDRecipeReqChildID = maxChildId++;
                item.YDRecipeReqMasterID = entity.YDRecipeReqMasterID;
                item.CCColorID = entity.CCColorID;
                item.EntityState = EntityState.Added;
            }
            foreach (YDRecipeDefinitionDyeingInfo item in entity.YDRecipeDefinitionDyeingInfos)
            {
                item.YDRecipeDInfoID = maxDyingInfoChildId++;
                item.YDRecipeReqMasterID = entity.YDRecipeReqMasterID;
                item.EntityState = EntityState.Added;
            }
            if (entity.IsRework)
            {
                entity.YDDyeingBatchReworkRecipe = new YDDyeingBatchReworkRecipe();
                entity.YDDyeingBatchReworkRecipe.YDDBRRID = await _service.GetMaxIdAsync(TableNames.YD_DYEING_BATCH_REWORK_RECIPE, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                entity.YDDyeingBatchReworkRecipe.DBatchID = entity.YDDBatchID;
                entity.YDDyeingBatchReworkRecipe.DBRID = 0;
                entity.YDDyeingBatchReworkRecipe.YDRecipeReqMasterID = entity.YDRecipeReqMasterID;
                entity.YDDyeingBatchReworkRecipe.EntityState = EntityState.Added;
            }
            return entity;
        }

        private async Task<YDRecipeRequestMaster> UpdateAsync(YDRecipeRequestMaster entity, SqlTransaction transactionGmt)
        {
            var maxChildId = await _service.GetMaxIdAsync(TableNames.YD_RECIPE_REQ_CHILD, entity.YDRecipeRequestChilds.Where(x => x.EntityState == EntityState.Added).Count(), RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
            var maxDyingInfoChildId = await _service.GetMaxIdAsync(TableNames.YD_RECIPE_DEFINITION_DYEING_INFO, entity.YDRecipeDefinitionDyeingInfos.Where(x => x.EntityState == EntityState.Added).Count(), RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

            foreach (var item in entity.YDRecipeRequestChilds.Where(x => x.EntityState == EntityState.Added).ToList())
            {
                item.YDRecipeReqChildID = maxChildId++;
                item.YDRecipeReqMasterID = entity.YDRecipeReqMasterID;
                item.CCColorID = entity.CCColorID;
            }
            foreach (var item in entity.YDRecipeDefinitionDyeingInfos.Where(x => x.EntityState == EntityState.Added).ToList())
            {
                item.YDRecipeDInfoID = maxDyingInfoChildId++;
                item.YDRecipeReqMasterID = entity.YDRecipeReqMasterID;
            }
            return entity;
        }
        public async Task UpdateEntityAsync(YDRecipeRequestMaster entity)
        {
            FreeConceptChildColor childColor = await GetFreeConceptChildAsync(entity.CCColorID);
            SqlTransaction transaction = null;
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                await _service.SaveSingleAsync(entity, transaction);
                await _service.SaveAsync(entity.YDRecipeDefinitionDyeingInfos, transaction);

                childColor.RequestAck = true;
                childColor.RequestAckBy = entity.AddedBy;
                childColor.RequestAckDate = DateTime.Now;
                childColor.EntityState = EntityState.Modified;
                await _service.SaveSingleAsync(childColor, transaction);

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
