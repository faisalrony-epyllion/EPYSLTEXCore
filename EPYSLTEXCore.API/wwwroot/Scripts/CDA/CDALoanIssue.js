(function () {
    'use strict' 
    var menuId, pageName;
    var toolbarId, pageId;
    var $pageEl, $divTblEl, $divDetailsEl, $toolbarEl, $formEl,
        $tblMasterEl, tblMasterId,
        $tblChildEl, tblChildId;

    var status;
    var isEditable = false;
    var isCDALIssuePage = false;
    var isDCApprovePage = false;
    var isGPPage = false;
    var isGPApprovePage = false;
    var isCDACheckOutPage = false; 
    var selectedIds;

    var Flag = "";

    var tableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    }

    var masterData;

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

        isCDALIssuePage = convertToBoolean($(`#${pageId}`).find("#CDALIssuePage").val());
        isDCApprovePage = convertToBoolean($(`#${pageId}`).find("#DCApprovePage").val());
        isGPPage = convertToBoolean($(`#${pageId}`).find("#GPPage").val());
        isGPApprovePage = convertToBoolean($(`#${pageId}`).find("#GPApprovePage").val());
        isCDACheckOutPage = convertToBoolean($(`#${pageId}`).find("#CDACheckOutPage").val());

        $toolbarEl.find("#btnNew,#btnIssueLists,#btnPndForDCAppLists,#btnDCAppLists,#btnGPPendingLists,#btnPndForGPAppLists,#btnGPAppLists,#btnPndForChecking,#btnCheckedList").hide();
        $formEl.find("#btnSave,#btnSaveFDCApproval,#btnDCApprove,#btnGPSave,#btnGPApprove,#btnCheck").hide(); 

        if (isCDALIssuePage) {
            $toolbarEl.find("#btnNew,#btnIssueLists,#btnPndForDCAppLists,#btnDCAppLists,#btnGPPendingLists,#btnPndForGPAppLists,#btnGPAppLists,#btnPndForChecking,#btnCheckedList").show();

            $formEl.find("#btnSave,#btnSaveFDCApproval").show();
            $formEl.find("#btnDCApprove,#btnGPSave,#btnGPApprove,#btnCheck").hide();

            toggleActiveToolbarBtn($toolbarEl.find("#btnIssueLists"), $toolbarEl);
            status = statusConstants.PENDING;
            isEditable = false;
            Flag = "DC";
        }
        else if (isDCApprovePage) {
            $toolbarEl.find("#btnNew,#btnIssueLists,#btnGPPendingLists,#btnPndForGPAppLists,#btnGPAppLists,#btnPndForChecking,#btnCheckedList").hide();
            $toolbarEl.find("#btnPndForDCAppLists,#btnDCAppLists").show();

            $formEl.find("#btnSave,#btnSaveFDCApproval,#btnGPSave,#btnGPApprove,#btnCheck").hide();
            $formEl.find("#btnDCApprove").show();

            toggleActiveToolbarBtn($toolbarEl.find("#btnPndForDCAppLists"), $toolbarEl);
            status = statusConstants.PROPOSED_FOR_APPROVAL;
            isEditable = false;
            Flag = "DC";
        }
        else if (isGPPage) {
            $toolbarEl.find("#btnNew,#btnIssueLists,#btnPndForDCAppLists,#btnDCAppLists,#btnPndForChecking,#btnCheckedList").hide();
            $toolbarEl.find("#btnGPPendingLists,#btnPndForGPAppLists,#btnGPAppLists").show();

            $formEl.find("#btnSave,#btnSaveFDCApproval,#btnDCApprove,#btnGPApprove,#btnCheck").hide();
            $formEl.find("#btnGPSave").show();

            toggleActiveToolbarBtn($toolbarEl.find("#btnGPPendingLists"), $toolbarEl);
            status = statusConstants.PENDING;
            isEditable = false;
            Flag = "GP";
        }
        else if (isGPApprovePage) {
            $toolbarEl.find("#btnNew,#btnIssueLists,#btnPndForDCAppLists,#btnDCAppLists,#btnGPPendingLists,#btnPndForChecking,#btnCheckedList").hide();
            $toolbarEl.find("#btnPndForGPAppLists,#btnGPAppLists").show();

            $formEl.find("#btnSave,#btnSaveFDCApproval,#btnDCApprove,#btnGPSave,#btnCheck").hide();
            $formEl.find("#btnGPApprove").show();

            toggleActiveToolbarBtn($toolbarEl.find("#btnPndForGPAppLists"), $toolbarEl);
            status = statusConstants.PROPOSED_FOR_APPROVAL;
            isEditable = false;
            Flag = "GP";
        }
        else if (isCDACheckOutPage) {
            $toolbarEl.find("#btnNew,#btnIssueLists,#btnPndForDCAppLists,#btnDCAppLists,#btnGPPendingLists,#btnPndForGPAppLists,#btnGPAppLists").hide();
            $toolbarEl.find("#btnPndForChecking,#btnCheckedList").show();

            $formEl.find("#btnSave,#btnSaveFDCApproval,#btnDCApprove,#btnGPSave,#btnGPApprove,#btnGPApprove").hide();
            $formEl.find("#btnCheck").show();

            toggleActiveToolbarBtn($toolbarEl.find("#btnPndForChecking"), $toolbarEl);
            status = statusConstants.PENDING;
            isEditable = false;
            Flag = "CHK";
        }

        //Load Event
        initMasterTable();

        //Button Click Event
        $toolbarEl.find("#btnIssueLists").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PENDING;
            Flag = "DC";
            initMasterTable(); 
        });
        $toolbarEl.find("#btnPndForDCAppLists").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PROPOSED_FOR_APPROVAL;
            Flag = "DC";
            initMasterTable();
        });
        $toolbarEl.find("#btnDCAppLists").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.APPROVED;
            Flag = "DC";
            initMasterTable();
        });
        $toolbarEl.find("#btnGPPendingLists").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PENDING;
            Flag = "GP";
            initMasterTable();
        });
        $toolbarEl.find("#btnPndForGPAppLists").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PROPOSED_FOR_APPROVAL;
            Flag = "GP";
            initMasterTable();
        });
        $toolbarEl.find("#btnGPAppLists").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.APPROVED;
            Flag = "GP";
            initMasterTable();
        });
        $toolbarEl.find("#btnPndForChecking").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PENDING;
            Flag = "CHK";
            initMasterTable();
        });
        $toolbarEl.find("#btnCheckedList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.CHECK;
            Flag = "CHK";
            initMasterTable();
        });

        $formEl.find('#VehichleID').on('select2:select', function (e) {
            $formEl.find("#VehichleNo").val($formEl.find("#VehichleID").find(":selected").text()); 
        });

        $toolbarEl.find("#btnNew").on("click", getNew);

        $formEl.find("#btnSave").click(function (e) { 
            //Validation Set in Master & Child
            initializeValidation($formEl, validationConstraints);
            if (!isValidForm($formEl, validationConstraints)) return toastr.error("Please correct all validation errors!");
            else hideValidationErrors($formEl);

            e.preventDefault();
            save(false,false);
        });
        $formEl.find("#btnSaveFDCApproval").click(function (e) {
            //Validation Set in Master & Child
            initializeValidation($formEl, validationConstraints);
            if (!isValidForm($formEl, validationConstraints)) return toastr.error("Please correct all validation errors!");
            else hideValidationErrors($formEl);

            e.preventDefault();
            save(true,false);
        });
        $formEl.find("#btnGPSave").click(function (e) {
            //Validation Set in Master & Child
            initializeValidation($formEl, validationConstraints);
            if (!isValidForm($formEl, validationConstraints)) return toastr.error("Please correct all validation errors!");
            else hideValidationErrors($formEl);

            e.preventDefault();
            save(true, true);
        });

        $formEl.find("#btnDCApprove").on("click", DCApprove);
        $formEl.find("#btnGPApprove").on("click", GPApprove);
        $formEl.find("#btnCheck").on("click", CDACheck);
        $formEl.find("#btnCancel").on("click", backToList); 
    });

    function initMasterTable() {
        var columns = [
            {
                headerText: 'Commands', commands: [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } }]
            },
            {
                field: 'LIssueNo', headerText: 'Issue No'
            },
            {
                field: 'LIssueDate', headerText: 'Issue Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1
            },
            {
                field: 'LocationName', headerText: 'Location'
            },
            {
                field: 'CompanyName', headerText: 'Company'
            },
            {
                field: 'ChallanNo', headerText: 'Challan No'
            },
            {
                field: 'ChallanDate', headerText: 'Challan Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1
            },
            {
                field: 'Remarks', headerText: 'Remarks'
            }
        ];
       
        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: false,
            apiEndPoint: `/api/cda-loan-issue/list?status=${status}&Flag=${Flag}`,
            columns: columns,
            commandClick: handleCommands
        });
    }

    function handleCommands(args) {
        $formEl.find("#btnSave,#btnSaveFDCApproval,#btnDCApprove,#btnGPSave,#btnGPApprove,#btnCheck").hide();
        $formEl.find("#divGPNo,#divGPDate").hide();

        if (status == statusConstants.PENDING && Flag == "DC" && isCDALIssuePage) {
            $formEl.find("#btnSave,#btnSaveFDCApproval").show();
        }
        else if (status == statusConstants.PROPOSED_FOR_APPROVAL && Flag == "DC" && isDCApprovePage) {
            $formEl.find("#btnDCApprove").show();
        }
        else if (status == statusConstants.PENDING && Flag == "GP" && isGPPage) {
            $formEl.find("#btnGPSave").show();
        }
        else if (status == statusConstants.PROPOSED_FOR_APPROVAL && Flag == "GP" && isGPApprovePage) {
            $formEl.find("#btnGPApprove").show();
        }
        else if (status == statusConstants.PENDING && Flag == "CHK" && isCDACheckOutPage) {
            $formEl.find("#btnCheck").show();
        }
        //debugger
        if ((status == statusConstants.PENDING
            || status == statusConstants.PROPOSED_FOR_APPROVAL
            || status == statusConstants.APPROVED)
            && Flag == "GP") { 
            $formEl.find("#divGPNo,#divGPDate").show();
        }
        if ((status == statusConstants.PENDING 
            || status == statusConstants.PROPOSED_FOR_APPROVAL
            || status == statusConstants.CHECK)
            && Flag == "CHK") { 
            $formEl.find("#divGPNo,#divGPDate").show();
        }  
        if (args.commandColumn.type == 'Edit') {
            getDetails(args.rowData.CDALIssueMasterID);
        }
    }

    async function initChildTable(data) {
        isEditable = true;

        if ($tblChildEl) $tblChildEl.destroy();
        var columns = [];
        columns = await getYarnItemColumnsAsync(data, isEditable);

        var additionalColumns = [
            { field: 'CDALIssueChildID', isPrimaryKey: true, visible: false },
            { field: 'BatchNo', headerText: 'Batch No', width: 100 },
            {
                field: 'ExpiryDate', headerText: 'Expiry Date', type: 'date', format: _ch_date_format_1,
                editType: 'datepickeredit', width: 100, textAlign: 'Center'
            },
            { field: 'Rate', headerText: 'Rate', width: 100, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0 } } },
            { field: 'IssueQty', width: 100, headerText: 'Issue Qty(Kg)', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0 } } },
            { field: 'DisplayUnitDesc', width: 100, headerText: 'Unit', allowEditing: false, columnValues: 'Kg' },
            { field: 'Remarks', width: 100, headerText: 'Remarks' }
        ];
        columns.push.apply(columns, additionalColumns);
        var tableOptions = {
            tableId: tblChildId,
            data: data,
            columns: columns,
            actionBegin: function (args) {
                if (args.requestType === "add") {
                    args.data.CDALIssueChildID = getMaxIdForArray(masterData.Childs, "CDALIssueChildID");
                    args.data.DisplayUnitDesc = "Kg";
                    args.data.Rate = 0;
                    args.data.IssueQty = 0; 
                }
                else if (args.requestType === "save") {
                    ////args.data.Segment1ValueId = !args.rowData.Segment1ValueId ? 0 : args.rowData.Segment1ValueId;
                    //args.data.Segment2ValueId = !args.rowData.Segment2ValueId ? 0 : args.rowData.Segment2ValueId;
                    //args.data.Segment3ValueId = !args.rowData.Segment3ValueId ? 0 : args.rowData.Segment3ValueId;
                    //args.data.Segment4ValueId = !args.rowData.Segment4ValueId ? 0 : args.rowData.Segment4ValueId;
                    //args.data.Segment5ValueId = !args.rowData.Segment5ValueId ? 0 : args.rowData.Segment5ValueId;
                    //args.data.Segment6ValueId = !args.rowData.Segment6ValueId ? 0 : args.rowData.Segment6ValueId;
                    //args.data.Segment7ValueId = !args.rowData.Segment7ValueId ? 0 : args.rowData.Segment7ValueId;
                    //args.data.Segment8ValueId = !args.rowData.Segment8ValueId ? 0 : args.rowData.Segment8ValueId;

                    var index = masterData.Childs.findIndex(x => x.CDALIssueChildID == args.rowData.CDALIssueChildID);
                    masterData.Childs[index] = args.rowData;
                }
            },
            //commandClick: childCommandClick,
            autofitColumns: false,
            showDefaultToolbar: false,
            allowFiltering: false,
            allowPaging: false
        };

        tableOptions["toolbar"] = ['Add'];
        tableOptions["editSettings"] = {
            allowAdding: true, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true
        };
        $tblChildEl = new initEJ2Grid(tableOptions);
    } 

    function backToList() { 
        $divDetailsEl.fadeOut();
        $formEl.find("#CDALIssueMasterID").val(-1111);
        resetForm();
        $divTblEl.fadeIn();
        initMasterTable(); 
    }

    function resetForm() {
        $formEl.trigger("reset");
        $.each($formEl.find('select'), function (i, el) {
            $(el).select2('');
        });
        $formEl.find("#CDALIssueMasterID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function resetTableParams() {
        tableParams.offset = 0;
        tableParams.limit = 10;
        tableParams.filter = '';
        tableParams.sort = '';
        tableParams.order = '';
    }

    function getNew() {
        axios.get(`/api/cda-loan-issue/new`)
            .then(function (response) {
                isEditable = true;
                //status = statusConstants.NEW;
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.LIssueDate = formatDateToDefault(masterData.LIssueDate);
                masterData.ChallanDate = formatDateToDefault(masterData.ChallanDate); 
                masterData.GPDate = formatDateToDefault(masterData.GPDate);
                setFormData($formEl, masterData);
                initChildTable([]);  
                $formEl.find("#divGPNo,#divGPDate").hide();
            })
            .catch(showResponseError);
    }
    function getDetails(id) {
        var url = `/api/cda-loan-issue/${id}`;
        axios.get(url)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.LIssueDate = formatDateToDefault(masterData.LIssueDate);
                masterData.ChallanDate = formatDateToDefault(masterData.ChallanDate); 
                masterData.GPDate = formatDateToDefault(masterData.GPDate);
                setFormData($formEl, masterData);

                initChildTable(masterData.Childs);  
            })
            .catch(showResponseError);
    }

    function isValidChildForm(data) {
        var isValidItemInfo = false; 

        $.each(data["Childs"], function (i, el) {
            if (el.IssueQty == "" || el.IssueQty == null || el.IssueQty <= 0) {
                toastr.error("Issue Qty is required.");
                isValidItemInfo = true;
            }
            else if (el.Rate == "" || el.Rate == null || el.Rate <= 0) {
                toastr.error("Rate is required.");
                isValidItemInfo = true;
            }
            else if (el.BatchNo == "" || el.BatchNo == null) {
                toastr.error("Batch No is required.");
                isValidItemInfo = true;
            }
            else if (el.ExpiryDate == "" || el.ExpiryDate == null) {
                toastr.error("Expiry Date is required.");
                isValidItemInfo = true;
            }
        }); 
        return isValidItemInfo;
    }

    var validationConstraints = {  
        LIssueDate: {
            presence: true
        }, 
        LocationID: {
            presence: true
        },
        LoanIssueToID: {
            presence: true
        },
        TransportTypeID: {
            presence: true
        }, 
        TransportMode: {
            presence: true
        }
    }

    function save(isDCSendForApproval, isGPSendForApproval) { 
        //Data get for save process
        var data = formElToJson($formEl); 
        data.VehichleNo = $formEl.find("#VehichleID").find(":selected").text();  
        data["Childs"] = $tblChildEl.getCurrentViewRecords();
        if (data.Childs.length === 0) return toastr.error("At least 1 Yarn items is required."); 

        //Child Validation check 
        if (isValidChildForm(data)) return;

        //Data send to controller 
        if (isDCSendForApproval) {
            data.DCSendForApproval = true;
        } else {
            data.DCSendForApproval = false;
        }
        if (isGPSendForApproval)
            data.GPSendForApproval = true;

        if (Flag == "GP" && isGPPage && status == statusConstants.PENDING) {
            data.GPFlag = true;
        } else {
            data.GPFlag = false;
        } 
        axios.post("/api/cda-loan-issue/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(showResponseError);
    }

    function DCApprove() {
        var id = $formEl.find("#CDALIssueMasterID").val();
        var url = `/api/cda-loan-issue/dcapproval/${id}`;
        axios.post(url)
            .then(function () {
                toastr.success("DC Approved successfully.");
                backToList();
            })
            .catch(showResponseError);
    }
    function GPApprove() {
        var id = $formEl.find("#CDALIssueMasterID").val();
        var url = `/api/cda-loan-issue/gpapproval/${id}`;
        axios.post(url)
            .then(function () {
                toastr.success("GP Approved successfully.");
                backToList();
            })
            .catch(showResponseError);
    }
    function CDACheck() {
        var id = $formEl.find("#CDALIssueMasterID").val();
        var url = `/api/cda-loan-issue/chkapproval/${id}`;
        axios.post(url)
            .then(function () {
                toastr.success("Check Approved successfully.");
                backToList();
            })
            .catch(showResponseError);
    }
   
})();