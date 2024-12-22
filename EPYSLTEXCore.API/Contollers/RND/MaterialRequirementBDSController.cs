using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.API.Extends.Filters;
using EPYSLTEXCore.Application.Interfaces;
using EPYSLTEXCore.Application.Interfaces.Knitting;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Booking;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data.Entity;


namespace EPYSLTEXCore.API.Contollers.RND
{
    [Route("api/mr-bds")]
    public class MaterialRequirementBDSController : ApiBaseController
    {
        private readonly IItemMasterService<FreeConceptMRChild> _itemMasterRepository;
        //private readonly IGmtEfRepository<ItemSegmentName> _itemSegmentNameRepository;
        //private readonly IGmtEfRepository<ItemSegmentValue> _itemSegmentValueRepository;
        private readonly IMaterialRequirementBDSService _service;
        private readonly IYarnPRService _servicePR;
        //private readonly IMapper _mapper;

        public MaterialRequirementBDSController(IItemMasterService<FreeConceptMRChild> itemMasterRepository
            //, IGmtEfRepository<ItemSegmentName> itemSegmentNameRepository
            //, IGmtEfRepository<ItemSegmentValue> itemSegmentValueRepository
            , IMaterialRequirementBDSService service
            ,IUserService userService
            , IYarnPRService servicePR
            //, IMapper mapper
            ) : base(userService)
        {
            _itemMasterRepository = itemMasterRepository;
            //_itemSegmentNameRepository = itemSegmentNameRepository;
            //_itemSegmentValueRepository = itemSegmentValueRepository;
            _service = service;
            _servicePR = servicePR;
            //_mapper = mapper;
        }

        [Route("list")]
        public async Task<IActionResult> GetList(Status status, int isBDS)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<FreeConceptMRMaster> records = await _service.GetPagedAsync(status, isBDS, paginationInfo, AppUser);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        [HttpGet]
        [Route("new/{FBAckID}/{menuParam}")]
        public async Task<IActionResult> GetNew(int FBAckID, string menuParam = null)
        {
            return Ok(await _service.GetNewAsync(FBAckID, menuParam));
        }

        [Route("{grpConceptNo}/{menuParam}")]
        [HttpGet]
        public async Task<IActionResult> Get(string grpConceptNo, string menuParam = null)
        {
            return Ok(await _service.GetAsync(grpConceptNo, menuParam));
        }

        [Route("revision/{grpConceptNo}/{menuParam}")]
        [HttpGet]
        public async Task<IActionResult> GetRevisionOfCompleteList(string grpConceptNo, string menuParam = null)
        {
            return Ok(await _service.GetRevisionOfCompleteList(grpConceptNo, menuParam));
        }

        [Route("pendingacknowledgement/{grpConceptNo}")]
        [HttpGet]
        public async Task<IActionResult> GetPendingAcknowledgeList(string grpConceptNo)
        {
            return Ok(await _service.GetPendingAcknowledgeList(grpConceptNo));
        }

        [Route("revision/{fbAckId}/{grpConceptNo}/{menuParam}")]
        [HttpGet]
        public async Task<IActionResult> GetRevision(int fbAckId, string grpConceptNo, string menuParam = null)
        {
            return Ok(await _service.GetRevision(fbAckId, grpConceptNo));
        }

        [HttpGet]
        [Route("get-complete-mr-childs/{buyerIds}/{buyerTeamIDs}")]
        public async Task<IActionResult> GetCompleteMRChilds(string buyerIds, string buyerTeamIDs)
        {
            var paginationInfo = Request.GetPaginationInfo();
            var records = await _service.GetCompleteMRChilds(paginationInfo, buyerIds, buyerTeamIDs);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        //[Route("get/yarn/category/{segment1ValueDesc}/{segment2ValueDesc}/{segment3ValueDesc}/{segment4ValueDesc}/{segment5ValueDesc}/{segment6ValueDesc}/{shadeCode}")]
        ////[Route("get-yarn-category")]
        //[HttpGet]
        //public async Task<IActionResult> GetYarnCategory(string segment1ValueDesc, string segment2ValueDesc, string segment3ValueDesc, string segment4ValueDesc, string segment5ValueDesc, string segment6ValueDesc, string shadeCode)
        //{
        //    var yarnCategory = CommonFunction.GetYarnShortForm(segment1ValueDesc, segment2ValueDesc, segment3ValueDesc, segment4ValueDesc, segment5ValueDesc, segment6ValueDesc, shadeCode);
        //    return Ok(yarnCategory);
        //}

        [Route("get/yarn-category")]
        [HttpPost]
        public async Task<IActionResult> GetYarnCategory(YarnBookingChildItem obj)
        {
            var yarnCategory = CommonFunction.GetYarnShortForm(obj.Segment1ValueDesc, obj.Segment2ValueDesc, obj.Segment3ValueDesc, obj.Segment4ValueDesc, obj.Segment5ValueDesc, obj.Segment6ValueDesc, obj.ShadeCode);
            return Ok(yarnCategory);
        }

        private decimal GetBookingQtyKG(decimal gm, string size, decimal bcLength, decimal bcWidth, decimal pcs)
        {
            decimal qtyInKg = 0;
            if (size.IsNotNullOrEmpty())
            {
                var splitSize = size.Split('X');
                decimal selectedLenght = Convert.ToDecimal(splitSize[0].Trim());
                decimal selectedWidth = Convert.ToDecimal(splitSize[1].Trim());
                if (selectedLenght > 0 && selectedWidth > 0)
                {
                    qtyInKg = pcs * ((gm * Convert.ToDecimal(bcLength) * Convert.ToDecimal(bcWidth)) / (selectedLenght * selectedWidth));
                    qtyInKg = qtyInKg / 1000;
                }
            }
            return Math.Round(qtyInKg, 2);
        }
        private FreeConceptMRMaster GetCalculatedFieldsFBC(FreeConceptMRMaster fbc)
        {
            fbc.TotalQtyInKG = Math.Round(fbc.TotalQtyInKG, 2);
            return fbc;
        }
        private decimal GetQtyFromPer(decimal bookingQty, decimal distributionPer)
        {
            decimal YarnReqQty = (bookingQty * (distributionPer / 100));
            return YarnReqQty;
        }
        private decimal GetNetYarnReqQty(decimal yarnDistribution, decimal finishFabricUtilizationQty, decimal greyUtilizationQty, decimal dyedYarnUtilizationQty, decimal totalAllowance, decimal yDAllowance, decimal greyYarnUtilizationQty, decimal reqFinishFabricQty)
        {

            decimal yarnFFU = yarnDistribution * (finishFabricUtilizationQty / 100);
            yarnFFU = yarnFFU + (yarnFFU * totalAllowance) / 100;

            decimal yarnGU = yarnDistribution * (greyUtilizationQty / 100);
            yarnGU = yarnGU + (yarnGU * (yDAllowance + Convert.ToDecimal(0.5))) / 100;

            //decimal yarnDYU = yarnDistribution * (dyedYarnUtilizationQty / 100);
            //yarnDYU = yarnDYU + (yarnDYU * (yDAllowance)) / 100;

            //decimal yarnGYU = greyYarnUtilizationQty;
            reqFinishFabricQty = (reqFinishFabricQty / 100) * yarnDistribution;

            decimal netReqQty = reqFinishFabricQty + ((reqFinishFabricQty * totalAllowance) / 100) - yarnGU;
            //decimal netReqQty = reqFinishFabricQty + ((reqFinishFabricQty * totalAllowance) / 100) - yarnFFU - yarnGU;
            netReqQty = Math.Round(netReqQty, 2);
            return netReqQty;
        }
        private List<FreeConceptMRMaster> GetExtendChilds(List<FreeConceptMRMaster> modelChilds, List<FreeConceptMRMaster> entityChilds, List<FreeConceptMRChild> yarnBookingChildItems = null, FreeConceptMRMaster model = null)
        {
            if (yarnBookingChildItems == null) yarnBookingChildItems = new List<FreeConceptMRChild>();

            entityChilds.ForEach(ec =>
            {
                ec.Childs = new List<FreeConceptMRChild>();

                var modelChild = modelChilds.Find(x => x.Construction == ec.Construction && x.Composition == ec.Composition && x.Color == ec.Color && ec.DayValidDurationId == x.DayValidDurationId);

                if (ec.SubGroupID == 11)
                {
                    model.CollarWeightInGm = model.CollarWeightInGm.IsNull() ? 0 : model.CollarWeightInGm;
                    model.CollarSizeID = model.CollarSizeID.IsNullOrEmpty() ? "" : model.CollarSizeID;
                    ec.TotalQtyInKG = this.GetBookingQtyKG(model.CollarWeightInGm, model.CollarSizeID, ec.Length, ec.Width, ec.BookingQty);
                    ec = this.GetCalculatedFieldsFBC(ec);
                }
                else if (ec.SubGroupID == 12)
                {
                    model.CuffWeightInGm = model.CuffWeightInGm.IsNull() ? 0 : model.CuffWeightInGm;
                    model.CuffSizeID = model.CuffSizeID.IsNullOrEmpty() ? "" : model.CuffSizeID;
                    ec.TotalQtyInKG = this.GetBookingQtyKG(model.CuffWeightInGm, model.CuffSizeID, ec.Length, ec.Width, ec.BookingQty);

                    ec = this.GetCalculatedFieldsFBC(ec);
                }

                if (modelChild.IsNotNull())
                {
                    //ec.ConceptID = modelChild.ConceptID;
                    ec.ReqDate = modelChild.ReqDate;
                    ec.TrialNo = modelChild.TrialNo;
                    ec.HasYD = modelChild.HasYD;
                    ec.IsBDS = modelChild.IsBDS;
                    ec.FabricID = modelChild.FabricID;

                    modelChild.Childs.ForEach(y =>
                    {
                        y = CommonFunction.DeepClone(y);
                        y.BookingQty = this.GetQtyFromPer(ec.TotalQtyInKG, y.Distribution);
                        decimal netYRQ = GetNetYarnReqQty(y.Distribution, 0, 0, 0, y.Allowance, 0, 0, ec.TotalQtyInKG);
                        netYRQ = netYRQ > 0 ? netYRQ : 0;
                        y.ReqQty = netYRQ;

                        if (yarnBookingChildItems.Where(x => x.SubGroupId == ec.SubGroupID).Count() > 0)
                        {
                            var yarnChildItem = CommonFunction.DeepClone(yarnBookingChildItems.Where(x => x.SubGroupId == ec.SubGroupID && x.BookingChildID == ec.BookingChildID && x.ItemMasterID == y.ItemMasterID));
                            if (yarnChildItem.IsNotNull() && yarnChildItem.Count() > 0)
                            {
                                y.FCMRChildID = yarnChildItem.FirstOrDefault().FCMRChildID;
                                y.FCMRMasterID = yarnChildItem.FirstOrDefault().FCMRMasterID;
                                //y.Acknowledge = yarnChildItem.FirstOrDefault().Acknowledge;
                                ec.FCMRMasterID = y.FCMRMasterID;
                            }
                        }
                        ec.Childs.Add(y);
                    });
                }
            });
            modelChilds = CommonFunction.DeepClone(entityChilds);
            return modelChilds;
        }

        [Route("save")]
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> Save(dynamic jsnString)
        {
            List<FreeConceptMRMaster> models = JsonConvert.DeserializeObject<List<FreeConceptMRMaster>>(
              Convert.ToString(jsnString),
              new JsonSerializerSettings
              {
                  DateTimeZoneHandling = DateTimeZoneHandling.Local // Ensures the date is interpreted as local time
              });

            string grpConceptNo = models[0].GroupConceptNo;
            int vFCMRMasterID = models[0].FCMRMasterID;
            int bookingID = models[0].BookingID;
            int isBDS = models[0].IsBDS;
            //bool isModified = models[0].IsModified;
            bool isModified = models[0].Modify;
            bool isComplete = models[0].IsComplete;



            List<YarnStockAdjustmentMaster> stocks = new List<YarnStockAdjustmentMaster>();
            YarnStockAdjustmentMaster stock = new YarnStockAdjustmentMaster();

            List<FreeConceptMRChild> childRecords = new List<FreeConceptMRChild>();
            models.ForEach(mr =>
            {
                childRecords.AddRange(mr.Childs);
            });
            // Set Item master Id.
            _itemMasterRepository.GenerateItem(AppConstants.ITEM_SUB_GROUP_YARN_NEW, ref childRecords);

            List<FreeConceptMRMaster> entities = new List<FreeConceptMRMaster>();
            List<FreeConceptMRMaster> entitiesTemp = new List<FreeConceptMRMaster>();
            if (isModified)
            {
                entities = new List<FreeConceptMRMaster>();
                entitiesTemp = await _service.GetMultiDetailsAsync(grpConceptNo, bookingID);

                List<FreeConceptMaster> freeConceptsEntity = new List<FreeConceptMaster>();
                List<FreeConceptMaster> freeConceptsUpdate = new List<FreeConceptMaster>();
                FBookingAcknowledge bookingChildEntity = null;
                #region Extend Model
                var model = models.Where(x => x.SubGroupID == 11 || x.SubGroupID == 12).FirstOrDefault();
                if (model.IsNotNull())
                {
                    if (model.MenuParam == "MRPB")
                    {
                        freeConceptsEntity = await _service.GetAllConceptByBookingNo(grpConceptNo);
                        bookingChildEntity = await _service.GetPYBForBulkAsync(grpConceptNo);
                        List<FreeConceptMRChild> yarnBookingChildItems = await _service.GetPYBYarnByBookingNo(grpConceptNo);

                        var collarListModel = models.Where(x => x.SubGroupID == 11).ToList();
                        var cuffListModel = models.Where(x => x.SubGroupID == 12).ToList();

                        var collarsEntity = bookingChildEntity.MRMasters.Where(x => x.SubGroupID == 11).ToList();
                        var cuffsEntity = bookingChildEntity.MRMasters.Where(x => x.SubGroupID == 12).ToList();
                        List<FreeConceptMRMaster> collarList = this.GetExtendChilds(collarListModel, collarsEntity, yarnBookingChildItems, model);
                        List<FreeConceptMRMaster> cuffList = this.GetExtendChilds(cuffListModel, cuffsEntity, yarnBookingChildItems, model);

                        models = models.Where(x => x.SubGroupID == 1).ToList();
                        models.AddRange(collarList);
                        models.AddRange(cuffList);

                        models = CommonFunction.DeepClone(models);
                    }
                }
                #endregion

                entitiesTemp.ForEach(ent =>
                {
                    ent.EntityState = EntityState.Unchanged;
                    ent.Childs.SetUnchanged();
                });

                FreeConceptMRMaster entity;
                FreeConceptMRChild childEntity;
                models.ForEach(mr =>
                {
                    entity = entitiesTemp.FirstOrDefault(y => y.FCMRMasterID == mr.FCMRMasterID);
                    if (entity == null)
                    {
                        entity = mr;
                        entitiesTemp.Add(entity);
                    }
                    else
                    {
                        if (isComplete)
                        {
                            entity.IsComplete = true;
                        }
                        entity.IsBDS = isBDS;
                        entity.UpdatedBy = AppUser.UserCode;
                        entity.DateUpdated = DateTime.Now;
                        entity.EntityState = EntityState.Modified;

                        foreach (FreeConceptMRChild child in mr.Childs)
                        {
                            //var cfValue = CommonFunction.GetYarnShortForm(child.Segment1ValueDesc, child.Segment2ValueDesc, child.Segment3ValueDesc, child.Segment4ValueDesc, child.Segment5ValueDesc, child.Segment6ValueDesc, child.ShadeCode);

                            childEntity = entity.Childs.FirstOrDefault(c => c.FCMRChildID == child.FCMRChildID);
                            if (childEntity == null)
                            {
                                childEntity = child;
                                childEntity.EntityState = EntityState.Added;
                                childEntity.YarnCategory = CommonFunction.GetYarnShortForm(childEntity.Segment1ValueDesc, childEntity.Segment2ValueDesc, childEntity.Segment3ValueDesc, childEntity.Segment4ValueDesc, childEntity.Segment5ValueDesc, childEntity.Segment6ValueDesc, childEntity.ShadeCode);
                                entity.Childs.Add(childEntity);
                            }
                            else
                            {
                                childEntity.ItemMasterID = child.ItemMasterID;
                                childEntity.YDItem = child.YDItem;
                                childEntity.YD = child.YD;
                                childEntity.ReqQty = child.ReqQty;
                                childEntity.UnitID = child.UnitID;
                                childEntity.Remarks = child.Remarks;
                                childEntity.ReqCone = child.ReqCone;
                                childEntity.IsPR = child.IsPR;
                                childEntity.Acknowledge = child.Acknowledge;
                                childEntity.Distribution = child.Distribution;
                                childEntity.BookingQty = child.BookingQty;
                                childEntity.Allowance = child.Allowance;
                                childEntity.ShadeCode = child.ShadeCode;
                                childEntity.YarnStockSetId = child.YarnStockSetId;
                                childEntity.DayValidDurationId = child.DayValidDurationId;
                                childEntity.YarnCategory = CommonFunction.GetYarnShortForm(childEntity.Segment1ValueDesc, childEntity.Segment2ValueDesc, childEntity.Segment3ValueDesc, childEntity.Segment4ValueDesc, childEntity.Segment5ValueDesc, childEntity.Segment6ValueDesc, childEntity.ShadeCode);
                                childEntity.EntityState = EntityState.Modified;
                            }
                        }
                    }
                    if (model.IsNotNull())
                    {
                        if (model.MenuParam == "MRPB")
                        {
                            FreeConceptMaster fc = freeConceptsEntity.Where(fce => fce.ConceptID == mr.ConceptID).FirstOrDefault();
                            if (mr.SubGroupID != 1)
                            {
                                fc.QtyInKG = mr.TotalQtyInKG;
                                fc.TotalQtyInKG = mr.TotalQtyInKG;
                                fc.EntityState = EntityState.Modified;
                                freeConceptsUpdate.Add(fc);
                            }
                            mr.PreProcessRevNo = fc.PreProcessRevNo;
                            mr.RevisionNo = fc.RevisionNo;
                        }
                    }
                });
                if (model.IsNotNull())
                {
                    if (model.MenuParam == "MRPB")
                    {
                        bookingChildEntity.CollarSizeID = model.CollarSizeID;
                        bookingChildEntity.CollarWeightInGm = model.CollarWeightInGm;
                        bookingChildEntity.CuffSizeID = model.CuffSizeID;
                        bookingChildEntity.CuffWeightInGm = model.CuffWeightInGm;
                        bookingChildEntity.EntityState = EntityState.Modified;
                    }
                }
                entitiesTemp.ForEach(ent =>
                {
                    ent.Childs.Where(x => x.EntityState == EntityState.Unchanged).SetDeleted();
                });

                await _service.SaveMultipleAsync(entitiesTemp, EntityState.Modified, AppUser.UserCode, freeConceptsUpdate, bookingChildEntity);
            }
            else
            {
                entities = new List<FreeConceptMRMaster>();
                List<FreeConceptMaster> freeConceptsEntity = new List<FreeConceptMaster>();
                List<FreeConceptMaster> freeConceptsUpdate = new List<FreeConceptMaster>();
                FBookingAcknowledge bookingChildEntity = null;
                #region Extend Model
                var model = models.Where(x => x.SubGroupID == 11 || x.SubGroupID == 12).FirstOrDefault();
                if (model.IsNotNull())
                {
                    if (model.MenuParam == "MRPB")
                    {
                        freeConceptsEntity = await _service.GetAllConceptByBookingNo(grpConceptNo);
                        bookingChildEntity = await _service.GetPYBForBulkAsync(grpConceptNo);
                        List<FreeConceptMRChild> yarnBookingChildItems = await _service.GetPYBYarnByBookingNo(grpConceptNo);

                        var collarListModel = models.Where(x => x.SubGroupID == 11).ToList();
                        var cuffListModel = models.Where(x => x.SubGroupID == 12).ToList();

                        var collarsEntity = bookingChildEntity.MRMasters.Where(x => x.SubGroupID == 11).ToList();
                        var cuffsEntity = bookingChildEntity.MRMasters.Where(x => x.SubGroupID == 12).ToList();
                        List<FreeConceptMRMaster> collarList = this.GetExtendChilds(collarListModel, collarsEntity, yarnBookingChildItems, model);
                        List<FreeConceptMRMaster> cuffList = this.GetExtendChilds(cuffListModel, cuffsEntity, yarnBookingChildItems, model);

                        models = models.Where(x => x.SubGroupID == 1).ToList();
                        models.AddRange(collarList);
                        models.AddRange(cuffList);

                        models = CommonFunction.DeepClone(models);
                    }
                }
                #endregion

                models.ForEach(mr =>
                {
                    mr.IsBDS = isBDS;
                    mr.AddedBy = AppUser.UserCode;
                    mr.DateAdded = DateTime.Now;
                    if (isComplete)
                    {
                        mr.IsComplete = true;
                    }
                    mr.Childs.ForEach(child =>
                    {
                        //var cfValue = CommonFunction.GetYarnShortForm(child.Segment1ValueDesc, child.Segment2ValueDesc, child.Segment3ValueDesc, child.Segment4ValueDesc, child.Segment5ValueDesc, child.Segment6ValueDesc, child.ShadeCode);

                        //child.ItemMasterID = childRecords.Find(x => x.FCMRChildID == child.FCMRChildID).ItemMasterID;
                        child.Acknowledge = true;
                        child.AcknowledgeBy = AppUser.UserCode;
                        child.Reject = false;
                        //child.IsPR = true;
                        child.YarnCategory = CommonFunction.GetYarnShortForm(child.Segment1ValueDesc, child.Segment2ValueDesc, child.Segment3ValueDesc, child.Segment4ValueDesc, child.Segment5ValueDesc, child.Segment6ValueDesc, child.ShadeCode);
                        child.EntityState = EntityState.Added;
                    });
                    if (model.IsNotNull())
                    {
                        if (model.MenuParam == "MRPB")
                        {
                            FreeConceptMaster fc = freeConceptsEntity.Where(fce => fce.ConceptID == mr.ConceptID).FirstOrDefault();
                            if (mr.SubGroupID != 1)
                            {
                                fc.QtyInKG = mr.TotalQtyInKG;
                                fc.TotalQtyInKG = mr.TotalQtyInKG;
                                fc.EntityState = EntityState.Modified;
                                freeConceptsUpdate.Add(fc);
                            }
                            mr.PreProcessRevNo = fc.PreProcessRevNo;
                            mr.RevisionNo = fc.RevisionNo;
                        }
                    }
                });
                if (model.IsNotNull())
                {
                    if (model.MenuParam == "MRPB")
                    {
                        bookingChildEntity.CollarSizeID = model.CollarSizeID;
                        bookingChildEntity.CollarWeightInGm = model.CollarWeightInGm;
                        bookingChildEntity.CuffSizeID = model.CuffSizeID;
                        bookingChildEntity.CuffWeightInGm = model.CuffWeightInGm;
                        bookingChildEntity.EntityState = EntityState.Modified;
                    }
                }
                entities = models;
                await _service.SaveMultipleAsync(entities, EntityState.Added, AppUser.UserCode, freeConceptsUpdate, bookingChildEntity);
            }

            return Ok();
        }

        [HttpPost]
        [Route("acknowledge/{id}")]
        public async Task<IActionResult> Acknowledge(int id)
        {
            FreeConceptMRMaster entity = await _service.GetDetailsAsync(id);
            entity.EntityState = EntityState.Modified;
            entity.Childs.SetUnchanged();

            entity.Childs.Where(y => y.IsPR).ToList().ForEach(x =>
            {
                x.Acknowledge = true;
                x.AcknowledgeBy = AppUser.UserCode;
                x.AcknowledgeDate = DateTime.Now;
                x.Reject = false;
                x.EntityState = EntityState.Modified;
            });

            await _service.SaveAsync(entity);

            return Ok();
        }

        [HttpPost]
        [Route("acknowledgeAutoPR/{ConceptNo}")]
        public async Task<IActionResult> acknowledgeautopr(string ConceptNo)
        {
            FreeConceptMRMaster entity = await _service.GetAsync(ConceptNo);

            foreach (var item in entity.OtherItems)
            {
                FreeConceptMRMaster ackentity = await _service.GetDetailsAsync(item.FCMRMasterID);
                ackentity.EntityState = EntityState.Modified;
                ackentity.IsAcknowledge = true;
                ackentity.AcknowledgedBy = AppUser.UserCode;
                ackentity.AcknowledgeDate = DateTime.Now;
                ackentity.Childs.SetUnchanged();

                await _service.SaveAsync(ackentity);
            }
            //Auto PR
            YarnPRMaster yarnPRMaster = new YarnPRMaster();
            yarnPRMaster.YarnPRFromID = PRFrom.PROJECTION_YARN_BOOKING;

            yarnPRMaster.YarnPRFromTableId = YarnPRFromTable.FreeConceptMRBDS;
            yarnPRMaster.YarnPRFromMasterId = entity.FCMRMasterID;

            yarnPRMaster.SendForApproval = true;
            yarnPRMaster.Approve = true;
            yarnPRMaster.ApproveBy = AppUser.UserCode;
            yarnPRMaster.ApproveDate = DateTime.Now;
            yarnPRMaster.AddedBy = AppUser.UserCode;
            yarnPRMaster.DateAdded = DateTime.Now;
            yarnPRMaster.YarnPRDate = DateTime.Now;
            yarnPRMaster.YarnPRRequiredDate = DateTime.Now;
            yarnPRMaster.YarnPRBy = AppUser.UserCode;
            yarnPRMaster.TriggerPointID = 1252; //Projection Based
            yarnPRMaster.IsRNDPR = true;
            yarnPRMaster.IsCPR = false;
            yarnPRMaster.IsFPR = false;
            yarnPRMaster.CompanyID = 0;
            yarnPRMaster.SubGroupID = 102; // yarn
            yarnPRMaster.BookingNo = ConceptNo;
            yarnPRMaster.YDMaterialRequirementNo = ConceptNo;
            yarnPRMaster.ConceptNo = ConceptNo;
            yarnPRMaster.EntityState = EntityState.Added;

            foreach (FreeConceptMRMaster itm in entity.OtherItems)
            {
                itm.Childs.ForEach(child =>
                {
                    YarnPRChild yarnPRChild = new YarnPRChild();
                    yarnPRChild.ItemMasterID = child.ItemMasterID;
                    yarnPRChild.UnitID = child.UnitID;
                    yarnPRChild.ReqQty = child.ReqQty;
                    yarnPRChild.ReqCone = Convert.ToInt32(child.ReqCone);
                    yarnPRChild.Remarks = child.Remarks;
                    yarnPRChild.FPRCompanyID = 0;
                    //yarnPRChild.FPRCompanyID = entity.CompanyID;
                    yarnPRChild.ShadeCode = child.ShadeCode;

                    yarnPRChild.SetupChildID = 0;
                    yarnPRChild.HSCode = "";
                    //yarnPRChild.PYBBookingChildID = childitem.YBChildItemID;
                    yarnPRChild.PYBBookingChildID = 0;
                    yarnPRChild.DayValidDurationId = child.DayValidDurationId;
                    yarnPRChild.EntityState = EntityState.Added;
                    yarnPRMaster.Childs.Add(yarnPRChild);
                });
            }
            var itemMasterIdStr = string.Join(",", yarnPRMaster.Childs.Select(x => x.ItemMasterID).Distinct());
            var itemMasterIds = itemMasterIdStr.Split(',');

            List<YarnPRChild> groupChilds = new List<YarnPRChild>();
            foreach (string itemMasterId in itemMasterIds)
            {
                List<YarnPRChild> childs = yarnPRMaster.Childs.Where(x => x.ItemMasterID == Convert.ToInt32(itemMasterId)).ToList();

                YarnPRChild yarnPRChild = new YarnPRChild();

                yarnPRChild.ItemMasterID = Convert.ToInt32(itemMasterId);
                yarnPRChild.ReqQty = childs.Sum(x => x.ReqQty);
                yarnPRChild.UnitID = childs[0].UnitID;
                yarnPRChild.ReqCone = childs[0].ReqCone;
                yarnPRChild.Remarks = childs[0].Remarks;
                yarnPRChild.FPRCompanyID = 0;
                yarnPRChild.ShadeCode = childs[0].ShadeCode;
                yarnPRChild.SetupChildID = 0;
                yarnPRChild.HSCode = "";
                yarnPRChild.PYBBookingChildID = 0;
                yarnPRChild.EntityState = EntityState.Added;

                groupChilds.Add(yarnPRChild);
            }
            yarnPRMaster.Childs = groupChilds;
            await _service.AcknowledgeEntityAsync(yarnPRMaster, AppUser.UserCode);

            return Ok();
        }

        [HttpPost]
        [Route("reject/{id}/{reason}")]
        public async Task<IActionResult> Reject(int id, string reason)
        {
            FreeConceptMRMaster entity = await _service.GetDetailsAsync(id);
            entity.EntityState = EntityState.Unchanged;
            entity.Childs.SetUnchanged();

            entity.Childs.Where(y => y.IsPR).ToList().ForEach(x =>
            {
                x.Acknowledge = false;
                x.Reject = true;
                x.RejectBy = AppUser.UserCode;
                x.RejectDate = DateTime.Now;
                x.RejectReason = reason;
                x.EntityState = EntityState.Modified;
            });

            await _service.SaveAsync(entity);

            return Ok();
        }

        [HttpPost]
        [Route("remove-from-reject/{id}")]
        public async Task<IActionResult> RemoveFromReject(int id)
        {
            FreeConceptMRMaster entity = await _service.GetDetailsAsync(id);
            entity.EntityState = EntityState.Unchanged;
            entity.Childs.SetUnchanged();

            entity.Childs.Where(y => y.Reject).ToList().ForEach(x =>
            {
                x.Acknowledge = false;
                x.Reject = false;
                x.EntityState = EntityState.Modified;
            });

            await _service.SaveAsync(entity);

            return Ok();
        }

        [Route("revise")]
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> Revise(List<FreeConceptMRMaster> models)
        {
            string grpConceptNo = models[0].GroupConceptNo;
            int vFCMRMasterID = models[0].FCMRMasterID;
            int bookingID = models[0].BookingID;
            bool isModified = models[0].IsModified;
            bool isComplete = models[0].IsComplete;
            bool isOwnRevise = models[0].IsOwnRevise; //IF isOwnRevise=False then from Revision List, else from Complete List 

            List<FreeConceptMRChild> childRecords = new List<FreeConceptMRChild>();
            models.ForEach(mr =>
            {
                childRecords.AddRange(mr.Childs);
            });
            // Set Item master Id.
            _itemMasterRepository.GenerateItem(AppConstants.ITEM_SUB_GROUP_YARN_NEW, ref childRecords);

            List<FreeConceptMRMaster> entities = new List<FreeConceptMRMaster>();
            List<FreeConceptMRMaster> entitiesTemp = new List<FreeConceptMRMaster>();

            string fcmrChildIds = "";
            entities = new List<FreeConceptMRMaster>();

            entitiesTemp = await _service.GetDetailsAsyncForRevise(grpConceptNo, bookingID, isOwnRevise);

            List<FreeConceptMaster> freeConceptsEntity = new List<FreeConceptMaster>();
            List<FreeConceptMaster> freeConceptsUpdate = new List<FreeConceptMaster>();
            FBookingAcknowledge bookingChildEntity = null;
            #region Extend Model
            var model = models.Where(x => x.SubGroupID == 11 || x.SubGroupID == 12).FirstOrDefault();
            if (model.IsNotNull())
            {
                if (model.MenuParam == "MRPB")
                {
                    freeConceptsEntity = await _service.GetAllConceptByBookingNo(grpConceptNo);
                    bookingChildEntity = await _service.GetPYBForBulkAsync(grpConceptNo);
                    List<FreeConceptMRChild> yarnBookingChildItems = await _service.GetPYBYarnByBookingNo(grpConceptNo);

                    var collarListModel = models.Where(x => x.SubGroupID == 11).ToList();
                    var cuffListModel = models.Where(x => x.SubGroupID == 12).ToList();

                    var collarsEntity = bookingChildEntity.MRMasters.Where(x => x.SubGroupID == 11).ToList();
                    var cuffsEntity = bookingChildEntity.MRMasters.Where(x => x.SubGroupID == 12).ToList();
                    List<FreeConceptMRMaster> collarList = this.GetExtendChilds(collarListModel, collarsEntity, yarnBookingChildItems, model);
                    List<FreeConceptMRMaster> cuffList = this.GetExtendChilds(cuffListModel, cuffsEntity, yarnBookingChildItems, model);

                    models = models.Where(x => x.SubGroupID == 1).ToList();
                    models.AddRange(collarList);
                    models.AddRange(cuffList);

                    models = CommonFunction.DeepClone(models);
                }
            }
            #endregion

            entitiesTemp.ForEach(ent =>
            {
                ent.EntityState = EntityState.Unchanged;
                ent.Childs.SetUnchanged();
                //fcmrChildIds += fcmrChildIds.Length == 0 ? string.Join(",", ent.Childs.Select(y => y.FCMRChildID))
                //                                         : "," + string.Join(",", ent.Childs.Select(y => y.FCMRChildID));

                if (ent.Childs.Count() > 0)
                {
                    fcmrChildIds += string.Join(",", ent.Childs.Select(y => y.FCMRChildID));
                    fcmrChildIds += ",";
                }
            });
            if (fcmrChildIds.Length > 0)
            {
                fcmrChildIds = fcmrChildIds.Remove(fcmrChildIds.Length - 1);
            }

            FreeConceptMRMaster entity;
            FreeConceptMRChild childEntity;
            models.ForEach(mr =>
            {
                entity = entitiesTemp.FirstOrDefault(y => y.FCMRMasterID == mr.FCMRMasterID);
                if (entity == null)
                {
                    entity = mr;
                    entitiesTemp.Add(entity);
                }
                else
                {
                    if (isComplete)
                    {
                        entity.IsComplete = true;
                    }
                    //entity.PreProcessRevNo = entity.RevisionNo;
                    entity.RevisionNo = entity.RevisionNo + 1;
                    entity.RevisionDate = DateTime.Now;
                    entity.RevisionBy = AppUser.UserCode;
                    entity.RevisionReason = "";
                    if (mr.IsNeedRevisionTemp == true)
                    {
                        entity.IsNeedRevisionTemp = true;
                        entity.IsNeedRevision = true;
                    }
                    entity.IsBDS = 1;
                    entity.UpdatedBy = AppUser.UserCode;
                    entity.DateUpdated = DateTime.Now;
                    entity.EntityState = EntityState.Modified;

                    foreach (FreeConceptMRChild child in mr.Childs)
                    {
                        childEntity = entity.Childs.FirstOrDefault(c => c.FCMRChildID == child.FCMRChildID);
                        if (childEntity == null)
                        {
                            childEntity = child;
                            childEntity.YarnCategory = CommonFunction.GetYarnShortForm(child.Segment1ValueDesc, child.Segment2ValueDesc, child.Segment3ValueDesc, child.Segment4ValueDesc, child.Segment5ValueDesc, child.Segment6ValueDesc, childEntity.ShadeCode);
                            childEntity.EntityState = EntityState.Added;
                            entity.Childs.Add(childEntity);
                        }
                        else
                        {
                            childEntity.ItemMasterID = child.ItemMasterID;
                            childEntity.YarnCategory = CommonFunction.GetYarnShortForm(child.Segment1ValueDesc, child.Segment2ValueDesc, child.Segment3ValueDesc, child.Segment4ValueDesc, child.Segment5ValueDesc, child.Segment6ValueDesc, childEntity.ShadeCode);
                            childEntity.YDItem = child.YDItem;
                            childEntity.YD = child.YD;
                            childEntity.ReqQty = child.ReqQty;
                            childEntity.UnitID = child.UnitID;
                            childEntity.Remarks = child.Remarks;
                            childEntity.ReqCone = child.ReqCone;
                            childEntity.IsPR = child.IsPR;
                            childEntity.Acknowledge = child.Acknowledge;
                            childEntity.Distribution = child.Distribution;
                            childEntity.BookingQty = child.BookingQty;
                            childEntity.Allowance = child.Allowance;
                            childEntity.ShadeCode = child.ShadeCode;
                            childEntity.YarnStockSetId = child.YarnStockSetId;
                            childEntity.DayValidDurationId = child.DayValidDurationId;
                            childEntity.EntityState = EntityState.Modified;
                        }
                    }
                }
                if (model.IsNotNull())
                {
                    if (model.MenuParam == "MRPB")
                    {
                        FreeConceptMaster fc = freeConceptsEntity.Where(fce => fce.ConceptID == mr.ConceptID).FirstOrDefault();
                        if (mr.SubGroupID != 1)
                        {
                            fc.QtyInKG = mr.TotalQtyInKG;
                            fc.TotalQtyInKG = mr.TotalQtyInKG;
                            fc.EntityState = EntityState.Modified;
                            freeConceptsUpdate.Add(fc);
                        }
                        mr.PreProcessRevNo = fc.PreProcessRevNo;
                        mr.RevisionNo = fc.RevisionNo;
                    }
                }
            });
            if (model.IsNotNull())
            {
                if (model.MenuParam == "MRPB")
                {
                    bookingChildEntity.CollarSizeID = model.CollarSizeID;
                    bookingChildEntity.CollarWeightInGm = model.CollarWeightInGm;
                    bookingChildEntity.CuffSizeID = model.CuffSizeID;
                    bookingChildEntity.CuffWeightInGm = model.CuffWeightInGm;
                    bookingChildEntity.EntityState = EntityState.Modified;
                }
            }
            entitiesTemp.ForEach(ent =>
            {
                if (ent.EntityState == EntityState.Unchanged) ent.EntityState = EntityState.Deleted;
                ent.Childs.Where(x => x.EntityState == EntityState.Unchanged).SetDeleted();
            });

            List<YarnPRMaster> prMasters = new List<YarnPRMaster>();
            if (grpConceptNo.IsNotNullOrEmpty())
            {
                prMasters = await _servicePR.GetByPRNo(grpConceptNo);
                prMasters.ForEach(x =>
                {
                    x.NeedRevision = true;
                    x.EntityState = EntityState.Modified;
                });
            }

            await _service.ReviseAsync(entitiesTemp, grpConceptNo, AppUser.UserCode, fcmrChildIds, prMasters, freeConceptsUpdate, bookingChildEntity);
            return Ok();
        }
    }
}
