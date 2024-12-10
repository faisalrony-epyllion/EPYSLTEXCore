(function () {
    var menuId, pageName;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $tblChildEl, $formEl, tblMasterId, tblChildId,
        $modalRackItemEl, $tblRackLocationItemEl, tblRackLocationItem;

    var tableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    }
    var status;
    var _childRackBins = [];
    //_childRackBins.push({
    //    ReceiveChildID: _selectedReceiveChildId,
    //    ChildRackBins: []
    //});
    var _selectedReceiveChildId = 0;
    var _maxYQCICRBId = 999;

    var masterData;

    $(function () {
        if (!menuId)
            menuId = localStorage.getItem("menuId");
        if (!pageName)
            pageName = localStorage.getItem("pageName");

        var pageId = pageName + "-" + menuId;
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
        tblChildId = pageConstants.CHILD_TBL_ID_PREFIX + pageId;
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);
        $modalRackItemEl = $("#modalRackItem" + pageId);
        tblRackLocationItem = "#tblRackLocationItem" + pageId;

        status = statusConstants.PENDING;

        initMasterTable();

        $toolbarEl.find("#btnPending").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PENDING;

            initMasterTable();
        });

        $toolbarEl.find("#btnPartialList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PARTIALLY_COMPLETED;

            initMasterTable();
        });

        $toolbarEl.find("#btnApproveList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.APPROVED;

            initMasterTable();
        });

        $toolbarEl.find("#btnRejectList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.REJECT;

            initMasterTable();
        });

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save(this);
        });

        $formEl.find("#btnSaveAndApprove").click(function (e) {
            e.preventDefault();
            save(this, true, false);
        });

        $formEl.find("#btnReject").click(function (e) {
            e.preventDefault();
            save(this, false, true);
        });

        $formEl.find("#btnCancel").on("click", backToList);

        $formEl.find("#btnOk").click(function () {
            var hasErrorRack = false;
            if (_selectedReceiveChildId > 0) {
                var rackList = DeepClone($tblRackLocationItemEl.getCurrentViewRecords());
                var childList = DeepClone($tblChildEl.getCurrentViewRecords());

                var indexF = childList.findIndex(x => x.ReceiveChildID == _selectedReceiveChildId);
                if (indexF > -1) {
                    var totalIssueQty = 0;
                    var totalIssueQtyCone = 0;
                    var totalIssueQtyCarton = 0;

                    for (var iRack = 0; iRack < rackList.length; iRack++) {
                        var rack = rackList[iRack];
                        if (parseFloat(rack.IssueCartoon) > parseFloat(rack.NoOfCartoon)) {
                            hasErrorRack = true;
                            toastr.error("Issue cartoon (" + rack.IssueCartoon + ") cannot be greater then no of cartoon (" + rack.NoOfCartoon + ")");
                            break;
                        }
                        if (parseFloat(rack.IssueQtyCone) > parseFloat(rack.NoOfCone)) {
                            hasErrorRack = true;
                            toastr.error("Issue cone (" + rack.IssueQtyCone + ") cannot be greater then no of cone (" + rack.NoOfCone + ")");
                            break;
                        }
                        if (parseFloat(rack.IssueQtyKg) > parseFloat(rack.ReceiveQty)) {
                            hasErrorRack = true;
                            toastr.error("Issue qty (" + rack.IssueQtyKg + ") cannot be greater then stock qty (" + rack.ReceiveQty + ")");
                            break;
                        }

                        totalIssueQty += rack.IssueQtyKg;
                        totalIssueQtyCone += rack.IssueQtyCone;
                        totalIssueQtyCarton += rack.IssueCartoon;
                    }
                    if (!hasErrorRack) {
                        childList[indexF].IssueQty = totalIssueQty;
                        childList[indexF].IssueQtyCone = totalIssueQtyCone;
                        childList[indexF].IssueQtyCarton = totalIssueQtyCarton;

                        var childObj = DeepClone(childList[indexF]);
                        $tblChildEl.updateRow(indexF, childObj);
                    }
                }
            }
            if (!hasErrorRack) {
                $modalRackItemEl.modal('hide');
            }
        });
    });

    function initMasterTable() {
        var columns = [
            {
                headerText: '',
                textAlign: 'Center',
                width: 50,
                visible: status == statusConstants.PENDING,
                commands: [
                    {
                        type: 'Add', buttonOption: { cssClass: 'e-flat', iconCss: 'e-add e-icons' }
                    },
                    {
                        type: 'ViewReport', title: 'View Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' }
                    }
                ]
            },
            {
                headerText: '',
                textAlign: 'Center',
                width: 90,
                visible: status == statusConstants.PARTIALLY_COMPLETED,
                commands: [
                    {
                        type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' }
                    },
                    {
                        type: 'ViewReport', title: 'View Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' }
                    }
                ]
            },
            {
                headerText: '',
                textAlign: 'Center',
                width: 40,
                visible: status != statusConstants.PENDING && status != statusConstants.PARTIALLY_COMPLETED,
                commands: [
                    {
                        type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' }
                    }]
            },
            {
                field: 'QCIssueNo', headerText: 'Issue No', visible: status !== statusConstants.PENDING
            },
            {
                field: 'QCIssueDate', headerText: 'Issue Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, visible: status !== statusConstants.PENDING
            },
            {
                field: 'QCIssueByUser', headerText: 'Issue User', visible: status !== statusConstants.PENDING
            },
            {
                field: 'QCReqNo', headerText: 'Req No'
            },
            {
                field: 'QCReqDate', headerText: 'Req Date', type: 'date', format: _ch_date_format_1, textAlign: 'Center'
            },
            {
                field: 'ReceiveNo', headerText: 'Receive No'
            },
            {
                field: 'QCReqByUser', headerText: 'Req By'
            },
            //{
            //    field: 'QCReqFor', headerText: 'Req For'
            //}
        ];

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: false,
            apiEndPoint: `/api/yarn-qc-issue/list?status=${status}`,
            columns: columns,
            commandClick: handleCommands
        });
    }

    function handleCommands(args) {
        if (args.commandColumn.type == 'ViewReport') {
            window.open(`/reports/InlinePdfView?ReportName=DailyYarnQCRequisitionSlip.rdl&QCReqMasterID=${args.rowData.QCReqMasterID}`, '_blank');
        }
        else if (status === statusConstants.PENDING) {
            $formEl.find("#btnSave,#btnSaveAndApprove,#btnReject").fadeIn();

            if (args.rowData.IsMRIRCompleted == true) {
                toastr.error("Can't issue. Already MRIR Completed.");
                return;
            }

            getNew(args.rowData.QCReqMasterID);
        }
        else if (status === statusConstants.PARTIALLY_COMPLETED) {
            $formEl.find("#btnSave,#btnSaveAndApprove,#btnReject").fadeIn();
            getDetails(args.rowData.QCIssueMasterID);
        }
        else {
            $formEl.find("#btnSave,#btnSaveAndApprove,#btnReject").fadeOut();
            getDetails(args.rowData.QCIssueMasterID);
        }
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
                { field: 'QCIssueChildID', isPrimaryKey: true, visible: false },
                { field: 'QCIssueMasterID', visible: false },
                { field: 'QCReqChildID', visible: false },
                { field: 'ReceiveChildID', visible: false },

                { field: 'Segment1ValueDesc', headerText: 'Composition', allowEditing: false },
                { field: 'Segment2ValueDesc', headerText: 'Yarn Type', allowEditing: false },
                { field: 'Segment3ValueDesc', headerText: 'Process', allowEditing: false },
                { field: 'Segment4ValueDesc', headerText: 'Sub process', allowEditing: false },
                { field: 'Segment5ValueDesc', headerText: 'Quality Parameter', allowEditing: false },
                { field: 'Segment6ValueDesc', headerText: 'Yarn Count', allowEditing: false },
                { field: 'Segment7ValueDesc', headerText: 'No of Ply', allowEditing: false },
                { field: 'ShadeCode', headerText: 'Shade Code', allowEditing: false },
                { field: 'Uom', headerText: 'Uom', allowEditing: false, textAlign: 'Center' },
                { field: 'ChallanLot', headerText: 'Challan Lot', allowEditing: false },
                { field: 'LotNo', headerText: 'Physical Lot', allowEditing: false, textAlign: 'Center' },
                { field: 'PhysicalCount', headerText: 'Physical Count', allowEditing: false, textAlign: 'Center' },

                { field: 'SupplierName', headerText: 'Supplier', allowEditing: false, textAlign: 'left' },
                { field: 'Spinner', headerText: 'Spinner', allowEditing: false, textAlign: 'left' },

                { field: 'ReqBagPcs', headerText: 'Req Bag (Pcs)', width: 120, allowEditing: false },
                { field: 'ReqQtyCone', headerText: 'Req Qty (Cone)', allowEditing: false },
                { field: 'ReqQty', headerText: 'Req Qty (KG)', allowEditing: false },

                { field: 'IssueQtyCarton', headerText: 'Issue Qty (Carton)', allowEditing: false, textAlign: 'center', width: 120, valueAccessor: diplayNumberButton },
                { field: 'IssueQtyCone', headerText: 'Issue Qty (Cone)', allowEditing: false, textAlign: 'center', width: 120, valueAccessor: diplayNumberButton },
                { field: 'IssueQty', headerText: 'Issue Qty (KG)', allowEditing: false, textAlign: 'center', width: 120, valueAccessor: diplayNumberButton },

                { field: 'QCReqRemarks', headerText: 'Remarks', allowEditing: false, width: 100 }

            ],
            recordClick: function (args) {

                if (args.column && (args.column.field == "IssueQty" || args.column.field == "IssueQtyCone" || args.column.field == "IssueQtyCarton")) {
                    _selectedReceiveChildId = args.rowData.ReceiveChildID;
                    var childRackBinsExisting = args.rowData.ChildRackBins;
                    axios.get(`/api/yarn-rack-bin-allocation/get-by-receive-child/${_selectedReceiveChildId}/${masterData.LocationId}/0`)
                        .then(function (response) {
                            var list = response.data;
                            if (list.length == 0) {
                                toastr.error("Rack bin allocation not completed.");
                                return false;
                            }

                            //ChildRackBins
                            var issueCartoon = 0;
                            var issueQtyCone = 0;
                            var issueQtyKg = 0;

                            if ($tblRackLocationItemEl) $tblRackLocationItemEl.destroy();
                            //Set pop up field values
                            var indexF = _childRackBins.findIndex(x => x.ReceiveChildID == _selectedReceiveChildId);
                            if (indexF > -1) {
                                var childRackBins = _childRackBins[indexF].ChildRackBins;
                                list.map(x => {
                                    var indexC = childRackBins.findIndex(y => y.ChildRackBinID == x.ChildRackBinID);
                                    if (indexC > -1) {
                                        var crbObj = childRackBins[indexC];

                                        //------------Add Existing Qty-------------------------------
                                        issueCartoon = 0;
                                        issueQtyCone = 0;
                                        issueQtyKg = 0;

                                        childRackBinsExisting.filter(e => e.ChildRackBinID == x.ChildRackBinID).map(c => {
                                            issueCartoon += isNaN(c.IssueCartoon) ? 0 : c.IssueCartoon;
                                            issueQtyCone += isNaN(c.IssueQtyCone) ? 0 : c.IssueQtyCone;
                                            issueQtyKg += isNaN(c.IssueQtyKg) ? 0 : c.IssueQtyKg;
                                        });

                                        x.NoOfCartoon = x.NoOfCartoon + issueCartoon;
                                        x.NoOfCone = x.NoOfCone + issueQtyCone;
                                        x.ReceiveQty = x.ReceiveQty + issueQtyKg;
                                        //--------------------------------------------------------------

                                        x.YQCICRBId = crbObj.YQCICRBId;
                                        x.IssueCartoon = crbObj.IssueCartoon;
                                        x.IssueQtyCone = crbObj.IssueQtyCone;
                                        x.IssueQtyKg = crbObj.IssueQtyKg;
                                    }
                                });
                            }
                            list.filter(x => x.YQCICRBId == 0).map(y => {
                                y.YQCICRBId = _maxYQCICRBId++;
                            });

                            ej.base.enableRipple(true);
                            $tblRackLocationItemEl = new ej.grids.Grid({
                                dataSource: list,
                                allowResizing: true,

                                columns: [
                                    { field: 'ChildRackBinID', isPrimaryKey: true, visible: false, width: 10 },
                                    { field: 'QCIssueChildID', visible: false, width: 10 },
                                    { field: 'LocationName', headerText: 'Location', allowEditing: false, width: 80 },
                                    { field: 'RackNo', headerText: 'Rack', allowEditing: false, width: 80 },
                                    { field: 'ReceiveQty', headerText: 'Rack Qty', allowEditing: false, width: 80 },
                                    { field: 'YarnControlNo', headerText: 'Control No', allowEditing: false, width: 80 },
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
                                        var indexF = _childRackBins.findIndex(x => x.ReceiveChildID == _selectedReceiveChildId);
                                        if (indexF == -1) {
                                            _childRackBins.push({
                                                ReceiveChildID: _selectedReceiveChildId,
                                                ChildRackBins: []
                                            });
                                            indexF = _childRackBins.findIndex(x => x.ReceiveChildID == _selectedReceiveChildId);
                                            if (indexF > -1) {
                                                if (args.data.YQCICRBId == 0) args.data.YQCICRBId = _maxYQCICRBId++;
                                                _childRackBins[indexF].ChildRackBins.push(args.data);
                                            }
                                        } else {
                                            var indexC = _childRackBins[indexF].ChildRackBins.findIndex(x => x.ChildRackBinID == args.data.ChildRackBinID);
                                            if (indexC == -1) {
                                                if (args.data.YQCICRBId == 0) args.data.YQCICRBId = _maxYQCICRBId++;
                                                _childRackBins[indexF].ChildRackBins.push(args.data);
                                            } else {
                                                _childRackBins[indexF].ChildRackBins[indexC] = args.data;
                                            }
                                        }
                                    }
                                },
                            });
                            $tblRackLocationItemEl.refreshColumns;
                            $tblRackLocationItemEl.appendTo(tblRackLocationItem);
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
        initMasterTable();
    }

    function resetForm() {
        $formEl.trigger("reset");
        $formEl.find("#QCIssueMasterID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function resetTableParams() {
        tableParams.offset = 0;
        tableParams.limit = 10;
        tableParams.filter = '';
        tableParams.sort = '';
        tableParams.order = '';
    }
    function reset() {
        _childRackBins = [];
        _selectedReceiveChildId = 0;
    }
    function getNew(qcReqMasterID) {
        axios.get(`/api/yarn-qc-issue/new/${qcReqMasterID}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                reset();

                masterData = response.data;

                masterData.QCReqDate = formatDateToDefault(masterData.QCReqDate);
                masterData.QCIssueDate = formatDateToDefault(masterData.QCIssueDate);
                setFormData($formEl, masterData);
                initChildTable(masterData.YarnQCIssueChilds);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getDetails(id) {
        axios.get(`/api/yarn-qc-issue/${id}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                reset();

                masterData = response.data;
                masterData.QCReqDate = formatDateToDefault(masterData.QCReqDate);
                masterData.QCIssueDate = formatDateToDefault(masterData.QCIssueDate);
                setFormData($formEl, masterData);

                masterData.YarnQCIssueChilds.map(x => {
                    _childRackBins.push({
                        ReceiveChildID: x.ReceiveChildID,
                        ChildRackBins: x.ChildRackBins
                    });
                });

                initChildTable(masterData.YarnQCIssueChilds);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function save(invokedBy, approve = false, reject = false) {
        var data = formDataToJson($formEl.serializeArray());
        data.YarnQCIssueChilds = $tblChildEl.getCurrentViewRecords();
        data.Approve = approve;
        data.Reject = reject;
        data.ReceiveID = masterData.ReceiveID;

        var totalIssueQty = 0;
        var totalIssueQtyCone = 0;
        var totalIssueQtyCarton = 0;

        var hasError = false;

        //Validation check
        for (var index = 0; index < data.YarnQCIssueChilds.length; index++) {
            var child = data.YarnQCIssueChilds[index];
            if (child.ReqBagPcs > 0 && child.IssueQtyCarton == 0) {
                toastr.error("Give Issue Qty Carton.");
                hasError = true;
                break;
            }
            if (child.ReqQtyCone > 0 && child.IssueQtyCone == 0) {
                toastr.error("Give Issue Qty Cone.");
                hasError = true;
                break;
            }
            if (child.ReqQty > 0 && child.IssueQty == 0) {
                toastr.error("Give Issue Qty.");
                hasError = true;
                break;
            }
            /*
            if (child.IssueQtyCarton > child.ReqBagPcs) {
                toastr.error("Issue Qty (Carton) (" + child.IssueQtyCarton + ") cannot be greater than Req Bag (Pcs) (" + child.ReqBagPcs + ").");
                hasError = true;
                break;
            }
            if (child.IssueQtyCone > child.ReqQtyCone) {
                toastr.error("Issue Qty (Cone) (" + child.IssueQtyCone + ") cannot be greater than Req Qty (Cone) (" + child.ReqQtyCone + ").");
                hasError = true;
                break;
            }
            if (child.IssueQty > child.ReqQty) {
                toastr.error("Issue Qty (KG) (" + child.IssueQty + ") cannot be greater than Req Qty (KG) (" + child.ReqQty + ").");
                hasError = true;
                break;
            }*/
        }
        if (hasError) return false;

        for (var index = 0; index < data.YarnQCIssueChilds.length; index++) {
            var indexF = _childRackBins.findIndex(x => x.ReceiveChildID == data.YarnQCIssueChilds[index].ReceiveChildID);
            if (indexF > -1) {

                _childRackBins[indexF].ChildRackBins.filter(x => x.YQCICRBId == 0).map(x => x.YQCICRBId = _maxYQCICRBId++);
                data.YarnQCIssueChilds[index].ChildRackBins = _childRackBins[indexF].ChildRackBins.filter(x => x.IssueCartoon > 0 || x.IssueQtyCone > 0 || x.IssueQtyKg > 0);

                totalIssueQty = 0;
                totalIssueQtyCone = 0;
                totalIssueQtyCarton = 0;

                _childRackBins[indexF].ChildRackBins.map(x => {
                    totalIssueQty += x.IssueQtyKg;
                    totalIssueQtyCone += x.IssueQtyCone;
                    totalIssueQtyCarton += x.IssueCartoon;
                });
                data.YarnQCIssueChilds[index].IssueQty = totalIssueQty;
                data.YarnQCIssueChilds[index].IssueQtyCone = totalIssueQtyCone;
                data.YarnQCIssueChilds[index].IssueQtyCarton = totalIssueQtyCarton;
            }
        }

        axios.post("/api/yarn-qc-issue/save", data)
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
})();