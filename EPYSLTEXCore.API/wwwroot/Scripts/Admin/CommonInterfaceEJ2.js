(function () {
    'use strict'

    // #region variable declarations
    var constraints = [];
    var childConstraints = [];
    var $formEl, $tblChildEl, tblChildId, tblFinderId, $tblFinderEl;
    var masterData;
    var childForm;
    var childObject = {};
    var interfaceConfigs;
    var menuId;
    var filterBy = {};
    var finderApiUrl = "";
    var childGridApiUrl = "";
    var selectedChild = null;

    var selectColumnList = [];
    var childSelectColumnList = [];
    var typeElements = [];

    var finderTableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    };
    // #endregion

    $(function () {
        menuId = localStorage.getItem("current_common_interface_menuid");

        $formEl = $("#form-ci-" + menuId);
        childForm = $("#form-ci-child-" + menuId);
        tblChildId = "#tabaleGridData-" + menuId;
        tblFinderId = "#tblSearchData-" + menuId;
        getInterfaceChilds();

        $("#btnSaveMaster-" + menuId).click(saveMaster);

        $("#btnSaveChild-" + menuId).click(function (e) {
            e.preventDefault();
            saveChild();
        });

        $("#btnNewChildItem-" + menuId).click(function (e) {

            e.preventDefault();

            if (!validateMasterForm()) return;
          
            var cObj = _.clone(childObject);
            cObj.Id = getMaxIdForArray(masterData.Childs, "Id");
            //cObj.EntityState = 4;
            masterData.Childs.unshift(_.clone(cObj));
            $tblChildEl.bootstrapTable('load', masterData.Childs);
        });
        $("#btnResetMaster-" + menuId).click(resetForm);
    });
    function resetForm() {
        $formEl.trigger("reset");
        $.each($formEl.find('select'), function (i, el) {
            $(el).select2('');
        });
        initChildGrid([]);
    }

    // #region Genereting markup
    function getInterfaceChilds() {
        
        var url = '/api/common-interface/configs?menuId=' + menuId;
        axios.get(url)
            .then(function (response) {
   
                interfaceConfigs = response.data;
                $("#title-form-ci-" + menuId).text(interfaceConfigs.InterfaceName);
                $("#finderTitle-" + menuId).text(interfaceConfigs.InterfaceName + " List")

                generateElements();
                initControls();

                if (interfaceConfigs.HasGrid) {
                   
                    if (interfaceConfigs.ChildGrids.length == 0) {
                        toastr.error("No child grid found.");
                        return;
                    }
                    $("#title-child-grid-" + menuId).text(interfaceConfigs.ChildGrids[0].ChildGridName);
                    $("#boxChildGrid-" + menuId).show();
                    initChildGrid([]);
                }
            })
            .catch(showResponseError);
    }
    async function initChildGrid(data) {

        if (interfaceConfigs.HasGrid) {
        
            if (interfaceConfigs.ApiUrl.length == 0) {
                toastr.error("Api URL is missing.");
                return;
            }
            var allSelectListObj = await executeSelectApis();
            if ($tblChildEl) $tblChildEl.destroy();
            var columns = [];
            var childGrid = interfaceConfigs.ChildGrids[0];
            var columnNames = childGrid.ColumnNames.split(','),
                columnHeaders = childGrid.ColumnHeaders.split(','),
                columnAligns = childGrid.ColumnAligns.split(','),
                columnWidths = childGrid.ColumnWidths.split(','),
                hiddenColumns = childGrid.HiddenColumns.split(','),
                columnFilters = childGrid.ColumnFilters.split(','),
                columnTypes = childGrid.ColumnTypes.split(','),
                columnSortings = childGrid.ColumnSortings.split(',');
 
            for (var iColumnName = 0; iColumnName < columnNames.length; iColumnName++) {
                var columnName = $.trim(columnNames[iColumnName]);
                var columnObj = {
                    field: columnName,
                    headerText: $.trim(columnHeaders[iColumnName]),
                    width: columnWidths[iColumnName],
                    textAlign: columnAligns[iColumnName],
                    allowFiltering: convertToBoolean(columnFilters[iColumnName]),
                    visible: $.inArray(columnName, hiddenColumns) !== -1 ? false : true
                };
          
                if (columnName == $.trim(childGrid.PrimaryKeyColumn)) columnObj.isPrimaryKey = true;
                columnObj = setAdditionalProps(columnObj, columnTypes[iColumnName], columnName, allSelectListObj);
                columns.push(columnObj);
            }
            var commandsField = getCommandsFields();
            if (commandsField) columns.unshift(commandsField);
            ej.base.enableRipple(true);
             
            $tblChildEl = new ej.grids.Grid({
                dataSource: data,
                columns: columns,
                allowResizing: true,
                allowFiltering: true,

                editSettings: getGridEditSettings(),
                recordClick: function (args) {

                },
                actionBegin: function (args) {
                    if (args.requestType === "save") {
                      
                        var selectColumns = interfaceConfigs.ChildGridColumns.filter(x => x.EntryType == "select");
                        selectColumns.map(x => {
                            args.data[x.ValueColumnName] = args.rowData[x.ValueColumnName];
                            args.data[x.DisplayColumnName] = args.rowData[x.DisplayColumnName];
                        });
                    } else if (args.requestType === "add") {
                      
                      var  ParentColumn = interfaceConfigs.ChildGrids[0]['ParentColumn']
                      var   PrimaryKeyColumn = interfaceConfigs.ChildGrids[0]['PrimaryKeyColumn'] 
                       
                        var sysColName = getSysColumn();
       

                        args.data[ParentColumn] = $formEl.find("#" + sysColName + "").val();
                       /* for (var key in args.rowData) {
                            var wd = key.slice(-2);
                            if (wd.toLowerCase() == "id") {
                                args.rowData[key] = 0;
                                args.data[key] = 0;

                            }
                        }*/
                    }
                },
            });
            if (interfaceConfigs.IsInsertAllow) $tblChildEl.toolbar = ['Add'];
            //loadGrid();

            $tblChildEl.refreshColumns;
            $tblChildEl.appendTo(tblChildId);
        }
    }
    async function executeSelectApis() {
        var objList = [],
            dependentColumnList = [],
            apiUrls = [];
        
        interfaceConfigs.ChildGridColumns.filter(x => x.EntryType == "select" && x.ApiUrl.length > 0).map(x => {
            var dIndex = interfaceConfigs.ChildGridColumns.findIndex(d => d.DependentColumnName == x.ColumnName);
            if (!dependentColumnList.includes(x.ColumnName) || dIndex > -1) {
                var obj = {
                    ColumnName: x.ColumnName,
                    ApiUrl: x.ApiUrl,
                    ValueColumnName: x.ValueColumnName,
                    DisplayColumnName: x.DisplayColumnName,
                    Label: x.Label,
                    DependentColumnName: x.DependentColumnName,
                    ChildGridColumnID: x.ChildGridColumnID,
                    IsEnabled: x.IsEnabled
                };
               
                apiUrls.push(obj);
                if (x.HasDependentColumn) dependentColumnList.push(x.DependentColumnName);
            }
        });

        await Promise.all(
            apiUrls.map(x =>
                axios.get(x.ApiUrl).then(function (res) {
                
                    var obj = {
                        ColumnName: x.ColumnName,
                        List: res.data,                         
                        Label: x.Label,
                        DependentColumnName: x.DependentColumnName,
                        ChildGridColumnID: x.ChildGridColumnID,
                        IsEnabled: x.IsEnabled
                    };
                    objList.push(obj);
                })
            ));
        return objList;
    }
    function generateParams(columnName, dataObj) {
        var paramArray = [];
        var obj = interfaceConfigs.Childs.find(x => x.ColumnName = columnName);
        if (obj) {
            if (obj.SelectApiUrl) {
                var splitUrl = obj.SelectApiUrl.split('/');
                splitUrl = splitUrl.filter(element => element.includes("{"));
                splitUrl.map(x => {
                    columnName = x.replace("{", "").replace("}", "");
                    paramArray.push({
                        ColumnName: columnName,
                        Value: dataObj[columnName]
                    });
                });
            }
        }
        return paramArray;
    }
    function loadGrid(paramArray) {
        var selectApiUrl = getSelectApiUrl(paramArray);
        axios.get(selectApiUrl) //interfaceConfigs.ApiUrl
            .then(function (response) {
                $tblChildEl.dataSource = response.data.Items ? response.data.Items : response.data;
            })
            .catch(function (err) {
                toastr.error(err);
            });
    }
    function getSelectApiUrl(paramArray) {
        var selectApiUrl = interfaceConfigs.Childs.find(x => x.ColumnName = paramArray[0].ColumnName).SelectApiUrl;
        paramArray.map(x => {
            selectApiUrl = selectApiUrl.replace("{" + x.ColumnName + "}", x.Value);
        });
        return selectApiUrl;
    }
    function setAdditionalProps(columnObj, columnType, columnName, allSelectListObj) {
        switch (columnType) {
            case "date":
                columnObj.type = 'date';
                columnObj.format = 'dd/MM/yyyy';
                break;
            case "checkbox":
                columnObj.editType = "booleanedit";
                columnObj.displayAsCheckBox = true;
                break;
            case "select":
             
                var gridColumnObj = interfaceConfigs.ChildGridColumns.find(x => x.ColumnName == columnName),
                    columnObjWithList = allSelectListObj.find(x => x.ColumnName == columnName);
                if (gridColumnObj) {
                    var dIndex = interfaceConfigs.ChildGridColumns.findIndex(x => x.DependentColumnName == columnName);
                    /*
                     if (!gridColumnObj.HasDependentColumn && dIndex < 0) {
                        var objTemp = interfaceConfigs.ChildGridColumns.find(x => x.ColumnName == columnName);
                        columnObj.valueAccessor = ej2GridDisplayFormatter;
                        columnObj.dataSource = allSelectListObj.length > 0 && columnObjWithList ? allSelectListObj.find(x => x.ColumnName == columnName).List : [];
                        columnObj.displayField = objTemp.DisplayColumnName;
                        //columnObj.allowEditing = false;
                        columnObj.required = gridColumnObj.IsRequired;
                        columnObj.edit = ej2GridDropDownObj({
                        });
                        
                    }
                    */
                    var tIndex = typeElements.findIndex(x => x.ChildGridColumnID === columnObjWithList.ChildGridColumnID);
                    if (tIndex < 0) {
                        typeElements.push({
                            ChildGridColumnID: columnObjWithList.ChildGridColumnID,
                            ColumnName: columnName,
                            TypeElem: document.createElement('input'),
                            TypeObj: null,
                            IsEnabled: columnObjWithList.IsEnabled
                        });
                    }
                    
                    columnObj.valueAccessor = ej2GridDisplayFormatter;
                    columnObj.dataSource = columnObjWithList.List;
                    columnObj.displayField= columnObjWithList.List.length>0  && Object.keys(columnObjWithList.List[0]).length>2 ? Object.keys(columnObjWithList.List[0])[1]:'text';
                    columnObj.field = columnObjWithList.ColumnName;
                    columnObj.edit= ej2GridDropDownObj({
                    });
                    //columnObj.edit = {
                    //    create: function () {
                    //        typeElements.find(x => x.ChildGridColumnID == columnObjWithList.ChildGridColumnID && x.ColumnName == columnName).TypeElem = document.createElement('input');
                    //        return typeElements.find(x => x.ChildGridColumnID == columnObjWithList.ChildGridColumnID && x.ColumnName == columnName).TypeElem;
                    //    },
                    //    read: function () {
                    //        return typeElements.find(x => x.ChildGridColumnID == columnObjWithList.ChildGridColumnID && x.ColumnName == columnName).TypeObj.text;
                    //    },
                    //    destroy: function () {
                    //        typeElements.find(x => x.ChildGridColumnID == columnObjWithList.ChildGridColumnID && x.ColumnName == columnName).TypeObj.destroy();
                    //    },
                    //    write: function (e) {
                    //        var objTemp = interfaceConfigs.ChildGridColumns.find(x => x.ColumnName == columnName);
                    //        typeElements.find(x => x.ChildGridColumnID == columnObjWithList.ChildGridColumnID && x.ColumnName == columnName).TypeObj = new ej.dropdowns.DropDownList({
                    //            dataSource: allSelectListObj.length > 0 && columnObjWithList ? allSelectListObj.find(x => x.ColumnName == columnName).List : [],
                    //            fields: { value: 'id', text: 'text' },
                    //            //fields: { value: columnName, text: objTemp.DisplayColumnName },
                    //            placeholder: 'Select ' + columnObjWithList.Label,
                    //            floatLabelType: 'Never',
                    //            change: function (f) {

                    //                e.rowData[columnName] = f.itemData.id;
                    //                e.rowData[gridColumnObj.ValueColumnName] = f.itemData.id;
                    //                e.rowData[gridColumnObj.DisplayColumnName] = f.itemData.text;

                    //                var hasDependency = interfaceConfigs.ChildGridColumns.find(x => x.ColumnName == columnName).HasDependentColumn;
                    //                if (hasDependency) {
                    //                    var dependentColumnId = interfaceConfigs.ChildGridColumns.find(x => x.ColumnName == columnObjWithList.DependentColumnName).ChildGridColumnID;
                    //                    var obj = typeElements.find(x => x.ChildGridColumnID == dependentColumnId && x.ColumnName == columnObjWithList.DependentColumnName);
                    //                    if (obj) {
                    //                        obj = obj.TypeObj;
                    //                        obj.enabled = true;
                    //                        obj.dataSource = allSelectListObj.find(x => x.ColumnName == columnObjWithList.DependentColumnName).List.filter(x => x.id == f.itemData.id);
                    //                        obj.dataBind();
                    //                    }
                    //                }
                    //            }
                    //        });
                    //        typeElements.find(x =>
                    //            x.ChildGridColumnID == columnObjWithList.ChildGridColumnID && x.ColumnName == columnName)
                    //            .TypeObj
                    //            .appendTo(typeElements.find(x =>
                    //                x.ChildGridColumnID == columnObjWithList.ChildGridColumnID && x.ColumnName == columnName)
                    //                .TypeElem);
                    //    }
                    //}
                }
                break;
            default:
            // code block
        }
        return columnObj;
    }
    function getCommandsFields() {
        if (interfaceConfigs.IsUpdateAllow || interfaceConfigs.IsDeleteAllow) {
            var obj = {
                field: "Commands",
                headerText: '',
                width: 100,
                textAlign: 'Center',
                commands: []
            }
            if (interfaceConfigs.IsUpdateAllow) {
                obj.commands = [
                    {
                        type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' }
                    },
                    {
                        type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' }
                    },
                    {
                        type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' }
                    }];
            }
            if (interfaceConfigs.IsDeleteAllow) {
                obj.commands.push({
                    type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' }
                });
            }
            return obj;
        }
        return null;
    }
    function getGridEditSettings() {
        var obj = {
            allowAdding: interfaceConfigs.IsInsertAllow,
            allowEditing: interfaceConfigs.IsUpdateAllow,
            allowDeleting: interfaceConfigs.IsDeleteAllow,
            showDeleteConfirmDialog: interfaceConfigs.IsDeleteAllow,
            mode: "Normal"
        };
        return obj;
    }
    function generateElements() {
        var template = "";

        //var items = [];
        //items.push(interfaceConfigs.Childs[0]);
        //items.push(interfaceConfigs.Childs[1]);
        //interfaceConfigs.Childs = items;

        var totalColumn = interfaceConfigs.MasterColNum,
            totalRow = setColWiseRowValue(interfaceConfigs.MasterColNum, interfaceConfigs.MasterRowNum);

        template += setColStartDiv(totalColumn);
 
        var rowCount = 0,
            colCount = 1;

        $.each(interfaceConfigs.Childs, function (i, value) {
            var cssHidden = value.IsHidden ? "display:none;" : "",
                cssEnable = !value.IsEnable ? "disabled" : "";
             
            rowCount++;
            switch (value.EntryType) {
                case "text":
                    if (value.IsSys) {
                        template +=
                            `<div class="form-group" style='${cssHidden}'>
                                <label class="col-sm-2 control-label ci">${value.Label}</label>
                                <div class="col-sm-10">
                                    <div class="input-group input-group-sm" style='width: 100%;'>
                                        <input type="text" class="form-control" id="${value.ColumnName}" name="${value.ColumnName}" readonly />                                        
                                          ${adNew(interfaceConfigs.IsInsertAllow, menuId, value.ChildID)}                  
                                       
                                        ${setFinder(value.HasFinder, menuId, value.ChildID)}
                                    </div>
                                </div>
                            </div>`;
                    }
                    else if (!value.IsSys && value.IsHidden && value.IsEnabled) {
                        template += '<input type="hidden" id="' + value.ColumnName + '" name="' + value.ColumnName + '" value="" />'
                    }
                    else {
                        template +=
                            `<div class="form-group" style='${cssHidden}'>
                                <label class="col-sm-2 control-label ci">${value.Label}</label>
                                <div class="col-sm-10">
                                    <div class="input-group input-group-sm" style='width: 100%;'>
                                        <input type="text" class="form-control" id="${value.ColumnName}" name="${value.ColumnName}" ${cssEnable} />
                                          
                                        ${setFinder(value.HasFinder, menuId, value.ChildID)}
                                    </div>
                                </div>
                            </div>`;
                    }
                    break;
                case "datetime":
                case "date":
                    template +=
                        `<div class="form-group" style='${cssHidden}'>
                            <label class="col-sm-2 control-label ci">${value.Label}</label>
                            <div class="col-sm-10">
                                <input type="text" class="form-control input-sm bootstrap-datepicker" id="${value.ColumnName}" name="${value.ColumnName}" placeholder="${value.Label}" ${cssEnable} />
                            </div>
                        </div>`;
                    break;
                case "select":
                    template +=
                        `<div class="form-group" style='${cssHidden}'>
                                <label class="col-sm-2 control-label ci">${value.Label}</label>
                                <div class="col-sm-10">
                                    <div class="input-group input-group-sm" style='width: 100%;'>
                                        <select class="form-control" id="${value.ColumnName}" name="${value.ColumnName}" style="width: 100%;" ${cssEnable}></select>
                                          
                                        ${setFinder(value.HasFinder, menuId, value.ChildID)}
                                    </div>
                                </div>
                            </div>`;
                      
                    var selectColumn = {};
                    selectColumn.id = value.ColumnName;
                    selectColumn.placeholder = value.Label;
                    selectColumn.apiUrl = value.SelectApiUrl;
                    selectColumn.hasDependentColumn = value.HasDependentColumn;
                    selectColumn.dependentColumnName = value.DependentColumnName;
                    selectColumn.defaultValue = value.DefaultValue;
                    selectColumnList.push(selectColumn);
                    break;
                case "number":
                    template +=
                        `<div class="form-group" style='` + cssHidden + `'>
                            <label class="col-sm-2 control-label ci">${value.Label}</label>
                            <div class="col-sm-10">
                                <div class="input-group input-group-sm" style='width: 100%;'>
                                    <input type="number" class="form-control" id="${value.ColumnName}" name="${value.ColumnName}" ${cssEnable} />
                                      
                                    ${setFinder(value.HasFinder, menuId, value.ChildID)}
                                </div>
                            </div>
                        </div>`;
                    break;
                case "checkbox":
                    template +=
                        `<div class="form-group" style='` + cssHidden + `'>
                            <div class="col-sm-offset-2 col-sm-10">
                                <div class="checkbox checkbox-success">
                                    <input type="checkbox" id="${value.ColumnName}" name="${value.ColumnName}" ${cssEnable} />
                                    <label for="${value.ColumnName}" style='margin-top:4px;'>${value.Label}</label>
                                </div>
                            </div>
                        </div>`;
                    break;
                default:
                    break;
            }

            if (value.IsRequired) {
                constraints[value.ColumnName] = value.MaxLength > 0 ? { presence: true, length: { maximum: value.MaxLength } } : { presence: true };
            }

            if (rowCount == totalRow) {
                //Set col-sm-x
                rowCount = 0;
                template += "</div>";
                if (colCount < totalColumn) {
                    template += setColStartDiv(totalColumn);
                }
                colCount++;
            }
        });
        $formEl.append(template);
        
        initAddNew();
        initFinder();
    }
    function getSysColumn() {
       
       var colName=  interfaceConfigs.Childs.find(x => x.IsSys === true).ColumnName;
        return colName;
    }

    function adNew(IsInsertAllow, menuId, childID) {
       
        if (IsInsertAllow) {
            return `<span class="input-group-btn">
                        <button type="button" class="btn btn-success ci-adnew-${menuId}-${childID}"><i class="fa fa-plus" aria-hidden="true"></i></button>
                    </span>`;
        }
        return "";
    }

    function setFinder(hasFinder, menuId, childID) {
        if (hasFinder) {
            return `<span class="input-group-btn">
                        <button type="button" class="btn btn-success ci-finder-${menuId}-${childID}"><i class="fa fa-search" aria-hidden="true"></i></button>
                    </span>`;
        }
        return "";
    }

    function initAddNew() {         

        interfaceConfigs.Childs.map(child => {
            $formEl.find("." + `ci-adnew-${menuId}-${child.ChildID}`).click(function () {
               
                newId();
            });

        });
                    
    }
    function initFinder() {
        interfaceConfigs.Childs.filter(x => x.HasFinder == true).map(child => {
            $formEl.find("." + `ci-finder-${menuId}-${child.ChildID}`).click(function () {
                selectedChild = child;
                openSingleSelectFinder();
            });
        });
    }
    function openSingleSelectFinder() {
        var finder = new commonFinder({
            title: "Select " + selectedChild.Label,
            pageId: "divCommonInterface-" + menuId,
            height: 220,
            modalSize: "modal-lg",
            apiEndPoint: selectedChild.FinderApiUrl,
            fields: selectedChild.FinderHeaderColumns,
            widths: selectedChild.FinderColumnWidths,
            headerTexts: selectedChild.FinderDisplayColumns,
            primaryKeyColumn: selectedChild.FinderValueColumn,
            //allowPaging: true,
            allowSorting: selectedChild.FinderColumnSortings,
            allowFiltering: selectedChild.FinderFilterColumns,
            autofitColumns: true,
            onSelect: function (res) {
                finder.hideModal();
                var data = res.rowData;
                $formEl.find("#" + selectedChild.ColumnName).val(data[selectedChild.ColumnName]);
                $formEl.find("#" + selectedChild.FinderHeaderColumns).val(data[selectedChild.FinderHeaderColumns]);
                $formEl.find("#" + selectedChild.FinderValueColumn).val(data[selectedChild.FinderValueColumn]);
                //loadGrid(generateParams(selectedChild.ColumnName, data));
                if (data.Childs) {
                    initChildGrid(data.Childs);
                    //$tblChildEl.bootstrapTable('load', data.Childs);
                }
            }
        });
        finder.showModal();
    }
    function initControls() {
        $formEl.find('.bootstrap-datepicker').datepicker({
            endDate: "0d",
            todayHighlight: true
        });

        $.each(selectColumnList, function (i, value) {
            var el = $formEl.find("#" + value.id);

            if (value.hasDependentColumn > 0) {
                $formEl.find("#" + value.dependentColumnName).on('select2:select', function (e) {
                    loadDependentSelection(el, value.apiUrl);
                });

                el.on('select2:select', function (e) {
                    setSelect2Combo($formEl.find("#" + dependentSegmentEl.Id), e.params.data.desc)
                });
            }
      
            $.ajax({
                type: 'GET',
                url: value.apiUrl,
                async: false,
                success: function (data) {
                    el.select2({
                        placeholder: "Select " + value.placeholder,
                        allowClear: true,
                        data: data
                    });
                    el.val(null).trigger('change');
                },
                error: function () { console.log("Error in loading selection options") }
            });
        });

        $("#ci-new-" + menuId).click(function (e) {
            e.preventDefault();
            newId();
        });

        $("#ci-finder-" + menuId).click(function (e) {
            e.preventDefault();
            showFinderData();
        });
    }

    /*
    function initChildGrid() {
        axios.get(interfaceConfigs.ApiUrl + "/0")
            .then(function (response) {
                masterData = response.data;
                if (masterData.Childs.length === 0) {
                    toastr.error("You must add an empty child item in your api.");
                    return;
                }
                childObject = masterData.Childs.shift();
                generateChildGrid();
            })
            .catch(showResponseError);
    }
    */

    function loadDependentSelection(el, apiUrl) {
        var url = apiUrl + "/" + id;
        axios.get(url)
            .then(function (response) {
                el.select2({
                    data: response.data
                });
            })
            .catch(showResponseError);
    }

    function generateChildGrid() {
        var childGridConfig = interfaceConfigs.ChildGrids[0];
        childGridApiUrl = childGridConfig.ApiUrl;
        var _columnNames = childGridConfig.ColumnNames.split(',');
        var _columnHeaders = childGridConfig.ColumnHeaders.split(',');
        var _columnAligns = childGridConfig.ColumnAligns.split(',');
        var _columnWidths = childGridConfig.ColumnWidths.split(',');
        var _hiddenColumns = childGridConfig.HiddenColumns.split(',');
        var _columnFilters = childGridConfig.ColumnFilters.split(',');
        var _columnTypes = childGridConfig.ColumnTypes.split(',');
        var _columnSortings = childGridConfig.ColumnSortings.split(',');

        var columnList = [];
        var actionColumn = {
            headerText: 'Commands', width: 60, commands: [
                { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-delete e-icons' } },
                { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-update e-icons' } },
                { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-cancel-icon e-icons' } }]
        }
        columnList.push(actionColumn);

        $.each(_columnNames, function (i, value) {
            var hidden = _hiddenColumns.find(function (element) {
                return element == value;
            });
            if (hidden) return true;

            var column = {
                field: value,
                headerText: _columnHeaders[i],
                align: _columnAligns[i],
                width: _columnWidths[i],
                sortable: convertToBoolean(_columnSortings[i])
            }

            if (convertToBoolean(_columnFilters[i])) {
                column["allowFiltering"] = true;
            }

            switch (_columnTypes[i]) {
                case "checkbox":
                    column["type"] = "boolean";
                    column["editType"] = "booleanedit";
                    break;
                case "select":
                    var optionListName = value.replace("Id", '');
                    optionListName = optionListName.replace("ID", '');
                    optionListName += "List";

                    //column["type"] = "boolean";
                    column["editType"] = "dropdownedit";
                    column["edit"] = {
                        params: {
                            query: new ej.data.Query(),
                            dataSource: masterData[optionListName],
                            fields: { value: 'id', text: 'text' },
                            allowFiltering: true
                        }
                    };
                    break;
                default:
                    break;
            }

            columnList.push(column);
        });

        if ($tblChildEl) $tblChildEl.destroy();
        $tblChildEl = new ej.grids.Grid({
            allowResizing: true,
            allowFiltering: true,
            allowPaging: true,
            pageSettings: { pageCount: 5 },
            toolbar: ['Add'],
            editSettings: { allowAdding: true, allowEditing: true, allowDeleting: true },
            columns: columnList
        });

        $tblChildEl.appendTo(tblChildId);
    }
    function showFinderData() {
        var finderData = interfaceConfigs.Childs.find(function (element) {
            return element.HasFinder == true;
        });
        finderApiUrl = finderData.FinderApiUrl;

        var _columnsFields = finderData.FinderHeaderColumns.split(',');
        var _columnTitles = finderData.FinderDisplayColumns.split(',');
        var _columnAligns = finderData.FinderColumnAligns.split(',');
        var _columnWidths = finderData.FinderColumnWidths.split(',');
        var _columnSortings = finderData.FinderColumnSortings.split(',');
        var _columnFilters = finderData.FinderFilterColumns.split(',');

        var columnList = [];
        $.each(_columnsFields, function (i, value) {
            var column = {
                field: value,
                headerText: _columnTitles[i],
                align: _columnAligns[i],
                width: _columnWidths[i],
                allowFiltering: convertToBoolean(_columnFilters[i]),
                allowSorting: convertToBoolean(_columnSortings[i])
            };
            columnList.push(column);
        });

        if ($tblFinderEl) $tblFinderEl.destroy();
        $tblFinderEl = new ej.grids.Grid({
            allowRefreshing: true,
            allowResizing: true,
            allowFiltering: true,
            allowPaging: true,
            allowSorting: true,
            pageSettings: { currentPage: 1, pageSize: 10, pageSizes: true },
            columns: columnList,
            recordDoubleClick: function (args) {
                var url = interfaceConfigs.ApiUrl + "/" + args.rowData.Id;
                axios.get(url)
                    .then(function (response) {
                        masterData = response.data;
                        setMasterData();
                        $("#modal-finder-" + menuId).modal('hide');
                    })
                    .catch(showResponseError);
            },
        });
        $tblFinderEl.appendTo(tblFinderId);

        $(".filter-control input").attr('placeholder', 'Type & Enter for Search');
        $(".filter-control input").css('border', '1px solid gray');
        $(".filter-control input").css('font-size', '13px');

        axios.get(finderData.FinderApiUrl)
            .then(function (response) {
                $tblFinderEl.dataSource = response.data.rows;
                $tblFinderEl.pageSettings.totalRecordsCount = response.data.total;
                //$tblFinderEl.refresh();
                $("#modal-finder-" + menuId).modal("show");
            })
            .catch(showResponseError);
    }

    function getFinderData() {
        var queryParams = $.param(finderTableParams);
        var url = finderApiUrl + "?" + queryParams;
        axios.get(url)
            .then(function (response) {
                $(tblFinderId).bootstrapTable('load', response.data);
            })
            .catch(function () {
                toastr.error(constants.LOAD_ERROR_MESSAGE);
            })
    }
    function resetChildForm() {
        childForm.trigger("reset");
       updateSysID(-1111);
       /// childForm.find("#EntityState").val(4);
    }
    function updateSysID(id)
    {
        var  sysColName = getSysColumn();
       
        $formEl.find("#" + sysColName +"").val(id);
    }
    function newId() {
    
        resetForm();
      updateSysID(-1111);
  
    }

    // #region Save
    function saveMaster(e) {
   
        e.preventDefault();
       // if (!validateMasterForm()) return;
        $formEl.find(':checkbox').each(function () {
            this.value = this.checked;
        });
        var data = formDataToJson($formEl.serializeArray());
         if (masterData && masterData.Childs) data["Childs"] = masterData.Childs;
         if ($tblChildEl  ) data["Childs"] = $tblChildEl.getCurrentViewRecords();
         
        
        var config = {
            headers: {
                'Content-Type': 'application/json'
            }
        };
  
        $formEl.find('input:disabled').each(function () {
            data[$(this).attr("id")] = $(this).val();
        });
       
      
        axios.post(interfaceConfigs.SaveApiUrl, data, config)
            .then(function (response) {  
             
               updateSysID(response.data)
                toastr.success(constants.SUCCESS_MESSAGE);
                //resetForm();
            })
            .catch(e);
    }

    function validateMasterForm() {
        initializeValidation($formEl, constraints);

        if (!isValidForm($formEl, constraints)) {
            toastr.error("Please correct all validation ")
            return false;
        }
        else {
            hideValidationErrors($formEl);
            return true;
        }
    }

    function saveChild() {
        if (!validateChildForm()) {
            return;
        }

        $('#form-ci-child-' + menuId).find(':checkbox').each(function () {
            this.value = this.checked;
        });

        var formData = childForm.serializeArray();
        var data = {};
        $.each(formData, function (i, v) {
            data[v.name] = v.value;
            data["Actions"] = "";
        });
          var sysColName = getSysColumn();
           
       /// childForm.find("#EntityState").val(4);
        data[interfaceConfigs.ChildGrids[0].ParentColumn] = $formEl.find("#" + sysColName + "").val();

        masterData.Childs.rows.push(data);
        masterData.Childs.total = masterData.Childs.rows.length;

        $tblChildEl.bootstrapTable('load', masterData.Childs);

        $("#modal-child-" + menuId).modal('hide');
    }

    function validateChildForm() {
        initializeValidation(childForm, childConstraints);

        if (!isValidForm(childForm, childConstraints)) {
            toastr.error("Please correct all validation ")
            return false;
        }
        else {
            hideValidationErrors(childForm);
            return true;
        }
    }
    // #endregion

    function setMasterData() {
        $.each(interfaceConfigs.Childs, function (i, value) {
            switch (value.EntryType) {
                case "checkbox":
                    $formEl.find('#' + value.ColumnName).prop('checked', masterData[value.ColumnName]);
                    break;
                case "radio":
                    $formEl.find('input[name="' + value.ColumnName + '"]').val([masterData[value.ColumnName]]);
                    break;
                case "select":
                    $formEl.find('#' + value.ColumnName).val(masterData[value.ColumnName]).trigger("change");
                    break;
                default:
                    $formEl.find('#' + value.ColumnName).val(masterData[value.ColumnName]);
                    break;
            }
        });

        if (interfaceConfigs.HasGrid) {
            masterData.Childs.rows = masterData.Childs;
            $tblChildEl.dataSource = masterData.Childs.rows;
            $tblChildEl.refresh();
        }
    }

    function setChildData(data) {
        $.each(interfaceConfigs.ChildGrids[0].Childs, function (i, value) {
            switch (value.EntryType) {
                case "checkbox":
                    childForm.find("input[name=" + value.ColumnName + "]").prop('checked', data[value.ColumnName]);
                    break;
                case "radio":
                    childForm.find("input[name=" + value.ColumnName + "]").val([data[value.ColumnName]]);
                    break;
                case "select":
                    childForm.find("input[name=" + value.ColumnName + "]").val(data[value.ColumnName]).trigger("change");
                    break;
                default:
                    childForm.find("input[name=" + value.ColumnName + "]").val(data[value.ColumnName]);
                    break;
            }
        });

        $("#modal-child-" + menuId).modal('show');
    }

    function generateChildElements() {
        var template = "";
        $.each(interfaceConfigs.ChildGrids[0].Childs, function (i, value) {
      
            switch (value.EntryType) {
               
                case "text":
                    if (value.IsSys) {
                        template += '<div class="form-group">'
                            + '<label class="col-sm-3 control-label">' + value.Label + '</label>'
                            + '<div class="col-sm-5"><div class="input-group input-group-sm">'
                            + '<input type="text" class="form-control" id="' + value.ColumnName + '" name="' + value.ColumnName + '" readonly>'
                            + '<span class="input-group-btn">'
                            + '<button type="button" class="btn btn-default" id="ci-new-' + menuId + '"><i class="fa fa-plus" aria-hidden="true"></i></button>'
                            + '<button type="button" class="btn btn-success" id="ci-finder-' + menuId + '"><i class="fa fa-search" aria-hidden="true"></i></button>'
                            + '</span></div></div></div>';
                    }
                    else if (value.IsHidden && value.IsEnabled) {
                        template += '<input type="hidden" id="' + value.ColumnName + '" name="' + value.ColumnName + '" value=""/>'
                    }
                    else if (value.IsHidden && !value.IsEnabled) {
                        template += '<div class="form-group">'
                            + '<label class="col-sm-3 control-label">' + value.Label + '</label>'
                            + '<div class="col-sm-5">'
                            + '<input type="text" class="form-control input-sm" id="' + value.ColumnName + '" name="' + value.ColumnName + '" placeholder="' + value.Label + '" readonly>'
                            + '</div></div>';
                    }
                    else {
                        template += '<div class="form-group">'
                            + '<label class="col-sm-3 control-label">' + value.Label + '</label>'
                            + '<div class="col-sm-5">'
                            + '<input type="text" class="form-control input-sm" id="' + value.ColumnName + '" name="' + value.ColumnName + '" placeholder="' + value.Label + '">'
                            + '</div></div>';
                    }
                    break;
                case "datetime":
                case "date":
                    template += '<div class="form-group">'
                        + '<label class="col-sm-3 control-label">' + value.Label + '</label>'
                        + '<div class="col-sm-5">'
                        + '<input type="text" class="form-control input-sm bootstrap-datepicker" id="' + value.ColumnName + '" name="' + value.ColumnName + '" placeholder="' + value.Label + '">'
                        + '</div></div>';
                    break;
                case "select":
                    template += '<div class="form-group">'
                        + '<label class="col-sm-3 control-label">' + value.Label + '</label>'
                        + '<div class="col-sm-5">'
                        + '<select class="form-control" id="' + value.ColumnName + '" name="' + value.ColumnName + '" style="width: 100%"></select>'
                        + '</div></div>';
                    var selectColumn = {};
                    selectColumn.id = value.ColumnName;
                    selectColumn.placeholder = value.Label;
                    selectColumn.apiUrl = value.SelectApiUrl;
                    selectColumn.hasDependentColumn = value.HasDependentColumn;
                    selectColumn.dependentColumnName = value.DependentColumnName;
                    selectColumn.defaultValue = value.DefaultValue;
                    childSelectColumnList.push(selectColumn);
                    break;
                case "number":
                    template += '<div class="form-group">'
                        + '<label class="col-sm-3 control-label">' + value.Label + '</label>'
                        + '<div class="col-sm-5">'
                        + '<input type="number" class="form-control input-sm" id="' + value.ColumnName + '" name="' + value.ColumnName + '" placeholder="' + value.Label + '">'
                        + '</div></div>';
                    break;
                case "checkbox":
                    template += '<div class="form-group"><div class="col-sm-offset-2 col-sm-10"><div class="checkbox checkbox-success">'
                        + '<input type="checkbox" id="c-' + value.ColumnName + '" name="' + value.ColumnName + '"> <label for="c-' + value.ColumnName + '">' + value.Label + '</label></div></div></div>';
                    break;
                default:
                    break;
            }

            if (value.IsRequired) {
                childConstraints[value.ColumnName] = value.MaxLength > 0 ? { presence: true, length: { maximum: value.MaxLength } } : { presence: true };
            }
        });

        childForm.append(template);
    }
    /*
    function initChildControls() {
        childForm.find('.bootstrap-datepicker').datepicker({
            endDate: "0d",
            todayHighlight: true
        });

        $.each(childSelectColumnList, function (i, value) {
            var el = childForm.find("#" + value.id);

            if (value.hasDependentColumn > 0) {
                childForm.find("#" + value.dependentColumnName).on('select2:select', function (e) {
                    loadDependentSelection(el, value.apiUrl);
                });

                el.on('select2:select', function (e) {
                    setSelect2Combo($("#" + dependentSegmentEl.Id), e.params.data.desc)
                });
            }

            $.ajax({
                type: 'GET',
                url: value.apiUrl,
                async: false,
                success: function (data) {
                    el.select2({
                        placeholder: "Select " + value.placeholder,
                        allowClear: true,
                        data: data
                    });
                    el.val(null).trigger('change');
                },
                error: function () { console.log("Error in loading selection options") }
            });
        });

        $("#ci-new-" + menuId).click(function (e) {
            e.preventDefault();
            newId();
        });

        $("#ci-finder-" + menuId).click(function (e) {
            e.preventDefault();
            showFinderData();
        });
    }
    */
    function setColStartDiv(colNum) {
        var divStr = '<div class="col-sm-12">';
        switch (colNum) {
            case 0:
            case 1:
                divStr = '<div class="col-sm-12">';
                break;
            case 2:
                divStr = '<div class="col-sm-6">';
                break;
            case 3:
                divStr = '<div class="col-sm-4">';
                break;
            case 4:
                divStr = '<div class="col-sm-3">';
                break;
            case 6:
                divStr = '<div class="col-sm-2">';
                break;
            case 12:
                divStr = '<div class="col-sm-1">';
                break;
            default:
                break;
        }
        return divStr;
    }
    function setColWiseRowValue(colNum, dbRowNum) {
        if (colNum == 2 && dbRowNum == 3) return 4;
        return dbRowNum;
    }
})();