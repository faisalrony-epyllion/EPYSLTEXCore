using EPYSLTEXCore.Application.Interfaces.YarnProductSetup;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Static;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

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
        public async Task<List<YarnProductSetupChild>> GetAlYarnProductSetupChildBySetupMasterID(int setupMasterID)
        {
            

            var query =@"Select SetupChildID,SetupMasterID,BlendTypeID,YarnTypeID,ProgramID,SubProgramID,CertificationsID,TechnicalParameterID,CompositionsID,ShadeID,ManufacturingLineID,
                        ManufacturingProcessID,ManufacturingSubProcessID,YarnColorID,ColorGradeID  
                            From YarnProductSetupChild  where SetupMasterID=@SetupMasterID";


          

            return await _gmtService.GetDataAsync<YarnProductSetupChild>(query, new { setupMasterID });

        }

       
    }
}
