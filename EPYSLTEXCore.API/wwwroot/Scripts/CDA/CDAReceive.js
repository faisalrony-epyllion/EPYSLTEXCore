(function () {
    var menuId, pageName;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $tblChildEl, $formEl;
    var filterBy = {};
    var tableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    }
    var status;

    var CDAReceive;

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

        status = statusConstants.PendingReceiveCI;
        initMasterTable();
        initChildTable();
        getMasterTableData();

        $(".clockpicker").clockpicker({
            autoclose: true,
            default: 'now'
        });

        $toolbarEl.find("#btnPendingReceive").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PendingReceiveCI;
            $toolbarEl.find("#PendingType").fadeIn();
            $toolbarEl.find("#btnRdoCommercialInvoice").prop("checked", true);
            $toolbarEl.find("#btnRdoPurchaseOrder").prop("checked", false);
            initMasterTable();
            getMasterTableData();
        });

        $toolbarEl.find("#btnReceiveLists").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.COMPLETED;
            $toolbarEl.find("#PendingType").fadeOut();
            initMasterTable();
            getMasterTableData();
        });

        $toolbarEl.find("#btnRdoCommercialInvoice,#lblCommercialInvoice").on("click", function (e) {
            e.preventDefault();
            //toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PendingReceiveCI;

            initMasterTable();
            getMasterTableData();
            $toolbarEl.find("#btnRdoCommercialInvoice").prop("checked", true);
        });

        $toolbarEl.find("#btnRdoPurchaseOrder,#lblPurchaseOrder").on("click", function (e) {
            e.preventDefault();
            //toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PendingReceivePO;

            initMasterTable();
            getMasterTableData();
            $toolbarEl.find("#btnRdoPurchaseOrder").prop("checked", true);
        });

        $formEl.find("#btnSaveYR").click(function (e) {
            e.preventDefault();
            save(this);
        });

        $formEl.find("#btnYREditCancel").on("click", backToList);
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
                        if (status === statusConstants.PendingReceiveCI || status === statusConstants.PendingReceivePO) {
                            return `<a class="btn btn-xs btn-default add" href="javascript:void(0)" title="New Receive">
                                        <i class="fa fa-plus" aria-hidden="true"></i>
                                    </a>`;
                        }
                        else {
                            return `<a class="btn btn-xs btn-default edit" href="javascript:void(0)" title="Edit Receive">
                                        <i class="fa fa-edit" aria-hidden="true"></i>
                                    </a>`;
                        }
                    },
                    events: {
                        'click .add': function (e, value, row, index) {
                            e.preventDefault();
                            if (status === statusConstants.PendingReceiveCI) {
                                $formEl.find(".PO").fadeOut();
                                $formEl.find(".CI").fadeIn();
                                $formEl.find("#ACompanyInvoice").prop("disabled", true);
                            } else if (status === statusConstants.PendingReceivePO) {
                                $formEl.find(".PO").fadeIn();
                                $formEl.find(".CI").fadeOut();
                                $formEl.find("#ACompanyInvoice").prop("disabled", false);
                            }
                            initChildTable();
                            getNew(row.CiId, row.PoId);
                        },
                        'click .edit': function (e, value, row, index) {
                            e.preventDefault();
                            getDetails(row.Id);
                        }
                    }
                },
                {
                    field: "ReceiveNo",
                    title: "Receive No",
                    filterControl: "input",
                    visible: (status === statusConstants.COMPLETED),
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "ReceiveDate",
                    title: "Receive Date",
                    visible: (status === statusConstants.COMPLETED),
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "InvoiceNo",
                    title: "Invoice No",
                    filterControl: "input",
                    visible: (status === statusConstants.PendingReceiveCI || status === statusConstants.COMPLETED),
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "InvoiceDate",
                    title: "Invoice Date",
                    visible: (status === statusConstants.PendingReceiveCI || status === statusConstants.COMPLETED),
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "PoNo",
                    title: "PO No",
                    filterControl: "input",
                    visible: (status === statusConstants.PendingReceivePO || status === statusConstants.COMPLETED),
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "PoDate",
                    title: "PO Date",
                    visible: (status === statusConstants.PendingReceivePO || status === statusConstants.COMPLETED),
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "PoQty",
                    title: "Total Qty",
                    filterControl: "input"
                },
              
                {
                    field: "InvoiceValue",
                    title: "Invoice Value",
                    filterControl: "input",
                    visible: (status === statusConstants.PendingReceiveCI),
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    align: 'right',
                    footerFormatter: calculateCITotalInvoiceValue
                },
                {
                    field: "LcNo",
                    title: "LC No",
                    filterControl: "input",
                    visible: (status === statusConstants.PendingReceiveCI || status === statusConstants.COMPLETED),
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "LcDate",
                    title: "LC Date",
                    visible: (status === statusConstants.PendingReceiveCI || status === statusConstants.COMPLETED),
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
             


                {
                    field: "SupplierName",
                    title: "Supplier",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "RCompany",
                    title: "Rcv Company",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },



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
        var url = `/api/CDA-receive/list?status=${status}&${queryParams}`;
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
                    field: "ItemName",
                    title: "Item Name",
                    width: 150
                },
                {
                    field: "AgentName",
                    title: "Agent Name",
                    width: 150
                },
                {
                    field: "PoQty",
                    title: "PO Qty",
                    align: 'center',
                    width: 50,
                    visible: (status !== statusConstants.PendingReceiveCI),
                    footerFormatter: calculateCDAReceiveCITotalPIQty
                },
                {
                    field: "InvoiceQty",
                    title: "Invoice Qty",
                    align: 'center',
                    width: 50,
                    visible: (status !== statusConstants.PendingReceivePO),
                    footerFormatter: calculateCITotalPIQty
                },
                {
                    field: "DisplayUnitDesc",
                    title: "Unit",
                    width: 50
                },
                {
                    field: "LotNo",
                    title: "Lot No",
                    width: 30,
                    align: 'center',
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "ChallanQty",
                    title: "Challan/PL Qty",
                    width: 30,
                    align: 'center',
                    editable: {
                        type: 'number',
                        inputclass: 'input-sm',
                        showbuttons: false
                    },
                    footerFormatter: calculateCITotalChallanQty
                },
                {
                    field: "ReceiveQty",
                    title: "Receive Qty",
                    width: 30,
                    align: 'center',
                    editable: {
                        type: 'number',
                        inputclass: 'input-sm',
                        showbuttons: false
                    },
                    footerFormatter: calculateCITotalReceiveQty
                },
                {
                    field: "ExcessQty",
                    title: "Excess Qty",
                    align: 'center',
                    width: 50,
                    footerFormatter: calculateCITotalExcessQty
                },
                {
                    field: "ShortQty",
                    title: "Short Qty",
                    align: 'center',
                    width: 50,
                    footerFormatter: calculateCITotalShortQty
                },
                {
                    field: "Remarks",
                    title: "Remarks",
                    width: 50,
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                }
            ],
            onEditableSave: function (field, row, oldValue, $el) {
                if (parseFloat(row.ChallanQty) > parseFloat(row.ReceiveQty)) {
                    row.ShortQty = parseFloat(row.ChallanQty) - parseFloat(row.ReceiveQty);
                    row.ExcessQty = 0;
                } else if (parseFloat(row.ChallanQty) < parseFloat(row.ReceiveQty)) {
                    row.ExcessQty = parseFloat(row.ReceiveQty) - parseFloat(row.ChallanQty);
                    row.ShortQty = 0;
                } else {
                    row.ExcessQty = 0;
                    row.ShortQty = 0;
                }
                $tblChildEl.bootstrapTable('load', CDAReceive.ReceiveChilds);
            }
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

    function getNew(CiId, PoId) {

        axios.get(`/api/CDA-receive/new/${CiId}/${PoId}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                CDAReceive = response.data;
                CDAReceive.ReceiveDate = formatDateToDefault(CDAReceive.ReceiveDate);
                CDAReceive.PoDate = formatDateToDefault(CDAReceive.PoDate);
                CDAReceive.PiDate = formatDateToDefault(CDAReceive.PiDate);
                CDAReceive.LcDate = formatDateToDefault(CDAReceive.LcDate);
                CDAReceive.InvoiceDate = formatDateToDefault(CDAReceive.InvoiceDate);
                CDAReceive.ChallanDate = formatDateToDefault(CDAReceive.ChallanDate);
                setFormData($formEl, CDAReceive);
                $tblChildEl.bootstrapTable("load", CDAReceive.ReceiveChilds);
                $tblChildEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getDetails(id) {
        axios.get(`/api/CDA-receive/${id}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                CDAReceive = response.data;
                CDAReceive.ReceiveDate = formatDateToDefault(CDAReceive.ReceiveDate);
                CDAReceive.PoDate = formatDateToDefault(CDAReceive.PoDate);
                CDAReceive.PiDate = formatDateToDefault(CDAReceive.PiDate);
                CDAReceive.LcDate = formatDateToDefault(CDAReceive.LcDate);
                CDAReceive.InvoiceDate = formatDateToDefault(CDAReceive.InvoiceDate);
                CDAReceive.ChallanDate = formatDateToDefault(CDAReceive.ChallanDate);
                initChildTable();
                setFormData($formEl, CDAReceive);
                $tblChildEl.bootstrapTable("load", CDAReceive.ReceiveChilds);
                $tblChildEl.bootstrapTable('hideLoading');

                if (CDAReceive.PoNo == null || CDAReceive.PoNo == "") {
                    $formEl.find(".PO").fadeOut();
                    $formEl.find(".CI").fadeIn();
                    $formEl.find("#ACompanyInvoice").prop("disabled", true);
                } else {
                    $formEl.find(".PO").fadeIn();
                    $formEl.find(".CI").fadeOut();
                    $formEl.find("#ACompanyInvoice").prop("disabled", false);
                }
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function save(invokedBy) {
        var data = formDataToJson($formEl.serializeArray());
        data["ReceiveChilds"] = CDAReceive.ReceiveChilds;

        axios.post("/api/CDA-receive/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }


    function calculateCITotalInvoiceValue(data) {
        var ciLCValue = 0;

        $.each(data, function (i, row) {
            ciLCValue += isNaN(parseFloat(row.CiValue)) ? 0 : parseFloat(row.CiValue);
        });

        return ciLCValue.toFixed(2);
    }

    function calculateCDAReceiveCITotalPIQty(data) {
        var ciYRPIQty = 0;

        $.each(data, function (i, row) {
            ciYRPIQty += isNaN(parseFloat(row.PoQty)) ? 0 : parseFloat(row.PoQty);
        });

        return ciYRPIQty.toFixed(2);
    }

    function calculateCITotalPIQty(data) {
        var ciPIQty = 0;

        $.each(data, function (i, row) {
            ciPIQty += isNaN(parseFloat(row.InvoiceQty)) ? 0 : parseFloat(row.InvoiceQty);
        });

        return ciPIQty.toFixed(2);
    }

    function calculateCITotalReceiveQty(data) {
        var yRecQty = 0;

        $.each(data, function (i, row) {
            yRecQty += isNaN(parseFloat(row.ReceiveQty)) ? 0 : parseFloat(row.ReceiveQty);
        });

        return yRecQty.toFixed(2);
    }

    function calculateCITotalChallanQty(data) {
        var yChallancQty = 0;

        $.each(data, function (i, row) {
            yChallancQty += isNaN(parseFloat(row.ChallanQty)) ? 0 : parseFloat(row.ChallanQty);
        });

        return yChallancQty.toFixed(2);
    }

    function calculateCITotalExcessQty(data) {
        var yExchessQty = 0;

        $.each(data, function (i, row) {
            yExchessQty += isNaN(parseFloat(row.ExcessQty)) ? 0 : parseFloat(row.ExcessQty);
        });

        return yExchessQty.toFixed(2);
    }

    function calculateCITotalShortQty(data) {
        var yShortQty = 0;

        $.each(data, function (i, row) {
            yShortQty += isNaN(parseFloat(row.ShortQty)) ? 0 : parseFloat(row.ShortQty);
        });

        return yShortQty.toFixed(2);
    }


})();