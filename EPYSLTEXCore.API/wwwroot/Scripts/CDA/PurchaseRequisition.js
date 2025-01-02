(function () {
    // #region variables
    var menuId, pageName, menuParam;
    var toolbarId;

    var isPRPage = false;
    var isApprovePage = false;
    var isAcknowledgePage = false;
    var isCPRPage = false;
    var isFPRPage = false;
    var isPRCPage = false;
    var isEditable = false;
    var isDisplaySuggestedQty = false;
    var isIndent = 0;

    var subGroupId, subGroupName;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, tblMasterId, $tblChildEl, tblChildId, $formEl, tblCompanyModalId, $tblCompanyModalEl;
    var status = statusConstants.PENDING;
    var masterData = {};
    // #endregion

    $(function () {

        if (!menuId) menuId = localStorage.getItem("menuId");
        if (!pageName) pageName = localStorage.getItem("pageName");
        if (!menuParam) menuParam = localStorage.getItem("menuParam");

        var pageId = pageName + "-" + menuId;
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
        tblChildId = pageConstants.CHILD_TBL_ID_PREFIX + pageId;
        tblCompanyModalId = "#tblCompanyModal" + pageId;
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);


        if (menuParam == "PR") isPRPage = true;
        else if (menuParam == "A") isApprovePage = true;
        else if (menuParam == "Ack") isAcknowledgePage = true;
        else if (menuParam == "CPR") isCPRPage = true;
        else if (menuParam == "FPR") isFPRPage = true;
        else if (menuParam == "PRC") isPRCPage = true;

        //isPRPage = convertToBoolean($(`#${pageId}`).find("#PurchaseRequisitionPage").val());
        //isAcknowledgePage = convertToBoolean($(`#${pageId}`).find("#AcknowledgePage").val());
        //isApprovePage = convertToBoolean($(`#${pageId}`).find("#ApprovePage").val());
        //isCPRPage = convertToBoolean($(`#${pageId}`).find("#CPRPage").val());
        //isFPRPage = convertToBoolean($(`#${pageId}`).find("#FPRPage").val());
        //isPRCPage = convertToBoolean($(`#${pageId}`).find("#PRCPage").val());

        $formEl.find("#divCheckRejectReason").hide();
        $formEl.find("#divRejectReason").hide();

        if (isPRPage) {
            $toolbarEl.find("#btnCheckingRejectedList,#btnCheckingYPR,#btnCheckRejectYPR").hide();
            $toolbarEl.find("#btnPendingAkgList,#btnAcknowledgementList,#btnPendingCPRList,#btnCPRList,#btnPendingCFRList,#btnCFRList").hide();

            $toolbarEl.find("#btnNewPR,#btnPendingIndentList,#btnPRRequisitionList,#btnPendingForCheckingList,#btnCheckingList,#btnProposedList,#btnApprovedList,#btnRejectdList,#btnAllList").show();
            $toolbarEl.find("#divAddPRForCDAIndent").fadeOut();

            $formEl.find("#btnSave,#btnSaveAndSendForCheck").show();
            $formEl.find("#btnCheckAndSendForApprove").hide();
            $formEl.find("#btnApproveYPR,#btnAcknowledge,#btnRejectYPR").hide();

            toggleActiveToolbarBtn($toolbarEl.find("#btnPRRequisitionList"), $toolbarEl);

            status = statusConstants.PENDING;
            isEditable = false;
            isIndent = 0;
        }
        else if (isPRCPage) {
            $toolbarEl.find("#btnNewPR,#btnPendingIndentList,#btnPRRequisitionList,#btnPendingAkgList,#btnAcknowledgementList,#btnAllList,#btnPendingCPRList,#btnCPRList,#btnPendingCFRList,#btnCFRList").hide();
            $toolbarEl.find("#btnProposedList,#btnApprovedList,#btnRejectdList").hide();

            $toolbarEl.find("#btnPendingForCheckingList,#btnCheckingList,#btnCheckingRejectedList,#btnCheckingYPR").show();
            $toolbarEl.find("#divAddPRForCDAIndent").fadeOut();

            $formEl.find("#btnApproveYPR,#btnRejectYPR").hide();
            $formEl.find("#btnSave,#btnSaveAndSendForCheck,#btnAcknowledge").hide();
            $formEl.find("#btnCheckAndSendForApprove,#btnCheckRejectYPR").show();
            $formEl.find("#divCheckRejectReason").show();

            toggleActiveToolbarBtn($toolbarEl.find("#btnPendingForCheckingList"), $toolbarEl);

            status = statusConstants.PROPOSED;
            isEditable = false;
            isIndent = 0;
        }
        else if (isApprovePage) {
            $toolbarEl.find("#btnPendingForCheckingList,#btnCheckingList,#btnCheckingRejectedList,#btnCheckingYPR,#btnCheckRejectYPR").hide();
            $toolbarEl.find("#btnProposedList,#btnApprovedList,#btnRejectdList").show();
            $toolbarEl.find("#btnNewPR,#btnPendingIndentList,#btnPRRequisitionList,#btnPendingAkgList,#btnAcknowledgementList,#btnAllList,#btnPendingCPRList,#btnCPRList,#btnPendingCFRList,#btnCFRList").hide();

            $formEl.find("#btnApproveYPR,#btnRejectYPR").show();
            $formEl.find("#btnSave,#btnSaveAndSendForCheck,#btnCheckAndSendForApprove,#btnAcknowledge").hide();
            $toolbarEl.find("#divAddPRForCDAIndent").fadeOut();

            toggleActiveToolbarBtn($toolbarEl.find("#btnProposedList"), $toolbarEl);

            status = statusConstants.PROPOSED_FOR_APPROVAL;
            isEditable = false;
            isIndent = 0;
        }
        else if (isAcknowledgePage) {
            $toolbarEl.find("#btnPendingForCheckingList,#btnCheckingList,#btnCheckingRejectedList,#btnCheckingYPR,#btnCheckRejectYPR").hide();
            $toolbarEl.find("#btnPendingAkgList,#btnAcknowledgementList").show();
            $toolbarEl.find("#btnNewPR,#btnPendingIndentList,#btnPRRequisitionList,#btnProposedList,#btnApprovedList,#btnRejectdList,#btnAllList,#btnPendingCPRList,#btnCPRList,#btnPendingCFRList,#btnCFRList").hide();

            $formEl.find("#btnAcknowledge").show();
            $formEl.find("#btnSave,#btnSaveAndSendForCheck,#btnCheckAndSendForApprove,#btnApproveYPR,#btnRejectYPR").hide();
            $toolbarEl.find("#divAddPRForCDAIndent").fadeOut();

            toggleActiveToolbarBtn($toolbarEl.find("#btnPendingAkgList"), $toolbarEl);
            status = statusConstants.PARTIALLY_COMPLETED;
            isEditable = false;
            isIndent = 0;
        }

        else if (isCPRPage) {
            $toolbarEl.find("#btnPendingForCheckingList,#btnCheckingList,#btnCheckingRejectedList,#btnCheckingYPR,#btnCheckRejectYPR").hide();
            $toolbarEl.find("#btnPendingCPRList,#btnCPRList").show();
            $toolbarEl.find("#btnNewPR,#btnPendingIndentList,#btnPRRequisitionList,#btnProposedList,#btnApprovedList,#btnRejectdList,#btnPendingAkgList,#btnAcknowledgementList,#btnAllList,#btnPendingCFRList,#btnCFRList").hide();
            $toolbarEl.find("#divAddPRForCDAIndent").fadeOut();

            $formEl.find("#btnSave,#btnSaveAndSendForCheck,#btnCheckAndSendForApprove,#btnApproveYPR,#btnRejectYPR,#btnAcknowledge").hide();

            toggleActiveToolbarBtn($toolbarEl.find("#btnPendingCPRList"), $toolbarEl);
            status = statusConstants.PENDING;
            isEditable = false;
            isIndent = 0;
        }
        else if (isFPRPage) {
            $toolbarEl.find("#btnPendingForCheckingList,#btnCheckingList,#btnCheckingRejectedList,#btnCheckingYPR,#btnCheckRejectYPR").hide();
            $toolbarEl.find("#btnPendingCFRList,#btnCFRList").show();
            $toolbarEl.find("#btnNewPR,#btnPendingIndentList,#btnPRRequisitionList,#btnProposedList,#btnApprovedList,#btnRejectdList,#btnPendingAkgList,#btnAcknowledgementList,#btnAllList,#btnPendingCPRList,#btnCPRList").hide();
            $toolbarEl.find("#divAddPRForCDAIndent").fadeOut();

            $formEl.find("#btnSave,#btnSaveAndSendForCheck,#btnCheckAndSendForApprove,#btnApproveYPR,#btnRejectYPR,#btnAcknowledge").hide();

            toggleActiveToolbarBtn($toolbarEl.find("#btnPendingCFRList"), $toolbarEl);
            status = statusConstants.PENDING;
            isEditable = false;
            isIndent = 0;
        }

        initMasterTable();

        $toolbarEl.find("#btnPendingIndentList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.INDENT_PENDING;
            isEditable = true;
            isIndent = 1;
            initMasterTable_ForIndent();
            $toolbarEl.find("#divAddPRForCDAIndent").fadeIn();
        });

        $toolbarEl.find("#btnPRRequisitionList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PENDING;
            isIndent = 0;
            isEditable = true;
            initMasterTable();
        });

        $toolbarEl.find("#btnPendingForCheckingList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PROPOSED;
            isIndent = 0;
            isEditable = false;
            initMasterTable();
        });
        $toolbarEl.find("#btnCheckingList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.CHECK;
            isIndent = 0;
            isEditable = false;
            initMasterTable();
        });
        $toolbarEl.find("#btnCheckingRejectedList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.CHECK_REJECT;
            isIndent = 0;
            isEditable = false;
            initMasterTable();
        });

        $toolbarEl.find("#btnProposedList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PROPOSED_FOR_APPROVAL;
            isIndent = 0;
            //status = statusConstants.PROPOSED;
            //isEditable = isApprovePage ? false : true;
            isEditable = false;
            initMasterTable();
        });
        $toolbarEl.find("#btnApprovedList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.APPROVED;
            isIndent = 0;
            isEditable = false;
            initMasterTable();
        });
        $toolbarEl.find("#btnRejectdList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.REJECT;
            isIndent = 0;
            isEditable = false;
            initMasterTable();
        });
        $toolbarEl.find("#btnPendingAkgList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PARTIALLY_COMPLETED;
            isIndent = 0;
            isEditable = false;
            initMasterTable();
        });
        $toolbarEl.find("#btnAcknowledgementList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.ACKNOWLEDGE;
            isIndent = 0;
            isEditable = false;
            initMasterTable();
        });

        //$toolbarEl.find("#btnUnAcknowledgeList").on("click", function (e) {
        //    e.preventDefault();
        //    toggleActiveToolbarBtn(this, $toolbarEl);
        //    status = statusConstants.UN_ACKNOWLEDGE;
        //    isEditable = false;
        //    initMasterTable();
        //});

        $toolbarEl.find("#btnPendingCPRList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PENDING;
            isIndent = 0;
            isEditable = false;
            initMasterTable();
        });

        $toolbarEl.find("#btnCPRList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.COMPLETED;
            isIndent = 0;
            isEditable = false;
            initMasterTable();
        });

        $toolbarEl.find("#btnPendingCFRList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PENDING;
            isIndent = 0;
            isEditable = false;
            initMasterTable();
        });

        $toolbarEl.find("#btnCFRList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.COMPLETED;
            isIndent = 0;
            isEditable = false;
            initMasterTable();
        });

        //$toolbarEl.find("#btnList").on("click", function (e) {
        //    e.preventDefault();
        //    toggleActiveToolbarBtn(this, $toolbarEl);
        //    status = statusConstants.COMPLETED;
        //    isEditable = true;
        //    initMasterTable();
        //});


        $toolbarEl.find("#btnNewPR").on("click", showSubGroupSelection);

        $toolbarEl.find("#btnAddPRForCDAIndent").on("click", getNewDataPRForCDAIndent);

        $("#btnChangeSubGroup").on("click", changeSubGroup)

        $formEl.find("#btnRejectYPR").click(function (e) {
            e.preventDefault();

            bootbox.prompt("Are you sure you want to reject this?", function (result) {
                if (!result) {
                    return toastr.error("Reject reason is required.");
                }
                var id = $formEl.find("#CDAPRMasterID").val();
                var reason = result;
                axios.post(`/api/cda-pr/reject/${id}/${reason}`)
                    .then(function () {
                        toastr.success("Requisition rejected successfully.");
                        backToList();
                    })
                    .catch(showResponseError);
            });
        });

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save(false, false, false);
        });
        $formEl.find("#btnSaveAndSendForCheck").click(function (e) {
            e.preventDefault();
            save(true, false, false);
        });
        $formEl.find("#btnCheckRejectYPR").click(function (e) {
            e.preventDefault();
            save(true, false, true);
        });
        $formEl.find("#btnCheckAndSendForApprove").click(function (e) {
            e.preventDefault();
            save(false, true, false);
        });

        /*$formEl.find("#btnSave").click(save);*/

        $formEl.find("#btnApproveYPR").click(function (e) {
            e.preventDefault();
            var id = $formEl.find("#CDAPRMasterID").val();
            axios.post(`/api/cda-pr/approve/${id}`)
                .then(function () {
                    toastr.success(constants.APPROVE_SUCCESSFULLY);
                    backToList();
                })
                .catch(showResponseError);
        });

        $formEl.find("#btnAcknowledge").click(function (e) {
            e.preventDefault();
            var id = $formEl.find("#CDAPRMasterID").val();
            axios.post(`/api/cda-pr/acknowledge/${id}`)
                .then(function () {
                    toastr.success("Requisition acknowledged successfully.");
                    backToList();
                })
                .catch(showResponseError);
        });

        //$formEl.find("#btnAkgYPR").click(function (e) {
        //    e.preventDefault();
        //    bootbox.prompt("Are you sure you want to reject this?", function (result) {
        //        if (!result) {
        //            return toastr.error("Reject reason is required.");
        //        }
        //        var id = $formEl.find("#CDAPRMasterID").val();
        //        var reason = result;
        //        axios.post(`/api/cda-pr/unacknowledge/${id}/${reason}`)
        //            .then(function () {
        //                toastr.success("Requisition rejected successfully.");
        //                backToList();
        //            })
        //            .catch(showResponseError);
        //    });
        //});

        $formEl.find("#btnCancel").on("click", backToList);

        $formEl.find("#btnAddItems").on("click", function (e) {
            e.preventDefault();
            var finder = new commonFinder({
                title: "Select Items",
                pageId: pageId,
                data: masterData.ChildsItemSegments,
                fields: "Segment1ValueDesc,Segment2ValueDesc,Segment3ValueDesc,Segment4ValueDesc",
                headerTexts: "Primary Group, Agent,Item Name, Form",
                isMultiselect: true,
                selectedIds: masterData.ChildsItemSegments[0].ItemIDs,
                allowPaging: false,
                primaryKeyColumn: "ItemMasterID",
                onMultiselect: function (selectedRecords) {
                    //debugger
                    var ChildsItemSegmentsList = $tblChildEl.getCurrentViewRecords();
                    for (var i = 0; i < selectedRecords.length; i++) {
                        var exists = ChildsItemSegmentsList.find(function (el) { return el.ItemMasterID == selectedRecords[i].ItemMasterID })
                        if (!exists) {
                            var oPreProcess = {
                                CDAPRChildID: getMaxIdForArray(masterData.ChildsItemSegmentsPost, "CDAPRChildID"),
                                CDAPRMasterID: 0,
                                ItemMasterID: selectedRecords[i].ItemMasterID,
                                UnitID: selectedRecords[i].UnitID,
                                Segment1ValueId: selectedRecords[i].Segment1ValueId,
                                Segment1ValueDesc: selectedRecords[i].Segment1ValueDesc,
                                Segment2ValueId: selectedRecords[i].Segment2ValueId,
                                Segment2ValueDesc: selectedRecords[i].Segment2ValueDesc,
                                Segment3ValueId: selectedRecords[i].Segment3ValueId,
                                Segment3ValueDesc: selectedRecords[i].Segment3ValueDesc,
                                Segment4ValueId: selectedRecords[i].Segment4ValueId,
                                Segment4ValueDesc: selectedRecords[i].Segment4ValueDesc,
                                ReqQty: 0,
                                SetupChildID: 0,
                                FPRCompanyID: 0,
                            }
                            masterData.ChildsItemSegments[0].ItemIDs = selectedRecords.map(function (el) { return el.ItemMasterID }).toString();
                            masterData.ChildsItemSegmentsPost.push(oPreProcess);
                        }
                    }
                    initChildTable(masterData.ChildsItemSegmentsPost);
                }
            });
            finder.showModal();
        });
    });

    function changeSubGroup(e) {
        showHideControls();
        isEditable = true;

        e.preventDefault;
        showBootboxConfirm("Change Sub Group", "Are you sure you want to change sub group?", function (yes) {
            if (yes) showSubGroupSelection();
        })
    }

    function initMasterTable_ForIndent() {
        $toolbarEl.find("#divAddPRForCDAIndent").fadeIn();

        var commands = [];
        commands = [
            { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
            { type: 'CDAIndentReport', title: 'CDA Indent', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
        ]

        var columns = [
            //{
            //    headerText: '', commands: commands, textAlign: 'Center', width: 100, minWidth: 100, maxWidth: 100
            //},
            {
                field: 'SubGroupID', visible: false
            },
            {
                field: 'ItemMasterID', visible: false
            },
            {
                field: 'CompanyID', visible: false
            },
            {
                field: 'CDAPRChildID', isPrimaryKey: true, visible: false
            },
            {
                type: 'checkbox', width: 70, headerText: 'Select'
            },
            {
                field: 'CompanyName', headerText: 'Company', width: 80
            },
            {
                field: 'IndentNo', headerText: 'Indent No', width: 100
            },
            {
                field: 'IndentDate', headerText: 'Indent Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, width: 100
            },
            { field: 'Segment1ValueDesc', headerText: 'Primary Group', allowEditing: false, width: 80 },
            { field: 'Segment2ValueDesc', headerText: 'Agent', allowEditing: false, width: 120 },
            { field: 'Segment3ValueDesc', headerText: 'Item Name', allowEditing: false, width: 200 },
            { field: 'Segment4ValueDesc', headerText: 'Form', allowEditing: false, width: 80 },
            {
                field: 'ReqQty', headerText: 'Indent Qty(Kg)', width: 80, textAlign: 'Right'
            },
            {
                field: 'TriggerPoint', headerText: 'Trigger Point', width: 120
            },
            {
                field: 'CDAIndentByUser', headerText: 'Indent By', width: 120
            },
            {
                field: 'Remarks', headerText: 'Remarks', width: 200
            },
            {
                field: 'AcknowledgeDate', headerText: 'Commercial Ack Date', textAlign: 'Center', type: 'date', format: _ch_date_format_5//, visible: showAck, width: 100
            },
            {
                field: 'TexAcknowledgeDate', headerText: 'Acknowledge Date', textAlign: 'Center', type: 'date', format: _ch_date_format_5//, visible: showTexAck, width: 100
            }
        ];

        //var pageName = isCPRPage ? pageNameConstants.CPR : isFPRPage ? pageNameConstants.FPR : "";
        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: false,
            apiEndPoint: `/api/cda-pr/pendingIndentList?status=${status}&pageName=${pageName}`,
            columns: columns,
            //commandClick: handleCommands
        });
    }
    function initMasterTable() {
        $toolbarEl.find("#divAddPRForCDAIndent").fadeOut();

        var commands = [];

        if (isPRPage) {
            if (status == statusConstants.PENDING) {
                commands = [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                    { type: 'CDAReport', title: 'CDA PR', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                ]
            }
            else {
                commands = [
                    { type: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                    { type: 'CDAReport', title: 'CDA PR', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                ]
            }
        }
        else if (isPRCPage) {
            if (status == statusConstants.CHECK || status == statusConstants.CHECK_REJECT) {
                commands = [
                    { type: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                    { type: 'CDAReport', title: 'CDA PR', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                ]
            }
            else {
                commands = [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                    //{ type: 'Check', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-check' } },
                    //{ type: 'CheckReject', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-ban' } },
                    { type: 'CDAReport', title: 'CDA PR', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                ]
            }
        }
        else if (isApprovePage) {
            if (status == statusConstants.APPROVED || status == statusConstants.REJECT) {
                commands = [
                    { type: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                    { type: 'CDAReport', title: 'CDA PR', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                ]
            }
            else {
                commands = [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                    { type: 'Approve', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-check' } },
                    { type: 'Reject', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-ban' } },
                    { type: 'CDAReport', title: 'CDA PR', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                ]
            }
        }
        else if (isAcknowledgePage) {
            if (status == statusConstants.ACKNOWLEDGE || status == statusConstants.UN_ACKNOWLEDGE) {
                commands = [
                    { type: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                    { type: 'CDAReport', title: 'CDA PR', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                ]
            } else {
                commands = [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                    { type: 'Acknowledge', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-check' } },
                    { type: 'CDAReport', title: 'CDA PR', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                    /*{ type: 'UnAcknowledge', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-times' } }*/
                ]
            }
        }
        else if (isCPRPage == true || isFPRPage == true) {
            if (status == statusConstants.PENDING) {
                commands = [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                    { type: 'CDAReport', title: 'CDA PR', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                ]
            } else {
                commands = [
                    { type: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                    { type: 'CDAReport', title: 'CDA PR', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                ]
            }
        }

        var columns = [
            {
                headerText: '', commands: commands, textAlign: 'Center', width: 130, minWidth: 130, maxWidth: 130
            },
            {
                field: 'SubGroupID', visible: false
            },
            {
                field: 'IndentNo', headerText: 'Indent No'
            },
            {
                field: 'CDAPRNo', headerText: 'PR No'
            },
            {
                field: 'CDAPRDate', headerText: 'PR Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1
            },
            {
                field: 'TriggerPoint', headerText: 'Trigger Point'
            },
            {
                field: 'CDAPRRequiredDate', headerText: 'Required Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1
            },
            {
                field: 'CDAPRByUser', headerText: 'Requisition By'
            },
            {
                field: 'Remarks', headerText: 'Remarks'
            }
        ];

        //var pageName = isCPRPage ? pageNameConstants.CPR : isFPRPage ? pageNameConstants.FPR : "";
        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: false,
            apiEndPoint: `/api/cda-pr/list?status=${status}&pageName=${pageName}`,
            columns: columns,
            commandClick: handleCommands
        });
    }

    function handleCommands(args) {
        $formEl.find("#btnSave,#btnSaveAndSendForCheck,#btnCheckAndSendForApprove,#btnApproveYPR,#btnRejectYPR,#btnAcknowledge,#btnCheckRejectYPR,#divCheckRejectReason").fadeOut();
        $formEl.find("#btnAddItems").fadeOut();

        if (isPRCPage && status == statusConstants.PROPOSED) {
            $formEl.find("#btnCheckRejectYPR,#divCheckRejectReason").fadeIn();
        }

        var vSubGroupName = "";
        if (args.rowData.SubGroupID == "100") {
            vSubGroupName = "Dyes_Group";
        } else {
            vSubGroupName = "Chemicals_Group";
        }

        if (args.commandColumn.type == 'Edit') {
            getDetails(args.rowData.CDAPRMasterID, vSubGroupName);
            if (isPRPage) {
                $formEl.find("#btnSave,#btnSaveAndSendForCheck").fadeIn();
                $formEl.find("#btnAddItems").fadeIn();
                isEditable = true;
            }
            else if (isPRCPage) {
                $formEl.find("#btnCheckAndSendForApprove").fadeIn();
                //isEditable = true;
            }
            else if (isApprovePage) {
                $formEl.find("#btnApproveYPR,#btnRejectYPR").fadeIn();
            }
            else if (isAcknowledgePage) {
                $formEl.find("#btnAcknowledge").fadeIn();
            }
            else {
                $formEl.find("#btnSave").fadeIn();
            }
        }
        else if (args.commandColumn.type == 'View') {
            getDetails(args.rowData.CDAPRMasterID, vSubGroupName);
        }
        else if (args.commandColumn.type == 'Approve') {
            approvePR(args.rowData.CDAPRMasterID);
        }
        else if (args.commandColumn.type == 'Reject') {
            showBootboxPrompt("Reject Yarn PR", "Are you sure you want to Reject this PR?", function (result) {
                if (result) {
                    rejectPR(args.rowData.CDAPRMasterID, result);
                }
            });
        }
        else if (args.commandColumn.type == 'Acknowledge') {
            acknowledgePR(args.rowData.CDAPRMasterID);
        }
        else if (args.commandColumn.type == 'UnAcknowledge') {
            showBootboxPrompt("Reject Yarn PR", "Are you sure you want to Reject this PR?", function (result) {
                if (result) {
                    unacknowledgePR(args.rowData.CDAPRMasterID, result);
                }
            });
        }
        else if (args.commandColumn.type == "CDAReport") {
            window.open(`/reports/InlinePdfView?ReportName=CDAPurchaseRequisition.rdl&CDAPRNo=${args.rowData.CDAPRNo}`, '_blank');
        }
    }

    function backToList() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        if (isIndent == 0) {
            initMasterTable();
        } else {
            initMasterTable_ForIndent();
        }
    }

    function resetForm() {
        filterBy = {};
        $formEl.trigger("reset");
        $.each($formEl.find('select'), function (i, el) {
            $(el).select2('');
        });
        $formEl.find("#CDAPRMasterID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    //function showSubGroupSelection() {
    //    axios.get("/api/selectoption/cda-dyes-chemical")
    //        .then(function (response) {
    //            showBootboxSelect2Dialog("Select SubGroup", "ddlSubGroupID", "Choose SubGroup", response.data, function (data) {
    //                if (data) {
    //                    HoldOn.open({
    //                        theme: "sk-circle"
    //                    });
    //                    resetForm();
    //                    subGroupId = data.id;
    //                    subGroupName = data.text;
    //                    $formEl.find("#SubGroupID").val(masterData.SubGroupID);
    //                    $formEl.find("#lblSubGroup").text(masterData.SubGroupName);
    //                    getNewData();
    //                }
    //                else toastr.warning("You must select a supplier.");
    //            })
    //        })
    //        .catch(showResponseError);
    //}

    function showSubGroupSelection() {
        axios.get("/api/cda-pr/cda-dyes-chemical")
            .then(function (response) {
                //console.log(response);

                showBootboxSelect2Dialog("Select SubGroup", "ddlSubGroupID", "Choose SubGroup", response.data.SubGroupList, function (data) {
                    //console.log(data);
                    if (data) {
                        HoldOn.open({
                            theme: "sk-circle"
                        });
                        resetForm();
                        subGroupId = data.id;
                        subGroupName = data.text;
                        $formEl.find("#SubGroupID").val(subGroupId);
                        $formEl.find("#lblSubGroup").text(subGroupName);
                        //$formEl.find("#SubGroupID").val(masterData.SubGroupID);
                        //$formEl.find("#lblSubGroup").text(masterData.SubGroupName);
                        isIndent = 0;
                        var vSubGroupName = "";
                        if (subGroupId == "100") {
                            vSubGroupName = "Dyes_Group";
                        } else {
                            vSubGroupName = "Chemicals_Group";
                        }
                        getNewData(vSubGroupName);
                    }
                    else toastr.warning("You must select a supplier.");
                })
            })
            .catch(showResponseError);
    }

    function getNewData(SubGroupName) {
        axios.get(`/api/cda-pr/new/${SubGroupName}`)
            .then(function (response) {
                isEditable = true;
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                $formEl.find("#btnSave,#btnSaveAndSendForCheck").fadeIn();
                $formEl.find("#btnApproveYPR,#btnRejectYPR,#btnAcknowledge").fadeOut();
                $formEl.find("#divRejectReason").hide();
                /*$formEl.find("#divUnAcknowledgeReason").hide();*/
                masterData = response.data;
                masterData.CDAPRRequiredDate = formatDateToDefault(masterData.CDAPRRequiredDate);
                masterData.CDAPRDate = formatDateToDefault(masterData.CDAPRDate);
                masterData.SubGroupID = subGroupId;
                masterData.SubGroupName = subGroupName;
                setFormData($formEl, masterData);
                $formEl.find("#SubGroupID").val(masterData.SubGroupID);
                initChildTable([]);
                showHideControls();
            })
            .catch(showResponseError);
    }

    function getDetails(id, SubGroupName) {
        var url = `/api/cda-pr/${id}/${SubGroupName}`;
        axios.get(url)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                //console.log(response.data);
                masterData.CDAPRRequiredDate = formatDateToDefault(masterData.CDAPRRequiredDate);
                masterData.CDAPRDate = formatDateToDefault(masterData.CDAPRDate);

                if (isPRCPage) {
                    $formEl.find("#divCheckRejectReason").fadeIn();
                    isDisplaySuggestedQty = false;
                }
                else if (masterData.CheckRejectReason.trim().length > 0) {
                    $formEl.find("#divCheckRejectReason").fadeIn();
                    isDisplaySuggestedQty = true;
                } else {
                    $formEl.find("#divCheckRejectReason").fadeOut();
                    isDisplaySuggestedQty = false;
                }

                $formEl.find("#CheckRejectReason").attr('readonly', !isPRCPage);

                //masterData.Childs.forEach(function (value) {
                //    value.CompanyIDs = value.CompanyIDs.map(function (el) { return el.toString() })
                //})

                showHideControls();
                setFormData($formEl, masterData);
                $formEl.find("#lblSubGroup").text(masterData.SubGroupName);

                initChildTable(masterData.Childs);
                masterData.ChildsItemSegmentsPost = masterData.Childs
                masterData.ChildsItemSegments[0].ItemIDs = masterData.ChildsItemSegmentsPost.map(function (el) { return el.ItemMasterID }).toString();
                //initChildTable(masterData.ChildsItemSegments);
            })
            .catch(showResponseError);
    }

    function getNewDataPRForCDAIndent() {
        if ($tblMasterEl.getSelectedRecords().length == 0) {
            toastr.error("Please select row(s)!");
            return;
        }
        var uniqueAry = distinctArrayByProperty($tblMasterEl.getSelectedRecords(), "SubGroupID");

        if (uniqueAry.length != 1) {
            toastr.error("Selected row(s) Sub Group should be same!");
            return;
        } else {
            var uniqueAry1 = distinctArrayByProperty($tblMasterEl.getSelectedRecords(), "CompanyID");
            if (uniqueAry1.length != 1) {
                toastr.error("Selected row(s) Company should be same!");
                return;
            }
        }

        var iDs = $tblMasterEl.getSelectedRecords().map(function (el) { return el.CDAPRChildID }).toString();
        subGroupId = uniqueAry[0].SubGroupID;
        subGroupName = uniqueAry[0].SubGroupName;

        var vSubGroupName = "";
        if (subGroupId == "100") {
            vSubGroupName = "Dyes_Group";
        } else {
            vSubGroupName = "Chemicals_Group";
        }
        axios.get(`/api/cda-pr/new/${vSubGroupName}/${iDs}`)
            .then(function (response) {
                isEditable = true;
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                $formEl.find("#btnSave,#btnSaveAndSendForCheck").fadeIn();
                $formEl.find("#btnApproveYPR,#btnRejectYPR,#btnAcknowledge").fadeOut();
                $formEl.find("#divRejectReason").hide();
                $formEl.find("#SubGroupID").val(subGroupId);
                $formEl.find("#lblSubGroup").text(subGroupName);

                masterData = response.data;
                masterData.CDAPRRequiredDate = formatDateToDefault(masterData.CDAPRRequiredDate);
                masterData.CDAPRDate = formatDateToDefault(masterData.CDAPRDate);
                masterData.SubGroupID = subGroupId;
                masterData.SubGroupName = subGroupName;
                masterData.CompanyID = masterData.Childs[0].CompanyID;
                masterData.TriggerPointID = 1251;
                isIndent = 1;
                $formEl.find("#btnAddItems").fadeOut();

                //$formEl.find("#TriggerPointID").val('1251').trigger("change");
                //$formEl.find("#CompanyID").val(masterData.Childs[0].CompanyID).trigger("change"); 

                setFormData($formEl, masterData);
                initChildTable(masterData.Childs);

                showHideControls();
                $formEl.find("#CompanyID").attr("disabled", true);
                $formEl.find("#TriggerPointID").attr("disabled", true);
            })
            .catch(showResponseError);
    }

    function save(SendForCheck, SendForApproval, isCheckReject) {
        var data = formDataToJson($formEl.serializeArray());

        data["Childs"] = $tblChildEl.getCurrentViewRecords();
        initializeValidation($formEl, validationConstraints);
        if (!isValidForm($formEl, validationConstraints)) return toastr.error("Please correct all validation errors!");
        else hideValidationErrors($formEl);

        if (data.Childs.length === 0) return toastr.error("At least 1 Yarn items is required.");

        data.CheckRejectReason = $formEl.find("#CheckRejectReason").val();
        if (isCheckReject && data.CheckRejectReason.trim().length == 0) {
            $formEl.find("#CheckRejectReason").focus();
            return toastr.error("Must give reject reason.");
        }

        data.CompanyID = $formEl.find("#CompanyID").val();
        data.TriggerPointID = $formEl.find("#TriggerPointID").val();
        data.IsRNDPR = convertToBoolean(data.IsRNDPR);
        data.IsCPR = isCPRPage;
        data.IsFPR = isFPRPage;
        data.SubGroupName = masterData.SubGroupName;
        data.IsSendForCheck = SendForCheck;
        data.SendForApproval = SendForApproval;
        data.IsCheckReject = isCheckReject;

        var url = isCPRPage ? "/api/cda-pr/save-cpr" : (isFPRPage ? "/api/cda-pr/save-fpr" : "/api/cda-pr/save");

        axios.post(url, data)
            .then(function (response) {
                showBootboxAlert("CDA PR No: <b>" + response.data + "</b> saved successfully.");
                backToList();
            })
            .catch(showResponseError);
    }

    function approvePR(id) {
        var url = `/api/cda-pr/approve/${id}`;
        axios.post(url)
            .then(function () {
                toastr.success(constants.APPROVE_SUCCESSFULLY);
                backToList();
            })
            .catch(showResponseError);
    }

    function acknowledgePR(id) {
        var url = `/api/cda-pr/acknowledge/${id}`;
        axios.post(url)
            .then(function () {
                toastr.success(constants.ACKNOWLEDGE_SUCCESSFULLY);
                backToList();
            })
            .catch(showResponseError);
    }

    function rejectPR(id, reason) {
        var url = `/api/cda-pr/reject/${id}/${reason}`;
        axios.post(url)
            .then(function () {
                toastr.success(constants.UNAPPROVE_SUCCESSFULLY);
                backToList();
            })
            .catch(showResponseError);
    }

    async function initChildTable(data) {
        if ($tblChildEl) $tblChildEl.destroy();
        var columns = [], SubGroupColumns = [];
        columns = [
            {
                headerText: '', commands: [
                    { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } }
                ], width: 60
            }
        ];

        if (masterData.SubGroupName === subGroupNames.DYES) {
            SubGroupColumns = [
                { field: 'ItemMasterID', visible: false },
                { field: 'Segment1ValueId', visible: false },
                { field: 'Segment2ValueId', visible: false },
                { field: 'UnitID', visible: false },
                { field: 'Segment1ValueDesc', headerText: 'Primary Group', allowEditing: false, width: 100 },
                { field: 'Segment2ValueDesc', headerText: 'Item Name', allowEditing: false, width: 300 }
            ];
            columns.push.apply(columns, SubGroupColumns);
            //columns = await getDyesItemColumnsAsync(isEditable);
        }
        else {
            SubGroupColumns = [
                { field: 'ItemMasterID', visible: false },
                { field: 'Segment1ValueId', visible: false },
                { field: 'Segment2ValueId', visible: false },
                { field: 'Segment3ValueId', visible: false },
                { field: 'Segment4ValueId', visible: false },
                { field: 'UnitID', visible: false },
                { field: 'Segment1ValueDesc', headerText: 'Primary Group', allowEditing: false, width: 80 },
                { field: 'Segment2ValueDesc', headerText: 'Agent', allowEditing: false, width: 120 },
                { field: 'Segment3ValueDesc', headerText: 'Item Name', allowEditing: false, width: 200 },
                { field: 'Segment4ValueDesc', headerText: 'Form', allowEditing: false, width: 80 }
            ];
            columns.push.apply(columns, SubGroupColumns);
            //columns = await getChemicalsItemColumnsAsync(isEditable);
        }

        //if (isPRPage && status == statusConstants.PENDING) {
        //    columns.push.apply(columns,
        //        [
        //            {
        //                field: 'CompanyID', headerText: 'Company', valueAccessor: ej2GridDisplayFormatter,
        //                dataSource: masterData.CompanyList, displayField: "CompanyName", edit: ej2GridDropDownObj({

        //                })
        //            }
        //        ]
        //    )
        //} else {
        //    columns.push.apply(columns,
        //        [
        //            {
        //                field: 'CompanyName', headerText: 'Company', allowEditing: false
        //            }
        //        ]
        //    )
        //} 
        var additionalColumns = [
            { field: 'CDAPRChildID', isPrimaryKey: true, visible: false },
            { field: 'CDAIndentChildID', visible: false },
            //{ field: 'ReqQty', headerText: 'Req Qty(Kg)', allowEditing: isDisplaySuggestedQty, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 2, min: 0 } } },
            { field: 'ReqQty', headerText: 'Req Qty(Kg)', width: 80, textAlign: 'Right', allowEditing: true, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 0, format: "N" } } },
            { field: 'SuggestedQty', headerText: 'Suggested Qty(Kg)', width: 80, textAlign: 'Right', allowEditing: status === statusConstants.PROPOSED || status === statusConstants.PENDING, visible: isPRCPage || isDisplaySuggestedQty, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 0, format: "N" } } },
            { field: 'Remarks', allowEditing: false, headerText: 'Remarks' }
        ];
        //debugger;
        columns.push.apply(columns, additionalColumns);

        var tableOptions = {
            tableId: tblChildId,
            data: data,
            columns: columns,
            editSettings: { allowEditing: true, mode: "Normal" },
            actionBegin: function (args) {
                if (args.requestType === "add") {
                    args.data.CDAPRChildID = getMaxIdForArray(masterData.Childs, "CDAPRChildID");
                }
                else if (args.requestType === "delete") {
                    //debugger
                    var index = $tblChildEl.getRowIndexByPrimaryKey(masterData.Childs, "CDAPRChildID");
                    //masterData.Childs[index] = args.data;  

                    var ChildsItemSegmentsList = $tblChildEl.getCurrentViewRecords();
                    masterData.ChildsItemSegments[0].ItemIDs = ChildsItemSegmentsList.map(function (el) {
                        if (args.data[0].ItemMasterID != el.ItemMasterID) {
                            return el.ItemMasterID
                        }
                    }).toString();
                }
                else if (args.requestType === "save") {
                    //debugger
                    var index = $tblChildEl.getRowIndexByPrimaryKey(args.rowData.CDAPRChildID);
                    //masterData.Childs[index] = args.data; 
                }
            },
            commandClick: childCommandClick,
            autofitColumns: false,
            showDefaultToolbar: false,
            allowFiltering: false
        };

        if (isEditable) {
            //tableOptions["toolbar"] = ['Add'];
            tableOptions["editSettings"] = { allowAdding: true, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true };
        }
        //else if ((isCPRPage && status === statusConstants.PENDING) || (isFPRPage && status === statusConstants.PENDING)) {
        //    tableOptions["editSettings"] = { allowEditing: true };
        //}
        //else if ((isCPRPage && status === statusConstants.PENDING) || (isFPRPage && status === statusConstants.PENDING)) {
        //    tableOptions["editSettings"] = { allowEditing: true };
        //}
        $tblChildEl = new initEJ2Grid(tableOptions);
    }

    function childCommandClick(args) {
        if (args.commandColumn.buttonOption.type == 'companySearch') {
            var childData = args.rowData;
            axios.get(`/api/cda-pr/commercial-company/${childData.CDAPRChildID}`)
                .then(function (response) {
                    initCompanyTable(response.data);
                    $formEl.find("#modal-child").modal('show');
                })
                .catch(showResponseError);
        }
    }

    function initCompanyTable(data) {
        if ($tblCompanyModalEl) {
            $tblCompanyModalEl.destroy();
        }
        var columnList1 = [];
        columnList1.push(
            {
                field: "FPRCompanyName",
                headerText: "Company Name"
            }
        );

        ej.base.enableRipple(true);
        $tblCompanyModalEl = new ej.grids.Grid({
            dataSource: data,
            allowResizing: true,
            //autofitColumns: true,
            //editSettings: { allowAdding: true, allowEditing: true, allowDeleting: true },
            columns: columnList1
        });
        $tblCompanyModalEl.refreshColumns;
        $tblCompanyModalEl.appendTo(tblCompanyModalId);
    }

    function showHideControls() {
        if (masterData.Reject) {
            $formEl.find("#divRejectReason").show();
        } else {
            $formEl.find("#divRejectReason").hide();
        }
        if (isEditable) {
            $formEl.find("#CompanyID").prop("disabled", false);
            $formEl.find("#TriggerPointID").prop("disabled", false);
            $formEl.find("#CDAPRBy").prop("disabled", false);
            $formEl.find("#CDAPRRequiredDate").prop("disabled", false);
            //$formEl.find("#IsRNDPR").prop("disabled", false);
            $formEl.find("#Remarks").prop("disabled", false);
        }
        else {
            $formEl.find("#CompanyID").prop("disabled", true);
            $formEl.find("#TriggerPointID").prop("disabled", true);
            $formEl.find("#CDAPRBy").prop("disabled", true);
            $formEl.find("#CDAPRRequiredDate").prop("disabled", true);
            //$formEl.find("#IsRNDPR").prop("disabled", true);
            $formEl.find("#Remarks").prop("disabled", true);
        }
    }

    var validationConstraints = {
        TriggerPointID: {
            presence: true
        },
        CDAPRDate: {
            presence: true
        },
        CompanyID: {
            presence: true
        },
    }
})();