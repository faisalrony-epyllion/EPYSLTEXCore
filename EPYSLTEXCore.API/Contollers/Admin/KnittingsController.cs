using AutoMapper;
using EPYSLTEX.Core.Interfaces.Repositories;
using EPYSLTEX.Web.Extends.Helpers;
using EPYSLTEX.Web.Models;
using EPYSLTEXCore.Application.DTO;
using EPYSLTEXCore.Application.Entities;
using Microsoft.AspNetCore.Mvc;

namespace EPYSLTEXCore.API.Contollers.Admin
{
    [Route("api/[controller]")]
    [ApiController]
    public class KnittingsController : ControllerBase
    {
        private readonly ICommonHelpers _commonHelpers;
        private readonly IEfRepository<KnittingUnit> _knittingUnitRepository;
        private readonly IMapper _mapper;

        public KnittingsController(ICommonHelpers commonHelpers, IEfRepository<KnittingUnit> knittingUnitRepository, IMapper mapper)
        {
            _commonHelpers = commonHelpers;
            _knittingUnitRepository = knittingUnitRepository;
            _mapper = mapper;
        }


        [Route("knitting-unit")]
        public IActionResult GetKnittingUnit(int offset = 0, int limit = 10, string filter = null, string sort = null, string order = "asc")
        {
            var filterBy = _commonHelpers.GetFilterByModel(filter);
            var records = _knittingUnitRepository.ListAll(offset, limit, filterBy, sort, order, out int count);

            var response = new TableResponseModel(count, _mapper.Map<List<KnittingUnitViewModel>>(records));

            return Ok(response);
        }
    }
}
