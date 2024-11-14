var formYarnIssueReturn;
var YarnIssueReturnMaster;
var filterBy = {};
var Id;
var yrStatus = 3;
var IssueChilds = [];
var isAllChecked = false;
var tableParams = {
    offset: 0,
    limit: 10,
    sort: '',
    order: '',
    filter: ''
}

$(function () {
    formYarnIssueReturn = $("#formYarnIssueReturn");

    $("#btnPendingIssue").css('background', '#008080');
    $("#btnPendingIssue").css('color', '#FFFFFF');

    initPendingYarnIssueReturnMasterTable();
    getPendingYarnIssueReturnMasterData();

    $("#btnPendingIssue").on("click", function (e) {
        e.preventDefault();
        yrStatus = 3;
        isAllChecked = false;
        resetTableParams();
        initPendingYarnIssueReturnMasterTable();
        getPendingYarnIssueReturnMasterData();

        $("#btnPendingIssue").css('background', '#008080');
        $("#btnPendingIssue").css('color', '#FFFFFF');

        $("#btnPendingIssueforApproval").css('background', '#FFFFFF');
        $("#btnPendingIssueforApproval").css('color', '#000000');

        $("#btnIssueLists").css('background', '#FFFFFF');
        $("#btnIssueLists").css('color', '#000000');
    });

    $("#btnIssueLists").on("click", function (e) {
        e.preventDefault();
        yrStatus = 3;
        isAllChecked = true;
        resetTableParams();
        initPendingYarnIssueReturnMasterTable();
        getPendingYarnIssueReturnMasterData();

        $("#btnIssueLists").css('background', '#008080');
        $("#btnIssueLists").css('color', '#FFFFFF');

        $("#btnPendingIssue").css('background', '#FFFFFF');
        $("#btnPendingIssue").css('color', '#000000');

        $("#btnPendingIssueforApproval").css('background', '#FFFFFF');
        $("#btnPendingIssueforApproval").css('color', '#000000');
    });

    $("#btnIssueUnApproved").on("click", function (e) {
        e.preventDefault();
        yrStatus = 3;
        resetTableParams();
        initPendingYarnIssueReturnMasterTable();
        getPendingYarnIssueReturnMasterData();
    });

    $("#btnAddOrders").on("click", function (e) {
        e.preventDefault();
        getExportOrdersFromBuyerCompany();
        $("#modal-child").modal('show');
    });

    $("#btnAddIssueBy").on("click", function (e) {
        e.preventDefault();
        $("#modal-child-").modal('show');
        getIssueByUsers();
    });

    $("#btnSaveYIssue").click(function (e) {
        e.preventDefault();
        var data = formDataToJson(formYarnIssueReturn.serializeArray());
        data["IssueChilds"] = YarnIssueReturnMaster.IssueChilds;

        var config = { headers: { 'Content-Type': 'application/json' } };
        axios.post("/yissueApi/YarnIssueSave", data, config)
            .then(function () {
                toastr.success("Your Issue saved successfully." + " Issue No: " + $("#IssueNo2").val());
                YarnIssueReturnbackToList();
            })
            .catch(showResponseError);
    });

    $("#btnYIssueEditCancel").on("click", function (e) {
        e.preventDefault();
        YarnIssueReturnbackToList();
    });

    $("#btnApprovedYIssue").click(function (e) {
        e.preventDefault();

        var data = { Id: $("#Id").val() };

        var config = { headers: { 'Content-Type': 'application/json' } };
        axios.post("/yissueApi/YarnIssueApprovedLists", data, config)
            .then(function () {
                toastr.success(constants.APPROVE_SUCCESSFULLY);
                $("#divNewYarnIssueReturn").fadeOut();
                $("#divtblYarnIssueReturn").fadeIn();
                $("#divYarnIssueReturnButtonExecutions").fadeOut();
                getPendingYarnIssueReturnMasterData();
            })
            .catch(showResponseError);
    });

    $("#btnUnApprovedYReq").click(function (e) {
        e.preventDefault();

        bootbox.prompt("Are you sure you want to Unapproved this?", function (result) {
            if (!result) {
                return toastr.error("Unapproved reason is required.");
            }

            var data = { Id: $("#Id").val() };
            data.UnapproveReason = result;

            var config = { headers: { 'Content-Type': 'application/json' } };
            axios.post("/yrApi/UnapproveYarnIssueReturnlist", data, config)
                .then(function () {
                    toastr.success(constants.APPROVE_SUCCESSFULLY);
                    $("#divNewYarnIssueReturn").fadeOut();
                    $("#divtblYarnIssueReturn").fadeIn();
                    $("#divYarnIssueReturnButtonExecutions").fadeOut();
                    getPendingYarnIssueReturnMasterData();
                })
                .catch(showResponseError);
        });
    });
});

function initPendingYarnIssueReturnMasterTable() {
    $("#tblYarnIssueReturnMaster").bootstrapTable('destroy');
    $("#tblYarnIssueReturnMaster").bootstrapTable({
        showRefresh: true,
        showExport: true,
        showColumns: true,
        toolbar: "#YarnIssueReturnToolbar",
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
                        if (yrStatus == 1) {
                            getYarnIssueReturnNew(row.Id);
                            $("#divNewYarnIssueReturn").fadeIn();
                            $("#divtblYarnIssueReturn").fadeOut();
                            $("#divYarnIssueReturnButtonExecutions").fadeIn();
                            $("#btnApprovedYReq").fadeOut();
                            $("#btnSaveYIssue").fadeIn();
                            $("#btnUnApprovedYReq").fadeIn();
                        }
                        else if (yrStatus == 2) {
                            $("#Id").val(row.Id);
                            getYarnIssueReturnEdit(row.Id);
                            $("#divNewYarnIssueReturn").fadeIn();
                            $("#divtblYarnIssueReturn").fadeOut();
                            $("#divYarnIssueReturnButtonExecutions").fadeIn();
                            $("#btnApprovedYIssue").fadeIn();
                            $("#btnSaveYIssue").fadeIn();
                            $("#btnUnApprovedYReq").fadeOut();
                        }
                        else {
                            $("#Id").val(row.Id);
                            getYarnIssueReturnEdit(row.Id);
                            $("#divNewYarnIssueReturn").fadeIn();
                            $("#divtblYarnIssueReturn").fadeOut();
                            $("#divYarnIssueReturnButtonExecutions").fadeIn();
                            $("#btnApprovedYIssue").fadeOut();
                            $("#btnSaveYIssue").fadeOut();
                        }
                    }
                }
            },
            {
                field: "ReqNo",
                title: "Requisition No",
                width: 250,
                filterControl: "input",
                visible: !isAllChecked,
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "ReqDateStr",
                title: "Requisition Date",
                width: 250,
                filterControl: "input",
                visible: !isAllChecked,
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "IssueNo",
                title: "Issue No",
                width: 250,
                filterControl: "input",
                visible: isAllChecked,
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "IssueDateStr",
                title: "Issue Date",
                width: 250,
                filterControl: "input",
                visible: isAllChecked,
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "ExportOrderNo",
                title: "Export Order No",
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "RequisitionBy",
                title: "Requisition By",
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

            getPendingYarnIssueReturnMasterData();
        },
        onSort: function (name, order) {
            tableParams.sort = name;
            tableParams.order = order;
            tableParams.offset = 0;

            getPendingYarnIssueReturnMasterData();
        },
        onRefresh: function () {
            resetTableParams();
            getPendingYarnIssueReturnMasterData();
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

            getPendingYarnIssueReturnMasterData();
        }
    });
}

function getPendingYarnIssueReturnMasterData() {
    var queryParams = $.param(tableParams);
    $('#tblYarnIssueReturnMaster').bootstrapTable('showLoading');
    var url = "/yissueApi/YarnIssueLists?yrStatus=" + yrStatus + "&" + queryParams;
    axios.get(url)
        .then(function (response) {
            $("#tblYarnIssueReturnMaster").bootstrapTable('load', response.data);
            $('#tblYarnIssueReturnMaster').bootstrapTable('hideLoading');
        })
        .catch(function (err) {
            toastr.error(err.response.data.Message);
        })
}

function getYarnIssueReturnNew(reqId) {
    url = "/yissueApi/getYarnIssueNew/" + reqId;

    axios.get(url)
        .then(function (response) {
            YarnIssueReturnMaster = response.data;
            $("#IssueNo2").val(response.data.IssueNo);
            $("#ReqID").val(response.data.ReqId);
            $("#ReqNo").val(response.data.ReqNo);
            $("#ReqDate").val(response.data.ReqDateStr);
            $("#ExportOrderID").val(response.data.ExportOrderId);
            $("#ExportOrderNo").val(response.data.ExportOrderNo);
            $("#ReqBy").val(response.data.ReqBy);
            $("#RequisitionBy").val(response.data.RequisitionBy);

            $("#LocationID").select2({ 'data': response.data.LocationList });

            initTblYarnIssueReturnChildItems();

            $("#tblYarnIssueReturnChilds").bootstrapTable('load', response.data.IssueChilds);
        })
        .catch(function (err) {
            toastr.error(err.response.data.Message);
        })
}

function getYarnIssueReturnEdit(id) {
    url = "/yissueApi/getYarnIssueEdit/" + id;

    axios.get(url)
        .then(function (response) {
            YarnIssueReturnMaster = response.data;
            $("#Id").val(response.data.Id);
            $("#ReqID").val(response.data.ReqId);
            $("#ReqNo").val(response.data.ReqNo);
            $("#ReqDate").val(response.data.ReqDateStr);
            $("#ExportOrderID").val(response.data.ExportOrderId);
            $("#ExportOrderNo").val(response.data.ExportOrderNo);
            $("#ReqBy").val(response.data.ReqBy);
            $("#RequisitionBy").val(response.data.RequisitionBy);

            $("#IssueNo").val(response.data.IssueNo);
            $("#IssueDate").val(response.data.IssueDateStr);

            $("#LocationID").select2({ 'data': response.data.LocationList });
            $("#LocationID").val(response.data.LocationId).trigger("change");

            initTblYarnIssueReturnChildItems();
            $("#tblYarnIssueReturnChilds").bootstrapTable('load', response.data.IssueChilds);
        })
        .catch(function (err) {
            toastr.error(err.response.data.Message);
        })
}

function initTblYarnIssueReturnChildItems() {
    $("#tblYarnIssueReturnChilds").bootstrapTable({
        showFooter: true,
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
                field: "DisplayUnitDesc",
                title: "Unit",
                width: 50
            },
            {
                field: "StockQty",
                title: "Stock Qty",
                align: 'center',
                width: 50
            },
            {
                field: "ReqQty",
                title: "Requisition Qty",
                align: 'center',
                width: 50
            },
            {
                field: "Remarks",
                title: "Remarks",
                align: 'center',
                width: 50
            },
            {
                field: "IssueQty",
                title: "Issue Qty",
                width: 30,
                align: 'center',
                editable: {
                    type: 'text',
                    inputclass: 'input-sm',
                    showbuttons: false
                }
            },
            {
                field: "IssueChildRemarks",
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
            if (row.IssueQty > row.ReqQty) {
                bootbox.alert({
                    message: "Issue Qty can't larger than Requisition Qty !!!!",
                    backdrop: true
                });
                row.IssueQty = 0;
            }

            $("#tblYarnIssueReturnChilds").bootstrapTable('load', YarnIssueReturnMaster.IssueChilds);
        }
    });
}

function YarnIssueReturnbackToList() {
    $("#divNewYarnIssueReturn").fadeOut();
    $("#divtblYarnIssueReturn").fadeIn();
    $("#divYarnIssueReturnButtonExecutions").fadeOut();
    getPendingYarnIssueReturnMasterData();
}

function resetTableParams() {
    tableParams.offset = 0;
    tableParams.limit = 10;
    tableParams.filter = '';
    tableParams.sort = '';
    tableParams.order = '';
}