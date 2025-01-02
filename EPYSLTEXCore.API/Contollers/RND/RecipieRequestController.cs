using EPYSLTEX.Core.Interfaces;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEXCore.API.Contollers.APIBaseController;
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
    [Route("api/recipe-request")]
    public class RecipieRequestController : ApiBaseController
    {
        private readonly IRecipieRequestService _recipieRequestService;
        private readonly IRecipeDefinitionService _serviceRecipeDefinition;
        //private readonly IEmailService _emailService;
        private readonly ICommonHelperService _commonService;
        private readonly IDyeingBatchService _DyeingBatchService;
        private readonly IBatchService _service;

        public RecipieRequestController(IRecipieRequestService recipieRequestService,
            //IEmailService emailService,
            ICommonHelperService commonService,
            IRecipeDefinitionService serviceRecipeDefinition,
            IDyeingBatchService DyeingBatchService,
            IBatchService service, IUserService userService) : base(userService)
        {
            _recipieRequestService = recipieRequestService;
            _commonService = commonService;
            //_emailService = emailService;
            _serviceRecipeDefinition = serviceRecipeDefinition;
            _DyeingBatchService = DyeingBatchService;
            _service = service;
        }

        [Route("list")]
        public async Task<IActionResult> GetList(Status status)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<RecipeRequestMaster> records = await _recipieRequestService.GetPagedAsync(status, paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        [HttpGet]
        [Route("new/{ccColorId}/{grpConceptNo}/{isBDS}/{isRework}/{recipeReqNo}/{DBatchID}")]
        public async Task<IActionResult> GetNew(int ccColorId, string grpConceptNo, int isBDS, bool isRework, string recipeReqNo, int DBatchID = 0)
        {
            RecipeRequestMaster data = await _recipieRequestService.GetNewAsync(ccColorId, grpConceptNo, isBDS, DBatchID);
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
            RecipeRequestMaster data = await _recipieRequestService.GetAsync(id, groupConceptNo);
            return Ok(data);
        }

        [Route("save")]
        [HttpPost]
        public async Task<IActionResult> Save(dynamic jsonString)
        {
            RecipeRequestMaster model = JsonConvert.DeserializeObject<RecipeRequestMaster>(Convert.ToString(jsonString));

                RecipeRequestMaster entity;
            bool isRework = model.IsRework;
            string recipeFor = string.Join(",", model.RecipeDefinitionDyeingInfos.Where(x => x.RecipeOn && x.FiberPart != "Empty").Select(x => x.FiberPart).Distinct());
            if (model.IsModified)
            {
                entity = await _recipieRequestService.GetAllByIDAsync(model.RecipeReqMasterID);

                entity.DPID = model.DPID;
                entity.DPProcessInfo = model.DPProcessInfo;
                entity.DBatchID = model.DBatchID;
                entity.Remarks = model.Remarks;
                entity.Approved = model.Approved;
                entity.ConceptNo = model.ConceptNo;
                entity.UpdatedBy = AppUser.UserCode;
                entity.DateUpdated = DateTime.Now;
                entity.EntityState = EntityState.Modified;
                entity.RecipeFor = recipeFor;

                entity.RecipeRequestChilds.SetUnchanged();
                foreach (RecipeRequestChild item in model.RecipeRequestChilds)
                {
                    RecipeRequestChild existingChild = entity.RecipeRequestChilds.FirstOrDefault(x => x.RecipeReqChildID == item.RecipeReqChildID && x.RecipeReqMasterID == item.RecipeReqMasterID);
                    if (existingChild == null)
                    {
                        existingChild = item;
                        entity.RecipeRequestChilds.Add(existingChild);
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
                entity.RecipeRequestChilds.Where(x => x.EntityState == EntityState.Unchanged).ToList().ForEach(y => y.EntityState = EntityState.Deleted);

                entity.RecipeDefinitionDyeingInfos.SetUnchanged();
                foreach (RecipeDefinitionDyeingInfo item in model.RecipeDefinitionDyeingInfos)
                {
                    RecipeDefinitionDyeingInfo existingChild = entity.RecipeDefinitionDyeingInfos.FirstOrDefault(x => x.RecipeDInfoID == item.RecipeDInfoID && x.RecipeReqMasterID == item.RecipeReqMasterID);
                    if (existingChild == null)
                    {
                        existingChild = item;
                        entity.RecipeDefinitionDyeingInfos.Add(existingChild);
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
                entity.RecipeDefinitionDyeingInfos.Where(x => x.EntityState == EntityState.Unchanged).ToList().ForEach(y => y.EntityState = EntityState.Deleted);
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
                    entity.RecipeDefinitions = await _serviceRecipeDefinition.GetByRecipeReqNo(model.RecipeReqNo);
                    entity.RecipeDefinitions.ForEach(x =>
                    {
                        x.IsActive = false;
                        x.UpdatedBy = AppUser.UserCode;
                        x.DateUpdated = DateTime.Now;
                        x.EntityState = EntityState.Modified;
                    });
                }
            }
            entity.IsRework = isRework;
            await _recipieRequestService.SaveAsync(entity);
            await _commonService.UpdateFreeConceptStatus(InterfaceFrom.RecipeRequest, model.ConceptID, model.ConceptNo, 0, model.IsBDS, model.CCColorID, model.ColorID);
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
                //if (entity.UpdatedBy == null)
                //{
                //    await EmailSend(entity.AddedBy, entity.RecipeReqNo, sConceptNo);
                //}
                //else
                //{
                //    await EmailSend((int)entity.UpdatedBy, entity.RecipeReqNo, sConceptNo);
                //}
            }
            return Ok();
        }
        [Route("batchRecipeUpdate")]
        [HttpPost]
        public async Task<IActionResult> BatchRecipeUpdate(RecipeRequestMaster model)
        {

            var entity = await _DyeingBatchService.GetAllByIDAsync(model.DBatchID);
            entity.RecipeID = model.RecipeID;
            entity.EntityState = EntityState.Modified;
            await _DyeingBatchService.SaveAsyncRecipeCopy(entity);

            var dyeingBatchMaster = await _DyeingBatchService.GetAllByIDAsync(model.DBatchID);
            BatchMaster BM = await _service.GetAllAsync(dyeingBatchMaster.DyeingBatchItems[0].BatchID);
            BM.RecipeID = model.RecipeID;
            BM.EntityState = EntityState.Modified;
            await _service.SaveAsyncRecipeCopy(BM);
            return Ok();
        }

        [Route("revise")]
        [HttpPost]
        public async Task<IActionResult> Revise(dynamic jsonString)
        {
            RecipeRequestMaster model = JsonConvert.DeserializeObject<RecipeRequestMaster>(Convert.ToString(jsonString));
            RecipeRequestMaster entity;
            bool isRework = model.IsRework;
            string recipeFor = string.Join(",", model.RecipeDefinitionDyeingInfos.Where(x => x.RecipeOn && x.FiberPart != "Empty").Select(x => x.FiberPart).Distinct());

            entity = await _recipieRequestService.GetAllByIDAsync(model.RecipeReqMasterID);

            entity.DPID = model.DPID;
            entity.DPProcessInfo = model.DPProcessInfo;
            entity.DBatchID = model.DBatchID;
            entity.Remarks = model.Remarks;
            entity.ConceptNo = model.ConceptNo;


            entity.RevisionNo = entity.RevisionNo + 1;
            entity.RevisionDate = DateTime.Now;
            entity.RevisionBy = AppUser.UserCode;
            entity.RevisionReason = model.RevisionReason;
            entity.UnAcknowledge = false;


            entity.EntityState = EntityState.Modified;
            entity.RecipeFor = recipeFor;

            entity.RecipeRequestChilds.SetUnchanged();
            foreach (RecipeRequestChild item in model.RecipeRequestChilds)
            {
                RecipeRequestChild existingChild = entity.RecipeRequestChilds.FirstOrDefault(x => x.RecipeReqChildID == item.RecipeReqChildID && x.RecipeReqMasterID == item.RecipeReqMasterID);
                if (existingChild == null)
                {
                    existingChild = item;
                    entity.RecipeRequestChilds.Add(existingChild);
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
            entity.RecipeRequestChilds.Where(x => x.EntityState == EntityState.Unchanged).ToList().ForEach(y => y.EntityState = EntityState.Deleted);

            entity.RecipeDefinitionDyeingInfos.SetUnchanged();
            foreach (RecipeDefinitionDyeingInfo item in model.RecipeDefinitionDyeingInfos)
            {
                RecipeDefinitionDyeingInfo existingChild = entity.RecipeDefinitionDyeingInfos.FirstOrDefault(x => x.RecipeDInfoID == item.RecipeDInfoID && x.RecipeReqMasterID == item.RecipeReqMasterID);
                if (existingChild == null)
                {
                    existingChild = item;
                    entity.RecipeDefinitionDyeingInfos.Add(existingChild);
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
            entity.RecipeDefinitionDyeingInfos.Where(x => x.EntityState == EntityState.Unchanged).ToList().ForEach(y => y.EntityState = EntityState.Deleted);

            entity.IsRework = isRework;
            await _recipieRequestService.RevisionAsync(entity);
            await _commonService.UpdateFreeConceptStatus(InterfaceFrom.RecipeRequest, model.ConceptID, model.ConceptNo, 0, model.IsBDS, model.CCColorID, model.ColorID);
            return Ok();
        }

        [Route("acknowledge")]
        [HttpPost]
        public async Task<IActionResult> Acknowledge(dynamic jsonString)
        {
            RecipeRequestMaster model = JsonConvert.DeserializeObject<RecipeRequestMaster>(Convert.ToString(jsonString));
            RecipeRequestMaster entity;
            entity = await _recipieRequestService.GetAllByIDAsync(model.RecipeReqMasterID);

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

            entity.RecipeRequestChilds.SetUnchanged();
            entity.RecipeDefinitionDyeingInfos.SetUnchanged();

            await _recipieRequestService.UpdateEntityAsync(entity);
            return Ok();
        }

        [HttpGet]
        [Route("get-concept-item/{conceptNo}/{colorID}/{isBDS}")]
        public async Task<IActionResult> GetItems(string conceptNo, int colorID, int isBDS)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<RecipeRequestChild> data = await _recipieRequestService.GetItems(conceptNo, colorID, isBDS);
            return Ok(new TableResponseModel(data, paginationInfo.GridType));
        }


        //#region :: Send Mail
        //public async Task<IActionResult> EmailSend(int AddedBy, string RecipeReqNo, string sConceptNo)
        //{
        //    try
        //    {
        //        EPYSL.Encription.Encryption objEncription = new EPYSL.Encription.Encryption();

        //        var isgDTO = await _emailService.GetItemSubGroupMailSetupAsync("Yarn New", "Recipe Request");
        //        var uInfo = await _emailService.GetUserEmailInfoAsync(AddedBy);
        //        //var attachment = await _reportingService.GetPdfByte(990, UserId, entity.BookingNo);
        //        var attachment = new byte[] { 0 };
        //        String fromMailID = "";
        //        String toMailID = "";
        //        String ccMailID = "";
        //        String bccMailID = "";
        //        String password = "";

        //        if (Request.Headers.Host.ToUpper() == "texerp.epylliongroup.com".ToUpper())
        //        {
        //            fromMailID = AppUser.Email;
        //            password = objEncription.Decrypt(AppUser.EmailPassword, AppUser.UserName);
        //            toMailID = uInfo.Email.IsNullOrEmpty() ? AppUser.Email : uInfo.Email;

        //            if (ccMailID.IsNullOrEmpty())
        //                ccMailID = AppUser.Email;
        //            else
        //            {
        //                ccMailID = ccMailID + ";" + AppUser.Email;
        //            }

        //            if (isgDTO.IsNotNull())
        //            {
        //                toMailID += isgDTO.ToMailID.IsNullOrEmpty() ? "" : ";" + isgDTO.ToMailID;
        //                ccMailID += isgDTO.CCMailID.IsNullOrEmpty() ? "" : ";" + isgDTO.CCMailID;
        //                bccMailID = isgDTO.BCCMailID;
        //            }
        //        }
        //        else
        //        {
        //            fromMailID = "erpnoreply@epylliongroup.com";
        //            password = "Ugr7jT5d";
        //            toMailID = "anupam@epylliongroup.com;abdussalam@epylliongroup.com;litonekl@epylliongroup.com";
        //            //toMailID = "azizul.sailor@epylliongroup.com";
        //            ccMailID = "";
        //            bccMailID = "";
        //        }

        //        string subject = $"Recipe Request : Recipe No {RecipeReqNo}) ";

        //        string messageBody =
        //                $"Dear Sir,</br>" +
        //                $"Please proceed the color formulation and provide recipe information to fill the requirement described" +
        //                $" in the attached file of {sConceptNo}. </br></br></br>" +
        //                $"If you have any query please feel free to communicate with concerns. </br>" +
        //                $"</br></br>" +
        //                $"Thanks & Best Regards," +
        //                $"</br>" +
        //                $"{AppUser.EmployeeName}</br>" +
        //                $"{AppUser.DepertmentDescription}</br></br>" +
        //                $"This is system generated mail.";

        //        String fileName = String.Empty;
        //        //fileName = $"{entity.BookingNo}.pdf";
        //        await _emailService.SendAutoEmailAsync(fromMailID, password, toMailID, ccMailID, bccMailID, subject, messageBody, fileName, attachment);
        //    }
        //    catch
        //    {

        //    }
        //    return Ok();
        //}
        //#endregion
    }
}