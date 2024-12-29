using EPYSLTEX.Core.Interfaces;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.API.Extends.Filters;
using EPYSLTEXCore.Application.Interfaces;
using EPYSLTEXCore.Application.Interfaces.Admin;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities.Tex;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Admin;
using EPYSLTEXCore.Infrastructure.Entities.Tex.General;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Knitting;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Exceptions;
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
    [Route("api/dyeing-machine")]
    public class DyeingMachineController : ApiBaseController
    {
        private readonly IDyeingMachineService _service;

        public DyeingMachineController(IUserService userService, IDyeingMachineService service) : base(userService)
        {
            _service = service;
        }

        [Route("list")]
        [HttpGet]
        public async Task<IActionResult> GetList()
        {
            var paginationInfo = Request.GetPaginationInfo();
            var records = await _service.GetPagedAsync(paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        [Route("nozzle-list")]
        [HttpGet]
        public async Task<IActionResult> GetNozzleList()
        {
            var paginationInfo = Request.GetPaginationInfo();
            var records = await _service.GetNozzleInfoAsync(paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        [HttpGet]
        [Route("new")]
        public async Task<IActionResult> GetNew()
        {
            return Ok(await _service.GetNewAsync());
        }

        [Route("{id}")]
        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var record = await _service.GetAsync(id);
            Guard.Against.NullObject(id, record);

            return Ok(record);
        }

        [Route("dyeing-machine-by-nozzle/{nozzle}")]
        [HttpGet]
        public async Task<IActionResult> GetDyeingMachineList(int nozzle)
        {
            var paginationInfo = Request.GetPaginationInfo();
            var records = await _service.GetDyeingMachineByNozzleListAsync(nozzle);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        [Route("save")]
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> Save(dynamic jsonString)
        {
            DyeingMachine entity;
            DyeingMachine model = JsonConvert.DeserializeObject<DyeingMachine>(Convert.ToString(jsonString));

            if (model.DMID > 0)
            {
                entity = await _service.GetAllAsync(model.DMID);

                entity.DyeingMcNameId = model.DyeingMcNameId;
                entity.CompanyId = model.CompanyId;
                entity.DyeingMcslNo = model.DyeingMcslNo;
                entity.DyeingMcStatusId = model.DyeingMcStatusId;
                entity.DyeingMcBrandId = model.DyeingMcBrandId;
                entity.DyeingMcCapacity = model.DyeingMcCapacity;
                entity.DyeingNozzleQty = model.DyeingNozzleQty;
                entity.IsCC = model.IsCC;
                entity.EntityState = EntityState.Modified;
                entity.DyeingMachineProcesses.SetUnchanged();

                foreach (var item in model.DyeingMachineProcesses)
                {
                    var childEntity = entity.DyeingMachineProcesses.FirstOrDefault(x => x.DMProcessID == item.DMProcessID);

                    if (childEntity == null)
                    {
                        childEntity = item;
                        item.DMID = entity.DMID;
                        childEntity.EntityState = EntityState.Added;
                        entity.DyeingMachineProcesses.Add(childEntity);
                    }
                    else
                    {
                        childEntity.DyeProcessID = item.DyeProcessID;
                        childEntity.EntityState = EntityState.Modified;
                    }
                }
                foreach (var item in entity.DyeingMachineProcesses.Where(x => x.EntityState == EntityState.Unchanged))
                {
                    item.EntityState = EntityState.Deleted;
                }
            }
            else
            {
                entity = model;
            }

            await _service.SaveAsync(entity);

            return Ok();
        }
    }
}
