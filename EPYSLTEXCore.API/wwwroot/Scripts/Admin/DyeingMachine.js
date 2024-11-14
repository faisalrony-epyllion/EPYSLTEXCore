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
    var DyeingMachine;

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
            var newChildItem = {
                Id: getMaxIdForArray(DyeingMachine.DyeingMachineProcesses, "Id"),
                DmId: DyeingMachine.Id,
                DyeProcessId: 0
            };
            DyeingMachine.DyeingMachineProcesses.push(newChildItem);
            $tblChildEl.bootstrapTable('load', DyeingMachine.DyeingMachineProcesses);
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
                        return `<a class="btn btn-xs btn-default edit" href="javascript:void(0)" title="Edit Dyeing Machine">
                                        <i class="fa fa-edit" aria-hidden="true"></i>
                                    </a>`;
                    },
                    events: {
                        'click .edit': function (e, value, row, index) {
                            e.preventDefault();
                            getDetails(row.DMID);
                        }
                    }
                },
                {
                    field: "DyeingMcName",
                    title: "Dyeing M/C Name",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "Company",
                    title: "Company",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "DyeingMcStatus",
                    title: "Machine Status",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "DyeingMcBrand",
                    title: "Machine Brand",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "DyeingMcslNo",
                    title: "SL. No"
                },
                {
                    field: "DyeingMcCapacity",
                    title: "Capacity"
                },
                {
                    field: "DyeingNozzleQty",
                    title: "Nozzle"
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
        var url = `/api/dyeing-machine/list?gridType=bootstrap-table&${queryParams}`;
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
            columns: [
                {
                    title: "Actions",
                    align: "center",
                    width: 30,
                    formatter: function (value, row, index, field) {
                        return `<a class="btn btn-xs btn-default remove" href="javascript:void(0)" title="Remove">
                                        <i class="fa fa-remove" aria-hidden="true"></i>
                                    </a>`;
                    },
                    events: {
                        'click .remove': function (e, value, row, index) {
                            e.preventDefault();
                            row.EntityState = 8;
                            $tblChildEl.bootstrapTable('updateByUniqueId', { id: row.DMProcessID, row: row });
                            $tblChildEl.bootstrapTable('hideRow', { index: index });
                        }
                    }
                },
                {
                    field: "DyeProcessID",
                    title: "Dye Process",
                    width: 250,
                    cellStyle: function () { return { classes: 'm-w-300' } },
                    editable: {
                        type: 'select2',
                        title: 'Select Dye Process',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: DyeingMachine.DyeProcessList,
                        select2: { width: 200, placeholder: 'Dye Process', allowClear: true }
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
        axios.get(`/api/dyeing-machine/new`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                DyeingMachine = response.data;
                setFormData($formEl, DyeingMachine);
                initChildTable();
                $tblChildEl.bootstrapTable("load", DyeingMachine.DyeingMachineProcesses);
                $tblChildEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getDetails(id) {
        axios.get(`/api/dyeing-machine/${id}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                DyeingMachine = response.data;
                setFormData($formEl, DyeingMachine);
                initChildTable();
                $tblChildEl.bootstrapTable("load", DyeingMachine.DyeingMachineProcesses);
                $tblChildEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function save() {
        var data = formDataToJson($formEl.serializeArray());
        data.IsCC = $formEl.find("#IsCC").prop("checked");
        data["DyeingMachineProcesses"] = DyeingMachine.DyeingMachineProcesses;
        axios.post("/api/dyeing-machine/save", data)
            .then(function () {
                toastr.success("Saved successfully!");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }


})();