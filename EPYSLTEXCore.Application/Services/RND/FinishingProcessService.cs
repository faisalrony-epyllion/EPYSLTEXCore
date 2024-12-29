using Dapper;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Application.Interfaces;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.General;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.Data.SqlClient;
using System.Data.Entity;

namespace EPYSLTEX.Core.Interfaces.Services
{
    public class FinishingProcessService : IFinishingProcessService
    {
        private readonly IDapperCRUDService<FinishingProcessMaster> _service;
        private readonly SqlConnection _connection;
        private readonly SqlConnection _gmtConnection;

        public FinishingProcessService(IDapperCRUDService<FinishingProcessMaster> service
)
        {
            _service = service;
            _service.Connection = _service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _connection = service.Connection;
            _gmtConnection = service.GetConnection(AppConstants.GMT_CONNECTION);
        }

        public async Task<List<FinishingProcessMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By FPMasterID Desc" : paginationInfo.OrderBy;
            string sql;
            if (status == Status.Pre_Pending)
            {
                sql = $@"WITH 
                NeedFPPList AS
                (
	                SELECT KPM.ConceptID, KPM.NeedPreFinishingProcess
	                FROM KnittingPlanMaster KPM
	                WHERE KPM.NeedPreFinishingProcess = 1
	                GROUP BY KPM.ConceptID, KPM.NeedPreFinishingProcess
                ),
                PendingFPPCon1 AS
                (
	                SELECT FPMasterID = 0, KPM.ConceptID, PFBatchNo = '', KPM.NeedPreFinishingProcess
	                FROM NeedFPPList KPM
	                WHERE KPM.ConceptID NOT IN 
	                (SELECT FPM.ConceptID FROM FinishingProcessMaster FPM)
                ),
                PendingFPPCon2 AS
                (
	                SELECT FPM.FPMasterID, FPM.ConceptID, FPM.PFBatchNo, NFPP.NeedPreFinishingProcess
	                FROM FinishingProcessMaster FPM
	                INNER JOIN NeedFPPList NFPP ON NFPP.ConceptID = FPM.ConceptID
	                LEFT JOIN FinishingProcessChild FPC ON FPC.FPMasterID = FPM.FPMasterID
	                WHERE FPM.FPMasterID NOT IN 
	                (SELECT FPC.FPMasterID FROM FinishingProcessChild FPC WHERE FPC.IsPreProcess = 1) 
	                GROUP BY FPM.FPMasterID, FPM.ConceptID, FPM.PFBatchNo, NFPP.NeedPreFinishingProcess
                ),

                R AS (
	                SELECT FPMasterID, ConceptID, PFBatchNo, NeedPreFinishingProcess FROM PendingFPPCon1
	                UNION
	                SELECT FPMasterID, ConceptID, PFBatchNo, NeedPreFinishingProcess FROM PendingFPPCon2
                ),
                FinalList AS
                (
	                SELECT
                    R.FPMasterID, R.PFBatchNo , R.NeedPreFinishingProcess,
	                CM.MachineGauge,CM.Length,CM.Width, Gsm.SegmentValue Gsm,
	                Composition.SegmentValue Composition,MSC.SubClassName, CM.ConceptID,CM.TechnicalNameId,
	                Technical.TechnicalName,CM.CompositionID, CM.ConceptNo, CM.ConceptDate,CM.MCSubClassID,
	                CM.SubGroupID,IG.SubGroupName,CM.FUPartID,FU.PartName FUPartName, CM.IsBDS, CM.GroupConceptNo,
	                ColorName = Case When CM.ISBDS = 0 Then '' Else FCC.ColorName End
	                FROM R
	                INNER JOIN FreeConceptMaster CM ON CM.ConceptID = R.ConceptID
	                INNER JOIN KnittingPlanMaster KPM ON R.ConceptID=KPM.ConceptID
	                LEFT JOIN FreeConceptChildColor FCC ON FCC.ConceptID = CM.ConceptID
	                LEFT JOIN FinishingProcessMaster FPM ON FPM.ConceptID = R.ConceptID
	                LEFT JOIN KnittingMachineSubClass MSC ON MSC.SubClassID = CM.MCSubClassID
	                LEFT JOIN FabricTechnicalName Technical ON Technical.TechnicalNameId=CM.TechnicalNameId
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = CM.CompositionID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = CM.GSMID
	                INNER JOIN {DbNames.EPYSL}..ItemSubGroup IG ON IG.SubGroupID = CM.SubGroupID
	                LEFT JOIN {DbNames.EPYSL}..FabricUsedPart FU ON FU.FUPartID = CM.FUPartID
	                WHERE KPM.GrayFabricOK = 1
	                --AND FPM.ConceptID IS NULL 
	                --AND KP.NeedPreFinishingProcess=1     
	                GROUP BY R.FPMasterID, R.PFBatchNo, R.NeedPreFinishingProcess,CM.MachineGauge,CM.Length,CM.Width, Gsm.SegmentValue,KPM.GrayFabricOK,
	                Composition.SegmentValue,MSC.SubClassName, CM.ConceptID,CM.TechnicalNameId,
	                Technical.TechnicalName,CM.CompositionID, CM.ConceptNo, CM.ConceptDate,CM.MCSubClassID,
	                CM.SubGroupID,IG.SubGroupName,CM.FUPartID,FU.PartName, CM.IsBDS, CM.GroupConceptNo,
	                Case When CM.ISBDS = 0 Then '' Else FCC.ColorName End
                )
                SELECT FinalList.*, Count(*) Over() TotalRows
                FROM FinalList
               ";
                #region 14-Sept-2022
                //14-Sept-2022
                //           sql = $@"WITH F AS (
                //            SELECT CM.MachineGauge,CM.Length,CM.Width, Gsm.SegmentValue Gsm,
                //            Composition.SegmentValue Composition,MSC.SubClassName, CM.ConceptID,CM.TechnicalNameId,
                //            Technical.TechnicalName,CM.CompositionID, CM.ConceptNo, CM.ConceptDate,CM.MCSubClassID,
                //            CM.SubGroupID,IG.SubGroupName,CM.FUPartID,FU.PartName FUPartName, CM.IsBDS, CM.GroupConceptNo,
                //ColorName = Case When CM.ISBDS = 0 Then '' Else FCC.ColorName End
                //            FROM KnittingPlanMaster KP
                //            INNER JOIN FreeConceptMaster CM ON CM.ConceptID = KP.ConceptID
                //LEFT JOIN FreeConceptChildColor FCC ON FCC.ConceptID = CM.ConceptID
                //            LEFT JOIN FinishingProcessMaster FPM ON FPM.ConceptID = KP.ConceptID
                //            LEFT JOIN KnittingMachineSubClass MSC ON MSC.SubClassID = CM.MCSubClassID
                //            LEFT JOIN FabricTechnicalName Technical ON Technical.TechnicalNameId=CM.TechnicalNameId
                //            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = CM.CompositionID
                //            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = CM.GSMID
                //            INNER JOIN {DbNames.EPYSL}..ItemSubGroup IG ON IG.SubGroupID = CM.SubGroupID
                //            LEFT JOIN {DbNames.EPYSL}..FabricUsedPart FU ON FU.FUPartID = CM.FUPartID
                //            --WHERE KP.GrayFabricOK = 1 AND FPM.ConceptID IS NULL
                //               WHERE KP.GrayFabricOK = 1 AND FPM.ConceptID IS NULL AND KP.NeedPreFinishingProcess=1     
                //            GROUP BY CM.MachineGauge,CM.Length,CM.Width, Gsm.SegmentValue,
                //            Composition.SegmentValue,MSC.SubClassName, CM.ConceptID,CM.TechnicalNameId,
                //            Technical.TechnicalName,CM.CompositionID, CM.ConceptNo, CM.ConceptDate,CM.MCSubClassID,
                //            CM.SubGroupID,IG.SubGroupName,CM.FUPartID,FU.PartName, CM.IsBDS, CM.GroupConceptNo,
                //Case When CM.ISBDS = 0 Then '' Else FCC.ColorName End
                //           )
                //           SELECT F.*, Count(*) Over() TotalRows
                //           FROM F
                //          ";
                //end-14-Sept-2022
                #endregion 14-Sept-2022
                orderBy = " ORDER BY FinalList.ConceptID DESC";
            }
            else if (status == Status.Post_Pending)
            {
                #region 14-Sept-2022
                //           sql = $@"WITH F AS (
                //            SELECT CM.MachineGauge,CM.Length,CM.Width, Gsm.SegmentValue Gsm,
                //            Composition.SegmentValue Composition,MSC.SubClassName, CM.ConceptID,CM.TechnicalNameId,
                //            Technical.TechnicalName,CM.CompositionID, CM.ConceptNo, CM.ConceptDate,CM.MCSubClassID,
                //            CM.SubGroupID,IG.SubGroupName,CM.FUPartID,FU.PartName FUPartName, CM.IsBDS, CM.GroupConceptNo,
                //ColorName = Case When CM.ISBDS = 0 Then '' Else FCC.ColorName End
                //            FROM KnittingPlanMaster KP
                //            INNER JOIN FreeConceptMaster CM ON CM.ConceptID = KP.ConceptID
                //LEFT JOIN FreeConceptChildColor FCC ON FCC.ConceptID = CM.ConceptID
                //            LEFT JOIN FinishingProcessMaster FPM ON FPM.ConceptID = KP.ConceptID
                //            LEFT JOIN KnittingMachineSubClass MSC ON MSC.SubClassID = CM.MCSubClassID
                //            LEFT JOIN FabricTechnicalName Technical ON Technical.TechnicalNameId=CM.TechnicalNameId
                //            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = CM.CompositionID
                //            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = CM.GSMID
                //            INNER JOIN {DbNames.EPYSL}..ItemSubGroup IG ON IG.SubGroupID = CM.SubGroupID
                //            LEFT JOIN {DbNames.EPYSL}..FabricUsedPart FU ON FU.FUPartID = CM.FUPartID
                //            --WHERE KP.GrayFabricOK = 1 AND FPM.ConceptID IS NULL
                //               WHERE KP.GrayFabricOK = 1 AND FPM.ConceptID IS NULL AND KP.NeedPreFinishingProcess=0     
                //            GROUP BY CM.MachineGauge,CM.Length,CM.Width, Gsm.SegmentValue,
                //            Composition.SegmentValue,MSC.SubClassName, CM.ConceptID,CM.TechnicalNameId,
                //            Technical.TechnicalName,CM.CompositionID, CM.ConceptNo, CM.ConceptDate,CM.MCSubClassID,
                //            CM.SubGroupID,IG.SubGroupName,CM.FUPartID,FU.PartName, CM.IsBDS, CM.GroupConceptNo,
                //Case When CM.ISBDS = 0 Then '' Else FCC.ColorName End
                //           )
                //           SELECT F.*, Count(*) Over() TotalRows
                //           FROM F
                //          ";
                #endregion 14-Sept-2022
                sql = $@"WITH 
                        NeedFPreList AS
                        (
	                        SELECT KPM.ConceptID, KPM.NeedPreFinishingProcess
	                        FROM KnittingPlanMaster KPM
	                        WHERE KPM.NeedPreFinishingProcess = 1
	                        GROUP BY KPM.ConceptID, KPM.NeedPreFinishingProcess
                        ),
                        NeedFPostList AS
                        (
	                        SELECT KPM.ConceptID, KPM.NeedPreFinishingProcess
	                        FROM KnittingPlanMaster KPM
	                        WHERE KPM.NeedPreFinishingProcess = 0
	                        GROUP BY KPM.ConceptID, KPM.NeedPreFinishingProcess
                        ),
                        PendingFPostCon1 AS
                        (
	                        SELECT FPMasterID = 0, KPM.ConceptID, PFBatchNo = '', KPM.NeedPreFinishingProcess
	                        FROM NeedFPostList KPM
	                        WHERE KPM.ConceptID NOT IN 
	                        (SELECT FPM.ConceptID FROM FinishingProcessMaster FPM)
                        ),
                        PendingFPostCon2 AS
                        (
	                        SELECT FPM.FPMasterID, FPM.ConceptID, FPM.PFBatchNo, NFPP.NeedPreFinishingProcess
	                        FROM FinishingProcessMaster FPM
	                        INNER JOIN NeedFPreList NFPP ON NFPP.ConceptID = FPM.ConceptID
	                        LEFT JOIN FinishingProcessChild FPC ON FPC.FPMasterID = FPM.FPMasterID
	                        WHERE FPM.FPMasterID IN (SELECT FPC.FPMasterID FROM FinishingProcessChild FPC WHERE FPC.IsPreProcess = 1)
	                        AND FPM.FPMasterID NOT IN (SELECT FPC.FPMasterID FROM FinishingProcessChild FPC WHERE FPC.IsPreProcess = 0)
	                        GROUP BY FPM.FPMasterID, FPM.ConceptID, FPM.PFBatchNo, NFPP.NeedPreFinishingProcess
                        ),
                        R AS (
	                        SELECT FPMasterID, ConceptID, PFBatchNo, NeedPreFinishingProcess FROM PendingFPostCon1
	                        UNION
	                        SELECT FPMasterID, ConceptID, PFBatchNo, NeedPreFinishingProcess FROM PendingFPostCon2
                        ),
                        FinalList AS
                        (
	                        SELECT
                            R.FPMasterID, R.PFBatchNo, R.NeedPreFinishingProcess,
	                        CM.MachineGauge,CM.Length,CM.Width, Gsm.SegmentValue Gsm,
	                        Composition.SegmentValue Composition,MSC.SubClassName, CM.ConceptID,CM.TechnicalNameId,
	                        Technical.TechnicalName,CM.CompositionID, CM.ConceptNo, CM.ConceptDate,CM.MCSubClassID,
	                        CM.SubGroupID,IG.SubGroupName,CM.FUPartID,FU.PartName FUPartName, CM.IsBDS, CM.GroupConceptNo,
	                        ColorName = Case When CM.ISBDS = 0 Then '' Else FCC.ColorName End
	                        FROM R
	                        INNER JOIN FreeConceptMaster CM ON CM.ConceptID = R.ConceptID
	                        INNER JOIN KnittingPlanMaster KPM ON R.ConceptID=KPM.ConceptID
	                        LEFT JOIN FreeConceptChildColor FCC ON FCC.ConceptID = CM.ConceptID
	                        LEFT JOIN FinishingProcessMaster FPM ON FPM.ConceptID = R.ConceptID
	                        LEFT JOIN KnittingMachineSubClass MSC ON MSC.SubClassID = CM.MCSubClassID
	                        LEFT JOIN FabricTechnicalName Technical ON Technical.TechnicalNameId=CM.TechnicalNameId
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = CM.CompositionID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = CM.GSMID
	                        INNER JOIN {DbNames.EPYSL}..ItemSubGroup IG ON IG.SubGroupID = CM.SubGroupID
	                        LEFT JOIN {DbNames.EPYSL}..FabricUsedPart FU ON FU.FUPartID = CM.FUPartID
	                        WHERE KPM.GrayFabricOK = 1
	                        --AND FPM.ConceptID IS NULL 
	                        --AND KP.NeedPreFinishingProcess=1     
	                        GROUP BY R.FPMasterID, R.PFBatchNo, R.NeedPreFinishingProcess ,CM.MachineGauge,CM.Length,CM.Width, Gsm.SegmentValue,KPM.GrayFabricOK,
	                        Composition.SegmentValue,MSC.SubClassName, CM.ConceptID,CM.TechnicalNameId,
	                        Technical.TechnicalName,CM.CompositionID, CM.ConceptNo, CM.ConceptDate,CM.MCSubClassID,
	                        CM.SubGroupID,IG.SubGroupName,CM.FUPartID,FU.PartName, CM.IsBDS, CM.GroupConceptNo,
	                        Case When CM.ISBDS = 0 Then '' Else FCC.ColorName End
                        )
                    SELECT FinalList.*, Count(*) Over() TotalRows
                    FROM FinalList
                ";
                orderBy = " ORDER BY FinalList.ConceptID DESC";
            }
            else
            {
                #region completelist
                //sql = $@"WITH F AS (
                //SELECT FP.FPMasterID, FP.ConceptID, FP.BookingID, FP.TrialNo, FP.TrialDate, CM.ConceptNo, CM.ConceptDate,FP.PFBatchNo,FP.PFBatchDate,FP.BatchQty,
                //MSC.SubClassName, Technical.TechnicalName, Composition.SegmentValue Composition, Gsm.SegmentValue Gsm, IG.SubGroupName, FU.PartName FUPartName,
                //CM.[Length], CM.Width, CM.IsBDS, CM.GroupConceptNo,
                //ColorName = Case When CM.ISBDS = 0 Then '' Else FCC.ColorName End
                //FROM FinishingProcessMaster FP
                //LEFT JOIN FreeConceptMaster CM ON CM.ConceptID = FP.ConceptID
                //LEFT JOIN FreeConceptChildColor FCC ON FCC.ConceptID = CM.ConceptID
                //LEFT JOIN KnittingMachineSubClass MSC ON MSC.SubClassID = CM.MCSubClassID
                //LEFT JOIN FabricTechnicalName Technical ON Technical.TechnicalNameId=CM.TechnicalNameId
                //LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = CM.CompositionID
                //LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = CM.GSMID
                //INNER JOIN {DbNames.EPYSL}..ItemSubGroup IG ON IG.SubGroupID = CM.SubGroupID
                //LEFT JOIN {DbNames.EPYSL}..FabricUsedPart FU ON FU.FUPartID = CM.FUPartID
                //GROUP BY  FP.FPMasterID, FP.ConceptID, FP.BookingID, FP.TrialNo, FP.TrialDate, CM.ConceptNo, CM.ConceptDate,FP.PFBatchNo,FP.PFBatchDate,FP.BatchQty,
                //MSC.SubClassName, Technical.TechnicalName, Composition.SegmentValue, Gsm.SegmentValue, IG.SubGroupName, FU.PartName,
                //CM.[Length], CM.Width, CM.IsBDS, CM.GroupConceptNo,
                //Case When CM.ISBDS = 0 Then '' Else FCC.ColorName End
                //)
                //SELECT F.*, Count(*) Over() TotalRows
                //FROM F";
                #endregion completelist
                sql = $@"WITH 
                NeedFPreList AS
                (
                 SELECT KPM.ConceptID, KPM.NeedPreFinishingProcess
                 FROM KnittingPlanMaster KPM
                 WHERE KPM.NeedPreFinishingProcess = 1
                 GROUP BY KPM.ConceptID, KPM.NeedPreFinishingProcess
                ),
                NeedFPostList AS
                (
                 SELECT KPM.ConceptID, KPM.NeedPreFinishingProcess
                 FROM KnittingPlanMaster KPM
                 WHERE KPM.NeedPreFinishingProcess = 0
                 GROUP BY KPM.ConceptID, KPM.NeedPreFinishingProcess
                ),
                FPreDoneList AS
                (
                 SELECT FPM.FPMasterID, FPM.ConceptID, NFPP.NeedPreFinishingProcess
                 FROM FinishingProcessMaster FPM
                 INNER JOIN NeedFPreList NFPP ON NFPP.ConceptID = FPM.ConceptID
                 LEFT JOIN FinishingProcessChild FPC ON FPC.FPMasterID = FPM.FPMasterID
                 WHERE FPM.FPMasterID IN (SELECT FPC.FPMasterID FROM FinishingProcessChild FPC WHERE FPC.IsPreProcess = 1)
                 AND FPM.FPMasterID IN (SELECT FPC.FPMasterID FROM FinishingProcessChild FPC WHERE FPC.IsPreProcess = 0)
                 GROUP BY FPM.FPMasterID, FPM.ConceptID, NFPP.NeedPreFinishingProcess
                ),
                FPostDoneList AS
                (
                 SELECT FPM.FPMasterID, FPM.ConceptID, NFPP.NeedPreFinishingProcess
                 FROM FinishingProcessMaster FPM
                 INNER JOIN NeedFPostList NFPP ON NFPP.ConceptID = FPM.ConceptID
                 LEFT JOIN FinishingProcessChild FPC ON FPC.FPMasterID = FPM.FPMasterID
                 WHERE FPM.FPMasterID IN (SELECT FPC.FPMasterID FROM FinishingProcessChild FPC WHERE FPC.IsPreProcess = 0)
                 GROUP BY FPM.FPMasterID, FPM.ConceptID, NFPP.NeedPreFinishingProcess
                ),
                R AS (
                 SELECT FPMasterID, ConceptID, NeedPreFinishingProcess FROM FPreDoneList
                 UNION
                 SELECT FPMasterID, ConceptID, NeedPreFinishingProcess FROM FPostDoneList
                ),

                FinalList AS
                (
                 SELECT FPM.FPMasterID, FPM.ConceptID, R.NeedPreFinishingProcess, FPM.BookingID, FPM.TrialNo, FPM.TrialDate, CM.ConceptNo, CM.ConceptDate,FPM.PFBatchNo,FPM.PFBatchDate,FPM.BatchQty,
                    MSC.SubClassName, Technical.TechnicalName, Composition.SegmentValue Composition, Gsm.SegmentValue Gsm, IG.SubGroupName, FU.PartName FUPartName,
                    CM.[Length], CM.Width, CM.IsBDS, CM.GroupConceptNo,
                    ColorName = Case When CM.ISBDS = 0 Then '' Else FCC.ColorName End
                 FROM FinishingProcessMaster FPM 
                 INNER JOIN FreeConceptMaster CM ON CM.ConceptID = FPM.ConceptID
                 INNER JOIN KnittingPlanMaster KPM ON FPM.ConceptID=KPM.ConceptID
                 LEFT JOIN FreeConceptChildColor FCC ON FCC.ConceptID = CM.ConceptID
                 INNER JOIN R ON FPM.ConceptID = R.ConceptID
                 LEFT JOIN KnittingMachineSubClass MSC ON MSC.SubClassID = CM.MCSubClassID
                 LEFT JOIN FabricTechnicalName Technical ON Technical.TechnicalNameId=CM.TechnicalNameId
                 LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = CM.CompositionID
                 LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = CM.GSMID
                 INNER JOIN {DbNames.EPYSL}..ItemSubGroup IG ON IG.SubGroupID = CM.SubGroupID
                 LEFT JOIN {DbNames.EPYSL}..FabricUsedPart FU ON FU.FUPartID = CM.FUPartID

                 GROUP BY  FPM.FPMasterID, FPM.ConceptID, R.NeedPreFinishingProcess, FPM.BookingID, FPM.TrialNo, FPM.TrialDate, CM.ConceptNo, CM.ConceptDate,FPM.PFBatchNo,FPM.PFBatchDate,FPM.BatchQty,
                    MSC.SubClassName, Technical.TechnicalName, Composition.SegmentValue, Gsm.SegmentValue, IG.SubGroupName, FU.PartName,
                    CM.[Length], CM.Width, CM.IsBDS, CM.GroupConceptNo,
                    Case When CM.ISBDS = 0 Then '' Else FCC.ColorName End
                )
                SELECT FinalList.*, Count(*) Over() TotalRows
                FROM FinalList";
                orderBy = " ORDER BY FinalList.ConceptID DESC";
            }

            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<FinishingProcessMaster>(sql);
        }

        public async Task<FinishingProcessMaster> GetNewAsync(int conceptId, int isBDS, string grpConceptNo)
        {
            string colString = isBDS == 1 ? $@"={conceptId}" : $@" In  (Select ConceptID From FreeConceptMaster Where GroupConceptNo = '{grpConceptNo}')";
            var query =
                $@"
                -- Item Segments
                SELECT CM.ConceptID, CM.ConceptNo, CM.ConceptDate, KP.NeedPreFinishingProcess,
                CM.TechnicalNameId,FN.TechnicalName,CM.KnittingTypeID, KT.TypeName KnittingTypeName, 
                CM.Remarks,Gsm.SegmentValue Gsm,CM.CompositionID, 
                Composition.SegmentValue Composition
                FROM KnittingPlanMaster KP
                INNER JOIN FreeConceptMaster CM ON CM.ConceptID = KP.ConceptID 
                LEFT JOIN KnittingMachineType KT ON KT.TypeID = CM.KnittingTypeID 
				INNER JOIN FabricTechnicalName FN ON FN.TechnicalNameId = CM.TechnicalNameId
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = CM.GSMID
                LEFT JOIN FinishingProcessMaster FPM ON FPM.ConceptID = KP.ConceptID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = CM.CompositionID
                WHERE CM.ConceptID = {conceptId}
                GROUP BY CM.ConceptID, CM.ConceptNo, CM.ConceptDate, KP.NeedPreFinishingProcess,
                CM.TechnicalNameId,FN.TechnicalName,CM.KnittingTypeID, KT.TypeName,
                CM.Remarks, Gsm.SegmentValue,CM.CompositionID,Composition.SegmentValue;

                ----Pre-Process
                ;SELECT FMC.FMProcessID ProcessID, FMC.ProcessName ProcessName, FMC.ProcessTypeID, ET.ValueName ProcessType,FMC.FMCMasterID, b.ProcessName MachineName
                From FinishingMachineProcess_HK FMC
                Inner Join FinishingMachineConfigurationMaster b on b.FMCMasterID = FMC.FMCMasterID
                Inner join {DbNames.EPYSL}..EntityTypeValue ET on ET.ValueID = FMC.ProcessTypeID
                WHERE FMC.ProcessName != '' AND ET.ValueName IN ('Pre/Post Set', 'Pre Set');

                ----Post-Process
                ;SELECT FMC.FMProcessID ProcessID, FMC.ProcessName ProcessName, FMC.ProcessTypeID, ET.ValueName ProcessType,FMC.FMCMasterID, b.ProcessName MachineName
                From FinishingMachineProcess_HK FMC
                Inner Join FinishingMachineConfigurationMaster b on b.FMCMasterID = FMC.FMCMasterID
                Inner join {DbNames.EPYSL}..EntityTypeValue ET on ET.ValueID = FMC.ProcessTypeID
                WHERE FMC.ProcessName != '' AND ET.ValueName IN ('Pre/Post Set', 'Post Set');

                ----FinishingMachineConfigurationChild
                ;SELECT *
                From FinishingMachineConfigurationChild FMC;

                ----Child Color
                ;SELECT ColorID, ColorName, ColorCode
                FROM FreeConceptChildColor FC
                Where ConceptID {colString}
                GROUP BY ColorID, ColorName, ColorCode; ";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                FinishingProcessMaster data = records.Read<FinishingProcessMaster>().FirstOrDefault();
                data.PreProcessList = records.Read<FinishingProcessChild>().ToList();
                data.PostProcessList = records.Read<FinishingProcessChild>().ToList();
                data.FinishingMachineConfigurationChildList = records.Read<FinishingMachineConfigurationChild>().ToList();
                data.ColorList = records.Read<FinishingProcessChild>().ToList();
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

        public async Task<FinishingProcessMaster> GetMachineParam(int fmsId)
        {
            var query = $@"

                -- Process Machine param List

                ;WITH FMS AS( SELECT FMSID,FMCMasterID,REPLACE(REPLACE(ParamName, 'Param', ''),'Value','') AS SerialNo, MachineNo, BrandID, UnitID, Capacity,ParamName, ParamValue
                FROM (SELECT * FROM FinishingMachineSetup
                WHERE FMSID = {fmsId}
                ) p
                UNPIVOT
                (ParamValue FOR ParamName IN (Param1Value, Param2Value, Param3Value, Param4Value, Param5Value, Param6Value, Param7Value, Param8Value, Param9Value, Param10Value, Param11Value, Param12Value, Param13Value, Param14Value, Param15Value, Param16Value, Param17Value, Param18Value, Param19Value, Param20Value)
                )AS unpvt
				)
				,M AS(
				SELECT FMS.FMSID,FMS.FMCMasterID,FMS.SerialNo, FMS.MachineNo, FMS.BrandID, FMS.UnitID,FMS.Capacity,FMS.ParamName, FMS.ParamValue, FMCC.DefaultValue,
                FMCC.ParamName AS ParamDispalyName, ET.ValueName ProcessType,FMCC.NeedItem
                FROM FinishingMachineConfigurationChild FMCC
				INNER JOIN FMS ON FMS.FMCMasterID=FMCC.FMCMasterID AND FMS.SerialNo=FMCC.Sequence
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ET ON ET.ValueID = FMCC.ProcessTypeID
				)
				SELECT * FROM  M
                ";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                FinishingProcessMaster data = new FinishingProcessMaster();
                data.ProcessMachineList = records.Read<FinishingProcessChild>().ToList();
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

        public async Task<FinishingProcessMaster> GetAsync(int id, int conceptId, int isBDS, string grpConceptNo)
        {
            string colString = isBDS == 1 ? $@"={conceptId}" : $@" In  (Select ConceptID From FreeConceptMaster Where GroupConceptNo = '{grpConceptNo}')";
            var query =
                $@"
                ;SELECT FP.FPMasterID, FP.ConceptID, FP.BookingID, FP.TrialNo, FP.TrialDate,
                CM.ConceptNo, CM.ConceptDate, KP.NeedPreFinishingProcess,FP.PFBatchNo,
                FP.PFBatchDate,FP.BatchQty,FN.TechnicalName,CM.KnittingTypeID, KT.TypeName KnittingTypeName, 
                CM.Remarks,Gsm.SegmentValue Gsm,CM.CompositionID, Composition.SegmentValue Composition
                FROM FinishingProcessMaster FP
                LEFT JOIN KnittingPlanMaster KP ON KP.ConceptID = FP.ConceptID
                LEFT JOIN FreeConceptMaster CM ON CM.ConceptID = FP.ConceptID 
                Left JOIN KnittingMachineType KT ON KT.TypeID = CM.KnittingTypeID 
				INNER JOIN FabricTechnicalName FN ON FN.TechnicalNameId = CM.TechnicalNameId
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = CM.GSMID
                LEFT JOIN FinishingProcessMaster FPM ON FPM.ConceptID = KP.ConceptID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = CM.CompositionID
                WHERE FP.FPMasterID = {id};

                ----Child Pre-Process
                ;SELECT FPC.FPChildID,FPC.ColorID, FPC.FPMasterID, FPC.ProcessID, FPC.SeqNo, FMP.ProcessName, C.ShortName UnitName,b.ValueName BrandName, FPC.ProcessTypeID, ET.ValueName ProcessType, FPC.IsPreProcess, FPC.FMSID, FMC.FMCMasterID, FMS.MachineNo, MachineName=FMC.ProcessName , FPC.Remarks, FPC.Param1Value, FPC.Param2Value, FPC.Param3Value, FPC.Param4Value, FPC.Param5Value, FPC.Param6Value, FPC.Param7Value, FPC.Param8Value, FPC.Param9Value, FPC.Param10Value, FPC.Param11Value, FPC.Param12Value, FPC.Param13Value, FPC.Param14Value, FPC.Param15Value, FPC.Param16Value, FPC.Param17Value, FPC.Param18Value, FPC.Param19Value, FPC.Param20Value
                FROM FinishingProcessChild FPC
                INNER JOIN FinishingMachineProcess_HK FMP On FMP.FMProcessID = FPC.ProcessID
                INNER JOIN FinishingMachineConfigurationMaster FMC ON FMC.FMCMasterID = FMP.FMCMasterID
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ET ON ET.ValueID = FPC.ProcessTypeID
                LEFT JOIN FinishingMachineSetup FMS On FMS.FMSID = FPC.FMSID
                Left Join {DbNames.EPYSL}..EntityTypeValue b on b.ValueID = FMS.BrandID
				Left Join KnittingUnit c on c.KnittingUnitID = FMS.UnitID
                WHERE FPC.FPMasterID = {id} AND FPC.IsPreProcess = 1 ORDER BY FPC.SeqNo ASC;

                ----Child Post-Process
                ;SELECT FPC.FPChildID,FPC.ColorID, FPC.FPMasterID, FPC.ProcessID, FPC.ColorID, FPC.SeqNo, FMP.ProcessName, C.ShortName UnitName,b.ValueName BrandName, FPC.ProcessTypeID, ET.ValueName ProcessType, FPC.IsPreProcess, FPC.FMSID,FMC.FMCMasterID, FMS.MachineNo, MachineName=FMC.ProcessName, FPC.Remarks, FPC.Param1Value, FPC.Param2Value, FPC.Param3Value, FPC.Param4Value, FPC.Param5Value, FPC.Param6Value, FPC.Param7Value, FPC.Param8Value, FPC.Param9Value, FPC.Param10Value, FPC.Param11Value, FPC.Param12Value, FPC.Param13Value, FPC.Param14Value, FPC.Param15Value, FPC.Param16Value, FPC.Param17Value, FPC.Param18Value, FPC.Param19Value, FPC.Param20Value
                FROM FinishingProcessChild FPC
                INNER JOIN FinishingMachineProcess_HK FMP On FMP.FMProcessID = FPC.ProcessID
                INNER JOIN FinishingMachineConfigurationMaster FMC ON FMC.FMCMasterID = FMP.FMCMasterID
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ET ON ET.ValueID = FPC.ProcessTypeID
                LEFT JOIN FinishingMachineSetup FMS On FMS.FMSID = FPC.FMSID
                Left Join {DbNames.EPYSL}..EntityTypeValue b on b.ValueID = FMS.BrandID
				Left Join KnittingUnit c on c.KnittingUnitID = FMS.UnitID
                WHERE FPC.FPMasterID = {id} AND FPC.IsPreProcess = 0 ORDER BY FPC.SeqNo ASC;

                ----Post Process Colors
                SELECT FPC.ColorID, ISV.SegmentValue ColorName
				FROM FinishingProcessChild FPC
				INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = FPC.ColorID
				WHERE FPC.FPMasterID = {id} AND FPC.IsPreProcess = 0
				GROUP BY FPC.ColorID, ISV.SegmentValue

                ----Pre-Process
                ;SELECT FMC.FMProcessID ProcessID, FMC.ProcessName ProcessName, FMC.ProcessTypeID, ET.ValueName ProcessType,FMC.FMCMasterID, b.ProcessName MachineName
                From FinishingMachineProcess_HK FMC
                Inner Join FinishingMachineConfigurationMaster b on b.FMCMasterID = FMC.FMCMasterID
                Inner join {DbNames.EPYSL}..EntityTypeValue ET on ET.ValueID = FMC.ProcessTypeID
                WHERE FMC.ProcessName != '' AND ET.ValueName IN ('Pre/Post Set', 'Pre Set');

                ----Post-Process
                ;SELECT FMC.FMProcessID ProcessID, FMC.ProcessName ProcessName, FMC.ProcessTypeID, ET.ValueName ProcessType,FMC.FMCMasterID, b.ProcessName MachineName
                From FinishingMachineProcess_HK FMC
                Inner Join FinishingMachineConfigurationMaster b on b.FMCMasterID = FMC.FMCMasterID
                Inner join {DbNames.EPYSL}..EntityTypeValue ET on ET.ValueID = FMC.ProcessTypeID
                WHERE FMC.ProcessName != '' AND ET.ValueName IN ('Pre/Post Set', 'Post Set')

                ----FinishingMachineConfigurationChild
                ;SELECT *
                From FinishingMachineConfigurationChild FMC;

                ----Child Items
                select FCI.FPChildItemID, FCI.FPChildID, FCI.FPMasterID, FCI.SegmentNo, FCI.ItemMasterID, FCI.Qty, FCI.IsPreProcess, IM.ItemName text
	            From FinishingProcessChildItem FCI
	            Inner Join {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = FCI.ItemMasterID
	            WHERE FPMasterID = {id}

                ----Child Color
                ;SELECT ColorID, ColorName, ColorCode
                FROM FreeConceptChildColor FC
                Where ConceptID {colString}
                GROUP BY ColorID, ColorName, ColorCode; ";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                FinishingProcessMaster data = records.Read<FinishingProcessMaster>().FirstOrDefault();
                Guard.Against.NullObject(id, data);
                data.PreFinishingProcessChilds = records.Read<FinishingProcessChild>().ToList();
                data.PostFinishingProcessChilds = records.Read<FinishingProcessChild>().ToList();
                data.PostFinishingProcessChildColors = records.Read<FinishingProcessChild>().ToList();
                data.PreProcessList = records.Read<FinishingProcessChild>().ToList();
                data.PostProcessList = records.Read<FinishingProcessChild>().ToList();
                data.FinishingMachineConfigurationChildList = records.Read<FinishingMachineConfigurationChild>().ToList();
                data.PreFinishingProcessChildItems = records.Read<FinishingProcessChildItem>().ToList();
                data.ColorList = records.Read<FinishingProcessChild>().ToList();

                data.PreFinishingProcessChilds.ForEach(child =>
                {
                    child.PreFinishingProcessChildItems = data.PreFinishingProcessChildItems.Where(x => x.FPChildID == child.FPChildID && x.IsPreProcess == true).ToList();
                });
                data.PostFinishingProcessChilds.ForEach(child =>
                {
                    child.ItemIDs = string.Join(",", data.PreFinishingProcessChildItems.Where(x => x.FPChildID == child.FPChildID).Select(y => y.ItemMasterID));
                    child.PreFinishingProcessChildItems = data.PreFinishingProcessChildItems.Where(x => x.FPChildID == child.FPChildID && x.IsPreProcess == false).ToList();
                });

                data.PostFinishingProcessChildColors.ForEach(col =>
                {
                    col.PostFinishingProcessChilds = data.PostFinishingProcessChilds.Where(x => x.ColorID == col.ColorID).OrderBy(y=>y.SeqNo).ToList();
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

        public async Task<List<Select2OptionModel>> GetChamicalItem(string particularName, int fpChildId)
        {
            string sql;
            if (fpChildId > 0)
            {
                sql = $@"
                   With
                    SG As(
	                    Select SubGroupID
	                    From {DbNames.EPYSL}..ItemSubGroup ISG
	                    Inner Join {DbNames.EPYSL}..EntityTypeValue EV On ISG.SubGroupName Like '%' + EV.ValueName + '%'
	                    Where EV.ValueName = {particularName}
	                    Group By SubGroupID
                    ),
				    FPC AS (
					    SELECT FPChildItemID, FPChildID, ItemMasterID, Qty
					    FROM FinishingProcessChildItem
					    WHERE FPChildID = {fpChildId}
				    )
                    SELECT CAST(IM.ItemMasterID AS VARCHAR) id, ItemName text, CAST(FPC.Qty AS VARCHAR) [desc]
                    FROM {DbNames.EPYSL}..ItemMaster IM
                    Inner Join SG On SG.SubGroupID = IM.SubGroupID
				    LEFT JOIN FPC ON FPC.ItemMasterID = IM.ItemMasterID
                    ";
            }
            else
            {
                sql = $@"
                   With
                    SG As(
	                    Select SubGroupID
	                    From {DbNames.EPYSL}..ItemSubGroup ISG
	                    Inner Join {DbNames.EPYSL}..EntityTypeValue EV On ISG.SubGroupName Like '%' + EV.ValueName + '%'
	                    Where EV.ValueName = {particularName}
	                    Group By SubGroupID
                    )

                    SELECT CAST(ItemMasterID AS VARCHAR) id, ItemName text
                    FROM {DbNames.EPYSL}..ItemMaster IM
                    Inner Join SG On SG.SubGroupID = IM.SubGroupID
                    ";
            }

            return await _service.GetDataAsync<Select2OptionModel>(sql);
        }

        public async Task<FinishingProcessMaster> GetAllByIDAsync(int id)
        {
            string sql = $@"
            ;Select * From FinishingProcessMaster Where FPMasterID = {id}

            ;Select * From FinishingProcessChild Where FPMasterID = {id}

            ;Select * From FinishingProcessChildItem Where FPMasterID = {id}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                FinishingProcessMaster data = records.Read<FinishingProcessMaster>().FirstOrDefault();
                Guard.Against.NullObject(data);
                data.FinishingProcessChilds = records.Read<FinishingProcessChild>().ToList();
                data.PreFinishingProcessChildItems = records.Read<FinishingProcessChildItem>().ToList();
                foreach (FinishingProcessChild item in data.FinishingProcessChilds.Where(x => x.IsPreProcess == true).ToList())
                {
                    item.PreFinishingProcessChildItems = data.PreFinishingProcessChildItems.Where(x => x.FPChildID == item.FPChildID && x.IsPreProcess == true).ToList();
                }
                foreach (FinishingProcessChild item in data.FinishingProcessChilds.Where(x => x.IsPreProcess == false).ToList())
                {
                    item.PreFinishingProcessChildItems = data.PreFinishingProcessChildItems.Where(x => x.FPChildID == item.FPChildID && x.IsPreProcess == false).ToList();
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

        public async Task SaveAsync(FinishingProcessMaster entity)
        {
            SqlTransaction transaction = null;
            SqlTransaction transactionGmt = null;
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                await _gmtConnection.OpenAsync();
                transactionGmt = _gmtConnection.BeginTransaction();


                switch (entity.EntityState)
                {
                    case EntityState.Added:
                        entity =  await AddAsync(entity, transaction, _connection, transactionGmt, _gmtConnection);
                        break;

                    case EntityState.Modified:
                        entity = await UpdateAsync(entity, transaction, _connection, transactionGmt, _gmtConnection);
                        break;

                    default:
                        break;
                }

                #region Delete FinishingProcessChilds
                List<FinishingProcessChild> finishingProcessChilds = entity.FinishingProcessChilds.Where(x => x.EntityState == EntityState.Deleted).ToList();
                List<FinishingProcessChildItem> preFinishingProcessChildItems = new List<FinishingProcessChildItem>();

                finishingProcessChilds.ForEach(y =>
                {
                    List<FinishingProcessChildItem> list = entity.PreFinishingProcessChildItems.Where(x => x.FPChildID == y.FPChildID).ToList();
                    list.ForEach(z => z.EntityState = EntityState.Deleted);
                    preFinishingProcessChildItems.AddRange(list);
                });

                if (preFinishingProcessChildItems.Count() > 0)
                {
                    await _service.SaveAsync(preFinishingProcessChildItems, transaction);
                }
                if (finishingProcessChilds.Count() > 0)
                {
                    await _service.SaveAsync(finishingProcessChilds, transaction);
                }
                #endregion Delete FinishingProcessChilds

                finishingProcessChilds = entity.FinishingProcessChilds.Where(x => x.EntityState != EntityState.Deleted).ToList();
                preFinishingProcessChildItems = entity.PreFinishingProcessChildItems.Where(x => x.EntityState != EntityState.Deleted).ToList();

                await _service.SaveSingleAsync(entity, transaction);
                await _service.SaveAsync(finishingProcessChilds, transaction);
                await _service.SaveAsync(preFinishingProcessChildItems, transaction);

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
                _gmtConnection.Close();
            }
        }

        private async Task<FinishingProcessMaster> AddAsync(FinishingProcessMaster entity, SqlTransaction transaction, SqlConnection _connection, SqlTransaction transactionGmt, SqlConnection _gmtConnection)
        {
            entity.FPMasterID = _service.GetMaxId(TableNames.FINISHING_PROCESS_MASTER,0);
            entity.PFBatchNo = await _service.GetMaxNoAsync(TableNames.FP_BATCH_NO, 1, RepeatAfterEnum.NoRepeat, "00000", transactionGmt, _gmtConnection);
            var maxYRChildId = await _service.GetMaxIdAsync(TableNames.FINISHING_PROCESS_CHILD, entity.FinishingProcessChilds.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _gmtConnection);
            var maxChildItemId = await _service.GetMaxIdAsync(TableNames.FINISHING_PROCESS_CHILD_ITEM, entity.FinishingProcessChilds.Sum(x => x.PreFinishingProcessChildItems.Count), RepeatAfterEnum.NoRepeat, transactionGmt, _gmtConnection);
            foreach (FinishingProcessChild item in entity.FinishingProcessChilds)
            {
                item.FPChildID = maxYRChildId++;
                item.FPMasterID = entity.FPMasterID;
                foreach (FinishingProcessChildItem cItem in item.PreFinishingProcessChildItems)
                {
                    cItem.FPChildItemID = maxChildItemId++;
                    cItem.FPChildID = item.FPChildID;
                    cItem.FPMasterID = item.FPMasterID;
                    cItem.SegmentNo = item.SeqNo;
                    cItem.IsPreProcess = item.IsPreProcess;
                    entity.PreFinishingProcessChildItems.Add(cItem);
                }
            }
            return entity;
        }

        private async Task<FinishingProcessMaster> UpdateAsync(FinishingProcessMaster entity, SqlTransaction transaction, SqlConnection _connection, SqlTransaction transactionGmt, SqlConnection _gmtConnection)
        {
            var maxYRChildId = await _service.GetMaxIdAsync(TableNames.FINISHING_PROCESS_CHILD, entity.FinishingProcessChilds.Where(x => x.EntityState == EntityState.Added).Count(), RepeatAfterEnum.NoRepeat, transactionGmt, _gmtConnection);
            var maxChildItemId = await _service.GetMaxIdAsync(TableNames.FINISHING_PROCESS_CHILD_ITEM, entity.FinishingProcessChilds.Sum(x => x.PreFinishingProcessChildItems.Where(c => c.EntityState == EntityState.Added).Count()), RepeatAfterEnum.NoRepeat, transactionGmt, _gmtConnection);

            foreach (FinishingProcessChild item in entity.FinishingProcessChilds.ToList())
            {
                switch (item.EntityState)
                {
                    case EntityState.Added:
                        item.FPChildID = maxYRChildId++;
                        item.FPMasterID = entity.FPMasterID;
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
                List<FinishingProcessChildItem> ChildItems = new List<FinishingProcessChildItem>();
                foreach (FinishingProcessChildItem cItem in item.PreFinishingProcessChildItems)
                {
                    switch (cItem.EntityState)
                    {
                        case EntityState.Added:
                            cItem.FPChildItemID = maxChildItemId++;
                            cItem.FPChildID = item.FPChildID;
                            cItem.FPMasterID = item.FPMasterID;
                            cItem.SegmentNo = item.SeqNo;
                            cItem.IsPreProcess = item.IsPreProcess;
                            cItem.EntityState = EntityState.Added;
                            break;

                        case EntityState.Deleted:
                        case EntityState.Unchanged:
                            cItem.EntityState = EntityState.Deleted;
                            break;

                        case EntityState.Modified:
                            cItem.EntityState = EntityState.Modified;
                            break;

                        default:
                            break;
                    }
                    entity.PreFinishingProcessChildItems.Add(cItem);
                }
            }
            return entity;
        }

        public async Task<List<FinishingProcessChildItem>> GetFinishingProcessChildItems(string particularName, int fpChildId)
        {
            string sql = $@"SELECT FPCI.*,IM.ItemName FROM FinishingProcessChildItem FPCI
                            LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID=FPCI.ItemMasterID
                            WHERE FPCI.FPChildID = {fpChildId}";
            return await _service.GetDataAsync<FinishingProcessChildItem>(sql);
        }
        public async Task<List<FinishingProcessChild>> GetFinishingMachineProcess(PaginationInfo paginationInfo, string setName)
        {
            //setName = 'Pre Set' OR 'Post Set'

            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By ProcessName ASC" : paginationInfo.OrderBy;

            var query = $@"
                WITH
                F AS 
                (
                    SELECT FMC.FMProcessID ProcessID, FMC.ProcessName ProcessName, FMC.ProcessTypeID, ET.ValueName ProcessType,
                    FMC.FMCMasterID, b.ProcessName MachineName
                    From FinishingMachineProcess_HK FMC
                    Inner Join FinishingMachineConfigurationMaster b on b.FMCMasterID = FMC.FMCMasterID
                    Inner join {DbNames.EPYSL}..EntityTypeValue ET on ET.ValueID = FMC.ProcessTypeID
                    WHERE FMC.ProcessName != '' AND ET.ValueName IN ('Pre/Post Set', '{setName}')
                )

                Select *, COUNT(*) Over() TotalRows From F
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<FinishingProcessChild>(query);
        }
    }
}