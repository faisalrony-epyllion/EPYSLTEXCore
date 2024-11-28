using EPYSLTEX.Core.DTOs;
using EPYSLTEX.Core.Entities.Gmt;
using EPYSLTEXCore.Application.DTO;
using EPYSLTEXCore.Application.Entities;
using EPYSLTEXCore.Application.Interfaces.YarnProductSetup;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Static;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Application.Services
{

    public class YarnProductSetupService : IYarnProductSetupService
    {

        private readonly IDapperCRUDService<LoginUser> _gmtService;


        private readonly IConfiguration _configuration;
        private readonly SqlConnection _connection = null;

   
        public YarnProductSetupService (IConfiguration configuration , IDapperCRUDService<LoginUser> gmtService)
        {

            _configuration = configuration;
            _connection = new SqlConnection(_configuration.GetConnectionString(AppConstants.TEXTILE_CONNECTION));
            _gmtService = gmtService;
            _gmtService.Connection = _connection;

        }

  
        public async Task<List<YarnProductSetupFinder>> GetAllFiberType(PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By LEN(FiberType), FiberType ASC" : paginationInfo.OrderBy;

            var query = $@"
                With F As (
                        Select SetupMasterID, FiberTypeID, b.SegmentValue FiberType  
                            From YarnProductSetupMaster a
                        Inner Join   {DbNames.EPYSL}..ItemSegmentValue b on b.SegmentValueID = a.FiberTypeID
                 )
                Select *, COUNT(*) Over() TotalRows  From F
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";


            return await _gmtService.GetDataAsync<YarnProductSetupFinder>(query);

        }

        
    }
}
