(function () {
    'use strict'
    var currentChildRowData;
    var _currentRowForDD = {};
    var menuId, pageName;
    var toolbarId, pageId;
    var $pageEl, $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, tblMasterId, $formEl, $tblChildEl, tblChildId, $tblCreateCompositionEl, tblCreateCompositionId;
    var status;
    var index;
    var compositionComponents = [];
    var resultItem = null;
    var isYQCRemarks = false, isYQCRemarksApproval = false;

    var isEditable = false;
    var isAcknowledge = false;
    var tableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    }
    var status;

    var masterData;
    var dataset = new Array();
    var _QCRemarksChildResultID = 99999;
    var _QCRemarksChildFiberID = 99999;

    $(function () {

        if (!menuId)
            menuId = localStorage.getItem("menuId");
        if (!pageName)
            pageName = localStorage.getItem("pageName");

        pageId = pageName + "-" + menuId;
        $pageEl = $(pageConstants.PAGE_ID_PREFIX + pageId);
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
        tblChildId = pageConstants.CHILD_TBL_ID_PREFIX + pageId;
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);
        tblCreateCompositionId = `#tblCreateComposition-${pageId}`;

        isYQCRemarks = convertToBoolean($(`#${pageId}`).find("#YQCRemarks").val());
        isYQCRemarksApproval = convertToBoolean($(`#${pageId}`).find("#YQCRemarksApproval").val());

        $toolbarEl.find("#btnPending").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PENDING;
            initMasterTable();
        });
        $toolbarEl.find("#btnDraft").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.DRAFT;
            initMasterTable();
        });
        $toolbarEl.find("#btnPendingForApproval").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PROPOSED_FOR_APPROVAL;
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
        $toolbarEl.find("#btnCommerciallyApproveList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.APPROVED2;
            initMasterTable();
        });
        $toolbarEl.find("#btnRetestList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.ReTest;
            initMasterTable();
        });
        $toolbarEl.find("#btnDiagnosticList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.CHECK_REJECT;
            initMasterTable();
        });
        $toolbarEl.find("#btnAllList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.ALL;
            initMasterTable();
        });

        $formEl.find("#btnCancel").on("click", backToList);

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save(false, false, false);
        });
        $formEl.find("#btnSaveAndSendForApproval").click(function (e) {
            e.preventDefault();
            save(true, false, false);
        });
        $formEl.find("#btnRetest").click(function (e) {
            e.preventDefault();
            save(false, true, false);
        });
        $formEl.find("#btnRetestForRequisition").click(function (e) {
            e.preventDefault();
            save(false, false, true);
        });

        $formEl.find("#btnApprove").click(function (e) {
            e.preventDefault();
            approve(dataset, index);
        });

        if (!isYQCRemarks) {
            $toolbarEl.find("#btnPendingListForApproval").show();
            $toolbarEl.find("#btnApprovalList").show();
            $formEl.find("#btnApprove").show();
            $toolbarEl.find("#btnPending,#btnApproveList,#btnRejectList,#btnRetestList,#btnDiagnosticList,#btnCommerciallyApproveList,#btnAllList,#btnDraft,#btnPendingForApproval").hide();

            $toolbarEl.find("#btnPendingListForApproval").on("click", function (e) {
                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);
                resetTableParams();
                status = statusConstants.PROPOSED_FOR_APPROVAL;
                $formEl.find("#btnSave").hide();
                $formEl.find("#btnApprove").show();
                $formEl.find("#btnSaveAndSendForApproval").hide();
                initMasterTable();
            });

            $toolbarEl.find("#btnApprovalList").on("click", function (e) {
                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);
                resetTableParams();
                status = statusConstants.APPROVED_DONE;
                $formEl.find("#btnSave").hide();
                $formEl.find("#btnApprove").hide();
                initMasterTable();
            });
            $toolbarEl.find("#btnPendingListForApproval").click();
        }
        else {
            $toolbarEl.find("#btnPending,#btnApproveList,#btnRejectList,#btnRetestList,#btnDiagnosticList,#btnCommerciallyApproveList,#btnAllList").show();
            $toolbarEl.find("#btnPendingListForApproval").hide();

            $formEl.find("#btnApprove").hide();
            $formEl.find("#btnSaveAndSendForApproval").show();
            $toolbarEl.find("#btnApprovalList").hide();
            $toolbarEl.find("#btnPending").click();
        }

        $pageEl.find("#btnAddComposition").click(saveComposition);
    });


    function initMasterTable() {
        var commandList = [],
            commandsWidth = 40;

        if (status == statusConstants.PENDING) {
            commandList = [{ type: 'Add', buttonOption: { cssClass: 'e-flat', iconCss: 'e-add e-icons' } }];
        }
        else if (status != statusConstants.ALL) {
            commandList = [
                { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                { type: 'Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
            ];
            commandsWidth = 80;
        }

        var columns = [
            {
                headerText: '', width: status == statusConstants.PENDING ? 40 : 80, textAlign: 'center', visible: status != statusConstants.ALL,
                commands: commandList
                /*commands: [{ type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } }]*/
            },
            {
                field: 'Status', headerText: 'Status', width: 160, allowEditing: false, visible: status == statusConstants.ALL
            },
            {
                field: 'QCReceiveChildID', width: 40, headerText: 'QCReceiveChildID', visible: false
            },
            {
                field: 'QCRemarksChildID', width: 40, headerText: 'QCRemarksChildID', visible: false
            },
            { field: 'YarnDetail', headerText: 'Yarn Details', width: 400 },

            { field: 'ReceiveDate', headerText: 'Receive Date', width: 100, textAlign: 'Center', type: 'date', format: _ch_date_format_1 },
            { field: 'ApprovedDate', headerText: 'Approved Date', width: 100, textAlign: 'Center', type: 'date', format: _ch_date_format_1, visible: (status != statusConstants.PENDING && status != statusConstants.DRAFT && status != statusConstants.PROPOSED_FOR_APPROVAL) },
            { field: 'Spinner', headerText: 'Spinner', width: 130 },
            { field: 'LotNo', headerText: 'Physical Lot', width: 100 },
            { field: 'TechnicalName', headerText: 'Technical Name', width: 100 },
            //{ field: 'BuyerName', headerText: 'Buyer', width: 100 },
            { field: 'Remarks', headerText: 'Remarks', width: 100, visible: status != statusConstants.PENDING },
            { field: 'YarnStatus', headerText: 'Zone', width: 100, visible: status != statusConstants.PENDING },
            {
                field: 'ReceiveNo', width: 140, headerText: 'Receive No'
            },
            {
                field: 'QCReqNo', width: 140, headerText: 'QC Req No'
            },
            {
                field: 'QCIssueNo', width: 140, headerText: 'QC Issue No'
            },
            {
                field: 'QCReceiveNo', width: 140, headerText: 'QC Receive No'
            },
            {
                field: 'QCRemarksNo', width: 140, headerText: 'QC Remarks No', visible: status != statusConstants.PENDING
            },
            { field: 'QCRemarksByUser', headerText: 'Remarks By', width: 80, visible: status != statusConstants.PENDING },
            { field: 'RetestReqBy', headerText: 'Retest Req By', width: 80, visible: status != statusConstants.PENDING },
            { field: 'RetestReason', headerText: 'Retest Reason', width: 80, visible: status != statusConstants.PENDING }
            /*

            {
                field: 'QCReceiveDate', width: 80, headerText: 'Receive Date', textAlign: 'Center', type: 'date', format:_ch_date_format_1, visible: status != statusConstants.PENDING
            },
            {
                field: 'QCIssueNo', width: 100, headerText: 'Issue No', visible: status != statusConstants.PENDING
            },
            {
                field: 'QCIssueDate', width: 80, headerText: 'Issue Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, visible: status != statusConstants.PENDING
            },
            {
                field: 'QCRemarksNo', width: 80, headerText: 'QC No', visible: status !== statusConstants.PENDING
            },
            {
                field: 'QCRemarksDate', width: 80, headerText: 'QC Date', type: 'date', format: _ch_date_format_1, visible: status !== statusConstants.PENDING
            },
            {
                field: 'QCReqFor', width: 80, headerText: 'Req For', visible: status !== statusConstants.PENDING
            },
            {
                field: 'QCReceivedByUser', width: 80, headerText: 'Receive User', visible: status != statusConstants.PENDING
            },
            {
                field: 'ReceiveQty', width: 80, headerText: 'Receive Qty', visible: status !== statusConstants.PENDING
            },
            {
                field: 'ReceiveQtyCone', width: 80, headerText: 'Receive Qty(Cone)', visible: status !== statusConstants.PENDING
            },
            {
                field: 'ReceiveQtyCarton', width: 80, headerText: 'Receive Qty(Carton)', visible: status !== statusConstants.PENDING
            }
            */
        ];

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: false,
            apiEndPoint: `/api/yarn-qc-remarks/list?status=${status}`,
            columns: columns,
            commandClick: handleCommands
        });

    }
    function handleCommands(args) {
        if (args.commandColumn.type == 'Report') {
            window.open(`/reports/InlinePdfView?ReportName=YarnTestReport.rdl&QCRemarksMasterId=${args.rowData.QCRemarksMasterID}`, '_blank');
        }
        else if (status === statusConstants.PENDING) {
            getNew(args.rowData.QCReceiveChildID);
        }
        else {
            getDetails(args.rowData.QCRemarksChildID);
        }
    }
    async function initChildTable(data) {
        var isEditable = true;
        if ($tblChildEl) $tblChildEl.destroy();

        var columns = [];

        var additionalColumns = [
            { field: 'QCRemarksChildID', isPrimaryKey: true, visible: false },
            { field: 'QCRemarksMasterID', visible: false },
            { field: 'QCReceiveChildID', visible: false },
            { field: 'YarnStatusID', visible: false },
            { field: 'ReceiveChildID', visible: false },

            { field: 'ReceiveDate', headerText: 'Receive Date', width: 100, textAlign: 'Center', type: 'date', format: _ch_date_format_1, allowEditing: false },
            { field: 'YarnDetail', headerText: 'Yarn Details', allowEditing: false, width: 230 },
            { field: 'Spinner', headerText: 'Spinner', allowEditing: false, width: 130 },
            { field: 'ChallanLot', headerText: 'Challan Lot', allowEditing: false, width: 100 },
            { field: 'LotNo', headerText: 'Physical Lot', allowEditing: false, textAlign: 'Center', width: 100 },
            { field: 'PhysicalCount', headerText: 'Physical Count', allowEditing: false, textAlign: 'Center', width: 100 },

            { field: 'NoOfCartoon', headerText: 'Yarn Store Carton(Pcs)', allowEditing: false, textAlign: 'Center', width: 100 },
            { field: 'NoOfCone', headerText: 'Yarn Store Cone(Pcs)', allowEditing: false, textAlign: 'Center', width: 100 },
            { field: 'ReceiveQtyYS', headerText: 'Yarn Store Receive Qty(KG)', allowEditing: false, textAlign: 'Center', width: 100 },

            { field: 'ReceiveQtyCarton', headerText: 'Receive Qty Bag/Carton(Pcs)', allowEditing: false, textAlign: 'Center', width: 100 },
            { field: 'ReceiveQtyCone', headerText: 'Receive Qty Cone(Pcs)', allowEditing: false, textAlign: 'Center', width: 100 },
            { field: 'ReceiveQty', headerText: 'Receive Qty(KG)', allowEditing: false, textAlign: 'Center', width: 100 },
            //{
            //    field: 'TechnicalNameID', headerText: 'Fabric Technical Name', allowEditing: isEditable,
            //    valueAccessor: ej2GridDisplayFormatterV2,
            //    dataSource: masterData.TechnicalNameList,
            //    displayField: "TechnicalName", edit: ej2GridDropDownObj({
            //    })
            //},
            { field: 'BuyerName', headerText: 'Buyer', width: 100, allowEditing: false },
            { field: 'Remarks', headerText: 'Remarks', width: 100 },
            {
                field: 'YarnStatusID', headerText: 'Zone', allowEditing: isEditable,
                valueAccessor: ej2GridDisplayFormatter,
                dataSource: masterData.YarnAssessmentZoneList,
                displayField: "YarnStatus", edit: ej2GridDropDownObj({
                })
            },
            {
                field: 'id', headerText: 'Status', allowEditing: isEditable, required: true, width: 200, valueAccessor: ej2GridDisplayFormatter,
                dataSource: masterData.YarnAssessmentStatusList,
                displayField: "text", edit: ej2GridDropDownObj({
                    onChange: function (selectedData, currentRowData) {
                        index = $tblChildEl.getRowIndexByPrimaryKey(currentRowData.QCRemarksChildID);
                        if (selectedData.id == "Approve") {
                            var data = currentRowData;
                            data.Remarks = currentRowData.Remarks ? currentRowData.Remarks.trim() : "";
                            data.Approve = true;
                            data.Reject = false;
                            data.ReTest = false;
                            data.Diagnostic = false;
                            data.CommerciallyApprove = false;
                            dataset.push(data);
                        }
                        else if (selectedData.id == "Reject") {
                            var data = currentRowData;
                            data.Remarks = currentRowData.Remarks ? currentRowData.Remarks.trim() : "";
                            data.Approve = false;
                            data.Reject = true;
                            data.ReTest = false;
                            data.Diagnostic = false;
                            dataset.push(data);
                        }
                        else if (selectedData.id == "ReTest") {
                            var data = currentRowData;
                            data.Remarks = currentRowData.Remarks ? currentRowData.Remarks.trim() : "";
                            data.Approve = false;
                            data.Reject = false;
                            data.ReTest = true;
                            data.Diagnostic = false;
                            data.CommerciallyApprove = false;
                            dataset.push(data);
                        }
                        else if (selectedData.id == "Diagnostic") {
                            var data = currentRowData;
                            data.Remarks = currentRowData.Remarks ? currentRowData.Remarks.trim() : "";
                            data.Approve = false;
                            data.Reject = false;
                            data.ReTest = false;
                            data.Diagnostic = true;
                            data.CommerciallyApprove = false;
                            dataset.push(data);
                        }
                        else if (selectedData.id == "CommerciallyApprove") {
                            var data = currentRowData;
                            data.Remarks = currentRowData.Remarks ? currentRowData.Remarks.trim() : "";
                            data.Approve = false;
                            data.Reject = false;
                            data.ReTest = false;
                            data.Diagnostic = false;
                            data.CommerciallyApprove = true;
                            dataset.push(data);
                        }
                    }
                })
            },
            {
                headerText: 'Composition', textAlign: 'Center', width: 80, commands: [
                    {
                        buttonOption: {
                            type: 'AddComposition', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-plus'
                        }

                    }
                ]
            }

        ];
        columns.push.apply(columns, additionalColumns);

        var childColumns = [
            { field: 'QCRemarksChildResultID', isPrimaryKey: true, visible: false },
            { field: 'QCRemarksChildID', visible: false },
            {
                headerText: 'Action', width: 100, commands: [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                    { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                    { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                    { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }
                ]
            },
            { field: 'ACountNe', headerText: 'A/Count Ne', width: 150 },
            { field: 'Twist', headerText: 'Twist', width: 150 },
            { field: 'CSP', headerText: 'CSP', width: 150 },
            { field: 'ColorName', headerText: 'Fabric Color Code', width: 150, allowEditing: false },
            {
                field: 'TechnicalNameID', headerText: 'Fabric Technical Name', allowEditing: isEditable,
                valueAccessor: ej2GridDisplayFormatter,
                dataSource: masterData.TechnicalNameList,
                displayField: "TechnicalName", edit: ej2GridDropDownObj({
                })
            },
            {
                field: 'DyeingProcessID', headerText: 'Dyeing Process'
                , valueAccessor: ej2GridDisplayFormatter, displayField: "DyeingProcessName", dataSource: masterData.DPList, edit: ej2GridDropDownObj({
                })
            },
            {
                field: 'ThickThin', headerText: 'Thick-thin', allowEditing: isEditable,
                valueAccessor: ej2GridDisplayFormatter,
                dataSource: masterData.TestParamSetList.filter(x => x.additionalValue == 1),
                displayField: "ThickThinName", edit: ej2GridDropDownObj({
                })
            },
            {
                field: 'BarreMark', headerText: 'Barre Mark', allowEditing: isEditable,
                valueAccessor: ej2GridDisplayFormatter,
                dataSource: masterData.TestParamSetList.filter(x => x.additionalValue == 2),
                displayField: "BarreMarkName", edit: ej2GridDropDownObj({
                })
            },
            {
                field: 'Naps', headerText: 'Naps', allowEditing: isEditable,
                valueAccessor: ej2GridDisplayFormatter,
                dataSource: masterData.TestParamSetList.filter(x => x.additionalValue == 1),
                displayField: "NapsName", edit: ej2GridDropDownObj({
                })
            },
            {
                field: 'Hairiness', headerText: 'Hairiness', allowEditing: isEditable,
                valueAccessor: ej2GridDisplayFormatter,
                dataSource: masterData.TestParamSetList.filter(x => x.additionalValue == 1),
                displayField: "HairinessName", edit: ej2GridDropDownObj({
                })
            },
            {
                field: 'WhiteSpecks', headerText: 'White Specks / Black Specks', allowEditing: isEditable,
                valueAccessor: ej2GridDisplayFormatter,
                dataSource: masterData.TestParamSetList.filter(x => x.additionalValue == 3),
                displayField: "WhiteSpecksName", edit: ej2GridDropDownObj({
                })
            },
            {
                field: 'Polypropylyne', headerText: 'Polypropylyne', allowEditing: isEditable,
                valueAccessor: ej2GridDisplayFormatter,
                dataSource: masterData.TestParamSetList.filter(x => x.additionalValue == 3),
                displayField: "PolypropylyneName", edit: ej2GridDropDownObj({
                })
            },
            {
                field: 'Contamination', headerText: 'Contamination', allowEditing: isEditable,
                valueAccessor: ej2GridDisplayFormatter,
                dataSource: masterData.TestParamSetList.filter(x => x.additionalValue == 4),
                displayField: "ContaminationName", edit: ej2GridDropDownObj({
                })
            },
            {
                field: 'DyePicksUpPerformance', headerText: 'Dye Picks Up Performance', allowEditing: isEditable,
                valueAccessor: ej2GridDisplayFormatter,
                dataSource: masterData.TestParamSetList.filter(x => x.additionalValue == 5),
                displayField: "DyePicksUpPerformanceName", edit: ej2GridDropDownObj({
                })
            },
            {
                field: 'TestMethodRefID', headerText: 'Test Method Ref', allowEditing: isEditable,
                valueAccessor: ej2GridDisplayFormatter,
                dataSource: masterData.BuyerList,
                displayField: "TestMethodRefName", edit: ej2GridDropDownObj({
                })
            },
            { field: 'PillingGrade', headerText: 'Pilling Grade', width: 150 },
            { field: 'Remarks', headerText: 'Remarks', width: 150 },

        ];

        ej.base.enableRipple(true);
        $tblChildEl = new ej.grids.Grid({
            dataSource: data,
            allowResizing: true,
            showColumnChooser: true,
            showDefaultToolbar: false,
            editSettings: { allowAdding: true, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
            commandClick: childCommandClick,
            actionBegin: function (args) {
                if (args.requestType === "save") {
                    args.data = setTechnicalInfo(args.data);
                    args.data.ReTest = args.rowData.ReTest;
                    $tblChildEl.updateRow(args.rowIndex, args.data);
                }

            },
            columns: columns,
            childGrid: {
                //queryString: menuParam == "MRPBAck" ? 'YBChildID' : 'FCMRMasterID',
                queryString: 'QCRemarksChildID',
                allowResizing: true,
                autofitColumns: false,
                toolbar: [{ text: 'Add Item', tooltipText: 'Add Item', prefixIcon: 'e-expand e-add', id: 'addItem' }],
                //toolbar: ['Add'],
                editSettings: { allowEditing: true, allowAdding: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
                columns: childColumns,
                actionBegin: function (args) {
                    if (args.requestType === "save") {
                        this.parentDetails.parentRowData = setTechnicalInfo(this.parentDetails.parentRowData);
                    }
                },
                enableContextMenu: true,
                contextMenuItems: [
                    { text: 'Copy Information', target: '.e-content', id: 'copy' },
                    { text: 'Paste Information', target: '.e-content', id: 'paste' }
                ],
                contextMenuClick: function (args) {
                    if (args.item.id === 'copy') {
                        resultItem = objectCopy(args.rowInfo.rowData);
                        if (resultItem.length == 0) {
                            toastr.error("No Yarn information found to copy!!");
                            return;
                        }
                    }
                    else if (args.item.id === 'paste') {
                        if (resultItem == null || resultItem.length == 0) {
                            toastr.error("Please copy first!!");
                            return;
                        } else {
                            var copiedItem = objectCopy(resultItem);

                            var fabricColorID = args.rowInfo.rowData.FabricColorID;
                            var colorName = args.rowInfo.rowData.ColorName;
                            args.rowInfo.rowData = DeepClone(copiedItem);
                            args.rowInfo.rowData.FabricColorID = fabricColorID;
                            args.rowInfo.rowData.ColorName = colorName;
                            args.rowInfo.rowData.QCRemarksChildResultID = _QCRemarksChildResultID++;

                            var indexF = this.parentDetails.parentRowData.YarnQCRemarksChildResults.findIndex(x => x.FabricColorID == fabricColorID);
                            if (indexF > -1) {
                                this.parentDetails.parentRowData.YarnQCRemarksChildResults[indexF] = args.rowInfo.rowData;
                            }
                            $tblChildEl.refresh();
                        }
                    }
                },
                toolbarClick: function (args) {
                    if (args.item.id === 'addItem') {
                        var parentData = this.parentDetails.parentRowData;
                        var qcRemarksChildID = parentData.QCRemarksChildID;

                        var finder = new commonFinder({
                            title: "Select Color",
                            pageId: pageId,
                            height: 350,
                            apiEndPoint: "/api/fabric-color-book-setups/allcolor",
                            fields: "ColorSource,ColorCode,ColorName,RGBOrHex",
                            headerTexts: "Source,Code,Name,Visual",
                            customFormats: ",,,ej2GridColorFormatter",
                            widths: "50,80,150,100",
                            isMultiselect: true,
                            primaryKeyColumn: "PTNID",
                            onMultiselect: function (selectedRecords) {
                                var results = [];
                                selectedRecords.map(x => {
                                    results.push({
                                        QCRemarksChildResultID: _QCRemarksChildResultID++,
                                        QCRemarksChildID: qcRemarksChildID,
                                        ACountNe: "",
                                        Twist: 0,
                                        CSP: 0,
                                        FabricColorID: x.ColorID,
                                        ColorName: x.ColorName,
                                        DyeingProcessID: 0,
                                        ThickThin: 0,
                                        BarreMark: 0,
                                        Naps: 0,
                                        Hairiness: 0,
                                        WhiteSpecks: 0,
                                        Polypropylyne: 0,
                                        Contamination: 0,
                                        DyePicksUpPerformance: 0,
                                        TestMethodRefID: 0,
                                        TechnicalNameID: 0,
                                        PillingGrade: "",
                                        Remarks: ""
                                    });
                                });

                                results.map(x => {
                                    var indexF = parentData.YarnQCRemarksChildResults.findIndex(c => c.FabricColorID == x.FabricColorID && c.TestMethodRefID == x.TestMethodRefID && c.PillingGrade == x.PillingGrade);
                                    if (indexF == -1) parentData.YarnQCRemarksChildResults.push(x);
                                });
                                $tblChildEl.updateRow(0, parentData);
                            }
                        });
                        finder.showModal();
                    }
                },
                actionBegin: function (args) {
                    if (args.requestType === "save") {
                        var parentData = this.parentDetails.parentRowData;
                        var indexF = parentData.YarnQCRemarksChildResults.findIndex(x => x.QCRemarksChildResultID == args.data.QCRemarksChildResultID);
                        if (indexF > -1) {
                            parentData.YarnQCRemarksChildResults[indexF] = args.data;
                        }
                    }

                },
                load: loadChildResults,
            },
        });
        $tblChildEl.refreshColumns;
        $tblChildEl.appendTo(tblChildId);
    }
    async function childCommandClick(e) {
        if (e.commandColumn.buttonOption.type == 'AddComposition') {
            initTblCreateComposition(e.rowData.QCRemarksChildID);
            $pageEl.find(`#modal-new-composition-${pageId}`).modal("show");
        }
    }
    function initTblCreateComposition(qcRemarksChildID) {
        var data = masterData.YarnQCRemarksChilds.find(x => x.QCRemarksChildID == qcRemarksChildID);
        if (data) {
            data = data.YarnQCRemarksChildFibers;
        }
        //compositionComponents = [];
        var ChildFiberColumns = [
            { field: 'QCRemarksChildFiberID', isPrimaryKey: true, visible: false },
            { field: 'QCRemarksChildID', visible: false },
            {
                headerText: '', width: 100, commands: [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                    { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                    { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                    { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }]
            },
            {
                field: 'PercentageValue', headerText: 'Percent(%)', width: 100, editType: "numericedit"
                //field: 'Percent', headerText: 'Percent(%)', width: 120, editType: "numericedit", params: { decimals: 0, format: "N", min: 1, validateDecimalOnType: true }, allowEditing: isBlended
            },
            //{
            //    field: 'ComponentID', headerText: 'Component', editType: 'dropdownedit',
            //    edit: new ej2DropdownParams({ dataSource: masterData.FabricComponents, field: "Fiber" })
            //}
            {
                field: 'ComponentID', headerText: 'Component',
                editType: 'dropdownedit',
                valueAccessor: ej2GridDisplayFormatter,
                dataSource: masterData.FabricComponents,
                displayField: "ComponentName", edit: ej2GridDropDownObj({
                })
            },
        ];

        var gridOptions = {
            tableId: tblCreateCompositionId,
            queryString: 'QCRemarksChildID',
            data: data,
            columns: ChildFiberColumns,
            actionBegin: function (args) {
                if (args.requestType === "add") {
                    args.data.QCRemarksChildFiberID = _QCRemarksChildFiberID++;
                    args.data.QCRemarksChildID = qcRemarksChildID;
                }
                else if (args.requestType === "save" && args.action === "edit") {
                    if (!args.data.ComponentID) {
                        toastr.warning("Fabric component is required.");
                        args.cancel = true;
                        return;
                    }
                    else if (!args.data.PercentageValue || args.data.PercentageValue <= 0 || args.data.PercentageValue > 100) {
                        toastr.warning("Composition percent must be greater than 0 and less than or equal 100.");
                        args.cancel = true;
                        return;
                    }
                }
            },
            load: loadChildFibers,
            autofitColumns: false,
            showDefaultToolbar: false,
            allowFiltering: false,
            allowPaging: false,
            toolbar: ['Add'],
            isMultiselect: true,
            editSettings: {
                allowAdding: true,
                allowEditing: true,
                allowDeleting: true,
                mode: "Normal",
                showDeleteConfirmDialog: true
            },
        }

        if ($tblCreateCompositionEl) $tblCreateCompositionEl.destroy();
        $tblCreateCompositionEl = new initEJ2Grid(gridOptions);
    }
    function setTechnicalInfo(objParam) {
        if ((objParam.TechnicalNameID == null || objParam.TechnicalNameID == 0) && typeof objParam.TechnicalName !== "undefined" && objParam.TechnicalName != null && objParam.TechnicalName.length > 0) {
            var obj = masterData.TechnicalNameList.find(x => x.text == objParam.TechnicalName);
            if (obj) {
                objParam.TechnicalNameID = obj.id;
            }
        }

        if (objParam.TechnicalNameID > 0) {
            var obj = masterData.TechnicalNameList.find(x => x.id == objParam.TechnicalNameID);
            if (obj) {
                objParam.TechnicalName = obj.text;
            }
        }
        return objParam;
    }
    function loadChildFibers() {
        this.dataSource = this.parentDetails.parentRowData.YarnQCRemarksChildFibers;
    }
    function loadChildResults() {
        this.dataSource = this.parentDetails.parentRowData.YarnQCRemarksChildResults;
        //if (menuParam == "MRPBAck") {
        //    this.dataSource = this.parentDetails.parentRowData.ChildItems;
        //}
        //else {
        //    this.dataSource = this.parentDetails.parentRowData.Childs;
        //}
    }

    function backToList() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        initMasterTable();
    }

    function resetForm() {
        $formEl.trigger("reset");
        $formEl.find("#QCRemarksMasterID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function resetTableParams() {
        tableParams.offset = 0;
        tableParams.limit = 10;
        tableParams.filter = '';
        tableParams.sort = '';
        tableParams.order = '';
    }

    function getNew(qcReceiveChildID) {
        axios.get(`/api/yarn-qc-remarks/new/${qcReceiveChildID}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                actionBtnHideShow();

                masterData = response.data;
                masterData.QCRemarksDate = formatDateToDefault(masterData.QCRemarksDate);
                masterData.QCReceiveDate = formatDateToDefault(masterData.QCReceiveDate);
                masterData.QCReqDate = formatDateToDefault(masterData.QCReqDate);
                setFormData($formEl, masterData);
                initChildTable(masterData.YarnQCRemarksChilds);
                $formEl.find("#btnSave").show();
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getDetails(qcRemarksChildID) {
        axios.get(`/api/yarn-qc-remarks/${qcRemarksChildID}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                actionBtnHideShow();

                masterData = response.data;
                masterData.QCRemarksDate = formatDateToDefault(masterData.QCRemarksDate);
                masterData.QCReceiveDate = formatDateToDefault(masterData.QCReceiveDate);
                masterData.QCReqDate = formatDateToDefault(masterData.QCReqDate);
                setFormData($formEl, masterData);
                initChildTable(masterData.YarnQCRemarksChilds);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function actionBtnHideShow() {
        $formEl.find(".btnAction").hide();
        switch (status) {
            case statusConstants.PENDING:
                $formEl.find("#btnSave,#btnSaveAndSendForApproval").show();
                break;
            case statusConstants.DRAFT:
                $formEl.find("#btnSave,#btnSaveAndSendForApproval").show();
                break;
            case statusConstants.PROPOSED_FOR_APPROVAL:
                if (isYQCRemarksApproval) {
                    $formEl.find("#btnApprove").show();
                }
                break;
            case statusConstants.APPROVED_DONE:
                if (isYQCRemarksApproval) {
                    $formEl.find("#btnRetest").show();
                }
                break;
            case statusConstants.ReTest:
                if (isYQCRemarks) {
                    $formEl.find("#btnSaveAndSendForApproval,#btnRetestForRequisition").show();
                }
                break;
            //case statusConstants.ReTest:
            //    if (isYQCRemarks) {
            //        $formEl.find("#btnRetest").show();
            //    }
            //    break;
            default:
            // code block
        }
    }

    function hasUnique(list) {
        var hasUnique = true;
        var tempList = [];
        for (var iL = 0; iL < list.length; iL++) {
            var item = list[iL];

            var indexF = tempList.findIndex(x => x.FabricColorID == item.FabricColorID
                && x.TestMethodRefID == item.TestMethodRefID
                && x.PillingGrade == item.PillingGrade);

            if (indexF > -1) {
                hasUnique = false;
                break;
            }

            tempList.push({
                FabricColorID: item.FabricColorID,
                TestMethodRefID: item.TestMethodRefID,
                PillingGrade: item.PillingGrade
            });
        }
        return hasUnique;
    }

    function save(isSendForApproval, isRetest, isRetestForRequisition) {
        var data = formDataToJson($formEl.serializeArray());
        data.YarnQCRemarksChilds = $tblChildEl.getCurrentViewRecords();
        data.IsSendForApproval = isSendForApproval;
        data.ReceiveID = masterData.ReceiveID;
        data.IsRetestForRequisition = getDefaultValueWhenInvalidN(isRetestForRequisition);
        if (isRetest) {
            data.IsRetest = isRetest;
            data.RetestParentQCRemarksMasterID = $formEl.find("#QCRemarksMasterID").val();
        }

        var hasError = false;
        for (var i = 0; i < data.YarnQCRemarksChilds.length; i++) {
            var child = data.YarnQCRemarksChilds[i];

            if (child.YarnQCRemarksChildResults.length == 0) {
                toastr.error("No result found.");
                hasError = true;
                break;
            }

            //if (child.TechnicalNameID == null) {
            //    toastr.error("Select Fabric Technical Name.");
            //    hasError = true;
            //    break;
            //}
            if ($.trim(child.Remarks) == "") {
                toastr.error("Give Remarks.");
                hasError = true;
                break;
            }
            if (child.YarnStatusID == null || child.YarnStatusID == 0) {
                toastr.error("Select Zone.");
                hasError = true;
                break;
            }
            if (child.id == null || child.id == 0) {
                toastr.error("Select Status.");
                hasError = true;
                break;
            }
            //YarnStatusID

            for (var j = 0; j < child.YarnQCRemarksChildResults.length; j++) {
                var childResult = child.YarnQCRemarksChildResults[j];
                if (childResult.ACountNe == null || childResult.ACountNe == 0) {
                    toastr.error("Give ACountNe");
                    hasError = true;
                    break;
                }
                if (childResult.Twist == null || childResult.Twist == 0) {
                    toastr.error("Give Twist");
                    hasError = true;
                    break;
                }
                if (childResult.CSP == null || childResult.CSP == 0) {
                    toastr.error("Give CSP");
                    hasError = true;
                    break;
                }
                if (childResult.FabricColorID == null || childResult.FabricColorID == 0) {
                    toastr.error("Select Fabric Color");
                    hasError = true;
                    break;
                }
                if (childResult.DyeingProcessID == null || childResult.DyeingProcessID == 0) {
                    toastr.error("Select Dyeing Process");
                    hasError = true;
                    break;
                }
                if (childResult.ThickThin == null || childResult.ThickThin == 0) {
                    toastr.error("Select Thick-Thin");
                    hasError = true;
                    break;
                }
                if (childResult.BarreMark == null || childResult.BarreMark == 0) {
                    toastr.error("Select Barre-Mark");
                    hasError = true;
                    break;
                }
                if (childResult.Naps == null || childResult.Naps == 0) {
                    toastr.error("Select Naps");
                    hasError = true;
                    break;
                }
                if (childResult.Hairiness == null || childResult.Hairiness == 0) {
                    toastr.error("Select Hairiness");
                    hasError = true;
                    break;
                }
                if (childResult.WhiteSpecks == null || childResult.WhiteSpecks == 0) {
                    toastr.error("Select WhiteSpecks");
                    hasError = true;
                    break;
                }
                if (childResult.Polypropylyne == null || childResult.Polypropylyne == 0) {
                    toastr.error("Select Polypropylyne");
                    hasError = true;
                    break;
                }
                if (childResult.Contamination == null || childResult.Contamination == 0) {
                    toastr.error("Select Contamination");
                    hasError = true;
                    break;
                }
                if (childResult.TestMethodRefID == null) {
                    childResult.TestMethodRefID = 0;
                }
                //if (childResult.TestMethodRefID == null) {
                //    toastr.error("Give Test Method Ref");
                //    hasError = true;
                //    break;
                //}
                //if (childResult.PillingGrade == null || childResult.PillingGrade == 0) {
                //    toastr.error("Give Pilling Grade");
                //    hasError = true;
                //    break;
                //}
            }
        }

        if (hasError) return false;

        if (!hasUnique(data.YarnQCRemarksChilds[0].YarnQCRemarksChildResults)) {
            toastr.error("Fabric Color Code, Test Method Ref & Pilling Grade must be unique.");
            return false;
        }

        axios.post("/api/yarn-qc-remarks/save", data)
            .then(function (response) {
                toastr.success("Saved successfully.");
                /*
                if (isRetest) {
                    showBootboxAlert("New Assessment No : <b>" + response.data.QCRemarksNo + "</b> created successfully.");
                } else {
                    toastr.success("Saved successfully.");
                }
                */
                backToList();
            })
            .catch(showResponseError);
    }

    function approve() {
        var data = formDataToJson($formEl.serializeArray());
        data.YarnQCRemarksChilds = $tblChildEl.getCurrentViewRecords();
        axios.post("/api/yarn-qc-remarks/approve", data)
            .then(function (response) {
                toastr.success("Approved successfully.");
                backToList();
            })
            .catch(showResponseError);
    }

    function saveComposition() {
        var fibers = $tblCreateCompositionEl.getCurrentViewRecords();
        if (fibers.length > 0) {
            var totalComponents = fibers.map(x => x.ComponentID);
            var uniqueComponents = [...new Set(totalComponents)];
            if (totalComponents.length != uniqueComponents.length) {
                toastr.warning("Duplicate data detected. Please enter unique values for Component.");
                return false;
            }
            var totalPer = 0;
            fibers.map(x => {
                var perValue = parseFloat(x.PercentageValue);
                if (isNaN(perValue)) perValue = 0;
                totalPer += perValue;
            });
            if (totalPer != parseFloat(100)) {
                toastr.warning("Composition percent must be equal 100.");
                return false;
            }
            var indexF = masterData.YarnQCRemarksChilds.findIndex(x => x.QCRemarksChildID == fibers[0].QCRemarksChildID);
            if (indexF > -1) {
                masterData.YarnQCRemarksChilds[indexF].YarnQCRemarksChildFibers = fibers;
            }
        }
        $pageEl.find(`#modal-new-composition-${pageId}`).modal("hide");
    }
    function DeepClone(obj) {
        return JSON.parse(JSON.stringify(obj));
    }
})();