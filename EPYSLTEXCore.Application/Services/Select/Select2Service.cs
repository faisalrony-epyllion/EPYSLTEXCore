using EPYSLTEX.Core.DTOs;
using EPYSLTEX.Core.Interfaces.Repositories;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEXCore.Application.DTO;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Static;
namespace EPYSLTEXCore.Application.Services.Select
{
    public class Select2Service:ISelect2Service
    {
        private readonly ISqlQueryRepository<Select2OptionModel> _sqlQueryRepository;
        private readonly IGmtSqlQueryRepository<Select2OptionModel> _gmtSqlQueryRepository;

        private readonly IDapperCRUDService<LoginUser> _gmtService;

        public   Select2Service(       
        //ISqlQueryRepository<Select2OptionModel> sqlQueryRepository
          IDapperCRUDService<LoginUser> gmtService)
        {

        //    _sqlQueryRepository = sqlQueryRepository;
            _gmtService = gmtService;      
            _gmtService.Connection = _gmtService.GetConnection(AppConstants.GMT_CONNECTION);

        }

        public async Task<IList<Select2OptionModel>> GetKnittingUnit()
        {
            //var query = $@"
            //    select cast(KnittingUnitID as varchar) id,UnitName [text] from KnittingUnit WHERE IsKnitting = 0 order by UnitName";
            //return await _sqlQueryRepository.GetDataDapperAsync<Select2OptionModel>(query);
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetAdditionalBookingAsync()
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetBankBranchAsync()
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetBinIdByRackNoAsync(int rackId)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetBinListAsync()
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetBlendTypes(int fiberTypeId)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetBookingByBuyer(int buyerId)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetBrandByGaugeAndSubclass(int subClassId, int guage, int knittingTypeId)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetBrandByGaugeDiaAndSubclass(int subClassId, int dia, int contactId)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetBrandByGaugeDiaAndSubclassFinder(int subClassId, int dia, int contactId, PaginationInfo paginationInfo)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetBuyerNameCusAsync(int contactCategoryId)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetBuyerNamefromCompanyAsync(int companyId)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetBuyerTeamAsync()
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetBuyerTeamFromBuyerAsync(int buyerId)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetCDAAgentAsync(int SegmentNameId)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetCDAItemAsync(int SegmentNameId)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetCDASuppliersAsync(int SubGroupId)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetColorListAsync(string filterQuery)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetCommericalAttachmentDocsAsync()
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetCompanyAsync()
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetCompanyBankByCompanyID(int companyid)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetCompanyWiseBankBranchAsync(int CompanyId)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetConstructionAndYarnTypeWiseYarnCountListsAsync(int yarnType)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetConstructionWiseYarnCountListsAsync(int yarnType, int constructionId, int fabricGsm)
        {
            throw new NotImplementedException();
        }

        public async Task<IList<Select2OptionModel>> GetContactNamesAsync(int contactCategoryId)
        {
            var query = $@"SELECT CAST(Contacts.ContactID AS VARCHAR) AS id, Contacts.Name AS text
                FROM Contacts
                INNER JOIN ContactCategoryChild ON Contacts.ContactID = ContactCategoryChild.ContactID
                INNER JOIN ContactCategoryHK ON ContactCategoryChild.ContactCategoryID = ContactCategoryHK.ContactCategoryID
                WHERE ContactCategoryHK.ContactCategoryID = {contactCategoryId}
                ORDER BY Contacts.Name";
            return await _gmtService.GetDataAsync<Select2OptionModel>(query);
        }

        public async Task<IList<Select2OptionModel>> GetContactNamesAsync(string contactCategory)
        {
            var query = $@"
                Select Cast(C.ContactID As varchar) id, C.ShortName [text]
                From Contacts C
                Inner Join ContactCategoryChild CCC On C.ContactID = CCC.ContactID
                Inner Join ContactCategoryHK CCHK On CCC.ContactCategoryID = CCHK.ContactCategoryID
                Where CCHK.ContactCategoryName = '{contactCategory}'
                Order By C.ShortName";
            return await _gmtService.GetDataAsync<Select2OptionModel>(query);
        }
    

        Task<IList<Select2OptionModel>> ISelect2Service.GetContactNamesByCintactIdsAsync(int contactCategoryId, string ids)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetContactsByCategoryWithDefaultContactAsync(string contactCategory, int defaultContactId)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetContactsByContactCategoryAsync(string contactCategory)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetCountFromProcessSetup(int processSetupId)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetCountFromProductSetupByChild(int setupChildID)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetCountryNameAsync()
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetCountryOfOriginAsync()
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetCurrencyTypeAsync()
        {
            throw new NotImplementedException();
        }

        IList<Select2OptionModel> ISelect2Service.GetDyeingMCBrandSetup()
        {
            throw new NotImplementedException();
        }

        IList<Select2OptionModel> ISelect2Service.GetDyeingMCNameSetup()
        {
            throw new NotImplementedException();
        }

        IList<Select2OptionModel> ISelect2Service.GetDyeingMCStatusSetup()
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetDyesChemicalsAsync()
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetEmployeeList(int locationId)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetEntityTypes(int entityTypeId)
        {
            throw new NotImplementedException();
        }

      

        Task<IList<Select2OptionModel>> ISelect2Service.GetEntityTypesAsync(int entityTypeId)
        {
            throw new NotImplementedException();
        }

      
        public async Task<IList<Select2OptionModel>> GetEntityTypesAsync(string entityTypeName)
        {
            var query = $@"{CommonQueries.GetEntityTypesByEntityTypeName(entityTypeName)}";
            return await _gmtService.GetDataAsync<Select2OptionModel>(query);
        }
        public async Task<IList<YarnProductSetup>> GetAllFiberType()
        {
            var query = $@"{CommonQueries.GetAllFiberType()}";
            return await _gmtService.GetDataAsync<YarnProductSetup>(query);
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetExportLCForTextileAsync()
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetExportOrderListAsync(int buyerId, int buyerTeamId)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetExportOrdersAsync()
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetExportOrdersfromBuyerCompanyAsync(int companyId, int buyerId)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetFabricCollarCuffAsync()
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetFabricColorAsync()
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetFabricCompositionAsync()
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetFabricCompositionOperatorAsync()
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetFabricConstructionAsync()
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetFabricDyeingTypeAsync()
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetFabricFinishingTypeAsync()
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetFabricGSMAsync()
        {
            throw new NotImplementedException();
        }

        IList<Select2OptionModel> ISelect2Service.GetFabricTechnicalName()
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetFabricWidthAsync()
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetFiberTypes()
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetIncoTermsAsync()
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetIncoTermsSupplierWiseAsync(int id)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetInhouseSuppliersAsync(string subGroupName)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetItemSegmentsAsync(string itemSegmentName)
        {
            throw new NotImplementedException();
        }

        IList<Select2OptionModel> ISelect2Service.GetknittingMachineNo()
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetKnittingTypeAsync()
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetKnittingUnit()
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetLCIssuingBankAsync()
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetLienBankAsync()
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetLocationAsync()
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetMachineByContactGaugeDia(int MCSubClassID, int MachineGauge, int BrandID, int ContactID, int MachineDia)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetMachineByContactKnittingTypeGaugeDiaAsync(int MCSubClassID, int BrandID, int ContactID, int MachineKnittingTypeID, int MachineDia)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetMachineByContactKnittingTypeGaugeDiaAsyncFinder(int MCSubClassID, int BrandID, int ContactID, int MachineKnittingTypeID, int MachineDia, Infrastructure.Entities.PaginationInfo paginationInfo)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetMachineBySubClass(int subClassId)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.getMachineByUnitAsync(int unitId, int procesId)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetMachineDia(int subClassId, int guage, int brandId, int contactId, int knittingTypeId)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetMachineDiaBySubClassGuage(int subClassId)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetMachineGaugeBySubClass(int subClassId)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetNonEPZSupplier()
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetPaymentBankAsync()
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetPaymentTermsAsync()
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetPaymentTermsSupplierWiseAsync(int id)
        {
            throw new NotImplementedException();
        }

        IList<Select2OptionModel> ISelect2Service.GetPIFor()
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetPortOfDischarge(int companyId)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetPortOfDischargeAsync(int contactId)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetProcessChildSubProcessAsync(int processId)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetRackListAsync(int locationId)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetRackNoByLocationAsync(int locationId, string rackFor)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetRawCDAItemByTypeAsync(int particularId)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetRawCDAItemByTypeAsyncFinder(int particularId, Infrastructure.Entities.PaginationInfo paginationInfo)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetRawUnitByType(int rackId)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetReportBuyersAsync(int employeeCode)
        {
            throw new NotImplementedException();
        }

        IList<Select2OptionModel> ISelect2Service.GetSampleBookingReviseInfo()
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetSampleBookingSPTypeAsync()
        {
            throw new NotImplementedException();
        }

        IList<Select2OptionModel> ISelect2Service.GetSampleBookingYarnStatus()
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetShadeAsync(int SegmentNameId)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetShadeListAsync(string filterQuery)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetSubClassByMachineType(int machineTypeId)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetSubContract(int subClassId, int guage, int brandId, int knittingTypeId)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetSubContractByGgDia(int subClassId, int dia)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetSubContractFinder(int subClassId, int guage, int brandId, int knittingTypeId, PaginationInfo paginationInfo)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetSupplierWiseCountryNameAsync(int id)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetTextileProcessesAsync()
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetTypeOfLCAsync()
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetUnitAsync()
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetUnitsAsync(int unitSetId)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetUnitServiceWOAsync()
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetUserDetailsCodeAsync()
        {
            throw new NotImplementedException();
        }

        IList<Select2OptionModel> ISelect2Service.GetYarnAdditionalValueSetup()
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetYarnColorAsync(int SegmentNameId)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetYarnColorYarnPOAsync(int SegmentNameId)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetYarnCompositionAsync(int SegmentNameId)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetYarnCountAsync(int SegmentNameId)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetYarnCountByYarnItemAsync(int fiberTypeId, int blendTypeId, int yarnTypeId, int prograrmId, int subProgramId, int certificationsId, int technicalParamterId, int compositionId, int manufacturingLineId, int manufacturingProcessId, int manufacturingSubprocessId, int shadeId, int colorId, int colorGradeId)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetYarnCountByYarnTypeAsync(string yarnTypeIds)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetYarnCountByYarnTypeAsync(int supplierId, int yarnTypeId)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetYarnCountElastaneAsync(int constructionId, int fabricGsm)
        {
            throw new NotImplementedException();
        }

        IList<Select2OptionModel> ISelect2Service.GetYarnDeductionValueSetup()
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetYarnProgramCustomAsync(int entityTypeId)
        {
            throw new NotImplementedException();
        }

        IList<Select2OptionModel> ISelect2Service.GetYarnProposedFrom()
        {
            throw new NotImplementedException();
        }

        IList<Select2OptionModel> ISelect2Service.GetYarnSupplierNames_YarnPO()
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetYarnSuppliersAsync()
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetYarnTypeAsync(int SegmentNameId)
        {
            throw new NotImplementedException();
        }

        Task<IList<Select2OptionModel>> ISelect2Service.GetYarnTypes(int blendTypeId)
        {
            throw new NotImplementedException();
        }

        IList<Select2OptionModel> ISelect2Service.GetYarnYDStatusLists()
        {
            throw new NotImplementedException();
        }

        IList<Select2OptionModel> ISelect2Service.GetEntityTypesAsync()
        {
            throw new NotImplementedException();
        }
    }
}
