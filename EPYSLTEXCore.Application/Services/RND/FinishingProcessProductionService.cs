using Dapper;
using EPYSLTEXCore.Application.Interfaces.RND;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.General;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.Data.SqlClient;

namespace EPYSLTEXCore.Application.Services.RND
{
    public class FinishingProcessProductionService : IFinishingProcessProductionService
    {
        private readonly IDapperCRUDService<FinishingProcessMaster> _service;
        private readonly SqlConnection _connection;
        private readonly SqlConnection _gmtConnection;
        private decimal _currentTime=0;

        public FinishingProcessProductionService(
            IDapperCRUDService<FinishingProcessMaster> service)
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
            if (status == Status.Pending)
            {
                sql = $@"
                WITH
                FinalList AS
                (
	                SELECT FP.FPMasterID, FP.ConceptID, FP.BookingID, FP.TrialNo, FP.TrialDate, CM.ConceptNo, CM.ConceptDate,FP.PFBatchNo,FP.PFBatchDate,FP.BatchQty
	                FROM FinishingProcessChild FPC
	                INNER JOIN FinishingProcessMaster FP ON FP.FPMasterID = FPC.FPMasterID
	                LEFT JOIN FreeConceptMaster CM ON CM.ConceptID = FP.ConceptID
	                WHERE FP.PDProductionComplete = 0 AND FPC.IsPreProcess = 1
	                GROUP BY FP.FPMasterID, FP.ConceptID, FP.BookingID, FP.TrialNo, FP.TrialDate, CM.ConceptNo, CM.ConceptDate,FP.PFBatchNo,FP.PFBatchDate,FP.BatchQty
                )
                SELECT *,Count(*) Over() TotalRows FROM FinalList ";
            }
            else
            {
                sql = $@"
                WITH
                FinalList AS
                (
	                SELECT FP.FPMasterID, FP.ConceptID, FP.BookingID, FP.TrialNo, FP.TrialDate, CM.ConceptNo, CM.ConceptDate,FP.PFBatchNo,FP.PFBatchDate,FP.BatchQty
	                FROM FinishingProcessChild FPC
	                INNER JOIN FinishingProcessMaster FP ON FP.FPMasterID = FPC.FPMasterID
	                LEFT JOIN FreeConceptMaster CM ON CM.ConceptID = FP.ConceptID
	                WHERE FP.PDProductionComplete = 1 AND FPC.IsPreProcess = 1
	                GROUP BY FP.FPMasterID, FP.ConceptID, FP.BookingID, FP.TrialNo, FP.TrialDate, CM.ConceptNo, CM.ConceptDate,FP.PFBatchNo,FP.PFBatchDate,FP.BatchQty
                )
                SELECT *,Count(*) Over() TotalRows FROM FinalList ";
            }

            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<FinishingProcessMaster>(sql);
        }

        public async Task<FinishingProcessMaster> GetAsync(int id, Status status)
        {
            var query =
                $@"
                ;SELECT FP.FPMasterID, FP.ConceptID, FP.BookingID, FP.TrialNo, FP.TrialDate, CM.ConceptNo, CM.ConceptDate, KP.NeedPreFinishingProcess,FP.PFBatchNo,FP.PFBatchDate,FP.BatchQty
                FROM FinishingProcessMaster FP
                LEFT JOIN KnittingPlanMaster KP ON KP.ConceptID = FP.ConceptID
                LEFT JOIN FreeConceptMaster CM ON CM.ConceptID = FP.ConceptID
                WHERE FP.FPMasterID = {id};

                ----Child Pre-Process
                ;SELECT FPC.FPChildID, FPC.FPMasterID, FPC.ProcessID, FPC.SeqNo, FMP.ProcessName, C.ShortName UnitName,b.ValueName BrandName,
                FPC.ProcessTypeID, ET.ValueName ProcessType, FPC.IsPreProcess, FPC.FMSID, FMC.FMCMasterID, FMS.MachineNo, MachineName=FMC.ProcessName ,
                FPC.Remarks, FPC.Param1Value, FPC.Param2Value, FPC.Param3Value, FPC.Param4Value, FPC.Param5Value, FPC.Param6Value, FPC.Param7Value,
                FPC.Param8Value, FPC.Param9Value, FPC.Param10Value, FPC.Param11Value, FPC.Param12Value, FPC.Param13Value, FPC.Param14Value, FPC.Param15Value,
                FPC.Param16Value, FPC.Param17Value, FPC.Param18Value, FPC.Param19Value, FPC.Param20Value,
                FPC.ProductionDate, FPC.ShiftID, FPC.OperatorID, FPC.PFMSID, PFMS.MachineNo PMachineNo, pb.ValueName PBrandName, pc.ShortName PUnitName, FPC.PParam1Value, FPC.PParam2Value, FPC.PParam3Value, FPC.PParam4Value,
                FPC.PParam5Value, FPC.PParam6Value, FPC.PParam7Value, FPC.PParam8Value, FPC.PParam9Value, FPC.PParam10Value, FPC.PParam11Value,
                FPC.PParam12Value, FPC.PParam13Value, FPC.PParam14Value, FPC.PParam15Value, FPC.PParam16Value,
                FPC.PParam17Value, FPC.PParam18Value, FPC.PParam19Value, FPC.PParam20Value
                FROM FinishingProcessChild FPC
                INNER JOIN FinishingMachineProcess_HK FMP On FMP.FMProcessID = FPC.ProcessID
                INNER JOIN FinishingMachineConfigurationMaster FMC ON FMC.FMCMasterID = FMP.FMCMasterID
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ET ON ET.ValueID = FPC.ProcessTypeID
                LEFT JOIN FinishingMachineSetup FMS On FMS.FMSID = FPC.FMSID
                LEFT JOIN FinishingMachineSetup PFMS On PFMS.FMSID = FPC.PFMSID
                Left Join {DbNames.EPYSL}..EntityTypeValue b on b.ValueID = FMS.BrandID
		        Left Join {DbNames.EPYSL}..EntityTypeValue pb on pb.ValueID = PFMS.BrandID
                Left Join KnittingUnit c on c.KnittingUnitID = FMS.UnitID
                Left Join KnittingUnit pc on pc.KnittingUnitID = PFMS.UnitID
                WHERE FPC.FPMasterID = {id} AND FPC.IsPreProcess = 1 ORDER BY FPC.SeqNo ASC;

                --Operators
                ;select E.EmployeeCode id, E.DisplayEmployeeCode +' '+EmployeeName text
                from {DbNames.EPYSL}..Employee E
                INNER JOIN {DbNames.EPYSL}..EmployeeDesignation D ON D.DesigID=E.DesigID
				INNER JOIN {DbNames.EPYSL}..EmployeeDepartment ED ON ED.DepertmentID= E.DepertmentID
                where D.Designation like '%operator%' and ED.DepertmentDescription like '%Knitting%';

                --Shifts
                ;SELECT CAST(ShiftId AS VARCHAR) id, ShortName text, CAST(FromHour AS nvarchar(6)) [desc],CAST(ToHour AS nvarchar(6)) additionalValue
                FROM {DbNames.EPYSL}..ShiftInfo
                Where CompanyID=6
                order by SeqNo;

                ----FinishingMachineConfigurationChild
                ;SELECT *
                From FinishingMachineConfigurationChild FMC;";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                FinishingProcessMaster data = records.Read<FinishingProcessMaster>().FirstOrDefault();
                data.PreFinishingProcessChilds = records.Read<FinishingProcessChild>().ToList();
                data.OperatorList = records.Read<Select2OptionModel>().ToList();
                data.ShiftList = records.Read<Select2OptionModel>().ToList();
                data.FinishingMachineConfigurationChildList = records.Read<FinishingMachineConfigurationChild>().ToList();

                //var currentTime = Convert.ToDecimal($"{DateTime.Now.Hour}.{DateTime.Now.Minute}");
                //int.TryParse(data.ShiftList.FirstOrDefault(x => currentTime >= Convert.ToDecimal(x.desc) && currentTime <= Convert.ToDecimal(x.additionalValue))?.id, out int shiftId);

                int shiftId = 0;
                if (status == Status.Pending)
                {
                    _currentTime = Convert.ToDecimal($"{DateTime.Now.Hour}.{DateTime.Now.Minute}");
                    shiftId = Convert.ToInt32(data.ShiftList.FirstOrDefault(x => IsInTimeSlot(x)).id);
                }

                if (shiftId > 0)
                {
                    data.PreFinishingProcessChilds.ForEach(x =>
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

        public async Task<FinishingProcessMaster> GetMachineParam(int fmsId, int fpChildId)
        {
            var query = $@"
                -- Process Machine param List
                    ;WITH FMS AS(
                    SELECT FMSID,FMCMasterID,REPLACE(REPLACE(ParamName, 'Param', ''),'Value','') AS SerialNo, MachineNo, BrandID, UnitID, Capacity,ParamName, ParamValue
                    FROM (SELECT * FROM FinishingMachineSetup
                    WHERE FMSID = {fmsId}
                    ) p
                    UNPIVOT
                    (ParamValue FOR ParamName IN (Param1Value, Param2Value, Param3Value, Param4Value, Param5Value, Param6Value, Param7Value, Param8Value, Param9Value, Param10Value, Param11Value, Param12Value, Param13Value, Param14Value, Param15Value, Param16Value, Param17Value, Param18Value, Param19Value, Param20Value)
                    )AS unpvt
                    ), FP AS(
                    SELECT FMSID,REPLACE(REPLACE(ParamName, 'Param', ''),'Value','') AS SerialNo, ParamName, ParamValue
                    FROM (Select * From FinishingProcessChild
                    Where FMSID = {fmsId} And FPChildID = {fpChildId} AND IsPreProcess = 1
                    ) p
                    UNPIVOT
                    (ParamValue FOR ParamName IN (Param1Value, Param2Value, Param3Value, Param4Value, Param5Value, Param6Value, Param7Value, Param8Value, Param9Value, Param10Value, Param11Value, Param12Value, Param13Value, Param14Value, Param15Value, Param16Value, Param17Value, Param18Value, Param19Value, Param20Value)
                    )AS unpvt
                    ), AFP AS(
                    SELECT FMSID, FPChildID, PFMSID, REPLACE(REPLACE(ParamName, 'PParam', ''),'Value','') AS SerialNo, ParamName, ParamValue
                    FROM (Select FMSID, FPChildID,PFMSID,
                    ISNULL(PParam1Value,'') PParam1Value, ISNULL(PParam2Value,'') PParam2Value, ISNULL(PParam3Value,'') PParam3Value, ISNULL(PParam4Value,'') PParam4Value, ISNULL(PParam5Value,'') PParam5Value,
                    ISNULL(PParam6Value,'') PParam6Value, ISNULL(PParam7Value,'') PParam7Value, ISNULL(PParam8Value,'') PParam8Value, ISNULL(PParam9Value,'') PParam9Value, ISNULL(PParam10Value,'') PParam10Value,
                    ISNULL(PParam11Value,'') PParam11Value, ISNULL(PParam12Value,'') PParam12Value, ISNULL(PParam13Value,'') PParam13Value, ISNULL(PParam14Value,'') PParam14Value, ISNULL(PParam15Value,'') PParam15Value,
                    ISNULL(PParam16Value,'') PParam16Value, ISNULL(PParam17Value,'') PParam17Value, ISNULL(PParam18Value,'') PParam18Value, ISNULL(PParam19Value,'') PParam19Value, ISNULL(PParam20Value,'') PParam20Value
                    From FinishingProcessChild
                    Where FMSID = {fmsId} And FPChildID = {fpChildId} AND IsPreProcess = 1
                    ) p
                    UNPIVOT
                    (ParamValue FOR ParamName IN (PParam1Value, PParam2Value, PParam3Value, PParam4Value, PParam5Value, PParam6Value, PParam7Value, PParam8Value, PParam9Value, PParam10Value, PParam11Value, PParam12Value, PParam13Value, PParam14Value, PParam15Value, PParam16Value, PParam17Value, PParam18Value, PParam19Value, PParam20Value)
                    )AS unpvt
                    )
                    --Select * From FMS
                    ,M AS(
                    SELECT ETV.ValueName, FMS.FMSID,FMS.FMCMasterID,FMS.SerialNo, FMS.MachineNo, FMS.BrandID, FMS.UnitID,FMS.Capacity,FMS.ParamName, FMS.ParamValue,FMCC.ParamName AS ParamDispalyName, FMCC.NeedItem, FP.ParamValue PlanParamValue, AFP.ParamValue ActulaPlanParamValue, AFP.PFMSID, AFMS.BrandID ABrandID, AFMS.UnitID AUnitID
                    ,pb.ValueName PBrandName, pc.ShortName PUnitName, AFP.FPChildID
                    FROM FinishingMachineConfigurationChild FMCC
                    INNER JOIN FMS ON FMS.FMCMasterID=FMCC.FMCMasterID AND FMS.SerialNo=FMCC.Sequence
                    LEFT JOIN FP ON FP.FMSID=FMS.FMSID AND FP.SerialNo=FMS.SerialNo
                    LEFT JOIN AFP ON AFP.FMSID=FMS.FMSID AND AFP.SerialNo=FMS.SerialNo
                    INNER JOIN {DbNames.EPYSL}..EntityTypeValue ETV On ETV.ValueID = FMCC.ProcessTypeID
                    LEFT JOIN FinishingMachineSetup AFMS On AFMS.FMSID = AFP.PFMSID
                    LEFT JOIN {DbNames.EPYSL}..EntityTypeValue pb on pb.ValueID = AFMS.BrandID
                    LEFT JOIN KnittingUnit pc on pc.KnittingUnitID = AFMS.UnitID
                    Where ETV.ValueName in ('Pre/Post Set','Pre Set')
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

        /*
            public async Task<FinishingProcessMaster> GetMachineParam(int fmsId, int fpChildId)
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
                FROM (Select * From FinishingProcessChild
                Where FMSID = {fmsId} And FPChildID = {fpChildId} AND IsPreProcess = 1
                ) p
                UNPIVOT
                (ParamValue FOR ParamName IN (Param1Value, Param2Value, Param3Value, Param4Value, Param5Value, Param6Value, Param7Value, Param8Value, Param9Value, Param10Value, Param11Value, Param12Value, Param13Value, Param14Value, Param15Value, Param16Value, Param17Value, Param18Value, Param19Value, Param20Value)
                )AS unpvt
                ), AFP AS(
                SELECT FMSID,REPLACE(REPLACE(ParamName, 'PParam', ''),'Value','') AS SerialNo, ParamName, ParamValue
                FROM (Select * From FinishingProcessChild
                Where FMSID = {fmsId} And FPChildID = {fpChildId} AND IsPreProcess = 1
                ) p
                UNPIVOT
                (ParamValue FOR ParamName IN (PParam1Value, PParam2Value, PParam3Value, PParam4Value, PParam5Value, PParam6Value, PParam7Value, PParam8Value, PParam9Value, PParam10Value, PParam11Value, PParam12Value, PParam13Value, PParam14Value, PParam15Value, PParam16Value, PParam17Value, PParam18Value, PParam19Value, PParam20Value)
                )AS unpvt
                )
                --Select * From FP
                ,M AS(
                SELECT ETV.ValueName, FMS.FMSID,FMS.FMCMasterID,FMS.SerialNo, FMS.MachineNo, FMS.BrandID, FMS.UnitID,FMS.Capacity,FMS.ParamName, FMS.ParamValue,FMCC.ParamName AS ParamDispalyName, FMCC.NeedItem, FP.ParamValue PlanParamValue, AFP.ParamValue ActulaPlanParamValue
                FROM FinishingMachineConfigurationChild FMCC
                INNER JOIN FMS ON FMS.FMCMasterID=FMCC.FMCMasterID AND FMS.SerialNo=FMCC.Sequence
                LEFT JOIN FP ON FP.FMSID=FMS.FMSID AND FP.SerialNo=FMS.SerialNo
                LEFT JOIN AFP ON AFP.FMSID=FMS.FMSID AND AFP.SerialNo=FMS.SerialNo
                Inner Join {DbNames.EPYSL}..EntityTypeValue ETV On ETV.ValueID = FMCC.ProcessTypeID
                Where ETV.ValueName in ('Pre/Post Set','Pre Set')
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
         */
        public async Task<FinishingProcessMaster> GetAllByIDAsync(int id)
        {
            string sql = $@"
            
            ;SELECT * FROM FinishingProcessMaster WHERE FPMasterID = {id}
			
			;SELECT *, pb.ValueName PBrandName, pc.ShortName PUnitName
			FROM FinishingProcessChild FPC
			LEFT JOIN FinishingMachineSetup PFMS On PFMS.FMSID = FPC.PFMSID
			LEFT JOIN {DbNames.EPYSL}..EntityTypeValue pb on pb.ValueID = PFMS.BrandID
			LEFT JOIN KnittingUnit pc on pc.KnittingUnitID = PFMS.UnitID
			WHERE FPMasterID = {id} AND IsPreProcess = 1";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                FinishingProcessMaster data = records.Read<FinishingProcessMaster>().FirstOrDefault();
                Guard.Against.NullObject(data);
                data.FinishingProcessChilds = records.Read<FinishingProcessChild>().ToList();
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

                await _service.SaveSingleAsync(entity, transaction);
                await _service.SaveAsync(entity.FinishingProcessChilds, transaction);

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
    }
}