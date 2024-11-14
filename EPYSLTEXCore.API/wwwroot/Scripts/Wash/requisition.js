(function () {
    var menuId, pageName, toolbarId;
    var $divTblEl, $formEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $tblChildEl, $formEl, $tblWashTypeEl, $washTypeModalEl, $btnAddWashTypeEl;
    var filterBy = {};
    var tableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        order: '',
        filter: ''
    }

    var washReqMaster = {};
    var status = statusConstants.PENDING;

    var unitList = [];
    var washTypeList = [];
    var selectedWashReqChild = {};

    $(function () {
        if (!menuId)
            menuId = localStorage.getItem("menuId");
        if (!pageName)
            pageName = localStorage.getItem("pageName");

        washReqMaster.MenuId = menuId;

        var pageId = pageName + "-" + menuId;
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        $tblMasterEl = $(pageConstants.MASTER_TBL_ID_PREFIX + pageId);
        $tblChildEl = $(pageConstants.CHILD_TBL_ID_PREFIX + pageId);
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);
        $tblWashTypeEl = $("#tblWashType" + pageId);
        $washTypeModalEl = $("#modalWashtype" + pageId);
        $btnAddWashTypeEl = $("#btnAddWashType" + pageId);

        initMasterTable();
        getMasterTableData();

        getUnits();

        initWashTypeTable();

        getWashTypes();

        $toolbarEl.find("#btnPending").on("click", function (e) {
            e.preventDefault();
            resetTableParams();
            status = statusConstants.PENDING;
            initMasterTable();
            getMasterTableData();

            toggleActiveToolbarBtn(this, $toolbarEl);
        });

        $toolbarEl.find("#btnAcknowledged").on("click", function (e) {
            e.preventDefault();
            resetTableParams();
            status = statusConstants.ACKNOWLEDGE;
            initMasterTable();
            getMasterTableData();

            toggleActiveToolbarBtn(this, $toolbarEl);
        });

        $toolbarEl.find("#btnProposed").on("click", function (e) {
            e.preventDefault();
            resetTableParams();
            status = statusConstants.PROPOSED;
            initMasterTable();
            getMasterTableData();

            toggleActiveToolbarBtn(this, $toolbarEl);
        });

        $toolbarEl.find("#btnReturnProposedPrice").on("click", function (e) {
            e.preventDefault();
            resetTableParams();
            status = statusConstants.RETURN_PROPOSE_PRICE;
            initMasterTable();
            getMasterTableData();

            toggleActiveToolbarBtn(this, $toolbarEl);
        });

        $toolbarEl.find("#btnAll").on("click", function (e) {
            e.preventDefault();
            resetTableParams();
            status = statusConstants.ALL;
            initMasterTable();
            getMasterTableData();

            toggleActiveToolbarBtn(this, $toolbarEl);
        });

        $btnAddWashTypeEl.click(handleAddWashType);

        $formEl.find("#btnCancel").click(backToList);

        $formEl.find("#btnSave").click(handleSave);

        $formEl.find("#btnAcknowledge").click(handleAcknowledge);

        $formEl.find("#btnPropose").click(handlePropose);
    });

    function handleAcknowledge(e) {
        e.preventDefault();

        var id = $formEl.find("#ReqID").val();
        axios.post("/api/wash-requisition/acknowledge/" + id)
            .then(function () {
                toastr.success("Acknowledged successfully.");
                backToList();
            })
            .catch(showResponseError);
    }

    function handlePropose(e) {
        e.preventDefault();
        washReqMaster.ProposedPrice = true;
        save();
    }

    function handleSave(e) {
        e.preventDefault();
        save();
    }

    function handleAddWashType(e) {
        e.preventDefault();

        $washTypeModalEl.modal("hide");

        var selectedWashTypes = $tblWashTypeEl.bootstrapTable('getSelections');
        if (selectedWashTypes.length === 0) return toastr.warning("You must select a wash type.");

        selectedWashReqChild.WashReqWashTypes = selectedWashTypes.map(function (item) { return { WashTypeId: item.id } });
        selectedWashReqChild.ActualWashType = selectedWashTypes.map(function (item) { return item.text }).toString();
        $tblChildEl.bootstrapTable("updateByUniqueId", selectedWashReqChild.ChildID, selectedWashReqChild);
    }

    function save() {
        if (status === statusConstants.RETURN_PROPOSE_PRICE) washReqMaster.ReturnProposedPrice = true;
        washReqMaster.MenuId = menuId;

        axios.post("/api/wash-requisition", washReqMaster)
            .then(function () {
                toastr.success("Wash requisition saved successfully.");
                backToList();
            })
            .catch(showResponseError);
    }

    function initMasterTable() {
        $tblMasterEl.bootstrapTable('destroy');
        $tblMasterEl.bootstrapTable({
            showRefresh: true,
            showExport: true,
            showColumns: true,
            toolbar: $toolbarEl,
            exportTypes: "['csv', 'excel']",
            pagination: true,
            filterControl: true,
            searchOnEnterKey: true,
            sidePagination: "server",
            pageList: "[10, 25, 50, 100, 500]",
            cache: false,
            columns: [
                {
                    field: "Actions",
                    title: "Actions",
                    align: "center",
                    width: 50,
                    formatter: function (value, row, index, field) {
                        var template = "";
                        if (status === statusConstants.PENDING) {
                            template =
                                `<a class="btn btn-xs btn-primary m-w-30 ack" href="javascript:void(0)" title="Acknowledge">
                                    <i class="fa fa-eye" aria-hidden="true"></i>
                                </a>`;
                        }
                        else if (status === statusConstants.ACKNOWLEDGE || status == statusConstants.RETURN_PROPOSE_PRICE) {
                            template =
                                `<a class="btn btn-xs btn-primary m-w-30 edit" href="javascript:void(0)" title="Propose">
                                    <i class="fa fa-edit" aria-hidden="true"></i>
                                </a>`;
                        }
                        else {
                            template =
                                `<a class="btn btn-xs btn-primary m-w-30 view" href="javascript:void(0)" title="View Details">
                                    <i class="fa fa-eye" aria-hidden="true"></i>
                                </a>`;
                        }

                        return template;
                    },
                    events: {
                        'click .ack': function (e, value, row, index) {
                            e.preventDefault();

                            showDetails(row);

                            $formEl.find("#btnSave").hide();
                            $formEl.find("#btnPropose").hide();
                            $formEl.find("#btnAcknowledge").show();
                        },
                        'click .edit': function (e, value, row, index) {
                            e.preventDefault();

                            showDetails(row);

                            if (status === statusConstants.ACKNOWLEDGE) {
                                $formEl.find("#btnSave").show();
                                $formEl.find("#btnPropose").show();
                                $formEl.find("#btnAcknowledge").hide();
                            }
                            else { // Return for Propose
                                $formEl.find("#btnSave").hide();
                                $formEl.find("#btnPropose").show();
                                $formEl.find("#btnAcknowledge").hide();
                            }
                        },
                        'click .view': function (e, value, row, index) {
                            e.preventDefault();

                            showDetails(row);

                            $formEl.find("#btnSave").hide();
                            $formEl.find("#btnPropose").hide();
                            $formEl.find("#btnAcknowledge").hide();
                        }
                    }
                },
                {
                    field: "ReqNo",
                    title: "ReqNo",
                    filterControl: "input",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "ReqDate",
                    title: "Req Date",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "SupplierName",
                    title: "Supplier",
                    filterControl: "input",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "BuyerName",
                    title: "Buyer",
                    filterControl: "input",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "TeamName",
                    title: "Team",
                    filterControl: "input",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "WashType",
                    title: "Wash Type",
                    filterControl: "input",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "WashSampleType",
                    title: "Wash Sample Type",
                    filterControl: "input",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "StyleNoList",
                    title: "Style No",
                    filterControl: "input",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "TotalStyleQty",
                    title: "Style Qty",
                    cellStyle: function () { return { classes: 'm-w-50' } }
                },
                {
                    field: "SeasonName",
                    title: "Season",
                    filterControl: "input",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "DeliveryDate",
                    title: "Delivery Date",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "BookingNo",
                    title: "Booking No",
                    filterControl: "input",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "Status",
                    title: "Status",
                    cellStyle: function () { return { classes: 'm-w-100' } }
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

    function initChildTable() {
        $tblChildEl.bootstrapTable('destroy');
        $tblChildEl.bootstrapTable({
            editable: status == statusConstants.ACKNOWLEDGE || status === statusConstants.RETURN_PROPOSE_PRICE,
            idField: "ChildID",
            columns: [
                {
                    field: "Color",
                    title: "GMT Color",
                    cellStyle: function () { return { classes: 'm-w-100' } }
                },
                {
                    field: "WashType",
                    title: "Wash Type",
                    cellStyle: function () { return { classes: 'm-w-100' } }
                },
                {
                    field: "ActualWashType",
                    title: "Actual Wash Type",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    visible: status === statusConstants.ACKNOWLEDGE || status === statusConstants.PROPOSED,
                    formatter: function (value, row, index, field) {
                        if (status === statusConstants.ACKNOWLEDGE) {
                            var text = value ? value : row.WashType;
                            return `<a role="button" class="btn btn-link edit-actual-wash-type">${text}</a>`;
                        }
                    },
                    events: {
                        'click .edit-actual-wash-type': function (e, value, row, index) {
                            selectedWashReqChild = row;

                            row.WashReqWashTypes.forEach(function (item) {
                                var washType = _.find(washTypeList, function (el) { return el.WashTypeID == item.WashTypeId });
                                if (washType) washType.IsActual = true;
                            })

                            washTypeList = _.sortBy(washTypeList, function (item) { return item.IsActual });

                            $tblWashTypeEl.bootstrapTable("load", washTypeList);

                            $washTypeModalEl.modal("show");
                        }
                    }
                },
                {
                    field: "ReqQty",
                    title: "Qty",
                    cellStyle: function () { return { classes: 'm-w-100' } }
                },
                {
                    field: "Unit",
                    title: "UOM",
                    cellStyle: function () { return { classes: 'm-w-60' } }
                },
                {
                    field: "WeightPerGmt",
                    title: "Weight/GMT (gram)",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    visible: status != statusConstants.PENDING,
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
                },
                {
                    field: "ProductionDayQty",
                    title: "Production/Day Qty",
                    visible: status !== statusConstants.PENDING,
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    editable: {
                        type: "text",
                        showbuttons: false,
                        tpl: '<input type="number" class="form-control input-sm" min="1" style="padding-right: 24px;">',
                        validate: function (value) {
                            if (!value || !value.trim() || isNaN(parseInt(value)) || parseInt(value) <= 0) {
                                return 'Must be a positive integer.';
                            }
                        }
                    }
                },
                {
                    field: "Limitation",
                    title: "Limitation",
                    visible: status !== statusConstants.PENDING,
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    editable: {
                        type: "text",
                        showbuttons: false
                    }
                },
                {
                    field: "ProposedPrice",
                    title: "Proposed Price",
                    visible: status !== statusConstants.PENDING,
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    editable: {
                        type: "text",
                        showbuttons: false,
                        validate: function (value) {
                            if (!value || !value.trim() || isNaN(value) || parseFloat(value) <= 0) {
                                return 'Must be a valid number.';
                            }
                        }
                    }
                },
                {
                    field: "RateUnitID",
                    title: "Proposed Unit",
                    visible: status == statusConstants.ACKNOWLEDGE,
                    editable: {
                        type: 'select2',
                        title: 'Select Proposed Rate Unit',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: unitList,
                        select2: { width: 100, placeholder: 'Proposed Rate Unit' },
                        cellStyle: function () { return { classes: 'm-w-100' } },
                    }
                },
                {
                    field: "ReturnProposedPrice",
                    title: "Return Propose Price",
                    visible: status == statusConstants.RETURN_PROPOSE_PRICE,
                    cellStyle: function () { return { classes: 'm-w-60' } }
                },
                {
                    field: "TCXCode",
                    title: "Tcx Code",
                    cellStyle: function () { return { classes: 'm-w-100' } }
                },
                {
                    field: "StyleTypeName",
                    title: "Style Type",
                    cellStyle: function () { return { classes: 'm-w-100' } }
                },
                {
                    field: "StyleNoList",
                    title: "Style No",
                    cellStyle: function () { return { classes: 'm-w-100' } }
                },
                {
                    field: "Remarks",
                    title: "Remarks",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    editable: {
                        type: "text",
                        showbuttons: false
                    }
                }
            ]
        });
    }

    function initWashTypeTable() {
        $tblWashTypeEl.bootstrapTable('destroy');
        $tblWashTypeEl.bootstrapTable({
            checkboxHeader: false,
            idField: "WashTypeChildID",
            height: 400,
            clickToSelect: true,
            columns: [
                {
                    field: "IsActual",
                    checkbox: true
                },
                {
                    field: "text",
                    title: "Wash Type",
                    cellStyle: function () { return { classes: 'm-w-100' } }
                },
                {
                    field: "desc",
                    title: "Process Child Name",
                    cellStyle: function () { return { classes: 'm-w-100' } }
                }
            ]
        });
    }

    function getMasterTableData() {
        var queryParams = $.param(tableParams);
        $tblMasterEl.bootstrapTable('showLoading');
        var url = "/api/wash-requisition?gridType=bootstrap-table&status=" + status + "&" + queryParams;
        axios.get(url)
            .then(function (response) {
                $tblMasterEl.bootstrapTable('load', response.data);
                $tblMasterEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            })
    }

    function getChilds(id) {
        $tblChildEl.bootstrapTable('showLoading');

        axios.get("/api/wash-requisition/childs/" + id)
            .then(function (response) {
                washReqMaster.Childs = response.data;
                $tblChildEl.bootstrapTable('load', washReqMaster.Childs);
                $tblChildEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data);
            });
    }

    function resetTableParams() {
        tableParams.offset = 0;
        tableParams.limit = 10;
        tableParams.filter = '';
        tableParams.sort = '';
        tableParams.order = '';
    }

    function backToList() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();

        getMasterTableData();
    }

    function resetForm() {
        $formEl.trigger("reset");
        $formEl.find("#ReqID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function getUnits() {
        axios.get("/api/selectoption/units")
            .then(function (response) {
                unitList = _.filter(response.data, function (item) {
                    return item.text == "Pcs" || item.text == "Dzn";
                });
            })
            .catch(function () {
                console.error(err.response);
                toastr.error(err.response.data);
            })
    }

    function getWashTypes() {
        axios.get("/api/selectoption/process-child-sub-process")
            .then(function (response) {
                washTypeList = response.data;
            })
            .catch(function () {
                console.error(err.response);
                toastr.error(err.response.data);
            })
    }

    function showDetails(data) { 
        $divTblEl.fadeOut();
        $divDetailsEl.fadeIn();
        washReqMaster = data;
        washReqMaster.ReqDate = formatDateToDefault(washReqMaster.ReqDate);
        washReqMaster.DeliveryDate = formatDateToDefault(washReqMaster.DeliveryDate);

        setFormData($formEl, data);

        var preveiwData = [constants.GMT_ERP_BASE_PATH + data.ImagePath];
        var previewConfig = [{ type: "pdf", caption: "Wash development attachment.", key: 1 }];
        $formEl.find("#ImagePath").fileinput('destroy');
        $formEl.find("#ImagePath").fileinput({
            initialPreview: preveiwData,
            initialPreviewAsData: true,
            initialPreviewConfig: previewConfig,
            showBrowse: false
        });

        initChildTable();
        getChilds(data.ReqID);
    }
})();