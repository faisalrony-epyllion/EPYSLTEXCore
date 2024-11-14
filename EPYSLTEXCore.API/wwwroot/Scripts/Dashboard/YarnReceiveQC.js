var formYarnReceiveQC;
var YarnReceiveQCMaster;
var filterBy = {};
var Id;
var yrStatus = 2;
var ReceiveChilds = [];
var isAllChecked = true;
var tableParams = {
    offset: 0,
    limit: 10,
    sort: '',
    order: '',
    filter: ''
}

$(function () {
    formYarnReceiveQC = $("#formYarnReceiveQC");

    $("#btnPendingReceiveQC").css('background', '#008080');
    $("#btnPendingReceiveQC").css('color', '#FFFFFF');

    initPendingYarnReceiveQCMasterTable();
    getPendingYarnReceiveQCMasterData();
    initTblYarnReceiveQCChildItems();

    $("#btnPendingReceiveQC").on("click", function (e) {
        e.preventDefault();
        resetTableParams();
        yrStatus = 2;
        isAllChecked = true;
        initPendingYarnReceiveQCMasterTable();
        getPendingYarnReceiveQCMasterData();

        $("#btnPendingReceiveQC").css('background', '#008080');
        $("#btnPendingReceiveQC").css('color', '#FFFFFF');

        $("#btnReceiveQCLists").css('background', '#FFFFFF');
        $("#btnReceiveQCLists").css('color', '#000000');
    });

    $("#btnReceiveQCLists").on("click", function (e) {
        e.preventDefault();
        resetTableParams();
        yrStatus = 4;
        isAllChecked = true;
        initPendingYarnReceiveQCMasterTable();
        getPendingYarnReceiveQCMasterData();

        $("#btnPendingReceiveQC").css('background', '#FFFFFF');
        $("#btnPendingReceiveQC").css('color', '#000000');

        $("#btnReceiveQCLists").css('background', '#008080');
        $("#btnReceiveQCLists").css('color', '#FFFFFF');
    });

    $("#btnSaveYRQC").click(function (e) {
        e.preventDefault();
        var data = formDataToJson(formYarnReceiveQC.serializeArray());
        data["ReceiveChilds"] = YarnReceiveQCMaster.ReceiveChilds;

        var config = { headers: { 'Content-Type': 'application/json' } };
        axios.post("/yrQCApi/yarnReceiveQCSave", data, config)
            .then(function () {
                toastr.success("Your Received QC saved successfully.");
                YarnReceiveQCbackToList();
            })
            .catch(showResponseError);
    });

    $("#btnYRQCEditCancel").on("click", function (e) {
        e.preventDefault();
        YarnReceiveQCbackToList();
    });
});

function initPendingYarnReceiveQCMasterTable() {
    $("#tblYarnReceiveQCMaster").bootstrapTable('destroy');
    $("#tblYarnReceiveQCMaster").bootstrapTable({
        showRefresh: true,
        showExport: true,
        showColumns: true,
        toolbar: "#YarnReceiveQCToolbar",
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
                width: 50,
                formatter: function (value, row, index, field) {
                    return [
                        '<a class="btn btn-xs btn-default edit-attachment" href="javascript:void(0)" title="Select Invoice">',
                        '<i class="fa fa-edit" aria-hidden="true"></i>',
                        '</span>'
                    ].join(' ');
                },
                events: {
                    'click .edit-attachment': function (e, value, row, index) {
                        e.preventDefault();
                        if (yrStatus == 2) {
                            $("#Id").val(row.Id);
                            getYarnReceiveQCEdit(row.Id);
                            $("#divNewYarnReceiveQC").fadeIn();
                            $("#divtblYarnReceiveQC").fadeOut();
                            $("#divYarnReceiveQCButtonExecutions").fadeIn();
                            $("#btnYRAcceptance").fadeOut();
                            $("#btnSaveYRQC").fadeIn();
                        }
                        else if (yrStatus == 4) {
                            $("#Id").val(row.Id);
                            getYarnReceiveQCEdit(row.Id);
                            $("#divNewYarnReceiveQC").fadeIn();
                            $("#divtblYarnReceiveQC").fadeOut();
                            $("#divYarnReceiveQCButtonExecutions").fadeIn();
                            $("#btnYRAcceptance").fadeOut();
                            $("#btnSaveYRQC").fadeIn();
                        }
                    }
                }
            },
            {
                field: "CustomerName",
                title: "Customer",
                width: 250,
                filterControl: "input",
                visible: !isAllChecked,
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "SupplierName",
                title: "Supplier",
                width: 250,
                filterControl: "input",
                visible: isAllChecked,
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "ReceiveNo",
                title: "Receive No",
                filterControl: "input",
                visible: isAllChecked,
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "ReceiveDateStr",
                title: "Receive Date",
                filterControl: "input",
                visible: isAllChecked,
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "ChallanNo",
                title: "Challan No",
                filterControl: "input",
                visible: isAllChecked,
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "ChallanDateStr",
                title: "Challan Date",
                filterControl: "input",
                visible: isAllChecked,
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "CiNo",
                title: "Invoice No",
                filterControl: "input",
                visible: !isAllChecked,
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "CiDateStr",
                title: "Invoice Date",
                filterControl: "input",
                visible: !isAllChecked,
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "CiValue",
                title: "Invoice Value",
                filterControl: "input",
                align: 'right',
                visible: !isAllChecked,
                footerFormatter: calculateCITotalInvoiceValue,
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "LcNo",
                title: "L/C No",
                filterControl: "input",
                visible: !isAllChecked,
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "LcDateStr",
                title: "L/C Date",
                filterControl: "input",
                visible: !isAllChecked,
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

            getPendingYarnReceiveQCMasterData();
        },
        onSort: function (name, order) {
            tableParams.sort = name;
            tableParams.order = order;
            tableParams.offset = 0;

            getPendingYarnReceiveQCMasterData();
        },
        onRefresh: function () {
            resetTableParams();
            getPendingYarnReceiveQCMasterData();
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

            getPendingYarnReceiveQCMasterData();
        }
    });
}

function getPendingYarnReceiveQCMasterData() {
    var queryParams = $.param(tableParams);
    $('#tblYarnReceiveQCMaster').bootstrapTable('showLoading');
    var url = "/yrQCApi/YarnReceivedQCLists?yrStatus=" + yrStatus + "&" + queryParams;
    axios.get(url)
        .then(function (response) {
            $("#tblYarnReceiveQCMaster").bootstrapTable('load', response.data);
            $('#tblYarnReceiveQCMaster').bootstrapTable('hideLoading');
        })
        .catch(function (err) {
            toastr.error(err.response.data.Message);
        })
}

function getNewYarnReceiveQC(id) {
    url = "/yrApi/newYarnReceiveQCData/" + id;

    axios.get(url)
        .then(function (response) {
            YarnReceiveQCMaster = response.data;
            $("#Id").val(response.data.Id);

            $("#ReceiveNo2").val(response.data.ReceiveNo);
            $("#SupplierID").val(response.data.SupplierId);
            $("#SupplierName").val(response.data.SupplierName);
            $("#PONo").val(response.data.PoNo);
            $("#PODate").val(response.data.PoDateStr);
            $("#PINo").val(response.data.PiNo);
            $("#PIDate").val(response.data.PiDateStr);
            $("#LCNo").val(response.data.LcNo);
            $("#LCDate").val(response.data.LcDateStr);
            $("#Tolerance").val(response.data.Tolerance);
            $("#InvoiceNo").val(response.data.InvoiceNo);
            $("#InvoiceDate").val(response.data.InvoiceDateStr);
            $("#BankBranchID").val(response.data.BankBranchId);
            $("#BankBranchName").val(response.data.BranchName);

            $("#TransportMode").select2({ 'data': response.data.TransportModeList, 'allowClear': true, 'placeholder': "Select a Value" });
            $("#TransportTypeID").select2({ 'data': response.data.TransportTypeList, 'allowClear': true, 'placeholder': "Select a Value" });
            $("#CContractorID").select2({ 'data': response.data.TransportAgencyList, 'allowClear': true, 'placeholder': "Select a Value" });
            $("#ShipmentStatus").select2({ 'data': response.data.ShipmentStatusList, 'allowClear': true, 'placeholder': "Select a Value" });
            $("#LocationID").select2({ 'data': response.data.LocationList, 'allowClear': true, 'placeholder': "Select a Value" });

            initTblYarnReceiveQCChildItems();

            $("#tblYarnReceiveQCChilds").bootstrapTable('load', response.data.ReceiveChilds);
        })
        .catch(function (err) {
            toastr.error(err.response.data.Message);
        })
}

function getYarnReceiveQCEdit(id) {
    url = "/yrQCApi/getYarnReceiveQCEdit/" + id;

    axios.get(url)
        .then(function (response) {
            YarnReceiveQCMaster = response.data;
            formYarnReceiveQC.find("#Id").val(response.data.Id);
            formYarnReceiveQC.find("#SupplierID").val(response.data.SupplierId);
            formYarnReceiveQC.find("#SupplierName").val(response.data.SupplierName);
            formYarnReceiveQC.find("#PONo").val(response.data.PoNo);
            formYarnReceiveQC.find("#PODate").val(response.data.PoDateStr);
            formYarnReceiveQC.find("#PINo").val(response.data.PiNo);
            formYarnReceiveQC.find("#PIDate").val(response.data.PiDateStr);
            formYarnReceiveQC.find("#LCNo").val(response.data.LcNo);
            formYarnReceiveQC.find("#LCDate").val(response.data.LcDateStr);
            formYarnReceiveQC.find("#Tolerance").val(response.data.Tolerance);
            formYarnReceiveQC.find("#InvoiceNo").val(response.data.InvoiceNo);
            formYarnReceiveQC.find("#InvoiceDate").val(response.data.InvoiceDateStr);
            formYarnReceiveQC.find("#BankBranchID").val(response.data.BankBranchId);
            formYarnReceiveQC.find("#BankBranchName").val(response.data.BranchName);

            formYarnReceiveQC.find("#ACompanyInvoice").val(response.data.ACompanyInvoice);
            formYarnReceiveQC.find("#ChallanNo").val(response.data.ChallanNo);
            formYarnReceiveQC.find("#ChallanDate").val(response.data.ChallanDateStr);
            formYarnReceiveQC.find("#VehicalNo").val(response.data.VehicalNo);
            formYarnReceiveQC.find("#Remarks").val(response.data.Remarks);
            formYarnReceiveQC.find("#ReceiveNo").val(response.data.ReceiveNo);
            formYarnReceiveQC.find("#ReceiveDate").val(response.data.ReceiveDateStr);

            formYarnReceiveQC.find("#TransportMode").select2({ 'data': response.data.TransportModeList });
            formYarnReceiveQC.find("#TransportMode").val(response.data.TransportMode).trigger("change");
            formYarnReceiveQC.find("#TransportTypeID").select2({ 'data': response.data.TransportTypeList });
            formYarnReceiveQC.find("#TransportTypeID").val(response.data.TransportTypeId).trigger("change");
            formYarnReceiveQC.find("#CContractorID").select2({ 'data': response.data.TransportAgencyList });
            formYarnReceiveQC.find("#CContractorID").val(response.data.CContractorId).trigger("change");
            formYarnReceiveQC.find("#ShipmentStatus").select2({ 'data': response.data.ShipmentStatusList });
            formYarnReceiveQC.find("#ShipmentStatus").val(response.data.ShipmentStatus).trigger("change");
            formYarnReceiveQC.find("#LocationID").select2({ 'data': response.data.LocationList });
            formYarnReceiveQC.find("#LocationID").val(response.data.LocationId).trigger("change");

            initTblYarnReceiveQCChildItems();

            formYarnReceiveQC.find("#tblYarnReceiveQCChilds").bootstrapTable('load', response.data.ReceiveChilds);
        })
        .catch(function (err) {
            toastr.error(err.response.data.Message);
        })
}

function initTblYarnReceiveQCChildItems() {
    $("#tblYarnReceiveQCChilds").bootstrapTable({
        showFooter: true,
        //detailView: true,
        columns: [
            {
                field: "YarnType",
                title: "Yarn Type",
                width: 60
            },
            {
                field: "YarnComposition",
                title: "Yarn Composition",
                width: 100
            },
            {
                field: "YarnCount",
                title: "Yarn Count",
                width: 60
            },
            {
                field: "YarnColor",
                title: "Yarn Color",
                width: 80
            },
            {
                field: "YarnShade",
                title: "Shade",
                width: 60
            },
            {
                field: "PoQty",
                title: "PO Qty",
                align: 'center',
                width: 50,
                footerFormatter: calculateYarnReceiveQCCITotalPIQty
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
                align: 'center'
            },
            {
                field: "NoOfCartoon",
                title: "No Of Cartoon",
                width: 30,
                align: 'center'
            },
            {
                field: "NoOfCone",
                title: "No Of Cone",
                width: 30,
                align: 'center'
            },
            {
                field: "InvoiceQty",
                title: "Invoice Qty",
                align: 'center',
                width: 50,
                footerFormatter: calculateCITotalPIQty
            },
            {
                field: "ChallanQty",
                title: "Challan/PL Qty",
                width: 30,
                align: 'center',
                footerFormatter: calculateCITotalChallanQty
            },
            {
                field: "ReceiveQty",
                title: "Receive Qty",
                width: 30,
                align: 'center',
                footerFormatter: calculateCITotalReceiveQty
            },
            {
                field: "Remarks",
                title: "Remarks",
                width: 50
            },
            {
                field: "QcPassedQty",
                title: "QC Passed Qty",
                align: 'center',
                width: 50,
                editable: {
                    type: 'text',
                    inputclass: 'input-sm',
                    showbuttons: false
                },
                footerFormatter: calculateYarnReceiveTotalQCPassedQty
            },
            {
                field: "QcFailedQty",
                title: "QC Failed Qty",
                align: 'center',
                editable: {
                    type: 'text',
                    inputclass: 'input-sm',
                    showbuttons: false
                },
                width: 50,
                footerFormatter: calculateYarnReceiveTotalQCFailedQty
            }
        ],
        //onExpandRow: function (index, row, $detail) {
        //    TechnicalNameId = row.TechnicalNameId;
        //    populateBookingAnalysisChildYarn(row.Id, row.TechnicalNameId, row.FabricComposition, $detail);
        //},
        onEditableSave: function (field, row, oldValue, $el) {
            if (row.ChallanQty > row.ReceiveQty) {
                row.ShortQty = row.ChallanQty - row.ReceiveQty;
                row.ExcessQty = 0;
            }
            else {
                row.ExcessQty = row.ReceiveQty - row.ChallanQty;
                row.ShortQty = 0;
            }

            $("#tblYarnReceiveQCChilds").bootstrapTable('load', YarnReceiveQCMaster.ReceiveChilds);
        }
    });
}
function YarnReceiveQCbackToList() {
    $("#divNewYarnReceiveQC").fadeOut();
    $("#divtblYarnReceiveQC").fadeIn();
    $("#divYarnReceiveQCButtonExecutions").fadeOut();
    getPendingYarnReceiveQCMasterData();

    $("#btnPendingReceiveQC").css('background', '#008080');
    $("#btnPendingReceiveQC").css('color', '#FFFFFF');

    $("#btnReceiveQCLists").css('background', '#FFFFFF');
    $("#btnReceiveQCLists").css('color', '#000000');
}

function resetTableParams() {
    tableParams.offset = 0;
    tableParams.limit = 10;
    tableParams.filter = '';
    tableParams.sort = '';
    tableParams.order = '';
}

function calculateCITotalInvoiceValue(data) {
    var ciLCValue = 0;

    $.each(data, function (i, row) {
        ciLCValue += isNaN(parseFloat(row.CiValue)) ? 0 : parseFloat(row.CiValue);
    });

    return ciLCValue.toFixed(2);
}

function calculateYarnReceiveQCCITotalPIQty(data) {
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

function calculateYarnReceiveTotalQCPassedQty(data) {
    var yQCPassedQty = 0;

    $.each(data, function (i, row) {
        yQCPassedQty += isNaN(parseFloat(row.QCPassedQty)) ? 0 : parseFloat(row.QCPassedQty);
    });

    return yQCPassedQty.toFixed(2);
}

function calculateYarnReceiveTotalQCFailedQty(data) {
    var yQCFailedQty = 0;

    $.each(data, function (i, row) {
        yQCFailedQty += isNaN(parseFloat(row.QCFailedQty)) ? 0 : parseFloat(row.QCFailedQty);
    });

    return yQCFailedQty.toFixed(2);
}