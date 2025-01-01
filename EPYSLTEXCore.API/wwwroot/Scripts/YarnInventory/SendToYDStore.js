(function () {
    var menuId, pageName, menuParam;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $tblChildEl, $formEl, $tblChildFabricInfoId, tblMasterId;
    var status = statusConstants.PENDING;
    var isApprovePage = false;
    var tableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    }
    var isEditable = true;
    var masterData;
    var status = statusConstants.PENDING;

    var isRequisitionPage = false,
        isApprovePage = false;

    $(function () {
        if (!menuId)
            menuId = localStorage.getItem("menuId");
        if (!pageName)
            pageName = localStorage.getItem("pageName");
        if (!menuParam)
            menuParam = localStorage.getItem("menuParam");
        var pageId = pageName + "-" + menuId;
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
        $tblChildEl = $(pageConstants.CHILD_TBL_ID_PREFIX + pageId);
        $tblChildFabricInfoId = $("#tblChildFabricInfoId" + pageId);
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);

        //Get data ViewBag variable wise
        //isRequisitionPage = convertToBoolean($(`#${pageId}`).find("#RequisitionPage").val());
        //isApprovePage = convertToBoolean($(`#${pageId}`).find("#ApprovePage").val());
        isRequisitionPage = menuParam === "R" ? true : false;
        isApprovePage = menuParam === "A" ? true : false;

        if (isRequisitionPage) {
            $toolbarEl.find("#btnPendingList,#btnDraftList,#btnPendingApprovalList,#btnApproveList,#btnRejectList").show();

            $formEl.find("#btnSave,#btnSaveAndSend").show();
            $formEl.find("#btnApprove,#btnAcknowledge,#btnReject").hide();

            toggleActiveToolbarBtn($toolbarEl.find("#btnPendingList"), $toolbarEl);

            status = statusConstants.PENDING;
        }
        else if (isApprovePage) {
            $toolbarEl.find("#btnPendingApprovalList,#btnApproveList,#btnRejectList").show();
            $toolbarEl.find("#btnPendingList,#btnDraftList").hide();

            $formEl.find("#btnApprove,#btnReject").show();
            $formEl.find("#btnSave,#btnSaveAndSend,#btnAcknowledge").hide();

            toggleActiveToolbarBtn($toolbarEl.find("#btnPendingApprovalList"), $toolbarEl);

            status = statusConstants.AWAITING_PROPOSE;
        }

        initMasterTable();
        //initChildTable();
        // getMasterTableData();

        $toolbarEl.find("#btnPendingList,#btnDraftList,#btnPendingApprovalList,#btnApproveList,#btnRejectList").on("click", function (e) {
            var ClickBTN = $(this).attr('id');

            if (isRequisitionPage) {
                if (ClickBTN == 'btnPendingList') {

                    e.preventDefault();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.PENDING;
                    initMasterTable();
                    isEditable = true;
                }
                else if (ClickBTN == 'btnDraftList') {
                    e.preventDefault();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.DRAFT;
                    initMasterTable();
                    isEditable = true;
                }
                else if (ClickBTN == 'btnPendingApprovalList') {
                    e.preventDefault();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.AWAITING_PROPOSE;
                    initMasterTable();
                    isEditable = false;
                }
                else if (ClickBTN == 'btnApproveList') {
                    e.preventDefault();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.APPROVED;
                    initMasterTable();
                    isEditable = false;
                }
                else if (ClickBTN == 'btnRejectList') {
                    e.preventDefault();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.REJECT;
                    initMasterTable();
                    isEditable = false;
                }
            }
            else if (isApprovePage) {
                if (ClickBTN == 'btnPendingApprovalList') {
                    e.preventDefault();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.AWAITING_PROPOSE;
                    initMasterTable();
                    isEditable = true;
                }
                else if (ClickBTN == 'btnApproveList') {
                    e.preventDefault();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.APPROVED;
                    initMasterTable();
                    isEditable = false;
                }
                else if (ClickBTN == 'btnRejectList') {
                    e.preventDefault();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.REJECT;
                    initMasterTable();
                    isEditable = false;
                }
            }
        });
        /*
        $toolbarEl.find("#btnPendingList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PENDING;

            initMasterTable();
            // getMasterTableData();
        });

        $toolbarEl.find("#btnDraftList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.DRAFT;

            initMasterTable();
            // getMasterTableData();
        });
        */
        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save();
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
        var columns = [
            {
                headerText: 'Commands', width: 80, visible: status == statusConstants.PENDING, commands: [
                    { type: 'Add', buttonOption: { cssClass: 'e-flat', iconCss: 'e-add e-icons' } }]
            },
            {
                headerText: 'Commands', width: 100, visible: status !== statusConstants.PENDING, commands: [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } }]
            },
            {
                field: 'SendToYDStoreNo', headerText: 'Send To YD Store No', visible: status !== statusConstants.PENDING
            },
            {
                field: 'SendToYDStoreDate', headerText: 'Send To YD Store Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, visible: status !== statusConstants.PENDING
            },
            {
                field: 'YDBatchID', headerText: 'YDBatchID', visible: false
            },
            {
                field: 'YDBatchNo', headerText: 'YD Batch No'
            },
            {
                field: 'YDBatchDate', headerText: 'YD Batch Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1
            },
            {
                field: 'YDBookingNo', headerText: 'YD Booking No'
            },
            {
                field: 'YDBookingDate', headerText: 'YD Booking Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1
            },

        ];

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: false,
            apiEndPoint: `/api/send-to-yd-store/list?status=${status}`,
            columns: columns,
            commandClick: handleCommands
        });
    }

    function handleCommands(args) {
        if (status === statusConstants.PENDING) {
            getNew(args.rowData.YDBatchID);
        }
        else {
            //getDetails(row.Id);
            getDetails(args.rowData.SendToYDStoreMasterID);
        }
    }

    function initChildTable() {
        $tblChildEl.bootstrapTable("destroy");
        $tblChildEl.bootstrapTable({
            uniqueId: 'YDProductionChildID',
            editable: isEditable,
            checkboxHeader: false,
            showFooter: true,
            columns: [
                {
                    field: "YarnCategory",
                    title: "Yarn Details",
                    cellStyle: function () { return { classes: 'm-w-200' } }
                },
                {
                    field: "YarnType",
                    title: "Yarn Type",
                    cellStyle: function () { return { classes: 'm-w-100' } }
                },
                {
                    field: "YarnComposition",
                    title: "Yarn Composition",
                    cellStyle: function () { return { classes: 'm-w-200' } }
                },
                {
                    field: "YarnCount",
                    title: "Yarn Count",
                    cellStyle: function () { return { classes: 'm-w-50' } }
                },
                {
                    field: "YarnColor",
                    title: "Yarn Color",
                    cellStyle: function () { return { classes: 'm-w-150' } }
                },
                {
                    field: "YarnShade",
                    title: "Shade",
                    cellStyle: function () { return { classes: 'm-w-60' } }
                },
                {
                    field: "Uom",
                    title: "Unit",
                    cellStyle: function () { return { classes: 'm-w-60' } }
                },
                {
                    field: "BookingQty",
                    title: "Booking Qty",
                    align: 'center'
                },
                {
                    field: "QCQty",
                    title: "QC Qty",
                    align: 'center',
                    visible: status == statusConstants.PENDING
                },
                {
                    field: "QCCone",
                    title: "QC Cone",
                    align: 'center',
                    visible: status == statusConstants.PENDING
                },
                {
                    field: "QCPacket",
                    title: "QC Packet",
                    align: 'center',
                    visible: status == statusConstants.PENDING
                },
                {
                    field: "BalanceSendQty",
                    title: "Balance Send Qty",
                    align: 'center',
                    visible: status == statusConstants.PENDING
                },
                {
                    field: "BalanceSendConeQty",
                    title: "Balance Send Cone Qty",
                    align: 'center',
                    visible: status == statusConstants.PENDING
                },
                {
                    field: "BalanceSendPacketQty",
                    title: "Balance Send Packet Qty",
                    align: 'center',
                    visible: status == statusConstants.PENDING
                },
                {
                    field: "SendQty",
                    title: "Send Qty",
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
                    field: "SendConeQty",
                    title: "Send Cone Qty",
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
                    field: "SendPacketQty",
                    title: "Send Packet Qty",
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
                    field: "Remarks",
                    title: "Remarks",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "YDQCChildID",
                    title: "YDQCChildID",
                    align: 'center',
                    visible: false
                },
                {
                    field: "YDRICRBId",
                    title: "YDRICRBId",
                    align: 'center',
                    visible: false
                }
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

    function getNew(newId) {
        axios.get(`/api/send-to-yd-store/new/${newId}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                //initChildTable();
                masterData.SendToYDStoreDate = formatDateToDefault(masterData.SendToYDStoreDate);
                masterData.YDBookingDate = formatDateToDefault(masterData.YDBookingDate);
                masterData.YDBatchDate = formatDateToDefault(masterData.YDBatchDate);

                setFormData($formEl, masterData);
                initChildTable();
                $tblChildEl.bootstrapTable("load", masterData.Childs);
                $tblChildEl.bootstrapTable('hideLoading');

                $formEl.find("#btnSave").fadeIn();
                $formEl.find("#btnSaveAndSend").fadeIn();
            })
        //.catch(function (err) {
        //    toastr.error(err.response.data.Message);
        //});
    }

    function getDetails(id) {
        axios.get(`/api/send-to-yd-store/${id}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                initChildTable();
                masterData.SendToYDStoreDate = formatDateToDefault(masterData.SendToYDStoreDate);
                masterData.YDBookingDate = formatDateToDefault(masterData.YDBookingDate);
                masterData.YDBatchDate = formatDateToDefault(masterData.YDBatchDate);

                setFormData($formEl, masterData);
                $tblChildEl.bootstrapTable("load", masterData.Childs);
                $tblChildEl.bootstrapTable('hideLoading');


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
        var data = formDataToJson($formEl.serializeArray());
        data["Childs"] = masterData.Childs;
        data.IsSendForApprove = IsSendForApprove;

        axios.post("/api/send-to-yd-store/save", data)
            .then(function () {
                toastr.success("Saved successfully!");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }
    function SaveProcess(IsApprove, IsReject) {
        var data = formDataToJson($formEl.serializeArray());
        data["Childs"] = masterData.Childs;

        //Validation Set in Master & Child
        initializeValidation($formEl, validationConstraints);
        if (!isValidForm($formEl, validationConstraints)) return toastr.error("Please correct all validation errors!");
        else hideValidationErrors($formEl);

        if (isValidChildForm(data)) return;

        data.IsApprove = IsApprove;
        data.IsReject = IsReject;

        axios.post("/api/send-to-yd-store/save", data)
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
            if (el.SendQty == "" || el.SendQty == null || el.SendQty <= 0) {
                toastr.error("Send Qty is required.");
                isValidItemInfo = true;
            }
            if (el.SendConeQty == "" || el.SendConeQty == null || el.SendConeQty <= 0) {
                toastr.error("Send Cone Qty is required.");
                isValidItemInfo = true;
            }
            if (el.SendPacketQty == "" || el.SendPacketQty == null || el.SendPacketQty <= 0) {
                toastr.error("Send Packet Qty is required.");
                isValidItemInfo = true;
            }

        });

        return isValidItemInfo;
    }

    var validationConstraints = {
        SendToYDStoreDate: {
            presence: true
        }
    }
})();