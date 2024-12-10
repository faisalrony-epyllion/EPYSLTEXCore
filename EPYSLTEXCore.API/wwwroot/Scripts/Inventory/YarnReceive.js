(function () {
    var menuId, pageName, menuParam;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, tblMasterId, $tblChildEl, tblChildId, $formEl;
    var $pageEl;
    var pageId;
    var filterBy = {};
    var tableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    }
    var status;
    var tblCreateItemId, $tblCreateItemEl;

    var masterData, currentChildRowData;
    var childData;
    var _itemSegmentValues;
    var _isSampleYarn = false;
    var copiedRecord = null;
    var _childID = 1000;
    var _isBuyersChange = false;
    var _selectedIndex = -1;
    var _ignoreValidationPOIds = [];

    var isCDAPage = false;
    var isYarnRcv = false;
    var isYarnRcvApp = false;
    var _actionProps = {
        IsSendForApprove: false,
        IsApproved: false
    };

    $(function () {
        if (!menuId)
            menuId = localStorage.getItem("menuId");
        if (!pageName)
            pageName = localStorage.getItem("pageName");
        if (!menuParam)
            menuParam = localStorage.getItem("menuParam");


        pageId = pageName + "-" + menuId;
        $pageEl = $(`#${pageId}`);
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
        tblChildId = pageConstants.CHILD_TBL_ID_PREFIX + pageId;
        tblCreateItemId = `#tblCreateItem-${pageId}`;
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);

        if (menuParam == "YR") isYarnRcv = true;
        else if (menuParam == "CDA") isCDAPage = true;
        else if (menuParam == "YRA") isYarnRcvApp = true;


        $toolbarEl.find(".btnToolBar").hide();
        if (isYarnRcv || isCDAPage) {
            $toolbarEl.find("#btnPendingReceive,#btnReceiveLists,#btnNewSampleYarn,#btnPendingForApprovalList,#btnApproveList").show();
        } else if (isYarnRcvApp) {
            $toolbarEl.find("#btnPendingForApprovalList,#btnApproveList").show();
        }

        $(".clockpicker").clockpicker({
            autoclose: true,
            default: 'now'
        });

        $toolbarEl.find("#btnPendingReceive").click(function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PendingReceivePO;
            $toolbarEl.find("#PendingType").fadeIn();
            $toolbarEl.find("#btnRdoCommercialInvoice").prop("checked", false);
            $toolbarEl.find("#btnRdoPurchaseOrder").prop("checked", true);
            initMasterTable();
        });
        $toolbarEl.find("#btnNewSampleYarn").click(function (e) {
            e.preventDefault();
            loadNewSampleYarn();
        });
        $toolbarEl.find("#btnReceiveLists").click(function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.DRAFT;
            $toolbarEl.find("#PendingType").fadeOut();
            initMasterTable();
        });
        $toolbarEl.find("#btnPendingForApprovalList").click(function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PROPOSED_FOR_APPROVAL;
            $toolbarEl.find("#PendingType").fadeOut();
            initMasterTable();
        });
        $toolbarEl.find("#btnApproveList").click(function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.APPROVED;
            $toolbarEl.find("#PendingType").fadeOut();
            initMasterTable();
        });


        $toolbarEl.find("#btnRdoCommercialInvoice,#lblCommercialInvoice").click(function (e) {
            e.preventDefault();
            //toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PendingReceiveCI;
            initMasterTable();
            _isSampleYarn = false;
            $toolbarEl.find("#btnRdoPurchaseOrder").prop("checked", false);
            $toolbarEl.find("#btnRdoCommercialInvoice").prop("checked", true);
        });

        $toolbarEl.find("#btnRdoPurchaseOrder,#lblPurchaseOrder").click(function (e) {
            e.preventDefault();
            //toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PendingReceivePO;
            initMasterTable();
            _isSampleYarn = false;
            $toolbarEl.find("#btnRdoPurchaseOrder").prop("checked", true);
            $toolbarEl.find("#btnRdoCommercialInvoice").prop("checked", false);
        });


        $formEl.find("#btnSaveYR").click(function (e) {
            e.preventDefault();

            save();
        });
        $formEl.find("#btnSendForApproval").click(function (e) {
            e.preventDefault();
            _actionProps.IsSendForApprove = true;
            save();
        });
        $formEl.find("#btnApprove").click(function (e) {
            e.preventDefault();
            _actionProps.IsApproved = true;
            save();
        });

        $toolbarEl.find("#btnRdoPurchaseOrder").click();
        $formEl.find("#btnYREditCancel").on("click", backToList);
        $formEl.find("#btnSupplierName").click(function (e) {
            e.preventDefault();
            var finder = new commonFinder({
                title: "Supplier List",
                pageId: pageId,
                data: masterData.SupplierList,
                isMultiselect: false,
                modalSize: "modal-md",
                top: "2px",
                primaryKeyColumn: "id",
                fields: "text",
                headerTexts: "Supplier Name",
                widths: "30",
                onSelect: function (res) {
                    finder.hideModal();
                    masterData.SupplierID = res.rowData.id;
                    $formEl.find("#SupplierID").val(res.rowData.id);
                    $formEl.find("#SupplierName").val(res.rowData.text);
                },
            });
            finder.showModal();
        });
        $formEl.find("#btnCompanyName").click(function (e) {
            e.preventDefault();
            var finder = new commonFinder({
                title: "Company List",
                pageId: pageId,
                data: masterData.RCompanyList,
                isMultiselect: false,
                modalSize: "modal-md",
                top: "2px",
                primaryKeyColumn: "id",
                fields: "text",
                headerTexts: "Company Name",
                widths: "30",
                onSelect: function (res) {
                    finder.hideModal();
                    masterData.RCompanyID = res.rowData.id;
                    $formEl.find("#RCompanyID").val(res.rowData.id);
                    $formEl.find("#RCompany").val(res.rowData.text);
                },
            });
            finder.showModal();
        });

        $formEl.find("#btnAddItemPopup").click(function (e) {
            showAddItem();
        });
        $pageEl.find("#btnAddItem").click(saveItem);

        if (isYarnRcv || isCDAPage) {
            $toolbarEl.find("#btnPendingReceive").click();
        }
        else if (isYarnRcvApp) {
            $toolbarEl.find("#btnPendingForApprovalList").click();
        }

        axios.get(getYarnItemsApiUrl([])).then(res => {
            _itemSegmentValues = res.data;
        });
    });

    function loadNewSampleYarn() {

        _isSampleYarn = true;
        actionBtnHideShow();
        axios.get(`/api/yarn-receive/new/sample-yarn`)
            .then(function (response) {
                $formEl.find(".clsHideShow").hide();

                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.ReceiveDate = formatDateToDefault(masterData.ReceiveDate);
                masterData.PODate = formatDateToDefault(masterData.PODate);
                //masterData.PiDate = formatDateToDefault(masterData.PiDate);
                masterData.LCDate = formatDateToDefault(masterData.LCDate);
                masterData.InvoiceDate = formatDateToDefault(masterData.InvoiceDate);
                masterData.ChallanDate = formatDateToDefault(masterData.ChallanDate);
                masterData.TruckChallanDate = formatDateToDefault(masterData.TruckChallanDate);
                masterData.MushakDate = formatDateToDefault(masterData.MushakDate);

                setFormData($formEl, masterData);
                fieldsHideShow();

                initChildTable([]);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function fieldsHideShow() {
        if (_isSampleYarn) {
            $formEl.find(".clsHideShow").hide();
            $formEl.find(".PO").fadeOut();
            //$formEl.find(".CI").fadeOut();
            $formEl.find(".TOL").fadeOut();
            $formEl.find("#divACompanyInvoice").fadeOut();
            $formEl.find("#ShipmentStatus").prop("disabled", true);
            $formEl.find("#btnSupplierName").fadeIn();
            $formEl.find("#btnCompanyName").fadeIn();
            $formEl.find("#btnAddItemPopup").fadeIn();
            $formEl.find("#SupplierName,#RCompany").css({
                "width": "82%",
                "float": "left"
            });
        } else {
            $formEl.find(".clsHideShow").show();
            $formEl.find("#btnSupplierName").fadeOut();
            $formEl.find("#btnCompanyName").fadeOut();
            $formEl.find("#btnAddItemPopup").fadeOut();
            $formEl.find("#SupplierName,#RCompany").css({
                "width": "100%",
            });
        }
    }

    function showAddItem() {
        initTblCreateItem();
        $pageEl.find(`#modal-new-item-${pageId}`).modal("show");
    }
    function resizeColumns(columnList) {
        var cAry = ["Segment1ValueId", "Segment2ValueId", "Segment3ValueId", "Segment4ValueId", "Segment5ValueId", "Segment6ValueId"];
        cAry.map(c => {
            var indexF = columnList.findIndex(x => x.field == c);
            var widthValue = 62;
            if (c == "Segment1ValueId") widthValue = 180;
            if (indexF > -1) columnList[indexF].width = widthValue;
        });
        return columnList;
    }
    async function initTblCreateItem() {
        compositionItems = [];
        var itemcolumns = await getYarnItemColumnsAsync([], true);
        itemcolumns = resizeColumns(itemcolumns);
        var columns = [
            {
                field: 'Id', isPrimaryKey: true, visible: false
            },
            {
                headerText: '', width: 20, commands: [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                    { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                    { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                    { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }]
            },
        ];
        columns.push.apply(itemcolumns);
        var gridOptions = {
            tableId: tblCreateItemId,
            data: compositionItems,
            columns: itemcolumns,
            actionBegin: function (args) {
                if (args.requestType === "add") {
                    args.data.Id = getMaxIdForArray(compositionItems, "Id");
                }
                else if (args.requestType === "save" && args.action === "edit") {
                    ////args.data.Segment1ValueId = !args.rowData.Segment1ValueId ? 0 : args.rowData.Segment1ValueId;
                    //args.data.Segment2ValueId = !args.rowData.Segment2ValueId ? 0 : args.rowData.Segment2ValueId;
                    //args.data.Segment3ValueId = !args.rowData.Segment3ValueId ? 0 : args.rowData.Segment3ValueId;
                    //args.data.Segment4ValueId = !args.rowData.Segment4ValueId ? 0 : args.rowData.Segment4ValueId;
                    //args.data.Segment5ValueId = !args.rowData.Segment5ValueId ? 0 : args.rowData.Segment5ValueId;
                    //args.data.Segment6ValueId = !args.rowData.Segment6ValueId ? 0 : args.rowData.Segment6ValueId;
                    //args.data.Segment7ValueId = !args.rowData.Segment7ValueId ? 0 : args.rowData.Segment7ValueId;
                    //args.data.Segment8ValueId = !args.rowData.Segment8ValueId ? 0 : args.rowData.Segment8ValueId;
                }
            },
            autofitColumns: false,
            showDefaultToolbar: false,
            allowFiltering: false,
            allowPaging: false,
            toolbar: ['Add'],
            editSettings: { allowAdding: true, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true }
        };

        if ($tblCreateItemEl) $tblCreateItemEl.destroy();
        $tblCreateItemEl = new initEJ2Grid(gridOptions);
    }
    function DeepClone(obj) {
        return JSON.parse(JSON.stringify(obj));
    }
    async function saveItem() {
        var itemList = [];
        var yarnReceiveChilds = $tblChildEl.getCurrentViewRecords();
        if (typeof yarnReceiveChilds !== "undefined" && yarnReceiveChilds.length > 0) {
            itemList = yarnReceiveChilds;
        }

        for (var i = 0; i < compositionItems.length; i++) {
            compositionItems[i].ChildID = _childID++;

            if (compositionItems[i].Segment1ValueId) {
                compositionItems[i].Segment1ValueDesc = _itemSegmentValues.Segment1ValueList.find(x => x.id == compositionItems[i].Segment1ValueId).text;
            }
            if (compositionItems[i].Segment2ValueId) {
                compositionItems[i].Segment2ValueDesc = _itemSegmentValues.Segment2ValueList.find(x => x.id == compositionItems[i].Segment2ValueId).text;
            }
            if (compositionItems[i].Segment3ValueId) {
                compositionItems[i].Segment3ValueDesc = _itemSegmentValues.Segment3ValueList.find(x => x.id == compositionItems[i].Segment3ValueId).text;
            }
            if (compositionItems[i].Segment4ValueId) {
                compositionItems[i].Segment4ValueDesc = _itemSegmentValues.Segment4ValueList.find(x => x.id == compositionItems[i].Segment4ValueId).text;
            }
            if (compositionItems[i].Segment5ValueId) {
                compositionItems[i].Segment5ValueDesc = _itemSegmentValues.Segment5ValueList.find(x => x.id == compositionItems[i].Segment5ValueId).text;
            }
            if (compositionItems[i].Segment6ValueId) {
                compositionItems[i].Segment6ValueDesc = _itemSegmentValues.Segment6ValueList.find(x => x.id == compositionItems[i].Segment6ValueId).text;
            }
            itemList.push(DeepClone(compositionItems[i]));
        }
        compositionItems = DeepClone(itemList);
        initChildTable(compositionItems);
        $pageEl.find(`#modal-new-item-${pageId}`).modal("hide");
    }
    function isDisplayColumn() {
        if (status == statusConstants.DRAFT || status == statusConstants.PROPOSED_FOR_APPROVAL || status == statusConstants.APPROVED) return true;
        return false;
    }
    function initMasterTable() {
        var commands = [];
        if (status === statusConstants.PendingReceiveCI || status === statusConstants.PendingReceivePO || status === statusConstants.PendingReceiveSF) {
            commands = [
                { type: 'New', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-plus' } }
            ]
        } else if (status === statusConstants.DRAFT) {
            commands = [
                { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-delete e-icons' } },
                { type: 'Yarn Control Sheet', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
            ]
        }
        else {
            commands = [
                { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                { type: 'Yarn Control Sheet', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
            ]
        }

        var columns = [
            {
                headerText: '', commands: commands, textAlign: 'Center', width: ch_setActionCommandCellWidth(commands)
            },
            {
                field: 'YarnReceiveType', headerText: 'Type', visible: isDisplayColumn()
            },
            {
                field: 'PONo', headerText: 'PO No', visible: (status === statusConstants.PendingReceivePO || status === statusConstants.PendingReceiveSF || isDisplayColumn())
            },
            {
                field: 'LCNo', headerText: 'LC No', visible: (status === statusConstants.PendingReceiveCI || isDisplayColumn() || status == statusConstants.PendingReceivePO)
            },
            {
                field: 'InvoiceNo', headerText: 'Invoice No', textAlign: 'Right', visible: (status === statusConstants.PendingReceiveCI || isDisplayColumn())
            },
            {
                field: 'ReceiveNo', headerText: 'Receive No', visible: isDisplayColumn()
            },
            {
                field: 'ReceiveDate', headerText: 'Receive Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, visible: isDisplayColumn()
            },
            {
                field: 'ApprovedDate', headerText: 'Approved Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, visible: status == statusConstants.APPROVED
            },
            {
                field: 'ApprovedByName', headerText: 'Approved By', visible: status == statusConstants.APPROVED
            },
            {
                //field: 'InvoiceDate', headerText: 'Invoice Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, visible: (status === statusConstants.PendingReceiveCI || status === statusConstants.DRAFT)
                field: 'InvoiceDate', headerText: 'Invoice Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, visible: (status === statusConstants.PendingReceiveCI)
            },
            {
                field: 'InvoiceValue', headerText: 'Invoice Value', textAlign: 'Right', visible: (status === statusConstants.PendingReceiveCI)
            },
            {
                //field: 'LCDate', headerText: 'LC Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, visible: (status === statusConstants.PendingReceiveCI || status === statusConstants.DRAFT)
                field: 'LCDate', headerText: 'LC Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, visible: (status === statusConstants.PendingReceiveCI)
            },

            {
                field: 'ChallanNo', headerText: 'Challan No', textAlign: 'Right', visible: isDisplayColumn()
            },

            {
                field: 'ChallanDate', headerText: 'Challan Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, visible: isDisplayColumn()

            },
            {
                field: 'PODate', headerText: 'PO Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, visible: (status === statusConstants.PendingReceivePO || status === statusConstants.PendingReceiveSF)
            },
            {
                field: 'PINo', headerText: 'PI No', visible: (status === statusConstants.PendingReceivePO || status === statusConstants.PendingReceiveSF || isDisplayColumn())
            },
            {
                field: 'QuotationRefNo', headerText: 'Quotation Ref No', visible: (status === statusConstants.PendingReceivePO || status === statusConstants.PendingReceiveSF)
            },
            {
                field: 'SupplierName', headerText: 'Supplier'
            },
            {
                field: 'TransportAgencyName', headerText: 'Agency', visible: isDisplayColumn()
            },
            {
                field: 'LocationName', headerText: 'Store Location', visible: isDisplayColumn()
            },
            {
                field: 'VehicalNo', headerText: 'Vehicle', visible: isDisplayColumn()
            },
            {
                field: 'RCompany', headerText: 'Rcv. Company'
            }
        ];

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: true,
            apiEndPoint: `/api/yarn-receive/list?status=${status}&isCDAPage=${isCDAPage}`,
            columns: columns,
            commandClick: handleCommands
        });
    }

    function handleCommands(args) {
        if (args.commandColumn.type == 'New') {
            $formEl.find("#SupplierName,#RCompany").css({
                "width": "100%",
            });
            if (status === statusConstants.PendingReceiveCI) {
                $formEl.find(".PO").fadeOut();
                $formEl.find(".CI").fadeIn();
                $formEl.find("#divACompanyInvoice").fadeOut();
                $formEl.find("#ShipmentStatus").prop("disabled", false);
                $formEl.find("#btnSupplierName").fadeOut();
                $formEl.find("#btnCompanyName").fadeOut();
                $formEl.find("#btnAddItemPopup").fadeOut();
            }
            else if (status === statusConstants.PendingReceivePO) {
                $formEl.find(".PO").fadeIn();
                $formEl.find(".CI").fadeOut();
                $formEl.find("#divACompanyInvoice").fadeIn();
                $formEl.find("#ShipmentStatus").prop("disabled", false);
                $formEl.find("#btnSupplierName").fadeOut();
                $formEl.find("#btnCompanyName").fadeOut();
                $formEl.find("#btnAddItemPopup").fadeOut();
            }
            //else if (status === statusConstants.PendingReceiveSF) {
            //    $formEl.find(".PO").fadeOut();
            //    //$formEl.find(".CI").fadeOut();
            //    $formEl.find(".TOL").fadeOut();
            //    $formEl.find("#divACompanyInvoice").fadeOut();
            //    $formEl.find("#ShipmentStatus").prop("disabled", true);
            //    $formEl.find("#btnSupplierName").fadeIn();
            //    $formEl.find("#btnCompanyName").fadeIn();
            //    $formEl.find("#btnAddItemPopup").fadeIn();
            //    $formEl.find("#SupplierName,#RCompany").css({
            //        "width": "82%",
            //        "float": "left"
            //    });
            //}
            getNew(args.rowData.CIID, args.rowData.POID);
        }
        else if (args.commandColumn.type == 'Edit') {
            getDetails(args.rowData.ReceiveID, args.rowData.POID);
        }
        else if (args.commandColumn.type == 'Delete') {
            deleteYarnReceive(args.rowData.ReceiveID);
        }
        else if (args.commandColumn.type == 'Yarn Control Sheet') {
            window.open(`/reports/InlinePdfView?ReportName=YarnReceiveControlSheet.rdl&ReceiveNo=${args.rowData.ReceiveNo}`, '_blank');
        }

    }

    //function loadSapinnerWisePackingList() {
    //    var sapinnerWisePackingList = [];
    //    if (masterData.SupplierID > 0) {
    //        return masterData.SpinnerWisePackingList.filter(x=>x.);
    //    }
    //}

    var spinnerElem, spinnerObj, packNoElem, packNoObj;

    async function initChildTable(data) {
        if ($tblChildEl) $tblChildEl.destroy();
        var columns = [];

        if (isCDAPage) {
            columns.push(
                { field: 'Segment1ValueDesc', headerText: 'Item Name', allowEditing: false, width: 80 },
                { field: 'Segment2ValueDesc', headerText: 'Agent Name', allowEditing: false, width: 80 }
            )
        }
        else {
            columns = [
                {
                    headerText: '', commands: [
                        { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } }
                    ], width: 60
                }
            ];
            //columns = await getYarnItemColumnsForDisplayOnly();
            columns.push.apply(columns, await getYarnItemColumnsForDisplayOnly());
            columns.push.apply(columns, [{ field: 'ShadeCode', headerText: 'Shade Code', allowEditing: _isSampleYarn, width: 100 }]);
        }

        var additionalColumns = [
            //{ field: 'YarnSubProgramNames', headerText: 'Yarn Sub Program', allowEditing: false },
            { field: 'ChildID', isPrimaryKey: true, visible: false, width: 10 },
            {
                field: 'ReceiveForId',
                headerText: 'Receive For',
                valueAccessor: ej2GridDisplayFormatter,
                dataSource: masterData.ReceiveForList,
                displayField: "text",
                valueFieldName: "id",
                visible: _isSampleYarn,
                width: 80,
                edit: ej2GridDropDownObj({
                })
            },
            { field: 'ChallanCount', headerText: 'Challan Count', width: 120, textAlign: 'left' },
            { field: 'PhysicalCount', headerText: 'Physical Count', width: 120, textAlign: 'left' },
            //{
            //    field: 'BuyerNames',
            //    headerText: 'Buyer',
            //    minWidth: 120,
            //    maxWidth: 220,
            //    allowEditing: _isSampleYarn,
            //    edit: ej2GridMultipleDropDownObj({
            //        dataSource: masterData.BuyerList,
            //        displayField: "YarnChildPoBuyerIds",
            //        valueFieldName: "YarnChildPoBuyerIds",
            //        onChange: function (selectedData, currentRowData) {
            //        }
            //    })
            //},
            { field: 'YarnChildPoExportIds', headerText: 'EWO', visible: false },
            {
                field: 'YarnChildPoEWOs',
                headerText: 'EWO',
                minWidth: 120,
                maxWidth: 220,
                allowEditing: _isSampleYarn,
                //visible: _isSampleYarn
            },
            {
                headerText: '',
                visible: _isSampleYarn,
                textAlign: 'Center',
                width: 40,
                commands: [
                    {
                        buttonOption: {
                            type: 'AddEWO', content: '', cssClass: 'btn btn-success btn-xs',
                            iconCss: 'fa fa-search'
                        }
                    }
                ]
            },
            /*
            {
                field: 'SpinnerID',
                headerText: 'Spinner',
                valueAccessor: ej2GridDisplayFormatter,
                dataSource: masterData.SpinnerList,
                displayField: "text",
                width: 130,
                edit: ej2GridDropDownObj({
                })
            },
            {
                field: 'YarnPackingID',
                headerText: 'Pack No',
                valueAccessor: ej2GridDisplayFormatter,
                dataSource: masterData.SpinnerWisePackingList,
                displayField: "text",
                width: 130,
                edit: ej2GridDropDownObj({
                })
            },
            */
            {
                field: 'SpinnerName', headerText: 'Spinner', width: 130, valueAccessor: ej2GridDisplayFormatterV2, edit: {
                    create: function () {
                        spinnerElem = document.createElement('input');
                        return spinnerElem;
                    },
                    read: function () {
                        return spinnerObj.value;
                    },
                    destroy: function () {
                        spinnerObj.destroy();
                    },
                    write: function (e) {
                        spinnerObj = new ej.dropdowns.DropDownList({
                            dataSource: masterData.SpinnerList,
                            fields: { value: 'id', text: 'text' },

                            placeholder: 'Select Spinner',
                            floatLabelType: 'Never',
                            allowFiltering: true,
                            popupWidth: 'auto',
                            filtering: async function (e) {
                                var query = new ej.data.Query();
                                query = (e.text != "") ? query.where(fields.text, "contains", e.text, true) : query;
                                e.updateData(dataSource, query);
                            },

                            change: function (f) {
                                packNoObj.enabled = true;
                                var tempQuery = new ej.data.Query().where(ej.data.Predicate.or([
                                    new ej.data.Predicate('desc', 'equal', spinnerObj.value),
                                    new ej.data.Predicate('desc', 'equal', '0')
                                ]))
                                packNoObj.query = tempQuery;
                                packNoObj.text = null;
                                packNoObj.dataBind();

                                e.rowData.SpinnerID = f.itemData.id;
                                e.rowData.SpinnerName = f.itemData.text;
                            },
                            placeholder: 'Select Spinner',
                            floatLabelType: 'Never'
                        });
                        spinnerObj.appendTo(spinnerElem);
                    }
                }
            },
            {
                field: 'PackNo', headerText: 'Pack No', width: 130, valueAccessor: ej2GridDisplayFormatterV2, edit: {
                    create: function () {
                        packNoElem = document.createElement('input');
                        return packNoElem;
                    },
                    read: function () {
                        return packNoObj.value;
                    },
                    destroy: function () {
                        packNoObj.destroy();
                    },
                    write: function (e) {
                        packNoObj = new ej.dropdowns.DropDownList({
                            dataSource: masterData.SpinnerWisePackingList,
                            fields: { value: 'id', text: 'text' },
                            //enabled: false,
                            placeholder: 'Select Pack No',
                            floatLabelType: 'Never',
                            allowFiltering: true,
                            popupWidth: 'auto',
                            filtering: async function (e) {

                                var query = new ej.data.Query();
                                query = (e.text != "") ? query.where(fields.text, "contains", e.text, true) : query;
                                e.updateData(dataSource, query);
                            },

                            change: function (f) {
                                if (!f.isInteracted || !f.itemData) return false;
                                e.rowData.YarnPackingID = f.itemData.id;
                                e.rowData.PackNo = f.itemData.text;
                            }
                        });
                        packNoObj.appendTo(packNoElem);
                    }
                }
            },
            { field: 'YarnControlNo', headerText: 'Control No', allowEditing: false, visible: status != statusConstants.PendingReceivePO && status != statusConstants.PendingReceiveCI, textAlign: 'center', width: 80 },
            { field: 'ChallanLot', headerText: 'Challan Lot', textAlign: 'left', width: 120 },
            { field: 'LotNo', headerText: 'Physical Lot', textAlign: 'left', width: 120 },
            { field: 'DisplayUnitDesc', headerText: 'Unit', allowEditing: false, width: 80 },
            { field: 'POQty', headerText: 'PO Qty', allowEditing: false, width: 80 },
            { field: 'InvoiceQty', headerText: 'Invoice Qty', allowEditing: false, width: 80 },
            { field: 'ReceivedQty', headerText: 'Received Qty', allowEditing: false, width: 80 },
            { field: 'BalanceQty', headerText: 'Balance Qty', allowEditing: false, width: 80 },
            { field: 'NoOfCartoon', headerText: 'No Of Cartoon', textAlign: 'center', width: 80, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 1 } } },
            { field: 'NoOfCone', headerText: 'No Of Cone', textAlign: 'center', width: 80, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 1 } } },
            { field: 'ChallanQty', headerText: 'Challan/PL Qty', textAlign: 'right', width: 80, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 2, min: 1 } } },
            { field: 'ReceiveQty', headerText: 'Receive Qty', textAlign: 'right', width: 80, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 2, min: 1 } } },
            { field: 'ExcessQty', headerText: 'Excess Qty', textAlign: 'right', allowEditing: false, width: 80 },
            { field: 'ShortQty', headerText: 'Short Qty', textAlign: 'right', allowEditing: false, width: 80 },
            { field: 'Remarks', headerText: 'Remarks', textAlign: 'left', width: 400 }
        ];


        columns.push.apply(columns, additionalColumns);

        columns = setMandatoryFieldsCSS(columns, "ChallanCount,PhysicalCount,SpinnerID,ChallanLot,LotNo,NoOfCartoon,NoOfCone,ChallanQty,ReceiveQty");

        var buyerField = {};
        if (masterData.IsSampleYarn || _isSampleYarn) {
            buyerField = {
                field: 'BuyerNames',
                headerText: 'Buyer',
                minWidth: 120,
                maxWidth: 220,
                allowEditing: _isSampleYarn,
                edit: ej2GridMultipleDropDownObj({
                    dataSource: masterData.BuyerList,
                    displayField: "YarnChildPoBuyerIds",
                    valueFieldName: "YarnChildPoBuyerIds",
                    onChange: function (selectedData, currentRowData) {
                    }
                })
            };
        } else {
            buyerField = {
                field: 'BuyerNames',
                headerText: 'Buyer',
                minWidth: 120,
                maxWidth: 220,
                allowEditing: _isSampleYarn,
            };
        }

        var indexF = columns.findIndex(x => x.field == "PhysicalCount");
        if (indexF > -1) {
            indexF = indexF + 1;
            columns.splice(indexF, 0, buyerField);
        }

        $tblChildEl = new initEJ2Grid({
            tableId: tblChildId,
            data: data,
            columns: columns,
            allowPaging: false,
            allowResizing: true,
            actionBegin: function (args) {
                if (args.requestType === "beginEdit") {
                    if (args.rowData.NoOfCone == 0) args.rowData.NoOfCone = "";
                    if (args.rowData.NoOfCartoon == 0) args.rowData.NoOfCartoon = "";
                    if (args.rowData.ChallanQty == 0) args.rowData.ChallanQty = "";
                    if (args.rowData.ReceiveQty == 0) args.rowData.ReceiveQty = "";
                    if (args.rowData.Remarks == 0) args.rowData.Remarks = "";

                }
                else if (args.requestType === "save") {

                    if (typeof args.rowData !== "undefined") {
                        args.data.SpinnerID = args.rowData.SpinnerID;
                        args.data.SpinnerName = args.rowData.SpinnerName;

                        args.data.YarnPackingID = args.rowData.YarnPackingID;
                        args.data.PackNo = args.rowData.PackNo;
                    }

                    if (args.data.YarnPackingID > 0) {
                        var indexF = masterData.SpinnerWisePackingList.findIndex(x => x.id == args.data.YarnPackingID && x.desc == args.data.SpinnerID);
                        if (indexF == -1) {
                            args.data.YarnPackingID = 0;
                            args.data.PackNo = "";
                            toastr.error("Invalid pack for selected spinner.");
                        }
                        else if (indexF > -1) {
                            args.data.ReceiveQty = getDefaultValueWhenInvalidN_Float(args.data.ReceiveQty);

                            var cone = getDefaultValueWhenInvalidN(masterData.SpinnerWisePackingList[indexF].additionalValue);
                            var netWt = getDefaultValueWhenInvalidN_Float(masterData.SpinnerWisePackingList[indexF].additionalValue2);

                            args.data.NoOfCartoon = netWt > 0 ? args.data.ReceiveQty / netWt : 0;
                            args.data.NoOfCone = args.data.NoOfCartoon * cone;

                            args.data.NoOfCartoon = getDefaultValueWhenInvalidN_FloatWithFourDigit(args.data.NoOfCartoon);
                            args.data.NoOfCone = getDefaultValueWhenInvalidN_FloatWithFourDigit(args.data.NoOfCone);

                            args.data.NoOfCartoon = Math.ceil(args.data.NoOfCartoon);
                            args.data.NoOfCone = Math.ceil(args.data.NoOfCone);
                        }
                    }

                    if (args.data.POQty > 0 && args.data.ReceiveQty > args.data.MaxReceiveQty) {
                        toastr.error("Receive Qty(" + args.data.ReceiveQty + ") should not greater than " + args.data.MaxReceiveQty + " !!");
                        if (args.data.NoOfCartoon == null) args.data.NoOfCartoon = 0;
                        if (args.data.NoOfCone == null) args.data.NoOfCone = 0;
                        if (args.data.ChallanQty == null) args.data.ChallanQty = 0;
                        args.data.ReceiveQty = 0;

                        if (args.data.YarnPackingID > 0) {
                            args.data.NoOfCartoon = 0;
                            args.data.NoOfCone = 0;
                        }

                        return;
                    }
                    if (args.data.ReceiveForId != null && args.data.ReceiveForId != 0) {
                        args.data.ReceiveForName = masterData.ReceiveForList.find(y => y.id == args.data.ReceiveForId).text;
                    }
                    if (args.data.NoOfCone == "" || args.data.NoOfCone == null) args.data.NoOfCone = 0;
                    if (args.data.NoOfCartoon == "" || args.data.NoOfCartoon == null) args.data.NoOfCartoon = 0;
                    if (args.data.ChallanQty == "" || args.data.ChallanQty == null) args.data.ChallanQty = 0;
                    if (args.data.ReceiveQty == "" || args.data.ReceiveQty == null) args.data.ReceiveQty = 0;
                    if (args.data.Remarks == "" || args.data.Remarks == null) args.data.Remarks = 0;

                    if (parseFloat(args.data.ChallanQty) > parseFloat(args.data.ReceiveQty)) {
                        args.data.ShortQty = parseFloat(args.data.ChallanQty) - parseFloat(args.data.ReceiveQty);
                        args.data.ExcessQty = 0;
                    } else if (parseFloat(args.data.ChallanQty) < parseFloat(args.data.ReceiveQty)) {
                        //args.data.ExcessQty = parseFloat(args.data.ReceiveQty) - parseFloat(args.data.ChallanQty);
                        args.data.ExcessQty = parseFloat(args.data.ReceiveQty) - parseFloat(args.data.BalanceQty);
                        args.data.ShortQty = 0;
                    } else {
                        args.data.ExcessQty = 0;
                        args.data.ShortQty = 0;
                    }
                    if (args.data.ExcessQty == null || isNaN(parseFloat(args.data.ExcessQty)) == true) args.data.ExcessQty = 0;

                    _selectedIndex = -1;
                    if (copiedRecord != null) {
                        //masterData.YarnReceiveChilds.push(DeepClone(copiedRecord));
                        copiedRecord = null;
                    }
                    else {
                        var index = $tblChildEl.getRowIndexByPrimaryKey(args.rowData.ChildID);
                        _selectedIndex = index;

                        args.data.BuyerNames = args.data.BuyerNames == "" ? args.rowData.BuyerNames : args.data.BuyerNames;
                        args.data.YarnChildPoBuyerIds = getBuyerIDs(args.data.BuyerNames);

                        if (args.data.BuyerNames) {
                            args.data.YarnReceiveChildBuyers = [];
                            var buyerNames = args.data.BuyerNames.split(',');
                            buyerNames.map(bn => {
                                var buyer = masterData.BuyerList.find(x => x.text == bn);
                                if (buyer) {
                                    args.data.YarnReceiveChildBuyers.push({
                                        ReceiveChildID: args.data.ChildID,
                                        BuyerID: buyer.id
                                    });
                                }
                            });
                        }
                        masterData.YarnReceiveChilds[index] = args.data;
                    }

                }
                else if (args.requestType === "delete") {
                    var YRCList = $tblChildEl.getCurrentViewRecords();
                    masterData.YarnReceiveChilds[0].ChildID = YRCList.map(function (el) {
                        if (args.data[0].ChildID != el.ChildID) {
                            return el.ChildID
                        }
                    }).toString();

                }
                //else if (args.requestType === "add") {
                //    currentChildRowData = new YarnPRChild();
                //    masterData.ReceiveChilds.push(currentChildRowData);
                //    args.data.Id = currentChildRowData.Id;
                //    args.data = currentChildRowData;
                //}
            },
            autofitColumns: true,
            showDefaultToolbar: false,
            allowFiltering: false,
            toolbar: ['ColumnChooser'],
            //toolbar: ['Add'],
            editSettings: { allowAdding: true, allowEditing: true, allowDeleting: true }, // mode: "Normal", showDeleteConfirmDialog: true
            enableContextMenu: true,
            contextMenuItems: [
                { text: 'Copy', target: '.e-content', id: 'copy' },
                { text: 'Paste', target: '.e-content', id: 'paste' },
                { text: 'Copy And Paste', target: '.e-content', id: 'copyAndPaste' },
            ],
            contextMenuClick: function (args) {
                if (args.item.id === 'copy') {
                    copiedRecord = objectCopy(args.rowInfo.rowData);
                    copiedRecord.ChildID = _childID++;
                }
                else if (args.item.id === 'paste') {
                    if (copiedRecord == null) {
                        toastr.error("Please copy first!!");
                        return;
                    }

                    args.rowData = DeepClone(copiedRecord);
                    $tblChildEl.addRecord(args.rowData);
                }
                else if (args.item.id === 'copyAndPaste') {
                    copiedRecord = objectCopy(args.rowInfo.rowData);
                    copiedRecord.ChildID = _childID++;
                    $tblChildEl.addRecord(DeepClone(copiedRecord));
                }
            },
            commandClick: childCommandClick,
        });
    }
    async function childCommandClick(e) {
        childData = e.rowData;
        if (e.commandColumn.buttonOption.type == 'AddEWO') {
            if (childData.YarnChildPoBuyerIds == null || childData.YarnChildPoBuyerIds == "") {
                toastr.info("You must select buyer first.");
                return;
            }
            var finder = new commonFinder({
                title: "Select EWO",
                pageId: pageId,
                height: 320,
                apiEndPoint: `/api/ypo/ewo-list/${childData.YarnChildPoBuyerIds}`,
                fields: "EWONo,IsSample,BuyerName,BuyerTeam",
                headerTexts: "EWO,Is Sample?,Buyer,Buyer Team",
                isMultiselect: true,
                autofitColumns: true,
                primaryKeyColumn: "EWONo",
                selectedIds: childData.YarnChildPoEWOs,
                seperateSelection: false,
                onMultiselect: function (selectedRecords) {
                    finder.hideModal();
                    childData.YarnReceiveChildOrders = [];
                    for (var i = 0; i < selectedRecords.length; i++) {
                        var selectedValue = selectedRecords[i];
                        var YarnReceiveChildOrder = {
                            ReceiveChildID: childData.ChildID,
                            ExportOrderID: selectedValue.ExportOrderId,
                            EWONo: selectedValue.EWONo,
                            IsSample: selectedValue.IsSample,
                            BuyerID: selectedValue.BuyerID,
                            BuyerTeamID: selectedValue.BuyerTeamID,
                            BuyerName: selectedValue.BuyerName
                        }
                        childData.YarnReceiveChildOrders.push(YarnReceiveChildOrder);
                    }
                    childData.YarnChildPoEWOs = childData.YarnReceiveChildOrders.map(function (item) {
                        return item.EWONo;
                    }).join(",");
                    childData.YarnChildPoExportIds = childData.YarnReceiveChildOrders.map(function (item) {
                        return item.ExportOrderID;
                    }).join(",");

                    var index = $tblChildEl.getRowIndexByPrimaryKey(childData.ChildID);
                    $tblChildEl.updateRow(index, childData);
                }
            });
            finder.showModal();
        }
    }
    function getBuyerIDs(buyerNames) {
        var buyerIds = [];
        if (buyerNames != null && buyerNames.trim().length > 0 && masterData.BuyerList.length > 0) {
            var splitBuyers = buyerNames.split(',');
            splitBuyers.map(x => {
                var obj = masterData.BuyerList.find(b => b.text.trim() == x.trim());
                if (obj) {
                    buyerIds.push(obj.id);
                }
            });
        }
        var buyerIdsComma = "";
        if (buyerIds.length > 0) {
            buyerIdsComma = buyerIds.join(",");
        }
        return buyerIdsComma;
    }
    function backToList() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        initMasterTable();
    }

    function resetForm() {
        $formEl.trigger("reset");
        $formEl.find("#ReceiveID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function resetTableParams() {
        tableParams.offset = 0;
        tableParams.limit = 10;
        tableParams.filter = '';
        tableParams.sort = '';
        tableParams.order = '';
    }

    function getNew(CIID, POID) {

        actionBtnHideShow();
        axios.get(`/api/yarn-receive/new/${CIID}/${POID}/${isCDAPage}`)
            .then(function (response) {
                $formEl.find(".clsHideShow").show();
                resetGlobalObj();
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.ReceiveDate = formatDateToDefault(masterData.ReceiveDate);
                masterData.PODate = formatDateToDefault(masterData.PODate);
                //masterData.PiDate = formatDateToDefault(masterData.PiDate);
                masterData.LCDate = formatDateToDefault(masterData.LCDate);
                masterData.InvoiceDate = formatDateToDefault(masterData.InvoiceDate);
                masterData.ChallanDate = formatDateToDefault(masterData.ChallanDate);
                masterData.PLDate = formatDateToDefault(masterData.PLDate);
                masterData.GPDate = formatDateToDefault(masterData.GPDate);
                masterData.TruckChallanDate = formatDateToDefault(masterData.TruckChallanDate);
                masterData.MushakDate = formatDateToDefault(masterData.MushakDate);


                _isSampleYarn = false;
                fieldsHideShow();
                setFormData($formEl, masterData);
                initChildTable(masterData.YarnReceiveChilds);

                _ignoreValidationPOIds = masterData.IgnoreValidationPOIds;
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function deleteYarnReceive(receiveId) {
        if (status === statusConstants.DRAFT) {
            showBootboxConfirm("Delete Record.", "Are you sure want to delete?", function (yes) {
                if (yes) {
                    var data = {
                        ReceiveID: receiveId
                    };
                    axios.post("/api/yarn-receive/delete", data)
                        .then(function () {
                            toastr.success("Successfully deleted.");
                            $tblMasterEl.refresh();
                        })
                        .catch(function (error) {
                            toastr.error(error.response.data.Message);
                        });
                }
            });
        }
    }

    function getDetails(id, poId) {
        actionBtnHideShow();
        axios.get(`/api/yarn-receive/${id}/${poId}`)
            .then(function (response) {
                $formEl.find(".clsHideShow").show();
                resetGlobalObj();
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;

                masterData.ReceiveDate = formatDateToDefault(masterData.ReceiveDate);
                masterData.PODate = formatDateToDefault(masterData.PODate);
                //masterData.PiDate = formatDateToDefault(masterData.PiDate);
                masterData.LCDate = formatDateToDefault(masterData.LCDate);
                masterData.InvoiceDate = formatDateToDefault(masterData.InvoiceDate);
                masterData.ChallanDate = formatDateToDefault(masterData.ChallanDate);
                masterData.PLDate = formatDateToDefault(masterData.PLDate);
                masterData.GPDate = formatDateToDefault(masterData.GPDate);
                masterData.TruckChallanDate = formatDateToDefault(masterData.TruckChallanDate);
                masterData.MushakDate = formatDateToDefault(masterData.MushakDate);

                setFormData($formEl, masterData);
                initChildTable(masterData.YarnReceiveChilds);

                _ignoreValidationPOIds = masterData.IgnoreValidationPOIds;


                if (masterData.IsSampleYarn) {
                    _isSampleYarn = true;
                    fieldsHideShow();

                    $formEl.find("#btnCompanyName").fadeOut();
                    $formEl.find("#RCompany").css({
                        "width": "100%"
                    });
                } else {
                    _isSampleYarn = false;
                    fieldsHideShow();

                    if (masterData.PONo == null || masterData.PONo == "") {
                        $formEl.find(".PO").fadeOut();
                        $formEl.find(".CI").fadeIn();
                        $formEl.find("#divACompanyInvoice").fadeOut();
                    } else {
                        $formEl.find(".PO").fadeIn();
                        $formEl.find(".CI").fadeOut();
                        $formEl.find("#divACompanyInvoice").fadeIn();
                    }
                }
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function checkItemInfos(list) {
        var maxSeg = 6;
        for (var i = 0; i < list.length; i++) {
            for (var indexS = 1; indexS <= maxSeg; indexS++) {
                if (list[i]["Segment" + indexS + "ValueDesc"] && list[i]["Segment" + indexS + "ValueId"] == 0) {
                    var obj = _itemSegmentValues["Segment" + indexS + "ValueList"].find(x => x.text == list[i]["Segment" + indexS + "ValueDesc"]);
                    if (obj) {
                        list[i]["Segment" + indexS + "ValueId"] = obj.id;
                    }
                }
            }
            /*
            if (data.list[i].Segment1ValueDesc && data.list[i].Segment1ValueId == 0) {
                data.list[i].Segment1ValueId = _itemSegmentValues.Segment1ValueList.find(x => x.text == data.list[i].Segment1ValueDesc).id;
            }
            if (data.list[i].Segment2ValueDesc && data.list[i].Segment2ValueId == 0) {
                data.list[i].Segment2ValueId = _itemSegmentValues.Segment2ValueList.find(x => x.text == data.list[i].Segment2ValueDesc).id;
            }
            if (data.list[i].Segment3ValueDesc && data.list[i].Segment3ValueId == 0) {
                data.list[i].Segment3ValueId = _itemSegmentValues.Segment3ValueList.find(x => x.text == data.list[i].Segment3ValueDesc).id;
            }
            if (data.list[i].Segment4ValueDesc && data.list[i].Segment4ValueId == 0) {
                data.list[i].Segment4ValueId = _itemSegmentValues.Segment4ValueList.find(x => x.text == data.list[i].Segment4ValueDesc).id;
            }
            if (data.list[i].Segment5ValueDesc && data.list[i].Segment5ValueId == 0) {
                data.list[i].Segment5ValueId = _itemSegmentValues.Segment5ValueList.find(x => x.text == data.list[i].Segment5ValueDesc).id;
            }
            if (data.list[i].Segment6ValueDesc && data.list[i].Segment6ValueId == 0) {

                data.list[i].Segment6ValueId = _itemSegmentValues.Segment6ValueList.find(x => x.text == data.list[i].Segment6ValueDesc).id;
            }
            */
        }
        return list;
    }
    function setYarnSegDesc(obj) {
        for (var indexSeg = 1; indexSeg <= 6; indexSeg++) {
            var segIdProp = "Segment" + indexSeg + "ValueId";
            var segDescProp = "Segment" + indexSeg + "ValueDesc";
            var listName = "Segment" + indexSeg + "ValueList";

            if (obj[segIdProp] > 0 && (typeof obj[segDescProp] !== "undefined" || obj[segDescProp] != "")) {
                var objSeg = _itemSegmentValues[listName].find(s => s.id == obj[segIdProp]);
                if (objSeg) {
                    obj[segDescProp] = objSeg.text;
                }
            }
        }
        obj = getYarnCategory(obj);
        return obj;
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

    function isInvalidSegment(segId) {
        if (typeof segId === "undefined" || segId == null || segId == 0) return true;
        return false;
    }

    function save() {
        var data = formDataToJson($formEl.serializeArray());

        var today = new Date();
        today.setHours(0, 0, 0, 0);

        var challanDate = new Date(data.ChallanDate);
        challanDate.setHours(0, 0, 0, 0);
        if (challanDate > today) {
            return toastr.error("Challan Date should not greater than today.");
        }

        var pLDate = new Date(data.PLDate);
        pLDate.setHours(0, 0, 0, 0);
        if (pLDate > today) {
            return toastr.error("PL Date should not greater than today.");
        }

        var truckChallanDate = new Date(data.TruckChallanDate);
        truckChallanDate.setHours(0, 0, 0, 0);
        if (truckChallanDate > today) {
            return toastr.error("Truck Challan Date should not greater than today.");
        }

        var gPDate = new Date(data.GPDate);
        gPDate.setHours(0, 0, 0, 0);
        if (gPDate > today) {
            return toastr.error("GP Date should not greater than today.");
        }


        var mushakDate = new Date(data.MushakDate);
        mushakDate.setHours(0, 0, 0, 0);
        if (mushakDate > today) {
            return toastr.error("Mushak Date should not greater than today.");
        }

        data.IsSendForApprove = _actionProps.IsSendForApprove;
        data.IsApproved = _actionProps.IsApproved;

        data.IsCDA = isCDAPage;
        data.TransportMode = 0;
        data.YarnReceiveChilds = $tblChildEl.getCurrentViewRecords();
        data.YarnReceiveChilds = checkItemInfos(data.YarnReceiveChilds);
        //Validation Set in Maste

 

        //initializeValidation($formEl, validationConstraints);
        if (!isValidForm($formEl, validationConstraints)) return toastr.error("Please correct all validation errors!");
        else hideValidationErrors($formEl);



        if (data.YarnReceiveChilds.length === 0) {
            return toastr.error("At least 1 item is required.");
        }

        if (data.ChallanNo == null || data.ChallanNo == "") {
            toastr.error("Give Challan No");
            return false;
        }

        if (_actionProps.IsSendForApprove) {
            var currentDate = new Date();
            if (isInvalidDate(data.ReceiveDate, currentDate)) {
                toastr.error("Receive date cannot be greater then current date.");
                _actionProps.IsSendForApprove = false;
                return false;
            }
            if (isInvalidDate(data.ChallanDate, currentDate)) {
                toastr.error("Challan date cannot be greater then current date.");
                _actionProps.IsSendForApprove = false;
                return false;
            }
            if (isInvalidDate(data.PLDate, currentDate)) {
                toastr.error("PL date cannot be greater then current date.");
                _actionProps.IsSendForApprove = false;
                return false;
            }
            if (isInvalidDate(data.TruckChallanDate, currentDate)) {
                toastr.error("Truck challan date cannot be greater then current date.");
                _actionProps.IsSendForApprove = false;
                return false;
            }
            if (isInvalidDate(data.GPDate, currentDate)) {
                toastr.error("GP date cannot be greater then current date.");
                _actionProps.IsSendForApprove = false;
                return false;
            }
            if (isInvalidDate(data.MushakDate, currentDate)) {
                toastr.error("Mushak date cannot be greater then current date.");
                _actionProps.IsSendForApprove = false;
                return false;
            }
        }

        var hasError = false;
        for (var i = 0; i < data.YarnReceiveChilds.length; i++) {
            var child = data.YarnReceiveChilds[i],
                rowNo = i + 1;

            child.ChallanCount = getDefaultValueWhenInvalidS(child.ChallanCount);
            child.PhysicalCount = getDefaultValueWhenInvalidS(child.PhysicalCount);
            child.SpinnerID = getDefaultValueWhenInvalidN(child.SpinnerID);
            child.YarnPackingID = getDefaultValueWhenInvalidN(child.YarnPackingID);
            child.ChallanLot = getDefaultValueWhenInvalidS(child.ChallanLot);
            child.LotNo = getDefaultValueWhenInvalidS(child.LotNo);
            child.NoOfCartoon = getDefaultValueWhenInvalidN(child.NoOfCartoon);
            child.NoOfCone = getDefaultValueWhenInvalidN(child.NoOfCone);
            child.ChallanQty = getDefaultValueWhenInvalidN_FloatWithFourDigit(child.ChallanQty);
            child.ReceiveQty = getDefaultValueWhenInvalidN_FloatWithFourDigit(child.ReceiveQty);

            if (child.YarnPackingID > 0) {
                var indexF = masterData.SpinnerWisePackingList.findIndex(x => x.id == child.YarnPackingID && x.desc == child.SpinnerID);
                if (indexF == -1) {
                    child.YarnPackingID = 0;
                    child.PackNo = "";
                    toastr.error("Invalid pack for selected spinner. Row(" + rowNo + ")");
                    hasError = true;
                    break;
                }
            }

            if (_actionProps.IsSendForApprove) {
                if (child.ChallanCount.length == 0) {
                    toastr.error("Give Challan Count. Row(" + rowNo + ")");
                    hasError = true;
                    break;
                }
                if (child.PhysicalCount.length == 0) {
                    toastr.error("Give Physical Count. Row(" + rowNo + ")");
                    hasError = true;
                    break;
                }
                if (child.SpinnerID == 0) {
                    toastr.error("Select Spinner. Row(" + rowNo + ")");
                    hasError = true;
                    break;
                }
                if (child.ChallanLot.length == 0) {
                    toastr.error("Give Challan Lot. Row(" + rowNo + ")");
                    hasError = true;
                    break;
                }
                if (child.LotNo.length == 0) {
                    toastr.error("Give Physical Lot. Row(" + rowNo + ")");
                    hasError = true;
                    break;
                }
                if (child.NoOfCartoon == 0) {
                    toastr.error("Give No of Cartoon. Row(" + rowNo + ")");
                    hasError = true;
                    break;
                }
                if (child.NoOfCone == 0) {
                    toastr.error("Give No of Cone. Row(" + rowNo + ")");
                    hasError = true;
                    break;
                }
                if (child.ChallanQty == 0) {
                    toastr.error("Give Challan Qty. Row(" + rowNo + ")");
                    hasError = true;
                    break;
                }
                if (child.ReceiveQty == 0) {
                    toastr.error("Give Receive Qty. Row(" + rowNo + ")");
                    hasError = true;
                    break;
                }
            }

            //if (!_isSampleYarn) {
            //    if (child.ReceiveQty > child.BalanceQty) {
            //        toastr.error("Receive Qty (" + child.ReceiveQty + ") cannot be greater then balance Qty (" + child.BalanceQty+"). Row(" + rowNo + ")");
            //        hasError = true;
            //        break;
            //    }
            //}

            data.YarnReceiveChilds[i] = setYarnSegDesc(data.YarnReceiveChilds[i]);

            if (!isIgnoreValidation(masterData.POID)) {
                child = data.YarnReceiveChilds[i];
                if ((child.Segment5ValueDesc.toLowerCase() == "melange" || child.Segment5ValueDesc.toLowerCase() == "color melange") && (child.ShadeCode == null || child.ShadeCode == "")) {
                    toastr.error("Select shade code for color melange");
                    hasError = true;
                    break;
                }
            }
        }
        if (hasError) return false;

        if (_isSampleYarn) {
            data.IsSampleYarn = true;

            masterData.SupplierID = getDefaultValueWhenInvalidN(masterData.SupplierID);
            if (masterData.SupplierID == 0) {
                toastr.error("Select supplier");
                return false;
            }

            masterData.RCompanyID = getDefaultValueWhenInvalidN(masterData.RCompanyID);
            if (masterData.RCompanyID == 0) {
                toastr.error("Select company");
                return false;
            }

            hasError = false;
            for (var iChild = 0; iChild < data.YarnReceiveChilds.length; iChild++) {
                var child = data.YarnReceiveChilds[iChild];

                child.ReceiveForId = getDefaultValueWhenInvalidN(child.ReceiveForId);
                if (child.ReceiveForId == 0) {
                    toastr.error("Select receive for (At row " + (iChild + 1) + ").");
                    hasError = true;
                    break;
                }

                var buyerIds = child.YarnChildPoBuyerIds;
                if (buyerIds) {
                    buyerIds = buyerIds.split(',');
                }
                var orders = child.YarnReceiveChildOrders;
                if (orders != null && orders.length > 0) {
                    for (var iOrder = 0; iOrder < orders.length; iOrder++) {
                        var fIndex = buyerIds.findIndex(x => x == orders[iOrder].BuyerID);
                        if (fIndex == -1) {
                            toastr.error("Invalid EWO, select buyer wise EWO (At row " + (iChild + 1) + ").");
                            hasError = true;
                            break;
                        }
                    }
                }

                if (isInvalidSegment(child.Segment1ValueId)) {
                    toastr.error("Select composition");
                    hasError = true;
                    break;
                }
                if (isInvalidSegment(child.Segment2ValueId)) {
                    toastr.error("Select yarn type");
                    hasError = true;
                    break;
                }
                if (isInvalidSegment(child.Segment3ValueId)) {
                    toastr.error("Select manufacturing process");
                    hasError = true;
                    break;
                }
                if (isInvalidSegment(child.Segment4ValueId)) {
                    toastr.error("Select sub process");
                    hasError = true;
                    break;
                }
                if (isInvalidSegment(child.Segment5ValueId)) {
                    toastr.error("Select quality parameter");
                    hasError = true;
                    break;
                }
                if (isInvalidSegment(child.Segment6ValueId)) {
                    toastr.error("Select count");
                    hasError = true;
                    break;
                }
                if ((child.Segment5ValueDesc.toLowerCase() == "melange" || child.Segment5ValueDesc.toLowerCase() == "color melange") && (child.ShadeCode == null || child.ShadeCode == "")) {
                    toastr.error("Select shade code for color melange");
                    hasError = true;
                    break;
                }
                if (hasError) break;
            }

        } else {
            data.IsSampleYarn = false;
        }

        if (hasError) return false;

        axios.post("/api/yarn-receive/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                resetGlobalObj();
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }


    function calculateCITotalInvoiceValue(data) {
        var ciLCValue = 0;

        $.each(data, function (i, row) {
            ciLCValue += isNaN(parseFloat(row.CIValue)) ? 0 : parseFloat(row.CIValue);
        });

        return ciLCValue.toFixed(2);
    }

    function calculateYarnReceiveCITotalPIQty(data) {
        var ciYRPIQty = 0;

        $.each(data, function (i, row) {
            ciYRPIQty += isNaN(parseFloat(row.POQty)) ? 0 : parseFloat(row.POQty);
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

    function calculateCITotalExcessQty(data) {
        var yExchessQty = 0;

        $.each(data, function (i, row) {
            yExchessQty += isNaN(parseFloat(row.ExcessQty)) ? 0 : parseFloat(row.ExcessQty);
        });

        return yExchessQty.toFixed(2);
    }

    function calculateCITotalShortQty(data) {
        var yShortQty = 0;

        $.each(data, function (i, row) {
            yShortQty += isNaN(parseFloat(row.ShortQty)) ? 0 : parseFloat(row.ShortQty);
        });

        return yShortQty.toFixed(2);
    }
    var validationConstraints = {
        ReceiveDate: {
            presence: true
        },
        LocationID: {
            presence: true
        },
        TransportTypeID: {
            presence: true
        },
        CContractorID: {
            presence: true
        }
        //SpinnerID: {
        //    presence: true
        //}
    }
    function isIgnoreValidation(poId) {
        if (poId > 0) {
            var indexV = _ignoreValidationPOIds.findIndex(x => x == poId);
            if (indexV > -1) return true;
        }
        return false;
    }
    function actionBtnHideShow() {
        $formEl.find(".btnAction").hide();
        if (isYarnRcv || isCDAPage) {
            if (status == statusConstants.PendingReceivePO || status == statusConstants.DRAFT || _isSampleYarn) {
                $formEl.find("#btnSaveYR,#btnSendForApproval").show();
            }
        }
        else if (isYarnRcvApp) {
            if (status == statusConstants.PROPOSED_FOR_APPROVAL) {
                $formEl.find("#btnApprove").show();
            }
        }
    }
    function resetGlobalObj() {
        _actionProps = {
            IsSendForApprove: false,
            IsApproved: false
        };

        _isSampleYarn = false;
    }
    function setChangeProps(previousData, currentData) {
        if (!masterData.IsSampleYarn) {

            var currentBuyers = getProperString(currentData.BuyerNames); // = "C&A,CARLINGS"
            var previousBuyers = getProperString(previousData.BuyerNames); // = "C&A,CARLINGS"

            if (currentBuyers != previousBuyers) {
                currentData.BuyerNames = previousData.BuyerNames;
            }

            var currentEWOs = getProperString(currentData.YarnChildPoEWOs); // = "243783,243670,243113"
            var previousEWOs = getProperString(previousData.YarnChildPoEWOs); // = "243783,243670,243113"

            if (currentEWOs != previousEWOs) {
                currentData.YarnChildPoEWOs = previousData.YarnChildPoEWOs;
            }
            //YarnChildPoEWOs = "243783,243670,243113"
        }
        return currentData;
    }
    function getProperString(propValue) {
        propValue = propValue.split(',');
        propValue.map(b => b = $.trim(b));
        propValue = propValue.sort();
        propValue = propValue.join(',');
        return propValue;
    }
})();