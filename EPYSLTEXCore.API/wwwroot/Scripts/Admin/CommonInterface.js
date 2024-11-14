(function () {
    'use strict'

    // #region variable declarations
    var constraints = [];
    var childConstraints = [];
    var $formEl, $tblChildEl, tblChildId;
    var masterData;
    var childForm;
    var childObject = {};
    var interfaceConfigs;
    var menuId, pageName;
    var filterBy = {};
    var finderApiUrl = "";

    var selectColumnList = [];

    var finderTableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    };

    var masterPrimaryKey, childPrimaryKey, childGridApiUrl;
    // #endregion

    $(function () {
        if (!pageName)
            pageName = localStorage.getItem("pageName");

        menuId = localStorage.getItem("current_common_interface_menuid");

        var pageId = pageName + "-" + menuId;
        $formEl = $("#form-ci-" + menuId);
        childForm = $("#form-ci-child-" + menuId);
        //$tblChildEl = $("#tabaleGridData-" + menuId);
        tblChildId = pageConstants.CHILD_TBL_ID_PREFIX + pageId;
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
            cObj[childPrimaryKey] = getMaxIdForArray(masterData.Childs, childPrimaryKey);
            cObj.EntityState = 4;
            masterData.Childs.unshift(_.clone(cObj));
            // $tblChildEl.bootstrapTable('load', masterData.Childs);
        });
        $("#btnResetMaster-" + menuId).click(resetForm);
    });

    function resetForm() {
        $formEl.trigger("reset");
        $.each($formEl.find('select'), function (i, el) {
            $(el).select2('');
        });
    }

    // #region Genereting markup
    function getInterfaceChilds() {
        var url = '/api/common-interface/configs?menuId=' + menuId;
        axios.get(url)
            .then(function (response) {
                interfaceConfigs = response.data;
                $("#title-form-ci-" + menuId).text(interfaceConfigs.InterfaceName);
                $("#finderTitle-" + menuId).text(interfaceConfigs.InterfaceName + " List")

                masterPrimaryKey = interfaceConfigs.Childs.find(function (el) { return el.IsSys }).ColumnName;
                generateElements();
                initControls();

                if (interfaceConfigs.HasGrid) {
                    if (interfaceConfigs.ChildGrids.length == 0) {
                        toastr.error("No child grid found.");
                        return;
                    }
                    childPrimaryKey = interfaceConfigs.ChildGrids[0].PrimaryKeyColumn;
                    $("#title-child-grid-" + menuId).text(interfaceConfigs.ChildGrids[0].ChildGridName);
                    $("#boxChildGrid-" + menuId).show();
                    initChild();
                    //initChildGrid();
                }
            })
            .catch(showResponseError);
    }
    function initChild() {
        if ($tblChildEl) $tblChildEl.destroy();

        var columns = [];
        for (var iChildGrid = 0; iChildGrid < interfaceConfigs.ChildGrids.length; iChildGrid++) {
            var childGrid = interfaceConfigs.ChildGrids[iChildGrid];
            var columnNames = childGrid.ColumnNames.split(','),
                columnHeaders = childGrid.ColumnHeaders.split(','),
                columnAligns = childGrid.ColumnAligns.split(','),
                columnWidths = childGrid.ColumnWidths.split(','),
                hiddenColumns = childGrid.HiddenColumns.split(','),
                columnFilters = childGrid.ColumnFilters.split(','),
                columnTypes = childGrid.ColumnTypes.split(','),
                columnSortings = childGrid.ColumnSortings.split(',');
            //PrimaryKeyColumn, ParentColumn
            for (var iColumnName = 0; iColumnName < columnNames.length; iColumnName++) {
                var columnName = $.trim(columnNames[iColumnName]);
                var columnObj = {
                    field: columnName,
                    headerText: $.trim(columnHeaders[iColumnName]),
                    width: columnWidths[iColumnName],
                    textAlign: columnAligns[iColumnName],
                    visible: $.inArray(columnName, hiddenColumns) !== -1 ? false : true
                };
                if (columnName == $.trim(childGrid.PrimaryKeyColumn)) columnObj.isPrimaryKey = true;
                columns.push(columnObj);
            }
            ej.base.enableRipple(true);
            $tblChildEl = new ej.grids.Grid({
                dataSource: [],
                allowResizing: true,
                columns: columns,
                //editSettings: { allowEditing: true, allowAdding: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
                recordClick: function (args) {

                }
            });
            $tblChildEl.refreshColumns;
            $tblChildEl.appendTo(tblChildId);
        }

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
            //IsEnable
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
                                        ${setFinder(value.HasFinder, menuId, value.IsEnable)}
                                    </div>
                                </div>
                            </div>`;
                    }
                    else if (!value.IsSys && value.IsHidden && value.IsEnable) {
                        template += '<input type="hidden" id="' + value.ColumnName + '" name="' + value.ColumnName + '" value="" />'
                    }
                    else {
                        template +=
                            `<div class="form-group" style='${cssHidden}'>
                                <label class="col-sm-2 control-label ci">${value.Label}</label>
                                <div class="col-sm-10">
                                    <div class="input-group input-group-sm" style='width: 100%;'>
                                        <input type="text" class="form-control" id="${value.ColumnName}" name="${value.ColumnName}" ${cssEnable} />
                                        ${setFinder(value.HasFinder, menuId, value.IsEnable)}
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
                                        ${setFinder(value.HasFinder, menuId, value.IsEnable)}
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
                                    ${setFinder(value.HasFinder, menuId, value.IsEnable)}
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
    }
    function setFinder(hasFinder, menuId, isEnable) {
        if (hasFinder && isEnable) {
            return `<span class="input-group-btn">
                        <button type="button" class="btn btn-success" id="ci-finder-${menuId}"><i class="fa fa-search" aria-hidden="true"></i></button>
                    </span>`;
        }
        return "";
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

            axios.get(value.apiUrl)
                .then(function (response) {
                    el.select2({
                        placeholder: "Select " + value.placeholder,
                        allowClear: true,
                        data: response.data
                    });
                    el.val(null).trigger('change');
                })
                .catch(showResponseError);
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
            title: 'Actions',
            align: 'center',
            width: 100,
            formatter: function () {
                return [
                    '<span class="btn-group">',
                    '<a class="btn btn-success btn-xs remove" href="javascript:void(0)" title="Delete Item">',
                    '<i class="fa fa-trash"></i>',
                    '</a>',
                    '</span>'
                ].join('');
            },
            events: {
                'click .remove': function (e, value, row, index) {
                    e.stopPropagation();
                    var itemIndex = masterData.Childs.rows.findIndex(function (element) {
                        return element[childPrimaryKey] == row[childPrimaryKey];
                    });

                    if (!row[childPrimaryKey] || row[childPrimaryKey] <= 0) {
                        masterData.Childs.rows.splice(itemIndex, 1);
                        masterData.Childs.total = masterData.Childs.rows.length;
                        //$tblChildEl.bootstrapTable('load', masterData.Childs);
                    }
                    else {
                        masterData.Childs.rows[itemIndex].EntityState = 8;
                        var $target = $(e.target);
                        $target.closest("tr").addClass('deleted-row');
                    }
                }
            }
        }
        columnList.push(actionColumn);

        $.each(_columnNames, function (i, value) {
            var hidden = _hiddenColumns.find(function (element) {
                return element == value;
            });
            if (hidden) return true;

            var column = {
                field: value,
                title: _columnHeaders[i],
                align: _columnAligns[i],
                width: _columnWidths[i],
                sortable: convertToBoolean(_columnSortings[i])
            }

            if (convertToBoolean(_columnFilters[i])) {
                column["filterControl"] = "input";
            }

            switch (_columnTypes[i]) {
                case "checkbox":
                    column["formatter"] = function (v) {
                        return v ? '<span class="bg-success">Yes<span>' : '<span class="bg-warning">No<span>'
                    };
                    break;
                case "select":
                    var optionListName = value.replace(new RegExp("id", "ig"), "");
                    optionListName += "List";

                    column.editable = {
                        type: 'select2',
                        showbuttons: false,
                        title: _columnHeaders[i],
                        inputclass: 'input-sm',
                        source: masterData[optionListName],
                        select2: { width: 150, placeholder: _columnHeaders[i] }
                    };
                    break;
                default:
                    column.editable = {
                        type: 'text',
                        inputclass: 'input-sm m-w-50',
                        showbuttons: false
                    };
                    break;
            }

            columnList.push(column);
        });

        //$tblChildEl.bootstrapTable({
        //    pagination: true,
        //    filterControl: true,
        //    sidePagination: "client",
        //    pageList: "[10, 25, 50, 100, 500]",
        //    cache: false,
        //    columns: columnList,
        //    editable: true,
        //    data: masterData.Childs
        //});
    }

    // #endregion

    // #region Finder Section
    function showFinderData() {
        var finderData = interfaceConfigs.Childs.find(function (element) {
            return element.HasFinder == true;
        });

        finderApiUrl = finderData.FinderApiUrl + "?gridType=bootstrap-table";

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
                title: _columnTitles[i],
                align: _columnAligns[i],
                width: _columnWidths[i]

                //Sorting ignored because it was not working properly
                //sortable: convertToBoolean(_columnSortings[i])
            };
            if (convertToBoolean(_columnFilters[i])) {
                column["filterControl"] = "input";
            }
            columnList.push(column);
        });

        /*
        $("#tblSearchData-" + menuId).bootstrapTable({
            pagination: true,
            filterControl: true,
            searchOnEnterKey: true,
            sidePagination: "server",
            pageList: "[10, 25, 50, 100, 500]",
            cache: false,
            columns: columnList,
            onDblClickRow: function (row, $element, field) {

                var url = interfaceConfigs.ApiUrl + "/" + row[masterPrimaryKey];
                axios.get(url)
                    .then(function (response) {
                        masterData = response.data;
                        setMasterData();
                        $("#modal-finder-" + menuId).modal('hide');
                    })
                    .catch(showResponseError);
            },
            onPageChange: function (number, size) {
                var newOffset = (number - 1) * size;
                var newLimit = size;
                if (finderTableParams.offset == newOffset && finderTableParams.limit == newLimit)
                    return;
                finderTableParams.offset = newOffset;
                finderTableParams.limit = newLimit;

                getFinderData();
            },
            onSort: function (name, order) {
                finderTableParams.sort = name;
                finderTableParams.order = order;
                finderTableParams.offset = 0;

                getFinderData();
            },
            onRefresh: function () {
                finderTableParams.offset = 0;
                finderTableParams.limit = 10;
                finderTableParams.sort = '';
                finderTableParams.order = '';
                finderTableParams.filter = '';

                getFinderData();
            },
            onColumnSearch: function (columnName, filterValue) {
                if (columnName in filterBy && !filterValue) {
                    delete filterBy[columnName];
                }

                if (filterValue)
                    filterBy[columnName] = filterValue;

                if (Object.keys(filterBy).length === 0 && filterBy.constructor === Object)
                    finderTableParams.filter = "";
                else
                    finderTableParams.filter = JSON.stringify(filterBy);

                getFinderData();
            }
        });
        */
        $(".filter-control input").attr('placeholder', 'Type & Enter for Search');
        $(".filter-control input").css('border', '1px solid gray');
        $(".filter-control input").css('font-size', '13px');

        axios.get(finderApiUrl)
            .then(function (response) {
                //$("#tblSearchData-" + menuId).bootstrapTable('load', response.data);
                $("#modal-finder-" + menuId).modal("show");
            })
            .catch(showResponseError);
    }

    function getFinderData() {
        var queryParams = $.param(finderTableParams);
        var url = finderApiUrl + "?" + queryParams;
        axios.get(url)
            .then(function (response) {
                //$("#tblSearchData-" + menuId).bootstrapTable('load', response.data);
            })
            .catch(showResponseError)
    }
    // #endregion

    function resetForm() {
        $formEl.trigger("reset");

        $.each(interfaceConfigs.Childs, function (i, value) {
            if (value.EntryType == "select")
                $('#' + value.ColumnName).val('').trigger('change');
        });

        $formEl.find(`#${masterPrimaryKey}`).val(-1111);
        $formEl.find("#EntityState").val(4);

        resetChildForm();
        //$tblChildEl.bootstrapTable("removeAll");
    }

    function resetChildForm() {
        childForm.trigger("reset");
        childForm.find(`#${childPrimaryKey}`).val(-1111);
        childForm.find("#EntityState").val(4);
    }

    function newId() {
        resetForm();
        $formEl.find(`#${masterPrimaryKey}`).val(-1111);
    }

    // #region Save
    function saveMaster(e) {
        e.preventDefault();
        if (!validateMasterForm()) return;

        $formEl.find(':checkbox').each(function () {
            this.value = this.checked;
        });
        var data = formDataToJson($formEl.serializeArray());
        if (masterData && masterData.Childs) data["Childs"] = masterData.Childs;

        var config = {
            headers: {
                'Content-Type': 'application/json'
            }
        };

        axios.post(interfaceConfigs.ApiUrl, data, config)
            .then(function () {
                toastr.success(constants.SUCCESS_MESSAGE);
                resetForm();
            })
            .catch(showResponseError);
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

        data[interfaceConfigs.ChildGrids[0].ParentColumn] = $formEl.find(`#${masterPrimaryKey}`).val();

        masterData.Childs.rows.push(data);
        masterData.Childs.total = masterData.Childs.rows.length;

        // $tblChildEl.bootstrapTable('load', masterData.Childs);

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
            // $tblChildEl.bootstrapTable('load', masterData.Childs);
        }
    }
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