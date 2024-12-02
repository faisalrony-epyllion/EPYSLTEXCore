using EPYSLTEX.Core.DTOs;
using EPYSLTEXCore.Application.DTO;
using EPYSLTEXCore.Infrastructure.Entities;

namespace EPYSLTEX.Core.Interfaces.Services
{
    public interface ISelect2Service
    {
         Task<IList<Select2OptionModel>> GetContactNamesAsync(int contactCategoryId);

        Task<IList<Select2OptionModel>> GetContactNamesByCintactIdsAsync(int contactCategoryId, string ids);

        Task<IList<Select2OptionModel>> GetContactNamesAsync(string contactCategory);

        Task<IList<Select2OptionModel>> GetBuyerNameCusAsync(int contactCategoryId);

        Task<IList<Select2OptionModel>> GetYarnSuppliersAsync();

        Task<IList<Select2OptionModel>> GetNonEPZSupplier();

        IList<Select2OptionModel> GetYarnSupplierNames_YarnPO();

        IList<Select2OptionModel> GetEntityTypesAsync();

        Task<IList<Select2OptionModel>> GetEntityTypes(int entityTypeId);

        Task<IList<Select2OptionModel>> GetEntityTypesAsync(int entityTypeId);

        Task<IList<Select2OptionModel>> getMachineByUnitAsync(int unitId, int procesId);

        Task<IList<Select2OptionModel>> GetEntityTypesAsync(string entityTypeName);

        Task<IList<Select2OptionModel>> GetItemSegmentsAsync(string itemSegmentName);

        Task<IList<Select2OptionModel>> GetYarnProgramCustomAsync(int entityTypeId);

        Task<IList<Select2OptionModel>> GetIncoTermsAsync();

        IList<Select2OptionModel> GetFabricTechnicalName();

        Task<IList<Select2OptionModel>> GetFabricDyeingTypeAsync();

        Task<IList<Select2OptionModel>> GetFabricFinishingTypeAsync();

        Task<IList<Select2OptionModel>> GetKnittingTypeAsync();

        Task<IList<Select2OptionModel>> GetIncoTermsSupplierWiseAsync(int id);

        Task<IList<Select2OptionModel>> GetConstructionWiseYarnCountListsAsync(int yarnType, int constructionId, int fabricGsm);

        Task<IList<Select2OptionModel>> GetConstructionAndYarnTypeWiseYarnCountListsAsync(int yarnType);

        Task<IList<Select2OptionModel>> GetPaymentTermsAsync();

        Task<IList<Select2OptionModel>> GetExportOrdersAsync();

        Task<IList<Select2OptionModel>> GetPaymentTermsSupplierWiseAsync(int id);

        Task<IList<Select2OptionModel>> GetSupplierWiseCountryNameAsync(int id);

        IList<Select2OptionModel> GetPIFor();

        Task<IList<Select2OptionModel>> GetYarnTypeAsync(int SegmentNameId);

        Task<IList<Select2OptionModel>> GetFabricCollarCuffAsync();

        Task<IList<Select2OptionModel>> GetYarnCompositionAsync(int SegmentNameId);

        Task<IList<Select2OptionModel>> GetYarnColorAsync(int SegmentNameId);

        Task<IList<Select2OptionModel>> GetYarnColorYarnPOAsync(int SegmentNameId);

        Task<IList<Select2OptionModel>> GetShadeAsync(int SegmentNameId);

        Task<IList<Select2OptionModel>> GetYarnCountAsync(int SegmentNameId);

        Task<IList<Select2OptionModel>> GetCurrencyTypeAsync();

        Task<IList<Select2OptionModel>> GetCompanyAsync();

        Task<IList<Select2OptionModel>> GetBankBranchAsync();

        Task<IList<Select2OptionModel>> GetCompanyWiseBankBranchAsync(int CompanyId);

        Task<IList<Select2OptionModel>> GetCountryOfOriginAsync();

        Task<IList<Select2OptionModel>> GetTypeOfLCAsync();

        Task<IList<Select2OptionModel>> GetUnitAsync();

        Task<IList<Select2OptionModel>> GetUnitServiceWOAsync();

        Task<IList<Select2OptionModel>> GetUnitsAsync(int unitSetId);

        Task<IList<Select2OptionModel>> GetExportOrderListAsync(int buyerId, int buyerTeamId);

        Task<IList<Select2OptionModel>> GetBuyerTeamFromBuyerAsync(int buyerId);

        Task<IList<Select2OptionModel>> GetBuyerNamefromCompanyAsync(int companyId);

        Task<IList<Select2OptionModel>> GetExportOrdersfromBuyerCompanyAsync(int companyId, int buyerId);

        Task<IList<Select2OptionModel>> GetLCIssuingBankAsync();

        Task<IList<Select2OptionModel>> GetLienBankAsync();

        Task<IList<Select2OptionModel>> GetPaymentBankAsync();

        Task<IList<Select2OptionModel>> GetYarnCountElastaneAsync(int constructionId, int fabricGsm);

        Task<IList<Select2OptionModel>> GetCommericalAttachmentDocsAsync();

        Task<IList<Select2OptionModel>> GetLocationAsync();

        IList<Select2OptionModel> GetYarnAdditionalValueSetup();

        IList<Select2OptionModel> GetYarnYDStatusLists();

        IList<Select2OptionModel> GetYarnDeductionValueSetup();

        IList<Select2OptionModel> GetDyeingMCNameSetup();

        IList<Select2OptionModel> GetDyeingMCStatusSetup();

        IList<Select2OptionModel> GetDyeingMCBrandSetup();

        IList<Select2OptionModel> GetknittingMachineNo();

        IList<Select2OptionModel> GetYarnProposedFrom();

        IList<Select2OptionModel> GetSampleBookingReviseInfo();

        Task<IList<Select2OptionModel>> GetSampleBookingSPTypeAsync();

        IList<Select2OptionModel> GetSampleBookingYarnStatus();

        Task<IList<Select2OptionModel>> GetBuyerTeamAsync();

        Task<IList<Select2OptionModel>> GetFabricConstructionAsync();

        Task<IList<Select2OptionModel>> GetFabricCompositionAsync();

        Task<IList<Select2OptionModel>> GetFabricColorAsync();

        Task<IList<Select2OptionModel>> GetFabricGSMAsync();

        Task<IList<Select2OptionModel>> GetFabricWidthAsync();

        Task<IList<Select2OptionModel>> GetReportBuyersAsync(int employeeCode);

        Task<IList<Select2OptionModel>> GetYarnCountByYarnTypeAsync(string yarnTypeIds);

        Task<IList<Select2OptionModel>> GetPortOfDischargeAsync(int contactId);

        Task<IList<Select2OptionModel>> GetPortOfDischarge(int companyId);

        Task<IList<Select2OptionModel>> GetInhouseSuppliersAsync(string subGroupName);

        Task<IList<Select2OptionModel>> GetContactsByCategoryWithDefaultContactAsync(string contactCategory, int defaultContactId);

        Task<IList<Select2OptionModel>> GetProcessChildSubProcessAsync(int processId);

        Task<IList<Select2OptionModel>> GetRackListAsync(int locationId);

        Task<IList<Select2OptionModel>> GetBinListAsync();

        Task<IList<Select2OptionModel>> GetEmployeeList(int locationId);

        Task<IList<Select2OptionModel>> GetBinIdByRackNoAsync(int rackId);

        Task<IList<Select2OptionModel>> GetRackNoByLocationAsync(int locationId, string rackFor = "");

        Task<IList<Select2OptionModel>> GetBookingByBuyer(int buyerId);

        Task<IList<Select2OptionModel>> GetKnittingUnit();

        Task<IList<Select2OptionModel>> GetContactsByContactCategoryAsync(string contactCategory);

        Task<IList<Select2OptionModel>> GetExportLCForTextileAsync();

        Task<IList<Select2OptionModel>> GetYarnCountByYarnTypeAsync(int supplierId, int yarnTypeId);

        Task<IList<Select2OptionModel>> GetDyesChemicalsAsync();

        Task<IList<Select2OptionModel>> GetCountryNameAsync();

        Task<IList<Select2OptionModel>> GetCDAItemAsync(int SegmentNameId);

        Task<IList<Select2OptionModel>> GetCDAAgentAsync(int SegmentNameId);

        Task<IList<Select2OptionModel>> GetCDASuppliersAsync(int SubGroupId);

        Task<IList<Select2OptionModel>> GetRawCDAItemByTypeAsync(int particularId);
        Task<IList<Select2OptionModel>> GetRawCDAItemByTypeAsyncFinder(int particularId, EPYSLTEXCore.Infrastructure.Entities.PaginationInfo paginationInfo);
        Task<IList<Select2OptionModel>> GetRawUnitByType(int rackId);

        Task<IList<Select2OptionModel>> GetAdditionalBookingAsync();

        Task<IList<Select2OptionModel>> GetFabricCompositionOperatorAsync();

        Task<IList<Select2OptionModel>> GetTextileProcessesAsync();

        Task<IList<Select2OptionModel>> GetUserDetailsCodeAsync();

        Task<IList<Select2OptionModel>> GetFiberTypes();

        Task<IList<Select2OptionModel>> GetBlendTypes(int fiberTypeId);

        Task<IList<Select2OptionModel>> GetYarnTypes(int blendTypeId);

        Task<IList<Select2OptionModel>> GetYarnCountByYarnItemAsync(int fiberTypeId, int blendTypeId, int yarnTypeId, int prograrmId, int subProgramId
            , int certificationsId, int technicalParamterId, int compositionId, int manufacturingLineId, int manufacturingProcessId
            , int manufacturingSubprocessId, int shadeId, int colorId, int colorGradeId);

        Task<IList<Select2OptionModel>> GetCountFromProcessSetup(int processSetupId);

        Task<IList<Select2OptionModel>> GetCountFromProductSetupByChild(int setupChildID);

        Task<IList<Select2OptionModel>> GetSubClassByMachineType(int machineTypeId);

        Task<IList<Select2OptionModel>> GetMachineBySubClass(int subClassId);

        Task<IList<Select2OptionModel>> GetMachineGaugeBySubClass(int subClassId);

        Task<IList<Select2OptionModel>> GetMachineDiaBySubClassGuage(int subClassId);

        Task<IList<Select2OptionModel>> GetBrandByGaugeAndSubclass(int subClassId, int guage, int knittingTypeId);

        Task<IList<Select2OptionModel>> GetBrandByGaugeDiaAndSubclass(int subClassId, int dia, int contactId);
        Task<IList<Select2OptionModel>> GetBrandByGaugeDiaAndSubclassFinder(int subClassId, int dia, int contactId, PaginationInfo paginationInfo);

        Task<IList<Select2OptionModel>> GetSubContract(int subClassId, int guage, int brandId, int knittingTypeId);
        Task<IList<Select2OptionModel>> GetSubContractFinder(int subClassId, int guage, int brandId, int knittingTypeId, PaginationInfo paginationInfo);
        Task<IList<Select2OptionModel>> GetSubContractByGgDia(int subClassId, int dia);

        Task<IList<Select2OptionModel>> GetMachineDia(int subClassId, int guage, int brandId, int contactId, int knittingTypeId);

        Task<IList<Select2OptionModel>> GetMachineByContactKnittingTypeGaugeDiaAsync(int MCSubClassID, int BrandID, int ContactID, int MachineKnittingTypeID, int MachineDia);
        Task<IList<Select2OptionModel>> GetMachineByContactKnittingTypeGaugeDiaAsyncFinder(int MCSubClassID, int BrandID, int ContactID, int MachineKnittingTypeID, int MachineDia, PaginationInfo paginationInfo);
        Task<IList<Select2OptionModel>> GetMachineByContactGaugeDia(int MCSubClassID, int MachineGauge, int BrandID, int ContactID, int MachineDia);
        Task<IList<Select2OptionModel>> GetCompanyBankByCompanyID(int companyid);

        Task<IList<Select2OptionModel>> GetColorListAsync(string filterQuery);

        Task<IList<Select2OptionModel>> GetShadeListAsync(string filterQuery);
        Task<IList<YarnProductSetup>> GetAllFiberType();
        
    }
}