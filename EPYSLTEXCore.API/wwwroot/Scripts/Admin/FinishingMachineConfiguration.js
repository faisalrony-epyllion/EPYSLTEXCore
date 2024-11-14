(function () {
    var menuId, pageName;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $tblChildEl, $formEl;
    var filterBy = {};
    var tableParams = {
        offset: 0,
        limit: 10,
        filter: '',
        sort: '',
        order: ''
    }
    var status;
    var FinishingMachineConfiguration;

    $(function () {
        if (!menuId)
            menuId = localStorage.getItem("menuId");
        if (!pageName)
            pageName = localStorage.getItem("pageName");

        var pageId = pageName + "-" + menuId;
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        $tblMasterEl = $(pageConstants.MASTER_TBL_ID_PREFIX + pageId);
        $tblChildEl = $(pageConstants.CHILD_TBL_ID_PREFIX + pageId);
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);
        status = statusConstants.COMPLETED;

        initMasterTable();
       // initChildTable();
        getMasterTableData();

        $toolbarEl.find("#btnNew").on("click", function (e) {
            e.preventDefault();
            getNew();
        });

        $toolbarEl.find("#btnList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.COMPLETED;

            initMasterTable();
            getMasterTableData();
        });

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save();
        });

        $formEl.find("#btnCancel").on("click", backToList);

        $formEl.find("#btnNewItem").on("click", function (e) {
            e.preventDefault();
            if (FinishingMachineConfiguration.FinishingMachineConfigurationChilds.length == 20) {
                toastr.error("Total Child can not be greater than 20!");
                return;
            }
            var newChildItem = {
                Id: getMaxIdForArray(FinishingMachineConfiguration.FinishingMachineConfigurationChilds, "Id"),
                FMCMasterID: FinishingMachineConfiguration.Id,
                ParamName: '',
                ParamDisplayName: '',
                Sequence: getMaxIdForArray(FinishingMachineConfiguration.FinishingMachineConfigurationChilds, "Sequence")
            };

            FinishingMachineConfiguration.FinishingMachineConfigurationChilds.push(newChildItem);
            $tblChildEl.bootstrapTable('load', FinishingMachineConfiguration.FinishingMachineConfigurationChilds);
        });
    });

    function initMasterTable() {
        $tblMasterEl.bootstrapTable('destroy');
        $tblMasterEl.bootstrapTable({
            showRefresh: true,
            showExport: true,
            showColumns: true,
            toolbar: toolbarId,
            exportTypes: "['csv', 'excel']",
            pagination: true,
            filterControl: true,
            searchOnEnterKey: true,
            sidePagination: "server",
            pageList: "[10, 25, 50, 100, 500]",
            cache: false,
            showFooter: true,
            columns: [
                {
                    title: "Actions",
                    align: "center",
                    formatter: function (value, row, index, field) {
                        return `<a class="btn btn-xs btn-default edit" href="javascript:void(0)" title="Edit Machine Configuration">
                                        <i class="fa fa-edit" aria-hidden="true"></i>
                                    </a>`;
                    },
                    events: {
                        'click .edit': function (e, value, row, index) {
                            e.preventDefault();
                            getDetails(row.FMCMasterID);
                        }
                    }
                },
                {
                    field: "ProcessType",
                    title: "Process Type",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "ProcessName",
                    title: "Process Name",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                }
            ],
            onPageChange: function (number, size) {
                var newOffset = (number - 1) * size;
                var newLimit = size;
                if (tableParams.offset == newOffset && tableParams.limit == newLimit)
                    return;

                tableParams.offset = newOffset;
                tableParams.limit = newLimit;

                getMasterTableData();
            },
            onSort: function (name, order) {
                tableParams.sort = name;
                tableParams.order = order;
                tableParams.offset = 0;

                getMasterTableData();
            },
            onRefresh: function () {
                resetTableParams();
                getMasterTableData();
            },
            onColumnSearch: function (columnName, filterValue) {
                if (columnName in filterBy && !filterValue) {
                    delete filterBy[columnName];
                }
                else
                    filterBy[columnName] = filterValue;

                if (Object.keys(filterBy).length === 0 && filterBy.constructor === Object)
                    tableParams.filter = "";
                else
                    tableParams.filter = JSON.stringify(filterBy);

                getMasterTableData();
            }
        });
    }

    function getMasterTableData() {
        var queryParams = $.param(tableParams);
        $tblMasterEl.bootstrapTable('showLoading');
        var url = `/api/finishing-machine-configuration/list?gridType=bootstrap-table&status=${status}&${queryParams}`;
        axios.get(url)
            .then(function (response) {
                $tblMasterEl.bootstrapTable('load', response.data);
                $tblMasterEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            })
    }

    function initChildTable() {
        $tblChildEl.bootstrapTable("destroy");
        $tblChildEl.bootstrapTable({
            uniqueId: 'FMCChildID',
            //editable: isEditable,
            showFooter: true,
            checkboxHeader: false,
         
            columns: [

                {
                    title: "Actions",
                    align: "center",
                    formatter: function (value, row, index, field) {
                        return `<a class="btn btn-xs btn-default remove" href="javascript:void(0)" title="Remove">
                                        <i class="fa fa-remove" aria-hidden="true"></i>
                                    </a>`;
                    },
                    events: {
                        'click .remove': function (e, value, row, index) {
                            showBootboxConfirm("Delete Record.", "Are you sure want to delete this?", function (yes) {
                                if (yes) {
                                    e.preventDefault();
                                    row.EntityState = 8;
                                    $tblChildEl.bootstrapTable('updateByUniqueId', { id: row.Id, row: row });
                                    $tblChildEl.bootstrapTable('hideRow', { index: index });
                                }
                            });
                           
                        }
                    },
                    width: 50
                },
                {
                    field: "ParamName",
                    title: "Param Name",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "ParamDisplayName",
                    title: "Param Display Name",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "Sequence",
                    title: "Sequence",
                    cellStyle: function () { return { classes: 'm-w-60' } }
                },
                {
                    field: "ProcessTypeID",
                    title: "Process Type",
                    width: 250,
                    cellStyle: function () { return { classes: 'm-w-300' } },
                    editable: {
                        type: 'select2',
                        title: 'Select Dye Process',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: FinishingMachineConfiguration.ProcessTypeList,
                        select2: { width: 200, placeholder: 'Process Type', allowClear: true }
                    }
                },
                {
                    field: "NeedItem",
                    title: "Need Item?",
                    width: 80,
                    checkbox: true,
                    showSelectTitle: true
                  
                },
                {
                    field: "DefaultValue",
                    title: "Default Value",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                }
             ]
        });
    }

    function backToList() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        getMasterTableData();
    }

    function resetForm() {
        $formEl.trigger("reset");
        $formEl.find("#Id").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function resetTableParams() {
        tableParams.offset = 0;
        tableParams.limit = 10;
        tableParams.filter = '';
        tableParams.sort = '';
        tableParams.order = '';
    }

    function getNew() {
        axios.get(`/api/finishing-machine-configuration/new`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                FinishingMachineConfiguration = response.data;
                setFormData($formEl, FinishingMachineConfiguration);
                initChildTable();
                $tblChildEl.bootstrapTable("load", FinishingMachineConfiguration.FinishingMachineConfigurationChilds);
                $tblChildEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getDetails(id) {
        axios.get(`/api/finishing-machine-configuration/${id}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                FinishingMachineConfiguration = response.data;
                setFormData($formEl, FinishingMachineConfiguration);
                initChildTable();
                $tblChildEl.bootstrapTable("load", FinishingMachineConfiguration.FinishingMachineConfigurationChilds);
                $tblChildEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function save() {
        var data = formDataToJson($formEl.serializeArray());
        data["FinishingMachineConfigurationChilds"] = FinishingMachineConfiguration.FinishingMachineConfigurationChilds;
        axios.post("/api/finishing-machine-configuration/save", data)
            .then(function () {
                toastr.success("Saved successfully!");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }


})();