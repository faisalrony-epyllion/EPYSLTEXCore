using Dapper;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.HouseKeeping;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Entity;

namespace EPYSLTEXCore.Application.Services.RND
{
    internal class LabTestRequisitionService : ILabTestRequisitionService
    {
        private readonly IDapperCRUDService<LabTestRequisitionMaster> _service;
        private readonly SqlConnection _connection;
        private readonly SqlConnection _gmtConnection;

        public LabTestRequisitionService(IDapperCRUDService<LabTestRequisitionMaster> service)
        {
            _service = service;
            _service.Connection = _service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _connection = service.Connection;
            _gmtConnection = service.GetConnection(AppConstants.GMT_CONNECTION);
        }

        public async Task<List<LabTestRequisitionMaster>> GetPagedAsync(int isBDS, Status status, PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By LTReqMasterID Desc" : paginationInfo.OrderBy;
            string sql = string.Empty;

            if (status == Status.Pending)
            {
                sql += $@";
                      WITH 
                        BM As(
                            Select a.DBatchID, a.DBatchNo, a.CCColorID, a.DBatchDate, a.RecipeID, a.ColorID, b.Qty BatchWeightKG, b.ConceptID
                            From DyeingBatchMaster a
                            Inner Join DyeingBatchItem b on b.DBatchID = a.DBatchID
	                        Where b.ItemSubGroupID = 1
                        ),M AS (
                            SELECT BM.DBatchID, BM.DBatchNo, BM.DBatchDate, BM.RecipeID, BM.ColorID, BM.BatchWeightKG,
                            BM.CCColorID, LTRM.ContactPersonID, BM.ConceptID, LTRM.IsProduction
                            FROM BM
                            Left Join LabTestRequisitionMaster LTRM On LTRM.DBatchID = BM.DBatchID And LTRM.ConceptID = BM.ConceptID
                            WHERE LTRM.DBatchID IS NULL
                        ),
                        FABRIC AS (
                            SELECT M.DBatchID, M.DBatchNo, FCM.ConceptID, FCM.SubGroupID, ISG.SubGroupName, M.DBatchDate, M.RecipeID, M.ColorID, M.BatchWeightKG FabricQty, COL.SegmentValue ColorName,
                            FCM.ConceptNo, M.ContactPersonID,T.TechnicalName,MSC.SubClassName,Gsm.SegmentValue Gsm,Composition.SegmentValue Composition,FCM.Length,FCM.Width,FU.PartName FUPartName, FCM.GroupConceptNo, FCM.BuyerID, 
                            Buyer=CASE WHEN FCM.BuyerID=0 THEN 'R&D' ELSE ISNULL(B.ShortName,'') END,
                            BuyerTeam=CASE WHEN FCM.BuyerTeamID=0 THEN 'R&D' ELSE ISNULL(CCT.TeamName,'') END,
	                        IsProduction = ISNULL(M.IsProduction,0)
                            FROM M
                            INNER JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID = M.ColorID
                            INNER JOIN FreeConceptMaster FCM ON FCM.ConceptID = M.ConceptID
                            LEFT JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = FCM.BuyerID
                            LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = FCM.BuyerTeamID
                            LEFT JOIN FabricTechnicalName T ON T.TechnicalNameId = FCM.TechnicalNameId
                            LEFT JOIN KnittingMachineSubClass MSC ON MSC.SubClassID = FCM.MCSubClassID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = FCM.GSMID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = FCM.CompositionID
                            LEFT JOIN {DbNames.EPYSL}..FabricUsedPart FU ON FU.FUPartID = FCM.FUPartID
                            LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = FCM.SubGroupID
                        ),
                        BM1 As(
                            Select a.DBatchID, a.DBatchNo, a.CCColorID, a.DBatchDate, a.RecipeID, a.ColorID, b.Qty BatchWeightKG, b.ConceptID, B.ItemSubGroupID
                            From DyeingBatchMaster a
	                        Inner Join DyeingBatchItem b on b.DBatchID = a.DBatchID
	                        Where b.ItemSubGroupID in  (11,12)
                        ),M1 AS (
                            SELECT BM1.DBatchID, BM1.DBatchNo, BM1.DBatchDate, BM1.RecipeID, BM1.ColorID, BM1.BatchWeightKG,
                            BM1.CCColorID, LTRM.ContactPersonID, BM1.ConceptID, LTRM.IsProduction
                            FROM BM1
                            Left Join LabTestRequisitionMaster LTRM On LTRM.DBatchID = BM1.DBatchID And LTRM.SubGroupID = BM1.ItemSubGroupID --And LTRM.ConceptID = BM1.ConceptID
                            WHERE LTRM.DBatchID IS NULL
                        ),
                        CC AS (
	                        SELECT  M1.DBatchID, M1.DBatchNo, 0 ConceptID, FCM.SubGroupID, ISG.SubGroupName, M1.DBatchDate, M1.RecipeID, M1.ColorID, Sum(M1.BatchWeightKG) FabricQty, COL.SegmentValue ColorName,
                            FCM.GroupConceptNo ConceptNo, M1.ContactPersonID, T.TechnicalName, MSC.SubClassName, '' Gsm, '' Composition, 0 Length, 0 Width, 
	                        '' FUPartName, FCM.GroupConceptNo, FCM.BuyerID, ISNULL(B.ShortName,'') Buyer, ISNULL(CCT.TeamName,'') BuyerTeam,
	                        IsProduction = ISNULL(M1.IsProduction,0)
                            FROM M1
                            INNER JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID = M1.ColorID
                            INNER JOIN FreeConceptMaster FCM ON FCM.ConceptID = M1.ConceptID
                            LEFT JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = FCM.BuyerID
                            LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = FCM.BuyerTeamID
                            LEFT JOIN FabricTechnicalName T ON T.TechnicalNameId = FCM.TechnicalNameId
                            LEFT JOIN KnittingMachineSubClass MSC ON MSC.SubClassID = FCM.MCSubClassID
                            LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = FCM.SubGroupID
                            Group by M1.DBatchID, M1.DBatchNo, FCM.SubGroupID, ISG.SubGroupName, M1.DBatchDate, M1.RecipeID, M1.ColorID, COL.SegmentValue,
                            FCM.GroupConceptNo, M1.ContactPersonID, T.TechnicalName, MSC.SubClassName, FCM.GroupConceptNo, FCM.BuyerID, ISNULL(B.ShortName,''), 
	                        ISNULL(CCT.TeamName,''), ISNULL(M1.IsProduction,0)
                        ),
                        BMProd As(
                            Select a.DBatchID, a.DBatchNo, a.CCColorID, a.DBatchDate, a.RecipeID, a.ColorID, b.Qty BatchWeightKG, b.ConceptID
                            From DyeingBatchMaster a
                            Inner Join DyeingBatchItem b on b.DBatchID = a.DBatchID
	                        Where b.ItemSubGroupID = 1
                        ),
                        MProdWithIsProduction AS (
                            SELECT BM.DBatchID, BM.DBatchNo, BM.DBatchDate, BM.RecipeID, BM.ColorID, BM.BatchWeightKG,
                            BM.CCColorID, LTRM.ContactPersonID, BM.ConceptID, LTRM.IsProduction
                            FROM BMProd BM
                            LEFT JOIN LabTestRequisitionMaster LTRM On LTRM.DBatchID = BM.DBatchID And LTRM.ConceptID = BM.ConceptID
                            WHERE LTRM.IsProduction = 1 AND LTRM.LTReqMasterID IS NULL 
                        ),
                        PL AS
                        (
	                        SELECT LTRM.LTReqMasterID, LTRM.ConceptID, LTRM.IsProduction
	                        FROM MProdWithIsProduction MP
	                        INNER JOIN LabTestRequisitionMaster LTRM ON LTRM.ConceptID = MP.ConceptID
	                        WHERE LTRM.IsProduction = 0
	                        GROUP BY LTRM.LTReqMasterID, LTRM.ConceptID, LTRM.IsProduction
                        ),
                        ProdList AS (
                            SELECT M.DBatchID, M.DBatchNo, FCM.ConceptID, FCM.SubGroupID, ISG.SubGroupName, M.DBatchDate, M.RecipeID, M.ColorID, M.BatchWeightKG FabricQty, COL.SegmentValue ColorName,
                            FCM.ConceptNo, M.ContactPersonID,T.TechnicalName,MSC.SubClassName,Gsm.SegmentValue Gsm,Composition.SegmentValue Composition,FCM.Length,FCM.Width,FU.PartName FUPartName, 
	                        FCM.GroupConceptNo, FCM.BuyerID, 
                            Buyer=CASE WHEN FCM.BuyerID=0 THEN 'R&D' ELSE ISNULL(B.ShortName,'') END,
                            BuyerTeam=CASE WHEN FCM.BuyerTeamID=0 THEN 'R&D' ELSE ISNULL(CCT.TeamName,'') END,
	                        IsProduction = ISNULL(M.IsProduction,0)
                            FROM MProdWithIsProduction M
                            INNER JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID = M.ColorID
                            INNER JOIN FreeConceptMaster FCM ON FCM.ConceptID = M.ConceptID
                            LEFT JOIN PL ON PL.ConceptID = M.ConceptID
                            LEFT JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = FCM.BuyerID
                            LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = FCM.BuyerTeamID
                            LEFT JOIN FabricTechnicalName T ON T.TechnicalNameId = FCM.TechnicalNameId
                            LEFT JOIN KnittingMachineSubClass MSC ON MSC.SubClassID = FCM.MCSubClassID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = FCM.GSMID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = FCM.CompositionID
                            LEFT JOIN {DbNames.EPYSL}..FabricUsedPart FU ON FU.FUPartID = FCM.FUPartID
                            LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = FCM.SubGroupID
                        ),
                        ALL_DATA AS (
	                        SELECT *, LabTestStatus = CASE WHEN IsProduction = 1 THEN 'Production' ELSE '' END FROM FABRIC
	                        UNION
	                        SELECT *, LabTestStatus = CASE WHEN IsProduction = 1 THEN 'Production' ELSE '' END FROM CC
	                        UNION
	                        SELECT *, LabTestStatus = CASE WHEN IsProduction = 1 THEN 'Production' ELSE '' END FROM ProdList
                        )
                        SELECT *, Count(*) Over() TotalRows 
                        FROM ALL_DATA ";

                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "ORDER BY DBatchID DESC" : paginationInfo.OrderBy;
            }
            else if (status == Status.Completed)
            {
                sql += $@"
                ;WITH A AS(
	                Select *
	                FROM LabTestRequisitionMaster M
	                WHERE IsApproved = 0 AND IsAcknowledge = 0
                ), 
                TNI AS
                (
	                SELECT LTR.LTReqMasterID, LTR.TestNatureID, TN.TestNatureName
	                FROM LabTestRequisitionBuyerParameter LTR
	                INNER JOIN A ON A.LTReqMasterID = LTR.LTReqMasterID
	                LEFT JOIN TestNature TN ON TN.TestNatureID = LTR.TestNatureID
	                GROUP BY LTR.LTReqMasterID, LTR.TestNatureID, TN.TestNatureName
                ),
                M AS (
	                SELECT A.LTReqMasterID, A.ReqNo, A.ReqDate, A.DBatchID, A.ConceptID, A.GroupConceptNo, A.SubGroupID, A.KnittingUnitID, A.BookingID,
	                A.ExportOrderID, A.BuyerID, A.BuyerTeamID, A.ItemMasterID, A.ColorID, A.FabricQty, A.UnitID, A.Remarks,
	                Batch.DBatchNo, Batch.DBatchDate, A.ContactPersonID, A.IsRetest, A.IsProduction
	                FROM A
	                INNER JOIN DyeingBatchMaster Batch ON Batch.DBatchID = A.DBatchID
                ),
                ALL_DATA AS (
	                SELECT M.*,CM.ConceptNo, ISG.SubGroupName,T.TechnicalName,MSC.SubClassName,Gsm.SegmentValue Gsm,Composition.SegmentValue Composition,CM.Length,CM.Width,FU.PartName FUPartName,COL.SegmentValue ColorName,
                    --ISNULL(B.ShortName,'') Buyer, ISNULL(CCT.TeamName,'') BuyerTeam
                    Buyer=CASE WHEN M.BuyerID=0 THEN 'R&D' ELSE ISNULL(B.ShortName,'') END,
                    BuyerTeam=CASE WHEN M.BuyerTeamID=0 THEN 'R&D' ELSE ISNULL(CCT.TeamName,'') END,
	                LabTestStatus = CASE WHEN M.IsProduction = 1 THEN 'Production' ELSE '' END,
                    TNI.TestNatureName
	                FROM M
	                INNER JOIN FreeConceptMaster CM ON CM.ConceptID = M.ConceptID
                    LEFT JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = CM.BuyerID
                    LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = CM.BuyerTeamID
	                LEFT JOIN FabricTechnicalName T ON T.TechnicalNameId = CM.TechnicalNameId
	                LEFT JOIN KnittingMachineSubClass MSC ON MSC.SubClassID = CM.MCSubClassID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = CM.GSMID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = CM.CompositionID
	                LEFT JOIN {DbNames.EPYSL}..FabricUsedPart FU ON FU.FUPartID = CM.FUPartID
	                INNER JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID = M.ColorID
                    LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = CM.SubGroupID
                    LEFT JOIN TNI ON TNI.LTReqMasterID = M.LTReqMasterID
	                WHERE M.SubGroupID = 1
	                Union
	                SELECT Distinct M.*,CM.ConceptNo, ISG.SubGroupName, T.TechnicalName, MSC.SubClassName, '' Gsm, '' Composition, 0 Length, 0 Width, '' FUPartName,COL.SegmentValue ColorName, 
                    ISNULL(B.ShortName,'') Buyer, ISNULL(CCT.TeamName,'') BuyerTeam,
	                LabTestStatus = CASE WHEN M.IsProduction = 1 THEN 'Production' ELSE '' END,
                    TNI.TestNatureName
	                FROM M
	                INNER JOIN FreeConceptMaster CM ON CM.GroupConceptNo = M.GroupConceptNo
                    LEFT JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = CM.BuyerID
                    LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = CM.BuyerTeamID
	                INNER JOIN FreeConceptChildColor FCC ON FCC.ConceptID = CM.ConceptID And FCC.ColorID = M.ColorID
	                LEFT JOIN FabricTechnicalName T ON T.TechnicalNameId = CM.TechnicalNameId
	                LEFT JOIN KnittingMachineSubClass MSC ON MSC.SubClassID = CM.MCSubClassID
	                INNER JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID = M.ColorID
                    LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = CM.SubGroupID
                    LEFT JOIN TNI ON TNI.LTReqMasterID = M.LTReqMasterID
	                WHERE M.SubGroupID in (11,12)
                )
                SELECT *, Count(*) Over() TotalRows 
                FROM ALL_DATA ";

                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "ORDER BY LTReqMasterID DESC" : paginationInfo.OrderBy;
            }
            else if (status == Status.Approved)
            {
                sql += $@"
                ;WITH A AS(
	                    Select *
	                    FROM LabTestRequisitionMaster M
	                    WHERE IsApproved = 1 AND IsAcknowledge = 0
                ), 
                TNI AS
                (
	                SELECT LTR.LTReqMasterID, LTR.TestNatureID, TN.TestNatureName
	                FROM LabTestRequisitionBuyerParameter LTR
	                INNER JOIN A ON A.LTReqMasterID = LTR.LTReqMasterID
	                LEFT JOIN TestNature TN ON TN.TestNatureID = LTR.TestNatureID
	                GROUP BY LTR.LTReqMasterID, LTR.TestNatureID, TN.TestNatureName
                ),
                M AS (
	                SELECT A.LTReqMasterID, A.ReqNo, A.ReqDate, A.DBatchID, A.ConceptID, A.GroupConceptNo, A.SubGroupID, A.KnittingUnitID, A.BookingID,
	                A.ExportOrderID, A.BuyerID, A.BuyerTeamID, A.ItemMasterID, A.ColorID, A.FabricQty, A.UnitID, A.Remarks,
	                Batch.DBatchNo, Batch.DBatchDate, A.ContactPersonID, A.IsRetest, A.IsProduction
	                FROM A
	                INNER JOIN DyeingBatchMaster Batch ON Batch.DBatchID = A.DBatchID
                ),
                ALL_DATA AS (
	                SELECT M.*,CM.ConceptNo, ISG.SubGroupName,T.TechnicalName,MSC.SubClassName,Gsm.SegmentValue Gsm,Composition.SegmentValue Composition,CM.Length,CM.Width,FU.PartName FUPartName,COL.SegmentValue ColorName,
                    --ISNULL(B.ShortName,'') Buyer, ISNULL(CCT.TeamName,'') BuyerTeam
                    Buyer=CASE WHEN M.BuyerID=0 THEN 'R&D' ELSE ISNULL(B.ShortName,'') END,
                    BuyerTeam=CASE WHEN M.BuyerTeamID=0 THEN 'R&D' ELSE ISNULL(CCT.TeamName,'') END,
	                LabTestStatus = CASE WHEN M.IsProduction = 1 THEN 'Production' ELSE '' END,
                    TNI.TestNatureName
	                FROM M
	                INNER JOIN FreeConceptMaster CM ON CM.ConceptID = M.ConceptID
                    LEFT JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = CM.BuyerID
                    LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = CM.BuyerTeamID
	                LEFT JOIN FabricTechnicalName T ON T.TechnicalNameId = CM.TechnicalNameId
	                LEFT JOIN KnittingMachineSubClass MSC ON MSC.SubClassID = CM.MCSubClassID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = CM.GSMID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = CM.CompositionID
	                LEFT JOIN {DbNames.EPYSL}..FabricUsedPart FU ON FU.FUPartID = CM.FUPartID
	                INNER JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID = M.ColorID
                    LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = CM.SubGroupID
                    LEFT JOIN TNI ON TNI.LTReqMasterID = M.LTReqMasterID
	                WHERE M.SubGroupID = 1
	                Union
	                SELECT Distinct M.*,CM.ConceptNo, ISG.SubGroupName, T.TechnicalName, MSC.SubClassName, '' Gsm, '' Composition, 0 Length, 0 Width, '' FUPartName,COL.SegmentValue ColorName,
                    --ISNULL(B.ShortName,'') Buyer, ISNULL(CCT.TeamName,'') BuyerTeam
                    Buyer=CASE WHEN M.BuyerID=0 THEN 'R&D' ELSE ISNULL(B.ShortName,'') END,
                    BuyerTeam=CASE WHEN M.BuyerTeamID=0 THEN 'R&D' ELSE ISNULL(CCT.TeamName,'') END,
	                LabTestStatus = CASE WHEN M.IsProduction = 1 THEN 'Production' ELSE '' END,
                    TNI.TestNatureName
	                FROM M
	                INNER JOIN FreeConceptMaster CM ON CM.GroupConceptNo = M.GroupConceptNo
                    LEFT JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = CM.BuyerID
                    LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = CM.BuyerTeamID
	                INNER JOIN FreeConceptChildColor FCC ON FCC.ConceptID = CM.ConceptID And FCC.ColorID = M.ColorID
	                LEFT JOIN FabricTechnicalName T ON T.TechnicalNameId = CM.TechnicalNameId
	                LEFT JOIN KnittingMachineSubClass MSC ON MSC.SubClassID = CM.MCSubClassID
	                INNER JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID = M.ColorID
                    LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = CM.SubGroupID
                    LEFT JOIN TNI ON TNI.LTReqMasterID = M.LTReqMasterID
	                WHERE M.SubGroupID in (11,12)
                )
                SELECT *, Count(*) Over() TotalRows 
                FROM ALL_DATA ";

                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "ORDER BY LTReqMasterID DESC" : paginationInfo.OrderBy;
            }
            else if (status == Status.Acknowledge)
            {
                sql += $@";WITH A AS(
	                            Select *
	                            FROM LabTestRequisitionMaster M
	                            WHERE IsApproved = 1 AND IsAcknowledge = 1
                        ), 
                        TNI AS
                        (
	                        SELECT LTR.LTReqMasterID, LTR.TestNatureID, TN.TestNatureName
	                        FROM LabTestRequisitionBuyerParameter LTR
	                        INNER JOIN A ON A.LTReqMasterID = LTR.LTReqMasterID
	                        LEFT JOIN TestNature TN ON TN.TestNatureID = LTR.TestNatureID
	                        GROUP BY LTR.LTReqMasterID, LTR.TestNatureID, TN.TestNatureName
                        ),
                        M AS (
	                        SELECT A.LTReqMasterID, A.ReqNo, A.ReqDate, A.DBatchID, A.ConceptID, A.GroupConceptNo, A.SubGroupID, A.KnittingUnitID, A.BookingID,
	                        A.ExportOrderID, A.BuyerID, A.BuyerTeamID, A.ItemMasterID, A.ColorID, A.FabricQty, A.UnitID, A.Remarks,
	                        Batch.DBatchNo, Batch.DBatchDate, A.ContactPersonID, A.IsRetest, A.IsProduction
	                        FROM A
	                        INNER JOIN DyeingBatchMaster Batch ON Batch.DBatchID = A.DBatchID
                        ),
                        ALL_DATA AS (
	                        SELECT M.*,CM.ConceptNo, ISG.SubGroupName,T.TechnicalName,MSC.SubClassName,Gsm.SegmentValue Gsm,Composition.SegmentValue Composition,CM.Length,CM.Width,FU.PartName FUPartName,COL.SegmentValue ColorName,
                            --ISNULL(B.ShortName,'') Buyer, ISNULL(CCT.TeamName,'') BuyerTeam
                            Buyer=CASE WHEN M.BuyerID=0 THEN 'R&D' ELSE ISNULL(B.ShortName,'') END,
                            BuyerTeam=CASE WHEN M.BuyerTeamID=0 THEN 'R&D' ELSE ISNULL(CCT.TeamName,'') END,
                            LabTestStatus = CASE WHEN M.IsProduction = 1 THEN 'Production' ELSE '' END,
                            TNI.TestNatureName
	                        FROM M
	                        INNER JOIN FreeConceptMaster CM ON CM.ConceptID = M.ConceptID
                            LEFT JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = CM.BuyerID
                            LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = CM.BuyerTeamID
	                        LEFT JOIN FabricTechnicalName T ON T.TechnicalNameId = CM.TechnicalNameId
	                        LEFT JOIN KnittingMachineSubClass MSC ON MSC.SubClassID = CM.MCSubClassID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = CM.GSMID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = CM.CompositionID
	                        LEFT JOIN {DbNames.EPYSL}..FabricUsedPart FU ON FU.FUPartID = CM.FUPartID
	                        INNER JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID = M.ColorID
                            LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = CM.SubGroupID
                            LEFT JOIN TNI ON TNI.LTReqMasterID = M.LTReqMasterID
	                        WHERE M.SubGroupID = 1
		                    Union
	                        SELECT Distinct M.*,CM.ConceptNo, ISG.SubGroupName, T.TechnicalName, MSC.SubClassName, '' Gsm, '' Composition, 0 Length, 0 Width, '' FUPartName,COL.SegmentValue ColorName,
                            --ISNULL(B.ShortName,'') Buyer, ISNULL(CCT.TeamName,'') BuyerTeam
                            Buyer=CASE WHEN M.BuyerID=0 THEN 'R&D' ELSE ISNULL(B.ShortName,'') END,
                            BuyerTeam=CASE WHEN M.BuyerTeamID=0 THEN 'R&D' ELSE ISNULL(CCT.TeamName,'') END,
                            LabTestStatus = CASE WHEN M.IsProduction = 1 THEN 'Production' ELSE '' END,
                            TNI.TestNatureName
	                        FROM M
	                        INNER JOIN FreeConceptMaster CM ON CM.GroupConceptNo = M.GroupConceptNo
                            LEFT JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = CM.BuyerID
                            LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = CM.BuyerTeamID
		                    INNER JOIN FreeConceptChildColor FCC ON FCC.ConceptID = CM.ConceptID And FCC.ColorID = M.ColorID
	                        LEFT JOIN FabricTechnicalName T ON T.TechnicalNameId = CM.TechnicalNameId
	                        LEFT JOIN KnittingMachineSubClass MSC ON MSC.SubClassID = CM.MCSubClassID
	                        INNER JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID = M.ColorID
                            LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = CM.SubGroupID
                            LEFT JOIN TNI ON TNI.LTReqMasterID = M.LTReqMasterID
	                        WHERE M.SubGroupID in (11,12)
	                    )
                    SELECT *, Count(*) Over() TotalRows
                    FROM ALL_DATA";

                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "ORDER BY LTReqMasterID DESC" : paginationInfo.OrderBy;
            }
            else if (status == Status.UnAcknowledge)
            {
                sql += $@";WITH A AS(
	                            Select *
	                            FROM LabTestRequisitionMaster M
	                            WHERE IsApproved = 0 AND IsAcknowledge = 0 AND UnAcknowledge = 1
                        ), 
                        TNI AS
                        (
	                        SELECT LTR.LTReqMasterID, LTR.TestNatureID, TN.TestNatureName
	                        FROM LabTestRequisitionBuyerParameter LTR
	                        INNER JOIN A ON A.LTReqMasterID = LTR.LTReqMasterID
	                        LEFT JOIN TestNature TN ON TN.TestNatureID = LTR.TestNatureID
	                        GROUP BY LTR.LTReqMasterID, LTR.TestNatureID, TN.TestNatureName
                        ),
                        M AS (
	                        SELECT A.LTReqMasterID, A.ReqNo, A.ReqDate, A.DBatchID, A.ConceptID, A.GroupConceptNo, A.SubGroupID, A.KnittingUnitID, A.BookingID,
	                        A.ExportOrderID, A.BuyerID, A.BuyerTeamID, A.ItemMasterID, A.ColorID, A.FabricQty, A.UnitID, A.Remarks,
	                        Batch.DBatchNo, Batch.DBatchDate, A.ContactPersonID, A.IsRetest, A.IsProduction
	                        FROM A
	                        INNER JOIN DyeingBatchMaster Batch ON Batch.DBatchID = A.DBatchID
                        ),
                        ALL_DATA AS (
	                        SELECT M.*,CM.ConceptNo, ISG.SubGroupName,T.TechnicalName,MSC.SubClassName,Gsm.SegmentValue Gsm,Composition.SegmentValue Composition,CM.Length,CM.Width,FU.PartName FUPartName,COL.SegmentValue ColorName,
                            --ISNULL(B.ShortName,'') Buyer, ISNULL(CCT.TeamName,'') BuyerTeam
                            Buyer=CASE WHEN M.BuyerID=0 THEN 'R&D' ELSE ISNULL(B.ShortName,'') END,
                            BuyerTeam=CASE WHEN M.BuyerTeamID=0 THEN 'R&D' ELSE ISNULL(CCT.TeamName,'') END,
                            LabTestStatus = CASE WHEN M.IsProduction = 1 THEN 'Production' ELSE '' END,
                            TNI.TestNatureName
	                        FROM M
	                        INNER JOIN FreeConceptMaster CM ON CM.ConceptID = M.ConceptID
                            LEFT JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = CM.BuyerID
                            LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = CM.BuyerTeamID
	                        LEFT JOIN FabricTechnicalName T ON T.TechnicalNameId = CM.TechnicalNameId
	                        LEFT JOIN KnittingMachineSubClass MSC ON MSC.SubClassID = CM.MCSubClassID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = CM.GSMID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = CM.CompositionID
	                        LEFT JOIN {DbNames.EPYSL}..FabricUsedPart FU ON FU.FUPartID = CM.FUPartID
	                        INNER JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID = M.ColorID
                            LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = CM.SubGroupID
                            LEFT JOIN TNI ON TNI.LTReqMasterID = M.LTReqMasterID
	                        WHERE M.SubGroupID = 1
		                    Union
	                        SELECT Distinct M.*,CM.ConceptNo, ISG.SubGroupName, T.TechnicalName, MSC.SubClassName, '' Gsm, '' Composition, 0 Length, 0 Width, '' FUPartName,COL.SegmentValue ColorName,
                            --ISNULL(B.ShortName,'') Buyer, ISNULL(CCT.TeamName,'') BuyerTeam
                            Buyer=CASE WHEN M.BuyerID=0 THEN 'R&D' ELSE ISNULL(B.ShortName,'') END,
                            BuyerTeam=CASE WHEN M.BuyerTeamID=0 THEN 'R&D' ELSE ISNULL(CCT.TeamName,'') END,
                            LabTestStatus = CASE WHEN M.IsProduction = 1 THEN 'Production' ELSE '' END,
                            TNI.TestNatureName
	                        FROM M
	                        INNER JOIN FreeConceptMaster CM ON CM.GroupConceptNo = M.GroupConceptNo
                            LEFT JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = CM.BuyerID
                            LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = CM.BuyerTeamID
		                    INNER JOIN FreeConceptChildColor FCC ON FCC.ConceptID = CM.ConceptID And FCC.ColorID = M.ColorID
	                        LEFT JOIN FabricTechnicalName T ON T.TechnicalNameId = CM.TechnicalNameId
	                        LEFT JOIN KnittingMachineSubClass MSC ON MSC.SubClassID = CM.MCSubClassID
	                        INNER JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID = M.ColorID
                            LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = CM.SubGroupID
                            LEFT JOIN TNI ON TNI.LTReqMasterID = M.LTReqMasterID
	                        WHERE M.SubGroupID in (11,12)
	                    )
                    SELECT *, Count(*) Over() TotalRows
                    FROM ALL_DATA";

                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "ORDER BY LTReqMasterID DESC" : paginationInfo.OrderBy;
            }
            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<LabTestRequisitionMaster>(sql);
        }

        public async Task<LabTestRequisitionMaster> GetNewAsync(int newId, int conceptId, int subGroupId, int buyerId)
        {
            string sql = string.Empty;
            if (subGroupId == 1)
            {
                sql = $@";WITH 
					D AS (
						SELECT DBI.ConceptID, DBI.DBatchID 
						FROM DyeingBatchItem DBI
						WHERE DBI.DBatchID = {newId} AND DBI.ConceptID = {conceptId}
					),
					M AS (
                        SELECT BM.DBatchID, BM.DBatchNo, BM.DBatchDate, BM.RecipeID, BM.ColorID, FCM.CompositionID,FCM.TechnicalNameId,FCM.MCSubClassID,BM.BatchWeightKG, BM.CCColorID, D.ConceptID, FCM.SubGroupID,
						FCM.GroupConceptNo ConceptNo, FCM.BuyerID, FCM.BuyerTeamID,FCM.ItemMasterID,
                        FCM.BookingID,FCM.ExportOrderID
                        FROM DyeingBatchMaster BM
						INNER JOIN D ON D.DBatchID = BM.DBatchID
                        INNER JOIN FreeConceptMaster FCM ON FCM.ConceptID = D.ConceptID
                    )
                    SELECT M.DBatchID, M.DBatchNo, M.DBatchDate, M.RecipeID, M.ColorID, M.BatchWeightKG FabricQty, COL.SegmentValue ColorName,
                    COM.SegmentValue Composition,T.TechnicalName,MSC.SubClassName,
					M.CCColorID,M.ConceptID, M.ConceptNo, M.SubGroupID, M.BuyerID,M.BuyerTeamID,M.ItemMasterID,
                    M.BookingID,M.ExportOrderID,
                    --ISNULL(B.ShortName,'') Buyer, ISNULL(CCT.TeamName,'') BuyerTeam
                    Buyer=CASE WHEN M.BuyerID=0 THEN 'R&D' ELSE ISNULL(B.ShortName,'') END,
                    BuyerTeam=CASE WHEN M.BuyerTeamID=0 THEN 'R&D' ELSE ISNULL(CCT.TeamName,'') END,
                    CareInstruction = ISNULL(CI.CareInstruction,''),
                    DefaultCareInstruction = ISNULL(CI.CareInstruction,'')
                    FROM M
                    LEFT JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = M.BuyerID
                    LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = M.BuyerTeamID
                    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID = M.ColorID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Com ON COM.SegmentValueID = M.CompositionID
					LEFT JOIN FabricTechnicalName T ON T.TechnicalNameId = M.TechnicalNameId
					LEFT JOIN KnittingMachineSubClass MSC ON MSC.SubClassID = M.MCSubClassID
                    LEFT JOIN LaundryCareLableInstruction CI ON CI.BuyerID = M.BuyerID
                    ;";
            }
            else
            {
                sql = $@";WITH 
	            D AS (
		            SELECT ConceptID = MIN(DBI.ConceptID), DBI.DBatchID 
		            FROM DyeingBatchItem DBI
		            WHERE DBI.DBatchID = {newId} AND DBI.ItemSubGroupID = {subGroupId}
                    GROUP BY DBI.DBatchID
	            ),
	            M AS (
                    SELECT BM.DBatchID, BM.DBatchNo, BM.DBatchDate, BM.RecipeID, BM.ColorID, FCM.CompositionID,FCM.TechnicalNameId,FCM.MCSubClassID,BM.BatchWeightKG, BM.CCColorID, D.ConceptID, FCM.SubGroupID,
		            FCM.GroupConceptNo ConceptNo, FCM.BuyerID, FCM.BuyerTeamID,FCM.ItemMasterID,
                    FCM.BookingID,FCM.ExportOrderID
                    FROM DyeingBatchMaster BM
		            INNER JOIN D ON D.DBatchID = BM.DBatchID
                    INNER JOIN FreeConceptMaster FCM ON FCM.ConceptID = D.ConceptID
                )
                SELECT M.DBatchID, M.DBatchNo, M.DBatchDate, M.RecipeID, M.ColorID, SUM(M.BatchWeightKG) FabricQty, COL.SegmentValue ColorName,
	            COM.SegmentValue Composition,T.TechnicalName,MSC.SubClassName,
                M.CCColorID, M.ConceptID, M.ConceptNo, M.SubGroupID, M.BuyerID, M.BuyerTeamID,M.ItemMasterID,
                M.BookingID,M.ExportOrderID,
                Buyer=CASE WHEN M.BuyerID=0 THEN 'R&D' ELSE ISNULL(B.ShortName,'') END,
                BuyerTeam=CASE WHEN M.BuyerTeamID=0 THEN 'R&D' ELSE ISNULL(CCT.TeamName,'') END,
                CareInstruction = ISNULL(CI.CareInstruction,''),
                DefaultCareInstruction = ISNULL(CI.CareInstruction,'')
                --ISNULL(B.ShortName,'') Buyer, ISNULL(CCT.TeamName,'') BuyerTeam
                
                FROM M
                LEFT JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = M.BuyerID
                LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = M.BuyerTeamID
                INNER JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID = M.ColorID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Com ON COM.SegmentValueID = M.CompositionID
			    LEFT JOIN FabricTechnicalName T ON T.TechnicalNameId = M.TechnicalNameId
			    LEFT JOIN KnittingMachineSubClass MSC ON MSC.SubClassID = M.MCSubClassID
                LEFT JOIN LaundryCareLableInstruction CI ON CI.BuyerID = M.BuyerID
	            GROUP BY M.DBatchID, M.DBatchNo, M.DBatchDate, M.RecipeID, M.ColorID, COL.SegmentValue,
                COM.SegmentValue,T.TechnicalName,MSC.SubClassName,M.BuyerID,M.BuyerTeamID,
	            M.CCColorID, M.ConceptID,M.ConceptNo, M.SubGroupID, ISNULL(B.ShortName,''), ISNULL(CCT.TeamName,''),
                M.ItemMasterID,M.BookingID,M.ExportOrderID, ISNULL(CI.CareInstruction,'')
                ;";
            }
            sql += $@"

                    ----Unit
                    {CommonQueries.GetUnit()};

                   /*
                    ----TestingRequirement
                   WITH SB AS
					(
					   SELECT 0 AS LTReqBPID, BP.BPID, BP.BuyerID, BP.RefValueFrom, BP.RefValueTo, BP.TestName, 
					   SubTestName = BP.TestName, BP.Requirement, TestMethod = ETV.ValueName,
					   BP.SeqNo, STRING_AGG(BPS.BPSubID, ',') SubIDs
					   FROM LabTestBuyerWiseParameter_HK BP
					   LEFT JOIN LabTestBuyerWiseParameterSub_HK BPS ON BPS.BPID = BP.BPID
					   INNER JOIN {DbNames.EPYSL}..EntityTypeValue ETV on ETV.ValueID = BP.TestMethodID
					   WHERE BP.BuyerID = (
                            Select Top 1 BuyerID = CASE WHEN BuyerID = 0 THEN -1 ELSE ISNULL(BuyerID,-1) END 
                            From DyeingBatchItem 
                            WHERE DBatchID = {newId} AND ConceptID = {conceptId}
                            Group By BuyerID
                        )
                       --ISNULL((Select Top 1 BuyerID From DyeingBatchItem WHERE DBatchID = {newId} AND ConceptID = {conceptId} Group By BuyerID),-1)
					   GROUP BY BP.BPID, BP.BuyerID, BP.RefValueFrom, BP.RefValueTo, BP.TestName, BP.Requirement, BP.SeqNo,ETV.ValueName
					),
					BZ AS
					(
					   SELECT 0 AS LTReqBPID, BP.BPID, BP.BuyerID, BP.RefValueFrom, BP.RefValueTo, BP.TestName, 
					   SubTestName = BP.TestName, BP.Requirement, TestMethod = ETV.ValueName,
					   BP.SeqNo, STRING_AGG(BPS.BPSubID, ',') SubIDs
					   FROM LabTestBuyerWiseParameter_HK BP
					   LEFT JOIN LabTestBuyerWiseParameterSub_HK BPS ON BPS.BPID = BP.BPID
					   INNER JOIN {DbNames.EPYSL}..EntityTypeValue ETV on ETV.ValueID = BP.TestMethodID
					   WHERE BP.BuyerID = 0 AND BP.IsProduction = 1
					   GROUP BY BP.BPID, BP.BuyerID, BP.RefValueFrom, BP.RefValueTo, BP.TestName, BP.Requirement, BP.SeqNo,ETV.ValueName
					),
					FinalList AS 
                    (
	                    SELECT * FROM SB
	                    UNION 
	                    SELECT * FROM BZ
                    )
                    SELECT * FROM FinalList ORDER BY SeqNo, TestName;
                    */

                    ----Sub Sub TestingRequirement
                    WITH SB AS
                    (
	                    SELECT BPS.BPSubID AS LTReqBPID, BP.BPID, BP.BuyerID, BP.RefValueFrom, BP.TestName, 
	                    BPS.SubTestName, BPS.SubSubTestName, BP.SeqNo, TestMethod = ETV.ValueName
	                    FROM LabTestBuyerWiseParameter_HK BP
	                    LEFT JOIN LabTestBuyerWiseParameterSub_HK BPS ON BPS.BPID = BP.BPID
	                    INNER JOIN {DbNames.EPYSL}..EntityTypeValue ETV on ETV.ValueID = BP.TestMethodID
	                    WHERE BP.BuyerID = (
                            Select Top 1 BuyerID = CASE WHEN BuyerID = 0 THEN -1 ELSE ISNULL(BuyerID,-1) END 
                            From DyeingBatchItem 
                            WHERE DBatchID = {newId} AND ConceptID = {conceptId} 
                            Group By BuyerID
                        )
	                    GROUP BY BPS.BPSubID, BP.BPID, BP.BuyerID, BP.RefValueFrom, BP.TestName, BPS.SubTestName, BPS.SubSubTestName, BP.SeqNo,ETV.ValueName
                    ),
                    BZ AS
                    (
	                    SELECT BPS.BPSubID AS LTReqBPID, BP.BPID, BP.BuyerID, BP.RefValueFrom, BP.TestName, 
	                    BPS.SubTestName, BPS.SubSubTestName, BP.SeqNo, TestMethod = ETV.ValueName
	                    FROM LabTestBuyerWiseParameter_HK BP
	                    LEFT JOIN LabTestBuyerWiseParameterSub_HK BPS ON BPS.BPID = BP.BPID
	                    INNER JOIN {DbNames.EPYSL}..EntityTypeValue ETV on ETV.ValueID = BP.TestMethodID
	                    WHERE BP.BuyerID = 0 AND BP.IsProduction = 1
	                    GROUP BY BPS.BPSubID, BP.BPID, BP.BuyerID, BP.RefValueFrom, BP.TestName, BPS.SubTestName, BPS.SubSubTestName, BP.SeqNo,ETV.ValueName
                    ),
                    FinalList AS 
                    (
	                    SELECT * FROM SB
	                    UNION 
	                    SELECT * FROM BZ
                    )
                    SELECT * FROM FinalList ORDER BY SeqNo, TestName, SubSubTestName;

                    -- LabTestServiceType
                    SELECT CAST(LabTestServiceTypeID AS VARCHAR) id, SubTest text
                    FROM {DbNames.EPYSLTEX}..LabTestServiceType;
                     
                    -- Contact List
                   Select E.EmployeeCode id, E.DisplayEmployeeCode +' '+EmployeeName text
                   From {DbNames.EPYSL}..Employee E 
                   INNER JOIN {DbNames.EPYSL}..EmployeeDepartment ED ON ED.DepertmentID= E.DepertmentID 
                   Where ED.DepertmentDescription = 'Laboratory';
                
                    --Regions
                    SELECT C.*
                    FROM {DbNames.EPYSL}..CountryRegion C
                    ORDER BY C.RegionName;

                    --StyleGenders
                    SELECT SG.StyleGenderID, SG.StyleGenderName 
                    FROM {DbNames.EPYSL}..StyleGender SG
                    ORDER BY SG.StyleGenderName

                    --FinishDyeMethod
                    SELECT LT.FinishDyeMethodID, LT.FinishDyeName, LT.MethodType
                    FROM LabTestSpecialFinishDyeMethod_HK LT
                    WHERE LT.BuyerID = {buyerId} AND LT.IsActive = 1
                    ORDER BY LT.FinishDyeName

                    --TestNature
                    SELECT id = TestNatureID, [text] = TestNatureName
                    FROM TestNature";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                LabTestRequisitionMaster data = records.Read<LabTestRequisitionMaster>().FirstOrDefault();
                data.UnitList = records.Read<Select2OptionModel>().ToList();

                LabTestRequisitionBuyer buyerData = new LabTestRequisitionBuyer();
                if (data.BuyerID > 0)
                {
                    buyerData.BuyerID = data.BuyerID;
                    buyerData.BuyerName = data.Buyer;

                }
                else
                {
                    buyerData.BuyerID = data.BuyerID;
                    buyerData.BuyerName = "Epyllion Standard";
                }

                //buyerData.NewLabTestRequisitionBuyerParameters = records.Read<LabTestRequisitionBuyerParameter>().ToList();
                data.LabTestRequisitionBuyers.Add(buyerData);
                data.BuyerParameters = records.Read<LabTestRequisitionBuyerParameter>().ToList();
                data.LabTestServiceTypeList = await records.ReadAsync<Select2OptionModel>();
                data.ContactPersonList = await records.ReadAsync<Select2OptionModel>();
                data.CountryList = records.Read<LabTestRequisitionExportCountry>().ToList();
                data.EndUsesList = records.Read<LabTestRequisitionEndUse>().ToList();
                data.FinishDyeMethodsList = records.Read<LabTestRequisitionSpecialFinishDyeMethod>().ToList();
                //data.NewLabTestRequisitionBuyerParameters = buyerData.NewLabTestRequisitionBuyerParameters;
                data.TestNatureList = await records.ReadAsync<Select2OptionModel>();

                #region Wash Temp
                List<int> washTemps = new List<int>() { 0, 30, 40, 60 };
                washTemps.ForEach(x =>
                {
                    data.WashTempList.Add(new Select2OptionModel()
                    {
                        id = x.ToString(),
                        text = x.ToString()
                    });
                });
                #endregion

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

        public async Task<LabTestRequisitionMaster> GetAsync(int id, bool IsRetestFlag, int buyerId)
        {
            string sql = "";
            if (IsRetestFlag) // Click Retest Button
            {
                sql = $@"
                    ;WITH M AS (
                        SELECT	* FROM LabTestRequisitionMaster WHERE LTReqMasterID = {id}
                    )
                    SELECT M.LabTestServiceTypeID,M.IsRetest, 0 As LTReqMasterID, '**<< NEW >>**' As ReqNo, Getdate() As ReqDate, 
                    M.DBatchID, M.ConceptID,M.KnittingUnitID, M.BookingID, M.ExportOrderID, M.BuyerID, M.BuyerTeamID, M.IsProduction, M.WashTemp, 
                    M.ItemMasterID, M.ColorID, M.FabricQty, M.UnitID, M.Remarks, M.CareInstruction, Batch.DBatchNo, Getdate() As DBatchDate, 
                    M.GroupConceptNo ConceptNo, COL.SegmentValue ColorName, M.ContactPersonID, CM.SubGroupID,
                    DefaultCareInstruction = ISNULL(CI.CareInstruction,''),
                    (Case When M.RetestNo Is null Then M.ReqNo Else M.ReqNo End) RetestNo, E.EmployeeName ContactPersonName,
                    Buyer = CASE WHEN ISNULL(CM.BuyerID,0) > 0 THEN B.Name ELSE 'R&D' END,
                    BuyerTeam = CASE WHEN ISNULL(CM.BuyerTeamID,0) > 0 THEN CCT.TeamName ELSE 'R&D' END
                    FROM M
                    INNER JOIN DyeingBatchMaster Batch ON Batch.DBatchID = M.DBatchID
                    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID = M.ColorID
                    LEFT JOIN FreeConceptMaster CM ON CM.ConceptID = M.ConceptID
                    LEFT JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = CM.BuyerID
                    LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = CM.BuyerTeamID
                    INNER JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = M.ContactPersonID
                    LEFT JOIN LaundryCareLableInstruction CI ON CI.BuyerID = M.BuyerID;

                    -----childs
                    ;WITH M AS (
                        SELECT	* FROM LabTestRequisitionBuyer WHERE LTReqMasterID = {id}
                    )
                    SELECT M.LTReqBuyerID, M.BuyerID, M.LTReqMasterID, Contacts.Name BuyerName
                    FROM M
                    INNER JOIN {DbNames.EPYSL}..Contacts ON Contacts.ContactID = M.BuyerID;

                    ----Buyer Group Parameter
                    ;WITH M AS (
                        SELECT	* FROM LabTestRequisitionBuyerParameter WHERE LTReqMasterID = {id}
                    )
                    SELECT M.LTReqMasterID, M.BuyerID,	M.BPID, M.RefValueFrom, M.RefValueTo,BWP.TestName,BWP.Requirement,
					M.TestValue, M.TestValue1, M.Requirement, M.Requirement1, M.IsPass, M.Remarks, M.LTReqBuyerID, 
                    Contacts.Name BuyerName, BWP.RefValueFrom, BWP.RefValueTo, TestMethod = ETV.ValueName,
					STRING_AGG(M.BPSubID, ',') SubIDs
                    FROM M
                    INNER JOIN LabTestBuyerWiseParameter_HK BWP ON BWP.BPID = M.BPID
                    INNER JOIN {DbNames.EPYSL}..Contacts ON Contacts.ContactID = M.BuyerID
                    INNER JOIN {DbNames.EPYSL}..EntityTypeValue ETV on ETV.ValueID = BWP.TestMethodID
					GROUP BY M.LTReqMasterID, M.BuyerID,	M.BPID, M.RefValueFrom, M.RefValueTo,BWP.TestName,BWP.Requirement, ETV.ValueName,
					M.TestValue, M.TestValue1, M.Requirement, M.Requirement1, M.IsPass, M.Remarks, M.LTReqBuyerID, Contacts.Name, BWP.RefValueFrom, BWP.RefValueTo;
                    
                    ----Buyer Parameters
                    ;WITH M AS (
                        SELECT	* FROM LabTestRequisitionBuyerParameter WHERE LTReqMasterID = {id}
                    )
                    SELECT M.LTReqBPID, M.LTReqMasterID, M.LTReqBuyerID, M.BuyerID, M.BPID, M.BPSubID,
					M.TestValue, M.TestValue1, M.Requirement, M.Requirement1, M.IsPass, M.Remarks, M.TestNatureID, Contacts.Name BuyerName, BWP.TestName,
					SUB.SubTestName, SUB.SubSubTestName, SUB.Requirement RefValueFrom, SUB.Requirement RefValueTo, 
                    TestMethod = ETV.ValueName, TN.TestNatureName
                    FROM M
                    INNER JOIN LabTestBuyerWiseParameter_HK BWP ON BWP.BPID = M.BPID
                    INNER JOIN {DbNames.EPYSL}..Contacts ON Contacts.ContactID = M.BuyerID
					INNER JOIN LabTestBuyerWiseParameterSub_HK SUB ON SUB.BPSubID = M.BPSubID
                    INNER JOIN {DbNames.EPYSL}..EntityTypeValue ETV on ETV.ValueID = BWP.TestMethodID
                    LEFT JOIN TestNature TN ON TN.TestNatureID = M.TestNatureID
                    Order By SUB.SeqNo;

                    --LabTestRequisitionExportCountry
					SELECT L.*, C.RegionName 
					FROM LabTestRequisitionExportCountry L
					INNER JOIN {DbNames.EPYSL}..CountryRegion C ON C.CountryRegionID = L.CountryRegionID
					WHERE L.LTReqMasterID = {id};
					
					--LabTestRequisitionEndUse
					SELECT L.*, SG.StyleGenderName 
					FROM LabTestRequisitionEndUse L
					INNER JOIN {DbNames.EPYSL}..StyleGender SG ON SG.StyleGenderID = L.StyleGenderID
					WHERE L.LTReqMasterID = {id};

					--LabTestRequisitionSpecialFinishDyeMethod
					SELECT L.*, HK.MethodType
					FROM LabTestRequisitionSpecialFinishDyeMethod L
					INNER JOIN LabTestSpecialFinishDyeMethod_HK HK ON HK.FinishDyeMethodID = L.FinishDyeMethodID
					WHERE L.LTReqMasterID = {id};

                    -----Care Label
                    ;WITH M AS (
                        SELECT	* FROM LabTestRequisitionCareLabel WHERE LTReqMasterID = {id}
                    )
                    SELECT M.LTReqCareLabelID, M.LTReqMasterID, M.LCareLableID, M.SeqNo, M.GroupCode, CL.CareType, CL.CareName, CL.ImagePath
                    FROM M
                    INNER JOIN LaundryCareLable_HK CL ON CL.LCareLableID = M.LCareLableID;

                    ----Unit
                    {CommonQueries.GetUnit()};

                     ----TestingRequirement
                   SELECT 0 AS LTReqBPID, BP.BPID, BP.BuyerID, BP.RefValueFrom, 
                   BP.RefValueTo, BP.TestName, BP.Requirement, TestMethod = ETV.ValueName,
				   BP.SeqNo, STRING_AGG(BPS.BPSubID, ',') SubIDs
                   FROM LabTestBuyerWiseParameter_HK BP
				   INNER JOIN LabTestBuyerWiseParameterSub_HK BPS ON BPS.BPID = BP.BPID
                   INNER JOIN {DbNames.EPYSL}..EntityTypeValue ETV on ETV.ValueID = BP.TestMethodID
				   WHERE BP.BuyerID = 0
				   GROUP BY BP.BPID, BP.BuyerID, BP.RefValueFrom, BP.RefValueTo, BP.TestName, BP.Requirement, BP.SeqNo, ETV.ValueName
                   Order by BP.SeqNo, BP.TestName;

                    --LabTestServiceType
                    SELECT CAST(LabTestServiceTypeID AS VARCHAR) id, SubTest text
                    FROM { DbNames.EPYSLTEX}..LabTestServiceType;

                    -- Contact List
                   Select E.EmployeeCode id, E.DisplayEmployeeCode +' '+EmployeeName text
                   From {DbNames.EPYSL}..Employee E 
                   INNER JOIN {DbNames.EPYSL}..EmployeeDepartment ED ON ED.DepertmentID= E.DepertmentID 
                   Where ED.DepertmentDescription = 'Laboratory'; 

                    --Regions
                    SELECT C.*
                    FROM {DbNames.EPYSL}..CountryRegion C
                    ORDER BY C.RegionName;

                    --StyleGenders
                    SELECT SG.StyleGenderID, SG.StyleGenderName 
                    FROM {DbNames.EPYSL}..StyleGender SG
                    ORDER BY SG.StyleGenderName

                     --FinishDyeMethod
                    SELECT LT.FinishDyeMethodID, LT.FinishDyeName, LT.MethodType
                    FROM LabTestSpecialFinishDyeMethod_HK LT
                    WHERE LT.BuyerID = {buyerId} AND LT.IsActive = 1
                    ORDER BY LT.FinishDyeName;

                    --TestNature
                    SELECT id = TestNatureID, [text] = TestNatureName
                    FROM TestNature;";
            }
            else
            {
                sql = $@"
                    ;WITH M AS (
                        SELECT	* FROM LabTestRequisitionMaster WHERE LTReqMasterID = {id}
                    )
                    SELECT M.LabTestServiceTypeID,M.IsRetest,M.LTReqMasterID, M.ReqNo, M.ReqDate, M.DBatchID, M.ConceptID,M.KnittingUnitID, 
                    M.BookingID, M.ExportOrderID, M.BuyerID, M.BuyerTeamID, M.ItemMasterID, M.ColorID,M.FabricQty, M.UnitID, M.Remarks, M.IsProduction, M.WashTemp,
                    Batch.DBatchNo, Batch.DBatchDate, M.GroupConceptNo ConceptNo,COL.SegmentValue ColorName,
                    COM.SegmentValue Composition,T.TechnicalName,MSC.SubClassName,
                    M.ContactPersonID, M.CareInstruction,M.UnAcknowledgeReason,
                    --CM.SubGroupID,
                    IsNull(M.RetestNo,M.ReqNo)RetestNo, E.EmployeeName ContactPersonName,M.SubGroupID, 
                    --ISNULL(B.ShortName,'') Buyer, ISNULL(CCT.TeamName,'') BuyerTeam
                    Buyer = Case When M.BuyerID = 0 then 'R&D'  Else B.ShortName End,
                    BuyerTeam = Case When M.BuyerTeamID = 0 then 'R&D'  Else CCT.TeamName End,
                    DefaultCareInstruction = ISNULL(CI.CareInstruction,'')
                    FROM M
                    INNER JOIN DyeingBatchMaster Batch ON Batch.DBatchID = M.DBatchID
                    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID = M.ColorID
                    LEFT JOIN FreeConceptMaster CM ON CM.ConceptID = M.ConceptID
                    LEFT JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = CM.BuyerID
                    LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = CM.BuyerTeamID
                    INNER JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = M.ContactPersonID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue COM ON COM.SegmentValueID = CM.CompositionID
                    LEFT JOIN FabricTechnicalName T ON T.TechnicalNameId = CM.TechnicalNameId
                    LEFT JOIN KnittingMachineSubClass MSC ON MSC.SubClassID = CM.MCSubClassID
                    LEFT JOIN LaundryCareLableInstruction CI ON CI.BuyerID = M.BuyerID
                    WHERE M.SubGroupID = 1

                    Union all

                    SELECT M.LabTestServiceTypeID,M.IsRetest,M.LTReqMasterID, M.ReqNo, M.ReqDate, M.DBatchID, M.ConceptID,M.KnittingUnitID, 
                    M.BookingID, M.ExportOrderID, M.BuyerID, M.BuyerTeamID, M.ItemMasterID, M.ColorID,M.FabricQty, M.UnitID, M.Remarks, M.IsProduction, M.WashTemp, 
                    Batch.DBatchNo, Batch.DBatchDate, M.GroupConceptNo ConceptNo,COL.SegmentValue ColorName,
                    COM.SegmentValue Composition,T.TechnicalName,MSC.SubClassName,
                    M.ContactPersonID, M.CareInstruction, M.UnAcknowledgeReason,
                    --CM.SubGroupID,
                    IsNull(M.RetestNo,M.ReqNo)RetestNo, E.EmployeeName ContactPersonName,M.SubGroupID, 
                    --ISNULL(B.ShortName,'') Buyer, ISNULL(CCT.TeamName,'') BuyerTeam
                    Buyer = Case When M.BuyerID = 0 then 'R&D'  Else B.ShortName End,
                    BuyerTeam = Case When M.BuyerTeamID = 0 then 'R&D'  Else CCT.TeamName End,
                    DefaultCareInstruction = ISNULL(CI.CareInstruction,'')
                    FROM M
                    INNER JOIN DyeingBatchMaster Batch ON Batch.DBatchID = M.DBatchID 
                    INNER JOIN FreeConceptMaster CM ON CM.GroupConceptNo = M.GroupConceptNo
                    LEFT JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = CM.BuyerID
                    LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = CM.BuyerTeamID
                    INNER JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = M.ContactPersonID
                    INNER JOIN FreeConceptChildColor FCC ON FCC.ConceptID = CM.ConceptID And FCC.ColorID = M.ColorID
                    LEFT JOIN FabricTechnicalName T ON T.TechnicalNameId = CM.TechnicalNameId
                    LEFT JOIN KnittingMachineSubClass MSC ON MSC.SubClassID = CM.MCSubClassID
                    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID = M.ColorID
                    LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = CM.SubGroupID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue COM ON COM.SegmentValueID = CM.CompositionID
                    LEFT JOIN LaundryCareLableInstruction CI ON CI.BuyerID = M.BuyerID
                    WHERE M.SubGroupID in (11,12);

                    -----childs
                    ;WITH M AS (
                        SELECT	* FROM LabTestRequisitionBuyer WHERE LTReqMasterID = {id}
                    )
                    SELECT M.LTReqBuyerID, M.BuyerID, M.LTReqMasterID, Contacts.Name BuyerName
                    FROM M
                    INNER JOIN {DbNames.EPYSL}..Contacts ON Contacts.ContactID = M.BuyerID;

                    ----Buyer Group Parameter
                    ;WITH M AS (
                        SELECT	* FROM LabTestRequisitionBuyerParameter WHERE LTReqMasterID = {id}
                    )
                    SELECT M.LTReqMasterID, M.BuyerID,	M.BPID, M.RefValueFrom, M.TestNatureID, TN.TestNatureName, M.RefValueTo,BWP.TestName,BWP.Requirement,
					M.TestValue, M.TestValue1, M.Requirement, M.Requirement1, M.IsPass, M.Remarks, M.LTReqBuyerID, 
                    Contacts.Name BuyerName, BWP.RefValueFrom, BWP.RefValueTo, TestMethod = ETV.ValueName,
					STRING_AGG(M.BPSubID, ',') SubIDs
                    FROM M
                    INNER JOIN LabTestBuyerWiseParameter_HK BWP ON BWP.BPID = M.BPID
                    INNER JOIN {DbNames.EPYSL}..EntityTypeValue ETV on ETV.ValueID = BWP.TestMethodID
                    INNER JOIN {DbNames.EPYSL}..Contacts ON Contacts.ContactID = M.BuyerID
                    LEFT JOIN TestNature TN ON TN.TestNatureID = M.TestNatureID
					GROUP BY M.LTReqMasterID, M.BuyerID,	M.BPID, M.RefValueFrom, M.TestNatureID, TN.TestNatureName, M.RefValueTo,BWP.TestName,BWP.Requirement, ETV.ValueName,
					M.TestValue, M.TestValue1, M.Requirement, M.Requirement1, M.IsPass, M.Remarks, M.LTReqBuyerID, Contacts.Name, BWP.RefValueFrom, BWP.RefValueTo;
                    
                    ----Buyer Parameters
                    ;WITH M AS (
                        SELECT	* FROM LabTestRequisitionBuyerParameter WHERE LTReqMasterID = {id}
                    )
                    SELECT M.LTReqMasterID, M.BuyerID,	M.BPID, M.RefValueFrom, M.RefValueTo, M.TestNatureID, BWP.TestName,BWP.Requirement,
                    M.TestValue, M.TestValue1, M.Requirement, M.Requirement1, M.IsPass, M.Remarks, M.LTReqBuyerID, 
                    Contacts.Name BuyerName, BWP.RefValueFrom, BWP.RefValueTo, TestMethod = ETV.ValueName, TN.TestNatureName,
                    STRING_AGG(M.BPSubID, ',') SubIDs
                    FROM M
                    INNER JOIN LabTestBuyerWiseParameter_HK BWP ON BWP.BPID = M.BPID
                    INNER JOIN {DbNames.EPYSL}..EntityTypeValue ETV on ETV.ValueID = BWP.TestMethodID
                    INNER JOIN {DbNames.EPYSL}..Contacts ON Contacts.ContactID = M.BuyerID
                    LEFT JOIN TestNature TN ON TN.TestNatureID = M.TestNatureID
                    GROUP BY M.LTReqMasterID, M.BuyerID, M.BPID, M.RefValueFrom, M.RefValueTo, M.TestNatureID,BWP.TestName,BWP.Requirement, ETV.ValueName,
                    M.TestValue, M.TestValue1, M.Requirement, M.Requirement1, M.IsPass, M.Remarks, M.LTReqBuyerID, Contacts.Name, BWP.RefValueFrom, BWP.RefValueTo, TN.TestNatureName;

                    --LabTestRequisitionExportCountry
                    SELECT L.*, C.RegionName 
					FROM LabTestRequisitionExportCountry L
					INNER JOIN {DbNames.EPYSL}..CountryRegion C ON C.CountryRegionID = L.CountryRegionID
					WHERE L.LTReqMasterID = {id};
					
                    --LabTestRequisitionEndUse
					SELECT L.*, SG.StyleGenderName 
					FROM LabTestRequisitionEndUse L
					INNER JOIN {DbNames.EPYSL}..StyleGender SG ON SG.StyleGenderID = L.StyleGenderID
					WHERE L.LTReqMasterID = {id};

                    --LabTestRequisitionSpecialFinishDyeMethod
					SELECT L.*, HK.MethodType
					FROM LabTestRequisitionSpecialFinishDyeMethod L
                    INNER JOIN LabTestSpecialFinishDyeMethod_HK HK ON HK.FinishDyeMethodID = L.FinishDyeMethodID
					WHERE L.LTReqMasterID = {id};
                    
                    -----Care Label
                    ;WITH M AS (
                        SELECT	* FROM LabTestRequisitionCareLabel WHERE LTReqMasterID = {id}
                    )
                    SELECT M.LTReqCareLabelID, M.LTReqMasterID, M.LCareLableID, M.SeqNo, M.GroupCode, CL.CareType, CL.CareName, CL.ImagePath
                    FROM M
                    INNER JOIN LaundryCareLable_HK CL ON CL.LCareLableID = M.LCareLableID;

                    ----Unit
                    {CommonQueries.GetUnit()};

                     ----TestingRequirement
                    WITH ML AS
					(
					   SELECT 0 AS LTReqBPID, BP.BPID, BP.BuyerID, BP.RefValueFrom, 
					   BP.RefValueTo, BP.TestName, BP.Requirement, TestMethod = ETV.ValueName,
					   BP.SeqNo, STRING_AGG(BPS.BPSubID, ',') SubIDs
					   FROM LabTestBuyerWiseParameter_HK BP
					   INNER JOIN LabTestBuyerWiseParameterSub_HK BPS ON BPS.BPID = BP.BPID
					   INNER JOIN {DbNames.EPYSL}..EntityTypeValue ETV on ETV.ValueID = BP.TestMethodID
					   --WHERE BP.BuyerID = ISNULL((Select Top 1 BuyerID From LabTestRequisitionBuyerParameter WHERE LTReqMasterID = {id} Group By BuyerID),-1)
                       WHERE BP.BuyerID = (
                            Select Top 1 BuyerID = CASE WHEN BuyerID = 0 THEN -1 ELSE ISNULL(BuyerID,-1) END 
                            From LabTestRequisitionBuyerParameter 
                            WHERE LTReqMasterID = {id} 
                            Group By BuyerID
                        )
					   GROUP BY BP.BPID, BP.BuyerID, BP.RefValueFrom, BP.RefValueTo, BP.TestName, BP.Requirement, BP.SeqNo, ETV.ValueName
					),
					PL AS
					(
					   SELECT 0 AS LTReqBPID, BP.BPID, BP.BuyerID, BP.RefValueFrom, 
					   BP.RefValueTo, BP.TestName, BP.Requirement, TestMethod = ETV.ValueName,
					   BP.SeqNo, STRING_AGG(BPS.BPSubID, ',') SubIDs
					   FROM LabTestBuyerWiseParameter_HK BP
					   INNER JOIN LabTestBuyerWiseParameterSub_HK BPS ON BPS.BPID = BP.BPID
					   INNER JOIN {DbNames.EPYSL}..EntityTypeValue ETV on ETV.ValueID = BP.TestMethodID
					   WHERE BP.BuyerID = 0 AND BP.IsProduction = 1
					   GROUP BY BP.BPID, BP.BuyerID, BP.RefValueFrom, BP.RefValueTo, BP.TestName, BP.Requirement, BP.SeqNo, ETV.ValueName
					),
					FinalList AS
					(
						SELECT * FROM ML
						UNION
						SELECT * FROM PL
					)
					SELECT * FROM FinalList ORDER BY SeqNo, TestName;

                    --LabTestServiceType
                    SELECT CAST(LabTestServiceTypeID AS VARCHAR) id, SubTest text
                    FROM { DbNames.EPYSLTEX}..LabTestServiceType;

                    -- Contact List
                   Select E.EmployeeCode id, E.DisplayEmployeeCode +' '+EmployeeName text
                   From {DbNames.EPYSL}..Employee E 
                   INNER JOIN {DbNames.EPYSL}..EmployeeDepartment ED ON ED.DepertmentID= E.DepertmentID 
                   Where ED.DepertmentDescription = 'Laboratory'; 

                    --Regions
                    SELECT C.*
                    FROM {DbNames.EPYSL}..CountryRegion C
                    ORDER BY C.RegionName;

                    --StyleGenders
                    SELECT SG.StyleGenderID, SG.StyleGenderName 
                    FROM {DbNames.EPYSL}..StyleGender SG
                    ORDER BY SG.StyleGenderName

                     --FinishDyeMethod
                    SELECT LT.FinishDyeMethodID, LT.FinishDyeName, LT.MethodType
                    FROM LabTestSpecialFinishDyeMethod_HK LT
                    WHERE LT.BuyerID = {buyerId} AND LT.IsActive = 1
                    ORDER BY LT.FinishDyeName;

                    --TestNature
                    SELECT id = TestNatureID, [text] = TestNatureName
                    FROM TestNature;";
            }
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                LabTestRequisitionMaster data = records.Read<LabTestRequisitionMaster>().FirstOrDefault();
                data.LabTestRequisitionBuyers = records.Read<LabTestRequisitionBuyer>().ToList();
                data.LabTestRequisitionBuyers[0].LabTestRequisitionBuyerParameters = records.Read<LabTestRequisitionBuyerParameter>().ToList();
                data.BuyerParameters = records.Read<LabTestRequisitionBuyerParameter>().ToList();

                data.Countries = records.Read<LabTestRequisitionExportCountry>().ToList();
                data.EndUses = records.Read<LabTestRequisitionEndUse>().ToList();
                data.FinishDyeMethods = records.Read<LabTestRequisitionSpecialFinishDyeMethod>().ToList();

                data.CareLabels = records.Read<LabTestRequisitionCareLabel>().ToList();
                data.UnitList = records.Read<Select2OptionModel>().ToList();
                data.LabTestRequisitionBuyers[0].NewLabTestRequisitionBuyerParameters = records.Read<LabTestRequisitionBuyerParameter>().ToList();

                data.LabTestServiceTypeList = records.Read<Select2OptionModel>().ToList();
                data.ContactPersonList = await records.ReadAsync<Select2OptionModel>();
                data.CountryList = records.Read<LabTestRequisitionExportCountry>().ToList();
                data.EndUsesList = records.Read<LabTestRequisitionEndUse>().ToList();
                data.FinishDyeMethodsList = records.Read<LabTestRequisitionSpecialFinishDyeMethod>().ToList();
                data.TestNatureList = await records.ReadAsync<Select2OptionModel>();

                data.NewLabTestRequisitionBuyerParameters = data.LabTestRequisitionBuyers[0].NewLabTestRequisitionBuyerParameters;
                data.TestNatureID = data.BuyerParameters.Count() > 0 ? data.BuyerParameters.First().TestNatureID : 0;

                #region Wash Temp
                List<int> washTemps = new List<int>() { 0, 30, 40, 60 };
                washTemps.ForEach(x =>
                {
                    data.WashTempList.Add(new Select2OptionModel()
                    {
                        id = x.ToString(),
                        text = x.ToString()
                    });
                });
                #endregion

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

        public async Task<List<LabTestRequisitionBuyerParameter>> GetBuyerParameterByBuyerId(int id)
        {
            string sql = $@"
                    ;SELECT BWP.BPID, BWP.BuyerID, BWP.TestName ParameterName, BWP.RefValueFrom, 
                    BWP.RefValueTo, Contacts.Name BuyerName,BWP.Requirement, TestMethod = ETV.ValueName
                    FROM LabTestBuyerWiseParameter_HK BWP
                    INNER JOIN {DbNames.EPYSL}..EntityTypeValue ETV on ETV.ValueID = BWP.TestMethodID
                    INNER JOIN {DbNames.EPYSL}..Contacts ON Contacts.ContactID = BWP.BuyerID
                    WHERE BuyerID = {id}
                   Order by BWP.SeqNo, BWP.TestName;";

            return await _service.GetDataAsync<LabTestRequisitionBuyerParameter>(sql);
        }

        public async Task<LabTestRequisitionMaster> GetAllByIDAsync(int id)
        {
            string sql = $@"
            ;Select * From LabTestRequisitionMaster Where LTReqMasterID = {id}

            ;Select * From LabTestRequisitionBuyer Where LTReqMasterID = {id}

            ;Select * From LabTestRequisitionBuyerParameter Where LTReqMasterID = {id}

            ;Select * From LabTestRequisitionCareLabel Where LTReqMasterID = {id}

            ;Select * From LabTestRequisitionExportCountry Where LTReqMasterID = {id}

            ;Select * From LabTestRequisitionEndUse Where LTReqMasterID = {id}

            ;Select * From LabTestRequisitionSpecialFinishDyeMethod Where LTReqMasterID = {id}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                LabTestRequisitionMaster data = records.Read<LabTestRequisitionMaster>().FirstOrDefault();
                data.LabTestRequisitionBuyers = records.Read<LabTestRequisitionBuyer>().ToList();
                data.LabTestRequisitionBuyerParameters = records.Read<LabTestRequisitionBuyerParameter>().ToList();
                data.CareLabels = records.Read<LabTestRequisitionCareLabel>().ToList();
                data.Countries = records.Read<LabTestRequisitionExportCountry>().ToList();
                data.EndUses = records.Read<LabTestRequisitionEndUse>().ToList();
                data.FinishDyeMethods = records.Read<LabTestRequisitionSpecialFinishDyeMethod>().ToList();
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
        public async Task<List<LabTestRequisitionBuyerParameter>> GetLabTestRequisitionBuyerParameters(PaginationInfo paginationInfo, int buyerID, int testNatureID, int isProduction)
        {
            string orderBy = " ORDER BY SeqNo, TestName";
            string sql = $@"
                    WITH FinalList AS
                    (
	                    SELECT 0 AS LTReqBPID, BP.BPID, BP.BuyerID, BP.RefValueFrom, BP.RefValueTo, BP.TestName, 
	                    SubTestName = BP.TestName, BP.Requirement, TestMethod = ETV.ValueName,
	                    BP.SeqNo, STRING_AGG(BPS.BPSubID, ',') SubIDs, BP.TestNatureID
	                    FROM LabTestBuyerWiseParameter_HK BP
	                    LEFT JOIN LabTestBuyerWiseParameterSub_HK BPS ON BPS.BPID = BP.BPID
	                    INNER JOIN {DbNames.EPYSL}..EntityTypeValue ETV on ETV.ValueID = BP.TestMethodID
	                    WHERE BP.BuyerID = {buyerID} AND BP.IsProduction = {isProduction} AND BP.TestNatureID = {testNatureID}
	                    GROUP BY BP.BPID, BP.BuyerID, BP.RefValueFrom, BP.RefValueTo, BP.TestName, BP.Requirement, BP.SeqNo,ETV.ValueName, BP.TestNatureID
                    )
                    SELECT *, Count(*) Over() TotalRows FROM FinalList";

            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<LabTestRequisitionBuyerParameter>(sql);
        }
        public async Task<List<LaundryCareLable_HK>> GetAllLaundryCareLablesAsync(PaginationInfo paginationInfo)
        {
            string orderBy = " Order By SeqNo";

            string sql = $@"
                    WITH B AS
                    (
	                    SELECT CL.LCareLableID, CL.CareType, CL.CareName, CL.ImagePath, CL.SeqNo
                        FROM LaundryCareLable_HK CL
                    )
                    SELECT *, Count(*) Over() TotalRows FROM B";

            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<LaundryCareLable_HK>(sql);
        }
        public async Task<List<LaundryCareLableBuyerCode>> GetLaundryCareLableCodes(int buyerId, PaginationInfo paginationInfo)
        {
            string orderBy = " ORDER BY CareLableCode";

            string sql = $@"
                    WITH B AS 
                    (
	                    SELECT LCC.CareLableCode,LCC.GroupCode
	                    FROM LaundryCareLableBuyerCode LCC
	                    WHERE LCC.BuyerID = {buyerId}
	                    GROUP BY LCC.CareLableCode,LCC.GroupCode
                    )
                    SELECT *, Count(*) Over() TotalRows FROM B";

            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<LaundryCareLableBuyerCode>(sql);
        }
        public async Task<List<LaundryCareLableBuyerCode>> GetCareLablesByCode(string careLableCode)
        {
            string sql = $@"
                    SELECT LCC.*, LCL.CareType, LCL.CareName, LCL.ImagePath
                    FROM LaundryCareLableBuyerCode LCC
                    INNER JOIN LaundryCareLable_HK LCL ON LCL.LCareLableID = LCC.LCareLableID
                    WHERE LCC.CareLableCode = '{careLableCode}'
                    ORDER BY LCC.SeqNo, LCC.CareLableCode";
            return await _service.GetDataAsync<LaundryCareLableBuyerCode>(sql);
        }
        public async Task<List<LaundryCareLableBuyerCode>> GetCareLebelsByCodes(string careLableCodes,int buyerID)
        {
            careLableCodes = careLableCodes.Replace(",", "','");
            string sql = $@"
                    SELECT LCC.*, LCL.CareType, LCL.CareName, LCL.ImagePath
                    FROM LaundryCareLableBuyerCode LCC
                    INNER JOIN LaundryCareLable_HK LCL ON LCL.LCareLableID = LCC.LCareLableID
                    WHERE LCC.CareLableCode IN ('{careLableCodes}') AND LCC.BuyerID = {buyerID}
                    ORDER BY LCC.SeqNo, LCC.CareLableCode";
            return await _service.GetDataAsync<LaundryCareLableBuyerCode>(sql);
        }

        public async Task<LabTestRequisitionMaster> SaveAsync(LabTestRequisitionMaster entity)
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
                await _service.SaveAsync(entity.LabTestRequisitionBuyers, transaction);
                List<LabTestRequisitionBuyerParameter> buyerParmsList = new List<LabTestRequisitionBuyerParameter>();
                entity.LabTestRequisitionBuyers.ForEach(x =>
                {
                    buyerParmsList.AddRange(x.LabTestRequisitionBuyerParameters);
                });
                await _service.SaveAsync(buyerParmsList, transaction);
                await _service.SaveAsync(entity.CareLabels, transaction);
                await _service.SaveAsync(entity.Countries, transaction);
                await _service.SaveAsync(entity.EndUses, transaction);
                await _service.SaveAsync(entity.FinishDyeMethods, transaction);
                transaction.Commit();

                //LabTestStatus = CASE WHEN M.IsProduction = 1 THEN 'Production' ELSE '' END

                entity.LabTestStatus = entity.IsProduction ? "Production" : "";

                return entity;
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

        private async Task<LabTestRequisitionMaster> AddAsync(LabTestRequisitionMaster entity)
        {
            entity.LTReqMasterID = await _service.GetMaxIdAsync(TableNames.LAB_TEST_REQUISITION_MASTER);
            entity.ReqNo = await _service.GetMaxNoAsync(TableNames.REQ_NO);
            int maxLabTestRequisitionBuyerId = await _service.GetMaxIdAsync(TableNames.LAB_TEST_REQUISITION_BUYER, entity.LabTestRequisitionBuyers.Count());
            int maxLabTestRequisitionBuyerParamId = await _service.GetMaxIdAsync(TableNames.LAB_TEST_REQUISITION_BUYER_PARAMETER, entity.LabTestRequisitionBuyers.Sum(x => x.LabTestRequisitionBuyerParameters.Count()));
            int maxLabTestRequisitionCareLableId = await _service.GetMaxIdAsync(TableNames.LAB_TEST_REQUISITION_CARE_LABEL, entity.CareLabels.Count());

            int maxCountriesId = await _service.GetMaxIdAsync(TableNames.LAB_TEST_REQUISITION_EXPORT_COUNTRY, entity.Countries.Count());
            int maxEndUsesId = await _service.GetMaxIdAsync(TableNames.LAB_TEST_REQUISITION_END_USE, entity.EndUses.Count());
            int maxFinishDyeMethodsId = await _service.GetMaxIdAsync(TableNames.LAB_TEST_REQUISITION_FINISH_DYE_METHOD, entity.FinishDyeMethods.Count());

            int nLTReqBuyerID = 0;

            #region Buyer & Buyer Parameter
            foreach (LabTestRequisitionBuyer item in entity.LabTestRequisitionBuyers)
            {
                item.LTReqBuyerID = maxLabTestRequisitionBuyerId++;
                item.LTReqMasterID = entity.LTReqMasterID;
                foreach (LabTestRequisitionBuyerParameter child in item.LabTestRequisitionBuyerParameters)
                {
                    child.LTReqBPID = maxLabTestRequisitionBuyerParamId++;
                    child.LTReqBuyerID = item.LTReqBuyerID;
                    child.LTReqMasterID = entity.LTReqMasterID;

                    if (nLTReqBuyerID == 0)
                    {
                        nLTReqBuyerID = child.LTReqBuyerID;
                    }
                }
            }
            #endregion

            #region Care Lables
            foreach (LabTestRequisitionCareLabel item in entity.CareLabels)
            {
                item.LTReqCareLabelID = maxLabTestRequisitionCareLableId++;
                item.LTReqMasterID = entity.LTReqMasterID;
            }
            #endregion

            #region LabTestRequisitionExportCountry
            foreach (LabTestRequisitionExportCountry item in entity.Countries)
            {
                item.LTRECountryID = maxCountriesId++;
                item.LTReqMasterID = entity.LTReqMasterID;
                item.LTReqBuyerID = nLTReqBuyerID;
            }
            #endregion

            #region LabTestRequisitionEndUse
            foreach (LabTestRequisitionEndUse item in entity.EndUses)
            {
                item.LTREndUseID = maxEndUsesId++;
                item.LTReqMasterID = entity.LTReqMasterID;
                item.LTReqBuyerID = nLTReqBuyerID;
            }
            #endregion

            #region LabTestRequisitionSpecialFinishDyeMethod
            foreach (LabTestRequisitionSpecialFinishDyeMethod item in entity.FinishDyeMethods)
            {
                item.LTRFinishDyeMethodID = maxFinishDyeMethodsId++;
                item.LTReqMasterID = entity.LTReqMasterID;
                item.LTReqBuyerID = nLTReqBuyerID;
            }
            #endregion

            return entity;
        }

        private async Task<LabTestRequisitionMaster> UpdateAsync(LabTestRequisitionMaster entity)
        {
            int maxLabTestRequisitionBuyerId = await _service.GetMaxIdAsync(TableNames.LAB_TEST_REQUISITION_BUYER, entity.LabTestRequisitionBuyers.Where(x => x.EntityState == EntityState.Added).Count());
            int maxLabTestRequisitionBuyerParamId = await _service.GetMaxIdAsync(TableNames.LAB_TEST_REQUISITION_BUYER_PARAMETER, entity.LabTestRequisitionBuyers.Sum(x => x.LabTestRequisitionBuyerParameters.Where(y => y.EntityState == EntityState.Added).Count()));
            int maxLabTestRequisitionCareLableId = await _service.GetMaxIdAsync(TableNames.LAB_TEST_REQUISITION_CARE_LABEL, entity.CareLabels.Where(x => x.EntityState == EntityState.Added).Count());

            int maxCountriesId = await _service.GetMaxIdAsync(TableNames.LAB_TEST_REQUISITION_EXPORT_COUNTRY, entity.Countries.Where(x => x.EntityState == EntityState.Added).Count());
            int maxEndUsesId = await _service.GetMaxIdAsync(TableNames.LAB_TEST_REQUISITION_END_USE, entity.EndUses.Where(x => x.EntityState == EntityState.Added).Count());
            int maxFinishDyeMethodsId = await _service.GetMaxIdAsync(TableNames.LAB_TEST_REQUISITION_FINISH_DYE_METHOD, entity.FinishDyeMethods.Where(x => x.EntityState == EntityState.Added).Count());

            int nLTReqBuyerID = 0;


            #region Buyer & Buyer Parameter
            foreach (LabTestRequisitionBuyer item in entity.LabTestRequisitionBuyers)
            {
                #region Buyer Parameters
                foreach (LabTestRequisitionBuyerParameter child in item.LabTestRequisitionBuyerParameters.ToList())
                {
                    switch (child.EntityState)
                    {
                        case EntityState.Added:
                            child.LTReqBPID = maxLabTestRequisitionBuyerParamId++;
                            child.LTReqBuyerID = item.LTReqBuyerID;
                            child.LTReqMasterID = entity.LTReqMasterID;
                            break;

                        case EntityState.Unchanged:
                        case EntityState.Deleted:
                            child.EntityState = EntityState.Deleted;
                            break;

                        case EntityState.Modified:
                            child.EntityState = EntityState.Modified;
                            break;

                        default:
                            break;
                    }
                }
                #endregion

                switch (item.EntityState)
                {
                    case EntityState.Added:
                        item.LTReqBuyerID = maxLabTestRequisitionBuyerId++;
                        item.LTReqMasterID = entity.LTReqMasterID;
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
                if (nLTReqBuyerID == 0)
                {
                    nLTReqBuyerID = item.LTReqBuyerID;
                }
            }
            #endregion  

            #region Care Lables
            foreach (LabTestRequisitionCareLabel item in entity.CareLabels)
            {
                switch (item.EntityState)
                {
                    case EntityState.Added:
                        item.LTReqCareLabelID = maxLabTestRequisitionCareLableId++;
                        item.LTReqMasterID = entity.LTReqMasterID;
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
            #endregion

            #region LabTestRequisitionExportCountry
            foreach (LabTestRequisitionExportCountry item in entity.Countries)
            {
                switch (item.EntityState)
                {
                    case EntityState.Added:
                        item.LTRECountryID = maxCountriesId++;
                        item.LTReqMasterID = entity.LTReqMasterID;
                        item.LTReqBuyerID = nLTReqBuyerID;
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
            #endregion

            #region LabTestRequisitionEndUse
            foreach (LabTestRequisitionEndUse item in entity.EndUses)
            {
                switch (item.EntityState)
                {
                    case EntityState.Added:
                        item.LTREndUseID = maxEndUsesId++;
                        item.LTReqMasterID = entity.LTReqMasterID;
                        item.LTReqBuyerID = nLTReqBuyerID;
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
            #endregion

            #region LabTestRequisitionSpecialFinishDyeMethod
            foreach (LabTestRequisitionSpecialFinishDyeMethod item in entity.FinishDyeMethods)
            {
                switch (item.EntityState)
                {
                    case EntityState.Added:
                        item.FinishDyeMethodID = maxFinishDyeMethodsId++;
                        item.LTReqMasterID = entity.LTReqMasterID;
                        item.LTReqBuyerID = nLTReqBuyerID;
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
            #endregion

            return entity;
        }

        public async Task UpdateEntityAsync(LabTestRequisitionMaster entity)
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

        public async Task<LabTestRequisitionMaster> ReviseAsync(LabTestRequisitionMaster entity)
        {
            SqlTransaction transaction = null;
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                //only for revision after UnAcknowledge
                await _connection.ExecuteAsync("spBackupLabTestRequisition_Full", new { LTReqMasterID = entity.LTReqMasterID }, transaction, 30, CommandType.StoredProcedure);
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
                await _service.SaveAsync(entity.LabTestRequisitionBuyers, transaction);
                List<LabTestRequisitionBuyerParameter> buyerParmsList = new List<LabTestRequisitionBuyerParameter>();
                entity.LabTestRequisitionBuyers.ForEach(x =>
                {
                    buyerParmsList.AddRange(x.LabTestRequisitionBuyerParameters);
                });
                await _service.SaveAsync(buyerParmsList, transaction);
                await _service.SaveAsync(entity.CareLabels, transaction);
                await _service.SaveAsync(entity.Countries, transaction);
                await _service.SaveAsync(entity.EndUses, transaction);
                await _service.SaveAsync(entity.FinishDyeMethods, transaction);
                transaction.Commit();

                //LabTestStatus = CASE WHEN M.IsProduction = 1 THEN 'Production' ELSE '' END

                entity.LabTestStatus = entity.IsProduction ? "Production" : "";

                return entity;
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