using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Static;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;

namespace System
{
    public static class ExtensionMethods
    {
        public static string[] GridDateFormatArray = new string[] { "dd/MM/yyyy", "MM/dd/yyyy" };
        public const string DateFormat = "yyyy-MM-dd";

        public static object IfEmptyThenNull(this string str) => string.IsNullOrEmpty(str) ? null : str;

        public static object IfZeroThenNull(this int val) => val.IsZero() ? (int?)null : val;

        public static object IfZeroThen(this int val, object assVal) => val.IsZero() ? assVal : val;

        public static object IfZeroThenNull(this decimal val) => val.IsZero() ? (decimal?)null : val;

        public static object IfZeroThen(this decimal val, object assVal) => val.IsZero() ? assVal : val;

        public static object IfZeroThenNull(this double val) => val.IsZero() ? (double?)null : val;

        public static object IfZeroThen(this double val, object assVal) => val.IsZero() ? assVal : val;

        public static bool NotEquals(this object val, object compVal) => val != compVal;

        public static bool NotEquals(this string val, string compVal) => val != compVal;

        public static bool NotEquals(this decimal val, decimal compVal) => val != compVal;

        public static bool NotEquals(this double val, double compVal) => val != compVal;

        public static bool NotEquals(this int val, int compVal) => val != compVal;

        public static bool NotEquals(this short val, short compVal) => val != compVal;

        public static bool NotEquals(this long val, long compVal) => val != compVal;

        public static bool NotEquals(this DateTime val, DateTime compVal) => val != compVal;

        public static bool NotEquals(this bool val, bool compVal) => val != compVal;

        public static bool IsNullOrEmpty(this string str) => string.IsNullOrEmpty(str);

        public static bool IsNotNullOrEmpty(this string str) => !string.IsNullOrEmpty(str);

        public static bool IsZero(this decimal val) => val.Equals(decimal.Zero);

        public static bool IsNotZero(this decimal val) => !val.Equals(decimal.Zero);

        public static bool IsZero(this double val) => val.Equals(0);

        public static bool IsNotZero(this double val) => !val.Equals(0);

        public static bool IsZero(this int val) => val.Equals(0);

        public static bool IsNotZero(this int val) => !val.Equals(0);

        public static bool IsNull(this IDapperBaseEntity obj) => obj == null;

        public static bool IsNotNull(this IDapperBaseEntity obj) => obj != null;

        public static bool IsNull(this object obj) => obj == null;

        public static bool IsNotNull(this object obj) => obj != null;

        public static bool IsNullOrDBNull(this object obj) => obj == null || obj == DBNull.Value;

        public static string IfEmptyThenZero(this string str) => string.IsNullOrEmpty(str) ? "0" : str;

        public static bool IsNumber(this string str) => IsNumber(str, false);

        public static string ToShortDateStringWithDefault(this DateTime value)
        {
            return value == DateTime.MinValue || value.IsNullOrDBNull() ? "" : value.ToShortDateString();
        }

        public static string ToShortDateStringWithDefault(this DateTime? value)
        {
            return !value.HasValue || value == DateTime.MinValue || value.IsNullOrDBNull() ? "" : value.Value.ToShortDateString();
        }

        public static string FormatToShortDate(this string value)
        {
            if (value.IsNullOrEmpty() || value.IsNullOrDBNull()) return "";

            DateTime.TryParse(value, out DateTime cvtDate);
            if (cvtDate == DateTime.MinValue) return "";


            return cvtDate == DateTime.MinValue ? "" : cvtDate.ToShortDateString();
        }

        public static bool IsNumber(this string str, bool negativeCheck)
        {
            var isNumber = double.TryParse(str, out double result);
            if (negativeCheck && result < 0)
                return false;

            return isNumber;
        }

        public static decimal ToDecimal(this string str)
        {
            decimal.TryParse(str.Trim().Replace(",", ""), out decimal result);
            return result;
        }

        public static double ToDouble(this string str)
        {
            double.TryParse(str.Trim().Replace(",", ""), out double result);
            return result;
        }

        public static int ToInt(this string str)
        {
            int.TryParse(str.Trim().Replace(",", ""), out int result);
            return result;
        }

        public static bool ToBoolean(this string str)
        {
            bool.TryParse(str, out bool result);
            return result;
        }

        public static DateTime ToDateTime(this string str)
        {
            DateTime.TryParse(str.Trim(), out DateTime result);
            return result;
        }

        public static DateTime ToDateTime(this string str, string DateFormat)
        {
            DateTime.TryParseExact(str.Trim(), DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result);
            return result;
        }

        public static object Value(this DateTime dateTime, string format) => dateTime.Equals(DateTime.MinValue) ? null : dateTime.ToString(format);

        public static string StringValue(this DateTime dateTime) => dateTime.Equals(DateTime.MinValue) ? string.Empty : dateTime.ToShortDateString();

        public static bool GetBoolean(this IDataRecord reader, string columnName) => !reader[columnName].Equals(DBNull.Value) && (bool)reader[columnName];

        public static byte[] GetBytes(this IDataRecord reader, string columnName) => reader[columnName].Equals(DBNull.Value) ? null : (byte[])reader[columnName];

        public static DateTime? GetDateTime(this IDataRecord reader, string columnName)
        {
            try
            {
                return reader[columnName].Equals(DBNull.Value) ? (DateTime?)null : DateTime.Parse(reader[columnName].ToString());
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static decimal GetDecimal(this IDataRecord reader, string columnName)
        {
            try
            {
                return reader[columnName].Equals(DBNull.Value) ? 0.0m : (decimal)reader[columnName];
            }
            catch (Exception)
            {
                return 0.0m;
            }
        }

        public static double GetDouble(this IDataRecord reader, string columnName) => reader[columnName].Equals(DBNull.Value) ? 0.0d : (double)reader[columnName];

        public static short GetInt16(this IDataRecord reader, string columnName) => reader[columnName].Equals(DBNull.Value) ? (short)0 : (short)reader[columnName];

        public static int GetInt32(this IDataRecord reader, string columnName)
        {
            try
            {
                return reader[columnName].Equals(DBNull.Value) ? 0 : (int)reader[columnName];
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public static long GetInt64(this IDataRecord reader, string columnName) => reader[columnName].Equals(DBNull.Value) ? 0 : (long)reader[columnName];

        public static string GetString(this IDataRecord reader, string columnName)
        {
            try
            {
                return reader[columnName].ToString();
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static int GetEnumValue(Type enumType, String value)
        {
            if (value == "Integer")
                value = "Int32";
            else if (value == "Float")
                value = "Double";
            int i = (int)Enum.Parse(enumType, value);
            return i;
        }

        public static string ImageToString(this byte[] value) => value == null ? null : Convert.ToBase64String(value);

        public static Type GetTypeFromName(this string typeName)
        {
            if (typeName == null)
                throw new ArgumentNullException(nameof(typeName));

            bool isArray = false, isNullable = false;

            if (typeName.IndexOf("[]") != -1)
            {
                isArray = true;
                typeName = typeName.Remove(typeName.IndexOf("[]"), 2);
            }

            if (typeName.IndexOf("?") != -1)
            {
                isNullable = true;
                typeName = typeName.Remove(typeName.IndexOf("?"), 1);
            }

            typeName = typeName.ToLower();

            string parsedTypeName = null;
            switch (typeName)
            {
                case "bool":
                    parsedTypeName = "System.bool";
                    break;
                case "byte":
                    parsedTypeName = "System.Byte";
                    break;
                case "char":
                    parsedTypeName = "System.Char";
                    break;
                case "datetime":
                    parsedTypeName = "System.DateTime";
                    break;
                case "datetimeoffset":
                    parsedTypeName = "System.DateTimeOffset";
                    break;
                case "decimal":
                    parsedTypeName = "System.Decimal";
                    break;
                case "double":
                    parsedTypeName = "System.Double";
                    break;
                case "float":
                    parsedTypeName = "System.Single";
                    break;
                case "Int16":
                case "short":
                    parsedTypeName = "System.Int16";
                    break;
                case "Int32":
                case "int":
                    parsedTypeName = "System.Int32";
                    break;
                case "Int64":
                case "long":
                    parsedTypeName = "System.Int64";
                    break;
                case "object":
                    parsedTypeName = "System.Object";
                    break;
                case "sbyte":
                    parsedTypeName = "System.SByte";
                    break;
                case "string":
                    parsedTypeName = "System.String";
                    break;
                case "timespan":
                    parsedTypeName = "System.TimeSpan";
                    break;
                case "UInt16":
                case "ushort":
                    parsedTypeName = "System.UInt16";
                    break;
                case "UInt32":
                case "uint":
                    parsedTypeName = "System.UInt32";
                    break;
                case "UInt64":
                case "ulong":
                    parsedTypeName = "System.UInt64";
                    break;
            }

            if (parsedTypeName != null)
            {
                if (isArray)
                    parsedTypeName += "[]";

                if (isNullable)
                    parsedTypeName = string.Concat("System.Nullable`1[", parsedTypeName, "]");
            }
            else
                parsedTypeName = typeName;

            return Type.GetType(parsedTypeName);
        }

        public static string FromQueryString(this string gridstring)
        {
            return gridstring.Replace("_^amp;", "&").Replace("_`amp;", "+").Replace("&amp;", "&").Replace("_^com;", ",");
        }

        public static string ReplaceAt(this string value, int index, char newchar)
        {
            if (value.Length <= index)
                return value;
            else
                return string.Concat(value.Select((c, i) => i == index ? newchar : c));
        }

        /// <summary>
        /// Converts a base64 image string to Byte[]
        /// </summary>
        /// <param name="value">Base64String data</param>
        /// <returns></returns>
        public static byte[] ToBase64ToByteArray(this string value)
        {
            var imgData = value.Split(',')[1];
            imgData = imgData.Trim().Replace(" ", "+");
            if (imgData.Length % 4 > 0)
                imgData = imgData.PadRight(imgData.Length + 4 - imgData.Length % 4, '=');

            return Convert.FromBase64String(imgData);
        }

        public static DataSet DataReaderToDataSet(IDataReader reader)
        {
            var ds = new DataSet();
            DataTable table;
            do
            {
                int fieldCount = reader.FieldCount;
                table = new DataTable();
                for (int i = 0; i < fieldCount; i++)
                {
                    table.Columns.Add(reader.GetName(i), reader.GetFieldType(i));
                }
                table.BeginLoadData();
                var values = new Object[fieldCount];
                while (reader.Read())
                {
                    reader.GetValues(values);
                    table.LoadDataRow(values, true);
                }
                table.EndLoadData();

                ds.Tables.Add(table);

            } while (reader.NextResult());
            reader.Close();
            return ds;
        }

        /// <summary>
        /// Equality comparator for two BaseItemMaster
        /// </summary>
        /// <param name="baseItem">Source BaseItemMaster</param>
        /// <param name="destinationItem">Compare BaseItemMaster</param>
        /// <returns></returns>
        public static bool IsEqual(this BaseItemMaster baseItem, BaseItemMaster destinationItem)
        {
            return baseItem.SubGroupId == destinationItem.SubGroupId
                && baseItem.UnitID == destinationItem.UnitID
                && baseItem.Segment1ValueDesc.Equals(destinationItem.Segment1ValueDesc, StringComparison.OrdinalIgnoreCase)
                && baseItem.Segment2ValueDesc.Equals(destinationItem.Segment2ValueDesc, StringComparison.OrdinalIgnoreCase)
                && baseItem.Segment3ValueDesc.Equals(destinationItem.Segment3ValueDesc, StringComparison.OrdinalIgnoreCase)
                && baseItem.Segment4ValueDesc.Equals(destinationItem.Segment4ValueDesc, StringComparison.OrdinalIgnoreCase)
                && baseItem.Segment5ValueDesc.Equals(destinationItem.Segment5ValueDesc, StringComparison.OrdinalIgnoreCase)
                && baseItem.Segment6ValueDesc.Equals(destinationItem.Segment6ValueDesc, StringComparison.OrdinalIgnoreCase)
                && baseItem.Segment7ValueDesc.Equals(destinationItem.Segment7ValueDesc, StringComparison.OrdinalIgnoreCase)
                && baseItem.Segment8ValueDesc.Equals(destinationItem.Segment8ValueDesc, StringComparison.OrdinalIgnoreCase)
                && baseItem.Segment9ValueDesc.Equals(destinationItem.Segment9ValueDesc, StringComparison.OrdinalIgnoreCase)
                && baseItem.Segment10ValueDesc.Equals(destinationItem.Segment10ValueDesc, StringComparison.OrdinalIgnoreCase)
                && baseItem.Segment11ValueDesc.Equals(destinationItem.Segment11ValueDesc, StringComparison.OrdinalIgnoreCase)
                && baseItem.Segment12ValueDesc.Equals(destinationItem.Segment12ValueDesc, StringComparison.OrdinalIgnoreCase)
                && baseItem.Segment13ValueDesc.Equals(destinationItem.Segment13ValueDesc, StringComparison.OrdinalIgnoreCase)
                && baseItem.Segment14ValueDesc.Equals(destinationItem.Segment14ValueDesc, StringComparison.OrdinalIgnoreCase)
                && baseItem.Segment15ValueDesc.Equals(destinationItem.Segment15ValueDesc, StringComparison.OrdinalIgnoreCase);
        }

        public static Dictionary<string, object> ToDictionary(dynamic dynObj)
        {
            var dictionary = new Dictionary<string, object>();
            foreach (PropertyDescriptor propertyDescriptor in TypeDescriptor.GetProperties(dynObj))
            {
                if (propertyDescriptor.Name.IsNullOrEmpty()) continue;
                object obj = propertyDescriptor.GetValue(dynObj);

                if (!dictionary.Any(x => x.Key == propertyDescriptor.Name)) dictionary.Add(propertyDescriptor.Name, obj);
            }

            return dictionary;
        }

        public static List<T> ConvertToList<T>(this DataTable dt)
        {
            var columnNames = new List<string>();
            foreach (DataColumn column in dt.Columns)
            {
                string cName = dt.Rows[0][column.ColumnName].ToString();
                if (!dt.Columns.Contains(cName) && cName != "")
                {
                    column.ColumnName = cName;
                }

                columnNames.Add(column.ColumnName);
            }

            dt.Rows[0].Delete();
            dt.AcceptChanges();

            var properties = typeof(T).GetProperties();
            return dt.AsEnumerable().Select(row =>
            {
                var objT = Activator.CreateInstance<T>();
                foreach (var pro in properties)
                {
                    if (columnNames.Any(x => x.Equals(pro.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        if (typeof(bool).IsAssignableFrom(pro.PropertyType))
                        {
                            var value = row[pro.Name] != DBNull.Value && (row[pro.Name].ToString().Equals("1") || row[pro.Name].ToString().Equals("true", StringComparison.OrdinalIgnoreCase) || row[pro.Name].ToString().Equals("Yes", StringComparison.OrdinalIgnoreCase));
                            pro.SetValue(objT, value);
                        }
                        else if (typeof(string).IsAssignableFrom(pro.PropertyType))
                        {
                            var value = row[pro.Name] == DBNull.Value ? string.Empty : Convert.ChangeType(row[pro.Name], pro.PropertyType);
                            value = value.ToString().IsNullOrEmpty() ? "" : value.ToString().Trim();
                            value = new Regex(AppConstants.SPECIAL_CHARACTER_TO_REMOVE_IN_UPLOAD).Replace(value.ToString(), "");
                            pro.SetValue(objT, value);
                        }
                        else if (typeof(DateTime).IsAssignableFrom(pro.PropertyType) || typeof(DateTime?).IsAssignableFrom(pro.PropertyType))
                        {
                            DateTime.TryParse(row[pro.Name].ToString(), out DateTime value);
                            pro.SetValue(objT, value);
                        }
                        else pro.SetValue(objT, row[pro.Name] == DBNull.Value ? null : Convert.ChangeType(row[pro.Name], pro.PropertyType));
                    }
                }
                return objT;
            }).ToList();
        }

        public static bool IsEmpty(this int value)
        {
            return value == 0;
        }

        public static bool IsEmpty(this int? value)
        {
            return !value.HasValue || value == 0;
        }

    }

    public static class StringExtensions
    {
        public static bool NullOrEmpty(this string value)
        {
            var isEmtpy = string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value);
            return isEmtpy;
        }

        public static bool NotNullOrEmpty(this string value)
        {
            var notEmtpy = !string.IsNullOrEmpty(value) && !string.IsNullOrWhiteSpace(value);
            return notEmtpy;
        }

        public static bool IsDate(this string value)
        {
            return DateTime.TryParse(value, out _);
        }

        private static readonly HashSet<Type> NumericTypes = new HashSet<Type>
        {
            typeof(int),
            typeof(uint),
            typeof(double),
            typeof(decimal)
        };

        public static bool IsNumericType(this Type type)
        {
            return NumericTypes.Contains(type) ||
                   NumericTypes.Contains(Nullable.GetUnderlyingType(type));
        }

        public static bool IsDateTime(this Type type)
        {
            return type.Name == "DateTime";
        }

        public static string RemoveIllegalChars(this string value)
        {
            return value.Replace(",", "");
        }
    }

}
