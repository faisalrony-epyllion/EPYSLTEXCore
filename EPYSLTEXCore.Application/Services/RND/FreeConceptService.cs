using Dapper;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Application.Interfaces.Repositories;
using EPYSLTEXCore.Application.Interfaces.RND;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Knitting;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace EPYSLTEXCore.Application.Services.RND
{
    public class FreeConceptService : IFreeConceptService
    {
        
        private readonly IDapperCRUDService<FreeConceptMaster> _service;
        private readonly SqlConnection _connection;
        private SqlTransaction transaction = null;

        public FreeConceptService(
             IDapperCRUDService<FreeConceptMaster> service)
        {
            
            _service = service;
            _connection = service.Connection;
        }

        public async Task<List<FreeConceptMaster>> GetPagedAsync(FreeConceptStatus status, PaginationInfo paginationInfo)
        {
            string statusCondition = "",
                   statusPropCondition = ",LiveStatus=S.ValueName ,StatusRemarks=M.StatusRemarks",
                   fromString = "ALL_ITEM M",
                   pendingStatusJoin = "",
                   pendingQuery = "",
                   whereCondition = "",
                   prAndydCondition = "";

            if (status == FreeConceptStatus.Dropped)
            {
                statusCondition = " AND ETV.ValueName = 'Drop'";
            }
            else if (status == FreeConceptStatus.Preserved)
            {
                statusCondition = " AND ETV.ValueName = 'Future Preservation'";
            }
            else if (status == FreeConceptStatus.Live)
            {
                statusCondition = " AND ETV.ValueName LIKE '%Live%'"; //" AND FC.LiveStatus LIKE '%Live%'";
            }
            else if (status.ToString().LastIndexOf("Pending") > 0)
            {
                var conceptPendingStatusList = await this.GetPendingStatus();
                var cpsObj = conceptPendingStatusList.Find(x => x.StatusName == this.GetStatusName(status));
                whereCondition = $@" WHERE F.CPSID={cpsObj.CPSID} ";
                statusPropCondition = ",ISNULL(FCL.CPSID,0) CPSID, FCL.[Status] ,LiveStatus=CPS.StatusName, StatusRemarks=(CASE WHEN FCL.Remarks = 'Done' THEN '-' ELSE FCL.Remarks END)";
                pendingStatusJoin = $@" Inner JOIN FCL ON FCL.ConceptID = M.ConceptID Inner JOIN ConceptPendingStatus_HK CPS ON CPS.CPSID = FCL.CPSID ";
                pendingQuery = $@" FCL AS(
						SELECT A.*
						FROM (SELECT FC.*
						FROM FreeConceptStatus FC
						WHERE CPSID = {cpsObj.CPSID} AND Status = 0 AND IsApplicable=1) A
						INNER JOIN FreeConceptStatus B ON B.ConceptID = A.ConceptID And B.SeqNo = Case When A.SeqNo = 1 Then 1 Else (A.SeqNo - 1) End
                        WHERE (B.Status = Case When A.SeqNo = 1 Then 0 Else 1 End AND B.IsApplicable=1) or (B.Status = 0 AND B.IsApplicable=0)
				    ),";
                paginationInfo.FilterBy = paginationInfo.FilterBy.Replace("Where", " AND ");
            }

            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By GroupConceptNo Desc" : paginationInfo.OrderBy;

            string sql = $@"WITH FABRIC AS (
	                            SELECT FC.GroupConceptNo,FC.ConceptTypeID, ConceptDate = MIN(FC.ConceptDate), TechnicalName = STRING_AGG(T.TechnicalName,', '), 
	                            FC.ConceptStatusID, FC.AddedBy, FC.StatusRemarks, FC.SubGroupID
	                            FROM FreeConceptMaster FC
	                            INNER JOIN {DbNames.EPYSL}..EntityTypeValue ETV on ETV.ValueID = FC.ConceptStatusID
	                            LEFT JOIN FabricTechnicalName T ON T.TechnicalNameId = FC.TechnicalNameId
	                            WHERE FC.IsBDS = 0 AND FC.SubGroupID = 1 {statusCondition}
	                            GROUP BY FC.GroupConceptNo, FC.ConceptTypeID, FC.ConceptStatusID, FC.AddedBy, FC.StatusRemarks, FC.SubGroupID
                            ),
                            OTHER AS (
	                            SELECT FC.GroupConceptNo, FC.ConceptTypeID, ConceptDate = MIN(FC.ConceptDate),TechnicalName = STRING_AGG(T.TechnicalName,', '),
	                            FC.ConceptStatusID, FC.AddedBy, FC.StatusRemarks, SubGroupID = MAX(FC.SubGroupID)
	                            FROM FreeConceptMaster FC
	                            LEFT JOIN FabricTechnicalName T ON T.TechnicalNameId = FC.TechnicalNameId
	                            WHERE FC.IsBDS = 0 AND FC.SubGroupID <> 1 AND FC.GroupConceptNo NOT IN (SELECT GroupConceptNo FROM FABRIC) 
	                            GROUP BY FC.GroupConceptNo, FC.ConceptTypeID,
	                            FC.ConceptStatusID, FC.AddedBy, FC.StatusRemarks
                            ),
                            ALL_ITEM AS (
	                            SELECT * FROM FABRIC
	                            UNION
	                            SELECT * FROM OTHER
                            ),
                            {prAndydCondition}
                            {pendingQuery}
                            F AS (
	                            SELECT M.ConceptDate, M.GroupConceptNo,
	                            M.ConceptTypeID, T.ConcepTypeName, M.TechnicalName,
	                            S.ValueName ConceptStatus, ISG.SubGroupName ItemSubGroup, 
	                            E.EmployeeName UserName, LiveStatus = S.ValueName, StatusRemarks = M.StatusRemarks
	                            FROM {fromString}
	                            LEFT JOIN {DbNames.EPYSL}..EntityTypeValue S ON S.ValueID = M.ConceptStatusID
	                            LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID=M.SubGroupID
	                            LEFT JOIN {DbNames.EPYSL}..LoginUser L ON L.UserCode = M.AddedBy
	                            LEFT Join {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
	                            LEFT JOIN ConceptType T ON T.ConceptTypeID = M.ConceptTypeID
	                            {pendingStatusJoin}
                            )
                            SELECT F.*, Count(*) Over() TotalRows
                            FROM F {whereCondition} ";
            /*

            var sql = $@"
                 WITH FABRIC AS (
	                SELECT FC.*,ETV.ValueName
	                FROM FreeConceptMaster FC
					INNER JOIN {DbNames.EPYSL}..EntityTypeValue ETV on ETV.ValueID = FC.ConceptStatusID
					WHERE IsBDS = 0 AND SubGroupID = 1 AND FC.ConceptNo = FC.GroupConceptNo {statusCondition}
                ),
                OTHER AS (
	                SELECT FC.*,ETV.ValueName
	                FROM FreeConceptMaster FC
					INNER JOIN {DbNames.EPYSL}..EntityTypeValue ETV on ETV.ValueID = FC.ConceptStatusID
					WHERE IsBDS = 0 AND SubGroupID <> 1 AND FC.ConceptNo = FC.GroupConceptNo {statusCondition}
	                AND GroupConceptNo NOT IN (SELECT GroupConceptNo FROM FABRIC)
                ),
                ALL_ITEM AS (
	                SELECT * FROM FABRIC
	                UNION
	                SELECT * FROM OTHER
                ),
                {prAndydCondition}
                {pendingQuery}
                F AS (
                SELECT M.ConceptID, M.ConceptNo, M.ConceptDate, M.TrialNo [Re-TrialNo], M.Qty, M.Remarks, M.RevisionPending,KM.TypeName KnittingType, M.GroupConceptNo,
                M.ConceptTypeID, T.ConcepTypeName, Composition.SegmentValue Composition, Construction.SegmentValue Construction, Technical.TechnicalName, Gsm.SegmentValue Gsm,
                F.ValueName ConceptForName, S.ValueName ConceptStatus, ISG.SubGroupName ItemSubGroup, MSC.SubClassName, M.MCSubClassID, E.EmployeeName UserName
                {statusPropCondition}

                FROM {fromString}
                INNER JOIN KnittingMachineType KM ON KM.TypeID = M.KnittingTypeID
                LEFT JOIN KnittingMachineSubClass MSC ON MSC.SubClassID = M.MCSubClassID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = M.CompositionID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = M.ConstructionID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = M.GSMID
                LEFT JOIN FabricTechnicalName Technical ON Technical.TechnicalNameId=M.TechnicalNameId
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue F ON F.ValueID = M.ConceptFor
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue S ON S.ValueID = M.ConceptStatusID
                LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID=M.SubGroupID
                LEFT JOIN  {DbNames.EPYSL}..LoginUser L ON L.UserCode = M.AddedBy
                LEFT Join {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
                LEFT JOIN ConceptType T ON T.ConceptTypeID=M.ConceptTypeID
                {pendingStatusJoin}
                )
                SELECT F.*, Count(*) Over() TotalRows
                FROM F {whereCondition} ";

            */

            sql += $@"
                {paginationInfo.FilterBy}
            {orderBy}
            {paginationInfo.PageBy}
            ";

            return await _service.GetDataAsync<FreeConceptMaster>(sql);
        }

        public async Task<FreeConceptMaster> GetTechnicalNameList(int subClassId)
        {
            var query =
                $@"
                -- Technical name List
                SELECT Cast(TN.TechnicalNameId As varchar) id, TN.TechnicalName text
                FROM FabricTechnicalName TN
                INNER JOIN FabricTechnicalNameKMachineSubClass FTN ON FTN.TechnicalNameID=TN.TechnicalNameId
                WHERE FTN.SubClassID={subClassId}
                Group By TN.TechnicalNameId, TN.TechnicalName
                ";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                FreeConceptMaster data = new FreeConceptMaster
                {
                    TechnicalNameList = await records.ReadAsync<Select2OptionModel>()
                };
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

        public async Task<FreeConceptMaster> GetNewAsync()
        {
            var segmentNames = new
            {
                SegmentNames = new string[]
                {
                    ItemSegmentNameConstants.CONSTRUCTION,
                    ItemSegmentNameConstants.COMPOSITION,
                    ItemSegmentNameConstants.GSM
                }
            };
            var query =
                $@"
                -- Item Segments
                {CommonQueries.GetItemSegmentValuesBySegmentNamesWithSegmentName()};

                -- Fabric Components
                {CommonQueries.GetEntityTypeValuesOnly(EntityTypeNameConstants.FABRIC_TYPE)}

                ----knitting type
                ;SELECT Cast(V.TypeID As varchar) [id], V.TypeName [text]
				FROM KnittingMachineType V;

                ----Item Subgroup
                ;SELECT Cast(SubGroupID As varchar) [id], SubGroupName [text]
				FROM {DbNames.EPYSL}..ItemSubGroup
				WHERE DisplaySubGrupID IN ('FBR','Collar','Cuff','Inner Placket');

                ----Other Technical Name
                SELECT Cast(a.TechnicalNameId As varchar) id, a.TechnicalName text
                FROM FabricTechnicalName a
                Inner Join KnittingMachineSubClassTechnicalName b On b.TechnicalNameID = a.TechnicalNameId
                Group By a.TechnicalNameId, a.TechnicalName;

                ----Subclass
                ;SELECT CAST(a.MachineSubClassID AS varchar) [id], b.SubClassName [text], b.TypeID [desc], c.TypeName additionalValue
                FROM KnittingMachine a
                INNER JOIN KnittingMachineSubClass b ON b.SubClassID = a.MachineSubClassID
                Inner Join KnittingMachineType c On c.TypeID = b.TypeID
                --Where c.TypeName != 'Flat Bed'
                GROUP BY a.MachineSubClassID, b.SubClassName, b.TypeID, c.TypeName;

                -- Fabric FabricUsedPart
                {CommonQueries.GetFabricUsedPart()}

                -- Gauge
                ;SELECT Cast(a.GG As varchar) id, a.GG text
                FROM KnittingMachine a
                Inner Join KnittingMachineSubClassTechnicalName b On b.SubClassID = a.MachineSubClassID
                Group By a.GG

                --Technical Name
                SELECT Cast(T.TechnicalNameId As varchar) id, T.TechnicalName [text], ISNULL(ST.[Days], 0) [desc], Cast(SC.SubClassID as varchar) additionalValue
                FROM FabricTechnicalName T
                LEFT JOIN FabricTechnicalNameKMachineSubClass SC ON SC.TechnicalNameID = T.TechnicalNameId
                LEFT JOIN KnittingMachineStructureType_HK ST ON ST.StructureTypeID = SC.StructureTypeID
                Group By T.TechnicalNameId, T.TechnicalName, ST.Days, SC.SubClassID;";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query, segmentNames);
                FreeConceptMaster data = new FreeConceptMaster();

                var itemSegments = await records.ReadAsync<Select2OptionModel>();
                data.ConstructionList = itemSegments.Where(x => x.desc == ItemSegmentNameConstants.CONSTRUCTION);
                data.CompositionList = itemSegments.Where(x => x.desc == ItemSegmentNameConstants.COMPOSITION);
                data.GSMList = itemSegments.Where(x => x.desc == ItemSegmentNameConstants.GSM);

                data.FabricComponents = await records.ReadAsync<string>();

                data.KnittingTypeList = await records.ReadAsync<Select2OptionModel>();
                data.SubGroupList = await records.ReadAsync<Select2OptionModel>();
                data.OtherTechnicalNameList = await records.ReadAsync<Select2OptionModel>();

                List<Select2OptionModel> subClassList = records.Read<Select2OptionModel>().ToList();
                data.MCSubClassList = subClassList.Where(x => x.additionalValue != "Flat Bed");
                data.OtherMCSubClassList = subClassList.Where(x => x.additionalValue == "Flat Bed");
                data.FabricUsedPartList = records.Read<Select2OptionModel>().ToList();
                data.MachineGaugeList = records.Read<Select2OptionModel>().ToList();

                data.TechnicalNameList = await records.ReadAsync<Select2OptionModel>();
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

        public async Task<FreeConceptMaster> GetRevisionListAsync(int id, int subClassId)
        {
            var segmentNames = new
            {
                SegmentNames = new string[]
                {
                    ItemSegmentNameConstants.CONSTRUCTION,
                    ItemSegmentNameConstants.COMPOSITION,
                    ItemSegmentNameConstants.GSM
                }
            };

            var query =
                $@"
                ;Select a.ConceptID, a.ConceptNo, a.ConceptDate, a.TrialNo, a.TrialDate, a.ConceptFor, a.MCSubClassID, a.KnittingTypeID, KM.TypeName KnittingType,
                a.ConstructionID, a.TechnicalNameId, a.CompositionID, a.GSMID, a.Qty, a.ConceptStatusID, a.Remarks, a.SubGroupID
                From FreeConceptMaster a
                Left Join KnittingMachineType KM ON KM.TypeID = a.KnittingTypeID
                where a.ConceptID={id};

                -- Childs Color
                ;Select C.CCColorID, C.ColorID, FCBS.ColorCode, ISV.SegmentValue ColorName, FCBS.RGBOrHex, C.Remarks, C.IsLive
                From FreeConceptChildColor C
                LEFT Join {DbNames.EPYSL}..FabricColorBookSetup FCBS ON FCBS.ColorID = C.ColorID
                LEFT Join {DbNames.EPYSL}..ItemSegmentValue ISV On ISV.SegmentValueID = C.ColorID  
                Where ConceptID = {id}
                Group By C.CCColorID, C.ColorID, FCBS.ColorCode, C.ColorCode, ISV.SegmentValue, FCBS.RGBOrHex, C.Remarks, C.IsLive;

                -- Item Segments
                {CommonQueries.GetItemSegmentValuesBySegmentNamesWithSegmentName()};

                -- Fabric Components
                {CommonQueries.GetEntityTypeValuesOnly(EntityTypeNameConstants.FABRIC_TYPE)}

                ----knitting type
                ;SELECT Cast(V.TypeID As varchar) [id], V.TypeName [text]
							FROM KnittingMachineType V;

                ----Item Subgroup
                ;SELECT Cast(SubGroupID As varchar) [id], SubGroupName [text]
							FROM {DbNames.EPYSL}..ItemSubGroup
							WHERE DisplaySubGrupID IN ('FBR','Collar','Cuff','Inner Placket');

              -- Technical name List
                SELECT Cast(TN.TechnicalNameId As varchar) id, TN.TechnicalName text
                FROM FabricTechnicalName TN
                INNER JOIN FabricTechnicalNameKMachineSubClass FTN ON FTN.TechnicalNameID=TN.TechnicalNameId
                WHERE FTN.SubClassID={subClassId}
                Group By TN.TechnicalNameId, TN.TechnicalName

                  ----Subclass
                ;SELECT CAST(a.MachineSubClassID AS varchar) [id], b.SubClassName [text], b.TypeID [desc]
                FROM KnittingMachine a
                INNER JOIN KnittingMachineSubClass b ON b.SubClassID = a.MachineSubClassID
                GROUP BY a.MachineSubClassID, b.SubClassName, b.TypeID";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query, segmentNames);

                FreeConceptMaster data = await records.ReadFirstOrDefaultAsync<FreeConceptMaster>();
                data.ChildColors = records.Read<FreeConceptChildColor>().ToList();

                var itemSegments = await records.ReadAsync<Select2OptionModel>();
                data.ConstructionList = itemSegments.Where(x => x.desc == ItemSegmentNameConstants.CONSTRUCTION);
                data.CompositionList = itemSegments.Where(x => x.desc == ItemSegmentNameConstants.COMPOSITION);
                data.GSMList = itemSegments.Where(x => x.desc == ItemSegmentNameConstants.GSM);

                data.FabricComponents = await records.ReadAsync<string>();

                data.KnittingTypeList = await records.ReadAsync<Select2OptionModel>();
                data.SubGroupList = await records.ReadAsync<Select2OptionModel>();
                data.TechnicalNameList = await records.ReadAsync<Select2OptionModel>();
                data.MCSubClassList = records.Read<Select2OptionModel>().ToList();
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

        public async Task<FreeConceptMaster> GetAsync(int id, int subClassId)
        {
            var segmentNames = new
            {
                SegmentNames = new string[]
                {
                    ItemSegmentNameConstants.CONSTRUCTION,
                    ItemSegmentNameConstants.COMPOSITION,
                    ItemSegmentNameConstants.GSM
                }
            };

            var query =
                $@"
                ;Select a.ConceptID, a.ConceptNo, a.ConceptDate, a.TrialNo, a.TrialDate, a.ConceptFor, a.MCSubClassID, a.KnittingTypeID, KM.TypeName KnittingType,
                a.ConstructionID, a.TechnicalNameId, a.CompositionID, a.GSMID, a.Qty, a.ConceptStatusID, a.Remarks, a.SubGroupID
                From FreeConceptMaster a
                Left Join KnittingMachineType KM ON KM.TypeID = a.KnittingTypeID
                where a.ConceptID={id};

                -- Childs Color
                ;Select C.CCColorID, C.ColorID, C.ColorCode, ISV.SegmentValue ColorName, FCBS.RGBOrHex, C.Remarks, C.IsLive
                From FreeConceptChildColor C
                LEFT Join {DbNames.EPYSL}..FabricColorBookSetup FCBS ON FCBS.ColorID = C.ColorID
                LEFT Join {DbNames.EPYSL}..ItemSegmentValue ISV On ISV.SegmentValueID = C.ColorID  
                where C.ConceptID={id}
                Group By C.CCColorID, C.ColorID, C.ColorCode, ISV.SegmentValue, FCBS.RGBOrHex, C.Remarks, C.IsLive;

                -- Item Segments
                {CommonQueries.GetItemSegmentValuesBySegmentNamesWithSegmentName()};

                -- Fabric Components
                {CommonQueries.GetEntityTypeValuesOnly(EntityTypeNameConstants.FABRIC_TYPE)}

                ----knitting type
                ;SELECT Cast(V.TypeID As varchar) [id], V.TypeName [text]
							FROM KnittingMachineType V;

                ----Item Subgroup
                ;SELECT Cast(SubGroupID As varchar) [id], SubGroupName [text]
							FROM {DbNames.EPYSL}..ItemSubGroup
							WHERE DisplaySubGrupID IN ('FBR','Collar','Cuff','Inner Placket');

              --Technical Name
               SELECT Cast(TN.TechnicalNameId As varchar) id, TN.TechnicalName text
                FROM FabricTechnicalName TN
                INNER JOIN FabricTechnicalNameKMachineSubClass FTN ON FTN.TechnicalNameID=TN.TechnicalNameId
                WHERE FTN.SubClassID={subClassId}
                Group By TN.TechnicalNameId, TN.TechnicalName

               ----Subclass
                ;SELECT CAST(a.MachineSubClassID AS varchar) [id], b.SubClassName [text], b.TypeID [desc]
                FROM KnittingMachine a
                INNER JOIN KnittingMachineSubClass b ON b.SubClassID = a.MachineSubClassID
                GROUP BY a.MachineSubClassID, b.SubClassName, b.TypeID";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query, segmentNames);

                FreeConceptMaster data = await records.ReadFirstOrDefaultAsync<FreeConceptMaster>();
                data.ChildColors = records.Read<FreeConceptChildColor>().ToList();

                var itemSegments = await records.ReadAsync<Select2OptionModel>();
                data.ConstructionList = itemSegments.Where(x => x.desc == ItemSegmentNameConstants.CONSTRUCTION);
                data.CompositionList = itemSegments.Where(x => x.desc == ItemSegmentNameConstants.COMPOSITION);
                data.GSMList = itemSegments.Where(x => x.desc == ItemSegmentNameConstants.GSM);

                data.FabricComponents = await records.ReadAsync<string>();

                data.KnittingTypeList = await records.ReadAsync<Select2OptionModel>();
                data.SubGroupList = await records.ReadAsync<Select2OptionModel>();
                data.TechnicalNameList = await records.ReadAsync<Select2OptionModel>();
                data.MCSubClassList = records.Read<Select2OptionModel>().ToList();
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

        public async Task<FreeConceptMaster> GetByGroupConceptAsync(string grpConceptNo, int conceptTypeID)
        {
            var segmentNames = new
            {
                SegmentNames = new string[]
                {
                    ItemSegmentNameConstants.CONSTRUCTION,
                    ItemSegmentNameConstants.COMPOSITION,
                    ItemSegmentNameConstants.GSM
                }
            };
            var query = "";
            if (conceptTypeID == 1 || conceptTypeID == 2) //Only Fabric = 1, Fabric & Other Item = 2
            {
                query =
                $@"
                ;Select F.GroupConceptNo, F.ConceptID, F.ConceptNo, F.ConceptDate, F.TrialNo, F.TrialDate, F.ConceptFor, F.MCSubClassID, F.KnittingTypeID,
                KM.TypeName KnittingType, F.ConstructionID, F.TechnicalNameId, F.CompositionID, F.GSMID, F.Qty, F.ConceptStatusID, F.Remarks, F.SubGroupID,
                F.ItemMasterID, F.ConceptTypeID, F.FUPartID, F.IsYD, F.MachineGauge, F.Length, F.Width, F.PreProcessRevNo, F.RevisionNo, F.RevisionDate, F.RevisionBy, F.RevisionReason
                From FreeConceptMaster F
                LEFT Join KnittingMachineType KM ON KM.TypeID = F.KnittingTypeID
                where F.GroupConceptNo='{grpConceptNo}' AND F.SubGroupID=1;";
            }
            else  //Only Other Item = 3
            {
                query =
                $@"
                ;Select F.GroupConceptNo, F.ConceptID, F.ConceptNo, F.ConceptDate, F.TrialNo, F.TrialDate, F.ConceptFor, F.MCSubClassID, F.KnittingTypeID,
                F.ConstructionID,F.TechnicalNameId,F.CompositionID,F.GSMID,F.Qty,F.ConceptStatusID,F.Remarks, F.SubGroupID, --KM.TypeName KnittingType,
                F.ItemMasterID, F.ConceptTypeID, F.FUPartID, F.IsYD, F.MachineGauge, FU.PartName FUPartName, MCS.SubClassName MCSubClassName,
                T.TechnicalName, F.Length, F.Width, F.PreProcessRevNo, F.RevisionNo, F.RevisionDate, F.RevisionBy, F.RevisionReason
                From FreeConceptMaster F
                --Inner Join KnittingMachineType KM ON KM.TypeID = KnittingTypeID
				INNER JOIN {DbNames.EPYSL}..FabricUsedPart FU ON FU.FUPartID = F.FUPartID
				INNER JOIN KnittingMachineSubClass MCS ON MCS.SubClassID = F.MCSubClassID
				INNER JOIN FabricTechnicalName T ON T.TechnicalNameId = F.TechnicalNameId
                where F.GroupConceptNo='{grpConceptNo}' AND F.SubGroupID != 1 AND F.GroupConceptNo = F.ConceptNo ";
            }
            query +=
                $@"

                ;Select F.GroupConceptNo, F.ConceptID, F.ConceptNo, F.ConceptDate, F.TrialNo, F.TrialDate, F.ConceptFor, F.MCSubClassID, F.KnittingTypeID,
                F.ConstructionID, F.TechnicalNameId, F.CompositionID, F.GSMID, F.Qty, F.ConceptStatusID, F.Remarks, F.SubGroupID, --KM.TypeName KnittingType,
                F.ItemMasterID, F.ConceptTypeID, F.FUPartID, F.IsYD, F.MachineGauge, FU.PartName FUPartName, MCS.SubClassName MCSubClassName,
                T.TechnicalName, F.Length, F.Width, F.PreProcessRevNo, F.RevisionNo, F.RevisionDate, F.RevisionBy, F.RevisionReason
                From FreeConceptMaster F
                --Inner Join KnittingMachineType KM ON KM.TypeID = KnittingTypeID
				INNER JOIN {DbNames.EPYSL}..FabricUsedPart FU ON FU.FUPartID = F.FUPartID
				INNER JOIN KnittingMachineSubClass MCS ON MCS.SubClassID = F.MCSubClassID
				INNER JOIN FabricTechnicalName T ON T.TechnicalNameId = F.TechnicalNameId
                where F.GroupConceptNo='{grpConceptNo}' AND F.SubGroupID != 1;

                -- Childs Color
                ;Select C.CCColorID, C.ConceptID, C.ColorID, C.ColorCode, ISV.SegmentValue ColorName, FCBS.RGBOrHex, C.Remarks, C.IsLive
                ,IsRecipeDone = CASE WHEN ISNULL(RRC.RecipeReqChildID,0) = 0 THEN 0 ELSE 1 END
                From FreeConceptChildColor C
                LEFT Join {DbNames.EPYSL}..FabricColorBookSetup FCBS ON FCBS.ColorID = C.ColorID
                LEFT Join {DbNames.EPYSL}..ItemSegmentValue ISV On ISV.SegmentValueID = C.ColorID
                LEFT JOIN RecipeRequestChild RRC ON RRC.ConceptID = C.ConceptID AND RRC.CCColorID = C.CCColorID
                where C.ConceptID IN (SELECT ConceptID FROM FreeConceptMaster FC where FC.GroupConceptNo='{grpConceptNo}')
                Group By C.CCColorID, C.ConceptID, C.ColorID, C.ColorCode, ISV.SegmentValue, FCBS.RGBOrHex, C.Remarks, C.IsLive, ISNULL(RRC.RecipeReqChildID,0);

                ;With FC As (
					Select ConceptID, RevisionNo 
					from FreeConceptMaster
					WHERE IsBDS = 0 AND ConceptNo = GroupConceptNo AND GroupConceptNo='{grpConceptNo}'
				)
				Select FC.ConceptID
				From FC
				Inner Join FreeConceptMRMaster MR On MR.ConceptID = FC.ConceptID And MR.RevisionNo = FC.RevisionNo;

                -- Item Segments
                {CommonQueries.GetItemSegmentValuesBySegmentNamesWithSegmentName()};

                -- Fabric Components
                {CommonQueries.GetEntityTypeValuesOnly(EntityTypeNameConstants.FABRIC_TYPE)}

                ----knitting type
                ;SELECT Cast(V.TypeID As varchar) [id], V.TypeName [text]
							FROM KnittingMachineType V;

                ----Item Subgroup
                ;SELECT Cast(SubGroupID As varchar) [id], SubGroupName [text]
							FROM {DbNames.EPYSL}..ItemSubGroup
							WHERE DisplaySubGrupID IN ('FBR','Collar','Cuff','Inner Placket');

                ----Technical Name
                SELECT Cast(T.TechnicalNameId As varchar) id, T.TechnicalName [text], ISNULL(ST.[Days], 0) [desc], Cast(SC.SubClassID as varchar) additionalValue
                FROM FabricTechnicalName T
                LEFT JOIN FabricTechnicalNameKMachineSubClass SC ON SC.TechnicalNameID = T.TechnicalNameId
                LEFT JOIN KnittingMachineStructureType_HK ST ON ST.StructureTypeID = SC.StructureTypeID
                Group By T.TechnicalNameId, T.TechnicalName, ST.Days, SC.SubClassID;

              ----Other Technical Name
                ;SELECT Cast(a.TechnicalNameId As varchar) id, a.TechnicalName text
                FROM FabricTechnicalName a
                Inner Join KnittingMachineSubClassTechnicalName b On b.TechnicalNameID = a.TechnicalNameId
                Group By a.TechnicalNameId, a.TechnicalName;

               ----Subclass
                ;SELECT CAST(a.MachineSubClassID AS varchar) [id], b.SubClassName [text], b.TypeID [desc], c.TypeName additionalValue
                FROM KnittingMachine a
                INNER JOIN KnittingMachineSubClass b ON b.SubClassID = a.MachineSubClassID
                Inner Join KnittingMachineType c On c.TypeID = b.TypeID
                --Where c.TypeName != 'Flat Bed'
                GROUP BY a.MachineSubClassID, b.SubClassName, b.TypeID, c.TypeName;

                -- Fabric FabricUsedPart
                {CommonQueries.GetFabricUsedPart()}

                -- Gauge
                SELECT Cast(a.GG As varchar) id, a.GG text
                FROM KnittingMachine a
                Inner Join KnittingMachineSubClassTechnicalName b On b.SubClassID = a.MachineSubClassID
                Group By a.GG;

                --Material
                SELECT MR.*
                FROM FreeConceptMRMaster MR
                INNER JOIN FreeConceptMaster FCM ON FCM.ConceptID = MR.ConceptID
                WHERE FCM.GroupConceptNo = '{grpConceptNo}';";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query, segmentNames);

                FreeConceptMaster data = new FreeConceptMaster();

                data.GroupConceptNo = grpConceptNo;
                data.FabricItems = records.Read<FreeConceptMaster>().ToList();
                data.OtherItems = records.Read<FreeConceptMaster>().ToList();
                data.ChildColors = records.Read<FreeConceptChildColor>().ToList();

                if (data.FabricItems.Count() > 0)
                {
                    var fcMaster = data.FabricItems.Find(x => x.ConceptNo == x.GroupConceptNo);
                    if (fcMaster.IsNotNull())
                    {
                        data.GroupConceptNo = fcMaster.ConceptNo;
                        data.ConceptDate = fcMaster.ConceptDate;
                        data.Remarks = fcMaster.Remarks;
                    }
                }
                else
                {
                    var fcMaster = data.OtherItems.Find(x => x.ConceptNo == x.GroupConceptNo);
                    if (fcMaster == null)
                    {
                        fcMaster = data.OtherItems.First();
                        data.GroupConceptNo = fcMaster.GroupConceptNo;
                        data.ConceptDate = fcMaster.ConceptDate;
                        data.Remarks = fcMaster.Remarks;
                    }
                    else
                    {
                        data.GroupConceptNo = fcMaster.ConceptNo;
                        data.ConceptDate = fcMaster.ConceptDate;
                        data.Remarks = fcMaster.Remarks;
                    }
                }

                List<FreeConceptMRMaster> FreeConceptMRMaster = records.Read<FreeConceptMRMaster>().ToList();
                //if (FreeConceptMRMaster.Count > 0) data.NeedRevision = true;

                var itemSegments = await records.ReadAsync<Select2OptionModel>();
                data.ConstructionList = itemSegments.Where(x => x.desc == ItemSegmentNameConstants.CONSTRUCTION);
                data.CompositionList = itemSegments.Where(x => x.desc == ItemSegmentNameConstants.COMPOSITION);
                data.GSMList = itemSegments.Where(x => x.desc == ItemSegmentNameConstants.GSM);

                data.FabricComponents = await records.ReadAsync<string>();
                data.KnittingTypeList = await records.ReadAsync<Select2OptionModel>();
                data.SubGroupList = await records.ReadAsync<Select2OptionModel>();
                data.TechnicalNameList = await records.ReadAsync<Select2OptionModel>();
                data.OtherTechnicalNameList = await records.ReadAsync<Select2OptionModel>();

                List<Select2OptionModel> subClassList = records.Read<Select2OptionModel>().ToList();
                data.MCSubClassList = subClassList.Where(x => x.additionalValue != "Flat Bed");
                data.OtherMCSubClassList = subClassList.Where(x => x.additionalValue == "Flat Bed");

                data.FabricUsedPartList = records.Read<Select2OptionModel>().ToList();
                data.MachineGaugeList = records.Read<Select2OptionModel>().ToList();

                data.FabricItems.ForEach(x =>
                {
                    if (data.MCSubClassList.Count() > 0 && data.MCSubClassList.FirstOrDefault(y => y.id == x.MCSubClassID.ToString()).IsNotNull())
                    {
                        x.MCSubClassName = data.MCSubClassList.FirstOrDefault(y => y.id == x.MCSubClassID.ToString()).text;
                    }
                    if (data.TechnicalNameList.Count() > 0 && data.TechnicalNameList.FirstOrDefault(y => y.id == x.TechnicalNameId.ToString()).IsNotNull())
                    {
                        x.TechnicalName = data.TechnicalNameList.FirstOrDefault(y => y.id == x.TechnicalNameId.ToString()).text;
                    }
                    if (data.CompositionList.Count() > 0 && data.CompositionList.FirstOrDefault(y => y.id == x.CompositionId.ToString()).IsNotNull())
                    {
                        x.Composition = data.CompositionList.FirstOrDefault(y => y.id == x.CompositionId.ToString()).text;
                    }
                    if (data.GSMList.Count() > 0 && data.GSMList.FirstOrDefault(y => y.id == x.GSMId.ToString()).IsNotNull())
                    {
                        x.GSM = data.GSMList.FirstOrDefault(y => y.id == x.GSMId.ToString()).text;
                    }
                });
                var mrList = records.Read<FreeConceptMRMaster>().ToList();
                if (mrList.Count() > 0)
                {
                    data.NeedRevision = true;
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

        public async Task<FreeConceptMaster> GetDetailsAsync(int id)
        {
            var sql = $@"
            -- Master Data
            Select * From FreeConceptMaster Where ConceptID = {id}

            -- Child Colors
            Select * From FreeConceptChildColor Where ConceptID = {id}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);

                FreeConceptMaster data = await records.ReadFirstOrDefaultAsync<FreeConceptMaster>();
                Guard.Against.NullObject(data);

                data.ChildColors = records.Read<FreeConceptChildColor>().ToList();

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
        public async Task<List<FreeConceptMaster>> GetDatasAsync(string grpConceptNo)
        {
            var sql = $@"
            -- Master Data
            Select * From FreeConceptMaster Where GroupConceptNo = '{grpConceptNo}';

            -- Child Colors
            ;Select * From FreeConceptChildColor Where ConceptID IN( SELECT ConceptID FROM FreeConceptMaster Where GroupConceptNo = '{grpConceptNo}');";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                List<FreeConceptMaster> datas = records.Read<FreeConceptMaster>().ToList();
                List<FreeConceptChildColor> childColors = records.Read<FreeConceptChildColor>().ToList();
                datas.ForEach(data =>
                {
                    data.ChildColors = childColors.Where(x => x.ConceptID == data.ConceptID).ToList();
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

        public async Task<List<ConceptPendingStatus_HK>> GetPendingStatus()
        {
            var sql = $@"
            -- Master Data
            SELECT * FROM ConceptPendingStatus_HK;";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                List<ConceptPendingStatus_HK> datas = records.Read<ConceptPendingStatus_HK>().ToList();
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

        public async Task<List<Select2OptionModel>> GetTechnicalNameByMC(int subclassId)
        {
            var query =
                $@"
                 SELECT Cast(TN.TechnicalNameId As varchar) id, TN.TechnicalName text
                FROM FabricTechnicalName TN
                INNER JOIN FabricTechnicalNameKMachineSubClass FTN ON FTN.TechnicalNameID=TN.TechnicalNameId
                WHERE FTN.SubClassID={subclassId}
                Group By TN.TechnicalNameId, TN.TechnicalName";

            var data = await _service.GetDataAsync<Select2OptionModel>(query);
            return data;
        }

        public async Task SaveAsync(FreeConceptMaster entity)
        {
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                int maxChildId = 0;
                switch (entity.EntityState)
                {
                    case EntityState.Added:
                        entity = await AddAsync(entity, true);
                        break;

                    case EntityState.Modified:
                        var addedChilds = entity.ChildColors.FindAll(x => x.EntityState == EntityState.Added);
                        maxChildId = await _signatureRepository.GetMaxIdAsync(TableNames.RND_FREE_CONCEPT_CHILD_COLOR, addedChilds.Count);

                        foreach (var item in addedChilds)
                        {
                            item.CCColorID = maxChildId++;
                            item.ConceptID = entity.ConceptID;
                        }
                        break;

                    case EntityState.Unchanged:
                    case EntityState.Deleted:
                        entity.EntityState = EntityState.Deleted;
                        entity.ChildColors.SetDeleted();
                        break;

                    default:
                        break;
                }

                await _service.SaveSingleAsync(entity, transaction);
                await _service.SaveAsync(entity.ChildColors, transaction);

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

        public async Task SaveRevisionAsync(FreeConceptMaster revisionEntity, FreeConceptMaster entity, int userId)
        {
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                await UpdateRevisionAsync(revisionEntity, userId);

                entity = await AddAsync(entity, false);
                await _service.SaveSingleAsync(entity, transaction);

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

        #region Helpers



        private async Task<FreeConceptMaster> AddAsync(FreeConceptMaster entity, bool needNewConceptNo)
        {
            entity.ConceptID = await _signatureRepository.GetMaxIdAsync(TableNames.RND_FREE_CONCEPT_MASTER);
            if (needNewConceptNo) entity.ConceptNo = await GetMaxNoAsync();

            int maxChildId = await _signatureRepository.GetMaxIdAsync(TableNames.RND_FREE_CONCEPT_CHILD_COLOR, entity.ChildColors.Count);
            foreach (var item in entity.ChildColors)
            {
                item.CCColorID = maxChildId++;
                item.ConceptID = entity.ConceptID;
            }

            return entity;
        }

        private async Task UpdateRevisionAsync(FreeConceptMaster entity, int userId)
        {
            Tuple<List<KnittingPlanMaster>, List<KJobCardMaster>, List<KnittingProduction>> revisionData
                = await GetAllRevisionDataAsync(entity.ConceptID);

            foreach (var planMaster in revisionData.Item1) // Knitting Plans
            {
                planMaster.Active = false;
                planMaster.UpdatedBy = userId;
                planMaster.DateUpdated = DateTime.Now;
                planMaster.EntityState = EntityState.Modified;
            }

            foreach (var jobCard in revisionData.Item2) // Job Card Masters
            {
                jobCard.Active = false;
                jobCard.DateUpdated = DateTime.Now;
                jobCard.UpdatedBy = userId;
                jobCard.EntityState = EntityState.Modified;
            }

            foreach (var production in revisionData.Item3) // Knitting Production
            {
                production.Active = false;
                production.DateUpdated = DateTime.Now;
                production.UpdatedBy = userId;
                production.EntityState = EntityState.Modified;
            }
        }

        private async Task<string> GetMaxNoAsync()
        {
            var id = await _signatureRepository.GetMaxIdAsync(TableNames.RND_CONCEPTNO, RepeatAfterEnum.EveryMonth);
            var datePart = DateTime.Now.ToString("yyMM");
            return $@"{datePart}{id:0000}";
        }

        private async Task<Tuple<List<KnittingPlanMaster>, List<KJobCardMaster>, List<KnittingProduction>>> GetAllRevisionDataAsync(int conceptID)
        {
            var sql = $@"
            -- KnittingPlanMaster
            Select * From KnittingPlanMaster Where ConceptID = {conceptID}

            -- KJobCardMaster
            Select * From KJobCardMaster Where ConceptID = {conceptID}

            -- KnittingProduction
            Select * From KnittingProduction Where ConceptID = {conceptID}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);

                List<KnittingPlanMaster> knittingPlans = records.Read<KnittingPlanMaster>().ToList();
                List<KJobCardMaster> jobCardMasters = records.Read<KJobCardMaster>().ToList();
                List<KnittingProduction> knittingProductions = records.Read<KnittingProduction>().ToList();

                return Tuple.Create(knittingPlans, jobCardMasters, knittingProductions);
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

        #endregion Helpers

        public async Task SaveManyAsync(List<FreeConceptMaster> entities, EntityState entityState)
        {
            List<ConceptPendingStatus_HK> conceptPendingStatusList = await GetPendingStatus();
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                switch (entityState)
                {
                    case EntityState.Added:
                        entities = await AddManyAsync(entities, conceptPendingStatusList);
                        break;

                    case EntityState.Modified:
                        entities = UpdateMany(entities);
                        break;

                    //case EntityState.Unchanged:
                    //case EntityState.Deleted:
                    //    entity.EntityState = EntityState.Deleted;
                    //    entity.ChildColors.SetDeleted();
                    //    break;

                    default:
                        break;
                }

                await _service.SaveAsync(entities, transaction);
                List<FreeConceptChildColor> childColors = new List<FreeConceptChildColor>();
                entities.ForEach(entity =>
                {
                    childColors.AddRange(entity.ChildColors);
                });
                await _service.SaveAsync(childColors, transaction);

                if (entityState == EntityState.Added)
                {
                    List<ConceptStatus> statusList = new List<ConceptStatus>();
                    entities.ForEach(entity =>
                    {
                        statusList.AddRange(entity.ConceptStatusList);
                    });
                    await _service.SaveAsync(statusList, transaction);
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

        private async Task<List<FreeConceptMaster>> AddManyAsync(List<FreeConceptMaster> entities, List<ConceptPendingStatus_HK> conceptPendingStatusList)
        {
            int conceptId = await _service.GetMaxIdAsync(TableNames.RND_FREE_CONCEPT_MASTER, entities.Count);
            int maxChildId = await _service.GetMaxIdAsync(TableNames.RND_FREE_CONCEPT_CHILD_COLOR, entities.Sum(x => x.ChildColors.Count));
            int maxStatusChildId = await _service.GetMaxIdAsync(TableNames.RND_FREE_CONCEPT_STATUS, (entities.Count * conceptPendingStatusList.Count));
            List<ConceptStatus> freeConceptStatusList = new List<ConceptStatus>();

            int slNo = 0;
            entities.ToList().ForEach(entity =>
            {
                entity.ConceptID = conceptId++;
                if (slNo == 0) entity.ConceptNo = entity.GroupConceptNo;
                else entity.ConceptNo = entity.GroupConceptNo + "_" + slNo;
                slNo++;

                foreach (var item in entity.ChildColors)
                {
                    item.CCColorID = maxChildId++;
                    item.ConceptID = entity.ConceptID;
                }

                conceptPendingStatusList.ForEach(x =>
                {
                    ConceptStatus conStatus = new ConceptStatus
                    {
                        FCSID = maxStatusChildId++,
                        ConceptID = entity.ConceptID,
                        CPSID = x.CPSID,
                        IsApplicable = x.IsApplicable,
                        SeqNo = x.SeqNo,
                        Status = false,
                        Remarks = ""
                    };
                    entity.ConceptStatusList.Add(conStatus);
                });
            });

            return entities;
        }

        private List<FreeConceptMaster> UpdateMany(List<FreeConceptMaster> entities)
        {
            string conceptNo = entities.Where(x => x.EntityState == EntityState.Modified).FirstOrDefault().GroupConceptNo;
            int mConId = entities.Max(x => x.ConceptID);
            //int slNo = Convert.ToInt32(entities.Where(x => x.ConceptID == mConId).FirstOrDefault().ConceptNo.Last());
            int slNo = (entities.Where(x => x.ConceptID == mConId).FirstOrDefault().ConceptNo.Split('_').Length > 1) ?
                entities.Where(x => x.ConceptID == mConId).FirstOrDefault().ConceptNo.Split('_')[1].ToInt() : 1;

            int conceptId = _signatureRepository.GetMaxId(TableNames.RND_FREE_CONCEPT_MASTER, entities.Where(x => x.EntityState == EntityState.Added).Count());
            int maxChildId = _signatureRepository.GetMaxId(TableNames.RND_FREE_CONCEPT_CHILD_COLOR, entities.Sum(x => x.ChildColors.Where(y => y.EntityState == EntityState.Added).Count()));

            entities.ToList().ForEach(entity =>
            {
                switch (entity.EntityState)
                {
                    case EntityState.Added:
                        entity.GroupConceptNo = conceptNo;
                        entity.ConceptNo = conceptNo + "_" + ++slNo;
                        entity.ConceptID = conceptId++;

                        foreach (var item in entity.ChildColors)
                        {
                            item.CCColorID = maxChildId++;
                            item.ConceptID = entity.ConceptID;
                        }
                        break;

                    case EntityState.Modified:
                        var addedChilds = entity.ChildColors.FindAll(x => x.EntityState == EntityState.Added);
                        foreach (var item in addedChilds)
                        {
                            item.CCColorID = maxChildId++;
                            item.ConceptID = entity.ConceptID;
                        }
                        break;

                    case EntityState.Deleted:
                        entity.EntityState = EntityState.Deleted;
                        entity.ChildColors.SetDeleted();
                        break;

                    default:
                        break;
                }
            });

            return entities;
        }

        public async Task ReviseManyAsync(List<FreeConceptMaster> entities, string grpConceptNo, EntityState entityState)
        {
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                await _connection.ExecuteAsync("spBackupFreeConcept_Full", new { ConceptNo = grpConceptNo }, transaction, 30, CommandType.StoredProcedure);

                entities = UpdateMany(entities);

                await _service.SaveAsync(entities, transaction);
                List<FreeConceptChildColor> childColors = new List<FreeConceptChildColor>();
                entities.ForEach(entity =>
                {
                    childColors.AddRange(entity.ChildColors);
                });
                await _service.SaveAsync(childColors, transaction);

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
        private string GetStatusName(FreeConceptStatus status)
        {
            switch (status)
            {
                case FreeConceptStatus.SourcingPending: return "Yarn Purchase Pending";
                case FreeConceptStatus.YDPending: return "YD Pending";
                case FreeConceptStatus.KnitPending: return "Kniting Pending";
                case FreeConceptStatus.FinishPending: return "Finishing Pending";
                case FreeConceptStatus.WaitingForLivePending: return "Waiting for Live";
                default: return Regex.Replace(status.ToString(), "([a-z])([A-Z])", "$1 $2");
            }
        }

        public async Task<List<FreeConceptMaster>> GetByBookingIds(string bookingIds)
        {
            var sql = $@"
            -- Master Data
            Select * From FreeConceptMaster Where BookingID IN ({bookingIds}); 

            -- Child Colors
            Select * From FreeConceptChildColor FCCC WHERE FCCC.ConceptID IN (SELECT FCM.ConceptID FROM FreeConceptMaster FCM WHERE FCM.BookingID IN ({bookingIds}))";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                List<FreeConceptMaster> datas = records.Read<FreeConceptMaster>().ToList();
                List<FreeConceptChildColor> childColors = records.Read<FreeConceptChildColor>().ToList();
                datas.ForEach(data =>
                {
                    data.ChildColors = childColors.Where(x => x.ConceptID == data.ConceptID).ToList();
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

        public async Task<List<FreeConceptChildColor>> GetChildColorDatasAsync(int conceptID)
        {
            var sql = $@"
            -- Master Data
            Select * From FreeConceptChildColor Where ConceptID = {conceptID}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                List<FreeConceptChildColor> childColors = records.Read<FreeConceptChildColor>().ToList();
                return childColors;
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

        public async Task SaveAsyncChildColor(List<FreeConceptChildColor> entities)
        {
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();
                int maxId = 0;

                maxId = await _signatureRepository.GetMaxIdAsync(TableNames.RND_FREE_CONCEPT_CHILD_COLOR, entities.FindAll(x => x.EntityState == EntityState.Added).Count());
                foreach (var item in entities.Where(x => x.EntityState == EntityState.Added))
                {
                    item.CCColorID = maxId++;
                }
                await _service.SaveAsync(entities, transaction);

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
    }
}
