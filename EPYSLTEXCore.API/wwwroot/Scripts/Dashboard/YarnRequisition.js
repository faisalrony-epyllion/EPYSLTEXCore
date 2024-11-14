var formYarnRequisition;
var yarnRequisitionMaster;
var filterBy = {};
var ReqID;
var yrStatus = 1;
var RequisitionChilds = [];
var isAllChecked = false;
var tableParams = {
    offset: 0,
    limit: 10,
    sort: '',
    order: '',
    filter: ''
}
var ExportOrderfilterBy = {};
var ExportOrderTableParams = {
    offset: 0,
    limit: 10,
    sort: '',
    order: '',
    filter: ''
}

var RequisitionByfilterBy = {};
var RequisitionByTableParams = {
    offset: 0,
    limit: 10,
    sort: '',
    order: '',
    filter: ''
}

$(function () {
    formYarnRequisition = $("#formYarnRequisition");

    $("#btnPendingRequisition").css('background', '#008080');
    $("#btnPendingRequisition").css('color', '#FFFFFF');

    initPendingYarnRequisitionMasterTable();
    getPendingYarnRequisitionMasterData();
    initExportOrderListsTable();
    initRequisitionByUsersTable();

    $("#btnYReqNew").on("click", function (e) {
        e.preventDefault();
        YarnRequisitionresetForm();

        $("#divNewYarnRequisition").fadeIn();
        $("#divtblYarnRequisition").fadeOut();
        $("#divYarnRequisitionButtonExecutions").fadeIn();

        $("#btnApprovedYReq").fadeOut();
        $("#btnUnApprovedYReq").fadeOut();

        getNewYarnRequisitionData();

        $("#btnYReqNew").css('background', '#008080');
        $("#btnYReqNew").css('color', '#FFFFFF');

        $("#btnPendingRequisition").css('background', '#FFFFFF');
        $("#btnPendingRequisition").css('color', '#000000');

        $("#btnRequisitionLists").css('background', '#FFFFFF');
        $("#btnRequisitionLists").css('color', '#000000');

        $("#btnRequisitionUnApproved").css('background', '#FFFFFF');
        $("#btnRequisitionUnApproved").css('color', '#000000');
    });

    $("#btnPendingRequisition").on("click", function (e) {
        e.preventDefault();
        yrStatus = 1;
        resetTableParams();
        initPendingYarnRequisitionMasterTable();
        getPendingYarnRequisitionMasterData();

        $("#btnPendingRequisition").css('background', '#008080');
        $("#btnPendingRequisition").css('color', '#FFFFFF');

        $("#btnYReqNew").css('background', '#FFFFFF');
        $("#btnYReqNew").css('color', '#000000');

        $("#btnRequisitionLists").css('background', '#FFFFFF');
        $("#btnRequisitionLists").css('color', '#000000');

        $("#btnRequisitionUnApproved").css('background', '#FFFFFF');
        $("#btnRequisitionUnApproved").css('color', '#000000');
    });

    $("#btnRequisitionLists").on("click", function (e) {
        e.preventDefault();
        yrStatus = 2;
        resetTableParams();
        initPendingYarnRequisitionMasterTable();
        getPendingYarnRequisitionMasterData();

        $("#btnRequisitionLists").css('background', '#008080');
        $("#btnRequisitionLists").css('color', '#FFFFFF');

        $("#btnYReqNew").css('background', '#FFFFFF');
        $("#btnYReqNew").css('color', '#000000');

        $("#btnPendingRequisition").css('background', '#FFFFFF');
        $("#btnPendingRequisition").css('color', '#000000');

        $("#btnRequisitionUnApproved").css('background', '#FFFFFF');
        $("#btnRequisitionUnApproved").css('color', '#000000');
    });

    $("#btnRequisitionUnApproved").on("click", function (e) {
        e.preventDefault();
        yrStatus = 3;
        resetTableParams();
        initPendingYarnRequisitionMasterTable();
        getPendingYarnRequisitionMasterData();

        $("#btnRequisitionUnApproved").css('background', '#008080');
        $("#btnRequisitionUnApproved").css('color', '#FFFFFF');

        $("#btnYReqNew").css('background', '#FFFFFF');
        $("#btnYReqNew").css('color', '#000000');

        $("#btnPendingRequisition").css('background', '#FFFFFF');
        $("#btnPendingRequisition").css('color', '#000000');

        $("#btnRequisitionLists").css('background', '#FFFFFF');
        $("#btnRequisitionLists").css('color', '#000000');
    });

    $("#btnAddOrders").on("click", function (e) {
        e.preventDefault();
        getExportOrdersFromBuyerCompany();
        $("#modal-child").modal('show');
    });

    $("#btnAddRequisitionBy").on("click", function (e) {
        e.preventDefault();
        $("#modal-child-").modal('show');
        getRequisitionByUsers();
    });

    $("#btnSaveYReq").click(function (e) {
        e.preventDefault();
        var data = formDataToJson(formYarnRequisition.serializeArray());
        data["RequisitionChilds"] = RequisitionChilds;

        var config = { headers: { 'Content-Type': 'application/json' } };
        axios.post("/yrApi/yarnRequisitionSave", data, config)
            .then(function () {
                toastr.success("Your Requisition saved successfully." + " Requisition No: " + $("#ReqNo2").val());
                YarnRequisitionbackToList();
            })
            .catch(showResponseError);
    });

    $("#btnYReqEditCancel").on("click", function (e) {
        e.preventDefault();
        YarnRequisitionbackToList();
    });

    $("#btnApprovedYReq").click(function (e) {
        e.preventDefault();

        var data = { ReqID: $("#ReqID").val() };

        var config = { headers: { 'Content-Type': 'application/json' } };
        axios.post("/yrApi/YarnRequisitionApprovedLists", data, config)
            .then(function () {
                toastr.success(constants.APPROVE_SUCCESSFULLY);
                $("#divNewYarnRequisition").fadeOut();
                $("#divtblYarnRequisition").fadeIn();
                $("#divYarnRequisitionButtonExecutions").fadeOut();
                getPendingYarnRequisitionMasterData();
            })
            .catch(showResponseError);
    });

    $("#btnUnApprovedYReq").click(function (e) {
        e.preventDefault();

        bootbox.prompt("Are you sure you want to Unapproved this?", function (result) {
            if (!result) {
                return toastr.error("Unapproved reason is required.");
            }

            var data = { ReqID: $("#ReqID").val() };
            data.UnapproveReason = result;

            var config = { headers: { 'Content-Type': 'application/json' } };
            axios.post("/yrApi/UnapproveYarnRequisitionlist", data, config)
                .then(function () {
                    toastr.success(constants.APPROVE_SUCCESSFULLY);
                    formYarnRequisition.find("#divNewYarnRequisition").fadeOut();
                    formYarnRequisition.find("#divtblYarnRequisition").fadeIn();
                    formYarnRequisition.find("#divYarnRequisitionButtonExecutions").fadeOut();
                    getPendingYarnRequisitionMasterData();
                })
                .catch(showResponseError);
        });
    });
});

function getNewYarnRequisitionData() {
    url = "/yrApi/NewYarnRequisition/";

    axios.get(url)
        .then(function (response) {
            yarnRequisitionMaster = response.data;
            formYarnRequisition.find("#ReqNo2").val(response.data.ReqNo);
            //formYarnRequisition.find("#tblyarnRequisitionChilds").bootstrapTable('load', response.data.RequisitionChilds);
        })
        .catch(function (err) {
            toastr.error(err.response.data.Message);
        })
}

function getRequisitionByUsers() {
    var queryParams = $.param(RequisitionByTableParams);
    url = "/yrApi/getUsers" + "?" + queryParams;
    axios.get(url)
        .then(function (response) {
            $("#tblRequisitionByUserLists").bootstrapTable('load', response.data);
        })
        .catch(function () {
            toastr.error(err.response.data.Message);
        })
};

function getYarnChildItems(exportOrderId) {
    axios.get("/yrApi/yarnChildItems/" + exportOrderId)
        .then(function (response) {
            RequisitionChilds = response.data;
            formYarnRequisition.find("#ReqID").val(response.data.ReqID);

            initTblYarnRequisitionChildItems();

            formYarnRequisition.find("#tblyarnRequisitionChilds").bootstrapTable('load', response.data);
        })
        .catch(function (err) {
            toastr.error(err.response.data.Message);
        })
}

function initPendingYarnRequisitionMasterTable() {
    $("#tblYarnRequisitionMaster").bootstrapTable('destroy');
    $("#tblYarnRequisitionMaster").bootstrapTable({
        showRefresh: true,
        showExport: true,
        showColumns: true,
        toolbar: "#YarnRequisitionToolbar",
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
                            $("#ReqID").val(row.IReqIDd);
                            getYarnRequisitionEdit(row.ReqID);
                            $("#divNewYarnRequisition").fadeIn();
                            $("#divtblYarnRequisition").fadeOut();
                            $("#divYarnRequisitionButtonExecutions").fadeIn();
                            $("#btnAddOrders").fadeOut();
                            $("#btnAddRequisitionBy").fadeOut();
                            $("#btnApprovedYReq").fadeIn();
                            $("#btnSaveYReq").fadeIn();
                            $("#btnUnApprovedYReq").fadeIn();
                        }
                        else if (yrStatus == 2) {
                            $("#ReqID").val(row.ReqID);
                            getYarnRequisitionEdit(row.ReqID);
                            $("#divNewYarnRequisition").fadeIn();
                            $("#divtblYarnRequisition").fadeOut();
                            $("#divYarnRequisitionButtonExecutions").fadeIn();
                            $("#btnAddOrders").fadeOut();
                            $("#btnAddRequisitionBy").fadeOut();
                            $("#btnApprovedYReq").fadeOut();
                            $("#btnSaveYReq").fadeOut();
                            $("#btnUnApprovedYReq").fadeOut();
                        }
                        else if (yrStatus == 3) {
                            $("#ReqID").val(row.ReqID);
                            getYarnRequisitionEdit(row.ReqID);
                            $("#divNewYarnRequisition").fadeIn();
                            $("#divtblYarnRequisition").fadeOut();
                            $("#divYarnRequisitionButtonExecutions").fadeIn();
                            $("#btnAddOrders").fadeOut();
                            $("#btnAddRequisitionBy").fadeOut();
                            $("#btnApprovedYReq").fadeOut();
                            $("#btnSaveYReq").fadeOut();
                            $("#btnUnApprovedYReq").fadeOut();
                        }
                    }
                }
            },
            {
                field: "ReqNo",
                title: "Requisition No",
                width: 250,
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "ReqDateStr",
                title: "Requisition Date",
                width: 250,
                filterControl: "input",
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

            getPendingYarnRequisitionMasterData();
        },
        onSort: function (name, order) {
            tableParams.sort = name;
            tableParams.order = order;
            tableParams.offset = 0;

            getPendingYarnRequisitionMasterData();
        },
        onRefresh: function () {
            resetTableParams();
            getPendingYarnRequisitionMasterData();
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

            getPendingYarnRequisitionMasterData();
        }
    });
}

function getPendingYarnRequisitionMasterData() {
    var queryParams = $.param(tableParams);
    $('#tblYarnRequisitionMaster').bootstrapTable('showLoading');
    var url = "/yrApi/YarnRequisitionLists?yrStatus=" + yrStatus + "&" + queryParams;
    axios.get(url)
        .then(function (response) {
            $("#tblYarnRequisitionMaster").bootstrapTable('load', response.data);
            $('#tblYarnRequisitionMaster').bootstrapTable('hideLoading');
        })
        .catch(function (err) {
            toastr.error(err.response.data.Message);
        })
}

function getYarnRequisitionEdit(id) {
    url = "/yrApi/getYarnRequisitionEdit/" + id;

    axios.get(url)
        .then(function (response) {
            yarnRequisitionMaster = response.data;
            $("#ReqID").val(response.data.ReqID);
            $("#ReqNo").val(response.data.ReqNo);
            $("#ReqDate").val(response.data.ReqDateStr);
            $("#ExportOrderID").val(response.data.ExportOrderId);
            $("#ExportOrderNo").val(response.data.ExportOrderNo);
            $("#ReqBy").val(response.data.ReqBy);
            $("#RequisitionBy").val(response.data.RequisitionBy);

            initTblYarnRequisitionChildItems();

            $("#tblyarnRequisitionChilds").bootstrapTable('load', response.data.RequisitionChilds);
        })
        .catch(function (err) {
            toastr.error(err.response.data.Message);
        })
}

function initTblYarnRequisitionChildItems() {
    $("#tblyarnRequisitionChilds").bootstrapTable({
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
                width: 30,
                align: 'center',
                editable: {
                    type: 'text',
                    inputclass: 'input-sm',
                    showbuttons: false
                }
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
        ]
    });
}

function YarnRequisitionbackToList() {
    $("#divNewYarnRequisition").fadeOut();
    $("#divtblYarnRequisition").fadeIn();
    $("#divYarnRequisitionButtonExecutions").fadeOut();
    //getPendingYarnRequisitionMasterData();
}

function YarnRequisitionresetForm() {
    formYarnRequisition.trigger("reset");
    formYarnRequisition.find("#ReqID").val(-1111);
    formYarnRequisition.find("#EntityState").val(4);
}

function resetTableParams() {
    tableParams.offset = 0;
    tableParams.limit = 10;
    tableParams.filter = '';
    tableParams.sort = '';
    tableParams.order = '';
}

function getExportOrdersFromBuyerCompany() {
    var queryParams = $.param(ExportOrderTableParams);
    url = "/yrApi/YarnPOExportOrderLists" + "?" + queryParams;
    axios.get(url)
        .then(function (response) {
            exportOrderLists = response.data;
            $("#tblExportOrderLists").bootstrapTable('load', response.data);
        })
        .catch(function () {
            toastr.error(err.response.data.Message);
        })
};

function initExportOrderListsTable() {
    $("#tblExportOrderLists").bootstrapTable('destroy');
    $("#tblExportOrderLists").bootstrapTable({
        pagination: true,
        filterControl: true,
        searchOnEnterKey: true,
        sidePagination: "server",
        pageList: "[10, 25, 50, 100, 500]",
        cache: false,
        columns: [
            {
                field: "ExportOrderNo",
                title: "Export Order No",
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "BuyerName",
                title: "Buyer Name",
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "BuyerTeam",
                title: "Buyer Team",
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "StyleNo",
                title: "Style No",
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "ReceiveQty",
                title: "Receive Qty",
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            }
        ],
        onDblClickRow: function (row, $element, field) {
            $("#modal-child").modal('hide');
            $("#ExportOrderID").val(row.ExportOrderId);
            $("#ExportOrderNo").val(row.ExportOrderNo);
            getYarnChildItems(row.ExportOrderId);
        },
        onPageChange: function (number, size) {
            var newOffset = (number - 1) * size;
            var newLimit = size;
            if (ExportOrderTableParams.offset == newOffset && ExportOrderTableParams.limit == newLimit)
                return;

            ExportOrderTableParams.offset = newOffset;
            ExportOrderTableParams.limit = newLimit;
            getExportOrdersFromBuyerCompany();
        },
        onSort: function (name, order) {
            ExportOrderTableParams.sort = name;
            ExportOrderTableParams.order = order;
            ExportOrderTableParams.offset = 0;

            getExportOrdersFromBuyerCompany();
        },
        onRefresh: function () {
            resetTableParams();
            getExportOrdersFromBuyerCompany();
        },
        onColumnSearch: function (columnName, filterValue) {
            if (columnName in ExportOrderfilterBy && !filterValue) {
                delete ExportOrderfilterBy[columnName];
            }
            else
                ExportOrderfilterBy[columnName] = filterValue;

            if (Object.keys(ExportOrderfilterBy).length === 0 && ExportOrderfilterBy.constructor === Object)
                ExportOrderTableParams.filter = "";
            else
                ExportOrderTableParams.filter = JSON.stringify(ExportOrderfilterBy);

            getExportOrdersFromBuyerCompany();
        }
    });
}

function initRequisitionByUsersTable() {
    $("#tblRequisitionByUserLists").bootstrapTable('destroy');
    $("#tblRequisitionByUserLists").bootstrapTable({
        pagination: true,
        filterControl: true,
        searchOnEnterKey: true,
        sidePagination: "server",
        pageList: "[10, 25, 50, 100, 500]",
        cache: false,
        columns: [
            {
                field: "UserCode",
                title: "User Code",
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "UserName",
                title: "User Name",
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "EmployeeName",
                title: "Employee Name",
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "DepertmentDescription",
                title: "Description",
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "Designation",
                title: "Designation",
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            }
        ],
        onDblClickRow: function (row, $element, field) {
            $("#modal-child-").modal('hide');
            $("#ReqBy").val(row.UserCode);
            $("#RequisitionBy").val(row.EmployeeName);
        },
        onPageChange: function (number, size) {
            var newOffset = (number - 1) * size;
            var newLimit = size;
            if (RequisitionByTableParams.offset == newOffset && RequisitionByTableParams.limit == newLimit)
                return;

            RequisitionByTableParams.offset = newOffset;
            RequisitionByTableParams.limit = newLimit;
            getRequisitionByUsers();
        },
        onSort: function (name, order) {
            RequisitionByTableParams.sort = name;
            RequisitionByTableParams.order = order;
            RequisitionByTableParams.offset = 0;

            getRequisitionByUsers();
        },
        onRefresh: function () {
            resetTableParams();
            getRequisitionByUsers();
        },
        onColumnSearch: function (columnName, filterValue) {
            if (columnName in RequisitionByfilterBy && !filterValue) {
                delete RequisitionByfilterBy[columnName];
            }
            else
                RequisitionByfilterBy[columnName] = filterValue;

            if (Object.keys(RequisitionByfilterBy).length === 0 && RequisitionByfilterBy.constructor === Object)
                RequisitionByTableParams.filter = "";
            else
                RequisitionByTableParams.filter = JSON.stringify(RequisitionByfilterBy);

            getRequisitionByUsers();
        }
    });
}