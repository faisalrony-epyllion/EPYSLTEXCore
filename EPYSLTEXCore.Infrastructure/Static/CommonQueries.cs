using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Azure.Core.HttpHeader;

namespace EPYSLEMSCore.Infrastructure.Static
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
    }
}
