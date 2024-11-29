using EPYSLTEX.Core.DTOs;
using EPYSLTEXCore.Application.DTO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace System.Net.Http
{
    public static class RequestExtension
    {
        public static PaginationInfo GetPaginationInfo(this HttpRequest request)
        {
            var paginationInfo = new PaginationInfo();

            // Extract query parameters using request.Query
            var keyValuePairs = request.Query.ToDictionary(kv => kv.Key, kv => kv.Value.ToString());

            // Default to "ej2" if not specified
            paginationInfo.GridType = keyValuePairs
                .FirstOrDefault(x => x.Key.Equals("gridType", StringComparison.OrdinalIgnoreCase)).Value ?? "ej2";

            if (paginationInfo.GridType.Equals("bootstrap-table", StringComparison.OrdinalIgnoreCase))
            {
                var filterValue = keyValuePairs.FirstOrDefault(x => x.Key.Equals("filter", StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrEmpty(filterValue.Value))
                {
                    var filterValueList = new List<string>();
                    dynamic filterObj = JsonConvert.DeserializeObject(filterValue.Value);
                    foreach (var item in filterObj)
                    {
                        filterValueList.Add($"{item.Name} like '%{item.Value.Value}%'");
                    }

                    paginationInfo.FilterBy = $"Where {string.Join(" And ", filterValueList)}";
                }

                var sort = keyValuePairs.FirstOrDefault(x => x.Key.Equals("sort", StringComparison.OrdinalIgnoreCase));
                var order = keyValuePairs.FirstOrDefault(x => x.Key.Equals("order", StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrEmpty(sort.Value) && !string.IsNullOrEmpty(order.Value))
                {
                    paginationInfo.OrderBy = $"Order By {sort.Value} {order.Value}";
                }

                var skip = keyValuePairs.FirstOrDefault(x => x.Key.Equals("offset", StringComparison.OrdinalIgnoreCase));
                var take = keyValuePairs.FirstOrDefault(x => x.Key.Equals("limit", StringComparison.OrdinalIgnoreCase));

                if (!string.IsNullOrEmpty(skip.Value) && !string.IsNullOrEmpty(take.Value))
                {
                    paginationInfo.PageBy = $"Offset {skip.Value} Rows Fetch Next {take.Value} Rows Only";
                    paginationInfo.PageByNew = $"R_No_New BETWEEN {Convert.ToInt32(skip.Value)} AND {Convert.ToInt32(skip.Value) - 1 + Convert.ToInt32(take.Value)} ";
                }
            }
            else
            {
                var filterValue = keyValuePairs.FirstOrDefault(x => x.Key.Equals("$filter", StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrEmpty(filterValue.Value))
                {
                    var filterValueString = filterValue.Value;
                    var split2FinalString = string.Empty;
                    bool isApplySplite2FinalString = false;

                    // Check and process the filter string for date ranges
                    var filterValues1 = filterValueString.Split(new string[] { "or" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var fv in filterValues1)
                    {
                        if (fv.Contains(" gt datetime") && fv.Contains(" and ") && fv.Contains(" lt datetime"))
                        {
                            isApplySplite2FinalString = true;
                            var split2 = fv.Split(new string[] { "and" }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (var splitObj in split2)
                            {
                                if (splitObj.Contains("lt datetime"))
                                {
                                    var value1 = splitObj.Split(new string[] { " lt datetime" }, StringSplitOptions.RemoveEmptyEntries);
                                    string fieldName = ReplaceUselessStrings(value1[0]).Trim();

                                    if (!string.IsNullOrEmpty(split2FinalString))
                                    {
                                        if (split2FinalString.Contains(fieldName))
                                        {
                                            split2FinalString += " or ";
                                        }
                                        else
                                        {
                                            split2FinalString += " and ";
                                        }
                                    }

                                    var dateString = splitObj.Replace("lt", "eq");
                                    dateString = dateString.Replace("datetime", "datetime DDDDDDD");
                                    split2FinalString += dateString;
                                }
                                else if (!splitObj.Contains("gt datetime"))
                                {
                                    var fn = splitObj.Split(new string[] { "eq" }, StringSplitOptions.RemoveEmptyEntries);
                                    var fieldName = ReplaceUselessStrings(fn[0]).Trim();
                                    if (split2FinalString.Contains(fieldName))
                                    {
                                        split2FinalString += " or ";
                                    }
                                    else
                                    {
                                        split2FinalString += " and ";
                                    }

                                    split2FinalString += splitObj;
                                }
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(split2FinalString)) split2FinalString += " and ";
                            split2FinalString += fv;
                        }
                    }
                    if (!string.IsNullOrEmpty(split2FinalString) && isApplySplite2FinalString)
                    {
                        filterValueString = split2FinalString;
                    }

                    var filterValueList = new List<string>();
                    var filterValues = filterValueString.Split(new string[] { "and" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var fValue in filterValues)
                    {
                        if (fValue.Contains(" eq datetime") && (fValue.Contains(" or") || fValue.Contains(" DDDDDDD")))
                        {
                            var fValueStr = RequestExtension.GetDateCondition2(fValue);
                            filterValueList.Add(fValueStr);
                        }
                        else if (fValue.Contains(" gt datetime") ||
                                 fValue.Contains(" lt datetime") ||
                                 fValue.Contains(" eq datetime") ||
                                 fValue.Contains(" le datetime") ||
                                 fValue.Contains(" ge datetime"))
                        {
                            var fValueStr = RequestExtension.GetDateCondition(fValue);
                            filterValueList.Add(fValueStr);
                        }
                        else
                        {
                            var fValueStr = ReplaceUselessStrings2(fValue);
                            filterValueList.Add(fValueStr);
                        }
                    }

                    paginationInfo.FilterBy = $"Where {string.Join(" And ", filterValueList)}";
                }

                var orderBy = keyValuePairs.FirstOrDefault(x => x.Key.Equals("$orderby", StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrEmpty(orderBy.Value))
                    paginationInfo.OrderBy = $"Order By {orderBy.Value}";

                var skip = keyValuePairs.FirstOrDefault(x => x.Key.Equals("$skip", StringComparison.OrdinalIgnoreCase));
                var take = keyValuePairs.FirstOrDefault(x => x.Key.Equals("$top", StringComparison.OrdinalIgnoreCase));

                if (!string.IsNullOrEmpty(skip.Value) && !string.IsNullOrEmpty(take.Value))
                {
                    paginationInfo.PageBy = $"Offset {skip.Value} Rows Fetch Next {take.Value} Rows Only";
                    paginationInfo.PageByNew = $"R_No_New BETWEEN {Convert.ToInt32(skip.Value)} AND {Convert.ToInt32(skip.Value) - 1 + Convert.ToInt32(take.Value)} ";
                }
            }

            return paginationInfo;
        }
        private static string ReplaceUselessStrings(string value)
        {
            value = value.Replace("tolower", "");
            value = value.Replace("(", "");
            value = value.Replace(")", "");
            value = value.Replace("startswith", "");
            value = value.Replace("endswith", "");
            value = value.Replace("substringof", "");
            value = value.Replace("'", "");
            return value;
        }
        private static string ReplaceUselessStrings2(string value)
        {
            value = value.Replace("tolower", "");
            value = value.Replace("(", "");
            value = value.Replace(")", "");
            value = value.Replace("startswith", "");
            value = value.Replace("endswith", "");
            value = value.Replace("substringof", "");
            return value;
        }
        private static string GetDateCondition(string value)
        {
            /*
              "((ReceiveDate gt datetime*2023-06-12T17:59:59.999Z*) "
                    (ReceiveDate lt datetime'2023-06-13T18:00:00.000Z'))
            */

            string fieldName = value.Trim().Split(' ')[0];
            fieldName = fieldName.Replace("(", "");
            fieldName = "CAST(" + fieldName + " AS DATE)";

            value = value.Replace("'", "*");
            string dateString = value.Split('*')[1];
            dateString = dateString.Split(':')[0];
            dateString = dateString.Split('T')[0];

            var nextDate = Convert.ToDateTime(dateString).AddDays(1);
            dateString = nextDate.Year + "-" + nextDate.Month.ToString("00") + "-" + nextDate.Day.ToString("00");

            if (value.Contains("gt datetime"))
            {
                return fieldName + " > " + "'" + dateString + "'";
            }
            else if (value.Contains("lt datetime"))
            {
                return fieldName + " < " + "'" + dateString + "'";
            }
            else if (value.Contains("eq datetime"))
            {
                return fieldName + " = " + "'" + dateString + "'";
            }
            else if (value.Contains("le datetime"))
            {
                return fieldName + " <= " + "'" + dateString + "'";
            }
            else if (value.Contains("ge datetime"))
            {
                return fieldName + " >= " + "'" + dateString + "'";
            }
            return fieldName + " = " + "'" + dateString + "'";
        }
        private static string GetDateCondition2(string value)
        {
            value = value.Replace("DDDDDDD", "");

            string result = string.Empty;
            /*
              (FabricBookingDate eq datetime'2023-02-23T18:00:00.000Z')) or (FabricBookingDate eq datetime'2023-02-24T18:00:00.000Z')))
            */

            var value1 = value.Split(new string[] { " eq datetime" }, StringSplitOptions.RemoveEmptyEntries);
            string fieldName = "CAST(" + ReplaceUselessStrings(value1[0]) + " AS DATE)";

            var fields = value.Split(new string[] { "or" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var field in fields)
            {
                var fieldObj = field.Split(new string[] { " eq datetime" }, StringSplitOptions.RemoveEmptyEntries);
                var fieldObj1 = fieldObj[1].Replace("'", "*");
                string dateString = fieldObj1.Split('*')[1];
                dateString = dateString.Split(':')[0];
                dateString = dateString.Split('T')[0];

                var nextDate = Convert.ToDateTime(dateString);
                dateString = nextDate.Year + "-" + nextDate.Month.ToString("00") + "-" + nextDate.Day.ToString("00");

                if (result.IsNotNullOrEmpty()) result += " or ";
                result += fieldName + " = " + "'" + dateString + "'";
            }
            return result;
        }
    }
}