using System.Data;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace EPYSLTEXCore.Infrastructure.Static
{
    public static class StaticInfo
    {
        #region Off

        //public static Int32 MakeSystemID(int MenuID, string FieldOrTableName, int UserCode)
        //{
        //    try
        //    {
        //        return MakeUniqueCode(MenuID, FieldOrTableName, 50, DateTime.Now, "yy", "", "", string.Empty, "", UserCode, 1, RepeatAfter.NoRepeat, 1).ToInt();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        //public static string MakeUniqueCode(int MenuID, string FieldOrTableName, int codeLength, DateTime Dates, string dateFormat,
        // string prefix, string delimeter, String suffix, string FieldValue, int UserCode, int Increment, string repeatAfter, int StartNumber)
        //{
        //    String maxId = String.Empty;
        //    try
        //    {
        //        maxId = GetMaxNoToCreateUniqueCode(MenuID, FieldOrTableName, Dates, FieldValue, UserCode, Increment, repeatAfter, StartNumber).ToString();

        //        return maxId;

        //    }
        //    catch (Exception ex)
        //    {
        //        throw (ex);
        //    }
        //}

        //public static async Task<int> GetMaxNoToCreateUniqueCode(int MenuID, String FieldOrTableName, DateTime Dates, String FieldValue, int UserCode, int Increment, String RepeatAfter, int StartNumber)
        //{
        //    try
        //    {
        //        Int32 MaximumNo = 0;
        //        String sql = String.Format("Exec  EPYSL..spReturnMaxNoFromSystemKeyHistory '{0}','{1}','{2}','{3}',{4},'{5}',{6},'{7}',{8},'{9}',{10}", MenuID, FieldOrTableName, Dates, FieldValue, UserCode, DateTime.Now, 1, 1, Increment.ToString(), RepeatAfter, StartNumber.ToString());
        //        List<SystemKeyHistory> listSystemKeyHistory = new List<SystemKeyHistory>();

        //        var objConnection = new SqlConnection(ConfigurationManager.ConnectionStrings[AppConstants.GMT_CONNECTION].ConnectionString);

        //        using (var connection = objConnection)
        //        {
        //            await connection.OpenAsync();

        //            var records = await connection.QueryAsync<SystemKeyHistory>(sql);
        //            if (records.ToList().Count > 0)
        //                listSystemKeyHistory = records.ToList();

        //            if (connection.State == System.Data.ConnectionState.Open) connection.Close();
        //        }

        //        if (listSystemKeyHistory.Count > 0)
        //        {
        //            MaximumNo = Convert.ToInt32(listSystemKeyHistory[0].FieldValue);
        //            MaximumNo = MaximumNo - Increment + 1;
        //        }
        //        else
        //        {
        //            throw (new Exception("Error Create to Get MaxNo To Create UniqueCode!"));
        //        }
        //        return MaximumNo;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw (ex);
        //    }

        //}

        //public static async Task DeleteAutoNo(int MenuID, String FieldOrTableName, DateTime Dates, int UserCode)
        //{
        //    try
        //    {
        //       await StaticInfo.DeleteSystemKeyHistory(MenuID, FieldOrTableName, Dates, UserCode);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("System Key History Not Deleted Properly" + ex);
        //    }


        //}
        //public static async Task DeleteSystemKeyHistory(int MenuID, String FieldOrTableName, DateTime Dates, int UserCode)
        //{
        //    try
        //    {
        //        string strSql = String.Format(@"exec  EPYSL..spDeleteSystemKeyHistory {0},'{1}','{2}',{3},'{4}'", MenuID, FieldOrTableName, Dates, UserCode, RepeatAfter.NoRepeat);
        //        var objConnection = new SqlConnection(ConfigurationManager.ConnectionStrings[AppConstants.GMT_CONNECTION].ConnectionString);

        //        using (var connection = objConnection)
        //        {

        //            if (strSql.IsNotNullOrEmpty())
        //            {
        //               await connection.ExecuteAsync(strSql, AppConstants.GMT_CONNECTION);
        //            }

        //            if (connection.State == System.Data.ConnectionState.Open) connection.Close();
        //        }


        //    }
        //    catch (Exception ex)
        //    {

        //        throw new Exception("System Key History Not Deleted Properly" + ex);
        //    }

        //}
        #endregion

        public static string DataTableToJSONWithStringBuilder(DataTable table)
        {
            var JSONString = new StringBuilder();
            if (table.Rows.Count > 0)
            {
                JSONString.Append("[");
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    JSONString.Append("{");
                    for (int j = 0; j < table.Columns.Count; j++)
                    {
                        if (j < table.Columns.Count - 1)
                        {
                            if (table.Columns[j].ColumnName.ToString().Contains("date") && table.Rows[i][j].ToString() == "")
                            {
                                JSONString.Append("\"" + table.Columns[j].ColumnName.ToString() + "\":" + "null,");
                            }
                            else
                                JSONString.Append("\"" + table.Columns[j].ColumnName.ToString() + "\":" + "\"" + table.Rows[i][j].ToString() + "\",");
                        }
                        else if (j == table.Columns.Count - 1)
                        {
                            if (table.Columns[j].ColumnName.ToString().Contains("date") && table.Rows[i][j].ToString() == "")
                            {
                                JSONString.Append("\"" + table.Columns[j].ColumnName.ToString() + "\":" + "null,");
                            }
                            else
                                JSONString.Append("\"" + table.Columns[j].ColumnName.ToString() + "\":" + "\"" + table.Rows[i][j].ToString() + "\"");
                        }
                    }
                    if (i == table.Rows.Count - 1)
                    {
                        JSONString.Append("}");
                    }
                    else
                    {
                        JSONString.Append("},");
                    }
                }
                JSONString.Append("]");
            }
            return JSONString.ToString();
        }
        /*
        public static string DataTableToJSONWithJavaScriptSerializer(DataTable table)
        {
            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            List<Dictionary<string, object>> parentRow = new List<Dictionary<string, object>>();
            Dictionary<string, object> childRow;
            foreach (DataRow row in table.Rows)
            {
                childRow = new Dictionary<string, object>();
                foreach (DataColumn col in table.Columns)
                {
                    childRow.Add(col.ColumnName, row[col]);
                }
                parentRow.Add(childRow);
            }
            return jsSerializer.Serialize(parentRow);
        }
        */
        public static string DataTableToJSONWithJavaScriptSerializer(DataTable table)
        {
            List<Dictionary<string, object>> parentRow = new List<Dictionary<string, object>>();
            foreach (DataRow row in table.Rows)
            {
                Dictionary<string, object> childRow = new Dictionary<string, object>();
                foreach (DataColumn col in table.Columns)
                {
                    childRow.Add(col.ColumnName, row[col]);
                }
                parentRow.Add(childRow);
            }
            return JsonConvert.SerializeObject(parentRow);
        }
    }
    public sealed class RepeatAfter
    {
        public const string EveryYear = "EveryYear";
        public const string EveryMonth = "EveryMonth";
        public const string EveryDay = "EveryDay";
        public const string NoRepeat = "NoRepeat";
    }
}

