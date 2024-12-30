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
using EPYSLTEXCore.Infrastructure.Entities.Tex.Knitting;
using EPYSLTEXCore.Infrastructure.Entities.Knitting;
using EPYSLTEXCore.Infrastructure.Entities.Tex.General;
using System.Data.Entity.Validation;
using System.Data.Common;

namespace EPYSLTEXCore.Application.Services.RND
{
    public class KnittingProgramService : IKnittingProgramService
    {
        private readonly IDapperCRUDService<KnittingPlanMaster> _service;
        private readonly IDapperCRUDService<KJobCardMaster> _service1;
        private readonly SqlConnection _connection;
        private readonly SqlConnection _connectionGmt;
        private SqlTransaction transaction;
        private SqlTransaction transactionGmt;

        public KnittingProgramService(IDapperCRUDService<KJobCardMaster> service1, IDapperCRUDService<KnittingPlanMaster> service
           )
        {
            _service = service;
            _service1 = service1;
            
            _service.Connection = service.GetConnection(AppConstants.GMT_CONNECTION);
            _connectionGmt = service.Connection;

            _service.Connection = service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _connection = service.Connection;
        }


        public async Task<List<KnittingPlanMaster>> GetPagedAsync(KnittingProgramType type, Status status, PaginationInfo paginationInfo, LoginUser AppUser, bool isNew = false)
        {
            string tempGuid = CommonFunction.GetNewGuid();
            bool isTempGuidUsed = false;

            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By KPMasterID Desc" : paginationInfo.OrderBy;
            string sql;

            string sqlBuyerPermission = AppUser.IsSuperUser ? "" : $@" BT As (
                        Select CategoryTeamID
                        From {DbNames.EPYSL}..EmployeeAssignContactTeam
                        Where EmployeeCode = {AppUser.EmployeeCode} AND IsActive = 1
                        Group By CategoryTeamID
                    ),
                    B As (
	                    Select C.ContactID
	                    From {DbNames.EPYSL}..ContactAssignTeam C
	                    Inner Join BT On BT.CategoryTeamID = C.CategoryTeamID
	                    Group By C.ContactID
                    ), ";

            string sqlBuyerPerInnerJoin = AppUser.IsSuperUser ? "" : $@" INNER JOIN B ON B.ContactID = M.BuyerID ";

            #region Knitting Program for Concept

            if (status == Status.Revise) //Only for KP Group
            {
                sql = $@"
                ;WITH
                A AS (
                    Select PlanNo = KPG.GroupID, KPG.MachineGauge, KPG.MachineDia, KPG.BrandID, KPG.StartDate, KPG.EndDate, 
                    KPM.BuyerTeamID, FCM.SubGroupID, KPG.KnittingTypeID, KPG.Needle, KPG.CPI, FCM.IsBDS,
                    ISG.SubGroupName, KPM.BuyerID, KPG.GroupConceptNo,
                    ConceptNo = KPG.GroupConceptNo,FCM.ConceptDate, KPG.AddedBy, KPG.DateAdded, KPG.UpdatedBy, KPG.DateUpdated, 
                    Buyer = CASE WHEN KPM.BuyerID > 0 THEN ISNULL(CTO.ShortName,'') ELSE 'R&D' END, 
                    BuyerTeam = CASE WHEN KPM.BuyerTeamID > 0 THEN ISNULL(CCT.TeamName,'') ELSE 'R&D' END,
                    BalanceQty = FCM.TotalQty - SUM(ISNULL(KPC.BookingQty,0)), PlanQty = SUM(ISNULL(KPC.BookingQty,0)), FCM.TotalQty, FCM.Qty, KPM.NeedPreFinishingProcess, KPM.IsSubContact, 
                    FCM.TechnicalNameId, Technical.TechnicalName, KnittingType = KnittingType.TypeName, MCSubClass = KMS.SubClassName
                    FROM {TableNames.Knitting_Plan_Group} KPG
                    Inner Join {TableNames.Knitting_Plan_Master} KPM On KPM.PlanNo = KPG.GroupID
                    INNER JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPMasterID = KPM.KPMasterID
                    Inner Join {TableNames.RND_FREE_CONCEPT_MASTER} FCM On FCM.ConceptID = KPM.ConceptID
                    INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_MASTER} MR ON MR.ConceptID = FCM.ConceptID
                    Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = FCM.SubGroupID
                    LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = KPM.BuyerID
                    LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = KPM.BuyerTeamID
                    LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME} Technical ON Technical.TechnicalNameId=FCM.TechnicalNameId
                    LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID = FCM.KnittingTypeID
                    LEFT JOIN {TableNames.KNITTING_MACHINE_SUBCLASS} KMS ON KMS.SubClassID = FCM.MCSubClassID
                    WHERE MR.RevisionNo <> KPM.PreProcessRevNo
                    Group By KPG.GroupID, KPG.MachineGauge, KPG.MachineDia, FCM.ConceptDate, KPG.BrandID, 
                    KPM.BuyerTeamID, KPG.KnittingTypeID, KPG.Needle, KPG.CPI,
                    KPG.StartDate, KPG.EndDate, ISG.SubGroupName, KPM.BuyerID, FCM.SubGroupID,
                    KPG.GroupConceptNo, KPG.AddedBy, KPG.DateAdded, KPG.UpdatedBy, KPG.DateUpdated, ISNULL(CTO.ShortName,''), ISNULL(CCT.TeamName,''), 
                    FCM.TotalQty, FCM.Qty, KPM.NeedPreFinishingProcess, KPM.IsSubContact, FCM.TechnicalNameId, Technical.TechnicalName,
                    KnittingType.TypeName, KMS.SubClassName, FCM.IsBDS
                ),
                /*
                AllColors AS
                (
	                SELECT GroupID = A.PlanNo, FCM.GroupConceptNo, ColorName = CASE WHEN FCM.SubGroupID = 1 THEN ISV3.SegmentValue ELSE ISV5.SegmentValue END 
	                FROM {TableNames.RND_FREE_CONCEPT_MASTER} FCM
	                INNER JOIN {TableNames.Knitting_Plan_Master} KPM1 ON KPM1.ConceptID = FCM.ConceptID
	                INNER JOIN A ON A.PlanNo = KPM1.PlanNo
	                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = FCM.ItemMasterID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                ),
                ColorComma AS
                (
                    SELECT GroupID, GroupConceptNo, ColorName = 
	                STUFF((SELECT DISTINCT ', ' + ColorName
	                        FROM AllColors b 
	                        WHERE b.GroupID = a.GroupID AND b.GroupConceptNo = a.GroupConceptNo
	                        FOR XML PATH('')), 1, 2, '')
	                FROM AllColors a
	                GROUP BY GroupID, GroupConceptNo
                ),*/
                FinalList AS
                (
	                SELECT A.PlanNo, A.MachineGauge, A.MachineDia, A.ConceptDate, A.BrandID, 
                    A.BuyerTeamID, A.KnittingTypeID, A.Needle, A.CPI, --CC.ColorName,
                    A.StartDate, A.EndDate, A.SubGroupName, A.BuyerID, A.SubGroupID, 
                    A.GroupConceptNo, A.ConceptNo, A.AddedBy, A.DateAdded, A.UpdatedBy, A.DateUpdated, A.Buyer, A.BuyerTeam,
                    TotalQty = SUM(A.TotalQty), Qty = SUM(A.Qty), A.NeedPreFinishingProcess, A.IsSubContact, A.TechnicalNameId, A.TechnicalName,
                    A.KnittingType, A.MCSubClass, A.IsBDS, BalanceQty = SUM(BalanceQty), PlanQty = SUM(PlanQty)
	                FROM A
	                --LEFT JOIN ColorComma CC ON CC.GroupID = A.PlanNo AND CC.GroupConceptNo = A.GroupConceptNo
	                GROUP BY A.PlanNo, A.MachineGauge, A.MachineDia, A.ConceptDate, A.BrandID, 
                    A.BuyerTeamID, A.KnittingTypeID, A.Needle, A.CPI,
                    A.StartDate, A.EndDate, A.SubGroupName, A.BuyerID, A.SubGroupID, 
                    A.GroupConceptNo, A.ConceptNo, A.AddedBy, A.DateAdded, A.UpdatedBy, A.DateUpdated, A.Buyer, A.BuyerTeam,
                    A.NeedPreFinishingProcess, A.IsSubContact, A.TechnicalNameId, A.TechnicalName,
                    A.KnittingType, A.MCSubClass, A.IsBDS--, CC.ColorName
                )
                SELECT *, Count(*) Over() TotalRows FROM FinalList ";
                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "ORDER BY PlanNo DESC" : paginationInfo.OrderBy;
            }

            else if (type == KnittingProgramType.Concept)
            {
                if (status == Status.Pending)
                {
                    sql = $@"
                    ;WITH
                    M AS 
                    (
                        Select FCM.ConceptID, FCM.ConceptNo, FCM.ConceptDate, FCM.KnittingTypeID, FCM.ConstructionID,
                        FCM.TechnicalNameId,FCM.CompositionID, FCM.GSMID, Qty=FCM.TotalQty, FCM.Remarks, FCM.ConceptFor, FCM.ConceptStatusID, 
                        0 KPMasterID, 0 AS RevisionPending, FCM.SubGroupID, ISNULL(KPM.PlanQty, 0) PlanQty,E.EmployeeName UserName, FCM.TotalQty, FCM.GroupConceptNo
                        FROM {TableNames.RND_FREE_CONCEPT_MASTER} FCM
                        Inner Join {TableNames.RND_FREE_CONCEPT_MR_MASTER} FCMRM ON FCMRM.ConceptID = FCM.ConceptID
                        Inner Join {DbNames.EPYSL}..EntityTypeValue EV On FCM.ConceptStatusID = EV.ValueID
                        Left Join {TableNames.Knitting_Plan_Master} KPM ON FCM.ConceptID = KPM.ConceptID
                        LEFT JOIN  {DbNames.EPYSL}..LoginUser L ON L.UserCode = FCM.AddedBy
                        LEFT Join {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
                        WHERE FCMRM.IsBDS = 0 And EV.ValueName = 'Running' AND ((KPM.PlanQty < FCM.TotalQty And KPM.Active = 1) OR ISNULL(KPM.KPMasterID, 0) = 0)
                        AND FCMRM.RevisionNo = ISNULL(KPM.PreProcessRevNo,FCMRM.RevisionNo)
                        Group BY FCM.ConceptID, FCM.ConceptNo, FCM.ConceptDate, FCM.KnittingTypeID, FCM.ConstructionID,
                        FCM.TechnicalNameId,FCM.CompositionID, FCM.GSMID, FCM.TotalQty, FCM.Remarks, FCM.ConceptFor, FCM.GroupConceptNo, 
                        FCM.ConceptStatusID, FCM.SubGroupID, KPM.PlanQty,EmployeeName, FCM.TotalQty

                        UNION

                        SELECT CM.ConceptID, CM.ConceptNo, CM.ConceptDate, CM.KnittingTypeID, CM.ConstructionID, CM.TechnicalNameId, 
                        CM.CompositionID, CM.GSMID, CM.Qty, CM.Remarks, CM.ConceptFor, CM.ConceptStatusID, KP.KPMasterID, 
                        1 AS RevisionPending, CM.SubGroupID, ISNULL(KPC.BookingQty, 0) PlanQty, E.EmployeeName UserName, CM.TotalQty, CM.GroupConceptNo
	                    FROM {TableNames.Knitting_Plan_Master} KP
	                    INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} CM ON CM.ConceptID = KP.ConceptID
	                    INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_MASTER} MR ON MR.ConceptID = KP.ConceptID
	                    LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPMasterID = KP.KPMasterID
	                    LEFT JOIN  {DbNames.EPYSL}..LoginUser L ON L.UserCode = CM.AddedBy
                        LEFT Join {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
                        Where MR.IsBDS = 0 And KP.Active = 1 AND MR.RevisionNo != KP.PreProcessRevNo
	                    GROUP BY CM.ConceptID, CM.ConceptNo, CM.ConceptDate, CM.KnittingTypeID, CM.ConstructionID, 
                        CM.TechnicalNameId, CM.CompositionID, CM.GSMID, CM.Qty, CM.Remarks, CM.ConceptFor, CM.ConceptStatusID, 
                        KP.KPMasterID, CM.SubGroupID, E.EmployeeName, KPC.BookingQty, CM.TotalQty, CM.GroupConceptNo
                    )
                    , F As 
                    (
                        SELECT M.KPMasterID, M.RevisionPending, M.ConceptID, M.ConceptNo, M.ConceptDate, M.Qty, M.Remarks, 
                        M.PlanQty, KnittingType.TypeName KnittingType, Composition.SegmentValue Composition, 
                        Construction.SegmentValue Construction, Technical.TechnicalName,Gsm.SegmentValue Gsm, 
                        F.ValueName ConceptForName,S.ValueName ConceptStatus, ISG.SubGroupName,
                        UsesIn = Case FUP.PartName When 'Hem' Then FUP.PartName Else '' End,
                        M.UserName, M.TotalQty, M.GroupConceptNo,
                        Count(*) Over() TotalRows
                        FROM M
                        INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = M.ConceptID
                        LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID=M.KnittingTypeID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = M.CompositionID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = M.ConstructionID
                        LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME} Technical ON Technical.TechnicalNameId=M.TechnicalNameId
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = M.GSMID
                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue F ON F.ValueID = M.ConceptFor
                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue S ON S.ValueID = M.ConceptStatusID
                        LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG On M.SubGroupID = ISG.SubGroupID
                        LEFT JOIN {DbNames.EPYSL}..FabricUsedPart FUP ON FUP.FUPartID = FCM.FUPartID
                    )
                    Select *, Count(*) Over() TotalRows From F";

                    orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "ORDER BY ConceptID DESC" : paginationInfo.OrderBy;
                }
                else if (status == Status.Active)
                {
                    sql = $@"
                    ;WITH
                    M As 
                    (
	                    Select KP.KPMasterID, KP.ConceptID, KP.RevisionPending, KP.Active, SUM(ISNULL(KPC.BookingQty,0)) PlanQty, 
                        KP.PlanNo, KP.RevisionNo, KP.PreProcessRevNo,KPG.DateAdded
	                    FROM {TableNames.Knitting_Plan_Master} KP
	                    LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPMasterID = KP.KPMasterID
                        LEFT JOIN {TableNames.Knitting_Plan_Group} KPG ON KPG.GroupID=KP.PlanNo
                        Where KP.Active = 1 And KP.IsBDS = 0
	                    GROUP BY KP.KPMasterID, KP.ConceptID, KP.RevisionPending, KP.Active, KP.PlanNo, KP.RevisionNo, KP.PreProcessRevNo,KPG.DateAdded, KP.RevisionDate
                    ),
                    F AS 
                    (
                        SELECT KPMasterID, M.RevisionPending, M.ConceptID, FCM.ConceptNo, FCM.ConceptDate, FCM.Qty, FCM.Remarks, 
                        M.PlanQty, M.PlanNo, KnittingType.TypeName KnittingType,Composition.SegmentValue Composition, 
                        Construction.SegmentValue Construction, Technical.TechnicalName, Gsm.SegmentValue Gsm, F.ValueName ConceptForName, 
                        S.ValueName ConceptStatus, M.Active, E.EmployeeName UserName, ISG.SubGroupName,
                        UsesIn = Case FUP.PartName When 'Hem' Then FUP.PartName Else '' End,
                        FCM.TotalQty, FCM.GroupConceptNo,M.DateAdded
                        FROM M
                        INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = M.ConceptID
					    INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_MASTER} MR ON MR.ConceptID = M.ConceptID
                        LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID = FCM.KnittingTypeID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = FCM.CompositionID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = FCM.ConstructionID
                        LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME} Technical ON Technical.TechnicalNameId=FCM.TechnicalNameId
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = FCM.GSMID
                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue F ON F.ValueID = FCM.ConceptFor
                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue S ON S.ValueID = FCM.ConceptStatusID
                        LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG On FCM.SubGroupID = ISG.SubGroupID
                        LEFT JOIN  {DbNames.EPYSL}..LoginUser L ON L.UserCode = FCM.AddedBy
                        LEFT Join {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
                        LEFT JOIN {DbNames.EPYSL}..FabricUsedPart FUP ON FUP.FUPartID = FCM.FUPartID
					    WHERE MR.RevisionNo = M.PreProcessRevNo AND FCM.IsBDS = 0
                    ),
                    L As 
                    (
                        SELECT F.KPMasterID, F.RevisionPending, F.ConceptID, F.ConceptNo, F.ConceptDate, F.Qty, F.Remarks, 
                        F.PlanQty, F.PlanNo, F.KnittingType, F.Composition, F.Construction, F.TechnicalName, F.Gsm, F.TotalQty,
                        F.ConceptForName, F.ConceptStatus, F.Active, F.UserName, F.SubGroupName, F.GroupConceptNo,F.DateAdded, Count(*) Over() TotalRows 
					    FROM F
					    GROUP BY F.KPMasterID, F.RevisionPending, F.ConceptID, F.ConceptNo, F.ConceptDate, F.Qty, F.Remarks, 
                        F.PlanQty, F.PlanNo, F.KnittingType, F.Composition, F.Construction, F.TechnicalName, F.Gsm, 
                        F.ConceptForName, F.ConceptStatus, F.Active, F.UserName, F.SubGroupName, F.TotalQty, F.GroupConceptNo,F.DateAdded
                    )
                    Select *, Count(*) Over() TotalRows From L ";
                    orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "ORDER BY ConceptID DESC" : paginationInfo.OrderBy;
                }
                else if (status == Status.InActive)
                {
                    sql = $@"
                    ;WITH
                    M As (
	                    Select KP.KPMasterID, KP.ConceptID, KP.RevisionPending, KP.Active, SUM(ISNULL(KPC.BookingQty,0)) PlanQty, KP.PlanNo
	                    FROM {TableNames.Knitting_Plan_Master} KP
	                    LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPMasterID = KP.KPMasterID
                        Where KP.Active = 0 And KP.IsBDS = 0
	                    GROUP BY KP.KPMasterID, KP.ConceptID, KP.RevisionPending, KP.Active, KP.PlanNo
                    ), 
                    L As 
                    (
                        SELECT KPMasterID, M.RevisionPending, M.ConceptID, FCM.ConceptNo, FCM.ConceptDate, FCM.Qty, FCM.Remarks, M.PlanQty, M.PlanNo
	                    , KnittingType.TypeName KnittingType,Composition.SegmentValue Composition, Construction.SegmentValue Construction
	                    , Technical.TechnicalName,Gsm.SegmentValue Gsm, F.ValueName ConceptForName, S.ValueName ConceptStatus, M.Active,E.EmployeeName UserName
                        , FCM.TotalQty, Count(*) Over() TotalRows,ISG.SubGroupName, UsesIn = Case FUP.PartName When 'Hem' Then FUP.PartName Else '' End, FCM.GroupConceptNo
                        FROM M
                        INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = M.ConceptID
                        LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID = FCM.KnittingTypeID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = FCM.CompositionID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = FCM.ConstructionID
                        LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME} Technical ON Technical.TechnicalNameId=FCM.TechnicalNameId
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = FCM.GSMID
                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue F ON F.ValueID = FCM.ConceptFor
                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue S ON S.ValueID = FCM.ConceptStatusID
                        LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG On FCM.SubGroupID = ISG.SubGroupID
                        LEFT JOIN {DbNames.EPYSL}..LoginUser L ON L.UserCode = FCM.AddedBy
                        LEFT Join {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
                        LEFT JOIN {DbNames.EPYSL}..FabricUsedPart FUP ON FUP.FUPartID = FCM.FUPartID
                        WHERE FCM.IsBDS = 0
                        GROUP BY KPMasterID, M.RevisionPending, M.ConceptID, FCM.ConceptNo, FCM.ConceptDate, FCM.Qty, FCM.Remarks, M.PlanQty, M.PlanNo
	                    , KnittingType.TypeName,Composition.SegmentValue, Construction.SegmentValue, Technical.TechnicalName,Gsm.SegmentValue, F.ValueName, 
						S.ValueName, M.Active, E.EmployeeName, ISG.SubGroupName, FCM.TotalQty, FCM.GroupConceptNo,FUP.PartName
                    )
                    Select *, Count(*) Over() TotalRows From L ";
                    orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "ORDER BY ConceptID DESC" : paginationInfo.OrderBy;
                }
                else
                {
                    sql = $@"
                    ;WITH
                    M As 
                    (
	                    Select KP.KPMasterID, KP.ConceptID, KP.RevisionPending, KP.Active, SUM(ISNULL(KPC.BookingQty,0)) PlanQty, KP.PlanNo
	                    FROM {TableNames.Knitting_Plan_Master} KP
	                    LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPMasterID = KP.KPMasterID
                        Where KP.IsBDS = 0
	                    GROUP BY KP.KPMasterID, KP.ConceptID, KP.RevisionPending, KP.Active, KP.PlanNo
                    ), L As 
                    (
                        SELECT KPMasterID, M.RevisionPending, M.ConceptID, FCM.ConceptNo, FCM.ConceptDate, FCM.Qty, FCM.Remarks, M.PlanQty, M.PlanNo
	                    ,KnittingType.TypeName KnittingType,Composition.SegmentValue Composition, Construction.SegmentValue Construction
	                    ,Technical.TechnicalName,Gsm.SegmentValue Gsm, F.ValueName ConceptForName, S.ValueName ConceptStatus, M.Active
                        ,FCM.TotalQty, Count(*) Over() TotalRows,ISG.SubGroupName, UsesIn = Case FUP.PartName When 'Hem' Then FUP.PartName Else '' End, FCM.GroupConceptNo
                        FROM M
                        INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = M.ConceptID
                        LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID = FCM.KnittingTypeID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = FCM.CompositionID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = FCM.ConstructionID
                        LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME} Technical ON Technical.TechnicalNameId=FCM.TechnicalNameId
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = FCM.GSMID
                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue F ON F.ValueID = FCM.ConceptFor
                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue S ON S.ValueID = FCM.ConceptStatusID
                        LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG On FCM.SubGroupID = ISG.SubGroupID
                        LEFT JOIN {DbNames.EPYSL}..FabricUsedPart FUP ON FUP.FUPartID = FCM.FUPartID
                        WHERE FCM.IsBDS = 0
                        GROUP BY KPMasterID, M.RevisionPending, M.ConceptID, FCM.ConceptNo, FCM.ConceptDate, FCM.Qty, FCM.Remarks, M.PlanQty, M.PlanNo
	                    , KnittingType.TypeName,Composition.SegmentValue, Construction.SegmentValue, Technical.TechnicalName,Gsm.SegmentValue, F.ValueName, 
						S.ValueName, M.Active, ISG.SubGroupName, FCM.TotalQty, FCM.GroupConceptNo,FUP.PartName
                    )
                    Select *, Count(*) Over() TotalRows From L ";
                    orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "ORDER BY ConceptID DESC" : paginationInfo.OrderBy;
                }
            }

            #endregion Knitting Program for Concept

            #region Knitting Program for Bulk

            else if (type == KnittingProgramType.Bulk)
            {
                if (status == Status.Pending)
                {
                    sql = $@"
                      ;WITH
                    PMCApprovedList AS
                    (
	                    SELECT FBA.BookingID
	                    FROM {TableNames.FBBOOKING_ACKNOWLEDGE} FBA
	                    WHERE FBA.IsApprovedByPMC = 1
	                    GROUP BY FBA.BookingID
                    ),
                    FCM AS (
	                    Select b.WithoutOB, a.ConceptID, a.ConceptNo, a.BookingChildID, a.GroupConceptNo, a.BookingID
	                    From {TableNames.RND_FREE_CONCEPT_MASTER} a
	                    Inner Join {TableNames.FabricBookingAcknowledge} b on b.BookingID = a.BookingID
	                    INNER JOIN PMCApprovedList AL ON AL.BookingID = a.BookingID
	                    WHERE a.IsBDS = 2
	                    GROUP BY b.WithoutOB, a.ConceptID, a.ConceptNo, a.BookingChildID, a.GroupConceptNo, a.BookingID
                    ),
                    ParticalKP AS
                    (
	                    SELECT FCM.ConceptID, FCM.GroupConceptNo, KPC.UnitID, 
						TotalKPQty = Case When YBC.BookingUnitID =28 Then FBC.GreyProdQty 
						Else 
							Case When YBC.QtyInKG > 0 Then Round( (YBC.BookingQty/YBC.QtyInKG)*YBC.GreyProdQty,0) Else 0 END
						END,
						Qty = SUM(KPC.BookingQty), YBCItemMasterID = YBC.ItemMasterID, YBCBItemMasterID = YBCB.ItemMasterID
                        FROM {TableNames.Knitting_Plan_Child} KPC
	                    INNER JOIN {TableNames.Knitting_Plan_Master} KP ON KP.KPMasterID = KPC.KPMasterID
                        INNER JOIN FCM ON FCM.ConceptID = KP.ConceptID
	                    LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FBC ON FBC.BookingChildID = FCM.BookingChildID AND FBC.BookingID = FCM.BookingID
						LEFT JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.BookingChildID = FBC.BookingChildID
						LEFT JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID = YBC.YBookingID
						LEFT JOIN {TableNames.YarnBookingChild_New_Bk} YBCB ON YBCB.YBChildID = YBC.YBChildID AND YBCB.RevisionNo = YBM.RevisionNo - 1
	                    GROUP BY FCM.ConceptID, FCM.GroupConceptNo, KPC.UnitID, FBC.GreyProdQty, YBC.BookingUnitID, YBC.BookingQty, YBC.QtyInKG, YBC.GreyProdQty,
                        YBC.ItemMasterID, YBCB.ItemMasterID
                    ),
                    FinalParticalKP AS
                    (
	                    SELECT KP.*
	                    FROM ParticalKP KP
	                    WHERE KP.TotalKPQty > KP.Qty
                    ),
                    FinalParticalKP_RevPend AS
                    (
	                    SELECT KP.*
	                    FROM ParticalKP KP
	                    WHERE KP.TotalKPQty < KP.Qty OR KP.YBCItemMasterID <> KP.YBCBItemMasterID
                    ),
                    M_Main AS 
                    (
                        SELECT KPMasterID = 0, FCM.ConceptID, FCM.ConceptNo, FCM.GroupConceptNo, FCM.ConceptDate, FCM.Qty, FCM.Remarks, FCM.KnittingTypeID, FCM.CompositionID, FCM.GSMID,
                        FCM.ConstructionID, FCM.TechnicalNameId, FCM.ConceptFor, FCM.ConceptStatusID, FCM.SubGroupID, FCM.BookingID, FCM.BuyerID, FCM.BuyerTeamID,
	                    FCM.ExportOrderID, FCM1.WithoutOB, FCM.ItemMasterID,
						-------------------------
	                    ProduceKnittingQty = 
						CASE WHEN FCM.IsBDS = 2 THEN 
						
						ISNULL(
						Case When YBC.BookingUnitID =28 
						Then FBC.GreyProdQty 
						Else Case When isnull(YBC.QtyInKG,0)>0 Then Round((isnull(YBC.BookingQty,0)/isnull(YBC.QtyInKG,0))*isnull(YBC.GreyProdQty,0),0) Else 0 END 
						END
						,0) 
						
						ELSE FCM.ProduceKnittingQty END, 
						-------------------------
	                    RemainingPlanQty = ISNULL(
						CASE WHEN FCM.IsBDS = 2 THEN 
												ISNULL(
						Case When YBC.BookingUnitID =28 
						Then FBC.GreyProdQty 
						Else Case When isnull(YBC.QtyInKG,0)>0 Then Round((isnull(YBC.BookingQty,0)/isnull(YBC.QtyInKG,0))*isnull(YBC.GreyProdQty,0),0) Else 0 END 
						END
						,0) 
						ELSE FCM.ProduceKnittingQty END,0) - ISNULL(KP.PlanQty,0),
						RevisionPending = 0
						-------------------------
                        FROM {TableNames.RND_FREE_CONCEPT_MR_MASTER} FCMRM
                        INNER JOIN FCM FCM1 ON FCM1.ConceptID = FCMRM.ConceptID
	                    INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = FCM1.ConceptID
	                    LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FBC ON FBC.BookingChildID = FCM.BookingChildID AND FBC.BookingID = FCM.BookingID
						LEFT JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.BookingChildID = FBC.BookingChildID
	                    LEFT JOIN {TableNames.Knitting_Plan_Master} KP ON KP.ConceptID = FCM.ConceptID
                        WHERE KP.ConceptID IS NULL AND FCM1.WithoutOB = 1
	                    GROUP BY FCM.ConceptID, FCM.ConceptNo, FCM.GroupConceptNo, FCM.ConceptDate, FCM.Qty, FCM.Remarks, FCM.KnittingTypeID, FCM.CompositionID, FCM.GSMID,
                        FCM.ConstructionID, FCM.TechnicalNameId, FCM.ConceptFor, FCM.ConceptStatusID, FCM.SubGroupID, FCM.BookingID, FCM.BuyerID, 
	                    FCM.BuyerTeamID, FCM.ExportOrderID, FCM1.WithoutOB, FCM.ItemMasterID,FCM.ProduceKnittingQty,
	                    FCM.IsBDS, YBC.BookingUnitID, FBC.GreyProdQty, isnull(YBC.BookingQty,0), isnull(YBC.QtyInKG,0), isnull(YBC.GreyProdQty,0), FCM.ProduceKnittingQty,ISNULL(KP.PlanQty,0)
	                    
						UNION

	                    SELECT KPMasterID = 0, FCM.ConceptID, FCM.ConceptNo, FCM.GroupConceptNo, FCM.ConceptDate, FCM.Qty, FCM.Remarks, FCM.KnittingTypeID, FCM.CompositionID, FCM.GSMID,
                        FCM.ConstructionID, FCM.TechnicalNameId, FCM.ConceptFor, FCM.ConceptStatusID, FCM.SubGroupID, FCM.BookingID, FCM.BuyerID, 
	                    FCM.BuyerTeamID, FCM.ExportOrderID, FCM1.WithoutOB, FCM.ItemMasterID, 
	                    -------------------------
	                    ProduceKnittingQty = 
						CASE WHEN FCM.IsBDS = 2 THEN 
						
						ISNULL(
						Case When YBC.BookingUnitID =28 
						Then FBC.GreyProdQty 
						Else Case When isnull(YBC.QtyInKG,0)>0 Then Round((isnull(YBC.BookingQty,0)/isnull(YBC.QtyInKG,0))*isnull(YBC.GreyProdQty,0),0) Else 0 END 
						END
						,0) 
						
						ELSE FCM.ProduceKnittingQty END, 
						-------------------------
	                    RemainingPlanQty = ISNULL(
						CASE WHEN FCM.IsBDS = 2 THEN 
												ISNULL(
						Case When YBC.BookingUnitID =28 
						Then FBC.GreyProdQty 
						Else Case When isnull(YBC.QtyInKG,0)>0 Then Round((isnull(YBC.BookingQty,0)/isnull(YBC.QtyInKG,0))*isnull(YBC.GreyProdQty,0),0) Else 0 END 
						END
						,0) 
						ELSE FCM.ProduceKnittingQty END,0) - ISNULL(KP.PlanQty,0),
						RevisionPending = 0
						-------------------------
                        FROM {TableNames.RND_FREE_CONCEPT_MR_MASTER} FCMRM
                        INNER JOIN FCM FCM1 ON FCM1.ConceptID = FCMRM.ConceptID
	                    INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = FCM1.ConceptID
	                    LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FBC ON FBC.BookingChildID = FCM.BookingChildID AND FBC.BookingID = FCM.BookingID
	                    LEFT JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.BookingChildID = FBC.BookingChildID
						LEFT JOIN {TableNames.Knitting_Plan_Master} KP ON KP.ConceptID = FCM.ConceptID
                        WHERE KP.ConceptID IS NULL AND FCM1.WithoutOB = 0
	                    GROUP BY FCM.ConceptID, FCM.ConceptNo, FCM.GroupConceptNo, FCM.ConceptDate, FCM.Qty, FCM.Remarks, FCM.KnittingTypeID, FCM.CompositionID, FCM.GSMID,
                        FCM.ConstructionID, FCM.TechnicalNameId, FCM.ConceptFor, FCM.ConceptStatusID, FCM.SubGroupID, FCM.BookingID, FCM.BuyerID, 
	                    FCM.BuyerTeamID, FCM.ExportOrderID, FCM1.WithoutOB, FCM.ItemMasterID,FCM.ProduceKnittingQty,
	                    FCM.IsBDS, YBC.BookingUnitID, FBC.GreyProdQty, isnull(YBC.BookingQty,0), isnull(YBC.QtyInKG,0), isnull(YBC.GreyProdQty,0), FCM.ProduceKnittingQty,ISNULL(KP.PlanQty,0)
                        
						UNION

	                    SELECT KPMasterID = 0, FCM.ConceptID, FCM.ConceptNo, FCM.GroupConceptNo, FCM.ConceptDate, FCM.Qty, FCM.Remarks, FCM.KnittingTypeID, FCM.CompositionID, FCM.GSMID,
                        FCM.ConstructionID, FCM.TechnicalNameId, FCM.ConceptFor, FCM.ConceptStatusID, FCM.SubGroupID, FCM.BookingID, FCM.BuyerID, 
	                    FCM.BuyerTeamID, FCM.ExportOrderID, FCM1.WithoutOB, FCM.ItemMasterID, 
	                    --ProduceKnittingQty = CASE WHEN FCM.IsBDS = 2 THEN ISNULL(FBC.GreyProdQty,0) ELSE FCM.ProduceKnittingQty END,
						ProduceKnittingQty = CASE WHEN FCM.IsBDS = 2 THEN ISNULL(KP.TotalKPQty,0) ELSE FCM.ProduceKnittingQty END,
	                    RemainingPlanQty = ISNULL(KP.TotalKPQty,0) - ISNULL(KP.Qty,0),
						RevisionPending = 0
                        FROM {TableNames.RND_FREE_CONCEPT_MR_MASTER} FCMRM
                        INNER JOIN FCM FCM1 ON FCM1.ConceptID = FCMRM.ConceptID
	                    INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = FCM1.ConceptID
	                    LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FBC ON FBC.BookingChildID = FCM.BookingChildID AND FBC.BookingID = FCM.BookingID
	                    INNER JOIN FinalParticalKP KP ON KP.ConceptID = FCM.ConceptID
	                    GROUP BY FCM.ConceptID, FCM.ConceptNo, FCM.GroupConceptNo, FCM.ConceptDate, FCM.Qty, FCM.Remarks, FCM.KnittingTypeID, FCM.CompositionID, FCM.GSMID,
                        FCM.ConstructionID, FCM.TechnicalNameId, FCM.ConceptFor, FCM.ConceptStatusID, FCM.SubGroupID, FCM.BookingID, FCM.BuyerID, 
	                    FCM.BuyerTeamID, FCM.ExportOrderID, FCM1.WithoutOB, FCM.ItemMasterID, FCM.ProduceKnittingQty, ISNULL(KP.Qty,0), ISNULL(KP.TotalKPQty,0),
	                    CASE WHEN FCM.IsBDS = 2 THEN ISNULL(KP.TotalKPQty,0) ELSE FCM.ProduceKnittingQty END, ISNULL(KP.TotalKPQty,0),ISNULL(KP.Qty,0)
						
						--UNION 

						--SELECT KP1.KPMasterID, FCM.ConceptID, FCM.ConceptNo, FCM.GroupConceptNo, FCM.ConceptDate, FCM.Qty, FCM.Remarks, FCM.KnittingTypeID, FCM.CompositionID, FCM.GSMID,
                        --FCM.ConstructionID, FCM.TechnicalNameId, FCM.ConceptFor, FCM.ConceptStatusID, FCM.SubGroupID, FCM.BookingID, FCM.BuyerID, 
	                    --FCM.BuyerTeamID, FCM.ExportOrderID, FCM1.WithoutOB, FCM.ItemMasterID, 
	                    ----ProduceKnittingQty = CASE WHEN FCM.IsBDS = 2 THEN ISNULL(FBC.GreyProdQty,0) ELSE FCM.ProduceKnittingQty END,
						--ProduceKnittingQty = CASE WHEN FCM.IsBDS = 2 THEN ISNULL(KP.TotalKPQty,0) ELSE FCM.ProduceKnittingQty END,
	                    --RemainingPlanQty = ISNULL(KP.TotalKPQty,0) - ISNULL(KP.Qty,0),
						--RevisionPending = 1
                        --FROM {TableNames.Knitting_Plan_Master} KP1
						--INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_MASTER} MR ON MR.ConceptID = KP1.ConceptID
                        --INNER JOIN FCM FCM1 ON FCM1.ConceptID = KP1.ConceptID
	                    --INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = FCM1.ConceptID
	                    --LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FBC ON FBC.BookingChildID = FCM.BookingChildID AND FBC.BookingID = FCM.BookingID
						--LEFT JOIN FinalParticalKP KP ON KP.ConceptID = FCM.ConceptID
						--WHERE MR.RevisionNo <> KP1.PreProcessRevNo AND 
						--FCM.IsBDS = 2
	                    --GROUP BY FCM.ConceptID, FCM.ConceptNo, FCM.GroupConceptNo, FCM.ConceptDate, FCM.Qty, FCM.Remarks, FCM.KnittingTypeID, FCM.CompositionID, FCM.GSMID,
                        --FCM.ConstructionID, FCM.TechnicalNameId, FCM.ConceptFor, FCM.ConceptStatusID, FCM.SubGroupID, FCM.BookingID, FCM.BuyerID, 
	                    --FCM.BuyerTeamID, FCM.ExportOrderID, FCM1.WithoutOB, FCM.ItemMasterID, FCM.ProduceKnittingQty, ISNULL(KP.Qty,0), ISNULL(KP.TotalKPQty,0),
	                    --CASE WHEN FCM.IsBDS = 2 THEN ISNULL(KP.TotalKPQty,0) ELSE FCM.ProduceKnittingQty END, ISNULL(KP.TotalKPQty,0),ISNULL(KP.Qty,0),KP1.KPMasterID
                    ), 
					M_RevPend AS(
					
						SELECT KP1.KPMasterID, FCM.ConceptID, FCM.ConceptNo, FCM.GroupConceptNo, FCM.ConceptDate, FCM.Qty, FCM.Remarks, FCM.KnittingTypeID, FCM.CompositionID, FCM.GSMID,
                        FCM.ConstructionID, FCM.TechnicalNameId, FCM.ConceptFor, FCM.ConceptStatusID, FCM.SubGroupID, FCM.BookingID, FCM.BuyerID, 
	                    FCM.BuyerTeamID, FCM.ExportOrderID, FCM1.WithoutOB, FCM.ItemMasterID, 
	                    --ProduceKnittingQty = CASE WHEN FCM.IsBDS = 2 THEN ISNULL(FBC.GreyProdQty,0) ELSE FCM.ProduceKnittingQty END,
						ProduceKnittingQty = CASE WHEN FCM.IsBDS = 2 THEN ISNULL(KP.TotalKPQty,0) ELSE FCM.ProduceKnittingQty END,
	                    RemainingPlanQty = ISNULL(KP.TotalKPQty,0) - ISNULL(KP.Qty,0),
						RevisionPending = 1, KPQty = ISNULL(KP.Qty,0), YBCItemMasterID = YBC.ItemMasterID, YBCBItemMasterID = YBCB.ItemMasterID
                        FROM {TableNames.Knitting_Plan_Master} KP1
						INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_MASTER} MR ON MR.ConceptID = KP1.ConceptID
                        INNER JOIN FCM FCM1 ON FCM1.ConceptID = KP1.ConceptID
	                    INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = FCM1.ConceptID
	                    LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FBC ON FBC.BookingChildID = FCM.BookingChildID AND FBC.BookingID = FCM.BookingID
						LEFT JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.BookingChildID = FBC.BookingChildID
						LEFT JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID = YBC.YBookingID
						LEFT JOIN {TableNames.YarnBookingChild_New_Bk} YBCB ON YBCB.YBChildID = YBC.YBChildID AND YBCB.RevisionNo = YBM.RevisionNo - 1
						LEFT JOIN FinalParticalKP_RevPend KP ON KP.ConceptID = FCM.ConceptID
						WHERE --MR.RevisionNo <> KP1.PreProcessRevNo AND 
						FCM.IsBDS = 2 
	                    GROUP BY FCM.ConceptID, FCM.ConceptNo, FCM.GroupConceptNo, FCM.ConceptDate, FCM.Qty, FCM.Remarks, FCM.KnittingTypeID, FCM.CompositionID, FCM.GSMID,
                        FCM.ConstructionID, FCM.TechnicalNameId, FCM.ConceptFor, FCM.ConceptStatusID, FCM.SubGroupID, FCM.BookingID, FCM.BuyerID, 
	                    FCM.BuyerTeamID, FCM.ExportOrderID, FCM1.WithoutOB, FCM.ItemMasterID, FCM.ProduceKnittingQty, ISNULL(KP.Qty,0), ISNULL(KP.TotalKPQty,0),
	                    CASE WHEN FCM.IsBDS = 2 THEN ISNULL(KP.TotalKPQty,0) ELSE FCM.ProduceKnittingQty END, ISNULL(KP.TotalKPQty,0),ISNULL(KP.Qty,0),KP1.KPMasterID,
						YBC.ItemMasterID, YBCB.ItemMasterID
					),
					M_RevPend2 As(
					SELECT MRP.KPMasterID, MRP.ConceptID, MRP.ConceptNo, MRP.GroupConceptNo, MRP.ConceptDate, MRP.Qty, MRP.Remarks, MRP.KnittingTypeID, MRP.CompositionID, MRP.GSMID,
                        MRP.ConstructionID, MRP.TechnicalNameId, MRP.ConceptFor, MRP.ConceptStatusID, MRP.SubGroupID, MRP.BookingID, MRP.BuyerID, 
	                    MRP.BuyerTeamID, MRP.ExportOrderID, MRP.WithoutOB, MRP.ItemMasterID, MRP.ProduceKnittingQty, MRP.RemainingPlanQty, MRP.RevisionPending
					FROM M_RevPend MRP
					WHERE MRP.ProduceKnittingQty < MRP.KPQty OR MRP.YBCItemMasterID <> MRP.YBCBItemMasterID
					),
					M AS (
					Select * FROM M_Main 
					UNION
					Select * FROM M_RevPend2
					), 
                    L As 
                    (
                        SELECT M.KPMasterID,M.ConceptID, M.ConceptNo, M.GroupConceptNo, M.ConceptDate, M.Qty, M.Remarks, 0 PlanQty,
                        Technical.TechnicalName,
	                    F.ValueName ConceptForName,S.ValueName ConceptStatus,
                        M.SubGroupID, ISG.SubGroupName, M.BookingID, M.BuyerID, M.BuyerTeamID,
                        Buyer = CASE WHEN M.BuyerID > 0 THEN ISNULL(CTO.ShortName,'') ELSE 'R&D' END, 
                        BuyerTeam = CASE WHEN M.BuyerTeamID > 0 THEN ISNULL(CCT.TeamName,'') ELSE 'R&D' END,
                        M.WithoutOB, BookingQty = FCM.Qty, M.ProduceKnittingQty,
	                    M.RemainingPlanQty, Uom = CASE WHEN FCM.SubGroupId = 1 THEN 'Kg' ELSE 'Pcs' END,

	                    Construction = ISV1.SegmentValue, 
                        Composition = CASE WHEN M.SubGroupID = 1 THEN ISV2.SegmentValue ELSE '' END,
	                    ColorName = CASE WHEN M.SubGroupID = 1 THEN ISV3.SegmentValue ELSE ISV5.SegmentValue END, 
                        GSM = CASE WHEN M.SubGroupID = 1 THEN ISV4.SegmentValue ELSE '' END, 
                        DyeingType = CASE WHEN M.SubGroupID = 1 THEN ISV6.SegmentValue ELSE '' END, 
                        KnittingType = CASE WHEN M.SubGroupID = 1 THEN ISV7.SegmentValue ELSE '' END,
                        Length = CASE WHEN M.SubGroupID = 1 THEN 0 ELSE CONVERT(decimal(18,2),ISV3.SegmentValue) END,
                        Width = CASE WHEN M.SubGroupID = 1 THEN 0 ELSE CONVERT(decimal(18,2),ISV4.SegmentValue) END,
	                    Size = CASE WHEN M.SubGroupID <> 1 THEN ISV3.SegmentValue + ' X ' + ISV4.SegmentValue ELSE '' END,
						M.RevisionPending

                        FROM M
                        INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = M.ConceptID
                        LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID = M.KnittingTypeID
                        LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME} Technical ON Technical.TechnicalNameId=M.TechnicalNameId

	                    LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = M.ItemMasterID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID

                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue F ON F.ValueID = M.ConceptFor
                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue S ON S.ValueID = M.ConceptStatusID
                        LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG On M.SubGroupID = ISG.SubGroupID
	                    LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = M.BuyerID
	                    LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = M.BuyerTeamID
                    ) 
                    Select *, Count(*) Over() TotalRows From L";

                    orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "ORDER BY ConceptID DESC" : paginationInfo.OrderBy;
                }
                else if (status == Status.Active)
                {
                    if (!isNew)
                    {
                        sql = $@"
                    ;WITH
                    M As 
                    (
	                   Select KP.KPMasterID, KP.ConceptID, KP.PreProcessRevNo, KP.RevisionPending, KP.Active, SUM(ISNULL(KPC.BookingQty,0)) PlanQty, 
                        KP.PlanNo, KP.RevisionNo, KP.BuyerID, KP.BuyerTeamID, KP.ExportOrderID, KPC.SubGroupID,
	                    Contact = CASE WHEN ISNULL(KJC.IsSubContact,0)=1 THEN C.ShortName  ELSE CC.UnitName END
	                    FROM {TableNames.Knitting_Plan_Master} KP
	                    LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPMasterID = KP.KPMasterID
	                    LEFT JOIN {TableNames.KNITTING_JOB_CARD_Master} KJC ON KJC.GroupID = KPC.PlanNo AND KJC.ConceptID = KP.ConceptID AND KJC.GroupID NOT IN (1,0)
	                    LEFT JOIN {TableNames.KNITTING_UNIT} CC ON CC.KnittingUnitID = KJC.ContactID
	                    LEFT JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = KJC.ContactID
                        Where KP.Active = 1 And IsBDS = 2
	                    GROUP BY KP.KPMasterID, KP.ConceptID, KP.PreProcessRevNo, KP.RevisionPending, KP.Active, KP.PlanNo, KP.RevisionNo, 
	                    KP.BuyerID, KP.BuyerTeamID, KP.ExportOrderID, KPC.SubGroupID,ISNULL(KJC.IsSubContact,0),C.ShortName,CC.UnitName
                    ),
                    TotalKPDone AS
		            (
			            SELECT M.ConceptID, RemainingPlanQty = SUM(M.PlanQty)
			            FROM M
			            GROUP BY M.ConceptID
		            ),
                    F AS 
                    (
                        SELECT KPMasterID, M.RevisionPending, M.ConceptID, FCM.ConceptNo, FCM.ConceptDate, FCM.Qty, FCM.Remarks, 
						M.PlanQty, M.PlanNo, 
						Technical.TechnicalName, 
						F.ValueName ConceptForName, S.ValueName ConceptStatus, M.Active, E.EmployeeName UserName, ISG.SubGroupName, 
						M.BuyerID, M.BuyerTeamID, M.ExportOrderID, 
                        Buyer = CASE WHEN M.BuyerID > 0 THEN ISNULL(CTO.ShortName,'') ELSE 'R&D' END, 
                        BuyerTeam = CASE WHEN M.BuyerTeamID > 0 THEN ISNULL(CCT.TeamName,'') ELSE 'R&D' END,
                        FCM.GroupConceptNo, BookingQty = FCM.Qty, M.Contact,
                        -------------------------
                        ProduceKnittingQty = 
						CASE WHEN FCM.IsBDS = 2 THEN 
						ISNULL(
						Case When YBC.BookingUnitID =28 
						Then FBC.GreyProdQty 
						Else Case When isnull(YBC.QtyInKG,0)>0 Then Round((isnull(YBC.BookingQty,0)/isnull(YBC.QtyInKG,0))*isnull(YBC.GreyProdQty,0),0) Else 0 END 
						END
						,0) 
						ELSE FCM.ProduceKnittingQty END,
						-------------------------
                        RemainingPlanQty = ISNULL(CASE WHEN FCM.IsBDS = 2 THEN 
						ISNULL(
						Case When YBC.BookingUnitID =28 
						Then FBC.GreyProdQty 
						Else Case When isnull(YBC.QtyInKG,0)>0 Then Round((isnull(YBC.BookingQty,0)/isnull(YBC.QtyInKG,0))*isnull(YBC.GreyProdQty,0),0) Else 0 END 
						END
						,0) 
						ELSE FCM.ProduceKnittingQty END,0) 
						- ISNULL(KPD.RemainingPlanQty,0), 
		                -------------------------
		                Uom = CASE WHEN FCM.SubGroupId = 1 THEN 'Kg' ELSE 'Pcs' END,

						Construction = ISV1.SegmentValue, 
                        Composition = CASE WHEN M.SubGroupID = 1 THEN ISV2.SegmentValue ELSE '' END,
						ColorName = CASE WHEN M.SubGroupID = 1 THEN ISV3.SegmentValue ELSE ISV5.SegmentValue END, 
                        GSM = CASE WHEN M.SubGroupID = 1 THEN ISV4.SegmentValue ELSE '' END, 
                        DyeingType = CASE WHEN M.SubGroupID = 1 THEN ISV6.SegmentValue ELSE '' END, 
                        KnittingType = CASE WHEN M.SubGroupID = 1 THEN ISV7.SegmentValue ELSE '' END,
                        Length = CASE WHEN M.SubGroupID = 1 THEN 0 ELSE CONVERT(decimal(18,2),ISV3.SegmentValue) END,
                        Width = CASE WHEN M.SubGroupID = 1 THEN 0 ELSE CONVERT(decimal(18,2),ISV4.SegmentValue) END,
						Size = CASE WHEN M.SubGroupID <> 1 THEN ISV3.SegmentValue + ' X ' + ISV4.SegmentValue ELSE '' END,
                        YBCItemMasterID = YBC.ItemMasterID, YBCBItemMasterID = YBCB.ItemMasterID

                        FROM M
                        INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = M.ConceptID
					    INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_MASTER} MR ON MR.ConceptID = M.ConceptID
                        LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FBC ON FBC.BookingChildID = FCM.BookingChildID AND FBC.BookingID = FCM.BookingID AND FBC.BookingQty <> 0
                        LEFT JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.BookingChildID = FBC.BookingChildID
						LEFT JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID = YBC.YBookingID
						LEFT JOIN {TableNames.YarnBookingChild_New_Bk} YBCB ON YBCB.YBChildID = YBC.YBChildID AND YBCB.RevisionNo = YBM.RevisionNo - 1
                        LEFT JOIN TotalKPDone KPD ON KPD.ConceptID = FCM.ConceptID
                        LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME} Technical ON Technical.TechnicalNameId=FCM.TechnicalNameId
                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue F ON F.ValueID = FCM.ConceptFor
                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue S ON S.ValueID = FCM.ConceptStatusID
                        LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG On FCM.SubGroupID = ISG.SubGroupID
                        LEFT JOIN {DbNames.EPYSL}..LoginUser L ON L.UserCode = FCM.AddedBy
                        LEFT Join {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
						LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = M.BuyerID
						LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = M.BuyerTeamID

						LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = FCM.ItemMasterID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
					    WHERE --MR.RevisionNo = M.PreProcessRevNo AND 
                        FCM.IsBDS = 2
                        GROUP BY KPMasterID, M.RevisionPending, M.ConceptID, FCM.ConceptNo, FCM.ConceptDate, FCM.Qty, FCM.Remarks, 
						M.PlanQty, M.PlanNo, 
						Technical.TechnicalName, 
						F.ValueName, S.ValueName, M.Active, E.EmployeeName, ISG.SubGroupName, 
						M.BuyerID, M.BuyerTeamID, M.ExportOrderID, 
                        CASE WHEN M.BuyerID > 0 THEN ISNULL(CTO.ShortName,'') ELSE 'R&D' END, 
                        CASE WHEN M.BuyerTeamID > 0 THEN ISNULL(CCT.TeamName,'') ELSE 'R&D' END,
                        FCM.GroupConceptNo, FCM.Qty, M.Contact,
                        -------------------------
                        
						CASE WHEN FCM.IsBDS = 2 THEN 
						ISNULL(
						Case When YBC.BookingUnitID =28 
						Then FBC.GreyProdQty 
						Else Case When isnull(YBC.QtyInKG,0)>0 Then Round((isnull(YBC.BookingQty,0)/isnull(YBC.QtyInKG,0))*isnull(YBC.GreyProdQty,0),0) Else 0 END 
						END
						,0) 
						ELSE FCM.ProduceKnittingQty END,
						-------------------------
                        ISNULL(CASE WHEN FCM.IsBDS = 2 THEN 
						ISNULL(
						Case When YBC.BookingUnitID =28 
						Then FBC.GreyProdQty 
						Else Case When isnull(YBC.QtyInKG,0)>0 Then Round((isnull(YBC.BookingQty,0)/isnull(YBC.QtyInKG,0))*isnull(YBC.GreyProdQty,0),0) Else 0 END 
						END
						,0) 
						ELSE FCM.ProduceKnittingQty END,0) 
						- ISNULL(KPD.RemainingPlanQty,0), 
		                -------------------------
		                CASE WHEN FCM.SubGroupId = 1 THEN 'Kg' ELSE 'Pcs' END,

						ISV1.SegmentValue, 
                        CASE WHEN M.SubGroupID = 1 THEN ISV2.SegmentValue ELSE '' END,
						CASE WHEN M.SubGroupID = 1 THEN ISV3.SegmentValue ELSE ISV5.SegmentValue END, 
                        CASE WHEN M.SubGroupID = 1 THEN ISV4.SegmentValue ELSE '' END, 
                        CASE WHEN M.SubGroupID = 1 THEN ISV6.SegmentValue ELSE '' END, 
                        CASE WHEN M.SubGroupID = 1 THEN ISV7.SegmentValue ELSE '' END,
                        CASE WHEN M.SubGroupID = 1 THEN 0 ELSE CONVERT(decimal(18,2),ISV3.SegmentValue) END,
                        CASE WHEN M.SubGroupID = 1 THEN 0 ELSE CONVERT(decimal(18,2),ISV4.SegmentValue) END,
						CASE WHEN M.SubGroupID <> 1 THEN ISV3.SegmentValue + ' X ' + ISV4.SegmentValue ELSE '' END,
                        YBC.ItemMasterID, YBCB.ItemMasterID
                    ),
					FinalResult As(
					SELECT F.* FROM F 
					INNER JOIN TotalKPDone KPD ON KPD.ConceptID = F.ConceptID
					WHERE F.ProduceKnittingQty >= KPD.RemainingPlanQty AND YBCItemMasterID = Case When Isnull(YBCBItemMasterID,0) = 0 Then  YBCItemMasterID Else YBCBItemMasterID End
					)
                    Select *, Count(*) Over() TotalRows  From FinalResult ";
                        orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "ORDER BY ConceptID DESC" : paginationInfo.OrderBy;
                    }
                    else
                    {
                        sql = $@";WITH
                                M As 
                                (
	                                Select ConceptNo = G.GroupConceptNo, ConceptDate = G.DateAdded, KP.PlanNo, KP.Active, SUM(ISNULL(KPC.BookingQty,0)) PlanQty, 
                                    KP.RevisionNo, KP.BuyerID, KP.BuyerTeamID, KP.ExportOrderID, KPC.SubGroupID,
                                    SubGroupName = CASE WHEN KPC.SubGroupID = 1 THEN 'Fabric' WHEN KPC.SubGroupID = 11 THEN 'Collar' ELSE 'CUFF' END
	                                FROM {TableNames.Knitting_Plan_Group} G
	                                LEFT JOIN {TableNames.Knitting_Plan_Master} KP ON KP.PlanNo = G.GroupID
	                                LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPMasterID = KP.KPMasterID
                                    Where KP.Active = 1 And IsBDS = 2
	                                GROUP BY G.GroupConceptNo, G.DateAdded,  KP.PlanNo, KP.Active, KP.RevisionNo, 
	                                KP.BuyerID, KP.BuyerTeamID, KP.ExportOrderID, KPC.SubGroupID
                                )
                                Select *, Count(*) Over() TotalRows From M ";
                        orderBy = paginationInfo.OrderBy.NullOrEmpty() ? " ORDER BY ConceptDate DESC, ConceptNo" : paginationInfo.OrderBy;
                    }
                }
                else if (status == Status.InActive)
                {
                    sql = $@"
                    ;WITH
                    M As 
                    (
	                   Select KP.KPMasterID, KP.ConceptID, KP.PreProcessRevNo, KP.RevisionPending, KP.Active, SUM(ISNULL(KPC.BookingQty,0)) PlanQty, 
                        KP.PlanNo, KP.RevisionNo, KP.BuyerID, KP.BuyerTeamID, KP.ExportOrderID, KPC.SubGroupID,
	                    Contact = CASE WHEN ISNULL(KJC.IsSubContact,0)=1 THEN C.ShortName  ELSE CC.UnitName END
	                    FROM {TableNames.Knitting_Plan_Master} KP
	                    LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPMasterID = KP.KPMasterID
	                    LEFT JOIN {TableNames.KNITTING_JOB_CARD_Master} KJC ON KJC.GroupID = KPC.PlanNo AND KJC.ConceptID = KP.ConceptID AND KJC.GroupID NOT IN (1,0)
	                    LEFT JOIN {TableNames.KNITTING_UNIT} CC ON CC.KnittingUnitID = KJC.ContactID
	                    LEFT JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = KJC.ContactID
                        Where KP.Active = 1 And IsBDS = 2
	                    GROUP BY KP.KPMasterID, KP.ConceptID, KP.PreProcessRevNo, KP.RevisionPending, KP.Active, KP.PlanNo, KP.RevisionNo, 
	                    KP.BuyerID, KP.BuyerTeamID, KP.ExportOrderID, KPC.SubGroupID,ISNULL(KJC.IsSubContact,0),C.ShortName,CC.UnitName
                    ),
                    TotalKPDone AS
		            (
			            SELECT M.ConceptID, RemainingPlanQty = SUM(M.PlanQty)
			            FROM M
			            GROUP BY M.ConceptID
		            ),
                    F AS 
                    (
                        SELECT KPMasterID, M.RevisionPending, M.ConceptID, FCM.ConceptNo, FCM.ConceptDate, FCM.Qty, FCM.Remarks, 
						M.PlanQty, M.PlanNo, 
						Technical.TechnicalName, 
						F.ValueName ConceptForName, S.ValueName ConceptStatus, M.Active, E.EmployeeName UserName, ISG.SubGroupName, 
						M.BuyerID, M.BuyerTeamID, M.ExportOrderID, 
                        Buyer = CASE WHEN M.BuyerID > 0 THEN ISNULL(CTO.ShortName,'') ELSE 'R&D' END, 
                        BuyerTeam = CASE WHEN M.BuyerTeamID > 0 THEN ISNULL(CCT.TeamName,'') ELSE 'R&D' END,
                        FCM.GroupConceptNo, BookingQty = FCM.Qty, M.Contact,
                        -------------------------
                        ProduceKnittingQty = 
						CASE WHEN FCM.IsBDS = 2 THEN 
						ISNULL(
						Case When YBC.BookingUnitID =28 
						Then FBC.GreyProdQty 
						Else Case When isnull(YBC.QtyInKG,0)>0 Then Round((isnull(YBC.BookingQty,0)/isnull(YBC.QtyInKG,0))*isnull(YBC.GreyProdQty,0),0) Else 0 END 
						END
						,0) 
						ELSE FCM.ProduceKnittingQty END,
						-------------------------
                        RemainingPlanQty = ISNULL(CASE WHEN FCM.IsBDS = 2 THEN 
						ISNULL(
						Case When YBC.BookingUnitID =28 
						Then FBC.GreyProdQty 
						Else Case When isnull(YBC.QtyInKG,0)>0 Then Round((isnull(YBC.BookingQty,0)/isnull(YBC.QtyInKG,0))*isnull(YBC.GreyProdQty,0),0) Else 0 END 
						END
						,0) 
						ELSE FCM.ProduceKnittingQty END,0) 
						- ISNULL(KPD.RemainingPlanQty,0), 
		                -------------------------
		                Uom = CASE WHEN FCM.SubGroupId = 1 THEN 'Kg' ELSE 'Pcs' END,

						Construction = ISV1.SegmentValue, 
                        Composition = CASE WHEN M.SubGroupID = 1 THEN ISV2.SegmentValue ELSE '' END,
						ColorName = CASE WHEN M.SubGroupID = 1 THEN ISV3.SegmentValue ELSE ISV5.SegmentValue END, 
                        GSM = CASE WHEN M.SubGroupID = 1 THEN ISV4.SegmentValue ELSE '' END, 
                        DyeingType = CASE WHEN M.SubGroupID = 1 THEN ISV6.SegmentValue ELSE '' END, 
                        KnittingType = CASE WHEN M.SubGroupID = 1 THEN ISV7.SegmentValue ELSE '' END,
                        Length = CASE WHEN M.SubGroupID = 1 THEN 0 ELSE CONVERT(decimal(18,2),ISV3.SegmentValue) END,
                        Width = CASE WHEN M.SubGroupID = 1 THEN 0 ELSE CONVERT(decimal(18,2),ISV4.SegmentValue) END,
						Size = CASE WHEN M.SubGroupID <> 1 THEN ISV3.SegmentValue + ' X ' + ISV4.SegmentValue ELSE '' END,
                        YBCItemMasterID = YBC.ItemMasterID, YBCBItemMasterID = YBCB.ItemMasterID

                        FROM M
                        INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = M.ConceptID
					    INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_MASTER} MR ON MR.ConceptID = M.ConceptID
                        LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FBC ON FBC.BookingChildID = FCM.BookingChildID AND FBC.BookingID = FCM.BookingID AND FBC.BookingQty = 0
                        LEFT JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.BookingChildID = FBC.BookingChildID
						LEFT JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID = YBC.YBookingID
						LEFT JOIN {TableNames.YarnBookingChild_New_Bk} YBCB ON YBCB.YBChildID = YBC.YBChildID AND YBCB.RevisionNo = YBM.RevisionNo - 1
                        LEFT JOIN TotalKPDone KPD ON KPD.ConceptID = FCM.ConceptID
                        LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME} Technical ON Technical.TechnicalNameId=FCM.TechnicalNameId
                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue F ON F.ValueID = FCM.ConceptFor
                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue S ON S.ValueID = FCM.ConceptStatusID
                        LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG On FCM.SubGroupID = ISG.SubGroupID
                        LEFT JOIN {DbNames.EPYSL}..LoginUser L ON L.UserCode = FCM.AddedBy
                        LEFT Join {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
						LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = M.BuyerID
						LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = M.BuyerTeamID

						LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = FCM.ItemMasterID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
					    WHERE --MR.RevisionNo = M.PreProcessRevNo AND 
                        FCM.IsBDS = 2
                    ),
					FinalResult As(
					SELECT F.* FROM F 
					INNER JOIN TotalKPDone KPD ON KPD.ConceptID = F.ConceptID
					WHERE F.ProduceKnittingQty >= KPD.RemainingPlanQty AND YBCItemMasterID = Case When Isnull(YBCBItemMasterID,0) = 0 Then  YBCItemMasterID Else YBCBItemMasterID End
					)
                    Select *, Count(*) Over() TotalRows  From FinalResult ";
                    orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "ORDER BY ConceptID DESC" : paginationInfo.OrderBy;

                    /*sql = $@"
                    ;WITH
                    M As 
                    (
	                    Select KP.KPMasterID, KP.ConceptID, KP.RevisionPending, KP.Active, SUM(ISNULL(KPC.BookingQty,0)) PlanQty, KP.PlanNo,
						KP.BuyerID, KP.BuyerTeamID, KP.ExportOrderID, KPC.SubGroupID
	                    FROM {TableNames.Knitting_Plan_Master} KP
	                    LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPMasterID = KP.KPMasterID
                        Where KP.Active = 0 And IsBDS = 2
	                    GROUP BY KP.KPMasterID, KP.ConceptID, KP.RevisionPending, KP.Active, KP.PlanNo, KP.BuyerID, 
                        KP.BuyerTeamID, KP.ExportOrderID, KPC.SubGroupID
                    ), 
                    L As 
                    (
                        SELECT KPMasterID, M.RevisionPending, M.ConceptID, FCM.ConceptNo, FCM.ConceptDate, FCM.Qty, FCM.Remarks, 
                        M.PlanQty, M.PlanNo,
                        Technical.TechnicalName, 
		                F.ValueName ConceptForName, 
                        S.ValueName ConceptStatus, M.Active,E.EmployeeName UserName, ISG.SubGroupName, M.BuyerID, M.BuyerTeamID, M.ExportOrderID, 
		                Buyer = CASE WHEN M.BuyerID > 0 THEN ISNULL(CTO.ShortName,'') ELSE 'R&D' END, 
                        BuyerTeam = CASE WHEN M.BuyerTeamID > 0 THEN ISNULL(CCT.TeamName,'') ELSE 'R&D' END,
                        FCM.GroupConceptNo, Uom = CASE WHEN FCM.SubGroupId = 1 THEN 'Kg' ELSE 'Pcs' END,
		                Construction = ISV1.SegmentValue, 
                        Composition = CASE WHEN M.SubGroupID = 1 THEN ISV2.SegmentValue ELSE '' END,
		                ColorName = CASE WHEN M.SubGroupID = 1 THEN ISV3.SegmentValue ELSE ISV5.SegmentValue END, 
                        GSM = CASE WHEN M.SubGroupID = 1 THEN ISV4.SegmentValue ELSE '' END, 
                        DyeingType = CASE WHEN M.SubGroupID = 1 THEN ISV6.SegmentValue ELSE '' END, 
                        KnittingType = CASE WHEN M.SubGroupID = 1 THEN ISV7.SegmentValue ELSE '' END,
                        Length = CASE WHEN M.SubGroupID = 1 THEN 0 ELSE CONVERT(decimal(18,2),ISV3.SegmentValue) END,
                        Width = CASE WHEN M.SubGroupID = 1 THEN 0 ELSE CONVERT(decimal(18,2),ISV4.SegmentValue) END,
		                Size = CASE WHEN M.SubGroupID <> 1 THEN ISV3.SegmentValue + ' X ' + ISV4.SegmentValue ELSE '' END
                        FROM M
                        INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = M.ConceptID
                        LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID = FCM.KnittingTypeID
                        LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME} Technical ON Technical.TechnicalNameId=FCM.TechnicalNameId
                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue F ON F.ValueID = FCM.ConceptFor
                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue S ON S.ValueID = FCM.ConceptStatusID
                        LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG On FCM.SubGroupID = ISG.SubGroupID
                        LEFT JOIN {DbNames.EPYSL}..LoginUser L ON L.UserCode = FCM.AddedBy
                        LEFT Join {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
		                LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = M.BuyerID
		                LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = M.BuyerTeamID

		                LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = FCM.ItemMasterID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                        WHERE FCM.IsBDS = 2
                    )
                    Select *, Count(*) Over() TotalRows From L";
                    orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "ORDER BY ConceptID DESC" : paginationInfo.OrderBy;*/
                }
                else
                {
                    sql = $@"
                    ;WITH
                    M As 
                    (
	                    Select KP.KPMasterID, KP.ConceptID, KP.RevisionPending, KP.Active, SUM(ISNULL(KPC.BookingQty,0)) PlanQty, KP.PlanNo, 
	                    KP.BuyerID, KP.BuyerTeamID, KP.ExportOrderID, KPC.SubGroupID
	                    FROM {TableNames.Knitting_Plan_Master} KP
	                    LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPMasterID = KP.KPMasterID
                        Where KP.IsBDS = 2
	                    GROUP BY KP.KPMasterID, KP.ConceptID, KP.RevisionPending, KP.Active, KP.PlanNo, KP.BuyerID, 
                        KP.BuyerTeamID, KP.ExportOrderID, KPC.SubGroupID
                    ), 
                    L As 
                    (
                        SELECT KPMasterID, M.RevisionPending, M.ConceptID, FCM.ConceptNo, FCM.ConceptDate, FCM.Remarks,
	                    BookingQty = CASE WHEN FCM.IsBDS = 2 THEN FCM.Qty ELSE 0 END,
	                    Qty = CASE WHEN FCM.IsBDS = 2 THEN FBC.GreyProdQty ELSE 0 END,

                        M.PlanQty, M.PlanNo,
                        Technical.TechnicalName, 
	                    F.ValueName ConceptForName, 
                        S.ValueName ConceptStatus, M.Active, ISG.SubGroupName, M.BuyerID, M.BuyerTeamID, M.ExportOrderID, 
                        Buyer = CASE WHEN M.BuyerID > 0 THEN ISNULL(CTO.ShortName,'') ELSE 'R&D' END, 
                        BuyerTeam = CASE WHEN M.BuyerTeamID > 0 THEN ISNULL(CCT.TeamName,'') ELSE 'R&D' END, 
                        FCM.GroupConceptNo, Uom = CASE WHEN FCM.SubGroupId = 1 THEN 'Kg' ELSE 'Pcs' END,
	                    Construction = ISV1.SegmentValue, 
                        Composition = CASE WHEN M.SubGroupID = 1 THEN ISV2.SegmentValue ELSE '' END,
	                    ColorName = CASE WHEN M.SubGroupID = 1 THEN ISV3.SegmentValue ELSE ISV5.SegmentValue END, 
                        GSM = CASE WHEN M.SubGroupID = 1 THEN ISV4.SegmentValue ELSE '' END, 
                        DyeingType = CASE WHEN M.SubGroupID = 1 THEN ISV6.SegmentValue ELSE '' END, 
                        KnittingType = CASE WHEN M.SubGroupID = 1 THEN ISV7.SegmentValue ELSE '' END,
                        Length = CASE WHEN M.SubGroupID = 1 THEN 0 ELSE CONVERT(decimal(18,2),ISV3.SegmentValue) END,
                        Width = CASE WHEN M.SubGroupID = 1 THEN 0 ELSE CONVERT(decimal(18,2),ISV4.SegmentValue) END,
	                    Size = CASE WHEN M.SubGroupID <> 1 THEN ISV3.SegmentValue + ' X ' + ISV4.SegmentValue ELSE '' END
                        FROM M
                        INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = M.ConceptID
	                    LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FBC ON FBC.BookingChildID = FCM.BookingChildID AND FBC.BookingID = FCM.BookingID
                        LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID = FCM.KnittingTypeID
                        LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME} Technical ON Technical.TechnicalNameId=FCM.TechnicalNameId
                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue F ON F.ValueID = FCM.ConceptFor
                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue S ON S.ValueID = FCM.ConceptStatusID
                        LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG On FCM.SubGroupID = ISG.SubGroupID
	                    LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = M.BuyerID
	                    LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = M.BuyerTeamID

	                    LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = FCM.ItemMasterID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                        WHERE FCM.IsBDS = 2
                    )
                    Select *, Count(*) Over() TotalRows From L";

                    orderBy = paginationInfo.OrderBy.NullOrEmpty() ? " ORDER BY ConceptID DESC" : paginationInfo.OrderBy;
                }
            }

            #endregion Knitting Program for Bulk

            #region Knitting Program From BDS

            else
            {
                if (status == Status.PendingGroup)
                {
                    sql = $@"
                    ;WITH 
                    {sqlBuyerPermission}
                    KPMasters AS 
                    (
	                    SELECT KPM.KPMasterID, KPM.ConceptID 
	                    FROM {TableNames.Knitting_Plan_Master} KPM
                        INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = KPM.ConceptID
	                    WHERE FCM.IsBDS = 0
	                    GROUP BY KPM.KPMasterID, KPM.ConceptID
                    ),
                    M AS 
                    (
                        SELECT FCM.ConceptID, FCM.ConceptNo, FCM.GroupConceptNo, FCM.ConceptDate, FCM.Qty, FCM.Remarks, FCM.KnittingTypeID, FCM.CompositionID, FCM.GSMID,
                        FCM.ConstructionID, FCM.TechnicalNameId, FCM.ConceptFor, FCM.ConceptStatusID, RevisionPending = Case When KP.ConceptID IS NULL Then 0 Else 1 End, FCM.SubGroupID, FCM.BookingID, BM.BuyerID, 
                        BM.BuyerTeamID, FCM.TotalQty, FCM.ItemMasterID, ISNULL(KP.KPMasterID,0) KPMasterID
                        FROM {TableNames.RND_FREE_CONCEPT_MR_MASTER} FCMRM
                        INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = FCMRM.ConceptID
					    LEFT JOIN {TableNames.Knitting_Plan_Master} KP ON KP.ConceptID = FCM.ConceptID
						LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster BM ON BM.BookingID = FCM.BookingID
                        LEFT JOIN KPMasters ON KP.ConceptID = FCMRM.ConceptID
                        WHERE FCM.IsBDS = 0 AND (KP.ConceptID IS NULL or KP.PreProcessRevNo <> FCMRM.RevisionNo)
                        AND KPMasters.KPMasterID IS NULL
					    GROUP BY FCM.ConceptID, FCM.ConceptNo, FCM.GroupConceptNo, FCM.ConceptDate, FCM.Qty, FCM.Remarks, FCM.KnittingTypeID, FCM.CompositionID, FCM.GSMID,
                        FCM.ConstructionID, FCM.TechnicalNameId, FCM.ConceptFor, FCM.ConceptStatusID, KP.ConceptID, FCM.SubGroupID, FCM.BookingID, BM.BuyerID, BM.BuyerTeamID, FCM.TotalQty, FCM.ItemMasterID, ISNULL(KP.KPMasterID,0)
                    ), 
                    L As 
                    (
                        SELECT M.ConceptID, M.ConceptNo, M.GroupConceptNo, M.ConceptDate, M.Qty, M.Remarks, 0 PlanQty, KnittingType.TypeName KnittingType, Composition.SegmentValue Composition,
                        Construction.SegmentValue Construction, Technical.TechnicalName, GSM = Case When M.SubGroupID = 1 Then Gsm.SegmentValue Else '' End, 
						ColorName = Case When M.SubGroupID = 1 Then Color.SegmentValue Else C1.SegmentValue End, Size = Case When M.SubGroupID <> 1 Then Color.SegmentValue + ' X ' + Gsm.SegmentValue Else '' End, F.ValueName ConceptForName, M.RevisionPending, S.ValueName ConceptStatus,
                        ISG.SubGroupName, M.BookingID, M.BuyerID, M.BuyerTeamID, 
                        Buyer = CASE WHEN M.BuyerID > 0 THEN ISNULL(CTO.ShortName,'') ELSE 'R&D' END, 
                        BuyerTeam = CASE WHEN M.BuyerTeamID > 0 THEN ISNULL(CCT.TeamName,'') ELSE 'R&D' END,
                        M.TotalQty, M.KPMasterID
                        FROM M
                        {sqlBuyerPerInnerJoin}
                        LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID=M.KnittingTypeID
                        LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = M.ItemMasterID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = M.CompositionID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = M.ConstructionID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = IM.Segment3ValueID
                        LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME} Technical ON Technical.TechnicalNameId=M.TechnicalNameId
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = IM.Segment4ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue C1 ON C1.SegmentValueID = IM.Segment5ValueID
                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue F ON F.ValueID = M.ConceptFor
                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue S ON S.ValueID = M.ConceptStatusID
                        LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG On M.SubGroupID = ISG.SubGroupID
						LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = M.BuyerID
						LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = M.BuyerTeamID
                    )
                    Select *, Count(*) Over() TotalRows From L";

                    orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "ORDER BY ConceptID DESC" : paginationInfo.OrderBy;
                }
                else if (status == Status.Pending)
                {
                    sql = $@"
                    ;WITH 
                    {sqlBuyerPermission}
                    M AS 
                    (
                        SELECT FCM.ConceptID, FCM.ConceptNo, FCM.GroupConceptNo, FCM.ConceptDate, FCM.Qty, FCM.Remarks, FCM.KnittingTypeID, FCM.CompositionID, FCM.GSMID,
                        FCM.ConstructionID, FCM.TechnicalNameId, FCM.ConceptFor, FCM.ConceptStatusID, RevisionPending = Case When KP.ConceptID IS NULL Then 0 Else 1 End, FCM.SubGroupID, FCM.BookingID, BM.BuyerID, 
                        BM.BuyerTeamID, FCM.TotalQty, FCM.ItemMasterID, ISNULL(KP.KPMasterID,0) KPMasterID
                        FROM {TableNames.RND_FREE_CONCEPT_MR_MASTER} FCMRM
                        INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = FCMRM.ConceptID
					    LEFT JOIN {TableNames.Knitting_Plan_Master} KP ON KP.ConceptID = FCM.ConceptID
						LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster BM ON BM.BookingID = FCM.BookingID
                        WHERE FCM.IsBDS = 1 AND (KP.ConceptID IS NULL or (KP.PreProcessRevNo <> FCMRM.RevisionNo AND FCMRM.IsNeedRevision = 1))
					    GROUP BY FCM.ConceptID, FCM.ConceptNo, FCM.GroupConceptNo, FCM.ConceptDate, FCM.Qty, FCM.Remarks, FCM.KnittingTypeID, FCM.CompositionID, FCM.GSMID, 
                        FCM.ConstructionID, FCM.TechnicalNameId, FCM.ConceptFor, FCM.ConceptStatusID, KP.ConceptID, FCM.SubGroupID, FCM.BookingID, BM.BuyerID, BM.BuyerTeamID, FCM.TotalQty, FCM.ItemMasterID, ISNULL(KP.KPMasterID,0)
                    ), 
                    L As 
                    (
                        SELECT M.ConceptID, M.ConceptNo, M.GroupConceptNo, M.ConceptDate, M.Qty, M.Remarks, 0 PlanQty, KnittingType.TypeName KnittingType, Composition.SegmentValue Composition,
                        Construction.SegmentValue Construction, Technical.TechnicalName, GSM = Case When M.SubGroupID = 1 Then Gsm.SegmentValue Else '' End, 
						ColorName = Case When M.SubGroupID = 1 Then Color.SegmentValue Else C1.SegmentValue End, Size = Case When M.SubGroupID <> 1 Then Color.SegmentValue + ' X ' + Gsm.SegmentValue Else '' End, F.ValueName ConceptForName, M.RevisionPending, S.ValueName ConceptStatus,
                        ISG.SubGroupName, M.BookingID, M.BuyerID, M.BuyerTeamID, 
                        Buyer = CASE WHEN M.BuyerID > 0 THEN ISNULL(CTO.ShortName,'') ELSE 'R&D' END, 
                        BuyerTeam = CASE WHEN M.BuyerTeamID > 0 THEN ISNULL(CCT.TeamName,'') ELSE 'R&D' END,
                        M.TotalQty, M.KPMasterID
                        FROM M
                        {sqlBuyerPerInnerJoin}
                        LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID=M.KnittingTypeID
                        LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = M.ItemMasterID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = M.CompositionID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = M.ConstructionID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = IM.Segment3ValueID
                        LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME} Technical ON Technical.TechnicalNameId=M.TechnicalNameId
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = IM.Segment4ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue C1 ON C1.SegmentValueID = IM.Segment5ValueID
                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue F ON F.ValueID = M.ConceptFor
                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue S ON S.ValueID = M.ConceptStatusID
                        LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG On M.SubGroupID = ISG.SubGroupID
						LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = M.BuyerID
						LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = M.BuyerTeamID
                    ) 
                    SELECT * INTO #TempData{tempGuid} FROM L
                    Select *, Count(*) Over() TotalRows From #TempData{tempGuid}";

                    orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "ORDER BY ConceptID DESC" : paginationInfo.OrderBy;

                    isTempGuidUsed = true;
                }
                else if (status == Status.Active)
                {
                    if (isNew)
                    {
                        sql = $@" 
                        ;With KPM AS (
                            Select KPM.PlanNo, KPM.Active
                            From {TableNames.Knitting_Plan_Master} KPM
                            INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_MASTER} FCMR ON FCMR.ConceptID = KPM.ConceptID
                            Where KPM.Active = 1 AND KPM.PreProcessRevNo = FCMR.RevisionNo
                            Group By KPM.PlanNo, KPM.Active--, FCM.SubGroupID, ISV3.SegmentValue, ISV5.SegmentValue
                        ), 
                        /*AllColors AS
                        (
	                        SELECT GroupID = KPM.PlanNo, FCM.GroupConceptNo, ColorName = CASE WHEN FCM.SubGroupID = 1 THEN ISV3.SegmentValue ELSE ISV5.SegmentValue END 
	                        FROM {TableNames.RND_FREE_CONCEPT_MASTER} FCM
	                        INNER JOIN {TableNames.Knitting_Plan_Master} KPM1 ON KPM1.ConceptID = FCM.ConceptID
	                        INNER JOIN KPM ON KPM.PlanNo = KPM1.PlanNo
	                        INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = FCM.ItemMasterID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                        ),
                        ColorComma AS
                        (
                            SELECT GroupID, GroupConceptNo, ColorName = 
	                        STUFF((SELECT DISTINCT ', ' + ColorName
	                                FROM AllColors b 
	                                WHERE b.GroupID = a.GroupID AND b.GroupConceptNo = a.GroupConceptNo
	                                FOR XML PATH('')), 1, 2, '')
	                        FROM AllColors a
	                        GROUP BY GroupID, GroupConceptNo
                        ),
                        */
                        FinalList AS(
                            Select ConceptNo = KPG.GroupConceptNo, KPG.PlanQty Qty, KPG.PlanQty, PlanNo = KPG.GroupID, '' ConceptForName, NULL ConceptStatus, KPG.GroupConceptNo,
                            KPM.Active, E.EmployeeName UserName, ISG.SubGroupName, KPG.BuyerID, KPG.BuyerTeamID,
                            Buyer = CASE WHEN KPG.BuyerID > 0 THEN ISNULL(CTO.ShortName,'') ELSE 'R&D' END, 
                            BuyerTeam = CASE WHEN KPG.BuyerTeamID > 0 THEN ISNULL(CCT.TeamName,'') ELSE 'R&D' END,
                            KPG.DateAdded,
                            KPG.TotalQty--, CC.ColorName
                            From {TableNames.Knitting_Plan_Group} KPG
                            Inner Join KPM On KPM.PlanNo = KPG.GroupID
                            LEFT JOIN {DbNames.EPYSL}..LoginUser L ON L.UserCode = KPG.AddedBy
                            LEFT Join {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
                            LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = KPG.SubGroupID
                            LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = KPG.BuyerID
                            LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = KPG.BuyerTeamID
	                        --LEFT JOIN ColorComma CC ON CC.GroupID = KPG.GroupID AND CC.GroupConceptNo = KPG.GroupConceptNo
                        )

                        Select *, Count(*) Over() TotalRows From FinalList";

                        orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "ORDER BY PlanNo DESC" : paginationInfo.OrderBy;

                    }
                    else
                    {
                        sql = $@" 
                        ;WITH
                        {sqlBuyerPermission}
                        F AS 
                        (
                            SELECT M.KPMasterID, M.RevisionPending, M.ConceptID, FCM.ConceptNo, FCM.ConceptDate, FCM.Qty, FCM.Remarks, 
	                        PlanQty = SUM(ISNULL(KPC.BookingQty,0)), M.PlanNo
	                        , KnittingType.TypeName KnittingType,Composition.SegmentValue Composition, Construction.SegmentValue Construction
	                        ,Technical.TechnicalName, GSM = Case When FCM.SubGroupID = 1 Then Gsm.SegmentValue Else '' End, 
	                        ColorName = Case When FCM.SubGroupID = 1 Then Color.SegmentValue Else C1.SegmentValue End, Size = Case When FCM.SubGroupID <> 1 Then Color.SegmentValue + ' X ' + Gsm.SegmentValue Else '' End, F.ValueName ConceptForName, S.ValueName ConceptStatus, M.Active,E.EmployeeName UserName,
	                        ISG.SubGroupName, M.BuyerID, M.BuyerTeamID,M.DateAdded,
                            Buyer = CASE WHEN M.BuyerID > 0 THEN ISNULL(CTO.ShortName,'') ELSE 'R&D' END, 
                            BuyerTeam = CASE WHEN M.BuyerTeamID > 0 THEN ISNULL(CCT.TeamName,'') ELSE 'R&D' END, 
                            FCM.TotalQty, FCM.GroupConceptNo, M.RevisionNo, 
	                        M.RevisionDate
                            FROM {TableNames.Knitting_Plan_Master} M
                            INNER JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPMasterID = M.KPMasterID
                            {sqlBuyerPerInnerJoin}
                            INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = M.ConceptID
					        INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_MASTER} MR ON MR.ConceptID = M.ConceptID
                            LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = FCM.ItemMasterID
                            LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID = FCM.KnittingTypeID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = FCM.CompositionID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = FCM.ConstructionID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = IM.Segment3ValueID
                            LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME} Technical ON Technical.TechnicalNameId=FCM.TechnicalNameId
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = IM.Segment4ValueID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue C1 ON C1.SegmentValueID = IM.Segment5ValueID
                            LEFT JOIN {DbNames.EPYSL}..EntityTypeValue F ON F.ValueID = FCM.ConceptFor
                            LEFT JOIN {DbNames.EPYSL}..EntityTypeValue S ON S.ValueID = FCM.ConceptStatusID
                            LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG On FCM.SubGroupID = ISG.SubGroupID
                            LEFT JOIN {DbNames.EPYSL}..LoginUser L ON L.UserCode = FCM.AddedBy
                            LEFT Join {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
						    LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = M.BuyerID
						    LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = M.BuyerTeamID
					        WHERE M.Active = 1 And M.IsBDS = 1 AND MR.RevisionNo = M.PreProcessRevNo
	                        GROUP BY M.KPMasterID, M.RevisionPending, M.ConceptID, FCM.ConceptNo, FCM.ConceptDate, FCM.Qty, FCM.Remarks,M.PlanNo
	                        ,KnittingType.TypeName,Composition.SegmentValue, Construction.SegmentValue
	                        ,Technical.TechnicalName, Case When FCM.SubGroupID = 1 Then Gsm.SegmentValue Else '' End, 
	                        Case When FCM.SubGroupID = 1 Then Color.SegmentValue Else C1.SegmentValue End, Case When FCM.SubGroupID <> 1 Then Color.SegmentValue + ' X ' + Gsm.SegmentValue Else '' End, F.ValueName, S.ValueName, M.Active,E.EmployeeName,
	                        ISG.SubGroupName, M.BuyerID, M.BuyerTeamID,M.DateAdded,
                            CASE WHEN M.BuyerID > 0 THEN ISNULL(CTO.ShortName,'') ELSE 'R&D' END, 
                            CASE WHEN M.BuyerTeamID > 0 THEN ISNULL(CCT.TeamName,'') ELSE 'R&D' END, 
                            FCM.TotalQty, FCM.GroupConceptNo, M.RevisionNo, 
	                        M.RevisionDate
                        ), 
                        L As 
                        (
					        SELECT F.KPMasterID, F.RevisionPending, F.ConceptID, F.ConceptNo, F.ConceptDate, F.Qty, F.Remarks, F.PlanQty, 
					        F.PlanNo, F.KnittingType, F.Composition, F.Construction, F.TechnicalName, F.Gsm, F.ColorName, F.Size, F.ConceptForName, F.ConceptStatus, 
					        F.Active, F.UserName, F.SubGroupName, F.BuyerID, F.BuyerTeamID, F.Buyer, F.BuyerTeam, F.TotalQty, F.GroupConceptNo,F.DateAdded, F.RevisionNo, F.RevisionDate
					        FROM F
					        GROUP BY F.KPMasterID, F.RevisionPending, F.ConceptID, F.ConceptNo, F.ConceptDate, F.Qty, F.Remarks, F.PlanQty, 
					        F.PlanNo, F.KnittingType, F.Composition, F.Construction, F.TechnicalName, F.Gsm, F.ColorName, F.Size, F.ConceptForName, F.ConceptStatus, 
					        F.Active, F.UserName, F.SubGroupName, F.BuyerID, F.BuyerTeamID, F.Buyer, F.BuyerTeam, F.TotalQty, F.GroupConceptNo,F.DateAdded, F.RevisionNo, F.RevisionDate
                        ) 
                        SELECT * INTO #TempData{tempGuid} FROM L
                        Select *, Count(*) Over() TotalRows From #TempData{tempGuid}";

                        orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "ORDER BY ConceptID DESC" : paginationInfo.OrderBy;
                        isTempGuidUsed = true;

                    }

                }
                else if (status == Status.InActive)
                {
                    if (isNew)
                    {
                        sql = $@" 
                        ;With KPM AS (
                            Select KPM.PlanNo, KPM.Active
                            From {TableNames.Knitting_Plan_Master} KPM
                            INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_MASTER} FCMR ON FCMR.ConceptID = KPM.ConceptID
                            Where KPM.Active = 0 AND KPM.PreProcessRevNo = FCMR.RevisionNo
                            Group By KPM.PlanNo, KPM.Active--, FCM.SubGroupID, ISV3.SegmentValue, ISV5.SegmentValue
                        ), 
                        /*AllColors AS
                        (
	                        SELECT GroupID = KPM.PlanNo, FCM.GroupConceptNo, ColorName = CASE WHEN FCM.SubGroupID = 1 THEN ISV3.SegmentValue ELSE ISV5.SegmentValue END 
	                        FROM {TableNames.RND_FREE_CONCEPT_MASTER} FCM
	                        INNER JOIN {TableNames.Knitting_Plan_Master} KPM1 ON KPM1.ConceptID = FCM.ConceptID
	                        INNER JOIN KPM ON KPM.PlanNo = KPM1.PlanNo
	                        INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = FCM.ItemMasterID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                        ),
                        ColorComma AS
                        (
                            SELECT GroupID, GroupConceptNo, ColorName = 
	                        STUFF((SELECT DISTINCT ', ' + ColorName
	                                FROM AllColors b 
	                                WHERE b.GroupID = a.GroupID AND b.GroupConceptNo = a.GroupConceptNo
	                                FOR XML PATH('')), 1, 2, '')
	                        FROM AllColors a
	                        GROUP BY GroupID, GroupConceptNo
                        ),*/
                        FinalList AS(
                            Select ConceptNo = KPG.GroupConceptNo, KPG.PlanQty Qty, KPG.PlanQty, PlanNo = KPG.GroupID, '' ConceptForName, NULL ConceptStatus, KPG.GroupConceptNo,
                            KPM.Active, E.EmployeeName UserName, ISG.SubGroupName, KPG.BuyerID, KPG.BuyerTeamID,
                            Buyer = CASE WHEN KPG.BuyerID > 0 THEN ISNULL(CTO.ShortName,'') ELSE 'R&D' END, 
                            BuyerTeam = CASE WHEN KPG.BuyerTeamID > 0 THEN ISNULL(CCT.TeamName,'') ELSE 'R&D' END,
                            KPG.TotalQty--, CC.ColorName
                            From {TableNames.Knitting_Plan_Group} KPG
                            Inner Join KPM On KPM.PlanNo = KPG.GroupID
                            LEFT JOIN {DbNames.EPYSL}..LoginUser L ON L.UserCode = KPG.AddedBy
                            LEFT Join {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
                            LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = KPG.SubGroupID
                            LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = KPG.BuyerID
                            LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = KPG.BuyerTeamID
	                        --LEFT JOIN ColorComma CC ON CC.GroupID = KPG.GroupID AND CC.GroupConceptNo = KPG.GroupConceptNo
                        )
                        Select *, Count(*) Over() TotalRows From FinalList";

                        orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "ORDER BY PlanNo DESC" : paginationInfo.OrderBy;
                    }
                    else
                    {
                        sql = $@" 
                        ;WITH
                        {sqlBuyerPermission}
                        M As 
                        (
	                        Select KP.KPMasterID, KP.ConceptID, KP.RevisionPending, KP.Active, SUM(ISNULL(KPC.BookingQty,0)) PlanQty, KP.PlanNo,
						    KP.BuyerID, KP.BuyerTeamID, KP.RevisionNo, KP.RevisionDate
	                        FROM {TableNames.Knitting_Plan_Master} KP
	                        LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPMasterID = KP.KPMasterID
                            Where KP.Active = 0 And KP.IsBDS = 1
	                        GROUP BY KP.KPMasterID, KP.ConceptID, KP.RevisionPending, KP.Active, KP.PlanNo, KP.BuyerID, KP.BuyerTeamID, KP.RevisionNo, KP.RevisionDate
                        ), 
                        L As 
                        (
                            SELECT KPMasterID, M.RevisionPending, M.ConceptID, FCM.ConceptNo, FCM.ConceptDate, FCM.Qty, FCM.Remarks, M.PlanQty, M.PlanNo
	                        , KnittingType.TypeName KnittingType,Composition.SegmentValue Composition, Construction.SegmentValue Construction
	                        , Technical.TechnicalName, GSM = Case When FCM.SubGroupID = 1 Then Gsm.SegmentValue Else '' End, 
						    ColorName = Case When FCM.SubGroupID = 1 Then Color.SegmentValue Else ISNULL(C1.SegmentValue,'') End, Size = Case When FCM.SubGroupID <> 1 Then Color.SegmentValue + ' X ' + Gsm.SegmentValue Else '' End, F.ValueName ConceptForName, S.ValueName ConceptStatus, M.Active,E.EmployeeName UserName,
						    ISG.SubGroupName, M.BuyerID, M.BuyerTeamID, 
                            Buyer = CASE WHEN M.BuyerID > 0 THEN ISNULL(CTO.ShortName,'') ELSE 'R&D' END, 
                            BuyerTeam = CASE WHEN M.BuyerTeamID > 0 THEN ISNULL(CCT.TeamName,'') ELSE 'R&D' END, 
                            FCM.TotalQty, FCM.GroupConceptNo, M.RevisionNo, M.RevisionDate
                            FROM M
                            {sqlBuyerPerInnerJoin}
                            INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = M.ConceptID
                            LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID = FCM.KnittingTypeID
                            LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = FCM.ItemMasterID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = FCM.CompositionID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = FCM.ConstructionID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = IM.Segment3ValueID
                            LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME} Technical ON Technical.TechnicalNameId=FCM.TechnicalNameId
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = IM.Segment4ValueID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue C1 ON C1.SegmentValueID = IM.Segment5ValueID
                            LEFT JOIN {DbNames.EPYSL}..EntityTypeValue F ON F.ValueID = FCM.ConceptFor
                            LEFT JOIN {DbNames.EPYSL}..EntityTypeValue S ON S.ValueID = FCM.ConceptStatusID
                            LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG On FCM.SubGroupID = ISG.SubGroupID
                            LEFT JOIN  {DbNames.EPYSL}..LoginUser L ON L.UserCode = FCM.AddedBy
                            LEFT Join {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
						    LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = M.BuyerID
						    LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = M.BuyerTeamID
                            WHERE FCM.IsBDS = 1
                            GROUP BY KPMasterID, M.RevisionPending, M.ConceptID, FCM.ConceptNo, FCM.ConceptDate, FCM.Qty, FCM.Remarks, M.PlanQty, M.PlanNo, M.RevisionNo, M.RevisionDate
	                        , KnittingType.TypeName,Composition.SegmentValue, Construction.SegmentValue, Technical.TechnicalName,Gsm.SegmentValue, Color.SegmentValue, F.ValueName, 
						    S.ValueName, M.Active, E.EmployeeName, ISG.SubGroupName, M.BuyerID, M.BuyerTeamID, ISNULL(CTO.ShortName,''), ISNULL(CCT.TeamName,''), FCM.TotalQty, FCM.SubGroupID, ISNULL(C1.SegmentValue,''), FCM.GroupConceptNo
                        )
                        SELECT * INTO #TempData{tempGuid} FROM L
                        Select *, Count(*) Over() TotalRows From #TempData{tempGuid} ";

                        orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "ORDER BY ConceptID DESC" : paginationInfo.OrderBy;
                        isTempGuidUsed = true;
                    }
                }
                else
                {
                    sql = $@"
                    ;WITH
                    {sqlBuyerPermission}
                    M As 
                    (
	                    Select KP.KPMasterID, KP.ConceptID, KP.RevisionPending, KP.Active, SUM(ISNULL(KPC.BookingQty,0)) PlanQty, KP.PlanNo,
						KP.BuyerID, KP.BuyerTeamID, KP.RevisionNo, KP.RevisionDate
	                    FROM {TableNames.Knitting_Plan_Master} KP 
	                    LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPMasterID = KP.KPMasterID
                        Where KP.IsBDS = 1
	                    GROUP BY KP.KPMasterID, KP.ConceptID, KP.RevisionPending, KP.Active, KP.PlanNo, KP.BuyerID, KP.BuyerTeamID, KP.RevisionNo, KP.RevisionDate
                    ), 
                    L As 
                    (
                        SELECT KPMasterID, M.RevisionPending, M.ConceptID, FCM.ConceptNo, FCM.ConceptDate, FCM.Qty, FCM.Remarks, M.PlanQty, M.PlanNo
	                    , KnittingType.TypeName KnittingType,Composition.SegmentValue Composition, Construction.SegmentValue Construction
	                    , Technical.TechnicalName, GSM = Case When FCM.SubGroupID = 1 Then Gsm.SegmentValue Else '' End, 
						ColorName = Case When FCM.SubGroupID = 1 Then Color.SegmentValue Else ISNULL(C1.SegmentValue,'') End, Size = Case When FCM.SubGroupID <> 1 Then Color.SegmentValue + ' X ' + Gsm.SegmentValue Else '' End, F.ValueName ConceptForName, S.ValueName ConceptStatus, M.Active,
						ISG.SubGroupName, M.BuyerID, M.BuyerTeamID, 
                        Buyer = CASE WHEN M.BuyerID > 0 THEN ISNULL(CTO.ShortName,'') ELSE 'R&D' END, 
                        BuyerTeam = CASE WHEN M.BuyerTeamID > 0 THEN ISNULL(CCT.TeamName,'') ELSE 'R&D' END, 
                        FCM.TotalQty, FCM.GroupConceptNo, M.RevisionNo, M.RevisionDate
                        FROM M
                        {sqlBuyerPerInnerJoin}
                        INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = M.ConceptID
                        LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID = FCM.KnittingTypeID
                        LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = FCM.ItemMasterID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = FCM.CompositionID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = FCM.ConstructionID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = IM.Segment3ValueID
                        LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME} Technical ON Technical.TechnicalNameId=FCM.TechnicalNameId
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = IM.Segment4ValueID
		                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue C1 ON C1.SegmentValueID = IM.Segment5ValueID
                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue F ON F.ValueID = FCM.ConceptFor
                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue S ON S.ValueID = FCM.ConceptStatusID
                        LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG On FCM.SubGroupID = ISG.SubGroupID
						LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = M.BuyerID
						LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = M.BuyerTeamID
                        WHERE FCM.IsBDS = 1
                        GROUP BY KPMasterID, M.RevisionPending, M.ConceptID, FCM.ConceptNo, FCM.ConceptDate, FCM.Qty, FCM.Remarks, M.PlanQty, M.PlanNo, M.RevisionNo, M.RevisionDate
	                    , KnittingType.TypeName,Composition.SegmentValue, Construction.SegmentValue, Technical.TechnicalName,Gsm.SegmentValue, Color.SegmentValue, F.ValueName, 
						S.ValueName, M.Active, ISG.SubGroupName, M.BuyerID, M.BuyerTeamID, ISNULL(CTO.ShortName,''), ISNULL(CCT.TeamName,''), FCM.TotalQty, FCM.SubGroupID, ISNULL(C1.SegmentValue,''), FCM.GroupConceptNo
                    ) 
                    Select *, Count(*) Over() TotalRows From L ";
                    orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "ORDER BY ConceptID DESC" : paginationInfo.OrderBy;
                }
            }

            #endregion Knitting Program From BDS

            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            if (isTempGuidUsed)
            {
                sql += $@" DROP TABLE #TempData{tempGuid}";
            }

            return await _service.GetDataAsync<KnittingPlanMaster>(sql);
        }
        public async Task<List<KnittingPlanBookingChildDTO>> GetBookingChildsAsync(KnittingProgramType type, PaginationInfo paginationInfo)
        {
            string query;
            if (type == KnittingProgramType.Bulk)
            {
                query = $@"
               With YBM As (
				 Select * FROM {TableNames.YarnBookingMaster_New}
	             {paginationInfo.FilterBy}
                  )
				  SELECT YBM.YBookingID, YBM.YBookingNo, YBM.RevisionNo, YBM.YBookingDate BookingDate, ISG.SubGroupName, Unit.DisplayUnitDesc Unit, SUM(YCI.BookingQty) BookingQty
                  FROM YBM
				  INNER JOIN {TableNames.YarnBookingChild_New} YCI ON YBM.YBookingID = YCI.YBookingID
				  INNER JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON YBM.SubGroupID = ISG.SubGroupID
				  INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON YCI.ItemMasterID = IM.ItemMasterID
                  INNER JOIN {DbNames.EPYSL}..Unit ON IM.DefaultTranUnitID = Unit.UnitID
                  LEFT JOIN {TableNames.Knitting_Plan_Child} C ON C.YBookingID = YBM.YBookingID And C.ItemMasterID = YCI.ItemMasterID
                  LEFT Join {TableNames.Knitting_Plan_Master} M ON C.KPMasterID = M.KPMasterID WHERE C.KPChildID IS NULL
				  GROUP BY YBM.YBookingID, YBM.YBookingNo, YBM.RevisionNo, YBM.YBookingDate, ISG.SubGroupName, Unit.DisplayUnitDesc";

            }
            else // BDS
            {
                query = $@"
                With
                FB AS (
	                Select * FROM {TableNames.FBBOOKING_ACKNOWLEDGE}
	                {paginationInfo.FilterBy}
                )
                SELECT FBC.BookingID BookingId, FB.BookingNo, FB.BookingDate,FBC.SubGroupID, ISG.SubGroupName, U.DisplayUnitDesc Unit, SUM(FBC.BookingQty) BookingQty
                FROM FB
                INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FBC ON FB.BookingID = FBC.BookingID
                INNER JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON FBC.SubGroupID = ISG.SubGroupID
				INNER JOIN {DbNames.EPYSL}..Unit U ON FBC.BookingUnitID = U.UnitID
                GROUP BY FBC.BookingID,FB.BookingNo,FB.BookingDate,FBC.SubGroupID, ISG.SubGroupName, U.DisplayUnitDesc";
            }

            return await _service.GetDataAsync<KnittingPlanBookingChildDTO>(query);
        }

        public async Task<List<KnittingPlanBookingChildDetailsDTO>> GetBookingChildsDetailsAsync(Status status, KnittingProgramType type, PaginationInfo paginationInfo)
        {
            string query;
            if (type == KnittingProgramType.Bulk)
            {
                if (status == Status.Pending)
                {
                    query = $@"
                    With F As (
                        SELECT YBookingID, ItemMasterID, BookingQty
                        FROM {TableNames.YarnBookingChild_New}
                       {paginationInfo.FilterBy}
                    )

                    Select ISNULL(C.KPMasterID, 0) KPMasterID, F.YBookingID, F.ItemMasterID, F.BookingQty, IM.ItemName, Unit.DisplayUnitDesc Unit
	                    , CASE WHEN KPChildID IS NULL THEN 0 ELSE 1 END AS KnittingPlanned
                    From F
                    INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON F.ItemMasterID = IM.ItemMasterID
                    INNER JOIN {DbNames.EPYSL}..Unit ON IM.DefaultTranUnitID = Unit.UnitID
                    LEFT JOIN {TableNames.Knitting_Plan_Child} C ON C.YBookingID = F.YBookingID And C.ItemMasterID = F.ItemMasterID
                    LEFT Join {TableNames.Knitting_Plan_Master} M ON C.KPMasterID = M.KPMasterID WHERE KPChildID IS NULL";
                }
                else
                {
                    query = $@"
                    With F As (
                        SELECT YBookingID, ItemMasterID, BookingQty
                        FROM {TableNames.YarnBookingChild_New}
                       {paginationInfo.FilterBy}
                    )

                    Select ISNULL(C.KPMasterID, 0) KPMasterID, F.YBookingID, F.ItemMasterID, F.BookingQty, IM.ItemName, Unit.DisplayUnitDesc Unit
	                    , CASE WHEN KPChildID IS NULL THEN 0 ELSE 1 END AS KnittingPlanned
                    From F
                    INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON F.ItemMasterID = IM.ItemMasterID
                    INNER JOIN {DbNames.EPYSL}..Unit ON IM.DefaultTranUnitID = Unit.UnitID
                    LEFT JOIN {TableNames.Knitting_Plan_Child} C ON C.YBookingID = F.YBookingID And C.ItemMasterID = F.ItemMasterID
                    LEFT Join {TableNames.Knitting_Plan_Master} M ON C.KPMasterID = M.KPMasterID WHERE KPChildID IS NOT NULL";
                }
            }
            else // BDS
            {
                if (status == Status.Pending)
                {
                    query = $@"
                    With F As (
                        SELECT BookingID, ItemMasterID, BookingQty
                        FROM {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD_DETAILS}
                        {paginationInfo.FilterBy}
                    )

                    Select ISNULL(C.KPMasterID, 0) KPMasterID, F.BookingID, F.ItemMasterID, F.BookingQty, IM.ItemName, Unit.DisplayUnitDesc Unit
	                    , CASE WHEN KPChildID IS NULL THEN 0 ELSE 1 END AS KnittingPlanned
                    From F
                    INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON F.ItemMasterID = IM.ItemMasterID
                    INNER JOIN {DbNames.EPYSL}..Unit ON IM.DefaultTranUnitID = Unit.UnitID
                    LEFT JOIN {TableNames.Knitting_Plan_Child} C ON C.YBookingID = F.BookingID And C.ItemMasterID = F.ItemMasterID
                    LEFT Join {TableNames.Knitting_Plan_Master} M ON C.KPMasterID = M.KPMasterID WHERE KPChildID IS NULL";
                }
                else
                {
                    query = $@"
                    With F As (
                        SELECT BookingID, ItemMasterID, BookingQty
                        FROM {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD_DETAILS}
                        {paginationInfo.FilterBy}
                    )

                    Select ISNULL(C.KPMasterID, 0) KPMasterID, F.BookingID, F.ItemMasterID, F.BookingQty, IM.ItemName, Unit.DisplayUnitDesc Unit
	                    , CASE WHEN KPChildID IS NULL THEN 0 ELSE 1 END AS KnittingPlanned
                    From F
                    INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON F.ItemMasterID = IM.ItemMasterID
                    INNER JOIN {DbNames.EPYSL}..Unit ON IM.DefaultTranUnitID = Unit.UnitID
                    LEFT JOIN {TableNames.Knitting_Plan_Child} C ON C.YBookingID = F.BookingID And C.ItemMasterID = F.ItemMasterID
                    LEFT Join {TableNames.Knitting_Plan_Master} M ON C.KPMasterID = M.KPMasterID WHERE KPChildID IS NOT NULL";
                }
            }

            return await _service.GetDataAsync<KnittingPlanBookingChildDetailsDTO>(query);
        }

        public async Task<KnittingPlanMaster> GetNewAsync(int conceptId, bool isBulkPage, bool withoutOB, string subGroupName)
        {
            string colorQuery = subGroupName == "Fabric" ? "IM.Segment3ValueID" : "IM.Segment5ValueID";
            string query = "";
            if (isBulkPage)
            {
                query = $@"
                        -- Master Data
                        With
                        M As (
	                        Select FCM.ConceptID, FCM.ConceptNo, FCM.ConceptDate, FCM.KnittingTypeID, FCM.ConstructionID, FCM.TechnicalNameId, FCM.CompositionID,
							FCM.GSMID, FCM.Remarks, FCM.MCSubClassID, FCM.SubGroupID, FCM.IsBDS, FCM.Qty, FCM.ConceptTypeID, FCM.FUPartID,
							FCM.IsYD, FCM.MachineGauge, FCM.MachineDia, FCM.BrandID, FCM.GroupConceptNo, FCM.ItemMasterID, FCM.BookingID,
							FCM.BuyerID, FCM.BuyerTeamID, FCM.ConceptFor, FCM.ConceptStatusID, FCM.BookingChildID, FCM.TotalQty,
                            GreyProdQty = Case When YBC.BookingUnitID =28 Then FBC.GreyProdQty Else Case When isnull(YBC.QtyInKG,0)>0 Then Round( (YBC.BookingQty/YBC.QtyInKG)*YBC.GreyProdQty,0) Else 0 END END
	                        From {TableNames.RND_FREE_CONCEPT_MASTER} FCM
	                        INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FBC ON FBC.BookingChildID = FCM.BookingChildID AND FBC.BookingID = FCM.BookingID
                            LEFT JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.BookingChildID = FBC.BookingChildID
	                        Where FCM.ConceptID = {conceptId}
                        ),
                        TotalKPDone AS
                        (
	                        SELECT M.ConceptID, RemainingPlanQty = SUM(KPC.BookingQty)
	                        FROM {TableNames.Knitting_Plan_Master} KPM
	                        INNER JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPMasterID = KPM.KPMasterID
	                        INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} M ON M.ConceptID = KPM.ConceptID
	                        GROUP BY M.ConceptID
                        ), 
                        F AS
                        (
	                        SELECT M.ConceptID, M.ConceptNo, M.ConceptDate, M.KnittingTypeID, M.ConstructionID,M.TechnicalNameId, M.CompositionID, M.GSMID, 
	                        M.Remarks, M.MCSubClassID,
	                        ISNULL(KPD.RemainingPlanQty,0) PlanQty, M.SubGroupID SubGroupID, ISG.SubGroupName,
                            BookingQty = CASE WHEN M.IsBDS = 2 THEN M.Qty ELSE 0 END,
	                        Qty = CASE WHEN M.IsBDS = 2 THEN M.GreyProdQty ELSE 0 END,
	                        Technical.TechnicalName,
                            F.ValueName ConceptForName, S.ValueName ConceptStatus, KMS.SubClassName MCSubClass,
	                        M.ConceptTypeID, M.FUPartID, M.IsYD, M.MachineGauge, M.MachineDia, M.BrandID, Brand = ETV2.ValueName, MR.RevisionNo PreProcessRevNo, MR.RevisionNo, MR.RevisionDate, MR.RevisionBy, MR.RevisionReason,
	                        FU.PartName FUPartName, M.GroupConceptNo, M.IsBDS, M.ItemMasterID, M.BookingID, M.BuyerID, M.BuyerTeamID, BM.ExportOrderID, 
	                        Buyer = CASE WHEN M.BuyerID > 0 THEN ISNULL(CTO.ShortName,'') ELSE 'R&D' END, 
	                        BuyerTeam = CASE WHEN M.BuyerTeamID > 0 THEN ISNULL(CCT.TeamName,'') ELSE 'R&D' END, 
	                        TotalQty = ISNULL(KPD.RemainingPlanQty,0), MaxQty = ISNULL(KPD.RemainingPlanQty,0), Isnull(FAC.IsSubContact,0)IsSubContact,
	                        Construction = ISV1.SegmentValue, 
                            Composition = CASE WHEN M.SubGroupID = 1 THEN ISV2.SegmentValue ELSE '' END,
	                        ColorName = CASE WHEN M.SubGroupID = 1 THEN ISV3.SegmentValue ELSE ISV5.SegmentValue END, 
                            GSM = CASE WHEN M.SubGroupID = 1 THEN ISV4.SegmentValue ELSE '' END, 
                            DyeingType = CASE WHEN M.SubGroupID = 1 THEN ISV6.SegmentValue ELSE '' END, 
                            KnittingType = CASE WHEN M.SubGroupID = 1 THEN ISV7.SegmentValue ELSE '' END,
                            Length = CASE WHEN M.SubGroupID = 1 THEN 0 ELSE CONVERT(decimal(18,2),ISV3.SegmentValue) END,
                            Width = CASE WHEN M.SubGroupID = 1 THEN 0 ELSE CONVERT(decimal(18,2),ISV4.SegmentValue) END,
	                        Size = CASE WHEN M.SubGroupID <> 1 THEN ISV3.SegmentValue + ' X ' + ISV4.SegmentValue ELSE '' END
	                        FROM M
	                        INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_MASTER} MR ON MR.ConceptID = M.ConceptID
	                        LEFT JOIN TotalKPDone KPD ON KPD.ConceptID = M.ConceptID
	                        INNER JOIN {DbNames.EPYSL}..ItemSubGroup ISG On M.SubGroupID = ISG.SubGroupID
	                        LEFT JOIN {TableNames.Knitting_Plan_Master} KP ON M.ConceptID = KP.ConceptID
	                        LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPMasterID = KP.KPMasterID
	                        LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID = M.KnittingTypeID
	                        LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME} Technical ON Technical.TechnicalNameId = M.TechnicalNameId
	                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue F ON F.ValueID = M.ConceptFor
	                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue S ON S.ValueID = M.ConceptStatusID
	                        LEFT JOIN {TableNames.KNITTING_MACHINE_SUBCLASS} KMS ON KMS.SubClassID = M.MCSubClassID
	                        LEFT JOIN {DbNames.EPYSL}..FabricUsedPart FU ON FU.FUPartID = M.FUPartID
	                        LEFT JOIN {DbNames.EPYSL}..BookingMaster BM ON BM.BookingID = M.BookingID
	                        LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = M.BuyerID
	                        LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = M.BuyerTeamID
	                        Left Join {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FAC On FAC.BookingID = M.BookingID And FAC.BookingChildID = M.BookingChildID  
	                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ETV2 ON ETV2.ValueID = M.BrandID

	                        LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = M.ItemMasterID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID

	                        GROUP BY M.ConceptID, M.ConceptNo, M.ConceptDate, M.KnittingTypeID, M.ConstructionID,M.TechnicalNameId, M.CompositionID, M.GSMID, M.Qty, M.Remarks, M.MCSubClassID,
	                        M.SubGroupID, ISG.SubGroupName,CASE WHEN M.IsBDS = 2 THEN M.GreyProdQty ELSE 0 END,
	                        Technical.TechnicalName,
                            F.ValueName, S.ValueName, KMS.SubClassName,
	                        M.ConceptTypeID, M.FUPartID, M.IsYD, M.MachineGauge, M.MachineDia, M.BrandID, ETV2.ValueName, MR.RevisionNo, MR.RevisionNo, MR.RevisionDate, MR.RevisionBy, MR.RevisionReason,
	                        FU.PartName, M.GroupConceptNo, M.IsBDS, M.ItemMasterID, M.BookingID, M.BuyerID, M.BuyerTeamID, BM.ExportOrderID, 
	                        CASE WHEN M.BuyerID > 0 THEN ISNULL(CTO.ShortName,'') ELSE 'R&D' END, 
	                        CASE WHEN M.BuyerTeamID > 0 THEN ISNULL(CCT.TeamName,'') ELSE 'R&D' END, 
	                        M.TotalQty, ISNULL(KPD.RemainingPlanQty,0), Isnull(FAC.IsSubContact,0),
	                        ISV1.SegmentValue, 
                            CASE WHEN M.SubGroupID = 1 THEN ISV2.SegmentValue ELSE '' END,
	                        CASE WHEN M.SubGroupID = 1 THEN ISV3.SegmentValue ELSE ISV5.SegmentValue END, 
                            CASE WHEN M.SubGroupID = 1 THEN ISV4.SegmentValue ELSE '' END, 
                            CASE WHEN M.SubGroupID = 1 THEN ISV6.SegmentValue ELSE '' END, 
                            CASE WHEN M.SubGroupID = 1 THEN ISV7.SegmentValue ELSE '' END,
                            CASE WHEN M.SubGroupID = 1 THEN 0 ELSE CONVERT(decimal(18,2),ISV3.SegmentValue) END,
                            CASE WHEN M.SubGroupID = 1 THEN 0 ELSE CONVERT(decimal(18,2),ISV4.SegmentValue) END,
	                        CASE WHEN M.SubGroupID <> 1 THEN ISV3.SegmentValue + ' X ' + ISV4.SegmentValue ELSE '' END
                        ),
                        CheckPreprocess AS
                        (
	                        SELECT M.BookingID, PreProcessCount = COUNT(*)
	                        FROM {TableNames.FBOOKING_ACKNOWLEDGE_CHILD_PLANNING} FCP
	                        INNER JOIN M ON M.BookingChildID = FCP.BookingChildID
	                        WHERE FCP.CriteriaID IN (7,8,9)
	                        GROUP BY M.BookingID
                        ),
                        FinalObj AS
                        (
	                        SELECT F.*, ProcessTime = ISNULL(CP.PreProcessCount,0)
	                        FROM F
	                        LEFT JOIN CheckPreprocess CP ON CP.BookingID = F.BookingID
                        )
                        SELECT * FROM FinalObj";
            }
            else
            {
                query = $@"-- Master Data
                With
                M As (
	                Select FCM.ConceptID, FCM.ConceptNo, FCM.ConceptDate, FCM.KnittingTypeID, FCM.ConstructionID, FCM.TechnicalNameId, 
					FCM.CompositionID, FCM.GSMID, FCM.IsBDS, FCM.Qty, FCM.Remarks, FCM.MCSubClassID, FCM.SubGroupID,FCM.ConceptTypeID, 
					FCM.FUPartID, FCM.IsYD, FCM.MachineGauge, FCM.[Length], FCM.Width, FCM.GroupConceptNo, FCM.ItemMasterID, FCM.BookingID,
					FCM.TotalQty, FCM.BookingChildID, FCM.ConceptFor, FCM.ConceptStatusID, 
                    GreyProdQty = Round( Case When YBC.BookingUnitID =28 Then FBC.GreyProdQty Else (YBC.BookingQty/YBC.QtyInKG)*YBC.GreyProdQty END, 0)
	                From {TableNames.RND_FREE_CONCEPT_MASTER} FCM
	                LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FBC ON FBC.BookingChildID = FCM.BookingChildID
                    LEFT JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.BookingChildID = FBC.BookingChildID
	                Where FCM.ConceptID = {conceptId}
                ),
                F AS
                (
	                SELECT M.ConceptID, M.ConceptNo, M.ConceptDate, M.KnittingTypeID, M.ConstructionID,M.TechnicalNameId, M.CompositionID, M.GSMID, 
	                BookingQty = CASE WHEN M.IsBDS = 2 THEN M.Qty ELSE 0 END,
	                Qty = CASE WHEN M.IsBDS = 2 THEN M.GreyProdQty ELSE M.Qty END,
	                M.Remarks, M.MCSubClassID,
	                SUM(ISNULL(KPC.BookingQty,0)) PlanQty, M.SubGroupID SubGroupID, ISG.SubGroupName, KnittingType.TypeName KnittingType, 
	                Composition.SegmentValue Composition,
	                Construction.SegmentValue Construction,Technical.TechnicalName, 

                    ColorName = CASE WHEN M.SubGroupID = 1 THEN Color.SegmentValue ELSE ISV5.SegmentValue END, 
                    Gsm = CASE WHEN M.SubGroupID = 1 THEN Gsm.SegmentValue ELSE '' END,

                    F.ValueName ConceptForName, S.ValueName ConceptStatus, KMS.SubClassName MCSubClass,
	                M.ConceptTypeID, M.FUPartID, M.IsYD, M.MachineGauge, MR.RevisionNo PreProcessRevNo, MR.RevisionNo, MR.RevisionDate, MR.RevisionBy, MR.RevisionReason,
	                FU.PartName FUPartName, M.[Length], M.Width, M.GroupConceptNo, M.IsBDS, M.ItemMasterID, M.BookingID, BM.BuyerID, BM.BuyerTeamID, 
	                Buyer = CASE WHEN BM.BuyerID > 0 THEN ISNULL(CTO.ShortName,'') ELSE 'R&D' END, 
	                BuyerTeam = CASE WHEN BM.BuyerTeamID > 0 THEN ISNULL(CCT.TeamName,'') ELSE 'R&D' END,  
	
	                M.TotalQty, MaxQty = M.TotalQty, 
	
	                Isnull(FAC.IsSubContact,0)IsSubContact
	                FROM M
	                INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_MASTER} MR ON MR.ConceptID = M.ConceptID
	                LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = M.ItemMasterID
	                INNER JOIN {DbNames.EPYSL}..ItemSubGroup ISG On M.SubGroupID = ISG.SubGroupID
	                LEFT JOIN {TableNames.Knitting_Plan_Master} KP ON M.ConceptID = KP.ConceptID
	                LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPMasterID = KP.KPMasterID
	                LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID = M.KnittingTypeID
	                Left Join {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FAC On FAC.BookingID = M.BookingID And FAC.BookingChildID = M.BookingChildID                   
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = M.CompositionID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = M.ConstructionID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = IM.Segment3ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
	                LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME} Technical ON Technical.TechnicalNameId=M.TechnicalNameId
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = M.GSMID
	                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue F ON F.ValueID = M.ConceptFor
	                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue S ON S.ValueID = M.ConceptStatusID
	                LEFT JOIN {TableNames.KNITTING_MACHINE_SUBCLASS} KMS ON KMS.SubClassID = M.MCSubClassID
	                LEFT JOIN {DbNames.EPYSL}..FabricUsedPart FU ON FU.FUPartID = M.FUPartID
	                LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster BM ON BM.BookingID = M.BookingID
	                LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = BM.BuyerID
	                LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = BM.BuyerTeamID
	                GROUP BY M.ConceptID, M.ConceptNo, M.ConceptDate, M.KnittingTypeID, M.ConstructionID,M.TechnicalNameId, M.CompositionID, M.GSMID, M.Qty,
	                CASE WHEN M.IsBDS = 2 THEN M.Qty ELSE 0 END,CASE WHEN M.IsBDS = 2 THEN M.GreyProdQty ELSE 0 END,
	                M.Remarks, M.MCSubClassID, CASE WHEN M.IsBDS = 2 THEN M.Qty ELSE 0 END, CASE WHEN M.IsBDS = 2 THEN M.GreyProdQty ELSE M.Qty END
	                , M.SubGroupID, ISG.SubGroupName, KnittingType.TypeName, Composition.SegmentValue, Construction.SegmentValue,Technical.TechnicalName ,Gsm.SegmentValue, Color.SegmentValue, F.ValueName
	                , S.ValueName, KMS.SubClassName, M.ConceptTypeID, M.FUPartID, M.IsYD, M.MachineGauge, MR.RevisionNo, MR.RevisionDate, MR.RevisionBy, MR.RevisionReason, FU.PartName
	                , M.[Length], M.Width, M.GroupConceptNo, M.IsBDS, M.ItemMasterID, M.BookingID, BM.BuyerID, BM.BuyerTeamID, ISV5.SegmentValue,
	                ISNULL(CTO.ShortName,''), ISNULL(CCT.TeamName,''), M.TotalQty, Isnull(FAC.IsSubContact,0)
                ),
                CheckPreprocess AS
                (
	                SELECT M.BookingID, PreProcessCount = COUNT(*)
	                FROM {TableNames.FBOOKING_ACKNOWLEDGE_CHILD_PLANNING} FCP
	                INNER JOIN M ON M.BookingChildID = FCP.BookingChildID
	                WHERE FCP.CriteriaID IN (7,8,9)
	                GROUP BY M.BookingID
                ),
                FinalObj AS
                (
	                SELECT F.*, ProcessTime = ISNULL(CP.PreProcessCount,0)
	                FROM F
	                LEFT JOIN CheckPreprocess CP ON CP.BookingID = F.BookingID
                )
                SELECT * FROM FinalObj";
            }

            query += $@"
            --Yarn List
            ;With FCMRC AS 
            (
	            Select FCMRC.FCMRChildID, FCMRC.ItemMasterID, FCMRC.YarnCategory, FCMRC.YarnStockSetId, FCMRC.YD, FCMRC.YDItem, FCMRC.ReqQty, 
                FCM.ConceptID, FCM.TotalQty, StitchLength = ISNULL(YBCI.StitchLength,0),
	            YSS.PhysicalCount, YSS.YarnLotNo, Spinner = SP.ShortName, YarnBrandID = YSS.SpinnerId,
                IsStockItemFromMR = CASE WHEN FCMRC.YarnStockSetId > 0 THEN 1 ELSE 0 END
	            FROM {TableNames.RND_FREE_CONCEPT_MR_CHILD} AS FCMRC
	            INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_MASTER} MR ON MR.FCMRMasterID = FCMRC.FCMRMasterID
	            INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = MR.ConceptID
	            LEFT JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBCI.YBChildItemID = FCMRC.YBChildItemID
	            LEFT JOIN {TableNames.YarnStockSet} YSS ON YSS.YarnStockSetId = FCMRC.YarnStockSetId
	            LEFT JOIN {DbNames.EPYSL}..Contacts SP ON SP.ContactID = YSS.SpinnerId
	            WHERE MR.ConceptID = {conceptId} AND FCMRC.YD = 0
            ),
            YD AS 
            (
	            Select Distinct FCMRC.FCMRChildID, FCMRC.ItemMasterID, 
	            FCMRC.YD, FCMRC.YDItem, PM.BatchNo , FCMRC.YarnCategory, FCMRC.ReqQty, FCM.TotalQty, 
	            FCMRC.YarnStockSetId, YSS.PhysicalCount, YSS.YarnLotNo, Spinner = SP.ShortName, YarnBrandID = YSS.SpinnerId,
                IsStockItemFromMR = CASE WHEN FCMRC.YarnStockSetId > 0 THEN 1 ELSE 0 END
	            FROM {TableNames.RND_FREE_CONCEPT_MR_CHILD} AS FCMRC
	            INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_MASTER} MR ON MR.FCMRMasterID = FCMRC.FCMRMasterID
	            INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = MR.ConceptID
	            Inner Join {TableNames.YD_BOOKING_MASTER} YBM On YBM.GroupConceptNo = FCM.GroupConceptNo
	            Inner Join {TableNames.YDBookingChild} YBC On YBC.YDBookingMasterID = YBM.YDBookingMasterID 
										            And YBC.ItemMasterID = FCMRC.ItemMasterId 
										            --And YBC.YD = FCMRC.YD And YBC.YDItem = FCMRC.YDItem ----off because user want to knitting program before production 
	            LEFT JOIN {TableNames.YD_PRODUCTION_MASTER} PM ON PM.YDBookingMasterID = YBM.YDBookingMasterID
	            LEFT JOIN {TableNames.YD_PRODUCTION_CHILD} YPC ON YPC.YDProductionMasterID = PM.YDProductionMasterID And YPC.ItemMasterID = YBC.ItemMasterID
	            LEFT JOIN {TableNames.YarnStockSet} YSS ON YSS.YarnStockSetId = FCMRC.YarnStockSetId
	            LEFT JOIN {DbNames.EPYSL}..Contacts SP ON SP.ContactID = YSS.SpinnerId
	            WHERE MR.ConceptID = {conceptId} AND FCMRC.YD = 1  AND ISNULL(FCMRC.YarnCategory,'') != '' --AND PM.IsAcknowledge = 1 And Isnull(YPC.ItemMasterID,0) != 0
            )
            SELECT DISTINCT FCMRC.FCMRChildID, FCMRC.ItemMasterID, FCMRC.YarnCategory, ISV2.SegmentValueID YarnTypeID, ISV2.SegmentValue YarnType, 
            ISV6.SegmentValueID YarnCountID, ISV6.SegmentValue YarnCount, FCMRC.YD, FCMRC.YDItem,'' AS BatchNo, ISV1.SegmentValue Composition, FCMRC.ReqQty, FCMRC.TotalQty,
            FCMRC.StitchLength, FCMRC.YarnStockSetId, FCMRC.PhysicalCount, FCMRC.YarnLotNo, FCMRC.Spinner, FCMRC.YarnBrandID, FCMRC.IsStockItemFromMR
            FROM FCMRC
            INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = FCMRC.ItemMasterID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
            UNION
            SELECT DISTINCT YD.FCMRChildID, YD.ItemMasterID, YD.YarnCategory, ISV2.SegmentValueID YarnTypeID, ISV2.SegmentValue YarnType, 
            ISV6.SegmentValueID YarnCountID, ISV6.SegmentValue YarnCount, YD.YD, YD.YDItem, YD.BatchNo, ISV1.SegmentValue Composition, YD.ReqQty, YD.TotalQty,
            StitchLength = 0, YD.YarnStockSetId, YD.PhysicalCount, YD.YarnLotNo, YD.Spinner, YD.YarnBrandID, YD.IsStockItemFromMR
            FROM YD
            INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YD.ItemMasterID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID;
            

            ----Knitting Type
            Select CAST(a.SegmentValueID AS varchar) [id], a.SegmentValue [text]
            From {DbNames.EPYSL}..ItemSegmentValue a
            Inner Join {DbNames.EPYSL}..ItemSegmentName b On b.SegmentNameID = a.SegmentNameID
            where SegmentName = 'Knitting Type' and a.SegmentValue <> 'Both';

            {CommonQueries.GetSpinner()}

            -- Machine Information
            /* SELECT KM.KnittingMachineID,KM.KnittingUnitID,KM.MachineNo,KM.MachineTypeID,KM.MachineSubClassID,KM.GG,KM.Dia,KM.BrandID,KM.Capacity,KU.ShortName AS Contact,EV.ValueName AS Brand,
            IsSubContact = Case When C.MappingCompanyID = 0 Then 1 Else 0 End
            from {TableNames.KNITTING_MACHINE} KM 
            Left Join {TableNames.KNITTING_UNIT} KU ON KU.KnittingUnitID = KM.KnittingUnitID
            Left Join {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID = KM.BrandID
			Left Join {DbNames.EPYSL}..Contacts C ON C.ContactID = KU.ContactID; */ 

            -- Machine Information
            SELECT KM.KnittingMachineID,KM.KnittingUnitID,KM.MachineNo,KM.MachineTypeID,KM.MachineSubClassID,
            KM.GG,KM.Dia,KM.BrandID,KM.Capacity,KU.ShortName AS Contact,EV.ValueName AS Brand,
            IsSubContact = 0
            from {TableNames.KNITTING_MACHINE} KM
            Left Join {TableNames.KNITTING_UNIT} KU ON KU.KnittingUnitID = KM.KnittingUnitID
            Left Join {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID = KM.BrandID
            Left Join {DbNames.EPYSL}..Contacts C ON C.ContactID = KU.ContactID
            Left Join {DbNames.EPYSL}..ContactAdditionalInfo CAI ON CAI.ContactID = C.ContactID
            Where CAI.InHouse = 1 And C.MappingCompanyID <> 0;

            -- SubContract
            SELECT KM.KnittingMachineID,KM.KnittingUnitID,KM.MachineNo,KM.MachineTypeID,KM.MachineSubClassID,
            KM.GG,KM.Dia,KM.BrandID,KM.Capacity,KU.ContactID, KU.ShortName AS Contact,EV.ValueName AS Brand,
            IsSubContact = 1 
            from {TableNames.KNITTING_MACHINE} KM
            Left Join {TableNames.KNITTING_UNIT} KU ON KU.KnittingUnitID = KM.KnittingUnitID
            Left Join {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID = KM.BrandID
            Left Join {DbNames.EPYSL}..Contacts C ON C.ContactID = KU.ContactID
            Left Join {DbNames.EPYSL}..ContactAdditionalInfo CAI ON CAI.ContactID = C.ContactID
            Where CAI.InLand = 1 And C.MappingCompanyID = 0; 
            
            --Machine Type List
            SELECT CAST(a.MachineSubClassID AS varchar) [id], b.SubClassName [text], b.TypeID [desc], c.TypeName additionalValue
            FROM {TableNames.KNITTING_MACHINE} a
            INNER JOIN {TableNames.KNITTING_MACHINE_SUBCLASS} b ON b.SubClassID = a.MachineSubClassID
            Inner Join {TableNames.KNITTING_MACHINE_TYPE} c On c.TypeID = b.TypeID
            Where a.MachineSubClassID in(SELECT FTN.SubClassID
            FROM {TableNames.FABRIC_TECHNICAL_NAME} TN
            INNER JOIN {TableNames.FABRIC_TECHNICAL_NAME}KMachineSubClass FTN ON FTN.TechnicalNameID=TN.TechnicalNameId
            WHERE TN.TechnicalNameId = (Select TechnicalNameId From {TableNames.RND_FREE_CONCEPT_MASTER}
	        WHERE ConceptID = {conceptId}))
            GROUP BY a.MachineSubClassID, b.SubClassName, b.TypeID, c.TypeName;
            ";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                KnittingPlanMaster data = await records.ReadFirstOrDefaultAsync<KnittingPlanMaster>();
                data.Yarns = records.Read<KnittingPlanYarn>().ToList();
                data.KnittingTypeList = records.Read<Select2OptionModel>().ToList();
                data.YarnBrandList = await records.ReadAsync<Select2OptionModel>();
                data.KnittingMachines = records.Read<KnittingMachine>().ToList();  // records.ReadAsync<KnittingMachine>();
                data.KnittingSubContracts = records.Read<KnittingMachine>().ToList();
                data.MachineTypeList = records.Read<Select2OptionModel>().ToList();

                if (isBulkPage)
                {
                    data.Yarns.ForEach(c =>
                    {
                        c.YDItem = c.YD;
                    });
                }

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
        public async Task<KnittingPlanGroup> GetNewGroupAsync(string conceptIds, bool isBulkPage, bool withoutOB, string subGroupName)
        {
            string colorQuery = subGroupName == "Fabric" ? "IM.Segment3ValueID" : "IM.Segment3ValueID";
            string query = "";
            if (isBulkPage && !withoutOB)
            {
                query = $@"-- Master Data
                        With
                        M As (
	                        Select * From {TableNames.RND_FREE_CONCEPT_MASTER}
	                        Where ConceptID IN ({conceptIds})
                        )
                        SELECT M.GroupConceptNo,M.ConceptDate, M.ConceptID, M.ConceptNo, M.ConceptDate, M.KnittingTypeID, M.ConstructionID,M.TechnicalNameId, M.CompositionID, M.GSMID, M.Qty, M.Remarks, M.MCSubClassID,
			            M.TotalQty - SUM(ISNULL(KPC.BookingQty,0)) TotalPlanedQty, M.SubGroupID, ISG.SubGroupName, KnittingType.TypeName KnittingType, Composition.SegmentValue Composition,
			            Construction.SegmentValue Construction,Technical.TechnicalName, Gsm.SegmentValue Gsm, Color.SegmentValue ColorName, F.ValueName ConceptForName, S.ValueName ConceptStatus, KMS.SubClassName MCSubClass,
			            M.ConceptTypeID, M.FUPartID, M.IsYD, Size = Case When M.SubGroupID <> 1 Then Color.SegmentValue + ' X ' + Gsm.SegmentValue Else '' End,
			            FU.PartName FUPartName, M.[Length], M.Width, M.GroupConceptNo, M.IsBDS, M.ItemMasterID, M.BookingID, BM.BuyerID, BM.BuyerTeamID, BM.ExportOrderID, 
                        Buyer = CASE WHEN BM.BuyerID > 0 THEN ISNULL(CTO.ShortName,'') ELSE 'R&D' END, 
                        BuyerTeam = CASE WHEN BM.BuyerTeamID > 0 THEN ISNULL(CCT.TeamName,'') ELSE 'R&D' END, MaxQty = M.TotalQty, 
                        BookingQty = M.TotalQty, M.TotalQty, Isnull(FAC.IsSubContact,0)IsSubContact, UU.DisplayUnitDesc Uom
                        FROM M
			            --INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_MASTER} MR ON MR.ConceptID = M.ConceptID
                        LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = M.ItemMasterID
                        INNER JOIN {DbNames.EPYSL}..ItemSubGroup ISG On M.SubGroupID = ISG.SubGroupID
                        LEFT JOIN {TableNames.Knitting_Plan_Master} KP ON M.ConceptID = KP.ConceptID
                        LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPMasterID = KP.KPMasterID
                        LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID = M.KnittingTypeID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = M.CompositionID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = M.ConstructionID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = {colorQuery}
                        LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME} Technical ON Technical.TechnicalNameId=M.TechnicalNameId
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = M.GSMID
                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue F ON F.ValueID = M.ConceptFor
                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue S ON S.ValueID = M.ConceptStatusID
			            LEFT JOIN {TableNames.KNITTING_MACHINE_SUBCLASS} KMS ON KMS.SubClassID = M.MCSubClassID
			            LEFT JOIN {DbNames.EPYSL}..FabricUsedPart FU ON FU.FUPartID = M.FUPartID
			            LEFT JOIN {DbNames.EPYSL}..BookingMaster BM ON BM.BookingID = M.BookingID
			            LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = BM.BuyerID
			            LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = BM.BuyerTeamID
                        Left Join {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FAC On FAC.BookingID = M.BookingID And FAC.BookingChildID = M.BookingChildID
                        LEFT JOIN {TableNames.FBOOKING_ACKNOWLEDGE_CHILD_PLANNING} FCP on FCP.BookingChildID=FAC.BookingChildID
                        LEFT Join {DbNames.EPYSL}..Unit AS UU On UU.UnitID = KPC.UnitID
						LEFT JOIN {TableNames.BDS_CRITERIA_HK} BDSC on BDSC.CriteriaID = FCP.CriteriaID and BDSC.CriteriaName='Preprocess'     
                        GROUP BY M.ConceptID, M.ConceptNo, M.ConceptDate, M.KnittingTypeID, M.ConstructionID,M.TechnicalNameId, M.CompositionID, M.GSMID, M.Qty, M.Remarks, M.MCSubClassID
	                    , M.SubGroupID, ISG.SubGroupName, KnittingType.TypeName, Composition.SegmentValue, Construction.SegmentValue,Technical.TechnicalName ,Gsm.SegmentValue, Color.SegmentValue, F.ValueName
			            , S.ValueName, KMS.SubClassName, M.ConceptTypeID, M.FUPartID, M.IsYD, FU.PartName
			            , M.[Length], M.Width, M.GroupConceptNo, M.IsBDS, M.ItemMasterID, M.BookingID, BM.BuyerID, BM.BuyerTeamID, 
                        BM.ExportOrderID, ISNULL(CTO.ShortName,''), ISNULL(CCT.TeamName,''), M.TotalQty, Isnull(FAC.IsSubContact,0), UU.DisplayUnitDesc;
                        ";
            }
            else
            {
                query = $@"-- Master Data
                        With
                        M As (
	                        Select * From {TableNames.RND_FREE_CONCEPT_MASTER}
	                        Where ConceptID IN ({conceptIds})
                        )
                        SELECT M.GroupConceptNo,M.ConceptDate, M.ConceptID, M.ConceptNo, M.ConceptDate, M.KnittingTypeID, M.ConstructionID,M.TechnicalNameId, M.CompositionID, M.GSMID, M.Qty, M.Remarks, M.MCSubClassID,
			            M.TotalQty - SUM(ISNULL(KPC.BookingQty,0)) TotalPlanedQty, M.SubGroupID SubGroupID, ISG.SubGroupName, KnittingType.TypeName KnittingType, Composition.SegmentValue Composition,
			            Construction.SegmentValue Construction,Technical.TechnicalName, Gsm.SegmentValue Gsm, Color.SegmentValue ColorName, F.ValueName ConceptForName, S.ValueName ConceptStatus, KMS.SubClassName MCSubClass,
			            M.ConceptTypeID, M.FUPartID, M.IsYD, Size = Case When M.SubGroupID <> 1 Then Color.SegmentValue + ' X ' + Gsm.SegmentValue Else '' End,
			            FU.PartName FUPartName, M.[Length], M.Width, M.GroupConceptNo, M.IsBDS, M.ItemMasterID, M.BookingID, BM.BuyerID, BM.BuyerTeamID, 
                        Buyer = CASE WHEN BM.BuyerID > 0 THEN ISNULL(CTO.ShortName,'') ELSE 'R&D' END, 
                        BuyerTeam = CASE WHEN BM.BuyerTeamID > 0 THEN ISNULL(CCT.TeamName,'') ELSE 'R&D' END, MaxQty = M.TotalQty,  
                        BookingQty = M.TotalQty, M.TotalQty, Isnull(FAC.IsSubContact,0)IsSubContact, UU.DisplayUnitDesc Uom,
                        PreProcessRevNo = MR.RevisionNo
                        FROM M
			            LEFT JOIN {TableNames.RND_FREE_CONCEPT_MR_MASTER} MR ON MR.ConceptID = M.ConceptID
                        LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = M.ItemMasterID
                        INNER JOIN {DbNames.EPYSL}..ItemSubGroup ISG On M.SubGroupID = ISG.SubGroupID
                        LEFT JOIN {TableNames.Knitting_Plan_Master} KP ON M.ConceptID = KP.ConceptID
                        LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPMasterID = KP.KPMasterID
                        LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID = M.KnittingTypeID
                        Left Join {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FAC On FAC.BookingID = M.BookingID And FAC.BookingChildID = M.BookingChildID
                        LEFT JOIN {TableNames.FBOOKING_ACKNOWLEDGE_CHILD_PLANNING} FCP on FCP.BookingChildID=FAC.BookingChildID
						LEFT JOIN {TableNames.BDS_CRITERIA_HK} BDSC on BDSC.CriteriaID = FCP.CriteriaID and BDSC.CriteriaName='Preprocess'  
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = M.CompositionID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = M.ConstructionID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = {colorQuery}
                        LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME} Technical ON Technical.TechnicalNameId=M.TechnicalNameId
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = M.GSMID
                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue F ON F.ValueID = M.ConceptFor
                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue S ON S.ValueID = M.ConceptStatusID
			            LEFT JOIN {TableNames.KNITTING_MACHINE_SUBCLASS} KMS ON KMS.SubClassID = M.MCSubClassID
			            LEFT JOIN {DbNames.EPYSL}..FabricUsedPart FU ON FU.FUPartID = M.FUPartID
			            LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster BM ON BM.BookingID = M.BookingID
			            LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = BM.BuyerID
			            LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = BM.BuyerTeamID
                        LEFT Join {DbNames.EPYSL}..Unit AS UU On UU.UnitID = KPC.UnitID
                        GROUP BY M.ConceptID, M.ConceptNo, M.ConceptDate, M.KnittingTypeID, M.ConstructionID,M.TechnicalNameId, M.CompositionID, M.GSMID, M.Qty, M.Remarks, M.MCSubClassID
	                    , M.SubGroupID, ISG.SubGroupName, KnittingType.TypeName, Composition.SegmentValue, Construction.SegmentValue,Technical.TechnicalName ,Gsm.SegmentValue, Color.SegmentValue, F.ValueName
			            , S.ValueName, KMS.SubClassName, M.ConceptTypeID, M.FUPartID, M.IsYD, FU.PartName
			            , M.[Length], M.Width, M.GroupConceptNo, M.IsBDS, M.ItemMasterID, M.BookingID, BM.BuyerID, BM.BuyerTeamID, 
                        ISNULL(CTO.ShortName,''), ISNULL(CCT.TeamName,''), M.TotalQty, Isnull(FAC.IsSubContact,0), UU.DisplayUnitDesc, MR.RevisionNo;";
            }

            query += $@"

            -- Yarn Data
             ;With FCMRC AS 
            (
	            Select FCMRC.*
	            FROM {TableNames.RND_FREE_CONCEPT_MR_CHILD} AS FCMRC
	            INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_MASTER} MR ON MR.FCMRMasterID = FCMRC.FCMRMasterID
	            INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = MR.ConceptID
	            WHERE MR.ConceptID IN ({conceptIds}) AND FCMRC.YD = 0
            ),
            YD AS 
            (
	            Select Distinct FCMRC.ItemMasterID, FCMRC.YD, FCMRC.YDItem, PM.BatchNo , FCMRC.YarnCategory
	            FROM {TableNames.RND_FREE_CONCEPT_MR_CHILD} AS FCMRC
	            INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_MASTER} MR ON MR.FCMRMasterID = FCMRC.FCMRMasterID
	            INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = MR.ConceptID
	            Inner Join {TableNames.YD_BOOKING_MASTER} YBM On YBM.GroupConceptNo = FCM.GroupConceptNo
	            Inner Join {TableNames.YDBookingChild} YBC On YBC.YDBookingMasterID = YBM.YDBookingMasterID 
									             And YBC.ItemMasterID = FCMRC.ItemMasterId 
									             --And YBC.YD = FCMRC.YD And YBC.YDItem = FCMRC.YDItem ----off because user want to knitting program before production 
	            LEFT JOIN  {TableNames.YD_PRODUCTION_MASTER} PM ON PM.YDBookingMasterID = YBM.YDBookingMasterID
	            LEFT JOIN  {TableNames.YD_PRODUCTION_CHILD} YPC ON YPC.YDProductionMasterID = PM.YDProductionMasterID And YPC.ItemMasterID = YBC.ItemMasterID	 
	            WHERE MR.ConceptID IN ({conceptIds}) AND FCMRC.YD = 1 --AND PM.IsAcknowledge = 1 And Isnull(YPC.ItemMasterID,0) != 0
            )
            SELECT DISTINCT FCMRC.ItemMasterID, FCMRC.YarnCategory, ISV2.SegmentValueID YarnTypeID, ISV2.SegmentValue YarnType, 
            ISV6.SegmentValueID YarnCountID, ISV6.SegmentValue YarnCount, FCMRC.YD, FCMRC.YDItem,'' AS BatchNo, ISV1.SegmentValue Composition
            FROM FCMRC
            INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = FCMRC.ItemMasterID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
            UNION
            SELECT DISTINCT YD.ItemMasterID, YD.YarnCategory, ISV2.SegmentValueID YarnTypeID, ISV2.SegmentValue YarnType, 
            ISV6.SegmentValueID YarnCountID, ISV6.SegmentValue YarnCount, YD.YD, YD.YDItem, YD.BatchNo, ISV1.SegmentValue Composition
            FROM YD
            INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YD.ItemMasterID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID;

            ----Knitting Type
            Select CAST(a.SegmentValueID AS varchar) [id], a.SegmentValue [text]
            From {DbNames.EPYSL}..ItemSegmentValue a
            Inner Join {DbNames.EPYSL}..ItemSegmentName b On b.SegmentNameID = a.SegmentNameID
            where SegmentName = 'Knitting Type' and a.SegmentValue <> 'Both';

            {CommonQueries.GetSpinner()}

            -- Machine Information
            /* SELECT KM.KnittingMachineID,KM.KnittingUnitID,KM.MachineNo,KM.MachineTypeID,KM.MachineSubClassID,KM.GG,KM.Dia,KM.BrandID,KM.Capacity,KU.ShortName AS Contact,EV.ValueName AS Brand,
            IsSubContact = Case When C.MappingCompanyID = 0 Then 1 Else 0 End
            from {TableNames.KNITTING_MACHINE} KM 
            Left Join {TableNames.KNITTING_UNIT} KU ON KU.KnittingUnitID = KM.KnittingUnitID
            Left Join {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID = KM.BrandID
			Left Join {DbNames.EPYSL}..Contacts C ON C.ContactID = KU.ContactID; */ 

            -- Machine Information
            SELECT KM.KnittingMachineID,KM.KnittingUnitID,KM.MachineNo,KM.MachineTypeID,KM.MachineSubClassID,
            KM.GG,KM.Dia,KM.BrandID,KM.Capacity,KU.ShortName AS Contact,EV.ValueName AS Brand,
            IsSubContact = 0
            from {TableNames.KNITTING_MACHINE} KM
            Left Join {TableNames.KNITTING_UNIT} KU ON KU.KnittingUnitID = KM.KnittingUnitID
            Left Join {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID = KM.BrandID
            Left Join {DbNames.EPYSL}..Contacts C ON C.ContactID = KU.ContactID
            Left Join {DbNames.EPYSL}..ContactAdditionalInfo CAI ON CAI.ContactID = C.ContactID
            Where CAI.InHouse = 1 And C.MappingCompanyID <> 0;

            -- SubContract
            SELECT KM.KnittingMachineID,KM.KnittingUnitID,KM.MachineNo,KM.MachineTypeID,KM.MachineSubClassID,
            KM.GG,KM.Dia,KM.BrandID,KM.Capacity,KU.ContactID, KU.ShortName AS Contact,EV.ValueName AS Brand,
            IsSubContact = 1 
            from {TableNames.KNITTING_MACHINE} KM
            Left Join {TableNames.KNITTING_UNIT} KU ON KU.KnittingUnitID = KM.KnittingUnitID
            Left Join {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID = KM.BrandID
            Left Join {DbNames.EPYSL}..Contacts C ON C.ContactID = KU.ContactID
            Left Join {DbNames.EPYSL}..ContactAdditionalInfo CAI ON CAI.ContactID = C.ContactID
            Where CAI.InLand = 1 And C.MappingCompanyID = 0; 

            --Machine Type List
            SELECT CAST(a.MachineSubClassID AS varchar) [id], b.SubClassName [text], b.TypeID [desc], c.TypeName additionalValue
            FROM {TableNames.KNITTING_MACHINE} a
            INNER JOIN {TableNames.KNITTING_MACHINE_SUBCLASS} b ON b.SubClassID = a.MachineSubClassID
            Inner Join {TableNames.KNITTING_MACHINE_TYPE} c On c.TypeID = b.TypeID
            Where a.MachineSubClassID in(SELECT FTN.SubClassID
            FROM {TableNames.FABRIC_TECHNICAL_NAME} TN
            INNER JOIN {TableNames.FABRIC_TECHNICAL_NAME}KMachineSubClass FTN ON FTN.TechnicalNameID=TN.TechnicalNameId
            WHERE TN.TechnicalNameId IN (Select TechnicalNameId From {TableNames.RND_FREE_CONCEPT_MASTER} WHERE ConceptID IN ({conceptIds})))
            GROUP BY a.MachineSubClassID, b.SubClassName, b.TypeID, c.TypeName;

            -- Yarn Data (With FCMRChildID) => only for checking yarns of multiple concepts
            ;With FCMRC AS 
            (
	            Select FCMRC.*
	            FROM {TableNames.RND_FREE_CONCEPT_MR_CHILD} AS FCMRC
	            INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_MASTER} MR ON MR.FCMRMasterID = FCMRC.FCMRMasterID
	            INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = MR.ConceptID
	            WHERE MR.ConceptID IN ({conceptIds}) AND FCMRC.YD = 0
            ),
            YD AS 
            (
	            Select Distinct FCMRC.ItemMasterID, FCMRC.YD, FCMRC.YDItem, PM.BatchNo , FCMRC.YarnCategory, FCMRC.FCMRChildID, FCMRC.FCMRMasterID
	            FROM {TableNames.RND_FREE_CONCEPT_MR_CHILD} AS FCMRC
	            INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_MASTER} MR ON MR.FCMRMasterID = FCMRC.FCMRMasterID
	            INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = MR.ConceptID
	            Inner Join {TableNames.YD_BOOKING_MASTER} YBM On YBM.GroupConceptNo = FCM.GroupConceptNo
	            Inner Join {TableNames.YDBookingChild} YBC On YBC.YDBookingMasterID = YBM.YDBookingMasterID 
									                And YBC.ItemMasterID = FCMRC.ItemMasterId 
	            LEFT JOIN  {TableNames.YD_PRODUCTION_MASTER} PM ON PM.YDBookingMasterID = YBM.YDBookingMasterID
	            LEFT JOIN  {TableNames.YD_PRODUCTION_CHILD} YPC ON YPC.YDProductionMasterID = PM.YDProductionMasterID And YPC.ItemMasterID = YBC.ItemMasterID	 
	            WHERE MR.ConceptID IN ({conceptIds}) AND FCMRC.YD = 1
            ),
            FinalList AS
            (
	            SELECT DISTINCT FCMRC.ItemMasterID, FCMRC.YarnCategory, ISV2.SegmentValueID YarnTypeID, ISV2.SegmentValue YarnType, FCMRC.FCMRChildID, FCMRC.FCMRMasterID,
	            ISV6.SegmentValueID YarnCountID, ISV6.SegmentValue YarnCount, FCMRC.YD, FCMRC.YDItem,'' AS BatchNo, ISV1.SegmentValue Composition
	            FROM FCMRC
	            INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = FCMRC.ItemMasterID
	            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
	            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
	            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
	            UNION
	            SELECT DISTINCT YD.ItemMasterID, YD.YarnCategory, ISV2.SegmentValueID YarnTypeID, ISV2.SegmentValue YarnType, YD.FCMRChildID, YD.FCMRMasterID,
	            ISV6.SegmentValueID YarnCountID, ISV6.SegmentValue YarnCount, YD.YD, YD.YDItem, YD.BatchNo, ISV1.SegmentValue Composition
	            FROM YD
	            INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YD.ItemMasterID
	            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
	            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
	            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
            )
            SELECT * FROM FinalList ORDER BY FCMRMasterID, ItemMasterId";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);

                var knittingPlanGroup = new KnittingPlanGroup();
                knittingPlanGroup.KnittingPlans = records.Read<KnittingPlanMaster>().ToList();
                //List<KnittingPlanChild> childs = records.Read<KnittingPlanChild>().ToList();
                //knittingPlanGroup.KnittingPlans.ForEach(m=> {
                //    m.Childs = childs.Where(c => c.KPMasterID == m.KPMasterID).ToList();
                //});
                knittingPlanGroup.GroupConceptNo = knittingPlanGroup.KnittingPlans.Count() > 0 ? knittingPlanGroup.KnittingPlans.First().GroupConceptNo : "";
                knittingPlanGroup.MCSubClassID = knittingPlanGroup.KnittingPlans.Count() > 0 ? knittingPlanGroup.KnittingPlans.First().MCSubClassID : 0;
                knittingPlanGroup.IsBDS = knittingPlanGroup.KnittingPlans.Count() > 0 ? knittingPlanGroup.KnittingPlans.First().IsBDS : 0;
                knittingPlanGroup.Yarns = records.Read<KnittingPlanYarn>().ToList();
                knittingPlanGroup.KnittingTypeList = records.Read<Select2OptionModel>().ToList();
                knittingPlanGroup.YarnBrandList = records.Read<Select2OptionModel>().ToList();
                knittingPlanGroup.KnittingMachines = records.Read<KnittingMachine>().ToList();
                knittingPlanGroup.KnittingSubContracts = records.Read<KnittingMachine>().ToList();
                knittingPlanGroup.MachineTypeList = records.Read<Select2OptionModel>().ToList();

                //knittingPlanGroup.TotalQty = knittingPlanGroup.KnittingPlans.Sum(x => x.TotalQty);
                //knittingPlanGroup.PlanQty = knittingPlanGroup.TotalQty;
                //knittingPlanGroup.BalanceQty = knittingPlanGroup.TotalQty - knittingPlanGroup.PlanQty;

                #region Checking yarns for multiple concepts
                var yarns = records.Read<KnittingPlanYarn>().ToList();
                if (yarns.Count() > 0)
                {
                    string fcmrMasterIds = string.Join(",", yarns.Select(x => x.FCMRMasterID).Distinct());
                    var fcmrMasterIdList = fcmrMasterIds.Split(',');
                    List<string> itemMasterIDs = new List<string>();
                    for (int index = 0; index < fcmrMasterIdList.Count(); index++)
                    {
                        int fcmrMasterId = fcmrMasterIdList[index].ToInt();
                        var yarnList = yarns.Where(x => x.FCMRMasterID == fcmrMasterId).OrderBy(x => x.ItemMasterID).ToList();
                        string itemMasterID = string.Join(",", yarnList.Select(x => x.ItemMasterID).Distinct());
                        if (itemMasterIDs.Count() == 0)
                        {
                            itemMasterIDs.Add(itemMasterID);
                        }
                        else
                        {
                            bool isExist = itemMasterIDs.Exists(x => x == itemMasterID);
                            if (!isExist)
                            {
                                throw new Exception("Concepts should have same yarns");
                            }
                        }
                    }
                }
                #endregion

                var newGroupList = knittingPlanGroup.KnittingPlans.GroupBy(g => new { g.GroupConceptNo, g.ConceptDate, g.BuyerID, g.Buyer, g.BuyerTeamID, g.BuyerTeam, g.SubGroupID, g.SubGroupName, g.KnittingType, g.TechnicalName, g.TechnicalNameId, g.MCSubClassID, g.MCSubClass })
                   .Select(g => new
                   {
                       g.Key.GroupConceptNo,
                       g.Key.ConceptDate,
                       g.Key.BuyerID,
                       g.Key.Buyer,
                       g.Key.BuyerTeamID,
                       g.Key.BuyerTeam,
                       g.Key.SubGroupID,
                       g.Key.SubGroupName,
                       g.Key.KnittingType,
                       g.Key.TechnicalName,
                       g.Key.TechnicalNameId,
                       g.Key.MCSubClassID,
                       g.Key.MCSubClass,
                       TotalQty = g.Sum(s => s.TotalQty),
                       PlanQty = g.Sum(s => s.TotalQty),
                       //BalanceQty = g.Sum(s => s.TotalPlanedQty), ///Because Total Plan Qty is balance qty in query
                       BalanceQty = 0,
                       oItemList = g.ToList()
                   }).ToList();
                if (newGroupList.Count > 0)
                {
                    knittingPlanGroup.ConceptNo = newGroupList[0].GroupConceptNo;
                    knittingPlanGroup.ConceptDate = newGroupList[0].ConceptDate;
                    knittingPlanGroup.BuyerID = newGroupList[0].BuyerID;
                    knittingPlanGroup.Buyer = newGroupList[0].Buyer;
                    knittingPlanGroup.BuyerTeamID = newGroupList[0].BuyerTeamID;
                    knittingPlanGroup.BuyerTeam = newGroupList[0].BuyerTeam;
                    knittingPlanGroup.SubGroupID = newGroupList[0].SubGroupID;
                    knittingPlanGroup.SubGroupName = newGroupList[0].SubGroupName;
                    knittingPlanGroup.KnittingType = newGroupList[0].KnittingType;
                    knittingPlanGroup.TechnicalName = newGroupList[0].TechnicalName;
                    knittingPlanGroup.TechnicalNameId = newGroupList[0].TechnicalNameId;
                    knittingPlanGroup.MCSubClassID = newGroupList[0].MCSubClassID;
                    knittingPlanGroup.MCSubClass = newGroupList[0].MCSubClass;
                    knittingPlanGroup.TotalQty = newGroupList[0].TotalQty;
                    knittingPlanGroup.PlanQty = newGroupList[0].PlanQty;
                    //knittingPlanGroup.PlanQty = newGroupList[0].TotalQty - newGroupList[0].BalanceQty;
                    knittingPlanGroup.BalanceQty = newGroupList[0].BalanceQty;
                }
                return knittingPlanGroup;
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
        public async Task<List<KnittingPlanYarn>> GetYarns(string conceptIds, bool isBulkPage, bool withoutOB, string subGroupName)
        {
            string query = $@"
            ;With FCMRC AS 
            (
	            Select FCMRC.FCMRChildID, FCMRC.ItemMasterID, FCMRC.YarnCategory, FCMRC.YD, FCMRC.YDItem, FCM.ConceptID
	            FROM {TableNames.RND_FREE_CONCEPT_MR_CHILD} AS FCMRC
	            INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_MASTER} MR ON MR.FCMRMasterID = FCMRC.FCMRMasterID
	            INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = MR.ConceptID
	            WHERE MR.ConceptID IN ({conceptIds}) AND FCMRC.YD = 0
            ),
            YD AS 
            (
	            Select FCMRC.FCMRChildID, FCMRC.ItemMasterID, FCMRC.YD, FCMRC.YDItem, PM.BatchNo, FCM.ConceptID, FCMRC.YarnCategory
	            FROM {TableNames.RND_FREE_CONCEPT_MR_CHILD} AS FCMRC
	            INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_MASTER} MR ON MR.FCMRMasterID = FCMRC.FCMRMasterID
	            INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = MR.ConceptID
	            Inner Join {TableNames.YD_BOOKING_MASTER} YBM On YBM.GroupConceptNo = FCM.GroupConceptNo
	            Inner Join {TableNames.YDBookingChild} YBC On YBC.YDBookingMasterID = YBM.YDBookingMasterID 
									             And YBC.ItemMasterID = FCMRC.ItemMasterId 
									             And YBC.YD = FCMRC.YD And YBC.YDItem = FCMRC.YDItem
	            LEFT JOIN {TableNames.YD_PRODUCTION_MASTER} PM ON PM.YDBookingMasterID = YBM.YDBookingMasterID
	            LEFT JOIN {TableNames.YD_PRODUCTION_CHILD} YPC ON YPC.YDProductionMasterID = PM.YDProductionMasterID And YPC.ItemMasterID = YBC.ItemMasterID	 
	            WHERE MR.ConceptID IN ({conceptIds}) AND FCMRC.YD = 1 AND PM.IsAcknowledge = 1 And Isnull(YPC.ItemMasterID,0) != 0
            ),
            A AS
            (
	            SELECT DISTINCT FCMRC.FCMRChildID, FCMRC.ItemMasterID, FCMRC.YarnCategory, ISV2.SegmentValueID YarnTypeID, ISV2.SegmentValue YarnType, 
	            ISV6.SegmentValueID YarnCountID, ISV6.SegmentValue YarnCount, FCMRC.YD, FCMRC.YDItem,'' AS BatchNo, FCMRC.ConceptID, ISV1.SegmentValue Composition 
	            FROM FCMRC
	            INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = FCMRC.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
	            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
	            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
	            UNION
	            SELECT DISTINCT YD.FCMRChildID, YD.ItemMasterID, YD.YarnCategory, ISV2.SegmentValueID YarnTypeID, ISV2.SegmentValue YarnType, 
	            ISV6.SegmentValueID YarnCountID, ISV6.SegmentValue YarnCount, YD.YD, YD.YDItem, YD.BatchNo, YD.ConceptID, ISV1.SegmentValue Composition
	            FROM YD
	            INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YD.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
	            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
	            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
            ),
            R AS
            (
	            SELECT ConceptID, FCMRChildID, YarnTypeID,YarnType,YarnCountID,YarnCount,YD,YDItem,BatchNo, ItemMasterId, Composition, YarnCategory
	            FROM A 
	            GROUP BY ConceptID, FCMRChildID, YarnTypeID,YarnType,YarnCountID,YarnCount,YD,YDItem,BatchNo, ItemMasterId, Composition, YarnCategory
            )
            SELECT * FROM R;";

            return await _service.GetDataAsync<KnittingPlanYarn>(query);
        }
        public async Task<List<KnittingPlanYarn>> GetYarnsForFCMRChild(string conceptIds, bool isBulkPage, bool withoutOB, string subGroupName)
        {
            string query = $@"
            ;With FCMRC AS 
            (
	            Select FCMRC.ItemMasterID, FCMRC.YarnCategory,  FCMRC.FCMRChildID, FCMRC.YD, FCMRC.YDItem, FCMRC.ReqQty, FCM.ConceptID, FCM.TotalQty
	            FROM {TableNames.RND_FREE_CONCEPT_MR_CHILD} AS FCMRC
	            INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_MASTER} MR ON MR.FCMRMasterID = FCMRC.FCMRMasterID
	            INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = MR.ConceptID
	            WHERE MR.ConceptID IN ({conceptIds}) AND FCMRC.YD = 0
            ),
            YD AS 
            (
	            Select Distinct FCMRC.ItemMasterID, FCMRC.YD, FCMRC.YDItem, PM.BatchNo , 
	            FCMRC.YarnCategory, FCMRC.FCMRChildID, FCM.ConceptID, FCMRC.ReqQty, FCM.TotalQty
	            FROM {TableNames.RND_FREE_CONCEPT_MR_CHILD} FCMRC
	            INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_MASTER} MR ON MR.FCMRMasterID = FCMRC.FCMRMasterID
	            INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = MR.ConceptID
	            Inner Join {TableNames.YD_BOOKING_MASTER} YBM On YBM.GroupConceptNo = FCM.GroupConceptNo
	            Inner Join {TableNames.YDBookingChild} YBC On YBC.YDBookingMasterID = YBM.YDBookingMasterID 
	            And YBC.ItemMasterID = FCMRC.ItemMasterId 
	            LEFT JOIN {TableNames.YD_PRODUCTION_MASTER} PM ON PM.YDBookingMasterID = YBM.YDBookingMasterID
	            LEFT JOIN {TableNames.YD_PRODUCTION_CHILD} YPC ON YPC.YDProductionMasterID = PM.YDProductionMasterID And YPC.ItemMasterID = YBC.ItemMasterID	 
	            WHERE MR.ConceptID IN ({conceptIds}) AND FCMRC.YD = 1 
            )
            SELECT DISTINCT FCMRC.ItemMasterID, FCMRC.YarnCategory, ISV2.SegmentValueID YarnTypeID, ISV2.SegmentValue YarnType, FCMRC.FCMRChildID, FCMRC.ConceptID,
            ISV6.SegmentValueID YarnCountID, ISV6.SegmentValue YarnCount, FCMRC.YD, FCMRC.YDItem,'' AS BatchNo, ISV1.SegmentValue Composition, FCMRC.ReqQty, FCMRC.TotalQty
            FROM FCMRC
            INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = FCMRC.ItemMasterID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
            UNION
            SELECT DISTINCT YD.ItemMasterID, YD.YarnCategory, ISV2.SegmentValueID YarnTypeID, ISV2.SegmentValue YarnType, YD.FCMRChildID, YD.ConceptID,
            ISV6.SegmentValueID YarnCountID, ISV6.SegmentValue YarnCount, YD.YD, YD.YDItem, YD.BatchNo, ISV1.SegmentValue Composition, YD.ReqQty, YD.TotalQty
            FROM YD
            INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YD.ItemMasterID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID;";

            return await _service.GetDataAsync<KnittingPlanYarn>(query);
        }
        public async Task<KnittingPlanMaster> GetNewAsync(KnittingProgramType type, int id, int itemMasterId)
        {
            string query;
            if (type == KnittingProgramType.Bulk)
            {
                query = $@"
                With -- Master Data
                YBC As 
                (
	                Select * From {TableNames.YarnBookingChild_New} Where YBookingID = {id} And ItemMasterID = {itemMasterId}
                )

                SELECT BM.BookingNo, YBM.YBookingNo, YBM.YBookingDate BookingDate, YBM.BuyerID, YBM.BuyerTeamID, 
                ISNULL(Buyer.ShortName,'') Buyer, ISNULL(BuyerTeam.TeamName,'') BuyerTeam, SM.StyleNo, CS.ShortName SeasonName, EV.ValueName Status, 
                YBM.CompanyID, YBM.ExportOrderID, CS.SeasonID, YBM.ContactPerson ContactID, EOM.ExportOrderNo EWONo, 
                C.CompanyName Company, YBM.SubGroupID, ISG.SubGroupName
                FROM YBC
                INNER JOIN {TableNames.YarnBookingMaster_New} YBM ON YBC.YBookingID = YBM.YBookingID
				INNER JOIN {DbNames.EPYSL}..BookingMaster BM ON YBM.BookingID = BM.BookingID
                INNER JOIN {DbNames.EPYSL}..Contacts Buyer ON YBM.BuyerID = Buyer.ContactID
                INNER JOIN {DbNames.EPYSL}..ContactCategoryTeam BuyerTeam ON YBM.BuyerTeamID = BuyerTeam.CategoryTeamID
                INNER JOIN {DbNames.EPYSL}..ExportOrderMaster EOM ON YBM.ExportOrderID = EOM.ExportOrderID
                INNER JOIN {DbNames.EPYSL}..StyleMaster SM ON EOM.StyleMasterID = SM.StyleMasterID
                INNER JOIN {DbNames.EPYSL}..ContactSeason CS ON SM.SeasonID = CS.SeasonID
				INNER JOIN {DbNames.EPYSL}..CompanyEntity C ON YBM.CompanyID = C.CompanyID
                INNER JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID = YBM.SubGroupID
                LEFT JOIN {TableNames.Knitting_Plan_Master} KPM ON YBM.YBookingNo = KPM.YBookingNo
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue EV ON KPM.Status = EV.ValueID

                ;With -- Child Data
                YBC As 
                (
	                Select * From {TableNames.YarnBookingChild_New} Where YBookingID = {id} And ItemMasterID = {itemMasterId}
                )

                Select YBC.YBookingID, IM.ItemMasterID, IM.SubGroupID, IM.ItemName, YBC.BookingQty, U.DisplayUnitDesc UOM,
                CASE WHEN IM.SubGroupID=1 THEN G.SegmentValue ELSE 0 END FabricGsm ,CASE WHEN IM.SubGroupID=1 THEN W.SegmentValue ELSE 0 END FabricWidth,
                '**<< NEW >>**' AS  KJobCardNo
                FROM YBC
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON YBC.ItemMasterID = IM.ItemMasterID
                INNER JOIN {DbNames.EPYSL}..Unit U ON YBC.BookingUnitID = U.UnitID
				LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue G ON IM.Segment4ValueID = G.SegmentValueID
				LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue W ON IM.Segment5ValueID = W.SegmentValueID;

                ;With -- Child Yarn Data
                YBC As 
                (
	                Select * From {TableNames.YarnBookingChild_New} Where YBookingID = {id} And ItemMasterID = {itemMasterId}
                )

                SELECT YBCI.YItemMasterID ItemMasterID,IsNull(ISV3.SegmentValueID,0) YarnTypeID, IsNull(ISV12.ChildID,0) YarnCountID,
                IsNull(ISV3.SegmentValue,'') YarnType,IsNull(ISV12.DisplayValue,'') YarnCount, YIPD.LotNo YarnLotNo, 
                C.ShortName YarnBrand, IM.ItemName
                FROM YBC
                INNER JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBC.YBChildID = YBCI.YBChildID
                LEFT JOIN {TableNames.YarnItemPriceDetails} YIPD ON YBCI.YBChildItemID = YIPD.YBChildItemID
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON YBCI.YItemMasterID = IM.ItemMasterID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentDisplayValueYarnCount ISV12 ON ISV12.ChildID = IM.Segment12ValueID
                LEFT JOIN {DbNames.EPYSL}..Contacts C ON YIPD.SpinnerID = C.ContactID;

                -- Subclass
                -- Selected all subclass because there is no relation with ItemMaster yet.
                SELECT CAST(MachineSubClassID AS varchar) [id], SubClassName [text]  FROM {TableNames.KNITTING_MACHINE}
                INNER JOIN {TableNames.KNITTING_MACHINE_SUBCLASS} ON SubClassID = MachineSubClassID
                GROUP BY MachineSubClassID, SubClassName

                ----Knitting Type
                Select CAST(a.SegmentValueID AS varchar) [id], a.SegmentValue [text]
                From {DbNames.EPYSL}..ItemSegmentValue a
                Inner Join {DbNames.EPYSL}..ItemSegmentName b On b.SegmentNameID = a.SegmentNameID
                where SegmentName = 'Knitting Type' and a.SegmentValue <> 'Both';

                {CommonQueries.GetSpinner()}";
            }
            else // BDS
            {
                query = $@"
                With -- Master Data
                FBCD As (
	                Select *
	                From {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD_DETAILS}
	                Where BookingID = {id} And ItemMasterID = {itemMasterId}
                )

                SELECT BM.BookingID,BM.BookingNo,YBM.YBookingID,YBM.YBookingNo, BM.BookingDate, BM.BuyerID, BM.BuyerTeamID, ISNULL(Buyer.ShortName,'') Buyer, ISNULL(BuyerTeam.TeamName,'') BuyerTeam, BM.StyleNo
	                , EV.ValueName Status, BM.ExecutionCompanyID CompanyID, BM.ExportOrderID, CS.SeasonID, BM.BuyerID ContactID, EOM.ExportOrderNo EWONo, C.CompanyName Company,ISG.SubGroupName
                FROM FBCD
                INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FBC ON FBCD.BookingID = FBC.BookingID
				INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} BM ON FBC.BookingID = BM.BookingID
				LEFT JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.BookingID = BM.BookingID
                LEFT JOIN {DbNames.EPYSL}..Contacts Buyer ON BM.BuyerID = Buyer.ContactID
                LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam BuyerTeam ON BM.BuyerTeamID = BuyerTeam.CategoryTeamID
                LEFT JOIN {DbNames.EPYSL}..ExportOrderMaster EOM ON BM.ExportOrderID = EOM.ExportOrderID
                LEFT JOIN {DbNames.EPYSL}..StyleMaster SM ON BM.StyleMasterID = SM.StyleMasterID
                LEFT JOIN {DbNames.EPYSL}..ContactSeason CS ON SM.SeasonID = CS.SeasonID
				LEFT JOIN {DbNames.EPYSL}..CompanyEntity C ON BM.ExecutionCompanyID = C.CompanyID
                LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID = FBC.SubGroupID
                LEFT JOIN {TableNames.Knitting_Plan_Master} KPM ON BM.BookingNo = KPM.BookingNo
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue EV ON KPM.Status = EV.ValueID
				GROUP BY BM.BookingID,BM.BookingNo,YBM.YBookingID,YBM.YBookingNo, BM.BookingDate, BM.BuyerID, BM.BuyerTeamID, ISNULL(Buyer.ShortName,''), ISNULL(BuyerTeam.TeamName''), BM.StyleNo,
	                 EV.ValueName, BM.ExecutionCompanyID, BM.ExportOrderID, CS.SeasonID, BM.BuyerID , EOM.ExportOrderNo, C.CompanyName,ISG.SubGroupName

                ;With -- Child Data
                YBC As (
	                Select *
	                From {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD_DETAILS}
	                 Where BookingID = {id} And ItemMasterID = {itemMasterId}
                )

                Select YBC.BookingID,YBC.ConsumptionID, IM.ItemMasterID, IM.SubGroupID, IM.ItemName, YBC.BookingQty, U.DisplayUnitDesc UOM,CASE WHEN IM.SubGroupID=1 THEN G.SegmentValue ELSE 0 END FabricGsm ,CASE WHEN IM.SubGroupID=1 THEN W.SegmentValue ELSE 0 END FabricWidth,'**<< NEW >>**' AS  KJobCardNo
                FROM YBC
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON YBC.ItemMasterID = IM.ItemMasterID
                INNER JOIN {DbNames.EPYSL}..Unit U ON YBC.BookingUnitID = U.UnitID
				LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue G ON IM.Segment4ValueID = G.SegmentValueID
				LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue W ON IM.Segment5ValueID = W.SegmentValueID;

                ;With -- Child Yarn Data
                YBC As (
	                Select YBKC.YBChildID
	                From {TableNames.YarnBookingChild_New} YBKC
					INNER JOIN {TableNames.YarnBookingMaster_New} YBKM ON YBKM.YBookingID=YBKC.YBookingID
	               Where YBKM.BookingID = {id} And YBKC.ItemMasterID = {itemMasterId}
                )

                SELECT YBCI.YItemMasterID ItemMasterID,IsNull(ISV3.SegmentValueID,0) YarnTypeID, IsNull(ISV12.ChildID,0) YarnCountID,IsNull(ISV3.SegmentValue,'') YarnType,IsNull(ISV12.DisplayValue,'') YarnCount, YIPD.LotNo YarnLotNo, C.ShortName YarnBrand, IM.ItemName
                FROM YBC
                INNER JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBC.YBChildID = YBCI.YBChildID
                INNER JOIN {TableNames.YarnItemPriceDetails} YIPD ON YBCI.YBChildItemID = YIPD.YBChildItemID
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON YBCI.YItemMasterID = IM.ItemMasterID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentDisplayValueYarnCount ISV12 ON ISV12.ChildID = IM.Segment12ValueID
                INNER JOIN {DbNames.EPYSL}..Contacts C ON YIPD.SpinnerID = C.ContactID;

                -- Subclass
                -- Selected all subclass because there is no relation with ItemMaster yet.
                SELECT CAST(MachineSubClassID AS varchar) [id], SubClassName [text]  FROM {TableNames.KNITTING_MACHINE}
                INNER JOIN {TableNames.KNITTING_MACHINE_SUBCLASS} ON SubClassID = MachineSubClassID
                GROUP BY MachineSubClassID, SubClassName

                ----Knitting Type
                Select CAST(a.SegmentValueID AS varchar) [id], a.SegmentValue [text]
                From {DbNames.EPYSL}..ItemSegmentValue a
                Inner Join {DbNames.EPYSL}..ItemSegmentName b On b.SegmentNameID = a.SegmentNameID
                where SegmentName = 'Knitting Type' and a.SegmentValue <> 'Both';

                {CommonQueries.GetSpinner()}";
            }

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                KnittingPlanMaster data = await records.ReadFirstOrDefaultAsync<KnittingPlanMaster>();
                data.Childs = records.Read<KnittingPlanChild>().ToList();
                data.Yarns = records.Read<KnittingPlanYarn>().ToList();
                data.MCSubClassList = records.Read<Select2OptionModel>().ToList();
                data.KnittingTypeList = records.Read<Select2OptionModel>().ToList();
                data.YarnBrandList = await records.ReadAsync<Select2OptionModel>();
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

        public async Task<List<KnittingPlanMaster>> GetListByMCSubClass(KnittingProgramType type, int mcSubClassId, PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By KPMasterID Desc" : paginationInfo.OrderBy;
            string sql;

            #region Knitting Program for Concept

            if (type == KnittingProgramType.Concept)
            {
                sql = $@"
                    WITH
                    M As 
                    (
	                    Select KP.KPMasterID, KP.ConceptID, KP.RevisionPending, KP.Active, SUM(ISNULL(KPC.BookingQty,0)) PlanQty, 
                        KP.PlanNo, KP.RevisionNo, KP.PreProcessRevNo
	                    FROM {TableNames.Knitting_Plan_Master} KP
	                    LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPMasterID = KP.KPMasterID
                        Where KP.Active = 1 And KP.IsBDS = 0 AND KP.MCSubClassID = {mcSubClassId}
	                    GROUP BY KP.KPMasterID, KP.ConceptID, KP.RevisionPending, KP.Active, KP.PlanNo, KP.RevisionNo, KP.PreProcessRevNo
                    ),
                    F AS 
                    (
                        SELECT KPMasterID, M.RevisionPending, M.ConceptID, FCM.ConceptNo, FCM.ConceptDate, FCM.Qty, FCM.Remarks, 
                        M.PlanQty, M.PlanNo, KnittingType.TypeName KnittingType,Composition.SegmentValue Composition, 
                        Construction.SegmentValue Construction, Technical.TechnicalName, Gsm.SegmentValue Gsm, F.ValueName ConceptForName, 
                        S.ValueName ConceptStatus, M.Active, E.EmployeeName UserName, ISG.SubGroupName, FCM.TotalQty
                        FROM M
                        INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = M.ConceptID
					    INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_MASTER} MR ON MR.ConceptID = M.ConceptID
                        LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID = FCM.KnittingTypeID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = FCM.CompositionID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = FCM.ConstructionID
                        LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME} Technical ON Technical.TechnicalNameId=FCM.TechnicalNameId
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = FCM.GSMID
                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue F ON F.ValueID = FCM.ConceptFor
                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue S ON S.ValueID = FCM.ConceptStatusID
                        LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG On FCM.SubGroupID = ISG.SubGroupID
                        LEFT JOIN  {DbNames.EPYSL}..LoginUser L ON L.UserCode = FCM.AddedBy
                        LEFT Join {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
					    WHERE MR.RevisionNo = M.PreProcessRevNo AND FCM.IsBDS = 0
                    )
					, L As 
                    (
                        SELECT F.KPMasterID, F.RevisionPending, F.ConceptID, F.ConceptNo, F.ConceptDate, F.Qty, F.Remarks, 
                        F.PlanQty, F.PlanNo, F.KnittingType, F.Composition, F.Construction, F.TechnicalName, F.Gsm, F.TotalQty,
                        F.ConceptForName, F.ConceptStatus, F.Active, F.UserName, F.SubGroupName, Count(*) Over() TotalRows 
					    FROM F
					    GROUP BY F.KPMasterID, F.RevisionPending, F.ConceptID, F.ConceptNo, F.ConceptDate, F.Qty, F.Remarks, 
                        F.PlanQty, F.PlanNo, F.KnittingType, F.Composition, F.Construction, F.TechnicalName, F.Gsm, 
                        F.ConceptForName, F.ConceptStatus, F.Active, F.UserName, F.SubGroupName, F.TotalQty
                    )
                    Select * From L ";
                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "ORDER BY ConceptID DESC" : paginationInfo.OrderBy;
            }

            #endregion Knitting Program for Concept

            #region Knitting Program for Bulk

            else if (type == KnittingProgramType.Bulk)
            {
                sql = $@"
                    WITH
                    M As 
                    (
	                    Select KP.KPMasterID, KP.ConceptID, KP.PreProcessRevNo, KP.RevisionPending, KP.Active, SUM(ISNULL(KPC.BookingQty,0)) PlanQty, 
                        KP.PlanNo, KP.RevisionNo, KP.BuyerID, KP.BuyerTeamID, KP.ExportOrderID
	                    FROM {TableNames.Knitting_Plan_Master} KP
	                    LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPMasterID = KP.KPMasterID
                        Where KP.Active = 1 And IsBDS = 2 AND KP.MCSubClassID = {mcSubClassId}
	                    GROUP BY KP.KPMasterID, KP.ConceptID, KP.PreProcessRevNo, KP.RevisionPending, KP.Active, KP.PlanNo, KP.RevisionNo, 
						KP.BuyerID, KP.BuyerTeamID, KP.ExportOrderID
                    ),
                    F AS 
                    (
                        SELECT KPMasterID, M.RevisionPending, M.ConceptID, FCM.ConceptNo, FCM.ConceptDate, FCM.Qty, FCM.Remarks, 
						M.PlanQty, M.PlanNo, KnittingType.TypeName KnittingType,Composition.SegmentValue Composition, 
						Construction.SegmentValue Construction, Technical.TechnicalName, GSM = Case When FCM.SubGroupID = 1 Then Gsm.SegmentValue Else '' End, 
						ColorName = Case When FCM.SubGroupID = 1 Then Color.SegmentValue Else '' End, Size = Case When FCM.SubGroupID <> 1 Then Color.SegmentValue + ' X ' + Gsm.SegmentValue Else '' End, 
						F.ValueName ConceptForName, S.ValueName ConceptStatus, M.Active, E.EmployeeName UserName, ISG.SubGroupName, 
						M.BuyerID, M.BuyerTeamID, M.ExportOrderID, 
                        Buyer = CASE WHEN M.BuyerID > 0 THEN ISNULL(CTO.ShortName,'') ELSE 'R&D' END, 
                        BuyerTeam = CASE WHEN M.BuyerTeamID > 0 THEN ISNULL(CCT.TeamName,'') ELSE 'R&D' END
                        FROM M
                        INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = M.ConceptID
					    INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_MASTER} MR ON MR.ConceptID = M.ConceptID
                        LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID = FCM.KnittingTypeID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = FCM.CompositionID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = FCM.ConstructionID
                        LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME} Technical ON Technical.TechnicalNameId=FCM.TechnicalNameId
						LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = FCM.ItemMasterID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = IM.Segment3ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = IM.Segment4ValueID
                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue F ON F.ValueID = FCM.ConceptFor
                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue S ON S.ValueID = FCM.ConceptStatusID
                        LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG On FCM.SubGroupID = ISG.SubGroupID
                        LEFT JOIN  {DbNames.EPYSL}..LoginUser L ON L.UserCode = FCM.AddedBy
                        LEFT Join {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
						LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = M.BuyerID
						LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = M.BuyerTeamID
					    WHERE MR.RevisionNo = M.PreProcessRevNo AND FCM.IsBDS = 2
                    )
					, L As 
                    (
                        SELECT F.KPMasterID, F.RevisionPending, F.ConceptID, F.ConceptNo, F.ConceptDate, F.Qty, F.Remarks, 
                        F.PlanQty, F.PlanNo, F.KnittingType, F.Composition, F.Construction, F.TechnicalName, F.Gsm, F.Size, 
                        F.ConceptForName, F.ConceptStatus, F.Active, F.UserName, F.SubGroupName, F.BuyerID, F.BuyerTeamID, 
						F.ExportOrderID, F.Buyer, F.BuyerTeam
					    FROM F
					    GROUP BY F.KPMasterID, F.RevisionPending, F.ConceptID, F.ConceptNo, F.ConceptDate, F.Qty, F.Remarks, 
                        F.PlanQty, F.PlanNo, F.KnittingType, F.Composition, F.Construction, F.TechnicalName, F.Gsm, F.Size,
                        F.ConceptForName, F.ConceptStatus, F.Active, F.UserName, F.SubGroupName, F.BuyerID, F.BuyerTeamID, 
						F.ExportOrderID, F.Buyer, F.BuyerTeam
                    )
                    Select *, Count(*) Over() TotalRows  From L ";
                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "ORDER BY ConceptID DESC" : paginationInfo.OrderBy;
            }

            #endregion Knitting Program for Bulk

            #region Knitting Program From BDS

            else
            {
                sql = $@" ;WITH
                    M As 
                    (
	                    Select KP.KPMasterID, KP.ConceptID, KP.PreProcessRevNo, KP.RevisionPending, KP.Active, SUM(ISNULL(KPC.BookingQty,0)) PlanQty, KP.PlanNo, KP.RevisionNo,
						KP.BuyerID, KP.BuyerTeamID
	                    FROM {TableNames.Knitting_Plan_Master} KP
	                    LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPMasterID = KP.KPMasterID
                        Where KP.Active = 1 And KP.IsBDS = 1 AND KP.MCSubClassID = {mcSubClassId}
	                    GROUP BY KP.KPMasterID, KP.ConceptID, KP.PreProcessRevNo, KP.RevisionPending, KP.Active, KP.PlanNo, KP.RevisionNo, KP.BuyerID, KP.BuyerTeamID
                    ),
                    F AS 
                    (
                        SELECT KPMasterID, M.RevisionPending, M.ConceptID, FCM.ConceptNo, FCM.ConceptDate, FCM.Qty, FCM.Remarks, M.PlanQty, M.PlanNo
	                    , KnittingType.TypeName KnittingType,Composition.SegmentValue Composition, Construction.SegmentValue Construction
	                    ,Technical.TechnicalName, GSM = Case When FCM.SubGroupID = 1 Then Gsm.SegmentValue Else '' End, 
						ColorName = Case When FCM.SubGroupID = 1 Then Color.SegmentValue Else C1.SegmentValue End, Size = Case When FCM.SubGroupID <> 1 Then Color.SegmentValue + ' X ' + Gsm.SegmentValue Else '' End, F.ValueName ConceptForName, S.ValueName ConceptStatus, M.Active,E.EmployeeName UserName,
						ISG.SubGroupName, M.BuyerID, M.BuyerTeamID, 
                        Buyer = CASE WHEN M.BuyerID > 0 THEN ISNULL(CTO.ShortName,'') ELSE 'R&D' END, 
                        BuyerTeam = CASE WHEN M.BuyerTeamID > 0 THEN ISNULL(CCT.TeamName,'') ELSE 'R&D' END, 
                        FCM.TotalQty
                        FROM M
                        INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = M.ConceptID
					    INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_MASTER} MR ON MR.ConceptID = M.ConceptID
                        LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = FCM.ItemMasterID
                        LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID = FCM.KnittingTypeID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = FCM.CompositionID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = FCM.ConstructionID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = IM.Segment3ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue C1 ON C1.SegmentValueID = IM.Segment5ValueID
                        LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME} Technical ON Technical.TechnicalNameId=FCM.TechnicalNameId
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = IM.Segment4ValueID
                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue F ON F.ValueID = FCM.ConceptFor
                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue S ON S.ValueID = FCM.ConceptStatusID
                        LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG On FCM.SubGroupID = ISG.SubGroupID
                        LEFT JOIN  {DbNames.EPYSL}..LoginUser L ON L.UserCode = FCM.AddedBy
                        LEFT Join {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
						LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = M.BuyerID
						LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = M.BuyerTeamID
					    WHERE MR.RevisionNo = M.PreProcessRevNo AND FCM.IsBDS = 1
                    )
                    , L As 
                    (
					    SELECT F.KPMasterID, F.RevisionPending, F.ConceptID, F.ConceptNo, F.ConceptDate, F.Qty, F.Remarks, F.PlanQty, 
					    F.PlanNo, F.KnittingType, F.Composition, F.Construction, F.TechnicalName, F.Gsm, F.ColorName, F.Size, F.ConceptForName, F.ConceptStatus, 
					    F.Active, F.UserName, F.SubGroupName, F.BuyerID, F.BuyerTeamID, F.Buyer, F.BuyerTeam, F.TotalQty
					    FROM F
					    GROUP BY F.KPMasterID, F.RevisionPending, F.ConceptID, F.ConceptNo, F.ConceptDate, F.Qty, F.Remarks, F.PlanQty, 
					    F.PlanNo, F.KnittingType, F.Composition, F.Construction, F.TechnicalName, F.Gsm, F.ColorName, F.Size, F.ConceptForName, F.ConceptStatus, 
					    F.Active, F.UserName, F.SubGroupName, F.BuyerID, F.BuyerTeamID, F.Buyer, F.BuyerTeam, F.TotalQty
                    ) 
                    Select *, Count(*) Over() TotalRows From L";
                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "ORDER BY ConceptID DESC" : paginationInfo.OrderBy;
            }

            #endregion Knitting Program From BDS

            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<KnittingPlanMaster>(sql);
        }

        public async Task<KnittingPlanMaster> GetRevisionedListAsync(int id, int ConceptID, string subGroupName)
        {
            string colorQuery = subGroupName == "Fabric" ? "IM.Segment3ValueID" : "IM.Segment5ValueID";
            string query = $@"
                ;WITH
                M As (
	                Select *
	                FROM {TableNames.Knitting_Plan_Master} KP
	                Where KPMasterID = {id}
                )
                SELECT M.KPMasterID, M.BAnalysisID, M.ReqDeliveryDate, FCM.BuyerID, FCM.BuyerTeamID, 1 RevisionPending, FCM.ExportOrderID, M.MerchandiserTeamID, M.StyleNo, M.SeasonID
                , M.Status, ETV.ValueName StatusDesc, M.BookingNo, M.ConceptID, FCM.ConceptNo, FCM.ConceptDate, KnittingType.TypeName KnittingType
                , Composition.SegmentValue Composition, Construction.SegmentValue Construction,Color.SegmentValue ColorName,Technical.TechnicalName, Gsm.SegmentValue Gsm
                , FCM.SubGroupID SubGroupID, ISG.SubGroupName, F.ValueName ConceptForName,S.ValueName ConceptStatus, FCM.Qty, ISNULL(SUM(KPC.BookingQty), 0) PlanQty
                , MR.RevisionNo PreProcessRevNo, M.RevisionNo, M.RevisionDate, M.RevisionBy, M.RevisionReason, M.PlanQty TotalPlanedQty, FCM.MCSubClassID, KMS.SubClassName MCSubClass
                , FCM.ConceptTypeID, FCM.FUPartID, FCM.IsYD, FCM.MachineGauge, FCM.Length, FCM.Width, FCM.GroupConceptNo, FCM.IsBDS, FCM.ItemMasterID,
                ISNULL(BuyerTeam.TeamName,'') BuyerTeam, ISNULL(Buyer.ShortName,'') Buyer, FCM.TotalQty, KPC.KnittingTypeID, MaxQty = FCM.TotalQty,FCM.BookingID,FCM.BookingChildID,M.NeedPreFinishingProcess
                FROM M
                LEFT Join {DbNames.EPYSL}..EntityTypeValue ETV On ETV.ValueID = M.Status
                INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = M.ConceptID
				INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_MASTER} MR ON MR.ConceptID = M.ConceptID
                INNER JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON FCM.SubGroupID = ISG.SubGroupID
                LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPMasterID = M.KPMasterID
                LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID = FCM.KnittingTypeID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = FCM.CompositionID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = FCM.ConstructionID
                LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = FCM.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME} Technical ON Technical.TechnicalNameId=FCM.TechnicalNameId
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = FCM.GSMID
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue F ON F.ValueID = FCM.ConceptFor
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue S ON S.ValueID = FCM.ConceptStatusID
				LEFT JOIN {TableNames.KNITTING_MACHINE_SUBCLASS} KMS ON KMS.SubClassID = FCM.MCSubClassID
				LEFT JOIN {DbNames.EPYSL}..Contacts Buyer ON M.BuyerID = Buyer.ContactID
				LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam BuyerTeam ON M.BuyerTeamID = BuyerTeam.CategoryTeamID
                Group By M.KPMasterID, M.BAnalysisID, M.ReqDeliveryDate, FCM.BuyerID, FCM.BuyerTeamID, FCM.ExportOrderID, M.MerchandiserTeamID, M.StyleNo, M.SeasonID
                , M.Status, ETV.ValueName, M.BookingNo, M.ConceptID, FCM.ConceptNo, FCM.ConceptDate, KnittingType.TypeName, KPC.KnittingTypeID
                , Composition.SegmentValue, Construction.SegmentValue, Color.SegmentValue, Technical.TechnicalName,Gsm.SegmentValue, FCM.SubGroupID, ISG.SubGroupName
                , F.ValueName,S.ValueName, FCM.Qty, MR.RevisionNo, M.RevisionNo, M.RevisionDate, M.RevisionBy, M.RevisionReason, FCM.MCSubClassID
                , KMS.SubClassName, M.PlanQty, FCM.ConceptTypeID, FCM.FUPartID, FCM.IsYD, FCM.MachineGauge, FCM.Length, FCM.Width, FCM.GroupConceptNo, 
                FCM.IsBDS, FCM.ItemMasterID, ISNULL(BuyerTeam.TeamName,''), ISNULL(Buyer.ShortName,''), FCM.TotalQty,FCM.BookingID,FCM.BookingChildID,M.NeedPreFinishingProcess;

                -- Yarn Information
                ;SELECT KPY.KPYarnID,MRC.FCMRChildID,ISV3.SegmentValueID YarnTypeID, ISV6.SegmentValueID YarnCountID,ISV3.SegmentValue YarnType,ISV6.SegmentValue YarnCount, KPY.YarnLotNo,
                    KPY.YarnBrandID, C.ShortName YarnBrand,KPY.YarnPly,MRC.YDItem, KPY.StitchLength,KPY.PhysicalCount,KPY.BatchNo,ISV1.SegmentValue Composition, MRC.YarnCategory, MRC.ItemMasterId,
                    YSS.YarnLotNo, YSS.PhysicalCount, MRC.YarnStockSetId, 
                    IsStockItemFromMR = CASE WHEN MRC.YarnStockSetId > 0 THEN 1 ELSE 0 END, 
                    YarnBrandID = ISNULL(YSS.SpinnerId,0),
                    Spinner = CASE WHEN ISNULL(YSS.SpinnerId,0) > 0 THEN SP.ShortName ELSE '' END
                FROM {TableNames.RND_FREE_CONCEPT_MR_CHILD} MRC
				INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_MASTER} MRM ON MRM.FCMRMasterID = MRC.FCMRMasterID
				LEFT JOIN {TableNames.Knitting_Plan_Yarn} KPY ON KPY.FCMRChildID = MRC.FCMRChildID
                LEFT JOIN {TableNames.YarnStockSet} YSS ON YSS.YarnStockSetId = MRC.YarnStockSetId
                LEFT JOIN {DbNames.EPYSL}..Contacts SP ON SP.ContactID = YSS.SpinnerId
				LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = MRC.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                Left Join {DbNames.EPYSL}..Contacts C ON KPY.YarnBrandID = C.ContactID
                LEFT JOIN {TableNames.YARN_DYEING_BOOKING_CHILD_TWISTING} BCT ON BCT.FCMRChildID = MRC.FCMRChildID
				LEFT JOIN {TableNames.YD_PRODUCTION_CHILD} PC ON PC.YDBCTwistingID = BCT.YDBCTwistingID
				LEFT JOIN  {TableNames.YD_PRODUCTION_MASTER} PM ON PM.YDProductionMasterID = PC.YDProductionMasterID
                WHERE MRM.ConceptID = {ConceptID} AND (KPY.KPMasterID = {id} OR ISNULL(KPY.KPYarnID,0) = 0);

                ----Subclass
                SELECT CAST(MachineSubClassID AS varchar) [id], SubClassName [text]  FROM {TableNames.KNITTING_MACHINE}
                INNER JOIN {TableNames.KNITTING_MACHINE_SUBCLASS} ON SubClassID = MachineSubClassID
                WHERE MachineTypeID = (SELECT KnittingTypeId FROM {TableNames.RND_FREE_CONCEPT_MASTER} WHERE ConceptID = {ConceptID})
                GROUP BY MachineSubClassID, SubClassName;

                ----Childs
                ;With
                KPC As(
	                Select F.KPChildID, F.KPMasterID, F.BAnalysisChildID, F.ItemMasterID, F.MachineGauge, F.MCSubClassID, F.YBookingID, F.SubGroupID, F.StartDate, F.EndDate,
				    F.UnitID, F.BookingQty, F.CCColorID, F.FabricGsm, F.FabricWidth, F.Needle, F.CPI, F.TotalNeedle, F.TotalCourse, F.PlanNo
	                From {TableNames.Knitting_Plan_Child} AS F
	                Where F.KPMasterID ={id}
                )
	            SELECT KPC.KPChildID, KPC.KPMasterID, KPC.BAnalysisChildID, KPC.ItemMasterID, KPC.MachineGauge, KPC.MCSubClassID, KPC.YBookingID, KPC.SubGroupID, KPC.StartDate, KPC.EndDate, KPC.UnitID,
				KPC.BookingQty, UU.DisplayUnitDesc AS UOM, KPC.CCColorID, KPC.FabricGsm, KPC.FabricWidth,KJC.BrandID,KJC.ContactID,KJC.MachineDia,KM.MachineNo KnittingMachineNo,EV.ValueName Brand,
                CC.UnitName Contact,KJC.IsSubContact,KJC.KJobCardMasterID,KJC.KJobCardNo,KJC.KnittingMachineID, MS.SubClassName MCSubClassName,KJC.Remarks, FC.[Length], FC.Width,
                KPC.Needle, KPC.CPI, KPC.TotalNeedle, KPC.TotalCourse, MaxQty = FC.TotalQty
				FROM KPC
                INNER JOIN {TableNames.Knitting_Plan_Master} KP ON KP.KPMasterID = KPC.KPMasterID
                INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FC ON FC.ConceptID = KP.ConceptID
				Inner Join {DbNames.EPYSL}..Unit AS UU On UU.UnitID = KPC.UnitID
				LEFT JOIN {TableNames.KNITTING_JOB_CARD_Master} KJC ON KJC.GroupID = KPC.PlanNo AND KJC.ConceptID = KP.ConceptID AND KJC.GroupID NOT IN (1,0)
				LEFT Join {TableNames.KNITTING_MACHINE} AS KM On KM.KnittingMachineID = KJC.KnittingMachineID
				LEFT JOIN {DbNames.EPYSL}..EntityTypeValue EV ON ValueID = KJC.BrandID
				LEFT JOIN {TableNames.KNITTING_UNIT} CC ON CC.KnittingUnitID = KJC.ContactID
                LEFT JOIN {TableNames.KNITTING_MACHINE_SUBCLASS} MS ON MS.SubClassID = KPC.MCSubClassID
				GROUP BY KPC.KPChildID, KPC.KPMasterID, KPC.BAnalysisChildID, KPC.ItemMasterID, KPC.MachineGauge, KPC.MCSubClassID, KPC.YBookingID,
                KPC.SubGroupID, KPC.StartDate, KPC.EndDate, KPC.UnitID, KPC.BookingQty, UU.DisplayUnitDesc, KPC.CCColorID, KPC.FabricGsm,
                KPC.FabricWidth,KJC.BrandID,KJC.ContactID,KJC.MachineDia,KM.MachineNo,EV.ValueName, CC.UnitName,KJC.IsSubContact,KJC.KJobCardMasterID,KJC.KJobCardNo,
                KJC.KnittingMachineID,MS.SubClassName,KJC.Remarks, KPC.Needle, KPC.CPI, KPC.TotalNeedle, KPC.TotalCourse, FC.[Length], FC.Width, FC.TotalQty;

                ----KJobCard
                ;WITH
                M AS (
	                SELECT * FROM {TableNames.KNITTING_JOB_CARD_Master} WHERE KPChildID IN (SELECT KPChildID FROM {TableNames.Knitting_Plan_Child} WHERE KPMasterID = {id})
                ),
                Res AS (
	                SELECT M.KJobCardMasterID, M.KJobCardNo, M.KJobCardDate, M.KPChildID, M.BAnalysisChildID, M.ItemMasterID, M.IsSubContact, M.ContactID, M.MachineKnittingTypeID,
	                M.KnittingMachineID, M.MachineGauge, M.MachineDia, M.BookingID, M.SubGroupID, M.UnitID, M.BookingQty, M.KJobCardQty, M.Remarks, CC.UnitName Contact,
	                UU.DisplayUnitDesc AS UOM, KM.MachineNo KnittingMachineNo, M.ExportOrderID, M.BuyerID, M.BuyerTeamID, M.ConceptID, KT.TypeName MachineKnittingType, M.BrandID, M.Width,
					EV.ValueName Brand, KPC.MCSubClassID, MS.SubClassName MCSubClassName
	                FROM M
					LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPChildID = M.KPChildID
	                LEFT JOIN {TableNames.KNITTING_UNIT} CC ON CC.KnittingUnitID = M.ContactID
	                LEFT Join {DbNames.EPYSL}..Unit AS UU On UU.UnitID = M.UnitID
	                LEFT Join {TableNames.KNITTING_MACHINE} AS KM On KM.KnittingMachineID = M.KnittingMachineID
					LEFT Join {TableNames.KNITTING_MACHINE_TYPE} AS KT On KT.TypeID = M.MachineKnittingTypeID
					LEFT JOIN {DbNames.EPYSL}..EntityTypeValue EV ON ValueID = M.BrandID
					LEFT JOIN {TableNames.KNITTING_MACHINE_SUBCLASS} MS ON MS.SubClassID = KPC.MCSubClassID
                )
                SELECT  KJobCardMasterID, KJobCardNo, KJobCardDate, KPChildID, BAnalysisChildID, ItemMasterID, IsSubContact, ContactID, MachineKnittingTypeID,
	                KnittingMachineID, MachineGauge, MachineDia, BookingID, SubGroupID, UnitID, BookingQty, KJobCardQty, Remarks, Contact, UOM,
					KnittingMachineNo, ExportOrderID, BuyerID, BuyerTeamID, ConceptID, MachineKnittingType, BrandID, Width, Brand, MCSubClassID, MCSubClassName
	                FROM Res;

                {CommonQueries.GetSpinner()}

                ----Knitting Type
                ;Select CAST(a.SegmentValueID AS varchar) [id], a.SegmentValue [text]
                From {DbNames.EPYSL}..ItemSegmentValue a
                Inner Join {DbNames.EPYSL}..ItemSegmentName b On b.SegmentNameID = a.SegmentNameID
                where SegmentName = 'Knitting Type' and a.SegmentValue <> 'Both';

                -- Machine Information
                SELECT KM.KnittingMachineID,KM.KnittingUnitID,KM.MachineNo,KM.MachineTypeID,KM.MachineSubClassID,
                KM.GG,KM.Dia,KM.BrandID,KM.Capacity,KU.ShortName AS Contact,EV.ValueName AS Brand,
                IsSubContact = Case When C.MappingCompanyID = 0 Then 1 Else 0 End
                from {TableNames.KNITTING_MACHINE} KM
                Left Join {TableNames.KNITTING_UNIT} KU ON KU.KnittingUnitID = KM.KnittingUnitID
                Left Join {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID = KM.BrandID
                Left Join {DbNames.EPYSL}..Contacts C ON C.ContactID = KU.ContactID
                Left Join {DbNames.EPYSL}..ContactAdditionalInfo CAI ON CAI.ContactID = C.ContactID
                Where CAI.InHouse = 1;

                -- SubContract
                SELECT KM.KnittingMachineID,KM.KnittingUnitID,KM.MachineNo,KM.MachineTypeID,KM.MachineSubClassID,
                KM.GG,KM.Dia,KM.BrandID,KM.Capacity,KU.ContactID, KU.ShortName AS Contact,EV.ValueName AS Brand,
                IsSubContact = 1 
                from {TableNames.KNITTING_MACHINE} KM
                Left Join {TableNames.KNITTING_UNIT} KU ON KU.KnittingUnitID = KM.KnittingUnitID
                Left Join {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID = KM.BrandID
                Left Join {DbNames.EPYSL}..Contacts C ON C.ContactID = KU.ContactID
                Left Join {DbNames.EPYSL}..ContactAdditionalInfo CAI ON CAI.ContactID = C.ContactID
                Where CAI.InLand = 1 And C.MappingCompanyID = 0; ";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);

                List<Select2OptionModel> oContactList = new List<Select2OptionModel>();
                KnittingPlanMaster data = await records.ReadFirstOrDefaultAsync<KnittingPlanMaster>();
                Guard.Against.NullObject(data);

                data.Yarns = records.Read<KnittingPlanYarn>().ToList();
                data.MCSubClassList = records.Read<Select2OptionModel>().ToList();

                data.Childs = records.Read<KnittingPlanChild>().ToList();
                var jobCards = records.Read<KJobCardMaster>().ToList();
                data.Childs.ForEach(x => x.KJobCardMasters = jobCards.FindAll(c => c.KPChildID == x.KPChildID));

                data.YarnBrandList = await records.ReadAsync<Select2OptionModel>();
                data.KnittingTypeList = records.Read<Select2OptionModel>().ToList();
                data.KnittingMachines = records.Read<KnittingMachine>().ToList();  // records.ReadAsync<KnittingMachine>();
                data.KnittingSubContracts = records.Read<KnittingMachine>().ToList();
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
        public async Task<KnittingPlanGroup> GetGroupRevisionedListAsync(int id, string groupConceptNo, string subGroupName)
        {
            string colorQuery = subGroupName == "Fabric" ? "IM.Segment3ValueID" : "IM.Segment5ValueID";
            string query = $@"

            Select KPG.GroupID, KPG.MachineGauge, KPG.MachineDia, KPG.IsSubContact, KPG.BrandID, KPG.StartDate, KPG.EndDate, KPG.BrandID, 
            KPG.BuyerTeamID, FCM.SubGroupID, KPG.KnittingTypeID, KPG.Needle, KPG.CPI, FCM.MCSubClassID, KPG.IsAdditional, KPG.ParentGroupID, KPG.AdditionNo, 
            ISG.SubGroupName, KPM.BuyerID, KPM.BuyerTeamID, MaxQty = FCM.TotalQty,
            ConceptNo = KPG.GroupConceptNo,FCM.ConceptDate, KPG.AddedBy, KPG.DateAdded, KPG.UpdatedBy, KPG.DateUpdated, 
            Buyer = CASE WHEN KPM.BuyerID > 0 THEN ISNULL(CTO.ShortName,'') ELSE 'R&D' END, 
            BuyerTeam = CASE WHEN KPM.BuyerTeamID > 0 THEN ISNULL(CCT.TeamName,'') ELSE 'R&D' END, 
            FCM.TotalQty - SUM(ISNULL(KPC.BookingQty,0)) BalanceQty, SUM(ISNULL(KPC.BookingQty,0)) PlanQty, FCM.TotalQty, FCM.Qty, KPM.NeedPreFinishingProcess, KPM.IsSubContact, 
            FCM.TechnicalNameId, Technical.TechnicalName, KnittingType.TypeName KnittingType, KMS.SubClassName MCSubClass, KPC.MachineGauge, KPC.MachineDia
            FROM {TableNames.Knitting_Plan_Group} KPG
            Inner Join {TableNames.Knitting_Plan_Master} KPM On KPM.PlanNo = KPG.GroupID
            INNER JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPMasterID = KPM.KPMasterID
            Inner Join {TableNames.RND_FREE_CONCEPT_MASTER} FCM On FCM.ConceptID = KPM.ConceptID
            Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = FCM.SubGroupID
            LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = KPM.BuyerID
            LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = KPM.BuyerTeamID
            LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME} Technical ON Technical.TechnicalNameId=FCM.TechnicalNameId
            LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID = FCM.KnittingTypeID
            LEFT JOIN {TableNames.KNITTING_MACHINE_SUBCLASS} KMS ON KMS.SubClassID = FCM.MCSubClassID
            WHERE KPG.GroupConceptNo = '{groupConceptNo}' AND KPG.GroupID = {id} AND KPG.GroupID <> 1
            Group By KPG.GroupID, KPG.MachineGauge, KPG.MachineDia, KPG.IsSubContact, FCM.ConceptDate, KPG.BrandID, 
            KPG.BuyerTeamID, KPG.KnittingTypeID, KPG.Needle, KPG.CPI, KPG.IsAdditional, KPG.ParentGroupID, KPG.AdditionNo,
            KPG.StartDate, KPG.EndDate, ISG.SubGroupName, KPM.BuyerID, KPM.BuyerTeamID, FCM.SubGroupID, 
            KPG.GroupConceptNo, KPG.AddedBy, KPG.DateAdded, KPG.UpdatedBy, KPG.DateUpdated, ISNULL(CTO.ShortName,''), ISNULL(CCT.TeamName,''), 
            FCM.TotalQty, FCM.Qty, KPM.NeedPreFinishingProcess, KPM.IsSubContact, FCM.TechnicalNameId, Technical.TechnicalName,
            KnittingType.TypeName, KMS.SubClassName, KPC.MachineGauge, KPC.MachineDia, FCM.MCSubClassID;


            SELECT M.KPMasterID, M.BAnalysisID, M.ReqDeliveryDate, FCM.BuyerID, FCM.BuyerTeamID, 1 RevisionPending, FCM.ExportOrderID, M.MerchandiserTeamID, M.StyleNo, M.SeasonID
            , M.Status, ETV.ValueName StatusDesc, M.BookingNo, M.ConceptID, FCM.ConceptNo, FCM.ConceptDate, KnittingType.TypeName KnittingType
            , Technical.TechnicalName
            , FCM.SubGroupID SubGroupID, ISG.SubGroupName, F.ValueName ConceptForName,S.ValueName ConceptStatus, FCM.Qty, ISNULL(SUM(KPC.BookingQty), 0) PlanQty
            , MR.RevisionNo PreProcessRevNo, M.RevisionNo, M.RevisionDate, M.RevisionBy, M.RevisionReason, M.PlanQty TotalPlanedQty, FCM.MCSubClassID, KMS.SubClassName MCSubClass
            , FCM.ConceptTypeID, FCM.FUPartID, FCM.IsYD, FCM.MachineGauge, FCM.Length, FCM.Width, FCM.GroupConceptNo, FCM.IsBDS, FCM.ItemMasterID,BookingQty = FCM.TotalQty, 
            ISNULL(BuyerTeam.TeamName,'') BuyerTeam, ISNULL(Buyer.ShortName,'') Buyer, FCM.TotalQty, KPC.KnittingTypeID, FU.PartName FUPartName, UU.DisplayUnitDesc Uom,
            MaxQty = FCM.TotalQty,
            Construction = ISV1.SegmentValue, 
            Composition = ISV2.SegmentValue,
            ColorName = CASE WHEN FCM.SubGroupID = 1 THEN ISV3.SegmentValue ELSE ISV5.SegmentValue END, 
            Gsm = CASE WHEN FCM.SubGroupID = 1 THEN ISV4.SegmentValue ELSE '' END, 
            DyeingType = CASE WHEN FCM.SubGroupID = 1 THEN ISV6.SegmentValue ELSE '' END, 
            KnittingType = CASE WHEN FCM.SubGroupID = 1 THEN ISV7.SegmentValue ELSE '' END,
            Length = CASE WHEN FCM.SubGroupID = 1 THEN 0 ELSE CONVERT(decimal(18,2),ISV3.SegmentValue) END,
            Width = CASE WHEN FCM.SubGroupID = 1 THEN 0 ELSE CONVERT(decimal(18,2),ISV4.SegmentValue) END,
            Size = Case When FCM.SubGroupID <> 1 Then CONVERT(varchar(100),FCM.Length) + ' X ' + CONVERT(varchar(100),FCM.Width) ELSE '' END
            FROM {TableNames.Knitting_Plan_Master} M
            INNER JOIN {TableNames.Knitting_Plan_Group} KPG ON KPG.GroupID = M.PlanNo
            LEFT Join {DbNames.EPYSL}..EntityTypeValue ETV On ETV.ValueID = M.Status
            INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = M.ConceptID
            INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_MASTER} MR ON MR.ConceptID = M.ConceptID
            INNER JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON FCM.SubGroupID = ISG.SubGroupID
            LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPMasterID = M.KPMasterID
            LEFT Join {DbNames.EPYSL}..Unit AS UU On UU.UnitID = KPC.UnitID
            LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID = FCM.KnittingTypeID
            LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = FCM.ItemMasterID
            LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME} Technical ON Technical.TechnicalNameId=FCM.TechnicalNameId
            LEFT JOIN {DbNames.EPYSL}..EntityTypeValue F ON F.ValueID = FCM.ConceptFor
            LEFT JOIN {DbNames.EPYSL}..EntityTypeValue S ON S.ValueID = FCM.ConceptStatusID
            LEFT JOIN {TableNames.KNITTING_MACHINE_SUBCLASS} KMS ON KMS.SubClassID = FCM.MCSubClassID
            LEFT JOIN {DbNames.EPYSL}..Contacts Buyer ON M.BuyerID = Buyer.ContactID
            LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam BuyerTeam ON M.BuyerTeamID = BuyerTeam.CategoryTeamID
            LEFT JOIN {DbNames.EPYSL}..FabricUsedPart FU ON FU.FUPartID = FCM.FUPartID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
            WHERE KPG.GroupID = {id} 
            Group By M.KPMasterID, M.BAnalysisID, M.ReqDeliveryDate, FCM.BuyerID, FCM.BuyerTeamID, FCM.ExportOrderID, M.MerchandiserTeamID, M.StyleNo, M.SeasonID
            , M.Status, ETV.ValueName, M.BookingNo, M.ConceptID, FCM.ConceptNo, FCM.ConceptDate, KnittingType.TypeName, KPC.KnittingTypeID
            , Technical.TechnicalName,FCM.SubGroupID, ISG.SubGroupName
            , F.ValueName,S.ValueName, FCM.Qty, MR.RevisionNo, M.RevisionNo, M.RevisionDate, M.RevisionBy, M.RevisionReason, FCM.MCSubClassID
            , KMS.SubClassName, M.PlanQty, FCM.ConceptTypeID, FCM.FUPartID, FCM.IsYD, FCM.MachineGauge, FCM.Length, FCM.Width, FCM.GroupConceptNo, 
            FCM.IsBDS, FCM.ItemMasterID, ISNULL(BuyerTeam.TeamName,''), ISNULL(Buyer.ShortName,''), FCM.TotalQty,FU.PartName, UU.DisplayUnitDesc,
            ISV1.SegmentValue,ISV2.SegmentValue,ISV3.SegmentValue,ISV4.SegmentValue,ISV5.SegmentValue,ISV6.SegmentValue,ISV7.SegmentValue;

            -- Yarn Information
            ;With 
            KPM AS
            (
	            SELECT KPM.ConceptID
	            FROM {TableNames.Knitting_Plan_Master} KPM
	            WHERE KPM.PlanNo = {id}
	            GROUP BY KPM.ConceptID
            ),
            FCMRC AS 
            (
	            Select FCMRC.*
	            FROM {TableNames.RND_FREE_CONCEPT_MR_CHILD} AS FCMRC
	            INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_MASTER} MR ON MR.FCMRMasterID = FCMRC.FCMRMasterID
	            INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = MR.ConceptID
	            INNER JOIN KPM ON KPM.ConceptID = MR.ConceptID
	            WHERE FCMRC.YD = 0
            ),
            YD AS 
            (
	            Select Distinct FCMRC.ItemMasterID, FCMRC.YD, FCMRC.YDItem, PM.BatchNo , FCMRC.YarnCategory
	            FROM {TableNames.RND_FREE_CONCEPT_MR_CHILD} AS FCMRC
	            INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_MASTER} MR ON MR.FCMRMasterID = FCMRC.FCMRMasterID
	            INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = MR.ConceptID
	            Inner Join {TableNames.YD_BOOKING_MASTER} YBM On YBM.GroupConceptNo = FCM.GroupConceptNo
	            Inner Join {TableNames.YDBookingChild} YBC On YBC.YDBookingMasterID = YBM.YDBookingMasterID 
										            And YBC.ItemMasterID = FCMRC.ItemMasterId 
										            --And YBC.YD = FCMRC.YD And YBC.YDItem = FCMRC.YDItem ----off because user want to knitting program before production 
	            LEFT JOIN  {TableNames.YD_PRODUCTION_MASTER} PM ON PM.YDBookingMasterID = YBM.YDBookingMasterID
	            LEFT JOIN  {TableNames.YD_PRODUCTION_CHILD} YPC ON YPC.YDProductionMasterID = PM.YDProductionMasterID And YPC.ItemMasterID = YBC.ItemMasterID	 
	            WHERE MR.ConceptID IN (4819) AND FCMRC.YD = 1 --AND PM.IsAcknowledge = 1 And Isnull(YPC.ItemMasterID,0) != 0
            ),
            UnionList AS
            (
	            SELECT DISTINCT FCMRC.ItemMasterID, FCMRC.YarnCategory, ISV2.SegmentValueID YarnTypeID, ISV2.SegmentValue YarnType, 
	            ISV6.SegmentValueID YarnCountID, ISV6.SegmentValue YarnCount, FCMRC.YD, FCMRC.YDItem,'' AS BatchNo, ISV1.SegmentValue Composition
	            FROM FCMRC
	            INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = FCMRC.ItemMasterID
	            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
	            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
	            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
	            UNION
	            SELECT DISTINCT YD.ItemMasterID, YD.YarnCategory, ISV2.SegmentValueID YarnTypeID, ISV2.SegmentValue YarnType, 
	            ISV6.SegmentValueID YarnCountID, ISV6.SegmentValue YarnCount, YD.YD, YD.YDItem, YD.BatchNo, ISV1.SegmentValue Composition
	            FROM YD
	            INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YD.ItemMasterID
	            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
	            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
	            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
            ),
            FinalList AS
            (
	            SELECT UL.ItemMasterId, UL.YarnCategory, UL.YarnTypeID, UL.YarnType, UL.YarnCountID, UL.YarnCount, UL.YD, UL.YDItem, UL.BatchNo, UL.Composition
	            FROM UnionList UL
	            GROUP BY UL.ItemMasterId, UL.YarnCategory, UL.YarnTypeID, UL.YarnType, UL.YarnCountID, UL.YarnCount, UL.YD, UL.YDItem, UL.BatchNo, UL.Composition
            )
            SELECT * FROM FinalList;

            ----Subclass
            SELECT CAST(MachineSubClassID AS varchar) [id], SubClassName [text]  FROM {TableNames.KNITTING_MACHINE}
            INNER JOIN {TableNames.KNITTING_MACHINE_SUBCLASS} ON SubClassID = MachineSubClassID
            WHERE MachineTypeID = (SELECT TOP(1) KnittingTypeId FROM {TableNames.RND_FREE_CONCEPT_MASTER} FCM
            LEFT JOIN {TableNames.Knitting_Plan_Master} KPM ON KPM.ConceptID = FCM.ConceptID
            WHERE KPM.PlanNo = {id})
            GROUP BY MachineSubClassID, SubClassName;

            ----Childs
             SELECT KPC.KPChildID, KPC.KPMasterID, KPC.BAnalysisChildID, KPC.ItemMasterID, KPC.MachineGauge, KPC.MCSubClassID, KPC.YBookingID, KPC.SubGroupID, KPC.StartDate, KPC.EndDate, KPC.UnitID,
            KPC.BookingQty, UU.DisplayUnitDesc AS UOM, KPC.CCColorID, KPC.FabricGsm, KPC.FabricWidth,KJC.BrandID,KJC.ContactID,KJC.MachineDia,KM.MachineNo KnittingMachineNo,EV.ValueName Brand,
            CC.UnitName Contact,KJC.IsSubContact,KJC.KJobCardMasterID,KJC.KJobCardNo,KJC.KnittingMachineID, MS.SubClassName MCSubClassName,KJC.Remarks, FC.[Length], FC.Width,
            KPC.Needle, KPC.CPI, KPC.TotalNeedle, KPC.TotalCourse,
            ColorName = CASE WHEN FC.SubGroupID = 1 THEN ISV3.SegmentValue ELSE ISV5.SegmentValue END,
            Size = Case When FC.SubGroupID <> 1 Then CONVERT(varchar(100),FC.Length) + ' X ' + CONVERT(varchar(100),FC.Width) ELSE '' END
            FROM {TableNames.Knitting_Plan_Child} KPC
            INNER JOIN {TableNames.Knitting_Plan_Master} KP ON KP.KPMasterID = KPC.KPMasterID
            INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FC ON FC.ConceptID = KP.ConceptID
            Inner Join {DbNames.EPYSL}..Unit AS UU On UU.UnitID = KPC.UnitID
            LEFT JOIN {TableNames.KNITTING_JOB_CARD_Master} KJC ON KJC.GroupID = KPC.PlanNo AND KJC.ConceptID = KP.ConceptID AND KJC.GroupID NOT IN (1,0)
            LEFT Join {TableNames.KNITTING_MACHINE} AS KM On KM.KnittingMachineID = KJC.KnittingMachineID
            LEFT JOIN {DbNames.EPYSL}..EntityTypeValue EV ON ValueID = KJC.BrandID
            LEFT JOIN {TableNames.KNITTING_UNIT} CC ON CC.KnittingUnitID = KJC.ContactID
            LEFT JOIN {TableNames.KNITTING_MACHINE_SUBCLASS} MS ON MS.SubClassID = KPC.MCSubClassID
            LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = FC.ItemMasterID
			LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
			LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
			LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
			LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
			LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
			LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
			LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
            WHERE KPC.PlanNo = {id}
            GROUP BY KPC.KPChildID, KPC.KPMasterID, KPC.BAnalysisChildID, KPC.ItemMasterID, KPC.MachineGauge, KPC.MCSubClassID, KPC.YBookingID,
            KPC.SubGroupID, KPC.StartDate, KPC.EndDate, KPC.UnitID, KPC.BookingQty, UU.DisplayUnitDesc, KPC.CCColorID, KPC.FabricGsm,
            KPC.FabricWidth,KJC.BrandID,KJC.ContactID,KJC.MachineDia,KM.MachineNo,EV.ValueName, CC.UnitName,KJC.IsSubContact,KJC.KJobCardMasterID,KJC.KJobCardNo,
            KJC.KnittingMachineID,MS.SubClassName,KJC.Remarks, KPC.Needle, KPC.CPI, KPC.TotalNeedle, KPC.TotalCourse, FC.[Length], FC.Width,
            ISV3.SegmentValue,ISV5.SegmentValue, FC.SubGroupID;

            ----KJobCard
            ;WITH
            Res AS (
            SELECT M.KJobCardMasterID, M.KJobCardNo, M.KJobCardDate, M.KPChildID, M.BAnalysisChildID, M.ItemMasterID, M.IsSubContact, M.ContactID, M.MachineKnittingTypeID,
            M.KnittingMachineID, M.MachineGauge, M.MachineDia, M.BookingID, M.SubGroupID, M.UnitID, M.BookingQty, M.KJobCardQty, M.Remarks, CC.UnitName Contact,
            UU.DisplayUnitDesc AS UOM, KM.MachineNo KnittingMachineNo, M.ExportOrderID, M.BuyerID, M.BuyerTeamID, M.ConceptID, KT.TypeName MachineKnittingType, M.BrandID, M.Width,
            EV.ValueName Brand, KPC.MCSubClassID, MS.SubClassName MCSubClassName
            FROM {TableNames.KNITTING_JOB_CARD_Master} M
            LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPChildID = M.KPChildID
            LEFT JOIN {TableNames.KNITTING_UNIT} CC ON CC.KnittingUnitID = M.ContactID
            LEFT Join {DbNames.EPYSL}..Unit AS UU On UU.UnitID = M.UnitID
            LEFT Join {TableNames.KNITTING_MACHINE} AS KM On KM.KnittingMachineID = M.KnittingMachineID
            LEFT Join {TableNames.KNITTING_MACHINE_TYPE} AS KT On KT.TypeID = M.MachineKnittingTypeID
            LEFT JOIN {DbNames.EPYSL}..EntityTypeValue EV ON ValueID = M.BrandID
            LEFT JOIN {TableNames.KNITTING_MACHINE_SUBCLASS} MS ON MS.SubClassID = KPC.MCSubClassID
            WHERE M.GroupID = {id}
            )
            SELECT KJobCardMasterID, KJobCardNo, KJobCardDate, KPChildID, BAnalysisChildID, ItemMasterID, IsSubContact, ContactID, MachineKnittingTypeID,
            KnittingMachineID, MachineGauge, MachineDia, BookingID, SubGroupID, UnitID, BookingQty, KJobCardQty, Remarks, Contact, UOM,
            KnittingMachineNo, ExportOrderID, BuyerID, BuyerTeamID, ConceptID, MachineKnittingType, BrandID, Width, Brand, MCSubClassID, MCSubClassName
            FROM Res;

            {CommonQueries.GetSpinner()}

            ----Knitting Type
            ;Select CAST(a.SegmentValueID AS varchar) [id], a.SegmentValue [text]
            From {DbNames.EPYSL}..ItemSegmentValue a
            Inner Join {DbNames.EPYSL}..ItemSegmentName b On b.SegmentNameID = a.SegmentNameID
            where SegmentName = 'Knitting Type' and a.SegmentValue <> 'Both';

            -- Machine Information
            SELECT KM.KnittingMachineID,KM.KnittingUnitID,KM.MachineNo,KM.MachineTypeID,KM.MachineSubClassID,
            KM.GG,KM.Dia,KM.BrandID,KM.Capacity,KU.ShortName AS Contact,EV.ValueName AS Brand,
            IsSubContact = Case When C.MappingCompanyID = 0 Then 1 Else 0 End
            from {TableNames.KNITTING_MACHINE} KM
            Left Join {TableNames.KNITTING_UNIT} KU ON KU.KnittingUnitID = KM.KnittingUnitID
            Left Join {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID = KM.BrandID
            Left Join {DbNames.EPYSL}..Contacts C ON C.ContactID = KU.ContactID
            Left Join {DbNames.EPYSL}..ContactAdditionalInfo CAI ON CAI.ContactID = C.ContactID
            Where CAI.InHouse = 1;

            -- SubContract
            SELECT KM.KnittingMachineID,KM.KnittingUnitID,KM.MachineNo,KM.MachineTypeID,KM.MachineSubClassID,
            KM.GG,KM.Dia,KM.BrandID,KM.Capacity,KU.ContactID, KU.ShortName AS Contact,EV.ValueName AS Brand,
            IsSubContact = 1 
            from {TableNames.KNITTING_MACHINE} KM
            Left Join {TableNames.KNITTING_UNIT} KU ON KU.KnittingUnitID = KM.KnittingUnitID
            Left Join {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID = KM.BrandID
            Left Join {DbNames.EPYSL}..Contacts C ON C.ContactID = KU.ContactID
            Left Join {DbNames.EPYSL}..ContactAdditionalInfo CAI ON CAI.ContactID = C.ContactID
            Where CAI.InLand = 1 And C.MappingCompanyID = 0; 

            SELECT KPY.ItemMasterID, KPY.YarnCountID, KPY.YarnTypeID,
            KPY.YarnLotNo, KPY.YarnBrandID, KPY.YarnPly, KPY.StitchLength,
            KPY.PhysicalCount, KPY.BatchNo, KPY.YDItem, KPY.GroupID
            FROM {TableNames.Knitting_Plan_Yarn} KPY
            WHERE KPY.GroupID = {id}
            GROUP BY KPY.ItemMasterID, KPY.YarnCountID, KPY.YarnTypeID,
            KPY.YarnLotNo, KPY.YarnBrandID, KPY.YarnPly, KPY.StitchLength,
            KPY.PhysicalCount, KPY.BatchNo, KPY.YDItem, KPY.GroupID";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);

                List<Select2OptionModel> oContactList = new List<Select2OptionModel>();
                KnittingPlanGroup data = await records.ReadFirstOrDefaultAsync<KnittingPlanGroup>();
                if (data != null)
                {
                    data.KnittingPlans = records.Read<KnittingPlanMaster>().ToList();
                    Guard.Against.NullObject(data);

                    data.Yarns = records.Read<KnittingPlanYarn>().ToList();
                    data.MCSubClassList = records.Read<Select2OptionModel>().ToList();

                    var childs = records.Read<KnittingPlanChild>().ToList();
                    var jobCards = records.Read<KJobCardMaster>().ToList();
                    //data.Childs.ForEach(x => x.KJobCardMasters = jobCards.FindAll(c => c.KPChildID == x.KPChildID));

                    data.KnittingPlans.ForEach(m =>
                    {
                        m.Childs = childs.Where(c => c.KPMasterID == m.KPMasterID).ToList();
                        m.Childs.ForEach(x => x.KJobCardMasters = jobCards.FindAll(c => c.KPChildID == x.KPChildID));
                    });

                    data.YarnBrandList = records.Read<Select2OptionModel>().ToList();
                    data.KnittingTypeList = records.Read<Select2OptionModel>().ToList();
                    data.KnittingMachines = records.Read<KnittingMachine>().ToList();  // records.ReadAsync<KnittingMachine>();
                    data.KnittingSubContracts = records.Read<KnittingMachine>().ToList();

                    List<KnittingPlanYarn> previousYarns = records.Read<KnittingPlanYarn>().ToList();
                    data.Yarns.ForEach(y =>
                    {
                        KnittingPlanYarn kpy = previousYarns.Find(x => x.ItemMasterID == y.ItemMasterID);
                        if (kpy != null)
                        {
                            y.PhysicalCount = kpy.PhysicalCount;
                            y.YarnLotNo = kpy.YarnLotNo;
                            y.BatchNo = kpy.BatchNo;
                            y.YarnBrandID = kpy.YarnBrandID;
                            y.YarnPly = kpy.YarnPly;
                            y.StitchLength = kpy.StitchLength;
                        }
                    });

                    data.TotalQty = data.KnittingPlans.Sum(x => x.TotalQty);
                    data.PlanQty = data.KnittingPlans.Sum(x => x.BookingQty);
                    data.BalanceQty = data.TotalQty - data.PlanQty;
                }
                else
                {
                    data = new KnittingPlanGroup();
                }
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
        public async Task<KnittingPlanMaster> GetAsync(KnittingProgramType type, int id, string subgroupName)
        {
            string colorQuery = subgroupName == "Fabric" ? "IM.Segment3ValueID" : "IM.Segment5ValueID";
            string query = $@"
                WITH
                M As (
	                Select *
	                FROM {TableNames.Knitting_Plan_Master} KP
	                Where KPMasterID = {id}
                ),
                TotalKPDone AS
                (
	                SELECT M.ConceptID, KPDoneQty = SUM(KPC.BookingQty)
	                FROM {TableNames.Knitting_Plan_Master} KPM
	                INNER JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPMasterID = KPM.KPMasterID
	                INNER JOIN M ON M.ConceptID = KPM.ConceptID
	                INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = M.ConceptID
	                GROUP BY M.ConceptID
                )
                SELECT M.KPMasterID, BAnalysisID, ReqDeliveryDate,
                M.BuyerID, M.BuyerTeamID, M.CompanyID, M.ExportOrderID, M.MerchandiserTeamID, M.StyleNo, M.SeasonID
                ,M.Status, ETV.ValueName StatusDesc, M.BookingNo, M.ConceptID, FCM.ConceptNo, FCM.ConceptDate, FCM.KnittingTypeID
                ,Technical.TechnicalName, M.MCSubClassID, KMS.SubClassName MCSubClass
                , FCM.SubGroupID SubGroupID, ISG.SubGroupName, F.ValueName ConceptForName,S.ValueName ConceptStatus, 

                BookingQty = CASE WHEN M.IsBDS = 2 THEN FCM.Qty ELSE 0 END,
                --Qty = CASE WHEN M.IsBDS = 2 THEN FBC.GreyProdQty ELSE FCM.Qty END,
				Qty = CASE WHEN M.IsBDS = 2 THEN 
				CASE WHEN FCM.SubGroupID = 1 Then FBC.GreyProdQty Else Cast (Round((FBC.BookingQty/FBC.BookingQtyKG)*FBC.GreyProdQty,0) As int) END 
				ELSE FCM.Qty 
				END,
                ISNULL(SUM(KPC.BookingQty), 0) PlanQty
                , YBM.YBookingDate BookingDate,
                ISNULL(BuyerTeam.TeamName,'') BuyerTeam,CS.ShortName SeasonName,
                ISNULL(Buyer.ShortName,'') Buyer, C.CompanyName Company, EOM.ExportOrderNo EWONo,KPC.SubGroupID,
                M.PlanQty TotalPlanedQty, FCM.ConceptTypeID, FCM.FUPartID, FCM.IsYD, FCM.MachineGauge, M.FilePath, M.AttachmentPreviewTemplate, FCM.Length, FCM.Width, TotalQty = CASE WHEN FCM.IsBDS = 2 THEN ISNULL(KPD.KPDoneQty,0) ELSE FCM.TotalQty END,
                M.IsSubContact,M.NeedPreFinishingProcess, MaxQty = CASE WHEN FCM.IsBDS = 2 THEN FCM.Qty - ISNULL(KPD.KPDoneQty,0) + M.PlanQty ELSE FCM.TotalQty END,
                Construction = ISV1.SegmentValue, 
                Composition = CASE WHEN FCM.SubGroupID = 1 THEN ISV2.SegmentValue ELSE '' END,
                ColorName = CASE WHEN FCM.SubGroupID = 1 THEN ISV3.SegmentValue ELSE ISV5.SegmentValue END, 
                GSM = CASE WHEN FCM.SubGroupID = 1 THEN ISV4.SegmentValue ELSE '' END, 
                DyeingType = CASE WHEN FCM.SubGroupID = 1 THEN ISV6.SegmentValue ELSE '' END, 
                KnittingType = CASE WHEN FCM.SubGroupID = 1 THEN ISV7.SegmentValue ELSE '' END,
                Length = CASE WHEN FCM.SubGroupID = 1 THEN 0 ELSE CONVERT(decimal(18,2),ISV3.SegmentValue) END,
                Width = CASE WHEN FCM.SubGroupID = 1 THEN 0 ELSE CONVERT(decimal(18,2),ISV4.SegmentValue) END,
                Size = CASE WHEN FCM.SubGroupID <> 1 THEN ISV3.SegmentValue + ' X ' + ISV4.SegmentValue ELSE '' END
                FROM M
                LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPMasterID = M.KPMasterID
                LEFT JOIN TotalKPDone KPD ON KPD.ConceptID = M.ConceptID
                LEFT Join {DbNames.EPYSL}..EntityTypeValue ETV On ETV.ValueID = M.Status
                Left JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = M.ConceptID
                LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FBC ON FBC.BookingChildID = FCM.BookingChildID AND FBC.BookingID = FCM.BookingID
                Left JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID = KPC.SubGroupID
                LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID = FCM.KnittingTypeID
                LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME} Technical ON Technical.TechnicalNameId=FCM.TechnicalNameId
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue F ON F.ValueID = FCM.ConceptFor
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue S ON S.ValueID = FCM.ConceptStatusID
                LEFT JOIN {TableNames.KNITTING_MACHINE_SUBCLASS} KMS ON KMS.SubClassID = M.MCSubClassID
                LEFT JOIN {TableNames.YarnBookingMaster_New} YBM ON KPC.YBookingID = YBM.YBookingID
                LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam BuyerTeam ON BuyerTeam.CategoryTeamID = FCM.BuyerTeamID
                LEFT JOIN {DbNames.EPYSL}..ExportOrderMaster EOM ON YBM.ExportOrderID = EOM.ExportOrderID
                LEFT JOIN {DbNames.EPYSL}..StyleMaster SM ON EOM.StyleMasterID = SM.StyleMasterID
                LEFT JOIN {DbNames.EPYSL}..ContactSeason CS ON SM.SeasonID = CS.SeasonID
                LEFT JOIN {DbNames.EPYSL}..CompanyEntity C ON YBM.CompanyID = C.CompanyID
                LEFT JOIN {DbNames.EPYSL}..Contacts Buyer ON Buyer.ContactID = FCM.BuyerID

                LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = FCM.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID

                GROUP BY M.KPMasterID, BAnalysisID, ReqDeliveryDate, M.BuyerID, M.BuyerTeamID, M.CompanyID, M.ExportOrderID, M.MerchandiserTeamID, M.StyleNo, M.SeasonID
                ,M.Status, ETV.ValueName, M.BookingNo, M.ConceptID, FCM.ConceptNo, FCM.ConceptDate, FCM.KnittingTypeID
                ,Technical.TechnicalName, M.MCSubClassID, KMS.SubClassName
                , FCM.SubGroupID, ISG.SubGroupName, F.ValueName,S.ValueName, FCM.Qty
                , YBM.YBookingDate,ISNULL(BuyerTeam.TeamName,''),CS.ShortName,ISNULL(Buyer.ShortName,''), C.CompanyName, EOM.ExportOrderNo,KPC.SubGroupID,
                M.PlanQty, FCM.ConceptTypeID, FCM.FUPartID, FCM.IsYD, FCM.MachineGauge, M.FilePath, M.AttachmentPreviewTemplate, FCM.Length, FCM.Width, FCM.TotalQty,
                M.IsSubContact,M.NeedPreFinishingProcess, FCM.TotalQty,
                ISV1.SegmentValue, ISNULL(KPD.KPDoneQty,0),FCM.IsBDS,CASE WHEN M.IsBDS = 2 THEN FCM.Qty ELSE 0 END, 
				CASE WHEN M.IsBDS = 2 THEN 
				CASE WHEN FCM.SubGroupID = 1 Then FBC.GreyProdQty Else Cast (Round((FBC.BookingQty/FBC.BookingQtyKG)*FBC.GreyProdQty,0) As int) END 
				ELSE FCM.Qty 
				END,
                CASE WHEN FCM.SubGroupID = 1 THEN ISV2.SegmentValue ELSE '' END,
                CASE WHEN FCM.SubGroupID = 1 THEN ISV3.SegmentValue ELSE ISV5.SegmentValue END, 
                CASE WHEN FCM.SubGroupID = 1 THEN ISV4.SegmentValue ELSE '' END, 
                CASE WHEN FCM.SubGroupID = 1 THEN ISV6.SegmentValue ELSE '' END, 
                CASE WHEN FCM.SubGroupID = 1 THEN ISV7.SegmentValue ELSE '' END,
                CASE WHEN FCM.SubGroupID = 1 THEN 0 ELSE CONVERT(decimal(18,2),ISV3.SegmentValue) END,
                CASE WHEN FCM.SubGroupID = 1 THEN 0 ELSE CONVERT(decimal(18,2),ISV4.SegmentValue) END,
                CASE WHEN FCM.SubGroupID <> 1 THEN ISV3.SegmentValue + ' X ' + ISV4.SegmentValue ELSE '' END;

                -- Yarn Information
                SELECT DISTINCT KPY.KPMasterID, KPY.FCMRChildID,KPY.KPYarnID ,IsNull(KPY.YarnTypeID,0) YarnTypeID, KPY.BatchNo
                    , IsNull(KPY.YarnCountID,0) YarnCountID,ISV2.SegmentValue YarnType,ISV6.SegmentValue  YarnCount, KPY.YarnLotNo
                    , KPY.YarnBrandID, C.ShortName YarnBrand,KPY.YarnPly, KPY.StitchLength, IM.ItemName, KPY.ItemMasterID, 
                KPY.PhysicalCount, KPY.YDItem,ISV1.SegmentValue Composition, FCMRC.YarnCategory,
                IsStockItemFromMR = CASE WHEN FCMRC.YarnStockSetId > 0 THEN 1 ELSE 0 END,
                KPY.YarnStockSetId
                FROM {TableNames.Knitting_Plan_Yarn} KPY
				INNER JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPMasterID=KPY.KPMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = KPY.YarnTypeID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = KPY.YarnCountID
                Left Join {DbNames.EPYSL}..Contacts C ON KPY.YarnBrandID = C.ContactID
				LEFT JOIN {TableNames.RND_FREE_CONCEPT_MR_CHILD} FCMRC ON FCMRC.FCMRChildID = KPY.FCMRChildID
				LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = FCMRC.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
                WHERE KPY.KPMasterID = {id};

                ----Childs
                ;With
                KPC As(
	                Select F.KPChildID, F.KPMasterID, F.BAnalysisChildID, F.ItemMasterID, F.MachineGauge, F.MCSubClassID, F.YBookingID, F.SubGroupID, F.StartDate, F.EndDate,
				    F.UnitID, F.BookingQty, F.CCColorID, F.FabricGsm, F.FabricWidth , F.KnittingTypeID, F.Needle, F.CPI, F.TotalNeedle, F.TotalCourse, F.PlanNo
	                From {TableNames.Knitting_Plan_Child} AS F
	                Where F.KPMasterID ={id}
                )
	            SELECT DISTINCT KPC.KPChildID, KPC.KPMasterID, KPC.BAnalysisChildID, KPC.ItemMasterID, KPC.MachineGauge, KPC.MCSubClassID, KPC.YBookingID, KPC.SubGroupID, KPC.StartDate, KPC.EndDate, KPC.UnitID,
				KPC.BookingQty, MaxQty = FC.TotalQty, UU.DisplayUnitDesc AS UOM, KPC.CCColorID, KPC.FabricGsm, KPC.FabricWidth,KJC.BrandID,KJC.ContactID,KJC.MachineDia,KM.MachineNo KnittingMachineNo,EV.ValueName Brand,
				KJC.IsSubContact,(CASE WHEN KJC.IsSubContact=1 THEN C.ShortName  ELSE CC.UnitName END) AS Contact,KJC.KJobCardNo,KJC.KnittingMachineID,MS.SubClassName MCSubClassName,KJC.Remarks, KPC.KnittingTypeID, KT.SegmentValue KnittingType,IM.ItemName
                , KPC.Needle, KPC.CPI, KPC.TotalNeedle, KPC.TotalCourse, FC.FUPartID, FU.PartName FUPartName, FC.[Length], FC.Width
                FROM KPC
				INNER JOIN {TableNames.Knitting_Plan_Master} KP ON KP.KPMasterID = KPC.KPMasterID
				INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FC ON FC.ConceptID = KP.ConceptID
				Inner Join {DbNames.EPYSL}..Unit AS UU On UU.UnitID = KPC.UnitID
				LEFT JOIN {TableNames.KNITTING_JOB_CARD_Master} KJC ON KJC.GroupID = KPC.PlanNo AND KJC.ConceptID = KP.ConceptID AND KJC.GroupID NOT IN (1,0)
				LEFT Join {TableNames.KNITTING_MACHINE} AS KM On KM.KnittingMachineID = KJC.KnittingMachineID
				LEFT JOIN {DbNames.EPYSL}..EntityTypeValue EV ON ValueID = KJC.BrandID
				LEFT JOIN {TableNames.KNITTING_UNIT} CC ON CC.KnittingUnitID = KJC.ContactID
				LEFT JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = KJC.ContactID
                LEFT JOIN {TableNames.KNITTING_MACHINE_SUBCLASS} MS ON MS.SubClassID = KPC.MCSubClassID
				LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue KT ON KT.SegmentValueID = KPC.KnittingTypeID
                LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON KPC.ItemMasterID = IM.ItemMasterID
				LEFT JOIN {DbNames.EPYSL}..FabricUsedPart FU ON FU.FUPartID = FC.FUPartID

                ----KJobCard
                ;WITH
                M AS (
	                SELECT * FROM {TableNames.KNITTING_JOB_CARD_Master} WHERE KPChildID IN (SELECT KPChildID FROM {TableNames.Knitting_Plan_Child} WHERE KPMasterID = {id})
                ),
                Res AS (
	                SELECT M.KJobCardMasterID, M.KJobCardNo, M.KJobCardDate, M.KPChildID, M.BAnalysisChildID, M.ItemMasterID, M.IsSubContact, M.ContactID, M.MachineKnittingTypeID,
	                M.KnittingMachineID, M.MachineGauge, M.MachineDia, M.BookingID, M.SubGroupID, M.UnitID, M.BookingQty, M.KJobCardQty, M.Remarks, CC.UnitName Contact,
	                UU.DisplayUnitDesc AS UOM, KM.MachineNo KnittingMachineNo, M.ExportOrderID, M.BuyerID, M.BuyerTeamID, M.ConceptID, KT.TypeName MachineKnittingType, M.BrandID, M.Width,
					EV.ValueName Brand, KPC.MCSubClassID, MS.SubClassName MCSubClassName
	                FROM M
					LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPChildID = M.KPChildID
	                LEFT JOIN {TableNames.KNITTING_UNIT} CC ON CC.KnittingUnitID = M.ContactID
	                LEFT Join {DbNames.EPYSL}..Unit AS UU On UU.UnitID = M.UnitID
	                LEFT Join {TableNames.KNITTING_MACHINE} AS KM On KM.KnittingMachineID = M.KnittingMachineID
					LEFT Join {TableNames.KNITTING_MACHINE_TYPE} AS KT On KT.TypeID = M.MachineKnittingTypeID
					LEFT JOIN {DbNames.EPYSL}..EntityTypeValue EV ON ValueID = M.BrandID
					LEFT JOIN {TableNames.KNITTING_MACHINE_SUBCLASS} MS ON MS.SubClassID = KPC.MCSubClassID
                )
                SELECT KJobCardMasterID, KJobCardNo, KJobCardDate, KPChildID, BAnalysisChildID, ItemMasterID, IsSubContact, ContactID, MachineKnittingTypeID,
	                KnittingMachineID, MachineGauge, MachineDia, BookingID, SubGroupID, UnitID, BookingQty, KJobCardQty, Remarks, Contact, UOM,
					KnittingMachineNo, ExportOrderID, BuyerID, BuyerTeamID, ConceptID, MachineKnittingType, BrandID, Width, Brand, MCSubClassID, MCSubClassName
	                FROM Res;

                {CommonQueries.GetSpinner()}

                ;
                -- SubContract for IsSubContact=0
                 --SELECT KM.KnittingMachineID,KM.KnittingUnitID,KM.MachineNo,KM.MachineTypeID,KM.MachineSubClassID,KM.GG,KM.Dia,KM.BrandID,KM.Capacity,KU.ShortName AS Contact,EV.ValueName AS Brand
                --from KnittingMachine KM
                --Left Join KnittingUnit KU ON KU.KnittingUnitID = KM.KnittingUnitID
                --Left Join {DbNames.EPYSL}..EntityTypeValue EV ON ValueID = KM.BrandID;

                SELECT KM.KnittingMachineID,KM.KnittingUnitID,KM.MachineNo,
                KM.MachineTypeID,KM.MachineSubClassID,KM.GG,KM.Dia,KM.BrandID,KM.Capacity,
                KU.ContactID, EV.ValueID, KU.ShortName AS Contact,EV.ValueName AS Brand
                from {TableNames.KNITTING_MACHINE} KM
                INNER Join {TableNames.KNITTING_UNIT} KU ON KU.KnittingUnitID = KM.KnittingUnitID
                INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = KU.ContactID
                Left Join {DbNames.EPYSL}..EntityTypeValue EV ON ValueID = KM.BrandID
                WHERE C.MappingCompanyID > 0

               

            -- SubContract for IsSubContact=1
            --SELECT KM.KnittingMachineID,KM.KnittingUnitID,KM.MachineNo,KM.MachineTypeID,KM.MachineSubClassID,
            --KM.GG,KM.Dia,KM.BrandID,KM.Capacity,KU.ContactID, KU.ShortName AS Contact,EV.ValueName AS Brand,
            --IsSubContact = 1 
            --from {TableNames.KNITTING_MACHINE} KM
            --Left Join KnittingUnit KU ON KU.KnittingUnitID = KM.KnittingUnitID
            --Left Join {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID = KM.BrandID
            --Left Join {DbNames.EPYSL}..Contacts C ON C.ContactID = KU.ContactID
            --Left Join {DbNames.EPYSL}..ContactAdditionalInfo CAI ON CAI.ContactID = C.ContactID
            --Where CAI.InLand = 1 And C.MappingCompanyID = 0;

            SELECT KM.KnittingMachineID,KM.KnittingUnitID,KM.MachineNo,
            KM.MachineTypeID,KM.MachineSubClassID,KM.GG,KM.Dia,KM.BrandID,KM.Capacity,
            KU.ContactID, EV.ValueID, KU.ShortName AS Contact,EV.ValueName AS Brand, IsSubContact = 1 
            from {TableNames.KNITTING_MACHINE} KM
            INNER Join {TableNames.KNITTING_UNIT} KU ON KU.KnittingUnitID = KM.KnittingUnitID
            Left Join {DbNames.EPYSL}..EntityTypeValue EV ON ValueID = KM.BrandID
            INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = KU.ContactID
            INNER JOIN {DbNames.EPYSL}..ContactCategoryChild CCC ON CCC.ContactID = KU.ContactID
            INNER JOIN {DbNames.EPYSL}..ContactCategoryHK CCHK ON CCHK.ContactCategoryID = CCC.ContactCategoryID
            Left Join {DbNames.EPYSL}..ContactAdditionalInfo CAI ON CAI.ContactID = C.ContactID
            WHERE CCHK.ContactCategoryName = 'Knitting Sub Contractor' AND C.MappingCompanyID = 0
            AND CAI.InLand = 1
            ";

            if (type == KnittingProgramType.Concept)
            {
                query += $@"

                ----Knitting Type
                Select CAST(a.SegmentValueID AS varchar) [id], a.SegmentValue [text]
                From {DbNames.EPYSL}..ItemSegmentValue a
                Inner Join {DbNames.EPYSL}..ItemSegmentName b On b.SegmentNameID = a.SegmentNameID
                where SegmentName = 'Knitting Type' and a.SegmentValue <> 'Both';

                --Machine Type list
                SELECT CAST(a.MachineSubClassID AS varchar) [id], b.SubClassName [text], b.TypeID [desc], c.TypeName additionalValue
				FROM {TableNames.KNITTING_MACHINE} a
				INNER JOIN {TableNames.KNITTING_MACHINE_SUBCLASS} b ON b.SubClassID = a.MachineSubClassID
				Inner Join {TableNames.KNITTING_MACHINE_TYPE} c On c.TypeID = b.TypeID
				Where a.MachineSubClassID in(SELECT FTN.SubClassID
				FROM {TableNames.FABRIC_TECHNICAL_NAME} TN
				INNER JOIN {TableNames.FABRIC_TECHNICAL_NAME_KMACHINE_SUB_CLASS} FTN ON FTN.TechnicalNameID=TN.TechnicalNameId
				WHERE TN.TechnicalNameId = (Select TechnicalNameId From {TableNames.RND_FREE_CONCEPT_MASTER} FC
				INNER JOIN {TableNames.Knitting_Plan_Master} KP ON FC.ConceptID = KP.ConceptID
				Where KP.KPMasterID = {id}))
				GROUP BY a.MachineSubClassID, b.SubClassName, b.TypeID, c.TypeName
                ";
            }
            else if (type == KnittingProgramType.Bulk || type == KnittingProgramType.BDS)
            {
                query += $@"
                ----Subclass
                SELECT CAST(MachineSubClassID AS varchar) [id], SubClassName [text]  FROM {TableNames.KNITTING_MACHINE}
                INNER JOIN {TableNames.KNITTING_MACHINE_SUBCLASS} ON SubClassID = MachineSubClassID
                GROUP BY MachineSubClassID, SubClassName;

                 ----Knitting Type
                Select CAST(a.SegmentValueID AS varchar) [id], a.SegmentValue [text]
                From {DbNames.EPYSL}..ItemSegmentValue a
                Inner Join {DbNames.EPYSL}..ItemSegmentName b On b.SegmentNameID = a.SegmentNameID
                where SegmentName = 'Knitting Type' and a.SegmentValue <> 'Both';";
            }

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                KnittingPlanMaster data = await records.ReadFirstOrDefaultAsync<KnittingPlanMaster>();
                Guard.Against.NullObject(data);

                data.Yarns = records.Read<KnittingPlanYarn>().ToList();
                data.Childs = records.Read<KnittingPlanChild>().ToList();
                List<KJobCardMaster> jobCards = records.Read<KJobCardMaster>().ToList();
                data.Childs.ForEach(x => x.KJobCardMasters = jobCards.FindAll(c => c.KPChildID == x.KPChildID));

                data.YarnBrandList = await records.ReadAsync<Select2OptionModel>();
                data.KnittingMachines = records.Read<KnittingMachine>().ToList();
                data.KnittingSubContracts = records.Read<KnittingMachine>().ToList();

                if (type == KnittingProgramType.Concept)
                {
                    data.KnittingTypeList = await records.ReadAsync<Select2OptionModel>();
                    data.MachineTypeList = records.Read<Select2OptionModel>().ToList();
                }
                else if (type == KnittingProgramType.Bulk || type == KnittingProgramType.BDS)
                {
                    data.MCSubClassList = await records.ReadAsync<Select2OptionModel>();
                    data.KnittingTypeList = await records.ReadAsync<Select2OptionModel>();
                }

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

        public async Task<KnittingPlanGroup> GetAdditionGroupAsync(string groupConceptNo, int planNo, KnittingProgramType type, string subgroupName)
        {
            string colorQuery = subgroupName == "Fabric" ? "IM.Segment3ValueID" : "IM.Segment5ValueID";
            string query = $@"
                            WITH MainObj AS
                            (
	                            Select KPG.GroupID, KPG.MachineGauge, KPG.MachineDia, KPG.IsSubContact, KPG.BrandID, KPG.StartDate, KPG.EndDate, 
	                            KPG.BuyerTeamID, FCM.SubGroupID, KPG.KnittingTypeID, KPG.Needle, KPG.CPI,
	                            ISG.SubGroupName, KPM.BuyerID, KPG.IsAdditional, KPG.ParentGroupID, KPG.AdditionNo,
	                            ConceptNo = KPG.GroupConceptNo,FCM.ConceptDate, KPG.AddedBy, KPG.DateAdded, KPG.UpdatedBy, KPG.DateUpdated, 
	                            Buyer = CASE WHEN KPM.BuyerID > 0 THEN ISNULL(CTO.ShortName,'') ELSE 'R&D' END, 
	                            BuyerTeam = CASE WHEN KPG.BuyerTeamID > 0 THEN ISNULL(CCT.TeamName,'') ELSE 'R&D' END, 
	                            FCM.TotalQty - SUM(ISNULL(KPC.BookingQty,0)) BalanceQty, SUM(ISNULL(KPC.BookingQty,0)) PlanQty, FCM.TotalQty, FCM.Qty, KPM.NeedPreFinishingProcess,
	                            FCM.TechnicalNameId, Technical.TechnicalName, KnittingType.TypeName KnittingType, KMS.SubClassName MCSubClass
	                            --MaxQty = FCM.TotalQty
	                            FROM {TableNames.Knitting_Plan_Group} KPG
	                            Inner Join {TableNames.Knitting_Plan_Master} KPM On KPM.PlanNo = KPG.GroupID
	                            INNER JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPMasterID = KPM.KPMasterID
	                            Inner Join {TableNames.RND_FREE_CONCEPT_MASTER} FCM On FCM.ConceptID = KPM.ConceptID
	                            Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = FCM.SubGroupID
	                            LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = KPM.BuyerID
	                            LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = KPM.BuyerTeamID
	                            LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME} Technical ON Technical.TechnicalNameId=FCM.TechnicalNameId
	                            LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID = FCM.KnittingTypeID
	                            LEFT JOIN {TableNames.KNITTING_MACHINE_SUBCLASS} KMS ON KMS.SubClassID = FCM.MCSubClassID
	                            WHERE KPG.GroupConceptNo = '{groupConceptNo}' AND KPG.GroupID = {planNo}
	                            Group By KPG.GroupID, KPG.MachineGauge, KPG.MachineDia, KPG.IsSubContact, FCM.ConceptDate, KPG.BrandID, 
	                            KPG.BuyerTeamID, KPG.KnittingTypeID, KPG.IsAdditional, KPG.ParentGroupID, KPG.AdditionNo, KPG.Needle, KPG.CPI,
	                            KPG.StartDate, KPG.EndDate, ISG.SubGroupName, KPM.BuyerID, FCM.SubGroupID, 
	                            KPG.GroupConceptNo, KPG.AddedBy, KPG.DateAdded, KPG.UpdatedBy, KPG.DateUpdated, ISNULL(CTO.ShortName,''), ISNULL(CCT.TeamName,''), 
	                            FCM.TotalQty, FCM.Qty, KPM.NeedPreFinishingProcess, FCM.TechnicalNameId, Technical.TechnicalName,
	                            KnittingType.TypeName, KMS.SubClassName
                            ),
                            TotalQtyList AS
                            (
	                            SELECT KPG.ParentGroupID, TotalQty = SUM(ISNULL(KPG.TotalQty,0))
	                            FROM {TableNames.Knitting_Plan_Group} KPG
	                            INNER JOIN MainObj MO ON MO.ParentGroupID = KPG.ParentGroupID AND MO.ParentGroupID > 0
	                            WHERE KPG.GroupID <> MO.GroupID
	                            GROUP BY KPG.ParentGroupID
                            ),
                            FinalObj AS
                            (
	                            SELECT MO.*, MaxQty = MO.TotalQty - ISNULL(TQL.TotalQty,0), BookingQty =  MO.TotalQty - ISNULL(TQL.TotalQty,0)
	                            FROM MainObj MO
	                            LEFT JOIN TotalQtyList TQL ON TQL.ParentGroupID = MO.ParentGroupID
                            )
                            SELECT * FROM FinalObj;

                            WITH MainObj AS
                            (
	                            SELECT M.*, FCM.ConceptNo, FCM.ConceptDate,  FCM.Qty, FCM.Remarks, FCM.Length, FCM.Width,
                                KnittingType.TypeName KnittingType,Composition.SegmentValue Composition, UU.DisplayUnitDesc Uom, G.ParentGroupID, G.GroupID, --MaxQty = FCM.TotalQty,
                                Construction.SegmentValue Construction, Technical.TechnicalName, Gsm.SegmentValue Gsm, F.ValueName ConceptForName, 
                                S.ValueName ConceptStatus, E.EmployeeName UserName, ISG.SubGroupID, ISG.SubGroupName, FCM.TotalQty, KMS.SubClassName MCSubClass,
                                ColorName = Case When FCM.SubGroupID = 1 Then Color.SegmentValue Else C1.SegmentValue End,
                                Size = Case When FCM.SubGroupID <> 1 Then CONVERT(varchar(100),FCM.Length) + ' X ' + CONVERT(varchar(100),FCM.Width) ELSE '' END
                                FROM {TableNames.Knitting_Plan_Master} M
                                INNER JOIN {TableNames.Knitting_Plan_Group} G ON G.GroupID = M.PlanNo
                                LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPMasterID = M.KPMasterID
                                LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = M.ConceptID
                                LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID = FCM.KnittingTypeID
                                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = FCM.CompositionID
                                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = FCM.ConstructionID
                                LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME} Technical ON Technical.TechnicalNameId=FCM.TechnicalNameId
                                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = FCM.GSMID
                                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue F ON F.ValueID = FCM.ConceptFor
                                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue S ON S.ValueID = FCM.ConceptStatusID
                                LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG On FCM.SubGroupID = ISG.SubGroupID
                                LEFT JOIN {DbNames.EPYSL}..LoginUser L ON L.UserCode = FCM.AddedBy
                                LEFT Join {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
                                LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = FCM.ItemMasterID
                                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = IM.Segment3ValueID
                                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue C1 ON C1.SegmentValueID = IM.Segment5ValueID
                                LEFT JOIN {TableNames.KNITTING_MACHINE_SUBCLASS} KMS ON KMS.SubClassID = M.MCSubClassID
                                LEFT Join {DbNames.EPYSL}..Unit AS UU On UU.UnitID = KPC.UnitID
                                WHERE G.GroupConceptNo = '{groupConceptNo}' AND M.PlanNo = {planNo}
                            ),
                            TotalQtyList AS
                            (
	                            SELECT KPG.ParentGroupID, TotalQty = SUM(ISNULL(KPG.TotalQty,0))
	                            FROM {TableNames.Knitting_Plan_Group} KPG
	                            INNER JOIN MainObj MO ON MO.ParentGroupID = KPG.ParentGroupID AND MO.ParentGroupID > 0
	                            WHERE KPG.GroupID <> MO.GroupID
	                            GROUP BY KPG.ParentGroupID
                            ),
                            FinalObj AS
                            (
	                            SELECT MO.*, MaxQty = MO.TotalQty - ISNULL(TQL.TotalQty,0), BookingQty =  MO.TotalQty - ISNULL(TQL.TotalQty,0)
	                            FROM MainObj MO
	                            LEFT JOIN TotalQtyList TQL ON TQL.ParentGroupID = MO.ParentGroupID
                            )
                            SELECT * FROM FinalObj;

                            WITH MainObj AS
                            (
	                            SELECT KPChildID,C.KPMasterID,C.BAnalysisChildID,C.ItemMasterID,C.MachineGauge,C.MachineDia,C.SubGroupID,
	                            C.StartDate,C.EndDate,C.ActualStartDate,C.ActualEndDate,C.UnitID,C.KJobCardQty,C.CCColorID,
	                            C.FabricGsm,C.FabricWidth,C.MCSubClassID,C.YBookingID,C.BookingID,C.KnittingTypeID,C.Remarks,
	                            C.Needle,C.CPI,C.TotalNeedle,C.TotalCourse,C.PlanNo, G.ParentGroupID, G.GroupID, FCM.TotalQty
                                FROM {TableNames.Knitting_Plan_Child} C
                                INNER JOIN {TableNames.Knitting_Plan_Master} M ON M.KPMasterID = C.KPMasterID
                                INNER JOIN {TableNames.Knitting_Plan_Group} G ON G.GroupID = M.PlanNo
                                INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = M.ConceptID
                                WHERE G.GroupConceptNo = '{groupConceptNo}' AND M.PlanNo = {planNo}

                            ),
                            TotalQtyList AS
                            (
	                            SELECT KPG.ParentGroupID, TotalQty = SUM(ISNULL(KPG.TotalQty,0))
	                            FROM {TableNames.Knitting_Plan_Group} KPG
	                            INNER JOIN MainObj MO ON MO.ParentGroupID = KPG.ParentGroupID AND MO.ParentGroupID > 0
	                            WHERE KPG.GroupID <> MO.GroupID
	                            GROUP BY KPG.ParentGroupID
                            ),
                            FinalObj AS
                            (
	                            SELECT MO.*, MaxQty = MO.TotalQty - ISNULL(TQL.TotalQty,0), BookingQty =  MO.TotalQty - ISNULL(TQL.TotalQty,0)
	                            FROM MainObj MO
	                            LEFT JOIN TotalQtyList TQL ON TQL.ParentGroupID = MO.ParentGroupID
                            )
                            SELECT * FROM FinalObj;

                            SELECT Y.YarnCountID, Y.YarnTypeID, Y.YarnLotNo, Y.YarnBrandID, Y.YarnPly, Y.StitchLength, Y.PhysicalCount, Y.YDItem, Y.BatchNo, Y.ItemMasterID,
                            ISV6.SegmentValue YarnCount, ISV2.SegmentValue YarnType, ISV1.SegmentValue Composition, KPYC.YarnCategory
                            FROM {TableNames.Knitting_Plan_Yarn} Y
                            INNER JOIN {TableNames.Knitting_Plan_Master} M ON M.KPMasterID = Y.KPMasterID
                            INNER JOIN {TableNames.Knitting_Plan_Group} G ON G.GroupID = M.PlanNo
                            INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = Y.ItemMasterID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                            LEFT JOIN (
	                            SELECT FCMRChildID = MIN(KPYC.FCMRChildID), KPYC.KPYarnID, FCMRC.YarnCategory
	                            FROM {TableNames.Knitting_Plan_Yarn_Child} KPYC
	                            INNER JOIN {TableNames.Knitting_Plan_Yarn} KPY ON KPY.KPYarnID = KPYC.KPYarnID
	                            LEFT JOIN {TableNames.RND_FREE_CONCEPT_MR_CHILD} FCMRC ON FCMRC.FCMRChildID = KPYC.FCMRChildID
	                            WHERE KPY.GroupID = {planNo}
	                            GROUP BY KPYC.KPYarnID, FCMRC.YarnCategory
                            ) KPYC ON KPYC.KPYarnID = Y.KPYarnID
                            WHERE G.GroupConceptNo = '{groupConceptNo}' AND M.PlanNo = {planNo}
                            GROUP BY Y.YarnCountID, Y.YarnTypeID, Y.YarnLotNo, Y.YarnBrandID, Y.YarnPly, Y.StitchLength, Y.PhysicalCount, Y.YDItem, Y.BatchNo, Y.ItemMasterID,
                            ISV6.SegmentValue, ISV2.SegmentValue, ISV1.SegmentValue, KPYC.YarnCategory;

                            {CommonQueries.GetSpinner()}

                            ;SELECT KM.KnittingMachineID,KM.KnittingUnitID,KM.MachineNo,KM.MachineTypeID,KM.MachineSubClassID,KM.GG,KM.Dia,KM.BrandID,KM.Capacity,KU.ShortName AS Contact,EV.ValueName AS Brand
                            from {TableNames.KNITTING_MACHINE} KM
                            Left Join {TableNames.KNITTING_UNIT} KU ON KU.KnittingUnitID = KM.KnittingUnitID
                            Left Join {DbNames.EPYSL}..EntityTypeValue EV ON ValueID = KM.BrandID;

                            -- SubContract
                            SELECT KM.KnittingMachineID,KM.KnittingUnitID,KM.MachineNo,KM.MachineTypeID,KM.MachineSubClassID,
                            KM.GG,KM.Dia,KM.BrandID,KM.Capacity,KU.ContactID, KU.ShortName AS Contact,EV.ValueName AS Brand,
                            IsSubContact = 1 
                            from {TableNames.KNITTING_MACHINE} KM
                            Left Join {TableNames.KNITTING_UNIT} KU ON KU.KnittingUnitID = KM.KnittingUnitID
                            Left Join {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID = KM.BrandID
                            Left Join {DbNames.EPYSL}..Contacts C ON C.ContactID = KU.ContactID
                            Left Join {DbNames.EPYSL}..ContactAdditionalInfo CAI ON CAI.ContactID = C.ContactID
                            Where CAI.InLand = 1 And C.MappingCompanyID = 0; ";


            if (type == KnittingProgramType.Concept)
            {
                query += $@"

                ----Knitting Type
                Select CAST(a.SegmentValueID AS varchar) [id], a.SegmentValue [text]
                From {DbNames.EPYSL}..ItemSegmentValue a
                Inner Join {DbNames.EPYSL}..ItemSegmentName b On b.SegmentNameID = a.SegmentNameID
                where SegmentName = 'Knitting Type' and a.SegmentValue <> 'Both';";
            }
            else if (type == KnittingProgramType.Bulk || type == KnittingProgramType.BDS)
            {
                query += $@"
                ----Subclass
                SELECT CAST(MachineSubClassID AS varchar) [id], SubClassName [text]  FROM {TableNames.KNITTING_MACHINE}
                INNER JOIN {TableNames.KNITTING_MACHINE_SUBCLASS} ON SubClassID = MachineSubClassID
                GROUP BY MachineSubClassID, SubClassName;

                 ----Knitting Type
                Select CAST(a.SegmentValueID AS varchar) [id], a.SegmentValue [text]
                From {DbNames.EPYSL}..ItemSegmentValue a
                Inner Join {DbNames.EPYSL}..ItemSegmentName b On b.SegmentNameID = a.SegmentNameID
                where SegmentName = 'Knitting Type' and a.SegmentValue <> 'Both';";
            }

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                KnittingPlanGroup data = await records.ReadFirstOrDefaultAsync<KnittingPlanGroup>();
                Guard.Against.NullObject(data);

                data.KnittingPlans = records.Read<KnittingPlanMaster>().ToList();
                var childs = records.Read<KnittingPlanChild>().ToList();
                var yarns = records.Read<KnittingPlanYarn>().ToList();

                if (childs.Count() > 0)
                {
                    if (data.SubGroupID == 0) data.SubGroupID = childs.First().SubGroupID;
                    if (data.SubGroupID == 1 && data.KnittingTypeID == 0)
                    {
                        data.KnittingTypeID = childs.First().KnittingTypeID;
                    }
                    else if ((data.SubGroupID == 11 || data.SubGroupID == 12) && (data.Needle == 0 || data.CPI == 0))
                    {
                        data.Needle = childs.First().Needle;
                        data.CPI = childs.First().CPI;
                    }
                }

                data.KnittingPlans.ForEach(x =>
                {
                    data.MCSubClassID = x.MCSubClassID;
                    data.IsSubContact = x.IsSubContact;
                    var child = childs.Find(c => c.KPMasterID == x.KPMasterID);
                    if (child != null)
                    {
                        data.MachineDia = child.MachineDia;
                        data.MachineGauge = child.MachineGauge;

                        x.KnittingTypeID = child.KnittingTypeID;
                        x.MachineDia = child.MachineDia;
                        x.MachineGauge = child.MachineGauge;
                        x.BookingQty = child.BookingQty;
                        x.Remarks = child.Remarks;
                    }
                });

                //knittingPlan.Childs = childs;
                data.Yarns = yarns;
                data.YarnBrandList = records.Read<Select2OptionModel>().ToList();
                data.KnittingMachines = records.Read<KnittingMachine>().ToList();
                data.KnittingSubContracts = records.Read<KnittingMachine>().ToList();

                if (type == KnittingProgramType.Concept)
                {
                    data.KnittingTypeList = records.Read<Select2OptionModel>().ToList();
                }
                else if (type == KnittingProgramType.Bulk || type == KnittingProgramType.BDS)
                {
                    data.MCSubClassList = records.Read<Select2OptionModel>().ToList();
                    data.KnittingTypeList = records.Read<Select2OptionModel>().ToList();
                }

                decimal totalQty = 0,
                    planQty = 0;

                data.KnittingPlans.ForEach(m =>
                {
                    totalQty += m.TotalQty;
                    planQty += m.BookingQty;

                    m.KnittingTypeList = data.KnittingTypeList;
                    var child = childs.Find(x => x.KPMasterID == m.KPMasterID);
                    if (child != null)
                    {
                        m.TotalNeedle = child.TotalNeedle;
                        m.TotalCourse = child.TotalCourse;
                        m.KnittingTypeID = child.KnittingTypeID;
                    }
                });
                data.TotalQty = totalQty;
                data.PlanQty = planQty;
                data.BalanceQty = totalQty - planQty;
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

        public async Task<KnittingPlanGroup> GetGroupAsync(string groupConceptNo, int planNo, KnittingProgramType type, string subgroupName)
        {
            string colorQuery = subgroupName == "Fabric" ? "IM.Segment3ValueID" : "IM.Segment5ValueID";
            string query = $@"
                            Select KPG.GroupID, KPG.MachineGauge, KPG.MachineDia, KPG.IsSubContact, KPG.BrandID, KPG.StartDate, KPG.EndDate, KPG.BrandID, 
							KPG.BuyerTeamID, FCM.SubGroupID, KPG.KnittingTypeID, KPG.Needle, KPG.CPI,
							ISG.SubGroupName, KPM.BuyerID, KPM.BuyerTeamID, KPG.IsAdditional, KPG.ParentGroupID, KPG.AdditionNo,
				            ConceptNo = KPG.GroupConceptNo,FCM.ConceptDate, KPG.AddedBy, KPG.DateAdded, KPG.UpdatedBy, KPG.DateUpdated, 
                            Buyer = CASE WHEN KPM.BuyerID > 0 THEN ISNULL(CTO.ShortName,'') ELSE 'R&D' END, 
                            BuyerTeam = CASE WHEN KPM.BuyerTeamID > 0 THEN ISNULL(CCT.TeamName,'') ELSE 'R&D' END, 
                            FCM.TotalQty - SUM(ISNULL(KPC.BookingQty,0)) BalanceQty, SUM(ISNULL(KPC.BookingQty,0)) PlanQty, FCM.TotalQty, FCM.Qty, KPM.NeedPreFinishingProcess, KPM.IsSubContact, 
                            FCM.TechnicalNameId, Technical.TechnicalName, KnittingType.TypeName KnittingType, KMS.SubClassName MCSubClass, KPC.MachineGauge, KPC.MachineDia,
                            MaxQty = FCM.TotalQty
				            FROM {TableNames.Knitting_Plan_Group} KPG
				            Inner Join {TableNames.Knitting_Plan_Master} KPM On KPM.PlanNo = KPG.GroupID
                            INNER JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPMasterID = KPM.KPMasterID
				            Inner Join {TableNames.RND_FREE_CONCEPT_MASTER} FCM On FCM.ConceptID = KPM.ConceptID
				            Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = FCM.SubGroupID
				            LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = KPM.BuyerID
				            LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = KPM.BuyerTeamID
							LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME} Technical ON Technical.TechnicalNameId=FCM.TechnicalNameId
                            LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID = FCM.KnittingTypeID
                            LEFT JOIN {TableNames.KNITTING_MACHINE_SUBCLASS} KMS ON KMS.SubClassID = FCM.MCSubClassID
                            WHERE KPG.GroupConceptNo = '{groupConceptNo}' AND KPG.GroupID = {planNo}
				            Group By KPG.GroupID, KPG.MachineGauge, KPG.MachineDia, KPG.IsSubContact, FCM.ConceptDate, KPG.BrandID, 
							KPG.BuyerTeamID, KPG.KnittingTypeID, KPG.IsAdditional, KPG.ParentGroupID, KPG.AdditionNo, KPG.Needle, KPG.CPI,
							KPG.StartDate, KPG.EndDate, ISG.SubGroupName, KPM.BuyerID, KPM.BuyerTeamID, FCM.SubGroupID, 
				            KPG.GroupConceptNo, KPG.AddedBy, KPG.DateAdded, KPG.UpdatedBy, KPG.DateUpdated, ISNULL(CTO.ShortName,''), ISNULL(CCT.TeamName,''), 
                            FCM.TotalQty, FCM.Qty, KPM.NeedPreFinishingProcess, KPM.IsSubContact, FCM.TechnicalNameId, Technical.TechnicalName,
							KnittingType.TypeName, KMS.SubClassName, KPC.MachineGauge, KPC.MachineDia;

                            SELECT M.*, FCM.ConceptNo, FCM.ConceptDate,  FCM.Qty, FCM.Remarks, M.NeedPreFinishingProcess, FCM.Length, FCM.Width,
                            KnittingType.TypeName KnittingType,Composition.SegmentValue Composition, UU.DisplayUnitDesc Uom, MaxQty = FCM.TotalQty,
                            Construction.SegmentValue Construction, Technical.TechnicalName, Gsm.SegmentValue Gsm, F.ValueName ConceptForName, 
                            S.ValueName ConceptStatus, M.Active, E.EmployeeName UserName, ISG.SubGroupID, ISG.SubGroupName, FCM.TotalQty, FCM.TotalQty BookingQty, KMS.SubClassName MCSubClass,
                            ColorName = Case When FCM.SubGroupID = 1 Then Color.SegmentValue Else C1.SegmentValue End,
                            Size = Case When FCM.SubGroupID <> 1 Then CONVERT(varchar(100),FCM.Length) + ' X ' + CONVERT(varchar(100),FCM.Width) ELSE '' END
                            FROM {TableNames.Knitting_Plan_Master} M
                            INNER JOIN {TableNames.Knitting_Plan_Group} G ON G.GroupID = M.PlanNo
                            LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPMasterID = M.KPMasterID
                            LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = M.ConceptID
                            LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KnittingType ON KnittingType.TypeID = FCM.KnittingTypeID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = FCM.CompositionID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = FCM.ConstructionID
                            LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME} Technical ON Technical.TechnicalNameId=FCM.TechnicalNameId
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = FCM.GSMID
                            LEFT JOIN {DbNames.EPYSL}..EntityTypeValue F ON F.ValueID = FCM.ConceptFor
                            LEFT JOIN {DbNames.EPYSL}..EntityTypeValue S ON S.ValueID = FCM.ConceptStatusID
                            LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG On FCM.SubGroupID = ISG.SubGroupID
                            LEFT JOIN {DbNames.EPYSL}..LoginUser L ON L.UserCode = FCM.AddedBy
                            LEFT Join {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
                            LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = FCM.ItemMasterID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = IM.Segment3ValueID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue C1 ON C1.SegmentValueID = IM.Segment5ValueID
                            LEFT JOIN {TableNames.KNITTING_MACHINE_SUBCLASS} KMS ON KMS.SubClassID = M.MCSubClassID
                            LEFT Join {DbNames.EPYSL}..Unit AS UU On UU.UnitID = KPC.UnitID
                            WHERE G.GroupConceptNo = '{groupConceptNo}' AND M.PlanNo = {planNo};

                            SELECT C.*, MaxQty = FCM.TotalQty 
                            FROM {TableNames.Knitting_Plan_Child} C
                            INNER JOIN {TableNames.Knitting_Plan_Master} M ON M.KPMasterID = C.KPMasterID
                            INNER JOIN {TableNames.Knitting_Plan_Group} G ON G.GroupID = M.PlanNo
                            INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = M.ConceptID
                            WHERE G.GroupConceptNo = '{groupConceptNo}' AND M.PlanNo = {planNo};

                            SELECT Y.YarnCountID, Y.YarnTypeID, Y.YarnLotNo, Y.YarnBrandID, Y.YarnPly, Y.StitchLength, Y.PhysicalCount, Y.YDItem, Y.BatchNo, Y.ItemMasterID,
                            ISV6.SegmentValue YarnCount, ISV2.SegmentValue YarnType, ISV1.SegmentValue Composition, KPYC.YarnCategory
                            FROM {TableNames.Knitting_Plan_Yarn} Y
                            INNER JOIN {TableNames.Knitting_Plan_Master} M ON M.KPMasterID = Y.KPMasterID
                            INNER JOIN {TableNames.Knitting_Plan_Group} G ON G.GroupID = M.PlanNo
                            INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = Y.ItemMasterID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                            LEFT JOIN (
	                            SELECT FCMRChildID = MIN(KPYC.FCMRChildID), KPYC.KPYarnID, FCMRC.YarnCategory
	                            FROM {TableNames.Knitting_Plan_Yarn_Child} KPYC
	                            INNER JOIN {TableNames.Knitting_Plan_Yarn} KPY ON KPY.KPYarnID = KPYC.KPYarnID
	                            LEFT JOIN {TableNames.RND_FREE_CONCEPT_MR_CHILD} FCMRC ON FCMRC.FCMRChildID = KPYC.FCMRChildID
	                            WHERE KPY.GroupID = {planNo}
	                            GROUP BY KPYC.KPYarnID, FCMRC.YarnCategory
                            ) KPYC ON KPYC.KPYarnID = Y.KPYarnID
                            WHERE G.GroupConceptNo = '{groupConceptNo}' AND M.PlanNo = {planNo}
                            GROUP BY Y.YarnCountID, Y.YarnTypeID, Y.YarnLotNo, Y.YarnBrandID, Y.YarnPly, Y.StitchLength, Y.PhysicalCount, Y.YDItem, Y.BatchNo, Y.ItemMasterID,
                            ISV6.SegmentValue, ISV2.SegmentValue, ISV1.SegmentValue, KPYC.YarnCategory;

                            {CommonQueries.GetSpinner()}

                            ;SELECT KM.KnittingMachineID,KM.KnittingUnitID,KM.MachineNo,KM.MachineTypeID,KM.MachineSubClassID,KM.GG,KM.Dia,KM.BrandID,KM.Capacity,KU.ShortName AS Contact,EV.ValueName AS Brand
                            from {TableNames.KNITTING_MACHINE} KM
                            Left Join {TableNames.KNITTING_UNIT} KU ON KU.KnittingUnitID = KM.KnittingUnitID
                            Left Join {DbNames.EPYSL}..EntityTypeValue EV ON ValueID = KM.BrandID;

                            -- SubContract
                            SELECT KM.KnittingMachineID,KM.KnittingUnitID,KM.MachineNo,KM.MachineTypeID,KM.MachineSubClassID,
                            KM.GG,KM.Dia,KM.BrandID,KM.Capacity,KU.ContactID, KU.ShortName AS Contact,EV.ValueName AS Brand,
                            IsSubContact = 1 
                            from {TableNames.KNITTING_MACHINE} KM
                            Left Join {TableNames.KNITTING_UNIT} KU ON KU.KnittingUnitID = KM.KnittingUnitID
                            Left Join {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID = KM.BrandID
                            Left Join {DbNames.EPYSL}..Contacts C ON C.ContactID = KU.ContactID
                            Left Join {DbNames.EPYSL}..ContactAdditionalInfo CAI ON CAI.ContactID = C.ContactID
                            Where CAI.InLand = 1 And C.MappingCompanyID = 0; ";


            if (type == KnittingProgramType.Concept)
            {
                query += $@"

                ----Knitting Type
                Select CAST(a.SegmentValueID AS varchar) [id], a.SegmentValue [text]
                From {DbNames.EPYSL}..ItemSegmentValue a
                Inner Join {DbNames.EPYSL}..ItemSegmentName b On b.SegmentNameID = a.SegmentNameID
                where SegmentName = 'Knitting Type' and a.SegmentValue <> 'Both';";
            }
            else if (type == KnittingProgramType.Bulk || type == KnittingProgramType.BDS)
            {
                query += $@"
                ----Subclass
                SELECT CAST(MachineSubClassID AS varchar) [id], SubClassName [text]  FROM {TableNames.KNITTING_MACHINE}
                INNER JOIN {TableNames.KNITTING_MACHINE_SUBCLASS} ON SubClassID = MachineSubClassID
                GROUP BY MachineSubClassID, SubClassName;

                 ----Knitting Type
                Select CAST(a.SegmentValueID AS varchar) [id], a.SegmentValue [text]
                From {DbNames.EPYSL}..ItemSegmentValue a
                Inner Join {DbNames.EPYSL}..ItemSegmentName b On b.SegmentNameID = a.SegmentNameID
                where SegmentName = 'Knitting Type' and a.SegmentValue <> 'Both';";
            }

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                KnittingPlanGroup data = await records.ReadFirstOrDefaultAsync<KnittingPlanGroup>();
                Guard.Against.NullObject(data);

                data.KnittingPlans = records.Read<KnittingPlanMaster>().ToList();
                var childs = records.Read<KnittingPlanChild>().ToList();
                var yarns = records.Read<KnittingPlanYarn>().ToList();

                if (childs.Count() > 0)
                {
                    if (data.SubGroupID == 0) data.SubGroupID = childs.First().SubGroupID;
                    if (data.SubGroupID == 1 && data.KnittingTypeID == 0)
                    {
                        data.KnittingTypeID = childs.First().KnittingTypeID;
                    }
                    else if ((data.SubGroupID == 11 || data.SubGroupID == 12) && (data.Needle == 0 || data.CPI == 0))
                    {
                        data.Needle = childs.First().Needle;
                        data.CPI = childs.First().CPI;
                    }
                }

                data.KnittingPlans.ForEach(x =>
                {
                    data.MCSubClassID = x.MCSubClassID;
                    data.IsSubContact = x.IsSubContact;
                    var child = childs.Find(c => c.KPMasterID == x.KPMasterID);
                    if (child != null)
                    {
                        data.MachineDia = child.MachineDia;
                        data.MachineGauge = child.MachineGauge;

                        x.KnittingTypeID = child.KnittingTypeID;
                        x.MachineDia = child.MachineDia;
                        x.MachineGauge = child.MachineGauge;
                        x.BookingQty = child.BookingQty;
                        x.Remarks = child.Remarks;
                    }
                });

                //knittingPlan.Childs = childs;
                data.Yarns = yarns;
                data.YarnBrandList = records.Read<Select2OptionModel>().ToList();
                data.KnittingMachines = records.Read<KnittingMachine>().ToList();
                data.KnittingSubContracts = records.Read<KnittingMachine>().ToList();

                if (type == KnittingProgramType.Concept)
                {
                    data.KnittingTypeList = records.Read<Select2OptionModel>().ToList();
                }
                else if (type == KnittingProgramType.Bulk || type == KnittingProgramType.BDS)
                {
                    data.MCSubClassList = records.Read<Select2OptionModel>().ToList();
                    data.KnittingTypeList = records.Read<Select2OptionModel>().ToList();
                }

                decimal totalQty = 0,
                    planQty = 0;

                data.KnittingPlans.ForEach(m =>
                {
                    totalQty += m.TotalQty;
                    planQty += m.BookingQty;

                    m.KnittingTypeList = data.KnittingTypeList;
                    var child = childs.Find(x => x.KPMasterID == m.KPMasterID);
                    if (child != null)
                    {
                        m.TotalNeedle = child.TotalNeedle;
                        m.TotalCourse = child.TotalCourse;
                        m.KnittingTypeID = child.KnittingTypeID;
                    }
                });
                data.TotalQty = totalQty;
                data.PlanQty = planQty;
                data.BalanceQty = totalQty - planQty;
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

        public async Task<KnittingPlanMaster> GetKnittingPlanMasterAsync(int id)
        {
            var query = $@"Select * From {TableNames.Knitting_Plan_Master} Where KPMasterID = {id}";
            return await _service.GetFirstOrDefaultAsync<KnittingPlanMaster>(query);
        }

        public async Task<List<KnittingPlanChild>> GetChildsAsync(int masterId, int subGroupID, int conceptID)
        {
            string sql = $@"
                ;With
                KPC As(
	                Select F.KPChildID, F.KPMasterID, F.BAnalysisChildID, F.ItemMasterID, F.MachineGauge, F.MCSubClassID, F.YBookingID, F.SubGroupID, F.StartDate, F.EndDate,
				    F.UnitID, F.BookingQty, F.CCColorID, F.FabricGsm, F.FabricWidth, F.Needle, F.CPI, F.TotalNeedle, F.TotalCourse, F.PlanNo
	                From {TableNames.Knitting_Plan_Child} AS F
	                Where F.KPMasterID ={masterId}
                )
	            SELECT KPC.KPChildID, KPC.KPMasterID, KPC.BAnalysisChildID, KPC.ItemMasterID, KPC.MachineGauge, KPC.MCSubClassID, KPC.YBookingID, KPC.SubGroupID, KPC.StartDate, KPC.EndDate, KPC.UnitID,
				KPC.BookingQty, UU.DisplayUnitDesc AS UOM, KPC.CCColorID, KPC.FabricGsm, KPC.FabricWidth,KJC.BrandID,KJC.ContactID,KJC.MachineDia,KM.MachineNo KnittingMachineNo,EV.ValueName Brand, CC.UnitName Contact,KJC.IsSubContact,KJC.KJobCardNo,KJC.KnittingMachineID,MS.SubClassName MCSubClassName,KJC.Remarks
                , KPC.Needle, KPC.CPI, KPC.TotalNeedle, KPC.TotalCourse
                FROM KPC
				Inner Join {DbNames.EPYSL}..Unit AS UU On UU.UnitID = KPC.UnitID
				LEFT JOIN {TableNames.KNITTING_JOB_CARD_Master} KJC ON KJC.GroupID = KPC.PlanNo AND KJC.GroupID NOT IN (1,0)
				LEFT Join {TableNames.KNITTING_MACHINE} AS KM On KM.KnittingMachineID = KJC.KnittingMachineID
				LEFT JOIN {DbNames.EPYSL}..EntityTypeValue EV ON ValueID = KJC.BrandID
				LEFT JOIN {TableNames.KNITTING_UNIT} CC ON CC.KnittingUnitID = KJC.ContactID
                LEFT JOIN {TableNames.KNITTING_MACHINE_SUBCLASS} MS ON MS.SubClassID = KPC.MCSubClassID
				GROUP BY KPC.KPChildID, KPC.KPMasterID, KPC.BAnalysisChildID, KPC.ItemMasterID, KPC.MachineGauge, KPC.MCSubClassID, KPC.YBookingID, KPC.SubGroupID, KPC.StartDate, KPC.EndDate, KPC.UnitID,
				KPC.BookingQty, UU.DisplayUnitDesc, KPC.CCColorID, KPC.FabricGsm, KPC.FabricWidth,KJC.BrandID,KJC.ContactID,KJC.MachineDia,KM.MachineNo,EV.ValueName, CC.UnitName,KJC.IsSubContact,
                KJC.KJobCardNo,KJC.KnittingMachineID,MS.SubClassName,KJC.Remarks, KPC.Needle, KPC.CPI, KPC.TotalNeedle, KPC.TotalCourse;";

            return await _service.GetDataAsync<KnittingPlanChild>(sql);
        }
        public async Task<IList<KnittingMachine>> GetMachineByGaugeDia(int MachineGauge, int MachineDia)
        {
            var sql = $@"
                        SELECT KM.KnittingMachineID,KM.KnittingUnitID,KM.MachineNo,KM.MachineTypeID,KM.MachineSubClassID,
                        KM.GG,KM.Dia,KM.BrandID,KM.Capacity,KU.ShortName AS Contact,EV.ValueName AS Brand, KU.ContactID,
                        IsSubContact = 0
                        from {TableNames.KNITTING_MACHINE} KM
                        Left Join {TableNames.KNITTING_UNIT} KU ON KU.KnittingUnitID = KM.KnittingUnitID
                        Left Join {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID = KM.BrandID
                        Left Join {DbNames.EPYSL}..Contacts C ON C.ContactID = KU.ContactID
                        Left Join {DbNames.EPYSL}..ContactAdditionalInfo CAI ON CAI.ContactID = C.ContactID
                        Where CAI.InHouse = 1 And C.MappingCompanyID <> 0 AND KM.GG = {MachineGauge} AND KM.Dia = {MachineDia}";
            return await _service.GetDataAsync<KnittingMachine>(sql);
        }

        public async Task<int> GetKnittingPlanCompletionStatusForBdsAsync(string bookingNo)
        {
            var query = $@"GO
                With
                KP AS (
	                Select Count(Distinct C.ItemMasterID) ItemCount, M.BookingNo
	                From {TableNames.Knitting_Plan_Master} M
	                INNER JOIN {TableNames.Knitting_Plan_Child} C ON M.KPMasterID = C.KPMasterID
	                Where M.BookingNo = '{bookingNo}'
	                Group By M.BookingNo
                )
                , YB AS (
	                Select Count(Distinct(C.ItemMasterID)) ItemCount, M.BookingNo
	                From {TableNames.FBBOOKING_ACKNOWLEDGE} M
	                INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} C ON M.BookingID = C.BookingID
	                Where BookingNo = '{bookingNo}'
	                Group By M.BookingNo
                )
                , ST AS (
	                Select CASE WHEN KP.ItemCount = YB.ItemCount THEN 'Complete' ELSE 'Partially Complete' END AS KP
	                From KP
	                LEFT JOIN YB ON KP.BookingNo = YB.BookingNo
                )

                Select EV.ValueID
                From ST
                INNER JOIN {DbNames.EPYSL}..EntityTypeValue EV ON ST.KP = EV.ValueName
                INNER JOIN {DbNames.EPYSL}..EntityType ET ON EV.EntityTypeID = ET.EntityTypeID
                WHERE ET.EntityTypeName = 'Status'";

            try
            {
                await _connection.OpenAsync();
                return await _connection.QueryFirstAsync<int>(query);
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

        public async Task<int> GetKnittingPlanCompletionStatusAsync(string yBookingNo)
        {
            var query = $@"GO
                With
                KP AS (
	                Select Count(Distinct C.ItemMasterID) ItemCount, M.YBookingNo
	                From {TableNames.Knitting_Plan_Master} M
	                INNER JOIN {TableNames.Knitting_Plan_Child} C ON M.KPMasterID = C.KPMasterID
	                Where M.YBookingNo = '{yBookingNo}'
	                Group By M.YBookingNo
                )
                , YB AS (
	                Select Count(Distinct(C.ItemMasterID)) ItemCount, M.YBookingNo
	                From {TableNames.YarnBookingMaster_New} M
	                INNER JOIN {TableNames.YarnBookingChild_New} C ON M.YBookingID = C.YBookingID
	                Where YBookingNo = '{yBookingNo}'
	                Group By M.YBookingNo
                )
                , ST AS (
	                Select CASE WHEN KP.ItemCount = YB.ItemCount THEN 'Complete' ELSE 'Partially Complete' END AS KP
	                From KP
	                LEFT JOIN YB ON KP.YBookingNo = YB.YBookingNo
                )

                Select EV.ValueID
                From ST
                INNER JOIN {DbNames.EPYSL}..EntityTypeValue EV ON ST.KP = EV.ValueName
                INNER JOIN {DbNames.EPYSL}..EntityType ET ON EV.EntityTypeID = ET.EntityTypeID
                WHERE ET.EntityTypeName = 'Status'";

            try
            {
                await _connection.OpenAsync();
                return await _connection.QueryFirstAsync<int>(query);
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

        public async Task<List<KnittingPlanMaster>> GetDetailsByGroupAsync(int groupId)
        {
            var query = $@"
                -- Plans
                Select M.*, FCM.ConceptNo, FCM.ConceptDate, FCM.KnittingTypeID, FCM.SubGroupID, FCM.GroupConceptNo, FCM.IsBDS, FCM.ItemMasterID
                From {TableNames.Knitting_Plan_Master} M
                INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = M.ConceptID
                Where M.PlanNo = {groupId}

                -- Childs
                Select * From {TableNames.Knitting_Plan_Child} Where PlanNo = {groupId}

                -- Yarns
                SELECT * 
                FROM {TableNames.Knitting_Plan_Yarn} KPY
                INNER JOIN {TableNames.Knitting_Plan_Master} KPM ON KPM.KPMasterID = KPY.KPMasterID
                WHERE KPM.PlanNo = {groupId}

                -- Job Cards
                ;With KPC As (
	                Select * From {TableNames.Knitting_Plan_Child} Where PlanNo = {groupId}
                )
                Select Distinct JCM.*
                From {TableNames.KNITTING_JOB_CARD_Master} JCM
                Inner Join KPC On JCM.KPChildID = KPC.KPChildID

                --KP Group
                SELECT TOP(1) KPG.*
                FROM {TableNames.Knitting_Plan_Group} KPG
                LEFT JOIN {TableNames.Knitting_Plan_Master} KPM ON KPM.PlanNo = KPG.GroupID
                WHERE KPG.GroupID = {groupId}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                List<KnittingPlanMaster> data = records.Read<KnittingPlanMaster>().ToList();
                Guard.Against.NullObject(data);
                var childs = records.Read<KnittingPlanChild>().ToList();
                var yarns = records.Read<KnittingPlanYarn>().ToList();
                var jobCards = records.Read<KJobCardMaster>().ToList();
                var kpGroups = records.Read<KnittingPlanGroup>().ToList();

                //if (kpGroups != null && kpGroups.Count() > 0) data.KPGroup = kpGroups.First();

                foreach (KnittingPlanMaster kpm in data)
                {
                    kpm.Childs = childs.Where(x => x.KPMasterID == kpm.KPMasterID).ToList();
                    kpm.Yarns = yarns.Where(x => x.KPMasterID == kpm.KPMasterID).ToList();
                    foreach (var child in kpm.Childs)
                    {
                        child.KJobCardMasters = jobCards.FindAll(x => x.KPChildID == child.KPChildID);
                    }
                }
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
        public async Task<KnittingPlanMaster> GetDetailsAsync(int id)
        {
            var query = $@"
                -- Plans
                Select M.*, FCM.ConceptNo, FCM.ConceptDate, FCM.KnittingTypeID, FCM.SubGroupID, FCM.GroupConceptNo, FCM.IsBDS, FCM.ItemMasterID
                From {TableNames.Knitting_Plan_Master} M
                INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = M.ConceptID
                Where M.KPMasterID = {id}

                -- Childs
                Select * From {TableNames.Knitting_Plan_Child} Where KPMasterID = {id}

                -- Yarns
                Select * From {TableNames.Knitting_Plan_Yarn} Where KPMasterID = {id}

                -- Yarn Childs
                Select KPYC.*, FCM.TotalQty
                From {TableNames.Knitting_Plan_Yarn_Child} KPYC
                INNER JOIN {TableNames.Knitting_Plan_Yarn} KPY ON KPY.KPYarnID = KPYC.KPYarnID
                INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_CHILD} FCMRC ON FCMRC.FCMRChildID = KPYC.FCMRChildID
                INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_MASTER} MR ON MR.FCMRMasterID = FCMRC.FCMRMasterID
                INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = MR.ConceptID
                Where KPY.KPMasterID = {id}

                -- Job Cards
                ;With KPC As (
	                Select * From {TableNames.Knitting_Plan_Child} Where KPMasterID = {id}
                )
                Select Distinct JCM.*
                From {TableNames.KNITTING_JOB_CARD_Master} JCM
                Inner Join KPC On KPC.PlanNo = JCM.GroupID

                --KP Group
                SELECT TOP(1) KPG.*
                FROM {TableNames.Knitting_Plan_Group} KPG
                LEFT JOIN {TableNames.Knitting_Plan_Master} KPM ON KPM.PlanNo = KPG.GroupID
                WHERE KPM.KPMasterID = {id}

                -- Job Cards Child
                Select KJC.*
                From KJobCardChild KJC
				INNER JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPChildID = KJC.KPChildID
				WHERE KPC.KPMasterID = {id}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                KnittingPlanMaster data = await records.ReadFirstOrDefaultAsync<KnittingPlanMaster>();
                Guard.Against.NullObject(data);
                data.Childs = records.Read<KnittingPlanChild>().ToList();
                data.Yarns = records.Read<KnittingPlanYarn>().ToList();
                var yarnChilds = records.Read<KnittingPlanYarnChild>().ToList();
                var jobCards = records.Read<KJobCardMaster>().ToList();
                var kpGroups = records.Read<KnittingPlanGroup>().ToList();
                var jobCardChilds = records.Read<KJobCardChild>().ToList();
                if (kpGroups != null && kpGroups.Count() > 0) data.KPGroup = kpGroups.First();

                foreach (var child in data.Childs)
                {
                    child.KJobCardMasters = jobCards.FindAll(x => x.KPChildID == child.KPChildID);
                    child.KJobCardMasters.ForEach(x =>
                    {
                        x.Childs = jobCardChilds.Where(y => y.KJobCardMasterID == x.KJobCardMasterID).ToList();
                    });
                }
                data.Yarns.ForEach(y =>
                {
                    y.Childs = yarnChilds.Where(yc => yc.KPYarnID == y.KPYarnID).ToList();
                });
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
        public async Task<KnittingPlanGroup> GetDetailsAsync(string groupConceptNo, int planNo)
        {
            var query = $@"
                SELECT * 
                FROM {TableNames.Knitting_Plan_Group} 
                WHERE GroupID = {planNo};

                SELECT M.*, FC.Width, FC.Length
                FROM {TableNames.Knitting_Plan_Master} M
			    LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FC ON FC.ConceptID = M.ConceptID
                WHERE M.PlanNo = {planNo};

                SELECT C.*
                FROM {TableNames.Knitting_Plan_Child} C
                WHERE C.PlanNo = {planNo};

                SELECT Y.*
                FROM {TableNames.Knitting_Plan_Yarn} Y
                WHERE Y.GroupID = {planNo};

                SELECT YC.*
                FROM {TableNames.Knitting_Plan_Yarn_Child} YC
				INNER JOIN {TableNames.Knitting_Plan_Yarn} Y ON Y.KPYarnID = YC.KPYarnID
                WHERE Y.GroupID = {planNo};";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                KnittingPlanGroup data = await records.ReadFirstOrDefaultAsync<KnittingPlanGroup>();
                Guard.Against.NullObject(data);

                data.KnittingPlans = records.Read<KnittingPlanMaster>().ToList();
                var childs = records.Read<KnittingPlanChild>().ToList();
                var yarns = records.Read<KnittingPlanYarn>().ToList();
                var yarnChilds = records.Read<KnittingPlanYarnChild>().ToList();

                data.KnittingPlans.ForEach(kp =>
                {
                    kp.Childs = childs.Where(c => c.KPMasterID == kp.KPMasterID).ToList();
                    kp.Yarns = yarns.Where(c => c.KPMasterID == kp.KPMasterID).ToList();
                    kp.Yarns.ForEach(y =>
                    {
                        y.Childs = yarnChilds.Where(yc => yc.KPYarnID == y.KPYarnID).ToList();
                    });
                });

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
        public async Task<KnittingPlanChild> GetKnittingPlanDetailsAsync(int id)
        {
            var query = $@"
                Select * From {TableNames.Knitting_Plan_Child} Where KPChildID = {id}

                Select * From {TableNames.KNITTING_JOB_CARD_Master} Where KPChildID = {id}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);

                KnittingPlanChild data = await records.ReadFirstOrDefaultAsync<KnittingPlanChild>();
                Guard.Against.NullObject(data);

                data.KJobCardMasters = records.Read<KJobCardMaster>().ToList();
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
        public async Task<KnittingPlanGroup> GetKnittingPlanGroupAsync(int groupId)
        {
            var query = $@"
                --Master Data
                Select KPG.* 
                From {TableNames.Knitting_Plan_Group} KPG
                WHERE KPG.GroupID = {groupId};

                --Knitting Plan Master
                Select KPM.* 
                From {TableNames.Knitting_Plan_Master} KPM
                LEFT JOIN {TableNames.Knitting_Plan_Group} KPG ON KPG.GroupID = KPM.PlanNo
                WHERE KPG.GroupID = {groupId};

                --Knitting Plan Child
                Select KPC.* 
                From {TableNames.Knitting_Plan_Child} KPC
                LEFT JOIN {TableNames.Knitting_Plan_Group} KPG ON KPG.GroupID = KPC.PlanNo
                WHERE KPG.GroupID = {groupId}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);

                KnittingPlanGroup data = await records.ReadFirstOrDefaultAsync<KnittingPlanGroup>();
                Guard.Against.NullObject(data);

                data.KJobCardMasters = records.Read<KJobCardMaster>().ToList();
                data.KJobCardChilds = records.Read<KJobCardChild>().ToList();
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
        public async Task SaveGroupWiseAsync(List<KnittingPlanMaster> entities, KnittingProgramType type, int oldPlanQty = 0)
        {
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();
                
                await _connectionGmt.OpenAsync();
                transactionGmt = _connectionGmt.BeginTransaction();

                KnittingPlanGroup kPGroup = new KnittingPlanGroup();

                if (entities.Count() > 0 && entities.First().EntityState == EntityState.Added)
                {
                    var firstEntity = entities.First();
                    kPGroup = new KnittingPlanGroup();
                    kPGroup.GroupID = await _service.GetMaxIdAsync(TableNames.Knitting_Plan_Group, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                    kPGroup.IsSubContact = firstEntity.IsSubContact;
                    kPGroup.GroupConceptNo = firstEntity.GroupConceptNo;
                    kPGroup.AddedBy = firstEntity.AddedBy;
                    kPGroup.DateAdded = firstEntity.DateAdded;

                    int countChild = 0,
                        countYarns = 0,
                        countJobCards = 0;

                    entities.ForEach(x =>
                    {
                        countChild += x.Childs.Count();
                        countYarns += x.Yarns.Count();
                        x.Childs.ForEach(c =>
                        {
                            countJobCards += c.KJobCardMasters.Count();
                        });
                    });

                    var maxKPMasterId = await _service.GetMaxIdAsync(TableNames.Knitting_Plan_Master, entities.Count(), RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                    var maxChildId = await _service.GetMaxIdAsync(TableNames.Knitting_Plan_Child, countChild, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                    var maxYRYarnId = await _service.GetMaxIdAsync(TableNames.Knitting_Plan_Yarn, countYarns, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                    var maxJobCardId = await _service.GetMaxIdAsync(TableNames.KNITTING_JOB_CARD_Master, countJobCards, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

                    bool isSetGroupInfo = false;
                    foreach (KnittingPlanMaster entity in entities)
                    {
                        entity.KPMasterID = maxKPMasterId++;
                        entity.PlanNo = kPGroup.GroupID;

                        if (entity.Childs.Count() > 0 && !isSetGroupInfo)
                        {
                            var childItemF = entity.Childs.First();
                            var childItemL = entity.Childs.Last();

                            kPGroup.MachineDia = childItemF.MachineDia;
                            kPGroup.MachineGauge = childItemF.MachineGauge;
                            kPGroup.BrandID = childItemF.BrandID;
                            kPGroup.StartDate = childItemF.StartDate;
                            kPGroup.EndDate = childItemL.EndDate;
                            kPGroup.KnittingTypeID = childItemF.KnittingTypeID;

                            isSetGroupInfo = true;
                        }

                        foreach (var item in entity.Childs)
                        {
                            item.KPChildID = maxChildId++;
                            item.KPMasterID = entity.KPMasterID;
                            item.ActualStartDate = DateTime.Now;
                            item.ActualEndDate = DateTime.Now;
                            item.PlanNo = kPGroup.GroupID;

                            foreach (var jobCard in item.KJobCardMasters)
                            {
                                jobCard.GrayFabricOK = entity.GrayFabricOK;
                                jobCard.GrayGSM = entity.GrayGSM;
                                jobCard.GrayWidth = entity.GrayWidth;
                                jobCard.ProductionStatusId = entity.ProductionStatusId;
                                jobCard.NeedPreFinishingProcess = entity.NeedPreFinishingProcess;
                                jobCard.ColorWayDesignOk = entity.ColorWayDesignOk;
                                jobCard.ActualTotalNeedle = entity.ActualTotalNeedle;
                                jobCard.ActualTotalCourse = entity.ActualTotalCourse;
                                jobCard.ActualGreyHeight = entity.ActualGreyHeight;
                                jobCard.ActualGreyLength = entity.ActualGreyLength;
                                jobCard.ActualNeedle = entity.ActualNeedle;
                                jobCard.ActualCPI = entity.ActualCPI;
                                jobCard.AddedBy = entity.AddedBy;
                                jobCard.DateAdded = DateTime.Now;
                                jobCard.KJobCardMasterID = maxJobCardId++;
                                jobCard.KPChildID = item.KPChildID;
                                jobCard.GroupID = kPGroup.GroupID;
                                item.KPMasterID = entity.KPMasterID;
                                //jobCard.KJobCardNo = await _signatureRepository.GetMaxNoAsync(TableNames.KNITTING_JOB_CARD_No, jobCard.ContactID, RepeatAfterEnum.EveryDay, "0000");
                            }
                        }

                        foreach (var item in entity.Yarns)
                        {
                            item.KPYarnID = maxYRYarnId++;
                            item.KPMasterID = entity.KPMasterID;
                            item.GroupID = kPGroup.GroupID;
                        }
                    }
                }
                else if (entities.Count() > 0 && entities.First().EntityState == EntityState.Modified)
                {
                    kPGroup = new KnittingPlanGroup();
                    var obj = entities.First();
                    if (obj.Childs.Count() > 0)
                    {
                        var childItemF = obj.Childs.First();
                        var childItemL = obj.Childs.Last();

                        kPGroup.GroupID = obj.PlanNo;
                        kPGroup.MachineDia = childItemF.MachineDia;
                        kPGroup.MachineGauge = childItemF.MachineGauge;
                        kPGroup.BrandID = childItemF.BrandID;
                        kPGroup.StartDate = childItemF.StartDate;
                        kPGroup.EndDate = childItemL.EndDate;
                        kPGroup.KnittingTypeID = childItemF.KnittingTypeID;
                        kPGroup.GroupConceptNo = entities.First().GroupConceptNo;
                        kPGroup.EntityState = EntityState.Modified;

                        kPGroup.DateAdded = entities.First().DateAdded;
                    }
                    int countChild = 0,
                        countYarns = 0,
                        countJobCards = 0;

                    entities.ForEach(x =>
                    {
                        countChild += x.Childs.Where(c => c.EntityState == EntityState.Added).Count();
                        countYarns += x.Yarns.Where(c => c.EntityState == EntityState.Added).Count();
                        x.Childs.ForEach(c =>
                        {
                            countJobCards += c.KJobCardMasters.Where(j => j.EntityState == EntityState.Added).Count();
                        });
                    });

                    var maxKPMasterId = await _service.GetMaxIdAsync(TableNames.Knitting_Plan_Master, entities.Where(c => c.EntityState == EntityState.Added).Count(), RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                    var maxChildId = await _service.GetMaxIdAsync(TableNames.Knitting_Plan_Child, countChild, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                    var maxYRYarnId = await _service.GetMaxIdAsync(TableNames.Knitting_Plan_Yarn, countYarns, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                    var maxJobCardId = await _service.GetMaxIdAsync(TableNames.KNITTING_JOB_CARD_Master, countJobCards, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

                    foreach (KnittingPlanMaster entity in entities)
                    {
                        entity.PlanNo = kPGroup.GroupID;
                        if (type == KnittingProgramType.BDS) entity.Status = await GetKnittingPlanCompletionStatusForBdsAsync(entity.BookingNo);
                        if (type == KnittingProgramType.Bulk) entity.Status = await GetKnittingPlanCompletionStatusAsync(entity.YBookingNo);

                        foreach (var item in entity.Childs)
                        {
                            item.PlanNo = entity.PlanNo;

                            var query = "";
                            if (item.KPChildID != 0)
                                query = $@"Update {DbNames.EPYSLTEX}..{TableNames.KNITTING_JOB_CARD_Master} Set GrayFabricOK ='{entity.GrayFabricOK}', GrayGSM = '{entity.GrayGSM}'       
                                ,GrayWidth = {entity.GrayWidth}
                                ,ProductionStatusId = {entity.ProductionStatusId}
                                ,NeedPreFinishingProcess = '{entity.NeedPreFinishingProcess}'
                                ,ColorWayDesignOk = '{entity.ColorWayDesignOk}'
                                ,ActualTotalNeedle = {entity.ActualTotalNeedle}
                                ,ActualTotalCourse = {entity.ActualTotalCourse}
                                ,ActualGreyHeight = {entity.ActualGreyHeight}
                                ,ActualGreyLength = {entity.ActualGreyLength}
                                ,ActualNeedle = {entity.ActualNeedle}
                                ,ActualCPI = {entity.ActualCPI} Where KPChildID = {item.KPChildID}";
                            await _service1.GetFirstOrDefaultAsync(query, AppConstants.TEXTILE_CONNECTION);

                            if (item.EntityState == EntityState.Added)
                            {
                                item.KPChildID = maxChildId++;
                                item.KPMasterID = entity.KPMasterID;
                            }

                            foreach (var jobCard in item.KJobCardMasters.Where(x => x.EntityState == EntityState.Added))
                            {
                                jobCard.GrayFabricOK = entity.GrayFabricOK;
                                jobCard.GrayGSM = entity.GrayGSM;
                                jobCard.GrayWidth = entity.GrayWidth;
                                jobCard.ProductionStatusId = entity.ProductionStatusId;
                                jobCard.NeedPreFinishingProcess = entity.NeedPreFinishingProcess;
                                jobCard.ColorWayDesignOk = entity.ColorWayDesignOk;
                                jobCard.ActualTotalNeedle = entity.ActualTotalNeedle;
                                jobCard.ActualTotalCourse = entity.ActualTotalCourse;
                                jobCard.ActualGreyHeight = entity.ActualGreyHeight;
                                jobCard.ActualGreyLength = entity.ActualGreyLength;
                                jobCard.ActualNeedle = entity.ActualNeedle;
                                jobCard.ActualCPI = entity.ActualCPI;
                                jobCard.KJobCardMasterID = maxJobCardId++;
                                jobCard.KPChildID = item.KPChildID;
                                jobCard.GroupID = kPGroup.GroupID;
                                //jobCard.KJobCardNo = await _signatureRepository.GetMaxNoAsync(TableNames.KNITTING_JOB_CARD_No, jobCard.ContactID, RepeatAfterEnum.EveryDay, "0000");
                            }
                        }

                        foreach (var item in entity.Yarns)
                        {
                            if (item.EntityState == EntityState.Added)
                            {
                                item.KPYarnID = maxYRYarnId++;
                                item.KPMasterID = entity.KPMasterID;
                                item.GroupID = kPGroup.GroupID;
                            }
                        }
                    }
                }

                List<KnittingPlanChild> childs = new List<KnittingPlanChild>();
                List<KnittingPlanYarn> yarns = new List<KnittingPlanYarn>();
                entities.ForEach(x =>
                {
                    childs.AddRange(x.Childs);
                    yarns.AddRange(x.Yarns);
                });

                //var yarnsCheck = yarns.Where(x => x.FCMRChildID == 0).ToList();
                //if (yarnsCheck.Count() > 0)
                //{
                //    throw new Exception("FCMRChild missing => SaveGroupWiseAsync => KnittingProgramService");
                //}

                await _service.SaveSingleAsync(kPGroup, transaction);
                await _service.SaveAsync(entities, transaction);
                await _service.SaveAsync(childs, transaction);
                await _service.SaveAsync(yarns, transaction);

                //List<KJobCardMaster> jobCards = new List<KJobCardMaster>();
                //entity.Childs.ForEach(x => jobCards.AddRange(x.KJobCardMasters));
                //await _service.SaveAsync(jobCards, transaction);

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
        /*
        private string CheckValidations(List<KnittingPlanGroup> kpgs,
            List<KnittingPlanMaster> kpms,
            List<KnittingPlanChild> kpcs,
            List<KnittingPlanYarn> kpys,
            string methodName,
            string serviceName)
        {
            string methodService = $@"=> missing => {methodName} => {serviceName}";
            int i = 0;
            for (i = 0; i < kpgs.Count(); i++)
            {
                var kpg = kpgs[i];
                if (kpg.GroupConceptNo.IsNullOrEmpty())
                {
                    return $@"GroupConceptNo {methodService}";
                }
                else if (kpg.BrandID == 0)
                {
                    return $@"Brand {methodService}";
                }
                else if (kpg.BuyerID == 0)
                {
                    return $@"BuyerID {methodService}";
                }
                else if (kpg.BuyerTeamID == 0)
                {
                    return $@"BuyerTeamID {methodService}";
                }
            }
            return "";
        }
        */
        public async Task SaveAsync(KnittingPlanMaster entity, KnittingProgramType type, decimal oldPlanQty = 0)
        {
            try
            {
                await _connection.OpenAsync();
                entity.PlanQty = await GetAndUpdatePlanQty(_connection, entity, oldPlanQty);
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

                if (type == KnittingProgramType.BDS) entity.Status = await GetKnittingPlanCompletionStatusForBdsAsync(entity.BookingNo);
                if (type == KnittingProgramType.Bulk) entity.Status = await GetKnittingPlanCompletionStatusAsync(entity.YBookingNo);

                //var yarnsCheck = entity.Yarns.Where(x => x.FCMRChildID == 0).ToList();
                //if (yarnsCheck.Count() > 0)
                //{
                //    throw new Exception("FCMRChild missing => SaveAsync => KnittingProgramService");
                //}

                List<KnittingPlanYarnChild> yarnChilds = new List<KnittingPlanYarnChild>();
                entity.Yarns.ForEach(y =>
                {
                    yarnChilds.AddRange(y.Childs);
                });

                await _service.SaveSingleAsync(entity.KPGroup, transaction);
                await _service.SaveSingleAsync(entity, transaction);
                await _service.SaveAsync(entity.Childs, transaction);
                await _service.SaveAsync(entity.Yarns, transaction);
                foreach (KnittingPlanYarn item in entity.Yarns.Where(x => x.EntityState == EntityState.Added || x.EntityState == EntityState.Modified))
                {
                    if (entity.UpdatedBy.IsNull()) entity.UpdatedBy = 0;
                    int userId = entity.EntityState == EntityState.Added ? entity.AddedBy : (int)entity.UpdatedBy;
                    await _service.ValidationSingleAsync(item, transaction, "sp_Validation_KnittingPlanYarn", item.EntityState, userId, item.KPYarnID);
                }
                await _service.SaveAsync(yarnChilds, transaction);

                List<KJobCardMaster> jobCards = new List<KJobCardMaster>();
                entity.Childs.ForEach(x => jobCards.AddRange(x.KJobCardMasters));

                List<KJobCardChild> jobCardChilds = new List<KJobCardChild>();
                jobCards.ForEach(jc =>
                {
                    jc.GroupID = entity.KPGroup.GroupID;
                    jobCardChilds.AddRange(jc.Childs);
                });

                await _service.SaveAsync(jobCards, transaction);
                await _service.SaveAsync(jobCardChilds, transaction);
                //await _service.ExecuteAsync("spUpdateFreeConceptStatus", new { InterfaceFrom = interfaceFrom, ConceptID = conceptID, GroupConceptNo = groupConceptNo, BookingID = bookingID, IsBDS = isBDS, CCColorID = ccColorID, ColorID = colorID, ItemMasterID = itemMasterID, ConceptIDs = conceptIDs }, 30, CommandType.StoredProcedure);
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
        public async Task UpdateFreeConceptMasterAsync(int ConceptID, int MCSubClassID)
        {
            await _service.ExecuteAsync("spUpdateFreeConceptMaster", new { ConceptID = ConceptID, MCSubClassID = MCSubClassID }, 30, CommandType.StoredProcedure);
        }

        //public async Task UpdateBDSTNA_BatchPreparationPlanAsync(int BatchID)
        //{
        //    await _service.ExecuteAsync("spUpdateBDSTNA_BatchPreparationPlan", new { BatchID = BatchID }, 30, CommandType.StoredProcedure);
        //}

        public async Task SaveRevisionAsync(KnittingPlanMaster revisionEntity, KnittingPlanMaster entity, int userId)
        {
            try
            {
                await _connection.OpenAsync();

                entity.PlanQty = await GetAndUpdatePlanQty(_connection, entity, 0);

                transaction = _connection.BeginTransaction();

                await _connectionGmt.OpenAsync();
                transactionGmt = _connectionGmt.BeginTransaction();

                var updateEntity = await UpdateRevisionAsync(revisionEntity, userId);
                await _service.SaveSingleAsync(updateEntity, transaction);

                entity = await AddAsync(entity);
                entity.Status = await GetKnittingPlanCompletionStatusAsync(entity.YBookingNo);

                //var yarnsCheck = entity.Yarns.Where(x => x.FCMRChildID == 0).ToList();
                //if (yarnsCheck.Count() > 0)
                //{
                //    throw new Exception("FCMRChild missing => SaveRevisionAsync => KnittingProgramService");
                //}

                //await _service.SaveSingleAsync(entity.KPGroup, transaction);
                await _service.SaveSingleAsync(entity, transaction);
                await _service.SaveAsync(entity.Childs, transaction);
                await _service.SaveAsync(entity.Yarns, transaction);
                foreach (KnittingPlanYarn item in entity.Yarns.Where(x => x.EntityState == EntityState.Added || x.EntityState == EntityState.Modified))
                {
                    if (entity.UpdatedBy.IsNull()) entity.UpdatedBy = 0;
                    int userId1 = entity.EntityState == EntityState.Added ? entity.AddedBy : (int)entity.UpdatedBy;
                    await _service.ValidationSingleAsync(item, transaction, "sp_Validation_KnittingPlanYarn", item.EntityState, userId1, item.KPYarnID);
                }

                List<KJobCardMaster> jobCards = new List<KJobCardMaster>();
                entity.Childs.ForEach(x => jobCards.AddRange(x.KJobCardMasters));
                await _service.SaveAsync(jobCards, transaction);

                transaction.Commit();
                transactionGmt.Commit();
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
        public async Task SaveGroupAsync(KnittingPlanGroup entity, bool isRevision)
        {
            try
            {
                if (isRevision)
                {
                    await _service.ExecuteAsync("spBackupKnittingProgramGroup_Full", new { GroupID = entity.GroupID, UserID = entity.UserId }, 30, CommandType.StoredProcedure);
                }
                await _connection.OpenAsync();

                transaction = _connection.BeginTransaction();

                await _connectionGmt.OpenAsync();
                transactionGmt = _connectionGmt.BeginTransaction();

                switch (entity.EntityState)
                {
                    case EntityState.Added:
                        entity = await AddGroupAsync(entity);
                        break;

                    case EntityState.Modified:
                        entity = await UpdateGroupAsync(entity);
                        break;

                    default:
                        break;
                }

                List<KnittingPlanMaster> knittingPlans = new List<KnittingPlanMaster>();
                List<KnittingPlanChild> childs = new List<KnittingPlanChild>();
                List<KnittingPlanYarn> yarns = new List<KnittingPlanYarn>();
                List<KnittingPlanYarnChild> yarnChilds = new List<KnittingPlanYarnChild>();

                entity.KnittingPlans.ForEach(kp =>
                {
                    knittingPlans.Add(kp);
                    childs.AddRange(kp.Childs);
                    yarns.AddRange(kp.Yarns);
                    kp.Yarns.ForEach(y =>
                    {
                        yarnChilds.AddRange(y.Childs);
                    });
                });

                //var yarnsCheck = yarns.Where(x => x.FCMRChildID == 0).ToList();
                //if (yarnsCheck.Count() > 0)
                //{
                //    throw new Exception("FCMRChild missing => SaveRevisionAsync => KnittingProgramService");
                //}
                if (entity.SubGroupID == 0 && childs.Count() > 0)
                {
                    entity.SubGroupID = childs.First().SubGroupID;
                }
                await _service.SaveSingleAsync(entity, transaction);
                await _service.SaveAsync(knittingPlans, transaction);
                await _service.SaveAsync(childs, transaction);
                await _service.SaveAsync(yarns, transaction);
                await _service.SaveAsync(yarnChilds, transaction);

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

        #region Helpers
        private async Task<KnittingPlanMaster> AddAsync(KnittingPlanMaster entity)
        {
            KnittingPlanGroup kPGroup = new KnittingPlanGroup();
            kPGroup.GroupID = await _service.GetMaxIdAsync(TableNames.Knitting_Plan_Group, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
            kPGroup.IsSubContact = entity.IsSubContact;
            kPGroup.GroupConceptNo = entity.GroupConceptNo;
            kPGroup.ColorName = string.Join(",", entity.Childs.Where(x => x.ColorName.IsNotNullOrEmpty()).Select(x => x.ColorName).Distinct());
            kPGroup.PlanQty = entity.Childs.Sum(x => x.BookingQty);
            kPGroup.TotalQty = entity.TotalQty;
            kPGroup.BuyerID = entity.BuyerID;
            kPGroup.BuyerTeamID = entity.BuyerTeamID;
            kPGroup.SubGroupID = entity.SubGroupID;
            kPGroup.AddedBy = entity.AddedBy;
            kPGroup.DateAdded = entity.DateAdded;
            if (entity.Childs.Count() > 0)
            {
                var child = entity.Childs.First();

                entity.KPGroup.KnittingTypeID = child.KnittingTypeID;
                entity.KPGroup.MachineDia = child.MachineDia;
                entity.KPGroup.MachineGauge = child.MachineGauge;
                entity.KPGroup.BrandID = child.BrandID;
                entity.KPGroup.StartDate = child.StartDate;
                entity.KPGroup.EndDate = child.EndDate;
                entity.KPGroup.Needle = child.Needle;
                entity.KPGroup.CPI = child.CPI;
            }

            entity.KPMasterID = await _service.GetMaxIdAsync(TableNames.Knitting_Plan_Master, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
            entity.PlanNo = kPGroup.GroupID; //await GetPlanNoAsync(entity.ConceptID);

            var maxYRChildId = await _service.GetMaxIdAsync(TableNames.Knitting_Plan_Child, entity.Childs.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
            var maxYRYarnId = await _service.GetMaxIdAsync(TableNames.Knitting_Plan_Yarn, entity.Yarns.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

            int yarnChildCount = 0;
            entity.Yarns.ForEach(y =>
            {
                yarnChildCount += y.Childs.Count();
            });
            var maxYRYarnChildId = await _service.GetMaxIdAsync(TableNames.Knitting_Plan_Yarn_Child, yarnChildCount, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

            var maxJobCardId = await _service.GetMaxIdAsync(TableNames.KNITTING_JOB_CARD_Master, entity.Childs.Sum(x => x.KJobCardMasters.Count()), RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
            var countKJCChilds = 0;
            entity.Childs.ForEach(c =>
            {
                c.KJobCardMasters.ForEach(kj =>
                {
                    countKJCChilds += kj.Childs.Count(); ;
                });
            });
            var maxJobCardChildId = await _service.GetMaxIdAsync(TableNames.KNITTING_JOB_CARD_Child, countKJCChilds, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

            if (entity.Childs.Count() > 0)
            {
                var childItemF = entity.Childs.First();
                var childItemL = entity.Childs.Last();

                kPGroup.MachineDia = childItemF.MachineDia;
                kPGroup.MachineGauge = childItemF.MachineGauge;
                kPGroup.BrandID = childItemF.BrandID;
                kPGroup.StartDate = childItemF.StartDate;
                kPGroup.EndDate = childItemL.EndDate;
                kPGroup.KnittingTypeID = childItemF.KnittingTypeID;
            }
            entity.KPGroup = CommonFunction.DeepClone(kPGroup);

            foreach (var item in entity.Childs)
            {
                item.KPChildID = maxYRChildId++;
                item.KPMasterID = entity.KPMasterID;
                item.ActualStartDate = DateTime.Now;
                item.ActualEndDate = DateTime.Now;
                item.PlanNo = kPGroup.GroupID;

                foreach (var jobCard in item.KJobCardMasters)
                {
                    jobCard.GrayFabricOK = entity.GrayFabricOK;
                    jobCard.GrayGSM = entity.GrayGSM;
                    jobCard.GrayWidth = entity.GrayWidth;
                    jobCard.ProductionStatusId = entity.ProductionStatusId;
                    jobCard.NeedPreFinishingProcess = entity.NeedPreFinishingProcess;
                    jobCard.ColorWayDesignOk = entity.ColorWayDesignOk;
                    jobCard.ActualTotalNeedle = entity.ActualTotalNeedle;
                    jobCard.ActualTotalCourse = entity.ActualTotalCourse;
                    jobCard.ActualGreyHeight = entity.ActualGreyHeight;
                    jobCard.ActualGreyLength = entity.ActualGreyLength;
                    jobCard.ActualNeedle = entity.ActualNeedle;
                    jobCard.ActualCPI = entity.ActualCPI;
                    jobCard.AddedBy = entity.AddedBy;
                    jobCard.DateAdded = DateTime.Now;
                    jobCard.KJobCardMasterID = maxJobCardId++;
                    jobCard.KPChildID = item.KPChildID;
                    jobCard.GroupID = kPGroup.GroupID;
                    item.KPMasterID = entity.KPMasterID;
                    jobCard.KJobCardNo = await _service.GetMaxNoAsync(TableNames.KNITTING_JOB_CARD_No, jobCard.ContactID, RepeatAfterEnum.EveryDay, "0000", transactionGmt, _connectionGmt);

                    jobCard.Childs.ForEach(c =>
                    {
                        c.KJobCardMasterID = jobCard.KJobCardMasterID;
                        c.KJobCardChildID = maxJobCardChildId++;
                        c.KPChildID = item.KPChildID;
                    });
                }
            }

            foreach (var item in entity.Yarns)
            {
                item.KPYarnID = maxYRYarnId++;
                item.KPMasterID = entity.KPMasterID;
                item.GroupID = kPGroup.GroupID;

                item.Childs.ForEach(yc =>
                {
                    yc.KPYarnChildID = maxYRYarnChildId++;
                    yc.KPYarnID = item.KPYarnID;
                });
            }

            return entity;
        }

        private async Task<KnittingPlanMaster> UpdateAsync(KnittingPlanMaster entity)
        {
            entity.KPGroup.EntityState = EntityState.Modified;
            entity.KPGroup.ColorName = string.Join(",", entity.Childs.Where(x => x.ColorName.IsNotNullOrEmpty()).Select(x => x.ColorName).Distinct());
            entity.KPGroup.PlanQty = entity.Childs.Sum(x => x.BookingQty);
            entity.KPGroup.TotalQty = entity.TotalQty;
            entity.KPGroup.GroupConceptNo = entity.GroupConceptNo;
            entity.KPGroup.BuyerID = entity.BuyerID;
            entity.KPGroup.BuyerTeamID = entity.BuyerTeamID;
            entity.KPGroup.SubGroupID = entity.SubGroupID;

            if (entity.Childs.Count() > 0)
            {
                var child = entity.Childs.First();
                entity.KPGroup.KnittingTypeID = child.KnittingTypeID;
                entity.KPGroup.MachineDia = child.MachineDia;
                entity.KPGroup.MachineGauge = child.MachineGauge;
                entity.KPGroup.IsSubContact = child.IsSubContact;
                entity.KPGroup.BrandID = child.KJobCardMasters.Count() > 0 ? child.KJobCardMasters.First().BrandID : 0;
                entity.KPGroup.StartDate = child.StartDate;
                entity.KPGroup.EndDate = child.EndDate;
                entity.KPGroup.Needle = child.Needle;
                entity.KPGroup.CPI = child.CPI;
            }

            var maxChildId = await _service.GetMaxIdAsync(TableNames.Knitting_Plan_Child, entity.Childs.Where(x => x.EntityState == EntityState.Added).Count(), RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
            var maxYRYarnId = await _service.GetMaxIdAsync(TableNames.Knitting_Plan_Yarn, entity.Yarns.Where(x => x.EntityState == EntityState.Added).Count(), RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

            int yarnChildCount = 0;
            entity.Yarns.ForEach(y =>
            {
                yarnChildCount += y.Childs.Where(x => x.EntityState == EntityState.Added).Count();
            });
            var maxYRYarnChildId = await _service.GetMaxIdAsync(TableNames.Knitting_Plan_Yarn_Child, yarnChildCount, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

            var maxJobCardId = await _service.GetMaxIdAsync(TableNames.KNITTING_JOB_CARD_Master, entity.Childs.Sum(x => x.KJobCardMasters.Where(y => y.EntityState == EntityState.Added).Count()), RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
            var countKJCChilds = 0;
            entity.Childs.ForEach(c =>
            {
                c.KJobCardMasters.ForEach(kj =>
                {
                    countKJCChilds += kj.Childs.Where(x => x.EntityState == EntityState.Added).Count(); ;
                });
            });
            var maxJobCardChildId = await _service.GetMaxIdAsync(TableNames.KNITTING_JOB_CARD_Child, countKJCChilds, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

            foreach (var item in entity.Childs)
            {
                item.PlanNo = entity.PlanNo;

                var query = "";
                if (item.KPChildID != 0)
                    query = $@"Update {DbNames.EPYSLTEX}..KJobCardMaster Set GrayFabricOK ='{entity.GrayFabricOK}', GrayGSM = '{entity.GrayGSM}'       
                    ,GrayWidth = {entity.GrayWidth}
                    ,ProductionStatusId = {entity.ProductionStatusId}
                    ,NeedPreFinishingProcess = '{entity.NeedPreFinishingProcess}'
                    ,ColorWayDesignOk = '{entity.ColorWayDesignOk}'
                    ,ActualTotalNeedle = {entity.ActualTotalNeedle}
                    ,ActualTotalCourse = {entity.ActualTotalCourse}
                    ,ActualGreyHeight = {entity.ActualGreyHeight}
                    ,ActualGreyLength = {entity.ActualGreyLength}
                    ,ActualNeedle = {entity.ActualNeedle}
                    ,ActualCPI = {entity.ActualCPI} Where KPChildID = {item.KPChildID}";
                await _service1.GetFirstOrDefaultAsync(query, AppConstants.TEXTILE_CONNECTION);

                if (item.EntityState == EntityState.Added)
                {
                    item.KPChildID = maxChildId++;
                    item.KPMasterID = entity.KPMasterID;
                }

                foreach (var jobCard in item.KJobCardMasters.Where(x => x.EntityState == EntityState.Added))
                {
                    jobCard.GrayFabricOK = entity.GrayFabricOK;
                    jobCard.GrayGSM = entity.GrayGSM;
                    jobCard.GrayWidth = entity.GrayWidth;
                    jobCard.ProductionStatusId = entity.ProductionStatusId;
                    jobCard.NeedPreFinishingProcess = entity.NeedPreFinishingProcess;
                    jobCard.ColorWayDesignOk = entity.ColorWayDesignOk;
                    jobCard.ActualTotalNeedle = entity.ActualTotalNeedle;
                    jobCard.ActualTotalCourse = entity.ActualTotalCourse;
                    jobCard.ActualGreyHeight = entity.ActualGreyHeight;
                    jobCard.ActualGreyLength = entity.ActualGreyLength;
                    jobCard.ActualNeedle = entity.ActualNeedle;
                    jobCard.ActualCPI = entity.ActualCPI;
                    jobCard.KJobCardMasterID = maxJobCardId++;
                    jobCard.KPChildID = item.KPChildID;
                    jobCard.GroupID = entity.KPGroup.GroupID;
                    jobCard.KJobCardNo = await _service.GetMaxNoAsync(TableNames.KNITTING_JOB_CARD_No, jobCard.ContactID, RepeatAfterEnum.EveryDay, "0000", transactionGmt, _connectionGmt);

                    jobCard.Childs.Where(x => x.EntityState == EntityState.Added).ToList().ForEach(c =>
                    {
                        c.KJobCardMasterID = jobCard.KJobCardMasterID;
                        c.KJobCardChildID = maxJobCardChildId++;
                        c.KPChildID = item.KPChildID;
                    });
                }
            }

            foreach (var item in entity.Yarns)
            {
                if (item.EntityState == EntityState.Added)
                {
                    item.KPYarnID = maxYRYarnId++;
                    item.KPMasterID = entity.KPMasterID;
                    item.GroupID = entity.KPGroup.GroupID;
                }
                item.Childs.Where(x => x.EntityState == EntityState.Added).ToList().ForEach(yc =>
                {
                    yc.KPYarnChildID = maxYRYarnChildId++;
                    yc.KPYarnID = item.KPYarnID;
                });
            }
            return entity;
        }

        private async Task<KnittingPlanMaster> UpdateRevisionAsync(KnittingPlanMaster entity, int userId)
        {
            var revisionData = await GetAllRevisionDataAsync(entity.KPMasterID);

            foreach (var jobCard in revisionData.Item1)
            {
                jobCard.Active = false;
                jobCard.DateUpdated = DateTime.Now;
                jobCard.UpdatedBy = userId;
                jobCard.EntityState = EntityState.Modified;
            }

            foreach (var production in revisionData.Item2)
            {
                production.Active = false;
                production.DateUpdated = DateTime.Now;
                production.UpdatedBy = userId;
                production.EntityState = EntityState.Modified;
            }

            return entity;
        }

        private async Task<int> GetPlanNoAsync(int conceptId)
        {
            var query = $@"Select Count(*) From KnittingPlanMaster Where ConceptID = {conceptId}";
            int planNo = await _connection.QueryFirstOrDefaultAsync<int>(query, transaction: transaction);
            return ++planNo;
        }

        private async Task<decimal> GetAndUpdatePlanQty(SqlConnection connection, KnittingPlanMaster entity, decimal oldPlanQty)
        {
            var totalPlanQty = entity.PlanQty - oldPlanQty;

            string query;
            if (entity.ConceptID > 0)
            {
                query = $@"Select * From {TableNames.Knitting_Plan_Master} Where KPMasterID != {entity.KPMasterID} And Active = 1 And ConceptID = {entity.ConceptID}";
            }
            else
            {
                query = $@"Select * From {TableNames.Knitting_Plan_Master} Where KPMasterID != {entity.KPMasterID} And Active = 1 And YBookingNo = ISNULL('{entity.YBookingNo}',0)";
            }

            IEnumerable<KnittingPlanMaster> planList = await connection.QueryAsync<KnittingPlanMaster>(query);

            if (planList.Any())
            {
                totalPlanQty += planList.First().PlanQty;
                foreach (var plan in planList)
                {
                    plan.PlanQty = totalPlanQty;
                    plan.EntityState = EntityState.Modified;
                }
            }
            else
            {
                totalPlanQty = entity.PlanQty;
            }

            return totalPlanQty;
        }

        private async Task<Tuple<List<KJobCardMaster>, List<KnittingProduction>>> GetAllRevisionDataAsync(int kpMasterID)
        {
            var sql = $@"
            -- Job Cards
            With KPC As (
	            Select * From {TableNames.Knitting_Plan_Child} Where KPMasterID = {kpMasterID}
            )

            Select KJM.*
            From {TableNames.KNITTING_JOB_CARD_Master} KJM
            Inner Join KPC On KJM.KPChildID = KPC.KPChildID

            -- Knitting Productions
            ;With KPC As (
	            Select * From {TableNames.Knitting_Plan_Child} Where KPMasterID = {kpMasterID}
            )

            Select KPD.*
            From {TableNames.KNITTING_JOB_CARD_Master} KJM
            Inner Join KPC On KJM.KPChildID = KPC.KPChildID
            Inner Join KnittingProduction KPD On KJM.KJobCardMasterID = KPD.KJobCardMasterID";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);

                List<KnittingPlanMaster> knittingPlans = records.Read<KnittingPlanMaster>().ToList();
                List<KJobCardMaster> jobCardMasters = records.Read<KJobCardMaster>().ToList();
                List<KnittingProduction> knittingProductions = records.Read<KnittingProduction>().ToList();

                return Tuple.Create(jobCardMasters, knittingProductions);
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
        private async Task<int> GetMaxAdditionNo(int parentGroupID)
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["TexConnection"].ConnectionString;
            var queryString = $"SELECT MaxValue=COUNT(*) FROM {TableNames.Knitting_Plan_Group} WHERE ParentGroupID = {parentGroupID}";

            int maxNo = 0;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(queryString, connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                try
                {
                    while (reader.Read())
                    {
                        maxNo = Convert.ToInt32(reader["MaxValue"]);
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            return maxNo;
        }
        private async Task<KnittingPlanGroup> AddGroupAsync(KnittingPlanGroup entity)
        {
            entity.GroupID = await _service.GetMaxIdAsync(TableNames.Knitting_Plan_Group, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
            if (entity.IsAdditional)
            {
                int maxCount = await this.GetMaxAdditionNo(entity.ParentGroupID);
                entity.AdditionNo = maxCount + 1;
            }


            //entity.PlanNo = await GetPlanNoAsync(entity.ConceptID);

            var maxKPId = await _service.GetMaxIdAsync(TableNames.Knitting_Plan_Master, entity.KnittingPlans.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
            int totalChilds = 0,
                totalYarns = 0,
                totalYarnChilds = 0;

            entity.KnittingPlans.ForEach(x =>
            {
                x.IsSubContact = entity.IsSubContact;

                x.Childs.ForEach(c =>
                {
                    c.KPMasterID = x.KPMasterID;
                    c.IsSubContact = entity.IsSubContact;
                    c.MachineGauge = entity.MachineGauge;
                    c.MachineDia = entity.MachineDia;
                    c.StartDate = entity.StartDate;
                    c.EndDate = entity.EndDate;
                    c.SubGroupID = x.SubGroupID;
                    c.ItemMasterID = x.ItemMasterID;
                    c.MCSubClassID = x.MCSubClassID;
                    c.PlanNo = entity.GroupID;
                });

                totalChilds += x.Childs.Count();
                totalYarns += x.Yarns.Count();

                x.Yarns.ForEach(y => totalYarnChilds += y.Childs.Count());
            });
            var maxYRChildId = await _service.GetMaxIdAsync(TableNames.Knitting_Plan_Child, totalChilds, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
            var maxYRYarnId = await _service.GetMaxIdAsync(TableNames.Knitting_Plan_Yarn, totalYarns, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
            var maxYRYarnChildId = await _service.GetMaxIdAsync(TableNames.Knitting_Plan_Yarn_Child, totalYarnChilds, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

            entity.KnittingPlans.ForEach(kp =>
            {
                kp.PlanNo = entity.GroupID;
                kp.KPMasterID = maxKPId++;
                kp.AddedBy = entity.AddedBy;
                kp.DateAdded = DateTime.Now;
                entity.IsSubContact = kp.IsSubContact;

                List<KnittingPlanChild> childs = new List<KnittingPlanChild>();
                kp.Childs.ForEach(kpc =>
                {
                    KnittingPlanChild newKPC = new KnittingPlanChild();
                    newKPC = CommonFunction.DeepClone(kpc);
                    newKPC.KPChildID = maxYRChildId++;
                    newKPC.KPMasterID = kp.KPMasterID;
                    newKPC.PlanNo = entity.GroupID;
                    entity.MachineDia = kpc.MachineDia;
                    entity.MachineGauge = kpc.MachineGauge;
                    childs.Add(newKPC);
                });
                kp.Childs = childs;

                List<KnittingPlanYarn> knittingPlanYarns = new List<KnittingPlanYarn>();

                kp.Yarns.ForEach(kpy =>
                {
                    KnittingPlanYarn newKPY = new KnittingPlanYarn();
                    newKPY = CommonFunction.DeepClone(kpy);
                    newKPY.KPYarnID = maxYRYarnId++;
                    newKPY.KPMasterID = kp.KPMasterID;
                    newKPY.GroupID = entity.GroupID;

                    List<KnittingPlanYarnChild> knittingPlanYarnChilds = new List<KnittingPlanYarnChild>();
                    kpy.Childs.ForEach(yc =>
                    {
                        KnittingPlanYarnChild newYC = new KnittingPlanYarnChild();
                        newYC = CommonFunction.DeepClone(yc);
                        newYC.KPYarnChildID = maxYRYarnChildId++;
                        newYC.KPYarnID = newKPY.KPYarnID;
                        //newYC.FCMRChildID = newKPY.FCMRChildID;
                        knittingPlanYarnChilds.Add(newYC);
                    });
                    newKPY.Childs = knittingPlanYarnChilds;
                    knittingPlanYarns.Add(newKPY);
                });
                kp.Yarns = knittingPlanYarns;
            });
            return entity;
        }

        private async Task<KnittingPlanGroup> UpdateGroupAsync(KnittingPlanGroup entity)
        {
            int totalKP = 0,
                totalChilds = 0,
                totalYarns = 0,
                totalYarnChilds = 0;

            entity.KnittingPlans.ForEach(x =>
            {
                if (x.EntityState == EntityState.Added) totalKP++;
                x.Childs.Where(c => c.EntityState == EntityState.Added).ToList().ForEach(c =>
                {
                    totalChilds++;
                });
                x.Yarns.Where(c => c.EntityState == EntityState.Added).ToList().ForEach(c =>
                {
                    totalYarns++;
                    c.Childs.Where(yc => yc.EntityState == EntityState.Added).ToList().ForEach(yc =>
                    {
                        totalYarnChilds++;
                    });
                });
            });
            var maxKPId = await _service.GetMaxIdAsync(TableNames.Knitting_Plan_Master, totalKP, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
            var maxChildId = await _service.GetMaxIdAsync(TableNames.Knitting_Plan_Child, totalChilds, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
            var maxYRYarnId = await _service.GetMaxIdAsync(TableNames.Knitting_Plan_Yarn, totalYarns, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
            var maxYRYarnChildId = await _service.GetMaxIdAsync(TableNames.Knitting_Plan_Yarn_Child, totalYarnChilds, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

            entity.KnittingPlans.ForEach(kp =>
            {
                if (kp.EntityState == EntityState.Added)
                {
                    kp.PlanNo = entity.GroupID;
                    kp.KPMasterID = maxKPId++;
                    kp.AddedBy = entity.AddedBy;
                    kp.DateAdded = DateTime.Now;
                }
                else if (kp.EntityState == EntityState.Modified)
                {
                    kp.UpdatedBy = entity.AddedBy;
                    kp.DateUpdated = DateTime.Now;
                    entity.IsSubContact = kp.IsSubContact;
                }

                kp.Childs.Where(c => c.EntityState == EntityState.Added).ToList().ForEach(kpc =>
                {
                    KnittingPlanChild newKPC = new KnittingPlanChild();
                    newKPC = CommonFunction.DeepClone(kpc);
                    newKPC.KPChildID = maxChildId++;
                    newKPC.KPMasterID = kp.KPMasterID;
                    newKPC.PlanNo = entity.GroupID;
                    entity.MachineDia = kpc.MachineDia;
                    entity.MachineGauge = kpc.MachineGauge;
                    kpc = newKPC;
                });

                List<KnittingPlanYarn> knittingPlanYarns = new List<KnittingPlanYarn>();

                foreach (KnittingPlanYarn kpc in kp.Yarns)
                {
                    if (kpc.EntityState == EntityState.Added)
                    {
                        kpc.KPYarnID = maxYRYarnId++;
                        kpc.KPMasterID = kp.KPMasterID;
                        kpc.GroupID = entity.GroupID;
                    }
                    foreach (KnittingPlanYarnChild yc in kpc.Childs)
                    {
                        if (yc.EntityState == EntityState.Added)
                        {
                            yc.KPYarnChildID = maxYRYarnChildId++;
                        }
                        yc.KPYarnID = kpc.KPYarnID;
                    }
                }
            });
            return entity;
        }

        #endregion Helpers

        public async Task ReviseAsync(KnittingPlanMaster entity, KnittingProgramType type, decimal oldPlanQty = 0)
        {
            try
            {
                await _service.ExecuteAsync("spBackupKnittingProgram_Full", new { ConceptNo = entity.ConceptNo }, 30, CommandType.StoredProcedure);

                await _connection.OpenAsync();

                entity.PlanQty = await GetAndUpdatePlanQty(_connection, entity, oldPlanQty);

                transaction = _connection.BeginTransaction();

                await _connectionGmt.OpenAsync();
                transactionGmt = _connectionGmt.BeginTransaction();

                entity = await UpdateAsync(entity);

                if (type == KnittingProgramType.BDS) entity.Status = await GetKnittingPlanCompletionStatusForBdsAsync(entity.BookingNo);
                if (type == KnittingProgramType.Bulk) entity.Status = await GetKnittingPlanCompletionStatusAsync(entity.YBookingNo);

                //var yarnsCheck = entity.Yarns.Where(x => x.FCMRChildID == 0).ToList();
                //if (yarnsCheck.Count() > 0)
                //{
                //    throw new Exception("FCMRChild missing => ReviseAsync => KnittingProgramService");
                //}

                List<KnittingPlanYarnChild> yarnChilds = new List<KnittingPlanYarnChild>();
                entity.Yarns.ForEach(y =>
                {
                    yarnChilds.AddRange(y.Childs);
                });
                int userId = entity.EntityState == EntityState.Added ? entity.AddedBy : (int)entity.UpdatedBy;
                await _service.SaveSingleAsync(entity.KPGroup, transaction);
                await _service.SaveSingleAsync(entity, transaction);
                await _service.ValidationSingleAsync(entity, transaction, "sp_UpdateFreeConceptMRMaster_IsNeedRevision", entity.EntityState, userId, entity.ConceptID);
                await _service.SaveAsync(entity.Childs, transaction);
                await _service.SaveAsync(entity.Yarns, transaction);
                foreach (KnittingPlanYarn item in entity.Yarns.Where(x => x.EntityState == EntityState.Added || x.EntityState == EntityState.Modified))
                {
                    if (entity.UpdatedBy.IsNull()) entity.UpdatedBy = 0;
                    //int userId = entity.EntityState == EntityState.Added ? entity.AddedBy : (int)entity.UpdatedBy;
                    await _service.ValidationSingleAsync(item, transaction, "sp_Validation_KnittingPlanYarn", item.EntityState, userId, item.KPYarnID);
                }
                await _service.SaveAsync(yarnChilds, transaction);

                List<KJobCardMaster> jobCards = new List<KJobCardMaster>();
                entity.Childs.ForEach(x => jobCards.AddRange(x.KJobCardMasters));
                await _service.SaveAsync(jobCards, transaction);

                transaction.Commit();
                transactionGmt.Commit();
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

    }
}