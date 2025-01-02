using EPYSLTEX.Core.Interfaces;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEX.Infrastructure.Services;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.API.Extends.Filters;
using EPYSLTEXCore.Application.Interfaces.RND;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Yarn;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data.Entity;

namespace EPYSLTEXCore.API.Contollers.RND
{
    [Route("api/rnd-recipe-defination")]
    public class RecipeDefinitionController : ApiBaseController
    {
        private readonly IRecipeDefinitionService _service;
        private readonly ICommonHelperService _commonService;
        private readonly IDyeingBatchService _DyeingBatchService;
        private readonly IRecipieRequestService _recipieRequestService;

        public RecipeDefinitionController(IRecipeDefinitionService service, ICommonHelperService commonService, IDyeingBatchService DyeingBatchService, 
            IRecipieRequestService recipieRequestService, IUserService userService) : base(userService)
        {
            _service = service;
            _commonService = commonService;
            _DyeingBatchService = DyeingBatchService;
            _recipieRequestService = recipieRequestService;
        }

        [Route("list")]
        public async Task<IActionResult> GetList(Status status)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<RecipeDefinitionMaster> records = await _service.GetPagedAsync(status, paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        [HttpGet]
        [Route("new/{id}")]
        public async Task<IActionResult> GetNew(int id)
        {
            RecipeDefinitionMaster data = await _service.GetNewAsync(id);
            return Ok(data);
        }

        [Route("{id}")]
        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            RecipeDefinitionMaster record = await _service.GetAsync(id);
            foreach (RecipeDefinitionChild recipeDefinitionChild in record.Childs)
            {
                recipeDefinitionChild.DefChilds = record.DefChilds.Where(x => x.RecipeDInfoID == recipeDefinitionChild.RecipeDInfoID).ToList();
            }
            return Ok(record);
        }

        [Route("save")]
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> Save(dynamic jsonString)
        {
            RecipeDefinitionMaster model = JsonConvert.DeserializeObject<RecipeDefinitionMaster>(Convert.ToString(jsonString));

            RecipeDefinitionMaster entity;
            if (model.IsModified)
            {
                entity = await _service.GetAllByIDAsync(model.RecipeID);
                entity.RecipeFor = model.RecipeFor;
                entity.BatchWeightKG = model.BatchWeightKG;
                entity.Remarks = model.Remarks;
                entity.Temperature = model.Temperature;
                entity.ProcessTime = model.ProcessTime;
                entity.UpdatedBy = AppUser.UserCode; 
                entity.DateUpdated = DateTime.Now;
                entity.EntityState = EntityState.Modified;

                entity.Childs.SetUnchanged();

                foreach (RecipeDefinitionChild item in model.Childs)
                {
                    RecipeDefinitionChild existingChilds = entity.Childs.FirstOrDefault(x => x.RecipeChildID == item.RecipeChildID);
                    if (existingChilds == null)
                    {
                        existingChilds = item;
                        entity.Childs.Add(existingChilds);
                    }
                    else
                    {
                        existingChilds.ProcessId = item.ProcessId;
                        existingChilds.ParticularsId = item.ParticularsId;
                        existingChilds.RawItemId = item.RawItemId;
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
                foreach (RecipeDefinitionItemInfo item in model.RecipeDefinitionItemInfos)
                {
                    RecipeDefinitionItemInfo infoChild = entity.RecipeDefinitionItemInfos.FirstOrDefault(x => x.RecipeItemInfoID == item.RecipeItemInfoID);
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
            RecipeDefinitionMaster entity = await _service.GetAllByIDAsync(id);

            var RecipeRequestMaster = await _recipieRequestService.GetAllByIDAsync(entity.RecipeReqMasterID);
            entity.IsApproved = true;
            entity.ApprovedBy = AppUser.UserCode;
            entity.ApprovedDate = DateTime.Now;
            entity.EntityState = EntityState.Modified;
            await _service.UpdateEntityAsync(entity);
            await _service.UpdateRecipeId(entity, RecipeRequestMaster.DBatchID);
            return Ok();
        }

        [HttpPost]
        [Route("acknowledge/{id}/{isArchive}")]
        public async Task<IActionResult> Acknowledge(int id, int isArchive)
        {
            RecipeDefinitionMaster entity = await _service.GetAllByIDAsync(id);
            entity.UpdatedBy = AppUser.UserCode;
            entity.DateUpdated = DateTime.Now;
            entity.Acknowledged = true;
            entity.AcknowledgedBy = AppUser.UserCode;
            entity.AcknowledgedDate = DateTime.Now;
            entity.EntityState = EntityState.Modified;
            entity.IsArchive = Convert.ToBoolean(isArchive);


            if (entity.IsBDS == 1 && entity.DBatchID > 0)
            {
                DyeingBatchMaster dyeingBatchEntity = await _DyeingBatchService.GetAllByIDAsync(entity.DBatchID);
                List<BatchMaster> batchEntities = await _service.GetBatchDetails(string.Join(",", dyeingBatchEntity.DyeingBatchWithBatchMasters.Select(x => x.BatchID)));
                await _service.UpdateRecipeWithBatchAsync(entity, dyeingBatchEntity, batchEntities);
                await _service.UpdateRecipeId(entity, entity.DBatchID);
            }
            else
            {
                await _service.UpdateEntityAsync(entity);
                await _service.UpdateRecipeId(entity, entity.DBatchID);
            }

            return Ok();
        }

        [Route("list-by-buyer-compositon-color")]
        [HttpGet]
        public async Task<IActionResult> GetListByMCSubClass(String dpID, String buyer, String fabricComposition, String color)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<RecipeDefinitionMaster> records = await _service.GetAllApproveListForCopy(dpID, buyer, fabricComposition, color, paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }
        [Route("list-by-groupconcept-compositon-color")]
        [HttpGet]
        public async Task<IActionResult> GetListByMCTSubClass(String fabricComposition, String color, string GroupConceptNo)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<RecipeDefinitionMaster> records = await _service.GetConceptWiseRecipeForCopy(fabricComposition, color, GroupConceptNo, paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        [Route("dyeingInfo-by-recipe/{recipeReqMasterID}")]
        [HttpGet]
        public async Task<IActionResult> GetRecipeDyeingInfo(int recipeReqMasterID)
        {
            RecipeDefinitionMaster data = await _service.GetRecipeDyeingInfo(recipeReqMasterID);
            return Ok(data);

        }

    }
}
