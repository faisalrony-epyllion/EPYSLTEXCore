var formYarnProposed;
var yarnProposedMaster;
var filterBy = {};

var yPrposedStatus = 1;
var YarnProposedFrom = 0;
var YarnProposedChilds = [];
var isAllChecked = false;
var itemMasterId = 0;
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

var ProposedByfilterBy = {};
var ProposedByTableParams = {
    offset: 0,
    limit: 10,
    sort: '',
    order: '',
    filter: ''
}

$(function () {
    formYarnProposed = $("#formYarnProposed");

    $("#btnPendingProposed").css('background', '#008080');
    $("#btnPendingProposed").css('color', '#FFFFFF');

    formYarnProposed.find("#ProposedFrom").on("select2:select", function (e) {
        YarnProposedFrom = e.params.data.ProposedID;
    });

    initPendingYarnProposedMasterTable();
    getPendingYarnProposedMasterData();
    initExportOrderListsTable();
    initProposedByUsersTable();
    initTblYarnProposedChildItems();

    $("#btnYProposedNew").on("click", function (e) {
        e.preventDefault();
        YarnProposedresetForm();

        var today = new Date();
        var datetoday = (today.getMonth() + 1) + '/' + today.getDate() + '/' + today.getFullYear();
        formYarnProposed.find("#ProposedDate").val(datetoday);

        $("#divNewYarnProposed").fadeIn();
        $("#divtblYarnProposed").fadeOut();
        $("#divYarnProposedButtonExecutions").fadeIn();

        $("#btnApprovedYProposed").fadeOut();
        $("#btnUnApprovedYProposed").fadeOut();

        getNewYarnProposedData();

        $("#btnYProposedNew").css('background', '#008080');
        $("#btnYProposedNew").css('color', '#FFFFFF');

        $("#btnPendingProposed").css('background', '#FFFFFF');
        $("#btnPendingProposed").css('color', '#000000');

        $("#btnProposedLists").css('background', '#FFFFFF');
        $("#btnProposedLists").css('color', '#000000');

        $("#btnProposedUnApproved").css('background', '#FFFFFF');
        $("#btnProposedUnApproved").css('color', '#000000');
    });

    $("#btnPendingProposed").on("click", function (e) {
        e.preventDefault();
        yPrposedStatus = 1;
        resetTableParams();
        initPendingYarnProposedMasterTable();
        getPendingYarnProposedMasterData();

        $("#btnPendingProposed").css('background', '#008080');
        $("#btnPendingProposed").css('color', '#FFFFFF');

        $("#btnYProposedNew").css('background', '#FFFFFF');
        $("#btnYProposedNew").css('color', '#000000');

        $("#btnProposedLists").css('background', '#FFFFFF');
        $("#btnProposedLists").css('color', '#000000');

        $("#btnProposedUnApproved").css('background', '#FFFFFF');
        $("#btnProposedUnApproved").css('color', '#000000');
    });

    $("#btnProposedLists").on("click", function (e) {
        e.preventDefault();
        yPrposedStatus = 2;
        resetTableParams();
        initPendingYarnProposedMasterTable();
        getPendingYarnProposedMasterData();

        $("#btnProposedLists").css('background', '#008080');
        $("#btnProposedLists").css('color', '#FFFFFF');

        $("#btnYProposedNew").css('background', '#FFFFFF');
        $("#btnYProposedNew").css('color', '#000000');

        $("#btnPendingProposed").css('background', '#FFFFFF');
        $("#btnPendingProposed").css('color', '#000000');

        $("#btnProposedUnApproved").css('background', '#FFFFFF');
        $("#btnProposedUnApproved").css('color', '#000000');
    });

    $("#btnProposedUnApproved").on("click", function (e) {
        e.preventDefault();
        yPrposedStatus = 3;
        resetTableParams();
        initPendingYarnProposedMasterTable();
        getPendingYarnProposedMasterData();

        $("#btnProposedUnApproved").css('background', '#008080');
        $("#btnProposedUnApproved").css('color', '#FFFFFF');

        $("#btnYProposedNew").css('background', '#FFFFFF');
        $("#btnYProposedNew").css('color', '#000000');

        $("#btnPendingProposed").css('background', '#FFFFFF');
        $("#btnPendingProposed").css('color', '#000000');

        $("#btnProposedLists").css('background', '#FFFFFF');
        $("#btnProposedLists").css('color', '#000000');
    });

    $("#btnYarnProposedAddOrders").on("click", function (e) {
        e.preventDefault();

        if (formYarnProposed.find("#ProposedFrom").val() < 1) {
            bootbox.alert({
                size: "small",
                title: "Alert !!!",
                message: "Select Yarn Proposed From !!!!",
                callback: function () {
                }
            });
        }
        else {
            getExportOrdersFromBuyerCompany();
            $("#modal-child").modal('show');
        }
    });

    $("#btnAddProposedBy").on("click", function (e) {
        e.preventDefault();
        $("#modal-child-").modal('show');
        getProposedByUsers();
    });

    $("#btnSaveYProposed").click(function (e) {
        e.preventDefault();
        var data = formDataToJson(formYarnProposed.serializeArray());
        data["YarnProposedChilds"] = YarnProposedChilds;

        var config = { headers: { 'Content-Type': 'application/json' } };
        axios.post("/yProposedApi/yarnProposedSave", data, config)
            .then(function () {
                toastr.success("Your Proposed saved successfully." + " Proposed No: " + $("#ProposedNo2").val());
                YarnProposedbackToList();
            })
            .catch(showResponseError);
    });

    $("#btnYProposedEditCancel").on("click", function (e) {
        e.preventDefault();
        YarnProposedbackToList();
    });

    $("#btnApprovedYProposed").click(function (e) {
        e.preventDefault();

        var data = { ProposedID: $("#ProposedID").val() };

        var config = { headers: { 'Content-Type': 'application/json' } };
        axios.post("/yProposedApi/YarnProposedApprovedLists", data, config)
            .then(function () {
                toastr.success(constants.APPROVE_SUCCESSFULLY);
                $("#divNewYarnProposed").fadeOut();
                $("#divtblYarnProposed").fadeIn();
                $("#divYarnProposedButtonExecutions").fadeOut();
                getPendingYarnProposedMasterData();
            })
            .catch(showResponseError);
    });

    $("#btnUnApprovedYProposed").click(function (e) {
        e.preventDefault();

        bootbox.prompt("Are you sure you want to Unapproved this?", function (result) {
            if (!result) {
                return toastr.error("Unapproved reason is required.");
            }

            var data = { ProposedID: $("#ProposedID").val() };
            data.UnapproveReason = result;

            var config = { headers: { 'Content-Type': 'application/json' } };
            axios.post("/yProposedApi/UnapproveYarnProposedlist", data, config)
                .then(function () {
                    toastr.success(constants.APPROVE_SUCCESSFULLY);
                    formYarnProposed.find("#divNewYarnProposed").fadeOut();
                    formYarnProposed.find("#divtblYarnProposed").fadeIn();
                    formYarnProposed.find("#divYarnProposedButtonExecutions").fadeOut();
                    getPendingYarnProposedMasterData();
                })
                .catch(showResponseError);
        });
    });
});

function getNewYarnProposedData() {
    url = "/yProposedApi/NewYarnProposed/";

    axios.get(url)
        .then(function (response) {
            yarnProposedMaster = response.data;
            formYarnProposed.find("#ProposedNo2").val(response.data.ProposedNo);
            initSelect2(formYarnProposed.find("#ProposedFrom"), response.data.YarnProposedFromList);

            initTblYarnProposedChildItems();
            formYarnProposed.find("#tblYarnProposedChildsNew").bootstrapTable('load', response.data);
        })
        .catch(function (err) {
            toastr.error(err.response.data.Message);
        })
}

function getProposedByUsers() {
    var queryParams = $.param(ProposedByTableParams);
    url = "/yProposedApi/getUsers" + "?" + queryParams;
    axios.get(url)
        .then(function (response) {
            $("#tblProposedByUserLists").bootstrapTable('load', response.data);
        })
        .catch(function () {
            toastr.error(err.response.data.Message);
        })
};

function getYarnChildItems(exportOrderId) {
    axios.get("/yProposedApi/yarnChildItems/" + exportOrderId)
        .then(function (response) {
            YarnProposedChilds = response.data;
            formYarnProposed.find("#ProposedID").val(response.data.ProposedID);
            initTblYarnProposedChildItems();
            formYarnProposed.find("#tblYarnProposedChildsNew").bootstrapTable('load', response.data);
        })
        .catch(function (err) {
            toastr.error(err.response.data.Message);
        })
}

function initPendingYarnProposedMasterTable() {
    $("#tblYarnProposedMaster").bootstrapTable('destroy');
    $("#tblYarnProposedMaster").bootstrapTable({
        showRefresh: true,
        showExport: true,
        showColumns: true,
        toolbar: "#YarnProposedToolbar",
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
                        if (yPrposedStatus == 1) {
                            $("#ProposedID").val(row.ProposedID);
                            getYarnProposedEdit(row.ProposedID);
                            $("#divNewYarnProposed").fadeIn();
                            $("#divtblYarnProposed").fadeOut();
                            $("#divYarnProposedButtonExecutions").fadeIn();
                            $("#btnYarnProposedAddOrders").fadeOut();
                            $("#btnAddProposedBy").fadeOut();
                            $("#btnApprovedYProposed").fadeIn();
                            $("#btnSaveYProposed").fadeIn();
                            $("#btnUnApprovedYProposed").fadeIn();
                        }
                        else if (yPrposedStatus == 2) {
                            $("#ProposedID").val(row.ProposedID);
                            getYarnProposedEdit(row.ProposedID);
                            $("#divNewYarnProposed").fadeIn();
                            $("#divtblYarnProposed").fadeOut();
                            $("#divYarnProposedButtonExecutions").fadeIn();
                            $("#btnYarnProposedAddOrders").fadeOut();
                            $("#btnAddProposedBy").fadeOut();
                            $("#btnApprovedYProposed").fadeOut();
                            $("#btnSaveYProposed").fadeOut();
                            $("#btnUnApprovedYProposed").fadeOut();
                        }
                        else if (yPrposedStatus == 3) {
                            $("#ProposedID").val(row.ProposedID);
                            getYarnProposedEdit(row.ProposedID);
                            $("#divNewYarnProposed").fadeIn();
                            $("#divtblYarnProposed").fadeOut();
                            $("#divYarnProposedButtonExecutions").fadeIn();
                            $("#btnYarnProposedAddOrders").fadeOut();
                            $("#btnAddProposedBy").fadeOut();
                            $("#btnApprovedYProposed").fadeOut();
                            $("#btnSaveYProposed").fadeOut();
                            $("#btnUnApprovedYProposed").fadeOut();
                        }
                    }
                }
            },
            {
                field: "ProposedFromName",
                title: "Proposed From",
                width: 250,
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "ProposedNo",
                title: "Proposed No",
                width: 250,
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "ProposedDateStr",
                title: "Proposed Date",
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
                field: "ProposedBy",
                title: "Proposed By",
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

            getPendingYarnProposedMasterData();
        },
        onSort: function (name, order) {
            tableParams.sort = name;
            tableParams.order = order;
            tableParams.offset = 0;

            getPendingYarnProposedMasterData();
        },
        onRefresh: function () {
            resetTableParams();
            getPendingYarnProposedMasterData();
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

            getPendingYarnProposedMasterData();
        }
    });
}

function getPendingYarnProposedMasterData() {
    var queryParams = $.param(tableParams);
    $('#tblYarnProposedMaster').bootstrapTable('showLoading');
    var url = "/yProposedApi/YarnProposedLists?yPrposedStatus=" + yPrposedStatus + "&" + queryParams;
    axios.get(url)
        .then(function (response) {
            $("#tblYarnProposedMaster").bootstrapTable('load', response.data);
            $('#tblYarnProposedMaster').bootstrapTable('hideLoading');
        })
        .catch(function (err) {
            toastr.error(err.response.data.Message);
        })
}

function getYarnProposedEdit(id) {
    url = "/yProposedApi/getYarnProposedEdit/" + id;

    axios.get(url)
        .then(function (response) {
            yarnProposedMaster = response.data;
            $("#ProposedID").val(response.data.ProposedID);
            $("#ProposedNo").val(response.data.ProposedNo);
            $("#ProposedDate").val(response.data.ProposedDateStr);
            $("#ExportOrderID").val(response.data.ExportOrderId);
            $("#ExportOrderNo").val(response.data.ExportOrderNo);
            $("#ProposedBy").val(response.data.ProposedBy);
            $("#ProposedByName").val(response.data.ProposedByName);
            initSelect2(formYarnProposed.find("#ProposedFrom"), response.data.YarnProposedFromList);
            formYarnProposed.find("#ProposedFrom").val(response.data.ProposedFrom).trigger("change");

            initTblYarnProposedChildItems();

            formYarnProposed.find("#tblYarnProposedChildsNew").bootstrapTable('load', response.data.YarnProposedChilds);
        })
        .catch(function (err) {
            toastr.error(err.response.data.Message);
        })
}

function initTblYarnProposedChildItems() {
    formYarnProposed.find("#tblYarnProposedChildsNew").bootstrapTable({
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
                field: "ProposedQty",
                title: "Proposed Qty",
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

function YarnProposedbackToList() {
    $("#divNewYarnProposed").fadeOut();
    $("#divtblYarnProposed").fadeIn();
    $("#divYarnProposedButtonExecutions").fadeOut();
    getPendingYarnProposedMasterData();
}

function YarnProposedresetForm() {
    formYarnProposed.trigger("reset");
    formYarnProposed.find("#ProposedID").val(-1111);
    formYarnProposed.find("#EntityState").val(4);
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
    if (YarnProposedFrom == 1) {
        url = "/yProposedApi/YarnPOExportOrderListsStockQty" + "?" + queryParams;
    }
    else {
        url = "/yProposedApi/YarnPOExportOrderLists" + "?" + queryParams;
    }
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
                title: "Stock Qty",
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            }
        ],
        onDblClickRow: function (row, $element, field) {
            $("#modal-child").modal('hide');
            $("#ExportOrderID").val(row.ExportOrderId);
            $("#ExportOrderNo").val(row.ExportOrderNo);
            $("#YPOMasterId").val(row.YPOMasterID);
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

function initProposedByUsersTable() {
    $("#tblProposedByUserLists").bootstrapTable('destroy');
    $("#tblProposedByUserLists").bootstrapTable({
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
            $("#ProposedBy").val(row.UserCode);
            $("#ProposedByName").val(row.EmployeeName);
        },
        onPageChange: function (number, size) {
            var newOffset = (number - 1) * size;
            var newLimit = size;
            if (ProposedByTableParams.offset == newOffset && ProposedByTableParams.limit == newLimit)
                return;

            ProposedByTableParams.offset = newOffset;
            ProposedByTableParams.limit = newLimit;
            getProposedByUsers();
        },
        onSort: function (name, order) {
            ProposedByTableParams.sort = name;
            ProposedByTableParams.order = order;
            ProposedByTableParams.offset = 0;

            getProposedByUsers();
        },
        onRefresh: function () {
            resetTableParams();
            getProposedByUsers();
        },
        onColumnSearch: function (columnName, filterValue) {
            if (columnName in ProposedByfilterBy && !filterValue) {
                delete ProposedByfilterBy[columnName];
            }
            else
                ProposedByfilterBy[columnName] = filterValue;

            if (Object.keys(ProposedByfilterBy).length === 0 && ProposedByfilterBy.constructor === Object)
                ProposedByTableParams.filter = "";
            else
                ProposedByTableParams.filter = JSON.stringify(ProposedByfilterBy);

            getProposedByUsers();
        }
    });
}