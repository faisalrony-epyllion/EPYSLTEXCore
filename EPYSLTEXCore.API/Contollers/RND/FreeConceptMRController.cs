using EPYSLTEX.Core.Interfaces;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.API.Extends.Filters;
using EPYSLTEXCore.Application.Interfaces;
using EPYSLTEXCore.Application.Interfaces.RND;
using EPYSLTEXCore.Application.Services;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities.Gmt.General.Item;
using EPYSLTEXCore.Infrastructure.Entities.Tex;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System.Data.Entity;

namespace EPYSLTEXCore.API.Contollers.RND
{
    [Route("api/rnd-free-concept-mr")]
    public class FreeConceptMRController : ApiBaseController
    {
        private readonly IItemMasterService<FreeConceptMRChild> _itemMasterRepository;
        private readonly IDapperCRUDService<ItemSegmentName> _itemSegmentNameRepository;
        private readonly IDapperCRUDService<ItemSegmentValue> _itemSegmentValueRepository;
        private readonly IFreeConceptMRService _service;
        private readonly IFreeConceptService _serviceConcept;
        private readonly ICommonHelperService _commonHelperService;
        private readonly IItemSetupService _itemSetupService;
        private readonly IMemoryCache _memoryCache;
        //private readonly ISqlQueryRepository<Select2MappingOptionModel> _sqlQueryRepository;
        //private readonly IDapperCRUDService<DapperBaseEntity> _sqlQueryService;

        public FreeConceptMRController(IItemMasterService<FreeConceptMRChild> itemMasterRepository, IMemoryCache memoryCache
            , IDapperCRUDService<ItemSegmentName> itemSegmentNameRepository
            , IDapperCRUDService<ItemSegmentValue> itemSegmentValueRepository
            , IFreeConceptMRService service
            , IFreeConceptService serviceConcept
            , ICommonHelperService commonHelperService
            , IItemSetupService itemSetupService, IUserService userService) : base(userService)
        {
            _itemMasterRepository = itemMasterRepository;
            _itemSegmentNameRepository = itemSegmentNameRepository;
            _itemSegmentValueRepository = itemSegmentValueRepository;
            _service = service;
            _serviceConcept = serviceConcept;
            _commonHelperService = commonHelperService;
            //_sqlQueryRepository = sqlQueryRepository;
            _itemSetupService = itemSetupService;
            _memoryCache = memoryCache;
            //_sqlQueryService = sqlQueryService;
        }

        [Route("list")]
        public async Task<IActionResult> GetList(Status status)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<FreeConceptMRMaster> records = await _service.GetPagedAsync(status, paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        [HttpGet]
        [Route("new/{conceptId}")]
        public async Task<IActionResult> GetNew(int conceptId)
        {
            return Ok(await _service.GetNewAsync(conceptId));
        }

        [Route("{id}")]
        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            return Ok(await _service.GetAsync(id));
        }

        [Route("new-by-group-concept/{grpConceptNo}/{conceptTypeId}")]
        [HttpGet]
        public async Task<IActionResult> GetByGroupConcept(string grpConceptNo, int conceptTypeId)
        {
            List<FreeConceptMRMaster> entities = new List<FreeConceptMRMaster>();
            entities = await _service.GetMultiDetailsAsync(grpConceptNo);
            if (entities.Count > 0)
            {
                return Ok(await _service.GetMultipleAsyncRevision(grpConceptNo, conceptTypeId));
            }
            else
            {
                FreeConceptMRMaster record = await _service.GetByGroupConceptAsync(grpConceptNo, conceptTypeId);
                return Ok(record);
            }

        }

        [Route("multiple-mr/{grpConceptNo}/{conceptTypeId}")]
        [HttpGet]
        public async Task<IActionResult> GetMultipleAsync(string grpConceptNo, int conceptTypeId)
        {
            return Ok(await _service.GetMultipleAsync(grpConceptNo, conceptTypeId));
        }

        [Route("save")]
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> Save(dynamic jsonString)
        {
            FreeConceptMRMaster model = JsonConvert.DeserializeObject<FreeConceptMRMaster>(Convert.ToString(jsonString));
            // Set Item master Id.
            List<FreeConceptMRChild> childRecords = model.Childs;
            _itemMasterRepository.GenerateItem(AppConstants.ITEM_SUB_GROUP_YARN_NEW, ref childRecords);

            FreeConceptMRMaster entity;
            if (model.IsModified)
            {
                entity = await _service.GetDetailsAsync(model.FCMRMasterID);

                entity.ReqDate = model.ReqDate;
                entity.UpdatedBy = AppUser.UserCode;
                entity.DateUpdated = DateTime.Now;
                entity.EntityState = EntityState.Modified;

                entity.Childs.SetUnchanged();

                foreach (FreeConceptMRChild item in childRecords)
                {
                    FreeConceptMRChild child = entity.Childs.FirstOrDefault(x => x.FCMRChildID == item.FCMRChildID);

                    if (child == null)
                    {
                        child = item;
                        child.ItemMasterID = item.ItemMasterID;
                        child.YarnCategory = CommonFunction.GetYarnShortForm(item.Segment1ValueDesc, item.Segment2ValueDesc, item.Segment3ValueDesc, item.Segment4ValueDesc, item.Segment5ValueDesc, item.Segment6ValueDesc, item.ShadeCode);
                        child.Acknowledge = true;
                        child.AcknowledgeBy = AppUser.UserCode;
                        child.AcknowledgeDate = DateTime.Now;
                        child.Reject = false;
                        child.EntityState = EntityState.Added;
                        entity.Childs.Add(child);
                    }
                    else
                    {
                        child.ShadeCode = item.ShadeCode;
                        child.YD = item.YD;
                        child.YDItem = item.YDItem;
                        child.IsPR = item.IsPR;
                        child.ReqQty = item.ReqQty;
                        child.ReqCone = item.ReqCone;
                        child.ItemMasterID = item.ItemMasterID;
                        child.YarnCategory = CommonFunction.GetYarnShortForm(item.Segment1ValueDesc, item.Segment2ValueDesc, item.Segment3ValueDesc, item.Segment4ValueDesc, item.Segment5ValueDesc, item.Segment6ValueDesc, item.ShadeCode);
                        child.Acknowledge = true;
                        child.AcknowledgeBy = AppUser.UserCode;
                        child.AcknowledgeDate = DateTime.Now;
                        child.Reject = false;
                        child.EntityState = EntityState.Modified;
                    }
                }
            }
            else
            {
                if (await _service.ExistsAsync(model.ConceptID, model.TrialNo)) return BadRequest("Material requirement for this conept is already generated by another user. Please check in complete list ny concept no for details.");

                entity = model;
                entity.AddedBy = AppUser.UserCode;
                entity.DateAdded = DateTime.Now;

                foreach (FreeConceptMRChild child in entity.Childs)
                {
                    child.ItemMasterID = childRecords.Find(x => x.FCMRChildID == child.FCMRChildID).ItemMasterID;
                    child.YarnCategory = CommonFunction.GetYarnShortForm(child.Segment1ValueDesc, child.Segment2ValueDesc, child.Segment3ValueDesc, child.Segment4ValueDesc, child.Segment5ValueDesc, child.Segment6ValueDesc, child.ShadeCode);
                    child.Acknowledge = true;
                    child.AcknowledgeBy = AppUser.UserCode;
                    child.AcknowledgeDate = DateTime.Now;
                    child.Reject = false;
                }
            }

            await _service.SaveAsync(entity, AppUser.UserCode);
            return Ok();
        }

        [Route("save-multiple")]
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> SaveMultiple(dynamic jsnString)
        {
            List<FreeConceptMRMaster> models = JsonConvert.DeserializeObject<List<FreeConceptMRMaster>>(
              Convert.ToString(jsnString),
              new JsonSerializerSettings
              {
                  DateTimeZoneHandling = DateTimeZoneHandling.Local // Ensures the date is interpreted as local time
              });
            //List<FreeConceptMRMaster> models = model.models.ToObject<List<FreeConceptMRMaster>>();
            string grpConceptNo = models.First().GroupConceptNo;
            bool isModified = models.First().IsModified;
            bool isComplete = models.First().IsComplete;

            List<FreeConceptMRChild> childRecords = new List<FreeConceptMRChild>();
            models.ForEach(mr =>
            {
                childRecords.AddRange(mr.Childs);
            });

            // Set Item master Id.
            _itemMasterRepository.GenerateItem(AppConstants.ITEM_SUB_GROUP_YARN_NEW, ref childRecords);

            List<FreeConceptMaster> concepts = await _serviceConcept.GetDatasAsync(grpConceptNo);
            int revisionNo = concepts.Max(x => x.RevisionNo);

            List<FreeConceptMRMaster> entities = new List<FreeConceptMRMaster>();
            FreeConceptMRMaster entity;
            if (isModified)
            {
                entities = await _service.GetMultiDetailsAsync(grpConceptNo);
                entities.ForEach(x =>
                {
                    x.EntityState = EntityState.Unchanged;
                    x.Childs.SetUnchanged();
                });

                models.ForEach(item =>
                {
                    entity = entities.FirstOrDefault(x => x.ConceptID == item.ConceptID);
                    if (entity == null)
                    {
                        entity = item;
                        entity.AddedBy = AppUser.UserCode;
                        entity.PreProcessRevNo = revisionNo;
                        entity.Childs.ForEach(child =>
                        {
                            child.ItemMasterID = childRecords.Find(x => x.FCMRChildID == child.FCMRChildID).ItemMasterID;
                            child.YarnCategory = CommonFunction.GetYarnShortForm(child.Segment1ValueDesc, child.Segment2ValueDesc, child.Segment3ValueDesc, child.Segment4ValueDesc, child.Segment5ValueDesc, child.Segment6ValueDesc, child.ShadeCode);
                            child.Acknowledge = true;
                            child.AcknowledgeBy = AppUser.UserCode;
                            child.AcknowledgeDate = DateTime.Now;
                            child.Reject = false;
                            child.EntityState = EntityState.Added;
                        });
                        entities.Add(entity);
                    }
                    else
                    {
                        entity.ReqDate = item.ReqDate;
                        entity.UpdatedBy = AppUser.UserCode;
                        entity.DateUpdated = DateTime.Now;
                        entity.EntityState = EntityState.Modified;
                        entity.IsComplete = item.IsComplete;
                        entity.PreProcessRevNo = revisionNo;

                        item.Childs.ForEach(mrChild =>
                        {
                            FreeConceptMRChild child = entity.Childs.FirstOrDefault(x => x.FCMRChildID == mrChild.FCMRChildID);
                            if (child == null)
                            {
                                child = mrChild;
                                child.YarnCategory = CommonFunction.GetYarnShortForm(mrChild.Segment1ValueDesc, mrChild.Segment2ValueDesc, mrChild.Segment3ValueDesc, mrChild.Segment4ValueDesc, mrChild.Segment5ValueDesc, mrChild.Segment6ValueDesc, mrChild.ShadeCode);
                                child.Acknowledge = true;
                                child.AcknowledgeBy = AppUser.UserCode;
                                child.AcknowledgeDate = DateTime.Now;
                                child.Reject = false;
                                child.EntityState = EntityState.Added;
                                entity.Childs.Add(child);
                            }
                            else
                            {
                                child.ShadeCode = mrChild.ShadeCode;
                                child.YD = mrChild.YD;
                                child.YDItem = mrChild.YDItem;
                                child.IsPR = mrChild.IsPR;
                                child.ReqQty = mrChild.ReqQty;
                                child.ReqCone = mrChild.ReqCone;
                                child.ItemMasterID = mrChild.ItemMasterID;
                                child.YarnCategory = CommonFunction.GetYarnShortForm(mrChild.Segment1ValueDesc, mrChild.Segment2ValueDesc, mrChild.Segment3ValueDesc, mrChild.Segment4ValueDesc, mrChild.Segment5ValueDesc, mrChild.Segment6ValueDesc, mrChild.ShadeCode);
                                child.Acknowledge = true;
                                child.AcknowledgeBy = AppUser.UserCode;
                                child.AcknowledgeDate = DateTime.Now;
                                child.Reject = false;
                                child.YarnStockSetId = mrChild.YarnStockSetId;
                                child.DayValidDurationId = mrChild.DayValidDurationId;
                                child.EntityState = EntityState.Modified;
                            }
                        });

                        entity.Childs.Where(x => x.EntityState == EntityState.Unchanged).ToList().ForEach(x =>
                        {
                            x.EntityState = EntityState.Deleted;
                        });
                    }
                });
                await _service.SaveMultipleAsync(entities, EntityState.Modified);
            }
            else
            {
                //Trial no
                //if (await _service.ExistsAsync(model.ConceptID, model.TrialNo)) return BadRequest("Material requirement for this conept is already generated by another user. Please check in complete list ny concept no for details.");

                models.ForEach(mr =>
                {
                    mr.AddedBy = AppUser.UserCode;
                    mr.DateAdded = DateTime.Now;
                    mr.PreProcessRevNo = revisionNo;
                    mr.Childs.ForEach(child =>
                    {
                        child.ItemMasterID = childRecords.Find(x => x.FCMRChildID == child.FCMRChildID).ItemMasterID;
                        child.YarnCategory = CommonFunction.GetYarnShortForm(child.Segment1ValueDesc, child.Segment2ValueDesc, child.Segment3ValueDesc, child.Segment4ValueDesc, child.Segment5ValueDesc, child.Segment6ValueDesc, child.ShadeCode);
                        child.Acknowledge = true;
                        child.AcknowledgeBy = AppUser.UserCode;
                        child.AcknowledgeDate = DateTime.Now;
                        child.Reject = false;
                        child.EntityState = EntityState.Added;
                    });
                });

                entities = models;
                await _service.SaveMultipleAsync(entities, EntityState.Added);
            }
            if (isComplete) await _commonHelperService.UpdateFreeConceptStatus(InterfaceFrom.MaterialRequirement, 0, grpConceptNo);
            return Ok();
        }

        #region Composition Part
        [Route("save-yarn-composition")]
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> SaveComposition(dynamic jsonString)
        {
            ItemSegmentDTO model = JsonConvert.DeserializeObject<ItemSegmentDTO>(Convert.ToString(jsonString));
                //ItemSegmentName itemSegmentName = await _service.FindAsync(a => a.SegmentName == ItemSegmentNameConstants.YARN_COMPOSITION);
                ItemSegmentName itemSegmentName = await _service.FindAsync(ItemSegmentNameConstants.YARN_COMPOSITION);

            if (itemSegmentName.IsNull())
                return BadRequest("Yarn Composition Segment Not Found");
            bool y = await _service.ExistsAsync(itemSegmentName.SegmentNameID, model.SegmentValue);
            if (y)
                return BadRequest("This composition is already exists.");

            ItemSegmentValue entity = new ItemSegmentValue
            {
                SegmentValue = model.SegmentValue,
                SegmentNameID = itemSegmentName.SegmentNameID
            };

            await _service.AddAsync(entity, TableNames.ITEM_SEGMENT_VALUE);


            #region Update Cache

            string cacheKey = CacheKeys.Yarn_Item_Segments;

            _memoryCache.Remove(cacheKey);

            //var itemSegmentValueList = await _service.GetDataDapperAsync<Select2MappingOptionModel>(CommonQueries.GetItemSegmentValuesBySegmentNamesWithMapping());
            var itemSegmentValueList = _itemMasterRepository.GetDataAsync<Select2MappingOptionModel>(CommonQueries.GetItemSegmentValuesBySegmentNamesWithMapping(), DB_TYPE.textile).ToList();
            var TechnicalParameterList = _itemMasterRepository.GetDataAsync<Select2MappingOptionModel>(CommonQueries.GetQualityParameterIDs(), DB_TYPE.textile).ToList();

            var itemSegmenValues = new ItemSegmentMappingValuesDTO
            {
                Segment1ValueList = itemSegmentValueList.FindAll(x => x.desc == ItemSegmentNameConstants.YARN_COMPOSITION),
                Segment2ValueList = itemSegmentValueList.FindAll(x => x.desc == ItemSegmentNameConstants.YARN_MANUFACTURING_LINE),
                Segment3ValueList = itemSegmentValueList.FindAll(x => x.desc == ItemSegmentNameConstants.YARN_MANUFACTURING_PROCESS),
                Segment4ValueList = itemSegmentValueList.FindAll(x => x.desc == ItemSegmentNameConstants.YARN_MANUFACTURING_SUB_PROCESS),
                Segment5ValueList = itemSegmentValueList.FindAll(x => x.desc == ItemSegmentNameConstants.YARN_QUALITY_PARAMETER),
                Segment6ValueList = itemSegmentValueList.FindAll(x => x.desc == ItemSegmentNameConstants.YARN_COLOR),
                Segment7ValueList = itemSegmentValueList.FindAll(x => x.desc == ItemSegmentNameConstants.YARN_COLOR_GRADE),
                Segment8ValueList = itemSegmentValueList.FindAll(x => x.desc == ItemSegmentNameConstants.YARN_COUNT),
                ShadeReferenceList = itemSegmentValueList.FindAll(x => x.desc == ItemSegmentNameConstants.SHADE)
                //YarnCountMaster = itemSegmentValueList.FindAll(x => x.desc == ItemSegmentNameConstants.YARN_COUNT_MASTER)
            };

            foreach (Select2MappingOptionModel item in itemSegmenValues.Segment1ValueList)
            {
                //var filterValues = item.YarnTypes.Split(',').Select(f => f.Trim()).ToList();
                var filterValues = item.YarnTypes != null
                        ? item.YarnTypes.Split(',').Select(f => f.Trim()).ToList()
                        : new List<string>();
                var filteredData = TechnicalParameterList.Where(row => filterValues.Contains(row.text)).ToList();
                string commaSeparatedString = string.Join(",", filteredData.Select(row => row.desc));
                item.QualityParameterIDs = commaSeparatedString;

            }


            _memoryCache.Set(cacheKey, CommonFunction.DeepClone(itemSegmenValues), TimeSpan.FromDays(1));
            //#if DEBUG
            //                await _itemSetupService.SaveCacheForYarnSegmentFilterUpdateTimeAsync("Local PC");
            //#else
            //                    await _itemSetupService.SaveCacheForYarnSegmentFilterUpdateTimeAsync("Server");
            //#endif

            //}
            #endregion
            //var responseData = _mapper.Map<ItemSegmentValue>(entity);
            #region Save BlendTypeName
            CompositionBlendType obj = new CompositionBlendType();
            obj.CompositionID = entity.SegmentValueID;
            obj.BlendTypeName = model.BlendTypeName;
            obj.ProgramTypeName = model.ProgramTypeName;
            obj.ManufacturingLines = model.ManufacturingLines;
            obj.YarnTypes = model.YarnTypes;
            if (obj.BlendTypeName.IsNotNullOrEmpty())
            {
                await _service.SaveBlendTypeName(obj);
            }

            #endregion
            //return Ok(responseData);
            return Ok();
        }
        [Route("yarn-type")]
        [HttpGet]
        public async Task<IActionResult> GetYarnTypes()
        {
            return Ok(await _service.GetYarnTypes());
        }
        [Route("yarn-sub-progran-new/{yarnTypeId}")]
        [HttpGet]
        public async Task<IActionResult> GetYarnSubProgramNews(string yarnTypeId) //yarnTypeId = FiberId
        {
            var list = await _service.GetYarnSubProgramNews(yarnTypeId);
            return Ok(list);
        }
        [Route("certification/{yarnTypeId}/{yarnSubProgranNewId}")]
        [HttpGet]
        public async Task<IActionResult> GetCertifications(string yarnTypeId, string yarnSubProgranNewId) //yarnTypeId = FiberId
        {
            var list = await _service.GetCertifications(yarnTypeId, yarnSubProgranNewId);
            return Ok(list);
        }
        #endregion

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

            await _service.SaveAsync(entity, AppUser.UserCode);

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

            await _service.SaveAsync(entity, AppUser.UserCode);

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

            await _service.SaveAsync(entity, AppUser.UserCode);

            return Ok();
        }

        [Route("revise")]
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> Revise(dynamic jsnString)
        {
            List<FreeConceptMRMaster> models = JsonConvert.DeserializeObject<List<FreeConceptMRMaster>>(
              Convert.ToString(jsnString),
              new JsonSerializerSettings
              {
                  DateTimeZoneHandling = DateTimeZoneHandling.Local // Ensures the date is interpreted as local time
              });
            string grpConceptNo = models.First().GroupConceptNo;
            bool isModified = models.First().IsModified;

            List<FreeConceptMRChild> childRecords = new List<FreeConceptMRChild>();
            models.ForEach(mr =>
            {
                childRecords.AddRange(mr.Childs);
            });

            // Set Item master Id.
            _itemMasterRepository.GenerateItem(AppConstants.ITEM_SUB_GROUP_YARN_NEW, ref childRecords);

            List<FreeConceptMaster> concepts = await _serviceConcept.GetDatasAsync(grpConceptNo);
            int revisionNoFreeConcept = concepts.Max(x => x.RevisionNo);

            FreeConceptMRMaster entity = new FreeConceptMRMaster();
            string fcmrChildIds = "";
            List<FreeConceptMRMaster> entities = await _service.GetMultiDetailsAsync(grpConceptNo);
            int revisionNo = entities.Max(x => x.RevisionNo);
            revisionNo += 1;
            entities.ForEach(x =>
            {
                x.EntityState = EntityState.Unchanged;
                x.Childs.SetUnchanged();
                string fChilds = fcmrChildIds.Length == 0 ? string.Join(",", x.Childs.Select(y => y.FCMRChildID))
                                                         : "," + string.Join(",", x.Childs.Select(y => y.FCMRChildID));

                if (fChilds.Trim() != "," && fChilds.Trim().Length > 1)
                {
                    fcmrChildIds += fChilds;
                }
            });

            models.ForEach(item =>
            {
                entity = entities.FirstOrDefault(x => x.ConceptID == item.ConceptID);
                if (entity == null)
                {
                    entity = item;
                    entity.AddedBy = AppUser.UserCode;
                    entity.PreProcessRevNo = revisionNoFreeConcept;
                    entity.RevisionNo = revisionNo;
                    entity.Childs.ForEach(child =>
                    {
                        child.ItemMasterID = childRecords.Find(x => x.FCMRChildID == child.FCMRChildID).ItemMasterID;
                        child.YarnCategory = CommonFunction.GetYarnShortForm(child.Segment1ValueDesc, child.Segment2ValueDesc, child.Segment3ValueDesc, child.Segment4ValueDesc, child.Segment5ValueDesc, child.Segment6ValueDesc, child.ShadeCode);
                        child.Acknowledge = true;
                        child.AcknowledgeBy = AppUser.UserCode;
                        child.AcknowledgeDate = DateTime.Now;
                        child.Reject = false;
                        child.EntityState = EntityState.Added;
                    });
                    entities.Add(entity);
                }
                else
                {
                    entity.PreProcessRevNo = revisionNoFreeConcept;
                    entity.RevisionNo = revisionNo;
                    entity.RevisionDate = DateTime.Now;
                    entity.RevisionBy = AppUser.UserCode;
                    entity.RevisionReason = "";
                    entity.ReqDate = item.ReqDate;
                    entity.UpdatedBy = AppUser.UserCode;
                    entity.DateUpdated = DateTime.Now;
                    entity.EntityState = EntityState.Modified;

                    item.Childs.ForEach(mrChild =>
                    {
                        FreeConceptMRChild child = entity.Childs.FirstOrDefault(x => x.FCMRChildID == mrChild.FCMRChildID);
                        if (child == null)
                        {
                            child = mrChild;
                            child.FCMRMasterID = entity.FCMRMasterID;
                            child.YarnCategory = CommonFunction.GetYarnShortForm(mrChild.Segment1ValueDesc, mrChild.Segment2ValueDesc, mrChild.Segment3ValueDesc, mrChild.Segment4ValueDesc, mrChild.Segment5ValueDesc, mrChild.Segment6ValueDesc, mrChild.ShadeCode);
                            child.Acknowledge = true;
                            child.AcknowledgeBy = AppUser.UserCode;
                            child.AcknowledgeDate = DateTime.Now;
                            child.Reject = false;
                            child.EntityState = EntityState.Added;
                            entity.Childs.Add(child);
                        }
                        else
                        {
                            child.ShadeCode = mrChild.ShadeCode;
                            child.YD = mrChild.YD;
                            child.YDItem = mrChild.YDItem;
                            child.IsPR = mrChild.IsPR;
                            child.ReqQty = mrChild.ReqQty;
                            child.ReqCone = mrChild.ReqCone;
                            child.ItemMasterID = mrChild.ItemMasterID;
                            child.YarnCategory = CommonFunction.GetYarnShortForm(mrChild.Segment1ValueDesc, mrChild.Segment2ValueDesc, mrChild.Segment3ValueDesc, mrChild.Segment4ValueDesc, mrChild.Segment5ValueDesc, mrChild.Segment6ValueDesc, mrChild.ShadeCode);
                            child.Acknowledge = true;
                            child.AcknowledgeBy = AppUser.UserCode;
                            child.AcknowledgeDate = DateTime.Now;
                            child.Reject = false;
                            child.YarnStockSetId = mrChild.YarnStockSetId;
                            child.DayValidDurationId = mrChild.DayValidDurationId;
                            child.EntityState = EntityState.Modified;
                        }
                    });
                    entity.Childs.Where(x => x.EntityState == EntityState.Unchanged).ToList().ForEach(x =>
                    {
                        x.EntityState = EntityState.Deleted;
                    });
                }
            });
            await _service.ReviseAsync(entities, grpConceptNo, AppUser.UserCode, fcmrChildIds);
            //await _commonService.UpdateFreeConceptStatus(InterfaceFrom.MaterialRequirement, 0, grpConceptNo);
            return Ok();
        }
    }
}