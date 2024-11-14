//var formGreyFabricProduction;
//var GreyFabricProductionMaster;
//var filterBy = {};
//var Id;
//var GFPStatus = 2;
//var GreyFabricProductionChilds = [];
//var GreyFabricProductionChildsPopUp = [];
//var GreyFabricProductionChildRolls = [];
//var isAllChecked = false;
//var tableParams = {
//    offset: 0,
//    limit: 10,
//    sort: '',
//    order: '',
//    filter: ''
//}
//var ExportOrderfilterBy = {};
//var ExportOrderTableParams = {
//    offset: 0,
//    limit: 10,
//    sort: '',
//    order: '',
//    filter: ''
//}
//var $childTableEl;
//var greyFabricProductionChildList = [];
//var greyFabricProductionChildId; 

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
var status,
    isIssuePage = false,
    isApprovePage = false;

var masterData;
var formGreyFabricProduction;
var GreyFabricProductionMaster;
var GFPStatus = 2;
var GreyFabricProductionChilds = [];
var GreyFabricProductionChildsPopUp = [];
var GreyFabricProductionChildRolls = [];
var isAllChecked = false;
var ExportOrderfilterBy = {};
var greyFabricProductionChildList = [];
var greyFabricProductionChildId;

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


    formGreyFabricProduction = $("#formGreyFabricProduction");

    $("#btnPendingGFP").css('background', '#008080');
    $("#btnPendingGFP").css('color', '#FFFFFF');

    initPendingGreyFabricProductionMasterTableMain();
    getPendingGreyFabricProductionMasterDataMain();

    $("#btnGFPNew").on("click", function (e) {
        e.preventDefault();

        $("#divNewGreyFabricProduction").fadeIn();
        $("#divtblGreyFabricProduction").fadeOut();
        $("#divGreyFabricProductionButtonExecutions").fadeIn();

        $("#btnApprovedGFP").fadeOut();
        $("#btnUnApprovedGFP").fadeOut();

        initPendingGreyFabricProductionMasterTable();

        getNewGreyFabricProductionNo();

        $("#btnGFPNew").css('background', '#008080');
        $("#btnGFPNew").css('color', '#FFFFFF');

        $("#btnPendingGFP").css('background', '#FFFFFF');
        $("#btnPendingGFP").css('color', '#000000');

        $("#btnGFPLists").css('background', '#FFFFFF');
        $("#btnGFPLists").css('color', '#000000');

        $("#btnGFPUnApproved").css('background', '#FFFFFF');
        $("#btnGFPUnApproved").css('color', '#000000');
    });

    $("#btnPendingGFP").on("click", function (e) {
        e.preventDefault();
        GFPStatus = 2;
        //resetTableParamsGFP();
        initPendingGreyFabricProductionMasterTableMain();
        getPendingGreyFabricProductionMasterDataMain();

        $("#btnPendingGFP").css('background', '#008080');
        $("#btnPendingGFP").css('color', '#FFFFFF');

        $("#btnGFPNew").css('background', '#FFFFFF');
        $("#btnGFPNew").css('color', '#000000');

        $("#btnGFPLists").css('background', '#FFFFFF');
        $("#btnGFPLists").css('color', '#000000');

        $("#btnGFPUnApproved").css('background', '#FFFFFF');
        $("#btnGFPUnApproved").css('color', '#000000');
    });

    $("#btnGFPLists").on("click", function (e) {
        e.preventDefault();
        GFPStatus = 3;
        resetTableParamsGFP();
        initPendingGreyFabricProductionMasterTableMain();
        getPendingGreyFabricProductionMasterDataMain();

        $("#btnGFPLists").css('background', '#008080');
        $("#btnGFPLists").css('color', '#FFFFFF');

        $("#btnGFPNew").css('background', '#FFFFFF');
        $("#btnGFPNew").css('color', '#000000');

        $("#btnPendingGFP").css('background', '#FFFFFF');
        $("#btnPendingGFP").css('color', '#000000');

        $("#btnGFPUnApproved").css('background', '#FFFFFF');
        $("#btnGFPUnApproved").css('color', '#000000');
    });

    formGreyFabricProduction.find("#btnAddOrders").on("click", function (e) {
        e.preventDefault();
        var GFPStatus = 1;
        getPendingGreyFabricProductionMasterData();
        $("#GreyFabricProductionModal-Child").modal('show');
    });

    formGreyFabricProduction.find("#btnCIAddOrderItems").on("click", function (e) {
        e.preventDefault();
        getGreyFabricProductionChildData();
        initTblGreyFabricProductionChildItemsPopUp();
        $("#modal-booking-order-Items").modal('show');
    });

    $("#btnSaveGFP").click(function (e) {
        e.preventDefault();

        var data = formDataToJson(formGreyFabricProduction.serializeArray());
        data.Id = formGreyFabricProduction.find("#GFPID").val();
        data["GreyFabricProductionChilds"] = GreyFabricProductionChilds;

        var config = { headers: { 'Content-Type': 'application/json' } };
        axios.post("/GFPApi/greyFabricProductionSave", data, config)
            .then(function () {
                toastr.success("Your production saved successfully." + " Production No: " + $("#GFPNo2").val());
                GreyFabricProductionbackToList();
            })
            .catch(showResponseError);
    });

    $("#btnGFPEditCancel").on("click", function (e) {
        e.preventDefault();
        GreyFabricProductionbackToList();
    });

    $("#btnApprovedGFP").click(function (e) {
        e.preventDefault();

        var data = { Id: formGreyFabricProduction.find("#GFPID").val() };

        var config = { headers: { 'Content-Type': 'application/json' } };
        axios.post("/GFPApi/greyFabricProductionApprovedLists", data, config)
            .then(function () {
                toastr.success(constants.APPROVE_SUCCESSFULLY);
                GreyFabricProductionbackToList();
            })
            .catch(showResponseError);
    });

    $("#btnAddChildRollData").click(function (e) {
        e.preventDefault;
        var newGreyFabricProductionChildRollData = {
            BookingChildId: $("#BookingChildID").val(),
            RollNo: $("#RollNo").val(),
            RollQtyInKG: $("#RollQtyInKG").val(),
            LengthInInch: $("#LengthInInch").val(),
            WidthInInch: $("#WidthInInch").val()
        };

        var data = $childTableEl.bootstrapTable('getData', false);
        data.push(newGreyFabricProductionChildRollData);
        $childTableEl.bootstrapTable('load', data);

        var gC = GreyFabricProductionChilds.find(function (el) {
            return el.BookingChildId == newGreyFabricProductionChildRollData.BookingChildId;
        });

        if (gC.GreyFabricProductionChildRolls == undefined)
            gC["GreyFabricProductionChildRolls"] = [];

        gC.GreyFabricProductionChildRolls.push(newGreyFabricProductionChildRollData);

        var LastSavedRollNoIni = $("#RollNo").val();
        $("#LastSavedRollNo").val(LastSavedRollNoIni);
        //$("#GreyFabricProductionModal-Child-roll").modal('hide');

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

function getNewGreyFabricProductionNo() {
    url = "/GFPApi/NewGreyFabricProductionSLNo/";

    axios.get(url)
        .then(function (response) {
            GreyFabricProductionMaster = response.data;
            formGreyFabricProduction.find("#GFPNo2").val(response.data.GFPNo);
        })
        .catch(function (err) {
            toastr.error(err.response.data.Message);
        })
}

function initPendingGreyFabricProductionMasterTableMain() { 
    var columns = [
        {
            headerText: 'Commands', commands: [
                { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } }]
        },
        {
            field: 'GFPNo', headerText: 'Production No', visible: !isAllChecked
        },
        {
            field: 'GFPDate', headerText: 'Production Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1
        },
        {
            field: 'BookingNo', headerText: 'Booking No'
        },
        {
            field: 'ExportOrderNo', headerText: 'Export Order No'
        },  
        {
            field: 'BuyerName', headerText: 'Buyer'
        },
        {
            field: 'BuyerTeamName', headerText: 'Buyer Team'
        },
        {
            field: 'StyleNo', headerText: 'Style No'
        },
        {
            field: 'BookingDate', headerText: 'Booking Date'
        },
        {
            field: 'AcknowledgeDate', headerText: 'Acknowledge Date'
        }  
    ]; 
    if ($tblMasterEl) $tblMasterEl.destroy();
    $tblMasterEl = new initEJ2Grid({
        tableId: tblMasterId,
        autofitColumns: false,
        apiEndPoint: `/GFPApi/greyFabricProductionMasterData?gFPStatus=${GFPStatus}`,
        columns: columns,
        commandClick: handleCommands
    }); 
}
function handleCommands(args) {
    $("#GFPID").val(args.rowData.GFPID);

    getGreyFabricProductionMasterEdit(args.rowData.GFPID);

    $("#divNewGreyFabricProduction").fadeIn();
    $("#FabricBookingInfo").fadeIn();
    $("#divtblGreyFabricProduction").fadeOut();
    $("#divGreyFabricProductionButtonExecutions").fadeIn();
    $("#btnAddOrders").fadeOut();
    $("#btnApprovedGFP").fadeIn();
}

function initPendingGreyFabricProductionMasterTable() {
    var columns = [
        {
            headerText: 'Commands', commands: [
                { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } }]
        }, 
        {
            field: 'BookingNo', headerText: 'Booking No'
        },
        {
            field: 'ExportOrderNo', headerText: 'Export Order No'
        },
        {
            field: 'BuyerName', headerText: 'Buyer'
        },
        {
            field: 'BuyerTeamName', headerText: 'Buyer Team'
        },
        {
            field: 'StyleNo', headerText: 'Style No', visible: !isAllChecked
        },
        {
            field: 'BookingDate', headerText: 'Booking Date', visible: !isAllChecked
        } 
    ];
    if ($tblMasterEl) $tblMasterEl.destroy();
    $tblMasterEl = new initEJ2Grid({
        tableId: tblMasterId,
        autofitColumns: false,
        apiEndPoint: `/GFPApi/fabricBookingPendingData?gFPStatus=${1}`,
        columns: columns,
        onDblClickRow: handleCommands2
       
    }); 
}
function handleCommands2(args) {
    $("#GreyFabricProductionModal-Child").modal('hide');
    formGreyFabricProduction.find("#ExportOrderID").val(args.rowData.ExportOrderID);
    formGreyFabricProduction.find("#ExportOrderNo").val(args.rowData.BookingNo);
    formGreyFabricProduction.find("#BookingID").val(args.rowData.BookingID);
    getGreyFabricProductionMaster(args.rowData.GFPID, args.rowData.BookingID);
    formGreyFabricProduction.find("#FabricBookingInfo").fadeIn();
    formGreyFabricProduction.find("#FabricBookingOrderItems").fadeIn(); 
}


function getGreyFabricProductionMaster(id, bookingId) {
    var url = "";
    url = "/GFPApi/fabricBookingInformation/" + bookingId;
    axios.get(url)
        .then(function (response) {
            //greyFabricProductionChildList = [];
            var data = response.data;
            formGreyFabricProduction.find("#GFPID").val(data.GFPID);
            formGreyFabricProduction.find("#BuyerID").val(data.BuyerID);
            formGreyFabricProduction.find("#BuyerTeamID").val(data.BuyerTeamID);
            formGreyFabricProduction.find("#CompanyID").val(data.CompanyID);
            formGreyFabricProduction.find("#ExportOrderID").val(data.ExportOrderID);
            formGreyFabricProduction.find("#BookingID").val(data.BookingID);
            formGreyFabricProduction.find("#SubGroupID").val(data.SubGroupID);
            formGreyFabricProduction.find("#RevisionNo").val(data.RevisionNo);
            formGreyFabricProduction.find("#YInHouseDate").val(data.YInHouseDate);
            formGreyFabricProduction.find("#YRequiredDate").val(data.YRequiredDate);
            formGreyFabricProduction.find("#Remarks").val(data.Remarks);
            formGreyFabricProduction.find("#lblBookingNo").val(data.BookingNo);
            formGreyFabricProduction.find("#lblExportOrderNo").val(data.ExportOrderNo);
            formGreyFabricProduction.find("#lblBuyerName").val(data.BuyerName);
            formGreyFabricProduction.find("#lblBuyerTeam").val(data.BuyerTeamName);
            formGreyFabricProduction.find("#lblMerchandiserName").val(data.MerchandiserName);
            if (data.AcknowledgeDate)
                formGreyFabricProduction.find("#lblAcknowledgeDate1").val(moment(data.AcknowledgeDate).format('DD/MM/YYYY'));
            if (data.BookingDate)
                formGreyFabricProduction.find("#lblBookingDate").val(moment(data.BookingDate).format('DD/MM/YYYY'));
            formGreyFabricProduction.find("#lblStyleNo").val(data.StyleNo);
        })
        .catch(function () {
            toastr.error(constants.LOAD_ERROR_MESSAGE);
        })
}

function getGreyFabricProductionMasterEdit(id) {
    var url = "";
    url = "/GFPApi/greyFabricProductionMasterEdit/" + formGreyFabricProduction.find("#Id").val();
    axios.get(url)
        .then(function (response) {
            greyFabricProductionChildList = [];
            var data = response.data;
            formGreyFabricProduction.find("#GFPID").val(data.GFPID);
            formGreyFabricProduction.find("#BuyerID").val(data.BuyerID);
            formGreyFabricProduction.find("#BuyerTeamID").val(data.BuyerTeamID);
            formGreyFabricProduction.find("#CompanyID").val(data.CompanyID);
            formGreyFabricProduction.find("#ExportOrderID").val(data.ExportOrderID);
            formGreyFabricProduction.find("#BookingID").val(data.BookingID);
            formGreyFabricProduction.find("#SubGroupID").val(data.SubGroupID);
            formGreyFabricProduction.find("#RevisionNo").val(data.RevisionNo);
            formGreyFabricProduction.find("#YInHouseDate").val(data.YInHouseDate);
            formGreyFabricProduction.find("#YRequiredDate").val(data.YRequiredDate);
            formGreyFabricProduction.find("#Remarks").val(data.Remarks);
            formGreyFabricProduction.find("#lblBookingNo").text(data.BookingNo);
            formGreyFabricProduction.find("#lblExportOrderNo").text(data.ExportOrderNo);
            formGreyFabricProduction.find("#lblBuyerName").text(data.BuyerName);
            formGreyFabricProduction.find("#lblBuyerTeam").text(data.BuyerTeamName);
            formGreyFabricProduction.find("#lblMerchandiserName").text(data.MerchandiserName);
            if (data.AcknowledgeDate)
                formGreyFabricProduction.find("#lblAcknowledgeDate1").text(moment(data.AcknowledgeDate).format('DD/MM/YYYY'));
            if (data.BookingDate)
                formGreyFabricProduction.find("#lblBookingDate").text(moment(data.BookingDate).format('DD/MM/YYYY'));
            formGreyFabricProduction.find("#lblStyleNo").text(data.StyleNo);

            formGreyFabricProduction.find("#GFPNo").val(data.GFPNo);
            formGreyFabricProduction.find("#GFPDate").val(data.GfpDateStr);
            formGreyFabricProduction.find("#ExportOrderNo").val(data.BookingNo);

            formGreyFabricProduction.find("#tblGreyFabricProductionChilds").bootstrapTable('load', response.data.GreyFabricProductionChilds);
        })
        .catch(function () {
            toastr.error(constants.LOAD_ERROR_MESSAGE);
        })
}

function getPendingGreyFabricProductionMasterDataMain() {
    //var queryParams = $.param(tableParams);
    //$('#tblGreyFabricProductionMain').bootstrapTable('showLoading');
    //var url = "/GFPApi/greyFabricProductionMasterData" + "?gFPStatus=" + GFPStatus + "&" + queryParams;
    //axios.get(url)
    //    .then(function (response) {
    //        $("#tblGreyFabricProductionMain").bootstrapTable('load', response.data);
    //        $('#tblGreyFabricProductionMain').bootstrapTable('hideLoading');
    //    })
    //    .catch(function (err) {
    //        toastr.error(err.response.data.Message);
    //    })
}

function getPendingGreyFabricProductionMasterData() {
    //var queryParams = $.param(tableParams);
    //$('#tblGreyFabricProduction').bootstrapTable('showLoading');
    //var url = "/GFPApi/fabricBookingPendingData" + "?gFPStatus=" + 1 + "&" + queryParams;
    //axios.get(url)
    //    .then(function (response) {
    //        $("#tblGreyFabricProduction").bootstrapTable('load', response.data);
    //        $('#tblGreyFabricProduction').bootstrapTable('hideLoading');
    //    })
    //    .catch(function (err) {
    //        toastr.error(err.response.data.Message);
    //    })
}

function getGreyFabricProductionChildData(bookingId) {
    var url = "";
    var id = formGreyFabricProduction.find("#GFPID").val();
    if (id > 0)
        url = "/api/greyFabricProductionChild/" + id;
    else
        url = "/GFPApi/fabricBookingChildData/" + formGreyFabricProduction.find("#BookingID").val();
    axios.get(url)
        .then(function (response) {
            GreyFabricProductionChildsPopUp = response.data;
            $("#tblGreyFabricProductionChildsPopUp").bootstrapTable('load', response.data);
        })
        .catch(function () {
            toastr.error(constants.LOAD_ERROR_MESSAGE);
        })
}

function initTblGreyFabricProductionChildItems() {
    formGreyFabricProduction.find("#tblGreyFabricProductionChilds").bootstrapTable({
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
            {
                field: "BookingQty",
                title: "Booking Qty",
                align: "right",
                filterControl: "input",
                footerFormatter: calculateGreyFabricProductionTotalBookingQty
            },
            {
                field: "DisplayUnitDesc",
                title: "Unit",
                align: "left",
                filterControl: "input"
            },
            {
                field: "ProducedQty",
                title: "Produced Qty",
                align: "right",
                filterControl: "input",
                footerFormatter: calculateGreyFabricAlreadyProducedQty
            },
            {
                field: "BalanceQty",
                title: "Balance Qty",
                align: "right",
                filterControl: "input",
                footerFormatter: calculateGreyFabricProductionBalanceQty
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
                footerFormatter: calculateGreyFabricProductionTotalProductionQty
            },
            {
                field: "ProductionExcessQty",
                title: "Excess Qty",
                align: "right",
                filterControl: "input",
                footerFormatter: calculateGreyFabricProductionTotalExcessQty
            }
        ],
        onEditableSave: function (field, row, oldValue, $el) {
            row.ProductionExcessQty = (row.ProductionQty - row.BookingQty).toFixed(4);
            row.BalanceQty = (row.BookingQty - row.ProductionQty).toFixed(4);

            formGreyFabricProduction.find("#tblGreyFabricProductionChilds").bootstrapTable('load', GreyFabricProductionChilds);
        },
        onExpandRow: function (index, row, $detail) {
            if (formGreyFabricProduction.find("#GFPID").val() > 0) {
                formGreyFabricProduction.find("#GFPChildID").val(row.GFPChildID);
                var url = "/GFPApi/greyFabricProductionChildRollEdit/" + $("#GFPChildID").val();
                axios.get(url)
                    .then(function (response) {
                        $childTableEl = $("#tblGreyFabricProductionChildRoll-" + row.GFPChildID);
                        var data = $childTableEl.bootstrapTable("getData");
                        data.push(response.data);
                        $childTableEl.bootstrapTable("load", data);

                        initGreyFabricProductionChildRolls(index, data);
                    })
                    .catch(function () {
                        toastr.error(constants.LOAD_ERROR_MESSAGE);
                    })
            }
            else {
                $childTableEl = $detail.html('<table id="tblGreyFabricProductionChildRoll-' + index + '"></table>').find('table');
                //var newGreyFabricProductionChildRollData = {
                //    RollNo: "",
                //    RollQtyInKG: 0.00,
                //    LengthInInch: 0.00,
                //    WidthInInch: 0.00
                //};
                var data = [];

                var greyFabricChild = row.BookingChildID;
                $("#BookingChildID").val(greyFabricChild);

                initGreyFabricProductionChildRolls(index, data);

                var LastSavedRollNoIni = "0";
                $("#LastSavedRollNo").val(LastSavedRollNoIni);
                var RollNoIni = "1";
                $("#RollNo").val(RollNoIni);
            }
        }
    });
}

function initTblGreyFabricProductionChildItemsPopUp() {
    $("#tblGreyFabricProductionChildsPopUp").bootstrapTable({
        showFooter: true,
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
            {
                field: "BookingQty",
                title: "Booking Qty",
                align: "right",
                filterControl: "input",
                footerFormatter: calculateGreyFabricProductionTotalBookingQty
            },
            {
                field: "DisplayUnitDesc",
                title: "Unit",
                align: "left",
                filterControl: "input"
            }
        ],
        onDblClickRow: function (row, $element, field) {
            $("#modal-booking-order-Items").modal('hide');
            GreyFabricProductionChilds.push(row);
            $("#FabricBookingOrderItems").fadeIn();
            initTblGreyFabricProductionChildItems();
            $("#tblGreyFabricProductionChilds").bootstrapTable('load', GreyFabricProductionChilds);
        }
    });
}

function addNewChildRow(e, rowId) {
    $("#GreyFabricProductionModal-Child-roll").modal('show');
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
}

function initGreyFabricProductionChildRolls(rowId, data) { 
    if ($tblChildEl) $tblChildEl.destroy(); 

    var columns = [
        { field: 'GFPChildRollID', isPrimaryKey: true, visible: false }, 
        { field: 'RollNo', headerText: 'Roll No' },
        { field: 'LengthInInch', headerText: 'Length (Inch)' },
        { field: 'WidthInInch', headerText: 'Width (Inch)' },
        { field: 'RollQtyInKG', headerText: 'Roll Qty (KG)', footerFormatter: calculateGreyFabricProductionTotalRollQty },
    ];
    columns.push.apply(columns, additionalColumns);
    var tableOptions = {
        tableId: tblChildId,
        data: data,
        columns: columns,
        /*showTimeIndicator :false,*/
        actionBegin: function (args) {
            //console.log("requestType");
            //console.log(args.requestType);
            //if (args.requestType === "add") {
            //    args.data.GFPChildRollID = getMaxIdForArray(masterData.Childs, "GFPChildRollID"); 
            //    console.log(args.data);
            //} 
        },
        //commandClick: childCommandClick,
        autofitColumns: false,
        showDefaultToolbar: false,
        allowFiltering: false,
        allowPaging: false
    };

    
    $tblChildEl = new initEJ2Grid(tableOptions);

    //$childTableEl.bootstrapTable({
    //    showFooter: true,
    //    data: data,
    //    rowStyle: function (row, index) {
    //        if (row.EntityState == 8)
    //            return { classes: 'deleted-row' };

    //        return "";
    //    },
    //    columns: [
    //        {
    //            title: 'Actions',
    //            align: 'center',
    //            width: 120,
    //            formatter: function () {
    //                return [
    //                    '<span class="btn-group">',
    //                    '<a class="btn btn-danger btn-xs remove" href="javascript:void(0)" title="Delete Item">',
    //                    '<i class="fa fa-remove" style="font-size:15px"></i>',
    //                    '</a>',
    //                    '</span>'
    //                ].join('');
    //            },
    //            footerFormatter: function () {
    //                return [
    //                    '<span class="btn-group">',
    //                    '<button class="btn btn-success btn-xs edit" onclick="return addNewChildRow(event, ' + rowId + ')" title="Add">',
    //                    '<i class="fa fa-plus" style="font-size:15px"></i>',
    //                    ' Add',
    //                    '</button>',
    //                    '</span>'
    //                ].join('');
    //            },
    //            events: {
    //                'click .remove': function (e, value, row, index) {
    //                    this.data[index].EntityState = 8;
    //                    var $target = $(e.target);
    //                    $target.closest("tr").addClass('deleted-row');
    //                }
    //            }
    //        },
    //        {
    //            field: "RollNo",
    //            title: "Roll No",
    //            filterControl: "input"
    //        },
    //        {
    //            field: "LengthInInch",
    //            title: "Length (Inch)",
    //            filterControl: "input",
    //            editable: {
    //                type: 'text',
    //                inputclass: 'input-sm',
    //                showbuttons: false
    //            }
    //        },
    //        {
    //            field: "WidthInInch",
    //            title: "Width (Inch)",
    //            filterControl: "input",
    //            editable: {
    //                type: 'text',
    //                inputclass: 'input-sm',
    //                showbuttons: false
    //            }
    //        },
    //        {
    //            field: "RollQtyInKG",
    //            title: "Roll Qty (KG)",
    //            filterControl: "input",
    //            editable: {
    //                type: 'text',
    //                inputclass: 'input-sm',
    //                showbuttons: false
    //            },
    //            footerFormatter: calculateGreyFabricProductionTotalRollQty
    //        }
    //    ]
    //});
}

function GreyFabricProductionbackToList() {
    $("#divNewGreyFabricProduction").fadeOut();
    $("#divtblGreyFabricProduction").fadeIn();
    $("#divGreyFabricProductionButtonExecutions").fadeOut();
    getPendingGreyFabricProductionMasterDataMain();
}

//function resetTableParamsGFP() {
//    tableParams.offset = 0;
//    tableParams.limit = 10;
//    tableParams.filter = '';
//    tableParams.sort = '';
//    tableParams.order = '';
//}

function calculateGreyFabricProductionTotalBookingQty(data) {
    var BookingQty = 0;

    $.each(data, function (i, row) {
        BookingQty += isNaN(parseFloat(row.BookingQty)) ? 0 : parseFloat(row.BookingQty);
    });

    return BookingQty.toFixed(4);
}

function calculateGreyFabricAlreadyProducedQty(data) {
    var ProducedQty = 0;

    $.each(data, function (i, row) {
        ProducedQty += isNaN(parseFloat(row.ProducedQty)) ? 0 : parseFloat(row.ProducedQty);
    });

    return ProducedQty.toFixed(4);
}

function calculateGreyFabricProductionBalanceQty(data) {
    var BalanceQty = 0;

    $.each(data, function (i, row) {
        BalanceQty += isNaN(parseFloat(row.BalanceQty)) ? 0 : parseFloat(row.BalanceQty);
    });

    return BalanceQty.toFixed(4);
}

function calculateGreyFabricProductionTotalProductionQty(data) {
    var ProductionQty = 0;

    $.each(data, function (i, row) {
        ProductionQty += isNaN(parseFloat(row.ProductionQty)) ? 0 : parseFloat(row.ProductionQty);
    });

    return ProductionQty.toFixed(4);
}

function calculateGreyFabricProductionTotalExcessQty(data) {
    var ProductionExcessQty = 0;

    $.each(data, function (i, row) {
        ProductionExcessQty += isNaN(parseFloat(row.ProductionExcessQty)) ? 0 : parseFloat(row.ProductionExcessQty);
    });

    return ProductionExcessQty.toFixed(4);
}

function calculateGreyFabricProductionTotalRollQty(data) {
    var RollQtyInKG = 0;

    $.each(data, function (i, row) {
        RollQtyInKG += isNaN(parseFloat(row.RollQtyInKG)) ? 0 : parseFloat(row.RollQtyInKG);
    });

    return RollQtyInKG.toFixed(4);
}