using EPYSLTEX.Core.Interfaces;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEX.Web.Extends.Helpers;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.API.Extends.Filters;
using EPYSLTEXCore.Application.Interfaces;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data.Entity;

namespace EPYSLTEX.Web.Controllers.Apis
{
    [Authorize]
    [Route("api/finishing-process")]
    [ApiController]

    public class FinishingProcessController : ApiBaseController
    {
        private readonly IFinishingProcessService _FinishingProcessService;
        private readonly ICommonHelpers _commonHelpers;
        private readonly ICommonHelperService _commonService;
        public FinishingProcessController(IFinishingProcessService FinishingProcessService, IUserService userService
            , ICommonHelpers commonHelpers, ICommonHelperService commonService) : base(userService)
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

        [HttpGet]
        [Route("new/{conceptId}/{isBDS}/{grpConceptNo}")]
        public async Task<IActionResult> GetNew(int conceptId, int isBDS, string grpConceptNo)
        {
            return Ok(await _FinishingProcessService.GetNewAsync(conceptId, isBDS, grpConceptNo));
        }

        [HttpGet]
        [Route("machine/{fmsId}")]
        public async Task<IActionResult> GetMachineParam(int fmsId)
        {
            return Ok(await _FinishingProcessService.GetMachineParam(fmsId));
        }

        [Route("{id}/{conceptId}/{isBDS}/{grpConceptNo}")]
        [HttpGet]
        public async Task<IActionResult> Get(int id, int conceptId, int isBDS, string grpConceptNo)
        {
            FinishingProcessMaster record = await _FinishingProcessService.GetAsync(id, conceptId, isBDS, grpConceptNo);
            return Ok(record);
        }

        [Route("raw-item-by-type/{particularName}/{fpChildId}")]
        [HttpGet]
        public async Task<IActionResult> GetChamicalItem(string particularName, int fpChildId)
        {
            var paginationInfo = Request.GetPaginationInfo();
            var records = await _FinishingProcessService.GetChamicalItem(particularName, fpChildId);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        [Route("get-finishing-process-child-items/{particularName}/{fpChildId}")]
        [HttpGet]
        public async Task<IActionResult> GetFinishingProcessChildItems(string particularName, int fpChildId)
        {
            var paginationInfo = Request.GetPaginationInfo();
            var records = await _FinishingProcessService.GetFinishingProcessChildItems(particularName, fpChildId);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }
        [HttpGet]
        [Route("get-finishing-process/{setName}")]
        public async Task<IActionResult> GetFinishingMachineProcess(string setName)
        {
            var paginationInfo = Request.GetPaginationInfo();
            var records = await _FinishingProcessService.GetFinishingMachineProcess(paginationInfo, setName);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        [Route("save")]
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> Save(FinishingProcessMaster model)
        {
            FinishingProcessMaster entity; 
            if (model.FPMasterID > 0) 
            {
                entity = await _FinishingProcessService.GetAllByIDAsync(model.FPMasterID);
                entity.UpdatedBy = AppUser.UserCode;
                entity.DateUpdated = DateTime.Now;
                entity.PFBatchDate = model.PFBatchDate;
                entity.BatchQty = model.BatchQty;
                entity.EntityState = EntityState.Modified;

                foreach (FinishingProcessChild item in entity.FinishingProcessChilds)
                    item.EntityState = EntityState.Unchanged;
                foreach (FinishingProcessChildItem item in entity.PreFinishingProcessChildItems)
                    item.EntityState = EntityState.Unchanged;
                //foreach (FinishingProcessChild item in entity.FinishingProcessChilds)
                //{
                //    item.EntityState = EntityState.Unchanged;
                //    foreach (FinishingProcessChildItem childItem in item.PreFinishingProcessChildItems)
                //        childItem.EntityState = EntityState.Unchanged;
                //}

                int nSeq = 1;
                foreach (FinishingProcessChild item in model.PreFinishingProcessChilds)
                {
                    FinishingProcessChild childEntity = entity.FinishingProcessChilds.FirstOrDefault(x => x.IsPreProcess == true && x.FPChildID == item.FPChildID && x.FPMasterID == item.FPMasterID);
                    if (childEntity == null)
                    {
                        childEntity = item;
                        childEntity.SeqNo = nSeq++;
                        childEntity.IsPreProcess = true;
                        childEntity.FMSID = item.FMSID;
                        childEntity.Param1Value = item.Param1Value;
                        childEntity.Param2Value = item.Param2Value;
                        childEntity.Param3Value = item.Param3Value;
                        childEntity.Param4Value = item.Param4Value;
                        childEntity.Param5Value = item.Param5Value;
                        childEntity.Param6Value = item.Param6Value;
                        childEntity.Param7Value = item.Param7Value;
                        childEntity.Param8Value = item.Param8Value;
                        childEntity.Param9Value = item.Param9Value;
                        childEntity.Param10Value = item.Param10Value;
                        childEntity.Param11Value = item.Param11Value;
                        childEntity.Param12Value = item.Param12Value;
                        childEntity.Param13Value = item.Param13Value;
                        childEntity.Param14Value = item.Param14Value;
                        childEntity.Param15Value = item.Param15Value;
                        childEntity.Param16Value = item.Param16Value;
                        childEntity.Param17Value = item.Param17Value;
                        childEntity.Param18Value = item.Param18Value;
                        childEntity.Param19Value = item.Param19Value;
                        childEntity.Param20Value = item.Param20Value;
                        // childEntity.PreFinishingProcessChildItems = item.PreFinishingProcessChildItems;
                        entity.FinishingProcessChilds.Add(childEntity);
                    }
                    else
                    {
                        childEntity.ProcessID = item.ProcessID;
                        childEntity.SeqNo = nSeq++;
                        childEntity.IsPreProcess = true;
                        childEntity.FMSID = item.FMSID;
                        childEntity.Remarks = item.Remarks;
                        childEntity.Param1Value = item.Param1Value;
                        childEntity.Param2Value = item.Param2Value;
                        childEntity.Param3Value = item.Param3Value;
                        childEntity.Param4Value = item.Param4Value;
                        childEntity.Param5Value = item.Param5Value;
                        childEntity.Param6Value = item.Param6Value;
                        childEntity.Param7Value = item.Param7Value;
                        childEntity.Param8Value = item.Param8Value;
                        childEntity.Param9Value = item.Param9Value;
                        childEntity.Param10Value = item.Param10Value;
                        childEntity.Param11Value = item.Param11Value;
                        childEntity.Param12Value = item.Param12Value;
                        childEntity.Param13Value = item.Param13Value;
                        childEntity.Param14Value = item.Param14Value;
                        childEntity.Param15Value = item.Param15Value;
                        childEntity.Param16Value = item.Param16Value;
                        childEntity.Param17Value = item.Param17Value;
                        childEntity.Param18Value = item.Param18Value;
                        childEntity.Param19Value = item.Param19Value;
                        childEntity.Param20Value = item.Param20Value;
                        //foreach (FinishingProcessChildItem ci in item.PreFinishingProcessChildItems)
                        //{
                        //    FinishingProcessChildItem ci1 = childEntity.PreFinishingProcessChildItems.FirstOrDefault(y => y.ItemMasterID == ci.ItemMasterID);
                        //    if (ci1 == null)
                        //    {
                        //        childEntity.PreFinishingProcessChildItems.Add(ci1);
                        //    }
                        //    else
                        //    {
                        //        ci1.EntityState = EntityState.Modified;
                        //    }
                        //}
                        childEntity.EntityState = item.EntityState == EntityState.Deleted ? EntityState.Deleted : EntityState.Modified;
                    }
                }
                nSeq = 1;
                foreach (FinishingProcessChild item in model.PostFinishingProcessChilds)
                {
                    FinishingProcessChild childEntity = entity.FinishingProcessChilds.FirstOrDefault(x => x.IsPreProcess == false && x.FPChildID == item.FPChildID && x.FPMasterID == item.FPMasterID && x.ColorID == item.ColorID);
                    if (childEntity == null)
                    {
                        childEntity = item;
                        childEntity.SeqNo = nSeq++;
                        childEntity.IsPreProcess = false;
                        childEntity.FMSID = item.FMSID;
                        childEntity.Remarks = item.Remarks;
                        childEntity.Param1Value = item.Param1Value;
                        childEntity.Param2Value = item.Param2Value;
                        childEntity.Param3Value = item.Param3Value;
                        childEntity.Param4Value = item.Param4Value;
                        childEntity.Param5Value = item.Param5Value;
                        childEntity.Param6Value = item.Param6Value;
                        childEntity.Param7Value = item.Param7Value;
                        childEntity.Param8Value = item.Param8Value;
                        childEntity.Param9Value = item.Param9Value;
                        childEntity.Param10Value = item.Param10Value;
                        childEntity.Param11Value = item.Param11Value;
                        childEntity.Param12Value = item.Param12Value;
                        childEntity.Param13Value = item.Param13Value;
                        childEntity.Param14Value = item.Param14Value;
                        childEntity.Param15Value = item.Param15Value;
                        childEntity.Param16Value = item.Param16Value;
                        childEntity.Param17Value = item.Param17Value;
                        childEntity.Param18Value = item.Param18Value;
                        childEntity.Param19Value = item.Param19Value;
                        childEntity.Param20Value = item.Param20Value;
                        //  childEntity.PreFinishingProcessChildItems = item.PreFinishingProcessChildItems;
                        entity.FinishingProcessChilds.Add(childEntity);
                    }
                    else
                    {
                        childEntity.ProcessID = item.ProcessID;
                        childEntity.SeqNo = nSeq++;
                        childEntity.IsPreProcess = false;
                        childEntity.FMSID = item.FMSID;
                        childEntity.Remarks = item.Remarks;
                        childEntity.Param1Value = item.Param1Value;
                        childEntity.Param2Value = item.Param2Value;
                        childEntity.Param3Value = item.Param3Value;
                        childEntity.Param4Value = item.Param4Value;
                        childEntity.Param5Value = item.Param5Value;
                        childEntity.Param6Value = item.Param6Value;
                        childEntity.Param7Value = item.Param7Value;
                        childEntity.Param8Value = item.Param8Value;
                        childEntity.Param9Value = item.Param9Value;
                        childEntity.Param10Value = item.Param10Value;
                        childEntity.Param11Value = item.Param11Value;
                        childEntity.Param12Value = item.Param12Value;
                        childEntity.Param13Value = item.Param13Value;
                        childEntity.Param14Value = item.Param14Value;
                        childEntity.Param15Value = item.Param15Value;
                        childEntity.Param16Value = item.Param16Value;
                        childEntity.Param17Value = item.Param17Value;
                        childEntity.Param18Value = item.Param18Value;
                        childEntity.Param19Value = item.Param19Value;
                        childEntity.Param20Value = item.Param20Value;
                        foreach (FinishingProcessChildItem ci in item.PreFinishingProcessChildItems)
                        {
                            FinishingProcessChildItem childItem = childEntity.PreFinishingProcessChildItems.FirstOrDefault(y => y.ItemMasterID == ci.ItemMasterID);
                            if (childItem == null)
                            {
                                childItem = new FinishingProcessChildItem();
                                childItem.FPChildID = childEntity.FPChildID;
                                childItem.FPMasterID = childEntity.FPMasterID;
                                childItem.ItemMasterID = ci.ItemMasterID;
                                childItem.Qty = ci.Qty;
                                childEntity.PreFinishingProcessChildItems.Add(childItem);
                            }
                            else
                            {
                                childItem.Qty = ci.Qty;
                                childItem.EntityState = EntityState.Modified;
                            }
                        }
                        childEntity.EntityState = item.EntityState == EntityState.Deleted ? EntityState.Deleted : EntityState.Modified;
                    }

                }
            }
            else
            {
                entity = model;
                entity.AddedBy = AppUser.UserCode;
                FinishingProcessChild childEntity;
                int nSeq = 1;
                foreach (FinishingProcessChild item in model.PreFinishingProcessChilds)
                {
                    childEntity = item;
                    childEntity.SeqNo = nSeq++;
                    childEntity.IsPreProcess = true;
                    childEntity.FMSID = item.FMSID;
                    entity.FinishingProcessChilds.Add(childEntity);
                }
                nSeq = 1;
                foreach (FinishingProcessChild item in model.PostFinishingProcessChilds)
                {
                    childEntity = item;
                    childEntity.SeqNo = nSeq++;
                    childEntity.IsPreProcess = false;
                    childEntity.FMSID = item.FMSID;
                    entity.FinishingProcessChilds.Add(childEntity);
                }
            }
            await _FinishingProcessService.SaveAsync(entity);
            return Ok();
        }
    }
}