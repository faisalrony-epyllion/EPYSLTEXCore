'use strict'
function CM_Test() {
    alert("From Common-methods");
}
//async function getLocalOrImportTypes() {
//    await axios.get(`/api/graph/get-summay-block-records/${getParam()}`).then(res => {
//        var obj = res.data;
//        $("#spnBlock1").text(obj.PropS1);
//        $("#spnBlock2").text(obj.PropS2);
//        $("#spnBlock3").text(obj.PropS3);
//        $("#spnBlock4").text(obj.PropS4);
//    });
//}
function CM_StockOperation_GetAllStocks_For_View(divDisplayAllStock, objYSS, modalTitle) {
    /*
      objYSS param formate
      
      var objYSS = {
        YarnCategory = "";
        PhysicalCount = "";
        YarnLotNo = "";
        ShadeCode = "";
        Supplier = "";
        Spinner = "";
        Count = "";
      };

     */

    if (getDefaultValueWhenInvalidS(modalTitle) == 0) modalTitle = "Item List"

    if (typeof objYSS === "undefined" || objYSS == null) {
        var objYSS = {};
        objYSS.YarnCategory = "";
        objYSS.PhysicalCount = "";
        objYSS.YarnLotNo = "";
        objYSS.ShadeCode = "";
        objYSS.Supplier = "";
        objYSS.Spinner = "";
        objYSS.Count = "";
    } else {
        objYSS.YarnCategory = getDefaultValueWhenInvalidS(objYSS.YarnCategory);
        objYSS.PhysicalCount = getDefaultValueWhenInvalidS(objYSS.PhysicalCount);
        objYSS.YarnLotNo = getDefaultValueWhenInvalidS(objYSS.YarnLotNo);
        objYSS.ShadeCode = getDefaultValueWhenInvalidS(objYSS.ShadeCode);
        objYSS.Supplier = getDefaultValueWhenInvalidS(objYSS.Supplier);
        objYSS.Spinner = getDefaultValueWhenInvalidS(objYSS.Spinner);
        objYSS.Count = getDefaultValueWhenInvalidS(objYSS.Count);
    }

    objYSS.YarnCategory = replaceInvalidChar(objYSS.YarnCategory);
    objYSS.PhysicalCount = replaceInvalidChar(objYSS.PhysicalCount);
    objYSS.YarnLotNo = replaceInvalidChar(objYSS.YarnLotNo);
    objYSS.ShadeCode = replaceInvalidChar(objYSS.ShadeCode);
    objYSS.Supplier = replaceInvalidChar(objYSS.Supplier);
    objYSS.Spinner = replaceInvalidChar(objYSS.Spinner);
    objYSS.Count = replaceInvalidChar(objYSS.Count);


    var $tblAllStockYSSEl;
    var parentDiv = $(divDisplayAllStock);
    parentDiv.css({
        "float": "right"
    });
    var guidValue = ch_getNewGuid();

    var modalId = "modal" + guidValue;
    var titleSpanId = "titleSpan" + guidValue;
    var stockTableId = "stockTable" + guidValue;

    var btnId = "btn" + guidValue;
    $(".displayAllStockYSS").remove();
    parentDiv.append(`<button type="button" class="btn btn-sm btn-success displayAllStockYSS" id="${btnId}" title='Show Stocks'
                        YarnCategory = "${objYSS.YarnCategory}"
                        PhysicalCount = "${objYSS.PhysicalCount}"
                        YarnLotNo = "${objYSS.YarnLotNo}"
                        ShadeCode = "${objYSS.ShadeCode}"
                        Supplier = "${objYSS.Supplier}"
                        Spinner = "${objYSS.Spinner}"
                        Count = "${objYSS.Count}"
                     >
                            <i class="fa fa-eye"></i>
                     </button>`);
    btnId = "#" + btnId;

    $(btnId).closest('form').append(`
        <div class="modal fade displayAllStockYSS" id="${modalId}">
                <div class="modal-dialog modal-lg">
                    <div class="modal-content">
                        <div class="modal-header" style="text-align:center;" onmousedown="if (drag) drag(this.parentNode, event)">
                            <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                                <span aria-hidden="true">×</span>
                            </button>
                        </div>
                        <div class="modal-body">
                            <div class="col-sm-12">
                                <div class="panel panel-success" style="border-color:transparent; border-style: none;">
                                    <div class="panel-heading">
                                        <label class="lblTableTitle">${modalTitle}</label>
                                    </div>
                                    <div class="panel-body">
                                        <div class="form-horizontal">
                                            <div class="col-sm-12">
                                                <div class="form-group">
                                                    <table id="${stockTableId}"></table>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-default" id="btnClose" data-dismiss="modal">Close</button>
                        </div>
                    </div>
                </div>
            </div>
        `);

    modalId = "#" + modalId;
    titleSpanId = "#" + titleSpanId;
    stockTableId = "#" + stockTableId;

    parentDiv.find(btnId).click(function () {
        $(modalId).modal('show');

        var objYSS = {};
        objYSS.YarnCategory = $(btnId).attr("YarnCategory");
        objYSS.PhysicalCount = $(btnId).attr("PhysicalCount");
        objYSS.YarnLotNo = $(btnId).attr("YarnLotNo");
        objYSS.ShadeCode = $(btnId).attr("ShadeCode");
        objYSS.Supplier = $(btnId).attr("Supplier");
        objYSS.Spinner = $(btnId).attr("Spinner");
        objYSS.Count = $(btnId).attr("Count");

        var columnList = [
            {
                field: 'YarnStockSetId', headerText: 'Set Id', width: 10, isPrimaryKey: true, visible: false
            },
            {
                field: 'YarnCategory', headerText: 'Yarn Details', width: 150
            },
            {
                field: 'Count', headerText: 'Count', width: 80
            },
            {
                field: 'PhysicalCount', headerText: 'Physical Count', width: 80
            },
            {
                field: 'YarnLotNo', headerText: 'Lot No', width: 80
            },
            {
                field: 'ShadeCode', headerText: 'Shade Code', width: 100
            },
            {
                field: 'SupplierName', headerText: 'Supplier', width: 100
            },
            {
                field: 'SpinnerName', headerText: 'Spinner', width: 100
            },
            //{
            //    field: 'QuarantineStockQty', headerText: 'Quarantine Stock', width: 100
            //},
            {
                field: 'AdvanceStockQty', headerText: 'Advance Stock', width: 100
            },
            {
                field: 'AllocatedStockQty', headerText: 'Allocated Stock', width: 100
            },
            {
                field: 'SampleStockQty', headerText: 'Sample Stock', width: 100
            },
            {
                field: 'LiabilitiesStockQty', headerText: 'Liabilities Stock', width: 100
            },
            {
                field: 'LeftoverStockQty', headerText: 'Leftover Stock', width: 100
            },
            {
                field: 'UnusableStockQty', headerText: 'Unusable Stock', width: 100
            },
        ];
        var apiUrl = `/api/yarn-stock-adjustment/get-all-stocks/${objYSS.YarnCategory}/${objYSS.PhysicalCount}/${objYSS.YarnLotNo}/${objYSS.ShadeCode}/${objYSS.Supplier}/${objYSS.Spinner}/${objYSS.Count}`;
        if ($tblAllStockYSSEl) $tblAllStockYSSEl.destroy();
        $tblAllStockYSSEl = new initEJ2Grid({
            tableId: stockTableId,
            autofitColumns: true,
            apiEndPoint: apiUrl,
            columns: columnList,
            allowSorting: true,
            editSettings: { allowAdding: false, allowEditing: false, allowDeleting: false, mode: "Normal" },
        });
    });
}
function CM_StockOperation_GetAllStocks_For_Multi_Select(pageId, modalTitle) {

    /*
    if (getDefaultValueWhenInvalidS(modalTitle) == 0) modalTitle = "Item List"

    var $tblAllStockYSSEl_MS;
    var guidValue = ch_getNewGuid();

    var modalId = "modal" + guidValue;
    var titleSpanId = "titleSpan" + guidValue;
    var stockTableId = "stockTable" + guidValue;

    $(".displayAllStockYSS_MS").remove();

    $("#" + pageId).find('form').append(`
        <div class="modal fade displayAllStockYSS_MS" id="${modalId}">
                <div class="modal-dialog modal-lg">
                    <div class="modal-content">
                        <div class="modal-header" style="text-align:center;" onmousedown="if (drag) drag(this.parentNode, event)">
                            <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                                <span aria-hidden="true">×</span>
                            </button>
                        </div>
                        <div class="modal-body">
                            <div class="col-sm-12">
                                <div class="panel panel-success" style="border-color:transparent; border-style: none;">
                                    <div class="panel-heading">
                                        <label class="lblTableTitle">${modalTitle}</label>
                                    </div>
                                    <div class="panel-body">
                                        <div class="form-horizontal">
                                            <div class="col-sm-12">
                                                <div class="form-group">
                                                    <table id="${stockTableId}"></table>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-default" id="btnClose" data-dismiss="modal">Close</button>
                        </div>
                    </div>
                </div>
            </div>
        `);

    modalId = "#" + modalId;
    titleSpanId = "#" + titleSpanId;
    stockTableId = "#" + stockTableId;

    $(modalId).modal('show');

    var columnList = [
        {
            field: 'YarnStockSetId', headerText: 'Set Id', width: 10, isPrimaryKey: true, visible: false
        },
        {
            field: 'YarnCategory', headerText: 'Yarn Details', width: 150
        },
        {
            field: 'Count', headerText: 'Count', width: 80
        },
        {
            field: 'PhysicalCount', headerText: 'Physical Count', width: 80
        },
        {
            field: 'YarnLotNo', headerText: 'Lot No', width: 80
        },
        {
            field: 'ShadeCode', headerText: 'Shade Code', width: 100
        },
        {
            field: 'SupplierName', headerText: 'Supplier', width: 100
        },
        {
            field: 'SpinnerName', headerText: 'Spinner', width: 100
        },
        //{
        //    field: 'QuarantineStockQty', headerText: 'Quarantine Stock', width: 100
        //},
        {
            field: 'AdvanceStockQty', headerText: 'Advance Stock', width: 100
        },
        {
            field: 'AllocatedStockQty', headerText: 'Allocated Stock', width: 100
        },
        {
            field: 'SampleStockQty', headerText: 'Sample Stock', width: 100
        },
        {
            field: 'LiabilitiesStockQty', headerText: 'Liabilities Stock', width: 100
        },
        {
            field: 'LeftoverStockQty', headerText: 'Leftover Stock', width: 100
        },
        {
            field: 'UnusableStockQty', headerText: 'Unusable Stock', width: 100
        },
    ];

    var apiUrl = `/api/yarn-stock-adjustment/get-all-stocks-without-param`;
    if ($tblAllStockYSSEl_MS) $tblAllStockYSSEl_MS.destroy();
    $tblAllStockYSSEl_MS = new initEJ2Grid({
        tableId: stockTableId,
        autofitColumns: true,
        apiEndPoint: apiUrl,
        columns: columnList,
        allowSorting: true,
        editSettings: { allowAdding: false, allowEditing: false, allowDeleting: false, mode: "Normal" },
    });
    */
}

function CM_IsValidAllYarnItems(yarnItems) {
    var hasError2 = false;
    for (var iY = 0; iY < yarnItems.length; iY++) {
        hasError2 = CM_IsValidYarnItemSingle(yarnItems[iY]);
        if (hasError2) break;
    }
    return hasError2;
}
function CM_IsValidYarnItemSingle(yarnItem) {
    yarnItem.Segment1ValueId = getDefaultValueWhenInvalidN(yarnItem.Segment1ValueId);
    yarnItem.Segment2ValueId = getDefaultValueWhenInvalidN(yarnItem.Segment2ValueId);
    yarnItem.Segment3ValueId = getDefaultValueWhenInvalidN(yarnItem.Segment3ValueId);
    yarnItem.Segment4ValueId = getDefaultValueWhenInvalidN(yarnItem.Segment4ValueId);
    yarnItem.Segment5ValueId = getDefaultValueWhenInvalidN(yarnItem.Segment5ValueId);
    yarnItem.Segment6ValueId = getDefaultValueWhenInvalidN(yarnItem.Segment6ValueId);

    yarnItem.Segment5ValueDesc = getDefaultValueWhenInvalidS(yarnItem.Segment5ValueDesc);
    yarnItem.ShadeCode = getDefaultValueWhenInvalidS(yarnItem.ShadeCode);

    if (yarnItem.Segment1ValueId == 0) {
        toastr.error("Select composition");
        return true;
    }
    if (yarnItem.Segment2ValueId == 0) {
        toastr.error("Select yarn type");
        return true;
    }
    if (yarnItem.Segment3ValueId == 0) {
        toastr.error("Select manufacturing process");
        return true;
    }
    if (yarnItem.Segment4ValueId == 0) {
        toastr.error("Select sub process");
        return true;
    }
    if (yarnItem.Segment5ValueId == 0) {
        toastr.error("Select quality parameter");
        return true;
    }
    if (yarnItem.Segment6ValueId == 0) {
        toastr.error("Select count");
        return true;
    }
    if ((yarnItem.Segment5ValueDesc.toLowerCase() == "melange" || yarnItem.Segment5ValueDesc.toLowerCase() == "color melange") && (yarnItem.ShadeCode == "")) {
        toastr.error("Select shade code for color melange");
        return true;
    }
    return false;
}

