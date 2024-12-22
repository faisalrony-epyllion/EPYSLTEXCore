using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.API.Extends.Filters;
using EPYSLTEXCore.Application.Interfaces;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities.Tex.General;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data.Entity;

namespace EPYSLTEXCore.API.Contollers.Admin
{
    [Route("api/item-wise-rol")]
    public class ItemWiseROLController : ApiBaseController
    {
        //private readonly IEfRepository<YarnProductSetupSupplier> _YarnProductSetupSupplierRepository;
        private readonly IItemWiseROLService _service;
        //private readonly IMapper _mapper;

        public ItemWiseROLController(
            //IEfRepository<YarnProductSetupSupplier> YarnProductSetupSupplierRepository,
            IUserService userService, IItemWiseROLService SegmentFilterSetupService
            //, IMapper mapper
            ) : base(userService)
        {
            // _YarnProductSetupSupplierRepository = YarnProductSetupSupplierRepository;
            _service = SegmentFilterSetupService;
            //_mapper = mapper;
        }

        [Route("list")]
        public async Task<IActionResult> GetList()
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<ItemMasterReOrderStatus> records = await _service.GetPagedAsync(paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }
        [Route("save")]
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> Save(dynamic jsonString)
        {
            //ItemMasterReOrderStatus model = JsonConvert.DeserializeObject<ItemMasterReOrderStatus>(Convert.ToString(jsonString));
            ItemMasterReOrderStatus model = JsonConvert.DeserializeObject<ItemMasterReOrderStatus>
                                            (
                                                Convert.ToString(jsonString),
                                                new JsonSerializerSettings
                                                {
                                                    DateTimeZoneHandling = DateTimeZoneHandling.Local
                                                }
                                            );
            ItemMasterReOrderStatus entity = new ItemMasterReOrderStatus();
            if (model.ROSID > 0)
            {
                entity = await _service.GetAsync(model.ROSID);
                entity.EntityState = EntityState.Modified;
                entity.MonthlyAvgConsumptionLP = model.MonthlyAvgConsumptionLP;
                entity.MonthlyAvgConsumptionFP = model.MonthlyAvgConsumptionFP;
                entity.ROLLocalPurchase = model.ROLLocalPurchase;
                entity.ROLForeignPurchase = model.ROLForeignPurchase;
                entity.ReOrderQty = model.ROLLocalPurchase + model.ROLForeignPurchase;
                entity.MaximumPRQtyLP = model.MaximumPRQtyLP;
                entity.MaximumPRQtyFP = model.MaximumPRQtyFP;
                entity.MOQ = model.MaximumPRQtyLP + model.MaximumPRQtyFP;
                entity.ValidDate = model.ValidDate;
            }
            else
            {
                entity.UnitID = 28;
                entity = model;
                entity.EntityState = EntityState.Added;
                entity.AddedBy = AppUser.UserCode;
                entity.DateAdded = DateTime.Now;
                entity.ReOrderQty = entity.ROLLocalPurchase + entity.ROLForeignPurchase;
                entity.MOQ = entity.MaximumPRQtyLP + entity.MaximumPRQtyFP;
            }
            await _service.SaveAsync(entity);
            return Ok();
        }
        [HttpGet]
        [Route("GetMaster")]
        public async Task<IActionResult> GetMaster()
        {
            var data = await _service.GetMaster();
            return Ok(data);
        }
        [HttpGet]
        [Route("get-item-master")]
        public async Task<IActionResult> GetItemMaster()
        {
            var paginationInfo = Request.GetPaginationInfo();
            var records = await _service.GetItemMasterDataAsync(paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }
    }
}
