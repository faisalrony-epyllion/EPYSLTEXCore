using Dapper;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Static;
using System.Data;
using System.Data.Entity;
using Microsoft.Data.SqlClient;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Statics;
using EPYSLTEXCore.Infrastructure.Entities;

namespace EPYSLTEX.Infrastructure.Services
{
    public class RollFinishingInfoService : IRollFinishingInfoService
    {
        private readonly IDapperCRUDService<DyeingBatchMaster> _service;
        
        private readonly SqlConnection _connection;
        private decimal _currentTime = 0;

        public RollFinishingInfoService(IDapperCRUDService<DyeingBatchMaster> service)
        {
            _service = service;
            _connection = service.Connection;
        }

        public async Task<List<DyeingBatchMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo)
        {
            string tempGuid = CommonFunction.GetNewGuid();

            string sql="";
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By F.DBatchID Desc" : paginationInfo.OrderBy;
            if (status == Status.Pending)
            {
                sql = $@"
                ;WITH 
                FPWithPreProcess AS
                (
	                SELECT FPM.FPMasterID
	                FROM FinishingProcessMaster FPM
	                INNER JOIN FinishingProcessChild FPC ON FPC.FPMasterID = FPM.FPMasterID
	                WHERE FPC.IsPreProcess = 1 AND FPM.PDProductionComplete = 0
	                GROUP BY FPM.FPMasterID
                ),
                FP AS (
	                SELECT FPM.ConceptID, FPC.ColorID
	                FROM FinishingProcessMaster FPM
	                INNER JOIN FinishingProcessChild FPC ON FPC.FPMasterID = FPM.FPMasterID
	                WHERE FPC.IsPreProcess = 0 AND FPM.FPMasterID NOT IN (SELECT FWP.FPMasterID FROM FPWithPreProcess FWP)
	                GROUP BY FPM.ConceptID, FPC.ColorID
                ), 
                BM As(
					SELECT BM.*, DBI.ItemMasterID, DBI.ConceptID, DBI.DBIID
					FROM DyeingBatchMaster BM
					INNER JOIN DyeingBatchItem DBI ON DBI.DBatchID = BM.DBatchID
                    INNER JOIN FP ON FP.ConceptID = DBI.ConceptID And FP.ColorID = BM.ColorID
					Where DBI.PostProductionComplete = 0
                    AND BM.ProductionDate IS NOT NULL And BM.BatchStartTime IS NOT NULL And BM.BatchEndTime IS NOT NULL
                ),
                F AS (
	                SELECT BM.DBatchID, BM.DBatchNo, BM.DBatchDate, BM.RecipeID, BM.BatchWeightKG, BM.ProductionDate,
                    BM.BatchStartTime, BM.BatchEndTime, FCM.ConceptNo, FCM.GroupConceptNo, ISG.SubGroupName, KT.TypeName KnittingType,
                    FTN.TechnicalName, ISV.SegmentValue Composition, GSV.SegmentValue Gsm, Color.SegmentValue ColorName,
                    BM.DBIID, FCM.Length, FCM.Width
                    FROM BM
                    INNER JOIN FreeConceptMaster FCM ON FCM.ConceptID = BM.ConceptID
                    LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID=FCM.SubGroupID
                    LEFT JOIN KnittingMachineType KT ON KT.TypeID=FCM.KnittingTypeID
                    LEFT JOIN FabricTechnicalName FTN ON FTN.TechnicalNameId=FCM.TechnicalNameId
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = FCM.CompositionID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue GSV ON GSV.SegmentValueID = FCM.GSMID
				    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = BM.ColorID
                )
                SELECT F.* INTO #TempTable{tempGuid} FROM F
                SELECT *, Count(*) Over() TotalRows FROM #TempTable{tempGuid} F ";
            }
            else
            {
                sql = $@"
                ;WITH FP AS (
	                SELECT FPM.ConceptID, FPC.ColorID
	                FROM FinishingProcessMaster FPM
	                INNER JOIN FinishingProcessChild FPC ON FPC.FPMasterID = FPM.FPMasterID
	                WHERE FPC.IsPreProcess = 0
	                GROUP BY FPM.ConceptID, FPC.ColorID
                ), BM As(
					SELECT BM.*, DBI.ItemMasterID, DBI.ConceptID, DBI.DBIID
					FROM DyeingBatchMaster BM
					INNER JOIN DyeingBatchItem DBI ON DBI.DBatchID = BM.DBatchID
                    INNER JOIN FP ON FP.ConceptID = DBI.ConceptID And FP.ColorID = BM.ColorID
					Where DBI.PostProductionComplete = 1
				),
                F AS (
	                SELECT BM.DBatchID, BM.DBatchNo, BM.DBatchDate, BM.RecipeID, BM.BatchWeightKG, BM.ProductionDate,
                    BM.BatchStartTime, BM.BatchEndTime,FCM.ConceptNo, FCM.GroupConceptNo, ISG.SubGroupName, KT.TypeName KnittingType,
                    FTN.TechnicalName, ISV.SegmentValue Composition, GSV.SegmentValue Gsm, Color.SegmentValue ColorName,
                    BM.DBIID, FCM.Length, FCM.Width
                    FROM BM
                    INNER JOIN FreeConceptMaster FCM ON FCM.ConceptID = BM.ConceptID
                    LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID=FCM.SubGroupID
                    LEFT JOIN KnittingMachineType KT ON KT.TypeID=FCM.KnittingTypeID
                    LEFT JOIN FabricTechnicalName FTN ON FTN.TechnicalNameId=FCM.TechnicalNameId
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = FCM.CompositionID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue GSV ON GSV.SegmentValueID = FCM.GSMID
				    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = BM.ColorID
                )
                SELECT *, Count(*) Over() TotalRows FROM F";
            }

            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            if (status == Status.Pending)
            {
                sql += $@" DROP TABLE #TempTable{tempGuid} ";
            }

            return await _service.GetDataAsync<DyeingBatchMaster>(sql);
        }

        public async Task<DyeingBatchMaster> GetAsync(int dbiID,Status status)
        {
            var sql = $@"
                SELECT BM.DBatchID, BM.DBatchNo, BM.DBatchDate, BM.RecipeID, BM.BatchWeightKG, BM.ProductionDate,
				BM.BatchStartTime, BM.BatchEndTime, FCM.ConceptNo, ISG.SubGroupName, KT.TypeName KnittingType,
                FTN.TechnicalName, ISV.SegmentValue Composition, GSV.SegmentValue Gsm, BM.CCColorID,
                Color.SegmentValue ColorName, FCM.Length, FCM.Width, DBI.DBIID, BM.ColorID, DBI.ConceptID
				FROM DyeingBatchMaster BM
				Inner Join DyeingBatchItem DBI On DBI.DBatchID = BM.DBatchID
				INNER JOIN FreeConceptChildColor FCC ON FCC.CCColorID = BM.CCColorID
				INNER JOIN FreeConceptMaster FCM ON FCM.ConceptID = DBI.ConceptID
                LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID=FCM.SubGroupID
				LEFT JOIN KnittingMachineType KT ON KT.TypeID=FCM.KnittingTypeID
				LEFT JOIN FabricTechnicalName FTN ON FTN.TechnicalNameId=FCM.TechnicalNameId
				LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID = FCM.CompositionID
				LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue GSV ON GSV.SegmentValueID = FCM.GSMID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = BM.ColorID
				WHERE DBI.DBIID = {dbiID}
				GROUP BY BM.DBatchID, BM.DBatchNo, BM.DBatchDate, BM.RecipeID, BM.BatchWeightKG, BM.ProductionDate,
				BM.BatchStartTime, BM.BatchEndTime, FCM.ConceptNo, ISG.SubGroupName, KT.TypeName,
                FTN.TechnicalName, ISV.SegmentValue, GSV.SegmentValue, BM.CCColorID,
                Color.SegmentValue, FCM.Length, FCM.Width, DBI.DBIID, BM.ColorID, DBI.ConceptID;

                ----Finishing Process
                ;SELECT FPC.DBCFPID, FPC.DBatchID, FPC.ProcessID, FPC.SeqNo, FMP.ProcessName, C.ShortName UnitName,b.ValueName BrandName,
                FPC.ProcessTypeID, ET.ValueName ProcessType, FPC.FMSID, FMC.FMCMasterID, FMS.MachineNo, MachineName=FMC.ProcessName ,
                FPC.Remarks, FPC.Param1Value, FPC.Param2Value, FPC.Param3Value, FPC.Param4Value, FPC.Param5Value, FPC.Param6Value, FPC.Param7Value,
                FPC.Param8Value, FPC.Param9Value, FPC.Param10Value, FPC.Param11Value, FPC.Param12Value, FPC.Param13Value, FPC.Param14Value, FPC.Param15Value,
                FPC.Param16Value, FPC.Param17Value, FPC.Param18Value, FPC.Param19Value, FPC.Param20Value,
                FPC.ProductionDate, FPC.ShiftID, FPC.OperatorID, FPC.PFMSID, PFMS.MachineNo PMachineNo, FPC.PParam1Value, FPC.PParam2Value, FPC.PParam3Value, FPC.PParam4Value,
                FPC.PParam5Value, FPC.PParam6Value, FPC.PParam7Value, FPC.PParam8Value, FPC.PParam9Value, FPC.PParam10Value, FPC.PParam11Value,
                FPC.PParam12Value, FPC.PParam13Value, FPC.PParam14Value, FPC.PParam15Value, FPC.PParam16Value,
                FPC.PParam17Value, FPC.PParam18Value, FPC.PParam19Value, FPC.PParam20Value, DBI.DBIID
                FROM DyeingBatchItem DBI
				Inner Join DyeingBatchChildFinishingProcess FPC ON FPC.DBIID = DBI.DBIID
                INNER JOIN FinishingMachineProcess_HK FMP On FMP.FMProcessID = FPC.ProcessID
                INNER JOIN FinishingMachineConfigurationMaster FMC ON FMC.FMCMasterID = FMP.FMCMasterID
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ET ON ET.ValueID = FPC.ProcessTypeID
                LEFT JOIN FinishingMachineSetup FMS On FMS.FMSID = FPC.FMSID
                LEFT JOIN FinishingMachineSetup PFMS On PFMS.FMSID = FPC.PFMSID
                Left Join {DbNames.EPYSL}..EntityTypeValue b on b.ValueID = FMS.BrandID
                Left Join KnittingUnit c on c.KnittingUnitID = FMS.UnitID
                WHERE DBI.DBIID = {dbiID} ORDER BY FPC.SeqNo ASC;

                ----Knitting Prod. /Roll Info
                ;SELECT GRollID, ConceptID, RollNo, RollLength, RollQty
                FROM KnittingProduction WHERE ConceptID IN (SELECT ConceptID FROM DyeingBatchItem WHERE DBIID = {dbiID});

                --Unit
                ;select cast(KnittingUnitID as varchar) id,UnitName [text]
                from KnittingUnit WHERE IsKnitting = 0 order by UnitName;

                --Finishing Machine Configuration
                SELECT * From FinishingMachineConfigurationChild FMC;

                --Shifts
                ;SELECT CAST(ShiftId AS VARCHAR) id, ShortName text, CAST(FromHour AS nvarchar(6)) [desc],CAST(ToHour AS nvarchar(6)) additionalValue
                FROM {DbNames.EPYSL}..ShiftInfo
                Where CompanyID=6
                order by SeqNo;

                --Operators
                ;select E.EmployeeCode id, E.DisplayEmployeeCode +' '+EmployeeName text
                from {DbNames.EPYSL}..Employee E
                INNER JOIN {DbNames.EPYSL}..EmployeeDesignation D ON D.DesigID=E.DesigID
				INNER JOIN {DbNames.EPYSL}..EmployeeDepartment ED ON ED.DepertmentID= E.DepertmentID
                where D.Designation like '%operator%' and ED.DepertmentDescription like '%Knitting%'; ";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                DyeingBatchMaster data = records.Read<DyeingBatchMaster>().FirstOrDefault();
                data.DyeingBatchChildFinishingProcesses = records.Read<DyeingBatchChildFinishingProcess>().ToList();
                data.KnittingProductions = records.Read<KnittingProduction>().ToList();
                data.UnitList = records.Read<Select2OptionModel>().ToList();
                data.FinishingMachineConfigurationChildList = records.Read<FinishingMachineConfigurationChild>().ToList();
                data.ShiftList = records.Read<Select2OptionModel>().ToList();
                data.OperatorList = records.Read<Select2OptionModel>().ToList();


                int shiftId = 0;
                if (status == Status.Pending)
                {
                    _currentTime = Convert.ToDecimal($"{DateTime.Now.Hour}.{DateTime.Now.Minute}");
                    shiftId = Convert.ToInt32(data.ShiftList.FirstOrDefault(x => IsInTimeSlot(x)).id);
                }

                if (shiftId > 0)
                {
                    data.DyeingBatchChildFinishingProcesses.ForEach(x =>
                    {
                        if (x.ShiftID == 0) x.ShiftID = shiftId;
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
                if (_connection.State == System.Data.ConnectionState.Open) _connection.Close();
            }
        }

        public async Task<FinishingProcessMaster> GetMachineParam(int fmsId, int childId)
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
                ), FP AS(
                SELECT FMSID,REPLACE(REPLACE(ParamName, 'Param', ''),'Value','') AS SerialNo, ParamName, ParamValue
                FROM (Select * From DyeingBatchChildFinishingProcess
                Where FMSID = {fmsId} And DBCFPID = {childId} AND IsPreProcess=0
                ) p
                UNPIVOT
                (ParamValue FOR ParamName IN (Param1Value, Param2Value, Param3Value, Param4Value, Param5Value, Param6Value, Param7Value, Param8Value, Param9Value, Param10Value, Param11Value, Param12Value, Param13Value, Param14Value, Param15Value, Param16Value, Param17Value, Param18Value, Param19Value, Param20Value)
                )AS unpvt
                ), AFP AS(
                SELECT FMSID,REPLACE(REPLACE(ParamName, 'PParam', ''),'Value','') AS SerialNo, ParamName, ParamValue
                FROM (Select * From DyeingBatchChildFinishingProcess
                Where FMSID = {fmsId} And DBCFPID = {childId} AND IsPreProcess=0
                ) p
                UNPIVOT
                (ParamValue FOR ParamName IN (PParam1Value, PParam2Value, PParam3Value, PParam4Value, PParam5Value, PParam6Value, PParam7Value, PParam8Value, PParam9Value, PParam10Value, PParam11Value, PParam12Value, PParam13Value, PParam14Value, PParam15Value, PParam16Value, PParam17Value, PParam18Value, PParam19Value, PParam20Value)
                )AS unpvt
                )
                --Select * From FP
                ,M AS(
                SELECT ETV.ValueName, FMS.FMSID,FMS.FMCMasterID,FMS.SerialNo, FMS.MachineNo, FMS.BrandID, FMS.UnitID,FMS.Capacity,FMS.ParamName, FMS.ParamValue,FMCC.ParamName AS ParamDispalyName,FMCC.NeedItem, FP.ParamValue PlanParamValue, AFP.ParamValue ActulaPlanParamValue
                FROM FinishingMachineConfigurationChild FMCC
                INNER JOIN FMS ON FMS.FMCMasterID=FMCC.FMCMasterID AND FMS.SerialNo=FMCC.Sequence
                LEFT JOIN FP ON FP.FMSID=FMS.FMSID AND FP.SerialNo=FMS.SerialNo
                LEFT JOIN AFP ON AFP.FMSID=FMS.FMSID AND AFP.SerialNo=FMS.SerialNo
                Inner Join {DbNames.EPYSL}..EntityTypeValue ETV On ETV.ValueID = FMCC.ProcessTypeID
                Where ETV.ValueName in ('Pre/Post Set','Post Set')
                )
                SELECT * FROM M
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

        public async Task<DyeingBatchMaster> GetAllByIDAsync(int id)
        {
            string sql = $@"
            ;Select * From DyeingBatchMaster Where DBatchID = {id}

            ;Select * From DyeingBatchChildFinishingProcess Where DBatchID = {id} AND IsPreProcess=0";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                DyeingBatchMaster data = records.Read<DyeingBatchMaster>().FirstOrDefault();
                Guard.Against.NullObject(data);
                data.DyeingBatchChildFinishingProcesses = records.Read<DyeingBatchChildFinishingProcess>().ToList();

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

        public async Task<DyeingBatchItem> GetAllByBDIIDAsync(int id)
        {
            string sql = $@"
            ;Select * From DyeingBatchItem Where DBIID = {id}

            ;Select * From DyeingBatchChildFinishingProcess Where DBIID = {id} AND IsPreProcess=0";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                DyeingBatchItem data = records.Read<DyeingBatchItem>().FirstOrDefault();
                Guard.Against.NullObject(data);
                data.DyeingBatchChildFinishingProcesses = records.Read<DyeingBatchChildFinishingProcess>().ToList();

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

        public async Task<List<DyeingBatchChildFinishingProcess>> GetExistFinishingProcessList(int dbId)
        {
            string sql = $@"SELECT FPC.DBCFPID, FPC.DBatchID, FPC.ProcessID, FPC.SeqNo, FMP.ProcessName, C.ShortName UnitName,b.ValueName BrandName,
                FPC.ProcessTypeID, ET.ValueName ProcessType, FPC.FMSID, FMC.FMCMasterID, FMS.MachineNo, MachineName=FMC.ProcessName ,
                FPC.Remarks, FPC.Param1Value, FPC.Param2Value, FPC.Param3Value, FPC.Param4Value, FPC.Param5Value, FPC.Param6Value, FPC.Param7Value,
                FPC.Param8Value, FPC.Param9Value, FPC.Param10Value, FPC.Param11Value, FPC.Param12Value, FPC.Param13Value, FPC.Param14Value, FPC.Param15Value,
                FPC.Param16Value, FPC.Param17Value, FPC.Param18Value, FPC.Param19Value, FPC.Param20Value,
                FPC.ProductionDate, FPC.ShiftID, FPC.OperatorID, FPC.PFMSID, PFMS.MachineNo PMachineNo, FPC.PParam1Value, FPC.PParam2Value, FPC.PParam3Value, FPC.PParam4Value,
                FPC.PParam5Value, FPC.PParam6Value, FPC.PParam7Value, FPC.PParam8Value, FPC.PParam9Value, FPC.PParam10Value, FPC.PParam11Value,
                FPC.PParam12Value, FPC.PParam13Value, FPC.PParam14Value, FPC.PParam15Value, FPC.PParam16Value,
                FPC.PParam17Value, FPC.PParam18Value, FPC.PParam19Value, FPC.PParam20Value, DBI.DBIID, DBI.ConceptID, FPC.IsPreProcess
                FROM DyeingBatchItem DBI
				Inner Join DyeingBatchChildFinishingProcess FPC ON FPC.DBIID = DBI.DBIID
                INNER JOIN FinishingMachineProcess_HK FMP On FMP.FMProcessID = FPC.ProcessID
                INNER JOIN FinishingMachineConfigurationMaster FMC ON FMC.FMCMasterID = FMP.FMCMasterID
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ET ON ET.ValueID = FPC.ProcessTypeID
                LEFT JOIN FinishingMachineSetup FMS On FMS.FMSID = FPC.FMSID
                LEFT JOIN FinishingMachineSetup PFMS On PFMS.FMSID = FPC.PFMSID
                Left Join {DbNames.EPYSL}..EntityTypeValue b on b.ValueID = FMS.BrandID
                Left Join KnittingUnit c on c.KnittingUnitID = FMS.UnitID
                WHERE DBI.DBIID = {dbId} ORDER BY FPC.SeqNo ASC";

            return await _service.GetDataAsync<DyeingBatchChildFinishingProcess>(sql);
        }

        public async Task<List<DyeingBatchChildFinishingProcess>> GetNewFinishingProcessList(int conceptId, int colorId)
        {
            string sql = $@"WITH FC AS (
	                SELECT FC.ConceptID
	                FROM FreeConceptMaster FC
	                WHERE FC.ConceptID = {conceptId}
                    ),
                    FPC AS (
                    SELECT FPC.*, FP.ConceptID
                    FROM FinishingProcessChild FPC
                    INNER JOIN FinishingProcessMaster FP ON FP.FPMasterID = FPC.FPMasterID
	                INNER JOIN FC ON FC.ConceptID = FP.ConceptID
                    WHERE FPC.ColorID = {colorId} AND FPC.IsPreProcess = 0
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

        public async Task SaveAsync(DyeingBatchMaster entity)
        {
            SqlTransaction transaction = null;
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();
                await _service.SaveSingleAsync(entity, transaction);
                await _service.SaveAsync(entity.DyeingBatchChildFinishingProcesses, transaction);
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

        public async Task SaveBatchItemAsync(DyeingBatchItem entity)
        {
            SqlTransaction transaction = null;
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();
                await _service.SaveSingleAsync(entity, transaction);
                await _service.SaveAsync(entity.DyeingBatchChildFinishingProcesses, transaction);
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

        public async Task UpdateFinishingProcess(List<DyeingBatchChildFinishingProcess> finishingProcessList)
        {
            SqlTransaction transaction = null;
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();
                var maxDBCFPID = await _signatureRepository.GetMaxIdAsync(TableNames.DYEING_BATCH_CHILD_FINISHING_PROCESS, finishingProcessList.Where(x=>x.EntityState == EntityState.Added).Count());
                finishingProcessList.Where(x => x.EntityState == EntityState.Added).ToList().ForEach(process =>
                {
                    process.DBCFPID = maxDBCFPID++;
                });

                //await _service.SaveSingleAsync(entity, transaction);
                await _service.SaveAsync(finishingProcessList, transaction);
                transaction.Commit();

                //#region Update TNA
                //if (finishingProcessList.Count > 0)
                //{
                //    foreach (DyeingBatchChildFinishingProcess item in finishingProcessList)
                //    {
                //        await UpdateBDSTNA_FinishingPlanAsync(item.DBatchID);
                //    }
                //}
                //#endregion

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

        private bool IsInTimeSlot(Select2OptionModel shift)
        {
            if (Convert.ToDecimal(shift.desc) <= Convert.ToDecimal(shift.additionalValue))
            {
                // Time slot does not wrap around to the next day
                return _currentTime >= Convert.ToDecimal(shift.desc) && _currentTime <= Convert.ToDecimal(shift.additionalValue);
            }
            else
            {
                // Time slot wraps around to the next day
                return _currentTime >= Convert.ToDecimal(shift.desc) || _currentTime <= Convert.ToDecimal(shift.additionalValue);
            }
        }
        public async Task UpdateBDSTNA_FinishingPlanAsync(int DBatchID)
        {
            await _service.ExecuteAsync("spUpdateBDSTNA_FinishingPlan", new { DBatchID = DBatchID }, 30, CommandType.StoredProcedure);
        }

    }
}