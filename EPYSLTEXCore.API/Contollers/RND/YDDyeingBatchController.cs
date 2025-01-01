using EPYSLTEX.Core.Interfaces;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.API.Extends.Filters;
using EPYSLTEXCore.Application.Interfaces;
using EPYSLTEXCore.Application.Interfaces.RND;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data.Entity;

namespace EPYSLTEXCore.API.Contollers.RND
{
    [Route("api/yd-dyeing-batch")]
    public class YDDyeingBatchController : ApiBaseController
    {
        private readonly IYDDyeingBatchService _YDDyeingBatchService;
        private readonly ICommonHelperService _commonService;

        public YDDyeingBatchController(IUserService userService, IYDDyeingBatchService YDDyeingBatchService, ICommonHelperService commonService) : base(userService)
        {
            _YDDyeingBatchService = YDDyeingBatchService;
            _commonService = commonService;
        }

        [Route("list")]
        [HttpGet]
        public async Task<IActionResult> GetList(Status status)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<YDDyeingBatchMaster> records = await _YDDyeingBatchService.GetPagedAsync(status, paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }
        [HttpGet]
        [Route("new/{newId}")]
        public async Task<IActionResult> GetNew(int newId)
        {
            YDDyeingBatchMaster record = await _YDDyeingBatchService.GetNewAsync(newId);
            foreach (YDDyeingBatchRecipe recipeDefinitionChild in record.YDDyeingBatchRecipes)
                recipeDefinitionChild.DefChilds = record.DefChilds.Where(x => x.YDRecipeDInfoID == recipeDefinitionChild.YDRecipeDInfoID).ToList();
            foreach (YDDyeingBatchItem item in record.YDDyeingBatchItems)
            {
                item.YDDyeingBatchItemRolls = record.YDDyeingBatchItemRolls.Where(x => x.BItemReqID == item.BItemReqID).ToList();
            }
            return Ok(record);
        }
        [HttpGet]
        [Route("new/multiSelect/{batchIds}")]
        public async Task<IActionResult> GetNewMultiSelect(string batchIds)
        {
            List<YDDyeingBatchMaster> record = await _YDDyeingBatchService.GetNewMultiSelectAsync(batchIds);
            record.ForEach(m =>
            {
                foreach (YDDyeingBatchRecipe recipeDefinitionChild in m.YDDyeingBatchRecipes)
                {
                    recipeDefinitionChild.DefChilds = m.DefChilds.Where(x => x.YDRecipeDInfoID == recipeDefinitionChild.YDRecipeDInfoID).ToList();
                }
                //if (m.YDDyeingBatchItems.Count > 0)
                //{
                //    foreach (YDDyeingBatchItem item in m.YDDyeingBatchItems)
                //    {
                //        item.YDDyeingBatchItemRolls = m.YDDyeingBatchItemRolls.Where(x => x.BItemReqID == item.BItemReqID).ToList();
                //    }
                //}
            });
            return Ok(record);
        }
        [Route("{id}")]
        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            YDDyeingBatchMaster record = await _YDDyeingBatchService.GetAsync(id);
            Guard.Against.NullObject(id, record);
            foreach (YDDyeingBatchRecipe recipeDefinitionChild in record.YDDyeingBatchRecipes)
                recipeDefinitionChild.DefChilds = record.DefChilds.Where(x => x.YDRecipeDInfoID == recipeDefinitionChild.YDRecipeDInfoID).ToList();
            foreach (YDDyeingBatchItem item in record.YDDyeingBatchItems)
            {
                item.YDDyeingBatchItemRolls = record.YDDyeingBatchItemRolls.Where(x => x.YDDBIID == item.YDDBIID).ToList();
            }
            return Ok(record);
        }

        [Route("batch-list/{batchIds}")]
        [HttpGet]
        public async Task<IActionResult> GetBatchList(string batchIds)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<YDDyeingBatchMaster> records = await _YDDyeingBatchService.GetBatchListAsync(batchIds);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        [HttpGet]
        [Route("get-batch-details/{batchIds}")]
        public async Task<IActionResult> GetBatchDetails(string batchIds)
        {
            List<YDDyeingBatchMaster> records = await _YDDyeingBatchService.GetBatchDetails(batchIds);
            return Ok(records);
        }

        [Route("save")]
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> Save(dynamic jsonString)
        {
            YDDyeingBatchMaster model = JsonConvert.DeserializeObject<YDDyeingBatchMaster>(Convert.ToString(jsonString));
            YDDyeingBatchMaster entity;
            bool isRework = model.IsRework;
            if (model.YDDBatchID > 0)
            {
                entity = await _YDDyeingBatchService.GetAllByIDAsync(model.YDDBatchID);

                entity.BatchWeightKG = model.BatchWeightKG;
                entity.BatchQtyPcs = model.BatchQtyPcs;
                entity.MachineLoading = model.MachineLoading;
                entity.DyeingNozzleQty = model.DyeingNozzleQty;
                entity.DyeingMcCapacity = model.DyeingMcCapacity;
                entity.DMID = model.DMID;
                entity.PlanBatchStartTime = model.PlanBatchStartTime;
                entity.PlanBatchEndTime = model.PlanBatchEndTime;
                entity.Remarks = model.Remarks;
                entity.UpdatedBy = AppUser.UserCode;
                entity.DateUpdated = DateTime.Now;
                entity.EntityState = EntityState.Modified;

                entity.YDDyeingBatchMergeBatchs.SetUnchanged();
                foreach (YDDyeingBatchWithBatchMaster item in entity.YDDyeingBatchWithBatchMasters)
                {
                    item.EntityState = EntityState.Unchanged;
                }
                foreach (YDDyeingBatchItem item in entity.YDDyeingBatchItems)
                {
                    item.EntityState = EntityState.Unchanged;
                    foreach (YDDyeingBatchItemRoll roll in item.YDDyeingBatchItemRolls)
                        roll.EntityState = EntityState.Unchanged;
                }
                foreach (YDDyeingBatchRecipe item in entity.YDDyeingBatchRecipes)
                    item.EntityState = EntityState.Unchanged;

                foreach (YDDyeingBatchChildFinishingProcess item in entity.YDDyeingBatchChildFinishingProcesses)
                    item.EntityState = EntityState.Unchanged;

                //entity.YDDyeingBatchChildFinishingProcesses.SetUnchanged();

                YDDyeingBatchWithBatchMaster YDDyeingBatch;
                foreach (YDDyeingBatchWithBatchMaster item in model.YDDyeingBatchWithBatchMasters)
                {
                    YDDyeingBatch = entity.YDDyeingBatchWithBatchMasters.FirstOrDefault(x => x.YDDBBMID == item.YDDBBMID && x.YDDBatchID == item.YDDBatchID);
                    if (YDDyeingBatch == null)
                    {
                        YDDyeingBatch = item;
                        entity.YDDyeingBatchWithBatchMasters.Add(YDDyeingBatch);
                    }
                    else
                    {
                        YDDyeingBatch.BatchUseQtyKG = item.BatchUseQtyKG;
                        YDDyeingBatch.BatchUseQtyPcs = item.BatchUseQtyPcs;
                        YDDyeingBatch.EntityState = EntityState.Modified;
                    }
                }

                YDDyeingBatchItem requirement;
                YDDyeingBatchItemRoll itemRoll;
                foreach (YDDyeingBatchItem item in model.YDDyeingBatchItems)
                {
                    requirement = entity.YDDyeingBatchItems.FirstOrDefault(x => x.YDDBIID == item.YDDBIID && x.YDDBatchID == item.YDDBatchID);
                    if (requirement == null)
                    {
                        requirement = item;
                        entity.YDDyeingBatchItems.Add(requirement);
                    }
                    else
                    {
                        requirement.QtyPcs = item.QtyPcs;
                        requirement.Qty = item.Qty;
                        requirement.EntityState = EntityState.Modified;

                        foreach (YDDyeingBatchItemRoll child in item.YDDyeingBatchItemRolls)
                        {
                            itemRoll = requirement.YDDyeingBatchItemRolls.FirstOrDefault(y => y.GRollID == child.GRollID && y.YDDBatchID == item.YDDBatchID);
                            if (itemRoll == null)
                            {
                                itemRoll = child;
                                requirement.YDDyeingBatchItemRolls.Add(itemRoll);
                            }
                            else
                            {
                                itemRoll.RollQty = child.RollQty;
                                itemRoll.RollQtyPcs = child.RollQtyPcs;
                                itemRoll.EntityState = EntityState.Modified;
                            }
                        }
                    }
                }

                YDDyeingBatchRecipe recipe;
                foreach (YDDyeingBatchRecipe item in model.YDDyeingBatchRecipes)
                {
                    recipe = entity.YDDyeingBatchRecipes.FirstOrDefault(x => x.YDDBRID == item.YDDBRID && x.YDDBatchID == item.YDDBatchID);
                    if (recipe == null)
                    {
                        recipe = item;
                        entity.YDDyeingBatchRecipes.Add(recipe);
                    }
                    else
                    {
                        //recipe.Qty = item.Qty;
                        recipe.EntityState = EntityState.Modified;
                    }
                }

                #region YDDyeingBatchChildFinishingProcess

                List<YDDyeingBatchChildFinishingProcess> lstYDDyeingBatchChildFinishingProcesses = await _YDDyeingBatchService.GetFinishingProcessByYDDyeingBatchAsync(entity.YDDBatchID, entity.ColorID);
                int dx = 0;
                dx = entity.YDDyeingBatchChildFinishingProcesses.Count() == 0 ? 0 : entity.YDDyeingBatchChildFinishingProcesses.Max(x => x.YDDBCFPID);
                YDDyeingBatchChildFinishingProcess process;
                foreach (YDDyeingBatchItem item in entity.YDDyeingBatchItems)
                {
                    foreach (YDDyeingBatchChildFinishingProcess fp in lstYDDyeingBatchChildFinishingProcesses.Where(x => x.ConceptID == item.ConceptID).ToList())
                    {
                        if (item.EntityState != EntityState.Deleted)
                        {
                            process = entity.YDDyeingBatchChildFinishingProcesses.FirstOrDefault(x => x.YDDBIID == item.YDDBIID && x.ProcessID == fp.ProcessID && x.IsPreProcess == fp.IsPreProcess && x.ProcessTypeID == fp.ProcessTypeID && x.FMSID == fp.FMSID);
                            if (process == null)
                            {
                                process = new YDDyeingBatchChildFinishingProcess();
                                process.YDDBIID = item.YDDBIID;
                                process.YDDBCFPID = dx++;
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
                                entity.YDDyeingBatchChildFinishingProcesses.Add(process);
                            }
                            else
                            {
                                process.EntityState = EntityState.Modified;
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
                }

                #endregion YDDyeingBatchChildFinishingProcess

                #region YDDyeingBatchMergeBatchs
                if (isRework)
                {
                    foreach (YDDyeingBatchMergeBatch item in entity.YDDyeingBatchMergeBatchs)
                    {
                        item.BatchQty = model.BatchWeightKG;
                        item.EntityState = EntityState.Modified;
                    }
                }
                #endregion
            }
            else
            {
                entity = CommonFunction.DeepClone(model);
                entity.AddedBy = AppUser.UserCode;

                #region Add YDDyeingBatchChildFinishingProcess for each item
                List<YDDyeingBatchChildFinishingProcess> lstYDDyeingBatchChildFinishingProcess = await _YDDyeingBatchService.GetFinishingProcessAsync(entity.ConceptID, entity.ColorID);
                int dx = 0, dBIID = 0;
                YDDyeingBatchChildFinishingProcess process;
                foreach (YDDyeingBatchItem item in entity.YDDyeingBatchItems)
                {
                    item.YDDBIID = dBIID++;
                    foreach (YDDyeingBatchChildFinishingProcess fp in lstYDDyeingBatchChildFinishingProcess.Where(x => x.ConceptID == item.ConceptID).ToList())
                    {
                        process = new YDDyeingBatchChildFinishingProcess();
                        process.YDDBIID = item.YDDBIID;
                        process.YDDBCFPID = dx++;
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
                        entity.YDDyeingBatchChildFinishingProcesses.Add(process);
                    }
                }
                #endregion Add YDDyeingBatchChildFinishingProcess for each item

                #region YDDyeingBatchMergeBatchs


                List<YDDyeingBatchMaster> YDDyeingBatchs = new List<YDDyeingBatchMaster>();
                if (isRework)
                {
                    YDDyeingBatchMaster parentDBatch = new YDDyeingBatchMaster();
                    parentDBatch = CommonFunction.DeepClone(model);
                    parentDBatch.IsParentDBatch = true;
                    parentDBatch.IsRework = true;
                    parentDBatch.EntityState = EntityState.Added;
                    parentDBatch.AddedBy = AppUser.UserCode;
                    parentDBatch.DateAdded = DateTime.Now;
                    //parentDBatch.YDDyeingBatchWithBatchMasters = new List<YDDyeingBatchWithBatchMaster>();
                    //parentDBatch.YDDyeingBatchRecipes = new List<YDDyeingBatchRecipe>();
                    //parentDBatch.YDDyeingBatchItems = new List<YDDyeingBatchItem>();
                    //parentDBatch.YDDyeingBatchChildFinishingProcesses = new List<YDDyeingBatchChildFinishingProcess>();

                    YDDyeingBatchMergeBatch dbm = new YDDyeingBatchMergeBatch();
                    parentDBatch.YDDyeingBatches.ToList().ForEach(dBatch =>
                    {
                        dbm = new YDDyeingBatchMergeBatch();
                        dbm.YDDBatchID = parentDBatch.YDDBatchID;
                        dbm.MergeDBatchID = dBatch.YDDBatchID;
                        dbm.BatchQty = dBatch.BatchWeightKG;
                        parentDBatch.YDDyeingBatchMergeBatchs.Add(dbm);
                    });
                    entity = parentDBatch;
                }
                #endregion
            }
            await _YDDyeingBatchService.SaveAsync(entity);

            #region UpdateFreeConceptStatus
            string conceptIds = string.Join(",", entity.YDDyeingBatchItems.Select(x => x.ConceptID).Distinct());
            await _commonService.UpdateFreeConceptStatus(InterfaceFrom.BatchPreparation, 0, "", 0, 0, entity.CCColorID, entity.ColorID, 0, conceptIds);
            #endregion UpdateFreeConceptStatus

            return Ok();
        }
        [HttpGet]
        [Route("get-dyeing-batch/{colorName}/{conceptNo}")]
        public async Task<IActionResult> GetYDDyeingBatchs(string colorName, string conceptNo)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<YDDyeingBatchMaster> data = await _YDDyeingBatchService.GetYDDyeingBatchs(paginationInfo, colorName, conceptNo);
            return Ok(new TableResponseModel(data, paginationInfo.GridType));
        }
    }
}
