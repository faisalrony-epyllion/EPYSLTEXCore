using EPYSLTEX.Core.Interfaces;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEX.Web.Extends.Helpers;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data.Entity;
namespace EPYSLTEX.Web.Controllers.Apis
{
    [Authorize]
    [Route("api/roll-finishing")]
    public class RollFinishingController : ApiBaseController
    {
        private readonly IRollFinishingInfoService _RollFinishingInfoService;
        private readonly ICommonHelpers _commonHelpers;
        private readonly ICommonHelperService _commonService;
        public RollFinishingController(IRollFinishingInfoService RollFinishingInfoService, IUserService userService
            , ICommonHelpers commonHelpers
            , ICommonHelperService commonService) : base(userService)
        {
            _RollFinishingInfoService = RollFinishingInfoService;
            _commonHelpers = commonHelpers;
            _commonService = commonService;
        }

        [Route("list")]
        [HttpGet]
        public async Task<IActionResult> GetList(Status status)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<DyeingBatchMaster> records = await _RollFinishingInfoService.GetPagedAsync(status, paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        [Route("{dbiID}/{status}")]
        [HttpGet]
        public async Task<IActionResult> Get(int dbiID,Status status)
        {
            DyeingBatchMaster record = await _RollFinishingInfoService.GetAsync(dbiID,status);
            Guard.Against.NullObject(dbiID, record);
            return Ok(record);
        }

        [HttpGet]
        [Route("machine/{fmsId}/{childId}")]
        public async Task<IActionResult> GetMachineParam(int fmsId, int childId)
        {
            return Ok(await _RollFinishingInfoService.GetMachineParam(fmsId, childId));
        }

        [Route("save")]
        [HttpPost]
        public async Task<IActionResult> Save(DyeingBatchChildFinishingProcess model)
        {
            DyeingBatchItem entity = new DyeingBatchItem();
            entity = await _RollFinishingInfoService.GetAllByBDIIDAsync(model.DBIID);
            entity.EntityState = EntityState.Modified;
            entity.DyeingBatchChildFinishingProcesses.SetUnchanged();

            DyeingBatchChildFinishingProcess child = entity.DyeingBatchChildFinishingProcesses.FirstOrDefault(x => x.DBCFPID == model.DBCFPID);
            if (child != null)
            {
                child.ProductionDate = model.ProductionDate;
                child.ShiftID = model.ShiftID;
                child.OperatorID = model.OperatorID;
                child.PFMSID = model.PFMSID;

                child.Param1Value = model.Param1Value;
                child.Param2Value = model.Param2Value;
                child.Param3Value = model.Param3Value;
                child.Param4Value = model.Param4Value;
                child.Param5Value = model.Param5Value;
                child.Param6Value = model.Param6Value;
                child.Param7Value = model.Param7Value;
                child.Param8Value = model.Param8Value;
                child.Param9Value = model.Param9Value;
                child.Param10Value = model.Param10Value;
                child.Param11Value = model.Param11Value;
                child.Param12Value = model.Param12Value;
                child.Param13Value = model.Param13Value;
                child.Param14Value = model.Param14Value;
                child.Param15Value = model.Param15Value;
                child.Param16Value = model.Param16Value;
                child.Param17Value = model.Param17Value;
                child.Param18Value = model.Param18Value;
                child.Param19Value = model.Param19Value;
                child.Param20Value = model.Param20Value;

                child.PParam1Value = model.PParam1Value;
                child.PParam2Value = model.PParam2Value;
                child.PParam3Value = model.PParam3Value;
                child.PParam4Value = model.PParam4Value;
                child.PParam5Value = model.PParam5Value;
                child.PParam6Value = model.PParam6Value;
                child.PParam7Value = model.PParam7Value;
                child.PParam8Value = model.PParam8Value;
                child.PParam9Value = model.PParam9Value;
                child.PParam10Value = model.PParam10Value;
                child.PParam11Value = model.PParam11Value;
                child.PParam12Value = model.PParam12Value;
                child.PParam13Value = model.PParam13Value;
                child.PParam14Value = model.PParam14Value;
                child.PParam15Value = model.PParam15Value;
                child.PParam16Value = model.PParam16Value;
                child.PParam17Value = model.PParam17Value;
                child.PParam18Value = model.PParam18Value;
                child.PParam19Value = model.PParam19Value;
                child.PParam20Value = model.PParam20Value;
                child.EntityState = EntityState.Modified;
            }

            if (!entity.DyeingBatchChildFinishingProcesses.Where(x => x.ProductionDate == null).Any())
            {
                entity.PostProductionComplete = true;
            }
            await _RollFinishingInfoService.SaveBatchItemAsync(entity);
            await _commonService.UpdateFreeConceptStatus(InterfaceFrom.FinishingProcessProduction, entity.ConceptID, "", entity.BookingID, 0, 0, 0, entity.ItemMasterID);
            return Ok();
        }

        [HttpPost]
        [Route("update-finishing-process")]
        public async Task<IActionResult> UpdateFinishingProcess(DyeingBatchMaster model)
        {
            List<DyeingBatchChildFinishingProcess> existFinishingProcessList = await _RollFinishingInfoService.GetExistFinishingProcessList(model.DBIID);
            List<DyeingBatchChildFinishingProcess> newFinishingProcessList = await _RollFinishingInfoService.GetNewFinishingProcessList(model.ConceptID, model.ColorID);

            existFinishingProcessList.ForEach(x =>
            {
                x.EntityState = EntityState.Unchanged;
                //if (x.ProductionDate == null)
                //{
                //    x.EntityState = EntityState.Unchanged;
                //}
            });

            DyeingBatchChildFinishingProcess process;
            foreach (DyeingBatchChildFinishingProcess fp in newFinishingProcessList)
            {
                process = existFinishingProcessList.FirstOrDefault(x => x.ConceptID == fp.ConceptID && x.ProcessID == fp.ProcessID &&
                x.IsPreProcess == fp.IsPreProcess && x.ProcessTypeID == fp.ProcessTypeID);
                if (process == null)
                {
                    process = new DyeingBatchChildFinishingProcess();
                    process.DBatchID = model.DBatchID;
                    process.DBIID = model.DBIID;
                    process.ProcessID = fp.ProcessID;
                    process.SeqNo = fp.SeqNo;
                    process.ProcessTypeID = fp.ProcessTypeID;
                    process.IsPreProcess = fp.IsPreProcess;
                    process.Remarks = fp.Remarks;
                    process.FMSID = fp.FMSID;
                    process.Param1Value = fp.Param1Value;
                    process.Param2Value = fp.Param2Value;
                    process.Param3Value = fp.Param3Value;
                    process.Param4Value = fp.Param4Value;
                    process.Param5Value = fp.Param5Value;
                    process.Param6Value = fp.Param6Value;
                    process.Param7Value = fp.Param7Value;
                    process.Param8Value = fp.Param8Value;
                    process.Param9Value = fp.Param9Value;
                    process.Param10Value = fp.Param10Value;
                    process.Param11Value = fp.Param11Value;
                    process.Param12Value = fp.Param12Value;
                    process.Param13Value = fp.Param13Value;
                    process.Param14Value = fp.Param14Value;
                    process.Param15Value = fp.Param15Value;
                    process.Param16Value = fp.Param16Value;
                    process.Param17Value = fp.Param17Value;
                    process.Param18Value = fp.Param18Value;
                    process.Param19Value = fp.Param19Value;
                    process.Param20Value = fp.Param20Value;
                    process.ProductionDate = fp.ProductionDate;
                    process.ShiftID = fp.ShiftID;
                    process.OperatorID = fp.OperatorID;
                    process.PFMSID = fp.PFMSID;
                    process.PParam1Value = fp.PParam1Value;
                    process.PParam2Value = fp.PParam2Value;
                    process.PParam3Value = fp.PParam3Value;
                    process.PParam4Value = fp.PParam4Value;
                    process.PParam5Value = fp.PParam5Value;
                    process.PParam6Value = fp.PParam6Value;
                    process.PParam7Value = fp.PParam7Value;
                    process.PParam8Value = fp.PParam8Value;
                    process.PParam9Value = fp.PParam9Value;
                    process.PParam10Value = fp.PParam10Value;
                    process.PParam11Value = fp.PParam11Value;
                    process.PParam12Value = fp.PParam12Value;
                    process.PParam13Value = fp.PParam13Value;
                    process.PParam14Value = fp.PParam14Value;
                    process.PParam15Value = fp.PParam15Value;
                    process.PParam16Value = fp.PParam16Value;
                    process.PParam17Value = fp.PParam17Value;
                    process.PParam18Value = fp.PParam18Value;
                    process.PParam19Value = fp.PParam19Value;
                    process.PParam20Value = fp.PParam20Value;
                    process.EntityState = EntityState.Added;
                    existFinishingProcessList.Add(process);
                }
                else
                {
                    process.EntityState = EntityState.Modified;
                    if (process.ProductionDate == null)
                    {
                        process.SeqNo = fp.SeqNo;
                        process.Param1Value = fp.Param1Value;
                        process.Param2Value = fp.Param2Value;
                        process.Param3Value = fp.Param3Value;
                        process.Param4Value = fp.Param4Value;
                        process.Param5Value = fp.Param5Value;
                        process.Param6Value = fp.Param6Value;
                        process.Param7Value = fp.Param7Value;
                        process.Param8Value = fp.Param8Value;
                        process.Param9Value = fp.Param9Value;
                        process.Param10Value = fp.Param10Value;
                        process.Param11Value = fp.Param11Value;
                        process.Param12Value = fp.Param12Value;
                        process.Param13Value = fp.Param13Value;
                        process.Param14Value = fp.Param14Value;
                        process.Param15Value = fp.Param15Value;
                        process.Param16Value = fp.Param16Value;
                        process.Param17Value = fp.Param17Value;
                        process.Param18Value = fp.Param18Value;
                        process.Param19Value = fp.Param19Value;
                        process.Param20Value = fp.Param20Value;
                        process.ShiftID = fp.ShiftID;
                        process.OperatorID = fp.OperatorID;
                        process.PFMSID = fp.PFMSID;
                        process.PParam1Value = fp.PParam1Value;
                        process.PParam2Value = fp.PParam2Value;
                        process.PParam3Value = fp.PParam3Value;
                        process.PParam4Value = fp.PParam4Value;
                        process.PParam5Value = fp.PParam5Value;
                        process.PParam6Value = fp.PParam6Value;
                        process.PParam7Value = fp.PParam7Value;
                        process.PParam8Value = fp.PParam8Value;
                        process.PParam9Value = fp.PParam9Value;
                        process.PParam10Value = fp.PParam10Value;
                        process.PParam11Value = fp.PParam11Value;
                        process.PParam12Value = fp.PParam12Value;
                        process.PParam13Value = fp.PParam13Value;
                        process.PParam14Value = fp.PParam14Value;
                        process.PParam15Value = fp.PParam15Value;
                        process.PParam16Value = fp.PParam16Value;
                        process.PParam17Value = fp.PParam17Value;
                        process.PParam18Value = fp.PParam18Value;
                        process.PParam19Value = fp.PParam19Value;
                        process.PParam20Value = fp.PParam20Value;
                    }
                }
            }

            //existFinishingProcessList.Where(x => x.ProductionDate == null).ToList().ForEach(x =>
            //  {
            //      if (x.EntityState == EntityState.Unchanged) x.EntityState = EntityState.Deleted;
            //  });
            existFinishingProcessList.ToList().ForEach(x =>
            {
                if (x.EntityState == EntityState.Unchanged) x.EntityState = EntityState.Deleted;
            });

            await _RollFinishingInfoService.UpdateFinishingProcess(existFinishingProcessList);
            var data = await _RollFinishingInfoService.GetExistFinishingProcessList(model.DBIID);
            if (model.DBatchID > 0)
            {
                await _RollFinishingInfoService.UpdateBDSTNA_FinishingPlanAsync(model.DBatchID);
            }
            return Ok(data);
        }

    }
}