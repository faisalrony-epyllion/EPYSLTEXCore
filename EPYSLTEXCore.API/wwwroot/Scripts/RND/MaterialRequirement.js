
(function () {
    var menuId, pageName;
    var toolbarId;
    var $divTblEl, pageId, $pageEl, $divDetailsEl, $toolbarEl, $tblMasterEl, tblMasterId, $tblColorChildEl, tblColorChildId,
        //$tblChildEl, tblChildId,
        $formEl, tblCreateCompositionId, $tblCreateCompositionEl, $tblOtherItemEl, tblOtherItemId,
        $tblFabricItemEl, tblFabricItemId, $tblYarnItemCopyIDEl, tblYarnItemCopyID, $modalYarnItemCopyIDEl;
    var status = statusConstants.PENDING;
    var isBlended = false;
    var compositionComponents = [];
    var masterData, currentChildRowData, maxCol = 999, conceptId;

    var validationConstraints = {};
    var collarYarnItem = null;
    var copyYarnItem = null;
    var _segments = null;

    $(function () {
        if (!menuId)
            menuId = localStorage.getItem("menuId");
        if (!pageName)
            pageName = localStorage.getItem("pageName");

        pageId = pageName + "-" + menuId;
        $pageEl = $(`#${pageId}`);
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
        tblChildId = pageConstants.CHILD_TBL_ID_PREFIX + pageId;
        tblColorChildId = "#tblColorChild" + pageId;
        tblFabricItemId = "#tblFabricItem" + pageId;
        tblOtherItemId = "#tblOtherItem" + pageId;
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);
        tblCreateCompositionId = `#tblCreateComposition-${pageId}`;

        tblYarnItemCopyID = "#tblYarnItemCopyID" + pageId;
        $modalYarnItemCopyIDEl = $("#modalYarnItemCopyID" + pageId);

        initMasterTable();
        $formEl.find("#addYarnComposition").on("click", function (e) {
            showAddComposition();
        });

        $toolbarEl.find("#btnList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PENDING;
            initMasterTable();
        });

        $toolbarEl.find("#btnCompleteList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.COMPLETED;
            initMasterTable();
        });

        $toolbarEl.find("#btndraftList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PARTIALLY_COMPLETED;
            initMasterTable();
        });
        $toolbarEl.find("#btnRejectList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.REJECT;
            initMasterTable();
        });

        $toolbarEl.find("#btnRevisionList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.REVISE;
            initMasterTable();
        });

        $pageEl.find('input[type=radio][name=Blended]').change(function (e) {
            e.preventDefault();
            isBlended = convertToBoolean(this.value);
            initTblCreateComposition();
            return false;
        });

        $toolbarEl.find("#btnRefreshList").on("click", function (e) {
            e.preventDefault();
            $tblMasterEl.refresh();
        });

        $pageEl.find("#btnAddComposition").click(saveComposition);

        $formEl.find("#btnSaveComplete").click(function (e) {
            e.preventDefault();
            save(true);
        });
        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save(false);
        });
        $formEl.find("#btnRevise").click(function (e) {
            e.preventDefault();
            revise(this);
        });

        $formEl.find("#btnReviseComplete").click(function (e) {
            e.preventDefault();
            revise(this);
        });

        $formEl.find("#btnAcknowledge").click(function (e) {
            e.preventDefault();
            approve(this);
        });

        $formEl.find("#btnCancel").on("click", backToListWithoutFilter);

        $formEl.find("#btnOkk").click(function (e) {
            e.preventDefault();

            copyYarnItem = $tblYarnItemCopyIDEl.getSelectedRecords();
            if (copyYarnItem.length == 0) {
                toastr.warning("Please select item(s)!");
                //e.preventDefault
                return;
            }

        });
        getSegments();
    });

    async function getSegments() {
        _segments = await axios.get(getYarnItemsApiUrl([]));
        _segments = _segments.data;
    }

    function initMasterTable() {
        var commands = [];
        if (status == statusConstants.PENDING) {
            commands = [
                { type: 'New', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-plus' } }
            ]
        }
        else if (status == statusConstants.REVISE) {
            commands = [
                { type: 'Revise', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-edit' } }
            ]
        }
        else {
            commands = [
                { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-edit' } }
            ]
        }
        var columns = [
            {
                headerText: 'Actions', commands: commands
            },
            //{
            //    field: 'Status', headerText: 'Status', visible: status == statusConstants.PENDING
            //},
            //{
            //    field: 'ConceptStatus', headerText: 'Status', visible: status != statusConstants.PENDING
            //}, 
            {
                field: 'ConceptNo', headerText: 'Concept No'
            },
            {
                field: 'ConceptDate', headerText: 'Concept Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1
            },
            {
                field: 'ConcepTypeName', headerText: 'Concept Type'
            },
            {
                field: 'KnittingType', headerText: 'Knitting Type'
            },
            {
                field: 'TechnicalName', headerText: 'Technical Name'
            },
            //{
            //    field: 'Composition', headerText: 'Composition'
            //},
            //{
            //    field: 'GSM', headerText: 'GSM'
            //},
            //{
            //    field: 'Qty', headerText: 'Qty (KG)'
            //},
            {
                field: 'MaterialRequirmentBy', headerText: 'Material Requirment By', visible: status != statusConstants.PENDING
            },
            {
                field: 'UserName', headerText: 'Concept By'
            }
        ];

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: true,
            apiEndPoint: `/api/rnd-free-concept-mr/list?status=${status}`,
            columns: columns,
            allowSorting: true,
            commandClick: handleCommands
        });
    }

    function handleCommands(args) {
        if (args.commandColumn.type == 'New') {
            getNew(args.rowData.GroupConceptNo, args.rowData.ConceptTypeID);
            $formEl.find("#btnSave").fadeIn();
            $formEl.find("#btnSaveComplete").fadeIn();
            $formEl.find("#btnAcknowledge,#btnRevise,#btnReviseComplete").fadeOut();
        }
        else if (args.commandColumn.type == 'Edit') {

            if (args.rowData) {

                getDetails(args.rowData.GroupConceptNo, args.rowData.ConceptTypeID);

                if (status == statusConstants.COMPLETED) {
                    //$formEl.find("#btnSave").fadeIn();
                    //$formEl.find("#btnSaveComplete").fadeIn();
                    $formEl.find("#btnReviseComplete").fadeIn();
                    $formEl.find("#btnSave,#btnSaveComplete,#btnAcknowledge,#btnRevise").fadeOut();
                }
                else if (status == statusConstants.REJECT) {
                    $formEl.find("#btnSave,#btnRevise,#btnSaveComplete,#btnReviseComplete").fadeOut();
                    $formEl.find("#btnRevise,#btnAcknowledge").fadeIn();
                }
                else if (status == statusConstants.PARTIALLY_COMPLETED) {
                    $formEl.find("#btnSave").fadeIn();
                    $formEl.find("#btnSaveComplete").fadeIn();
                    $formEl.find("#btnAcknowledge,#btnRevise,#btnReviseComplete").fadeOut();
                }
                else if (status == statusConstants.REVISE) {
                    $formEl.find("#btnRevise").fadeIn();
                    $formEl.find("#btnSave,#btnSaveComplete,#btnAcknowledge,#btnReviseComplete").fadeOut();
                }
            }
        }
        else if (args.commandColumn.type == 'Revise') {
            if (args.rowData) {

                getReviseDetails(args.rowData.GroupConceptNo, args.rowData.ConceptTypeID);

                if (status == statusConstants.COMPLETED) {
                    //$formEl.find("#btnSave").fadeIn();
                    //$formEl.find("#btnSaveComplete").fadeIn();
                    $formEl.find("#btnReviseComplete").fadeIn();
                    $formEl.find("#btnSave,#btnSaveComplete,#btnAcknowledge,#btnRevise").fadeOut();
                }
                else if (status == statusConstants.REJECT) {
                    $formEl.find("#btnSave,#btnRevise,#btnSaveComplete,#btnReviseComplete").fadeOut();
                    $formEl.find("#btnRevise,#btnAcknowledge").fadeIn();
                }
                else if (status == statusConstants.PARTIALLY_COMPLETED) {
                    $formEl.find("#btnSave").fadeIn();
                    //$formEl.find("#btnSaveComplete").fadeIn();
                    $formEl.find("#btnSaveComplete,#btnAcknowledge,#btnRevise,#btnReviseComplete").fadeOut();
                }
                else if (status == statusConstants.REVISE) {
                    $formEl.find("#btnRevise").fadeIn();
                    $formEl.find("#btnSave,#btnSaveComplete,#btnAcknowledge,#btnReviseComplete").fadeOut();
                }
            }
        }
    }

    function initChildTable(records) {
        if ($tblColorChildEl) $tblColorChildEl.destroy();

        $tblColorChildEl = new initEJ2Grid({
            tableId: tblColorChildId,
            data: records,
            autofitColumns: false,
            allowSorting: true,
            allowPaging: false,
            allowFiltering: false,
            columns: [
                { field: 'ColorCode', headerText: 'Code', allowEditing: false, width: 20 },
                { field: 'ColorName', headerText: 'Name', allowEditing: false, width: 20 },
                { field: 'Color', headerText: 'Visual', uid: "RGBOrHex", allowEditing: false, valueAccessor: ej2GridColorFormatter, width: 20 }
            ]
        });
    }

    async function initOtherItemTable(data) {
        if ($tblOtherItemEl) $tblOtherItemEl.destroy();

        var childColumns = await getYarnItemColumnsWithSearchDDLAsync(ch_getCountRelatedList(data, 4));
        childColumns.unshift({ field: 'FCMRChildID', isPrimaryKey: true, visible: false });
        var additionalColumns = [
            { field: 'ConceptID', visible: false },
            {
                field: 'ShadeCode'
                , headerText: 'Shade Code'
                , valueAccessor: ej2GridDisplayFormatter
                , dataSource: data.YarnShadeBooks
                , displayField: "ShadeCode"
                , edit: ej2GridDropDownObj({
                })
            },
            {
                field: 'DayValidDurationId', headerText: 'Yarn Sourcing Mode', width: 120,
                valueAccessor: ej2GridDisplayFormatter,
                dataSource: masterData.DayValidDurations,
                displayField: "text",
                edit: ej2GridDropDownObj({
                })
            },
            { field: 'YDItem', headerText: 'YD Item?', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center' },
            { field: 'YD', headerText: 'Go for YD?', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center' },
            { field: 'ReqQty', headerText: 'Req Qty(KG)', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 1 } } },
            { field: 'ReqCone', headerText: 'Req Cone(PCS)', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 1 } } },
            { field: 'IsPR', headerText: 'Go for PR?', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center' },
            //{ field: 'StockQty', headerText: 'Stock Qty(KG)', allowEditing: false }
            { field: 'PhysicalCount', headerText: 'Physical Count', width: 100, allowEditing: false },
            { field: 'YarnLotNo', headerText: 'Lot No', width: 60, allowEditing: false },
            { field: 'SpinnerName', headerText: 'Spinner', width: 100, allowEditing: false },
            { field: 'SampleStockQty', headerText: 'Sample Stock Qty', width: 100, allowEditing: false },
            { field: 'AdvanceStockQty', headerText: 'Advance Stock Qty', width: 100, allowEditing: false },
            { field: 'YarnStockSetId', headerText: 'YarnStockSetId', width: 10, allowEditing: false, visible: false }
        ];
        childColumns.push.apply(childColumns, additionalColumns);
        childColumns = setMandatoryFieldsCSS(childColumns, "Segment1ValueId, Segment6ValueId, Distribution, Allowance, ReqQty, ReqCone");

        data.OtherItems.map(x => {
            x.GetYarnFromStock = 0;
        });

        ej.base.enableRipple(true);
        $tblOtherItemEl = new ej.grids.Grid({
            dataSource: data.OtherItems,
            allowResizing: true,
            columns: [
                { field: 'ConceptID', isPrimaryKey: true, visible: false },
                { field: 'SubGroupID', visible: false },
                { field: 'GetYarnFromStock', headerText: 'From Stock', allowEditing: false, textAlign: 'center', width: 30, valueAccessor: displayStockIcon },
                { field: 'FUPartName', headerText: 'End User', width: 20, allowEditing: false },
                { field: 'MCSubClassName', headerText: 'Machine Type', width: 20, allowEditing: false },
                { field: 'TechnicalName', headerText: 'Technical Name', width: 20, allowEditing: false },
                { field: 'MachineGauge', headerText: 'Machine Gauge', width: 20, allowEditing: false },
                { field: 'Length', headerText: 'Length (CM)', width: 20, allowEditing: false },
                { field: 'Width', headerText: 'Width (CM)', width: 20, allowEditing: false },
                { field: 'Qty', headerText: 'Qty (Pcs)', width: 20, allowEditing: false }
            ],
            childGrid: {
                queryString: 'ConceptID',
                allowResizing: true,
                autofitColumns: false,
                toolbar: ['Add'],
                editSettings: { allowEditing: true, allowAdding: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
                columns: childColumns,
                actionBegin: function (args) {
                    if (args.requestType === 'beginEdit') {
                        if (args.rowData.YDProductionMasterID > 0) {
                            toastr.error("Yarn Dyeing found, You cannot modify anything.");
                            args.cancel = true;
                        }
                    }
                    else if (args.requestType === "add") {
                        args.data.FCMRChildID = maxCol++; //getMaxIdForArray($tblChildEl.getCurrentViewRecords(), "FCMRChildID");
                        args.data.ConceptID = this.parentDetails.parentKeyFieldValue;
                        args.data.StockQty = 0;
                    }
                    else if (args.requestType === "delete") {
                        if (args.data[0].YDProductionMasterID > 0) {
                            toastr.error("Yarn Dyeing found, You cannot modify anything.");
                            args.cancel = true;
                        }
                        else if (masterData.IsUsed && args.data[0].FCMRMasterID) {
                            toastr.error("Already yarn issued, You can't delete this item!");
                            args.cancel = true;
                        }
                    }
                    else if (args.requestType === "save") {
                        args.data = setSegDescById(args.data);
                        if (masterData.IsUsed && args.data.FCMRMasterID) {
                            //$tblChildEl.editModule.updateRow(args.rowIndex, args.previousData)
                            toastr.error("Already yarn issued, You can't update this item!");
                        }


                        args.data.IsInvalidItem = typeof args.data.IsInvalidItem === "undefined" ? 1 : args.data.IsInvalidItem;
                        args.data.StockItemNote = getDefaultValueWhenInvalidS(args.data.StockItemNote);
                        if (args.data.YarnStockSetId > 0 && args.data.IsInvalidItem && args.data.IsPR) {
                            toastr.error(`'Go For PR' is only for stock valid item`);
                            args.data.IsPR = false;
                            return false;
                        }

                        if (args.data.YD) {
                            args.data.YDItem = true;
                        }

                        //args.data.Segment1ValueId = !args.rowData.Segment1ValueId ? 0 : args.rowData.Segment1ValueId;
                        args.data.Segment2ValueId = !args.rowData.Segment2ValueId ? 0 : args.rowData.Segment2ValueId;
                        args.data.Segment3ValueId = !args.rowData.Segment3ValueId ? 0 : args.rowData.Segment3ValueId;
                        args.data.Segment4ValueId = !args.rowData.Segment4ValueId ? 0 : args.rowData.Segment4ValueId;
                        args.data.Segment5ValueId = !args.rowData.Segment5ValueId ? 0 : args.rowData.Segment5ValueId;
                        args.data.Segment6ValueId = !args.rowData.Segment6ValueId ? 0 : args.rowData.Segment6ValueId;
                        args.data.Segment7ValueId = !args.rowData.Segment7ValueId ? 0 : args.rowData.Segment7ValueId;
                        args.data.Segment8ValueId = !args.rowData.Segment8ValueId ? 0 : args.rowData.Segment8ValueId;

                        args.data = getYarnStockRelatedValues(args.data, args.rowData, args.previousData);

                        if (!args.data.IsPR) {
                            args.data.DayValidDurationId = 0;
                            args.data.DayDuration = 0;
                            args.data.DayValidDurationName = "Empty";
                        }
                    }
                },
                load: loadFirstLevelChildGrid
            },
            enableContextMenu: true,
            contextMenuItems: [
                { text: 'Copy Yarn Information', target: '.e-content', id: 'copy' },
                { text: 'Paste Yarn Information', target: '.e-content', id: 'paste' }//,
                //{ text: 'Copy Selected Yarn Information', target: '.e-content', id: 'copyOnlyYarn' },
                //{ text: 'Paste Selected Yarn Information', target: '.e-content', id: 'pasteOnlyYarn' }
            ],
            contextMenuClick: function (args) {
                if (args.item.id === 'copy') {
                    collarYarnItem = objectCopy(args.rowInfo.rowData.Childs);
                    if (collarYarnItem.length == 0) {
                        toastr.error("No Yarn information found to copy!!");
                        return;
                    }
                }
                else if (args.item.id === 'paste') {
                    var rowIndex = args.rowInfo.rowIndex;
                    if (collarYarnItem == null || collarYarnItem.length == 0) {
                        toastr.error("Please copy first!!");
                        return;
                    } else {
                        for (var i = 0; i < collarYarnItem.length; i++) {
                            //var parentRowData = $tblChildEl.getRowByIndex($tblChildEl.getRowIndexByPrimaryKey(args.rowInfo.rowData.FCMRMasterID));
                            var copiedItem = objectCopy(collarYarnItem[i]);
                            copiedItem.FCMRChildID = maxCol++;
                            copiedItem.FCMRMasterID = args.rowInfo.rowData.FCMRMasterID;
                            copiedItem.ConceptID = args.rowInfo.rowData.ConceptID;
                            copiedItem.ReqQty = 0;
                            copiedItem.ReqCone = 0;
                            copiedItem.SubGroupID = args.rowInfo.rowData.FUPartName.toLowerCase() == 'cuff' ? 12 : 11;

                            args.rowInfo.rowData.Childs.push(copiedItem);
                        }
                        $tblOtherItemEl.refresh();
                    }
                }
                else if (args.item.id === 'copyOnlyYarn') {

                    //var selectedRows = args.getSelectedRecords;
                    var selectedYarnItem = objectCopy(args.rowInfo.rowData.Childs);
                    if (selectedYarnItem.length == 0) {
                        toastr.error("No Yarn information found to copy!!");
                        return;
                    }
                    initYarnItemCopy(selectedYarnItem);
                    $modalYarnItemCopyIDEl.modal('show');
                }
                else if (args.item.id === 'pasteOnlyYarn') {

                    var rowIndex = args.rowInfo.rowIndex;
                    if (copyYarnItem == null || copyYarnItem.length == 0) {
                        toastr.error("Please copy first!!");
                        return;
                    } else {
                        for (var i = 0; i < copyYarnItem.length; i++) {

                            //var parentRowData = $tblChildEl.getRowByIndex($tblChildEl.getRowIndexByPrimaryKey(args.rowInfo.rowData.FCMRMasterID));
                            var copiedItem = objectCopy(copyYarnItem[i]);
                            copiedItem.FCMRChildID = maxCol++;
                            copiedItem.FCMRMasterID = args.rowInfo.rowData.FCMRMasterID;
                            copiedItem.ConceptID = args.rowInfo.rowData.ConceptID;
                            copiedItem.ReqQty = 0;
                            copiedItem.ReqCone = 0;

                            args.rowInfo.rowData.Childs.push(copiedItem);
                        }
                        $tblOtherItemEl.refresh();
                    }
                }
            },
            recordClick: function (args) {
                if (args.column && args.column.field == "GetYarnFromStock") {
                    var otherQuery = " AND (SampleStockQty > 0 OR AdvanceStockQty > 0) ";
                    otherQuery = replaceInvalidChar(otherQuery);
                    var finder = new commonFinder({
                        title: "Yarn Stock",
                        pageId: pageId,
                        height: 320,
                        modalSize: "modal-lg",
                        apiEndPoint: `/api/yarn-stock-adjustment/get-all-stocks-with-custom-query/${otherQuery}`,
                        headerTexts: "Yarn Detail,Count,Physical Count,Lot No,Shade Code,Supplier,Spinner,Sample Stock Qty,Advance Stock Qty,Block Sample Stock Qty,Block Advance Stock Qty,Issued Qty,Item Type,Note",
                        fields: "YarnCategory,Count,PhysicalCount,YarnLotNo,ShadeCode,SupplierName,SpinnerName,SampleStockQty,AdvanceStockQty,BlockSampleStockQty,BlockAdvanceStockQty,TotalIssueQty,InvalidItem_St,Note",
                        primaryKeyColumn: "YarnStockSetId",
                        autofitColumns: true,
                        onSelect: function (res) {
                            finder.hideModal();
                            loadChilds(11, args.rowData.ConceptID, res.rowData, maxCol++); //11 or 12
                        }
                    });
                    finder.showModal();
                }
            },
        });
        $tblOtherItemEl.refreshColumns;
        $tblOtherItemEl.appendTo(tblOtherItemId);
    }

    function loadFirstLevelChildGrid() {
        //this.parentDetails.parentKeyFieldValue = this.parentDetails.parentRowData['DBIID'];
        this.dataSource = this.parentDetails.parentRowData.Childs;
    }
    async function initFabricItemTable(data) {
        if ($tblFabricItemEl) $tblFabricItemEl.destroy();

        var childColumns = await getYarnItemColumnsWithSearchDDLAsync(ch_getCountRelatedList(data, 4));
        childColumns.unshift({ field: 'FCMRChildID', isPrimaryKey: true, visible: false });
        var additionalColumns = [
            { field: 'ConceptID', visible: false },
            {
                field: 'ShadeCode'
                , headerText: 'Shade Code'
                , valueAccessor: ej2GridDisplayFormatter
                , dataSource: data.YarnShadeBooks
                , displayField: "ShadeCode"
                , edit: ej2GridDropDownObj({
                })
            },
            {
                field: 'DayValidDurationId', headerText: 'Yarn Sourcing Mode', width: 120,
                valueAccessor: ej2GridDisplayFormatter,
                dataSource: masterData.DayValidDurations,
                displayField: "text",
                edit: ej2GridDropDownObj({
                })
            },
            { field: 'YDItem', headerText: 'YD Item?', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center' },
            { field: 'YD', headerText: 'Go for YD?', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center' },
            { field: 'ReqQty', headerText: 'Req Qty(KG)', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 1 } } },
            { field: 'ReqCone', headerText: 'Req Cone(PCS)', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 1 } } },
            { field: 'IsPR', headerText: 'Go for PR?', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center' },
            //{ field: 'StockQty', headerText: 'Stock Qty(KG)', allowEditing: false }
            { field: 'PhysicalCount', headerText: 'Physical Count', width: 100, allowEditing: false },
            { field: 'YarnLotNo', headerText: 'Lot No', width: 60, allowEditing: false },
            { field: 'SpinnerName', headerText: 'Spinner', width: 100, allowEditing: false },
            { field: 'SampleStockQty', headerText: 'Sample Stock Qty', width: 100, allowEditing: false },
            { field: 'AdvanceStockQty', headerText: 'Advance Stock Qty', width: 100, allowEditing: false },
            { field: 'YarnStockSetId', headerText: 'YarnStockSetId', width: 10, allowEditing: false, visible: false }
        ];
        childColumns.push.apply(childColumns, additionalColumns);
        childColumns = setMandatoryFieldsCSS(childColumns, "Segment1ValueId, Segment6ValueId, Distribution, Allowance, ReqQty, ReqCone");

        data.FabricItems.map(x => {
            x.GetYarnFromStock = 0;
        });

        ej.base.enableRipple(true);
        $tblFabricItemEl = new ej.grids.Grid({
            dataSource: data.FabricItems,
            allowResizing: true,
            columns: [
                { field: 'ConceptID', isPrimaryKey: true, visible: false },
                { field: 'SubGroupID', visible: false },
                { field: 'GetYarnFromStock', headerText: 'From Stock', allowEditing: false, textAlign: 'center', width: 30, valueAccessor: displayStockIcon },
                { field: 'MCSubClassName', headerText: 'Machine Type', width: 70, allowEditing: false },
                { field: 'TechnicalName', headerText: 'Technical Name', width: 70, allowEditing: false },
                { field: 'Composition', headerText: 'Composition', width: 70, allowEditing: false },
                { field: 'GSM', headerText: 'GSM', width: 50, allowEditing: false },
                { field: 'Qty', headerText: 'Quantity(pcs/kg)', width: 50, allowEditing: false }
            ],
            childGrid: {
                queryString: 'ConceptID',
                allowResizing: true,
                autofitColumns: false,
                toolbar: ['Add'],
                editSettings: { allowEditing: true, allowAdding: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
                columns: childColumns,
                actionBegin: function (args) {
                    if (args.requestType === 'beginEdit') {
                        if (args.rowData.YDProductionMasterID > 0) {
                            toastr.error("Yarn Dyeing found, You cannot modify anything.");
                            args.cancel = true;
                        }
                    }
                    else if (args.requestType === "add") {
                        args.data.FCMRChildID = maxCol++; //getMaxIdForArray($tblChildEl.getCurrentViewRecords(), "FCMRChildID");
                        args.data.ConceptID = this.parentDetails.parentKeyFieldValue;
                        args.data.StockQty = 0;
                    }
                    else if (args.requestType === "delete") {
                        if (args.data[0].YDProductionMasterID > 0) {
                            toastr.error("Yarn Dyeing found, You cannot modify anything.");
                            args.cancel = true;
                        }
                        else if (masterData.IsUsed && args.data[0].FCMRMasterID) {
                            toastr.error("Already yarn issued, You can't delete this item!");
                            args.cancel = true;
                        }
                    }
                    else if (args.requestType === "save") {
                        args.data = setSegDescById(args.data);
                        if (masterData.IsUsed && args.data.FCMRMasterID) {
                            //$tblChildEl.editModule.updateRow(args.rowIndex, args.previousData)
                            toastr.error("Already yarn issued, You can't update this item!");
                        }

                        args.data.IsInvalidItem = typeof args.data.IsInvalidItem === "undefined" ? 1 : args.data.IsInvalidItem;
                        args.data.StockItemNote = getDefaultValueWhenInvalidS(args.data.StockItemNote);
                        if (args.data.YarnStockSetId > 0 && args.data.IsInvalidItem && args.data.IsPR) {
                            toastr.error(`'Go For PR' is only for stock valid item`);
                            args.data.IsPR = false;
                            return false;
                        }

                        if (args.data.YD) {
                            args.data.YDItem = true;
                        }
                        //args.data.Segment1ValueId = !args.rowData.Segment1ValueId ? 0 : args.rowData.Segment1ValueId;
                        args.data.Segment2ValueId = !args.rowData.Segment2ValueId ? 0 : args.rowData.Segment2ValueId;
                        args.data.Segment3ValueId = !args.rowData.Segment3ValueId ? 0 : args.rowData.Segment3ValueId;
                        args.data.Segment4ValueId = !args.rowData.Segment4ValueId ? 0 : args.rowData.Segment4ValueId;
                        args.data.Segment5ValueId = !args.rowData.Segment5ValueId ? 0 : args.rowData.Segment5ValueId;
                        args.data.Segment6ValueId = !args.rowData.Segment6ValueId ? 0 : args.rowData.Segment6ValueId;
                        args.data.Segment7ValueId = !args.rowData.Segment7ValueId ? 0 : args.rowData.Segment7ValueId;
                        args.data.Segment8ValueId = !args.rowData.Segment8ValueId ? 0 : args.rowData.Segment8ValueId;

                        args.data = getYarnStockRelatedValues(args.data, args.rowData, args.previousData);

                        if (!args.data.IsPR) {
                            args.data.DayValidDurationId = 0;
                            args.data.DayDuration = 0;
                            args.data.DayValidDurationName = "Empty";
                        }
                    }
                },
                load: loadFirstLevelChildGrid
            },
            enableContextMenu: true,
            contextMenuItems: [
                { text: 'Copy Yarn Information', target: '.e-content', id: 'copy' },
                { text: 'Paste Yarn Information', target: '.e-content', id: 'paste' }//,
                //{ text: 'Copy Selected Yarn Information', target: '.e-content', id: 'copyOnlyYarn' },
                //{ text: 'Paste Selected Yarn Information', target: '.e-content', id: 'pasteOnlyYarn' }
            ],
            contextMenuClick: function (args) {
                if (args.item.id === 'copy') {
                    collarYarnItem = objectCopy(args.rowInfo.rowData.Childs);
                    if (collarYarnItem.length == 0) {
                        toastr.error("No Yarn information found to copy!!");
                        return;
                    }
                }
                else if (args.item.id === 'paste') {
                    var rowIndex = args.rowInfo.rowIndex;
                    if (collarYarnItem == null || collarYarnItem.length == 0) {
                        toastr.error("Please copy first!!");
                        return;
                    } else {
                        for (var i = 0; i < collarYarnItem.length; i++) {

                            //var parentRowData = $tblChildEl.getRowByIndex($tblChildEl.getRowIndexByPrimaryKey(args.rowInfo.rowData.FCMRMasterID));
                            var copiedItem = objectCopy(collarYarnItem[i]);
                            copiedItem.FCMRChildID = maxCol++;
                            copiedItem.FCMRMasterID = args.rowInfo.rowData.FCMRMasterID;
                            copiedItem.ConceptID = args.rowInfo.rowData.ConceptID;
                            copiedItem.ReqQty = 0;
                            copiedItem.ReqCone = 0;
                            copiedItem.SubGroupID = 1;

                            args.rowInfo.rowData.Childs.push(copiedItem);
                        }
                        $tblFabricItemEl.refresh();
                    }
                }
                else if (args.item.id === 'copyOnlyYarn') {

                    //var selectedRows = args.getSelectedRecords;
                    var selectedYarnItem = objectCopy(args.rowInfo.rowData.Childs);
                    if (selectedYarnItem.length == 0) {
                        toastr.error("No Yarn information found to copy!!");
                        return;
                    }
                    initYarnItemCopy(selectedYarnItem);
                    $modalYarnItemCopyIDEl.modal('show');
                }
                else if (args.item.id === 'pasteOnlyYarn') {

                    var rowIndex = args.rowInfo.rowIndex;
                    if (copyYarnItem == null || copyYarnItem.length == 0) {
                        toastr.error("Please copy first!!");
                        return;
                    } else {
                        for (var i = 0; i < copyYarnItem.length; i++) {

                            //var parentRowData = $tblChildEl.getRowByIndex($tblChildEl.getRowIndexByPrimaryKey(args.rowInfo.rowData.FCMRMasterID));
                            var copiedItem = objectCopy(copyYarnItem[i]);
                            copiedItem.FCMRChildID = maxCol++;
                            copiedItem.FCMRMasterID = args.rowInfo.rowData.FCMRMasterID;
                            copiedItem.ConceptID = args.rowInfo.rowData.ConceptID;
                            copiedItem.ReqQty = 0;
                            copiedItem.ReqCone = 0;

                            args.rowInfo.rowData.Childs.push(copiedItem);
                        }
                        $tblFabricItemEl.refresh();
                    }
                }
            },
            recordClick: function (args) {
                if (args.column && args.column.field == "GetYarnFromStock") {
                    var otherQuery = " AND (SampleStockQty > 0 OR AdvanceStockQty > 0) ";
                    otherQuery = replaceInvalidChar(otherQuery);
                    var finder = new commonFinder({
                        title: "Yarn Stock",
                        pageId: pageId,
                        height: 320,
                        modalSize: "modal-lg",
                        apiEndPoint: `/api/yarn-stock-adjustment/get-all-stocks-with-custom-query/${otherQuery}`,
                        headerTexts: "Yarn Detail,Count,Physical Count,Lot No,Shade Code,Supplier,Spinner,Sample Stock Qty,Advance Stock Qty,Block Sample Stock Qty,Block Advance Stock Qty,Issued Qty,Item Type,Note",
                        fields: "YarnCategory,Count,PhysicalCount,YarnLotNo,ShadeCode,SupplierName,SpinnerName,SampleStockQty,AdvanceStockQty,BlockSampleStockQty,BlockAdvanceStockQty,TotalIssueQty,InvalidItem_St,Note",
                        primaryKeyColumn: "YarnStockSetId",
                        autofitColumns: true,
                        onSelect: function (res) {
                            finder.hideModal();
                            loadChilds(1, args.rowData.ConceptID, res.rowData, maxCol++);
                        }
                    });
                    finder.showModal();
                }
            },
        });
        $tblFabricItemEl.refreshColumns;
        $tblFabricItemEl.appendTo(tblFabricItemId);
    }

    async function initYarnItemCopy(data) {

        if ($tblYarnItemCopyIDEl) $tblYarnItemCopyIDEl.destroy();
        ej.base.enableRipple(true);

        var childColumns = await getYarnItemColumnsWithSearchDDLAsync(ch_getCountRelatedList(data, 4));
        childColumns.unshift({ field: 'FCMRChildID', isPrimaryKey: true, visible: false });
        var additionalColumns = [
            { field: 'ConceptID', visible: false },
            {
                field: 'ShadeCode'
                , headerText: 'Shade Code'
                , valueAccessor: ej2GridDisplayFormatter
                , dataSource: data.YarnShadeBooks
                , displayField: "ShadeCode"
                , edit: ej2GridDropDownObj({
                })
            },
            {
                field: 'DayValidDurationId', headerText: 'Yarn Sourcing Mode', width: 120,
                valueAccessor: ej2GridDisplayFormatter,
                dataSource: masterData.DayValidDurations,
                displayField: "text",
                edit: ej2GridDropDownObj({
                })
            },
            { field: 'YDItem', headerText: 'YD Item?', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center' },
            { field: 'YD', headerText: 'Go for YD?', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center' },
            { field: 'ReqQty', headerText: 'Req Qty(KG)', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 1 } } },
            { field: 'ReqCone', headerText: 'Req Cone(PCS)', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 1 } } },
            { field: 'IsPR', headerText: 'Go for PR?', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center' },
            { field: 'StockQty', headerText: 'Stock Qty(KG)', allowEditing: false }
        ];
        childColumns.push.apply(childColumns, additionalColumns);
        childColumns = setMandatoryFieldsCSS(childColumns, "Segment1ValueId, Segment6ValueId, Distribution, Allowance, ReqQty, ReqCone");

        var tableOptions = {
            //tableId: tblYarnItemCopyID,
            columns: childColumns,
            actionBegin: function (args) {
                if (args.requestType === "add") {

                }
                else if (args.requestType === "save") {
                    //args.data.Segment1ValueId = !args.rowData.Segment1ValueId ? 0 : args.rowData.Segment1ValueId;
                    args.data.Segment2ValueId = !args.rowData.Segment2ValueId ? 0 : args.rowData.Segment2ValueId;
                    args.data.Segment3ValueId = !args.rowData.Segment3ValueId ? 0 : args.rowData.Segment3ValueId;
                    args.data.Segment4ValueId = !args.rowData.Segment4ValueId ? 0 : args.rowData.Segment4ValueId;
                    args.data.Segment5ValueId = !args.rowData.Segment5ValueId ? 0 : args.rowData.Segment5ValueId;
                    args.data.Segment6ValueId = !args.rowData.Segment6ValueId ? 0 : args.rowData.Segment6ValueId;
                    args.data.Segment7ValueId = !args.rowData.Segment7ValueId ? 0 : args.rowData.Segment7ValueId;
                    args.data.Segment8ValueId = !args.rowData.Segment8ValueId ? 0 : args.rowData.Segment8ValueId;

                }
            },
            dataSource: data,
            allowResizing: true,
            editSettings: { allowEditing: false, allowAdding: true, allowDeleting: false, mode: "Normal", showDeleteConfirmDialog: true }
        };
        //$tblYarnItemCopyIDEl = new initEJ2Grid(tableOptions);

        $tblYarnItemCopyIDEl = new ej.grids.Grid(
            tableOptions
        );
        $tblYarnItemCopyIDEl.refreshColumns;
        $tblYarnItemCopyIDEl.appendTo(tblYarnItemCopyID);
    }

    function displayStockIcon(field, data, column) {
        column.disableHtmlEncode = false;
        return `<button type="button" class="btn btn-sm" style="background-color: #ffffff; color: black;" title='Get item from yarn stock'><span class="fab fa-dropbox"></span></button>`;
    }
    function getYarnStockRelatedValues(obj, rowData, previousData) {
        obj.YarnStockSetId = getDefaultValueWhenInvalidN(rowData.YarnStockSetId);
        obj.IsInvalidItem = typeof rowData.IsInvalidItem === "undefined" ? false : rowData.IsInvalidItem;

        if (obj.IsInvalidItem) {
            obj.Segment1ValueId = previousData.Segment1ValueId;
            obj.Segment2ValueId = previousData.Segment2ValueId;
            obj.Segment3ValueId = previousData.Segment3ValueId;
            obj.Segment4ValueId = previousData.Segment4ValueId;
            obj.Segment5ValueId = previousData.Segment5ValueId;
            obj.Segment6ValueId = previousData.Segment6ValueId;
            obj.Segment1ValueDesc = previousData.Segment1ValueDesc;
            obj.Segment2ValueDesc = previousData.Segment2ValueDesc;
            obj.Segment3ValueDesc = previousData.Segment3ValueDesc;
            obj.Segment4ValueDesc = previousData.Segment4ValueDesc;
            obj.Segment5ValueDesc = previousData.Segment5ValueDesc;
            obj.Segment6ValueDesc = previousData.Segment6ValueDesc;
            obj.ShadeCode = previousData.ShadeCode;
        }
        if (obj.YarnStockSetId > 0) {
            if (obj.ShadeCode == null) obj.ShadeCode = "";
            if (obj.Segment1ValueId != previousData.Segment1ValueId
                || obj.Segment2ValueId != previousData.Segment2ValueId
                || obj.Segment3ValueId != previousData.Segment3ValueId
                || obj.Segment4ValueId != previousData.Segment4ValueId
                || obj.Segment5ValueId != previousData.Segment5ValueId
                || obj.Segment6ValueId != previousData.Segment6ValueId
                || obj.Segment1ValueDesc != previousData.Segment1ValueDesc
                || obj.Segment2ValueDesc != previousData.Segment2ValueDesc
                || obj.Segment3ValueDesc != previousData.Segment3ValueDesc
                || obj.Segment4ValueDesc != previousData.Segment4ValueDesc
                || obj.Segment5ValueDesc != previousData.Segment5ValueDesc
                || obj.Segment6ValueDesc != previousData.Segment6ValueDesc
                || obj.ShadeCode != previousData.ShadeCode) {
                obj.YarnStockSetId = 0;
            }
        }

        if (obj.YarnStockSetId == 0) {
            obj.PhysicalCount = "";
            obj.YarnLotNo = "";
            obj.SpinnerName = "";
            obj.SampleStockQty = 0;
            obj.AdvanceStockQty = 0;
        } else {
            obj.PhysicalCount = rowData.PhysicalCount;
            obj.YarnLotNo = rowData.YarnLotNo;
            obj.SpinnerName = rowData.SpinnerName;
            obj.SampleStockQty = rowData.SampleStockQty;
            obj.AdvanceStockQty = rowData.AdvanceStockQty;
        }
        return obj;
    }
    function loadChilds(subGroupId, conceptId, selectedDate, nextFCMRChildID) {
        if (subGroupId == 1) {
            masterData.FabricItems = $tblFabricItemEl.getCurrentViewRecords();
        }
        else {
            masterData.OtherItems = $tblOtherItemEl.getCurrentViewRecords();
        }
        var indexF = -1;
        if (subGroupId == 1) {
            indexF = masterData.FabricItems.findIndex(x => x.ConceptID == conceptId);
        }
        else {
            indexF = masterData.OtherItems.findIndex(x => x.ConceptID == conceptId);
        }

        if (indexF > -1) {
            var nFCMRMasterID = 0;
            if (subGroupId == 1 && (typeof masterData.FabricItems[indexF].Childs === "undefined" || masterData.FabricItems[indexF].Childs == null)) {
                masterData.FabricItems[indexF].Childs = [];
            }
            else if (subGroupId == 11 && (typeof masterData.OtherItems[indexF].Childs === "undefined" || masterData.OtherItems[indexF].Childs == null)) {
                masterData.OtherItems[indexF].Childs = [];
            }

            if (subGroupId == 1) {
                nFCMRMasterID = masterData.FabricItems[indexF].FCMRMasterID;
            }
            else {
                nFCMRMasterID = masterData.OtherItems[indexF].FCMRMasterID;
            }

            var obj = {
                FCMRChildID: nextFCMRChildID,
                FCMRMasterID: nFCMRMasterID,
                ItemMasterId: selectedDate.ItemMasterId,
                YarnCategory: selectedDate.YarnCategory,
                ConceptID: conceptId,
                YD: false,
                ReqQty: 0,
                SetupChildID: 0,
                UnitID: 28,
                Remarks: "",
                ReqCone: 0,
                IsPR: false,
                ShadeCode: selectedDate.ShadeCode,
                DayValidDurationId: getDefaultValueWhenInvalidN(selectedDate.DayValidDurationId),

                Distribution: 0,
                BookingQty: 0,
                Allowance: 0,
                YDItem: false,

                YarnStockSetId: selectedDate.YarnStockSetId,
                IsInvalidItem: selectedDate.IsInvalidItem,
                StockItemNote: selectedDate.Note,

                Segment1ValueId: selectedDate.Segment1ValueId,
                Segment1ValueDesc: selectedDate.Segment1ValueDesc,
                Segment2ValueId: selectedDate.Segment2ValueId,
                Segment2ValueDesc: selectedDate.Segment2ValueDesc,
                Segment3ValueId: selectedDate.Segment3ValueId,
                Segment3ValueDesc: selectedDate.Segment3ValueDesc,
                Segment4ValueId: selectedDate.Segment4ValueId,
                Segment4ValueDesc: selectedDate.Segment4ValueDesc,
                Segment5ValueId: selectedDate.Segment5ValueId,
                Segment5ValueDesc: selectedDate.Segment5ValueDesc,
                Segment6ValueId: selectedDate.Segment6ValueId,
                Segment6ValueDesc: selectedDate.Segment6ValueDesc,

                Composition: selectedDate.Composition,
                YarnType: selectedDate.YarnType,
                ManufacturingProcess: selectedDate.ManufacturingProcess,
                SubProcess: selectedDate.SubProcess,
                QualityParameter: selectedDate.QualityParameter,
                Count: selectedDate.Count,

                PhysicalCount: selectedDate.PhysicalCount,
                YarnLotNo: selectedDate.YarnLotNo,
                SpinnerName: selectedDate.SpinnerName,
                SampleStockQty: selectedDate.SampleStockQty,
                AdvanceStockQty: selectedDate.AdvanceStockQty
            };

            if (subGroupId == 1) {
                masterData.FabricItems[indexF].Childs.push(obj);
            }
            else if (subGroupId == 11) {
                masterData.OtherItems[indexF].Childs.push(obj);
            }

            if (subGroupId == 1) {
                initFabricItemTable(masterData)
            } else {
                initOtherItemTable(masterData);
            }
        }
    }
    function setSegDescById(obj) {
        var maxSeg = 6;
        for (var sIndex = 1; sIndex <= maxSeg; sIndex++) {
            var objTemp = _segments["Segment" + sIndex + "ValueList"].find(x => x.id == obj["Segment" + sIndex + "ValueId"]);
            if (objTemp) {
                obj["Segment" + sIndex + "ValueDesc"] = objTemp.text;
            }
        }

        return obj;
    }

    function backToListWithoutFilter() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
    }

    function backToList() {
        backToListWithoutFilter();
        initMasterTable();
    }

    function resetForm() {
        $formEl.trigger("reset");
        $.each($formEl.find('select'), function (i, el) {
            $(el).select2('');
        });
        $formEl.find("#FCMRMasterID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function getNew(grpConceptNo, conceptTypeId) {
        var url = `/api/rnd-free-concept-mr/new-by-group-concept/${grpConceptNo}/${conceptTypeId}`;
        axios.get(url)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.ConceptDate = formatDateToDefault(masterData.ConceptDate);
                masterData.ReqDate = formatDateToDefault(masterData.ReqDate);
                validationConstraints["ReqDate"] = {
                    datetime: {
                        dateOnly: true,
                        earliest: masterData.ConceptDate
                    }
                };

                setFormData($formEl, masterData);
                initChildTable(masterData.ChildColors);
                //initYarnChildTable(masterData);
                initOtherItemTable(masterData);
                initFabricItemTable(masterData);

                if (conceptTypeId == 1) {   //Only Fabric
                    $formEl.find("#divFabricItem").fadeIn();
                    $formEl.find("#divOtherItem").fadeOut();
                    $formEl.find("#divFabricInformation").fadeIn();
                    $formEl.find("#onlyFabric").prop('checked', true);
                } else if (conceptTypeId == 2) {    //Fabric & Other Item
                    $formEl.find("#divFabricItem, #divOtherItem").fadeIn();
                    $formEl.find("#divFabricInformation").fadeIn();
                    $formEl.find("#fabricOtherItem").prop('checked', true);
                } else {    //Other Item
                    $formEl.find("#divOtherItem").fadeIn();
                    $formEl.find("#divFabricInformation").fadeOut();
                    $formEl.find("#divFabricItem").fadeOut();
                    $formEl.find("#onlyOtherItem").prop('checked', true);
                }
                if (masterData.NeedRevision) {
                    $formEl.find("#btnRevise").fadeIn();
                    $formEl.find("#btnSave").fadeOut();
                    $formEl.find("#btnSaveComplete").fadeOut();
                } else {
                    $formEl.find("#btnRevise").fadeOut();
                    $formEl.find("#btnSave").fadeIn();
                    $formEl.find("#btnSaveComplete").fadeIn();
                }
            })

            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getDetails(grpConceptNo, conceptTypeId) {
        axios.get(`/api/rnd-free-concept-mr/multiple-mr/${grpConceptNo}/${conceptTypeId}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.ConceptDate = formatDateToDefault(masterData.ConceptDate);
                masterData.ReqDate = formatDateToDefault(masterData.ReqDate);

                setFormData($formEl, masterData);
                initChildTable(masterData.ChildColors);
                //initYarnChildTable(masterData);
                initOtherItemTable(masterData);
                initFabricItemTable(masterData);

                if (conceptTypeId == 1) {   //Only Fabric
                    $formEl.find("#divFabricItem").fadeIn();
                    $formEl.find("#divOtherItem").fadeOut();
                    $formEl.find("#divFabricInformation").fadeIn();
                    $formEl.find("#onlyFabric").prop('checked', true);
                } else if (conceptTypeId == 2) {    //Fabric & Other Item
                    $formEl.find("#divFabricItem, #divOtherItem").fadeIn();
                    $formEl.find("#divFabricInformation").fadeIn();
                    $formEl.find("#fabricOtherItem").prop('checked', true);
                } else {    //Other Item
                    $formEl.find("#divOtherItem").fadeIn();
                    $formEl.find("#divFabricInformation").fadeOut();
                    $formEl.find("#divFabricItem").fadeOut();
                    $formEl.find("#onlyOtherItem").prop('checked', true);
                }

                //if (masterData.IsUsed) $formEl.find("#btnSave").fadeOut();
                //else $formEl.find("#btnSave").fadeIn(); 

                //if (masterData.NeedRevision) {
                //    $formEl.find("#btnRevise").fadeIn();
                //    $formEl.find("#btnSave").fadeOut();
                //    $formEl.find("#btnSaveComplete").fadeOut();
                //} else {
                //    $formEl.find("#btnRevise").fadeOut();
                //    $formEl.find("#btnSave").fadeIn();
                //    $formEl.find("#btnSaveComplete").fadeIn();
                //}

            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getReviseDetails(grpConceptNo, conceptTypeId) {

        axios.get(`/api/rnd-free-concept-mr/new-by-group-concept/${grpConceptNo}/${conceptTypeId}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.ConceptDate = formatDateToDefault(masterData.ConceptDate);
                masterData.ReqDate = formatDateToDefault(masterData.ReqDate);

                setFormData($formEl, masterData);
                initChildTable(masterData.ChildColors);
                //initYarnChildTable(masterData);
                initOtherItemTable(masterData);
                initFabricItemTable(masterData);

                if (conceptTypeId == 1) {   //Only Fabric
                    $formEl.find("#divFabricItem").fadeIn();
                    $formEl.find("#divOtherItem").fadeOut();
                    $formEl.find("#divFabricInformation").fadeIn();
                    $formEl.find("#onlyFabric").prop('checked', true);
                } else if (conceptTypeId == 2) {    //Fabric & Other Item
                    $formEl.find("#divFabricItem, #divOtherItem").fadeIn();
                    $formEl.find("#divFabricInformation").fadeIn();
                    $formEl.find("#fabricOtherItem").prop('checked', true);
                } else {    //Other Item
                    $formEl.find("#divOtherItem").fadeIn();
                    $formEl.find("#divFabricInformation").fadeOut();
                    $formEl.find("#divFabricItem").fadeOut();
                    $formEl.find("#onlyOtherItem").prop('checked', true);
                }
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function isInvalidSegment(segId) {
        if (typeof segId === "undefined" || segId == null || segId == 0) return true;
        return false;
    }
    function save(IsComplete) {
        initializeValidation($formEl, validationConstraints);
        if (!isValidForm($formEl, validationConstraints)) return toastr.error("Please correct all validation errors!");
        else hideValidationErrors($formEl);

        var data = formDataToJson($formEl.serializeArray());
        data.IsComplete = IsComplete;

        var fabrics = $tblFabricItemEl.getCurrentViewRecords();
        var others = $tblOtherItemEl.getCurrentViewRecords();

        var models = [];
        var yarns = [];

        var hasError = false;
        var isFlatKnit = $formEl.find("#onlyOtherItem").is(':checked');
        var prChildList = [];
        if (!isFlatKnit) {
            for (var i = 0; i < fabrics.length; i++) {
                fabrics[i].ReqDate = data.ReqDate;
                fabrics[i].GroupConceptNo = $formEl.find('#GroupConceptNo').val();
                fabrics[i].IsComplete = IsComplete;
                fabrics[i].IsModified = status == statusConstants.PENDING ? false : true;

                if (fabrics[i].Childs.length == 0) {
                    toastr.error("Add yarn information.");
                    hasError = true;
                    break;
                }
                fabrics[i].Childs.map(x => {
                    x.DayValidDurationId = getDefaultValueWhenInvalidN(x.DayValidDurationId);
                    if (x.IsPR && x.DayValidDurationId == 0) {
                        prChildList.push(x);
                    }
                    if (!x.IsPR) {
                        x.DayValidDurationId = 0;
                        x.DayDuration = 0;
                        x.DayValidDurationName = "Empty";
                    }
                });

                yarns.push(...fabrics[i].Childs);
                models.push(fabrics[i]);
            }
        }
        if (!hasError) {
            for (var i = 0; i < others.length; i++) {
                others[i].ReqDate = data.ReqDate;
                others[i].GroupConceptNo = $formEl.find('#GroupConceptNo').val();
                others[i].IsComplete = IsComplete;
                others[i].IsModified = status == statusConstants.PENDING ? false : true;

                if (others[i].Childs.length == 0) {
                    toastr.error("Add yarn information.");
                    hasError = true;
                    break;
                }
                others[i].Childs.map(x => {
                    x.DayValidDurationId = getDefaultValueWhenInvalidN(x.DayValidDurationId);
                    if (x.IsPR && x.DayValidDurationId == 0) {
                        prChildList.push(x);
                    }
                    if (!x.IsPR) {
                        x.DayValidDurationId = 0;
                        x.DayDuration = 0;
                        x.DayValidDurationName = "Empty";
                    }
                });
                yarns.push(...others[i].Childs);
                models.push(others[i]);
            }
        }

        if (!hasError) {
            for (var i = 0; i < yarns.length; i++) {
                var yarn = yarns[i];

                if (isInvalidSegment(yarn.Segment1ValueId)) yarn.Segment1ValueId = 0;
                if (isInvalidSegment(yarn.Segment2ValueId)) yarn.Segment2ValueId = 0;
                if (isInvalidSegment(yarn.Segment3ValueId)) yarn.Segment3ValueId = 0;
                if (isInvalidSegment(yarn.Segment4ValueId)) yarn.Segment4ValueId = 0;
                if (isInvalidSegment(yarn.Segment5ValueId)) {
                    yarn.Segment5ValueId = 0;
                    yarn.Segment5ValueDesc = "";
                }
                if (isInvalidSegment(yarn.Segment6ValueId)) yarn.Segment6ValueId = 0;

                if (yarn.Segment1ValueId == 0) {
                    toastr.error("Select composition");
                    hasError = true;
                    break;
                }

                if (IsComplete) {
                    if (yarn.Segment2ValueId == 0 && yarn.IsPR) {
                        toastr.error("Select yarn type for 'Go for PR'");
                        hasError = true;
                        break;
                    }

                    if (yarn.Segment3ValueId == 0 && yarn.IsPR) {
                        toastr.error("Select manufacturing process for 'Go for PR'");
                        hasError = true;
                        break;
                    }

                    if (yarn.Segment4ValueId == 0 && yarn.IsPR) {
                        toastr.error("Select sub process for 'Go for PR'");
                        hasError = true;
                        break;
                    }

                    if (yarn.Segment5ValueId == 0 && yarn.IsPR) {
                        toastr.error("Select quality parameter for 'Go for PR'");
                        hasError = true;
                        break;
                    }

                    if (yarn.YarnStockSetId == 0 && !yarn.YDItem && !yarn.IsPR) {
                        toastr.error(`Select YD Item or Go for PR for non-stock item.`);
                        hasError = true;
                        break;
                    }
                }

                if (yarn.Segment6ValueId == 0) {
                    toastr.error("Select count");
                    hasError = true;
                    break;
                }
                if ((yarn.Segment5ValueDesc.toLowerCase() == "melange" || yarn.Segment5ValueDesc.toLowerCase() == "color melange") && (yarn.ShadeCode == null || yarn.ShadeCode == "")) {
                    toastr.error("Select shade code for color melange");
                    hasError = true;
                    break;
                }


                //if (yarn.Segment5ValueId == 60416 && (yarn.ShadeCode == null || yarn.ShadeCode == "")) { //Color Melange = 60416
                //    toastr.error("Select shade code for color melange.");
                //    hasError = true;
                //    break;
                //}
                if (yarn.ReqQty == null || yarn.ReqQty == 0) {
                    toastr.error("Must give Req Qty (KG).");
                    hasError = true;
                    break;
                }
                if (yarn.ReqCone == null || yarn.ReqCone == 0) {
                    toastr.error("Must give Req Cone (PCS).");
                    hasError = true;
                    break;
                }
            }
        }
        if (hasError) return false;

        if (IsComplete && prChildList.length > 0 && masterData.IsCheckDVD) {
            var itemStr = prChildList.length > 1 ? "items" : "item";
            toastr.error(`Select yarn sourcing mode for PR item (${prChildList.length} ${itemStr} found).`);
            return false;
        }

        axios.post("/api/rnd-free-concept-mr/save-multiple", models)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }

    function revise() {
        initializeValidation($formEl, validationConstraints);
        if (!isValidForm($formEl, validationConstraints)) return toastr.error("Please correct all validation errors!");
        else hideValidationErrors($formEl);

        var data = formDataToJson($formEl.serializeArray());

        var fabrics = $tblFabricItemEl.getCurrentViewRecords();
        var others = $tblOtherItemEl.getCurrentViewRecords();

        var models = [];
        var yarns = [];

        var hasError = false;
        var prChildList = [];
        var isFlatKnit = $formEl.find("#onlyOtherItem").is(':checked');
        if (!isFlatKnit) {
            for (var i = 0; i < fabrics.length; i++) {
                fabrics[i].ReqDate = data.ReqDate;
                fabrics[i].GroupConceptNo = $formEl.find('#GroupConceptNo').val();
                fabrics[i].IsModified = status == statusConstants.PENDING ? false : true;

                if (fabrics[i].Childs.length == 0) {
                    toastr.error("Add yarn information.");
                    hasError = true;
                    break;
                }
                fabrics[i].Childs.map(x => {
                    x.DayValidDurationId = getDefaultValueWhenInvalidN(x.DayValidDurationId);
                    if (x.IsPR && x.DayValidDurationId == 0) {
                        prChildList.push(x);
                    }
                    if (!x.IsPR) {
                        x.DayValidDurationId = 0;
                        x.DayDuration = 0;
                        x.DayValidDurationName = "Empty";
                    }
                });

                yarns.push(...fabrics[i].Childs);
                models.push(fabrics[i]);
            }
        }

        if (!hasError) {
            for (var i = 0; i < others.length; i++) {
                others[i].ReqDate = data.ReqDate;
                others[i].GroupConceptNo = $formEl.find('#GroupConceptNo').val();
                others[i].IsModified = status == statusConstants.PENDING ? false : true;

                if (others[i].Childs.length == 0) {
                    toastr.error("Add yarn information.");
                    hasError = true;
                    break;
                }
                others[i].Childs.map(x => {
                    x.DayValidDurationId = getDefaultValueWhenInvalidN(x.DayValidDurationId);
                    if (x.IsPR && x.DayValidDurationId == 0) {
                        prChildList.push(x);
                    }
                    if (!x.IsPR) {
                        x.DayValidDurationId = 0;
                        x.DayDuration = 0;
                        x.DayValidDurationName = "Empty";
                    }
                });

                yarns.push(...others[i].Childs);
                models.push(others[i]);
            }
        }

        if (!hasError) {
            for (var i = 0; i < yarns.length; i++) {
                var yarn = yarns[i];

                if (isInvalidSegment(yarn.Segment1ValueId)) yarn.Segment1ValueId = 0;
                if (isInvalidSegment(yarn.Segment2ValueId)) yarn.Segment2ValueId = 0;
                if (isInvalidSegment(yarn.Segment3ValueId)) yarn.Segment3ValueId = 0;
                if (isInvalidSegment(yarn.Segment4ValueId)) yarn.Segment4ValueId = 0;
                if (isInvalidSegment(yarn.Segment5ValueId)) {
                    yarn.Segment5ValueId = 0;
                    yarn.Segment5ValueDesc = "";
                }
                if (isInvalidSegment(yarn.Segment6ValueId)) yarn.Segment6ValueId = 0;

                if (yarn.Segment1ValueId == 0) {
                    toastr.error("Select composition");
                    hasError = true;
                    break;
                }

                if (yarn.Segment2ValueId == 0 && yarn.IsPR) {
                    toastr.error("Select yarn type for 'Go for PR'");
                    hasError = true;
                    break;
                }

                if (yarn.Segment3ValueId == 0 && yarn.IsPR) {
                    toastr.error("Select manufacturing process for 'Go for PR'");
                    hasError = true;
                    break;
                }

                if (yarn.Segment4ValueId == 0 && yarn.IsPR) {
                    toastr.error("Select sub process for 'Go for PR'");
                    hasError = true;
                    break;
                }

                if (yarn.Segment5ValueId == 0 && yarn.IsPR) {
                    toastr.error("Select quality parameter for 'Go for PR'");
                    hasError = true;
                    break;
                }

                if (yarn.Segment6ValueId == 0) {
                    toastr.error("Select count");
                    hasError = true;
                    break;
                }
                if ((yarn.Segment5ValueDesc.toLowerCase() == "melange" || yarn.Segment5ValueDesc.toLowerCase() == "color melange") && (yarn.ShadeCode == null || yarn.ShadeCode == "")) {
                    toastr.error("Select shade code for color melange");
                    hasError = true;
                    break;
                }
                if (yarn.ReqQty == null || yarn.ReqQty == 0) {
                    toastr.error("Must give Req Qty (KG).");
                    hasError = true;
                    break;
                }
                if (yarn.ReqCone == null || yarn.ReqCone == 0) {
                    toastr.error("Must give Req Cone (PCS).");
                    hasError = true;
                    break;
                }
            }
        }
        if (hasError) return false;

        
        if (prChildList.length > 0 && masterData.IsCheckDVD) {
            var itemStr = prChildList.length > 1 ? "items" : "item";
            toastr.error(`Select yarn sourcing mode for PR item (${prChildList.length} ${itemStr} found).`);
            return false;
        }

        axios.post("/api/rnd-free-concept-mr/revise", models)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }

    /*
    function save(IsComplete) {
        initializeValidation($formEl, validationConstraints);
        if (!isValidForm($formEl, validationConstraints)) return toastr.error("Please correct all validation errors!");
        else hideValidationErrors($formEl);


        var mrs = [];
        var data = formDataToJson($formEl.serializeArray());
        data.IsComplete = IsComplete;
        if (data.ConceptTypeID == 1 || data.ConceptTypeID == 2) {
            data = formDataToJson($formEl.serializeArray());
            data.IsComplete = IsComplete;
            data.Childs = $tblChildEl.getCurrentViewRecords();
            if (data.Childs.length === 0 && data.IsComplete == true) return toastr.error("At least 1 Yarn Information For fabric is required.");
            mrs.push(data);
        }

        $tblOtherItemEl.getCurrentViewRecords().forEach(function (mr) {
            mr.ReqDate = data.ReqDate;
            mrs.push(mr);
        });

        //New 
        var hasError = false;
        for (var iParent = 0; iParent < mrs.length; iParent++) {
            var childs = mrs[iParent].Childs;
            if (childs) {
                for (var iChild = 0; iChild < childs.length; iChild++) {
                    if (childs[iChild].Segment5ValueId) {
                        if (childs[iChild].Segment5ValueId == 60416 && (childs[iChild].ShadeCode == null || childs[iChild].ShadeCode == "")) {
                            toastr.error("Select shade code for color melange"); //Color Melange = 60416
                            hasError = true;
                            break;
                        }
                    }
                }
                if (hasError) break;
            }
        }
        if (hasError) return false;

        model = mrs;
        model.map(x => {
            x.GroupConceptNo = $formEl.find('#GroupConceptNo').val();
            x.IsComplete = IsComplete;
            x.IsModified = status == statusConstants.PENDING ? false : true;
        });

        axios.post("/api/rnd-free-concept-mr/save-multiple", model)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }
    */

    /*
    function revise() {
        initializeValidation($formEl, validationConstraints);
        if (!isValidForm($formEl, validationConstraints)) return toastr.error("Please correct all validation errors!");
        else hideValidationErrors($formEl);

        var data = formDataToJson($formEl.serializeArray());
        data.IsComplete = true;
        //data.Childs = $tblChildEl.getCurrentViewRecords();
        var mrs = [];

        mrs.push(data);
        $tblOtherItemEl.getCurrentViewRecords().forEach(function (mr) {
            mr.ReqDate = data.ReqDate;
            mrs.push(mr);
        });

        //var model = {
        //    models: mrs,
        //    grpConceptNo: $formEl.find('#GroupConceptNo').val(),
        //    IsModified: (status == statusConstants.PENDING) ? false : true
        //}
        //New 
        model = mrs;
        model.GroupConceptNo = $formEl.find('#GroupConceptNo').val();
        model.IsModified = (status == statusConstants.PENDING) ? false : true;

        axios.post("/api/rnd-free-concept-mr/revise", model)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }
    */

    function approve() {
        var url = `/api/rnd-free-concept-mr/remove-from-reject/${masterData.FCMRMasterID}`;
        axios.post(url)
            .then(function () {
                toastr.success(constants.ACKNOWLEDGE_SUCCESSFULLY);
                backToList();
            })
            .catch(showResponseError);
    }

    function showAddComposition() {
        initTblCreateComposition();
        $pageEl.find(`#modal-new-composition-${pageId}`).modal("show");
    }

    function initTblCreateComposition() {
        var YarnSubProgramNewsFilteredList = [];//masterData.YarnSubProgramNews;
        var CertificationsFilteredList = [];//masterData.Certifications;
        compositionComponents = [];
        var columns = [
            {
                field: 'Id', isPrimaryKey: true, visible: false
            },
            {
                headerText: '', width: 100, commands: [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                    { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                    { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                    { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }]
            },
            {
                field: 'Percent', headerText: 'Percent(%)', width: 120, editType: "numericedit", params: { decimals: 0, format: "N", min: 1, validateDecimalOnType: true }, allowEditing: isBlended
            },
            //{
            //    field: 'Fiber', headerText: 'Component', editType: 'dropdownedit', edit: new ej2DropdownParams({ dataSource: masterData.FabricComponents, field: "Fiber" })
            //},
            {
                field: 'Fiber', headerText: 'Fiber', valueAccessor: ej2GridDisplayFormatterV2, edit: {
                    create: function () {
                        fiberElem = document.createElement('input');
                        return fiberElem;
                    },
                    read: function () {
                        return fiberObj.text;
                    },
                    destroy: function () {
                        fiberObj.destroy();
                    },
                    write: function (e) {
                        fiberObj = new ej.dropdowns.DropDownList({
                            dataSource: masterData.FabricComponentsNew,
                            fields: { value: 'id', text: 'text' },
                            //enabled: false,
                            placeholder: 'Select Component',
                            floatLabelType: 'Never',
                            change: function (f) {

                                if (!f.isInteracted || !f.itemData) return false;
                                e.rowData.Fiber = f.itemData.id;
                                e.rowData.Fiber = f.itemData.text;

                                YarnSubProgramNewsFilteredList = masterData.YarnSubProgramNews.filter(y => y.additionalValue == f.itemData.id);
                                subProgramObj.dataSource = YarnSubProgramNewsFilteredList;
                                subProgramObj.dataBind();

                                certificationObj.dataSource = [];
                                certificationObj.dataBind();

                                $tblChildEl.updateRow(e.row.rowIndex, e.rowData);
                            }
                        });
                        fiberObj.appendTo(fiberElem);

                    }
                }
            },
            //{
            //    field: 'YarnSubProgramNew', headerText: 'Yarn Sub Program New', editType: 'dropdownedit', edit: new ej2DropdownParams({ dataSource: masterData.YarnSubProgramNews, field: "YarnSubProgramNew" })
            //},
            {
                field: 'YarnSubProgramNew', headerText: 'Yarn Sub Program New', valueAccessor: ej2GridDisplayFormatterV2, edit: {
                    create: function () {
                        subProgramElem = document.createElement('input');
                        return subProgramElem;
                    },
                    read: function () {
                        return subProgramObj.text;
                    },
                    destroy: function () {
                        subProgramObj.destroy();
                    },
                    write: function (e) {
                        subProgramObj = new ej.dropdowns.DropDownList({
                            dataSource: YarnSubProgramNewsFilteredList,
                            fields: { value: 'id', text: 'text' },
                            //enabled: false,
                            placeholder: 'Select Yarn Sub Program',
                            floatLabelType: 'Never',
                            change: function (f) {
                                if (!f.isInteracted || !f.itemData) return false;
                                e.rowData.YarnSubProgramNew = f.itemData.id;
                                e.rowData.YarnSubProgramNew = f.itemData.text;

                                //CertificationsFilteredList = masterData.Certifications.filter(y => y.additionalValue == f.itemData.id);
                                CertificationsFilteredList = masterData.Certifications.filter(y => y.additionalValue == f.itemData.id && y.additionalValue2 == f.itemData.additionalValue);
                                certificationObj.dataSource = CertificationsFilteredList;
                                certificationObj.dataBind();

                                $tblChildEl.updateRow(e.row.rowIndex, e.rowData);
                            }
                        });
                        subProgramObj.appendTo(subProgramElem);
                    }
                }
            },
            //{
            //    field: 'Certification', headerText: 'Certification', editType: 'dropdownedit', edit: new ej2DropdownParams({ dataSource: masterData.Certifications, field: "Certification" })
            //},
            {
                field: 'Certification', headerText: 'Certification', valueAccessor: ej2GridDisplayFormatterV2, edit: {
                    create: function () {
                        certificationElem = document.createElement('input');
                        return certificationElem;
                    },
                    read: function () {
                        return certificationObj.text;
                    },
                    destroy: function () {
                        certificationObj.destroy();
                    },
                    write: function (e) {
                        certificationObj = new ej.dropdowns.DropDownList({
                            dataSource: CertificationsFilteredList,
                            fields: { value: 'id', text: 'text' },
                            //enabled: false,
                            placeholder: 'Select Certification',
                            floatLabelType: 'Never',
                            change: function (f) {

                                if (!f.isInteracted || !f.itemData) return false;
                                e.rowData.Certification = f.itemData.id;
                                e.rowData.Certification = f.itemData.text;

                                $tblChildEl.updateRow(e.row.rowIndex, e.rowData);
                            }
                        });
                        certificationObj.appendTo(certificationElem);
                    }
                }
            }

        ];

        var gridOptions = {
            tableId: tblCreateCompositionId,
            data: compositionComponents,
            columns: columns,
            actionBegin: function (args) {
                if (args.requestType === "add") {
                    if (isBlended) {
                        if (compositionComponents.length === 5) {
                            toastr.info("You can only add 5 components.");
                            args.cancel = true;
                            return;
                        }
                    }
                    else {
                        if (compositionComponents.length === 1) {
                            toastr.info("You can only add 1 component.");
                            args.cancel = true;
                            return;
                        }
                        else args.data.Percent = 100;
                    }

                    args.data.Id = getMaxIdForArray(compositionComponents, "Id");
                }
                else if (args.requestType === "save") {
                    var fiberID = 0;
                    var subProgramID = 0;
                    var certificationsID = 0;
                    if (typeof args.rowData.Fiber != 'undefined') {
                        fiberID = masterData.FabricComponentsNew.find(y => y.text == args.rowData.Fiber).id;
                    }
                    if (typeof args.rowData.YarnSubProgramNew != 'undefined') {
                        subProgramID = masterData.YarnSubProgramNews.find(y => y.text == args.rowData.YarnSubProgramNew).id;
                    }
                    if (typeof args.rowData.Certification != 'undefined') {
                        certificationsID = masterData.Certifications.find(y => y.text == args.rowData.Certification).id;
                    }

                    var cnt = masterData.FabricComponentMappingSetupList.filter(y => y.FiberID == fiberID && y.SubProgramID == subProgramID && y.CertificationsID == certificationsID);
                    if (cnt == 0) {
                        if (fiberID == 0) {
                            toastr.warning("Fiber is required.");
                            args.cancel = true;
                            return;
                        }
                        if (subProgramID == 0) {
                            toastr.warning("Sub Program is required.");
                            args.cancel = true;
                            return;
                        }
                        if (certificationsID == 0) {
                            toastr.warning("certifications is required.");
                            args.cancel = true;
                            return;
                        }
                    }
                    if (args.action === "edit") {
                        if (!args.data.Fiber) {
                            toastr.warning("Fabric component is required.");
                            args.cancel = true;
                            return;
                        }
                        else if (!args.data.Percent || args.data.Percent <= 0 || args.data.Percent > 100) {
                            toastr.warning("Composition percent must be greater than 0 and less than or equal 100.");
                            args.cancel = true;
                            return;
                        }
                    }
                }
            },
            autofitColumns: false,
            showDefaultToolbar: false,
            allowFiltering: false,
            allowPaging: false,
            toolbar: ['Add'],
            editSettings: { allowAdding: true, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true }
        };

        if ($tblCreateCompositionEl) $tblCreateCompositionEl.destroy();
        $tblCreateCompositionEl = new initEJ2Grid(gridOptions);
    }


    function getValidStringValue(value) {
        if (typeof value === "undefined" || value == null) return "";
        return value;
    }

    function getYarnCategory(obj) {
        obj.Segment1ValueDesc = getValidStringValue(obj.Segment1ValueDesc);
        obj.Segment2ValueDesc = getValidStringValue(obj.Segment2ValueDesc);
        obj.Segment3ValueDesc = getValidStringValue(obj.Segment3ValueDesc);
        obj.Segment4ValueDesc = getValidStringValue(obj.Segment4ValueDesc);
        obj.Segment5ValueDesc = getValidStringValue(obj.Segment5ValueDesc);
        obj.Segment6ValueDesc = getValidStringValue(obj.Segment6ValueDesc);
        obj.ShadeCode = getValidStringValue(obj.ShadeCode);

        obj.YarnCategory = GetYarnShortForm(obj.Segment1ValueDesc,
            obj.Segment2ValueDesc,
            obj.Segment3ValueDesc,
            obj.Segment4ValueDesc,
            obj.Segment5ValueDesc,
            obj.Segment6ValueDesc,
            obj.ShadeCode);
        return obj;
    }

    function saveComposition() {
        debugger
        var totalPercent = sumOfArrayItem(compositionComponents, "Percent");
        if (totalPercent != 100) return toastr.error("Sum of compostion percent must be 100");
        compositionComponents.reverse();

        var composition = "";
        compositionComponents = _.sortBy(compositionComponents, "Percent").reverse();
        compositionComponents.forEach(function (component) {
            composition += composition ? ` ${component.Percent}%` : `${component.Percent}%`;
            if (component.YarnSubProgramNew) {
                if (component.YarnSubProgramNew != 'N/A') {
                    composition += ` ${component.YarnSubProgramNew}`;
                }
            }
            //if (component.Certification) composition += ` ${component.Certification}`;
            if (component.Certification) {
                if (component.Certification != 'N/A') {
                    composition += ` ${component.Certification}`;
                }
            }
            composition += ` ${component.Fiber}`;
        });

        var data = {
            SegmentValue: composition
        };

        axios.post("/api/rnd-free-concept-mr/save-yarn-composition", data)
            .then(function () {
                $pageEl.find(`#modal-new-composition-${pageId}`).modal("hide");
                toastr.success("Composition added successfully.");
                //masterData.CompositionList.unshift({ id: response.data.Id, text: response.data.SegmentValue });
                initChildTable(masterData.Childs);
            })
            .catch(showResponseError)
    }
})();