(function () {
    var menuId,
        pageName;
    var toolbarId,
        pageId; 

    var $divTblEl,
        $divDetailsEl,
        $toolbarEl,
        $tblMasterEl,
        $tblChildEl,
        $formEl,
        tblMasterId,
        tblChildId; 

    var masterData;
    var isIssuePage = false,
        isApprovePage = false, 
        isEditable = false,
        status = statusConstants.PROPOSED;

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

        isIssuePage = convertToBoolean($(`#${pageId}`).find("#IssuePage").val());
        isApprovePage = convertToBoolean($(`#${pageId}`).find("#ApprovePage").val());
        
        if (isIssuePage) {
            $toolbarEl.find("#btnPendingList,#btnIssueList,#btnApprovedList,#btnRejectList,#btnAllList").show();
            $toolbarEl.find("#btnPendingforApprovalList").hide();

            $formEl.find("#btnSave").show();
            $formEl.find("#btnApprove,#btnReject").hide();

            toggleActiveToolbarBtn($toolbarEl.find("#btnPendingList"), $toolbarEl);

            status = statusConstants.PROPOSED;
            isEditable = false;
        }
        else if (isApprovePage) {
            $toolbarEl.find("#btnPendingforApprovalList,#btnApprovedList,#btnRejectList").show();
            $toolbarEl.find("#btnPendingList,#btnIssueList,#btnAllList").hide();

            $formEl.find("#btnApprove,#btnReject").show();
            $formEl.find("#btnSave").hide();

            toggleActiveToolbarBtn($toolbarEl.find("#btnPendingforApprovalList"), $toolbarEl);

            status = statusConstants.AWAITING_PROPOSE;
            isEditable = false;
        } 

        initMasterTable(); 

        $toolbarEl.find("#btnPendingList,#btnIssueList,#btnPendingforApprovalList,#btnApprovedList,#btnRejectList,#btnAllList").on("click", function (e) {
            var ClickBTN = $(this).attr('id');
            
            if (isIssuePage) { 
                if (ClickBTN == 'btnPendingList') {
                    e.preventDefault();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.PROPOSED;
                    initMasterTable();
                }
                else if (ClickBTN == 'btnIssueList') {
                    e.preventDefault();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.AWAITING_PROPOSE;
                    initMasterTable();
                }
                //else if (ClickBTN == 'btnPendingforApprovalList') {
                //    e.preventDefault();
                //    toggleActiveToolbarBtn(this, $toolbarEl);
                //    status = statusConstants.AWAITING_PROPOSE;
                //    initMasterTable();
                //}
                else if (ClickBTN == 'btnApprovedList') {
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
                else if (ClickBTN == 'btnApprovedList') {
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
            save(true);
        }); 
        $formEl.find("#btnApprove,#btnReject").click(function (e) {
            var ClickBTN = $(this).attr('id');

            var IsApprove = false, 
                IsReject = false;

            if (ClickBTN == 'btnApprove') {
                e.preventDefault();
                IsApprove = true;
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
        //alert(1)
        var columns = [];
        
        if (isIssuePage) { 
            columns = [
                {
                    headerText: 'Commands', width: 80, visible: status == statusConstants.PROPOSED ? true : false, commands: [{ type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } }]
                },
                {
                    headerText: 'Commands', width: 80, visible: status != statusConstants.PROPOSED ? true : false, commands: [{ type: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } }]
                }
            ];
        }
        if (isApprovePage) {
            columns = [
                {
                    headerText: 'Commands', width: 80, visible: status == statusConstants.AWAITING_PROPOSE ? true : false, commands: [{ type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } }]
                },
                {
                    headerText: 'Commands', width: 80, visible: status != statusConstants.AWAITING_PROPOSE ? true : false, commands: [{ type: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } }]
                }
            ];
        }
        var additionalColumns = [ 
            {
                field: 'STSIssueNo', headerText: 'Issue No', visible: status != statusConstants.PROPOSED ? true : false
            },
            {
                field: 'STSIssueDate', headerText: 'Issue Date', type: 'date', format: _ch_date_format_1, visible: status != statusConstants.PROPOSED ? true : false
            },
            {
                field: 'STSIssueByUser', headerText: 'Issue By', visible: status != statusConstants.PROPOSED ? true : false
            },
            {
                field: 'STSReqNo', headerText: 'Req No'
            },
            {
                field: 'STSReqDate', headerText: 'Req Date', type: 'date', format: _ch_date_format_1
            },
            {
                field: 'ReqByUser', headerText: 'Req By'
            },
            {
                field: 'ReqQty', headerText: 'Req Qty'
            },
            {
                field: 'IssueQty', headerText: 'Issue Qty', visible: status != statusConstants.PROPOSED ? true : false
            }, 
            {
                field: 'Status', headerText: 'Status', visible: status == statusConstants.ALL ? true : false
            }
        ];
        columns.push.apply(columns, additionalColumns);

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = initEJ2Grid({
            tableId: tblMasterId,  
            apiEndPoint: `/api/CDA-STS-issue/list/${status}/${pageName}`,
            columns: columns,
            autofitColumns: false,
            commandClick: handleCommands
        });
    }
    function handleCommands(args) { 
        if (status === statusConstants.PROPOSED) {
            getNew(args.rowData.STSReqMasterID);
        }
        else {
            getDetails(args.rowData.STSIssueMasterID);
        }
    } 

    async function initChildTable(data) { 
        if ($tblChildEl) $tblChildEl.destroy();

        if (isApprovePage) {
            isEditable = false;
        }
        else {
            if (status == statusConstants.PROPOSED) {
                isEditable = true;
            } else {
                isEditable = false;
            }
        }

        var columns = [
            { field: 'STSIssueChildID', isPrimaryKey: true, visible: false }, 
            { field: 'ItemName', headerText: 'Item Name', allowEditing: false},
            /*{ field: 'AgentName', headerText: 'Agent Name', allowEditing: false},*/
            { field: 'BatchNo', headerText: 'Batch No', allowEditing: isEditable},
            { field: 'DisplayUnitDesc', headerText: 'Uom', allowEditing: isEditable},
            { field: 'ReqQty', headerText: 'Req Qty', allowEditing: false, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 0 } }},
            { field: 'IssueQty', headerText: 'Issue Qty', allowEditing: isEditable, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 0 } } },
            { field: 'Remarks', headerText: 'Remarks', allowEditing: isEditable}
        ];
        var tableOptions = {
            tableId: tblChildId,
            data: data,
            columns: columns,
            actionBegin: function (args) {
                if (args.requestType === "add") {
                    args.data.STSIssueChildID = getMaxIdForArray(masterData.Childs, "STSIssueChildID");  
                }
                else if (args.requestType === "save") {
                    var index = $tblChildEl.getRowIndexByPrimaryKey(args.rowData.STSIssueChildID);
                    masterData.Childs[index] = args.data;
                }
            }, 
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
        $formEl.find("#STSIssueMasterID").val(-1111);
        $formEl.find("#EntityState").val(4);
    } 
     
    function getNew(STSReqMasterId) {
        axios.get(`/api/CDA-STS-issue/new/${STSReqMasterId}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                copiedRecord = null;
                masterData = response.data; 
                $formEl.find("#btnSave").fadeIn();
                $formEl.find("#btnApprove,#btnReject").fadeOut();
                $formEl.find("#divRejectReason").hide();
                $formEl.find("#divRemarks").show(); 
                masterData.STSIssueNo = masterData.STSIssueNo;
                masterData.STSReqDate = formatDateToDefault(masterData.STSReqDate);
                masterData.STSIssueDate = formatDateToDefault(masterData.STSIssueDate); 
                setFormData($formEl, masterData);
                initChildTable(masterData.Childs);
                //IsElementDisable("Pending");
                $formEl.find("#Remarks").prop("disabled", false);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    } 
    function getDetails(id) {
        axios.get(`/api/CDA-STS-issue/${id}`)
            .then(function (response) {
                
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                copiedRecord = null;
                masterData = response.data;
                masterData.STSIssueNo = masterData.STSIssueNo;
                masterData.STSReqDate = formatDateToDefault(masterData.STSReqDate);
                masterData.STSIssueDate = formatDateToDefault(masterData.STSIssueDate);
                setFormData($formEl, masterData);
                initChildTable(masterData.Childs);

                if (isIssuePage) {
                    if (status == statusConstants.PROPOSED) {
                        $formEl.find("#btnSave").show(); 
                        $formEl.find("#btnApprove").hide(); 
                        $formEl.find("#btnReject").hide(); 
                        $formEl.find("#divRemarks").show();
                        $formEl.find("#divRejectReason").hide();
                        $formEl.find("#Remarks").prop("disabled", false);
                    }
                    else if (status == statusConstants.PENDING || status == statusConstants.AWAITING_PROPOSE || status == statusConstants.APPROVED || status == statusConstants.ALL) {
                        $formEl.find("#btnSave").hide(); 
                        $formEl.find("#btnApprove").hide(); 
                        $formEl.find("#btnReject").hide(); 
                        $formEl.find("#divRemarks").show();
                        $formEl.find("#divRejectReason").hide();
                        $formEl.find("#Remarks").prop("disabled", true);
                    }
                    else if (status == statusConstants.REJECT) {
                        $formEl.find("#btnSave").hide(); 
                        $formEl.find("#btnApprove").hide(); 
                        $formEl.find("#btnReject").hide(); 
                        $formEl.find("#divRemarks").hide();
                        $formEl.find("#divRejectReason").show();
                        $formEl.find("#Remarks").prop("disabled", true);
                    }
                }
                else if (isApprovePage) {
                    if (status == statusConstants.AWAITING_PROPOSE) {
                        $formEl.find("#btnSave").hide(); 
                        $formEl.find("#btnApprove").show(); 
                        $formEl.find("#btnReject").show(); 
                        $formEl.find("#divRemarks").show();
                        $formEl.find("#divRejectReason").show();
                        $formEl.find("#Remarks").prop("disabled", false);
                    }
                    else if (status == statusConstants.APPROVED) {
                        $formEl.find("#btnSave").hide(); 
                        $formEl.find("#btnApprove").hide(); 
                        $formEl.find("#btnReject").hide(); 
                        $formEl.find("#divRemarks").show();
                        $formEl.find("#divRejectReason").hide();
                        $formEl.find("#Remarks").prop("disabled", true);
                    }
                    else if (status == statusConstants.REJECT) {
                        $formEl.find("#btnSave").hide(); 
                        $formEl.find("#btnApprove").hide(); 
                        $formEl.find("#btnReject").hide(); 
                        $formEl.find("#divRemarks").hide();
                        $formEl.find("#divRejectReason").show();
                        $formEl.find("#Remarks").prop("disabled", true);
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
        //data.Approve = approve;
        //data.Reject = reject;

        //Validation Set in Master & Child
        initializeValidation($formEl, validationConstraints);
        if (!isValidForm($formEl, validationConstraints)) return toastr.error("Please correct all validation errors!");
        else hideValidationErrors($formEl);

        if (isValidChildForm(data)) return;

        //Data send to controller 
        data.IsSendForApprove = IsSendForApprove;
        axios.post("/api/CDA-STS-issue/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(showResponseError);
    }
    function SaveProcess(IsApprove, IsReject) {
        var data = formDataToJson($formEl.serializeArray());
        data.IsApprove = IsApprove; 
        data.IsReject = IsReject;

        axios.post("/api/CDA-STS-issue/saveprocess", data)
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
                toastr.error("Req. Qty is required.");
                isValidItemInfo = true;
            } 
            if (el.IssueQty == "" || el.IssueQty == null || el.IssueQty <= 0) {
                toastr.error("Issue Qty is required.");
                isValidItemInfo = true;
            } 
        });

        return isValidItemInfo;
    }

    var validationConstraints = {
        STSIssueNo: {
            presence: true
        },
        STSIssueDate: {
            presence: true
        },
        CompanyID: {
            presence: true
        }
    }

    function IsElementDisable(flag) {
        if (flag == "Pending") {
            //$formEl.find("#STSReqDate").prop("disabled", false);
            //$formEl.find("#CompanyID").prop("disabled", false);
            //$formEl.find("#STSReqBy").prop("disabled", false);
            //$formEl.find("#RCompanyID").prop("disabled", false);
            $formEl.find("#Remarks").prop("disabled", false);
            //$formEl.find("#RejectReason").prop("disabled", true);
        }
        else {
            //$formEl.find("#STSReqDate").prop("disabled", true);
            //$formEl.find("#CompanyID").prop("disabled", true);
            //$formEl.find("#STSReqBy").prop("disabled", true);
            //$formEl.find("#RCompanyID").prop("disabled", true);
            //if (isApprovePage == true && status == statusConstants.AWAITING_PROPOSE) {
            //    $formEl.find("#Remarks").prop("disabled", false);
            //} else {
            //    $formEl.find("#Remarks").prop("disabled", true);
            //}

            //$formEl.find("#RejectReason").prop("disabled", false);
        }
    }
})();