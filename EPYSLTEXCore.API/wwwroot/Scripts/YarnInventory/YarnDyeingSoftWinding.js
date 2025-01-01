(function () {
    var menuId,
        pageName, menuParam;
    var toolbarId,
        pageId;
    var status = statusConstants.PENDING;
    var $divTblEl,
        $divDetailsEl,
        $toolbarEl,
        $tblMasterEl,
        $tblChildEl,
        $formEl,
        tblMasterId,
        tblChildId;
    var masterData;

    var isRequisitionPage = false,
        isApprovePage = false;

    $(function () {
        if (!menuId)
            menuId = localStorage.getItem("menuId");
        if (!pageName)
            pageName = localStorage.getItem("pageName");
        if (!menuParam)
            menuParam = localStorage.getItem("menuParam");

        if (menuParam == 'R') {
            isApprovePage = false;
            isRequisitionPage = true;
        }
        else if (menuParam == 'A') { isRequisitionPage = true; isApprovePage = true; }
            
        else {
            isRequisitionPage = false;
            isApprovePage = false;
        }

        pageId = pageName + "-" + menuId;
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
        tblChildId = pageConstants.CHILD_TBL_ID_PREFIX + pageId;
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);

        //Get data ViewBag variable wise 
        //isRequisitionPage = convertToBoolean($(`#${pageId}`).find("#RequisitionPage").val());
        //isApprovePage = convertToBoolean($(`#${pageId}`).find("#ApprovePage").val());

        if (isRequisitionPage) {
            $toolbarEl.find("#btnPendingList,#btnDraftList,#btnPendingApprovalList,#btnApproveList,#btnRejectList").show();

            $formEl.find("#btnSave,#btnSaveAndSend").show();
            $formEl.find("#btnApprove,#btnAcknowledge,#btnReject").hide();
            $toolbarEl.find("#btnPendingList").click();
            toggleActiveToolbarBtn($toolbarEl.find("#btnPendingList"), $toolbarEl);

            status = statusConstants.PENDING;
            isEditable = false;
        }
        else if (isApprovePage) {
           
            $toolbarEl.find("#btnPendingApprovalList,#btnApproveList,#btnRejectList").show();
            $toolbarEl.find("#btnPendingList,#btnDraftList").hide();

            $formEl.find("#btnApprove,#btnReject").show();
            $formEl.find("#btnSave,#btnSaveAndSend,#btnAcknowledge").hide();

            toggleActiveToolbarBtn($toolbarEl.find("#btnPendingApprovalList"), $toolbarEl);
            $toolbarEl.find("#btnPendingApprovalList").click();
            status = statusConstants.AWAITING_PROPOSE;
            isEditable = false;
        }
        //Load Event  
        initMasterTable();


        $toolbarEl.find("#btnPendingList,#btnDraftList,#btnPendingApprovalList,#btnApproveList,#btnRejectList").on("click", function (e) {
            var ClickBTN = $(this).attr('id');
            
            if (isRequisitionPage) {
                if (ClickBTN == 'btnPendingList') {

                    e.preventDefault();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.PENDING;
                    initMasterTable();
                }
                else if (ClickBTN == 'btnDraftList') {
                    e.preventDefault();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.DRAFT;
                    initMasterTable();
                }
                else if (ClickBTN == 'btnPendingApprovalList') {
                    e.preventDefault();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.AWAITING_PROPOSE;
                    initMasterTable();
                }
                else if (ClickBTN == 'btnApproveList') {
                    e.preventDefault();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.APPROVED;
                    initMasterTable();
                }
                else if (ClickBTN == 'btnRejectList') {
                    e.preventDefault();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.REJECT;
                    initMasterTable();
                }
            }
            else if (isApprovePage) {
                if (ClickBTN == 'btnPendingApprovalList') {
                    e.preventDefault();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.AWAITING_PROPOSE;
                    initMasterTable();
                }
                else if (ClickBTN == 'btnApproveList') {
                    e.preventDefault();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.APPROVED;
                    initMasterTable();
                }
                else if (ClickBTN == 'btnRejectList') {
                    e.preventDefault();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.REJECT;
                    initMasterTable();
                }
            }
        });

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save(false);
        });

        $formEl.find("#btnSaveAndSend").click(function (e) {
            e.preventDefault();
            save(true);
        });

        $formEl.find("#btnApprove,#btnAcknowledge,#btnReject").click(function (e) {
            var ClickBTN = $(this).attr('id');

            var IsApprove = false,
                IsReject = false;

            if (ClickBTN == 'btnApprove') {
                e.preventDefault();
                IsApprove = true;
                SaveProcess(IsApprove, IsReject)
            }
            else if (ClickBTN == 'btnAcknowledge') {
                e.preventDefault();
                IsAcknowledge = true;
                SaveProcess(IsApprove, IsReject)
            }
            else if (ClickBTN == 'btnReject') {
                e.preventDefault();
                IsReject = true;
                SaveProcess(IsApprove, IsReject)
            }
        });

        $formEl.find("#btnCancel").on("click", backToList);
    });

    function initMasterTable() {
        var columns = [];
        if (status == statusConstants.PENDING) { // Data get to the booking table
            columns = [
                {
                    headerText: 'Commands', width: 80, commands: [
                        { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-plus' } }]
                },
                {
                    field: 'YDBatchID', headerText: 'YDBatchID', width: 100, visible: false
                },
                {
                    field: 'YDBatchNo', headerText: 'YD Batch No', width: 100
                },
                {
                    field: 'YDBatchDate', headerText: 'YD Batch Date', type: 'date', format: _ch_date_format_1, width: 80
                },
                {
                    field: 'YDBookingNo', headerText: 'YD Booking No', width: 100
                },
                {
                    field: 'YDBookingDate', headerText: 'YD Booking Date', type: 'date', format: _ch_date_format_1, width: 80
                },
                {
                    field: 'ConceptNo', headerText: 'Concept No', width: 100
                },
                {
                    field: 'TotalBookingQty', headerText: 'Total Booking Qty', width: 100
                },
                {
                    field: 'ReceiveQty', headerText: 'Receive Qty', width: 150
                },
                {
                    field: 'Qty', headerText: 'Soft Winding Qty', width: 150
                },
                //{
                //    field: 'Remarks', headerText: 'Remarks'
                //}
            ];
        }
        else {
            if (isRequisitionPage) {
                if (status == statusConstants.PENDING || status == statusConstants.DRAFT) {
                    columns = [
                        { headerText: 'Commands', width: 100, commands: [{ type: 'Edit', visible: true, buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-plus' } }] }];
                }
                if (status == statusConstants.AWAITING_PROPOSE || status == statusConstants.APPROVED || status == statusConstants.REJECT) {
                    columns = [
                        { headerText: 'Commands', width: 100, commands: [{ type: 'View', visible: true, buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } }] }];
                }
            }
            else if (isApprovePage) {
                if (status == statusConstants.AWAITING_PROPOSE) {
                    columns = [
                        { headerText: 'Commands', width: 100, commands: [{ type: 'Edit', visible: true, buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-plus' } }] }];
                }
                if (status == statusConstants.APPROVED || status == statusConstants.REJECT) {
                    columns = [
                        { headerText: 'Commands', width: 100, commands: [{ type: 'View', visible: true, buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } }] }];
                }
            }

            var additionalColumns = [
                {
                    field: 'SoftWindingNo', headerText: 'Soft Winding No', width: 150
                },
                {
                    field: 'SoftWindingDate', headerText: 'Soft Winding Date', type: 'date', format: _ch_date_format_1, width: 150
                },
                {
                    field: 'BuyerName', headerText: 'Buyer', width: 150
                },
                //{
                //    field: 'ReceiveQty', headerText: 'Receive Qty', width: 150
                //},
                {
                    field: 'Qty', headerText: 'Soft Winding Qty', width: 150
                },
                //{
                //    field: 'SendForApproveName', headerText: 'Send For Approve By'
                //},
                //{
                //    field: 'ApproveName', headerText: 'Approved By'
                //},
                //{
                //    field: 'AcknowledgeBy', headerText: 'Acknowledge By'
                //},
                //{
                //    field: 'RejectName', headerText: 'Reject By'
                //},
                {
                    field: 'Remarks', headerText: 'Remarks'
                }
            ];
            columns.push.apply(columns, additionalColumns);
        }

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = initEJ2Grid({
            tableId: tblMasterId,
            apiEndPoint: `/api/yarn-dyeing-soft-winding/list/${status}/${pageName}`,
            columns: columns,
            autofitColumns: false,
            commandClick: handleCommands
        });
    }

    function handleCommands(args) {

        if (status === statusConstants.PENDING) {
            getNew(args.rowData.YDBatchID);
        }
        else {
            getDetails(args.rowData.SoftWindingMasterID);
        }
    }

    async function initChildTable(data) {
        isEditable = true;
        if ($tblChildEl) $tblChildEl.destroy();
        var columns = [];
        if (isRequisitionPage) {
            columns.push(
                {
                    headerText: 'Commands', width: 120, commands: [
                        {
                            type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' }
                        },
                        { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                        { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                        { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }
                    ]
                },
            );
            columns.push.apply(columns, await getYarnItemColumnsForDisplayOnly());
            var additionalColumns = [
                { field: 'YDBItemReqID', headerText: 'YDBItemReqID', allowEditing: false, visible: false },
                { field: 'ShadeCode', headerText: 'Shade Code', allowEditing: false, visible: false },
                { field: 'NoOfThread', headerText: 'No of Thread', allowEditing: false, visible: false },
                { field: 'ColorID', headerText: 'Color', allowEditing: false, visible: false },
                { field: 'ColorName', headerText: 'Color', allowEditing: false, visible: true },
                { field: 'BookingFor', headerText: 'Booking For', allowEditing: false, visible: false },
                { field: 'ReceiveQty', headerText: 'Receive Qty', allowEditing: false },
                { field: 'ReceiveCone', headerText: 'Receive Cone', allowEditing: false },
                { field: 'ReceiveCarton', headerText: 'ReceiveCarton', allowEditing: false },
                { field: 'Qty', headerText: 'Soft Winding Qty', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 2, min: 1 } } },
                { field: 'Cone', headerText: 'No Of Cone (Pcs)', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 1 } } },
                { field: 'DisplayUnitDesc', headerText: 'Unit', allowEditing: false, visible: false },
                { field: 'Remarks', headerText: 'Remarks' },
                { field: 'YDReceiveChildID', headerText: 'YDReceiveChildID', allowEditing: false, visible: false },
                { field: 'YDRICRBId', headerText: 'YDRICRBId', allowEditing: false, visible: false },
            ];
        }
        else if ((isApprovePage)) {
            columns.push.apply(columns, await getYarnItemColumnsForDisplayOnly());
            var additionalColumns = [
                { field: 'YDBItemReqID', headerText: 'YDBItemReqID', allowEditing: false, visible: false },
                { field: 'ShadeCode', headerText: 'Shade Code', allowEditing: false, visible: false },
                { field: 'NoOfThread', headerText: 'No of Thread', allowEditing: false, visible: false },
                { field: 'ColorID', headerText: 'Color', allowEditing: false, visible: false },
                { field: 'ColorName', headerText: 'Color', allowEditing: false },
                { field: 'BookingFor', headerText: 'Booking For', allowEditing: false, visible: false },
                { field: 'ReceiveQty', headerText: 'Receive Qty', allowEditing: false },
                { field: 'ReceiveCone', headerText: 'Receive Cone', allowEditing: false },
                { field: 'ReceiveCarton', headerText: 'ReceiveCarton', allowEditing: false },
                { field: 'Qty', headerText: 'Soft Winding Qty', allowEditing: true, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 2, min: 1 } } },
                { field: 'Cone', headerText: 'No Of Cone (Pcs)', allowEditing: true, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 1 } } },
                { field: 'DisplayUnitDesc', headerText: 'Unit', allowEditing: false, visible: false },
                { field: 'Remarks', headerText: 'Remarks', allowEditing: false }
            ];
        }

        columns.push.apply(columns, additionalColumns);

        var tableOptions = {
            tableId: tblChildId,
            data: data,
            columns: columns,
            actionBegin: function (args) {
                if (args.requestType === "add") {

                    args.data.Id = getMaxIdForArray(masterData.Childs, "Id");
                    args.data.ItemMasterID = getMaxIdForArray(masterData.Childs, "ItemMasterID");
                    args.data.DisplayUnitDesc = "Kg";
                }
            },
            //commandClick: childCommandClick,
            autofitColumns: false,
            showDefaultToolbar: false,
            allowFiltering: false,
            allowPaging: false
        };
        tableOptions["editSettings"] = { allowAdding: true, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true };
        $tblChildEl = new initEJ2Grid(tableOptions);
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
        $formEl.find("#Id").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function getNew(id) {
        axios.get(`/api/yarn-dyeing-soft-winding/new/${id}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.SoftWindingNo = masterData.SoftWindingNo;
                masterData.SoftWindingDate = formatDateToDefault(masterData.SoftWindingDate);
                setFormData($formEl, masterData);
                initChildTable(masterData.Childs);
                $formEl.find("#btnSave").fadeIn();
                $formEl.find("#btnSaveAndSend").fadeIn();
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getDetails(id) {
        axios.get(`/api/yarn-dyeing-soft-winding/${id}`)
            .then(function (response) {

                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.SoftWindingNo = masterData.SoftWindingNo;
                masterData.SoftWindingDate = formatDateToDefault(masterData.SoftWindingDate);
                /* masterData.FabricBookingIds = masterData.FabricBookingIds.split(',').map(function (item) { return item.trim() });*/
                setFormData($formEl, masterData);
                initChildTable(masterData.Childs);

                if (isRequisitionPage) {
                    if (status == statusConstants.PENDING) {
                        $formEl.find("#btnSave").show();
                        $formEl.find("#btnSaveAndSend").show();
                        $formEl.find("#btnApprove").hide();
                        $formEl.find("#btnAcknowledge").hide();
                        $formEl.find("#btnReject").hide();
                        //Panel
                        $formEl.find("#pnlRemarks").show();
                        $formEl.find("#pnlRejectReason").hide();
                    }
                    else if (status == statusConstants.AWAITING_PROPOSE || status == statusConstants.APPROVED) {
                        $formEl.find("#btnSave").hide();
                        $formEl.find("#btnSaveAndSend").hide();
                        $formEl.find("#btnApprove").hide();
                        $formEl.find("#btnAcknowledge").hide();
                        $formEl.find("#btnReject").hide();
                        //Panel
                        $formEl.find("#pnlRemarks").show();
                        $formEl.find("#pnlRejectReason").hide();
                    }
                    else if (status == statusConstants.REJECT) {
                        $formEl.find("#btnSave").hide();
                        $formEl.find("#btnSaveAndSend").hide();
                        $formEl.find("#btnApprove").hide();
                        $formEl.find("#btnAcknowledge").hide();
                        $formEl.find("#btnReject").hide();
                        //Panel
                        $formEl.find("#pnlRemarks").hide();
                        $formEl.find("#pnlRejectReason").show();
                    }
                }
                else if (isApprovePage) {
                    if (status == statusConstants.AWAITING_PROPOSE) {
                        $formEl.find("#btnSave").hide();
                        $formEl.find("#btnSaveAndSend").hide();
                        $formEl.find("#btnApprove").show();
                        $formEl.find("#btnAcknowledge").hide();
                        $formEl.find("#btnReject").show();
                        //Panel
                        $formEl.find("#pnlRemarks").show();
                        $formEl.find("#pnlRejectReason").show();
                    }
                    else if (status == statusConstants.APPROVED) {
                        $formEl.find("#btnSave").hide();
                        $formEl.find("#btnSaveAndSend").hide();
                        $formEl.find("#btnApprove").hide();
                        $formEl.find("#btnAcknowledge").hide();
                        $formEl.find("#btnReject").hide();
                        //Panel
                        $formEl.find("#pnlRemarks").show();
                        $formEl.find("#pnlRejectReason").hide();
                    }
                    else if (status == statusConstants.REJECT) {
                        $formEl.find("#btnSave").hide();
                        $formEl.find("#btnSaveAndSend").hide();
                        $formEl.find("#btnApprove").hide();
                        $formEl.find("#btnAcknowledge").hide();
                        $formEl.find("#btnReject").hide();
                        //Panel
                        $formEl.find("#pnlRemarks").hide();
                        $formEl.find("#pnlRejectReason").show();
                    }
                }
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function save(IsSendForApprove) {
        //Data get for save process
        var data = formDataToJson($formEl.serializeArray());
        data["Childs"] = $tblChildEl.getCurrentViewRecords();

        //Validation Set in Master & Child
        initializeValidation($formEl, validationConstraints);
        if (!isValidForm($formEl, validationConstraints)) return toastr.error("Please correct all validation errors!");
        else hideValidationErrors($formEl);

        if (isValidChildForm(data)) return;

        //Data send to controller
        data.IsSendForApprove = IsSendForApprove;
        axios.post("/api/yarn-dyeing-soft-winding/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(showResponseError);
    }
    function SaveProcess(IsApprove, IsReject) {
        var data = formDataToJson($formEl.serializeArray());
        data["Childs"] = $tblChildEl.getCurrentViewRecords();

        //Validation Set in Master & Child
        initializeValidation($formEl, validationConstraints);
        if (!isValidForm($formEl, validationConstraints)) return toastr.error("Please correct all validation errors!");
        else hideValidationErrors($formEl);

        if (isValidChildForm(data)) return;

        data.IsApprove = IsApprove;
        data.IsReject = IsReject;

        axios.post("/api/yarn-dyeing-soft-winding/save", data)
            .then(function () {
                toastr.success("Save Process Successfully!");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }
    function isValidChildForm(data) {
        var isValidItemInfo = false;

        $.each(data["Childs"], function (i, el) {
            if (el.Qty == "" || el.Qty == null || el.Qty <= 0) {
                toastr.error("Soft Winding Qty is required.");
                isValidItemInfo = true;
            }
            else if (el.Qty > el.ReceivedQty) {
                toastr.error("Soft Winding Qty must be equal or less than Receive Qty");
                isValidItemInfo = true;
            }
        });

        return isValidItemInfo;
    }

    var validationConstraints = {
        SoftWindingDate: {
            presence: true
        }
    }
})();