using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.API.Extends.Filters;
using EPYSLTEXCore.Application.Interfaces;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities.Tex.General;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data.Entity;

namespace EPYSLTEXCore.API.Contollers.Admin
{//this is a rnd controller 
    [Route("api/spinner-wise-yarn-packing-hk-test")]
    public class SpinnerWiseYarnPackingHK_TestController : ApiBaseController
    {
        //private readonly IEfRepository<YarnProductSetupSupplier> _YarnProductSetupSupplierRepository;
        private readonly ISpinnerWiseYarnPackingHK_TestService _service;
        //private readonly IMapper _mapper;

        public SpinnerWiseYarnPackingHK_TestController(
            //IEfRepository<YarnProductSetupSupplier> YarnProductSetupSupplierRepository,
            IUserService userService, ISpinnerWiseYarnPackingHK_TestService SegmentFilterSetupService
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
            List<SpinnerWiseYarnPackingHK> records = await _service.GetPagedAsync(paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }
        [Route("list/{id}")]
        [HttpDelete]
        public async Task<IActionResult> GetList(int id)
        {

            SpinnerWiseYarnPackingHK entity = new SpinnerWiseYarnPackingHK();


            entity = await _service.GetAsync(id);

            entity.EntityState = EntityState.Deleted;

            await _service.SaveAsync(entity);
            return Ok();
        }
        [Route("list")]
        [HttpPut]
        public async Task<IActionResult> Update(dynamic jsonString)
        {
            SpinnerWiseYarnPackingHK model = JsonConvert.DeserializeObject<SpinnerWiseYarnPackingHK>(Convert.ToString(jsonString));
            SpinnerWiseYarnPackingHK entity = new SpinnerWiseYarnPackingHK();

            entity = await _service.GetAsync(model.YarnPackingID);
            entity.SpinnerID = model.SpinnerID;
            entity.PackNo = model.PackNo;
            entity.Cone = model.Cone;
            entity.NetWeight = model.NetWeight;
            entity.EntityState = EntityState.Modified;

            await _service.SaveAsync(entity);
            return Ok();
        }
        [Route("save")]
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> Save(dynamic jsonString)
        {
            SpinnerWiseYarnPackingHK model = JsonConvert.DeserializeObject<SpinnerWiseYarnPackingHK>(Convert.ToString(jsonString));
            SpinnerWiseYarnPackingHK entity = new SpinnerWiseYarnPackingHK();

            if (model.YarnPackingID > 0)
            {
                entity = await _service.GetAsync(model.YarnPackingID);
                entity.EntityState = EntityState.Modified;
                entity.SpinnerID = model.SpinnerID;
                entity.PackNo = model.PackNo;
                entity.Cone = model.Cone;
                entity.NetWeight = model.NetWeight;
                entity.UpdatedBy = AppUser.UserCode;
                entity.DateUpdated = DateTime.Now;
            }
            else
            {
                entity = model;
                entity.EntityState = EntityState.Added;
                entity.AddedBy = AppUser.UserCode;
                entity.DateAdded = DateTime.Now;
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
    }
}
