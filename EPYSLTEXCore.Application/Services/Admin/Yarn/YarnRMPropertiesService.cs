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
using EPYSLTEXCore.Infrastructure.Entities.Tex.General;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Admin;

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
					        SELECT YPM.YRMPID,YPMC.YRMPChildID,
							YPMC.SupplierID,Supplier = SUP.ShortName,
							YPMC.SpinnerID,Spinner = SPN.ShortName,
                            YPM.FiberTypeID, FiberType = FT.SegmentValue,
                            YPM.BlendTypeID, BlendType = CASE WHEN YPM.BlendTypeID=1 THEN 'Blended' WHEN YPM.BlendTypeID=2 THEN 'Non Blended' ELSE '' END, 
                            YPM.YarnTypeID, YT.ValueName YarnType,
                            YPM.ProgramID, Program = P.SegmentValue,
                            YPM.SubProgramID, SubProgram = SP.SegmentValue,
                            YPM.CertificationID, Certification = C.SegmentValue,
                            YPM.TechnicalParameterID, TechnicalParameter = TP.SegmentValue,
                            YPM.YarnCompositionID, YarnComposition = YC.SegmentValue,
                            YPM.ShadeReferenceID, ShadeReference = YSB.ShadeCode,
                            YPM.ManufacturingLineID, ManufacturingLine=ML.SegmentValue,
                            YPM.ManufacturingProcessID, ManufacturingProcess=MP.SegmentValue,
                            YPM.ManufacturingSubProcessID, ManufacturingSubProcess=MSP.SegmentValue,
                            YPM.ColorID, Color=COL.SegmentValue,
                            YPM.ColorGradeID, ColorGrade=CG.SegmentValue,
                            YPM.YarnCountID, YarnCount=YarnCount.SegmentValue
                            FROM {TableNames.YarnRMProperties} YPM
							LEFT JOIN YarnRMPropertiesChild YPMC ON YPMC.YRMPID=YPM.YRMPID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue FT ON FT.SegmentValueID=YPM.FiberTypeID
                            LEFT JOIN {DbNames.EPYSL}..EntityTypeValue YT ON YT.ValueID=YPM.YarnTypeID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue P ON P.SegmentValueID=YPM.ProgramID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue SP ON SP.SegmentValueID=YPM.SubProgramID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue C ON C.SegmentValueID=YPM.CertificationID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue TP ON TP.SegmentValueID=YPM.TechnicalParameterID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue YC ON YC.SegmentValueID=YPM.YarnCompositionID
                            LEFT JOIN {TableNames.YARN_SHADE_BOOK} YSB ON YSB.YSCID=YPM.ShadeReferenceID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ML ON ML.SegmentValueID=YPM.ManufacturingLineID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue MP ON MP.SegmentValueID=YPM.ManufacturingProcessID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue MSP ON MSP.SegmentValueID=YPM.ManufacturingSubProcessID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID=YPM.ColorID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue CG ON CG.SegmentValueID=YPM.ColorGradeID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue YarnCount ON YarnCount.SegmentValueID=YPM.YarnCountID
							LEFT JOIN {DbNames.EPYSL}..Contacts SPN ON SPN.ContactID=YPMC.SpinnerID
							LEFT JOIN {DbNames.EPYSL}..Contacts SUP ON SUP.ContactID=YPMC.SupplierID
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

        public async Task<YarnRMProperties> GetById(int id)
        {
            var sql =
                $@"
                   --Master 
                   SELECT * FROM {TableNames.YarnRMProperties} 
							Where YRMPID = {id};
                   --Child
                   SELECT * FROM {TableNames.YarnRMPropertiesChild} 
							Where YRMPID = {id};
                ";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YarnRMProperties data = records.Read<YarnRMProperties>().FirstOrDefault();
                data.Childs = records.Read<YarnRMPropertiesChild>().ToList();
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
        public async Task<YarnRMProperties> GetNewAsync()
        {
            var sql =
                $@"--Fiber Type
                    SELECT id=SegmentValueID,text=SegmentValue 
	                FROM {DbNames.EPYSL}..ItemSegmentValue 
	                WHERE SegmentNameID IN (261) AND SegmentValueID IN (58957,58956,58955)
					ORDER BY SegmentValue ASC;

                    --Blend Type
                    SELECT id=1, text='Blended'
                    UNION
                    SELECT id=2, text='Non Blended';

                    -- Fabric Components/Yarn Type
                    Select CAST(EV.ValueID As varchar) [id], EV.ValueName [text]
	                From EPYSL..EntityTypeValue EV
	                Inner Join {DbNames.EPYSL}..EntityType ET On EV.EntityTypeID = ET.EntityTypeID
	                LEFT JOIN {TableNames.FiberBasicSetup} FBS ON FBS.ValueID = EV.ValueID
	                Where ET.EntityTypeName = '{EntityTypeNameConstants.FABRIC_TYPE}' AND ISNULL(FBS.IsInactive,0) = 0
	                Group By EV.ValueID,EV.ValueName ORDER BY EV.ValueName; 

                    --Program
                    SELECT id=SegmentValueID, text=SegmentValue
	                FROM {DbNames.EPYSL}..ItemSegmentValue WHERE SegmentNameID IN (262)
					ORDER BY SegmentValue;
                    
                    --Sub-Program
                    SELECT distinct CAST(ISV.SegmentValueID As varchar) [id], ISV.SegmentValue [text]
	                FROM {DbNames.EPYSL}..ItemSegmentName ISN
	                INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISN.SegmentNameID = ISV.SegmentNameID
	                Left Join {DbNames.EPYSL}..YarnCountHiddenSetup YCH On YCH.YarnCountID = ISV.SegmentValueID
	                LEFT JOIN {TableNames.SubProgramBasicSetup} SBS ON SBS.SegmentValueID = ISV.SegmentValueID
	                WHERE ISNULL(ISV.SegmentValue, '') <> '' And YCH.YarnCountID IS NULL
	                AND ISNULL(SBS.IsInactive,0) = 0 AND ISN.SegmentName = '{ItemSegmentNameConstants.YARN_SUBPROGRAM_NEW}'
					ORDER BY ISV.SegmentValue;

                    --Certificate
                    SELECT distinct CAST(ISV.SegmentValueID As varchar) [id], ISV.SegmentValue [text]
	                FROM {DbNames.EPYSL}..ItemSegmentName ISN
	                INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISN.SegmentNameID = ISV.SegmentNameID
	                LEFT JOIN {DbNames.EPYSL}..YarnCountHiddenSetup YCH On YCH.YarnCountID = ISV.SegmentValueID
	                LEFT JOIN {TableNames.CertificationsBasicSetup} CBS ON CBS.SegmentValueID = ISV.SegmentValueID
	                WHERE ISN.SegmentName = '{ItemSegmentNameConstants.YARN_CERTIFICATIONS}' And 
	                ISNULL(ISV.SegmentValue, '') <> '' And YCH.YarnCountID IS NULL
	                AND ISNULL(CBS.IsInactive,0) = 0 ORDER BY ISV.SegmentValue;

                    --Technical Parameter
                    SELECT distinct 
	                CAST(ISV.SegmentValueID As varchar) [id], ISV.SegmentValue [text], Sequence = 2
	                from {DbNames.EPYSL}..ItemSegmentValue ISV    
	                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentName ISN ON ISN.SegmentNameID = ISV.SegmentNameID 
	                WHERE ISN.SegmentName In ('{ItemSegmentNameConstants.YARN_QUALITY_PARAMETER}') ORDER BY ISV.SegmentValue ;

                    --Yarn Composition

                    SELECT 
	                id = ISV.SegmentValueID
	                ,text = ISV.SegmentValue, Sequence=2
	                from {DbNames.EPYSL}..ItemSegmentValue ISV
	                INNER JOIN {DbNames.EPYSL}..ItemSegmentName ISN ON ISN.SegmentNameID = ISV.SegmentNameID
	                WHERE ISN.SegmentName In ('{ItemSegmentNameConstants.YARN_COMPOSITION}') AND ISNULL(ISV.SegmentValue,'') <> ''
	                AND ISV.SegmentValueID != 93222   ORDER BY ISV.SegmentValue;

                    --Shade Reference
                    SELECT id = YSCID, text = ShadeCode FROM {TableNames.YARN_SHADE_BOOK} ORDER BY ShadeCode;

                    --Manufacturing Line
                    SELECT distinct
					id = ISV.SegmentValueID
					,text = ISV.SegmentValue
					from {DbNames.EPYSL}..ItemSegmentValue ISV   
					LEFT JOIN  {DbNames.EPYSL}..ItemSegmentName ISN ON ISN.SegmentNameID = ISV.SegmentNameID 
					WHERE ISN.SegmentName In ('{ItemSegmentNameConstants.YARN_TYPE}') AND ISNULL(ISV.SegmentValue,'') <> ''
					ORDER BY ISV.SegmentValue;

                    --Manufacturing Process
                    SELECT distinct
                    id = ISV.SegmentValueID
                    ,text = ISV.SegmentValue
                    from {DbNames.EPYSL}..ItemSegmentValue ISV  
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentName ISN ON ISN.SegmentNameID = ISV.SegmentNameID 
                    WHERE ISN.SegmentName In ('{ItemSegmentNameConstants.YARN_MANUFACTURING_PROCESS}') AND ISNULL(ISV.SegmentValue,'') <> '' 
				    ORDER BY ISV.SegmentValue;

                    --Manufacturing Sub-process
                    SELECT distinct
	                id = ISV.SegmentValueID
	                ,text = ISV.SegmentValue
	                from {DbNames.EPYSL}..ItemSegmentValue ISV  
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentName ISN ON ISN.SegmentNameID = ISV.SegmentNameID 
	                WHERE ISN.SegmentName In ('{ItemSegmentNameConstants.YARN_MANUFACTURING_SUB_PROCESS}') AND ISNULL(ISV.SegmentValue,'') <> '' 
					ORDER BY ISV.SegmentValue;

                    --Color
                    SELECT id = SegmentValueID, text = SegmentValue
	                FROM {DbNames.EPYSL}..ItemSegmentValue a 
	                WHERE a.SegmentNameID = 131 ORDER BY SegmentValue;

                    --Color Grade
                    SELECT id = SegmentValueID, text = SegmentValue
	                FROM {DbNames.EPYSL}..ItemSegmentValue a 
	                WHERE a.SegmentNameID = 269 ORDER BY SegmentValue;

                    --Count
                    SELECT distinct
	                 id = ISV.SegmentValueID
	                ,text = ISV.SegmentValue, Sequence = 2
	                from {DbNames.EPYSL}..ItemSegmentValue ISV
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentName ISN ON ISN.SegmentNameID = ISV.SegmentNameID
	                WHERE ISN.SegmentName In ('{ItemSegmentNameConstants.YARN_COUNT}')
					ORDER BY ISV.SegmentValue;

                    -----SpinnerList
                    {CommonQueries.GetContactsByCategoryType(ContactCategoryNames.SPINNER)};

                    -----SupplierList
                    {CommonQueries.GetContactsByCategoryType(ContactCategoryNames.SUPPLIER)};";  
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
                data.SpinnerList = records.Read<Select2OptionModel>().ToList();
                data.SupplierList = records.Read<Select2OptionModel>().ToList();
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
        public async Task<YarnRMProperties> GetDetails(int yrmpID)
        {
            var sql =
                $@"
                    --Master
                    SELECT * FROM YarnRMProperties WHERE YRMPID={yrmpID};

                    --Child
                    SELECT YPMC.*, Supplier = SUP.ShortName, Spinner = SPN.ShortName FROM YarnRMPropertiesChild YPMC 
                    LEFT JOIN EPYSL..Contacts SPN ON SPN.ContactID=YPMC.SpinnerID
                    LEFT JOIN EPYSL..Contacts SUP ON SUP.ContactID=YPMC.SupplierID
                    WHERE YPMC.YRMPID={yrmpID};

                    --Fiber Type
                    SELECT id=SegmentValueID,text=SegmentValue 
	                FROM {DbNames.EPYSL}..ItemSegmentValue 
	                WHERE SegmentNameID IN (261) AND SegmentValueID IN (58957,58956,58955)
					ORDER BY SegmentValue ASC;

                    --Blend Type
                    SELECT id=1, text='Blended'
                    UNION
                    SELECT id=2, text='Non Blended';

                    -- Fabric Components/Yarn Type
                    Select CAST(EV.ValueID As varchar) [id], EV.ValueName [text]
	                From EPYSL..EntityTypeValue EV
	                Inner Join {DbNames.EPYSL}..EntityType ET On EV.EntityTypeID = ET.EntityTypeID
	                LEFT JOIN {TableNames.FiberBasicSetup} FBS ON FBS.ValueID = EV.ValueID
	                Where ET.EntityTypeName = '{EntityTypeNameConstants.FABRIC_TYPE}' AND ISNULL(FBS.IsInactive,0) = 0
	                Group By EV.ValueID,EV.ValueName ORDER BY EV.ValueName; 

                    --Program
                    SELECT id=SegmentValueID, text=SegmentValue
	                FROM {DbNames.EPYSL}..ItemSegmentValue WHERE SegmentNameID IN (262)
					ORDER BY SegmentValue;
                    
                    --Sub-Program
                    SELECT distinct CAST(ISV.SegmentValueID As varchar) [id], ISV.SegmentValue [text]
	                FROM {DbNames.EPYSL}..ItemSegmentName ISN
	                INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISN.SegmentNameID = ISV.SegmentNameID
	                Left Join {DbNames.EPYSL}..YarnCountHiddenSetup YCH On YCH.YarnCountID = ISV.SegmentValueID
	                LEFT JOIN {TableNames.SubProgramBasicSetup} SBS ON SBS.SegmentValueID = ISV.SegmentValueID
	                WHERE ISNULL(ISV.SegmentValue, '') <> '' And YCH.YarnCountID IS NULL
	                AND ISNULL(SBS.IsInactive,0) = 0 AND ISN.SegmentName = '{ItemSegmentNameConstants.YARN_SUBPROGRAM_NEW}'
					ORDER BY ISV.SegmentValue;

                    --Certificate
                    SELECT distinct CAST(ISV.SegmentValueID As varchar) [id], ISV.SegmentValue [text]
	                FROM {DbNames.EPYSL}..ItemSegmentName ISN
	                INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISN.SegmentNameID = ISV.SegmentNameID
	                LEFT JOIN {DbNames.EPYSL}..YarnCountHiddenSetup YCH On YCH.YarnCountID = ISV.SegmentValueID
	                LEFT JOIN {TableNames.CertificationsBasicSetup} CBS ON CBS.SegmentValueID = ISV.SegmentValueID
	                WHERE ISN.SegmentName = '{ItemSegmentNameConstants.YARN_CERTIFICATIONS}' And 
	                ISNULL(ISV.SegmentValue, '') <> '' And YCH.YarnCountID IS NULL
	                AND ISNULL(CBS.IsInactive,0) = 0 ORDER BY ISV.SegmentValue;

                    --Technical Parameter
                    SELECT distinct 
	                CAST(ISV.SegmentValueID As varchar) [id], ISV.SegmentValue [text], Sequence = 2
	                from {DbNames.EPYSL}..ItemSegmentValue ISV    
	                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentName ISN ON ISN.SegmentNameID = ISV.SegmentNameID 
	                WHERE ISN.SegmentName In ('{ItemSegmentNameConstants.YARN_QUALITY_PARAMETER}') ORDER BY ISV.SegmentValue ;

                    --Yarn Composition

                    SELECT 
	                id = ISV.SegmentValueID
	                ,text = ISV.SegmentValue, Sequence=2
	                from {DbNames.EPYSL}..ItemSegmentValue ISV
	                INNER JOIN {DbNames.EPYSL}..ItemSegmentName ISN ON ISN.SegmentNameID = ISV.SegmentNameID
	                WHERE ISN.SegmentName In ('{ItemSegmentNameConstants.YARN_COMPOSITION}') AND ISNULL(ISV.SegmentValue,'') <> ''
	                AND ISV.SegmentValueID != 93222   ORDER BY ISV.SegmentValue;

                    --Shade Reference
                    SELECT id = YSCID, text = ShadeCode FROM {TableNames.YARN_SHADE_BOOK} ORDER BY ShadeCode;

                    --Manufacturing Line
                    SELECT distinct
					id = ISV.SegmentValueID
					,text = ISV.SegmentValue
					from {DbNames.EPYSL}..ItemSegmentValue ISV   
					LEFT JOIN  {DbNames.EPYSL}..ItemSegmentName ISN ON ISN.SegmentNameID = ISV.SegmentNameID 
					WHERE ISN.SegmentName In ('{ItemSegmentNameConstants.YARN_TYPE}') AND ISNULL(ISV.SegmentValue,'') <> ''
					ORDER BY ISV.SegmentValue;

                    --Manufacturing Process
                    SELECT distinct
                    id = ISV.SegmentValueID
                    ,text = ISV.SegmentValue
                    from {DbNames.EPYSL}..ItemSegmentValue ISV  
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentName ISN ON ISN.SegmentNameID = ISV.SegmentNameID 
                    WHERE ISN.SegmentName In ('{ItemSegmentNameConstants.YARN_MANUFACTURING_PROCESS}') AND ISNULL(ISV.SegmentValue,'') <> '' 
				    ORDER BY ISV.SegmentValue;

                    --Manufacturing Sub-process
                    SELECT distinct
	                id = ISV.SegmentValueID
	                ,text = ISV.SegmentValue
	                from {DbNames.EPYSL}..ItemSegmentValue ISV  
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentName ISN ON ISN.SegmentNameID = ISV.SegmentNameID 
	                WHERE ISN.SegmentName In ('{ItemSegmentNameConstants.YARN_MANUFACTURING_SUB_PROCESS}') AND ISNULL(ISV.SegmentValue,'') <> '' 
					ORDER BY ISV.SegmentValue;

                    --Color
                    SELECT id = SegmentValueID, text = SegmentValue
	                FROM {DbNames.EPYSL}..ItemSegmentValue a 
	                WHERE a.SegmentNameID = 131 ORDER BY SegmentValue;

                    --Color Grade
                    SELECT id = SegmentValueID, text = SegmentValue
	                FROM {DbNames.EPYSL}..ItemSegmentValue a 
	                WHERE a.SegmentNameID = 269 ORDER BY SegmentValue;

                    --Count
                    SELECT distinct
	                 id = ISV.SegmentValueID
	                ,text = ISV.SegmentValue, Sequence = 2
	                from {DbNames.EPYSL}..ItemSegmentValue ISV
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentName ISN ON ISN.SegmentNameID = ISV.SegmentNameID
	                WHERE ISN.SegmentName In ('{ItemSegmentNameConstants.YARN_COUNT}')
					ORDER BY ISV.SegmentValue;

                    -----SpinnerList
                    {CommonQueries.GetContactsByCategoryType(ContactCategoryNames.SPINNER)};

                    -----SupplierList
                    {CommonQueries.GetContactsByCategoryType(ContactCategoryNames.SUPPLIER)};";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YarnRMProperties data = new YarnRMProperties();
                data = records.Read<YarnRMProperties>().FirstOrDefault();
                data.Childs = records.Read<YarnRMPropertiesChild>().ToList();
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
                data.SpinnerList = records.Read<Select2OptionModel>().ToList();
                data.SupplierList = records.Read<Select2OptionModel>().ToList();
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
        public async Task SaveAsync1(YarnRMProperties entity)
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

                int maxChildId = 0;
                int maxChildItemId = 0;

                switch (entity.EntityState)
                {
                    case EntityState.Added:

                        entity.YRMPID = await _service.GetMaxIdAsync(TableNames.YarnRMProperties, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        maxChildId = await _service.GetMaxIdAsync(TableNames.YarnRMPropertiesChild, entity.Childs.Count(x => x.EntityState == EntityState.Added), RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

                        foreach (var item in entity.Childs)
                        {
                            item.YRMPChildID = maxChildId++;
                            item.YRMPID = entity.YRMPID;
                            item.EntityState = EntityState.Added;
                        }

                        break;

                    case EntityState.Modified:

                        maxChildId = await _service.GetMaxIdAsync(TableNames.YarnRMPropertiesChild, entity.Childs.Count(x => x.EntityState == EntityState.Added), RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

                        foreach (var item in entity.Childs)
                        {
                            if (item.EntityState == EntityState.Added)
                            {
                                item.YRMPChildID = maxChildId++;
                                item.YRMPID = entity.YRMPID;
                                item.EntityState = EntityState.Added;
                            }
                        }


                        break;

                    case EntityState.Unchanged:
                    case EntityState.Deleted:
                        entity.EntityState = EntityState.Deleted;
                        entity.Childs.SetDeleted();
                        break;

                    default:
                        break;
                }

                await _service.SaveSingleAsync(entity, transaction);
                await _service.SaveAsync(entity.Childs, transaction);

                transaction.Commit();
                transactionGmt.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                transactionGmt.Rollback();
                throw ex;
            }
            finally
            {
                _connection.Close();
                _connectionGmt.Close();
            }
        }
        private async Task<YarnRMProperties> AddAsync(YarnRMProperties entity, SqlTransaction transactionGmt)
        {
            entity.YRMPID = await _service.GetMaxIdAsync(TableNames.YarnRMProperties, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

            return entity;
        }
        public async Task<bool> CheckDuplicateValue(YarnRMProperties model)
        {
            var condition = model.YRMPID > 0 ? $" AND YRMPID!={model.YRMPID}" : "";
            var sql = $@"SELECT *
			                FROM {TableNames.YarnRMProperties} 
							WHERE FiberTypeID = {model.FiberTypeID} 
                            AND BlendTypeID = {model.BlendTypeID}
                            AND YarnTypeID = {model.YarnTypeID} 
                            AND ProgramID = {model.ProgramID} 
                            AND SubProgramID = {model.SubProgramID} 
                            AND CertificationID = {model.CertificationID} 
                            AND TechnicalParameterID = {model.TechnicalParameterID} 
                            AND YarnCompositionID = {model.YarnCompositionID} 
                            AND ShadeReferenceID = {model.ShadeReferenceID} 
                            AND ManufacturingLineID = {model.ManufacturingLineID} 
                            AND ManufacturingProcessID = {model.ManufacturingProcessID} 
                            AND ManufacturingSubProcessID = {model.ManufacturingSubProcessID} 
                            AND ColorID = {model.ColorID} 
                            AND ColorGradeID = {model.ColorGradeID} 
                            AND YarnCountID = {model.YarnCountID} 
                            " + condition;
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YarnRMProperties data = records.Read<YarnRMProperties>().FirstOrDefault();
                return data == null ? false : true;
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
    }
}
