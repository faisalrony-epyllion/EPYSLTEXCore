//const { stat } = require("fs/promises");

(function () {
    var menuId, pageName;
    var toolbarId, pageId, pageIdWithHash;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, tblMasterId, $formEl, $tblChildEl, tblChildId;
    var status = statusConstants.PENDING;
    var isAcknowledge = false;
    var isQCR = false, isQCRApproval = false, isYQCMRSAck = false;
    var tableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    }
    var yarnQCReq;
    var _isRetest = false;
    var selectedargs;

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
        isAcknowledge = convertToBoolean($(pageId).find("#Acknowledge").val());

        var menuParam = $("#" + pageId).find("#txtMenuParam").val();
        if (menuParam == "YQCR") isQCR = true;
        else if (menuParam == "YQCRApproval") isQCRApproval = true;
        else if (menuParam == "YQCMRSAck") isYQCMRSAck = true;

        $toolbarEl.find(".btnTootBar").hide();
        $formEl.find(".btnAction").hide();
        if (isQCR) {
            $toolbarEl.find("#pandingList").show();
            $toolbarEl.find("#btnNoTest").show();
            $toolbarEl.find("#btnList").show();
            $toolbarEl.find("#btnReTestDiagnosticList").show();
            $toolbarEl.find(".divCreate").show();
            $toolbarEl.find("#btnPendingForApproval").show();
            $toolbarEl.find("#btnApprovedList").show();
            $toolbarEl.find("#btnAllList").show();
            $toolbarEl.find("#btnRejectList").show();
            $toolbarEl.find("#btnPendingForAcknowledgeList").show();
            $toolbarEl.find("#btnAcknowledgeList").show();

            $formEl.find("#btnSave").show();
            $formEl.find("#btnSaveAndSend").show();

        } else if (isQCRApproval) {
            $toolbarEl.find("#btnPendingForApproval").show();
            $toolbarEl.find("#btnApprovedList").show();
            $toolbarEl.find("#btnAllList").show();
            $toolbarEl.find("#btnRejectList").show();
            $toolbarEl.find("#btnPendingForAcknowledgeList").show();
            $toolbarEl.find("#btnAcknowledgeList").show();

            $formEl.find("#btnApprove").show();
            $formEl.find("#btnReject").show();

        }
        else if (isYQCMRSAck) {
            $toolbarEl.find("#btnPendingForAcknowledgeList").show();
            $toolbarEl.find("#btnAcknowledgeList").show();
        }
        if (isAcknowledge) {
            $formEl.find("#btnSave").hide();
        }
        $formEl.find("#ReceiveID").on("select2:select select2:unselect", function (e) {
            if (e.params.data.selected) {
                var receiveId = $(this).val();
                getReceiveData(receiveId);
            }
            else {
                initChildTable(yarnQCReq.YarnQCReqChilds);
            }
        })
        $("#QCForId").on("select2:select select2:unselect", function (e) {
            if (e.params.data.selected) {
                yarnQCReq.QCForId = $(this).val();
            }
            else {
                yarnQCReq.QCForId = 0;
            }
        })
        $toolbarEl.find("#btnNew").on("click", getNew);

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save(false, false, false, false, false);
        });
        $formEl.find("#btnSaveAndSend").click(function (e) {
            e.preventDefault();
            save(true, false, false, false, false);
        });
        $formEl.find("#btnApprove").click(function (e) {
            e.preventDefault();
            save(false, true, false, false, false);
        });
        $formEl.find("#btnAcknowledge").click(function (e) {
            e.preventDefault();
            save(false, false, false, false, true);
        });
        $formEl.find("#btnReject").click(function (e) {
            e.preventDefault();
            $formEl.find(".rejectReason").show();
            var rejectReason = $formEl.find("#RejectReason").val();
            if (rejectReason == null || $.trim(rejectReason).length == 0) {
                toastr.error("Give reject reason.");
                $formEl.find("#RejectReason").focus();
                return false;
            }
            save(false, false, true, false);
        });
        $formEl.find("#btnRevise").click(function (e) {
            e.preventDefault();
            save(false, false, false, true, false);
        });

        $toolbarEl.find(".divCreate").hide();
        $toolbarEl.find("#btnList").on("click", function (e) {
            status = statusConstants.REVISE;
            $toolbarEl.find(".divCreate").hide();
            $formEl.find("#btnApprove").hide();
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            initMasterTable();
        });
        $toolbarEl.find("#btnReTestDiagnosticList").on("click", function (e) {
            status = statusConstants.ReTest;
            $toolbarEl.find(".divCreate").hide();
            $formEl.find("#btnApprove").hide();
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            initMasterTable();
        });
        $toolbarEl.find("#pandingList").on("click", function (e) {
            status = statusConstants.PENDING;
            $toolbarEl.find(".divCreate").show();
            $formEl.find("#btnApprove").hide();
            $formEl.find("#btnSave").show();
            $formEl.find("#btnSaveAndSend").show();
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            initMasterTable();
        });
        $toolbarEl.find("#btnNoTest").on("click", function (e) {
            status = statusConstants.HOLD;
            $toolbarEl.find(".divCreate").show();
            $formEl.find("#btnApprove").hide();
            $formEl.find("#btnSave").show();
            $formEl.find("#btnSaveAndSend").show();
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            initMasterTable();
        });
        $toolbarEl.find("#btnCreate").click(function () {
            createNew();
        });
        $toolbarEl.find("#btnPendingForApproval").on("click", function (e) {
            status = statusConstants.PROPOSED_FOR_APPROVAL;
            $toolbarEl.find(".divCreate").show();
            if (isQCR && (status == statusConstants.PROPOSED_FOR_APPROVAL || status == statusConstants.APPROVED)) {
                $formEl.find("#btnSave").hide();
                $formEl.find("#btnSaveAndSend").hide();

                $toolbarEl.find(".divCreate").hide();
            }
            if (isQCRApproval) {
                $toolbarEl.find(".divCreate").hide();
            }
            $formEl.find("#btnApprove").show();
            $formEl.find("#btnReject").show();

            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            initMasterTable();
        });
        $toolbarEl.find("#btnApprovedList").on("click", function (e) {
            status = statusConstants.APPROVED;
            $toolbarEl.find(".divCreate").hide();
            if (isQCR && status == statusConstants.PROPOSED_FOR_APPROVAL) {
                $formEl.find("#btnSave").hide();
                $formEl.find("#btnSaveAndSend").hide();
            }
            if (status == statusConstants.APPROVED) {
                $formEl.find("#btnApprove").hide();
                $formEl.find("#btnReject").hide();
            }
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            initMasterTable();
        });
        $toolbarEl.find("#btnPendingForAcknowledgeList").on("click", function (e) {
            status = statusConstants.PROPOSED_FOR_ACKNOWLEDGE;
            $toolbarEl.find(".divCreate").hide();
            $formEl.find("#btnSave").hide();
            $formEl.find("#btnSaveAndSend").hide();
            $formEl.find("#btnApprove").hide();
            $formEl.find("#btnReject").hide();
            $formEl.find("#btnRevise").hide();
            if (isQCR) {
                $toolbarEl.find(".divCreate").show();
            }
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            initMasterTable();
        });
        $toolbarEl.find("#btnAcknowledgeList").on("click", function (e) {
            status = statusConstants.ACKNOWLEDGE;
            $toolbarEl.find(".divCreate").hide();
            if (isQCR) {
                $toolbarEl.find(".divCreate").show();
            }
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            initMasterTable();
        });
        $toolbarEl.find("#btnRejectList").on("click", function (e) {
            status = statusConstants.REJECT;
            if (isQCR && status == statusConstants.REJECT) {
                $formEl.find("#btnSave").hide();
                $formEl.find("#btnSaveAndSend").hide();
            }
            if (status == statusConstants.APPROVED || status == statusConstants.REJECT) {
                $formEl.find("#btnApprove").hide();
                $formEl.find("#btnReject").hide();
            }
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            initMasterTable();
        });

        //$toolbarEl.find("#btnAllList").on("click", function (e) {
        //    status = statusConstants.ALL;
        //    $toolbarEl.find(".divCreate").hide();
        //    if (isQCR && status == statusConstants.PROPOSED_FOR_APPROVAL) {
        //        $formEl.find("#btnSave").hide();
        //        $formEl.find("#btnSaveAndSend").hide();
        //    }
        //    if (status == statusConstants.ALL) {
        //        $formEl.find("#btnSave").hide();
        //        $formEl.find("#btnSaveAndSend").hide();
        //    }
        //    e.preventDefault();
        //    toggleActiveToolbarBtn(this, $toolbarEl);
        //    resetTableParams();
        //    initMasterTable();
        //});

        //$formEl.find("#btnCancel").on("click", backToListWithoutRefresh);
        $formEl.find("#btnCancel").on("click", backToList);
        if (isQCR) {
            $toolbarEl.find("#pandingList").click();
        } else if (isQCRApproval) {
            $toolbarEl.find("#btnPendingForApproval").click();
        } else if (isYQCMRSAck) {
            $toolbarEl.find("#btnPendingForAcknowledgeList").click();
        }

        pageIdWithHash = "#" + pageId;
        $(pageIdWithHash).find("#btnNoTestRemarkConfirm").click(function () {
            noTestStatusUpdate();
        });
        $(pageIdWithHash).find("#btnNoTestTagYes").click(function () {
            $(pageIdWithHash).find("#noTestTagModal").modal('hide');

            var ReceiveChildID = selectedargs.rowData.ReceiveChildID;
            var finder = new commonFinder({
                title: "Select Tagging",
                pageId: pageId,
                apiEndPoint: `/api/yarn-receive/getPrevReq/${selectedargs.rowData.LotNo}/${selectedargs.rowData.ItemMasterID}`,
                fields: "YarnDetail,QCReqNo,QCReqRemarks",
                headerTexts: "Yarn Details, QC Req No,Test Result",
                isMultiselect: false,
                autofitColumns: true,
                primaryKeyColumn: "ReceiveChildID",
                modalSize: "modal-md",
                top: "2px",
                onSelect: function (record) {
                    finder.hideModal();

                    var result = confirm("Do you want to Tag with QC Req No: " + record.rowData.QCReqNo + ", Result: " + record.rowData.QCReqRemarks + "?");
                    if (result) {
                        var data = {};
                        data.ChildID = ReceiveChildID;
                        data.TagYarnReceiveChildID = record.rowData.ReceiveChildID;

                        axios.post('/api/yarn-receive/update-Tag', data)
                            .then(function () {
                                toastr.success("Tagged successfully.");
                                backToList();
                            })
                            .catch(function (error) {
                                toastr.error(error.response.data.Message);
                            });
                    } else {

                    }

                }
            });
            finder.showModal();

        });
        $(pageIdWithHash).find("#btnNoTestTagNo").click(function () {
            $(pageIdWithHash).find("#noTestTagModal").modal('hide');
            var timer = setInterval(function () {
                $(pageIdWithHash).find("#noTestModal").modal('show');
                clearInterval(timer);
            }, 500);
        });
    });

    //if (pageName == "YarnPOApprovalV2") {
    //    commands = [
    //        { type: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
    //        { type: 'Approve', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-check' } },
    //        { type: 'Reject', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-ban' } },
    //        { type: 'Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
    //    ]
    //}

    //commands = [
    //    { type: 'Add', buttonOption: { cssClass: 'e-flat', iconCss: 'e-add e-icons' } }
    //]

    function initMasterTable() {
        var commandList = [],
            widthValue = 60;
        if (status == statusConstants.PENDING) {
            widthValue = 60;
            commandList = [{
                type: 'NoTest', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-ban' }
            }];
        }
        //else if (status == statusConstants.ALL) {
        //    widthValue = 40;
        //    commandList = [
        //        {
        //            type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' }
        //        }
        //    ];
        //}
        else if ((status == statusConstants.PROPOSED_FOR_APPROVAL || status == statusConstants.APPROVED || status == statusConstants.REVISE || status == statusConstants.REJECT) && isQCR) {
            widthValue = 100;
            commandList = [
                {
                    type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' }
                },
                {
                    type: 'ViewReport', title: 'View Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' }
                }
            ];
        }

        else if (status != statusConstants.PENDING && status != statusConstants.HOLD && status != statusConstants.ReTest && !isQCRApproval) {
            widthValue = 120;
            commandList = [
                {
                    type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' }
                },
                {
                    type: 'ReTest', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-recycle' }
                },
                {
                    type: 'ViewReport', title: 'View Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' }
                }
            ];
        }
        else if (isQCRApproval) {
            widthValue = 100;
            commandList = [
                {
                    type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' }
                },
                {
                    type: 'ViewReport', title: 'View Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' }
                }
            ];
        }
        else if (status == statusConstants.ReTest) {
            widthValue = 60;
            commandList = [
                {
                    type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' }
                }
            ];
        }

        var columns = [
            {
                headerText: '',
                width: widthValue,
                commands: commandList,
                textAlign: 'Center'
            },
            {
                field: 'Status', headerText: 'Status', width: 60, allowEditing: false, visible: status != statusConstants.PENDING && status != statusConstants.HOLD
            },
            { field: 'HasPrevQCReq', headerText: 'Has Prev QC Req?', textAlign: 'Center', width: 100, allowEditing: false, displayAsCheckBox: true, textAlign: 'Center' },
            {
                field: 'QCReqNo', headerText: 'Req No', width: 160, allowEditing: false, visible: status != statusConstants.PENDING && status != statusConstants.HOLD
            },
            {
                field: 'QCReqDate', headerText: 'Req Date', width: 80, allowEditing: false, textAlign: 'Center', type: 'date', format: _ch_date_format_1, visible: status != statusConstants.PENDING && status != statusConstants.HOLD
            },
            {
                field: 'QCReqByUser', headerText: 'Req By', width: 100, allowEditing: false, textAlign: 'Center', visible: status != statusConstants.PENDING && status != statusConstants.HOLD
            },
            {
                field: 'ReceiveDate', headerText: 'Receive Date', width: 80, allowEditing: false, textAlign: 'Center', type: 'date', format: _ch_date_format_1, visible: status == statusConstants.PENDING || status == statusConstants.HOLD
            },
            {
                field: 'RackBinDate', headerText: 'Rack Bin Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1
            },
            {
                field: 'ImportCategory', headerText: 'Import Category', width: 80, allowEditing: false, visible: status == statusConstants.PENDING || status == statusConstants.HOLD
            },
            {
                field: 'SupplierName', headerText: 'Supplier', width: 80, allowEditing: false, visible: status == statusConstants.PENDING || status == statusConstants.HOLD
            },
            {
                field: 'Spinner', headerText: 'Spinner', width: 80, allowEditing: false, visible: status == statusConstants.PENDING || status == statusConstants.HOLD
            },
            {
                field: 'POFor', headerText: 'Purchase For', width: 80, allowEditing: false, visible: status == statusConstants.PENDING || status == statusConstants.HOLD
            },
            {
                field: 'BuyerName', headerText: 'Buyer Name', width: 80, allowEditing: false, visible: status == statusConstants.PENDING || status == statusConstants.HOLD
            },
            {
                field: 'EWO', headerText: 'Allocated Order', width: 80, allowEditing: false, visible: status == statusConstants.PENDING || status == statusConstants.HOLD
            },
            //{
            //    field: 'ChallanLot', headerText: 'Challan Lot', width: 80, allowEditing: false, visible: status == statusConstants.PENDING || status == statusConstants.HOLD
            //},
            {
                field: 'LotNo', headerText: 'Physical Lot', width: 80, allowEditing: false, visible: status == statusConstants.PENDING || status == statusConstants.HOLD
            },
            {
                field: 'PhysicalCount', headerText: 'Physical Count', width: 80, allowEditing: false, visible: status == statusConstants.PENDING || status == statusConstants.HOLD
            },
            {
                field: 'ChallanLot', headerText: 'Challan Lot', width: 80, allowEditing: false, visible: status == statusConstants.PENDING || status == statusConstants.HOLD
            },
            {
                field: 'Yarndetail', headerText: 'PO Yarn details', width: 250, allowEditing: false, visible: status == statusConstants.PENDING || status == statusConstants.HOLD
            },
            {
                field: 'ReceivedQtyInKg', headerText: 'Received Qty in Kg', width: 80, allowEditing: false, visible: status == statusConstants.PENDING || status == statusConstants.HOLD
            },
            {
                field: 'ReceivingPlace', headerText: 'Receiving Place', width: 80, allowEditing: false, visible: status == statusConstants.PENDING || status == statusConstants.HOLD
            },
            {
                field: 'RackNo', headerText: 'Rack No.', width: 80, allowEditing: false, visible: status == statusConstants.PENDING || status == statusConstants.HOLD
            },
            {
                field: 'ShadeCode', headerText: 'Shade Code', width: 80, allowEditing: false, visible: status == statusConstants.PENDING || status == statusConstants.HOLD
            },
            {
                field: 'ChallanDate', headerText: 'Challan Date', width: 80, allowEditing: false, textAlign: 'Center', type: 'date', format: _ch_date_format_1
            },
            {
                field: 'ChallanNo', headerText: 'Challan No', width: 80, allowEditing: false
            },
            {
                field: 'ReceiveNo', headerText: 'Receive No', width: 100, allowEditing: false, textAlign: 'Center'
            },
            /*
            { field: 'Segment1ValueDesc', headerText: 'Composition', allowEditing: false, width: 120, visible: status != statusConstants.PENDING && status != statusConstants.HOLD },
            { field: 'Segment2ValueDesc', headerText: 'Yarn Type', allowEditing: false, allowEditing: false, visible: status != statusConstants.PENDING && status != statusConstants.HOLD },
            { field: 'Segment3ValueDesc', headerText: 'Process', allowEditing: false, allowEditing: false, visible: status != statusConstants.PENDING && status != statusConstants.HOLD },
            { field: 'Segment4ValueDesc', headerText: 'Sub process', allowEditing: false, allowEditing: false, visible: status != statusConstants.PENDING && status != statusConstants.HOLD },
            { field: 'Segment5ValueDesc', headerText: 'Quality Parameter', allowEditing: false, width: 84, visible: status != statusConstants.PENDING && status != statusConstants.HOLD },
            { field: 'Segment6ValueDesc', headerText: 'Yarn Count', allowEditing: false, allowEditing: false, visible: status != statusConstants.PENDING && status != statusConstants.HOLD },
            { field: 'Segment7ValueDesc', headerText: 'No of Ply', allowEditing: false, width: 80, visible: status != statusConstants.PENDING && status != statusConstants.HOLD },
            { field: 'Segment6ValueDesc', headerText: 'Physical Count', allowEditing: false, allowEditing: false, visible: status != statusConstants.PENDING && status != statusConstants.HOLD },

            */
            {
                field: 'QCReqFor', headerText: 'Req For', width: 80, allowEditing: false, visible: status != statusConstants.PENDING && status != statusConstants.HOLD
            },
            //{
            //    field: 'RCompany', headerText: 'Company Name', allowEditing: false, visible: status == statusConstants.PENDING || status == statusConstants.HOLD
            //},
            {
                field: 'IsApproveStr', headerText: 'Sent for Req', width: 80, allowEditing: false, visible: status != statusConstants.PENDING && status != statusConstants.HOLD
            },
            {
                field: 'IsAcknowledgeStr', headerText: 'Acknowledged', width: 80, allowEditing: false, visible: status != statusConstants.PENDING && status != statusConstants.HOLD
            },
            {
                field: 'ItemMasterID', headerText: 'Item Master', width: 80, allowEditing: false, visible: false
            },
            {
                field: 'NoTestRemarks', headerText: 'Note', width: 80, allowEditing: false, visible: status == statusConstants.HOLD
            },
            {
                field: 'RejectReason', headerText: 'Reject Reason', width: 80, allowEditing: false, visible: status == statusConstants.REJECT
            },
            {
                field: 'ParentQCRemarksNo', headerText: 'From Assessment No', width: 80, allowEditing: false, visible: status == statusConstants.ReTest
            }
        ];

        if (status == statusConstants.PENDING || status == statusConstants.HOLD) {
            columns.splice(0, 0, {
                type: 'checkbox', width: 30, headerText: 'Select'
            });
        }
        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            allowFiltering: true,
            allowSorting: true,
            apiEndPoint: `/api/yarn-qc-requisition/list?status=${status}`,
            columns: columns,
            commandClick: handleCommands
        });
    }
    function handleCommands(args) {
        debugger;
        _isRetest = false;
        selectedargs = args;
        if (args.commandColumn.type == "NoTest") {

            $(pageIdWithHash).find("#txtNoTestRemark").val("");
            $(pageIdWithHash).find("#txtChildID").val(args.rowData.ReceiveChildID);
            if (args.rowData.HasPrevQCReq == true) {
                $(pageIdWithHash).find("#noTestTagModal").modal('show');
            } else {
                $(pageIdWithHash).find("#noTestModal").modal('show');
            }

            //$('#myModal').modal('toggle');
            //$('#myModal').modal('show');
            //$('#myModal').modal('hide');
        }
        else if (args.commandColumn.type == 'ReTest') {
            _isRetest = true;
            getRetest(args.rowData.QCReqMasterID);
        }
        else if (args.commandColumn.type == 'ViewReport') {
            window.open(`/reports/InlinePdfView?ReportName=DailyYarnQCRequisitionSlip.rdl&QCReqMasterID=${args.rowData.QCReqMasterID}`, '_blank');
        }
        else {
            if (status == statusConstants.ReTest) {
                if (!args.rowData.IsRetestForRequisition) {
                    _isRetest = true;
                }
                getDetailsForRetest(args.rowData.QCReqMasterID, args.rowData.QCRemarksChildID);
            }
            else if (status != statusConstants.PENDING) {
                getDetails(args.rowData.QCReqMasterID, args.rowData.ItemMasterID);
            } else {
                getDetails(args.rowData.ReceiveID, args.rowData.ItemMasterID);
            }
            if (isQCR && status != statusConstants.PROPOSED_FOR_APPROVAL && status != statusConstants.APPROVED && status != statusConstants.ALL && status != statusConstants.PROPOSED_FOR_ACKNOWLEDGE && status != statusConstants.ACKNOWLEDGE) {
                $formEl.find("#btnSave,#btnSaveAndSend").show();
            }
            if (isYQCMRSAck) {
                if (status == statusConstants.PROPOSED_FOR_ACKNOWLEDGE) {
                    $formEl.find("#btnAcknowledge").show();
                }
                else {
                    $formEl.find("#btnSave,#btnSaveAndSend,#btnAcknowledge").hide();
                }
            }
            else {
                $formEl.find("#btnAcknowledge").hide();
            }
        }
    }

    var machineTypeElem;
    var machineTypeObj;
    var technicalNameElem;
    var technicalNameObj;
    function initChildTable(records) {
        if ($tblChildEl) $tblChildEl.destroy();

        $tblChildEl = new ej.grids.Grid({
            dataSource: records,
            autofitColumns: false,
            allowSorting: true,
            allowPaging: false,
            allowFiltering: false,
            allowResizing: true,
            editSettings: { allowAdding: true, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
            columns: [
                {
                    headerText: 'Command', width: 120, textAlign: 'Center', commands: [
                        { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                        { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                        { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                        { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }
                    ]
                },
                { field: 'QCReqChildID', isPrimaryKey: true, width: 20, allowEditing: false, visible: false },
                { field: 'ReceiveChildID', width: 20, allowEditing: false, visible: false },
                {
                    field: 'MachineType', headerText: 'Machine Type', width: 250,
                    valueAccessor: ej2GridDisplayFormatterV2, edit: {
                        create: function () {
                            machineTypeElem = document.createElement('input');
                            return machineTypeElem;
                        },
                        read: function () {
                            return machineTypeObj.text;
                        },
                        destroy: function () {
                            machineTypeObj.destroy();
                        },
                        write: function (e) {
                            machineTypeObj = new ej.dropdowns.DropDownList({
                                dataSource: yarnQCReq.MCTypeForFabricList,
                                fields: { value: 'id', text: 'text' },
                                change: function (f) {
                                    technicalNameObj.enabled = true;
                                    var tempQuery = new ej.data.Query().where('additionalValue', 'equal', machineTypeObj.value);
                                    technicalNameObj.query = tempQuery;
                                    technicalNameObj.text = null;
                                    technicalNameObj.dataBind();

                                    e.rowData.MachineTypeId = f.itemData.id;
                                    e.rowData.MachineType = f.itemData.text;
                                    e.rowData.KTypeId = f.itemData.desc;
                                },
                                placeholder: 'Select M/C Type',
                                floatLabelType: 'Never'
                            });
                            machineTypeObj.appendTo(machineTypeElem);
                        }
                    }
                },
                {
                    field: 'TechnicalName', headerText: 'Technical Name', width: 250, displayField: "TechnicalName", valueAccessor: ej2GridDisplayFormatterV2, edit: {
                        create: function () {
                            technicalNameElem = document.createElement('input');
                            return technicalNameElem;
                        },
                        read: function () {
                            return technicalNameObj.text;
                        },
                        destroy: function () {
                            technicalNameObj.destroy();
                        },
                        write: function (e) {
                            technicalNameObj = new ej.dropdowns.DropDownList({
                                dataSource: yarnQCReq.TechnicalNameList,//.filter(x => x.id == _machineTypeId),
                                fields: { value: 'id', text: 'text' },
                                enabled: false,
                                placeholder: 'Select Technical Name',
                                floatLabelType: 'Never',
                                change: function (f) {
                                    if (!f.isInteracted || !f.itemData) return false;
                                    e.rowData.TechnicalTime = parseInt(f.itemData.desc);
                                    e.rowData.TechnicalNameId = f.itemData.id;
                                    e.rowData.TechnicalName = f.itemData.text;
                                }
                            });
                            technicalNameObj.appendTo(technicalNameElem);
                        }
                    }
                },
                {
                    field: 'BuyerID', headerText: 'Buyer',
                    allowEditing: true,
                    width: 250,
                    valueAccessor: ej2GridDisplayFormatter,
                    dataSource: yarnQCReq.BuyerList,
                    displayField: "text",
                    edit: ej2GridDropDownObj({
                    })
                },
                { field: 'ReceiveNo', headerText: 'Receive No', width: 120, allowEditing: false },
                { field: 'ReceiveDate', headerText: 'Receive Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, allowEditing: false },
                {
                    field: 'ImportCategory', headerText: 'Import Category', allowEditing: false
                },
                {
                    field: 'POFor', headerText: 'Purchase For', allowEditing: false
                },
                {
                    field: 'ChallanLot', headerText: 'Challan Lot', allowEditing: false
                },
                {
                    field: 'LotNo', headerText: 'Physical Lot', allowEditing: false
                },
                {
                    field: 'PhysicalCount', headerText: 'Physical Count', allowEditing: false
                },
                {
                    field: 'YarnDetail', headerText: 'PO Yarn details', allowEditing: false
                },
                { field: 'SupplierName', headerText: 'Supplier', width: 100, allowEditing: false },
                { field: 'Spinner', headerText: 'Spinner', width: 100, allowEditing: false },
                /*
                { field: 'Segment1ValueDesc', width: 100, headerText: 'Composition', allowEditing: false },
                { field: 'Segment2ValueDesc', width: 100, headerText: 'Yarn Type', allowEditing: false },
                { field: 'Segment3ValueDesc', width: 100, headerText: 'Process', allowEditing: false },
                { field: 'Segment4ValueDesc', width: 100, headerText: 'Sub process', allowEditing: false },
                { field: 'Segment5ValueDesc', width: 100, headerText: 'Quality Parameter', allowEditing: false },
                { field: 'Segment6ValueDesc', width: 100, headerText: 'Yarn Count', allowEditing: false },
                { field: 'Segment7ValueDesc', width: 100, headerText: 'No of Ply', allowEditing: false },
                { field: 'ShadeCode', headerText: 'Shade Code', width: 100, allowEditing: false },
                */
                { field: 'DisplayUnitDesc', headerText: 'Uom', allowEditing: false, width: 100, textAlign: 'Center' },
                { field: 'NoOfCartoon', headerText: 'No Of Cartoon', allowEditing: false },
                { field: 'NoOfCone', headerText: 'No Of Cone', allowEditing: false, width: 120 },
                { field: 'ReceiveQty', headerText: 'Receive Qty(KG)', allowEditing: false},
                { field: 'ReqBagPcs', headerText: 'Req Qty Bag/Carton(Pcs)' },
                { field: 'ReqCone', headerText: 'Req Qty Cone(Pcs)' },
                { field: 'ReqQty', headerText: 'Req Qty(KG)', width: 130 },
                { field: 'HasPrevQCReq', headerText: 'Has Prev QC Req?', textAlign: 'Center', allowEditing: false, displayAsCheckBox: true, textAlign: 'Center' },
                {
                    field: 'TagWithPrevReq', headerText: 'Tag With Prev Req', allowEditing: false, textAlign: 'center', valueAccessor: diplayPlanningCriteria
                },
                { field: 'QCReqRemarks', headerText: 'Remarks', width: 120 }
            ],
            actionBegin: function (args) {

                if (args.requestType === 'beginEdit') {

                }
                else if (args.requestType === "add") {

                }
                else if (args.requestType === "save") {
                    var indexF = yarnQCReq.YarnQCReqChilds.findIndex(x => x.QCReqChildID == args.rowData.QCReqChildID);
                    if (indexF > -1) {
                        yarnQCReq.YarnQCReqChilds[indexF].MachineTypeId = args.rowData.MachineTypeId;
                        yarnQCReq.YarnQCReqChilds[indexF].TechnicalNameId = args.rowData.TechnicalNameId;
                        yarnQCReq.YarnQCReqChilds[indexF].MachineType = args.rowData.MachineType;
                        yarnQCReq.YarnQCReqChilds[indexF].TechnicalName = args.rowData.TechnicalName;

                        yarnQCReq.YarnQCReqChilds[indexF].BuyerID = args.data.BuyerID;
                        yarnQCReq.YarnQCReqChilds[indexF].ReqQty = args.data.ReqQty;
                        yarnQCReq.YarnQCReqChilds[indexF].ReqBagPcs = args.data.ReqBagPcs;
                        yarnQCReq.YarnQCReqChilds[indexF].ReqCone = args.data.ReqCone;

                        args.data.MachineTypeId = args.rowData.MachineTypeId;
                        args.data.TechnicalNameId = args.rowData.TechnicalNameId;

                        args.data.MachineType = args.rowData.MachineType;
                        args.data.TechnicalName = args.rowData.TechnicalName;

                        $tblChildEl.updateRow(args.rowIndex, args.data);
                    }
                }
                else if (args.requestType === "delete") {

                }
            },
            recordClick: function (args) {
                var ReceiveChildID = args.rowData.ReceiveChildID;
                if (args.column && args.column.field == "TagWithPrevReq") {

                    var finder = new commonFinder({
                        title: "Select Tagging",
                        pageId: pageId,
                        apiEndPoint: `/api/yarn-receive/getPrevReq/${args.rowData.LotNo}/${args.rowData.ItemMasterID}`,
                        fields: "YarnDetail,QCReqNo,QCReqRemarks",
                        headerTexts: "Yarn Details, QC Req No,Test Result",
                        isMultiselect: false,
                        autofitColumns: true,
                        primaryKeyColumn: "ReceiveChildID",
                        modalSize: "modal-md",
                        top: "2px",
                        onSelect: function (record) {
                            finder.hideModal();
                            var data = {};
                            data.ChildID = ReceiveChildID;
                            data.TagYarnReceiveChildID = record.rowData.ReceiveChildID;

                            var result = confirm("Do you want to Tag with QC Req No: " + record.rowData.QCReqNo + ", Result: " + record.rowData.QCReqRemarks + "?");
                            if (result) {
                                axios.post('/api/yarn-receive/update-Tag', data)
                                    .then(function () {
                                        toastr.success("Successfully tagged.");

                                        records = records.filter(x => x.ReceiveChildID != ReceiveChildID);
                                        initChildTable(records);
                                    })
                                    .catch(function (error) {
                                        toastr.error(error.response.data.Message);
                                    });
                            } else {

                            }

                        }
                    });
                    finder.showModal();

                }
            }
        });
        $tblChildEl.refreshColumns;
        $tblChildEl.appendTo(tblChildId);

    }
    /*
    function initChildTable(records) {
        if ($tblChildEl) $tblChildEl.destroy();

        $tblChildEl = new initEJ2Grid({
            tableId: tblChildId,
            data: records,
            autofitColumns: false,
            allowSorting: true,
            allowPaging: false,
            allowFiltering: false,
            editSettings: { allowAdding: true, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
            columns: [
                {
                    headerText: 'Command', width: 100, textAlign: 'Center', commands: [
                        { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                        { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                        { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                        { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }
                    ]
                },
                { field: 'QCReqChildID', isPrimaryKey: true, width: 20, allowEditing: false, visible: false },
                { field: 'ReceiveChildID', width: 20, allowEditing: false, visible: false },
                {
                    field: 'MachineType', headerText: 'Machine Type', width: 250,
                    valueAccessor: ej2GridDisplayFormatterV2, edit: {
                        create: function () {
                            machineTypeElem = document.createElement('input');
                            return machineTypeElem;
                        },
                        read: function () {
                            return machineTypeObj.text;
                        },
                        destroy: function () {
                            machineTypeObj.destroy();
                        },
                        write: function (e) {
                            machineTypeObj = new ej.dropdowns.DropDownList({
                                dataSource: yarnQCReq.MCTypeForFabricList,
                                fields: { value: 'id', text: 'text' },
                                change: function (f) {
                                    technicalNameObj.enabled = true;
                                    var tempQuery = new ej.data.Query().where('additionalValue', 'equal', machineTypeObj.value);
                                    technicalNameObj.query = tempQuery;
                                    technicalNameObj.text = null;
                                    technicalNameObj.dataBind();

                                    e.rowData.MachineTypeId = f.itemData.id;
                                    e.rowData.MachineType = f.itemData.text;
                                    e.rowData.KTypeId = f.itemData.desc;
                                },
                                placeholder: 'Select M/C Type',
                                floatLabelType: 'Never'
                            });
                            machineTypeObj.appendTo(machineTypeElem);
                        }
                    }
                },
                {
                    field: 'TechnicalName', headerText: 'Technical Name', width: 250, displayField: "TechnicalName", valueAccessor: ej2GridDisplayFormatterV2, edit: {
                        create: function () {
                            technicalNameElem = document.createElement('input');
                            return technicalNameElem;
                        },
                        read: function () {
                            return technicalNameObj.text;
                        },
                        destroy: function () {
                            technicalNameObj.destroy();
                        },
                        write: function (e) {
                            technicalNameObj = new ej.dropdowns.DropDownList({
                                dataSource: yarnQCReq.TechnicalNameList,//.filter(x => x.id == _machineTypeId),
                                fields: { value: 'id', text: 'text' },
                                enabled: false,
                                placeholder: 'Select Technical Name',
                                floatLabelType: 'Never',
                                change: function (f) {
                                    if (!f.isInteracted || !f.itemData) return false;
                                    e.rowData.TechnicalTime = parseInt(f.itemData.desc);
                                    e.rowData.TechnicalNameId = f.itemData.id;
                                    e.rowData.TechnicalName = f.itemData.text;
                                    //$tblChildEl.updateRow(e.row.rowIndex, e.rowData);
                                }
                            });
                            technicalNameObj.appendTo(technicalNameElem);
                        }
                    }
                },
                {
                    field: 'BuyerID', headerText: 'Buyer',
                    allowEditing: true,
                    width: 250,
                    valueAccessor: ej2GridDisplayFormatter,
                    dataSource: yarnQCReq.BuyerList,
                    displayField: "text",
                    edit: ej2GridDropDownObj({
                    })
                },
                { field: 'ReceiveNo', headerText: 'Receive No', width: 120, allowEditing: false },
                { field: 'ReceiveDate', headerText: 'Receive Date', width: 100, textAlign: 'Center', type: 'date', format: _ch_date_format_1, allowEditing: false },
                {
                    field: 'ImportCategory', headerText: 'Import Category', width: 100, allowEditing: false
                },
                {
                    field: 'POFor', headerText: 'Purchase For', width: 100, allowEditing: false
                },
                {
                    field: 'ChallanLot', headerText: 'Challan Lot', allowEditing: false
                },
                {
                    field: 'LotNo', headerText: 'Physical Lot', allowEditing: false
                },
                {
                    field: 'PhysicalCount', headerText: 'Physical Count', width: 100, allowEditing: false
                },
                {
                    field: 'YarnDetail', headerText: 'PO Yarn details', allowEditing: false
                },
                { field: 'SupplierName', headerText: 'Supplier', width: 100, allowEditing: false },
                { field: 'Spinner', headerText: 'Spinner', width: 100, allowEditing: false },
                
                //{ field: 'Segment1ValueDesc', width: 100, headerText: 'Composition', allowEditing: false },
                //{ field: 'Segment2ValueDesc', width: 100, headerText: 'Yarn Type', allowEditing: false },
                //{ field: 'Segment3ValueDesc', width: 100, headerText: 'Process', allowEditing: false },
                //{ field: 'Segment4ValueDesc', width: 100, headerText: 'Sub process', allowEditing: false },
                //{ field: 'Segment5ValueDesc', width: 100, headerText: 'Quality Parameter', allowEditing: false },
                //{ field: 'Segment6ValueDesc', width: 100, headerText: 'Yarn Count', allowEditing: false },
                //{ field: 'Segment7ValueDesc', width: 100, headerText: 'No of Ply', allowEditing: false },
                //{ field: 'ShadeCode', headerText: 'Shade Code', width: 100, allowEditing: false },
                
                { field: 'DisplayUnitDesc', headerText: 'Uom', allowEditing: false, width: 100, textAlign: 'Center' },
                { field: 'NoOfCartoon', headerText: 'No Of Cartoon', allowEditing: false, width: 100 },
                { field: 'NoOfCone', headerText: 'No Of Cone', allowEditing: false, width: 100 },
                { field: 'ReceiveQty', headerText: 'Receive Qty(KG)', allowEditing: false, width: 100 },
                { field: 'ReqBagPcs', headerText: 'Req Qty Bag/Carton(Pcs)', width: 120 },
                { field: 'ReqCone', headerText: 'Req Qty Cone(Pcs)', width: 100 },
                { field: 'ReqQty', headerText: 'Req Qty(KG)', width: 100 }, 
                { field: 'HasPrevQCReq', headerText: 'Has Prev QC Req?', textAlign: 'Center', width: 100,  allowEditing: false, displayAsCheckBox: true, textAlign: 'Center' },
                {
                    field: 'TagWithPrevReq', headerText: 'Tag',  allowEditing: false, textAlign: 'center', width: 85, valueAccessor: diplayPlanningCriteria
                },
                { field: 'QCReqRemarks', headerText: 'Remarks', width: 100 }
            ],
            actionBegin: function (args) {
                
                if (args.requestType === 'beginEdit') {

                }
                else if (args.requestType === "add") {

                }
                else if (args.requestType === "save") {
                    var indexF = yarnQCReq.YarnQCReqChilds.findIndex(x => x.QCReqChildID == args.rowData.QCReqChildID);
                    if (indexF > -1) {
                        yarnQCReq.YarnQCReqChilds[indexF].MachineTypeId = args.rowData.MachineTypeId;
                        yarnQCReq.YarnQCReqChilds[indexF].TechnicalNameId = args.rowData.TechnicalNameId;
                        yarnQCReq.YarnQCReqChilds[indexF].MachineType = args.rowData.MachineType;
                        yarnQCReq.YarnQCReqChilds[indexF].TechnicalName = args.rowData.TechnicalName;

                        yarnQCReq.YarnQCReqChilds[indexF].BuyerID = args.data.BuyerID;
                        yarnQCReq.YarnQCReqChilds[indexF].ReqQty = args.data.ReqQty;
                        yarnQCReq.YarnQCReqChilds[indexF].ReqBagPcs = args.data.ReqBagPcs;
                        yarnQCReq.YarnQCReqChilds[indexF].ReqCone = args.data.ReqCone;

                        args.data.MachineTypeId = args.rowData.MachineTypeId;
                        args.data.TechnicalNameId = args.rowData.TechnicalNameId;

                        args.data.MachineType = args.rowData.MachineType;
                        args.data.TechnicalName = args.rowData.TechnicalName;

                        $tblChildEl.updateRow(args.rowIndex, args.data);
                    }
                }
                else if (args.requestType === "delete") {

                }
            },
            recordClick: function (args) {
                
                if (args.column && args.column.field == "TagWithPrevReq") {
                    
                    var finder = new commonFinder({
                        title: "Select Process",
                        pageId: pageId,
                        apiEndPoint: `/api/yarn-receive/getPrevReq/${processType}`,
                        fields: "ProcessName,ProcessType,MachineName",
                        headerTexts: "Process Name, Process Type,Machine Name",
                        isMultiselect: true,
                        allowPaging: false,
                        primaryKeyColumn: "ProcessID",
                        onMultiselect: function (selectedRecords) {
                            var postProcessList = $tblColorChildElFP.getCurrentViewRecords();
                            for (var i = 0; i < selectedRecords.length; i++) {
                                var oPreProcess = {
                                    FPChildID: getMaxIdForArray(postProcessList, "FPChildID"),
                                    FPMasterID: 0,
                                    ProcessID: selectedRecords[i].ProcessID,
                                    ProcessTypeID: selectedRecords[i].ProcessTypeID,
                                    ProcessName: selectedRecords[i].ProcessName,
                                    ProcessType: selectedRecords[i].ProcessType,
                                    MachineName: selectedRecords[i].MachineName,
                                    FMCMasterID: selectedRecords[i].FMCMasterID,
                                    MachineNo: "",
                                    UnitName: "",
                                    BrandName: "",
                                    Remarks: "",
                                    ColorID: _fpBookingChildColorID,
                                    IsPreProcess: false,
                                    FMSID: null,
                                    PreFinishingProcessChildItems: []
                                }

                                var indexF = -1;
                                if (postProcessList.length > 0) {
                                    indexF = postProcessList.findIndex(y => y.ProcessName == oPreProcess.ProcessName && y.ProcessType == oPreProcess.ProcessType && y.MachineName == oPreProcess.MachineName);
                                }
                                if (indexF == -1) {
                                    postProcessList.push(oPreProcess);
                                }
                            }
                            initChildTableColorFP(postProcessList, true);
                        }
                    });
                    finder.showModal();

                }
            }
        });

    }*/
    function diplayPlanningCriteria(field, data, column) {
        column.disableHtmlEncode = false;
        return `<a class="btn btn-xs btn-default" href="javascript:void(0)" title="Tag">
                                     ${data[field] ? data[field] : 0}
                                </a>`;
    }
    function backToList() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        initMasterTable();
    }
    function backToListWithoutRefresh() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
    }

    function resetForm() {
        $formEl.trigger("reset");
        $.each($formEl.find('select'), function (i, el) {
            $(el).select2('');
        });
        $formEl.find("#QCReqMasterID").val(-1111);
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
        $formEl.find(".rejectReason").hide();
        $formEl.find("#btnRevise").hide();

        if (status == statusConstants.REJECT) {
            $formEl.find(".rejectReason").show();
        }
        if (status == statusConstants.APPROVED) {
            $formEl.find(".btnAction").hide();
        }

        if (isQCR) {
            if (status == statusConstants.REJECT) {
                $formEl.find(".btnAction").hide();
                $formEl.find("#btnRevise").show();
            }
            else if (status == statusConstants.PROPOSED_FOR_APPROVAL) {
                $formEl.find(".btnAction").hide();
            }
        }
    }

    function getNew() {
        axios.get("/api/yarn-qc-requisition/new")
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                reset();

                yarnQCReq = response.data;
                yarnQCReq.QCReqDate = formatDateToDefault(yarnQCReq.QCReqDate);
                yarnQCReq.ReceiveDate = formatDateToDefault(yarnQCReq.ReceiveDate);
                setFormData($formEl, yarnQCReq);
                $formEl.find("#ReceiveID").prop('disabled', false);
                initChildTable([]);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function getDetailsForRetest(id, qcReceiveChildID) {
        axios.get(`/api/yarn-qc-requisition/retest/${id}/${qcReceiveChildID}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                reset();

                yarnQCReq = response.data;
                yarnQCReq.QCReqDate = formatDateToDefault(yarnQCReq.QCReqDate);
                yarnQCReq.ReceiveDate = formatDateToDefault(yarnQCReq.ReceiveDate);
                setFormData($formEl, yarnQCReq);
                if (status == statusConstants.PENDING) {
                    yarnQCReq.YarnQCReqChilds = yarnQCReq.YarnQCReqChilds.filter(x => x.ItemMasterID == ItemMasterID);
                }
                initChildTable(yarnQCReq.YarnQCReqChilds);
                $formEl.find("#ReceiveID").prop('disabled', true);

            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getDetails(id, itemMasterID) {
        axios.get(`/api/yarn-qc-requisition/${id}/${status}/${itemMasterID}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                reset();

                yarnQCReq = response.data;
                yarnQCReq.QCReqDate = formatDateToDefault(yarnQCReq.QCReqDate);
                yarnQCReq.ReceiveDate = formatDateToDefault(yarnQCReq.ReceiveDate);
                setFormData($formEl, yarnQCReq);
                if (status == statusConstants.PENDING) {
                    yarnQCReq.YarnQCReqChilds = yarnQCReq.YarnQCReqChilds.filter(x => x.ItemMasterID == ItemMasterID);
                }
                initChildTable(yarnQCReq.YarnQCReqChilds);
                $formEl.find("#ReceiveID").prop('disabled', true);

            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getReceiveData(receiveId) {
        initChildTable(yarnQCReq.YarnQCReqChilds);
        axios.get(`/api/yarn-qc-requisition/new/receiveData?receiveId=${receiveId}`)
            .then(function (response) {
                yarnQCReq.ReceiveID = response.data.ReceiveID;
                yarnQCReq.ReceiveNo = response.data.ReceiveNo;
                yarnQCReq.ReceiveDate = response.data.ReceiveDate;
                yarnQCReq.LocationID = response.data.LocationID;
                yarnQCReq.CompanyID = response.data.CompanyID;
                yarnQCReq.RCompanyID = response.data.RCompanyID;
                yarnQCReq.ReceiveDate = formatDateToDefault(yarnQCReq.ReceiveDate);

                yarnQCReq.YarnQCReqChilds = response.data.YarnQCReqChilds;
                setFormData($formEl, yarnQCReq);
                initChildTable(yarnQCReq.YarnQCReqChilds);

            })
            .catch(showResponseError);
    }

    function save(isSendForApproval, isApprove, isReject, isRevise, isAcknowledge) {
        var data = formDataToJson($formEl.serializeArray());
        data.SupplierID = yarnQCReq.SupplierID;
        data.YarnQCReqChilds = yarnQCReq.YarnQCReqChilds; // $tblChildEl.getCurrentViewRecords();
        data.IsRetestForRequisition = yarnQCReq.IsRetestForRequisition;
        var hasError = false;

        if ((typeof data.ReceiveID === "undefined" || data.ReceiveID == 0) && data.YarnQCReqChilds.length > 0) {
            data.ReceiveID = data.YarnQCReqChilds[0].ReceiveID;
        }

        for (var iReq = 0; iReq < data.YarnQCReqChilds.length; iReq++) {
            var child = data.YarnQCReqChilds[iReq];
            if (child.MachineTypeId == 0 && child.MachineType != null) {
                var machineType = yarnQCReq.MCTypeForFabricList.find(x => x.text == child.MachineType);
                if (machineType) data.YarnQCReqChilds[iReq].MachineTypeId = machineType.id;
                else data.YarnQCReqChilds[iReq].MachineTypeId.MachineTypeId = 0;
            }
            if (child.TechnicalNameId == 0 && child.TechnicalName != null) {
                var techType = yarnQCReq.TechnicalNameList.find(x => x.text == child.TechnicalName);
                if (techType) data.YarnQCReqChilds[iReq].TechnicalNameId = techType.id;
                else data.YarnQCReqChilds[iReq].TechnicalNameId = 0;
            }
            if (typeof child.BuyerID === "undefined" || child.BuyerID == null) {
                child.BuyerID = 0;
            }
            if (child.ReqQty == 0) {
                toastr.error("Give req qty");
                hasError = true;
                break;
            }
            if (child.ReqQty > 50) {
                toastr.error("Max Req Qty should 50.");
                hasError = true;
                break;
            }
            if (child.ReqBagPcs == 0) {
                toastr.error("Give req bag qty");
                hasError = true;
                break;
            }
        }
        if (hasError) return false;

        var ChekBox1 = false, ChekBox2 = false, ChekBox3 = false;

        if ($formEl.find("#NeedUSTER").is(':checked')) ChekBox1 = 1;
        if ($formEl.find("#NeedYarnTRF").is(':checked')) ChekBox2 = 1;
        if ($formEl.find("#NeedFabricTRF").is(':checked')) ChekBox3 = 1;

        data.NeedUSTER = ChekBox1;
        data.NeedYarnTRF = ChekBox2;
        data.NeedFabricTRF = ChekBox3;

        if (data.YarnQCReqChilds.length > 0) {
            data.PhysicalCount = data.YarnQCReqChilds[0].PhysicalCount;
            data.LotNo = data.YarnQCReqChilds[0].LotNo;
        }

        data.IsFromNoTest = status == statusConstants.HOLD ? true : false;
        data.IsRetest = _isRetest;

        data.IsSendForApproval = isSendForApproval; //status == statusConstants.ReTest ? true : isSendForApproval;
        data.IsApprove = isApprove;
        data.IsReject = isReject;
        data.IsRevise = isRevise;
        data.IsAcknowledge = isAcknowledge;

        if (isReject) {
            data.RejectReason = $.trim($formEl.find("#RejectReason").val());
        }

        if (status == statusConstants.ReTest) {
            data.IsRetestDiagnostic = true;
        }

        axios.post("/api/yarn-qc-requisition/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                //if (status == statusConstants.ReTest) {
                //    showBootboxAlert("New QC Req No : <b>" + response.data.QCReqNo + "</b> created successfully.");
                //} else {
                //    toastr.success("Saved successfully.");
                //}
                backToList();
            })
            .catch(showResponseError);
    }

    function noTestStatusUpdate() {
        var data = {
            ChildID: $(pageIdWithHash).find("#txtChildID").val(),
            NoTestRemarks: $.trim($(pageIdWithHash).find("#txtNoTestRemark").val())
        };
        axios.post("/api/yarn-receive/update-NoTest", data)
            .then(function () {
                toastr.success("No test done.");
                $(pageIdWithHash).find("#txtChildID").val(0);
                backToList();
            })
            .catch(showResponseError);
    }
    function createNew() {
        var selectedRows = $tblMasterEl.getSelectedRecords();
        if (selectedRows.length == 0) {
            return toastr.error("You must select item(s)!");
        }
        var receiveChildIds = selectedRows.map(x => x.ReceiveChildID).join(",");
        getDetailsByChilds(receiveChildIds);
    }
    function getDetailsByChilds(receiveChildIds) {
        axios.get(`/api/yarn-qc-requisition/receive-child/${receiveChildIds}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                yarnQCReq = response.data;
                yarnQCReq.QCReqDate = formatDateToDefault(yarnQCReq.QCReqDate);
                yarnQCReq.ReceiveDate = formatDateToDefault(yarnQCReq.ReceiveDate);
                setFormData($formEl, yarnQCReq);
                initChildTable(yarnQCReq.YarnQCReqChilds);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function getRetest(qcReqMasterID) {
        axios.get(`/api/yarn-qc-requisition/retest/${qcReqMasterID}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                yarnQCReq = response.data;
                yarnQCReq.QCReqDate = formatDateToDefault(yarnQCReq.QCReqDate);
                yarnQCReq.ReceiveDate = formatDateToDefault(yarnQCReq.ReceiveDate);
                setFormData($formEl, yarnQCReq);
                initChildTable(yarnQCReq.YarnQCReqChilds);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
})();