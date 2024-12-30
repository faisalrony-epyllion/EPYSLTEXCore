using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.API.Extends.Filters;
using EPYSLTEXCore.Application.Interfaces.Booking;
using EPYSLTEXCore.Application.Interfaces.RND;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities.Gmt.Booking;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Booking;
using EPYSLTEXCore.Infrastructure.Entities.Tex.CountEntities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Fabric;
using EPYSLTEXCore.Infrastructure.Entities.Tex.General;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.Entity;
using System.Text.Json;
using Newtonsoft.Json;
using EPYSLTEXCore.Application.Interfaces;

namespace EPYSLTEXCore.API.Contollers.Booking
{
    [Route("api/yarn-booking")]
    public class YarnBookingController : ApiBaseController
    {
        private readonly IYarnBookingService _service;

        public YarnBookingController(IUserService userService
            , IYarnBookingService service
            ) : base(userService)
        {
            _service = service;
        }

        [HttpGet]
        [Route("forBulk/{bookingNo}/{isSample}")]
        public async Task<IActionResult> GetYBForBulkAsync(string bookingNo, bool isSample)
        {
            YarnBookingMaster record = await _service.GetYBForBulkAsync(bookingNo, isSample);

            return Ok(record);
        }

        /*
        private readonly IYarnBookingService _service;
        private readonly IFreeConceptService _serviceFreeConcept;
        private readonly IFreeConceptMRService _serviceFreeConceptMR;
        private readonly IEmailService _emailService;
        private readonly ICommonService _commonService;
        private static Logger _logger;
        private readonly ItemMasterRepository<YarnBookingChildYItem> _itemMasterRepository;
        public YarnBookingController(IYarnBookingService service,
            ItemMasterRepository<YarnBookingChildYItem> itemMasterRepository,
            ISignatureRepository signatureRepository,
            IFreeConceptService serviceFreeConcept,
            IFreeConceptMRService serviceFreeConceptMR,
            IEmailService emailService,
            ICommonService commonService)
        {
            _service = service;
            _itemMasterRepository = itemMasterRepository;
            _emailService = emailService;
            _logger = LogManager.GetCurrentClassLogger();
            _serviceFreeConcept = serviceFreeConcept;
            _serviceFreeConceptMR = serviceFreeConceptMR;
            _commonService = commonService;
        }
        */
    }
}
