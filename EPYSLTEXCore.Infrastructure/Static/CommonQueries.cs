using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Azure.Core.HttpHeader;

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

        public static string GetSpinner()
        {
            return $@"Select Cast(C.ContactID As varchar) [id], C.Name [text], CC.ContactCategoryName [desc]
                From Contacts C
                Inner Join ContactCategoryChild CCC On C.ContactID = CCC.ContactID
                Inner Join ContactCategoryHK CC ON CC.ContactCategoryID = CCC.ContactCategoryID
                Where CC.ContactCategoryName IN ('{ContactCategoryNames.SPINNER}')";
        }

        public static string GetUnit()
        {
            return $@"SELECT CAST(UnitID AS VARCHAR) AS id, DisplayUnitDesc AS text
                  FROM Unit";
        }
        public static string GetAllFiberType()
        {
            return $@"  Select SetupMasterID, FiberTypeID, b.SegmentValue FiberType  
                            From YarnProductSetupMaster a
                        Inner Join  {DbNames.EPYSL}..ItemSegmentValue b on b.SegmentValueID = a.FiberTypeID";
        }
        public static string GetEntityTypesByEntityTypeName(string segmentName)
        {
            /* return
                 $@"SELECT CAST(ValueID AS VARCHAR) id, ValueName text
                 FROM {DbNames.EPYSL}..EntityTypeValue EV
                 Inner Join {DbNames.EPYSL}..EntityType ET On EV.EntityTypeID = ET.EntityTypeID
                 WHERE ET.EntityTypeName = '{entityTypeName}' AND ValueName <> 'Select'
                 ORDER BY ValueName";*/
            return $@"Select CAST(ISV.SegmentValueID AS nvarchar) id, ISV.SegmentValue [text]
                From {DbNames.EPYSL}..ItemSegmentValue ISV
                Inner Join {DbNames.EPYSL}..ItemSegmentName ISN On ISV.SegmentNameID = ISN.SegmentNameID
                Where ISN.SegmentName = '{segmentName}'";
        }
        public static string GetItemSegmentValuesBySegmentName(string segmentName)
        {
            return $@"Select CAST(ISV.SegmentValueID AS nvarchar) id, ISV.SegmentValue [text]
                From {DbNames.EPYSL}..ItemSegmentValue ISV
                Inner Join {DbNames.EPYSL}..ItemSegmentName ISN On ISV.SegmentNameID = ISN.SegmentNameID
                Where ISN.SegmentName = '{segmentName}'";
        }
        public static string GetSampleTypeByBookingID(int bookingID)
        {
            return $@"Select SM.SampleID [id], ST.SampleTypeName [text]
                    From {DbNames.EPYSL}..SampleBookingMaster SM
                    Inner Join {DbNames.EPYSL}..SampleType ST On ST.SampleTypeID = SM.SampleID
                    Where SM.BookingID = {bookingID}";
        }
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
        public static string GetCertifications()
        {
            return $@"SELECT distinct CAST(ISV.SegmentValueID As varchar) [id], ISV.SegmentValue [text], ISN.SegmentName [desc],isnull(CAST(FCMS.SubProgramID As varchar),'0')additionalValue, isnull(CAST(FCMS.FiberID As varchar),'0')additionalValue2
                FROM {DbNames.EPYSL}..ItemSegmentName ISN
                INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISN.SegmentNameID = ISV.SegmentNameID
                Left Join {DbNames.EPYSL}..YarnCountHiddenSetup YCH On YCH.YarnCountID = ISV.SegmentValueID
                LEFT JOIN {DbNames.EPYSL}..FabricComponentMappingSetup FCMS on FCMS.CertificationsID=ISV.SegmentValueID
		        LEFT JOIN CertificationsBasicSetup CBS ON CBS.SegmentValueID = ISV.SegmentValueID
                WHERE ISN.SegmentName = '{ItemSegmentNameConstants.YARN_CERTIFICATIONS}' And ISNULL(ISV.SegmentValue, '') <> '' And YCH.YarnCountID IS NULL
		        	AND ISNULL(CBS.IsInactive,0) = 0
                ORDER BY ISV.SegmentValue";

        }

        public static string GetFabricComponents(string entityTypeName)
        {
            return $@"
                 Select CAST(EV.ValueID As varchar) [id], EV.ValueName [text]
                From {DbNames.EPYSL}..EntityTypeValue EV
                Inner Join {DbNames.EPYSL}..EntityType ET On EV.EntityTypeID = ET.EntityTypeID
		        LEFT JOIN FiberBasicSetup FBS ON FBS.ValueID = EV.ValueID
                Where ET.EntityTypeName = '{entityTypeName}' AND ISNULL(FBS.IsInactive,0) = 0
                Group By EV.ValueID,EV.ValueName";
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
        public static string GetYarnAndCDAUsers()
        {
            return $@"Select UserCode[id], EmployeeName[text]
                From {DbNames.EPYSL}..Employee E
                Inner Join {DbNames.EPYSL}..LoginUser L ON L.EmployeeCode = E.EmployeeCode
                Inner Join {DbNames.EPYSL}..EmployeeDepartment DPT ON E.DepertmentID = DPT.DepertmentID
                Where DPT.DepertmentDisplayName In ('Knitting', 'R&D') Or UserName = 'EPYMis'
                Order By EmployeeName";
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
        public static string GetSubPrograms()
        {
            return $@"SELECT distinct CAST(ISV.SegmentValueID As varchar) [id], ISV.SegmentValue [text], ISN.SegmentName [desc],isnull(CAST(FCMS.FiberID As varchar),'0')additionalValue
                FROM {DbNames.EPYSL}..ItemSegmentName ISN
                INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISN.SegmentNameID = ISV.SegmentNameID
                Left Join {DbNames.EPYSL}..YarnCountHiddenSetup YCH On YCH.YarnCountID = ISV.SegmentValueID
                LEFT JOIN {DbNames.EPYSL}..FabricComponentMappingSetup FCMS on FCMS.SubProgramID=ISV.SegmentValueID
		        LEFT JOIN SubProgramBasicSetup SBS ON SBS.SegmentValueID = ISV.SegmentValueID
                WHERE ISN.SegmentName = '{ItemSegmentNameConstants.YARN_SUBPROGRAM_NEW}' And ISNULL(ISV.SegmentValue, '') <> '' And YCH.YarnCountID IS NULL
			        AND ISNULL(SBS.IsInactive,0) = 0 
                ORDER BY ISV.SegmentValue";

        }


        public static string GetYarnShadeBooks()
        {
            return $@"SELECT ShadeCode [id], ShadeCode [text], ContactID [additionalValue] FROM YarnShadeBook";
        }



        public static object GetDayValidDurations()
        {
            throw new NotImplementedException();
        }
    }
}
