using Dapper;
using EPYSLTEX.Core.Interfaces;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.General;
using EPYSLTEXCore.Infrastructure.Static;
using System.Data;
using System.Data.SqlClient;

namespace EPYSLTEX.Infrastructure.Services
{
    public class CommonHelperService : ICommonHelperService
    {
        private readonly IDapperCRUDService<DapperBaseEntity> _service;
        
        private SqlConnection _texConnection;

        public CommonHelperService(IDapperCRUDService<DapperBaseEntity> service)
        {
            _service = service;
        }

        #region FabricColorShade

        public async Task<List<FabricColorShade>> GetFabricColorShadeAsync(PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By FCSFID Desc" : paginationInfo.OrderBy;
            var sql = string.Empty;
            sql += $@"select *,Count(*) Over() from  {DbNames.EPYSLTEX}..FabricColorShadeFactor";

            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<FabricColorShade>(sql);
        }

        public async Task<FabricColorShade> GetAllAsyncFabricColorShade(int id)
        {
            var sql = $@"Select * From FabricColorShadeFactor Where FCSFID = {id}";
            return await _service.GetFirstOrDefaultAsync<FabricColorShade>(sql);
        }



        #endregion FabricColorShade

        #region MachineGaugeSetup

        public async Task<List<MachineGaugeSetup>> GetMachineGaugeAsync(PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By MGSID Desc" : paginationInfo.OrderBy;
            var sql = $@"
            With F As (
                SELECT MGS.ConstructionID,
			    ISV2.SegmentValue AS ConstructionName,
			    MGS.YCFrom,
			    MGS.YCTo,
			    MGS.MachineGauge,
			    MGS.MGSID                
                FROM {DbNames.EPYSLTEX}..MachineGaugeSetup MGS
                INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = MGS.ConstructionID
            )

            Select *, Count(*) Over() TotalRows 
            From F
            {paginationInfo.FilterBy}
            {orderBy}
            {paginationInfo.PageBy}";

            return await _service.GetDataAsync<MachineGaugeSetup>(sql);
        }

        public async Task<MachineGaugeSetup> GetMachineGaugeSetupDetailsAsync(int id)
        {
            var sql = $@"Select * From MachineGaugeSetup Where MGSID = {id}";
            return await _service.GetFirstOrDefaultAsync<MachineGaugeSetup>(sql);
        }



        #endregion MachineGaugeSetup

        #region Fabric Technical Name

        public async Task<List<FabricTechnicalName>> GetFabricTechnicalNameAsync(PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By TechnicalNameId Desc" : paginationInfo.OrderBy;
            var sql = string.Empty;
            sql += $@"select TechnicalNameId,TechnicalName,ConstructionId,ConstructionName, Count(*) Over() TotalRows from  {DbNames.EPYSLTEX}..FabricTechnicalName";

            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<FabricTechnicalName>(sql);
        }

        public async Task<FabricTechnicalName> GetAllAsyncFabricTechnicalName(int id)
        {
            var sql = $@"Select * From FabricTechnicalName Where TechnicalNameId = {id} ";
            return await _service.GetFirstOrDefaultAsync<FabricTechnicalName>(sql);
        }


        #endregion Fabric Technical Name

        #region TextileProcessMaster

        public async Task<List<TextileProcessMaster>> GetTextileProcessAsync(PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By TProcessMasterID Desc" : paginationInfo.OrderBy;
            var sql = string.Empty;
            sql += $@"select *,Count(*) Over() from  {DbNames.EPYSLTEX}..TextileProcessMaster";

            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<TextileProcessMaster>(sql);
        }

        public async Task<List<TextileProcessMaster>> GetTextileProcessListAsync(PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By SeqNo Desc" : paginationInfo.OrderBy;
            var sql = string.Empty;
            sql += $@"select *,Count(*) Over() from  {DbNames.EPYSLTEX}..TextileProcessMaster";

            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<TextileProcessMaster>(sql);
        }
        public async Task<TextileProcessMaster> GetAllAsyncTextileProcessMaster(int id)
        {
            var sql = $@"Select * From TextileProcessMaster Where TProcessMasterID = {id}";
            return await _service.GetFirstOrDefaultAsync<TextileProcessMaster>(sql);

        }


        #endregion TextileProcessMaster


        public async Task<List<TextileProcessUserDTO>> GetTextileProcessUser(PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By Id Desc" : paginationInfo.OrderBy;

            var sql = $@"
             With F As (
              SELECT		    TPU.TPUID AS Id,
                        TPM.TProcessMasterID,
                        TPM.ProcessName,
                        LU.UserCode,
                        (LU.UserName + ' | ' + EMP.EmployeeName + ' | ' + ED.DepertmentDescription + ' | ' + ED2.Designation) AS UserName,
                                    Count(*) Over() TotalRows
                    FROM            {DbNames.EPYSL}..LoginUser LU
                    INNER JOIN	    {DbNames.EPYSL}..Employee EMP ON EMP.EmployeeCode = LU.EmployeeCode
                    INNER JOIN		{DbNames.EPYSLTEX}..TextileProcessUser TPU ON TPU.UserCode = LU.UserCode
                    INNER JOIN		{DbNames.EPYSLTEX}..TextileProcessMaster TPM ON TPM.TProcessMasterID = TPU.TProcessMasterID
                    INNER JOIN		{DbNames.EPYSL}..EmployeeDepartment ED ON ED.DepertmentID = EMP.DepertmentID
                    INNER JOIN		{DbNames.EPYSL}..EmployeeDesignation ED2 ON ED2.DesigID = EMP.DesigID
)

            Select *, Count(*) Over() TotalRows 
            From F
            {paginationInfo.FilterBy}
            {orderBy}
            {paginationInfo.PageBy}";

            //filterBy = string.IsNullOrEmpty(filterBy) ? string.Empty : "WHERE " + filterBy;
            //orderBy = string.IsNullOrEmpty(orderBy) ? "ORDER BY TProcessMasterID" : orderBy;
            //var pageBy = string.Format(@"OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", offset, limit);

            //sql += $@"{Environment.NewLine}{filterBy}{Environment.NewLine}{orderBy}{Environment.NewLine}{pageBy}";
            return await _service.GetDataAsync<TextileProcessUserDTO>(sql);

            //var records = _gmtConnection.GetData<TextileProcessUserDTO>(sql);
            //return records;
        }
        #region Fabric Technical Name Other
        public async Task<List<FabricTechnicalNameOther>> GetFabricTechnicalNameOthers(PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By FTNOID Desc" : paginationInfo.OrderBy;
            var sql = $@"With YPL AS(
                   SELECT			FTNO.ConstructionID,
                           ISV2.SegmentValue AS ConstructionName,
                           FTNO.GSM AS Gsm,
                           FTNO.YarnCount,
                           FTNO.MachineGauge,
        	FTNO.MachineDia,
        	FTNO.StitchLength,
                           FTNO.FTNOID,
                                    Count(*) Over() TotalRows
                    FROM		    {DbNames.EPYSLTEX}..FabricTechnicalNameOthers FTNO
                    INNER JOIN	     {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = FTNO.ConstructionID
                                )
                    SELECT			ConstructionID, ConstructionName, Gsm, YarnCount, MachineGauge
        	, MachineDia, StitchLength, FTNOID
           ,Count(*) Over() TotalRows
                                    FROM YPL
            {paginationInfo.FilterBy}
            {orderBy}
            {paginationInfo.PageBy}";
            return await _service.GetDataAsync<FabricTechnicalNameOther>(sql);

   
        }

        public async Task<FabricTechnicalNameOther> GetFabricTechnicalNameOtherAsync(int id)
        {
            var sql = $@"Select * From FabricTechnicalNameOthers Where FTNOID = {id}";
            return await _service.GetFirstOrDefaultAsync<FabricTechnicalNameOther>(sql);
        }

        #endregion Fabric Technical Name Other
        

        
        public async Task<List<FabricWastageGrid>> GetFabricWastageGridAsync(string wastageFor)
        {
            var sql = $@"Select FWG.FWGID, FWG.WastageFor, FWG.IsFabric, FWG.GSMFrom, FWG.GSMTo, FWG.BookingQtyFrom, FWG.BookingQtyTo, FWG.FixedQty, FWG.ExcessQty, FWG.ExcessPercentage
                        From FabricWastageGrid FWG WHERE wastageFor='{wastageFor}'";
            
            return await _service.GetDataAsync<FabricWastageGrid>(sql);
        }
        public async Task UpdateFreeConceptStatus(string interfaceFrom, int conceptID = 0, string groupConceptNo = "", int bookingID = 0, int isBDS = 0, int ccColorID = 0, int colorID = 0, int itemMasterID = 0, string conceptIDs = "")
        {
            await _service.ExecuteAsync("spUpdateFreeConceptStatus", new { InterfaceFrom = interfaceFrom, ConceptID = conceptID, GroupConceptNo = groupConceptNo, BookingID = bookingID, IsBDS = isBDS, CCColorID = ccColorID, ColorID = colorID, ItemMasterID = itemMasterID, ConceptIDs = conceptIDs }, 30, CommandType.StoredProcedure);
        }
    }
}