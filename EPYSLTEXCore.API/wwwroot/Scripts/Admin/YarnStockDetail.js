(function () {
    var menuId, menuParam, pageId, $pageEl, pageName = "YSA";
    var $tblMasterEl, $formEl, tblMasterId, tblChildId, $tblChildEl;
    var $divTblEl, $divDetailsEl;
    var status, $toolbarEl;
    var tblMain = $("#tblMain_YarnStockDetail");
    var tblMainTbody = $("#tblMain_YarnStockDetail").find("tbody");

    var _cssObj = {
        TopHeaderCSS: "height: 34px;min-width: 170px;text-align: center; padding: 5px; background-color: cornflowerblue; color: #ffffff; font-weight: bold; font-size: 18px !important; ",
        CSS2_Green: "height: 34px;min-width: 170px;text-align: center; font-weight: bold; background-color: #6C960C; color: #ffffff;",
        CSS3: "height: 34px;min-width: 170px;text-align: center;",
        CSS4_Red: "height: 34px;min-width: 170px;text-align: center; font-weight: bold; background-color: #DB261D; color: #ffffff;",
        CSS5: "height: 34px;min-width: 170px;text-align: center; font-weight: bold; background-color: #B07315; color: #ffffff;",
        CSS6_Bold: "height: 34px;min-width: 170px;text-align: center; font-weight: bold;"
    };

    var _childDisplayHeaders = [
        //"Stock From",
        "Serial",
        "-",
        "Number",
        "Transection Type",
        "Transection Date",
        "Stock Type",
        "Rack",
        "Qty",
        "Cone",
        "Cartoon",
        "Rate",
        "Block Advance",
        "Block Sample",
        "Block Allocated",
        "Block Leftover",
        "Block Liabilities",
        "Block Pipeline",
        "Transection By"
    ];

    var _childProps = [
        //"StockFromMenu",
        "SLNo",
        "StockFromMasterType",
        "StockFromMasterNo",
        "TransectionTypeName",
        "TransectionDate",
        "StockTypeName",
        "RackNo",
        "Qty",
        "Cone",
        "Cartoon",
        "Rate",
        "BlockAdvanceStockQty",
        "BlockSampleStockQty",
        "BlockAllocatedStockQty",
        "BlockLeftoverStockQty",
        "BlockLiabilitiesStockQty",
        "BlockPipelineStockQty",
        "TransectionByName"
    ];

    $(function () {
        if (!menuId) menuId = localStorage.getItem("menuId");
        if (!menuParam) menuParam = localStorage.getItem("menuParam");

        pageId = pageName + "-" + menuId;
        $pageEl = $(`#${pageId}`);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
        tblChildId = pageConstants.CHILD_TBL_ID_PREFIX + pageId;
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);

        $toolbarEl.find("#btnYanrStockItems").click(function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PENDING;
            initMasterTable();
        });
        $formEl.find("#btnBackToList").click(function () {
            backToList();
        });

        $toolbarEl.find("#btnYanrStockItems").click();

        $formEl.find("#btnGenerateExcel").click(function () {
            generateGrid();
            tblMain.table2excel({
                filename: "Transection History of item " + masterData.YarnCategory
            });
        });
    });
    function initMasterTable() {
        var commands = [
            { type: 'View', title: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } }
        ]

        var columns = [
            {
                headerText: '', commands: commands, textAlign: 'Center', width: ch_setActionCommandCellWidth(commands)
            },
            {
                field: 'YarnStockSetId', headerText: 'Code'
            },
            {
                field: 'YarnCategory', headerText: 'Item'
            },
            {
                field: 'Supplier', headerText: 'Supplier'
            },
            {
                field: 'Spinner', headerText: 'Spinner'
            },
            {
                field: 'YarnLotNo', headerText: 'Lot No'
            },
            {
                field: 'PhysicalCount', headerText: 'Physical Count'
            },
            {
                field: 'ShadeCode', headerText: 'Shade Code'
            },
            {
                field: 'IsPipelineRecord', headerText: 'Is Pipeline', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center'
            },
            {
                field: 'PipelineStockQty', headerText: 'Pipeline Stock'
            },
            {
                field: 'QuarantineStockQty', headerText: 'Quarantine Stock'
            },
            {
                field: 'TotalIssueQty', headerText: 'Issue Stock'
            },
            {
                field: 'AdvanceStockQty', headerText: 'Advance Stock'
            },
            {
                field: 'AllocatedStockQty', headerText: 'Allocated Stock'
            },
            {
                field: 'SampleStockQty', headerText: 'Sample Stock'
            },
            {
                field: 'LeftoverStockQty', headerText: 'Leftover Stock'
            },
            {
                field: 'LiabilitiesStockQty', headerText: 'Liabilities Stock'
            },
            {
                field: 'UnusableStockQty', headerText: 'Unusable Stock'
            },
            {
                field: 'BlockPipelineStockQty', headerText: 'Blocked Pipeline Stock'
            },
            {
                field: 'BlockAdvanceStockQty', headerText: 'Blocked Advance Stock'
            },
            {
                field: 'BlockSampleStockQty', headerText: 'Blocked Sample Stock'
            },
            {
                field: 'BlockLeftoverStockQty', headerText: 'Blocked Leftover Stock'
            },
            {
                field: 'BlockLiabilitiesStockQty', headerText: 'Blocked Liabilities Stock'
            },
            {
                field: 'YarnApprovedDate', headerText: 'Yarn Approved Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1
            }
        ];

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: true,
            apiEndPoint: `/api/yarn-stock/list?status=${status}`,
            columns: columns,
            //isFilterTypeExcel: true,
            commandClick: handleCommands
        });
    }
    function handleCommands(args) {
        if (args.commandColumn.type == 'View') {
            getDetail(args.rowData.YarnStockSetId);
        }
    }
    function getDetail(yarnStockSetId) {
        var url = `/api/yarn-stock/get-details/${yarnStockSetId}`;
        axios.get(url)
            .then(function (response) {
                $formEl.find(".actionBtn1").show();
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.YarnApprovedDate = formatDateToDefault(masterData.YarnApprovedDate);

                setFormData($formEl, masterData);
                initChildTable(masterData.Childs);


                //generateGrid();
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    async function initChildTable(data) {

        var slNo = 1;
        data.map(x => {
            x.SLNo = slNo++;
        });

        var columns = [];
        var indexC = -1;
        _childProps.map(x => {
            indexC++;
            columns.push({
                field: x,
                headerText: _childDisplayHeaders[indexC],
                width: 120,
                textAlign: x == "SLNo" ? 'center' : 'left'
            });
        });
        if ($tblChildEl) $tblChildEl.destroy();

        $tblChildEl = new initEJ2Grid({
            tableId: tblChildId,
            autofitColumns: true,
            data: data,
            columns: columns
        });
    }
    function backToList() {
        $formEl.find(".actionBtn1").hide();
        $divDetailsEl.fadeOut();
        $divTblEl.fadeIn();
    }
    function generateSingleRow(tdObjList, isTh) {
        var temp = `<tr>`;
        for (var iTd = 0; iTd < tdObjList.length; iTd++) {
            if (isTh) {
                temp += `<th colspan=` + tdObjList[iTd].ColSpan + ` style = "` + tdObjList[iTd].CSS + `">` + tdObjList[iTd].CellValue + `</th>`;
            } else {
                temp += `<td colspan=` + tdObjList[iTd].ColSpan + ` style = "` + tdObjList[iTd].CSS + `">` + tdObjList[iTd].CellValue + `</td>`;
            }
        }
        temp += `</tr>`;
        tblMainTbody.append(temp);
    }

    function generateGrid() {
        tblMainTbody.find("th,td").remove();
        generateTitleRow(masterData.YarnCategory);
        generateBasicItemInfoTitleRows();
        generateTitleRow("Stock Quantities");
        generateSummaryQtyRows();
        generateTitleRow("Transection History");
        generateTransectionHistoryRows();
    }
    function generateTitleRow(cellValue) {
        var tdObjList = [];
        tdObjList.push({
            ColSpan: _childProps.length,
            CSS: _cssObj.TopHeaderCSS,
            CellValue: cellValue
        });
        generateSingleRow(tdObjList, true);
    }
    function generateBasicItemInfoTitleRows() {
        var itemTotalBasicInfoHeaders = [
            "Supplier",
            "Spinner",
            "Physical Count",
            "Lot No"
        ];
        var itemTotalBasicInfoHeaderProps = [
            "Supplier",
            "Spinner",
            "PhysicalCount",
            "YarnLotNo"
        ];

        var maxColSpan = _childProps.length;
        var perCellColSpan = Math.floor(maxColSpan / itemTotalBasicInfoHeaders.length);
        var colSpanUsed = 0;
        var loopCount = 0;

        var tdObjList = [];
        for (var i = 0; i < itemTotalBasicInfoHeaders.length; i++) {
            loopCount++;
            colSpanUsed += perCellColSpan;
            if (loopCount == itemTotalBasicInfoHeaders.length) {
                perCellColSpan = perCellColSpan + (maxColSpan - colSpanUsed);
            }
            tdObjList.push({
                ColSpan: perCellColSpan,
                CSS: _cssObj.CSS2_Green,
                CellValue: itemTotalBasicInfoHeaders[i]
            });
        }
        generateSingleRow(tdObjList, false);

        perCellColSpan = Math.floor(maxColSpan / itemTotalBasicInfoHeaderProps.length);
        colSpanUsed = 0;
        loopCount = 0;

        tdObjList = [];
        for (var i = 0; i < itemTotalBasicInfoHeaderProps.length; i++) {
            loopCount++;
            colSpanUsed += perCellColSpan;
            if (loopCount == itemTotalBasicInfoHeaderProps.length) {
                perCellColSpan = perCellColSpan + (maxColSpan - colSpanUsed);
            }
            tdObjList.push({
                ColSpan: perCellColSpan,
                CSS: _cssObj.CSS3,
                CellValue: masterData[itemTotalBasicInfoHeaderProps[i]]
            });
        }
        generateSingleRow(tdObjList, false);
    }
    function generateSummaryQtyRows() {
        var qtySet1 = [
            "Quarantine",
            "Advance",
            "Allocated",
            "Sample",
            "Issue",
            "Leftover",
            "Liabilities",
            "Unusable",
            "Pipeline"
        ];
        var qtySet1_Props = [
            "QuarantineStockQty",
            "AdvanceStockQty",
            "AllocatedStockQty",
            "SampleStockQty",
            "TotalIssueQty",
            "LeftoverStockQty",
            "LiabilitiesStockQty",
            "UnusableStockQty",
            "PipelineStockQty"
        ];

        var qtySet2 = [
            "Block Pipeline",
            "Block Advance",
            "Block Allocated",
            "Block Sample",
            "Block Leftover",
            "Block Liabilities"
        ];
        var qtySet2_Props = [
            "BlockPipelineStockQty",
            "BlockAdvanceStockQty",
            "BlockAllocatedStockQty",
            "BlockSampleStockQty",
            "BlockLeftoverStockQty",
            "BlockLiabilitiesStockQty"
        ];

        for (var iSN = 1; iSN <= 2; iSN++) {
            if (iSN == 1) {
                generateSetNoWiseRows(qtySet1, qtySet1_Props, _cssObj.CSS5);
            }
            else if (iSN == 2) {
                generateSetNoWiseRows(qtySet2, qtySet2_Props, _cssObj.CSS5);
            }
        }
    }
    function generateSetNoWiseRows(headerSet, valueSet, headerCSSValue) {
        var maxColSpan = _childProps.length;
        var perCellColSpan = Math.floor(maxColSpan / headerSet.length);
        var colSpanUsed = 0;
        var loopCount = 0;

        var tdObjList = [];
        for (var i = 0; i < headerSet.length; i++) {
            loopCount++;
            colSpanUsed += perCellColSpan;
            if (loopCount == headerSet.length) {
                perCellColSpan = perCellColSpan + (maxColSpan - colSpanUsed);
            }
            tdObjList.push({
                ColSpan: perCellColSpan,
                CSS: headerCSSValue,
                CellValue: headerSet[i]
            });
        }
        generateSingleRow(tdObjList, false);


        perCellColSpan = Math.floor(maxColSpan / valueSet.length);
        colSpanUsed = 0;
        loopCount = 0;

        tdObjList = [];
        for (var i = 0; i < valueSet.length; i++) {
            loopCount++;
            colSpanUsed += perCellColSpan;
            if (loopCount == valueSet.length) {
                perCellColSpan = perCellColSpan + (maxColSpan - colSpanUsed);
            }
            tdObjList.push({
                ColSpan: perCellColSpan,
                CSS: _cssObj.CSS3,
                CellValue: typeof masterData[valueSet[i]] !== "undefined" ? masterData[valueSet[i]] : valueSet[i]
            });
        }
        generateSingleRow(tdObjList, false);
    }
    function generateTransectionHistoryRows() {
        var tdObjList = [];
        _childDisplayHeaders.map(x => {
            tdObjList.push({
                ColSpan: 1,
                CSS: _cssObj.CSS2_Green,
                CellValue: x
            });
        });
        generateSingleRow(tdObjList, false);

        masterData.Childs.map(child => {
            tdObjList = [];
            _childProps.map(x => {
                tdObjList.push({
                    ColSpan: 1,
                    CSS: _cssObj.CSS3,
                    CellValue: child[x]
                });
            });
            generateSingleRow(tdObjList, false);
        });

        if (masterData.Childs.length == 0) {
            tdObjList = [];
            tdObjList.push({
                ColSpan: _childProps.length,
                CSS: _cssObj.CSS3,
                CellValue: "NO TRANSECTION HISTORY FOUND"
            });
            generateSingleRow(tdObjList, false);
        }
    }
})();