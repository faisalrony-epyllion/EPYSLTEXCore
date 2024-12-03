using EPYSLTEXCore.Infrastructure.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Static
{
    public class FabricComponentType
    {
        public String FabricType { get; set; }
        public int Value { get; set; }
        //public Boolean IsCotton { get; set; }
    }
    public static class CommonConstent
    {
        public static string StockMigrationDate = "25-Mar-2024";
        public static string StockMigrationDate_For_SS = "15-Aug-2024";

        public static string StockMigrationDate_2 = "09-Sep-2024";

        public static DateTime YarnSourcingModeImplementDate = Convert.ToDateTime("08-Sep-2024"); //Also in sp_Validation_ProjectionYarnBookingItemChildDetails
    }
    public static class CommonFunction
    {
        public static string GetYarnShortForm(String YarnComposition, String YarnType, String ManufacturingProcess, String ManufacturingSubProcess, String QualityParameter, String YarnCount, String ShadeCode)
        {
            String shortForm = String.Empty;

            if (YarnComposition.IsNullOrEmpty()) YarnComposition = "";
            if (YarnType.IsNullOrEmpty()) YarnType = "";
            if (ManufacturingProcess.IsNullOrEmpty()) ManufacturingProcess = "";
            if (ManufacturingSubProcess.IsNullOrEmpty()) ManufacturingSubProcess = "";
            if (QualityParameter.IsNullOrEmpty()) QualityParameter = "";
            if (YarnCount.IsNullOrEmpty()) YarnCount = "";
            if (ShadeCode.IsNullOrEmpty()) ShadeCode = "";

            #region General Rule
            if (ManufacturingProcess.ToLower().Contains("carded"))
            {
                ManufacturingProcess = "";
            }
            if (YarnType.ToLower().Contains("ring spinning"))
            {
                YarnType = "Ring";
            }
            if (QualityParameter.ToLower().Contains("conta free"))
            {
                QualityParameter = "CF";
            }
            #endregion

            if (YarnCount.ToLower().Contains("ne") || YarnCount.ToLower().Contains("nm"))
            {
                #region Ne or NM
                if (YarnComposition.Contains("100% ") && YarnComposition.ToLower().Contains(" cotton"))
                {
                    if (YarnType.ToLower().Contains("ring"))
                    {
                        YarnType = "";
                    }
                    shortForm = YarnCount + " " + YarnComposition + " " + ManufacturingProcess + " " + ManufacturingSubProcess + " " + YarnType + " " + QualityParameter + " " + ShadeCode;
                }
                else if (QualityParameter.ToLower().Contains("grey melange"))
                {
                    shortForm = YarnCount + " GM " + YarnComposition + " " + ManufacturingProcess + " " + ManufacturingSubProcess + " " + YarnType + " " + QualityParameter + " " + ShadeCode;
                }
                else if (YarnComposition.ToLower().Contains(" cotton") && YarnComposition.ToLower().Contains(" polyester"))
                {
                    List<FabricComponentType> lstFCT = GetFabricType(YarnComposition);
                    if (lstFCT.Count > 0)
                    {
                        int CottonValue = 0, PolyValue = 0;
                        #region Getting Cotton & Polyester Value
                        FabricComponentType fc = lstFCT.Find(i => i.FabricType.Trim().ToLower().Contains("cotton"));
                        if (fc.IsNotNull())
                        {
                            CottonValue = fc.Value;
                        }
                        fc = lstFCT.Find(i => i.FabricType.Trim().ToLower().Contains("polyester"));
                        if (fc.IsNotNull())
                        {
                            PolyValue = fc.Value;
                        }
                        #endregion
                        if (CottonValue >= PolyValue && CottonValue > 0 && PolyValue > 0)
                        {
                            shortForm = YarnCount + " CVC " + YarnComposition + " " + ManufacturingProcess + " " + ManufacturingSubProcess + " " + YarnType + " " + QualityParameter + " " + ShadeCode;
                        }
                        else if (CottonValue < PolyValue && CottonValue > 0 && PolyValue > 0)
                        {
                            shortForm = YarnCount + " PC " + YarnComposition + " " + ManufacturingProcess + " " + ManufacturingSubProcess + " " + YarnType + " " + QualityParameter + " " + ShadeCode;
                        }
                        else
                        {
                            shortForm = YarnCount + " CVC " + YarnComposition + " " + ManufacturingProcess + " " + ManufacturingSubProcess + " " + YarnType + " " + QualityParameter + " " + ShadeCode;
                        }
                    }
                }
                else
                {
                    shortForm = YarnCount + " " + YarnComposition + " " + ManufacturingProcess + " " + ManufacturingSubProcess + " " + YarnType + " " + QualityParameter + " " + ShadeCode;
                }
                #endregion
            }
            else if (YarnCount.ToLower().Contains("d") && !ManufacturingProcess.ToLower().Contains("covered"))
            {
                #region D && not Covered
                if (YarnType.Trim().ToLower().Contains("spandex") || YarnType.Trim().ToLower().Contains("elastane") || ManufacturingSubProcess.ToLower().Contains("lurex"))
                {
                    shortForm = YarnCount + " " + YarnComposition + " " + QualityParameter + " " + ShadeCode;
                }
                else
                {
                    shortForm = YarnCount + " " + YarnComposition + " " + YarnType + " " + ManufacturingProcess + " " + ManufacturingSubProcess + " " + QualityParameter + " " + ShadeCode;
                }
                #endregion
            }
            else
            {
                shortForm = YarnCount + " " + YarnComposition + " " + YarnType + " " + ManufacturingProcess + " " + ManufacturingSubProcess + " " + QualityParameter + " " + ShadeCode;
            }
            shortForm = shortForm.Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace(" N/A", "").Replace(" n/a", "");
            return shortForm;
        }
        public static List<FabricComponentType> GetFabricType(String YarnComposition)
        {
            List<FabricComponentType> lstFCT = new List<FabricComponentType>();
            String[] allComp = YarnComposition.Split(' ');
            FabricComponentType fct = new FabricComponentType();
            foreach (String item in allComp)
            {
                if (item.Contains("%"))
                {
                    fct = new FabricComponentType();
                    fct.Value = item.Replace("%", "").ToInt();
                    lstFCT.Add(fct);
                }
                else
                {
                    if (fct.FabricType != null && fct.FabricType.Length == 0)
                    {
                        fct.FabricType = item.Trim();
                    }
                    else
                    {
                        fct.FabricType += " " + item.Trim();
                    }
                    //if (item.ToLower().Contains("cotton"))
                    //{
                    //    fct.IsCotton = true;
                    //}
                }
            }
            return lstFCT;
        }
        public static T DeepClone<T>(T instance)
        {
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(instance));
        }
        public static string GetNewGuid()
        {
            return Guid.NewGuid().ToString("N").ToUpper();
        }
        public static string GetNumberDisplayValue(decimal value)
        {
            return value > 0 ? value.ToString("N2") : "-";
        }
        public static string ReplaceInvalidChar(string pValue)
        {
            pValue = pValue.IsNullOrEmpty() ? "" : pValue;
            pValue = pValue.Replace("_PPPPP_", "%");
            pValue = pValue.Replace("_QQQQQ_", " ");
            pValue = pValue.Replace("_AAAAA_", "&");
            pValue = pValue.Replace("_DDDDD_", "\\");
            pValue = pValue.Replace("_XXXXX_", "/");
            pValue = pValue.Replace("_ZZZZZ_", "");
            pValue = pValue.Replace("_GGGGG_", ">");
            pValue = pValue.Replace("_LLLLL_", "<");
            pValue = pValue.Replace("_EEEEE_", "=");
            pValue = pValue.Replace("_TTTTT_", ".");
            pValue = pValue.Replace("_BBBBB_", "+");
            pValue = pValue.Replace("_CCCCC_", "-");
            pValue = pValue.Replace("_FFFFF_", "#");
            pValue = pValue.Replace("DefaultText", "");
            return pValue;
        }
        public static string GetDefaultValueWhenInvalidS(string pValue)
        {
            if (pValue.IsNullOrEmpty()) return "";
            return pValue.Trim();
        }
        public static bool IsNumberValue(string pValue)
        {
            if (pValue.IsNullOrEmpty()) pValue = "0";
            double n;
            return double.TryParse(pValue, out n);
        }
        public static string GetDistinct(string pValue)
        {
            var value = CommonFunction.GetDefaultValueWhenInvalidS(pValue).Split(',');
            pValue = string.Join(",", value.Distinct());
            return pValue;
        }
        public static List<Select2OptionModel> GetDayValidDurations(IEnumerable<Select2OptionModel> allDays, string usedDayValidDurationId)
        {
            var usedIds = usedDayValidDurationId.Split(',').Distinct().ToList();
            if (usedIds.Count() == 0) return allDays.ToList().Where(x => x.desc == 1.ToString()).ToList();

            List<Select2OptionModel> tempList = new List<Select2OptionModel>();
            allDays.ToList().ForEach(x =>
            {
                var obj = tempList.Find(y => y.id == x.id);
                if (obj.IsNull())
                {
                    if (x.desc == 1.ToString()) tempList.Add(x);
                    else
                    {
                        var index = usedIds.FindIndex(z => z == x.id);
                        if (index > -1) tempList.Add(x);
                    }
                }
            });
            tempList.Insert(0, new Select2OptionModel()
            {
                id = 0.ToString(),
                text = "Empty",
                additionalValue = 0.ToString(),
                desc = 1.ToString()
            });
            return tempList;
        }
        public static string GetDateInString(DateTime? dateTime)
        {
            if (dateTime == null) return "";
            DateTime dt = (DateTime)dateTime;
            return dt.ToString("dd-MMM-yyyy");
        }
    }
}
