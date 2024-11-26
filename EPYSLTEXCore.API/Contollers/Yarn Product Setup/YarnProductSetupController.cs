using AutoMapper;
using EPYSLTEXCore.Application.Entities;
using EPYSLTEXCore.Application.Interfaces;
using EPYSLTEXCore.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EPYSLTEXCore.API.Contollers.Yarn_Product_Setup
{
    [Route("api/[controller]")]
    [ApiController]
    public class YarnProductSetupController : ControllerBase
    {
        private readonly IReportAPISetupService _setupService;

        public YarnProductSetupController(IReportAPISetupService reportAPISetupService) { 
            _setupService = reportAPISetupService;
        }
        [HttpPost]
        [Route("Save")]
        public async Task<IActionResult> SaveYarnProductSetup(dynamic entity)
        {
             

             var cc= JsonConvert.DeserializeObject<YarnProductSetup>((Convert.ToString(entity)));
            // Initialize AutoMapper
            var configuration = new MapperConfiguration(cfg => cfg.CreateMap<dynamic, YarnProductSetup>());
            var mapper = configuration.CreateMapper();

            // Map dynamic object to Product model
          //  var product = mapper.Map<YarnProductSetup>(entity);

          //  var s =  await _setupService.AddNestedAsync(entity);

            return Ok();
        }
    }
}
