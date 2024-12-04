using AutoMapper;
using EPYSLTEX.Core.Interfaces;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEX.Core.Statics;
using EPYSLTEX.Infrastructure.Services;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.Application.Interfaces;
using EPYSLTEXCore.Application.Interfaces.Booking;
using EPYSLTEXCore.Application.Interfaces.Repositories;
using EPYSLTEXCore.Application.Interfaces.RND;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities.General;
using EPYSLTEXCore.Infrastructure.Entities.Gmt.General.Item;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
//using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.Entity;
namespace EPYSLTEXCore.API.Contollers.RND
{

    [Route("api/rnd-free-concept")]

    public class FreeConceptController : ApiBaseController
    {
        private readonly IGmtEfRepository<ItemSegmentName> _itemSegmentNameRepository;
        private readonly IGmtEfRepository<ItemSegmentValue> _itemSegmentValueRepository;
        private readonly IFreeConceptService _service;
        private readonly IMapper _mapper;
        private readonly ICommonHelperService _commonService;
        private readonly IDapperCRUDService<FreeConceptMaster> _signatureRepository;
        private bool isModified;
        //IUserService _userService;

        public FreeConceptController(IUserService userService,IGmtEfRepository<ItemSegmentName> itemSegmentNameRepository
            , IGmtEfRepository<ItemSegmentValue> itemSegmentRepository
            , IFreeConceptService freeConceptService
            , ICommonHelperService commonService
            , IDapperCRUDService<FreeConceptMaster> signatureRepository
            , IMapper mapper) : base(userService)
        {
            _itemSegmentNameRepository = itemSegmentNameRepository;
            _itemSegmentValueRepository = itemSegmentRepository;
            _service = freeConceptService;
            _mapper = mapper;
            _commonService = commonService;
            _signatureRepository = signatureRepository;
        }

        [Route("list")]
        [HttpGet]
        public async Task<IActionResult> GetList(FreeConceptStatus status)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<FreeConceptMaster> records = await _service.GetPagedAsync(status, paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        [Route("technical-names/{subClassId}")]
        [HttpGet]
        public async Task<IActionResult> GetTechnicalNameList(int subClassId)
        {
            return Ok(await _service.GetTechnicalNameList(subClassId));
        }

        [HttpGet]
        [Route("new")]
        public async Task<IActionResult> GetNew()
        {
            return Ok(await _service.GetNewAsync());
        }

        [Route("revision/{id}/{subClassId}")]
        [HttpGet]
        public async Task<IActionResult> GetRevisionList(int id, int subClassId)
        {
            FreeConceptMaster record = await _service.GetRevisionListAsync(id, subClassId);
            Guard.Against.NullObject(id, record);

            return Ok(record);
        }

        [Route("{id}/{subClassId}")]
        [HttpGet]
        public async Task<IActionResult> Get(int id, int subClassId)
        {
            FreeConceptMaster record = await _service.GetAsync(id, subClassId);
            Guard.Against.NullObject(id, record);

            return Ok(record);
        }

        [Route("technicalName-by-mc/{MachineSubClassID}")]
        [HttpGet]
        public async Task<IActionResult> GetTechnicalNameByMC(int MachineSubClassID)
        {
            var records = await _service.GetTechnicalNameByMC(MachineSubClassID);
            return Ok(records);
        }

        [Route("by-group-concept/{grpConceptNo}/{conceptTypeID}")]
        [HttpGet]
        public async Task<IActionResult> GetByGroupConcept(string grpConceptNo, int conceptTypeID)
        {
            FreeConceptMaster record = await _service.GetByGroupConceptAsync(grpConceptNo, conceptTypeID);
            return Ok(record);
        }

        [HttpPost]
        [Route("updateColor")]
        public async Task<IActionResult> Update(List<FreeConceptChildColor> models)
        {
            int conceptID = models.Count() > 0 ? models.Max(x => x.ConceptID) : 0;

            List<FreeConceptChildColor> entities = new List<FreeConceptChildColor>();
            entities = await _service.GetChildColorDatasAsync(conceptID);
            entities.SetUnchanged();

            models.ToList().ForEach(color =>
            {
                FreeConceptChildColor fcColor = entities.FirstOrDefault(c => c.ColorId == color.ColorId);
                if (fcColor == null)
                {
                    fcColor = color;
                    fcColor.ConceptID = conceptID;
                    fcColor.EntityState = EntityState.Added;
                    entities.Add(fcColor);
                }
                else
                {
                    fcColor.Remarks = color.Remarks;
                    fcColor.EntityState = EntityState.Modified;
                }
            });

            entities.Where(x => x.EntityState == EntityState.Unchanged).SetDeleted();

            await _service.SaveAsyncChildColor(entities);
            return Ok(entities);
        }
        [Route("save")]
        [HttpPost]
        public async Task<IActionResult> Save(List<FreeConceptMaster> models)
        {
            string grpConceptNo = models.First().GroupConceptNo;
            int conceptID = 0;
            int conceptTypeID = models.First().ConceptTypeID;

            List<FreeConceptMaster> entities = new List<FreeConceptMaster>();
            FreeConceptMaster entity;
            if (grpConceptNo != AppConstants.NEW)
            {
                entities = await _service.GetDatasAsync(grpConceptNo);

                entities.ForEach(x =>
                {
                    x.EntityState = EntityState.Unchanged;
                    x.ChildColors.SetUnchanged();
                });

                models.ForEach(item =>
                {
                    if (item.ConceptNo == item.GroupConceptNo) conceptID = item.ConceptID;

                    entity = entities.FirstOrDefault(x => x.ConceptID == item.ConceptID);

                    if (item.SubGroupID == 0)
                    {
                        if (item.FUPartID == 5 || item.FUPartID == 19) item.SubGroupID = 11; //19=Hem, 5=Collar
                        else if (item.FUPartID == 6) item.SubGroupID = 12; //6=Cuff
                    }

                    if (entity == null)
                    {
                        entity = item;
                        entity.ConceptID = 0;
                        entity.ConceptTypeID = conceptTypeID;
                        entity.AddedBy = AppUser.UserCode;
                        entity.ConceptStatusId = EntityConceptStatus.RUNNING;
                        entity.ConsumptionID = entity.ConsumptionID.IsNullOrDBNull() ? 0 : entity.ConsumptionID;

                        entity.ExcessPercentage = 0;
                        entity.ExcessQty = 0;
                        entity.ExcessQtyInKG = 0;
                        entity.TotalQty = entity.Qty + entity.ExcessQty;

                        entity.QtyInKG = (Convert.ToDecimal(entity.Length) *
                                         Convert.ToDecimal(entity.Width) *
                                         Convert.ToDecimal(0.045) *
                                         entity.BookingQty) / 420;

                        entity.TotalQtyInKG = (Convert.ToDecimal(entity.Length) *
                                        Convert.ToDecimal(entity.Width) *
                                        Convert.ToDecimal(0.045) *
                                        entity.TotalQty) / 420;

                        entity.ExportOrderID = entity.ExportOrderID.IsNullOrDBNull() ? 0 : entity.ExportOrderID;
                        entity.BuyerID = entity.BuyerID.IsNullOrDBNull() ? 0 : entity.BuyerID;
                        entity.BuyerTeamID = entity.BuyerTeamID.IsNullOrDBNull() ? 0 : entity.BuyerTeamID;
                        entities.Add(entity);
                    }
                    else
                    {
                        entity.ConceptTypeID = conceptTypeID;
                        entity.KnittingTypeID = item.KnittingTypeID;
                        entity.MCSubClassID = item.MCSubClassID;
                        entity.SubGroupID = item.SubGroupID;
                        entity.ConstructionId = item.ConstructionId;
                        entity.CompositionId = item.CompositionId;
                        entity.TechnicalNameId = item.TechnicalNameId;
                        entity.GSMId = item.GSMId;
                        entity.Qty = item.Qty;
                        entity.Remarks = item.Remarks;
                        entity.UpdatedBy = AppUser.UserCode;
                        entity.DateUpdated = DateTime.Now;
                        entity.RevisionPending = false;
                        entity.FUPartID = item.FUPartID;
                        entity.IsYD = item.IsYD;
                        entity.MachineGauge = item.MachineGauge;
                        entity.Length = item.Length;
                        entity.Width = item.Width;
                        entity.ConsumptionID = entity.ConsumptionID.IsNullOrDBNull() ? 0 : entity.ConsumptionID;

                        entity.ExcessPercentage = 0;
                        entity.ExcessQty = 0;
                        entity.ExcessQtyInKG = 0;
                        entity.TotalQty = entity.Qty + entity.ExcessQty;

                        entity.QtyInKG = (Convert.ToDecimal(entity.Length) *
                                        Convert.ToDecimal(entity.Width) *
                                        Convert.ToDecimal(0.045) *
                                        entity.BookingQty) / 420;

                        entity.TotalQtyInKG = (Convert.ToDecimal(entity.Length) *
                                        Convert.ToDecimal(entity.Width) *
                                        Convert.ToDecimal(0.045) *
                                        entity.TotalQty) / 420;

                        entity.ExportOrderID = entity.ExportOrderID.IsNullOrDBNull() ? 0 : entity.ExportOrderID;
                        entity.BuyerID = entity.BuyerID.IsNullOrDBNull() ? 0 : entity.BuyerID;
                        entity.BuyerTeamID = entity.BuyerTeamID.IsNullOrDBNull() ? 0 : entity.BuyerTeamID;

                        if (entity.CompanyID == 0) entity.CompanyID = CompnayIDConstants.EFL;
                        entity.EntityState = EntityState.Modified;

                        item.ChildColors.ForEach(color =>
                        {
                            FreeConceptChildColor fcColor = entity.ChildColors.FirstOrDefault(c => c.ColorId == color.ColorId);
                            if (fcColor == null)
                            {
                                fcColor = color;
                                fcColor.ConceptID = conceptID;
                                entity.ChildColors.Add(fcColor);
                                fcColor.EntityState = EntityState.Added;
                            }
                            else
                            {
                                fcColor.ConceptID = color.ConceptID == 0 ? item.ConceptID : color.ConceptID;
                                fcColor.ColorId = color.ColorId;
                                fcColor.ColorCode = color.ColorCode;
                                fcColor.RequestRecipe = color.RequestRecipe;
                                fcColor.RequestBy = color.RequestBy;
                                fcColor.RequestDate = color.RequestDate;
                                fcColor.RequestAck = color.RequestAck;
                                fcColor.RequestAckBy = color.RequestAckBy;
                                fcColor.RequestAckDate = color.RequestAckDate;
                                fcColor.GrayFabricOK = color.GrayFabricOK;
                                fcColor.Remarks = color.Remarks;
                                fcColor.DPID = color.DPID;
                                fcColor.DPProcessInfo = color.DPProcessInfo;
                                fcColor.ColorName = color.ColorName;
                                fcColor.EntityState = EntityState.Modified;
                            }
                        });
                    }
                });

                entities.ForEach(m =>
                {
                    if (m.EntityState == EntityState.Unchanged)
                    {
                        m.EntityState = EntityState.Deleted;
                        m.ChildColors.SetDeleted();
                    }
                    m.ChildColors.Where(c => c.EntityState == EntityState.Unchanged).ToList().ForEach(c =>
                    {
                        c.EntityState = EntityState.Deleted;
                    });
                });

                await _service.SaveManyAsync(entities, EntityState.Modified);

                await _commonService.UpdateFreeConceptStatus(InterfaceFrom.FreeConcept, 0, grpConceptNo);
            }
            else
            {
                grpConceptNo = await GetMaxGroupConceptNoAsync();
                models.ForEach(x =>
                {
                    x.ConceptTypeID = conceptTypeID;
                    x.GroupConceptNo = grpConceptNo;
                    x.AddedBy = AppUser.UserCode;
                    x.ConceptStatusId = EntityConceptStatus.RUNNING;
                    x.ConsumptionID = x.ConsumptionID.IsNullOrDBNull() ? 0 : x.ConsumptionID;

                    x.ExcessPercentage = 0;
                    x.ExcessQty = 0;
                    x.ExcessQtyInKG = 0;
                    x.TotalQty = x.Qty + x.ExcessQty;

                    x.QtyInKG = (Convert.ToDecimal(x.Length) *
                                        Convert.ToDecimal(x.Width) *
                                        Convert.ToDecimal(0.045) *
                                        x.BookingQty) / 420;

                    x.TotalQtyInKG = (Convert.ToDecimal(x.Length) *
                                    Convert.ToDecimal(x.Width) *
                                    Convert.ToDecimal(0.045) *
                                    x.TotalQty) / 420;

                    x.ExportOrderID = x.ExportOrderID.IsNullOrDBNull() ? 0 : x.ExportOrderID;
                    x.BuyerID = x.BuyerID.IsNullOrDBNull() ? 0 : x.BuyerID;
                    x.BuyerTeamID = x.BuyerTeamID.IsNullOrDBNull() ? 0 : x.BuyerTeamID;
                });
                entities = models;
                await _service.SaveManyAsync(entities, EntityState.Added);
                await _commonService.UpdateFreeConceptStatus(InterfaceFrom.FreeConcept, 0, grpConceptNo);
            }

            return Ok();
        }
        private async Task<string> GetMaxGroupConceptNoAsync()
        {
            var id = await _signatureRepository.GetMaxIdAsync(TableNames.RND_GROUP_CONCEPTNO, RepeatAfterEnum.EveryMonth);
            var datePart = DateTime.Now.ToString("yyMM");
            return $@"{datePart}{id:0000}";
        }

        [Route("revise")]
        [HttpPost]
        public async Task<IActionResult> revise(List<FreeConceptMaster> models)
        {
            string grpConceptNo = models.First().GroupConceptNo;
            int conceptID = 0;
            int conceptTypeID = models.First().ConceptTypeID;

            List<FreeConceptMaster> entities = new List<FreeConceptMaster>();
            FreeConceptMaster entity;

            entities = await _service.GetDatasAsync(grpConceptNo);

            entities.ForEach(x =>
            {
                x.EntityState = EntityState.Unchanged;
                x.ChildColors.SetUnchanged();
            });
            int revisionNo = entities.Max(x => x.RevisionNo);
            revisionNo = revisionNo + 1;

            models.ForEach(item =>
            {
                if (item.ConceptNo == item.GroupConceptNo) conceptID = item.ConceptID;

                if (item.SubGroupID == 0)
                {
                    if (item.FUPartID == 5 || item.FUPartID == 19) item.SubGroupID = 11; //19=Hem, 5=Collar
                    else if (item.FUPartID == 6) item.SubGroupID = 12; //6=Cuff
                }

                entity = entities.FirstOrDefault(x => x.ConceptID == item.ConceptID);
                if (entity == null)
                {
                    entity = item;
                    entity.RevisionNo = revisionNo;
                    entity.ConceptID = 0;
                    entity.ConceptTypeID = conceptTypeID;

                    entity.TotalQty = entity.Qty + entity.ExcessQty;

                    entity.QtyInKG = (Convert.ToDecimal(entity.Length) *
                                        Convert.ToDecimal(entity.Width) *
                                        Convert.ToDecimal(0.045) *
                                        entity.BookingQty) / 420;

                    entity.TotalQtyInKG = (Convert.ToDecimal(entity.Length) *
                                    Convert.ToDecimal(entity.Width) *
                                    Convert.ToDecimal(0.045) *
                                    entity.TotalQty) / 420;

                    entity.AddedBy = AppUser.UserCode;
                    entity.ConceptStatusId = EntityConceptStatus.RUNNING;
                    entities.Add(entity);
                }
                else
                {
                    entity.ConceptTypeID = conceptTypeID;
                    entity.KnittingTypeID = item.KnittingTypeID;
                    entity.MCSubClassID = item.MCSubClassID;
                    entity.SubGroupID = item.SubGroupID;
                    entity.ConstructionId = item.ConstructionId;
                    entity.CompositionId = item.CompositionId;
                    entity.TechnicalNameId = item.TechnicalNameId;
                    entity.GSMId = item.GSMId;
                    entity.Qty = item.Qty;
                    entity.Remarks = item.Remarks;
                    entity.UpdatedBy = AppUser.UserCode;
                    entity.DateUpdated = DateTime.Now;
                    entity.RevisionPending = false;
                    entity.FUPartID = item.FUPartID;
                    entity.IsYD = item.IsYD;
                    entity.MachineGauge = item.MachineGauge;
                    entity.Length = item.Length;
                    entity.Width = item.Width;
                    entity.PreProcessRevNo = 0;
                    entity.RevisionNo = revisionNo;
                    entity.RevisionDate = DateTime.Now;
                    entity.RevisionBy = AppUser.UserCode;
                    entity.RevisionReason = "";

                    entity.TotalQty = entity.Qty + entity.ExcessQty;

                    entity.QtyInKG = (Convert.ToDecimal(entity.Length) *
                                        Convert.ToDecimal(entity.Width) *
                                        Convert.ToDecimal(0.045) *
                                        entity.BookingQty) / 420;

                    entity.TotalQtyInKG = (Convert.ToDecimal(entity.Length) *
                                    Convert.ToDecimal(entity.Width) *
                                    Convert.ToDecimal(0.045) *
                                    entity.TotalQty) / 420;

                    entity.UpdatedBy = AppUser.UserCode;
                    entity.DateUpdated = DateTime.Now;
                    if (entity.CompanyID == 0) entity.CompanyID = CompnayIDConstants.EFL;
                    entity.EntityState = EntityState.Modified;

                    item.ChildColors.ForEach(color =>
                    {
                        FreeConceptChildColor fcColor = entity.ChildColors.FirstOrDefault(c => c.ColorId == color.ColorId);
                        if (fcColor == null)
                        {
                            fcColor = color;
                            fcColor.ConceptID = conceptID;
                            entity.ChildColors.Add(fcColor);
                            fcColor.EntityState = EntityState.Added;
                        }
                        else
                        {
                            fcColor.ConceptID = color.ConceptID;
                            fcColor.ColorId = color.ColorId;
                            fcColor.ColorCode = color.ColorCode;
                            fcColor.RequestRecipe = color.RequestRecipe;
                            fcColor.RequestBy = color.RequestBy;
                            fcColor.RequestDate = color.RequestDate;
                            fcColor.RequestAck = color.RequestAck;
                            fcColor.RequestAckBy = color.RequestAckBy;
                            fcColor.RequestAckDate = color.RequestAckDate;
                            fcColor.GrayFabricOK = color.GrayFabricOK;
                            fcColor.Remarks = color.Remarks;
                            fcColor.DPID = color.DPID;
                            fcColor.DPProcessInfo = color.DPProcessInfo;
                            fcColor.ColorName = color.ColorName;
                            if (fcColor.EntityState != EntityState.Added)
                            {
                                fcColor.EntityState = EntityState.Modified;
                            }
                        }
                    });
                }
            });

            entities.Where(a => a.EntityState == EntityState.Unchanged).ToList().ForEach(x =>
            {
                x.EntityState = EntityState.Deleted;
                x.ChildColors.SetDeleted();
            });

            await _service.ReviseManyAsync(entities, grpConceptNo, EntityState.Modified);

            await _commonService.UpdateFreeConceptStatus(InterfaceFrom.FreeConcept, 0, grpConceptNo);

            return Ok();
        }

        [Route("save-fabric-composition")]
        [HttpPost]
        public async Task<IActionResult> SaveComposition(ItemSegmentValueBindingModel model)
        {
            ItemSegmentName itemSegmentName = await _itemSegmentNameRepository.FindAsync(a => a.SegmentName == ItemSegmentNameConstants.COMPOSITION);
            if (itemSegmentName.IsNull())
                return BadRequest("Composition Segment Not Found");

            if (await _itemSegmentValueRepository.ExistsAsync(x => x.SegmentNameId == itemSegmentName.Id && x.SegmentValue == model.SegmentValue))
                return BadRequest("This composition is already exists.");

            //var itemSegmentName = await _itemSegmentNameRepository.FindAsync(x => x.SegmentName == ItemSegmentNameConstants.COMPOSITION);
            //Guard.Against.NullObject(itemSegmentName);

            ItemSegmentValue entity = new ItemSegmentValue
            {
                SegmentValue = model.SegmentValue,
                SegmentNameId = itemSegmentName.Id
            };

            await _itemSegmentValueRepository.AddAsync(entity, TableNames.ITEM_SEGMENT_VALUE);

            var responseData = _mapper.Map<ItemSegmentValueBindingModel>(entity);

            return Ok(responseData);
        }
    }
}
