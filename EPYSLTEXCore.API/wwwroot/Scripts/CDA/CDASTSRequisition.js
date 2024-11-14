(function () {
    var menuId,
        pageName,
        toolbarId,
        pageId;

    var $divTblEl,
        $divDetailsEl,
        $toolbarEl,
        $tblMasterEl,
        $tblChildEl,
        $formEl,
        tblMasterId,
        tblChildId;

    var masterData = {};

    var isRequisitionPage = false,
        isApprovePage = false,
        isAcknowledgePage = false,
        isEditable = false,
        status = statusConstants.PENDING;  
    
    var subGroupId, subGroupName;

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
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);

        $formEl.find("#divRejectReason").hide();

        isRequisitionPage = convertToBoolean($(`#${pageId}`).find("#RequisitionPage").val());
        isApprovePage = convertToBoolean($(`#${pageId}`).find("#ApprovePage").val());
        isAcknowledgePage = convertToBoolean($(`#${pageId}`).find("#AcknowledgePage").val());

        if (isRequisitionPage) {
            $toolbarEl.find("#btnNew,#btnRequisitionList,#btnPendingforApprovalList,#btnApproveList,#btnRejectList,#btnAllList").show();
            $toolbarEl.find("#btnPendingAkgList,#btnAcknowledgementList").hide();

            $formEl.find("#btnSave,#btnSaveAndSend").show();
            $formEl.find("#btnApprove,#btnAcknowledge,#btnReject").hide();

            toggleActiveToolbarBtn($toolbarEl.find("#btnRequisitionList"), $toolbarEl);

            status = statusConstants.PENDING;
            isEditable = false;
        }
        else if (isApprovePage) {
            $toolbarEl.find("#btnPendingforApprovalList,#btnApproveList,#btnRejectList").show();
            $toolbarEl.find("#btnNew,#btnRequisitionList,#btnPendingAkgList,#btnAcknowledgementList,#btnAllList").hide();

            $formEl.find("#btnApprove,#btnReject").show();
            $formEl.find("#btnSave,#btnSaveAndSend,#btnAcknowledge").hide();

            toggleActiveToolbarBtn($toolbarEl.find("#btnPendingforApprovalList"), $toolbarEl);

            status = statusConstants.AWAITING_PROPOSE;
            isEditable = false;
        }
        else if (isAcknowledgePage) {
            $toolbarEl.find("#btnPendingAkgList,#btnAcknowledgementList").show();
            $toolbarEl.find("#btnNew,#btnRequisitionList,#btnPendingforApprovalList,#btnApproveList,#btnRejectList,#btnAllList").hide();

            $formEl.find("#btnAcknowledge").show();
            $formEl.find("#btnSave,#btnSaveAndSend,#btnApprove,#btnReject").hide();

            toggleActiveToolbarBtn($toolbarEl.find("#btnPendingAkgList"), $toolbarEl);

            status = statusConstants.APPROVED;
            isEditable = false;
        } 

        initMasterTable();

        //Button List
        //btnPendingList = PROPOSED
        //btnRequisitionList = PENDING
        //btnPendingForApprovalList = AWAITING_PROPOSE
        //btnApproveList = APPROVED

        $toolbarEl.find("#btnRequisitionList,#btnPendingforApprovalList,#btnApproveList,#btnRejectList,#btnPendingAkgList,#btnAcknowledgementList,#btnAllList").on("click", function (e) {
            var ClickBTN = $(this).attr('id');

            if (isRequisitionPage) { 
                if (ClickBTN == 'btnRequisitionList') {
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
                else if (ClickBTN == 'btnAllList') {
                    e.preventDefault();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.ALL;
                    initMasterTable();
                }
            }
            else if (isApprovePage) {
                if (ClickBTN == 'btnPendingforApprovalList') {
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
                if (ClickBTN == 'btnPendingAkgList') {
                    e.preventDefault();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.APPROVED;
                    initMasterTable();
                }
                if (ClickBTN == 'btnAcknowledgementList') {
                    e.preventDefault();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.ACKNOWLEDGE;
                    initMasterTable();
                }
            }
        }); 
         
        $toolbarEl.find("#btnNew").on("click", showSubGroupSelection); 

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

        if (isRequisitionPage) { 
            if (status == statusConstants.PENDING) {
                columns = [
                    { headerText: 'Commands', width: 80, commands: [{ type: 'Edit', visible: true, buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } }] }];
            }
            else if (status == statusConstants.AWAITING_PROPOSE || status == statusConstants.APPROVED || status == statusConstants.REJECT || status == statusConstants.ALL) {
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
                field: 'STSReqNo', headerText: 'Requisition No'
            },
            {
                field: 'STSReqDate', headerText: 'Requisition Date', type: 'date', format: _ch_date_format_1
            },
            {
                field: 'TotalQty', headerText: 'Total Qty'
            },
            {
                field: 'RequisitionByUser', headerText: 'Requisition By'
            },
            {
                field: 'ApproveByUser', headerText: 'Approved By', visible: status == statusConstants.APPROVED ? true : false
            },
            {
                field: 'AcknowledgeByUser', headerText: 'Acknowledge By', visible: status == statusConstants.ACKNOWLEDGE ? true : false
            },
            {
                field: 'RejectBy', headerText: 'Reject By', visible: status == statusConstants.REJECT ? true : false
            },
            {
                field: 'Remarks', headerText: 'Remarks'
            },
            {
                field: 'Status', headerText: 'Status', visible: status == statusConstants.ALL ? true : false
            }
        ];
        columns.push.apply(columns, additionalColumns);

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = initEJ2Grid({
            tableId: tblMasterId,  
            apiEndPoint: `/api/CDA-STS-requisition/list/${status}/${pageName}`,
            columns: columns,
            autofitColumns: false,
            commandClick: handleCommands
        });
    }
    function handleCommands(args) { 
        getDetails(args.rowData.STSReqMasterID);
    }

    function ej2GridDisplayFormatter_Ex(args, rowData, column) { 
        var text = "";
        var field = args.replace(new RegExp("id", "ig"), ""); 
        if (args.contains("Id")) {
            text = rowData[field + "Desc"];
            if (!text) {
                var segmentValue = masterData[field + "List"].find(function (el) { return el.id == rowData[args] });
                text = segmentValue ? segmentValue.text : "";
            }
        }
        else text = rowData[field + "Name"] || rowData[field + "Names"] || "Empty";

        return text;
    }


    async function initChildTable(data) {
        if ($tblChildEl)
            $tblChildEl.destroy();

        if ((isApprovePage) || (isAcknowledgePage)) {
            isEditable = false;
        }
        else {
            if (status == statusConstants.PENDING) {
                isEditable = true;
            } else {
                isEditable = false;
            } 
        } 

        var columns = [];
        
        columns = [
            { field: 'STSReqChildID', isPrimaryKey: true, visible: false },
            {
                field: 'Segment1ValueId', headerText: 'Item Name', valueAccessor: ej2GridDisplayFormatter_Ex,dataSource: masterData.Segment1ValueList, displayField: "Segment1ValueDesc", edit: ej2GridDropDownObj({
                    
                })

            },
            { field: "Segment1ValueDesc", visible: false }, 
            { field: "text", visible: false },
            { field: 'ReqQty', headerText: 'Req Qty(Kg)', allowEditing: isEditable, edit: { params: { showSpinButton: false, decimals: 0, min: 0 } } },
            { field: 'DisplayUnitDesc', headerText: 'Unit', allowEditing: false },
            { field: 'Remarks', headerText: 'Remarks', allowEditing: isEditable }
        ];
         
        var tableOptions = {
            tableId: tblChildId,
            data: data,
            columns: columns,
            actionBegin: function (args) {
                if (args.requestType === "add") {
                    args.data.STSReqChildID = getMaxIdForArray(masterData.Childs, "STSReqChildID");
                    args.data.DisplayUnitDesc = "Kg";
                }
                else if (args.requestType === "save") {
                    var index = $tblChildEl.getRowIndexByPrimaryKey(args.rowData.STSReqChildID);
                    masterData.Childs[index] = args.data;
                }
            }, 
            autofitColumns: false,
            showDefaultToolbar: false,
            allowFiltering: false,
            allowPaging: false
        };

        if (status == statusConstants.PENDING) {
            tableOptions["toolbar"] = ['Add'];
        }
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
        $formEl.find("#STSReqMasterID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }  
    function showSubGroupSelection() {
        axios.get("/api/selectoption/cda-dyes-chemical")
            .then(function (response) {
                showBootboxSelect2Dialog("Select SubGroup", "ddlSubGroupID", "Choose SubGroup", response.data, function (data) {
                    if (data) {
                        HoldOn.open({
                            theme: "sk-circle"
                        });
                        resetForm();
                        subGroupId = data.id;
                        subGroupName = data.text;
                        $formEl.find("#SubGroupID").val(subGroupId);
                        $formEl.find("#lblSubGroup").text(subGroupName); 
                        getNew(subGroupName);
                        status = statusConstants.PENDING;
                    }
                    else toastr.warning("You must select a supplier.");
                })
            })
            .catch(showResponseError); 
    } 
    function getNew(subGroupName) {
        axios.get(`/api/CDA-STS-requisition/new/${subGroupName}`)
            .then(function (response) {
                isEditable = true;
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                $formEl.find("#btnSave,#btnSaveAndSend").fadeIn();
                $formEl.find("#btnApprove,#btnReject,#btnAkgYPR,#btnAcknowledge").fadeOut();
                $formEl.find("#divRejectReason").hide();
                $formEl.find("#divRemarks").show();
                masterData = response.data;
                masterData.STSReqDate = formatDateToDefault(masterData.STSReqDate);
                masterData.SubGroupID = subGroupId;
                masterData.SubGroupName = subGroupName;
                setFormData($formEl, masterData);
                $formEl.find("#SubGroupID").val(masterData.SubGroupID);
                initChildTable([]);
                IsElementDisable("New"); 
            })
            .catch(showResponseError); 
    } 
    function getDetails(id) {
        var url = `/api/CDA-STS-requisition/${id}`;
        axios.get(url)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.STSReqDate = formatDateToDefault(masterData.STSReqDate); 
                setFormData($formEl, masterData);
                $formEl.find("#lblSubGroup").text(masterData.SubGroupName);
                $formEl.find("#SubGroupID").val(masterData.SubGroupID);
                initChildTable(masterData.Childs);

                if (isRequisitionPage) {
                    if (status == statusConstants.PENDING) {
                        $formEl.find("#btnSave").show();
                        $formEl.find("#btnSaveAndSend").show();
                        $formEl.find("#btnApprove").hide();
                        $formEl.find("#btnAcknowledge").hide();
                        $formEl.find("#btnReject").hide();
                        //Panel
                        $formEl.find("#divRemarks").show();
                        $formEl.find("#divRejectReason").hide();
                        IsElementDisable("New");
                    }
                    else if (status == statusConstants.AWAITING_PROPOSE || status == statusConstants.APPROVED || status == statusConstants.ALL) {
                        $formEl.find("#btnSave").hide();
                        $formEl.find("#btnSaveAndSend").hide();
                        $formEl.find("#btnApprove").hide();
                        $formEl.find("#btnAcknowledge").hide();
                        $formEl.find("#btnReject").hide();
                        //Panel
                        $formEl.find("#divRemarks").show();
                        $formEl.find("#divRejectReason").hide();
                        IsElementDisable("Others"); 
                    }
                    else if (status == statusConstants.REJECT) {
                        $formEl.find("#btnSave").hide();
                        $formEl.find("#btnSaveAndSend").hide();
                        $formEl.find("#btnApprove").hide();
                        $formEl.find("#btnAcknowledge").hide();
                        $formEl.find("#btnReject").hide();
                        //Panel
                        $formEl.find("#divRemarks").hide();
                        $formEl.find("#divRejectReason").show();
                        IsElementDisable("Others");
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
                        $formEl.find("#divRemarks").show();
                        $formEl.find("#divRejectReason").show();
                        IsElementDisable("Others");
                    }
                    else if (status == statusConstants.APPROVED) {
                        $formEl.find("#btnSave").hide();
                        $formEl.find("#btnSaveAndSend").hide();
                        $formEl.find("#btnApprove").hide();
                        $formEl.find("#btnAcknowledge").hide();
                        $formEl.find("#btnReject").hide();
                        //Panel
                        $formEl.find("#divRemarks").show();
                        $formEl.find("#divRejectReason").hide();
                        IsElementDisable("Others");
                    }
                    else if (status == statusConstants.REJECT) {
                        $formEl.find("#btnSave").hide();
                        $formEl.find("#btnSaveAndSend").hide();
                        $formEl.find("#btnApprove").hide();
                        $formEl.find("#btnAcknowledge").hide();
                        $formEl.find("#btnReject").hide();
                        //Panel
                        $formEl.find("#divRemarks").hide();
                        $formEl.find("#divRejectReason").show();
                        IsElementDisable("Others");
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
                        $formEl.find("#divRemarks").show();
                        $formEl.find("#divRejectReason").hide();
                        IsElementDisable("Others");
                    }
                    else if (status == statusConstants.ACKNOWLEDGE) {
                        $formEl.find("#btnSave").hide();
                        $formEl.find("#btnSaveAndSend").hide();
                        $formEl.find("#btnApprove").hide();
                        $formEl.find("#btnAcknowledge").hide();
                        $formEl.find("#btnReject").hide();
                        //Panel
                        $formEl.find("#divRemarks").show();
                        $formEl.find("#divRejectReason").hide();
                        IsElementDisable("Others");
                    }
                }
            })
            .catch(showResponseError);
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
        axios.post("/api/CDA-STS-requisition/save", data)
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

        axios.post("/api/CDA-STS-requisition/saveprocess", data)
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
            if (el.ReqQty == "" || el.ReqQty == null || el.ReqQty <= 0) {
                toastr.error("Req Qty is required.");
                isValidItemInfo = true;
            } 
        });

        return isValidItemInfo;
    }

    var validationConstraints = {
        STSReqNo: {
            presence: true
        },
        STSReqDate: {
            presence: true
        },
        STSReqBy: {
            presence: true
        },
        CompanyID: {
            presence: true
        },
        RCompanyID: {
            presence: true
        }
    }

    function IsElementDisable(flag) {
        if (flag == "New") {
            $formEl.find("#STSReqDate").prop("disabled", false);
            $formEl.find("#CompanyID").prop("disabled", false);
            $formEl.find("#STSReqBy").prop("disabled", false);
            $formEl.find("#RCompanyID").prop("disabled", false);
            $formEl.find("#Remarks").prop("disabled", false);
            $formEl.find("#RejectReason").prop("disabled", true);
        }
        else {
            $formEl.find("#STSReqDate").prop("disabled", true);
            $formEl.find("#CompanyID").prop("disabled", true);
            $formEl.find("#STSReqBy").prop("disabled", true);
            $formEl.find("#RCompanyID").prop("disabled", true);
            if (isApprovePage == true && status == statusConstants.AWAITING_PROPOSE) {
                $formEl.find("#Remarks").prop("disabled", false);
            } else {
                $formEl.find("#Remarks").prop("disabled", true);
            }
           
            $formEl.find("#RejectReason").prop("disabled", false);
        }
    }
})();