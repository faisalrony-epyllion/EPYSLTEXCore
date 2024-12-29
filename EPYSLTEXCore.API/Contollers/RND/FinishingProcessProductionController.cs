using EPYSLTEX.Core.Interfaces;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEX.Web.Extends.Helpers;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.Application.Interfaces.RND;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data.Entity;

namespace EPYSLTEX.Web.Controllers.Apis
{
    [Authorize]
    [Route("api/finishing-process-production")]
    public class FinishingProcessProductionController : ApiBaseController
    {
        private readonly IFinishingProcessProductionService _FinishingProcessService;
        private readonly ICommonHelpers _commonHelpers;
        private readonly ICommonHelperService _commonService;
        public FinishingProcessProductionController(IFinishingProcessProductionService FinishingProcessService, IUserService userService
        , ICommonHelpers commonHelpers
             , ICommonHelperService commonService) : base(userService)
        {
            _FinishingProcessService = FinishingProcessService;
            _commonHelpers = commonHelpers;
            _commonService = commonService;
        }

        [Route("list")]
        [HttpGet]
        public async Task<IActionResult> GetList(Status status)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<FinishingProcessMaster> records = await _FinishingProcessService.GetPagedAsync(status, paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        [Route("{id}/{status}")]
        [HttpGet]
        public async Task<IActionResult> Get(int id,Status status)
        {
            FinishingProcessMaster record = await _FinishingProcessService.GetAsync(id,status);
            Guard.Against.NullObject(id, record);

            return Ok(record);
        }

        [HttpGet]
        [Route("machine/{fmsId}/{fpChildID}")]
        public async Task<IActionResult> GetMachineParam(int fmsId, int fpChildID)
        {
            return Ok(await _FinishingProcessService.GetMachineParam(fmsId, fpChildID));
        }

        [Route("save")]
        [HttpPost]
        public async Task<IActionResult> Save(dynamic JsonString )
        {
            FinishingProcessChild model = JsonConvert.DeserializeObject<FinishingProcessChild>(Convert.ToString(JsonString));
            FinishingProcessMaster entity = new FinishingProcessMaster();
            entity = await _FinishingProcessService.GetAllByIDAsync(model.FPMasterID);
            //entity.BatchQty = model.BatchQty;
            entity.EntityState = EntityState.Modified;
            entity.FinishingProcessChilds.SetUnchanged();

            FinishingProcessChild child = entity.FinishingProcessChilds.FirstOrDefault(x => x.FPChildID == model.FPChildID);
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

            if (!entity.FinishingProcessChilds.Where(x => x.ProductionDate == null).Any())
            {
                entity.PDProductionComplete = true;
            }
            await _FinishingProcessService.SaveAsync(entity);
            return Ok();
        }
    }
}