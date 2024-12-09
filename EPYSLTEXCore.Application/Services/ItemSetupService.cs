using Dapper;
using EPYSLTEX.Core.Interfaces.Repositories;
using EPYSLTEXCore.Application.Interfaces;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.General;
using EPYSLTEXCore.Infrastructure.Entities.Tex.General.Yarn;
using EPYSLTEXCore.Infrastructure.Static;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;

namespace EPYSLTEX.Infrastructure.Services
{
    public class ItemSetupService : IItemSetupService
    {
        private readonly IDapperCRUDService<DapperBaseEntity> _sqlQueryService;
        private readonly SqlConnection _connection;
        private readonly IDapperCRUDService<ItemSegmentChild> _service;

        public ItemSetupService(IDapperCRUDService<DapperBaseEntity> sqlQueryService, IDapperCRUDService<ItemSegmentChild> service)
        {
            _sqlQueryService = sqlQueryService;
            _service = service;
           _service.Connection = service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _connection = _service.Connection;
        }

        public async Task<List<YarnProcessSetupMasterDTO>> GetProcessSetupAsync(int fiberTypeId)
        {
            var query = $@"
                WITH 
                M As (
	                Select M.*, FT.FiberTypeID 
	                From YarnProcessSetupChildFiberType FT
	                Inner Join YarnProcessSetupMaster M On FT.SetupMasterID = M.SetupMasterID
	                Where FT.FiberTypeID = {fiberTypeId}
                )
                , COM As (
	                Select M.SetupMasterID, ISV.ChildID [CountID], ISV.DisplayValue [Count] 
	                From YarnProcessSetupChildYarnCount YSC
	                INNER JOIN M On YSC.SetupMasterID = M.SetupMasterID
	                INNER JOIN {DbNames.EPYSL}..ItemSegmentDisplayValueYarnCount ISV On YSC.CountID = ISV.ChildID
                )
                , PRSP As (
	                Select SetupMasterID,
	                STUFF(
	                (
		                SELECT ', ' + CAST(SPI.[Count] As varchar) AS [text()]
		                FROM COM SPI
		                WHERE SPI.SetupMasterID = COM.SetupMasterID
		                FOR XML PATH('')
	                ), 1, 2, '') As CountNames
	                From COM
	                Group By SetupMasterID
                )

                SELECT M.SetupMasterID Id, M.FiberTypeID, M.ManufacturingLineID, M.ManufacturingProcessID, M.ManufacturingSubProcessID, M.YarnColorID, M.ColorGradeID,
	                ML.SegmentValue ManufacturingLine, MP.SegmentValue ManufacturingProcess, SMP.SegmentValue ManufacturingSubProcess, YC.SegmentValue YarnColor, 
	                CG.SegmentValue ColorGrade, PRSP.CountNames Counts
                FROM M 
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ML ON ML.SegmentValueID = M.ManufacturingLineID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue MP ON MP.SegmentValueID = M.ManufacturingProcessID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue SMP ON SMP.SegmentValueID = M.ManufacturingSubProcessID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue YC ON YC.SegmentValueID = M.YarnColorID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue CG ON CG.SegmentValueID = M.ColorGradeID
                LEFT JOIN PRSP ON PRSP.SetupMasterID = M.SetupMasterID";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryAsync<YarnProcessSetupMasterDTO>(query);
                return records.ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _connection.Close();
            }
        }

        public async Task<List<YarnProductSetupChildDTO>> GetProductSetupAsync(int fiberTypeId)
        {
            var query = $@"
                ;WITH MM AS (
	                SELECT M.SetupMasterID, M.FiberTypeID
	                From YarnProductSetupMaster M Where FiberTypeID = 58947 
                )
                ,C As (
	                Select C.* 
	                From MM
                    Inner Join YarnProductSetupChild C On MM.SetupMasterID = C.SetupMasterID
                )

                Select BlendTypeID, BlendType.SegmentValue BlendType, YarnTypeID, YarnType.SegmentValue YarnType
	                , ProgramID, ISNULL(Program.SegmentValue, '') Program, SubProgramID, ISNULL(SubProgram.SegmentValue, '') SubProgram
	                , TechnicalParameterID, ISNULL(TechnicalParameter.SegmentValue, '') TechnicalParameter
	                , CertificationsID, ISNULL(Certifications.SegmentValue, '') Certifications, C.TechnicalParameterID, TP.SegmentValue TechnicalParameter
	                , CompositionsID, CP.SegmentValue Compositions, ShadeID, SD.SegmentValue Shade
	                , ManufacturingLineID, ML.SegmentValue ManufacturingLine, ManufacturingProcessID, MP.SegmentValue ManufacturingProcess
	                , ManufacturingSubProcessID, MSP.SegmentValue ManufacturingSubProcess, YarnColorID, YC.SegmentValue YarnColor, ColorGradeID, CG.SegmentValue ColorGrade
                From C
                Inner Join MM ON MM.SetupMasterID = C.SetupMasterID
                Left Join {DbNames.EPYSL}..ItemSegmentValue BlendType On C.BlendTypeID = BlendType.SegmentValueID
                Left Join {DbNames.EPYSL}..ItemSegmentValue YarnType On C.YarnTypeID = YarnType.SegmentValueID
                Left Join {DbNames.EPYSL}..ItemSegmentValue Program On C.ProgramID = Program.SegmentValueID
                Left Join {DbNames.EPYSL}..ItemSegmentValue SubProgram On C.SubProgramID = SubProgram.SegmentValueID
                Left Join {DbNames.EPYSL}..ItemSegmentValue TechnicalParameter On C.TechnicalParameterID = TechnicalParameter.SegmentValueID
                Left Join {DbNames.EPYSL}..ItemSegmentValue Certifications On C.CertificationsID = Certifications.SegmentValueID
                Left Join {DbNames.EPYSL}..ItemSegmentValue TP On C.TechnicalParameterID = TP.SegmentValueID
                Left Join {DbNames.EPYSL}..ItemSegmentValue CP On C.CompositionsID = CP.SegmentValueID
                Left Join {DbNames.EPYSL}..ItemSegmentValue SD On C.ShadeID = SD.SegmentValueID
                Left Join {DbNames.EPYSL}..ItemSegmentValue ML On C.ManufacturingLineID = ML.SegmentValueID
                Left Join {DbNames.EPYSL}..ItemSegmentValue MP On C.ManufacturingProcessID = MP.SegmentValueID
                Left Join {DbNames.EPYSL}..ItemSegmentValue MSP On C.ManufacturingSubProcessID = MSP.SegmentValueID
                Left Join {DbNames.EPYSL}..ItemSegmentValue YC On C.YarnColorID = YC.SegmentValueID
                Left Join {DbNames.EPYSL}..ItemSegmentValue CG On C.ColorGradeID = CG.SegmentValueID
                Group By BlendTypeID, BlendType.SegmentValue, YarnTypeID, YarnType.SegmentValue
	                , ProgramID, Program.SegmentValue, SubProgramID, SubProgram.SegmentValue
	                , TechnicalParameterID, TechnicalParameter.SegmentValue, CertificationsID, Certifications.SegmentValue
	                , C.TechnicalParameterID, TP.SegmentValue, CompositionsID, CP.SegmentValue, ShadeID, SD.SegmentValue
	                , ManufacturingLineID, ML.SegmentValue, ManufacturingProcessID, MP.SegmentValue
	                , ManufacturingSubProcessID, MSP.SegmentValue, YarnColorID, YC.SegmentValue, ColorGradeID, CG.SegmentValue";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryAsync<YarnProductSetupChildDTO>(query);
                return records.ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _connection.Close();
            }
           
        }

        public async Task<ItemInformation> GetItemStructureBySubGroup(string subGroupName)
        {
            var query = $@"
                {CommonQueries.GetItemStructureBySubGroupForAllItem(subGroupName)};

                ;Select CAST(ISV.SegmentValueID AS VARCHAR) id, ISV.SegmentValue [text], IST.SegmentNameID [desc]
                From {DbNames.EPYSL}..ItemStructure IST
                Left Join {DbNames.EPYSL}..ItemSegmentValue ISV On ISV.SegmentNameID = IST.SegmentNameID
                Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On IST.SubGroupID = ISG.SubGroupID
                Where ISG.SubGroupName = '{subGroupName}' And AllowAdd = 0 And ISNULL(ISV.SegmentValue, '') != '';";

  


            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                ItemInformation itemInformation = new ItemInformation
                {
                    ItemStructures = records.Read<ItemStructureDTO>().ToList(),
                    ItemSegmentValues = records.Read<Select2OptionModel>().ToList()
                };
                return itemInformation;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _connection.Close();
            }
        }
        public async Task<bool> GetCacheUseSetup()
        {
            var query = $@"SELECT IsUseCacheForYarnSegmentFilter FROM CacheUseSetup";

      

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);

                bool isUseCacheForYarnSegmentFilter = records.Read<bool>().FirstOrDefault();


                return isUseCacheForYarnSegmentFilter;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _connection.Close();
            }
        }
        public async Task<List<CacheResetSetup>> GetCacheResetSetupsAsync(PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? " ORDER BY CacheID DESC" : paginationInfo.OrderBy;
            var sql = string.Empty;

            sql += $@"Select * FROM CacheResetSetup";

            sql += $@"
                  {paginationInfo.FilterBy}
                  {orderBy}
                  {paginationInfo.PageBy}";
            return await _service.GetDataAsync<CacheResetSetup>(sql);
        }
        public async Task<List<ItemStructureDTO>> GetItemStructureForDisplayBySubGroup(string subGroupName)
        {
            var query = $@"{CommonQueries.GetItemStructureBySubGroupForAllItem(subGroupName)};";
            

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryAsync<ItemStructureDTO>(query);
                return records.ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _connection.Close();
            }
        }
        public async Task SaveAsync(ItemSegmentMaster entity, int userId)
        {
            SqlTransaction transaction = null;
            try
            {


                foreach (ItemSegmentChild item in entity.Compositions)
                {
                    
                    await _service.ExecuteAsync("sp_Segment_Active_Inactive_Setup", new { EntityState = item.EntityState , UserId = userId,
                        PrimaryKeyId = item.SegmentValueId,
                        SecondParamValue = Convert.ToInt32(item.IsInactive),
                        ThirdParamValue = EnumSegmentType.Composition,
                    }, 30, CommandType.StoredProcedure);
                }
                foreach (ItemSegmentChild item in entity.YarnTypes)
                {
                    await _service.ExecuteAsync("sp_Segment_Active_Inactive_Setup", new
                    {
                        EntityState = item.EntityState,
                        UserId = userId,
                        PrimaryKeyId = item.SegmentValueId,
                        SecondParamValue = Convert.ToInt32(item.IsInactive),
                        ThirdParamValue = EnumSegmentType.YarnType,
                    }, 30, CommandType.StoredProcedure);
                }
                foreach (ItemSegmentChild item in entity.Processes)
                {
                    await _service.ExecuteAsync("sp_Segment_Active_Inactive_Setup", new
                    {
                        EntityState = item.EntityState,
                        UserId = userId,
                        PrimaryKeyId = item.SegmentValueId,
                        SecondParamValue = Convert.ToInt32(item.IsInactive),
                        ThirdParamValue = EnumSegmentType.Process,
                    }, 30, CommandType.StoredProcedure);
                }
                foreach (ItemSegmentChild item in entity.SubProcesses)
                {
                    await _service.ExecuteAsync("sp_Segment_Active_Inactive_Setup", new
                    {
                        EntityState = item.EntityState,
                        UserId = userId,
                        PrimaryKeyId = item.SegmentValueId,
                        SecondParamValue = Convert.ToInt32(item.IsInactive),
                        ThirdParamValue = EnumSegmentType.SubProcess,
                    }, 30, CommandType.StoredProcedure);
                }
                foreach (ItemSegmentChild item in entity.QualityParameters)
                {
                    await _service.ExecuteAsync("sp_Segment_Active_Inactive_Setup", new
                    {
                        EntityState = item.EntityState,
                        UserId = userId,
                        PrimaryKeyId = item.SegmentValueId,
                        SecondParamValue = Convert.ToInt32(item.IsInactive),
                        ThirdParamValue = EnumSegmentType.QualityParameter,
                    }, 30, CommandType.StoredProcedure);
                }
                foreach (ItemSegmentChild item in entity.Counts)
                {
                    await _service.ExecuteAsync("sp_Segment_Active_Inactive_Setup", new
                    {
                        EntityState = item.EntityState,
                        UserId = userId,
                        PrimaryKeyId = item.SegmentValueId,
                        SecondParamValue = Convert.ToInt32(item.IsInactive),
                        ThirdParamValue = EnumSegmentType.Count,
                    }, 30, CommandType.StoredProcedure);
                }
                foreach (ItemSegmentChild item in entity.Fibers)
                {
                    await _service.ExecuteAsync("sp_Segment_Active_Inactive_Setup", new
                    {
                        EntityState = item.EntityState,
                        UserId = userId,
                        PrimaryKeyId = item.SegmentValueId,
                        SecondParamValue = Convert.ToInt32(item.IsInactive),
                        ThirdParamValue = EnumSegmentType.Fiber,
                    }, 30, CommandType.StoredProcedure);
                }
                foreach (ItemSegmentChild item in entity.SubPrograms)
                {
                    await _service.ExecuteAsync("sp_Segment_Active_Inactive_Setup", new
                    {
                        EntityState = item.EntityState,
                        UserId = userId,
                        PrimaryKeyId = item.SegmentValueId,
                        SecondParamValue = Convert.ToInt32(item.IsInactive),
                        ThirdParamValue = EnumSegmentType.SubProgram,
                    }, 30, CommandType.StoredProcedure);
                }
                    foreach (ItemSegmentChild item in entity.Certifications)
                {
                    await _service.ExecuteAsync("sp_Segment_Active_Inactive_Setup", new
                    {
                        EntityState = item.EntityState,
                        UserId = userId,
                        PrimaryKeyId = item.SegmentValueId,
                        SecondParamValue = Convert.ToInt32(item.IsInactive),
                        ThirdParamValue = EnumSegmentType.Certification,
                    }, 30, CommandType.StoredProcedure);
                }
                transaction.Commit();
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                if (ex.Message.Contains('~')) throw new Exception(ex.Message.Split('~')[0]);
                throw ex;
            }
            finally
            {
                _connection.Close();
            }
        }
        public async Task SaveCacheForYarnSegmentFilterUpdateTimeAsync(string runningMode)
        {
            SqlTransaction transaction = null;
            try
            {
                await _connection.OpenAsync();
                await _connection.ExecuteAsync("spStoreCacheForYarnSegmentFilterUpdateTime", new { RunningMode = runningMode }, transaction, 30, CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                if (ex.Message.Contains('~')) throw new Exception(ex.Message.Split('~')[0]);
                throw ex;
            }
            finally
            {
                _connection.Close();
            }
        }
    }
}
