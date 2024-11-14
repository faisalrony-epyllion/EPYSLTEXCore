var formGreyFabricProductionQA;
var GreyFabricProductionQAMaster;
var filterBy = {};
var Id;
var GFPStatus = 3;
var GreyFabricProductionQAChilds = [];
var GreyFabricProductionQAChildRolls = [];
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
var $childTableEl;
var GreyFabricProductionQAChildList = [];
var GreyFabricProductionQAChildId;

$(function () {
    formGreyFabricProductionQA = $("#formGreyFabricProductionQA");

    $("#btnGFPApprovedLists").css('background', '#008080');
    $("#btnGFPApprovedLists").css('color', '#FFFFFF');

    initPendingGreyFabricProductionQAMasterTableMain();
    getPendingGreyFabricProductionQAMasterDataMain();

    //$("#btnGFPNew").on("click", function (e) {
    //    e.preventDefault();

    //    $("#divNewGreyFabricProductionQA").fadeIn();
    //    $("#divtblGreyFabricProductionQA").fadeOut();
    //    $("#divGreyFabricProductionQAButtonExecutions").fadeIn();

    //    $("#btnApprovedGFP").fadeOut();
    //    $("#btnUnApprovedGFP").fadeOut();

    //    initPendingGreyFabricProductionQAMasterTable();

    //    getNewGreyFabricProductionQANo();

    //    $("#btnGFPNew").css('background', '#008080');
    //    $("#btnGFPNew").css('color', '#FFFFFF');

    //    $("#btnPendingGFP").css('background', '#FFFFFF');
    //    $("#btnPendingGFP").css('color', '#000000');

    //    $("#btnGFPLists").css('background', '#FFFFFF');
    //    $("#btnGFPLists").css('color', '#000000');

    //    $("#btnGFPUnApproved").css('background', '#FFFFFF');
    //    $("#btnGFPUnApproved").css('color', '#000000');
    //});

    $("#btnGFPApprovedLists").on("click", function (e) {
        e.preventDefault();
        GFPStatus = 3;
        resetTableParamsGFP();
        initPendingGreyFabricProductionQAMasterTableMain();
        getPendingGreyFabricProductionQAMasterDataMain();

        $("#btnGFPApprovedLists").css('background', '#008080');
        $("#btnGFPApprovedLists").css('color', '#FFFFFF');

        $("#btnGFPQAPendingLists").css('background', '#FFFFFF');
        $("#btnGFPQAPendingLists").css('color', '#000000');

        $("#btnGFPQAApprovedLists").css('background', '#FFFFFF');
        $("#btnGFPQAApprovedLists").css('color', '#000000');
    });

    $("#btnGFPQAPendingLists").on("click", function (e) {
        e.preventDefault();
        GFPStatus = 4;
        resetTableParamsGFP();
        initPendingGreyFabricProductionQAMasterTableMain();
        getPendingGreyFabricProductionQAMasterDataMain();

        $("#btnGFPQAPendingLists").css('background', '#008080');
        $("#btnGFPQAPendingLists").css('color', '#FFFFFF');

        $("#btnGFPApprovedLists").css('background', '#FFFFFF');
        $("#btnGFPApprovedLists").css('color', '#000000');

        $("#btnGFPQAApprovedLists").css('background', '#FFFFFF');
        $("#btnGFPQAApprovedLists").css('color', '#000000');
    });

    $("#btnGFPQAApprovedLists").on("click", function (e) {
        e.preventDefault();
        GFPStatus = 5;
        resetTableParamsGFP();
        initPendingGreyFabricProductionQAMasterTableMain();
        getPendingGreyFabricProductionQAMasterDataMain();

        $("#btnGFPQAApprovedLists").css('background', '#008080');
        $("#btnGFPQAApprovedLists").css('color', '#FFFFFF');

        $("#btnGFPApprovedLists").css('background', '#FFFFFF');
        $("#btnGFPApprovedLists").css('color', '#000000');

        $("#btnGFPQAPendingLists").css('background', '#FFFFFF');
        $("#btnGFPQAPendingLists").css('color', '#000000');
    });

    //$("#btnAddOrders").on("click", function (e) {
    //    e.preventDefault();
    //    var GFPStatus = 1;
    //    getPendingGreyFabricProductionQAMasterData();
    //    $("#modal-child").modal('show');
    //});

    $("#btnSaveGFP").click(function (e) {
        e.preventDefault();

        var data = formDataToJson(formGreyFabricProductionQA.serializeArray());
        data.Id = $("#Id").val();
        data["GreyFabricProductionQAChilds"] = GreyFabricProductionQAChilds;

        var config = { headers: { 'Content-Type': 'application/json' } };
        axios.post("/GFPApi/GreyFabricProductionQASave", data, config)
            .then(function () {
                toastr.success("Your production saved successfully." + " Production No: " + $("#GFPNo2").val());
                GreyFabricProductionQAbackToList();
            })
            .catch(showResponseError);
    });

    $("#btnGFPEditCancel").on("click", function (e) {
        e.preventDefault();
        GreyFabricProductionQAbackToList();
    });

    $("#btnApprovedGFP").click(function (e) {
        e.preventDefault();

        var data = { Id: $("#Id").val() };

        var config = { headers: { 'Content-Type': 'application/json' } };
        axios.post("/yrApi/GreyFabricProductionQAApprovedLists", data, config)
            .then(function () {
                toastr.success(constants.APPROVE_SUCCESSFULLY);
                $("#divNewGreyFabricProductionQA").fadeOut();
                $("#divtblGreyFabricProductionQA").fadeIn();
                $("#divGreyFabricProductionQAButtonExecutions").fadeOut();
                getPendingGreyFabricProductionQAMasterData();
            })
            .catch(showResponseError);
    });

    $("#btnAddChildRollData").click(function (e) {
        e.preventDefault;
        var newGreyFabricProductionQAChildRollData = {
            BookingChildId: $("#BookingChildId").val(),
            RollNo: $("#RollNo").val(),
            RollQtyInKG: $("#RollQtyInKG").val(),
            LengthInInch: $("#LengthInInch").val(),
            WidthInInch: $("#WidthInInch").val()
        };

        var data = $childTableEl.bootstrapTable('getData', false);
        data.push(newGreyFabricProductionQAChildRollData);
        $childTableEl.bootstrapTable('load', data);

        var gC = GreyFabricProductionQAChilds.find(function (el) {
            return el.BookingChildId == newGreyFabricProductionQAChildRollData.BookingChildId;
        });

        if (gC.GreyFabricProductionQAChildRolls == undefined)
            gC["GreyFabricProductionQAChildRolls"] = [];

        gC.GreyFabricProductionQAChildRolls.push(newGreyFabricProductionQAChildRollData);

        var LastSavedRollNoIni = $("#RollNo").val();
        $("#LastSavedRollNo").val(LastSavedRollNoIni);
        //$("#modal-child-roll").modal('hide');

        var RollNoIni = parseInt($("#LastSavedRollNo").val()) + parseInt(1);
        $("#RollNo").val(RollNoIni);
        var RollLengthIni = "0";
        var RollWidthIni = "0";
        var RollQtyIni = "0";
        $("#LengthInInch").val(RollLengthIni);
        $("#WidthInInch").val(RollWidthIni);
        $("#RollQtyInKG").val(RollQtyIni);
        $("#LengthInInch").focus();
    });
});

function getNewGreyFabricProductionQANo() {
    url = "/GFPApi/NewGreyFabricProductionQASLNo/";

    axios.get(url)
        .then(function (response) {
            GreyFabricProductionQAMaster = response.data;
            $("#GFPNo2").val(response.data.GFPNo);
        })
        .catch(function (err) {
            toastr.error(err.response.data.Message);
        })
}

function initPendingGreyFabricProductionQAMasterTableMain() {
    $("#tblGreyFabricProductionQAMain").bootstrapTable('destroy');
    $("#tblGreyFabricProductionQAMain").bootstrapTable({
        showRefresh: true,
        showExport: true,
        showColumns: true,
        toolbar: "#GreyFabricProductionQAToolbar",
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
                        $("#Id").val(row.Id);
                        getGreyFabricProductionQAMasterEdit(row.Id);

                        $("#divNewGreyFabricProductionQA").fadeIn();
                        $("#FabricBookingInfo").fadeIn();
                        $("#divtblGreyFabricProductionQA").fadeOut();
                        $("#divGreyFabricProductionQAButtonExecutions").fadeIn();
                        $("#btnAddOrders").fadeOut();
                        $("#btnApprovedGFP").fadeIn();

                        initTblGreyFabricProductionQAChildItems();
                    }
                }
            },
            {
                field: "GfpNo",
                title: "Production No",
                filterControl: "input",
                visible: !isAllChecked,
                width: 100,
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "GFPDate",
                title: "Production Date",
                visible: !isAllChecked,
                filterControl: "input",
                width: 100,
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "BookingNo",
                title: "Booking No",
                filterControl: "input",
                width: 150,
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "ExportOrderNo",
                title: "Export Order No",
                filterControl: "input",
                width: 80,
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "BuyerName",
                title: "Buyer",
                filterControl: "input",
                width: 150,
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            //{
            //    field: "BuyerTeamName",
            //    title: "Buyer Team",
            //    filterControl: "input",
            //    width: 180
            //},
            {
                field: "StyleNo",
                title: "Style No",
                filterControl: "input",
                visible: !isAllChecked,
                width: 130,
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "BookingDate",
                title: "Booking Date",
                filterControl: "input",
                visible: !isAllChecked,
                width: 70,
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

            getPendingGreyFabricProductionQAMasterData();
        },
        onSort: function (name, order) {
            tableParams.sort = name;
            tableParams.order = order;
            tableParams.offset = 0;

            getPendingGreyFabricProductionQAMasterData();
        },
        onRefresh: function () {
            resetTableParams();
            getPendingGreyFabricProductionQAMasterData();
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

            getPendingGreyFabricProductionQAMasterData();
        }
    });
}

function initPendingGreyFabricProductionQAMasterTable() {
    $("#tblGreyFabricProductionQA").bootstrapTable('destroy');
    $("#tblGreyFabricProductionQA").bootstrapTable({
        showRefresh: true,
        showExport: true,
        showColumns: true,
        toolbar: "#BookingAnalysisToolbar",
        exportTypes: "['csv', 'excel']",
        pagination: true,
        filterControl: true,
        searchOnEnterKey: true,
        sidePagination: "server",
        pageList: "[10, 25, 50, 100, 500]",
        cache: false,
        columns: [
            {
                field: "BookingNo",
                title: "Booking No",
                filterControl: "input",
                width: 150,
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "ExportOrderNo",
                title: "Export Order No",
                filterControl: "input",
                width: 80,
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "BuyerName",
                title: "Buyer",
                filterControl: "input",
                width: 150,
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "BuyerTeamName",
                title: "Buyer Team",
                filterControl: "input",
                width: 180,
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "StyleNo",
                title: "Style No",
                filterControl: "input",
                visible: !isAllChecked,
                width: 130,
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "BookingDate",
                title: "Booking Date",
                filterControl: "input",
                visible: !isAllChecked,
                width: 70,
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            }
        ],
        onDblClickRow: function (row, $element, field) {
            $("#modal-child").modal('hide');
            $("#ExportOrderID").val(row.ExportOrderId);
            $("#ExportOrderNo").val(row.BookingNo);
            $("#BookingId").val(row.BookingId);
            getGreyFabricProductionQAMaster(row.Id, row.BookingId);
            getGreyFabricProductionQAChildData(row.BookingId);
            initTblGreyFabricProductionQAChildItems();
            $("#FabricBookingInfo").fadeIn();
        },
        onPageChange: function (number, size) {
            var newOffset = (number - 1) * size;
            var newLimit = size;
            if (tableParams.offset == newOffset && tableParams.limit == newLimit)
                return;

            tableParams.offset = newOffset;
            tableParams.limit = newLimit;

            getPendingGreyFabricProductionQAMasterData();
        },
        onSort: function (name, order) {
            tableParams.sort = name;
            tableParams.order = order;
            tableParams.offset = 0;

            getPendingGreyFabricProductionQAMasterData();
        },
        onRefresh: function () {
            resetTableParams();
            getPendingGreyFabricProductionQAMasterData();
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

            getPendingGreyFabricProductionQAMasterData();
        }
    });
}

function getGreyFabricProductionQAMaster(id, bookingId) {
    var url = "";
    url = "/GFPApi/fabricBookingInformation/" + bookingId;
    axios.get(url)
        .then(function (response) {
            GreyFabricProductionQAChildList = [];
            var data = response.data;
            $("#Id").val(data.Id);
            $("#BuyerId").val(data.BuyerId);
            $("#BuyerTeamId").val(data.BuyerTeamId);
            $("#CompanyId").val(data.CompanyId);
            $("#ExportOrderId").val(data.ExportOrderId);
            $("#BookingId").val(data.BookingId);
            $("#SubGroupId").val(data.SubGroupId);
            $("#RevisionNo").val(data.RevisionNo);
            $("#YInHouseDate").val(data.YInHouseDate);
            $("#YRequiredDate").val(data.YRequiredDate);
            $("#Remarks").val(data.Remarks);

            $("#lblBookingNo").text(data.BookingNo);
            $("#lblExportOrderNo").text(data.ExportOrderNo);
            $("#lblBuyerName").text(data.BuyerName);
            $("#lblBuyerTeam").text(data.BuyerTeamName);
            $("#lblMerchandiserName").text(data.MerchandiserName);
            if (data.AcknowledgeDate)
                $("#lblAcknowledgeDate1").text(moment(data.AcknowledgeDate).format('DD/MM/YYYY'));
            if (data.BookingDate)
                $("#lblBookingDate").text(moment(data.BookingDate).format('DD/MM/YYYY'));
            $("#lblStyleNo").text(data.StyleNo);
        })
        .catch(function () {
            toastr.error(constants.LOAD_ERROR_MESSAGE);
        })
}

function getGreyFabricProductionQAMasterEdit(id) {
    var url = "";
    url = "/GFPApi/GreyFabricProductionMasterEdit/" + $("#Id").val();
    axios.get(url)
        .then(function (response) {
            GreyFabricProductionQAChildList = [];
            var data = response.data;
            $("#Id").val(data.Id);
            $("#BuyerId").val(data.BuyerId);
            $("#BuyerTeamId").val(data.BuyerTeamId);
            $("#CompanyId").val(data.CompanyId);
            $("#ExportOrderId").val(data.ExportOrderId);
            $("#BookingId").val(data.BookingId);
            $("#SubGroupId").val(data.SubGroupId);
            $("#RevisionNo").val(data.RevisionNo);
            $("#YInHouseDate").val(data.YInHouseDate);
            $("#YRequiredDate").val(data.YRequiredDate);
            $("#Remarks").val(data.Remarks);

            $("#lblBookingNo").text(data.BookingNo);
            $("#lblExportOrderNo").text(data.ExportOrderNo);
            $("#lblBuyerName").text(data.BuyerName);
            $("#lblBuyerTeam").text(data.BuyerTeamName);
            $("#lblMerchandiserName").text(data.MerchandiserName);
            if (data.AcknowledgeDate)
                $("#lblAcknowledgeDate1").text(moment(data.AcknowledgeDate).format('DD/MM/YYYY'));
            if (data.BookingDate)
                $("#lblBookingDate").text(moment(data.BookingDate).format('DD/MM/YYYY'));
            $("#lblStyleNo").text(data.StyleNo);

            $("#GFPNo").val(data.GfpNo);
            $("#GFPDate").val(data.GfpDateStr);
            $("#ExportOrderNo").val(data.BookingNo);

            $("#tblGreyFabricProductionQAChilds").bootstrapTable('load', response.data.GreyFabricProductionChilds);
        })
        .catch(function () {
            toastr.error(constants.LOAD_ERROR_MESSAGE);
        })
}

function getPendingGreyFabricProductionQAMasterDataMain() {
    var queryParams = $.param(tableParams);
    $('#tblGreyFabricProductionQAMain').bootstrapTable('showLoading');
    var url = "/GFPApi/greyFabricProductionMasterData" + "?gFPStatus=" + GFPStatus + "&" + queryParams;
    axios.get(url)
        .then(function (response) {
            $("#tblGreyFabricProductionQAMain").bootstrapTable('load', response.data);
            $('#tblGreyFabricProductionQAMain').bootstrapTable('hideLoading');
        })
        .catch(function (err) {
            toastr.error(err.response.data.Message);
        })
}

function getPendingGreyFabricProductionQAMasterData() {
    var queryParams = $.param(tableParams);
    $('#tblGreyFabricProductionQA').bootstrapTable('showLoading');
    var url = "/GFPApi/fabricBookingPendingData" + "?gFPStatus=" + 1 + "&" + queryParams;
    axios.get(url)
        .then(function (response) {
            $("#tblGreyFabricProductionQA").bootstrapTable('load', response.data);
            $('#tblGreyFabricProductionQA').bootstrapTable('hideLoading');
        })
        .catch(function (err) {
            toastr.error(err.response.data.Message);
        })
}

function getGreyFabricProductionQAChildData(bookingId) {
    var url = "";
    var id = $("#Id").val();
    if (id > 0)
        url = "/api/GreyFabricProductionQAChild/" + id;
    else
        url = "/GFPApi/fabricBookingChildData/" + $("#BookingId").val();
    axios.get(url)
        .then(function (response) {
            GreyFabricProductionQAChilds = response.data;
            $("#tblGreyFabricProductionQAChilds").bootstrapTable('load', response.data);
        })
        .catch(function () {
            toastr.error(constants.LOAD_ERROR_MESSAGE);
        })
}

function initTblGreyFabricProductionQAChildItems() {
    $("#tblGreyFabricProductionQAChilds").bootstrapTable({
        showFooter: true,
        detailView: true,
        columns: [
            {
                field: "FabricConstruction",
                title: "Construction",
                filterControl: "input"
            },
            {
                field: "FabricComposition",
                title: "Composition",
                filterControl: "input"
            },
            {
                field: "ColorName",
                title: "Fabric Color",
                filterControl: "input"
            },
            {
                field: "FabricGsm",
                title: "GSM",
                align: "center",
                filterControl: "input"
            },
            {
                field: "FabricWidth",
                title: "Width",
                align: "center",
                filterControl: "input",
                width: 50
            },
            {
                field: "KnittingType",
                title: "Knitting Type",
                align: "left",
                filterControl: "input"
            },
            {
                field: "DyeingType",
                title: "Dyeing Type",
                align: "left",
                filterControl: "input"
            },
            //{
            //    field: "Finishing",
            //    title: "Finishing",
            //    align: "left",
            //    filterControl: "input"
            //},
            //{
            //    field: "Washing",
            //    title: "Washing",
            //    align: "left",
            //    filterControl: "input"
            //},
            //{
            //    field: "Remarks",
            //    title: "Instruction",
            //    align: "left",
            //    filterControl: "input"
            //},
            {
                field: "BookingQty",
                title: "Booking Qty",
                align: "right",
                filterControl: "input",
                footerFormatter: calculateGreyFabricProductionQATotalBookingQty
            },
            {
                field: "DisplayUnitDesc",
                title: "Unit",
                align: "left",
                filterControl: "input"
            },
            {
                field: "ProductionQty",
                title: "Production Qty",
                align: "right",
                filterControl: "input",
                editable: {
                    type: 'text',
                    inputclass: 'input-sm',
                    showbuttons: false
                },
                footerFormatter: calculateGreyFabricProductionQATotalProductionQty
            },
            {
                field: "ProductionExcessQty",
                title: "Excess Qty",
                align: "right",
                filterControl: "input",
                footerFormatter: calculateGreyFabricProductionQATotalExcessQty
            }
        ],
        onEditableSave: function (field, row, oldValue, $el) {
            row.ProductionExcessQty = (row.ProductionQty - row.BookingQty).toFixed(4);

            $("#tblGreyFabricProductionQAChilds").bootstrapTable('load', GreyFabricProductionQAChilds);
        },
        onExpandRow: function (index, row, $detail) {
            if ($("#Id").val() > 0) {
                $("#GFPChildId").val(row.GFPChildId);
                var url = "/GFPApi/GreyFabricProductionQAChildRollEdit/" + $("#GFPChildId").val();
                axios.get(url)
                    .then(function (response) {
                        var data = $(childTableEl).bootstrapTable('getData', false);
                        data.push(response.data);
                        $(childTableEl).bootstrapTable('load', data);

                        initGreyFabricProductionQAChildRolls(index, data);
                    })
                    .catch(function () {
                        toastr.error(constants.LOAD_ERROR_MESSAGE);
                    })
            }
            else {
                $childTableEl = $detail.html('<table id="tblGreyFabricProductionQAChildRoll-' + index + '"></table>').find('table');
                //var newGreyFabricProductionQAChildRollData = {
                //    RollNo: "",
                //    RollQtyInKG: 0.00,
                //    LengthInInch: 0.00,
                //    WidthInInch: 0.00
                //};
                var data = [];

                var greyFabricChild = row.BookingChildId;
                $("#BookingChildId").val(greyFabricChild);

                initGreyFabricProductionQAChildRolls(index, data);

                var LastSavedRollNoIni = "0";
                $("#LastSavedRollNo").val(LastSavedRollNoIni);
                var RollNoIni = "1";
                $("#RollNo").val(RollNoIni);
            }
        }
    });
}

function addNewChildRow(e, rowId) {
    $("#modal-child-roll").modal('show');
    if ($("#RollNo").val() == "0") {
        var RollNoIni = "1";
        $("#RollNo").val(RollNoIni);
        $("#LengthInInch").focus();
    }
    else {
        var RollNoIni = parseInt($("#LastSavedRollNo").val()) + parseInt(1);
        $("#RollNo").val(RollNoIni);
        var RollLengthIni = "0";
        var RollWidthIni = "0";
        var RollQtyIni = "0";
        $("#LengthInInch").val(RollLengthIni);
        $("#WidthInInch").val(RollWidthIni);
        $("#RollQtyInKG").val(RollQtyIni);
        $("#LengthInInch").focus();
    }

    //e.preventDefault;
    //var $el = $("#tblGreyFabricProductionQAChildRoll-" + rowId);
    //var newGreyFabricProductionQAChildRollData = {
    //    RollNo: "",
    //    RollQtyInKG: 0.00,
    //    LengthInInch: 0.00,
    //    WidthInInch: 0.00
    //};
    //var data = $el.bootstrapTable("getData");
    //data.push(newGreyFabricProductionQAChildRollData);
    //$el.bootstrapTable("load", data);
}

function initGreyFabricProductionQAChildRolls(rowId, data) {
    $childTableEl.bootstrapTable({
        showFooter: true,
        data: data,
        rowStyle: function (row, index) {
            if (row.EntityState == 8)
                return { classes: 'deleted-row' };

            return "";
        },
        columns: [
            {
                title: 'Actions',
                align: 'center',
                width: 120,
                formatter: function () {
                    return [
                        '<span class="btn-group">',
                        '<a class="btn btn-danger btn-xs remove" href="javascript:void(0)" title="Delete Item">',
                        '<i class="fa fa-remove" style="font-size:15px"></i>',
                        '</a>',
                        '</span>'
                    ].join('');
                },
                footerFormatter: function () {
                    return [
                        '<span class="btn-group">',
                        '<button class="btn btn-success btn-xs edit" onclick="return addNewChildRow(event, ' + rowId + ')" title="Add">',
                        '<i class="fa fa-plus" style="font-size:15px"></i>',
                        ' Add',
                        '</button>',
                        '</span>'
                    ].join('');
                },
                events: {
                    'click .remove': function (e, value, row, index) {
                        this.data[index].EntityState = 8;
                        var $target = $(e.target);
                        $target.closest("tr").addClass('deleted-row');
                    }
                }
            },
            {
                field: "RollNo",
                title: "Roll No",
                filterControl: "input"
            },
            {
                field: "LengthInInch",
                title: "Length (Inch)",
                filterControl: "input",
                editable: {
                    type: 'text',
                    inputclass: 'input-sm',
                    showbuttons: false
                }
            },
            {
                field: "WidthInInch",
                title: "Width (Inch)",
                filterControl: "input",
                editable: {
                    type: 'text',
                    inputclass: 'input-sm',
                    showbuttons: false
                }
            },
            {
                field: "RollQtyInKG",
                title: "Roll Qty (KG)",
                filterControl: "input",
                editable: {
                    type: 'text',
                    inputclass: 'input-sm',
                    showbuttons: false
                },
                footerFormatter: calculateGreyFabricProductionQATotalRollQty
            }
        ]
    });
}

function GreyFabricProductionQAbackToList() {
    $("#divNewGreyFabricProductionQA").fadeOut();
    $("#divtblGreyFabricProductionQA").fadeIn();
    $("#divGreyFabricProductionQAButtonExecutions").fadeOut();
    //getPendingGreyFabricProductionQAMasterData();
}

function resetTableParamsGFP() {
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

function calculateGreyFabricProductionQATotalBookingQty(data) {
    var BookingQty = 0;

    $.each(data, function (i, row) {
        BookingQty += isNaN(parseFloat(row.BookingQty)) ? 0 : parseFloat(row.BookingQty);
    });

    return BookingQty.toFixed(4);
}

function calculateGreyFabricProductionQATotalProductionQty(data) {
    var ProductionQty = 0;

    $.each(data, function (i, row) {
        ProductionQty += isNaN(parseFloat(row.ProductionQty)) ? 0 : parseFloat(row.ProductionQty);
    });

    return ProductionQty.toFixed(4);
}

function calculateGreyFabricProductionQATotalExcessQty(data) {
    var ProductionExcessQty = 0;

    $.each(data, function (i, row) {
        ProductionExcessQty += isNaN(parseFloat(row.ProductionExcessQty)) ? 0 : parseFloat(row.ProductionExcessQty);
    });

    return ProductionExcessQty.toFixed(4);
}

function calculateGreyFabricProductionQATotalRollQty(data) {
    var RollQtyInKG = 0;

    $.each(data, function (i, row) {
        RollQtyInKG += isNaN(parseFloat(row.RollQtyInKG)) ? 0 : parseFloat(row.RollQtyInKG);
    });

    return RollQtyInKG.toFixed(4);
}