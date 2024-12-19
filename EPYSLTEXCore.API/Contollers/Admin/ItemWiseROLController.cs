using AutoMapper;
using Azure.Core;
using EPYSLTEX.Core.Interfaces;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.API.Extends.Filters;
using EPYSLTEXCore.Application.Interfaces;
using EPYSLTEXCore.Application.Interfaces.Admin;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Admin;
using EPYSLTEXCore.Infrastructure.Entities.Tex.General;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Knitting;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Yarn;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Logging;
using Microsoft.SqlServer.Server;
using Newtonsoft.Json;
using NLog;
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
            ItemMasterReOrderStatus model = JsonConvert.DeserializeObject<ItemMasterReOrderStatus>(Convert.ToString(jsonString));
            ItemMasterReOrderStatus entity = new ItemMasterReOrderStatus();

            if (model.ROSID > 0)
            {
                entity = await _service.GetAsync(model.ROSID);
                entity.EntityState = EntityState.Modified;
                entity.MonthlyAvgConsumption = model.MonthlyAvgConsumption;
                entity.LeadTimeDays = model.LeadTimeDays;
                entity.SafetyStockDays = model.SafetyStockDays;
                entity.MonthlyWorkingDays = model.MonthlyWorkingDays;
                entity.PackSize = model.PackSize;
                entity.MOQ = model.MOQ;
            }
            else
            {
                entity.UnitID = 28;
                entity = model;
                entity.EntityState = EntityState.Added;
                entity.AddedBy = AppUser.UserCode;
                entity.DateAdded = DateTime.Now;
                //entity.ProposeDate = null;
                //entity.AcknowledgeDate = null;
                //entity.ApproveDate = null;
                //entity.UnApproveDate = null;
                //entity.LeadTimeProposeDate = null;
                //entity.LeadTimeApproveDate = null;
                //entity.LeadTimeUnApproveDate = null;
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
        [Route("getitemmaster")]
        public async Task<IActionResult> GetItemMaster()
        {
            var paginationInfo = Request.GetPaginationInfo();
            var records = await _service.GetItemMasterDataAsync(paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }
    }
}
