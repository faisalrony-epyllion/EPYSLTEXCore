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
    var columnList = [];
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

        status = statusConstants.PENDING;
        initMasterTable();
        //initChildTable();
        getMasterTableData();

        $toolbarEl.find("#btnPending").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PENDING;

            initMasterTable();
            getMasterTableData();
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
            var newChildItem = {
                FMSID: getMaxIdForArray(FinishingMachineConfiguration.FinishingMachineSetups, "FMSID"),
                FMCMasterID: FinishingMachineConfiguration.FMCMasterID,
                Brand: 0,
                Unit: 0,
                Capacity: 0,
                MachineNo: ''
            };
            for (var i = 0; i < FinishingMachineConfiguration.FinishingMachineConfigurationChilds.length; i++) {
                newChildItem[`Param${i + 1}Value`] = "";
            }

            FinishingMachineConfiguration.FinishingMachineSetups.push(newChildItem);
            $tblChildEl.bootstrapTable('load', FinishingMachineConfiguration.FinishingMachineSetups);
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
                        if (status === statusConstants.PENDING) {
                            return `<a class="btn btn-xs btn-default add" href="javascript:void(0)" title="New Setup">
                                        <i class="fa fa-plus" aria-hidden="true"></i>
                                    </a>`;
                        }
                        else {
                            return `<a class="btn btn-xs btn-default edit" href="javascript:void(0)" title="View Setup">
                                        <i class="fa fa-edit" aria-hidden="true"></i>
                                    </a>`;
                        }
                    },
                    events: {
                        'click .add': function (e, value, row, index) {
                            e.preventDefault();
                            getNew(row.FMCMasterID);
                        },
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
        var url = `/api/finishing-machine-setup/list?gridType=bootstrap-table&status=${status}&${queryParams}`;
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
            showFooter: true,
            columns: columnList
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

    function getNew(newId) {
        axios.get(`/api/finishing-machine-setup/new/${newId}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                FinishingMachineConfiguration = response.data;
                setFormData($formEl, FinishingMachineConfiguration);

                columnList = [];
                generateChildGrid();
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function generateChildGrid() {
        var column = {
            field: "BrandID",
            title: "Brand",
            editable: {
                type: 'select2',
                title: 'Select Brand',
                inputclass: 'input-sm',
                showbuttons: false,
                source: FinishingMachineConfiguration.BrandList,
                select2: { width: 200, placeholder: 'Select Brand', allowClear: true }
            }
        }
        columnList.push(column);

        column = {
            field: "UnitID",
            title: "Unit",
            editable: {
                type: 'select2',
                title: 'Select Unit',
                inputclass: 'input-sm',
                showbuttons: false,
                source: FinishingMachineConfiguration.UnitList,
                select2: { width: 200, placeholder: 'Select Unit', allowClear: true }
            }
        }
        columnList.push(column);

        column = {
            field: "Capacity",
            title: "Capacity",
            align: 'center',
            editable: {
                type: "text",
                showbuttons: false,
                tpl: '<input type="number" class="form-control input-sm" min="0" style="padding-right: 24px;">',
                validate: function (value) {
                    if (!value || !value.trim() || isNaN(parseInt(value)) || parseInt(value) <= 0) {
                        return 'Must be a positive integer.';
                    }
                }
            }
        }
        columnList.push(column);

        column = {
            field: "MachineNo",
            title: "Machine No",
            align: 'center',
            editable: {
                type: "text",
                showbuttons: false,
                tpl: '<input type="text" class="form-control input-sm" min="0" style="padding-right: 24px;">'
            }
        }
        columnList.push(column);

        for (var i = 0; i < FinishingMachineConfiguration.FinishingMachineConfigurationChilds.length; i++) {
            column = {
                field: `Param${i+1}Value`,
                title: FinishingMachineConfiguration.FinishingMachineConfigurationChilds[i].ParamDisplayName,
                align: "center",
                editable: {
                    type: "text",
                    showbuttons: false,
                    tpl: '<input type="text" class="form-control input-sm" min="0" style="padding-right: 24px;">'
                }
            }
            columnList.push(column);
        }

        initChildTable();
        $tblChildEl.bootstrapTable("load", FinishingMachineConfiguration.FinishingMachineSetups);
        $tblChildEl.bootstrapTable('hideLoading');
    }

    function getDetails(id) {
        axios.get(`/api/finishing-machine-setup/${id}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                FinishingMachineConfiguration = response.data;
                setFormData($formEl, FinishingMachineConfiguration);

                columnList = [];
                generateChildGrid();
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function save() {
        axios.post("/api/finishing-machine-setup/save", FinishingMachineConfiguration)
        .then(function () {
            toastr.success("Saved successfully.");
            backToList();
        })
        .catch(function (error) {
            toastr.error(error.response.data.Message);
        });
    }


})();