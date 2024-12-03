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
    }
}
