using Dapper;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Application.Interfaces.Admin;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using System.Data.Entity;
using Microsoft.Data.SqlClient;
using EPYSLTEXCore.Infrastructure.Entities.Tex.General.Yarn;

namespace EPYSLTEXCore.Application.Services.General
{
    public class YarnRMPropertiesService : IYarnRMPropertiesService
    {

        private readonly IDapperCRUDService<YarnRMProperties> _service;
        private readonly SqlConnection _connection;
        private readonly SqlConnection _connectionGmt;

        public YarnRMPropertiesService(IDapperCRUDService<YarnRMProperties> service)
        {
            _service = service;
            _service.Connection = service.GetConnection(AppConstants.GMT_CONNECTION);
            _connectionGmt = service.Connection;

            _service.Connection = service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _connection = service.Connection;
        }

        public async Task<List<YarnRMProperties>> GetPagedAsync(PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? " ORDER BY YRMPID DESC" : paginationInfo.OrderBy;
            var sql = string.Empty;

            sql += $@"WITH
                        FinalList AS
                        (
					        SELECT YPM.YRMPID,
                            YPM.FiberTypeID, FiberType = FT.SegmentValue,
                            YPM.BlendTypeID, BlendType = CASE WHEN YPM.BlendTypeID=1 THEN 'Blended' WHEN YPM.BlendTypeID=2 THEN 'Non Blended' ELSE '' END, 
                            YPM.YarnTypeID, YT.ValueName YarnType,
                            YPM.ProgramID, Program = P.SegmentValue,
                            YPM.SubProgramID, SubProgram = SP.SegmentValue,
                            YPM.CertificationID, Certification = C.SegmentValue,
                            YPM.TechnicalParameterID, TechnicalParameter = TPISV.SegmentValue,
                            YPM.YarnCompositionID, YarnComposition = YC.SegmentValue,
                            YPM.ShadeReferenceID, ShadeReference = YSB.ShadeCode,
                            YPM.ManufacturingLineID, ManufacturingLine=ML.SegmentValue,
                            YPM.ManufacturingProcessID, ManufacturingProcess=MP.SegmentValue,
                            YPM.ManufacturingSubProcessID, ManufacturingSubProcess=MSP.SegmentValue,
                            YPM.ColorID, Color=COL.SegmentValue,
                            YPM.ColorGradeID, ColorGrade=CG.SegmentValue,
                            YPM.YarnCountID, YarnCount=YarnCount.SegmentValue
                            FROM {TableNames.YarnRMProperties} YPM
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue FT ON FT.SegmentValueID=YPM.FiberTypeID
                            LEFT JOIN {DbNames.EPYSL}..EntityTypeValue YT ON YT.ValueID=YPM.YarnTypeID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue P ON P.SegmentValueID=YPM.ProgramID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue SP ON SP.SegmentValueID=YPM.SubProgramID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue C ON C.SegmentValueID=YPM.CertificationID
                            LEFT JOIN {DbNames.EPYSL}..SegmentValueYarnTypeMappingSetup TP ON TP.QualityParameterSVID=YPM.TechnicalParameterID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue TPISV ON TPISV.SegmentValueID=TP.QualityParameterSVID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue YC ON YC.SegmentValueID=YPM.YarnCompositionID
                            LEFT JOIN {TableNames.YARN_SHADE_BOOK} YSB ON YSB.YSCID=YPM.ShadeReferenceID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ML ON ML.SegmentValueID=YPM.ManufacturingLineID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue MP ON MP.SegmentValueID=YPM.ManufacturingProcessID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue MSP ON MSP.SegmentValueID=YPM.ManufacturingSubProcessID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID=YPM.ColorID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue CG ON CG.SegmentValueID=YPM.ColorGradeID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue YarnCount ON YarnCount.SegmentValueID=YPM.YarnCountID
			            )
                        SELECT *, Count(*) Over() TotalRows FROM FinalList";

            sql += $@"
                  {paginationInfo.FilterBy}
                  {orderBy}
                  {paginationInfo.PageBy}";
            return await _service.GetDataAsync<YarnRMProperties>(sql);
        }
        public async Task<List<YarnRMProperties>> GetAsync(YarnRMProperties entitie)
        {
            var query =
                $@"
                select * from {TableNames.YarnRMProperties} 
                where YRMPID = {entitie.YRMPID} AND ColorID = '';";
            return await _service.GetDataAsync<YarnRMProperties>(query);
        }


        public async Task<YarnRMProperties> GetAsync(int yrmpID)
        {
            var sql =
                $@"SELECT * FROM {TableNames.YarnRMProperties} 
							Where YRMPID = {yrmpID}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YarnRMProperties data = records.Read<YarnRMProperties>().FirstOrDefault();
                Guard.Against.NullObject(data);

                return data;

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open) _connection.Close();
            }
        }
        public async Task<YarnRMProperties> GetMaster()
        {
            var sql =
                $@"--Fiber Type
                    WITH List AS(
	                    SELECT id = 0, text = 'Select', Sequence = 1
	                    UNION
	                    SELECT id=SegmentValueID,text=SegmentValue, Sequence = 2 
	                    FROM {DbNames.EPYSL}..ItemSegmentValue 
	                    WHERE SegmentNameID IN (261) AND SegmentValueID IN (58957,58956,58955)
                    )
                    SELECT id,text FROM List ORDER BY Sequence,text;

                    --Blend Type

                    SELECT id = 0, text = 'Select Blend Type'
                    UNION
                    SELECT id=1, text='Blended'
                    UNION
                    SELECT id=2, text='Non Blended';

                    -- Fabric Components/Yarn Type

                    WITH List AS(
	                    SELECT id = 0, text = 'Select', Sequence = 1
	                    UNION
	                     Select CAST(EV.ValueID As varchar) [id], EV.ValueName [text], Sequence = 2
	                    From {DbNames.EPYSL}..EntityTypeValue EV
	                    Inner Join {DbNames.EPYSL}..EntityType ET On EV.EntityTypeID = ET.EntityTypeID
	                    LEFT JOIN {TableNames.FiberBasicSetup} FBS ON FBS.ValueID = EV.ValueID
	                    Where ET.EntityTypeName = '{EntityTypeNameConstants.FABRIC_TYPE}' AND ISNULL(FBS.IsInactive,0) = 0
	                    Group By EV.ValueID,EV.ValueName       
                    )
                    SELECT id,text FROM List ORDER BY Sequence,text;

                    --Program

                    WITH List AS(
	                    SELECT id = 0, text = 'Select', Sequence = 1
	                    UNION
	                    SELECT id=SegmentValueID, text=SegmentValue, Sequence = 2
	                    FROM {DbNames.EPYSL}..ItemSegmentValue WHERE SegmentNameID IN (262)       
                    )
                    SELECT id,text FROM List ORDER BY Sequence,text;
                    
                    --Sub-Program

                    WITH List AS(
	                    SELECT id = 0, text = 'Select', Sequence=1
	                    UNION
	                    SELECT distinct CAST(ISV.SegmentValueID As varchar) [id], ISV.SegmentValue [text], Sequence=2
	                    FROM {DbNames.EPYSL}..ItemSegmentName ISN
	                    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISN.SegmentNameID = ISV.SegmentNameID
	                    Left Join {DbNames.EPYSL}..YarnCountHiddenSetup YCH On YCH.YarnCountID = ISV.SegmentValueID
	                    LEFT JOIN T_SubProgramBasicSetup SBS ON SBS.SegmentValueID = ISV.SegmentValueID
	                    WHERE ISNULL(ISV.SegmentValue, '') <> '' And YCH.YarnCountID IS NULL
	                    AND ISNULL(SBS.IsInactive,0) = 0 AND ISN.SegmentName = '{ItemSegmentNameConstants.YARN_SUBPROGRAM_NEW}'       
                    )
                    SELECT id,text FROM List ORDER BY Sequence,text;

                    --Certificate

                    WITH List AS(
	                    SELECT id = 0, text = 'Select', Sequence=1
	                    UNION
	                    SELECT distinct CAST(ISV.SegmentValueID As varchar) [id], ISV.SegmentValue [text], Sequence=2
	                    FROM {DbNames.EPYSL}..ItemSegmentName ISN
	                    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISN.SegmentNameID = ISV.SegmentNameID
	                    LEFT JOIN {DbNames.EPYSL}..YarnCountHiddenSetup YCH On YCH.YarnCountID = ISV.SegmentValueID
	                    LEFT JOIN T_CertificationsBasicSetup CBS ON CBS.SegmentValueID = ISV.SegmentValueID
	                    WHERE ISN.SegmentName = '{ItemSegmentNameConstants.YARN_CERTIFICATIONS}' And 
	                    ISNULL(ISV.SegmentValue, '') <> '' And YCH.YarnCountID IS NULL
	                    AND ISNULL(CBS.IsInactive,0) = 0         
                    )
                    SELECT id,text FROM List ORDER BY Sequence,text;

                    --Technical Parameter

                    WITH List AS(
	                    SELECT id = 0, text = 'Select', Sequence=1
	                    UNION
	                    SELECT distinct 
	                    CAST(ISV.SegmentValueID As varchar) [id], ISV.SegmentValue [text], Sequence = 2
	                    from {DbNames.EPYSL}..ItemSegmentValue ISV    
	                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentName ISN ON ISN.SegmentNameID = ISV.SegmentNameID 
	                    WHERE ISN.SegmentName In ('{ItemSegmentNameConstants.YARN_QUALITY_PARAMETER}')                  
                    )
                    SELECT id,text FROM List ORDER BY Sequence,text;

                    --Yarn Composition

                     WITH List AS(
	                    SELECT id = 0, text = 'Select', Sequence=1
	                    UNION
	                    SELECT 
	                    id = ISV.SegmentValueID
	                    ,text = ISV.SegmentValue, Sequence=2
	                    from {DbNames.EPYSL}..ItemSegmentValue ISV
	                    INNER JOIN {DbNames.EPYSL}..ItemSegmentName ISN ON ISN.SegmentNameID = ISV.SegmentNameID
	                    WHERE ISN.SegmentName In ('{ItemSegmentNameConstants.YARN_COMPOSITION}') AND ISNULL(ISV.SegmentValue,'') <> ''
	                    AND ISV.SegmentValueID != 93222                    
                    )
                    SELECT id,text FROM List ORDER BY Sequence,text;

                    --Shade Reference

                    WITH List AS(
	                    SELECT id = 0, text = 'Select', Sequence=1
	                    UNION
	                    SELECT id = YSCID, text = ShadeCode, Sequence=2 FROM {TableNames.YARN_SHADE_BOOK}                      
                    )
                    SELECT id,text FROM List ORDER BY Sequence,text;

                    --Manufacturing Line

                    WITH List AS(
	                    SELECT id = 0, text = 'Select', Sequence=1
	                    UNION
	                    SELECT distinct
	                    id = ISV.SegmentValueID
	                    ,text = ISV.SegmentValue, Sequence=2
	                    from {DbNames.EPYSL}..ItemSegmentValue ISV   
	                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentName ISN ON ISN.SegmentNameID = ISV.SegmentNameID 
	                    WHERE ISN.SegmentName In ('{ItemSegmentNameConstants.YARN_TYPE}') AND ISNULL(ISV.SegmentValue,'') <> ''                      
                    )
                SELECT id,text FROM List ORDER BY Sequence,text;

                    --Manufacturing Process

                    WITH List AS(
					    SELECT id = 0, text = 'Select', Sequence=1
	                    UNION
                        SELECT distinct
                         id = ISV.SegmentValueID
                        ,text = ISV.SegmentValue, Sequence=2
                        from {DbNames.EPYSL}..ItemSegmentValue ISV  
                        LEFT JOIN  {DbNames.EPYSL}..ItemSegmentName ISN ON ISN.SegmentNameID = ISV.SegmentNameID 
                        WHERE ISN.SegmentName In ('{ItemSegmentNameConstants.YARN_MANUFACTURING_PROCESS}') AND ISNULL(ISV.SegmentValue,'') <> ''            
                    )
                    SELECT id,text FROM List ORDER BY Sequence,text;

                    --Manufacturing Sub-process

                   WITH List AS(
	                SELECT id = 0, text = 'Select', Sequence=1
	                UNION
	                SELECT distinct
	                id = ISV.SegmentValueID
	                ,text = ISV.SegmentValue, Sequence=2
	                from {DbNames.EPYSL}..ItemSegmentValue ISV  
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentName ISN ON ISN.SegmentNameID = ISV.SegmentNameID 
	                WHERE ISN.SegmentName In ('{ItemSegmentNameConstants.YARN_MANUFACTURING_SUB_PROCESS}') AND ISNULL(ISV.SegmentValue,'') <> ''            
                )
                SELECT id,text FROM List ORDER BY Sequence,text;

                    --Color

                    WITH List AS(
	                SELECT id = 0, text = 'Select', Sequence=1
	                UNION
	                SELECT id = SegmentValueID, text = SegmentValue , Sequence=2
	                FROM {DbNames.EPYSL}..ItemSegmentValue a 
	                WHERE a.SegmentNameID = 131)
                    SELECT id,text FROM List ORDER BY Sequence,text;

                    --Color Grade

                    WITH List AS(
	                SELECT id = 0, text = 'Select', Sequence=1
	                UNION
	                SELECT id = SegmentValueID, text = SegmentValue , Sequence=2
	                FROM {DbNames.EPYSL}..ItemSegmentValue a 
	                WHERE a.SegmentNameID = 269)
                    SELECT id,text FROM List ORDER BY Sequence,text;

                    --Count

                    WITH List AS(
	                SELECT id = 0, text = 'Select', Sequence=1
	                UNION
	                SELECT distinct
	                 id = ISV.SegmentValueID
	                ,text = ISV.SegmentValue, Sequence = 2
	                from {DbNames.EPYSL}..ItemSegmentValue ISV
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentName ISN ON ISN.SegmentNameID = ISV.SegmentNameID
	                WHERE ISN.SegmentName In ('{ItemSegmentNameConstants.YARN_COUNT}')
                )
                SELECT id,text FROM List ORDER BY Sequence,text;";  
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YarnRMProperties data = new YarnRMProperties();
                data.FiberTypeList = records.Read<Select2OptionModel>().ToList();
                data.BlendTypeList = records.Read<Select2OptionModel>().ToList();
                data.YarnTypeList = records.Read<Select2OptionModel>().ToList();
                data.ProgramList = records.Read<Select2OptionModel>().ToList();
                data.SubProgramList = records.Read<Select2OptionModel>().ToList();
                data.CertificationList = records.Read<Select2OptionModel>().ToList();
                data.TechnicalParameterList = records.Read<Select2OptionModel>().ToList();
                data.YarnCompositionList = records.Read<Select2OptionModel>().ToList();
                data.ShadeReferenceList = records.Read<Select2OptionModel>().ToList();
                data.ManufacturingLineList = records.Read<Select2OptionModel>().ToList();
                data.ManufacturingProcessList = records.Read<Select2OptionModel>().ToList();
                data.ManufacturingSubProcessList = records.Read<Select2OptionModel>().ToList();
                data.ColorList = records.Read<Select2OptionModel>().ToList();
                data.ColorGradeList = records.Read<Select2OptionModel>().ToList();
                data.YarnCountList = records.Read<Select2OptionModel>().ToList();
                Guard.Against.NullObject(data);

                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open) _connection.Close();
            }
        }
        public async Task SaveAsync(YarnRMProperties entity)
        {
            SqlTransaction transaction = null;
            SqlTransaction transactionGmt = null;
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                await _connectionGmt.OpenAsync();
                transactionGmt = _connectionGmt.BeginTransaction();

                switch (entity.EntityState)
                {
                    case EntityState.Added:
                        entity.YRMPID = await _service.GetMaxIdAsync(TableNames.YarnRMProperties, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        break;

                    //case EntityState.Modified:
                    //    await UpdateAsync(entity);
                    //    break;

                    default:
                        break;
                }

                await _service.SaveSingleAsync(entity, _connection, transaction);

                transaction.Commit();
                transactionGmt.Commit();
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                if (transactionGmt != null) transactionGmt.Rollback();
                throw (ex);
            }
            finally
            {
                if (transaction != null) transaction.Dispose();
                if (transactionGmt != null) transactionGmt.Dispose();
                _connection.Close();
                _connectionGmt.Close();
            }
        }
        private async Task<YarnRMProperties> AddAsync(YarnRMProperties entity, SqlTransaction transactionGmt)
        {
            entity.YRMPID = await _service.GetMaxIdAsync(TableNames.YarnRMProperties, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

            return entity;
        }
        
    }
}
