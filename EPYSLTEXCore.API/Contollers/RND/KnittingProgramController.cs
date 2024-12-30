using EPYSLTEX.Core.Interfaces;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEX.Infrastructure.Services;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.API.Extends.Filters;
using EPYSLTEXCore.Application.Interfaces;
using EPYSLTEXCore.Application.Interfaces.Inventory.Yarn;
using EPYSLTEXCore.Application.Interfaces.RND;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities.General;
using EPYSLTEXCore.Infrastructure.Entities.Gmt.General.Item;
using EPYSLTEXCore.Infrastructure.Entities.Knitting;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Knitting;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System.Data.Entity;
using System.Text.RegularExpressions;

namespace EPYSLTEXCore.API.Contollers.RND
{
    [Route("api/knitting-program")]
    public class KnittingProgramController : ApiBaseController
    {
        private readonly IKnittingProgramService _service;
        private readonly IYarnStockAdjustmentService _serviceYSS;
        private readonly IMemoryCache _memoryCache;
        private readonly ICommonHelperService _commonHelperService;

        public KnittingProgramController(IKnittingProgramService knittingPlanService
            , IYarnStockAdjustmentService serviceYSS
            , IMemoryCache memoryCache
            , ICommonHelperService commonHelperService, IUserService userService) : base(userService)
        {
            _service = knittingPlanService;
            _serviceYSS = serviceYSS;
            _memoryCache = memoryCache;
            _commonHelperService = commonHelperService;
        }

        [Route("list")]
        [HttpGet]
        public async Task<IActionResult> GetList(KnittingProgramType type, Status status)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<KnittingPlanMaster> records = await _service.GetPagedAsync(type, status, paginationInfo, AppUser);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }
        [Route("list/group")]
        [HttpGet]
        public async Task<IActionResult> GetListGroup(KnittingProgramType type, Status status)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<KnittingPlanMaster> records = await _service.GetPagedAsync(type, status, paginationInfo, AppUser, true);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }
        [HttpGet]
        [Route("new-kp/{conceptId}/{isBulkPage}/{withoutOB}/{subGroupName}")]
        public async Task<IActionResult> GetNew(int conceptId, bool isBulkPage, bool withoutOB, string subGroupName)
        {
            var list = await _service.GetNewAsync(conceptId, isBulkPage, withoutOB, subGroupName);
            list.Yarns = await this.GetYarnStockSets(list.Yarns);
            return Ok(list);
        }
        private async Task<List<KnittingPlanYarn>> GetYarnStockSets(List<KnittingPlanYarn> yarns)
        {
            string yarnStockSetIds = string.Join(",", yarns.Where(x => x.YarnStockSetId > 0).Select(x => x.YarnStockSetId).Distinct());
            if (yarnStockSetIds.IsNullOrEmpty()) return yarns;
            List<YarnStockAdjustmentMaster> list = new List<YarnStockAdjustmentMaster>();
            list = await _serviceYSS.GetAllStocks($@" AND YarnStockSetId IN ({yarnStockSetIds})");
            yarns.Where(x => x.YarnStockSetId > 0).ToList().ForEach(y =>
            {
                y.YarnStockSet = list.Find(x => x.YarnStockSetId == y.YarnStockSetId);
            });
            return yarns;
        }
        [HttpGet]
        [Route("new-kp/group/{conceptIds}/{isBulkPage}/{withoutOB}/{subGroupName}")]
        public async Task<IActionResult> GetNewGroup(string conceptIds, bool isBulkPage, bool withoutOB, string subGroupName)
        {
            return Ok(await _service.GetNewGroupAsync(conceptIds, isBulkPage, withoutOB, subGroupName));
        }
        [HttpGet]
        [Route("new/{type}/{id}/{itemMasterId}")]
        public async Task<IActionResult> GetNew(KnittingProgramType type, int id, int itemMasterId)
        {
            return Ok(await _service.GetNewAsync(type, id, itemMasterId));
        }
        [Route("booking-child-list/{type}")]
        [HttpGet]
        public async Task<IActionResult> GetBookingChildList(KnittingProgramType type)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<KnittingPlanBookingChildDTO> records = await _service.GetBookingChildsAsync(type, paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }
        [Route("booking-child-details-list/{status}/{type}/{bookingId?}")]
        [HttpGet]
        public async Task<IActionResult> GetBookingChildDetailsList(Status status, KnittingProgramType type, int bookingId = 0)
        {
            var paginationInfo = Request.GetPaginationInfo();
            if (type == KnittingProgramType.BDS) paginationInfo.FilterBy += $@" AND BookingID = {bookingId}";
            List<KnittingPlanBookingChildDetailsDTO> records = await _service.GetBookingChildsDetailsAsync(status, type, paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        [Route("{type}/{id}/{subgroupName}")]
        [HttpGet]
        public async Task<IActionResult> Get(KnittingProgramType type, int id, string subgroupName)
        {
            var list = await _service.GetAsync(type, id, subgroupName);
            list.Yarns = await this.GetYarnStockSets(list.Yarns);
            return Ok(list);
        }
        [Route("group/{groupConceptNo}/{planNo}/{type}/{subgroupName}")]
        [HttpGet]
        public async Task<IActionResult> GetGroup(string groupConceptNo, int planNo, KnittingProgramType type, string subgroupName)
        {
            return Ok(await _service.GetGroupAsync(groupConceptNo, planNo, type, subgroupName));
        }
        [Route("group/addition/{groupConceptNo}/{planNo}/{type}/{subgroupName}")]
        [HttpGet]
        public async Task<IActionResult> GetAdditionGroup(string groupConceptNo, int planNo, KnittingProgramType type, string subgroupName)
        {
            return Ok(await _service.GetAdditionGroupAsync(groupConceptNo, planNo, type, subgroupName));
        }
        [HttpGet]
        [Route("get-machine-by-gauge-dia-finder/{MachineGauge}/{MachineDia}")]
        public async Task<IActionResult> GetMachineByGaugeDia(int MachineGauge, int MachineDia)
        {
            var paginationInfo = Request.GetPaginationInfo();
            var records = await _service.GetMachineByGaugeDia(MachineGauge, MachineDia);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }
        [Route("revision/{id}/{conceptID}/{subGroupName}")]
        [HttpGet]
        public async Task<IActionResult> GetRevisionedList(int id, int conceptID, string subGroupName)
        {
            KnittingPlanMaster record = await _service.GetRevisionedListAsync(id, conceptID, subGroupName);
            Guard.Against.NullObject(id, record);
            record.Yarns = await this.GetYarnStockSets(record.Yarns);
            return Ok(record);
        }
        [Route("group/revision/{groupId}/{groupConceptNo}/{subGroupName}")]
        [HttpGet]
        public async Task<IActionResult> GetGroupRevisionedListAsync(int groupId, string groupConceptNo, string subGroupName)
        {
            KnittingPlanGroup record = await _service.GetGroupRevisionedListAsync(groupId, groupConceptNo, subGroupName);
            Guard.Against.NullObject(groupId, record);
            return Ok(record);
        }
        [Route("list-by-mcsubclass")]
        [HttpGet]
        public async Task<IActionResult> GetListByMCSubClass(KnittingProgramType type, int mcSubClassId)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<KnittingPlanMaster> records = await _service.GetListByMCSubClass(type, mcSubClassId, paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }
        [Route("childs")]
        [HttpGet]
        public async Task<IActionResult> GetChilds(int masterID, int subGroupID, int conceptID)
        {
            return Ok(await _service.GetChildsAsync(masterID, subGroupID, conceptID));
        }
        [Route("save")]
        [HttpPost]
        public async Task<IActionResult> Save()
        {
            var formData = Request.Form;
            var file = Request.Form.Files.FirstOrDefault();
            //if (!Request.HasFormContentType.IsMimeMultipartContent()) return BadRequest("Unsupported media type.");

            KnittingPlanMaster model = formData.ConvertToObject<KnittingPlanMaster>();
            model.Yarns = JsonConvert.DeserializeObject<List<KnittingPlanYarn>>(formData["Yarns"]);
            model.Childs = JsonConvert.DeserializeObject<List<KnittingPlanChild>>(formData["Childs"]);

            #region Save Image

            var filePath = "";
            var previewTemplate = "";
            if (file != null)
            {
                if (file.Length > 4 * 1024 * 1024)
                    return BadRequest("File is bigger than 4MB.");
                var originalFile = file;
                var inputStream = originalFile.OpenReadStream();

                var fileName = string.Join("", originalFile.FileName.Split(Path.GetInvalidFileNameChars()));
                fileName = GetValidFileName(fileName);
                var contentType = originalFile.ContentType;

                var fileExtension = Path.GetExtension(fileName);
                var provider = new FileExtensionContentTypeProvider();
                string mimeType;

                if (provider.TryGetContentType(fileName, out mimeType))
                {
                    // Set previewTemplate based on file type
                    previewTemplate = fileExtension.Contains(".pdf") ? "pdf" :
                                      mimeType.StartsWith("image/") ? "image" :
                                      "office";
                }
                else
                {
                    // If MIME type couldn't be determined, fallback to "office"
                    previewTemplate = "office";
                }

                fileName = DateTime.Now.ToString("yyyyMMdd'T'HHmmss");
                filePath = $"{AppConstants.KNITTING_PROGRAM_PATH}/{fileName}{fileExtension}";
                var savePath = Path.GetDirectoryName(filePath); //HttpContext.Current.Server.MapPath(filePath);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await inputStream.CopyToAsync(fileStream);
                }
            }
            else
            {
                filePath = "defaultFilePath";  // Handle no file uploaded scenario
                previewTemplate = "office";    // Set default preview template
            }

            #endregion Save Image

            KnittingPlanMaster entity;
            if (model.IsModified)
            {
                entity = await _service.GetDetailsAsync(model.KPMasterID);
                List<KnittingPlanYarn> kpYarnsFCMR = await _service.GetYarnsForFCMRChild(entity.ConceptID.ToString(), false, false, "");

                entity.ReqDeliveryDate = model.ReqDeliveryDate;
                entity.UpdatedBy = AppUser.UserCode;;
                entity.DateUpdated = DateTime.Now;
                entity.FilePath = filePath;
                entity.AttachmentPreviewTemplate = previewTemplate;
                entity.IsSubContact = model.IsSubContact;
                entity.MCSubClassID = model.MCSubClassID;
                entity.NeedPreFinishingProcess = model.NeedPreFinishingProcess;

                if (model.IsRevise)
                {
                    entity.RevisionNo = entity.RevisionNo + 1;
                    entity.RevisionDate = DateTime.Now;
                    entity.RevisionBy = AppUser.UserCode;;
                    entity.RevisionReason = "";
                }

                entity.EntityState = EntityState.Modified;

                entity.Childs.SetUnchanged();
                entity.Childs.ToList().ForEach(x => x.KJobCardMasters.SetUnchanged());

                decimal oldPlanQty = entity.Childs.Sum(x => x.BookingQty);
                decimal planQty = 0;
                KJobCardMaster jobCardMaster;
                foreach (KnittingPlanChild item in model.Childs)
                {
                    KnittingPlanChild knittingPlanChild = entity.Childs.FirstOrDefault(x => x.KPChildID == item.KPChildID);
                    if (knittingPlanChild == null)
                    {
                        knittingPlanChild = item;
                        if (knittingPlanChild.SubGroupID != 1) knittingPlanChild.UnitID = 1; //Pcs
                        knittingPlanChild.KJobCardQty = (int)knittingPlanChild.KJobCardMasters.Sum(x => x.KJobCardQty);
                        entity.Childs.Add(knittingPlanChild);
                        planQty += knittingPlanChild.BookingQty;
                    }
                    else
                    {
                        planQty += knittingPlanChild.BookingQty;

                        knittingPlanChild.MCSubClassID = item.MCSubClassID;
                        knittingPlanChild.FabricGsm = item.FabricGsm;
                        knittingPlanChild.FabricWidth = item.FabricWidth;
                        knittingPlanChild.ItemMasterID = item.ItemMasterID;
                        knittingPlanChild.MachineDia = item.MachineDia;
                        knittingPlanChild.MachineGauge = item.MachineGauge;
                        knittingPlanChild.CCColorID = item.CCColorID;
                        knittingPlanChild.StartDate = item.StartDate;
                        knittingPlanChild.EndDate = item.EndDate;
                        knittingPlanChild.ActualStartDate = DateTime.Now;
                        knittingPlanChild.ActualEndDate = DateTime.Now;
                        knittingPlanChild.BookingQty = item.BookingQty;
                        knittingPlanChild.KnittingTypeID = item.KnittingTypeID;
                        knittingPlanChild.Needle = item.Needle;
                        knittingPlanChild.CPI = item.CPI;
                        knittingPlanChild.TotalNeedle = item.TotalNeedle;
                        knittingPlanChild.TotalCourse = item.TotalCourse;
                        if (knittingPlanChild.SubGroupID != 1) knittingPlanChild.UnitID = 1; //Pcs
                        knittingPlanChild.EntityState = EntityState.Modified;

                        foreach (KJobCardMaster jobCard in item.KJobCardMasters)
                        {
                            jobCardMaster = knittingPlanChild.KJobCardMasters.FirstOrDefault(y => y.KJobCardMasterID == jobCard.KJobCardMasterID);
                            if (jobCardMaster == null)
                            {
                                jobCardMaster = jobCard;

                                jobCard.ItemMasterID = item.ItemMasterID;
                                jobCard.BAnalysisChildID = item.BAnalysisChildID;
                                jobCard.IsSubContact = entity.IsSubContact;
                                jobCard.ExportOrderID = entity.ExportOrderID;

                                jobCardMaster.KnittingMachineID = item.KnittingMachineID;
                                jobCardMaster.MachineDia = item.MachineDia;
                                jobCardMaster.MachineGauge = item.MachineGauge;

                                if (jobCardMaster.SubGroupID != 1) jobCardMaster.UnitID = 1; //Pcs

                                jobCard.Childs = new List<KJobCardChild>();
                                jobCard.Childs.Add(new KJobCardChild()
                                {
                                    KJobCardMasterID = jobCard.KJobCardMasterID,
                                    KPChildID = knittingPlanChild.KPChildID,
                                    ProdQty = item.ProdQty,
                                    ProdQtyPcs = item.ProdQtyPcs
                                });

                                knittingPlanChild.KJobCardMasters.Add(jobCardMaster);
                            }
                            else
                            {
                                jobCardMaster.ConceptID = jobCard.ConceptID;
                                jobCardMaster.KJobCardDate = jobCard.KJobCardDate;
                                jobCardMaster.BrandID = jobCard.BrandID;
                                jobCardMaster.IsSubContact = jobCard.IsSubContact;
                                jobCardMaster.ContactID = jobCard.ContactID;

                                jobCardMaster.KnittingMachineID = item.KnittingMachineID;
                                jobCardMaster.MachineDia = item.MachineDia;
                                jobCardMaster.MachineGauge = item.MachineGauge;

                                jobCardMaster.KJobCardQty = jobCard.KJobCardQty;
                                jobCardMaster.Remarks = jobCard.Remarks;
                                jobCardMaster.UpdatedBy = AppUser.UserCode;;
                                jobCardMaster.DateUpdated = DateTime.Now;
                                if (jobCardMaster.SubGroupID != 1) jobCardMaster.UnitID = 1; //Pcs
                                jobCardMaster.EntityState = EntityState.Modified;

                                jobCardMaster.Childs.ForEach(c =>
                                {
                                    c.ProdQty = item.ProdQty;
                                    c.ProdQtyPcs = item.ProdQtyPcs;
                                    c.EntityState = EntityState.Modified;
                                });
                            }
                        }
                        knittingPlanChild.KJobCardQty = (int)knittingPlanChild.KJobCardMasters.Sum(x => x.KJobCardQty);
                    }
                }
                entity.PlanQty = model.PlanQty;

                entity.Yarns.SetUnchanged();
                entity.Yarns.ForEach(y =>
                {
                    y.Childs.SetUnchanged();
                });

                foreach (KnittingPlanYarn yarnItem in model.Yarns)
                {
                    KnittingPlanYarn existingYarn = entity.Yarns.FirstOrDefault(x => x.KPYarnID == yarnItem.KPYarnID);
                    if (existingYarn == null)
                    {
                        KnittingPlanYarn knittingPlanYarn = new KnittingPlanYarn()
                        {
                            PhysicalCount = yarnItem.PhysicalCount,
                            YarnTypeID = yarnItem.YarnTypeID,
                            FCMRChildID = yarnItem.FCMRChildID,
                            YarnCountID = yarnItem.YarnCountID,
                            YarnLotNo = yarnItem.YarnLotNo,
                            YarnBrandID = yarnItem.YarnBrandID,
                            YarnPly = yarnItem.YarnPly,
                            StitchLength = yarnItem.StitchLength,
                            BatchNo = yarnItem.BatchNo,
                            ItemMasterID = yarnItem.ItemMasterID,
                            YDItem = yarnItem.YDItem,
                            YarnStockSetId = yarnItem.YarnStockSetId,
                            EntityState = EntityState.Added
                        };

                        #region KnittingPlanYarnChild

                        List<KnittingPlanYarn> yarnFCMRs = kpYarnsFCMR.Where(x => x.ConceptID == entity.ConceptID &&
                                                                                          x.YarnCountID == yarnItem.YarnCountID &&
                                                                                          x.YarnTypeID == yarnItem.YarnTypeID).ToList();

                        yarnFCMRs.ForEach(yc =>
                        {
                            KnittingPlanYarnChild yarnChild = new KnittingPlanYarnChild()
                            {
                                KPYarnChildID = 0,
                                KPYarnID = knittingPlanYarn.YarnTypeID,
                                FCMRChildID = yarnItem.FCMRChildID,
                                ConceptID = entity.ConceptID,
                                ItemMasterID = yarnItem.ItemMasterID,
                                ReqQty = yc.TotalQty != 0 ? (yc.ReqQty * entity.PlanQty) / yc.TotalQty : 0,
                                EntityState = EntityState.Added
                            };
                            knittingPlanYarn.Childs.Add(yarnChild);
                        });
                        #endregion

                        entity.Yarns.Add(knittingPlanYarn);
                    }
                    else
                    {
                        existingYarn.PhysicalCount = yarnItem.PhysicalCount;
                        existingYarn.YarnTypeID = yarnItem.YarnTypeID;
                        existingYarn.FCMRChildID = yarnItem.FCMRChildID;
                        existingYarn.YarnCountID = yarnItem.YarnCountID;
                        existingYarn.YarnLotNo = yarnItem.YarnLotNo;
                        existingYarn.YarnBrandID = yarnItem.YarnBrandID;
                        existingYarn.YarnPly = yarnItem.YarnPly;
                        existingYarn.StitchLength = yarnItem.StitchLength;
                        existingYarn.BatchNo = yarnItem.BatchNo;
                        existingYarn.ItemMasterID = yarnItem.ItemMasterID;
                        existingYarn.YDItem = yarnItem.YDItem;
                        existingYarn.YarnStockSetId = yarnItem.YarnStockSetId;
                        existingYarn.EntityState = EntityState.Modified;

                        foreach (KnittingPlanYarnChild kpyc in existingYarn.Childs)
                        {
                            KnittingPlanYarnChild existingKpyc = existingYarn.Childs.FirstOrDefault(x => x.KPYarnChildID == kpyc.KPYarnChildID);
                            if (existingKpyc == null)
                            {
                                List<KnittingPlanYarn> yarnFCMRs = kpYarnsFCMR.Where(x => x.ConceptID == entity.ConceptID &&
                                                                                             x.YarnCountID == existingYarn.YarnCountID &&
                                                                                             x.YarnTypeID == existingYarn.YarnTypeID).ToList();

                                yarnFCMRs.ForEach(yc =>
                                {
                                    KnittingPlanYarnChild yarnChild = new KnittingPlanYarnChild()
                                    {
                                        KPYarnChildID = 0,
                                        KPYarnID = existingYarn.YarnTypeID,
                                        FCMRChildID = yarnItem.FCMRChildID,
                                        ConceptID = entity.ConceptID,
                                        ItemMasterID = yarnItem.ItemMasterID,
                                        ReqQty = yc.TotalQty != 0 ? (yc.ReqQty * entity.PlanQty) / yc.TotalQty : 0,
                                        EntityState = EntityState.Added
                                    };
                                    existingYarn.Childs.Add(yarnChild);
                                });
                            }
                            else
                            {
                                kpyc.EntityState = EntityState.Modified;
                            }
                        }

                        if (existingYarn.Childs.Count() == 0)
                        {
                            List<KnittingPlanYarn> yarnFCMRs = kpYarnsFCMR.Where(x => x.ConceptID == entity.ConceptID &&
                                                                                                 x.YarnCountID == existingYarn.YarnCountID &&
                                                                                                 x.YarnTypeID == existingYarn.YarnTypeID).ToList();

                            yarnFCMRs.ForEach(yc =>
                            {
                                KnittingPlanYarnChild yarnChild = new KnittingPlanYarnChild()
                                {
                                    KPYarnChildID = 0,
                                    KPYarnID = existingYarn.YarnTypeID,
                                    FCMRChildID = yarnItem.FCMRChildID,
                                    ConceptID = entity.ConceptID,
                                    ItemMasterID = yarnItem.ItemMasterID,
                                    ReqQty = yc.TotalQty != 0 ? (yc.ReqQty * entity.PlanQty) / yc.TotalQty : 0,
                                    EntityState = EntityState.Added
                                };
                                existingYarn.Childs.Add(yarnChild);
                            });
                        }

                        //existingYarn.Childs.ForEach(yc =>
                        //{
                        //    yc.EntityState = EntityState.Modified;
                        //});
                    }
                }

                //entity.Yarns.Where(x => x.EntityState == EntityState.Unchanged).ToList().SetDeleted();
                entity.Yarns.ForEach(y =>
                {
                    if (y.EntityState == EntityState.Unchanged)
                    {
                        y.EntityState = EntityState.Deleted;
                    }
                    y.Childs.ForEach(yc =>
                    {
                        if (yc.EntityState == EntityState.Unchanged)
                        {
                            yc.EntityState = EntityState.Deleted;
                        }
                    });
                });

                #region Knitting Plan Group

                if (entity.KPGroup.GroupID > 0)
                {
                    entity.KPGroup.UpdatedBy = AppUser.UserCode;;
                    entity.KPGroup.DateUpdated = DateTime.Now;
                    entity.KPGroup.EntityState = EntityState.Modified;
                }
                else
                {
                    entity.KPGroup.AddedBy = AppUser.UserCode;;
                    entity.KPGroup.DateAdded = DateTime.Now;
                    entity.KPGroup.EntityState = EntityState.Added;
                }
                entity.KPGroup.IsSubContact = entity.IsSubContact;

                var validChilds = entity.Childs.Where(x => x.EntityState == EntityState.Added || x.EntityState == EntityState.Modified).ToList();
                if (validChilds.Count() > 0)
                {
                    var childItemF = validChilds.First();
                    var childItemL = validChilds.Last();

                    entity.KPGroup.MachineDia = childItemF.MachineDia;
                    entity.KPGroup.MachineGauge = childItemF.MachineGauge;
                    entity.KPGroup.BrandID = childItemF.BrandID;
                    entity.KPGroup.StartDate = childItemF.StartDate;
                    entity.KPGroup.EndDate = childItemL.EndDate;
                    entity.KPGroup.KnittingTypeID = childItemF.KnittingTypeID;
                }
                #endregion Knitting Plan Group

                await _service.SaveAsync(entity, model.KnittingProgramType, oldPlanQty);
            }
            else
            {
                entity = model;
                entity.AddedBy = AppUser.UserCode;;
                entity.DateAdded = DateTime.Now;
                entity.FilePath = filePath;
                entity.AttachmentPreviewTemplate = previewTemplate;
                entity.Childs.ForEach(child =>
                {
                    entity.SubGroupID = entity.SubGroupID == 0 && child.SubGroupID > 0 ? child.SubGroupID : entity.SubGroupID;
                    child.KJobCardQty = (int)child.KJobCardMasters.Sum(x => x.KJobCardQty);
                    if (child.SubGroupID != 1) child.UnitID = 1; //Pcs
                    child.KJobCardMasters.ForEach(jobCard =>
                    {
                        if (jobCard.SubGroupID != 1) jobCard.UnitID = 1; //Pcs

                        jobCard.ItemMasterID = child.ItemMasterID;
                        jobCard.BAnalysisChildID = child.BAnalysisChildID;
                        jobCard.IsSubContact = entity.IsSubContact;
                        jobCard.ExportOrderID = entity.ExportOrderID;

                        jobCard.KnittingMachineID = child.KnittingMachineID;
                        jobCard.MachineDia = child.MachineDia;
                        jobCard.MachineGauge = child.MachineGauge;
                        jobCard.AddedBy = AppUser.UserCode;;
                        jobCard.DateAdded = DateTime.Now;

                        jobCard.Childs = new List<KJobCardChild>();
                        jobCard.Childs.Add(new KJobCardChild()
                        {
                            KJobCardMasterID = jobCard.KJobCardMasterID,
                            KPChildID = child.KPChildID,
                            ProdQty = child.ProdQty,
                            ProdQtyPcs = child.ProdQtyPcs
                        });
                    });
                });

                #region KnittingPlanYarnChild
                KnittingPlanYarnChild yarnChild = new KnittingPlanYarnChild();
                entity.Yarns.ForEach(y =>
                {
                    yarnChild = new KnittingPlanYarnChild();
                    yarnChild.KPYarnChildID = 0;
                    yarnChild.KPYarnID = y.KPYarnID;
                    yarnChild.FCMRChildID = y.FCMRChildID;
                    yarnChild.ConceptID = entity.ConceptID;
                    yarnChild.ItemMasterID = y.ItemMasterID;
                    yarnChild.ReqQty = y.TotalQty != 0 ? (y.ReqQty * entity.TotalQty) / y.TotalQty : 0;
                    y.Childs.Add(yarnChild);
                });
                #endregion

                await _service.SaveAsync(entity, model.KnittingProgramType, 0);
            }
            //await _BatchWiseProductionService.UpdateBDSTNA_DyeingPlanAsync(entity.DBatchID);
            await _service.UpdateFreeConceptMasterAsync(entity.ConceptID, entity.MCSubClassID);
            //await _service.ExecuteAsync("spUpdateFreeConceptMaster", new { entity.MCSubClassID, ConceptID = entity.ConceptID }, 30, CommandType.StoredProcedure);
            return Ok();
        }

        [Route("revise")]
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> Revise()
        {
            var formData = Request.Form;
            var file = Request.Form.Files.FirstOrDefault();
            //if (!Request.Content.IsMimeMultipartContent()) return BadRequest("Unsupported media type.");

            KnittingPlanMaster model = formData.ConvertToObject<KnittingPlanMaster>();
            model.Yarns = JsonConvert.DeserializeObject<List<KnittingPlanYarn>>(formData["Yarns"]);
            model.Childs = JsonConvert.DeserializeObject<List<KnittingPlanChild>>(formData["Childs"]);

            #region Save Image

            var filePath = "";
            var previewTemplate = "";
            if (file != null)
            {
                if (file.Length > 4 * 1024 * 1024)
                    return BadRequest("File is bigger than 4MB.");
                var originalFile = file;
                var inputStream = originalFile.OpenReadStream();

                var fileName = string.Join("", originalFile.FileName.Split(Path.GetInvalidFileNameChars()));
                fileName = GetValidFileName(fileName);
                var contentType = originalFile.ContentType;

                var fileExtension = Path.GetExtension(fileName);
                var provider = new FileExtensionContentTypeProvider();
                string mimeType;

                if (provider.TryGetContentType(fileName, out mimeType))
                {
                    // Set previewTemplate based on file type
                    previewTemplate = fileExtension.Contains(".pdf") ? "pdf" :
                                      mimeType.StartsWith("image/") ? "image" :
                                      "office";
                }
                else
                {
                    // If MIME type couldn't be determined, fallback to "office"
                    previewTemplate = "office";
                }

                fileName = DateTime.Now.ToString("yyyyMMdd'T'HHmmss");
                filePath = $"{AppConstants.KNITTING_PROGRAM_PATH}/{fileName}{fileExtension}";
                var savePath = Path.GetDirectoryName(filePath); //HttpContext.Current.Server.MapPath(filePath);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await inputStream.CopyToAsync(fileStream);
                }
            }
            else
            {
                filePath = "defaultFilePath";  // Handle no file uploaded scenario
                previewTemplate = "office";    // Set default preview template
            }

            #endregion Save Image


            KnittingPlanMaster entity = await _service.GetDetailsAsync(model.KPMasterID);
            List<KnittingPlanYarn> kpYarnsFCMR = await _service.GetYarnsForFCMRChild(entity.ConceptID.ToString(), false, false, "");

            entity.ReqDeliveryDate = model.ReqDeliveryDate;
            entity.UpdatedBy = AppUser.UserCode;;
            entity.DateUpdated = DateTime.Now;

            entity.PreProcessRevNo = model.PreProcessRevNo;
            //entity.RevisionNo = model.RevisionNo + 1;
            entity.RevisionDate = DateTime.Now;
            entity.RevisionBy = AppUser.UserCode;;
            entity.RevisionReason = "";
            entity.IsSubContact = model.IsSubContact;
            entity.MCSubClassID = model.MCSubClassID;
            entity.NeedPreFinishingProcess = model.NeedPreFinishingProcess;
            entity.EntityState = EntityState.Modified;

            entity.Childs.SetUnchanged();
            entity.Childs.ToList().ForEach(x => x.KJobCardMasters.SetUnchanged());

            decimal oldPlanQty = entity.Childs.Sum(x => x.BookingQty);
            decimal planQty = 0;
            KJobCardMaster jobCardMaster;
            foreach (KnittingPlanChild item in model.Childs)
            {
                KnittingPlanChild knittingPlanChild = entity.Childs.FirstOrDefault(x => x.KPChildID == item.KPChildID);
                if (knittingPlanChild == null)
                {
                    knittingPlanChild = item;
                    if (knittingPlanChild.SubGroupID != 1) knittingPlanChild.UnitID = 1; //Pcs
                    knittingPlanChild.KJobCardQty = (int)knittingPlanChild.KJobCardMasters.Sum(x => x.KJobCardQty);
                    knittingPlanChild.PlanNo = entity.PlanNo;
                    entity.Childs.Add(knittingPlanChild);
                    planQty += knittingPlanChild.BookingQty;
                }
                else
                {
                    planQty += knittingPlanChild.BookingQty;

                    knittingPlanChild.MCSubClassID = item.MCSubClassID;
                    knittingPlanChild.FabricGsm = item.FabricGsm;
                    knittingPlanChild.FabricWidth = item.FabricWidth;
                    knittingPlanChild.MachineDia = item.MachineDia;
                    knittingPlanChild.MachineGauge = item.MachineGauge;
                    knittingPlanChild.CCColorID = item.CCColorID;
                    knittingPlanChild.StartDate = item.StartDate;
                    knittingPlanChild.EndDate = item.EndDate;
                    knittingPlanChild.ActualStartDate = DateTime.Now;
                    knittingPlanChild.ActualEndDate = DateTime.Now;
                    knittingPlanChild.BookingQty = item.BookingQty;
                    knittingPlanChild.KnittingTypeID = item.KnittingTypeID;
                    knittingPlanChild.Needle = item.Needle;
                    knittingPlanChild.CPI = item.CPI;
                    knittingPlanChild.TotalNeedle = item.TotalNeedle;
                    knittingPlanChild.TotalCourse = item.TotalCourse;
                    knittingPlanChild.PlanNo = entity.PlanNo;
                    if (knittingPlanChild.SubGroupID != 1) knittingPlanChild.UnitID = 1; //Pcs
                    knittingPlanChild.EntityState = EntityState.Modified;

                    foreach (KJobCardMaster jobCard in item.KJobCardMasters)
                    {
                        jobCardMaster = knittingPlanChild.KJobCardMasters.FirstOrDefault(y => y.KJobCardMasterID == jobCard.KJobCardMasterID);
                        if (jobCardMaster == null)
                        {
                            jobCardMaster = jobCard;
                            jobCard.AddedBy = AppUser.UserCode;;
                            jobCard.DateAdded = DateTime.Now;
                            if (jobCardMaster.SubGroupID != 1) jobCardMaster.UnitID = 1; //Pcs
                            knittingPlanChild.KJobCardMasters.Add(jobCardMaster);
                        }
                        else
                        {
                            jobCardMaster.ConceptID = jobCard.ConceptID;
                            jobCardMaster.KJobCardDate = jobCard.KJobCardDate;
                            jobCardMaster.BrandID = jobCard.BrandID;
                            jobCardMaster.IsSubContact = jobCard.IsSubContact;
                            jobCardMaster.ContactID = jobCard.ContactID;
                            jobCardMaster.MachineDia = jobCard.MachineDia;
                            jobCardMaster.KnittingMachineID = jobCard.KnittingMachineID;

                            jobCardMaster.ItemMasterID = item.ItemMasterID;
                            jobCardMaster.BAnalysisChildID = item.BAnalysisChildID;
                            jobCardMaster.ExportOrderID = model.ExportOrderID;

                            jobCardMaster.KJobCardQty = jobCard.KJobCardQty;
                            jobCardMaster.Remarks = jobCard.Remarks;
                            jobCardMaster.GroupID = entity.PlanNo;
                            jobCardMaster.UpdatedBy = AppUser.UserCode;;
                            jobCardMaster.DateUpdated = DateTime.Now;
                            if (jobCardMaster.SubGroupID != 1) jobCardMaster.UnitID = 1; //Pcs
                            jobCardMaster.EntityState = EntityState.Modified;
                        }
                    }
                    knittingPlanChild.KJobCardQty = (int)knittingPlanChild.KJobCardMasters.Sum(x => x.KJobCardQty);
                }
            }
            entity.PlanQty = model.PlanQty;
            entity.ConceptNo = model.ConceptNo;

            entity.Yarns.SetUnchanged();
            entity.Yarns.ForEach(y =>
            {
                y.Childs.SetUnchanged();
            });

            foreach (KnittingPlanYarn yarnItem in model.Yarns)
            {
                KnittingPlanYarn existingYarn = entity.Yarns.FirstOrDefault(x => x.KPYarnID == yarnItem.KPYarnID && x.FCMRChildID == yarnItem.FCMRChildID);
                if (existingYarn == null)
                {
                    KnittingPlanYarn knittingPlanYarn = new KnittingPlanYarn()
                    {
                        PhysicalCount = yarnItem.PhysicalCount,
                        YarnTypeID = yarnItem.YarnTypeID,
                        FCMRChildID = yarnItem.FCMRChildID,
                        YarnCountID = yarnItem.YarnCountID,
                        YarnLotNo = yarnItem.YarnLotNo,
                        YarnBrandID = yarnItem.YarnBrandID,
                        YarnPly = yarnItem.YarnPly,
                        StitchLength = yarnItem.StitchLength,
                        BatchNo = yarnItem.BatchNo,
                        ItemMasterID = yarnItem.ItemMasterID,
                        YDItem = yarnItem.YDItem,
                        GroupID = entity.PlanNo,
                        YarnStockSetId = yarnItem.YarnStockSetId,
                        EntityState = EntityState.Added
                    };

                    #region KnittingPlanYarnChild

                    List<KnittingPlanYarn> yarnFCMRs = kpYarnsFCMR.Where(x => x.ConceptID == entity.ConceptID &&
                                                                                      x.YarnCountID == yarnItem.YarnCountID &&
                                                                                      x.YarnTypeID == yarnItem.YarnTypeID).ToList();

                    yarnFCMRs.ForEach(yc =>
                    {
                        KnittingPlanYarnChild yarnChild = new KnittingPlanYarnChild()
                        {
                            KPYarnChildID = 0,
                            KPYarnID = knittingPlanYarn.YarnTypeID,
                            FCMRChildID = yarnItem.FCMRChildID,
                            ConceptID = entity.ConceptID,
                            ItemMasterID = yarnItem.ItemMasterID,
                            ReqQty = yc.TotalQty != 0 ? (yc.ReqQty * entity.PlanQty) / yc.TotalQty : 0,
                            EntityState = EntityState.Added
                        };
                        knittingPlanYarn.Childs.Add(yarnChild);
                    });
                    #endregion

                    entity.Yarns.Add(knittingPlanYarn);
                }
                else
                {
                    existingYarn.PhysicalCount = yarnItem.PhysicalCount;
                    existingYarn.YarnTypeID = yarnItem.YarnTypeID;
                    existingYarn.FCMRChildID = yarnItem.FCMRChildID;
                    existingYarn.YarnCountID = yarnItem.YarnCountID;
                    existingYarn.YarnLotNo = yarnItem.YarnLotNo;
                    existingYarn.YarnBrandID = yarnItem.YarnBrandID;
                    existingYarn.YarnPly = yarnItem.YarnPly;
                    existingYarn.StitchLength = yarnItem.StitchLength;
                    existingYarn.BatchNo = yarnItem.BatchNo;
                    existingYarn.ItemMasterID = yarnItem.ItemMasterID;
                    existingYarn.YDItem = yarnItem.YDItem;
                    existingYarn.GroupID = entity.PlanNo;
                    existingYarn.YarnStockSetId = yarnItem.YarnStockSetId;
                    existingYarn.EntityState = EntityState.Modified;

                    foreach (KnittingPlanYarnChild kpyc in existingYarn.Childs)
                    {
                        KnittingPlanYarnChild existingKpyc = existingYarn.Childs.FirstOrDefault(x => x.KPYarnChildID == kpyc.KPYarnChildID);
                        if (existingKpyc == null)
                        {
                            List<KnittingPlanYarn> yarnFCMRs = kpYarnsFCMR.Where(x => x.ConceptID == entity.ConceptID &&
                                                                                         x.YarnCountID == existingYarn.YarnCountID &&
                                                                                         x.YarnTypeID == existingYarn.YarnTypeID).ToList();

                            yarnFCMRs.ForEach(yc =>
                            {
                                KnittingPlanYarnChild yarnChild = new KnittingPlanYarnChild()
                                {
                                    KPYarnChildID = 0,
                                    KPYarnID = existingYarn.YarnTypeID,
                                    FCMRChildID = yarnItem.FCMRChildID,
                                    ConceptID = entity.ConceptID,
                                    ItemMasterID = yarnItem.ItemMasterID,
                                    ReqQty = yc.TotalQty != 0 ? (yc.ReqQty * entity.PlanQty) / yc.TotalQty : 0,
                                    EntityState = EntityState.Added
                                };
                                existingYarn.Childs.Add(yarnChild);
                            });
                        }
                        else
                        {
                            kpyc.EntityState = EntityState.Modified;
                        }
                    }

                    if (existingYarn.Childs.Count() == 0)
                    {
                        List<KnittingPlanYarn> yarnFCMRs = kpYarnsFCMR.Where(x => x.ConceptID == entity.ConceptID &&
                                                                                             x.YarnCountID == existingYarn.YarnCountID &&
                                                                                             x.YarnTypeID == existingYarn.YarnTypeID).ToList();

                        yarnFCMRs.ForEach(yc =>
                        {
                            KnittingPlanYarnChild yarnChild = new KnittingPlanYarnChild()
                            {
                                KPYarnChildID = 0,
                                KPYarnID = existingYarn.YarnTypeID,
                                FCMRChildID = yarnItem.FCMRChildID,
                                ConceptID = entity.ConceptID,
                                ItemMasterID = yarnItem.ItemMasterID,
                                ReqQty = yc.TotalQty != 0 ? (yc.ReqQty * entity.PlanQty) / yc.TotalQty : 0,
                                EntityState = EntityState.Added
                            };
                            existingYarn.Childs.Add(yarnChild);
                        });
                    }
                }
            }
            //entity.Yarns.Where(x => x.EntityState == EntityState.Unchanged).ToList().ForEach(yarn => yarn.EntityState = EntityState.Deleted);
            entity.Yarns.ForEach(y =>
            {
                if (y.EntityState == EntityState.Unchanged)
                {
                    y.EntityState = EntityState.Deleted;
                }
                y.Childs.ForEach(yc =>
                {
                    if (yc.EntityState == EntityState.Unchanged)
                    {
                        yc.EntityState = EntityState.Deleted;
                    }
                });
            });

            await _service.ReviseAsync(entity, model.KnittingProgramType, oldPlanQty);
            return Ok();
        }

        #region Knitting Plan Grouping
        [Route("save/group")]
        [HttpPost]
        public async Task<IActionResult> SaveGroup()
        {
            var formData = Request.Form;
            var file = Request.Form.Files.FirstOrDefault();
            //if (!Request.Content.IsMimeMultipartContent()) return BadRequest("Unsupported media type.");
          
            KnittingPlanGroup model = formData.ConvertToObject<KnittingPlanGroup>();

          

            model.GroupConceptNo = model.GroupConceptNo.Split(',').Length > 0 ? model.GroupConceptNo.Split(',')[0] : "";
            if (model.GroupConceptNo.IsNullOrEmpty()) model.GroupConceptNo = model.ConceptNo;
            model.KnittingPlans = JsonConvert.DeserializeObject<List<KnittingPlanMaster>>(formData["KPlans"]);

            var childs = JsonConvert.DeserializeObject<List<KnittingPlanChild>>(formData["Childs"]);
            var yarns = JsonConvert.DeserializeObject<List<KnittingPlanYarn>>(formData["Yarns"]);

            model.KnittingPlans.ForEach(kp =>
            {
                kp.Childs = childs.Where(c => c.KPMasterID == kp.KPMasterID).ToList();
                kp.Yarns = yarns.Where(c => c.KPMasterID == kp.KPMasterID).ToList();
            });

            #region Save Image

            var filePath = "";
            var previewTemplate = "";
            if (file != null)
            {
                if (file.Length > 4 * 1024 * 1024)
                    return BadRequest("File is bigger than 4MB.");
                var originalFile = file;
                var inputStream = originalFile.OpenReadStream();

                var fileName = string.Join("", originalFile.FileName.Split(Path.GetInvalidFileNameChars()));
                fileName = GetValidFileName(fileName);
                var contentType = originalFile.ContentType;

                var fileExtension = Path.GetExtension(fileName);
                var provider = new FileExtensionContentTypeProvider();
                string mimeType;

                if (provider.TryGetContentType(fileName, out mimeType))
                {
                    // Set previewTemplate based on file type
                    previewTemplate = fileExtension.Contains(".pdf") ? "pdf" :
                                      mimeType.StartsWith("image/") ? "image" :
                                      "office";
                }
                else
                {
                    // If MIME type couldn't be determined, fallback to "office"
                    previewTemplate = "office";
                }

                fileName = DateTime.Now.ToString("yyyyMMdd'T'HHmmss");
                filePath = $"{AppConstants.KNITTING_PROGRAM_PATH}/{fileName}{fileExtension}";
                var savePath = Path.GetDirectoryName(filePath); //HttpContext.Current.Server.MapPath(filePath);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await inputStream.CopyToAsync(fileStream);
                }
            }
            else
            {
                filePath = "defaultFilePath";  // Handle no file uploaded scenario
                previewTemplate = "office";    // Set default preview template
            }

            #endregion Save Image

            KnittingPlanGroup entity = new KnittingPlanGroup();

            if (model.IsModified && !model.IsAdditional)
            {
                entity = await _service.GetDetailsAsync(model.GroupConceptNo, model.GroupID);

                string conceptIds = string.Join(",", entity.KnittingPlans.Select(x => x.ConceptID));
                List<KnittingPlanYarn> kpYarnsFCMR = await _service.GetYarnsForFCMRChild(conceptIds, false, false, "");

                int buyerID = model.BuyerID;
                int buyerTeamID = model.BuyerTeamID;
                if (entity.KnittingPlans.Count() > 0)
                {
                    KnittingPlanMaster kpmObj = entity.KnittingPlans.First();
                    if (buyerID == 0) buyerID = kpmObj.BuyerID;
                    if (buyerTeamID == 0) buyerID = kpmObj.BuyerTeamID;
                }

                entity.EntityState = EntityState.Modified;
                entity.DateUpdated = DateTime.Now;
                entity.UpdatedBy = AppUser.UserCode;;
                entity.CPI = model.CPI;
                entity.Needle = model.Needle;
                entity.KnittingTypeID = model.KnittingTypeID;
                entity.NeedPreFinishingProcess = model.NeedPreFinishingProcess;
                entity.IsSubContact = model.IsSubContact;
                entity.BrandID = model.BrandID;
                entity.BuyerID = buyerID;
                entity.BuyerTeamID = buyerTeamID;

                entity.GroupConceptNo = model.GroupConceptNo;

                entity.KnittingPlans.ForEach(kp =>
                {
                    kp.EntityState = EntityState.Unchanged;
                    kp.Childs.SetUnchanged();
                    kp.Yarns.SetUnchanged();
                    kp.Yarns.ForEach(y =>
                    {
                        y.Childs.SetUnchanged();
                    });
                });

                List<KnittingPlanMaster> knittingPlans = new List<KnittingPlanMaster>();
                entity.KnittingPlans.ForEach(kp =>
                {
                    var objKP = model.KnittingPlans.Find(x => x.KPMasterID == kp.KPMasterID);
                    if (objKP == null)
                    {
                        kp.AddedBy = AppUser.UserCode;;
                        kp.DateAdded = DateTime.Now;
                        kp.EntityState = EntityState.Added;
                        knittingPlans.Add(kp);
                    }
                    else
                    {
                        objKP.Childs = CommonFunction.DeepClone(kp.Childs);
                        var yarnList = CommonFunction.DeepClone(kp.Yarns);
                        kp = CommonFunction.DeepClone(objKP);
                        kp.IsSubContact = model.IsSubContact;
                        kp.EntityState = EntityState.Modified;
                        kp.DateUpdated = DateTime.Now;
                        kp.UpdatedBy = AppUser.UserCode;;
                        kp.CompanyID = model.CompanyID;
                        kp.ExportOrderID = model.ExportOrderID;
                        kp.MerchandiserTeamID = model.MachineKnittingTypeID;
                        kp.StyleNo = model.StyleNo;
                        kp.SeasonID = model.SeasonID;
                        kp.ContactID = model.ContactID;
                        kp.MCSubClassID = model.MCSubClassID;
                        kp.Yarns = yarnList;
                        var tempChilds = new List<KnittingPlanChild>();
                        kp.Childs.ForEach(c =>
                        {
                            var objKPC = objKP.Childs.Find(x => x.KPMasterID == c.KPMasterID && x.ItemMasterID == c.ItemMasterID);
                            if (objKPC == null)
                            {
                                c.EntityState = EntityState.Added;
                                c.Needle = model.Needle;
                                c.CPI = model.CPI;
                                c.KnittingTypeID = model.KnittingTypeID;
                                c.MCSubClassID = model.MCSubClassID;
                                c.TotalNeedle = (int)Math.Ceiling((kp.Length + (kp.Length * (6 / 100))) * c.Needle);
                                c.TotalCourse = (int)Math.Ceiling((kp.Width + kp.Width * (6 / 100)) * (decimal)(c.CPI / 2.54));
                                tempChilds.Add(c);
                            }
                            else
                            {
                                c = CommonFunction.DeepClone(objKPC);
                                c.MCSubClassID = model.MCSubClassID;
                                c.MachineDia = model.MachineDia;
                                c.MachineGauge = model.MachineGauge;
                                c.StartDate = model.StartDate;
                                c.Needle = model.Needle;
                                c.CPI = model.CPI;
                                c.BookingQty = objKP.BookingQty;
                                c.KnittingTypeID = model.KnittingTypeID;
                                c.TotalNeedle = (int)Math.Ceiling((kp.Length + (kp.Length * (6 / 100))) * c.Needle);
                                c.TotalCourse = (int)Math.Ceiling((kp.Width + kp.Width * (6 / 100)) * (decimal)(c.CPI / 2.54));
                                c.EntityState = EntityState.Modified;
                                tempChilds.Add(c);
                            }
                        });
                        kp.Childs = tempChilds;
                        var tempYarns = new List<KnittingPlanYarn>();

                        objKP.Yarns.ForEach(c =>
                        {
                            var kpyObj = kp.Yarns.Find(x => x.KPMasterID == c.KPMasterID
                                                            && x.ItemMasterID == c.ItemMasterID
                                                            && x.PhysicalCount == c.PhysicalCount
                                                            && x.YarnLotNo == c.YarnLotNo
                                                            && x.BatchNo == c.BatchNo
                                                            && x.YarnBrandID == c.YarnBrandID
                                                            && x.YarnPly == c.YarnPly
                                                            && x.StitchLength == c.StitchLength
                                                            && x.YDItem == c.YDItem);
                            if (kpyObj == null)
                            {
                                c.EntityState = EntityState.Added;

                                #region KnittingPlanYarnChild
                                List<KnittingPlanYarn> yarnFCMRs = kpYarnsFCMR.Where(x => x.ConceptID == kp.ConceptID &&
                                                                                           x.YarnCountID == c.YarnCountID &&
                                                                                           x.YarnTypeID == c.YarnTypeID).ToList();
                                yarnFCMRs.ForEach(x =>
                                {
                                    KnittingPlanYarnChild kpyChild = new KnittingPlanYarnChild();
                                    kpyChild.KPYarnID = c.KPYarnID;
                                    kpyChild.FCMRChildID = x.FCMRChildID;
                                    kpyChild.ConceptID = x.ConceptID;
                                    kpyChild.ItemMasterID = x.ItemMasterID;
                                    kpyChild.ReqQty = x.TotalQty != 0 ? (x.ReqQty * kp.BookingQty) / x.TotalQty : 0;
                                    c.Childs.Add(kpyChild);
                                });
                                #endregion

                                tempYarns.Add(c);
                            }
                            else
                            {
                                c = CommonFunction.DeepClone(kpyObj);
                                kp.Yarns.Find(x => x.KPMasterID == c.KPMasterID
                                                            && x.ItemMasterID == c.ItemMasterID
                                                            && x.PhysicalCount == c.PhysicalCount
                                                            && x.YarnLotNo == c.YarnLotNo
                                                            && x.BatchNo == c.BatchNo
                                                            && x.YarnBrandID == c.YarnBrandID
                                                            && x.YarnPly == c.YarnPly
                                                            && x.StitchLength == c.StitchLength
                                                            && x.YDItem == c.YDItem).EntityState = EntityState.Modified;
                                c.EntityState = EntityState.Modified;

                                tempYarns.Add(c);
                            }
                        });
                        kp.Yarns.ToList().ForEach(y =>
                        {
                            if (y.EntityState == EntityState.Unchanged)
                            {
                                y.EntityState = EntityState.Deleted;
                                y.Childs.SetDeleted();
                            }
                            else
                            {
                                y.Childs.Where(c => c.EntityState == EntityState.Unchanged).ToList().ForEach(c =>
                                {
                                    c.EntityState = EntityState.Deleted;
                                });
                            }
                            tempYarns.Add(y);
                        });
                        kp.Yarns = tempYarns;
                        knittingPlans.Add(kp);
                    }
                });
                entity.KnittingPlans = knittingPlans;
                entity.UserId = AppUser.UserCode;
                await _service.SaveGroupAsync(entity, false);
            }
            else
            {
                entity = model;
                entity.AddedBy = AppUser.UserCode;;
                entity.DateAdded = DateTime.Now;

                string conceptIds = string.Join(",", model.KnittingPlans.Select(x => x.ConceptID));
                List<KnittingPlanYarn> kpYarns = await _service.GetYarns(conceptIds, false, false, "");
                List<KnittingPlanYarn> kpYarnsFCMR = await _service.GetYarnsForFCMRChild(conceptIds, false, false, "");

                #region Yarn Detail
                List<KnittingPlanYarn> modelKPYarns = new List<KnittingPlanYarn>();
                model.KnittingPlans.ForEach(x =>
                {
                    x.CompanyID = model.CompanyID;
                    x.ExportOrderID = model.ExportOrderID;
                    x.MerchandiserTeamID = model.MachineKnittingTypeID;
                    x.StyleNo = model.StyleNo;
                    x.SeasonID = model.SeasonID;
                    x.ContactID = model.ContactID;
                    x.MCSubClassID = model.MCSubClassID;
                    x.Yarns.ForEach(y => y.ConceptID = x.ConceptID);
                    modelKPYarns.AddRange(x.Yarns);
                });
                foreach (KnittingPlanMaster kp in entity.KnittingPlans)
                {
                    kp.Childs.ForEach(c =>
                    {
                        c.KnittingTypeID = model.KnittingTypeID;
                        c.Needle = model.Needle;
                        c.CPI = model.CPI;
                        c.MCSubClassID = model.MCSubClassID;
                        c.BrandID = model.BrandID;
                        c.ContactID = model.ContactID;
                        c.ExportOrderID = model.ExportOrderID;
                    });
                    kp.Yarns = modelKPYarns.Where(y => y.ConceptID == kp.ConceptID).ToList();

                    #region KnittingPlanYarnChild
                    KnittingPlanYarnChild kpyChild = new KnittingPlanYarnChild();
                    foreach (KnittingPlanYarn yarnObj in kp.Yarns)
                    {
                        List<KnittingPlanYarn> yarnFCMRs = kpYarnsFCMR.Where(x => x.ConceptID == yarnObj.ConceptID &&
                                                                             x.YarnCountID == yarnObj.YarnCountID &&
                                                                             x.YarnTypeID == yarnObj.YarnTypeID).ToList();
                        yarnFCMRs.ForEach(x =>
                        {
                            kpyChild = new KnittingPlanYarnChild();
                            kpyChild.KPYarnID = yarnObj.KPYarnID;
                            kpyChild.FCMRChildID = x.FCMRChildID;
                            kpyChild.ConceptID = x.ConceptID;
                            kpyChild.ItemMasterID = x.ItemMasterID;
                            kpyChild.ReqQty = x.TotalQty != 0 ? (x.ReqQty * kp.BookingQty) / x.TotalQty : 0;
                            yarnObj.Childs.Add(kpyChild);
                        });
                    }
                    #endregion
                }
                #endregion
                entity.UserId = AppUser.UserCode;
                await _service.SaveGroupAsync(entity, false);
            }

            return Ok();
        }
        [Route("revise/group")]
        [HttpPost]
        public async Task<IActionResult> ReviseGroup()
        {
            var formData = Request.Form;
            var file = Request.Form.Files.FirstOrDefault();
            //if (!Request.Content.IsMimeMultipartContent()) return BadRequest("Unsupported media type.");
       
            KnittingPlanGroup model = formData.ConvertToObject<KnittingPlanGroup>();

            //KnittingPlanGroup knittingPlan = JsonConvert.DeserializeObject<KnittingPlanGroup>(formData.Get("KPlanGroup"));

            model.GroupConceptNo = model.GroupConceptNo.Split(',').Length > 0 ? model.GroupConceptNo.Split(',')[0] : "";
            if (model.GroupConceptNo.IsNullOrEmpty()) model.GroupConceptNo = model.ConceptNo;
            model.KnittingPlans = JsonConvert.DeserializeObject<List<KnittingPlanMaster>>(formData["KPlans"]);
            var childs = JsonConvert.DeserializeObject<List<KnittingPlanChild>>(formData["Childs"]);
            var yarns = JsonConvert.DeserializeObject<List<KnittingPlanYarn>>(formData["Yarns"]);


            model.KnittingPlans.ForEach(kp =>
            {
                kp.Childs = childs.Where(c => c.KPMasterID == kp.KPMasterID).ToList();
                kp.Yarns = yarns.Where(c => c.KPMasterID == kp.KPMasterID).ToList();
            });

            KnittingPlanGroup entity = new KnittingPlanGroup();

            entity = await _service.GetDetailsAsync(model.GroupConceptNo, model.GroupID);
            string conceptIds = string.Join(",", entity.KnittingPlans.Select(x => x.ConceptID));
            List<KnittingPlanYarn> kpYarnsFCMR = await _service.GetYarnsForFCMRChild(conceptIds, false, false, "");

            entity.EntityState = EntityState.Modified;
            entity.DateUpdated = DateTime.Now;
            entity.UpdatedBy = AppUser.UserCode; ;
            entity.CPI = model.CPI;
            entity.Needle = model.Needle;
            entity.KnittingTypeID = model.KnittingTypeID;
            entity.NeedPreFinishingProcess = model.NeedPreFinishingProcess;
            entity.IsSubContact = model.IsSubContact;
            entity.GroupConceptNo = model.GroupConceptNo;
            entity.SubGroupID = model.SubGroupID;
            entity.PreRevisionNo = model.PreRevisionNo;
            entity.RevisionNo = model.RevisionNo + 1;

            if (entity.KnittingPlans.Count() > 0)
            {
                var kpMaster = entity.KnittingPlans.ToList().First();
                entity.BuyerID = kpMaster.BuyerID;
                entity.BuyerTeamID = kpMaster.BuyerTeamID;
                entity.SubGroupID = entity.SubGroupID == 0 ? kpMaster.SubGroupID : entity.SubGroupID;
            }

            entity.KnittingPlans.ForEach(kp =>
            {
                kp.EntityState = EntityState.Unchanged;
                kp.Childs.SetUnchanged();
                kp.Yarns.SetUnchanged();
                kp.Yarns.ForEach(y =>
                {
                    y.Childs.SetUnchanged();
                });
            });

            List<KnittingPlanMaster> knittingPlans = new List<KnittingPlanMaster>();
            entity.KnittingPlans.ForEach(kp =>
            {
                var objKP = model.KnittingPlans.Find(x => x.KPMasterID == kp.KPMasterID);
                if (objKP == null)
                {
                    kp.AddedBy = AppUser.UserCode;
                    kp.DateAdded = DateTime.Now;
                    kp.PlanNo = entity.GroupID;
                    kp.EntityState = EntityState.Added;
                    knittingPlans.Add(kp);
                }
                else
                {
                    objKP.Childs = CommonFunction.DeepClone(kp.Childs);
                    var yarnList = CommonFunction.DeepClone(kp.Yarns);
                    kp = CommonFunction.DeepClone(objKP);
                    kp.IsSubContact = model.IsSubContact;
                    kp.EntityState = EntityState.Modified;
                    kp.DateUpdated = DateTime.Now;
                    kp.UpdatedBy = AppUser.UserCode;
                    kp.Yarns = yarnList;
                    kp.PlanNo = entity.GroupID;
                    kp.PreProcessRevNo = objKP.PreProcessRevNo;
                    kp.RevisionNo = objKP.RevisionNo + 1;
                    kp.RevisionDate = DateTime.Now;
                    kp.RevisionBy = AppUser.UserCode;
                    //kp.RevisionReason = kp.RevisionReason;

                    var tempChilds = new List<KnittingPlanChild>();
                    kp.Childs.ForEach(c =>
                    {
                        var objKPC = objKP.Childs.Find(x => x.KPMasterID == c.KPMasterID && x.ItemMasterID == c.ItemMasterID);
                        if (objKPC == null)
                        {
                            c.EntityState = EntityState.Added;
                            c.Needle = model.Needle;
                            c.CPI = model.CPI;
                            c.KnittingTypeID = model.KnittingTypeID;
                            c.PlanNo = entity.GroupID;
                            c.TotalNeedle = (int)Math.Ceiling((kp.Length + (kp.Length * (6 / 100))) * c.Needle);
                            c.TotalCourse = (int)Math.Ceiling((kp.Width + kp.Width * (6 / 100)) * (decimal)(c.CPI / 2.54));
                            tempChilds.Add(c);
                        }
                        else
                        {
                            c = CommonFunction.DeepClone(objKPC);
                            c.MachineDia = model.MachineDia;
                            c.MachineGauge = model.MachineGauge;
                            c.StartDate = model.StartDate;
                            c.Needle = model.Needle;
                            c.CPI = model.CPI;
                            c.BookingQty = objKP.BookingQty;
                            c.PlanNo = entity.GroupID;
                            c.KnittingTypeID = model.KnittingTypeID;
                            c.TotalNeedle = (int)Math.Ceiling((kp.Length + (kp.Length * (6 / 100))) * c.Needle);
                            c.TotalCourse = (int)Math.Ceiling((kp.Width + kp.Width * (6 / 100)) * (decimal)(c.CPI / 2.54));
                            c.EntityState = EntityState.Modified;
                            tempChilds.Add(c);
                        }
                    });
                    kp.Childs = tempChilds;
                    var tempYarns = new List<KnittingPlanYarn>();

                    objKP.Yarns.ForEach(c =>
                    {
                        var kpyObj = kp.Yarns.Find(x => x.KPMasterID == c.KPMasterID
                                                        && x.ItemMasterID == c.ItemMasterID
                                                        && x.PhysicalCount == c.PhysicalCount
                                                        && x.YarnLotNo == c.YarnLotNo
                                                        && x.BatchNo == c.BatchNo
                                                        && x.YarnBrandID == c.YarnBrandID
                                                        && x.YarnPly == c.YarnPly
                                                        && x.StitchLength == c.StitchLength
                                                        && x.YDItem == c.YDItem);
                        if (kpyObj == null)
                        {
                            c.EntityState = EntityState.Added;
                            c.GroupID = entity.GroupID;

                            #region KnittingPlanYarnChild
                            List<KnittingPlanYarn> yarnFCMRs = kpYarnsFCMR.Where(x => x.ConceptID == kp.ConceptID &&
                                                                                       x.YarnCountID == c.YarnCountID &&
                                                                                       x.YarnTypeID == c.YarnTypeID).ToList();
                            yarnFCMRs.ForEach(x =>
                            {
                                KnittingPlanYarnChild kpyChild = new KnittingPlanYarnChild();
                                kpyChild.KPYarnID = c.KPYarnID;
                                kpyChild.FCMRChildID = x.FCMRChildID;
                                kpyChild.ConceptID = x.ConceptID;
                                kpyChild.ItemMasterID = x.ItemMasterID;
                                kpyChild.ReqQty = x.TotalQty != 0 ? (x.ReqQty * kp.BookingQty) / x.TotalQty : 0;
                                c.Childs.Add(kpyChild);
                            });
                            #endregion
                            tempYarns.Add(c);
                        }
                        else
                        {

                            kp.Yarns.Find(x => x.KPMasterID == c.KPMasterID
                                                        && x.ItemMasterID == c.ItemMasterID
                                                        && x.PhysicalCount == c.PhysicalCount
                                                        && x.YarnLotNo == c.YarnLotNo
                                                        && x.BatchNo == c.BatchNo
                                                        && x.YarnBrandID == c.YarnBrandID
                                                        && x.YarnPly == c.YarnPly
                                                        && x.StitchLength == c.StitchLength
                                                        && x.YDItem == c.YDItem).EntityState = EntityState.Modified;

                            kp.Yarns.Find(x => x.KPMasterID == c.KPMasterID
                                                        && x.ItemMasterID == c.ItemMasterID
                                                        && x.PhysicalCount == c.PhysicalCount
                                                        && x.YarnLotNo == c.YarnLotNo
                                                        && x.BatchNo == c.BatchNo
                                                        && x.YarnBrandID == c.YarnBrandID
                                                        && x.YarnPly == c.YarnPly
                                                        && x.StitchLength == c.StitchLength
                                                        && x.YDItem == c.YDItem).Childs.ForEach(yc =>
                                                        {
                                                            yc.EntityState = EntityState.Modified;
                                                        });


                            c = CommonFunction.DeepClone(kpyObj);
                            c.GroupID = entity.GroupID;
                            c.EntityState = EntityState.Modified;
                            tempYarns.Add(c);
                        }
                    });
                    kp.Yarns.ToList().ForEach(y =>
                    {
                        if (y.EntityState == EntityState.Unchanged)
                        {
                            y.EntityState = EntityState.Deleted;
                            y.Childs.SetDeleted();
                        }
                        else
                        {
                            y.Childs.Where(c => c.EntityState == EntityState.Unchanged).ToList().ForEach(c =>
                            {
                                c.EntityState = EntityState.Deleted;
                            });
                        }
                        tempYarns.Add(y);
                    });
                    kp.Yarns = tempYarns;
                    knittingPlans.Add(kp);
                }
            });
            entity.KnittingPlans = knittingPlans;
            entity.UserId = AppUser.UserCode;
            await _service.SaveGroupAsync(entity, true);

            return Ok();
        }
        #endregion

        private string GetValidFileName(string fileName)
        {
            fileName = fileName.Replace("#", " ")
                               .Replace("?", "");
            fileName = Regex.Replace(fileName, @"[^\u0000-\u007F]+", string.Empty);
            return fileName;
        }

    }
}