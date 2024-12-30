using AutoMapper;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.API.Extends.Filters;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Static;
using System.Net.Mail;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.AspNetCore.Mvc;
using System.Data.Entity;
using NLog;
using EPYSLTEXCore.Application.Interfaces.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using EPYSLTEXCore.Application.Interfaces.Knitting;
using EPYSLTEXCore.Application.Interfaces;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEX.Infrastructure.Services;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Knitting;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using Newtonsoft.Json;
using EPYSLTEXCore.Infrastructure.Entities.Gmt.SupplyChain;

namespace EPYSLTEXCore.API.Contollers.Inventory.Yarn
{
    [Route("api/yarn-rnd-requisition")]
    public class YarnRnDRequisitionController : ApiBaseController
    {
        private readonly IYarnRnDReqService _service;
        private readonly IKYReqService _kyService;
        private readonly IItemMasterService<YarnRnDReqChild> _itemMasterRepository;

        private readonly IMapper _mapper;
        #region PendingWork
        // Pending Work
        //private readonly IEmailService _emailService;
        // private readonly IReportingService _reportingService;
        #endregion 
        private static Logger _logger;

        public YarnRnDRequisitionController(IYarnRnDReqService service, IKYReqService kyService,
            IItemMasterService<YarnRnDReqChild> itemMasterRepository
            //, IMapper mapper
            , IUserService userService) : base(userService)
        {
            _service = service;
            _kyService = kyService;
            _itemMasterRepository = itemMasterRepository;
            //_mapper = mapper;
           // _emailService = emailService;
            //_reportingService = reportingService;
            _logger = LogManager.GetCurrentClassLogger();
        }

        [Route("list")]
        public async Task<IActionResult> GetList(Status status, string pageName, bool isReqForYDShow = true)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<YarnRnDReqMaster> records = await _service.GetPagedAsync(status, paginationInfo, pageName, isReqForYDShow);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        //[Route("new/{fcIds}/{Status}")]
        //[HttpGet]
        //public async Task<IHttpActionResult> GetFreeConceptMRData(string[] fcIds, string Status)
        //{
        //    return Ok(await _service.GetFreeConceptMRData(fcIds, Status));
        //}
        [Route("new2")]
        [HttpPost]
        public async Task<IActionResult> GetFreeConceptMRDataNew(dynamic jsnString)
        {
            List<YarnRnDReqMaster> model = JsonConvert.DeserializeObject<List<YarnRnDReqMaster>>(
                Convert.ToString(jsnString),
                new JsonSerializerSettings
                {
                    DateTimeZoneHandling = DateTimeZoneHandling.Local // Ensures the date is interpreted as local time
                });

            model[0].GroupIDs = string.Join(",", model.Select(x => x.GroupID).Distinct());
            return Ok(await _service.GetFreeConceptMRDataNew(model));
        }
        [Route("new-without-knitting-info")]
        [HttpPost]
        public async Task<IActionResult> GetNewWithoutKnittingInfo()
        {
            YarnRnDReqMaster yarnRnDReq = await _service.GetNewWithoutKnittingInfo();
            return Ok(yarnRnDReq);
        }

        [HttpGet]
        [Route("MRs/{fcIds}")]
        public async Task<IActionResult> GetMRs(string fcIds)
        {
            var paginationInfo = Request.GetPaginationInfo();
            var records = await _service.GetMRs(fcIds, paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        [Route("{id}/{flag}/{requisitionType}")]
        [HttpGet]
        public async Task<IActionResult> Get(int id, int flag, string requisitionType = null)
        {
            return Ok(await _service.GetAsync(id, flag, requisitionType));
        }
        [Route("groupBy/{id}/{flag}/{requisitionType}")]
        [HttpGet]
        public async Task<IActionResult> GetGroupBy(int id, int flag, string requisitionType = null)
        {
            return Ok(await _service.GetAsyncGroupBy(id, flag, requisitionType));
        }
        [Route("revise/{id}/{flag}/{mrId}")]
        [HttpGet]
        public async Task<IActionResult> GetRevise(int id, int flag, string mrId)
        {
            return Ok(await _service.GetReviseAsync(id, flag, mrId));
        }

        [Route("save")]
        [HttpPost]
        public async Task<IActionResult> Save(dynamic jsnString)
        {
            YarnRnDReqMaster model = JsonConvert.DeserializeObject<YarnRnDReqMaster>(
                  Convert.ToString(jsnString),
                  new JsonSerializerSettings
                  {
                      DateTimeZoneHandling = DateTimeZoneHandling.Local // Ensures the date is interpreted as local time
                  });

            YarnRnDReqMaster entity;
            bool isAdditional = model.IsAdditional;
            string conceptNo = string.Join(",", model.Childs.Select(x => x.ConceptNo).Distinct());

            List<YarnRnDReqChild> childRecords = model.Childs.Where(x => x.ItemMasterID == 0).ToList();
            if (childRecords.Count() > 0)
            {
                _itemMasterRepository.GenerateItem(AppConstants.ITEM_SUB_GROUP_YARN_NEW, ref childRecords);
                childRecords.ForEach(x =>
                {
                    model.Childs.Find(y => y.RnDReqChildID == x.RnDReqChildID).ItemMasterID = x.ItemMasterID;
                });
            }

            if (model.IsModified)
            {
                entity = await _service.GetDetailsAsync(model.RnDReqMasterID);

                entity.ConceptNo = conceptNo;
                entity.RnDReqDate = model.RnDReqDate;
                entity.RCompanyID = model.RCompanyID;
                entity.IsReqForYD = model.IsReqForYD;
                entity.OCompanyID = model.OCompanyID;
                //entity.LocationID = model.LocationID;
                //entity.SupplierID = model.SupplierID;
                entity.UpdatedBy = AppUser.UserCode;
                entity.DateUpdated = DateTime.Now;
                entity.EntityState = EntityState.Modified;

                entity.Childs.SetUnchanged();

                foreach (YarnRnDReqChild mChild in model.Childs)
                {
                    YarnRnDReqChild childEntity = entity.Childs.FirstOrDefault(x => x.RnDReqChildID == mChild.RnDReqChildID);
                    if (childEntity == null)
                    {
                        mChild.EntityState = EntityState.Added;
                        mChild.YarnCategory = CommonFunction.GetYarnShortForm(mChild.Segment1ValueDesc, mChild.Segment2ValueDesc, mChild.Segment3ValueDesc, mChild.Segment4ValueDesc, mChild.Segment5ValueDesc, mChild.Segment6ValueDesc, mChild.ShadeCode);
                        entity.Childs.Add(mChild);
                    }
                    else
                    {
                        childEntity.YarnBrandID = mChild.YarnBrandID;
                        childEntity.BatchNo = mChild.BatchNo;
                        childEntity.ReqQty = mChild.ReqQty;
                        childEntity.ReqCone = mChild.ReqCone;
                        childEntity.Remarks = mChild.Remarks;
                        childEntity.PhysicalCount = mChild.PhysicalCount;
                        childEntity.YarnLotNo = mChild.YarnLotNo;
                        childEntity.PreProcessRevNo = mChild.PreProcessRevNo;
                        childEntity.ItemMasterID = mChild.ItemMasterID;
                        childEntity.StockTypeId = mChild.StockTypeId;
                        childEntity.YarnCategory = CommonFunction.GetYarnShortForm(childEntity.Segment1ValueDesc, childEntity.Segment2ValueDesc, childEntity.Segment3ValueDesc, childEntity.Segment4ValueDesc, childEntity.Segment5ValueDesc, childEntity.Segment6ValueDesc, childEntity.ShadeCode);
                        childEntity.EntityState = EntityState.Modified;
                    }
                }
                entity.Childs.Where(x => x.EntityState == EntityState.Unchanged).SetDeleted();
            }
            else
            {
                //entity = _mapper.Map<YarnRnDReqMaster>(model);
                entity = model;
                entity.ConceptNo = conceptNo;
                //entity.OCompanyID = entity.RCompanyID;
                entity.RnDReqBy = AppUser.EmployeeCode;
                entity.AddedBy = AppUser.UserCode;
                entity.DateAdded = DateTime.Now;
                if (isAdditional)
                {
                    entity.IsAdditional = true;
                    entity.RnDReqDate = DateTime.Now;
                }
                entity.Childs.ToList().ForEach(childEntity =>
                {
                    childEntity.YarnCategory = CommonFunction.GetYarnShortForm(childEntity.Segment1ValueDesc, childEntity.Segment2ValueDesc, childEntity.Segment3ValueDesc, childEntity.Segment4ValueDesc, childEntity.Segment5ValueDesc, childEntity.Segment6ValueDesc, childEntity.ShadeCode);
                    childEntity.EntityState= EntityState.Added;
                });

                
            }
            await _service.SaveAsync(entity, AppUser.UserCode);
            return Ok();
        }
        [Route("approve")]
        [HttpPost]
        public async Task<IActionResult> Approve(dynamic jsnString)
        {
            YarnRnDReqMaster model = JsonConvert.DeserializeObject<YarnRnDReqMaster>(
                  Convert.ToString(jsnString),
                  new JsonSerializerSettings
                  {
                      DateTimeZoneHandling = DateTimeZoneHandling.Local // Ensures the date is interpreted as local time
                  });
            YarnRnDReqMaster entity = await _service.GetDetailsAsync(model.RnDReqMasterID);
            entity.Childs.SetUnchanged();
            entity.YarnRnDReqBuyerTeams.SetUnchanged();
            entity.IsApprove = true;
            entity.ApproveBy = AppUser.UserCode;
            entity.ApproveDate = DateTime.Now;
            entity.IsAcknowledge = false;
            entity.AcknowledgeBy = 0;
            entity.AcknowledgeDate = null;
            entity.IsUnAcknowledge = false;
            entity.UnAcknowledgeBy = 0;
            entity.UnAcknowledgeDate = null;
            entity.EntityState = EntityState.Modified;
            entity.Childs.ForEach(x =>
            {
                YarnRnDReqChild child = model.Childs.Find(y => y.RnDReqChildID == x.RnDReqChildID);
                if (child != null)
                {
                    x.Remarks = child.Remarks;
                    x.EntityState = EntityState.Modified;
                }
            });

            entity.Childs.Where(x => x.EntityState == EntityState.Unchanged).SetDeleted();
            await _service.SaveAsync(entity, AppUser.UserCode);
            return Ok();
        }

        /*
        [Route("approve/{id}")]
        [HttpPost]
        [ValidateModel]
        public async Task<IHttpActionResult> Approve(int id)
        {
            YarnRnDReqMaster entity = await _service.GetDetailsAsync(id);
            entity.Childs.SetUnchanged();

            entity.IsApprove = true;
            entity.ApproveBy = AppUser.EmployeeCode;
            entity.ApproveDate = DateTime.Now;

            entity.IsAcknowledge = false;
            entity.AcknowledgeBy = 0;
            entity.AcknowledgeDate = null;

            entity.IsUnAcknowledge = false;
            entity.UnAcknowledgeBy = 0;
            entity.UnAcknowledgeDate = null;

            entity.EntityState = EntityState.Modified;

            await _service.SaveAsync(entity);
            return Ok();
        }
        */

        [Route("acknowledge/{id}/{requisitionType}")]
        [HttpPost]
        public async Task<IActionResult> Acknowledge(int id, string requisitionType = null)
        {
            if (requisitionType.IsNotNull())
            {
                requisitionType = CommonFunction.ReplaceInvalidChar(requisitionType);
                requisitionType = requisitionType.Trim() == "R&D" ? "RnD" : requisitionType;
            }

            if (requisitionType == null || requisitionType == "RnD")
            {
                YarnRnDReqMaster entity = await _service.GetDetailsAsync(id);
                entity.Childs.SetUnchanged();

                entity.IsAcknowledge = true;
                entity.AcknowledgeBy = AppUser.EmployeeCode;
                entity.AcknowledgeDate = DateTime.Now;
                entity.IsUnAcknowledge = false;
                entity.UnAcknowledgeBy = 0;
                entity.UnAcknowledgeDate = null;

                entity.EntityState = EntityState.Modified;

                await _service.SaveAsync(entity, AppUser.UserCode);
            }
            else if (requisitionType == "Bulk")
            {
                KYReqMaster entity = await _kyService.GetDetailsAsync(id);
                entity.Childs.SetUnchanged();

                entity.Acknowledge = true;
                entity.AcknowledgeBy = AppUser.EmployeeCode;
                entity.AcknowledgeDate = DateTime.Now;

                entity.UnAcknowledge = false;
                entity.UnAcknowledgeBy = 0;
                entity.UnAcknowledgeDate = null;

                entity.EntityState = EntityState.Modified;

                await _kyService.SaveAsync(entity);
            }
            return Ok();
        }

        [Route("unacknowledge/{id}/{unAcknowledgeReason}/{requisitionType}")]
        [HttpPost]
        public async Task<IActionResult> UnAcknowledge(int id, string unAcknowledgeReason, string requisitionType = null)
        {
            if (requisitionType == null || requisitionType == "RnD")
            {
                YarnRnDReqMaster entity;
                entity = await _service.GetDetailsAsync(id);

                entity.IsUnAcknowledge = true;
                entity.UnAcknowledgeBy = AppUser.EmployeeCode;
                entity.UnAcknowledgeDate = DateTime.Now;
                entity.UnAcknowledgeReason = unAcknowledgeReason;

                entity.IsAcknowledge = false;
                entity.AcknowledgeBy = 0;
                entity.AcknowledgeDate = null;

                entity.EntityState = EntityState.Modified;

                entity.Childs.SetUnchanged();

                await _service.SaveAsync(entity, AppUser.UserCode);
            }
            else if (requisitionType == "Bulk")
            {
                KYReqMaster entity = await _kyService.GetDetailsAsync(id);
                entity.Childs.SetUnchanged();

                entity.UnAcknowledge = true;
                entity.UnAcknowledgeBy = AppUser.EmployeeCode;
                entity.UnAcknowledgeDate = DateTime.Now;
                entity.UnAcknowledgeReason = unAcknowledgeReason;

                entity.Acknowledge = false;
                entity.AcknowledgeBy = 0;
                entity.AcknowledgeDate = null;

                entity.EntityState = EntityState.Modified;

                entity.Childs.SetUnchanged();

                await _kyService.SaveAsync(entity);
            }
            return Ok();
        }

        [Route("revise")]
        [HttpPost]
        public async Task<IActionResult> Revise(YarnRnDReqMaster model)
        {
            YarnRnDReqMaster entity;
            string conceptNo = string.Join(",", model.Childs.Select(x => x.ConceptNo).Distinct());

            entity = await _service.GetDetailsForReviseAsync(model.RnDReqMasterID);

            entity.ConceptNo = conceptNo;
            entity.RnDReqDate = model.RnDReqDate;
            entity.RCompanyID = model.RCompanyID;
            entity.IsReqForYD = model.IsReqForYD;
            entity.UpdatedBy = AppUser.UserCode;
            entity.DateUpdated = DateTime.Now;
            entity.EntityState = EntityState.Modified;
            //entity.PreProcessRevNo = entity.RevisionNo;
            entity.RevisionNo = entity.RevisionNo + 1;
            entity.RevisionDate = DateTime.Now;
            entity.RevisionBy = AppUser.UserCode;
            entity.RevisionReason = model.RevisionReason;

            entity.IsApprove = false;
            entity.ApproveDate = null;
            entity.ApproveBy = 0;

            entity.IsAcknowledge = false;
            entity.AcknowledgeDate = null;
            entity.AcknowledgeBy = 0;

            entity.Childs.SetUnchanged();

            foreach (YarnRnDReqChild mChild in model.Childs)
            {
                if (mChild.RnDReqChildID == 0)
                {
                    mChild.EntityState = EntityState.Added;
                    mChild.PreProcessRevNo = entity.PreProcessRevNo;
                    entity.Childs.Add(mChild);
                }
                else
                {
                    YarnRnDReqChild childEntity = entity.Childs.FirstOrDefault(x => x.RnDReqChildID == mChild.RnDReqChildID);
                    if (childEntity == null)
                    {
                        mChild.EntityState = EntityState.Added;
                        mChild.PreProcessRevNo = entity.PreProcessRevNo;
                        entity.Childs.Add(mChild);
                    }
                    else
                    {
                        childEntity.YarnBrandID = mChild.YarnBrandID;
                        childEntity.BatchNo = mChild.BatchNo;
                        childEntity.ReqQty = mChild.ReqQty;
                        childEntity.ReqCone = mChild.ReqCone;
                        childEntity.Remarks = mChild.Remarks;
                        childEntity.PhysicalCount = mChild.PhysicalCount;
                        childEntity.YarnLotNo = mChild.YarnLotNo;
                        childEntity.PreProcessRevNo = entity.PreProcessRevNo;
                        childEntity.ItemMasterID = mChild.ItemMasterID;
                        childEntity.StockTypeId = mChild.StockTypeId;
                        childEntity.EntityState = EntityState.Modified;
                    }
                }
            }

            entity.Childs.Where(x => x.EntityState == EntityState.Unchanged).ToList().ForEach(x =>
            {
                x.EntityState = EntityState.Deleted;
            });

            await _service.ReviseAsync(entity, AppUser.UserCode);
            return Ok();
        }
    }
}
