(function () {
    var menuId, pageName;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $tblChildEl, $formEl;
    var filterBy = {};
    var status = statusConstants.PENDING;
    var tableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    }
    var isEditable = true;
    var RollFinishing;

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
        initMasterTable();
        getMasterTableData();

        $toolbarEl.find("#btnList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PENDING;
            initMasterTable();
            getMasterTableData();
        });
        $toolbarEl.find("#btnCompleteList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.COMPLETED;
            initMasterTable();
            getMasterTableData();
        });

        $formEl.find("#btnAddItem").on("click", function (e) {
            e.preventDefault();
            addNewItem();
        });

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save(this);
        });

        $formEl.find("#btnCancel").on("click", backToList);
    });

    function addNewItem() {
        var newChildItem = {
            RFinishingID: getMaxIdForArray(RollFinishing.Childs, "RFinishingID"),
            ProcessID: 0,
            MachineID: 0,
            TempIn: 0,
            Speed: 0,
            Feed: 0,
            Stream: 0,
            EntityState: 4
        };
        RollFinishing.Childs.push(newChildItem);
        $tblChildEl.bootstrapTable('load', RollFinishing.Childs);
    }

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
                    field: "",
                    align: "center",
                    visible: status == statusConstants.PENDING,
                    formatter: function (value, row, index, field) {
                        return [
                            '<a class="btn btn-xs btn-default edit" href="javascript:void(0)" title="New">',
                            '<i class="fa fa-plus" aria-hidden="true"></i>',
                            '</a>'
                        ].join(' ');
                    },
                    events: {
                        'click .edit': function (e, value, row, index) {
                            e.preventDefault();
                            getNew(row.GRollID);
                        }
                    }
                },
                {
                    field: "view",
                    align: "center",
                    width: 50,
                    visible: status !== statusConstants.PENDING,
                    formatter: function (value, row, index, field) {
                        return [
                            '<a class="btn btn-xs btn-default view" href="javascript:void(0)" title="View">',
                            '<i class="fa fa-eye" aria-hidden="true"></i>',
                            '</a>'
                        ].join(' ');
                    },
                    events: {
                        'click .view': function (e, value, row, index) {
                            e.preventDefault();
                            if (row) {
                                HoldOn.open({
                                    theme: "sk-circle"
                                });
                                getDetails(row.GRollID);
                            }
                        }
                    }
                },
                {
                    field: "RollNo",
                    title: "Roll No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "RollFinishingQCStatusName",
                    title: "Status",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    visible: status == statusConstants.COMPLETED
                },
                {
                    field: "RollFinishingWidth",
                    title: "Width",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    visible: status == statusConstants.COMPLETED
                },
                {
                    field: "RollFinishingGSM",
                    title: "GSM",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    visible: status == statusConstants.COMPLETED
                },
                {
                    field: "BatchNo",
                    title: "Batch No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "ProductionDate",
                    title: "Production Date",
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "BatchStartTime",
                    title: "Batch Start Time",
                    formatter: function (value, row, index, field) {
                        return formatDateToHHMMADMMMYYYY(value);
                    }
                },
                {
                    field: "BatchEndTime",
                    title: "Batch End Time",
                    formatter: function (value, row, index, field) {
                        return formatDateToHHMMADMMMYYYY(value);
                    }
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
        var url = "/api/roll-finishing-qc/list?gridType=bootstrap-table&status=" + status + "&" + queryParams;
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
            uniqueId: 'RFinishingID',
            editable: isEditable,
            columns: [
                {
                    field: "ProcessID",
                    title: "Process",
                    align: 'center',
                    visible: isEditable,
                    editable: {
                        type: 'select2',
                        title: 'Select Process',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: RollFinishing.ProcessList,
                        select2: { width: 200, placeholder: 'Select Process Name', allowClear: true }
                    }
                },
                {
                    field: "MachineID",
                    title: "Machine No",
                    align: 'center',
                    visible: isEditable,
                    editable: {
                        type: 'select2',
                        title: 'Select Machine',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: RollFinishing.MachineList,
                        select2: { width: 200, placeholder: 'Select Machine Name', allowClear: true }
                    }
                },
                {
                    field: "Process",
                    title: "Process",
                    align: 'center',
                    visible: !isEditable,
                },
                {
                    field: "MachineNo",
                    title: "Machine No",
                    align: 'center',
                    visible: !isEditable,
                },
                {
                    field: "TempIn",
                    title: "Temp",
                    editable: {
                        type: "text",
                        showbuttons: false,
                        tpl: '<input type="text" class="form-control input-sm" min="0" style="padding-right: 24px;">',

                    }
                },
                {
                    field: "Speed",
                    title: "Speed",
                    editable: {
                        type: "text",
                        showbuttons: false,
                        tpl: '<input type="text" class="form-control input-sm" min="0" style="padding-right: 24px;">',

                    }
                },
                {
                    field: "Feed",
                    title: "Feed",
                    editable: {
                        type: "text",
                        showbuttons: false,
                        tpl: '<input type="text" class="form-control input-sm" min="0" style="padding-right: 24px;">',

                    }
                },
                {
                    field: "Stream",
                    title: "Stream",
                    editable: {
                        type: "text",
                        showbuttons: false,
                        tpl: '<input type="text" class="form-control input-sm" min="0" style="padding-right: 24px;">',

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
        $.each($formEl.find('select'), function (i, el) {
            $(el).select2('');
        });
        $formEl.find("#RFinishingID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function resetTableParams() {
        tableParams.offset = 0;
        tableParams.limit = 10;
        tableParams.filter = '';
        tableParams.sort = '';
        tableParams.order = '';
    }

    function getNew(batchId) {
        var url = "/api/roll-finishing-qc/new/" + batchId;
        axios.get(url)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                RollFinishing = response.data;
                $formEl.find("#btnSave").show();
                isEditable = false;
                setFormData($formEl, RollFinishing);
                initChildTable();

                $tblChildEl.bootstrapTable("load", RollFinishing.Childs);
                $tblChildEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function getDetails(id) {
        axios.get(`/api/roll-finishing-qc/${id}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                RollFinishing = response.data;
                $formEl.find("#btnAddItem").hide();
                $formEl.find("#btnSave").hide();
                isEditable = false;
                $formEl.find("#RollFinishingQCStatus").prop("disabled", true);
                $formEl.find("#RollFinishingWidth").prop("readonly", true);
                $formEl.find("#RollFinishingGSM").prop("readonly", true);
                $formEl.find("#RollFinishingComments").prop("disabled", true);
                setFormData($formEl, RollFinishing);
                initChildTable();

                $tblChildEl.bootstrapTable("load", RollFinishing.Childs);
                $tblChildEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function save() {
        var data = formDataToJson($formEl.serializeArray());
        data["ChildColors"] = RollFinishing.ChildColors;
        data["Childs"] = RollFinishing.Childs;
        axios.post("/api/roll-finishing-qc/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }
})();