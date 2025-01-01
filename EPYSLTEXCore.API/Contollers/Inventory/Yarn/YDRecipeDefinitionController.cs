using EPYSLTEX.Core.Interfaces;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.API.Extends.Filters;
using EPYSLTEXCore.Application.Interfaces;
using EPYSLTEXCore.Application.Interfaces.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NLog;
using System.Data.Entity;
using System.Reflection;

namespace EPYSLTEXCore.API.Contollers.Inventory.Yarn
{
    [Route("api/yd-recipe-defination")]
    public class YDRecipeDefinitionController : ApiBaseController
    {
        private readonly IYDRecipeDefinitionService _service;
        private readonly ICommonHelperService _commonService;
        //private readonly IDyeingBatchService _DyeingBatchService;
        private readonly IYDRecipeRequestService _recipieRequestService;

        public YDRecipeDefinitionController(IUserService userService, IYDRecipeDefinitionService service, ICommonHelperService commonService, IYDRecipeRequestService recipieRequestService) : base(userService)
        {
            _service = service;
            _commonService = commonService;
            _recipieRequestService = recipieRequestService;
        }
        [Route("list")]
        public async Task<IActionResult> GetList(Status status)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<YDRecipeDefinitionMaster> records = await _service.GetPagedAsync(status, paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }
        [HttpGet]
        [Route("new/{id}")]
        public async Task<IActionResult> GetNew(int id)
        {
            YDRecipeDefinitionMaster data = await _service.GetNewAsync(id);
            return Ok(data);
        }
        [Route("{id}")]
        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            YDRecipeDefinitionMaster record = await _service.GetAsync(id);
            foreach (YDRecipeDefinitionChild recipeDefinitionChild in record.Childs)
            {
                recipeDefinitionChild.DefChilds = record.DefChilds.Where(x => x.YDRecipeDInfoID == recipeDefinitionChild.YDRecipeDInfoID).ToList();
            }
            return Ok(record);
        }
        [Route("save")]
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> Save(dynamic jsonString)
        {
            YDRecipeDefinitionMaster model = JsonConvert.DeserializeObject<YDRecipeDefinitionMaster>(Convert.ToString(jsonString));
            YDRecipeDefinitionMaster entity;
            if (model.IsModified)
            {
                entity = await _service.GetAllByIDAsync(model.YDRecipeID);
                entity.RecipeFor = model.RecipeFor;
                entity.BatchWeightKG = model.BatchWeightKG;
                entity.Remarks = model.Remarks;
                entity.Temperature = model.Temperature;
                entity.ProcessTime = model.ProcessTime;
                entity.UpdatedBy = AppUser.UserCode;
                entity.DateUpdated = DateTime.Now;
                entity.EntityState = EntityState.Modified;

                entity.Childs.SetUnchanged();

                foreach (YDRecipeDefinitionChild item in model.Childs)
                {
                    YDRecipeDefinitionChild existingChilds = entity.Childs.FirstOrDefault(x => x.YDRecipeChildID == item.YDRecipeChildID);
                    if (existingChilds == null)
                    {
                        existingChilds = item;
                        entity.Childs.Add(existingChilds);
                    }
                    else
                    {
                        existingChilds.ProcessID = item.ProcessID;
                        existingChilds.ParticularsID = item.ParticularsID;
                        existingChilds.RawItemID = item.RawItemID;
                        existingChilds.UnitID = item.UnitID;
                        existingChilds.Qty = item.Qty;
                        existingChilds.IsPercentage = item.IsPercentage;
                        existingChilds.Temperature = item.Temperature;
                        existingChilds.ProcessTime = item.ProcessTime;
                        existingChilds.EntityState = EntityState.Modified;
                    }
                }

                entity.Childs.Where(x => x.EntityState == EntityState.Unchanged).ToList().ForEach(y => y.EntityState = EntityState.Deleted);

                entity.RecipeDefinitionItemInfos.SetUnchanged();
                foreach (YDRecipeDefinitionItemInfo item in model.RecipeDefinitionItemInfos)
                {
                    YDRecipeDefinitionItemInfo infoChild = entity.RecipeDefinitionItemInfos.FirstOrDefault(x => x.YDRecipeItemInfoID == item.YDRecipeItemInfoID);
                    if (infoChild == null)
                    {
                        infoChild = item;
                        entity.RecipeDefinitionItemInfos.Add(infoChild);
                    }
                    else
                    {
                        infoChild.Qty = item.Qty;
                        infoChild.Pcs = item.Pcs;
                        infoChild.EntityState = EntityState.Modified;
                    }
                }
            }
            else
            {
                entity = model;
                entity.AddedBy = AppUser.UserCode;
                entity.DateAdded = DateTime.Now;
            }
            await _service.SaveAsync(entity);

            await _commonService.UpdateFreeConceptStatus(InterfaceFrom.RecipeDefinition, 0, model.ConceptNo, 0, 0, model.CCColorID, model.ColorID);

            return Ok();
        }
        [HttpPost]
        [Route("approve/{id}")]
        public async Task<IActionResult> Approve(int id)
        {
            YDRecipeDefinitionMaster entity = await _service.GetAllByIDAsync(id);

            var YDRecipeRequestMaster = await _recipieRequestService.GetAllByIDAsync(entity.YDRecipeReqMasterID);
            entity.IsApproved = true;
            entity.ApprovedBy = AppUser.UserCode;
            entity.ApprovedDate = DateTime.Now;
            entity.EntityState = EntityState.Modified;
            await _service.UpdateEntityAsync(entity);
            await _service.UpdateRecipeId(entity, YDRecipeRequestMaster.YDDBatchID);
            return Ok();
        }
        [HttpPost]
        [Route("acknowledge/{id}/{isArchive}")]
        public async Task<IActionResult> Acknowledge(int id, int isArchive)
        {
            YDRecipeDefinitionMaster entity = await _service.GetAllByIDAsync(id);
            entity.UpdatedBy = AppUser.UserCode;
            entity.DateUpdated = DateTime.Now;
            entity.Acknowledged = true;
            entity.AcknowledgedBy = AppUser.UserCode;
            entity.AcknowledgedDate = DateTime.Now;
            entity.EntityState = EntityState.Modified;
            entity.IsArchive = Convert.ToBoolean(isArchive);


            if (entity.IsBDS == 1 && entity.YDDBatchID > 0)
            {
                YDDyeingBatchMaster dyeingBatchEntity = await _service.GetAllByIDAsyncYDDBM(entity.YDDBatchID);
                List<YDBatchMaster> batchEntities = await _service.GetBatchDetails(string.Join(",", dyeingBatchEntity.YDDyeingBatchWithBatchMasters.Select(x => x.YDBatchID)));
                await _service.UpdateRecipeWithBatchAsync(entity, dyeingBatchEntity, batchEntities);
                await _service.UpdateRecipeId(entity, entity.YDDBatchID);
            }
            else
            {
                await _service.UpdateEntityAsync(entity);
                await _service.UpdateRecipeId(entity, entity.YDDBatchID);
            }

            return Ok();
        }
        [Route("list-by-buyer-compositon-color")]
        [HttpGet]
        public async Task<IActionResult> GetListByMCSubClass(String dpID, String buyer, String fabricComposition, String color)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<YDRecipeDefinitionMaster> records = await _service.GetAllApproveListForCopy(dpID, buyer, fabricComposition, color, paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }
        [Route("list-by-groupconcept-compositon-color")]
        [HttpGet]
        public async Task<IActionResult> GetListByMCTSubClass(String fabricComposition, String color, string GroupConceptNo)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<YDRecipeDefinitionMaster> records = await _service.GetConceptWiseRecipeForCopy(fabricComposition, color, GroupConceptNo, paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }
        [Route("dyeingInfo-by-recipe/{recipeReqMasterID}")]
        [HttpGet]
        public async Task<IActionResult> GetRecipeDyeingInfo(int recipeReqMasterID)
        {
            YDRecipeDefinitionMaster data = await _service.GetRecipeDyeingInfo(recipeReqMasterID);
            return Ok(data);

        }
    }
}
