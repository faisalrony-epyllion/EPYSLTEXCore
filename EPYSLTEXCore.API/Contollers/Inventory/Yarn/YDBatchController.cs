using Azure.Core;
using EPYSLTEX.Core.Interfaces;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.API.Extends.Filters;
using EPYSLTEXCore.Application.Interfaces;
using EPYSLTEXCore.Application.Interfaces.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Yarn;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data.Entity;

namespace EPYSLTEXCore.API.Contollers.Inventory.Yarn
{
    [Route("api/yd-batch")]
    public class YDBatchController : ApiBaseController
    {

        private readonly IYDBatchService _service;
        private readonly IKnittingProductionService _knittingProductionService;
        private readonly ICommonHelperService _commonService;

        public YDBatchController(IUserService userService, IYDBatchService service
            , IKnittingProductionService knittingProductionService
            , ICommonHelperService commonService) : base(userService)
        {
            _service = service;
            _knittingProductionService = knittingProductionService;
            _commonService = commonService;
        }

        [Route("list")]
        [HttpGet]
        public async Task<IActionResult> GetList(Status status)
        {
            var paginationInfo = Request.GetPaginationInfo();
            var records = await _service.GetPagedAsync(status, paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }
        [HttpGet]
        [Route("allBatchByColor/{colorID}")]
        public async Task<IActionResult> GetAllBatchByColorList(int colorID)
        {
            var paginationInfo = Request.GetPaginationInfo();
            var records = await _service.GetAllBatchByColorAsync(paginationInfo, colorID);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }
        [HttpGet]
        [Route("new/{yDBookingMasterID}/{recipeID}/{conceptNo}/{bookingID}/{isBDS}/{colorID}")]
        public async Task<IActionResult> GetNew(int yDBookingMasterID, int recipeID, string conceptNo, int bookingID, int isBDS, int colorID)
        {
            YDBatchMaster data = await _service.GetNewAsync(yDBookingMasterID, recipeID, conceptNo, bookingID, isBDS, colorID);
            foreach (YDBatchWiseRecipeChild recipeDefinitionChild in data.YDBatchWiseRecipeChilds)
                recipeDefinitionChild.YDDefChilds = data.YDDefChilds.Where(x => x.YDRecipeDInfoID == recipeDefinitionChild.RecipeDInfoID).ToList();
            return Ok(data);
        }

        [Route("{id}/{conceptNo}")]
        [HttpGet]
        public async Task<IActionResult> Get(int id, string conceptNo)
        {
            YDBatchMaster record = await _service.GetAsync(id, conceptNo);
            Guard.Against.NullObject(id, record);
            foreach (YDBatchWiseRecipeChild recipeDefinitionChild in record.YDBatchWiseRecipeChilds)
                recipeDefinitionChild.YDDefChilds = record.YDDefChilds.Where(x => x.YDRecipeDInfoID == recipeDefinitionChild.RecipeDInfoID).ToList();
            return Ok(record);
        }

        [Route("get-roll/{gRollID}")]
        [HttpGet]
        public async Task<IActionResult> GetRoll(int gRollID)
        {
            List<KnittingProduction> record = await _knittingProductionService.GetRollAsync(gRollID);
            Guard.Against.NullObject(gRollID, record);
            return Ok(record);
        }
        [Route("get-groll/{gRollID}")]
        [HttpGet]
        public async Task<IActionResult> GetgRoll(int gRollID)
        {
            List<KnittingProduction> record = await _knittingProductionService.GetGRollAsync(gRollID);
            Guard.Against.NullObject(gRollID, record);
            return Ok(record);
        }

        [Route("get-other-items/{yDBookingChildIds}/{colorId}/{yDBookingMasterID}")]
        [HttpGet]
        public async Task<IActionResult> GetOtherItems(string yDBookingChildIds, int colorId, int yDBookingMasterID)
        {
            var paginationInfo = Request.GetPaginationInfo();
            var records = await _service.GetOtherItems(paginationInfo, yDBookingChildIds, colorId, yDBookingMasterID);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        [Route("save")]
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> Save(YDBatchMaster model)
        {
            YDBatchMaster entity;
            if (model.IsModified)
            {
                entity = await _service.GetAllAsync(model.YDBatchID);

                entity.DMID = model.DMID;
                entity.MachineLoading = model.MachineLoading;
                entity.DyeingNozzleQty = model.DyeingNozzleQty;
                entity.DyeingMcCapacity = model.DyeingMcCapacity;
                entity.BatchWeightKG = model.BatchWeightKG;
                entity.BatchWeightPcs = model.BatchWeightPcs;
                entity.YDBatchDate = model.YDBatchDate;
                entity.Remarks = model.Remarks;
                entity.ExportOrderID = model.ExportOrderID;
                entity.BuyerID = model.BuyerID;
                entity.BuyerTeamID = model.BuyerID;
                entity.UpdatedBy = AppUser.UserCode;
                entity.DateUpdated = DateTime.Now;
                entity.EntityState = EntityState.Modified;
                if (model.IsApproved)
                {
                    entity.IsApproved = true;
                    entity.ApprovedBy = AppUser.UserCode;
                    entity.ApprovedDate = DateTime.Now;
                }

                foreach (YDBatchItemRequirement item in entity.YDBatchItemRequirements)
                {
                    item.EntityState = EntityState.Unchanged;
                    item.YDBatchChilds.SetUnchanged();
                }

                entity.YDBatchWiseRecipeChilds.SetUnchanged();

                model.YDBatchOtherItemRequirements.ForEach(item => model.YDBatchItemRequirements.Add(item));
                YDBatchItemRequirement requirement;
                YDBatchChild batchChild;
                foreach (YDBatchItemRequirement item in model.YDBatchItemRequirements)
                {
                    requirement = entity.YDBatchItemRequirements.FirstOrDefault(x => x.YDBookingChildID == item.YDBookingChildID);
                    if (requirement == null)
                    {
                        requirement = item;
                        requirement.EntityState = EntityState.Added;
                        entity.YDBatchItemRequirements.Add(item);
                    }
                    else
                    {
                        requirement.Pcs = item.Pcs;
                        requirement.Qty = item.Qty;
                        requirement.EntityState = EntityState.Modified;

                        foreach (YDBatchChild child in item.YDBatchChilds)
                        {
                            batchChild = requirement.YDBatchChilds.FirstOrDefault(y => y.GRollID == child.GRollID);
                            if (batchChild == null)
                            {
                                batchChild = child;
                                batchChild.EntityState = EntityState.Added;
                                requirement.YDBatchChilds.Add(batchChild);
                            }
                            else
                            {
                                batchChild.EntityState = EntityState.Modified;
                            }
                        }
                    }
                }

                YDBatchWiseRecipeChild recipe;
                foreach (YDBatchWiseRecipeChild item in model.YDBatchWiseRecipeChilds)
                {
                    recipe = entity.YDBatchWiseRecipeChilds.FirstOrDefault(x => x.YDRecipeChildID == item.YDRecipeChildID);
                    if (recipe == null)
                    {
                        recipe = item;
                        recipe.EntityState = EntityState.Added;
                        entity.YDBatchWiseRecipeChilds.Add(recipe);
                    }
                    else
                    {
                        recipe.Qty = item.Qty;
                        recipe.EntityState = EntityState.Modified;
                    }
                }

                await _service.SaveAsync(entity); //knittingProductions
            }
            else
            {
                model.YDBatchOtherItemRequirements.ForEach(item => model.YDBatchItemRequirements.Add(item));
                entity = model;
                YDBatchMaster ydbm = await _service.GetYDBatchNo(entity.YDBookingMasterID, entity.ColorID);
                entity.YDBatchNo = ydbm.YDBatchNo;
                entity.AddedBy = AppUser.UserCode;
                entity.EntityState = EntityState.Added;
                await _service.SaveAsync(entity); //list
            }
            if (entity.YDBatchID > 0)
            {
                //await _service.UpdateBDSTNA_YDBatchPreparationPlanAsync(entity.YDBatchID);
            }
            return Ok();
        }

        [Route("save-kProd")]
        [HttpPost]
        public async Task<IActionResult> SaveKProd(List<KnittingProduction> models)
        {
            List<KnittingProduction> knittingProductions = new List<KnittingProduction>();

            KnittingProduction entity = await _knittingProductionService.GetDetailsAsync(models.First().GRollID);
            entity.InActive = true;
            entity.InActiveBy = AppUser.UserCode;
            entity.InActiveDate = DateTime.Now;
            entity.InActiveReason = "Split Roll";
            entity.EntityState = EntityState.Modified;
            knittingProductions.Add(entity);

            if (models.First().IsSaveDyeingBatchItemRoll && models.First().DBIRollID > 0)
            {
                DyeingBatchItemRoll entityDBRollItem = await _knittingProductionService.GetDyingBatchItemRoll(models.First().DBIRollID);
                if (entityDBRollItem != null)
                {
                    models.ForEach(x =>
                    {
                        x.DyeingBatchRollItem = entityDBRollItem;
                    });
                }
            }


            foreach (KnittingProduction model in models)
            {
                KnittingProduction knitProd = new KnittingProduction
                {
                    KJobCardMasterID = entity.KJobCardMasterID,
                    ProductionDate = entity.ProductionDate,
                    ConceptID = entity.ConceptID,
                    BookingID = entity.BookingID,
                    ExportOrderID = entity.ExportOrderID,
                    BuyerID = entity.BuyerID,
                    //BatchID = entity.BatchID, 
                    BuyerTeamID = entity.BuyerTeamID,
                    OperatorID = entity.OperatorID,
                    ShiftID = entity.ShiftID,
                    RollSeqNo = entity.RollSeqNo,
                    RollNo = model.RollNo,
                    RollQty = model.RollQty,
                    RollQtyPcs = model.RollQtyPcs,
                    ProductionGSM = entity.ProductionGSM,
                    ProductionWidth = entity.ProductionWidth,
                    FirstRollCheck = entity.FirstRollCheck,
                    FirstRollCheckBy = entity.FirstRollCheckBy,
                    FirstRollCheckDate = entity.FirstRollCheckDate,
                    FirstRollPass = entity.FirstRollPass,
                    SendforQC = entity.SendforQC,
                    SendQCDate = entity.SendQCDate,
                    SendQCBy = entity.SendQCBy,
                    RollLength = entity.RollLength,
                    QCComplete = entity.QCComplete,
                    QCCompleteDate = entity.QCCompleteDate,
                    QCCompleteBy = entity.QCCompleteBy,
                    QCWidth = entity.QCWidth,
                    QCGSM = entity.QCGSM,
                    QCPass = entity.QCPass,
                    QCPassQty = entity.QCPassQty,
                    ParentGRollID = entity.GRollID,
                    ProdComplete = entity.ProdComplete,
                    ProdQty = entity.ProdQty,
                    AddedBy = AppUser.UserCode,
                    DateAdded = DateTime.Now,
                    Hole = entity.Hole,
                    Loop = entity.Loop,
                    SetOff = entity.SetOff,
                    LycraOut = entity.LycraOut,
                    LycraDrop = entity.LycraDrop,
                    LycraDrop1 = entity.LycraDrop1,
                    LycraDrop2 = entity.LycraDrop2,
                    LycraDrop3 = entity.LycraDrop3,
                    OilSpot = entity.OilSpot,
                    OilSpot1 = entity.OilSpot1,
                    OilSpot2 = entity.OilSpot2,
                    OilSpot3 = entity.OilSpot3,
                    Slub = entity.Slub,
                    FlyingDust = entity.FlyingDust,
                    MissingYarn = entity.MissingYarn,
                    Knot = entity.Knot,
                    DropStitch = entity.DropStitch,
                    DropStitch1 = entity.DropStitch1,
                    DropStitch2 = entity.DropStitch2,
                    DropStitch3 = entity.DropStitch3,
                    YarnContra = entity.YarnContra,
                    NeddleBreakage = entity.NeddleBreakage,
                    Defected = entity.Defected,
                    WrongDesign = entity.WrongDesign,
                    Patta = entity.Patta,
                    ShinkerMark = entity.ShinkerMark,
                    NeddleMark = entity.NeddleMark,
                    EdgeMark = entity.EdgeMark,
                    WheelFree = entity.WheelFree,
                    CountMix = entity.CountMix,
                    ThickAndThin = entity.ThickAndThin,
                    LineStar = entity.LineStar,
                    QCOthers = entity.QCOthers,
                    Comment = entity.Comment,
                    CalculateValue = entity.CalculateValue,
                    CalculateQCStatus = entity.CalculateQCStatus,
                    Grade = entity.Grade,
                    //Hold = entity.Hold,
                    QCBy = entity.QCBy,
                    QCShiftID = entity.QCShiftID,
                    EntityState = EntityState.Added
                };
                if ((knitProd.BatchID == 0 || knitProd.BatchID == null) && model.BatchID > 0) //Need for Sample delivery Challan
                {
                    knitProd.BatchID = model.BatchID;
                }
                knitProd.IsSaveDyeingBatchItemRoll = model.IsSaveDyeingBatchItemRoll;
                knitProd.DyeingBatchRollItem = model.DyeingBatchRollItem;
                knittingProductions.Add(knitProd);
            }

            await _knittingProductionService.SaveAsync(knittingProductions);

            List<KnittingProduction> list = await _knittingProductionService.GetDetailsByParentGRollIdAsync(entity.GRollID);
            knittingProductions = await _knittingProductionService.GetKProductionsByConcept(entity.ConceptNo);

            if (models.Count() > 0) await _commonService.UpdateFreeConceptStatus(InterfaceFrom.KnittingProduction, models.First().ConceptID, "", models.First().BookingID);

            return Ok(new { newSplitedList = list, totalUpdatedList = knittingProductions });
        }

        [Route("kProduction-by-concept/{conceptId}")]
        [HttpGet]
        public async Task<IActionResult> GetKProductionsByConcept(int conceptId)
        {
            return Ok(await _knittingProductionService.GetKProductionsByConceptId(conceptId));
        }
    }
}
