using EPYSLTEX.Core.DTOs;
using Newtonsoft.Json;
using System;
using System.Text;

namespace EPYSLTEX.Web.Extends.Helpers
{
    public interface ICommonHelpers
    {
        FilterByExpressionModel GetFilterByModel(string value);
        string GetFilterBy(string value);
    }

    public class CommonHelpers : ICommonHelpers
    {
        public string GetFilterBy(string value)
        {
            var filterBy = string.Empty;
            if (!string.IsNullOrEmpty(value))
            {
                var singleFilter = true;
                dynamic filterObj = JsonConvert.DeserializeObject(value);
                foreach (var item in filterObj)
                {
                    var appendAnd = singleFilter ? "" : " And ";
                    filterBy += $"{appendAnd}{item.Name} like '%{item.Value.Value}%'";
                    singleFilter = false;
                }
            }

            return filterBy;
        }

        public FilterByExpressionModel GetFilterByModel(string value)
        {
            FilterByExpressionModel filterExpressionModel = null;

            if (!string.IsNullOrEmpty(value))
            {
                filterExpressionModel = new FilterByExpressionModel();
                StringBuilder expression = null;
                var singleFilter = true;
                dynamic filterObj = JsonConvert.DeserializeObject(value);
                int i = 0;
                foreach (var item in filterObj)
                {
                    if (string.IsNullOrEmpty(Convert.ToString(item.Value)))
                        continue;

                    var appendAnd = singleFilter ? "" : " AND ";
                    expression = new StringBuilder();
                    expression.Append($"{appendAnd}{item.Name}.ToString().Contains(@{i++})");
                    singleFilter = false;

                    filterExpressionModel.Parameters.Add(Convert.ToString(item.Value));
                }

                filterExpressionModel.Expression = expression.ToString();
            }

            return filterExpressionModel;
        }



        public static void SetNullsToDefaultValues(object obj)
        {
            var properties = obj.GetType().GetProperties();

            foreach (var property in properties)
            {
                // Only handle properties that are not readonly and are public
                if (property.CanWrite)
                {
                    var value = property.GetValue(obj);

                    // Check if the property is nullable and if the value is null
                    if (value == null)
                    {
                        // If it's a nullable value type, set it to 0 or other default value
                        if (Nullable.GetUnderlyingType(property.PropertyType) != null)
                        {
                            // Set to 0 for numeric types (int?, double?, etc.)
                            if (property.PropertyType == typeof(int?))
                                property.SetValue(obj, 0);
                            else if (property.PropertyType == typeof(double?))
                                property.SetValue(obj, 0.0);
                            else if (property.PropertyType == typeof(decimal?))
                                property.SetValue(obj, 0.0m);
                            else if (property.PropertyType == typeof(DateTime?))
                                property.SetValue(obj, DateTime.MinValue); // Set to a default date
                        }
                        // If it's a reference type (string, object), set to a default value
                        else if (property.PropertyType == typeof(string))
                        {
                            property.SetValue(obj, string.Empty); // Empty string for strings
                        }
                        else
                        {
                            // For other reference types, you can set to null, or a default value if needed
                            property.SetValue(obj, null);
                        }
                    }
                }
            }
        }
    }
}
