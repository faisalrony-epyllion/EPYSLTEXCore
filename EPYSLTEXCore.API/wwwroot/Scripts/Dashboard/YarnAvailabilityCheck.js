var formYarnAvailabilityCheckMaster;
var status = 5;
var filterBy = {};
var ShadeID2;
var tableParams = {
    offset: 0,
    limit: 10,
    sort: '',
    order: '',
    filter: ''
}
var isPendingItem = false;
var yarnProposedList = [];
var yarnColorList = [];
var yarnCountList = [];
var fabricTechnicalNameList = [];
var dyeingMCNameList = [];
var finishingMCNameList = [];
var processMasterList = [];
var constructionChildList = [];

var childEl;
var childSaveData = [];
var isAllChecked = false;
var isCustomChecked = false;
var isCustomCheckedYarnColor = false;
var isCustomCheckedYarnCount = false;
var isCustomCheckedYarnCount2 = false;
var isLaterCustomChecked = false;
var isFMTDDateChecked = false;
var bookingChildId;
var constructionId;
var fabricGsm;
var fabricWidth;
var shadeId;

var YarnAvailabilityCheckChildList = [];

$(function () {
    formYarnAvailabilityCheckMaster = $("#formYarnAvailabilityCheckMaster");

    $("#btnYACheckPending").css('background', '#008080');
    $("#btnYACheckPending").css('color', '#FFFFFF');

    initMasterTable();
    getYarnAvailabilityCheckData();
    getYarnProposedFrom();
    isFMTDDateChecked = false;

    $("#btnSaveYarnAvailabilityCheck").click(function (e) {
        e.preventDefault();
        var data = formDataToJson(formYarnAvailabilityCheckMaster.serializeArray());
        data.Id = 0;
        data["YarnAvailabilityCheckChilds"] = YarnAvailabilityCheckChildList;

        var config = { headers: { 'Content-Type': 'application/json' } };
        axios.post("/yacapi/yarnAvailabilityCheck", data, config)
            .then(function () {
                toastr.success("Your finishing processes saved successfully.");
                backToListYACheck();
            })
            .catch(showResponseError);
    });

    $("#btnProposeYarnAvailabilityCheck").click(function (e) {
        e.preventDefault();
        var data = {};
        data.BAnalysisId = $("#Id").val();
        data.TProcessMasterId = processMasterList[0].TProcessMasterId;

        var config = { headers: { 'Content-Type': 'application/json' } };
        axios.post("/api/proposeYarnAvailabilityCheckprocess", data, config)
            .then(function () {
                toastr.success(constants.PROPOSE_SUCCESSFULLY);
                backToListYACheck();
            })
            .catch(showResponseError);
    });

    $("#btnApprovedYarnAvailabilityCheck").click(function (e) {
        e.preventDefault();
        var data = {};
        data.BAnalysisId = $("#Id").val();
        data.TProcessMasterId = processMasterList[0].TProcessMasterId;

        var config = { headers: { 'Content-Type': 'application/json' } };
        axios.post("/api/approveYarnAvailabilityCheckprocess", data, config)
            .then(function () {
                toastr.success(constants.APPROVE_SUCCESSFULLY);
                backToListYACheck();
            })
            .catch(showResponseError);
    });

    $("#btnRejectYarnAvailabilityCheck").click(function (e) {
        e.preventDefault();

        bootbox.prompt("Are you sure you want to reject this?", function (result) {
            if (!result) {
                return toastr.error("Reject reason is required.");
            }

            var data = {};
            data.BAnalysisId = $("#Id").val();
            data.TProcessMasterId = processMasterList[0].TProcessMasterId;
            data.RejectReason = result;

            var config = { headers: { 'Content-Type': 'application/json' } };
            axios.post("/api/rejectYarnAvailabilityCheckprocess", data, config)
                .then(function () {
                    toastr.success(constants.REJECT_SUCCESSFULLY);
                    backToListYACheck();
                })
                .catch(showResponseError);
        });
    });

    $("#btnAcknowledgeYarnAvailabilityCheck").click(function (e) {
        e.preventDefault();

        var data = {};
        data.BAnalysisId = $("#Id").val();
        data.TProcessMasterId = processMasterList[0].TProcessMasterId;

        var config = { headers: { 'Content-Type': 'application/json' } };
        axios.post("/api/acknowledgeYarnAvailabilityCheckprocess", data, config)
            .then(function () {
                toastr.success(constants.SUCCESS_MESSAGE);
                backToListYACheck();
                $("#btnAcknowledgeYarnAvailabilityCheck").hasClass('active');
            })
            .catch(showResponseError);
    });

    $("#btnYACheckEditCancel").on("click", function (e) {
        e.preventDefault();
        backToListYACheck();
    });

    $("#btnYACheckPending").on("click", function (e) {
        e.preventDefault();
        resetYarnAvailabilityCheckTableParams();
        status = 5;
        isAllChecked = false;
        isFMTDDateChecked = false;
        isLaterCustomChecked = true;
        initMasterTable();
        getYarnAvailabilityCheckData();

        $("#btnSaveYarnAvailabilityCheck").fadeIn();
        $("#btnProposeYarnAvailabilityCheck").fadeOut();
        $("#btnApprovedYarnAvailabilityCheck").fadeOut();
        $("#btnAcknowledgeYarnAvailabilityCheck").fadeOut();
        $("#btnRejectYarnAvailabilityCheck").fadeOut();

        $("#btnYACheckPending").css('background', '#008080');
        $("#btnYACheckPending").css('color', '#FFFFFF');

        $("#btnPartiallyCompleted").css('background', '#FFFFFF');
        $("#btnPartiallyCompleted").css('color', '#000000');

        $("#btnProposed").css('background', '#FFFFFF');
        $("#btnProposed").css('color', '#000000');

        $("#btnApproved").css('background', '#FFFFFF');
        $("#btnApproved").css('color', '#000000');

        $("#btnReject").css('background', '#FFFFFF');
        $("#btnReject").css('color', '#000000');

        $("#btnALL").css('background', '#FFFFFF');
        $("#btnALL").css('color', '#000000');
    });

    $("#btnProposed").on("click", function (e) {
        e.preventDefault();
        resetYarnAvailabilityCheckTableParams();
        status = 4;
        isAllChecked = false;
        isFMTDDateChecked = false;
        initMasterTable();
        getYarnAvailabilityCheckData();

        $("#btnSaveYarnAvailabilityCheck").fadeOut();
        $("#btnProposeYarnAvailabilityCheck").fadeOut();
        $("#btnApprovedYarnAvailabilityCheck").fadeIn();
        $("#btnRejectYarnAvailabilityCheck").fadeIn();
        $("#btnAcknowledgeYarnAvailabilityCheck").fadeOut();

        $("#btnProposed").css('background', '#008080');
        $("#btnProposed").css('color', '#FFFFFF');

        $("#btnPartiallyCompleted").css('background', '#FFFFFF');
        $("#btnPartiallyCompleted").css('color', '#000000');

        $("#btnYACheckPending").css('background', '#FFFFFF');
        $("#btnYACheckPending").css('color', '#000000');

        $("#btnApproved").css('background', '#FFFFFF');
        $("#btnApproved").css('color', '#000000');

        $("#btnReject").css('background', '#FFFFFF');
        $("#btnReject").css('color', '#000000');

        $("#btnALL").css('background', '#FFFFFF');
        $("#btnALL").css('color', '#000000');
    });

    $("#btnApproved").on("click", function (e) {
        e.preventDefault();
        resetYarnAvailabilityCheckTableParams();
        status = 5;
        isAllChecked = false;
        isFMTDDateChecked = false;
        initMasterTable();
        getYarnAvailabilityCheckData();

        $("#btnSaveYarnAvailabilityCheck").fadeOut();
        $("#btnProposeYarnAvailabilityCheck").fadeOut();
        $("#btnApprovedYarnAvailabilityCheck").fadeOut();
        $("#btnRejectYarnAvailabilityCheck").fadeOut();
        $("#btnAcknowledgeYarnAvailabilityCheck").fadeIn();

        $("#btnApproved").css('background', '#008080');
        $("#btnApproved").css('color', '#FFFFFF');

        $("#btnPartiallyCompleted").css('background', '#FFFFFF');
        $("#btnPartiallyCompleted").css('color', '#000000');

        $("#btnYACheckPending").css('background', '#FFFFFF');
        $("#btnYACheckPending").css('color', '#000000');

        $("#btnProposed").css('background', '#FFFFFF');
        $("#btnProposed").css('color', '#000000');

        $("#btnReject").css('background', '#FFFFFF');
        $("#btnReject").css('color', '#000000');

        $("#btnALL").css('background', '#FFFFFF');
        $("#btnALL").css('color', '#000000');
    });

    $("#btnAcknowledge").on("click", function (e) {
        e.preventDefault();
        resetYarnAvailabilityCheckTableParams();
        status = 6;
        isFMTDDateChecked = true;
        isAllChecked = false;
        initMasterTable();
        getYarnAvailabilityCheckData();

        $("#btnSaveYarnAvailabilityCheck").fadeOut();
        $("#btnProposeYarnAvailabilityCheck").fadeOut();
        $("#btnRejectYarnAvailabilityCheck").fadeOut();
        $("#btnApprovedYarnAvailabilityCheck").fadeOut();
        $("#btnRejectYarnAvailabilityCheck").fadeOut();

        $("#btnPartiallyCompleted").css('background', '#FFFFFF');
        $("#btnPartiallyCompleted").css('color', '#000000');

        $("#btnYACheckPending").css('background', '#FFFFFF');
        $("#btnYACheckPending").css('color', '#000000');

        $("#btnProposed").css('background', '#FFFFFF');
        $("#btnProposed").css('color', '#000000');

        $("#btnReject").css('background', '#FFFFFF');
        $("#btnReject").css('color', '#000000');

        $("#btnApproved").css('background', '#FFFFFF');
        $("#btnApproved").css('color', '#000000');
    });

    $("#btnReject").on("click", function (e) {
        e.preventDefault();
        resetYarnAvailabilityCheckTableParams();
        status = 9;
        isFMTDDateChecked = false;
        initMasterTable();
        getYarnAvailabilityCheckData();

        $("#btnSaveYarnAvailabilityCheck").fadeOut();
        $("#btnProposeYarnAvailabilityCheck").fadeOut();
        $("#btnApprovedYarnAvailabilityCheck").fadeOut();
        $("#btnRejectYarnAvailabilityCheck").fadeOut();
        $("#btnAcknowledgeYarnAvailabilityCheck").fadeOut();

        $("#btnReject").css('background', '#008080');
        $("#btnReject").css('color', '#FFFFFF');

        $("#btnPartiallyCompleted").css('background', '#FFFFFF');
        $("#btnPartiallyCompleted").css('color', '#000000');

        $("#btnYACheckPending").css('background', '#FFFFFF');
        $("#btnYACheckPending").css('color', '#000000');

        $("#btnProposed").css('background', '#FFFFFF');
        $("#btnProposed").css('color', '#000000');

        $("#btnALL").css('background', '#FFFFFF');
        $("#btnALL").css('color', '#000000');

        $("#btnApproved").css('background', '#FFFFFF');
        $("#btnApproved").css('color', '#000000');

        $("#btnALL").css('background', '#FFFFFF');
        $("#btnALL").css('color', '#000000');
    });
});

function initMasterTable() {
    $("#tblYarnAvailabilityCheckMaster").bootstrapTable('destroy');
    $("#tblYarnAvailabilityCheckMaster").bootstrapTable({
        showRefresh: true,
        showExport: true,
        showColumns: true,
        toolbar: "#YarnAvailabilityCheckToolbar",
        exportTypes: "['csv', 'excel']",
        pagination: true,
        filterControl: true,
        searchOnEnterKey: true,
        sidePagination: "server",
        pageList: "[10, 25, 50, 100, 500]",
        cache: false,
        columns: [
            {
                title: 'Actions',
                align: 'center',
                width: 50,
                visible: !isAllChecked,
                formatter: function () {
                    return [
                        '<span class="btn-group">',
                        '<a class="btn btn-default btn-xs edit" href="javascript:void(0)" title="View Booking">',
                        '<i class="fa fa-eye"></i>',
                        '</a>',
                        '</span>'
                    ].join('');
                },
                events: {
                    'click .edit': function (e, value, row, index) {
                        e.preventDefault();

                        YarnAvailabilityCheckresetForm();
                        //formYarnAvailabilityCheckMaster.find("#divEditYarnAvailabilityCheckMaster1").fadeOut();
                        formYarnAvailabilityCheckMaster.find("#divEditYarnAvailabilityCheckLab").fadeOut();
                        formYarnAvailabilityCheckMaster.find("#divEditYarnAvailabilityCheckDyeing").fadeOut();
                        formYarnAvailabilityCheckMaster.find("#divEditYarnAvailabilityCheckFinishing").fadeOut();

                       
                        
                        $("#BookingID").val(row.BookingId);
                        //getYarnAvailabilityCheckMaster(row.Id, row.BookingId);
                        getYarnAvailabilityCheckMaster(row.Id, row.BookingNo);

                        //Child
                        initTblFabricYarnAvailabilityCheckChild();
                        getYACheckChildData(row.BookingId);

                        if (!row.Id) { 
                            //getTextileProcess();
                        }
                        else { 
                            initTblFabricYarnAvailabilityCheckChild();
                            getYACheckChildData(row.BookingId);

                            //formYarnAvailabilityCheckMaster.find("#divEditYarnAvailabilityCheckMaster1").fadeIn();
                        }
                    }
                }
            },
            {
                field: "BookingId",
                title: "BookingId",
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                visible: true
            },
            {
                field: "BookingNo",
                title: "Booking No",
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
                field: "BuyerName",
                title: "Buyer",
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "BuyerTeamName",
                title: "Buyer Team",
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "StyleNo",
                title: "Style No",
                filterControl: "input",
                visible: !isAllChecked,
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "BookingDate",
                title: "Booking Date",
                filterControl: "input",
                visible: !isAllChecked,
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "AcknowledgeDate",
                title: "Booking Acknowledged Date",
                filterControl: "input",
                visible: !isAllChecked,
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "FMTDAcknowledgeDate",
                title: "FMTD Acknowledged Date",
                filterControl: "input",
                visible: isFMTDDateChecked,
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "Knitting",
                title: "Knitting",
                filterControl: "input",
                visible: isAllChecked,
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "Lab",
                title: "Lab",
                filterControl: "input",
                visible: isAllChecked,
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "Dyeing",
                title: "Dyeing",
                filterControl: "input",
                visible: isAllChecked,
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "Wash",
                title: "Wash",
                filterControl: "input",
                visible: isAllChecked,
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

            getYarnAvailabilityCheckData();
        },
        onSort: function (name, order) {
            tableParams.sort = name;
            tableParams.order = order;
            tableParams.offset = 0;

            getYarnAvailabilityCheckData();
        },
        onRefresh: function () {
            resetYarnAvailabilityCheckTableParams();
            getYarnAvailabilityCheckData();
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

            getYarnAvailabilityCheckData();
        }
    });
}

function initTblFabricYarnAvailabilityCheckChild() {
    formYarnAvailabilityCheckMaster.find("#tblFabricYarnAvailabilityCheckChild").bootstrapTable('destroy').bootstrapTable({
        columns: [
            {
                field: "YarnComposition",
                title: "Yarn Composition",
                filterControl: "input",
            },
            {
                field: "YarnColor",
                title: "Yarn Color",
                filterControl: "input"
            },
            {
                field: "FinalYarnCount",
                title: "Yarn Count",
                filterControl: "input"
            },
            {
                field: "StockQty",
                title: "Available Stock Qty",
                align: 'center',
                width: 50,
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "ProposedStockQtySum",
                title: "Proposed Stock Qty",
                width: 30,
                align: 'center',
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "WIPYettoProposedQty",
                title: "WIP (Yet to Proposed) Qty",
                width: 30,
                align: 'center',
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "WIPProposedQty",
                title: "Proposed WIP Qty",
                width: 30,
                align: 'center',
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "AllocatedQty",
                title: "Allocated Qty",
                width: 30,
                align: 'center',
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "YettoAllocatedQty",
                title: "Yet to Allocated Qty",
                width: 30,
                align: 'center',
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "ProposedStockQty",
                title: "From Stock Qty",
                align: 'center',
                editable: {
                    type: 'text',
                    inputclass: 'input-sm',
                    showbuttons: false
                }
            },
            {
                field: "ProposedWIPQty",
                title: "From WIP Qty",
                width: 30,
                align: 'center',
                editable: {
                    type: 'text',
                    inputclass: 'input-sm',
                    showbuttons: false
                }
            },
            {
                field: "ProposedRemarks",
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

function getYarnAvailabilityCheckData() {
    var queryParams = $.param(tableParams);
    $('#tblYarnAvailabilityCheckMaster').bootstrapTable('showLoading');
    var url = "/yacapi/YarnAvailabilityCheck" + "?status=" + status + "&processMasterId=" + 1 + "&" + queryParams;
    axios.get(url)
        .then(function (response) {
            console.log(response.data);
            $("#tblYarnAvailabilityCheckMaster").bootstrapTable('load', response.data);
            $('#tblYarnAvailabilityCheckMaster').bootstrapTable('hideLoading');
        })
        .catch(function (err) {
            toastr.error(err.response.data.Message);
        })
}

function getYarnAvailabilityCheckMaster(id, bookingId) {
    var url = "";
    var bookingNo = bookingId;
    //if (id)
    //    url = "/yacapi/YarnAvailabilityCheck/" + id + "/" + bookingId;
    //else
    //    url = "/yacapi/newYarnAvailabilityCheck/" + bookingId;

    if (id)
        url = "/yacapi/YarnAvailabilityCheck/" + id + "/" + bookingId;
    else
        url = "/yacapi/newYarnAvailabilityCheck/" + bookingNo;


    axios.get(url)
        .then(function (response) {
            //YarnAvailabilityCheckChildList = [];
            var data = response.data;
            formYarnAvailabilityCheckMaster.find("#BAnalysisNo").val(data.BAnalysisNo);
            formYarnAvailabilityCheckMaster.find("#BAnalysisNoOrigin").val(data.BAnalysisNoOrigin);
            formYarnAvailabilityCheckMaster.find("#Id").val(data.Id);
            formYarnAvailabilityCheckMaster.find("#PreBAnalysisId").val(data.Id);
            formYarnAvailabilityCheckMaster.find("#BuyerId").val(data.BuyerId);
            formYarnAvailabilityCheckMaster.find("#BuyerTeamId").val(data.BuyerTeamId);
            formYarnAvailabilityCheckMaster.find("#CompanyId").val(data.CompanyId);
            formYarnAvailabilityCheckMaster.find("#ExportOrderId").val(data.ExportOrderId);
            formYarnAvailabilityCheckMaster.find("#BookingId").val(data.BookingId);
            formYarnAvailabilityCheckMaster.find("#SubGroupId").val(data.SubGroupId);
            formYarnAvailabilityCheckMaster.find("#AdditionalBooking").val(data.AdditionalBooking);
            formYarnAvailabilityCheckMaster.find("#PreProcessRevNo").val(data.PreProcessRevNo);
            formYarnAvailabilityCheckMaster.find("#RevisionNo").val(data.RevisionNo);
            formYarnAvailabilityCheckMaster.find("#YInHouseDate").val(data.YInHouseDate);
            formYarnAvailabilityCheckMaster.find("#YRequiredDate").val(data.YRequiredDate);
            formYarnAvailabilityCheckMaster.find("#Remarks").val(data.Remarks);

            formYarnAvailabilityCheckMaster.find("#lblBookingNo").val(data.BookingNo);
            formYarnAvailabilityCheckMaster.find("#lblExportOrderNo").val(data.ExportOrderNo);
            formYarnAvailabilityCheckMaster.find("#lblBuyerName").val(data.BuyerName);
            formYarnAvailabilityCheckMaster.find("#lblBuyerTeam").val(data.BuyerTeamName);
            formYarnAvailabilityCheckMaster.find("#lblMerchandiserName").val(data.MerchandiserName);
            if (data.AcknowledgeDate)
                formYarnAvailabilityCheckMaster.find("#lblAcknowledgeDate1").val(moment(data.AcknowledgeDate).format('DD/MM/YYYY'));
            if (data.BookingDate)
                formYarnAvailabilityCheckMaster.find("#lblBookingDate").val(moment(data.BookingDate).format('DD/MM/YYYY'));
            formYarnAvailabilityCheckMaster.find("#lblStyleNo").val(data.StyleNo);
            formYarnAvailabilityCheckMaster.find("#lblStyleType").val(data.StyleTypes);
            formYarnAvailabilityCheckMaster.find("#divEditYarnAvailabilityCheckMaster").fadeIn();
            formYarnAvailabilityCheckMaster.find("#tblTextileProcessMaster").fadeIn();
            $("#divtblYarnAvailabilityCheckMaster").fadeOut();
            formYarnAvailabilityCheckMaster.find("#divButtonExecutionsBookingYACheck").fadeIn();
        })
        .catch(function () {
            toastr.error(constants.LOAD_ERROR_MESSAGE);
        })
}

function getYACheckChildData(bookingId) {
    var url = "";
    url = "/yacapi/newYarnAvailabilityCheckchild/" + bookingId;

    axios.get(url)
        .then(function (response) {
            YarnAvailabilityCheckChildList = response.data;
            formYarnAvailabilityCheckMaster.find("#tblFabricYarnAvailabilityCheckChild").bootstrapTable('load', response.data);
        })
        .catch(function () {
            toastr.error(constants.LOAD_ERROR_MESSAGE);
        })
}

function YarnAvailabilityCheckresetForm() {
    formYarnAvailabilityCheckMaster.trigger("reset");
    formYarnAvailabilityCheckMaster.find("#Id").val(-1111);
    formYarnAvailabilityCheckMaster.find("#EntityState").val(4);
}

function getYarnProposedFrom() {
    axios.get("/api/selectoption/YarnProposedFrom")
        .then(function (response) {
            $.each(response.data, function (i, v) {
                var item = { value: v.id, text: v.text };
                yarnProposedList.push(item);
            })
        })
        .catch(function (err) {
            toastr.error(err.response.data.Message);
        })
}

function resetYarnAvailabilityCheckTableParams() {
    tableParams.offset = 0;
    tableParams.limit = 10;
    tableParams.filter = '';
    tableParams.sort = '';
    tableParams.order = '';
}

function backToListYACheck() {
    $("#divtblYarnAvailabilityCheckMaster").fadeIn();
    $("#divEditYarnAvailabilityCheckMaster").fadeOut();
    $("#divEditYarnAvailabilityCheckMaster1").fadeOut();

    getYarnAvailabilityCheckData();
}


