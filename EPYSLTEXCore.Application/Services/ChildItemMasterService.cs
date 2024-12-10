using Dapper;
using EPYSLTEX.Core.Interfaces.Repositories;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Application.Interfaces;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Gmt.General.Item;
using EPYSLTEXCore.Infrastructure.Static;
using System.Data.Entity;
using System.Data.SqlClient;

namespace EPYSLTEXCore.Application.Services
{
    public class ChildItemMasterService<T> : IChildItemMasterService<T> where T : BaseChildItemMaster
    {
        private readonly IDapperCRUDService<DapperBaseEntity> _sqlQueryService;
        private readonly IDapperCRUDService<ItemSegmentValue> _itemSegmentService;
        private readonly SqlConnection _connection;
        private readonly SqlConnection _connectionTex;
        public ChildItemMasterService(IDapperCRUDService<DapperBaseEntity> sqlQueryService, IDapperCRUDService<ItemSegmentValue> itemSegmentService)
        {
            
            _itemSegmentService = itemSegmentService;
            _sqlQueryService = sqlQueryService;
            _connectionTex = _itemSegmentService.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _itemSegmentService.Connection = _itemSegmentService.GetConnection(AppConstants.GMT_CONNECTION);
            _connection = _itemSegmentService.Connection;
            
        }

        public void GenerateItemWithYItem(int subGroupId, ref List<ItemMasterBomTemp> itemMasterList, ref List<T> itemList)
        {
            try
            {
                //var itemMasterList = GetItemMasters(subGroupId);
                var newItemMasterList = new List<ItemMasterBomTemp>();
                var subGroup = GetItemSubGroup(subGroupId);

                SetSegmentValueDesc(subGroupId, ref itemList);

                foreach (var item in itemList) item.ItemMasterID = AddItemInTempItemMaster(ref newItemMasterList, itemMasterList, subGroup, item);

                if (!RegisterItemMaster(ref newItemMasterList, subGroup))
                {
                    throw new Exception("Item registration problem!");
                }
                else
                {
                    foreach (var newItem in newItemMasterList)
                    {
                        itemMasterList.Add(newItem);
                    }
                }


                #region Set ItemMasterID & New SystemID in Yarn Booking Child Table

                foreach (var item in itemList)
                {
                    var entity = newItemMasterList.Find(x =>
                        x.Segment1ValueId == item.Segment1ValueId && x.Segment2ValueId == item.Segment2ValueId
                        && x.Segment3ValueId == item.Segment3ValueId && x.Segment4ValueId == item.Segment4ValueId
                        && x.Segment5ValueId == item.Segment5ValueId && x.Segment6ValueId == item.Segment6ValueId
                        && x.Segment7ValueId == item.Segment7ValueId && x.Segment8ValueId == item.Segment8ValueId
                        && x.Segment9ValueId == item.Segment9ValueId && x.Segment10ValueId == item.Segment10ValueId
                        && x.Segment11ValueId == item.Segment11ValueId && x.Segment12ValueId == item.Segment12ValueId
                        && x.Segment13ValueId == item.Segment13ValueId && x.Segment14ValueId == item.Segment14ValueId
                        && x.Segment15ValueId == item.Segment15ValueId);

                    if (entity != null)
                        item.ItemMasterID = entity.Id;
                }

                #endregion Set ItemMasterID & New SystemID in Yarn Booking Child Table

                return;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<ItemMasterBomTemp> GetItemMasterList(int subGroupId)
        {
            try
            {
                List<ItemMasterBomTemp> itemList = new List<ItemMasterBomTemp>();
                itemList = GetItemMasters(subGroupId);


                return itemList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private ItemSubGroupDTO GetItemSubGroup(int subGroupId)
        {
            var query = $@"Select ISG.SubGroupID Id, ISG.DisplaySubGrupID DisplaySubGrupId, ISG.SubGroupName, ISG.ItemPrefix, ISG.ItemGroupID ItemGroupId
	                , ISG.SeqNo, ISG.IsUsed , IG.GroupName, IG.UnitSetID UnitSetId, IG.DefaultUnitID DefaultUnitId, ISNULL(MaxDisplayID,0) MaxDisplayId
                From ItemSubGroup ISG
                Inner Join ItemGroup IG On IG.ItemGroupID = ISG.ItemGroupID
                Left Join (
	                Select ISG.SubGroupID, Max(Convert(int,RTRIM(LTRIM(REPLACE(IM.DisplayItemID, ISNULL(ISG.ItemPrefix,''),''))))) MaxDisplayID
	                From ItemMaster IM
	                Inner Join ItemSubGroup ISG On ISG.SubGroupID = IM.SubGroupID
                    Where ISG.SubGroupID = {subGroupId}
	                Group By ISG.SubGroupID
                ) M On M.SubGroupID = ISG.SubGroupID
                Where ISG.SubGroupID = {subGroupId}";

            var dto= GetData<ItemSubGroupDTO>(query, DB_TYPE.gmt);
            return dto.FirstOrDefault();
        }
        private List<ItemMasterBomTemp> GetItemMasters(int subGroupId)
        {
            var sql = $@"Select ItemMasterID Id, DisplayItemID DisplayItemId, ItemName, DisplayItemName, ItemGroupID ItemGroupId, SubGroupID SubGroupId
	                        , UnitSetID, DefaultTranUnitID DefaultTranUnitId, DefaultReportUnitID DefaultReportUnitId, Segment1ValueID Segment1ValueId
	                        , Segment2ValueID Segment2ValueId, Segment3ValueID Segment3ValueId, Segment4ValueID Segment4ValueId, Segment5ValueID Segment5ValueId
	                        , Segment6ValueID Segment6ValueId, Segment7ValueID Segment7ValueId, Segment8ValueID Segment8ValueId, Segment9ValueID Segment9ValueId
	                        , Segment10ValueID Segment10ValueId, Segment11ValueID Segment11ValueId, Segment12ValueID Segment12ValueId, Segment13ValueID Segment13ValueId
	                        , Segment14ValueID Segment14ValueId, Segment15ValueID Segment15ValueId
                        From ItemMaster
                        Where SubGroupID = {subGroupId}";

            var dto = GetData<ItemMasterBomTemp>(sql, DB_TYPE.gmt);
            return dto.ToList();

        }
        public IEnumerable<T> GetData<T>(string query, int db_type)
        {
            switch (db_type)
            {
                case 1:
                    try
                    {
                        _connectionTex.Open();
                        return _connectionTex.Query<T>(query);

                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        _connectionTex.Close();
                    }
                case 2:
                    try
                    {
                        _connection.Open();
                        return _connection.Query<T>(query);

                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        _connection.Close();
                    }
                default:
                    throw new NotImplementedException();

            }


        }

        private void SetSegmentValueDesc(int subGroupId, ref List<T> list)
        {
            List<Select2OptionModel> itemSegmentValueList = GetData<Select2OptionModel>(CommonQueries.GetItemSegmentValuesBySubGroupId(subGroupId),DB_TYPE.gmt).ToList();


            foreach (var itemMaster in list)
            {
                if (itemMaster.Segment1ValueDesc.NullOrEmpty() && itemMaster.Segment1ValueId > 0)
                {
                    var segmentValue = itemMaster.Segment1ValueId;
                    itemMaster.Segment1ValueDesc = itemSegmentValueList.Find(x => x.id == segmentValue.ToString())?.text;
                }

                if (itemMaster.Segment2ValueDesc.NullOrEmpty() && itemMaster.Segment2ValueId > 0)
                {
                    var segmentValue = itemMaster.Segment2ValueId;
                    itemMaster.Segment2ValueDesc = itemSegmentValueList.Find(x => x.id == segmentValue.ToString())?.text;
                }

                if (itemMaster.Segment3ValueDesc.NullOrEmpty() && itemMaster.Segment3ValueId > 0)
                {
                    var segmentValue = itemMaster.Segment3ValueId;
                    itemMaster.Segment3ValueDesc = itemSegmentValueList.Find(x => x.id == segmentValue.ToString())?.text;
                }

                if (itemMaster.Segment4ValueDesc.NullOrEmpty() && itemMaster.Segment4ValueId > 0)
                {
                    var segmentValue = itemMaster.Segment4ValueId;
                    itemMaster.Segment4ValueDesc = itemSegmentValueList.Find(x => x.id == segmentValue.ToString())?.text;
                }

                if (itemMaster.Segment5ValueDesc.NullOrEmpty() && itemMaster.Segment5ValueId > 0)
                {
                    var segmentValue = itemMaster.Segment5ValueId;
                    itemMaster.Segment5ValueDesc = itemSegmentValueList.Find(x => x.id == segmentValue.ToString())?.text;
                }

                if (itemMaster.Segment6ValueDesc.NullOrEmpty() && itemMaster.Segment6ValueId > 0)
                {
                    var segmentValue = itemMaster.Segment6ValueId;
                    itemMaster.Segment6ValueDesc = itemSegmentValueList.Find(x => x.id == segmentValue.ToString())?.text;
                }

                if (itemMaster.Segment7ValueDesc.NullOrEmpty() && itemMaster.Segment7ValueId > 0)
                {
                    var segmentValue = itemMaster.Segment7ValueId;
                    itemMaster.Segment7ValueDesc = itemSegmentValueList.Find(x => x.id == segmentValue.ToString())?.text;
                }

                if (itemMaster.Segment8ValueDesc.NullOrEmpty() && itemMaster.Segment8ValueId > 0)
                {
                    var segmentValue = itemMaster.Segment8ValueId;
                    itemMaster.Segment8ValueDesc = itemSegmentValueList.Find(x => x.id == segmentValue.ToString())?.text;
                }

                if (itemMaster.Segment9ValueDesc.NullOrEmpty() && itemMaster.Segment9ValueId > 0)
                {
                    var segmentValue = itemMaster.Segment9ValueId;
                    itemMaster.Segment9ValueDesc = itemSegmentValueList.Find(x => x.id == segmentValue.ToString())?.text;
                }

                if (itemMaster.Segment10ValueDesc.NullOrEmpty() && itemMaster.Segment10ValueId > 0)
                {
                    var segmentValue = itemMaster.Segment10ValueId;
                    itemMaster.Segment10ValueDesc = itemSegmentValueList.Find(x => x.id == segmentValue.ToString())?.text;
                }

                if (itemMaster.Segment11ValueDesc.NullOrEmpty() && itemMaster.Segment11ValueId > 0)
                {
                    var segmentValue = itemMaster.Segment11ValueId;
                    itemMaster.Segment11ValueDesc = itemSegmentValueList.Find(x => x.id == segmentValue.ToString())?.text;
                }

                if (itemMaster.Segment12ValueDesc.NullOrEmpty() && itemMaster.Segment12ValueId > 0)
                {
                    var segmentValue = itemMaster.Segment12ValueId;
                    itemMaster.Segment12ValueDesc = itemSegmentValueList.Find(x => x.id == segmentValue.ToString())?.text;
                }

                if (itemMaster.Segment13ValueDesc.NullOrEmpty() && itemMaster.Segment13ValueId > 0)
                {
                    var segmentValue = itemMaster.Segment13ValueId;
                    itemMaster.Segment13ValueDesc = itemSegmentValueList.Find(x => x.id == segmentValue.ToString())?.text;
                }

                if (itemMaster.Segment14ValueDesc.NullOrEmpty() && itemMaster.Segment14ValueId > 0)
                {
                    var segmentValue = itemMaster.Segment14ValueId;
                    itemMaster.Segment14ValueDesc = itemSegmentValueList.Find(x => x.id == segmentValue.ToString())?.text;
                }

                if (itemMaster.Segment15ValueDesc.NullOrEmpty() && itemMaster.Segment15ValueId > 0)
                {
                    var segmentValue = itemMaster.Segment15ValueId;
                    itemMaster.Segment15ValueDesc = itemSegmentValueList.Find(x => x.id == segmentValue.ToString())?.text;
                }
            }
        }

        private bool RegisterItemMaster(ref List<ItemMasterBomTemp> newItemMasters
    , ItemSubGroupDTO objItemSubGroup)
        {
            try
            {
                #region 1. Clear Item Temp Table

                _sqlQueryService.RunSqlCommand("Delete ItemMasterBOMTemp", false);

                #endregion 1. Clear Item Temp Table

                #region 2. Save All Item In Temp Table

                var maxItemMasterId = _sqlQueryService.GetMaxId(TableNames.ITEMMASTER, newItemMasters.Count);
                foreach (var item in newItemMasters)
                {
                    objItemSubGroup.MaxDisplayID = objItemSubGroup.MaxDisplayID + 1;
                    item.Id = maxItemMasterId++;
                    item.DisplayItemId = objItemSubGroup.ItemPrefix + objItemSubGroup.MaxDisplayID.ToString();

                    //_dbContext.ItemMasterBomTempSet.Add(item);//Ratin
                }

                //_dbContext.SaveChanges();// Ratin

                #endregion 2. Save All Item In Temp Table

                #region 3. Insert Newly Add Item To ItemMaster Table From Temp Table

                var sql = $@"Insert Into ItemMaster
                        Select IMBOM.*
                        From ItemMasterBOMTemp IMBOM";
                _sqlQueryService.RunSqlCommand(sql, false);

                #endregion 3. Insert Newly Add Item To ItemMaster Table From Temp Table

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private int AddItemInTempItemMaster(ref List<ItemMasterBomTemp> newItemMasters, List<ItemMasterBomTemp> itemMasters, ItemSubGroupDTO subGroup, BaseChildItemMaster baseItem)
        {
            try
            {
                int maxId = 0;

                var entity = itemMasters.FirstOrDefault(item => item.ItemGroupId == subGroup.ItemGroupId && item.SubGroupId == subGroup.Id
                    && item.Segment1ValueId == baseItem.Segment1ValueId && item.Segment2ValueId == baseItem.Segment2ValueId
                    && item.Segment3ValueId == baseItem.Segment3ValueId && item.Segment4ValueId == baseItem.Segment4ValueId
                    && item.Segment5ValueId == baseItem.Segment5ValueId && item.Segment6ValueId == baseItem.Segment6ValueId
                    && item.Segment7ValueId == baseItem.Segment7ValueId && item.Segment8ValueId == baseItem.Segment8ValueId
                    && item.Segment9ValueId == baseItem.Segment9ValueId && item.Segment10ValueId == baseItem.Segment10ValueId
                    && item.Segment11ValueId == baseItem.Segment11ValueId && item.Segment12ValueId == baseItem.Segment12ValueId
                    && item.Segment13ValueId == baseItem.Segment13ValueId && item.Segment14ValueId == baseItem.Segment14ValueId
                    && item.Segment15ValueId == baseItem.Segment15ValueId);

                // Check for Duplicate Items in ItemMasterBomTemp
                if (entity == null)
                {
                    entity = newItemMasters.FirstOrDefault(item => item.ItemGroupId == subGroup.ItemGroupId && item.SubGroupId == subGroup.Id
                        && item.Segment1ValueId == baseItem.Segment1ValueId && item.Segment2ValueId == baseItem.Segment2ValueId
                        && item.Segment3ValueId == baseItem.Segment3ValueId && item.Segment4ValueId == baseItem.Segment4ValueId
                        && item.Segment5ValueId == baseItem.Segment5ValueId && item.Segment6ValueId == baseItem.Segment6ValueId
                        && item.Segment7ValueId == baseItem.Segment7ValueId && item.Segment8ValueId == baseItem.Segment8ValueId
                        && item.Segment9ValueId == baseItem.Segment9ValueId && item.Segment10ValueId == baseItem.Segment10ValueId
                        && item.Segment11ValueId == baseItem.Segment11ValueId && item.Segment12ValueId == baseItem.Segment12ValueId
                        && item.Segment13ValueId == baseItem.Segment13ValueId && item.Segment14ValueId == baseItem.Segment14ValueId
                        && item.Segment15ValueId == baseItem.Segment15ValueId);
                }

                if (entity == null)
                {
                    maxId = (itemMasters.Any() ? 99900000 : (maxId < 99900000 ? 99900000 : maxId)) + 1;
                    var itemName = baseItem.GetItemName(subGroup.ItemPrefix);
                    entity = new ItemMasterBomTemp
                    {
                        Id = maxId,
                        DisplayItemId = maxId.ToString(),
                        ItemName = itemName,
                        DisplayItemName = itemName,
                        ItemGroupId = subGroup.ItemGroupId,
                        SubGroupId = subGroup.Id,
                        UnitSetId = subGroup.UnitSetId,
                        DefaultTranUnitId = subGroup.DefaultUnitId,
                        DefaultReportUnitId = subGroup.DefaultUnitId,
                        Segment1ValueId = baseItem.Segment1ValueId,
                        Segment2ValueId = baseItem.Segment2ValueId,
                        Segment3ValueId = baseItem.Segment3ValueId,
                        Segment4ValueId = baseItem.Segment4ValueId,
                        Segment5ValueId = baseItem.Segment5ValueId,
                        Segment6ValueId = baseItem.Segment6ValueId,
                        Segment7ValueId = baseItem.Segment7ValueId,
                        Segment8ValueId = baseItem.Segment8ValueId,
                        Segment9ValueId = baseItem.Segment9ValueId,
                        Segment10ValueId = baseItem.Segment10ValueId,
                        Segment11ValueId = baseItem.Segment11ValueId,
                        Segment12ValueId = baseItem.Segment12ValueId,
                        Segment13ValueId = baseItem.Segment13ValueId,
                        Segment14ValueId = baseItem.Segment14ValueId,
                        Segment15ValueId = baseItem.Segment15ValueId
                    };

                    newItemMasters.Add(entity);
                }
                else
                    maxId = entity.Id;

                return maxId;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}