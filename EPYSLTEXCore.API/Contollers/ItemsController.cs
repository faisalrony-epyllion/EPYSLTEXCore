using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.API.Extends.Filters;
using EPYSLTEXCore.Application.Interfaces;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.General;
using EPYSLTEXCore.Infrastructure.Entities.Gmt.General.Item;
using EPYSLTEXCore.Infrastructure.Entities.Tex.General.Yarn;
using EPYSLTEXCore.Infrastructure.Static;
using ExcelDataReader;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace EPYSLTEX.Web.Controllers.Apis
{
    [Authorize]
    [Route("api/items")]
    public class ItemsController : ApiBaseController
    {
        private readonly IItemMasterService<ItemMasterUploadBindingModel> _itemMasterService;     
        
        private readonly IItemSetupService _itemSetupService;
        private readonly IMemoryCache _memoryCache;
        public ItemsController(IItemMasterService<ItemMasterUploadBindingModel> itemMasterService, IMemoryCache memoryCache, IUserService userService
            , IItemSetupService itemSetupService) : base(userService)
        {
            _itemMasterService = itemMasterService;          
            _itemSetupService = itemSetupService;
            _memoryCache = memoryCache;
        }

        [HttpGet]
        [Route("table-structure/{subGroupId}")]
        public async Task<IActionResult> GetTableStructure(int subGroupId)
        {
            return Ok( _itemMasterService.GetDataAsync<ItemStructureDTO>(CommonQueries.GetItemStructureBySubGroupId(subGroupId), DB_TYPE.textile));
        }

        [Route("preview")]
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> Preview()
        {
            if (!Request.HasFormContentType)
            {
                return BadRequest("Unsupported media type.");
            }

            var provider = await Request.ReadFormAsync();
            if (!provider.Files.Any()) return BadRequest("You must upload data.");

            if (!int.TryParse(provider["SubGroupID"], out int subGroupId) || subGroupId <= 0)
            {
                return BadRequest("Sub Group is required. Please select one.");
            }

            var records = new List<ItemMasterUploadBindingModel>();
            var uploadedFile = provider.Files[0];



            using (var inputStream = uploadedFile.OpenReadStream())
            {
                using (var reader = ExcelReaderFactory.CreateReader(inputStream))
                {
                    var result = reader.AsDataSet();
                    records = result.Tables[0].ConvertToList<ItemMasterUploadBindingModel>();
                }
            }

 

            records.ForEach(x => x.SubGroupId = subGroupId);

            return Ok(records);
        }

        [Route("upload")]
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> Save(ItemMasterListUploadBindingModel model)
        {
            var itemSegments = _itemMasterService.GetDataAsync<ItemStructureDTO>(CommonQueries.GetItemStructureBySubGroupId(model.SubGroupID), DB_TYPE.textile).ToList();
            var itemSegmentValues = _itemMasterService.GetDataAsync<Select2OptionModel>(CommonQueries.GetItemSegmentValuesBySubGroupId(model.SubGroupID), DB_TYPE.textile).ToList();

            // if select value is null & user wants to create new segment value
            // which is not found this code segment will be executed.
            var newSegmentValues = new List<ItemSegmentValue>();
            foreach (var segment in itemSegments)
            {
                var segmentValues = itemSegmentValues.FindAll(x => x.desc.Equals(segment.SegmentDisplayName, StringComparison.OrdinalIgnoreCase));
                if (segmentValues.Any())
                {
                    model.Items.ForEach(x =>
                    {
                        var property = x.GetType().GetProperty(segment.SegmentValueIdName);
                        var displayValueObj = x.GetType().GetProperty(segment.SegmentValueDescName).GetValue(x);
                        var displayValue = displayValueObj.IsNull() ? string.Empty : displayValueObj.ToString();
                        var selectValue = segmentValues.Find(y => y.text.Equals(displayValue, StringComparison.OrdinalIgnoreCase));

                        if (selectValue.IsNull())
                        {
                            newSegmentValues.Add(
                                new ItemSegmentValue
                                {
                                    SegmentNameID = Convert.ToInt32(segmentValues.First().additionalValue),
                                    SegmentValue = displayValue
                                });

                            segmentValues.Add(
                                new Select2OptionModel
                                {
                                    id = Convert.ToString(segmentValues.Max(y => y.id.ToInt()) + 1),
                                    text = displayValue,
                                    additionalValue = segmentValues.First().additionalValue,
                                    desc = segmentValues.First().desc
                                });
                        }
                        else
                        {
                            property.SetValue(x, Convert.ToInt32(selectValue.id));
                        }
                    });
                }
            }

            // Add new segments to db
            await _itemMasterService.AddSegmentsAsync(newSegmentValues, TableNames.ITEM_SEGMENT_VALUE);

            itemSegmentValues = _itemMasterService.GetDataAsync<Select2OptionModel>(CommonQueries.GetItemSegmentValuesBySubGroupId(model.SubGroupID), DB_TYPE.textile).ToList();

            // Set Item Segment Value ID
            foreach (var segment in itemSegments)
            {
                var segmentValues = itemSegmentValues.FindAll(x => x.desc.Equals(segment.SegmentDisplayName, StringComparison.OrdinalIgnoreCase));
                if (segmentValues.Any())
                {
                    model.Items.ForEach(x =>
                    {
                        var property = x.GetType().GetProperty(segment.SegmentValueIdName);
                        var displayValueObj = x.GetType().GetProperty(segment.SegmentValueDescName).GetValue(x);
                        var displayValue = displayValueObj.IsNull() ? string.Empty : displayValueObj.ToString();
                        var selectValue = segmentValues.Find(y => y.text.Equals(displayValue, StringComparison.OrdinalIgnoreCase));

                        property.SetValue(x, Convert.ToInt32(selectValue.id));
                    });
                }
            }

            // Generate Items
            var items = model.Items;
            _itemMasterService.GenerateItem(model.SubGroupID, ref items);

            return Ok();
        }

        #region Yarn Item Selection

        [HttpGet]
        [Route("yarn/product-setup/{fiberTypeId}")]
        public async Task<IActionResult> GetProductSetup(int fiberTypeId)
        {
            var records = await _itemSetupService.GetProductSetupAsync(fiberTypeId);
            return Ok(records);
        }

        [HttpGet]
        [Route("yarn/process-setup/{fiberTypeId}")]
        public async Task<IActionResult> GetProcessSetup(int fiberTypeId)
        {
            var records = await _itemSetupService.GetProductSetupAsync(fiberTypeId);
            return Ok(records);
        }
        
        [HttpGet]
        [Route("yarn/item-segments/{countIds}/{compositionIds}")]
        public async Task<IActionResult> YarnItemSegments(string countIds, string compositionIds)
        {
            countIds = CommonFunction.ReplaceInvalidChar(countIds);
            compositionIds = CommonFunction.ReplaceInvalidChar(compositionIds);

            string cacheKey = CacheKeys.Yarn_Item_Segments;
            ItemSegmentMappingValuesDTO itemSegmenValues =null ;
            _memoryCache.TryGetValue(cacheKey, out itemSegmenValues);


            if (itemSegmenValues is null)
            {
                var itemSegmentValueList = _itemMasterService.GetDataAsync<Select2MappingOptionModel>(CommonQueries.GetItemSegmentValuesBySegmentNamesWithMapping(), DB_TYPE.textile).ToList();

                itemSegmenValues = new ItemSegmentMappingValuesDTO
                {
                    Segment1ValueList = itemSegmentValueList.FindAll(x => x.desc == ItemSegmentNameConstants.YARN_COMPOSITION),
                    Segment2ValueList = itemSegmentValueList.FindAll(x => x.desc == ItemSegmentNameConstants.YARN_MANUFACTURING_LINE),
                    Segment3ValueList = itemSegmentValueList.FindAll(x => x.desc == ItemSegmentNameConstants.YARN_MANUFACTURING_PROCESS),
                    Segment4ValueList = itemSegmentValueList.FindAll(x => x.desc == ItemSegmentNameConstants.YARN_MANUFACTURING_SUB_PROCESS),
                    Segment5ValueList = itemSegmentValueList.FindAll(x => x.desc == ItemSegmentNameConstants.YARN_QUALITY_PARAMETER),
                    Segment6ValueList = itemSegmentValueList.FindAll(x => x.desc == ItemSegmentNameConstants.YARN_COLOR),
                    Segment7ValueList = itemSegmentValueList.FindAll(x => x.desc == ItemSegmentNameConstants.YARN_COLOR_GRADE),
                    Segment8ValueList = itemSegmentValueList.FindAll(x => x.desc == ItemSegmentNameConstants.YARN_COUNT),
                    ShadeReferenceList = itemSegmentValueList.FindAll(x => x.desc == ItemSegmentNameConstants.SHADE)
                    //YarnCountMaster = itemSegmentValueList.FindAll(x => x.desc == ItemSegmentNameConstants.YARN_COUNT_MASTER)

                };
         
                  
                _memoryCache.Set(cacheKey, CommonFunction.DeepClone(itemSegmenValues), TimeSpan.FromDays(1)); 



            }

            ItemSegmentMappingValuesDTO itemSegmenValuesFinal = CommonFunction.DeepClone(itemSegmenValues);
            if (!compositionIds.IsNullOrEmpty())
            {
                List<int> compositionList = compositionIds.Split(',')
                                      .Select(int.Parse)
                                      .ToList();
                if (compositionList.Count > 0)
                {
                    itemSegmenValuesFinal.Segment1ValueList = itemSegmenValuesFinal.Segment1ValueList.Where(n => compositionList.Contains(Convert.ToInt32(n.id)) || n.IsInactive == false).ToList();
                }
                else
                {
                    itemSegmenValuesFinal.Segment1ValueList = itemSegmenValuesFinal.Segment1ValueList.Where(n => n.IsInactive == false).ToList();
                }
            }
            else
            {
                itemSegmenValuesFinal.Segment1ValueList = itemSegmenValuesFinal.Segment1ValueList.Where(n => n.IsInactive == false).ToList();
            }
            if (!countIds.IsNullOrEmpty()) {
                List<int> countList = countIds.Split(',')
                                      .Select(int.Parse)
                                      .ToList();
                if (countList.Count > 0)
                {
                    itemSegmenValuesFinal.Segment8ValueList = itemSegmenValuesFinal.Segment8ValueList.Where(n => countList.Contains(Convert.ToInt32(n.id)) || n.IsInactive == false).ToList();
                }
                else
                {
                    itemSegmenValuesFinal.Segment8ValueList = itemSegmenValuesFinal.Segment8ValueList.Where(n => n.IsInactive == false).ToList();
                }
            }
            else
            {
                itemSegmenValuesFinal.Segment8ValueList = itemSegmenValuesFinal.Segment8ValueList.Where(n => n.IsInactive == false).ToList();
            }

            return Ok(itemSegmenValuesFinal);
        }
        [HttpPost]
        [Route("refreshOutputCache/{ParameterValue}")]
        public async Task<IActionResult> RefreshOutputCache(string ParameterValue)
        {
            if (ParameterValue == CacheKeys.Yarn_Item_Segments)
            {
                string cacheKey = CacheKeys.Yarn_Item_Segments;
              
                _memoryCache.Remove(cacheKey);
                var itemSegmentValueList = _itemMasterService.GetDataAsync<Select2MappingOptionModel>(CommonQueries.GetItemSegmentValuesBySegmentNamesWithMapping(), DB_TYPE.textile).ToList();

                var itemSegmenValues = new ItemSegmentMappingValuesDTO
                {
                    Segment1ValueList = itemSegmentValueList.FindAll(x => x.desc == ItemSegmentNameConstants.YARN_COMPOSITION),
                    Segment2ValueList = itemSegmentValueList.FindAll(x => x.desc == ItemSegmentNameConstants.YARN_MANUFACTURING_LINE),
                    Segment3ValueList = itemSegmentValueList.FindAll(x => x.desc == ItemSegmentNameConstants.YARN_MANUFACTURING_PROCESS),
                    Segment4ValueList = itemSegmentValueList.FindAll(x => x.desc == ItemSegmentNameConstants.YARN_MANUFACTURING_SUB_PROCESS),
                    Segment5ValueList = itemSegmentValueList.FindAll(x => x.desc == ItemSegmentNameConstants.YARN_QUALITY_PARAMETER),
                    Segment6ValueList = itemSegmentValueList.FindAll(x => x.desc == ItemSegmentNameConstants.YARN_COLOR),
                    Segment7ValueList = itemSegmentValueList.FindAll(x => x.desc == ItemSegmentNameConstants.YARN_COLOR_GRADE),
                    Segment8ValueList = itemSegmentValueList.FindAll(x => x.desc == ItemSegmentNameConstants.YARN_COUNT),
                    ShadeReferenceList = itemSegmentValueList.FindAll(x => x.desc == ItemSegmentNameConstants.SHADE)
                    //YarnCountMaster = itemSegmentValueList.FindAll(x => x.desc == ItemSegmentNameConstants.YARN_COUNT_MASTER)

                };
            
       
                _memoryCache.Set(cacheKey, CommonFunction.DeepClone(itemSegmenValues), TimeSpan.FromDays(1));
            }
            return Ok();
        }
        [HttpGet]
        [Route("yarn/item-segments-mapping")]
        public async Task<IActionResult> GetSegmentValueYarnTypeMappingSetup()
        {
            var itemSegmentValueList = _itemMasterService.GetDataAsync<SegmentValueYarnTypeMappingSetup>(CommonQueries.GetSegmentValueYarnTypeMappingSetup(), DB_TYPE.textile).ToList();
            return Ok(itemSegmentValueList);
        }
        //Saif 30/08/2023 END
        [HttpGet]
        [Route("yarn/item-segments-admin")]
        public async Task<IActionResult> YarnItemSegmentsAdmin()
        {
            var segmentNames = new
            {
                SegmentNames = new string[]
                {
                    ItemSegmentNameConstants.YARN_COMPOSITION,
                    ItemSegmentNameConstants.YARN_MANUFACTURING_LINE,
                    ItemSegmentNameConstants.YARN_MANUFACTURING_PROCESS,
                    ItemSegmentNameConstants.YARN_MANUFACTURING_SUB_PROCESS,
                    ItemSegmentNameConstants.YARN_QUALITY_PARAMETER,
                    ItemSegmentNameConstants.YARN_COUNT//,
                   
                }
            };

            var itemSegmentValueList = _itemMasterService.GetDataAsync<Select2MappingOptionModel>(CommonQueries.GetItemSegmentValuesBySegmentNamesWithMappingAdmin(), DB_TYPE.textile).ToList();
            var SegmentFilterMappingList = _itemMasterService.GetDataAsync<Select2MappingOptionModel>(CommonQueries.GetItemSegmentValuesFilterMapping(), DB_TYPE.textile).ToList();
            var itemSegmenValues = new ItemSegmentMappingValuesDTO
            {
                Segment1ValueList = itemSegmentValueList.FindAll(x => x.desc == ItemSegmentNameConstants.YARN_COMPOSITION),
                Segment2ValueList = itemSegmentValueList.FindAll(x => x.desc == ItemSegmentNameConstants.YARN_MANUFACTURING_LINE),
                Segment3ValueList = itemSegmentValueList.FindAll(x => x.desc == ItemSegmentNameConstants.YARN_MANUFACTURING_PROCESS),
                Segment4ValueList = itemSegmentValueList.FindAll(x => x.desc == ItemSegmentNameConstants.YARN_MANUFACTURING_SUB_PROCESS),
                Segment5ValueList = itemSegmentValueList.FindAll(x => x.desc == ItemSegmentNameConstants.YARN_QUALITY_PARAMETER),
                Segment6ValueList = itemSegmentValueList.FindAll(x => x.desc == ItemSegmentNameConstants.YARN_COUNT),
                YarnCountMaster = itemSegmentValueList.FindAll(x => x.desc == ItemSegmentNameConstants.YARN_COUNT_MASTER),
                SegmentFilterMappingList = SegmentFilterMappingList
            };

            return Ok(itemSegmenValues);
        }
        #endregion Yarn Item Selection
        [HttpGet]
        [Route("yarn/fiber-subprogram-certifications")]
        public async Task<IActionResult> GetFiberSubProgramCertifications()
        {

            var itemSegmentValueList = _itemMasterService.GetDataAsync<Select2MappingOptionModel>(CommonQueries.GetFiberSubProgramCertifications(), DB_TYPE.textile).ToList();
            var itemSegmenValues = new ItemSegmentMappingValuesDTO
            {
                FiberList = itemSegmentValueList.FindAll(x => x.desc == ItemSegmentNameConstants.FIBER_TYPE),
                SubProgramList = itemSegmentValueList.FindAll(x => x.desc == ItemSegmentNameConstants.YARN_SUBPROGRAM_NEW),
                CertificationsList = itemSegmentValueList.FindAll(x => x.desc == ItemSegmentNameConstants.YARN_CERTIFICATIONS),

                
            };

            return Ok(itemSegmenValues);
        }
        #region Dyes & Chemical

        [Route("get-cache-reset-setup")]
        public async Task<IActionResult> GetList()
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<CacheResetSetup> records = await _itemSetupService.GetCacheResetSetupsAsync(paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }
        [HttpGet]
        [Route("dyes/item-segments")]
        public async Task<IActionResult> DyesItemSegments()
        {
            //var segmentNames = new
            //{
            //    SegmentNames = new string[]
            //    {
            //        ItemSegmentNameConstants.DYES_GROUP,
            //        ItemSegmentNameConstants.DYES_ITEM_NAME
            //    }
            //};

            //var itemSegmentValueList = _itemMasterService.GetDataAsync<Select2OptionModel>(CommonQueries.GetItemSegmentValuesBySegmentNamesWithSegmentName(), segmentNames).ToList();

            //var itemSegmenValues = new ItemSegmenValuesDTO
            //{
            //    Segment1ValueList = itemSegmentValueList.FindAll(x => x.desc == ItemSegmentNameConstants.DYES_GROUP && x.text.IsNotNullOrEmpty()),
            //    Segment2ValueList = itemSegmentValueList.FindAll(x => x.desc == ItemSegmentNameConstants.DYES_ITEM_NAME && x.text.IsNotNullOrEmpty())
            //};

            //return Ok(itemSegmenValues);
            return Ok("itemSegmenValues");
        }

        [HttpGet]
        [Route("chemicals/item-segments")]
        public async Task<IActionResult> ChemicalsItemSegments()
        {
            //var segmentNames = new
            //{
            //    SegmentNames = new string[]
            //    {
            //        ItemSegmentNameConstants.CHEMICALS_GROUP,
            //        ItemSegmentNameConstants.CHEMICALS_AGENT,
            //        ItemSegmentNameConstants.CHEMICALS_FORM,
            //        ItemSegmentNameConstants.CHEMICALS_ITEM_NAME
            //    }
            //};

            //var itemSegmentValueList = _itemMasterService.GetDataAsync<Select2OptionModel>(CommonQueries.GetItemSegmentValuesBySegmentNamesWithSegmentName(), segmentNames).ToList();

            //var itemSegmenValues = new ItemSegmenValuesDTO
            //{
            //    Segment1ValueList = itemSegmentValueList.FindAll(x => x.desc == ItemSegmentNameConstants.CHEMICALS_GROUP && x.text.IsNotNullOrEmpty()),
            //    Segment2ValueList = itemSegmentValueList.FindAll(x => x.desc == ItemSegmentNameConstants.CHEMICALS_AGENT && x.text.IsNotNullOrEmpty()),
            //    Segment3ValueList = itemSegmentValueList.FindAll(x => x.desc == ItemSegmentNameConstants.CHEMICALS_FORM && x.text.IsNotNullOrEmpty()),
            //    Segment4ValueList = itemSegmentValueList.FindAll(x => x.desc == ItemSegmentNameConstants.CHEMICALS_ITEM_NAME && x.text.IsNotNullOrEmpty())
            //};

            return Ok("itemSegmenValues");
            //return Ok(itemSegmenValues);
        }

        #endregion Dyes & Chemical

        [HttpGet]
        [Route("item-structure/{subGroupName}")]
        public async Task<IActionResult> GetItemStructure(string subGroupName)
        {
            return Ok(await _itemSetupService.GetItemStructureBySubGroup(subGroupName));
        }

        [HttpGet]
        [Route("item-structure-for-display/{subGroupName}")]
        public async Task<IActionResult> GetItemStructureForDisplay(string subGroupName)
        {
            return Ok(await _itemSetupService.GetItemStructureForDisplayBySubGroup(subGroupName));
        }

        [HttpGet]
        [Route("yarn/all-item-segments-list")]
        public async Task<IActionResult> AllYarnItemSegmentsList()
        {
            ItemSegmentMaster obj = new ItemSegmentMaster();
            obj.Compositions = _itemMasterService.GetDataAsync<ItemSegmentChild>(CommonQueries.GetYarnCompositions(), DB_TYPE.textile).ToList();
      
            obj.Counts = _itemMasterService.GetDataAsync<ItemSegmentChild>(CommonQueries.GetYarnCounts(), DB_TYPE.textile).ToList();
            return Ok(obj);
        }

        [HttpGet]
        [Route("yarn/all-composition-segments-list")]
        public async Task<IActionResult> AllYarnCompositionSegmentsList()
        {
            ItemSegmentMaster obj = new ItemSegmentMaster();
            obj.Fibers = _itemMasterService.GetDataAsync<ItemSegmentChild>(CommonQueries.GetAllFibers(), DB_TYPE.textile).ToList();
            obj.SubPrograms = _itemMasterService.GetDataAsync<ItemSegmentChild>(CommonQueries.GetAllSubPrograms(), DB_TYPE.textile).ToList();
            obj.Certifications = _itemMasterService.GetDataAsync<ItemSegmentChild>(CommonQueries.GetAllCertifications(), DB_TYPE.textile).ToList();
            return Ok(obj);
        }
        [Route("save-segment-active-inactive")]
        [HttpPost]
        public async Task<IActionResult> SaveSegmentActiveInactive(ItemSegmentMaster model)
        {
            await _itemSetupService.SaveAsync(model, AppUser.UserCode);
            return Ok();
        }
        [Route("save-composition-segment-active-inactive")]
        [HttpPost]
        public async Task<IActionResult> SaveCompositionSegmentActiveInactive(ItemSegmentMaster model)
        {
            await _itemSetupService.SaveAsync(model, AppUser.UserCode);
            return Ok();
        }
    }
}