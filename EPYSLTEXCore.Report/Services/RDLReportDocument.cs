using EPYSLTEXCore.Report.Entities;
using EPYSLTEXCore.Report.Repositories;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Xml;

namespace EPYSLTEXCore.Report.Service
{
    public class RDLReportDocument
    {
        private readonly IReportSuiteRepository _reportSuiteRepository;

        public RDLReportDocument()
        {
            _reportSuiteRepository = DependencyResolver.Current.GetService<IReportSuiteRepository>();
            DsReport = new RDLDataSet();
            DsSource = new DataSet();
            Parameters = new List<RDLParameter>();
            Fields = new List<RDLColumn>();
            BodyTablesName = new List<string>();
            FilterSetList = new List<FilterSets>();
            SubReportList = new List<RDLReportDocument>();
        }

        public RDLReportDocument(IReportSuiteRepository reportSuiteRepository)
        {
            _reportSuiteRepository = reportSuiteRepository;
            DsReport = new RDLDataSet();
            DsSource = new DataSet();
            Parameters = new List<RDLParameter>();
            Fields = new List<RDLColumn>();
            BodyTablesName = new List<string>();
            FilterSetList = new List<FilterSets>();
            SubReportList = new List<RDLReportDocument>();
        }

        private XmlDocument xmlDocument = null;

        public string UserCode { get; set; }
        public string Name { get; set; }
        public string ReportPathWithOutName { get; set; }
        public string ReportPath { get; set; }
        public DataSet DsSource { get; set; }
        public RDLDataSet DsReport { get; set; }
        public List<FilterSets> FilterSetList { get; set; }
        public List<RDLParameter> Parameters { get; set; }
        public List<RDLColumn> Fields { get; set; }
        public List<string> BodyTablesName { get; set; }
        public List<RDLReportDocument> SubReportList { get; set; }

        public void SetFields(string userCode, string name, string reportPathWithoutName)
        {
            UserCode = userCode;
            Name = name;
            ReportPathWithOutName = reportPathWithoutName;
        }

        public void LoadFilterTable(DataColumnCollection dcCollection, List<ReportSuiteColumnValueViewModel> reportColumnList)
        {
            FilterSetList = new List<FilterSets>();
            FilterSets newFilter;
            bool HasParent = false;

            #region Filter Parameter Column
            if (reportColumnList.Any())
            {
                #region if report column exists
                foreach (var rp in Parameters)
                {
                    var reportColumn = reportColumnList.Find(item => item.ColumnId == rp.Name);
                    if (reportColumn != null)
                    {
                        if (!string.IsNullOrEmpty(reportColumn.ParentColumns))
                        {
                            AddOtherColumnToFilterTable(dcCollection, reportColumnList, reportColumn.ParentColumns, false);
                            HasParent = true;
                        }
                        else
                            HasParent = false;
                    }
                    AddParameterColumnToFilterTable(dcCollection, reportColumnList, rp, reportColumn, HasParent);
                }
                #endregion
            }
            else
            {
                foreach (var rp in Parameters)
                {
                    var reportColumn = reportColumnList.Find(item => item.ColumnId == rp.Name);
                    HasParent = false;
                    AddParameterColumnToFilterTable(dcCollection, reportColumnList, rp, reportColumn, HasParent);
                }
            }
            #endregion
            #region Add Other Column
            foreach (DataColumn dc in dcCollection)
            {
                var reportColumn = reportColumnList.Find(item => item.ColumnId == dc.ColumnName);
                HasParent = reportColumn != null && !string.IsNullOrEmpty(reportColumn.ParentColumns);
                AddOtherColumnToFilterTable(dcCollection, reportColumnList, dc.ColumnName, HasParent);
            }
            #endregion
            #region Add Empty Column
            foreach (string tableName in BodyTablesName)
            {
                newFilter = new FilterSets
                {
                    ColumnName = "Expression",
                    DataType = "Expression",
                    TableName = tableName,
                    OrAnd = "And",
                    Operators = "=",
                    IsParameter = false
                };
                FilterSetList.Add(newFilter);
                break;
            }
            #endregion
        }

        public void AddParameterColumnToFilterTable(DataColumnCollection dcCollection, List<ReportSuiteColumnValueViewModel> reportColumnList, RDLParameter rp, ReportSuiteColumnValueViewModel reportColumn, Boolean HasParent)
        {
            FilterSets newFilter;
            RDLColumn rc;
            try
            {
                #region Add Parameter Column
                rc = Fields.Find(item => item.Name == rp.Name);
                if (!FilterSetList.Any(item => item.ColumnName == rp.Name && (item.TableName == "Parameter" || item.TableName == rc.TableName)))
                {
                    newFilter = new FilterSets
                    {
                        ColumnName = rp.Name,
                        DataType = rp.DbType.ToString(),
                        TableName = "Parameter",
                        OrAnd = "And",
                        Operators = "=",
                        Caption = "***",
                        IsParameter = true,
                        HasParent = HasParent
                    };

                    if (rp.Name.Equals("UserCode", StringComparison.OrdinalIgnoreCase))
                    {
                        newFilter.ColumnValue = UserCode;
                        newFilter.DefaultValue = UserCode;
                    }
                    else
                        newFilter.DefaultValue = rp.DefaultValue == null ? string.Empty : rp.DefaultValue.ToString();

                    if (reportColumn != null)
                    {
                        newFilter.DefaultValue = reportColumn.DefaultValue ?? string.Empty;
                        newFilter.ParentColumn = reportColumn.ParentColumns ?? string.Empty;
                        newFilter.DAOClass = reportColumn.DaoClass ?? string.Empty;
                        newFilter.MethodName = reportColumn.MethodName ?? string.Empty;
                        newFilter.ValueColumnId = reportColumn.ValueColumnId ?? string.Empty;
                        newFilter.DisplayColumnId = reportColumn.DisplayColumnId ?? string.Empty;
                        newFilter.ShowAdditionalColumn = reportColumn.ShowAdditionalColumn;
                        newFilter.AdditionalColumnId = reportColumn.AdditionalColumnId ?? string.Empty;
                        newFilter.AdditionalColumnHeader = reportColumn.AdditionalColumnHeader ?? string.Empty;
                        newFilter.ColumnWidth = reportColumn.ColumnWidth ?? string.Empty;
                        newFilter.IsMultipleSelection = reportColumn.IsMultipleSelection;
                    }

                    if (newFilter.DataType == "Boolean" || newFilter.DataType == "System.Boolean")
                        newFilter.ColumnValue = "false";

                    FilterSetList.Add(newFilter);
                }
                #endregion

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void AddOtherColumnToFilterTable(DataColumnCollection dcCollection, List<ReportSuiteColumnValueViewModel> reportColumnList, string ColumnName, bool HasParent)
        {
            FilterSets newFilter;
            RDLColumn rc;
            try
            {
                #region Add Other Column
                string[] strColumnArr = ColumnName.Split(',');
                foreach (string strColumn in strColumnArr)
                {
                    if (strColumn.Equals("UserCode", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!FilterSetList.Any(x => x.ColumnName == strColumn))
                        {
                            newFilter = new FilterSets
                            {
                                ColumnName = strColumn,
                                DataType = "System.Int32",
                                TableName = string.Empty,
                                OrAnd = "And",
                                Operators = "=",
                                ColumnValue = UserCode,
                                DefaultValue = UserCode,
                                IsHidden = true
                            };

                            FilterSetList.Add(newFilter);
                        }
                    }
                    else
                    {
                        rc = Fields.Find(x => x.Name == strColumn);
                        var reportColumn = reportColumnList.Find(x => x.ColumnId == strColumn);
                        if (rc != null)
                        {
                            if (!FilterSetList.Any(item => item.ColumnName == strColumn && (item.TableName == "Parameter" || item.TableName == rc.TableName)))
                            {
                                if (reportColumn != null)
                                {
                                    if (!string.IsNullOrEmpty(reportColumn.ParentColumns))
                                    {
                                        AddOtherColumnToFilterTable(dcCollection, reportColumnList, reportColumn.ParentColumns, false);
                                        #region Add Filter Set
                                        newFilter = new FilterSets
                                        {
                                            ColumnName = rc.Name,
                                            DataType = rc.DataType,
                                            TableName = rc.TableName,
                                            OrAnd = "And",
                                            Operators = "=",
                                            IsParameter = false,
                                            HasParent = true
                                        };

                                        if (reportColumn != null)
                                        {
                                            newFilter.DefaultValue = reportColumn.DefaultValue ?? string.Empty;
                                            newFilter.ParentColumn = reportColumn.ParentColumns ?? string.Empty;
                                            newFilter.DAOClass = reportColumn.DaoClass ?? string.Empty;
                                            newFilter.MethodName = reportColumn.MethodName ?? string.Empty;
                                            newFilter.ValueColumnId = reportColumn.ValueColumnId ?? string.Empty;
                                            newFilter.DisplayColumnId = reportColumn.DisplayColumnId ?? string.Empty;
                                            newFilter.IsMultipleSelection = reportColumn.IsMultipleSelection;
                                            newFilter.ShowAdditionalColumn = reportColumn.ShowAdditionalColumn;
                                            newFilter.AdditionalColumnId = reportColumn.AdditionalColumnId ?? string.Empty;
                                            newFilter.AdditionalColumnHeader = reportColumn.AdditionalColumnHeader ?? string.Empty;
                                            newFilter.ColumnWidth = reportColumn.ColumnWidth ?? string.Empty;
                                        }
                                        if (rc.DataType == "Boolean" || rc.DataType == "System.Boolean")
                                        {
                                            newFilter.ColumnValue = "false";
                                            newFilter.DefaultValue = "false";
                                        }

                                        FilterSetList.Add(newFilter);
                                        #endregion
                                    }
                                    else
                                    {
                                        #region Add Filter Set
                                        newFilter = new FilterSets
                                        {
                                            ColumnName = rc.Name,
                                            DataType = rc.DataType,
                                            TableName = rc.TableName,
                                            OrAnd = "And",
                                            Operators = "=",
                                            IsParameter = false,
                                            HasParent = HasParent
                                        };

                                        if (reportColumn != null)
                                        {
                                            newFilter.DefaultValue = reportColumn.DefaultValue ?? string.Empty;
                                            newFilter.ParentColumn = reportColumn.ParentColumns ?? string.Empty;
                                            newFilter.DAOClass = reportColumn.DaoClass ?? string.Empty;
                                            newFilter.MethodName = reportColumn.MethodName ?? string.Empty;
                                            newFilter.ValueColumnId = reportColumn.ValueColumnId ?? string.Empty;
                                            newFilter.DisplayColumnId = reportColumn.DisplayColumnId ?? string.Empty;
                                            newFilter.IsMultipleSelection = reportColumn.IsMultipleSelection;
                                            newFilter.ShowAdditionalColumn = reportColumn.ShowAdditionalColumn;
                                            newFilter.AdditionalColumnId = reportColumn.AdditionalColumnId ?? string.Empty;
                                            newFilter.AdditionalColumnHeader = reportColumn.AdditionalColumnHeader ?? string.Empty;
                                            newFilter.ColumnWidth = reportColumn.ColumnWidth ?? string.Empty;
                                        }
                                        if (rc.DataType == "Boolean" || rc.DataType == "System.Boolean")
                                        {
                                            newFilter.ColumnValue = "false";
                                            newFilter.DefaultValue = "false";
                                        }

                                        FilterSetList.Add(newFilter);
                                        #endregion
                                    }
                                }
                                else
                                {
                                    #region Add Filter Set
                                    newFilter = new FilterSets
                                    {
                                        ColumnName = rc.Name,
                                        DataType = rc.DataType,
                                        TableName = rc.TableName,
                                        OrAnd = "And",
                                        Operators = "=",
                                        IsParameter = false,
                                        HasParent = HasParent
                                    };

                                    if (reportColumn != null)
                                    {
                                        newFilter.DefaultValue = reportColumn.DefaultValue ?? string.Empty;
                                        newFilter.ParentColumn = reportColumn.ParentColumns ?? string.Empty;
                                        newFilter.DAOClass = reportColumn.DaoClass ?? string.Empty;
                                        newFilter.MethodName = reportColumn.MethodName ?? string.Empty;
                                        newFilter.ValueColumnId = reportColumn.ValueColumnId ?? string.Empty;
                                        newFilter.DisplayColumnId = reportColumn.DisplayColumnId ?? string.Empty;
                                        newFilter.IsMultipleSelection = reportColumn.IsMultipleSelection;
                                        newFilter.ShowAdditionalColumn = reportColumn.ShowAdditionalColumn;
                                        newFilter.AdditionalColumnId = reportColumn.AdditionalColumnId ?? string.Empty;
                                        newFilter.AdditionalColumnHeader = reportColumn.AdditionalColumnHeader ?? string.Empty;
                                        newFilter.ColumnWidth = reportColumn.ColumnWidth ?? string.Empty;
                                    }
                                    if (rc.DataType == "Boolean" || rc.DataType == "System.Boolean")
                                    {
                                        newFilter.ColumnValue = "false";
                                        newFilter.DefaultValue = "false";
                                    }
                                    FilterSetList.Add(newFilter);
                                    #endregion
                                }
                            }
                        }
                        else
                        {
                            #region Add Filter Column Not in report
                            if (!FilterSetList.Any(x => x.ColumnName == strColumn))
                            {
                                if (reportColumn != null)
                                {
                                    if (!string.IsNullOrEmpty(reportColumn.ParentColumns))
                                    {
                                        AddOtherColumnToFilterTable(dcCollection, reportColumnList, reportColumn.ParentColumns, false);
                                        #region Add Filter Set
                                        newFilter = new FilterSets
                                        {
                                            ColumnName = strColumn,
                                            DataType = "System.String",
                                            TableName = string.Empty,
                                            OrAnd = "And",
                                            Operators = "=",
                                            IsParameter = false,
                                            HasParent = true,
                                            ExtraFilterColumn = true
                                        };

                                        if (reportColumn != null)
                                        {
                                            newFilter.DefaultValue = reportColumn.DefaultValue ?? string.Empty;
                                            newFilter.ParentColumn = reportColumn.ParentColumns ?? string.Empty;
                                            newFilter.DAOClass = reportColumn.DaoClass ?? string.Empty;
                                            newFilter.MethodName = reportColumn.MethodName ?? string.Empty;
                                            newFilter.ValueColumnId = reportColumn.ValueColumnId ?? string.Empty;
                                            newFilter.DisplayColumnId = reportColumn.DisplayColumnId ?? string.Empty;
                                            newFilter.IsMultipleSelection = reportColumn.IsMultipleSelection;
                                            newFilter.ShowAdditionalColumn = reportColumn.ShowAdditionalColumn;
                                            newFilter.AdditionalColumnId = reportColumn.AdditionalColumnId ?? string.Empty;
                                            newFilter.AdditionalColumnHeader = reportColumn.AdditionalColumnHeader ?? string.Empty;
                                            newFilter.ColumnWidth = reportColumn.ColumnWidth ?? string.Empty;
                                        }
                                        
                                        FilterSetList.Add(newFilter);
                                        #endregion
                                    }
                                    else
                                    {
                                        #region Add Filter Set
                                        newFilter = new FilterSets
                                        {
                                            ColumnName = strColumn,
                                            DataType = "System.String",
                                            TableName = string.Empty,
                                            OrAnd = "And",
                                            Operators = "=",
                                            IsParameter = false,
                                            HasParent = HasParent,
                                            ExtraFilterColumn = true
                                        };

                                        if (reportColumn != null)
                                        {
                                            newFilter.DefaultValue = reportColumn.DefaultValue ?? string.Empty;
                                            newFilter.ParentColumn = reportColumn.ParentColumns ?? string.Empty;
                                            newFilter.DAOClass = reportColumn.DaoClass ?? string.Empty;
                                            newFilter.MethodName = reportColumn.MethodName ?? string.Empty;
                                            newFilter.ValueColumnId = reportColumn.ValueColumnId ?? string.Empty;
                                            newFilter.DisplayColumnId = reportColumn.DisplayColumnId ?? string.Empty;
                                            newFilter.IsMultipleSelection = reportColumn.IsMultipleSelection;
                                            newFilter.ShowAdditionalColumn = reportColumn.ShowAdditionalColumn;
                                            newFilter.AdditionalColumnId = reportColumn.AdditionalColumnId ?? string.Empty;
                                            newFilter.AdditionalColumnHeader = reportColumn.AdditionalColumnHeader ?? string.Empty;
                                            newFilter.ColumnWidth = reportColumn.ColumnWidth ?? string.Empty;
                                        }

                                        FilterSetList.Add(newFilter);
                                        #endregion
                                    }
                                }
                            }
                            #endregion
                        }
                    }
                }
                #endregion

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Load(string reportPath)
        {
            ReportPath = reportPath;
            LoadReportBodyTables();
            LoadReportDataSet();
            LoadReportParameters();
            LoadSubReportInfo();
        }

        private XmlNodeList GetCustomNodeList(string rdlcTagName)
        {
            try
            {
                if (xmlDocument == null)
                {
                    xmlDocument = new XmlDocument();
                    try
                    {
                        xmlDocument.Load(ReportPath);
                    }
                    catch { }
                }

                XmlNodeList nodList = xmlDocument.GetElementsByTagName(rdlcTagName);
                return nodList;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private XmlNodeList GetCustomNodeList(object rdlcSource)
        {
            try
            {
                if (xmlDocument == null)
                {
                    xmlDocument = new XmlDocument();
                    try
                    {
                        xmlDocument.Load(this.ReportPath);
                    }
                    catch { }
                }

                XmlNamespaceManager nsManager = new XmlNamespaceManager(xmlDocument.NameTable);
                nsManager.AddNamespace("ns", "http://schemas.microsoft.com/sqlserver/reporting/2008/01/reportdefinition");
                XmlNodeList nodList = xmlDocument.SelectNodes(rdlcSource.ToString(), nsManager);
                return nodList;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private void LoadReportBodyTables()
        {
            XmlNodeList xmlItems = GetCustomNodeList("DataSetName");
            foreach (XmlNode nod in xmlItems)
            {
                if (!BodyTablesName.Contains(nod.InnerText))
                    BodyTablesName.Add(nod.InnerText);
            }
        }

        private void LoadSubReportInfo()
        {
            RDLReportDocument subRep;
            string reportName = string.Empty;
            XmlNodeList xmlItems = GetCustomNodeList("Subreport");
            foreach (XmlNode nod in xmlItems)
            {
                reportName = nod["ReportName"].InnerText;
                if (!SubReportList.Any(x => x.Name == reportName))
                {
                    subRep = new RDLReportDocument(_reportSuiteRepository);
                    subRep.SetFields(UserCode, reportName, ReportPathWithOutName);
                    subRep.Load(string.Format(@"{0}\{1}.{2}", ReportPathWithOutName, reportName, "rdlc"));
                    SubReportList.Add(subRep);
                }
            }

        }

        private void LoadReportDataSet()
        {
            try
            {
                DsReport.Tables.Clear();
                Fields.Clear();
                XmlNodeList xmlDataSet = GetCustomNodeList("DataSet");
                XmlNodeList ndSearchList = null;
                RDLTable rtTable = null;
                RDLParameter sqlParam = null;
                RDLColumn ReportColumn = null;

                foreach (XmlNode xmlTable in xmlDataSet)
                {
                    rtTable = new RDLTable();
                    rtTable.TableName = xmlTable.Attributes["Name"].InnerText;
                    try
                    {
                        rtTable.CommandType = (CommandType)ExtensionMethods.GetEnumValue(typeof(CommandType), xmlTable["Query"]["CommandType"].InnerText);
                    }
                    catch { rtTable.CommandType = CommandType.Text; }
                    //
                    rtTable.CommandText = xmlTable["Query"]["CommandText"].InnerText;
                    //For Parameters
                    try
                    {
                        ndSearchList = xmlTable["Query"]["QueryParameters"].ChildNodes;
                    }
                    catch { ndSearchList = null; }
                    if (ndSearchList.IsNotNull())
                    {
                        foreach (XmlNode xmlParam in ndSearchList)
                        {
                            sqlParam = new RDLParameter();
                            sqlParam.Name = xmlParam.Attributes["Name"].InnerText;
                            sqlParam.Prompt = GetSQLParamField(xmlParam["Value"].InnerText).ToString();
                            rtTable.Parameters.Add(sqlParam);
                        }
                    }
                    //
                    //For Fields
                    try
                    {
                        ndSearchList = xmlTable["Fields"].ChildNodes;
                    }
                    catch { ndSearchList = null; }

                    if (ndSearchList.IsNotNull())
                    {
                        foreach (XmlNode xmlColumn in ndSearchList)
                        {
                            ReportColumn = new RDLColumn();
                            ReportColumn.TableName = rtTable.TableName;
                            ReportColumn.Name = xmlColumn.Attributes["Name"].Value;
                            try
                            {
                                ReportColumn.DataType = xmlColumn["rd:TypeName"].InnerText;
                            }
                            catch
                            {
                                ReportColumn.DataType = "System.Calculate";
                            }
                            //
                            rtTable.Columns.Add(ReportColumn);
                            if (BodyTablesName.Contains(rtTable.TableName))
                                Fields.Add(ReportColumn);
                        }
                    }

                    DsReport.Tables.Add(rtTable);
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private void LoadReportParameters()
        {
            try
            {
                Parameters.Clear();
                XmlNodeList xmlParameters = GetCustomNodeList("ReportParameter");
                RDLParameter rpParam = null;

                foreach (XmlNode xmlParam in xmlParameters)
                {
                    rpParam = new RDLParameter
                    {
                        Name = xmlParam.Attributes["Name"].InnerText,
                        DbType = (DbType)ExtensionMethods.GetEnumValue(typeof(DbType), xmlParam["DataType"].InnerText),
                        Prompt = xmlParam["Prompt"].InnerText,
                        Value = xmlParam["Prompt"].InnerText
                    };

                    try
                    {
                        rpParam.DefaultValue = xmlParam["DefaultValue"]?.InnerText;
                    }
                    catch
                    {
                        if (rpParam.DbType == DbType.Boolean)
                        {
                            rpParam.DefaultValue = false;
                        }
                    }
                    Parameters.Add(rpParam);
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private void SetSQLParametersValue()
        {
            try
            {
                var searchList = FilterSetList.FindAll(x => x.IsParameter && string.IsNullOrEmpty(x.ColumnValue) && string.IsNullOrEmpty(x.DefaultValue));
                 
                if (searchList.Any())
                    throw new Exception("Parameter can not be blank. ");

                foreach (RDLTable rt in DsReport.Tables)
                {
                    foreach (RDLParameter param in rt.Parameters)
                    {
                        searchList = FilterSetList.FindAll(x => x.IsParameter && x.ColumnName == param.Prompt);
                        if (searchList.Count == 1)
                            SetValue(param, searchList[0]);
                        else
                            param.Value = string.Empty;
                    }
                }

                //Report Parameters
                foreach (RDLParameter param in Parameters)
                {
                    searchList = FilterSetList.FindAll(x => x.IsParameter && x.ColumnName == param.Name);
                    if (searchList.Count == 1)
                        SetValue(param, searchList[0]);
                    else
                        param.Value = string.Empty;
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }

        private void SetValue(RDLParameter param, FilterSets value)
        {
            try
            {
                string pvalue = value.ColumnValue.IsNullOrEmpty() ? value.DefaultValue : value.ColumnValue;
                param.DbType = (DbType)ExtensionMethods.GetEnumValue(typeof(DbType), value.DataType);
                switch (param.DbType)
                {
                    case DbType.DateTime:
                        DateTime.TryParse(pvalue, out DateTime dateTime);
                        param.Value = dateTime.ToString(ExtensionMethods.DateFormat);
                        break;
                    case DbType.Int16:
                        param.Value = Convert.ToInt16(pvalue);
                        break;
                    case DbType.Int32:
                        param.Value = Convert.ToInt32(pvalue);
                        break;
                    case DbType.Int64:
                        param.Value = Convert.ToInt64(pvalue);
                        break;
                    case DbType.Double:
                        param.Value = Convert.ToDouble(pvalue);
                        break;
                    case DbType.Decimal:
                        param.Value = Convert.ToDecimal(pvalue);
                        break;
                    case DbType.Boolean:
                        param.Value = Convert.ToBoolean(pvalue);
                        break;
                    default:
                        param.Value = pvalue;
                        break;
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public void SetFilterValue()
        {
            try
            {
                string filterString = string.Empty;
                foreach (string TableName in BodyTablesName)
                {
                    filterString = string.Empty;
                    var searchList = FilterSetList.FindAll(x => !x.IsParameter && x.TableName == TableName && x.ColumnValue.IsNotNullOrEmpty() && !x.ExtraFilterColumn);
                    foreach (FilterSets filter in searchList)
                    {
                        if (filter.Operators == "In"
                            || filter.Operators == "Not In")
                        {
                            filterString += filter.ColumnName + " " + filter.Operators + " (" + GetInOperatorString(filter.ColumnValue) + ") And ";
                        }
                        else
                        {
                            if (filter.ColumnName == "Expression")
                            {
                                filterString += filter.ColumnValue + " And ";
                            }
                            else
                            {
                                if (filter.DataType == "DateTime" || filter.DataType == "System.DateTime")
                                {
                                    CultureInfo provider = CultureInfo.InvariantCulture;
                                    DateTime datevalue = DateTime.ParseExact(filter.ColumnValue, ExtensionMethods.GridDateFormatArray, provider, DateTimeStyles.None);
                                    filterString += filter.ColumnName + " " + filter.Operators + " '" + datevalue.ToString(ExtensionMethods.DateFormat) + "' And ";
                                }
                                else if (filter.DataType == "Boolean" || filter.DataType == "System.Boolean")
                                {
                                    string filVal = filter.ColumnValue == "false" ? "0" : "1";
                                    filterString += filter.ColumnName + " " + filter.Operators + " " + filVal + " And ";
                                }
                                else
                                {
                                    filterString += filter.ColumnName + " " + filter.Operators + " '" + filter.ColumnValue + "' And ";
                                }
                            }
                        }
                    }

                    if (filterString.Length > 0)
                    {
                        filterString = filterString.Substring(0, filterString.Length - 4);
                        DsSource.Tables[TableName].DefaultView.RowFilter = filterString;
                    }
                }

            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }

        public void SetFilterValue(List<FilterSets> filterList)
        {
            try
            {
                string filterString = string.Empty;
                foreach (string TableName in BodyTablesName)
                {
                    filterString = string.Empty;
                    foreach (FilterSets filter in filterList)
                    {
                        if (filter.Operators == "In"
                            || filter.Operators == "Not In")
                        {
                            filterString += filter.ColumnName + " " + filter.Operators + " (" + GetInOperatorString(filter.ColumnValue) + ") And ";
                        }
                        else
                        {
                            if (filter.ColumnName == "Expression")
                            {
                                filterString += filter.ColumnValue + " And ";
                            }
                            else
                            {
                                if (filter.DataType == "DateTime" || filter.DataType == "System.DateTime")
                                {
                                    CultureInfo provider = CultureInfo.InvariantCulture;
                                    DateTime datevalue = DateTime.ParseExact(filter.ColumnValue, ExtensionMethods.GridDateFormatArray, provider, DateTimeStyles.None);
                                    filterString += filter.ColumnName + " " + filter.Operators + " '" + datevalue.ToString(ExtensionMethods.DateFormat) + "' And ";
                                }
                                else if (filter.DataType == "Boolean" || filter.DataType == "System.Boolean")
                                {
                                    string filVal = filter.ColumnValue == "false" ? "0" : "1";
                                    filterString += filter.ColumnName + " " + filter.Operators + " " + filVal + " And ";
                                }
                                else
                                {
                                    filterString += filter.ColumnName + " " + filter.Operators + " '" + filter.ColumnValue + "' And ";
                                }
                            }
                        }
                    }

                    if (filterString.Length.IsNotZero())
                    {
                        filterString = filterString.Substring(0, filterString.Length - 4);
                        DsSource.Tables[TableName].DefaultView.RowFilter = filterString;
                    }
                }

            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }

        private string GetInOperatorString(string Expression)
        {
            try
            {
                string strTemp = "";
                string[] strArray = Expression.Split(',', '|', '*', '!');
                foreach (string str in strArray)
                {
                    strTemp += "'" + str + "',";
                }
                //
                return strTemp;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private object GetSQLParamField(string Expression)
        {
            try
            {
                Object value = null;
                int index = Expression.IndexOf("!") + 1;
                int length = Expression.IndexOf(".");
                value = Expression.Substring(index, length - index);
                return value;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public void InitializeReportParameter()
        {
            try
            {
                foreach (RDLTable rtTable in DsReport.Tables)
                {
                    foreach (RDLParameter param in rtTable.Parameters)
                    {
                        param.Value = DBNull.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public void SetParameterValue(List<CustomeParameter> ParamList)
        {
            try
            {
                foreach (CustomeParameter cParam in ParamList)
                {
                    var searchList = FilterSetList.FindAll(x => x.ColumnName.ToUpper().Equals(cParam.ParameterName.ToUpper()));
                    if (searchList.Count.IsNotZero())
                    {
                        searchList[0].ColumnValue = cParam.ParameterValue;
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public void LoadSourceDataSet()
        {
            int i = 0;
          
            DsSource = new DataSet();
            IDbDataParameter[] sqlParam;

            //Load Parameter value            
            SetSQLParametersValue();
            //
            foreach (RDLTable rtTable in DsReport.Tables)
            {
                sqlParam = new IDbDataParameter[rtTable.Parameters.Count];
                i = 0;
                foreach (RDLParameter param in rtTable.Parameters)
                {
                    sqlParam[i] = new SqlParameter
                    {
                        ParameterName = param.Name,
                        DbType = param.DbType,
                        Value = param.Value
                    };
                    i++;
                }

                var dsTemp =  _reportSuiteRepository.LoadReportSourceDataSet(rtTable.CommandType, rtTable.CommandText, sqlParam);
                dsTemp.Tables[0].TableName = rtTable.TableName;
                DsSource.Tables.Add(dsTemp.Tables[rtTable.TableName].Copy());
            }
        }

        public void LoadSourceDataSet(IList<ReportParameter> parameters)
        {
            int i = 0;
            DataSet dsTemp = new DataSet();
            DsSource = new DataSet();
            IDbDataParameter[] sqlParam;

            foreach (RDLTable rtTable in DsReport.Tables)
            {
                sqlParam = new IDbDataParameter[rtTable.Parameters.Count];
                i = 0;
                foreach (RDLParameter param in rtTable.Parameters)
                {
                    sqlParam[i] = new SqlParameter();
                    sqlParam[i].ParameterName = param.Name;
                    sqlParam[i].DbType = param.DbType;

                    foreach (ReportParameter paramInfo in parameters)
                    {
                        if (paramInfo.Name == param.Prompt)
                        {
                            sqlParam[i].Value = paramInfo.Values[0];
                            param.Value = paramInfo.Values[0];
                            break;
                        }
                    }
                    i++;
                }

                dsTemp = _reportSuiteRepository.LoadReportSourceDataSet(rtTable.CommandType, rtTable.CommandText, sqlParam);
                dsTemp.Tables[0].TableName = rtTable.TableName;
                DsSource.Tables.Add(dsTemp.Tables[rtTable.TableName].Copy());
            }
        }

        public void LoadSubReportSourceDataSet(ReportParameterInfoCollection reportParameters)
        {
            int i = 0;
            DataSet dsTemp = new DataSet();
            IDbDataParameter[] sqlParam;
            Boolean needToLoad = false;
            foreach (RDLTable rtTable in DsReport.Tables)
            {
                sqlParam = new IDbDataParameter[rtTable.Parameters.Count];
                i = 0;
                foreach (RDLParameter param in rtTable.Parameters)
                {
                    sqlParam[i] = new SqlParameter();
                    sqlParam[i].ParameterName = param.Name;
                    sqlParam[i].DbType = param.DbType;

                    foreach (ReportParameterInfo paramInfo in reportParameters)
                    {
                        if (paramInfo.Name == param.Prompt)
                        {
                            sqlParam[i].Value = paramInfo.Values[0];
                            if (param.Value.NotEquals(paramInfo.Values[0]))
                                needToLoad = true;
                            param.Value = paramInfo.Values[0];
                            break;
                        }
                    }
                    i++;
                }

                if (needToLoad)
                {
                    dsTemp = _reportSuiteRepository.LoadReportSourceDataSet(rtTable.CommandType, rtTable.CommandText, sqlParam);
                    dsTemp.Tables[0].TableName = rtTable.TableName;
                    if (DsSource.Tables.Contains(rtTable.TableName))
                        DsSource.Tables.Remove(rtTable.TableName);
                    DsSource.Tables.Add(dsTemp.Tables[rtTable.TableName].Copy());
                }
            }
        }

        public TextReader GetCustomTextReader(string reportPath)
        {
            try
            {
                XmlDocument xmlReportDocument = null;
                TextReader reader = null;
                XmlNamespaceManager nsManager = null;
                XmlNodeList nodList = null;
                //
                if (xmlReportDocument.IsNull())
                {
                    xmlReportDocument = new XmlDocument();
                    try
                    {
                        xmlReportDocument.Load(reportPath);
                    }
                    catch { }
                }

                //Remove Filter Option from Report.
                nsManager = new XmlNamespaceManager(xmlReportDocument.NameTable);
                nsManager.AddNamespace("ns", "http://schemas.microsoft.com/sqlserver/reporting/2008/01/reportdefinition");
                nodList = xmlReportDocument.SelectNodes("//ns:Report/ns:DataSets/ns:DataSet/ns:Filters", nsManager);
                //
                foreach (XmlNode RemoveNode in nodList)
                    RemoveNode.ParentNode.RemoveChild(RemoveNode);

                reader = new System.IO.StringReader(xmlReportDocument.OuterXml);
                return reader;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
    }
}
