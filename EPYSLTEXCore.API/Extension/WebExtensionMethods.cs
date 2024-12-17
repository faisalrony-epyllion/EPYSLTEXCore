using EPYSLTEXCore.Infrastructure.Static;

public static class WebExtensionMethods
{
    public static T ConvertToObject<T>(this IFormCollection formData)
    {
        var objT = Activator.CreateInstance<T>();
        var properties = typeof(T).GetProperties();

        foreach (var pro in properties)
        {
            if (formData.ContainsKey(pro.Name))
            {
                try
                {
                    if (typeof(System.Data.Entity.EntityState).IsAssignableFrom(pro.PropertyType))
                    {
                        Enum.TryParse(formData[pro.Name], out System.Data.Entity.EntityState entityState);
                        pro.SetValue(objT, entityState);
                    }
                    else
                    {
                        Type t = pro.PropertyType;
                        t = Nullable.GetUnderlyingType(t) ?? t; // Get the underlying type if it's nullable

                        if (t.IsGenericType || t.IsArray)
                            continue; // Skip collection or generic types, if required

                        object value = null;

                        if (string.IsNullOrEmpty(formData[pro.Name]))
                        {
                            value = null; // Assign null if the form data is empty
                        }
                        else
                        {
                            // Special case for string properties: directly assign the value
                            if (t == typeof(string))
                            {
                                value = formData[pro.Name].ToString(); // Direct assignment for string properties
                            }
                            // Handle numeric types (e.g., int, double, etc.)
                            else if (t.IsNumericType())
                            {
                                value = Convert.ChangeType(formData[pro.Name].ToString().RemoveIllegalChars(), t);
                            }
                            // Handle DateTime types
                            else if (t.IsDateTime())
                            {
                                value = DateTime.ParseExact(formData[pro.Name], DateFormats.DEFAULT_DATE_FORMAT, null);
                            }
                            else if (t.Name=="Boolean")
                            {
                                string formValue = formData[pro.Name].ToString();
                                value = !string.IsNullOrEmpty(formValue) && bool.TryParse(formValue, out bool boolValue) ? (bool?)boolValue : null;
                            }
                            else
                            {
                                value = Convert.ChangeType(formData[pro.Name], t);
                            }
                        }

                        pro.SetValue(objT, value);

                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        return objT;
    }
}
