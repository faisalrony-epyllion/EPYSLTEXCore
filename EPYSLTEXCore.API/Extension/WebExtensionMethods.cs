using EPYSLTEXCore.Infrastructure.Static;
using System.Collections.Specialized;

namespace EPYSLTEXCore.API.Extension
{
    public static class WebExtensionMethods
    {
        public static T ConvertToObject<T>(this NameValueCollection formData)
        {
            var objT = Activator.CreateInstance<T>();
            var properties = typeof(T).GetProperties();
            foreach (var pro in properties)
            {
                if (formData.AllKeys.Any(x => x.Equals(pro.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    try
                    {
                        if (typeof(System.Data.Entity.EntityState).IsAssignableFrom(pro.PropertyType))
                        {
                            Enum.TryParse(formData.Get(pro.Name), out System.Data.Entity.EntityState entityState);
                            pro.SetValue(objT, entityState);
                        }
                        else
                        {
                            Type t = pro.PropertyType;
                            t = Nullable.GetUnderlyingType(t) ?? t;

                            if (t.IsGenericType || t.IsArray) continue;

                            object value;
                            if (t.IsNumericType()) value = string.IsNullOrEmpty(formData.Get(pro.Name)) ? null : Convert.ChangeType(formData.Get(pro.Name).RemoveIllegalChars(), t);
                            else if (t.IsDateTime()) value = string.IsNullOrEmpty(formData.Get(pro.Name)) ? (DateTime?)null : DateTime.ParseExact(formData.Get(pro.Name), DateFormats.DEFAULT_DATE_FORMAT, null);
                            else value = string.IsNullOrEmpty(formData.Get(pro.Name)) ? null : Convert.ChangeType(formData.Get(pro.Name), t);

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
}
