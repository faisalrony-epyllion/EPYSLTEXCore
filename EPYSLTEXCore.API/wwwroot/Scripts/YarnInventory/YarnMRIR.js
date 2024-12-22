(function () {
    //alert("OK");
    var menuId, pageName, menuParam;
    var toolbarId, $tblOtherItemEl, tblOtherItemId;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $tblChildEl, tblChildId, $formEl, tblMasterId, $tblRollEl, $divRollEl, $tblKProductionE1;;
    var status;
    var masterData = null;
    var pageId, pageIdWithHash;
    var _SFDID = 999;
    var _FBAckChildID = 999;
    var _paramType = {
        YarnMRIR: 0,
        GRNSignIn: 1,
        YarnMRIRSCD: 2
    }
    var menuType = 0;
    var cTypeID = 0;
    var vIsBDS = 0;
    var ChildFabricItemList = new Array();
    var ChildOthersItemList = new Array();
    var _subGroup = 1;
    var IsEdit = false;
    var RNoteType = 0;
    var MRIRMasterId = 0;
    $(function () {
        if (!menuId)
            menuId = localStorage.getItem("menuId");
        if (!pageName)
            pageName = localStorage.getItem("pageName");
        if (!menuParam)
            menuParam = localStorage.getItem("menuParam");

        pageId = pageName + "-" + menuId;
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
        tblChildId = pageConstants.CHILD_TBL_ID_PREFIX + pageId;
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);

        pageIdWithHash = "#" + pageId;
        //menuType = localStorage.getItem("YarnMRIRPage");
        if (menuParam === "YarnMRIRSCD") {
            debugger;
            menuType = 2;
        }
        else if (menuParam === "GRN Sign In") {
            menuType = 1;
        }
        else {
            menuType = 0;
        }
        menuType = parseInt(menuType);
        debugger;
        if (menuType == _paramType.YarnMRIR) {

            $toolbarEl.find("#btnPendingMRIRList").show();
            $toolbarEl.find("#btnPendingGRNList").show();
            $toolbarEl.find("#btnPendingMRNList").show();
            $toolbarEl.find("#btnMRNList").show();
            $toolbarEl.find("#btnRetestReqList").hide();
            $toolbarEl.find("#btnReturnReqList").hide();
            $toolbarEl.find("#btnGRNList").show();
            $toolbarEl.find("#btnMRIRList").show();
            $toolbarEl.find("#btnAllList").show();
            $toolbarEl.find("#btnPendingGRNSignInList").hide();
            $toolbarEl.find("#btnGRNMRIRList").hide();

            //toggleActiveToolbarBtn($(pageIdWithHash).find("#btnPendingMRIRList"), $toolbarEl);
            toggleActiveToolbarBtn($toolbarEl.find("#btnPendingMRIRList"), $toolbarEl);
            status = statusConstants.PENDING;
            $divDetailsEl.find("#btnGRNMRIRSave").hide();
            $divTblEl.find("#btnMRIRSave").show();
            RNoteType = ReceiveNoteType.MRIR;
            $divDetailsEl.find("#divMRIRNo").hide();
            $divDetailsEl.find("#divGRNNo").hide();
            $divDetailsEl.find("#divMRNNo").hide();

            initMasterTable();

            $toolbarEl.find("#btnPendingMRIRList").on("click", function (e) {
                e.preventDefault();
                status = statusConstants.PENDING;

                $divTblEl.find("#btnMRIRSave").show();
                RNoteType = ReceiveNoteType.MRIR;

                toggleActiveToolbarBtn(this, $toolbarEl);
                initMasterTable();
            });
            $toolbarEl.find("#btnPendingGRNList").on("click", function (e) {
                e.preventDefault();
                status = statusConstants.PENDING2;

                $divTblEl.find("#btnMRIRSave").show();
                RNoteType = ReceiveNoteType.GRN;

                toggleActiveToolbarBtn(this, $toolbarEl);
                initMasterTable();
            });
            $toolbarEl.find("#btnPendingMRNList").on("click", function (e) {
                e.preventDefault();
                status = statusConstants.PENDING3;

                $divTblEl.find("#btnMRIRSave").show();
                RNoteType = ReceiveNoteType.MRN;

                toggleActiveToolbarBtn(this, $toolbarEl);
                initMasterTable();
            });
            $toolbarEl.find("#btnMRNList").on("click", function (e) {
                e.preventDefault();
                status = statusConstants.COMPLETED;

                $divTblEl.find("#btnMRIRSave").hide();
                $divDetailsEl.find("#divMRIRNo").hide();
                $divDetailsEl.find("#divGRNNo").hide();
                $divDetailsEl.find("#divMRNNo").show();

                toggleActiveToolbarBtn(this, $toolbarEl);
                initMasterTable();
            });
            $toolbarEl.find("#btnGRNList").on("click", function (e) {
                e.preventDefault();
                status = statusConstants.COMPLETED2;

                $divTblEl.find("#btnMRIRSave").hide();
                $divDetailsEl.find("#divMRIRNo").hide();
                $divDetailsEl.find("#divGRNNo").show();
                $divDetailsEl.find("#divMRNNo").hide();

                toggleActiveToolbarBtn(this, $toolbarEl);
                initMasterTable();
            });
            $toolbarEl.find("#btnMRIRList").on("click", function (e) {
                e.preventDefault();
                status = statusConstants.COMPLETED3;

                $divTblEl.find("#btnMRIRSave").hide();
                $divDetailsEl.find("#divMRIRNo").show();
                $divDetailsEl.find("#divGRNNo").hide();
                $divDetailsEl.find("#divMRNNo").hide();

                toggleActiveToolbarBtn(this, $toolbarEl);
                initMasterTable();
            });
            $toolbarEl.find("#btnAllList").on("click", function (e) {
                e.preventDefault();
                status = statusConstants.ALL;

                $divTblEl.find("#btnMRIRSave").hide();
                $divDetailsEl.find("#divMRIRNo").show();
                $divDetailsEl.find("#divGRNNo").show();
                $divDetailsEl.find("#divMRNNo").show();

                toggleActiveToolbarBtn(this, $toolbarEl);
                initMasterTable();
            });
            $formEl.find("#btnCancel").on("click", backToList);

            $divTblEl.find("#btnMRIRSave").click(function (e) {
                e.preventDefault();
                saveMRIR(false, '', false);
            });
        }
        else if (menuType == _paramType.GRNSignIn) {

            $toolbarEl.find("#btnPendingMRIRList").hide();
            $toolbarEl.find("#btnPendingGRNList").hide();
            $toolbarEl.find("#btnPendingMRNList").hide();
            $toolbarEl.find("#btnMRNList").hide();
            $toolbarEl.find("#btnRetestReqList").hide();
            $toolbarEl.find("#btnReturnReqList").hide();
            $toolbarEl.find("#btnGRNList").hide();
            $toolbarEl.find("#btnMRIRList").hide();
            $toolbarEl.find("#btnAllList").hide();
            $toolbarEl.find("#btnPendingGRNSignInList").show();
            $toolbarEl.find("#btnGRNMRIRList").show();

            toggleActiveToolbarBtn($(pageIdWithHash).find("#btnPendingGRNSignInList"), $toolbarEl);
            status = statusConstants.PENDING_CONFIRMATION;
            $divDetailsEl.find("#btnGRNMRIRSave").show();
            $divTblEl.find("#btnMRIRSave").hide();
            RNoteType = ReceiveNoteType.MRIR;
            $divDetailsEl.find("#divMRIRNo").hide();
            $divDetailsEl.find("#divGRNNo").show();
            $divDetailsEl.find("#divMRNNo").hide();

            initMasterTable();

            $toolbarEl.find("#btnPendingGRNSignInList").on("click", function (e) {
                e.preventDefault();
                status = statusConstants.PENDING_CONFIRMATION;

                $divDetailsEl.find("#btnGRNMRIRSave").show();
                RNoteType = ReceiveNoteType.MRIR;
                $divDetailsEl.find("#divMRIRNo").hide();
                $divDetailsEl.find("#divGRNNo").show();
                $divDetailsEl.find("#divMRNNo").hide();
                toggleActiveToolbarBtn(this, $toolbarEl);
                initMasterTable();
            });
            $toolbarEl.find("#btnGRNMRIRList").on("click", function (e) {
                e.preventDefault();
                status = statusConstants.YDP_COMPLETE;

                $divDetailsEl.find("#btnGRNMRIRSave").hide();
                $divDetailsEl.find("#divMRIRNo").show();
                $divDetailsEl.find("#divGRNNo").show();
                $divDetailsEl.find("#divMRNNo").hide();

                toggleActiveToolbarBtn(this, $toolbarEl);
                initMasterTable();
            });

            $formEl.find("#btnCancel").on("click", backToList);

            $divDetailsEl.find("#btnGRNMRIRSave").click(function (e) {

                e.preventDefault();
                saveMRIR(false, '', false);
            });
        }
        else if (menuType == _paramType.YarnMRIRSCD) {
            $toolbarEl.find("#btnPendingMRIRList").hide();
            $toolbarEl.find("#btnPendingGRNList").hide();
            $toolbarEl.find("#btnPendingMRNList").hide();
            $toolbarEl.find("#btnMRNList").show();
            $toolbarEl.find("#btnRetestReqList").show();
            $toolbarEl.find("#btnReturnReqList").show();
            $toolbarEl.find("#btnGRNList").show();
            $toolbarEl.find("#btnMRIRList").show();
            $toolbarEl.find("#btnAllList").show();
            $toolbarEl.find("#btnPendingGRNSignInList").hide();
            $toolbarEl.find("#btnGRNMRIRList").hide();

            toggleActiveToolbarBtn($(pageIdWithHash).find("#btnMRNList"), $toolbarEl);
            status = statusConstants.COMPLETED;
            $divDetailsEl.find("#btnGRNMRIRSave").hide();
            RNoteType = ReceiveNoteType.MRIR;
            $divTblEl.find("#btnMRIRSave").hide();
            $divDetailsEl.find("#btnReturnSave").show();
            $divDetailsEl.find("#btnRetestSave").show();
            $divDetailsEl.find("#divMRIRNo").hide();
            $divDetailsEl.find("#divGRNNo").hide();
            $divDetailsEl.find("#divMRNNo").show();

            initMasterTable();

            $toolbarEl.find("#btnMRNList").on("click", function (e) {
                e.preventDefault();
                status = statusConstants.COMPLETED;

                $divDetailsEl.find("#btnReturnSave").show();
                $divDetailsEl.find("#btnRetestSave").show();
                $divTblEl.find("#btnMRIRSave").hide();
                $divDetailsEl.find("#divMRIRNo").hide();
                $divDetailsEl.find("#divGRNNo").hide();
                $divDetailsEl.find("#divMRNNo").show();

                toggleActiveToolbarBtn(this, $toolbarEl);
                initMasterTable();
            });
            $toolbarEl.find("#btnRetestReqList").on("click", function (e) {
                e.preventDefault();
                status = statusConstants.ReTest;

                $divDetailsEl.find("#btnReturnSave").hide();
                $divDetailsEl.find("#btnRetestSave").hide();
                $divDetailsEl.find("#divMRIRNo").hide();
                $divDetailsEl.find("#divGRNNo").hide();
                $divDetailsEl.find("#divMRNNo").show();

                toggleActiveToolbarBtn(this, $toolbarEl);
                initMasterTable();
            });
            $toolbarEl.find("#btnReturnReqList").on("click", function (e) {
                e.preventDefault();
                status = statusConstants.RETURN;

                $divDetailsEl.find("#btnReturnSave").hide();
                $divDetailsEl.find("#btnRetestSave").hide();
                $divDetailsEl.find("#divMRIRNo").hide();
                $divDetailsEl.find("#divGRNNo").hide();
                $divDetailsEl.find("#divMRNNo").show();

                toggleActiveToolbarBtn(this, $toolbarEl);
                initMasterTable();
            });
            $toolbarEl.find("#btnGRNList").on("click", function (e) {
                e.preventDefault();
                status = statusConstants.COMPLETED2;

                $divDetailsEl.find("#btnReturnSave").hide();
                $divDetailsEl.find("#btnRetestSave").hide();
                $divDetailsEl.find("#divMRIRNo").hide();
                $divDetailsEl.find("#divGRNNo").show();
                $divDetailsEl.find("#divMRNNo").hide();

                toggleActiveToolbarBtn(this, $toolbarEl);
                initMasterTable();
            });
            $toolbarEl.find("#btnMRIRList").on("click", function (e) {
                e.preventDefault();
                status = statusConstants.COMPLETED3;

                $divDetailsEl.find("#btnReturnSave").hide();
                $divDetailsEl.find("#btnRetestSave").hide();
                $divDetailsEl.find("#divMRIRNo").show();
                $divDetailsEl.find("#divGRNNo").hide();
                $divDetailsEl.find("#divMRNNo").hide();

                toggleActiveToolbarBtn(this, $toolbarEl);
                initMasterTable();
            });
            $toolbarEl.find("#btnAllList").on("click", function (e) {
                e.preventDefault();
                status = statusConstants.ALL;

                $divDetailsEl.find("#btnReturnSave").hide();
                $divDetailsEl.find("#btnRetestSave").hide();
                $divDetailsEl.find("#divMRIRNo").show();
                $divDetailsEl.find("#divGRNNo").show();
                $divDetailsEl.find("#divMRNNo").show();

                toggleActiveToolbarBtn(this, $toolbarEl);
                initMasterTable();
            });
            $formEl.find("#btnCancel").on("click", backToList);

            $("#btnReturnSave").click(function (e) {
                e.preventDefault();
                saveMRIR(false, '', true);
            });
            $("#btnRetestSave").click(function (e) {

                e.preventDefault();
                bootbox.prompt("Are you sure you want to retest this?", function (result) {
                    if (!result) {
                        return toastr.error("Retest reason is required.");
                    }

                    saveMRIR(true, result, false);
                });
            });
        }
        $formEl.find("#btnCancel").on("click", backToList);

    });
    function initMasterTable() {

        var columns;
        if (menuType == _paramType.YarnMRIR && (status == statusConstants.PENDING || status == statusConstants.PENDING2 || status == statusConstants.PENDING3)) {
            columns = [
                //{
                //    headerText: 'Command', width: 100, textAlign: 'center', visible: (menuType == _paramType.YarnMRIR && (status != statusConstants.PENDING && status != statusConstants.PENDING2 && status != statusConstants.PENDING3)), commands: [
                //        { type: 'Edit', title: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                //        { type: 'Test Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
                //        { type: 'View Attachment', buttonOption: { cssClass: 'e-flat booking_attImage', iconCss: 'fa fa-file-image-o' } }
                //    ]
                //},
                {
                    headerText: 'Command', width: 120, textAlign: 'center', visible: (menuType == _paramType.YarnMRIR && (status == statusConstants.PENDING || status == statusConstants.PENDING2 || status == statusConstants.PENDING3)), commands: [
                        /*{ type: 'View', title: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },*/
                        { type: 'Test Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
                        /* { type: 'View Attachment', buttonOption: { cssClass: 'e-flat booking_attImage', iconCss: 'fa fa-file-image-o' } }*/
                    ]
                },
                {
                    field: 'MRIRChildID', headerText: 'MRIRChildID', visible: false
                },
                {
                    field: 'RackLocationId', headerText: 'RackLocationId', visible: false
                },
                {
                    field: 'ReceiveChildID', headerText: 'ReceiveChildID', visible: false
                },
                {
                    field: 'MRIRMasterID', headerText: 'MRIRMasterID', visible: false
                },
                {
                    field: 'QCRemarksNo', headerText: 'Test No'
                },
                {
                    field: 'Status', headerText: 'Test Result'
                },
                {
                    field: 'TestType', headerText: 'Test Type'
                },
                {
                    field: 'ReceiveFrom', headerText: 'Receive From'
                },
                {
                    field: 'YarnDetail', headerText: 'Yarn Detail'
                },
                {
                    field: 'PONo', headerText: 'PO No'
                },
                {
                    field: 'POQty', headerText: 'PO Qty'
                },
                {
                    field: 'InvoiceNo', headerText: 'Invoice No'
                },
                {
                    field: 'ReceiveNo', headerText: 'Receive No'
                },
                {
                    field: 'ReceiveDate', headerText: 'Receive Date', type: 'date', format: _ch_date_format_1, visible: menuType == _paramType.YarnMRIR && status == statusConstants.PENDING
                },
                {
                    field: 'ReceiveQty', headerText: 'Receive Qty'
                },
                {
                    field: 'ChallanNo', headerText: 'Challan No'
                },
                {
                    field: 'Supplier', headerText: 'Supplier'
                },
                {
                    field: 'Spinner', headerText: 'Spinner'
                },
                {
                    field: 'POCount', headerText: 'PO Count'
                },
                {
                    field: 'PhysicalCount', headerText: 'Physical Count'
                },
                {
                    field: 'LotNo', headerText: 'Lot No'
                },
                {
                    field: 'YarnControlNo', headerText: 'Control No'
                },
                {
                    field: 'VehicalNo', headerText: 'VehicalNo'
                },
                {
                    field: 'POUnit', headerText: 'PO Unit'
                },
                {
                    field: 'Remarks', headerText: 'Test Remarks'
                }
            ];
            columns.unshift({ type: 'checkbox', width: 50 });
            selectionType = "Multiple";
        }
        else {
            columns = [
                {
                    headerText: 'Command', textAlign: 'center', visible: (menuType == _paramType.GRNSignIn && status == statusConstants.PENDING_CONFIRMATION) || (menuType == _paramType.YarnMRIRSCD && (status == statusConstants.COMPLETED)), commands: [
                        { type: 'Edit', title: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                        { type: 'GRN Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
                        { type: 'MRIR Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
                        { type: 'MRN Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
                        /*{ type: 'View Attachment', buttonOption: { cssClass: 'e-flat booking_attImage', iconCss: 'fa fa-file-image-o' } }*/
                    ]
                },
                {
                    headerText: 'Command', textAlign: 'center', visible: (menuType == _paramType.YarnMRIR && (status == statusConstants.COMPLETED || status == statusConstants.COMPLETED2 || status == statusConstants.COMPLETED3 || status == statusConstants.ALL)) || (menuType == _paramType.GRNSignIn && status == statusConstants.YDP_COMPLETE) || (menuType == _paramType.YarnMRIRSCD && (status == statusConstants.COMPLETED2 || status == statusConstants.COMPLETED3 || status == statusConstants.ALL || status == statusConstants.RETURN || status == statusConstants.ReTest)), commands: [
                        { type: 'View', title: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                        { type: 'GRN Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
                        { type: 'MRIR Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
                        { type: 'MRN Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
                        /* { type: 'View Attachment', buttonOption: { cssClass: 'e-flat booking_attImage', iconCss: 'fa fa-file-image-o' } }*/
                    ]
                },
                {
                    field: 'MRIRMasterID', headerText: 'MRIRMasterID', visible: false
                },
                {
                    field: 'MRIRNo', headerText: 'MRIR No', visible: (menuType == _paramType.YarnMRIR && (status == statusConstants.COMPLETED3 || status == statusConstants.ALL)) || (menuType == _paramType.GRNSignIn && status == statusConstants.YDP_COMPLETE) || (menuType == _paramType.YarnMRIRSCD && (status == statusConstants.COMPLETED3 || status == statusConstants.ALL))
                },
                {
                    field: 'GRNNo', headerText: 'GRN No', visible: (menuType == _paramType.YarnMRIR && (status == statusConstants.COMPLETED2 || status == statusConstants.COMPLETED3 || status == statusConstants.ALL)) || (menuType == _paramType.GRNSignIn) || (menuType == _paramType.YarnMRIRSCD && (status == statusConstants.COMPLETED2 || status == statusConstants.COMPLETED3 || status == statusConstants.ALL))
                },
                {
                    field: 'MRNNo', headerText: 'MRN No', visible: (menuType == _paramType.YarnMRIR && (status == statusConstants.COMPLETED || status == statusConstants.ALL)) || (menuType == _paramType.YarnMRIRSCD && (status == statusConstants.COMPLETED || status == statusConstants.RETURN || status == statusConstants.ReTest || status == statusConstants.ALL))
                },
                {
                    field: 'ReceiveFrom', headerText: 'Receive From'
                },
                {
                    field: 'PONo', headerText: 'PO No'
                },
                {
                    field: 'POQty', headerText: 'PO Qty'
                },
                {
                    field: 'InvoiceNo', headerText: 'Invoice No'
                },
                {
                    field: 'ReceiveNo', headerText: 'Receive No'
                },
                {
                    field: 'ReceiveDate', headerText: 'Receive Date', type: 'date', format: _ch_date_format_1
                },
                {
                    field: 'ReceiveQty', headerText: 'Receive Qty'
                },
                {
                    field: 'ReceiveNoteQty', headerText: 'MRIR/MRN/GRN Qty', visible: (menuType == _paramType.YarnMRIR && (status == statusConstants.COMPLETED || status == statusConstants.COMPLETED2 || status == statusConstants.COMPLETED3 || status == statusConstants.ALL)) || (menuType == _paramType.GRNSignIn) || (menuType == _paramType.YarnMRIRSCD)
                },
                {
                    field: 'ChallanNo', headerText: 'Challan No'
                },
                {
                    field: 'Supplier', headerText: 'Supplier'
                },
                {
                    field: 'Spinner', headerText: 'Spinner'
                },
                {
                    field: 'VehicalNo', headerText: 'VehicalNo'
                },
                {
                    field: 'POUnit', headerText: 'PO Unit'
                },
                {
                    field: 'AllocationChildID', headerText: 'AllocationChildID', visible: false
                }
            ];
        }
        /*if (menuType == _paramType.YarnMRIR) {
            if (status == statusConstants.PENDING || status == statusConstants.PENDING2 || status == statusConstants.PENDING3) {
                columns.unshift({ type: 'checkbox', width: 50 });
                selectionType = "Multiple";
            }
        }*/
        if ($tblMasterEl) $tblMasterEl.destroy();

        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            allowGrouping: false,
            apiEndPoint: `/api/yarn-mrir/list?status=${status}`,
            columns: columns,
            commandClick: handleCommands,
            queryCellInfo: cellModifyForBDSAck
        });
    }
    function handleCommands(args) {
        if (args.commandColumn.type == 'Edit') {

            getView(args.rowData.MRIRMasterId);

        }
        else if (args.commandColumn.type == 'View') {


            getView(args.rowData.MRIRMasterId);

        }
        else if (args.commandColumn.type == 'Test Report') {

            window.open(`/reports/InlinePdfView?ReportName=YarnTestReport.rdl&QCRemarksMasterId=${args.rowData.QCRemarksMasterID}`, '_blank');

        }
        else if (args.commandColumn.type == 'GRN Report') {
            window.open(`/reports/InlinePdfView?ReportName=YarnGRN.rdl&GRNNo=${args.rowData.GRNNo}`, '_blank');
        }
        else if (args.commandColumn.type == 'MRIR Report') {
            window.open(`/reports/InlinePdfView?ReportName=YarnMRIR.rdl&MRIRNo=${args.rowData.MRIRNo}`, '_blank');
        }
        else if (args.commandColumn.type == 'MRN Report') {
            window.open(`/reports/InlinePdfView?ReportName=YarnMRN.rdl&MRNNo=${args.rowData.MRNNo}`, '_blank');
        }

    }
    function handleCommandsChild(args) {
        if (args.commandColumn.type == 'Test Report') {

            window.open(`/reports/InlinePdfView?ReportName=YarnTestReport.rdl&QCRemarksMasterId=${args.rowData.QCRemarksMasterID}`, '_blank');

        }
    }
    function getView(MRIRMasterID) {
        MRIRMasterId = MRIRMasterID;
        var url = `/api/yarn-mrir/GetMRIRDetails/${MRIRMasterID}`;
        axios.get(url)
            .then(function (response) {

                $divDetailsEl.show();
                $divTblEl.hide();
                masterData = response.data;
                //masterData.DeliveredDate = formatDateToDefault(masterData.DeliveredDate);

                setFormData($formEl, masterData);
                initChildTable(masterData.YarnMRIRChilds);

            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function cellModifyForBDSAck(args) {
        if (args.data.ImagePath == '') {
            //if (args.cell.classList.contains("e-unboundcell")) {
            //args.cell.querySelector(".booking_attImage").style.display = "none";
            if (args.cell.childNodes.length > 0) {
                for (var i = 0; i < args.cell.childNodes[0].childNodes.length; i++) {
                    if (args.cell.childNodes[0].childNodes[i].title === 'View Attachment') {
                        args.cell.childNodes[0].childNodes[i].style.display = "none";
                    }
                }
            }
            //}
        }
    }
    function backToList() {

        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        if (status === statusConstants.NEW) {
            status = statusConstants.ACKNOWLEDGE;
            toggleActiveToolbarBtn("#btnPendingList", $toolbarEl);
        }
        initMasterTable();
    }
    function resetForm() {
        $formEl.trigger("reset");
        $formEl.find("#SFDID").val(-1111);
    }
    function cellModifyForBDSAck(args) {
        if (args.data.ImagePath == '') {
            //if (args.cell.classList.contains("e-unboundcell")) {
            //args.cell.querySelector(".booking_attImage").style.display = "none";
            if (args.cell.childNodes.length > 0) {
                for (var i = 0; i < args.cell.childNodes[0].childNodes.length; i++) {
                    if (args.cell.childNodes[0].childNodes[i].title === 'View Attachment') {
                        args.cell.childNodes[0].childNodes[i].style.display = "none";
                    }
                }
            }
            //}
        }
    }
    function initChildTable(data) {
        if ($tblChildEl) {
            $tblChildEl.destroy();
            $(tblChildId).html("");
        }
        ej.base.enableRipple(true);
        $tblChildEl = new ej.grids.Grid({
            dataSource: data,
            allowRowDragAndDrop: false,
            allowResizing: true,
            autofitColumns: false,
            selectionSettings: { type: 'Multiple' },
            editSettings: { allowAdding: true, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
            commandClick: handleCommandsChild,
            columns: [
                {
                    headerText: 'Commands', width: 120, textAlign: 'Center', commands: [
                        { type: 'Test Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
                        //{ type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                        //{ type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                        //{ type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                        //{ type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }
                    ]
                },
                {
                    field: 'MRIRChildID', headerText: 'MRIRChildID', visible: false
                },
                {
                    field: 'RackLocationId', headerText: 'RackLocationId', visible: false
                },
                {
                    field: 'ReceiveChildID', headerText: 'ReceiveChildID', visible: false
                },
                {
                    field: 'MRIRMasterID', headerText: 'MRIRMasterID', visible: false
                },
                {
                    field: 'QCRemarksNo', headerText: 'Test No'
                },
                {
                    field: 'Status', headerText: 'Test Result'
                },
                {
                    field: 'TestType', headerText: 'Test Type'
                },
                {
                    field: 'PONo', headerText: 'PO No'
                },
                {
                    field: 'POQty', headerText: 'PO Qty'
                },
                {
                    field: 'InvoiceNo', headerText: 'Invoice No'
                },
                {
                    field: 'ReceiveNo', headerText: 'Receive No'
                },
                {
                    field: 'ReceiveDate', headerText: 'Receive Date', type: 'date', format: _ch_date_format_1
                },
                {
                    field: 'ReceiveQty', headerText: 'Receive Qty'
                },
                {
                    field: 'ReceiveNoteQty', headerText: 'MRIR/MRN/GRN Qty'
                },
                {
                    field: 'ChallanNo', headerText: 'Challan No'
                },
                {
                    field: 'Supplier', headerText: 'Supplier'
                },
                {
                    field: 'Spinner', headerText: 'Spinner'
                },
                {
                    field: 'POCount', headerText: 'PO Count'
                },
                {
                    field: 'PhysicalCount', headerText: 'Physical Count'
                },
                {
                    field: 'YarnControlNo', headerText: 'Control No'
                },
                {
                    field: 'VehicalNo', headerText: 'VehicalNo'
                },
                {
                    field: 'POUnit', headerText: 'PO Unit'
                },
                {
                    field: 'Remarks', headerText: 'Test Remarks'
                }
            ],
            /*actionBegin: function (args) {
                if (args.requestType === "add") {
                    args.data.SFDChildID = getMaxIdForArray(masterData.Childs, "SFDChildID");
                }
                else if (args.requestType === "delete") {
                    var index = $tblChildEl.getRowIndexByPrimaryKey(masterData.Childs, "SFDChildID");
                    var ChildsItemSegmentsList = $tblChildEl.getCurrentViewRecords();
                    masterData.Childs[0].FabricItemIDs = ChildsItemSegmentsList.map(function (el) {
                        if (args.data[0].SFDChildID != el.SFDChildID) {
                            return el.SFDChildID
                        }
                    }).toString();
                }
                else if (args.requestType === "save") {
                    if (args.data.FormID == null || typeof args.data.FormID === "undefined" || args.data.FormID == 0) {
                        toastr.error("Select Form");
                        args.data.editable = true;
                        return;
                    }
                    if (args.data.FormID != 1120) {
                        args.rowData = args.data;
                        args.rowData.HangerQtyInPcs = 0;
                        $tblChildEl.updateRow(args.rowIndex, args.rowData);
                    }
                }
            },//
            childGrid: {
                queryString: 'SFDChildID',
                allowResizing: true,
                toolbar: [
                    { text: 'Add Roll', tooltipText: 'Add Roll', prefixIcon: 'e-icons e-add', id: 'addItem', visible: menuType == _paramType.LabdipFabricDelivery && (status == statusConstants.PENDING || status == statusConstants.EDIT) },
                    { text: 'Split Roll', tooltipText: 'Split Roll', prefixIcon: 'e-icons e-copy', id: 'splitRoll', visible: menuType == _paramType.LabdipFabricDelivery && (status == statusConstants.PENDING || status == statusConstants.EDIT) }
                ],
                editSettings: { allowAdding: true, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
                columns: [
                    {
                        headerText: 'Action', width: 10, visible: menuType == _paramType.LabdipFabricDelivery && (status == statusConstants.PENDING || status == statusConstants.EDIT), commands: [
                            //{ type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                            { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                            { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                            { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }
                        ]
                    },
                    { field: 'SFDChildRollID', isPrimaryKey: true, visible: false },
                    { field: 'SFDChildID', visible: false, width: 40 },
                    { field: 'BookingChildID', visible: false, width: 40 },
                    { field: 'BookingID', visible: false, width: 40 },
                    { field: 'ConsumptionID', visible: false, width: 40 },
                    { field: 'SubGroupID', visible: false, width: 40 },
                    { field: 'ItemMasterID', visible: false, width: 40 },
                    { field: 'RollID', visible: false, width: 40 },
                    { field: 'RollNo', headerText: 'Roll No', allowEditing: false, allowResizing: true, width: 40 },
                    //{ field: 'Shade', headerText: 'Shade', width: 100, minWidth: 100, maxWidth: 100 },
                    { field: 'BatchNo', headerText: 'Batch No', allowEditing: false, allowResizing: true, width: 40 },
                    { field: 'UseBatchNo', visible: false, width: 40 },
                    { field: 'RackID', visible: false, width: 40 },
                    { field: 'WeightSheetNo', visible: false, width: 40 },
                    { field: 'RollQtyKg', width: 40, allowEditing: false, allowResizing: true, headerText: 'Roll Qty(kg)', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 2, min: 0 } } },
                    { field: 'RollQtyPcs', width: 120, allowEditing: false, allowResizing: true, headerText: 'Roll Qty(Pcs)', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 0 } } },
                    {
                        field: 'IsAcknowledge', headerText: 'Acknowledged?', allowEditing: (menuType == _paramType.LabdipFabricAck && (status == statusConstants.APPROVED)), visible: menuType == _paramType.LabdipFabricAck || status == statusConstants.ACKNOWLEDGE, displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center', defaultValue: true
                    }
                ],
                toolbarClick: function (args) {
                    var iDCQty = 0;
                    var data = this.parentDetails.parentRowData;
                    selectedData = data;

                    if (args.item.id === "addItem") {

                        setSubGroupValue(masterData.ChildItems);
                        var ChildItemsFabric = masterData.ChildItems.filter(function (el) {
                            return el.ConceptID == selectedData.ConceptID;
                        });
                        if (ChildItemsFabric.length == 0) {
                            toastr.error("No Roll Found");
                            return false;
                        }

                        var ChildsFabric = masterData.Childs;

                        var FabricItemList = new Array();

                        if (ChildItemsFabric.length > 0) {
                            //var rollList = ChildItemsFabric.filter(x => x.TechnicalName == selectedData.TechnicalName);

                            var SFDChildRollIDs = "";
                            var childs = $tblChildEl.getCurrentViewRecords();
                            var childObj = childs[0];
                            if (typeof childObj.ChildItems !== "undefined" && childObj.ChildItems != null) {
                                SFDChildRollIDs = childObj.ChildItems.map(x => x.SFDChildRollID).join(",");
                            }

                            var rollList = ChildItemsFabric;
                            var finder = new commonFinder({
                                title: "Select Items",
                                pageId: pageId,
                                data: rollList, //masterData.Childs,
                                fields: "RollNo,RollQtyKg,RollQtyPcs,GroupConceptNo",
                                headerTexts: "Roll No,Roll Qty(kg),Roll Qty(Pcs),Group Concept No",
                                isMultiselect: true,
                                selectedIds: SFDChildRollIDs,
                                allowPaging: false,
                                primaryKeyColumn: "SFDChildRollID",
                                onMultiselect: function (selectedRecords) {
                                    for (var i = 0; i < selectedRecords.length; i++) {
                                        var oPreProcess = {
                                            SFDChildRollID: selectedRecords[i].SFDChildRollID,
                                            SFDChildID: data.SFDChildID,
                                            GroupConceptNo: selectedRecords[i].GroupConceptNo,
                                            ConceptID: selectedRecords[i].ConceptID,
                                            CCColorID: selectedRecords[i].CCColorID,
                                            ColorID: selectedRecords[i].ColorID,
                                            BChildID: selectedRecords[i].BChildID,
                                            BItemReqID: selectedRecords[i].BItemReqID,
                                            BatchID: selectedRecords[i].BatchID,
                                            RollQtyKg: selectedRecords[i].RollQtyKg,
                                            RollQtyPcs: selectedRecords[i].RollQtyPcs,
                                            RollID: selectedRecords[i].RollID,
                                            RollNo: selectedRecords[i].RollNo,
                                            SubGroupID: selectedRecords[i].SubGroupID,
                                            SubGroupName: selectedRecords[i].SubGroupName,
                                            BatchNo: selectedRecords[i].BatchNo
                                        }
                                        FabricItemList.push(oPreProcess);
                                        iDCQty += parseFloat(selectedRecords[i].RollQtyKg);
                                    }
                                    var indexFind = ChildsFabric.findIndex(x => x.SFDChildID == data.SFDChildID);
                                    ChildsFabric[indexFind].ChildItems = FabricItemList;
                                    //ChildsFabric[indexFind].ChildItems = FabricItemList;
                                    ChildsFabric[indexFind].DCQty = iDCQty;
                                    var index = $tblChildEl.getRowIndexByPrimaryKey(data.SFDChildID);
                                    $tblChildEl.updateRow(index, ChildsFabric[indexFind]);
                                    $tblChildEl.refreshColumns;
                                }
                            });
                            finder.showModal();
                        }
                    }
                    else if (args.item.id === "splitRoll") {

                        setSubGroupValue(masterData.ChildItems);
                        var ChildItemsFabric = masterData.ChildItems.filter(function (el) {
                            return el.ConceptID == selectedData.ConceptID;
                        });
                        initMachineParamTable(ChildItemsFabric);
                        $("#modal-machine").modal('show');

                    }
                },
                actionBegin: function (args) {
                    if (args.requestType === "add") {

                    }
                    //else if (args.requestType === "save") {
                    //    var data = this.parentDetails.parentRowData;
                    //}
                },
                load: loadFabricFirstLevelChildGrid
            }*/
        });
        $tblChildEl.refreshColumns;
        $tblChildEl.appendTo(tblChildId);
    }

    function saveMRIR(isRetest, RetestReason, isReturn) {
        if ($tblMasterEl.getSelectedRecords().length == 0) {
            toastr.error("Please select row(s)!");
            return;
        }
        var data = $tblMasterEl.getSelectedRecords();
        var checkObj = {
            ChallanNo: data[0].ChallanNo,
            ReceiveNo: data[0].ReceiveNo
        };
        for (var i = 0; i < data.length; i++) {
            var child = data[i];
            data[i].ReceiveNoteType = RNoteType;

            if (child.ChallanNo != checkObj.ChallanNo || child.ReceiveNo != checkObj.ReceiveNo) {
                toastr.error("Challan No & Receive No should be same!");
                return;
            }
        }

        var path = "/api/yarn-mrir/Save";
        if (menuType == _paramType.GRNSignIn) {
            path = "/api/yarn-mrir/SaveGRNMRIR";
            data = masterData;
            data.MRIRMasterId = MRIRMasterId;
        };
        if (isRetest) {
            path = "/api/yarn-mrir/retest";
            data = masterData;
            data.MRIRMasterId = MRIRMasterId;
            data.ReTest = true;
            data.ReTestReason = RetestReason;
        }
        if (isReturn) {
            path = "/api/yarn-mrir/return";
            data = masterData;
            data.MRIRMasterId = MRIRMasterId;
            data.Returned = true;
        }
        axios.post(path, data)
            .then(function (response) {
                toastr.success(`Successfully saved (MRIR No : ${response.data})`);
                backToList();
            })
            .catch(showResponseError);
    }

    function DeepClone(obj) {
        return JSON.parse(JSON.stringify(obj));
    }

})();