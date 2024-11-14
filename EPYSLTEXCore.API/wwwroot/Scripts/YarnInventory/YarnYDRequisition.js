(function () {
    var menuId,
        pageName;
    var toolbarId,
        pageId;
    var status = statusConstants.PROPOSED; 
    var $divTblEl,
        $divDetailsEl,
        $toolbarEl,
        $tblMasterEl,
        $tblChildEl,
        $formEl,
        tblMasterId,
        $tblColorChildEl,
        tblChildId,
        $tblChildTwistingEl,
        tblChildTwistingId;
    var masterData,
        currentChildRowData;
    var copiedRecord = null;
    var isAddItem = false;
    var isRequisitionPage = false, 
        isApprovePage = false,
        isAcknowledgePage = false;

    $(function () {
        if (!menuId)
            menuId = localStorage.getItem("menuId");
        if (!pageName)
            pageName = localStorage.getItem("pageName"); 

        pageId = pageName + "-" + menuId;
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
        tblChildId = pageConstants.CHILD_TBL_ID_PREFIX + pageId;
        tblChildTwistingId = "#tblChildTwisting" + pageId;
        $tblColorChildEl = $("#tblColorChild" + pageId);
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);

        //Get data ViewBag variable wise 
        isRequisitionPage = convertToBoolean($(`#${pageId}`).find("#RequisitionPage").val()); 
        isApprovePage = convertToBoolean($(`#${pageId}`).find("#ApprovePage").val());
        isAcknowledgePage = convertToBoolean($(`#${pageId}`).find("#AcknowledgePage").val()); 
         
        if (isRequisitionPage) { 
            $toolbarEl.find("#btnPendingList,#btnRequisitionList,#btnPendingforApprovalList,#btnApproveList,#btnRejectList").show();
            $toolbarEl.find("#btnPendingApprovalList,#btnYDYarnRequisitonList,#btnAcknowledgeList").hide();

            $formEl.find("#btnSave,#btnSaveAndSend").show();
            $formEl.find("#btnApprove,#btnAcknowledge,#btnReject").hide();

            toggleActiveToolbarBtn($toolbarEl.find("#btnPendingList"), $toolbarEl);

            status = statusConstants.PROPOSED;
            isEditable = false; 
        }
        else if (isApprovePage) { 
            $toolbarEl.find("#btnPendingApprovalList,#btnApproveList,#btnRejectList").show();
            $toolbarEl.find("#btnPendingList,#btnRequisitionList,#btnPendingforApprovalList,#btnYDYarnRequisitonList,#btnAcknowledgeList").hide();

            $formEl.find("#btnApprove,#btnReject").show();
            $formEl.find("#btnSave,#btnSaveAndSend,#btnAcknowledge").hide(); 

            toggleActiveToolbarBtn($toolbarEl.find("#btnPendingApprovalList"), $toolbarEl);

            status = statusConstants.AWAITING_PROPOSE;
            isEditable = false; 
        }
        else if (isAcknowledgePage) {
            $toolbarEl.find("#btnYDYarnRequisitonList,#btnAcknowledgeList").show();
            $toolbarEl.find("#btnPendingList,#btnRequisitionList,#btnPendingforApprovalList,#btnApproveList,#btnPendingApprovalList,#btnRejectList").hide();

            $formEl.find("#btnAcknowledge").show();
            $formEl.find("#btnSave,#btnSaveAndSend,#btnApprove,#btnReject").hide();
            
            toggleActiveToolbarBtn($toolbarEl.find("#btnYDYarnRequisitonList"), $toolbarEl);

            status = statusConstants.APPROVED;
            isEditable = false;
        }
        //Load Event  
        initMasterTable();

        //Button List
        //btnPendingList = PROPOSED
        //btnRequisitionList = PENDING
        //btnPendingForApprovalList = AWAITING_PROPOSE
        //btnApproveList = APPROVED

        $toolbarEl.find("#btnPendingList,#btnRequisitionList,#btnPendingforApprovalList,#btnPendingApprovalList,#btnApproveList,#btnYDYarnRequisitonList,#btnAcknowledgeList,#btnRejectList").on("click", function (e) {
            var ClickBTN = $(this).attr('id');

            if (isRequisitionPage) {
                if (ClickBTN == 'btnPendingList') {
                    e.preventDefault();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.PROPOSED;
                    initMasterTable();
                }
                else if (ClickBTN == 'btnRequisitionList') {
                    e.preventDefault();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.PENDING;
                    initMasterTable();
                }
                else if (ClickBTN == 'btnPendingforApprovalList') {
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
            else if (isAcknowledgePage) {
                if (ClickBTN == 'btnYDYarnRequisitonList') {
                    e.preventDefault();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.APPROVED;
                    initMasterTable();
                }
                if (ClickBTN == 'btnAcknowledgeList') {
                    e.preventDefault();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.ACKNOWLEDGE;
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
                IsAcknowledge = false,
                IsReject = false;

            if (ClickBTN == 'btnApprove') {
                e.preventDefault();
                IsApprove = true;
                SaveProcess(IsApprove, IsAcknowledge, IsReject)
            }
            else if (ClickBTN == 'btnAcknowledge') {
                e.preventDefault();
                IsAcknowledge = true;
                SaveProcess(IsApprove, IsAcknowledge, IsReject)
            }
            else if (ClickBTN == 'btnReject') {
                e.preventDefault();
                IsReject = true;
                SaveProcess(IsApprove, IsAcknowledge, IsReject)
            }
        });

        $formEl.find("#btnCancel").on("click", backToList);
    }); 

    function initMasterTable() {
        var columns = [];
        if (status == statusConstants.PROPOSED) { // Data get to the booking table
            columns = [
                {
                    headerText: 'Commands', width: 80, commands: [
                        { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } }]
                },
                {
                    field: 'YDBookingNo', headerText: 'YD Booking No', width: 80
                },
                {
                    field: 'YDBookingDate', headerText: 'YD Booking Date', type: 'date', format: _ch_date_format_1, width: 80
                },
                {
                    field: 'ConceptNo', headerText: 'Concept No', width: 80
                },
                {
                    field: 'Remarks', headerText: 'Remarks'
                },
                {
                    field: 'TotalBookingQty', headerText: 'Total Booking Qty', width: 80
                }
            ];
        }
        else {
            if (isRequisitionPage) {
                if (status == statusConstants.PENDING) {
                    columns = [
                        { headerText: 'Commands', width: 80, commands: [{ type: 'Edit', visible: true, buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } }] }];
                }
                if (status == statusConstants.AWAITING_PROPOSE || status == statusConstants.APPROVED || status == statusConstants.REJECT) {
                    columns = [
                        { headerText: 'Commands', width: 80, commands: [{ type: 'View', visible: true, buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } }] }];
                }
            }
            else if (isApprovePage) {
                if (status == statusConstants.AWAITING_PROPOSE) {
                    columns = [
                        { headerText: 'Commands', width: 80, commands: [{ type: 'Edit', visible: true, buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } }] }];
                }
                if (status == statusConstants.APPROVED || status == statusConstants.REJECT) {
                    columns = [
                        { headerText: 'Commands', width: 80, commands: [{ type: 'View', visible: true, buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } }] }];
                }
            }
            else if (isAcknowledgePage) {
                if (status == statusConstants.APPROVED) {
                    columns = [
                        { headerText: 'Commands', width: 80, commands: [{ type: 'Edit', visible: true, buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } }] }];
                }
                if (status == statusConstants.ACKNOWLEDGE) {
                    columns = [
                        { headerText: 'Commands', width: 80, commands: [{ type: 'View', visible: true, buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } }] }];
                }
            }

            var additionalColumns = [
                {
                    field: 'YDReqNo', headerText: 'Requisition No'
                },
                {
                    field: 'YDReqDate', headerText: 'Requisition Date', type: 'date', format: _ch_date_format_1
                },
                {
                    field: 'BuyerName', headerText: 'Buyer'
                },
                {
                    field: 'RequiredQty', headerText: 'Required Qty'
                },
                {
                    field: 'RequsitionQty', headerText: 'Requsition Qty'
                },
                {
                    field: 'SendForApproveName', headerText: 'Send For Approve By'
                },
                {
                    field: 'ApproveName', headerText: 'Approved By'
                },
                {
                    field: 'AcknowledgeBy', headerText: 'Acknowledge By'
                },
                {
                    field: 'RejectBy', headerText: 'Reject By'
                },
                {
                    field: 'Remarks', headerText: 'Remarks'
                }
            ];
            columns.push.apply(columns, additionalColumns);
        }

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = initEJ2Grid({
            tableId: tblMasterId,
            apiEndPoint: `/api/yarn-yd-req/list/${status}/${pageName}`,
            columns: columns,
            autofitColumns: false,
            commandClick: handleCommands
        });
    }
     
    function handleCommands(args) {
        
        if (status === statusConstants.PROPOSED) {
            getNew(args.rowData.YDBookingMasterID);
        }
        else {
            getDetails(args.rowData.YDReqMasterID);
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
                        { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                        { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }
                    ]
                },
            );
            columns.push.apply(columns, await getYarnItemColumnsForDisplayOnly());
            var additionalColumns = [
                { field: 'ShadeCode', headerText: 'Shade Code', allowEditing: false, visible: false },
                { field: 'NoOfThread', headerText: 'No of Thread', allowEditing: false, visible: false },
                { field: 'PhysicalCount', headerText: 'Physical Count' },
                { field: 'IsAdditionalItem', headerText: 'Additional Item?', allowEditing: false, visible: false, displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center' },
                { field: 'ColorID', headerText: 'Color', allowEditing: false, visible: false },
                { field: 'ColorCode', headerText: 'Color Code', allowEditing: false, visible: false },
                { field: 'BookingFor', headerText: 'Booking For', allowEditing: false, visible: false },
                { field: 'IsTwisting', headerText: 'Twisting?', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center', visible: false },
                { field: 'IsWaxing', headerText: 'Waxing?', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center', visible: false },
                { field: 'RequiredQty', headerText: 'Required Qty', allowEditing: false },
                { field: 'RequsitionQty', headerText: 'Requsition Qty', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 1 } } },
                { field: 'NoOfCone', headerText: 'No Of Cone (Pcs)', visible: false, allowEditing: false, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 1 } } },
                { field: 'DisplayUnitDesc', headerText: 'Unit', allowEditing: false, visible: false },
                { field: 'Remarks', headerText: 'Remarks' }
            ];
        }
        else if ((isApprovePage) || (isAcknowledgePage)) { 
            columns.push.apply(columns, await getYarnItemColumnsForDisplayOnly());
            var additionalColumns = [
                { field: 'ShadeCode', headerText: 'Shade Code', allowEditing: false, visible: false },
                { field: 'NoOfThread', headerText: 'No of Thread', allowEditing: false, visible: false },
                { field: 'PhysicalCount', headerText: 'Physical Count', allowEditing: false },
                { field: 'IsAdditionalItem', headerText: 'Additional Item?', allowEditing: false, visible: false, displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center' },
                { field: 'ColorID', headerText: 'Color', allowEditing: false, visible: false },
                { field: 'ColorCode', headerText: 'Color Code', allowEditing: false, visible: false },
                { field: 'BookingFor', headerText: 'Booking For', allowEditing: false, visible: false },
                { field: 'IsTwisting', headerText: 'Twisting?', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center', visible: false },
                { field: 'IsWaxing', headerText: 'Waxing?', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center', visible: false },
                { field: 'RequiredQty', headerText: 'Required Qty', allowEditing: false },
                { field: 'RequsitionQty', headerText: 'Requsition Qty', allowEditing: false, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 1 } } },
                { field: 'NoOfCone', headerText: 'No Of Cone (Pcs)', visible: false, allowEditing: false, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 1 } } },
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
                //else if (args.requestType === "save") {
                //    var index = $tblChildEl.getRowIndexByPrimaryKey(args.rowData.Id);
                //    args.data.Spinner = args.rowData.Spinner;
                //    args.data.BrandName = args.rowData.BrandName;
                //    masterData.Childs[index] = args.data;
                //}
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
        axios.get(`/api/yarn-yd-req/new/${id}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut(); 
                copiedRecord = null;
                masterData = response.data;
                masterData.YDReqNo = masterData.YDReqNo;
                masterData.YDReqDate = formatDateToDefault(masterData.YDReqDate);
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
        axios.get(`/api/yarn-yd-req/${id}`)
            .then(function (response) {
                
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                copiedRecord = null;
                masterData = response.data;
                masterData.YDReqNo = masterData.YDReqNo;
                masterData.YDReqDate = formatDateToDefault(masterData.YDReqDate);
                /* masterData.FabricBookingIds = masterData.FabricBookingIds.split(',').map(function (item) { return item.trim() });*/
                setFormData($formEl, masterData); 
                initChildTable(masterData.Childs);

                if (isRequisitionPage) {
                    if (status == statusConstants.PROPOSED || status == statusConstants.PENDING) { 
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
                else if (isAcknowledgePage) {
                    if (status == statusConstants.APPROVED) {
                        $formEl.find("#btnSave").hide();
                        $formEl.find("#btnSaveAndSend").hide();
                        $formEl.find("#btnApprove").hide();
                        $formEl.find("#btnAcknowledge").show();
                        $formEl.find("#btnReject").hide();
                        //Panel
                        $formEl.find("#pnlRemarks").show();
                        $formEl.find("#pnlRejectReason").hide();
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
        axios.post("/api/yarn-yd-req/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(showResponseError);
    }
    function SaveProcess(IsApprove, IsAcknowledge, IsReject) {
        var data = formDataToJson($formEl.serializeArray());
        data.IsApprove = IsApprove;
        data.IsAcknowledge = IsAcknowledge;
        data.IsReject = IsReject;

        axios.post("/api/yarn-yd-req/saveprocess", data)
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
            if (el.RequsitionQty == "" || el.RequsitionQty == null || el.RequsitionQty <= 0) {
                toastr.error("Requsition Qty is required.");
                isValidItemInfo = true;
            }
            else if (el.RequsitionQty > el.RequiredQty) {
                toastr.error("Requisition Qty must be equal or less than Required Qty");
                isValidItemInfo = true;
            } 
        });

        return isValidItemInfo;
    }

    var validationConstraints = {
        ReqFromID: {
            presence: true
        },
        YDReqDate: {
            presence: true 
        } 
    } 
})();