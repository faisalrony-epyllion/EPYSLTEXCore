(function () {
    var menuId, menuParam, pageId, $pageEl, pageName = "YSA";
    var $tblMasterEl, $formEl, tblMasterId, tblChildId, $tblChildEl, $modalRackBinEl, tblRackBinInfoId, $tblRackBinInfoEl;
    var $fiberTypeEl;
    var fiberTypeId, fiberType, isBlendedOrColorMelange;
    var tempBuyerTeamList = [];
    var teamleadersetup, teamleadersetups = [], productSetupChilds = [], productSetupChildPrograms = [], productSetupChildTechnicalParameters = [], ProcessSetupList = [];
    var $divTblEl, $divDetailsEl;
    var status, $toolbarEl;
    var isYSA = false;
    var isYSAApp = false;
    var _defaultStringPropValue = "ZZZZZ";
    var tblCreateItemId, $tblCreateItemEl;
    var _segments = [];
    var _stockObj = {
        PipelineStockRackBins: [],
        QuarantineStockRackBins: [],
        TotalIssueStockRackBins: [],
        AdvanceStockRackBins: [],
        AllocatedStockRackBins: [],
        SampleStockRackBins: [],
        LeftoverStockRackBins: [],
        LiabilitiesStockRackBins: [],
        UnusableStockRackBins: [],
        BlockUnBlockStockRackBins: []
    };
    var _statusObj = {
        IsSendForApproval: false,
        IsApproved: false,
        IsReject: false,
        RejectedReason: ""
    }

    /*
        1	Pipeline Stock
        2	Quarantine Stock
        3	Advance Stock
        4	Allocated Stock
        5	Sample Stock
        6	Leftover Stock
        7	Liabilities Stock
        8	Unusable Stock
        9	Block / UnBlock Stock
    */

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

        $modalRackBinEl = $("#modalRackBin" + pageId);
        tblRackBinInfoId = "#tblRackBins" + pageId;
        tblCreateItemId = `#tblCreateItem-${pageId}`;

        if (menuParam == "YSA") isYSA = true;
        else if (menuParam == "YSAApp") isYSAApp = true;

        tootbarBtnHideShow();

        //btnNewAdjustmentList
        //$toolbarEl.find("#btnNewAdjustment").click(function () {
        //    addNewAdjustment();
        //});
        $toolbarEl.find("#btnNewAdjustmentList").click(function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PENDING;
            initMasterTable();
        });
        $toolbarEl.find("#btnDraftAdjustmentList").click(function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.DRAFT;
            initMasterTable();
        });
        $toolbarEl.find("#btnPendingApprovalAdjustmentList").click(function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PROPOSED_FOR_APPROVAL;
            initMasterTable();
        });
        $toolbarEl.find("#btnApprovedAdjustmentList").click(function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.APPROVED;
            initMasterTable();
        });
        $toolbarEl.find("#btnRejectAdjustmentList").click(function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.REJECT;
            initMasterTable();
        });

        $formEl.find("#btnAddExistingItem").click(function () {
            addExistingItem();
        });

        $formEl.find("#btnBackToList").click(function () {
            backToList();
        });

        $formEl.find(".btnRackBin").click(function () {
            var stockTypeId = parseInt($(this).attr("stockTypeId"));
            setModalTitle(stockTypeId);
            loadRackBins(stockTypeId);
        });
        $formEl.find("#btnOk").click(function () {
            saveRackBins();
        });

        $formEl.find("#btnSave").click(function () {
            resetStatusObj();
            save();
        });
        $formEl.find("#btnSendForApproval").click(function () {
            resetStatusObj();
            _statusObj.IsSendForApproval = true;
            save();
        });
        $formEl.find("#btnApprove").click(function () {
            resetStatusObj();
            _statusObj.IsApproved = true;
            save();
        });
        $formEl.find("#btnReject").click(function () {
            resetStatusObj();
            _statusObj.IsReject = true;
            _statusObj.RejectedReason = "";
            save();
        });

        $formEl.find("#btnAddNewRack").on("click", loadAllRacks);
        $toolbarEl.find("#btnCreateNewItem").click(function (e) {
            addNewItem();
        });
        $pageEl.find("#btnAddItem").click(saveItem);

        getSegments();

        if (isYSA) {
            $toolbarEl.find("#btnNewAdjustmentList").click();
        }
        else if (isYSAApp) {
            $toolbarEl.find("#btnPendingApprovalAdjustmentList").click();
        }
    });
    function initMasterTable() {
        actionBtnHideShow();
        var commands = [];
        if (status == statusConstants.PENDING) {
            commands = [
                { type: 'Add', buttonOption: { cssClass: 'e-flat', iconCss: 'e-add e-icons' } }
            ]
        }
        else {
            commands = [
                { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } }
            ]
        }
        var columns = [
            {
                headerText: '', commands: commands, textAlign: 'Center', width: 75
            },
            {
                field: 'YarnStockSetId', headerText: 'Code'//, visible: false
            },
            {
                field: 'AdjustmentNo', headerText: 'Adjustment No', visible: status != statusConstants.PENDING
            },
            {
                field: 'AdjustmentDate', headerText: 'Adjustment Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, visible: status != statusConstants.PENDING
            },
            {
                field: 'YarnCategory', headerText: 'Item'
            },
            {
                field: 'Count', headerText: 'Count'
            },
            {
                field: 'Reason', headerText: 'Reason', visible: status != statusConstants.PENDING
            },
            {
                field: 'AdjustmentQty', headerText: 'Adjustment Qty', visible: status != statusConstants.PENDING
            },
            {
                field: 'IsPipelineRecord', headerText: 'Is Pipeline', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center'
            },
            {
                field: 'ItemMasterId', headerText: 'ItemMasterId', visible: false
            },
            {
                field: 'SupplierId', headerText: 'SupplierId', visible: false
            },
            {
                field: 'SupplierName', headerText: 'Supplier'
            },
            {
                field: 'SpinnerId', headerText: 'SpinnerId', visible: false
            },
            {
                field: 'SpinnerName', headerText: 'Spinner'
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
                field: 'PipelineStockQtySt', headerText: 'Pipeline Stock', visible: status == statusConstants.PENDING
            },
            {
                field: 'QuarantineStockQtySt', headerText: 'Quarantine Stock', visible: status == statusConstants.PENDING
            },
            {
                field: 'TotalIssueQtySt', headerText: 'Issue Stock', visible: status == statusConstants.PENDING
            },
            {
                field: 'AdvanceStockQtySt', headerText: 'Advance Stock', visible: status == statusConstants.PENDING
            },
            {
                field: 'AllocatedStockQtySt', headerText: 'Allocated Stock', visible: status == statusConstants.PENDING
            },
            {
                field: 'SampleStockQtySt', headerText: 'Sample Stock', visible: status == statusConstants.PENDING
            },
            {
                field: 'LeftoverStockQtySt', headerText: 'Leftover Stock', visible: status == statusConstants.PENDING
            },
            {
                field: 'LiabilitiesStockQtySt', headerText: 'Liabilities Stock', visible: status == statusConstants.PENDING
            },
            {
                field: 'UnusableStockQtySt', headerText: 'Unusable Stock', visible: status == statusConstants.PENDING
            },
            {
                field: 'BlockPipelineStockQtySt', headerText: 'Blocked Pipeline Stock', visible: status == statusConstants.PENDING
            },
            {
                field: 'BlockAdvanceStockQtySt', headerText: 'Blocked Advance Stock', visible: status == statusConstants.PENDING
            },
            {
                field: 'BlockSampleStockQtySt', headerText: 'Blocked Sample Stock', visible: status == statusConstants.PENDING
            },
            {
                field: 'BlockLeftoverStockQtySt', headerText: 'Blocked Leftover Stock', visible: status == statusConstants.PENDING
            },
            {
                field: 'BlockLiabilitiesStockQtySt', headerText: 'Blocked Liabilities Stock', visible: status == statusConstants.PENDING
            }
            //{
            //    field: 'Composition', headerText: 'Composition'
            //},
            //{
            //    field: 'YarnType', headerText: 'Yarn Type'
            //},
            //{
            //    field: 'ManufacturingProcess', headerText: 'Manufacturing Process'
            //},
            //{
            //    field: 'SubProcess', headerText: 'Sub Process'
            //},
            //{
            //    field: 'QualityParameter', headerText: 'Quality Parameter'
            //},
        ];

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: true,
            apiEndPoint: `/api/yarn-stock-adjustment/list?status=${status}`,
            columns: columns,
            commandClick: handleCommands
        });
    }
    function handleCommands(args) {
        if (args.commandColumn.type == 'Add') {
            getNew(args.rowData);
        }
        else if (args.commandColumn.type == 'Edit') {
            getDetail(args.rowData.YSAMasterId);
        }
    }
    async function getSegments() {
        _segments = await axios.get(getYarnItemsApiUrl([]));
        _segments = _segments.data;
    }
    function getNew(obj) {
        resetGlobals();

        var shadeCode = typeof obj.ShadeCode === "undefined" || obj.ShadeCode == null || obj.ShadeCode == "" ? _defaultStringPropValue : obj.ShadeCode;
        shadeCode = replaceInvalidChar(shadeCode);
        var url = `/api/yarn-stock-adjustment/getNew/${obj.IsPipelineRecord}/${obj.ItemMasterId}/${obj.SupplierId}/${shadeCode}/${obj.YarnStockSetId}`;
        axios.get(url)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;

                masterData.AdjustmentDate = formatDateToDefault(new Date());
                $formEl.find(".spnYarnCategory").text(masterData.YarnCategory);
                setFormData($formEl, masterData);

                //$formEl.find(".txtInputNew").prop("readonly", true);
                //if (masterData.IsPipelineRecord) {
                //    $formEl.find(".ppField").prop("readonly", false);
                //} else {
                //    $formEl.find(".nonPPField").prop("readonly", false);
                //}
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function getDetail(id) {
        resetGlobals();

        var url = `/api/yarn-stock-adjustment/getDetail/${id}`;
        axios.get(url)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;

                masterData.AdjustmentDate = formatDateToDefault(new Date());
                $formEl.find(".spnYarnCategory").text(masterData.YarnCategory);
                setFormData($formEl, masterData);
                setChilds(masterData.Childs);

                //$formEl.find(".txtInputNew").prop("readonly", true);
                //if (masterData.IsPipelineRecord) {
                //    $formEl.find(".ppField").prop("readonly", false);
                //} else {
                //    $formEl.find(".nonPPField").prop("readonly", false);
                //}
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function backToList() {
        $divDetailsEl.fadeOut();
        $divTblEl.fadeIn();
        initMasterTable();
    }
    function addNewAdjustment() {
        var url = `/api/yarn-stock-adjustment/new`;
        axios.get(url)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.AdjustmentNo = ch_NewNumberDefaultText;
                masterData.AdjustmentDate = formatDateToDefault(ch_NewDate);
                setFormData($formEl, masterData);
                initChild(masterData.Childs);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function initChild(data) {
        ej.base.enableRipple(true);
        if ($tblChildEl) $tblChildEl.destroy();
        $tblChildEl = new ej.grids.Grid({
            dataSource: data,
            allowResizing: true,
            showDefaultToolbar: false,
            actionBegin: function (args) {
                if (args.requestType === "add") {

                }
                else if (args.requestType === "save") {

                }
            },
            columns: [
                { field: 'YSAChildId', isPrimaryKey: true, visible: false },
                { field: 'YSAMasterId', visible: false },
                { field: 'YarnCategory', headerText: 'Item', allowEditing: false },
                { field: 'ReferanceNo', headerText: 'Referance No', allowEditing: true },
                {
                    field: 'UnitId',
                    headerText: 'Stock Unit',
                    valueAccessor: ej2GridDisplayFormatter,
                    dataSource: masterData.UnitList,
                    allowEditing: true,
                    displayField: "UnitName",
                    edit: ej2GridDropDownObj({
                    })
                },

                { field: 'GSM', headerText: 'GSM', width: 40, allowEditing: false },
                { field: 'DyeingType', headerText: 'Dyeing Type', width: 70, allowEditing: false },
                { field: 'KnittingType', headerText: 'Knitting Type', width: 100, allowEditing: false, visible: false },

                { field: 'YarnProgram', headerText: 'Yarn Program', width: 80, allowEditing: false },
                { field: 'YarnSubProgram', headerText: 'Yarn Sub Program', width: 100, allowEditing: false },

                { field: 'MachineType', headerText: 'Machine Type', width: 100, allowEditing: false, visible: false },
                { field: 'TechnicalName', headerText: 'Technical Name', width: 100, allowEditing: false },
                { field: 'IsSubContact', headerText: 'Sub-Contact?', textAlign: 'Center', width: 70, allowEditing: false, editType: "booleanedit", displayAsCheckBox: true, visible: false },
                { field: 'DeliveryDate', headerText: 'Delivery Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, allowEditing: false, width: 100, visible: false },
                { field: 'FabricWidth', headerText: 'Fabric Width', width: 100, allowEditing: false, visible: false },
                { field: 'YarnType', headerText: 'Yarn Type', width: 70, allowEditing: false },
                { field: 'ReferenceNo', headerText: 'Reference No', width: 100, allowEditing: false, visible: false },
                { field: 'ColorReferenceNo', headerText: 'ColorReference No', width: 100, allowEditing: false, visible: false },
                { field: 'LengthYds', headerText: 'Length (Yds)', width: 100, allowEditing: false, visible: false },
                { field: 'LengthInch', headerText: 'Length (Inch)', width: 100, allowEditing: false, visible: false },
                { field: 'Instruction', headerText: 'Instruction', width: 100, allowEditing: false, visible: false },
                { field: 'LabDipNo', headerText: 'Lab Dip No', width: 100, allowEditing: false, visible: false },
                { field: 'ForBDSStyleNo', headerText: 'Style No', width: 100, allowEditing: false, visible: false },
                { field: 'Qty', headerText: 'Booking Qty', width: 70, allowEditing: false, visible: false },
                { field: 'TotalQty', headerText: 'Req. Knitting Qty', width: 70, allowEditing: false }
            ],
        });
        $tblChildEl.refreshColumns;
        $tblChildEl.appendTo(tblChildId);
    }
    function resetGlobals() {
        $formEl.find(".spnYarnCategory").text("");

        _stockObj = {
            PipelineStockRackBins: [],
            QuarantineStockRackBins: [],
            AdvanceStockRackBins: [],
            AllocatedStockRackBins: [],
            SampleStockRackBins: [],
            LeftoverStockRackBins: [],
            LiabilitiesStockRackBins: [],
            UnusableStockRackBins: [],
            BlockUnBlockStockRackBins: []
        };

        _statusObj = {
            IsSendForApproval: false,
            IsApproved: false,
            IsReject: false,
            RejectedReason: ""
        }
    }
    function resetStatusObj() {
        _statusObj = {
            IsSendForApproval: false,
            IsApproved: false,
            IsReject: false,
            RejectedReason: ""
        }
    }
    function setModalTitle(stockTypeId) {
        var stockName = "";
        switch (stockTypeId) {
            case 1:
                stockName = "Pipeline"
                break;
            case 2:
                stockName = "Quarantine"
                break;
            case 20:
                stockName = "Issue"
                break;
            case 3:
                stockName = "Advance"
                break;
            case 4:
                stockName = "Allocated"
                break;
            case 5:
                stockName = "Sample"
                break;
            case 6:
                stockName = "Leftover"
                break;
            case 7:
                stockName = "Liabilities"
                break;
            case 8:
                stockName = "Unusable"
                break;
            default:
            // code block
        }
        $formEl.find(".spnStockName").text(stockName);
        $formEl.find(".spnStockTypeId").text(stockTypeId);
    }
    function checkUndefiendList(list) {
        if (typeof list === "undefined") return [];
        return list
    }
    function existingChildRackBinIDs() {
        var childRackBinIDs = [];
        _stockObj.PipelineStockRackBins.map(x => childRackBinIDs.push(x.ChildRackBinID));
        _stockObj.QuarantineStockRackBins.map(x => childRackBinIDs.push(x.ChildRackBinID));
        _stockObj.TotalIssueStockRackBins = checkUndefiendList(_stockObj.TotalIssueStockRackBins);
        _stockObj.TotalIssueStockRackBins.map(x => childRackBinIDs.push(x.ChildRackBinID));
        _stockObj.AdvanceStockRackBins.map(x => childRackBinIDs.push(x.ChildRackBinID));
        _stockObj.AllocatedStockRackBins.map(x => childRackBinIDs.push(x.ChildRackBinID));
        _stockObj.SampleStockRackBins.map(x => childRackBinIDs.push(x.ChildRackBinID));
        _stockObj.LeftoverStockRackBins.map(x => childRackBinIDs.push(x.ChildRackBinID));
        _stockObj.LiabilitiesStockRackBins.map(x => childRackBinIDs.push(x.ChildRackBinID));
        _stockObj.UnusableStockRackBins.map(x => childRackBinIDs.push(x.ChildRackBinID));
        childRackBinIDs = childRackBinIDs.join(",");
        return childRackBinIDs;
    }
    function loadRackBins(stockTypeId) {
        var yarnStockSetId = getDefaultValueWhenInvalidN(masterData.YarnStockSetId),
            itemMasterId = getDefaultValueWhenInvalidN(masterData.ItemMasterId),
            supplierId = getDefaultValueWhenInvalidN(masterData.SupplierId),
            spinnerId = getDefaultValueWhenInvalidN(masterData.SpinnerId),
            yarnLotNo = $.trim(masterData.YarnLotNo),
            shadeCode = $.trim(masterData.ShadeCode),
            physicalCount = $.trim(masterData.PhysicalCount);

        var childRackIds = existingChildRackBinIDs();
        childRackIds = getDefaultValueWhenInvalidS(childRackIds) == "" ? _defaultStringPropValue : childRackIds;

        yarnLotNo = getDefaultValueWhenInvalidS(yarnLotNo) == "" ? _defaultStringPropValue : replaceInvalidChar(yarnLotNo);
        shadeCode = getDefaultValueWhenInvalidS(shadeCode) == "" ? _defaultStringPropValue : replaceInvalidChar(shadeCode);
        physicalCount = getDefaultValueWhenInvalidS(physicalCount) == "" ? _defaultStringPropValue : replaceInvalidChar(physicalCount);

        var url = `/api/yarn-stock-adjustment/get-rack-bins/${yarnStockSetId}/
                                                            ${itemMasterId}/
                                                            ${supplierId}/
                                                            ${spinnerId}/
                                                            ${yarnLotNo}/
                                                            ${shadeCode}/
                                                            ${physicalCount}/
                                                            ${childRackIds}`;
        axios.get(url)
            .then(function (response) {
                var list = response.data;
                list = list.sort((a, b) => parseFloat(b.ReceiveQty) - parseFloat(a.ReceiveQty));
                list = list.sort((a, b) => parseFloat(b.NoOfCone) - parseFloat(a.NoOfCone));
                list = list.sort((a, b) => parseFloat(b.NoOfCartoon) - parseFloat(a.NoOfCartoon));

                var currentStockList = getCurrentStockList(stockTypeId);
                if (typeof currentStockList === "undefined") currentStockList = [];
                currentStockList.map(y => {
                    var indexF = list.findIndex(x => x.ChildRackBinID == y.ChildRackBinID);
                    if (indexF > -1) {
                        list.find(x => x.ChildRackBinID == y.ChildRackBinID).YSAChildItemId = y.YSAChildItemId;
                        list.find(x => x.ChildRackBinID == y.ChildRackBinID).YSAChildId = y.YSAChildId;
                        list.find(x => x.ChildRackBinID == y.ChildRackBinID).AdjustCartoon = y.AdjustCartoon;
                        list.find(x => x.ChildRackBinID == y.ChildRackBinID).AdjustCone = y.AdjustCone;
                        list.find(x => x.ChildRackBinID == y.ChildRackBinID).AdjustQtyKg = y.AdjustQtyKg;
                    }
                });
                currentStockList.map(x => {
                    var indexF = list.findIndex(y => y.ChildRackBinID == x.ChildRackBinID);
                    if (indexF == -1) {
                        list.push(x);
                    }
                });
                initRackBinInfo(list);
            });
    }
    function getCurrentStockList(stockTypeId) {
        switch (stockTypeId) {
            case 1:
                return _stockObj.PipelineStockRackBins;
                break;
            case 2:
                return _stockObj.QuarantineStockRackBins;
                break;
            case 20:
                return _stockObj.TotalIssueStockRackBins;
                break;
            case 3:
                return _stockObj.AdvanceStockRackBins;
                break;
            case 4:
                return _stockObj.AllocatedStockRackBins;
                break;
            case 5:
                return _stockObj.SampleStockRackBins;
                break;
            case 6:
                return _stockObj.LeftoverStockRackBins;
                break;
            case 7:
                return _stockObj.LiabilitiesStockRackBins;
                break;
            case 8:
                return _stockObj.UnusableStockRackBins;
                break;
            default:
            // code block
        }
        return [];
    }
    function initRackBinInfo(data) {
        if ($tblRackBinInfoEl) $tblRackBinInfoEl.destroy();
        $modalRackBinEl.modal('show');

        data.map(x => {
            x.AdjustCartoon = typeof x.AdjustCartoon === "undefined" ? 0 : parseFloat(x.AdjustCartoon);
            x.AdjustCone = typeof x.AdjustCone === "undefined" ? 0 : parseFloat(x.AdjustCone);
            x.AdjustQtyKg = typeof x.AdjustQtyKg === "undefined" ? 0 : parseFloat(x.AdjustQtyKg);
        });

        $tblRackBinInfoEl = new initEJ2Grid({
            tableId: tblRackBinInfoId,
            data: data,
            autofitColumns: false,
            allowSorting: true,
            allowPaging: false,
            allowFiltering: false,
            showDefaultToolbar: false,
            enableSingleClickEdit: true,
            editSettings: { allowAdding: true, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
            columns: [
                {
                    headerText: 'Command', width: 100, commands: [
                        { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                        { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                        { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                        { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }
                    ]
                },
                { field: 'ChildRackBinID', isPrimaryKey: true, visible: false, width: 10 },
                { field: 'YSAChildItemId', visible: false, width: 10 },
                { field: 'YSAChildId', visible: false, width: 10 },
                { field: 'LocationName', headerText: 'Location', allowEditing: false, width: 80 },
                { field: 'RackNo', headerText: 'Rack', allowEditing: false, width: 80 },
                { field: 'NoOfCartoon', headerText: 'Rack Cartoon', allowEditing: false, width: 80 },
                { field: 'NoOfCone', headerText: 'Rack Cone', allowEditing: false, width: 80 },
                { field: 'ReceiveQty', headerText: "Rack Qty", allowEditing: false, width: 80 },

                {
                    field: 'AdjustCartoon', headerText: 'Adjust Cartoon', editType: "numericedit", allowEditing: true,
                    edit: { params: { showSpinButton: false, decimals: 0, format: "N0", validateDecimalOnType: true } }, width: 80
                },
                {
                    field: 'AdjustCone', headerText: 'Adjust Cone', editType: "numericedit", allowEditing: true,
                    edit: { params: { showSpinButton: false, decimals: 0, format: "N0", validateDecimalOnType: true } }, width: 80
                },
                {
                    field: 'AdjustQtyKg', headerText: 'Adjust Qty (Kg)', editType: "numericedit", allowEditing: true,
                    edit: { params: { showSpinButton: false, decimals: 2, format: "N0", validateDecimalOnType: true } }, width: 80
                },
            ],
            recordClick: function (args) {

            },
            actionBegin: function (args) {
                if (args.requestType === "add") {

                }
                if (args.requestType === "save") {

                }
                if (args.requestType === "delete") {

                }
            },
        });
    }
    function saveRackBins() {
        var rackBins = $tblRackBinInfoEl.getCurrentViewRecords();
        if (typeof rackBins === "undefined") rackBins = [];
        rackBins = rackBins.filter(x => x.AdjustCartoon != 0 || x.AdjustCone != 0 || x.AdjustQtyKg != 0);
        var stockTypeId = parseInt($formEl.find(".spnStockTypeId").text());

        if (stockTypeId == 0) {
            toastr.error("Stock type missing.");
            return false;
        }
        if (rackBins.length == 0) {
            toastr.error("No record found for save.");
            return false;
        }
        setChildAndChildItems(stockTypeId, rackBins);
        $modalRackBinEl.modal('hide');
    }
    function setChildAndChildItems(stockTypeId, childItems) {
        var totalCartoon = 0,
            totalCone = 0,
            totalQty = 0;
        switch (stockTypeId) {
            case 1:
                _stockObj.PipelineStockRackBins = childItems;
                _stockObj.PipelineStockRackBins.map(x => {
                    totalCartoon += parseFloat(x.AdjustCartoon);
                    totalCone += parseFloat(x.AdjustCone);
                    totalQty += parseFloat(x.AdjustQtyKg);
                });
                $formEl.find("#PipelineStockQtyNew").val(totalQty);
                break;
            case 2:
                _stockObj.QuarantineStockRackBins = childItems;
                _stockObj.QuarantineStockRackBins.map(x => {
                    totalCartoon += parseFloat(x.AdjustCartoon);
                    totalCone += parseFloat(x.AdjustCone);
                    totalQty += parseFloat(x.AdjustQtyKg);
                });
                $formEl.find("#QuarantineStockQtyNew").val(totalQty);
                break;
            case 20:
                _stockObj.TotalIssueStockRackBins = childItems;
                _stockObj.TotalIssueStockRackBins.map(x => {
                    totalCartoon += parseFloat(x.AdjustCartoon);
                    totalCone += parseFloat(x.AdjustCone);
                    totalQty += parseFloat(x.AdjustQtyKg);
                });
                $formEl.find("#TotalIssueQtyNew").val(totalQty);
                break;
            case 3:
                _stockObj.AdvanceStockRackBins = childItems;
                _stockObj.AdvanceStockRackBins.map(x => {
                    totalCartoon += parseFloat(x.AdjustCartoon);
                    totalCone += parseFloat(x.AdjustCone);
                    totalQty += parseFloat(x.AdjustQtyKg);
                });
                $formEl.find("#AdvanceStockQtyNew").val(totalQty);
                break;
            case 4:
                _stockObj.AllocatedStockRackBins = childItems;
                _stockObj.AllocatedStockRackBins.map(x => {
                    totalCartoon += parseFloat(x.AdjustCartoon);
                    totalCone += parseFloat(x.AdjustCone);
                    totalQty += parseFloat(x.AdjustQtyKg);
                });
                $formEl.find("#AllocatedStockQtyNew").val(totalQty);
                break;
            case 5:
                _stockObj.SampleStockRackBins = childItems;
                _stockObj.SampleStockRackBins.map(x => {
                    totalCartoon += parseFloat(x.AdjustCartoon);
                    totalCone += parseFloat(x.AdjustCone);
                    totalQty += parseFloat(x.AdjustQtyKg);
                });
                $formEl.find("#SampleStockQtyNew").val(totalQty);
                break;
            case 6:
                _stockObj.LeftoverStockRackBins = childItems;
                _stockObj.LeftoverStockRackBins.map(x => {
                    totalCartoon += parseFloat(x.AdjustCartoon);
                    totalCone += parseFloat(x.AdjustCone);
                    totalQty += parseFloat(x.AdjustQtyKg);
                });
                $formEl.find("#LeftoverStockQtyNew").val(totalQty);
                break;
            case 7:
                _stockObj.LiabilitiesStockRackBins = childItems;
                _stockObj.LiabilitiesStockRackBins.map(x => {
                    totalCartoon += parseFloat(x.AdjustCartoon);
                    totalCone += parseFloat(x.AdjustCone);
                    totalQty += parseFloat(x.AdjustQtyKg);
                });
                $formEl.find("#LiabilitiesStockQtyNew").val(totalQty);
                break;
            case 8:
                _stockObj.UnusableStockRackBins = childItems;
                _stockObj.UnusableStockRackBins.map(x => {
                    totalCartoon += parseFloat(x.AdjustCartoon);
                    totalCone += parseFloat(x.AdjustCone);
                    totalQty += parseFloat(x.AdjustQtyKg);
                });
                $formEl.find("#UnusableStockQtyNew").val(totalQty);
                break;
            default:
            // code block
        }
    }
    function actionBtnHideShow() {
        ch_actionBtnHideShow($formEl, status, isYSA, isYSAApp, false);
    }
    function tootbarBtnHideShow() {
        $toolbarEl.find(".btnToolBar").hide();
        if (isYSA) {
            $toolbarEl.find("#btnNewAdjustmentList,#btnDraftAdjustmentList,#btnPendingApprovalAdjustmentList,#btnApprovedAdjustmentList,#btnRejectAdjustmentList").show();
        }
        else if (isYSAApp) {
            $toolbarEl.find("#btnPendingApprovalAdjustmentList,#btnApprovedAdjustmentList,#btnRejectAdjustmentList").show();
        }
    }
    function addExistingItem() {

    }
    function save() {
        var data = formDataToJson($formEl.serializeArray());
        if (typeof data.YSAMasterId === "undefined") data.YSAMasterId = 0;
        var childs = [];
        $formEl.find(".txtInputNew").each(function () {
            var qty = parseFloat($(this).val());
            if (qty != 0) {
                var stockTypeId = parseInt($(this).attr("stockTypeId"));
                childs.push({
                    YSAChildId: parseInt($(this).attr("YSAChildId")),
                    YSAMasterId: parseInt($formEl.find("#YSAMasterId").val()),
                    StockTypeId: stockTypeId,
                    AdjustmentTypeId: qty > 0 ? 1 : 2,
                    AdjustmentQty: qty,
                    ChildItems: getCurrentStockList(stockTypeId)
                });
            }
        });

        data.Childs = childs;
        data.YarnStockSetId = masterData.YarnStockSetId;

        if (data.Childs.length == 0) {
            toastr.error("No record found for save.");
            return false;
        }

        data.IsSendForApproval = _statusObj.IsSendForApproval;
        data.IsApproved = _statusObj.IsApproved;
        data.IsReject = _statusObj.IsReject;
        data.RejectedReason = _statusObj.RejectedReason;

        axios.post("/api/yarn-stock-adjustment/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }
    function setChilds(childs) {
        childs.map(x => {
            $formEl.find(".txtInputNew[stockTypeId=" + x.StockTypeId + "]").attr("YSAChildId", x.YSAChildId);
            setChildAndChildItems(x.StockTypeId, x.ChildItems);
        });
    }
    function loadAllRacks(e) {
        e.preventDefault();
        var finder = new commonFinder({
            title: "Rack List",
            pageId: pageId,
            height: 350,
            apiEndPoint: "/api/yarn-rack-bin-allocation/all-rack-list",
            fields: "LocationName,RackNo,NoOfCartoon,NoOfCone,ReceiveQty",
            headerTexts: "Location,Rack,Carton,Cone,Stock Qty",
            widths: "100,80,80,80,80",
            isMultiselect: false,
            primaryKeyColumn: "ChildRackBinID",
            onSelect: function (res) {
                finder.hideModal();
                var currentRacks = $tblRackBinInfoEl.getCurrentViewRecords();
                var indexF = currentRacks.findIndex(x => x.ChildRackBinID == res.rowData.ChildRackBinID)
                if (indexF == -1) {
                    currentRacks.push(res.rowData);
                    currentRacks.map(x => {
                        if (typeof x.YarnStockSetId === "undefined" || x.YarnStockSetId == 0) x.YarnStockSetId = masterData.YarnStockSetId;
                    });
                    initRackBinInfo(currentRacks);
                }
            }
        });
        finder.showModal();
    }
    function addNewItem() {
        var url = `/api/yarn-stock-adjustment/get-related-list/`;
        axios.get(url)
            .then(function (response) {
                var data = response.data;
                initTblCreateItem(data);
                $pageEl.find(`#modal-new-item-${pageId}`).modal("show");
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
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
    async function initTblCreateItem(data) {
        var $modal = $("#modal-new-item-" + pageId);
        setFormData($modal, data);

        compositionItems = [];
        var itemcolumns = await getYarnItemColumnsAsync(data, true);
        itemcolumns = resizeColumns(itemcolumns);
        itemcolumns.push({
            field: 'ShadeCode', headerText: 'Shade Code', allowEditing: false,
            valueAccessor: ej2GridDisplayFormatterV2,
            dataSource: data.ShadeCodes,
            displayField: "ShadeCode",
            edit: ej2GridDropDownObj({
            })
        });
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
    function saveItem() {
        var supplierId = $("#modal-new-item-" + pageId).find("#SupplierId").val();
        if (supplierId == null || typeof supplierId === "undefined" || parseInt(supplierId) == 0) {
            toastr.error("Select supplier");
            return false;
        }
        var spinnerId = $("#modal-new-item-" + pageId).find("#SpinnerId").val();
        if (spinnerId == null || typeof spinnerId === "undefined" || parseInt(spinnerId) == 0) {
            toastr.error("Select spinner");
            return false;
        }
        var yarnLotNo = $("#modal-new-item-" + pageId).find("#YarnLotNo").val();
        if (yarnLotNo == null || typeof yarnLotNo === "undefined" || yarnLotNo.trim().length == 0) {
            toastr.error("Give Lot no");
            return false;
        }
        var physicalCount = $("#modal-new-item-" + pageId).find("#PhysicalCount").val();
        if (physicalCount == null || typeof physicalCount === "undefined" || physicalCount.trim().length == 0) {
            toastr.error("Give physical count");
            return false;
        }
        yarnLotNo = yarnLotNo.trim();
        physicalCount = physicalCount.trim();

        var items = $tblCreateItemEl.getCurrentViewRecords();
        if (items.length == 0) {
            toastr.error("No item found");
            return false;
        }
        if (_segments != null && typeof _segments !== "undefined") {
            items.map(x => {
                x.SupplierId = parseInt(supplierId);
                x.SpinnerID = parseInt(spinnerId);
                x.YarnLotNo = yarnLotNo;
                x.PhysicalCount = physicalCount;
                for (var i = 1; i <= 7; i++) {
                    x["Segment" + i + "ValueDesc"] = getSegmentValueDesc(i, x);
                }
            });
        }
        var obj = {
            YarnReceiveChilds: items
        };
        axios.post("/api/yarn-stock-adjustment/item-save", obj)
            .then(function () {
                toastr.success("Saved successfully.");
                $pageEl.find(`#modal-new-item-${pageId}`).modal("hide");
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }
    function getSegmentValueDesc(segNo, argsObj) {
        var propValue = argsObj["Segment" + segNo + "ValueId"];
        if (typeof propValue === "undefined" || propValue == null) return "";
        var segValueList = _segments["Segment" + segNo + "ValueList"];
        if (typeof segValueList !== "undefined" && segValueList != null && parseInt(propValue) > 0) {
            var seg = segValueList.find(x => x.id == propValue);
            if (typeof seg !== "undefined" && seg != null) return seg.text;
        }
        return "";
    }
})();