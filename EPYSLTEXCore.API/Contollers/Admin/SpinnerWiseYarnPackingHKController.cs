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
{
    [Route("api/spinner-wise-yarn-packing-hk")]
    public class SpinnerWiseYarnPackingHKController : ApiBaseController
    {
        //private readonly IEfRepository<YarnProductSetupSupplier> _YarnProductSetupSupplierRepository;
        private readonly ISpinnerWiseYarnPackingHKService _service;
        //private readonly IMapper _mapper;

        public SpinnerWiseYarnPackingHKController(
            //IEfRepository<YarnProductSetupSupplier> YarnProductSetupSupplierRepository,
            IUserService userService, ISpinnerWiseYarnPackingHKService SegmentFilterSetupService
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
            List<SpinnerWiseYarnPackingHK> entities = new List<SpinnerWiseYarnPackingHK>();
            entity = model;
            //foreach (var model in models)
            //{
            //if (model.EntityState == EntityState.Added)
            //{
            //entity = await _YarnProductSetupSupplierRepository.FindAsync(model.Id);
            //Guard.Against.NullEntity(model.Id, entity);

            entity.AddedBy = 1;
            entity.DateAdded = DateTime.Now;
            entity.UpdatedBy = 1;
            entity.DateUpdated = DateTime.Now;

            entity.EntityState = EntityState.Added;


            //}

            //entities.Add(entity);
            //}
            var preData = await _service.GetAsync(entity);
            if (preData.Count == 0)
            {
                await _service.SaveAsync(entity);
            }
            else
            {
                return BadRequest("Duplicate Packing Setup!");
            }

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
