using Dapper;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.General.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;

namespace EPYSLTEX.Core.Interfaces.Services
{
    public class FreeConceptMRService : IFreeConceptMRService
    {
        private readonly IDapperCRUDService<FreeConceptMRMaster> _service;
        private readonly SqlConnection _connection;
        private SqlTransaction transaction;
        private readonly IConceptStatusService _conceptStatusService;
        public FreeConceptMRService(IDapperCRUDService<FreeConceptMRMaster> service
            , IConceptStatusService conceptStatusService)
        {
            _service = service;
            _connection = service.Connection;
            _conceptStatusService = conceptStatusService;
        }

        public async Task<List<FreeConceptMRMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo)
        {
            string tempGuid = CommonFunction.GetNewGuid();

            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By FCMRMasterID Desc" : paginationInfo.OrderBy;
            //        string groupBy = $@"Group By FCMRMasterID,ConceptID,ConceptNo,GroupConceptNo,ConceptDate,TrialNo,Qty,Remarks, ConceptTypeID,KnittingType,Composition,
            //Construction,TechnicalName,Gsm,ConceptForName,ConceptStatus,ItemSubGroup,AddedBy,Name,MaterialRequirmentBy, ConcepTypeName,UserName";

            string groupBy = $@"";

            string sql;
            if (status == Status.Pending)
            {
                sql = $@"
                 WITH FABRIC AS (
	                SELECT M.* 
	                FROM FreeConceptMaster M
					LEFT JOIN FreeConceptMRMaster MR ON MR.ConceptID = M.ConceptID
					WHERE M.IsBDS = 0 AND M.SubGroupID = 1 AND M.ConceptNo = M.GroupConceptNo AND MR.ConceptID IS NULL
                ),
                OTHER AS (
	                SELECT M.*
	                FROM FreeConceptMaster M
					LEFT JOIN FreeConceptMRMaster MR ON MR.ConceptID = M.ConceptID
					WHERE M.IsBDS = 0 AND SubGroupID != 1 AND MR.ConceptID IS NULL  
                    AND M.ConceptNo = M.GroupConceptNo
	                AND M.GroupConceptNo NOT IN (SELECT GroupConceptNo FROM FABRIC)
                ),
                ALL_ITEM AS (
	                SELECT * FROM FABRIC
	                UNION
	                SELECT * FROM OTHER
                ),
                TechName AS
                (
	                SELECT F.GroupConceptNo, TechnicalName = STRING_AGG(T.TechnicalName, ',')
	                FROM FABRIC F
	                LEFT JOIN FabricTechnicalName T ON T.TechnicalNameId = F.TechnicalNameId
	                GROUP BY F.GroupConceptNo
                ),
                FinalList AS
                (
                    SELECT M.ConceptID, M.ConceptNo, M.ConceptDate, M.TrialNo [Re-TrialNo], M.Qty, M.Remarks, M.RevisionPending,KM.TypeName KnittingType, M.GroupConceptNo,
                    M.ConceptTypeID, T.ConcepTypeName, Composition.SegmentValue Composition, Construction.SegmentValue Construction, TN.TechnicalName, Gsm.SegmentValue Gsm,
                    F.ValueName ConceptForName, S.ValueName ConceptStatus, ISG.SubGroupName ItemSubGroup, MSC.SubClassName, M.MCSubClassID, E.EmployeeName UserName, Count(*) Over() TotalRows
                    FROM ALL_ITEM M
                    LEFT JOIN KnittingMachineType KM ON KM.TypeID = M.KnittingTypeID
                    LEFT JOIN KnittingMachineSubClass MSC ON MSC.SubClassID = M.MCSubClassID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = M.CompositionID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = M.ConstructionID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = M.GSMID
	                LEFT JOIN TechName TN ON TN.GroupConceptNo = M.GroupConceptNo
                    --LEFT JOIN FabricTechnicalName Technical ON Technical.TechnicalNameId=M.TechnicalNameId
                    LEFT JOIN {DbNames.EPYSL}..EntityTypeValue F ON F.ValueID = M.ConceptFor
                    LEFT JOIN {DbNames.EPYSL}..EntityTypeValue S ON S.ValueID = M.ConceptStatusID
                    LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID=M.SubGroupID
                    LEFT JOIN  {DbNames.EPYSL}..LoginUser L ON L.UserCode = M.AddedBy
                    LEFT Join {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
                    LEFT JOIN ConceptType T ON T.ConceptTypeID=M.ConceptTypeID
                )
                SELECT *, Count(*) Over() TotalRows FROM FinalList";

                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By GroupConceptNo Desc" : paginationInfo.OrderBy;
                groupBy = "";
            }
            else if (status == Status.Completed)
            {
                sql = $@"
                With F As 
                (
	                Select MR.FCMRMasterID,M.ConceptID,ConceptNo,M.GroupConceptNo,ConceptDate,M.TrialNo,Qty,MR.Remarks, 
                    M.ConceptTypeID,KnittingType.TypeName KnittingType,Composition.SegmentValue Composition,
                    Construction.SegmentValue Construction,Gsm.SegmentValue Gsm, F.ValueName ConceptForName,
                    S.ValueName ConceptStatus,ISG.SubGroupName ItemSubGroup,MR.AddedBy,L.Name,L.Name[MaterialRequirmentBy], 
                    T.ConcepTypeName, E.EmployeeName UserName, Isnull(YPM.YDProductionMasterID,0)YDProductionMasterID,M.TechnicalNameId,
	                M.SubGroupID
	                From FreeConceptMRMaster MR
	                INNER JOIN FreeConceptMaster M ON M.ConceptID=MR.ConceptID --AND M.RevisionNo=MR.RevisionNo
	                LEFT JOIN KnittingMachineType KnittingType ON KnittingType.TypeID=M.KnittingTypeID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID=M.CompositionID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID=M.ConstructionID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID=M.GSMID
	                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue F ON F.ValueID = M.ConceptFor
	                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue S ON S.ValueID = M.ConceptStatusID
                    LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID=M.SubGroupID
                    INNER JOIN  {DbNames.EPYSL}..LoginUser L ON L.UserCode = MR.AddedBy
                    LEFT JOIN ConceptType T ON T.ConceptTypeID=M.ConceptTypeID
                    LEFT JOIN  {DbNames.EPYSL}..LoginUser LF ON LF.UserCode = M.AddedBy
                    LEFT Join {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
                    LEFT Join YDBookingMaster YBM ON YBM.ConceptID = MR.ConceptID And YBM.ConceptID = M.ConceptID
	                LEFT Join YDProductionMaster YPM ON YPM.YDBookingMasterID = YBM.YDBookingMasterID
	                WHERE MR.IsBDS = 0 AND M.ConceptNo = M.GroupConceptNo AND MR.IsComplete=1
                    AND M.RevisionNo = MR.PreProcessRevNo
	
                ),
                TechList AS
                (
	                SELECT F.GroupConceptNo, TechnicalName = STRING_AGG(T.TechnicalName,',')
	                FROM F
	                INNER JOIN FreeConceptMaster FCM ON FCM.GroupConceptNo = F.GroupConceptNo
	                LEFT JOIN FabricTechnicalName T ON T.TechnicalNameId = FCM.TechnicalNameId
	                WHERE FCM.SubGroupID = 1
	                GROUP BY F.GroupConceptNo
                ),
                FinalList AS
                (
	                SELECT F.*, TL.TechnicalName
	                FROM F
	                LEFT JOIN TechList TL ON TL.GroupConceptNo = F.GroupConceptNo
                )
                Select * INTO #TempTable{tempGuid} From FinalList 
                SELECT *, Count(*) Over() TotalRows FROM #TempTable{tempGuid}";
            }
            else if (status == Status.PartiallyCompleted)
            {
                sql = $@"
                With F As (
	                Select MR.FCMRMasterID,M.ConceptID,ConceptNo,M.GroupConceptNo,ConceptDate,M.TrialNo,Qty,MR.Remarks, M.ConceptTypeID
	                ,KnittingType.TypeName KnittingType,Composition.SegmentValue Composition,Construction.SegmentValue Construction,Gsm.SegmentValue Gsm
	                ,F.ValueName ConceptForName,S.ValueName ConceptStatus,ISG.SubGroupName ItemSubGroup,MR.AddedBy,L.Name
	                ,L.Name[MaterialRequirmentBy], T.ConcepTypeName, E.EmployeeName UserName
	                ,M.TechnicalNameId
	                From FreeConceptMRMaster MR
	                INNER JOIN FreeConceptMaster M ON M.ConceptID=MR.ConceptID --AND M.RevisionNo=MR.RevisionNo
	                LEFT JOIN KnittingMachineType KnittingType ON KnittingType.TypeID=M.KnittingTypeID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID=M.CompositionID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID=M.ConstructionID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID=M.GSMID
	                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue F ON F.ValueID = M.ConceptFor
                    --LEFT JOIN FabricTechnicalName Technical ON Technical.TechnicalNameId=M.TechnicalNameId
	                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue S ON S.ValueID = M.ConceptStatusID
                    LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID=M.SubGroupID
                    INNER JOIN  {DbNames.EPYSL}..LoginUser L ON L.UserCode = MR.AddedBy
                    LEFT JOIN ConceptType T ON T.ConceptTypeID=M.ConceptTypeID
                    LEFT JOIN  {DbNames.EPYSL}..LoginUser LF ON LF.UserCode = M.AddedBy
                    LEFT Join {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
	                WHERE MR.IsBDS = 0 AND M.ConceptNo = M.GroupConceptNo AND MR.IsComplete=0
                    AND M.RevisionNo = MR.PreProcessRevNo
                ),
                TechList AS
                (
	                SELECT F.GroupConceptNo, TechnicalName = STRING_AGG(T.TechnicalName,',')
	                FROM F
	                INNER JOIN FreeConceptMaster FCM ON FCM.GroupConceptNo = F.GroupConceptNo
	                LEFT JOIN FabricTechnicalName T ON T.TechnicalNameId = FCM.TechnicalNameId
	                WHERE FCM.SubGroupID = 1
	                GROUP BY F.GroupConceptNo
                ),
                FinalList AS
                (
	                SELECT F.*, TL.TechnicalName
	                FROM F
	                LEFT JOIN TechList TL ON TL.GroupConceptNo = F.GroupConceptNo
                )
                Select *, Count(*) Over() TotalRows
                From FinalList ";
            }
            else if (status == Status.Revise)
            {
                sql = $@"
                With F As (
	                Select MR.FCMRMasterID,M.ConceptID,ConceptNo,M.GroupConceptNo,ConceptDate,M.TrialNo,Qty,MR.Remarks, M.ConceptTypeID
	                ,KnittingType.TypeName KnittingType,Composition.SegmentValue Composition,Construction.SegmentValue Construction,Gsm.SegmentValue Gsm
	                ,F.ValueName ConceptForName,S.ValueName ConceptStatus,ISG.SubGroupName ItemSubGroup,MR.AddedBy,L.Name,L.Name[MaterialRequirmentBy], T.ConcepTypeName, E.EmployeeName UserName
	                From FreeConceptMRMaster MR
	                INNER JOIN FreeConceptMaster M ON M.ConceptID=MR.ConceptID --AND M.RevisionNo=MR.RevisionNo
	                LEFT JOIN KnittingMachineType KnittingType ON KnittingType.TypeID=M.KnittingTypeID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID=M.CompositionID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID=M.ConstructionID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID=M.GSMID
	                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue F ON F.ValueID = M.ConceptFor
	                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue S ON S.ValueID = M.ConceptStatusID
                    LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID=M.SubGroupID
                    INNER JOIN {DbNames.EPYSL}..LoginUser L ON L.UserCode = MR.AddedBy
                    LEFT JOIN ConceptType T ON T.ConceptTypeID = M.ConceptTypeID
                    LEFT JOIN {DbNames.EPYSL}..LoginUser LF ON LF.UserCode = M.AddedBy
                    LEFT Join {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
	                WHERE MR.IsBDS = 0 AND M.ConceptNo = M.GroupConceptNo 
                    AND M.RevisionNo <> MR.PreProcessRevNo
                ),
                TechList AS
                (
	                SELECT F.GroupConceptNo, TechnicalName = STRING_AGG(T.TechnicalName,',')
	                FROM F
	                INNER JOIN FreeConceptMaster FCM ON FCM.GroupConceptNo = F.GroupConceptNo
	                LEFT JOIN FabricTechnicalName T ON T.TechnicalNameId = FCM.TechnicalNameId
	                WHERE FCM.SubGroupID = 1
	                GROUP BY F.GroupConceptNo
                ),
                FinalList AS
                (
	                SELECT F.*, TL.TechnicalName
	                FROM F
	                LEFT JOIN TechList TL ON TL.GroupConceptNo = F.GroupConceptNo
                )
                Select *, Count(*) Over() TotalRows
                From FinalList
            ";
            }
            else
            {
                sql = $@"
                With F As 
                (
	                Select MR.FCMRMasterID, M.ConceptID, ConceptNo, M.GroupConceptNo, ConceptDate, M.TrialNo, Qty, MR.Remarks, 
                    M.ConceptTypeID, KnittingType.TypeName KnittingType, Composition.SegmentValue Composition, Construction.SegmentValue Construction,
                    Gsm.SegmentValue Gsm, F.ValueName ConceptForName, S.ValueName ConceptStatus,
                    ISG.SubGroupName ItemSubGroup, MR.AddedBy, L.Name, L.Name[MaterialRequirmentBy], T.ConcepTypeName,
                    E.EmployeeName UserName, Isnull(YPM.YDProductionMasterID,0)YDProductionMasterID
	                From FreeConceptMRMaster MR
	                INNER JOIN FreeConceptMaster M ON M.ConceptID=MR.ConceptID
	                LEFT JOIN KnittingMachineType KnittingType ON KnittingType.TypeID=M.KnittingTypeID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = M.CompositionID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = M.ConstructionID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = M.GSMID
	                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue F ON F.ValueID = M.ConceptFor
	                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue S ON S.ValueID = M.ConceptStatusID
                    LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID=M.SubGroupID
                    INNER JOIN {DbNames.EPYSL}..LoginUser L ON L.UserCode = MR.AddedBy
                    LEFT JOIN ConceptType T ON T.ConceptTypeID=M.ConceptTypeID
                    LEFT JOIN {DbNames.EPYSL}..LoginUser LF ON LF.UserCode = M.AddedBy
                    LEFT Join {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
                    LEFT Join YDBookingMaster YBM ON YBM.ConceptID = MR.ConceptID And YBM.ConceptID = M.ConceptID
	                LEFT Join YDProductionMaster YPM ON YPM.YDBookingMasterID = YBM.YDBookingMasterID
                    WHERE MR.IsBDS = 0 AND M.ConceptNo = M.GroupConceptNo AND MR.IsComplete = 0
                    AND M.RevisionNo = MR.PreProcessRevNo
                ),
                TechList AS
                (
	                SELECT F.GroupConceptNo, TechnicalName = STRING_AGG(T.TechnicalName,',')
	                FROM F
	                INNER JOIN FreeConceptMaster FCM ON FCM.GroupConceptNo = F.GroupConceptNo
	                LEFT JOIN FabricTechnicalName T ON T.TechnicalNameId = FCM.TechnicalNameId
	                GROUP BY F.GroupConceptNo
                ),
                FinalList AS
                (
	                SELECT F.*, TL.TechnicalName
	                FROM F
	                LEFT JOIN TechList TL ON TL.GroupConceptNo = F.GroupConceptNo
                )
                Select *, Count(*) Over() TotalRows
                From F";
            }

            sql += $@"
                {paginationInfo.FilterBy}
                {groupBy}
                {orderBy}
                {paginationInfo.PageBy}";

            if (status == Status.Completed)
            {
                sql += $@" DROP TABLE #TempTable{tempGuid}";
            }

            return await _service.GetDataAsync<FreeConceptMRMaster>(sql);
        }

        public async Task<FreeConceptMRMaster> GetNewAsync(int conceptId)
        {
            var segmentNames = new
            {
                SegmentNames = new string[]
                {
                    ItemSegmentNameConstants.YARN_SUBPROGRAM_NEW,
                    ItemSegmentNameConstants.YARN_CERTIFICATIONS
                }
            };

            string query =
                $@"-- Master Data
                With
                M As (
	                Select *
	                From FreeConceptMaster
	                Where ConceptID = {conceptId}
                )

                Select M.ConceptID, M.ConceptNo, M.ConceptDate, M.TrialNo, M.TrialDate, M.ConceptFor, M.KnittingTypeID, M.ConstructionID,M.TechnicalNameId, M.CompositionID
	                , M.GSMID, Qty, M.ConceptStatusID, E.ValueName ConceptForName, KnittingType.TypeName KnittingType, FTN.TechnicalName
	                , Composition.SegmentValue Composition, Construction.SegmentValue Construction,FTN.TechnicalName ,Gsm.SegmentValue GSM
                From M
                INNER JOIN {DbNames.EPYSL}..EntityTypeValue E ON E.ValueID = M.ConceptFor
                LEFT JOIN KnittingMachineType KnittingType ON KnittingType.TypeID = M.KnittingTypeID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = M.CompositionID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = M.ConstructionID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = M.GSMID
                LEFT JOIN FabricTechnicalName FTN ON M.TechnicalNameId = FTN.TechnicalNameId;

                -- child colors
                ;Select CCColorID, C.ColorID, C.ColorCode, ISV.SegmentValue ColorName, FCBS.RGBOrHex, Remarks
                From FreeConceptChildColor C
                LEFT Join {DbNames.EPYSL}..FabricColorBookSetup FCBS ON FCBS.ColorID = C.ColorID
                LEFT Join {DbNames.EPYSL}..ItemSegmentValue ISV On FCBS.ColorID = ISV.SegmentValueID
                where C.ConceptID={conceptId}
                Group By CCColorID, C.ColorID, C.ColorCode, ISV.SegmentValue, FCBS.RGBOrHex, Remarks;

                -- Fabric Components
                {CommonQueries.GetEntityTypeValuesOnly(EntityTypeNameConstants.FABRIC_TYPE)};

                -- Shade book
                {CommonQueries.GetYarnShadeBooks()};

                -- Item Segments
                {CommonQueries.GetItemSegmentValuesBySegmentNamesWithSegmentName()};

                -- DayValidDuration
                { CommonQueries.GetDayValidDurations()};";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query, segmentNames);
                FreeConceptMRMaster data = await records.ReadFirstOrDefaultAsync<FreeConceptMRMaster>();
                Guard.Against.NullObject(data);
                data.ChildColors = records.Read<FreeConceptChildColor>().ToList();
                //data.Certifications = await records.ReadAsync<Select2OptionModel>();
                data.FabricComponents = await records.ReadAsync<string>();
                data.YarnShadeBooks = await records.ReadAsync<Select2OptionModel>();
                var itemSegments = await records.ReadAsync<Select2OptionModelExtended>();
                data.YarnSubProgramNews = itemSegments.Where(x => x.desc == ItemSegmentNameConstants.YARN_SUBPROGRAM_NEW);
                data.Certifications = itemSegments.Where(x => x.desc == ItemSegmentNameConstants.YARN_CERTIFICATIONS);

                data.DayValidDurations = await records.ReadAsync<Select2OptionModel>();
                data.DayValidDurations = CommonFunction.GetDayValidDurations(data.DayValidDurations, string.Join(",", data.Childs.Where(x => x.DayValidDurationId > 0).Select(x => x.DayValidDurationId).Distinct()));
                data.IsCheckDVD = true;

                return data;
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

        public async Task<FreeConceptMRMaster> GetByGroupConceptAsync(string grpConceptNo, int conceptTypeID)
        {
            var segmentNames = new
            {
                SegmentNames = new string[]
                {
                    ItemSegmentNameConstants.YARN_SUBPROGRAM_NEW,
                    ItemSegmentNameConstants.YARN_CERTIFICATIONS
                }
            };
            string query = "";
            if (conceptTypeID == 1 || conceptTypeID == 2) //Only Fabric = 1, Fabric & Other Item = 2
            {
                query =
                $@"
                Select M.GroupConceptNo, M.ConceptID, M.ConceptNo, M.ConceptDate, M.TrialNo, M.TrialDate, M.ConceptFor, M.MCSubClassID, M.KnittingTypeID,
                KM.TypeName KnittingType, M.ConstructionID, M.TechnicalNameId, M.CompositionID, M.GSMID, M.Qty, M.ConceptStatusID, M.Remarks, M.SubGroupID,
                M.ItemMasterID, M.ConceptTypeID, M.FUPartID, M.IsYD, M.MachineGauge, E.ValueName ConceptForName, SC.SubClassName MCSubClassName, FTN.TechnicalName,
                Composition.SegmentValue Composition, Construction.SegmentValue Construction ,Gsm.SegmentValue GSM
                From FreeConceptMaster M
                Inner Join KnittingMachineType KM ON KM.TypeID = KnittingTypeID
                INNER JOIN {DbNames.EPYSL}..EntityTypeValue E ON E.ValueID = M.ConceptFor
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = M.CompositionID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = M.ConstructionID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = M.GSMID
                LEFT JOIN FabricTechnicalName FTN ON M.TechnicalNameId = FTN.TechnicalNameId
                LEFT JOIN KnittingMachineSubClass SC ON SC.SubClassID = M.MCSubClassID
                where GroupConceptNo='{grpConceptNo}' AND SubGroupID=1;";
            }
            else  //Only Other Item =3
            {
                query =
                $@"
                 ;Select M.GroupConceptNo, M.ConceptID, M.ConceptNo, M.ConceptDate, M.TrialNo, M.TrialDate, M.ConceptFor, M.MCSubClassID, M.KnittingTypeID,
                KM.TypeName KnittingType, M.ConstructionID, M.TechnicalNameId, M.CompositionID, M.GSMID, M.Qty, M.ConceptStatusID, M.Remarks, M.SubGroupID,
                M.ItemMasterID, M.ConceptTypeID, M.FUPartID, M.IsYD, M.MachineGauge, E.ValueName ConceptForName, FTN.TechnicalName, Composition.SegmentValue Composition,
                Construction.SegmentValue Construction,Gsm.SegmentValue GSM, FU.PartName FUPartName, MCS.SubClassName MCSubClassName,
                M.[Length], M.[Width]                
                From FreeConceptMaster M
                LEFT Join KnittingMachineType KM ON KM.TypeID = KnittingTypeID
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue E ON E.ValueID = M.ConceptFor
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = M.CompositionID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = M.ConstructionID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = M.GSMID
                LEFT JOIN FabricTechnicalName FTN ON M.TechnicalNameId = FTN.TechnicalNameId
                INNER JOIN {DbNames.EPYSL}..FabricUsedPart FU ON FU.FUPartID = M.FUPartID
				INNER JOIN KnittingMachineSubClass MCS ON MCS.SubClassID = M.MCSubClassID
                where GroupConceptNo='{grpConceptNo}' AND SubGroupID != 1 AND GroupConceptNo= ConceptNo;";
            }
            query +=
                $@"

                ;Select M.GroupConceptNo, M.ConceptID, M.ConceptNo, M.ConceptDate, M.TrialNo, M.TrialDate, M.ConceptFor, M.MCSubClassID, M.KnittingTypeID,
                KM.TypeName KnittingType, M.ConstructionID, M.TechnicalNameId, M.CompositionID, M.GSMID, M.Qty, M.ConceptStatusID, M.Remarks, M.SubGroupID,
                M.ItemMasterID, M.ConceptTypeID, M.FUPartID, M.IsYD, M.MachineGauge, E.ValueName ConceptForName, FTN.TechnicalName, Composition.SegmentValue Composition,
                Construction.SegmentValue Construction,FTN.TechnicalName ,Gsm.SegmentValue GSM, FU.PartName FUPartName, MCS.SubClassName MCSubClassName,
                M.[Length], M.[Width]                
                From FreeConceptMaster M
                LEFT Join KnittingMachineType KM ON KM.TypeID = KnittingTypeID
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue E ON E.ValueID = M.ConceptFor
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = M.CompositionID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = M.ConstructionID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = M.GSMID
                LEFT JOIN FabricTechnicalName FTN ON M.TechnicalNameId = FTN.TechnicalNameId
                INNER JOIN {DbNames.EPYSL}..FabricUsedPart FU ON FU.FUPartID = M.FUPartID
				INNER JOIN KnittingMachineSubClass MCS ON MCS.SubClassID = M.MCSubClassID
                where GroupConceptNo='{grpConceptNo}' AND SubGroupID != 1;

                -- Childs Color
                ;SELECT C.CCColorID, C.ConceptID, C.ColorID, C.ColorCode, ISV.SegmentValue ColorName, FCBS.RGBOrHex, C.Remarks
                FROM FreeConceptChildColor C
                LEFT Join {DbNames.EPYSL}..FabricColorBookSetup FCBS ON FCBS.ColorID = C.ColorID
                LEFT Join {DbNames.EPYSL}..ItemSegmentValue ISV ON FCBS.ColorID = ISV.SegmentValueID
                WHERE ConceptID = (SELECT ConceptID FROM FreeConceptMaster FM where GroupConceptNo='{grpConceptNo}' AND FM.GroupConceptNo=FM.ConceptNo)
                GROUP BY C.CCColorID, C.ConceptID, C.ColorID, C.ColorCode, ISV.SegmentValue, FCBS.RGBOrHex, C.Remarks;

                --Item Segments
                { CommonQueries.GetCertifications()};

                -- Fabric Components
                /*{CommonQueries.GetEntityTypeValuesOnly(EntityTypeNameConstants.FABRIC_TYPE)}*/;
                  {CommonQueries.GetFabricComponents(EntityTypeNameConstants.FABRIC_TYPE)};

                -- Shade book
                {CommonQueries.GetYarnShadeBooks()};

                -- Item Segments
                /*{CommonQueries.GetItemSegmentValuesBySegmentNamesWithSegmentName()}*/;
                { CommonQueries.GetSubPrograms()}; 

                -- DayValidDuration
                { CommonQueries.GetDayValidDurations()};


                --Fiber-SubProgram-Certifications Mapping Setup
                Select * FROM {DbNames.EPYSL}..FabricComponentMappingSetup";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query, segmentNames);

                FreeConceptMRMaster data = new FreeConceptMRMaster();
                data.FabricItems = records.Read<FreeConceptMRMaster>().ToList();
                data.OtherItems = records.Read<FreeConceptMRMaster>().ToList();
                data.ChildColors = records.Read<FreeConceptChildColor>().ToList();

                if (data.FabricItems.Count() > 0)
                {
                    var obj = data.FabricItems.Find(x => x.GroupConceptNo == x.ConceptNo);
                    if (obj.IsNotNull())
                    {
                        data.GroupConceptNo = obj.GroupConceptNo;
                        data.ConceptDate = obj.ConceptDate;
                        data.ConceptID = obj.ConceptID;
                        data.ConceptTypeID = obj.ConceptTypeID;
                        data.RevisionNo = obj.RevisionNo;
                        data.Remarks = obj.Remarks;
                    }
                }
                if (data.GroupConceptNo.IsNullOrEmpty() && data.OtherItems.Count() > 0)
                {
                    var obj = data.OtherItems.Find(x => x.GroupConceptNo == x.ConceptNo);
                    if (obj.IsNotNull())
                    {
                        data.GroupConceptNo = obj.GroupConceptNo;
                        data.ConceptDate = obj.ConceptDate;
                        data.ConceptID = obj.ConceptID;
                        data.ConceptTypeID = obj.ConceptTypeID;
                        data.RevisionNo = obj.RevisionNo;
                    }
                }

                var _recvCertifications = await records.ReadAsync<Select2OptionModelExtended>();
                data.Certifications = _recvCertifications.Where(x => x.desc == ItemSegmentNameConstants.YARN_CERTIFICATIONS);
                //data.FabricComponents = await records.ReadAsync<string>();
                data.FabricComponentsNew = await records.ReadAsync<Select2OptionModel>();
                data.YarnShadeBooks = await records.ReadAsync<Select2OptionModel>();
                var itemSegments = await records.ReadAsync<Select2OptionModelExtended>();
                data.YarnSubProgramNews = itemSegments.Where(x => x.desc == ItemSegmentNameConstants.YARN_SUBPROGRAM_NEW);

                data.DayValidDurations = await records.ReadAsync<Select2OptionModel>();
                data.DayValidDurations = CommonFunction.GetDayValidDurations(data.DayValidDurations, string.Join(",", data.Childs.Where(x => x.DayValidDurationId > 0).Select(x => x.DayValidDurationId).Distinct()));
                data.IsCheckDVD = true;
                data.FabricComponentMappingSetupList = records.Read<FabricComponentMappingSetup>().ToList();

                return data;
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

        public async Task<FreeConceptMRMaster> GetDetailsAsync(int id)
        {
            string query =
                $@"-- Master Data
                Select * From FreeConceptMRMaster Where FCMRMasterID = {id}

                Select * From FreeConceptMRChild Where FCMRMasterID = {id}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                FreeConceptMRMaster data = await records.ReadFirstOrDefaultAsync<FreeConceptMRMaster>();
                Guard.Against.NullObject(data);
                data.Childs = records.Read<FreeConceptMRChild>().ToList();
                return data;
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
        public async Task<List<FreeConceptMRMaster>> GetByGroupConceptNo(string groupConceptNo)
        {
            string query =
                $@"-- Master Data
                SELECT * 
                FROM FreeConceptMRMaster FCMR
                LEFT JOIN FreeConceptMaster FCM ON FCM.ConceptID=FCMR.ConceptID
                WHERE FCM.GroupConceptNo='{groupConceptNo}'

                -- Child Data
                SELECT * 
                FROM FreeConceptMRChild FCMRC
                LEFT JOIN  FreeConceptMRMaster FCMR ON FCMR.FCMRMasterID=FCMRC.FCMRMasterID
                LEFT JOIN FreeConceptMaster FCM ON FCM.ConceptID=FCMR.ConceptID
                WHERE FCM.GroupConceptNo='{groupConceptNo}'";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                List<FreeConceptMRMaster> masters = records.Read<FreeConceptMRMaster>().ToList();
                Guard.Against.NullObject(masters);
                List<FreeConceptMRChild> childs = records.Read<FreeConceptMRChild>().ToList();
                masters.ForEach(x => x.Childs = childs.Where(y => y.FCMRMasterID == x.FCMRMasterID).ToList());
                return masters;
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
        public async Task<List<FreeConceptMRChild>> GetMRChildByBookingNo(string bookingNo)
        {
            string query =
                $@"-- Master Data
                SELECT MRC.*
                FROM FreeConceptMRChild MRC
                INNER JOIN YarnBookingChildItem_New YBCI ON YBCI.YBChildItemID = MRC.YBChildItemID
                INNER JOIN YarnBookingChild_New YBC ON YBC.YBChildID = YBCI.YBChildID
                INNER JOIN YarnBookingMaster_New YBM ON YBM.YBookingID = YBC.YBookingID
                INNER JOIN FBookingAcknowledge FBA ON FBA.BookingID = YBM.BookingID
                WHERE FBA.BookingNo = '{bookingNo}'";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                List<FreeConceptMRChild> childs = records.Read<FreeConceptMRChild>().ToList();
                Guard.Against.NullObject(childs);

                return childs;
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
        public async Task<List<FreeConceptMRChild>> GetMRChildByBookingNoWithRevision(string bookingNo)
        {
            string query =
                $@"-- Master Data
                SELECT MRC.*
                FROM FreeConceptMRChild MRC
                INNER JOIN YarnBookingChildItem_New YBCI ON YBCI.YBChildItemID = MRC.YBChildItemID
                INNER JOIN YarnBookingChild_New YBC ON YBC.YBChildID = YBCI.YBChildID
                INNER JOIN YarnBookingMaster_New YBM ON YBM.YBookingID = YBC.YBookingID
                INNER JOIN FBookingAcknowledge FBA ON FBA.BookingID = YBM.BookingID
                WHERE FBA.BookingNo = '{bookingNo}'

                SELECT MRC.*
                FROM FreeConceptMRChild MRC
                INNER JOIN YarnBookingChildItem_New_Revision YBCI ON YBCI.YBChildItemID = MRC.YBChildItemID
                INNER JOIN YarnBookingChild_New YBC ON YBC.YBChildID = YBCI.YBChildID
                INNER JOIN YarnBookingMaster_New YBM ON YBM.YBookingID = YBC.YBookingID
                INNER JOIN FBookingAcknowledge FBA ON FBA.BookingID = YBM.BookingID
                WHERE FBA.BookingNo = '{bookingNo}'";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                List<FreeConceptMRChild> childs = records.Read<FreeConceptMRChild>().ToList();
                Guard.Against.NullObject(childs);
                List<FreeConceptMRChild> childsRevision = records.Read<FreeConceptMRChild>().ToList();
                if (childsRevision.Count > 0)
                {
                    childs = childsRevision;
                }
                return childs;
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
        public async Task<FreeConceptMRMaster> GetAsync(int id)
        {
            var segmentNames = new
            {
                SegmentNames = new string[]
                {
                    ItemSegmentNameConstants.YARN_SUBPROGRAM_NEW,
                    ItemSegmentNameConstants.YARN_CERTIFICATIONS
                }
            };

            string query =
                $@"-- Master Data
                WITH
                M As (
	                Select *
	                From FreeConceptMRMaster
	                Where FCMRMasterID = {id}
                )

                Select M.FCMRMasterID, M.ConceptID, ConceptNo, ConceptDate, M.TrialNo, CM.TrialDate, M.ReqDate, CM.ConceptFor, FTN.TechnicalName, M.HasYD
	                , KnittingTypeID, CM.ConstructionID, CM.TechnicalNameId, CompositionID, GSMID, Qty,ConceptStatusID, M.Remarks, E.ValueName ConceptForName
	                , KnittingType.TypeName KnittingType, Composition.SegmentValue Composition,Construction.SegmentValue Construction,FTN.TechnicalName, Gsm.SegmentValue GSM
                from M
                LEFT JOIN FreeConceptMaster CM ON M.ConceptID = CM.ConceptID
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue E ON E.ValueID = CM.ConceptFor
                LEFT JOIN KnittingMachineType KnittingType ON KnittingType.TypeID = CM.KnittingTypeID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = CM.CompositionID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = CM.ConstructionID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = CM.GSMID
                LEFT JOIN FabricTechnicalName FTN ON CM.TechnicalNameId = FTN.TechnicalNameId;

                --Childs
				select FCC.FCMRChildID, FCC.FCMRMasterID, FCC.ItemMasterID, FCC.YarnCategory, FCC.YD, FCC.YDItem, FCC.IsPR, FCC.ReqQty,FCC.ReqCone, FCC.ShadeCode,
                IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, IM.Segment4ValueID, IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID,
                IM.Segment8ValueID, ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc,
                ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc,
                ISV8.SegmentValue Segment8ValueDesc, FCC.DayValidDurationId
                from FreeConceptMRChild FCC
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = FCC.ItemMasterID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV8 ON ISV8.SegmentValueID = IM.Segment8ValueID
                where FCC.FCMRMasterID={id};

                select CCColorID, C.ColorId, C.ColorCode, C.ConceptID, Color.SegmentValue ColorName, FCBS.RGBOrHex
                from FreeConceptChildColor C
				INNER JOIN FreeConceptMaster M ON M.ConceptID=C.ConceptID
				INNER JOIN FreeConceptMRMaster MR ON MR.ConceptID=M.ConceptID
                LEFT Join {DbNames.EPYSL}..FabricColorBookSetup FCBS ON FCBS.ColorID = C.ColorID
                LEFT Join {DbNames.EPYSL}..ItemSegmentValue Color On FCBS.ColorID = Color.SegmentValueID
                where MR.FCMRMasterID={id}
                Group By CCColorID, C.ColorId, C.ColorCode, C.ConceptID, Color.SegmentValue, FCBS.RGBOrHex;

                -- Fabric Components
                {CommonQueries.GetEntityTypeValuesOnly(EntityTypeNameConstants.FABRIC_TYPE)}

                -- Shade book
                {CommonQueries.GetYarnShadeBooks()};

                -- Item Segments
                {CommonQueries.GetItemSegmentValuesBySegmentNamesWithSegmentName()};

                -- DayValidDuration
                { CommonQueries.GetDayValidDurations()};";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query, segmentNames);
                FreeConceptMRMaster data = await records.ReadFirstOrDefaultAsync<FreeConceptMRMaster>();
                Guard.Against.NullObject(data);
                data.Childs = records.Read<FreeConceptMRChild>().ToList();
                data.ChildColors = records.Read<FreeConceptChildColor>().ToList();
                // data.Certifications = await records.ReadAsync<Select2OptionModel>();
                data.FabricComponents = await records.ReadAsync<string>();
                data.YarnShadeBooks = await records.ReadAsync<Select2OptionModel>();
                var itemSegments = await records.ReadAsync<Select2OptionModelExtended>();
                data.YarnSubProgramNews = itemSegments.Where(x => x.desc == ItemSegmentNameConstants.YARN_SUBPROGRAM_NEW);
                data.Certifications = itemSegments.Where(x => x.desc == ItemSegmentNameConstants.YARN_CERTIFICATIONS);

                data.DayValidDurations = await records.ReadAsync<Select2OptionModel>();
                data.DayValidDurations = CommonFunction.GetDayValidDurations(data.DayValidDurations, string.Join(",", data.Childs.Where(x => x.DayValidDurationId > 0).Select(x => x.DayValidDurationId).Distinct()));
                data.IsCheckDVD = data.ReqDate < CommonConstent.YarnSourcingModeImplementDate ? false : true;

                return data;
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

        public async Task<FreeConceptMRMaster> GetMultipleAsync(string grpConceptNo, int conceptTypeID)
        {
            var segmentNames = new
            {
                SegmentNames = new string[]
                {
                    ItemSegmentNameConstants.YARN_SUBPROGRAM_NEW,
                    ItemSegmentNameConstants.YARN_CERTIFICATIONS
                }
            };
            string query = "";
            if (conceptTypeID == 1 || conceptTypeID == 2) //Only Fabric = 1, Fabric & Other Item = 2
            {
                query =
                $@"
                WITH M As (
                    SELECT F.*, CM.ConceptNo, CM.GroupConceptNo, CM.ConceptDate, CM.TrialDate, CM.ConceptFor, CM.KnittingTypeID, CM.CompositionID,
                    CM.GSMID, CM.Qty, CM.ConceptStatusID, CM.ConstructionID, CM.TechnicalNameId, CM.ConceptTypeID, SC.SubClassName MCSubClassName
                    FROM FreeConceptMRMaster F
                    LEFT JOIN FreeConceptMaster CM ON F.ConceptID = CM.ConceptID
                    LEFT JOIN KnittingMachineSubClass SC ON SC.SubClassID = CM.MCSubClassID
                    WHERE GroupConceptNo='{grpConceptNo}' AND SubGroupID=1
                )
                SELECT M.FCMRMasterID, M.ConceptID, M.GroupConceptNo, M.ConceptNo, M.ConceptDate, M.TrialNo, M.TrialDate, M.ReqDate, M.ConceptFor, FTN.TechnicalName, M.HasYD
                , KnittingTypeID, M.ConstructionID, M.TechnicalNameId, CompositionID, GSMID, Qty,ConceptStatusID, M.Remarks, E.ValueName ConceptForName
                , KnittingType.TypeName KnittingType, Composition.SegmentValue Composition,Construction.SegmentValue Construction,FTN.TechnicalName, Gsm.SegmentValue GSM
                , PreProcessRevNo, RevisionNo, RevisionDate, RevisionBy, RevisionReason, M.ConceptTypeID, M.MCSubClassName
                FROM M
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue E ON E.ValueID = M.ConceptFor
                LEFT JOIN KnittingMachineType KnittingType ON KnittingType.TypeID = M.KnittingTypeID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = M.CompositionID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = M.ConstructionID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = M.GSMID
                LEFT JOIN FabricTechnicalName FTN ON M.TechnicalNameId = FTN.TechnicalNameId;
                ";
            }
            else  //Only Other Item =3
            {
                query =
                $@"
                 ;Select M.GroupConceptNo, M.ConceptID, M.ConceptNo, M.ConceptDate, M.TrialNo, M.TrialDate, M.ConceptFor, M.MCSubClassID, M.KnittingTypeID,
                KM.TypeName KnittingType, M.ConstructionID, M.TechnicalNameId, M.CompositionID, M.GSMID, M.Qty, M.ConceptStatusID, M.Remarks, M.SubGroupID,
                M.ItemMasterID, M.ConceptTypeID, M.FUPartID, M.IsYD, M.MachineGauge, E.ValueName ConceptForName, FTN.TechnicalName, Composition.SegmentValue Composition,
                Construction.SegmentValue Construction,FTN.TechnicalName ,Gsm.SegmentValue GSM, FU.PartName FUPartName, MCS.SubClassName MCSubClassName,
                M.[Length], M.[Width]                
                From FreeConceptMaster M
                LEFT Join KnittingMachineType KM ON KM.TypeID = KnittingTypeID
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue E ON E.ValueID = M.ConceptFor
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = M.CompositionID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = M.ConstructionID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = M.GSMID
                LEFT JOIN FabricTechnicalName FTN ON M.TechnicalNameId = FTN.TechnicalNameId
                INNER JOIN {DbNames.EPYSL}..FabricUsedPart FU ON FU.FUPartID = M.FUPartID
				INNER JOIN KnittingMachineSubClass MCS ON MCS.SubClassID = M.MCSubClassID
                where GroupConceptNo='{grpConceptNo}' AND SubGroupID != 1 AND GroupConceptNo= ConceptNo;";
            }
            query +=
                $@"
                WITH M As (
                SELECT F.*, CM.ConceptNo, CM.ConceptDate, CM.TrialDate, CM.ConceptFor, CM.KnittingTypeID, CM.CompositionID, CM.[Length], CM.[Width],
                CM.GSMID, CM.Qty, CM.ConceptStatusID, CM.ConstructionID, CM.TechnicalNameId, CM.FUPartID, CM.MCSubClassID, CM.MachineGauge
                FROM FreeConceptMRMaster F
                LEFT JOIN FreeConceptMaster CM ON F.ConceptID = CM.ConceptID
                WHERE GroupConceptNo='{grpConceptNo}' AND SubGroupID != 1
                )
                
                SELECT M.FCMRMasterID, M.ConceptID, M.ConceptNo, M.ConceptDate, M.TrialNo, M.TrialDate, M.ReqDate, M.ConceptFor, FTN.TechnicalName, M.HasYD
                , KnittingTypeID, M.ConstructionID, M.TechnicalNameId, CompositionID, GSMID, Qty,ConceptStatusID, M.Remarks, E.ValueName ConceptForName
                , KnittingType.TypeName KnittingType, Composition.SegmentValue Composition,Construction.SegmentValue Construction,FTN.TechnicalName, Gsm.SegmentValue GSM
                , FU.PartName FUPartName, MCS.SubClassName MCSubClassName, M.MachineGauge, PreProcessRevNo, RevisionNo, RevisionDate, RevisionBy, RevisionReason
                , M.[Length], M.[Width]                
                FROM M
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue E ON E.ValueID = M.ConceptFor
                LEFT JOIN KnittingMachineType KnittingType ON KnittingType.TypeID = M.KnittingTypeID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = M.CompositionID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = M.ConstructionID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = M.GSMID
                LEFT JOIN FabricTechnicalName FTN ON M.TechnicalNameId = FTN.TechnicalNameId
                INNER JOIN {DbNames.EPYSL}..FabricUsedPart FU ON FU.FUPartID = M.FUPartID
				INNER JOIN KnittingMachineSubClass MCS ON MCS.SubClassID = M.MCSubClassID;

                --Childs
				SELECT FCC.FCMRChildID, FCC.FCMRMasterID, CM.ConceptID, FCC.ItemMasterID, FCC.YarnCategory, FCC.YD, FCC.YDItem, FCC.IsPR, FCC.ReqQty,FCC.ReqCone, FCC.ShadeCode,
                IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, IM.Segment4ValueID, IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID,
                IM.Segment8ValueID, ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc,
                ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc,
                ISV8.SegmentValue Segment8ValueDesc, Isnull(YPM.YDProductionMasterID,0)YDProductionMasterID,
                FCC.YarnStockSetId,YSS.PhysicalCount,YSS.YarnLotNo,YSM.SampleStockQty,YSM.AdvanceStockQty,SpinnerName=SP.ShortName
                , FCC.DayValidDurationId
                FROM FreeConceptMRChild FCC
				INNER JOIN FreeConceptMRMaster FCM ON FCM.FCMRMasterID = FCC.FCMRMasterID
				INNER JOIN FreeConceptMaster CM ON CM.ConceptID = FCM.ConceptID
                LEFT Join YDBookingMaster YBM ON YBM.ConceptID = FCM.ConceptID And YBM.ConceptID = CM.ConceptID
                LEFT Join YDProductionMaster YPM ON YPM.YDBookingMasterID = YBM.YDBookingMasterID
                LEFT JOIN YarnStockSet YSS ON YSS.YarnStockSetId=FCC.YarnStockSetId
                LEFT JOIN YarnStockMaster YSM ON YSM.YarnStockSetId=FCC.YarnStockSetId
                LEFT JOIN {DbNames.EPYSL}..Contacts SP ON SP.ContactID=YSS.SpinnerId
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = FCC.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV8 ON ISV8.SegmentValueID = IM.Segment8ValueID
                WHERE CM.GroupConceptNo = '{grpConceptNo}';

                ;SELECT CCColorID, C.ColorId, C.ColorCode, C.ConceptID, Color.SegmentValue ColorName, FCBS.RGBOrHex
                from FreeConceptChildColor C
				INNER JOIN FreeConceptMaster M ON M.ConceptID=C.ConceptID
				INNER JOIN FreeConceptMRMaster MR ON MR.ConceptID=M.ConceptID
                LEFT Join {DbNames.EPYSL}..FabricColorBookSetup FCBS ON FCBS.ColorID = C.ColorID
                LEFT Join {DbNames.EPYSL}..ItemSegmentValue Color ON FCBS.ColorID = Color.SegmentValueID
                WHERE M.ConceptID IN (SELECT ConceptID FROM FreeConceptMaster WHERE GroupConceptNo='{grpConceptNo}')
                GROUP BY CCColorID, C.ColorId, C.ColorCode, C.ConceptID, Color.SegmentValue, FCBS.RGBOrHex;

                -- Fabric Components
                {CommonQueries.GetEntityTypeValuesOnly(EntityTypeNameConstants.FABRIC_TYPE)}

                -- Shade book
                {CommonQueries.GetYarnShadeBooks()};

                -- Item Segments
                {CommonQueries.GetItemSegmentValuesBySegmentNamesWithSegmentName()};";

            string query2 = $@"
                    ;With C As(Select ConceptID From FreeConceptMaster WHERE GroupConceptNo='{grpConceptNo}') 
                    Select ConceptID From (
                    SELECT a.ConceptID FROM YDBookingMaster a Inner Join C On C.ConceptID = a.ConceptID GROUP BY a.ConceptID
                    Union
                    SELECT a.ConceptID FROM KnittingPlanMaster a Inner Join C On C.ConceptID = a.ConceptID GROUP BY a.ConceptID
                    Union
                    SELECT a.ConceptID FROM YarnPRChild a Inner Join C On C.ConceptID = a.ConceptID GROUP BY a.ConceptID
                    Union
                    SELECT a.ConceptID FROM YarnRnDReqChild a Inner Join C On C.ConceptID = a.ConceptID GROUP BY a.ConceptID
                    ) C Group By ConceptID;

                    ;With C As(Select ConceptID From FreeConceptMaster WHERE GroupConceptNo='{grpConceptNo}') 
                    Select ConceptID From (
                    SELECT a.ConceptID FROM 
                    YarnRnDReqChild a 
                    Inner Join C On C.ConceptID = a.ConceptID 
                    Inner Join YarnRnDIssueMaster I On I.RnDReqMasterID = a.RnDReqMasterID 
                    GROUP BY a.ConceptID
                    ) C Group By ConceptID;

                -- DayValidDuration
                { CommonQueries.GetDayValidDurations()};";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query + query2, segmentNames);
                FreeConceptMRMaster data = new FreeConceptMRMaster();

                data.FabricItems = records.Read<FreeConceptMRMaster>().ToList();
                data.OtherItems = records.Read<FreeConceptMRMaster>().ToList();

                List<FreeConceptMRChild> childs = records.Read<FreeConceptMRChild>().ToList();
                data.FabricItems.ForEach(x =>
                {
                    x.Childs = childs.Where(y => y.FCMRMasterID == x.FCMRMasterID).ToList();
                });
                data.OtherItems.ForEach(x =>
                {
                    x.Childs = childs.Where(y => y.FCMRMasterID == x.FCMRMasterID).ToList();
                });

                if (data.FabricItems.Count() > 0)
                {
                    var obj = data.FabricItems.Find(x => x.GroupConceptNo == x.ConceptNo);
                    if (obj.IsNotNull())
                    {
                        data.GroupConceptNo = obj.GroupConceptNo;
                        data.ConceptDate = obj.ConceptDate;
                        data.ConceptID = obj.ConceptID;
                        data.ConceptTypeID = obj.ConceptTypeID;
                        data.RevisionNo = obj.RevisionNo;
                        data.Remarks = obj.Remarks;
                    }
                }
                if (data.GroupConceptNo.IsNullOrEmpty() && data.OtherItems.Count() > 0)
                {
                    var obj = data.OtherItems.Find(x => x.GroupConceptNo == x.ConceptNo);
                    if (obj.IsNotNull())
                    {
                        data.GroupConceptNo = obj.GroupConceptNo;
                        data.ConceptDate = obj.ConceptDate;
                        data.ConceptID = obj.ConceptID;
                        data.ConceptTypeID = obj.ConceptTypeID;
                        data.RevisionNo = obj.RevisionNo;
                    }
                }

                data.ChildColors = records.Read<FreeConceptChildColor>().ToList();
                data.FabricComponents = await records.ReadAsync<string>();
                data.YarnShadeBooks = await records.ReadAsync<Select2OptionModel>();
                var itemSegments = await records.ReadAsync<Select2OptionModelExtended>();
                data.YarnSubProgramNews = itemSegments.Where(x => x.desc == ItemSegmentNameConstants.YARN_SUBPROGRAM_NEW);
                data.Certifications = itemSegments.Where(x => x.desc == ItemSegmentNameConstants.YARN_CERTIFICATIONS);

                List<FreeConceptMaster> FreeConceptMasters = records.Read<FreeConceptMaster>().ToList();
                if (FreeConceptMasters.Count > 0) data.NeedRevision = true;

                List<FreeConceptMaster> RevisionedList = records.Read<FreeConceptMaster>().ToList();
                if (RevisionedList.Count > 0) data.IsUsed = true;

                data.DayValidDurations = await records.ReadAsync<Select2OptionModel>();
                data.DayValidDurations = CommonFunction.GetDayValidDurations(data.DayValidDurations, string.Join(",", data.Childs.Where(x => x.DayValidDurationId > 0).Select(x => x.DayValidDurationId).Distinct()));
               
                if(data.FabricItems.Count() > 0)
                {
                    data.IsCheckDVD = data.FabricItems.First().ReqDate < CommonConstent.YarnSourcingModeImplementDate ? false : true;
                }

                return data;
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
        public async Task<FreeConceptMRMaster> GetMultipleAsyncRevision(string grpConceptNo, int conceptTypeID)
        {
            var segmentNames = new
            {
                SegmentNames = new string[]
                {
                    ItemSegmentNameConstants.YARN_SUBPROGRAM_NEW,
                    ItemSegmentNameConstants.YARN_CERTIFICATIONS
                }
            };
            string query = "";
            if (conceptTypeID == 1 || conceptTypeID == 2) //Only Fabric = 1, Fabric & Other Item = 2
            {
                query =
                $@" WITH M As (
					SELECT FCMRMasterID = ISNULL(F.FCMRMasterID,0), CM.ConceptID, CM.ConceptNo, CM.GroupConceptNo, CM.ConceptDate, CM.TrialDate, CM.ConceptFor, CM.KnittingTypeID, CM.CompositionID,
					CM.GSMID, CM.Qty, CM.ConceptStatusID, CM.ConstructionID, CM.TechnicalNameId, CM.SubGroupID, MCS.SubClassName MCSubClassName, CM.TrialNo, HasYD = ISNULL(F.HasYD,0),
					F.ReqDate, F.Remarks, PreProcessRevNo = ISNULL(CM.RevisionNo,0), RevisionNo = ISNULL(F.RevisionNo,0), F.RevisionDate, CM.RevisionBy, CM.RevisionReason
					FROM FreeConceptMaster CM
					LEFT JOIN FreeConceptMRMaster F ON F.ConceptID = CM.ConceptID
					LEFT JOIN KnittingMachineSubClass MCS ON MCS.SubClassID = CM.MCSubClassID
					WHERE GroupConceptNo='{grpConceptNo}' AND SubGroupID = 1
                )
                SELECT M.FCMRMasterID, M.ConceptID, M.GroupConceptNo, M.ConceptNo, M.ConceptDate, M.TrialNo, M.TrialDate, M.ReqDate, M.ConceptFor, FTN.TechnicalName, M.HasYD
                , KnittingTypeID, M.ConstructionID, M.TechnicalNameId, CompositionID, GSMID, Qty,ConceptStatusID, M.Remarks, E.ValueName ConceptForName
                , KnittingType.TypeName KnittingType, Composition.SegmentValue Composition,Construction.SegmentValue Construction,FTN.TechnicalName, Gsm.SegmentValue GSM
                , PreProcessRevNo, RevisionNo, RevisionDate, RevisionBy, RevisionReason, M.SubGroupID, M.MCSubClassName
                FROM M
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue E ON E.ValueID = M.ConceptFor
                LEFT JOIN KnittingMachineType KnittingType ON KnittingType.TypeID = M.KnittingTypeID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = M.CompositionID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = M.ConstructionID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = M.GSMID
                LEFT JOIN FabricTechnicalName FTN ON M.TechnicalNameId = FTN.TechnicalNameId;;
                ";
            }
            else  //Only Other Item =3
            {
                query =
                $@"
                 ;Select M.GroupConceptNo, M.ConceptID, M.ConceptNo, M.ConceptDate, M.TrialNo, M.TrialDate, M.ConceptFor, M.MCSubClassID, M.KnittingTypeID,
                KM.TypeName KnittingType, M.ConstructionID, M.TechnicalNameId, M.CompositionID, M.GSMID, M.Qty, M.ConceptStatusID, M.Remarks, M.SubGroupID,
                M.ItemMasterID, M.ConceptTypeID, M.FUPartID, M.IsYD, M.MachineGauge, E.ValueName ConceptForName, FTN.TechnicalName, Composition.SegmentValue Composition,
                Construction.SegmentValue Construction,FTN.TechnicalName ,Gsm.SegmentValue GSM, FU.PartName FUPartName, MCS.SubClassName MCSubClassName,
                M.[Length], M.[Width]                
                From FreeConceptMaster M
                LEFT Join KnittingMachineType KM ON KM.TypeID = KnittingTypeID
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue E ON E.ValueID = M.ConceptFor
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = M.CompositionID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = M.ConstructionID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = M.GSMID
                LEFT JOIN FabricTechnicalName FTN ON M.TechnicalNameId = FTN.TechnicalNameId
                INNER JOIN {DbNames.EPYSL}..FabricUsedPart FU ON FU.FUPartID = M.FUPartID
				INNER JOIN KnittingMachineSubClass MCS ON MCS.SubClassID = M.MCSubClassID
                where GroupConceptNo='{grpConceptNo}' AND SubGroupID != 1 AND GroupConceptNo= ConceptNo;";
            }
            query +=
                $@"
                WITH M As (
                SELECT 
                F.FCMRMasterID,F.TrialNo,F.ReqDate,F.HasYD,F.Remarks,F.PreProcessRevNo, F.RevisionNo,F.RevisionDate, F.RevisionBy, F.RevisionReason, 
                CM.ConceptID,CM.ConceptNo, CM.ConceptDate, CM.TrialDate, CM.ConceptFor, CM.KnittingTypeID, CM.CompositionID, CM.[Length], CM.[Width],
                CM.GSMID, CM.Qty, CM.ConceptStatusID, CM.ConstructionID, CM.TechnicalNameId, CM.FUPartID, CM.MCSubClassID, CM.MachineGauge
                FROM FreeConceptMaster CM 
                LEFT JOIN  FreeConceptMRMaster F ON F.ConceptID = CM.ConceptID
                WHERE GroupConceptNo='{grpConceptNo}' AND CM.IsBDS = 0 AND SubGroupID != 1
                )
                SELECT M.FCMRMasterID, M.ConceptID, M.ConceptNo, M.ConceptDate, M.TrialNo, M.TrialDate, M.ReqDate, M.ConceptFor, FTN.TechnicalName, M.HasYD
                , KnittingTypeID, M.ConstructionID, M.TechnicalNameId, CompositionID, GSMID, Qty,ConceptStatusID, M.Remarks, E.ValueName ConceptForName
                , KnittingType.TypeName KnittingType, Composition.SegmentValue Composition,Construction.SegmentValue Construction,FTN.TechnicalName, Gsm.SegmentValue GSM
                , FU.PartName FUPartName, MCS.SubClassName MCSubClassName, M.MachineGauge, PreProcessRevNo, RevisionNo, RevisionDate, RevisionBy, RevisionReason
                , M.[Length], M.[Width]                
                FROM M
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue E ON E.ValueID = M.ConceptFor
                LEFT JOIN KnittingMachineType KnittingType ON KnittingType.TypeID = M.KnittingTypeID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = M.CompositionID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = M.ConstructionID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = M.GSMID
                LEFT JOIN FabricTechnicalName FTN ON M.TechnicalNameId = FTN.TechnicalNameId
                INNER JOIN {DbNames.EPYSL}..FabricUsedPart FU ON FU.FUPartID = M.FUPartID
				INNER JOIN KnittingMachineSubClass MCS ON MCS.SubClassID = M.MCSubClassID;

                --Childs
				SELECT FCC.FCMRChildID, FCC.FCMRMasterID, CM.ConceptID, FCC.ItemMasterID, FCC.YarnCategory, FCC.YD, FCC.YDItem, FCC.IsPR, FCC.ReqQty,FCC.ReqCone, FCC.ShadeCode,
                IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, IM.Segment4ValueID, IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID,
                IM.Segment8ValueID, ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc,
                ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc,
                ISV8.SegmentValue Segment8ValueDesc, FCC.DayValidDurationId
                FROM FreeConceptMRChild FCC
				INNER JOIN FreeConceptMRMaster FCM ON FCM.FCMRMasterID = FCC.FCMRMasterID
				INNER JOIN FreeConceptMaster CM ON CM.ConceptID = FCM.ConceptID
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = FCC.ItemMasterID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV8 ON ISV8.SegmentValueID = IM.Segment8ValueID
                WHERE CM.GroupConceptNo = '{grpConceptNo}';

                ;SELECT CCColorID, C.ColorId, C.ColorCode, C.ConceptID, Color.SegmentValue ColorName, FCBS.RGBOrHex
                from FreeConceptChildColor C
				INNER JOIN FreeConceptMaster M ON M.ConceptID=C.ConceptID
				INNER JOIN FreeConceptMRMaster MR ON MR.ConceptID=M.ConceptID
                LEFT Join {DbNames.EPYSL}..FabricColorBookSetup FCBS ON FCBS.ColorID = C.ColorID
                LEFT Join {DbNames.EPYSL}..ItemSegmentValue Color ON FCBS.ColorID = Color.SegmentValueID
                WHERE M.ConceptID IN (SELECT ConceptID FROM FreeConceptMaster WHERE GroupConceptNo='{grpConceptNo}')
                GROUP BY CCColorID, C.ColorId, C.ColorCode, C.ConceptID, Color.SegmentValue, FCBS.RGBOrHex;

                -- Fabric Components
                {CommonQueries.GetEntityTypeValuesOnly(EntityTypeNameConstants.FABRIC_TYPE)}

                -- Shade book
                {CommonQueries.GetYarnShadeBooks()};

                -- Item Segments
                {CommonQueries.GetItemSegmentValuesBySegmentNamesWithSegmentName()};";

            string query2 = $@"

                    ;Select F.ConceptID From FreeConceptMRMaster F
					Inner JOIN FreeConceptMaster CM ON F.ConceptID = CM.ConceptID
					WHERE GroupConceptNo='{grpConceptNo}'

                    ;With C As(Select ConceptID From FreeConceptMaster WHERE GroupConceptNo='{grpConceptNo}') 
                    Select ConceptID From (
                    SELECT a.ConceptID FROM 
                    YarnRnDReqChild a 
                    Inner Join C On C.ConceptID = a.ConceptID 
                    Inner Join YarnRnDIssueMaster I On I.RnDReqMasterID = a.RnDReqMasterID 
                    GROUP BY a.ConceptID
                    ) C Group By ConceptID;

                -- DayValidDuration
                { CommonQueries.GetDayValidDurations()};";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query + query2, segmentNames);
                FreeConceptMRMaster data = new FreeConceptMRMaster();

                data.FabricItems = records.Read<FreeConceptMRMaster>().ToList();
                data.OtherItems = records.Read<FreeConceptMRMaster>().ToList();

                List<FreeConceptMRChild> childs = records.Read<FreeConceptMRChild>().ToList();
                data.FabricItems.ForEach(x =>
                {
                    x.Childs = childs.Where(y => y.FCMRMasterID == x.FCMRMasterID).ToList();
                });
                data.OtherItems.ForEach(x =>
                {
                    x.Childs = childs.Where(y => y.FCMRMasterID == x.FCMRMasterID).ToList();
                });
                if (data.FabricItems.Count() > 0)
                {
                    var obj = data.FabricItems.Find(x => x.GroupConceptNo == x.ConceptNo);
                    if (obj.IsNotNull())
                    {
                        data.GroupConceptNo = obj.GroupConceptNo;
                        data.ConceptDate = obj.ConceptDate;
                        data.ConceptID = obj.ConceptID;
                        data.ConceptTypeID = obj.ConceptTypeID;
                        data.RevisionNo = obj.RevisionNo;
                        data.Remarks = obj.Remarks;
                    }
                }
                if (data.GroupConceptNo.IsNullOrEmpty() && data.OtherItems.Count() > 0)
                {
                    var obj = data.OtherItems.Find(x => x.GroupConceptNo == x.ConceptNo);
                    if (obj.IsNotNull())
                    {
                        data.GroupConceptNo = obj.GroupConceptNo;
                        data.ConceptDate = obj.ConceptDate;
                        data.ConceptID = obj.ConceptID;
                        data.ConceptTypeID = obj.ConceptTypeID;
                        data.RevisionNo = obj.RevisionNo;
                    }
                }

                data.ChildColors = records.Read<FreeConceptChildColor>().ToList();
                data.FabricComponents = await records.ReadAsync<string>();
                data.YarnShadeBooks = await records.ReadAsync<Select2OptionModel>();
                var itemSegments = await records.ReadAsync<Select2OptionModelExtended>();
                data.YarnSubProgramNews = itemSegments.Where(x => x.desc == ItemSegmentNameConstants.YARN_SUBPROGRAM_NEW);
                data.Certifications = itemSegments.Where(x => x.desc == ItemSegmentNameConstants.YARN_CERTIFICATIONS);

                List<FreeConceptMaster> FreeConceptMasters = records.Read<FreeConceptMaster>().ToList();
                if (FreeConceptMasters.Count > 0) data.NeedRevision = true;

                List<FreeConceptMaster> RevisionedList = records.Read<FreeConceptMaster>().ToList();
                if (RevisionedList.Count > 0) data.IsUsed = true;

                data.DayValidDurations = await records.ReadAsync<Select2OptionModel>();
                data.DayValidDurations = CommonFunction.GetDayValidDurations(data.DayValidDurations, string.Join(",", data.Childs.Where(x => x.DayValidDurationId > 0).Select(x => x.DayValidDurationId).Distinct()));
                
                if (data.FabricItems.Count() > 0)
                {
                    data.IsCheckDVD = data.FabricItems.First().ReqDate < CommonConstent.YarnSourcingModeImplementDate ? false : true;
                }

                return data;
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

        public async Task<bool> ExistsAsync(int conceptID, int trialNo)
        {
            string sql = $@"Select * From FreeConceptMRMaster Where ConceptID = {conceptID} And TrialNo = {trialNo}";
            FreeConceptMRMaster record = await _service.GetFirstOrDefaultAsync<FreeConceptMRMaster>(sql);
            return record != null;
        }
        public async Task<FreeConceptMRMaster> GetMRByConceptNo(string conceptNo)
        {
            string sql = $@"SELECT MR.* 
                            FROM FreeConceptMRMaster MR
                            INNER JOIN FreeConceptMaster FCM ON FCM.ConceptID = MR.ConceptID
                            WHERE FCM.ConceptNo = '{conceptNo}'
                        ";
            FreeConceptMRMaster mrMaster = await _service.GetFirstOrDefaultAsync<FreeConceptMRMaster>(sql);
            return mrMaster;
        }

        public async Task SaveAsync(FreeConceptMRMaster entity, int userId)
        {
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                var freeConceptStatusList = await _conceptStatusService.GetByCPSIDs("1,2", entity.ConceptID.ToString());

                bool isPR = false;
                bool isYD = false;

                var objList = entity.Childs.Where(x => x.IsPR == true).ToList();
                if (objList != null && objList.Count() > 0) isPR = true;

                objList = entity.Childs.Where(x => x.YD == true).ToList();
                if (objList != null && objList.Count() > 0) isYD = true;

                int maxChildId = 0;
                switch (entity.EntityState)
                {
                    case EntityState.Added:
                        entity.FCMRMasterID = await _service.GetMaxIdAsync(TableNames.RND_FREE_CONCEPT_MR_MASTER);

                        maxChildId = await _service.GetMaxIdAsync(TableNames.RND_FREE_CONCEPT_MR_CHILD, entity.Childs.Count);
                        foreach (var item in entity.Childs)
                        {
                            item.FCMRChildID = maxChildId++;
                            item.FCMRMasterID = entity.FCMRMasterID;
                        }
                        break;

                    case EntityState.Modified:
                        var addedChilds = entity.Childs.FindAll(x => x.EntityState == EntityState.Added);
                        maxChildId = await _service.GetMaxIdAsync(TableNames.RND_FREE_CONCEPT_MR_CHILD, addedChilds.Count);

                        foreach (var item in addedChilds)
                        {
                            item.FCMRChildID = maxChildId++;
                            item.FCMRMasterID = entity.FCMRMasterID;
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

                entity.ConceptStatusList = this.GetConceptStatusList(entity.ConceptID, freeConceptStatusList, isPR, isYD);

                await _service.SaveSingleAsync(entity, transaction);
                await _service.SaveAsync(entity.Childs, transaction);
                foreach (FreeConceptMRChild item in entity.Childs)
                {
                    await _connection.ExecuteAsync("sp_Validation_FreeConceptMRChild", new { EntityState = item.EntityState, UserId = userId, PrimaryKeyId = item.FCMRChildID }, transaction, 30, CommandType.StoredProcedure);
                   
                }
                await _service.SaveAsync(entity.ConceptStatusList, transaction);
                transaction.Commit();
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                throw ex;
            }
            finally
            {
                if (transaction != null) transaction.Dispose();
                _connection.Close();
            }
        }

        public async Task<List<FreeConceptMRMaster>> GetMultiDetailsAsync(string grpConceptNo)
        {
            string query =
                $@"-- Master Data
                Select MR.*, FC.SubGroupID, FreeConceptRevisionNo = FC.RevisionNo
                From FreeConceptMRMaster MR
                INNER JOIN FreeConceptMaster FC ON FC.ConceptID = MR.ConceptID
                Where FC.GroupConceptNo = '{grpConceptNo}';

                ;Select MRC.*
                From FreeConceptMRChild MRC
                INNER JOIN FreeConceptMRMaster MR ON MR.FCMRMasterID = MRC.FCMRMasterID
                INNER JOIN FreeConceptMaster FC ON FC.ConceptID = MR.ConceptID
                Where FC.GroupConceptNo = '{grpConceptNo}'";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                List<FreeConceptMRMaster> datas = records.Read<FreeConceptMRMaster>().ToList();
                List<FreeConceptMRChild> childs = records.Read<FreeConceptMRChild>().ToList();
                datas.ForEach(data =>
                {
                    data.Childs = childs.Where(x => x.FCMRMasterID == data.FCMRMasterID).ToList();
                });
                return datas;
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

        public async Task SaveMultipleAsync(List<FreeConceptMRMaster> entities, EntityState entityState)
        {
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                var conceptIds = string.Join(",", entities.Select(x => x.ConceptID).Distinct());
                var freeConceptStatusList = await _conceptStatusService.GetByCPSIDs("1,2", conceptIds);

                switch (entityState)
                {
                    case EntityState.Added:
                        entities = await AddManyAsync(entities, freeConceptStatusList);
                        break;

                    case EntityState.Modified:
                        entities = UpdateMany(entities, freeConceptStatusList);
                        break;

                    //case EntityState.Unchanged:
                    //case EntityState.Deleted:
                    //    entity.EntityState = EntityState.Deleted;
                    //    entity.Childs.SetDeleted();
                    //    break;

                    default:
                        break;
                }

                await _service.SaveAsync(entities, transaction);
                List<FreeConceptMRChild> childs = new List<FreeConceptMRChild>();
                List<ConceptStatus> conceptStatusList = new List<ConceptStatus>();
                entities.ForEach(entity =>
                {
                    childs.AddRange(entity.Childs);
                    conceptStatusList.AddRange(entity.ConceptStatusList);
                });
                await _service.SaveAsync(childs, transaction);
                await _service.SaveAsync(conceptStatusList, transaction);
                transaction.Commit();
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                throw ex;
            }
            finally
            {
                if (transaction != null) transaction.Dispose();
                _connection.Close();
            }
        }

        private async Task<List<FreeConceptMRMaster>> AddManyAsync(List<FreeConceptMRMaster> entities, List<ConceptStatus> freeConceptStatusList)
        {
            int fcMRMasterID = await _service.GetMaxIdAsync(TableNames.RND_FREE_CONCEPT_MR_MASTER, entities.Count);
            int maxChildId = await _service.GetMaxIdAsync(TableNames.RND_FREE_CONCEPT_MR_CHILD, entities.Sum(x => x.Childs.Count));

            entities.ToList().ForEach(entity =>
            {
                entity.FCMRMasterID = fcMRMasterID++;
                entity.EntityState = EntityState.Added;
                entity.ConceptStatusList = new List<ConceptStatus>();

                bool isPR = false;
                bool isYD = false;

                var objList = entity.Childs.Where(x => x.IsPR == true).ToList();
                if (objList != null && objList.Count() > 0) isPR = true;

                objList = entity.Childs.Where(x => x.YD == true).ToList();
                if (objList != null && objList.Count() > 0) isYD = true;

                foreach (FreeConceptMRChild item in entity.Childs)
                {
                    item.FCMRChildID = maxChildId++;
                    item.FCMRMasterID = entity.FCMRMasterID;
                }
                entity.ConceptStatusList = this.GetConceptStatusList(entity.ConceptID, freeConceptStatusList, isPR, isYD);
            });

            return entities;
        }

        private async Task<List<FreeConceptMRMaster>> UpdateMany(List<FreeConceptMRMaster> entities, List<ConceptStatus> freeConceptStatusList)
        {
            int fcMRMasterID = await _service.GetMaxIdAsync(TableNames.RND_FREE_CONCEPT_MR_MASTER, entities.Where(x => x.EntityState == EntityState.Added).Count());
            int maxChildId = await _service.GetMaxIdAsync(TableNames.RND_FREE_CONCEPT_MR_CHILD, entities.Sum(x => x.Childs.Where(y => y.EntityState == EntityState.Added).Count()));

            entities.ToList().ForEach(entity =>
            {
                bool isPR = false;
                bool isYD = false;

                var objList = entity.Childs.Where(x => x.IsPR == true).ToList();
                if (objList != null && objList.Count() > 0) isPR = true;

                objList = entity.Childs.Where(x => x.YD == true).ToList();
                if (objList != null && objList.Count() > 0) isYD = true;

                switch (entity.EntityState)
                {
                    case EntityState.Added:
                        entity.FCMRMasterID = fcMRMasterID++;
                        foreach (FreeConceptMRChild item in entity.Childs)
                        {
                            item.FCMRChildID = maxChildId++;
                            item.FCMRMasterID = entity.FCMRMasterID;
                        }
                        break;

                    case EntityState.Modified:
                        List<FreeConceptMRChild> addedChilds = entity.Childs.FindAll(x => x.EntityState == EntityState.Added);
                        foreach (FreeConceptMRChild item in addedChilds)
                        {
                            item.FCMRChildID = maxChildId++;
                            item.FCMRMasterID = entity.FCMRMasterID;
                        }
                        break;

                    case EntityState.Deleted:
                        entity.EntityState = EntityState.Deleted;
                        entity.Childs.SetDeleted();
                        break;

                    default:
                        break;
                }
                entity.ConceptStatusList = this.GetConceptStatusList(entity.ConceptID, freeConceptStatusList, isPR, isYD);
            });

            return entities;
        }

        public async Task ReviseAsync(List<FreeConceptMRMaster> entities, string grpConceptNo, int userId, string fcmrChildIds)
        {
            try
            {
                //await _service.ExecuteAsync("spBackupFreeConceptMR_Full", new { ConceptNo = grpConceptNo, UserId = userId }, 30, CommandType.StoredProcedure);

                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                await _connection.ExecuteAsync("spBackupFreeConceptMR_Full", new { ConceptNo = grpConceptNo, UserId = userId }, transaction, 30, CommandType.StoredProcedure);

                var conceptIds = string.Join(",", entities.Select(x => x.ConceptID).Distinct());
                var freeConceptStatusList = await _conceptStatusService.GetByCPSIDs("1,2", conceptIds);

                entities = UpdateMany(entities, freeConceptStatusList);

                await _service.SaveAsync(entities, transaction);
                List<FreeConceptMRChild> childs = new List<FreeConceptMRChild>();
                List<ConceptStatus> conceptStatusList = new List<ConceptStatus>();
                entities.ForEach(entity =>
                {
                    childs.AddRange(entity.Childs);
                    conceptStatusList.AddRange(entity.ConceptStatusList);
                });
                await _service.SaveAsync(childs, transaction);
                foreach (FreeConceptMRChild item in childs)
                {
                    await _service.ValidationSingleAsync(item, transaction, "sp_Validation_FreeConceptMRChild", item.EntityState, userId, item.FCMRChildID);
                }
                await _service.SaveAsync(conceptStatusList, transaction);
                if (!fcmrChildIds.IsNullOrEmpty())
                {
                    var query = $@"UPDATE PR SET PR.NeedRevision = 1
                            FROM YarnPRMaster PR
                            INNER JOIN YarnPRChild PC ON PC.YarnPRMasterID = PR.YarnPRMasterID
                            WHERE PC.FCMRChildID IN ({fcmrChildIds});";
                    await _connection.ExecuteAsync(query, null, transaction);
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                throw ex;
            }
            finally
            {
                if (transaction != null) transaction.Dispose();
                _connection.Close();
            }
        }
        private List<ConceptStatus> GetConceptStatusList(int conceptID, List<ConceptStatus> freeConceptStatusList, bool isPR, bool isYD)
        {
            var conceptWiseFreeConceptStatusList = freeConceptStatusList.Where(x => x.ConceptID == conceptID).ToList();
            var finalList = new List<ConceptStatus>();
            conceptWiseFreeConceptStatusList.ForEach(x =>
            {
                bool isApplicable = x.IsApplicable;
                if (x.CPSID == 1) isApplicable = isPR;
                else if (x.CPSID == 2) isApplicable = isYD;

                finalList.Add(new ConceptStatus
                {
                    FCSID = x.FCSID,
                    ConceptID = conceptID,
                    CPSID = x.CPSID,
                    IsApplicable = isApplicable,
                    SeqNo = x.SeqNo,
                    Status = x.Status,
                    Remarks = x.Remarks,
                    EntityState = EntityState.Modified
                });
            });
            return finalList;
        }
    }
}