using EPYSLTEX.Core.Statics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Azure.Core.HttpHeader;
using static System.Net.Mime.MediaTypeNames;

namespace EPYSLTEXCore.Infrastructure.Static
{
    /// <summary>
    /// Write all your queries here.
    /// </summary>
    public static class CommonQueries
    {

        public static string GetContactByType(string categoryName)
        {
            return $@"Select Cast(C.ContactID As varchar) [id], C.Name [text], CC.ContactCategoryName [desc]
                From Contacts C
                Inner Join ContactCategoryChild CCC On C.ContactID = CCC.ContactID
                Inner Join ContactCategoryHK CC ON CC.ContactCategoryID = CCC.ContactCategoryID
                Where CC.ContactCategoryName IN ('{categoryName}')";
        }
        public static string GetItemSegmentValuesBySegmentNamesWithMapping()
        {
            //if (countIds.IsNotNullOrEmpty()) countSQL += $@" OR ISV.SegmentValueID IN ({countIds})";
            //if (compositionIds.IsNotNullOrEmpty()) compositionSQL += $@" OR ISV.SegmentValueID IN ({compositionIds})";

            string sql = $@";With YCO as(
                    select YarnCompositionID = ISV.SegmentValueID, 0 ManufacturingLineID,0 ManufacturingProcessID,0 ManufacturingSubProcessID,0 TechnicalParameterID,0 ColorID, 0 ColorGradeID,0 YarnCountID, 0 ShadeReferenceID, CAST(ISV.SegmentValueID As varchar) [id], ISV.SegmentValue [text], ISN.SegmentName [desc], IsInactive = ISNULL(YCBS.IsInactive,0)
                    from {DbNames.EPYSL}..ItemSegmentValue ISV
                    INNER JOIN {DbNames.EPYSL}..ItemSegmentName ISN ON ISN.SegmentNameID = ISV.SegmentNameID
                    LEFT JOIN {TableNames.YarnCompositionBasicSetup} YCBS ON YCBS.SegmentValueId = ISV.SegmentValueID
                    WHERE ISN.SegmentName In ('{ItemSegmentNameConstants.YARN_COMPOSITION}') --AND (ISNULL(YCBS.IsInactive,0) = 0)
                ),
                YT as (
                    select distinct SV.YarnCompositionID, SV.ManufacturingLineID,0 ManufacturingProcessID,0 ManufacturingSubProcessID,0 TechnicalParameterID,0 ColorID, 0 ColorGradeID,0 YarnCountID,
					0 ShadeReferenceID, CAST(SV.ManufacturingLineID As varchar) [id], ISV.SegmentValue [text], ISN.SegmentName [desc], IsInactive = 0
                    from {TableNames.YarnRMProperties} SV
                    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID=SV.ManufacturingLineID    
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentName ISN ON ISN.SegmentNameID = ISV.SegmentNameID 
                    WHERE ISN.SegmentName In ('{ItemSegmentNameConstants.YARN_TYPE}')                
                    --ORDER BY ISV.SegmentValue
                ),
                MP as (
                    select distinct SV.YarnCompositionID, SV.ManufacturingLineID, SV.ManufacturingProcessID,0 ManufacturingSubProcessID,
					0 TechnicalParameterID,0 ColorID, 0 ColorGradeID,0 YarnCountID, 0 ShadeReferenceID,CAST(SV.ManufacturingProcessID As varchar) [id], ISV.SegmentValue [text], ISN.SegmentName [desc], IsInactive = 0
                    from {TableNames.YarnRMProperties} SV
                    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID=SV.ManufacturingProcessID    
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentName ISN ON ISN.SegmentNameID = ISV.SegmentNameID 
                    WHERE ISN.SegmentName In ('{ItemSegmentNameConstants.YARN_MANUFACTURING_PROCESS}')                
                    --ORDER BY ISV.SegmentValue
                ),
                SP as (
                    select distinct SV.YarnCompositionID, SV.ManufacturingLineID, SV.ManufacturingProcessID,SV.ManufacturingSubProcessID,
					0 TechnicalParameterID,0 ColorID, 0 ColorGradeID,0 YarnCountID, 0 ShadeReferenceID,CAST(SV.ManufacturingSubProcessID As varchar) [id], ISV.SegmentValue [text], ISN.SegmentName [desc], IsInactive = 0
                    from {TableNames.YarnRMProperties} SV
                    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID=SV.ManufacturingSubProcessID    
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentName ISN ON ISN.SegmentNameID = ISV.SegmentNameID 
                    WHERE ISN.SegmentName In ('{ItemSegmentNameConstants.YARN_MANUFACTURING_SUB_PROCESS}')                
                    --ORDER BY ISV.SegmentValue
                ),
                YQP as (
                    select distinct SV.YarnCompositionID, SV.ManufacturingLineID, SV.ManufacturingProcessID, SV.ManufacturingSubProcessID, SV.TechnicalParameterID,0 ColorID, 0 ColorGradeID,0 YarnCountID, 0 ShadeReferenceID,CAST(SV.TechnicalParameterID As varchar) [id], ISV.SegmentValue [text], ISN.SegmentName [desc], IsInactive = 0
                    from {TableNames.YarnRMProperties} SV
                    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID=SV.TechnicalParameterID    
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentName ISN ON ISN.SegmentNameID = ISV.SegmentNameID 
                    WHERE ISN.SegmentName In ('{ItemSegmentNameConstants.YARN_QUALITY_PARAMETER}')                
                    --ORDER BY ISV.SegmentValue
                ),/*
                YCM as (
                    Select YarnTypeSVID,ManufacturingProcessSVID,SubProcessSVID,QualityParameterSVID,0 CountSVID,CAST(0 As varchar) [id],CountUnit [text],'Yarn Count Master' [desc], IsInactive = 0
				    from (
                    select SV.YarnTypeSVID,SV.ManufacturingProcessSVID,SV.SubProcessSVID,SV.QualityParameterSVID,CountUnit 
					from {TableNames.YarnRMProperties} SV
                    INNER JOIN YQP on SV.YarnTypeSVID=YQP.YarnTypeSVID  AND SV.ManufacturingProcessSVID=YQP.ManufacturingProcessSVID AND SV.SubProcessSVID=YQP.SubProcessSVID AND SV.QualityParameterSVID=YQP.QualityParameterSVID
                ) A
                ),*/
				CLR as(
                    select SV.YarnCompositionID, SV.ManufacturingLineID, SV.ManufacturingProcessID, SV.ManufacturingSubProcessID, SV.TechnicalParameterID, SV.ColorID, 
					0 ColorGradeID,0 YarnCountID, 0 ShadeReferenceID, CAST(ISV.SegmentValueID As varchar) [id], ISV.SegmentValue [text], ISN.SegmentName [desc], IsInactive = 0
                    from {TableNames.YarnRMProperties} SV
                    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID=SV.ColorID    
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentName ISN ON ISN.SegmentNameID = ISV.SegmentNameID
                    WHERE ISN.SegmentName In ('{ItemSegmentNameConstants.YARN_COLOR}') --AND (ISNULL(YCBS.IsInactive,0) = 0)
				),
				CLGR as(
                    select SV.YarnCompositionID, SV.ManufacturingLineID, SV.ManufacturingProcessID, SV.ManufacturingSubProcessID, SV.TechnicalParameterID, SV.ColorID, 
					SV.ColorGradeID,0 YarnCountID, 0 ShadeReferenceID, CAST(ISV.SegmentValueID As varchar) [id], ISV.SegmentValue [text], ISN.SegmentName [desc], IsInactive = 0
                    from {TableNames.YarnRMProperties} SV
                    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID=SV.ColorGradeID    
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentName ISN ON ISN.SegmentNameID = ISV.SegmentNameID
                    WHERE ISN.SegmentName In ('{ItemSegmentNameConstants.YARN_COLOR_GRADE}') --AND (ISNULL(YCBS.IsInactive,0) = 0)
				),
                YC as(
                    select SV.YarnCompositionID, SV.ManufacturingLineID, SV.ManufacturingProcessID, SV.ManufacturingSubProcessID, SV.TechnicalParameterID,SV.ColorID, 
					SV.ColorGradeID, SV.YarnCountID, 0 ShadeReferenceID, CAST(ISV.SegmentValueID As varchar) [id], ISV.SegmentValue [text], ISN.SegmentName [desc], IsInactive = ISNULL(YCBS.IsInactive,0)
                    from {TableNames.YarnRMProperties} SV
                    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID=SV.YarnCountID    
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentName ISN ON ISN.SegmentNameID = ISV.SegmentNameID
                    LEFT JOIN {TableNames.YarnCountBasicSetup} YCBS ON YCBS.SegmentValueId = ISV.SegmentValueID
                    WHERE ISN.SegmentName In ('{ItemSegmentNameConstants.YARN_COUNT}') --AND (ISNULL(YCBS.IsInactive,0) = 0)
				),
                SC AS(
                    select SV.YarnCompositionID, SV.ManufacturingLineID, SV.ManufacturingProcessID, SV.ManufacturingSubProcessID, SV.TechnicalParameterID, SV.ColorID, 
                    SV.ColorGradeID, SV.YarnCountID, ShadeReferenceID = YSB.YSCID, CAST(YSB.YSCID As varchar) [id], YSB.ShadeCode [text], [desc] = '{ItemSegmentNameConstants.SHADE}', IsInactive = 0
                    from {TableNames.YarnRMProperties} SV
                    INNER JOIN {TableNames.YARN_SHADE_BOOK} YSB ON YSB.YSCID = SV.ShadeReferenceID
                )
                Select * from YCO
                Union All
                Select * from YT
                Union All
                Select * from MP
                Union All
                Select * from SP
                Union All
                Select * from YQP
				--Union All
    --            Select * from YCM
				Union All
                Select * from CLR
				Union All
                Select * from CLGR
				Union All
                Select * from YC
				Union All
                Select * from SC

                ORDER BY [desc],[text];

                /*
                ;With YCO as(
                    select 0 YarnTypeSVID,0 ManufacturingProcessSVID,0 SubProcessSVID,0 QualityParameterSVID,0 CountSVID,CAST(ISV.SegmentValueID As varchar) [id], ISV.SegmentValue [text], ISN.SegmentName [desc], IsInactive = ISNULL(YCBS.IsInactive,0)
                    from {DbNames.EPYSL}..ItemSegmentValue ISV
                    INNER JOIN {DbNames.EPYSL}..ItemSegmentName ISN ON ISN.SegmentNameID = ISV.SegmentNameID
                    LEFT JOIN {TableNames.YarnCompositionBasicSetup} YCBS ON YCBS.SegmentValueId = ISV.SegmentValueID
                    WHERE ISN.SegmentName In ('Yarn Composition Live') --AND (ISNULL(YCBS.IsInactive,0) = 0)
                ),
                YT as (
                    select distinct SV.YarnTypeSVID,0 ManufacturingProcessSVID,0 SubProcessSVID,0 QualityParameterSVID,0 CountSVID,CAST(SV.YarnTypeSVID As varchar) [id], ISV.SegmentValue [text], ISN.SegmentName [desc], IsInactive = 0
                    from {DbNames.EPYSL}..SegmentValueYarnTypeMappingSetup SV
                    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID=SV.YarnTypeSVID    
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentName ISN ON ISN.SegmentNameID = ISV.SegmentNameID 
                    WHERE ISN.SegmentName In ('Yarn Type Live')                
                    --ORDER BY ISV.SegmentValue
                ),
                MP as (
                    select distinct SV.YarnTypeSVID,SV.ManufacturingProcessSVID,0 SubProcessSVID,0 QualityParameterSVID,0 CountSVID,CAST(SV.ManufacturingProcessSVID As varchar) [id], ISV.SegmentValue [text], ISN.SegmentName [desc], IsInactive = 0
                    from {DbNames.EPYSL}..SegmentValueYarnTypeMappingSetup SV
                    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID=SV.ManufacturingProcessSVID    
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentName ISN ON ISN.SegmentNameID = ISV.SegmentNameID 
                    WHERE ISN.SegmentName In ('Yarn Manufacturing Process Live')                
                    --ORDER BY ISV.SegmentValue
                ),
                SP as (
                    select distinct SV.YarnTypeSVID,SV.ManufacturingProcessSVID,SV.SubProcessSVID,0 QualityParameterSVID,0 CountSVID,CAST(SV.SubProcessSVID As varchar) [id], ISV.SegmentValue [text], ISN.SegmentName [desc], IsInactive = 0
                    from {DbNames.EPYSL}..SegmentValueYarnTypeMappingSetup SV
                    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID=SV.SubProcessSVID    
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentName ISN ON ISN.SegmentNameID = ISV.SegmentNameID 
                    WHERE ISN.SegmentName In ('Yarn Manufacturing Sub Process Live')                
                    --ORDER BY ISV.SegmentValue
                ),
                YQP as (
                    select distinct SV.YarnTypeSVID,SV.ManufacturingProcessSVID,SV.SubProcessSVID,SV.QualityParameterSVID,0 CountSVID,CAST(SV.QualityParameterSVID As varchar) [id], ISV.SegmentValue [text], ISN.SegmentName [desc], IsInactive = 0
                    from {DbNames.EPYSL}..SegmentValueYarnTypeMappingSetup SV
                    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID=SV.QualityParameterSVID    
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentName ISN ON ISN.SegmentNameID = ISV.SegmentNameID 
                    WHERE ISN.SegmentName In ('Yarn Quality Parameter Live')                
                    --ORDER BY ISV.SegmentValue
                ),
                YCM as (
                    Select YarnTypeSVID,ManufacturingProcessSVID,SubProcessSVID,QualityParameterSVID,0 CountSVID,CAST(0 As varchar) [id],CountUnit [text],'Yarn Count Master' [desc], IsInactive = 0
				    from (
                    select SV.YarnTypeSVID,SV.ManufacturingProcessSVID,SV.SubProcessSVID,SV.QualityParameterSVID,CountUnit from {DbNames.EPYSL}..SegmentValueYarnTypeMappingSetup SV
                    INNER JOIN YQP on SV.YarnTypeSVID=YQP.YarnTypeSVID  AND SV.ManufacturingProcessSVID=YQP.ManufacturingProcessSVID AND SV.SubProcessSVID=YQP.SubProcessSVID AND SV.QualityParameterSVID=YQP.QualityParameterSVID
                ) A
                ),
				YC as(
                    select 0 YarnTypeSVID,0 ManufacturingProcessSVID,0 SubProcessSVID,0 QualityParameterSVID,0 CountSVID,CAST(ISV.SegmentValueID As varchar) [id], ISV.SegmentValue [text], ISN.SegmentName [desc], IsInactive = ISNULL(YCBS.IsInactive,0)
                    from {DbNames.EPYSL}..ItemSegmentValue ISV
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentName ISN ON ISN.SegmentNameID = ISV.SegmentNameID
                    LEFT JOIN {TableNames.YarnCountBasicSetup} YCBS ON YCBS.SegmentValueId = ISV.SegmentValueID
                    WHERE ISN.SegmentName In ('{ItemSegmentNameConstants.YARN_COUNT}') --AND (ISNULL(YCBS.IsInactive,0) = 0)
				)
                --select * from(
                Select * from YCO
                Union All
                Select * from YT
                Union All
                Select * from MP
                Union All
                Select * from SP
                Union All
                Select * from YQP
                Union All
                Select * from YCM
				Union All
                Select * from YC
                --)X 
                --Where YarnTypeSVID=85385 AND ManufacturingProcessSVID=59238 AND SubProcessSVID=65478 AND QualityParameterSVID=59533
                ORDER BY [desc],[text]
                */";

            return sql;
        }

        public static string GetFiberSubProgramCertifications()
        {
            return $@"
                Select * FROM(
                Select CAST(EV.ValueID As varchar) [id], EV.ValueName [text], 'Fiber Type'[desc]
                From {DbNames.EPYSL}..EntityTypeValue EV
                Inner Join {DbNames.EPYSL}..EntityType ET On EV.EntityTypeID = ET.EntityTypeID
                LEFT JOIN {TableNames.FiberBasicSetup} FBS ON FBS.ValueID = EV.ValueID
                Where ET.EntityTypeName = 'Fabric Type' AND ISNULL(FBS.IsInactive,0) = 0
                Group By EV.ValueID,EV.ValueName
                UNION 
                SELECT distinct CAST(ISV.SegmentValueID As varchar) [id], ISV.SegmentValue [text], ISN.SegmentName [desc]
                FROM {DbNames.EPYSL}..ItemSegmentName ISN
                INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISN.SegmentNameID = ISV.SegmentNameID
                Left Join {DbNames.EPYSL}..YarnCountHiddenSetup YCH On YCH.YarnCountID = ISV.SegmentValueID
                LEFT JOIN {TableNames.SubProgramBasicSetup} SBS ON SBS.SegmentValueID = ISV.SegmentValueID
                WHERE ISN.SegmentName In ('Yarn Sub Program New') And ISNULL(ISV.SegmentValue, '') <> '' And YCH.YarnCountID IS NULL
                AND ISNULL(SBS.IsInactive,0) = 0
                UNION
                SELECT distinct CAST(ISV.SegmentValueID As varchar) [id], ISV.SegmentValue [text], ISN.SegmentName [desc]
                FROM {DbNames.EPYSL}..ItemSegmentName ISN
                INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISN.SegmentNameID = ISV.SegmentNameID
                Left Join {DbNames.EPYSL}..YarnCountHiddenSetup YCH On YCH.YarnCountID = ISV.SegmentValueID
                LEFT JOIN {TableNames.CertificationsBasicSetup} CBS ON CBS.SegmentValueID = ISV.SegmentValueID
                WHERE ISN.SegmentName In ('Yarn Certifications') And ISNULL(ISV.SegmentValue, '') <> '' And YCH.YarnCountID IS NULL
                AND ISNULL(CBS.IsInactive,0) = 0
                )FL 
                Order By [desc], [text] ";

        }
        public static string GetItemSegmentValuesFilterMapping()
        {
            return $@";select SV.*,ISV1.SegmentValue YarnType,ISV2.SegmentValue ManufacturingProcess,ISV3.SegmentValue SubProcess,ISV4.SegmentValue QualityParameter,SV.CountUnit [Count]
				from {DbNames.EPYSL}..SegmentValueYarnTypeMappingSetup SV
				LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID=SV.YarnTypeSVID  
				LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID=SV.ManufacturingProcessSVID  
				LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID=SV.SubProcessSVID  
				LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID=SV.QualityParameterSVID  
                ORDER BY SV.SegmentValueMappingID DESC";
        }

        public static string GetUnit()
        {
            return $@"SELECT CAST(UnitID AS VARCHAR) AS id, DisplayUnitDesc AS text
                  FROM {DbNames.EPYSL}..Unit";
        }
        public static string GetAllFiberType()
        {
            return $@"  Select SetupMasterID, FiberTypeID, b.SegmentValue FiberType  
                            From YarnProductSetupMaster a
                        Inner Join  {DbNames.EPYSL}..ItemSegmentValue b on b.SegmentValueID = a.FiberTypeID";
        }
        public static string GetEntityTypesByEntityTypeName(string segmentName)
        {
            return
                $@"SELECT CAST(ValueID AS VARCHAR) id, ValueName text
                 FROM {DbNames.EPYSL}..EntityTypeValue EV
                 Inner Join {DbNames.EPYSL}..EntityType ET On EV.EntityTypeID = ET.EntityTypeID
                 WHERE ET.EntityTypeName = '{segmentName}' AND ValueName <> 'Select'
                 ORDER BY ValueName";
   
        }
        public static string GetItemSegmentValuesBySegmentNamesWithSegmentName()
        {
            return $@"SELECT CAST(ISV.SegmentValueID As varchar) [id], ISV.SegmentValue [text], ISN.SegmentName [desc]
                FROM {DbNames.EPYSL}..ItemSegmentName ISN
                INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISN.SegmentNameID = ISV.SegmentNameID
                Left Join {DbNames.EPYSL}..YarnCountHiddenSetup YCH On YCH.YarnCountID = ISV.SegmentValueID
                WHERE ISN.SegmentName In @SegmentNames And ISNULL(ISV.SegmentValue, '') <> '' And YCH.YarnCountID IS NULL
                ORDER BY ISN.SegmentName";
            /*return $@"SELECT CAST(ISV.SegmentValueID As varchar) [id], ISV.SegmentValue [text], ISN.SegmentName [desc],CV.YarnTypeSegmentValueID [additionalValue]
                FROM {DbNames.EPYSL}..ItemSegmentName ISN
                INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISN.SegmentNameID = ISV.SegmentNameID
                Left Join {DbNames.EPYSL}..YarnCountHiddenSetup YCH On YCH.YarnCountID = ISV.SegmentValueID
                Left Join {DbNames.EPYSL}..SegmentValueYarnTypeMappingSetup CV ON CV.SegmentValueID=ISV.SegmentValueID
                WHERE ISN.SegmentName In @SegmentNames And ISNULL(ISV.SegmentValue, '') <> '' And YCH.YarnCountID IS NULL
                ORDER BY ISN.SegmentName";*/
        }
        /// <summary>
        /// Get entity type values by type name.
        /// </summary>
        /// <param name="entityTypeName"></param>
        /// <returns>Returns array of entity type values.</returns>
        public static string GetEntityTypeValuesOnly(string entityTypeName)
        {
            return $@"
                Select EV.ValueName
                From {DbNames.EPYSL}..EntityTypeValue EV
                Inner Join {DbNames.EPYSL}..EntityType ET On EV.EntityTypeID = ET.EntityTypeID
                Where ET.EntityTypeName = '{entityTypeName}'
                Group By EV.ValueName";
        }
        /// <summary>
        /// Get entity type values by type name.
        /// </summary>
        /// <param name="entityTypeName"></param>
        /// <returns>Returns array of entity type values.</returns>
        public static string GetFabricUsedPart()
        {
            return $@"
                SELECT CAST(FUPartID AS varchar) [id], PartName [text], ConceptSubGroupID [desc]
                FROM {DbNames.EPYSL}..FabricUsedPart WHERE ConceptSubGroupID <> 0";
        }

        public static string GetFabricComponents(string entityTypeName)
        {
            return $@"
                 Select CAST(EV.ValueID As varchar) [id], EV.ValueName [text]
                From {DbNames.EPYSL}..EntityTypeValue EV
                Inner Join {DbNames.EPYSL}..EntityType ET On EV.EntityTypeID = ET.EntityTypeID
		        LEFT JOIN {TableNames.FiberBasicSetup} FBS ON FBS.ValueID = EV.ValueID
                Where ET.EntityTypeName = '{entityTypeName}' AND ISNULL(FBS.IsInactive,0) = 0
                Group By EV.ValueID,EV.ValueName";
        }
        public static string GetYarnShadeBooks()
        {
            return $@"SELECT ShadeCode [id], ShadeCode [text], ContactID [additionalValue]   FROM {TableNames.YARN_SHADE_BOOK}";
        }
        public static string GetContainerList(int supplierID)
        {
            if (supplierID > 0)
                return $@"SELECT csm.ContainerSizeID[id], SizeName[text] FROM {TableNames.CONTAINER_SIZE} cs inner join 
                            {TableNames.CONTAINER_SIZE_CAPACITY_MASTER} csm on cs.ContainerSizeID = csm.ContainerSizeID
                        where supplierid = {supplierID} ";
            else
                return $@"SELECT csm.ContainerSizeID[id], SizeName[text] FROM {TableNames.CONTAINER_SIZE} cs inner join 
                            {TableNames.CONTAINER_SIZE_CAPACITY_MASTER} csm on cs.ContainerSizeID = csm.ContainerSizeID";
            

        }
        public static string GetNewYarnSuppliersForProductSetup(int categoryId)
        {
            return $@"SELECT CAST(C.ContactID AS VARCHAR) AS id, C.Name AS text
                FROM {DbNames.EPYSL}..Contacts C
                INNER JOIN {DbNames.EPYSL}..ContactCategoryChild CCC ON C.ContactID = CCC.ContactID
                INNER JOIN {DbNames.EPYSL}..ContactCategoryHK CHK ON CCC.ContactCategoryID = CHK.ContactCategoryID
                WHERE CHK.ContactCategoryID = {categoryId} And C.ContactId Not In (Select SupplierID From YarnSupplierProductSetup)
                ORDER BY C.Name";
        }

        public static string GetYarnSuppliersForProductSetup(int categoryId)
        {
            return $@"SELECT CAST(C.ContactID AS VARCHAR) AS id, C.Name AS text
                FROM {DbNames.EPYSL}..Contacts C
                INNER JOIN {DbNames.EPYSL}..ContactCategoryChild CCC ON C.ContactID = CCC.ContactID
                INNER JOIN {DbNames.EPYSL}..ContactCategoryHK CHK ON CCC.ContactCategoryID = CHK.ContactCategoryID
                WHERE CHK.ContactCategoryID = {categoryId}
                ORDER BY C.Name";
        }

        public static string GetYarnCountByYarnType(string yarnTypeIds)
        {
            return
                $@"Select CAST(ISV.SegmentValueID As varchar) [id], ISV.SegmentValue [text]
                From {DbNames.EPYSL}..YarnCountSetup YCS
                Inner Join {DbNames.EPYSL}..YarnCountSetupChild YCSC On YCS.YCSID = YCSC.YCSID
                Inner Join {DbNames.EPYSL}..ItemSegmentValue ISV On YCSC.YarnCountID = ISV.SegmentValueID
                Where YCS.YarnTypeID In({yarnTypeIds})
                Group By ISV.SegmentValueID, ISV.SegmentValue";
        }

        public static string GetAllYarnCount()
        {
            return
                $@"Select CAST(ISV.SegmentValueID As varchar) [id], ISV.SegmentValue [text]
                From YarnCountSetup YCS
                Inner Join YarnCountSetupChild YCSC On YCS.YCSID = YCSC.YCSID
                Inner Join ItemSegmentValue ISV On YCSC.YarnCountID = ISV.SegmentValueID
                Group By ISV.SegmentValueID, ISV.SegmentValue";
        }

        public static string GetEntityTypesByEntityTypeId(int entityTypeId)
        {
            return
                $@"SELECT CAST(ValueID AS VARCHAR) id, ValueName text
                        FROM {DbNames.EPYSL}..EntityTypeValue
                        WHERE EntityTypeID = {entityTypeId} AND ValueName <> 'Select'
                        ORDER BY ValueName";
        }

        public static string GetDyeingMachine()
        {
            return
                $@"SELECT	CAST(DyeingMCStatusID AS VARCHAR) AS id, DyeingMCStatus text
                FROM	DyeingMCStatusSetup";
        }

        public static string GetDyeingMCBrand()
        {
            return
                @"SELECT	CAST(DyeingMCBrandID AS VARCHAR) AS id, DyeingMCBrand text
                            FROM	DyeingMCBrandSetup";
        }

        public static string GetLocation()
        {
            return
                $@"Select CAST(L.LocationID AS VARCHAR) id, L.LocationName [text]
                From {DbNames.EPYSL}..ItemSubGroupLocation ISGL
                Left Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = ISGL.SubGroupID
                Left Join {DbNames.EPYSL}..Location L On L.LocationID = ISGL.MLocationID
                Left Join {DbNames.EPYSL}..WareHouse W On W.WareHouseID = L.WareHouseID
                Where L.YarnStore = 1 And ISG.SubGroupName in ('Yarn','Yarn New')
                Group By L.LocationID, L.LocationName, ISGL.SubGroupID , W.CompanyID, ISG.ItemGroupID";
        }

        public static string GetContactsByCategoryWithDefaultContactAsync(string contactCategory, int defaultContactId)
        {
            return
                $@"Select Cast(C.ContactID As varchar) [id], C.Name [text]
                From {DbNames.EPYSL}..Contacts C
                Inner Join {DbNames.EPYSL}..ContactCategoryChild CCC On C.ContactID = CCC.ContactID
                Inner Join {DbNames.EPYSL}..ContactCategoryHK CC ON CC.ContactCategoryID = CCC.ContactCategoryID
                Where CC.ContactCategoryName = '{contactCategory}'
                Union
                Select Cast(ContactID As varchar) [id], Name [text]
                From {DbNames.EPYSL}..Contacts Where ContactID = {defaultContactId}";
        }

        public static string GetContactsByCategoryType(string contactCategory)
        {
            return
                $@"Select Cast(C.ContactID As varchar) [id], C.ShortName [text]
                From {DbNames.EPYSL}..Contacts C
                Inner Join {DbNames.EPYSL}..ContactCategoryChild CCC On C.ContactID = CCC.ContactID
                Inner Join {DbNames.EPYSL}..ContactCategoryHK CC ON CC.ContactCategoryID = CCC.ContactCategoryID
                Where CC.ContactCategoryName = '{contactCategory}' And C.Name != 'Select'
                Order By C.ShortName";
        }

        public static string GetEntityTypeValues()
        {
            return $@"SELECT CAST(ValueID AS VARCHAR) id, ValueName [text], CAST(EntityTypeID AS VARCHAR) [desc]
                FROM {DbNames.EPYSL}..EntityTypeValue
                WHERE ValueName <> 'Select' AND ValueID NOT IN(42,86,216)
                ORDER BY ValueName";
        }

        public static string GetEntityTypeValuesWithTypeName()
        {
            return $@"SELECT CAST(EV.ValueID AS VARCHAR) id, EV.ValueName [text], ET.EntityTypeName [desc]
                FROM {DbNames.EPYSL}..EntityTypeValue EV
                Inner Join {DbNames.EPYSL}..EntityType ET On EV.EntityTypeID = ET.EntityTypeID
                WHERE EV.ValueName <> 'Select' And ISNULL(EV.ValueName, '') <> '' AND EV.ValueID NOT IN(42,86,216)
                ORDER BY EV.ValueName";
        }

        public static string GetEntityTypeValuesByEntityTypesWithTypeName()
        {
            return $@"SELECT CAST(EV.ValueID AS VARCHAR) id, EV.ValueName [text], ET.EntityTypeName [desc]
                FROM {DbNames.EPYSL}..EntityTypeValue EV
                Inner Join {DbNames.EPYSL}..EntityType ET On EV.EntityTypeID = ET.EntityTypeID
                WHERE ET.EntityTypeName In @EntityTypes
                ORDER BY EV.ValueName";
        }


        /// <summary>
        /// Get all item segment values.
        /// </summary>
        /// <returns></returns>
        public static string GetItemSegments()
        {
            return $@"SELECT CAST(ISV.SegmentValueID As varchar) [id], ISV.SegmentValue [text], CAST(ISN.SegmentNameID As varchar) [desc]
                FROM {DbNames.EPYSL}..ItemSegmentName ISN
                INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISN.SegmentNameID = ISV.SegmentNameID
                WHERE ISNULL(ISV.SegmentValue, '') <> ''
                ORDER BY ISV.SegmentValue";
        }

        /// <summary>
        /// Get all item segment values with segment name as description.
        /// </summary>
        /// <returns></returns>
        public static string GetItemSegmentValuesWithSegmentName()
        {
            return $@"SELECT CAST(ISV.SegmentValueID As varchar) [id], ISV.SegmentValue [text], ISN.SegmentName [desc]
                FROM {DbNames.EPYSL}..ItemSegmentName ISN
                INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISN.SegmentNameID = ISV.SegmentNameID
                WHERE ISNULL(ISV.SegmentValue, '') <> ''
                ORDER BY ISN.SegmentName";
        }

        public static string GetSegmentValueYarnTypeMappingSetup()
        {
            return $@"SELECT * FROM {DbNames.EPYSL}..SegmentValueYarnTypeMappingSetup";
        }

        /*
        public static string GetItemSegmentValuesBySegmentNamesWithMapping(string countIds, string compositionIds)
        {
            string countSQL = "";
            string compositionSQL = "";
            if (countIds.IsNotNullOrEmpty()) countSQL += $@" OR ISV.SegmentValueID IN ({countIds})";
            if (compositionIds.IsNotNullOrEmpty()) compositionSQL += $@" OR ISV.SegmentValueID IN ({compositionIds})";

            string sql = $@"
                ;With YCO as(
                    select 0 YarnTypeSVID,0 ManufacturingProcessSVID,0 SubProcessSVID,0 QualityParameterSVID,0 CountSVID,CAST(ISV.SegmentValueID As varchar) [id], ISV.SegmentValue [text], ISN.SegmentName [desc], IsInactive = ISNULL(YCBS.IsInactive,0)
                    from {DbNames.EPYSL}..ItemSegmentValue ISV
                    INNER JOIN {DbNames.EPYSL}..ItemSegmentName ISN ON ISN.SegmentNameID = ISV.SegmentNameID
                    LEFT JOIN {tableNames.YarnCompositionBasicSetup} YCBS ON YCBS.SegmentValueId = ISV.SegmentValueID
                    WHERE ISN.SegmentName In ('Yarn Composition') AND (ISNULL(YCBS.IsInactive,0) = 0 {compositionSQL})
                ),
                YT as (
                    select distinct SV.YarnTypeSVID,0 ManufacturingProcessSVID,0 SubProcessSVID,0 QualityParameterSVID,0 CountSVID,CAST(SV.YarnTypeSVID As varchar) [id], ISV.SegmentValue [text], ISN.SegmentName [desc], IsInactive = 0
                    from {DbNames.EPYSL}..SegmentValueYarnTypeMappingSetup SV
                    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID=SV.YarnTypeSVID    
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentName ISN ON ISN.SegmentNameID = ISV.SegmentNameID 
                    WHERE ISN.SegmentName In ('Yarn Type New')                
                    --ORDER BY ISV.SegmentValue
                ),
                MP as (
                    select distinct SV.YarnTypeSVID,SV.ManufacturingProcessSVID,0 SubProcessSVID,0 QualityParameterSVID,0 CountSVID,CAST(SV.ManufacturingProcessSVID As varchar) [id], ISV.SegmentValue [text], ISN.SegmentName [desc], IsInactive = 0
                    from {DbNames.EPYSL}..SegmentValueYarnTypeMappingSetup SV
                    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID=SV.ManufacturingProcessSVID    
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentName ISN ON ISN.SegmentNameID = ISV.SegmentNameID 
                    WHERE ISN.SegmentName In ('Yarn Manufacturing Process')                
                    --ORDER BY ISV.SegmentValue
                ),
                SP as (
                    select distinct SV.YarnTypeSVID,SV.ManufacturingProcessSVID,SV.SubProcessSVID,0 QualityParameterSVID,0 CountSVID,CAST(SV.SubProcessSVID As varchar) [id], ISV.SegmentValue [text], ISN.SegmentName [desc], IsInactive = 0
                    from {DbNames.EPYSL}..SegmentValueYarnTypeMappingSetup SV
                    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID=SV.SubProcessSVID    
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentName ISN ON ISN.SegmentNameID = ISV.SegmentNameID 
                    WHERE ISN.SegmentName In ('Yarn Manufacturing Sub Process')                
                    --ORDER BY ISV.SegmentValue
                ),
                YQP as (
                    select distinct SV.YarnTypeSVID,SV.ManufacturingProcessSVID,SV.SubProcessSVID,SV.QualityParameterSVID,0 CountSVID,CAST(SV.QualityParameterSVID As varchar) [id], ISV.SegmentValue [text], ISN.SegmentName [desc], IsInactive = 0
                    from {DbNames.EPYSL}..SegmentValueYarnTypeMappingSetup SV
                    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID=SV.QualityParameterSVID    
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentName ISN ON ISN.SegmentNameID = ISV.SegmentNameID 
                    WHERE ISN.SegmentName In ('Yarn Quality Parameter')                
                    --ORDER BY ISV.SegmentValue
                ),
                YCM as (
                    Select YarnTypeSVID,ManufacturingProcessSVID,SubProcessSVID,QualityParameterSVID,0 CountSVID,CAST(0 As varchar) [id],CountUnit [text],'Yarn Count Master' [desc], IsInactive = 0
				    from (
                    select SV.YarnTypeSVID,SV.ManufacturingProcessSVID,SV.SubProcessSVID,SV.QualityParameterSVID,CountUnit from {DbNames.EPYSL}..SegmentValueYarnTypeMappingSetup SV
                    INNER JOIN YQP on SV.YarnTypeSVID=YQP.YarnTypeSVID  AND SV.ManufacturingProcessSVID=YQP.ManufacturingProcessSVID AND SV.SubProcessSVID=YQP.SubProcessSVID AND SV.QualityParameterSVID=YQP.QualityParameterSVID
                ) A
                ),
				YC as(
                    select 0 YarnTypeSVID,0 ManufacturingProcessSVID,0 SubProcessSVID,0 QualityParameterSVID,0 CountSVID,CAST(ISV.SegmentValueID As varchar) [id], ISV.SegmentValue [text], ISN.SegmentName [desc], IsInactive = ISNULL(YCBS.IsInactive,0)
                    from {DbNames.EPYSL}..ItemSegmentValue ISV
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentName ISN ON ISN.SegmentNameID = ISV.SegmentNameID
                    LEFT JOIN {TableNames.YarnCountBasicSetup} YCBS ON YCBS.SegmentValueId = ISV.SegmentValueID
                    WHERE ISN.SegmentName In ('Yarn Count') AND (ISNULL(YCBS.IsInactive,0) = 0 {countSQL})
				)
                --select * from(
                Select * from YCO
                Union All
                Select * from YT
                Union All
                Select * from MP
                Union All
                Select * from SP
                Union All
                Select * from YQP
                Union All
                Select * from YCM
				Union All
                Select * from YC
                --)X 
                --Where YarnTypeSVID=85385 AND ManufacturingProcessSVID=59238 AND SubProcessSVID=65478 AND QualityParameterSVID=59533
                ORDER BY [desc],[text]";

            return sql;
        }
        */
        public static string GetYarnCompositions()
        {
            string sql = $@"SELECT 
                     SegmentValueId = ISV.SegmentValueID
                    ,SegmentValueName = ISV.SegmentValue
                    ,IsInactive = ISNULL(YCBS.IsInactive,0)
                    from {DbNames.EPYSL}..ItemSegmentValue ISV
                    INNER JOIN {DbNames.EPYSL}..ItemSegmentName ISN ON ISN.SegmentNameID = ISV.SegmentNameID
                    LEFT JOIN {TableNames.YarnCompositionBasicSetup} YCBS ON YCBS.SegmentValueId = ISV.SegmentValueID
                    WHERE ISN.SegmentName In ('{ItemSegmentNameConstants.YARN_COMPOSITION}') AND ISNULL(ISV.SegmentValue,'') <> ''
                    AND ISV.SegmentValueID != 93222
                    ORDER BY ISV.SegmentValue";
            return sql;
        }
        public static string GetYarnTypes()
        {
            string sql = $@"SELECT 
                     SegmentValueId = ISV.SegmentValueID
                    ,SegmentValueName = ISV.SegmentValue
                    from {DbNames.EPYSL}..SegmentValueYarnTypeMappingSetup SV
                    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID=SV.YarnTypeSVID    
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentName ISN ON ISN.SegmentNameID = ISV.SegmentNameID 
                    WHERE ISN.SegmentName In ('{ItemSegmentNameConstants.YARN_TYPE}') AND ISNULL(ISV.SegmentValue,'') <> ''             
                    ORDER BY ISV.SegmentValue";

            return sql;
        }
        public static string GetYarnManufacturingProcesses()
        {
            string sql = $@"SELECT 
                     SegmentValueId = ISV.SegmentValueID
                    ,SegmentValueName = ISV.SegmentValue
                    from {DbNames.EPYSL}..SegmentValueYarnTypeMappingSetup SV
                    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID=SV.ManufacturingProcessSVID    
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentName ISN ON ISN.SegmentNameID = ISV.SegmentNameID 
                    WHERE ISN.SegmentName In ('{ItemSegmentNameConstants.YARN_MANUFACTURING_PROCESS}') AND ISNULL(ISV.SegmentValue,'') <> ''            
                    ORDER BY ISV.SegmentValue";

            return sql;
        }
        public static string GetYarnManufacturingSubProcesses()
        {
            string sql = $@"SELECT 
                     SegmentValueId = ISV.SegmentValueID
                    ,SegmentValueName = ISV.SegmentValue
                    from {DbNames.EPYSL}..SegmentValueYarnTypeMappingSetup SV
                    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID=SV.SubProcessSVID    
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentName ISN ON ISN.SegmentNameID = ISV.SegmentNameID 
                    WHERE ISN.SegmentName In ('{ItemSegmentNameConstants.YARN_MANUFACTURING_SUB_PROCESS}') AND ISNULL(ISV.SegmentValue,'') <> ''            
                    ORDER BY ISV.SegmentValue";

            return sql;
        }
        public static string GetYarnQualityParameters()
        {
            string sql = $@"SELECT 
                     SegmentValueId = ISV.SegmentValueID
                    ,SegmentValueName = ISV.SegmentValue
                    from {DbNames.EPYSL}..SegmentValueYarnTypeMappingSetup SV
                    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISV.SegmentValueID=SV.QualityParameterSVID    
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentName ISN ON ISN.SegmentNameID = ISV.SegmentNameID 
                    WHERE ISN.SegmentName In ('{ItemSegmentNameConstants.YARN_QUALITY_PARAMETER}') AND ISNULL(ISV.SegmentValue,'') <> ''        
                    ORDER BY ISV.SegmentValue";
            return sql;
        }
        public static string GetYarnCounts()
        {
            string sql = $@"SELECT 
                     SegmentValueId = ISV.SegmentValueID
                    ,SegmentValueName = ISV.SegmentValue
                    ,IsInactive = ISNULL(YCBS.IsInactive,0)
                    from {DbNames.EPYSL}..ItemSegmentValue ISV
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentName ISN ON ISN.SegmentNameID = ISV.SegmentNameID
                    LEFT JOIN {TableNames.YarnCountBasicSetup} YCBS ON YCBS.SegmentValueId = ISV.SegmentValueID AND ISNULL(ISV.SegmentValue,'') <> ''
                    WHERE ISN.SegmentName In ('{ItemSegmentNameConstants.YARN_COUNT}')
                    ORDER BY ISV.SegmentValue";
            return sql;
        }
        public static string GetAllFibers()
        {
            string sql = $@"Select CAST(EV.ValueID As varchar) SegmentValueId, 
                    EV.ValueName SegmentValueName,
                    IsInactive = ISNULL(YCBS.IsInactive,0)
                    From {DbNames.EPYSL}..EntityTypeValue EV
                    Inner Join {DbNames.EPYSL}..EntityType ET On EV.EntityTypeID = ET.EntityTypeID
                    LEFT JOIN {TableNames.FiberBasicSetup} YCBS ON YCBS.ValueId = EV.ValueID
                    Where ET.EntityTypeName = '{ItemSegmentNameConstants.FABRIC_TYPE}' --AND ISNULL(EV.ValueName,'') <> ''";
            return sql;
        }
        public static string GetAllSubPrograms()
        {
            string sql = $@"SELECT 
                     SegmentValueId = ISV.SegmentValueID
                    ,SegmentValueName = ISV.SegmentValue
                    ,IsInactive = ISNULL(YCBS.IsInactive,0)
                    from {DbNames.EPYSL}..ItemSegmentValue ISV
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentName ISN ON ISN.SegmentNameID = ISV.SegmentNameID
                    LEFT JOIN {TableNames.SubProgramBasicSetup} YCBS ON YCBS.SegmentValueId = ISV.SegmentValueID
                    WHERE ISN.SegmentName In ('{ItemSegmentNameConstants.YARN_SUBPROGRAM_NEW}') --AND ISNULL(ISV.SegmentValue,'') <> ''
                    ORDER BY ISV.SegmentValue";
            return sql;
        }
        public static string GetAllCertifications()
        {
            string sql = $@"SELECT 
                     SegmentValueId = ISV.SegmentValueID
                    ,SegmentValueName = ISV.SegmentValue
                    ,IsInactive = ISNULL(YCBS.IsInactive,0)
                    from {DbNames.EPYSL}..ItemSegmentValue ISV
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentName ISN ON ISN.SegmentNameID = ISV.SegmentNameID
                    LEFT JOIN {TableNames.CertificationsBasicSetup} YCBS ON YCBS.SegmentValueId = ISV.SegmentValueID
                    WHERE ISN.SegmentName In ('{ItemSegmentNameConstants.YARN_CERTIFICATIONS}') --AND ISNULL(ISV.SegmentValue,'') <> ''
                    ORDER BY ISV.SegmentValue";
            return sql;
        }
        public static string GetItemSegmentValuesBySegmentNamesWithMappingAdmin()
        {
            return $@"
                ;With YCO as(
                select 0 YarnTypeSVID,0 ManufacturingProcessSVID,0 SubProcessSVID,0 QualityParameterSVID,0 CountSVID,CAST(ISV.SegmentValueID As varchar) [id], ISV.SegmentValue [text], ISN.SegmentName [desc]
                from {DbNames.EPYSL}..ItemSegmentValue ISV
                INNER JOIN  {DbNames.EPYSL}..ItemSegmentName ISN ON ISN.SegmentNameID = ISV.SegmentNameID 
                WHERE ISN.SegmentName In ('Yarn Composition Live') 
                ),
                YT as(
                select distinct isnull(SV.YarnTypeSVID,0)YarnTypeSVID,0 ManufacturingProcessSVID,0 SubProcessSVID,0 QualityParameterSVID,0 CountSVID,CAST(ISV.SegmentValueID As varchar) [id], ISV.SegmentValue [text], ISN.SegmentName [desc]
                from {DbNames.EPYSL}..ItemSegmentValue ISV
				LEFT JOIN {DbNames.EPYSL}..SegmentValueYarnTypeMappingSetup SV  ON ISV.SegmentValueID=SV.QualityParameterSVID 
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentName ISN ON ISN.SegmentNameID = ISV.SegmentNameID 
                WHERE ISN.SegmentName In ('Yarn Type Live')                
                --ORDER BY ISV.SegmentValue
                ),
                MP as(
                 select distinct isnull(SV.YarnTypeSVID,0)YarnTypeSVID,isnull(SV.ManufacturingProcessSVID,0)ManufacturingProcessSVID,0 SubProcessSVID,0 QualityParameterSVID,0 CountSVID,CAST(ISV.SegmentValueID As varchar) [id], ISV.SegmentValue [text], ISN.SegmentName [desc]
                from {DbNames.EPYSL}..ItemSegmentValue ISV
				LEFT JOIN {DbNames.EPYSL}..SegmentValueYarnTypeMappingSetup SV  ON ISV.SegmentValueID=SV.QualityParameterSVID 
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentName ISN ON ISN.SegmentNameID = ISV.SegmentNameID 
                WHERE ISN.SegmentName In ('Yarn Manufacturing Process Live')                   
                --ORDER BY ISV.SegmentValue
                ),
                SP as(
                select distinct SV.YarnTypeSVID,SV.ManufacturingProcessSVID,SV.SubProcessSVID,0 QualityParameterSVID,0 CountSVID,CAST(ISV.SegmentValueID As varchar) [id], ISV.SegmentValue [text], ISN.SegmentName [desc]
                from {DbNames.EPYSL}..ItemSegmentValue ISV 
				LEFT JOIN {DbNames.EPYSL}..SegmentValueYarnTypeMappingSetup SV  ON ISV.SegmentValueID=SV.QualityParameterSVID 
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentName ISN ON ISN.SegmentNameID = ISV.SegmentNameID 
                WHERE ISN.SegmentName In ('Yarn Manufacturing Sub Process Live')                
                --ORDER BY ISV.SegmentValue
                ),
                YQP as(
                select distinct isnull(SV.YarnTypeSVID,0)YarnTypeSVID,isnull(SV.ManufacturingProcessSVID,0)ManufacturingProcessSVID,isnull(SV.SubProcessSVID,0)SubProcessSVID,isnull(SV.QualityParameterSVID,0)QualityParameterSVID,0 CountSVID,CAST(ISV.SegmentValueID As varchar) [id], ISV.SegmentValue [text], ISN.SegmentName [desc]
                from {DbNames.EPYSL}..ItemSegmentValue ISV  
				LEFT JOIN {DbNames.EPYSL}..SegmentValueYarnTypeMappingSetup SV  ON ISV.SegmentValueID=SV.QualityParameterSVID 
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentName ISN ON ISN.SegmentNameID = ISV.SegmentNameID 
                WHERE ISN.SegmentName In ('Yarn Quality Parameter Live')                
                --ORDER BY ISV.SegmentValue
                ),
                YCM as (
                Select YarnTypeSVID,ManufacturingProcessSVID,SubProcessSVID,QualityParameterSVID,0 CountSVID,CAST(0 As varchar) [id],CountUnit [text],'Yarn Count Master' [desc]
				from (
                select SV.YarnTypeSVID,SV.ManufacturingProcessSVID,SV.SubProcessSVID,SV.QualityParameterSVID,CountUnit from {DbNames.EPYSL}..SegmentValueYarnTypeMappingSetup SV
                INNER JOIN YQP on SV.YarnTypeSVID=YQP.YarnTypeSVID  AND SV.ManufacturingProcessSVID=YQP.ManufacturingProcessSVID AND SV.SubProcessSVID=YQP.SubProcessSVID AND SV.QualityParameterSVID=YQP.QualityParameterSVID
                )A
                ),
				YC as(
				select 0 YarnTypeSVID,0 ManufacturingProcessSVID,0 SubProcessSVID,0 QualityParameterSVID,0 CountSVID,CAST(ISV.SegmentValueID As varchar) [id], ISV.SegmentValue [text], ISN.SegmentName [desc]
                from {DbNames.EPYSL}..ItemSegmentValue ISV
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentName ISN ON ISN.SegmentNameID = ISV.SegmentNameID
                WHERE ISN.SegmentName In ('{ItemSegmentNameConstants.YARN_COUNT}')  
				)
                --select * from(
                Select * from YCO
                Union All
                Select * from YT
                Union All
                Select * from MP
                Union All
                Select * from SP
                Union All
                Select * from YQP
                Union All
                Select * from YCM
				Union All
                Select * from YC
                --)X 
                --Where YarnTypeSVID=85385 AND ManufacturingProcessSVID=59238 AND SubProcessSVID=65478 AND QualityParameterSVID=59533
                ORDER BY [desc],[text]";

        }

   
        public static string GetFiberSubProgramCertificationsFilterMapping()
        {
            return $@";select SV.*,ETV.ValueName Fiber,ISV2.SegmentValue SubProgram,ISV3.SegmentValue Certifications
				from {TableNames.FIBER_SUBPROGRAM_CERTIFICATIONS_FILTER_SETUP} SV
				LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID=SV.FiberID  
				LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID=SV.SubProgramID  
				LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID=SV.CertificationsID
                ORDER BY SV.SetupID DESC";

        }
        public static string GetCertifications()
        {
            return $@"SELECT distinct CAST(ISV.SegmentValueID As varchar) [id], ISV.SegmentValue [text], ISN.SegmentName [desc],isnull(CAST(FCMS.SubProgramID As varchar),'0')additionalValue, isnull(CAST(FCMS.FiberID As varchar),'0')additionalValue2
                FROM {DbNames.EPYSL}..ItemSegmentName ISN
                INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISN.SegmentNameID = ISV.SegmentNameID
                Left Join {DbNames.EPYSL}..YarnCountHiddenSetup YCH On YCH.YarnCountID = ISV.SegmentValueID
                LEFT JOIN {TableNames.FIBER_SUBPROGRAM_CERTIFICATIONS_FILTER_SETUP} FCMS on FCMS.CertificationsID=ISV.SegmentValueID
		        LEFT JOIN {TableNames.CertificationsBasicSetup} CBS ON CBS.SegmentValueID = ISV.SegmentValueID
                WHERE ISN.SegmentName = '{ItemSegmentNameConstants.YARN_CERTIFICATIONS}' And ISNULL(ISV.SegmentValue, '') <> '' And YCH.YarnCountID IS NULL
		        	AND ISNULL(CBS.IsInactive,0) = 0
                ORDER BY ISV.SegmentValue";

        }
        public static string GetSubPrograms()
        {
            return $@"SELECT distinct CAST(ISV.SegmentValueID As varchar) [id], ISV.SegmentValue [text], ISN.SegmentName [desc],isnull(CAST(FCMS.FiberID As varchar),'0')additionalValue
                FROM {DbNames.EPYSL}..ItemSegmentName ISN
                INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISN.SegmentNameID = ISV.SegmentNameID
                Left Join {DbNames.EPYSL}..YarnCountHiddenSetup YCH On YCH.YarnCountID = ISV.SegmentValueID
                LEFT JOIN {TableNames.FIBER_SUBPROGRAM_CERTIFICATIONS_FILTER_SETUP} FCMS on FCMS.SubProgramID=ISV.SegmentValueID
		        LEFT JOIN {TableNames.SubProgramBasicSetup} SBS ON SBS.SegmentValueID = ISV.SegmentValueID
                WHERE ISN.SegmentName = '{ItemSegmentNameConstants.YARN_SUBPROGRAM_NEW}' And ISNULL(ISV.SegmentValue, '') <> '' And YCH.YarnCountID IS NULL
			        AND ISNULL(SBS.IsInactive,0) = 0 
                ORDER BY ISV.SegmentValue";

        }
        public static string GetYarnDyeingFor()
        {
            return $@"SELECT CAST(YDyeingForID As varchar) [id], YDyeingFor [text]
                FROM YarnDyeingFor_HK";
        }
        public static string GetCacheResetSetups()
        {
            return $@"SELECT * FROM CacheResetSetup;";
        }
        /// <summary>
        /// Get item segment values by segment name ids with segment name id as description.
        /// </summary>
        /// <param name="segmentNameIds">Segment name ids should be passed as "1, 2, 3" etc</param>
        /// <returns></returns>
        public static string GetItemSegmentsBySegmentNameIds(string segmentNameIds)
        {
            return $@"SELECT CAST(ISV.SegmentValueID As varchar) [id], ISV.SegmentValue [text], CAST(ISN.SegmentNameID As varchar) [desc]
                FROM {DbNames.EPYSL}..ItemSegmentName ISN
                INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISN.SegmentNameID = ISV.SegmentNameID
                WHERE ISN.SegmentNameID In({segmentNameIds}) AND ISNULL(ISV.SegmentValue, '') <> '' And SegmentValueID NOT IN(17725,5127,1616,1969,47221,2117,1616,1621,44382,44384,45969,46717,47220, 2155) --2155 for Ring Yarn Type which is not Needed by Supply Chain dept dated 11-02-2020
                ORDER BY ISV.SegmentValue";
        }

        /// <summary>
        /// Get item segment values of an Item Segment.
        /// </summary>
        /// <param name="segmentName">Segment name.</param>
        /// <returns>Sql query.</returns>
        public static string GetItemSegmentValuesBySegmentName(string segmentName)
        {
            return $@"Select CAST(ISV.SegmentValueID AS nvarchar) id, ISV.SegmentValue [text]
                From {DbNames.EPYSL}..ItemSegmentValue ISV
                Inner Join {DbNames.EPYSL}..ItemSegmentName ISN On ISV.SegmentNameID = ISN.SegmentNameID
                Where ISN.SegmentName = '{segmentName}'";
        }

        public static string GetItemSegmentValuesBySubGroupId(int subGroupId)
        {
            return $@"Select Cast(ISV.SegmentValueID as varchar) id, ISV.SegmentValue [text], ISN.SegmentName [desc], Cast(ISN.SegmentNameId as varchar) [additionalValue]
            From {DbNames.EPYSL}..ItemStructure IST
            Inner Join {DbNames.EPYSL}..ItemSegmentName ISN On IST.SegmentNameID = ISN.SegmentNameID
            Inner Join {DbNames.EPYSL}..ItemSegmentValue ISV On ISV.SegmentNameID = ISN.SegmentNameID
            Where SubGroupID = {subGroupId}
            Order By IST.SegmentSeqNo";
        }

        /// <summary>
        /// Get Item Structure to display table.
        /// </summary>
        /// <param name="subGroupId"></param>
        /// <returns>Data of type <see cref="ItemStructureDTO"/></returns>
        public static string GetItemStructureBySubGroupId(int subGroupId)
        {
            return $@"Select ISN.SegmentName SegmentDisplayName
	                , 'Segment' + Cast(ROW_NUMBER() Over (Order By IST.SegmentSeqNo) As varchar) + 'ValueDesc' SegmentValueDescName
	                , 'Segment' + Cast(ROW_NUMBER() Over (Order By IST.SegmentSeqNo) As varchar) + 'ValueId' SegmentValueIdName
                From {DbNames.EPYSL}..ItemStructure IST
                Left Join {DbNames.EPYSL}..ItemSegmentName ISN On IST.SegmentNameID = ISN.SegmentNameID
                Where SubGroupID = {subGroupId}
                Order By IST.SegmentSeqNo";
        }

        /// <summary>
        /// Get Item Structure to display table.
        /// </summary>
        /// <param name="subGroupId">Item SubGroup Id</param>
        /// <returns>Data of type <see cref="ItemStructureDTO"/></returns>
        public static string GetItemStructureBySubGroupForAllItem(int subGroupId)
        {
            return $@"Select IST.SegmentNameID, IST.AllowAdd, IST.IsNumericValue, ISN.SegmentName SegmentDisplayName
                    , 'Segment' + Cast(ROW_NUMBER() Over (Order By IST.SegmentSeqNo) As varchar) + 'ValueDesc' SegmentValueDescName
                    , 'Segment' + Cast(ROW_NUMBER() Over (Order By IST.SegmentSeqNo) As varchar) + 'ValueId' SegmentValueIdName
                    , IST.HasDefaultValue, IST.SegmentValueID
                From {DbNames.EPYSL}..ItemStructure IST
                Left Join {DbNames.EPYSL}..ItemSegmentName ISN On IST.SegmentNameID = ISN.SegmentNameID
                Where SubGroupID = {subGroupId}
                Order By IST.SegmentSeqNo";
        }

        /// <summary>
        /// Get Item Structure to display table.
        /// </summary>
        /// <param name="subGroupName">Item Sub Group Name</param>
        /// <returns>Data of type <see cref="ItemStructureDTO"/></returns>
        public static string GetItemStructureBySubGroupForAllItem(string subGroupName)
        {
            return $@"Select IST.SegmentNameID, IST.AllowAdd, IST.IsNumericValue, ISN.SegmentName SegmentDisplayName
                    , 'Segment' + Cast(ROW_NUMBER() Over (Order By IST.SegmentSeqNo) As varchar) + 'ValueDesc' SegmentValueDescName
                    , 'Segment' + Cast(ROW_NUMBER() Over (Order By IST.SegmentSeqNo) As varchar) + 'ValueId' SegmentValueIdName
                    , IST.HasDefaultValue, IST.SegmentValueID
                From {DbNames.EPYSL}..ItemStructure IST
                Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = IST.SubGroupID
                Left Join {DbNames.EPYSL}..ItemSegmentName ISN On IST.SegmentNameID = ISN.SegmentNameID
                Where ISG.SubGroupName = '{subGroupName}'
                Order By IST.SegmentSeqNo";
        }

        public static string GetYarnProgramsBySupplier(int supplierId)
        {
            return $@"With
                YSP As (
	                Select YSYPS.ProgramID
	                From YarnSupplierProductSetup YPS
	                Inner Join YarnSupplierYarnProgramSetup YSYPS On YPS.YSProductSetupID = YSYPS.YSProductSetupID
	                Where SupplierID = {supplierId}
                )

                SELECT CAST(EV.ValueID AS VARCHAR) id, EV.ValueName [text]
                FROM YSP
                Inner Join {DbNames.EPYSL}..EntityTypeValue EV On YSP.ProgramID = EV.ValueID
                ORDER BY ValueName";
        }

        public static string GetYarnSubProgramsBySupplier(int supplierId)
        {
            return $@"With
                YSP As (
	                Select YSYPS.SubProgramID
	                From YarnSupplierProductSetup YPS
	                Inner Join YarnSupplierYarnSubProgramSetup YSYPS On YPS.YSProductSetupID = YSYPS.YSProductSetupID
	                Where SupplierID = {supplierId}
                )

                SELECT CAST(EV.ValueID AS VARCHAR) id, EV.ValueName [text]
                FROM YSP
                Inner Join {DbNames.EPYSL}..EntityTypeValue EV On YSP.SubProgramID = EV.ValueID
                ORDER BY ValueName";
        }

        public static string GetYarnTypesBySupplier(int supplierId)
        {
            return $@"With
                YSP As (
	                Select Y.YarnTypeID
	                From YarnSupplierProductSetup YPS
	                Inner Join YarnSupplierYarnTypeSetup Y On YPS.YSProductSetupID = Y.YSProductSetupID
	                Where SupplierID = {supplierId}
                )

                SELECT CAST(ISV.SegmentValueID As varchar) [id], ISV.SegmentValue [text]
                FROM YSP
                INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON YSP.YarnTypeID = ISV.SegmentValueID
                ORDER BY ISV.SegmentValue";
        }

        public static string GetYarnCountBySupplier(int supplierId, int yarnTypeId)
        {
            return $@"With
                Y As (
	                Select Y.YarnCountID
	                From YarnSupplierProductSetup YPS
	                Inner Join YarnSupplierYarnCountSetup Y On YPS.YSProductSetupID = Y.YSProductSetupID
	                Where SupplierID = {supplierId}
                )

                Select CAST(ISV.SegmentValueID As varchar) [id], ISV.SegmentValue [text]
                From  Y
                Inner Join {DbNames.EPYSL}..YarnCountSetupChild YCSC On Y.YarnCountID = YCSC.YarnCountID
                Inner Join {DbNames.EPYSL}..YarnCountSetup YCS On YCSC.YCSID = YCS.YCSID
                Inner Join {DbNames.EPYSL}..ItemSegmentValue ISV On Y.YarnCountID = ISV.SegmentValueID
                Where YCS.YarnTypeID = {yarnTypeId}
                Group By ISV.SegmentValueID, ISV.SegmentValue";
        }

        public static string GetYarnSuppliers()
        {
            return $@"Select Cast(C.ContactID as varchar) [id], C.ShortName [text]
                From {DbNames.EPYSL}..SupplierItemGroupStatus ST
                Inner Join {DbNames.EPYSL}..ContactBusinessType CBT On ST.ContactID = CBT.ContactID
                Inner Join {DbNames.EPYSL}..BusinessType BT On BT.TypeID = CBT.TypeID
                Inner Join {DbNames.EPYSL}..Contacts C On C.ContactID = ST.ContactID
                Inner Join {DbNames.EPYSL}..ContactCategoryChild CCC On CCC.ContactID = ST.ContactID
                Inner Join {DbNames.EPYSL}..ContactCategoryHK CHK On CHK.ContactCategoryID = CCC.ContactCategoryID
                INNER JOIN {DbNames.EPYSL}..ItemSubGroup I ON I.SubGroupID = ST.SubGroupID
                Where BT.TypeName = 'Manufacturer' And CHK.ContactCategoryName = '{ContactCategoryNames.SUPPLIER}' AND  I.SubGroupName = 'Yarn'
                ORDER BY C.ShortName";
        }

        public static string GetYarnSpinners()
        {
            return $@"Select Cast(C.ContactID as varchar) [id], C.ShortName [text]
            From {DbNames.EPYSL}..SupplierItemGroupStatus ST
            Inner Join {DbNames.EPYSL}..Contacts C On C.ContactID = ST.ContactID
            Inner Join {DbNames.EPYSL}..ContactCategoryChild CCC On CCC.ContactID = ST.ContactID
            Inner Join {DbNames.EPYSL}..ContactCategoryHK CHK On CHK.ContactCategoryID = CCC.ContactCategoryID
            Inner Join {DbNames.EPYSL}..ItemSubGroup I ON I.SubGroupID = ST.SubGroupID
            Where CHK.ContactCategoryName = '{ContactCategoryNames.SPINNER}' AND I.SubGroupName = 'Yarn'
            ORDER BY C.ShortName";
        }
        public static string GetStockTypes(string stockTypeIds = "")
        {
            if (stockTypeIds.IsNotNullOrEmpty()) stockTypeIds = $@" AND S.StockTypeId IN ({stockTypeIds}) ";

            return $@"SELECT id = S.StockTypeId, text = S.Name
                        FROM StockType S
                        WHERE StockTypeId NOT IN (9) {stockTypeIds}
                        ORDER BY S.Name";
        }

        public static string GetYarnSupplierDetails()
        {
            return $@"Select
                    C.Name Supplier, C.ShortName, C.AgentName, C.Officeaddress, C.FactoryAddress, C.PhoneNo, C.FaxNo, C.EmailNo, C.ContactPerson, CAI.VATRegNo, CAI.TradeLicenceNo, CAI.BOIRegNo, CAI.BondedWareHouse,
                    CAI.BWHExpiryDate, CAI.InHouse, CAI.InLand, CAI.EPZ, CAI.TransportFacility, CAI.HasNotifyParty, CAI.DiscountPercent, CAI.PaymentType, CAI.CreditDays, CAI.CreditLimit, CAI.Commission, CAI.LCStatus,
                    ETV.ValueName PortOfLoading, POD.ValueName PortOfDischarge, SM.ValueName ShipmentMode, PaymentTermsName = dbo.[fnPaymentTermsNameByContactID] (C.ContactID), IncoTermsName = dbo.fnIncoTermsNameByContactID(C.ContactID),
                    Cty.CountryName Country 
                    From {DbNames.EPYSL}..SupplierItemGroupStatus ST
                    Inner Join {DbNames.EPYSL}..ContactBusinessType CBT On ST.ContactID = CBT.ContactID
                    Inner Join {DbNames.EPYSL}..BusinessType BT On BT.TypeID = CBT.TypeID
                    Inner Join {DbNames.EPYSL}..Contacts C On C.ContactID = ST.ContactID
                    Inner Join {DbNames.EPYSL}..ContactAdditionalInfo CAI On CAI.ContactID = C.ContactID
                    Inner Join {DbNames.EPYSL}..ContactCategoryChild CCC On CCC.ContactID = ST.ContactID
                    Inner Join {DbNames.EPYSL}..ContactCategoryHK CHK On CHK.ContactCategoryID = CCC.ContactCategoryID
                    INNER JOIN {DbNames.EPYSL}..ItemSubGroup I ON I.SubGroupID = ST.SubGroupID
                    Left Join {DbNames.EPYSL}..EntityTypeValue etv On etv.ValueID = CAI.PortOfLoadingID
                    Left Join {DbNames.EPYSL}..EntityTypeValue POD On POD.ValueID = CAI.PortOfDischargeID
                    Left Join {DbNames.EPYSL}..EntityTypeValue SM On SM.ValueID = CAI.ShipmentModeID
                    Left Join {DbNames.EPYSL}..Country cty On cty.CountryID = C.CountryID
                    Where BT.TypeName = 'Manufacturer' And CHK.ContactCategoryName = '{ContactCategoryNames.SUPPLIER}' AND  I.SubGroupName = 'Yarn'";
        }
        public static string GetDyesChemicals()
        {
            return $@"SELECT CAST(SubGroupID AS VARCHAR)as id, SubGroupName as text FROM {DbNames.EPYSL}..ITEMSUBGROUP
            WHERE SubGroupName='Dyes' OR SubGroupName='Chemicals'
            order by SeqNo";
        }

        public static string GetPortOfDischargeByCompany(int companyId)
        {
            return $@"Select CAST(ContactAddressID as varchar) [id], CA.ExternalID [text]
                From {DbNames.EPYSL}..ContactAddress CA
                Inner Join {DbNames.EPYSL}..Contacts C On C.ContactID = CA.ContactID
                Inner Join {DbNames.EPYSL}..CompanyEntity CE On C.MappingCompanyID = CE.CompanyID
                Where CompanyID = {companyId}";
        }

        public static string GetRackListByLocationID(int locationId)
        {
            return $@"SELECT  cast(RackID as varchar) id,RackNo [text]
                    FROM {DbNames.EPYSL}.[dbo].[Rack]
                    ----WHERE LocationID={locationId}
                    ORDER BY Sequence";
        }

        public static string GetRackListByCompanyID(int companyId, string rackFor = "")
        {
            string exSQL = "";
            if (rackFor != "")
            {
                exSQL += $@" AND RackFor = '{rackFor}' ";
            }

            return $@"SELECT  cast(R.RackID as varchar) id,R.RackNo [text]
                    From {DbNames.EPYSL}..Rack R
                    Inner Join {DbNames.EPYSL}..Location L On L.LocationID = R.LocationID
                    Inner Join {DbNames.EPYSL}..WareHouse W On W.WareHouseID = L.WareHouseID
                    Where W.CompanyID = {companyId} {exSQL}
                    ORDER BY R.Sequence";
        }
        public static string GetLocationListByCompanyID(int companyId, bool isYarnStore = false)
        {
            string sql = string.Empty;
            if (isYarnStore) sql = " AND L.YarnStore = 1 ";
            return $@"SELECT  cast(L.LocationID as varchar) id, L.ShortName [text]
                    From {DbNames.EPYSL}..Location L 
                    Inner Join {DbNames.EPYSL}..WareHouse W On W.WareHouseID = L.WareHouseID
                    Where W.CompanyID = {companyId} {sql}";
        }

        public static string GetEmployeeByDepartmentAndSectionList(string deptName, string sectionName)
        {
            return $@"SELECT e.DisplayEmployeeCode [id]
                        ,e.EmployeeName + ' (' + e.DisplayEmployeeCode + ')' [text]
                    FROM {DbNames.EPYSL}..Employee e
                    Inner Join {DbNames.EPYSL}..EmployeeDepartment d on d.DepertmentID = e.DepertmentID
                    Inner Join {DbNames.EPYSL}..EmployeeSection s on s.SectionID = e.SectionID
                    where d.DepertmentDescription = '{deptName}' And s.SectionName in ('{sectionName}')";
        }

        public static string GetEmployeeList()
        {
            return $@"SELECT [DisplayEmployeeCode] [id]
                        ,[EmployeeName] + ' (' + [DisplayEmployeeCode] + ')' [text]
                    FROM {DbNames.EPYSL}..[Employee]";
        }

        public static string GetCompany()
        {
            return $@"SELECT CAST(CompanyID AS VARCHAR) AS id, CompanyName AS text
            FROM {DbNames.EPYSL}..CompanyEntity
			WHERE CompanyID IN(8,6)
			ORDER BY CompanyName";
        }
        public static string GetBanks()
        {
            return $@"SELECT id = BankMasterID, text = BankMasterName 
                 FROM {DbNames.EPYSL}..BankMaster 
                 ORDER BY BankShortName;";
        }

        public static string GetKnittingUnit()
        {
            return $@"select cast(KnittingUnitID as varchar) id,UnitName [text] from KnittingUnit WHERE IsKnitting = 0 order by UnitName";
        }

        public static string GetCompanyName(int id)
        {
            return $@"Select C.Name + '(' + C.ShortName + ')' CompanyName
                From (select SubGroupID, ContactID from {DbNames.EPYSL}..SupplierItemGroupStatus Group By SubGroupID, ContactID) SIGS
                Inner Join {DbNames.EPYSL}..Contacts C On SIGS.ContactID = C.ContactID
                Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = SIGS.SubGroupID
                Inner Join {DbNames.EPYSL}..ContactAdditionalInfo CAI On CAI.ContactID = SIGS.ContactID
                Inner Join {DbNames.EPYSL}..CompanyEntity CE On C.MappingCompanyID = CE.CompanyID
                Where ISG.SubGroupName = 'Fabric' And Isnull(CAI.InHouse,0) = 1 and CE.CompanyID={id}
                Group by CE.CompanyID, C.Name, C.ShortName";
        }

        public static string GetSupplier()
        {
            return $@"Select Cast(C.ContactID As varchar) [id], C.Name [text], CC.ContactCategoryName [desc]
                From {DbNames.EPYSL}..Contacts C
                Inner Join {DbNames.EPYSL}..ContactCategoryChild CCC On C.ContactID = CCC.ContactID
                Inner Join {DbNames.EPYSL}..ContactCategoryHK CC ON CC.ContactCategoryID = CCC.ContactCategoryID
                Where CC.ContactCategoryName IN ('{ContactCategoryNames.SUPPLIER}')";
        }

        public static string GetSpinner()
        {
            return $@"Select Cast(C.ContactID As varchar) [id], C.Name [text], CC.ContactCategoryName [desc]
                From {DbNames.EPYSL}..Contacts C
                Inner Join {DbNames.EPYSL}..ContactCategoryChild CCC On C.ContactID = CCC.ContactID
                Inner Join {DbNames.EPYSL}..ContactCategoryHK CC ON CC.ContactCategoryID = CCC.ContactCategoryID
                Where CC.ContactCategoryName IN ('{ContactCategoryNames.SPINNER}')
                ORDER BY C.Name";
        }

        public static string GetEWOForYarnPO(string buyerIds)
        {
            return $@"
            With 
            EOM As (Select * From {DbNames.EPYSL}..ExportOrderMaster Where BuyerID In ({buyerIds}))
            , SBM As (Select * From {DbNames.EPYSL}..SampleBookingMaster Where BuyerID In ({buyerIds}))
            , F As (
	            SELECT ExportOrderID, ExportOrderNo EWONo, 0 IsSample, EOM.BuyerID, EOM.BuyerTeamID, C.ShortName BuyerName, CCT.TeamName BuyerTeam
	            FROM EOM
	            INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = EOM.BuyerID
	            INNER JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = EOM.BuyerTeamID
	            INNER JOIN {DbNames.EPYSL}..StyleMaster ON StyleMaster.StyleMasterID = EOM.StyleMasterID
	            WHERE EOM.EWOStatusID = 130 
	            Union
	            Select SBM.BookingID ExportOrderID, SBM.BookingNo EWONo, 1 IsSample, SBM.BuyerID, SBM.BuyerTeamID, C.ShortName BuyerName, CCT.TeamName BuyerTeam
	            From SBM
	            Inner Join {DbNames.EPYSL}..SampleType ST On SBM.SampleID = ST.SampleTypeID
	            INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = SBM.BuyerID
	            Inner Join {DbNames.EPYSL}..ContactCategoryTeam CCT On SBM.BuyerTeamID = CCT.CategoryTeamID
	            Where ST.DisplayCode != 'PPS' And BookingDate > '2020-01-01'
            )

            Select * From F";
        }

        public static string GetContactsByCategoryId(int contactCategoryId)
        {
            return $@"SELECT CAST(Contacts.ContactID AS VARCHAR) AS id, Contacts.ShortName AS text
                FROM {DbNames.EPYSL}..Contacts
                INNER JOIN {DbNames.EPYSL}..ContactCategoryChild ON Contacts.ContactID = ContactCategoryChild.ContactID
                INNER JOIN {DbNames.EPYSL}..ContactCategoryHK ON ContactCategoryChild.ContactCategoryID = ContactCategoryHK.ContactCategoryID
                WHERE ContactCategoryHK.ContactCategoryID = {contactCategoryId}
                ORDER BY Contacts.ShortName";
        }

        public static string GetYarnAssessmentStatus()
        {
            return $@"Select Cast('Approve' As varchar) [id],'Approve'[text]
                Union  
                Select Cast('ReTest' As varchar) [id],'ReTest'[text]
                --Union  
                --Select Cast('Reject' As varchar) [id],'Reject'[text]
                Union  
                Select Cast('Diagnostic' As varchar) [id],'Diagnostic'[text]
                Union   
                Select  Cast('CommerciallyApprove' As varchar) [id],'Commercially Approve'[text];";
        }

        public static string GetPOFor()
        {
            return $@"SELECT CAST(POForId AS VARCHAR) AS id, POFor AS text
                FROM YarnPIFor";
        }

        public static string GetInHouseSupplierByItemSubGroup(string subGroupName)
        {
            return $@"Select Cast (CE.CompanyID as varchar) [id] , C.ShortName [text]
                From (select SubGroupID, ContactID from {DbNames.EPYSL}..SupplierItemGroupStatus Group By SubGroupID, ContactID) SIGS
                Inner Join {DbNames.EPYSL}..Contacts C On SIGS.ContactID = C.ContactID
                Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = SIGS.SubGroupID
                Inner Join {DbNames.EPYSL}..ContactAdditionalInfo CAI On CAI.ContactID = SIGS.ContactID
                Inner Join {DbNames.EPYSL}..CompanyEntity CE On C.MappingCompanyID = CE.CompanyID
                Where ISG.SubGroupName = '{subGroupName}' And Isnull(CAI.InHouse,0) = 1
                Group by CE.CompanyID, C.Name, C.ShortName";
        }

        public static string GetPaymentMethods(int contactId)
        {
            return $@"SELECT CAST(PM.PaymentMethodID AS VARCHAR) AS id, PM.PaymentMethodName AS text
                FROM {DbNames.EPYSL}..PaymentMethod PM
                Inner Join {DbNames.EPYSL}..ContactPaymentMethod CPM On PM.PaymentMethodID = CPM.PaymentMethodID
                WHERE ContactID = {contactId} AND PM.PaymentMethodID > 0
                GROUP BY PM.PaymentMethodID, PM.PaymentMethodName";
        }

        public static string GetPaymentMethodsByPO(int ypoMasterId)
        {
            return $@"With
                S As (
	                Select SupplierId From  {TableNames.YarnPOMaster} Where YPOMasterID = {ypoMasterId}
                )

                SELECT CAST(PM.PaymentMethodID AS VARCHAR) AS id, PM.PaymentMethodName AS text
                FROM {DbNames.EPYSL}..PaymentMethod PM
                Inner Join {DbNames.EPYSL}..ContactPaymentMethod CPM On PM.PaymentMethodID = CPM.PaymentMethodID
                Inner Join S On S.SupplierId = CPM.ContactID
                GROUP BY PM.PaymentMethodID, PM.PaymentMethodName";
        }

        public static string GetIncoTermsBySupplier(int supplierId)
        {
            return $@"SELECT CAST(CIT.IncoTermsID AS VARCHAR) AS id, IT.IncoTermsName AS text
                FROM {DbNames.EPYSL}..ContactIncoTerms CIT
                INNER JOIN {DbNames.EPYSL}..ContactAdditionalInfo CAI ON CAI.ContactID = CIT.ContactID
                INNER JOIN {DbNames.EPYSL}..IncoTerms IT ON IT.IncoTermsID = CIT.IncoTermsID
                WHERE CAI.ContactID = {supplierId}";
        }

        public static string GetIncoTermsByPO(int ypoMasterId)
        {
            return $@";With
                S As (
	                Select SupplierId From {TableNames.YarnPOMaster} Where YPOMasterID = {ypoMasterId}
                )

                SELECT CAST(CIT.IncoTermsID AS VARCHAR) AS id, IT.IncoTermsName AS text
                FROM S
                Inner Join {DbNames.EPYSL}..ContactIncoTerms CIT ON CIT.ContactID = S.SupplierID
                INNER JOIN {DbNames.EPYSL}..ContactAdditionalInfo CAI ON CAI.ContactID = CIT.ContactID
                INNER JOIN {DbNames.EPYSL}..IncoTerms IT ON IT.IncoTermsID = CIT.IncoTermsID";
        }

        public static string GetPaymentTermsBySupplier(int supplierId)
        {
            return $@"SELECT CAST(CPT.PaymentTermsID AS VARCHAR) AS id, PT.PaymentTermsName AS text
                FROM {DbNames.EPYSL}..ContactPaymentTerms CPT
                INNER JOIN {DbNames.EPYSL}..PaymentTrems PT ON PT.PaymentTermsID = CPT.PaymentTermsID
                WHERE CPT.ContactID = {supplierId}";
        }

        public static string GetPaymentTermsByPO(int ypoMasterId)
        {
            return $@";With
                S As (
	                Select SupplierId From  {TableNames.YarnPOMaster}  Where YPOMasterID = {ypoMasterId}
                )

                SELECT CAST(CPT.PaymentTermsID AS VARCHAR) AS id, PT.PaymentTermsName AS text
                FROM S
                Inner Join {DbNames.EPYSL}..ContactPaymentTerms CPT ON CPT.ContactID = S.SupplierID
                INNER JOIN {DbNames.EPYSL}..PaymentTrems PT ON PT.PaymentTermsID = CPT.PaymentTermsID";
        }

        public static string GetCDASuppliers(int subgroupID)
        {
            return $@"Select Cast(C.ContactID as varchar) [id], C.Name [text]
                From SupplierItemGroupStatus ST
                Inner Join ContactBusinessType CBT On ST.ContactID = CBT.ContactID
                Inner Join BusinessType BT On BT.TypeID = CBT.TypeID
                Inner Join Contacts C On C.ContactID = ST.ContactID
                Inner Join ContactCategoryChild CCC On CCC.ContactID = ST.ContactID
                Inner Join ContactCategoryHK CHK On CHK.ContactCategoryID = CCC.ContactCategoryID
                Where BT.TypeName = 'Manufacturer' And CHK.ContactCategoryName = 'Supplier' And SubGroupID = {subgroupID}";
        }

        public static string GetFabricTechnicalNames()
        {
            return $@"SELECT CAST(TechnicalNameId AS VARCHAR) AS id, TechnicalName AS text
                FROM FabricTechnicalNameSetup";
        }

        public static string GetDyeingMCNames()
        {
            return $@"SELECT CAST(DyeingMCNameID AS VARCHAR) id, DyeingMCName [text]
                FROM DyeingMCNameSetup";
        }

        public static string GetYarnYDStatus()
        {
            return $@"SELECT CAST(YDStatusID AS VARCHAR) AS id, YDStatus text
                FROM FabricBookingAnalysisYarnYDStatus";
        }

        public static string GetFinishingMCNames()
        {
            return $@"SELECT	CAST(FinishingMCNameID AS VARCHAR) [id], FinishingMCName [text]
                FROM FinishingMCNameSetup";
        }

        public static string GetRawCDAItemByTypeId(int particularsId)
        {
            return $@"With
                SG As(
	                Select SubGroupID, SubGroupName
	                From ItemSubGroup ISG
	                Inner Join EntityTypeValue EV On ISG.SubGroupName Like '%' + EV.ValueName + '%'
	                Where EV.ValueID = {particularsId}
	                Group By SubGroupID, SubGroupName
                )

                SELECT CAST(ItemMasterID AS VARCHAR) id, text = Case When SG.SubGroupName = 'Dyes' Then ISV2.SegmentValue Else ItemName End
                FROM {DbNames.EPYSL}..ItemMaster IM
                Left Join {DbNames.EPYSL}..ItemSegmentValue ISV2 On ISV2.SegmentValueID = Segment2ValueID
                Inner Join SG On SG.SubGroupID = IM.SubGroupID
                ORDER BY ItemName";
        }

        public static string GetYarnCountByYarnItem(int fiberTypeId, int blendTypeId, int yarnTypeId, int prograrmId, int subProgramId
            , int certificationsId, int technicalParamterId, int compositionId, int manufacturingLineId, int manufacturingProcessId
            , int manufacturingSubprocessId, int shadeId, int colorId, int colorGradeId)
        {
            return $@"Select CAST(YC.CountID AS varchar) id, YC.Count [text]
                From YarnSupplierProductRelationChild C
                Inner Join YarnSupplierProductRelationChildYarnCount YC On C.SetupChildID = YC.SetupChildID
                Where FiberTypeID = {fiberTypeId} And BlendTypeID = {blendTypeId} And YarnTypeID = {yarnTypeId} And ProgramID = {prograrmId}
                    And SubProgramID = {subProgramId} And CertificationsID = {certificationsId} And TechnicalParameterID = {technicalParamterId}
                    And CompositionsID = {compositionId} And ManufacturingLineID = {manufacturingLineId} And ManufacturingProcessID = {manufacturingProcessId}
                    And ManufacturingSubProcessID = {manufacturingSubprocessId} And ShadeID = {shadeId} And YarnColorID = {colorId} And ColorGradeID = {colorGradeId}
                Group By YC.CountID, YC.Count";
        }

        public static string GetTextileCounts()
        {
            return $@"
                ;SELECT CAST(ChildID AS varchar) id, DisplayValue [text]
				FROM {DbNames.EPYSL}..ItemSegmentDisplayValueYarnCount;";
        }

        public static string GetTextileYarnCounts()
        {
            return $@"Select CAST(ChildID As varchar) [id], DisplayValue [text]
                From {DbNames.EPYSL}..ItemSegmentDisplayValueYarnCount";
        }

        public static string GetItemSubGroups()
        {
            return $@"Select Cast(SubGroupID As varchar) id, SubGroupName [text]
                From {DbNames.EPYSL}..ItemSubGroup";
        }

        public static string GetMachineByUnitAsync(int unitId, int processId)
        {
            return
                $@"SELECT CAST(S.FMSID AS VARCHAR) id, S.MachineNo text
					FROM FinishingMachineSetup S WHERE FMCMasterID = {processId} AND UnitID = {unitId}
					ORDER BY S.FMCMasterID";
        }

        public static string GetYarnAndCDAUsers()
        {
            return $@"Select UserCode[id], EmployeeName[text]
                From {DbNames.EPYSL}..Employee E
                Inner Join {DbNames.EPYSL}..LoginUser L ON L.EmployeeCode = E.EmployeeCode
                Inner Join {DbNames.EPYSL}..EmployeeDepartment DPT ON E.DepertmentID = DPT.DepertmentID
                Where DPT.DepertmentDisplayName In ('Knitting', 'R&D') Or UserName = 'EPYMis'
                Order By EmployeeName";
        }
        public static string GetYarnAndCDAUsersForYarnPR()
        {
            return $@"Select UserCode[id], EmployeeName[text]
                From {DbNames.EPYSL}..Employee E
                Inner Join {DbNames.EPYSL}..LoginUser L ON L.EmployeeCode = E.EmployeeCode
                Inner Join {DbNames.EPYSL}..EmployeeDepartment DPT ON E.DepertmentID = DPT.DepertmentID
                Where DPT.DepertmentDisplayName In ('Knitting', 'R&D', 'Production Management Control','Operation[Textile]','Operation','Planning, Monitoring & Control') Or UserName = 'EPYMis'
                Order By EmployeeName";
        }
        public static string GetCDAIndentByUsers()
        {
            return $@"Select UserCode[id], EmployeeName[text]
                From {DbNames.EPYSL}..Employee E
                Inner Join {DbNames.EPYSL}..LoginUser L ON L.EmployeeCode = E.EmployeeCode
                Inner Join {DbNames.EPYSL}..EmployeeDepartment DPT ON E.DepertmentID = DPT.DepertmentID
                Where DPT.DepertmentDisplayName In ('Formulation Lab') Or UserName = 'EPYMis'
                Order By EmployeeName";
        }
        public static string GetSupplierCountryOfOrigins(int supplierId)
        {
            return $@"
                SELECT CAST(CT.CountryID AS VARCHAR) AS id, C.CountryName AS text
                FROM Contacts CT
                INNER JOIN Country C ON C.CountryID = CT.CountryID
                WHERE ContactID = {supplierId}";
        }
        public static string GetItemSubGroupMailConfiguration(string subGroupName, string mailfor)
        {
            return $@"Select ISGMS.*
                    From ItemSubGroupMailSetup ISGMS
                    Inner Join ItemSubGroup ISG On ISG.SubGroupID = ISGMS.SubGroupID
                    Inner Join MailSetupFor MSF On MSF.SetupForID = ISGMS.SetupForID
                    Where ISG.SubGroupName='{subGroupName}' And MSF.SetupForName in (Select _ID From dbo.fnReturnStringArray('{mailfor}',','))";
        }

        public static string GetSampleTypeByBookingID(int bookingID)
        {
            return $@"Select SM.SampleID [id], ST.SampleTypeName [text]
                    From {DbNames.EPYSL}..SampleBookingMaster SM
                    Inner Join {DbNames.EPYSL}..SampleType ST On ST.SampleTypeID = SM.SampleID
                    Where SM.BookingID = {bookingID}";
        }
        public static string GetAllEmployeeMailSetupByUserCodeAndSetupForNameAndBuyerTeam(string BuyerTeamID, string UserCode, string SetupForName)
        {
            return $@"Select EMS.*
                      From BuyerTeamWiseEmployeeMailSetup EMS
                      Inner Join Employee E On E.EmployeeCode = EMS.EmployeeCode
                      Inner Join LoginUser LU On LU.EmployeeCode = EMS.EmployeeCode
                      Inner Join MailSetupFor MSF on MSF.SetupForID = EMS.SetupForID
                      Where EMS.CategoryTeamID = {BuyerTeamID} And LU.UserCode = {UserCode} 
                      And MSF.SetupForName in (Select _ID From dbo.fnReturnStringArray('{SetupForName}',','))";
        }
        public static string GetColorWiseSizeCollar(string BookingNo)
        {
            return $@"SELECT distinct id=CASE WHEN BAC.SubGroupID = 1 THEN '0' ELSE ISV3.SegmentValue END+' X '+CASE WHEN BAC.SubGroupID = 1 THEN '0' ELSE ISV4.SegmentValue END,
                      text=CASE WHEN BAC.SubGroupID = 1 THEN '0' ELSE ISV3.SegmentValue END+' X '+CASE WHEN BAC.SubGroupID = 1 THEN '0' ELSE ISV4.SegmentValue END
                      FROM {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} BAC
                      INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FCM ON FCM.BookingID = BAC.BookingID
                      INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = BAC.ItemMasterID
                      LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
                      LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                      LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                      LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                      LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                      LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                      LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                      WHERE FCM.BookingNo = '{BookingNo}' AND BAC.SubGroupID IN (11)";
        }
        public static string GetColorWiseSizeCuff(string BookingNo)
        {
            return $@"SELECT distinct id=CASE WHEN BAC.SubGroupID = 1 THEN '0' ELSE ISV3.SegmentValue END+' X '+CASE WHEN BAC.SubGroupID = 1 THEN '0' ELSE ISV4.SegmentValue END,
                      text=CASE WHEN BAC.SubGroupID = 1 THEN '0' ELSE ISV3.SegmentValue END+' X '+CASE WHEN BAC.SubGroupID = 1 THEN '0' ELSE ISV4.SegmentValue END
                      FROM {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} BAC
                      INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FCM ON FCM.BookingID = BAC.BookingID
                      INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = BAC.ItemMasterID
                      LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
                      LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                      LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                      LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                      LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                      LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                      LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                      WHERE FCM.BookingNo = '{BookingNo}' AND BAC.SubGroupID IN (12)";
        }
        public static string GetAllColorWiseSizeCollar(string BookingNo)
        {
            return $@"SELECT BAC.*,ConstructionId=ISV1.SegmentValueID, Construction = ISV1.SegmentValue, 
                      CompositionId = ISV2.SegmentValueID,Composition = ISV2.SegmentValue,
                      ColorID = CASE WHEN BAC.SubGroupID = 1 THEN ISV3.SegmentValueID ELSE ISV5.SegmentValueID END, 
                      Color = CASE WHEN BAC.SubGroupID = 1 THEN ISV3.SegmentValue ELSE ISV5.SegmentValue END, 
                      Length = CASE WHEN BAC.SubGroupID = 1 THEN '0' ELSE ISV3.SegmentValue END,
                      Width = CASE WHEN BAC.SubGroupID = 1 THEN '0' ELSE ISV4.SegmentValue END
                      FROM {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} BAC
                      INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FCM ON FCM.BookingID = BAC.BookingID
                      INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = BAC.ItemMasterID
                      LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
                      LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                      LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                      LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                      LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                      LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                      LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                      WHERE FCM.BookingNo = '{BookingNo}' AND BAC.SubGroupID IN (11);";
        }
        public static string GetAllColorWiseSizeCuff(string BookingNo)
        {
            return $@"SELECT BAC.*,ConstructionId=ISV1.SegmentValueID, Construction = ISV1.SegmentValue, 
                      CompositionId = ISV2.SegmentValueID,Composition = ISV2.SegmentValue,
                      ColorID = CASE WHEN BAC.SubGroupID = 1 THEN ISV3.SegmentValueID ELSE ISV5.SegmentValueID END, 
                      Color = CASE WHEN BAC.SubGroupID = 1 THEN ISV3.SegmentValue ELSE ISV5.SegmentValue END, 
                      Length = CASE WHEN BAC.SubGroupID = 1 THEN '0' ELSE ISV3.SegmentValue END,
                      Width = CASE WHEN BAC.SubGroupID = 1 THEN '0' ELSE ISV4.SegmentValue END
                      FROM {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} BAC
                      INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FCM ON FCM.BookingID = BAC.BookingID
                      INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = BAC.ItemMasterID
                      LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
                      LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                      LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                      LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                      LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                      LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                      LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                      WHERE FCM.BookingNo = '{BookingNo}' AND BAC.SubGroupID IN (12);";
        }

        #region ShortQuery
        public static string GetAdjustmentReason()
        {
            return $@"SELECT id = AdjustmentReasonId, Text = Reason FROM AdjustmentReason";
        }
        public static string GetAdjustmentType()
        {
            return $@"SELECT id = AdjustmentTypeId, Text = TypeName FROM AdjustmentType";
        }
        #endregion

        public static string GetImagePathQuery(string bookingNos, string imageType = "")
        {
            if (bookingNos.IsNotNullOrEmpty())
            {
                bookingNos = "'" + bookingNos + "'";
                bookingNos = $@" AND BM.BookingNo IN ({bookingNos}) ";
            }

            if (imageType == "TP") imageType = $@" AND ImagePath LIKE '%TechPackImage%' ";
            else if (imageType == "BK") imageType = $@" AND ImagePath NOT LIKE '%TechPackImage%' ";

            string sql = $@"WITH IPathF AS
                        (
	                        SELECT BM.BookingNo, BCI.ImagePath, ImageType='BK'
	                        FROM {DbNames.EPYSL}..BookingMaster BM
	                        INNER JOIN {DbNames.EPYSL}..BookingChildImage BCI ON BCI.BookingID = BM.BookingID
	                        WHERE 1=1 {bookingNos}
	                        GROUP BY BM.BookingNo, BCI.ImagePath

	                        UNION

	                        SELECT BM.BookingNo, TPI.ImagePath, ImageType='TP'
	                        FROM {DbNames.EPYSL}..BookingMaster BM
	                        INNER JOIN {DbNames.EPYSL}..ExportOrderMaster EOM ON EOM.ExportOrderID = BM.ExportOrderID
	                        INNER JOIN {DbNames.EPYSL}..TechPackMaster TPM ON TPM.StyleMasterID = EOM.StyleMasterID
	                        INNER JOIN {DbNames.EPYSL}..TechPackImage TPI ON TPI.TechPackID = TPM.TechPackID
	                        WHERE 1=1 AND TPI.ImagePath NOT LIKE '%Chart%' {bookingNos}
	                        GROUP BY BM.BookingNo, TPI.ImagePath

	                        UNION 

	                        SELECT BM.BookingNo, BCI.ImagePath, ImageType='BK'
	                        FROM {DbNames.EPYSL}..SampleBookingMaster BM
	                        INNER JOIN {DbNames.EPYSL}..SampleBookingChildImage BCI ON BCI.BookingID = BM.BookingID
	                        WHERE 1=1 {bookingNos}
	                        GROUP BY BM.BookingNo, BCI.ImagePath

	                        UNION

	                        SELECT BM.BookingNo, TPI.ImagePath, ImageType='TP'
	                        FROM {DbNames.EPYSL}..SampleBookingMaster BM
	                        INNER JOIN {DbNames.EPYSL}..TechPackMaster TPM ON TPM.StyleMasterID = BM.StyleMasterID
	                        INNER JOIN {DbNames.EPYSL}..TechPackImage TPI ON TPI.TechPackID = TPM.TechPackID
	                        WHERE 1=1 AND TPI.ImagePath NOT LIKE '%Chart%' {bookingNos}
	                        GROUP BY BM.BookingNo, TPI.ImagePath
                        ),
                        IPath AS
                        (
	                        SELECT IPathF.BookingNo, ImagePath = ISNULL(IPathF.ImagePath,'')
	                        FROM IPathF
	                        WHERE ISNULL(ImagePath,'') <> '' {imageType}
                        )
                        SELECT * FROM IPath";
            return sql;
        }

        public static string GetAgeing(string bookingNos)
        {
            string activeQuery = "";
            string usedQuery = "";

            //if (!isGetAll) activeQuery = " AND DVD.IsActive = 1 ";
            //if (usedDayValidDurationId > 0) usedQuery = $@" OR DVD.DayValidDurationId = {usedDayValidDurationId} ";

            return $@"SELECT id = CAST(DVD.DayValidDurationId AS VARCHAR)
                    ,[text] = CASE WHEN DVD.DayDuration > 1 
                                   THEN CONCAT(ET.ValueName,' (',CAST(DVD.DayDuration AS VARCHAR),' days)')
                                   ELSE CONCAT(ET.ValueName,' (',CAST(DVD.DayDuration AS VARCHAR),' day)')
                              END
                    ,additionalValue = CAST(DVD.DayDuration AS VARCHAR)
                    ,[desc] = CAST(DVD.IsActive AS VARCHAR)
                    FROM {TableNames.DayValidDuration} DVD
                    INNER JOIN {DbNames.EPYSL}..EntityTypeValue ET ON ET.ValueID = DVD.LocalOrImportId
                    WHERE 1 = 1 {activeQuery} {usedQuery}";
            if (bookingNos.IsNotNullOrEmpty())
            {
                bookingNos = "'" + bookingNos + "'";
                bookingNos = $@" AND FBA.BookingNo IN ({bookingNos}) ";
            }

            string sql = $@"SELECT FBA.BookingNo
                            ,DateUpdated = MAX(ISNULL(FBA.DateUpdated,FBA.DateAdded))
                            ,ApprovedDateAllowance = MAX(ISNULL(FBA.ApprovedDateAllowance,GETDATE()))
                            ,UtilizationProposalConfirmedDate = MAX(ISNULL(FBA.UtilizationProposalConfirmedDate,GETDATE()))
                            ,CheckDateKnittingHead = MAX(ISNULL(FBA.CheckDateKnittingHead,GETDATE()))
                            ,ApprovedDateProdHead = MAX(ISNULL(FBA.ApprovedDateProdHead,GETDATE()))
                            FROM FBookingAcknowledge FBA
	                        WHERE 1=1 {bookingNos}
                            GROUP BY FBA.BookingNo";
            return sql;
        }

        public static string GetYarnSegementWithValue(string itemMasterIds)
        {
            string sql = $@"SELECT 
             IM.ItemMasterID
            ,Segment1ValueId = ISV1.SegmentValueID
            ,Segment1ValueDesc = ISV1.SegmentValue
            ,Segment2ValueId = ISV2.SegmentValueID
            ,Segment2ValueDesc = ISV2.SegmentValue
            ,Segment3ValueId = ISV3.SegmentValueID
            ,Segment3ValueDesc = ISV3.SegmentValue
            ,Segment4ValueId = ISV4.SegmentValueID
            ,Segment4ValueDesc = ISV4.SegmentValue
            ,Segment5ValueId = ISV5.SegmentValueID
            ,Segment5ValueDesc = ISV5.SegmentValue
            ,Segment6ValueId = ISV6.SegmentValueID
            ,Segment6ValueDesc = ISV6.SegmentValue

            ,Composition = ISV1.SegmentValue
            ,YarnType = ISV2.SegmentValue
            ,ManufacturingProcess = ISV3.SegmentValue
            ,SubProcess = ISV4.SegmentValue
            ,QualityParameter = ISV5.SegmentValue
            ,Count = ISV6.SegmentValue

            From {DbNames.EPYSL}..ItemMaster IM
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
            WHERE IM.ItemMasterID IN ({itemMasterIds})";
            return sql;
        }
        public static string GetDayValidDurations()
        {
            string activeQuery = "";
            string usedQuery = "";



            return $@"SELECT id = CAST(DVD.DayValidDurationId AS VARCHAR)
                    ,[text] = CASE WHEN DVD.DayDuration > 1 
                                   THEN CONCAT(ET.ValueName,' (',CAST(DVD.DayDuration AS VARCHAR),' days)')
                                   ELSE CONCAT(ET.ValueName,' (',CAST(DVD.DayDuration AS VARCHAR),' day)')
                              END
                    ,additionalValue = CAST(DVD.DayDuration AS VARCHAR)
                    ,[desc] = CAST(DVD.IsActive AS VARCHAR)
                    FROM {TableNames.DayValidDuration} DVD
                    INNER JOIN {DbNames.EPYSL}..EntityTypeValue ET ON ET.ValueID = DVD.LocalOrImportId
                    WHERE 1 = 1 {activeQuery} {usedQuery}";
        }



    }
}
