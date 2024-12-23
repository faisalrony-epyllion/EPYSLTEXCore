(function () {
    var menuId, pageName;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $tblChildEl, $formEl, tblMasterId, tblChildId;
    var index;
    var status;
    var isYQCReturn = false, isYQCReturnApproval = false;
    var tableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    }

    var masterData;
    var dataset = new Array();
    var status;
    //var status = statusConstants.PENDING;

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

        var menuParam = $("#" + pageId).find("#txtMenuParam").val();
        if (menuParam == "YQCReturn") isYQCReturn = true;
        else if (menuParam == "YQCReturnApproval") isYQCReturnApproval = true;


        $toolbarEl.find("#btnPending").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PENDING;

            initMasterTable();
        });

        $toolbarEl.find("#btnList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.COMPLETED;

            initMasterTable();
        });

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save(false);
        });

        $formEl.find("#btnSaveAndSendForApproval").click(function (e) {
            e.preventDefault();
            save(true);
        });

        $formEl.find("#btnApprove").click(function (e) {
            e.preventDefault();
            approve(dataset, index);
        });

        $toolbarEl.find("#btnCreate").click(function () {
            createNew();
        });

        if (!isYQCReturn) {
            $toolbarEl.find("#btnPendingListForApproval").show();
            $toolbarEl.find("#btnApprovalList").show();
            $formEl.find("#btnApprove").show();
            $toolbarEl.find("#btnPending,#btnList,#btnCreate").hide();

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
                status = statusConstants.APPROVED;
                $formEl.find("#btnSave").hide();
                $formEl.find("#btnApprove").hide();
                $formEl.find("#btnCreate").hide();
                initMasterTable();
            });
            $toolbarEl.find("#btnPendingListForApproval").click();
        }
        else {
            $toolbarEl.find("#btnPending,#btnList").show();
            $toolbarEl.find("#btnPendingListForApproval").hide();

            $formEl.find("#btnApprove").hide();
            $formEl.find("#btnSaveAndSendForApproval").show();
            $toolbarEl.find("#btnApprovalList").hide();
            $toolbarEl.find("#btnPending").click();
        }

        $formEl.find("#btnCancel").on("click", backToList);
    });

    function initMasterTable() {
        var columns = [
            //{
            //    headerText: 'Commands', width: 100, visible: status == statusConstants.PENDING, commands: [
            //        { type: 'Add', buttonOption: { cssClass: 'e-flat', iconCss: 'e-add e-icons' } }
            //    ]
            //},
            {
                headerText: 'Commands', width: 100, visible: status !== statusConstants.PENDING, commands: [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                    { type: 'Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fas fa-file-pdf' } }
                ]
            },
            {
                field: 'QCReturnNo', headerText: 'Return No', visible: status !== statusConstants.PENDING
            },
            {
                field: 'QCReturnDate', headerText: 'Return Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, visible: status !== statusConstants.PENDING
            },
            {
                field: 'QCRemarksNo', headerText: 'Remarks No', visible: status !== statusConstants.PENDING
            },
            {
                field: 'QCRemarksDate', headerText: 'Remarks Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, visible: status !== statusConstants.PENDING
            },
            {
                field: 'QCReqNo', headerText: 'QC Requisition No'
            },
            {
                field: 'YarnCategory', headerText: 'Yarn Detail', visible: status === statusConstants.PENDING
            },
            {
                field: 'PhysicalCount', headerText: 'Physical Count', visible: status === statusConstants.PENDING
            },
            {
                field: 'LotNo', headerText: 'Lot No', visible: status === statusConstants.PENDING
            },
            {
                field: 'Spinner', headerText: 'Spinner', visible: status === statusConstants.PENDING
            },
            {
                field: 'ShadeCode', headerText: 'ShadeCode', visible: status === statusConstants.PENDING
            },
            {
                field: 'QCIssueNo', headerText: 'QC Issue No'
            },
            {
                field: 'QCReqQty', headerText: 'QC Requisition Qty', visible: false
            },
            {
                field: 'QCReceiveNo', headerText: 'QC Receive No'
            },
            {
                field: 'ReceiveNo', headerText: 'Yarn Receive No'
            },
            {
                field: 'ReceiveQtyCarton', headerText: 'Receive Qty(Carton)', visible: false
            },
            {
                field: 'ReceiveQtyCone', headerText: 'Receive Qty(Cone)', visible: false
            },
            {
                field: 'ReturnQtyCarton', headerText: 'Rtn Qty(Carton)', visible: false
            },
            {
                field: 'ReturnQtyCone', headerText: 'Rtn Qty(Cone)', visible: false
            },
            {
                field: 'QCReturnByUser', headerText: 'Return By', visible: status !== statusConstants.PENDING
            }
        ];

        if (status == statusConstants.PENDING) {
            columns.splice(0, 0, {
                type: 'checkbox', width: 30, headerText: 'Select'
            });
        }
        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: false,
            apiEndPoint: `/api/yarn-qc-return/list?status=${status}`,
            columns: columns,
            commandClick: handleCommands
        });
    }

    function handleCommands(args) {
        if (status === statusConstants.PENDING) {
            //getNew(args.rowData.QCRemarksMasterID);
            getNew(args.rowData.QCReceiveMasterID);
        }
        else if (args.commandColumn.type == 'Report') {
            window.open(`/reports/InlinePdfView?ReportName=MaterialReturnNote.rdl&ReturnNo=${args.rowData.QCReturnNo}`, '_blank');
        }
        else {
            getDetails(args.rowData.QCReturnMasterID);
        }
    }


    function initChildTable(records) {
        if ($tblChildEl) $tblChildEl.destroy();

        $tblChildEl = new initEJ2Grid({
            tableId: tblChildId,
            data: records,
            autofitColumns: true,
            allowSorting: true,
            allowPaging: false,
            allowFiltering: false,
            editSettings: { allowAdding: true, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
            columns: [
                {
                    headerText: 'Command', width: 100, commands: [
                        { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit', visible: status !== statusConstants.PROPOSED_FOR_APPROVAL } },
                        { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                        { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                        { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }
                    ]
                },
                { field: 'QCReturnChildID', isPrimaryKey: true, visible: false },
                { field: 'QCReturnMasterID', visible: false },
                { field: 'ReceiveChildID', visible: false },
                { field: 'QCRemarksChildID', visible: false },

                //{ field: 'Segment1ValueDesc', headerText: 'Composition', allowEditing: false },
                //{ field: 'Segment2ValueDesc', headerText: 'Yarn Type', allowEditing: false },
                //{ field: 'Segment3ValueDesc', headerText: 'Process', allowEditing: false },
                //{ field: 'Segment4ValueDesc', headerText: 'Sub process', allowEditing: false },
                //{ field: 'Segment5ValueDesc', headerText: 'Quality Parameter', allowEditing: false },
                //{ field: 'Segment6ValueDesc', headerText: 'Yarn Count', allowEditing: false },
                //{ field: 'Segment7ValueDesc', headerText: 'No of Ply', allowEditing: false },
                //{ field: 'ShadeCode', headerText: 'Shade Code', allowEditing: false },
                { field: 'YarnCategory', headerText: 'Yarn Details', allowEditing: false, textAlign: 'Center' },
                { field: 'Uom', headerText: 'Uom', allowEditing: false, textAlign: 'Center' },
                { field: 'ChallanLot', headerText: 'Challan Lot', allowEditing: false },
                { field: 'LotNo', headerText: 'Physical Lot', allowEditing: false, textAlign: 'Center' },
                { field: 'PhysicalCount', headerText: 'Physical Count', allowEditing: false, textAlign: 'Center' },
                { field: 'Spinner', headerText: 'Spinner', allowEditing: false, textAlign: 'Center' },

                { field: 'QCReceiveNo', headerText: 'QC Receive No', allowEditing: false },
                { field: 'QCIssueNo', headerText: 'QC Issue No', allowEditing: false },
                { field: 'ReceiveNo', headerText: 'Yarn Receive No', allowEditing: false },

                { field: 'ReceiveQty', headerText: 'Receive Qty', allowEditing: false },
                { field: 'ReceiveQtyCone', headerText: 'Receive Qty(Cone)', allowEditing: false },
                { field: 'ReceiveQtyCarton', headerText: 'Receive Qty(Carton)', allowEditing: false },
                { field: 'ReturnQty', headerText: 'Return Qty', allowEditing: isYQCReturn },
                { field: 'ReturnQtyCone', headerText: 'Return Qty(Cone)', allowEditing: isYQCReturn },
                { field: 'ReturnQtyCarton', headerText: 'Return Qty(Carton)', allowEditing: isYQCReturn },
                { field: 'Remarks', headerText: 'Remarks' }
            ]

        });

    }

    function backToList() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        initMasterTable();
    }

    function resetForm() {
        $formEl.trigger("reset");
        $.each($formEl.find('select'), function (i, el) {
            $(el).select2('');
        });
        $formEl.find("#QCReturnMasterID").val(-1111);
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
        axios.get(`/api/yarn-qc-return/new/${reqMasterId}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();

                masterData = response.data;
                masterData.QCReturnDate = formatDateToDefault(masterData.QCReturnDate);
                masterData.QCReqDate = formatDateToDefault(masterData.QCReqDate);
                masterData.QCReceiveDate = formatDateToDefault(masterData.QCReceiveDate);
                setFormData($formEl, masterData);
                initChildTable(masterData.YarnQCReturnChilds);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getDetails(id) {
        axios.get(`/api/yarn-qc-return/${id}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                actionBtnHideShow();

                masterData = response.data;
                masterData.QCReturnDate = formatDateToDefault(masterData.QCReturnDate);
                masterData.QCReqDate = formatDateToDefault(masterData.QCReqDate);
                masterData.QCReceiveDate = formatDateToDefault(masterData.QCReceiveDate);
                setFormData($formEl, masterData);
                initChildTable(masterData.YarnQCReturnChilds);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function actionBtnHideShow() {
        $formEl.find(".btnAction").hide();
        switch (status) {
            case statusConstants.PENDING:
                $formEl.find("#btnSave").show();
                break;
            case statusConstants.PROPOSED_FOR_APPROVAL: 
                $formEl.find("#btnApprove").show();
                break;
            case statusConstants.COMPLETED:
                 $formEl.find("#btnSaveAndSendForApproval").show();
               
                break;
            
            default:
            // code block
        }
    }

    function save(isSendForApproval) {
        var data = formDataToJson($formEl.serializeArray());
        data.IsSendForApproval = isSendForApproval;
        data.YarnQCReturnChilds = $tblChildEl.getCurrentViewRecords();

        var hasError = false;
        for (var i = 0; i < data.YarnQCReturnChilds.length; i++) {
            var child = data.YarnQCReturnChilds[i];
            if (child.ReceiveQtyCarton < child.ReturnQtyCarton) {
                toastr.error(`Return carton qty ${child.ReturnQtyCarton} cannot be greater than receive carton qty ${child.ReceiveQtyCarton}`);
                hasError = true;
                break;
            }
            if (child.ReceiveQtyCone < child.ReturnQtyCone) {
                toastr.error(`Return cone qty ${child.ReturnQtyCone} cannot be greater than receive cone qty ${child.ReceiveQtyCone}`);
                hasError = true;
                break;
            }
            if (child.ReceiveQty < child.ReturnQty) {
                toastr.error(`Return return qty ${child.ReturnQty} cannot be greater than receive qty ${child.ReceiveQty}`);
                hasError = true;
                break;
            }
        }

        if (hasError) return true;

        axios.post("/api/yarn-qc-return/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(function (error) {

                toastr.error(error.response.data.Message);
            });
    }

    function approve() {
        var data = formDataToJson($formEl.serializeArray());
        data.YarnQCReturnChilds = $tblChildEl.getCurrentViewRecords();
        axios.post("/api/yarn-qc-return/approve", data)
            .then(function (response) {
                toastr.success("Approved successfully.");
                backToList();
            })
            .catch(showResponseError);
    }

    function createNew() {
        var selectedRows = $tblMasterEl.getSelectedRecords();
        if (selectedRows.length == 0) {
            return toastr.error("You must select item(s)!");
        }
        var uniqueAry = distinctArrayByProperty($tblMasterEl.getSelectedRecords(), "QCReqNo");
        if (uniqueAry.length != 1) {
            toastr.error("Selected row(s) QC requisition no should be same !");
            return;
        }
        var qcReceiveChildIDs = selectedRows.map(x => x.QCReceiveChildID).join(",");
        getDetailsByQCReturnChilds(qcReceiveChildIDs);
    }
    function getDetailsByQCReturnChilds(qcReceiveChildIDs) {
        axios.get(`/api/yarn-qc-return/qc-receive-child/${qcReceiveChildIDs}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.QCReturnDate = formatDateToDefault(masterData.QCReturnDate);
                masterData.QCReqDate = formatDateToDefault(masterData.QCReqDate);
                setFormData($formEl, masterData);
                initChildTable(masterData.YarnQCReturnChilds);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
})();
