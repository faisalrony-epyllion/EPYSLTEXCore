(function () {
    var menuId, pageName;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, tblChildId, $tblChildEl, $formEl,
        $modalRackItemEl, $tblRackLocationItemEl, tblRackLocationItem;
    var filterBy = {};
    var tableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    }
    var pageId = "";
    var masterData;
    var status = statusConstants.PENDING;
    var _childRackBins = [];
    //_childRackBins.push({
    //    ReceiveChildID: _selectedReceiveChildId,
    //    ChildRackBins: []
    //});
    var _selectedReceiveChildId = 0;
    var _maxYQCRRId = 999;
    var _maxChildRackBinID = 999;
    var _existingRackBinsReciveChildWise = [];

    $(function () {
        if (!menuId)
            menuId = localStorage.getItem("menuId");
        if (!pageName)
            pageName = localStorage.getItem("pageName");

        pageId = pageName + "-" + menuId;
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        $tblMasterEl = $(pageConstants.MASTER_TBL_ID_PREFIX + pageId);
        tblChildId = pageConstants.CHILD_TBL_ID_PREFIX + pageId;
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);
        $modalRackItemEl = $("#modalRackItem" + pageId);
        tblRackLocationItem = "#tblRackLocationItem" + pageId;

        initMasterTable();
        getMasterTableData();

        $toolbarEl.find("#btnPending").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PENDING;

            initMasterTable();
            getMasterTableData();
        });

        $toolbarEl.find("#btnList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.COMPLETED;

            initMasterTable();
            getMasterTableData();
        });

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save();
        });

        $formEl.find("#btnOk").click(function () {
            var hasErrorRack = false;
            if (_selectedReceiveChildId > 0) {
                var rackList = DeepClone($tblRackLocationItemEl.getCurrentViewRecords());
                var childList = DeepClone($tblChildEl.getCurrentViewRecords());

                var indexF = childList.findIndex(x => x.ReceiveChildID == _selectedReceiveChildId);
                if (indexF > -1) {
                    var totalReceiveQty = 0;
                    var totalReceiveQtyCone = 0;
                    var totalReceiveQtyCarton = 0;

                    for (var iRack = 0; iRack < rackList.length; iRack++) {
                        var rack = rackList[iRack];

                        totalReceiveQty += rack.ReceiveQtyKg;
                        totalReceiveQtyCone += rack.ReceiveQtyCone;
                        totalReceiveQtyCarton += rack.ReceiveCartoon;
                    }

                    childList[indexF].ReceiveQty = totalReceiveQty;
                    childList[indexF].ReceiveQtyCone = totalReceiveQtyCone;
                    childList[indexF].ReceiveQtyCarton = totalReceiveQtyCarton;

                    var childObj = DeepClone(childList[indexF]);
                    $tblChildEl.updateRow(indexF, childObj);
                }
            }
            if (!hasErrorRack) {
                $modalRackItemEl.modal('hide');
            }
        });

        $formEl.find("#btnCancel").on("click", backToList);
        $formEl.find("#btnAddNewRack").on("click", loadAllRacks);

        $toolbarEl.find("#btnPending").click();
    });

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
                    field: "",
                    align: "center",
                    formatter: function (value, row, index, field) {
                        var template;
                        if (status === statusConstants.PENDING) {
                            template =
                                `<a class="btn btn-xs btn-default add" href="javascript:void(0)" title="New Yarn QC Retrurn Receive">
                                    <i class="fa fa-plus" aria-hidden="true"></i>
                                </a>
                                <a class="btn btn-xs btn-default viewreport" href="javascript:void(0)" title="View Report">
                                    <i class="fas fa-file-pdf" aria-hidden="true"></i>
                                </a>
                                `;
                        }
                        else {
                            template =
                                `<a class="btn btn-xs btn-default edit" href="javascript:void(0)" title="Edit Yarn QC Retrurn">
                                    <i class="fa fa-edit" aria-hidden="true"></i>
                                </a>
                                <a class="btn btn-xs btn-default viewreport" href="javascript:void(0)" title="View Report">
                                    <i class="fas fa-file-pdf" aria-hidden="true"></i>
                                </a>`;
                        }

                        return template;
                    },
                    events: {
                        'click .add': function (e, value, row, index) {
                            e.preventDefault();
                            //console.log(row);
                            getNew(row.QCReturnMasterID);
                        },
                        'click .edit': function (e, value, row, index) {
                            e.preventDefault();
                            getDetails(row.QCReturnReceivedMasterID);
                        },
                        'click .viewreport': function (e, value, row, index) {
                            e.preventDefault();

                            if ($.trim(row.QCReturnNo).length == 0) {
                                toastr.error("QC Return No Found.");
                                return false;
                            }
                            window.open(`/reports/InlinePdfView?ReportName=MaterialReturnNote.rdl&ReturnNo=${row.QCReturnNo}`, '_blank');
                        },
                    }
                },
                {
                    field: "QCReturnNo",
                    title: "Return No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    visible: status == statusConstants.PENDING
                },
                {
                    field: "QCReturnDate",
                    title: "Return Date",
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    },
                    visible: status == statusConstants.PENDING
                },
                {
                    field: "QCReturnReceivedDate",
                    title: "Receive Date",
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    },
                    visible: status !== statusConstants.PENDING
                },
                {
                    field: "ReceiveNo",
                    title: "Receive No",
                    visible: status !== statusConstants.PENDING
                },
                {
                    field: "QCReqNo",
                    title: "QC Req No",
                    //visible: status !== statusConstants.PENDING
                },
                {
                    field: "QCIssueNo",
                    title: "QC Issue No",
                    visible: status !== statusConstants.PENDING
                },
                {
                    field: "QCReceiveNo",
                    title: "QC Receive No",
                    visible: status !== statusConstants.PENDING
                },
                {
                    field: "ReceiveQty",
                    title: "Rcv Qty(KG)",
                    visible: status !== statusConstants.PENDING
                },
                {
                    field: "ReceiveQtyCarton",
                    title: "Rcv Qty(Crtn)",
                    visible: status !== statusConstants.PENDING
                },
                {
                    field: "ReceiveQtyCone",
                    title: "Rcv Qty(Cone)",
                    visible: status !== statusConstants.PENDING
                },
                {
                    field: "ReturnQty",
                    title: "Rtn Qty(KG)",
                    visible: status !== statusConstants.PENDING
                },
                {
                    field: "ReturnQtyCarton",
                    title: "Rtn Qty(Crtn)",
                    visible: status !== statusConstants.PENDING
                },
                {
                    field: "ReturnQtyCone",
                    title: "Rtn Qty(Cone)",
                    visible: status !== statusConstants.PENDING
                },
                {
                    field: "QCReturnByUser",
                    title: "Return By",
                    visible: status == statusConstants.PENDING
                },
                {
                    field: "ReturnReceivedByUser",
                    title: "Received By",
                    visible: status !== statusConstants.PENDING
                },

                //{
                //    field: "IsAcknowledge",
                //    title: "Acknowledged",
                //    formatter: function (value, row, index, field) {
                //        return value ? "Yes" : "No";
                //    }
                //}
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
    }

    function getMasterTableData() {
        var queryParams = $.param(tableParams);
        $tblMasterEl.bootstrapTable('showLoading');
        var url = `/api/yarn-qc-returnreceive/list?gridType=bootstrap-table&status=${status}&${queryParams}`;
        axios.get(url)
            .then(function (response) {
                $tblMasterEl.bootstrapTable('load', response.data);
                $tblMasterEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            })
    }

    function setDefaultValueZero(value) {
        if (typeof value === "undefined" || value == null) return 0;
        return value;
    }

    function initChildTable(records) {
        if ($tblChildEl) $tblChildEl.destroy();
        $tblChildEl = new ej.grids.Grid({
            //tableId: tblChildId,
            dataSource: records,
            allowResizing: true,
            autofitColumns: true,
            allowSorting: true,
            allowPaging: false,
            allowFiltering: false,
            editSettings: { allowAdding: false, allowEditing: true, allowDeleting: false, mode: "Normal", showDeleteConfirmDialog: true },
            columns: [
                {
                    headerText: 'Command', width: 100, commands: [
                        { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                        //{ type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                        { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                        { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }
                    ]
                },
                { field: 'QCReturnReceivedChildID', isPrimaryKey: true, visible: false },
                { field: 'QCReturnReceivedMasterID', visible: false },
                { field: 'QCReturnChildID', visible: false },
                { field: 'ReceiveChildID', visible: false },

                { field: 'YarnCategory', headerText: 'Yarn Details', allowEditing: false },

                { field: 'ChallanCount', headerText: 'Challan Count', allowEditing: false },
                { field: 'PhysicalCount', headerText: 'Physical Count', allowEditing: false },
                { field: 'ChallanLot', headerText: 'Challan Lot', allowEditing: false },
                { field: 'LotNo', headerText: 'Physical Lot', allowEditing: false, textAlign: 'Center' },

                //{ field: 'YarnType', headerText: 'Yarn Type', allowEditing: false },
                //{ field: 'YarnComposition', headerText: 'Composition', allowEditing: false },
                //{ field: 'YarnCount', headerText: 'Count', allowEditing: false },
                //{ field: 'YarnColor', headerText: 'Color', allowEditing: false },
                //{ field: 'YarnShade', headerText: 'Shade', allowEditing: false },

                { field: 'Supplier', headerText: 'Supplier', allowEditing: false },
                { field: 'Spinner', headerText: 'Spinner', allowEditing: false },

                { field: 'DisplayUnitDesc', headerText: 'Unit', allowEditing: false },

                //{ field: 'Uom', headerText: 'Uom', allowEditing: false, textAlign: 'Center' },

                { field: 'ReturnQtyCarton', headerText: 'Return Qty(Crtn)', allowEditing: false, textAlign: 'Center' },
                { field: 'ReturnQtyCone', headerText: 'Return Qty(Cone)', allowEditing: false, textAlign: 'Center' },
                { field: 'ReturnQty', headerText: 'Return Qty(KG)', allowEditing: false, textAlign: 'Center' },

                { field: 'ReceiveQtyCarton', headerText: 'Rcv Qty(Crtn)', allowEditing: false, textAlign: 'center', width: 120, valueAccessor: diplayNumberButton },
                { field: 'ReceiveQtyCone', headerText: 'Rcv Qty(Cone)', allowEditing: false, textAlign: 'center', width: 120, valueAccessor: diplayNumberButton },
                { field: 'ReceiveQty', headerText: 'Rcv Qty(KG)', allowEditing: false, textAlign: 'center', width: 120, valueAccessor: diplayNumberButton },

                { field: 'Remarks', headerText: 'Remarks', allowEditing: true }
            ],
            recordClick: function (args) {
                if (args.column && (args.column.field == "ReceiveQty" || args.column.field == "ReceiveQtyCone" || args.column.field == "ReceiveQtyCarton")) {
                    _selectedReceiveChildId = args.rowData.ReceiveChildID;
                    var childRackBinsExisting = args.rowData.ChildRackBins;
                    var qcReturnReceivedChildId = args.rowData.QCReturnReceivedChildID;

                    axios.get(`/api/yarn-rack-bin-allocation/get-by-receive-child/${_selectedReceiveChildId}/${args.rowData.LocationID}/${qcReturnReceivedChildId}`)
                        .then(function (response) {
                            var list = response.data;
                            var crIdList = [];
                            var indexER = _existingRackBinsReciveChildWise.findIndex(x => x.ReceiveChildID == _selectedReceiveChildId);

                            list.map(x => {
                                x.ReceiveQtyKg = setDefaultValueZero(x.ReceiveQtyKg);
                                x.ReceiveQtyCone = setDefaultValueZero(x.ReceiveQtyCone);
                                x.ReceiveCartoon = setDefaultValueZero(x.ReceiveCartoon);

                                if (indexER == -1) crIdList.push(x.ChildRackBinID);
                            });

                            if (indexER == -1) {
                                _existingRackBinsReciveChildWise.push({
                                    ReceiveChildID: _selectedReceiveChildId,
                                    ChildRackBinIDs: crIdList
                                });
                            }
                            _childRackBins.map(y => {
                                y.ChildRackBins.filter(x => x.RackBinType == "New").map(x => {
                                    x.ReceiveQtyKg = setDefaultValueZero(x.ReceiveQtyKg);
                                    x.ReceiveQtyCone = setDefaultValueZero(x.ReceiveQtyCone);
                                    x.ReceiveCartoon = setDefaultValueZero(x.ReceiveCartoon);
                                    list.push(x);
                                });
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
                            var indexF = _childRackBins.findIndex(x => x.ReceiveChildID == _selectedReceiveChildId);
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

                                        childRackBinsExisting.filter(e => e.ChildRackBinID == x.ChildRackBinID).map(c => {
                                            receiveCartoon += isNaN(c.ReceiveCartoon) ? 0 : c.ReceiveCartoon;
                                            receiveQtyCone += isNaN(c.ReceiveQtyCone) ? 0 : c.ReceiveQtyCone;
                                            receiveQtyKg += isNaN(c.ReceiveQtyKg) ? 0 : c.ReceiveQtyKg;
                                        });

                                        x.NoOfCartoon = x.NoOfCartoon - receiveCartoon;
                                        x.NoOfCone = x.NoOfCone - receiveQtyCone;
                                        x.ReceiveQty = x.ReceiveQty - receiveQtyKg;
                                        //--------------------------------------------------------------


                                        x.YQCRRId = crbObj.YQCRRId;
                                        x.ReceiveCartoon = crbObj.ReceiveCartoon;
                                        x.ReceiveQtyCone = crbObj.ReceiveQtyCone;
                                        x.ReceiveQtyKg = crbObj.ReceiveQtyKg;
                                    }
                                });
                            }
                            list.filter(x => x.YQCRRId == 0).map(y => {
                                y.YQCRRId = _maxYQCRRId++;
                            });
                            ej.base.enableRipple(true);
                            initRackLocation(list);
                            $modalRackItemEl.modal('show');
                        })
                        .catch(function (err) {
                            toastr.error(err.response.data.Message);
                        });
                }
            },
        });
        $tblChildEl.refreshColumns;
        $tblChildEl.appendTo(tblChildId);
    }
    function initRackLocation(list) {
        if ($tblRackLocationItemEl) $tblRackLocationItemEl.destroy();

        $tblRackLocationItemEl = new initEJ2Grid({
            tableId: tblRackLocationItem,
            data: list,
            autofitColumns: false,
            allowSorting: true,
            allowPaging: false,
            allowFiltering: false,
            showDefaultToolbar: false,
            enableSingleClickEdit: true,
            //toolbar: ['Add'],
            editSettings: { allowAdding: true, allowEditing: true, allowDeleting: false, mode: "Normal", showDeleteConfirmDialog: true },
            columns: [
                {
                    headerText: 'Command', width: 80, commands: [
                        { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                        //{ type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
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
                { field: 'NoOfCartoon', headerText: 'No of Cartoon', allowEditing: false, width: 80 },
                { field: 'NoOfCone', headerText: 'No of Cone', allowEditing: false, width: 80 },
                { field: 'ReceiveQty', headerText: 'Stock Qty', allowEditing: false, width: 80 },

                { field: 'ReceiveCartoon', headerText: 'Receive Cartoon', allowEditing: true, width: 100, editType: "numericedit", params: { showSpinButton: false, decimals: 0, format: "N2", min: 0, validateDecimalOnType: true } },
                { field: 'ReceiveQtyCone', headerText: 'Receive Cone', allowEditing: true, width: 100, editType: "numericedit", params: { showSpinButton: false, decimals: 0, format: "N2", min: 0, validateDecimalOnType: true } },
                { field: 'ReceiveQtyKg', headerText: 'Receive Qty (Kg)', allowEditing: true, width: 100, editType: "numericedit", params: { showSpinButton: false, decimals: 2, format: "N2", min: 0, validateDecimalOnType: true } }
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
                    var indexEX = _existingRackBinsReciveChildWise.findIndex(x => x.ReceiveChildID == _selectedReceiveChildId);
                    if (indexEX > -1) {
                        var rackIndex = _existingRackBinsReciveChildWise[indexEX].ChildRackBinIDs.findIndex(x => x == args.data.ChildRackBinID);
                        if (rackIndex == -1) {
                            args.data.RackBinType = "New";
                        }
                    }

                    var indexF = _childRackBins.findIndex(x => x.ReceiveChildID == _selectedReceiveChildId);
                    if (indexF == -1) {
                        _childRackBins.push({
                            ReceiveChildID: _selectedReceiveChildId,
                            ChildRackBins: []
                        });
                        indexF = _childRackBins.findIndex(x => x.ReceiveChildID == _selectedReceiveChildId);
                        if (indexF > -1) {
                            if (typeof args.data.YQCRRId === "undefined") args.data.YQCRRId = 0;
                            if (args.data.YQCRRId == 0) args.data.YQCRRId = _maxYQCRRId++;
                            _childRackBins[indexF].ChildRackBins.push(args.data);
                        }
                    } else {
                        var indexC = _childRackBins[indexF].ChildRackBins.findIndex(x => x.ChildRackBinID == args.data.ChildRackBinID);
                        if (indexC == -1) {
                            if (typeof args.data.YQCRRId === "undefined") args.data.YQCRRId = 0;
                            if (args.data.YQCRRId == 0) args.data.YQCRRId = _maxYQCRRId++;
                            _childRackBins[indexF].ChildRackBins.push(args.data);
                        } else {
                            _childRackBins[indexF].ChildRackBins[indexC] = args.data;
                        }
                    }
                }
            },
        });
    }

    function diplayNumberButton(field, data, column) {
        column.disableHtmlEncode = false;
        return `<a class="btn btn-xs btn-default" href="javascript:void(0)" title="${column.headerText}">
                                     ${data[field] ? data[field] : 0}
                                </a>`;
    }

    function backToList() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        getMasterTableData();
    }

    function resetForm() {
        $formEl.trigger("reset");
        $.each($formEl.find('select'), function (i, el) {
            $(el).select2('');
        });
        $formEl.find("#Id").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function resetTableParams() {
        tableParams.offset = 0;
        tableParams.limit = 10;
        tableParams.filter = '';
        tableParams.sort = '';
        tableParams.order = '';
    }

    function getNew(reqMasterId) {
        axios.get(`/api/yarn-qc-returnreceive/new/${reqMasterId}`)
            .then(function (response) {
                //console.log(response);
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                reset();

                masterData = response.data;

                masterData.QCReturnReceivedDate = formatDateToDefault(masterData.QCReturnReceivedDate);
                masterData.QCReturnDate = formatDateToDefault(masterData.QCReturnDate);
                masterData.QCReqDate = formatDateToDefault(masterData.QCReqDate);
                masterData.QCReceiveDate = formatDateToDefault(masterData.QCReceiveDate);
                setFormData($formEl, masterData);

                initChildTable(masterData.Childs);

            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getDetails(id) {
        axios.get(`/api/yarn-qc-returnreceive/${id}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                reset();

                masterData = response.data;
                masterData.QCReturnReceivedDate = formatDateToDefault(masterData.QCReturnReceivedDate);
                masterData.QCReturnDate = formatDateToDefault(masterData.QCReturnDate);
                masterData.QCReqDate = formatDateToDefault(masterData.QCReqDate);
                masterData.QCReceiveDate = formatDateToDefault(masterData.QCReceiveDate);
                setFormData($formEl, masterData);

                masterData.Childs.map(x => {
                    _childRackBins.push({
                        ReceiveChildID: x.ReceiveChildID,
                        ChildRackBins: x.ChildRackBins
                    });
                });

                initChildTable(masterData.Childs);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function save() {
        var data = formDataToJson($formEl.serializeArray());
        data.Childs = $tblChildEl.getCurrentViewRecords();

        if (data.Childs.length == 0) {
            toastr.error('No item found to receive.');
            return false;
        }
        var tempChilds = data.Childs.filter(x => x.ReceiveQtyCarton == 0 && x.ReceiveQtyCone == 0 && x.ReceiveQty == 0);
        if (tempChilds.length > 0) {
            toastr.error('Must have cartor or cone or receive qty.');
            return false;
        }

        var hasError = false;
        for (var index = 0; index < data.Childs.length; index++) {
            var child = data.Childs[index];
            if (child.ReturnQty < child.ReceiveQty) {
                toastr.error("Receive qty cannot be greater than return qty.");
                hasError = true;
                break;
            }
            if (child.ReturnQtyCone < child.ReceiveQtyCone) {
                toastr.error("Receive cone cannot be greater than return no of cone.");
                hasError = true;
                break;
            }
            if (child.ReturnQtyCarton < child.ReceiveQtyCarton) {
                toastr.error("Receive carton cannot be greater than return no of carton.");
                hasError = true;
                break;
            }

            var indexF = _childRackBins.findIndex(x => x.ReceiveChildID == data.Childs[index].ReceiveChildID);
            if (indexF > -1) {
                _childRackBins[indexF].ChildRackBins.filter(x => x.YQCICRBId == 0).map(x => x.YQCICRBId = _maxYQCRRId++);
                data.Childs[index].ChildRackBins = _childRackBins[indexF].ChildRackBins.filter(x => x.ReceiveCartoon > 0 || x.ReceiveQtyCone > 0 || x.ReceiveQtyKg > 0);

                totalReceiveQty = 0;
                totalReceiveQtyCone = 0;
                totalReceiveQtyCarton = 0;

                _childRackBins[indexF].ChildRackBins.map(x => {
                    totalReceiveQty += x.ReceiveQtyKg;
                    totalReceiveQtyCone += x.ReceiveQtyCone;
                    totalReceiveQtyCarton += x.ReceiveCartoon;
                });
                data.Childs[index].ReceiveQty = totalReceiveQty;
                data.Childs[index].ReceiveQtyCone = totalReceiveQtyCone;
                data.Childs[index].ReceiveQtyCarton = totalReceiveQtyCarton;
            }
        }

        if (hasError) return false;

        if (masterData.QCReturnReceivedMasterID > 0) data.IsModified = true;

        axios.post("/api/yarn-qc-returnreceive/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }
    function DeepClone(obj) {
        return JSON.parse(JSON.stringify(obj));
    }
    function reset() {
        _childRackBins = [];
        _selectedReceiveChildId = 0;
        _existingRackBinsReciveChildWise = [];
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
                var currentRacks = $tblRackLocationItemEl.getCurrentViewRecords();
                res.rowData.RackBinType == "New";
                currentRacks.push(res.rowData);
                initRackLocation(currentRacks);

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
})();
