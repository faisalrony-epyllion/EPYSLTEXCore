(function () {
    // #region variables
    var menuId, pageName, menuParam;
    var toolbarId;
    var isApprovePage = false;
    var isAcknowledgePage = false;
    var isCPRPage = false;
    var isFPRPage = false;
    var isEditable = false;
    var addAdditionalReq = false;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, tblMasterId, $tblChildEl, tblChildId, $formEl, tblCompanyModalId, $tblCompanyModalEl;
    var status = statusConstants.AWAITING_PROPOSE;
    var masterData;
    var prStatus = "";
    var FCMRMasterID;
    var source, yarnPRFromID = 0;
    var _yarnSegments = [];

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
        $toolbarEl.find("#divAddPRForMR").fadeIn();



        if (menuParam == "Ack") isAcknowledgePage = true;
        else if (menuParam == "A") isApprovePage = true;
        else if (menuParam == "CPR") isCPRPage = true;
        else if (menuParam == "FPR") isFPRPage = true;

        

  
        if (isAcknowledgePage) {
            $toolbarEl.find("#btnNewPR,#btnPendingMaterialReqList,#btnAcknowledgedMaterialReqList,#btnPendingRevisionList").hide();
            $toolbarEl.find("#btnEditList,#btnRevisionList").hide();
            $toolbarEl.find("#btnProposedList").hide();
            $toolbarEl.find("#btnApprovedList").hide();
            $toolbarEl.find("#btnRejectdList").hide();
            $toolbarEl.find("#btnAcknowledgeList").show();
            $toolbarEl.find("#btnUnAcknowledgeList").show();
            $toolbarEl.find("#btnPendingAcknowledgeList").show();
            $toolbarEl.find("#btnPendingCPRList").hide();
            $toolbarEl.find("#btnDraftList").hide();
            $toolbarEl.find("#btnCPRList").hide();
            $toolbarEl.find("#btnDraftList").hide();
            $toolbarEl.find("#btnPendingCFRList").hide();
            $toolbarEl.find("#btnCFRList").hide();
            $toolbarEl.find("#btnAllPRList").hide();
            $toolbarEl.find("#btnSave").hide();
            $toolbarEl.find("#btnSaveForApproval").hide();
            $toolbarEl.find("#btnAcknowledge").hide();
            $toolbarEl.find("#btnApproveYPR").hide();
            $toolbarEl.find("#btnRejectYPR").hide();
            $toolbarEl.find("#btnAkgYPR").show();
            $toolbarEl.find("#btnUnAkgYPR").show();
            $toolbarEl.find("#btnNewItem").hide();
            $formEl.find("#btnAcknowledgeMR").hide();

            status = statusConstants.PARTIALLY_COMPLETED;
            $toolbarEl.find("#divAddPRForMR").fadeOut();
            toggleActiveToolbarBtn($toolbarEl.find("#btnPendingAcknowledgeList"), $toolbarEl);
            $formEl.find("#BuyerTeam,#Buyer,#ApvBuyer,#ApvBuyerTeam").hide();

            isEditable = false;
        }
        else if (isApprovePage) {
            $toolbarEl.find("#btnNewPR,#btnPendingMaterialReqList,#btnAcknowledgedMaterialReqList,#btnPendingRevisionList").hide();
            $toolbarEl.find("#btnEditList,#btnRevisionList").hide();
            $toolbarEl.find("#btnProposedList").show();
            $toolbarEl.find("#btnApprovedList").show();
            $toolbarEl.find("#btnRejectdList").show();
            $toolbarEl.find("#btnAcknowledgeList").hide();
            $toolbarEl.find("#btnUnAcknowledgeList").hide();
            $toolbarEl.find("#btnPendingAcknowledgeList").hide();
            $toolbarEl.find("#btnPendingCPRList").hide();
            $toolbarEl.find("#btnDraftList").hide();
            $toolbarEl.find("#btnCPRList").hide();
            $toolbarEl.find("#btnDraftList").hide();
            $toolbarEl.find("#btnPendingCFRList").hide();
            $toolbarEl.find("#btnCFRList").show();
            $toolbarEl.find("#btnAllPRList").hide();
            $toolbarEl.find("#btnSave").hide();
            $toolbarEl.find("#btnSaveForApproval").hide();
            $toolbarEl.find("#btnAcknowledge").hide();

            $toolbarEl.find("#btnApproveYPR").show();
            $toolbarEl.find("#btnRejectYPR").show();
            $toolbarEl.find("#btnAkgYPR").hide();
            $toolbarEl.find("#btnUnAkgYPR").hide();
            $toolbarEl.find("#btnNewItem").hide();
            $formEl.find("#btnAcknowledgeMR").hide();

            status = statusConstants.PROPOSED;
            $toolbarEl.find("#divAddPRForMR").fadeOut();
            toggleActiveToolbarBtn($toolbarEl.find("#btnProposedList"), $toolbarEl);

            isEditable = false;
            $formEl.find("#BuyerTeam,#Buyer").show();
        }
        else if (isCPRPage) {
            $toolbarEl.find("#btnNewPR,#btnPendingMaterialReqList,#btnAcknowledgedMaterialReqList,#btnPendingRevisionList").hide();
            $toolbarEl.find("#btnEditList,#btnRevisionList").hide();
            $toolbarEl.find("#btnProposedList").hide();
            $toolbarEl.find("#btnApprovedList").hide();
            $toolbarEl.find("#btnRejectdList").hide();
            $toolbarEl.find("#btnAcknowledgeList").hide();
            $toolbarEl.find("#btnUnAcknowledgeList").hide();
            $toolbarEl.find("#btnPendingAcknowledgeList").hide();
            $toolbarEl.find("#btnPendingCPRList").show();
            $toolbarEl.find("#btnDraftList").show();
            $toolbarEl.find("#btnCPRList").show();
            $toolbarEl.find("#btnPendingCFRList").hide();
            $toolbarEl.find("#btnCFRList").hide();
            $toolbarEl.find("#btnAllPRList").hide();
            $toolbarEl.find("#btnSave").hide();
            $toolbarEl.find("#btnSaveForApproval").hide();
            $toolbarEl.find("#btnAcknowledge").hide();

            $toolbarEl.find("#btnApproveYPR").hide();
            $toolbarEl.find("#btnRejectYPR").hide();
            $toolbarEl.find("#btnAkgYPR").hide();
            $toolbarEl.find("#btnUnAkgYPR").hide();
            $toolbarEl.find("#btnNewItem").hide();
            $formEl.find("#btnAcknowledgeMR").hide();

            status = statusConstants.PENDING;
            $toolbarEl.find("#divAddPRForMR").fadeOut();
            toggleActiveToolbarBtn($toolbarEl.find("#btnPendingCPRList"), $toolbarEl);
            $formEl.find("#BuyerTeam,#Buyer,#ApvBuyer,#ApvBuyerTeam").hide();
            isEditable = true;
        }
        else if (isFPRPage) {
            $toolbarEl.find("#btnNewPR,#btnPendingMaterialReqList,#btnAcknowledgedMaterialReqList,#btnPendingRevisionList").hide();
            $toolbarEl.find("#btnNewItem").hide();
            $toolbarEl.find("#btnEditList,#btnRevisionList").hide();
            $toolbarEl.find("#btnProposedList").hide();
            $toolbarEl.find("#btnApprovedList").hide();
            $toolbarEl.find("#btnRejectdList").hide();
            $toolbarEl.find("#btnAcknowledgeList").hide();
            $toolbarEl.find("#btnUnAcknowledgeList").hide();
            $toolbarEl.find("#btnPendingAcknowledgeList").hide();
            $toolbarEl.find("#btnPendingCPRList").hide();
            $toolbarEl.find("#btnDraftList").hide();
            $toolbarEl.find("#btnCPRList").hide();
            $toolbarEl.find("#btnPendingCFRList").show();
            $toolbarEl.find("#btnCFRList").show();
            $toolbarEl.find("#btnAllPRList").show();
            //$toolbarEl.find("#btnSave").show();
            $toolbarEl.find("#btnSave").hide();
            $toolbarEl.find("#btnSaveForApproval").hide();
            $toolbarEl.find("#btnAcknowledge").show();
            $toolbarEl.find("#btnApproveYPR").hide();
            $toolbarEl.find("#btnRejectYPR").hide();
            $toolbarEl.find("#btnAkgYPR").hide();
            $toolbarEl.find("#btnUnAkgYPR").hide();
            $toolbarEl.find("#btnNewItem").hide();
            $formEl.find("#btnAcknowledgeMR").hide();

            status = statusConstants.PENDING;
            $toolbarEl.find("#divAddPRForMR").fadeOut();
            toggleActiveToolbarBtn($toolbarEl.find("#btnPendingCFRList"), $toolbarEl);
            $formEl.find("#BuyerTeam,#Buyer,#ApvBuyer,#ApvBuyerTeam").hide();
            isEditable = false;
        }
        else {
            $toolbarEl.find("#btnEditList").show();
            $toolbarEl.find("#btnPendingCPRList").hide();
            $toolbarEl.find("#btnDraftList").hide();
            $toolbarEl.find("#btnCPRList").hide();
            $toolbarEl.find("#btnPendingCFRList").hide();
            $toolbarEl.find("#btnCFRList").show();
            $toolbarEl.find("#btnAllPRList").show();
            $toolbarEl.find("#btnSave").show();
            $toolbarEl.find("#btnSaveForApproval").show();
            $toolbarEl.find("#btnAcknowledge").hide();
            $toolbarEl.find("#btnApproveYPR").hide();
            $toolbarEl.find("#btnRejectYPR").hide();
            $toolbarEl.find("#btnAkgYPR").hide();
            $toolbarEl.find("#btnUnAkgYPR").hide();
            $toolbarEl.find("#btnNewItem").hide();
            $formEl.find("#btnAcknowledgeMR").hide();
            $formEl.find("#BuyerTeam,#Buyer,#ApvBuyer,#ApvBuyerTeam").hide();
            isEditable = true;
        }
        initMasterTable();

        //$toolbarEl.find("#btnList").on("click", function (e) {
        //    e.preventDefault();
        //    toggleActiveToolbarBtn(this, $toolbarEl);
        //    status = statusConstants.COMPLETED;
        //    $toolbarEl.find("#divAddPRForMR").fadeOut();
        //    isEditable = true;
        //    initMasterTable();
        //});

        $toolbarEl.find("#btnEditList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.EDIT;
            $toolbarEl.find("#divAddPRForMR").fadeOut();
            /*isEditable = isApprovePage ? false : true;*/
            $formEl.find("#btnAcknowledge").hide();
            isEditable = true;
            initMasterTable();
        });

        $toolbarEl.find("#btnRevisionList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.REVISE;
            $toolbarEl.find("#divAddPRForMR").fadeOut();
            /*isEditable = isApprovePage ? false : true;*/
            isEditable = true;
            initMasterTable();
        });

        $toolbarEl.find("#btnProposedList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PROPOSED;
            $toolbarEl.find("#divAddPRForMR").fadeOut();
            /*isEditable = isApprovePage ? false : true;*/
            isEditable = false;
            initMasterTable();
        });

        $toolbarEl.find("#btnApprovedList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.APPROVED;
            $toolbarEl.find("#divAddPRForMR").fadeOut();
            isEditable = false;
            initMasterTable();
        });

        $toolbarEl.find("#btnPendingAcknowledgeList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PARTIALLY_COMPLETED;
            $toolbarEl.find("#divAddPRForMR").fadeOut();
            isEditable = false;
            initMasterTable();
        });

        $toolbarEl.find("#btnRejectdList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.REJECT;
            $toolbarEl.find("#divAddPRForMR").fadeOut();
            isEditable = false;
            initMasterTable();
        });

        $toolbarEl.find("#btnAcknowledgeList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.ACKNOWLEDGE;
            $toolbarEl.find("#divAddPRForMR").fadeOut();
            isEditable = false;
            initMasterTable();
        });

        $toolbarEl.find("#btnUnAcknowledgeList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.UN_ACKNOWLEDGE;
            $toolbarEl.find("#divAddPRForMR").fadeOut();
            isEditable = false;
            initMasterTable();
        });

        $toolbarEl.find("#btnPendingCPRList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PENDING;
            $toolbarEl.find("#divAddPRForMR").fadeOut();
            initMasterTable();
        });

        $toolbarEl.find("#btnDraftList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.ACTIVE;
            $toolbarEl.find("#divAddPRForMR").fadeOut();
            initMasterTable();
        });

        //$toolbarEl.find("#btnDraftList").on("click", function (e) {
        //    e.preventDefault();
        //    toggleActiveToolbarBtn(this, $toolbarEl);
        //    status = statusConstants.ACTIVE;
        //    $toolbarEl.find("#divAddPRForMR").fadeOut();
        //    initMasterTable();
        //});

        $toolbarEl.find("#btnCPRList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.COMPLETED;
            $toolbarEl.find("#divAddPRForMR").fadeOut();
            initMasterTable();
        });

        $toolbarEl.find("#btnPendingCFRList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PENDING;
            $toolbarEl.find("#btnSave").hide();
            $toolbarEl.find("#divAddPRForMR").fadeOut();
            initMasterTable();
        });

        $toolbarEl.find("#btnCFRList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.COMPLETED;
            $toolbarEl.find("#divAddPRForMR").fadeOut();
            initMasterTable();
        });

        $toolbarEl.find("#btnAllPRList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.ALL_STATUS;
            $toolbarEl.find("#divAddPRForMR").fadeOut();
            $toolbarEl.find("#btnSave").hide();
            $toolbarEl.find("#btnSaveForApproval").hide();
            $toolbarEl.find("#btnAcknowledge").hide();
            initMasterTable();
        });

        $toolbarEl.find("#btnPendingMaterialReqList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.ADDITIONAL;
            $toolbarEl.find("#divAddPRForMR").fadeOut();
            initMasterTable();
        });

        $toolbarEl.find("#btnAcknowledgedMaterialReqList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.AWAITING_PROPOSE;
            $toolbarEl.find("#divAddPRForMR").fadeIn();
            initMasterTable();
        });
        $toolbarEl.find("#btnPendingRevisionList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PENDING;
            $toolbarEl.find("#divAddPRForMR").fadeIn();
            initMasterTable();
        });

        $toolbarEl.find("#btnRefreshList").on("click", function (e) {
            e.preventDefault();
            $tblMasterEl.refresh();
        });

        $toolbarEl.find("#btnNewPR").on("click", getNewData);

        $toolbarEl.find("#btnAddPRForMR").on("click", getNewDataPRForMR);

       



        $formEl.find("#btnRejectYPR").click(function (e) {
            e.preventDefault();

            bootbox.prompt("Enter your reject reason:", function (result) {
                if (!result) {
                    return toastr.error("Reject reason is required.");
                }
                var vSource = masterData.Childs[0].Source;
                var id = $formEl.find("#YarnPRMasterID").val();
                var reason = result;
                axios.post(`/api/yarn-pr/reject/${id}/${reason}/${vSource}`)
                    .then(function () {
                        toastr.success("Requisition rejected successfully.");
                        backToList();
                    })
                    .catch(showResponseError);
            });
        });

        $formEl.find("#btnSave").click(function () {
            if (isValidRequiredDate()) {
                save(false);
            }
        });
        $formEl.find("#btnSaveForApproval").click(function () {
            if (isValidRequiredDate()) {
                save(true);
            }
        });

        $formEl.find("#btnAcknowledge").click(function () {
            save(true);
        });

        $formEl.find("#btnAcknowledgeMR").click(acknowledgeMR);

        $formEl.find("#btnApproveYPR").click(function (e) {
            e.preventDefault();
            var vSource = masterData.Childs[0].Source;
            var id = $formEl.find("#YarnPRMasterID").val();
            axios.post(`/api/yarn-pr/approve/${id}/${masterData.YarnPRFromID}/${vSource}`)
                .then(function () {
                    toastr.success(constants.APPROVE_SUCCESSFULLY);
                    reloadMasterTable();
                    //backToList();
                })
                .catch(showResponseError);
        });

        $formEl.find("#btnAkgYPR").click(function (e) {
            e.preventDefault();
            var id = $formEl.find("#YarnPRMasterID").val();
            axios.post(`/api/yarn-pr/acknowledge/${id}`)
                .then(function () {
                    toastr.success("Requisition acknowledged successfully.");
                    backToList();
                })
                .catch(showResponseError);
        });

        $formEl.find("#btnUnakgYPR").click(function (e) {
            e.preventDefault();
            bootbox.prompt("Enter your Un-acknowledge reason:", function (result) {
                if (!result) {
                    return toastr.error("Reject reason is required.");
                }
                var id = $formEl.find("#YarnPRMasterID").val();
                var reason = result;
                axios.post(`/api/yarn-pr/unacknowledge/${id}/${reason}`)
                    .then(function () {
                        toastr.success("Requisition rejected successfully.");
                        backToList();
                    })
                    .catch(showResponseError);
            });
        });

        $formEl.find("#btnCancel").on("click", backToList);

        getYarnSegments();
    });

    function isValidRequiredDate() {
        if (!isCPRPage) {
            var date1 = new Date($formEl.find("#YarnPRDate").val());
            var date2 = new Date($formEl.find("#YarnPRRequiredDate").val());
            var diffDays = parseInt((date2 - date1) / (1000 * 60 * 60 * 24), 10);
            if (diffDays < 1) {
                var date = new Date($formEl.find("#YarnPRDate").val());
                date = date.setDate(date.getDate() + 1)
                var fDate = formatDateToDefault(date);
                masterData.YarnPRRequiredDate = fDate;
                toastr.error(`Minimum required date is ${fDate}`);
                return false;
            }
        }
        return true;
    }

    function initMasterTable() {
        var commands = [];
        if (isApprovePage) {
            if (status == statusConstants.REJECT || status == statusConstants.COMPLETED) {
                commands = [
                    { type: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                    { type: 'Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
                ]
            }
            else if (status == statusConstants.APPROVED) {
                commands = [
                    { type: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                    { type: 'Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
                    //{ type: 'Email', title: 'Send Email', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-envelope-o' } }
                ]
            }
            else {
                commands = [
                    { type: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                    { type: 'Approve', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-check' } },
                    { type: 'Reject', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-ban' } },
                    { type: 'Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                ]
            }
        }

        else if (isAcknowledgePage) {
            if (status == statusConstants.ACKNOWLEDGE || status == statusConstants.UN_ACKNOWLEDGE) {
                commands = [
                    { type: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                    { type: 'Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                ]
            } else {
                commands = [
                    { type: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                    { type: 'Acknowledge', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-check' } },
                    { type: 'UnAcknowledge', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-times' } },
                    { type: 'Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                ]
            }
        }

        else {
            if (isCPRPage) {
                if (status == statusConstants.COMPLETED) {
                    commands = [
                        { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                        { type: 'Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                    ]
                } else {
                    commands = [
                        { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                        { type: 'Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                    ]
                }
            }
            else if (isFPRPage) {
                if (status == statusConstants.COMPLETED || status == statusConstants.ALL_STATUS) {
                    commands = [
                        { type: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                        { type: 'Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                    ]
                } else {
                    commands = [
                        { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                        { type: 'Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                    ]
                }
            }
            else {
                if (status == statusConstants.REJECT || status == statusConstants.ACKNOWLEDGE || status == statusConstants.UN_ACKNOWLEDGE) {
                    commands = [
                        { type: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                        { type: 'Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }

                    ]
                }
                else if (status == statusConstants.APPROVED) {
                    commands = [
                        { type: 'Addition', buttonOption: { cssClass: 'e-flat', iconCss: 'e-add e-icons' } },
                        { type: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                        { type: 'Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }

                    ]
                }
                else if (status == statusConstants.REVISE) {
                    commands = [
                        { type: 'Revise', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } }
                    ]
                }
                else {
                    commands = [
                        { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                        { type: 'Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                    ]
                }
            }
        }

        var columns = [];

        if (status == statusConstants.AWAITING_PROPOSE || status == statusConstants.ADDITIONAL) {
            commands = [
                { type: 'ViewMR', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                { type: 'AcknowledgeMR', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-check', title: "Acknowledge MR" } },
                { type: 'RejectMR', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-ban' } }

            ]
            columns = [
                {
                    headerText: '', commands: commands, visible: status == statusConstants.ADDITIONAL,
                    textAlign: 'Center', width: ch_setActionCommandCellWidth(commands)
                },
                {
                    field: 'YarnPRFromID', visible: false
                },
                {
                    field: 'Source', headerText: 'Source'
                },
                {
                    field: 'RevisionStatus', headerText: 'Status', width: 100
                },
                {
                    field: 'ConceptNo', headerText: 'Concept No', width: 100
                },
                {
                    field: 'ConceptDate', headerText: 'Concept Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, width: 100
                },
                {
                    field: 'BookingNo', headerText: 'Booking No', width: 100
                },
                {
                    field: 'BookingDate', headerText: 'Booking Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, width: 100
                },
                {
                    field: 'Buyer', headerText: 'Buyer', width: 200
                },
                {
                    field: 'ConceptForName', headerText: 'Concept For', width: 100
                },
                {
                    field: 'TrialNo', headerText: 'Trial No', width: 100
                },
                {
                    field: 'ItemSubGroup', headerText: 'Item Sub Group', width: 100
                },
                {
                    field: 'KnittingType', headerText: 'Knitting Type'
                },
                {
                    field: 'TechnicalName', headerText: 'Technical Name'
                },
                {
                    field: 'Composition', headerText: 'Composition'
                },
                {
                    field: 'GSM', headerText: 'GSM'
                },
                {
                    field: 'Qty', headerText: 'Qty (KG)'
                }
            ];
        }

        else if (status == statusConstants.APPROVED) {
            columns = [
                {
                    headerText: '', textAlign: 'Center', commands: commands,
                    textAlign: 'Center', width: ch_setActionCommandCellWidth(commands)
                },
                {
                    field: 'YarnPRFromID', visible: false
                },
                {
                    field: 'Source', headerText: 'Source', width: 100
                },
                {
                    field: 'YarnPRNo', headerText: 'PR No', width: 100
                },
                {
                    field: 'YarnPRDate', headerText: 'PR Date', textAlign: 'Center', type: 'date', format: _ch_date_format_5, width: 160
                },
                {
                    field: 'ConceptNo', headerText: 'Concept No', width: 100
                },
                {
                    field: 'BookingNo', headerText: 'Booking No', width: 100
                },
                {
                    field: 'ApproveDate', headerText: 'Approve Date', textAlign: 'Center', type: 'date', format: _ch_date_format_5, width: 160
                },
                {
                    field: 'TriggerPoint', headerText: 'Trigger Point', width: 150
                },
                {
                    field: 'YarnPRRequiredDate', headerText: 'Required Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, width: 100
                },
                {
                    field: 'YarnPRByUser', headerText: 'Requisition By', width: 100
                },
                {
                    field: 'CreateBy', headerText: 'Create By', width: 100
                },
                {
                    field: 'YpApproveBy', headerText: 'Approved By', width: 100
                },
                {
                    field: 'Buyer', headerText: 'Buyer Name', width: 100
                },
                {
                    field: 'RevisionNo', headerText: 'Revision No', width: 80, textAlign: 'Center'
                },
                {
                    field: 'Remarks', headerText: 'Remarks'
                }
            ];
        }

        else if (status == statusConstants.REJECT) {
            columns = [
                {
                    headerText: '', textAlign: 'Center', commands: commands,
                    textAlign: 'Center', width: ch_setActionCommandCellWidth(commands)
                },
                {
                    field: 'YarnPRFromID', visible: false
                },
                {
                    field: 'Source', headerText: 'Source', width: 100
                },
                {
                    field: 'YarnPRNo', headerText: 'PR No', width: 100
                },
                {
                    field: 'YarnPRDate', headerText: 'PR Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, width: 100
                },
                {
                    field: 'ConceptNo', headerText: 'Concept No', width: 100
                },
                {
                    field: 'BookingNo', headerText: 'Booking No', width: 100
                },
                {
                    field: 'RejectDate', headerText: 'Reject Date', type: 'date', format: _ch_date_format_1
                },
                {
                    field: 'TriggerPoint', headerText: 'Trigger Point', width: 150
                },
                {
                    field: 'YarnPRRequiredDate', headerText: 'Required Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, width: 100
                },
                {
                    field: 'YarnPRByUser', headerText: 'Requisition By', width: 100
                },
                {
                    field: 'CreateBy', headerText: 'Create By', width: 100
                },
                {
                    field: 'YpRejectBy', headerText: 'Rejected By', width: 100
                },
                {
                    field: 'Buyer', headerText: 'Buyer Name', width: 100
                },
                {
                    field: 'RevisionNo', headerText: 'Revision No', width: 80, textAlign: 'Center'
                },
                {
                    field: 'Remarks', headerText: 'Remarks'
                }
            ];
        }

        else {
            columns = [
                {
                    headerText: '', textAlign: 'Center', commands: commands,
                    textAlign: 'Center', width: ch_setActionCommandCellWidth(commands)
                },
                {
                    field: 'YarnPRFromID', visible: false
                },
                {
                    field: 'Source', headerText: 'Source', width: 100
                },
                {
                    field: 'YarnPRNo', headerText: 'PR No', width: 100
                },
                {
                    field: 'YarnPRDate', headerText: 'PR Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, width: 100
                },
                {
                    field: 'ConceptNo', headerText: 'Concept No', width: 100
                },
                {
                    field: 'BookingNo', headerText: 'Booking No', width: 100
                },
                {
                    field: 'TriggerPoint', headerText: 'Trigger Point', width: 150
                },
                {
                    field: 'YarnPRRequiredDate', headerText: 'Required Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, width: 100
                },
                {
                    field: 'YarnPRByUser', headerText: 'Requisition By', width: 100
                },
                {
                    field: 'CreateBy', headerText: 'Create By', width: 100
                },
                {
                    field: 'Buyer', headerText: 'Buyer Name', width: 100
                },

                {
                    field: 'RevisionNo', headerText: 'Revision No', width: 80, textAlign: 'Center'
                },
                {
                    field: 'Remarks', headerText: 'Remarks'
                }
            ];
        }

        if (isFPRPage && status == statusConstants.ALL_STATUS) {
            var indexNo = columns.length - 1;
            columns.splice(indexNo, 0, {
                field: 'PRStatus', headerText: 'Status', width: 150
            });
            $formEl.find("#divStatus").show();
        } else {
            $formEl.find("#divStatus").hide();
        }

        var selectionType = "Single";
        if (status == statusConstants.AWAITING_PROPOSE) {
            columns.unshift({ type: 'checkbox', width: 50 });
            //selectionType = "Multiple";
        }
        var pageName = isCPRPage ? pageNameConstants.CPR : isFPRPage ? pageNameConstants.FPR : "";
        pageName = replaceInvalidChar(pageName);

        if ($tblMasterEl) $tblMasterEl.destroy();

        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: false,
            apiEndPoint: `/api/yarn-pr/list?status=${status}&pageName=${pageName}`,
            columns: columns,
            commandClick: handleCommands,
            allowSelection: status == statusConstants.AWAITING_PROPOSE,
            selectionSettings: { type: selectionType, checkboxOnly: true, persistSelection: true }
        });
    }

    function handleCommands(args) {
        $formEl.find("#btnAcknowledgeMR").hide();
        if (args.commandColumn.type == 'Edit') {
            getDetails(args.rowData.YarnPRMasterID, args.rowData.YarnPRFromID, args.rowData.Source, args.commandColumn.type);
            $formEl.find("#btnApproveYPR,#btnRejectYPR,#btnAkgYPR,#btnUnAkgYPR").fadeOut();

            if (status == statusConstants.PROPOSED) {
                $formEl.find("#btnSave,#btnSaveForApproval,#btnAcknowledge,#btnNewItem").fadeOut();
            } else if (status == statusConstants.ALL_STATUS) {
                $formEl.find("#btnSave,#btnSaveForApproval,#btnAcknowledge,#btnNewItem").fadeOut();
            } else {
                $formEl.find("#btnSave,#btnSaveForApproval,#btnNewItem").fadeIn();
            }
            if (status == statusConstants.COMPLETED) {
                $formEl.find("#btnSave,#btnSaveForApproval,#btnAcknowledge").fadeOut();
            }

            //if (pageName == "YarnFinancePR" && status == statusConstants.PENDING) {
            //isFPRPage
            if (isFPRPage && status == statusConstants.PENDING) {
                $formEl.find("#btnAcknowledge").fadeIn();
                $formEl.find("#btnSave,#btnSaveForApproval").fadeOut();
            }
            //if (pageName == "YarnCommercialPR") {
            if (isCPRPage) {
                $formEl.find("#btnAcknowledge").fadeOut();
            }
            addAdditionalReq = false;
        }

        else if (args.commandColumn.type == 'Revise') {
            var groupConceptNo = getGroupConceptNo(args.rowData.ConceptNo);
            getDetailsForRevise(args.rowData.YarnPRMasterID, args.rowData.YarnPRFromID, args.rowData.Source, groupConceptNo);
            $formEl.find("#btnApproveYPR,#btnRejectYPR,#btnAkgYPR,#btnUnAkgYPR").fadeOut();

            if (status == statusConstants.PROPOSED) {
                $formEl.find("#btnSave,#btnSaveForApproval,#btnAcknowledge,#btnNewItem").fadeOut();
            } else if (status == statusConstants.ALL_STATUS) {
                $formEl.find("#btnSave,#btnSaveForApproval,#btnAcknowledge,#btnNewItem").fadeOut();
            } else {
                $formEl.find("#btnSave,#btnSaveForApproval,#btnNewItem").fadeIn();
            }
            addAdditionalReq = false;
        }

        else if (args.commandColumn.type == 'View') {
            getDetails(args.rowData.YarnPRMasterID, args.rowData.YarnPRFromID, args.rowData.Source, args.commandColumn.type);
            $formEl.find("#btnSave,#btnSaveForApproval,#btnAcknowledge,#btnNewItem,#btnApproveYPR,#btnRejectYPR,#btnAkgYPR,#btnUnAkgYPR").fadeOut();
            if (status == statusConstants.PROPOSED) {
                $formEl.find("#btnApproveYPR,#btnRejectYPR").fadeIn();
            } else if (status == statusConstants.PARTIALLY_COMPLETED) {
                $formEl.find("#btnAkgYPR,#btnUnAkgYPR").fadeIn();
            }
            prStatus = args.rowData.PRStatus;
            addAdditionalReq = false;
        }
        else if (args.commandColumn.type == 'Addition') {
            addAdditionalReq = true;
            getDetails(args.rowData.YarnPRMasterID, args.rowData.YarnPRFromID, args.rowData.Source, args.commandColumn.type);
            $formEl.find("#btnSave,#btnSaveForApproval,#btnAcknowledge,#btnNewItem,#btnApproveYPR,#btnRejectYPR,#btnAkgYPR,#btnUnAkgYPR").fadeOut();
            $formEl.find("#btnSave").fadeIn();
            $formEl.find("#btnSaveForApproval").fadeIn();


            $formEl.find("#YarnPRRequiredDate").removeAttr('readonly');

            //if (status == statusConstants.PROPOSED) {
            //    $formEl.find("#btnApproveYPR,#btnRejectYPR").fadeIn();
            //} else if (status == statusConstants.PARTIALLY_COMPLETED) {
            //    $formEl.find("#btnAkgYPR,#btnUnAkgYPR").fadeIn();
            //}
            prStatus = args.rowData.PRStatus;
        }

        else if (args.commandColumn.type == 'Approve') {
            addAdditionalReq = false;
            approvePR(args.rowData.YarnPRMasterID, args.rowData.YarnPRFromID, args.rowData.Source);
        }

        else if (args.commandColumn.type == 'Reject') {
            addAdditionalReq = false;
            showBootboxPrompt("Reject Yarn PR", "Enter your reject reason:", function (result) {
                if (result) {
                    rejectPR(args.rowData.YarnPRMasterID, result, args.rowData.Source);
                }
            });
        }

        else if (args.commandColumn.type == 'Acknowledge') {
            addAdditionalReq = false;
            acknowledgePR(args.rowData.YarnPRMasterID);
        }

        else if (args.commandColumn.type == 'UnAcknowledge') {
            addAdditionalReq = false;
            showBootboxPrompt("Reject Yarn PR", "Enter your unacknowledge reason:", function (result) {
                if (result) {
                    unacknowledgePR(args.rowData.YarnPRMasterID, result);
                }
            });
        }

        else if (args.commandColumn.type == 'ViewMR') {
            addAdditionalReq = false;
            FCMRMasterID = args.rowData.FCMRMasterID;
            Source = args.rowData.Source;
            RevisionStatus = args.rowData.RevisionStatus;
            //axios.get(`/api/yarn-pr/new-mr/${args.rowData.FCMRMasterID}`)
            axios.get(`/api/yarn-pr/new-mr?iDs=${FCMRMasterID}&source=${Source}&revisionstatus=${RevisionStatus}`)
                .then(function (response) {

               
                    isEditable = true;
                    status = statusConstants.ADDITIONAL;
                    $divDetailsEl.fadeIn();
                    $divTblEl.fadeOut();
                    $formEl.find("#divRejectReason").hide();
                    $formEl.find("#divUnAcknowledgeReason").hide();

                    if (status == statusConstants.ADDITIONAL) $formEl.find("#btnSave,#btnSaveForApproval,#btnAcknowledge").fadeOut();
                    else $formEl.find("#btnSave,#btnSaveForApproval").fadeIn();
                    $formEl.find("#btnAcknowledgeMR").show();
                    masterData = response.data;
                    if (masterData.YarnPRRequiredDate != null) {
                        masterData.YarnPRRequiredDate = formatDateToDefault(masterData.YarnPRRequiredDate);
                    }
                    if (masterData.YarnPRDate != null) {
                        masterData.YarnPRDate = formatDateToDefault(masterData.YarnPRDate);
                    }


                    showHideControls();
                    $formEl.find("#ConceptNo").show();
                    setFormData($formEl, masterData);

                    $formEl.find("#TriggerPointID").val('1252').trigger("change"); //Projection 
                    //$formEl.find("#YarnPRBy").val(masterData.YarnPRBy).trigger("change");
                    $formEl.find("#IsRNDPR").prop('checked', true);
                
                    initChildTable(masterData.Childs);
                })
                .catch(showResponseError);
        } else if (args.commandColumn.type == 'AcknowledgeMR') {
            addAdditionalReq = false;
            acknowledgeMR(null, args.rowData.FCMRMasterID, args.rowData.ConceptNo);
        }

        else if (args.commandColumn.type == 'RejectMR') {
            addAdditionalReq = false;
            showBootboxPrompt("Reject", "Enter your reject reason:", function (result) {
                if (result) {
                    rejectMR(args.rowData.FCMRMasterID, result);
                }
            });
        }

        else if (args.commandColumn.type == "Report") {
            window.open(`/reports/InlinePdfView?ReportName=YarnPurchaseRequisition.rdl&YarnPRNo=${args.rowData.YarnPRNo}`, '_blank');
        }
        else if (args.commandColumn.type == 'Email') {
            showBootboxConfirm("Send Mail.", "Are you sure to re-send mail?", function (yes) {
                if (yes) {
                    sendMail(args.rowData.YarnPRMasterID);
                }
            });
            //checkRevisionStatusView(args.rowData.BookingNo, args.rowData.ExportOrderNo, args.rowData.SubGroupID, args.rowData.WithoutOB)
            //getView(args.rowData.BookingNo, args.rowData.WithoutOB);
            //$formEl.find("#btnReceive,#btnReceived,#btnCancelAcknowledge,#btnCancelUnAcknowledge").fadeOut();
            //$formEl.find("#btnSave,#btnUnAcknowledge,#btnOkk").fadeIn();
            //if (status == statusConstants.REJECT) {
            //    $formEl.find("#btnCancelAcknowledge,#btnCancelUnAcknowledge").fadeIn();
            //    $formEl.find("#btnSave,#btnUnAcknowledge,#btnOkk").fadeOut();
            //}
        }
        //if (args.commandColumn.type == 'Report') {
        //    var a = document.createElement('a');
        //    a.href = "/reports/InlinePdfView?ReportName=YarnPurchaseRequisition.Rdl" + args.rowData.YarnPRNo;
        //    a.setAttribute('target', '_blank');
        //    a.click();
        //}
    }
    function sendMail(prMasterID) {
        if (status == statusConstants.APPROVED && isApprovePage && prMasterID > 0) {
            var url = "/api/yarn-pr/smail/" + prMasterID;
            axios.get(url)
                .then(function (response) {
                    if (response.data) {
                        toastr.success('Mail has been sent');
                    }
                    else {
                        toastr.error('Mail not sent properly!!!');
                    }
                })
                .catch(function (err) {
                    toastr.error(err.response.data.Message);
                });
        }
    }

    function backToList() {
        if (status === statusConstants.NEW) {
            status = statusConstants.ADDITIONAL;
            toggleActiveToolbarBtn($toolbarEl.find("#btnAcknowledgedMaterialReqList"), $toolbarEl);
        }
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        //initMasterTable();
        //$tblMasterEl.refresh();
    }
    function reloadMasterTable() {
        if (status === statusConstants.NEW) {
            status = statusConstants.ADDITIONAL;
            toggleActiveToolbarBtn($toolbarEl.find("#btnAcknowledgedMaterialReqList"), $toolbarEl);
        }
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        initMasterTable();
        //$tblMasterEl.refresh();
    }
    function reloadMasterTable() {
        if (status === statusConstants.NEW) {
            status = statusConstants.ADDITIONAL;
            toggleActiveToolbarBtn($toolbarEl.find("#btnAcknowledgedMaterialReqList"), $toolbarEl);
        }
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        initMasterTable();
        //$tblMasterEl.refresh();
    }
    function resetForm() {
        filterBy = {};
        $formEl.trigger("reset");
        $.each($formEl.find('select'), function (i, el) {
            $(el).select2('');
        });
        $formEl.find("#YarnPRMasterID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function getNewData() {
        axios.get(`/api/yarn-pr/new`)
            .then(function (response) {
                isEditable = true;
                status = statusConstants.NEW;
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                $formEl.find("#divRejectReason").hide();
                $formEl.find("#divUnAcknowledgeReason").hide();
                $formEl.find("#btnSave,#btnSaveForApproval").fadeIn();
                source = "";
                $formEl.find("#YarnPRFromID").val(0);
                masterData = response.data;
                if (masterData.YarnPRRequiredDate != null) {
                    masterData.YarnPRRequiredDate = formatDateToDefault(masterData.YarnPRRequiredDate);
                }
                if (masterData.YarnPRDate != null) {
                    masterData.YarnPRDate = formatDateToDefault(masterData.YarnPRDate);
                }

                $formEl.find("#TriggerPointID").prop("disabled", false);
                //$formEl.find("#YarnPRBy").prop("disabled", false);
                $formEl.find("#YarnPRRequiredDate").prop("disabled", false);
                $formEl.find("#IsRNDPR").prop("disabled", false);
                $formEl.find("#Remarks").prop("disabled", false);
                $formEl.find("#divRejectReason").hide();
                $formEl.find("#divUnAcknowledgeReason").hide();
                setFormData($formEl, masterData);
                initChildTable([]);
            })
            .catch(showResponseError);
    }

    function getNewDataPRForMR() {
        addAdditionalReq = false;
        if ($tblMasterEl.getSelectedRecords().length == 0) {
            toastr.error("Please select row!");
            return;
        }
        var selectedRecords = $tblMasterEl.getSelectedRecords();
        var conceptList = selectedRecords.map(x => x.ConceptNo);

        var unique_array = [...new Set(conceptList)];
        if (unique_array.length > 1) {
            toastr.error("Concept number should same.");
            return;
        }

        //if ($tblMasterEl.getSelectedRecords().length > 1) {
        //    toastr.error("You can not select more then one row!");
        //    return;
        //}
        var uniqueAry = distinctArrayByProperty($tblMasterEl.getSelectedRecords(), "Source");
        if (uniqueAry.length != 1) {
            toastr.error("Selected row(s) source should be same!");
            return;
        } else {
            var uniqueAry1 = distinctArrayByProperty($tblMasterEl.getSelectedRecords(), "RevisionStatus");
            if (uniqueAry1.length != 1) {
                toastr.error("Selected row(s) Status should be same!");
                return;
            }
        }

        source = uniqueAry[0].Source;
        var iDs = "";
        if (uniqueAry[0].Source == prFrom.CONCEPT) {
            iDs = $tblMasterEl.getSelectedRecords().map(function (el) { return "'" + el.ConceptNo + "'" }).toString();
            yarnPRFromID = 1;
        }
        else if (uniqueAry[0].Source == prFrom.BDS) {
            iDs = $tblMasterEl.getSelectedRecords().map(function (el) { return "'" + el.ConceptNo + "'" }).toString();
            yarnPRFromID = 2;
        }
        else if (uniqueAry[0].Source == prFrom.BULK_BOOKING) {
            //iDs = $tblMasterEl.getSelectedRecords().map(function (el) { return el.BookingID }).toString();
            iDs = $tblMasterEl.getSelectedRecords().map(function (el) { return "'" + el.ConceptNo + "'" }).toString();
            yarnPRFromID = 3;
        }
        else if (uniqueAry[0].Source == prFrom.PROJECTION_YARN_BOOKING) {
            iDs = $tblMasterEl.getSelectedRecords().map(function (el) { return el.BookingID }).toString();
            yarnPRFromID = 4;
        }
        else if (uniqueAry[0].Source == prFrom.FABRIC_PROJECTION_YARN_BOOKING) {
            iDs = $tblMasterEl.getSelectedRecords().map(function (el) { return "'" + el.ConceptNo + "'" }).toString();
            yarnPRFromID = 5;
        }
        axios.get(`/api/yarn-pr/new-mr?iDs=${iDs}&source=${uniqueAry[0].Source}&revisionstatus=${uniqueAry[0].RevisionStatus}`)
            .then(function (response) {
                isEditable = true;
                //console.log(response);
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                $formEl.find("#divRejectReason").hide();
                $formEl.find("#divUnAcknowledgeReason").hide();
                $formEl.find("#btnAcknowledgeMR").hide();
                $formEl.find("#btnAcknowledge").hide();

                if (status == statusConstants.ADDITIONAL) $formEl.find("#btnSave,#btnSaveForApproval").fadeOut();
                else $formEl.find("#btnSave,#btnSaveForApproval").fadeIn();
                masterData = response.data;
                if (masterData.YarnPRRequiredDate != null) {
                    masterData.YarnPRRequiredDate = formatDateToDefault(masterData.YarnPRRequiredDate);
                }

                if (masterData.YarnPRDate != null) {
                    masterData.YarnPRDate = formatDateToDefault(masterData.YarnPRDate);
                }

                showHideControls();
                masterData.YarnPRNo = "<<--New-->>";
                masterData.YarnPRDate = formatDateToDefault(new Date());
                //addAdditionalReq = true
                setFormData($formEl, masterData);

                if (uniqueAry[0].Source == "Bulk Booking") {
                    $formEl.find("#TriggerPointID").val('1251').trigger("change"); //Projection Based
                } else {
                    $formEl.find("#TriggerPointID").val('1252').trigger("change"); //Projection Based
                }

                $formEl.find("#IsRNDPR").prop('checked', true);
                $formEl.find("#YarnPRFromID").val(yarnPRFromID);
                $formEl.find("#YarnPRByName").val(masterData.YarnPRByName);
                
                initChildTable(masterData.Childs);
            })
            .catch(showResponseError);
    }

    function getDetails(id, prFromID, source, actionType) {
        var isNewForPRAck = false;
        if (isFPRPage) {
            if (status == statusConstants.PENDING) {
                isNewForPRAck = false;
            } else if (status == statusConstants.COMPLETED) {
                isNewForPRAck = true;
            }
        }
        var url = `/api/yarn-pr/${id}/${prFromID}/${source}/${isNewForPRAck}`;
        axios.get(url)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();

                masterData = response.data;
                if (masterData.YarnPRRequiredDate != null) {
                    masterData.YarnPRRequiredDate = formatDateToDefault(masterData.YarnPRRequiredDate);
                }

                if (masterData.YarnPRDate != null) {
                    masterData.YarnPRDate = formatDateToDefault(masterData.YarnPRDate);
                }

                $formEl.find("#PRStatus").hide();
                if (status == statusConstants.ALL_STATUS) {
                    $formEl.find("#PRStatus").show();
                    masterData.PRStatus = prStatus;
                }

                masterData.Childs.forEach(function (value) {
                    value.CompanyIDs = value.CompanyIDs.map(function (el) { return el.toString() })
                })

                showHideControls();

                //if (masterData.TriggerPointID == 1252 && masterData.IsRNDPR == true) {
                //    $formEl.find("#TriggerPointID").prop("disabled", true);
                //    $formEl.find("#IsRNDPR").prop("disabled", true);
                //}

                //if (masterData.YarnPRFromID == 1)
                //    source = prFrom.CONCEPT;
                //else if (masterData.YarnPRFromID == 2)
                //    source = prFrom.BDS;
                //else if (masterData.YarnPRFromID == 3)
                //    source = prFrom.BULK_BOOKING;
                //else
                //    source = prFrom.PROJECTION_YARN_BOOKING;

                if (addAdditionalReq) {
                    masterData.Childs.forEach(function (value) {
                        value.ReqQty = 0;
                        value.ReqCone = 0;
                        return value;
                    });
                    $formEl.find("#YarnPRNo").val("<<--New-->>");
                    //masterData.YarnPRNo = "<<--New-->>";
                    masterData.YarnPRDate = formatDateToDefault(new Date());
                }

                //if (masterData.YarnPRFromID == 4) {
                //    masterData.Childs.forEach(function (value) {
                //        value.ReqQty = 0;
                //        value.ReqCone = 0;
                //        return value;
                //    })
                //}

                setFormData($formEl, masterData);

                if (actionType == "Addition") {
                    $formEl.find("#YarnPRRequiredDate").removeAttr('readonly');
                    $formEl.find("#YarnPRRequiredDate").removeAttr('disabled');
                } else {
                    $formEl.find("#YarnPRRequiredDate").attr('readonly', 'readonly');
                    $formEl.find("#YarnPRRequiredDate").attr('disabled', 'disabled');
                }

                initChildTable(masterData.Childs);
            })
            .catch(showResponseError);
    }

    function getDetailsForRevise(id, prFromID, source, groupConceptNo) {
        var url = `/api/yarn-pr/revise/${id}/${prFromID}/${source}/${groupConceptNo}`;
        axios.get(url)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();

                $formEl.find("#btnAcknowledge").hide();
                masterData = response.data;
                if (masterData.YarnPRRequiredDate != null) {
                    masterData.YarnPRRequiredDate = formatDateToDefault(masterData.YarnPRRequiredDate);
                }
                if (masterData.YarnPRDate != null) {
                    masterData.YarnPRDate = formatDateToDefault(masterData.YarnPRDate);
                }

                $formEl.find("#PRStatus").hide();
                if (status == statusConstants.ALL_STATUS) {
                    $formEl.find("#PRStatus").show();
                    masterData.PRStatus = prStatus;
                }

                masterData.Childs.forEach(function (value) {
                    value.CompanyIDs = value.CompanyIDs.map(function (el) { return el.toString() })
                })

                showHideControls();

                setFormData($formEl, masterData);
                //$formEl.find("#ConceptNo").val(masterData.ConceptNo);
                $formEl.find("#GroupConceptNo").val(masterData.GroupConceptNo);
                initChildTable(masterData.Childs);
            })
            .catch(showResponseError);
    }

    function save(isSendForApproval) {
        var data = formElToJson($formEl);
        data.Status = status;
        data.Childs = $tblChildEl.getCurrentViewRecords();

        if (data.Childs.length === 0) return toastr.error("At least 1 Yarn items is required.");

        var dayDurations = [];

        data.Childs.map(x => {
            if (x.RefSpinnerID == null) x.RefSpinnerID = 0;
            var baseTypeId = 0;

            if (typeof source === "undefined") source = "";
            if (source.toLowerCase() == "bds" || source.toLowerCase() == "concept") baseTypeId = 2164;
            if (source.toLowerCase() == "fabric projection yarn booking") baseTypeId = 2161;

            x.BaseTypeId = baseTypeId;

            x.DayDuration = getDefaultValueWhenInvalidN(x.DayDuration);
            dayDurations.push(x.DayDuration);
        });
        if (!isCPRPage && !isFPRPage) {
            var minDay = Math.max(...dayDurations);
            if (minDay > 0) {
                var currentDayValidDurationId = data.Childs.find(x => x.DayDuration == minDay).DayValidDurationId;
                var isDateValidObj = ch_IsDateValid_DayValidDuration(data.YarnPRRequiredDate, currentDayValidDurationId, masterData.DayValidDurations);
                if (!isDateValidObj.IsValid && masterData.IsCheckDVD) {
                    toastr.error(`Minimum required date for this sourcing mode is ${ch_customDateFormat(isDateValidObj.MinDate)}`);
                    return false;
                }
            }
        }

        //Validation
        if (isValidChildForm(data)) return;

        data.IsRNDPR = convertToBoolean(data.IsRNDPR);
        data.IsCPR = isCPRPage;
        data.IsFPR = isFPRPage;
        data.SendForApproval = isSendForApproval;
        data.SendForCPRApproval = isSendForApproval;


        if (addAdditionalReq) {
            data.YarnPRMasterID = 0;
            data.IsAdditional = true;
            data.YarnPRNo = masterData.YarnPRNo;
        }
        var url = isCPRPage ? "/api/yarn-pr/save-cpr" : (isFPRPage ? "/api/yarn-pr/save-fpr" : "/api/yarn-pr/save");
        axios.post(url, data)
            .then(function () {
                toastr.success("Saved successfully.");
                //backToList();
                reloadMasterTable();
            })
            .catch(showResponseError);
    }

    function approvePR(id, YarnPRFromID, source) {
        var url = `/api/yarn-pr/approve/${id}/${YarnPRFromID}/${source}`;
        axios.post(url)
            .then(function () {
                toastr.success(constants.APPROVE_SUCCESSFULLY);
                //backToList();
                reloadMasterTable();
            })
            .catch(showResponseError);
    }

    function rejectPR(id, reason, source) {
        var url = `/api/yarn-pr/reject/${id}/${reason}/${source}`;
        axios.post(url)
            .then(function () {
                toastr.success(constants.UNAPPROVE_SUCCESSFULLY);
                //backToList();
                reloadMasterTable();
            })
            .catch(showResponseError);
    }
    //check
    async function initChildTable(data) {
        if ($tblChildEl) $tblChildEl.destroy();
      
        var columns = [{ field: 'YarnPRChildID', isPrimaryKey: true, visible: false }];
        if (status == statusConstants.AWAITING_PROPOSE || status == statusConstants.ADDITIONAL || status == statusConstants.APPROVED) {
            columns.push.apply(columns,
                [
                    {
                        headerText: 'Commands', commands: [
                            { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                            { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                            { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                            { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }
                        ]
                    }
                ]
            );
        }
        columns.push.apply(columns, [
            { field: 'GroupConceptNo', headerText: 'Group Concept No', allowEditing: false, visible: false },
            { field: 'ConceptNo', headerText: 'Concept No', allowEditing: false, visible: source == prFrom.CONCEPT },
            {
                field: 'BookingNo', headerText: 'Booking No', allowEditing: false,
                visible: source == prFrom.PROJECTION_YARN_BOOKING ||
                    source == prFrom.BULK_BOOKING
            }
        ]);
        columns.push.apply(columns, getYarnItemColumnsForDisplayOnly());
        columns.push.apply(columns, [
            { field: 'ShadeCode', headerText: 'Shade Code', allowEditing: false },
            { field: 'DayValidDurationName', headerText: 'Yarn Sourcing Mode', allowEditing: false },
            { field: 'FPRCompanyName', headerText: 'Company', visible: !isCPRPage, allowEditing: false },
            {
                field: 'FPRCompanyID', headerText: 'Company', visible: isCPRPage, allowEditing: isCPRPage, valueAccessor: ej2GridDisplayFormatter, dataSource: masterData.CompanyList,
                displayField: "FPRCompanyName", edit: ej2GridDropDownObj({
                })
            },
            { field: 'YarnCategory', headerText: 'Yarn Category', allowEditing: false, width: 350 },

        ]);

        var refSpinner = {
            field: 'RefSpinnerID', headerText: 'Ref Spinner', allowEditing: false
            , valueAccessor: ej2GridDisplayFormatter, dataSource: masterData.RefSpinnerList, displayField: "RefSpinner", edit: ej2GridDropDownObj({

            })
        }
        if (isFPRPage) {
            refSpinner = { field: 'RefSpinner', headerText: 'Ref Spinner', width: 40, allowEditing: false };
        }

        var additionalColumns = [
            { field: 'RefLotNo', headerText: 'Ref Lot No', allowEditing: true },
            //{
            //    field: 'RefSpinnerID', headerText: 'Ref Spinner', allowEditing: false
            //    , valueAccessor: ej2GridDisplayFormatter, dataSource: masterData.RefSpinnerList, displayField: "RefSpinner", edit: ej2GridDropDownObj({

            //    })
            //},
            { field: 'Source', headerText: 'Source', width: 20, visible: false },
            {
                field: 'ReqQty', headerText: 'Req Qty(Kg)', editType: "numericedit", allowEditing: status == statusConstants.AWAITING_PROPOSE || status == statusConstants.EDIT || status == statusConstants.REVISE || addAdditionalReq, //addAdditionalReq || status == statusConstants.REVISE,
                textAlign: 'Right', edit: { params: { showSpinButton: false, decimals: 2, min: 1, format: "N2" } }
            },
            {
                field: 'ReqCone', headerText: 'Req Cone(Pcs)', editType: "numericedit", allowEditing: status == statusConstants.AWAITING_PROPOSE || status == statusConstants.EDIT || status == statusConstants.REVISE || addAdditionalReq, //addAdditionalReq || status == statusConstants.REVISE,
                textAlign: 'Right', edit: { params: { showSpinButton: false, decimals: 0, min: 0, format: "N2" } }
            },
            {
                field: 'PurchaseQty', headerText: 'Plan Purchase Qty (Kg)', editType: "numericedit", visible: isFPRPage,
                textAlign: 'Right', edit: { params: { showSpinButton: false, decimals: 2, min: 0, format: "N2" } }
            },
            {
                field: 'AllocationQty', headerText: 'Plan Allocation Qty(Kg)', editType: "numericedit", allowEditing: false, visible: isFPRPage,
                textAlign: 'Right', edit: { params: { showSpinButton: false, decimals: 2, min: 0, format: "N2" } }
            },
            { field: 'Remarks', headerText: 'Remarks', width: 20, allowEditing: !isFPRPage }
        ];
        columns.push.apply(columns, additionalColumns);

        var indexF = columns.findIndex(x => x.field == "RefLotNo");
        columns.splice(indexF, 0, refSpinner);

        data.map(x => {
            x = setYarnSegDesc(x);
        });

        var tableOptions = {
            tableId: tblChildId,
            data: data,
            columns: columns,            
            editSettings: {
                allowAdding: false,
                allowEditing: true,
                allowDeleting: false,
                mode: "Normal",
                showDeleteConfirmDialog: true
            },
            actionBegin: function (args) {
                if (args.requestType === "add") {
                    args.data.YarnPRChildID = getMaxIdForArray($tblChildEl.getCurrentViewRecords(), "YarnPRChildID");
                }
                else if (args.requestType === "save") {
                    args.data.PurchaseQty = parseFloat(parseFloat(args.data.PurchaseQty).toFixed(2));
                    args.data.AllocationQty = parseFloat(parseFloat(args.data.AllocationQty).toFixed(2));
                    args.data.ReqQty = parseFloat(parseFloat(args.data.ReqQty).toFixed(2));

                    if (args.data.PurchaseQty > args.data.ReqQty) {
                        toastr.error(`Maximum Req Qty is ${args.data.ReqQty}`);
                        args.data.PurchaseQty = args.data.ReqQty;
                        args.data.AllocationQty = 0;
                        return false;
                    }
                    args.data.AllocationQty = args.data.ReqQty - args.data.PurchaseQty;
                    args.data.AllocationQty = args.data.AllocationQty.toFixed(2);
                    args.rowData.AllocationQty = args.data.AllocationQty;
                }
            },
            commandClick: childCommandClick,
            autofitColumns: true,
            showDefaultToolbar: false,
            allowFiltering: false,
            allowPaging: false
        };

        if (isEditable) {
            if (status == statusConstants.NEW || status == statusConstants.PROPOSED || status == statusConstants.APPROVED) {  // || status == statusConstants.AWAITING_PROPOSE
                tableOptions["toolbar"] = ['Add'];
            }
            tableOptions["editSettings"] = { allowAdding: true, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true };
        }
        if (status == statusConstants.APPROVED) {
            tableOptions["toolbar"] = ['Add'];
            tableOptions["editSettings"] = { allowAdding: true, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true };
        }
        //else if ((isCPRPage && status === statusConstants.PENDING) || (isFPRPage && status === statusConstants.PENDING)) {
        //    tableOptions["editSettings"] = { allowEditing: true };
        //}
        $tblChildEl = new initEJ2Grid(tableOptions);
    }

    function childCommandClick(args) {
        if (args.commandColumn.buttonOption.type == 'companySearch') {
            var childData = args.rowData;
            axios.get(`/api/yarn-pr/commercial-company/${childData.YarnPRChildID}`)
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
    function getGroupConceptNo(conceptNo) {
        var groupCNo = "-";
        if (conceptNo) {
            groupCNo = conceptNo.split(',')[0];
            groupCNo = groupCNo.split('_')[0];
        }
        return groupCNo;
    }
    function showHideControls() {
        if (masterData.Reject) {
            $formEl.find("#divRejectReason").show();
        } else {
            $formEl.find("#divRejectReason").hide();
        }

        if (masterData.UnAcknowlege) {
            $formEl.find("#divUnAcknowledgeReason").show();
        } else {
            $formEl.find("#divUnAcknowledgeReason").hide();
        }

        if (status == statusConstants.REVISE) {
            $formEl.find("#btnSave").hide();
        }

        if (isEditable) {
            $formEl.find("#TriggerPointID").prop("disabled", (status == statusConstants.AWAITING_PROPOSE || status == statusConstants.ADDITIONAL));
            //$formEl.find("#YarnPRBy").prop("disabled", false);
            $formEl.find("#YarnPRRequiredDate").prop("disabled", false);
            $formEl.find("#IsRNDPR").prop("disabled", (status == statusConstants.AWAITING_PROPOSE || status == statusConstants.ADDITIONAL));
            $formEl.find("#Remarks").prop("disabled", false);
        }
        else {
            $formEl.find("#TriggerPointID").prop("disabled", true);
            //$formEl.find("#YarnPRBy").prop("disabled", true);
            $formEl.find("#YarnPRRequiredDate").prop("disabled", true);
            $formEl.find("#IsRNDPR").prop("disabled", true);
            $formEl.find("#Remarks").prop("disabled", true);
        }
    }

    function acknowledgeMR(e, mrMasterID, conceptNo) {
        if (e) e.preventDefault();
        if (!mrMasterID) mrMasterID = FCMRMasterID;

        if (!conceptNo) conceptNo = $formEl.find("#ConceptNo").val();
        showBootboxConfirm("Acknowledge MR", `Are you sure want to ackowledge this?`, function (yes) {
            if (yes) {
                var url = `/api/rnd-free-concept-mr/acknowledge/${mrMasterID}`;
                axios.post(url)
                    .then(function () {
                        toastr.success(constants.ACKNOWLEDGE_SUCCESSFULLY);
                        backToList();
                    })
                    .catch(showResponseError);
            }
        })
    }

    function rejectMR(id, reason) {
        var url = `/api/rnd-free-concept-mr/reject/${id}/${reason}`;
        axios.post(url)
            .then(function () {
                toastr.success(constants.UNAPPROVE_SUCCESSFULLY);
                backToList();
            })
            .catch(showResponseError);
    }

    async function getYarnSegments() {
        console.log("i am getYarnSegments");
        var response = await axios.get(getYarnItemsApiUrl([]));
        console.log(getYarnItemsApiUrl([]));
        console.log(response);
        _yarnSegments = response.data;
    }

    function isValidChildForm(data) {
        var isValidItemInfo = false;

        for (var k = 0; k < data.Childs.length; k++) {
            var child = data.Childs[k];
            if (child.ReqQty == "" || child.ReqQty == null || child.ReqQty <= 0) {
                toastr.error("Req. Qty. is required.");
                isValidItemInfo = true;
                break;
            }
            if (masterData.YarnPRFromID != 3) //3 means bulk booking
            {
                if (!isCPRPage) {
                    if (child.ReqCone == "" || child.ReqCone == null || child.ReqCone <= 0) {
                        toastr.error("Req. Cone is required.");
                        isValidItemInfo = true;
                        break;
                    }
                }
            }
            if (isFPRPage) {
                if ((child.PurchaseQty == "" || child.PurchaseQty == null || child.PurchaseQty <= 0) && (child.AllocationQty == "" || child.AllocationQty == null || child.AllocationQty <= 0)) {
                    toastr.error("Purchase Qty or Allocation Qty is required.");
                    isValidItemInfo = true;
                    break;
                }
            }
        }
        return isValidItemInfo;
    }
    function setYarnSegDesc(obj) {

      
        //console.log(_yarnSegments);
        for (var indexSeg = 1; indexSeg <= 6; indexSeg++) {
            var segIdProp = "Segment" + indexSeg + "ValueId";
            var segDescProp = "Segment" + indexSeg + "ValueDesc";
            var listName = "Segment" + indexSeg + "ValueList";

            if (obj[segIdProp] > 0 && (typeof obj[segDescProp] !== "undefined" || obj[segDescProp] != "")) {
                var objSeg = _yarnSegments[listName].find(s => s.id == obj[segIdProp]);
                if (objSeg) {
                    obj[segDescProp] = objSeg.text;
                }
            }
        }
        return obj;
    }
})();