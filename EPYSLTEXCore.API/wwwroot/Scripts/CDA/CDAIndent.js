(function () {
    // #region variables
    var menuId, pageName, menuParam;
    var toolbarId;

    var isCDAPage = false;
    var isApprovePage = false;
    var isAcknowledgePage = false;
    var isTexAcknowledgePage = false;
    var isCheckPage = false;

    var isEditable = false;

    var subGroupId, subGroupName;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, tblMasterId, $tblChildEl, tblChildId, $formEl, tblCompanyModalId, $tblCompanyModalEl;
    var status = statusConstants.PENDING;
    var masterData = {};
    var maxCol = 999;
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

        if (menuParam == "Ack") isAcknowledgePage = true;
        else if (menuParam == "A") isApprovePage = true;

        isCDAPage = convertToBoolean($(`#${pageId}`).find("#CDAIndentPage").val());
        isApprovePage = convertToBoolean($(`#${pageId}`).find("#ApprovePage").val());
        isAcknowledgePage = convertToBoolean($(`#${pageId}`).find("#AcknowledgePage").val());
        isTexAcknowledgePage = convertToBoolean($(`#${pageId}`).find("#TexAcknowledgePage").val());
        isCheckPage = convertToBoolean($(`#${pageId}`).find("#CheckPage").val());

        //Hide All Buttons
        $toolbarEl.find("#btnNew,#btnIndentList,#btnPendingForApproval,#btnApprovalList,#btnPendingForAcknowledge,#btnAcknowledgeList,#btnPendingForTexAcknowledge,#btnTexAcknowledgeList,#btnPendingCheckList,#btnCheckList").hide();

        if (isCDAPage) {
            $toolbarEl.find("#btnNew,#btnIndentList,#btnPendingForApproval,#btnApprovalList,#btnPendingForAcknowledge,#btnAcknowledgeList,#btnPendingForTexAcknowledge,#btnTexAcknowledgeList,#btnPendingCheckList,#btnCheckList").show();

            toggleActiveToolbarBtn($toolbarEl.find("#btnIndentList"), $toolbarEl);
            status = statusConstants.PENDING;
            isEditable = false;
        }
        else if (isCheckPage) {
            $toolbarEl.find("#btnPendingCheckList,#btnCheckList").show();

            toggleActiveToolbarBtn($toolbarEl.find("#btnPendingCheckList"), $toolbarEl);
            status = statusConstants.PROPOSED;
            isEditable = false;
        }
        else if (isApprovePage) {
            $toolbarEl.find("#btnPendingForApproval,#btnApprovalList").show();

            toggleActiveToolbarBtn($toolbarEl.find("#btnPendingForApproval"), $toolbarEl);
            status = statusConstants.PROPOSED_FOR_APPROVAL;
            isEditable = false;
        }
        else if (isAcknowledgePage) {
            $toolbarEl.find("#btnPendingForAcknowledge,#btnAcknowledgeList").show();

            toggleActiveToolbarBtn($toolbarEl.find("#btnPendingForAcknowledge"), $toolbarEl);
            status = statusConstants.PROPOSED_FOR_ACKNOWLEDGE;
            isEditable = false;
        }
        else if (isTexAcknowledgePage) {
            $toolbarEl.find("#btnPendingForTexAcknowledge,#btnTexAcknowledgeList").show();

            toggleActiveToolbarBtn($toolbarEl.find("#btnPendingForTexAcknowledge"), $toolbarEl);
            status = statusConstants.PROPOSED_FOR_ACKNOWLEDGE_ACCEPTENCE;
            isEditable = false;
        }
        initMasterTable();
        $("#btnChangeSubGroup").on("click", changeSubGroup);
        $toolbarEl.find("#btnNew").on("click", showSubGroupSelection);
        $toolbarEl.find("#btnIndentList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PENDING;
            isEditable = true;
            initMasterTable();
        });
        $toolbarEl.find("#btnPendingCheckList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PROPOSED;
            isEditable = true;
            initMasterTable();
        });
        $toolbarEl.find("#btnCheckList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.CHECK;
            isEditable = true;
            initMasterTable();
        });
        $toolbarEl.find("#btnPendingForApproval").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PROPOSED_FOR_APPROVAL;
            isEditable = false;
            initMasterTable();
        });
        $toolbarEl.find("#btnApprovalList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.APPROVED;
            isEditable = false;
            initMasterTable();
        });
        $toolbarEl.find("#btnPendingForAcknowledge").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PROPOSED_FOR_ACKNOWLEDGE;
            isEditable = false;
            initMasterTable();
        });
        $toolbarEl.find("#btnAcknowledgeList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.ACKNOWLEDGE;
            isEditable = false;
            initMasterTable();
        });
        $toolbarEl.find("#btnPendingForTexAcknowledge").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PROPOSED_FOR_ACKNOWLEDGE_ACCEPTENCE;
            isEditable = false;
            initMasterTable();
        });
        $toolbarEl.find("#btnTexAcknowledgeList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.ACKNOWLEDGE_ACCEPTENCE;
            isEditable = false;
            initMasterTable();
        });

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save(false, false, false, false, false);
        });
        $formEl.find("#btnSaveAndSendForApprove").click(function (e) {
            e.preventDefault();
            save(true, false, false, false, false);
        });
        $formEl.find("#btnApprove").click(function (e) {
            e.preventDefault();
            save(false, true, false, false, false);
        });
        $formEl.find("#btnAcknowledge").click(function (e) {
            e.preventDefault();
            save(false, false, true, false, false);
        });
        $formEl.find("#btnTexAcknowledge").click(function (e) {
            e.preventDefault();
            save(false, false, false, true, false);
        });
        $formEl.find("#btnCheck").click(function (e) {
            e.preventDefault();
            save(false, false, false, false, true);
        });

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
                    var ChildsItemSegmentsList = $tblChildEl.getCurrentViewRecords();

                    for (var i = 0; i < selectedRecords.length; i++) {
                        var exists = ChildsItemSegmentsList.find(function (el) { return el.ItemMasterID == selectedRecords[i].ItemMasterID })
                        if (!exists) {
                            var oPreProcess = {
                                /*CDAPRChildID: getMaxIdForArray(masterData.ChildsItemSegmentsPost, "CDAPRChildID"),*/
                                CDAIndentChildID: getMaxIdForArray(masterData.ChildsItemSegmentsPost, "CDAIndentChildID"),
                                CDAIndentMasterID: 0,
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
                                ChildItems: []
                            }
                            masterData.ChildsItemSegments[0].ItemIDs = selectedRecords.map(function (el) { return el.ItemMasterID }).toString();
                            masterData.ChildsItemSegmentsPost.push(oPreProcess);
                        }
                    }
                    console.log(masterData.ChildsItemSegmentsPost);
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

    function initMasterTable() {
        
        $formEl.find("#btnChangeSubGroup").hide();
        var showAck = false, showTexAck = false;
        if (status == statusConstants.ACKNOWLEDGE || status == statusConstants.PROPOSED_FOR_ACKNOWLEDGE_ACCEPTENCE) { showAck = true; }
        else if (status == statusConstants.ACKNOWLEDGE_ACCEPTENCE) { showAck = true; showTexAck = true; }
        var commands = [];
        if (isCDAPage) {
            if (status == statusConstants.PENDING) {
                commands = [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                    { type: 'CDAIndentReport', title: 'CDA Indent', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                ]
            }
            else {
                commands = [
                    { type: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                    { type: 'CDAIndentReport', title: 'CDA Indent', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                ]
            }
        }
        if (isCheckPage) {
            if (status == statusConstants.PROPOSED) {
                commands = [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                    { type: 'CDAIndentReport', title: 'CDA Indent', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                ]
            }
            else {
                commands = [
                    { type: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                    { type: 'CDAIndentReport', title: 'CDA Indent', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                ]
            }
        }
        else if (isApprovePage) {
            if (status == statusConstants.APPROVED || status == statusConstants.REJECT) {
                commands = [
                    { type: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                    { type: 'CDAIndentReport', title: 'CDA Indent', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                ]
            }
            else {
                commands = [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                    //{ type: 'Approve', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-check' } }
                    { type: 'CDAIndentReport', title: 'CDA Indent', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                ]
            }
        }
        else if (isAcknowledgePage) {
            if (status == statusConstants.ACKNOWLEDGE || status == statusConstants.UN_ACKNOWLEDGE) {
                commands = [
                    { type: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                    { type: 'CDAIndentReport', title: 'CDA Indent', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                ]
            } else {
                commands = [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                    //{ type: 'Acknowledge', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-check' } }
                    { type: 'CDAIndentReport', title: 'CDA Indent', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                ]
            }
        }
        else if (isTexAcknowledgePage) {
            if (status == statusConstants.ACKNOWLEDGE) {
                commands = [
                    { type: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                    { type: 'CDAIndentReport', title: 'CDA Indent', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                ]
            } else {
                commands = [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                    //{ type: 'Acknowledge', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-check' } }
                    { type: 'CDAIndentReport', title: 'CDA Indent', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                ]
            }
        }

        var columns = [
            {
                headerText: '', commands: commands, textAlign: 'Center', width: 80
            },
            {
                field: 'SubGroupID', visible: false
            },
            {
                field: 'IndentNo', headerText: 'Indent No', width: 100
            },
            {
                field: 'IndentDate', headerText: 'Indent Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, width: 100
            },
            //{
            //    field: 'TriggerPoint', headerText: 'Trigger Point', width: 120
            //},
            {
                field: 'CDAIndentByUser', headerText: 'Indent By', width: 120
            },
            {
                field: 'Remarks', headerText: 'Remarks', width: 200
            },
            {
                field: 'AcknowledgeDate', headerText: 'Commercial Ack Date', textAlign: 'Center', type: 'date', format: _ch_date_format_5, visible: showAck, width: 100
            },
            {
                field: 'TexAcknowledgeDate', headerText: 'Acknowledge Date', textAlign: 'Center', type: 'date', format: _ch_date_format_5, visible: showTexAck, width: 100
            }
        ];

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: false,
            apiEndPoint: `/api/cda-indent/list?status=${status}&pageName=${pageName}`,
            columns: columns,
            commandClick: handleCommands
        });
    }

    function handleCommands(args) {
        var vSubGroupName = "";
        if (args.rowData.SubGroupID == "100") {
            vSubGroupName = "Dyes_Group";
        } else {
            vSubGroupName = "Chemicals_Group";
        }
        if (args.commandColumn.type == 'Edit') {
            internalBtnHideShow(args.commandColumn.type);
            if (isCDAPage) isEditable = true;
            else if (isApprovePage || isAcknowledgePage) isEditable = false;
            getDetails(args.rowData.CDAIndentMasterID, vSubGroupName);
        }
        else if (args.commandColumn.type == 'View') {
            getDetails(args.rowData.CDAIndentMasterID, vSubGroupName);
            internalBtnHideShow(args.commandColumn.type);
            isEditable = false;
        }
        else if (args.commandColumn.type == 'Approve') {
            getDetails(args.rowData.CDAIndentMasterID, vSubGroupName);
            internalBtnHideShow(args.commandColumn.type);
            isEditable = false;
        }
        else if (args.commandColumn.type == 'Acknowledge') {
            getDetails(args.rowData.CDAIndentMasterID, vSubGroupName);
            internalBtnHideShow(args.commandColumn.type);
            isEditable = false;
        }
        else if (args.commandColumn.type == 'TexAcknowledge') {
            getDetails(args.rowData.CDAIndentMasterID, vSubGroupName);
            internalBtnHideShow(args.commandColumn.type);
            isEditable = false;
        }
        //else if (args.commandColumn.type == 'Reject') {
        //    showBootboxPrompt("Reject Yarn PR", "Are you sure you want to Reject this PR?", function (result) {
        //        if (result) {
        //            rejectPR(args.rowData.CDAIndentMasterID, result);
        //        }
        //    });
        //}

        //else if (args.commandColumn.type == 'UnAcknowledge') {
        //    showBootboxPrompt("Reject Yarn PR", "Are you sure you want to Reject this PR?", function (result) {
        //        if (result) {
        //            unacknowledgePR(args.rowData.CDAIndentMasterID, result);
        //        }
        //    });
        //}
        else if (args.commandColumn.type == "CDAIndentReport") {
            window.open(`/reports/InlinePdfView?ReportName=CDAIndent.rdl&IndentNo=${args.rowData.IndentNo}`, '_blank');
        }
    }
    function internalBtnHideShow(buttonType) {
        $formEl.find("#btnAddItems").hide();
        $formEl.find("#btnSave,#btnSaveAndSendForApprove").hide();
        $formEl.find("#btnApprove,#btnAcknowledge,#btnTexAcknowledge,#btnCheck").hide();

        if (buttonType == 'Edit') {
            if (isCDAPage && status == statusConstants.PENDING) {
                $formEl.find("#btnSave,#btnSaveAndSendForApprove").show();
                $formEl.find("#btnAddItems").show();
            }
            else if (isCheckPage && status == statusConstants.PROPOSED) {
                $formEl.find("#btnCheck").show();
            }
            else if (isApprovePage && status == statusConstants.PROPOSED_FOR_APPROVAL) {
                $formEl.find("#btnApprove").show();
            }
            else if (isAcknowledgePage && status == statusConstants.PROPOSED_FOR_ACKNOWLEDGE) {
                $formEl.find("#btnAcknowledge").show();
            }
            else if (isTexAcknowledgePage && status == statusConstants.PROPOSED_FOR_ACKNOWLEDGE_ACCEPTENCE) {
                $formEl.find("#btnTexAcknowledge").show();
            }
        }
        else if (buttonType == 'View') {
            //if (isApprovePage && status == statusConstants.PROPOSED_FOR_APPROVAL) {
            //    $formEl.find("#btnApprove").show();
            //} else if (isAcknowledgePage && status == statusConstants.PROPOSED_FOR_ACKNOWLEDGE) {
            //    $formEl.find("#btnAcknowledge").show();
            //}
        }
    }

    function backToList() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        initMasterTable();
    }

    function resetForm() {
        filterBy = {};
        $formEl.trigger("reset");
        $.each($formEl.find('select'), function (i, el) {
            $(el).select2('');
        });
        $formEl.find("#CDAIndentMasterID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function showSubGroupSelection() {
        axios.get("/api/cda-indent/cda-dyes-chemical")
            .then(function (response) {
                showBootboxSelect2Dialog("Select Sub Group", "ddlSubGroupID", "Choose SubGroup", response.data.SubGroupList, function (data) {
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
        $formEl.find("#btnAddItems,#btnChangeSubGroup").show();
        $formEl.find("#btnSave,#btnSaveAndSendForApprove").show();
        $formEl.find("#btnApprove,#btnAcknowledge,#btnTexAcknowledge,#btnCheck").hide();

        axios.get(`/api/cda-indent/new/${SubGroupName}`)
            .then(function (response) {
                isEditable = true;
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.IndentDate = formatDateToDefault(masterData.IndentDate);
                masterData.IndentStartMonth = formatDateToDefault(masterData.IndentStartMonth);
                masterData.IndentEndMonth = formatDateToDefault(masterData.IndentEndMonth);
                masterData.SubGroupID = subGroupId;
                masterData.SubGroupName = subGroupName;
                setFormData($formEl, masterData);
                $formEl.find("#SubGroupID").val(masterData.SubGroupID);
                initChildTable(masterData.Childs);
                showHideControls();
                $formEl.find("#CIndentBy").prop("disabled", true);
            })
            .catch(showResponseError);
    }

    function getDetails(id, SubGroupName) {
        var url = `/api/cda-indent/${id}/${SubGroupName}`;
        axios.get(url)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;

                masterData.IndentDate = formatDateToDefault(masterData.IndentDate);
                masterData.IndentStartMonth = formatDateToDefault(masterData.IndentStartMonth);
                masterData.IndentEndMonth = formatDateToDefault(masterData.IndentEndMonth);

                if ((isCheckPage && status == statusConstants.PROPOSED) || (isApprovePage && status == statusConstants.PROPOSED_FOR_APPROVAL) || (isAcknowledgePage && status == statusConstants.PROPOSED_FOR_ACKNOWLEDGE)) {
                    for (var j = 0; j < masterData.Childs.length; j++) {
                        if (isCheckPage && status == statusConstants.PROPOSED && masterData.Childs[j].CheckQty == 0) masterData.Childs[j].CheckQty = masterData.Childs[j].IndentQty;
                        if (isApprovePage && status == statusConstants.PROPOSED_FOR_APPROVAL && masterData.Childs[j].ApprovQty == 0) masterData.Childs[j].ApprovQty = masterData.Childs[j].CheckQty;
                        if (isAcknowledgePage && status == statusConstants.PROPOSED_FOR_ACKNOWLEDGE && masterData.Childs[j].ReqQty == 0) masterData.Childs[j].ReqQty = masterData.Childs[j].ApprovQty;
                        for (var i = 0; i < masterData.Childs[j].ChildItems.length; i++) {
                            if (isCheckPage && status == statusConstants.PROPOSED && masterData.Childs[j].ChildItems[i].CheckQty == 0) masterData.Childs[j].ChildItems[i].CheckQty = masterData.Childs[j].ChildItems[i].IndentQty;
                            if (isApprovePage && status == statusConstants.PROPOSED_FOR_APPROVAL && masterData.Childs[j].ChildItems[i].ApprovQty == 0) masterData.Childs[j].ChildItems[i].ApprovQty = masterData.Childs[j].ChildItems[i].CheckQty;
                            if (isAcknowledgePage && status == statusConstants.PROPOSED_FOR_ACKNOWLEDGE && masterData.Childs[j].ChildItems[i].DetailsQTY == 0) masterData.Childs[j].ChildItems[i].DetailsQTY = masterData.Childs[j].ChildItems[i].ApprovQty;
                        }
                    }
                }

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

    function save(isSendForApprove, isApprove, isAck, isTexAck, isCheck) {
        var data = formDataToJson($formEl.serializeArray());
        data.CIndentBy = $formEl.find('#CIndentBy').find(":selected").val();
        if (data.CIndentBy == null || data.CIndentBy == 0) {
            toastr.error("Give indent by.");
            return false;
        }
        var ChildData = $tblChildEl.getCurrentViewRecords();
        var indentError = false;

        ChildData.forEach(function (value) {
            var index = ChildData.findIndex(x => x.CDAIndentChildID == value.CDAIndentChildID);
            if (value.IndentQty == 0 || value.IndentQty === undefined) {
                toastr.error("You Can not leave Indent Qty Blank Or 0");
                indentError = true;
            }
            if (isSendForApprove == true) {
                if (value.ChildItems.length == 0) {
                    toastr.error("You must give month Wise indent Qty for sending");
                    indentError = true;
                    var rowElement = $tblChildEl.getRowByIndex(index);
                    rowElement.classList.add("row-highlight");
                }
                else {
                    var rowElement = $tblChildEl.getRowByIndex(index);
                    rowElement.classList.add("row-default");
                }
            }

            if (value.ChildItems.length > 0) {
                if (isCDAPage) {
                    var indentQty = value.IndentQty;
                    const monthlyTotalIndentQty = value.ChildItems.reduce((total, singleRecord) => total + singleRecord.IndentQty, 0);
                    if (monthlyTotalIndentQty != indentQty) {
                        toastr.error("Month wise indent qty total value must be total indent qty in index " + index);
                        indentError = true;
                        var rowElement = $tblChildEl.getRowByIndex(index);
                        rowElement.classList.add("row-highlight");
                    }
                    else {
                        var rowElement = $tblChildEl.getRowByIndex(index);
                        rowElement.classList.add("row-default");
                    }
                }
                else if (isCheckPage) {
                    var checkQty = value.CheckQty;
                    const monthlyTotalCheckQty = value.ChildItems.reduce((total, singleRecord) => total + singleRecord.CheckQty, 0);
                    if (monthlyTotalCheckQty != checkQty) {
                        toastr.error("Month wise Check qty total value must be total check qty in index " + index);
                        indentError = true;
                        var rowElement = $tblChildEl.getRowByIndex(index);
                        rowElement.classList.add("row-highlight");
                    }
                    else {
                        var rowElement = $tblChildEl.getRowByIndex(index);
                        rowElement.classList.add("row-default");
                    }
                }
                else if (isApprovePage) {
                    var approveQty = value.ApprovQty;
                    const monthlyTotalApproveQty = value.ChildItems.reduce((total, singleRecord) => total + singleRecord.ApprovQty, 0);
                    if (monthlyTotalApproveQty != approveQty) {
                        toastr.error("Month wise Approve qty total value must be total Approve qty in index " + index);
                        indentError = true;
                        var rowElement = $tblChildEl.getRowByIndex(index);
                        rowElement.classList.add("row-highlight");
                    }
                    else {
                        var rowElement = $tblChildEl.getRowByIndex(index);
                        rowElement.classList.add("row-default");
                    }
                }
                else if (isAcknowledgePage) {
                    var detailsQty = value.ReqQty;
                    const monthlyTotalDetailsQty = value.ChildItems.reduce((total, singleRecord) => total + singleRecord.DetailsQTY, 0);
                    if (monthlyTotalDetailsQty != detailsQty) {
                        toastr.error("Month wise Req qty total value must be total Req qty in index " + index);
                        indentError = true;
                        var rowElement = $tblChildEl.getRowByIndex(index);
                        rowElement.classList.add("row-highlight");
                    }
                    else {
                        var rowElement = $tblChildEl.getRowByIndex(index);
                        rowElement.classList.add("row-default");
                    }
                }
            }
        })
        if (indentError == true) return false;

        for (var j = 0; j < ChildData.length; j++) {
            for (var i = 0; i < ChildData[j].ChildItems.length; i++) {
                ChildData[j].ChildItems[i].BookingDate = new Date(ChildData[j].ChildItems[i].BookingDate).toDateString();
            }
        }
        data.Childs = ChildData;

        initializeValidation($formEl, validationConstraints);
        if (!isValidForm($formEl, validationConstraints)) return toastr.error("Please correct all validation errors!");
        else hideValidationErrors($formEl);
        if (data.Childs.length === 0) return toastr.error("At least 1 Yarn items is required.");
        data.SubGroupName = masterData.SubGroupName;

        data.IsSendForApprove = isSendForApprove;
        data.IsApporve = isApprove;
        data.IsAck = isAck;
        data.IsTexAck = isTexAck;
        data.IsCheck = isCheck;
        if (isAck) {
            for (var j = 0; j < data.Childs.length; j++) {
                if (!data.Childs[j].CompanyID) {
                    toastr.warning('Please select Company in Yarn Information Row No ' + j + ' !');
                    return false;
                }
                if (!data.Childs[j].HSCode) {
                    toastr.warning('Please enter HSCode in Yarn Information Row No ' + j + ' !');
                    return false;
                }
            }
        }
        var url = "/api/cda-indent/save";
        axios.post(url, data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(showResponseError);
    }

    async function initChildTable(data) {
        if ($tblChildEl) $tblChildEl.destroy();
        var columns = [], additionalColumns = [], childDetailscolumns = [];

        columns = [
            {
                headerText: 'Commands', commands: [
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
        }
        var isEditableAck = false,
            isDisplay = false, isIndentQtyDis = false, isCheckQtyDis = false, isApprovQtyDis = false, isReqQtyDis = false;
        if (isCDAPage) {
            isIndentQtyDis = true;
            if (status == statusConstants.CHECK || status == statusConstants.APPROVED || status == statusConstants.ACKNOWLEDGE || status == statusConstants.PROPOSED_FOR_ACKNOWLEDGE_ACCEPTENCE || status == statusConstants.ACKNOWLEDGE_ACCEPTENCE) isCheckQtyDis = true;
            if (status == statusConstants.APPROVED || status == statusConstants.ACKNOWLEDGE || status == statusConstants.PROPOSED_FOR_ACKNOWLEDGE_ACCEPTENCE || status == statusConstants.ACKNOWLEDGE_ACCEPTENCE) isApprovQtyDis = true;
            if (status == statusConstants.ACKNOWLEDGE || status == statusConstants.PROPOSED_FOR_ACKNOWLEDGE_ACCEPTENCE || status == statusConstants.ACKNOWLEDGE_ACCEPTENCE) isReqQtyDis = true;
        }
        else if (isCheckPage) {
            isIndentQtyDis = true;
            isCheckQtyDis = true;
        }
        else if (isApprovePage) {
            isCheckQtyDis = true;
            isApprovQtyDis = true;
        }
        else if (isAcknowledgePage) {
            isApprovQtyDis = true;
            isReqQtyDis = true;
            if (status == statusConstants.PROPOSED_FOR_ACKNOWLEDGE) {
                isEditableAck = true;
                isDisplay = true;
            }
        }
        else if (isTexAcknowledgePage) {
            isIndentQtyDis = true;
            isCheckQtyDis = true;
            isApprovQtyDis = true;
            isReqQtyDis = true;
            if (status == statusConstants.PROPOSED_FOR_ACKNOWLEDGE_ACCEPTENCE) {
                isEditableAck = true;
                isDisplay = true;
            }
        }

        if (status == statusConstants.ACKNOWLEDGE || status == statusConstants.ACKNOWLEDGE_ACCEPTENCE) {
            isDisplay = true;
        }
        if (isDisplay) {
            SubGroupColumns.push({
                field: 'HSCode', headerText: 'HSCode', allowEditing: isEditableAck, width: 80
            });
            SubGroupColumns.push({
                field: 'CompanyID', headerText: 'Company', allowEditing: isEditableAck, valueAccessor: ej2GridDisplayFormatter,
                dataSource: masterData.CompanyList, displayField: "CompanyName",
                edit: ej2GridDropDownObj({})
            });
        }
        columns.push.apply(columns, SubGroupColumns);

        var additionalColumns = [
            /*{ field: 'CDAPRChildID', isPrimaryKey: true, visible: false },*/
            { field: 'CDAIndentChildID', isPrimaryKey: true, visible: false },
            { field: 'CDAIndentMasterID', visible: false },
            { field: 'IndentQty', headerText: 'Indent Qty(Kg)', width: 80, textAlign: 'Right', visible: isIndentQtyDis, allowEditing: isCDAPage && status == statusConstants.PENDING, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 0, format: "N" } } },
            { field: 'CheckQty', headerText: 'Check Qty(Kg)', width: 80, textAlign: 'Right', visible: isCheckQtyDis, allowEditing: isCheckPage && status == statusConstants.PROPOSED, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 0, format: "N" } } },
            { field: 'ApprovQty', headerText: 'Approv Qty(Kg)', width: 80, textAlign: 'Right', visible: isApprovQtyDis, allowEditing: isApprovePage && status == statusConstants.PROPOSED_FOR_APPROVAL, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 0, format: "N" } } },
            { field: 'ReqQty', headerText: 'Req Qty(Kg)', width: 80, textAlign: 'Right', visible: isReqQtyDis, allowEditing: isAcknowledgePage && status == statusConstants.ACKNOWLEDGE, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 0, format: "N" } } },
            { field: 'Remarks', allowEditing: isCDAPage && status == statusConstants.PENDING, headerText: 'Remarks' }
        ];
        columns.push.apply(columns, additionalColumns);

        //Child Items Column
        childDetailscolumns = [
            {
                field: 'Commands', headerText: '', width: 20, textAlign: 'Center', commands: [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                    { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                    { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                    { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }]
            },
            { field: 'CDAIndentChildDetailsID', isPrimaryKey: true, visible: false },
            { field: 'CDAIndentChildID', visible: false },
            { field: 'CDAIndentMasterID', visible: false },
            { field: 'BookingDate', headerText: 'Inhouse Date', type: 'date', format: _ch_date_format_1, editType: 'datepickeredit', width: 40, textAlign: 'Center' }, //valueAccessor: ej2GridDateFormatter,
            { field: 'IndentQty', headerText: 'Indent Qty(Kg)', width: 80, textAlign: 'Center', headerTextAlign: 'Center', visible: isIndentQtyDis, allowEditing: isCDAPage && status == statusConstants.PENDING, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 0, format: "N" } } },
            { field: 'CheckQty', headerText: 'Check Qty(Kg)', width: 80, textAlign: 'Center', headerTextAlign: 'Center', visible: isCheckQtyDis, allowEditing: isCheckPage && status == statusConstants.PROPOSED, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 0, format: "N" } } },
            { field: 'ApprovQty', headerText: 'Approv Qty(Kg)', width: 80, textAlign: 'Center', headerTextAlign: 'Center', visible: isApprovQtyDis, allowEditing: isApprovePage && status == statusConstants.PROPOSED_FOR_APPROVAL, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 0, format: "N" } } },
            { field: 'DetailsQTY', headerText: 'Req Qty(Kg)', width: 80, textAlign: 'Center', headerTextAlign: 'Center', visible: isReqQtyDis, allowEditing: isAcknowledgePage && status == statusConstants.PROPOSED_FOR_ACKNOWLEDGE, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 0, format: "N" } } }
        ];

        ej.base.enableRipple(true);
        $tblChildEl = new ej.grids.Grid({
            dataSource: data,
            allowResizing: true,
            columns: columns,
            editSettings: { allowEditing: true, allowAdding: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
            actionBegin: function (args) {
                if (args.requestType === "add") {
                    args.data.CDAIndentChildID = getMaxIdForArray(masterData.Childs, "CDAIndentChildID");
                }
                else if (args.requestType === "delete") {
                    var index = $tblChildEl.getRowIndexByPrimaryKey(masterData.Childs, "CDAIndentChildID");

                    var ChildsItemSegmentsList = $tblChildEl.getCurrentViewRecords();
                    masterData.ChildsItemSegments[0].ItemIDs = ChildsItemSegmentsList.map(function (el) {
                        if (args.data[0].ItemMasterID != el.ItemMasterID) {
                            return el.ItemMasterID
                        }
                    }).toString();
                }
                else if (args.requestType === "save") {

                    var index = $tblChildEl.getRowIndexByPrimaryKey(args.rowData.CDAIndentChildID);
                }
            },
            commandClick: childCommandClick,
            childGrid: {
                queryString: "CDAIndentChildID",
                additionalQueryParams: "ItemMasterID",
                allowResizing: true,
                autofitColumns: false,
                toolbar: ['Add'],
                editSettings: {
                    allowEditing: true, allowAdding: true, allowDeleting: true,
                    mode: "Normal", showDeleteConfirmDialog: true
                },
                columns: childDetailscolumns,
                actionBegin: function (args) {
                    if (args.requestType === "add") {
                        var totalDis = 0, remainDis = 0, maxDate;

                        if (typeof this.dataSource !== "undefined") {
                            this.dataSource.forEach(l => {
                                if (isCDAPage) totalDis += l.IndentQty;
                                else if (isCheckPage) totalDis += l.CheckQty;
                                else if (isApprovePage) totalDis += l.ApprovQty;
                                else if (isAcknowledgePage) totalDis += l.DetailsQTY;

                                if (!maxDate) {
                                    maxDate = l.BookingDate;
                                } else if (maxDate < l.BookingDate) {
                                    maxDate = l.BookingDate;
                                }
                            });
                        }

                        if (this.parentDetails.parentRowData.IndentQty == null) this.parentDetails.parentRowData.IndentQty = 0;
                        if (this.parentDetails.parentRowData.CheckQty == null) this.parentDetails.parentRowData.CheckQty = 0;
                        if (this.parentDetails.parentRowData.ApprovQty == null) this.parentDetails.parentRowData.ApprovQty = 0;
                        if (this.parentDetails.parentRowData.ReqQty == null) this.parentDetails.parentRowData.ReqQty = 0;

                        if (isCDAPage) {
                            if (totalDis < parseFloat(this.parentDetails.parentRowData.IndentQty)) {
                                remainDis = parseFloat(this.parentDetails.parentRowData.IndentQty) - totalDis;
                            }
                            else {
                                toastr.error("Distribution can not more then 100% !!");
                                args.cancel = true;
                                return;
                            }
                        }
                        else if (isCheckPage) {
                            if (totalDis < parseFloat(this.parentDetails.parentRowData.CheckQty)) {
                                remainDis = parseFloat(this.parentDetails.parentRowData.CheckQty) - totalDis;
                            }
                            else {
                                toastr.error("Distribution can not more then 100% !!");
                                args.cancel = true;
                                return;
                            }
                        }
                        else if (isApprovePage) {
                            if (totalDis < parseFloat(this.parentDetails.parentRowData.ApprovQty)) {
                                remainDis = parseFloat(this.parentDetails.parentRowData.ApprovQty) - totalDis;
                            }
                            else {
                                toastr.error("Distribution can not more then 100% !!");
                                args.cancel = true;
                                return;
                            }
                        }
                        else if (isAcknowledgePage) {
                            if (totalDis < parseFloat(this.parentDetails.parentRowData.ReqQty)) {
                                remainDis = parseFloat(this.parentDetails.parentRowData.ReqQty) - totalDis;
                            }
                            else {
                                toastr.error("Distribution can not more then 100% !!");
                                args.cancel = true;
                                return;
                            }
                        }

                        args.data.CDAIndentChildDetailsID = maxCol++;
                        args.data.CDAIndentChildID = this.parentDetails.parentKeyFieldValue;
                        args.data.CDAIndentMasterID = this.parentDetails.parentRowData.CDAIndentMasterID;

                        if (maxDate) {
                            var tempDate = new Date(maxDate);
                            var nextMonth = tempDate.getMonth() + 1;
                            tempDate.setMonth(nextMonth);
                            args.data.BookingDate = tempDate;
                            maxDate = tempDate;
                        }
                        if (isCDAPage) args.data.IndentQty = remainDis.toFixed(4);
                        else if (isCheckPage) args.data.CheckQty = remainDis.toFixed(4);
                        else if (isApprovePage) args.data.ApprovQty = remainDis.toFixed(4);
                        else if (isAcknowledgePage) args.data.DetailsQTY = remainDis.toFixed(4);
                    }
                    else if (args.requestType === "save") {
                        //new
                        var index = $tblChildEl.getRowIndexByPrimaryKey(args.data.CDAIndentChildID);
                        var ChildItems = masterData.Childs.length > 0 ? masterData.Childs[index].ChildItems : [];
                        var ChildItemsIndex = ChildItems.findIndex(x => x.CDAIndentChildDetailsID == args.data.CDAIndentChildDetailsID);

                        //debugger;
                        //var indexF = this.parentDetails.parentRowData.ChildItems.findIndex(x => x.CDAIndentChildDetailsID == args.data.CDAIndentChildDetailsID);
                        //if (indexF == -1) {
                        //    args.data.CDAIndentChildID = this.parentDetails.parentRowData.CDAIndentChildID;
                        //    args.data.CDAIndentMasterID = this.parentDetails.parentRowData.CDAIndentMasterID;

                        //    this.parentDetails.parentRowData.ChildItems.push(args.data);
                        //    indexF = masterData.Childs.findIndex(x => x.CDAIndentChildID == this.parentDetails.parentRowData.CDAIndentChildID);
                        //    if (indexF > -1) {
                        //        masterData.Childs[index].ChildItems = this.parentDetails.parentRowData.ChildItems;
                        //    }
                        //}

                        if (isCDAPage) {
                            if (ChildItems.length != 0) {
                                ChildItems[ChildItemsIndex].IndentQty = args.data.IndentQty;
                                const monthlyTotalIndentQty = ChildItems.reduce((total, singleRecord) => total + singleRecord.IndentQty, 0);
                                var totalIndentQty = masterData.Childs.length > 0 ? masterData.Childs[index].IndentQty : 0;
                                if (args.data.IndentQty == null || args.data.IndentQty == 0) {
                                    toastr.error("You can not leave Indent Qty Blank or Zero");
                                    return false;
                                }
                                else if (monthlyTotalIndentQty != totalIndentQty) {
                                    toastr.error("Month wise indent qty total value must be total indent qty");
                                    return false;
                                }
                            }
                        }
                        else if (isCheckPage) {
                            ChildItems[ChildItemsIndex].CheckQty = args.data.CheckQty;
                            const monthlyTotalCheckQty = ChildItems.reduce((total, singleRecord) => total + singleRecord.CheckQty, 0);
                            var totalCheckQty = masterData.Childs.length > 0 ? masterData.Childs[index].CheckQty : 0;
                            if (args.data.CheckQty == null || args.data.CheckQty == 0) {
                                toastr.error("You can not leave Check Qty Blank or Zero");
                                return false;
                            }
                            else if (monthlyTotalCheckQty != totalCheckQty) {
                                toastr.error("Month wise Check qty total value must be total Check qty");
                                return false;
                            }
                        }
                        else if (isApprovePage) {
                            ChildItems[ChildItemsIndex].ApprovQty = args.data.ApprovQty;
                            const monthlyTotalApproveQty = ChildItems.reduce((total, singleRecord) => total + singleRecord.ApprovQty, 0);
                            var totalApproveQty = masterData.Childs.length > 0 ? masterData.Childs[index].ApprovQty : 0;
                            if (args.data.ApprovQty == null || args.data.ApprovQty == 0) {
                                toastr.error("You can not leave Approve Qty Blank or Zero");
                                return false;
                            }
                            else if (monthlyTotalApproveQty != totalApproveQty) {
                                toastr.error("Month wise Approve qty total value must be total Approve qty");
                                return false;
                            }
                        }
                        else if (isAcknowledgePage) {
                            ChildItems[ChildItemsIndex].DetailsQTY = args.data.DetailsQTY;
                            const monthlyTotalDetailsQty = ChildItems.reduce((total, singleRecord) => total + singleRecord.DetailsQTY, 0);
                            var totalDetailsQty = masterData.Childs.length > 0 ? masterData.Childs[index].ReqQty : 0;
                            if (args.data.DetailsQTY == null || args.data.DetailsQTY == 0) {
                                toastr.error("You can not leave Req Qty Blank or Zero");
                                return false;
                            }
                            else if (monthlyTotalDetailsQty != totalDetailsQty) {
                                toastr.error("Month wise Req qty total value must be total Req qty");
                                return false;
                            }
                        }
                    }
                },
                load: loadChildItemDetailsGrid
            }
        });
        $tblChildEl.refreshColumns;
        $tblChildEl.appendTo(tblChildId);
    }
    function loadChildItemDetailsGrid() {
        this.dataSource = this.parentDetails.parentRowData.ChildItems;
    }

    function childCommandClick(args) {
        if (args.commandColumn.buttonOption.type == 'companySearch') {
            var childData = args.rowData;
            /*axios.get(`/api/cda-indent/commercial-company/${childData.CDAPRChildID}`)*/
            axios.get(`/api/cda-indent/commercial-company/${childData.CDAIndentChildID}`)
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
        if (isEditable) {
            $formEl.find("#CompanyID").prop("disabled", false);
            //$formEl.find("#TriggerPointID").prop("disabled", false);
            $formEl.find("#Remarks").prop("disabled", false);
            //$formEl.find("#CIndentBy").prop("disabled", false);
        }
        else {
            $formEl.find("#CompanyID").prop("disabled", true);
            //$formEl.find("#TriggerPointID").prop("disabled", true);
            $formEl.find("#Remarks").prop("disabled", true);
            //$formEl.find("#CIndentBy").prop("disabled", false);
        }
    }

    var validationConstraints = {
        //TriggerPointID: {
        //    presence: true
        //},
        IndentDate: {
            presence: true
        }
    }
})();