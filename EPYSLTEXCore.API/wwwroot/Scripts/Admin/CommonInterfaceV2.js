(function () {
    'use strict'

    // #region variable declarations
    var constraints = [];
    var childConstraints = [];
    var $formEl, $tblChildEl;
    var masterData;
    var childForm;
    var childObject = {};
    var interfaceConfigs;
    var menuId;
    var filterBy = {};
    var finderApiUrl = "";
    var childGridApiUrl = "";

    var selectColumnList = [];
    var childSelectColumnList = [];

    var finderTableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    };
    var childGridTableParams = {
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
        $tblChildEl = $("#tabaleGridData-" + menuId);
        getInterfaceChilds();

        $("#btnSaveMaster-" + menuId).click(saveMaster);

        $("#btnSaveChild-" + menuId).click(function (e) {
            e.preventDefault();
            saveChild();
        });

        $("#btnNewChildItem-" + menuId).click(function (e) {
            e.preventDefault();

            if (!validateMasterForm()) return;

            var newChild = Object.create(childObject);
            newChild.Id = getMaxIdForArray(masterData.Childs, "Id");
            masterData.Childs.unshift(newChild);
            $tblChildEl.bootstrapTable('load', masterData.Childs);
        });
    });

    // #region Genereting markup
    function getInterfaceChilds() {
        var url = '/api/common-interface/configs?menuId=' + menuId;
        axios.get(url)
            .then(function (response) {
                interfaceConfigs = response.data;
                $("#title-form-ci-" + menuId).text(interfaceConfigs.InterfaceName);
                $("#finderTitle-" + menuId).text(interfaceConfigs.InterfaceName + " List");

                //if (interfaceConfigs.is) {
                //    var deleteBtn = `<button class="btn btn-success w-150" id="btnDeleteMaster-${menuId}"><span class="ladda-label"><i class="fa fa-save"></i>&nbsp; Delete Info</span></button>`;
                //    $(`#masterButtons-${menuId}`).append(deleteBtn);
                //}

                generateElements();
                initControls();

                if (interfaceConfigs.HasGrid) {
                    $("#title-child-grid-" + menuId).text(interfaceConfigs.ChildGrids[0].ChildGridName);
                    $("#boxChildGrid-" + menuId).show();
                    initChildGrid();
                }
            })
            .catch(function (error) {
                //console.log(error)
            })
    }

    function generateElements() {
        var template = "";
        $.each(interfaceConfigs.Childs, function (i, value) {
            switch (value.EntryType) {
                case "text":
                    if (value.IsSys) {
                        template +=
                            `<div class="form-group">
                                <label class="col-sm-2 control-label ci">${value.Label}</label>
                                <div class="col-sm-10">
                                    <div class="input-group input-group-sm">
                                        <input type="text" class="form-control" id="${value.ColumnName}" name="${value.ColumnName}" readonly><span class="input-group-btn">
                                            <button type="button" class="btn btn-default" id="ci-new-${menuId}"><i class="fa fa-plus" aria-hidden="true"></i></button>
                                            <button type="button" class="btn btn-success" id="ci-finder-${menuId}"><i class="fa fa-search" aria-hidden="true"></i></button>
                                        </span>
                                    </div>
                                </div>
                            </div>`;
                    }
                    else if (!value.IsSys && value.IsHidden && value.IsEnable) {
                        template += '<input type="hidden" id="' + value.ColumnName + '" name="' + value.ColumnName + '" value=""/>'
                    }
                    else {
                        template +=
                            `<div class="form-group">
                                <label class="col-sm-2 control-label ci">${value.Label}</label>
                                <div class="col-sm-10">
                                    <input type="text" class="form-control input-sm" id="${value.ColumnName}" name="${value.ColumnName}" placeholder="${value.Label}">
                                </div>
                            </div>`;
                    }
                    break;
                case "datetime":
                case "date":
                    template +=
                        `<div class="form-group">
                            <label class="col-sm-2 control-label ci">${value.Label}</label>
                            <div class="col-sm-10">
                                <input type="text" class="form-control input-sm bootstrap-datepicker" id="${value.ColumnName}" name="${value.ColumnName}" placeholder="${value.Label}">
                            </div>
                        </div>`;
                    break;
                case "select":
                    template +=
                        `<div class="form-group">
                            <label class="col-sm-2 control-label ci">${value.Label}</label>
                            <div class="col-sm-10">
                                <select class="form-control" id="${value.ColumnName}" name="${value.ColumnName}" style="width: 100%"></select>
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
                        `<div class="form-group">
                            <label class="col-sm-2 control-label ci">${value.Label}</label>
                            <div class="col-sm-10">
                                <input type="number" class="form-control input-sm" id="${value.ColumnName}" name="${value.ColumnName}" placeholder="${value.Label}">
                            </div>
                        </div>`;
                    break;
                case "checkbox":
                    template +=
                        `<div class="form-group">
                            <div class="col-sm-offset-2 col-sm-10">
                                <div class="checkbox checkbox-success">
                                    <input type="checkbox" id="${value.ColumnName}" name="${value.ColumnName}"> <label for="${value.ColumnName}">${value.Label}</label>
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
        });

        $formEl.append(template);
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

    function initChildGrid() {
        var url = `/api/common-interface/details/${menuId}/0`;
        axios.get(url)
            .then(function (response) {
                masterData = response.data;
                childObject = masterData.Childs.pop();
                generateChildGrid();
            })
            .catch(showResponseError)
    }

    function loadDependentSelection(el, apiUrl) {
        var url = apiUrl + "/" + id;
        axios.get(url)
            .then(function (response) {
                el.select2({
                    data: response.data
                });
            })
            .catch(function (error) {
                //console.log(error.response)
            });
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
                    e.preventDefault();

                    showBootboxConfirm("Confirm Delete", "Are you sure you want to delete this?", function (yes) {
                        if (yes) {
                            var parentId = $formEl.find("#Id").val();
                            var index = masterData.Childs.findIndex(function (el) { return el.Id === row.Id });

                            if (parentId > 0 && row.Id > 0) { // If exists in DB
                                axios.delete(`/api/common-interface/delete-child/${menuId}/${row.Id}`)
                                    .then(function () {
                                        toastr.success("Item deleted successfully.");
                                        masterData.Childs.splice(index, 1);
                                        $tblChildEl.bootstrapTable("load", masterData.Childs);
                                    })
                                    .catch(showResponseError)
                            }
                            else {
                                masterData.Childs.splice(index, 1);
                                $tblChildEl.bootstrapTable("load", masterData.Childs);
                            }
                        }
                    });
                }
            }
        };

        columnList.push(actionColumn);

        if (!childObject) childObject = {};
        $.each(_columnNames, function (i, value) {
            childObject[value] = null;

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
                    var optionListName = value.replace("Id", '');
                    optionListName = optionListName.replace("ID", '');
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
                    break;
            }

            columnList.push(column);
        });

        $tblChildEl.bootstrapTable({
            pagination: true,
            filterControl: true,
            searchOnEnterKey: true,
            sidePagination: "client",
            pageList: "[10, 25, 50, 100, 500]",
            cache: false,
            columns: columnList,
            data: masterData.Childs,
            onPageChange: function (number, size) {
                var newOffset = (number - 1) * size;
                var newLimit = size;
                if (childGridTableParams.offset == newOffset && childGridTableParams.limit == newLimit)
                    return;

                childGridTableParams.offset = newOffset;
                childGridTableParams.limit = newLimit;

                getGridData();
            },
            onSort: function (name, order) {
                childGridTableParams.sort = name;
                childGridTableParams.order = order;
                childGridTableParams.offset = 0;

                getGridData();
            },
            onRefresh: function () {
                childGridTableParams.offset = 0;
                childGridTableParams.limit = 10;
                childGridTableParams.sort = '';
                childGridTableParams.order = '';
                childGridTableParams.filter = '';

                getGridData();
            },
            onColumnSearch: function (columnName, filterValue) {
                if (columnName in filterBy && !filterValue) {
                    delete filterBy[columnName];
                }
                else
                    filterBy[columnName] = filterValue;

                if (Object.keys(filterBy).length === 0 && filterBy.constructor === Object)
                    childGridTableParams.filter = "";
                else
                    childGridTableParams.filter = JSON.stringify(filterBy);

                getGridData();
            }
        });
    }

    function getGridData() {
        var queryParams = $.param(childGridTableParams);
        var url = childGridApiUrl + "?" + queryParams;
        axios.get(url)
            .then(function (response) {
                $tblChildEl.bootstrapTable('load', response.data);
            })
            .catch(function () {
                toastr.error(constants.LOAD_ERROR_MESSAGE);
            })
    }
    // #endregion

    // #region Finder Section
    function showFinderData() {
        var finderData = interfaceConfigs.Childs.find(function (element) {
            return element.HasFinder == true;
        });
        finderApiUrl = finderData.FinderApiUrl + "?menuId=" + menuId;

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

        $("#tblSearchData-" + menuId).bootstrapTable({
            pagination: true,
            filterControl: true,
            searchOnEnterKey: true,
            sidePagination: "server",
            pageList: "[10, 25, 50, 100, 500]",
            cache: false,
            columns: columnList,
            onDblClickRow: function (row, $element, field) {
                var url = `/api/common-interface/details/${menuId}/${row.Id}`;
                //var url = interfaceConfigs.ApiUrl + "/" + row.Id;
                axios.get(url)
                    .then(function (response) {
                        masterData = response.data;
                        setMasterData();
                        $("#modal-finder-" + menuId).modal('hide');
                    })
                    .catch(function (error) {
                        //console.log(error.response.data);
                        toastr.error(error.response.data.Message)
                    });
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

        $(".filter-control input").attr('placeholder', 'Type & Enter for Search');
        $(".filter-control input").css('border', '1px solid gray');
        $(".filter-control input").css('font-size', '13px');

        axios.get(finderApiUrl)
            .then(function (response) {
                $("#tblSearchData-" + menuId).bootstrapTable('load', response.data);
                $("#modal-finder-" + menuId).modal("show");
            })
            .catch(showResponseError);
    }

    function getFinderData() {
        var queryParams = $.param(finderTableParams);
        var url = finderApiUrl + "&" + queryParams;
        axios.get(url)
            .then(function (response) {
                $("#tblSearchData-" + menuId).bootstrapTable('load', response.data);
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

        $formEl.find("#Id").val(-1111);
        $formEl.find("#EntityState").val(4);

        resetChildForm();
        $tblChildEl.bootstrapTable("removeAll");
    }

    function resetChildForm() {
        childForm.trigger("reset");
        childForm.find("#Id").val(-1111);
        childForm.find("#EntityState").val(4);
    }

    function newId() {
        resetForm();
        $formEl.find("#Id").val(-1111);
    }

    // #region Save
    function saveMaster(e) {
        e.preventDefault();

        if (!validateMasterForm()) return;

        $formEl.find(':checkbox').each(function () {
            this.value = this.checked;
        });
        var data = formDataToJson($formEl.serializeArray());
        data["Childs"] = masterData.Childs;

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

        data[interfaceConfigs.ChildGrids[0].ParentColumn] = $formEl.find("#Id").val();

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
            $tblChildEl.bootstrapTable('load', masterData.Childs);
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
})();