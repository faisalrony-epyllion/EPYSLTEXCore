using EPYSLTEX.Core.Interfaces;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEXCore.API.Contollers.APIBaseController;
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
    [Route("api/yd-recipe-request")]
    public class YDRecipeRequestController : ApiBaseController
    {
        private readonly IYDRecipeRequestService _service;
        private readonly ICommonHelperService _commonService;
        private readonly IYDRecipeDefinitionService _serviceRecipeDefinition;
        public YDRecipeRequestController(IUserService userService, IYDRecipeRequestService service, ICommonHelperService commonService, IYDRecipeDefinitionService serviceRecipeDefinition) : base(userService)
        {
            _service = service;
            _commonService = commonService;
            _serviceRecipeDefinition = serviceRecipeDefinition;
        }

        [Route("list")]
        public async Task<IActionResult> GetList(Status status)
        {
            var paginationInfo = Request.GetPaginationInfo();
            var records = await _service.GetPagedAsync(status, paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        [HttpGet]
        [Route("new/{ccColorId}/{grpConceptNo}/{isBDS}/{isRework}/{recipeReqNo}/{YDBookingChildID}/{YDDBatchID}")]
        public async Task<IActionResult> GetNew(int ccColorId, string grpConceptNo, int isBDS, bool isRework, string recipeReqNo, int YDBookingChildID, int YDDBatchID = 0)
        {
            YDRecipeRequestMaster data = await _service.GetNewAsync(ccColorId, YDBookingChildID, grpConceptNo, isBDS, YDDBatchID);
            data.YDBookingChildID = YDBookingChildID;
            if (isRework)
            {
                data.RecipeReqNo = recipeReqNo;
            }
            return Ok(data);
        }
        [HttpGet]
        [Route("{id}/{groupConceptNo}")]
        public async Task<IActionResult> GetAsync(int id, string groupConceptNo)
        {
            YDRecipeRequestMaster data = await _service.GetAsync(id, groupConceptNo);
            return Ok(data);
        }
        [HttpGet]
        [Route("get-concept-item/{conceptNo}/{colorID}/{isBDS}")]
        public async Task<IActionResult> GetItems(string conceptNo, int colorID, int isBDS)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<YDRecipeRequestChild> data = await _service.GetItems(conceptNo, colorID, isBDS);
            return Ok(new TableResponseModel(data, paginationInfo.GridType));
        }
        [Route("save")]
        [HttpPost]
        public async Task<IActionResult> Save(YDRecipeRequestMaster model)
        {
            YDRecipeRequestMaster entity;
            bool isRework = model.IsRework;
            string recipeFor = string.Join(",", model.YDRecipeDefinitionDyeingInfos.Where(x => x.RecipeOn && x.FiberPart != "Empty").Select(x => x.FiberPart).Distinct());
            if (model.IsModified)
            {
                entity = await _service.GetAllByIDAsync(model.YDRecipeReqMasterID);

                entity.DPID = model.DPID;
                entity.DPProcessInfo = model.DPProcessInfo;
                entity.YDDBatchID = model.YDDBatchID;
                entity.Remarks = model.Remarks;
                entity.Approved = model.Approved;
                entity.ConceptNo = model.ConceptNo;
                entity.UpdatedBy = AppUser.UserCode;
                entity.DateUpdated = DateTime.Now;
                entity.EntityState = EntityState.Modified;
                entity.RecipeFor = recipeFor;

                entity.YDRecipeRequestChilds.SetUnchanged();
                foreach (YDRecipeRequestChild item in model.YDRecipeRequestChilds)
                {
                    YDRecipeRequestChild existingChild = entity.YDRecipeRequestChilds.FirstOrDefault(x => x.YDRecipeReqChildID == item.YDRecipeReqChildID && x.YDRecipeReqMasterID == item.YDRecipeReqMasterID);
                    if (existingChild == null)
                    {
                        existingChild = item;
                        entity.YDRecipeRequestChilds.Add(existingChild);
                    }
                    else
                    {
                        existingChild.ConceptID = item.ConceptID;
                        existingChild.BookingID = item.BookingID;
                        existingChild.SubGroupID = item.SubGroupID;
                        existingChild.ItemMasterID = item.ItemMasterID;
                        existingChild.RecipeOn = item.RecipeOn;

                        existingChild.CCColorID = entity.CCColorID;
                        existingChild.EntityState = EntityState.Modified;
                    }
                }
                entity.YDRecipeRequestChilds.Where(x => x.EntityState == EntityState.Unchanged).ToList().ForEach(y => y.EntityState = EntityState.Deleted);

                entity.YDRecipeDefinitionDyeingInfos.SetUnchanged();
                foreach (YDRecipeDefinitionDyeingInfo item in model.YDRecipeDefinitionDyeingInfos)
                {
                    YDRecipeDefinitionDyeingInfo existingChild = entity.YDRecipeDefinitionDyeingInfos.FirstOrDefault(x => x.YDRecipeDInfoID == item.YDRecipeDInfoID && x.YDRecipeReqMasterID == item.YDRecipeReqMasterID);
                    if (existingChild == null)
                    {
                        existingChild = item;
                        entity.YDRecipeDefinitionDyeingInfos.Add(existingChild);
                    }
                    else
                    {
                        existingChild.FiberPartID = item.FiberPartID;
                        existingChild.ColorID = item.ColorID;
                        existingChild.RecipeOn = item.RecipeOn;
                        existingChild.ColorCode = item.ColorCode;
                        existingChild.EntityState = EntityState.Modified;
                    }
                }
                entity.YDRecipeDefinitionDyeingInfos.Where(x => x.EntityState == EntityState.Unchanged).ToList().ForEach(y => y.EntityState = EntityState.Deleted);
            }
            else
            {
                entity = model;
                entity.AddedBy = AppUser.UserCode;
                entity.RecipeReqDate = DateTime.Now;
                entity.DateAdded = DateTime.Now;
                entity.RecipeFor = recipeFor;

                if (isRework)
                {
                    entity.RecipeReqNo = model.RecipeReqNo;
                    entity.YDRecipeDefinitions = await _serviceRecipeDefinition.GetByRecipeReqNo(model.RecipeReqNo);
                    entity.YDRecipeDefinitions.ForEach(x =>
                    {
                        x.IsActive = false;
                        x.UpdatedBy = AppUser.UserCode;
                        x.DateUpdated = DateTime.Now;
                        x.EntityState = EntityState.Modified;
                    });
                }
            }
            entity.IsRework = isRework;
            await _service.SaveAsync(entity);
            await _commonService.UpdateFreeConceptStatus(InterfaceFrom.YDRecipeRequest, model.ConceptID, model.ConceptNo, 0, model.IsBDS, model.CCColorID, model.ColorID);
            if (entity.Approved)
            {
                string sConceptNo = "";

                if (entity.IsBDS == 0)
                {
                    sConceptNo = "Concep No: " + entity.ConceptNo;
                }
                else
                {
                    sConceptNo = "Sample No: " + entity.ConceptNo;
                }
                if (entity.UpdatedBy == null)
                {
                    //await EmailSend(entity.AddedBy, entity.RecipeReqNo, sConceptNo);
                }
                else
                {
                    //await EmailSend((int)entity.UpdatedBy, entity.RecipeReqNo, sConceptNo);
                }
            }
            return Ok();
        }
        [Route("acknowledge")]
        [HttpPost]
        public async Task<IActionResult> Acknowledge(YDRecipeRequestMaster model)
        {
            YDRecipeRequestMaster entity;
            entity = await _service.GetAllByIDAsync(model.YDRecipeReqMasterID);

            if (model.Acknowledge)
            {
                entity.Acknowledge = true;
                entity.AcknowledgeBy = AppUser.UserCode;
                entity.AcknowledgeDate = DateTime.Now;
                entity.EntityState = EntityState.Modified;
            }
            else
            {
                entity.UnAcknowledge = true;
                entity.UnAcknowledgeBy = AppUser.UserCode;
                entity.UnAcknowledgeDate = DateTime.Now;
                entity.UnAcknowledgeReason = model.UnAcknowledgeReason;
                entity.EntityState = EntityState.Modified;
            }

            entity.YDRecipeRequestChilds.SetUnchanged();
            entity.YDRecipeDefinitionDyeingInfos.SetUnchanged();

            await _service.UpdateEntityAsync(entity);
            return Ok();
        }
    }
}
