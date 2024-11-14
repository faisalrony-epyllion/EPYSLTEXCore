(function () {
    var menuId, pageName;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, tblMasterId, $tblChildEl, $formEl, $modalPlanningEl, $tblStockInfoEl, tblStockInfoId, $modalPlanningUnuseableEl, $tblStockInfoUnuseableEl, tblStockInfoUnuseableId;
    var filterBy = {};
    var tableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    }
    var status;

    var masterData;
    var pageId = "";

    var _childRackBins = [];
    var _selectedKYLOReturnChildID = 0;
    var _maxKYLORRCRBId = 999;
    var _existingRackBinsReciveChildWise = [];
    var _childRackBinsUnuseable = [];
    var _existingRackBinsReciveChildWiseUnuseable = [];
    var _useableStockSetId = 0;
    var _unuseableStockSetId = 0;
    var _returnFormType = "";

    $(function () {
        if (!menuId)
            menuId = localStorage.getItem("menuId");
        if (!pageName)
            pageName = localStorage.getItem("pageName");

        pageId = pageName + "-" + menuId;
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        //$tblMasterEl = $(tblMasterId.MASTER_TBL_ID_PREFIX + pageId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
        $tblChildEl = $(pageConstants.CHILD_TBL_ID_PREFIX + pageId);
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);
        $modalPlanningEl = $("#modalPlanning" + pageId);
        tblStockInfoId = pageConstants.STOCK_INFO_PREFIX + pageId;
        $modalPlanningUnuseableEl = $("#modalPlanningUnuseable" + pageId);
        tblStockInfoUnuseableId = "#tblStockInfoUnuseable" + pageId;

        status = statusConstants.PENDING;
        CreateToolBarHideShow();
        initMasterTable();
        initChildTable();

        $toolbarEl.find("#btnPendingList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PENDING;
            CreateToolBarHideShow();
            initMasterTable();
            $formEl.find("#btnSave").show();
        });

        $toolbarEl.find("#btnReturnReceiveList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.COMPLETED;
            CreateToolBarHideShow();
            initMasterTable();
            $formEl.find("#btnSave").hide();
        });

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save(this);
        });

        $formEl.find("#btnSaveAndAapprove").click(function (e) {
            e.preventDefault();
            save(this, true);
        });

        $formEl.find("#btnCancel").on("click", backToList);
        $toolbarEl.find("#btnNewCreateReturnReceive").click(function () {
            getNewDataForKSCLOReturn();
        });
        $formEl.find("#btnAddNewRack").on("click", loadAllRacks);
        $formEl.find("#btnAddNewRackUnuseable").on("click", loadAllRacksUnuseable);
        $formEl.find("#btnOk").click(function () {

            var hasErrorRack = false;
            if (_selectedKYLOReturnChildID > 0) {
                var rackList = DeepClone($tblStockInfoEl.getCurrentViewRecords());
                var childList = masterData.Childs;//DeepClone($tblChildEl.getCurrentViewRecords());

                var indexF = childList.findIndex(x => x.KYLOReturnChildID == _selectedKYLOReturnChildID);
                if (indexF > -1) {
                    var totalUseableReceiveQty = 0;
                    var totalUseableReceiveQtyCone = 0;
                    var totalUseableReceiveQtyCarton = 0;

                    for (var iRack = 0; iRack < rackList.length; iRack++) {

                        var rack = rackList[iRack];
                        //-------------------------------- START Add data to _childRackBins ------------------------------
                        rack.ReceiveCartoon = getDefaultValueWhenInvalidN(rack.ReceiveCartoon);
                        rack.ReceiveQtyCone = getDefaultValueWhenInvalidN(rack.ReceiveQtyCone);
                        rack.ReceiveQtyKg = getDefaultValueWhenInvalidN(rack.ReceiveQtyKg);

                        var indexEX = _existingRackBinsReciveChildWise.findIndex(x => x.KYLOReturnChildID == _selectedKYLOReturnChildID);
                        if (indexEX > -1) {
                            var rackIndex = _existingRackBinsReciveChildWise[indexEX].ChildRackBinIDs.findIndex(x => x == rack.ChildRackBinID);
                            if (rackIndex == -1) {
                                rack.RackBinType = "New";
                            }
                        }

                        var indexF = _childRackBins.findIndex(x => x.KYLOReturnChildID == _selectedKYLOReturnChildID);
                        if (indexF == -1) {
                            _childRackBins.push({
                                KYLOReturnChildID: _selectedKYLOReturnChildID,
                                ChildRackBins: []
                            });
                            indexF = _childRackBins.findIndex(x => x.KYLOReturnChildID == _selectedKYLOReturnChildID);
                            if (indexF > -1) {
                                if (typeof rack.KYLORRCRBId === "undefined") rack.KYLORRCRBId = 0;
                                if (rack.KYLORRCRBId == 0) rack.KYLORRCRBId = _maxKYLORRCRBId++;
                                if (rack.YarnStockSetId == 0) rack.YarnStockSetId = _useableStockSetId;
                                _childRackBins[indexF].ChildRackBins.push(rack);
                            }
                        } else {
                            var indexC = _childRackBins[indexF].ChildRackBins.findIndex(x => x.ChildRackBinID == rack.ChildRackBinID);
                            if (indexC == -1) {
                                if (typeof rack.KYLORRCRBId === "undefined") rack.KYLORRCRBId = 0;
                                if (rack.KYLORRCRBId == 0) rack.KYLORRCRBId = _maxKYLORRCRBId++;
                                if (rack.YarnStockSetId == 0) rack.YarnStockSetId = _useableStockSetId;
                                _childRackBins[indexF].ChildRackBins.push(rack);
                            } else {
                                if (rack.YarnStockSetId == 0) rack.YarnStockSetId = _useableStockSetId;
                                _childRackBins[indexF].ChildRackBins[indexC] = rack;
                            }
                        }
                        //-------------------------------- END Add data to _childRackBins ------------------------------
                        totalUseableReceiveQty += rack.ReceiveQtyKg;
                        totalUseableReceiveQtyCone += rack.ReceiveQtyCone;
                        totalUseableReceiveQtyCarton += rack.ReceiveCartoon;
                    }
                    childList[indexF].UseableReceiveQtyKG = getDefaultValueWhenInvalidN(totalUseableReceiveQty);
                    childList[indexF].UseableReceiveQtyCone = getDefaultValueWhenInvalidN(totalUseableReceiveQtyCone);
                    childList[indexF].UseableReceiveQtyBag = getDefaultValueWhenInvalidN(totalUseableReceiveQtyCarton);

                    childList[indexF].YarnStockSetId = _useableStockSetId;

                    //var childObj = DeepClone(childList[indexF]);
                    //$tblChildEl.updateRow(indexF, childObj);
                    masterData.Childs = childList;
                    $tblChildEl.bootstrapTable('load', childList);
                }
            }
            if (!hasErrorRack) {
                $modalPlanningEl.modal('hide');
            }
        });
        $formEl.find("#btnOkUnuseable").click(function () {
            var hasErrorRack = false;
            if (_selectedKYLOReturnChildID > 0) {
                var rackList = DeepClone($tblStockInfoUnuseableEl.getCurrentViewRecords());
                var childList = masterData.Childs;//DeepClone($tblChildEl.getCurrentViewRecords());

                var indexF = childList.findIndex(x => x.KYLOReturnChildID == _selectedKYLOReturnChildID);
                if (indexF > -1) {
                    var totalUnuseableReceiveQty = 0;
                    var totalUnuseableReceiveQtyCone = 0;
                    var totalUnuseableReceiveQtyCarton = 0;

                    for (var iRack = 0; iRack < rackList.length; iRack++) {
                        var rack = rackList[iRack];

                        //-------------------------------- START Add data to _childRackBins ------------------------------
                        rack.ReceiveCartoon = getDefaultValueWhenInvalidN(rack.ReceiveCartoon);
                        rack.ReceiveQtyCone = getDefaultValueWhenInvalidN(rack.ReceiveQtyCone);
                        rack.ReceiveQtyKg = getDefaultValueWhenInvalidN(rack.ReceiveQtyKg);

                        var indexEX = _existingRackBinsReciveChildWiseUnuseable.findIndex(x => x.KYLOReturnChildID == _selectedKYLOReturnChildID);
                        if (indexEX > -1) {
                            var rackIndex = _existingRackBinsReciveChildWiseUnuseable[indexEX].ChildRackBinIDs.findIndex(x => x == rack.ChildRackBinID);
                            if (rackIndex == -1) {
                                rack.RackBinType = "New";
                            }
                        }

                        var indexF = _childRackBinsUnuseable.findIndex(x => x.KYLOReturnChildID == _selectedKYLOReturnChildID);
                        if (indexF == -1) {
                            _childRackBinsUnuseable.push({
                                KYLOReturnChildID: _selectedKYLOReturnChildID,
                                ChildRackBins: []
                            });
                            indexF = _childRackBinsUnuseable.findIndex(x => x.KYLOReturnChildID == _selectedKYLOReturnChildID);
                            if (indexF > -1) {
                                if (typeof rack.KYLORRCRBId === "undefined") rack.KYLORRCRBId = 0;
                                if (rack.KYLORRCRBId == 0) rack.KYLORRCRBId = _maxKYLORRCRBId++;
                                _childRackBinsUnuseable[indexF].ChildRackBins.push(rack);
                            }
                        } else {
                            var indexC = _childRackBinsUnuseable[indexF].ChildRackBins.findIndex(x => x.ChildRackBinID == rack.ChildRackBinID);
                            if (indexC == -1) {
                                if (typeof rack.KYLORRCRBId === "undefined") rack.KYLORRCRBId = 0;
                                if (rack.KYLORRCRBId == 0) rack.KYLORRCRBId = _maxKYLORRCRBId++;
                                _childRackBinsUnuseable[indexF].ChildRackBins.push(rack);
                            } else {
                                _childRackBinsUnuseable[indexF].ChildRackBins[indexC] = rack;
                            }
                        }
                        //-------------------------------- END Add data to _childRackBins ------------------------------
                        totalUnuseableReceiveQty += rack.ReceiveQtyKg;
                        totalUnuseableReceiveQtyCone += rack.ReceiveQtyCone;
                        totalUnuseableReceiveQtyCarton += rack.ReceiveCartoon;
                    }
                    childList[indexF].UnuseableReceiveQtyKG = getDefaultValueWhenInvalidN(totalUnuseableReceiveQty);
                    childList[indexF].UnuseableReceiveQtyCone = getDefaultValueWhenInvalidN(totalUnuseableReceiveQtyCone);
                    childList[indexF].UnuseableReceiveQtyBag = getDefaultValueWhenInvalidN(totalUnuseableReceiveQtyCarton);

                    childList[indexF].YarnStockSetId = _unuseableStockSetId;

                    masterData.Childs = childList;
                    $tblChildEl.bootstrapTable('load', childList);
                }
            }
            if (!hasErrorRack) {
                $modalPlanningUnuseableEl.modal('hide');
            }
        });
    });
    //function resetGlobal() {
    //    _useableStockSetId = 0;
    //    _unuseableStockSetId = 0;
    //}
    function DeepClone(obj) {
        return JSON.parse(JSON.stringify(obj));
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
                var currentRacks = $tblStockInfoEl.getCurrentViewRecords();

                res.rowData.RackBinType = "New";
                res.rowData.ReceiveCartoon = 0;
                res.rowData.ReceiveQtyCone = 0;
                res.rowData.ReceiveQtyKg = 0;
                currentRacks.push(res.rowData);
                initStockInfo(currentRacks);

                /*
                 *  { field: 'LocationName', headerText: 'Location', allowEditing: false, width: 80 },
                { field: 'RackNo', headerText: 'Rack', allowEditing: false, width: 80 },
                { field: 'NoOfCartoon', headerText: 'No of Cartoon', allowEditing: false, width: 80 },
                { field: 'NoOfCone', headerText: 'No of Cone', allowEditing: false, width: 80 },
                { field: 'ReceiveQty', headerText: 'Stock Qty', allowEditing: false, width: 80 },

                { field: 'ReceiveCartoon', headerText: 'Receive Cartoon', allowEditing: true, width: 80, editType: "numericedit", params: { showSpinButton: false, decimals: 0, format: "N2", min: 0, validateDecimalOnType: true } },
                { field: 'ReceiveQtyCone', headerText: 'Receive Cone', allowEditing: true, width: 80, editType: "numericedit", params: { showSpinButton: false, decimals: 0, format: "N2", min: 0, validateDecimalOnType: true } },
                { field: 'ReceiveQtyKg', headerText: 'Receive
                */
            }
        });
        finder.showModal();
    }
    function loadAllRacksUnuseable(e) {
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
                var currentRacks = $tblStockInfoUnuseableEl.getCurrentViewRecords();
                res.rowData.RackBinType = "New";
                res.rowData.ReceiveCartoon = 0;
                res.rowData.ReceiveQtyCone = 0;
                res.rowData.ReceiveQtyKg = 0;
                currentRacks.push(res.rowData);
                initStockInfoUnuseable(currentRacks);

            }
        });
        finder.showModal();
    }
    /*
    function initMasterTable() {
        $tblMasterEl.bootstrapTable('destroy');
        $tblMasterEl.bootstrapTable({
            showRefresh: true,
            showExport: true,
            showColumns: true,
            toolbar: toolbarId,
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
                    title: "Actions",
                    align: "center",
                    formatter: function (value, row, index, field) {
                        if (status === statusConstants.PENDING) {
                            return `<a class="btn btn-xs btn-default add" href="javascript:void(0)" title="New Return Receive">
                                        <i class="fa fa-plus" aria-hidden="true"></i>
                                    </a>`;
                        }
                        else {
                            return `<a class="btn btn-xs btn-default edit" href="javascript:void(0)" title="Edit Return Receive">
                                        <i class="fa fa-edit" aria-hidden="true"></i>
                                    </a>`;
                        }
                    },
                    events: {
                        'click .add': function (e, value, row, index) {
                            e.preventDefault();
                            getNew(row.KYLOReturnMasterID, row.ReturnFrom);
                        },
                        'click .edit': function (e, value, row, index) {
                            e.preventDefault();
                            getDetails(row.KYLOReturnReceiveMasterID, row.ReturnFrom);
                        }
                    }
                },
                {
                    field: "KYLOReturnNo",
                    title: "Return No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "KYLOReturnDate",
                    title: "Return Date",
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "Floor",
                    title: "Floor",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "ReturnFrom",
                    title: "Return From",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "KYLOReturnByUser",
                    title: "Return Rcv By",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },


                {
                    field: "KYLOReturnReceiveNo",
                    title: "Return Rcv No",
                    filterControl: "input",
                    visible: status == statusConstants.COMPLETED,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "KYLOReturnReceiveDate",
                    title: "Return Rcv Date",
                    visible: status == statusConstants.COMPLETED,
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "KYLOReturnReceiveBy",
                    title: "Return Rcv By",
                    filterControl: "input",
                    visible: status == statusConstants.COMPLETED,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
            ],
            onPageChange: function (number, size) {
                var newOffset = (number - 1) * size;
                var newLimit = size;
                if (tableParams.offset == newOffset && tableParams.limit == newLimit)
                    return;

                tableParams.offset = newOffset;
                tableParams.limit = newLimit;

                getMasterTableData();
            },
            onSort: function (name, order) {
                tableParams.sort = name;
                tableParams.order = order;
                tableParams.offset = 0;

                getMasterTableData();
            },
            onRefresh: function () {
                resetTableParams();
                getMasterTableData();
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

                getMasterTableData();
            }
        });
    }*/
    function initMasterTable() {
        var commands = [],
            isVisible = true,
            width = 200;
        if (status == statusConstants.APPROVED || status == statusConstants.PROPOSED) {
            commands = [
                { type: 'Edit', title: 'View this Receive', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } }
            ]
        }
        else {
            commands = [
                { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } }
            ]
        }
        var columns = [
            {
                headerText: 'Actions', width: width, textAlign: 'Center', commands: commands, visible: (status != statusConstants.PENDING)
            },
            {
                field: 'KSCIssueChildID', headerText: 'KSCIssueChildID', isPrimaryKey: true, visible: false
            },
            {
                field: 'YBChildItemID', headerText: 'YBChildItemID', visible: false
            },
            {
                field: 'KSCLOReturnNo', headerText: 'Return No', visible: (status != statusConstants.PENDING)
            },
            {
                field: 'KSCLOReturnDate', headerText: 'Return Date', type: 'date', format: _ch_date_format_1, visible: (status != statusConstants.PENDING)
            },
            {
                field: 'SubContractCompany', headerText: 'Sub Contract Factory', visible: (status == statusConstants.PENDING)
            },
            {
                field: 'YarnCategory', headerText: 'Yarn Category', visible: (status == statusConstants.PENDING)
            },
            {
                field: 'LotNo', headerText: 'Physical Lot', visible: (status == statusConstants.PENDING)
            },
            {
                field: 'PhysicalCount', headerText: 'Physical Count', visible: (status == statusConstants.PENDING)
            },
            {
                field: 'YarnCount', headerText: 'Yarn Count', visible: (status == statusConstants.PENDING)
            },
            {
                field: 'ExportOrderNo', headerText: 'EWO', visible: (status == statusConstants.PENDING)
            },
            {
                field: 'BookingNo', headerText: 'Booking No', visible: (status == statusConstants.PENDING)
            },
            {
                field: 'ChallanNo', headerText: 'Challan No', visible: (status == statusConstants.PENDING)
            },
            {
                field: 'IssueQty', headerText: 'IssueQty', width: 80, visible: (status == statusConstants.PENDING)
            },
            {
                field: 'BalanceQty', headerText: 'Balance Qty', width: 80, visible: (status == statusConstants.PENDING), textAlign: 'Center', headerTextAlign: 'Center'
            },
            {
                field: 'BatchNo', headerText: 'Batch No', visible: (status == statusConstants.PENDING)
            }

        ];
        if (status != statusConstants.PENDING) {
            columns = [
                {
                    headerText: 'Actions', width: width, textAlign: 'Center', commands: commands, visible: (status != statusConstants.PENDING)
                },
                {
                    field: 'KSCLOReturnReceiveMasterID', headerText: 'KSCLOReturnReceiveMasterID', isPrimaryKey: true, visible: false
                },
                {
                    field: 'KSCLOReturnReceiveNo', headerText: 'Return Receive No'
                },
                {
                    field: 'KSCLOReturnReceiveDate', headerText: 'Return Receive Date', type: 'date', format: _ch_date_format_1
                },
                {
                    field: 'ChallanNo', headerText: 'Challan No'
                },
                {
                    field: 'PLNo', headerText: 'PL No'
                },
                {
                    field: 'GPNo', headerText: 'GP No'
                },
                {
                    field: 'VehicleNo', headerText: 'Vehicle No'
                }

            ];
        }
        var selectionType = "Single";
        if (status == statusConstants.PENDING) {
            columns.unshift({ type: 'checkbox', width: 20 });
            selectionType = "Multiple";
        }

        if ($tblMasterEl) $tblMasterEl.destroy();

        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            allowGrouping: true,
            apiEndPoint: `/api/ksc-lo-return-receive/list?status=${status}`,
            columns: columns,
            //allowSorting: true,
            commandClick: handleCommands,
            allowSelection: status == statusConstants.PENDING,
            selectionSettings: { type: selectionType, checkboxOnly: true, persistSelection: true }
        });
    }
    function handleCommands(args) {
        if (args.commandColumn.type == 'Edit') {
            resetGlobals();
            getDetails(args.rowData.KSCLOReturnReceiveMasterID, 'SC');
            $toolbarEl.find("#btnNewCreateReturnReceive").fadeOut();
        }

    }
    function initChildTable() {
        $tblChildEl.bootstrapTable("destroy");
        $tblChildEl.bootstrapTable({
            showFooter: true,
            columns: [
                {
                    field: "KYLOReturnChildID",
                    title: "KYLOReturnChildID",
                    visible: false,
                    width: 100
                },
                {
                    field: "YarnCategory",
                    title: "Yarn Details",
                    width: 300
                },
                {
                    field: "PhysicalCount",
                    title: "Physical Count",
                    width: 300
                },
                {
                    field: "LotNo",
                    title: "Lot No",
                    width: 300
                },
                {
                    field: "SpinnerName",
                    title: "Spinner",
                    width: 300
                },
                /*
                {
                    field: "YarnType",
                    title: "Yarn Type",
                    width: 100
                },
                {
                    field: "YarnCount",
                    title: "Yarn Count",
                    width: 60
                },
                {
                    field: "YarnComposition",
                    title: "Yarn Composition",
                    width: 100
                },
                {
                    field: "Shade",
                    title: "Shade",
                    width: 60
                },
                {
                    field: "YarnColor",
                    title: "Yarn Color",
                    width: 80
                },

                {
                    field: "UOM",
                    title: "Unit",
                    width: 80
                },*/
                {
                    field: "IssueQtyKg",
                    title: "Issue Qty (Kg)",
                    width: 300
                },
                {
                    field: "IssueCartoon",
                    title: "Issue Qty (Cartoon)",
                    width: 300
                },
                {
                    field: "IssueQtyCone",
                    title: "Issue Qty (Cone)",
                    width: 300
                },
                {
                    field: "UseableReceiveQtyKG",
                    title: "Useable Receive Qty",
                    align: 'center',
                    formatter: function (value, row, index, field) {
                        // Add a class to the cell to enable the click event
                        return '<div class="receiveUsableQtyCell">' + value + '</div>';
                    }
                    //editable: {
                    //    type: "text",
                    //    showbuttons: false,
                    //    tpl: '<input type="number" class="form-control input-sm" min="0" style="padding-right: 24px;">',
                    //    validate: function (value) {
                    //        if (!value || !value.trim() || isNaN(parseInt(value)) || parseInt(value) <= 0) {
                    //            return 'Must be a positive integer.';
                    //        }
                    //    }
                    //}
                },
                {
                    field: "UseableReceiveQtyCone",
                    title: "Useable Receive Qty (Cone)",
                    align: 'center',
                    formatter: function (value, row, index, field) {
                        // Add a class to the cell to enable the click event
                        return '<div class="receiveUsableQtyCell">' + value + '</div>';
                    }
                    //editable: {
                    //    type: "text",
                    //    showbuttons: false,
                    //    tpl: '<input type="number" class="form-control input-sm" min="0" style="padding-right: 24px;">',
                    //    validate: function (value) {
                    //        if (!value || !value.trim() || isNaN(parseInt(value)) || parseInt(value) <= 0) {
                    //            return 'Must be a positive integer.';
                    //        }
                    //    }
                    //}
                },
                {
                    field: "UseableReceiveQtyBag",
                    title: "Useable Receive Qty (Crtn)",
                    align: 'center',
                    formatter: function (value, row, index, field) {
                        // Add a class to the cell to enable the click event
                        return '<div class="receiveUsableQtyCell">' + value + '</div>';
                    }
                    //editable: {
                    //    type: "text",
                    //    showbuttons: false,
                    //    tpl: '<input type="number" class="form-control input-sm" min="0" style="padding-right: 24px;">',
                    //    validate: function (value) {
                    //        if (!value || !value.trim() || isNaN(parseInt(value)) || parseInt(value) <= 0) {
                    //            return 'Must be a positive integer.';
                    //        }
                    //    }
                    //}
                },
                {
                    field: "UnuseableReceiveQtyKG",
                    title: "Unuseable Receive Qty",
                    align: 'center',
                    formatter: function (value, row, index, field) {
                        // Add a class to the cell to enable the click event
                        return '<div class="receiveUnuseableQtyCell">' + value + '</div>';
                    }
                    //editable: {
                    //    type: "text",
                    //    showbuttons: false,
                    //    tpl: '<input type="number" class="form-control input-sm" min="0" style="padding-right: 24px;">',
                    //    validate: function (value) {
                    //        if (!value || !value.trim() || isNaN(parseInt(value)) || parseInt(value) <= 0) {
                    //            return 'Must be a positive integer.';
                    //        }
                    //    }
                    //}
                },
                {
                    field: "UnuseableReceiveQtyCone",
                    title: "Unuseable Receive Qty (Cone)",
                    align: 'center',
                    formatter: function (value, row, index, field) {
                        // Add a class to the cell to enable the click event
                        return '<div class="receiveUnuseableQtyCell">' + value + '</div>';
                    }
                    //editable: {
                    //    type: "text",
                    //    showbuttons: false,
                    //    tpl: '<input type="number" class="form-control input-sm" min="0" style="padding-right: 24px;">',
                    //    validate: function (value) {
                    //        if (!value || !value.trim() || isNaN(parseInt(value)) || parseInt(value) <= 0) {
                    //            return 'Must be a positive integer.';
                    //        }
                    //    }
                    //}
                },
                {
                    field: "UnuseableReceiveQtyBag",
                    title: "Unuseable Receive Qty (Crtn)",
                    align: 'center',
                    formatter: function (value, row, index, field) {
                        // Add a class to the cell to enable the click event
                        return '<div class="receiveUnuseableQtyCell">' + value + '</div>';
                    }
                    //editable: {
                    //    type: "text",
                    //    showbuttons: false,
                    //    tpl: '<input type="number" class="form-control input-sm" min="0" style="padding-right: 24px;">',
                    //    validate: function (value) {
                    //        if (!value || !value.trim() || isNaN(parseInt(value)) || parseInt(value) <= 0) {
                    //            return 'Must be a positive integer.';
                    //        }
                    //    }
                    //}
                }
            ]
            /*columns: [
                {
                    field: "YarnProgramName",
                    title: "Yarn Program",
                    width: 100
                },
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
                    field: "YarnSubProgramNames",
                    title: "Yarn Sub Program",
                    width: 120
                },
                {
                    field: "Uom",
                    title: "Unit",
                    width: 80
                },
                {
                    field: "ReturnQty",
                    title: "Return Qty",
                    width: 80
                },
                {
                    field: "ReturnQtyCone",
                    title: "Return Qty (Cone)",
                    width: 80
                },
                {
                    field: "ReturnQtyCarton",
                    title: "Return Qty (Crtn)",
                    width: 80
                },
                {
                    field: "ReceiveQty",
                    title: "Receive Qty",
                    align: 'center',
                    editable: {
                        type: "text",
                        showbuttons: false,
                        tpl: '<input type="number" class="form-control input-sm" min="0" style="padding-right: 24px;">',
                        validate: function (value) {
                            if (!value || !value.trim() || isNaN(parseInt(value)) || parseInt(value) <= 0) {
                                return 'Must be a positive integer.';
                            }
                        }
                    }
                },
                {
                    field: "ReceiveQtyCone",
                    title: "Receive Qty (Cone)",
                    align: 'center',
                    editable: {
                        type: "text",
                        showbuttons: false,
                        tpl: '<input type="number" class="form-control input-sm" min="0" style="padding-right: 24px;">',
                        validate: function (value) {
                            if (!value || !value.trim() || isNaN(parseInt(value)) || parseInt(value) <= 0) {
                                return 'Must be a positive integer.';
                            }
                        }
                    }
                },
                {
                    field: "ReceiveQtyCarton",
                    title: "Receive Qty (Crtn)",
                    align: 'center',
                    editable: {
                        type: "text",
                        showbuttons: false,
                        tpl: '<input type="number" class="form-control input-sm" min="0" style="padding-right: 24px;">',
                        validate: function (value) {
                            if (!value || !value.trim() || isNaN(parseInt(value)) || parseInt(value) <= 0) {
                                return 'Must be a positive integer.';
                            }
                        }
                    }
                }
            ]*/
        });

        $tblChildEl.on('click', '.receiveUsableQtyCell', function (e) {
            var $cell = $(this);
            var index = $cell.closest('tr').index();
            var row = $tblChildEl.bootstrapTable('getData')[index];

            $formEl.find(".spnStockInfoBasicInfo").text("(" + row.YarnCategory + ")");


            selectedIndex = index;
            _selectedKYLOReturnChildID = row.KYLOReturnChildID;
            _selectedReceiveChildId = 0//args.rowData.ReceiveChildID;
            var childRackBinsExisting = row.ChildRackBins;
            var kReturnReceivedChildId = row.KYLOReturnReceiveChildID;
            var returnFrom = row.ReturnFrom;
            _returnFormType = row.ReturnFrom;

            var url = `/api/yarn-rack-bin-allocation/get-by-knitting-returnReceive/${_selectedReceiveChildId}/0/${kReturnReceivedChildId}/${returnFrom}`;
            var menuName = "KYLOReturnReceive";


            var lotNo = getDefaultValueForAPICall(replaceInvalidChar(row.LotNo));
            var physicalCount = getDefaultValueForAPICall(replaceInvalidChar(row.PhysicalCount));
            var shadeCode = getDefaultValueForAPICall(replaceInvalidChar(row.ShadeCode));

            var ItemMasterID = getDefaultValueWhenInvalidN(row.ItemMasterID);
            var SupplierId = getDefaultValueWhenInvalidN(row.SupplierId);
            var SpinnerId = getDefaultValueWhenInvalidN(row.SpinnerId);
            var YBChildItemID = getDefaultValueWhenInvalidN(row.YBChildItemID);

            var stockTypeId = 0//getDefaultValueWhenInvalidN(row.StockTypeId);
            var stockFromTableId = 0//getDefaultValueWhenInvalidN(row.StockFromTableId);
            var stockFromPKId = 0//getDefaultValueWhenInvalidN(row.StockFromPKId);

            var isFromDraft = status == statusConstants.COMPLETED || status == statusConstants.AWAITING_PROPOSE;
            var childRackBinID = row.ChildRackBins.length > 0 ? row.ChildRackBins[0].ChildRackBinID : 0;
            var issuedQtySt = replaceInvalidChar(getDefaultValueWhenInvalidN_Float(row.IssueQty).toString());

            if (status == statusConstants.PENDING) {
                if (masterData.ProgramName.toUpperCase() == "BULK") {
                    url = `/api/yarn-rnd-issues/GetAllocatedStockForIssue/${YBChildItemID}/${lotNo}/${physicalCount}/${ItemMasterID}/${SupplierId}/${SpinnerId}/${shadeCode}/${menuName}/${stockTypeId}/${stockFromTableId}/${stockFromPKId}/${isFromDraft}/${childRackBinID}/${issuedQtySt}`;
                } else if (masterData.ProgramName.toUpperCase() == "RND") {
                    var url = `/api/yarn-rnd-issues/GetStockForIssue/${lotNo}/${physicalCount}/${ItemMasterID}/${SpinnerId}/${menuName}/${stockTypeId}/${stockFromTableId}/${stockFromPKId}/${isFromDraft}/${childRackBinID}/${issuedQtySt}`;
                }
            }

            axios.get(url)
                .then(function (response) {

                    var list = response.data;
                    var crIdList = [];
                    var yarnStockSetIds = [];
                    var indexER = _existingRackBinsReciveChildWise.findIndex(x => x.KYLOReturnChildID == _selectedKYLOReturnChildID);
                    
                    list.map(x => {
                        x.ReceiveQtyKg = setDefaultValueZero(x.ReceiveQtyKg);
                        x.ReceiveQtyCone = setDefaultValueZero(x.ReceiveQtyCone);
                        x.ReceiveCartoon = setDefaultValueZero(x.ReceiveCartoon);
                        x.YarnStockSetId = getDefaultValueWhenInvalidN(x.YarnStockSetId);

                        var indexF = yarnStockSetIds.findIndex(i => i == x.YarnStockSetId);
                        if (indexF == -1) {
                            yarnStockSetIds.push(x.YarnStockSetId);
                        }
                        if (indexER == -1) crIdList.push(x.ChildRackBinID);
                    });

                    if (yarnStockSetIds.length > 1 && status == statusConstants.PENDING) {
                        toastr.error("Multiple Stock Set Id Found");
                        return false;
                    }

                    _useableStockSetId = 0;
                    if (list.length > 0) {
                        _useableStockSetId = list[0].YarnStockSetId;
                    }

                    if (indexER == -1) {
                        _existingRackBinsReciveChildWise.push({
                            KYLOReturnChildID: _selectedKYLOReturnChildID,
                            ChildRackBinIDs: crIdList
                        });
                    }
                    _childRackBins.map(y => {
                        if (y.KYLOReturnChildID == _selectedKYLOReturnChildID) {
                            //y.ChildRackBins.filter(x => x.RackBinType == "New").map(x => {
                            y.ChildRackBins.map(x => {
                                var rackIndex = list.findIndex(m => m.ChildRackBinID == x.ChildRackBinID);
                                if (rackIndex == -1) {
                                    x.ReceiveQtyKg = setDefaultValueZero(x.ReceiveQtyKg);
                                    x.ReceiveQtyCone = setDefaultValueZero(x.ReceiveQtyCone);
                                    x.ReceiveCartoon = setDefaultValueZero(x.ReceiveCartoon);
                                    list.push(x);
                                }

                            });
                        }
                    });
                    //ChildRackBins
                    var receiveCartoon = 0;
                    var receiveQtyCone = 0;
                    var receiveQtyKg = 0;

                    //Set pop up field values
                    var indexF = _childRackBins.findIndex(x => x.KYLOReturnChildID == _selectedKYLOReturnChildID);
                    if (indexF > -1) {
                        var childRackBins = _childRackBins[indexF].ChildRackBins;
                        list.map(x => {
                            var indexC = childRackBins.findIndex(y => y.ChildRackBinID == x.ChildRackBinID);
                            if (indexC > -1) {
                                var crbObj = childRackBins[indexC];

                                //------------Add Existing Qty-------------------------------
                                receiveCartoon = 0;
                                receiveQtyCone = 0;
                                receiveQtyKg = 0;
                                if (childRackBinsExisting == null) {
                                    childRackBinsExisting = [];
                                }
                                childRackBinsExisting.filter(e => e.ChildRackBinID == x.ChildRackBinID).map(c => {
                                    receiveCartoon += isNaN(c.ReceiveCartoon) ? 0 : c.ReceiveCartoon;
                                    receiveQtyCone += isNaN(c.ReceiveQtyCone) ? 0 : c.ReceiveQtyCone;
                                    receiveQtyKg += isNaN(c.ReceiveQtyKg) ? 0 : c.ReceiveQtyKg;
                                });

                                x.NoOfCartoon = x.NoOfCartoon - receiveCartoon;
                                x.NoOfCone = x.NoOfCone - receiveQtyCone;
                                x.ReceiveQty = x.ReceiveQty - receiveQtyKg;
                                //--------------------------------------------------------------


                                x.KYLORRCRBId = crbObj.KYLORRCRBId;
                                x.ReceiveCartoon = crbObj.ReceiveCartoon;
                                x.ReceiveQtyCone = crbObj.ReceiveQtyCone;
                                x.ReceiveQtyKg = crbObj.ReceiveQtyKg;
                            }
                        });
                    }
                    list.filter(x => x.KYLORRCRBId == 0).map(y => {
                        y.KYLORRCRBId = _maxKYLORRCRBId++;
                    });

                    ej.base.enableRipple(true);
                    initStockInfo(list);
                    $modalPlanningEl.modal('show');
                })
                .catch(function (err) {
                    toastr.error(err.response.data.Message);
                });


        });
        $tblChildEl.on('click', '.receiveUnuseableQtyCell', function (e) {
            var $cell = $(this);
            var index = $cell.closest('tr').index();
            var row = $tblChildEl.bootstrapTable('getData')[index];

            $formEl.find(".spnStockInfoBasicInfo").text("(" + row.YarnCategory + ")");


            selectedIndex = index;
            _selectedKYLOReturnChildID = row.KYLOReturnChildID;
            _selectedReceiveChildId = 0//args.rowData.ReceiveChildID;
            var childRackBinsExisting = row.ChildRackBinsUnuseable;
            var kReturnReceivedChildId = row.KYLOReturnReceiveChildID;
            var returnFrom = row.ReturnFrom;
            _returnFormType = row.ReturnFrom;
            var url = `/api/yarn-rack-bin-allocation/get-by-knitting-returnReceive-unuseable/${_selectedReceiveChildId}/0/${kReturnReceivedChildId}/${returnFrom}`;
            var menuName = "KYLOReturnReceive";

            var lotNo = getDefaultValueForAPICall(replaceInvalidChar(row.LotNo));
            var physicalCount = getDefaultValueForAPICall(replaceInvalidChar(row.PhysicalCount));
            var shadeCode = getDefaultValueForAPICall(replaceInvalidChar(row.ShadeCode));

            var ItemMasterID = getDefaultValueWhenInvalidN(row.ItemMasterID);
            var SupplierId = getDefaultValueWhenInvalidN(row.SupplierId);
            var SpinnerId = getDefaultValueWhenInvalidN(row.SpinnerId);
            var YBChildItemID = getDefaultValueWhenInvalidN(row.YBChildItemID);

            var stockTypeId = 0//getDefaultValueWhenInvalidN(row.StockTypeId);
            var stockFromTableId = 0//getDefaultValueWhenInvalidN(row.StockFromTableId);
            var stockFromPKId = 0//getDefaultValueWhenInvalidN(row.StockFromPKId);

            var isFromDraft = status == statusConstants.COMPLETED || status == statusConstants.AWAITING_PROPOSE;
            var childRackBinID = row.ChildRackBins.length > 0 ? row.ChildRackBins[0].ChildRackBinID : 0;
            var issuedQtySt = replaceInvalidChar(getDefaultValueWhenInvalidN_Float(row.IssueQty).toString());

            if (status == statusConstants.PENDING) {
                if (masterData.ProgramName.toUpperCase() == "BULK") {
                    url = `/api/yarn-rnd-issues/GetAllocatedStockForIssue/${YBChildItemID}/${lotNo}/${physicalCount}/${ItemMasterID}/${SupplierId}/${SpinnerId}/${shadeCode}/${menuName}/${stockTypeId}/${stockFromTableId}/${stockFromPKId}/${isFromDraft}/${childRackBinID}/${issuedQtySt}`;
                } else if (masterData.ProgramName.toUpperCase() == "RND") {
                    var url = `/api/yarn-rnd-issues/GetStockForIssue/${lotNo}/${physicalCount}/${ItemMasterID}/${SpinnerId}/${menuName}/${stockTypeId}/${stockFromTableId}/${stockFromPKId}/${isFromDraft}/${childRackBinID}/${issuedQtySt}`;
                }
            }

            axios.get(url)
                .then(function (response) {

                    var list = response.data;
                    var crIdList = [];
                    var yarnStockSetIds = [];
                    var indexER = _existingRackBinsReciveChildWiseUnuseable.findIndex(x => x.KYLOReturnChildID == _selectedKYLOReturnChildID);
                    
                    list.map(x => {
                        x.ReceiveQtyKg = setDefaultValueZero(x.ReceiveQtyKg);
                        x.ReceiveQtyCone = setDefaultValueZero(x.ReceiveQtyCone);
                        x.ReceiveCartoon = setDefaultValueZero(x.ReceiveCartoon);
                        x.YarnStockSetId = getDefaultValueWhenInvalidN(x.YarnStockSetId);
                        var indexF = yarnStockSetIds.findIndex(i => i == x.YarnStockSetId);
                        if (indexF == -1) {
                            yarnStockSetIds.push(x.YarnStockSetId);
                        }
                        if (indexER == -1) crIdList.push(x.ChildRackBinID);
                    });

                    if (yarnStockSetIds.length > 1) {
                        toastr.error("Multiple Stock Set Id Found");
                        return false;
                    }

                    _unuseableStockSetId = 0;
                    if (list.length > 0) {
                        _unuseableStockSetId = list[0].YarnStockSetId;
                    }

                    if (indexER == -1) {
                        _existingRackBinsReciveChildWiseUnuseable.push({
                            KYLOReturnChildID: _selectedKYLOReturnChildID,
                            ChildRackBinIDs: crIdList
                        });
                    }
                    _childRackBinsUnuseable.map(y => {
                        if (y.KYLOReturnChildID == _selectedKYLOReturnChildID) {
                            //y.ChildRackBins.filter(x => x.RackBinType == "New").map(x => {
                            y.ChildRackBins.map(x => {
                                var rackIndex = list.findIndex(m => m.ChildRackBinID == x.ChildRackBinID);
                                if (rackIndex == -1) {
                                    x.ReceiveQtyKg = setDefaultValueZero(x.ReceiveQtyKg);
                                    x.ReceiveQtyCone = setDefaultValueZero(x.ReceiveQtyCone);
                                    x.ReceiveCartoon = setDefaultValueZero(x.ReceiveCartoon);
                                    list.push(x);
                                }
                            });
                        }
                    });


                    //if (list.length == 0) {
                    //    toastr.error("Rack bin allocation not completed.");
                    //    return false;
                    //}

                    //ChildRackBins
                    var receiveCartoon = 0;
                    var receiveQtyCone = 0;
                    var receiveQtyKg = 0;

                    //Set pop up field values
                    var indexF = _childRackBinsUnuseable.findIndex(x => x.KYLOReturnChildID == _selectedKYLOReturnChildID);
                    if (indexF > -1) {
                        var childRackBins = _childRackBinsUnuseable[indexF].ChildRackBins;
                        list.map(x => {
                            var indexC = childRackBins.findIndex(y => y.ChildRackBinID == x.ChildRackBinID);
                            if (indexC > -1) {
                                var crbObj = childRackBins[indexC];

                                //------------Add Existing Qty-------------------------------
                                receiveCartoon = 0;
                                receiveQtyCone = 0;
                                receiveQtyKg = 0;
                                if (childRackBinsExisting == null) {
                                    childRackBinsExisting = [];
                                }
                                childRackBinsExisting.filter(e => e.ChildRackBinID == x.ChildRackBinID).map(c => {
                                    receiveCartoon += isNaN(c.ReceiveCartoon) ? 0 : c.ReceiveCartoon;
                                    receiveQtyCone += isNaN(c.ReceiveQtyCone) ? 0 : c.ReceiveQtyCone;
                                    receiveQtyKg += isNaN(c.ReceiveQtyKg) ? 0 : c.ReceiveQtyKg;
                                });

                                x.NoOfCartoon = x.NoOfCartoon - receiveCartoon;
                                x.NoOfCone = x.NoOfCone - receiveQtyCone;
                                x.ReceiveQty = x.ReceiveQty - receiveQtyKg;
                                //--------------------------------------------------------------


                                x.KYLORRCRBId = crbObj.KYLORRCRBId;
                                x.ReceiveCartoon = crbObj.ReceiveCartoon;
                                x.ReceiveQtyCone = crbObj.ReceiveQtyCone;
                                x.ReceiveQtyKg = crbObj.ReceiveQtyKg;
                            }
                        });
                    }
                    list.filter(x => x.KYLORRCRBId == 0).map(y => {
                        y.KYLORRCRBId = _maxKYLORRCRBId++;
                    });
                    ej.base.enableRipple(true);
                    initStockInfoUnuseable(list);
                    $modalPlanningUnuseableEl.modal('show');
                })
                .catch(function (err) {
                    toastr.error(err.response.data.Message);
                });


        });
    }
    function setDefaultValueZero(value) {
        if (typeof value === "undefined" || value == null) return 0;
        return value;
    }
    function initStockInfo(data) {
        if ($tblStockInfoEl) $tblStockInfoEl.destroy();

        var stockTitel = 'Allocated Stock Qty';
        if (_returnFormType == "RND") stockTitel = 'Sample Stock Qty';

        $tblStockInfoEl = new initEJ2Grid({
            tableId: tblStockInfoId,
            data: data,
            autofitColumns: false,
            allowSorting: true,
            allowPaging: false,
            allowFiltering: false,
            showDefaultToolbar: false,
            enableSingleClickEdit: true,
            //toolbar: ['Add'],
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
                //{
                //    field: 'LocationID', headerText: 'Location',
                //    allowEditing: true,
                //    required: true,
                //    width: 200,
                //    valueAccessor: ej2GridDisplayFormatter,
                //    dataSource: masterData.LocationList,
                //    displayField: "text", edit: ej2GridDropDownObj({
                //        width: 200
                //    })
                //},
                { field: 'LocationName', headerText: 'Location', allowEditing: false, width: 80 },
                { field: 'RackNo', headerText: 'Rack', allowEditing: false, width: 80 },
                { field: 'YarnControlNo', headerText: 'Control No', allowEditing: false, width: 80 },
                { field: 'AvgCartoonWeight', headerText: 'Avg. Cartoon Weight', allowEditing: false, width: 80 },
                { field: 'RackQty', headerText: 'Rack Qty', allowEditing: false, width: 80 },
                { field: 'LotNo', headerText: 'Lot No', allowEditing: false, width: 80 },
                { field: 'PhysicalCount', headerText: 'Physical Count', allowEditing: false, width: 80 },
                { field: 'NoOfCartoon', headerText: 'No of Cartoon', allowEditing: false, width: 80 },
                { field: 'NoOfCone', headerText: 'No of Cone', allowEditing: false, width: 80 },
                /*{ field: 'ReceiveQty', headerText: stockTitel, allowEditing: false, width: 80 },*/
                { field: 'ReceiveQtyKg', headerText: 'Receive Qty (Kg)', allowEditing: true, width: 80, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 2, min: 0 } } },
                { field: 'ReceiveCartoon', headerText: 'Receive Cartoon', allowEditing: true, width: 80, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 0 } } },
                { field: 'ReceiveQtyCone', headerText: 'Receive Cone', allowEditing: true, width: 80, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 0 } } },
            ],
            recordClick: function (args) {

            },
            actionBegin: function (args) {
                if (args.requestType === "add") {
                    //args.rowData.ChildRackBinID = _maxChildRackBinID++;
                    //args.rowData.LocationID = 0;
                    //args.rowData.NoOfCartoon = 0;
                    //args.rowData.NoOfCone = 0;
                    //args.rowData.RackNo = "";
                    //args.rowData.ReceiveCartoon = 0;
                    //args.rowData.ReceiveQtyCone = 0;
                    //args.rowData.ReceiveQtyKg = 0;
                    //args.rowData.ReceiveQty = 0;
                    //args.rowData.RackBinType = "New";
                }
                if (args.requestType === "save") {
                    /*if (args.data.ReceiveCartoon == null) {
                        args.data.ReceiveCartoon = 0;
                    }
                    if (args.data.ReceiveQtyCone == null) {
                        args.data.ReceiveQtyCone = 0;
                    }
                    if (args.data.ReceiveQtyKg == null) {
                        args.data.ReceiveQtyKg = 0;
                    }
                    var indexEX = _existingRackBinsReciveChildWise.findIndex(x => x.KYLOReturnChildID == _selectedKYLOReturnChildID);
                    if (indexEX > -1) {
                        var rackIndex = _existingRackBinsReciveChildWise[indexEX].ChildRackBinIDs.findIndex(x => x == args.data.ChildRackBinID);
                        if (rackIndex == -1) {
                            args.data.RackBinType = "New";
                        }
                    }

                    var indexF = _childRackBins.findIndex(x => x.KYLOReturnChildID == _selectedKYLOReturnChildID);
                    if (indexF == -1) {
                        _childRackBins.push({
                            KYLOReturnChildID: _selectedKYLOReturnChildID,
                            ChildRackBins: []
                        });
                        indexF = _childRackBins.findIndex(x => x.KYLOReturnChildID == _selectedKYLOReturnChildID);
                        if (indexF > -1) {
                            if (typeof args.data.KYLORRCRBId === "undefined") args.data.KYLORRCRBId = 0;
                            if (args.data.KYLORRCRBId == 0) args.data.KYLORRCRBId = _maxKYLORRCRBId++;
                            if (args.data.YarnStockSetId == 0) args.data.YarnStockSetId = _useableStockSetId;
                            _childRackBins[indexF].ChildRackBins.push(args.data);
                        }
                    } else {
                        var indexC = _childRackBins[indexF].ChildRackBins.findIndex(x => x.ChildRackBinID == args.data.ChildRackBinID);
                        if (indexC == -1) {
                            if (typeof args.data.KYLORRCRBId === "undefined") args.data.KYLORRCRBId = 0;
                            if (args.data.KYLORRCRBId == 0) args.data.KYLORRCRBId = _maxKYLORRCRBId++;
                            if (args.data.YarnStockSetId == 0) args.data.YarnStockSetId = _useableStockSetId;
                            _childRackBins[indexF].ChildRackBins.push(args.data);
                        } else {
                            if (args.data.YarnStockSetId == 0) args.data.YarnStockSetId = _useableStockSetId;
                            _childRackBins[indexF].ChildRackBins[indexC] = args.data;
                        }
                    }*/
                }
                if (args.requestType === "delete") {

                    var indexF = _childRackBins.findIndex(x => x.KYLOReturnChildID == _selectedKYLOReturnChildID);
                    if (indexF > -1) {
                        var indexD = _childRackBins[indexF].ChildRackBins.findIndex(x => x.ChildRackBinID == args.data[0].ChildRackBinID);
                        _childRackBins[indexF].ChildRackBins.splice(indexD, 1);
                    }
                }
            },
        });
    }
    function initStockInfoUnuseable(data) {
        if ($tblStockInfoUnuseableEl) $tblStockInfoUnuseableEl.destroy();

        var stockTitel = 'Allocated Stock Qty';
        if (_returnFormType == "RND") stockTitel = 'Sample Stock Qty';

        $tblStockInfoUnuseableEl = new initEJ2Grid({
            tableId: tblStockInfoUnuseableId,
            data: data,
            autofitColumns: false,
            allowSorting: true,
            allowPaging: false,
            allowFiltering: false,
            showDefaultToolbar: false,
            enableSingleClickEdit: true,
            //toolbar: ['Add'],
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
                //{
                //    field: 'LocationID', headerText: 'Location',
                //    allowEditing: true,
                //    required: true,
                //    width: 200,
                //    valueAccessor: ej2GridDisplayFormatter,
                //    dataSource: masterData.LocationList,
                //    displayField: "text", edit: ej2GridDropDownObj({
                //        width: 200
                //    })
                //},
                { field: 'LocationName', headerText: 'Location', allowEditing: false, width: 80 },
                { field: 'RackNo', headerText: 'Rack', allowEditing: false, width: 80 },
                { field: 'YarnControlNo', headerText: 'Control No', allowEditing: false, width: 80 },
                { field: 'AvgCartoonWeight', headerText: 'Avg. Cartoon Weight', allowEditing: false, width: 80 },
                { field: 'RackQty', headerText: 'Rack Qty', allowEditing: false, width: 80 },
                { field: 'LotNo', headerText: 'Lot No', allowEditing: false, width: 80 },
                { field: 'PhysicalCount', headerText: 'Physical Count', allowEditing: false, width: 80 },
                { field: 'NoOfCartoon', headerText: 'No of Cartoon', allowEditing: false, width: 80 },
                { field: 'NoOfCone', headerText: 'No of Cone', allowEditing: false, width: 80 },
                { field: 'ReceiveQty', headerText: stockTitel, allowEditing: false, width: 80 },
                { field: 'ReceiveQtyKg', headerText: 'Receive Qty (Kg)', allowEditing: true, width: 80, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 2, min: 0 } } },
                { field: 'ReceiveCartoon', headerText: 'Receive Cartoon', allowEditing: true, width: 80, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 0 } } },
                { field: 'ReceiveQtyCone', headerText: 'Receive Cone', allowEditing: true, width: 80, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 0 } } },
            ],
            recordClick: function (args) {

            },
            actionBegin: function (args) {
                if (args.requestType === "add") {
                    //args.rowData.ChildRackBinID = _maxChildRackBinID++;
                    //args.rowData.LocationID = 0;
                    //args.rowData.NoOfCartoon = 0;
                    //args.rowData.NoOfCone = 0;
                    //args.rowData.RackNo = "";
                    //args.rowData.ReceiveCartoon = 0;
                    //args.rowData.ReceiveQtyCone = 0;
                    //args.rowData.ReceiveQtyKg = 0;
                    //args.rowData.ReceiveQty = 0;
                    //args.rowData.RackBinType = "New";
                }
                if (args.requestType === "save") {

                    /*var indexEX = _existingRackBinsReciveChildWiseUnuseable.findIndex(x => x.KYLOReturnChildID == _selectedKYLOReturnChildID);
                    if (indexEX > -1) {
                        var rackIndex = _existingRackBinsReciveChildWiseUnuseable[indexEX].ChildRackBinIDs.findIndex(x => x == args.data.ChildRackBinID);
                        if (rackIndex == -1) {
                            args.data.RackBinType = "New";
                        }
                    }

                    var indexF = _childRackBinsUnuseable.findIndex(x => x.KYLOReturnChildID == _selectedKYLOReturnChildID);
                    if (indexF == -1) {
                        _childRackBinsUnuseable.push({
                            KYLOReturnChildID: _selectedKYLOReturnChildID,
                            ChildRackBins: []
                        });
                        indexF = _childRackBinsUnuseable.findIndex(x => x.KYLOReturnChildID == _selectedKYLOReturnChildID);
                        if (indexF > -1) {
                            if (typeof args.data.KYLORRCRBId === "undefined") args.data.KYLORRCRBId = 0;
                            if (args.data.KYLORRCRBId == 0) args.data.KYLORRCRBId = _maxKYLORRCRBId++;
                            _childRackBinsUnuseable[indexF].ChildRackBins.push(args.data);
                        }
                    } else {
                        var indexC = _childRackBinsUnuseable[indexF].ChildRackBins.findIndex(x => x.ChildRackBinID == args.data.ChildRackBinID);
                        if (indexC == -1) {
                            if (typeof args.data.KYLORRCRBId === "undefined") args.data.KYLORRCRBId = 0;
                            if (args.data.KYLORRCRBId == 0) args.data.KYLORRCRBId = _maxKYLORRCRBId++;
                            _childRackBinsUnuseable[indexF].ChildRackBins.push(args.data);
                        } else {
                            _childRackBinsUnuseable[indexF].ChildRackBins[indexC] = args.data;
                        }
                    }*/
                }
                if (args.requestType === "delete") {

                    var indexF = _childRackBinsUnuseable.findIndex(x => x.KYLOReturnChildID == _selectedKYLOReturnChildID);
                    if (indexF > -1) {
                        var indexD = _childRackBinsUnuseable[indexF].ChildRackBins.findIndex(x => x.ChildRackBinID == args.data[0].ChildRackBinID);
                        _childRackBinsUnuseable[indexF].ChildRackBins.splice(indexD, 1);
                    }
                }
            },
        });
    }
    /*
    function initStockInfo(data) {
        if ($tblStockInfoEl) $tblStockInfoEl.destroy();

        $tblStockInfoEl = new ej.grids.Grid({
            dataSource: data,
            allowResizing: true,
            allowPaging: false,
            columns: [
                { field: 'ChildRackBinID', isPrimaryKey: true, visible: false, width: 10 },
                { field: 'IssueChildID', visible: false, width: 10 },
                { field: 'LocationName', headerText: 'Location', allowEditing: false, width: 80 },
                { field: 'RackNo', headerText: 'Rack', allowEditing: false, width: 80 },
                { field: 'LotNo', headerText: 'Lot No', allowEditing: false, width: 80 },
                { field: 'PhysicalCount', headerText: 'Physical Count', allowEditing: false, width: 80 },
                { field: 'SpinnerName', headerText: 'Spinner', allowEditing: false, width: 80 },
                { field: 'NoOfCartoon', headerText: 'No of Cartoon', allowEditing: false, width: 80 },
                { field: 'NoOfCone', headerText: 'No of Cone', allowEditing: false, width: 80 },
                { field: 'ReceiveQty', headerText: 'Stock Qty', allowEditing: false, width: 80 },
                { field: 'IssueCartoon', headerText: 'Issue Cartoon', allowEditing: true, width: 80, edit: { params: { showSpinButton: false, decimals: 0, format: "N2" } } },
                { field: 'IssueQtyCone', headerText: 'Issue Cone', allowEditing: true, width: 80, edit: { params: { showSpinButton: false, decimals: 0, format: "N2" } } },
                { field: 'IssueQtyKg', headerText: 'Issue Qty (Kg)', allowEditing: true, width: 80, edit: { params: { showSpinButton: false, decimals: 0, format: "N2" } } }
            ],
            editSettings: { allowEditing: true, allowAdding: false, allowDeleting: false, mode: "Normal", showDeleteConfirmDialog: true },
            recordClick: function (args) {

            },
            actionBegin: function (args) {
                if (args.requestType === "save") {

                    var indexF = _childRackBins.findIndex(x => x.RnDReqChildID == _selectedRnDReqChildID);
                    if (indexF == -1) {
                        _childRackBins.push({
                            RnDReqChildID: _selectedRnDReqChildID,
                            ChildRackBins: []
                        });
                        indexF = _childRackBins.findIndex(x => x.RnDReqChildID == _selectedRnDReqChildID);
                        if (indexF > -1) {
                            if (args.data.YRICRBId == 0) args.data.YRICRBId = _maxYRICRBId++;
                            _childRackBins[indexF].ChildRackBins.push(args.data);
                        }
                    } else {
                        var indexC = _childRackBins[indexF].ChildRackBins.findIndex(x => x.ChildRackBinID == args.data.ChildRackBinID);
                        if (indexC == -1) {
                            if (args.data.YRICRBId == 0) args.data.YRICRBId = _maxYRICRBId++;
                            _childRackBins[indexF].ChildRackBins.push(args.data);
                        } else {
                            _childRackBins[indexF].ChildRackBins[indexC] = args.data;
                        }
                    }
                }
            },
        });
        $tblStockInfoEl.refreshColumns;
        $tblStockInfoEl.appendTo(tblStockInfoId);
    }*/
    function backToList() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        initMasterTable();
    }

    function resetForm() {
        $formEl.trigger("reset");
        $formEl.find("#KYLOReturnReceiveMasterID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function resetTableParams() {
        tableParams.offset = 0;
        tableParams.limit = 10;
        tableParams.filter = '';
        tableParams.sort = '';
        tableParams.order = '';
    }

    function getNewDataForKSCLOReturn() {
        resetGlobals();
        var selectedRecords = $tblMasterEl.getSelectedRecords();

        if (selectedRecords.length == 0) {
            toastr.error("Please select row(s)!");
            return;
        }
        //--
        var uniqueAry = distinctArrayByProperty($tblMasterEl.getSelectedRecords(), "SubContractCompany");
        if (uniqueAry.length != 1) {
            toastr.error("Selected row(s) Sub Contract Factory should be same!");
            return;
        }

        var KSCIssueChildIDs = selectedRecords.map(x => x.KSCIssueChildID).join(",")
        getNewForKSCLOReturn(KSCIssueChildIDs);


    }
    /* function getNewForKSCLOReturn(KYIssueChildID) {
 
         axios.get(`/api/KYLO-Return/new/${KYIssueChildID}`)
             .then(function (response) {
                 $divDetailsEl.fadeIn();
                 $divTblEl.fadeOut();
                 $formEl.find("#divChildInfo").show();
                 masterData = response.data;
                 masterData.KYLOReturnDate = formatDateToDefault(masterData.KYLOReturnDate);
                 masterData.KYIssueDate = formatDateToDefault(masterData.KYIssueDate);
 
                 if (masterData.Childs.length > 0) {
                     setFormData($formEl, masterData);
                     initTblKYLOReturntChild(masterData.Childs);
                 }
             })
             .catch(function (err) {
                 toastr.error(err.response.data.Message);
             });
     }*/
    function getNewForKSCLOReturn(KYIssueChildIDs) {
        KYIssueChildIDs = KYIssueChildIDs.replace(/,/g, '_');
        resetGlobals();
        var ReturnFrom = 'SC';
        axios.get(`/api/ksc-lo-return-receive/new/${KYIssueChildIDs}/${ReturnFrom}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                reset();
                masterData = response.data;

                const currentDate = new Date();
                masterData.KYLOReturnReceiveDate = formatDateToDefault(currentDate);
                masterData.ChallanDate = formatDateToDefault(currentDate);
                masterData.PLDate = formatDateToDefault(currentDate);
                masterData.GPDate = formatDateToDefault(currentDate);

                setFormData($formEl, masterData);
                $tblChildEl.bootstrapTable("load", masterData.Childs);
                $tblChildEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getDetails(id, ReturnFrom) {
        resetGlobals();
        axios.get(`/api/ksc-lo-return-receive/${id}/${ReturnFrom}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                reset();
                masterData = response.data;

                masterData.KYLOReturnReceiveDate = formatDateToDefault(masterData.KYLOReturnReceiveDate);
                masterData.ChallanDate = formatDateToDefault(masterData.ChallanDate);
                masterData.PLDate = formatDateToDefault(masterData.PLDate);
                masterData.GPDate = formatDateToDefault(masterData.GPDate);

                setFormData($formEl, masterData);

                masterData.Childs.map(x => {
                    _childRackBins.push({
                        KYLOReturnChildID: x.KYLOReturnChildID,
                        ChildRackBins: x.ChildRackBins
                    });
                    _childRackBinsUnuseable.push({
                        KYLOReturnChildID: x.KYLOReturnChildID,
                        ChildRackBins: x.ChildRackBinsUnuseable
                    });
                });

                $tblChildEl.bootstrapTable("load", masterData.Childs);
                $tblChildEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function save(invokedBy, isApprove = false) {

        var data = formDataToJson($formEl.serializeArray());
        data.Childs = masterData.Childs;
        data.Approve = isApprove;
        
        if (data.ChallanNo == '' || typeof data.ChallanNo == "undefined") {
            toastr.error('Must have Challan No.');
            return false;
        }
        if (data.PLNo == '' || typeof data.PLNo == "undefined") {
            toastr.error('Must have PL No.');
            return false;
        }
        if (data.GPNo == '' || typeof data.GPNo == "undefined") {
            toastr.error('Must have GP No.');
            return false;
        }
        if (data.TransportTypeID == '' || typeof data.TransportTypeID == "undefined") {
            toastr.error('Must have Transport Type.');
            return false;
        }
        if (data.TransportAgencyID == '' || typeof data.TransportAgencyID == "undefined") {
            toastr.error('Must have Transport Agency.');
            return false;
        }
        if (data.VehicleNo == '' || typeof data.VehicleNo == "undefined") {
            toastr.error('Must have Vehicle No.');
            return false;
        }
        if (data.DriverName == '' || typeof data.DriverName == "undefined") {
            toastr.error('Must have Driver Name.');
            return false;
        }
        if (data.ContactNo == '' || typeof data.ContactNo == "undefined") {
            toastr.error('Must have Contact No.');
            return false;
        }

        var tempChilds = data.Childs.filter(x => x.UseableReceiveQtyBag == 0 && x.UseableReceiveQtyCone == 0 && x.UseableReceiveQtyKG == 0 && x.UnuseableReceiveQtyBag == 0 && x.UnuseableReceiveQtyCone == 0 && x.UnuseableReceiveQtyKG == 0);
        if (tempChilds.length > 0) {
            toastr.error('Must have cartor or cone or receive qty.');
            return false;
        }

        var hasError = false;
        for (var index = 0; index < data.Childs.length; index++) {
            var child = data.Childs[index];
            /*
            if (child.UseableReturnQtyKG < child.UseableReceiveQtyKG) {
                toastr.error("Receive qty cannot be greater than return qty.");
                hasError = true;
                break;
            }
            if (child.UseableReturnQtyCone < child.UseableReceiveQtyCone) {
                toastr.error("Receive cone cannot be greater than return no of cone.");
                hasError = true;
                break;
            }
            if (child.UseableReturnQtyBag < child.UseableReceiveQtyBag) {
                toastr.error("Receive carton cannot be greater than return no of carton.");
                hasError = true;
                break;
            }

            if (child.UnuseableReturnQtyKG < child.UnuseableReceiveQtyKG) {
                toastr.error("Receive qty cannot be greater than return qty.");
                hasError = true;
                break;
            }
            if (child.UnuseableReturnQtyCone < child.UnuseableReceiveQtyCone) {
                toastr.error("Receive cone cannot be greater than return no of cone.");
                hasError = true;
                break;
            }
            if (child.UnuseableReturnQtyBag < child.UnuseableReceiveQtyBag) {
                toastr.error("Receive carton cannot be greater than return no of carton.");
                hasError = true;
                break;
            }
            */
            var indexF = _childRackBins.findIndex(x => x.KYLOReturnChildID == data.Childs[index].KYLOReturnChildID);
            if (indexF > -1) {
                _childRackBins[indexF].ChildRackBins.filter(x => x.KYLORRCRBId == 0).map(x => x.KYLORRCRBId = _maxKYLORRCRBId++);
                data.Childs[index].ChildRackBins = _childRackBins[indexF].ChildRackBins.filter(x => x.ReceiveCartoon > 0 || x.ReceiveQtyCone > 0 || x.ReceiveQtyKg > 0);

                totalReceiveQty = 0;
                totalReceiveQtyCone = 0;
                totalReceiveQtyCarton = 0;

                _childRackBins[indexF].ChildRackBins.map(x => {
                    totalReceiveQty += x.ReceiveQtyKg;
                    totalReceiveQtyCone += x.ReceiveQtyCone;
                    totalReceiveQtyCarton += x.ReceiveCartoon;
                });
                data.Childs[index].UseableReceiveQtyKG = totalReceiveQty;
                data.Childs[index].UseableReceiveQtyCone = totalReceiveQtyCone;
                data.Childs[index].UseableReceiveQtyBag = totalReceiveQtyCarton;
            }
            var indexFU = _childRackBinsUnuseable.findIndex(x => x.KYLOReturnChildID == data.Childs[index].KYLOReturnChildID);
            if (indexFU > -1) {
                _childRackBinsUnuseable[indexFU].ChildRackBins.filter(x => x.KYLORRCRBId == 0).map(x => x.KYLORRCRBId = _maxKYLORRCRBId++);
                data.Childs[index].ChildRackBinsUnuseable = _childRackBinsUnuseable[indexFU].ChildRackBins.filter(x => x.ReceiveCartoon > 0 || x.ReceiveQtyCone > 0 || x.ReceiveQtyKg > 0);

                totalReceiveQty = 0;
                totalReceiveQtyCone = 0;
                totalReceiveQtyCarton = 0;

                _childRackBinsUnuseable[indexFU].ChildRackBins.map(x => {
                    totalReceiveQty += x.ReceiveQtyKg;
                    totalReceiveQtyCone += x.ReceiveQtyCone;
                    totalReceiveQtyCarton += x.ReceiveCartoon;
                });
                data.Childs[index].UnuseableReceiveQtyKG = totalReceiveQty;
                data.Childs[index].UnuseableReceiveQtyCone = totalReceiveQtyCone;
                data.Childs[index].UnuseableReceiveQtyBag = totalReceiveQtyCarton;
            }
        }

        if (hasError) return false;
        if (masterData.KYLOReturnReceiveMasterID > 0) data.IsModified = true;

        axios.post("/api/ksc-lo-return-receive/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }
    function reset() {
        _childRackBins = [];
        _childRackBinsUnuseable = [];
        _selectedKYLOReturnChildID = 0;
        _existingRackBinsReciveChildWise = [];
        _existingRackBinsReciveChildWiseUnuseable = [];
    }
    function CreateToolBarHideShow() {
        $toolbarEl.find("#btnNewCreateReturnReceive").fadeOut();
        if (status == statusConstants.PENDING)
            $toolbarEl.find("#btnNewCreateReturnReceive").fadeIn();

    }
    function resetGlobals() {
        _childRackBins = [];
        _selectedKYLOReturnChildID = 0;
        _maxKYLORRCRBId = 999;
        _existingRackBinsReciveChildWise = [];
        _childRackBinsUnuseable = [];
        _existingRackBinsReciveChildWiseUnuseable = [];
        _useableStockSetId = 0;
        _unuseableStockSetId = 0;
        _returnFormType = "";
    }
})();